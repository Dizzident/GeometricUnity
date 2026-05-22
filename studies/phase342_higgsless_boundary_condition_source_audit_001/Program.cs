using System.Text.Json;

const string DefaultOutputDir = "studies/phase342_higgsless_boundary_condition_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase265Path = "studies/phase265_gauge_higgs_boundary_source_audit_001/output/gauge_higgs_boundary_source_audit_summary.json";
const string Phase279Path = "studies/phase279_technicolor_walking_electroweak_scale_source_audit_001/output/technicolor_walking_electroweak_scale_source_audit_summary.json";
const string Phase325Path = "studies/phase325_electroweak_unitarity_scattering_source_audit_001/output/electroweak_unitarity_scattering_source_audit_summary.json";
const string Phase327Path = "studies/phase327_oblique_precision_electroweak_source_audit_001/output/oblique_precision_electroweak_source_audit_summary.json";
const string Phase333Path = "studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/output/kaluza_klein_internal_symmetry_source_audit_summary.json";
const string Phase341Path = "studies/phase341_scherk_schwarz_twisted_compactification_source_audit_001/output/scherk_schwarz_twisted_compactification_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE342_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase265 = JsonDocument.Parse(File.ReadAllText(Phase265Path));
using var phase279 = JsonDocument.Parse(File.ReadAllText(Phase279Path));
using var phase325 = JsonDocument.Parse(File.ReadAllText(Phase325Path));
using var phase327 = JsonDocument.Parse(File.ReadAllText(Phase327Path));
using var phase333 = JsonDocument.Parse(File.ReadAllText(Phase333Path));
using var phase341 = JsonDocument.Parse(File.ReadAllText(Phase341Path));

const string realisticWarpedHiggslessUrl = "https://arxiv.org/abs/hep-ph/0308038";
const string nomuraWarpedHiggslessUrl = "https://arxiv.org/abs/hep-ph/0309189";
const string theorySpaceHiggslessUrl = "https://arxiv.org/abs/hep-ph/0312324";
const string warpedConstraintsUrl = "https://arxiv.org/abs/hep-ph/0312193";
const string sixDHiggslessUrl = "https://arxiv.org/abs/hep-ph/0406020";
const string higgslessSumRulesUrl = "https://arxiv.org/abs/0808.1682";

const bool higgslessBoundaryConditionSourceAuditPassedExpected = true;
const bool higgslessBoundaryLeadPresent = true;
const bool higgslessPrimarySourcesReviewed = true;
const bool higgslessRouteExternalToGu = true;
const bool warpedHiggslessBoundaryConditionLeadPresent = true;
const bool warpedRouteBreaksElectroweakSymmetryByBoundaryConditions = true;
const bool warpedRouteUsesBulkSu2lSu2rU1BLGaugeGroup = true;
const bool warpedRoutePredictsNoFundamentalOrCompositeHiggs = true;
const bool warpedRouteHasObservedHiggsConflict = true;
const bool warpedRouteHasTevVectorResonancePrediction = true;
const bool nomuraWarpedDualStrongDynamicsLeadPresent = true;
const bool nomuraRouteIntroducesBulkGaugeCouplingRatioParameter = true;
const bool theorySpaceHiggslessLeadPresent = true;
const bool theorySpaceRouteCanFitWzMassesByVaryingCouplings = true;
const bool theorySpaceLightestKkStatesIdentifiedAsWz = true;
const bool warpedPhenomenologyPrecisionConstraintLeadPresent = true;
const bool warpedPhenomenologyFindsPerturbativeUnitarityViolation = true;
const bool sixDHiggslessLeadPresent = true;
const bool sixDRouteArrangesWzSplittingByCompactificationScales = true;
const bool sixDRouteUsesElectroweakScaleBulkKineticCouplings = true;
const bool unitaritySumRulesLeadPresent = true;
const bool unitarityRouteRequiresKkTowerForConsistency = true;
const bool precisionConstraintBoundaryPresent = true;
const bool routeOverlapsScherkSchwarzBoundaryCondition = true;
const bool routeOverlapsKaluzaKleinInternalSymmetry = true;
const bool routeOverlapsTechnicolorCompositeDual = true;
const bool routeDistinctFromGaugeHiggsWilsonLine = true;

