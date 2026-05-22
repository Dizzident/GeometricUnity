using System.Text.Json;

const string DefaultOutputDir = "studies/phase339_macdowell_mansouri_cartan_breaking_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase335Path = "studies/phase335_graviweak_plebanski_source_audit_001/output/graviweak_plebanski_source_audit_summary.json";
const string Phase338Path = "studies/phase338_metric_affine_torsion_source_audit_001/output/metric_affine_torsion_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE339_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase335 = JsonDocument.Parse(File.ReadAllText(Phase335Path));
using var phase338 = JsonDocument.Parse(File.ReadAllText(Phase338Path));

const string deSitterGaugeElectroweakUrl = "https://arxiv.org/abs/hep-th/9605217";
const string macdowellMansouriCartanGeometryUrl = "https://arxiv.org/abs/gr-qc/0611154";
const string so33BfElectroweakUrl = "https://arxiv.org/abs/2602.19151";

const bool macdowellMansouriCartanSourceAuditPassedExpected = true;
const bool macdowellMansouriCartanLeadPresent = true;
const bool macdowellMansouriPrimarySourcesReviewed = true;
const bool macdowellMansouriRouteExternalToGu = true;
const bool cartanGeometryMacdowellMansouriGravityLeadPresent = true;
const bool cartanGeometryCombinesConnectionAndCoframe = true;
const bool cartanGeometryUsesDeSitterOrAntiDeSitterCartanConnection = true;
const bool cartanGeometryRewritesPalatiniGravity = true;
const bool cartanGeometryBfReformulationPresent = true;
const bool deSitterGaugeElectroweakLeadPresent = true;
const bool deSitterGaugeClaimsCorrectBosonMassAssignments = true;
const bool deSitterGaugeNoConventionalHiggsClaimPresent = true;
const bool deSitterGaugeMassMatrixDependsOnGeometricBreakingParameters = true;
const bool deSitterGaugeTradesScaleForObservedMwOrMz = true;
const bool deSitterGaugeNeedsWeakAngleRhoAndOrientationParameters = true;
const bool deSitterGaugeObservedHiggsConflict = true;
const bool so33BfMacdowellMansouriLeadPresent = true;
const bool so33BfRecoversConventionalElectroweakSpectrum = true;
const bool so33BfUsesStandardHiggsMechanism = true;
const bool so33BfRequiresVevAndWeakCoupling = true;
const bool so33BfHierarchyAnsatzNotDerived = true;
const bool so33BfMatterCouplingAndOrderParameterFutureWork = true;
const bool routeDistinctFromMetricAffineTorsion = true;
const bool routeOverlapsGraviweakPlebanskiButAddsCartanBreaking = true;

const bool macdowellRouteRequiresGuLocalCartanDeSitterMap = true;
const bool macdowellRouteRequiresBreakingOrderParameterSource = true;
const bool macdowellRouteRequiresTargetIndependentBreakingScale = true;
const bool macdowellRouteRequiresWeakAngleRhoAndOrientationLineage = true;
const bool macdowellRouteRequiresElectroweakEmbeddingAndNeutralProjection = true;
const bool macdowellRouteRequiresObservedPhotonWzHProjection = true;
const bool macdowellRouteRequiresTargetIndependentVevAndGaugeCouplingSource = true;
const bool macdowellRouteRequiresHiggsScalarSourceCompatibleWithObservedHiggs = true;
const bool macdowellRouteRequiresRgTransportAndPrecisionMatching = true;
const bool macdowellRouteRequiresGeVUnitNormalization = true;

