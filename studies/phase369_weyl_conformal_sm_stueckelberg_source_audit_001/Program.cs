using System.Text.Json;

const string DefaultOutputDir = "studies/phase369_weyl_conformal_sm_stueckelberg_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase224Path = "studies/phase224_electroweak_parameter_dependency_audit_001/output/electroweak_parameter_dependency_audit_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase330Path = "studies/phase330_weyl_geometric_mass_generation_source_audit_001/output/weyl_geometric_mass_generation_source_audit_summary.json";
const string Phase343Path = "studies/phase343_stueckelberg_vector_mass_source_audit_001/output/stueckelberg_vector_mass_source_audit_summary.json";
const string Phase346Path = "studies/phase346_nielsen_pole_mass_gauge_independence_source_audit_001/output/nielsen_pole_mass_gauge_independence_source_audit_summary.json";
const string Phase368Path = "studies/phase368_oxford_inhomogeneous_gauge_equation_bridge_audit_001/output/oxford_inhomogeneous_gauge_equation_bridge_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE369_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase224 = JsonDocument.Parse(File.ReadAllText(Phase224Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase330 = JsonDocument.Parse(File.ReadAllText(Phase330Path));
using var phase343 = JsonDocument.Parse(File.ReadAllText(Phase343Path));
using var phase346 = JsonDocument.Parse(File.ReadAllText(Phase346Path));
using var phase368 = JsonDocument.Parse(File.ReadAllText(Phase368Path));

const string ghilenceaSmWeylUrl = "https://arxiv.org/abs/2104.15118";
const string ghilenceaSmWeylPdfUrl = "https://arxiv.org/pdf/2104.15118";
const string ghilenceaSmWeylEpjcUrl = "https://link.springer.com/article/10.1140/epjc/s10052-021-09887-y";
const string ghilenceaHillLocalVsGaugedUrl = "https://arxiv.org/abs/2303.02515";

const bool weylConformalSmStueckelbergSourceAuditPassedExpected = true;
const bool weylConformalSmLeadPresent = true;
const bool weylConformalSmPrimarySourceReviewed = true;
const bool weylConformalSmRouteExternalToGu = true;
const bool routeUsesWeylConformalGeometry = true;
const bool routeUsesGaugedScaleSymmetryD1 = true;
const bool routeUsesGeometricStueckelbergMechanism = true;
const bool routeGeneratesPlanckScaleFromStueckelbergField = true;
const bool routeInducesElectroweakSymmetryBreakingFromWeylGeometry = true;
const bool routeGeneratesHiggsPotentialFromGeometry = true;
const bool routeGivesHiggsWeylGaugeBosonCouplings = true;
const bool routeAllowsHyperchargeWeylKineticMixing = true;
const bool routeProvidesPhotonZAndWeylBosonNeutralMixingMatrix = true;
const bool routeKeepsPhotonMasslessByNeutralMassMatrixDeterminant = true;
const bool routeProvidesZMassCorrectionFromWeylMixing = true;
const bool routeConstrainsWeylParametersUsingObservedZMass = true;
const bool routeProvidesExternalGeometricMassGenerationTemplate = true;

const bool routeProvidesGuLocalWeylEmbedding = false;
const bool routeProvidesGuLocalWzTheorem = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool routeProvidesSeparateWzSourceRows = false;
const bool routeProvidesWzRawAmplitudeGates = false;
const bool routeProvidesWzCommonBridgeGate = false;
const bool routeProvidesWzTargetComparisonGate = false;
const bool routeProvidesWzStabilitySidecars = false;
const bool routeProvidesTargetIndependentGuVevSource = false;
const bool routeProvidesWeakMixingAngleSource = false;
const bool routeProvidesGaugeCouplingNormalization = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesGuObservedFieldExtraction = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesHiggsIdentityEnvelope = false;
const bool routeProvidesHiggsQuarticOrExcitationSource = false;
const bool routeProvidesTargetIndependentHiggsMass = false;
const bool routeProvidesPoleMassExtraction = false;
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
var observedFieldExtractionRequiredFieldCount = JsonInt(phase256.RootElement, "requiredFieldCount") ?? -1;
var observedFieldExtractionFilledRequiredFieldCount = JsonInt(phase256.RootElement, "filledRequiredFieldCount") ?? -1;
var observedFieldExtractionContractPromotable = JsonBool(phase256.RootElement, "observedFieldExtractionContractPromotable") is true;

