using System.Text.Json;

const string DefaultOutputDir = "studies/phase334_su21_superconnection_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase265Path = "studies/phase265_gauge_higgs_boundary_source_audit_001/output/gauge_higgs_boundary_source_audit_summary.json";
const string Phase268Path = "studies/phase268_spectral_action_boson_source_audit_001/output/spectral_action_boson_source_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase333Path = "studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/output/kaluza_klein_internal_symmetry_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE334_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase265 = JsonDocument.Parse(File.ReadAllText(Phase265Path));
using var phase268 = JsonDocument.Parse(File.ReadAllText(Phase268Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase333 = JsonDocument.Parse(File.ReadAllText(Phase333Path));

const string roepstorffSuperconnectionUrl = "https://arxiv.org/abs/hep-th/9801040";
const string loginovSu21GaugeHiggsUrl = "https://arxiv.org/abs/1012.3692";
const string aydemirSuperconnectionLrsmUrl = "https://arxiv.org/abs/1409.7574";
const string su21ReviewUrl = "https://doi.org/10.1016/j.physrep.2004.10.003";
const string coquereauxSu21SuperconnectionUrl = "https://doi.org/10.1016/0370-2693(91)90979-Z";

const double classicTreeLevelSinSquaredTheta = 0.25;
const double classicTreeLevelMzOverMw = 2.0 / 1.7320508075688772;
const double classicTreeLevelMhOverMw = 2.0;
const string classicTreeLevelMassSquaredRatio = "3:4:12";
const double loginovTreeLevelMhFromMwGeV = 131.267;
const double loginovOneLoopMhGeV = 126.58;
const double loginovTwoLoopMhGeV = 125.77;
const double aydemirClassicSu21HiggsEstimateGeV = 170.0;
const double aydemirSu22LrsmHiggsEstimateGeV = 126.0;

const bool su21SuperconnectionLeadPresent = true;
const bool su21PrimarySourcesReviewed = true;
const bool su21RouteExternalToGu = true;
const bool su21RouteGeometricHiggsAsConnectionBased = true;
const bool su21RouteEmbedsElectroweakGroupInGradedAlgebra = true;
const bool su21RouteProvidesTreeLevelWzHMassRatio = true;
const bool su21RouteProvidesExternalWeinbergAngleRelation = true;
const bool su21RouteClaimsQuarticHiggsCouplingConstraint = true;
const bool su21RouteUnifiesGaugeAndHiggsFields = true;
const bool su21RouteIncludesDiscreteExtraDimensionLead = true;
const bool su21ClassicTreeLevelHiggsPredictionConflictsWithObservedMass = true;
const bool su21ModifiedGaugeHiggsClaimNearObservedHiggsPresent = true;
const bool su21LeftRightExtensionClaimNearObservedHiggsPresent = true;
const bool su21RouteDistinctFromSpectralAction = true;
const bool su21RouteDistinctFromKaluzaKleinInternalSymmetry = true;
const bool su21RouteRequiresSpecificSuperalgebraChoice = true;
const bool su21RouteRequiresSupertraceOrNormalizationConvention = true;
const bool su21RouteRequiresElectroweakScaleMatching = true;
const bool su21RouteRequiresLoopEffectivePotentialAndTopInputs = true;
const bool su21RouteRequiresModelExtensionForObservedHiggs = true;
const bool su21RouteRequiresObservedElectroweakInputs = true;
const bool su21RouteRequiresRgTransport = true;

const bool su21RouteProvidesGuLocalWzTheorem = false;
const bool su21RouteProvidesSeparateWzSourceRows = false;
const bool su21RouteProvidesWzRawAmplitudeGates = false;
const bool su21RouteProvidesWzCommonBridgeGate = false;
const bool su21RouteProvidesTargetIndependentGuVevSource = false;
const bool su21RouteProvidesGuWeakMixingAngleSource = false;
const bool su21RouteProvidesGuGaugeCouplingNormalization = false;
const bool su21RouteProvidesObservedPhotonWzProjectionRows = false;
const bool su21RouteProvidesNeutralMassMatrixDiagonalization = false;
const bool su21RouteProvidesGuObservedFieldExtraction = false;
const bool su21RouteProvidesGuHiggsScalarSourceOperator = false;
const bool su21RouteProvidesGuHiggsQuarticOrExcitationSource = false;
const bool su21RouteProvidesObservedHiggsMassFromGu = false;
const bool su21RouteProvidesGeVUnitNormalization = false;
const bool su21RoutePromotesWzMasses = false;
const bool su21RoutePromotesHiggsMass = false;
const bool su21RouteCompletesBosonPredictions = false;
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
var lowEnergyRgTransportSourcePromotable = JsonBool(phase236.RootElement, "lowEnergyRgTransportSourcePromotable") is true;
var gaugeHiggsBoundarySourceAuditPassed = JsonBool(phase265.RootElement, "gaugeHiggsBoundarySourceAuditPassed") is true;
var gaugeHiggsBoundaryPromotesHiggsMass = JsonBool(phase265.RootElement, "gaugeHiggsBoundaryPromotesHiggsMass") is true;
var spectralActionBosonSourceAuditPassed = JsonBool(phase268.RootElement, "spectralActionBosonSourceAuditPassed") is true;
var spectralActionPromotesWzMasses = JsonBool(phase268.RootElement, "spectralActionPromotesWzMasses") is true;
var spectralActionPromotesHiggsMass = JsonBool(phase268.RootElement, "spectralActionPromotesHiggsMass") is true;
var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;
var kaluzaKleinInternalSymmetrySourceAuditPassed = JsonBool(phase333.RootElement, "kaluzaKleinInternalSymmetrySourceAuditPassed") is true;
var kkRoutePromotesWzMasses = JsonBool(phase333.RootElement, "kkRoutePromotesWzMasses") is true;
var kkRoutePromotesHiggsMass = JsonBool(phase333.RootElement, "kkRoutePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-hep-th-9801040-roepstorff-superconnections",
        roepstorffSuperconnectionUrl,
        "Quillen superconnection Higgs-as-connection route",
        "The U(2) model gives sin^2(theta)=1/4 and the tree-level mass-squared ratio m_W^2:m_Z^2:m_H^2=3:4:12.",
        "This is a direct geometric W/Z/H ratio lead, but it is external to GU, tree-level, and does not supply GU source rows or observed-field extraction."),
    new SourceRow(
        "arxiv-1012-3692-loginov-su21-gauge-higgs",
        loginovSu21GaugeHiggsUrl,
        "SU(2/1) gauge-Higgs unification",
        "The route embeds SU(2)xU(1)_Y in SU(2/1), fixes the Higgs quartic coupling, and claims a two-loop Higgs value near 125.77 GeV.",
        "The near value depends on the model's electroweak-scale matching, effective-potential/RG treatment, and top/electroweak inputs, not on a GU-local source law."),
    new SourceRow(
        "arxiv-1409-7574-superconnection-lrsm",
        aydemirSuperconnectionLrsmUrl,
        "superconnection Higgs mass and left-right extension",
        "The paper records that the minimal SU(2/1) superconnection Higgs estimate is around 170 GeV and uses an SU(2/2) left-right extension to accommodate a near-126 GeV Higgs.",
        "The extension is a model-building repair, not a GU-local prediction artifact."),
    new SourceRow(
        "doi-10-1016-j-physrep-2004-10-003-su21-review",
        su21ReviewUrl,
        "review of electroweak SU(2/1) superconnections",
        "The review summarizes electroweak isospin/hypercharge spectrum, Weinberg-angle, and Higgs-mass predictions from SU(2/1) embedding.",
        "It is useful source context but does not provide repository source-lineage rows."),
    new SourceRow(
        "doi-10-1016-0370-2693-91-90979-z-su21-generalized-electroweak",
        coquereauxSu21SuperconnectionUrl,
        "algebraic superconnection generalized electroweak model",
        "The source unifies the usual gauge fields and Higgs fields in one generalized Yang-Mills field or superconnection.",
        "It supports the geometric-Higgs route but remains external and model dependent."),
};

var checks = new[]
{
    new Check(
        "su21-superconnection-primary-sources-reviewed",
        su21SuperconnectionLeadPresent
            && su21PrimarySourcesReviewed
            && su21RouteExternalToGu
            && su21RouteGeometricHiggsAsConnectionBased
            && sourceRows.Length == 5,
        $"lead={su21SuperconnectionLeadPresent}; primaryReviewed={su21PrimarySourcesReviewed}; externalToGu={su21RouteExternalToGu}; geometricHiggsConnection={su21RouteGeometricHiggsAsConnectionBased}; sourceRows={sourceRows.Length}"),
    new Check(
        "su21-route-captures-direct-wzh-geometry-lead",
        su21RouteEmbedsElectroweakGroupInGradedAlgebra
            && su21RouteProvidesTreeLevelWzHMassRatio
            && su21RouteProvidesExternalWeinbergAngleRelation
            && su21RouteClaimsQuarticHiggsCouplingConstraint
            && su21RouteUnifiesGaugeAndHiggsFields
            && su21RouteIncludesDiscreteExtraDimensionLead,
        $"gradedElectroweak={su21RouteEmbedsElectroweakGroupInGradedAlgebra}; ratio={classicTreeLevelMassSquaredRatio}; sin2Theta={classicTreeLevelSinSquaredTheta}; quartic={su21RouteClaimsQuarticHiggsCouplingConstraint}; gaugeHiggsUnified={su21RouteUnifiesGaugeAndHiggsFields}; discreteExtraDimension={su21RouteIncludesDiscreteExtraDimensionLead}"),
    new Check(
        "su21-route-higgs-claims-are-not-current-gu-source-lineage",
        su21ClassicTreeLevelHiggsPredictionConflictsWithObservedMass
            && su21ModifiedGaugeHiggsClaimNearObservedHiggsPresent
            && su21LeftRightExtensionClaimNearObservedHiggsPresent
            && gaugeHiggsBoundarySourceAuditPassed
            && !gaugeHiggsBoundaryPromotesHiggsMass
            && spectralActionBosonSourceAuditPassed
            && !spectralActionPromotesWzMasses
            && !spectralActionPromotesHiggsMass,
        $"classicSu21HiggsGeV~{aydemirClassicSu21HiggsEstimateGeV}; loginovTwoLoopHiggsGeV={loginovTwoLoopMhGeV}; su22LrsmHiggsGeV~{aydemirSu22LrsmHiggsEstimateGeV}; p265={gaugeHiggsBoundarySourceAuditPassed}; p268={spectralActionBosonSourceAuditPassed}"),
    new Check(
        "su21-route-remains-external-and-conditional",
        su21RouteDistinctFromSpectralAction
            && su21RouteDistinctFromKaluzaKleinInternalSymmetry
            && su21RouteRequiresSpecificSuperalgebraChoice
            && su21RouteRequiresSupertraceOrNormalizationConvention
            && su21RouteRequiresElectroweakScaleMatching
            && su21RouteRequiresLoopEffectivePotentialAndTopInputs
            && su21RouteRequiresModelExtensionForObservedHiggs
            && su21RouteRequiresObservedElectroweakInputs
            && su21RouteRequiresRgTransport,
        $"distinctSpectral={su21RouteDistinctFromSpectralAction}; distinctKk={su21RouteDistinctFromKaluzaKleinInternalSymmetry}; superalgebra={su21RouteRequiresSpecificSuperalgebraChoice}; normalization={su21RouteRequiresSupertraceOrNormalizationConvention}; ewScale={su21RouteRequiresElectroweakScaleMatching}; loopsTop={su21RouteRequiresLoopEffectivePotentialAndTopInputs}; extension={su21RouteRequiresModelExtensionForObservedHiggs}; observedInputs={su21RouteRequiresObservedElectroweakInputs}; rg={su21RouteRequiresRgTransport}"),
    new Check(
        "su21-route-does-not-supply-gu-wz-source-lineage",
        electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !lowEnergyRgTransportSourcePromotable
            && electroweakMassMatrixBridgeSourceAuditPassed
            && !smMassMatrixPromotesWzMasses
            && !su21RouteProvidesGuLocalWzTheorem
            && !su21RouteProvidesSeparateWzSourceRows
            && !su21RouteProvidesWzRawAmplitudeGates
            && !su21RouteProvidesWzCommonBridgeGate
            && !su21RouteProvidesTargetIndependentGuVevSource
            && !su21RouteProvidesGuWeakMixingAngleSource
            && !su21RouteProvidesGuGaugeCouplingNormalization
            && !su21RouteProvidesObservedPhotonWzProjectionRows
            && !su21RouteProvidesNeutralMassMatrixDiagonalization
            && !su21RoutePromotesWzMasses
            && !canFillPhase201WzContract,
        $"p224={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; rg={lowEnergyRgTransportSourcePromotable}; p317={electroweakMassMatrixBridgeSourceAuditPassed}; smPromotesWz={smMassMatrixPromotesWzMasses}; guWzTheorem={su21RouteProvidesGuLocalWzTheorem}; separateRows={su21RouteProvidesSeparateWzSourceRows}; guWeakAngle={su21RouteProvidesGuWeakMixingAngleSource}; promotesWz={su21RoutePromotesWzMasses}"),
    new Check(
        "su21-route-does-not-supply-gu-higgs-source-lineage",
        !higgsMassParameterClosure
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && !smMassMatrixPromotesHiggsMass
            && kaluzaKleinInternalSymmetrySourceAuditPassed
            && !kkRoutePromotesWzMasses
            && !kkRoutePromotesHiggsMass
            && !su21RouteProvidesGuObservedFieldExtraction
            && !su21RouteProvidesGuHiggsScalarSourceOperator
            && !su21RouteProvidesGuHiggsQuarticOrExcitationSource
            && !su21RouteProvidesObservedHiggsMassFromGu
            && !su21RoutePromotesHiggsMass
            && !canFillPhase201HiggsContract,
        $"higgsClosure={higgsMassParameterClosure}; p323={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Completes={coupledYangMillsHiggsRouteCompletesBosonPredictions}; smPromotesHiggs={smMassMatrixPromotesHiggsMass}; p333={kaluzaKleinInternalSymmetrySourceAuditPassed}; observedExtraction={su21RouteProvidesGuObservedFieldExtraction}; higgsOperator={su21RouteProvidesGuHiggsScalarSourceOperator}; higgsQuartic={su21RouteProvidesGuHiggsQuarticOrExcitationSource}; observedHiggsGu={su21RouteProvidesObservedHiggsMassFromGu}; promotesHiggs={su21RoutePromotesHiggsMass}"),
    new Check(
        "source-lineage-contracts-remain-unfilled-after-su21-audit",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !su21RouteProvidesGeVUnitNormalization
            && !su21RouteCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; geV={su21RouteProvidesGeVUnitNormalization}; completes={su21RouteCompletesBosonPredictions}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
};

var su21SuperconnectionSourceAuditPassed = checks.All(check => check.Passed)
    && su21SuperconnectionLeadPresent
    && su21PrimarySourcesReviewed
    && su21RouteExternalToGu
    && su21RouteGeometricHiggsAsConnectionBased
    && su21RouteProvidesTreeLevelWzHMassRatio
    && su21RouteProvidesExternalWeinbergAngleRelation
    && !su21RouteProvidesGuLocalWzTheorem
    && !su21RouteProvidesSeparateWzSourceRows
    && !su21RouteProvidesTargetIndependentGuVevSource
    && !su21RouteProvidesGuWeakMixingAngleSource
    && !su21RouteProvidesGuGaugeCouplingNormalization
    && !su21RouteProvidesGuObservedFieldExtraction
    && !su21RouteProvidesGuHiggsScalarSourceOperator
    && !su21RouteProvidesGuHiggsQuarticOrExcitationSource
    && !su21RouteProvidesObservedHiggsMassFromGu
    && !su21RouteProvidesGeVUnitNormalization
    && !su21RoutePromotesWzMasses
    && !su21RoutePromotesHiggsMass
    && !su21RouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = su21SuperconnectionSourceAuditPassed
    ? "su21-superconnection-source-audit-geometric-ratio-lead-not-gu-source"
    : "su21-superconnection-source-audit-review-required";

var result = new
{
    phaseId = "phase334-su21-superconnection-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    su21SuperconnectionSourceAuditPassed,
    su21SuperconnectionLeadPresent,
    su21PrimarySourcesReviewed,
    su21RouteExternalToGu,
    su21RouteGeometricHiggsAsConnectionBased,
    su21RouteEmbedsElectroweakGroupInGradedAlgebra,
    su21RouteProvidesTreeLevelWzHMassRatio,
    su21RouteProvidesExternalWeinbergAngleRelation,
    su21RouteClaimsQuarticHiggsCouplingConstraint,
    su21RouteUnifiesGaugeAndHiggsFields,
    su21RouteIncludesDiscreteExtraDimensionLead,
    su21ClassicTreeLevelHiggsPredictionConflictsWithObservedMass,
    su21ModifiedGaugeHiggsClaimNearObservedHiggsPresent,
    su21LeftRightExtensionClaimNearObservedHiggsPresent,
    su21RouteDistinctFromSpectralAction,
    su21RouteDistinctFromKaluzaKleinInternalSymmetry,
    su21RouteRequiresSpecificSuperalgebraChoice,
    su21RouteRequiresSupertraceOrNormalizationConvention,
    su21RouteRequiresElectroweakScaleMatching,
    su21RouteRequiresLoopEffectivePotentialAndTopInputs,
    su21RouteRequiresModelExtensionForObservedHiggs,
    su21RouteRequiresObservedElectroweakInputs,
    su21RouteRequiresRgTransport,
    su21RouteProvidesGuLocalWzTheorem,
    su21RouteProvidesSeparateWzSourceRows,
    su21RouteProvidesWzRawAmplitudeGates,
    su21RouteProvidesWzCommonBridgeGate,
    su21RouteProvidesTargetIndependentGuVevSource,
    su21RouteProvidesGuWeakMixingAngleSource,
    su21RouteProvidesGuGaugeCouplingNormalization,
    su21RouteProvidesObservedPhotonWzProjectionRows,
    su21RouteProvidesNeutralMassMatrixDiagonalization,
    su21RouteProvidesGuObservedFieldExtraction,
    su21RouteProvidesGuHiggsScalarSourceOperator,
    su21RouteProvidesGuHiggsQuarticOrExcitationSource,
    su21RouteProvidesObservedHiggsMassFromGu,
    su21RouteProvidesGeVUnitNormalization,
    su21RoutePromotesWzMasses,
    su21RoutePromotesHiggsMass,
    su21RouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    numericalLead = new
    {
        classicTreeLevelSinSquaredTheta,
        classicTreeLevelMzOverMw,
        classicTreeLevelMhOverMw,
        classicTreeLevelMassSquaredRatio,
        loginovTreeLevelMhFromMwGeV,
        loginovOneLoopMhGeV,
        loginovTwoLoopMhGeV,
        aydemirClassicSu21HiggsEstimateGeV,
        aydemirSu22LrsmHiggsEstimateGeV,
    },
    sourceRows,
    checks,
    currentBlockerEvidence = new
    {
        phase201AllRequiredLineagesPromotable,
        existingEvidenceFound,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
        electroweakParameterAuditPassed,
        wAbsoluteMassParameterClosure,
        zAbsoluteMassParameterClosure,
        higgsMassParameterClosure,
        lowEnergyRgTransportSourcePromotable,
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
    },
    decision = su21SuperconnectionSourceAuditPassed
        ? "Do not promote W/Z or Higgs physical masses from the SU(2/1) superconnection route in this repository. The route is a serious geometric W/Z/H ratio and Higgs-as-connection lead, but it is external to GU and does not supply GU-local W/Z source rows, observed photon/W/Z/H projection, a target-independent GU VEV or scale source, GU weak-angle and coupling normalization, Higgs scalar-source/self-coupling lineage, or GeV normalization."
        : "Review the SU(2/1) superconnection route before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local superconnection or equivalent theorem mapping GU fields to observed photon/W/Z/H rows.",
        "Separate W and Z source rows with weak-angle, neutral mixing, photon projection, and physical-unit normalization independent of target values.",
        "A target-independent GU weak-scale source and low-energy transport theorem that does not import electroweak measured inputs.",
        "A solved GU Higgs scalar source/operator and self-coupling/excitation lineage if the Higgs-as-connection route is to supply the physical Higgs.",
    },
    sourceEvidence = new
    {
        roepstorffSuperconnectionUrl,
        loginovSu21GaugeHiggsUrl,
        aydemirSuperconnectionLrsmUrl,
        su21ReviewUrl,
        coquereauxSu21SuperconnectionUrl,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase256Path = Phase256Path,
        phase265Path = Phase265Path,
        phase268Path = Phase268Path,
        phase317Path = Phase317Path,
        phase323Path = Phase323Path,
        phase333Path = Phase333Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "su21_superconnection_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "su21_superconnection_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.su21SuperconnectionSourceAuditPassed,
            result.su21SuperconnectionLeadPresent,
            result.su21PrimarySourcesReviewed,
            result.su21RouteExternalToGu,
            result.su21RouteGeometricHiggsAsConnectionBased,
            result.su21RouteEmbedsElectroweakGroupInGradedAlgebra,
            result.su21RouteProvidesTreeLevelWzHMassRatio,
            result.su21RouteProvidesExternalWeinbergAngleRelation,
            result.su21RouteClaimsQuarticHiggsCouplingConstraint,
            result.su21RouteUnifiesGaugeAndHiggsFields,
            result.su21ClassicTreeLevelHiggsPredictionConflictsWithObservedMass,
            result.su21ModifiedGaugeHiggsClaimNearObservedHiggsPresent,
            result.su21LeftRightExtensionClaimNearObservedHiggsPresent,
            result.su21RouteRequiresSpecificSuperalgebraChoice,
            result.su21RouteRequiresSupertraceOrNormalizationConvention,
            result.su21RouteRequiresElectroweakScaleMatching,
            result.su21RouteRequiresLoopEffectivePotentialAndTopInputs,
            result.su21RouteRequiresModelExtensionForObservedHiggs,
            result.su21RouteRequiresObservedElectroweakInputs,
            result.su21RouteRequiresRgTransport,
            result.su21RouteProvidesGuLocalWzTheorem,
            result.su21RouteProvidesSeparateWzSourceRows,
            result.su21RouteProvidesTargetIndependentGuVevSource,
            result.su21RouteProvidesGuWeakMixingAngleSource,
            result.su21RouteProvidesGuGaugeCouplingNormalization,
            result.su21RouteProvidesGuObservedFieldExtraction,
            result.su21RouteProvidesGuHiggsScalarSourceOperator,
            result.su21RouteProvidesGuHiggsQuarticOrExcitationSource,
            result.su21RouteProvidesObservedHiggsMassFromGu,
            result.su21RouteProvidesGeVUnitNormalization,
            result.su21RoutePromotesWzMasses,
            result.su21RoutePromotesHiggsMass,
            result.su21RouteCompletesBosonPredictions,
            result.canFillPhase201WzContract,
            result.canFillPhase201HiggsContract,
            result.canFillPhase256ObservedFieldExtractionContract,
            result.numericalLead,
            sourceRowCount = result.sourceRows.Length,
            result.contractImpact,
            result.decision,
            result.nextRequiredArtifact,
        },
        options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"su21SuperconnectionSourceAuditPassed={su21SuperconnectionSourceAuditPassed}");
Console.WriteLine($"su21RouteGeometricHiggsAsConnectionBased={su21RouteGeometricHiggsAsConnectionBased}");
Console.WriteLine($"su21RouteProvidesTreeLevelWzHMassRatio={su21RouteProvidesTreeLevelWzHMassRatio}");
Console.WriteLine($"su21RoutePromotesWzMasses={su21RoutePromotesWzMasses}");
Console.WriteLine($"su21RoutePromotesHiggsMass={su21RoutePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value))
    {
        return null;
    }

    return value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var intValue)
        ? intValue
        : null;
}

sealed record SourceRow(string SourceId, string Url, string Scope, string Finding, string PredictionImpact);
sealed record Check(string CheckId, bool Passed, string Detail);
