using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase549 independently adjudicates the Phase548 bounded pilot. It does not
// reference the Phase545 kernel and does not reuse any Phase548 code: the
// sampler, the estimators, and the checkpoint reader are re-implemented here
// from the frozen contracts alone, so that an implementation defect in
// Phase548 cannot hide inside its own verdict.

const string Root = "studies/phase549_bounded_pilot_independent_result_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase549_independent_result_adjudicator_contract_v1.json";
const string OutputPath = Root + "/output/bounded_pilot_independent_result_adjudicator.json";
const string SummaryPath = Root + "/output/bounded_pilot_independent_result_adjudicator_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;

var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new
{
    Id = x.GetProperty("id").GetString()!,
    Path = x.GetProperty("path").GetString()!,
    ExpectedSha256 = x.GetProperty("sha256").GetString()!,
}).ToArray();
var bindings = bindingSpecs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { x.Id, x.Path, x.ExpectedSha256, ActualSha256 = actual, HashMatches = actual == x.ExpectedSha256 };
}).ToArray();
bool exactBindingsValid = bindings.All(x => x.HashMatches) && bindings.Length == 16;

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase549-a29-bounded-pilot-independent-result-adjudicator-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("independentOfPhase548Implementation").GetBoolean()
    && contract.GetProperty("mayNotReferencePhase545Kernel").GetBoolean()
    && exactBindingsValid
    && taxonomy.Length == 7
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// The audited phase's own frozen contract supplies every executed parameter.
using var p548ContractDocument = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs.First(x => x.Id == "phase548-contract").Path));
JsonElement p548Contract = p548ContractDocument.RootElement;
using var p548Document = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs.First(x => x.Id == "phase548-summary").Path));
JsonElement p548 = p548Document.RootElement;

JsonElement target = p548Contract.GetProperty("target");
JsonElement defaults = p548Contract.GetProperty("defaultConfiguration");
int extent = target.GetProperty("extent").GetInt32();
double stepSize = defaults.GetProperty("stepSize").GetDouble();
int leapfrogSteps = defaults.GetProperty("leapfrogSteps").GetInt32();
int warmupPerChain = defaults.GetProperty("warmupPerChain").GetInt32();
int retainedPerChain = defaults.GetProperty("retainedPerChain").GetInt32();
int trajectoriesPerChain = defaults.GetProperty("trajectoriesPerChain").GetInt32();
double divergenceThreshold = defaults.GetProperty("divergenceAbsoluteDeltaH").GetDouble();
JsonElement p548Thresholds = p548Contract.GetProperty("telemetrySchema").GetProperty("convergenceThresholds");
double maximumRhat = p548Thresholds.GetProperty("maximumSplitRankNormalizedRhat").GetDouble();
double minimumBulkEss = p548Thresholds.GetProperty("minimumBulkEss").GetDouble();
double minimumTailEss = p548Thresholds.GetProperty("minimumTailEss").GetDouble();
double minimumAcceptance = defaults.GetProperty("minimumAcceptanceRate").GetDouble();
var chainPlan = p548Contract.GetProperty("seedTables").EnumerateArray().SelectMany(t =>
{
    string tableId = t.GetProperty("id").GetString()!;
    int offset = t.GetProperty("seedOffset").GetInt32();
    int[] seeds = t.GetProperty("seeds").EnumerateArray().Select(y => y.GetInt32()).ToArray();
    double[] scales = t.GetProperty("initialScales").EnumerateArray().Select(y => y.GetDouble()).ToArray();
    return seeds.Select((seed, i) => new { TableId = tableId, RawSeed = seed, ExecutionSeed = seed + offset, Scale = scales[i] });
}).ToArray();

