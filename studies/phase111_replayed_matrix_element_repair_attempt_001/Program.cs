using System.Text.Json;

const string DefaultOutputDir = "studies/phase111_replayed_matrix_element_repair_attempt_001/output";
const string Phase110ContractPath = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string ReplayPackageSummaryPath = "studies/phase99_selector_eigenvector_full_lift_001/output/replay_probe_4x4/first_boson_replay_package_summary.json";
const string Phase108ClosurePath = "studies/phase108_candidate3_physical_comparison_closure_001/output/candidate3_physical_comparison_closure.json";
const string Phase63GeneratorNormalizationPath = "studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE111_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var contract = JsonDocument.Parse(File.ReadAllText(Phase110ContractPath));
using var replay = JsonDocument.Parse(File.ReadAllText(ReplayPackageSummaryPath));
using var closure = JsonDocument.Parse(File.ReadAllText(Phase108ClosurePath));
using var generator = JsonDocument.Parse(File.ReadAllText(Phase63GeneratorNormalizationPath));

var coupling = replay.RootElement.GetProperty("coupling");
double? rawMagnitude = JsonDouble(coupling, "couplingProxyMagnitude");
double? generatorScale = JsonDouble(generator.RootElement, "internalToPhysicalGeneratorScale");
double? normalizedCandidateWeakCoupling = Product(rawMagnitude, generatorScale);
double? targetWeakCoupling = JsonDouble(
    contract.RootElement.GetProperty("repairTarget"),
    "targetImpliedWeakCoupling");

bool rawEvidenceValidated = string.Equals(
    JsonString(replay.RootElement, "evidenceStatus"),
    "raw-weak-coupling-matrix-element-evidence-validated",
    StringComparison.Ordinal);
bool candidate3Closed = string.Equals(
    JsonString(closure.RootElement, "terminalStatus"),
    "candidate3-physical-comparison-closed-internal-only",
    StringComparison.Ordinal);
bool sourceCompatibleWithWzRepair = false;

var result = new
{
    phaseId = "phase111-replayed-matrix-element-repair-attempt",
    terminalStatus = rawEvidenceValidated && !sourceCompatibleWithWzRepair
        ? "replayed-matrix-element-evidence-valid-but-wz-repair-incompatible"
        : "replayed-matrix-element-repair-blocked",
    strategyId = "replayed-analytic-raw-matrix-element",
    rawEvidenceValidated,
    sourceCompatibleWithWzRepair,
    candidate3RouteClosed = candidate3Closed,
    sourceBosonModeId = JsonString(replay.RootElement, "bosonModeId"),
    rawMatrixElementMagnitude = rawMagnitude,
    generatorNormalizationScale = generatorScale,
    normalizedCandidateWeakCoupling,
    targetImpliedWeakCoupling = targetWeakCoupling,
    normalizedCandidateToTargetWeakCouplingRatio = Ratio(normalizedCandidateWeakCoupling, targetWeakCoupling),
    repairAccepted = false,
    diagnosis = new[]
    {
        "replayed analytic raw matrix-element evidence is present and validated for the candidate-3 internal replay",
        "Phase108 closes candidate-3 as internal-only, so this evidence cannot repair W/Z absolute masses",
        "W/Z absolute repair still needs replayed analytic raw matrix-element evidence tied to the validated W/Z absolute route or a target-independent scalar relation revision",
    },
    closureRequirements = new[]
    {
        "produce replayed analytic raw matrix-element evidence for the validated W/Z absolute mass route, not the closed candidate-3 route",
        "or use the scalar-sector relation revision strategy from Phase110",
    },
    sourceEvidence = new
    {
        contractPath = Phase110ContractPath,
        replayPackageSummaryPath = ReplayPackageSummaryPath,
        candidate3ClosurePath = Phase108ClosurePath,
        generatorNormalizationPath = Phase63GeneratorNormalizationPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "replayed_matrix_element_repair_attempt.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "replayed_matrix_element_repair_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase111-replayed-matrix-element-repair-attempt",
        result.terminalStatus,
        result.rawEvidenceValidated,
        result.sourceCompatibleWithWzRepair,
        result.normalizedCandidateWeakCoupling,
        result.targetImpliedWeakCoupling,
        result.normalizedCandidateToTargetWeakCouplingRatio,
        result.repairAccepted,
    }, options));

Console.WriteLine(result.terminalStatus);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static double? Product(double? a, double? b) =>
    a is { } x && b is { } y && double.IsFinite(x) && double.IsFinite(y) ? x * y : null;

static double? Ratio(double? numerator, double? denominator) =>
    numerator is { } n && denominator is { } d && double.IsFinite(n) && double.IsFinite(d) && d != 0.0
        ? n / d
        : null;
