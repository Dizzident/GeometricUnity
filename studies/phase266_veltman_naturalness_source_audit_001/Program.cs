using System.Text.Json;

const string DefaultOutputDir = "studies/phase266_veltman_naturalness_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase262Path = "studies/phase262_higgs_top_empirical_relation_source_audit_001/output/higgs_top_empirical_relation_source_audit_summary.json";
const string Phase264Path = "studies/phase264_higgs_vacuum_criticality_source_audit_001/output/higgs_vacuum_criticality_source_audit_summary.json";
const string Phase265Path = "studies/phase265_gauge_higgs_boundary_source_audit_001/output/gauge_higgs_boundary_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE266_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase262 = JsonDocument.Parse(File.ReadAllText(Phase262Path));
using var phase264 = JsonDocument.Parse(File.ReadAllText(Phase264Path));
using var phase265 = JsonDocument.Parse(File.ReadAllText(Phase265Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wRow = FindRow(rows, "w-boson");
var zRow = FindRow(rows, "z-boson");
var higgsRow = FindRow(rows, "higgs");
var empiricalRelations = phase262.RootElement.GetProperty("empiricalRelations");

var wTargetGeV = RequiredDouble(wRow, "targetValue");
var wTargetUncertaintyGeV = RequiredDouble(wRow, "targetUncertainty");
var zTargetGeV = RequiredDouble(zRow, "targetValue");
var zTargetUncertaintyGeV = RequiredDouble(zRow, "targetUncertainty");
var higgsTargetGeV = RequiredDouble(higgsRow, "targetValue");
var higgsTargetUncertaintyGeV = RequiredDouble(higgsRow, "targetUncertainty");
var topMassGeV = RequiredDouble(empiricalRelations, "topMassGeV");
var topMassUncertaintyGeV = RequiredDouble(empiricalRelations, "topMassUncertaintyGeV");

var veltmanConditionLeadPresent = true;
var veltmanConditionExpression = "m_H^2 + 2 m_W^2 + m_Z^2 - 4 m_t^2 = 0";
var veltmanPredictedHiggsMassSquaredGeV2 = 4.0 * topMassGeV * topMassGeV
    - 2.0 * wTargetGeV * wTargetGeV
    - zTargetGeV * zTargetGeV;
var veltmanPredictedHiggsMassGeV = Math.Sqrt(veltmanPredictedHiggsMassSquaredGeV2);
var veltmanPredictedMassSquaredUncertaintyGeV2 = Math.Sqrt(
    Math.Pow(8.0 * topMassGeV * topMassUncertaintyGeV, 2.0) +
    Math.Pow(4.0 * wTargetGeV * wTargetUncertaintyGeV, 2.0) +
    Math.Pow(2.0 * zTargetGeV * zTargetUncertaintyGeV, 2.0));
var veltmanPredictedHiggsMassUncertaintyGeV = veltmanPredictedMassSquaredUncertaintyGeV2 / (2.0 * veltmanPredictedHiggsMassGeV);
var veltmanPredictionPull = Pull(
    veltmanPredictedHiggsMassGeV,
    veltmanPredictedHiggsMassUncertaintyGeV,
    higgsTargetGeV,
    higgsTargetUncertaintyGeV);
var observedVeltmanCoefficientGeV2 = higgsTargetGeV * higgsTargetGeV
    + 2.0 * wTargetGeV * wTargetGeV
    + zTargetGeV * zTargetGeV
    - 4.0 * topMassGeV * topMassGeV;
var observedVeltmanCoefficientUncertaintyGeV2 = Math.Sqrt(
    Math.Pow(2.0 * higgsTargetGeV * higgsTargetUncertaintyGeV, 2.0) +
    Math.Pow(4.0 * wTargetGeV * wTargetUncertaintyGeV, 2.0) +
    Math.Pow(2.0 * zTargetGeV * zTargetUncertaintyGeV, 2.0) +
    Math.Pow(8.0 * topMassGeV * topMassUncertaintyGeV, 2.0));
var observedVeltmanCoefficientPullFromZero = Math.Abs(observedVeltmanCoefficientGeV2) / observedVeltmanCoefficientUncertaintyGeV2;

var veltmanNumericallyClosesHiggsMass = veltmanPredictionPull < 3.0;
var observedVeltmanConditionNearZero = observedVeltmanCoefficientPullFromZero < 3.0;
var veltmanUsesExternalMeasuredMasses = true;
var veltmanConditionAssumedNotGuDerived = true;
var veltmanProvidesGuNaturalnessSource = false;
var veltmanProvidesGuScalarPotentialSource = false;
var veltmanProvidesGuTopYukawaSource = false;
var veltmanProvidesGuVevSource = false;
var veltmanProvidesObservedFieldExtraction = false;
var veltmanPromotesHiggsMass = false;
var veltmanCompletesBosonPredictions = false;

var wAbsoluteMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out var p224Closure)
    && JsonBool(p224Closure, "wAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out p224Closure)
    && JsonBool(p224Closure, "higgsMassParameterClosure") is true;
var higgsScalarRepairPossibleFromCurrentRegistry = JsonBool(phase248.RootElement, "higgsScalarSourceRepairPossibleFromCurrentRegistry") is true;
var newHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;
var vacuumCriticalityPromotesHiggsMass = JsonBool(phase264.RootElement, "vacuumCriticalityPromotesHiggsMass") is true;
var gaugeHiggsBoundaryPromotesHiggsMass = JsonBool(phase265.RootElement, "gaugeHiggsBoundaryPromotesHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var veltmanNaturalnessSourceAuditPassed =
    veltmanConditionLeadPresent
    && !veltmanNumericallyClosesHiggsMass
    && !observedVeltmanConditionNearZero
    && veltmanUsesExternalMeasuredMasses
    && veltmanConditionAssumedNotGuDerived
    && !veltmanProvidesGuNaturalnessSource
    && !veltmanProvidesGuScalarPotentialSource
    && !veltmanProvidesGuTopYukawaSource
    && !veltmanProvidesGuVevSource
    && !veltmanProvidesObservedFieldExtraction
    && !veltmanPromotesHiggsMass
    && !veltmanCompletesBosonPredictions
    && !wAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !higgsScalarRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !vacuumCriticalityPromotesHiggsMass
    && !gaugeHiggsBoundaryPromotesHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "veltman-condition-numerically-fails-observed-higgs",
        !veltmanNumericallyClosesHiggsMass && veltmanPredictionPull > 100.0,
        $"veltmanPredictedHiggsMassGeV={veltmanPredictedHiggsMassGeV:R}; veltmanPredictedHiggsMassUncertaintyGeV={veltmanPredictedHiggsMassUncertaintyGeV:R}; veltmanPredictionPull={veltmanPredictionPull:R}"),
    new Check(
        "observed-masses-do-not-satisfy-veltman-condition",
        !observedVeltmanConditionNearZero && observedVeltmanCoefficientPullFromZero > 100.0,
        $"observedVeltmanCoefficientGeV2={observedVeltmanCoefficientGeV2:R}; observedVeltmanCoefficientUncertaintyGeV2={observedVeltmanCoefficientUncertaintyGeV2:R}; observedVeltmanCoefficientPullFromZero={observedVeltmanCoefficientPullFromZero:R}"),
    new Check(
        "veltman-condition-is-external-naturalness-not-gu-source",
        veltmanUsesExternalMeasuredMasses && veltmanConditionAssumedNotGuDerived && !veltmanProvidesGuNaturalnessSource,
        $"veltmanUsesExternalMeasuredMasses={veltmanUsesExternalMeasuredMasses}; veltmanConditionAssumedNotGuDerived={veltmanConditionAssumedNotGuDerived}; veltmanProvidesGuNaturalnessSource={veltmanProvidesGuNaturalnessSource}"),
    new Check(
        "veltman-condition-does-not-fill-higgs-contract",
        !veltmanProvidesGuScalarPotentialSource
            && !veltmanProvidesGuTopYukawaSource
            && !veltmanProvidesGuVevSource
            && !veltmanProvidesObservedFieldExtraction,
        $"scalarPotentialSource={veltmanProvidesGuScalarPotentialSource}; topYukawaSource={veltmanProvidesGuTopYukawaSource}; guVevSource={veltmanProvidesGuVevSource}; observedFieldExtraction={veltmanProvidesObservedFieldExtraction}"),
    new Check(
        "current-higgs-shortcuts-remain-nonpromotional",
        !higgsScalarRepairPossibleFromCurrentRegistry
            && newHiggsScalarSourceStillRequired
            && !vacuumCriticalityPromotesHiggsMass
            && !gaugeHiggsBoundaryPromotesHiggsMass,
        $"higgsScalarRepairPossibleFromCurrentRegistry={higgsScalarRepairPossibleFromCurrentRegistry}; newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}; vacuumCriticalityPromotesHiggsMass={vacuumCriticalityPromotesHiggsMass}; gaugeHiggsBoundaryPromotesHiggsMass={gaugeHiggsBoundaryPromotesHiggsMass}"),
    new Check(
        "veltman-condition-does-not-complete-boson-predictions",
        !veltmanPromotesHiggsMass && !veltmanCompletesBosonPredictions && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0,
        $"veltmanPromotesHiggsMass={veltmanPromotesHiggsMass}; veltmanCompletesBosonPredictions={veltmanCompletesBosonPredictions}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = veltmanNaturalnessSourceAuditPassed
    ? "veltman-naturalness-source-audit-condition-fails-not-promotion"
    : "veltman-naturalness-source-audit-review-required";

var result = new
{
    phaseId = "phase266-veltman-naturalness-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    veltmanNaturalnessSourceAuditPassed,
    veltmanPromotesHiggsMass,
    veltmanCompletesBosonPredictions,
    veltmanNumericallyClosesHiggsMass,
    observedVeltmanConditionNearZero,
    veltmanCondition = new
    {
        veltmanConditionExpression,
        wTargetGeV,
        wTargetUncertaintyGeV,
        zTargetGeV,
        zTargetUncertaintyGeV,
        topMassGeV,
        topMassUncertaintyGeV,
        higgsTargetGeV,
        higgsTargetUncertaintyGeV,
        veltmanPredictedHiggsMassSquaredGeV2,
        veltmanPredictedHiggsMassGeV,
        veltmanPredictedHiggsMassUncertaintyGeV,
        veltmanPredictionPull,
        observedVeltmanCoefficientGeV2,
        observedVeltmanCoefficientUncertaintyGeV2,
        observedVeltmanCoefficientPullFromZero,
    },
    sourceLineageBoundary = new
    {
        veltmanUsesExternalMeasuredMasses,
        veltmanConditionAssumedNotGuDerived,
        veltmanProvidesGuNaturalnessSource,
        veltmanProvidesGuScalarPotentialSource,
        veltmanProvidesGuTopYukawaSource,
        veltmanProvidesGuVevSource,
        veltmanProvidesObservedFieldExtraction,
    },
    currentBlockerEvidence = new
    {
        phase224 = new
        {
            wAbsoluteMassParameterClosure,
            higgsMassParameterClosure,
        },
        phase248 = new
        {
            higgsScalarRepairPossibleFromCurrentRegistry,
            newHiggsScalarSourceStillRequired,
        },
        phase264 = new
        {
            vacuumCriticalityPromotesHiggsMass,
        },
        phase265 = new
        {
            gaugeHiggsBoundaryPromotesHiggsMass,
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
            "veltman-condition-standard-form",
            "https://doi.org/10.1016/0370-2693(81)90647-8",
            "The one-loop naturalness condition cancels the quadratic divergence in the Higgs mass parameter; with current masses it predicts a Higgs far above 125 GeV."),
        new ExternalSource(
            "guises-and-disguises-quadratic-divergences",
            "https://www.sciencedirect.com/science/article/abs/pii/S0003491614002887",
            "The review notes that the Standard Model Veltman condition leads to a Higgs mass around 316 GeV, not the observed 125 GeV."),
        new ExternalSource(
            "model-independent-veltman-condition",
            "https://doi.org/10.1088/1674-1137/ac2ffa",
            "Modern Veltman-condition work treats cancellation as a naturalness criterion involving new physics, not as a target-independent GU scalar-source row."),
    },
    checks,
    decision = veltmanNaturalnessSourceAuditPassed
        ? "Do not promote a Higgs mass prediction from the Standard Model Veltman naturalness condition. The observed W/Z/top/Higgs masses do not satisfy the condition, the inferred Higgs mass is far too heavy, and the condition supplies no GU scalar-source, top/Yukawa, VEV, observed-field extraction, or W/Z absolute-scale source lineage."
        : "Review Veltman naturalness source audit before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived naturalness or cancellation theorem if quadratic-divergence cancellation is to be predictive.",
        "A solved Higgs scalar-source/operator and top/Yukawa source independent of observed masses.",
        "A GU VEV/source-scale row and observed-field extraction theorem.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase248Path = Phase248Path,
        phase262Path = Phase262Path,
        phase264Path = Phase264Path,
        phase265Path = Phase265Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "veltman_naturalness_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "veltman_naturalness_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.veltmanNaturalnessSourceAuditPassed,
        result.veltmanPromotesHiggsMass,
        result.veltmanCompletesBosonPredictions,
        result.veltmanNumericallyClosesHiggsMass,
        result.observedVeltmanConditionNearZero,
        result.veltmanCondition,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"veltmanNaturalnessSourceAuditPassed={veltmanNaturalnessSourceAuditPassed}");
Console.WriteLine($"veltmanPromotesHiggsMass={veltmanPromotesHiggsMass}");
Console.WriteLine($"veltmanPredictedHiggsMassGeV={veltmanPredictedHiggsMassGeV}");
Console.WriteLine($"veltmanPredictionPull={veltmanPredictionPull}");

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
