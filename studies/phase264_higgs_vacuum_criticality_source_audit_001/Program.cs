using System.Text.Json;

const string DefaultOutputDir = "studies/phase264_higgs_vacuum_criticality_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase237Path = "studies/phase237_cox_ii_higgs_yukawa_texture_dependency_audit_001/output/cox_ii_higgs_yukawa_texture_dependency_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase262Path = "studies/phase262_higgs_top_empirical_relation_source_audit_001/output/higgs_top_empirical_relation_source_audit_summary.json";
const string Phase263Path = "studies/phase263_top_yukawa_unity_higgs_closure_audit_001/output/top_yukawa_unity_higgs_closure_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE264_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase237 = JsonDocument.Parse(File.ReadAllText(Phase237Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase262 = JsonDocument.Parse(File.ReadAllText(Phase262Path));
using var phase263 = JsonDocument.Parse(File.ReadAllText(Phase263Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var higgsRow = FindRow(rows, "higgs");
var empiricalRelations = phase262.RootElement.GetProperty("empiricalRelations");

var higgsTargetGeV = RequiredDouble(higgsRow, "targetValue");
var higgsTargetUncertaintyGeV = RequiredDouble(higgsRow, "targetUncertainty");
var measuredTopMassGeV = RequiredDouble(empiricalRelations, "topMassGeV");
var measuredTopMassUncertaintyGeV = RequiredDouble(empiricalRelations, "topMassUncertaintyGeV");

var alphaSAtMz = 0.1184;
var alphaSUncertainty = 0.0007;
var stabilityFormulaTheoryUncertaintyGeV = 0.3;
var absoluteStabilityBoundaryHiggsMassGeV = 129.6
    + 2.0 * (measuredTopMassGeV - 173.34)
    - 0.5 * ((alphaSAtMz - 0.1184) / 0.0007);
var absoluteStabilityBoundaryUncertaintyGeV = Math.Sqrt(
    Math.Pow(2.0 * measuredTopMassUncertaintyGeV, 2.0) +
    Math.Pow(0.5 * alphaSUncertainty / 0.0007, 2.0) +
    Math.Pow(stabilityFormulaTheoryUncertaintyGeV, 2.0));
var targetToStabilityBoundaryGapGeV = higgsTargetGeV - absoluteStabilityBoundaryHiggsMassGeV;
var targetToStabilityBoundaryPull = Math.Abs(targetToStabilityBoundaryGapGeV) /
    Math.Sqrt(absoluteStabilityBoundaryUncertaintyGeV * absoluteStabilityBoundaryUncertaintyGeV + higgsTargetUncertaintyGeV * higgsTargetUncertaintyGeV);

var vacuumCriticalityLeadPresent = true;
var vacuumCriticalityBoundaryNumericallyNearHiggsMass = Math.Abs(targetToStabilityBoundaryGapGeV) < 5.0;
var vacuumCriticalityBoundaryEqualsTarget = targetToStabilityBoundaryPull < 2.0;
var vacuumCriticalityUsesExternalSmRgInputs = true;
var vacuumCriticalityConditionAssumedNotDerived = true;
var vacuumCriticalityProvidesGuScalarPotentialSource = false;
var vacuumCriticalityProvidesGuQuarticBoundarySource = false;
var vacuumCriticalityProvidesGuTopYukawaSource = false;
var vacuumCriticalityProvidesGuVevSource = false;
var vacuumCriticalityProvidesObservedFieldExtraction = false;
var vacuumCriticalityPromotesHiggsMass = false;
var vacuumCriticalityCompletesBosonPredictions = false;

var canPromoteTargetImpliedHiggsSelfCoupling = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is true;
var wAbsoluteMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out var p224Closure)
    && JsonBool(p224Closure, "wAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = phase224.RootElement.TryGetProperty("closure", out p224Closure)
    && JsonBool(p224Closure, "higgsMassParameterClosure") is true;
var coxIiHiggsYukawaTexturePromotable = JsonBool(phase237.RootElement, "coxIiHiggsYukawaTexturePromotableForHiggsMass") is true;
var higgsScalarRepairPossibleFromCurrentRegistry = JsonBool(phase248.RootElement, "higgsScalarSourceRepairPossibleFromCurrentRegistry") is true;
var newHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;
var p262RelationPromotesHiggsMass = JsonBool(phase262.RootElement, "relationPromotesHiggsMass") is true;
var topYukawaUnityPromotesHiggsMass = JsonBool(phase263.RootElement, "topYukawaUnityPromotesHiggsMass") is true;
var topYukawaUnityNumericallyCloses = JsonBool(phase263.RootElement, "topYukawaUnityNumericallyCloses") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var higgsVacuumCriticalitySourceAuditPassed =
    vacuumCriticalityLeadPresent
    && vacuumCriticalityBoundaryNumericallyNearHiggsMass
    && !vacuumCriticalityBoundaryEqualsTarget
    && vacuumCriticalityUsesExternalSmRgInputs
    && vacuumCriticalityConditionAssumedNotDerived
    && !vacuumCriticalityProvidesGuScalarPotentialSource
    && !vacuumCriticalityProvidesGuQuarticBoundarySource
    && !vacuumCriticalityProvidesGuTopYukawaSource
    && !vacuumCriticalityProvidesGuVevSource
    && !vacuumCriticalityProvidesObservedFieldExtraction
    && !vacuumCriticalityPromotesHiggsMass
    && !vacuumCriticalityCompletesBosonPredictions
    && !canPromoteTargetImpliedHiggsSelfCoupling
    && !wAbsoluteMassParameterClosure
    && !higgsMassParameterClosure
    && !coxIiHiggsYukawaTexturePromotable
    && !higgsScalarRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !p262RelationPromotesHiggsMass
    && !topYukawaUnityPromotesHiggsMass
    && !topYukawaUnityNumericallyCloses
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "vacuum-criticality-lead-is-numerically-near-not-equality",
        vacuumCriticalityLeadPresent && vacuumCriticalityBoundaryNumericallyNearHiggsMass && !vacuumCriticalityBoundaryEqualsTarget,
        $"absoluteStabilityBoundaryHiggsMassGeV={absoluteStabilityBoundaryHiggsMassGeV:R}; targetToStabilityBoundaryGapGeV={targetToStabilityBoundaryGapGeV:R}; targetToStabilityBoundaryPull={targetToStabilityBoundaryPull:R}"),
    new Check(
        "criticality-uses-external-sm-rg-inputs",
        vacuumCriticalityUsesExternalSmRgInputs && vacuumCriticalityConditionAssumedNotDerived,
        $"alphaSAtMz={alphaSAtMz:R}; measuredTopMassGeV={measuredTopMassGeV:R}; vacuumCriticalityConditionAssumedNotDerived={vacuumCriticalityConditionAssumedNotDerived}"),
    new Check(
        "criticality-does-not-fill-gu-higgs-source-contract",
        !vacuumCriticalityProvidesGuScalarPotentialSource
            && !vacuumCriticalityProvidesGuQuarticBoundarySource
            && !vacuumCriticalityProvidesGuTopYukawaSource
            && !vacuumCriticalityProvidesObservedFieldExtraction,
        $"scalarPotentialSource={vacuumCriticalityProvidesGuScalarPotentialSource}; quarticBoundarySource={vacuumCriticalityProvidesGuQuarticBoundarySource}; topYukawaSource={vacuumCriticalityProvidesGuTopYukawaSource}; observedFieldExtraction={vacuumCriticalityProvidesObservedFieldExtraction}"),
    new Check(
        "criticality-does-not-fill-gu-scale-or-wz-contract",
        !vacuumCriticalityProvidesGuVevSource && !wAbsoluteMassParameterClosure && wzMissingFieldCount > 0,
        $"vacuumCriticalityProvidesGuVevSource={vacuumCriticalityProvidesGuVevSource}; wAbsoluteMassParameterClosure={wAbsoluteMassParameterClosure}; wzMissingFieldCount={wzMissingFieldCount}"),
    new Check(
        "current-higgs-shortcuts-remain-nonpromotional",
        !canPromoteTargetImpliedHiggsSelfCoupling
            && !coxIiHiggsYukawaTexturePromotable
            && !p262RelationPromotesHiggsMass
            && !topYukawaUnityPromotesHiggsMass
            && newHiggsScalarSourceStillRequired,
        $"canPromoteTargetImpliedHiggsSelfCoupling={canPromoteTargetImpliedHiggsSelfCoupling}; coxIiHiggsYukawaTexturePromotable={coxIiHiggsYukawaTexturePromotable}; p262RelationPromotesHiggsMass={p262RelationPromotesHiggsMass}; topYukawaUnityPromotesHiggsMass={topYukawaUnityPromotesHiggsMass}; newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}"),
    new Check(
        "criticality-does-not-complete-boson-predictions",
        !vacuumCriticalityPromotesHiggsMass && !vacuumCriticalityCompletesBosonPredictions && higgsMissingFieldCount > 0,
        $"vacuumCriticalityPromotesHiggsMass={vacuumCriticalityPromotesHiggsMass}; vacuumCriticalityCompletesBosonPredictions={vacuumCriticalityCompletesBosonPredictions}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = higgsVacuumCriticalitySourceAuditPassed
    ? "higgs-vacuum-criticality-source-audit-near-critical-not-promotion"
    : "higgs-vacuum-criticality-source-audit-review-required";

var result = new
{
    phaseId = "phase264-higgs-vacuum-criticality-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    higgsVacuumCriticalitySourceAuditPassed,
    vacuumCriticalityPromotesHiggsMass,
    vacuumCriticalityCompletesBosonPredictions,
    vacuumCriticalityBoundaryNumericallyNearHiggsMass,
    vacuumCriticalityBoundaryEqualsTarget,
    criticalityBoundary = new
    {
        formulaId = "buttazzo-2013-absolute-stability-boundary-approximation",
        expression = "M_h^crit ~= 129.6 + 2.0*(M_t - 173.34) - 0.5*((alpha_s - 0.1184)/0.0007) GeV",
        measuredTopMassGeV,
        measuredTopMassUncertaintyGeV,
        alphaSAtMz,
        alphaSUncertainty,
        stabilityFormulaTheoryUncertaintyGeV,
        absoluteStabilityBoundaryHiggsMassGeV,
        absoluteStabilityBoundaryUncertaintyGeV,
        higgsTargetGeV,
        higgsTargetUncertaintyGeV,
        targetToStabilityBoundaryGapGeV,
        targetToStabilityBoundaryPull,
    },
    sourceLineageBoundary = new
    {
        vacuumCriticalityUsesExternalSmRgInputs,
        vacuumCriticalityConditionAssumedNotDerived,
        vacuumCriticalityProvidesGuScalarPotentialSource,
        vacuumCriticalityProvidesGuQuarticBoundarySource,
        vacuumCriticalityProvidesGuTopYukawaSource,
        vacuumCriticalityProvidesGuVevSource,
        vacuumCriticalityProvidesObservedFieldExtraction,
    },
    currentBlockerEvidence = new
    {
        phase215 = new
        {
            canPromoteTargetImpliedHiggsSelfCoupling,
        },
        phase224 = new
        {
            wAbsoluteMassParameterClosure,
            higgsMassParameterClosure,
        },
        phase237 = new
        {
            coxIiHiggsYukawaTexturePromotable,
        },
        phase248 = new
        {
            higgsScalarRepairPossibleFromCurrentRegistry,
            newHiggsScalarSourceStillRequired,
        },
        phase262 = new
        {
            p262RelationPromotesHiggsMass,
        },
        phase263 = new
        {
            topYukawaUnityPromotesHiggsMass,
            topYukawaUnityNumericallyCloses,
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
            "PDG reports that top mass is crucial for vacuum stability and that current measurements suggest a nearly vanishing Higgs quartic near the Planck scale, but no clear UV picture is supplied."),
        new ExternalSource(
            "buttazzo-2013-near-criticality",
            "https://arxiv.org/abs/1307.3536",
            "The near-criticality analysis extracts SM parameters from data and studies phase boundaries; it treats the Higgs mass as near a metastability boundary, not as a GU source-lineage prediction."),
        new ExternalSource(
            "degrassi-2012-nnlo-vacuum-stability",
            "https://arxiv.org/abs/1205.6497",
            "NNLO stability analyses map Higgs/top/alpha_s inputs to vacuum-stability conditions, which are external SM RG constraints rather than GU scalar-source artifacts."),
    },
    checks,
    decision = higgsVacuumCriticalitySourceAuditPassed
        ? "Do not promote a Higgs mass prediction from Standard Model vacuum criticality. The observed Higgs mass is near a criticality/stability boundary, but the boundary imports external SM RG inputs and an assumed high-scale condition, does not derive GU Higgs scalar-source, quartic, top/Yukawa, VEV, or observed-field extraction artifacts, and does not complete W/Z absolute masses."
        : "Review Higgs vacuum criticality source audit before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived high-scale boundary condition for the Higgs quartic and beta function if criticality is to be predictive.",
        "A GU top/Yukawa and RG-transport source lineage, not measured top-mass input.",
        "A solved Higgs scalar-source/operator, observed-field extraction theorem, and GU VEV/source-scale row.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase213Path = Phase213Path,
        phase215Path = Phase215Path,
        phase224Path = Phase224Path,
        phase237Path = Phase237Path,
        phase248Path = Phase248Path,
        phase262Path = Phase262Path,
        phase263Path = Phase263Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_vacuum_criticality_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_vacuum_criticality_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.higgsVacuumCriticalitySourceAuditPassed,
        result.vacuumCriticalityPromotesHiggsMass,
        result.vacuumCriticalityCompletesBosonPredictions,
        result.vacuumCriticalityBoundaryNumericallyNearHiggsMass,
        result.vacuumCriticalityBoundaryEqualsTarget,
        result.criticalityBoundary,
        result.sourceLineageBoundary,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"higgsVacuumCriticalitySourceAuditPassed={higgsVacuumCriticalitySourceAuditPassed}");
Console.WriteLine($"vacuumCriticalityPromotesHiggsMass={vacuumCriticalityPromotesHiggsMass}");
Console.WriteLine($"absoluteStabilityBoundaryHiggsMassGeV={absoluteStabilityBoundaryHiggsMassGeV}");
Console.WriteLine($"targetToStabilityBoundaryPull={targetToStabilityBoundaryPull}");

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
