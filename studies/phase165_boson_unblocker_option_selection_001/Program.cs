using System.Text.Json;

const string DefaultOutputDir = "studies/phase165_boson_unblocker_option_selection_001/output";
const string Phase118Path = "studies/phase118_wz_matrix_element_normalization_diagnostic_001/output/wz_matrix_element_normalization_diagnostic.json";
const string Phase119Path = "studies/phase119_operator_source_scale_derivation_audit_001/output/operator_source_scale_derivation_audit.json";
const string Phase120Path = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency.json";
const string Phase164Path = "studies/phase164_source_level_wz_bridge_candidate_census_001/output/source_level_wz_bridge_candidate_census.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE165_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase118 = JsonDocument.Parse(File.ReadAllText(Phase118Path));
using var phase119 = JsonDocument.Parse(File.ReadAllText(Phase119Path));
using var phase120 = JsonDocument.Parse(File.ReadAllText(Phase120Path));
using var phase164 = JsonDocument.Parse(File.ReadAllText(Phase164Path));

int existingBridgePromotableCount = JsonInt(phase164.RootElement, "promotableCandidateCount") ?? 0;
bool analyticMeasureReady = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true;
bool sourceScalePreviouslyPromotable = JsonBool(phase119.RootElement, "promotableAmplitudeScaleFound") is true;
var dimensionalCandidates = phase118.RootElement.GetProperty("dimensionalScaleCandidates").EnumerateArray()
    .Select(candidate => new DimensionalCandidate(
        RequiredString(candidate, "candidateId"),
        JsonDouble(candidate, "candidateValue") ?? 0.0,
        JsonDouble(candidate, "requiredToCandidateRatio"),
        JsonDouble(candidate, "candidateToRequiredRatio")))
    .OrderBy(candidate => Math.Abs(Math.Log(Math.Max(candidate.CandidateToRequiredRatio ?? 1e-300, 1e-300))))
    .ToArray();
var bestDimensionalCandidate = dimensionalCandidates.First();

var bridgeSourceOption = new
{
    optionId = "derive-new-wz-bridge-source",
    status = existingBridgePromotableCount > 0 ? "existing-source-candidate-available" : "blocked-no-existing-source-candidate",
    evidence = "P164 scanned existing Phase12 variation matrices.",
    defensibility = existingBridgePromotableCount > 0
        ? "requires identity review before promotion"
        : "cannot progress from existing local candidates; a genuinely new source derivation is required",
    score = existingBridgePromotableCount > 0 ? 2 : 0,
};
var amplitudeNormalizationOption = new
{
    optionId = "test-source-level-amplitude-normalization",
    status = analyticMeasureReady && dimensionalCandidates.Length > 0
        ? "testable-nonpromotional-candidate-available"
        : "blocked-no-testable-normalization-candidate",
    evidence = "P118 materializes dimensional scale candidates and P120 validates the analytic variation measure, while P119 correctly refuses promotion without derivation.",
    defensibility = sourceScalePreviouslyPromotable
        ? "promotable"
        : "testable but not promotable unless common-scale and derivation gates pass",
    score = analyticMeasureReady && dimensionalCandidates.Length > 0 ? 1 : 0,
    bestCandidate = bestDimensionalCandidate,
};

string selectedOptionId = amplitudeNormalizationOption.score > bridgeSourceOption.score
    ? amplitudeNormalizationOption.optionId
    : bridgeSourceOption.optionId;
string terminalStatus = selectedOptionId == "test-source-level-amplitude-normalization"
    ? "boson-unblocker-option-selected-source-level-amplitude-normalization"
    : "boson-unblocker-option-selected-new-bridge-source-required";

var result = new
{
    phaseId = "phase165-boson-unblocker-option-selection",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    selectedOptionId,
    optionAssessment = new
    {
        bridgeSourceOption,
        amplitudeNormalizationOption,
    },
    decisionRationale = selectedOptionId == "test-source-level-amplitude-normalization"
        ? "A new bridge source has no local candidate after P164; source-level amplitude normalization has a concrete target-independent dimensional candidate that can be tested and rejected or accepted by gates."
        : "A bridge source candidate exists and should be identity-audited before any normalization repair is attempted.",
    nextWork = selectedOptionId == "test-source-level-amplitude-normalization"
        ? "run a nonpromotional source-normalized W/Z prediction attempt using the best P118 dimensional candidate and reject promotion unless common-scale and target-comparison gates pass"
        : "derive or identity-audit the W/Z bridge source before rerunning boson predictions",
    sourceEvidence = new
    {
        phase118Path = Phase118Path,
        phase119Path = Phase119Path,
        phase120Path = Phase120Path,
        phase164Path = Phase164Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "boson_unblocker_option_selection.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "boson_unblocker_option_selection_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.selectedOptionId,
        result.optionAssessment,
        result.decisionRationale,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"selectedOptionId={selectedOptionId}");
Console.WriteLine($"bestDimensionalCandidate={bestDimensionalCandidate.CandidateId}");

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? throw new InvalidDataException($"Missing {propertyName}")
        : throw new InvalidDataException($"Missing {propertyName}");
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record DimensionalCandidate(
    string CandidateId,
    double CandidateValue,
    double? RequiredToCandidateRatio,
    double? CandidateToRequiredRatio);
