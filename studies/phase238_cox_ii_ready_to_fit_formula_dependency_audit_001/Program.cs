using System.Text.Json;

const string DefaultOutputDir = "studies/phase238_cox_ii_ready_to_fit_formula_dependency_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase232Path = "studies/phase232_external_cox_gu_paper_ii_source_intake_audit_001/output/external_cox_gu_paper_ii_source_intake_audit_summary.json";
const string Phase234Path = "studies/phase234_cox_ii_electroweak_formula_dependency_audit_001/output/cox_ii_electroweak_formula_dependency_audit_summary.json";
const string Phase235Path = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output/pati_salam_weak_mixing_normalization_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE238_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase232 = JsonDocument.Parse(File.ReadAllText(Phase232Path));
using var phase234 = JsonDocument.Parse(File.ReadAllText(Phase234Path));
using var phase235 = JsonDocument.Parse(File.ReadAllText(Phase235Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));

var phase224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var targetIndependentGuVevSourcePromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var externalCoxPaperIISourceIntakeAuditPassed = JsonBool(phase232.RootElement, "externalCoxPaperIISourceIntakeAuditPassed") is true;
var externalCoxPaperIIPromotableForBosonMasses = JsonBool(phase232.RootElement, "externalCoxPaperIIPromotableForBosonMasses") is true;
var coxIiElectroweakFormulaDependencyAuditPassed = JsonBool(phase234.RootElement, "coxIiElectroweakFormulaDependencyAuditPassed") is true;
var symbolicFormulaLeadPromotableForAbsoluteMasses = JsonBool(phase234.RootElement, "symbolicFormulaLeadPromotableForAbsoluteMasses") is true;
var patiSalamNormalizationPromotableForLowEnergyWz = JsonBool(phase235.RootElement, "patiSalamNormalizationPromotableForLowEnergyWz") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var coxIiReadyToFitFormulaLeadPresent = true;
var coxIiAppendixSReviewed = true;
var readyToFitFormulasAlgebraicallyClosed = true;
var formulasAdvertisedForGlobalFits = true;
var formulasUseTargetObservables = false;
var readyToFitFormulasProvideFixedParameterValues = false;
var readyToFitFormulasProvideObservedWzHRows = false;
var readyToFitFormulasProvideHiggsPotentialOrMass = false;

var fitParameterRequirements = new[]
{
    new FitParameterRequirement("g_L", true, false, "No GU-derived low-energy left weak coupling source is present."),
    new FitParameterRequirement("g_R", true, false, "No GU-derived right weak coupling source and breaking-scale transport source are present."),
    new FitParameterRequirement("g_B-L", true, false, "No GU-derived B-L coupling source and transport row are present."),
    new FitParameterRequirement("kappa", true, targetIndependentGuVevSourcePromotable, "The electroweak bi-doublet scale is not supplied as a GU VEV source."),
    new FitParameterRequirement("beta", true, false, "No GU-derived tan(beta) or kappa_u/kappa_d source is present."),
    new FitParameterRequirement("v_R", true, false, "No GU-derived SU(2)_R breaking-scale source is present."),
    new FitParameterRequirement("g_4", true, false, "No GU-derived SU(4) coupling source is present."),
    new FitParameterRequirement("v_4", true, false, "No GU-derived SU(4) breaking-scale source is present."),
    new FitParameterRequirement("c_X", true, false, "No representation/source normalization fixing the leptoquark vector coefficient is promotable."),
};

var missingPromotionRequirements = new[]
{
    new Requirement("fixed-fit-parameter-source-rows", readyToFitFormulasProvideFixedParameterValues, "Appendix S lists formulas in terms of parameters; it does not derive fixed parameter values."),
    new Requirement("gu-breaking-scale-source", false, "No GU source fixes v_R or v_4."),
    new Requirement("low-energy-rg-threshold-transport", lowEnergyRgTransportSourcePromotable, "No promotable RG/threshold transport source maps high-scale normalization to low-energy W/Z parameters."),
    new Requirement("observed-wz-particle-rows", readyToFitFormulasProvideObservedWzHRows, "No W/Z source rows with replay, stability, and target-comparison gates are supplied."),
    new Requirement("higgs-potential-or-mass-row", readyToFitFormulasProvideHiggsPotentialOrMass, "The ready-to-fit gauge formulas do not supply a Higgs potential, quartic, scalar profile, or Higgs mass row."),
    new Requirement("post-fit-is-not-pre-fit-prediction", false, "Global-fit readiness is not a target-independent pre-fit prediction under the repository gate."),
};

