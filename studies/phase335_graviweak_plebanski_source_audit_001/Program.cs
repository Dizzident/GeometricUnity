using System.Text.Json;

const string DefaultOutputDir = "studies/phase335_graviweak_plebanski_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase236Path = "studies/phase236_low_energy_rg_transport_source_audit_001/output/low_energy_rg_transport_source_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase333Path = "studies/phase333_kaluza_klein_internal_symmetry_source_audit_001/output/kaluza_klein_internal_symmetry_source_audit_summary.json";
const string Phase334Path = "studies/phase334_su21_superconnection_source_audit_001/output/su21_superconnection_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE335_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase236 = JsonDocument.Parse(File.ReadAllText(Phase236Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase333 = JsonDocument.Parse(File.ReadAllText(Phase333Path));
using var phase334 = JsonDocument.Parse(File.ReadAllText(Phase334Path));

const string nestiPercacciGraviweakUrl = "https://arxiv.org/abs/0706.3307";
const string dasLaperashviliTureanuGraviweakUrl = "https://arxiv.org/abs/1304.3069";
const string froggattDasLaperashviliNielsenTureanuMppUrl = "https://arxiv.org/abs/1311.4413";
const string dasLaperashviliInflationUrl = "https://arxiv.org/abs/1409.1115";
const string krasnovPercacciReviewUrl = "https://arxiv.org/abs/1712.03061";

const double reducedPlanckScaleGeV = 2.43e18;
const double standardHiggsVevInputGeV = 246.0;
const double mirrorHiggsVevScaleFactorLead = 100.0;
const double mppHiggsMassLowerGeV = 125.0;
const double mppHiggsMassUpperGeV = 143.0;
const double darkEnergySusyScaleLeadGeV = 1.0e10;

const bool graviweakPlebanskiLeadPresent = true;
const bool graviweakPrimarySourcesReviewed = true;
const bool graviweakRouteExternalToGu = true;
const bool graviweakRouteUnifiesGravityAndWeakSu2 = true;
const bool graviweakRouteUsesExtendedPlebanskiAction = true;
const bool graviweakRouteUsesSpin44GaugeSymmetry = true;
const bool graviweakRouteRecoversGravityYangMillsHiggsAction = true;
const bool graviweakRouteTreatsHiggsAsFrameHiggsConnection = true;
const bool graviweakRouteRelatesBareCouplingsToUnifiedParameterAndVev = true;
const bool graviweakRouteProvidesPlanckScaleWeakGravityCouplingRelation = true;
const bool graviweakRouteLeavesWFieldsMasslessBeforeElectroweakBreaking = true;
const bool graviweakRouteIncludesMppHiggsMassIntervalLead = true;
const bool graviweakRouteIncludesMirrorSectorScaleLead = true;
const bool graviweakRouteDistinctFromSu21Superconnection = true;
const bool graviweakRouteDistinctFromKaluzaKleinInternalSymmetry = true;
const bool graviweakRouteRequiresSpecificGaugeGroupChoice = true;
const bool graviweakRouteRequiresSymmetryBreakingAnsatz = true;
const bool graviweakRouteRequiresHiggsVevOrCurvatureVacuum = true;
const bool graviweakRouteRequiresElectroweakBreakingMechanism = true;
const bool graviweakRouteRequiresPlanckScaleBareParameterMatching = true;
const bool graviweakRouteRequiresRunningFromPlanckToElectroweakScale = true;
const bool graviweakRouteRequiresObservedGnAndVevInputs = true;
const bool graviweakRouteRequiresMppCriticalityAssumptions = true;
const bool graviweakRouteRequiresFermionHyperchargeAndSmCompletion = true;

