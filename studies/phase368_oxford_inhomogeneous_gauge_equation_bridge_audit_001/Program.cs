using System.Text.Json;

const string DefaultOutputDir = "studies/phase368_oxford_inhomogeneous_gauge_equation_bridge_audit_001/output";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase313Path = "studies/phase313_official_draft_electroweak_projection_map_audit_001/output/official_draft_electroweak_projection_map_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase331Path = "studies/phase331_theta_omega_inhomogeneous_gauge_source_audit_001/output/theta_omega_inhomogeneous_gauge_source_audit_summary.json";
const string Phase367Path = "studies/phase367_theta_omega_source_equation_availability_audit_001/output/theta_omega_source_equation_availability_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE368_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase313 = JsonDocument.Parse(File.ReadAllText(Phase313Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase331 = JsonDocument.Parse(File.ReadAllText(Phase331Path));
using var phase367 = JsonDocument.Parse(File.ReadAllText(Phase367Path));

const string officialOxfordLectureUrl = "https://geometricunity.org/2013-oxford-lecture/";
const string officialDraftUrl = "https://geometricunity.nyc3.digitaloceanspaces.com/Geometric_Unity-Draft-April-1st-2021.pdf";

const bool officialOxfordTranscriptReviewed = true;
const bool officialOxfordTranscriptIsPrimaryGuLectureSource = true;
const bool officialOxfordPowerpointSectionsAvailable = true;
const bool inhomogeneousGaugeGroupSectionReviewed = true;
const bool swervatureDisplasionSectionReviewed = true;
const bool diracSquareClosingSectionReviewed = true;

const bool inhomogeneousGaugeGroupDefinitionAvailable = true;
const bool quotientUnifiedFieldContentFormulaAvailable = true;
const bool tiltedGaugeGroupDefinitionAvailable = true;
const bool tangentBundlePullbackToFourteenDimensionalUAvailable = true;
const bool homogeneousVectorBundleFermionConstructionAvailable = true;
const bool firstOrderActionShapeAvailable = true;
const bool actionTargetIsCotangentSpaceOfFieldContent = true;
const bool tiltedGaugeInvariantEquationSketchAvailable = true;
const bool swervatureEquationAvailable = true;
const bool displasionEquationAvailable = true;
const bool generalizedYangMillsOperatorSketchAvailable = true;
const bool diracSquareProgramForYangMillsHiggsAvailable = true;
const bool routeProvidesGuNativeEquationScaffold = true;
const bool routeProvidesBridgeEquationScaffold = true;

const bool equationScaffoldIsTranscribedButNotFullyTypedDerivation = true;
const bool equationScaffoldContainsEllipsesOrVagueCompletion = true;
const bool transcriptStatesManuscriptCompletionPending = true;
const bool sourceExplicitlyKeepsFreedomInWritingUpTensors = true;
const bool sourceProvidesExactAcceptedTheorem = false;
const bool sourceProvidesMachineReplayableDerivation = false;
const bool sourceProvidesNumericalPredictionRows = false;

