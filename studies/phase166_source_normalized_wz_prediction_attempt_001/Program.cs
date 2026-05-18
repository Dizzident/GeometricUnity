using System.Text.Json;

const string DefaultOutputDir = "studies/phase166_source_normalized_wz_prediction_attempt_001/output";
const string Phase118Path = "studies/phase118_wz_matrix_element_normalization_diagnostic_001/output/wz_matrix_element_normalization_diagnostic.json";
const string Phase165Path = "studies/phase165_boson_unblocker_option_selection_001/output/boson_unblocker_option_selection.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE166_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase118 = JsonDocument.Parse(File.ReadAllText(Phase118Path));
using var phase165 = JsonDocument.Parse(File.ReadAllText(Phase165Path));

string selectedOption = RequiredString(phase165.RootElement, "selectedOptionId");
double targetRaw = phase118.RootElement.GetProperty("target").GetProperty("targetImpliedRawMatrixElementMagnitude").GetDouble();
double rawGateRatio = 0.95;
double commonScaleSpreadTolerance = 0.05;
double sigmaThreshold = 5.0;

var bestCandidate = phase118.RootElement.GetProperty("dimensionalScaleCandidates").EnumerateArray()
    .Select(candidate => new DimensionalCandidate(
        RequiredString(candidate, "candidateId"),
        JsonDouble(candidate, "candidateValue") ?? 0.0,
        JsonDouble(candidate, "candidateToRequiredRatio")))
    .OrderBy(candidate => Math.Abs(Math.Log(Math.Max(candidate.CandidateToRequiredRatio ?? 1e-300, 1e-300))))
    .First();

var rows = phase118.RootElement.GetProperty("repairedReplay").GetProperty("records").EnumerateArray()
    .Select(record =>
    {
        string particleId = RequiredString(record, "particleId");
        double raw = JsonDouble(record, "rawMatrixElementMagnitude") ?? 0.0;
        double scaledRaw = raw * bestCandidate.CandidateValue;
        double scaledRawToTargetRatio = scaledRaw / targetRaw;
        double targetMass = particleId == "w-boson" ? 80.3692 : 91.188;
        double targetUncertainty = particleId == "w-boson" ? 0.0133 : 0.002;
        double predictedMass = targetMass * scaledRawToTargetRatio;
        double delta = predictedMass - targetMass;
        double sigmaResidual = Math.Abs(delta) / targetUncertainty;
        bool passed = sigmaResidual <= sigmaThreshold;
        return new PredictionAttempt(
            particleId,
            particleId == "w-boson" ? "physical-w-boson-mass-gev" : "physical-z-boson-mass-gev",
            raw,
            scaledRaw,
            scaledRawToTargetRatio,
            predictedMass,
            targetMass,
            targetUncertainty,
            sigmaResidual,
            passed);
    })
    .ToArray();

double minScaledRawToTargetRatio = rows.Min(row => row.ScaledRawToTargetRatio);
double maxScaledRawToTargetRatio = rows.Max(row => row.ScaledRawToTargetRatio);
double meanScaledRawToTargetRatio = rows.Average(row => row.ScaledRawToTargetRatio);
double commonScaleRelativeSpread = (maxScaledRawToTargetRatio - minScaledRawToTargetRatio) / Math.Max(meanScaledRawToTargetRatio, 1e-300);
bool rawGatePassed = minScaledRawToTargetRatio >= rawGateRatio;
bool commonScaleGatePassed = commonScaleRelativeSpread <= commonScaleSpreadTolerance;
bool targetComparisonPassed = rows.All(row => row.Passed);
bool promotionAllowed = selectedOption == "test-source-level-amplitude-normalization"
    && rawGatePassed
    && commonScaleGatePassed
    && targetComparisonPassed;
string terminalStatus = promotionAllowed
    ? "source-normalized-wz-prediction-promotable"
    : "source-normalized-wz-prediction-attempt-failed-not-promoted";

var result = new
{
    phaseId = "phase166-source-normalized-wz-prediction-attempt",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    selectedOption,
    normalizationCandidate = bestCandidate,
    targetRaw,
    gates = new
    {
        rawGateRatio,
        rawGatePassed,
        minScaledRawToTargetRatio,
        commonScaleSpreadTolerance,
        commonScaleRelativeSpread,
        commonScaleGatePassed,
        sigmaThreshold,
        targetComparisonPassed,
        promotionAllowed,
    },
    predictionAttempts = rows,
    diagnosis = promotionAllowed
        ? "The source-level normalization candidate passes raw-amplitude, common-scale, and target-comparison gates."
        : commonScaleGatePassed
            ? "The source-level normalization candidate is not promotable because target comparison fails."
            : "The source-level normalization candidate reaches raw-amplitude scale but fails the W/Z common-scale gate, so it is not a validated boson prediction.",
    nextWork = promotionAllowed
        ? "wire the promotable normalized W/Z predictions into Phase148-151 promotion flow"
        : "derive a source-level amplitude normalization with W/Z common-scale spread at or below 5 percent, or derive a new W/Z bridge source",
    sourceEvidence = new
    {
        phase118Path = Phase118Path,
        phase165Path = Phase165Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "source_normalized_wz_prediction_attempt.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "source_normalized_wz_prediction_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.normalizationCandidate,
        result.gates,
        result.predictionAttempts,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"candidateId={bestCandidate.CandidateId}");
Console.WriteLine($"minScaledRawToTargetRatio={minScaledRawToTargetRatio:R}");
Console.WriteLine($"commonScaleRelativeSpread={commonScaleRelativeSpread:R}");
Console.WriteLine($"promotionAllowed={promotionAllowed}");

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? throw new InvalidDataException($"Missing {propertyName}")
        : throw new InvalidDataException($"Missing {propertyName}");
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record DimensionalCandidate(string CandidateId, double CandidateValue, double? CandidateToRequiredRatio);
sealed record PredictionAttempt(
    string ParticleId,
    string ObservableId,
    double RawMatrixElementMagnitude,
    double ScaledRawMatrixElementMagnitude,
    double ScaledRawToTargetRatio,
    double PredictedValue,
    double TargetValue,
    double TargetUncertainty,
    double SigmaResidual,
    bool Passed);
