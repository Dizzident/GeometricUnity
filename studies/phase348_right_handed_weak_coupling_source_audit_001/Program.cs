using System.Text.Json;

const string DefaultOutputDir = "studies/phase348_right_handed_weak_coupling_source_audit_001/output";
const string Phase201Path = "studies/phase201_boson_source_lineage_intake_contract_001/output/boson_source_lineage_intake_contract_summary.json";
const string Phase213Path = "studies/phase213_boson_source_lineage_blocker_matrix_001/output/boson_source_lineage_blocker_matrix_summary.json";
const string Phase256Path = "studies/phase256_observed_field_extraction_intake_contract_001/output/observed_field_extraction_intake_contract_summary.json";
const string Phase276Path = "studies/phase276_top_condensation_source_audit_001/output/top_condensation_source_audit_summary.json";
const string Phase317Path = "studies/phase317_electroweak_mass_matrix_bridge_source_audit_001/output/electroweak_mass_matrix_bridge_source_audit_summary.json";
const string Phase322Path = "studies/phase322_higgs_upsilon_scalar_source_boundary_audit_001/output/higgs_upsilon_scalar_source_boundary_audit_summary.json";
const string Phase346Path = "studies/phase346_nielsen_pole_mass_gauge_independence_source_audit_001/output/nielsen_pole_mass_gauge_independence_source_audit_summary.json";
const string Phase347Path = "studies/phase347_dispersive_electroweak_scale_mass_source_audit_001/output/dispersive_electroweak_scale_mass_source_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE348_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase201 = JsonDocument.Parse(File.ReadAllText(Phase201Path));
using var phase213 = JsonDocument.Parse(File.ReadAllText(Phase213Path));
using var phase256 = JsonDocument.Parse(File.ReadAllText(Phase256Path));
using var phase276 = JsonDocument.Parse(File.ReadAllText(Phase276Path));
using var phase317 = JsonDocument.Parse(File.ReadAllText(Phase317Path));
using var phase322 = JsonDocument.Parse(File.ReadAllText(Phase322Path));
using var phase346 = JsonDocument.Parse(File.ReadAllText(Phase346Path));
using var phase347 = JsonDocument.Parse(File.ReadAllText(Phase347Path));

const string xueNpbDoi = "https://doi.org/10.1016/j.nuclphysb.2022.115992";
const string xueWMassArxiv = "https://arxiv.org/abs/2205.14957";
const string xueVectorlikeWArxiv = "https://arxiv.org/abs/1506.05994";

const bool xueRightHandedWeakCouplingSourceAuditPassedExpected = true;
const bool xueWMassTensionLeadPresent = true;
const bool xueVectorlikeWLeadPresent = true;
const bool xuePrimarySourcesReviewed = true;
const bool xueRouteExternalToGu = true;
const bool smGaugeSymmetricFourFermionInteractionsUsed = true;
const bool compositeParticleUvFixedPointUsed = true;
const bool topCondensateIrfixedPointUsed = true;
const bool rightHandedWVertexInduced = true;
const bool rightHandedZVertexInduced = true;
const bool parityRestorationScaleTevClaimed = true;
const bool cdfWMassTensionMotivatedRoute = true;
const bool wMassCorrectionFormulaCaptured = true;
const bool zMassCorrectionFormulaCaptured = true;
const bool widthCorrectionFormulaCaptured = true;
const bool higgsMassUsedAsBoundaryInput = true;
const bool topMassUsedAsBoundaryInput = true;
const bool fermiConstantVevUsedAsBoundaryInput = true;
const double transitionScaleTev = 5.1;
const double electroweakVevInputGeV = 246.0;
const double topMassBoundaryInputGeV = 173.0;
const double higgsMassBoundaryInputGeV = 126.0;
const double cWLowerFit = 1.68;
const double cWUpperFit = 2.09;
const double cZUpperConstraint = 0.379;
const double zMassMeasurementInputGeV = 91.1876;
const double zMassMeasurementUncertaintyGeV = 0.0021;
const string wMassCorrectionFormula = "M_W^exp ~= M_W^SM * sqrt(1 + c_w^2 * (M_W^SM / Lambda)^2)";
const string zMassCorrectionFormula = "M_Z^exp ~= M_Z^SM * sqrt(1 + 0.5 * c_z^2 * (M_Z^SM / Lambda)^2)";

