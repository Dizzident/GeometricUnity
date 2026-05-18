using System.Text.Json;

const string DefaultOutputDir = "studies/phase263_top_yukawa_unity_higgs_closure_audit_001/output";
const string Phase54Path = "studies/phase54_external_electroweak_scale_input_001/external_electroweak_scale_input.json";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase237Path = "studies/phase237_cox_ii_higgs_yukawa_texture_dependency_audit_001/output/cox_ii_higgs_yukawa_texture_dependency_audit_summary.json";
const string Phase262Path = "studies/phase262_higgs_top_empirical_relation_source_audit_001/output/higgs_top_empirical_relation_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE263_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase54 = JsonDocument.Parse(File.ReadAllText(Phase54Path));
using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase237 = JsonDocument.Parse(File.ReadAllText(Phase237Path));
using var phase262 = JsonDocument.Parse(File.ReadAllText(Phase262Path));

var fermiScale = phase54.RootElement
    .GetProperty("derivedExternalScaleCandidates")
    .EnumerateArray()
    .First(row => JsonString(row, "scaleId") == "phase54-fermi-derived-electroweak-vacuum-scale");
var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var zRow = FindRow(rows, "z-boson");
var higgsRow = FindRow(rows, "higgs");
var empiricalRelations = phase262.RootElement.GetProperty("empiricalRelations");

var externalVevGeV = RequiredDouble(fermiScale, "value");
var externalVevUncertaintyGeV = RequiredDouble(fermiScale, "standardUncertainty");
var zTargetGeV = RequiredDouble(zRow, "targetValue");
var zTargetUncertaintyGeV = RequiredDouble(zRow, "targetUncertainty");
var higgsTargetGeV = RequiredDouble(higgsRow, "targetValue");
var higgsTargetUncertaintyGeV = RequiredDouble(higgsRow, "targetUncertainty");
var measuredTopMassGeV = RequiredDouble(empiricalRelations, "topMassGeV");
var measuredTopMassUncertaintyGeV = RequiredDouble(empiricalRelations, "topMassUncertaintyGeV");

var unityTopYukawa = 1.0;
var unityTopMassGeV = unityTopYukawa * externalVevGeV / Math.Sqrt(2.0);
var unityTopMassUncertaintyGeV = externalVevUncertaintyGeV / Math.Sqrt(2.0);
var unityTopMassPull = Pull(unityTopMassGeV, unityTopMassUncertaintyGeV, measuredTopMassGeV, measuredTopMassUncertaintyGeV);

var targetImpliedTopYukawa = Math.Sqrt(2.0) * measuredTopMassGeV / externalVevGeV;
var targetImpliedTopYukawaUncertainty = targetImpliedTopYukawa * Math.Sqrt(
    Math.Pow(measuredTopMassUncertaintyGeV / measuredTopMassGeV, 2.0) +
    Math.Pow(externalVevUncertaintyGeV / externalVevGeV, 2.0));
var exactHiggsTopRelationTopYukawa = Math.Sqrt(2.0) * higgsTargetGeV * higgsTargetGeV / (zTargetGeV * externalVevGeV);

var unityTopHiggsGeometricMeanGeV = Math.Sqrt(zTargetGeV * unityTopMassGeV);
var unityTopHiggsGeometricMeanUncertaintyGeV = 0.5 * unityTopHiggsGeometricMeanGeV * Math.Sqrt(
    Math.Pow(zTargetUncertaintyGeV / zTargetGeV, 2.0) +
    Math.Pow(unityTopMassUncertaintyGeV / unityTopMassGeV, 2.0));
var unityTopHiggsGeometricMeanPull = Pull(
    unityTopHiggsGeometricMeanGeV,
    unityTopHiggsGeometricMeanUncertaintyGeV,
    higgsTargetGeV,
    higgsTargetUncertaintyGeV);
var unityTopHiggsRatio = higgsTargetGeV * higgsTargetGeV / (zTargetGeV * unityTopMassGeV);

var topYukawaUnityNumericallyCloses = unityTopMassPull < 3.0 && unityTopHiggsGeometricMeanPull < 3.0;
var topYukawaUnityPromotesHiggsMass = false;
var topYukawaUnityProvidesGuYukawaSource = false;
var topYukawaUnityProvidesHiggsScalarSource = false;
var topYukawaUnityProvidesPotentialOrSelfCouplingSource = false;
var topYukawaUnityProvidesObservedFieldExtraction = false;
var topYukawaUnityProvidesGuVevSource = false;
var targetImpliedYukawaPromotable = false;

