using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase415: fermionic cohomology / square-root delta_omega ansatz probe.
//
// Superphysics part-09b section 9.3 is treated as a research clue only. It
// names chi, omega subfields, Upsilon_omega, cohomology obstruction language,
// and a possible first-order square root delta_omega. It does not define the
// differential, carrier complex, cohomology target, square law, adjoint, scalar
// doublet projection, observed-field rows, weak-angle lineage, or scale law.
//
// This phase turns that gap into a machine-readable admissibility result:
// locally specified first-order-complex candidates are closed by existing
// phase evidence, while the remaining branches are recorded as requiring a
// new source-level delta_omega specification.
//
// Fail-closed: no targets, no fitted couplings, no source-contract mutation,
// no W/Z/H promotion.

const string DefaultOutputDir = "studies/phase415_fermionic_cohomology_square_root_ansatz_probe_001/output";
const string Phase370SummaryPath = "studies/phase370_completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_001/output/completion_fermionic_yukawa_higgs_mixed_linearization_source_audit_summary.json";
const string Phase389SummaryPath = "studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001/output/vo7_mixed_linearization_gauge_compatibility_identity_probe_summary.json";
const string Phase397SummaryPath = "studies/phase397_parametrized_u1_extension_neutral_mixing_underdetermination_probe_001/output/parametrized_u1_extension_neutral_mixing_underdetermination_probe_summary.json";
const string Phase401SummaryPath = "studies/phase401_full_quartic_action_coupled_critical_point_construction_001/output/full_quartic_action_coupled_critical_point_construction_summary.json";
const string Phase411SummaryPath = "studies/phase411_quartic_dirac_squared_spinor_composite_probe_001/output/quartic_dirac_squared_spinor_composite_probe_summary.json";
const string Phase414SummaryPath = "studies/phase414_general_shiab_epsilon_operator_ansatz_probe_001/output/general_shiab_epsilon_operator_ansatz_probe_summary.json";
const string ApplicationSubjectKind = "fermionic-cohomology-square-root-ansatz-probe";

