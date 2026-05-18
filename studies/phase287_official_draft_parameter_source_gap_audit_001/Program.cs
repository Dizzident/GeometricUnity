using System.Text.Json;

const string DefaultOutputDir = "studies/phase287_official_draft_parameter_source_gap_audit_001/output";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";
const string Phase226Path = "studies/phase226_official_gu_higgs_potential_notation_audit_001/output/official_gu_higgs_potential_notation_audit_summary.json";
const string Phase227Path = "studies/phase227_official_gu_shiab_upsilon_extraction_obstruction_audit_001/output/official_gu_shiab_upsilon_extraction_obstruction_audit_summary.json";
const string Phase228Path = "studies/phase228_boson_mass_matrix_extraction_obstruction_audit_001/output/boson_mass_matrix_extraction_obstruction_audit_summary.json";
const string Phase229Path = "studies/phase229_electroweak_vev_source_lineage_obstruction_audit_001/output/electroweak_vev_source_lineage_obstruction_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase286Path = "studies/phase286_alpha_running_threshold_source_viability_audit_001/output/alpha_running_threshold_source_viability_audit_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE287_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));
using var phase226 = JsonDocument.Parse(File.ReadAllText(Phase226Path));
using var phase227 = JsonDocument.Parse(File.ReadAllText(Phase227Path));
using var phase228 = JsonDocument.Parse(File.ReadAllText(Phase228Path));
using var phase229 = JsonDocument.Parse(File.ReadAllText(Phase229Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase286 = JsonDocument.Parse(File.ReadAllText(Phase286Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));

var officialGuParameterLocationLeadPresent = true;
var officialDraftProvidesAlphaSource = false;
var officialDraftProvidesChargeNormalizationSource = false;
var officialDraftProvidesChargedLeptonThresholdSource = false;
var officialDraftProvidesRgTransportSource = false;
var officialDraftProvidesHadronicSchemeClosure = false;
var officialDraftProvidesTargetIndependentVevSource = false;
var officialDraftProvidesHiggsScalarExtraction = false;
var officialDraftFillsPhase286Gaps = false;
var officialDraftPromotesWzMasses = false;
var officialDraftPromotesHiggsMass = false;
var sourceContractsFilled = false;
var newSourceEvidenceStillRequired = true;

var officialPublicSourceAuditMaterialized = JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized") is true;
var officialDraftProvidesCompletionSource = JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true;
var officialGuHiggsPotentialNotationObstructionCertified = JsonBool(phase226.RootElement, "officialGuHiggsPotentialNotationObstructionCertified") is true;
var officialGuShiabUpsilonExtractionObstructionCertified = JsonBool(phase227.RootElement, "officialGuShiabUpsilonExtractionObstructionCertified") is true;
var bosonMassMatrixExtractionObstructionCertified = JsonBool(phase228.RootElement, "bosonMassMatrixExtractionObstructionCertified") is true;
var electroweakVevSourceLineageObstructionCertified = JsonBool(phase229.RootElement, "electroweakVevSourceLineageObstructionCertified") is true;
var lowEnergyRgTransportSourceAuditPassed = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourceAuditPassed") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var phase245NewSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var leptonicRunningNumericallyClosesWz = JsonBool(phase286.RootElement, "leptonicRunningNumericallyClosesWz") is true;
var alphaRunningSourcePromotable = JsonBool(phase286.RootElement, "alphaRunningSourcePromotable") is true;
var alphaRunningThresholdRoutePromotesWzMasses = JsonBool(phase286.RootElement, "alphaRunningThresholdRoutePromotesWzMasses") is true;
var phase286SourceContractsFilled = JsonBool(phase286.RootElement, "sourceContractsFilled") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;

var publicSourcePointers = new[]
{
    new PublicSourcePointer(
        "official-gu-site-draft-release",
        "https://geometricunity.org/",
        "lines 25-28",
        "The official site identifies the April 1, 2021 public manuscript draft as the current draft source."),
    new PublicSourcePointer(
        "official-draft-standard-model-target-map",
        "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
        "page 3, lines 103-117 in the reviewed PDF text",
        "The draft lists the Standard Model Lagrangian ingredients as a target recovery map, including Yang-Mills, Higgs, and Yukawa terms."),
    new PublicSourcePointer(
        "official-draft-upsilon-bosonic-action",
        "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
        "page 44, lines 2361-2403 in the reviewed PDF text",
        "The draft gives a Upsilon-norm second-order bosonic action and a Yang-Mills-Maxwell-like equation."),
    new PublicSourcePointer(
        "official-draft-locations-within-gu",
        "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf",
        "appendix page 67, lines 4035-4080 in the reviewed PDF text",
        "The appendix assigns intended GU locations for the Higgs field, weak isospin, weak hypercharge, Higgs potential, cosmological constant as VEV, and Yukawa couplings as VEV."),
    new PublicSourcePointer(
        "portal-wiki-higgs-artificiality-summary",
        "https://theportal.wiki/wiki/Theory_of_Geometric_Unity",
        "lines 143-145",
        "The public summary frames the Higgs potential as an artificiality the program wants to explain, not as a completed parameter extraction."),
};

var checks = new[]
{
    new Check(
        "official-draft-parameter-location-leads-recorded",
        officialGuParameterLocationLeadPresent && publicSourcePointers.Length >= 4,
        $"officialGuParameterLocationLeadPresent={officialGuParameterLocationLeadPresent}; pointerCount={publicSourcePointers.Length}"),
    new Check(
        "official-public-source-audit-remains-noncompletion",
        officialPublicSourceAuditMaterialized && !officialDraftProvidesCompletionSource,
        $"officialPublicSourceAuditMaterialized={officialPublicSourceAuditMaterialized}; officialDraftProvidesCompletionSource={officialDraftProvidesCompletionSource}"),
    new Check(
        "phase286-numerical-closure-remains-external",
        leptonicRunningNumericallyClosesWz && !alphaRunningSourcePromotable && !alphaRunningThresholdRoutePromotesWzMasses && !phase286SourceContractsFilled,
        $"leptonicRunningNumericallyClosesWz={leptonicRunningNumericallyClosesWz}; alphaRunningSourcePromotable={alphaRunningSourcePromotable}; alphaRunningThresholdRoutePromotesWzMasses={alphaRunningThresholdRoutePromotesWzMasses}; phase286SourceContractsFilled={phase286SourceContractsFilled}"),
    new Check(
        "official-draft-does-not-fill-alpha-or-rg-transport-gaps",
        !officialDraftProvidesAlphaSource
            && !officialDraftProvidesChargeNormalizationSource
            && !officialDraftProvidesChargedLeptonThresholdSource
            && !officialDraftProvidesRgTransportSource
            && !officialDraftProvidesHadronicSchemeClosure
            && lowEnergyRgTransportSourceAuditPassed
            && !lowEnergyRgTransportSourcePromotable,
        $"officialDraftProvidesAlphaSource={officialDraftProvidesAlphaSource}; officialDraftProvidesChargeNormalizationSource={officialDraftProvidesChargeNormalizationSource}; officialDraftProvidesChargedLeptonThresholdSource={officialDraftProvidesChargedLeptonThresholdSource}; officialDraftProvidesRgTransportSource={officialDraftProvidesRgTransportSource}; officialDraftProvidesHadronicSchemeClosure={officialDraftProvidesHadronicSchemeClosure}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}"),
    new Check(
        "official-draft-does-not-fill-vev-or-higgs-extraction-gaps",
        !officialDraftProvidesTargetIndependentVevSource
            && !officialDraftProvidesHiggsScalarExtraction
            && officialGuHiggsPotentialNotationObstructionCertified
            && officialGuShiabUpsilonExtractionObstructionCertified
            && bosonMassMatrixExtractionObstructionCertified
            && electroweakVevSourceLineageObstructionCertified,
        $"officialDraftProvidesTargetIndependentVevSource={officialDraftProvidesTargetIndependentVevSource}; officialDraftProvidesHiggsScalarExtraction={officialDraftProvidesHiggsScalarExtraction}; higgsNotationObstructionCertified={officialGuHiggsPotentialNotationObstructionCertified}; shiabUpsilonObstructionCertified={officialGuShiabUpsilonExtractionObstructionCertified}; massMatrixObstructionCertified={bosonMassMatrixExtractionObstructionCertified}; vevObstructionCertified={electroweakVevSourceLineageObstructionCertified}"),
    new Check(
        "source-contracts-remain-unfilled",
        !unlockContractFilled
            && phase245NewSourceEvidenceStillRequired
            && !officialDraftFillsPhase286Gaps
            && !sourceContractsFilled
            && wzMissingFieldCount > 0
            && higgsMissingFieldCount > 0,
        $"unlockContractFilled={unlockContractFilled}; phase245NewSourceEvidenceStillRequired={phase245NewSourceEvidenceStillRequired}; officialDraftFillsPhase286Gaps={officialDraftFillsPhase286Gaps}; sourceContractsFilled={sourceContractsFilled}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}"),
    new Check(
        "official-draft-parameter-leads-do-not-promote-boson-masses",
        !officialDraftPromotesWzMasses && !officialDraftPromotesHiggsMass,
        $"officialDraftPromotesWzMasses={officialDraftPromotesWzMasses}; officialDraftPromotesHiggsMass={officialDraftPromotesHiggsMass}"),
};

var officialDraftParameterSourceGapAuditPassed = checks.All(check => check.Passed)
    && officialGuParameterLocationLeadPresent
    && !officialDraftFillsPhase286Gaps
    && !officialDraftPromotesWzMasses
    && !officialDraftPromotesHiggsMass
    && !sourceContractsFilled
    && newSourceEvidenceStillRequired;
var terminalStatus = officialDraftParameterSourceGapAuditPassed
    ? "official-draft-parameter-source-gap-audit-symbolic-locations-not-promotion"
    : "official-draft-parameter-source-gap-audit-review-required";

var result = new
{
    phaseId = "phase287-official-draft-parameter-source-gap-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    officialDraftParameterSourceGapAuditPassed,
    officialGuParameterLocationLeadPresent,
    officialDraftProvidesAlphaSource,
    officialDraftProvidesChargeNormalizationSource,
    officialDraftProvidesChargedLeptonThresholdSource,
    officialDraftProvidesRgTransportSource,
    officialDraftProvidesHadronicSchemeClosure,
    officialDraftProvidesTargetIndependentVevSource,
    officialDraftProvidesHiggsScalarExtraction,
    officialDraftFillsPhase286Gaps,
    officialDraftPromotesWzMasses,
    officialDraftPromotesHiggsMass,
    sourceContractsFilled,
    newSourceEvidenceStillRequired,
    publicSourcePointers,
    inheritedBlockerEvidence = new
    {
        phase218 = new
        {
            officialPublicSourceAuditMaterialized,
            officialDraftProvidesCompletionSource,
        },
        phase226 = new
        {
            officialGuHiggsPotentialNotationObstructionCertified,
        },
        phase227 = new
        {
            officialGuShiabUpsilonExtractionObstructionCertified,
        },
        phase228 = new
        {
            bosonMassMatrixExtractionObstructionCertified,
        },
        phase229 = new
        {
            electroweakVevSourceLineageObstructionCertified,
        },
        phase236 = new
        {
            lowEnergyRgTransportSourceAuditPassed,
            lowEnergyRgTransportSourcePromotable,
        },
        phase245 = new
        {
            unlockContractFilled,
            phase245NewSourceEvidenceStillRequired,
        },
        phase286 = new
        {
            leptonicRunningNumericallyClosesWz,
            alphaRunningSourcePromotable,
            alphaRunningThresholdRoutePromotesWzMasses,
            phase286SourceContractsFilled,
        },
        phase213 = new
        {
            wzMissingFieldCount,
            higgsMissingFieldCount,
        },
    },
    missingParameterSourceRequirements = new[]
    {
        new MissingRequirement("gu-alpha-or-charge-source", "A GU-derived electromagnetic charge or alpha value at a declared reference scale.", officialDraftProvidesAlphaSource || officialDraftProvidesChargeNormalizationSource),
        new MissingRequirement("gu-charged-threshold-sources", "GU-derived charged thresholds sufficient for low-energy alpha transport.", officialDraftProvidesChargedLeptonThresholdSource),
        new MissingRequirement("gu-rg-transport-and-scheme", "A GU low-energy running operator plus hadronic vacuum-polarization or renormalization-scheme closure.", officialDraftProvidesRgTransportSource && officialDraftProvidesHadronicSchemeClosure),
        new MissingRequirement("gu-vev-source", "A target-independent GU vacuum or VEV source replacing the external Phase54 Fermi scale.", officialDraftProvidesTargetIndependentVevSource),
        new MissingRequirement("gu-higgs-scalar-extraction", "A worked scalar-source/operator, identity envelope, massive profile, and potential/self-coupling extraction.", officialDraftProvidesHiggsScalarExtraction),
    },
    checks,
    decision = officialDraftParameterSourceGapAuditPassed
        ? "Do not promote W/Z/H masses from the official draft parameter-location passages. The draft supplies symbolic locations and a Upsilon/Higgs architecture, but not the alpha/charge source, charged-threshold source, RG/scheme transport, target-independent VEV, or Higgs scalar extraction needed to turn the Phase286 numerical closure into a GU prediction."
        : "Review the official-draft parameter source gap audit before relying on its non-promotional classification.",
    nextRequiredArtifact = new[]
    {
        "A GU-local alpha or electromagnetic charge source row at a declared reference scale.",
        "A GU-local low-energy RG transport theorem with charged thresholds and hadronic/scheme closure.",
        "A GU-local VEV or direct W/Z absolute-scale source row independent of the Phase54 Fermi input.",
        "A GU-local Higgs scalar-source/operator extraction and self-coupling or excitation relation.",
    },
    sourceEvidence = new
    {
        phase218Path = Phase218Path,
        phase226Path = Phase226Path,
        phase227Path = Phase227Path,
        phase228Path = Phase228Path,
        phase229Path = Phase229Path,
        phase236Path = Phase236Path,
        phase245Path = Phase245Path,
        phase286Path = Phase286Path,
        phase213Path = Phase213Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "official_draft_parameter_source_gap_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "official_draft_parameter_source_gap_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.officialDraftParameterSourceGapAuditPassed,
        result.officialGuParameterLocationLeadPresent,
        result.officialDraftProvidesAlphaSource,
        result.officialDraftProvidesChargeNormalizationSource,
        result.officialDraftProvidesChargedLeptonThresholdSource,
        result.officialDraftProvidesRgTransportSource,
        result.officialDraftProvidesHadronicSchemeClosure,
        result.officialDraftProvidesTargetIndependentVevSource,
        result.officialDraftProvidesHiggsScalarExtraction,
        result.officialDraftFillsPhase286Gaps,
        result.officialDraftPromotesWzMasses,
        result.officialDraftPromotesHiggsMass,
        result.sourceContractsFilled,
        result.newSourceEvidenceStillRequired,
        result.publicSourcePointers,
        result.inheritedBlockerEvidence,
        result.missingParameterSourceRequirements,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"officialDraftParameterSourceGapAuditPassed={officialDraftParameterSourceGapAuditPassed}");
Console.WriteLine($"officialGuParameterLocationLeadPresent={officialGuParameterLocationLeadPresent}");
Console.WriteLine($"officialDraftFillsPhase286Gaps={officialDraftFillsPhase286Gaps}");
Console.WriteLine($"officialDraftPromotesWzMasses={officialDraftPromotesWzMasses}");
Console.WriteLine($"officialDraftPromotesHiggsMass={officialDraftPromotesHiggsMass}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var property))
    {
        return null;
    }

    return property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
}

sealed record PublicSourcePointer(string SourceId, string Url, string Locator, string Finding);
sealed record MissingRequirement(string RequirementId, string Detail, bool Filled);
sealed record Check(string CheckId, bool Passed, string Detail);