const bool routeProvidesThetaOmegaEquation = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool routeProvidesGuLocalWzTheorem = false;
const bool routeProvidesSeparateWzSourceRows = false;
const bool routeProvidesWzRawAmplitudeGates = false;
const bool routeProvidesWzCommonBridgeGate = false;
const bool routeProvidesWzTargetComparisonGate = false;
const bool routeProvidesWzStabilitySidecars = false;
const bool routeProvidesTargetIndependentVevSource = false;
const bool routeProvidesWeakMixingAngleSource = false;
const bool routeProvidesGaugeCouplingNormalization = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesNeutralMassMatrixDiagonalization = false;
const bool routeProvidesObservedFieldExtraction = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesHiggsIdentityEnvelope = false;
const bool routeProvidesHiggsQuarticOrExcitationSource = false;
const bool routeProvidesHiggsMassiveScalarProfile = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var officialDraftElectroweakProjectionMapAuditPassed = JsonBool(phase313.RootElement, "officialDraftElectroweakProjectionMapAuditPassed") is true;
var officialDraftProvidesPhotonZWeinbergRotation = JsonBool(phase313.RootElement, "officialDraftProvidesPhotonZWeinbergRotation") is true;
var officialDraftProvidesWeakMixingAngleSource = JsonBool(phase313.RootElement, "officialDraftProvidesWeakMixingAngleSource") is true;
var officialDraftProvidesWChargedProjectionRows = JsonBool(phase313.RootElement, "officialDraftProvidesWChargedProjectionRows") is true;
var officialDraftProvidesZSourceRowProjection = JsonBool(phase313.RootElement, "officialDraftProvidesZSourceRowProjection") is true;
var officialDraftProjectionMapCompletesObservedFieldExtraction = JsonBool(phase313.RootElement, "officialDraftProjectionMapCompletesObservedFieldExtraction") is true;

var coupledYangMillsHiggsMassExtractionAuditPassed = JsonBool(phase323.RootElement, "coupledYangMillsHiggsMassExtractionAuditPassed") is true;
var officialDraftAppendixLocatesWeakIsospin = JsonBool(phase323.RootElement, "officialDraftAppendixLocatesWeakIsospin") is true;
var officialDraftAppendixLocatesWeakHypercharge = JsonBool(phase323.RootElement, "officialDraftAppendixLocatesWeakHypercharge") is true;
var officialDraftAppendixLocatesHiggsField = JsonBool(phase323.RootElement, "officialDraftAppendixLocatesHiggsField") is true;
var officialPublicSourcesProvideTargetIndependentVevSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideTargetIndependentVevSource") is true;
var officialPublicSourcesProvideGaugeCouplingNormalization = JsonBool(phase323.RootElement, "officialPublicSourcesProvideGaugeCouplingNormalization") is true;
var officialPublicSourcesProvidePhotonWzHiggsProjectionRows = JsonBool(phase323.RootElement, "officialPublicSourcesProvidePhotonWzHiggsProjectionRows") is true;
var officialPublicSourcesProvideHiggsScalarSelfCouplingSource = JsonBool(phase323.RootElement, "officialPublicSourcesProvideHiggsScalarSelfCouplingSource") is true;
var coupledYangMillsHiggsRoutePromotesWzMasses = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRoutePromotesWzMasses") is true;
var coupledYangMillsHiggsRoutePromotesHiggsMass = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRoutePromotesHiggsMass") is true;

var thetaOmegaInhomogeneousGaugeSourceAuditPassed = JsonBool(phase331.RootElement, "thetaOmegaInhomogeneousGaugeSourceAuditPassed") is true;
var thetaOmegaInhomogeneousGaugeRouteGuNative = JsonBool(phase331.RootElement, "thetaOmegaInhomogeneousGaugeRouteGuNative") is true;
var thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw = JsonBool(phase331.RootElement, "thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw") is true;
var thetaOmegaRouteProvidesSeparateWzSourceRows = JsonBool(phase331.RootElement, "thetaOmegaRouteProvidesSeparateWzSourceRows") is true;
var thetaOmegaRouteProvidesTargetIndependentVevSource = JsonBool(phase331.RootElement, "thetaOmegaRouteProvidesTargetIndependentVevSource") is true;
var thetaOmegaRoutePromotesWzMasses = JsonBool(phase331.RootElement, "thetaOmegaRoutePromotesWzMasses") is true;
var thetaOmegaRoutePromotesHiggsMass = JsonBool(phase331.RootElement, "thetaOmegaRoutePromotesHiggsMass") is true;

