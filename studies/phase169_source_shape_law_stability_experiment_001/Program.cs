using System.Text.Json;

const string DefaultOutputDir = "studies/phase169_source_shape_law_stability_experiment_001/output";
const string Phase167Path = "studies/phase167_source_shape_normalized_wz_attempt_001/output/source_shape_normalized_wz_attempt.json";
const string Phase168Path = "studies/phase168_source_shape_scalar_relation_closure_audit_001/output/source_shape_scalar_relation_closure_audit.json";
const string ModeDir = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes";

var outputDir = Environment.GetEnvironmentVariable("PHASE169_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase167 = JsonDocument.Parse(File.ReadAllText(Phase167Path));
using var phase168 = JsonDocument.Parse(File.ReadAllText(Phase168Path));

var backgrounds = new[] { "bg-phase12-bg-a-20260315212202", "bg-phase12-bg-b-20260315212202" };
var backgroundFeatures = backgrounds
    .Select(backgroundId => new BackgroundFeatureSet(
        backgroundId,
        ExtractFeatures("w-boson", Path.Combine(ModeDir, $"{backgroundId}-mode-0.json")),
        ExtractFeatures("z-boson", Path.Combine(ModeDir, $"{backgroundId}-mode-2.json"))))
    .ToArray();

var laws = new[]
{
    ShapeLaw("l1", "L1 source coefficient norm", features => features.L1Norm),
    ShapeLaw("sqrt-l1", "square-root L1 source coefficient norm", features => Math.Sqrt(features.L1Norm)),
    ShapeLaw("inverse-sqrt-linf", "inverse square-root max absolute source coefficient", features => 1.0 / Math.Sqrt(features.MaxAbs)),
    ShapeLaw("entropy-participation", "entropy effective support", features => features.EntropyParticipation),
    ShapeLaw("sqrt-entropy-participation", "square-root entropy effective support", features => Math.Sqrt(features.EntropyParticipation)),
    ShapeLaw("participation", "inverse participation ratio effective support", features => features.ParticipationRatio),
    ShapeLaw("sqrt-participation", "square-root inverse participation ratio effective support", features => Math.Sqrt(features.ParticipationRatio)),
};

double stabilityTolerance = 0.05;
var lawAssessments = laws
    .Select(law => AssessLaw(law, backgroundFeatures, stabilityTolerance))
    .OrderBy(assessment => assessment.RelativeSpread)
    .ThenBy(assessment => assessment.LawId, StringComparer.Ordinal)
    .ToArray();

var p167BestLawId = phase167.RootElement.GetProperty("bestAttempt").GetProperty("candidateId").GetString() ?? "";
var p167LawAssessment = lawAssessments.FirstOrDefault(assessment => assessment.LawId == p167BestLawId);
var bestStableLaw = lawAssessments.FirstOrDefault(assessment => assessment.StabilityPassed);
bool p167BestLawStable = p167LawAssessment?.StabilityPassed is true;
bool derivationBacked = false;
bool downstreamPredictionPromotable = JsonBool(phase167.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "promotionAllowed") is true
    || JsonBool(phase168.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "promotionAllowed") is true;
bool canPromoteShapeLaw = p167BestLawStable && derivationBacked && downstreamPredictionPromotable;

string terminalStatus = canPromoteShapeLaw
    ? "source-shape-law-stability-promotable"
    : p167BestLawStable
        ? "source-shape-law-stability-necessary-evidence-passed-derivation-blocked"
        : "source-shape-law-stability-failed-for-p167-law";

var result = new
{
    phaseId = "phase169-source-shape-law-stability-experiment",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    stabilityTolerance,
    p167BestLawId,
    p167BestLawStable,
    derivationBacked,
    downstreamPredictionPromotable,
    canPromoteShapeLaw,
    bestStableLaw,
    p167LawAssessment,
    lawAssessments,
    backgroundFeatures,
    diagnosis = canPromoteShapeLaw
        ? "The P167 source-shape law has stability, derivation, and downstream prediction evidence."
        : p167BestLawStable
            ? "The P167 L1 source-shape law is stable across sibling Phase12 source backgrounds, but stability alone is not enough: an analytic derivation and downstream prediction validation remain required."
            : "The P167 source-shape law is not stable across sibling Phase12 source backgrounds and cannot be made defensible from this evidence.",
    nextWork = canPromoteShapeLaw
        ? "wire the promotable source-shape law into the boson prediction promotion path"
        : p167BestLawStable
            ? "derive the L1 source-shape normalization analytically and rerun P167/P168 prediction gates without using W/Z target residuals"
            : "derive a different source-shape law or a new W/Z bridge source; do not promote the P167 diagnostic law",
    sourceEvidence = new
    {
        phase167Path = Phase167Path,
        phase168Path = Phase168Path,
        modeDir = ModeDir,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "source_shape_law_stability_experiment.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "source_shape_law_stability_experiment_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.stabilityTolerance,
        result.p167BestLawId,
        result.p167BestLawStable,
        result.derivationBacked,
        result.downstreamPredictionPromotable,
        result.canPromoteShapeLaw,
        result.bestStableLaw,
        result.p167LawAssessment,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"p167BestLawId={p167BestLawId}");
Console.WriteLine($"p167BestLawStable={p167BestLawStable}");
Console.WriteLine($"derivationBacked={derivationBacked}");
Console.WriteLine($"canPromoteShapeLaw={canPromoteShapeLaw}");

static ShapeLawRecord ShapeLaw(string lawId, string rationale, Func<SourceFeatures, double> factor) => new(lawId, rationale, factor);

static ShapeLawAssessment AssessLaw(ShapeLawRecord law, IReadOnlyList<BackgroundFeatureSet> backgrounds, double stabilityTolerance)
{
    var records = backgrounds
        .Select(background =>
        {
            double wFactor = law.Factor(background.WFeatures);
            double zFactor = law.Factor(background.ZFeatures);
            double wzCorrectionRatio = wFactor / zFactor;
            return new ShapeLawBackgroundRecord(background.BackgroundId, wFactor, zFactor, wzCorrectionRatio);
        })
        .ToArray();

    double min = records.Min(record => record.WzCorrectionRatio);
    double max = records.Max(record => record.WzCorrectionRatio);
    double mean = records.Average(record => record.WzCorrectionRatio);
    double spread = (max - min) / Math.Max(Math.Abs(mean), 1e-300);
    return new ShapeLawAssessment(law.LawId, law.Rationale, records, spread, spread <= stabilityTolerance);
}

static SourceFeatures ExtractFeatures(string particleId, string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var root = doc.RootElement;
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
    return new SourceFeatures(
        particleId,
        root.GetProperty("modeId").GetString() ?? Path.GetFileNameWithoutExtension(path),
        vector.Length,
        nonzero,
        l1,
        l2,
        max,
        participation,
        entropyParticipation);
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

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

sealed record BackgroundFeatureSet(string BackgroundId, SourceFeatures WFeatures, SourceFeatures ZFeatures);
sealed record ShapeLawRecord(string LawId, string Rationale, Func<SourceFeatures, double> Factor);
sealed record ShapeLawBackgroundRecord(string BackgroundId, double WFactor, double ZFactor, double WzCorrectionRatio);
sealed record ShapeLawAssessment(
    string LawId,
    string Rationale,
    IReadOnlyList<ShapeLawBackgroundRecord> BackgroundRecords,
    double RelativeSpread,
    bool StabilityPassed);
