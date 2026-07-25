using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase552 re-analyses the six already-determined Phase548 chains. It draws no new
// sample: it re-executes six committed chains under the identical frozen
// configuration and seeds and computes a DIFFERENT function of an
// already-determined dataset. Every statistic and threshold below is frozen in the
// contract before the replay runs, and the artifact carries
// analysisIsRetrospectiveOnKnownData so that no reader mistakes this for a
// convergence assessment of the pilot.

const string Root = "studies/phase552_committed_chain_stationarity_reanalysis_001";
const string ContractPath = Root + "/preregistration/phase552_committed_chain_stationarity_reanalysis_contract_v1.json";
const string OutputPath = Root + "/output/committed_chain_stationarity_reanalysis.json";
const string SummaryPath = Root + "/output/committed_chain_stationarity_reanalysis_summary.json";

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
bool exactBindingsValid = bindings.Length == 17 && bindings.All(x => x.HashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
JsonElement driftSpec = contract.GetProperty("driftTest");
JsonElement resource = contract.GetProperty("resourceRule");

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase552-a30-committed-chain-stationarity-reanalysis-v1"
    && contract.GetProperty("planSection").GetString() == "COMPLETE_LATTICE_FLAT_SECTOR_PLAN_2026-07-25 A30"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("analysisIsRetrospectiveOnKnownData").GetBoolean()
    && contract.GetProperty("statisticsAndThresholdsFrozenBeforeReplay").GetBoolean()
    && contract.GetProperty("isDiagnosticOnly").GetBoolean()
    && contract.GetProperty("mayChangePhase548Terminal").GetBoolean() == false
    && contract.GetProperty("isAConvergenceAssessmentOfThePilot").GetBoolean() == false
    && contract.GetProperty("newSamplingPerformed").GetBoolean() == false
    && exactBindingsValid
    && taxonomy.Length == 6
    && taxonomy[0] == "invalid-or-drifted-input"
    && taxonomy[5] == "stationary-under-resolved-consistent"
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// -------------------------------------------- frozen pilot configuration, reused
using var p548ContractDocument = JsonDocument.Parse(File.ReadAllBytes(
    bindingSpecs.First(x => x.Id == "phase548-contract").Path));
JsonElement p548Contract = p548ContractDocument.RootElement;
JsonElement target = p548Contract.GetProperty("target");
JsonElement defaults = p548Contract.GetProperty("defaultConfiguration");
int extent = target.GetProperty("extent").GetInt32();
double stepSize = defaults.GetProperty("stepSize").GetDouble();
int leapfrogSteps = defaults.GetProperty("leapfrogSteps").GetInt32();
int warmupPerChain = defaults.GetProperty("warmupPerChain").GetInt32();
int retainedPerChain = defaults.GetProperty("retainedPerChain").GetInt32();
int trajectoriesPerChain = defaults.GetProperty("trajectoriesPerChain").GetInt32();
double divergenceThreshold = defaults.GetProperty("divergenceAbsoluteDeltaH").GetDouble();
var chainPlan = p548Contract.GetProperty("seedTables").EnumerateArray().SelectMany(table =>
{
    string tableId = table.GetProperty("id").GetString()!;
    int offset = table.GetProperty("seedOffset").GetInt32();
    int[] seeds = table.GetProperty("seeds").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    double[] scales = table.GetProperty("initialScales").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    return seeds.Select((seed, i) => new { TableId = tableId, RawSeed = seed, ExecutionSeed = seed + offset, Scale = scales[i] });
}).ToArray();

// The Phase546 resource rule is forwarded unchanged; no ceiling is modified and no
// new work is requested beyond re-executing the already-committed trajectories.
JsonElement p548Resource = p548Contract.GetProperty("resourceRule");
long maximumAggregateCpuTicks = p548Resource.GetProperty("maximumAggregateCpuTicks").GetInt64();
long ticksPerForceDegreeOfFreedom = p548Resource.GetProperty("cpuTicksPerForceDegreeOfFreedom").GetInt64();
long degreesOfFreedom = 45L * extent * extent * extent * extent;
long requestedTicks = (long)chainPlan.Length * trajectoriesPerChain * leapfrogSteps
    * ticksPerForceDegreeOfFreedom * degreesOfFreedom;
long perTableTicks = requestedTicks / 2;
bool resourceAccepted = perTableTicks < maximumAggregateCpuTicks
    && resource.GetProperty("ceilingModified").GetBoolean() == false
    && resource.GetProperty("newSamplingRequested").GetBoolean() == false;

if (!contractValid || !resourceAccepted)
{
    string earlyVerdict = !contractValid ? taxonomy[0] : taxonomy[1];
    WriteResult(new
    {
        schemaVersion = 1, phase = 552, phaseId = "phase552-committed-chain-stationarity-reanalysis",
        contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath),
        contractValid, exactBindingsValid, resourceAccepted, bindings,
        verdictKind = earlyVerdict,
        terminalStatus = "committed-chain-stationarity-reanalysis-" + earlyVerdict,
        decision = "The phase refused before replaying the committed chains.",
        analysisIsRetrospectiveOnKnownData = true, newSamplingPerformed = false,
        phase548TerminalChanged = false, nullSpaceInterpretedAsGaugeVolume = false, quotientApplied = false,
        phase535ExecutedReopenedOrMutated = false, phase481PackCreatedOrMutated = false,
        productionDefaultSelected = false, phase458G3Satisfied = false, phase458G4Satisfied = false,
        phase458G5Satisfied = false, o4Discharged = false, sourceContractApplicationAllowed = false,
        physicalUnitClaimAllowed = false, gevClaimAllowed = false, productionAuthorized = false,
        launchAuthorized = false, externalReviewPending = true, allDownstreamAuthority = false,
        promotedPhysicalMassClaimCount = 0,
    });
    Console.WriteLine($"Phase552 verdict: {earlyVerdict}");
    return;
}

