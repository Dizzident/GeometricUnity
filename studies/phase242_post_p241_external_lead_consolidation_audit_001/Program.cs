using System.Text.Json;

const string DefaultOutputDir = "studies/phase242_post_p241_external_lead_consolidation_audit_001/output";
const string Phase208Path = "studies/phase208_boson_local_route_exhaustion_certificate_001/output/boson_local_route_exhaustion_certificate_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";
const string Phase226Path = "studies/phase226_official_gu_higgs_potential_notation_audit_001/output/official_gu_higgs_potential_notation_audit_summary.json";
const string Phase227Path = "studies/phase227_official_gu_shiab_upsilon_extraction_obstruction_audit_001/output/official_gu_shiab_upsilon_extraction_obstruction_audit_summary.json";
const string Phase231Path = "studies/phase231_external_cox_gu_paper_i_source_intake_audit_001/output/external_cox_gu_paper_i_source_intake_audit_summary.json";
const string Phase232Path = "studies/phase232_external_cox_gu_paper_ii_source_intake_audit_001/output/external_cox_gu_paper_ii_source_intake_audit_summary.json";
const string Phase233Path = "studies/phase233_external_cox_gu_papers_iii_iv_source_intake_audit_001/output/external_cox_gu_papers_iii_iv_source_intake_audit_summary.json";
const string Phase234Path = "studies/phase234_cox_ii_electroweak_formula_dependency_audit_001/output/cox_ii_electroweak_formula_dependency_audit_summary.json";
const string Phase235Path = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output/pati_salam_weak_mixing_normalization_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase237Path = "studies/phase237_cox_ii_higgs_yukawa_texture_dependency_audit_001/output/cox_ii_higgs_yukawa_texture_dependency_audit_summary.json";
const string Phase238Path = "studies/phase238_cox_ii_ready_to_fit_formula_dependency_audit_001/output/cox_ii_ready_to_fit_formula_dependency_audit_summary.json";
const string Phase239Path = "studies/phase239_cox_iv_gubc_single_parameter_boson_relevance_audit_001/output/cox_iv_gubc_single_parameter_boson_relevance_audit_summary.json";
const string Phase240Path = "studies/phase240_cox_iii_axial_contact_rg_boson_parameter_audit_001/output/cox_iii_axial_contact_rg_boson_parameter_audit_summary.json";
const string Phase241Path = "studies/phase241_cox_iv_quartic_gauge_sign_falsifier_boson_mass_audit_001/output/cox_iv_quartic_gauge_sign_falsifier_boson_mass_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE242_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase208 = JsonDocument.Parse(File.ReadAllText(Phase208Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));
using var phase226 = JsonDocument.Parse(File.ReadAllText(Phase226Path));
using var phase227 = JsonDocument.Parse(File.ReadAllText(Phase227Path));
using var phase231 = JsonDocument.Parse(File.ReadAllText(Phase231Path));
using var phase232 = JsonDocument.Parse(File.ReadAllText(Phase232Path));
using var phase233 = JsonDocument.Parse(File.ReadAllText(Phase233Path));
using var phase234 = JsonDocument.Parse(File.ReadAllText(Phase234Path));
using var phase235 = JsonDocument.Parse(File.ReadAllText(Phase235Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase237 = JsonDocument.Parse(File.ReadAllText(Phase237Path));
using var phase238 = JsonDocument.Parse(File.ReadAllText(Phase238Path));
using var phase239 = JsonDocument.Parse(File.ReadAllText(Phase239Path));
using var phase240 = JsonDocument.Parse(File.ReadAllText(Phase240Path));
using var phase241 = JsonDocument.Parse(File.ReadAllText(Phase241Path));

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var localRouteExhaustionStillCertified =
    JsonBool(phase208.RootElement, "localRouteExhaustionCertified") is true
    && JsonBool(phase208.RootElement, "anyCurrentLocalRouteActionable") is false
    && JsonBool(phase213.RootElement, "localRouteExhaustionCertified") is true
    && JsonBool(phase213.RootElement, "existingEvidenceFound") is false
    && wzMissingFieldCount > 0
    && higgsMissingFieldCount > 0;

var officialGuActionLevelLeadPromotable =
    JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true
    || JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationPromotable") is true
    || JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionPromotable") is true;

var coxExternalLeadPromotable =
    JsonBool(phase231.RootElement, "externalCoxPaperIPromotableForBosonMasses") is true
    || JsonBool(phase232.RootElement, "externalCoxPaperIIPromotableForBosonMasses") is true
    || JsonBool(phase233.RootElement, "externalCoxPapersIIIIVPromotableForBosonMasses") is true
    || JsonBool(phase234.RootElement, "symbolicFormulaLeadPromotableForAbsoluteMasses") is true
    || JsonBool(phase235.RootElement, "patiSalamNormalizationPromotableForLowEnergyWz") is true
    || JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true
    || JsonBool(phase237.RootElement, "coxIiHiggsYukawaTexturePromotableForHiggsMass") is true
    || JsonBool(phase238.RootElement, "coxIiReadyToFitFormulaPromotableForBosonMasses") is true
    || JsonBool(phase239.RootElement, "coxIvGubcSingleParameterPromotableForBosonMasses") is true
    || JsonBool(phase240.RootElement, "coxIiiAxialContactRgPromotableForBosonMasses") is true
    || JsonBool(phase241.RootElement, "coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses") is true;

var anyExternalLeadPromotableForBosonMasses = officialGuActionLevelLeadPromotable || coxExternalLeadPromotable;
var newSourceLineageArtifactRequired = localRouteExhaustionStillCertified && !anyExternalLeadPromotableForBosonMasses;

var leadRows = new[]
{
    new LeadRow(
        "official-gu-public-draft-and-lecture",
        "official-gu",
        LeadPresent: JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized") is true,
        PromotableForBosonMasses: JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true,
        MissingContracts: new[]
        {
            "direct W/Z theoremOrDerivationId",
            "separate W/Z source rows with replay and target-comparison gates",
            "solved Higgs scalar-source operator and prediction row",
        },
        EvidencePath: Phase218Path,
        Detail: JsonString(phase218.RootElement, "conclusion") ?? "Official public source audit found no completion source."),
    new LeadRow(
        "official-gu-higgs-potential-notation",
        "official-gu",
        LeadPresent: JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationObstructionCertified") is true,
        PromotableForBosonMasses: JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationPromotable") is true,
        MissingContracts: new[]
        {
            "Higgs scalarSourceOperatorId",
            "Higgs quartic lambda or potential source lineage",
            "Higgs target-comparison and stability sidecars",
        },
        EvidencePath: Phase226Path,
        Detail: JsonString(phase226.RootElement, "decision") ?? "Notation audit preserves Higgs-source blockers."),
    new LeadRow(
        "official-gu-shiab-upsilon-extraction",
        "official-gu",
        LeadPresent: JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionObstructionCertified") is true,
        PromotableForBosonMasses: JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionPromotable") is true,
        MissingContracts: new[]
        {
            "fixed Shiab operator identity",
            "fixed kappa1 and inner-product normalization",
            "physical W/Z/H extraction theorem and mass rows",
        },
        EvidencePath: Phase227Path,
        Detail: JsonString(phase227.RootElement, "decision") ?? "Extraction audit keeps action-level material non-promotional."),
    new LeadRow(
        "cox-gu-i-shiab-invariant-geometry",
        "cox-gu",
        LeadPresent: JsonBool(phase231.RootElement, "externalCoxPaperIResearchLeadPresent") is true,
        PromotableForBosonMasses: JsonBool(phase231.RootElement, "externalCoxPaperIPromotableForBosonMasses") is true,
        MissingContracts: new[]
        {
            "W/Z absolute-mass bridge theorem",
            "electroweak coupling and VEV source rows",
            "Higgs scalar-source lineage",
        },
        EvidencePath: Phase231Path,
        Detail: JsonString(phase231.RootElement, "decision") ?? "Paper I is a Shiab/invariant-geometry lead, not a boson mass source."),
    new LeadRow(
        "cox-gu-ii-gauge-scalar-yukawa-and-formulas",
        "cox-gu",
        LeadPresent: JsonBool(phase232.RootElement, "externalCoxPaperIIResearchLeadPresent") is true
            && JsonBool(phase234.RootElement, "coxIiSymbolicElectroweakFormulaLeadPresent") is true
            && JsonBool(phase237.RootElement, "coxIiGeometrySourcedScalarLeadPresent") is true
            && JsonBool(phase238.RootElement, "coxIiReadyToFitFormulaLeadPresent") is true,
        PromotableForBosonMasses: JsonBool(phase232.RootElement, "externalCoxPaperIIPromotableForBosonMasses") is true
            || JsonBool(phase234.RootElement, "symbolicFormulaLeadPromotableForAbsoluteMasses") is true
            || JsonBool(phase237.RootElement, "coxIiHiggsYukawaTexturePromotableForHiggsMass") is true
            || JsonBool(phase238.RootElement, "coxIiReadyToFitFormulaPromotableForBosonMasses") is true,
        MissingContracts: new[]
        {
            "fixed g_L/g_Y or weak-mixing source",
            "fixed kappa/electroweak VEV source",
            "Higgs quartic or scalar-potential source",
            "pre-fit W/Z/H mass rows",
        },
        EvidencePath: Phase232Path,
        Detail: "Cox II leads are symbolic or fit-parameterized and do not supply fixed, target-independent W/Z/H mass rows."),
    new LeadRow(
        "cox-gu-iii-quantization-axial-contact-rg",
        "cox-gu",
        LeadPresent: JsonBool(phase233.RootElement, "externalCoxPapersIIIIVResearchLeadPresent") is true
            && JsonBool(phase240.RootElement, "coxIiiAxialContactRgLeadPresent") is true,
        PromotableForBosonMasses: JsonBool(phase233.RootElement, "externalCoxPapersIIIIVPromotableForBosonMasses") is true
            || JsonBool(phase240.RootElement, "coxIiiAxialContactRgPromotableForBosonMasses") is true,
        MissingContracts: new[]
        {
            "low-energy electroweak coupling running source",
            "Higgs quartic running or potential source",
            "GU-derived VEV and physical mass rows",
        },
        EvidencePath: Phase240Path,
        Detail: JsonString(phase240.RootElement, "decision") ?? "Cox III is quantization and BC-stiffness evidence, not a boson mass-parameter source."),
    new LeadRow(
        "cox-gu-iv-gubc-observables-quartic-sign",
        "cox-gu",
        LeadPresent: JsonBool(phase233.RootElement, "externalCoxPapersIIIIVResearchLeadPresent") is true
            && JsonBool(phase239.RootElement, "coxIvGubcSingleParameterLeadPresent") is true
            && JsonBool(phase241.RootElement, "coxIvQuarticGaugeSignFalsifierLeadPresent") is true,
        PromotableForBosonMasses: JsonBool(phase233.RootElement, "externalCoxPapersIIIIVPromotableForBosonMasses") is true
            || JsonBool(phase239.RootElement, "coxIvGubcSingleParameterPromotableForBosonMasses") is true
            || JsonBool(phase241.RootElement, "coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses") is true,
        MissingContracts: new[]
        {
            "electroweak coupling source rows",
            "electroweak VEV source row",
            "Higgs quartic lambda rather than quartic gauge-sign constraint",
            "W/Z/H prediction rows",
        },
        EvidencePath: Phase241Path,
        Detail: JsonString(phase241.RootElement, "decision") ?? "Cox IV observables and sign falsifiers do not fill W/Z/H mass-source contracts."),
};

var officialGuLeadCount = leadRows.Count(row => row.LeadFamily == "official-gu" && row.LeadPresent);
var coxLeadCount = leadRows.Count(row => row.LeadFamily == "cox-gu" && row.LeadPresent);
var nonPromotableLeadCount = leadRows.Count(row => row.LeadPresent && !row.PromotableForBosonMasses);
var promotableLeadCount = leadRows.Count(row => row.PromotableForBosonMasses);

var checks = new[]
{
    new Check("official-gu-action-leads-nonpromotable", !officialGuActionLevelLeadPromotable && officialGuLeadCount >= 3, $"officialGuActionLevelLeadPromotable={officialGuActionLevelLeadPromotable}; officialGuLeadCount={officialGuLeadCount}"),
    new Check("cox-external-leads-nonpromotable", !coxExternalLeadPromotable && coxLeadCount >= 4, $"coxExternalLeadPromotable={coxExternalLeadPromotable}; coxLeadCount={coxLeadCount}"),
    new Check("post-p241-specialized-audits-nonpromotable", JsonBool(phase237.RootElement, "coxIiHiggsYukawaTextureDependencyAuditPassed") is true && JsonBool(phase238.RootElement, "coxIiReadyToFitFormulaDependencyAuditPassed") is true && JsonBool(phase239.RootElement, "coxIvGubcSingleParameterBosonRelevanceAuditPassed") is true && JsonBool(phase240.RootElement, "coxIiiAxialContactRgBosonParameterAuditPassed") is true && JsonBool(phase241.RootElement, "coxIvQuarticGaugeSignFalsifierBosonMassAuditPassed") is true && !coxExternalLeadPromotable, "P237-P241 specialized audits all preserve non-promotional status."),
    new Check("blocker-matrix-still-open", wzMissingFieldCount == 15 && higgsMissingFieldCount == 14 && JsonBool(phase213.RootElement, "existingEvidenceFound") is false, $"wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; existingEvidenceFound={JsonBool(phase213.RootElement, "existingEvidenceFound")}"),
    new Check("local-route-exhaustion-still-certified", localRouteExhaustionStillCertified, $"localRouteExhaustionStillCertified={localRouteExhaustionStillCertified}"),
    new Check("no-external-lead-promotable", !anyExternalLeadPromotableForBosonMasses && promotableLeadCount == 0, $"anyExternalLeadPromotableForBosonMasses={anyExternalLeadPromotableForBosonMasses}; promotableLeadCount={promotableLeadCount}"),
    new Check("new-source-lineage-artifact-required", newSourceLineageArtifactRequired, $"newSourceLineageArtifactRequired={newSourceLineageArtifactRequired}"),
};

var postP241ExternalLeadConsolidationAuditPassed = checks.All(check => check.Passed)
    && newSourceLineageArtifactRequired
    && nonPromotableLeadCount == leadRows.Length;
var terminalStatus = postP241ExternalLeadConsolidationAuditPassed
    ? "post-p241-external-lead-consolidation-new-source-lineage-required"
    : "post-p241-external-lead-consolidation-review-required";

var result = new
{
    phaseId = "phase242-post-p241-external-lead-consolidation-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    postP241ExternalLeadConsolidationAuditPassed,
    anyExternalLeadPromotableForBosonMasses,
    officialGuActionLevelLeadPromotable,
    coxExternalLeadPromotable,
    localRouteExhaustionStillCertified,
    newSourceLineageArtifactRequired,
    officialGuLeadCount,
    coxLeadCount,
    nonPromotableLeadCount,
    promotableLeadCount,
    objective = "Consolidate official GU and Cox GU external/public lead audits through P241 and determine whether any lead now fills the W/Z/H boson mass source-lineage contracts.",
    leadRows,
    currentBlockers = new
    {
        phase213Status = JsonString(phase213.RootElement, "terminalStatus"),
        wzMissingFieldCount,
        higgsMissingFieldCount,
        nextRequiredArtifacts = phase213.RootElement.TryGetProperty("missingEvidenceMap", out var missingEvidenceMap)
            ? missingEvidenceMap.Clone()
            : default,
    },
    checks,
    decision = postP241ExternalLeadConsolidationAuditPassed
        ? "Do not promote any official/public or Cox external lead as W/Z/H absolute mass predictions. P213 source-lineage blockers remain open; a new derivation-backed W/Z source lineage and a solved Higgs scalar-source lineage are still required."
        : "Review external lead consolidation before relying on package boundaries.",
    nextRequiredArtifact = new[]
    {
        "A derivation-backed, target-independent W/Z absolute source lineage with separate W and Z source rows and all P209 gates true.",
        "A solved, target-independent Higgs scalar source/operator lineage with identity envelope, massive profile, coupling or excitation source, stability sidecars, and a passing prediction row.",
    },
    sourceEvidence = new
    {
        phase208Path = Phase208Path,
        phase213Path = Phase213Path,
        phase218Path = Phase218Path,
        phase226Path = Phase226Path,
        phase227Path = Phase227Path,
        phase231Path = Phase231Path,
        phase232Path = Phase232Path,
        phase233Path = Phase233Path,
        phase234Path = Phase234Path,
        phase235Path = Phase235Path,
        phase236Path = Phase236Path,
        phase237Path = Phase237Path,
        phase238Path = Phase238Path,
        phase239Path = Phase239Path,
        phase240Path = Phase240Path,
        phase241Path = Phase241Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "post_p241_external_lead_consolidation_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "post_p241_external_lead_consolidation_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.postP241ExternalLeadConsolidationAuditPassed,
        result.anyExternalLeadPromotableForBosonMasses,
        result.officialGuActionLevelLeadPromotable,
        result.coxExternalLeadPromotable,
        result.localRouteExhaustionStillCertified,
        result.newSourceLineageArtifactRequired,
        result.officialGuLeadCount,
        result.coxLeadCount,
        result.nonPromotableLeadCount,
        result.promotableLeadCount,
        result.leadRows,
        result.currentBlockers,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"postP241ExternalLeadConsolidationAuditPassed={postP241ExternalLeadConsolidationAuditPassed}");
Console.WriteLine($"anyExternalLeadPromotableForBosonMasses={anyExternalLeadPromotableForBosonMasses}");
Console.WriteLine($"newSourceLineageArtifactRequired={newSourceLineageArtifactRequired}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record LeadRow(string LeadId, string LeadFamily, bool LeadPresent, bool PromotableForBosonMasses, string[] MissingContracts, string EvidencePath, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
