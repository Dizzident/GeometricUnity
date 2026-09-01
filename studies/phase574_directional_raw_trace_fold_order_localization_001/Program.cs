using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase574_directional_raw_trace_fold_order_localization_001";
const string ContractPath = Root + "/preregistration/phase574_directional_raw_trace_fold_order_localization_contract_v1.json";
const string OutputPath = Root + "/output/directional_raw_trace_fold_order_localization.json";
const string SummaryPath = Root + "/output/directional_raw_trace_fold_order_localization_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
bool exactBindingsValid = bindings.Length == 36
    && contract.GetProperty("requiredExactBindingCount").GetInt32() == 36
    && specs.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == 36
    && specs.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == 36
    && bindings.All(x => x.hashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input", "known-answer-battery-failed", "a38-upstream-gate-refused",
    "resource-refusal", "committed-chain-replay-not-bit-identical", "committed-diagnostics-not-reproduced",
    "raw-series-bit-identical-contradiction", "ordinary-rank-divergence-present", "disagreement-not-localized",
    "fold-order-flip-confirmed-source-unresolved", "fold-order-flip-confirmed-source-attributed",
];
string[] expectedMismatchKeys = contract.GetProperty("expectedMismatchKeys").EnumerateArray().Select(x => x.GetString()!).ToArray();
JsonElement resourceSpec = contract.GetProperty("resourceRefusal");
JsonElement replayRule = contract.GetProperty("replayRule");
JsonElement firewalls = contract.GetProperty("authorityFirewalls");
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("phase").GetInt32() == 574
    && contract.GetProperty("contractId").GetString() == "phase574-a38-directional-raw-trace-fold-order-localization-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministic").GetBoolean()
    && contract.GetProperty("retrospectiveKnownDataAudit").GetBoolean()
    && contract.GetProperty("newSamplingPerformed").GetBoolean() == false
    && contract.GetProperty("batteryRehearsal").GetProperty("performedOutsideRepositoryBeforeFreeze").GetBoolean()
    && contract.GetProperty("batteryRehearsal").GetProperty("upstreamNumericRowsRead").GetBoolean() == false
    && contract.GetProperty("expectedTableRowCount").GetInt32() == 36
    && contract.GetProperty("expectedMismatchCount").GetInt32() == 5
    && expectedMismatchKeys.Length == 5
    && taxonomy.SequenceEqual(expectedTaxonomy, StringComparer.Ordinal)
    && replayRule.GetProperty("deltaHRelativeTolerance").GetDouble() == 1e-12
    && replayRule.GetProperty("requireEveryDecisionMatch").GetBoolean()
    && replayRule.GetProperty("requireFinalPositionBitIdentical").GetBoolean()
    && replayRule.GetProperty("retainedPerChain").GetInt32() == 340
    && replayRule.GetProperty("rngUseRestrictedToCommittedReplay").GetBoolean()
    && replayRule.GetProperty("markovChainAdvancedBeyondCommittedReplay").GetBoolean() == false
    && replayRule.GetProperty("configurationsRetained").GetBoolean() == false
    && resourceSpec.GetProperty("estimatedForceEvaluations").GetInt64() == 21600
    && resourceSpec.GetProperty("maximumForceEvaluations").GetInt64() == 25000
    && resourceSpec.GetProperty("refuseBeforeAllocation").GetBoolean()
    && resourceSpec.GetProperty("noDenseHessianAllocated").GetBoolean()
    && resourceSpec.GetProperty("allocationMenu").EnumerateArray().All(x => x.GetProperty("shape").GetString() != "dof*dof")
    && contract.GetProperty("scope").GetProperty("closesOnlyPhase573DiagnosticQuestion").GetBoolean()
    && contract.GetProperty("scope").GetProperty("phase572ComparisonToleranceUnchanged").GetBoolean()
    && contract.GetProperty("scope").GetProperty("rawDirectionalSeriesRetained").GetBoolean()
    && firewalls.EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && exactBindingsValid;

// ---------------------------------------------------------------------------
// Known-answer battery: runs before any audited upstream numeric row is read.
// Fixtures were deterministically rehearsed outside the repository before the
// contract freeze; the rehearsal read no upstream rows and wrote no repo byte.
// ---------------------------------------------------------------------------
string SelectTerminal(bool invalid, bool batteryFailed, bool gateRefused, bool resourceRefused,
    bool replayFailed, bool reproductionFailed, bool contradiction, bool rankDivergence,
    bool notLocalized, bool sourceUnresolved)
{
    if (invalid) return taxonomy[0];
    if (batteryFailed) return taxonomy[1];
    if (gateRefused) return taxonomy[2];
    if (resourceRefused) return taxonomy[3];
    if (replayFailed) return taxonomy[4];
    if (reproductionFailed) return taxonomy[5];
    if (contradiction) return taxonomy[6];
    if (rankDivergence) return taxonomy[7];
    if (notLocalized) return taxonomy[8];
    if (sourceUnresolved) return taxonomy[9];
    return taxonomy[10];
}
var truthTable = new[]
{
    new { id = "invalid", actual = SelectTerminal(true, false, false, false, false, false, false, false, false, false), expected = taxonomy[0] },
    new { id = "battery", actual = SelectTerminal(false, true, false, false, false, false, false, false, false, false), expected = taxonomy[1] },
    new { id = "gate", actual = SelectTerminal(false, false, true, false, false, false, false, false, false, false), expected = taxonomy[2] },
    new { id = "resource", actual = SelectTerminal(false, false, false, true, false, false, false, false, false, false), expected = taxonomy[3] },
    new { id = "replay", actual = SelectTerminal(false, false, false, false, true, false, false, false, false, false), expected = taxonomy[4] },
    new { id = "reproduction", actual = SelectTerminal(false, false, false, false, false, true, false, false, false, false), expected = taxonomy[5] },
    new { id = "contradiction", actual = SelectTerminal(false, false, false, false, false, false, true, false, false, false), expected = taxonomy[6] },
    new { id = "rank-divergence", actual = SelectTerminal(false, false, false, false, false, false, false, true, false, false), expected = taxonomy[7] },
    new { id = "not-localized", actual = SelectTerminal(false, false, false, false, false, false, false, false, true, false), expected = taxonomy[8] },
    new { id = "source-unresolved", actual = SelectTerminal(false, false, false, false, false, false, false, false, false, true), expected = taxonomy[9] },
    new { id = "attributed", actual = SelectTerminal(false, false, false, false, false, false, false, false, false, false), expected = taxonomy[10] },
    new { id = "reproduction-precedes-contradiction", actual = SelectTerminal(false, false, false, false, false, true, true, false, false, false), expected = taxonomy[5] },
    new { id = "contradiction-precedes-rank", actual = SelectTerminal(false, false, false, false, false, false, true, true, false, false), expected = taxonomy[6] },
    new { id = "early-precedence", actual = SelectTerminal(true, true, true, true, true, true, true, true, true, true), expected = taxonomy[0] },
};
bool truthTablePassed = truthTable.All(x => x.actual == x.expected)
    && expectedTaxonomy.All(terminal => truthTable.Any(x => x.actual == terminal));

