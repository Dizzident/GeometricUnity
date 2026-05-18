using System.Text.Json;

const string DefaultOutputDir = "studies/phase206_direct_bridge_normalization_closure_001/output";
const string Phase190Path = "studies/phase190_wz_direct_target_independent_geometric_bridge_source_law_001/output/wz_direct_target_independent_geometric_bridge_source_law_summary.json";
const string Phase191Path = "studies/phase191_wz_direct_bridge_prediction_decision_001/output/wz_direct_bridge_prediction_decision_summary.json";
const string Phase185Path = "studies/phase185_wz_operator_unit_scale_materialization_001/output/wz_operator_unit_scale_materialization_summary.json";
const string Phase166Path = "studies/phase166_source_normalized_wz_prediction_attempt_001/output/source_normalized_wz_prediction_attempt_summary.json";
const string Phase170Path = "studies/phase170_stable_source_shape_prediction_attempt_001/output/stable_source_shape_prediction_attempt_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE206_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase190 = JsonDocument.Parse(File.ReadAllText(Phase190Path));
using var phase191 = JsonDocument.Parse(File.ReadAllText(Phase191Path));
using var phase185 = JsonDocument.Parse(File.ReadAllText(Phase185Path));
using var phase166 = JsonDocument.Parse(File.ReadAllText(Phase166Path));
using var phase170 = JsonDocument.Parse(File.ReadAllText(Phase170Path));

var bestCandidate = phase190.RootElement
    .GetProperty("siblingStability")
    .GetProperty("bestCandidate");
var meanMagnitude = RequiredDouble(bestCandidate, "meanMagnitude");
var relativeSpread = RequiredDouble(bestCandidate, "relativeSpread");
var complexAlignment = RequiredDouble(bestCandidate, "complexAlignment");
var stableCandidatePresent = RequiredBool(bestCandidate, "stabilityPassed");
var theoremClaimed = RequiredBool(phase190.RootElement, "theoremClaimed");
var targetIndependent = phase190.RootElement
    .GetProperty("branchLocalLaw")
    .GetProperty("targetIndependent")
    .GetBoolean();
var wZParticleSplitPresent = phase191.RootElement
    .GetProperty("gates")
    .GetProperty("wZParticleSplitPresent")
    .GetBoolean();
var targetRaw = RequiredDouble(phase191.RootElement.GetProperty("gates"), "targetRaw");
var unitScale = RequiredDouble(phase185.RootElement.GetProperty("derivation"), "derivedUnitScale");

var directUnitScaledMagnitude = meanMagnitude * unitScale;
var directUnitScaledToTargetRaw = directUnitScaledMagnitude / targetRaw;
var directUnitScaleRawGatePassed = directUnitScaledToTargetRaw >= 0.95;
var directUnitScaleOvershootRatio = directUnitScaledToTargetRaw > 0 ? Math.Abs(directUnitScaledToTargetRaw - 1.0) : double.PositiveInfinity;

var p166RawGatePassed = JsonBool(phase166.RootElement.GetProperty("gates"), "rawGatePassed") is true;
var p166CommonScaleGatePassed = JsonBool(phase166.RootElement.GetProperty("gates"), "commonScaleGatePassed") is true;
var p166TargetComparisonPassed = JsonBool(phase166.RootElement.GetProperty("gates"), "targetComparisonPassed") is true;
var p170PromotionAllowed = JsonBool(phase170.RootElement.GetProperty("bestAttempt").GetProperty("gates"), "promotionAllowed") is true;

