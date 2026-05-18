using System.Text.Json;

const string DefaultOutputDir = "studies/phase185_wz_operator_unit_scale_materialization_001/output";
const string Phase118Path = "studies/phase118_wz_matrix_element_normalization_diagnostic_001/output/wz_matrix_element_normalization_diagnostic.json";
const string Phase120Path = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency.json";
const string Phase166Path = "studies/phase166_source_normalized_wz_prediction_attempt_001/output/source_normalized_wz_prediction_attempt.json";
const string Phase170Path = "studies/phase170_stable_source_shape_prediction_attempt_001/output/stable_source_shape_prediction_attempt.json";
const string GeometryPath = "studies/phase12_joined_calculation_001/output/background_family/manifest/geometry.json";
const string SpinorPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/spinor_representation.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE185_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase118 = JsonDocument.Parse(File.ReadAllText(Phase118Path));
using var phase120 = JsonDocument.Parse(File.ReadAllText(Phase120Path));
using var phase166 = JsonDocument.Parse(File.ReadAllText(Phase166Path));
using var phase170 = JsonDocument.Parse(File.ReadAllText(Phase170Path));
using var geometry = JsonDocument.Parse(File.ReadAllText(GeometryPath));
using var spinor = JsonDocument.Parse(File.ReadAllText(SpinorPath));

int ambientFaceCount = RequiredInt(geometry.RootElement.GetProperty("ambientSpace"), "faceCount");
int ambientEdgeCount = RequiredInt(geometry.RootElement.GetProperty("ambientSpace"), "edgeCount");
int gaugeDimension = 3;
int spinorComponents = RequiredInt(spinor.RootElement, "spinorComponents");
int complexFermionDofFromMesh = ambientFaceCount * gaugeDimension * spinorComponents + ambientEdgeCount * gaugeDimension * spinorComponents;
int fermionCoefficientLength = phase118.RootElement.GetProperty("vectorNorms").GetProperty("selectedFermionVectors").EnumerateArray()
    .Select(vector => RequiredInt(vector, "length"))
    .Distinct()
    .Single();
int bosonConnectionVectorLength = phase118.RootElement.GetProperty("vectorNorms").GetProperty("bosonSourceVectors").EnumerateArray()
    .Select(vector => RequiredInt(vector, "length"))
    .Distinct()
    .Single();
double derivedUnitScale = bosonConnectionVectorLength * Math.Sqrt(fermionCoefficientLength);

var p118Candidates = phase118.RootElement.GetProperty("dimensionalScaleCandidates").EnumerateArray()
    .Select(candidate => new DimensionalCandidate(
        RequiredString(candidate, "candidateId"),
        RequiredDouble(candidate, "candidateValue"),
        JsonDouble(candidate, "candidateToRequiredRatio"),
        JsonDouble(candidate, "requiredToCandidateRatio")))
    .ToArray();
var p118MatchingCandidate = p118Candidates.SingleOrDefault(candidate =>
    string.Equals(candidate.CandidateId, "boson-vector-length-times-sqrt-fermion-coefficient-length", StringComparison.Ordinal));

bool analyticVariationMeasureValid = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true
    && JsonBool(phase120.RootElement.GetProperty("validationGate"), "passes") is true;
bool dimensionalConstructionStable = p118MatchingCandidate is not null
    && Math.Abs(p118MatchingCandidate.CandidateValue - derivedUnitScale) <= Math.Max(1e-9, Math.Abs(derivedUnitScale) * 1e-12);
bool targetIndependentConstruction = true;
bool unitScaleArtifactMaterialized = analyticVariationMeasureValid && dimensionalConstructionStable && targetIndependentConstruction;

var p166Gates = phase166.RootElement.GetProperty("gates");
bool p166RawGatePassed = JsonBool(p166Gates, "rawGatePassed") is true;
bool p166CommonScaleGatePassed = JsonBool(p166Gates, "commonScaleGatePassed") is true;
bool p166TargetComparisonPassed = JsonBool(p166Gates, "targetComparisonPassed") is true;
bool p166PromotionAllowed = JsonBool(p166Gates, "promotionAllowed") is true;
bool p170PromotionAllowed = JsonBool(phase170.RootElement, "promotionAllowed") is true
    || string.Equals(JsonString(phase170.RootElement, "terminalStatus"), "stable-source-shape-prediction-promoted", StringComparison.Ordinal);