const bool macdowellRouteProvidesGuLocalWzTheorem = false;
const bool macdowellRouteProvidesSeparateWzSourceRows = false;
const bool macdowellRouteProvidesWzRawAmplitudeGates = false;
const bool macdowellRouteProvidesWzCommonBridgeGate = false;
const bool macdowellRouteProvidesTargetIndependentGuVevSource = false;
const bool macdowellRouteProvidesGuWeakMixingAngleSource = false;
const bool macdowellRouteProvidesGuGaugeCouplingNormalization = false;
const bool macdowellRouteProvidesObservedPhotonWzProjectionRows = false;
const bool macdowellRouteProvidesGuObservedFieldExtraction = false;
const bool macdowellRouteProvidesGuHiggsScalarSourceOperator = false;
const bool macdowellRouteProvidesGuHiggsQuarticOrExcitationSource = false;
const bool macdowellRouteProvidesObservedHiggsMassFromGu = false;
const bool macdowellRouteProvidesGeVUnitNormalization = false;
const bool macdowellRoutePromotesWzMasses = false;
const bool macdowellRoutePromotesHiggsMass = false;
const bool macdowellRouteCompletesBosonPredictions = false;
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
var graviweakPlebanskiSourceAuditPassed = JsonBool(phase335.RootElement, "graviweakPlebanskiSourceAuditPassed") is true;
var graviweakRoutePromotesWzMasses = JsonBool(phase335.RootElement, "graviweakRoutePromotesWzMasses") is true;
var graviweakRoutePromotesHiggsMass = JsonBool(phase335.RootElement, "graviweakRoutePromotesHiggsMass") is true;
var graviweakRouteCompletesBosonPredictions = JsonBool(phase335.RootElement, "graviweakRouteCompletesBosonPredictions") is true;
var metricAffineTorsionSourceAuditPassed = JsonBool(phase338.RootElement, "metricAffineTorsionSourceAuditPassed") is true;
var torsionRouteCompletesBosonPredictions = JsonBool(phase338.RootElement, "torsionRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-hep-th-9605217-de-sitter-gauge-electroweak",
        deSitterGaugeElectroweakUrl,
        "Complexified de Sitter gauge route to electroweak mass assignments",
        "Claims that geometry-imposed symmetry breaking can replace the conventional Higgs sector and produce the standard electroweak gauge-boson spectrum through a mass matrix depending on breaking scale, orientation, weak angle, and rho.",
        "Direct W/Z lead, but it can trade its scale for observed W or Z mass, requires weak-angle and rho/orientation inputs, and conflicts with the observed Higgs sector."),
    new SourceRow(
        "arxiv-gr-qc-0611154-macdowell-mansouri-cartan-geometry",
        macdowellMansouriCartanGeometryUrl,
        "Cartan-geometric interpretation of MacDowell-Mansouri gravity",
        "Explains the larger Cartan connection combining Lorentz connection and coframe, the de Sitter/anti-de Sitter model-spacetime interpretation, and the BF reformulation of MacDowell-Mansouri gravity.",
        "Important gauge-geometric infrastructure, but it is a gravity construction and does not supply electroweak W/Z/H source rows by itself."),
    new SourceRow(
        "arxiv-2602-19151-so33-bf-electroweak",
        so33BfElectroweakUrl,
        "SO(3,3) BF symmetry breaking with electroweak branch",
        "Uses MacDowell-Mansouri-type BF breaking to obtain chiral sectors and an electroweak branch, then introduces the conventional Higgs mechanism and W mass relation through the VEV and weak coupling.",
        "Serious BF/graviweak lead, but the electroweak masses still depend on standard Higgs-sector inputs and a hierarchy ansatz explicitly marked as not derived.")
};

