using System.Text.Json;

const string DefaultOutputDir = "studies/phase161_post_rule_boson_prediction_attempt_001/output";
const string Phase122Path = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase135Path = "studies/phase135_corrected_wz_sweep_readiness_gate_001/output/corrected_wz_sweep_readiness_gate.json";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";
const string Phase160Path = "studies/phase160_phase_sensitive_transition_rule_materialization_001/output/phase_sensitive_transition_rule_materialization.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE161_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase122 = JsonDocument.Parse(File.ReadAllText(Phase122Path));
using var phase135 = JsonDocument.Parse(File.ReadAllText(Phase135Path));
using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));
using var phase160 = JsonDocument.Parse(File.ReadAllText(Phase160Path));

bool rulePromotable = JsonBool(phase160.RootElement, "transitionRulePromotable") is true;
bool readinessOpen = JsonBool(phase135.RootElement, "rerunReady") is true;
bool projectionCandidateFound = !IsNull(phase122.RootElement, "projectionCandidate");
int validatedPredictionCount = JsonInt(phase151.RootElement, "validatedPredictionCount") ?? 0;
int failedAttemptCount = JsonInt(phase151.RootElement, "failedAttemptCount") ?? 0;
int blockedRowCount = JsonInt(phase151.RootElement, "blockedRowCount") ?? 0;
bool allRowsValidated = JsonBool(phase151.RootElement, "allRowsValidated") is true;

string terminalStatus = allRowsValidated
    ? "post-rule-boson-prediction-attempt-all-rows-validated"
    : rulePromotable && readinessOpen && !projectionCandidateFound
        ? "post-rule-boson-prediction-attempt-no-new-promotions-p122-projection-blocked"
        : rulePromotable && readinessOpen
            ? "post-rule-boson-prediction-attempt-completed-partial"
            : "post-rule-boson-prediction-attempt-readiness-blocked";

var result = new
{
    phaseId = "phase161-post-rule-boson-prediction-attempt",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    transitionRuleStatus = new
    {
        phase160Status = JsonString(phase160.RootElement, "terminalStatus"),
        rulePromotable,
        selectedBosonModeId = JsonString(phase160.RootElement, "selectedBosonModeId"),
        gates = phase160.RootElement.GetProperty("gates").Clone(),
    },
    readinessStatus = new
    {
        phase135Status = JsonString(phase135.RootElement, "terminalStatus"),
        readinessOpen,
        intakeTransitionRuleReady = JsonBool(phase135.RootElement, "intakeTransitionRuleReady"),
    },
    correctedProjectionStatus = new
    {
        phase122Status = JsonString(phase122.RootElement, "terminalStatus"),
        projectionCandidateFound,
        strongestTransitionId = JsonString(phase122.RootElement.GetProperty("strongest"), "transitionId"),
        strongestMinRawToTargetRatio = JsonDouble(phase122.RootElement.GetProperty("strongest"), "minRawToTargetRatio"),
        bestBridgeTransitionId = JsonString(phase122.RootElement.GetProperty("bestBridge"), "transitionId"),
        bestBridgeSpread = JsonDouble(phase122.RootElement.GetProperty("bestBridge"), "commonBridgeRelativeSpread"),
    },
    predictionState = new
    {
        phase151Status = JsonString(phase151.RootElement, "terminalStatus"),
        allRowsValidated,
        knownBosonRowCount = JsonInt(phase151.RootElement, "knownBosonRowCount"),
        validatedPredictionCount,
        failedAttemptCount,
        blockedRowCount,
        validatedPredictions = phase151.RootElement.GetProperty("validatedPredictions").Clone(),
        failedAttempts = phase151.RootElement.GetProperty("failedAttempts").Clone(),
        blockedRows = phase151.RootElement.GetProperty("blockedRows").Clone(),
    },
    conclusion = projectionCandidateFound
        ? "The transition rule is accepted and P122 has a projection candidate; downstream W/Z absolute projection should be rerun for promotion review."
        : "The transition rule is accepted and the prediction generator was rerun, but no new boson values can be promoted because P122 still lacks a corrected W/Z projection candidate.",
    nextWork = projectionCandidateFound
        ? "rerun Phase116/Phase150/Phase151 using the P122 projection candidate"
        : "revise the W/Z bridge or corrected-operator projection construction so the accepted phase-sensitive transition rule is used by P122 instead of remaining only an intake/readiness rule",
    sourceEvidence = new
    {
        phase122Path = Phase122Path,
        phase135Path = Phase135Path,
        phase151Path = Phase151Path,
        phase160Path = Phase160Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "post_rule_boson_prediction_attempt.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "post_rule_boson_prediction_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.transitionRuleStatus,
        result.readinessStatus,
        result.correctedProjectionStatus,
        result.predictionState,
        result.conclusion,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"validatedPredictionCount={validatedPredictionCount}");
Console.WriteLine($"failedAttemptCount={failedAttemptCount}");
Console.WriteLine($"blockedRowCount={blockedRowCount}");
Console.WriteLine($"projectionCandidateFound={projectionCandidateFound}");

static bool IsNull(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
