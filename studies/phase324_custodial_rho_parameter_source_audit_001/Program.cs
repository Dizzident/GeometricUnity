using System.Text.Json;

const string DefaultOutputDir = "studies/phase324_custodial_rho_parameter_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase244Path = "studies/phase244_electroweak_identifiability_rank_audit_001/output/electroweak_identifiability_rank_audit_summary.json";
const string Phase245Path = "studies/phase245_rank_deficit_minimal_unlock_contract_001/output/rank_deficit_minimal_unlock_contract_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase270Path = "studies/phase270_composite_higgs_pngb_source_audit_001/output/composite_higgs_pngb_source_audit_summary.json";
const string Phase295Path = "studies/phase295_observed_field_extraction_contract_candidate_scan_001/output/observed_field_extraction_contract_candidate_scan_summary.json";
const string Phase296Path = "studies/phase296_source_lineage_contract_field_candidate_scan_001/output/source_lineage_contract_field_candidate_scan_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase320Path = "studies/phase320_standard_electroweak_ladder_normalization_boundary_audit_001/output/standard_electroweak_ladder_normalization_boundary_audit_summary.json";
const string Phase321Path = "studies/phase321_neutral_electroweak_mixing_source_audit_001/output/neutral_electroweak_mixing_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE324_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase244 = JsonDocument.Parse(File.ReadAllText(Phase244Path));
using var phase245 = JsonDocument.Parse(File.ReadAllText(Phase245Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase270 = JsonDocument.Parse(File.ReadAllText(Phase270Path));
using var phase295 = JsonDocument.Parse(File.ReadAllText(Phase295Path));
using var phase296 = JsonDocument.Parse(File.ReadAllText(Phase296Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase320 = JsonDocument.Parse(File.ReadAllText(Phase320Path));
using var phase321 = JsonDocument.Parse(File.ReadAllText(Phase321Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));

const string pdg2025HiggsCustodialSymmetryUrl = "https://pdgweb.lbl.gov/2025/reviews/rpp2025-rev-higgs-boson.pdf";
const bool pdgHiggsCustodialSymmetryReferencePresent = true;
const bool standardCustodialRhoRelationLeadPresent = true;
const bool custodialSymmetryProtectsRhoRelationFromLargeCorrections = true;
const bool rhoRelationConstrainsMwMzCosTheta = true;
const bool rhoRelationEquivalentToTreeLevelMwEqualsMzCosTheta = true;
const bool rhoRelationProvidesAbsoluteWzScale = false;
const bool rhoRelationProvidesWeakMixingAngleSource = false;
const bool rhoRelationProvidesTargetIndependentVevSource = false;
const bool rhoRelationProvidesGaugeCouplingNormalization = false;
const bool rhoRelationProvidesObservedFieldExtraction = false;
const bool rhoRelationProvidesHiggsScalarSource = false;
const bool custodialRoutePromotesWzMasses = false;
const bool custodialRoutePromotesHiggsMass = false;
const bool custodialRouteCompletesBosonPredictions = false;
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
var rankAuditPromotableForBosonMasses = JsonBool(phase244.RootElement, "rankAuditPromotableForBosonMasses") is true;
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

var compositeHiggsPngbSourceAuditPassed = JsonBool(phase270.RootElement, "compositeHiggsPngbSourceAuditPassed") is true;
var custodialSymmetryLeadPresent = JsonBool(phase270.RootElement, "custodialSymmetryLeadPresent") is true;
var compositeHiggsPromotesWzMasses = JsonBool(phase270.RootElement, "compositeHiggsPromotesWzMasses") is true;
var compositeHiggsPromotesHiggsMass = JsonBool(phase270.RootElement, "compositeHiggsPromotesHiggsMass") is true;
var compositeHiggsCompletesBosonPredictions = JsonBool(phase270.RootElement, "compositeHiggsCompletesBosonPredictions") is true;
var phase270SourceBoundary = phase270.RootElement.GetProperty("sourceLineageBoundary");
var localGuCompositeVevSourceFound = JsonBool(phase270SourceBoundary, "localGuCompositeVevSourceFound") is true;
var localGuCompositeObservedFieldExtractionFound = JsonBool(phase270SourceBoundary, "localGuCompositeObservedFieldExtractionFound") is true;
var localGuCompositeWzMassMatrixSourceFound = JsonBool(phase270SourceBoundary, "localGuCompositeWzMassMatrixSourceFound") is true;
var localGuCompositeHiggsScalarSourceFound = JsonBool(phase270SourceBoundary, "localGuCompositeHiggsScalarSourceFound") is true;

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
var smDefinesPhotonZWeinbergRotation = JsonBool(phase317.RootElement, "smDefinesPhotonZWeinbergRotation") is true;
var smTreeLevelMwDependsOnGAndV = JsonBool(phase317.RootElement, "smTreeLevelMwDependsOnGAndV") is true;
var smTreeLevelMzDependsOnGAndGPrimeAndV = JsonBool(phase317.RootElement, "smTreeLevelMzDependsOnGAndGPrimeAndV") is true;
var smMassMatrixProvidesExternalDependencyMap = JsonBool(phase317.RootElement, "smMassMatrixProvidesExternalDependencyMap") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;

var standardElectroweakNormalizationBoundaryAuditPassed = JsonBool(phase320.RootElement, "standardElectroweakNormalizationBoundaryAuditPassed") is true;
var standardZRequiresNeutralSu2U1Mixing = JsonBool(phase320.RootElement, "standardZRequiresNeutralSu2U1Mixing") is true;
var standardPhotonZRotationRequiresWeakMixingAngle = JsonBool(phase320.RootElement, "standardPhotonZRotationRequiresWeakMixingAngle") is true;
var standardTreeLevelMassesRequireGgv = JsonBool(phase320.RootElement, "standardTreeLevelMassesRequireGgv") is true;
var standardElectroweakBoundaryPromotesWzMasses = JsonBool(phase320.RootElement, "standardElectroweakBoundaryPromotesWzMasses") is true;
var standardElectroweakBoundaryPromotesHiggsMass = JsonBool(phase320.RootElement, "standardElectroweakBoundaryPromotesHiggsMass") is true;
var standardElectroweakBoundaryCompletesBosonPredictions = JsonBool(phase320.RootElement, "standardElectroweakBoundaryCompletesBosonPredictions") is true;

var neutralElectroweakMixingSourceAuditPassed = JsonBool(phase321.RootElement, "neutralElectroweakMixingSourceAuditPassed") is true;
var lowEnergyHyperchargeSourcePresent = JsonBool(phase321.RootElement, "lowEnergyHyperchargeSourcePresent") is true;
var neutralMixingRouteProvidesTargetIndependentWeakMixingAngle = JsonBool(phase321.RootElement, "neutralMixingRouteProvidesTargetIndependentWeakMixingAngle") is true;
var neutralMixingRouteCompletesBosonPredictions = JsonBool(phase321.RootElement, "neutralMixingRouteCompletesBosonPredictions") is true;

var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialPublicSourcesProvideTargetIndependentVevSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideTargetIndependentVevSource") is true;
var officialPublicSourcesProvideGaugeCouplingNormalization = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeCouplingNormalization") is true;
var officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle = JsonBool(phase323.RootElement, "officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle") is true;
var officialPublicSourcesProvideGaugeFixedQuadraticExpansion = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeFixedQuadraticExpansion") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;

var custodialRequirements = new[]
{
    new Requirement("custodial-rho-standard-relation", "A standard custodial-symmetry rho relation rho=MW^2/(MZ^2 cos^2 thetaW)=1 is present in external physics references.", standardCustodialRhoRelationLeadPresent),
    new Requirement("repo-existing-wz-ratio-lane", "The repository already has a promoted W/Z ratio lane, equivalent to a rank-one electroweak constraint.", promotedWzRatioPresent && currentPromotedConstraintRank == 1),
    new Requirement("target-independent-wz-absolute-scale", "A target-independent GU source row for the common W/Z absolute scale.", rhoRelationProvidesAbsoluteWzScale),
    new Requirement("target-independent-weak-angle-source", "A target-independent GU weak-mixing angle or equivalent g/g-prime source.", rhoRelationProvidesWeakMixingAngleSource),
    new Requirement("target-independent-vev-source", "A target-independent GU electroweak VEV/source-selection rule.", rhoRelationProvidesTargetIndependentVevSource),
    new Requirement("observed-electroweak-field-extraction", "Observed photon/W/Z projection and field-extraction rows.", rhoRelationProvidesObservedFieldExtraction),
    new Requirement("higgs-scalar-source", "A solved Higgs scalar source/operator and self-coupling lineage.", rhoRelationProvidesHiggsScalarSource),
};

var checks = new[]
{
    new Check(
        "standard-custodial-rho-relation-recorded",
        pdgHiggsCustodialSymmetryReferencePresent
            && standardCustodialRhoRelationLeadPresent
            && custodialSymmetryProtectsRhoRelationFromLargeCorrections
            && rhoRelationConstrainsMwMzCosTheta
            && rhoRelationEquivalentToTreeLevelMwEqualsMzCosTheta
            && compositeHiggsPngbSourceAuditPassed
            && custodialSymmetryLeadPresent,
        $"pdgReference={pdgHiggsCustodialSymmetryReferencePresent}; rhoRelation={standardCustodialRhoRelationLeadPresent}; protectsRelation={custodialSymmetryProtectsRhoRelationFromLargeCorrections}; constrainsMwMzCosTheta={rhoRelationConstrainsMwMzCosTheta}; phase270CustodialLead={custodialSymmetryLeadPresent}"),
    new Check(
        "rho-relation-is-rank-one-ratio-not-absolute-scale",
        electroweakIdentifiabilityRankAuditPassed
            && promotedWzRatioPresent
            && currentPromotedConstraintRank == 1
            && remainingNullity == 2
            && minimumAdditionalIndependentSourceConstraints == 2
            && !anyPromotedAbsoluteMassPresent
            && !rankAuditPromotableForBosonMasses
            && !rhoRelationProvidesAbsoluteWzScale,
        $"rankAuditPassed={electroweakIdentifiabilityRankAuditPassed}; promotedWzRatio={promotedWzRatioPresent}; rank={currentPromotedConstraintRank}; nullity={remainingNullity}; minAdditionalSources={minimumAdditionalIndependentSourceConstraints}; anyAbsolute={anyPromotedAbsoluteMassPresent}; rhoAbsoluteScale={rhoRelationProvidesAbsoluteWzScale}"),
    new Check(
        "minimal-unlock-contract-still-requires-new-sources",
        rankDeficitMinimalUnlockContractPassed
            && unlockRowCount == 2
            && !unlockContractFilled
            && newSourceEvidenceStillRequired
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !weakCouplingSourcePromotable
            && !canPromoteExternalElectroweakBridge,
        $"unlockPassed={rankDeficitMinimalUnlockContractPassed}; unlockRows={unlockRowCount}; unlockFilled={unlockContractFilled}; newSourceRequired={newSourceEvidenceStillRequired}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; weakCouplingSource={weakCouplingSourcePromotable}; externalBridge={canPromoteExternalElectroweakBridge}"),
    new Check(
        "custodial-route-does-not-supply-mixing-vev-or-observed-field-extraction",
        !rhoRelationProvidesWeakMixingAngleSource
            && !rhoRelationProvidesTargetIndependentVevSource
            && !rhoRelationProvidesGaugeCouplingNormalization
            && !rhoRelationProvidesObservedFieldExtraction
            && officialDraftElectroweakProjectionMapAuditPassed
            && officialDraftProvidesWeakIsospinLocation
            && officialDraftProvidesWeakHyperchargeLocation
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesNeutralMassMatrixDiagonalization
            && !officialDraftProvidesWChargedProjectionRows
            && !officialDraftProvidesZSourceRowProjection
            && !officialDraftProvidesObservedElectroweakGaugeEmbedding
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"weakAngleSource={rhoRelationProvidesWeakMixingAngleSource}; vevSource={rhoRelationProvidesTargetIndependentVevSource}; gaugeNormalization={rhoRelationProvidesGaugeCouplingNormalization}; observedExtraction={rhoRelationProvidesObservedFieldExtraction}; p313PhotonZ={officialDraftProvidesPhotonZWeinbergRotation}; p313WeakAngle={officialDraftProvidesWeakMixingAngleSource}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}"),
    new Check(
        "standard-electroweak-and-sm-matrix-remain-dependency-maps",
        electroweakMassMatrixBridgeSourceAuditPassed
            && smDefinesPhotonZWeinbergRotation
            && smTreeLevelMwDependsOnGAndV
            && smTreeLevelMzDependsOnGAndGPrimeAndV
            && smMassMatrixProvidesExternalDependencyMap
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && standardElectroweakNormalizationBoundaryAuditPassed
            && standardZRequiresNeutralSu2U1Mixing
            && standardPhotonZRotationRequiresWeakMixingAngle
            && standardTreeLevelMassesRequireGgv
            && !standardElectroweakBoundaryPromotesWzMasses
            && !standardElectroweakBoundaryPromotesHiggsMass
            && !standardElectroweakBoundaryCompletesBosonPredictions,
        $"p317Passed={electroweakMassMatrixBridgeSourceAuditPassed}; smExternalMap={smMassMatrixProvidesExternalDependencyMap}; smPromotesWz={smMassMatrixPromotesWzMasses}; p320Passed={standardElectroweakNormalizationBoundaryAuditPassed}; zRequiresMixing={standardZRequiresNeutralSu2U1Mixing}; weakAngleRequired={standardPhotonZRotationRequiresWeakMixingAngle}; ggvRequired={standardTreeLevelMassesRequireGgv}; p320PromotesWz={standardElectroweakBoundaryPromotesWzMasses}"),
    new Check(
        "neutral-mixing-and-coupled-gates-remain-blocked",
        neutralElectroweakMixingSourceAuditPassed
            && !lowEnergyHyperchargeSourcePresent
            && !neutralMixingRouteProvidesTargetIndependentWeakMixingAngle
            && !neutralMixingRouteCompletesBosonPredictions
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeCouplingNormalization
            && !officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle
            && !officialPublicSourcesProvideGaugeFixedQuadraticExpansion
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions,
        $"p321Passed={neutralElectroweakMixingSourceAuditPassed}; lowEnergyHypercharge={lowEnergyHyperchargeSourcePresent}; neutralWeakAngle={neutralMixingRouteProvidesTargetIndependentWeakMixingAngle}; neutralCompletes={neutralMixingRouteCompletesBosonPredictions}; p323Passed={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Vev={officialPublicSourcesProvideTargetIndependentVevSource}; p323ProjectionRows={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}"),
    new Check(
        "custodial-route-does-not-fill-source-lineage-contracts",
        !custodialRoutePromotesWzMasses
            && !custodialRoutePromotesHiggsMass
            && !custodialRouteCompletesBosonPredictions
            && !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && phase295IntakeReadyObservedFieldExtractionCandidateCount == 0
            && !phase295AnyObservedFieldExtractionCandidateFillsContract
            && phase296IntakeReadySourceLineageFieldCandidateCount == 0
            && !phase296AnySourceLineageCandidateFillsContract
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"routePromotesWz={custodialRoutePromotesWzMasses}; routePromotesHiggs={custodialRoutePromotesHiggsMass}; routeCompletes={custodialRouteCompletesBosonPredictions}; phase201AllLineages={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; p295Ready={phase295IntakeReadyObservedFieldExtractionCandidateCount}; p296Ready={phase296IntakeReadySourceLineageFieldCandidateCount}"),
    new Check(
        "composite-custodial-lead-remains-nonpromotional",
        compositeHiggsPngbSourceAuditPassed
            && custodialSymmetryLeadPresent
            && !compositeHiggsPromotesWzMasses
            && !compositeHiggsPromotesHiggsMass
            && !compositeHiggsCompletesBosonPredictions
            && !localGuCompositeVevSourceFound
            && !localGuCompositeObservedFieldExtractionFound
            && !localGuCompositeWzMassMatrixSourceFound
            && !localGuCompositeHiggsScalarSourceFound,
        $"p270Passed={compositeHiggsPngbSourceAuditPassed}; custodialLead={custodialSymmetryLeadPresent}; compositePromotesWz={compositeHiggsPromotesWzMasses}; compositePromotesHiggs={compositeHiggsPromotesHiggsMass}; localVev={localGuCompositeVevSourceFound}; localObserved={localGuCompositeObservedFieldExtractionFound}; localWzMatrix={localGuCompositeWzMassMatrixSourceFound}; localHiggsScalar={localGuCompositeHiggsScalarSourceFound}"),
};

var custodialRhoParameterSourceAuditPassed = checks.All(check => check.Passed)
    && custodialRequirements.Count(requirement => requirement.Filled) == 2
    && rhoRelationConstrainsMwMzCosTheta
    && !rhoRelationProvidesAbsoluteWzScale
    && !rhoRelationProvidesWeakMixingAngleSource
    && !rhoRelationProvidesTargetIndependentVevSource
    && !rhoRelationProvidesGaugeCouplingNormalization
    && !rhoRelationProvidesObservedFieldExtraction
    && !rhoRelationProvidesHiggsScalarSource
    && !custodialRoutePromotesWzMasses
    && !custodialRoutePromotesHiggsMass
    && !custodialRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = custodialRhoParameterSourceAuditPassed
    ? "custodial-rho-parameter-source-audit-ratio-constraint-not-absolute-source"
    : "custodial-rho-parameter-source-audit-review-required";

var result = new
{
    phaseId = "phase324-custodial-rho-parameter-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    custodialRhoParameterSourceAuditPassed,
    pdgHiggsCustodialSymmetryReferencePresent,
    standardCustodialRhoRelationLeadPresent,
    custodialSymmetryProtectsRhoRelationFromLargeCorrections,
    rhoRelationConstrainsMwMzCosTheta,
    rhoRelationEquivalentToTreeLevelMwEqualsMzCosTheta,
    rhoRelationProvidesAbsoluteWzScale,
    rhoRelationProvidesWeakMixingAngleSource,
    rhoRelationProvidesTargetIndependentVevSource,
    rhoRelationProvidesGaugeCouplingNormalization,
    rhoRelationProvidesObservedFieldExtraction,
    rhoRelationProvidesHiggsScalarSource,
    custodialRoutePromotesWzMasses,
    custodialRoutePromotesHiggsMass,
    custodialRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    externalSources = new[]
    {
        new ExternalSource(
            "pdg-2025-higgs-custodial-symmetry",
            "Particle Data Group, Status of Higgs Boson Physics, Standard Model custodial symmetry section",
            pdg2025HiggsCustodialSymmetryUrl,
            "Records the custodial-symmetry rho relation as a tree-level W/Z ratio constraint protected from large radiative corrections, not as an absolute W/Z mass-scale source."),
    },
    custodialRequirements,
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
            rankAuditPromotableForBosonMasses,
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
        phase270 = new
        {
            path = Phase270Path,
            compositeHiggsPngbSourceAuditPassed,
            custodialSymmetryLeadPresent,
            compositeHiggsPromotesWzMasses,
            compositeHiggsPromotesHiggsMass,
            compositeHiggsCompletesBosonPredictions,
            localGuCompositeVevSourceFound,
            localGuCompositeObservedFieldExtractionFound,
            localGuCompositeWzMassMatrixSourceFound,
            localGuCompositeHiggsScalarSourceFound,
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
            smDefinesPhotonZWeinbergRotation,
            smTreeLevelMwDependsOnGAndV,
            smTreeLevelMzDependsOnGAndGPrimeAndV,
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
            standardElectroweakBoundaryPromotesWzMasses,
            standardElectroweakBoundaryPromotesHiggsMass,
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
        phase323 = new
        {
            path = Phase323Path,
            coupledYangMillsHiggsMassExtractionAuditPassed,
            officialPublicSourcesProvideTargetIndependentVevSource,
            officialPublicSourcesProvideGaugeCouplingNormalization,
            officialPublicSourcesProvideHyperchargeCouplingOrWeakAngle,
            officialPublicSourcesProvideGaugeFixedQuadraticExpansion,
            officialPublicSourcesProvidePhotonWzHiggsProjectionRows,
            coupledYangMillsHiggsRouteCompletesBosonPredictions,
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
    decision = custodialRhoParameterSourceAuditPassed
        ? "Do not promote W/Z or Higgs absolute masses from the custodial rho-parameter relation. The relation is valid external physics and protects a W/Z ratio constraint, but it does not derive the absolute W/Z scale, weak mixing angle, electroweak VEV, gauge-coupling normalization, observed photon/W/Z rows, or Higgs scalar source required by the Phase201 and Phase256 contracts."
        : "Review the custodial rho-parameter source audit before using this route as boson prediction evidence.",
    nextRequiredArtifact = new[]
    {
        "A target-independent GU source row fixing the common W/Z absolute scale, or equivalent independent GU rows for v and g.",
        "A GU-local weak-mixing or g-prime/g source plus photon/Z projection rows if the custodial relation is to be used for Z.",
        "A filled observed electroweak field-extraction artifact satisfying Phase256.",
        "A solved GU Higgs scalar-source/self-coupling lineage for Higgs mass completion.",
    },
    sourceEvidence = new
    {
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase244Path = Phase244Path,
        phase245Path = Phase245Path,
        phase256Path = Phase256Path,
        phase270Path = Phase270Path,
        phase295Path = Phase295Path,
        phase296Path = Phase296Path,
        phase313Path = Phase313Path,
        phase317Path = Phase317Path,
        phase320Path = Phase320Path,
        phase321Path = Phase321Path,
        phase323Path = Phase323Path,
        pdg2025HiggsCustodialSymmetryUrl,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "custodial_rho_parameter_source_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "custodial_rho_parameter_source_audit_summary.json"),
    JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"custodialRhoParameterSourceAuditPassed={custodialRhoParameterSourceAuditPassed}");
Console.WriteLine($"rhoRelationConstrainsMwMzCosTheta={rhoRelationConstrainsMwMzCosTheta}");
Console.WriteLine($"rhoRelationProvidesAbsoluteWzScale={rhoRelationProvidesAbsoluteWzScale}");
Console.WriteLine($"rhoRelationProvidesWeakMixingAngleSource={rhoRelationProvidesWeakMixingAngleSource}");
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
