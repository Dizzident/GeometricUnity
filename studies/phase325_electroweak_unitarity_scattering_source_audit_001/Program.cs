using System.Text.Json;

const string DefaultOutputDir = "studies/phase325_electroweak_unitarity_scattering_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase244Path = "studies/phase244_electroweak_identifiability_rank_audit_001/output/electroweak_identifiability_rank_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase296Path = "studies/phase296_source_lineage_contract_field_candidate_scan_001/output/source_lineage_contract_field_candidate_scan_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase320Path = "studies/phase320_standard_electroweak_ladder_normalization_boundary_audit_001/output/standard_electroweak_ladder_normalization_boundary_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase324Path = "studies/phase324_custodial_rho_parameter_source_audit_001/output/custodial_rho_parameter_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE325_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase244 = JsonDocument.Parse(File.ReadAllText(Phase244Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase296 = JsonDocument.Parse(File.ReadAllText(Phase296Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase320 = JsonDocument.Parse(File.ReadAllText(Phase320Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase324 = JsonDocument.Parse(File.ReadAllText(Phase324Path));

const string pdg2025HiggsReviewUrl = "https://pdgweb.lbl.gov/2025/reviews/rpp2025-rev-higgs-boson.pdf";
const string leeQuiggThackerCernRecordUrl = "https://cds.cern.ch/record/423909";
const bool pdgHiggsUnitarityReferencePresent = true;
const bool leeQuiggThackerReferencePresent = true;
const bool longitudinalWzScatteringUnitarityLeadPresent = true;
const bool higgsRestoresPerturbativeUnitarityLeadPresent = true;
const bool unitarityRouteProvidesConsistencyBound = true;
const bool unitarityRouteProvidesUpperBoundOnly = true;
const bool unitarityRouteProvidesExactHiggsMass = false;
const bool unitarityRouteProvidesAbsoluteWzScale = false;
const bool unitarityRouteProvidesWeakMixingAngleSource = false;
const bool unitarityRouteProvidesTargetIndependentVevSource = false;
const bool unitarityRouteProvidesGaugeCouplingNormalization = false;
const bool unitarityRouteProvidesObservedFieldExtraction = false;
const bool unitarityRouteProvidesHiggsScalarSelfCouplingSource = false;
const bool unitarityRouteProvidesPhotonWzProjectionRows = false;
const bool unitarityRoutePromotesWzMasses = false;
const bool unitarityRoutePromotesHiggsMass = false;
const bool unitarityRouteCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;

var phase224Closure = phase224.RootElement.GetProperty("closure");
var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var wAbsoluteMassParameterClosure = JsonBool(phase224Closure, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224Closure, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224Closure, "higgsMassParameterClosure") is true;
var weakCouplingSourcePromotable = JsonBool(phase224Closure, "weakCouplingSourcePromotable") is true;
var canPromoteExternalElectroweakBridge = JsonBool(phase224Closure, "canPromoteExternalElectroweakBridge") is true;

var electroweakIdentifiabilityRankAuditPassed = JsonBool(phase244.RootElement, "electroweakIdentifiabilityRankAuditPassed") is true;
var promotedWzRatioPresent = JsonBool(phase244.RootElement, "promotedWzRatioPresent") is true;
var anyPromotedAbsoluteMassPresent = JsonBool(phase244.RootElement, "anyPromotedAbsoluteMassPresent") is true;
var currentPromotedConstraintRank = JsonInt(phase244.RootElement, "currentPromotedConstraintRank") ?? -1;
var remainingNullity = JsonInt(phase244.RootElement, "remainingNullity") ?? -1;
var minimumAdditionalIndependentSourceConstraints = JsonInt(phase244.RootElement, "minimumAdditionalIndependentSourceConstraints") ?? -1;

var rankDeficitMinimalUnlockContractPassed = JsonBool(phase245.RootElement, "rankDeficitMinimalUnlockContractPassed") is true;
var unlockContractFilled = JsonBool(phase245.RootElement, "unlockContractFilled") is true;
var newSourceEvidenceStillRequired = JsonBool(phase245.RootElement, "newSourceEvidenceStillRequired") is true;
var unlockRowCount = JsonArray(phase245.RootElement, "unlockRows").Count;