var electroweakParameterAuditPassed = JsonBool(phase224.RootElement, "electroweakParameterAuditPassed") is true;
var wAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "wAbsoluteMassParameterClosure") is true;
var zAbsoluteMassParameterClosure = JsonBool(phase224.RootElement, "zAbsoluteMassParameterClosure") is true;
var higgsMassParameterClosure = JsonBool(phase224.RootElement, "higgsMassParameterClosure") is true;
var weakCouplingSourcePromotable = JsonBool(phase224.RootElement, "weakCouplingSourcePromotable") is true;

var electroweakMassMatrixBridgeSourceAuditPassed = JsonBool(phase317.RootElement, "electroweakMassMatrixBridgeSourceAuditPassed") is true;
var smMassGenerationRequiresVev = JsonBool(phase317.RootElement, "smMassGenerationRequiresVev") is true;
var smMassGenerationRequiresWeakCouplingG = JsonBool(phase317.RootElement, "smMassGenerationRequiresWeakCouplingG") is true;
var smMassGenerationRequiresHyperchargeCouplingGPrime = JsonBool(phase317.RootElement, "smMassGenerationRequiresHyperchargeCouplingGPrime") is true;
var smTreeLevelMwDependsOnGAndV = JsonBool(phase317.RootElement, "smTreeLevelMwDependsOnGAndV") is true;
var smTreeLevelMzDependsOnGAndGPrimeAndV = JsonBool(phase317.RootElement, "smTreeLevelMzDependsOnGAndGPrimeAndV") is true;
var smTreeLevelHiggsMassDependsOnPotentialParameter = JsonBool(phase317.RootElement, "smTreeLevelHiggsMassDependsOnPotentialParameter") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;

var weylGeometricMassGenerationSourceAuditPassed = JsonBool(phase330.RootElement, "weylGeometricMassGenerationSourceAuditPassed") is true;
var phase330WeylRouteExternalToGu = JsonBool(phase330.RootElement, "weylRouteExternalToGu") is true;
var phase330WeylRoutePromotesWzMasses = JsonBool(phase330.RootElement, "weylRoutePromotesWzMasses") is true;
var phase330WeylRoutePromotesHiggsMass = JsonBool(phase330.RootElement, "weylRoutePromotesHiggsMass") is true;

var stueckelbergVectorMassSourceAuditPassed = JsonBool(phase343.RootElement, "stueckelbergVectorMassSourceAuditPassed") is true;
var stueckelbergRouteProvidesGaugeInvariantVectorMassMechanism = JsonBool(phase343.RootElement, "abelianRoutePreservesGaugeInvariance") is true
    && JsonBool(phase343.RootElement, "abelianRouteUsesCompensatorScalarOrFrame") is true;
var stueckelbergRoutePromotesWzMasses = JsonBool(phase343.RootElement, "stueckelbergRoutePromotesWzMasses") is true;
var stueckelbergRoutePromotesHiggsMass = JsonBool(phase343.RootElement, "stueckelbergRoutePromotesHiggsMass") is true;

var nielsenPoleMassGaugeIndependenceSourceAuditPassed = JsonBool(phase346.RootElement, "nielsenPoleMassGaugeIndependenceSourceAuditPassed") is true;
var poleMassRouteProvidesPhysicalMassExtractionBoundary = JsonBool(phase346.RootElement, "routeSupportsGaugeInvariantObservedPoleExtractionBoundary") is true;
var nielsenRoutePromotesWzMasses = JsonBool(phase346.RootElement, "nielsenRoutePromotesWzMasses") is true;
var nielsenRoutePromotesHiggsMass = JsonBool(phase346.RootElement, "nielsenRoutePromotesHiggsMass") is true;