var thetaOmegaSourceEquationAvailabilityAuditPassed = JsonBool(phase367.RootElement, "thetaOmegaSourceEquationAvailabilityAuditPassed") is true;
var officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable = JsonBool(phase367.RootElement, "officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable") is true;
var phase367OfficialOxfordTranscriptProvidesThetaOmegaEquation = JsonBool(phase367.RootElement, "officialOxfordTranscriptProvidesThetaOmegaEquation") is true;
var phase367OfficialOxfordTranscriptProvidesDirectWzMassLaw = JsonBool(phase367.RootElement, "officialOxfordTranscriptProvidesDirectWzMassLaw") is true;
var phase367OfficialOxfordTranscriptProvidesHiggsMassLaw = JsonBool(phase367.RootElement, "officialOxfordTranscriptProvidesHiggsMassLaw") is true;
var phase367PublicEquationSourceFound = JsonBool(phase367.RootElement, "publicEquationSourceFound") is true;

var sourceRows = new[]
{
    new SourceRow(
        "official-oxford-inhomogeneous-gauge-group",
        officialOxfordLectureUrl,
        "2013 Oxford transcript, unified field content / inhomogeneous gauge group section",
        "The transcript introduces the inhomogeneous gauge-group and tilted-gauge-group construction as GU-native unified field content.",
        "Provides public equation scaffold for field content and gauge symmetry, but not observed W/Z/H particle rows or mass laws."),
    new SourceRow(
        "official-oxford-action-and-tilted-invariance",
        officialOxfordLectureUrl,
        "2013 Oxford transcript, toolkit and equation-of-motion sections",
        "The transcript sketches an action on the cotangent space of field content and tilted-gauge-invariant equation structure.",
        "Provides an action/equation scaffold; it does not provide a fully typed theorem replaying W/Z/H source-lineage fields."),
    new SourceRow(
        "official-oxford-swervature-displasion-yang-mills",
        officialOxfordLectureUrl,
        "2013 Oxford transcript, swervature/displasion and generalized Yang-Mills sections",
        "The transcript presents swervature/displasion language and a generalized Yang-Mills operator sketch with shiab operators.",
        "Relevant to a possible bridge-source law, but still lacks W/Z split, weak-angle/VEV/coupling normalization, and observed-field extraction."),
    new SourceRow(
        "official-oxford-dirac-square-yang-mills-higgs",
        officialOxfordLectureUrl,
        "2013 Oxford transcript, closing Dirac-square statement",
        "The transcript places Yang-Mills and Higgs/Klein-Gordon content in the square part of a proposed Dirac-square organization.",
        "Motivates a Higgs source route, but supplies no Higgs scalar-source operator, identity envelope, self-coupling, or massive profile."),
    new SourceRow(
        "official-gu-draft-cross-check",
        officialDraftUrl,
        "2021 GU working draft, cross-check through Phase313/323/331",
        "The draft cross-checks electroweak/Higgs placement and confirms the route remains location/scaffold evidence rather than observed source rows.",
        "Keeps the Oxford transcript in context and prevents promotion from lecture-only equations.")
};

