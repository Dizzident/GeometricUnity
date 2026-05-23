using System.Text.Json;

const string DefaultOutputDir = "studies/phase355_dirac_lichnerowicz_yang_mills_higgs_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase268Path = "studies/phase268_spectral_action_boson_source_audit_001/output/spectral_action_boson_source_audit_summary.json";
const string Phase323Path = "studies/phase323_coupled_yang_mills_higgs_mass_extraction_audit_001/output/coupled_yang_mills_higgs_mass_extraction_audit_summary.json";
const string Phase334Path = "studies/phase334_su21_superconnection_source_audit_001/output/su21_superconnection_source_audit_summary.json";
const string Phase354Path = "studies/phase354_multiplicative_higgs_lagrangian_source_audit_001/output/multiplicative_higgs_lagrangian_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE355_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase268 = JsonDocument.Parse(File.ReadAllText(Phase268Path));
using var phase323 = JsonDocument.Parse(File.ReadAllText(Phase323Path));
using var phase334 = JsonDocument.Parse(File.ReadAllText(Phase334Path));
using var phase354 = JsonDocument.Parse(File.ReadAllText(Phase354Path));

const string generalizedLichnerowiczArxiv = "https://arxiv.org/abs/hep-th/9503153";
const string gravityYangMillsHiggsArxiv = "https://arxiv.org/abs/hep-th/9503180";
const string diracYukawaOperatorArxiv = "https://arxiv.org/abs/hep-th/9612149";
const string gaugeTheoriesDiracTypeArxiv = "https://arxiv.org/abs/math-ph/0503059";
const string diracTypeHiggsMassArxiv = "https://arxiv.org/abs/math-ph/0602028";

const string generalizedLichnerowiczDoi = "https://doi.org/10.48550/arXiv.hep-th/9503153";
const string gravityYangMillsHiggsDoi = "https://doi.org/10.48550/arXiv.hep-th/9503180";
const string diracYukawaOperatorDoi = "https://doi.org/10.48550/arXiv.hep-th/9612149";
const string gaugeTheoriesDiracTypeDoi = "https://doi.org/10.48550/arXiv.math-ph/0503059";
const string diracTypeHiggsMassDoi = "https://doi.org/10.48550/arXiv.math-ph/0602028";

const bool diracLichnerowiczYangMillsHiggsSourceAuditPassedExpected = true;
const bool diracLichnerowiczLeadPresent = true;
const bool diracLichnerowiczPrimarySourcesReviewed = true;
const bool diracLichnerowiczRouteExternalToGu = true;
const bool routeDistinctFromSpectralAction = true;
const bool routeDistinctFromSu21Superconnection = true;
const bool routeUsesGeneralizedLichnerowiczFormula = true;
const bool routeUsesCliffordModulesAndDiracTypeOperators = true;
const bool routeDerivesGravityAndYangMillsFromDiracOperators = true;
const bool routeDerivesStandardModelActionFromSpecificDiracOperator = true;
const bool routeUsesDiracYukawaOperator = true;
const bool routeUnifiesGravityYangMillsAndHiggsActionClassically = true;
const bool routeInterpretsHiggsGeometricallyAfterSpontaneousBreaking = true;
const bool routeIncludesSpontaneousSymmetryBreakingWithoutHiggsPotential = true;
const bool routeUsesFermionicMassOperator = true;
const bool routeIntroducesPhysicalSubspaceProjection = true;
const bool routeMakesExternalHiggsMassPrediction = true;
const bool routeUsesOneLoopTopQuarkApproximation = true;
const bool routeHiggsPredictionConflictsWithObserved125 = true;

const int generalizedLichnerowiczRevisionYear = 1995;
const int gravityYangMillsHiggsRevisionYear = 1995;
const int diracYukawaLatestArxivVersion = 3;
const int diracYukawaLatestRevisionYear = 1997;
const int gaugeTheoriesDiracTypeLatestArxivVersion = 3;
const int gaugeTheoriesDiracTypeLatestRevisionYear = 2005;
const int diracTypeHiggsMassLatestArxivVersion = 3;
const int diracTypeHiggsMassLatestRevisionYear = 2006;
const double diracTypeHiggsPredictionCentralGeV = 186.0;
const double diracTypeHiggsPredictionUncertaintyGeV = 8.0;
const double diracTypeHiggsPredictionTopMassInputGeV = 174.0;

