using System.Text.Json;

const string DefaultOutputDir = "studies/phase240_cox_iii_axial_contact_rg_boson_parameter_audit_001/output";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase233Path = "studies/phase233_external_cox_gu_papers_iii_iv_source_intake_audit_001/output/external_cox_gu_papers_iii_iv_source_intake_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase239Path = "studies/phase239_cox_iv_gubc_single_parameter_boson_relevance_audit_001/output/cox_iv_gubc_single_parameter_boson_relevance_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE240_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase233 = JsonDocument.Parse(File.ReadAllText(Phase233Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase239 = JsonDocument.Parse(File.ReadAllText(Phase239Path));

var phase224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var canPromoteHiggsFromPotentialOrSelfCoupling = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;
var canPromoteHiggsQuarticSelfCouplingSource = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var higgsQuarticIntakeReadyFindingCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount") ?? 0;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var phase233Claims = phase233.RootElement.GetProperty("claimedResearchLeads");
var paperIIIBrstQuantizationLeadPresent = JsonBool(phase233Claims, "paperIIIBrstQuantizationLeadPresent") is true;
var paperIIICohomologyAndAnomalyLeadPresent = JsonBool(phase233Claims, "paperIIICohomologyAndAnomalyLeadPresent") is true;
var paperIIIRunningSignLeadPresent = JsonBool(phase233Claims, "paperIIIRunningSignLeadPresent") is true;
var externalCoxPapersIIIIVSourceIntakeAuditPassed = JsonBool(phase233.RootElement, "externalCoxPapersIIIIVSourceIntakeAuditPassed") is true;
var externalCoxPapersIIIIVPromotableForBosonMasses = JsonBool(phase233.RootElement, "externalCoxPapersIIIIVPromotableForBosonMasses") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var coxIvGubcSingleParameterPromotableForBosonMasses = JsonBool(phase239.RootElement, "coxIvGubcSingleParameterPromotableForBosonMasses") is true;

var coxIiiAxialContactRgLeadPresent = true;
var coxIiiSignCorridorLeadPresent = paperIIIRunningSignLeadPresent;
var coxIiiBrstQuantizationLeadPresent = paperIIIBrstQuantizationLeadPresent;
var coxIiiAnomalyClosureLeadPresent = paperIIICohomologyAndAnomalyLeadPresent;
var coxIiiMapsToBcStiffnessSigma0 = true;
var coxIiiControlsElectroweakCouplingRunning = false;
var coxIiiControlsHiggsQuarticRunning = false;
var coxIiiProvidesElectroweakVevSource = false;
var coxIiiProvidesObservedWzHMassRows = false;
var coxIiiUsesTargetBosonMasses = false;

var rgScope = new[]
{
    new ScopeRow("semidirect-brst-bv-consistency", true, "Paper III supplies BRST/BV and cohomology control for the semidirect gauge algebra."),
    new ScopeRow("axial-contact-running-sign", true, "Paper III supplies one-loop running and sign stability for the torsion-eliminated axial contact."),
    new ScopeRow("bc-stiffness-sigma0", true, "Paper III maps GU parameters to a BC stiffness parameter sigma0."),
    new ScopeRow("electroweak-gauge-coupling-running", false, "The axial-contact RG is not a source row for low-energy g_L or g_Y."),
    new ScopeRow("higgs-quartic-running", false, "The axial-contact sign corridor is not a GU-derived Higgs quartic or potential source."),
    new ScopeRow("wzh-physical-mass-rows", false, "Paper III does not supply observed W/Z/H mass rows with replay and comparison gates."),
};

var missingBosonParameterRequirements = new[]
{
    new Requirement("low-energy-electroweak-coupling-rg-source", coxIiiControlsElectroweakCouplingRunning, "No Cox III source row provides beta functions or fixed values for low-energy g_L/g_Y."),
    new Requirement("electroweak-vev-source", coxIiiProvidesElectroweakVevSource, "No Cox III source row provides kappa or the electroweak VEV."),
    new Requirement("higgs-quartic-or-potential-source", coxIiiControlsHiggsQuarticRunning, "The axial-contact sign corridor is not a Higgs quartic, scalar potential, or excitation relation."),
    new Requirement("observed-wzh-mass-rows", coxIiiProvidesObservedWzHMassRows, "No W/Z/H particle-specific prediction rows are supplied."),
    new Requirement("rg-threshold-transport-to-electroweak-scale", lowEnergyRgTransportSourcePromotable, "The repository still lacks low-energy RG/threshold transport for electroweak comparison parameters."),
};

var coxIiiAxialContactRgPromotableForBosonMasses =
    coxIiiAxialContactRgLeadPresent
    && coxIiiSignCorridorLeadPresent
    && coxIiiControlsElectroweakCouplingRunning
    && coxIiiControlsHiggsQuarticRunning
    && coxIiiProvidesElectroweakVevSource
    && coxIiiProvidesObservedWzHMassRows
    && missingBosonParameterRequirements.All(row => row.Filled)
    && wAbsoluteMassParameterClosure
    && zAbsoluteMassParameterClosure
    && higgsMassParameterClosure
    && canPromoteHiggsFromPotentialOrSelfCoupling
    && canPromoteHiggsQuarticSelfCouplingSource
    && externalCoxPapersIIIIVPromotableForBosonMasses
    && lowEnergyRgTransportSourcePromotable
    && coxIvGubcSingleParameterPromotableForBosonMasses;

var checks = new[]
{
    new Check("cox-iii-quantum-rg-leads-present", coxIiiAxialContactRgLeadPresent && coxIiiSignCorridorLeadPresent && coxIiiBrstQuantizationLeadPresent && coxIiiAnomalyClosureLeadPresent, $"axialContactRg={coxIiiAxialContactRgLeadPresent}; signCorridor={coxIiiSignCorridorLeadPresent}; brst={coxIiiBrstQuantizationLeadPresent}; anomalyClosure={coxIiiAnomalyClosureLeadPresent}"),
    new Check("rg-scope-is-axial-contact-bc-not-wzh-parameters", coxIiiMapsToBcStiffnessSigma0 && !coxIiiControlsElectroweakCouplingRunning && !coxIiiControlsHiggsQuarticRunning && rgScope.Count(row => row.InScope) == 3, $"mapsToSigma0={coxIiiMapsToBcStiffnessSigma0}; controlsElectroweakCouplings={coxIiiControlsElectroweakCouplingRunning}; controlsHiggsQuartic={coxIiiControlsHiggsQuarticRunning}; inScopeCount={rgScope.Count(row => row.InScope)}"),
    new Check("cox-iii-does-not-fill-boson-parameter-requirements", missingBosonParameterRequirements.All(row => !row.Filled), $"missingRequirementCount={missingBosonParameterRequirements.Count(row => !row.Filled)}"),
    new Check("phase233-preserves-paper-iii-nonpromotional-status", externalCoxPapersIIIIVSourceIntakeAuditPassed && !externalCoxPapersIIIIVPromotableForBosonMasses, $"externalCoxPapersIIIIVSourceIntakeAuditPassed={externalCoxPapersIIIIVSourceIntakeAuditPassed}; externalCoxPapersIIIIVPromotableForBosonMasses={externalCoxPapersIIIIVPromotableForBosonMasses}"),
    new Check("wz-higgs-parameter-closure-still-blocked", !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && !higgsMassParameterClosure && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("higgs-quartic-and-potential-source-still-blocked", !canPromoteHiggsFromPotentialOrSelfCoupling && !canPromoteHiggsQuarticSelfCouplingSource && higgsQuarticIntakeReadyFindingCount == 0, $"canPromotePotentialOrSelfCoupling={canPromoteHiggsFromPotentialOrSelfCoupling}; canPromoteQuartic={canPromoteHiggsQuarticSelfCouplingSource}; higgsQuarticIntakeReadyFindingCount={higgsQuarticIntakeReadyFindingCount}"),
    new Check("low-energy-rg-and-gubc-routes-still-nonpromotable", !lowEnergyRgTransportSourcePromotable && !coxIvGubcSingleParameterPromotableForBosonMasses, $"lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}; coxIvGubcSingleParameterPromotableForBosonMasses={coxIvGubcSingleParameterPromotableForBosonMasses}"),
    new Check("axial-contact-rg-not-boson-mass-prediction", !coxIiiAxialContactRgPromotableForBosonMasses && !coxIiiUsesTargetBosonMasses, $"coxIiiAxialContactRgPromotableForBosonMasses={coxIiiAxialContactRgPromotableForBosonMasses}; targetBosonMassesUsed={coxIiiUsesTargetBosonMasses}"),
};

var coxIiiAxialContactRgBosonParameterAuditPassed = checks.All(check => check.Passed)
    && !coxIiiAxialContactRgPromotableForBosonMasses;
var terminalStatus = coxIiiAxialContactRgBosonParameterAuditPassed
    ? "cox-iii-axial-contact-rg-sign-corridor-not-boson-parameter-source"
    : "cox-iii-axial-contact-rg-boson-parameter-review-required";

var result = new
{
    phaseId = "phase240-cox-iii-axial-contact-rg-boson-parameter-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    coxIiiAxialContactRgLeadPresent,
    coxIiiSignCorridorLeadPresent,
    coxIiiBrstQuantizationLeadPresent,
    coxIiiMapsToBcStiffnessSigma0,
    coxIiiControlsElectroweakCouplingRunning,
    coxIiiControlsHiggsQuarticRunning,
    coxIiiAxialContactRgPromotableForBosonMasses,
    coxIiiAxialContactRgBosonParameterAuditPassed,
    objective = "Determine whether Cox GU III's axial-contact RG/sign-corridor layer supplies the missing W/Z/H boson mass source parameters or only quantization/BC-stiffness constraints.",
    externalLead = new
    {
        title = "Geometric Unity III: Quantization, BRST, and Deformation Complex",
        sourceUrl = "https://www.researchgate.net/publication/396557263_Geometric_Unity_III_Quantization_BRST_and_Deformation_Complex",
        interpretation = "Cox III is useful for BRST/BV consistency, anomaly closure, admissible counterterms, axial-contact running, positivity, and the GU-to-BC stiffness bridge. It does not derive low-energy electroweak couplings, an electroweak VEV, a Higgs quartic/potential, or W/Z/H mass rows.",
    },
    rgScope,
    missingBosonParameterRequirements,
    currentRepoEvidence = new
    {
        phase196 = new
        {
            status = JsonString(phase196.RootElement, "terminalStatus"),
            canPromoteHiggsFromPotentialOrSelfCoupling,
        },
        phase207 = new
        {
            status = JsonString(phase207.RootElement, "terminalStatus"),
            canPromoteHiggsQuarticSelfCouplingSource,
            higgsQuarticIntakeReadyFindingCount,
        },
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
        phase233 = new
        {
            status = JsonString(phase233.RootElement, "terminalStatus"),
            externalCoxPapersIIIIVSourceIntakeAuditPassed,
            externalCoxPapersIIIIVPromotableForBosonMasses,
            paperIIIRunningSignLeadPresent,
        },
        phase236 = new
        {
            status = JsonString(phase236.RootElement, "terminalStatus"),
            lowEnergyRgTransportSourcePromotable,
        },
        phase239 = new
        {
            status = JsonString(phase239.RootElement, "terminalStatus"),
            coxIvGubcSingleParameterPromotableForBosonMasses,
        },
    },
    checks,
    decision = coxIiiAxialContactRgBosonParameterAuditPassed
        ? "Do not promote Cox III axial-contact RG/sign-corridor material as a W/Z/H boson mass prediction. It constrains quantization and the BC stiffness bridge, while the electroweak coupling, VEV, Higgs quartic/potential, low-energy transport, and observed mass-row requirements remain unfilled."
        : "Review Cox III axial-contact RG evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A Cox III or successor source row deriving low-energy g_L and g_Y, not only axial-contact running.",
        "A GU-derived electroweak VEV/kappa source row.",
        "A Higgs scalar potential or quartic source tied to the observed Higgs identity envelope.",
        "W/Z/H prediction rows with replay, stability, and post-construction target-comparison gates.",
    },
    sourceEvidence = new
    {
        phase196Path = Phase196Path,
        phase207Path = Phase207Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase233Path = Phase233Path,
        phase236Path = Phase236Path,
        phase239Path = Phase239Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "cox_iii_axial_contact_rg_boson_parameter_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "cox_iii_axial_contact_rg_boson_parameter_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.coxIiiAxialContactRgLeadPresent,
        result.coxIiiSignCorridorLeadPresent,
        result.coxIiiBrstQuantizationLeadPresent,
        result.coxIiiMapsToBcStiffnessSigma0,
        result.coxIiiControlsElectroweakCouplingRunning,
        result.coxIiiControlsHiggsQuarticRunning,
        result.coxIiiAxialContactRgPromotableForBosonMasses,
        result.coxIiiAxialContactRgBosonParameterAuditPassed,
        result.externalLead,
        result.rgScope,
        result.missingBosonParameterRequirements,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coxIiiAxialContactRgLeadPresent={coxIiiAxialContactRgLeadPresent}");
Console.WriteLine($"coxIiiControlsElectroweakCouplingRunning={coxIiiControlsElectroweakCouplingRunning}");
Console.WriteLine($"coxIiiControlsHiggsQuarticRunning={coxIiiControlsHiggsQuarticRunning}");
Console.WriteLine($"coxIiiAxialContactRgPromotableForBosonMasses={coxIiiAxialContactRgPromotableForBosonMasses}");
Console.WriteLine($"coxIiiAxialContactRgBosonParameterAuditPassed={coxIiiAxialContactRgBosonParameterAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ScopeRow(string ScopeId, bool InScope, string Detail);
sealed record Requirement(string RequirementId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