var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var phase295IntakeReadyObservedFieldExtractionCandidateCount = JsonInt(phase295.RootElement, "intakeReadyObservedFieldExtractionCandidateCount") ?? -1;
var phase295AnyObservedFieldExtractionCandidateFillsContract = JsonBool(phase295.RootElement, "anyObservedFieldExtractionCandidateFillsContract") is true;
var phase296IntakeReadySourceLineageFieldCandidateCount = JsonInt(phase296.RootElement, "intakeReadySourceLineageFieldCandidateCount") ?? -1;
var phase296AnySourceLineageCandidateFillsContract = JsonBool(phase296.RootElement, "anySourceLineageCandidateFillsContract") is true;

var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var officialDraftProvidesWeakIsospinLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakIsospinLocation") is true;
var officialDraftProvidesWeakHyperchargeLocation = JsonBool(phase313.RootElement, "officialDraftProvidesWeakHyperchargeLocation") is true;
var officialDraftProvidesPhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var officialDraftProvidesWeakMixingAngleSource = JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var officialDraftProvidesNeutralMassMatrixDiagonalization = JsonBool(phase313.RootElement, "officialDraftProvidesNeutralMassMatrixDiagonalization") is true;
var officialDraftProvidesWChargedProjectionRows = JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows") is true;
var officialDraftProvidesZSourceRowProjection = JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection") is true;
var officialDraftProvidesObservedElectroweakGaugeEmbedding = JsonBool(phase313.RootElement, "officialDraftProvidesObservedElectroweakGaugeEmbedding") is true;

var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smMassGenerationRequiresHiggsDoublet = JsonBool(phase317.RootElement, "smMassGenerationRequiresHiggsDoublet") is true;
var smMassGenerationRequiresVev = JsonBool(phase317.RootElement, "smMassGenerationRequiresVev") is true;
var smMassGenerationRequiresWeakCouplingG = JsonBool(phase317.RootElement, "smMassGenerationRequiresWeakCouplingG") is true;
var smMassGenerationRequiresHyperchargeCouplingGPrime = JsonBool(phase317.RootElement, "smMassGenerationRequiresHyperchargeCouplingGPrime") is true;
var smTreeLevelHiggsMassDependsOnPotentialParameter = JsonBool(phase317.RootElement, "smTreeLevelHiggsMassDependsOnPotentialParameter") is true;
var smMassMatrixProvidesExternalDependencyMap = JsonBool(phase317.RootElement, "smMassMatrixProvidesExternalDependencyMap") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;

var standardElectroweakNormalizationBoundaryAuditPassed = JsonBool(phase320.RootElement, "standardElectroweakNormalizationBoundaryAuditPassed") is true;
var standardZRequiresNeutralSu2U1Mixing = JsonBool(phase320.RootElement, "standardZRequiresNeutralSu2U1Mixing") is true;
var standardPhotonZRotationRequiresWeakMixingAngle = JsonBool(phase320.RootElement, "standardPhotonZRotationRequiresWeakMixingAngle") is true;
var standardTreeLevelMassesRequireGgv = JsonBool(phase320.RootElement, "standardTreeLevelMassesRequireGgv") is true;
var standardTreeLevelHiggsRequiresLambdaAndV = JsonBool(phase320.RootElement, "standardTreeLevelHiggsRequiresLambdaAndV") is true;
var standardElectroweakBoundaryCompletesBosonPredictions = JsonBool(phase320.RootElement, "standardElectroweakBoundaryCompletesBosonPredictions") is true;

var neutralElectroweakMixingSourceAuditPassed = JsonBool(phase321.RootElement, "neutralElectroweakMixingSourceAuditPassed") is true;
var lowEnergyHyperchargeSourcePresent = JsonBool(phase321.RootElement, "lowEnergyHyperchargeSourcePresent") is true;
var neutralMixingRouteProvidesTargetIndependentWeakMixingAngle = JsonBool(phase321.RootElement, "neutralMixingRouteProvidesTargetIndependentWeakMixingAngle") is true;
var neutralMixingRouteCompletesBosonPredictions = JsonBool(phase321.RootElement, "neutralMixingRouteCompletesBosonPredictions") is true;