var oxfordInhomogeneousGaugeEquationBridgeAuditPassed = JsonBool(phase368.RootElement, "oxfordInhomogeneousGaugeEquationBridgeAuditPassed") is true;
var phase368RouteProvidesBridgeEquationScaffold = JsonBool(phase368.RootElement, "routeProvidesBridgeEquationScaffold") is true;
var phase368RouteProvidesDirectTargetIndependentWzBridgeSourceLaw = JsonBool(phase368.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") is true;
var phase368RoutePromotesWzMasses = JsonBool(phase368.RootElement, "routePromotesWzMasses") is true;
var phase368RoutePromotesHiggsMass = JsonBool(phase368.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "ghilencea-2104-15118-abstract",
        ghilenceaSmWeylUrl,
        "abstract",
        "The paper embeds the Standard Model in Weyl conformal geometry, with Weyl quadratic gravity breaking gauged scale symmetry through a geometric Stueckelberg mechanism.",
        "Strong external geometric mass-generation lead; not GU-local source lineage."),
    new SourceRow(
        "ghilencea-2104-15118-z-mass-constraints",
        ghilenceaSmWeylEpjcUrl,
        "EPJ C section 2.7 / arXiv section on Z-mass constraints",
        "The source diagonalizes the neutral photon/Z/Weyl-boson mass matrix, keeps the photon massless, and records a Weyl-mixing correction to the Z mass.",
        "Relevant to W/Z bridge-source shape, but it constrains model parameters from observed Z data rather than deriving GU W/Z source rows."),
    new SourceRow(
        "ghilencea-hill-2303-02515-local-vs-gauged",
        ghilenceaHillLocalVsGaugedUrl,
        "abstract",
        "The follow-up compares local versus gauged Weyl scale symmetry and presents the gauged Weyl model as a UV completion with a massive Weyl gauge boson.",
        "Useful context for the Weyl route boundary; still external to GU and not a W/Z/H prediction contract filler."),
    new SourceRow(
        "phase330-weyl-geometric-mass-generation-boundary",
        "studies/phase330_weyl_geometric_mass_generation_source_audit_001/output/weyl_geometric_mass_generation_source_audit_summary.json",
        "local Phase330 output",
        "The newer Weyl x SU(2)L x U(1)Y route reproduces Standard Model mass-generation structure but leaves couplings, VEV, observed fields, and units external.",
        "Keeps this Phase369 route from being promoted merely because it is geometric."),
};