// ------------------------------------- independent estimator known-answer battery
JsonElement batterySpec = contract.GetProperty("estimatorKnownAnswerBattery");
int syntheticSeed = batterySpec.GetProperty("syntheticSeed").GetInt32();
int syntheticChains = batterySpec.GetProperty("chains").GetInt32();
int syntheticDraws = batterySpec.GetProperty("drawsPerChain").GetInt32();
var batteryRng = new Xoshiro(ExpandSeed((ulong)syntheticSeed));
var batteryRows = new List<object>();
bool batteryPassed = true;
foreach (JsonElement testCase in batterySpec.GetProperty("cases").EnumerateArray())
{
    string id = testCase.GetProperty("id").GetString()!;
    string kind = testCase.GetProperty("kind").GetString()!;
    var series = new double[syntheticChains][];
    for (int c = 0; c < syntheticChains; c++)
    {
        var values = new double[syntheticDraws];
        double previous = 0.0;
        for (int i = 0; i < syntheticDraws; i++)
        {
            double z = Gauss(batteryRng);
            if (kind == "ar1")
            {
                double rho = testCase.GetProperty("rho").GetDouble();
                previous = rho * previous + System.Math.Sqrt(1 - rho * rho) * z;
                values[i] = previous;
            }
            else if (kind == "iid-with-offset")
            {
                values[i] = z + c * testCase.GetProperty("offset").GetDouble();
            }
            else
            {
                values[i] = z;
            }
        }
        series[c] = values;
    }
    Diagnostics d = Diagnose(series);
    double total = syntheticChains * syntheticDraws;
    bool passed = true;
    if (testCase.TryGetProperty("expectRhatBelow", out var rhatBelow)) passed &= d.Rhat < rhatBelow.GetDouble();
    if (testCase.TryGetProperty("expectRhatAbove", out var rhatAbove)) passed &= d.Rhat > rhatAbove.GetDouble();
    if (testCase.TryGetProperty("expectBulkEssFractionAbove", out var bulkAbove)) passed &= d.BulkEss / total > bulkAbove.GetDouble();
    if (testCase.TryGetProperty("expectBulkEssFractionBelow", out var bulkBelow)) passed &= d.BulkEss / total < bulkBelow.GetDouble();
    if (testCase.TryGetProperty("expectTailEssFractionAbove", out var tailAbove)) passed &= d.TailEss / total > tailAbove.GetDouble();
    batteryPassed &= passed;
    batteryRows.Add(new
    {
        id, kind, rhat = Reportable(d.Rhat), bulkEss = Reportable(d.BulkEss), tailEss = Reportable(d.TailEss),
        bulkEssFraction = Reportable(d.BulkEss / total), tailEssFraction = Reportable(d.TailEss / total), passed,
    });
}

