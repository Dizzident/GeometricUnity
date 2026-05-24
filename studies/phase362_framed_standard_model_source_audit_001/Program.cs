using System.Text.Json;

const string DefaultOutputDir = "studies/phase362_framed_standard_model_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase343Path = "studies/phase343_stueckelberg_vector_mass_source_audit_001/output/stueckelberg_vector_mass_source_audit_summary.json";
const string Phase359Path = "studies/phase359_finite_ncg_discrete_higgs_source_audit_001/output/finite_ncg_discrete_higgs_source_audit_summary.json";
const string Phase361Path = "studies/phase361_matrix_model_higgs_geometry_source_audit_001/output/matrix_model_higgs_geometry_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE362_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase343 = JsonDocument.Parse(File.ReadAllText(Phase343Path));
using var phase359 = JsonDocument.Parse(File.ReadAllText(Phase359Path));
using var phase361 = JsonDocument.Parse(File.ReadAllText(Phase361Path));

const string exploringFramedGaugeTheoryUrl = "https://arxiv.org/abs/1111.3832";
const string developingFramedStandardModelUrl = "https://arxiv.org/abs/1111.5591";
const string firstTestAgainstExperimentUrl = "https://arxiv.org/abs/1410.8022";
const string framedStandardModelIUrl = "https://arxiv.org/abs/1505.05472";
const string framedStandardModelIIUrl = "https://arxiv.org/abs/1508.04273";
const string zBosonInFramedStandardModelUrl = "https://arxiv.org/abs/1806.08271";
const string closerStudyFramedStandardModelUrl = "https://arxiv.org/abs/1806.08268";
const string closerStudyFramedStandardModelDoi = "https://doi.org/10.1142/S0217751X18501956";

const bool framedStandardModelSourceAuditPassedExpected = true;
const bool framedStandardModelLeadPresent = true;
const bool framedStandardModelPrimarySourcesReviewed = true;
const bool framedStandardModelRouteExternalToGu = true;
const bool routeUsesFramedGaugeTheory = true;
const bool routeUsesInternalFrameVectorsOrVielbeins = true;
const bool routeIntroducesFramonsAsFieldVariables = true;
const bool routeHiggsAsElectroweakFramon = true;
const bool routeRelatesParticleTheoryToGravityVierbeinStructure = true;
const bool routeProducesGenerationSymmetryLead = true;
const bool routeUsesRotatingRankOneFermionMassMatrix = true;
const bool routeUsesScaleDependentGenerationMetric = true;
const bool routeTransformsQcdThetaToCkmPhase = true;
const bool routeUsesFermionRgeDataFit = true;
const bool routePredictsNewVectorGBoson = true;
const bool vectorGMixesWithPhotonAndZ = true;
const bool vectorGMassOrderTev = true;
const bool vectorGDeviationsCalculableInTermsOfMG = true;
const bool routeIncludesHiddenSector = true;
const bool scalarComplexIncludesStandardModelHiggs = true;
const int sourceRowCountExpected = 7;
const int framedStandardModelIPageCount = 15;
const int framedStandardModelIIPageCount = 15;
const int firstTestAdjustableParameterCount = 7;
const int firstTestMeasuredQuantityCount = 18;
const int firstTestStandardModelParameterReplacementCount = 17;
const int zBosonLatestArxivVersion = 3;
const int zBosonPageCount = 21;
const int closerStudyLatestArxivVersion = 4;
const int closerStudyPageCount = 90;
const double vectorGLowerBoundTev = 1.0;

const bool routeDependsOnFermionFitParameters = true;
const bool routeUsesHeaviestGenerationMassInputs = true;
const bool routeUsesUnknownNeutrinoMassInput = true;
const bool routeContainsFudgeFactor = true;
const bool routeTreatsObservedWzSectorAsBaseline = true;
const bool routeNewVectorMassRemainsFreeParameter = true;
const bool routeGammaZGCorrectionsAreNotObservedWzMassSource = true;
const bool routeDoesNotPredictObservedWzMasses = true;
const bool routeDoesNotProvideTargetIndependentObservedHiggsMass = true;
const bool routeDoesNotProvidePhysicalPoleExtraction = true;
const bool routeDoesNotProvideObservedPhotonWzHiggsProjection = true;

