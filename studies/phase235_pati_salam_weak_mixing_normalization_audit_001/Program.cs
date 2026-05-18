using System.Text.Json;

const string DefaultOutputDir = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase232Path = "studies/phase232_external_cox_gu_paper_ii_source_intake_audit_001/output/external_cox_gu_paper_ii_source_intake_audit_summary.json";
const string Phase234Path = "studies/phase234_cox_ii_electroweak_formula_dependency_audit_001/output/cox_ii_electroweak_formula_dependency_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE235_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase232 = JsonDocument.Parse(File.ReadAllText(Phase232Path));
using var phase234 = JsonDocument.Parse(File.ReadAllText(Phase234Path));

var coxIiGaugeMassMatrixLeadPresent = phase232.RootElement.TryGetProperty("claimedResearchLeads", out var p232Leads)
    && JsonBool(p232Leads, "paperIIGaugeMassMatrixLeadPresent") is true;
var coxIiGaugeNormalizationLeadPresent = phase232.RootElement.TryGetProperty("claimedResearchLeads", out p232Leads)
    && JsonBool(p232Leads, "paperIIGaugeNormalizationLeadPresent") is true;
var coxIiSymbolicFormulaLeadPresent = JsonBool(phase234.RootElement, "coxIiSymbolicElectroweakFormulaLeadPresent") is true;
var coxIiFormulaPromotable = JsonBool(phase234.RootElement, "symbolicFormulaLeadPromotableForAbsoluteMasses") is true;
var canPromoteWzFromWeakCouplingMassRelation = JsonBool(phase197.RootElement, "canPromoteWzFromWeakCouplingMassRelation") is true;
var closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(closure, "zAbsoluteMassParameterClosure") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;

var patiSalamHyperchargeEmbeddingLeadPresent = true;
var highScaleWeakMixingBoundaryPresent = true;
var canonicalHighScaleSin2ThetaW = 3.0 / 8.0;
var canonicalHighScaleCosThetaW = Math.Sqrt(1.0 - canonicalHighScaleSin2ThetaW);
var naiveHighScaleWzMassRatio = canonicalHighScaleCosThetaW;
var unificationScaleBoundaryOnly = true;
var guBreakingScalePresent = false;
var rgEvolutionToElectroweakScalePresent = false;
var thresholdCorrectionsPresent = false;
var lowEnergyWeakMixingPredictionPresent = false;
var targetIndependentLowEnergyGYPredictionPresent = false;
var patiSalamNormalizationPromotableForLowEnergyWz =
    patiSalamHyperchargeEmbeddingLeadPresent
    && highScaleWeakMixingBoundaryPresent
    && !unificationScaleBoundaryOnly
    && guBreakingScalePresent
    && rgEvolutionToElectroweakScalePresent
    && thresholdCorrectionsPresent
    && lowEnergyWeakMixingPredictionPresent
    && targetIndependentLowEnergyGYPredictionPresent
    && canPromoteWzFromWeakCouplingMassRelation
    && wAbsoluteMassParameterClosure
    && zAbsoluteMassParameterClosure
    && coxIiFormulaPromotable;

var requiredTransport = new[]
{
    new TransportRequirement("gu-breaking-scale", false, "No GU-derived Pati-Salam or left-right breaking scale is materialized."),
    new TransportRequirement("rg-evolution-to-electroweak-scale", false, "No repository RG flow transports the high-scale normalization to the W/Z comparison scale."),
    new TransportRequirement("threshold-corrections", false, "No heavy-threshold or scalar-sector threshold corrections are supplied."),
    new TransportRequirement("low-energy-hypercharge-coupling", false, "No target-independent low-energy g_Y or weak-mixing value is supplied."),
};

var checks = new[]
{
    new Check("pati-salam-normalization-lead-present", patiSalamHyperchargeEmbeddingLeadPresent && coxIiGaugeNormalizationLeadPresent, $"patiSalamHyperchargeEmbeddingLeadPresent={patiSalamHyperchargeEmbeddingLeadPresent}; coxIiGaugeNormalizationLeadPresent={coxIiGaugeNormalizationLeadPresent}"),
    new Check("symbolic-wz-formula-context-present", coxIiGaugeMassMatrixLeadPresent && coxIiSymbolicFormulaLeadPresent, $"coxIiGaugeMassMatrixLeadPresent={coxIiGaugeMassMatrixLeadPresent}; coxIiSymbolicFormulaLeadPresent={coxIiSymbolicFormulaLeadPresent}"),
    new Check("high-scale-boundary-is-not-low-energy-prediction", highScaleWeakMixingBoundaryPresent && unificationScaleBoundaryOnly && !lowEnergyWeakMixingPredictionPresent, $"canonicalHighScaleSin2ThetaW={canonicalHighScaleSin2ThetaW}; unificationScaleBoundaryOnly={unificationScaleBoundaryOnly}; lowEnergyWeakMixingPredictionPresent={lowEnergyWeakMixingPredictionPresent}"),
    new Check("transport-requirements-missing", requiredTransport.All(row => !row.Filled), $"missingTransportCount={requiredTransport.Count(row => !row.Filled)}"),
    new Check("wz-parameter-closure-still-blocked", !canPromoteWzFromWeakCouplingMassRelation && !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && wzMissingFieldCount > 0, $"canPromoteWzFromWeakCouplingMassRelation={canPromoteWzFromWeakCouplingMassRelation}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; wzMissingFieldCount={wzMissingFieldCount}"),
    new Check("pati-salam-normalization-not-promotable-for-low-energy-wz", !patiSalamNormalizationPromotableForLowEnergyWz, $"patiSalamNormalizationPromotableForLowEnergyWz={patiSalamNormalizationPromotableForLowEnergyWz}"),
};

