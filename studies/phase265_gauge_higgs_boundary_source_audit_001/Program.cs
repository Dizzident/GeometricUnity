using System.Text.Json;

const string DefaultOutputDir = "studies/phase265_gauge_higgs_boundary_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase264Path = "studies/phase264_higgs_vacuum_criticality_source_audit_001/output/higgs_vacuum_criticality_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE265_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase264 = JsonDocument.Parse(File.ReadAllText(Phase264Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var higgsRow = FindRow(rows, "higgs");
var higgsTargetGeV = RequiredDouble(higgsRow, "targetValue");
var higgsTargetUncertaintyGeV = RequiredDouble(higgsRow, "targetUncertainty");

var gaugeHiggsBoundaryLeadPresent = true;
var gaugeHiggsCondition = "lambda(M_KK)=0";
var externalGaugeHiggsPredictionCentralGeV = 125.0;
var externalGaugeHiggsPredictionUncertaintyGeV = 4.0;
var externalGaugeHiggsRangeLowerGeV = 119.0;
var externalGaugeHiggsRangeUpperGeV = 126.0;
var externalGaugeHiggsPredictionPull = Math.Abs(externalGaugeHiggsPredictionCentralGeV - higgsTargetGeV) /
    Math.Sqrt(externalGaugeHiggsPredictionUncertaintyGeV * externalGaugeHiggsPredictionUncertaintyGeV + higgsTargetUncertaintyGeV * higgsTargetUncertaintyGeV);
var targetInsideExternalGaugeHiggsRange = higgsTargetGeV >= externalGaugeHiggsRangeLowerGeV
    && higgsTargetGeV <= externalGaugeHiggsRangeUpperGeV;

var localGuGaugeHiggsBoundaryArtifactFound = false;
var compactificationScaleSourcePresent = false;
var fifthDimensionalGaugeInvarianceSourcePresent = false;
var wilsonLineHosotaniSourcePresent = false;
var guQuarticBoundarySourcePresent = false;
var guRgTransportSourcePresent = false;
var guTopYukawaAndAlphaSSourcePresent = false;
var guObservedHiggsExtractionPresent = false;
var guVevSourcePresent = false;
var gaugeHiggsBoundaryPromotesHiggsMass = false;
var gaugeHiggsBoundaryCompletesBosonPredictions = false;

var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var wAbsoluteMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out var p224Closure)
    && JsonBool(p224Closure, "wAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out p224Closure)
    && JsonBool(p224Closure, "higgsMassParameterClosure") is true;
var higgsScalarRepairPossibleFromCurrentRegistry = JsonBool(phase248.RootElement, "higgsScalarSourceRepairPossibleFromCurrentRegistry") is true;
var newHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;
var vacuumCriticalityPromotesHiggsMass = JsonBool(phase264.RootElement, "vacuumCriticalityPromotesHiggsMass") is true;
var vacuumCriticalityBoundaryNumericallyNearHiggsMass = JsonBool(phase264.RootElement, "vacuumCriticalityBoundaryNumericallyNearHiggsMass") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var gaugeHiggsBoundarySourceAuditPassed =
    gaugeHiggsBoundaryLeadPresent
    && targetInsideExternalGaugeHiggsRange
    && externalGaugeHiggsPredictionPull < 1.0
    && !localGuGaugeHiggsBoundaryArtifactFound
    && !compactificationScaleSourcePresent
    && !fifthDimensionalGaugeInvarianceSourcePresent
    && !wilsonLineHosotaniSourcePresent
    && !guQuarticBoundarySourcePresent
    && !guRgTransportSourcePresent
    && !guTopYukawaAndAlphaSSourcePresent
    && !guObservedHiggsExtractionPresent
    && !guVevSourcePresent
    && !gaugeHiggsBoundaryPromotesHiggsMass
    && !gaugeHiggsBoundaryCompletesBosonPredictions
    && !lowEnergyRgTransportSourcePromotable
    && !wAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !higgsScalarRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !vacuumCriticalityPromotesHiggsMass
    && vacuumCriticalityBoundaryNumericallyNearHiggsMass
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "external-gauge-higgs-boundary-numerically-near",
        gaugeHiggsBoundaryLeadPresent && targetInsideExternalGaugeHiggsRange && externalGaugeHiggsPredictionPull < 1.0,
        $"externalGaugeHiggsPredictionCentralGeV={externalGaugeHiggsPredictionCentralGeV:R}; externalGaugeHiggsPredictionUncertaintyGeV={externalGaugeHiggsPredictionUncertaintyGeV:R}; targetInsideExternalGaugeHiggsRange={targetInsideExternalGaugeHiggsRange}; externalGaugeHiggsPredictionPull={externalGaugeHiggsPredictionPull:R}"),
    new Check(
        "no-local-gu-gauge-higgs-boundary-artifact",
        !localGuGaugeHiggsBoundaryArtifactFound && !guQuarticBoundarySourcePresent && !compactificationScaleSourcePresent,
        $"localGuGaugeHiggsBoundaryArtifactFound={localGuGaugeHiggsBoundaryArtifactFound}; guQuarticBoundarySourcePresent={guQuarticBoundarySourcePresent}; compactificationScaleSourcePresent={compactificationScaleSourcePresent}"),
    new Check(
        "external-model-ingredients-not-gu-source-lineage",
        !fifthDimensionalGaugeInvarianceSourcePresent
            && !wilsonLineHosotaniSourcePresent
            && !guRgTransportSourcePresent
            && !guTopYukawaAndAlphaSSourcePresent,
        $"fifthDimensionalGaugeInvarianceSourcePresent={fifthDimensionalGaugeInvarianceSourcePresent}; wilsonLineHosotaniSourcePresent={wilsonLineHosotaniSourcePresent}; guRgTransportSourcePresent={guRgTransportSourcePresent}; guTopYukawaAndAlphaSSourcePresent={guTopYukawaAndAlphaSSourcePresent}"),
    new Check(
        "gauge-higgs-boundary-does-not-fill-observed-higgs-contract",
        !guObservedHiggsExtractionPresent && !guVevSourcePresent && !higgsMassParameterClosure && newHiggsScalarSourceStillRequired,
        $"guObservedHiggsExtractionPresent={guObservedHiggsExtractionPresent}; guVevSourcePresent={guVevSourcePresent}; higgsMassParameterClosure={higgsMassParameterClosure}; newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}"),
    new Check(
        "gauge-higgs-boundary-does-not-complete-wz",
        !wAbsoluteMassParameterClosure && wzMissingFieldCount > 0,
        $"wAbsoluteMassParameterClosure={wAbsoluteMassParameterClosure}; wzMissingFieldCount={wzMissingFieldCount}"),
    new Check(
        "gauge-higgs-boundary-remains-nonpromotional",
        !gaugeHiggsBoundaryPromotesHiggsMass && !gaugeHiggsBoundaryCompletesBosonPredictions,
        $"gaugeHiggsBoundaryPromotesHiggsMass={gaugeHiggsBoundaryPromotesHiggsMass}; gaugeHiggsBoundaryCompletesBosonPredictions={gaugeHiggsBoundaryCompletesBosonPredictions}"),
};