var checks = new[]
{
    new Check("p190-direct-candidate-stable", stableCandidatePresent, $"candidateId={JsonString(bestCandidate, "candidateId")}; relativeSpread={relativeSpread}"),
    new Check("p190-target-independent", targetIndependent, "P190 construction excludes W/Z target observables."),
    new Check("p190-theorem-promoted", theoremClaimed, "P190 must be theorem/derivation promoted before absolute-mass prediction."),
    new Check("p190-wz-particle-split", wZParticleSplitPresent, "P190/P191 must provide separate W and Z source rows."),
    new Check("direct-unit-scale-raw-gate", directUnitScaleRawGatePassed, $"P190 meanMagnitude * P185 unitScale / targetRaw = {directUnitScaledToTargetRaw}."),
    new Check("p166-source-normalized-common-scale", p166CommonScaleGatePassed, "P166 target-independent unit-scale replay must pass the W/Z common-scale gate."),
    new Check("p166-source-normalized-target-comparison", p166TargetComparisonPassed, "P166 target-independent unit-scale replay must pass physical W/Z target comparison."),
    new Check("p170-source-shape-promotion", p170PromotionAllowed, "P170 stable source-shape replay must be derivation-backed and pass target comparison."),
};

var canPromoteDirectBridgeNormalization = checks.All(check => check.Passed);
var terminalStatus = canPromoteDirectBridgeNormalization
    ? "direct-bridge-normalization-closure-promotable"
    : "direct-bridge-normalization-closure-blocked";

var result = new
{
    phaseId = "phase206-direct-bridge-normalization-closure",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    canPromoteDirectBridgeNormalization,
    directCandidate = new
    {
        candidateId = JsonString(bestCandidate, "candidateId"),
        meanMagnitude,
        relativeSpread,
        complexAlignment,
        stableCandidatePresent,
        theoremClaimed,
        targetIndependent,
        wZParticleSplitPresent,
    },
    directUnitScaleReplay = new
    {
        unitScale,
        targetRaw,
        directUnitScaledMagnitude,
        directUnitScaledToTargetRaw,
        directUnitScaleRawGatePassed,
        directUnitScaleOvershootRatio,
        note = "Diagnostic only: this combines two target-independent artifacts, but it is not a derivation-backed W/Z source theorem and still lacks particle-specific W/Z rows.",
    },
    downstreamNormalizationReplay = new
    {
        p166RawGatePassed,
        p166CommonScaleGatePassed,
        p166TargetComparisonPassed,
        p170PromotionAllowed,
    },
    checks,
    decision = canPromoteDirectBridgeNormalization
        ? "The direct bridge plus target-independent normalization route is promotable for W/Z absolute masses."
        : stableCandidatePresent
            ? "Do not promote W/Z absolute masses from direct bridge normalization. P190's stable branch-local source remains non-theorem, lacks separate W/Z source rows, and existing target-independent normalization replays fail downstream common-scale and/or target-comparison gates."
            : "Do not promote W/Z absolute masses from direct bridge normalization. After branch-local finite-difference replay, P190 has no sibling-stable direct bridge source; it also remains non-theorem, lacks separate W/Z source rows, and existing target-independent normalization replays fail downstream common-scale and/or target-comparison gates.",
    nextRequiredArtifact = "A derivation-backed direct W/Z bridge-source theorem with particle-specific source rows and a normalization/source-shape law that passes raw, common-scale, and target-comparison gates without target-fitted construction.",
    sourceEvidence = new
    {
        phase190Path = Phase190Path,
        phase191Path = Phase191Path,
        phase185Path = Phase185Path,
        phase166Path = Phase166Path,
        phase170Path = Phase170Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "direct_bridge_normalization_closure.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "direct_bridge_normalization_closure_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.canPromoteDirectBridgeNormalization,
        result.directCandidate,
        result.directUnitScaleReplay,
        result.downstreamNormalizationReplay,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"canPromoteDirectBridgeNormalization={canPromoteDirectBridgeNormalization}");
Console.WriteLine($"directUnitScaledToTargetRaw={directUnitScaledToTargetRaw}");
Console.WriteLine($"wZParticleSplitPresent={wZParticleSplitPresent}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value)
        ? value
        : throw new InvalidOperationException($"Missing numeric property {propertyName}.");

static bool RequiredBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => throw new InvalidOperationException($"Property {propertyName} is not boolean."),
    } : throw new InvalidOperationException($"Missing boolean property {propertyName}.");

sealed record Check(string CheckId, bool Passed, string Detail);