var outputDir = Environment.GetEnvironmentVariable("PHASE415_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase370 = JsonDocument.Parse(File.ReadAllText(Phase370SummaryPath));
using var phase389 = JsonDocument.Parse(File.ReadAllText(Phase389SummaryPath));
using var phase397 = JsonDocument.Parse(File.ReadAllText(Phase397SummaryPath));
using var phase401 = JsonDocument.Parse(File.ReadAllText(Phase401SummaryPath));
using var phase411 = JsonDocument.Parse(File.ReadAllText(Phase411SummaryPath));
using var phase414 = JsonDocument.Parse(File.ReadAllText(Phase414SummaryPath));

bool phase370PrecursorPassed =
    JsonBool(phase370.RootElement, "completionFermionicYukawaHiggsMixedLinearizationSourceAuditPassed") is true &&
    JsonBool(phase370.RootElement, "officialDraftFermionicSectorArchitecturePresent") is true &&
    JsonBool(phase370.RootElement, "completionTypedPlaceholderEvidencePresent") is true &&
    JsonBool(phase370.RootElement, "routeProvidesCompletedFermionicAction") is false &&
    JsonBool(phase370.RootElement, "routeProvidesFixedFermionicOperatorBranch") is false &&
    JsonBool(phase370.RootElement, "routeProvidesExplicitYukawaFunctional") is false &&
    JsonBool(phase370.RootElement, "routeProvidesCompletedMixedLinearizationBlocks") is false &&
    JsonBool(phase370.RootElement, "routeProvidesHiggsScalarSourceOperator") is false;

bool phase389PrecursorPassed =
    JsonBool(phase389.RootElement, "vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed") is true &&
    JsonBool(phase389.RootElement, "discreteGaugeCompatibilityIdentityExact") is true &&
    JsonBool(phase389.RootElement, "targetBlindConstruction") is true &&
    JsonBool(phase389.RootElement, "routeProvidesPhysicalMassPsiCompatibleBranch") is false &&
    JsonBool(phase389.RootElement, "routeProvidesCompletedFermionicAction") is false &&
    JsonBool(phase389.RootElement, "routeProvidesPhysicalEffectiveActionHessian") is false;

bool phase397PrecursorPassed =
    JsonBool(phase397.RootElement, "parametrizedU1ExtensionNeutralMixingUnderdeterminationProbePassed") is true &&
    JsonBool(phase397.RootElement, "neutralMixingElementVanishesInFermionBilinearChannel") is true &&
    JsonBool(phase397.RootElement, "photonZSeparationUnderdetermined") is true &&
    JsonBool(phase397.RootElement, "hyperchargeAssignmentsDerived") is false &&
    JsonBool(phase397.RootElement, "couplingRatioDerived") is false &&
    JsonBool(phase397.RootElement, "weakMixingAngleSelected") is false;

bool phase401PrecursorPassed =
    JsonBool(phase401.RootElement, "fullQuarticActionCoupledCriticalPointConstructionPassed") is true &&
    JsonBool(phase401.RootElement, "noPerturbativeCoupledCriticalPointFound") is true &&
    JsonBool(phase401.RootElement, "kernelRelaxationNonperturbative") is true &&
    JsonBool(phase401.RootElement, "physicalBosonicActionUsed") is false &&
    JsonBool(phase401.RootElement, "physicalCouplingProvided") is false;

bool phase411PrecursorPassed =
    JsonBool(phase411.RootElement, "quarticDiracSquaredSpinorCompositeProbePassed") is true &&
    JsonBool(phase411.RootElement, "leftRightBilinearChannelHasNoWeldedScalar") is true &&
    JsonBool(phase411.RootElement, "spinorBilinearSpinZeroDoubletAbsent") is true &&
    JsonInt(phase411.RootElement, "majoranaSpinZeroSmDoubletCount") == 0;

bool phase414PrecursorPassed =
    JsonBool(phase414.RootElement, "generalShiabEpsilonOperatorAnsatzProbePassed") is true &&
    JsonBool(phase414.RootElement, "fermionicCohomologySquareRootStillRequiresSpecification") is true &&
    JsonBool(phase414.RootElement, "anyClosedLowOrderAnsatzProducesWeldedScalarSmDoublet") is false &&
    JsonBool(phase414.RootElement, "canFillPhase201WzContract") is false;

var part09bClue = new
{
    source = "https://www.superphysics.org/research/weinstein/unity/part-09b/#93-the-fermionic-sector",
    sourceRole = "secondary readable mirror / research clue, not promotion evidence",
    reviewedByLocalLedger = "docs/Reference/ExperimentReferences/SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md",
    chiContentNamed = true,
    omegaSubfieldsNamed = true,
    upsilonOmegaCohomologyObstructionQuestionPresent = true,
    squareRootDeltaOmegaQuestionPresent = true,
    sourceProvidesExplicitDeltaOmegaDifferential = false,
    sourceProvidesCarrierComplex = false,
    sourceProvidesCohomologyTarget = false,
    sourceProvidesNilpotenceOrSquareLaw = false,
    sourceProvidesAdjointOrInnerProductConvention = false,
    sourceProvidesScalarDoubletProjection = false,
    sourceProvidesObservedPhotonWzHiggsRows = false,
    sourceProvidesWeakAngleScalePoleOrGevLineage = false,
};

var requiredSpecificationFields = new[]
{
    new RequiredSpecField("delta-omega-differential", "Explicit first-order operator delta_omega, including principal symbol and lower-order terms.", false),
    new RequiredSpecField("carrier-complex", "Domain/codomain carrier sequence for chi, omega, and Upsilon_omega.", false),
    new RequiredSpecField("cohomology-target", "The cohomology group or obstruction class whose vanishing/non-vanishing is tested.", false),
    new RequiredSpecField("square-law", "A source-defined equation such as delta_omega^2 = wedge(Upsilon_omega) plus curvature/obstruction terms.", false),
    new RequiredSpecField("adjoint-inner-product", "Adjoint and inner-product convention connecting the complex to the action.", false),
    new RequiredSpecField("scalar-doublet-projection", "A target-independent projection from the complex to a welded spin-0 SM doublet.", false),
    new RequiredSpecField("observed-boson-rows", "Photon/W/Z/H observed-field extraction rows or namespace map.", false),
    new RequiredSpecField("normalization-lineage", "Weak-angle, VEV/source-scale, pole extraction, and GeV/unit normalization lineage.", false),
};

var candidateComplexes = new[]
{
    new CandidateComplex(
        "de-rham-upsilon-wedge-complex",
        "omega-form complex with zero-order Upsilon_omega wedge map",
        "source clue only",
        false,
        false,
        false,
        false,
        false,
        "open",
        "The page suggests a cohomology obstruction but does not define the first-order square root, grading, or square law."),
    new CandidateComplex(
        "dirac-pair-chiral-square-root",
        "chiral Dirac-pair route S_L -> S_R with a Dirac-squared/Yukawa reading",
        "phase411",
        true,
        true,
        false,
        false,
        false,
        "closed",
        "Phase411 closes the natural bilinear Yukawa channel: the left-right Dirac mass channel contains no welded scalar, and the Majorana welded scalars carry no SM doublet."),
    new CandidateComplex(
        "vo7-gauge-compatibility-control-complex",
        "candidate mixed block with exact discrete gauge-compatibility identity",
        "phase389",
        true,
        true,
        false,
        false,
        false,
        "closed",
        "Phase389 materializes an exact control-branch identity, but no physical M_psi branch, completed fermionic action, or physical effective Hessian is supplied."),
    new CandidateComplex(
        "fermion-induced-source-backreaction-complex",
        "mixed-Hessian/backreaction route to coupled stationarity",
        "phase397/phase401",
        true,
        true,
        false,
        false,
        false,
        "closed",
        "Phase397 shows neutral mixing vanishes in the fermion-bilinear channel, and Phase401 characterizes the coupled critical-point relaxation as non-perturbative on the toy action."),
    new CandidateComplex(
        "superconnection-lower-right-quadrant-complex",
        "superconnection-style completion with a nontrivial lower-right quadrant",
        "none",
        false,
        false,
        false,
        false,
        false,
        "open",
        "No source-defined lower-right map, grading, or square law exists in the local/public material reviewed."),
    new CandidateComplex(
        "unobserved-phase-augmented-complex",
        "delta_omega complex on draft unobserved-phase fields",
        "none",
        false,
        false,
        false,
        false,
        false,
        "open",
        "No computable unobserved-phase representation, action, or coupling has been pinned to source text."),
};

int localCandidateComplexCount = candidateComplexes.Length;
int locallySpecifiedCandidateCount = candidateComplexes.Count(c => c.LocallySpecified);
int closedLocalCandidateCount = candidateComplexes.Count(c => c.Status == "closed");
int openSpecificationCandidateCount = candidateComplexes.Count(c => c.Status == "open");
int candidateComplexesWithWeldedScalarSmDoubletCount = candidateComplexes.Count(c => c.ProducesWeldedScalarSmDoublet);
int candidateComplexesWithObservedProjectionDataCount = candidateComplexes.Count(c => c.ProducesObservedProjectionData);

bool sourceProvidesExplicitDeltaOmega = false;
bool sourceProvidesFirstOrderComplex = false;
bool sourceProvidesCohomologyTarget = false;
bool sourceProvidesSquareLaw = false;
bool sourceProvidesScalarDoubletProjection = false;
bool sourceProvidesObservedFieldExtractionRows = false;
bool sourceProvidesWeakAngleScalePoleOrGevLineage = false;

bool specifiedLocalCandidatesClosed =
    closedLocalCandidateCount == locallySpecifiedCandidateCount &&
    candidateComplexes.Where(c => c.LocallySpecified).All(c =>
        c.Status == "closed" &&
        c.ProducesWeldedScalarSmDoublet is false &&
        c.ProducesObservedProjectionData is false);
bool openCandidatesRequireNewSpecification =
    openSpecificationCandidateCount > 0 &&
    candidateComplexes.Where(c => c.Status == "open").All(c => !c.LocallySpecified);
bool requiredSpecificationAbsent =
    requiredSpecificationFields.All(f => !f.SuppliedByReviewedSource);

bool ansatzUsesPhysicalTargets = false;
var hashPayload = string.Join("\n", candidateComplexes.Select(c =>
    $"{c.Id}|{c.Carrier}|{c.EvidencePhase}|{c.LocallySpecified}|{c.TargetBlind}|{c.SquareLawMaterialized}|{c.ProducesWeldedScalarSmDoublet}|{c.ProducesObservedProjectionData}|{c.Status}|{c.DecisionReason}"));
string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(hashPayload))).ToLowerInvariant();

