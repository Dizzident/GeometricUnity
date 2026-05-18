using System.Text.Json;

const string DefaultOutputDir = "studies/phase186_wz_source_shape_law_closure_001/output";
const string Phase167Path = "studies/phase167_source_shape_normalized_wz_attempt_001/output/source_shape_normalized_wz_attempt.json";
const string Phase169Path = "studies/phase169_source_shape_law_stability_experiment_001/output/source_shape_law_stability_experiment.json";
const string Phase170Path = "studies/phase170_stable_source_shape_prediction_attempt_001/output/stable_source_shape_prediction_attempt.json";
const string Phase185Path = "studies/phase185_wz_operator_unit_scale_materialization_001/output/wz_operator_unit_scale_materialization.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE186_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase167 = JsonDocument.Parse(File.ReadAllText(Phase167Path));
using var phase169 = JsonDocument.Parse(File.ReadAllText(Phase169Path));
using var phase170 = JsonDocument.Parse(File.ReadAllText(Phase170Path));
using var phase185 = JsonDocument.Parse(File.ReadAllText(Phase185Path));

bool unitScaleMaterialized = JsonBool(phase185.RootElement, "unitScaleArtifactMaterialized") is true;
bool p185ReplayPromotable = phase185.RootElement.TryGetProperty("predictiveReplay", out var p185Replay)
    && JsonBool(p185Replay, "predictiveReplayPromotable") is true;
bool p167HasCommonScalePassing = (JsonInt(phase167.RootElement, "commonScalePassingAttemptCount") ?? 0) > 0;
bool p167HasPromotableAttempt = (JsonInt(phase167.RootElement, "promotableAttemptCount") ?? 0) > 0;
bool p167BestDerivationBacked = JsonBool(phase167.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "derivationBacked") is true;
bool p169BestLawStable = phase169.RootElement.TryGetProperty("bestStableLaw", out var bestStableLaw)
    && JsonBool(bestStableLaw, "stabilityPassed") is true;
bool p169CanPromoteShapeLaw = JsonBool(phase169.RootElement, "canPromoteShapeLaw") is true;
bool p170HasPromotableAttempt = (JsonInt(phase170.RootElement, "promotableAttemptCount") ?? 0) > 0;
bool p170BestStable = JsonBool(phase170.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "stabilityPassed") is true;
bool p170BestDerivationBacked = JsonBool(phase170.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "derivationBacked") is true;
bool p170CommonScalePassed = JsonBool(phase170.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "commonScaleGatePassed") is true;
bool p170TargetComparisonPassed = JsonBool(phase170.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "targetComparisonPassed") is true;
bool p170PromotionAllowed = JsonBool(phase170.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "promotionAllowed") is true;

var checks = new[]
{
    new ClosureCheck("operator-unit-scale-materialized", unitScaleMaterialized, "P185 materialized the dimensional unit-scale artifact."),
    new ClosureCheck("unit-scale-replay-promotable", p185ReplayPromotable, "P185 replay must pass downstream W/Z gates before it can promote absolute masses."),
    new ClosureCheck("p167-common-scale-candidate", p167HasCommonScalePassing, "At least one P167 source-shape diagnostic passes common-scale only."),
    new ClosureCheck("p167-promotable-candidate", p167HasPromotableAttempt, "No P167 source-shape diagnostic passes full promotion."),
    new ClosureCheck("p167-derivation-backed", p167BestDerivationBacked, "P167 best source-shape law is diagnostic, not derivation-backed."),
    new ClosureCheck("p169-stable-law-present", p169BestLawStable, "P169 found stable laws, but not a promotable derivation-backed predictive law."),
    new ClosureCheck("p169-shape-law-promotable", p169CanPromoteShapeLaw, "P169 cannot promote the source-shape law."),
    new ClosureCheck("p170-promotable-attempt", p170HasPromotableAttempt, "P170 has no stable source-shape prediction attempt that passes target comparison and derivation gates."),
    new ClosureCheck("p170-best-stable", p170BestStable, "P170 best attempt is stable."),
    new ClosureCheck("p170-best-derivation-backed", p170BestDerivationBacked, "P170 best attempt is not derivation-backed."),
    new ClosureCheck("p170-common-scale", p170CommonScalePassed, "P170 best attempt still fails the common-scale gate."),
    new ClosureCheck("p170-target-comparison", p170TargetComparisonPassed, "P170 best attempt fails physical target comparison."),
};

bool sourceShapeLawPromotable = unitScaleMaterialized
    && p185ReplayPromotable
    && p169CanPromoteShapeLaw
    && p170HasPromotableAttempt
    && p170BestStable
    && p170BestDerivationBacked
    && p170CommonScalePassed
    && p170TargetComparisonPassed
    && p170PromotionAllowed;
string terminalStatus = sourceShapeLawPromotable
    ? "wz-source-shape-law-closure-promotable"
    : "wz-source-shape-law-closure-blocked";

var result = new
{
    phaseId = "phase186-wz-source-shape-law-closure",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    sourceShapeLawPromotable,
    checks,
    p167BestAttempt = phase167.RootElement.GetProperty("bestAttempt").Clone(),
    p169BestStableLaw = phase169.RootElement.TryGetProperty("bestStableLaw", out var stableLaw) ? stableLaw.Clone() : default,
    p170BestAttempt = phase170.RootElement.GetProperty("bestAttempt").Clone(),
    decision = sourceShapeLawPromotable
        ? "A derivation-backed stable source-shape law is promotable; rerun W/Z absolute comparison."
        : "Do not promote W/Z absolute masses from existing source-shape laws. The remaining path requires a new derivation-backed bridge/source law outside the exhausted diagnostics.",
    nextRequiredArtifact = "A derivation-backed W/Z bridge/source law that is stable across sibling backgrounds and passes common-scale and target-comparison gates without W/Z target-fitted factors.",
    sourceEvidence = new
    {
        phase167Path = Phase167Path,
        phase169Path = Phase169Path,
        phase170Path = Phase170Path,
        phase185Path = Phase185Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_source_shape_law_closure.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_source_shape_law_closure_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.sourceShapeLawPromotable,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"sourceShapeLawPromotable={sourceShapeLawPromotable}");
Console.WriteLine($"p167HasCommonScalePassing={p167HasCommonScalePassing}");
Console.WriteLine($"p170TargetComparisonPassed={p170TargetComparisonPassed}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record ClosureCheck(string CheckId, bool Passed, string Detail);
