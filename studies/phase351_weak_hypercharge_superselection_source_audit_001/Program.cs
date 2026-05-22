using System.Text.Json;

const string DefaultOutputDir = "studies/phase351_weak_hypercharge_superselection_source_audit_001/output";
const string Phase148Path = "studies/phase148_all_known_boson_prediction_comparison_001/output/all_known_boson_prediction_comparison.json";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase235Path = "studies/phase235_pati_salam_weak_mixing_normalization_audit_001/output/pati_salam_weak_mixing_normalization_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase334Path = "studies/phase334_su21_superconnection_source_audit_001/output/su21_superconnection_source_audit_summary.json";
const string Phase337Path = "studies/phase337_octonion_clifford_internal_space_source_audit_001/output/octonion_clifford_internal_space_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE351_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase148 = JsonDocument.Parse(File.ReadAllText(Phase148Path));
using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase235 = JsonDocument.Parse(File.ReadAllText(Phase235Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase334 = JsonDocument.Parse(File.ReadAllText(Phase334Path));
using var phase337 = JsonDocument.Parse(File.ReadAllText(Phase337Path));

const string weakHyperchargeSuperselectionArxiv = "https://arxiv.org/abs/2010.15621";
const string weakHyperchargeSuperselectionJhepDoi = "https://doi.org/10.1007/JHEP04(2021)164";
const string weakHyperchargeSuperselectionIhesPdf = "https://preprints.ihes.fr/storage/_010_1_1_1_.pdf";

const bool weakHyperchargeSuperselectionAuditPassedExpected = true;
const bool weakHyperchargeSuperselectionLeadPresent = true;
const bool weakHyperchargeSuperselectionPrimarySourceReviewed = true;
const bool weakHyperchargeSuperselectionRouteExternalToGu = true;
const bool routeUsesZ2GradedCliffordTensorProduct = true;
const bool routeUsesCl4HatTensorCl6 = true;
const bool routeRestrictsToParticleSubspace = true;
const bool routePromotesWeakHyperchargeToSuperselectionRule = true;
const bool routeDefinesHiggsAsQuillenSuperconnectionScalar = true;
const bool routePlacesHiggsInCl4OddPart = true;
const bool routeExcludesSterileNeutrinoZeroHyperchargeSubspace = true;
const bool routeYieldsSl21Superalgebra = true;
const bool routeProjectsOutColourHiggsComponents = true;
const bool routePreservesGluonMasslessness = true;
const bool routeDerivesMasslessPhotonInUnitaryGauge = true;
const bool routeProvidesTheoreticalWeinbergAngleRelation = true;
const bool routeProvidesExternalHiggsWRelation = true;
const bool sourceClaimsWithinOnePercentAccuracy = true;
const bool routeProvidesObservedLowEnergyWeakMixingAngle = false;
const bool routeProvidesObservedZMass = false;
const bool routeProvidesObservedWMass = false;
const bool routeProvidesObservedHiggsMass = false;

const double theoreticalCosSquaredTheta = 5.0 / 8.0;
const double theoreticalTanSquaredTheta = 3.0 / 5.0;
const double theoreticalMhSquaredOverMwSquared = 5.0 / 2.0;
var theoreticalMhOverMw = Math.Sqrt(theoreticalMhSquaredOverMwSquared);
var observedWTargetGeV = FindComparisonDouble(phase148.RootElement, "physical-w-boson-mass-gev", "targetValue");
var observedWTargetUncertaintyGeV = FindComparisonDouble(phase148.RootElement, "physical-w-boson-mass-gev", "targetUncertainty");
var observedHiggsTargetGeV = FindComparisonDouble(phase148.RootElement, "physical-higgs-mass-gev", "targetValue");
var observedHiggsTargetUncertaintyGeV = FindComparisonDouble(phase148.RootElement, "physical-higgs-mass-gev", "targetUncertainty");
var diagnosticHiggsFromObservedWGeV = observedWTargetGeV is not null
    ? observedWTargetGeV.Value * theoreticalMhOverMw
    : (double?)null;
var diagnosticHiggsResidualGeV = diagnosticHiggsFromObservedWGeV is not null && observedHiggsTargetGeV is not null
    ? diagnosticHiggsFromObservedWGeV.Value - observedHiggsTargetGeV.Value
    : (double?)null;
var diagnosticHiggsCombinedUncertaintyGeV = observedWTargetUncertaintyGeV is not null && observedHiggsTargetUncertaintyGeV is not null
    ? Math.Sqrt(Math.Pow(observedWTargetUncertaintyGeV.Value * theoreticalMhOverMw, 2.0) + Math.Pow(observedHiggsTargetUncertaintyGeV.Value, 2.0))
    : (double?)null;
var diagnosticHiggsPull = diagnosticHiggsResidualGeV is not null && diagnosticHiggsCombinedUncertaintyGeV is > 0.0
    ? Math.Abs(diagnosticHiggsResidualGeV.Value) / diagnosticHiggsCombinedUncertaintyGeV.Value
    : (double?)null;
var observedMhOverMw = observedWTargetGeV is > 0.0 && observedHiggsTargetGeV is not null
    ? observedHiggsTargetGeV.Value / observedWTargetGeV.Value
    : (double?)null;
var currentTargetRelationRelativeResidual = observedMhOverMw is > 0.0
    ? Math.Abs(theoreticalMhOverMw - observedMhOverMw.Value) / observedMhOverMw.Value
    : (double?)null;
var currentTargetRelationWithinOnePercent = currentTargetRelationRelativeResidual is not null
    && currentTargetRelationRelativeResidual < 0.01;
var currentTargetRelationWithinTwoPercent = currentTargetRelationRelativeResidual is not null
    && currentTargetRelationRelativeResidual < 0.02;

const bool routeRequiresExternalCliffordModel = true;
const bool routeRequiresParticleSubspaceSelectionRule = true;
const bool routeRequiresSuperconnectionNormalizationTransfer = true;
const bool routeRequiresTheoreticalToLowEnergyWeakAngleTransport = true;
const bool routeRequiresGuLocalCliffordTensorMap = true;
const bool routeRequiresGuWeakHyperchargeSuperselectionDerivation = true;
const bool routeRequiresGuHiggsSuperconnectionScalarOperator = true;
const bool routeRequiresGuObservedPhotonWzHiggsProjection = true;
const bool routeRequiresGuIndependentWzSourceRows = true;
const bool routeRequiresTargetIndependentVevOrScale = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalCliffordTensorMap = false;
const bool routeProvidesGuWeakHyperchargeSuperselectionDerivation = false;
const bool routeProvidesGuLocalWzTheorem = false;
const bool routeProvidesSeparateObservedWzRows = false;
const bool routeProvidesTargetIndependentVevOrMassScale = false;
const bool routeProvidesGuLowEnergyWeakMixingAngleSource = false;
const bool routeProvidesGuGaugeCouplingNormalization = false;
const bool routeProvidesObservedPhotonWzHiggsProjectionRows = false;
const bool routeProvidesGuObservedFieldExtractionContract = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
const bool routeProvidesObservedHiggsMassFromGu = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesObservedFieldExtraction = false;
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
var patiSalamNormalizationPromotableForLowEnergyWz = JsonBool(phase235.RootElement, "patiSalamNormalizationPromotableForLowEnergyWz") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var su21RoutePromotesWzMasses = JsonBool(phase334.RootElement, "su21RoutePromotesWzMasses") is true;
var su21RoutePromotesHiggsMass = JsonBool(phase334.RootElement, "su21RoutePromotesHiggsMass") is true;
var octonionRoutePromotesWzMasses = JsonBool(phase337.RootElement, "routePromotesWzMasses") is true;
var octonionRoutePromotesHiggsMass = JsonBool(phase337.RootElement, "routePromotesHiggsMass") is true;

var sourceRows = new[]
{
    new SourceRow(
        "arxiv-2010.15621-weak-hypercharge-superselection",
        weakHyperchargeSuperselectionArxiv,
        "Weak-hypercharge superselection and Clifford algebra Standard Model source",
        "Restricts Cl4 hat-tensor Cl6 to a particle subspace, treats weak hypercharge as a superselection rule, defines the Higgs as the scalar part of Quillen's superconnection, and reports a W-Higgs mass relation from equal superconnection normalizations.",
        "Direct algebraic Higgs/W ratio lead; still external to GU and not an observed W/Z/H source-lineage artifact."),
    new SourceRow(
        "jhep04-2021-164-weak-hypercharge-superselection",
        weakHyperchargeSuperselectionJhepDoi,
        "JHEP version of the weak-hypercharge superselection paper",
        "Records the peer-reviewed version connected to the arXiv source and its Clifford/superconnection Higgs relation.",
        "Useful provenance; no GU-local field map, observed projection, absolute scale, or GeV normalization is supplied."),
    new SourceRow(
        "ihes-p-21-02-weak-hypercharge-superselection-pdf",
        weakHyperchargeSuperselectionIhesPdf,
        "IHES PDF with section-level W/Z/H relation details",
        "Displays the massless photon discussion, theoretical Weinberg-angle relation, and mH = 2 cos(thetaW) mW = sqrt(5/2) mW relation.",
        "Specific equation-level diagnostic source; still uses an external Clifford model and cannot by itself fill GU contracts.")
};

var checks = new[]
{
    new Check(
        "weak-hypercharge-superselection-primary-source-reviewed",
        weakHyperchargeSuperselectionLeadPresent
            && weakHyperchargeSuperselectionPrimarySourceReviewed
            && weakHyperchargeSuperselectionRouteExternalToGu
            && sourceRows.Length == 3,
        $"lead={weakHyperchargeSuperselectionLeadPresent}; reviewed={weakHyperchargeSuperselectionPrimarySourceReviewed}; externalToGu={weakHyperchargeSuperselectionRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "weak-hypercharge-clifford-superconnection-claims-captured",
        routeUsesZ2GradedCliffordTensorProduct
            && routeUsesCl4HatTensorCl6
            && routeRestrictsToParticleSubspace
            && routePromotesWeakHyperchargeToSuperselectionRule
            && routeDefinesHiggsAsQuillenSuperconnectionScalar
            && routePlacesHiggsInCl4OddPart
            && routeExcludesSterileNeutrinoZeroHyperchargeSubspace
            && routeYieldsSl21Superalgebra
            && routeProjectsOutColourHiggsComponents
            && routePreservesGluonMasslessness
            && routeDerivesMasslessPhotonInUnitaryGauge,
        $"z2Clifford={routeUsesZ2GradedCliffordTensorProduct}; cl4cl6={routeUsesCl4HatTensorCl6}; particleSubspace={routeRestrictsToParticleSubspace}; hyperchargeSuperselection={routePromotesWeakHyperchargeToSuperselectionRule}; higgsSuperconnection={routeDefinesHiggsAsQuillenSuperconnectionScalar}; sl21={routeYieldsSl21Superalgebra}; colourHiggsProjectedOut={routeProjectsOutColourHiggsComponents}; photonMassless={routeDerivesMasslessPhotonInUnitaryGauge}"),
    new Check(
        "external-higgs-w-relation-captured-but-diagnostic-only",
        routeProvidesTheoreticalWeinbergAngleRelation
            && routeProvidesExternalHiggsWRelation
            && sourceClaimsWithinOnePercentAccuracy
            && Math.Abs(theoreticalCosSquaredTheta - 0.625) < 1e-12
            && Math.Abs(theoreticalTanSquaredTheta - 0.6) < 1e-12
            && Math.Abs(theoreticalMhSquaredOverMwSquared - 2.5) < 1e-12
            && currentTargetRelationWithinTwoPercent
            && !routeProvidesObservedLowEnergyWeakMixingAngle
            && !routeProvidesObservedZMass
            && !routeProvidesObservedWMass
            && !routeProvidesObservedHiggsMass,
        $"cos2Theta={theoreticalCosSquaredTheta:R}; tan2Theta={theoreticalTanSquaredTheta:R}; mh2OverMw2={theoreticalMhSquaredOverMwSquared:R}; diagnosticHiggsFromObservedWGeV={diagnosticHiggsFromObservedWGeV:R}; residualGeV={diagnosticHiggsResidualGeV:R}; pull={diagnosticHiggsPull:R}; relativeResidual={currentTargetRelationRelativeResidual:R}; currentWithinOnePercent={currentTargetRelationWithinOnePercent}; currentWithinTwoPercent={currentTargetRelationWithinTwoPercent}; lowEnergyWeakAngle={routeProvidesObservedLowEnergyWeakMixingAngle}; observedZ={routeProvidesObservedZMass}; observedW={routeProvidesObservedWMass}; observedH={routeProvidesObservedHiggsMass}"),
    new Check(
        "adjacent-superconnection-and-hypercharge-blockers-still-bind",
        !patiSalamNormalizationPromotableForLowEnergyWz
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && !su21RoutePromotesWzMasses
            && !su21RoutePromotesHiggsMass
            && !octonionRoutePromotesWzMasses
            && !octonionRoutePromotesHiggsMass,
        $"p235LowEnergyPromotable={patiSalamNormalizationPromotableForLowEnergyWz}; smWz={smMassMatrixPromotesWzMasses}; smHiggs={smMassMatrixPromotesHiggsMass}; su21Wz={su21RoutePromotesWzMasses}; su21Higgs={su21RoutePromotesHiggsMass}; octonionWz={octonionRoutePromotesWzMasses}; octonionHiggs={octonionRoutePromotesHiggsMass}"),
    new Check(
        "weak-hypercharge-route-requires-missing-gu-source-data",
        routeRequiresExternalCliffordModel
            && routeRequiresParticleSubspaceSelectionRule
            && routeRequiresSuperconnectionNormalizationTransfer
            && routeRequiresTheoreticalToLowEnergyWeakAngleTransport
            && routeRequiresGuLocalCliffordTensorMap
            && routeRequiresGuWeakHyperchargeSuperselectionDerivation
            && routeRequiresGuHiggsSuperconnectionScalarOperator
            && routeRequiresGuObservedPhotonWzHiggsProjection
            && routeRequiresGuIndependentWzSourceRows
            && routeRequiresTargetIndependentVevOrScale
            && routeRequiresGeVUnitNormalization,
        $"externalClifford={routeRequiresExternalCliffordModel}; particleSubspace={routeRequiresParticleSubspaceSelectionRule}; superconnectionNorm={routeRequiresSuperconnectionNormalizationTransfer}; weakAngleTransport={routeRequiresTheoreticalToLowEnergyWeakAngleTransport}; guCliffordMap={routeRequiresGuLocalCliffordTensorMap}; guHypercharge={routeRequiresGuWeakHyperchargeSuperselectionDerivation}; guHiggsOperator={routeRequiresGuHiggsSuperconnectionScalarOperator}; observedProjection={routeRequiresGuObservedPhotonWzHiggsProjection}; wzRows={routeRequiresGuIndependentWzSourceRows}; scale={routeRequiresTargetIndependentVevOrScale}; gev={routeRequiresGeVUnitNormalization}"),
    new Check(
        "weak-hypercharge-route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalCliffordTensorMap
            && !routeProvidesGuWeakHyperchargeSuperselectionDerivation
            && !routeProvidesGuLocalWzTheorem
            && !routeProvidesSeparateObservedWzRows
            && !routeProvidesTargetIndependentVevOrMassScale
            && !routeProvidesGuLowEnergyWeakMixingAngleSource
            && !routeProvidesGuGaugeCouplingNormalization
            && !routeProvidesObservedPhotonWzHiggsProjectionRows
            && !routeProvidesGuObservedFieldExtractionContract
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesObservedHiggsMassFromGu
            && !routeProvidesGeVUnitNormalization
            && !routePromotesObservedFieldExtraction
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guCliffordMap={routeProvidesGuLocalCliffordTensorMap}; guHypercharge={routeProvidesGuWeakHyperchargeSuperselectionDerivation}; guWzTheorem={routeProvidesGuLocalWzTheorem}; observedWzRows={routeProvidesSeparateObservedWzRows}; scale={routeProvidesTargetIndependentVevOrMassScale}; weakAngle={routeProvidesGuLowEnergyWeakMixingAngleSource}; coupling={routeProvidesGuGaugeCouplingNormalization}; observedRows={routeProvidesObservedPhotonWzHiggsProjectionRows}; observedContract={routeProvidesGuObservedFieldExtractionContract}; higgsOperator={routeProvidesGuHiggsScalarSourceOperator}; higgsMass={routeProvidesObservedHiggsMassFromGu}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}"),
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

var weakHyperchargeSuperselectionSourceAuditPassed = checks.All(check => check.Passed)
    && weakHyperchargeSuperselectionAuditPassedExpected
    && !routePromotesObservedFieldExtraction
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = weakHyperchargeSuperselectionSourceAuditPassed
    ? "weak-hypercharge-superselection-source-audit-external-higgs-w-relation-not-gu-source"
    : "weak-hypercharge-superselection-source-audit-review-required";

var result = new
{
    phaseId = "phase351-weak-hypercharge-superselection-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    weakHyperchargeSuperselectionSourceAuditPassed,
    weakHyperchargeSuperselectionLeadPresent,
    weakHyperchargeSuperselectionPrimarySourceReviewed,
    weakHyperchargeSuperselectionRouteExternalToGu,
    routeUsesZ2GradedCliffordTensorProduct,
    routeUsesCl4HatTensorCl6,
    routeRestrictsToParticleSubspace,
    routePromotesWeakHyperchargeToSuperselectionRule,
    routeDefinesHiggsAsQuillenSuperconnectionScalar,
    routePlacesHiggsInCl4OddPart,
    routeExcludesSterileNeutrinoZeroHyperchargeSubspace,
    routeYieldsSl21Superalgebra,
    routeProjectsOutColourHiggsComponents,
    routePreservesGluonMasslessness,
    routeDerivesMasslessPhotonInUnitaryGauge,
    routeProvidesTheoreticalWeinbergAngleRelation,
    routeProvidesExternalHiggsWRelation,
    sourceClaimsWithinOnePercentAccuracy,
    routeProvidesObservedLowEnergyWeakMixingAngle,
    routeProvidesObservedZMass,
    routeProvidesObservedWMass,
    routeProvidesObservedHiggsMass,
    theoreticalCosSquaredTheta,
    theoreticalTanSquaredTheta,
    theoreticalMhSquaredOverMwSquared,
    theoreticalMhOverMw,
    diagnosticHiggsFromObservedWGeV,
    diagnosticHiggsResidualGeV,
    diagnosticHiggsPull,
    currentTargetRelationRelativeResidual,
    currentTargetRelationWithinOnePercent,
    currentTargetRelationWithinTwoPercent,
    routeRequiresExternalCliffordModel,
    routeRequiresParticleSubspaceSelectionRule,
    routeRequiresSuperconnectionNormalizationTransfer,
    routeRequiresTheoreticalToLowEnergyWeakAngleTransport,
    routeRequiresGuLocalCliffordTensorMap,
    routeRequiresGuWeakHyperchargeSuperselectionDerivation,
    routeRequiresGuHiggsSuperconnectionScalarOperator,
    routeRequiresGuObservedPhotonWzHiggsProjection,
    routeRequiresGuIndependentWzSourceRows,
    routeRequiresTargetIndependentVevOrScale,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalCliffordTensorMap,
    routeProvidesGuWeakHyperchargeSuperselectionDerivation,
    routeProvidesGuLocalWzTheorem,
    routeProvidesSeparateObservedWzRows,
    routeProvidesTargetIndependentVevOrMassScale,
    routeProvidesGuLowEnergyWeakMixingAngleSource,
    routeProvidesGuGaugeCouplingNormalization,
    routeProvidesObservedPhotonWzHiggsProjectionRows,
    routeProvidesGuObservedFieldExtractionContract,
    routeProvidesGuHiggsScalarSourceOperator,
    routeProvidesObservedHiggsMassFromGu,
    routeProvidesGeVUnitNormalization,
    routePromotesObservedFieldExtraction,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
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
    relatedBlockingEvidence = new
    {
        patiSalamNormalizationPromotableForLowEnergyWz,
        smMassMatrixPromotesWzMasses,
        smMassMatrixPromotesHiggsMass,
        su21RoutePromotesWzMasses,
        su21RoutePromotesHiggsMass,
        octonionRoutePromotesWzMasses,
        octonionRoutePromotesHiggsMass
    },
    decision = "Do not promote W/Z or Higgs physical masses from the weak-hypercharge superselection route. It gives a serious external Clifford/superconnection Higgs-to-W ratio lead, but it imports a separate particle-subspace construction and theoretical weak-angle normalization; it does not supply a GU-local Clifford tensor map, weak-hypercharge superselection derivation, observed photon/W/Z/H projection rows, independent W/Z source rows, target-independent VEV or scale, low-energy weak-angle transport, Higgs scalar-source operator, or GeV unit normalization.",
    nextRequiredArtifact = new[]
    {
        "A GU-native derivation mapping the draft's internal normal-spinor or gauge data into the Cl4 hat-tensor Cl6 particle-subspace construction.",
        "A proof that the weak-hypercharge superselection and superconnection normalization are GU-local source data rather than imported external algebraic assumptions.",
        "Observed photon/W/Z/H projection rows, including a low-energy weak-angle and Z-mass source if the theoretical angle is to be connected to physical W/Z.",
        "A target-independent W scale or VEV source and GeV unit normalization before using the Higgs/W relation as a mass prediction.",
        "A Higgs scalar-source/operator artifact satisfying the Phase201 and Phase256 contracts."
    }
};

var fullPath = Path.Combine(outputDir, "weak_hypercharge_superselection_source_audit.json");
var summaryPath = Path.Combine(outputDir, "weak_hypercharge_superselection_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.weakHyperchargeSuperselectionSourceAuditPassed,
    result.weakHyperchargeSuperselectionLeadPresent,
    result.weakHyperchargeSuperselectionPrimarySourceReviewed,
    result.weakHyperchargeSuperselectionRouteExternalToGu,
    result.routeUsesZ2GradedCliffordTensorProduct,
    result.routeUsesCl4HatTensorCl6,
    result.routeRestrictsToParticleSubspace,
    result.routePromotesWeakHyperchargeToSuperselectionRule,
    result.routeDefinesHiggsAsQuillenSuperconnectionScalar,
    result.routePlacesHiggsInCl4OddPart,
    result.routeExcludesSterileNeutrinoZeroHyperchargeSubspace,
    result.routeYieldsSl21Superalgebra,
    result.routeProjectsOutColourHiggsComponents,
    result.routePreservesGluonMasslessness,
    result.routeDerivesMasslessPhotonInUnitaryGauge,
    result.routeProvidesTheoreticalWeinbergAngleRelation,
    result.routeProvidesExternalHiggsWRelation,
    result.sourceClaimsWithinOnePercentAccuracy,
    result.routeProvidesObservedLowEnergyWeakMixingAngle,
    result.routeProvidesObservedZMass,
    result.routeProvidesObservedWMass,
    result.routeProvidesObservedHiggsMass,
    result.theoreticalCosSquaredTheta,
    result.theoreticalTanSquaredTheta,
    result.theoreticalMhSquaredOverMwSquared,
    result.theoreticalMhOverMw,
    result.diagnosticHiggsFromObservedWGeV,
    result.diagnosticHiggsResidualGeV,
    result.diagnosticHiggsPull,
    result.currentTargetRelationRelativeResidual,
    result.currentTargetRelationWithinOnePercent,
    result.currentTargetRelationWithinTwoPercent,
    result.routeRequiresExternalCliffordModel,
    result.routeRequiresParticleSubspaceSelectionRule,
    result.routeRequiresSuperconnectionNormalizationTransfer,
    result.routeRequiresTheoreticalToLowEnergyWeakAngleTransport,
    result.routeRequiresGuLocalCliffordTensorMap,
    result.routeRequiresGuWeakHyperchargeSuperselectionDerivation,
    result.routeRequiresGuHiggsSuperconnectionScalarOperator,
    result.routeRequiresGuObservedPhotonWzHiggsProjection,
    result.routeRequiresGuIndependentWzSourceRows,
    result.routeRequiresTargetIndependentVevOrScale,
    result.routeRequiresGeVUnitNormalization,
    result.routeProvidesGuLocalCliffordTensorMap,
    result.routeProvidesGuWeakHyperchargeSuperselectionDerivation,
    result.routeProvidesGuLocalWzTheorem,
    result.routeProvidesSeparateObservedWzRows,
    result.routeProvidesTargetIndependentVevOrMassScale,
    result.routeProvidesGuLowEnergyWeakMixingAngleSource,
    result.routeProvidesGuGaugeCouplingNormalization,
    result.routeProvidesObservedPhotonWzHiggsProjectionRows,
    result.routeProvidesGuObservedFieldExtractionContract,
    result.routeProvidesGuHiggsScalarSourceOperator,
    result.routeProvidesObservedHiggsMassFromGu,
    result.routeProvidesGeVUnitNormalization,
    result.routePromotesObservedFieldExtraction,
    result.routePromotesWzMasses,
    result.routePromotesHiggsMass,
    result.routeCompletesBosonPredictions,
    result.canFillPhase201WzContract,
    result.canFillPhase201HiggsContract,
    result.canFillPhase256ObservedFieldExtractionContract,
    sourceRowCount = sourceRows.Length,
    result.contractImpact,
    result.relatedBlockingEvidence,
    result.decision,
    result.nextRequiredArtifact
}, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"weakHyperchargeSuperselectionSourceAuditPassed={weakHyperchargeSuperselectionSourceAuditPassed}");
Console.WriteLine($"routeProvidesExternalHiggsWRelation={routeProvidesExternalHiggsWRelation}");
Console.WriteLine($"theoreticalMhSquaredOverMwSquared={theoreticalMhSquaredOverMwSquared:R}");
Console.WriteLine($"diagnosticHiggsFromObservedWGeV={diagnosticHiggsFromObservedWGeV:R}");
Console.WriteLine($"routePromotesWzMasses={routePromotesWzMasses}");
Console.WriteLine($"routePromotesHiggsMass={routePromotesHiggsMass}");
Console.WriteLine($"canFillPhase256ObservedFieldExtractionContract={canFillPhase256ObservedFieldExtractionContract}");

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