const bool routeRequiresExternalFourFermionOperator = true;
const bool routeRequiresCriticalCouplingAndGapEquation = true;
const bool routeRequiresRgTransport = true;
const bool routeRequiresMeasuredTopHiggsMasses = true;
const bool routeRequiresMeasuredFermiConstantVev = true;
const bool routeRequiresSmHighOrderMassBaseline = true;
const bool routeRequiresFittedOrConstrainedCwCz = true;
const bool routeRequiresObservedZConstraint = true;
const bool routeRequiresGuLocalFourFermionSource = true;
const bool routeRequiresGuCompositeFixedPointSource = true;
const bool routeRequiresGuTransitionScaleDerivation = true;
const bool routeRequiresGuRightHandedVertexTheorem = true;
const bool routeRequiresGuObservedFieldExtraction = true;
const bool routeRequiresGuIndependentWzSourceRows = true;
const bool routeRequiresGuHiggsScalarSourceOperator = true;
const bool routeRequiresGeVUnitNormalization = true;

const bool routeProvidesGuLocalFourFermionSource = false;
const bool routeProvidesGuLocalCompositeFixedPoint = false;
const bool routeProvidesGuLocalWzTheorem = false;
const bool routeProvidesTargetIndependentTransitionScale = false;
const bool routeProvidesTargetIndependentCwCz = false;
const bool routeProvidesSeparateObservedWzRows = false;
const bool routeProvidesIndependentHiggsMassPrediction = false;
const bool routeProvidesGuObservedFieldExtractionContract = false;
const bool routeProvidesGuHiggsScalarSourceOperator = false;
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
var topCondensationSourceAuditPassed = JsonBool(phase276.RootElement, "topCondensationSourceAuditPassed") is true;
var topCondensationPromotesWzMasses = JsonBool(phase276.RootElement, "topCondensationPromotesWzMasses") is true;
var topCondensationPromotesHiggsMass = JsonBool(phase276.RootElement, "topCondensationPromotesHiggsMass") is true;
var smMassMatrixPromotesWzMasses = JsonBool(phase317.RootElement, "smMassMatrixPromotesWzMasses") is true;
var smMassMatrixPromotesHiggsMass = JsonBool(phase317.RootElement, "smMassMatrixPromotesHiggsMass") is true;
var higgsUpsilonRouteCompletesBosonPredictions = JsonBool(phase322.RootElement, "higgsUpsilonRouteCompletesBosonPredictions") is true;
var nielsenRouteCompletesBosonPredictions = JsonBool(phase346.RootElement, "nielsenRouteCompletesBosonPredictions") is true;
var dispersiveRouteCompletesBosonPredictions = JsonBool(phase347.RootElement, "dispersiveRouteCompletesBosonPredictions") is true;

var sourceRows = new[]
{
    new SourceRow(
        "doi-10.1016-j.nuclphysb.2022.115992-right-handed-weak-coupling",
        xueNpbDoi,
        "Nuclear Physics B article on W-mass tension from right-handed W coupling",
        "Introduces SM-gauge-symmetric four-fermion interactions, a TeV-scale composite-particle/parity-restoration transition, and W/Z mass corrections from induced right-handed gauge couplings.",
        "Direct W/Z mass-correction lead, but it fits or constrains c_w and c_z against external measurements and uses SM boundary inputs rather than GU-local source rows."),
    new SourceRow(
        "arxiv-2205.14957-right-handed-weak-coupling",
        xueWMassArxiv,
        "Open arXiv record for the 2022 W-mass tension paper",
        "Records the claim that non-trivial right-handed W coupling at TeV energies can relieve the CDF W-mass tension and that W/Z propagators and decays are computed.",
        "Useful for source provenance and route boundaries; not a target-independent geometric prediction law."),
    new SourceRow(
        "arxiv-1506.05994-vectorlike-w-coupling",
        xueVectorlikeWArxiv,
        "Earlier vectorlike W-coupling and third-family mass source",
        "Studies SM-gauge-symmetric four-fermion operators where top condensation generates the top mass and W coupling becomes approximately vectorlike at TeV scale.",
        "Background for the 2022 route; overlaps top-condensation dynamics but adds no GU-local W/Z/H source lineage.")
};