const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesFixedFermionicOperatorBranch = false;
const bool routeProvidesExplicitYukawaFunctional = false;
const bool routeProvidesSolvedYukawaCouplingMap = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesWeakAngleOrCouplingLineage = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

bool probeInternallyConsistent =
    phase370PrecursorPassed &&
    phase389PrecursorPassed &&
    phase397PrecursorPassed &&
    phase401PrecursorPassed &&
    phase411PrecursorPassed &&
    phase414PrecursorPassed &&
    requiredSpecificationAbsent &&
    specifiedLocalCandidatesClosed &&
    openCandidatesRequireNewSpecification &&
    candidateComplexesWithWeldedScalarSmDoubletCount == 0 &&
    candidateComplexesWithObservedProjectionDataCount == 0 &&
    !ansatzUsesPhysicalTargets;

bool fermionicCohomologySquareRootAnsatzProbePassed = probeInternallyConsistent;
bool deltaOmegaStillRequiresSpecification =
    !sourceProvidesExplicitDeltaOmega &&
    !sourceProvidesFirstOrderComplex &&
    !sourceProvidesCohomologyTarget &&
    !sourceProvidesSquareLaw;
bool unobservedPhaseStillRequiresCarrier =
    candidateComplexes.Any(c => c.Id == "unobserved-phase-augmented-complex" && c.Status == "open");

