using System.Text.Json;

const string DefaultOutputDir = "studies/phase338_metric_affine_torsion_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase279Path = "studies/phase279_technicolor_walking_electroweak_scale_source_audit_001/output/technicolor_walking_electroweak_scale_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase327Path = "studies/phase327_oblique_precision_electroweak_source_audit_001/output/oblique_precision_electroweak_source_audit_summary.json";
const string Phase337Path = "studies/phase337_octonion_clifford_internal_space_source_audit_001/output/octonion_clifford_internal_space_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE338_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase279 = JsonDocument.Parse(File.ReadAllText(Phase279Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase327 = JsonDocument.Parse(File.ReadAllText(Phase327Path));
using var phase337 = JsonDocument.Parse(File.ReadAllText(Phase337Path));

const string torsionInsteadTechnicolorUrl = "https://arxiv.org/abs/1003.5473";
const string holstDynamicalEwsbUrl = "https://arxiv.org/abs/1004.1375";
const string threeBfSmEinsteinCartanUrl = "https://arxiv.org/abs/2402.17675";
const string metricAffineHiggsScalarUrl = "https://arxiv.org/abs/2204.03003";
const string metricAffineElectroweakVacuumDecayUrl = "https://arxiv.org/abs/2305.07693";

const bool metricAffineTorsionSourceAuditPassedExpected = true;
const bool metricAffineTorsionLeadPresent = true;
const bool metricAffineTorsionPrimarySourcesReviewed = true;
const bool metricAffineTorsionRouteExternalToGu = true;
const bool torsionRouteUsesEinsteinCartanOrPoincareGravity = true;
const bool torsionRouteUsesHolstPalatiniAction = true;
const bool torsionRouteUsesNonminimalSpinorTorsionCoupling = true;
const bool torsionRouteGeneratesEffectiveFourFermionInteractions = true;
const bool torsionRouteUsesParityBreakingTorsionCouplings = true;
const bool torsionRouteCanInduceTechnifermionCondensation = true;
const bool torsionRouteCanGenerateGaugeBosonMassesViaCondensate = true;
const bool torsionRouteRequiresTevScaleTorsionMassParameter = true;
const bool torsionRouteUsesNjlOrEnjlCondensateDynamics = true;
const bool torsionRouteIntroducesAdditionalTechnifermions = true;
const bool torsionRouteNeedsPrecisionElectroweakMatching = true;
const bool threeBfRouteUsesStandardModelEinsteinCartanThreeGroup = true;
const bool threeBfRouteStudiesExplicitAndSpontaneousSymmetryBreaking = true;
const bool threeBfRouteFormulatesElectroweakHiggsMechanism = true;
const bool threeBfRouteFormulatesProcaMassTerms = true;
const bool threeBfRouteRecoversTextbookElectroweakMassStructure = true;
const bool threeBfRouteUsesVevGaugeCouplingsAndHiggsQuarticAsParameters = true;
const bool metricAffineScalarHiggsContextPresent = true;
const bool metricAffineElectroweakVacuumStabilityContextPresent = true;
const bool torsionRouteOverlapsTechnicolorButAddsGeometricContactSource = true;
const bool torsionRouteRelatedToGuAugmentedTorsionHooks = true;
const bool torsionRouteDistinctFromPhase337OctonionClifford = true;

const bool torsionRouteRequiresGuLocalMetricAffineEinsteinCartanMap = true;
const bool torsionRouteRequiresTorsionHolstScaleOrImmirziSource = true;
const bool torsionRouteRequiresTechnifermionRepresentationSource = true;
const bool torsionRouteRequiresCondensateDynamicsAndNormalization = true;
const bool torsionRouteRequiresElectroweakEmbeddingAndPrecisionMatching = true;
const bool torsionRouteRequiresObservedPhotonWzProjection = true;
const bool torsionRouteRequiresVevOrScaleLineage = true;
const bool torsionRouteRequiresWeakAngleLineage = true;
const bool torsionRouteRequiresHiggsScalarSourceOrCompositeProfile = true;
const bool torsionRouteRequiresGeVUnitNormalization = true;

