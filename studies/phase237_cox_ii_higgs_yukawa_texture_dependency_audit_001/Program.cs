using System.Text.Json;

const string DefaultOutputDir = "studies/phase237_cox_ii_higgs_yukawa_texture_dependency_audit_001/output";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase223Path = "studies/phase223_higgs_casimir_quartic_numerical_probe_001/output/higgs_casimir_quartic_numerical_probe_summary.json";
const string Phase226Path = "studies/phase226_official_gu_higgs_potential_notation_audit_001/output/official_gu_higgs_potential_notation_audit_summary.json";
const string Phase232Path = "studies/phase232_external_cox_gu_paper_ii_source_intake_audit_001/output/external_cox_gu_paper_ii_source_intake_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE237_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase223 = JsonDocument.Parse(File.ReadAllText(Phase223Path));
using var phase226 = JsonDocument.Parse(File.ReadAllText(Phase226Path));
using var phase232 = JsonDocument.Parse(File.ReadAllText(Phase232Path));

var phase232Claims = phase232.RootElement.GetProperty("claimedResearchLeads");
var coxIiGeometrySourcedScalarLeadPresent = JsonBool(phase232Claims, "paperIIGeometrySourcedScalarLeadPresent") is true;
var coxIiYukawaTextureLeadPresent = JsonBool(phase232Claims, "paperIIYukawaTextureLeadPresent") is true;
var coxIiHiggsLikeModuliLeadPresent = coxIiGeometrySourcedScalarLeadPresent;
var externalCoxPaperIISourceIntakeAuditPassed = JsonBool(phase232.RootElement, "externalCoxPaperIISourceIntakeAuditPassed") is true;
var externalCoxPaperIIPromotableForBosonMasses = JsonBool(phase232.RootElement, "externalCoxPaperIIPromotableForBosonMasses") is true;
var canPromoteHiggsFromPotentialOrSelfCoupling = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;
var canPromoteAnyHiggsScalarSourceLineage = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var canPromoteHiggsQuarticSelfCouplingSource = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var intakeReadyFindingCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var higgsCasimirNumericalLeadPresent = JsonBool(phase223.RootElement, "numericalLeadPresent") is true;
var higgsCasimirSourceLineagePromotable = JsonBool(phase223.RootElement, "sourceLineagePromotable") is true;
var officialGuHiggsPotentialNotationPromotable = JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationPromotable") is true;
var officialGuHiggsPotentialNotationObstructionCertified = JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationObstructionCertified") is true;

var scalarSourceOperatorPresent = false;
var higgsIdentityEnvelopePresent = false;
var massiveScalarProfilePresent = false;
var potentialOrSelfCouplingSourcePresent = false;
var quarticLambdaSourcePresent = false;
var higgsVevSourcePresent = false;
var stabilitySidecarsPresent = false;
var targetComparisonGatePresent = false;

var missingHiggsMassRequirements = new[]
{
    new Requirement("scalar-source-operator", scalarSourceOperatorPresent, "Cox II identifies Higgs-like moduli, but no solved local scalar-source operator is filled."),
    new Requirement("higgs-identity-envelope", higgsIdentityEnvelopePresent, "No target-independent identity envelope selects a physical observed Higgs row."),
    new Requirement("massive-scalar-profile", massiveScalarProfilePresent, "No physical massive scalar excitation profile is supplied."),
    new Requirement("potential-or-self-coupling-source", potentialOrSelfCouplingSourcePresent, "Schematic Yukawa textures do not provide a Higgs potential, self-coupling, or excitation relation."),
    new Requirement("quartic-lambda-source", quarticLambdaSourcePresent, "No GU-derived quartic lambda source is present."),
    new Requirement("higgs-vev-source", higgsVevSourcePresent, "No GU-derived Higgs VEV source is present."),
    new Requirement("stability-sidecars", stabilitySidecarsPresent, "No branch/refinement/environment/representation/coupling stability sidecars are supplied for a Higgs prediction row."),
    new Requirement("target-comparison-gate", targetComparisonGatePresent, "No post-construction target-comparison gate is available for a Higgs prediction."),
};

var coxIiHiggsYukawaTexturePromotableForHiggsMass =
    coxIiGeometrySourcedScalarLeadPresent
    && coxIiYukawaTextureLeadPresent
    && coxIiHiggsLikeModuliLeadPresent
    && missingHiggsMassRequirements.All(row => row.Filled)
    && canPromoteHiggsFromPotentialOrSelfCoupling
    && canPromoteAnyHiggsScalarSourceLineage
    && canPromoteHiggsQuarticSelfCouplingSource
    && higgsCasimirSourceLineagePromotable
    && officialGuHiggsPotentialNotationPromotable
    && externalCoxPaperIIPromotableForBosonMasses;