string terminalStatus = fermionicCohomologySquareRootAnsatzProbePassed
    ? "fermionic-cohomology-square-root-ansatz-closed-on-specified-carriers-delta-omega-source-required"
    : "fermionic-cohomology-square-root-ansatz-probe-review-required";

string decision = fermionicCohomologySquareRootAnsatzProbePassed
    ? "The fermionic cohomology/square-root ansatz is CLOSED on every locally specified carrier currently available. The Superphysics part-09b clue names chi, omega subfields, Upsilon_omega, cohomology obstruction language, and a possible first-order square root delta_omega, but it does not define delta_omega, a carrier complex, a cohomology target, a square law, or any scalar-doublet/observed-boson projection data. The locally specified candidates reduce to existing no-go evidence: Phase411 closes the chiral Dirac/Yukawa bilinear route; Phase389 materializes only a discrete control-branch gauge-compatibility identity; Phase397 shows neutral mixing vanishes in the fermion-bilinear channel; Phase401 closes the perturbative coupled-critical-point route on the toy action; Phase414 already closes the low-order Shiab/epsilon envelope. Further progress requires a new source-level delta_omega specification or a computable unobserved-phase carrier. No contract field is filled."
    : "Do not use this ansatz result until all precursor evidence and fail-closed accounting checks pass.";

