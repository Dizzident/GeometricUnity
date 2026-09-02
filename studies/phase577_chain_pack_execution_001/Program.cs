using System.Security.Cryptography;
using System.Text.Json;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase577_chain_pack_execution_001";
const string ContractPath = Root + "/preregistration/phase577_chain_pack_execution_contract_v1.json";
const string OutputPath = Root + "/output/chain_pack_execution.json";
const string SummaryPath = Root + "/output/chain_pack_execution_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
bool exactBindingsValid = bindings.Length == 11
    && contract.GetProperty("requiredExactBindingCount").GetInt32() == 11
    && specs.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == 11
    && specs.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == 11
    && bindings.All(x => x.hashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input", "known-answer-battery-failed", "a41-upstream-gate-refused",
    "resource-refusal", "pack-execution-incomplete", "pack-diagnostics-inconclusive",
    "pack-executed-under-resolved", "pack-executed-all-frozen-resolution-gates-pass",
];
JsonElement batterySpec = contract.GetProperty("estimatorBattery");
JsonElement precheckSpec = contract.GetProperty("prechecks");
JsonElement diagnosticSpec = contract.GetProperty("diagnostics");
JsonElement resourceSpec = contract.GetProperty("resourceRefusal");
JsonElement authorization = contract.GetProperty("userSamplingAuthorization");
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("phase").GetInt32() == 577
    && contract.GetProperty("contractId").GetString() == "phase577-a41-chain-pack-execution-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("frozenBeforeFirstRegisteredSeedDrawn").GetBoolean()
    && contract.GetProperty("pristineSeedBlindPreregistration").GetBoolean()
    && authorization.GetProperty("quote").GetString() == "Perform the next step when you're ready, I'll run it over night"
    && authorization.GetProperty("scope").GetString() == "single execution of a40-disjoint-seed-chain-pack-v1"
    && taxonomy.SequenceEqual(expectedTaxonomy, StringComparer.Ordinal)
    && diagnosticSpec.GetProperty("maximumRhat").GetDouble() == 1.01
    && diagnosticSpec.GetProperty("minimumEss").GetDouble() == 100.0
    && diagnosticSpec.GetProperty("seriesCount").GetInt32() == 18
    && diagnosticSpec.GetProperty("tableRowCount").GetInt32() == 36
    && contract.GetProperty("retention").GetProperty("rawDirectionalSeriesRetentionMandatory").GetBoolean()
    && contract.GetProperty("retention").GetProperty("checkpointEveryTrajectories").GetInt32() == 250
    && resourceSpec.GetProperty("expectedForceEvaluations").GetInt64() == 561000
    && resourceSpec.GetProperty("maximumForceEvaluations").GetInt64() == 620000
    && resourceSpec.GetProperty("refuseBeforeAllocation").GetBoolean()
    && contract.GetProperty("claimBoundary").GetProperty("workbenchRelativeLatticeUnitsOnly").GetBoolean()
    && contract.GetProperty("claimBoundary").GetProperty("establishesStationarity").GetBoolean() == false
    && contract.GetProperty("claimBoundary").GetProperty("underResolvedTerminalIsFirstClassNegative").GetBoolean()
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && exactBindingsValid;

string SelectTerminal(bool invalid, bool batteryFailed, bool gateRefused, bool resourceRefused,
    bool incomplete, bool inconclusive, bool underResolved)
{
    if (invalid) return taxonomy[0];
    if (batteryFailed) return taxonomy[1];
    if (gateRefused) return taxonomy[2];
    if (resourceRefused) return taxonomy[3];
    if (incomplete) return taxonomy[4];
    if (inconclusive) return taxonomy[5];
    if (underResolved) return taxonomy[6];
    return taxonomy[7];
}
var truthTable = new[]
{
    new { id = "invalid", actual = SelectTerminal(true, false, false, false, false, false, false), expected = taxonomy[0] },
    new { id = "battery", actual = SelectTerminal(false, true, false, false, false, false, false), expected = taxonomy[1] },
    new { id = "gate", actual = SelectTerminal(false, false, true, false, false, false, false), expected = taxonomy[2] },
    new { id = "resource", actual = SelectTerminal(false, false, false, true, false, false, false), expected = taxonomy[3] },
    new { id = "incomplete", actual = SelectTerminal(false, false, false, false, true, false, false), expected = taxonomy[4] },
    new { id = "inconclusive", actual = SelectTerminal(false, false, false, false, false, true, false), expected = taxonomy[5] },
    new { id = "under-resolved", actual = SelectTerminal(false, false, false, false, false, false, true), expected = taxonomy[6] },
    new { id = "gates-pass", actual = SelectTerminal(false, false, false, false, false, false, false), expected = taxonomy[7] },
    new { id = "incomplete-precedes-inconclusive", actual = SelectTerminal(false, false, false, false, true, true, false), expected = taxonomy[4] },
    new { id = "early-precedence", actual = SelectTerminal(true, true, true, true, true, true, true), expected = taxonomy[0] },
};
bool truthTablePassed = truthTable.All(x => x.actual == x.expected)
    && expectedTaxonomy.All(terminal => truthTable.Any(x => x.actual == terminal));