const bool higgslessRouteRequiresGuLocalIntervalOrBoundaryGeometry = true;
const bool higgslessRouteRequiresTargetIndependentBoundaryConditions = true;
const bool higgslessRouteRequiresCompactificationOrWarpScaleSource = true;
const bool higgslessRouteRequiresBulkGaugeCouplingLineage = true;
const bool higgslessRouteRequiresElectroweakEmbeddingAndNeutralProjection = true;
const bool higgslessRouteRequiresObservedPhotonWzProjection = true;
const bool higgslessRouteRequiresObservedHiggsCompatibilityOrReplacement = true;
const bool higgslessRouteRequiresKkTowerAndUnitarityCompletion = true;
const bool higgslessRouteRequiresPrecisionElectroweakAndRgLineage = true;
const bool higgslessRouteRequiresGeVUnitNormalization = true;

const bool higgslessRouteProvidesGuLocalWzTheorem = false;
const bool higgslessRouteProvidesSeparateWzSourceRows = false;
const bool higgslessRouteProvidesTargetIndependentBoundaryConditionSource = false;
const bool higgslessRouteProvidesTargetIndependentCompactificationOrWarpScaleSource = false;
const bool higgslessRouteProvidesGuWeakMixingAngleSource = false;
const bool higgslessRouteProvidesGuGaugeCouplingNormalization = false;
const bool higgslessRouteProvidesObservedPhotonWzProjectionRows = false;
const bool higgslessRouteProvidesGuObservedFieldExtraction = false;
const bool higgslessRouteProvidesGuHiggsScalarSourceOperator = false;
const bool higgslessRouteProvidesObservedHiggsMassFromGu = false;
const bool higgslessRouteProvidesGeVUnitNormalization = false;
const bool higgslessRoutePromotesWzMasses = false;
const bool higgslessRoutePromotesHiggsMass = false;
const bool higgslessRouteCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var wAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224.RootElement, "higgsMassParameterClosure") is true;
var gaugeHiggsBoundarySourceAuditPassed = JsonBool(phase265.RootElement, "gaugeHiggsBoundarySourceAuditPassed") is true;
var gaugeHiggsBoundaryCompletesBosonPredictions = JsonBool(phase265.RootElement, "gaugeHiggsBoundaryCompletesBosonPredictions") is true;
var technicolorWalkingElectroweakScaleSourceAuditPassed = JsonBool(phase279.RootElement, "technicolorWalkingElectroweakScaleSourceAuditPassed") is true;
var technicolorCompletesBosonPredictions = JsonBool(phase279.RootElement, "technicolorCompletesBosonPredictions") is true;
var electroweakUnitarityScatteringSourceAuditPassed = JsonBool(phase325.RootElement, "electroweakUnitarityScatteringSourceAuditPassed") is true;
var unitarityRouteProvidesUpperBoundOnly = JsonBool(phase325.RootElement, "unitarityRouteProvidesUpperBoundOnly") is true;
var unitarityRouteCompletesBosonPredictions = JsonBool(phase325.RootElement, "unitarityRouteCompletesBosonPredictions") is true;
var obliquePrecisionElectroweakSourceAuditPassed = JsonBool(phase327.RootElement, "obliquePrecisionElectroweakSourceAuditPassed") is true;
var obliqueFitUsesPrecisionWzData = JsonBool(phase327.RootElement, "obliqueFitUsesPrecisionWzData") is true;
var obliqueRouteCompletesBosonPredictions = JsonBool(phase327.RootElement, "obliqueRouteCompletesBosonPredictions") is true;
var kaluzaKleinInternalSymmetrySourceAuditPassed = JsonBool(phase333.RootElement, "kaluzaKleinInternalSymmetrySourceAuditPassed") is true;
var kkRouteCompletesBosonPredictions = JsonBool(phase333.RootElement, "kkRouteCompletesBosonPredictions") is true;
var scherkSchwarzTwistedCompactificationSourceAuditPassed = JsonBool(phase341.RootElement, "scherkSchwarzTwistedCompactificationSourceAuditPassed") is true;
var scherkRouteCompletesBosonPredictions = JsonBool(phase341.RootElement, "scherkRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-hep-ph-0308038-realistic-warped-higgsless-ewsb",
        realisticWarpedHiggslessUrl,
        "Warped 5D Higgsless boundary-condition electroweak breaking",
        "Uses a bulk SU(2)L x SU(2)R x U(1)B-L gauge group in warped space, with electroweak symmetry broken by boundary conditions and no fundamental or composite Higgs particles.",
        "Direct W/Z mass-generation lead; still parameterized by extra-dimensional geometry, boundary choices, bulk couplings, and no observed Higgs."),
    new SourceRow(
        "arxiv-hep-ph-0309189-nomura-warped-higgsless",
        nomuraWarpedHiggslessUrl,
        "Warped Higgsless dual strong-dynamics model",
        "Frames the 4D dual as the Standard Model without a Higgs plus a strong TeV-scale gauge sector, and introduces a bulk gauge-coupling ratio parameter.",
        "Relevant to W/Z mass generation, but the scale and couplings are external model data."),
    new SourceRow(
        "arxiv-hep-ph-0312324-higgsless-theory-space",
        theorySpaceHiggslessUrl,
        "Theory-space/deconstructed Higgsless electroweak breaking",
        "Breaks U(1) x [SU(2)]^N x SU(2) to electromagnetism through nonlinear link fields; W/Z masses can be fitted by varying endpoint couplings.",
        "Shows boundary/deconstruction mass spectra can identify lightest KK states as W/Z; not a target-independent GU source law."),
    new SourceRow(
        "arxiv-hep-ph-0312193-warped-higgsless-constraints",
        warpedConstraintsUrl,
        "Warped Higgsless precision and unitarity constraints",
        "Studies boundary-condition EWSB in warped backgrounds and finds the parameter range can face perturbative unitarity problems without additional completion.",
        "Supplies a consistency boundary rather than source-lineage rows."),
    new SourceRow(
        "arxiv-hep-ph-0406020-six-dimensional-higgsless-standard-model",
        sixDHiggslessUrl,
        "6D Higgsless Standard Model",
        "Uses flat extra dimensions on a rectangle; W/Z splitting can be arranged by compactification scales and electroweak-scale bulk kinetic couplings.",
        "Direct boundary-condition W/Z lead, but the mass splitting remains arranged by model scales/couplings."),
    new SourceRow(
        "arxiv-0808.1682-higgsless-ww-scattering-sum-rules",
        higgslessSumRulesUrl,
        "Higgsless WW scattering sum rules",
        "Derives sum rules for deconstructed and continuum Higgsless models, showing KK towers are part of the unitarity/completion story.",
        "Useful consistency check; not an observed W/Z/H source law.")
};

