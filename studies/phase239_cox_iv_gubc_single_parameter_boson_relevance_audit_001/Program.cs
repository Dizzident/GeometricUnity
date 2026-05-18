using System.Text.Json;

const string DefaultOutputDir = "studies/phase239_cox_iv_gubc_single_parameter_boson_relevance_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase233Path = "studies/phase233_external_cox_gu_papers_iii_iv_source_intake_audit_001/output/external_cox_gu_papers_iii_iv_source_intake_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase238Path = "studies/phase238_cox_ii_ready_to_fit_formula_dependency_audit_001/output/cox_ii_ready_to_fit_formula_dependency_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE239_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase233 = JsonDocument.Parse(File.ReadAllText(Phase233Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase238 = JsonDocument.Parse(File.ReadAllText(Phase238Path));

var phase224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var targetIndependentGuVevSourcePromotable = JsonBool(phase229.RootElement, "targetIndependentGuVevSourcePromotable") is true;
var externalCoxPapersIIIIVSourceIntakeAuditPassed = JsonBool(phase233.RootElement, "externalCoxPapersIIIIVSourceIntakeAuditPassed") is true;
var externalCoxPapersIIIIVPromotableForBosonMasses = JsonBool(phase233.RootElement, "externalCoxPapersIIIIVPromotableForBosonMasses") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var coxIiReadyToFitFormulaDependencyAuditPassed = JsonBool(phase238.RootElement, "coxIiReadyToFitFormulaDependencyAuditPassed") is true;
var coxIiReadyToFitFormulaPromotableForBosonMasses = JsonBool(phase238.RootElement, "coxIiReadyToFitFormulaPromotableForBosonMasses") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;

var coxIvGubcSingleParameterLeadPresent = true;
var coxIvBoundaryFamiliesLeadPresent = true;
var coxIvObservableMapLeadPresent = true;
var coxIvSingleParameterIsSigma0 = true;
var coxIvSingleParameterControlsBcHooks = true;
var coxIvSingleParameterControlsWzHMasses = false;
var coxIvProvidesElectroweakCouplingSources = false;
var coxIvProvidesElectroweakVevSource = false;
var coxIvProvidesHiggsScalarPotentialSource = false;
var coxIvProvidesObservedWzHMassRows = false;
var coxIvUsesTargetBosonMasses = false;

var singleParameterScope = new[]
{
    new ScopeRow("background-expansion", true, "Cox IV uses sigma0 in a background cosmology pack with H(z), distance, and BAO formulas."),
    new ScopeRow("vorticity-damping", true, "Cox IV lists a homogeneous vorticity-damping hook controlled by sigma0."),
    new ScopeRow("weak-field-log-surrogate", true, "Cox IV lists a weak-field logarithmic-potential surrogate controlled by sigma0 in a stated validity domain."),
    new ScopeRow("wz-electroweak-masses", false, "The single GUBC/BC parameter does not supply g_L, g_Y, kappa, or W/Z particle rows."),
    new ScopeRow("higgs-mass", false, "The single GUBC/BC parameter does not supply a Higgs scalar-source operator, potential, quartic, VEV, or mass row."),
};

var missingBosonPromotionRequirements = new[]
{
    new Requirement("electroweak-coupling-source-rows", coxIvProvidesElectroweakCouplingSources, "No Cox IV GUBC source row fixes g_L or g_Y at the W/Z comparison scale."),
    new Requirement("electroweak-vev-source-row", coxIvProvidesElectroweakVevSource, "No Cox IV GUBC source row fixes kappa or the electroweak VEV."),
    new Requirement("higgs-scalar-potential-source", coxIvProvidesHiggsScalarPotentialSource, "No Cox IV GUBC source supplies a Higgs potential, quartic, scalar profile, or excitation relation."),
    new Requirement("observed-wzh-mass-rows", coxIvProvidesObservedWzHMassRows, "No Cox IV GUBC artifact supplies W/Z/H particle mass rows with replay, stability, and target-comparison gates."),
    new Requirement("rg-threshold-transport-to-electroweak-scale", lowEnergyRgTransportSourcePromotable, "No promotable transport source maps high-scale/slice data to low-energy electroweak mass parameters."),
};