// Estimator known-answer battery, before any registered seed is drawn.
double[][] iidChains = SyntheticChains(0.0, 0.0);
double[][] ar1Chains = SyntheticChains(0.9, 0.0);
double[][] separatedChains = SyntheticChains(0.0, 4.0);
Diagnostics iidDiagnostics = Diagnose(iidChains);
Diagnostics ar1Diagnostics = Diagnose(ar1Chains);
Diagnostics separatedDiagnostics = Diagnose(separatedChains);
bool estimatorBatteryPassed =
    InBand(iidDiagnostics.Rhat, batterySpec.GetProperty("iidRhat"))
    && InBand(iidDiagnostics.BulkEss, batterySpec.GetProperty("iidBulkEss"))
    && InBand(iidDiagnostics.TailEss, batterySpec.GetProperty("iidTailEss"))
    && InBand(ar1Diagnostics.Rhat, batterySpec.GetProperty("ar1Phi09Rhat"))
    && InBand(ar1Diagnostics.BulkEss, batterySpec.GetProperty("ar1Phi09BulkEss"))
    && InBand(ar1Diagnostics.TailEss, batterySpec.GetProperty("ar1Phi09TailEss"))
    && separatedDiagnostics.Rhat > batterySpec.GetProperty("separatedRhatMinimumExclusive").GetDouble();
byte[] checksumFixture = System.Text.Encoding.UTF8.GetBytes("{\"phase\":577,\"fixture\":\"checksum\"}");
byte[] tamperedFixture = (byte[])checksumFixture.Clone();
tamperedFixture[^2] ^= 1;
bool checksumTamperDetected = Convert.ToHexString(SHA256.HashData(checksumFixture))
    != Convert.ToHexString(SHA256.HashData(tamperedFixture));