var checks = new[]
{
    new Check(
        "official-oxford-equation-sections-reviewed",
        officialOxfordTranscriptReviewed
            && officialOxfordTranscriptIsPrimaryGuLectureSource
            && officialOxfordPowerpointSectionsAvailable
            && inhomogeneousGaugeGroupSectionReviewed
            && swervatureDisplasionSectionReviewed
            && diracSquareClosingSectionReviewed
            && sourceRows.Length == 5,
        $"reviewed={officialOxfordTranscriptReviewed}; primary={officialOxfordTranscriptIsPrimaryGuLectureSource}; powerpoint={officialOxfordPowerpointSectionsAvailable}; inhomogeneous={inhomogeneousGaugeGroupSectionReviewed}; swervature={swervatureDisplasionSectionReviewed}; diracSquare={diracSquareClosingSectionReviewed}; sourceRows={sourceRows.Length}"),
    new Check(
        "gu-native-equation-scaffold-present",
        inhomogeneousGaugeGroupDefinitionAvailable
            && quotientUnifiedFieldContentFormulaAvailable
            && tiltedGaugeGroupDefinitionAvailable
            && tangentBundlePullbackToFourteenDimensionalUAvailable
            && homogeneousVectorBundleFermionConstructionAvailable
            && firstOrderActionShapeAvailable
            && actionTargetIsCotangentSpaceOfFieldContent
            && tiltedGaugeInvariantEquationSketchAvailable
            && swervatureEquationAvailable
            && displasionEquationAvailable
            && generalizedYangMillsOperatorSketchAvailable
            && diracSquareProgramForYangMillsHiggsAvailable
            && routeProvidesGuNativeEquationScaffold
            && routeProvidesBridgeEquationScaffold,
        $"inhomogeneousGauge={inhomogeneousGaugeGroupDefinitionAvailable}; quotientFieldContent={quotientUnifiedFieldContentFormulaAvailable}; tiltedGauge={tiltedGaugeGroupDefinitionAvailable}; U14={tangentBundlePullbackToFourteenDimensionalUAvailable}; homogeneousBundle={homogeneousVectorBundleFermionConstructionAvailable}; action={firstOrderActionShapeAvailable}; tiltedInvariant={tiltedGaugeInvariantEquationSketchAvailable}; swervature={swervatureEquationAvailable}; displasion={displasionEquationAvailable}; generalizedYm={generalizedYangMillsOperatorSketchAvailable}; diracSquare={diracSquareProgramForYangMillsHiggsAvailable}"),
    new Check(
        "equation-scaffold-is-not-promotable-theorem",
        equationScaffoldIsTranscribedButNotFullyTypedDerivation
            && equationScaffoldContainsEllipsesOrVagueCompletion
            && transcriptStatesManuscriptCompletionPending
            && sourceExplicitlyKeepsFreedomInWritingUpTensors
            && !sourceProvidesExactAcceptedTheorem
            && !sourceProvidesMachineReplayableDerivation
            && !sourceProvidesNumericalPredictionRows,
        $"transcribedButNotTyped={equationScaffoldIsTranscribedButNotFullyTypedDerivation}; ellipses={equationScaffoldContainsEllipsesOrVagueCompletion}; manuscriptPending={transcriptStatesManuscriptCompletionPending}; tensorFreedom={sourceExplicitlyKeepsFreedomInWritingUpTensors}; exactTheorem={sourceProvidesExactAcceptedTheorem}; replayable={sourceProvidesMachineReplayableDerivation}; numericalRows={sourceProvidesNumericalPredictionRows}"),
    new Check(
        "oxford-equation-scaffold-does-not-fill-wz-contract",
        !routeProvidesThetaOmegaEquation
            && !routeProvidesDirectTargetIndependentWzBridgeSourceLaw
            && !routeProvidesGuLocalWzTheorem
            && !routeProvidesSeparateWzSourceRows
            && !routeProvidesWzRawAmplitudeGates
            && !routeProvidesWzCommonBridgeGate
            && !routeProvidesWzTargetComparisonGate
            && !routeProvidesWzStabilitySidecars
            && !routeProvidesTargetIndependentVevSource
            && !routeProvidesWeakMixingAngleSource
            && !routeProvidesGaugeCouplingNormalization
            && !routeProvidesObservedPhotonWzHiggsProjectionRows
            && !routeProvidesNeutralMassMatrixDiagonalization
            && !routePromotesWzMasses
            && !canFillPhase201WzContract,
        $"thetaOmegaEquation={routeProvidesThetaOmegaEquation}; directBridge={routeProvidesDirectTargetIndependentWzBridgeSourceLaw}; wzRows={routeProvidesSeparateWzSourceRows}; raw={routeProvidesWzRawAmplitudeGates}; common={routeProvidesWzCommonBridgeGate}; target={routeProvidesWzTargetComparisonGate}; stability={routeProvidesWzStabilitySidecars}; vev={routeProvidesTargetIndependentVevSource}; weakAngle={routeProvidesWeakMixingAngleSource}; coupling={routeProvidesGaugeCouplingNormalization}; observedRows={routeProvidesObservedPhotonWzHiggsProjectionRows}; neutralMatrix={routeProvidesNeutralMassMatrixDiagonalization}"),
    new Check(
        "oxford-equation-scaffold-does-not-fill-higgs-contract",
        !routeProvidesObservedFieldExtraction
            && !routeProvidesHiggsScalarSourceOperator
            && !routeProvidesHiggsIdentityEnvelope
            && !routeProvidesHiggsQuarticOrExcitationSource
            && !routeProvidesHiggsMassiveScalarProfile
            && !routeProvidesGeVUnitNormalization
            && !routePromotesHiggsMass
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"observedExtraction={routeProvidesObservedFieldExtraction}; higgsSource={routeProvidesHiggsScalarSourceOperator}; higgsEnvelope={routeProvidesHiggsIdentityEnvelope}; quarticOrExcitation={routeProvidesHiggsQuarticOrExcitationSource}; massiveProfile={routeProvidesHiggsMassiveScalarProfile}; geV={routeProvidesGeVUnitNormalization}; promotesHiggs={routePromotesHiggsMass}; canFillHiggs={canFillPhase201HiggsContract}; canFillObserved={canFillPhase256ObservedFieldExtractionContract}"),
    new Check(
        "official-draft-and-theta-omega-boundaries-remain-binding",
        officialDraftElectroweakProjectionMapAuditPassed
            && !officialDraftProvidesPhotonZWeinbergRotation
            && !officialDraftProvidesWeakMixingAngleSource
            && !officialDraftProvidesWChargedProjectionRows
            && !officialDraftProvidesZSourceRowProjection
            && !officialDraftProjectionMapCompletesObservedFieldExtraction
            && coupledYangMillsHiggsMassExtractionAuditPassed
            && officialDraftAppendixLocatesWeakIsospin
            && officialDraftAppendixLocatesWeakHypercharge
            && officialDraftAppendixLocatesHiggsField
            && !officialPublicSourcesProvideTargetIndependentVevSource
            && !officialPublicSourcesProvideGaugeCouplingNormalization
            && !officialPublicSourcesProvidePhotonWzHiggsProjectionRows
            && !officialPublicSourcesProvideHiggsScalarSelfCouplingSource
            && !coupledYangMillsHiggsRoutePromotesWzMasses
            && !coupledYangMillsHiggsRoutePromotesHiggsMass
            && thetaOmegaInhomogeneousGaugeSourceAuditPassed
            && thetaOmegaInhomogeneousGaugeRouteGuNative
            && !thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw
            && !thetaOmegaRouteProvidesSeparateWzSourceRows
            && !thetaOmegaRouteProvidesTargetIndependentVevSource
            && !thetaOmegaRoutePromotesWzMasses
            && !thetaOmegaRoutePromotesHiggsMass,
        $"p313Passed={officialDraftElectroweakProjectionMapAuditPassed}; p313PhotonZ={officialDraftProvidesPhotonZWeinbergRotation}; p313WeakAngle={officialDraftProvidesWeakMixingAngleSource}; p323Passed={coupledYangMillsHiggsMassExtractionAuditPassed}; p323Vev={officialPublicSourcesProvideTargetIndependentVevSource}; p323Coupling={officialPublicSourcesProvideGaugeCouplingNormalization}; p323Observed={officialPublicSourcesProvidePhotonWzHiggsProjectionRows}; p331Passed={thetaOmegaInhomogeneousGaugeSourceAuditPassed}; p331DirectBridge={thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw}; p331Rows={thetaOmegaRouteProvidesSeparateWzSourceRows}; p331Vev={thetaOmegaRouteProvidesTargetIndependentVevSource}"),
    new Check(
        "phase367-current-availability-boundary-remains-binding",
        thetaOmegaSourceEquationAvailabilityAuditPassed
            && officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable
            && !phase367OfficialOxfordTranscriptProvidesThetaOmegaEquation
            && !phase367OfficialOxfordTranscriptProvidesDirectWzMassLaw
            && !phase367OfficialOxfordTranscriptProvidesHiggsMassLaw
            && !phase367PublicEquationSourceFound,
        $"p367Passed={thetaOmegaSourceEquationAvailabilityAuditPassed}; oxfordEquations={officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable}; thetaOmegaEquation={phase367OfficialOxfordTranscriptProvidesThetaOmegaEquation}; wzLaw={phase367OfficialOxfordTranscriptProvidesDirectWzMassLaw}; higgsLaw={phase367OfficialOxfordTranscriptProvidesHiggsMassLaw}; publicEquation={phase367PublicEquationSourceFound}"),
    new Check(
        "phase213-and-phase256-blocker-state-preserved",
        !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; phase256Required={observedFieldExtractionRequiredFieldCount}; phase256Filled={observedFieldExtractionFilledRequiredFieldCount}; phase256Promotable={observedFieldExtractionContractPromotable}")
};

var oxfordInhomogeneousGaugeEquationBridgeAuditPassed = checks.All(check => check.Passed)
    && routeProvidesGuNativeEquationScaffold
    && routeProvidesBridgeEquationScaffold
    && !sourceProvidesExactAcceptedTheorem
    && !routeProvidesDirectTargetIndependentWzBridgeSourceLaw
    && !routeProvidesSeparateWzSourceRows
    && !routeProvidesTargetIndependentVevSource
    && !routeProvidesObservedPhotonWzHiggsProjectionRows
    && !routeProvidesHiggsScalarSourceOperator
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions;

var terminalStatus = oxfordInhomogeneousGaugeEquationBridgeAuditPassed
    ? "oxford-inhomogeneous-gauge-equation-bridge-audit-scaffold-not-source-law"
    : "oxford-inhomogeneous-gauge-equation-bridge-audit-review-required";

var result = new
{
    phaseId = "phase368-oxford-inhomogeneous-gauge-equation-bridge-audit",
    terminalStatus,
    oxfordInhomogeneousGaugeEquationBridgeAuditPassed,
    officialOxfordTranscriptReviewed,
    officialOxfordTranscriptIsPrimaryGuLectureSource,
    officialOxfordPowerpointSectionsAvailable,
    inhomogeneousGaugeGroupSectionReviewed,
    swervatureDisplasionSectionReviewed,
    diracSquareClosingSectionReviewed,
    inhomogeneousGaugeGroupDefinitionAvailable,
    quotientUnifiedFieldContentFormulaAvailable,
    tiltedGaugeGroupDefinitionAvailable,
    tangentBundlePullbackToFourteenDimensionalUAvailable,
    homogeneousVectorBundleFermionConstructionAvailable,
    firstOrderActionShapeAvailable,
    actionTargetIsCotangentSpaceOfFieldContent,
    tiltedGaugeInvariantEquationSketchAvailable,
    swervatureEquationAvailable,
    displasionEquationAvailable,
    generalizedYangMillsOperatorSketchAvailable,
    diracSquareProgramForYangMillsHiggsAvailable,
    routeProvidesGuNativeEquationScaffold,
    routeProvidesBridgeEquationScaffold,
    equationScaffoldIsTranscribedButNotFullyTypedDerivation,
    equationScaffoldContainsEllipsesOrVagueCompletion,
    transcriptStatesManuscriptCompletionPending,
    sourceExplicitlyKeepsFreedomInWritingUpTensors,
    sourceProvidesExactAcceptedTheorem,
    sourceProvidesMachineReplayableDerivation,
    sourceProvidesNumericalPredictionRows,
    routeProvidesThetaOmegaEquation,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesGuLocalWzTheorem,
    routeProvidesSeparateWzSourceRows,
    routeProvidesWzRawAmplitudeGates,
    routeProvidesWzCommonBridgeGate,
    routeProvidesWzTargetComparisonGate,
    routeProvidesWzStabilitySidecars,
    routeProvidesTargetIndependentVevSource,
    routeProvidesWeakMixingAngleSource,
    routeProvidesGaugeCouplingNormalization,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesNeutralMassMatrixDiagonalization,
    routeProvidesObservedFieldExtraction,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesHiggsIdentityEnvelope,
    routeProvidesHiggsQuarticOrExcitationSource,
    routeProvidesHiggsMassiveScalarProfile,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRows,
    checks,
    adjacentRouteBoundary = new
    {
        officialDraftElectroweakProjectionMapAuditPassed,
        officialDraftProvidesPhotonZWeinbergRotation,
        officialDraftProvidesWeakMixingAngleSource,
        officialDraftProvidesWChargedProjectionRows,
        officialDraftProvidesZSourceRowProjection,
        officialDraftProjectionMapCompletesObservedFieldExtraction,
        coupledYangMillsHiggsMassExtractionAuditPassed,
        officialDraftAppendixLocatesWeakIsospin,
        officialDraftAppendixLocatesWeakHypercharge,
        officialDraftAppendixLocatesHiggsField,
        officialPublicSourcesProvideTargetIndependentVevSource,
        officialPublicSourcesProvideGaugeCouplingNormalization,
        officialPublicSourcesProvidePhotonWzHiggsProjectionRows,
        officialPublicSourcesProvideHiggsScalarSelfCouplingSource,
        coupledYangMillsHiggsRoutePromotesWzMasses,
        coupledYangMillsHiggsRoutePromotesHiggsMass,
        thetaOmegaInhomogeneousGaugeSourceAuditPassed,
        thetaOmegaInhomogeneousGaugeRouteGuNative,
        thetaOmegaRouteProvidesDirectTargetIndependentWzBridgeSourceLaw,
        thetaOmegaRouteProvidesSeparateWzSourceRows,
        thetaOmegaRouteProvidesTargetIndependentVevSource,
        thetaOmegaRoutePromotesWzMasses,
        thetaOmegaRoutePromotesHiggsMass,
        thetaOmegaSourceEquationAvailabilityAuditPassed,
        officialOxfordTranscriptInhomogeneousGaugeEquationsAvailable,
        phase367OfficialOxfordTranscriptProvidesThetaOmegaEquation,
        phase367OfficialOxfordTranscriptProvidesDirectWzMassLaw,
        phase367OfficialOxfordTranscriptProvidesHiggsMassLaw,
        phase367PublicEquationSourceFound
    },
    contractImpact = new
    {
        existingEvidenceFound,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract
    },
    decision = "Do not promote W/Z or Higgs masses from the public Oxford inhomogeneous-gauge equation scaffold. The transcript gives genuine GU-native scaffold equations and bridge direction, including inhomogeneous/tilted gauge structure, action/equation sketches, swervature/displasion, generalized Yang-Mills terms, and a Dirac-square organization for Yang-Mills/Higgs. It is still not a replayable source law: it contains incomplete/vague tensor choices and no separate W/Z source rows, target-independent VEV, weak-angle/coupling normalization, observed photon/W/Z/H projection rows, Higgs scalar-source/self-coupling lineage, pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A fully typed public GU derivation turning the Oxford inhomogeneous/tilted gauge scaffold into observed electroweak photon, W, Z, and Higgs operators.",
        "A target-independent W/Z bridge theorem with separate source rows, raw/common/target/stability gates, and derivation IDs.",
        "A GU-local electroweak scale/VEV and weak-angle/coupling normalization derived before target comparison.",
        "A GU-local Higgs scalar-source, identity envelope, massive profile, self-coupling or excitation lineage, and GeV normalization."
    }
};

