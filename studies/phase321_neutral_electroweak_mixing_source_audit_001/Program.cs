using System.Text.Json;

const string DefaultOutputDir = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output";
const string Phase25Path = "studies/phase25_internal_electroweak_features_001/identity_rule_readiness_after_features.json";
const string Phase26Path = "studies/phase26_electroweak_mixing_convention_001/mixing_convention_readiness.json";
const string Phase27Path = "studies/phase27_charge_sector_convention_001/mixing_convention_readiness.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase235Path = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output/pati_salam_weak_mixing_normalization_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase287Path = "studies/phase287_official_draft_parameter_source_gap_audit_001/output/official_draft_parameter_source_gap_audit_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase296Path = "studies/phase296_source_lineage_contract_field_candidate_scan_001/output/source_lineage_contract_field_candidate_scan_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase320Path = "studies/phase320_standard_electroweak_ladder_normalization_boundary_audit_001/output/standard_electroweak_ladder_normalization_boundary_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE321_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase25 = JsonDocument.Parse(File.ReadAllText(Phase25Path));
using var phase26 = JsonDocument.Parse(File.ReadAllText(Phase26Path));
using var phase27 = JsonDocument.Parse(File.ReadAllText(Phase27Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase235 = JsonDocument.Parse(File.ReadAllText(Phase235Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase287 = JsonDocument.Parse(File.ReadAllText(Phase287Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase296 = JsonDocument.Parse(File.ReadAllText(Phase296Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase320 = JsonDocument.Parse(File.ReadAllText(Phase320Path));

var phase25Coverage = JsonArray(phase25.RootElement, "coverage");
var phase25FeatureRecordCount = phase25Coverage.Count;
var phase25Su2AdjointFeatureCount = phase25Coverage.Count(row => JsonBool(row, "hasElectroweakMultipletId") is true);
var phase25ChargedNeutralSectorSignatureCount = phase25Coverage.Count(row => JsonBool(row, "hasChargeSector") is true);
var phase26LegacyMixingConventionBlocked = JsonString(phase26.RootElement, "terminalStatus") == "mixing-convention-blocked";
var phase26LegacyClosureRequirements = JsonStringArray(phase26.RootElement, "closureRequirements");
var phase27InternalCartanConventionReady = JsonString(phase27.RootElement, "terminalStatus") == "mixing-convention-ready"
    && phase27.RootElement.TryGetProperty("convention", out var phase27Convention)
    && JsonString(phase27Convention, "status") == "validated"
    && JsonBool(phase27Convention, "externalTargetValuesUsed") is false;
var phase27NeutralAxisIndex = phase27.RootElement.TryGetProperty("convention", out phase27Convention)
    ? JsonInt(phase27Convention, "neutralBasisAxisIndex")
    : null;
var phase27ChargedAxisCount = phase27.RootElement.TryGetProperty("convention", out phase27Convention)
    && phase27Convention.TryGetProperty("chargedBasisAxisIndices", out var chargedAxisIndices)
    && chargedAxisIndices.ValueKind == JsonValueKind.Array
        ? chargedAxisIndices.GetArrayLength()
        : 0;

var patiSalamWeakMixingNormalizationAuditPassed = JsonBool(phase235.RootElement, "patiSalamWeakMixingNormalizationAuditPassed") is true;
var patiSalamHyperchargeEmbeddingLeadPresent = JsonBool(phase235.RootElement, "patiSalamHyperchargeEmbeddingLeadPresent") is true;
var highScaleWeakMixingBoundaryPresent = JsonBool(phase235.RootElement, "highScaleWeakMixingBoundaryPresent") is true;
var canonicalHighScaleSin2ThetaW = JsonDouble(phase235.RootElement, "canonicalHighScaleSin2ThetaW");
var patiSalamNormalizationPromotableForLowEnergyWz = JsonBool(phase235.RootElement, "patiSalamNormalizationPromotableForLowEnergyWz") is true;
var missingPatiSalamTransportCount = CountUnfilled(phase235.RootElement, "requiredTransport");

var lowEnergyRgTransportSourceAuditPassed = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourceAuditPassed") is true;
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var lowEnergyTransportMissingRequirementCount = CountUnfilled(phase236.RootElement, "requirements");
var lowEnergyHyperchargeSourcePresent = RequirementFilled(phase236.RootElement, "requirements", "low-energy-hypercharge-source");

var officialDraftParameterSourceGapAuditPassed = JsonBool(phase287.RootElement, "officialDraftParameterSourceGapAuditPassed") is true;
var officialGuParameterLocationLeadPresent = JsonBool(phase287.RootElement, "officialGuParameterLocationLeadPresent") is true;
var officialDraftFillsPhase286Gaps = JsonBool(phase287.RootElement, "officialDraftFillsPhase286Gaps") is true;
var officialDraftProvidesRgTransportSource = JsonBool(phase287.RootElement, "officialDraftProvidesRgTransportSource") is true;
var officialDraftProvidesTargetIndependentVevSource = JsonBool(phase287.RootElement, "officialDraftProvidesTargetIndependentVevSource") is true;

var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var officialDraftProvidesWeakIsospinLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakIsospinLocation") is true;
var officialDraftProvidesWeakHyperchargeLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakHyperchargeLocation") is true;
var officialDraftProvidesPhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var officialDraftProvidesElectromagneticUnbrokenGenerator = JsonBool(phase313.RootElement, "officialDraftProvidesElectromagneticUnbrokenGenerator") is true;
var officialDraftProvidesWeakMixingAngleSource = JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var officialDraftProvidesNeutralMassMatrixDiagonalization = JsonBool(phase313.RootElement, "officialDraftProvidesNeutralMassMatrixDiagonalization") is true;
var officialDraftProvidesPhotonMasslessProjectionRow = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonMasslessProjectionRow") is true;
var officialDraftProvidesWChargedProjectionRows = JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows") is true;
var officialDraftProvidesZSourceRowProjection = JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection") is true;
var officialDraftProvidesObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;
var officialDraftProjectionMapCompletesObservedFieldExtraction = JsonBool(phase313.RootElement, "officialDraftProjectionMapCompletesObservedFieldExtraction") is true;
var phase313CanFillPhase201WzContract = JsonBool(phase313.RootElement, "canFillPhase201WzContract") is true;
var phase313CanFillPhase256ObservedFieldExtractionContract = JsonBool(phase313.RootElement, "canFillPhase256ObservedFieldExtractionContract") is true;

var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smDefinesPhotonZWeinbergRotation = JsonBool(phase317.RootElement, "smDefinesPhotonZWeinbergRotation") is true;
var smDefinesChargedWCombination = JsonBool(phase317.RootElement, "smDefinesChargedWCombination") is true;
var smMassMatrixProvidesExternalDependencyMap = JsonBool(phase317.RootElement, "smMassMatrixProvidesExternalDependencyMap") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixCompletesBosonPredictions = JsonBool(phase317.RootElement, "smMassMatrixCompletesBosonPredictions") is true;

var standardElectroweakNormalizationBoundaryAuditPassed = JsonBool(phase320.RootElement, "standardElectroweakNormalizationBoundaryAuditPassed") is true;
var standardZRequiresNeutralSu2U1Mixing = JsonBool(phase320.RootElement, "standardZRequiresNeutralSu2U1Mixing") is true;
var standardPhotonZRotationRequiresWeakMixingAngle = JsonBool(phase320.RootElement, "standardPhotonZRotationRequiresWeakMixingAngle") is true;
var standardElectroweakBoundaryPromotesWzMasses = JsonBool(phase320.RootElement, "standardElectroweakBoundaryPromotesWzMasses") is true;
var standardElectroweakBoundaryCompletesBosonPredictions = JsonBool(phase320.RootElement, "standardElectroweakBoundaryCompletesBosonPredictions") is true;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase295IntakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var phase295AnyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var phase296IntakeReadySourceLineageFieldCandidateCount = JsonInt(phase296.RootElement, "intakeReadySourceLineageFieldCandidateCount") ?? -1;
var phase296AnySourceLineageCandidateFillsContract = JsonBool(phase296.RootElement, "anySourceLineageCandidateFillsContract") is true;

const bool neutralMixingRouteProvidesTargetIndependentWeakMixingAngle = false;
const bool neutralMixingRouteProvidesSourceDerivedHyperchargeCoupling = false;
const bool neutralMixingRouteProvidesPhotonMasslessProjection = false;
const bool neutralMixingRouteProvidesZPhysicalProjectionRow = false;
const bool neutralMixingRouteProvidesObservedEmbedding = false;
const bool neutralMixingRoutePromotesWzMasses = false;
const bool neutralMixingRouteCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var neutralMixingRequirements = new[]
{
    new Requirement("internal-su2-adjoint-sector-features", "Internal SU(2)-adjoint features exist before physical identification.", phase25FeatureRecordCount == 12 && phase25Su2AdjointFeatureCount == 12),
    new Requirement("internal-cartan-charge-sector-convention", "Phase27 supplies a target-independent internal charged/neutral axis convention.", phase27InternalCartanConventionReady && phase27NeutralAxisIndex == 2 && phase27ChargedAxisCount == 2),
    new Requirement("weak-hypercharge-location", "Official-draft placement language locates a weak-hypercharge-like sector.", officialDraftProvidesWeakHyperchargeLocation),
    new Requirement("low-energy-hypercharge-coupling-source", "A target-independent low-energy hypercharge coupling or g-prime source row.", lowEnergyHyperchargeSourcePresent),
    new Requirement("weak-mixing-angle-source", "A GU-local weak mixing angle or equivalent g/g-prime source before target comparison.", officialDraftProvidesWeakMixingAngleSource),
    new Requirement("photon-z-weinberg-rotation", "A source-derived neutral W3/B rotation into photon and Z rows.", officialDraftProvidesPhotonZWeinbergRotation),
    new Requirement("unbroken-electromagnetic-generator", "A source-derived unbroken U(1)_em generator with photon masslessness.", officialDraftProvidesElectromagneticUnbrokenGenerator),
    new Requirement("neutral-mass-matrix-diagonalization", "A source-derived neutral mass matrix and diagonalization.", officialDraftProvidesNeutralMassMatrixDiagonalization),
    new Requirement("photon-massless-projection-row", "A photon projection row carrying a massless gate.", officialDraftProvidesPhotonMasslessProjectionRow),
    new Requirement("physical-z-source-row", "A physical Z source row with branch, normalization, and source-lineage provenance.", officialDraftProvidesZSourceRowProjection),
    new Requirement("observed-electroweak-gauge-embedding", "A branch-stable observed electroweak embedding tying GU fields to W/Z/photon observables.", officialDraftProvidesObservedElectroweakGaugeEmbedding),
    new Requirement("wz-source-lineage-contract", "Phase201 W/Z source-lineage intake fields and gates filled by the neutral route.", canFillPhase201WzContract),
};

var checks = new[]
{
    new Check(
        "legacy-internal-feature-gap-and-phase27-ratio-lane-recorded",
        phase25FeatureRecordCount == 12
            && phase25Su2AdjointFeatureCount == 12
            && phase25ChargedNeutralSectorSignatureCount == 0
            && phase26LegacyMixingConventionBlocked
            && phase26LegacyClosureRequirements.Count == 3
            && phase27InternalCartanConventionReady
            && phase27NeutralAxisIndex == 2
            && phase27ChargedAxisCount == 2,
        $"phase25Records={phase25FeatureRecordCount}; su2AdjointFeatures={phase25Su2AdjointFeatureCount}; chargedNeutralSignatures={phase25ChargedNeutralSectorSignatureCount}; phase26Blocked={phase26LegacyMixingConventionBlocked}; phase27Ready={phase27InternalCartanConventionReady}; neutralAxis={phase27NeutralAxisIndex}; chargedAxisCount={phase27ChargedAxisCount}"),
    new Check(
        "pati-salam-boundary-is-high-scale-not-low-energy-source",
        patiSalamWeakMixingNormalizationAuditPassed
            && patiSalamHyperchargeEmbeddingLeadPresent
            && highScaleWeakMixingBoundaryPresent
            && canonicalHighScaleSin2ThetaW == 0.375
            && !patiSalamNormalizationPromotableForLowEnergyWz
            && missingPatiSalamTransportCount == 4,
        $"p235Passed={patiSalamWeakMixingNormalizationAuditPassed}; hyperchargeLead={patiSalamHyperchargeEmbeddingLeadPresent}; highScaleBoundary={highScaleWeakMixingBoundaryPresent}; sin2={canonicalHighScaleSin2ThetaW}; lowEnergyPromotable={patiSalamNormalizationPromotableForLowEnergyWz}; missingTransport={missingPatiSalamTransportCount}"),
    new Check(
        "low-energy-rg-hypercharge-transport-source-missing",
        lowEnergyRgTransportSourceAuditPassed
            && !lowEnergyRgTransportSourcePromotable
            && lowEnergyTransportMissingRequirementCount == 4
            && !lowEnergyHyperchargeSourcePresent,
        $"p236Passed={lowEnergyRgTransportSourceAuditPassed}; rgTransportPromotable={lowEnergyRgTransportSourcePromotable}; missingRequirements={lowEnergyTransportMissingRequirementCount}; lowEnergyHyperchargeSourcePresent={lowEnergyHyperchargeSourcePresent}"),
    new Check(
        "official-draft-locations-do-not-supply-neutral-projection-map",
        officialDraftParameterSourceGapAuditPassed
            && officialGuParameterLocationLeadPresent
            && officialDraftElectroweakProjectionMapAuditPassed
            && officialDraftProvidesWeakIsospinLocation
            && officialDraftProvidesWeakHyperchargeLocation
            && !officialDraftFillsPhase286Gaps
            && !officialDraftProvidesRgTransportSource
            && !officialDraftProvidesTargetIndependentVevSource
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesElectromagneticUnbrokenGenerator
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesNeutralMassMatrixDiagonalization
            && !officialDraftProvidesPhotonMasslessProjectionRow
            && !officialDraftProvidesWChargedProjectionRows
            && !officialDraftProvidesZSourceRowProjection
            && !officialDraftProvidesObservedElectroweakGaugeEmbedding,
        $"p287Passed={officialDraftParameterSourceGapAuditPassed}; p313Passed={officialDraftElectroweakProjectionMapAuditPassed}; isospinLocation={officialDraftProvidesWeakIsospinLocation}; hyperchargeLocation={officialDraftProvidesWeakHyperchargeLocation}; draftFillsP286={officialDraftFillsPhase286Gaps}; rgSource={officialDraftProvidesRgTransportSource}; vevSource={officialDraftProvidesTargetIndependentVevSource}; photonZRotation={officialDraftProvidesPhotonZWeinbergRotation}; weakMixingSource={officialDraftProvidesWeakMixingAngleSource}; neutralMassMatrix={officialDraftProvidesNeutralMassMatrixDiagonalization}; zProjection={officialDraftProvidesZSourceRowProjection}; observedEmbedding={officialDraftProvidesObservedElectroweakGaugeEmbedding}"),
    new Check(
        "standard-neutral-map-dependency-does-not-promote-gu-source-law",
        electroweakMassMatrixBridgeSourceAuditPassed
            && smDefinesPhotonZWeinbergRotation
            && smDefinesChargedWCombination
            && smMassMatrixProvidesExternalDependencyMap
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixCompletesBosonPredictions
            && standardElectroweakNormalizationBoundaryAuditPassed
            && standardZRequiresNeutralSu2U1Mixing
            && standardPhotonZRotationRequiresWeakMixingAngle
            && !standardElectroweakBoundaryPromotesWzMasses
            && !standardElectroweakBoundaryCompletesBosonPredictions,
        $"p317Passed={electroweakMassMatrixBridgeSourceAuditPassed}; smPhotonZ={smDefinesPhotonZWeinbergRotation}; externalDependencyMap={smMassMatrixProvidesExternalDependencyMap}; smPromotesWz={smMassMatrixPromotesWzMasses}; p320Passed={standardElectroweakNormalizationBoundaryAuditPassed}; zRequiresMixing={standardZRequiresNeutralSu2U1Mixing}; weakAngleRequired={standardPhotonZRotationRequiresWeakMixingAngle}; standardPromotesWz={standardElectroweakBoundaryPromotesWzMasses}"),
    new Check(
        "neutral-mixing-route-does-not-fill-contracts-or-scanners",
        !neutralMixingRouteProvidesTargetIndependentWeakMixingAngle
            && !neutralMixingRouteProvidesSourceDerivedHyperchargeCoupling
            && !neutralMixingRouteProvidesPhotonMasslessProjection
            && !neutralMixingRouteProvidesZPhysicalProjectionRow
            && !neutralMixingRouteProvidesObservedEmbedding
            && !neutralMixingRoutePromotesWzMasses
            && !neutralMixingRouteCompletesBosonPredictions
            && !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && phase295IntakeReadyObservedFieldExtractionCandidateCount == 0
            && !phase295AnyObservedFieldExtractionCandidateFillsContract
            && phase296IntakeReadySourceLineageFieldCandidateCount == 0
            && !phase296AnySourceLineageCandidateFillsContract
            && !phase313CanFillPhase201WzContract
            && !phase313CanFillPhase256ObservedFieldExtractionContract
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"weakMixingAngle={neutralMixingRouteProvidesTargetIndependentWeakMixingAngle}; hyperchargeCoupling={neutralMixingRouteProvidesSourceDerivedHyperchargeCoupling}; photonProjection={neutralMixingRouteProvidesPhotonMasslessProjection}; zProjection={neutralMixingRouteProvidesZPhysicalProjectionRow}; observedEmbedding={neutralMixingRouteProvidesObservedEmbedding}; p201AllLineages={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; p295IntakeReady={phase295IntakeReadyObservedFieldExtractionCandidateCount}; p296IntakeReady={phase296IntakeReadySourceLineageFieldCandidateCount}"),
};

var neutralElectroweakMixingSourceAuditPassed = checks.All(check => check.Passed)
    && neutralMixingRequirements.Count(requirement => requirement.Filled) == 3
    && !neutralMixingRoutePromotesWzMasses
    && !neutralMixingRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;
var terminalStatus = neutralElectroweakMixingSourceAuditPassed
    ? "neutral-electroweak-mixing-source-audit-no-promotable-source"
    : "neutral-electroweak-mixing-source-audit-review-required";

var result = new
{
    phaseId = "phase321-neutral-electroweak-mixing-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    neutralElectroweakMixingSourceAuditPassed,
    phase25Su2AdjointFeatureCount,
    phase25ChargedNeutralSectorSignatureCount,
    phase26LegacyMixingConventionBlocked,
    phase27InternalCartanConventionReady,
    phase27NeutralAxisIndex,
    phase27ChargedAxisCount,
    patiSalamHyperchargeEmbeddingLeadPresent,
    highScaleWeakMixingBoundaryPresent,
    canonicalHighScaleSin2ThetaW,
    patiSalamNormalizationPromotableForLowEnergyWz,
    lowEnergyRgTransportSourcePromotable,
    lowEnergyHyperchargeSourcePresent,
    officialDraftProvidesWeakIsospinLocation,
    officialDraftProvidesWeakHyperchargeLocation,
    officialDraftProvidesPhotonZWeinbergRotation,
    officialDraftProvidesElectromagneticUnbrokenGenerator,
    officialDraftProvidesWeakMixingAngleSource,
    officialDraftProvidesNeutralMassMatrixDiagonalization,
    officialDraftProvidesPhotonMasslessProjectionRow,
    officialDraftProvidesWChargedProjectionRows,
    officialDraftProvidesZSourceRowProjection,
    officialDraftProvidesObservedElectroweakGaugeEmbedding,
    officialDraftProjectionMapCompletesObservedFieldExtraction,
    smDefinesPhotonZWeinbergRotation,
    smMassMatrixProvidesExternalDependencyMap,
    standardZRequiresNeutralSu2U1Mixing,
    standardPhotonZRotationRequiresWeakMixingAngle,
    neutralMixingRouteProvidesTargetIndependentWeakMixingAngle,
    neutralMixingRouteProvidesSourceDerivedHyperchargeCoupling,
    neutralMixingRouteProvidesPhotonMasslessProjection,
    neutralMixingRouteProvidesZPhysicalProjectionRow,
    neutralMixingRouteProvidesObservedEmbedding,
    neutralMixingRoutePromotesWzMasses,
    neutralMixingRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    neutralMixingRequirements,
    inheritedEvidence = new
    {
        phase25 = new
        {
            path = Phase25Path,
            phase25FeatureRecordCount,
            phase25Su2AdjointFeatureCount,
            phase25ChargedNeutralSectorSignatureCount,
        },
        phase26 = new
        {
            path = Phase26Path,
            phase26LegacyMixingConventionBlocked,
            phase26LegacyClosureRequirements,
        },
        phase27 = new
        {
            path = Phase27Path,
            phase27InternalCartanConventionReady,
            phase27NeutralAxisIndex,
            phase27ChargedAxisCount,
        },
        phase235 = new
        {
            path = Phase235Path,
            patiSalamWeakMixingNormalizationAuditPassed,
            patiSalamHyperchargeEmbeddingLeadPresent,
            highScaleWeakMixingBoundaryPresent,
            canonicalHighScaleSin2ThetaW,
            patiSalamNormalizationPromotableForLowEnergyWz,
            missingPatiSalamTransportCount,
        },
        phase236 = new
        {
            path = Phase236Path,
            lowEnergyRgTransportSourceAuditPassed,
            lowEnergyRgTransportSourcePromotable,
            lowEnergyTransportMissingRequirementCount,
            lowEnergyHyperchargeSourcePresent,
        },
        phase287 = new
        {
            path = Phase287Path,
            officialDraftParameterSourceGapAuditPassed,
            officialGuParameterLocationLeadPresent,
            officialDraftFillsPhase286Gaps,
            officialDraftProvidesRgTransportSource,
            officialDraftProvidesTargetIndependentVevSource,
        },
        phase313 = new
        {
            path = Phase313Path,
            officialDraftElectroweakProjectionMapAuditPassed,
            officialDraftProvidesWeakIsospinLocation,
            officialDraftProvidesWeakHyperchargeLocation,
            officialDraftProvidesPhotonZWeinbergRotation,
            officialDraftProvidesElectromagneticUnbrokenGenerator,
            officialDraftProvidesWeakMixingAngleSource,
            officialDraftProvidesNeutralMassMatrixDiagonalization,
            officialDraftProvidesPhotonMasslessProjectionRow,
            officialDraftProvidesWChargedProjectionRows,
            officialDraftProvidesZSourceRowProjection,
            officialDraftProvidesObservedElectroweakGaugeEmbedding,
            officialDraftProjectionMapCompletesObservedFieldExtraction,
            phase313CanFillPhase201WzContract,
            phase313CanFillPhase256ObservedFieldExtractionContract,
        },
        phase317 = new
        {
            path = Phase317Path,
            electroweakMassMatrixBridgeSourceAuditPassed,
            smDefinesPhotonZWeinbergRotation,
            smDefinesChargedWCombination,
            smMassMatrixProvidesExternalDependencyMap,
            smMassMatrixPromotesWzMasses,
            smMassMatrixCompletesBosonPredictions,
        },
        phase320 = new
        {
            path = Phase320Path,
            standardElectroweakNormalizationBoundaryAuditPassed,
            standardZRequiresNeutralSu2U1Mixing,
            standardPhotonZRotationRequiresWeakMixingAngle,
            standardElectroweakBoundaryPromotesWzMasses,
            standardElectroweakBoundaryCompletesBosonPredictions,
        },
        contracts = new
        {
            phase201Path = Phase201Path,
            phase213Path = Phase213Path,
            phase256Path = Phase256Path,
            phase295Path = Phase295Path,
            phase296Path = Phase296Path,
            phase201AllRequiredLineagesPromotable,
            existingEvidenceFound,
            wzMissingFieldCount,
            higgsMissingFieldCount,
            observedFieldExtractionRequiredFieldCount,
            observedFieldExtractionFilledRequiredFieldCount,
            observedFieldExtractionContractPromotable,
            phase295IntakeReadyObservedFieldExtractionCandidateCount,
            phase295AnyObservedFieldExtractionCandidateFillsContract,
            phase296IntakeReadySourceLineageFieldCandidateCount,
            phase296AnySourceLineageCandidateFillsContract,
        },
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
    checks,
    decision = neutralElectroweakMixingSourceAuditPassed
        ? "Do not promote the neutral electroweak mixing route as a W/Z source law. The repo has an internal SU(2) Cartan convention and official-draft weak-isospin/hypercharge location leads, but it lacks a target-independent low-energy hypercharge coupling, weak-mixing angle, photon/Z rotation, unbroken electromagnetic generator, neutral mass-matrix diagonalization, observed electroweak embedding, and physical Z source row. Standard electroweak theory supplies the dependency shape only after importing external v, g, g-prime, and mixing data."
        : "Review the neutral electroweak mixing source audit before using it as boson prediction evidence.",
    nextRequiredArtifact = new[]
    {
        "A GU-local low-energy hypercharge coupling or weak-mixing angle source before target comparison.",
        "A source-derived neutral mass matrix and photon/Z diagonalization with a massless photon row.",
        "A branch-stable observed electroweak embedding mapping GU fields to W, Z, and photon observables.",
        "W/Z source-lineage rows satisfying Phase201 and observed-field extraction gates without target-fitted scaling.",
    },
    sourceEvidence = new
    {
        phase25Path = Phase25Path,
        phase26Path = Phase26Path,
        phase27Path = Phase27Path,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase235Path = Phase235Path,
        phase236Path = Phase236Path,
        phase256Path = Phase256Path,
        phase287Path = Phase287Path,
        phase295Path = Phase295Path,
        phase296Path = Phase296Path,
        phase313Path = Phase313Path,
        phase317Path = Phase317Path,
        phase320Path = Phase320Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "neutral_electroweak_mixing_source_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "neutral_electroweak_mixing_source_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"neutralElectroweakMixingSourceAuditPassed={neutralElectroweakMixingSourceAuditPassed}");
Console.WriteLine($"lowEnergyHyperchargeSourcePresent={lowEnergyHyperchargeSourcePresent}");
Console.WriteLine($"officialDraftProvidesWeakMixingAngleSource={officialDraftProvidesWeakMixingAngleSource}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static IReadOnlyList<JsonElement> JsonArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
        ? value.EnumerateArray().ToArray()
        : Array.Empty<JsonElement>();

static IReadOnlyList<string> JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
        ? value.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString() ?? "")
            .ToArray()
        : Array.Empty<string>();

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static int CountUnfilled(JsonElement element, string propertyName) =>
    JsonArray(element, propertyName).Count(row => JsonBool(row, "filled") is false);

static bool RequirementFilled(JsonElement element, string propertyName, string requirementId) =>
    JsonArray(element, propertyName).Any(row =>
        JsonString(row, "requirementId") == requirementId
        && JsonBool(row, "filled") is true);

public sealed record Requirement(string RequirementId, string Detail, bool Filled);

public sealed record Check(string CheckId, bool Passed, string Detail);
