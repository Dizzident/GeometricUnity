using System.Text.Json;

const string DefaultOutputDir = "studies/phase147_boson_prediction_completion_attempt_001/output";
const string Phase51Path = "studies/phase51_broad_boson_prediction_readiness_001/broad_boson_prediction_readiness.json";
const string Phase74Path = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase76Path = "studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json";
const string Phase79Path = "studies/phase79_analytic_weak_coupling_replay_harness_001/analytic_weak_coupling_replay_harness.json";
const string Phase100Path = "studies/phase100_boson_prediction_readiness_001/output/boson_prediction_readiness.json";
const string Phase101Path = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package.json";
const string Phase106Path = "studies/phase106_candidate3_observable_identity_derivation_001/output/candidate3_observable_identity_derivation.json";
const string Phase107Path = "studies/phase107_candidate3_target_independent_normalization_001/output/candidate3_target_independent_normalization.json";
const string Phase109Path = "studies/phase109_post_candidate3_route_selection_001/output/post_candidate3_route_selection.json";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase122Path = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase140Path = "studies/phase140_fermion_sector_artifact_intake_contract_001/output/fermion_sector_artifact_intake_contract.json";
const string Phase141Path = "studies/phase141_fermion_sector_intake_application_gate_001/output/fermion_sector_intake_application_gate.json";
const string Phase142Path = "studies/phase142_post_intake_rerun_plan_gate_001/output/post_intake_rerun_plan_gate.json";
const string Phase146Path = "studies/phase146_fermion_sector_evidence_census_001/output/fermion_sector_evidence_census.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE147_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase51 = JsonDocument.Parse(File.ReadAllText(Phase51Path));
using var phase74 = JsonDocument.Parse(File.ReadAllText(Phase74Path));
using var phase76 = JsonDocument.Parse(File.ReadAllText(Phase76Path));
using var phase79 = JsonDocument.Parse(File.ReadAllText(Phase79Path));
using var phase100 = JsonDocument.Parse(File.ReadAllText(Phase100Path));
using var phase101 = JsonDocument.Parse(File.ReadAllText(Phase101Path));
using var phase106 = JsonDocument.Parse(File.ReadAllText(Phase106Path));
using var phase107 = JsonDocument.Parse(File.ReadAllText(Phase107Path));
using var phase109 = JsonDocument.Parse(File.ReadAllText(Phase109Path));
using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));
using var phase122 = JsonDocument.Parse(File.ReadAllText(Phase122Path));
using var phase140 = JsonDocument.Parse(File.ReadAllText(Phase140Path));
using var phase141 = JsonDocument.Parse(File.ReadAllText(Phase141Path));
using var phase142 = JsonDocument.Parse(File.ReadAllText(Phase142Path));
using var phase146 = JsonDocument.Parse(File.ReadAllText(Phase146Path));

var phase51Records = phase51.RootElement.GetProperty("records").EnumerateArray().ToArray();
var completedPredictions = phase51Records
    .Where(record => string.Equals(JsonString(record, "readinessStatus"), "predicted", StringComparison.Ordinal))
    .Select(record => new CompletedPrediction(
        RecordId: RequiredString(record, "recordId"),
        TargetObservableId: RequiredString(record, "targetObservableId"),
        ComputedValue: JsonDouble(record, "computedValue"),
        ComputedUncertainty: JsonDouble(record, "computedUncertainty"),
        TargetValue: JsonDouble(record, "targetValue"),
        TargetUncertainty: JsonDouble(record, "targetUncertainty"),
        Pull: JsonDouble(record, "pull")))
    .ToArray();

var blockedPredictionRows = phase51Records
    .Where(record => !string.Equals(JsonString(record, "readinessStatus"), "predicted", StringComparison.Ordinal))
    .Select(record => new BlockedPrediction(
        RecordId: RequiredString(record, "recordId"),
        TargetObservableId: JsonString(record, "targetObservableId"),
        ReadinessStatus: JsonString(record, "readinessStatus"),
        ClosureRequirements: StringArray(record, "closureRequirements")))
    .ToArray();

