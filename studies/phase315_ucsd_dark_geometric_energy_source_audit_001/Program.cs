using System.Text.Json;

const string DefaultOutputDir = "studies/phase315_ucsd_dark_geometric_energy_source_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase218Path = "studies/phase218_official_gu_public_source_audit_001/output/official_gu_public_source_audit_summary.json";
const string Phase235Path = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output/pati_salam_weak_mixing_normalization_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase312Path = "studies/phase312_current_public_gu_rvg_revision_delta_audit_001/output/current_public_gu_rvg_revision_delta_audit_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase314Path = "studies/phase314_dimension_casimir_wz_source_law_audit_001/output/dimension_casimir_wz_source_law_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE315_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase218 = JsonDocument.Parse(File.ReadAllText(Phase218Path));
using var phase235 = JsonDocument.Parse(File.ReadAllText(Phase235Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase312 = JsonDocument.Parse(File.ReadAllText(Phase312Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase314 = JsonDocument.Parse(File.ReadAllText(Phase314Path));

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var officialPublicSourceAuditMaterialized = JsonBool(phase218.RootElement, "officialPublicSourceAuditMaterialized") is true;
var officialDraftProvidesCompletionSource = JsonBool(phase218.RootElement, "officialDraftProvidesCompletionSource") is true;
var patiSalamNormalizationPromotableForLowEnergyWz = JsonBool(phase235.RootElement, "patiSalamNormalizationPromotableForLowEnergyWz") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var currentPublicGuRvgRevisionDeltaAuditPassed = JsonBool(phase312.RootElement, "currentPublicGuRvgRevisionDeltaAuditPassed") is true;
var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var dimensionCasimirWzSourceLawAuditPassed = JsonBool(phase314.RootElement, "dimensionCasimirWzSourceLawAuditPassed") is true;

var publicSources = new[]
{
    new PublicSourceRow(
        "portal-group-dark-to-geometric-energy",
        "From Dark to Geometric Energy - A Sector of Geometric Unity",
        "https://theportal.group/from-dark-to-geometric-energy-a-sector-of-geometric-unity/",
        "2025-05-22",
        "Portal Group page for an April 2025 UCSD Astrophysics and Cosmology Seminar lecture by Eric Weinstein. The abstract describes GU dark-energy geometry, theta_omega, an inhomogeneous gauge group over the Dirac spinor bundle of a 14-dimensional Lorentzian-metric space, supersymmetric Einstein-Dirac structure, three Pati-Salam generations under compactification, and Seiberg-Witten monopole alignment.",
        IsPrimaryOrOfficialPortalSource: true,
        TranscriptAvailable: false,
        ProvidesWzSourceLineageFields: false,
        ProvidesHiggsSourceLineageFields: false,
        ProvidesObservedFieldExtractionFields: false),
    new PublicSourceRow(
        "portal-wiki-dark-to-geometric-energy-page",
        "From Dark to Geometric Energy - A Sector of Geometric Unity (YouTube Content)",
        "https://theportal.wiki/wiki/From_Dark_to_Geometric_Energy_-_A_Sector_of_Geometric_Unity_%28YouTube_Content%29",
        "2025-06-09",
        "Portal Wiki metadata records the host as University of California, San Diego, the guest as Eric Weinstein, a 00:50:40 length, a May 22, 2025 release date, and no edited transcript.",
        IsPrimaryOrOfficialPortalSource: false,
        TranscriptAvailable: false,
        ProvidesWzSourceLineageFields: false,
        ProvidesHiggsSourceLineageFields: false,
        ProvidesObservedFieldExtractionFields: false),
};

var ucsdDarkGeometricEnergyLeadPresent = true;
var ucsdDarkGeometricEnergyLecturePubliclyIndexed = true;
var ucsdDarkGeometricEnergyPublicAbstractAvailable = true;
var ucsdDarkGeometricEnergyEditedTranscriptAvailable = false;
var ucsdDarkGeometricEnergyMentionsThetaOmega = true;
var ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup = true;
var ucsdDarkGeometricEnergyMentionsFourteenDimensionalMetrics = true;
var ucsdDarkGeometricEnergyMentionsSupersymmetricEinsteinDirac = true;
var ucsdDarkGeometricEnergyMentionsThreePatiSalamGenerations = true;
var ucsdDarkGeometricEnergyMentionsSeibergWittenMonopoleEquations = true;
var ucsdDarkGeometricEnergyMentionsMatterAndGaugeFields = true;
var ucsdDarkGeometricEnergyProvidesGuLocalWzTheorem = false;
var ucsdDarkGeometricEnergyProvidesSeparateWzSourceRows = false;
var ucsdDarkGeometricEnergyProvidesRawAmplitudeGate = false;
var ucsdDarkGeometricEnergyProvidesCommonBridgeGate = false;
var ucsdDarkGeometricEnergyProvidesTargetIndependentVevSource = false;
var ucsdDarkGeometricEnergyProvidesLowEnergyWeakCouplingSource = false;
var ucsdDarkGeometricEnergyProvidesRgThresholdTransport = false;
var ucsdDarkGeometricEnergyProvidesPhotonWzEigenstateProjectionRows = false;
var ucsdDarkGeometricEnergyProvidesObservedFieldExtraction = false;
var ucsdDarkGeometricEnergyProvidesHiggsScalarSourceOperator = false;
var ucsdDarkGeometricEnergyProvidesHiggsIdentityEnvelope = false;
var ucsdDarkGeometricEnergyProvidesObservedHiggsMassiveScalarProfile = false;
var ucsdDarkGeometricEnergyProvidesHiggsSelfCouplingSource = false;
var ucsdDarkGeometricEnergyPromotesWzMasses = false;
var ucsdDarkGeometricEnergyPromotesHiggsMass = false;
var ucsdDarkGeometricEnergyCompletesBosonPredictions = false;
var canFillPhase201WzContract = false;
var canFillPhase201HiggsContract = false;
var canFillPhase256ObservedFieldExtractionContract = false;

var allPublicSourcesNonPromotable = publicSources.All(row =>
    !row.ProvidesWzSourceLineageFields
    && !row.ProvidesHiggsSourceLineageFields
    && !row.ProvidesObservedFieldExtractionFields);
var priorAuditsRemainBinding = officialPublicSourceAuditMaterialized
    && !officialDraftProvidesCompletionSource
    && currentPublicGuRvgRevisionDeltaAuditPassed
    && officialDraftElectroweakProjectionMapAuditPassed
    && dimensionCasimirWzSourceLawAuditPassed;
var pPhase201BlockersRemainBinding = wzMissingFieldCount == 15
    && higgsMissingFieldCount == 14;
var observedFieldExtractionBlockerRemainsBinding = observedFieldExtractionFilledRequiredFieldCount == 0
    && !observedFieldExtractionContractPromotable;
var pPatiSalamRgBlockersRemainBinding = !patiSalamNormalizationPromotableForLowEnergyWz
    && !lowEnergyRgTransportSourcePromotable;

var checks = new[]
{
    new Check(
        "ucsd-dark-geometric-energy-public-source-recorded",
        ucsdDarkGeometricEnergyLeadPresent
            && ucsdDarkGeometricEnergyLecturePubliclyIndexed
            && ucsdDarkGeometricEnergyPublicAbstractAvailable
            && publicSources.Length == 2,
        $"leadPresent={ucsdDarkGeometricEnergyLeadPresent}; publicAbstract={ucsdDarkGeometricEnergyPublicAbstractAvailable}; sourceCount={publicSources.Length}"),
    new Check(
        "ucsd-dark-geometric-energy-abstract-leads-captured",
        ucsdDarkGeometricEnergyMentionsThetaOmega
            && ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup
            && ucsdDarkGeometricEnergyMentionsFourteenDimensionalMetrics
            && ucsdDarkGeometricEnergyMentionsSupersymmetricEinsteinDirac
            && ucsdDarkGeometricEnergyMentionsThreePatiSalamGenerations
            && ucsdDarkGeometricEnergyMentionsSeibergWittenMonopoleEquations
            && ucsdDarkGeometricEnergyMentionsMatterAndGaugeFields,
        $"thetaOmega={ucsdDarkGeometricEnergyMentionsThetaOmega}; patiSalamGenerations={ucsdDarkGeometricEnergyMentionsThreePatiSalamGenerations}; seibergWitten={ucsdDarkGeometricEnergyMentionsSeibergWittenMonopoleEquations}; matterGaugeFields={ucsdDarkGeometricEnergyMentionsMatterAndGaugeFields}"),
    new Check(
        "public-transcript-not-available-as-source-lineage",
        !ucsdDarkGeometricEnergyEditedTranscriptAvailable
            && publicSources.All(row => !row.TranscriptAvailable),
        $"editedTranscriptAvailable={ucsdDarkGeometricEnergyEditedTranscriptAvailable}; transcriptAvailableCount={publicSources.Count(row => row.TranscriptAvailable)}"),
    new Check(
        "ucsd-source-does-not-fill-wz-contract",
        !ucsdDarkGeometricEnergyProvidesGuLocalWzTheorem
            && !ucsdDarkGeometricEnergyProvidesSeparateWzSourceRows
            && !ucsdDarkGeometricEnergyProvidesRawAmplitudeGate
            && !ucsdDarkGeometricEnergyProvidesCommonBridgeGate
            && !ucsdDarkGeometricEnergyProvidesTargetIndependentVevSource
            && !ucsdDarkGeometricEnergyProvidesLowEnergyWeakCouplingSource
            && !ucsdDarkGeometricEnergyProvidesRgThresholdTransport
            && !ucsdDarkGeometricEnergyProvidesPhotonWzEigenstateProjectionRows
            && !ucsdDarkGeometricEnergyPromotesWzMasses
            && !canFillPhase201WzContract,
        $"guLocalWzTheorem={ucsdDarkGeometricEnergyProvidesGuLocalWzTheorem}; separateWzRows={ucsdDarkGeometricEnergyProvidesSeparateWzSourceRows}; targetIndependentVev={ucsdDarkGeometricEnergyProvidesTargetIndependentVevSource}; lowEnergyWeakCoupling={ucsdDarkGeometricEnergyProvidesLowEnergyWeakCouplingSource}; photonWzRows={ucsdDarkGeometricEnergyProvidesPhotonWzEigenstateProjectionRows}; canFillWz={canFillPhase201WzContract}"),
    new Check(
        "ucsd-source-does-not-fill-higgs-contract",
        !ucsdDarkGeometricEnergyProvidesHiggsScalarSourceOperator
            && !ucsdDarkGeometricEnergyProvidesHiggsIdentityEnvelope
            && !ucsdDarkGeometricEnergyProvidesObservedHiggsMassiveScalarProfile
            && !ucsdDarkGeometricEnergyProvidesHiggsSelfCouplingSource
            && !ucsdDarkGeometricEnergyPromotesHiggsMass
            && !canFillPhase201HiggsContract,
        $"higgsScalarSourceOperator={ucsdDarkGeometricEnergyProvidesHiggsScalarSourceOperator}; higgsIdentityEnvelope={ucsdDarkGeometricEnergyProvidesHiggsIdentityEnvelope}; higgsMassiveProfile={ucsdDarkGeometricEnergyProvidesObservedHiggsMassiveScalarProfile}; higgsSelfCoupling={ucsdDarkGeometricEnergyProvidesHiggsSelfCouplingSource}; canFillHiggs={canFillPhase201HiggsContract}"),
    new Check(
        "ucsd-source-does-not-fill-observed-field-extraction",
        !ucsdDarkGeometricEnergyProvidesObservedFieldExtraction
            && !canFillPhase256ObservedFieldExtractionContract
            && observedFieldExtractionBlockerRemainsBinding,
        $"observedFieldExtraction={ucsdDarkGeometricEnergyProvidesObservedFieldExtraction}; canFillPhase256={canFillPhase256ObservedFieldExtractionContract}; filledRequiredFieldCount={observedFieldExtractionFilledRequiredFieldCount}; phase256Promotable={observedFieldExtractionContractPromotable}"),
    new Check(
        "pati-salam-generation-lead-remains-high-scale-not-low-energy-wz",
        ucsdDarkGeometricEnergyMentionsThreePatiSalamGenerations
            && pPatiSalamRgBlockersRemainBinding,
        $"mentionsThreePatiSalamGenerations={ucsdDarkGeometricEnergyMentionsThreePatiSalamGenerations}; patiSalamNormalizationPromotableForLowEnergyWz={patiSalamNormalizationPromotableForLowEnergyWz}; lowEnergyRgTransportSourcePromotable={lowEnergyRgTransportSourcePromotable}"),
    new Check(
        "prior-source-audits-remain-binding",
        priorAuditsRemainBinding
            && pPhase201BlockersRemainBinding
            && allPublicSourcesNonPromotable,
        $"officialPublicSourceAuditMaterialized={officialPublicSourceAuditMaterialized}; officialDraftProvidesCompletionSource={officialDraftProvidesCompletionSource}; p312Passed={currentPublicGuRvgRevisionDeltaAuditPassed}; p313Passed={officialDraftElectroweakProjectionMapAuditPassed}; p314Passed={dimensionCasimirWzSourceLawAuditPassed}; wzMissingFieldCount={wzMissingFieldCount}; higgsMissingFieldCount={higgsMissingFieldCount}; allSourcesNonPromotable={allPublicSourcesNonPromotable}"),
};

var ucsdDarkGeometricEnergySourceAuditPassed = checks.All(check => check.Passed)
    && !ucsdDarkGeometricEnergyPromotesWzMasses
    && !ucsdDarkGeometricEnergyPromotesHiggsMass
    && !ucsdDarkGeometricEnergyCompletesBosonPredictions;
var terminalStatus = ucsdDarkGeometricEnergySourceAuditPassed
    ? "ucsd-dark-geometric-energy-source-audit-no-wzh-source-lineage"
    : "ucsd-dark-geometric-energy-source-audit-review-required";

var result = new
{
    phaseId = "phase315-ucsd-dark-geometric-energy-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    researchPerformedOn = "2026-05-20",
    ucsdDarkGeometricEnergySourceAuditPassed,
    ucsdDarkGeometricEnergyLeadPresent,
    ucsdDarkGeometricEnergyLecturePubliclyIndexed,
    ucsdDarkGeometricEnergyPublicAbstractAvailable,
    ucsdDarkGeometricEnergyEditedTranscriptAvailable,
    ucsdDarkGeometricEnergyMentionsThetaOmega,
    ucsdDarkGeometricEnergyMentionsInhomogeneousGaugeGroup,
    ucsdDarkGeometricEnergyMentionsFourteenDimensionalMetrics,
    ucsdDarkGeometricEnergyMentionsSupersymmetricEinsteinDirac,
    ucsdDarkGeometricEnergyMentionsThreePatiSalamGenerations,
    ucsdDarkGeometricEnergyMentionsSeibergWittenMonopoleEquations,
    ucsdDarkGeometricEnergyMentionsMatterAndGaugeFields,
    ucsdDarkGeometricEnergyPromotesWzMasses,
    ucsdDarkGeometricEnergyPromotesHiggsMass,
    ucsdDarkGeometricEnergyCompletesBosonPredictions,
    ucsdDarkGeometricEnergyBoundary = new
    {
        ucsdDarkGeometricEnergyProvidesGuLocalWzTheorem,
        ucsdDarkGeometricEnergyProvidesSeparateWzSourceRows,
        ucsdDarkGeometricEnergyProvidesRawAmplitudeGate,
        ucsdDarkGeometricEnergyProvidesCommonBridgeGate,
        ucsdDarkGeometricEnergyProvidesTargetIndependentVevSource,
        ucsdDarkGeometricEnergyProvidesLowEnergyWeakCouplingSource,
        ucsdDarkGeometricEnergyProvidesRgThresholdTransport,
        ucsdDarkGeometricEnergyProvidesPhotonWzEigenstateProjectionRows,
        ucsdDarkGeometricEnergyProvidesObservedFieldExtraction,
        ucsdDarkGeometricEnergyProvidesHiggsScalarSourceOperator,
        ucsdDarkGeometricEnergyProvidesHiggsIdentityEnvelope,
        ucsdDarkGeometricEnergyProvidesObservedHiggsMassiveScalarProfile,
        ucsdDarkGeometricEnergyProvidesHiggsSelfCouplingSource,
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
    },
    priorAuditEvidence = new
    {
        phase218 = new
        {
            officialPublicSourceAuditMaterialized,
            officialDraftProvidesCompletionSource,
        },
        phase235 = new
        {
            patiSalamNormalizationPromotableForLowEnergyWz,
        },
        phase236 = new
        {
            lowEnergyRgTransportSourcePromotable,
        },
        phase312 = new
        {
            currentPublicGuRvgRevisionDeltaAuditPassed,
        },
        phase313 = new
        {
            officialDraftElectroweakProjectionMapAuditPassed,
        },
        phase314 = new
        {
            dimensionCasimirWzSourceLawAuditPassed,
        },
    },
    publicSources,
    checks,
    decision = "Do not promote W/Z or Higgs mass predictions from the UCSD Dark to Geometric Energy public-source lead. The abstract is relevant GU research evidence for dark-energy geometry, Pati-Salam generations, and Seiberg-Witten alignment, but the public material currently provides no transcript-level derivation, W/Z source rows, low-energy weak-coupling or VEV source, photon/W/Z projection, observed-field extraction, or Higgs scalar-source/self-coupling lineage.",
    nextRequiredArtifact = new[]
    {
        "A transcript or paper section deriving a GU-local W/Z source theorem with separate W and Z rows and Phase201/P209 gates.",
        "A low-energy electroweak transport/source artifact connecting any Pati-Salam-generation lead to W/Z couplings, VEV, mass matrix, and observed projection rows.",
        "A solved Higgs scalar source/operator/profile/self-coupling lineage independent of observed W/Z/H targets.",
    },
    sourceEvidence = new
    {
        phase213Path = Phase213Path,
        phase218Path = Phase218Path,
        phase235Path = Phase235Path,
        phase236Path = Phase236Path,
        phase256Path = Phase256Path,
        phase312Path = Phase312Path,
        phase313Path = Phase313Path,
        phase314Path = Phase314Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "ucsd_dark_geometric_energy_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "ucsd_dark_geometric_energy_source_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"ucsdDarkGeometricEnergySourceAuditPassed={ucsdDarkGeometricEnergySourceAuditPassed}");
Console.WriteLine($"ucsdDarkGeometricEnergyLeadPresent={ucsdDarkGeometricEnergyLeadPresent}");
Console.WriteLine($"ucsdDarkGeometricEnergyEditedTranscriptAvailable={ucsdDarkGeometricEnergyEditedTranscriptAvailable}");
Console.WriteLine($"ucsdDarkGeometricEnergyMentionsThreePatiSalamGenerations={ucsdDarkGeometricEnergyMentionsThreePatiSalamGenerations}");
Console.WriteLine($"ucsdDarkGeometricEnergyPromotesWzMasses={ucsdDarkGeometricEnergyPromotesWzMasses}");
Console.WriteLine($"ucsdDarkGeometricEnergyPromotesHiggsMass={ucsdDarkGeometricEnergyPromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? property.GetBoolean()
        : null;
}

static int? JsonInt(JsonElement element, string propertyName)
{
    return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value)
        ? value
        : null;
}

public sealed record Check(
    string CheckId,
    bool Passed,
    string Detail);

public sealed record PublicSourceRow(
    string SourceId,
    string Title,
    string Url,
    string PublishedOrEditedDate,
    string Finding,
    bool IsPrimaryOrOfficialPortalSource,
    bool TranscriptAvailable,
    bool ProvidesWzSourceLineageFields,
    bool ProvidesHiggsSourceLineageFields,
    bool ProvidesObservedFieldExtractionFields);
