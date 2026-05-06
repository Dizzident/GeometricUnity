using System.Text.Json;
using System.Text.Json.Serialization;

const string DefaultOutputDir = "studies/phase119_operator_source_scale_derivation_audit_001/output";
const string Phase118Path = "studies/phase118_wz_matrix_element_normalization_diagnostic_001/output/wz_matrix_element_normalization_diagnostic.json";
const string Phase33Path = "studies/phase33_wz_canonical_operator_normalization_001/canonical_operator_normalization_derivation.json";
const string Phase32Path = "studies/phase33_wz_canonical_operator_normalization_001/operator_normalization_source_audit_with_phase33.json";
const string Phase31Path = "studies/phase33_wz_canonical_operator_normalization_001/wz_normalization_closure_with_phase33.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE119_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase118 = JsonDocument.Parse(File.ReadAllText(Phase118Path));
using var phase33 = JsonDocument.Parse(File.ReadAllText(Phase33Path));
using var phase32 = JsonDocument.Parse(File.ReadAllText(Phase32Path));
using var phase31 = JsonDocument.Parse(File.ReadAllText(Phase31Path));

var repairedReplay = phase118.RootElement.GetProperty("repairedReplay");
double rawRequiredScaleMean = RequiredDouble(repairedReplay, "rawRequiredScaleMean");
double rawRequiredScaleRelativeSpread = RequiredDouble(repairedReplay, "rawRequiredScaleRelativeSpread");
double generatorScaleRequiredMean = RequiredDouble(repairedReplay, "generatorScaleRequiredMean");
double generatorScaleRequiredRelativeSpread = RequiredDouble(repairedReplay, "generatorScaleRequiredRelativeSpread");
double dimensionlessWzScale = RequiredDouble(phase33.RootElement, "dimensionlessWzScale");
double phase31RequiredRatioScale = RequiredDouble(phase31.RootElement, "requiredScaleToTarget");
int phase32PromotableCount = RequiredInt(phase32.RootElement, "promotableSourceCount");

var bestDimensionalCandidate = phase118.RootElement.GetProperty("dimensionalScaleCandidates")
    .EnumerateArray()
    .Select(candidate => new ScaleCandidateAssessment
    {
        CandidateId = RequiredString(candidate, "candidateId"),
        CandidateValue = RequiredDouble(candidate, "candidateValue"),
        RequiredToCandidateRatio = JsonDouble(candidate, "requiredToCandidateRatio"),
        CandidateToRequiredRatio = JsonDouble(candidate, "candidateToRequiredRatio"),
        SourcePath = Phase118Path,
        Promotable = false,
        Classification = "dimension-count-diagnostic-only",
        Blockers =
        [
            "candidate was generated from dimensional counting in Phase118, not from an operator/source derivation artifact",
            "candidate still inherits target-implied required-scale comparison",
        ],
    })
    .MinBy(candidate => Math.Abs((candidate.RequiredToCandidateRatio ?? double.PositiveInfinity) - 1.0));

var phase33Assessment = new ScaleCandidateAssessment
{
    CandidateId = "phase33-shared-internal-mass-operator-unit",
    CandidateValue = dimensionlessWzScale,
    RequiredToCandidateRatio = rawRequiredScaleMean / dimensionlessWzScale,
    CandidateToRequiredRatio = dimensionlessWzScale / rawRequiredScaleMean,
    SourcePath = Phase33Path,
    Promotable = false,
    Classification = "ratio-normalization-not-amplitude-scale",
    Blockers =
    [
        "Phase33 derives a dimensionless W/Z shared-operator ratio scale, not a connection-mode-to-Dirac-variation amplitude scale",
        "scale 1 leaves the Phase118 repaired matrix element low by the required amplitude scale",
    ],
};

var targetRatioAssessment = new ScaleCandidateAssessment
{
    CandidateId = "phase31-target-required-ratio-scale",
    CandidateValue = phase31RequiredRatioScale,
    RequiredToCandidateRatio = rawRequiredScaleMean / phase31RequiredRatioScale,
    CandidateToRequiredRatio = phase31RequiredRatioScale / rawRequiredScaleMean,
    SourcePath = Phase31Path,
    Promotable = false,
    Classification = "target-comparison-diagnostic-only",
    Blockers =
    [
        "Phase31 scale is the W/Z ratio target-required miss, not an independently derived amplitude scale",
        "target-derived scales are not promotable for a prediction replay",
    ],
};

var commonScaleAcceptance = new
{
    requiredRawScaleMean = rawRequiredScaleMean,
    requiredRawScaleRelativeSpread = rawRequiredScaleRelativeSpread,
    requiredGeneratorScaleMean = generatorScaleRequiredMean,
    requiredGeneratorScaleRelativeSpread = generatorScaleRequiredRelativeSpread,
    spreadTolerance = 0.05,
    passesCommonScaleGate = rawRequiredScaleRelativeSpread <= 0.05,
    note = "A target-independent shared amplitude scale must be common to W and Z before absolute projection replay.",
};