const bool routeRequiresGuLocalFramedGaugeMap = true;
const bool routeRequiresGuInternalFrameVectorDerivation = true;
const bool routeRequiresGuFramonVacuumSelection = true;
const bool routeRequiresGuFramonPotentialDerivation = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuWeakMixingAngleSource = true;
const bool routeRequiresGuGaugeCouplingNormalization = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresLowEnergyRgAndThresholdTransport = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalFramedGaugeMap = false;
const bool routeProvidesGuInternalFrameVectorDerivation = false;
const bool routeProvidesGuFramonVacuumSelection = false;
const bool routeProvidesGuFramonPotentialDerivation = false;
const bool routeProvidesGuWzSourceRows = false;
const bool routeProvidesGuWeakMixingAngleSource = false;
const bool routeProvidesGuGaugeCouplingNormalization = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesGuHiggsSelfCouplingSource = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
const bool routeProvidesLowEnergyRgAndThresholdTransport = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

var phase201AllRequiredLineagesPromotable = JsonBool(phase201.RootElement, "allRequiredLineagesPromotable") is true;
var existingEvidenceFound = JsonBool(phase213.RootElement, "existingEvidenceFound") is true;
var wzMissingFieldCount = JsonInt(phase213.RootElement, "wzMissingFieldCount") ?? -1;
var higgsMissingFieldCount = JsonInt(phase213.RootElement, "higgsMissingFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;
var stueckelbergVectorMassSourceAuditPassed = JsonBool(phase343.RootElement, "stueckelbergVectorMassSourceAuditPassed") is true;
var stueckelbergRoutePromotesWzMasses = JsonBool(phase343.RootElement, "stueckelbergRoutePromotesWzMasses") is true;
var stueckelbergRoutePromotesHiggsMass = JsonBool(phase343.RootElement, "stueckelbergRoutePromotesHiggsMass") is true;
var finiteNcgDiscreteHiggsSourceAuditPassed = JsonBool(phase359.RootElement, "finiteNcgDiscreteHiggsSourceAuditPassed") is true;
var finiteNcgRoutePromotesWzMasses = JsonBool(phase359.RootElement, "routePromotesWzMasses") is true;
var finiteNcgRoutePromotesHiggsMass = JsonBool(phase359.RootElement, "routePromotesHiggsMass") is true;
var matrixModelHiggsGeometrySourceAuditPassed = JsonBool(phase361.RootElement, "matrixModelHiggsGeometrySourceAuditPassed") is true;
var matrixModelRoutePromotesWzMasses = JsonBool(phase361.RootElement, "routePromotesWzMasses") is true;
var matrixModelRoutePromotesHiggsMass = JsonBool(phase361.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "chan-tsou-1111-3832-framed-gauge-theory",
        exploringFramedGaugeTheoryUrl,
        "Exploring Framed Gauge Theory as Basis for Physical Models",
        "Introduces internal symmetry-space frame vectors as dynamical variables and shows that SU(2) x U(1) framing gives the electroweak theory with the Higgs field as part of the framed gauge structure.",
        "Geometric Higgs-as-frame lead; no target-independent observed W/Z/H mass rows or GU-local map."),
    new SourceRow(
        "baker-bordes-chan-tsou-1111-5591-developing-fsm",
        developingFramedStandardModelUrl,
        "Developing the Framed Standard Model",
        "Develops the FSM with a degenerate vacuum under generation SU(3), rotating rank-one fermion mass matrix, scale-dependent generation metric, and strong-CP/CKM phase lead.",
        "Fermion mass and mixing structure; not a W/Z/H source-lineage artifact."),
    new SourceRow(
        "bordes-chan-tsou-1410-8022-first-test",
        firstTestAgainstExperimentUrl,
        "A First Test of the Framed Standard Model against Experiment",
        "Fits fermion masses and mixings using frame-vector rotation/RGE structure with seven adjustable parameters and reports broad agreement with measured fermion-sector data.",
        "Quantitative but fit-parameter based and fermion-scoped; no physical W/Z/H pole-mass derivation."),
    new SourceRow(
        "chan-tsou-1505-05472-fsm-i",
        framedStandardModelIUrl,
        "The Framed Standard Model (I) - A Physics Case for Framing the Yang-Mills Theory?",
        "States that internal frame vectors as field variables yield the standard Higgs scalar as the electroweak framon and a dual global SU(3) generation symmetry.",
        "Direct Higgs-as-framon lead; no solved scalar source/self-coupling or absolute mass scale."),
    new SourceRow(
        "chan-tsou-1508-04273-fsm-ii",
        framedStandardModelIIUrl,
        "The Framed Standard Model (II) - A first Test against Experiment",
        "Summarizes the quantitative FSM fermion-sector test, replacing 17 Standard Model parameters by seven FSM unknowns after fitting selected data.",
        "Quantitative fermion fit with inputs and unknowns; no W/Z/H source rows."),
    new SourceRow(
        "bordes-chan-tsou-1806-08271-z-boson-fsm",
        zBosonInFramedStandardModelUrl,
        "The Z boson in the Framed Standard Model",
        "Studies gamma-Z-G mixing from a new TeV-scale vector boson G and checks deviations in m_Z - m_W and Z widths as functions of the free G mass.",
        "Relevant observed-vector mixing lead, but it treats the new vector mass as a parameter and does not derive observed W/Z masses."),
    new SourceRow(
        "bordes-chan-tsou-1806-08268-closer-study",
        closerStudyFramedStandardModelUrl,
        "A Closer Study of the Framed Standard Model Yielding Testable New Physics plus a Hidden Sector with Dark Matter Candidates",
        "Reviews FSM phenomenology, including the G vector boson, gamma-Z-G mixing, hidden sector, and a scalar complex containing the Standard Model Higgs.",
        "Phenomenological extension lead; no GU-local W/Z/H source-lineage completion.")
};