bool predictiveReplayPromotable = unitScaleArtifactMaterialized
    && p166RawGatePassed
    && p166CommonScaleGatePassed
    && p166TargetComparisonPassed
    && p166PromotionAllowed
    && p170PromotionAllowed;

string terminalStatus = predictiveReplayPromotable
    ? "wz-operator-unit-scale-materialized-prediction-promotable"
    : unitScaleArtifactMaterialized
        ? "wz-operator-unit-scale-materialized-prediction-still-blocked"
        : "wz-operator-unit-scale-materialization-blocked";

var result = new
{
    phaseId = "phase185-wz-operator-unit-scale-materialization",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    derivation = new
    {
        derivationId = "connection-mode-to-dirac-variation-dimensional-unit-scale-v1",
        formula = "ambient connection vector length * sqrt(real fermion coefficient length)",
        ambientFaceCount,
        ambientEdgeCount,
        gaugeDimension,
        spinorComponents,
        complexFermionDofFromMesh,
        fermionCoefficientLength,
        bosonConnectionVectorLength,
        derivedUnitScale,
        targetIndependent = targetIndependentConstruction,
        excludedTargetObservableIds = new[] { "physical-w-boson-mass-gev", "physical-z-boson-mass-gev" },
    },
    p118MatchingCandidate,
    analyticVariationMeasureValid,
    dimensionalConstructionStable,
    unitScaleArtifactMaterialized,
    predictiveReplay = new
    {
        p166RawGatePassed,
        p166CommonScaleGatePassed,
        p166TargetComparisonPassed,
        p166PromotionAllowed,
        p170PromotionAllowed,
        predictiveReplayPromotable,
        p166NormalizationCandidate = phase166.RootElement.GetProperty("normalizationCandidate").Clone(),
        p166Gates = p166Gates.Clone(),
        p170BestAttempt = phase170.RootElement.TryGetProperty("bestAttempt", out var bestAttempt) ? bestAttempt.Clone() : default,
    },
    diagnosis = predictiveReplayPromotable
        ? new[] { "target-independent operator unit scale and downstream W/Z replay gates passed" }
        : new[]
        {
            "the dimensional operator/source unit scale can be materialized target-independently",
            "this artifact does not make W/Z absolute masses promotable because downstream common-scale and/or target comparison gates remain failed",
            "do not replace the failed predictive replay with a target-implied scalar factor",
        },
    nextWork = predictiveReplayPromotable
        ? "rerun the W/Z absolute comparison with this unit-scale artifact"
        : "derive a different bridge/source construction or source-shape law; the dimensional unit-scale lead is now recorded as insufficient for W/Z promotion",
    sourceEvidence = new
    {
        phase118Path = Phase118Path,
        phase120Path = Phase120Path,
        phase166Path = Phase166Path,
        phase170Path = Phase170Path,
        geometryPath = GeometryPath,
        spinorPath = SpinorPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_operator_unit_scale_materialization.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_operator_unit_scale_materialization_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.derivation,
        result.analyticVariationMeasureValid,
        result.dimensionalConstructionStable,
        result.unitScaleArtifactMaterialized,
        result.predictiveReplay,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"derivedUnitScale={derivedUnitScale:R}");
Console.WriteLine($"unitScaleArtifactMaterialized={unitScaleArtifactMaterialized}");
Console.WriteLine($"predictiveReplayPromotable={predictiveReplayPromotable}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int RequiredInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
        ? value
        : throw new InvalidDataException($"Missing integer property '{propertyName}'.");
static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property '{propertyName}'.");
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record DimensionalCandidate(
    string CandidateId,
    double CandidateValue,
    double? CandidateToRequiredRatio,
    double? RequiredToCandidateRatio);