var checks = new[]
{
    new Check(
        "weyl-conformal-sm-stueckelberg-source-reviewed",
        weylConformalSmLeadPresent
            && weylConformalSmPrimarySourceReviewed
            && weylConformalSmRouteExternalToGu
            && sourceRows.Length == 4,
        $"lead={weylConformalSmLeadPresent}; reviewed={weylConformalSmPrimarySourceReviewed}; externalToGu={weylConformalSmRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "external-weyl-stueckelberg-mass-structure-recorded",
        routeUsesWeylConformalGeometry
            && routeUsesGaugedScaleSymmetryD1
            && routeUsesGeometricStueckelbergMechanism
            && routeGeneratesPlanckScaleFromStueckelbergField
            && routeInducesElectroweakSymmetryBreakingFromWeylGeometry
            && routeGeneratesHiggsPotentialFromGeometry
            && routeGivesHiggsWeylGaugeBosonCouplings
            && routeAllowsHyperchargeWeylKineticMixing
            && routeProvidesPhotonZAndWeylBosonNeutralMixingMatrix
            && routeKeepsPhotonMasslessByNeutralMassMatrixDeterminant
            && routeProvidesZMassCorrectionFromWeylMixing
            && routeConstrainsWeylParametersUsingObservedZMass
            && routeProvidesExternalGeometricMassGenerationTemplate,
        $"weylGeometry={routeUsesWeylConformalGeometry}; D1={routeUsesGaugedScaleSymmetryD1}; stueckelberg={routeUsesGeometricStueckelbergMechanism}; ewBreaking={routeInducesElectroweakSymmetryBreakingFromWeylGeometry}; higgsPotential={routeGeneratesHiggsPotentialFromGeometry}; kineticMixing={routeAllowsHyperchargeWeylKineticMixing}; neutralMatrix={routeProvidesPhotonZAndWeylBosonNeutralMixingMatrix}; photonMassless={routeKeepsPhotonMasslessByNeutralMassMatrixDeterminant}; zCorrection={routeProvidesZMassCorrectionFromWeylMixing}; zConstraint={routeConstrainsWeylParametersUsingObservedZMass}"),
    new Check(
        "standard-electroweak-dependency-boundary-remains-binding",
        electroweakParameterAuditPassed
            && !wAbsoluteMassParameterClosure
            && !zAbsoluteMassParameterClosure
            && !higgsMassParameterClosure
            && !weakCouplingSourcePromotable
            && electroweakMassMatrixBridgeSourceAuditPassed
            && smMassGenerationRequiresVev
            && smMassGenerationRequiresWeakCouplingG
            && smMassGenerationRequiresHyperchargeCouplingGPrime
            && smTreeLevelMwDependsOnGAndV
            && smTreeLevelMzDependsOnGAndGPrimeAndV
            && smTreeLevelHiggsMassDependsOnPotentialParameter
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass,
        $"p224={electroweakParameterAuditPassed}; wClosure={wAbsoluteMassParameterClosure}; zClosure={zAbsoluteMassParameterClosure}; higgsClosure={higgsMassParameterClosure}; weakCoupling={weakCouplingSourcePromotable}; p317={electroweakMassMatrixBridgeSourceAuditPassed}; requiresV={smMassGenerationRequiresVev}; requiresG={smMassGenerationRequiresWeakCouplingG}; requiresGPrime={smMassGenerationRequiresHyperchargeCouplingGPrime}; p317PromotesWz={smMassMatrixPromotesWzMasses}; p317PromotesHiggs={smMassMatrixPromotesHiggsMass}"),
    new Check(
        "adjacent-weyl-and-stueckelberg-boundaries-remain-nonpromotional",
        weylGeometricMassGenerationSourceAuditPassed
            && phase330WeylRouteExternalToGu
            && !phase330WeylRoutePromotesWzMasses
            && !phase330WeylRoutePromotesHiggsMass
            && stueckelbergVectorMassSourceAuditPassed
            && stueckelbergRouteProvidesGaugeInvariantVectorMassMechanism
            && !stueckelbergRoutePromotesWzMasses
            && !stueckelbergRoutePromotesHiggsMass,
        $"p330={weylGeometricMassGenerationSourceAuditPassed}; p330External={phase330WeylRouteExternalToGu}; p330PromotesWz={phase330WeylRoutePromotesWzMasses}; p330PromotesHiggs={phase330WeylRoutePromotesHiggsMass}; p343={stueckelbergVectorMassSourceAuditPassed}; p343Mechanism={stueckelbergRouteProvidesGaugeInvariantVectorMassMechanism}; p343PromotesWz={stueckelbergRoutePromotesWzMasses}; p343PromotesHiggs={stueckelbergRoutePromotesHiggsMass}"),
    new Check(
        "observed-mass-extraction-boundary-remains-required",
        nielsenPoleMassGaugeIndependenceSourceAuditPassed
            && poleMassRouteProvidesPhysicalMassExtractionBoundary
            && !nielsenRoutePromotesWzMasses
            && !nielsenRoutePromotesHiggsMass
            && !routeProvidesPoleMassExtraction,
        $"p346={nielsenPoleMassGaugeIndependenceSourceAuditPassed}; physicalBoundary={poleMassRouteProvidesPhysicalMassExtractionBoundary}; p346PromotesWz={nielsenRoutePromotesWzMasses}; p346PromotesHiggs={nielsenRoutePromotesHiggsMass}; routePoleExtraction={routeProvidesPoleMassExtraction}"),
    new Check(
        "gu-native-bridge-boundary-remains-unfilled",
        oxfordInhomogeneousGaugeEquationBridgeAuditPassed
            && phase368RouteProvidesBridgeEquationScaffold
            && !phase368RouteProvidesDirectTargetIndependentWzBridgeSourceLaw
            && !phase368RoutePromotesWzMasses
            && !phase368RoutePromotesHiggsMass
            && !routeProvidesGuLocalWeylEmbedding
            && !routeProvidesGuLocalWzTheorem
            && !routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
        $"p368={oxfordInhomogeneousGaugeEquationBridgeAuditPassed}; p368Scaffold={phase368RouteProvidesBridgeEquationScaffold}; p368DirectBridge={phase368RouteProvidesDirectTargetIndependentWzBridgeSourceLaw}; p368PromotesWz={phase368RoutePromotesWzMasses}; guWeylEmbedding={routeProvidesGuLocalWeylEmbedding}; guWzTheorem={routeProvidesGuLocalWzTheorem}; directBridge={routeProvidesDirectTargetIndependentWzBridgeSourceLaw}"),
    new Check(
        "weyl-conformal-route-does-not-fill-source-contracts",
        !routeProvidesSeparateWzSourceRows
            && !routeProvidesWzRawAmplitudeGates
            && !routeProvidesWzCommonBridgeGate
            && !routeProvidesWzTargetComparisonGate
            && !routeProvidesWzStabilitySidecars
            && !routeProvidesTargetIndependentGuVevSource
            && !routeProvidesWeakMixingAngleSource
            && !routeProvidesGaugeCouplingNormalization
            && !routeProvidesObservedPhotonWzHiggsProjectionRows
            && !routeProvidesGuObservedFieldExtraction
            && !routeProvidesHiggsScalarSourceOperator
            && !routeProvidesHiggsIdentityEnvelope
            && !routeProvidesHiggsQuarticOrExcitationSource
            && !routeProvidesTargetIndependentHiggsMass
            && !routeProvidesGeVUnitNormalization
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"wzRows={routeProvidesSeparateWzSourceRows}; raw={routeProvidesWzRawAmplitudeGates}; common={routeProvidesWzCommonBridgeGate}; target={routeProvidesWzTargetComparisonGate}; stability={routeProvidesWzStabilitySidecars}; vev={routeProvidesTargetIndependentGuVevSource}; weakAngle={routeProvidesWeakMixingAngleSource}; coupling={routeProvidesGaugeCouplingNormalization}; observedRows={routeProvidesObservedPhotonWzHiggsProjectionRows}; higgsSource={routeProvidesHiggsScalarSourceOperator}; higgsEnvelope={routeProvidesHiggsIdentityEnvelope}; higgsRelation={routeProvidesHiggsQuarticOrExcitationSource}; geV={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}"),
    new Check(
        "phase213-and-phase256-blocker-state-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14
            && observedFieldExtractionRequiredFieldCount == 20
            && observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable,
        $"p201Promotable={phase201AllRequiredLineagesPromotable}; existing={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}; observedRequired={observedFieldExtractionRequiredFieldCount}; observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}"),
};

