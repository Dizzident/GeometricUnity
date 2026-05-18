using System.Text.Json;

const string DefaultOutputDir = "studies/phase234_cox_ii_electroweak_formula_dependency_audit_001/output";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";
const string Phase214Path = "studies/phase214_external_electroweak_input_loophole_audit_001/output/external_electroweak_input_loophole_audit_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase232Path = "studies/phase232_external_cox_gu_paper_ii_source_intake_audit_001/output/external_cox_gu_paper_ii_source_intake_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE234_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));
using var phase214 = JsonDocument.Parse(File.ReadAllText(Phase214Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase232 = JsonDocument.Parse(File.ReadAllText(Phase232Path));

var canPromoteWzFromWeakCouplingMassRelation = JsonBool(phase197.RootElement, "canPromoteWzFromWeakCouplingMassRelation") is true;
var canPromoteExternalElectroweakBridge = JsonBool(phase214.RootElement, "canPromoteExternalElectroweakBridge") is true;
var closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(closure, "zAbsoluteMassParameterClosure") is true;
var targetIndependentGuVevSourcePromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var externalCoxPaperIISourceIntakeAuditPassed = JsonBool(phase232.RootElement, "externalCoxPaperIISourceIntakeAuditPassed") is true;
var externalCoxPaperIIPromotableForBosonMasses = JsonBool(phase232.RootElement, "externalCoxPaperIIPromotableForBosonMasses") is true;

var coxIiSymbolicElectroweakFormulaLeadPresent = true;
var formulaStructureMatchesStandardElectroweakTreeLevelMasses = true;
var formulasUseTargetObservables = false;
var requiredParameterCount = 3;
var promotableParameterSourceCount = 0;
var requiresWeakCouplingSource = true;
var requiresHyperchargeOrMixingSource = true;
var requiresVevOrKappaSource = true;
var weakCouplingSourcePromotable = false;
var hyperchargeOrMixingSourcePromotable = false;
var vevOrKappaSourcePromotable = targetIndependentGuVevSourcePromotable;
var symbolicFormulaLeadPromotableForAbsoluteMasses =
    coxIiSymbolicElectroweakFormulaLeadPresent
    && formulaStructureMatchesStandardElectroweakTreeLevelMasses
    && !requiresWeakCouplingSource
    && !requiresHyperchargeOrMixingSource
    && !requiresVevOrKappaSource
    && weakCouplingSourcePromotable
    && hyperchargeOrMixingSourcePromotable
    && vevOrKappaSourcePromotable
    && wAbsoluteMassParameterClosure
    && zAbsoluteMassParameterClosure
    && canPromoteWzFromWeakCouplingMassRelation
    && !canPromoteExternalElectroweakBridge
    && externalCoxPaperIIPromotableForBosonMasses;

var checks = new[]
{
    new Check("cox-ii-symbolic-electroweak-formula-lead-present", coxIiSymbolicElectroweakFormulaLeadPresent && formulaStructureMatchesStandardElectroweakTreeLevelMasses, $"formulaLead={coxIiSymbolicElectroweakFormulaLeadPresent}; standardTreeLevelStructure={formulaStructureMatchesStandardElectroweakTreeLevelMasses}; externalTargetsUsed={formulasUseTargetObservables}"),
    new Check("formula-dependencies-explicit", requiredParameterCount == 3 && requiresWeakCouplingSource && requiresHyperchargeOrMixingSource && requiresVevOrKappaSource, $"requiredParameterCount={requiredParameterCount}; needs_gL={requiresWeakCouplingSource}; needs_gY_or_mixing={requiresHyperchargeOrMixingSource}; needs_vev_or_kappa={requiresVevOrKappaSource}"),
    new Check("repo-parameter-sources-not-promotable", promotableParameterSourceCount == 0 && !weakCouplingSourcePromotable && !hyperchargeOrMixingSourcePromotable && !vevOrKappaSourcePromotable, $"promotableParameterSourceCount={promotableParameterSourceCount}; weakCouplingSourcePromotable={weakCouplingSourcePromotable}; hyperchargeOrMixingSourcePromotable={hyperchargeOrMixingSourcePromotable}; vevOrKappaSourcePromotable={vevOrKappaSourcePromotable}"),
    new Check("phase197-phase214-block-promotion", !canPromoteWzFromWeakCouplingMassRelation && !canPromoteExternalElectroweakBridge, $"canPromoteWzFromWeakCouplingMassRelation={canPromoteWzFromWeakCouplingMassRelation}; canPromoteExternalElectroweakBridge={canPromoteExternalElectroweakBridge}"),
    new Check("phase224-phase229-parameter-closure-blocked", !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && !targetIndependentGuVevSourcePromotable, $"wAbsoluteMassParameterClosure={wAbsoluteMassParameterClosure}; zAbsoluteMassParameterClosure={zAbsoluteMassParameterClosure}; targetIndependentGuVevSourcePromotable={targetIndependentGuVevSourcePromotable}"),
    new Check("phase232-cox-ii-nonpromotable", externalCoxPaperIISourceIntakeAuditPassed && !externalCoxPaperIIPromotableForBosonMasses, $"externalCoxPaperIISourceIntakeAuditPassed={externalCoxPaperIISourceIntakeAuditPassed}; externalCoxPaperIIPromotableForBosonMasses={externalCoxPaperIIPromotableForBosonMasses}"),
    new Check("symbolic-formula-not-absolute-prediction", !symbolicFormulaLeadPromotableForAbsoluteMasses, $"symbolicFormulaLeadPromotableForAbsoluteMasses={symbolicFormulaLeadPromotableForAbsoluteMasses}"),
};

var coxIiElectroweakFormulaDependencyAuditPassed = checks.All(check => check.Passed)
    && !symbolicFormulaLeadPromotableForAbsoluteMasses;
var terminalStatus = coxIiElectroweakFormulaDependencyAuditPassed
    ? "cox-ii-electroweak-formula-symbolic-dependency-blocked"
    : "cox-ii-electroweak-formula-dependency-review-required";

var result = new
{
    phaseId = "phase234-cox-ii-electroweak-formula-dependency-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    coxIiSymbolicElectroweakFormulaLeadPresent,
    formulaStructureMatchesStandardElectroweakTreeLevelMasses,
    symbolicFormulaLeadPromotableForAbsoluteMasses,
    coxIiElectroweakFormulaDependencyAuditPassed,
    objective = "Determine whether the Cox GU II electroweak mass-formula lead closes W/Z absolute-mass prediction or remains a symbolic dependency relation.",
    formulaLead = new
    {
        source = "Cox 2025 GU II and standard electroweak tree-level mass structure",
        wMassFormula = "m_W^2 = g_L^2 * kappa^2 / 4",
        zMassFormula = "m_Z^2 = (g_L^2 + g_Y^2) * kappa^2 / 4",
        hyperchargeMixingFormula = "1/g_Y^2 = 1/g_R^2 + 1/g_{B-L}^2 in the left-right/Pati-Salam normalization lead",
        formulasUseTargetObservables,
        interpretation = "This is a useful symbolic bridge/formula lead, but it is not an absolute prediction until g_L, g_Y or the equivalent mixing data, and kappa/VEV are independently derived from GU source lineage.",
    },
    requiredParameters = new[]
    {
        new ParameterRequirement("g_L", requiresWeakCouplingSource, weakCouplingSourcePromotable, "No promotable GU weak-coupling source is present."),
        new ParameterRequirement("g_Y-or-equivalent-mixing", requiresHyperchargeOrMixingSource, hyperchargeOrMixingSourcePromotable, "No promotable GU hypercharge/mixing source is present."),
        new ParameterRequirement("kappa-or-electroweak-VEV", requiresVevOrKappaSource, vevOrKappaSourcePromotable, "Current VEV bridge is external/Fermi-derived, not a GU VEV source."),
    },
    currentRepoEvidence = new
    {
        phase197 = new
        {
            status = JsonString(phase197.RootElement, "terminalStatus"),
            canPromoteWzFromWeakCouplingMassRelation,
            currentWeakCoupling = JsonDouble(phase197.RootElement, "currentWeakCoupling"),
            targetImpliedWeakCoupling = JsonDouble(phase197.RootElement, "targetImpliedWeakCoupling"),
        },
        phase214 = new
        {
            status = JsonString(phase214.RootElement, "terminalStatus"),
            canPromoteExternalElectroweakBridge,
        },
        phase224 = new
        {
            status = JsonString(phase224.RootElement, "terminalStatus"),
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
        },
        phase229 = new
        {
            status = JsonString(phase229.RootElement, "terminalStatus"),
            targetIndependentGuVevSourcePromotable,
            externalVevGeV = JsonDouble(phase229.RootElement, "externalVevGeV"),
        },
        phase232 = new
        {
            status = JsonString(phase232.RootElement, "terminalStatus"),
            externalCoxPaperIISourceIntakeAuditPassed,
            externalCoxPaperIIPromotableForBosonMasses,
        },
    },
    checks,
    decision = coxIiElectroweakFormulaDependencyAuditPassed
        ? "Do not promote W/Z absolute masses from the Cox II formula lead alone. It supplies a symbolic electroweak mass-structure lead, but the absolute masses remain blocked by missing GU-derived g_L, g_Y/mixing, and kappa/VEV source lineages."
        : "Review Cox II electroweak formula dependencies before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A GU source-lineage derivation of g_L.",
        "A GU source-lineage derivation of g_Y or the equivalent hypercharge/mixing relation with fixed normalizations.",
        "A GU source-lineage derivation of kappa/electroweak VEV.",
        "Repository W and Z rows with replay, stability, and target-comparison gates after the parameters are supplied.",
    },
    sourceEvidence = new
    {
        phase197Path = Phase197Path,
        phase214Path = Phase214Path,
        phase224Path = Phase224Path,
        phase229Path = Phase229Path,
        phase232Path = Phase232Path,
        coxGuIiDiscoveryUrl = "https://www.researchgate.net/publication/396557260_Geometric_Unity_II_Matter_Symmetry_on_the_Observation_Slice_One-Family_Factorization_Pati-Salam_Embedding_Anomaly_Closure_and_Embryo_HiggsYukawa_Textures",
        pdgStandardModelReviewUrl = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "cox_ii_electroweak_formula_dependency_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "cox_ii_electroweak_formula_dependency_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.coxIiSymbolicElectroweakFormulaLeadPresent,
        result.formulaStructureMatchesStandardElectroweakTreeLevelMasses,
        result.symbolicFormulaLeadPromotableForAbsoluteMasses,
        result.coxIiElectroweakFormulaDependencyAuditPassed,
        result.formulaLead,
        result.requiredParameters,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coxIiSymbolicElectroweakFormulaLeadPresent={coxIiSymbolicElectroweakFormulaLeadPresent}");
Console.WriteLine($"symbolicFormulaLeadPromotableForAbsoluteMasses={symbolicFormulaLeadPromotableForAbsoluteMasses}");
Console.WriteLine($"coxIiElectroweakFormulaDependencyAuditPassed={coxIiElectroweakFormulaDependencyAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record Check(string CheckId, bool Passed, string Detail);
sealed record ParameterRequirement(string ParameterId, bool Required, bool PromotableSourcePresent, string Detail);