var checks = new[]
{
    new Check(
        "xue-primary-sources-reviewed",
        xueWMassTensionLeadPresent
            && xueVectorlikeWLeadPresent
            && xuePrimarySourcesReviewed
            && xueRouteExternalToGu
            && sourceRows.Length == 3,
        $"wMassLead={xueWMassTensionLeadPresent}; vectorlikeLead={xueVectorlikeWLeadPresent}; reviewed={xuePrimarySourcesReviewed}; externalToGu={xueRouteExternalToGu}; sourceRows={sourceRows.Length}"),
    new Check(
        "right-handed-wz-mass-correction-claims-captured",
        smGaugeSymmetricFourFermionInteractionsUsed
            && compositeParticleUvFixedPointUsed
            && topCondensateIrfixedPointUsed
            && rightHandedWVertexInduced
            && rightHandedZVertexInduced
            && parityRestorationScaleTevClaimed
            && cdfWMassTensionMotivatedRoute
            && wMassCorrectionFormulaCaptured
            && zMassCorrectionFormulaCaptured
            && widthCorrectionFormulaCaptured
            && transitionScaleTev == 5.1
            && cWLowerFit == 1.68
            && cWUpperFit == 2.09
            && cZUpperConstraint == 0.379,
        $"fourFermion={smGaugeSymmetricFourFermionInteractionsUsed}; uvComposite={compositeParticleUvFixedPointUsed}; rightW={rightHandedWVertexInduced}; rightZ={rightHandedZVertexInduced}; lambdaTev={transitionScaleTev}; cW=[{cWLowerFit},{cWUpperFit}]; cZUpper={cZUpperConstraint}"),
    new Check(
        "route-uses-external-boundary-inputs",
        higgsMassUsedAsBoundaryInput
            && topMassUsedAsBoundaryInput
            && fermiConstantVevUsedAsBoundaryInput
            && routeRequiresMeasuredTopHiggsMasses
            && routeRequiresMeasuredFermiConstantVev
            && routeRequiresSmHighOrderMassBaseline
            && routeRequiresFittedOrConstrainedCwCz
            && routeRequiresObservedZConstraint,
        $"higgsBoundary={higgsMassUsedAsBoundaryInput}; topBoundary={topMassUsedAsBoundaryInput}; vevBoundary={fermiConstantVevUsedAsBoundaryInput}; smBaseline={routeRequiresSmHighOrderMassBaseline}; fittedCwCz={routeRequiresFittedOrConstrainedCwCz}; zConstraint={routeRequiresObservedZConstraint}"),
    new Check(
        "current-source-lineage-blockers-preserved",
        !phase201AllRequiredLineagesPromotable
            && !existingEvidenceFound
            && wzMissingFieldCount == 15
            && higgsMissingFieldCount == 14,
        $"phase201Promotable={phase201AllRequiredLineagesPromotable}; existingEvidence={existingEvidenceFound}; wzMissing={wzMissingFieldCount}; higgsMissing={higgsMissingFieldCount}"),
    new Check(
        "observed-field-contract-and-adjacent-routes-remain-nonpromotional",
        observedFieldExtractionFilledRequiredFieldCount == 0
            && !observedFieldExtractionContractPromotable
            && topCondensationSourceAuditPassed
            && !topCondensationPromotesWzMasses
            && !topCondensationPromotesHiggsMass
            && !smMassMatrixPromotesWzMasses
            && !smMassMatrixPromotesHiggsMass
            && !higgsUpsilonRouteCompletesBosonPredictions
            && !nielsenRouteCompletesBosonPredictions
            && !dispersiveRouteCompletesBosonPredictions,
        $"observedFilled={observedFieldExtractionFilledRequiredFieldCount}; observedPromotable={observedFieldExtractionContractPromotable}; p276={topCondensationSourceAuditPassed}; topCondWz={topCondensationPromotesWzMasses}; smWz={smMassMatrixPromotesWzMasses}; higgsUpsilonCompletes={higgsUpsilonRouteCompletesBosonPredictions}; nielsenCompletes={nielsenRouteCompletesBosonPredictions}; dispersiveCompletes={dispersiveRouteCompletesBosonPredictions}"),
    new Check(
        "xue-route-requires-missing-gu-source-data",
        routeRequiresExternalFourFermionOperator
            && routeRequiresCriticalCouplingAndGapEquation
            && routeRequiresRgTransport
            && routeRequiresGuLocalFourFermionSource
            && routeRequiresGuCompositeFixedPointSource
            && routeRequiresGuTransitionScaleDerivation
            && routeRequiresGuRightHandedVertexTheorem
            && routeRequiresGuObservedFieldExtraction
            && routeRequiresGuIndependentWzSourceRows
            && routeRequiresGuHiggsScalarSourceOperator
            && routeRequiresGeVUnitNormalization,
        $"externalFourFermion={routeRequiresExternalFourFermionOperator}; gap={routeRequiresCriticalCouplingAndGapEquation}; rg={routeRequiresRgTransport}; guFourFermion={routeRequiresGuLocalFourFermionSource}; guFixedPoint={routeRequiresGuCompositeFixedPointSource}; guTransitionScale={routeRequiresGuTransitionScaleDerivation}; guVertex={routeRequiresGuRightHandedVertexTheorem}; observed={routeRequiresGuObservedFieldExtraction}; wzRows={routeRequiresGuIndependentWzSourceRows}; scalar={routeRequiresGuHiggsScalarSourceOperator}; gev={routeRequiresGeVUnitNormalization}"),
    new Check(
        "xue-route-does-not-fill-gu-contracts",
        !routeProvidesGuLocalFourFermionSource
            && !routeProvidesGuLocalCompositeFixedPoint
            && !routeProvidesGuLocalWzTheorem
            && !routeProvidesTargetIndependentTransitionScale
            && !routeProvidesTargetIndependentCwCz
            && !routeProvidesSeparateObservedWzRows
            && !routeProvidesIndependentHiggsMassPrediction
            && !routeProvidesGuObservedFieldExtractionContract
            && !routeProvidesGuHiggsScalarSourceOperator
            && !routeProvidesGeVUnitNormalization
            && !routePromotesObservedFieldExtraction
            && !routePromotesWzMasses
            && !routePromotesHiggsMass
            && !routeCompletesBosonPredictions
            && !canFillPhase201WzContract
            && !canFillPhase201HiggsContract
            && !canFillPhase256ObservedFieldExtractionContract,
        $"guFourFermion={routeProvidesGuLocalFourFermionSource}; guFixedPoint={routeProvidesGuLocalCompositeFixedPoint}; guWzTheorem={routeProvidesGuLocalWzTheorem}; targetIndependentScale={routeProvidesTargetIndependentTransitionScale}; targetIndependentCwCz={routeProvidesTargetIndependentCwCz}; observedWzRows={routeProvidesSeparateObservedWzRows}; higgsPrediction={routeProvidesIndependentHiggsMassPrediction}; observedContract={routeProvidesGuObservedFieldExtractionContract}; scalarSource={routeProvidesGuHiggsScalarSourceOperator}; gev={routeProvidesGeVUnitNormalization}; promotesWz={routePromotesWzMasses}; promotesHiggs={routePromotesHiggsMass}; completes={routeCompletesBosonPredictions}")
};