// Fold-flip fixture: an ulp-scale value change near a designed fold tie flips
// the folded-rank ordering while ordinary ranks, bulk ESS, tail ESS, and the
// ranked R-hat component stay bit-identical and only the folded component moves.
const int FixtureChainCount = 4, FixtureLength = 100;
var fixtureBase = new double[FixtureChainCount][];
for (int c = 0; c < FixtureChainCount; c++)
{
    fixtureBase[c] = new double[FixtureLength];
    for (int i = 0; i < FixtureLength; i++) fixtureBase[c][i] = StatelessNormal(c, i);
}
double fixtureBaseMedian = Median(fixtureBase.SelectMany(x => x).ToArray());
const double FoldOffset = 0.3;
double[][] FoldVariantRaw(double center, double epsilon)
{
    var variant = fixtureBase.Select(x => (double[])x.Clone()).ToArray();
    variant[0][20] = center + FoldOffset;
    variant[2][70] = center - FoldOffset + epsilon;
    return variant;
}
double foldCenter = Median(FoldVariantRaw(fixtureBaseMedian, 0.0).SelectMany(x => x).ToArray());
double[][] foldVariantA = FoldVariantRaw(foldCenter, -1e-13);
double[][] foldVariantB = FoldVariantRaw(foldCenter, +1e-13);
double[] foldPooledA = foldVariantA.SelectMany(x => x).ToArray();
double[] foldPooledB = foldVariantB.SelectMany(x => x).ToArray();
double foldMedianA = Median(foldPooledA), foldMedianB = Median(foldPooledB);
double[] foldRanksA = RankVector(foldPooledA), foldRanksB = RankVector(foldPooledB);
double[] foldedVectorA = RankVector(foldPooledA.Select(x => System.Math.Abs(x - foldMedianA)).ToArray());
double[] foldedVectorB = RankVector(foldPooledB.Select(x => System.Math.Abs(x - foldMedianB)).ToArray());
double nearestOtherFoldGap = foldPooledA.Select(x => System.Math.Abs(System.Math.Abs(x - foldMedianA) - FoldOffset))
    .Where(x => x > 1e-12).Min();
DiagnosticBundle foldDiagnosticsA = Diagnose(foldVariantA);
DiagnosticBundle foldDiagnosticsB = Diagnose(foldVariantB);
bool foldFlipFixturePassed =
    BitEqual(foldMedianA, foldMedianB) && BitEqual(foldMedianA, foldCenter)
    && foldRanksA.Zip(foldRanksB).All(pair => pair.First == pair.Second)
    && !foldedVectorA.Zip(foldedVectorB).All(pair => pair.First == pair.Second)
    && nearestOtherFoldGap > 1e-6
    && BitEqual(foldDiagnosticsA.BulkEss, foldDiagnosticsB.BulkEss)
    && BitEqual(foldDiagnosticsA.TailEss, foldDiagnosticsB.TailEss)
    && BitEqual(foldDiagnosticsA.RankedComponent, foldDiagnosticsB.RankedComponent)
    && foldDiagnosticsA.FoldedComponent != foldDiagnosticsB.FoldedComponent
    && foldDiagnosticsA.Rhat != foldDiagnosticsB.Rhat;

// Fold-stability fixture: a one-ulp change far from any fold tie leaves every
// diagnostic output bit-identical.
var stabilityVariant = fixtureBase.Select(x => (double[])x.Clone()).ToArray();
stabilityVariant[1][40] = System.Math.BitIncrement(stabilityVariant[1][40]);
DiagnosticBundle stabilityReference = Diagnose(fixtureBase);
DiagnosticBundle stabilityPerturbed = Diagnose(stabilityVariant);
bool foldStabilityFixturePassed = BitEqual(stabilityReference.Rhat, stabilityPerturbed.Rhat)
    && BitEqual(stabilityReference.BulkEss, stabilityPerturbed.BulkEss)
    && BitEqual(stabilityReference.TailEss, stabilityPerturbed.TailEss);

// Eigensolver-pair fixture: both ported Jacobi variants agree within 1e-12
// scaled on deterministic Gram matrices; bit differences are recorded, never
// asserted, because they are the phenomenon under audit.
int eigenBitDifferentCount = 0;
double eigenWorstScaledDifference = 0.0;
bool eigenPairFixturePassed = true;
for (int matrix = 0; matrix < 64 && eigenPairFixturePassed; matrix++)
{
    var rows = new double[84][];
    for (int r = 0; r < 84; r++)
        rows[r] = [StatelessNormal(100 + matrix, 3 * r), StatelessNormal(100 + matrix, 3 * r + 1), StatelessNormal(100 + matrix, 3 * r + 2)];
    if (matrix % 4 == 0)
        for (int r = 0; r < 84; r++)
        { rows[r][1] = 2.0 * rows[r][0] + 1e-9 * rows[r][1]; rows[r][2] = -rows[r][0] + 1e-9 * rows[r][2]; }
    double[,] gram = GramFromRows(rows);
    double[] eigenA = SymmetricEigenvalues3(gram);
    double[] eigenB = Symmetric3EigenvaluesByJacobi(gram).Reverse().ToArray();
    for (int i = 0; i < 3; i++)
    {
        double scaled = ScaledAbsoluteDifference(eigenA[i], eigenB[i]);
        eigenWorstScaledDifference = System.Math.Max(eigenWorstScaledDifference, scaled);
        eigenPairFixturePassed &= scaled <= 1e-12;
        if (!BitEqual(eigenA[i], eigenB[i])) eigenBitDifferentCount++;
    }
}

byte[] checksumFixture = Encoding.UTF8.GetBytes("{\"phase\":574,\"fixture\":\"checksum\"}");
byte[] tamperedFixture = (byte[])checksumFixture.Clone();
tamperedFixture[^2] ^= 1;
bool checksumTamperDetected = Convert.ToHexString(SHA256.HashData(checksumFixture))
    != Convert.ToHexString(SHA256.HashData(tamperedFixture));
bool knownAnswerPassed = truthTablePassed && foldFlipFixturePassed && foldStabilityFixturePassed
    && eigenPairFixturePassed && checksumTamperDetected;
var knownAnswerBattery = new
{
    auditedNumericDataParsedBeforeBattery = false,
    rehearsedOutsideRepositoryBeforeFreeze = true,
    foldFlip = new
    {
        nearestOtherFoldGap,
        ordinaryRanksUnchanged = foldRanksA.Zip(foldRanksB).All(pair => pair.First == pair.Second),
        foldOrderChanged = !foldedVectorA.Zip(foldedVectorB).All(pair => pair.First == pair.Second),
        bulkEssBitIdentical = BitEqual(foldDiagnosticsA.BulkEss, foldDiagnosticsB.BulkEss),
        tailEssBitIdentical = BitEqual(foldDiagnosticsA.TailEss, foldDiagnosticsB.TailEss),
        rankedComponentBitIdentical = BitEqual(foldDiagnosticsA.RankedComponent, foldDiagnosticsB.RankedComponent),
        foldedComponentChanged = foldDiagnosticsA.FoldedComponent != foldDiagnosticsB.FoldedComponent,
        rhatChanged = foldDiagnosticsA.Rhat != foldDiagnosticsB.Rhat,
        observedScaledRhatChange = ScaledAbsoluteDifference(foldDiagnosticsA.Rhat, foldDiagnosticsB.Rhat),
        passed = foldFlipFixturePassed,
    },
    foldStability = new { passed = foldStabilityFixturePassed },
    eigensolverPair = new { worstScaledDifference = eigenWorstScaledDifference, bitDifferentCount = eigenBitDifferentCount, passed = eigenPairFixturePassed },
    classificationTruthTable = new { rows = truthTable, everyTerminalReached = expectedTaxonomy.All(t => truthTable.Any(x => x.actual == t)), passed = truthTablePassed },
    checksumTamperDetected, passed = knownAnswerPassed,
};

