using System.Text.Json;

const string DefaultOutputDir = "studies/phase148_all_known_boson_prediction_comparison_001/output";
const string Phase18TargetsPath = "studies/phase18_experimental_targets_001/physical_targets.json";
const string Phase51ReadinessPath = "studies/phase51_broad_boson_prediction_readiness_001/broad_boson_prediction_readiness.json";
const string Phase74AbsoluteComparisonPath = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase147CompletionAttemptPath = "studies/phase147_boson_prediction_completion_attempt_001/output/boson_prediction_completion_attempt.json";
const string Phase177Path = "studies/phase177_massless_benchmark_contracts_001/output/massless_benchmark_contracts.json";
const string Phase183Path = "studies/phase183_massless_sector_invariant_prediction_001/output/massless_sector_invariant_prediction.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE148_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var targetsDoc = JsonDocument.Parse(File.ReadAllText(Phase18TargetsPath));
using var phase51 = JsonDocument.Parse(File.ReadAllText(Phase51ReadinessPath));
using var phase74 = JsonDocument.Parse(File.ReadAllText(Phase74AbsoluteComparisonPath));
using var phase147 = JsonDocument.Parse(File.ReadAllText(Phase147CompletionAttemptPath));
using var phase177 = File.Exists(Phase177Path) ? JsonDocument.Parse(File.ReadAllText(Phase177Path)) : null;
using var phase183 = File.Exists(Phase183Path) ? JsonDocument.Parse(File.ReadAllText(Phase183Path)) : null;

var targets = targetsDoc.RootElement.GetProperty("targets").EnumerateArray()
    .ToDictionary(target => RequiredString(target, "observableId"), target => target.Clone(), StringComparer.Ordinal);
var masslessBenchmarkContracts = phase177 is null
    ? new Dictionary<string, JsonElement>(StringComparer.Ordinal)
    : phase177.RootElement.GetProperty("contracts").EnumerateArray()
        .Where(contract => JsonBool(contract, "benchmarkContractPresent") is true)
        .ToDictionary(contract => RequiredString(contract, "observableId"), contract => contract.Clone(), StringComparer.Ordinal);
var masslessSectorPredictions = phase183 is null || JsonBool(phase183.RootElement, "knownMasslessPredictionAllowed") is not true
    ? new Dictionary<string, JsonElement>(StringComparer.Ordinal)
    : phase183.RootElement.GetProperty("predictions").EnumerateArray()
        .Where(prediction => JsonBool(prediction, "promotionAllowed") is true && JsonBool(prediction, "passed") is true)
        .ToDictionary(prediction => RequiredString(prediction, "observableId"), prediction => prediction.Clone(), StringComparer.Ordinal);
var phase51Records = phase51.RootElement.GetProperty("records").EnumerateArray()
    .ToArray();
var absoluteAttempts = phase74.RootElement.GetProperty("comparisons").EnumerateArray()
    .ToDictionary(comparison => RequiredString(comparison, "observableId"), comparison => comparison.Clone(), StringComparer.Ordinal);

var rows = phase51Records.Select(record =>
{
    string observableId = RequiredString(record, "targetObservableId");
    targets.TryGetValue(observableId, out var target);
    masslessBenchmarkContracts.TryGetValue(observableId, out var benchmarkContract);
    masslessSectorPredictions.TryGetValue(observableId, out var masslessSectorPrediction);
    absoluteAttempts.TryGetValue(observableId, out var failedAttempt);

    bool phase51Predicted = string.Equals(JsonString(record, "readinessStatus"), "predicted", StringComparison.Ordinal);
    bool masslessSectorPredicted = masslessSectorPrediction.ValueKind == JsonValueKind.Object;
    bool hasFailedAttempt = failedAttempt.ValueKind != JsonValueKind.Undefined;
    double? predictedValue = phase51Predicted
        ? JsonDouble(record, "computedValue")
        : masslessSectorPredicted
            ? JsonDouble(masslessSectorPrediction, "predictedValue")
        : hasFailedAttempt
            ? JsonDouble(failedAttempt, "predictedValue")
            : null;
    double? predictedUncertainty = phase51Predicted
        ? JsonDouble(record, "computedUncertainty")
        : masslessSectorPredicted
            ? JsonDouble(masslessSectorPrediction, "predictedUncertainty")
        : hasFailedAttempt
            ? JsonDouble(failedAttempt, "predictedUncertainty")
            : null;
    double? targetValue = target.ValueKind == JsonValueKind.Object
        ? JsonDouble(target, "value")
        : benchmarkContract.ValueKind == JsonValueKind.Object
            ? JsonDouble(benchmarkContract, "targetValue")
        : JsonDouble(record, "targetValue");
    double? targetUncertainty = target.ValueKind == JsonValueKind.Object
        ? JsonDouble(target, "uncertainty")
        : benchmarkContract.ValueKind == JsonValueKind.Object
            ? JsonDouble(benchmarkContract, "targetUncertainty")
        : JsonDouble(record, "targetUncertainty");
    double? pull = phase51Predicted
        ? JsonDouble(record, "pull")
        : masslessSectorPredicted
            ? JsonDouble(masslessSectorPrediction, "pullOrSigmaResidual")
        : hasFailedAttempt
            ? JsonDouble(failedAttempt, "sigmaResidual")
            : null;
    bool? passed = phase51Predicted
        ? true
        : masslessSectorPredicted
            ? JsonBool(masslessSectorPrediction, "passed")
        : hasFailedAttempt
            ? JsonBool(failedAttempt, "passed")
            : null;
    string status = phase51Predicted
        ? "predicted"
        : masslessSectorPredicted
            ? "predicted"
        : hasFailedAttempt
            ? "failed-comparison-attempt-not-promoted"
            : target.ValueKind == JsonValueKind.Object || benchmarkContract.ValueKind == JsonValueKind.Object
                ? "blocked-target-available"
                : "blocked-target-contract-missing";

    string? readinessStatus = masslessSectorPredicted ? "predicted" : JsonString(record, "readinessStatus");
    string? claimGateStatus = masslessSectorPredicted ? "predicted" : JsonString(record, "claimGateStatus");

    return new ComparisonRow(
        ParticleId: RequiredString(record, "particleId"),
        ObservableId: observableId,
        ObservableType: JsonString(record, "physicalObservableType"),
        Status: status,
        PredictedValue: predictedValue,
        PredictedUncertainty: predictedUncertainty,
        TargetValue: targetValue,
        TargetUncertainty: targetUncertainty,
        Unit: target.ValueKind == JsonValueKind.Object
            ? JsonString(target, "unit")
            : benchmarkContract.ValueKind == JsonValueKind.Object
                ? JsonString(benchmarkContract, "unit")
                : null,
        PullOrSigmaResidual: pull,
        Passed: passed,
        ReadinessStatus: readinessStatus,
        ClaimGateStatus: claimGateStatus,
        ClosureRequirements: masslessSectorPredicted ? Array.Empty<string>() : StringArray(record, "closureRequirements"));
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
        phase177Path = File.Exists(Phase177Path) ? Phase177Path : null,
        phase183Path = File.Exists(Phase183Path) ? Phase183Path : null,
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