var coxIvGubcSingleParameterPromotableForBosonMasses =
    coxIvGubcSingleParameterLeadPresent
    && coxIvSingleParameterControlsWzHMasses
    && coxIvProvidesElectroweakCouplingSources
    && coxIvProvidesElectroweakVevSource
    && coxIvProvidesHiggsScalarPotentialSource
    && coxIvProvidesObservedWzHMassRows
    && missingBosonPromotionRequirements.All(row => row.Filled)
    && wAbsoluteMassParameterClosure
    && zAbsoluteMassParameterClosure
    && higgsMassParameterClosure
    && targetIndependentGuVevSourcePromotable
    && externalCoxPapersIIIIVPromotableForBosonMasses
    && lowEnergyRgTransportSourcePromotable
    && coxIiReadyToFitFormulaPromotableForBosonMasses;

var checks = new[]
{
    new Check("cox-iv-gubc-single-parameter-lead-present", coxIvGubcSingleParameterLeadPresent && coxIvBoundaryFamiliesLeadPresent && coxIvObservableMapLeadPresent && coxIvSingleParameterIsSigma0, $"gubcLead={coxIvGubcSingleParameterLeadPresent}; boundaryFamilies={coxIvBoundaryFamiliesLeadPresent}; observableMap={coxIvObservableMapLeadPresent}; sigma0={coxIvSingleParameterIsSigma0}"),
    new Check("single-parameter-scope-is-bc-observable-hooks-not-wzh-masses", coxIvSingleParameterControlsBcHooks && !coxIvSingleParameterControlsWzHMasses && singleParameterScope.Count(row => row.InScope) == 3, $"controlsBcHooks={coxIvSingleParameterControlsBcHooks}; controlsWzHMasses={coxIvSingleParameterControlsWzHMasses}; inScopeCount={singleParameterScope.Count(row => row.InScope)}"),
    new Check("cox-iv-does-not-fill-electroweak-or-higgs-source-rows", missingBosonPromotionRequirements.All(row => !row.Filled), $"missingRequirementCount={missingBosonPromotionRequirements.Count(row => !row.Filled)}"),
    new Check("phase233-preserves-iii-iv-nonpromotional-status", externalCoxPapersIIIIVSourceIntakeAuditPassed && !externalCoxPapersIIIIVPromotableForBosonMasses, $"externalCoxPapersIIIIVSourceIntakeAuditPassed={externalCoxPapersIIIIVSourceIntakeAuditPassed}; externalCoxPapersIIIIVPromotableForBosonMasses={externalCoxPapersIIIIVPromotableForBosonMasses}"),
    new Check("wz-higgs-parameter-closure-still-blocked", !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && !higgsMassParameterClosure && !targetIndependentGuVevSourcePromotable && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; vevSource={targetIndependentGuVevSourcePromotable}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("downstream-fit-formula-and-rg-routes-still-nonpromotable", coxIiReadyToFitFormulaDependencyAuditPassed && !coxIiReadyToFitFormulaPromotableForBosonMasses && !lowEnergyRgTransportSourcePromotable, $"phase238Passed={coxIiReadyToFitFormulaDependencyAuditPassed}; phase238Promotable={coxIiReadyToFitFormulaPromotableForBosonMasses}; phase236Promotable={lowEnergyRgTransportSourcePromotable}"),
    new Check("gubc-single-parameter-not-boson-mass-prediction", !coxIvGubcSingleParameterPromotableForBosonMasses && !coxIvUsesTargetBosonMasses, $"coxIvGubcSingleParameterPromotableForBosonMasses={coxIvGubcSingleParameterPromotableForBosonMasses}; targetBosonMassesUsed={coxIvUsesTargetBosonMasses}"),
};

var coxIvGubcSingleParameterBosonRelevanceAuditPassed = checks.All(check => check.Passed)
    && !coxIvGubcSingleParameterPromotableForBosonMasses;
var terminalStatus = coxIvGubcSingleParameterBosonRelevanceAuditPassed
    ? "cox-iv-gubc-single-parameter-bc-interface-not-boson-mass-source"
    : "cox-iv-gubc-single-parameter-boson-relevance-review-required";

var result = new
{
    phaseId = "phase239-cox-iv-gubc-single-parameter-boson-relevance-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    coxIvGubcSingleParameterLeadPresent,
    coxIvSingleParameterIsSigma0,
    coxIvSingleParameterControlsBcHooks,
    coxIvSingleParameterControlsWzHMasses,
    coxIvGubcSingleParameterPromotableForBosonMasses,
    coxIvGubcSingleParameterBosonRelevanceAuditPassed,
    objective = "Determine whether Cox GU IV's single-parameter GU-to-BC interface fixes the missing W/Z/H boson mass source-lineage parameters or only supplies boundary/cosmology observable hooks.",
    externalLead = new
    {
        title = "Geometric Unity IV: Boundary Dynamics, Observables, and the Single-Parameter GU->BC Interface",
        sourceUrl = "https://www.researchgate.net/publication/396557449_Geometric_Unity_IV_Boundary_Dynamics_Observables_and_the_Single-Parameter_GUBC_Interface_Admissible_Boundary_Families_Slice_EFT_Data_Maps_and_Global_Consistency_Tests",
        interpretation = "Cox IV supplies boundary families, slice EFT observable maps, and a single sigma0 parameter for BC/cosmology hooks. It does not derive the electroweak couplings, electroweak VEV, Higgs potential, or observed W/Z/H particle mass rows required by this repository.",
    },
    singleParameterScope,
    missingBosonPromotionRequirements,
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
        phase233 = new
        {
            status = JsonString(phase233.RootElement, "terminalStatus"),
            externalCoxPapersIIIIVSourceIntakeAuditPassed,
            externalCoxPapersIIIIVPromotableForBosonMasses,
        },
        phase236 = new
        {
            status = JsonString(phase236.RootElement, "terminalStatus"),
            lowEnergyRgTransportSourcePromotable,
        },
        phase238 = new
        {
            status = JsonString(phase238.RootElement, "terminalStatus"),
            coxIiReadyToFitFormulaDependencyAuditPassed,
            coxIiReadyToFitFormulaPromotableForBosonMasses,
        },
    },
    checks,
    decision = coxIvGubcSingleParameterBosonRelevanceAuditPassed
        ? "Do not promote the Cox IV single-parameter GUBC interface as a W/Z/H boson mass prediction. Its single parameter belongs to BC/cosmology observable hooks, while the electroweak coupling, VEV, Higgs scalar-source, mass-row, and RG/threshold source-lineage requirements remain unfilled."
        : "Review Cox IV GUBC single-parameter evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A source-lineage derivation showing sigma0 or another Cox IV boundary parameter fixes electroweak couplings at the W/Z comparison scale.",
        "A GU-derived electroweak VEV/kappa source row.",
        "A Higgs scalar-source/potential package with quartic or excitation relation.",
        "Observed W/Z/H prediction rows with replay, stability, and post-construction target-comparison gates.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase229Path = Phase229Path,
        phase233Path = Phase233Path,
        phase236Path = Phase236Path,
        phase238Path = Phase238Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "cox_iv_gubc_single_parameter_boson_relevance_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "cox_iv_gubc_single_parameter_boson_relevance_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.coxIvGubcSingleParameterLeadPresent,
        result.coxIvSingleParameterIsSigma0,
        result.coxIvSingleParameterControlsBcHooks,
        result.coxIvSingleParameterControlsWzHMasses,
        result.coxIvGubcSingleParameterPromotableForBosonMasses,
        result.coxIvGubcSingleParameterBosonRelevanceAuditPassed,
        result.externalLead,
        result.singleParameterScope,
        result.missingBosonPromotionRequirements,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coxIvGubcSingleParameterLeadPresent={coxIvGubcSingleParameterLeadPresent}");
Console.WriteLine($"coxIvSingleParameterControlsWzHMasses={coxIvSingleParameterControlsWzHMasses}");
Console.WriteLine($"coxIvGubcSingleParameterPromotableForBosonMasses={coxIvGubcSingleParameterPromotableForBosonMasses}");
Console.WriteLine($"coxIvGubcSingleParameterBosonRelevanceAuditPassed={coxIvGubcSingleParameterBosonRelevanceAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ScopeRow(string ScopeId, bool InScope, string Detail);
sealed record Requirement(string RequirementId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
