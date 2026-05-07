using System.Globalization;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase151_validated_boson_prediction_generator_001/output";
const string Phase150Path = "studies/phase150_all_boson_prediction_prerequisite_execution_001/output/all_boson_prediction_prerequisite_execution.json";
const string Phase149Path = "studies/phase149_known_boson_predictability_contracts_001/output/known_boson_predictability_contracts.json";
const string GeneratorScriptPath = "scripts/generate_validated_boson_predictions.sh";

var outputDir = Environment.GetEnvironmentVariable("PHASE151_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase150 = JsonDocument.Parse(File.ReadAllText(Phase150Path));
using var phase149 = JsonDocument.Parse(File.ReadAllText(Phase149Path));

var rows = phase150.RootElement.GetProperty("predictionRows").EnumerateArray()
    .Select(row => new PredictionRow(
        ParticleId: RequiredString(row, "particleId"),
        ObservableId: RequiredString(row, "observableId"),
        Status: RequiredString(row, "status"),
        PredictedValue: JsonDouble(row, "predictedValue"),
        PredictedUncertainty: JsonDouble(row, "predictedUncertainty"),
        TargetValue: JsonDouble(row, "targetValue"),
        TargetUncertainty: JsonDouble(row, "targetUncertainty"),
        Unit: JsonString(row, "unit"),
        PullOrSigmaResidual: JsonDouble(row, "pullOrSigmaResidual"),
        Passed: JsonBool(row, "passed"),
        PromotionAllowed: JsonBool(row, "promotionAllowed") is true,
        ClosureRequirements: StringArray(row, "closureRequirements")))
    .ToArray();

var validatedPredictions = rows.Where(row => row.PromotionAllowed && row.Passed is true).ToArray();
var failedAttempts = rows.Where(row => row.Status == "failed-comparison-attempt-not-promoted").ToArray();
var blockedRows = rows.Where(row => row.Status.StartsWith("blocked-", StringComparison.Ordinal)).ToArray();
bool allRowsValidated = rows.Length > 0 && rows.All(row => row.PromotionAllowed && row.Passed is true);
string terminalStatus = allRowsValidated
    ? "validated-boson-predictions-generated-all-rows"
    : "validated-boson-predictions-generated-partial";

var result = new
{
    phaseId = "phase151-validated-boson-prediction-generator",
    terminalStatus,
    generatorScriptPath = GeneratorScriptPath,
    generatedAt = DateTimeOffset.UtcNow,
    allRowsValidated,
    knownBosonRowCount = rows.Length,
    validatedPredictionCount = validatedPredictions.Length,
    failedAttemptCount = failedAttempts.Length,
    blockedRowCount = blockedRows.Length,
    validatedPredictions,
    failedAttempts,
    blockedRows,
    contractReadiness = new
    {
        phase149Status = JsonString(phase149.RootElement, "terminalStatus"),
        allContractsReady = JsonBool(phase149.RootElement, "allContractsReady"),
        readyContractCount = JsonInt(phase149.RootElement, "readyContractCount"),
        openContractCount = JsonInt(phase149.RootElement, "openContractCount"),
    },
    generationInputs = new
    {
        phase150Path = Phase150Path,
        phase149Path = Phase149Path,
    },
    reviewPolicy = new
    {
        validatedRowsOnly = "Rows are validated predictions only when promotionAllowed=true and passed=true.",
        failedAttempts = "Failed comparison attempts are preserved but are not promoted as predictions.",
        blockedRows = "Blocked rows must receive new target-independent source/identity/benchmark evidence before prediction.",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(
    Path.Combine(outputDir, "validated_boson_predictions.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "validated_boson_predictions_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.generatorScriptPath,
        result.allRowsValidated,
        result.knownBosonRowCount,
        result.validatedPredictionCount,
        result.failedAttemptCount,
        result.blockedRowCount,
        result.validatedPredictions,
        result.failedAttempts,
        result.blockedRows,
        result.contractReadiness,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "validated_boson_predictions.md"),
    BuildMarkdown(terminalStatus, rows, validatedPredictions, failedAttempts, blockedRows));

Console.WriteLine(terminalStatus);
Console.WriteLine($"validatedPredictionCount={validatedPredictions.Length}");
Console.WriteLine($"failedAttemptCount={failedAttempts.Length}");
Console.WriteLine($"blockedRowCount={blockedRows.Length}");

static string BuildMarkdown(
    string terminalStatus,
    IReadOnlyList<PredictionRow> rows,
    IReadOnlyList<PredictionRow> validatedPredictions,
    IReadOnlyList<PredictionRow> failedAttempts,
    IReadOnlyList<PredictionRow> blockedRows)
{
    var builder = new StringBuilder();
    builder.AppendLine("# Validated Boson Predictions");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    builder.AppendLine();
    builder.AppendLine($"Validated predictions: {validatedPredictions.Count}");
    builder.AppendLine($"Failed non-promotable attempts: {failedAttempts.Count}");
    builder.AppendLine($"Blocked rows: {blockedRows.Count}");
    builder.AppendLine();
    builder.AppendLine("| Particle | Observable | Status | Prediction | Target | Result |");
    builder.AppendLine("|---|---|---|---:|---:|---|");
    foreach (var row in rows)
    {
        builder.Append("| ");
        builder.Append(Escape(row.ParticleId));
        builder.Append(" | ");
        builder.Append(Escape(row.ObservableId));
        builder.Append(" | ");
        builder.Append(Escape(row.Status));
        builder.Append(" | ");
        builder.Append(FormatValue(row.PredictedValue, row.PredictedUncertainty, row.Unit));
        builder.Append(" | ");
        builder.Append(FormatValue(row.TargetValue, row.TargetUncertainty, row.Unit));
        builder.Append(" | ");
        builder.Append(FormatResult(row));
        builder.AppendLine(" |");
    }

    if (blockedRows.Count > 0)
    {
        builder.AppendLine();
        builder.AppendLine("## Blocked Rows");
        foreach (var row in blockedRows)
        {
            builder.AppendLine();
            builder.AppendLine($"### {row.ParticleId} `{row.ObservableId}`");
            foreach (string requirement in row.ClosureRequirements)
                builder.AppendLine($"- {requirement}");
        }
    }

    return builder.ToString();
}

static string FormatValue(double? value, double? uncertainty, string? unit)
{
    if (value is null)
        return "none";

    string text = FormatDouble(value.Value);
    if (uncertainty is not null)
        text += " +/- " + FormatDouble(uncertainty.Value);
    if (!string.IsNullOrWhiteSpace(unit))
        text += " " + unit;
    return text;
}

static string FormatResult(PredictionRow row)
{
    if (row.PromotionAllowed && row.Passed is true)
        return row.PullOrSigmaResidual is null ? "validated" : $"validated, pull {FormatDouble(row.PullOrSigmaResidual.Value)}";
    if (row.Passed is false)
        return row.PullOrSigmaResidual is null ? "failed, not promoted" : $"failed, {FormatDouble(row.PullOrSigmaResidual.Value)} sigma";
    return "blocked";
}

static string FormatDouble(double value) => value.ToString("G12", CultureInfo.InvariantCulture);
static string Escape(string value) => value.Replace("|", "\\|", StringComparison.Ordinal);
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

sealed record PredictionRow(
    string ParticleId,
    string ObservableId,
    string Status,
    double? PredictedValue,
    double? PredictedUncertainty,
    double? TargetValue,
    double? TargetUncertainty,
    string? Unit,
    double? PullOrSigmaResidual,
    bool? Passed,
    bool PromotionAllowed,
    IReadOnlyList<string> ClosureRequirements);