var summary = new
{
    result.phaseId,
    result.terminalStatus,
    result.oxfordInhomogeneousGaugeEquationBridgeAuditPassed,
    result.officialOxfordTranscriptReviewed,
    result.officialOxfordTranscriptIsPrimaryGuLectureSource,
    result.officialOxfordPowerpointSectionsAvailable,
    result.inhomogeneousGaugeGroupSectionReviewed,
    result.swervatureDisplasionSectionReviewed,
    result.diracSquareClosingSectionReviewed,
    result.inhomogeneousGaugeGroupDefinitionAvailable,
    result.quotientUnifiedFieldContentFormulaAvailable,
    result.tiltedGaugeGroupDefinitionAvailable,
    result.tangentBundlePullbackToFourteenDimensionalUAvailable,
    result.homogeneousVectorBundleFermionConstructionAvailable,
    result.firstOrderActionShapeAvailable,
    result.actionTargetIsCotangentSpaceOfFieldContent,
    result.tiltedGaugeInvariantEquationSketchAvailable,
    result.swervatureEquationAvailable,
    result.displasionEquationAvailable,
    result.generalizedYangMillsOperatorSketchAvailable,
    result.diracSquareProgramForYangMillsHiggsAvailable,
    result.routeProvidesGuNativeEquationScaffold,
    result.routeProvidesBridgeEquationScaffold,
    result.equationScaffoldIsTranscribedButNotFullyTypedDerivation,
    result.equationScaffoldContainsEllipsesOrVagueCompletion,
    result.transcriptStatesManuscriptCompletionPending,
    result.sourceExplicitlyKeepsFreedomInWritingUpTensors,
    result.sourceProvidesExactAcceptedTheorem,
    result.sourceProvidesMachineReplayableDerivation,
    result.sourceProvidesNumericalPredictionRows,
    result.routeProvidesThetaOmegaEquation,
    result.routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    result.routeProvidesGuLocalWzTheorem,
    result.routeProvidesSeparateWzSourceRows,
    result.routeProvidesWzRawAmplitudeGates,
    result.routeProvidesWzCommonBridgeGate,
    result.routeProvidesWzTargetComparisonGate,
    result.routeProvidesWzStabilitySidecars,
    result.routeProvidesTargetIndependentVevSource,
    result.routeProvidesWeakMixingAngleSource,
    result.routeProvidesGaugeCouplingNormalization,
    result.routeProvidesObservedPhotonWzHiggsProjectionRows,
    result.routeProvidesNeutralMassMatrixDiagonalization,
    result.routeProvidesObservedFieldExtraction,
    result.routeProvidesHiggsScalarSourceOperator,
    result.routeProvidesHiggsIdentityEnvelope,
    result.routeProvidesHiggsQuarticOrExcitationSource,
    result.routeProvidesHiggsMassiveScalarProfile,
    result.routeProvidesGeVUnitNormalization,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.adjacentRouteBoundary,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "oxford_inhomogeneous_gauge_equation_bridge_audit.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "oxford_inhomogeneous_gauge_equation_bridge_audit_summary.json"),
    JsonSerializer.Serialize(summary, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"oxfordInhomogeneousGaugeEquationBridgeAuditPassed={oxfordInhomogeneousGaugeEquationBridgeAuditPassed}");
Console.WriteLine($"routeProvidesGuNativeEquationScaffold={routeProvidesGuNativeEquationScaffold}");
Console.WriteLine($"sourceProvidesExactAcceptedTheorem={sourceProvidesExactAcceptedTheorem}");
Console.WriteLine($"routeProvidesDirectTargetIndependentWzBridgeSourceLaw={routeProvidesDirectTargetIndependentWzBridgeSourceLaw}");
Console.WriteLine($"routeProvidesSeparateWzSourceRows={routeProvidesSeparateWzSourceRows}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string name)
{
    if (!element.TryGetProperty(name, out var property))
    {
        return null;
    }

    return property.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => null
    };
}

static int? JsonInt(JsonElement element, string name)
{
    if (!element.TryGetProperty(name, out var property))
    {
        return null;
    }

    if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
    {
        return value;
    }

    return null;
}

record Check(string CheckId, bool Passed, string Detail);
record SourceRow(string SourceId, string Url, string Locator, string Finding, string PredictionImpact);
