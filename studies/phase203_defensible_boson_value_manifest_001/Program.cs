using System.Text.Json;

const string DefaultOutputDir = "studies/phase203_defensible_boson_value_manifest_001/output";
const string Phase192Path = "studies/phase192_boson_scientific_defensibility_ledger_001/output/boson_scientific_defensibility_ledger_summary.json";
const string Phase202Path = "studies/phase202_boson_objective_completion_audit_001/output/boson_objective_completion_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE203_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase192 = JsonDocument.Parse(File.ReadAllText(Phase192Path));
using var phase202 = JsonDocument.Parse(File.ReadAllText(Phase202Path));

var defensibleValues = phase192.RootElement.GetProperty("defensibleValues")
    .EnumerateArray()
    .Select(row => new BosonValueRow(
        ParticleId: JsonString(row, "particleId"),
        ObservableId: JsonString(row, "observableId"),
        Status: "defensible-promoted",
        PredictedValue: JsonDouble(row, "predictedValue"),
        PredictedUncertainty: JsonDouble(row, "predictedUncertainty"),
        TargetValue: JsonDouble(row, "targetValue"),
        TargetUncertainty: JsonDouble(row, "targetUncertainty"),
        Unit: JsonString(row, "unit"),
        PullOrSigmaResidual: JsonDouble(row, "pullOrSigmaResidual"),
        GatePromoted: JsonBool(row, "gatePromoted"),
        EvidencePath: Phase192Path,
        Reason: "Predicted row passed target/readiness gates and is promoted in Phase192."))
    .ToArray();

var failedAttempts = phase192.RootElement.GetProperty("failedAttempts")
    .EnumerateArray()
    .Select(row => new BosonValueRow(
        ParticleId: JsonString(row, "particleId"),
        ObservableId: JsonString(row, "observableId"),
        Status: "not-defensible-failed-comparison",
        PredictedValue: JsonDouble(row, "predictedValue"),
        PredictedUncertainty: JsonDouble(row, "predictedUncertainty"),
        TargetValue: JsonDouble(row, "targetValue"),
        TargetUncertainty: JsonDouble(row, "targetUncertainty"),
        Unit: JsonString(row, "unit"),
        PullOrSigmaResidual: JsonDouble(row, "pullOrSigmaResidual"),
        GatePromoted: JsonBool(row, "gatePromoted"),
        EvidencePath: Phase192Path,
        Reason: "A numeric prediction attempt exists, but it failed comparison and is not gate-promoted."))
    .ToArray();

var blockedValues = phase192.RootElement.GetProperty("blockedValues")
    .EnumerateArray()
    .Select(row => new BosonValueRow(
        ParticleId: JsonString(row, "particleId"),
        ObservableId: JsonString(row, "observableId"),
        Status: "not-defensible-blocked",
        PredictedValue: JsonDouble(row, "predictedValue"),
        PredictedUncertainty: JsonDouble(row, "predictedUncertainty"),
        TargetValue: JsonDouble(row, "targetValue"),
        TargetUncertainty: JsonDouble(row, "targetUncertainty"),
        Unit: JsonString(row, "unit"),
        PullOrSigmaResidual: JsonDouble(row, "pullOrSigmaResidual"),
        GatePromoted: JsonBool(row, "gatePromoted"),
        EvidencePath: Phase192Path,
        Reason: "No defensible predicted value exists because required source-lineage gates are missing."))
    .ToArray();

var manifestRows = defensibleValues.Concat(failedAttempts).Concat(blockedValues).ToArray();
var objectiveAchieved = JsonBool(phase202.RootElement, "objectiveAchieved") is true;
var allKnownBosonValuesDefensible = JsonBool(phase192.RootElement, "allKnownBosonValuesDefensible") is true;
var terminalStatus = allKnownBosonValuesDefensible
    ? "defensible-boson-value-manifest-complete"
    : "defensible-boson-value-manifest-partial";

var result = new
{
    phaseId = "phase203-defensible-boson-value-manifest",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    objectiveAchieved,
    allKnownBosonValuesDefensible,
    rowCount = manifestRows.Length,
    defensibleValueCount = defensibleValues.Length,
    failedAttemptCount = failedAttempts.Length,
    blockedValueCount = blockedValues.Length,
    defensibleValues,
    nonDefensibleRows = failedAttempts.Concat(blockedValues).ToArray(),
    manifestRows,
    publicationPolicy = "Only rows with status=defensible-promoted are scientifically defensible values. Failed and blocked rows are retained as explicit non-values to prevent accidental promotion.",
    conclusion = allKnownBosonValuesDefensible
        ? "All known boson rows are defensible."
        : "Current scientifically defensible boson values are limited to the W/Z mass ratio and protected photon/gluon masslessness indicators. W, Z absolute masses and Higgs mass are explicit non-values under current gates.",
    sourceEvidence = new
    {
        phase192Path = Phase192Path,
        phase202Path = Phase202Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "defensible_boson_value_manifest.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "defensible_boson_value_manifest_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.objectiveAchieved,
        result.allKnownBosonValuesDefensible,
        result.rowCount,
        result.defensibleValueCount,
        result.failedAttemptCount,
        result.blockedValueCount,
        result.defensibleValues,
        result.nonDefensibleRows,
        result.publicationPolicy,
        result.conclusion,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"defensibleValueCount={defensibleValues.Length}");
Console.WriteLine($"failedAttemptCount={failedAttempts.Length}");
Console.WriteLine($"blockedValueCount={blockedValues.Length}");
Console.WriteLine($"objectiveAchieved={objectiveAchieved}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record BosonValueRow(
    string? ParticleId,
    string? ObservableId,
    string Status,
    double? PredictedValue,
    double? PredictedUncertainty,
    double? TargetValue,
    double? TargetUncertainty,
    string? Unit,
    double? PullOrSigmaResidual,
    bool? GatePromoted,
    string EvidencePath,
    string Reason);