const bool graviweakRouteProvidesGuLocalWzTheorem = false;
const bool graviweakRouteProvidesSeparateWzSourceRows = false;
const bool graviweakRouteProvidesWzRawAmplitudeGates = false;
const bool graviweakRouteProvidesWzCommonBridgeGate = false;
const bool graviweakRouteProvidesTargetIndependentGuVevSource = false;
const bool graviweakRouteProvidesGuWeakMixingAngleSource = false;
const bool graviweakRouteProvidesGuGaugeCouplingNormalization = false;
const bool graviweakRouteProvidesObservedPhotonWzProjectionRows = false;
const bool graviweakRouteProvidesNeutralMassMatrixDiagonalization = false;
const bool graviweakRouteProvidesGuObservedFieldExtraction = false;
const bool graviweakRouteProvidesGuHiggsScalarSourceOperator = false;
const bool graviweakRouteProvidesGuHiggsQuarticOrExcitationSource = false;
const bool graviweakRouteProvidesObservedHiggsMassFromGu = false;
const bool graviweakRouteProvidesGeVUnitNormalization = false;
const bool graviweakRoutePromotesWzMasses = false;
const bool graviweakRoutePromotesHiggsMass = false;
const bool graviweakRouteCompletesBosonPredictions = false;
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
var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var coupledYangMillsHiggsRouteCompletesBosonPredictions = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRouteCompletesBosonPredictions") is true;
var kaluzaKleinInternalSymmetrySourceAuditPassed = JsonBool(phase333.RootElement, "kaluzaKleinInternalSymmetrySourceAuditPassed") is true;
var kkRoutePromotesWzMasses = JsonBool(phase333.RootElement, "kkRoutePromotesWzMasses") is true;
var su21SuperconnectionSourceAuditPassed = JsonBool(phase334.RootElement, "su21SuperconnectionSourceAuditPassed") is true;
var su21RouteCompletesBosonPredictions = JsonBool(phase334.RootElement, "su21RouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-0706-3307-nesti-percacci-graviweak",
        nestiPercacciGraviweakUrl,
        "graviweak unification from selfdual/antiselfdual connection split",
        "The route identifies the antiselfdual part of the Lorentz connection with weak SU(2) and leaves the W gauge fields massless until lower-scale electroweak breaking.",
        "This is a geometric gravity-weak unification lead, but it explicitly does not produce observed W/Z masses without an additional electroweak-breaking source."),
    new SourceRow(
        "arxiv-1304-3069-das-laperashvili-tureanu-spin44",
        dasLaperashviliTureanuGraviweakUrl,
        "Spin(4,4) extended Plebanski graviweak action",
        "The model recovers gravity, SU(2) Yang-Mills, and Higgs-field actions, with physical constants determined by a parameter and Higgs VEVs.",
        "It treats the VEV and Planck-scale matching as required inputs/solutions and does not supply GU observed-field rows."),
    new SourceRow(
        "arxiv-1311-4413-graviweak-mpp",
        froggattDasLaperashviliNielsenTureanuMppUrl,
        "graviweak unification with Multiple Point Principle",
        "The source relates Newton, cosmological, and weak couplings at the bare Planck scale and quotes an MPP Higgs-mass interval of 125-143 GeV.",
        "The Higgs interval is an MPP/SM effective-potential lead, not a GU-local Higgs scalar-source lineage."),
    new SourceRow(
        "arxiv-1409-1115-graviweak-inflation-visible-invisible",
        dasLaperashviliInflationUrl,
        "visible/invisible Spin(4,4) graviweak and Higgs false-vacuum route",
        "The source uses visible and mirror Higgs doublet VEVs, including a visible electroweak VEV near 246 GeV and a mirror scale factor lead.",
        "The VEVs are model inputs/assumptions for this repository's contract and cannot fill target-independent GU VEV lineage."),
    new SourceRow(
        "arxiv-1712-03061-krasnov-percacci-gravity-unification-review",
        krasnovPercacciReviewUrl,
        "review of gravity-Yang-Mills unification by enlarged internal/gauge bundles",
        "The review frames Plebanski and related formulations as four-dimensional gauge-gravity unification routes with enlarged internal spaces.",
        "It supplies context for the graviweak route but no W/Z/H physical prediction source rows."),
};