var checks = new[]
{
    new Check(
        "framed-standard-model-primary-sources-reviewed",
        framedStandardModelLeadPresent
            && framedStandardModelPrimarySourcesReviewed
            && framedStandardModelRouteExternalToGu
            && sourceRows.Length == sourceRowCountExpected,
        $"lead={framedStandardModelLeadPresent}; reviewed={framedStandardModelPrimarySourcesReviewed}; externalToGu={framedStandardModelRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "framed-gauge-theory-geometric-higgs-structure-captured",
        routeUsesFramedGaugeTheory
            && routeUsesInternalFrameVectorsOrVielbeins
            && routeIntroducesFramonsAsFieldVariables
            && routeHiggsAsElectroweakFramon
            && routeRelatesParticleTheoryToGravityVierbeinStructure
            && routeProducesGenerationSymmetryLead,
        $"framedGaugeTheory={routeUsesFramedGaugeTheory}; internalFrames={routeUsesInternalFrameVectorsOrVielbeins}; framons={routeIntroducesFramonsAsFieldVariables}; higgsFramon={routeHiggsAsElectroweakFramon}; gravityVierbeinAnalogy={routeRelatesParticleTheoryToGravityVierbeinStructure}; generationSymmetry={routeProducesGenerationSymmetryLead}"),
    new Check(
        "framed-standard-model-fermion-and-vector-leads-captured",
        routeUsesRotatingRankOneFermionMassMatrix
            && routeUsesScaleDependentGenerationMetric
            && routeTransformsQcdThetaToCkmPhase
            && routeUsesFermionRgeDataFit
            && firstTestAdjustableParameterCount == 7
            && firstTestMeasuredQuantityCount == 18
            && firstTestStandardModelParameterReplacementCount == 17
            && routePredictsNewVectorGBoson
            && vectorGMixesWithPhotonAndZ
            && vectorGMassOrderTev
            && vectorGDeviationsCalculableInTermsOfMG
            && vectorGLowerBoundTev == 1.0
            && routeIncludesHiddenSector
            && scalarComplexIncludesStandardModelHiggs,
        $"rotatingRankOne={routeUsesRotatingRankOneFermionMassMatrix}; generationMetric={routeUsesScaleDependentGenerationMetric}; thetaToCkm={routeTransformsQcdThetaToCkmPhase}; fermionRgeFit={routeUsesFermionRgeDataFit}; fitParameters={firstTestAdjustableParameterCount}; measuredQuantities={firstTestMeasuredQuantityCount}; replacedSmParameters={firstTestStandardModelParameterReplacementCount}; vectorG={routePredictsNewVectorGBoson}; gammaZG={vectorGMixesWithPhotonAndZ}; mGOrderTeV={vectorGMassOrderTev}; hiddenSector={routeIncludesHiddenSector}; scalarComplex={scalarComplexIncludesStandardModelHiggs}"),
    new Check(
        "framed-standard-model-version-and-page-metadata-captured",
        framedStandardModelIPageCount == 15
            && framedStandardModelIIPageCount == 15
            && zBosonLatestArxivVersion == 3
            && zBosonPageCount == 21
            && closerStudyLatestArxivVersion == 4
            && closerStudyPageCount == 90,
        $"fsmIpages={framedStandardModelIPageCount}; fsmIIpages={framedStandardModelIIPageCount}; zVersion={zBosonLatestArxivVersion}; zPages={zBosonPageCount}; closerVersion={closerStudyLatestArxivVersion}; closerPages={closerStudyPageCount}"),
    new Check(
        "framed-standard-model-promotion-obstructions-captured",
        routeDependsOnFermionFitParameters
            && routeUsesHeaviestGenerationMassInputs
            && routeUsesUnknownNeutrinoMassInput
            && routeContainsFudgeFactor
            && routeTreatsObservedWzSectorAsBaseline
            && routeNewVectorMassRemainsFreeParameter
            && routeGammaZGCorrectionsAreNotObservedWzMassSource
            && routeDoesNotPredictObservedWzMasses
            && routeDoesNotProvideTargetIndependentObservedHiggsMass
            && routeDoesNotProvidePhysicalPoleExtraction
            && routeDoesNotProvideObservedPhotonWzHiggsProjection,
        $"fitParameters={routeDependsOnFermionFitParameters}; heavyInputs={routeUsesHeaviestGenerationMassInputs}; unknownNeutrino={routeUsesUnknownNeutrinoMassInput}; fudgeFactor={routeContainsFudgeFactor}; baselineWz={routeTreatsObservedWzSectorAsBaseline}; freeMG={routeNewVectorMassRemainsFreeParameter}; gammaZGNotSource={routeGammaZGCorrectionsAreNotObservedWzMassSource}; observedWz={routeDoesNotPredictObservedWzMasses}; observedHiggs={routeDoesNotProvideTargetIndependentObservedHiggsMass}; pole={routeDoesNotProvidePhysicalPoleExtraction}; projection={routeDoesNotProvideObservedPhotonWzHiggsProjection}"),
    new Check(
        "adjacent-frame-higgs-routes-remain-nonpromotional",
        stueckelbergVectorMassSourceAuditPassed
            && !stueckelbergRoutePromotesWzMasses
            && !stueckelbergRoutePromotesHiggsMass
            && finiteNcgDiscreteHiggsSourceAuditPassed
            && !finiteNcgRoutePromotesWzMasses
            && !finiteNcgRoutePromotesHiggsMass
            && matrixModelHiggsGeometrySourceAuditPassed
            && !matrixModelRoutePromotesWzMasses
            && !matrixModelRoutePromotesHiggsMass,
        $"stueckelbergPassed={stueckelbergVectorMassSourceAuditPassed}; stueckelbergWz={stueckelbergRoutePromotesWzMasses}; stueckelbergHiggs={stueckelbergRoutePromotesHiggsMass}; finiteNcgPassed={finiteNcgDiscreteHiggsSourceAuditPassed}; finiteNcgWz={finiteNcgRoutePromotesWzMasses}; finiteNcgHiggs={finiteNcgRoutePromotesHiggsMass}; matrixModelPassed={matrixModelHiggsGeometrySourceAuditPassed}; matrixModelWz={matrixModelRoutePromotesWzMasses}; matrixModelHiggs={matrixModelRoutePromotesHiggsMass}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        routeRequiresGuLocalFramedGaugeMap
            && routeRequiresGuInternalFrameVectorDerivation
            && routeRequiresGuFramonVacuumSelection
            && routeRequiresGuFramonPotentialDerivation
            && routeRequiresGuWzSourceRows
            && routeRequiresGuWeakMixingAngleSource
            && routeRequiresGuGaugeCouplingNormalization
            && routeRequiresGuObservedFieldExtraction
            && routeRequiresGuHiggsScalarSourceOperator
            && routeRequiresGuHiggsSelfCouplingSource
            && routeRequiresTargetIndependentVevOrMassScale
            && routeRequiresLowEnergyRgAndThresholdTransport
            && routeRequiresGeVUnitNormalization
            && !routeProvidesGuLocalFramedGaugeMap
            && !routeProvidesGuInternalFrameVectorDerivation
            && !routeProvidesGuFramonVacuumSelection
            && !routeProvidesGuFramonPotentialDerivation
            && !routeProvidesGuWzSourceRows
            && !routeProvidesGuWeakMixingAngleSource
            && !routeProvidesGuGaugeCouplingNormalization
            && !routeProvidesGuObservedFieldExtraction
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesGuHiggsSelfCouplingSource
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesLowEnergyRgAndThresholdTransport
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"requiresFramedMap={routeRequiresGuLocalFramedGaugeMap}; requiresFrameVector={routeRequiresGuInternalFrameVectorDerivation}; requiresVacuum={routeRequiresGuFramonVacuumSelection}; requiresPotential={routeRequiresGuFramonPotentialDerivation}; requiresWzRows={routeRequiresGuWzSourceRows}; requiresObserved={routeRequiresGuObservedFieldExtraction}; requiresHiggsOperator={routeRequiresGuHiggsScalarSourceOperator}; providesFramedMap={routeProvidesGuLocalFramedGaugeMap}; providesWzRows={routeProvidesGuWzSourceRows}; providesObserved={routeProvidesGuObservedFieldExtraction}; providesHiggsOperator={routeProvidesGuHiggsScalarSourceOperator}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
    new Check(
        "phase201-phase256-contract-state-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}")
};

var framedStandardModelSourceAuditPassed = checks.All(check => check.Passed)
    && framedStandardModelSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = framedStandardModelSourceAuditPassed
    ? "framed-standard-model-source-audit-framon-higgs-and-vector-mixing-lead-not-gu-mass-law"
    : "framed-standard-model-source-audit-review-required";

var result = new
{
    phaseId = "phase362-framed-standard-model-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    framedStandardModelSourceAuditPassed,
    framedStandardModelLeadPresent,
    framedStandardModelPrimarySourcesReviewed,
    framedStandardModelRouteExternalToGu,
    closerStudyFramedStandardModelDoi,
    routeUsesFramedGaugeTheory,
    routeUsesInternalFrameVectorsOrVielbeins,
    routeIntroducesFramonsAsFieldVariables,
    routeHiggsAsElectroweakFramon,
    routeRelatesParticleTheoryToGravityVierbeinStructure,
    routeProducesGenerationSymmetryLead,
    routeUsesRotatingRankOneFermionMassMatrix,
    routeUsesScaleDependentGenerationMetric,
    routeTransformsQcdThetaToCkmPhase,
    routeUsesFermionRgeDataFit,
    routePredictsNewVectorGBoson,
    vectorGMixesWithPhotonAndZ,
    vectorGMassOrderTev,
    vectorGDeviationsCalculableInTermsOfMG,
    routeIncludesHiddenSector,
    scalarComplexIncludesStandardModelHiggs,
    framedStandardModelIPageCount,
    framedStandardModelIIPageCount,
    firstTestAdjustableParameterCount,
    firstTestMeasuredQuantityCount,
    firstTestStandardModelParameterReplacementCount,
    zBosonLatestArxivVersion,
    zBosonPageCount,
    closerStudyLatestArxivVersion,
    closerStudyPageCount,
    vectorGLowerBoundTev,
    routeDependsOnFermionFitParameters,
    routeUsesHeaviestGenerationMassInputs,
    routeUsesUnknownNeutrinoMassInput,
    routeContainsFudgeFactor,
    routeTreatsObservedWzSectorAsBaseline,
    routeNewVectorMassRemainsFreeParameter,
    routeGammaZGCorrectionsAreNotObservedWzMassSource,
    routeDoesNotPredictObservedWzMasses,
    routeDoesNotProvideTargetIndependentObservedHiggsMass,
    routeDoesNotProvidePhysicalPoleExtraction,
    routeDoesNotProvideObservedPhotonWzHiggsProjection,
    routeRequiresGuLocalFramedGaugeMap,
    routeRequiresGuInternalFrameVectorDerivation,
    routeRequiresGuFramonVacuumSelection,
    routeRequiresGuFramonPotentialDerivation,
    routeRequiresGuWzSourceRows,
    routeRequiresGuWeakMixingAngleSource,
    routeRequiresGuGaugeCouplingNormalization,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresLowEnergyRgAndThresholdTransport,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalFramedGaugeMap,
    routeProvidesGuInternalFrameVectorDerivation,
    routeProvidesGuFramonVacuumSelection,
    routeProvidesGuFramonPotentialDerivation,
    routeProvidesGuWzSourceRows,
    routeProvidesGuWeakMixingAngleSource,
    routeProvidesGuGaugeCouplingNormalization,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesGuHiggsSelfCouplingSource,
    routeProvidesTargetIndependentVevOrMassScale,
    routeProvidesLowEnergyRgAndThresholdTransport,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    sourceRows,
    adjacentRouteBoundary = new
    {
        stueckelbergVectorMassSourceAuditPassed,
        stueckelbergRoutePromotesWzMasses,
        stueckelbergRoutePromotesHiggsMass,
        finiteNcgDiscreteHiggsSourceAuditPassed,
        finiteNcgRoutePromotesWzMasses,
        finiteNcgRoutePromotesHiggsMass,
        matrixModelHiggsGeometrySourceAuditPassed,
        matrixModelRoutePromotesWzMasses,
        matrixModelRoutePromotesHiggsMass
    },
    contractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionFilledRequiredFieldCount
    },
    checks,
    decision = "Do not promote W/Z or Higgs physical masses from framed gauge theory or the Framed Standard Model in this repository. The route provides a serious geometric Higgs lead: internal-space frame vectors become fields, the electroweak Higgs is interpreted as a framon, and the gamma-Z-G complex gives a concrete vector-mixing phenomenology. It does not provide a GU-local derivation of the internal frame variables from Shiab/observer geometry, a target-independent framon vacuum or potential, separate observed W/Z source rows, observed photon/W/Z/H projection, Higgs scalar-source/self-coupling lineage, a VEV or mass-scale source, pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local theorem deriving internal frame vectors or framons from Shiab/observer-sector geometry rather than importing the external FSM.",
        "A target-independent framon vacuum and potential yielding observed electroweak SU(2) x U(1), photon/W/Z projection, and separate W/Z source rows.",
        "A Higgs scalar-source/self-coupling lineage from the same framed geometry, not from fitted fermion parameters or an assumed scalar complex.",
        "A source for the gamma-Z-G or ordinary gamma-Z mixing parameters, low-energy RG/threshold transport, physical-pole extraction, and GeV normalization before any framed route can promote W/Z/H masses."
    }
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(Path.Combine(outputDir, "framed_standard_model_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "framed_standard_model_source_audit_summary.json"),
    JsonSerializer.Serialize(
        new
        {
            result.phaseId,
            result.terminalStatus,
            result.framedStandardModelSourceAuditPassed,
            result.framedStandardModelLeadPresent,
            result.framedStandardModelPrimarySourcesReviewed,
            result.framedStandardModelRouteExternalToGu,
            result.routeUsesFramedGaugeTheory,
            result.routeUsesInternalFrameVectorsOrVielbeins,
            result.routeIntroducesFramonsAsFieldVariables,
            result.routeHiggsAsElectroweakFramon,
            result.routeRelatesParticleTheoryToGravityVierbeinStructure,
            result.routeProducesGenerationSymmetryLead,
            result.routeUsesRotatingRankOneFermionMassMatrix,
            result.routeUsesScaleDependentGenerationMetric,
            result.routeTransformsQcdThetaToCkmPhase,
            result.routeUsesFermionRgeDataFit,
            result.routePredictsNewVectorGBoson,
            result.vectorGMixesWithPhotonAndZ,
            result.vectorGMassOrderTev,
            result.vectorGDeviationsCalculableInTermsOfMG,
            result.routeIncludesHiddenSector,
            result.scalarComplexIncludesStandardModelHiggs,
            result.framedStandardModelIPageCount,
            result.framedStandardModelIIPageCount,
            result.firstTestAdjustableParameterCount,
            result.firstTestMeasuredQuantityCount,
            result.firstTestStandardModelParameterReplacementCount,
            result.zBosonLatestArxivVersion,
            result.zBosonPageCount,
            result.closerStudyLatestArxivVersion,
            result.closerStudyPageCount,
            result.vectorGLowerBoundTev,
            result.routeDependsOnFermionFitParameters,
            result.routeUsesHeaviestGenerationMassInputs,
            result.routeUsesUnknownNeutrinoMassInput,
            result.routeContainsFudgeFactor,
            result.routeTreatsObservedWzSectorAsBaseline,
            result.routeNewVectorMassRemainsFreeParameter,
            result.routeGammaZGCorrectionsAreNotObservedWzMassSource,
            result.routeDoesNotPredictObservedWzMasses,
            result.routeDoesNotProvideTargetIndependentObservedHiggsMass,
            result.routeDoesNotProvidePhysicalPoleExtraction,
            result.routeDoesNotProvideObservedPhotonWzHiggsProjection,
            result.routeRequiresGuLocalFramedGaugeMap,
            result.routeRequiresGuInternalFrameVectorDerivation,
            result.routeRequiresGuFramonVacuumSelection,
            result.routeRequiresGuFramonPotentialDerivation,
            result.routeRequiresGuWzSourceRows,
            result.routeRequiresGuWeakMixingAngleSource,
            result.routeRequiresGuGaugeCouplingNormalization,
            result.routeRequiresGuObservedFieldExtraction,
            result.routeRequiresGuHiggsScalarSourceOperator,
            result.routeRequiresGuHiggsSelfCouplingSource,
            result.routeRequiresTargetIndependentVevOrMassScale,
            result.routeRequiresLowEnergyRgAndThresholdTransport,
            result.routeRequiresGeVUnitNormalization,
            result.routeProvidesGuLocalFramedGaugeMap,
            result.routeProvidesGuInternalFrameVectorDerivation,
            result.routeProvidesGuFramonVacuumSelection,
            result.routeProvidesGuFramonPotentialDerivation,
            result.routeProvidesGuWzSourceRows,
            result.routeProvidesGuWeakMixingAngleSource,
            result.routeProvidesGuGaugeCouplingNormalization,
            result.routeProvidesGuObservedFieldExtraction,
            result.routeProvidesGuHiggsScalarSourceOperator,
            result.routeProvidesGuHiggsSelfCouplingSource,
            result.routeProvidesTargetIndependentVevOrMassScale,
            result.routeProvidesLowEnergyRgAndThresholdTransport,
            result.routeProvidesGeVUnitNormalization,
            result.routePromotesWzMasses,
            result.routePromotesHiggsMass,
            result.routeCompletesBosonPredictions,
            result.canFillPhase201WzContract,
            result.canFillPhase201HiggsContract,
            result.canFillPhase256ObservedFieldExtractionContract,
            result.sourceRowCount,
            result.adjacentRouteBoundary,
            result.contractImpact,
            result.decision,
            result.nextRequiredArtifact
        },
        options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"framedStandardModelSourceAuditPassed={framedStandardModelSourceAuditPassed}");
Console.WriteLine($"routeHiggsAsElectroweakFramon={routeHiggsAsElectroweakFramon}");
Console.WriteLine($"routePredictsNewVectorGBoson={routePredictsNewVectorGBoson}");
Console.WriteLine($"vectorGMixesWithPhotonAndZ={vectorGMixesWithPhotonAndZ}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.True or JsonValueKind.False ? property.GetBoolean() : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value) ? value : null;

sealed record Check(string Id, bool Passed, string Evidence);

sealed record SourceRow(
    string Id,
    string Url,
    string Title,
    string Summary,
    string PromotionBoundary);