var checks = new[]
{
    new Check(
        "macdowell-mansouri-primary-sources-reviewed",
        macdowellMansouriCartanLeadPresent
            && macdowellMansouriPrimarySourcesReviewed
            && macdowellMansouriRouteExternalToGu
            && sourceRows.Length == 3,
        $"lead={macdowellMansouriCartanLeadPresent}; reviewed={macdowellMansouriPrimarySourcesReviewed}; externalToGu={macdowellMansouriRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "cartan-geometry-gravity-baseline-captured",
        cartanGeometryMacdowellMansouriGravityLeadPresent
            && cartanGeometryCombinesConnectionAndCoframe
            && cartanGeometryUsesDeSitterOrAntiDeSitterCartanConnection
            && cartanGeometryRewritesPalatiniGravity
            && cartanGeometryBfReformulationPresent,
        $"gravityLead={cartanGeometryMacdowellMansouriGravityLeadPresent}; combinesConnectionAndCoframe={cartanGeometryCombinesConnectionAndCoframe}; deSitterConnection={cartanGeometryUsesDeSitterOrAntiDeSitterCartanConnection}; palatiniRewrite={cartanGeometryRewritesPalatiniGravity}; bfReformulation={cartanGeometryBfReformulationPresent}"),
    new Check(
        "de-sitter-electroweak-lead-captured-as-non-predictive",
        deSitterGaugeElectroweakLeadPresent
            && deSitterGaugeClaimsCorrectBosonMassAssignments
            && deSitterGaugeNoConventionalHiggsClaimPresent
            && deSitterGaugeMassMatrixDependsOnGeometricBreakingParameters
            && deSitterGaugeTradesScaleForObservedMwOrMz
            && deSitterGaugeNeedsWeakAngleRhoAndOrientationParameters
            && deSitterGaugeObservedHiggsConflict,
        $"ewLead={deSitterGaugeElectroweakLeadPresent}; massAssignments={deSitterGaugeClaimsCorrectBosonMassAssignments}; noHiggsClaim={deSitterGaugeNoConventionalHiggsClaimPresent}; geometricMassMatrix={deSitterGaugeMassMatrixDependsOnGeometricBreakingParameters}; tradesScale={deSitterGaugeTradesScaleForObservedMwOrMz}; weakAngleRhoOrientation={deSitterGaugeNeedsWeakAngleRhoAndOrientationParameters}; observedHiggsConflict={deSitterGaugeObservedHiggsConflict}"),
    new Check(
        "so33-bf-electroweak-branch-captured-as-standard-higgs-dependent",
        so33BfMacdowellMansouriLeadPresent
            && so33BfRecoversConventionalElectroweakSpectrum
            && so33BfUsesStandardHiggsMechanism
            && so33BfRequiresVevAndWeakCoupling
            && so33BfHierarchyAnsatzNotDerived
            && so33BfMatterCouplingAndOrderParameterFutureWork,
        $"so33Lead={so33BfMacdowellMansouriLeadPresent}; conventionalSpectrum={so33BfRecoversConventionalElectroweakSpectrum}; standardHiggs={so33BfUsesStandardHiggsMechanism}; vevWeakCoupling={so33BfRequiresVevAndWeakCoupling}; hierarchyNotDerived={so33BfHierarchyAnsatzNotDerived}; futureMatterOrderParameter={so33BfMatterCouplingAndOrderParameterFutureWork}"),
    new Check(
        "related-route-boundaries-preserved",
        routeDistinctFromMetricAffineTorsion
            && routeOverlapsGraviweakPlebanskiButAddsCartanBreaking
            && graviweakPlebanskiSourceAuditPassed
            && !graviweakRoutePromotesWzMasses
            && !graviweakRoutePromotesHiggsMass
            && !graviweakRouteCompletesBosonPredictions
            && metricAffineTorsionSourceAuditPassed
            && !torsionRouteCompletesBosonPredictions,
        $"distinctFromTorsion={routeDistinctFromMetricAffineTorsion}; overlapsGraviweak={routeOverlapsGraviweakPlebanskiButAddsCartanBreaking}; graviweakAudit={graviweakPlebanskiSourceAuditPassed}; graviweakPromotesWz={graviweakRoutePromotesWzMasses}; graviweakPromotesHiggs={graviweakRoutePromotesHiggsMass}; torsionAudit={metricAffineTorsionSourceAuditPassed}; torsionCompletes={torsionRouteCompletesBosonPredictions}"),
    new Check(
        "macdowell-route-requires-missing-gu-source-data",
        macdowellRouteRequiresGuLocalCartanDeSitterMap
            && macdowellRouteRequiresBreakingOrderParameterSource
            && macdowellRouteRequiresTargetIndependentBreakingScale
            && macdowellRouteRequiresWeakAngleRhoAndOrientationLineage
            && macdowellRouteRequiresElectroweakEmbeddingAndNeutralProjection
            && macdowellRouteRequiresObservedPhotonWzHProjection
            && macdowellRouteRequiresTargetIndependentVevAndGaugeCouplingSource
            && macdowellRouteRequiresHiggsScalarSourceCompatibleWithObservedHiggs
            && macdowellRouteRequiresRgTransportAndPrecisionMatching
            && macdowellRouteRequiresGeVUnitNormalization,
        $"guCartanMap={macdowellRouteRequiresGuLocalCartanDeSitterMap}; orderParameter={macdowellRouteRequiresBreakingOrderParameterSource}; scale={macdowellRouteRequiresTargetIndependentBreakingScale}; weakAngleRho={macdowellRouteRequiresWeakAngleRhoAndOrientationLineage}; neutralProjection={macdowellRouteRequiresElectroweakEmbeddingAndNeutralProjection}; observedProjection={macdowellRouteRequiresObservedPhotonWzHProjection}; vevGauge={macdowellRouteRequiresTargetIndependentVevAndGaugeCouplingSource}; higgsCompatible={macdowellRouteRequiresHiggsScalarSourceCompatibleWithObservedHiggs}; precision={macdowellRouteRequiresRgTransportAndPrecisionMatching}; gev={macdowellRouteRequiresGeVUnitNormalization}"),
    new Check(
        "macdowell-route-does-not-fill-gu-contracts",
        !macdowellRouteProvidesGuLocalWzTheorem
            && !macdowellRouteProvidesSeparateWzSourceRows
            && !macdowellRouteProvidesWzRawAmplitudeGates
            && !macdowellRouteProvidesWzCommonBridgeGate
            && !macdowellRouteProvidesTargetIndependentGuVevSource
            && !macdowellRouteProvidesGuWeakMixingAngleSource
            && !macdowellRouteProvidesGuGaugeCouplingNormalization
            && !macdowellRouteProvidesObservedPhotonWzProjectionRows
            && !macdowellRouteProvidesGuObservedFieldExtraction
            && !macdowellRouteProvidesGuHiggsScalarSourceOperator
            && !macdowellRouteProvidesGuHiggsQuarticOrExcitationSource
            && !macdowellRouteProvidesObservedHiggsMassFromGu
            && !macdowellRouteProvidesGeVUnitNormalization
            && !macdowellRoutePromotesWzMasses
            && !macdowellRoutePromotesHiggsMass
            && !macdowellRouteCompletesBosonPredictions,
        $"guWzTheorem={macdowellRouteProvidesGuLocalWzTheorem}; separateRows={macdowellRouteProvidesSeparateWzSourceRows}; targetVev={macdowellRouteProvidesTargetIndependentGuVevSource}; weakAngle={macdowellRouteProvidesGuWeakMixingAngleSource}; gaugeNorm={macdowellRouteProvidesGuGaugeCouplingNormalization}; projection={macdowellRouteProvidesObservedPhotonWzProjectionRows}; observedExtraction={macdowellRouteProvidesGuObservedFieldExtraction}; higgsOperator={macdowellRouteProvidesGuHiggsScalarSourceOperator}; higgsQuartic={macdowellRouteProvidesGuHiggsQuarticOrExcitationSource}; gev={macdowellRouteProvidesGeVUnitNormalization}; promotesWz={macdowellRoutePromotesWzMasses}; promotesHiggs={macdowellRoutePromotesHiggsMass}"),
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

var macdowellMansouriCartanSourceAuditPassed = checks.All(check => check.Passed)
    && macdowellMansouriCartanSourceAuditPassedExpected
    && !macdowellRouteProvidesGuLocalWzTheorem
    && !macdowellRouteProvidesSeparateWzSourceRows
    && !macdowellRouteProvidesTargetIndependentGuVevSource
    && !macdowellRouteProvidesGuWeakMixingAngleSource
    && !macdowellRouteProvidesGuGaugeCouplingNormalization
    && !macdowellRouteProvidesGuObservedFieldExtraction
    && !macdowellRouteProvidesGuHiggsScalarSourceOperator
    && !macdowellRouteProvidesGuHiggsQuarticOrExcitationSource
    && !macdowellRouteProvidesObservedHiggsMassFromGu
    && !macdowellRouteProvidesGeVUnitNormalization
    && !macdowellRoutePromotesWzMasses
    && !macdowellRoutePromotesHiggsMass
    && !macdowellRouteCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = macdowellMansouriCartanSourceAuditPassed
    ? "macdowell-mansouri-cartan-breaking-source-audit-external-gauge-breaking-lead-not-gu-source"
    : "macdowell-mansouri-cartan-breaking-source-audit-review-required";

var result = new
{
    phaseId = "phase339-macdowell-mansouri-cartan-breaking-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    macdowellMansouriCartanSourceAuditPassed,
    macdowellMansouriCartanLeadPresent,
    macdowellMansouriPrimarySourcesReviewed,
    macdowellMansouriRouteExternalToGu,
    cartanGeometryMacdowellMansouriGravityLeadPresent,
    cartanGeometryCombinesConnectionAndCoframe,
    cartanGeometryUsesDeSitterOrAntiDeSitterCartanConnection,
    cartanGeometryRewritesPalatiniGravity,
    cartanGeometryBfReformulationPresent,
    deSitterGaugeElectroweakLeadPresent,
    deSitterGaugeClaimsCorrectBosonMassAssignments,
    deSitterGaugeNoConventionalHiggsClaimPresent,
    deSitterGaugeMassMatrixDependsOnGeometricBreakingParameters,
    deSitterGaugeTradesScaleForObservedMwOrMz,
    deSitterGaugeNeedsWeakAngleRhoAndOrientationParameters,
    deSitterGaugeObservedHiggsConflict,
    so33BfMacdowellMansouriLeadPresent,
    so33BfRecoversConventionalElectroweakSpectrum,
    so33BfUsesStandardHiggsMechanism,
    so33BfRequiresVevAndWeakCoupling,
    so33BfHierarchyAnsatzNotDerived,
    so33BfMatterCouplingAndOrderParameterFutureWork,
    routeDistinctFromMetricAffineTorsion,
    routeOverlapsGraviweakPlebanskiButAddsCartanBreaking,
    macdowellRouteRequiresGuLocalCartanDeSitterMap,
    macdowellRouteRequiresBreakingOrderParameterSource,
    macdowellRouteRequiresTargetIndependentBreakingScale,
    macdowellRouteRequiresWeakAngleRhoAndOrientationLineage,
    macdowellRouteRequiresElectroweakEmbeddingAndNeutralProjection,
    macdowellRouteRequiresObservedPhotonWzHProjection,
    macdowellRouteRequiresTargetIndependentVevAndGaugeCouplingSource,
    macdowellRouteRequiresHiggsScalarSourceCompatibleWithObservedHiggs,
    macdowellRouteRequiresRgTransportAndPrecisionMatching,
    macdowellRouteRequiresGeVUnitNormalization,
    macdowellRouteProvidesGuLocalWzTheorem,
    macdowellRouteProvidesSeparateWzSourceRows,
    macdowellRouteProvidesWzRawAmplitudeGates,
    macdowellRouteProvidesWzCommonBridgeGate,
    macdowellRouteProvidesTargetIndependentGuVevSource,
    macdowellRouteProvidesGuWeakMixingAngleSource,
    macdowellRouteProvidesGuGaugeCouplingNormalization,
    macdowellRouteProvidesObservedPhotonWzProjectionRows,
    macdowellRouteProvidesGuObservedFieldExtraction,
    macdowellRouteProvidesGuHiggsScalarSourceOperator,
    macdowellRouteProvidesGuHiggsQuarticOrExcitationSource,
    macdowellRouteProvidesObservedHiggsMassFromGu,
    macdowellRouteProvidesGeVUnitNormalization,
    macdowellRoutePromotesWzMasses,
    macdowellRoutePromotesHiggsMass,
    macdowellRouteCompletesBosonPredictions,
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
    decision = "Do not promote W/Z or Higgs physical masses from the MacDowell-Mansouri / Cartan-breaking route in this repository. The route is a serious external geometric gauge-breaking lead, especially through de Sitter electroweak mass matrices and SO(3,3) BF symmetry breaking, but the current sources either trade the electroweak scale for observed W/Z masses, require weak-angle/rho/orientation inputs, conflict with the observed Higgs sector, or recover the conventional Higgs mechanism with VEV and coupling inputs. It still lacks a GU-local Cartan/de Sitter bridge theorem, target-independent breaking scale, observed photon/W/Z/H projection, Higgs scalar-source lineage, precision matching, and GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem mapping draft field content, observer projection, Shiab/operator data, and any Cartan/de Sitter connection into observed electroweak W/Z/H source rows.",
        "A target-independent source for the Cartan or MacDowell-Mansouri breaking order parameter, scale, weak angle, rho/orientation data, VEV, and gauge-coupling normalization.",
        "A neutral-sector diagonalization and observed photon/W/Z/H projection that does not import measured W, Z, or Higgs masses.",
        "A Higgs scalar-source operator or compatible composite profile that explains the observed Higgs rather than removing it.",
        "RG transport, precision-electroweak checks, physical-unit normalization, and validation through Phase201/Phase256 without weakening existing gates."
    }
};