var higgsUpsilonScalarSourceBoundaryAuditPassed = JsonBool(phase322.RootElement, "higgsUpsilonScalarSourceBoundaryAuditPassed") is true;
var officialGuSourcesProvideFixedScalarSourceOperator = JsonBool(phase322.RootElement, "officialGuSourcesProvideFixedScalarSourceOperator") is true;
var officialGuSourcesProvideMassiveScalarProfile = JsonBool(phase322.RootElement, "officialGuSourcesProvideMassiveScalarProfile") is true;
var officialGuSourcesProvideQuarticSelfCouplingValue = JsonBool(phase322.RootElement, "officialGuSourcesProvideQuarticSelfCouplingValue") is true;
var higgsUpsilonRouteCompletesBosonPredictions = JsonBool(phase322.RootElement, "higgsUpsilonRouteCompletesBosonPredictions") is true;

var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialPublicSourcesProvideTargetIndependentVevSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideTargetIndependentVevSource") is true;
var officialPublicSourcesProvideGaugeFixedQuadraticExpansion = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeFixedQuadraticExpansion") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;

var custodialRhoParameterSourceAuditPassed = JsonBool(phase324.RootElement, "custodialRhoParameterSourceAuditPassed") is true;
var rhoRelationProvidesAbsoluteWzScale = JsonBool(phase324.RootElement, "rhoRelationProvidesAbsoluteWzScale") is true;
var rhoRelationProvidesWeakMixingAngleSource = JsonBool(phase324.RootElement, "rhoRelationProvidesWeakMixingAngleSource") is true;
var custodialRouteCompletesBosonPredictions = JsonBool(phase324.RootElement, "custodialRouteCompletesBosonPredictions") is true;

var unitarityRequirements = new[]
{
    new Requirement("longitudinal-wz-scattering-unitarity", "External electroweak references record longitudinal W/Z scattering unitarity as a real consistency constraint.", longitudinalWzScatteringUnitarityLeadPresent),
    new Requirement("higgs-restores-perturbative-unitarity", "The SM Higgs mechanism cancels bad high-energy growth in longitudinal W/Z scattering amplitudes.", higgsRestoresPerturbativeUnitarityLeadPresent),
    new Requirement("exact-higgs-mass-source", "A target-independent exact Higgs mass source, not merely a consistency or upper-bound argument.", unitarityRouteProvidesExactHiggsMass),
    new Requirement("target-independent-wz-absolute-scale", "A target-independent GU W/Z absolute-scale source row.", unitarityRouteProvidesAbsoluteWzScale),
    new Requirement("target-independent-weak-angle-or-coupling-source", "A target-independent weak-angle or gauge-coupling source.", unitarityRouteProvidesWeakMixingAngleSource && unitarityRouteProvidesGaugeCouplingNormalization),
    new Requirement("target-independent-vev-source", "A target-independent electroweak VEV or vacuum-selection source.", unitarityRouteProvidesTargetIndependentVevSource),
    new Requirement("observed-field-extraction", "Observed photon/W/Z/H projection and field-extraction rows.", unitarityRouteProvidesObservedFieldExtraction && unitarityRouteProvidesPhotonWzProjectionRows),
    new Requirement("higgs-scalar-self-coupling-source", "A solved Higgs scalar self-coupling/source lineage.", unitarityRouteProvidesHiggsScalarSelfCouplingSource),
};