// ------------------------------------------------------------ registered target
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
int dimG = algebra.Dimension;
int edgeCount = mesh.EdgeCount, faceCount = mesh.FaceCount;
int dof = edgeCount * dimG;
var thetaZero = new double[mesh.VertexCount * dimG];
(double Action, double[] Gradient) Evaluate(double[] omega)
{
    var joint = op.ComputeJointGradient(omega, thetaZero, massMatrix);
    return (joint.Objective, joint.GradOmega);
}

// ------------------------------------------- flat basis, rebuilt from the mesh
int[][] flatGenerators =
[
    .. Enumerable.Range(0, mesh.VertexCount).Select(v =>
    {
        var g = new int[edgeCount];
        for (int e = 0; e < edgeCount; e++)
            g[e] = (mesh.Edges[e][1] == v ? 1 : 0) - (mesh.Edges[e][0] == v ? 1 : 0);
        return g;
    }),
    .. Enumerable.Range(0, 4).Select(axis =>
    {
        var g = new int[edgeCount];
        for (int e = 0; e < edgeCount; e++)
        {
            var c0 = mesh.GetVertexCoordinates(mesh.Edges[e][0]);
            var c1 = mesh.GetVertexCoordinates(mesh.Edges[e][1]);
            int difference = (int)System.Math.Round(c1[axis] - c0[axis]);
            int wrapped = ((difference % extent) + extent) % extent;
            g[e] = wrapped == extent - 1 ? -1 : wrapped;
        }
        return g;
    }),
];
int generatorsNotClosed = flatGenerators.Count(generator =>
{
    for (int f = 0; f < faceCount; f++)
    {
        int sum = 0;
        int[] be = mesh.FaceBoundaryEdges[f];
        int[] bo = mesh.FaceBoundaryOrientations[f];
        for (int i = 0; i < be.Length; i++) sum += bo[i] * generator[be[i]];
        if (sum != 0) return true;
    }
    return false;
});
var flatBasis = new List<double[]>();
foreach (int[] generator in flatGenerators)
    for (int a = 0; a < dimG; a++)
    {
        var candidate = new double[dof];
        for (int e = 0; e < edgeCount; e++) candidate[e * dimG + a] = generator[e];
        for (int pass = 0; pass < 2; pass++)
            foreach (double[] existing in flatBasis)
            {
                double projection = Dot(candidate, existing);
                for (int i = 0; i < dof; i++) candidate[i] -= projection * existing[i];
            }
        double norm = System.Math.Sqrt(Dot(candidate, candidate));
        if (norm <= 1e-08) continue;
        for (int i = 0; i < dof; i++) candidate[i] /= norm;
        flatBasis.Add(candidate);
    }
double[][] flatBasisMatrix = [.. flatBasis];
using var p550Document = JsonDocument.Parse(File.ReadAllBytes(
    bindingSpecs.First(x => x.Id == "phase550-summary").Path));
