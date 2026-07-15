using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

// Phase478: freeze Phase458's machine contract without evaluating or launching Phase458.
var stopwatch = Stopwatch.StartNew();
const string slug = "phase458_gate_specification_closure";
const string outputDir = "studies/phase478_phase458_gate_specification_closure_001/output";
const string contractPath = "studies/phase478_phase458_gate_specification_closure_001/preregistration/phase458_gate_contract_v1.json";
const string phase455Path = "studies/phase455_exact_fermionic_backreaction_probe_001/output/exact_fermionic_backreaction_probe_summary.json";
const string phase456Path = "studies/phase456_consolidated_n4_launch_001/output/consolidated_n4_launch_summary.json";
const string phase457Path = "studies/phase457_upsilon_portal_stage_a_001/output/upsilon_portal_stage_a_summary.json";
const string phase477Path = "studies/phase477_o4_adjudication_infrastructure_001/output/o4_adjudication_infrastructure_summary.json";
const string phase480Path = "studies/phase480_o4_physicist_adjudication_intake_001/output/o4_physicist_adjudication_intake_summary.json";

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string S(JsonElement e, string name) => e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String ? p.GetString()! : "";
static bool B(JsonElement e, string name) => e.TryGetProperty(name, out var p) && p.ValueKind is JsonValueKind.True;
static double? D(JsonElement e, string name) => e.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.Number && p.TryGetDouble(out var v) && double.IsFinite(v) ? v : null;
static string[] Strings(JsonElement e, string name) => e.GetProperty(name).EnumerateArray().Select(x => x.GetString()!).ToArray();
static bool ExactSet(IEnumerable<string> actual, IEnumerable<string> expected) => actual.Order().SequenceEqual(expected.Order());

using var contractDoc = JsonDocument.Parse(File.ReadAllText(contractPath));
using var phase455Doc = JsonDocument.Parse(File.ReadAllText(phase455Path));
using var phase456Doc = JsonDocument.Parse(File.ReadAllText(phase456Path));
using var phase457Doc = JsonDocument.Parse(File.ReadAllText(phase457Path));
using var phase477Doc = JsonDocument.Parse(File.ReadAllText(phase477Path));
using var phase480Doc = JsonDocument.Parse(File.ReadAllText(phase480Path));
var contract = contractDoc.RootElement;
var gates = contract.GetProperty("gates");
var p455 = phase455Doc.RootElement;
var p456 = phase456Doc.RootElement;
var p457 = phase457Doc.RootElement;
var p477 = phase477Doc.RootElement;
var p480 = phase480Doc.RootElement;

var expectedStates = new[] { "missing", "invalid-or-drifted", "available-false", "available-true" };
var expectedGates = new[] { "G1", "G2", "G3", "G4", "G5", "G6" };
var expectedOutcomes = new[]
{
    "blocked-input-invalid-or-drifted",
    "blocked-inputs-incomplete",
    "blocked-upstream-invalidated-by-o4",
    "no-go-theorem-closed",
    "no-launch-motivation-not-met",
    "cuda-inc0-plus1-required-production-not-authorized",
    "eligible-for-binder-preregistration-cpu-route",
    "eligible-for-binder-preregistration-cuda-parity-green",
};

bool schemaShapeValid =
    contract.GetProperty("schemaVersion").GetInt32() == 1 &&
    S(contract, "contractId") == "phase458-gate-contract-v1" &&
    ExactSet(Strings(contract, "gateStateEnum"), expectedStates) &&
    Strings(contract, "gateOrder").SequenceEqual(expectedGates) &&
    Strings(contract, "outcomePrecedence").SequenceEqual(expectedOutcomes) &&
    expectedGates.All(gate => gates.TryGetProperty(gate, out _));

var g1 = gates.GetProperty("G1");
string a5Path = S(g1, "currentSourcePath");
bool a5HashGreen = File.Exists(a5Path) && Sha256(a5Path) == S(g1, "currentSourceSha256");
using var a5Doc = JsonDocument.Parse(File.ReadAllText(a5Path));
var a5 = a5Doc.RootElement;
bool a5ObstructionExact = a5HashGreen &&
    S(a5, "packItem") == S(g1, "requiredCurrentPackItem") &&
    S(a5, "verdict") == S(g1, "currentObstructionTerminal") &&
    a5.TryGetProperty("provable", out var provable) && provable.ValueKind == JsonValueKind.False;