bool knownAnswerPassed = truthTablePassed && estimatorBatteryPassed && checksumTamperDetected;
var knownAnswerBattery = new
{
    auditedNumericDataParsedBeforeBattery = false,
    registeredSeedDrawnBeforeBattery = false,
    estimator = new { iidDiagnostics, ar1Diagnostics, separatedDiagnostics, passed = estimatorBatteryPassed },
    classificationTruthTable = new { rows = truthTable, everyTerminalReached = expectedTaxonomy.All(t => truthTable.Any(x => x.actual == t)), passed = truthTablePassed },
    checksumTamperDetected, passed = knownAnswerPassed,
};
if (!contractValid || !knownAnswerPassed)
{
    string early = !contractValid ? taxonomy[0] : taxonomy[1];
    Emit(Early(early, contractValid, exactBindingsValid, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase577 verdict: {early}");
    return;
}

// Only now parse the exact-bound upstream records and the frozen pack.
JsonElement p575 = ReadBinding("phase575-full");
JsonElement p576 = ReadBinding("phase576-full");
JsonElement requiredVerdicts = contract.GetProperty("requiredUpstreamVerdicts");
JsonElement pack = p576.GetProperty("frozenChainPack");
bool upstreamGateOpen = p575.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase575").GetString()
    && p576.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase576").GetString()
    && p575.GetProperty("prospectiveChainPackPlanningGateOpen").GetBoolean()
    && pack.GetProperty("packId").GetString() == "a40-disjoint-seed-chain-pack-v1";
if (!upstreamGateOpen)
{
    Emit(Early(taxonomy[2], true, true, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase577 verdict: {taxonomy[2]}");
    return;
}

double stepSize = pack.GetProperty("proposal").GetProperty("stepSize").GetDouble();
int leapfrogSteps = pack.GetProperty("proposal").GetProperty("leapfrogSteps").GetInt32();
double divergenceThreshold = pack.GetProperty("proposal").GetProperty("divergenceAbsoluteDeltaH").GetDouble();
int warmupPerChain = pack.GetProperty("schedule").GetProperty("warmupPerChain").GetInt32();
int retainedPerChain = pack.GetProperty("schedule").GetProperty("retainedPerChain").GetInt32();
int trajectoriesPerChain = pack.GetProperty("schedule").GetProperty("trajectoriesPerChain").GetInt32();
int checkpointEvery = pack.GetProperty("retention").GetProperty("checkpointEveryTrajectories").GetInt32();
var chainPlan = pack.GetProperty("schedule").GetProperty("tables").EnumerateArray().SelectMany(table =>
{
    string tableId = table.GetProperty("id").GetString()!;
    int offset = table.GetProperty("seedOffset").GetInt32();
    int[] seeds = table.GetProperty("seeds").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    double[] scales = table.GetProperty("initialScales").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    return seeds.Select((seed, i) => new ChainPlan(tableId, seed, seed + offset, scales[i]));
}).ToArray();
long estimatedForceEvaluations = checked((long)chainPlan.Length * trajectoriesPerChain * (leapfrogSteps + 1));
bool resourceAccepted = chainPlan.Length == 8 && trajectoriesPerChain == warmupPerChain + retainedPerChain
    && trajectoriesPerChain == 2125 && retainedPerChain == 1847 && leapfrogSteps == 32 && stepSize == 0.06
    && estimatedForceEvaluations == resourceSpec.GetProperty("expectedForceEvaluations").GetInt64()
    && estimatedForceEvaluations <= resourceSpec.GetProperty("maximumForceEvaluations").GetInt64();
if (!resourceAccepted)
{
    Emit(Early(taxonomy[3], true, true, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase577 verdict: {taxonomy[3]}");
    return;
}

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(3, latticeCanonical: true);
var member = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2,
    Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5,
    EpsilonMode = "independent-theta",
};
var op = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: 3);
var mass = new CpuMassMatrix(mesh, algebra);
int dimG = algebra.Dimension, edgeCount = mesh.EdgeCount, dof = edgeCount * dimG;
var thetaZero = new double[mesh.VertexCount * dimG];
(double Action, double[] Gradient) Evaluate(double[] omega)
{
    var joint = op.ComputeJointGradient(omega, thetaZero, mass);
    return (joint.Objective, joint.GradOmega);
}

// Deterministic prechecks before any registered seed is drawn.
(double originAction, double[] originGradient) = Evaluate(new double[dof]);
bool originExact = originAction == 0.0 && originGradient.All(value => value == 0.0);
var probeState = new double[dof];
for (int i = 0; i < dof; i++) probeState[i] = 0.01 * System.Math.Sin(0.7 * i + 0.3);
var probeDirection = new double[dof];
for (int i = 0; i < dof; i++) probeDirection[i] = System.Math.Cos(1.3 * i + 0.1);
double directionNorm = System.Math.Sqrt(Dot(probeDirection, probeDirection));
for (int i = 0; i < dof; i++) probeDirection[i] /= directionNorm;
(double probeAction, double[] probeGradient) = Evaluate(probeState);
const double FiniteDifferenceStep = 1e-5;
var forwardState = (double[])probeState.Clone();
var backwardState = (double[])probeState.Clone();
for (int i = 0; i < dof; i++) { forwardState[i] += FiniteDifferenceStep * probeDirection[i]; backwardState[i] -= FiniteDifferenceStep * probeDirection[i]; }
double centered = (Evaluate(forwardState).Action - Evaluate(backwardState).Action) / (2.0 * FiniteDifferenceStep);
double analytic = Dot(probeGradient, probeDirection);
double gradientScaledError = System.Math.Abs(centered - analytic) / System.Math.Max(1.0, System.Math.Abs(analytic));
var reverseMomentum = new double[dof];
for (int i = 0; i < dof; i++) reverseMomentum[i] = 0.05 * System.Math.Sin(2.1 * i + 0.9);
(double[] forwardQ, double[] forwardP, bool forwardFinite) = Leapfrog(probeState, reverseMomentum, probeAction, probeGradient);
var negatedP = forwardP.Select(value => -value).ToArray();
(double forwardAction, double[] forwardGradient) = Evaluate(forwardQ);
(double[] returnQ, _, bool reverseFinite) = Leapfrog(forwardQ, negatedP, forwardAction, forwardGradient);
double reversibilityError = System.Math.Sqrt(Dot(Subtract(returnQ, probeState), Subtract(returnQ, probeState)))
    / System.Math.Max(1.0, System.Math.Sqrt(Dot(probeState, probeState)));
bool prechecksPassed = originExact && forwardFinite && reverseFinite
    && gradientScaledError <= precheckSpec.GetProperty("directionalGradientMaximumScaledError").GetDouble()
    && reversibilityError <= precheckSpec.GetProperty("reversibilityMaximumRelativeError").GetDouble();
if (!prechecksPassed)
{
    Emit(Early(taxonomy[1], true, true, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase577 verdict: {taxonomy[1]}");
    return;
}

// Side-A observable basis (phase570 scalar construction, verbatim).
var rawEGenerators = new List<int[]>();
for (int vertex = 0; vertex < mesh.VertexCount; vertex++)
{
    var candidate = new int[edgeCount];
    for (int edge = 0; edge < edgeCount; edge++)
        candidate[edge] = (mesh.Edges[edge][1] == vertex ? 1 : 0) - (mesh.Edges[edge][0] == vertex ? 1 : 0);
    rawEGenerators.Add(candidate);
}
var rawWGenerators = new List<int[]>();
for (int axis = 0; axis < 4; axis++)
{
    var candidate = new int[edgeCount];
    for (int edge = 0; edge < edgeCount; edge++)
    {
        var c0 = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
        var c1 = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
        int difference = (int)System.Math.Round(c1[axis] - c0[axis]);
        int wrapped = ((difference % 3) + 3) % 3;
        candidate[edge] = wrapped == 2 ? -1 : wrapped;
    }
    rawWGenerators.Add(candidate);
}
var eBasis = new List<double[]>();
foreach (int[] raw in rawEGenerators) AddOrthonormal(raw.Select(x => (double)x).ToArray(), eBasis);
var wBasis = new List<double[]>();
foreach (int[] raw in rawWGenerators)
{
    double[] candidate = raw.Select(x => (double)x).ToArray();
    Orthogonalize(candidate, eBasis);
    AddOrthonormal(candidate, wBasis);
}
double[][] cBasis = [.. eBasis, .. wBasis];
if (eBasis.Count != 80 || wBasis.Count != 4)
{
    Emit(Early(taxonomy[0], true, true, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase577 verdict: {taxonomy[0]}");
    return;
}

string[] seriesNames =
[
    "actionDensity", "forceNormSquared", "configurationNormSquared",
    "eNormSquared", "wNormSquared", "closedNormSquared", "closedPerpNormSquared",
    "closedGramLargest", "closedGramMiddle", "closedGramSmallest",
    "closedRankOneAlignment", "withinClosedRankOneDistanceSquared", "withinClosedRankOneRelativeDistance", "fullRankOneDistanceSquared",
    "eMovementSquared", "wMovementSquared", "closedMovementSquared", "closedPerpMovementSquared",
];
var seriesByChain = new Dictionary<string, Dictionary<string, double[]>>(StringComparer.Ordinal);
var chainRows = new List<object>();
bool executionComplete = true;
long observedForceEvaluations = 0;
int totalNonFinite = 0, totalDivergent = 0;
double largestAbsoluteEnergyError = 0.0;
foreach (ChainPlan plan in chainPlan)
{
    string chainId = $"{plan.TableId}-{plan.RawSeed}";
    var rng = new Xoshiro(ExpandSeed((ulong)plan.ExecutionSeed));
    var position = new double[dof];
    for (int i = 0; i < dof; i++) position[i] = plan.InitialScale * Gauss(rng);
    (double Action, double[] Gradient) current = Evaluate(position);
    var series = seriesNames.ToDictionary(x => x, _ => new List<double>(), StringComparer.Ordinal);
    var telemetryRows = new List<object>();
    int accepted = 0, chainNonFinite = 0, chainDivergent = 0;
    for (int trajectory = 0; trajectory < trajectoriesPerChain; trajectory++)
    {
        double[] before = (double[])position.Clone();
        var momentum = new double[dof];
        for (int i = 0; i < dof; i++) momentum[i] = Gauss(rng);
        double logUniform = System.Math.Log(Uniform(rng));
        double initialHamiltonian = current.Action + 0.5 * Dot(momentum, momentum);
        var q = (double[])position.Clone();
        var p = (double[])momentum.Clone();
        double action = current.Action;
        double[] gradient = current.Gradient;
        bool finite = true;
        for (int i = 0; i < dof; i++) p[i] -= 0.5 * stepSize * gradient[i];
        for (int leap = 0; leap < leapfrogSteps; leap++)
        {
            for (int i = 0; i < dof; i++) q[i] += stepSize * p[i];
            (action, gradient) = Evaluate(q);
            observedForceEvaluations++;
            if (!double.IsFinite(action) || !gradient.All(double.IsFinite) || !q.All(double.IsFinite)) { finite = false; break; }
            double kick = leap + 1 == leapfrogSteps ? 0.5 * stepSize : stepSize;
            for (int i = 0; i < dof; i++) p[i] -= kick * gradient[i];
        }
        observedForceEvaluations++;
        finite &= p.All(double.IsFinite);
        double finalHamiltonian = finite ? action + 0.5 * Dot(p, p) : double.NaN;
        double deltaH = finalHamiltonian - initialHamiltonian;
        bool divergent = !finite || !double.IsFinite(deltaH) || System.Math.Abs(deltaH) > divergenceThreshold;
        bool accept = finite && !divergent && logUniform <= System.Math.Min(0.0, -deltaH);
        if (accept) { position = q; current = (action, gradient); accepted++; }
        if (!finite) chainNonFinite++;
        if (divergent) chainDivergent++;
        if (finite) largestAbsoluteEnergyError = System.Math.Max(largestAbsoluteEnergyError, System.Math.Abs(deltaH));
        telemetryRows.Add(new { trajectory, accepted = accept, deltaH = double.IsFinite(deltaH) ? deltaH : (double?)null, finite, divergent });
        if (trajectory >= warmupPerChain)
        {
            InvariantMetrics state = Measure(position);
            InvariantMetrics move = Measure(Subtract(position, before));
            series["actionDensity"].Add(current.Action / dof);
            series["forceNormSquared"].Add(Dot(current.Gradient, current.Gradient));
            series["configurationNormSquared"].Add(state.TotalNormSquared);
            series["eNormSquared"].Add(state.ENormSquared);
            series["wNormSquared"].Add(state.WNormSquared);
            series["closedNormSquared"].Add(state.ClosedNormSquared);
            series["closedPerpNormSquared"].Add(state.ClosedPerpNormSquared);
            series["closedGramLargest"].Add(state.GramEigenvalues[0]);
            series["closedGramMiddle"].Add(state.GramEigenvalues[1]);
            series["closedGramSmallest"].Add(state.GramEigenvalues[2]);
            series["closedRankOneAlignment"].Add(state.RankOneAlignment);
            series["withinClosedRankOneDistanceSquared"].Add(state.WithinClosedRankOneDistanceSquared);
            series["withinClosedRankOneRelativeDistance"].Add(state.WithinClosedRankOneRelativeDistance);
            series["fullRankOneDistanceSquared"].Add(state.FullRankOneDistanceSquared);
            series["eMovementSquared"].Add(move.ENormSquared);
            series["wMovementSquared"].Add(move.WNormSquared);
            series["closedMovementSquared"].Add(move.ClosedNormSquared);
            series["closedPerpMovementSquared"].Add(move.ClosedPerpNormSquared);
        }
        if ((trajectory + 1) % checkpointEvery == 0 || trajectory + 1 == trajectoriesPerChain)
            WriteCheckpoint(chainId, trajectory + 1, position);
    }
    WriteTelemetry(chainId, telemetryRows);
    bool chainComplete = telemetryRows.Count == trajectoriesPerChain
        && series.All(x => x.Value.Count == retainedPerChain);
    executionComplete &= chainComplete;
    totalNonFinite += chainNonFinite;
    totalDivergent += chainDivergent;
    seriesByChain[chainId] = series.ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.Ordinal);
    chainRows.Add(new
    {
        chainId, seed = plan.RawSeed, executionSeed = plan.ExecutionSeed, initialScale = plan.InitialScale,
        acceptanceRate = (double)accepted / trajectoriesPerChain,
        nonFiniteTrajectories = chainNonFinite, divergentTrajectories = chainDivergent,
        retainedDraws = retainedPerChain, complete = chainComplete,
    });
    Console.WriteLine($"phase577 chain {chainId}: acceptance {(double)accepted / trajectoriesPerChain:F4}, nonFinite {chainNonFinite}, divergent {chainDivergent}");
}

double maximumRhat = diagnosticSpec.GetProperty("maximumRhat").GetDouble();
double minimumEss = diagnosticSpec.GetProperty("minimumEss").GetDouble();
var tableRows = new List<object>();
bool diagnosticsConclusive = true;
bool allGatesPass = true;
foreach (var table in chainPlan.GroupBy(x => x.TableId))
    foreach (string name in seriesNames)
    {
        double[][] chains = table.Select(x => seriesByChain[$"{x.TableId}-{x.RawSeed}"][name]).ToArray();
        Diagnostics d = Diagnose(chains);
        bool conclusive = double.IsFinite(d.Rhat) && double.IsFinite(d.BulkEss) && double.IsFinite(d.TailEss);
        bool passed = conclusive && d.Rhat <= maximumRhat && d.BulkEss >= minimumEss && d.TailEss >= minimumEss;
        diagnosticsConclusive &= conclusive;
        allGatesPass &= passed;
        tableRows.Add(new { table = table.Key, series = name, rhat = Reportable(d.Rhat), bulkEss = Reportable(d.BulkEss), tailEss = Reportable(d.TailEss), passed });
    }

string finalVerdict = SelectTerminal(false, false, false, false,
    !executionComplete, executionComplete && !diagnosticsConclusive,
    executionComplete && diagnosticsConclusive && !allGatesPass);
var rawTraces = seriesByChain.ToDictionary(chain => chain.Key, chain => chain.Value, StringComparer.Ordinal);
var result = new
{
    schemaVersion = 1, phase = 577, phaseId = "phase577-chain-pack-execution",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath),
    contractValid = true, exactBindingsValid = true, resourceAccepted = true, bindings, knownAnswerBattery,
    userSamplingAuthorization = new
    {
        grantedBy = authorization.GetProperty("grantedBy").GetString(),
        date = authorization.GetProperty("date").GetString(),
        quote = authorization.GetProperty("quote").GetString(),
        scope = authorization.GetProperty("scope").GetString(),
    },
    upstream = new
    {
        gatePassed = true,
        phase575Verdict = p575.GetProperty("verdictKind").GetString(),
        phase576Verdict = p576.GetProperty("verdictKind").GetString(),
        packId = pack.GetProperty("packId").GetString(),
    },
    prechecks = new
    {
        originExact, directionalGradientScaledError = gradientScaledError,
        reversibilityRelativeError = reversibilityError, passed = true,
    },
    execution = new
    {
        chainCount = chainPlan.Length, trajectoriesPerChain, warmupPerChain, retainedPerChain,
        stepSize, leapfrogSteps, observedForceEvaluations,
        totalNonFiniteTrajectories = totalNonFinite, totalDivergentTrajectories = totalDivergent,
        largestAbsoluteEnergyError, chains = chainRows, complete = executionComplete,
        pristineSeedBlindPreregistration = true,
    },
    tableDiagnostics = new
    {
        thresholds = new { maximumRhat, minimumBulkEss = minimumEss, minimumTailEss = minimumEss },
        rowCount = tableRows.Count, conclusive = diagnosticsConclusive, allGatesPass, rows = tableRows,
    },
    rawTraces,
    verdictKind = finalVerdict,
    terminalStatus = "chain-pack-execution-" + finalVerdict,
    workbenchRelativeLatticeUnitsOnly = true,
    establishesStationarity = false,
    establishesSamplingCorrectnessBeyondFrozenGates = false,
    establishesSpectralOrPhysicalQuantity = false,
    isProductionBenchmark = false,
    samplingPerformedUnderExplicitUserAuthorization = true,
    rawDirectionalSeriesRetained = true,
    phase548Or549TerminalChanged = false, phase570Or571Or572Reinterpreted = false,
    phase572ToleranceRelaxed = false, registeredBlindSeedTouched = false,
    protectedPhase554SeedsRead = false, registeredTargetChanged = false,
    directionCalledGaugeOrRedundant = false, quotientApplied = false, gaugeFixingApplied = false,
    measureNormalizationApplied = false, sourceOrModelSelected = false,
    phase561Opened = false, o4Discharged = false, phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};
Emit(result);
Console.WriteLine($"Phase577 verdict: {finalVerdict}");
Console.WriteLine($"executionComplete={executionComplete}");
Console.WriteLine($"allGatesPass={allGatesPass}");
Console.WriteLine($"totalNonFinite={totalNonFinite} totalDivergent={totalDivergent}");
Console.WriteLine($"promotedPhysicalMassClaimCount=0");

(double[] Q, double[] P, bool Finite) Leapfrog(double[] startQ, double[] startP, double startAction, double[] startGradient)
{
    var q = (double[])startQ.Clone();
    var p = (double[])startP.Clone();
    double action = startAction;
    double[] gradient = startGradient;
    bool finite = true;
    for (int i = 0; i < dof; i++) p[i] -= 0.5 * stepSize * gradient[i];
    for (int leap = 0; leap < leapfrogSteps; leap++)
    {
        for (int i = 0; i < dof; i++) q[i] += stepSize * p[i];
        (action, gradient) = Evaluate(q);
        if (!double.IsFinite(action) || !gradient.All(double.IsFinite)) { finite = false; break; }
        double kick = leap + 1 == leapfrogSteps ? 0.5 * stepSize : stepSize;
        for (int i = 0; i < dof; i++) p[i] -= kick * gradient[i];
    }
    return (q, p, finite);
}
InvariantMetrics Measure(double[] position)
{
    double total = Dot(position, position), eNorm = 0.0, wNorm = 0.0;
    var coefficientRows = new double[cBasis.Length][];
    for (int r = 0; r < cBasis.Length; r++)
    {
        var row = new double[dimG];
        for (int a = 0; a < dimG; a++)
            for (int edge = 0; edge < edgeCount; edge++)
                row[a] += cBasis[r][edge] * position[edge * dimG + a];
        coefficientRows[r] = row;
        double norm = Dot(row, row);
        if (r < eBasis.Count) eNorm += norm; else wNorm += norm;
    }
    double closed = eNorm + wNorm;
    double[] eigen = SymmetricEigenvalues3(GramFromRows(coefficientRows));
    double trace = System.Math.Max(0.0, eigen.Sum());
    double distanceSquared = System.Math.Max(0.0, eigen[1] + eigen[2]);
    double closedPerp = System.Math.Max(0.0, total - closed);
    return new InvariantMetrics(total, eNorm, wNorm, closed, System.Math.Max(0.0, total - closed), eigen,
        trace > 0.0 ? eigen[0] / trace : 1.0, distanceSquared, trace > 0.0 ? distanceSquared / trace : 0.0,
        closedPerp + distanceSquared);
}
void WriteCheckpoint(string chainId, int trajectoryCount, double[] position)
{
    string path = Root + $"/output/checkpoints/{chainId}_t{trajectoryCount:D4}.json";
    var payload = new { chainId, trajectoryCount, position };
    byte[] body = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    var envelope = new { checksum = Convert.ToHexString(SHA256.HashData(body)).ToLowerInvariant(), payload = new { chainId, trajectoryCount, position } };
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(envelope, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
}
void WriteTelemetry(string chainId, object rows)
{
    string path = Root + $"/output/telemetry/{chainId}_trajectories.json";
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    File.WriteAllBytes(path, JsonSerializer.SerializeToUtf8Bytes(new { chainId, rows },
        new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
}
static Diagnostics Diagnose(double[][] chains)
{
    int n = chains.Min(x => x.Length);
    double[] pooled = chains.SelectMany(x => x.Take(n)).ToArray();
    if (pooled.Distinct().Count() <= 1) return new Diagnostics(double.NaN, double.NaN, double.NaN);
    double[] ranked = RankNormalize(pooled), folded = RankNormalize(pooled.Select(x => System.Math.Abs(x - Median(pooled))).ToArray());
    double[][] r = Regroup(ranked, chains.Length, n), f = Regroup(folded, chains.Length, n);
    double[] ordered = pooled.Order().ToArray();
    double q05 = ordered[(int)System.Math.Floor(0.05 * (ordered.Length - 1))];
    double q95 = ordered[(int)System.Math.Ceiling(0.95 * (ordered.Length - 1))];
    double[][] lower = chains.Select(x => x.Take(n).Select(value => value <= q05 ? 1.0 : 0.0).ToArray()).ToArray();
    double[][] upper = chains.Select(x => x.Take(n).Select(value => value >= q95 ? 1.0 : 0.0).ToArray()).ToArray();
    return new Diagnostics(System.Math.Max(SplitRhat(r), SplitRhat(f)), Ess(Split(r)), System.Math.Min(Ess(Split(lower)), Ess(Split(upper))));
}
static double[] RankNormalize(double[] values)
{
    int n = values.Length; int[] order = Enumerable.Range(0, n).OrderBy(i => values[i]).ToArray(); var ranks = new double[n];
    for (int i = 0; i < n;) { int j = i; while (j + 1 < n && values[order[j + 1]] == values[order[i]]) j++; double rank = (i + j) / 2.0 + 1.0; for (int k = i; k <= j; k++) ranks[order[k]] = rank; i = j + 1; }
    return ranks.Select(x => InverseNormalCdf((x - 0.375) / (n + 0.25))).ToArray();
}
static double Median(double[] values) { double[] x = values.Order().ToArray(); return x.Length % 2 == 1 ? x[x.Length / 2] : 0.5 * (x[x.Length / 2 - 1] + x[x.Length / 2]); }
static double[][] Regroup(double[] flat, int m, int n) => Enumerable.Range(0, m).Select(c => flat.Skip(c * n).Take(n).ToArray()).ToArray();
static double[][] Split(double[][] chains) => chains.SelectMany(x => new[] { x.Take(x.Length / 2).ToArray(), x.Skip(x.Length - x.Length / 2).ToArray() }).ToArray();
static double SplitRhat(double[][] chains)
{
    double[][] split = Split(chains); int m = split.Length, n = split.Min(x => x.Length);
    double[] means = split.Select(x => x.Take(n).Average()).ToArray();
    double within = split.Select(x => Variance(x.Take(n).ToArray())).Average(); if (within <= 0.0) return double.NaN;
    double grand = means.Average(), between = n * means.Sum(x => (x - grand) * (x - grand)) / (m - 1);
    return System.Math.Sqrt((((n - 1.0) / n) * within + between / n) / within);
}
static double Ess(double[][] chains)
{
    int m = chains.Length, n = chains.Min(x => x.Length); double[][] x = chains.Select(y => y.Take(n).ToArray()).ToArray();
    double[] means = x.Select(y => y.Average()).ToArray(); double within = x.Select(Variance).Average(); if (within <= 0.0) return double.NaN;
    double grand = means.Average(), between = n * means.Sum(y => (y - grand) * (y - grand)) / (m - 1);
    double varPlus = ((n - 1.0) / n) * within + between / n; if (varPlus <= 0.0) return double.NaN;
    double[] rho = new double[n]; rho[0] = 1.0;
    for (int lag = 1; lag < n; lag++)
    {
        double covariance = 0.0;
        for (int c = 0; c < m; c++) { double sum = 0.0; for (int i = 0; i + lag < n; i++) sum += (x[c][i] - means[c]) * (x[c][i + lag] - means[c]); covariance += sum / n; }
        rho[lag] = 1.0 - (within - covariance / m) / varPlus;
    }
    double tau = -1.0, previous = double.PositiveInfinity;
    for (int k = 0; 2 * k + 1 < n; k++) { double pair = rho[2 * k] + rho[2 * k + 1]; if (pair < 0) break; pair = System.Math.Min(pair, previous); previous = pair; tau += 2.0 * pair; }
    return tau > 0.0 ? m * n / tau : double.NaN;
}
static double Variance(double[] values)
{
    if (values.Length < 2) return 0.0; double mean = values.Average(); return values.Sum(x => (x - mean) * (x - mean)) / (values.Length - 1);
}
static double InverseNormalCdf(double p)
{
    double[] a=[-39.69683028665376,220.9460984245205,-275.9285104469687,138.357751867269,-30.66479806614716,2.506628277459239];
    double[] b=[-54.47609879822406,161.5858368580409,-155.6989798598866,66.80131188771972,-13.28068155288572];
    double[] c=[-0.007784894002430293,-0.3223964580411365,-2.400758277161838,-2.549732539343734,4.374664141464968,2.938163982698783];
    double[] d=[0.007784695709041462,0.3224671290700398,2.445134137142996,3.754408661907416]; const double low=0.02425;
    if(p<low){double q=System.Math.Sqrt(-2*System.Math.Log(p));return (((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5])/((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);}
    if(p>1-low){double q=System.Math.Sqrt(-2*System.Math.Log(1-p));return -(((((c[0]*q+c[1])*q+c[2])*q+c[3])*q+c[4])*q+c[5])/((((d[0]*q+d[1])*q+d[2])*q+d[3])*q+1);}
    double r=p-0.5,s=r*r;return (((((a[0]*s+a[1])*s+a[2])*s+a[3])*s+a[4])*s+a[5])*r/(((((b[0]*s+b[1])*s+b[2])*s+b[3])*s+b[4])*s+1);}
static double[][] SyntheticChains(double phi, double separation)
{
    const int chainCount = 4, length = 400;
    var result = new double[chainCount][];
    for (int chain = 0; chain < chainCount; chain++)
    {
        var values = new double[length];
        for (int i = 1; i < length; i++)
            values[i] = phi * values[i - 1] + StatelessNormal(chain, i) + separation * (chain - 1.5);
        result[chain] = values;
    }
    return result;
}
static double StatelessNormal(int chain, int index)
{
    static ulong Mix(ulong value)
    {
        value += 0x9E3779B97F4A7C15UL;
        value = (value ^ (value >> 30)) * 0xBF58476D1CE4E5B9UL;
        value = (value ^ (value >> 27)) * 0x94D049BB133111EBUL;
        return value ^ (value >> 31);
    }
    ulong key = ((ulong)(chain + 1) << 32) | (uint)(index + 1);
    double u1 = ((Mix(key) >> 11) + 0.5) / 9007199254740992.0;
    double u2 = ((Mix(key ^ 0xD1B54A32D192ED03UL) >> 11) + 0.5) / 9007199254740992.0;
    return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
}
static bool InBand(double value, JsonElement band) => double.IsFinite(value)
    && value >= band.GetProperty("minimumInclusive").GetDouble()
    && value <= band.GetProperty("maximumInclusive").GetDouble();
static void AddOrthonormal(double[] candidate, List<double[]> basis)
{
    Orthogonalize(candidate, basis); Orthogonalize(candidate, basis);
    double norm = System.Math.Sqrt(Dot(candidate, candidate));
    if (norm <= 1e-10) return;
    for (int i = 0; i < candidate.Length; i++) candidate[i] /= norm;
    basis.Add(candidate);
}
static void Orthogonalize(double[] candidate, IEnumerable<double[]> basis)
{
    foreach (double[] vector in basis)
    {
        double projection = Dot(candidate, vector);
        for (int i = 0; i < candidate.Length; i++) candidate[i] -= projection * vector[i];
    }
}
static double[,] GramFromRows(IEnumerable<double[]> rows)
{
    var gram = new double[3, 3];
    foreach (double[] row in rows) for (int a = 0; a < 3; a++) for (int b = 0; b < 3; b++) gram[a, b] += row[a] * row[b];
    return gram;
}
static double[] SymmetricEigenvalues3(double[,] source)
{
    var a = (double[,])source.Clone();
    for (int sweep = 0; sweep < 40; sweep++)
    {
        int p = 0, q = 1;
        if (System.Math.Abs(a[0, 2]) > System.Math.Abs(a[p, q])) { p = 0; q = 2; }
        if (System.Math.Abs(a[1, 2]) > System.Math.Abs(a[p, q])) { p = 1; q = 2; }
        if (System.Math.Abs(a[p, q]) <= 1e-15 * System.Math.Max(1.0, System.Math.Abs(a[p, p]) + System.Math.Abs(a[q, q]))) break;
        double angle = 0.5 * System.Math.Atan2(2.0 * a[p, q], a[q, q] - a[p, p]);
        double c = System.Math.Cos(angle), s = System.Math.Sin(angle);
        for (int k = 0; k < 3; k++) if (k != p && k != q)
        {
            double apk = a[p, k], aqk = a[q, k];
            a[p, k] = a[k, p] = c * apk - s * aqk;
            a[q, k] = a[k, q] = s * apk + c * aqk;
        }
        double app = a[p, p], aqq = a[q, q], apq = a[p, q];
        a[p, p] = c * c * app - 2.0 * s * c * apq + s * s * aqq;
        a[q, q] = s * s * app + 2.0 * s * c * apq + c * c * aqq;
        a[p, q] = a[q, p] = 0.0;
    }
    double[] values = [a[0, 0], a[1, 1], a[2, 2]];
    Array.Sort(values); Array.Reverse(values); return values;
}
static double? Reportable(double value) => double.IsFinite(value) ? value : null;
static double[] Subtract(double[] a, double[] b) => a.Zip(b, (x, y) => x - y).ToArray();
static double Dot(double[] a, double[] b) { double sum = 0.0; for (int i = 0; i < a.Length; i++) sum += a[i] * b[i]; return sum; }
static double Uniform(Xoshiro rng) => ((rng.Next() >> 11) + 0.5) / 9007199254740992.0;
static double Gauss(Xoshiro rng) { double u1 = Uniform(rng), u2 = Uniform(rng); return System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Cos(2 * System.Math.PI * u2); }
static ulong[] ExpandSeed(ulong seed) { ulong state = seed; ulong Next() { state += 0x9E3779B97F4A7C15UL; ulong z = state; z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL; z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL; return z ^ (z >> 31); } return [Next(), Next(), Next(), Next()]; }
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
JsonElement ReadBinding(string id) => JsonDocument.Parse(File.ReadAllBytes(PathFor(id))).RootElement.Clone();
string PathFor(string id) => specs.Single(x => x.Id == id).Path;
void Emit(object payload)
{
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    File.WriteAllBytes(OutputPath, bytes); File.WriteAllBytes(SummaryPath, bytes);
}
object Early(string verdict, bool valid, bool bindingsValid, bool accepted, object bindingRows, object battery) => new
{
    schemaVersion = 1, phase = 577, phaseId = "phase577-chain-pack-execution",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid = valid,
    exactBindingsValid = bindingsValid, resourceAccepted = accepted, bindings = bindingRows, knownAnswerBattery = battery,
    upstream = (object?)null, prechecks = (object?)null, execution = (object?)null,
    tableDiagnostics = (object?)null, rawTraces = (object?)null,
    verdictKind = verdict, terminalStatus = "chain-pack-execution-" + verdict,
    workbenchRelativeLatticeUnitsOnly = true, establishesStationarity = false,
    establishesSamplingCorrectnessBeyondFrozenGates = false, establishesSpectralOrPhysicalQuantity = false,
    isProductionBenchmark = false, samplingPerformedUnderExplicitUserAuthorization = false,
    rawDirectionalSeriesRetained = false,
    phase548Or549TerminalChanged = false, phase570Or571Or572Reinterpreted = false,
    phase572ToleranceRelaxed = false, registeredBlindSeedTouched = false,
    protectedPhase554SeedsRead = false, registeredTargetChanged = false,
    directionCalledGaugeOrRedundant = false, quotientApplied = false, gaugeFixingApplied = false,
    measureNormalizationApplied = false, sourceOrModelSelected = false,
    phase561Opened = false, o4Discharged = false, phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};

sealed record Binding(string Id, string Path, string Hash);
sealed record ChainPlan(string TableId, int RawSeed, int ExecutionSeed, double InitialScale);
sealed record Diagnostics(double Rhat, double BulkEss, double TailEss);
sealed record InvariantMetrics(double TotalNormSquared, double ENormSquared, double WNormSquared, double ClosedNormSquared, double ClosedPerpNormSquared, double[] GramEigenvalues, double RankOneAlignment, double WithinClosedRankOneDistanceSquared, double WithinClosedRankOneRelativeDistance, double FullRankOneDistanceSquared);
sealed class Xoshiro(ulong[] state)
{
    private ulong s0 = state[0], s1 = state[1], s2 = state[2], s3 = state[3];
    public ulong Next() { ulong result = RotateLeft(s1 * 5, 7) * 9, t = s1 << 17; s2 ^= s0; s3 ^= s1; s1 ^= s2; s0 ^= s3; s2 ^= t; s3 = RotateLeft(s3, 45); return result; }
    private static ulong RotateLeft(ulong x, int k) => (x << k) | (x >> (64 - k));
}