int reportedFlatDimension = p550Document.RootElement.GetProperty("structuralPrechecks")
    .GetProperty("measuredNullBasisDimension").GetInt32();
bool flatBasisAgreesWithPhase550 = generatorsNotClosed == 0 && flatBasisMatrix.Length == reportedFlatDimension;

// ------------------------------------------------ replay of the committed chains
string[] observableNames = ["actionDensity", "forceNormSquared", "configurationNormSquared"];
string[] decompositionNames = ["flatSectorNormSquared", "complementNormSquared"];
string[] allSeries = [.. observableNames, .. decompositionNames];
double replayTolerance = contract.GetProperty("replay").GetProperty("deltaHRelativeTolerance").GetDouble();
var replayRows = new List<object>();
var seriesByChain = new Dictionary<string, Dictionary<string, double[]>>();
bool replayBitIdentical = true;
foreach (var plan in chainPlan)
{
    string chainId = $"{plan.TableId}-{plan.RawSeed}";
    using var telemetry = JsonDocument.Parse(File.ReadAllBytes(
        $"studies/phase548_bounded_complete_lattice_pilot_execution_001/output/telemetry/{chainId}_trajectories.json"));
    var recorded = telemetry.RootElement.GetProperty("rows").EnumerateArray().Select(r => new
    {
        Accepted = r.GetProperty("accepted").GetBoolean(),
        DeltaH = r.GetProperty("deltaH").GetDouble(),
    }).ToArray();

    var rng = new Xoshiro(ExpandSeed((ulong)plan.ExecutionSeed));
    var position = new double[dof];
    for (int i = 0; i < dof; i++) position[i] = plan.Scale * Gauss(rng);
    (double action, double[] gradient) current = Evaluate(position);
    var series = allSeries.ToDictionary(name => name, _ => new List<double>());
    double worstDelta = 0.0;
    int accepted = 0;
    bool decisionsMatch = true;
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
        if (accept) { position = q; current = (action, gradient); accepted++; }

        double scale = System.Math.Max(1.0, System.Math.Abs(recorded[t].DeltaH));
        worstDelta = System.Math.Max(worstDelta, System.Math.Abs(delta - recorded[t].DeltaH) / scale);
        decisionsMatch &= accept == recorded[t].Accepted;

        if (t >= warmupPerChain)
        {
            double flatSquared = 0.0;
            foreach (double[] basisVector in flatBasisMatrix)
            {
                double coefficient = Dot(position, basisVector);
                flatSquared += coefficient * coefficient;
            }
            double total = Dot(position, position);
            series["actionDensity"].Add(current.action / dof);
            series["forceNormSquared"].Add(Dot(current.gradient, current.gradient));
            series["configurationNormSquared"].Add(total);
            series["flatSectorNormSquared"].Add(flatSquared);
            series["complementNormSquared"].Add(total - flatSquared);
        }
    }

    string checkpointPath = $"studies/phase548_bounded_complete_lattice_pilot_execution_001/output/checkpoints/{chainId}.json";
    using var checkpoint = JsonDocument.Parse(File.ReadAllBytes(checkpointPath));
    double[] storedPosition = checkpoint.RootElement.GetProperty("payload").GetProperty("position")
        .EnumerateArray().Select(x => x.GetDouble()).ToArray();
    bool positionBitIdentical = storedPosition.Length == dof
        && storedPosition.Zip(position).All(pair =>
            BitConverter.DoubleToInt64Bits(pair.First) == BitConverter.DoubleToInt64Bits(pair.Second));
    bool ok = decisionsMatch && positionBitIdentical && worstDelta <= replayTolerance
        && series["actionDensity"].Count == retainedPerChain;
    replayBitIdentical &= ok;
    seriesByChain[chainId] = series.ToDictionary(x => x.Key, x => x.Value.ToArray());
    replayRows.Add(new
    {
        chainId, decisionsMatch, positionBitIdentical,
        worstRelativeDeltaHDeviation = worstDelta,
        acceptanceRate = (double)accepted / trajectoriesPerChain,
        retainedDraws = series["actionDensity"].Count, passed = ok,
    });
    Console.WriteLine($"  replayed {chainId}: bitIdentical={positionBitIdentical}, decisions={decisionsMatch}");
}