var result = new
{
    phaseId = "phase415-fermionic-cohomology-square-root-ansatz-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    fermionicCohomologySquareRootAnsatzProbePassed,
    phase370PrecursorPassed,
    phase389PrecursorPassed,
    phase397PrecursorPassed,
    phase401PrecursorPassed,
    phase411PrecursorPassed,
    phase414PrecursorPassed,
    probeInternallyConsistent,
    part09bClue,
    requiredSpecificationFields,
    requiredSpecificationFieldCount = requiredSpecificationFields.Length,
    suppliedRequiredSpecificationFieldCount = requiredSpecificationFields.Count(f => f.SuppliedByReviewedSource),
    requiredSpecificationAbsent,
    sourceProvidesExplicitDeltaOmega,
    sourceProvidesFirstOrderComplex,
    sourceProvidesCohomologyTarget,
    sourceProvidesSquareLaw,
    sourceProvidesScalarDoubletProjection,
    sourceProvidesObservedFieldExtractionRows,
    sourceProvidesWeakAngleScalePoleOrGevLineage,
    candidateComplexes,
    localCandidateComplexCount,
    locallySpecifiedCandidateCount,
    closedLocalCandidateCount,
    openSpecificationCandidateCount,
    specifiedLocalCandidatesClosed,
    openCandidatesRequireNewSpecification,
    candidateComplexesWithWeldedScalarSmDoubletCount,
    candidateComplexesWithObservedProjectionDataCount,
    deltaOmegaStillRequiresSpecification,
    unobservedPhaseStillRequiresCarrier,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = ansatzUsesPhysicalTargets,
    targetBlindConstructionHash,
    physicalCouplingProvided,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesWeakAngleOrCouplingLineage,
    routeProvidesVevOrSourceScaleLineage,
    routeProvidesPoleExtractionAndGeVNormalization,
    routeCompletesBosonPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    sourceContractApplicationAllowed,
    phase201TemplateMutated,
    fieldsAppliedToPhase201TemplateCount,
    acceptedContractFieldCount,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    sourceEvidence = new
    {
        phase370SummaryPath = Phase370SummaryPath,
        phase389SummaryPath = Phase389SummaryPath,
        phase397SummaryPath = Phase397SummaryPath,
        phase401SummaryPath = Phase401SummaryPath,
        phase411SummaryPath = Phase411SummaryPath,
        phase414SummaryPath = Phase414SummaryPath,
        superphysicsPart09b = "https://www.superphysics.org/research/weinstein/unity/part-09b/#93-the-fermionic-sector",
        superphysicsMirrorNote = "docs/Reference/ExperimentReferences/SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md",
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "this is an ansatz admissibility and closure probe, not a derivation of delta_omega",
        "the Superphysics mirror clue is not promotion evidence and does not supply a differential or cohomology complex",
        "locally specified candidates produce neither a welded spin-0 SM doublet nor observed photon/W/Z/H projection rows",
        "open candidates require a new source-level delta_omega specification or a computable unobserved-phase carrier",
        "no physical coupling, VEV, pole extraction, or GeV normalization is supplied",
        "no Phase201 or Phase256 contract field is filled",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "fermionic_cohomology_square_root_ansatz_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "fermionic_cohomology_square_root_ansatz_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"fermionicCohomologySquareRootAnsatzProbePassed={fermionicCohomologySquareRootAnsatzProbePassed}");
Console.WriteLine($"requiredSpecificationFieldCount={requiredSpecificationFields.Length} supplied=0");
Console.WriteLine($"localCandidateComplexCount={localCandidateComplexCount} specified={locallySpecifiedCandidateCount} closed={closedLocalCandidateCount} open={openSpecificationCandidateCount}");
Console.WriteLine($"specifiedLocalCandidatesClosed={specifiedLocalCandidatesClosed}");
Console.WriteLine($"candidateComplexesWithWeldedScalarSmDoubletCount={candidateComplexesWithWeldedScalarSmDoubletCount}");
Console.WriteLine($"candidateComplexesWithObservedProjectionDataCount={candidateComplexesWithObservedProjectionDataCount}");
Console.WriteLine($"deltaOmegaStillRequiresSpecification={deltaOmegaStillRequiresSpecification}");
Console.WriteLine($"unobservedPhaseStillRequiresCarrier={unobservedPhaseStillRequiresCarrier}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetInt32()
        : null;

sealed record RequiredSpecField(
    string Id,
    string Requirement,
    bool SuppliedByReviewedSource);

sealed record CandidateComplex(
    string Id,
    string Carrier,
    string EvidencePhase,
    bool LocallySpecified,
    bool TargetBlind,
    bool SquareLawMaterialized,
    bool ProducesWeldedScalarSmDoublet,
    bool ProducesObservedProjectionData,
    string Status,
    string DecisionReason);