var checks = new[]
{
    new Check(
        "higgsless-boundary-primary-sources-reviewed",
        higgslessBoundaryLeadPresent && higgslessPrimarySourcesReviewed && higgslessRouteExternalToGu && sourceRows.Length == 6,
        $"lead={higgslessBoundaryLeadPresent}; reviewed={higgslessPrimarySourcesReviewed}; externalToGu={higgslessRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "boundary-condition-wz-mass-lead-captured",
        warpedHiggslessBoundaryConditionLeadPresent
            && warpedRouteBreaksElectroweakSymmetryByBoundaryConditions
            && warpedRouteUsesBulkSu2lSu2rU1BLGaugeGroup
            && warpedRouteHasTevVectorResonancePrediction
            && theorySpaceLightestKkStatesIdentifiedAsWz
            && sixDRouteArrangesWzSplittingByCompactificationScales,
        $"warpedLead={warpedHiggslessBoundaryConditionLeadPresent}; boundaryBreaking={warpedRouteBreaksElectroweakSymmetryByBoundaryConditions}; bulkGroup={warpedRouteUsesBulkSu2lSu2rU1BLGaugeGroup}; tevResonances={warpedRouteHasTevVectorResonancePrediction}; theorySpaceWz={theorySpaceLightestKkStatesIdentifiedAsWz}; sixDWzSplitting={sixDRouteArrangesWzSplittingByCompactificationScales}"),
    new Check(
        "route-is-parameterized-and-higgsless",
        warpedRoutePredictsNoFundamentalOrCompositeHiggs
            && warpedRouteHasObservedHiggsConflict
            && nomuraWarpedDualStrongDynamicsLeadPresent
            && nomuraRouteIntroducesBulkGaugeCouplingRatioParameter
            && theorySpaceRouteCanFitWzMassesByVaryingCouplings
            && sixDRouteUsesElectroweakScaleBulkKineticCouplings,
        $"noHiggs={warpedRoutePredictsNoFundamentalOrCompositeHiggs}; observedHiggsConflict={warpedRouteHasObservedHiggsConflict}; strongDual={nomuraWarpedDualStrongDynamicsLeadPresent}; couplingRatio={nomuraRouteIntroducesBulkGaugeCouplingRatioParameter}; fitWzCouplings={theorySpaceRouteCanFitWzMassesByVaryingCouplings}; sixDBulkKineticScale={sixDRouteUsesElectroweakScaleBulkKineticCouplings}"),
    new Check(
        "unitarity-and-precision-boundaries-preserved",
        warpedPhenomenologyPrecisionConstraintLeadPresent
            && warpedPhenomenologyFindsPerturbativeUnitarityViolation
            && unitaritySumRulesLeadPresent
            && unitarityRouteRequiresKkTowerForConsistency
            && precisionConstraintBoundaryPresent
            && electroweakUnitarityScatteringSourceAuditPassed
            && unitarityRouteProvidesUpperBoundOnly
            && !unitarityRouteCompletesBosonPredictions
            && obliquePrecisionElectroweakSourceAuditPassed
            && obliqueFitUsesPrecisionWzData
            && !obliqueRouteCompletesBosonPredictions,
        $"precisionLead={warpedPhenomenologyPrecisionConstraintLeadPresent}; unitarityViolation={warpedPhenomenologyFindsPerturbativeUnitarityViolation}; sumRules={unitaritySumRulesLeadPresent}; kkTower={unitarityRouteRequiresKkTowerForConsistency}; phase325={electroweakUnitarityScatteringSourceAuditPassed}; upperBoundOnly={unitarityRouteProvidesUpperBoundOnly}; phase327={obliquePrecisionElectroweakSourceAuditPassed}; obliqueFitUsesWz={obliqueFitUsesPrecisionWzData}"),
    new Check(
        "related-routes-preserved",
        routeOverlapsScherkSchwarzBoundaryCondition
            && scherkSchwarzTwistedCompactificationSourceAuditPassed
            && !scherkRouteCompletesBosonPredictions
            && routeOverlapsKaluzaKleinInternalSymmetry
            && kaluzaKleinInternalSymmetrySourceAuditPassed
            && !kkRouteCompletesBosonPredictions
            && routeOverlapsTechnicolorCompositeDual
            && technicolorWalkingElectroweakScaleSourceAuditPassed
            && !technicolorCompletesBosonPredictions
            && routeDistinctFromGaugeHiggsWilsonLine
            && gaugeHiggsBoundarySourceAuditPassed
            && !gaugeHiggsBoundaryCompletesBosonPredictions,
        $"overlapsScherk={routeOverlapsScherkSchwarzBoundaryCondition}; phase341={scherkSchwarzTwistedCompactificationSourceAuditPassed}; overlapsKk={routeOverlapsKaluzaKleinInternalSymmetry}; phase333={kaluzaKleinInternalSymmetrySourceAuditPassed}; overlapsTechnicolor={routeOverlapsTechnicolorCompositeDual}; phase279={technicolorWalkingElectroweakScaleSourceAuditPassed}; distinctGaugeHiggs={routeDistinctFromGaugeHiggsWilsonLine}; phase265={gaugeHiggsBoundarySourceAuditPassed}"),
    new Check(
        "higgsless-route-requires-missing-gu-source-data",
        higgslessRouteRequiresGuLocalIntervalOrBoundaryGeometry
            && higgslessRouteRequiresTargetIndependentBoundaryConditions
            && higgslessRouteRequiresCompactificationOrWarpScaleSource
            && higgslessRouteRequiresBulkGaugeCouplingLineage
            && higgslessRouteRequiresElectroweakEmbeddingAndNeutralProjection
            && higgslessRouteRequiresObservedPhotonWzProjection
            && higgslessRouteRequiresObservedHiggsCompatibilityOrReplacement
            && higgslessRouteRequiresKkTowerAndUnitarityCompletion
            && higgslessRouteRequiresPrecisionElectroweakAndRgLineage
            && higgslessRouteRequiresGeVUnitNormalization,
        $"boundaryGeometry={higgslessRouteRequiresGuLocalIntervalOrBoundaryGeometry}; boundarySource={higgslessRouteRequiresTargetIndependentBoundaryConditions}; scale={higgslessRouteRequiresCompactificationOrWarpScaleSource}; couplings={higgslessRouteRequiresBulkGaugeCouplingLineage}; embedding={higgslessRouteRequiresElectroweakEmbeddingAndNeutralProjection}; projection={higgslessRouteRequiresObservedPhotonWzProjection}; higgs={higgslessRouteRequiresObservedHiggsCompatibilityOrReplacement}; kkUnitarity={higgslessRouteRequiresKkTowerAndUnitarityCompletion}; precisionRg={higgslessRouteRequiresPrecisionElectroweakAndRgLineage}; gev={higgslessRouteRequiresGeVUnitNormalization}"),
    new Check(
        "higgsless-route-does-not-fill-gu-contracts",
        !higgslessRouteProvidesGuLocalWzTheorem
            && !higgslessRouteProvidesSeparateWzSourceRows
            && !higgslessRouteProvidesTargetIndependentBoundaryConditionSource
            && !higgslessRouteProvidesTargetIndependentCompactificationOrWarpScaleSource
            && !higgslessRouteProvidesGuWeakMixingAngleSource
            && !higgslessRouteProvidesGuGaugeCouplingNormalization
            && !higgslessRouteProvidesObservedPhotonWzProjectionRows
            && !higgslessRouteProvidesGuObservedFieldExtraction
            && !higgslessRouteProvidesGuHiggsScalarSourceOperator
            && !higgslessRouteProvidesObservedHiggsMassFromGu
            && !higgslessRouteProvidesGeVUnitNormalization
            && !higgslessRoutePromotesWzMasses
            && !higgslessRoutePromotesHiggsMass
            && !higgslessRouteCompletesBosonPredictions,
        $"guWzTheorem={higgslessRouteProvidesGuLocalWzTheorem}; separateRows={higgslessRouteProvidesSeparateWzSourceRows}; boundarySource={higgslessRouteProvidesTargetIndependentBoundaryConditionSource}; scaleSource={higgslessRouteProvidesTargetIndependentCompactificationOrWarpScaleSource}; weakAngle={higgslessRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={higgslessRouteProvidesGuGaugeCouplingNormalization}; projection={higgslessRouteProvidesObservedPhotonWzProjectionRows}; observedExtraction={higgslessRouteProvidesGuObservedFieldExtraction}; higgsOperator={higgslessRouteProvidesGuHiggsScalarSourceOperator}; observedHiggs={higgslessRouteProvidesObservedHiggsMassFromGu}; gev={higgslessRouteProvidesGeVUnitNormalization}; promotesWz={higgslessRoutePromotesWzMasses}; promotesHiggs={higgslessRoutePromotesHiggsMass}"),
    new Check(
        "phase201-phase256-contract-state-unchanged",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; p224={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
};

var higgslessBoundaryConditionSourceAuditPassed = checks.All(check => check.Passed)
    && higgslessBoundaryConditionSourceAuditPassedExpected
    && !higgslessRouteProvidesGuLocalWzTheorem
    && !higgslessRouteProvidesSeparateWzSourceRows
    && !higgslessRouteProvidesTargetIndependentBoundaryConditionSource
    && !higgslessRouteProvidesTargetIndependentCompactificationOrWarpScaleSource
    && !higgslessRouteProvidesGuWeakMixingAngleSource
    && !higgslessRouteProvidesGuGaugeCouplingNormalization
    && !higgslessRouteProvidesGuObservedFieldExtraction
    && !higgslessRouteProvidesGuHiggsScalarSourceOperator
    && !higgslessRouteProvidesObservedHiggsMassFromGu
    && !higgslessRouteProvidesGeVUnitNormalization
    && !higgslessRoutePromotesWzMasses
    && !higgslessRoutePromotesHiggsMass
    && !higgslessRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = higgslessBoundaryConditionSourceAuditPassed
    ? "higgsless-boundary-condition-source-audit-external-boundary-mass-lead-not-gu-source"
    : "higgsless-boundary-condition-source-audit-review-required";

var result = new
{
    phaseId = "phase342-higgsless-boundary-condition-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    higgslessBoundaryConditionSourceAuditPassed,
    higgslessBoundaryLeadPresent,
    higgslessPrimarySourcesReviewed,
    higgslessRouteExternalToGu,
    warpedHiggslessBoundaryConditionLeadPresent,
    warpedRouteBreaksElectroweakSymmetryByBoundaryConditions,
    warpedRouteUsesBulkSu2lSu2rU1BLGaugeGroup,
    warpedRoutePredictsNoFundamentalOrCompositeHiggs,
    warpedRouteHasObservedHiggsConflict,
    warpedRouteHasTevVectorResonancePrediction,
    nomuraWarpedDualStrongDynamicsLeadPresent,
    nomuraRouteIntroducesBulkGaugeCouplingRatioParameter,
    theorySpaceHiggslessLeadPresent,
    theorySpaceRouteCanFitWzMassesByVaryingCouplings,
    theorySpaceLightestKkStatesIdentifiedAsWz,
    warpedPhenomenologyPrecisionConstraintLeadPresent,
    warpedPhenomenologyFindsPerturbativeUnitarityViolation,
    sixDHiggslessLeadPresent,
    sixDRouteArrangesWzSplittingByCompactificationScales,
    sixDRouteUsesElectroweakScaleBulkKineticCouplings,
    unitaritySumRulesLeadPresent,
    unitarityRouteRequiresKkTowerForConsistency,
    precisionConstraintBoundaryPresent,
    routeOverlapsScherkSchwarzBoundaryCondition,
    routeOverlapsKaluzaKleinInternalSymmetry,
    routeOverlapsTechnicolorCompositeDual,
    routeDistinctFromGaugeHiggsWilsonLine,
    higgslessRouteRequiresGuLocalIntervalOrBoundaryGeometry,
    higgslessRouteRequiresTargetIndependentBoundaryConditions,
    higgslessRouteRequiresCompactificationOrWarpScaleSource,
    higgslessRouteRequiresBulkGaugeCouplingLineage,
    higgslessRouteRequiresElectroweakEmbeddingAndNeutralProjection,
    higgslessRouteRequiresObservedPhotonWzProjection,
    higgslessRouteRequiresObservedHiggsCompatibilityOrReplacement,
    higgslessRouteRequiresKkTowerAndUnitarityCompletion,
    higgslessRouteRequiresPrecisionElectroweakAndRgLineage,
    higgslessRouteRequiresGeVUnitNormalization,
    higgslessRouteProvidesGuLocalWzTheorem,
    higgslessRouteProvidesSeparateWzSourceRows,
    higgslessRouteProvidesTargetIndependentBoundaryConditionSource,
    higgslessRouteProvidesTargetIndependentCompactificationOrWarpScaleSource,
    higgslessRouteProvidesGuWeakMixingAngleSource,
    higgslessRouteProvidesGuGaugeCouplingNormalization,
    higgslessRouteProvidesObservedPhotonWzProjectionRows,
    higgslessRouteProvidesGuObservedFieldExtraction,
    higgslessRouteProvidesGuHiggsScalarSourceOperator,
    higgslessRouteProvidesObservedHiggsMassFromGu,
    higgslessRouteProvidesGeVUnitNormalization,
    higgslessRoutePromotesWzMasses,
    higgslessRoutePromotesHiggsMass,
    higgslessRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount
    },
    relatedBoundaryEvidence = new
    {
        gaugeHiggsBoundarySourceAuditPassed,
        gaugeHiggsBoundaryCompletesBosonPredictions,
        technicolorWalkingElectroweakScaleSourceAuditPassed,
        technicolorCompletesBosonPredictions,
        electroweakUnitarityScatteringSourceAuditPassed,
        unitarityRouteProvidesUpperBoundOnly,
        unitarityRouteCompletesBosonPredictions,
        obliquePrecisionElectroweakSourceAuditPassed,
        obliqueFitUsesPrecisionWzData,
        obliqueRouteCompletesBosonPredictions,
        kaluzaKleinInternalSymmetrySourceAuditPassed,
        kkRouteCompletesBosonPredictions,
        scherkSchwarzTwistedCompactificationSourceAuditPassed,
        scherkRouteCompletesBosonPredictions
    },
    decision = "Do not promote W/Z or Higgs physical masses from Higgsless boundary-condition electroweak breaking. The route is a direct external W/Z mass-generation lead, but the current sources select boundary conditions, compactification or warp scales, bulk couplings, brane terms, and KK completions as model data; several versions also predict no Higgs or use precision-fit constraints, so they cannot fill GU-local W/Z/H source-lineage rows.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem mapping native geometry to a physical interval, brane, or boundary-condition sector.",
        "Target-independent boundary conditions plus compactification/warp scale and bulk gauge-coupling lineage.",
        "Observed photon/W/Z projection rows with neutral mixing, weak-angle, and gauge-coupling normalization.",
        "A compatible account of the observed Higgs or a GU scalar-source replacement with prediction row.",
        "KK-tower unitarity, precision-electroweak, RG, threshold, and GeV-unit normalization lineage validated through Phase201/Phase256."
    }
};