// --------------------------------------------------------- telemetry integrity
JsonElement telemetrySpec = contract.GetProperty("telemetryAudit");
string[] requiredFields = telemetrySpec.GetProperty("requiredTrajectoryFields").EnumerateArray().Select(x => x.GetString()!).ToArray();
double hamiltonianTolerance = telemetrySpec.GetProperty("hamiltonianAbsoluteTolerance").GetDouble();
var telemetryRows = new List<object>();
var telemetryByChain = new Dictionary<string, TelemetryRow[]>();
bool telemetryValid = true;
foreach (var plan in chainPlan)
{
    string chainId = $"{plan.TableId}-{plan.RawSeed}";
    string path = $"studies/phase548_bounded_complete_lattice_pilot_execution_001/output/telemetry/{chainId}_trajectories.json";
    using var document = JsonDocument.Parse(File.ReadAllBytes(path));
    JsonElement rowsElement = document.RootElement.GetProperty("rows");
    var rows = rowsElement.EnumerateArray().Select(r => new TelemetryRow(
        r.GetProperty("chainId").GetString()!, r.GetProperty("trajectoryIndex").GetInt32(),
        r.GetProperty("phase").GetString()!, r.GetProperty("accepted").GetBoolean(),
        r.GetProperty("initialHamiltonian").GetDouble(), r.GetProperty("proposedHamiltonian").GetDouble(),
        r.GetProperty("deltaH").GetDouble(), r.GetProperty("injectedThreshold").GetDouble(),
        r.GetProperty("nonFinite").GetBoolean(), r.GetProperty("divergent").GetBoolean())).ToArray();
    telemetryByChain[chainId] = rows;
    bool fieldsPresent = rowsElement.EnumerateArray().All(r => requiredFields.All(f => r.TryGetProperty(f, out _)));
    bool countsOk = rows.Length == telemetrySpec.GetProperty("expectedRowsPerChain").GetInt32()
        && rows.Count(r => r.Phase == "warmup") == telemetrySpec.GetProperty("expectedWarmupRows").GetInt32()
        && rows.Count(r => r.Phase == "retained") == telemetrySpec.GetProperty("expectedRetainedRows").GetInt32()
        && rows.Select((r, i) => r.TrajectoryIndex == i).All(x => x)
        && rows.All(r => r.ChainId == chainId);
    bool deltaConsistent = rows.All(r => System.Math.Abs(r.DeltaH - (r.ProposedHamiltonian - r.InitialHamiltonian)) <= hamiltonianTolerance);
    bool acceptanceRuleHolds = rows.All(r =>
        r.Accepted == (!r.NonFinite && !r.Divergent && r.InjectedThreshold <= System.Math.Min(0.0, -r.DeltaH)));
    bool divergenceLabelHolds = rows.All(r => r.Divergent == (r.NonFinite || System.Math.Abs(r.DeltaH) > divergenceThreshold));
    int nonFinite = rows.Count(r => r.NonFinite);
    int divergent = rows.Count(r => r.Divergent);
    bool cleanOk = nonFinite == 0 && divergent == 0;
    bool ok = fieldsPresent && countsOk && deltaConsistent && acceptanceRuleHolds && divergenceLabelHolds && cleanOk;
    telemetryValid &= ok;
    telemetryRows.Add(new
    {
        chainId, rowCount = rows.Length, fieldsPresent, countsOk, deltaConsistent,
        acceptanceRuleHolds, divergenceLabelHolds, nonFinite, divergent,
        independentAcceptanceRate = (double)rows.Count(r => r.Accepted) / rows.Length, passed = ok,
    });
}

// ---------------------------------------------- independent replay of the chains
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
var member = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2,
    Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5,
    EpsilonMode = "independent-theta",
};
var op = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: extent);
var massMatrix = new CpuMassMatrix(mesh, algebra);
double[] thetaZero = new double[mesh.VertexCount * algebra.Dimension];
int dof = mesh.EdgeCount * algebra.Dimension;

