using System.Text.Json;

const string DefaultOutputDir = "studies/phase262_higgs_top_empirical_relation_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase237Path = "studies/phase237_cox_ii_higgs_yukawa_texture_dependency_audit_001/output/cox_ii_higgs_yukawa_texture_dependency_audit_summary.json";
const string Phase248Path = "studies/phase248_higgs_scalar_repairability_audit_001/output/higgs_scalar_repairability_audit_summary.json";
const string Phase261Path = "studies/phase261_electroweak_scheme_radiative_source_audit_001/output/electroweak_scheme_radiative_source_audit_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE262_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase237 = JsonDocument.Parse(File.ReadAllText(Phase237Path));
using var phase248 = JsonDocument.Parse(File.ReadAllText(Phase248Path));
using var phase261 = JsonDocument.Parse(File.ReadAllText(Phase261Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var rows = phase148.RootElement.GetProperty("comparisonRows").EnumerateArray().ToArray();
var wRow = FindRow(rows, "w-boson");
var zRow = FindRow(rows, "z-boson");
var higgsRow = FindRow(rows, "higgs");

var wTargetGeV = RequiredDouble(wRow, "targetValue");
var zTargetGeV = RequiredDouble(zRow, "targetValue");
var higgsTargetGeV = RequiredDouble(higgsRow, "targetValue");
var higgsTargetUncertaintyGeV = RequiredDouble(higgsRow, "targetUncertainty");

var topMassGeV = 172.52;
var topMassUncertaintyGeV = 0.33;
var topMassIsExternalMeasuredFermionInput = true;
var topYukawaSourceLineagePresent = false;
var relationHasGuDerivation = false;
var relationProvidesHiggsScalarSource = false;
var relationProvidesPotentialOrSelfCouplingSource = false;
var relationProvidesObservedFieldExtraction = false;
var relationProvidesWzAbsoluteScale = false;
var relationPromotesHiggsMass = false;

var geometricMeanHiggsGeV = Math.Sqrt(zTargetGeV * topMassGeV);
var geometricMeanSigmaGeV = 0.5 * geometricMeanHiggsGeV * Math.Sqrt(
    Math.Pow(0.0021 / zTargetGeV, 2.0) +
    Math.Pow(topMassUncertaintyGeV / topMassGeV, 2.0));
var geometricMeanPull = Pull(geometricMeanHiggsGeV, geometricMeanSigmaGeV, higgsTargetGeV, higgsTargetUncertaintyGeV);
var geometricMeanRatio = higgsTargetGeV * higgsTargetGeV / (zTargetGeV * topMassGeV);

var averageWTopHiggsGeV = 0.5 * (wTargetGeV + topMassGeV);
var averageWTopSigmaGeV = 0.5 * Math.Sqrt(0.0133 * 0.0133 + topMassUncertaintyGeV * topMassUncertaintyGeV);
var averageWTopPull = Pull(averageWTopHiggsGeV, averageWTopSigmaGeV, higgsTargetGeV, higgsTargetUncertaintyGeV);
var averageWTopRatio = (wTargetGeV + topMassGeV) / (2.0 * higgsTargetGeV);

var canPromoteAnyHiggsScalarSourceLineage = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var coxIiHiggsYukawaTexturePromotable = JsonBool(phase237.RootElement, "coxIiHiggsYukawaTexturePromotableForHiggsMass") is true;
var higgsScalarRepairPossibleFromCurrentRegistry = JsonBool(phase248.RootElement, "higgsScalarSourceRepairPossibleFromCurrentRegistry") is true;
var newHiggsScalarSourceStillRequired = JsonBool(phase248.RootElement, "newHiggsScalarSourceStillRequired") is true;
var phase261SchemeChoicePromotesBosonMasses = JsonBool(phase261.RootElement, "schemeChoicePromotesBosonMasses") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var relationNumericallyClose = geometricMeanPull < 3.0 || Math.Abs(geometricMeanRatio - 1.0) < 0.01;
var higgsTopEmpiricalRelationSourceAuditPassed = relationNumericallyClose
    && topMassIsExternalMeasuredFermionInput
    && !topYukawaSourceLineagePresent
    && !relationHasGuDerivation
    && !relationProvidesHiggsScalarSource
    && !relationProvidesPotentialOrSelfCouplingSource
    && !relationProvidesObservedFieldExtraction
    && !relationProvidesWzAbsoluteScale
    && !relationPromotesHiggsMass
    && !canPromoteAnyHiggsScalarSourceLineage
    && !coxIiHiggsYukawaTexturePromotable
    && !higgsScalarRepairPossibleFromCurrentRegistry
    && newHiggsScalarSourceStillRequired
    && !phase261SchemeChoicePromotesBosonMasses
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var checks = new[]
{
    new Check(
        "higgs-top-relations-are-numerical-coincidence-leads",
        relationNumericallyClose,
        $"geometricMeanHiggsGeV={geometricMeanHiggsGeV:R}; geometricMeanPull={geometricMeanPull:R}; geometricMeanRatio={geometricMeanRatio:R}; averageWTopHiggsGeV={averageWTopHiggsGeV:R}; averageWTopPull={averageWTopPull:R}; averageWTopRatio={averageWTopRatio:R}"),
    new Check(
        "top-input-is-external-not-gu-source-lineage",
        topMassIsExternalMeasuredFermionInput && !topYukawaSourceLineagePresent && !relationHasGuDerivation,
        $"topMassIsExternalMeasuredFermionInput={topMassIsExternalMeasuredFermionInput}; topYukawaSourceLineagePresent={topYukawaSourceLineagePresent}; relationHasGuDerivation={relationHasGuDerivation}"),
    new Check(
        "relation-does-not-fill-higgs-source-contract",
        !relationProvidesHiggsScalarSource && !relationProvidesPotentialOrSelfCouplingSource && !relationProvidesObservedFieldExtraction,
        $"relationProvidesHiggsScalarSource={relationProvidesHiggsScalarSource}; relationProvidesPotentialOrSelfCouplingSource={relationProvidesPotentialOrSelfCouplingSource}; relationProvidesObservedFieldExtraction={relationProvidesObservedFieldExtraction}"),
    new Check(
        "relation-does-not-fill-wz-absolute-scale",
        !relationProvidesWzAbsoluteScale && wzMissingFieldCount > 0,
        $"relationProvidesWzAbsoluteScale={relationProvidesWzAbsoluteScale}; wzMissingFieldCount={wzMissingFieldCount}"),
    new Check(
        "current-higgs-lineage-blockers-preserved",
        !canPromoteAnyHiggsScalarSourceLineage
            && !coxIiHiggsYukawaTexturePromotable
            && !higgsScalarRepairPossibleFromCurrentRegistry
            && newHiggsScalarSourceStillRequired
            && higgsMissingFieldCount > 0,
        $"canPromoteAnyHiggsScalarSourceLineage={canPromoteAnyHiggsScalarSourceLineage}; coxIiHiggsYukawaTexturePromotable={coxIiHiggsYukawaTexturePromotable}; higgsScalarRepairPossibleFromCurrentRegistry={higgsScalarRepairPossibleFromCurrentRegistry}; newHiggsScalarSourceStillRequired={newHiggsScalarSourceStillRequired}; higgsMissingFieldCount={higgsMissingFieldCount}"),
};

var terminalStatus = higgsTopEmpiricalRelationSourceAuditPassed
    ? "higgs-top-empirical-relation-source-audit-numerical-lead-not-promotion"
    : "higgs-top-empirical-relation-source-audit-review-required";

var result = new
{
    phaseId = "phase262-higgs-top-empirical-relation-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    higgsTopEmpiricalRelationSourceAuditPassed,
    relationPromotesHiggsMass,
    relationNumericallyClose,
    empiricalRelations = new
    {
        topMassGeV,
        topMassUncertaintyGeV,
        topMassIsExternalMeasuredFermionInput,
        topYukawaSourceLineagePresent,
        relationHasGuDerivation,
        relationProvidesHiggsScalarSource,
        relationProvidesPotentialOrSelfCouplingSource,
        relationProvidesObservedFieldExtraction,
        relationProvidesWzAbsoluteScale,
        geometricMean = new
        {
            relationId = "mh-squared-approximately-mz-mt",
            expression = "m_H^2 ~= m_Z * m_t",
            geometricMeanHiggsGeV,
            geometricMeanSigmaGeV,
            geometricMeanPull,
            geometricMeanRatio,
        },
        arithmeticMean = new
        {
            relationId = "two-mh-approximately-mw-plus-mt",
            expression = "2 m_H ~= m_W + m_t",
            averageWTopHiggsGeV,
            averageWTopSigmaGeV,
            averageWTopPull,
            averageWTopRatio,
        },
    },
    currentBlockerEvidence = new
    {
        phase199 = new
        {
            canPromoteAnyHiggsScalarSourceLineage,
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
        phase261 = new
        {
            phase261SchemeChoicePromotesBosonMasses,
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
            "PDG top review reports direct LHC measurements yielding m_t = 172.52 +/- 0.33 GeV and stresses mass-scheme issues."),
        new ExternalSource(
            "torrente-lujan-2014-higgs-top-coincidence",
            "https://link.springer.com/article/10.1140/epjc/s10052-014-2744-3",
            "The Higgs-top/Z relation is presented as an empirical mass coincidence, not a solved scalar-source theorem."),
        new ExternalSource(
            "arxiv-1209-0474",
            "https://arxiv.org/abs/1209.0474",
            "The arXiv abstract frames m_H^2 ~= m_Z m_t and 2m_H ~= m_W + m_t as ratios needing an underlying mechanism."),
    },
    checks,
    decision = higgsTopEmpiricalRelationSourceAuditPassed
        ? "Do not promote a Higgs mass prediction from Higgs-top empirical relations. The m_H^2 ~= m_Z m_t relation is numerically close, but it imports the measured top-quark mass and existing boson targets, has no GU derivation, and supplies no Higgs scalar-source/operator, potential/self-coupling source, observed-field extraction, or W/Z absolute-scale row."
        : "Review Higgs-top empirical relation source audit before relying on this numerical lead boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived top/Yukawa source lineage if top-sector relations are to be used in a Higgs prediction.",
        "A solved Higgs scalar-source/operator with potential or excitation relation, independent of the observed Higgs mass.",
        "A W/Z absolute-scale source lineage if a relation is intended to complete all known boson masses.",
    },
    sourceEvidence = new
    {
        phase148Path = Phase148Path,
        phase199Path = Phase199Path,
        phase237Path = Phase237Path,
        phase248Path = Phase248Path,
        phase261Path = Phase261Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "higgs_top_empirical_relation_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "higgs_top_empirical_relation_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.higgsTopEmpiricalRelationSourceAuditPassed,
        result.relationPromotesHiggsMass,
        result.relationNumericallyClose,
        result.empiricalRelations,
        result.currentBlockerEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"higgsTopEmpiricalRelationSourceAuditPassed={higgsTopEmpiricalRelationSourceAuditPassed}");
Console.WriteLine($"relationPromotesHiggsMass={relationPromotesHiggsMass}");
Console.WriteLine($"geometricMeanHiggsGeV={geometricMeanHiggsGeV}");

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
