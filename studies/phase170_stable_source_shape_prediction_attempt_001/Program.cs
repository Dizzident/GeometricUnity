using System.Text.Json;

const string DefaultOutputDir = "studies/phase170_stable_source_shape_prediction_attempt_001/output";
const string Phase118Path = "studies/phase118_wz_matrix_element_normalization_diagnostic_001/output/wz_matrix_element_normalization_diagnostic.json";
const string Phase166Path = "studies/phase166_source_normalized_wz_prediction_attempt_001/output/source_normalized_wz_prediction_attempt.json";
const string Phase169Path = "studies/phase169_source_shape_law_stability_experiment_001/output/source_shape_law_stability_experiment.json";
const string WModePath = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/bg-phase12-bg-a-20260315212202-mode-0.json";
const string ZModePath = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/bg-phase12-bg-a-20260315212202-mode-2.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE170_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase118 = JsonDocument.Parse(File.ReadAllText(Phase118Path));
using var phase166 = JsonDocument.Parse(File.ReadAllText(Phase166Path));
using var phase169 = JsonDocument.Parse(File.ReadAllText(Phase169Path));
using var wModeDoc = JsonDocument.Parse(File.ReadAllText(WModePath));
using var zModeDoc = JsonDocument.Parse(File.ReadAllText(ZModePath));

double globalScale = phase166.RootElement.GetProperty("normalizationCandidate").GetProperty("candidateValue").GetDouble();
double targetRaw = phase166.RootElement.GetProperty("targetRaw").GetDouble();
double rawGateRatio = 0.95;
double commonScaleSpreadTolerance = 0.05;
double sigmaThreshold = 5.0;

var stableLawIds = phase169.RootElement.GetProperty("lawAssessments").EnumerateArray()
    .Where(law => JsonBool(law, "stabilityPassed") is true)
    .Select(law => RequiredString(law, "lawId"))
    .ToHashSet(StringComparer.Ordinal);

var rawRecords = phase118.RootElement.GetProperty("repairedReplay").GetProperty("records").EnumerateArray()
    .ToDictionary(record => RequiredString(record, "particleId"), record => JsonDouble(record, "rawMatrixElementMagnitude") ?? 0.0, StringComparer.Ordinal);
var featureMap = new Dictionary<string, SourceFeatures>(StringComparer.Ordinal)
{
    ["w-boson"] = ExtractFeatures("w-boson", wModeDoc.RootElement),
    ["z-boson"] = ExtractFeatures("z-boson", zModeDoc.RootElement),
};

var allCandidates = new[]
{
    ShapeCandidate("inverse-sqrt-linf", "inverse square-root max absolute source coefficient", features => 1.0 / Math.Sqrt(features.MaxAbs)),
    ShapeCandidate("sqrt-l1", "square-root L1 source coefficient norm", features => Math.Sqrt(features.L1Norm)),
    ShapeCandidate("l1", "L1 source coefficient norm", features => features.L1Norm),
    ShapeCandidate("sqrt-participation", "square-root inverse participation ratio effective support", features => Math.Sqrt(features.ParticipationRatio)),
    ShapeCandidate("sqrt-entropy-participation", "square-root entropy effective support", features => Math.Sqrt(features.EntropyParticipation)),
};
var candidates = allCandidates
    .Where(candidate => stableLawIds.Contains(candidate.CandidateId))
    .ToArray();

var attempts = candidates
    .Select(candidate => Evaluate(candidate, featureMap, rawRecords, globalScale, targetRaw, rawGateRatio, commonScaleSpreadTolerance, sigmaThreshold))
    .OrderBy(attempt => attempt.Gates.PromotionAllowed ? 0 : 1)
    .ThenBy(attempt => attempt.Gates.MaxSigmaResidual)
    .ThenBy(attempt => attempt.Gates.CommonScaleRelativeSpread)
    .ToArray();

var best = attempts.FirstOrDefault();
bool anyPromotable = attempts.Any(attempt => attempt.Gates.PromotionAllowed);
string terminalStatus = anyPromotable
    ? "stable-source-shape-prediction-promotable"
    : attempts.Length == 0
        ? "stable-source-shape-prediction-no-stable-law-candidates"
        : "stable-source-shape-prediction-failed-not-promoted";