JsonElement replaySpec = contract.GetProperty("independentReplay");
double deltaTolerance = replaySpec.GetProperty("deltaHRelativeTolerance").GetDouble();
string[] observableNames = ["actionDensity", "forceNormSquared", "configurationNormSquared"];
var replayRows = new List<object>();
var replayDraws = new Dictionary<string, Dictionary<string, double[]>>();
var replayFinalPositions = new Dictionary<string, double[]>();
bool replayMatches = true;
foreach (var plan in chainPlan)
{
    string chainId = $"{plan.TableId}-{plan.RawSeed}";
    var rng = new Xoshiro(ExpandSeed((ulong)plan.ExecutionSeed));
    var position = new double[dof];
    for (int i = 0; i < dof; i++) position[i] = plan.Scale * Gauss(rng);
    (double action, double[] gradient) current = Evaluate(position);
    var draws = observableNames.ToDictionary(n => n, _ => new List<double>());
    double worstDelta = 0.0;
    int acceptedCount = 0;
    bool decisionsMatch = true;
    TelemetryRow[] recorded = telemetryByChain[chainId];
    for (int t = 0; t < trajectoriesPerChain; t++)
    {
        var momentum = new double[dof];
        for (int i = 0; i < dof; i++) momentum[i] = Gauss(rng);
        double logUniform = System.Math.Log(Uniform(rng));
        double initialHamiltonian = current.action + 0.5 * Dot(momentum, momentum);
        var q = (double[])position.Clone();
        var p = (double[])momentum.Clone();
        double action = current.action;
        double[] gradient = current.gradient;
        bool finite = true;
        for (int i = 0; i < dof; i++) p[i] -= 0.5 * stepSize * gradient[i];
        for (int leap = 0; leap < leapfrogSteps; leap++)
        {
            for (int i = 0; i < dof; i++) q[i] += stepSize * p[i];
            (action, gradient) = Evaluate(q);
            if (!double.IsFinite(action) || !gradient.All(double.IsFinite) || !q.All(double.IsFinite)) { finite = false; break; }
            double kick = leap + 1 == leapfrogSteps ? 0.5 * stepSize : stepSize;
            for (int i = 0; i < dof; i++) p[i] -= kick * gradient[i];
        }
        finite &= p.All(double.IsFinite);
        double finalHamiltonian = finite ? action + 0.5 * Dot(p, p) : double.NaN;
        double delta = finalHamiltonian - initialHamiltonian;
        bool divergent = !finite || !double.IsFinite(delta) || System.Math.Abs(delta) > divergenceThreshold;
        bool accept = finite && !divergent && logUniform <= System.Math.Min(0.0, -delta);
        if (accept) { position = q; current = (action, gradient); acceptedCount++; }

        TelemetryRow row = recorded[t];
        double scale = System.Math.Max(1.0, System.Math.Abs(row.DeltaH));
        worstDelta = System.Math.Max(worstDelta, System.Math.Abs(delta - row.DeltaH) / scale);
        decisionsMatch &= accept == row.Accepted;

        if (t >= warmupPerChain)
        {
            draws["actionDensity"].Add(current.action / dof);
            draws["forceNormSquared"].Add(Dot(current.gradient, current.gradient));
            draws["configurationNormSquared"].Add(Dot(position, position));
        }
    }
    bool ok = decisionsMatch && worstDelta <= deltaTolerance
        && draws[observableNames[0]].Count == retainedPerChain;
    replayMatches &= ok;
    replayDraws[chainId] = draws.ToDictionary(x => x.Key, x => x.Value.ToArray());
    replayFinalPositions[chainId] = position;
    replayRows.Add(new
    {
        chainId, worstRelativeDeltaHDeviation = Reportable(worstDelta), decisionsMatch,
        independentAcceptanceRate = (double)acceptedCount / trajectoriesPerChain,
        retainedDraws = draws[observableNames[0]].Count, passed = ok,
    });
}

// ------------------------------------------------------------ checkpoint audit
var checkpointRows = new List<object>();
bool checkpointsValid = true;
foreach (var plan in chainPlan)
{
    string chainId = $"{plan.TableId}-{plan.RawSeed}";
    string path = $"studies/phase548_bounded_complete_lattice_pilot_execution_001/output/checkpoints/{chainId}.json";
    using var document = JsonDocument.Parse(File.ReadAllBytes(path));
    JsonElement root = document.RootElement;
    JsonElement payload = root.GetProperty("payload");
    byte[] payloadBytes = Encoding.UTF8.GetBytes(payload.GetRawText());
    string recomputed = Convert.ToHexString(SHA256.HashData(payloadBytes)).ToLowerInvariant();
    bool checksumOk = root.GetProperty("checksumAlgorithm").GetString() == "SHA-256"
        && recomputed == root.GetProperty("payloadSha256").GetString();
    double[] storedPosition = payload.GetProperty("position").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    bool positionMatchesReplay = storedPosition.Length == dof
        && storedPosition.Zip(replayFinalPositions[chainId]).All(pair =>
            BitConverter.DoubleToInt64Bits(pair.First) == BitConverter.DoubleToInt64Bits(pair.Second));
    bool headerOk = payload.GetProperty("chainId").GetString() == chainId
        && payload.GetProperty("seed").GetInt32() == plan.ExecutionSeed
        && payload.GetProperty("degreesOfFreedom").GetInt32() == dof
        && payload.GetProperty("thetaRule").GetString() == "theta-identically-zero"
        && payload.GetProperty("rngAlgorithm").GetString() == "xoshiro256-starstar"
        && payload.GetProperty("stepSize").GetDouble() == stepSize
        && payload.GetProperty("leapfrogSteps").GetInt32() == leapfrogSteps;
    bool ok = checksumOk && positionMatchesReplay && headerOk;
    checkpointsValid &= ok;
    checkpointRows.Add(new { chainId, checksumOk, headerOk, positionMatchesReplay, passed = ok });
}

