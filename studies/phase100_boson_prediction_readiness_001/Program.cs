using System.Text.Json;
using Gu.Phase5.Reporting;

const string DefaultOutputDir = "studies/phase100_boson_prediction_readiness_001/output";
const string Phase99EvidencePath = "studies/phase99_selector_eigenvector_full_lift_001/output/selector_eigenvector_full_lift_evidence.json";
const string Phase91BranchEvidencePath = "studies/phase91_branch_stability_evidence_promotion_001/output/projected_branch_stability_evidence.json";
const string Phase95SummaryPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/target_blind_refinement_mode_matching_summary.json";
const string Phase90SummaryPath = "studies/phase90_branch_stability_scan_001/output/branch_stability_scan_summary.json";
const string Phase44MappingsPath = "studies/phase44_selector_eigen_wz_physical_prediction_001/physical_observable_mappings.json";
const string Phase44CalibrationsPath = "studies/phase44_selector_eigen_wz_physical_prediction_001/physical_calibrations.json";
const string Phase18TargetsPath = "studies/phase18_experimental_targets_001/physical_targets.json";
const string Phase74ComparisonPath = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase47FalsifierAuditPath = "studies/phase47_wz_physical_claim_falsifier_relevance_001/physical_claim_falsifier_relevance_audit.json";
const string Phase102PromotionPath = "studies/phase102_internal_boson_claim_promotion_001/output/internal_boson_claim_promotion.json";
const string Phase103PolicyPath = "studies/phase103_target_scoped_falsifier_policy_001/output/target_scoped_falsifier_policy.json";
const string Phase104MappingAttemptPath = "studies/phase104_candidate3_physical_mapping_attempt_001/output/candidate3_physical_mapping_attempt.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE100_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase99 = JsonDocument.Parse(File.ReadAllText(Phase99EvidencePath));
using var phase91 = JsonDocument.Parse(File.ReadAllText(Phase91BranchEvidencePath));
using var phase95 = JsonDocument.Parse(File.ReadAllText(Phase95SummaryPath));
using var phase90 = JsonDocument.Parse(File.ReadAllText(Phase90SummaryPath));
using var mappings = JsonDocument.Parse(File.ReadAllText(Phase44MappingsPath));
using var calibrations = JsonDocument.Parse(File.ReadAllText(Phase44CalibrationsPath));
using var targets = JsonDocument.Parse(File.ReadAllText(Phase18TargetsPath));
using var comparison = JsonDocument.Parse(File.ReadAllText(Phase74ComparisonPath));
using var falsifiers = JsonDocument.Parse(File.ReadAllText(Phase47FalsifierAuditPath));
using var promotion = TryParseJson(Phase102PromotionPath);
using var policy = TryParseJson(Phase103PolicyPath);
using var mappingAttempt = TryParseJson(Phase104MappingAttemptPath);

var replayProbe = phase99.RootElement.GetProperty("replayProbe");
var fullLift = phase99.RootElement.GetProperty("fullConnectionLift");
var selectorMode = phase99.RootElement.GetProperty("selectorEigenmode");
var bestCandidate = phase90.RootElement.GetProperty("bestCandidate");

string candidateId = JsonString(phase91.RootElement, "candidateId") ?? "candidate-3";
string selectedModeId = JsonString(replayProbe, "selectedBosonModeId")
    ?? "phase99-selector-eigenvector-full-lift-candidate-3-mode-0-4x4";
string claimClass = JsonString(bestCandidate, "candidateClaimClass") ?? "C0_NumericalMode";
if (promotion is not null &&
    string.Equals(JsonString(promotion.RootElement, "terminalStatus"), "internal-boson-claim-promoted", StringComparison.Ordinal) &&
    string.Equals(JsonString(promotion.RootElement, "candidateId"), candidateId, StringComparison.Ordinal))
{
    claimClass = JsonString(promotion.RootElement, "proposedClaimClass") ?? claimClass;
}

bool hasCandidateSpecificMapping = mappingAttempt is not null
    ? JsonBool(mappingAttempt.RootElement, "candidateSpecificPhysicalMappingValidated") is true
    : HasValidatedCandidateSpecificRecord(mappings.RootElement, candidateId);
bool hasCandidateSpecificCalibration = mappingAttempt is not null
    ? JsonBool(mappingAttempt.RootElement, "candidateSpecificCalibrationValidated") is true
    : HasValidatedCandidateSpecificRecord(calibrations.RootElement, candidateId);
bool hasTargetScopedPolicy = policy is not null &&
                             JsonBool(policy.RootElement, "targetScopedPolicyAdopted") is true;