var checks = new[]
{
    new Check("cox-ii-higgs-yukawa-leads-present", coxIiGeometrySourcedScalarLeadPresent && coxIiYukawaTextureLeadPresent && coxIiHiggsLikeModuliLeadPresent, $"geometryScalarLead={coxIiGeometrySourcedScalarLeadPresent}; yukawaTextureLead={coxIiYukawaTextureLeadPresent}; higgsLikeModuliLead={coxIiHiggsLikeModuliLeadPresent}"),
    new Check("cox-ii-paper-ii-overall-nonpromotable", externalCoxPaperIISourceIntakeAuditPassed && !externalCoxPaperIIPromotableForBosonMasses, $"externalCoxPaperIISourceIntakeAuditPassed={externalCoxPaperIISourceIntakeAuditPassed}; externalCoxPaperIIPromotableForBosonMasses={externalCoxPaperIIPromotableForBosonMasses}"),
    new Check("higgs-source-lineage-remains-blocked", !canPromoteAnyHiggsScalarSourceLineage && higgsMissingFieldCount > 0, $"canPromoteAnyHiggsScalarSourceLineage={canPromoteAnyHiggsScalarSourceLineage}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("higgs-potential-self-coupling-not-closed", !canPromoteHiggsFromPotentialOrSelfCoupling && !canPromoteHiggsQuarticSelfCouplingSource && intakeReadyFindingCount == 0, $"canPromoteHiggsFromPotentialOrSelfCoupling={canPromoteHiggsFromPotentialOrSelfCoupling}; canPromoteHiggsQuarticSelfCouplingSource={canPromoteHiggsQuarticSelfCouplingSource}; intakeReadyFindingCount={intakeReadyFindingCount}"),
    new Check("higgs-casimir-quartic-lead-nonpromotional", higgsCasimirNumericalLeadPresent && !higgsCasimirSourceLineagePromotable, $"higgsCasimirNumericalLeadPresent={higgsCasimirNumericalLeadPresent}; sourceLineagePromotable={higgsCasimirSourceLineagePromotable}"),
    new Check("official-gu-higgs-notation-suggestive-not-source", officialGuHiggsPotentialNotationObstructionCertified && !officialGuHiggsPotentialNotationPromotable, $"officialGuHiggsPotentialNotationObstructionCertified={officialGuHiggsPotentialNotationObstructionCertified}; officialGuHiggsPotentialNotationPromotable={officialGuHiggsPotentialNotationPromotable}"),
    new Check("cox-ii-does-not-fill-higgs-mass-requirements", missingHiggsMassRequirements.All(row => !row.Filled), $"missingRequirementCount={missingHiggsMassRequirements.Count(row => !row.Filled)}"),
    new Check("cox-ii-higgs-yukawa-texture-not-higgs-mass-prediction", !coxIiHiggsYukawaTexturePromotableForHiggsMass, $"coxIiHiggsYukawaTexturePromotableForHiggsMass={coxIiHiggsYukawaTexturePromotableForHiggsMass}"),
};

var coxIiHiggsYukawaTextureDependencyAuditPassed = checks.All(check => check.Passed)
    && !coxIiHiggsYukawaTexturePromotableForHiggsMass;
var terminalStatus = coxIiHiggsYukawaTextureDependencyAuditPassed
    ? "cox-ii-higgs-yukawa-texture-lead-not-higgs-mass-source"
    : "cox-ii-higgs-yukawa-texture-dependency-review-required";

var result = new
{
    phaseId = "phase237-cox-ii-higgs-yukawa-texture-dependency-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    coxIiGeometrySourcedScalarLeadPresent,
    coxIiYukawaTextureLeadPresent,
    coxIiHiggsLikeModuliLeadPresent,
    coxIiHiggsYukawaTexturePromotableForHiggsMass,
    coxIiHiggsYukawaTextureDependencyAuditPassed,
    objective = "Determine whether the Cox GU II geometry-sourced scalar/Higgs-like moduli and schematic Yukawa texture lead closes the Higgs mass source-lineage contract.",
    externalLead = new
    {
        title = "Geometric Unity II: Matter & Symmetry on the Observation Slice One-Family Factorization, Pati-Salam Embedding, Anomaly Closure, and Embryo Higgs/Yukawa Textures",
        sourceUrl = "https://www.researchgate.net/publication/396557260_Geometric_Unity_II_Matter_Symmetry_on_the_Observation_Slice_One-Family_Factorization_Pati-Salam_Embedding_Anomaly_Closure_and_Embryo_HiggsYukawa_Textures",
        doiUrl = "https://doi.org/10.5281/zenodo.17373503",
        interpretation = "The lead is useful for scalar-sector model building because it identifies internal-geometry scalar modes and schematic Yukawa overlaps, but it does not by itself provide a Higgs scalar source operator, potential, quartic coupling, VEV, or physical mass row.",
    },
    standardModelDependencyContext = new
    {
        sourceUrl = "https://pdg.lbl.gov/2025/reviews/rpp2025-rev-standard-model.pdf",
        relationContext = "A physical Higgs mass prediction needs scalar-potential parameters and electroweak symmetry-breaking data; the existing repository gates require a GU source for the scalar operator, self-coupling or excitation relation, and VEV rather than a target-implied value.",
    },
    missingHiggsMassRequirements,
    currentRepoEvidence = new
    {
        phase196 = new
        {
            status = JsonString(phase196.RootElement, "terminalStatus"),
            canPromoteHiggsFromPotentialOrSelfCoupling,
        },
        phase199 = new
        {
            status = JsonString(phase199.RootElement, "terminalStatus"),
            canPromoteAnyHiggsScalarSourceLineage,
        },
        phase207 = new
        {
            status = JsonString(phase207.RootElement, "terminalStatus"),
            canPromoteHiggsQuarticSelfCouplingSource,
            intakeReadyFindingCount,
        },
        phase213 = new
        {
            status = JsonString(phase213.RootElement, "terminalStatus"),
            higgsMissingFieldCount,
        },
        phase223 = new
        {
            status = JsonString(phase223.RootElement, "terminalStatus"),
            numericalLeadPresent = higgsCasimirNumericalLeadPresent,
            sourceLineagePromotable = higgsCasimirSourceLineagePromotable,
        },
        phase226 = new
        {
            status = JsonString(phase226.RootElement, "terminalStatus"),
            officialGuHiggsPotentialNotationPromotable,
            officialGuHiggsPotentialNotationObstructionCertified,
        },
        phase232 = new
        {
            status = JsonString(phase232.RootElement, "terminalStatus"),
            externalCoxPaperIISourceIntakeAuditPassed,
            externalCoxPaperIIPromotableForBosonMasses,
        },
    },
    checks,
    decision = coxIiHiggsYukawaTextureDependencyAuditPassed
        ? "Do not promote the Cox II Higgs/Yukawa texture lead as a Higgs mass prediction. It preserves a geometry-sourced scalar/Yukawa research lead, but the Higgs source-lineage, potential/self-coupling, quartic, VEV, stability, and target-comparison requirements remain unfilled."
        : "Review Cox II Higgs/Yukawa dependency evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A solved GU scalar-source operator for the observed Higgs candidate.",
        "A target-independent Higgs identity envelope and massive scalar profile.",
        "A GU-derived potential/self-coupling source, quartic lambda, or equivalent excitation relation.",
        "A GU-derived Higgs VEV source and stability sidecars.",
        "A repository Higgs prediction row with replay and post-construction target-comparison gates.",
    },
    sourceEvidence = new
    {
        phase196Path = Phase196Path,
        phase199Path = Phase199Path,
        phase207Path = Phase207Path,
        phase213Path = Phase213Path,
        phase223Path = Phase223Path,
        phase226Path = Phase226Path,
        phase232Path = Phase232Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "cox_ii_higgs_yukawa_texture_dependency_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "cox_ii_higgs_yukawa_texture_dependency_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.coxIiGeometrySourcedScalarLeadPresent,
        result.coxIiYukawaTextureLeadPresent,
        result.coxIiHiggsLikeModuliLeadPresent,
        result.coxIiHiggsYukawaTexturePromotableForHiggsMass,
        result.coxIiHiggsYukawaTextureDependencyAuditPassed,
        result.externalLead,
        result.standardModelDependencyContext,
        result.missingHiggsMassRequirements,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coxIiGeometrySourcedScalarLeadPresent={coxIiGeometrySourcedScalarLeadPresent}");
Console.WriteLine($"coxIiYukawaTextureLeadPresent={coxIiYukawaTextureLeadPresent}");
Console.WriteLine($"coxIiHiggsYukawaTexturePromotableForHiggsMass={coxIiHiggsYukawaTexturePromotableForHiggsMass}");
Console.WriteLine($"coxIiHiggsYukawaTextureDependencyAuditPassed={coxIiHiggsYukawaTextureDependencyAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record Requirement(string RequirementId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