var patiSalamWeakMixingNormalizationAuditPassed = checks.All(check => check.Passed)
    && !patiSalamNormalizationPromotableForLowEnergyWz;
var terminalStatus = patiSalamWeakMixingNormalizationAuditPassed
    ? "pati-salam-weak-mixing-normalization-boundary-not-low-energy-wz-source"
    : "pati-salam-weak-mixing-normalization-review-required";

var result = new
{
    phaseId = "phase235-pati-salam-weak-mixing-normalization-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    patiSalamHyperchargeEmbeddingLeadPresent,
    highScaleWeakMixingBoundaryPresent,
    canonicalHighScaleSin2ThetaW,
    canonicalHighScaleCosThetaW,
    naiveHighScaleWzMassRatio,
    patiSalamNormalizationPromotableForLowEnergyWz,
    patiSalamWeakMixingNormalizationAuditPassed,
    objective = "Determine whether Pati-Salam/left-right hypercharge normalization closes the W/Z weak-mixing source requirement or remains only a high-scale boundary condition.",
    normalizationLead = new
    {
        source = "Pati-Salam/left-right hypercharge embedding and Cox GU II gauge-normalization lead",
        hyperchargeEmbedding = "Y = T3_R + (B-L)/2",
        canonicalHighScaleBoundary = "sin^2(theta_W)=3/8 when canonically normalized gauge couplings unify",
        canonicalHighScaleSin2ThetaW,
        canonicalHighScaleCosThetaW,
        interpretation = "This supplies a high-scale normalization boundary. It is not a low-energy W/Z prediction without a GU-derived breaking scale, RG transport, thresholds, and low-energy coupling values.",
    },
    requiredTransport,
    currentRepoEvidence = new
    {
        phase197 = new
        {
            status = JsonString(phase197.RootElement, "terminalStatus"),
            canPromoteWzFromWeakCouplingMassRelation,
            currentWeakCoupling = JsonDouble(phase197.RootElement, "currentWeakCoupling"),
            targetImpliedWeakCoupling = JsonDouble(phase197.RootElement, "targetImpliedWeakCoupling"),
        },
        phase213 = new
        {
            status = JsonString(phase213.RootElement, "terminalStatus"),
            wzMissingFieldCount,
        },
        phase224 = new
        {
            status = JsonString(phase224.RootElement, "terminalStatus"),
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
        },
        phase232 = new
        {
            status = JsonString(phase232.RootElement, "terminalStatus"),
            coxIiGaugeMassMatrixLeadPresent,
            coxIiGaugeNormalizationLeadPresent,
        },
        phase234 = new
        {
            status = JsonString(phase234.RootElement, "terminalStatus"),
            coxIiSymbolicFormulaLeadPresent,
            coxIiFormulaPromotable,
        },
    },
    checks,
    decision = patiSalamWeakMixingNormalizationAuditPassed
        ? "Do not promote W/Z masses from the Pati-Salam normalization alone. It is a high-scale hypercharge/weak-mixing boundary condition, not a target-independent low-energy W/Z source without GU-derived breaking scale, RG evolution, thresholds, and coupling values."
        : "Review Pati-Salam weak-mixing normalization before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived Pati-Salam or left-right breaking scale.",
        "An RG/threshold transport from the high-scale normalization to the electroweak comparison scale.",
        "A target-independent low-energy g_Y or weak-mixing source row.",
        "Repository W/Z particle rows with replay, stability, and target-comparison gates.",
    },
    sourceEvidence = new
    {
        phase197Path = Phase197Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase232Path = Phase232Path,
        phase234Path = Phase234Path,
        coxGuIiDiscoveryUrl = "https://www.researchgate.net/publication/396557260_Geometric_Unity_II_Matter_Symmetry_on_the_Observation_Slice_One-Family_Factorization_Pati-Salam_Embedding_Anomaly_Closure_and_Embryo_HiggsYukawa_Textures",
        scholarpediaGrandUnificationUrl = "https://www.scholarpedia.org/article/Grand_unification",
        spectralPatiSalamReferenceUrl = "https://link.springer.com/chapter/10.1007/978-3-031-59120-4_15",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "pati_salam_weak_mixing_normalization_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "pati_salam_weak_mixing_normalization_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.patiSalamHyperchargeEmbeddingLeadPresent,
        result.highScaleWeakMixingBoundaryPresent,
        result.canonicalHighScaleSin2ThetaW,
        result.canonicalHighScaleCosThetaW,
        result.naiveHighScaleWzMassRatio,
        result.patiSalamNormalizationPromotableForLowEnergyWz,
        result.patiSalamWeakMixingNormalizationAuditPassed,
        result.normalizationLead,
        result.requiredTransport,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"patiSalamHyperchargeEmbeddingLeadPresent={patiSalamHyperchargeEmbeddingLeadPresent}");
Console.WriteLine($"patiSalamNormalizationPromotableForLowEnergyWz={patiSalamNormalizationPromotableForLowEnergyWz}");
Console.WriteLine($"patiSalamWeakMixingNormalizationAuditPassed={patiSalamWeakMixingNormalizationAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record Check(string CheckId, bool Passed, string Detail);
sealed record TransportRequirement(string RequirementId, bool Filled, string Detail);