// ----------------------------------- independent diagnostics and comparison
var reportedTables = p548.GetProperty("diagnostics").GetProperty("tables").EnumerateArray().ToArray();
JsonElement comparisonSpec = contract.GetProperty("reportedDiagnosticsComparison");
double rhatTolerance = comparisonSpec.GetProperty("rhatAbsoluteTolerance").GetDouble();
double essTolerance = comparisonSpec.GetProperty("essRelativeTolerance").GetDouble();
var comparisonRows = new List<object>();
bool diagnosticsReproduced = true;
bool independentDiagnosticsValid = true;
foreach (var tableGroup in chainPlan.GroupBy(x => x.TableId))
{
    JsonElement reported = reportedTables.First(t => t.GetProperty("tableId").GetString() == tableGroup.Key);
    double independentMinimumAcceptance = tableGroup.Min(plan =>
    {
        TelemetryRow[] rows = telemetryByChain[$"{plan.TableId}-{plan.RawSeed}"];
        return (double)rows.Count(r => r.Accepted) / rows.Length;
    });
    bool acceptanceOk = independentMinimumAcceptance >= minimumAcceptance;
    foreach (string name in observableNames)
    {
        Diagnostics d = Diagnose(tableGroup.Select(plan => replayDraws[$"{plan.TableId}-{plan.RawSeed}"][name]).ToArray());
        JsonElement reportedRow = reported.GetProperty("observables").EnumerateArray()
            .First(o => o.GetProperty("name").GetString() == name);
        double reportedRhat = reportedRow.GetProperty("splitRankNormalizedRhat").GetDouble();
        double reportedBulk = reportedRow.GetProperty("bulkEss").GetDouble();
        double reportedTail = reportedRow.GetProperty("tailEss").GetDouble();
        bool reportedPassed = reportedRow.GetProperty("passed").GetBoolean();
        bool rhatOk = double.IsFinite(d.Rhat) && d.Rhat <= maximumRhat;
        bool bulkOk = double.IsFinite(d.BulkEss) && d.BulkEss >= minimumBulkEss;
        bool tailOk = double.IsFinite(d.TailEss) && d.TailEss >= minimumTailEss;
        bool independentPassed = rhatOk && bulkOk && tailOk;
        bool agrees = System.Math.Abs(d.Rhat - reportedRhat) <= rhatTolerance
            && RelativeClose(d.BulkEss, reportedBulk, essTolerance)
            && RelativeClose(d.TailEss, reportedTail, essTolerance)
            && independentPassed == reportedPassed;
        diagnosticsReproduced &= agrees;
        independentDiagnosticsValid &= independentPassed;
        comparisonRows.Add(new
        {
            table = tableGroup.Key, name,
            independentRhat = Reportable(d.Rhat), reportedRhat = Reportable(reportedRhat),
            independentBulkEss = Reportable(d.BulkEss), reportedBulkEss = Reportable(reportedBulk),
            independentTailEss = Reportable(d.TailEss), reportedTailEss = Reportable(reportedTail),
            independentPassed, reportedPassed, agrees,
        });
    }
    independentDiagnosticsValid &= acceptanceOk;
    comparisonRows.Add(new
    {
        table = tableGroup.Key, name = "acceptanceFloor",
        independentRhat = (double?)null, reportedRhat = (double?)null,
        independentBulkEss = (double?)null, reportedBulkEss = (double?)null,
        independentTailEss = (double?)null, reportedTailEss = (double?)null,
        independentPassed = acceptanceOk,
        reportedPassed = reported.GetProperty("acceptancePassed").GetBoolean(),
        agrees = acceptanceOk == reported.GetProperty("acceptancePassed").GetBoolean(),
    });
    diagnosticsReproduced &= acceptanceOk == reported.GetProperty("acceptancePassed").GetBoolean();
}