var coxIiHiggsYukawaTexturePromotable = JsonBool(phase237.RootElement, "coxIiHiggsYukawaTexturePromotableForHiggsMass") is true;
var p262RelationPromotesHiggsMass = JsonBool(phase262.RootElement, "relationPromotesHiggsMass") is true;
var p262TopYukawaSourceLineagePresent = JsonBool(empiricalRelations, "topYukawaSourceLineagePresent") is true;
var p262RelationHasGuDerivation = JsonBool(empiricalRelations, "relationHasGuDerivation") is true;
var wAbsoluteMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out var p224Closure)
    && JsonBool(p224Closure, "wAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out p224Closure)
    && JsonBool(p224Closure, "higgsMassParameterClosure") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var topYukawaUnityHiggsClosureAuditPassed =
    !topYukawaUnityNumericallyCloses
    && !topYukawaUnityPromotesHiggsMass
    && !topYukawaUnityProvidesGuYukawaSource
    && !topYukawaUnityProvidesHiggsScalarSource
    && !topYukawaUnityProvidesPotentialOrSelfCouplingSource
    && !topYukawaUnityProvidesObservedFieldExtraction
    && !topYukawaUnityProvidesGuVevSource
    && !targetImpliedYukawaPromotable
    && !coxIiHiggsYukawaTexturePromotable
    && !p262RelationPromotesHiggsMass
    && !p262TopYukawaSourceLineagePresent
    && !p262RelationHasGuDerivation
    && !wAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "exact-top-yukawa-unity-does-not-close-targets",
        !topYukawaUnityNumericallyCloses && unityTopMassPull > 3.0 && unityTopHiggsGeometricMeanPull > 3.0,
        $"unityTopMassGeV={unityTopMassGeV:R}; unityTopMassPull={unityTopMassPull:R}; unityTopHiggsGeometricMeanGeV={unityTopHiggsGeometricMeanGeV:R}; unityTopHiggsGeometricMeanPull={unityTopHiggsGeometricMeanPull:R}; unityTopHiggsRatio={unityTopHiggsRatio:R}"),
    new Check(
        "target-implied-top-yukawa-is-not-source",
        !targetImpliedYukawaPromotable && targetImpliedTopYukawa < 1.0,
        $"targetImpliedTopYukawa={targetImpliedTopYukawa:R}; targetImpliedTopYukawaUncertainty={targetImpliedTopYukawaUncertainty:R}; exactHiggsTopRelationTopYukawa={exactHiggsTopRelationTopYukawa:R}; targetImpliedYukawaPromotable={targetImpliedYukawaPromotable}"),
    new Check(
        "no-gu-top-yukawa-source-lineage",
        !topYukawaUnityProvidesGuYukawaSource && !p262TopYukawaSourceLineagePresent && !coxIiHiggsYukawaTexturePromotable,
        $"topYukawaUnityProvidesGuYukawaSource={topYukawaUnityProvidesGuYukawaSource}; p262TopYukawaSourceLineagePresent={p262TopYukawaSourceLineagePresent}; coxIiHiggsYukawaTexturePromotable={coxIiHiggsYukawaTexturePromotable}"),
    new Check(
        "unity-shortcut-does-not-fill-higgs-contract",
        !topYukawaUnityProvidesHiggsScalarSource
            && !topYukawaUnityProvidesPotentialOrSelfCouplingSource
            && !topYukawaUnityProvidesObservedFieldExtraction,
        $"topYukawaUnityProvidesHiggsScalarSource={topYukawaUnityProvidesHiggsScalarSource}; topYukawaUnityProvidesPotentialOrSelfCouplingSource={topYukawaUnityProvidesPotentialOrSelfCouplingSource}; topYukawaUnityProvidesObservedFieldExtraction={topYukawaUnityProvidesObservedFieldExtraction}"),
    new Check(
        "external-vev-remains-non-gu-source",
        !topYukawaUnityProvidesGuVevSource && !wAbsoluteMassParameterClosure && !higgsMassParameterClosure,
        $"topYukawaUnityProvidesGuVevSource={topYukawaUnityProvidesGuVevSource}; wAbsoluteMassParameterClosure={wAbsoluteMassParameterClosure}; higgsMassParameterClosure={higgsMassParameterClosure}"),
    new Check(
        "phase213-blockers-preserved",
        wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = topYukawaUnityHiggsClosureAuditPassed
    ? "top-yukawa-unity-higgs-closure-audit-no-promotion"
    : "top-yukawa-unity-higgs-closure-audit-review-required";

var result = new
{
    phaseId = "phase263-top-yukawa-unity-higgs-closure-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    topYukawaUnityHiggsClosureAuditPassed,
    topYukawaUnityPromotesHiggsMass,
    topYukawaUnityNumericallyCloses,
    topYukawaUnityProvidesGuYukawaSource,
    targetImpliedYukawaPromotable,
    topYukawaUnityHypothesis = new
    {
        unityTopYukawa,
        externalVevGeV,
        externalVevUncertaintyGeV,
        unityTopMassGeV,
        unityTopMassUncertaintyGeV,
        measuredTopMassGeV,
        measuredTopMassUncertaintyGeV,
        unityTopMassPull,
        targetImpliedTopYukawa,
        targetImpliedTopYukawaUncertainty,
        exactHiggsTopRelationTopYukawa,
    },
    higgsTopClosureReplay = new
    {
        relationId = "mh-squared-approximately-mz-mt-with-yt-unity",
        expression = "m_H^2 ~= m_Z * (v / sqrt(2)) for y_t = 1",
        zTargetGeV,
        zTargetUncertaintyGeV,
        higgsTargetGeV,
        higgsTargetUncertaintyGeV,
        unityTopHiggsGeometricMeanGeV,
        unityTopHiggsGeometricMeanUncertaintyGeV,
        unityTopHiggsGeometricMeanPull,
        unityTopHiggsRatio,
    },
    sourceLineageBoundary = new
    {
        topYukawaUnityProvidesGuYukawaSource,
        topYukawaUnityProvidesHiggsScalarSource,
        topYukawaUnityProvidesPotentialOrSelfCouplingSource,
        topYukawaUnityProvidesObservedFieldExtraction,
        topYukawaUnityProvidesGuVevSource,
    },
    currentBlockerEvidence = new
    {
        phase237 = new
        {
            coxIiHiggsYukawaTexturePromotable,
        },
        phase262 = new
        {
            p262RelationPromotesHiggsMass,
            p262TopYukawaSourceLineagePresent,
            p262RelationHasGuDerivation,
        },
        phase224 = new
        {
            wAbsoluteMassParameterClosure,
            higgsMassParameterClosure,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    externalResearchSnapshot = new[]
    {
        new ExternalSource(
            "pdg-2025-top-quark-review",
            "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-top-quark.pdf",
            "The top review frames the top Yukawa as order unity and reports direct LHC top-mass input, not a GU-derived value."),
        new ExternalSource(
            "pdg-2025-higgs-boson-review",
            "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-higgs-boson.pdf",
            "Higgs couplings and Yukawa measurements are empirical Standard Model tests, not source-lineage derivations for GU prediction gates."),
        new ExternalSource(
            "torrente-lujan-2014-higgs-top-coincidence",
            "https://link.springer.com/article/10.1140/epjc/s10052-014-2744-3",
            "The Higgs/top relation is treated as an empirical coincidence suggesting unknown symmetry, not as a solved scalar-source theorem."),
    },
    checks,
    decision = topYukawaUnityHiggsClosureAuditPassed
        ? "Do not promote a Higgs prediction from exact top-Yukawa unity. The exact y_t=1 shortcut misses the current top and Higgs targets by several sigma, the target-implied Yukawa is an external/target-derived number, and no GU top/Yukawa source, Higgs scalar source, observed-field extraction, or GU VEV source is supplied."
        : "Review top-Yukawa unity Higgs closure audit before relying on this shortcut boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived top/Yukawa source lineage if top-sector unity or texture relations are to be used.",
        "A solved Higgs scalar-source/operator with potential/self-coupling or excitation relation.",
        "A GU-derived electroweak VEV/source-scale row and observed-field extraction theorem.",
    },
    sourceEvidence = new
    {
        phase54Path = Phase54Path,
        phase148Path = Phase148Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase237Path = Phase237Path,
        phase262Path = Phase262Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "top_yukawa_unity_higgs_closure_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "top_yukawa_unity_higgs_closure_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.topYukawaUnityHiggsClosureAuditPassed,
        result.topYukawaUnityPromotesHiggsMass,
        result.topYukawaUnityNumericallyCloses,
        result.topYukawaUnityProvidesGuYukawaSource,
        result.targetImpliedYukawaPromotable,
        result.topYukawaUnityHypothesis,
        result.higgsTopClosureReplay,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"topYukawaUnityHiggsClosureAuditPassed={topYukawaUnityHiggsClosureAuditPassed}");
Console.WriteLine($"topYukawaUnityPromotesHiggsMass={topYukawaUnityPromotesHiggsMass}");
Console.WriteLine($"topYukawaUnityNumericallyCloses={topYukawaUnityNumericallyCloses}");
Console.WriteLine($"unityTopMassPull={unityTopMassPull}");
Console.WriteLine($"unityTopHiggsGeometricMeanPull={unityTopHiggsGeometricMeanPull}");

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId) =>
    rows.First(row => string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal));

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : throw new InvalidOperationException($"Missing numeric property '{propertyName}'.");

static double Pull(double prediction, double predictionSigma, double target, double targetSigma) =>
    Math.Abs(prediction - target) / Math.Sqrt(predictionSigma * predictionSigma + targetSigma * targetSigma);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record Check(string CheckId, bool Passed, string Detail);
sealed record ExternalSource(string SourceId, string Url, string Finding);