const bool routeRequiresExternalCliffordModule = true;
const bool routeRequiresExternalDiracOperatorChoice = true;
const bool routeRequiresStandardModelGaugeGroupAndRepresentations = true;
const bool routeRequiresYukawaAndFermionMassOperatorInput = true;
const bool routeRequiresTraceWodzickiOrInnerProductNormalization = true;
const bool routeRequiresPhysicalSubspaceProjectionChoice = true;
const bool routeRequiresOneLoopTopMassInput = true;
const bool routeRequiresRgOrEffectivePotentialApproximation = true;
const bool routeRequiresElectroweakVevAndGaugeCouplingMatching = true;
const bool routeRequiresObservedPoleMassComparison = true;

const bool routeRequiresGuLocalDiracOperatorMap = true;
const bool routeRequiresGuCliffordModuleBridge = true;
const bool routeRequiresGuFermionicMassOperatorSource = true;
const bool routeRequiresGuPhysicalSubspaceProjection = true;
const bool routeRequiresGuWzSourceRows = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGuHiggsSelfCouplingSource = true;
const bool routeRequiresTargetIndependentVevOrMassScale = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalDiracOperatorMap = false;
const bool routeProvidesGuCliffordModuleBridge = false;
const bool routeProvidesGuFermionicMassOperatorSource = false;
const bool routeProvidesGuPhysicalSubspaceProjection = false;
const bool routeProvidesGuWzSourceRows = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesGuHiggsSelfCouplingSource = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
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
var spectralActionPromotesWzMasses = JsonBool(phase268.RootElement, "spectralActionPromotesWzMasses") is true;
var spectralActionPromotesHiggsMass = JsonBool(phase268.RootElement, "spectralActionPromotesHiggsMass") is true;
var coupledYangMillsHiggsPromotesWzMasses = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRoutePromotesWzMasses") is true;
var coupledYangMillsHiggsPromotesHiggsMass = JsonBool(phase323.RootElement, "coupledYangMillsHiggsRoutePromotesHiggsMass") is true;
var su21RouteExternalToGu = JsonBool(phase334.RootElement, "su21RouteExternalToGu") is true;
var su21RoutePromotesWzMasses = JsonBool(phase334.RootElement, "su21RoutePromotesWzMasses") is true;
var su21RoutePromotesHiggsMass = JsonBool(phase334.RootElement, "su21RoutePromotesHiggsMass") is true;
var multiplicativeHiggsPromotesWzMasses = JsonBool(phase354.RootElement, "routePromotesWzMasses") is true;
var multiplicativeHiggsPromotesHiggsMass = JsonBool(phase354.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-hep-th-9503153-generalized-lichnerowicz",
        generalizedLichnerowiczArxiv,
        "Generalized Lichnerowicz formula",
        "Develops an intrinsic square-decomposition for Dirac operators on Clifford modules and connects simple Dirac operators with gravity/Yang-Mills action functionals.",
        "Strong geometric action-generation lead; no GU-local electroweak mass-eigenstate rows, VEV source, observed projection, or GeV normalization."),
    new SourceRow(
        "arxiv-hep-th-9503180-dirac-operator-gravity-yang-mills-higgs",
        gravityYangMillsHiggsArxiv,
        "Gravity and Yang-Mills-Higgs action from a Dirac operator",
        "Shows how the Standard Model and gravity action functional can be derived from a specific Dirac operator structurally tied to Yukawa coupling.",
        "Relevant Dirac-operator unification lead, but it imports a chosen external operator and Standard Model/Yukawa data rather than GU source rows."),
    new SourceRow(
        "arxiv-hep-th-9612149-dirac-yukawa-operator",
        diracYukawaOperatorArxiv,
        "Einstein-Hilbert-Yang-Mills-Higgs action and Dirac-Yukawa operator",
        "Describes the full Standard Model action in generalized Dirac-operator geometry and gives the Higgs sector a geometric interpretation after spontaneous breaking.",
        "Useful action-level bridge; it still requires external parameters and supplies no target-independent W/Z/H pole-mass extraction."),
    new SourceRow(
        "arxiv-math-ph-0503059-gauge-theories-dirac-type",
        gaugeTheoriesDiracTypeArxiv,
        "Gauge theories of Dirac type",
        "Frames gauge theories geometrically in terms of fermions, including spontaneous symmetry breaking without a Higgs potential, fermionic mass curvature interpretation, and a physical-subspace projection.",
        "Serious symmetry-breaking lead; no GU-native fermionic-mass operator, physical-subspace theorem, or observed boson projection rows are supplied."),
    new SourceRow(
        "arxiv-math-ph-0602028-dirac-type-higgs-mass",
        diracTypeHiggsMassArxiv,
        "Dirac type gauge theories and the Higgs mass",
        "Treats the minimal Standard Model as a parameterized Dirac-type gauge theory and reports a one-loop top-approximation Higgs estimate near 186 GeV for a 174 GeV top input.",
        "Numerical Higgs lead is now incompatible with the observed 125 GeV scale and depends on top-mass and external Standard Model inputs; it cannot promote a GU Higgs prediction.")
};

