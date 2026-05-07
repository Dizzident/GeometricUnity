using System.Text.Json;

const string DefaultOutputDir = "studies/phase148_all_known_boson_prediction_comparison_001/output";
const string Phase18TargetsPath = "studies/phase18_experimental_targets_001/physical_targets.json";
const string Phase51ReadinessPath = "studies/phase51_broad_boson_prediction_readiness_001/broad_boson_prediction_readiness.json";
const string Phase74AbsoluteComparisonPath = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase147CompletionAttemptPath = "studies/phase147_boson_prediction_completion_attempt_001/output/boson_prediction_completion_attempt.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE148_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var targetsDoc = JsonDocument.Parse(File.ReadAllText(Phase18TargetsPath));
using var phase51 = JsonDocument.Parse(File.ReadAllText(Phase51ReadinessPath));
using var phase74 = JsonDocument.Parse(File.ReadAllText(Phase74AbsoluteComparisonPath));
using var phase147 = JsonDocument.Parse(File.ReadAllText(Phase147CompletionAttemptPath));

var targets = targetsDoc.RootElement.GetProperty("targets").EnumerateArray()
    .ToDictionary(target => RequiredString(target, "observableId"), target => target.Clone(), StringComparer.Ordinal);
var phase51Records = phase51.RootElement.GetProperty("records").EnumerateArray()
    .ToArray();
var absoluteAttempts = phase74.RootElement.GetProperty("comparisons").EnumerateArray()
    .ToDictionary(comparison => RequiredString(comparison, "observableId"), comparison => comparison.Clone(), StringComparer.Ordinal);

var rows = phase51Records.Select(record =>
{
    string observableId = RequiredString(record, "targetObservableId");
    targets.TryGetValue(observableId, out var target);
    absoluteAttempts.TryGetValue(observableId, out var failedAttempt);

    bool phase51Predicted = string.Equals(JsonString(record, "readinessStatus"), "predicted", StringComparison.Ordinal);
    bool hasFailedAttempt = failedAttempt.ValueKind != JsonValueKind.Undefined;
    double? predictedValue = phase51Predicted
        ? JsonDouble(record, "computedValue")
        : hasFailedAttempt
            ? JsonDouble(failedAttempt, "predictedValue")
            : null;
    double? predictedUncertainty = phase51Predicted
        ? JsonDouble(record, "computedUncertainty")
        : hasFailedAttempt
            ? JsonDouble(failedAttempt, "predictedUncertainty")
            : null;
    double? targetValue = target.ValueKind == JsonValueKind.Object
        ? JsonDouble(target, "value")
        : JsonDouble(record, "targetValue");
    double? targetUncertainty = target.ValueKind == JsonValueKind.Object
        ? JsonDouble(target, "uncertainty")
        : JsonDouble(record, "targetUncertainty");
    double? pull = phase51Predicted
        ? JsonDouble(record, "pull")
        : hasFailedAttempt
            ? JsonDouble(failedAttempt, "sigmaResidual")
            : null;
    bool? passed = phase51Predicted
        ? true
        : hasFailedAttempt
            ? JsonBool(failedAttempt, "passed")
            : null;
    string status = phase51Predicted
        ? "predicted"
        : hasFailedAttempt
            ? "failed-comparison-attempt-not-promoted"
            : target.ValueKind == JsonValueKind.Object
                ? "blocked-target-available"
                : "blocked-target-contract-missing";

    return new ComparisonRow(
        ParticleId: RequiredString(record, "particleId"),
        ObservableId: observableId,
        ObservableType: JsonString(record, "physicalObservableType"),
        Status: status,
        PredictedValue: predictedValue,
        PredictedUncertainty: predictedUncertainty,
        TargetValue: targetValue,
        TargetUncertainty: targetUncertainty,
        Unit: target.ValueKind == JsonValueKind.Object ? JsonString(target, "unit") : null,
        PullOrSigmaResidual: pull,
        Passed: passed,
        ReadinessStatus: JsonString(record, "readinessStatus"),
        ClaimGateStatus: JsonString(record, "claimGateStatus"),
        ClosureRequirements: StringArray(record, "closureRequirements"));
}).ToArray();

var predictedRows = rows.Where(row => row.Status == "predicted").ToArray();
var failedAttemptRows = rows.Where(row => row.Status == "failed-comparison-attempt-not-promoted").ToArray();
var blockedRows = rows.Where(row => row.Status.StartsWith("blocked-", StringComparison.Ordinal)).ToArray();
bool allKnownBosonPredictionsComplete = rows.Length > 0 && rows.All(row => row.Status == "predicted");
string terminalStatus = allKnownBosonPredictionsComplete
    ? "all-known-boson-predictions-complete"
    : "all-known-boson-predictions-partial";

var result = new
{
    phaseId = "phase148-all-known-boson-prediction-comparison",
    terminalStatus,
    allKnownBosonPredictionsComplete,
    knownBosonRowCount = rows.Length,
    predictedCount = predictedRows.Length,
    failedComparisonAttemptCount = failedAttemptRows.Length,
    blockedCount = blockedRows.Length,
    comparisonRows = rows,
    completedPhysicalPredictions = predictedRows,
    failedComparisonAttempts = failedAttemptRows,
    blockedPredictions = blockedRows,
    phase147Completion = new
    {
        terminalStatus = JsonString(phase147.RootElement, "terminalStatus"),
        fullBosonPredictionComplete = JsonBool(phase147.RootElement, "fullBosonPredictionComplete"),
        completedPhysicalPredictionCount = JsonInt(phase147.RootElement, "completedPhysicalPredictionCount"),
    },
    blockers = blockedRows.Select(row => new
    {
        row.ParticleId,
        row.ObservableId,
        row.Status,
        row.ClosureRequirements,
    }).ToArray(),
    sourceEvidence = new
    {
        phase18TargetsPath = Phase18TargetsPath,
        phase51ReadinessPath = Phase51ReadinessPath,
        phase74AbsoluteComparisonPath = Phase74AbsoluteComparisonPath,
        phase147CompletionAttemptPath = Phase147CompletionAttemptPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "all_known_boson_prediction_comparison.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "all_known_boson_prediction_comparison_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.allKnownBosonPredictionsComplete,
        result.knownBosonRowCount,
        result.predictedCount,
        result.failedComparisonAttemptCount,
        result.blockedCount,
        result.comparisonRows,
        result.phase147Completion,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"predictedCount={predictedRows.Length}");
Console.WriteLine($"failedComparisonAttemptCount={failedAttemptRows.Length}");
Console.WriteLine($"blockedCount={blockedRows.Length}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static IReadOnlyList<string> StringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? "")
            .ToArray()
        : Array.Empty<string>();

sealed record ComparisonRow(
    string ParticleId,
    string ObservableId,
    string? ObservableType,
    string Status,
    double? PredictedValue,
    double? PredictedUncertainty,
    double? TargetValue,
    double? TargetUncertainty,
    string? Unit,
    double? PullOrSigmaResidual,
    bool? Passed,
    string? ReadinessStatus,
    string? ClaimGateStatus,
    IReadOnlyList<string> ClosureRequirements);