var weylConformalSmStueckelbergSourceAuditPassed = weylConformalSmStueckelbergSourceAuditPassedExpected
    && checks.All(check => check.Passed);

var terminalStatus = weylConformalSmStueckelbergSourceAuditPassed
    ? "weyl-conformal-sm-stueckelberg-source-audit-external-template-not-source-law"
    : "weyl-conformal-sm-stueckelberg-source-audit-review-required";

var result = new
{
    phaseId = "phase369-weyl-conformal-sm-stueckelberg-source-audit",
    terminalStatus,
    weylConformalSmStueckelbergSourceAuditPassed,
    weylConformalSmLeadPresent,
    weylConformalSmPrimarySourceReviewed,
    weylConformalSmRouteExternalToGu,
    routeUsesWeylConformalGeometry,
    routeUsesGaugedScaleSymmetryD1,
    routeUsesGeometricStueckelbergMechanism,
    routeGeneratesPlanckScaleFromStueckelbergField,
    routeInducesElectroweakSymmetryBreakingFromWeylGeometry,
    routeGeneratesHiggsPotentialFromGeometry,
    routeGivesHiggsWeylGaugeBosonCouplings,
    routeAllowsHyperchargeWeylKineticMixing,
    routeProvidesPhotonZAndWeylBosonNeutralMixingMatrix,
    routeKeepsPhotonMasslessByNeutralMassMatrixDeterminant,
    routeProvidesZMassCorrectionFromWeylMixing,
    routeConstrainsWeylParametersUsingObservedZMass,
    routeProvidesExternalGeometricMassGenerationTemplate,
    routeProvidesGuLocalWeylEmbedding,
    routeProvidesGuLocalWzTheorem,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesSeparateWzSourceRows,
    routeProvidesWzRawAmplitudeGates,
    routeProvidesWzCommonBridgeGate,
    routeProvidesWzTargetComparisonGate,
    routeProvidesWzStabilitySidecars,
    routeProvidesTargetIndependentGuVevSource,
    routeProvidesWeakMixingAngleSource,
    routeProvidesGaugeCouplingNormalization,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesGuObservedFieldExtraction,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesHiggsIdentityEnvelope,
    routeProvidesHiggsQuarticOrExcitationSource,
    routeProvidesTargetIndependentHiggsMass,
    routeProvidesPoleMassExtraction,
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
        phase330 = new
        {
            weylGeometricMassGenerationSourceAuditPassed,
            phase330WeylRouteExternalToGu,
            phase330WeylRoutePromotesWzMasses,
            phase330WeylRoutePromotesHiggsMass,
        },
        phase343 = new
        {
            stueckelbergVectorMassSourceAuditPassed,
            stueckelbergRouteProvidesGaugeInvariantVectorMassMechanism,
            stueckelbergRoutePromotesWzMasses,
            stueckelbergRoutePromotesHiggsMass,
        },
        phase346 = new
        {
            nielsenPoleMassGaugeIndependenceSourceAuditPassed,
            poleMassRouteProvidesPhysicalMassExtractionBoundary,
            nielsenRoutePromotesWzMasses,
            nielsenRoutePromotesHiggsMass,
        },
        phase368 = new
        {
            oxfordInhomogeneousGaugeEquationBridgeAuditPassed,
            phase368RouteProvidesBridgeEquationScaffold,
            phase368RouteProvidesDirectTargetIndependentWzBridgeSourceLaw,
            phase368RoutePromotesWzMasses,
            phase368RoutePromotesHiggsMass,
        },
    },
    contractImpact = new
    {
        phase201AllRequiredLineagesPromotable,
        existingEvidenceFound,
        wzMissingFieldCount,
        higgsMissingFieldCount,
        observedFieldExtractionRequiredFieldCount,
        observedFieldExtractionFilledRequiredFieldCount,
        observedFieldExtractionContractPromotable,
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
    },
    checks,
    decision = "Do not promote W/Z or Higgs masses from the Weyl conformal Standard Model / geometric Stueckelberg route. It is a serious external geometric mass-generation template: it uses gauged Weyl geometry, a geometric Stueckelberg field, Higgs-Weyl couplings, neutral photon/Z/Weyl mixing, and a Weyl correction to the Z mass. It still constrains parameters using observed Z data and does not supply a GU-local Weyl embedding, direct target-independent W/Z source theorem, separate W/Z source rows, target-independent VEV, weak-angle or coupling normalization, observed photon/W/Z/H extraction, Higgs scalar-source/self-coupling lineage, pole extraction, or GeV normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-local map from the Oxford inhomogeneous/tilted gauge scaffold to a Weyl/Stueckelberg or equivalent electroweak mass matrix, if this route is to be imported at all.",
        "Separate derivation-backed W and Z source rows with raw/common/target/stability gates true before target comparison.",
        "A target-independent GU VEV or mass-scale source, weak-angle/coupling normalization, observed photon/W/Z/H projection, pole extraction, and GeV unit lineage.",
        "A GU-local Higgs scalar-source/operator and self-coupling or excitation relation independent of observed W/Z/H masses.",
    },
    sourceEvidence = new
    {
        ghilenceaSmWeylUrl,
        ghilenceaSmWeylPdfUrl,
        ghilenceaSmWeylEpjcUrl,
        ghilenceaHillLocalVsGaugedUrl,
        phase201Path = Phase201Path,
        phase213Path = Phase213Path,
        phase224Path = Phase224Path,
        phase256Path = Phase256Path,
        phase317Path = Phase317Path,
        phase330Path = Phase330Path,
        phase343Path = Phase343Path,
        phase346Path = Phase346Path,
        phase368Path = Phase368Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "weyl_conformal_sm_stueckelberg_source_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "weyl_conformal_sm_stueckelberg_source_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.weylConformalSmStueckelbergSourceAuditPassed,
        result.weylConformalSmLeadPresent,
        result.weylConformalSmPrimarySourceReviewed,
        result.weylConformalSmRouteExternalToGu,
        result.routeUsesWeylConformalGeometry,
        result.routeUsesGaugedScaleSymmetryD1,
        result.routeUsesGeometricStueckelbergMechanism,
        result.routeGeneratesPlanckScaleFromStueckelbergField,
        result.routeInducesElectroweakSymmetryBreakingFromWeylGeometry,
        result.routeGeneratesHiggsPotentialFromGeometry,
        result.routeGivesHiggsWeylGaugeBosonCouplings,
        result.routeAllowsHyperchargeWeylKineticMixing,
        result.routeProvidesPhotonZAndWeylBosonNeutralMixingMatrix,
        result.routeKeepsPhotonMasslessByNeutralMassMatrixDeterminant,
        result.routeProvidesZMassCorrectionFromWeylMixing,
        result.routeConstrainsWeylParametersUsingObservedZMass,
        result.routeProvidesExternalGeometricMassGenerationTemplate,
        result.routeProvidesGuLocalWeylEmbedding,
        result.routeProvidesGuLocalWzTheorem,
        result.routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
        result.routeProvidesSeparateWzSourceRows,
        result.routeProvidesWzRawAmplitudeGates,
        result.routeProvidesWzCommonBridgeGate,
        result.routeProvidesWzTargetComparisonGate,
        result.routeProvidesWzStabilitySidecars,
        result.routeProvidesTargetIndependentGuVevSource,
        result.routeProvidesWeakMixingAngleSource,
        result.routeProvidesGaugeCouplingNormalization,
        result.routeProvidesObservedPhotonWzHiggsProjectionRows,
        result.routeProvidesGuObservedFieldExtraction,
        result.routeProvidesHiggsScalarSourceOperator,
        result.routeProvidesHiggsIdentityEnvelope,
        result.routeProvidesHiggsQuarticOrExcitationSource,
        result.routeProvidesTargetIndependentHiggsMass,
        result.routeProvidesPoleMassExtraction,
        result.routeProvidesGeVUnitNormalization,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.sourceRowCount,
        result.sourceRows,
        result.adjacentRouteBoundary,
        result.contractImpact,
        result.checks,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"weylConformalSmStueckelbergSourceAuditPassed={weylConformalSmStueckelbergSourceAuditPassed}");
Console.WriteLine($"routeUsesGeometricStueckelbergMechanism={routeUsesGeometricStueckelbergMechanism}");
Console.WriteLine($"routeProvidesZMassCorrectionFromWeylMixing={routeProvidesZMassCorrectionFromWeylMixing}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record SourceRow(string SourceId, string Url, string Locator, string Finding, string PredictionImpact);
sealed record Check(string CheckId, bool Passed, string Detail);