const bool torsionRouteProvidesGuLocalWzTheorem = false;
const bool torsionRouteProvidesSeparateWzSourceRows = false;
const bool torsionRouteProvidesWzRawAmplitudeGates = false;
const bool torsionRouteProvidesWzCommonBridgeGate = false;
const bool torsionRouteProvidesTargetIndependentGuVevSource = false;
const bool torsionRouteProvidesGuWeakMixingAngleSource = false;
const bool torsionRouteProvidesGuGaugeCouplingNormalization = false;
const bool torsionRouteProvidesObservedPhotonWzProjectionRows = false;
const bool torsionRouteProvidesGuObservedFieldExtraction = false;
const bool torsionRouteProvidesGuHiggsScalarSourceOperator = false;
const bool torsionRouteProvidesGuHiggsQuarticOrExcitationSource = false;
const bool torsionRouteProvidesObservedHiggsMassFromGu = false;
const bool torsionRouteProvidesGeVUnitNormalization = false;
const bool torsionRoutePromotesWzMasses = false;
const bool torsionRoutePromotesHiggsMass = false;
const bool torsionRouteCompletesBosonPredictions = false;
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
var technicolorWalkingAuditPassed = JsonBool(phase279.RootElement, "technicolorWalkingElectroweakScaleSourceAuditPassed") is true;
var technicolorEwsbLeadPresent = JsonBool(phase279.RootElement, "technicolorEwsbLeadPresent") is true;
var technifermionCondensateLeadPresent = JsonBool(phase279.RootElement, "technifermionCondensateLeadPresent") is true;
var technicolorPromotesWzMasses = JsonBool(phase279.RootElement, "technicolorPromotesWzMasses") is true;
var technicolorPromotesHiggsMass = JsonBool(phase279.RootElement, "technicolorPromotesHiggsMass") is true;
var technicolorCompletesBosonPredictions = JsonBool(phase279.RootElement, "technicolorCompletesBosonPredictions") is true;
var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var coupledYangMillsHiggsRoutePromotesWzMasses = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRoutePromotesWzMasses") is true;
var coupledYangMillsHiggsRoutePromotesHiggsMass = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRoutePromotesHiggsMass") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;
var obliquePrecisionElectroweakSourceAuditPassed = JsonBool(phase327.RootElement, "obliquePrecisionElectroweakSourceAuditPassed") is true;
var obliqueParametersConstrainNewPhysics = JsonBool(phase327.RootElement, "obliqueParametersConstrainNewPhysics") is true;
var obliqueRoutePromotesWzMasses = JsonBool(phase327.RootElement, "obliqueRoutePromotesWzMasses") is true;
var obliqueRoutePromotesHiggsMass = JsonBool(phase327.RootElement, "obliqueRoutePromotesHiggsMass") is true;
var octonionCliffordSourceAuditPassed = JsonBool(phase337.RootElement, "octonionCliffordSourceAuditPassed") is true;
var octonionRouteCompletesBosonPredictions = JsonBool(phase337.RootElement, "octonionRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-1003-5473-torsion-instead-technicolor",
        torsionInsteadTechnicolorUrl,
        "Einstein-Cartan/Poincare torsion as dynamical EWSB source",
        "Nonminimal spinor coupling to torsion yields parity-asymmetric effective four-fermion interactions. Additional technifermions condense and the condensate is used to generate gauge-boson masses.",
        "Direct geometric EWSB lead, but it imports technifermions, condensate dynamics, torsion-scale assumptions, and precision-matching obligations."),
    new SourceRow(
        "arxiv-1004-1375-holst-action-dynamical-ewsb",
        holstDynamicalEwsbUrl,
        "Holst/Palatini torsion contact route to technifermion condensation",
        "Uses the Holst action plus nonminimal fermion couplings to derive torsion-induced four-fermion terms; suitable couplings can make technifermion channels attractive and SM channels repulsive.",
        "Important because Holst/contorsion geometry is explicit, but the electroweak scale and condensate normalization remain model inputs."),
    new SourceRow(
        "arxiv-2402-17675-3bf-sm-einstein-cartan-symmetry-breaking",
        threeBfSmEinsteinCartanUrl,
        "Constrained 3BF Standard Model coupled to Einstein-Cartan gravity",
        "Studies explicit and spontaneous symmetry breaking in the constrained 3BF formulation and reproduces electroweak Higgs and Proca mass structures in higher-gauge variables.",
        "Useful geometric formulation of the mass matrix, but it still depends on VEV, gauge couplings, and Higgs self-coupling parameters."),
    new SourceRow(
        "arxiv-2204-03003-metric-affine-higgs-like-scalar",
        metricAffineHiggsScalarUrl,
        "Metric-affine gravity coupled to a Higgs-like scalar",
        "Builds scalar-field couplings in metric-affine gravity including curvature, torsion, and nonmetricity structures.",
        "Relevant scalar-gravity context; does not provide observed W/Z/H projection or target-independent GU mass lineage."),
    new SourceRow(
        "arxiv-2305-07693-electroweak-vacuum-decay-metric-affine-gravity",
        metricAffineElectroweakVacuumDecayUrl,
        "Electroweak vacuum decay in metric-affine gravity",
        "Studies how metric-affine gravitational couplings affect electroweak vacuum stability and Higgs-sector running.",
        "Useful stability context; not a direct W/Z/H source law.")
};