var promotableAmplitudeScaleFound = false;
var terminalStatus = "operator-source-scale-derivation-audit-blocked";
var closureRequirements = new[]
{
    "derive the connection-mode-to-Dirac-variation amplitude measure from the analytic Dirac operator discretization",
    "materialize that derivation as a target-independent operator/source-scale artifact with a scale value, unit convention, and source-mode scope",
    "validate the derived scale on W and Z without using physical W/Z target values; require common-scale relative spread at or below 5 percent",
    "rerun Phase115 and Phase116 only after the derived artifact is promotable for matrix-element amplitude projection",
};

var result = new
{
    phaseId = "phase119-operator-source-scale-derivation-audit",
    terminalStatus,
    promotableAmplitudeScaleFound,
    upstreamStatus = new
    {
        phase118TerminalStatus = JsonString(phase118.RootElement, "terminalStatus"),
        phase33TerminalStatus = JsonString(phase33.RootElement, "terminalStatus"),
        phase32TerminalStatus = JsonString(phase32.RootElement, "terminalStatus"),
        phase31TerminalStatus = JsonString(phase31.RootElement, "terminalStatus"),
        phase32PromotableSourceCount = phase32PromotableCount,
    },
    amplitudeScaleRequirement = commonScaleAcceptance,
    existingNormalizationLineage = new
    {
        phase33OperatorNormalizationDerivationId = JsonString(phase33.RootElement, "operatorNormalizationDerivationId"),
        phase33NormalizationConvention = JsonString(phase33.RootElement, "normalizationConvention"),
        phase33DimensionlessWzScale = dimensionlessWzScale,
        phase33TargetIndependent = JsonBool(phase33.RootElement, "targetIndependent"),
        phase31RequiredScaleToWzRatioTarget = phase31RequiredRatioScale,
        phase31DerivationBackedScaleAvailable = JsonBool(phase31.RootElement, "derivationBackedScaleAvailable"),
    },
    scaleAssessments = new[]
    {
        phase33Assessment,
        targetRatioAssessment,
        bestDimensionalCandidate!,
    },
    decision = new
    {
        acceptedScaleId = (string?)null,
        reason = "No existing target-independent artifact derives the matrix-element amplitude scale required by Phase118.",
        targetScaleUseAllowed = false,
        projectionRerunAllowed = false,
    },
    diagnosis = new[]
    {
        "Phase33 repairs only the dimensionless shared W/Z operator-unit convention; it is not the missing amplitude lift.",
        $"Phase118 requires raw amplitude scale mean {rawRequiredScaleMean:R} with W/Z relative spread {rawRequiredScaleRelativeSpread:R}.",
        "The closest Phase118 dimensional candidate is numerically suggestive but is not derivation-backed.",
        "The current blocker is therefore analytic/operator-source scale derivation, not pair selection, vector unit norms, or canonical SU(2) generator normalization.",
    },
    closureRequirements,
    sourceEvidence = new
    {
        phase118Path = Phase118Path,
        phase33Path = Phase33Path,
        phase32Path = Phase32Path,
        phase31Path = Phase31Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
var resultPath = Path.Combine(outputDir, "operator_source_scale_derivation_audit.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "operator_source_scale_derivation_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        terminalStatus,
        promotableAmplitudeScaleFound,
        rawRequiredScaleMean,
        rawRequiredScaleRelativeSpread,
        phase33DimensionlessWzScale = dimensionlessWzScale,
        bestDimensionalCandidateId = bestDimensionalCandidate?.CandidateId,
        bestDimensionalCandidateValue = bestDimensionalCandidate?.CandidateValue,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"promotableAmplitudeScaleFound={promotableAmplitudeScaleFound}");
Console.WriteLine($"rawRequiredScaleMean={rawRequiredScaleMean:R}");
Console.WriteLine($"rawRequiredScaleRelativeSpread={rawRequiredScaleRelativeSpread:R}");
Console.WriteLine($"resultPath={resultPath}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property {propertyName}");

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property {propertyName}");

static int RequiredInt(JsonElement element, string propertyName)
{
    if (element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i))
        return i;

    throw new InvalidDataException($"Missing integer property {propertyName}");
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

public sealed class ScaleCandidateAssessment
{
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("candidateValue")]
    public required double CandidateValue { get; init; }

    [JsonPropertyName("requiredToCandidateRatio")]
    public double? RequiredToCandidateRatio { get; init; }

    [JsonPropertyName("candidateToRequiredRatio")]
    public double? CandidateToRequiredRatio { get; init; }

    [JsonPropertyName("sourcePath")]
    public required string SourcePath { get; init; }

    [JsonPropertyName("promotable")]
    public required bool Promotable { get; init; }

    [JsonPropertyName("classification")]
    public required string Classification { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}
