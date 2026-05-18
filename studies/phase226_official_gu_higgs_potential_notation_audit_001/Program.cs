using System.Text.Json;

const string DefaultOutputDir = "studies/phase226_official_gu_higgs_potential_notation_audit_001/output";
const string Phase187Path = "studies/phase187_higgs_scalar_source_identity_scaffold_001/output/higgs_scalar_source_identity_scaffold_summary.json";
const string Phase189Path = "studies/phase189_higgs_scalar_source_operator_census_001/output/higgs_scalar_source_operator_census_summary.json";
const string Phase196Path = "studies/phase196_higgs_potential_self_coupling_closure_audit_001/output/higgs_potential_self_coupling_closure_audit_summary.json";
const string Phase199Path = "studies/phase199_higgs_scalar_source_lineage_closure_audit_001/output/higgs_scalar_source_lineage_closure_audit_summary.json";
const string Phase207Path = "studies/phase207_higgs_quartic_self_coupling_source_scan_001/output/higgs_quartic_self_coupling_source_scan_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase215Path = "studies/phase215_higgs_target_implied_self_coupling_loophole_audit_001/output/higgs_target_implied_self_coupling_loophole_audit_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";
const string Phase223Path = "studies/phase223_higgs_casimir_quartic_numerical_probe_001/output/higgs_casimir_quartic_numerical_probe_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE226_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase187 = JsonDocument.Parse(File.ReadAllText(Phase187Path));
using var phase189 = JsonDocument.Parse(File.ReadAllText(Phase189Path));
using var phase196 = JsonDocument.Parse(File.ReadAllText(Phase196Path));
using var phase199 = JsonDocument.Parse(File.ReadAllText(Phase199Path));
using var phase207 = JsonDocument.Parse(File.ReadAllText(Phase207Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase215 = JsonDocument.Parse(File.ReadAllText(Phase215Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));
using var phase223 = JsonDocument.Parse(File.ReadAllText(Phase223Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));

var officialNotationPresent = true;
var officialSourceAuditMaterialized = JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized") is true;
var officialSourceAlreadyCompletion = JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true;
var scalarSourceIdentityValidated = JsonBool(phase187.RootElement, "identityEnvelopeValidated") is true;
var scalarSourceOperatorFound = JsonBool(phase189.RootElement, "solvedScalarSourceOperatorFound") is true;
var potentialOrSelfCouplingPromotable = JsonBool(phase196.RootElement, "canPromoteHiggsFromPotentialOrSelfCoupling") is true;
var scalarSourceLineagePromotable = JsonBool(phase199.RootElement, "canPromoteAnyHiggsScalarSourceLineage") is true;
var quarticSourcePromotable = JsonBool(phase207.RootElement, "canPromoteHiggsQuarticSelfCouplingSource") is true;
var targetImpliedQuarticPromotable = JsonBool(phase215.RootElement, "canPromoteTargetImpliedHiggsSelfCoupling") is true;
var casimirQuarticLeadPromotable = JsonBool(phase223.RootElement, "canPromoteHiggsCasimirQuarticLead") is true;
var higgsParameterClosure = phase224.RootElement.TryGetProperty("closure", out var phase224Closure)
    && JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? 0;
var higgsMissingFields = JsonStringArray(phase213.RootElement, "higgsMissingFields");

var notationPromotesAnyRequiredSource =
    scalarSourceIdentityValidated
    || scalarSourceOperatorFound
    || potentialOrSelfCouplingPromotable
    || scalarSourceLineagePromotable
    || quarticSourcePromotable
    || targetImpliedQuarticPromotable
    || casimirQuarticLeadPromotable
    || higgsParameterClosure
    || officialSourceAlreadyCompletion;

var officialGuHiggsPotentialNotationPromotable = officialNotationPresent && notationPromotesAnyRequiredSource;

var checks = new[]
{
    new Check("official-gu-higgs-potential-notation-recorded", officialNotationPresent, "The official GU draft context contains Higgs-potential notation involving Upsilon-omega style terms and broader Yang-Mills/Higgs recovery language."),
    new Check("official-public-source-audit-still-noncompletion", officialSourceAuditMaterialized && !officialSourceAlreadyCompletion, $"officialPublicSourceAuditMaterialized={officialSourceAuditMaterialized}; officialDraftProvidesCompletionSource={officialSourceAlreadyCompletion}"),
    new Check("higgs-scalar-source-identity-not-validated", !scalarSourceIdentityValidated, $"identityEnvelopeValidated={scalarSourceIdentityValidated}"),
    new Check("higgs-scalar-source-operator-not-solved", !scalarSourceOperatorFound, $"solvedScalarSourceOperatorFound={scalarSourceOperatorFound}"),
    new Check("higgs-potential-self-coupling-not-promotable", !potentialOrSelfCouplingPromotable, $"canPromoteHiggsFromPotentialOrSelfCoupling={potentialOrSelfCouplingPromotable}"),
    new Check("higgs-scalar-source-lineage-not-promotable", !scalarSourceLineagePromotable, $"canPromoteAnyHiggsScalarSourceLineage={scalarSourceLineagePromotable}"),
    new Check("higgs-quartic-source-scan-not-promotable", !quarticSourcePromotable, $"canPromoteHiggsQuarticSelfCouplingSource={quarticSourcePromotable}"),
    new Check("target-implied-higgs-quartic-still-closed", !targetImpliedQuarticPromotable, $"canPromoteTargetImpliedHiggsSelfCoupling={targetImpliedQuarticPromotable}"),
    new Check("higgs-casimir-quartic-lead-still-nonpromotional", !casimirQuarticLeadPromotable, $"canPromoteHiggsCasimirQuarticLead={casimirQuarticLeadPromotable}"),
    new Check("higgs-parameter-closure-still-blocked", !higgsParameterClosure, $"higgsMassParameterClosure={higgsParameterClosure}"),
    new Check("higgs-blocker-fields-remain", higgsMissingFieldCount > 0 && higgsMissingFields.Contains("scalarSourceOperatorId"), $"higgsMissingFieldCount={higgsMissingFieldCount}; includesScalarSourceOperatorId={higgsMissingFields.Contains("scalarSourceOperatorId")}"),
};

var officialGuHiggsPotentialNotationObstructionCertified = checks.All(check => check.Passed)
    && !officialGuHiggsPotentialNotationPromotable;
var terminalStatus = officialGuHiggsPotentialNotationObstructionCertified
    ? "official-gu-higgs-potential-notation-audit-suggestive-not-source-lineage"
    : "official-gu-higgs-potential-notation-audit-review-required";

var result = new
{
    phaseId = "phase226-official-gu-higgs-potential-notation-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    officialGuHiggsPotentialNotationPromotable,
    officialGuHiggsPotentialNotationObstructionCertified,
    objective = "Record whether official GU Higgs-potential notation can fill the current Higgs scalar-source and self-coupling contracts.",
    officialGuSourceContext = new
    {
        officialDraftUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
        officialSiteUrl = "https://geometricunity.org/",
        officialLectureTranscriptUrl = "https://geometricunity.org/2013-oxford-lecture/",
        notationSummary = "The official draft and transcript place Higgs/Yang-Mills structure inside the GU recovery map and use Higgs-potential notation involving Upsilon-omega style terms.",
        obstruction = "That notation is architectural and suggestive, but the current repo has no worked extraction theorem that converts it into a scalar source operator, a Higgs identity envelope, a massive scalar profile, or a target-independent quartic/self-coupling value.",
    },
    currentRepoBlockers = new
    {
        phase187 = new
        {
            status = JsonString(phase187.RootElement, "terminalStatus"),
            identityEnvelopeValidated = scalarSourceIdentityValidated,
        },
        phase189 = new
        {
            status = JsonString(phase189.RootElement, "terminalStatus"),
            solvedScalarSourceOperatorFound = scalarSourceOperatorFound,
            massiveScalarProfileFound = JsonBool(phase189.RootElement, "massiveScalarProfileFound"),
        },
        phase196 = new
        {
            status = JsonString(phase196.RootElement, "terminalStatus"),
            canPromoteHiggsFromPotentialOrSelfCoupling = potentialOrSelfCouplingPromotable,
        },
        phase199 = new
        {
            status = JsonString(phase199.RootElement, "terminalStatus"),
            canPromoteAnyHiggsScalarSourceLineage = scalarSourceLineagePromotable,
        },
        phase207 = new
        {
            status = JsonString(phase207.RootElement, "terminalStatus"),
            canPromoteHiggsQuarticSelfCouplingSource = quarticSourcePromotable,
            intakeReadyFindingCount = JsonInt(phase207.RootElement, "intakeReadyFindingCount"),
        },
        phase213 = new
        {
            status = JsonString(phase213.RootElement, "terminalStatus"),
            higgsMissingFieldCount,
            higgsMissingFields,
        },
        phase215 = new
        {
            status = JsonString(phase215.RootElement, "terminalStatus"),
            canPromoteTargetImpliedHiggsSelfCoupling = targetImpliedQuarticPromotable,
        },
        phase223 = new
        {
            status = JsonString(phase223.RootElement, "terminalStatus"),
            numericalLeadPresent = JsonBool(phase223.RootElement, "numericalLeadPresent"),
            canPromoteHiggsCasimirQuarticLead = casimirQuarticLeadPromotable,
        },
        phase224 = new
        {
            status = JsonString(phase224.RootElement, "terminalStatus"),
            higgsMassParameterClosure = higgsParameterClosure,
        },
    },
    checks,
    decision = officialGuHiggsPotentialNotationObstructionCertified
        ? "Do not promote the official GU Higgs-potential notation as a Higgs mass prediction source. It should be treated as a research pointer until a worked target-independent scalar-sector extraction fills the Phase201/Phase209/Phase210/Phase213 Higgs contracts."
        : "Review the official GU Higgs-potential notation audit before relying on this obstruction.",
    nextRequiredArtifact = new[]
    {
        "A worked GU extraction theorem that identifies the scalar source operator represented by the Higgs-potential notation.",
        "A target-independent derivation of the Higgs identity envelope, massive scalar profile, and scalar potential/self-coupling value.",
        "A particle-specific Higgs prediction row with raw/source gates, target-comparison gate, and stability sidecars filled without observed Higgs-mass back-solving.",
    },
    sourceEvidence = new
    {
        phase187Path = Phase187Path,
        phase189Path = Phase189Path,
        phase196Path = Phase196Path,
        phase199Path = Phase199Path,
        phase207Path = Phase207Path,
        phase213Path = Phase213Path,
        phase215Path = Phase215Path,
        phase218Path = Phase218Path,
        phase223Path = Phase223Path,
        phase224Path = Phase224Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "official_gu_higgs_potential_notation_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "official_gu_higgs_potential_notation_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.officialGuHiggsPotentialNotationPromotable,
        result.officialGuHiggsPotentialNotationObstructionCertified,
        result.officialGuSourceContext,
        result.currentRepoBlockers,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"officialGuHiggsPotentialNotationPromotable={officialGuHiggsPotentialNotationPromotable}");
Console.WriteLine($"officialGuHiggsPotentialNotationObstructionCertified={officialGuHiggsPotentialNotationObstructionCertified}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();

sealed record Check(string CheckId, bool Passed, string Detail);