var checks = new[]
{
    new Check(
        "metric-affine-torsion-primary-sources-reviewed",
        metricAffineTorsionLeadPresent
            && metricAffineTorsionPrimarySourcesReviewed
            && metricAffineTorsionRouteExternalToGu
            && sourceRows.Length == 5,
        $"lead={metricAffineTorsionLeadPresent}; reviewed={metricAffineTorsionPrimarySourcesReviewed}; externalToGu={metricAffineTorsionRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "torsion-holst-dynamical-ewsb-lead-captured",
        torsionRouteUsesEinsteinCartanOrPoincareGravity
            && torsionRouteUsesHolstPalatiniAction
            && torsionRouteUsesNonminimalSpinorTorsionCoupling
            && torsionRouteGeneratesEffectiveFourFermionInteractions
            && torsionRouteUsesParityBreakingTorsionCouplings
            && torsionRouteCanInduceTechnifermionCondensation
            && torsionRouteCanGenerateGaugeBosonMassesViaCondensate
            && torsionRouteRequiresTevScaleTorsionMassParameter
            && torsionRouteUsesNjlOrEnjlCondensateDynamics
            && torsionRouteIntroducesAdditionalTechnifermions,
        $"ecOrPoincare={torsionRouteUsesEinsteinCartanOrPoincareGravity}; holstPalatini={torsionRouteUsesHolstPalatiniAction}; nonminimalSpinorCoupling={torsionRouteUsesNonminimalSpinorTorsionCoupling}; fourFermion={torsionRouteGeneratesEffectiveFourFermionInteractions}; parityBreaking={torsionRouteUsesParityBreakingTorsionCouplings}; technifermionCondensation={torsionRouteCanInduceTechnifermionCondensation}; gaugeBosonMasses={torsionRouteCanGenerateGaugeBosonMassesViaCondensate}; tevScale={torsionRouteRequiresTevScaleTorsionMassParameter}; njl={torsionRouteUsesNjlOrEnjlCondensateDynamics}; extraTechnifermions={torsionRouteIntroducesAdditionalTechnifermions}"),
    new Check(
        "three-bf-higher-gauge-ewsb-lead-captured",
        threeBfRouteUsesStandardModelEinsteinCartanThreeGroup
            && threeBfRouteStudiesExplicitAndSpontaneousSymmetryBreaking
            && threeBfRouteFormulatesElectroweakHiggsMechanism
            && threeBfRouteFormulatesProcaMassTerms
            && threeBfRouteRecoversTextbookElectroweakMassStructure
            && threeBfRouteUsesVevGaugeCouplingsAndHiggsQuarticAsParameters,
        $"smEcThreeGroup={threeBfRouteUsesStandardModelEinsteinCartanThreeGroup}; symmetryBreaking={threeBfRouteStudiesExplicitAndSpontaneousSymmetryBreaking}; higgsMechanism={threeBfRouteFormulatesElectroweakHiggsMechanism}; proca={threeBfRouteFormulatesProcaMassTerms}; textbookMasses={threeBfRouteRecoversTextbookElectroweakMassStructure}; parameters={threeBfRouteUsesVevGaugeCouplingsAndHiggsQuarticAsParameters}"),
    new Check(
        "metric-affine-higgs-context-captured",
        metricAffineScalarHiggsContextPresent
            && metricAffineElectroweakVacuumStabilityContextPresent,
        $"metricAffineScalar={metricAffineScalarHiggsContextPresent}; electroweakVacuumStability={metricAffineElectroweakVacuumStabilityContextPresent}"),
    new Check(
        "torsion-route-overlap-and-existing-ewsb-blockers-preserved",
        torsionRouteOverlapsTechnicolorButAddsGeometricContactSource
            && torsionRouteRelatedToGuAugmentedTorsionHooks
            && torsionRouteDistinctFromPhase337OctonionClifford
            && technicolorWalkingAuditPassed
            && technicolorEwsbLeadPresent
            && technifermionCondensateLeadPresent
            && !technicolorPromotesWzMasses
            && !technicolorPromotesHiggsMass
            && !technicolorCompletesBosonPredictions
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !coupledYangMillsHiggsRoutePromotesWzMasses
            && !coupledYangMillsHiggsRoutePromotesHiggsMass
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && obliquePrecisionElectroweakSourceAuditPassed
            && obliqueParametersConstrainNewPhysics
            && !obliqueRoutePromotesWzMasses
            && !obliqueRoutePromotesHiggsMass
            && octonionCliffordSourceAuditPassed
            && !octonionRouteCompletesBosonPredictions,
        $"technicolorAudit={technicolorWalkingAuditPassed}; technicolorPromotesWz={technicolorPromotesWzMasses}; technicolorPromotesHiggs={technicolorPromotesHiggsMass}; p323={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Completes={coupledYangMillsHiggsRouteCompletesBosonPredictions}; obliqueAudit={obliquePrecisionElectroweakSourceAuditPassed}; obliqueConstrains={obliqueParametersConstrainNewPhysics}; octonionAudit={octonionCliffordSourceAuditPassed}; octonionCompletes={octonionRouteCompletesBosonPredictions}"),
    new Check(
        "metric-affine-torsion-requires-missing-gu-source-data",
        torsionRouteRequiresGuLocalMetricAffineEinsteinCartanMap
            && torsionRouteRequiresTorsionHolstScaleOrImmirziSource
            && torsionRouteRequiresTechnifermionRepresentationSource
            && torsionRouteRequiresCondensateDynamicsAndNormalization
            && torsionRouteRequiresElectroweakEmbeddingAndPrecisionMatching
            && torsionRouteRequiresObservedPhotonWzProjection
            && torsionRouteRequiresVevOrScaleLineage
            && torsionRouteRequiresWeakAngleLineage
            && torsionRouteRequiresHiggsScalarSourceOrCompositeProfile
            && torsionRouteRequiresGeVUnitNormalization,
        $"guMap={torsionRouteRequiresGuLocalMetricAffineEinsteinCartanMap}; scaleOrImmirzi={torsionRouteRequiresTorsionHolstScaleOrImmirziSource}; technifermions={torsionRouteRequiresTechnifermionRepresentationSource}; condensate={torsionRouteRequiresCondensateDynamicsAndNormalization}; ewEmbedding={torsionRouteRequiresElectroweakEmbeddingAndPrecisionMatching}; projection={torsionRouteRequiresObservedPhotonWzProjection}; vev={torsionRouteRequiresVevOrScaleLineage}; weakAngle={torsionRouteRequiresWeakAngleLineage}; higgs={torsionRouteRequiresHiggsScalarSourceOrCompositeProfile}; gev={torsionRouteRequiresGeVUnitNormalization}"),
    new Check(
        "metric-affine-torsion-route-does-not-fill-gu-contracts",
        !torsionRouteProvidesGuLocalWzTheorem
            && !torsionRouteProvidesSeparateWzSourceRows
            && !torsionRouteProvidesWzRawAmplitudeGates
            && !torsionRouteProvidesWzCommonBridgeGate
            && !torsionRouteProvidesTargetIndependentGuVevSource
            && !torsionRouteProvidesGuWeakMixingAngleSource
            && !torsionRouteProvidesGuGaugeCouplingNormalization
            && !torsionRouteProvidesObservedPhotonWzProjectionRows
            && !torsionRouteProvidesGuObservedFieldExtraction
            && !torsionRouteProvidesGuHiggsScalarSourceOperator
            && !torsionRouteProvidesGuHiggsQuarticOrExcitationSource
            && !torsionRouteProvidesObservedHiggsMassFromGu
            && !torsionRouteProvidesGeVUnitNormalization
            && !torsionRoutePromotesWzMasses
            && !torsionRoutePromotesHiggsMass
            && !torsionRouteCompletesBosonPredictions,
        $"guWzTheorem={torsionRouteProvidesGuLocalWzTheorem}; separateRows={torsionRouteProvidesSeparateWzSourceRows}; targetVev={torsionRouteProvidesTargetIndependentGuVevSource}; weakAngle={torsionRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={torsionRouteProvidesGuGaugeCouplingNormalization}; projection={torsionRouteProvidesObservedPhotonWzProjectionRows}; observedExtraction={torsionRouteProvidesGuObservedFieldExtraction}; higgsOperator={torsionRouteProvidesGuHiggsScalarSourceOperator}; higgsQuartic={torsionRouteProvidesGuHiggsQuarticOrExcitationSource}; gev={torsionRouteProvidesGeVUnitNormalization}; promotesWz={torsionRoutePromotesWzMasses}; promotesHiggs={torsionRoutePromotesHiggsMass}"),
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

var metricAffineTorsionSourceAuditPassed = checks.All(check => check.Passed)
    && metricAffineTorsionSourceAuditPassedExpected
    && !torsionRouteProvidesGuLocalWzTheorem
    && !torsionRouteProvidesSeparateWzSourceRows
    && !torsionRouteProvidesTargetIndependentGuVevSource
    && !torsionRouteProvidesGuWeakMixingAngleSource
    && !torsionRouteProvidesGuGaugeCouplingNormalization
    && !torsionRouteProvidesGuObservedFieldExtraction
    && !torsionRouteProvidesGuHiggsScalarSourceOperator
    && !torsionRouteProvidesGuHiggsQuarticOrExcitationSource
    && !torsionRouteProvidesObservedHiggsMassFromGu
    && !torsionRouteProvidesGeVUnitNormalization
    && !torsionRoutePromotesWzMasses
    && !torsionRoutePromotesHiggsMass
    && !torsionRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = metricAffineTorsionSourceAuditPassed
    ? "metric-affine-torsion-source-audit-external-ewsb-lead-not-gu-source"
    : "metric-affine-torsion-source-audit-review-required";

var result = new
{
    phaseId = "phase338-metric-affine-torsion-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    metricAffineTorsionSourceAuditPassed,
    metricAffineTorsionLeadPresent,
    metricAffineTorsionPrimarySourcesReviewed,
    metricAffineTorsionRouteExternalToGu,
    torsionRouteUsesEinsteinCartanOrPoincareGravity,
    torsionRouteUsesHolstPalatiniAction,
    torsionRouteUsesNonminimalSpinorTorsionCoupling,
    torsionRouteGeneratesEffectiveFourFermionInteractions,
    torsionRouteUsesParityBreakingTorsionCouplings,
    torsionRouteCanInduceTechnifermionCondensation,
    torsionRouteCanGenerateGaugeBosonMassesViaCondensate,
    torsionRouteRequiresTevScaleTorsionMassParameter,
    torsionRouteUsesNjlOrEnjlCondensateDynamics,
    torsionRouteIntroducesAdditionalTechnifermions,
    torsionRouteNeedsPrecisionElectroweakMatching,
    threeBfRouteUsesStandardModelEinsteinCartanThreeGroup,
    threeBfRouteStudiesExplicitAndSpontaneousSymmetryBreaking,
    threeBfRouteFormulatesElectroweakHiggsMechanism,
    threeBfRouteFormulatesProcaMassTerms,
    threeBfRouteRecoversTextbookElectroweakMassStructure,
    threeBfRouteUsesVevGaugeCouplingsAndHiggsQuarticAsParameters,
    metricAffineScalarHiggsContextPresent,
    metricAffineElectroweakVacuumStabilityContextPresent,
    torsionRouteOverlapsTechnicolorButAddsGeometricContactSource,
    torsionRouteRelatedToGuAugmentedTorsionHooks,
    torsionRouteDistinctFromPhase337OctonionClifford,
    torsionRouteRequiresGuLocalMetricAffineEinsteinCartanMap,
    torsionRouteRequiresTorsionHolstScaleOrImmirziSource,
    torsionRouteRequiresTechnifermionRepresentationSource,
    torsionRouteRequiresCondensateDynamicsAndNormalization,
    torsionRouteRequiresElectroweakEmbeddingAndPrecisionMatching,
    torsionRouteRequiresObservedPhotonWzProjection,
    torsionRouteRequiresVevOrScaleLineage,
    torsionRouteRequiresWeakAngleLineage,
    torsionRouteRequiresHiggsScalarSourceOrCompositeProfile,
    torsionRouteRequiresGeVUnitNormalization,
    torsionRouteProvidesGuLocalWzTheorem,
    torsionRouteProvidesSeparateWzSourceRows,
    torsionRouteProvidesWzRawAmplitudeGates,
    torsionRouteProvidesWzCommonBridgeGate,
    torsionRouteProvidesTargetIndependentGuVevSource,
    torsionRouteProvidesGuWeakMixingAngleSource,
    torsionRouteProvidesGuGaugeCouplingNormalization,
    torsionRouteProvidesObservedPhotonWzProjectionRows,
    torsionRouteProvidesGuObservedFieldExtraction,
    torsionRouteProvidesGuHiggsScalarSourceOperator,
    torsionRouteProvidesGuHiggsQuarticOrExcitationSource,
    torsionRouteProvidesObservedHiggsMassFromGu,
    torsionRouteProvidesGeVUnitNormalization,
    torsionRoutePromotesWzMasses,
    torsionRoutePromotesHiggsMass,
    torsionRouteCompletesBosonPredictions,
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
    decision = "Do not promote W/Z or Higgs physical masses from the metric-affine or Einstein-Cartan torsion route in this repository. The route is a serious external geometric EWSB lead, especially through torsion/Holst four-fermion condensation and constrained 3BF electroweak symmetry breaking, but it still lacks a GU-local torsion bridge theorem, source-derived torsion or Immirzi scale, technifermion representation and condensate lineage, observed photon/W/Z/H projection, weak-angle and VEV lineage, Higgs scalar-source lineage, precision matching, and GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem mapping augmented torsion, contorsion, or metric-affine/Holst structures into observed electroweak W/Z source rows.",
        "A target-independent source for the torsion/Holst/Immirzi scale and any technifermion representation content.",
        "Condensate dynamics, weak-angle lineage, VEV or scale normalization, and precision-electroweak matching that do not import W, Z, or Higgs targets.",
        "Observed photon/W/Z/H projection rows plus a Higgs scalar-source operator or composite scalar profile at the same vacuum.",
        "Physical-unit normalization and validation through Phase201/Phase256 without weakening existing gates."
    }
};