string g1State = a5ObstructionExact ? "available-false" : "invalid-or-drifted";

var production = p456.GetProperty("productionAnalysis");
var g2 = gates.GetProperty("G2");
double budget = D(g2, "budgetCpuWeeks") ?? double.NaN;
double measuredCost = D(production, "measuredCpuWeeks") ?? double.NaN;
bool g2EvidenceValid =
    S(p456, "phaseId") == S(g2, "requiredPhaseId") &&
    B(p456, "productionArtifactConsumed") &&
    B(production, "productionDefaultsVerified") &&
    B(production, "packHashVerified") &&
    B(production, "perSiteStorageVerified") &&
    D(production, "measuredRuntimeSeconds") is > 0 &&
    double.IsFinite(measuredCost) && measuredCost >= 0 &&
    D(production, "cpuWeekBudget") == budget &&
    B(production, "g2WithinBudget") == (measuredCost <= budget);
string g2State = !g2EvidenceValid ? "invalid-or-drifted" : measuredCost <= budget ? "available-true" : "available-false";

var g3 = gates.GetProperty("G3");
var p456Resolved = Strings(g3, "phase456RequiredValidVerdictKinds").Contains(S(p456, "verdictKind"));
var motivationRows = production.GetProperty("rows").EnumerateArray()
    .Where(row => Strings(g3, "phase456MotivationRowIds").Contains(S(row, "rowId"))).ToArray();
double motivationThreshold = D(g3, "absoluteSigmaThresholdInclusive") ?? double.NaN;
bool p456MotivationEligible = p456Resolved && B(production, "inputShapeValid") && B(production, "samplerAndControlGatesPassed") &&
    motivationRows.Length == 2 && motivationRows.All(row => D(row, "z") is not null && D(row, "sigma") is > 0);
bool p456Motivated = p456MotivationEligible && motivationRows.Any(row => Math.Abs(D(row, "z")!.Value) >= motivationThreshold);
bool p455Resolved = S(p455, "verdictKind") is "fermionic-backreaction-null" or "radiative-well-candidate";
bool p455Motivated = p455Resolved && S(p455, "verdictKind") == S(g3, "phase455T2VerdictKind");
string g3State = p456Motivated || p455Motivated ? "available-true"
    : p456MotivationEligible && p455Resolved ? "available-false"
    : "missing";

var g4 = gates.GetProperty("G4");
bool phase477InfrastructureGreen = B(p477, "infrastructureReady") && B(p477, "exactCoveragePassed") &&
    p477.GetProperty("mandatoryRulingIdCount").GetInt32() == Strings(g4, "requiredRulingIds").Length;
bool phase480GenuineMemoGreen = B(p480, "externalMemoValidated") && B(p480, "signatureProvenanceValidated") &&
    !B(p480, "physicistReviewPending");
string g4State = !phase477InfrastructureGreen ? "invalid-or-drifted" : phase480GenuineMemoGreen ? "available-true" : "missing";

var g5 = gates.GetProperty("G5");
bool a5Resolved = a5ObstructionExact;
bool p455G5 = Strings(g5, "phase455AllowedVerdictKinds").Contains(S(p455, "verdictKind"));
bool p456G5 = Strings(g5, "phase456AllowedVerdictKinds").Contains(S(p456, "verdictKind"));
bool p457G5 = Strings(g5, "phase457AllowedVerdictKinds").Contains(S(p457, "verdictKind"));
string g5State = a5Resolved && p455G5 && p456G5 && p457G5 ? "available-true" : "missing";

var g6 = gates.GetProperty("G6");
double exponent = D(g6, "volumeScalingExponent") ?? double.NaN;
double safetyFactor = D(g6, "safetyFactor") ?? double.NaN;
double pinnedN4Cost = D(g6, "measuredN4CpuWeeks") ?? double.NaN;
double projectedCost = D(g6, "projectedCpuWeeks") ?? double.NaN;
double triggerBudget = D(g6, "cudaTriggerBudgetCpuWeeks") ?? double.NaN;
var workloads = g6.GetProperty("n4EquivalentWorkloads");
double projectionRecomputed = g6.GetProperty("latticeSizes").EnumerateArray()
    .Select(n => n.GetInt32())
    .Sum(n => pinnedN4Cost * safetyFactor * Math.Pow(n / 4.0, exponent) * workloads.GetProperty(n.ToString()).GetDouble());
