using System.Text.Json;

const string DefaultOutputDir = "studies/phase337_octonion_clifford_internal_space_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase235Path = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output/pati_salam_weak_mixing_normalization_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase334Path = "studies/phase334_su21_superconnection_source_audit_001/output/su21_superconnection_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE337_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase235 = JsonDocument.Parse(File.ReadAllText(Phase235Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase334 = JsonDocument.Parse(File.ReadAllText(Phase334Path));

const string todorovOctonionInternalSpaceUrl = "https://arxiv.org/abs/2206.06912";
const string fureyLadderSymmetryUrl = "https://arxiv.org/abs/1806.00612";
const string fureyStandardModelAlgebraUrl = "https://arxiv.org/abs/1611.09182";

const bool octonionCliffordSourceAuditPassedExpected = true;
const bool octonionCliffordLeadPresent = true;
const bool octonionPrimarySourcesReviewed = true;
const bool octonionRouteExternalToGu = true;
const bool octonionRouteUsesCl10InternalSpace = true;
const bool octonionRouteUsesPreferredComplexStructure = true;
const bool octonionRouteUsesParticleSubspaceProjector = true;
const bool octonionRouteUsesPatiSalamSpin10Embedding = true;
const bool octonionRouteIdentifiesSmGaugeGroupAsSterileNeutrinoStabilizer = true;
const bool octonionRoutePlacesHiggsAsSuperconnectionScalar = true;
const bool octonionRoutePreservesColourByEvenCl6Projection = true;
const bool octonionRouteProvidesExternalHiggsWRelation = true;
const bool octonionRouteProvidesTheoreticalWeinbergAngleRelation = true;
const bool octonionRouteDivisionAlgebraLadderSupportPresent = true;
const bool octonionRouteDistinctFromPhase334Su21 = true;
const bool octonionRouteRelatedToPhase334Su21Superconnection = true;

const double theoreticalTanSquaredTheta = 3.0 / 5.0;
var theoreticalCosSquaredTheta = 1.0 / (1.0 + theoreticalTanSquaredTheta);
var externalMhSquaredOverMwSquared = 4.0 * theoreticalCosSquaredTheta;
var externalMhOverMw = Math.Sqrt(externalMhSquaredOverMwSquared);
var observedWTargetGeV = FindComparisonDouble(phase148.RootElement, "physical-w-boson-mass-gev", "targetValue");
var observedWTargetUncertaintyGeV = FindComparisonDouble(phase148.RootElement, "physical-w-boson-mass-gev", "targetUncertainty");
var observedHiggsTargetGeV = FindComparisonDouble(phase148.RootElement, "physical-higgs-mass-gev", "targetValue");
var observedHiggsTargetUncertaintyGeV = FindComparisonDouble(phase148.RootElement, "physical-higgs-mass-gev", "targetUncertainty");
var higgsFromObservedWDiagnosticGeV = observedWTargetGeV is not null ? observedWTargetGeV.Value * externalMhOverMw : (double?)null;
var higgsFromObservedWResidualGeV = higgsFromObservedWDiagnosticGeV is not null && observedHiggsTargetGeV is not null
    ? higgsFromObservedWDiagnosticGeV.Value - observedHiggsTargetGeV.Value
    : (double?)null;
var higgsFromObservedWCombinedUncertaintyGeV = observedWTargetUncertaintyGeV is not null && observedHiggsTargetUncertaintyGeV is not null
    ? Math.Sqrt(Math.Pow(observedWTargetUncertaintyGeV.Value * externalMhOverMw, 2.0) + Math.Pow(observedHiggsTargetUncertaintyGeV.Value, 2.0))
    : (double?)null;
var higgsFromObservedWPull = higgsFromObservedWResidualGeV is not null && higgsFromObservedWCombinedUncertaintyGeV is > 0.0
    ? Math.Abs(higgsFromObservedWResidualGeV.Value) / higgsFromObservedWCombinedUncertaintyGeV.Value
    : (double?)null;
var observedMhOverMw = observedWTargetGeV is > 0.0 && observedHiggsTargetGeV is not null
    ? observedHiggsTargetGeV.Value / observedWTargetGeV.Value
    : (double?)null;
var observedMhSquaredOverMwSquared = observedMhOverMw is not null
    ? observedMhOverMw.Value * observedMhOverMw.Value
    : (double?)null;
var externalMhOverMwRelativeResidual = observedMhOverMw is > 0.0
    ? Math.Abs(externalMhOverMw - observedMhOverMw.Value) / observedMhOverMw.Value
    : (double?)null;
var externalRelationWithinTwoPercentOfObservedRatio = externalMhOverMwRelativeResidual is not null
    && externalMhOverMwRelativeResidual < 0.02;

const bool octonionRouteRequiresCl10ToGuFieldMap = true;
const bool octonionRouteRequiresParticleProjectorSource = true;
const bool octonionRouteRequiresReducedParticleSubspaceChoice = true;
const bool octonionRouteRequiresSuperconnectionNormalization = true;
const bool octonionRouteRequiresLowEnergyWeinbergAngleTransport = true;
const bool octonionRouteRequiresElectroweakScaleOrVevSource = true;
const bool octonionRouteRequiresObservedPhotonWzProjection = true;
const bool octonionRouteRequiresHiggsScalarSourceValidation = true;
const bool octonionRouteRequiresGeVUnitNormalization = true;

const bool octonionRouteProvidesGuLocalWzTheorem = false;
const bool octonionRouteProvidesSeparateWzSourceRows = false;
const bool octonionRouteProvidesWzRawAmplitudeGates = false;
const bool octonionRouteProvidesWzCommonBridgeGate = false;
const bool octonionRouteProvidesTargetIndependentGuVevSource = false;
const bool octonionRouteProvidesGuWeakMixingAngleSource = false;
const bool octonionRouteProvidesGuGaugeCouplingNormalization = false;
const bool octonionRouteProvidesObservedPhotonWzProjectionRows = false;
const bool octonionRouteProvidesGuObservedFieldExtraction = false;
const bool octonionRouteProvidesGuHiggsScalarSourceOperator = false;
const bool octonionRouteProvidesGuHiggsQuarticOrExcitationSource = false;
const bool octonionRouteProvidesObservedHiggsMassFromGu = false;
const bool octonionRouteProvidesGeVUnitNormalization = false;
const bool octonionRoutePromotesWzMasses = false;
const bool octonionRoutePromotesHiggsMass = false;
const bool octonionRouteCompletesBosonPredictions = false;
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
var patiSalamWeakMixingNormalizationAuditPassed = JsonBool(phase235.RootElement, "patiSalamWeakMixingNormalizationAuditPassed") is true;
var patiSalamHyperchargeEmbeddingLeadPresent = JsonBool(phase235.RootElement, "patiSalamHyperchargeEmbeddingLeadPresent") is true;
var patiSalamNormalizationPromotableForLowEnergyWz = JsonBool(phase235.RootElement, "patiSalamNormalizationPromotableForLowEnergyWz") is true;
var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;
var su21SuperconnectionSourceAuditPassed = JsonBool(phase334.RootElement, "su21SuperconnectionSourceAuditPassed") is true;
var su21RoutePromotesWzMasses = JsonBool(phase334.RootElement, "su21RoutePromotesWzMasses") is true;
var su21RoutePromotesHiggsMass = JsonBool(phase334.RootElement, "su21RoutePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-2206-06912-octonion-internal-space",
        todorovOctonionInternalSpaceUrl,
        "Octonion/Clifford internal-space algebra for the Standard Model",
        "Uses a preferred complex structure in Cl10, a particle-subspace projector, Pati-Salam Spin(10) structure, and the sterile neutrino stabilizer to recover the SM gauge group; treats the Higgs as a superconnection scalar and records m_H^2/m_W^2 = 5/2 = 4 cos^2(theta_th).",
        "Serious algebraic W/H ratio lead, but external to GU and not a GU-local W/Z/H source-lineage artifact."),
    new SourceRow(
        "arxiv-1806-00612-division-algebra-ladder-symmetries",
        fureyLadderSymmetryUrl,
        "Division-algebraic ladder-operator SM gauge symmetries",
        "Shows SU(3)c x SU(2)L x U(1)Y structure emerging as ladder symmetries of division-algebraic Clifford systems.",
        "Supports octonion/Clifford internal-algebra relevance but does not provide boson mass source rows."),
    new SourceRow(
        "arxiv-1611-09182-standard-model-physics-from-an-algebra",
        fureyStandardModelAlgebraUrl,
        "Standard Model representation structure from normed-division algebra",
        "Builds SM-like fermion and charge representation structure using complex octonions, quaternions, and Clifford-algebra ideals.",
        "Useful representation-context source; no W/Z/H mass prediction contract is filled.")
};

var checks = new[]
{
    new Check(
        "octonion-clifford-primary-sources-reviewed",
        octonionCliffordLeadPresent
            && octonionPrimarySourcesReviewed
            && octonionRouteExternalToGu
            && sourceRows.Length == 3,
        $"lead={octonionCliffordLeadPresent}; reviewed={octonionPrimarySourcesReviewed}; externalToGu={octonionRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "octonion-clifford-standard-model-algebra-lead-captured",
        octonionRouteUsesCl10InternalSpace
            && octonionRouteUsesPreferredComplexStructure
            && octonionRouteUsesParticleSubspaceProjector
            && octonionRouteUsesPatiSalamSpin10Embedding
            && octonionRouteIdentifiesSmGaugeGroupAsSterileNeutrinoStabilizer
            && octonionRoutePreservesColourByEvenCl6Projection
            && octonionRouteDivisionAlgebraLadderSupportPresent,
        $"cl10={octonionRouteUsesCl10InternalSpace}; complexStructure={octonionRouteUsesPreferredComplexStructure}; projector={octonionRouteUsesParticleSubspaceProjector}; patiSalam={octonionRouteUsesPatiSalamSpin10Embedding}; sterileNeutrinoStabilizer={octonionRouteIdentifiesSmGaugeGroupAsSterileNeutrinoStabilizer}; colourPreserved={octonionRoutePreservesColourByEvenCl6Projection}; ladderSupport={octonionRouteDivisionAlgebraLadderSupportPresent}"),
    new Check(
        "octonion-clifford-higgs-w-ratio-lead-captured",
        octonionRoutePlacesHiggsAsSuperconnectionScalar
            && octonionRouteProvidesExternalHiggsWRelation
            && octonionRouteProvidesTheoreticalWeinbergAngleRelation
            && externalMhSquaredOverMwSquared == 2.5
            && externalRelationWithinTwoPercentOfObservedRatio,
        $"higgsSuperconnection={octonionRoutePlacesHiggsAsSuperconnectionScalar}; tan2Theta={theoreticalTanSquaredTheta:R}; cos2Theta={theoreticalCosSquaredTheta:R}; mh2OverMw2={externalMhSquaredOverMwSquared:R}; diagnosticHiggsFromObservedWGeV={higgsFromObservedWDiagnosticGeV:R}; relativeRatioResidual={externalMhOverMwRelativeResidual:R}; pull={higgsFromObservedWPull:R}"),
    new Check(
        "octonion-clifford-route-related-to-but-distinct-from-su21",
        octonionRouteDistinctFromPhase334Su21
            && octonionRouteRelatedToPhase334Su21Superconnection
            && su21SuperconnectionSourceAuditPassed
            && !su21RoutePromotesWzMasses
            && !su21RoutePromotesHiggsMass,
        $"distinctFromP334={octonionRouteDistinctFromPhase334Su21}; relatedSuperconnection={octonionRouteRelatedToPhase334Su21Superconnection}; p334={su21SuperconnectionSourceAuditPassed}; p334PromotesWz={su21RoutePromotesWzMasses}; p334PromotesHiggs={su21RoutePromotesHiggsMass}"),
    new Check(
        "octonion-clifford-requires-missing-gu-source-data",
        octonionRouteRequiresCl10ToGuFieldMap
            && octonionRouteRequiresParticleProjectorSource
            && octonionRouteRequiresReducedParticleSubspaceChoice
            && octonionRouteRequiresSuperconnectionNormalization
            && octonionRouteRequiresLowEnergyWeinbergAngleTransport
            && octonionRouteRequiresElectroweakScaleOrVevSource
            && octonionRouteRequiresObservedPhotonWzProjection
            && octonionRouteRequiresHiggsScalarSourceValidation
            && octonionRouteRequiresGeVUnitNormalization,
        $"guCl10Map={octonionRouteRequiresCl10ToGuFieldMap}; particleProjector={octonionRouteRequiresParticleProjectorSource}; reducedSubspace={octonionRouteRequiresReducedParticleSubspaceChoice}; normalization={octonionRouteRequiresSuperconnectionNormalization}; weakAngleTransport={octonionRouteRequiresLowEnergyWeinbergAngleTransport}; vev={octonionRouteRequiresElectroweakScaleOrVevSource}; projection={octonionRouteRequiresObservedPhotonWzProjection}; higgsValidation={octonionRouteRequiresHiggsScalarSourceValidation}; gev={octonionRouteRequiresGeVUnitNormalization}"),
    new Check(
        "existing-pati-salam-and-electroweak-dependency-blockers-still-bind",
        patiSalamWeakMixingNormalizationAuditPassed
            && patiSalamHyperchargeEmbeddingLeadPresent
            && !patiSalamNormalizationPromotableForLowEnergyWz
            && electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && electroweakMassMatrixBridgeSourceAuditPassed
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions,
        $"p235={patiSalamWeakMixingNormalizationAuditPassed}; p235LowEnergyPromotable={patiSalamNormalizationPromotableForLowEnergyWz}; p224={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; p317={electroweakMassMatrixBridgeSourceAuditPassed}; smPromotesWz={smMassMatrixPromotesWzMasses}; smPromotesHiggs={smMassMatrixPromotesHiggsMass}; p323Completes={coupledYangMillsHiggsRouteCompletesBosonPredictions}"),
    new Check(
        "octonion-clifford-route-does-not-fill-gu-wz-higgs-contracts",
        !octonionRouteProvidesGuLocalWzTheorem
            && !octonionRouteProvidesSeparateWzSourceRows
            && !octonionRouteProvidesWzRawAmplitudeGates
            && !octonionRouteProvidesWzCommonBridgeGate
            && !octonionRouteProvidesTargetIndependentGuVevSource
            && !octonionRouteProvidesGuWeakMixingAngleSource
            && !octonionRouteProvidesGuGaugeCouplingNormalization
            && !octonionRouteProvidesObservedPhotonWzProjectionRows
            && !octonionRouteProvidesGuObservedFieldExtraction
            && !octonionRouteProvidesGuHiggsScalarSourceOperator
            && !octonionRouteProvidesGuHiggsQuarticOrExcitationSource
            && !octonionRouteProvidesObservedHiggsMassFromGu
            && !octonionRouteProvidesGeVUnitNormalization
            && !octonionRoutePromotesWzMasses
            && !octonionRoutePromotesHiggsMass
            && !octonionRouteCompletesBosonPredictions,
        $"guWzTheorem={octonionRouteProvidesGuLocalWzTheorem}; separateRows={octonionRouteProvidesSeparateWzSourceRows}; targetVev={octonionRouteProvidesTargetIndependentGuVevSource}; weakAngle={octonionRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={octonionRouteProvidesGuGaugeCouplingNormalization}; projection={octonionRouteProvidesObservedPhotonWzProjectionRows}; observedExtraction={octonionRouteProvidesGuObservedFieldExtraction}; higgsOperator={octonionRouteProvidesGuHiggsScalarSourceOperator}; higgsQuartic={octonionRouteProvidesGuHiggsQuarticOrExcitationSource}; gev={octonionRouteProvidesGeVUnitNormalization}; promotesWz={octonionRoutePromotesWzMasses}; promotesHiggs={octonionRoutePromotesHiggsMass}"),
    new Check(
        "phase201-phase256-contract-state-unchanged",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
};

var octonionCliffordSourceAuditPassed = checks.All(check => check.Passed)
    && octonionCliffordSourceAuditPassedExpected
    && !octonionRouteProvidesGuLocalWzTheorem
    && !octonionRouteProvidesSeparateWzSourceRows
    && !octonionRouteProvidesTargetIndependentGuVevSource
    && !octonionRouteProvidesGuWeakMixingAngleSource
    && !octonionRouteProvidesGuGaugeCouplingNormalization
    && !octonionRouteProvidesGuObservedFieldExtraction
    && !octonionRouteProvidesGuHiggsScalarSourceOperator
    && !octonionRouteProvidesGuHiggsQuarticOrExcitationSource
    && !octonionRouteProvidesObservedHiggsMassFromGu
    && !octonionRouteProvidesGeVUnitNormalization
    && !octonionRoutePromotesWzMasses
    && !octonionRoutePromotesHiggsMass
    && !octonionRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = octonionCliffordSourceAuditPassed
    ? "octonion-clifford-internal-space-source-audit-ratio-lead-not-gu-source"
    : "octonion-clifford-internal-space-source-audit-review-required";

var result = new
{
    phaseId = "phase337-octonion-clifford-internal-space-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    octonionCliffordSourceAuditPassed,
    octonionCliffordLeadPresent,
    octonionPrimarySourcesReviewed,
    octonionRouteExternalToGu,
    octonionRouteUsesCl10InternalSpace,
    octonionRouteUsesPreferredComplexStructure,
    octonionRouteUsesParticleSubspaceProjector,
    octonionRouteUsesPatiSalamSpin10Embedding,
    octonionRouteIdentifiesSmGaugeGroupAsSterileNeutrinoStabilizer,
    octonionRoutePlacesHiggsAsSuperconnectionScalar,
    octonionRoutePreservesColourByEvenCl6Projection,
    octonionRouteProvidesExternalHiggsWRelation,
    octonionRouteProvidesTheoreticalWeinbergAngleRelation,
    octonionRouteDivisionAlgebraLadderSupportPresent,
    octonionRouteDistinctFromPhase334Su21,
    octonionRouteRelatedToPhase334Su21Superconnection,
    octonionRouteRequiresCl10ToGuFieldMap,
    octonionRouteRequiresParticleProjectorSource,
    octonionRouteRequiresReducedParticleSubspaceChoice,
    octonionRouteRequiresSuperconnectionNormalization,
    octonionRouteRequiresLowEnergyWeinbergAngleTransport,
    octonionRouteRequiresElectroweakScaleOrVevSource,
    octonionRouteRequiresObservedPhotonWzProjection,
    octonionRouteRequiresHiggsScalarSourceValidation,
    octonionRouteRequiresGeVUnitNormalization,
    octonionRouteProvidesGuLocalWzTheorem,
    octonionRouteProvidesSeparateWzSourceRows,
    octonionRouteProvidesWzRawAmplitudeGates,
    octonionRouteProvidesWzCommonBridgeGate,
    octonionRouteProvidesTargetIndependentGuVevSource,
    octonionRouteProvidesGuWeakMixingAngleSource,
    octonionRouteProvidesGuGaugeCouplingNormalization,
    octonionRouteProvidesObservedPhotonWzProjectionRows,
    octonionRouteProvidesGuObservedFieldExtraction,
    octonionRouteProvidesGuHiggsScalarSourceOperator,
    octonionRouteProvidesGuHiggsQuarticOrExcitationSource,
    octonionRouteProvidesObservedHiggsMassFromGu,
    octonionRouteProvidesGeVUnitNormalization,
    octonionRoutePromotesWzMasses,
    octonionRoutePromotesHiggsMass,
    octonionRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    numericalLead = new
    {
        theoreticalTanSquaredTheta,
        theoreticalCosSquaredTheta,
        externalMhSquaredOverMwSquared,
        externalMhOverMw,
        observedMhOverMw,
        observedMhSquaredOverMwSquared,
        externalMhOverMwRelativeResidual,
        observedWTargetGeV,
        observedHiggsTargetGeV,
        higgsFromObservedWDiagnosticGeV,
        higgsFromObservedWResidualGeV,
        higgsFromObservedWPull,
        externalRelationWithinTwoPercentOfObservedRatio,
        diagnosticWarning = "The Higgs value here imports the observed W mass and is therefore a comparison diagnostic, not a prediction."
    },
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
    decision = "Do not promote W/Z or Higgs physical masses from the octonion/Clifford internal-space route in this repository. The route gives a serious algebraic Standard Model and Higgs/W mass-ratio lead, but it is external to GU and still lacks a GU-local Cl10/internal-algebra map, particle projector source, reduced particle-subspace selection law, low-energy weak-angle transport, target-independent VEV or scale source, observed photon/W/Z/H projection rows, Higgs scalar-source validation, and GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem mapping the draft's spinor/internal normal factor into the octonion/Clifford Cl10 particle-subspace algebra without choosing the observed target by hand.",
        "A source-derived reduced particle projector or equivalent selection law that fixes the physical electroweak representation content.",
        "Low-energy weak-angle, gauge-coupling, and VEV/scale transport from the algebraic relation to W/Z source rows.",
        "Observed photon/W/Z/H projection rows and a GU Higgs scalar-source operator or potential lineage at the same vacuum.",
        "Physical-unit normalization and validation through Phase201/Phase256 without importing W, Z, or Higgs targets."
    }
};

var fullPath = Path.Combine(outputDir, "octonion_clifford_internal_space_source_audit.json");
var summaryPath = Path.Combine(outputDir, "octonion_clifford_internal_space_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.octonionCliffordSourceAuditPassed,
    result.octonionCliffordLeadPresent,
    result.octonionPrimarySourcesReviewed,
    result.octonionRouteExternalToGu,
    result.octonionRouteUsesCl10InternalSpace,
    result.octonionRouteUsesPreferredComplexStructure,
    result.octonionRouteUsesParticleSubspaceProjector,
    result.octonionRouteUsesPatiSalamSpin10Embedding,
    result.octonionRouteIdentifiesSmGaugeGroupAsSterileNeutrinoStabilizer,
    result.octonionRoutePlacesHiggsAsSuperconnectionScalar,
    result.octonionRoutePreservesColourByEvenCl6Projection,
    result.octonionRouteProvidesExternalHiggsWRelation,
    result.octonionRouteProvidesTheoreticalWeinbergAngleRelation,
    result.octonionRouteDivisionAlgebraLadderSupportPresent,
    result.octonionRouteDistinctFromPhase334Su21,
    result.octonionRouteRelatedToPhase334Su21Superconnection,
    result.octonionRouteRequiresCl10ToGuFieldMap,
    result.octonionRouteRequiresParticleProjectorSource,
    result.octonionRouteRequiresReducedParticleSubspaceChoice,
    result.octonionRouteRequiresSuperconnectionNormalization,
    result.octonionRouteRequiresLowEnergyWeinbergAngleTransport,
    result.octonionRouteRequiresElectroweakScaleOrVevSource,
    result.octonionRouteRequiresObservedPhotonWzProjection,
    result.octonionRouteRequiresHiggsScalarSourceValidation,
    result.octonionRouteRequiresGeVUnitNormalization,
    result.octonionRouteProvidesGuLocalWzTheorem,
    result.octonionRouteProvidesSeparateWzSourceRows,
    result.octonionRouteProvidesTargetIndependentGuVevSource,
    result.octonionRouteProvidesGuWeakMixingAngleSource,
    result.octonionRouteProvidesGuGaugeCouplingNormalization,
    result.octonionRouteProvidesObservedPhotonWzProjectionRows,
    result.octonionRouteProvidesGuObservedFieldExtraction,
    result.octonionRouteProvidesGuHiggsScalarSourceOperator,
    result.octonionRouteProvidesGuHiggsQuarticOrExcitationSource,
    result.octonionRouteProvidesObservedHiggsMassFromGu,
    result.octonionRouteProvidesGeVUnitNormalization,
    result.octonionRoutePromotesWzMasses,
    result.octonionRoutePromotesHiggsMass,
    result.octonionRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    result.numericalLead,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"octonionCliffordSourceAuditPassed={octonionCliffordSourceAuditPassed}");
Console.WriteLine($"octonionRouteProvidesExternalHiggsWRelation={octonionRouteProvidesExternalHiggsWRelation}");
Console.WriteLine($"externalMhSquaredOverMwSquared={externalMhSquaredOverMwSquared:R}");
Console.WriteLine($"higgsFromObservedWDiagnosticGeV={higgsFromObservedWDiagnosticGeV:R}");
Console.WriteLine($"octonionRoutePromotesWzMasses={octonionRoutePromotesWzMasses}");
Console.WriteLine($"octonionRoutePromotesHiggsMass={octonionRoutePromotesHiggsMass}");
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

static double? FindComparisonDouble(JsonElement root, string observableId, string propertyName)
{
    if (!root.TryGetProperty("comparisonRows", out var rows) || rows.ValueKind != JsonValueKind.Array)
    {
        return null;
    }

    foreach (var row in rows.EnumerateArray())
    {
        if (row.TryGetProperty("observableId", out var observable)
            && observable.ValueKind == JsonValueKind.String
            && string.Equals(observable.GetString(), observableId, StringComparison.Ordinal)
            && row.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.Number
            && property.TryGetDouble(out var value))
        {
            return value;
        }
    }

    return null;
}

public sealed record SourceRow(string SourceId, string Url, string FindingKind, string Summary, string PredictionImpact);

public sealed record Check(string CheckId, bool Passed, string Detail);