var xueRightHandedWeakCouplingSourceAuditPassed = checks.All(check => check.Passed)
    && xueRightHandedWeakCouplingSourceAuditPassedExpected
    && !routePromotesObservedFieldExtraction
    && !routePromotesWzMasses
    && !routePromotesHiggsMass
    && !routeCompletesBosonPredictions
    && !canFillPhase201WzContract
    && !canFillPhase201HiggsContract
    && !canFillPhase256ObservedFieldExtractionContract;

var terminalStatus = xueRightHandedWeakCouplingSourceAuditPassed
    ? "right-handed-weak-coupling-source-audit-external-four-fermion-fit-not-gu-source"
    : "right-handed-weak-coupling-source-audit-review-required";

var result = new
{
    phaseId = "phase348-right-handed-weak-coupling-source-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    xueRightHandedWeakCouplingSourceAuditPassed,
    xueWMassTensionLeadPresent,
    xueVectorlikeWLeadPresent,
    xuePrimarySourcesReviewed,
    xueRouteExternalToGu,
    smGaugeSymmetricFourFermionInteractionsUsed,
    compositeParticleUvFixedPointUsed,
    topCondensateIrfixedPointUsed,
    rightHandedWVertexInduced,
    rightHandedZVertexInduced,
    parityRestorationScaleTevClaimed,
    cdfWMassTensionMotivatedRoute,
    wMassCorrectionFormulaCaptured,
    zMassCorrectionFormulaCaptured,
    widthCorrectionFormulaCaptured,
    higgsMassUsedAsBoundaryInput,
    topMassUsedAsBoundaryInput,
    fermiConstantVevUsedAsBoundaryInput,
    transitionScaleTev,
    electroweakVevInputGeV,
    topMassBoundaryInputGeV,
    higgsMassBoundaryInputGeV,
    cWLowerFit,
    cWUpperFit,
    cZUpperConstraint,
    zMassMeasurementInputGeV,
    zMassMeasurementUncertaintyGeV,
    wMassCorrectionFormula,
    zMassCorrectionFormula,
    routeRequiresExternalFourFermionOperator,
    routeRequiresCriticalCouplingAndGapEquation,
    routeRequiresRgTransport,
    routeRequiresMeasuredTopHiggsMasses,
    routeRequiresMeasuredFermiConstantVev,
    routeRequiresSmHighOrderMassBaseline,
    routeRequiresFittedOrConstrainedCwCz,
    routeRequiresObservedZConstraint,
    routeRequiresGuLocalFourFermionSource,
    routeRequiresGuCompositeFixedPointSource,
    routeRequiresGuTransitionScaleDerivation,
    routeRequiresGuRightHandedVertexTheorem,
    routeRequiresGuObservedFieldExtraction,
    routeRequiresGuIndependentWzSourceRows,
    routeRequiresGuHiggsScalarSourceOperator,
    routeRequiresGeVUnitNormalization,
    routeProvidesGuLocalFourFermionSource,
    routeProvidesGuLocalCompositeFixedPoint,
    routeProvidesGuLocalWzTheorem,
    routeProvidesTargetIndependentTransitionScale,
    routeProvidesTargetIndependentCwCz,
    routeProvidesSeparateObservedWzRows,
    routeProvidesIndependentHiggsMassPrediction,
    routeProvidesGuObservedFieldExtractionContract,
    routeProvidesGuHiggsScalarSourceOperator,
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
        topCondensationSourceAuditPassed,
        topCondensationPromotesWzMasses,
        topCondensationPromotesHiggsMass,
        smMassMatrixPromotesWzMasses,
        smMassMatrixPromotesHiggsMass,
        higgsUpsilonRouteCompletesBosonPredictions,
        nielsenRouteCompletesBosonPredictions,
        dispersiveRouteCompletesBosonPredictions
    },
    decision = "Do not promote W/Z or Higgs physical masses from the Xue right-handed weak-coupling route. It is a direct W/Z mass-correction lead, but it imports SM-gauge-symmetric four-fermion dynamics, measured top and Higgs masses, the Fermi-constant VEV, SM high-order mass baselines, and fitted or externally constrained c_w/c_z coefficients; it supplies no GU-local four-fermion source, transition-scale derivation, right-handed-vertex theorem, observed-field extraction, Higgs scalar-source lineage, or GeV-unit derivation.",
    nextRequiredArtifact = new[]
    {
        "A GU-local source for the relevant four-fermion operator or a replacement geometric interaction that preserves the required gauge structure.",
        "A target-independent derivation of the parity-restoration or composite transition scale, not a scale inferred from measured top/Higgs inputs.",
        "A GU theorem deriving the W and Z right-handed vertex functions and their coefficients c_w and c_z without fitting W or Z target data.",
        "Observed photon/W/Z/H projection rows and complex-pole extraction before physical mass comparison.",
        "A GU Higgs scalar-source/operator and GeV normalization that do not import the measured Higgs mass as a boundary condition."
    }
};