var input = new BosonPredictionReadinessInput
{
    CandidateId = candidateId,
    SelectedBosonModeId = selectedModeId,
    ReplayTerminalStatus = JsonString(replayProbe, "replayTerminalStatus") ?? "",
    ReplayClosureRequirements = JsonStringArray(replayProbe, "replayClosureRequirements"),
    LocalGateBlockers = JsonStringArray(replayProbe, "physicalPredictionGateBlockers"),
    CouplingMagnitude = JsonDouble(replayProbe, "couplingMagnitude"),
    FullConnectionLiftMaterialized = string.Equals(
        JsonString(phase99.RootElement, "terminalStatus"),
        "selector-eigenvector-full-lift-materialized",
        StringComparison.Ordinal),
    FullConnectionModeVectorLength = JsonInt(fullLift, "modeVectorLength") ?? 0,
    BranchStabilityScore = JsonDouble(phase91.RootElement, "branchStabilityScore"),
    RefinementStabilityScore = JsonDouble(phase95.RootElement, "refinementStabilityScore"),
    ClaimClass = claimClass,
    HasCandidateSpecificPhysicalMapping = hasCandidateSpecificMapping,
    HasCandidateSpecificCalibration = hasCandidateSpecificCalibration,
    HasPhysicalTargets = HasAnyArrayItem(targets.RootElement, "targets"),
    PriorAbsoluteComparisonPassed = string.Equals(
        JsonString(comparison.RootElement, "terminalStatus"),
        "wz-absolute-mass-target-comparison-passed",
        StringComparison.Ordinal),
    TargetRelevantSevereFalsifierCount = JsonInt(falsifiers.RootElement, "targetRelevantSevereFalsifierCount"),
    GlobalSidecarSevereFalsifierCount = JsonInt(falsifiers.RootElement, "globalSidecarSevereFalsifierCount"),
    HasTargetScopedFalsifierPolicy = hasTargetScopedPolicy,
    ResidualLimitations = JsonStringArray(phase99.RootElement, "residualLimitations"),
};

var result = BosonPredictionReadinessAuditor.Evaluate(input);
var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "boson_prediction_readiness_input.json"),
    JsonSerializer.Serialize(input, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_prediction_readiness.json"),
    JsonSerializer.Serialize(result, options));

var summary = new
{
    phaseId = "phase100-boson-prediction-readiness",
    result.TerminalStatus,
    result.PredictionLevel,
    result.InternalBosonReplayPredictionReady,
    result.ExternalPhysicalComparisonReady,
    result.CouplingMagnitude,
    selectedSelectorMode = new
    {
        modeId = JsonString(selectorMode, "modeId"),
        modeIndex = JsonInt(selectorMode, "modeIndex"),
        basisDimension = JsonInt(selectorMode, "basisDimension"),
    },
    readinessPath = Path.Combine(outputDir, "boson_prediction_readiness.json"),
    consumedFollowOnEvidence = new
    {
        claimPromotionPath = promotion is null ? null : Phase102PromotionPath,
        targetScopedFalsifierPolicyPath = policy is null ? null : Phase103PolicyPath,
        physicalMappingAttemptPath = mappingAttempt is null ? null : Phase104MappingAttemptPath,
    },
    requiredFixes = result.RequiredFixes,
};
File.WriteAllText(
    Path.Combine(outputDir, "boson_prediction_readiness_summary.json"),
    JsonSerializer.Serialize(summary, options));

Console.WriteLine(result.TerminalStatus);
Console.WriteLine($"predictionLevel={result.PredictionLevel}");
Console.WriteLine($"couplingMagnitude={result.CouplingMagnitude:R}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static JsonDocument? TryParseJson(string path) =>
    File.Exists(path) ? JsonDocument.Parse(File.ReadAllText(path)) : null;

static IReadOnlyList<string> JsonStringArray(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        return [];

    return value.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.String)
        .Select(item => item.GetString()!)
        .ToList();
}

static bool HasAnyArrayItem(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    value.ValueKind == JsonValueKind.Array &&
    value.GetArrayLength() > 0;

static bool HasValidatedCandidateSpecificRecord(JsonElement document, string candidateId)
{
    foreach (var propertyName in new[] { "mappings", "calibrations" })
    {
        if (!document.TryGetProperty(propertyName, out var records) || records.ValueKind != JsonValueKind.Array)
            continue;

        foreach (var record in records.EnumerateArray())
        {
            var status = JsonString(record, "status");
            var serialized = record.GetRawText();
            if (string.Equals(status, "validated", StringComparison.OrdinalIgnoreCase) &&
                serialized.Contains(candidateId, StringComparison.Ordinal))
            {
                return true;
            }
        }
    }

    return false;
}