var checks = new[]
{
    new Check(
        "unitarity-scattering-reference-recorded",
        pdgHiggsUnitarityReferencePresent
            && leeQuiggThackerReferencePresent
            && longitudinalWzScatteringUnitarityLeadPresent
            && higgsRestoresPerturbativeUnitarityLeadPresent
            && unitarityRouteProvidesConsistencyBound
            && unitarityRouteProvidesUpperBoundOnly,
        $"pdgReference={pdgHiggsUnitarityReferencePresent}; lqtReference={leeQuiggThackerReferencePresent}; wzScattering={longitudinalWzScatteringUnitarityLeadPresent}; higgsRestores={higgsRestoresPerturbativeUnitarityLeadPresent}; consistencyBound={unitarityRouteProvidesConsistencyBound}; upperBoundOnly={unitarityRouteProvidesUpperBoundOnly}"),
    new Check(
        "unitarity-bound-is-not-exact-source-row",
        unitarityRouteProvidesUpperBoundOnly
            && !unitarityRouteProvidesExactHiggsMass
            && !unitarityRouteProvidesAbsoluteWzScale
            && !unitarityRouteProvidesWeakMixingAngleSource
            && !unitarityRouteProvidesTargetIndependentVevSource
            && !unitarityRouteProvidesHiggsScalarSelfCouplingSource
            && electroweakIdentifiabilityRankAuditPassed
            && promotedWzRatioPresent
            && currentPromotedConstraintRank == 1
            && remainingNullity == 2
            && minimumAdditionalIndependentSourceConstraints == 2
            && !anyPromotedAbsoluteMassPresent,
        $"upperBoundOnly={unitarityRouteProvidesUpperBoundOnly}; exactHiggs={unitarityRouteProvidesExactHiggsMass}; absoluteWz={unitarityRouteProvidesAbsoluteWzScale}; weakAngle={unitarityRouteProvidesWeakMixingAngleSource}; vev={unitarityRouteProvidesTargetIndependentVevSource}; higgsSelfCoupling={unitarityRouteProvidesHiggsScalarSelfCouplingSource}; rank={currentPromotedConstraintRank}; nullity={remainingNullity}; anyAbsolute={anyPromotedAbsoluteMassPresent}"),
    new Check(
        "standard-sm-dependency-map-remains-external",
        electroweakMassMatrixBridgeSourceAuditPassed
            && smMassGenerationRequiresHiggsDoublet
            && smMassGenerationRequiresVev
            && smMassGenerationRequiresWeakCouplingG
            && smMassGenerationRequiresHyperchargeCouplingGPrime
            && smTreeLevelHiggsMassDependsOnPotentialParameter
            && smMassMatrixProvidesExternalDependencyMap
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && standardElectroweakNormalizationBoundaryAuditPassed
            && standardZRequiresNeutralSu2U1Mixing
            && standardPhotonZRotationRequiresWeakMixingAngle
            && standardTreeLevelMassesRequireGgv
            && standardTreeLevelHiggsRequiresLambdaAndV
            && !standardElectroweakBoundaryCompletesBosonPredictions,
        $"p317Passed={electroweakMassMatrixBridgeSourceAuditPassed}; smExternalMap={smMassMatrixProvidesExternalDependencyMap}; smPromotesWz={smMassMatrixPromotesWzMasses}; smPromotesHiggs={smMassMatrixPromotesHiggsMass}; p320Passed={standardElectroweakNormalizationBoundaryAuditPassed}; ggv={standardTreeLevelMassesRequireGgv}; lambdaV={standardTreeLevelHiggsRequiresLambdaAndV}; p320Completes={standardElectroweakBoundaryCompletesBosonPredictions}"),
    new Check(
        "official-and-coupled-gu-extraction-gates-remain-blocked",
        officialDraftElectroweakProjectionMapAuditPassed
            && officialDraftProvidesWeakIsospinLocation
            && officialDraftProvidesWeakHyperchargeLocation
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesNeutralMassMatrixDiagonalization
            && !officialDraftProvidesWChargedProjectionRows
            && !officialDraftProvidesZSourceRowProjection
            && !officialDraftProvidesObservedElectroweakGaugeEmbedding
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeFixedQuadraticExpansion
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions,
        $"p313Passed={officialDraftElectroweakProjectionMapAuditPassed}; weakIsospin={officialDraftProvidesWeakIsospinLocation}; weakHypercharge={officialDraftProvidesWeakHyperchargeLocation}; p313PhotonZ={officialDraftProvidesPhotonZWeinbergRotation}; p313WeakAngle={officialDraftProvidesWeakMixingAngleSource}; p313Observed={officialDraftProvidesObservedElectroweakGaugeEmbedding}; p323Passed={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Vev={officialPublicSourcesProvideTargetIndependentVevSource}; p323Quadratic={officialPublicSourcesProvideGaugeFixedQuadraticExpansion}; p323Rows={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}"),
    new Check(
        "neutral-custodial-and-higgs-source-gates-remain-blocked",
        neutralElectroweakMixingSourceAuditPassed
            && !lowEnergyHyperchargeSourcePresent
            && !neutralMixingRouteProvidesTargetIndependentWeakMixingAngle
            && !neutralMixingRouteCompletesBosonPredictions
            && custodialRhoParameterSourceAuditPassed
            && !rhoRelationProvidesAbsoluteWzScale
            && !rhoRelationProvidesWeakMixingAngleSource
            && !custodialRouteCompletesBosonPredictions
            && higgsUpsilonScalarSourceBoundaryAuditPassed
            && !officialGuSourcesProvideFixedScalarSourceOperator
            && !officialGuSourcesProvideMassiveScalarProfile
            && !officialGuSourcesProvideQuarticSelfCouplingValue
            && !higgsUpsilonRouteCompletesBosonPredictions,
        $"p321Passed={neutralElectroweakMixingSourceAuditPassed}; lowEnergyHypercharge={lowEnergyHyperchargeSourcePresent}; neutralWeakAngle={neutralMixingRouteProvidesTargetIndependentWeakMixingAngle}; p324Passed={custodialRhoParameterSourceAuditPassed}; rhoAbsoluteScale={rhoRelationProvidesAbsoluteWzScale}; rhoWeakAngle={rhoRelationProvidesWeakMixingAngleSource}; p322Passed={higgsUpsilonScalarSourceBoundaryAuditPassed}; scalarOperator={officialGuSourcesProvideFixedScalarSourceOperator}; quartic={officialGuSourcesProvideQuarticSelfCouplingValue}"),
    new Check(
        "minimal-unlock-and-parameter-closures-remain-blocked",
        electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !weakCouplingSourcePromotable
            && !canPromoteExternalElectroweakBridge
            && rankDeficitMinimalUnlockContractPassed
            && unlockRowCount == 2
            && !unlockContractFilled
            && newSourceEvidenceStillRequired,
        $"p224Passed={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; weakCouplingSource={weakCouplingSourcePromotable}; externalBridge={canPromoteExternalElectroweakBridge}; unlockRows={unlockRowCount}; unlockFilled={unlockContractFilled}; newSourceRequired={newSourceEvidenceStillRequired}"),
    new Check(
        "unitarity-route-does-not-fill-source-lineage-or-observed-extraction-contracts",
        !unitarityRoutePromotesWzMasses
            && !unitarityRoutePromotesHiggsMass
            && !unitarityRouteCompletesBosonPredictions
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
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"routePromotesWz={unitarityRoutePromotesWzMasses}; routePromotesHiggs={unitarityRoutePromotesHiggsMass}; routeCompletes={unitarityRouteCompletesBosonPredictions}; phase201AllLineages={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; p295Ready={phase295IntakeReadyObservedFieldExtractionCandidateCount}; p296Ready={phase296IntakeReadySourceLineageFieldCandidateCount}"),
};