var result = new
{
    phaseId = "phase170-stable-source-shape-prediction-attempt",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    stableLawIds = stableLawIds.OrderBy(id => id, StringComparer.Ordinal).ToArray(),
    globalScaleCandidate = phase166.RootElement.GetProperty("normalizationCandidate").Clone(),
    targetRaw,
    attemptCount = attempts.Length,
    promotableAttemptCount = attempts.Count(attempt => attempt.Gates.PromotionAllowed),
    bestAttempt = best,
    attempts,
    diagnosis = anyPromotable
        ? "A P169-stable source-shape law passes the W/Z prediction promotion gates."
        : "No P169-stable source-shape law passes the W/Z target-comparison and derivation gates.",
    nextWork = anyPromotable
        ? "wire the stable source-shape prediction into Phase148-151 promotion flow"
        : "derive a new W/Z bridge source or an analytic source-shape law that is both stable and target-validating",
    sourceEvidence = new
    {
        phase118Path = Phase118Path,
        phase166Path = Phase166Path,
        phase169Path = Phase169Path,
        wModePath = WModePath,
        zModePath = ZModePath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "stable_source_shape_prediction_attempt.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "stable_source_shape_prediction_attempt_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.stableLawIds,
        result.attemptCount,
        result.promotableAttemptCount,
        result.bestAttempt,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"stableLawCount={stableLawIds.Count}");
Console.WriteLine($"bestCandidateId={best?.CandidateId}");
Console.WriteLine($"bestMaxSigmaResidual={best?.Gates.MaxSigmaResidual:R}");
Console.WriteLine($"promotionAllowed={best?.Gates.PromotionAllowed}");

static ShapeCandidateRecord ShapeCandidate(string candidateId, string rationale, Func<SourceFeatures, double> factor) =>
    new(candidateId, rationale, factor, DerivationBacked: false);

static ShapeAttempt Evaluate(
    ShapeCandidateRecord candidate,
    IReadOnlyDictionary<string, SourceFeatures> featureMap,
    IReadOnlyDictionary<string, double> rawRecords,
    double globalScale,
    double targetRaw,
    double rawGateRatio,
    double commonScaleSpreadTolerance,
    double sigmaThreshold)
{
    var rawFactors = featureMap.ToDictionary(entry => entry.Key, entry => candidate.Factor(entry.Value), StringComparer.Ordinal);
    double geometricMean = Math.Sqrt(rawFactors["w-boson"] * rawFactors["z-boson"]);
    var rows = rawRecords
        .Select(entry =>
        {
            double normalizedShapeFactor = rawFactors[entry.Key] / geometricMean;
            double scaledRaw = entry.Value * globalScale * normalizedShapeFactor;
            double scaledRawToTargetRatio = scaledRaw / targetRaw;
            double targetMass = entry.Key == "w-boson" ? 80.3692 : 91.188;
            double targetUncertainty = entry.Key == "w-boson" ? 0.0133 : 0.002;
            double predictedMass = targetMass * scaledRawToTargetRatio;
            double sigmaResidual = Math.Abs(predictedMass - targetMass) / targetUncertainty;
            return new ShapePredictionAttempt(
                entry.Key,
                entry.Key == "w-boson" ? "physical-w-boson-mass-gev" : "physical-z-boson-mass-gev",
                entry.Value,
                normalizedShapeFactor,
                scaledRaw,
                scaledRawToTargetRatio,
                predictedMass,
                targetMass,
                targetUncertainty,
                sigmaResidual,
                sigmaResidual <= sigmaThreshold);
        })
        .OrderBy(row => row.ParticleId, StringComparer.Ordinal)
        .ToArray();

    double minRatio = rows.Min(row => row.ScaledRawToTargetRatio);
    double maxRatio = rows.Max(row => row.ScaledRawToTargetRatio);
    double meanRatio = rows.Average(row => row.ScaledRawToTargetRatio);
    double spread = (maxRatio - minRatio) / Math.Max(meanRatio, 1e-300);
    bool rawGatePassed = minRatio >= rawGateRatio;
    bool commonScaleGatePassed = spread <= commonScaleSpreadTolerance;
    bool targetComparisonPassed = rows.All(row => row.Passed);
    var gates = new ShapeGates(
        rawGatePassed,
        minRatio,
        commonScaleGatePassed,
        spread,
        targetComparisonPassed,
        rows.Max(row => row.SigmaResidual),
        StabilityPassed: true,
        candidate.DerivationBacked,
        rawGatePassed && commonScaleGatePassed && targetComparisonPassed && candidate.DerivationBacked);
    return new ShapeAttempt(candidate.CandidateId, candidate.Rationale, candidate.DerivationBacked, rows, gates);
}

static SourceFeatures ExtractFeatures(string particleId, JsonElement root)
{
    var vector = root.GetProperty("modeVector").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    var abs = vector.Select(Math.Abs).ToArray();
    double l2 = Math.Sqrt(vector.Sum(x => x * x));
    double l1 = abs.Sum();
    double max = abs.Max();
    int nonzero = abs.Count(x => x > 1e-12);
    var probabilities = vector.Select(x => x * x).ToArray();
    double probabilitySum = probabilities.Sum();
    var normalized = probabilities.Select(p => p / probabilitySum).ToArray();
    double participation = 1.0 / normalized.Sum(p => p * p);
    double entropy = -normalized.Where(p => p > 0).Sum(p => p * Math.Log(p));
    double entropyParticipation = Math.Exp(entropy);
    return new SourceFeatures(particleId, RequiredString(root, "modeId"), vector.Length, nonzero, l1, l2, max, participation, entropyParticipation);
}

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? throw new InvalidDataException($"Missing {propertyName}")
        : throw new InvalidDataException($"Missing {propertyName}");
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record SourceFeatures(
    string ParticleId,
    string ModeId,
    int Length,
    int NonzeroCount,
    double L1Norm,
    double L2Norm,
    double MaxAbs,
    double ParticipationRatio,
    double EntropyParticipation);

sealed record ShapeCandidateRecord(string CandidateId, string Rationale, Func<SourceFeatures, double> Factor, bool DerivationBacked);
sealed record ShapeAttempt(
    string CandidateId,
    string Rationale,
    bool DerivationBacked,
    IReadOnlyList<ShapePredictionAttempt> PredictionAttempts,
    ShapeGates Gates);
sealed record ShapePredictionAttempt(
    string ParticleId,
    string ObservableId,
    double RawMatrixElementMagnitude,
    double NormalizedShapeFactor,
    double ScaledRawMatrixElementMagnitude,
    double ScaledRawToTargetRatio,
    double PredictedValue,
    double TargetValue,
    double TargetUncertainty,
    double SigmaResidual,
    bool Passed);
sealed record ShapeGates(
    bool RawGatePassed,
    double MinScaledRawToTargetRatio,
    bool CommonScaleGatePassed,
    double CommonScaleRelativeSpread,
    bool TargetComparisonPassed,
    double MaxSigmaResidual,
    bool StabilityPassed,
    bool DerivationBacked,
    bool PromotionAllowed);
