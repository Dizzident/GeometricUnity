using System.Text.Json;

const string DefaultOutputDir = "studies/phase241_cox_iv_quartic_gauge_sign_falsifier_boson_mass_audit_001/output";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase223Path = "studies/phase223_higgs_casimir_quartic_numerical_probe_001/output/higgs_casimir_quartic_numerical_probe_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase233Path = "studies/phase233_external_cox_gu_papers_iii_iv_source_intake_audit_001/output/external_cox_gu_papers_iii_iv_source_intake_audit_summary.json";
const string Phase239Path = "studies/phase239_cox_iv_gubc_single_parameter_boson_relevance_audit_001/output/cox_iv_gubc_single_parameter_boson_relevance_audit_summary.json";
const string Phase240Path = "studies/phase240_cox_iii_axial_contact_rg_boson_parameter_audit_001/output/cox_iii_axial_contact_rg_boson_parameter_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE241_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase223 = JsonDocument.Parse(File.ReadAllText(Phase223Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase233 = JsonDocument.Parse(File.ReadAllText(Phase233Path));
using var phase239 = JsonDocument.Parse(File.ReadAllText(Phase239Path));
using var phase240 = JsonDocument.Parse(File.ReadAllText(Phase240Path));

var phase224Closure = phase224.RootElement.GetProperty("closure");
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var canPromoteHiggsQuarticSelfCouplingSource = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var higgsQuarticIntakeReadyFindingCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount") ?? 0;
var higgsCasimirSourceLineagePromotable = JsonBool(phase223.RootElement, "sourceLineagePromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? 0;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var phase233Claims = phase233.RootElement.GetProperty("claimedResearchLeads");
var paperIVGlobalFalsifierWorkflowLeadPresent = JsonBool(phase233Claims, "paperIVGlobalFalsifierWorkflowLeadPresent") is true;
var paperIVSliceEftObservableMapLeadPresent = JsonBool(phase233Claims, "paperIVSliceEftObservableMapLeadPresent") is true;
var externalCoxPapersIIIIVSourceIntakeAuditPassed = JsonBool(phase233.RootElement, "externalCoxPapersIIIIVSourceIntakeAuditPassed") is true;
var externalCoxPapersIIIIVPromotableForBosonMasses = JsonBool(phase233.RootElement, "externalCoxPapersIIIIVPromotableForBosonMasses") is true;
var coxIvGubcSingleParameterPromotableForBosonMasses = JsonBool(phase239.RootElement, "coxIvGubcSingleParameterPromotableForBosonMasses") is true;
var coxIiiAxialContactRgPromotableForBosonMasses = JsonBool(phase240.RootElement, "coxIiiAxialContactRgPromotableForBosonMasses") is true;

var coxIvQuarticGaugeSignFalsifierLeadPresent = true;
var anomalousQuarticGaugeCouplingSignConstraintPresent = true;
var forwardPositivityCoefficientLeadPresent = true;
var quarticGaugeSignFalsifierUsesTargetBosonMasses = false;
var quarticGaugeSignFalsifierProvidesHiggsQuarticLambda = false;
var quarticGaugeSignFalsifierProvidesElectroweakCouplings = false;
var quarticGaugeSignFalsifierProvidesElectroweakVev = false;
var quarticGaugeSignFalsifierProvidesObservedWzHMassRows = false;

var signFalsifierScope = new[]
{
    new ScopeRow("forward-positivity-sign", true, "Cox IV and the cited positivity literature use the sign of forward EFT coefficients as a consistency/falsifier condition."),
    new ScopeRow("anomalous-quartic-gauge-coupling-sign", true, "Negative anomalous quartic gauge-boson signs are treated as a falsifier/causality obstruction, not a mass source."),
    new ScopeRow("higgs-quartic-lambda", false, "An anomalous quartic gauge-coupling sign is not the Higgs scalar quartic lambda."),
    new ScopeRow("wz-absolute-masses", false, "The sign constraint does not fix W/Z mass values, electroweak couplings, or VEV."),
    new ScopeRow("higgs-mass", false, "The sign constraint does not fix a Higgs scalar-source operator, quartic, VEV, or physical mass row."),
};

var missingBosonMassRequirements = new[]
{
    new Requirement("higgs-quartic-lambda-source", quarticGaugeSignFalsifierProvidesHiggsQuarticLambda, "No Higgs scalar quartic/self-coupling source is supplied by anomalous quartic gauge-coupling sign constraints."),
    new Requirement("electroweak-coupling-source", quarticGaugeSignFalsifierProvidesElectroweakCouplings, "No g_L, g_Y, or weak-mixing source row is supplied."),
    new Requirement("electroweak-vev-source", quarticGaugeSignFalsifierProvidesElectroweakVev, "No kappa or electroweak VEV source row is supplied."),
    new Requirement("observed-wz-mass-rows", quarticGaugeSignFalsifierProvidesObservedWzHMassRows, "No W/Z mass rows with replay, stability, and target-comparison gates are supplied."),
    new Requirement("observed-higgs-mass-row", quarticGaugeSignFalsifierProvidesObservedWzHMassRows, "No Higgs mass row with scalar identity, stability, and target-comparison gates is supplied."),
};

var coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses =
    coxIvQuarticGaugeSignFalsifierLeadPresent
    && anomalousQuarticGaugeCouplingSignConstraintPresent
    && quarticGaugeSignFalsifierProvidesHiggsQuarticLambda
    && quarticGaugeSignFalsifierProvidesElectroweakCouplings
    && quarticGaugeSignFalsifierProvidesElectroweakVev
    && quarticGaugeSignFalsifierProvidesObservedWzHMassRows
    && missingBosonMassRequirements.All(row => row.Filled)
    && wAbsoluteMassParameterClosure
    && zAbsoluteMassParameterClosure
    && higgsMassParameterClosure
    && canPromoteHiggsQuarticSelfCouplingSource
    && higgsCasimirSourceLineagePromotable
    && externalCoxPapersIIIIVPromotableForBosonMasses
    && coxIvGubcSingleParameterPromotableForBosonMasses
    && coxIiiAxialContactRgPromotableForBosonMasses;

var checks = new[]
{
    new Check("cox-iv-quartic-gauge-sign-falsifier-lead-present", coxIvQuarticGaugeSignFalsifierLeadPresent && anomalousQuarticGaugeCouplingSignConstraintPresent && forwardPositivityCoefficientLeadPresent && paperIVGlobalFalsifierWorkflowLeadPresent && paperIVSliceEftObservableMapLeadPresent, $"leadPresent={coxIvQuarticGaugeSignFalsifierLeadPresent}; anomalousQuarticSign={anomalousQuarticGaugeCouplingSignConstraintPresent}; forwardPositivity={forwardPositivityCoefficientLeadPresent}; globalFalsifierWorkflow={paperIVGlobalFalsifierWorkflowLeadPresent}"),
    new Check("sign-falsifier-scope-not-mass-source", signFalsifierScope.Count(row => row.InScope) == 2 && signFalsifierScope.Any(row => row.ScopeId == "higgs-quartic-lambda" && !row.InScope) && signFalsifierScope.Any(row => row.ScopeId == "wz-absolute-masses" && !row.InScope), $"inScopeCount={signFalsifierScope.Count(row => row.InScope)}"),
    new Check("quartic-gauge-sign-does-not-fill-boson-mass-requirements", missingBosonMassRequirements.All(row => !row.Filled), $"missingRequirementCount={missingBosonMassRequirements.Count(row => !row.Filled)}"),
    new Check("higgs-quartic-source-still-blocked", !canPromoteHiggsQuarticSelfCouplingSource && higgsQuarticIntakeReadyFindingCount == 0 && !higgsCasimirSourceLineagePromotable, $"canPromoteHiggsQuarticSelfCouplingSource={canPromoteHiggsQuarticSelfCouplingSource}; higgsQuarticIntakeReadyFindingCount={higgsQuarticIntakeReadyFindingCount}; higgsCasimirSourceLineagePromotable={higgsCasimirSourceLineagePromotable}"),
    new Check("wz-higgs-parameter-closure-still-blocked", !wAbsoluteMassParameterClosure && !zAbsoluteMassParameterClosure && !higgsMassParameterClosure && wzMissingFieldCount > 0 && higgsMissingFieldCount > 0, $"wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check("cox-iii-iv-routes-still-nonpromotable", externalCoxPapersIIIIVSourceIntakeAuditPassed && !externalCoxPapersIIIIVPromotableForBosonMasses && !coxIvGubcSingleParameterPromotableForBosonMasses && !coxIiiAxialContactRgPromotableForBosonMasses, $"externalCoxPapersIIIIVPromotableForBosonMasses={externalCoxPapersIIIIVPromotableForBosonMasses}; phase239Promotable={coxIvGubcSingleParameterPromotableForBosonMasses}; phase240Promotable={coxIiiAxialContactRgPromotableForBosonMasses}"),
    new Check("quartic-gauge-sign-falsifier-not-boson-mass-prediction", !coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses && !quarticGaugeSignFalsifierUsesTargetBosonMasses, $"coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses={coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses}; targetBosonMassesUsed={quarticGaugeSignFalsifierUsesTargetBosonMasses}"),
};

var coxIvQuarticGaugeSignFalsifierBosonMassAuditPassed = checks.All(check => check.Passed)
    && !coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses;
var terminalStatus = coxIvQuarticGaugeSignFalsifierBosonMassAuditPassed
    ? "cox-iv-quartic-gauge-sign-falsifier-not-boson-mass-source"
    : "cox-iv-quartic-gauge-sign-falsifier-boson-mass-review-required";

var result = new
{
    phaseId = "phase241-cox-iv-quartic-gauge-sign-falsifier-boson-mass-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    coxIvQuarticGaugeSignFalsifierLeadPresent,
    anomalousQuarticGaugeCouplingSignConstraintPresent,
    forwardPositivityCoefficientLeadPresent,
    quarticGaugeSignFalsifierProvidesHiggsQuarticLambda,
    coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses,
    coxIvQuarticGaugeSignFalsifierBosonMassAuditPassed,
    objective = "Determine whether Cox GU IV's anomalous quartic gauge-boson sign/falsifier material supplies W/Z/H mass predictions or only positivity/falsifiability constraints.",
    externalLead = new
    {
        coxIvUrl = "https://www.researchgate.net/publication/396557449_Geometric_Unity_IV_Boundary_Dynamics_Observables_and_the_Single-Parameter_GUBC_Interface_Admissible_Boundary_Families_Slice_EFT_Data_Maps_and_Global_Consistency_Tests",
        positivityReferenceUrl = "https://arxiv.org/abs/hep-th/0602178",
        interpretation = "The quartic gauge-boson sign material is a forward-positivity/falsifier constraint for anomalous gauge couplings. It is not the Higgs quartic lambda and does not fix electroweak couplings, VEV, or W/Z/H mass rows.",
    },
    signFalsifierScope,
    missingBosonMassRequirements,
    currentRepoEvidence = new
    {
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
        phase223 = new
        {
            status = JsonString(phase223.RootElement, "terminalStatus"),
            higgsCasimirSourceLineagePromotable,
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
            paperIVGlobalFalsifierWorkflowLeadPresent,
        },
        phase239 = new
        {
            status = JsonString(phase239.RootElement, "terminalStatus"),
            coxIvGubcSingleParameterPromotableForBosonMasses,
        },
        phase240 = new
        {
            status = JsonString(phase240.RootElement, "terminalStatus"),
            coxIiiAxialContactRgPromotableForBosonMasses,
        },
    },
    checks,
    decision = coxIvQuarticGaugeSignFalsifierBosonMassAuditPassed
        ? "Do not promote Cox IV quartic gauge-boson sign/falsifier material as a W/Z/H mass prediction. It is a positivity/falsifiability constraint, while Higgs quartic lambda, electroweak coupling, VEV, observed mass-row, replay, stability, and target-comparison requirements remain unfilled."
        : "Review Cox IV quartic gauge-boson sign evidence before relying on this audit.",
    nextRequiredArtifact = new[]
    {
        "A GU-derived Higgs quartic lambda or scalar potential source, not an anomalous quartic gauge-coupling sign test.",
        "GU source rows for low-energy electroweak couplings and VEV.",
        "W/Z/H mass rows with replay, stability, and post-construction target-comparison gates.",
    },
    sourceEvidence = new
    {
        phase207Path = Phase207Path,
        phase213Path = Phase213Path,
        phase223Path = Phase223Path,
        phase224Path = Phase224Path,
        phase233Path = Phase233Path,
        phase239Path = Phase239Path,
        phase240Path = Phase240Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "cox_iv_quartic_gauge_sign_falsifier_boson_mass_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "cox_iv_quartic_gauge_sign_falsifier_boson_mass_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.coxIvQuarticGaugeSignFalsifierLeadPresent,
        result.anomalousQuarticGaugeCouplingSignConstraintPresent,
        result.forwardPositivityCoefficientLeadPresent,
        result.quarticGaugeSignFalsifierProvidesHiggsQuarticLambda,
        result.coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses,
        result.coxIvQuarticGaugeSignFalsifierBosonMassAuditPassed,
        result.externalLead,
        result.signFalsifierScope,
        result.missingBosonMassRequirements,
        result.currentRepoEvidence,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coxIvQuarticGaugeSignFalsifierLeadPresent={coxIvQuarticGaugeSignFalsifierLeadPresent}");
Console.WriteLine($"quarticGaugeSignFalsifierProvidesHiggsQuarticLambda={quarticGaugeSignFalsifierProvidesHiggsQuarticLambda}");
Console.WriteLine($"coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses={coxIvQuarticGaugeSignFalsifierPromotableForBosonMasses}");
Console.WriteLine($"coxIvQuarticGaugeSignFalsifierBosonMassAuditPassed={coxIvQuarticGaugeSignFalsifierBosonMassAuditPassed}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ScopeRow(string ScopeId, bool InScope, string Detail);
sealed record Requirement(string RequirementId, bool Filled, string Detail);
sealed record Check(string CheckId, bool Passed, string Detail);