double? strongestMinRawToTarget = phase122.RootElement.TryGetProperty("strongest", out var strongest)
    ? JsonDouble(strongest, "minRawToTargetRatio")
    : null;
double? strongestOffDiagonalMinRawToTarget = phase122.RootElement.TryGetProperty("strongestOffDiagonal", out var strongestOffDiagonal)
    ? JsonDouble(strongestOffDiagonal, "minRawToTargetRatio")
    : null;
bool ratioComplete = completedPredictions.Any(prediction =>
    string.Equals(prediction.TargetObservableId, "physical-w-z-mass-ratio", StringComparison.Ordinal));
bool candidate3PhysicalReady = JsonBool(phase100.RootElement, "externalPhysicalComparisonReady") is true
    && JsonBool(phase101.RootElement, "canCompareToExternalBosonValues") is true;
bool candidate3IdentityValidated = JsonBool(phase106.RootElement, "identityValidated") is true;
bool candidate3NormalizationValidated = JsonBool(phase107.RootElement, "normalizationValidated") is true;
bool absoluteMassComparisonPassed = string.Equals(
    JsonString(phase74.RootElement, "terminalStatus"),
    "wz-absolute-mass-target-comparison-passed",
    StringComparison.Ordinal);
bool allowedWeakCouplingRepairReady = string.Equals(
    JsonString(phase122.RootElement, "terminalStatus"),
    "corrected-operator-selection-rule-sweep-found-projection-candidate",
    StringComparison.Ordinal);
bool intakeReady = JsonBool(phase140.RootElement, "intakeArtifactPromotable") is true
    && JsonBool(phase141.RootElement, "intakeApplicationPromotable") is true
    && JsonBool(phase142.RootElement, "rerunPlanExecutable") is true;
bool existingFermionSectorEvidenceFound = JsonBool(phase146.RootElement, "currentEvidencePresent") is true;
bool fullBosonPredictionComplete = ratioComplete
    && candidate3PhysicalReady
    && candidate3IdentityValidated
    && candidate3NormalizationValidated
    && absoluteMassComparisonPassed;

string terminalStatus = fullBosonPredictionComplete
    ? "boson-prediction-complete"
    : ratioComplete
        ? "boson-prediction-partial-wz-ratio-complete-full-physical-blocked"
        : "boson-prediction-completion-blocked";

var attemptedRoutes = new[]
{
    new
    {
        routeId = "wz-ratio",
        status = ratioComplete ? "complete" : "blocked",
        evidence = "Phase51 validates the W/Z mass-ratio prediction route.",
    },
    new
    {
        routeId = "candidate3-physical-promotion",
        status = candidate3PhysicalReady && candidate3IdentityValidated && candidate3NormalizationValidated ? "complete" : "blocked",
        evidence = "Phase100/101 remain external-comparison blocked; Phase106 rejects candidate-3 as internal-only; Phase107 cannot normalize an internal-only coupling proxy.",
    },
    new
    {
        routeId = "wz-absolute-mass-repair",
        status = absoluteMassComparisonPassed ? "complete" : "blocked",
        evidence = "Phase74 absolute W/Z comparison fails; Phase76 says the coherent miss requires replayed raw matrix-element evidence or scalar-sector revision.",
    },
    new
    {
        routeId = "corrected-operator-raw-matrix-element-replay",
        status = allowedWeakCouplingRepairReady ? "complete" : "blocked",
        evidence = $"Phase122 found no projection candidate; strongest quality transition reaches minRawToTargetRatio={strongestMinRawToTarget:R}.",
    },
    new
    {
        routeId = "fermion-sector-intake",
        status = intakeReady ? "complete" : "blocked",
        evidence = existingFermionSectorEvidenceFound
            ? "P146 found a candidate local evidence artifact, but the intake chain is not yet complete."
            : "P146 scanned local outputs and found no non-synthetic artifact satisfying the P140 intake contract.",
    },
};