if (!contractValid || !knownAnswerPassed)
{
    string early = !contractValid ? taxonomy[0] : taxonomy[1];
    Emit(Early(early, contractValid, exactBindingsValid, false, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase574 verdict: {early}");
    return;
}

// Only now parse the exact-bound upstream scientific records.
JsonElement p548 = ReadBinding("phase548-summary");
JsonElement p570 = ReadBinding("phase570-full");
JsonElement p572 = ReadBinding("phase572-full");
JsonElement p573 = ReadBinding("phase573-full");
JsonElement requiredVerdicts = contract.GetProperty("requiredUpstreamVerdicts");
bool upstreamGateOpen = p548.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase548").GetString()
    && p570.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase570").GetString()
    && p572.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase572").GetString()
    && p573.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase573").GetString()
    && p573.GetProperty("comparison").GetProperty("mismatchKeys").EnumerateArray()
        .Select(x => x.GetString()!).SequenceEqual(expectedMismatchKeys, StringComparer.Ordinal);
if (!upstreamGateOpen)
{
    Emit(Early(taxonomy[2], true, true, false, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase574 verdict: {taxonomy[2]}");
    return;
}

JsonElement phase548Contract = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase548-contract"))).RootElement.Clone();
JsonElement target = phase548Contract.GetProperty("target");
JsonElement defaults = phase548Contract.GetProperty("defaultConfiguration");
int extent = target.GetProperty("extent").GetInt32();
double stepSize = defaults.GetProperty("stepSize").GetDouble();
int leapfrogSteps = defaults.GetProperty("leapfrogSteps").GetInt32();
int warmupPerChain = defaults.GetProperty("warmupPerChain").GetInt32();
int retainedPerChain = defaults.GetProperty("retainedPerChain").GetInt32();
int trajectoriesPerChain = defaults.GetProperty("trajectoriesPerChain").GetInt32();
double divergenceThreshold = defaults.GetProperty("divergenceAbsoluteDeltaH").GetDouble();
var chainPlan = phase548Contract.GetProperty("seedTables").EnumerateArray().SelectMany(table =>
{
    string tableId = table.GetProperty("id").GetString()!;
    int offset = table.GetProperty("seedOffset").GetInt32();
    int[] seeds = table.GetProperty("seeds").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    double[] scales = table.GetProperty("initialScales").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    return seeds.Select((seed, i) => new ChainPlan(tableId, seed, seed + offset, scales[i]));
}).ToArray();

int declaredDof = target.GetProperty("degreesOfFreedom").GetInt32();
long estimatedForceEvaluations = checked((long)chainPlan.Length * trajectoriesPerChain * (leapfrogSteps + 1));
int derivedEdgeCount = declaredDof % 3 == 0 ? declaredDof / 3 : 0;
var derivedAllocationMenu = new[]
{
    new AllocationRow("mesh-topology", "edge*256", checked((long)derivedEdgeCount * 256)),
    new AllocationRow("side-a-scalar-bases", "84*edge*8", checked(84L * derivedEdgeCount * sizeof(double))),
    new AllocationRow("side-b-direction-construction", "(243+12+64+252)*dof*8", checked(571L * declaredDof * sizeof(double))),
    new AllocationRow("replay-vectors", "16*dof*8", checked(16L * declaredDof * sizeof(double))),
    new AllocationRow("evaluator-working-reserve", "128*dof*8", checked(128L * declaredDof * sizeof(double))),
    new AllocationRow("retained-raw-series", "2*6*18*340*8", checked(2L * chainPlan.Length * 18 * retainedPerChain * sizeof(double))),
    new AllocationRow("position-gram-log", "2*2040*9*8", checked(2L * chainPlan.Length * retainedPerChain * 9 * sizeof(double))),
    new AllocationRow("telemetry-checkpoint", "6*(400*32+dof*8)", checked((long)chainPlan.Length * (trajectoriesPerChain * 32L + declaredDof * sizeof(double)))),
    new AllocationRow("diagnostics-scratch", "8*6*18*340*8", checked(8L * chainPlan.Length * 18 * retainedPerChain * sizeof(double))),
    new AllocationRow("serialization-reserve", "64*1024*1024", 64L * 1024 * 1024),
    new AllocationRow("object-overhead-reserve", "64*1024*1024", 64L * 1024 * 1024),
    new AllocationRow("runtime-reserve", "128*1024*1024", 128L * 1024 * 1024),
};
long derivedPeakBytes = checked(derivedAllocationMenu.Sum(x => x.Bytes));
var declaredAllocationMenu = resourceSpec.GetProperty("allocationMenu").EnumerateArray().Select(x => new AllocationRow(
    x.GetProperty("id").GetString()!, x.GetProperty("shape").GetString()!, x.GetProperty("bytes").GetInt64())).ToArray();
bool resourceAccepted = chainPlan.Length == 6 && trajectoriesPerChain == 400 && leapfrogSteps == 8
    && extent == 3 && declaredDof == 3645 && derivedEdgeCount == 1215
    && retainedPerChain == 340 && warmupPerChain == 60
    && estimatedForceEvaluations == 21600
    && estimatedForceEvaluations <= resourceSpec.GetProperty("maximumForceEvaluations").GetInt64()
    && declaredAllocationMenu.SequenceEqual(derivedAllocationMenu)
    && derivedPeakBytes == resourceSpec.GetProperty("derivedPeakBytes").GetInt64()
    && derivedPeakBytes <= resourceSpec.GetProperty("maximumPeakBytes").GetInt64();
if (!resourceAccepted)
{
    Emit(Early(taxonomy[3], true, true, false, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase574 verdict: {taxonomy[3]}");
    return;
}

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
var mass = new CpuMassMatrix(mesh, algebra);
int dimG = algebra.Dimension, edgeCount = mesh.EdgeCount, dof = edgeCount * dimG;
var thetaZero = new double[mesh.VertexCount * dimG];
(double Action, double[] Gradient) Evaluate(double[] omega)
{
    var joint = op.ComputeJointGradient(omega, thetaZero, mass);
    return (joint.Objective, joint.GradOmega);
}

// Observable path A: the phase570 v7 scalar edge-space basis, ported verbatim.
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
        int wrapped = ((difference % extent) + extent) % extent;
        candidate[edge] = wrapped == extent - 1 ? -1 : wrapped;
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

// Observable path B: the phase572 full-dof sign-canonicalized directions,
// ported verbatim including candidate order and the acceptance-order grouping.
Subspaces subspaces = BuildSubspaces(mesh, dimG, extent);
bool basesValid = eBasis.Count == 80 && wBasis.Count == 4 && cBasis.Length == 84
    && subspaces.Exact.Length == 240 && subspaces.Winding.Length == 12 && subspaces.Closed.Length == 252;
if (!basesValid)
{
    Emit(Early(taxonomy[0], true, true, true, false, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase574 verdict: {taxonomy[0]}");
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
double replayTolerance = replayRule.GetProperty("deltaHRelativeTolerance").GetDouble();
var sideASeriesByChain = new Dictionary<string, Dictionary<string, double[]>>(StringComparer.Ordinal);
var sideBSeriesByChain = new Dictionary<string, Dictionary<string, double[]>>(StringComparer.Ordinal);
var gramLogAByChain = new Dictionary<string, double[][]>(StringComparer.Ordinal);
var gramLogBByChain = new Dictionary<string, double[][]>(StringComparer.Ordinal);
var replayRows = new List<object>();
bool replayBitIdentical = true;
foreach (ChainPlan plan in chainPlan)
{
    string chainId = $"{plan.TableId}-{plan.RawSeed}";
    using var telemetry = JsonDocument.Parse(File.ReadAllBytes(PathFor($"phase548-telemetry-{Suffix(chainId)}")));
    var recorded = telemetry.RootElement.GetProperty("rows").EnumerateArray().Select(x => new
    {
        Accepted = x.GetProperty("accepted").GetBoolean(),
        DeltaH = x.GetProperty("deltaH").GetDouble(),
    }).ToArray();
    var rng = new Xoshiro(ExpandSeed((ulong)plan.ExecutionSeed));
    var position = new double[dof];
    for (int i = 0; i < dof; i++) position[i] = plan.InitialScale * Gauss(rng);
    (double Action, double[] Gradient) current = Evaluate(position);
    var seriesA = seriesNames.ToDictionary(x => x, _ => new List<double>(), StringComparer.Ordinal);
    var seriesB = seriesNames.ToDictionary(x => x, _ => new List<double>(), StringComparer.Ordinal);
    var gramLogA = new List<double[]>();
    var gramLogB = new List<double[]>();
    double worstDelta = 0.0;
    bool decisionsMatch = true;
    int accepted = 0;
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
            if (!double.IsFinite(action) || !gradient.All(double.IsFinite) || !q.All(double.IsFinite)) { finite = false; break; }
            double kick = leap + 1 == leapfrogSteps ? 0.5 * stepSize : stepSize;
            for (int i = 0; i < dof; i++) p[i] -= kick * gradient[i];
        }
        finite &= p.All(double.IsFinite);
        double finalHamiltonian = finite ? action + 0.5 * Dot(p, p) : double.NaN;
        double deltaH = finalHamiltonian - initialHamiltonian;
        bool divergent = !finite || !double.IsFinite(deltaH) || System.Math.Abs(deltaH) > divergenceThreshold;
        bool accept = finite && !divergent && logUniform <= System.Math.Min(0.0, -deltaH);
        if (accept) { position = q; current = (action, gradient); accepted++; }
        double scale = System.Math.Max(1.0, System.Math.Abs(recorded[trajectory].DeltaH));
        worstDelta = System.Math.Max(worstDelta, System.Math.Abs(deltaH - recorded[trajectory].DeltaH) / scale);
        decisionsMatch &= accept == recorded[trajectory].Accepted;

        if (trajectory >= warmupPerChain)
        {
            double[] movement = Subtract(position, before);
            InvariantMetrics stateA = MeasureSideA(position);
            InvariantMetrics moveA = MeasureSideA(movement);
            seriesA["actionDensity"].Add(current.Action / dof);
            seriesA["forceNormSquared"].Add(Dot(current.Gradient, current.Gradient));
            seriesA["configurationNormSquared"].Add(stateA.TotalNormSquared);
            seriesA["eNormSquared"].Add(stateA.ENormSquared);
            seriesA["wNormSquared"].Add(stateA.WNormSquared);
            seriesA["closedNormSquared"].Add(stateA.ClosedNormSquared);
            seriesA["closedPerpNormSquared"].Add(stateA.ClosedPerpNormSquared);
            seriesA["closedGramLargest"].Add(stateA.GramEigenvalues[0]);
            seriesA["closedGramMiddle"].Add(stateA.GramEigenvalues[1]);
            seriesA["closedGramSmallest"].Add(stateA.GramEigenvalues[2]);
            seriesA["closedRankOneAlignment"].Add(stateA.RankOneAlignment);
            seriesA["withinClosedRankOneDistanceSquared"].Add(stateA.WithinClosedRankOneDistanceSquared);
            seriesA["withinClosedRankOneRelativeDistance"].Add(stateA.WithinClosedRankOneRelativeDistance);
            seriesA["fullRankOneDistanceSquared"].Add(stateA.FullRankOneDistanceSquared);
            seriesA["eMovementSquared"].Add(moveA.ENormSquared);
            seriesA["wMovementSquared"].Add(moveA.WNormSquared);
            seriesA["closedMovementSquared"].Add(moveA.ClosedNormSquared);
            seriesA["closedPerpMovementSquared"].Add(moveA.ClosedPerpNormSquared);
            gramLogA.Add(stateA.GramFlat);

            ProjectionMetric stateB = ProjectMetric(position, subspaces, dimG);
            ProjectionMetric moveB = ProjectMetric(movement, subspaces, dimG);
            double[] eigenDescending = stateB.ClosedGramEigenvalues.Reverse().ToArray();
            double withinClosed = eigenDescending[1] + eigenDescending[2];
            seriesB["actionDensity"].Add(current.Action / dof);
            seriesB["forceNormSquared"].Add(NormSquared(current.Gradient));
            seriesB["configurationNormSquared"].Add(stateB.TotalNormSquared);
            seriesB["eNormSquared"].Add(stateB.ENormSquared);
            seriesB["wNormSquared"].Add(stateB.WNormSquared);
            seriesB["closedNormSquared"].Add(stateB.CNormSquared);
            seriesB["closedPerpNormSquared"].Add(stateB.CperpNormSquared);
            seriesB["closedGramLargest"].Add(eigenDescending[0]);
            seriesB["closedGramMiddle"].Add(eigenDescending[1]);
            seriesB["closedGramSmallest"].Add(eigenDescending[2]);
            seriesB["closedRankOneAlignment"].Add(stateB.ClosedRankOneAlignment);
            seriesB["withinClosedRankOneDistanceSquared"].Add(withinClosed);
            seriesB["withinClosedRankOneRelativeDistance"].Add(stateB.CNormSquared > 0.0 ? withinClosed / stateB.CNormSquared : 0.0);
            seriesB["fullRankOneDistanceSquared"].Add(stateB.CperpNormSquared + withinClosed);
            seriesB["eMovementSquared"].Add(moveB.ENormSquared);
            seriesB["wMovementSquared"].Add(moveB.WNormSquared);
            seriesB["closedMovementSquared"].Add(moveB.CNormSquared);
            seriesB["closedPerpMovementSquared"].Add(moveB.CperpNormSquared);
            gramLogB.Add(stateB.GramFlat);
        }
    }
    using var checkpoint = JsonDocument.Parse(File.ReadAllBytes(PathFor($"phase548-checkpoint-{Suffix(chainId)}")));
    double[] stored = checkpoint.RootElement.GetProperty("payload").GetProperty("position").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    bool finalBitsMatch = stored.Length == position.Length && stored.Zip(position).All(x => BitEqual(x.First, x.Second));
    bool rowPassed = recorded.Length == trajectoriesPerChain && decisionsMatch && finalBitsMatch
        && worstDelta <= replayTolerance
        && seriesA.All(x => x.Value.Count == retainedPerChain) && seriesB.All(x => x.Value.Count == retainedPerChain);
    replayBitIdentical &= rowPassed;
    sideASeriesByChain[chainId] = seriesA.ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.Ordinal);
    sideBSeriesByChain[chainId] = seriesB.ToDictionary(x => x.Key, x => x.Value.ToArray(), StringComparer.Ordinal);
    gramLogAByChain[chainId] = gramLogA.ToArray();
    gramLogBByChain[chainId] = gramLogB.ToArray();
    replayRows.Add(new { chainId, decisionsMatch, finalPositionBitIdentical = finalBitsMatch, worstRelativeDeltaHDeviation = worstDelta, acceptanceRate = (double)accepted / trajectoriesPerChain, retainedDraws = retainedPerChain, passed = rowPassed });
}
if (!replayBitIdentical)
{
    Emit(Early(taxonomy[4], true, true, true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase574 verdict: {taxonomy[4]}");
    return;
}

// Reproduce each side's committed diagnostics bit-for-bit with the shared
// kernel (phase573 proved kernel parity on identical inputs).
var committedA = p570.GetProperty("tableDiagnostics").GetProperty("rows").EnumerateArray()
    .ToDictionary(x => $"{x.GetProperty("table").GetString()}|{x.GetProperty("series").GetString()}", x => x, StringComparer.Ordinal);
var committedB = p572.GetProperty("directionalAdjudication").GetProperty("tableRows").EnumerateArray()
    .ToDictionary(x => $"{x.GetProperty("table").GetString()}|{x.GetProperty("series").GetString()}", x => x, StringComparer.Ordinal);
string[] tableIds = chainPlan.Select(x => x.TableId).Distinct().ToArray();
var comparisonRows = new List<object>();
var attributionRows = new List<object>();
bool reproductionPassed = committedA.Count == 36 && committedB.Count == 36;
bool anyContradiction = false, anyRankDivergence = false, anyMismatchWithoutFoldFlip = false;
var maskedFoldFlipRowKeys = new List<string>();
int rawDifferingRowCount = 0;
foreach (string tableId in tableIds)
{
    ChainPlan[] tableChains = chainPlan.Where(x => x.TableId == tableId).ToArray();
    foreach (string name in seriesNames)
    {
        string rowKey = $"{tableId}|{name}";
        bool isMismatchRow = expectedMismatchKeys.Contains(rowKey, StringComparer.Ordinal);
        double[][] chainsA = tableChains.Select(x => sideASeriesByChain[$"{x.TableId}-{x.RawSeed}"][name]).ToArray();
        double[][] chainsB = tableChains.Select(x => sideBSeriesByChain[$"{x.TableId}-{x.RawSeed}"][name]).ToArray();
        double[] pooledA = chainsA.SelectMany(x => x).ToArray();
        double[] pooledB = chainsB.SelectMany(x => x).ToArray();

        int differingCount = 0, firstDivergence = -1;
        double maximumScaledRawDifference = 0.0;
        for (int i = 0; i < pooledA.Length; i++)
        {
            if (BitEqual(pooledA[i], pooledB[i])) continue;
            differingCount++;
            if (firstDivergence < 0) firstDivergence = i;
            maximumScaledRawDifference = System.Math.Max(maximumScaledRawDifference, ScaledAbsoluteDifference(pooledA[i], pooledB[i]));
        }
        bool rawBitIdentical = differingCount == 0;
        if (!rawBitIdentical) rawDifferingRowCount++;

        double medianA = Median(pooledA), medianB = Median(pooledB);
        double[] ranksA = RankVector(pooledA), ranksB = RankVector(pooledB);
        bool ordinaryRanksEqual = ranksA.Zip(ranksB).All(x => x.First == x.Second);
        double[] foldedA = RankVector(pooledA.Select(x => System.Math.Abs(x - medianA)).ToArray());
        double[] foldedB = RankVector(pooledB.Select(x => System.Math.Abs(x - medianB)).ToArray());
        bool foldedRanksEqual = foldedA.Zip(foldedB).All(x => x.First == x.Second);
        int foldedRankDifferingCount = foldedA.Zip(foldedB).Count(x => x.First != x.Second);
        double[] orderedA = pooledA.Order().ToArray(), orderedB = pooledB.Order().ToArray();
        double q05A = orderedA[(int)System.Math.Floor(0.05 * (orderedA.Length - 1))];
        double q95A = orderedA[(int)System.Math.Ceiling(0.95 * (orderedA.Length - 1))];
        double q05B = orderedB[(int)System.Math.Floor(0.05 * (orderedB.Length - 1))];
        double q95B = orderedB[(int)System.Math.Ceiling(0.95 * (orderedB.Length - 1))];
        bool lowerIndicatorsEqual = pooledA.Zip(pooledB).All(x => (x.First <= q05A) == (x.Second <= q05B));
        bool upperIndicatorsEqual = pooledA.Zip(pooledB).All(x => (x.First >= q95A) == (x.Second >= q95B));

        DiagnosticBundle sideA = Diagnose(chainsA);
        DiagnosticBundle sideB = Diagnose(chainsB);
        JsonElement rowA = committedA[rowKey];
        JsonElement rowB = committedB[rowKey];
        bool sideAReproduces = BitEqual(sideA.Rhat, rowA.GetProperty("rhat").GetDouble())
            && BitEqual(sideA.BulkEss, rowA.GetProperty("bulkEss").GetDouble())
            && BitEqual(sideA.TailEss, rowA.GetProperty("tailEss").GetDouble())
            && (sideA.Rhat <= 1.01 && sideA.BulkEss >= 100.0 && sideA.TailEss >= 100.0) == rowA.GetProperty("passed").GetBoolean();
        bool sideBReproduces = BitEqual(sideB.Rhat, rowB.GetProperty("rhat").GetDouble())
            && BitEqual(sideB.BulkEss, rowB.GetProperty("bulkEss").GetDouble())
            && BitEqual(sideB.TailEss, rowB.GetProperty("tailEss").GetDouble())
            && (sideB.Rhat <= 1.01 && sideB.BulkEss >= 100.0 && sideB.TailEss >= 100.0) == rowB.GetProperty("passed").GetBoolean();
        reproductionPassed &= sideAReproduces && sideBReproduces;

        anyContradiction |= isMismatchRow && rawBitIdentical;
        anyRankDivergence |= !ordinaryRanksEqual;
        anyMismatchWithoutFoldFlip |= isMismatchRow && foldedRanksEqual;
        if (!isMismatchRow && !foldedRanksEqual) maskedFoldFlipRowKeys.Add(rowKey);

        if (isMismatchRow && !rawBitIdentical)
        {
            string attributionStage;
            bool gramSeries = name.StartsWith("closedGram", StringComparison.Ordinal);
            if (!gramSeries) attributionStage = "projection-accumulation";
            else
            {
                int chainIndex = firstDivergence / retainedPerChain, drawIndex = firstDivergence % retainedPerChain;
                string chainId = $"{tableChains[chainIndex].TableId}-{tableChains[chainIndex].RawSeed}";
                double[] gramA = gramLogAByChain[chainId][drawIndex];
                double[] gramB = gramLogBByChain[chainId][drawIndex];
                bool gramBitEqual = gramA.Zip(gramB).All(x => BitEqual(x.First, x.Second));
                attributionStage = gramBitEqual ? "eigenvalue-computation" : "gram-matrix";
            }
            attributionRows.Add(new
            {
                rowKey, series = name, table = tableId, attributionStage,
                firstDivergencePooledIndex = firstDivergence, differingCount, maximumScaledRawDifference,
                foldedRankDifferingCount,
            });
        }
        comparisonRows.Add(new
        {
            table = tableId, series = name, committedMismatchRow = isMismatchRow,
            rawBitIdentical, differingCount, firstDivergencePooledIndex = firstDivergence < 0 ? (int?)null : firstDivergence,
            maximumScaledRawDifference, ordinaryRanksEqual,
            medianBitIdentical = BitEqual(medianA, medianB), foldedRanksEqual, foldedRankDifferingCount,
            lowerIndicatorsEqual, upperIndicatorsEqual,
            sideA = new { rhat = sideA.Rhat, bulkEss = sideA.BulkEss, tailEss = sideA.TailEss, rankedComponent = sideA.RankedComponent, foldedComponent = sideA.FoldedComponent },
            sideB = new { rhat = sideB.Rhat, bulkEss = sideB.BulkEss, tailEss = sideB.TailEss, rankedComponent = sideB.RankedComponent, foldedComponent = sideB.FoldedComponent },
            sideAReproducesCommitted = sideAReproduces, sideBReproducesCommitted = sideBReproduces,
        });
    }
}
bool allMismatchRowsAttributed = attributionRows.Count == 5;
bool sourceUnresolved = !anyContradiction && !anyRankDivergence && !anyMismatchWithoutFoldFlip && !allMismatchRowsAttributed;
string verdict = SelectTerminal(false, false, false, false, false,
    !reproductionPassed, anyContradiction, anyRankDivergence, anyMismatchWithoutFoldFlip, sourceUnresolved);
var hFold = new
{
    clauseARawSeriesDifferInEveryMismatchRow = !anyContradiction && comparisonRows.Count == 36,
    clauseBOrdinaryRanksAgreeOnAllRows = !anyRankDivergence,
    clauseCFoldOrderDiffersInEveryMismatchRow = !anyMismatchWithoutFoldFlip,
    clauseDCommittedDiagnosticsReproduceBitForBit = reproductionPassed,
    maskedFoldFlipRowKeys,
    rawDifferingRowCount,
    confirmed = reproductionPassed && !anyContradiction && !anyRankDivergence && !anyMismatchWithoutFoldFlip,
};

var rawTraces = new
{
    sideA = sideASeriesByChain.ToDictionary(chain => chain.Key, chain => chain.Value, StringComparer.Ordinal),
    sideB = sideBSeriesByChain.ToDictionary(chain => chain.Key, chain => chain.Value, StringComparer.Ordinal),
};
bool rawTracesFinite = sideASeriesByChain.Values.Concat(sideBSeriesByChain.Values)
    .SelectMany(chain => chain.Values).SelectMany(x => x).All(double.IsFinite);
if (!rawTracesFinite)
{
    Emit(Early(taxonomy[0], true, true, true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase574 verdict: {taxonomy[0]}");
    return;
}

var result = new
{
    schemaVersion = 1, phase = 574, phaseId = "phase574-directional-raw-trace-fold-order-localization",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath),
    contractValid = true, exactBindingsValid = true, resourceAccepted = true, bindings, knownAnswerBattery,
    upstream = new
    {
        gatePassed = true,
        phase548Verdict = p548.GetProperty("verdictKind").GetString(),
        phase570Verdict = p570.GetProperty("verdictKind").GetString(),
        phase572Verdict = p572.GetProperty("verdictKind").GetString(),
        phase573Verdict = p573.GetProperty("verdictKind").GetString(),
        committedMismatchKeysMatched = true,
    },
    observablePaths = new
    {
        sideASource = "phase570-program-v7 scalar edge-space basis and 40-sweep descending unclamped Jacobi",
        sideBSource = "phase572-program full-dof sign-canonicalized directions and 32-sweep ascending zero-clamped Jacobi",
        sharedEstimatorKernel = "single kernel ported from phase570-program-v7; phase573 proved bit-for-bit kernel parity on identical inputs",
        positionStreamSharedAcrossSides = true,
    },
    replay = new { rows = replayRows, bitIdentical = true, newSamplingPerformed = false, reExecutesAlreadyCommittedTrajectories = true },
    resource = new
    {
        forceEvaluationFormula = "6*400*(8+1)", estimatedForceEvaluations,
        maximumForceEvaluations = resourceSpec.GetProperty("maximumForceEvaluations").GetInt64(),
        derivedPeakBytes, maximumPeakBytes = resourceSpec.GetProperty("maximumPeakBytes").GetInt64(),
        allocationMenu = derivedAllocationMenu, allocationMenuMatches = true,
        forbiddenAllocationShape = "dof*dof", noDenseHessianAllocated = true,
    },
    reproduction = new
    {
        rule = "each side's committed R-hat, bulk ESS, tail ESS, and pass flag reproduce bit-for-bit from its own recovered raw series through the shared kernel",
        committedSideARowCount = committedA.Count, committedSideBRowCount = committedB.Count,
        passed = reproductionPassed,
    },
    comparison = new
    {
        rowCount = comparisonRows.Count,
        committedMismatchKeys = expectedMismatchKeys,
        rawDifferingRowCount,
        rows = comparisonRows,
    },
    hFold,
    attribution = new
    {
        stages = new[] { "projection-accumulation", "gram-matrix", "eigenvalue-computation" },
        rows = attributionRows,
        allMismatchRowsAttributed,
    },
    rawTraces,
    verdictKind = verdict,
    terminalStatus = "directional-raw-trace-fold-order-localization-" + verdict,
    closesOnlyPhase573DiagnosticQuestion = true,
    phase570Or571Or572Reinterpreted = false,
    phase572ToleranceRelaxed = false,
    phase571LeverConfirmed = false,
    prospectivePackPlanningOpened = false,
    newSamplingPerformed = false,
    markovChainAdvancedBeyondCommittedReplay = false,
    configurationsRetained = false,
    rawDirectionalSeriesRetained = true,
    rngUsed = true, replayRngAllocated = true, rngUseRestrictedToCommittedReplay = true,
    phase548Or549TerminalChanged = false, registeredBlindSeedTouched = false,
    protectedPhase554SeedsRead = false, registeredTargetChanged = false,
    directionCalledGaugeOrRedundant = false, quotientApplied = false,
    gaugeFixingApplied = false, measureNormalizationApplied = false, sourceOrModelSelected = false,
    phase561Opened = false, o4Discharged = false, phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};
Emit(result);
Console.WriteLine($"Phase574 verdict: {verdict}");
Console.WriteLine($"reproductionPassed={reproductionPassed}");
Console.WriteLine($"rawDifferingRowCount={rawDifferingRowCount}");
Console.WriteLine($"hFoldConfirmed={hFold.confirmed}");
Console.WriteLine($"promotedPhysicalMassClaimCount=0");

// --- Observable path A (phase570 v7 verbatim port) ---
InvariantMetrics MeasureSideA(double[] position)
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
    double[,] gram = GramFromRows(coefficientRows);
    double[] eigen = SymmetricEigenvalues3(gram);
    double trace = System.Math.Max(0.0, eigen.Sum());
    double distanceSquared = System.Math.Max(0.0, eigen[1] + eigen[2]);
    double closedPerp = System.Math.Max(0.0, total - closed);
    return new InvariantMetrics(total, eNorm, wNorm, closed, System.Math.Max(0.0, total - closed), eigen,
        trace > 0.0 ? eigen[0] / trace : 1.0, distanceSquared, trace > 0.0 ? distanceSquared / trace : 0.0,
        closedPerp + distanceSquared,
        [gram[0, 0], gram[0, 1], gram[0, 2], gram[1, 0], gram[1, 1], gram[1, 2], gram[2, 0], gram[2, 1], gram[2, 2]]);
}

// --- Observable path B (phase572 verbatim port) ---
static Subspaces BuildSubspaces(SimplicialMesh mesh, int dimG, int extent)
{
    int edgeCount = mesh.EdgeCount;
    int dof = edgeCount * dimG;
    var exactCandidates = new List<Direction>();
    for (int vertex = 0; vertex < mesh.VertexCount; vertex++)
        for (int algebra = 0; algebra < dimG; algebra++)
        {
            var vector = new double[dof];
            for (int edge = 0; edge < edgeCount; edge++)
                vector[edge * dimG + algebra] =
                    (mesh.Edges[edge][1] == vertex ? 1.0 : 0.0)
                    - (mesh.Edges[edge][0] == vertex ? 1.0 : 0.0);
            exactCandidates.Add(new Direction($"E-v{vertex:D3}-a{algebra}", "exact", vector));
        }
    Direction[] exact = OrthonormalizeDirections(exactCandidates, []);
    var windingCandidates = new List<Direction>();
    for (int axis = 0; axis < 4; axis++)
        for (int algebra = 0; algebra < dimG; algebra++)
        {
            var vector = new double[dof];
            for (int edge = 0; edge < edgeCount; edge++)
            {
                ReadOnlySpan<double> c0 = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
                ReadOnlySpan<double> c1 = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
                int difference = (int)System.Math.Round(c1[axis] - c0[axis]);
                int wrapped = ((difference % extent) + extent) % extent;
                vector[edge * dimG + algebra] = wrapped == extent - 1 ? -1.0 : wrapped;
            }
            windingCandidates.Add(new Direction($"W-axis{axis}-a{algebra}", "winding", vector));
        }
    Direction[] winding = OrthonormalizeDirections(windingCandidates, exact);
    Direction[] closed = [.. exact, .. winding];
    return new Subspaces(exact, winding, closed);
}
static Direction[] OrthonormalizeDirections(IEnumerable<Direction> candidates, IEnumerable<Direction> prefix)
{
    var basis = prefix.Select(row => row.Vector).ToList();
    var accepted = new List<Direction>();
    foreach (Direction candidateRow in candidates)
    {
        double[] candidate = (double[])candidateRow.Vector.Clone();
        for (int pass = 0; pass < 2; pass++)
            foreach (double[] existing in basis)
            {
                double coefficient = Dot(candidate, existing);
                for (int i = 0; i < candidate.Length; i++) candidate[i] -= coefficient * existing[i];
            }
        double norm = System.Math.Sqrt(NormSquared(candidate));
        if (norm <= 1e-10) continue;
        for (int i = 0; i < candidate.Length; i++) candidate[i] /= norm;
        int first = Array.FindIndex(candidate, value => System.Math.Abs(value) > 1e-14);
        if (first >= 0 && candidate[first] < 0)
            for (int i = 0; i < candidate.Length; i++) candidate[i] = -candidate[i];
        basis.Add(candidate);
        accepted.Add(candidateRow with { Vector = candidate });
    }
    return [.. accepted];
}
static ProjectionMetric ProjectMetric(double[] vector, Subspaces subspaces, int dimG)
{
    double total = NormSquared(vector);
    double eSquared = subspaces.Exact.Sum(direction =>
    {
        double coefficient = Dot(vector, direction.Vector);
        return coefficient * coefficient;
    });
    double wSquared = subspaces.Winding.Sum(direction =>
    {
        double coefficient = Dot(vector, direction.Vector);
        return coefficient * coefficient;
    });
    double cSquared = eSquared + wSquared;
    double cPerpSquared = System.Math.Max(0.0, total - cSquared);
    var gram = new double[3, 3];
    int scalarClosedDimension = subspaces.Closed.Length / dimG;
    for (int scalar = 0; scalar < scalarClosedDimension; scalar++)
    {
        var coefficients = new double[3];
        for (int algebra = 0; algebra < dimG; algebra++)
            coefficients[algebra] = Dot(vector, subspaces.Closed[scalar * dimG + algebra].Vector);
        for (int a = 0; a < dimG; a++)
            for (int b = 0; b < dimG; b++)
                gram[a, b] += coefficients[a] * coefficients[b];
    }
    double[] eigenvalues = Symmetric3EigenvaluesByJacobi(gram);
    double trace = eigenvalues.Sum();
    double alignment = trace > 0.0 ? eigenvalues[^1] / trace : 0.0;
    return new ProjectionMetric(total, eSquared, wSquared, cSquared, cPerpSquared, eigenvalues, alignment,
        [gram[0, 0], gram[0, 1], gram[0, 2], gram[1, 0], gram[1, 1], gram[1, 2], gram[2, 0], gram[2, 1], gram[2, 2]]);
}
static double[] Symmetric3EigenvaluesByJacobi(double[,] source)
{
    var matrix = (double[,])source.Clone();
    for (int sweep = 0; sweep < 32; sweep++)
    {
        int p = 0, q = 1;
        double largest = System.Math.Abs(matrix[0, 1]);
        if (System.Math.Abs(matrix[0, 2]) > largest) { p = 0; q = 2; largest = System.Math.Abs(matrix[0, 2]); }
        if (System.Math.Abs(matrix[1, 2]) > largest) { p = 1; q = 2; largest = System.Math.Abs(matrix[1, 2]); }
        if (largest <= 1e-15 * System.Math.Max(1.0,
            System.Math.Abs(matrix[0, 0]) + System.Math.Abs(matrix[1, 1]) + System.Math.Abs(matrix[2, 2]))) break;
        double angle = 0.5 * System.Math.Atan2(2.0 * matrix[p, q], matrix[q, q] - matrix[p, p]);
        double cosine = System.Math.Cos(angle), sine = System.Math.Sin(angle);
        double app = matrix[p, p], aqq = matrix[q, q], apq = matrix[p, q];
        matrix[p, p] = cosine * cosine * app - 2.0 * sine * cosine * apq + sine * sine * aqq;
        matrix[q, q] = sine * sine * app + 2.0 * sine * cosine * apq + cosine * cosine * aqq;
        matrix[p, q] = matrix[q, p] = 0.0;
        for (int r = 0; r < 3; r++)
        {
            if (r == p || r == q) continue;
            double arp = matrix[r, p], arq = matrix[r, q];
            matrix[r, p] = matrix[p, r] = cosine * arp - sine * arq;
            matrix[r, q] = matrix[q, r] = sine * arp + cosine * arq;
        }
    }
    return new[] { matrix[0, 0], matrix[1, 1], matrix[2, 2] }
        .Select(value => System.Math.Max(0.0, value)).OrderBy(value => value).ToArray();
}

// --- Shared estimator kernel (phase570 v7 verbatim port, with components) ---
static DiagnosticBundle Diagnose(double[][] chains)
{
    int n = chains.Min(x => x.Length);
    double[] pooled = chains.SelectMany(x => x.Take(n)).ToArray();
    if (pooled.Distinct().Count() <= 1) return new DiagnosticBundle(double.NaN, double.NaN, double.NaN, double.NaN, double.NaN);
    double[] ranked = RankNormalize(pooled), folded = RankNormalize(pooled.Select(x => System.Math.Abs(x - Median(pooled))).ToArray());
    double[][] r = Regroup(ranked, chains.Length, n), f = Regroup(folded, chains.Length, n);
    double[] ordered = pooled.Order().ToArray();
    double q05 = ordered[(int)System.Math.Floor(0.05 * (ordered.Length - 1))];
    double q95 = ordered[(int)System.Math.Ceiling(0.95 * (ordered.Length - 1))];
    double[][] lower = chains.Select(x => x.Take(n).Select(value => value <= q05 ? 1.0 : 0.0).ToArray()).ToArray();
    double[][] upper = chains.Select(x => x.Take(n).Select(value => value >= q95 ? 1.0 : 0.0).ToArray()).ToArray();
    double rankedComponent = SplitRhat(r), foldedComponent = SplitRhat(f);
    return new DiagnosticBundle(System.Math.Max(rankedComponent, foldedComponent), Ess(Split(r)),
        System.Math.Min(Ess(Split(lower)), Ess(Split(upper))), rankedComponent, foldedComponent);
}
static double[] RankVector(double[] values)
{
    int n = values.Length; int[] order = Enumerable.Range(0, n).OrderBy(i => values[i]).ToArray(); var ranks = new double[n];
    for (int i = 0; i < n;) { int j = i; while (j + 1 < n && values[order[j + 1]] == values[order[i]]) j++; double rank = (i + j) / 2.0 + 1.0; for (int k = i; k <= j; k++) ranks[order[k]] = rank; i = j + 1; }
    return ranks;
}
static double[] RankNormalize(double[] values)
{
    int n = values.Length;
    return RankVector(values).Select(x => InverseNormalCdf((x - 0.375) / (n + 0.25))).ToArray();
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
    double r=p-0.5,s=r*r;return (((((a[0]*s+a[1])*s+a[2])*s+a[3])*s+a[4])*s+a[5])*r/(((((b[0]*s+b[1])*s+b[2])*s+b[3])*s+b[4])*s+1);
}

// --- Side-A basis helpers (phase570 v7 verbatim port) ---
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

// --- Common numerics and plumbing ---
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
static bool BitEqual(double a, double b) => BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b);
static double ScaledAbsoluteDifference(double a, double b) => System.Math.Abs(a - b) / System.Math.Max(1.0, System.Math.Abs(b));
static double[] Subtract(double[] a, double[] b) => a.Zip(b, (x, y) => x - y).ToArray();
static double Dot(double[] a, double[] b) { double sum = 0.0; for (int i = 0; i < a.Length; i++) sum += a[i] * b[i]; return sum; }
static double NormSquared(double[] vector) => Dot(vector, vector);
static double Uniform(Xoshiro rng) => ((rng.Next() >> 11) + 0.5) / 9007199254740992.0;
static double Gauss(Xoshiro rng) { double u1 = Uniform(rng), u2 = Uniform(rng); return System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Cos(2 * System.Math.PI * u2); }
static ulong[] ExpandSeed(ulong seed) { ulong state = seed; ulong Next() { state += 0x9E3779B97F4A7C15UL; ulong z = state; z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL; z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL; return z ^ (z >> 31); } return [Next(), Next(), Next(), Next()]; }
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string Suffix(string chainId) => chainId.Replace("complete-lattice-pilot-", string.Empty, StringComparison.Ordinal);
JsonElement ReadBinding(string id) => JsonDocument.Parse(File.ReadAllBytes(PathFor(id))).RootElement.Clone();
string PathFor(string id) => specs.Single(x => x.Id == id).Path;
void Emit(object payload)
{
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    File.WriteAllBytes(OutputPath, bytes); File.WriteAllBytes(SummaryPath, bytes);
}
object Early(string verdict, bool valid, bool bindingsValid, bool accepted, bool rngWasUsed, object bindingRows, object battery) => new
{
    schemaVersion = 1, phase = 574, phaseId = "phase574-directional-raw-trace-fold-order-localization",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid = valid,
    exactBindingsValid = bindingsValid, resourceAccepted = accepted, bindings = bindingRows, knownAnswerBattery = battery,
    upstream = (object?)null, reproduction = (object?)null, comparison = (object?)null, hFold = (object?)null,
    attribution = (object?)null, rawTraces = (object?)null,
    verdictKind = verdict, terminalStatus = "directional-raw-trace-fold-order-localization-" + verdict,
    closesOnlyPhase573DiagnosticQuestion = true,
    phase570Or571Or572Reinterpreted = false, phase572ToleranceRelaxed = false, phase571LeverConfirmed = false,
    prospectivePackPlanningOpened = false, newSamplingPerformed = false,
    markovChainAdvancedBeyondCommittedReplay = false, configurationsRetained = false,
    rawDirectionalSeriesRetained = false,
    rngUsed = rngWasUsed, replayRngAllocated = rngWasUsed, rngUseRestrictedToCommittedReplay = true,
    phase548Or549TerminalChanged = false, registeredBlindSeedTouched = false,
    protectedPhase554SeedsRead = false, registeredTargetChanged = false,
    directionCalledGaugeOrRedundant = false, quotientApplied = false, gaugeFixingApplied = false,
    measureNormalizationApplied = false, sourceOrModelSelected = false,
    phase561Opened = false, o4Discharged = false, phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};

sealed record Binding(string Id, string Path, string Hash);
sealed record AllocationRow(string Id, string Shape, long Bytes);
sealed record ChainPlan(string TableId, int RawSeed, int ExecutionSeed, double InitialScale);
sealed record DiagnosticBundle(double Rhat, double BulkEss, double TailEss, double RankedComponent, double FoldedComponent);
sealed record InvariantMetrics(double TotalNormSquared, double ENormSquared, double WNormSquared, double ClosedNormSquared, double ClosedPerpNormSquared, double[] GramEigenvalues, double RankOneAlignment, double WithinClosedRankOneDistanceSquared, double WithinClosedRankOneRelativeDistance, double FullRankOneDistanceSquared, double[] GramFlat);
sealed record ProjectionMetric(double TotalNormSquared, double ENormSquared, double WNormSquared, double CNormSquared, double CperpNormSquared, double[] ClosedGramEigenvalues, double ClosedRankOneAlignment, double[] GramFlat);
sealed record Direction(string Id, string Kind, double[] Vector);
sealed record Subspaces(Direction[] Exact, Direction[] Winding, Direction[] Closed);
sealed class Xoshiro(ulong[] state)
{
    private ulong s0 = state[0], s1 = state[1], s2 = state[2], s3 = state[3];
    public ulong Next() { ulong result = RotateLeft(s1 * 5, 7) * 9, t = s1 << 17; s2 ^= s0; s3 ^= s1; s1 ^= s2; s0 ^= s3; s2 ^= t; s3 = RotateLeft(s3, 45); return result; }
    private static ulong RotateLeft(ulong x, int k) => (x << k) | (x >> (64 - k));
}