var fullPath = Path.Combine(outputDir, "metric_affine_torsion_source_audit.json");
var summaryPath = Path.Combine(outputDir, "metric_affine_torsion_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.metricAffineTorsionSourceAuditPassed,
    result.metricAffineTorsionLeadPresent,
    result.metricAffineTorsionPrimarySourcesReviewed,
    result.metricAffineTorsionRouteExternalToGu,
    result.torsionRouteUsesEinsteinCartanOrPoincareGravity,
    result.torsionRouteUsesHolstPalatiniAction,
    result.torsionRouteUsesNonminimalSpinorTorsionCoupling,
    result.torsionRouteGeneratesEffectiveFourFermionInteractions,
    result.torsionRouteUsesParityBreakingTorsionCouplings,
    result.torsionRouteCanInduceTechnifermionCondensation,
    result.torsionRouteCanGenerateGaugeBosonMassesViaCondensate,
    result.torsionRouteRequiresTevScaleTorsionMassParameter,
    result.torsionRouteUsesNjlOrEnjlCondensateDynamics,
    result.torsionRouteIntroducesAdditionalTechnifermions,
    result.torsionRouteNeedsPrecisionElectroweakMatching,
    result.threeBfRouteUsesStandardModelEinsteinCartanThreeGroup,
    result.threeBfRouteStudiesExplicitAndSpontaneousSymmetryBreaking,
    result.threeBfRouteFormulatesElectroweakHiggsMechanism,
    result.threeBfRouteFormulatesProcaMassTerms,
    result.threeBfRouteRecoversTextbookElectroweakMassStructure,
    result.threeBfRouteUsesVevGaugeCouplingsAndHiggsQuarticAsParameters,
    result.metricAffineScalarHiggsContextPresent,
    result.metricAffineElectroweakVacuumStabilityContextPresent,
    result.torsionRouteOverlapsTechnicolorButAddsGeometricContactSource,
    result.torsionRouteRelatedToGuAugmentedTorsionHooks,
    result.torsionRouteDistinctFromPhase337OctonionClifford,
    result.torsionRouteRequiresGuLocalMetricAffineEinsteinCartanMap,
    result.torsionRouteRequiresTorsionHolstScaleOrImmirziSource,
    result.torsionRouteRequiresTechnifermionRepresentationSource,
    result.torsionRouteRequiresCondensateDynamicsAndNormalization,
    result.torsionRouteRequiresElectroweakEmbeddingAndPrecisionMatching,
    result.torsionRouteRequiresObservedPhotonWzProjection,
    result.torsionRouteRequiresVevOrScaleLineage,
    result.torsionRouteRequiresWeakAngleLineage,
    result.torsionRouteRequiresHiggsScalarSourceOrCompositeProfile,
    result.torsionRouteRequiresGeVUnitNormalization,
    result.torsionRouteProvidesGuLocalWzTheorem,
    result.torsionRouteProvidesSeparateWzSourceRows,
    result.torsionRouteProvidesTargetIndependentGuVevSource,
    result.torsionRouteProvidesGuWeakMixingAngleSource,
    result.torsionRouteProvidesGuGaugeCouplingNormalization,
    result.torsionRouteProvidesObservedPhotonWzProjectionRows,
    result.torsionRouteProvidesGuObservedFieldExtraction,
    result.torsionRouteProvidesGuHiggsScalarSourceOperator,
    result.torsionRouteProvidesGuHiggsQuarticOrExcitationSource,
    result.torsionRouteProvidesObservedHiggsMassFromGu,
    result.torsionRouteProvidesGeVUnitNormalization,
    result.torsionRoutePromotesWzMasses,
    result.torsionRoutePromotesHiggsMass,
    result.torsionRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"metricAffineTorsionSourceAuditPassed={metricAffineTorsionSourceAuditPassed}");
Console.WriteLine($"torsionRouteCanGenerateGaugeBosonMassesViaCondensate={torsionRouteCanGenerateGaugeBosonMassesViaCondensate}");
Console.WriteLine($"threeBfRouteRecoversTextbookElectroweakMassStructure={threeBfRouteRecoversTextbookElectroweakMassStructure}");
Console.WriteLine($"torsionRoutePromotesWzMasses={torsionRoutePromotesWzMasses}");
Console.WriteLine($"torsionRoutePromotesHiggsMass={torsionRoutePromotesHiggsMass}");
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