var coxIiReadyToFitFormulaPromotableForBosonMasses =
    coxIiReadyToFitFormulaLeadPresent
    && readyToFitFormulasAlgebraicallyClosed
    && readyToFitFormulasProvideFixedParameterValues
    && readyToFitFormulasProvideObservedWzHRows
    && readyToFitFormulasProvideHiggsPotentialOrMass
    && fitParameterRequirements.All(row => row.PromotableSourcePresent)
    && missingPromotionRequirements.All(row => row.Filled)
    && wAbsoluteMassParameterClosure
    && zAbsoluteMassParameterClosure
    && higgsMassParameterClosure
    && externalCoxPaperIIPromotableForBosonMasses
    && !symbolicFormulaLeadPromotableForAbsoluteMasses
    && patiSalamNormalizationPromotableForLowEnergyWz
    && lowEnergyRgTransportSourcePromotable;

var checks = new[]
{
    new Check("cox-ii-appendix-s-ready-to-fit-lead-present", coxIiReadyToFitFormulaLeadPresent && coxIiAppendixSReviewed && readyToFitFormulasAlgebraicallyClosed, $"leadPresent={coxIiReadyToFitFormulaLeadPresent}; appendixSReviewed={coxIiAppendixSReviewed}; algebraicallyClosed={readyToFitFormulasAlgebraicallyClosed}"),
    new Check("ready-to-fit-means-parameterized-global-fit-not-fixed-prediction", formulasAdvertisedForGlobalFits && !readyToFitFormulasProvideFixedParameterValues && !formulasUseTargetObservables, $"formulasAdvertisedForGlobalFits={formulasAdvertisedForGlobalFits}; fixedParameterValues={readyToFitFormulasProvideFixedParameterValues}; targetObservablesUsed={formulasUseTargetObservables}"),
    new Check("required-fit-parameters-not-promotable", fitParameterRequirements.Count(row => row.PromotableSourcePresent) == 0, $"promotableFitParameterCount={fitParameterRequirements.Count(row => row.PromotableSourcePresent)}; requiredFitParameterCount={fitParameterRequirements.Length}"),
    new Check("known-boson-rows-not-filled-by-ready-to-fit-formulas", !readyToFitFormulasProvideObservedWzHRows && !readyToFitFormulasProvideHiggsPotentialOrMass && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("phase224-phase229-source-closure-still-blocked", !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && !higgsMassParameterClosure && !targetIndependentGuVevSourcePromotable, $"wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; guVevSource={targetIndependentGuVevSourcePromotable}"),
    new Check("phase232-phase234-phase235-phase236-block-promotion", externalCoxPaperIISourceIntakeAuditPassed && !externalCoxPaperIIPromotableForBosonMasses && coxIiElectroweakFormulaDependencyAuditPassed && !symbolicFormulaLeadPromotableForAbsoluteMasses && !patiSalamNormalizationPromotableForLowEnergyWz && !lowEnergyRgTransportSourcePromotable, $"phase232Promotable={externalCoxPaperIIPromotableForBosonMasses}; phase234Promotable={symbolicFormulaLeadPromotableForAbsoluteMasses}; phase235Promotable={patiSalamNormalizationPromotableForLowEnergyWz}; phase236Promotable={lowEnergyRgTransportSourcePromotable}"),
    new Check("ready-to-fit-formula-lead-not-boson-mass-prediction", !coxIiReadyToFitFormulaPromotableForBosonMasses, $"coxIiReadyToFitFormulaPromotableForBosonMasses={coxIiReadyToFitFormulaPromotableForBosonMasses}"),
};

var coxIiReadyToFitFormulaDependencyAuditPassed = checks.All(check => check.Passed)
    && !coxIiReadyToFitFormulaPromotableForBosonMasses;
var terminalStatus = coxIiReadyToFitFormulaDependencyAuditPassed
    ? "cox-ii-ready-to-fit-formula-lead-parameterized-not-prediction"
    : "cox-ii-ready-to-fit-formula-dependency-review-required";

var result = new
{
    phaseId = "phase238-cox-ii-ready-to-fit-formula-dependency-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    coxIiReadyToFitFormulaLeadPresent,
    readyToFitFormulasAlgebraicallyClosed,
    formulasAdvertisedForGlobalFits,
    readyToFitFormulasProvideFixedParameterValues,
    coxIiReadyToFitFormulaPromotableForBosonMasses,
    coxIiReadyToFitFormulaDependencyAuditPassed,
    objective = "Determine whether Cox GU II Appendix S ready-to-fit formulas for mixings and masses close W/Z/H target-independent boson predictions or remain parameterized fit relations.",
    externalLead = new
    {
        title = "Geometric Unity II: Matter & Symmetry on the Observation Slice One-Family Factorization, Pati-Salam Embedding, Anomaly Closure, and Embryo Higgs/Yukawa Textures",
        sourceUrl = "https://www.researchgate.net/publication/396557260_Geometric_Unity_II_Matter_Symmetry_on_the_Observation_Slice_One-Family_Factorization_Pati-Salam_Embedding_Anomaly_Closure_and_Embryo_HiggsYukawa_Textures",
        section = "Appendix S: Ready-to-Fit Formulas for Mixings and Masses",
        formulaFamilies = new[]
        {
            "charged W_L/W_R mixing and masses",
            "neutral Z/Z' mixing and masses",
            "Pati-Salam hypercharge relation",
            "SU(4) leptoquark vector mass and decay width",
        },
        interpretation = "The formulas are useful phenomenological relations with fixed kernels and normalizations, but they are expressed in terms of fit/source parameters rather than fixed target-independent boson mass predictions.",
    },
    formulaDependencySummary = new
    {
        chargedSector = "theta_WW' and m_W^2, m_WR^2 depend on g_L, g_R, kappa, beta, and v_R.",
        neutralSector = "theta_ZZ', m_Z' and g_Y depend on g_L, g_R, g_B-L, kappa, beta, and v_R.",
        leptoquarkSector = "m_X and width depend on c_X, g_4, and v_4.",
        predictionBoundary = "A global fit can estimate these parameters from data; a repository prediction requires independent GU source rows for them before comparing to observed W/Z/H masses.",
    },
    fitParameterRequirements,
    missingPromotionRequirements,
    currentRepoEvidence = new
    {
        phase213 = new
        {
            status = JsonString(phase213.RootElement, "terminalStatus"),
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
        phase224 = new
        {
            status = JsonString(phase224.RootElement, "terminalStatus"),
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
            higgsMassParameterClosure,
        },
        phase229 = new
        {
            status = JsonString(phase229.RootElement, "terminalStatus"),
            targetIndependentGuVevSourcePromotable,
        },
        phase232 = new
        {
            status = JsonString(phase232.RootElement, "terminalStatus"),
            externalCoxPaperIISourceIntakeAuditPassed,
            externalCoxPaperIIPromotableForBosonMasses,
        },
        phase234 = new
        {
            status = JsonString(phase234.RootElement, "terminalStatus"),
            coxIiElectroweakFormulaDependencyAuditPassed,
            symbolicFormulaLeadPromotableForAbsoluteMasses,
        },
        phase235 = new
        {
            status = JsonString(phase235.RootElement, "terminalStatus"),
            patiSalamNormalizationPromotableForLowEnergyWz,
        },
        phase236 = new
        {
            status = JsonString(phase236.RootElement, "terminalStatus"),
            lowEnergyRgTransportSourcePromotable,
        },
    },
    checks,
    decision = coxIiReadyToFitFormulaDependencyAuditPassed
        ? "Do not promote Cox II ready-to-fit formulas as W/Z/H predictions. They are algebraically useful, but remain parameterized global-fit relations until GU-derived parameter, scale, RG/threshold, observed-row, stability, and target-comparison source artifacts are supplied."
        : "Review Cox II ready-to-fit formula dependencies before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "GU-derived source rows for g_L, g_R, g_B-L, kappa, beta, v_R, g_4, v_4, and c_X as applicable.",
        "RG/threshold transport from the high-scale Pati-Salam formulas to the electroweak comparison scale.",
        "W/Z particle rows with replay, stability, and post-construction target-comparison gates.",
        "A Higgs scalar-source/potential package if the same route is to claim the Higgs mass.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase229Path = Phase229Path,
        phase232Path = Phase232Path,
        phase234Path = Phase234Path,
        phase235Path = Phase235Path,
        phase236Path = Phase236Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "cox_ii_ready_to_fit_formula_dependency_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "cox_ii_ready_to_fit_formula_dependency_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.coxIiReadyToFitFormulaLeadPresent,
        result.readyToFitFormulasAlgebraicallyClosed,
        result.formulasAdvertisedForGlobalFits,
        result.readyToFitFormulasProvideFixedParameterValues,
        result.coxIiReadyToFitFormulaPromotableForBosonMasses,
        result.coxIiReadyToFitFormulaDependencyAuditPassed,
        result.externalLead,
        result.formulaDependencySummary,
        result.fitParameterRequirements,
        result.missingPromotionRequirements,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coxIiReadyToFitFormulaLeadPresent={coxIiReadyToFitFormulaLeadPresent}");
Console.WriteLine($"readyToFitFormulasProvideFixedParameterValues={readyToFitFormulasProvideFixedParameterValues}");
Console.WriteLine($"coxIiReadyToFitFormulaPromotableForBosonMasses={coxIiReadyToFitFormulaPromotableForBosonMasses}");
Console.WriteLine($"coxIiReadyToFitFormulaDependencyAuditPassed={coxIiReadyToFitFormulaDependencyAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record FitParameterRequirement(string ParameterId, bool Required, bool PromotableSourcePresent, string Detail);
sealed record Requirement(string RequirementId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