// ---------------------------------------------------- frozen drift statistics
int batchCount = driftSpec.GetProperty("batchCount").GetInt32();
double zThreshold = driftSpec.GetProperty("absoluteZThreshold").GetDouble();
int drivingChainCount = driftSpec.GetProperty("chainsRequiredToDeclareDrift").GetInt32();
string[] decisionSeries = driftSpec.GetProperty("decisionSeries").EnumerateArray().Select(x => x.GetString()!).ToArray();
var driftRows = new List<object>();
bool driftInconclusive = false;
var driftCountBySeries = allSeries.ToDictionary(x => x, _ => 0);
foreach (var entry in seriesByChain)
{
    foreach (string name in allSeries)
    {
        double[] values = entry.Value[name];
        int batchSize = values.Length / batchCount;
        var batchMeans = new double[batchCount];
        for (int b = 0; b < batchCount; b++)
        {
            double sum = 0.0;
            for (int i = 0; i < batchSize; i++) sum += values[b * batchSize + i];
            batchMeans[b] = sum / batchSize;
        }
        int half = batchCount / 2;
        double firstMean = batchMeans.Take(half).Average();
        double secondMean = batchMeans.Skip(batchCount - half).Average();
        double firstVariance = Variance(batchMeans.Take(half).ToArray());
        double secondVariance = Variance(batchMeans.Skip(batchCount - half).ToArray());
        double halfStandardError = System.Math.Sqrt(firstVariance / half + secondVariance / half);
        double halfZ = halfStandardError > 0.0 ? (secondMean - firstMean) / halfStandardError : double.NaN;

        double indexMean = (batchCount - 1) / 2.0;
        double batchMean = batchMeans.Average();
        double covariance = 0.0, indexVariance = 0.0;
        for (int b = 0; b < batchCount; b++)
        {
            covariance += (b - indexMean) * (batchMeans[b] - batchMean);
            indexVariance += (b - indexMean) * (b - indexMean);
        }
        double slope = covariance / indexVariance;
        double residualSum = 0.0;
        for (int b = 0; b < batchCount; b++)
        {
            double predicted = batchMean + slope * (b - indexMean);
            residualSum += (batchMeans[b] - predicted) * (batchMeans[b] - predicted);
        }
        double slopeStandardError = System.Math.Sqrt(residualSum / (batchCount - 2) / indexVariance);
        double slopeZ = slopeStandardError > 0.0 ? slope / slopeStandardError : double.NaN;

        bool conclusive = double.IsFinite(halfZ) && double.IsFinite(slopeZ);
        driftInconclusive |= !conclusive;
        bool drifts = conclusive && (System.Math.Abs(halfZ) > zThreshold || System.Math.Abs(slopeZ) > zThreshold);
        if (drifts) driftCountBySeries[name]++;
        driftRows.Add(new
        {
            chainId = entry.Key, series = name, batchCount, batchSize,
            firstHalfMean = firstMean, secondHalfMean = secondMean,
            halfWindowZ = Reportable(halfZ), regressionSlope = slope, slopeZ = Reportable(slopeZ),
            conclusive, drifts,
        });
    }
}
bool driftDetected = decisionSeries.Any(name => driftCountBySeries[name] >= drivingChainCount);

// ---------------------------------- split rank-normalized R-hat on every series
var convergenceRows = new List<object>();
foreach (var table in chainPlan.GroupBy(x => x.TableId))
    foreach (string name in allSeries)
    {
        double[][] chains = table.Select(plan => seriesByChain[$"{plan.TableId}-{plan.RawSeed}"][name]).ToArray();
        Diagnostics diagnostics = Diagnose(chains);
        convergenceRows.Add(new
        {
            table = table.Key, series = name,
            splitRankNormalizedRhat = Reportable(diagnostics.Rhat),
            bulkEss = Reportable(diagnostics.BulkEss),
            tailEss = Reportable(diagnostics.TailEss),
            isDiagnosticOnlyAndDoesNotRehabilitateAnyGate = true,
        });
    }

string verdict = !contractValid || !exactBindingsValid || !flatBasisAgreesWithPhase550 ? taxonomy[0]
    : !resourceAccepted ? taxonomy[1]
    : !replayBitIdentical ? taxonomy[2]
    : driftInconclusive ? taxonomy[3]
    : driftDetected ? taxonomy[4]
    : taxonomy[5];