var blockers = new List<string>();
if (!candidate3IdentityValidated)
    blockers.Add("candidate-3 remains an internal coupling proxy, not a validated physical boson observable");
if (!candidate3NormalizationValidated)
    blockers.Add("candidate-3 has no target-independent physical normalization");
if (!absoluteMassComparisonPassed)
    blockers.Add("absolute W/Z mass comparison remains failed");
if (!allowedWeakCouplingRepairReady)
    blockers.Add("corrected-operator replay does not produce a raw matrix-element projection candidate");
if (!existingFermionSectorEvidenceFound)
    blockers.Add("no existing local target-blind fermion-sector evidence satisfies the P140 intake contract");

var result = new
{
    phaseId = "phase147-boson-prediction-completion-attempt",
    terminalStatus,
    fullBosonPredictionComplete,
    completedPhysicalPredictionCount = completedPredictions.Length,
    completedPredictions,
    blockedPredictionRows,
    attemptedRoutes,
    strongestCorrectedOperatorTransition = new
    {
        minRawToTargetRatio = strongestMinRawToTarget,
        strongestOffDiagonalMinRawToTargetRatio = strongestOffDiagonalMinRawToTarget,
        phase122Status = JsonString(phase122.RootElement, "terminalStatus"),
    },
    currentReadiness = new
    {
        phase100Status = JsonString(phase100.RootElement, "terminalStatus"),
        phase101Status = JsonString(phase101.RootElement, "terminalStatus"),
        phase74Status = JsonString(phase74.RootElement, "terminalStatus"),
        phase76Status = JsonString(phase76.RootElement, "terminalStatus"),
        phase79Status = JsonString(phase79.RootElement, "terminalStatus"),
        phase110Status = JsonString(phase110.RootElement, "terminalStatus"),
        phase140Status = JsonString(phase140.RootElement, "terminalStatus"),
        phase146Status = JsonString(phase146.RootElement, "terminalStatus"),
    },
    blockers,
    cannotCompleteFromCurrentLocalArtifacts = !fullBosonPredictionComplete,
    requiredNewInputs = new[]
    {
        "target-blind fermion-sector label table or nontrivial transition rule satisfying P140, or a target-independent W/Z bridge revision",
        "candidate-3 observable identity derivation if candidate-3 is to be promoted beyond internal replay",
        "target-independent absolute mass-energy calibration or scalar-sector relation revision if absolute W/Z masses are to be promoted",
    },
    sourceEvidence = new
    {
        phase51Path = Phase51Path,
        phase74Path = Phase74Path,
        phase76Path = Phase76Path,
        phase79Path = Phase79Path,
        phase100Path = Phase100Path,
        phase101Path = Phase101Path,
        phase106Path = Phase106Path,
        phase107Path = Phase107Path,
        phase109Path = Phase109Path,
        phase110Path = Phase110Path,
        phase122Path = Phase122Path,
        phase140Path = Phase140Path,
        phase141Path = Phase141Path,
        phase142Path = Phase142Path,
        phase146Path = Phase146Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "boson_prediction_completion_attempt.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_prediction_completion_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.fullBosonPredictionComplete,
        result.completedPhysicalPredictionCount,
        result.completedPredictions,
        result.attemptedRoutes,
        result.strongestCorrectedOperatorTransition,
        result.currentReadiness,
        result.blockers,
        result.cannotCompleteFromCurrentLocalArtifacts,
        result.requiredNewInputs,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"fullBosonPredictionComplete={fullBosonPredictionComplete}");
Console.WriteLine($"completedPhysicalPredictionCount={completedPredictions.Length}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static IReadOnlyList<string> StringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? "")
            .ToArray()
        : Array.Empty<string>();

sealed record CompletedPrediction(
    string RecordId,
    string TargetObservableId,
    double? ComputedValue,
    double? ComputedUncertainty,
    double? TargetValue,
    double? TargetUncertainty,
    double? Pull);
sealed record BlockedPrediction(
    string RecordId,
    string? TargetObservableId,
    string? ReadinessStatus,
    IReadOnlyList<string> ClosureRequirements);