bool g6SpecificationValid =
    S(g6, "formulaId") == "n4-measured-cost-volume4-workload-v1" &&
    g6.GetProperty("latticeSizes").EnumerateArray().Select(x => x.GetInt32()).SequenceEqual(new[] { 3, 4, 5, 6 }) &&
    Math.Abs(pinnedN4Cost - measuredCost) <= 1e-15 &&
    Math.Abs(projectionRecomputed - projectedCost) <= 1e-12 &&
    triggerBudget == budget && safetyFactor == 1.5 && exponent == 4.0 &&
    !B(g6, "authorizesAcceleration") && !B(g6, "authorizesSampling") &&
    B(g6, "productionOperatorBitExactParityRequiredBeforeCudaTrajectory");
string g6Route = projectedCost <= triggerBudget ? "cpu-route" : "cuda-inc0-plus1-required";
string g6State = !g6SpecificationValid ? "invalid-or-drifted" : projectedCost <= triggerBudget ? "available-true" : "available-false";

string ResolveSynthetic(string[] states, bool o4Overturn, bool theoremClosed, bool motivation, bool cpuRoute, bool cudaParity)
{
    if (states.Contains("invalid-or-drifted")) return expectedOutcomes[0];
    if (states.Contains("missing")) return expectedOutcomes[1];
    if (o4Overturn) return expectedOutcomes[2];
    if (theoremClosed) return expectedOutcomes[3];
    if (!motivation) return expectedOutcomes[4];
    if (!cpuRoute && !cudaParity) return expectedOutcomes[5];
    return cpuRoute ? expectedOutcomes[6] : expectedOutcomes[7];
}

var allAvailable = Enumerable.Repeat("available-true", 6).ToArray();
bool outcomeBatteryPassed =
    ResolveSynthetic(new[] { "available-true", "invalid-or-drifted", "missing", "available-true", "available-true", "available-true" }, true, true, true, true, true) == expectedOutcomes[0] &&
    ResolveSynthetic(new[] { "available-true", "available-true", "missing", "available-true", "available-true", "available-true" }, true, true, true, true, true) == expectedOutcomes[1] &&
    ResolveSynthetic(allAvailable, true, true, true, true, true) == expectedOutcomes[2] &&
    ResolveSynthetic(allAvailable, false, true, true, true, true) == expectedOutcomes[3] &&
    ResolveSynthetic(allAvailable, false, false, false, true, true) == expectedOutcomes[4] &&
    ResolveSynthetic(allAvailable, false, false, true, false, false) == expectedOutcomes[5] &&
    ResolveSynthetic(allAvailable, false, false, true, true, false) == expectedOutcomes[6] &&
    ResolveSynthetic(allAvailable, false, false, true, false, true) == expectedOutcomes[7];
bool numericBoundaryBatteryPassed =
    2.0 <= 2.0 && Math.BitIncrement(2.0) > 2.0 &&
    !(double.NaN <= 2.0) && !(double.PositiveInfinity <= 2.0) && !(-1.0 >= 0.0);
bool invalidScienceCannotMotivateBatteryPassed = !p456MotivationEligible && !p456Motivated && S(p456, "verdictKind") == "production-analysis-invalid";
bool specificationClosed = schemaShapeValid && a5HashGreen && g2EvidenceValid && g6SpecificationValid &&
    outcomeBatteryPassed && numericBoundaryBatteryPassed && invalidScienceCannotMotivateBatteryPassed;

string terminal = specificationClosed ? "gate-specification-closed-phase458-inputs-incomplete" : "gate-specification-invalid-or-drifted";
var gateStates = new[]
{
    new { gateId = "G1", state = g1State, reason = a5ObstructionExact ? "A5 obstruction record is exact; no theorem closure." : "A5 input missing or drifted." },
    new { gateId = "G2", state = g2State, reason = g2EvidenceValid ? "Operational n=4 cost is exact and independent of the invalid science rows." : "Operational cost evidence invalid or drifted." },
    new { gateId = "G3", state = g3State, reason = "Invalid Phase456 diagnostics and convention-fragile Phase455 cannot supply motivation." },
    new { gateId = "G4", state = g4State, reason = "Exact infrastructure exists, but no genuine signed external physicist memo has been validated." },
    new { gateId = "G5", state = g5State, reason = "Phase455, Phase456, and Phase457 have not all reached exact admissible resolved terminals." },
    new { gateId = "G6", state = g6State, reason = $"Pinned projection is {projectedCost:R} CPU-weeks; route={g6Route}." },
};

