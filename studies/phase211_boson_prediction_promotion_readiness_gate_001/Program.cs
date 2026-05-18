using System.Text.Json;

const string DefaultOutputDir = "studies/phase211_boson_prediction_promotion_readiness_gate_001/output";
const string Phase101Path = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package_summary.json";
const string Phase202Path = "studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json";
const string Phase203Path = "studies/phase203_defensible_boson_value_manifest_001/output/defensible_boson_value_manifest_summary.json";
const string Phase210Path = "studies/phase210_boson_source_lineage_evidence_application_gate_001/output/boson_source_lineage_evidence_application_gate_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE211_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase101 = JsonDocument.Parse(File.ReadAllText(Phase101Path));
using var phase202 = JsonDocument.Parse(File.ReadAllText(Phase202Path));
using var phase203 = JsonDocument.Parse(File.ReadAllText(Phase203Path));
using var phase210 = JsonDocument.Parse(File.ReadAllText(Phase210Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var allKnownBosonValuesDefensible = JsonBool(phase101.RootElement, "allKnownBosonValuesDefensible") is true;
var completionAuditPassed = JsonBool(phase101.RootElement, "completionAuditPassed") is true;
var objectiveAchieved = JsonBool(phase202.RootElement, "objectiveAchieved") is true;
var rerunPromotionAllowed = JsonBool(phase210.RootElement, "rerunPromotionAllowed") is true;
var anyEvidenceReadyForApplication = JsonBool(phase210.RootElement, "anyEvidenceReadyForApplication") is true;
var defensibleValueCount = JsonInt(phase203.RootElement, "defensibleValueCount") ?? 0;
var failedAttemptCount = JsonInt(phase203.RootElement, "failedAttemptCount") ?? 0;
var blockedValueCount = JsonInt(phase203.RootElement, "blockedValueCount") ?? 0;
var blockerMatrixReady = JsonBool(phase213.RootElement, "blockerMatrixReady") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var checks = new[]
{
    new Check("current-defensible-manifest-present", defensibleValueCount > 0, $"defensibleValueCount={defensibleValueCount}"),
    new Check("new-source-evidence-ready", rerunPromotionAllowed && anyEvidenceReadyForApplication, $"rerunPromotionAllowed={rerunPromotionAllowed}; anyEvidenceReadyForApplication={anyEvidenceReadyForApplication}"),
    new Check("all-known-values-defensible", allKnownBosonValuesDefensible, $"allKnownBosonValuesDefensible={allKnownBosonValuesDefensible}"),
    new Check("completion-audit-passed", completionAuditPassed, $"completionAuditPassed={completionAuditPassed}"),
    new Check("objective-achieved", objectiveAchieved, $"objectiveAchieved={objectiveAchieved}"),
    new Check("source-lineage-blocker-matrix-ready", blockerMatrixReady, $"blockerMatrixReady={blockerMatrixReady}; existingEvidenceFound={existingEvidenceFound}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var promotionAttemptReady = rerunPromotionAllowed && anyEvidenceReadyForApplication;
var predictionSetComplete = allKnownBosonValuesDefensible && completionAuditPassed && objectiveAchieved;
var terminalStatus = predictionSetComplete
    ? "boson-prediction-promotion-readiness-complete"
    : promotionAttemptReady
        ? "boson-prediction-promotion-readiness-rerun-allowed"
        : "boson-prediction-promotion-readiness-blocked-awaiting-source-evidence";

var result = new
{
    phaseId = "phase211-boson-prediction-promotion-readiness-gate",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    promotionAttemptReady,
    predictionSetComplete,
    allKnownBosonValuesDefensible,
    completionAuditPassed,
    objectiveAchieved,
    rerunPromotionAllowed,
    anyEvidenceReadyForApplication,
    defensibleValueCount,
    failedAttemptCount,
    blockedValueCount,
    blockerMatrixReady,
    existingEvidenceFound,
    wzMissingFieldCount,
    higgsMissingFieldCount,
    checks,
    decision = predictionSetComplete
        ? "All known boson values are defensible and the objective is complete."
        : promotionAttemptReady
            ? "New source-lineage evidence is ready for downstream promotion rerun. Run the generator and require P192/P193/P202/P101 to decide completion."
            : "Do not claim new W/Z absolute or Higgs values and do not run promotion as if new source evidence were available. Current defensible values remain limited to the manifest rows until Phase201/P210 are satisfied.",
    requiredNextAction = predictionSetComplete
        ? "No further promotion work is required."
        : promotionAttemptReady
            ? "Rerun the full generator and inspect P192/P193/P202/P101."
            : "Fill the Phase201 source-lineage intake templates using the P209 evidence requests; then rerun P210.",
    sourceEvidence = new
    {
        phase101Path = Phase101Path,
        phase202Path = Phase202Path,
        phase203Path = Phase203Path,
        phase210Path = Phase210Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_prediction_promotion_readiness_gate.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_prediction_promotion_readiness_gate_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.promotionAttemptReady,
        result.predictionSetComplete,
        result.allKnownBosonValuesDefensible,
        result.completionAuditPassed,
        result.objectiveAchieved,
        result.rerunPromotionAllowed,
        result.anyEvidenceReadyForApplication,
        result.defensibleValueCount,
        result.failedAttemptCount,
        result.blockedValueCount,
        result.blockerMatrixReady,
        result.existingEvidenceFound,
        result.wzMissingFieldCount,
        result.higgsMissingFieldCount,
        result.checks,
        result.decision,
        result.requiredNextAction,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"promotionAttemptReady={promotionAttemptReady}");
Console.WriteLine($"predictionSetComplete={predictionSetComplete}");
Console.WriteLine($"defensibleValueCount={defensibleValueCount}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record Check(string CheckId, bool Passed, string Detail);