var fullPath = Path.Combine(outputDir, "macdowell_mansouri_cartan_breaking_source_audit.json");
var summaryPath = Path.Combine(outputDir, "macdowell_mansouri_cartan_breaking_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.macdowellMansouriCartanSourceAuditPassed,
    result.macdowellMansouriCartanLeadPresent,
    result.macdowellMansouriPrimarySourcesReviewed,
    result.macdowellMansouriRouteExternalToGu,
    result.cartanGeometryMacdowellMansouriGravityLeadPresent,
    result.cartanGeometryCombinesConnectionAndCoframe,
    result.cartanGeometryUsesDeSitterOrAntiDeSitterCartanConnection,
    result.cartanGeometryRewritesPalatiniGravity,
    result.cartanGeometryBfReformulationPresent,
    result.deSitterGaugeElectroweakLeadPresent,
    result.deSitterGaugeClaimsCorrectBosonMassAssignments,
    result.deSitterGaugeNoConventionalHiggsClaimPresent,
    result.deSitterGaugeMassMatrixDependsOnGeometricBreakingParameters,
    result.deSitterGaugeTradesScaleForObservedMwOrMz,
    result.deSitterGaugeNeedsWeakAngleRhoAndOrientationParameters,
    result.deSitterGaugeObservedHiggsConflict,
    result.so33BfMacdowellMansouriLeadPresent,
    result.so33BfRecoversConventionalElectroweakSpectrum,
    result.so33BfUsesStandardHiggsMechanism,
    result.so33BfRequiresVevAndWeakCoupling,
    result.so33BfHierarchyAnsatzNotDerived,
    result.so33BfMatterCouplingAndOrderParameterFutureWork,
    result.routeDistinctFromMetricAffineTorsion,
    result.routeOverlapsGraviweakPlebanskiButAddsCartanBreaking,
    result.macdowellRouteRequiresGuLocalCartanDeSitterMap,
    result.macdowellRouteRequiresBreakingOrderParameterSource,
    result.macdowellRouteRequiresTargetIndependentBreakingScale,
    result.macdowellRouteRequiresWeakAngleRhoAndOrientationLineage,
    result.macdowellRouteRequiresElectroweakEmbeddingAndNeutralProjection,
    result.macdowellRouteRequiresObservedPhotonWzHProjection,
    result.macdowellRouteRequiresTargetIndependentVevAndGaugeCouplingSource,
    result.macdowellRouteRequiresHiggsScalarSourceCompatibleWithObservedHiggs,
    result.macdowellRouteRequiresRgTransportAndPrecisionMatching,
    result.macdowellRouteRequiresGeVUnitNormalization,
    result.macdowellRouteProvidesGuLocalWzTheorem,
    result.macdowellRouteProvidesSeparateWzSourceRows,
    result.macdowellRouteProvidesTargetIndependentGuVevSource,
    result.macdowellRouteProvidesGuWeakMixingAngleSource,
    result.macdowellRouteProvidesGuGaugeCouplingNormalization,
    result.macdowellRouteProvidesObservedPhotonWzProjectionRows,
    result.macdowellRouteProvidesGuObservedFieldExtraction,
    result.macdowellRouteProvidesGuHiggsScalarSourceOperator,
    result.macdowellRouteProvidesGuHiggsQuarticOrExcitationSource,
    result.macdowellRouteProvidesObservedHiggsMassFromGu,
    result.macdowellRouteProvidesGeVUnitNormalization,
    result.macdowellRoutePromotesWzMasses,
    result.macdowellRoutePromotesHiggsMass,
    result.macdowellRouteCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"macdowellMansouriCartanSourceAuditPassed={macdowellMansouriCartanSourceAuditPassed}");
Console.WriteLine($"deSitterGaugeClaimsCorrectBosonMassAssignments={deSitterGaugeClaimsCorrectBosonMassAssignments}");
Console.WriteLine($"deSitterGaugeTradesScaleForObservedMwOrMz={deSitterGaugeTradesScaleForObservedMwOrMz}");
Console.WriteLine($"so33BfUsesStandardHiggsMechanism={so33BfUsesStandardHiggsMechanism}");
Console.WriteLine($"macdowellRoutePromotesWzMasses={macdowellRoutePromotesWzMasses}");
Console.WriteLine($"macdowellRoutePromotesHiggsMass={macdowellRoutePromotesHiggsMass}");
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