var electroweakUnitarityScatteringSourceAuditPassed = checks.All(check => check.Passed)
    && unitarityRequirements.Count(requirement => requirement.Filled) == 2
    && unitarityRouteProvidesConsistencyBound
    && unitarityRouteProvidesUpperBoundOnly
    && !unitarityRouteProvidesExactHiggsMass
    && !unitarityRouteProvidesAbsoluteWzScale
    && !unitarityRouteProvidesWeakMixingAngleSource
    && !unitarityRouteProvidesTargetIndependentVevSource
    && !unitarityRouteProvidesGaugeCouplingNormalization
    && !unitarityRouteProvidesObservedFieldExtraction
    && !unitarityRouteProvidesHiggsScalarSelfCouplingSource
    && !unitarityRouteProvidesPhotonWzProjectionRows
    && !unitarityRoutePromotesWzMasses
    && !unitarityRoutePromotesHiggsMass
    && !unitarityRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = electroweakUnitarityScatteringSourceAuditPassed
    ? "electroweak-unitarity-scattering-source-audit-bound-not-source"
    : "electroweak-unitarity-scattering-source-audit-review-required";

var result = new
{
    phaseId = "phase325-electroweak-unitarity-scattering-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    electroweakUnitarityScatteringSourceAuditPassed,
    pdgHiggsUnitarityReferencePresent,
    leeQuiggThackerReferencePresent,
    longitudinalWzScatteringUnitarityLeadPresent,
    higgsRestoresPerturbativeUnitarityLeadPresent,
    unitarityRouteProvidesConsistencyBound,
    unitarityRouteProvidesUpperBoundOnly,
    unitarityRouteProvidesExactHiggsMass,
    unitarityRouteProvidesAbsoluteWzScale,
    unitarityRouteProvidesWeakMixingAngleSource,
    unitarityRouteProvidesTargetIndependentVevSource,
    unitarityRouteProvidesGaugeCouplingNormalization,
    unitarityRouteProvidesObservedFieldExtraction,
    unitarityRouteProvidesHiggsScalarSelfCouplingSource,
    unitarityRouteProvidesPhotonWzProjectionRows,
    unitarityRoutePromotesWzMasses,
    unitarityRoutePromotesHiggsMass,
    unitarityRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    externalSources = new[]
    {
        new ExternalSource(
            "pdg-2025-higgs-unitarity",
            "Particle Data Group, Status of Higgs Boson Physics, 2025 update",
            pdg2025HiggsReviewUrl,
            "Records that without the Higgs boson, perturbative unitarity would be lost in high-energy longitudinal W/Z scattering; the VEV is an external electroweak input in the SM review."),
        new ExternalSource(
            "lee-quigg-thacker-1977",
            "Lee, Quigg, and Thacker, Weak interactions at very high energies: The role of the Higgs-boson mass",
            leeQuiggThackerCernRecordUrl,
            "Classic longitudinal weak-boson scattering unitarity bound paper. It supplies a consistency bound on Higgs-sector behavior, not a GU-local exact mass/source-lineage artifact."),
    },
    unitarityRequirements,
    inheritedEvidence = new
    {
        phase224 = new
        {
            path = Phase224Path,
            electroweakParameterAuditPassed,
            wAbsoluteMassParameterClosure,
            zAbsoluteMassParameterClosure,
            higgsMassParameterClosure,
            weakCouplingSourcePromotable,
            canPromoteExternalElectroweakBridge,
        },
        phase244 = new
        {
            path = Phase244Path,
            electroweakIdentifiabilityRankAuditPassed,
            promotedWzRatioPresent,
            anyPromotedAbsoluteMassPresent,
            currentPromotedConstraintRank,
            remainingNullity,
            minimumAdditionalIndependentSourceConstraints,
        },
        phase245 = new
        {
            path = Phase245Path,
            rankDeficitMinimalUnlockContractPassed,
            unlockContractFilled,
            newSourceEvidenceStillRequired,
            unlockRowCount,
        },
        phase313 = new
        {
            path = Phase313Path,
            officialDraftElectroweakProjectionMapAuditPassed,
            officialDraftProvidesWeakIsospinLocation,
            officialDraftProvidesWeakHyperchargeLocation,
            officialDraftProvidesPhotonZWeinbergRotation,
            officialDraftProvidesWeakMixingAngleSource,
            officialDraftProvidesNeutralMassMatrixDiagonalization,
            officialDraftProvidesWChargedProjectionRows,
            officialDraftProvidesZSourceRowProjection,
            officialDraftProvidesObservedElectroweakGaugeEmbedding,
        },
        phase317 = new
        {
            path = Phase317Path,
            electroweakMassMatrixBridgeSourceAuditPassed,
            smMassGenerationRequiresHiggsDoublet,
            smMassGenerationRequiresVev,
            smMassGenerationRequiresWeakCouplingG,
            smMassGenerationRequiresHyperchargeCouplingGPrime,
            smTreeLevelHiggsMassDependsOnPotentialParameter,
            smMassMatrixProvidesExternalDependencyMap,
            smMassMatrixPromotesWzMasses,
            smMassMatrixPromotesHiggsMass,
        },
        phase320 = new
        {
            path = Phase320Path,
            standardElectroweakNormalizationBoundaryAuditPassed,
            standardZRequiresNeutralSu2U1Mixing,
            standardPhotonZRotationRequiresWeakMixingAngle,
            standardTreeLevelMassesRequireGgv,
            standardTreeLevelHiggsRequiresLambdaAndV,
            standardElectroweakBoundaryCompletesBosonPredictions,
        },
        phase321 = new
        {
            path = Phase321Path,
            neutralElectroweakMixingSourceAuditPassed,
            lowEnergyHyperchargeSourcePresent,
            neutralMixingRouteProvidesTargetIndependentWeakMixingAngle,
            neutralMixingRouteCompletesBosonPredictions,
        },
        phase322 = new
        {
            path = Phase322Path,
            higgsUpsilonScalarSourceBoundaryAuditPassed,
            officialGuSourcesProvideFixedScalarSourceOperator,
            officialGuSourcesProvideMassiveScalarProfile,
            officialGuSourcesProvideQuarticSelfCouplingValue,
            higgsUpsilonRouteCompletesBosonPredictions,
        },
        phase323 = new
        {
            path = Phase323Path,
            coupledYangMillsHiggsMassExtractionAuditPassed,
            officialPublicSourcesProvideTargetIndependentVevSource,
            officialPublicSourcesProvideGaugeFixedQuadraticExpansion,
            officialPublicSourcesProvidePhotonWzHiggsProjectionRows,
            coupledYangMillsHiggsRouteCompletesBosonPredictions,
        },
        phase324 = new
        {
            path = Phase324Path,
            custodialRhoParameterSourceAuditPassed,
            rhoRelationProvidesAbsoluteWzScale,
            rhoRelationProvidesWeakMixingAngleSource,
            custodialRouteCompletesBosonPredictions,
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
    decision = electroweakUnitarityScatteringSourceAuditPassed
        ? "Do not promote W/Z or Higgs absolute masses from perturbative unitarity or longitudinal W/Z scattering. The argument is a real electroweak consistency bound and supports the need for a Higgs/EWSB mechanism, but it does not derive an exact Higgs mass, absolute W/Z scale, weak-mixing angle, gauge-coupling normalization, target-independent VEV, observed photon/W/Z/H rows, or GU Higgs scalar-source/self-coupling lineage."
        : "Review the electroweak unitarity scattering audit before using this route as boson prediction evidence.",
    nextRequiredArtifact = new[]
    {
        "A GU-local electroweak scattering or quadratic-mass theorem that yields exact W/Z/H source rows, not only a unitarity upper bound.",
        "A target-independent electroweak VEV and weak/hypercharge coupling source if the unitarity argument is to be turned into masses.",
        "Observed photon/W/Z/H projection rows satisfying Phase256.",
        "A solved Higgs scalar-source/self-coupling lineage satisfying Phase201.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase244Path = Phase244Path,
        phase245Path = Phase245Path,
        phase256Path = Phase256Path,
        phase295Path = Phase295Path,
        phase296Path = Phase296Path,
        phase313Path = Phase313Path,
        phase317Path = Phase317Path,
        phase320Path = Phase320Path,
        phase321Path = Phase321Path,
        phase322Path = Phase322Path,
        phase323Path = Phase323Path,
        phase324Path = Phase324Path,
        pdg2025HiggsReviewUrl,
        leeQuiggThackerCernRecordUrl,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "electroweak_unitarity_scattering_source_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "electroweak_unitarity_scattering_source_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"electroweakUnitarityScatteringSourceAuditPassed={electroweakUnitarityScatteringSourceAuditPassed}");
Console.WriteLine($"longitudinalWzScatteringUnitarityLeadPresent={longitudinalWzScatteringUnitarityLeadPresent}");
Console.WriteLine($"unitarityRouteProvidesUpperBoundOnly={unitarityRouteProvidesUpperBoundOnly}");
Console.WriteLine($"unitarityRouteProvidesExactHiggsMass={unitarityRouteProvidesExactHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static IReadOnlyList<JsonElement> JsonArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
        ? value.EnumerateArray().ToArray()
        : Array.Empty<JsonElement>();

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

public sealed record Requirement(string RequirementId, string Detail, bool Filled);

public sealed record Check(string CheckId, bool Passed, string Detail);

public sealed record ExternalSource(string SourceId, string Title, string Url, string Finding);