var checks = new[]
{
    new Check(
        "graviweak-plebanski-primary-sources-reviewed",
        graviweakPlebanskiLeadPresent
            && graviweakPrimarySourcesReviewed
            && graviweakRouteExternalToGu
            && sourceRows.Length == 5,
        $"lead={graviweakPlebanskiLeadPresent}; primaryReviewed={graviweakPrimarySourcesReviewed}; externalToGu={graviweakRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "graviweak-route-captures-geometric-gravity-weak-higgs-lead",
        graviweakRouteUnifiesGravityAndWeakSu2
            && graviweakRouteUsesExtendedPlebanskiAction
            && graviweakRouteUsesSpin44GaugeSymmetry
            && graviweakRouteRecoversGravityYangMillsHiggsAction
            && graviweakRouteTreatsHiggsAsFrameHiggsConnection
            && graviweakRouteRelatesBareCouplingsToUnifiedParameterAndVev
            && graviweakRouteProvidesPlanckScaleWeakGravityCouplingRelation,
        $"unifiesGravityWeak={graviweakRouteUnifiesGravityAndWeakSu2}; plebanski={graviweakRouteUsesExtendedPlebanskiAction}; spin44={graviweakRouteUsesSpin44GaugeSymmetry}; recoversActions={graviweakRouteRecoversGravityYangMillsHiggsAction}; frameHiggs={graviweakRouteTreatsHiggsAsFrameHiggsConnection}; bareRelation={graviweakRouteRelatesBareCouplingsToUnifiedParameterAndVev}; planckRelation={graviweakRouteProvidesPlanckScaleWeakGravityCouplingRelation}"),
    new Check(
        "graviweak-route-numerical-leads-are-not-current-gu-source-lineage",
        graviweakRouteLeavesWFieldsMasslessBeforeElectroweakBreaking
            && graviweakRouteIncludesMppHiggsMassIntervalLead
            && graviweakRouteIncludesMirrorSectorScaleLead
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && !coupledYangMillsHiggsRouteCompletesBosonPredictions
            && kaluzaKleinInternalSymmetrySourceAuditPassed
            && !kkRoutePromotesWzMasses
            && su21SuperconnectionSourceAuditPassed
            && !su21RouteCompletesBosonPredictions,
        $"wMasslessUntilEwBreaking={graviweakRouteLeavesWFieldsMasslessBeforeElectroweakBreaking}; mppHiggsRangeGeV={mppHiggsMassLowerGeV}-{mppHiggsMassUpperGeV}; mirrorScale={mirrorHiggsVevScaleFactorLead}; p323={coupledYangMillsHiggsMassExtractionAuditPassed}; p333={kaluzaKleinInternalSymmetrySourceAuditPassed}; p334={su21SuperconnectionSourceAuditPassed}"),
    new Check(
        "graviweak-route-remains-external-and-conditional",
        graviweakRouteDistinctFromSu21Superconnection
            && graviweakRouteDistinctFromKaluzaKleinInternalSymmetry
            && graviweakRouteRequiresSpecificGaugeGroupChoice
            && graviweakRouteRequiresSymmetryBreakingAnsatz
            && graviweakRouteRequiresHiggsVevOrCurvatureVacuum
            && graviweakRouteRequiresElectroweakBreakingMechanism
            && graviweakRouteRequiresPlanckScaleBareParameterMatching
            && graviweakRouteRequiresRunningFromPlanckToElectroweakScale
            && graviweakRouteRequiresObservedGnAndVevInputs
            && graviweakRouteRequiresMppCriticalityAssumptions
            && graviweakRouteRequiresFermionHyperchargeAndSmCompletion,
        $"distinctSu21={graviweakRouteDistinctFromSu21Superconnection}; distinctKk={graviweakRouteDistinctFromKaluzaKleinInternalSymmetry}; gaugeGroup={graviweakRouteRequiresSpecificGaugeGroupChoice}; breaking={graviweakRouteRequiresSymmetryBreakingAnsatz}; vevVacuum={graviweakRouteRequiresHiggsVevOrCurvatureVacuum}; ewBreaking={graviweakRouteRequiresElectroweakBreakingMechanism}; planckMatch={graviweakRouteRequiresPlanckScaleBareParameterMatching}; running={graviweakRouteRequiresRunningFromPlanckToElectroweakScale}; observedInputs={graviweakRouteRequiresObservedGnAndVevInputs}; mpp={graviweakRouteRequiresMppCriticalityAssumptions}; smCompletion={graviweakRouteRequiresFermionHyperchargeAndSmCompletion}"),
    new Check(
        "graviweak-route-does-not-supply-gu-wz-source-lineage",
        electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !lowEnergyRgTransportSourcePromotable
            && !graviweakRouteProvidesGuLocalWzTheorem
            && !graviweakRouteProvidesSeparateWzSourceRows
            && !graviweakRouteProvidesWzRawAmplitudeGates
            && !graviweakRouteProvidesWzCommonBridgeGate
            && !graviweakRouteProvidesTargetIndependentGuVevSource
            && !graviweakRouteProvidesGuWeakMixingAngleSource
            && !graviweakRouteProvidesGuGaugeCouplingNormalization
            && !graviweakRouteProvidesObservedPhotonWzProjectionRows
            && !graviweakRouteProvidesNeutralMassMatrixDiagonalization
            && !graviweakRoutePromotesWzMasses
            && !canFillPhase201WzContract,
        $"p224={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; rg={lowEnergyRgTransportSourcePromotable}; guWzTheorem={graviweakRouteProvidesGuLocalWzTheorem}; separateRows={graviweakRouteProvidesSeparateWzSourceRows}; targetVev={graviweakRouteProvidesTargetIndependentGuVevSource}; weakAngle={graviweakRouteProvidesGuWeakMixingAngleSource}; promotesWz={graviweakRoutePromotesWzMasses}"),
    new Check(
        "graviweak-route-does-not-supply-gu-higgs-source-lineage",
        !higgsMassParameterClosure
            && !graviweakRouteProvidesGuObservedFieldExtraction
            && !graviweakRouteProvidesGuHiggsScalarSourceOperator
            && !graviweakRouteProvidesGuHiggsQuarticOrExcitationSource
            && !graviweakRouteProvidesObservedHiggsMassFromGu
            && !graviweakRoutePromotesHiggsMass
            && !canFillPhase201HiggsContract,
        $"higgsClosure={higgsMassParameterClosure}; observedExtraction={graviweakRouteProvidesGuObservedFieldExtraction}; higgsOperator={graviweakRouteProvidesGuHiggsScalarSourceOperator}; higgsQuartic={graviweakRouteProvidesGuHiggsQuarticOrExcitationSource}; observedHiggsGu={graviweakRouteProvidesObservedHiggsMassFromGu}; promotesHiggs={graviweakRoutePromotesHiggsMass}"),
    new Check(
        "source-lineage-contracts-remain-unfilled-after-graviweak-audit",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && !graviweakRouteProvidesGeVUnitNormalization
            && !graviweakRouteCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; geV={graviweakRouteProvidesGeVUnitNormalization}; completes={graviweakRouteCompletesBosonPredictions}; canFillWz={canFillPhase201WzContract}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
};

var graviweakPlebanskiSourceAuditPassed = checks.All(check => check.Passed)
    && graviweakPlebanskiLeadPresent
    && graviweakPrimarySourcesReviewed
    && graviweakRouteExternalToGu
    && graviweakRouteUnifiesGravityAndWeakSu2
    && graviweakRouteUsesExtendedPlebanskiAction
    && graviweakRouteRecoversGravityYangMillsHiggsAction
    && !graviweakRouteProvidesGuLocalWzTheorem
    && !graviweakRouteProvidesSeparateWzSourceRows
    && !graviweakRouteProvidesTargetIndependentGuVevSource
    && !graviweakRouteProvidesGuWeakMixingAngleSource
    && !graviweakRouteProvidesGuGaugeCouplingNormalization
    && !graviweakRouteProvidesGuObservedFieldExtraction
    && !graviweakRouteProvidesGuHiggsScalarSourceOperator
    && !graviweakRouteProvidesGuHiggsQuarticOrExcitationSource
    && !graviweakRouteProvidesObservedHiggsMassFromGu
    && !graviweakRouteProvidesGeVUnitNormalization
    && !graviweakRoutePromotesWzMasses
    && !graviweakRoutePromotesHiggsMass
    && !graviweakRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = graviweakPlebanskiSourceAuditPassed
    ? "graviweak-plebanski-source-audit-geometric-unification-lead-not-gu-source"
    : "graviweak-plebanski-source-audit-review-required";

var result = new
{
    phaseId = "phase335-graviweak-plebanski-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    graviweakPlebanskiSourceAuditPassed,
    graviweakPlebanskiLeadPresent,
    graviweakPrimarySourcesReviewed,
    graviweakRouteExternalToGu,
    graviweakRouteUnifiesGravityAndWeakSu2,
    graviweakRouteUsesExtendedPlebanskiAction,
    graviweakRouteUsesSpin44GaugeSymmetry,
    graviweakRouteRecoversGravityYangMillsHiggsAction,
    graviweakRouteTreatsHiggsAsFrameHiggsConnection,
    graviweakRouteRelatesBareCouplingsToUnifiedParameterAndVev,
    graviweakRouteProvidesPlanckScaleWeakGravityCouplingRelation,
    graviweakRouteLeavesWFieldsMasslessBeforeElectroweakBreaking,
    graviweakRouteIncludesMppHiggsMassIntervalLead,
    graviweakRouteIncludesMirrorSectorScaleLead,
    graviweakRouteDistinctFromSu21Superconnection,
    graviweakRouteDistinctFromKaluzaKleinInternalSymmetry,
    graviweakRouteRequiresSpecificGaugeGroupChoice,
    graviweakRouteRequiresSymmetryBreakingAnsatz,
    graviweakRouteRequiresHiggsVevOrCurvatureVacuum,
    graviweakRouteRequiresElectroweakBreakingMechanism,
    graviweakRouteRequiresPlanckScaleBareParameterMatching,
    graviweakRouteRequiresRunningFromPlanckToElectroweakScale,
    graviweakRouteRequiresObservedGnAndVevInputs,
    graviweakRouteRequiresMppCriticalityAssumptions,
    graviweakRouteRequiresFermionHyperchargeAndSmCompletion,
    graviweakRouteProvidesGuLocalWzTheorem,
    graviweakRouteProvidesSeparateWzSourceRows,
    graviweakRouteProvidesWzRawAmplitudeGates,
    graviweakRouteProvidesWzCommonBridgeGate,
    graviweakRouteProvidesTargetIndependentGuVevSource,
    graviweakRouteProvidesGuWeakMixingAngleSource,
    graviweakRouteProvidesGuGaugeCouplingNormalization,
    graviweakRouteProvidesObservedPhotonWzProjectionRows,
    graviweakRouteProvidesNeutralMassMatrixDiagonalization,
    graviweakRouteProvidesGuObservedFieldExtraction,
    graviweakRouteProvidesGuHiggsScalarSourceOperator,
    graviweakRouteProvidesGuHiggsQuarticOrExcitationSource,
    graviweakRouteProvidesObservedHiggsMassFromGu,
    graviweakRouteProvidesGeVUnitNormalization,
    graviweakRoutePromotesWzMasses,
    graviweakRoutePromotesHiggsMass,
    graviweakRouteCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    numericalLead = new
    {
        reducedPlanckScaleGeV,
        standardHiggsVevInputGeV,
        mirrorHiggsVevScaleFactorLead,
        mppHiggsMassLowerGeV,
        mppHiggsMassUpperGeV,
        darkEnergySusyScaleLeadGeV,
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
    decision = graviweakPlebanskiSourceAuditPassed
        ? "Do not promote W/Z or Higgs physical masses from the graviweak/Plebanski route in this repository. The route is a serious external geometric gravity-weak-Higgs unification lead, but it leaves low-energy W masses to a later electroweak-breaking mechanism and does not supply GU-local W/Z source rows, observed photon/W/Z/H projection, a target-independent GU VEV or scale source, GU weak-angle and coupling normalization, Higgs scalar-source/self-coupling lineage, or GeV normalization."
        : "Review the graviweak/Plebanski route before relying on this boundary.",
    nextRequiredArtifact = new[]
    {
        "A GU-local gauge-gravity/Higgs theorem mapping GU fields to observed photon/W/Z/H rows.",
        "Separate W and Z source rows with electroweak breaking, weak-angle, neutral mixing, photon projection, and physical-unit normalization independent of target values.",
        "A target-independent GU vacuum/VEV and low-energy transport theorem that does not import GN, v, or electroweak measured inputs as source rows.",
        "A solved GU Higgs scalar source/operator and quartic or excitation lineage if a frame-Higgs connection route is to supply the physical Higgs.",
    },
    sourceEvidence = new
    {
        nestiPercacciGraviweakUrl,
        dasLaperashviliTureanuGraviweakUrl,
        froggattDasLaperashviliNielsenTureanuMppUrl,
        dasLaperashviliInflationUrl,
        krasnovPercacciReviewUrl,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase236Path = Phase236Path,
        phase256Path = Phase256Path,
        phase323Path = Phase323Path,
        phase333Path = Phase333Path,
        phase334Path = Phase334Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "graviweak_plebanski_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "graviweak_plebanski_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.graviweakPlebanskiSourceAuditPassed,
            result.graviweakPlebanskiLeadPresent,
            result.graviweakPrimarySourcesReviewed,
            result.graviweakRouteExternalToGu,
            result.graviweakRouteUnifiesGravityAndWeakSu2,
            result.graviweakRouteUsesExtendedPlebanskiAction,
            result.graviweakRouteUsesSpin44GaugeSymmetry,
            result.graviweakRouteRecoversGravityYangMillsHiggsAction,
            result.graviweakRouteTreatsHiggsAsFrameHiggsConnection,
            result.graviweakRouteRelatesBareCouplingsToUnifiedParameterAndVev,
            result.graviweakRouteProvidesPlanckScaleWeakGravityCouplingRelation,
            result.graviweakRouteLeavesWFieldsMasslessBeforeElectroweakBreaking,
            result.graviweakRouteIncludesMppHiggsMassIntervalLead,
            result.graviweakRouteIncludesMirrorSectorScaleLead,
            result.graviweakRouteRequiresSpecificGaugeGroupChoice,
            result.graviweakRouteRequiresSymmetryBreakingAnsatz,
            result.graviweakRouteRequiresHiggsVevOrCurvatureVacuum,
            result.graviweakRouteRequiresElectroweakBreakingMechanism,
            result.graviweakRouteRequiresPlanckScaleBareParameterMatching,
            result.graviweakRouteRequiresRunningFromPlanckToElectroweakScale,
            result.graviweakRouteRequiresObservedGnAndVevInputs,
            result.graviweakRouteRequiresMppCriticalityAssumptions,
            result.graviweakRouteRequiresFermionHyperchargeAndSmCompletion,
            result.graviweakRouteProvidesGuLocalWzTheorem,
            result.graviweakRouteProvidesSeparateWzSourceRows,
            result.graviweakRouteProvidesTargetIndependentGuVevSource,
            result.graviweakRouteProvidesGuWeakMixingAngleSource,
            result.graviweakRouteProvidesGuGaugeCouplingNormalization,
            result.graviweakRouteProvidesGuObservedFieldExtraction,
            result.graviweakRouteProvidesGuHiggsScalarSourceOperator,
            result.graviweakRouteProvidesGuHiggsQuarticOrExcitationSource,
            result.graviweakRouteProvidesObservedHiggsMassFromGu,
            result.graviweakRouteProvidesGeVUnitNormalization,
            result.graviweakRoutePromotesWzMasses,
            result.graviweakRoutePromotesHiggsMass,
            result.graviweakRouteCompletesBosonPredictions,
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
Console.WriteLine($"graviweakPlebanskiSourceAuditPassed={graviweakPlebanskiSourceAuditPassed}");
Console.WriteLine($"graviweakRouteUnifiesGravityAndWeakSu2={graviweakRouteUnifiesGravityAndWeakSu2}");
Console.WriteLine($"graviweakRouteRecoversGravityYangMillsHiggsAction={graviweakRouteRecoversGravityYangMillsHiggsAction}");
Console.WriteLine($"graviweakRoutePromotesWzMasses={graviweakRoutePromotesWzMasses}");
Console.WriteLine($"graviweakRoutePromotesHiggsMass={graviweakRoutePromotesHiggsMass}");
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