var result = new
{
    schemaVersion = 1,
    phase = 552,
    phaseId = "phase552-committed-chain-stationarity-reanalysis",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    resourceAccepted,
    bindings,
    honestyDisclosure = new
    {
        analysisIsRetrospectiveOnKnownData = true,
        statisticsAndThresholdsFrozenBeforeReplay = true,
        isDiagnosticOnly = true,
        changesPhase548Terminal = false,
        isAConvergenceAssessmentOfThePilot = false,
        note = "The summary of this dataset was already known when these statistics were chosen. Recomputing a different function of an already-determined dataset consumes no blind currency, but it is not a prospective test and is not reported as one.",
    },
    flatSectorBasis = new
    {
        rebuiltFromMesh = true,
        generatorsNotClosed,
        dimension = flatBasisMatrix.Length,
        reportedPhase550Dimension = reportedFlatDimension,
        agreesWithPhase550 = flatBasisAgreesWithPhase550,
    },
    replay = new
    {
        chains = replayRows,
        bitIdentical = replayBitIdentical,
        newSamplingPerformed = false,
        reExecutesAlreadyCommittedTrajectoriesUnderIdenticalSeeds = true,
        corroboratedByPhase549 = true,
    },
    driftTest = new
    {
        batchCount, absoluteZThreshold = zThreshold,
        chainsRequiredToDeclareDrift = drivingChainCount,
        decisionSeries,
        driftChainCountBySeries = driftCountBySeries,
        driftDetected, driftInconclusive,
        rows = driftRows,
    },
    decomposedConvergence = new
    {
        rows = convergenceRows,
        note = "Recomputed on the decomposed series for diagnosis. It rehabilitates no Phase548 gate and selects no configuration.",
    },
    resource = new
    {
        forwardedPhase546Rule = true, ceilingModified = false, newSamplingRequested = false,
        perTableRequestedCpuTicks = perTableTicks, maximumAggregateCpuTicks, accepted = resourceAccepted,
    },
    verdictKind = verdict,
    terminalStatus = "committed-chain-stationarity-reanalysis-" + verdict,
    decision = verdict switch
    {
        "stationary-under-resolved-consistent" =>
            "Under the frozen batch-mean drift statistics, the six committed chains show no detected drift in the squared configuration norm or in either of its flat-sector and complement components across the retained window. This is consistent with chains that were stationary but under-resolved, and it is a diagnostic statement about six already-determined chains, not a stationarity result for the target.",
        "non-stationary-drift-detected" =>
            "The frozen drift statistics detect systematic movement across the retained window in at least one decision series in at least the required number of chains. This is load-bearing for stopping the lane and requires an independent adjudicator before it is relied on.",
        "drift-test-inconclusive" =>
            "At least one frozen drift statistic was not computable, so the test is inconclusive and no stationarity reading follows.",
        _ => "The earliest frozen failure is preserved.",
    },
    independentAdjudicationRequired = verdict == "non-stationary-drift-detected",
    scope = new
    {
        establishesStationarityOfTheTarget = false,
        rehabilitatesAnyFailedGate = false,
        selectsADefault = false,
        touchesPhase481Phase458OrO4 = false,
        workbenchRelativeLatticeUnitsOnly = true,
    },
    analysisIsRetrospectiveOnKnownData = true,
    newSamplingPerformed = false,
    phase548TerminalChanged = false,
    registeredBlindSeedTouched = false,
    nullSpaceInterpretedAsGaugeVolume = false,
    quotientApplied = false,
    gaugeFixingApplied = false,
    measureNormalizationApplied = false,
    phase548Or549Reinterpreted = false,
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
WriteResult(result);
Console.WriteLine($"Phase552 verdict: {verdict}");
Console.WriteLine($"replayBitIdentical={replayBitIdentical}, driftDetected={driftDetected}, inconclusive={driftInconclusive}");

void WriteResult(object payload)
{
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(payload,
        new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    File.WriteAllBytes(OutputPath, bytes);
    File.WriteAllBytes(SummaryPath, bytes);
}

static double? Reportable(double value) => double.IsFinite(value) ? value : null;
static double Dot(double[] a, double[] b)
{
    double sum = 0.0;
    for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
    return sum;
}
static double Variance(double[] values)
{
    if (values.Length < 2) return 0.0;
    double mean = values.Average();
    double sum = 0.0;
    foreach (double value in values) sum += (value - mean) * (value - mean);
    return sum / (values.Length - 1);
}
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

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

sealed record Diagnostics(double Rhat, double BulkEss, double TailEss);
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