string reportedTerminal = p548.GetProperty("verdictKind").GetString()!;
string independentTerminal = independentDiagnosticsValid
    ? "pilot-executed-diagnostics-valid"
    : "pilot-executed-diagnostics-invalid";
bool terminalReproduced = independentTerminal == reportedTerminal;

string verdict = !contractValid ? taxonomy[0]
    : !batteryPassed ? taxonomy[1]
    : !telemetryValid ? taxonomy[2]
    : !replayMatches ? taxonomy[3]
    : !checkpointsValid ? taxonomy[4]
    : !(diagnosticsReproduced && terminalReproduced) ? taxonomy[5]
    : taxonomy[6];

var output = new
{
    schemaVersion = 1,
    phase = 549,
    phaseId = "phase549-bounded-pilot-independent-result-adjudicator",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    bindings,
    independence = new
    {
        reusesPhase548Code = false,
        referencesPhase545Kernel = false,
        samplerReimplementedFromContract = true,
        estimatorsReimplemented = true,
        checkpointReaderReimplemented = true,
    },
    estimatorKnownAnswerBattery = new
    {
        motivation = "Phase548's tail effective-sample-size estimator was defective; these estimators are validated against known answers before use.",
        syntheticSeed, chains = syntheticChains, drawsPerChain = syntheticDraws,
        cases = batteryRows, passed = batteryPassed,
    },
    telemetryIntegrity = new { chains = telemetryRows, passed = telemetryValid },
    independentReplay = new
    {
        chains = replayRows,
        passed = replayMatches,
        perDrawObservableSeriesWasRetainedByPhase548 = false,
        replayWasRequiredBecauseObservableSeriesWasNotRetained = true,
    },
    checkpointAudit = new { chains = checkpointRows, passed = checkpointsValid },
    diagnosticsComparison = new
    {
        thresholds = new { maximumRhat, minimumBulkEss, minimumTailEss, minimumAcceptance },
        rows = comparisonRows,
        diagnosticsReproduced,
        independentTerminal,
        reportedTerminal,
        terminalReproduced,
    },
    verdictKind = verdict,
    terminalStatus = "bounded-pilot-independent-result-adjudicator-" + verdict,
    decision = verdict == taxonomy[6]
        ? "An independent re-implementation of the sampler, the estimators, and the checkpoint reader reproduces the Phase548 telemetry, checkpoints, convergence numbers, per-observable gate outcomes, and terminal. The confirmed terminal remains a negative convergence result, and confirming it grants no authority."
        : "The independent adjudication did not reach agreement with the reported Phase548 result; the earliest frozen failure is preserved.",
    scope = new
    {
        confirmsReportedTerminalOnly = true,
        reinterpretsPhase548 = false,
        establishesStationarityOfRegisteredTarget = false,
        establishesSamplingCorrectness = false,
        establishesTransferToLargerExtent = false,
        workbenchRelativeLatticeUnitsOnly = true,
    },
    phase535ExecutedReopenedOrMutated = false,
    phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    o4Discharged = false,
    sourceContractApplicationAllowed = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    productionAuthorized = false,
    launchAuthorized = false,
    externalReviewPending = true,
    allDownstreamAuthority = false,
    promotedPhysicalMassClaimCount = 0,
};

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
byte[] outputBytes = JsonSerializer.SerializeToUtf8Bytes(output,
    new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
File.WriteAllBytes(OutputPath, outputBytes);
File.WriteAllBytes(SummaryPath, outputBytes);
Console.WriteLine($"Phase549 verdict: {verdict}");
Console.WriteLine($"battery={batteryPassed}, telemetry={telemetryValid}, replay={replayMatches}, checkpoints={checkpointsValid}");
Console.WriteLine($"diagnosticsReproduced={diagnosticsReproduced}, independentTerminal={independentTerminal}, reported={reportedTerminal}");

(double Action, double[] Gradient) Evaluate(double[] omega)
{
    var gradient = op.ComputeJointGradient(omega, thetaZero, massMatrix);
    return (gradient.Objective, gradient.GradOmega);
}

static bool RelativeClose(double a, double b, double tolerance)
{
    if (!double.IsFinite(a) || !double.IsFinite(b)) return false;
    return System.Math.Abs(a - b) <= tolerance * System.Math.Max(1.0, System.Math.Abs(b));
}
static double? Reportable(double value) => double.IsFinite(value) ? value : null;

static Diagnostics Diagnose(double[][] chains)
{
    double[][] usable = chains.Where(x => x.Length >= 8).ToArray();
    if (usable.Length == 0) return new Diagnostics(double.NaN, double.NaN, double.NaN);
    int n = usable.Min(x => x.Length);
    double[][] trimmed = usable.Select(x => x.Take(n).ToArray()).ToArray();
    double[] pooled = trimmed.SelectMany(x => x).ToArray();
    if (pooled.Distinct().Count() <= 1) return new Diagnostics(double.NaN, double.NaN, double.NaN);
    double[] ranked = RankNormalize(pooled);
    double median = Median(pooled);
    double[] folded = RankNormalize(pooled.Select(x => System.Math.Abs(x - median)).ToArray());
    double[][] rankedChains = Regroup(ranked, trimmed.Length, n);
    double[][] foldedChains = Regroup(folded, trimmed.Length, n);
    return new Diagnostics(
        System.Math.Max(SplitRhat(rankedChains), SplitRhat(foldedChains)),
        Ess(Split(rankedChains)), Ess(Split(foldedChains)));
}
static double[][] Regroup(double[] flat, int chains, int n) =>
    Enumerable.Range(0, chains).Select(c => flat.Skip(c * n).Take(n).ToArray()).ToArray();
static double[][] Split(double[][] chains) =>
    chains.SelectMany(x => new[] { x.Take(x.Length / 2).ToArray(), x.Skip(x.Length - x.Length / 2).ToArray() }).ToArray();
static double SplitRhat(double[][] chains)
{
    double[][] split = Split(chains);
    int m = split.Length;
    int n = split.Min(x => x.Length);
    if (m < 2 || n < 2) return double.NaN;
    double[] means = split.Select(x => x.Take(n).Average()).ToArray();
    double within = split.Select(x =>
    {
        double mean = x.Take(n).Average();
        return x.Take(n).Sum(v => (v - mean) * (v - mean)) / (n - 1);
    }).Average();
    if (within <= 0) return double.NaN;
    double grand = means.Average();
    double between = n * means.Sum(x => (x - grand) * (x - grand)) / (m - 1);
    return System.Math.Sqrt((((n - 1.0) / n) * within + between / n) / within);
}
static double Ess(double[][] chains)
{
    int m = chains.Length;
    int n = chains.Min(x => x.Length);
    if (m < 2 || n < 4) return double.NaN;
    double[][] trimmed = chains.Select(x => x.Take(n).ToArray()).ToArray();
    double[] means = trimmed.Select(x => x.Average()).ToArray();
    double within = trimmed.Select(x =>
    {
        double mean = x.Average();
        return x.Sum(v => (v - mean) * (v - mean)) / (n - 1);
    }).Average();
    if (within <= 0) return double.NaN;
    double grand = means.Average();
    double between = n * means.Sum(x => (x - grand) * (x - grand)) / (m - 1);
    double varPlus = ((n - 1.0) / n) * within + between / n;
    if (varPlus <= 0) return double.NaN;
    var rho = new double[n];
    rho[0] = 1.0;
    for (int lag = 1; lag < n; lag++)
    {
        double covariance = 0.0;
        for (int c = 0; c < m; c++)
        {
            double mean = means[c];
            double sum = 0.0;
            for (int i = 0; i + lag < n; i++) sum += (trimmed[c][i] - mean) * (trimmed[c][i + lag] - mean);
            covariance += sum / n;
        }
        rho[lag] = 1.0 - (within - covariance / m) / varPlus;
    }
    double tau = -1.0;
    double previousPair = double.PositiveInfinity;
    for (int k = 0; 2 * k + 1 < n; k++)
    {
        double pair = rho[2 * k] + rho[2 * k + 1];
        if (pair < 0) break;
        pair = System.Math.Min(pair, previousPair);
        previousPair = pair;
        tau += 2.0 * pair;
    }
    return tau > 0 ? m * n / tau : double.NaN;
}
static double[] RankNormalize(double[] values)
{
    int n = values.Length;
    int[] order = Enumerable.Range(0, n).OrderBy(i => values[i]).ToArray();
    var ranks = new double[n];
    int index = 0;
    while (index < n)
    {
        int run = index;
        while (run + 1 < n && values[order[run + 1]] == values[order[index]]) run++;
        double average = (index + run) / 2.0 + 1.0;
        for (int k = index; k <= run; k++) ranks[order[k]] = average;
        index = run + 1;
    }
    return ranks.Select(r => InverseNormalCdf((r - 0.375) / (n + 0.25))).ToArray();
}
static double Median(double[] values)
{
    double[] sorted = values.OrderBy(x => x).ToArray();
    int n = sorted.Length;
    return n % 2 == 1 ? sorted[n / 2] : 0.5 * (sorted[n / 2 - 1] + sorted[n / 2]);
}
static double InverseNormalCdf(double p)
{
    double[] a = [-3.969683028665376e+01, 2.209460984245205e+02, -2.759285104469687e+02, 1.383577518672690e+02, -3.066479806614716e+01, 2.506628277459239e+00];
    double[] b = [-5.447609879822406e+01, 1.615858368580409e+02, -1.556989798598866e+02, 6.680131188771972e+01, -1.328068155288572e+01];
    double[] c = [-7.784894002430293e-03, -3.223964580411365e-01, -2.400758277161838e+00, -2.549732539343734e+00, 4.374664141464968e+00, 2.938163982698783e+00];
    double[] d = [7.784695709041462e-03, 3.224671290700398e-01, 2.445134137142996e+00, 3.754408661907416e+00];
    const double low = 0.02425;
    if (p <= 0 || p >= 1) return double.NaN;
    if (p < low)
    {
        double q = System.Math.Sqrt(-2 * System.Math.Log(p));
        return (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) / ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
    }
    if (p > 1.0 - low)
    {
        double q = System.Math.Sqrt(-2 * System.Math.Log(1 - p));
        return -(((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) / ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
    }
    double r = p - 0.5;
    double s = r * r;
    return (((((a[0] * s + a[1]) * s + a[2]) * s + a[3]) * s + a[4]) * s + a[5]) * r
        / (((((b[0] * s + b[1]) * s + b[2]) * s + b[3]) * s + b[4]) * s + 1);
}
static double Dot(double[] a, double[] b)
{
    double sum = 0.0;
    for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
    return sum;
}
static double Uniform(Xoshiro rng) => ((rng.Next() >> 11) + 0.5) / 9007199254740992.0;
static double Gauss(Xoshiro rng)
{
    double u1 = Uniform(rng), u2 = Uniform(rng);
    return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
}
static ulong[] ExpandSeed(ulong seed)
{
    ulong state = seed;
    ulong Next()
    {
        state += 0x9E3779B97F4A7C15UL;
        ulong z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
    return [Next(), Next(), Next(), Next()];
}
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

sealed record Diagnostics(double Rhat, double BulkEss, double TailEss);
sealed record TelemetryRow(string ChainId, int TrajectoryIndex, string Phase, bool Accepted,
    double InitialHamiltonian, double ProposedHamiltonian, double DeltaH, double InjectedThreshold,
    bool NonFinite, bool Divergent);
sealed class Xoshiro(ulong[] state)
{
    private ulong _s0 = state[0], _s1 = state[1], _s2 = state[2], _s3 = state[3];
    public ulong Next()
    {
        ulong result = RotateLeft(_s1 * 5, 7) * 9;
        ulong t = _s1 << 17;
        _s2 ^= _s0; _s3 ^= _s1; _s1 ^= _s2; _s0 ^= _s3; _s2 ^= t; _s3 = RotateLeft(_s3, 45);
        return result;
    }
    private static ulong RotateLeft(ulong x, int k) => (x << k) | (x >> (64 - k));
}