var fullPath = Path.Combine(outputDir, "right_handed_weak_coupling_source_audit.json");
var summaryPath = Path.Combine(outputDir, "right_handed_weak_coupling_source_audit_summary.json");
var jsonOptions = new JsonSerializerOptions { WriteIndented = true };

File.WriteAllText(fullPath, JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.xueRightHandedWeakCouplingSourceAuditPassed,
    result.xueWMassTensionLeadPresent,
    result.xueVectorlikeWLeadPresent,
    result.xuePrimarySourcesReviewed,
    result.xueRouteExternalToGu,
    result.smGaugeSymmetricFourFermionInteractionsUsed,
    result.compositeParticleUvFixedPointUsed,
    result.topCondensateIrfixedPointUsed,
    result.rightHandedWVertexInduced,
    result.rightHandedZVertexInduced,
    result.parityRestorationScaleTevClaimed,
    result.cdfWMassTensionMotivatedRoute,
    result.wMassCorrectionFormulaCaptured,
    result.zMassCorrectionFormulaCaptured,
    result.widthCorrectionFormulaCaptured,
    result.higgsMassUsedAsBoundaryInput,
    result.topMassUsedAsBoundaryInput,
    result.fermiConstantVevUsedAsBoundaryInput,
    result.transitionScaleTev,
    result.electroweakVevInputGeV,
    result.topMassBoundaryInputGeV,
    result.higgsMassBoundaryInputGeV,
    result.cWLowerFit,
    result.cWUpperFit,
    result.cZUpperConstraint,
    result.zMassMeasurementInputGeV,
    result.zMassMeasurementUncertaintyGeV,
    result.wMassCorrectionFormula,
    result.zMassCorrectionFormula,
    result.routeRequiresExternalFourFermionOperator,
    result.routeRequiresCriticalCouplingAndGapEquation,
    result.routeRequiresRgTransport,
    result.routeRequiresMeasuredTopHiggsMasses,
    result.routeRequiresMeasuredFermiConstantVev,
    result.routeRequiresSmHighOrderMassBaseline,
    result.routeRequiresFittedOrConstrainedCwCz,
    result.routeRequiresObservedZConstraint,
    result.routeRequiresGuLocalFourFermionSource,
    result.routeRequiresGuCompositeFixedPointSource,
    result.routeRequiresGuTransitionScaleDerivation,
    result.routeRequiresGuRightHandedVertexTheorem,
    result.routeRequiresGuObservedFieldExtraction,
    result.routeRequiresGuIndependentWzSourceRows,
    result.routeRequiresGuHiggsScalarSourceOperator,
    result.routeRequiresGeVUnitNormalization,
    result.routeProvidesGuLocalFourFermionSource,
    result.routeProvidesGuLocalCompositeFixedPoint,
    result.routeProvidesGuLocalWzTheorem,
    result.routeProvidesTargetIndependentTransitionScale,
    result.routeProvidesTargetIndependentCwCz,
    result.routeProvidesSeparateObservedWzRows,
    result.routeProvidesIndependentHiggsMassPrediction,
    result.routeProvidesGuObservedFieldExtractionContract,
    result.routeProvidesGuHiggsScalarSourceOperator,
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
Console.WriteLine($"xueRightHandedWeakCouplingSourceAuditPassed={xueRightHandedWeakCouplingSourceAuditPassed}");
Console.WriteLine($"rightHandedWVertexInduced={rightHandedWVertexInduced}");
Console.WriteLine($"transitionScaleTev={transitionScaleTev}");
Console.WriteLine($"cWFitRange=[{cWLowerFit},{cWUpperFit}]");
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

public sealed record SourceRow(string SourceId, string Url, string FindingKind, string Summary, string PredictionImpact);

public sealed record Check(string CheckId, bool Passed, string Detail);