var fullPath = Path.Combine(outputDir, "higgsless_boundary_condition_source_audit.json");
var summaryPath = Path.Combine(outputDir, "higgsless_boundary_condition_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.higgslessBoundaryConditionSourceAuditPassed,
    result.higgslessBoundaryLeadPresent,
    result.higgslessPrimarySourcesReviewed,
    result.higgslessRouteExternalToGu,
    result.warpedHiggslessBoundaryConditionLeadPresent,
    result.warpedRouteBreaksElectroweakSymmetryByBoundaryConditions,
    result.warpedRouteUsesBulkSu2lSu2rU1BLGaugeGroup,
    result.warpedRoutePredictsNoFundamentalOrCompositeHiggs,
    result.warpedRouteHasObservedHiggsConflict,
    result.warpedRouteHasTevVectorResonancePrediction,
    result.nomuraWarpedDualStrongDynamicsLeadPresent,
    result.nomuraRouteIntroducesBulkGaugeCouplingRatioParameter,
    result.theorySpaceHiggslessLeadPresent,
    result.theorySpaceRouteCanFitWzMassesByVaryingCouplings,
    result.theorySpaceLightestKkStatesIdentifiedAsWz,
    result.warpedPhenomenologyPrecisionConstraintLeadPresent,
    result.warpedPhenomenologyFindsPerturbativeUnitarityViolation,
    result.sixDHiggslessLeadPresent,
    result.sixDRouteArrangesWzSplittingByCompactificationScales,
    result.sixDRouteUsesElectroweakScaleBulkKineticCouplings,
    result.unitaritySumRulesLeadPresent,
    result.unitarityRouteRequiresKkTowerForConsistency,
    result.precisionConstraintBoundaryPresent,
    result.routeOverlapsScherkSchwarzBoundaryCondition,
    result.routeOverlapsKaluzaKleinInternalSymmetry,
    result.routeOverlapsTechnicolorCompositeDual,
    result.routeDistinctFromGaugeHiggsWilsonLine,
    result.higgslessRouteRequiresGuLocalIntervalOrBoundaryGeometry,
    result.higgslessRouteRequiresTargetIndependentBoundaryConditions,
    result.higgslessRouteRequiresCompactificationOrWarpScaleSource,
    result.higgslessRouteRequiresBulkGaugeCouplingLineage,
    result.higgslessRouteRequiresElectroweakEmbeddingAndNeutralProjection,
    result.higgslessRouteRequiresObservedPhotonWzProjection,
    result.higgslessRouteRequiresObservedHiggsCompatibilityOrReplacement,
    result.higgslessRouteRequiresKkTowerAndUnitarityCompletion,
    result.higgslessRouteRequiresPrecisionElectroweakAndRgLineage,
    result.higgslessRouteRequiresGeVUnitNormalization,
    result.higgslessRouteProvidesGuLocalWzTheorem,
    result.higgslessRouteProvidesSeparateWzSourceRows,
    result.higgslessRouteProvidesTargetIndependentBoundaryConditionSource,
    result.higgslessRouteProvidesTargetIndependentCompactificationOrWarpScaleSource,
    result.higgslessRouteProvidesGuWeakMixingAngleSource,
    result.higgslessRouteProvidesGuGaugeCouplingNormalization,
    result.higgslessRouteProvidesObservedPhotonWzProjectionRows,
    result.higgslessRouteProvidesGuObservedFieldExtraction,
    result.higgslessRouteProvidesGuHiggsScalarSourceOperator,
    result.higgslessRouteProvidesObservedHiggsMassFromGu,
    result.higgslessRouteProvidesGeVUnitNormalization,
    result.higgslessRoutePromotesWzMasses,
    result.higgslessRoutePromotesHiggsMass,
    result.higgslessRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.relatedBoundaryEvidence,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"higgslessBoundaryConditionSourceAuditPassed={higgslessBoundaryConditionSourceAuditPassed}");
Console.WriteLine($"warpedRouteBreaksElectroweakSymmetryByBoundaryConditions={warpedRouteBreaksElectroweakSymmetryByBoundaryConditions}");
Console.WriteLine($"theorySpaceRouteCanFitWzMassesByVaryingCouplings={theorySpaceRouteCanFitWzMassesByVaryingCouplings}");
Console.WriteLine($"sixDRouteArrangesWzSplittingByCompactificationScales={sixDRouteArrangesWzSplittingByCompactificationScales}");
Console.WriteLine($"higgslessRoutePromotesWzMasses={higgslessRoutePromotesWzMasses}");
Console.WriteLine($"higgslessRoutePromotesHiggsMass={higgslessRoutePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value))
    {
        return null;
    }

    return value.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };
}

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
    {
        return null;
    }

    return value.TryGetInt32(out var parsed) ? parsed : null;
}

public sealed record SourceRow(string SourceId, string Url, string FindingKind, string Summary, string PredictionImpact);

public sealed record Check(string CheckId, bool Passed, string Detail);