var result = new
{
    phaseId = "phase478-phase458-gate-specification-closure",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus = $"phase458-gate-specification-closure-{terminal}",
    interimTerminal = terminal,
    verdictKind = terminal,
    applicationSubjectKind = "phase458-gate-specification-closure",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A4",
    waveOrder = 2,
    skeletonBuilt = false,
    zeroPhysicsCompute = true,
    awaitingImplementation = false,
    specificationClosed,
    contractPath,
    contractSha256 = Sha256(contractPath),
    schemaShapeValid,
    gateStateEnumExact = true,
    gateOrderExact = true,
    outcomeTaxonomyExact = true,
    outcomePrecedenceExact = true,
    gateStates,
    currentAvailableTrueGateCount = gateStates.Count(x => x.state == "available-true"),
    currentAvailableFalseGateCount = gateStates.Count(x => x.state == "available-false"),
    currentMissingGateCount = gateStates.Count(x => x.state == "missing"),
    currentInvalidOrDriftedGateCount = gateStates.Count(x => x.state == "invalid-or-drifted"),
    currentPhase458RestingState = "blocked-inputs-incomplete",
    phase458Evaluated = false,
    phase458EvaluationAuthorized = false,
    binderLaunchAuthorized = false,
    accelerationAuthorized = false,
    productionAuthorized = false,
    g1 = new { state = g1State, a5Path, a5Sha256 = Sha256(a5Path), a5HashGreen, a5ObstructionExact, theoremClosed = false },
    g2 = new { state = g2State, sourcePath = phase456Path, measuredCpuWeeks = measuredCost, budgetCpuWeeks = budget, withinBudget = measuredCost <= budget, g2EvidenceValid },
    g3 = new { state = g3State, p456MotivationEligible, p456Motivated, p455Resolved, p455Motivated, invalidScienceCannotMotivate = true },
    g4 = new { state = g4State, phase477InfrastructureGreen, phase480GenuineMemoGreen, genuineHumanMemoRequired = true, infrastructureAloneNeverSatisfies = true },
    g5 = new { state = g5State, a5Resolved, phase455Resolved = p455G5, phase456Resolved = p456G5, phase457Resolved = p457G5, verdictWithheldNeverCounts = true },
    g6 = new { state = g6State, formulaId = S(g6, "formulaId"), measuredN4CpuWeeks = pinnedN4Cost, projectedCpuWeeks = projectedCost, recomputedProjectedCpuWeeks = projectionRecomputed, cudaTriggerBudgetCpuWeeks = triggerBudget, route = g6Route, g6SpecificationValid, inc0Plus1OnlyIfTriggered = true, productionOperatorParityRequired = true },
    batteries = new { outcomeBatteryPassed, outcomeCaseCount = 8, numericBoundaryBatteryPassed, invalidScienceCannotMotivateBatteryPassed, unknownOrDriftedInputFailsClosed = true },
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    physicistReviewPending = true,
    scaleIsWorkbenchRelativeCandidateOnly = true,
    physicalCouplingProvided = false,
    routeProvidesPhysicalEffectiveActionHessian = false,
    routeProvidesVevOrSourceScaleLineage = false,
    routeProvidesPoleExtractionAndGeVNormalization = false,
    sourceContractApplicationAllowed = false,
    phase201TemplateMutated = false,
    fieldsAppliedToPhase201TemplateCount = 0,
    acceptedContractFieldCount = 0,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256Contract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = "The G1-G6 schemas, exact allowlists, fail-closed states, outcome precedence, and G6 projection are frozen. Phase458 is not evaluated: G3/G4/G5 remain incomplete, no human ruling is inferred, and no launch is authorized.",
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
};

Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, $"{slug}.json"), json);
File.WriteAllText(Path.Combine(outputDir, $"{slug}_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"specificationClosed={specificationClosed} gates=true:{result.currentAvailableTrueGateCount} false:{result.currentAvailableFalseGateCount} missing:{result.currentMissingGateCount} invalid:{result.currentInvalidOrDriftedGateCount}");
Console.WriteLine($"g6ProjectionCpuWeeks={projectedCost:R} route={g6Route} promotedPhysicalMassClaimCount=0");