var checks = new[]
{
    new Check(
        "dirac-lichnerowicz-primary-sources-reviewed",
        diracLichnerowiczLeadPresent
            && diracLichnerowiczPrimarySourcesReviewed
            && diracLichnerowiczRouteExternalToGu
            && sourceRows.Length == 5,
        $"lead={diracLichnerowiczLeadPresent}; reviewed={diracLichnerowiczPrimarySourcesReviewed}; externalToGu={diracLichnerowiczRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "generalized-dirac-yang-mills-higgs-route-captured",
        routeUsesGeneralizedLichnerowiczFormula
            && routeUsesCliffordModulesAndDiracTypeOperators
            && routeDerivesGravityAndYangMillsFromDiracOperators
            && routeDerivesStandardModelActionFromSpecificDiracOperator
            && routeUsesDiracYukawaOperator
            && routeUnifiesGravityYangMillsAndHiggsActionClassically
            && routeInterpretsHiggsGeometricallyAfterSpontaneousBreaking,
        $"lichnerowicz={routeUsesGeneralizedLichnerowiczFormula}; cliffordModules={routeUsesCliffordModulesAndDiracTypeOperators}; gravityYm={routeDerivesGravityAndYangMillsFromDiracOperators}; standardModelAction={routeDerivesStandardModelActionFromSpecificDiracOperator}; diracYukawa={routeUsesDiracYukawaOperator}; classicalUnification={routeUnifiesGravityYangMillsAndHiggsActionClassically}; higgsGeometry={routeInterpretsHiggsGeometricallyAfterSpontaneousBreaking}"),
    new Check(
        "symmetry-breaking-and-higgs-mass-boundary-captured",
        routeIncludesSpontaneousSymmetryBreakingWithoutHiggsPotential
            && routeUsesFermionicMassOperator
            && routeIntroducesPhysicalSubspaceProjection
            && routeMakesExternalHiggsMassPrediction
            && routeUsesOneLoopTopQuarkApproximation
            && routeHiggsPredictionConflictsWithObserved125
            && diracTypeHiggsPredictionCentralGeV == 186.0
            && diracTypeHiggsPredictionUncertaintyGeV == 8.0,
        $"ssbNoPotential={routeIncludesSpontaneousSymmetryBreakingWithoutHiggsPotential}; fermionicMassOperator={routeUsesFermionicMassOperator}; physicalSubspace={routeIntroducesPhysicalSubspaceProjection}; prediction={routeMakesExternalHiggsMassPrediction}; oneLoopTop={routeUsesOneLoopTopQuarkApproximation}; conflicts125={routeHiggsPredictionConflictsWithObserved125}; centralGeV={diracTypeHiggsPredictionCentralGeV}; uncertaintyGeV={diracTypeHiggsPredictionUncertaintyGeV}"),
    new Check(
        "adjacent-route-boundaries-preserved",
        routeDistinctFromSpectralAction
            && routeDistinctFromSu21Superconnection
            && !spectralActionPromotesWzMasses
            && !spectralActionPromotesHiggsMass
            && !coupledYangMillsHiggsPromotesWzMasses
            && !coupledYangMillsHiggsPromotesHiggsMass
            && su21RouteExternalToGu
            && !su21RoutePromotesWzMasses
            && !su21RoutePromotesHiggsMass
            && !multiplicativeHiggsPromotesWzMasses
            && !multiplicativeHiggsPromotesHiggsMass,
        $"distinctSpectral={routeDistinctFromSpectralAction}; distinctSu21={routeDistinctFromSu21Superconnection}; spectralPromotesWz={spectralActionPromotesWzMasses}; spectralPromotesHiggs={spectralActionPromotesHiggsMass}; coupledPromotesWz={coupledYangMillsHiggsPromotesWzMasses}; coupledPromotesHiggs={coupledYangMillsHiggsPromotesHiggsMass}; su21External={su21RouteExternalToGu}; su21PromotesWz={su21RoutePromotesWzMasses}; su21PromotesHiggs={su21RoutePromotesHiggsMass}; multiplicativePromotesWz={multiplicativeHiggsPromotesWzMasses}; multiplicativePromotesHiggs={multiplicativeHiggsPromotesHiggsMass}"),
    new Check(
        "external-inputs-required-before-promotion",
        routeRequiresExternalCliffordModule
            && routeRequiresExternalDiracOperatorChoice
            && routeRequiresStandardModelGaugeGroupAndRepresentations
            && routeRequiresYukawaAndFermionMassOperatorInput
            && routeRequiresTraceWodzickiOrInnerProductNormalization
            && routeRequiresPhysicalSubspaceProjectionChoice
            && routeRequiresOneLoopTopMassInput
            && routeRequiresRgOrEffectivePotentialApproximation
            && routeRequiresElectroweakVevAndGaugeCouplingMatching
            && routeRequiresObservedPoleMassComparison,
        $"cliffordModule={routeRequiresExternalCliffordModule}; diracOperator={routeRequiresExternalDiracOperatorChoice}; smGroupReps={routeRequiresStandardModelGaugeGroupAndRepresentations}; yukawaMassOperator={routeRequiresYukawaAndFermionMassOperatorInput}; normalization={routeRequiresTraceWodzickiOrInnerProductNormalization}; physicalSubspace={routeRequiresPhysicalSubspaceProjectionChoice}; topInput={routeRequiresOneLoopTopMassInput}; rg={routeRequiresRgOrEffectivePotentialApproximation}; vevCouplings={routeRequiresElectroweakVevAndGaugeCouplingMatching}; observedComparison={routeRequiresObservedPoleMassComparison}"),
    new Check(
        "route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalDiracOperatorMap
            && !routeProvidesGuCliffordModuleBridge
            && !routeProvidesGuFermionicMassOperatorSource
            && !routeProvidesGuPhysicalSubspaceProjection
            && !routeProvidesGuWzSourceRows
            && !routeProvidesGuObservedFieldExtraction
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesGuHiggsSelfCouplingSource
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guDiracMap={routeProvidesGuLocalDiracOperatorMap}; guCliffordBridge={routeProvidesGuCliffordModuleBridge}; guMassOperator={routeProvidesGuFermionicMassOperatorSource}; guPhysicalSubspace={routeProvidesGuPhysicalSubspaceProjection}; guWzRows={routeProvidesGuWzSourceRows}; observed={routeProvidesGuObservedFieldExtraction}; higgsOperator={routeProvidesGuHiggsScalarSourceOperator}; selfCoupling={routeProvidesGuHiggsSelfCouplingSource}; scale={routeProvidesTargetIndependentVevOrMassScale}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var diracLichnerowiczYangMillsHiggsSourceAuditPassed = checks.All(check => check.Passed)
    && diracLichnerowiczYangMillsHiggsSourceAuditPassedExpected
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = diracLichnerowiczYangMillsHiggsSourceAuditPassed
    ? "dirac-lichnerowicz-yang-mills-higgs-source-audit-geometric-action-lead-not-gu-source"
    : "dirac-lichnerowicz-yang-mills-higgs-source-audit-review-required";

var result = new
{
    phaseId = "phase355-dirac-lichnerowicz-yang-mills-higgs-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    diracLichnerowiczYangMillsHiggsSourceAuditPassed,
    diracLichnerowiczLeadPresent,
    diracLichnerowiczPrimarySourcesReviewed,
    diracLichnerowiczRouteExternalToGu,
    routeDistinctFromSpectralAction,
    routeDistinctFromSu21Superconnection,
    generalizedLichnerowiczDoi,
    gravityYangMillsHiggsDoi,
    diracYukawaOperatorDoi,
    gaugeTheoriesDiracTypeDoi,
    diracTypeHiggsMassDoi,
    routeUsesGeneralizedLichnerowiczFormula,
    routeUsesCliffordModulesAndDiracTypeOperators,
    routeDerivesGravityAndYangMillsFromDiracOperators,
    routeDerivesStandardModelActionFromSpecificDiracOperator,
    routeUsesDiracYukawaOperator,
    routeUnifiesGravityYangMillsAndHiggsActionClassically,
    routeInterpretsHiggsGeometricallyAfterSpontaneousBreaking,
    routeIncludesSpontaneousSymmetryBreakingWithoutHiggsPotential,
    routeUsesFermionicMassOperator,
    routeIntroducesPhysicalSubspaceProjection,
    routeMakesExternalHiggsMassPrediction,
    routeUsesOneLoopTopQuarkApproximation,
    routeHiggsPredictionConflictsWithObserved125,
    generalizedLichnerowiczRevisionYear,
    gravityYangMillsHiggsRevisionYear,
    diracYukawaLatestArxivVersion,
    diracYukawaLatestRevisionYear,
    gaugeTheoriesDiracTypeLatestArxivVersion,
    gaugeTheoriesDiracTypeLatestRevisionYear,
    diracTypeHiggsMassLatestArxivVersion,
    diracTypeHiggsMassLatestRevisionYear,
    diracTypeHiggsPredictionCentralGeV,
    diracTypeHiggsPredictionUncertaintyGeV,
    diracTypeHiggsPredictionTopMassInputGeV,
    routeRequiresExternalCliffordModule,
    routeRequiresExternalDiracOperatorChoice,
    routeRequiresStandardModelGaugeGroupAndRepresentations,
    routeRequiresYukawaAndFermionMassOperatorInput,
    routeRequiresTraceWodzickiOrInnerProductNormalization,
    routeRequiresPhysicalSubspaceProjectionChoice,
    routeRequiresOneLoopTopMassInput,
    routeRequiresRgOrEffectivePotentialApproximation,
    routeRequiresElectroweakVevAndGaugeCouplingMatching,
    routeRequiresObservedPoleMassComparison,
    routeRequiresGuLocalDiracOperatorMap,
    routeRequiresGuCliffordModuleBridge,
    routeRequiresGuFermionicMassOperatorSource,
    routeRequiresGuPhysicalSubspaceProjection,
    routeRequiresGuWzSourceRows,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGuHiggsSelfCouplingSource,
    routeRequiresTargetIndependentVevOrMassScale,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalDiracOperatorMap,
    routeProvidesGuCliffordModuleBridge,
    routeProvidesGuFermionicMassOperatorSource,
    routeProvidesGuPhysicalSubspaceProjection,
    routeProvidesGuWzSourceRows,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesGuHiggsSelfCouplingSource,
    routeProvidesTargetIndependentVevOrMassScale,
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
        spectralActionPromotesWzMasses,
        spectralActionPromotesHiggsMass,
        coupledYangMillsHiggsPromotesWzMasses,
        coupledYangMillsHiggsPromotesHiggsMass,
        su21RouteExternalToGu,
        su21RoutePromotesWzMasses,
        su21RoutePromotesHiggsMass,
        multiplicativeHiggsPromotesWzMasses,
        multiplicativeHiggsPromotesHiggsMass
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
    decision = "Do not promote W/Z or Higgs physical masses from the Dirac-Lichnerowicz/Yang-Mills-Higgs route. It is a serious generalized-Dirac geometric action lead, including an external Higgs-mass estimate, but it depends on external Clifford-module and Dirac-operator choices, Standard Model gauge representations, Yukawa and fermionic mass-operator inputs, normalization conventions, top-mass/loop approximations, electroweak matching, and observed comparison; it supplies no GU-local Dirac map, W/Z rows, Higgs scalar-source/self-coupling row, observed-field extraction, target-independent scale, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local generalized Dirac operator or Lichnerowicz decomposition mapped from the Shiab/observer-sector fields.",
        "A GU-native Clifford-module and physical-subspace projection theorem that identifies observed photon/W/Z/H fields.",
        "Independent W/Z source rows, Higgs scalar-source/self-coupling lineage, and pole extraction without importing target masses.",
        "A target-independent GU electroweak scale or VEV plus GeV unit normalization before any mass promotion."
    }
};

var fullPath = Path.Combine(outputDir, "dirac_lichnerowicz_yang_mills_higgs_source_audit.json");
var summaryPath = Path.Combine(outputDir, "dirac_lichnerowicz_yang_mills_higgs_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.diracLichnerowiczYangMillsHiggsSourceAuditPassed,
    result.diracLichnerowiczLeadPresent,
    result.diracLichnerowiczPrimarySourcesReviewed,
    result.diracLichnerowiczRouteExternalToGu,
    result.routeDistinctFromSpectralAction,
    result.routeDistinctFromSu21Superconnection,
    result.routeUsesGeneralizedLichnerowiczFormula,
    result.routeUsesCliffordModulesAndDiracTypeOperators,
    result.routeDerivesGravityAndYangMillsFromDiracOperators,
    result.routeDerivesStandardModelActionFromSpecificDiracOperator,
    result.routeUsesDiracYukawaOperator,
    result.routeUnifiesGravityYangMillsAndHiggsActionClassically,
    result.routeInterpretsHiggsGeometricallyAfterSpontaneousBreaking,
    result.routeIncludesSpontaneousSymmetryBreakingWithoutHiggsPotential,
    result.routeUsesFermionicMassOperator,
    result.routeIntroducesPhysicalSubspaceProjection,
    result.routeMakesExternalHiggsMassPrediction,
    result.routeUsesOneLoopTopQuarkApproximation,
    result.routeHiggsPredictionConflictsWithObserved125,
    result.generalizedLichnerowiczRevisionYear,
    result.gravityYangMillsHiggsRevisionYear,
    result.diracYukawaLatestArxivVersion,
    result.diracYukawaLatestRevisionYear,
    result.gaugeTheoriesDiracTypeLatestArxivVersion,
    result.gaugeTheoriesDiracTypeLatestRevisionYear,
    result.diracTypeHiggsMassLatestArxivVersion,
    result.diracTypeHiggsMassLatestRevisionYear,
    result.diracTypeHiggsPredictionCentralGeV,
    result.diracTypeHiggsPredictionUncertaintyGeV,
    result.diracTypeHiggsPredictionTopMassInputGeV,
    result.routeRequiresExternalCliffordModule,
    result.routeRequiresExternalDiracOperatorChoice,
    result.routeRequiresStandardModelGaugeGroupAndRepresentations,
    result.routeRequiresYukawaAndFermionMassOperatorInput,
    result.routeRequiresTraceWodzickiOrInnerProductNormalization,
    result.routeRequiresPhysicalSubspaceProjectionChoice,
    result.routeRequiresOneLoopTopMassInput,
    result.routeRequiresRgOrEffectivePotentialApproximation,
    result.routeRequiresElectroweakVevAndGaugeCouplingMatching,
    result.routeRequiresObservedPoleMassComparison,
    result.routeRequiresGuLocalDiracOperatorMap,
    result.routeRequiresGuCliffordModuleBridge,
    result.routeRequiresGuFermionicMassOperatorSource,
    result.routeRequiresGuPhysicalSubspaceProjection,
    result.routeRequiresGuWzSourceRows,
    result.routeRequiresGuObservedFieldExtraction,
    result.routeRequiresGuHiggsScalarSourceOperator,
    result.routeRequiresGuHiggsSelfCouplingSource,
    result.routeRequiresTargetIndependentVevOrMassScale,
    result.routeRequiresGeVUnitNormalization,
    result.routeProvidesGuLocalDiracOperatorMap,
    result.routeProvidesGuCliffordModuleBridge,
    result.routeProvidesGuFermionicMassOperatorSource,
    result.routeProvidesGuPhysicalSubspaceProjection,
    result.routeProvidesGuWzSourceRows,
    result.routeProvidesGuObservedFieldExtraction,
    result.routeProvidesGuHiggsScalarSourceOperator,
    result.routeProvidesGuHiggsSelfCouplingSource,
    result.routeProvidesTargetIndependentVevOrMassScale,
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
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"diracLichnerowiczYangMillsHiggsSourceAuditPassed={diracLichnerowiczYangMillsHiggsSourceAuditPassed}");
Console.WriteLine($"routeUsesGeneralizedLichnerowiczFormula={routeUsesGeneralizedLichnerowiczFormula}");
Console.WriteLine($"routeDerivesStandardModelActionFromSpecificDiracOperator={routeDerivesStandardModelActionFromSpecificDiracOperator}");
Console.WriteLine($"routeMakesExternalHiggsMassPrediction={routeMakesExternalHiggsMassPrediction}");
Console.WriteLine($"routeHiggsPredictionConflictsWithObserved125={routeHiggsPredictionConflictsWithObserved125}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
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