var terminalStatus = gaugeHiggsBoundarySourceAuditPassed
    ? "gauge-higgs-boundary-source-audit-external-rg-boundary-not-promotion"
    : "gauge-higgs-boundary-source-audit-review-required";

var result = new
{
    phaseId = "phase265-gauge-higgs-boundary-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    gaugeHiggsBoundarySourceAuditPassed,
    gaugeHiggsBoundaryPromotesHiggsMass,
    gaugeHiggsBoundaryCompletesBosonPredictions,
    gaugeHiggsBoundaryLeadPresent,
    targetInsideExternalGaugeHiggsRange,
    gaugeHiggsBoundary = new
    {
        gaugeHiggsCondition,
        externalGaugeHiggsPredictionCentralGeV,
        externalGaugeHiggsPredictionUncertaintyGeV,
        externalGaugeHiggsRangeLowerGeV,
        externalGaugeHiggsRangeUpperGeV,
        higgsTargetGeV,
        higgsTargetUncertaintyGeV,
        externalGaugeHiggsPredictionPull,
    },
    sourceLineageBoundary = new
    {
        localGuGaugeHiggsBoundaryArtifactFound,
        compactificationScaleSourcePresent,
        fifthDimensionalGaugeInvarianceSourcePresent,
        wilsonLineHosotaniSourcePresent,
        guQuarticBoundarySourcePresent,
        guRgTransportSourcePresent,
        guTopYukawaAndAlphaSSourcePresent,
        guObservedHiggsExtractionPresent,
        guVevSourcePresent,
    },
    currentBlockerEvidence = new
    {
        phase236 = new
        {
            lowEnergyRgTransportSourcePromotable,
        },
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
            vacuumCriticalityBoundaryNumericallyNearHiggsMass,
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
            "arxiv-0705-3035",
            "https://arxiv.org/abs/0705.3035",
            "Gauge-Higgs unification can estimate m_H = 125 +/- 4 GeV by imposing a vanishing quartic at a compactification scale, but this is a five-dimensional model boundary condition."),
        new ExternalSource(
            "arxiv-1307-5079",
            "https://arxiv.org/abs/1307.5079",
            "The Snowmass white paper reports a 119-126 GeV Higgs range from the gauge-Higgs condition after including top and QCD uncertainties."),
        new ExternalSource(
            "arxiv-1510-03092",
            "https://arxiv.org/abs/1510.03092",
            "A 5D gauge-Higgs unification analysis reproduces 125 GeV by choosing model-dependent bulk mass and compactification-scale relations."),
    },
    localSearchFinding = "Repository search found generic GU Higgs/gauge context and already-blocked diagnostic text, but no GU-local compactification scale, Wilson-line/Hosotani scalar extraction, or lambda(M_KK)=0 source theorem that fills the Higgs source-lineage contract.",
    checks,
    decision = gaugeHiggsBoundarySourceAuditPassed
        ? "Do not promote a Higgs mass prediction from external gauge-Higgs unification boundary conditions. The lambda(M_KK)=0 condition is numerically interesting, but the repository lacks the GU compactification-scale, Wilson-line/Hosotani, RG-transport, top/Yukawa, VEV, and observed-Higgs extraction source artifacts needed for promotion."
        : "Review gauge-Higgs boundary source audit before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local derivation of the Higgs as a gauge/Wilson-line scalar with an observed-sector extraction theorem.",
        "A target-independent GU compactification or boundary scale and lambda(M_KK)=0 source theorem.",
        "A GU RG-transport source with top/Yukawa, alpha_s, VEV, and stability sidecars.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase248Path = Phase248Path,
        phase264Path = Phase264Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "gauge_higgs_boundary_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "gauge_higgs_boundary_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.gaugeHiggsBoundarySourceAuditPassed,
        result.gaugeHiggsBoundaryPromotesHiggsMass,
        result.gaugeHiggsBoundaryCompletesBosonPredictions,
        result.gaugeHiggsBoundaryLeadPresent,
        result.targetInsideExternalGaugeHiggsRange,
        result.gaugeHiggsBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.localSearchFinding,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"gaugeHiggsBoundarySourceAuditPassed={gaugeHiggsBoundarySourceAuditPassed}");
Console.WriteLine($"gaugeHiggsBoundaryPromotesHiggsMass={gaugeHiggsBoundaryPromotesHiggsMass}");
Console.WriteLine($"externalGaugeHiggsPredictionPull={externalGaugeHiggsPredictionPull}");

static JsonElement FindRow(IEnumerable<JsonElement> rows, string particleId) =>
    rows.First(row => string.Equals(JsonString(row, "particleId"), particleId, StringComparison.Ordinal));

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number
        ? property.GetDouble()
        : throw new InvalidOperationException($"Missing numeric property '{propertyName}'.");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record Check(string CheckId, bool Passed, string Detail);
sealed record ExternalSource(string SourceId, string Url, string Finding);
