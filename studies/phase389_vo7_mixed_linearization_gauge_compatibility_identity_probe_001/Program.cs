using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

// Phase389: VO-7 mixed-linearization discrete gauge-compatibility identity probe.
//
// VO-7 (Geometric_Unity_Completion_Reorganized_Updated_v29.md) requires the
// coupled boson-fermion mixed linearization blocks together with their
// gauge-compatibility identities. Phase371/372 materialized the candidate
// mixed block delta_D[b_k] and its reciprocal bilinear structure on the
// identity-weight control branch, but no phase has ever constructed or tested
// the gauge-compatibility identities themselves.
//
// This probe constructs them at the discrete control-branch level and tests
// an exact matrix identity. With the persisted Phase12 base Dirac operator
//
//   D(omega) = D_kin + delta_D[omega]            (exactly linear in omega)
//
// and an infinitesimal vertex-supported gauge parameter X (su(2) adjoint,
// X_hat = blockdiag_v rho(X_v) (x) I_spinor, anti-Hermitian), the claim is
//
//   [D(omega), X_hat] = delta_D[v(X)] + R(X)
//
// where, per edge e = (t, h) with midpoint average Xbar_e = (X_t + X_h)/2 and
// lattice differential DeltaX_e = X_h - X_t:
//
//   v(X)_e = DeltaX_e + [omega_e, Xbar_e]        (discrete covariant differential)
//   R(X)   = sum_e (1/h_e) [ E_th (x) Gamma (x) S_e + E_ht (x) Gamma^dagger (x) S_e ]
//   S_e    = ( rho(omega_e) rho(DeltaX_e) + rho(DeltaX_e) rho(omega_e) ) / 2
//
// The symmetric anticommutator term S_e is the exact gauge-compatibility
// obstruction of this discretization. It vanishes identically for constant
// (global) gauge parameters, where the identity reduces to exact equivariance
// [D, X_hat] = delta_D[[omega, X]].
//
// Contraction with persisted fermion modes gives the pure-gauge Ward identity
// for the candidate mixed-block source currents:
//
//   J_{v(X)}(psi) = Re<psi, delta_D[v(X)] psi>
//                 = Re<psi, [D, X_hat] psi> - Re<psi, R(X) psi>
//
// with Re<psi, [D, X_hat] psi> bounded by the eigen-residual of psi.
//
// This is a fail-closed boundary probe. Even if every identity check passes,
// the result is a discrete control-branch gauge-compatibility artifact for the
// candidate mixed block only. It is NOT the physical VO-7 completion: there is
// still no physical M_psi-compatible branch, no completed fermionic action, no
// coupled residual, no physical effective-action Hessian, and no observed
// electroweak namespace map. No Phase201/Phase256 contract field is filled.

const string DefaultOutputDir = "studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string BackgroundStateDir = $"{Phase12Root}/background_states";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase372SummaryPath = "studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/output/discrete_fermionic_bilinear_reciprocal_mixed_block_audit_summary.json";
const string Phase388SummaryPath = "studies/phase388_vo7_observed_electroweak_namespace_source_theorem_probe_001/output/vo7_observed_electroweak_namespace_source_theorem_probe_summary.json";

const int ExpectedBackgroundCount = 2;
const int ExpectedModesPerBackground = 12;
const int DimG = 3;
const double ReconstructionTolerance = 1e-10;
const double GammaHermiticityTolerance = 1e-12;
const double BracketClosureTolerance = 1e-12;
const double ExactIdentityTolerance = 1e-10;
const double GlobalObstructionTolerance = 1e-13;
const double WardContractionConsistencyTolerance = 1e-10;
const double WardEigenBoundSafetyFactor = 10.0;
const double WardAbsoluteFloor = 1e-12;
const double WardEigenBoundSharpnessThreshold = 1e-8;

var outputDir = Environment.GetEnvironmentVariable("PHASE389_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursor boundary checks (fail closed if upstream boundaries moved).
// ---------------------------------------------------------------------------

using var phase372 = JsonDocument.Parse(File.ReadAllText(Phase372SummaryPath));
bool phase372PrecursorPassed =
    JsonBool(phase372.RootElement, "discreteFermionicBilinearReciprocalMixedBlockAuditPassed") is true &&
    JsonBool(phase372.RootElement, "identityWeightControlBranchPassed") is true &&
    JsonBool(phase372.RootElement, "reciprocalDiscreteBilinearSourceBlockCandidateIsVo7BuildingBlock") is true &&
    JsonBool(phase372.RootElement, "reciprocalDiscreteBilinearSourceBlockCandidateCompletesVo7") is false &&
    JsonBool(phase372.RootElement, "routeProvidesPhysicalMassPsiCompatibleBranch") is false &&
    JsonBool(phase372.RootElement, "routeProvidesCompletedMixedLinearizationBlocks") is false &&
    JsonBool(phase372.RootElement, "routeProvidesMixedLinearizationGaugeCompatibilityIdentities") is false &&
    JsonBool(phase372.RootElement, "canFillPhase201WzContract") is false;

using var phase388 = JsonDocument.Parse(File.ReadAllText(Phase388SummaryPath));
bool phase388PrecursorPassed =
    JsonBool(phase388.RootElement, "vo7ObservedNamespaceSourceTheoremProbePassed") is true &&
    JsonBool(phase388.RootElement, "candidateTheoremPresent") is false &&
    JsonInt(phase388.RootElement, "missingTheoremRequirementCount") is > 0 &&
    JsonBool(phase388.RootElement, "canFillPhase201WzContract") is false;

// ---------------------------------------------------------------------------
// Shared geometry, gammas, and representation data (Phase372 conventions).
// ---------------------------------------------------------------------------

using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase389-vo7-mixed-linearization-gauge-compatibility-identity-probe",
        Branch = new() { BranchId = "phase389-vo7-mixed-linearization-gauge-compatibility-identity-probe", SchemaVersion = "1.0" },
        Backend = "cpu-reference",
    });

var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int spinorDim = spinorSpec.SpinorComponents;
int dofsPerCell = spinorDim * DimG;
int vertexCount = mesh.VertexCount;
int totalDof = vertexCount * dofsPerCell;
int edgeCount = mesh.EdgeCount;

var edgeLengths = new double[edgeCount];
var edgeDirections = new double[edgeCount][];
var cellsPerEdge = new int[edgeCount][];
for (int edge = 0; edge < edgeCount; edge++)
{
    edgeLengths[edge] = ComputeEdgeLength(mesh, edge);
    edgeDirections[edge] = ComputeEdgeDirection(mesh, edge);
    cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
}

double maxGammaHermiticityResidual = 0.0;
for (int mu = 0; mu < gammas.GammaMatrices.Length; mu++)
{
    var gamma = gammas.GammaMatrices[mu];
    double diff2 = 0.0;
    double norm2 = 0.0;
    for (int r = 0; r < spinorDim; r++)
        for (int c = 0; c < spinorDim; c++)
        {
            double dRe = gamma[r, c].Real - gamma[c, r].Real;
            double dIm = gamma[r, c].Imaginary + gamma[c, r].Imaginary;
            diff2 += dRe * dRe + dIm * dIm;
            norm2 += gamma[r, c].Real * gamma[r, c].Real + gamma[r, c].Imaginary * gamma[r, c].Imaginary;
        }
    double residual = norm2 > 0.0 ? Math.Sqrt(diff2 / norm2) : 0.0;
    maxGammaHermiticityResidual = Math.Max(maxGammaHermiticityResidual, residual);
}
bool gammaHermiticityPassed = maxGammaHermiticityResidual <= GammaHermiticityTolerance;

var fermionModesByBackground = LoadFermionModesByBackground(FermionDir);

// ---------------------------------------------------------------------------
// Per-background identity probe.
// ---------------------------------------------------------------------------

var backgroundRecords = fermionModesByBackground
    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
    .Select(pair => ProbeBackground(pair.Key, pair.Value))
    .ToArray();

int backgroundCount = backgroundRecords.Length;
int totalDirectionCount = backgroundRecords.Sum(record => record.DirectionCount);
int totalGlobalDirectionCount = backgroundRecords.Sum(record => record.GlobalDirectionCount);
int exactIdentityPassedCount = backgroundRecords.Sum(record => record.ExactIdentityPassedCount);
int bracketClosurePassedCount = backgroundRecords.Sum(record => record.BracketClosurePassedCount);
int globalObstructionFreeCount = backgroundRecords.Sum(record => record.GlobalObstructionFreeCount);
int wardContractionConsistentCount = backgroundRecords.Sum(record => record.WardContractionConsistentCount);
int wardEigenBoundPassedCount = backgroundRecords.Sum(record => record.WardEigenBoundPassedCount);
int expectedWardRowCount = totalDirectionCount * ExpectedModesPerBackground;

bool expectedCoveragePresent =
    backgroundCount == ExpectedBackgroundCount &&
    backgroundRecords.All(record =>
        record.DirectionCount == vertexCount * DimG + DimG &&
        record.GlobalDirectionCount == DimG &&
        record.ModeCount == ExpectedModesPerBackground);
bool baseDiracReconstructionFromPersistedOmegaPassed =
    backgroundRecords.All(record => record.ReconstructionPassed);
bool discreteGaugeCompatibilityIdentityExact =
    expectedCoveragePresent &&
    exactIdentityPassedCount == totalDirectionCount &&
    bracketClosurePassedCount == totalDirectionCount;
bool globalGaugeEquivarianceExactWithoutObstruction =
    globalObstructionFreeCount == totalGlobalDirectionCount;
bool pureGaugeWardContractionConsistent =
    wardContractionConsistentCount == expectedWardRowCount;
bool pureGaugeWardEigenBoundPassed =
    wardEigenBoundPassedCount == expectedWardRowCount;
double maxEigenResidualEarly = backgroundRecords.Max(record => record.MaxEigenResidual);
// The persisted Phase12 fermion modes carry large solver residuals
// (residualNorm ~ 12 is recorded in the mode artifacts themselves), so the
// eigen-residual bound is wide and the Ward zero-current statement is NOT
// sharply tested on this branch. The bound check is diagnostic only and is
// deliberately excluded from the materialization pass condition.
bool wardEigenBoundSharp = maxEigenResidualEarly <= WardEigenBoundSharpnessThreshold;
bool persistedModeEigenResidualsLarge = !wardEigenBoundSharp;
bool discreteControlBranchGaugeCompatibilityIdentityMaterialized =
    gammaHermiticityPassed &&
    baseDiracReconstructionFromPersistedOmegaPassed &&
    discreteGaugeCompatibilityIdentityExact &&
    globalGaugeEquivarianceExactWithoutObstruction &&
    pureGaugeWardContractionConsistent;

double maxReconstructionResidual = backgroundRecords.Max(record => record.ReconstructionResidual);
double maxExactIdentityRelativeResidual = backgroundRecords.Max(record => record.MaxExactIdentityRelativeResidual);
double maxBracketClosureResidual = backgroundRecords.Max(record => record.MaxBracketClosureResidual);
double maxGlobalObstructionRelativeNorm = backgroundRecords.Max(record => record.MaxGlobalObstructionRelativeNorm);
double maxObstructionToCommutatorRatio = backgroundRecords.Max(record => record.MaxObstructionToCommutatorRatio);
double maxWardContractionConsistencyResidual = backgroundRecords.Max(record => record.MaxWardContractionConsistencyResidual);
double maxEigenResidual = backgroundRecords.Max(record => record.MaxEigenResidual);

// ---------------------------------------------------------------------------
// Fail-closed VO-7 and contract boundary (unchanged by this probe).
// ---------------------------------------------------------------------------

const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesCompletedMixedLinearizationBlocks = false;
const bool routeProvidesMixedLinearizationGaugeCompatibilityIdentities = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
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
const string ApplicationSubjectKind = "vo7-mixed-linearization-gauge-compatibility-identity-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    vertexCount.ToString(),
    edgeCount.ToString(),
    DimG.ToString(),
    spinorDim.ToString(),
    totalDirectionCount.ToString(),
    "v(X)_e = DeltaX_e + [omega_e, Xbar_e]",
    "S_e = {rho(omega_e), rho(DeltaX_e)}/2",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed =
    phase372PrecursorPassed &&
    phase388PrecursorPassed &&
    discreteControlBranchGaugeCompatibilityIdentityMaterialized &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesCompletedMixedLinearizationBlocks &&
    !routeProvidesMixedLinearizationGaugeCompatibilityIdentities &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeProvidesDirectTargetIndependentWzBridgeSourceLaw &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesWeakAngleOrCouplingLineage &&
    !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization &&
    !routeCompletesBosonPredictions &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !sourceContractApplicationAllowed &&
    !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 &&
    acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed
    ? "vo7-discrete-gauge-compatibility-identity-materialized-control-branch-only-vo7-still-incomplete"
    : "vo7-discrete-gauge-compatibility-identity-probe-blocked";
string decision = vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed
    ? "The discrete gauge-compatibility identity [D(omega), X_hat] = delta_D[v(X)] + R(X) holds at machine precision on both persisted Phase12 backgrounds for every vertex-supported and global su(2) gauge parameter, with v(X)_e = DeltaX_e + [omega_e, Xbar_e] and the obstruction R(X) exactly characterized by the symmetric anticommutator S_e = {rho(omega_e), rho(DeltaX_e)}/2. Global gauge parameters are exactly equivariant with zero obstruction, and the pure-gauge mixed-block source currents satisfy the contracted identity J_v(X) = <psi,[D,X_hat]psi> - <psi,R psi> at machine precision. The Ward zero-current statement itself is NOT sharply tested because the persisted Phase12 fermion modes carry large solver residuals (recorded residualNorm ~ 12 in the mode artifacts); the eigen-residual bound is reported as diagnostic only. This is the first VO-7 gauge-compatibility artifact, but it is a discrete identity-weight control-branch result for the candidate mixed block only: it does not supply a physical M_psi-compatible branch, a completed fermionic action, completed physical mixed blocks, a physical effective-action Hessian, an observed electroweak namespace map, or any Phase201/Phase256 contract field. VO-7 remains incomplete and no physical W/Z/H promotion is possible from this route."
    : "Do not use the discrete gauge-compatibility identity until the Phase372/Phase388 precursors, base Dirac reconstruction from persisted omega, exact identity residuals, global equivariance, and Ward contraction checks all pass.";

var result = new
{
    phaseId = "phase389-vo7-mixed-linearization-gauge-compatibility-identity-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed,
    phase372PrecursorPassed,
    phase388PrecursorPassed,
    discreteControlBranchGaugeCompatibilityIdentityMaterialized,
    discreteControlBranchGaugeCompatibilityIdentityIsVo7BuildingBlock = true,
    discreteControlBranchGaugeCompatibilityIdentityCompletesVo7 = false,
    identityDefinition = "[D(omega), X_hat] = delta_D[v(X)] + R(X)",
    covariantDifferentialDefinition = "v(X)_e = (X_head - X_tail) + [omega_e, (X_head + X_tail)/2]",
    obstructionDefinition = "R(X) = sum_e (1/h_e) [E_th (x) Gamma (x) S_e + E_ht (x) Gamma^dagger (x) S_e], S_e = {rho(omega_e), rho(DeltaX_e)}/2",
    wardIdentityDefinition = "Re<psi, delta_D[v(X)] psi> = Re<psi, [D, X_hat] psi> - Re<psi, R(X) psi>, with |Re<psi, [D, X_hat] psi>| <= 2 ||D psi - lambda psi|| ||X_hat psi|| (bound diagnostic only on this branch: persisted modes are not tight eigenmodes)",
    gaugeParameterModel = "X_hat = blockdiag_v rho(X_v) (x) I_spinor with rho the su(2) adjoint representation [rho(T_a)]_{bc} = eps_{abc}; X ranges over all vertex-local generator indicators and all global generators",
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    targetBlindConstructionRule = "all gauge parameters are basis indicators fixed by mesh enumeration and generator index; no physical boson target, observed mass, or external calibration consulted",
    expectedCoveragePresent,
    gammaHermiticityPassed,
    maxGammaHermiticityResidual,
    baseDiracReconstructionFromPersistedOmegaPassed,
    discreteGaugeCompatibilityIdentityExact,
    globalGaugeEquivarianceExactWithoutObstruction,
    pureGaugeWardContractionConsistent,
    pureGaugeWardEigenBoundPassed,
    wardEigenBoundSharp,
    persistedModeEigenResidualsLarge,
    wardEigenBoundSharpnessThreshold = WardEigenBoundSharpnessThreshold,
    backgroundCount,
    vertexCount,
    edgeCount,
    spinorDim,
    dimG = DimG,
    totalDof,
    totalDirectionCount,
    totalGlobalDirectionCount,
    expectedWardRowCount,
    exactIdentityPassedCount,
    bracketClosurePassedCount,
    globalObstructionFreeCount,
    wardContractionConsistentCount,
    wardEigenBoundPassedCount,
    reconstructionTolerance = ReconstructionTolerance,
    exactIdentityTolerance = ExactIdentityTolerance,
    bracketClosureTolerance = BracketClosureTolerance,
    globalObstructionTolerance = GlobalObstructionTolerance,
    wardContractionConsistencyTolerance = WardContractionConsistencyTolerance,
    wardEigenBoundSafetyFactor = WardEigenBoundSafetyFactor,
    maxReconstructionResidual,
    maxExactIdentityRelativeResidual,
    maxBracketClosureResidual,
    maxGlobalObstructionRelativeNorm,
    maxObstructionToCommutatorRatio,
    maxWardContractionConsistencyResidual,
    maxEigenResidual,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
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
    explicitCandidateOnlyNonclaims = new[]
    {
        "discrete identity-weight control-branch result only",
        "Ward zero-current statement not sharply tested: persisted Phase12 fermion modes carry large solver residuals",
        "candidate mixed block, not a completed GU fermionic action",
        "not a physical M_psi-compatible branch",
        "not completed VO-7 mixed linearization blocks",
        "not the physical VO-7 gauge-compatibility identities",
        "no physical effective-action Hessian",
        "no observed electroweak namespace map",
        "no W/Z bridge law",
        "no Higgs scalar operator",
        "no weak-angle or coupling lineage",
        "no VEV or source-scale lineage",
        "no pole extraction or GeV normalization",
        "no physical predictions",
        "no Phase201 or Phase256 fill",
    },
    backgrounds = backgroundRecords,
    sourceEvidence = new
    {
        phase372SummaryPath = Phase372SummaryPath,
        phase388SummaryPath = Phase388SummaryPath,
        phase12Root = Phase12Root,
        fermionDir = FermionDir,
        backgroundStateDir = BackgroundStateDir,
        spinorRepresentationPath = SpinorRepresentationPath,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "vo7_mixed_linearization_gauge_compatibility_identity_probe.json");
string summaryPath = Path.Combine(outputDir, "vo7_mixed_linearization_gauge_compatibility_identity_probe_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    summaryPath,
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.generatedAt,
        terminalStatus,
        vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed,
        phase372PrecursorPassed,
        phase388PrecursorPassed,
        discreteControlBranchGaugeCompatibilityIdentityMaterialized,
        result.discreteControlBranchGaugeCompatibilityIdentityIsVo7BuildingBlock,
        result.discreteControlBranchGaugeCompatibilityIdentityCompletesVo7,
        result.identityDefinition,
        result.covariantDifferentialDefinition,
        result.obstructionDefinition,
        result.wardIdentityDefinition,
        result.applicationSubjectKind,
        result.targetBlindConstruction,
        physicalTargetsConsultedForConstruction,
        targetBlindConstructionHash,
        expectedCoveragePresent,
        gammaHermiticityPassed,
        baseDiracReconstructionFromPersistedOmegaPassed,
        discreteGaugeCompatibilityIdentityExact,
        globalGaugeEquivarianceExactWithoutObstruction,
        pureGaugeWardContractionConsistent,
        pureGaugeWardEigenBoundPassed,
        wardEigenBoundSharp,
        persistedModeEigenResidualsLarge,
        backgroundCount,
        totalDirectionCount,
        totalGlobalDirectionCount,
        expectedWardRowCount,
        exactIdentityPassedCount,
        bracketClosurePassedCount,
        globalObstructionFreeCount,
        wardContractionConsistentCount,
        wardEigenBoundPassedCount,
        maxReconstructionResidual,
        maxExactIdentityRelativeResidual,
        maxBracketClosureResidual,
        maxGlobalObstructionRelativeNorm,
        maxObstructionToCommutatorRatio,
        maxWardContractionConsistencyResidual,
        maxEigenResidual,
        routeProvidesPhysicalMassPsiCompatibleBranch,
        routeProvidesCompletedFermionicAction,
        routeProvidesCompletedMixedLinearizationBlocks,
        routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
        routeProvidesPhysicalEffectiveActionHessian,
        routeProvidesObservedElectroweakNamespaceMap,
        routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
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
        result.predictionContractImpact,
        result.explicitCandidateOnlyNonclaims,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed={vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed}");
Console.WriteLine($"phase372PrecursorPassed={phase372PrecursorPassed}");
Console.WriteLine($"phase388PrecursorPassed={phase388PrecursorPassed}");
Console.WriteLine($"discreteControlBranchGaugeCompatibilityIdentityMaterialized={discreteControlBranchGaugeCompatibilityIdentityMaterialized}");
Console.WriteLine($"baseDiracReconstructionFromPersistedOmegaPassed={baseDiracReconstructionFromPersistedOmegaPassed}");
Console.WriteLine($"discreteGaugeCompatibilityIdentityExact={discreteGaugeCompatibilityIdentityExact}");
Console.WriteLine($"globalGaugeEquivarianceExactWithoutObstruction={globalGaugeEquivarianceExactWithoutObstruction}");
Console.WriteLine($"pureGaugeWardContractionConsistent={pureGaugeWardContractionConsistent}");
Console.WriteLine($"pureGaugeWardEigenBoundPassed={pureGaugeWardEigenBoundPassed}");
Console.WriteLine($"wardEigenBoundSharp={wardEigenBoundSharp}");
Console.WriteLine($"persistedModeEigenResidualsLarge={persistedModeEigenResidualsLarge}");
Console.WriteLine($"totalDirectionCount={totalDirectionCount}");
Console.WriteLine($"exactIdentityPassedCount={exactIdentityPassedCount}");
Console.WriteLine($"maxReconstructionResidual={maxReconstructionResidual:R}");
Console.WriteLine($"maxExactIdentityRelativeResidual={maxExactIdentityRelativeResidual:R}");
Console.WriteLine($"maxGlobalObstructionRelativeNorm={maxGlobalObstructionRelativeNorm:R}");
Console.WriteLine($"maxObstructionToCommutatorRatio={maxObstructionToCommutatorRatio:R}");
Console.WriteLine($"maxWardContractionConsistencyResidual={maxWardContractionConsistencyResidual:R}");
Console.WriteLine($"maxEigenResidual={maxEigenResidual:R}");
Console.WriteLine($"routeProvidesMixedLinearizationGaugeCompatibilityIdentities={routeProvidesMixedLinearizationGaugeCompatibilityIdentities}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Background probe implementation.
// ---------------------------------------------------------------------------

BackgroundIdentityRecord ProbeBackground(string backgroundId, IReadOnlyList<FermionModeSnapshot> fermionModes)
{
    if (fermionModes.Count != ExpectedModesPerBackground)
        throw new InvalidDataException($"Expected {ExpectedModesPerBackground} fermion modes for {backgroundId}, found {fermionModes.Count}.");

    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    int[] shape = metadataDoc.RootElement.GetProperty("matrixShape").EnumerateArray().Select(value => value.GetInt32()).ToArray();
    if (shape.Length != 2 || shape[0] != shape[1] || shape[0] != totalDof)
        throw new InvalidDataException($"Expected a {totalDof}x{totalDof} base Dirac matrix for {backgroundId}.");
    string matrixPath = Path.Combine(FermionDir, matrixRef);
    var (dRe, dIm) = LoadFlatInterleavedMatrix(matrixPath, totalDof);

    string omegaPath = Path.Combine(BackgroundStateDir, $"{backgroundId}_omega.json");
    using var omegaDoc = JsonDocument.Parse(File.ReadAllText(omegaPath));
    double[] omega = omegaDoc.RootElement.GetProperty("coefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray();
    if (omega.Length != edgeCount * DimG)
        throw new InvalidDataException($"Expected {edgeCount * DimG} omega coefficients for {backgroundId}, found {omega.Length}.");

    // Reconstruction: D - delta_D[omega] must be the omega-independent kinetic
    // operator (gauge-diagonal and gauge-replicated), proving exact linearity
    // of the persisted assembly in omega.
    var (gaugeRe, gaugeIm) = DiracVariationComputer.ComputeAnalytical(
        omega, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
    var kinRe = new double[totalDof, totalDof];
    var kinIm = new double[totalDof, totalDof];
    for (int row = 0; row < totalDof; row++)
        for (int col = 0; col < totalDof; col++)
        {
            kinRe[row, col] = dRe[row, col] - gaugeRe[row, col];
            kinIm[row, col] = dIm[row, col] - gaugeIm[row, col];
        }
    double reconstructionResidual = KineticStructureResidual(kinRe, kinIm);
    bool reconstructionPassed = reconstructionResidual <= ReconstructionTolerance;

    // Mode preprocessing: Rayleigh eigenvalues and eigen-residuals.
    var modeInfos = fermionModes
        .OrderBy(mode => mode.ModeIndex)
        .Select(mode =>
        {
            var dPsi = ApplyComplexMatrix(dRe, dIm, mode.EigenvectorCoefficients);
            var norm2 = ComplexInnerProduct(mode.EigenvectorCoefficients, mode.EigenvectorCoefficients).Real;
            double lambda = ComplexInnerProduct(mode.EigenvectorCoefficients, dPsi).Real / norm2;
            double residual2 = 0.0;
            for (int index = 0; index < dPsi.Length; index += 2)
            {
                double rRe = dPsi[index] - lambda * mode.EigenvectorCoefficients[index];
                double rIm = dPsi[index + 1] - lambda * mode.EigenvectorCoefficients[index + 1];
                residual2 += rRe * rRe + rIm * rIm;
            }
            return new ModeInfo
            {
                Mode = mode,
                RayleighEigenvalue = lambda,
                EigenResidualNorm = Math.Sqrt(residual2),
                ModeNorm = Math.Sqrt(norm2),
            };
        })
        .ToArray();
    double maxEigenResidualLocal = modeInfos.Max(info => info.EigenResidualNorm / Math.Max(info.ModeNorm, 1e-30));

    // Gauge parameter directions: all vertex-local indicators, then globals.
    var directions = new List<GaugeDirectionSpec>();
    for (int vertex = 0; vertex < vertexCount; vertex++)
        for (int generator = 0; generator < DimG; generator++)
            directions.Add(new GaugeDirectionSpec
            {
                DirectionId = $"vertex-{vertex}-generator-{generator}",
                IsGlobal = false,
                Vertex = vertex,
                Generator = generator,
            });
    for (int generator = 0; generator < DimG; generator++)
        directions.Add(new GaugeDirectionSpec
        {
            DirectionId = $"global-generator-{generator}",
            IsGlobal = true,
            Vertex = -1,
            Generator = generator,
        });

    var directionRecords = directions
        .Select(spec => ProbeDirection(spec, dRe, dIm, omega, modeInfos))
        .ToArray();

    int exactPassed = directionRecords.Count(record => record.ExactIdentityPassed);
    int closurePassed = directionRecords.Count(record => record.BracketClosurePassed);
    int globalCount = directionRecords.Count(record => record.IsGlobal);
    int globalObstructionFree = directionRecords.Count(record => record.IsGlobal && record.ObstructionRelativeNorm <= GlobalObstructionTolerance);
    int wardConsistent = directionRecords.Sum(record => record.WardContractionConsistentCount);
    int wardEigenBound = directionRecords.Sum(record => record.WardEigenBoundPassedCount);

    return new BackgroundIdentityRecord
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMetadataPath = metadataPath,
        BaseDiracMatrixPath = matrixPath,
        PersistedOmegaPath = omegaPath,
        ReconstructionResidual = reconstructionResidual,
        ReconstructionPassed = reconstructionPassed,
        ModeCount = modeInfos.Length,
        MaxEigenResidual = maxEigenResidualLocal,
        DirectionCount = directionRecords.Length,
        GlobalDirectionCount = globalCount,
        ExactIdentityPassedCount = exactPassed,
        BracketClosurePassedCount = closurePassed,
        GlobalObstructionFreeCount = globalObstructionFree,
        WardContractionConsistentCount = wardConsistent,
        WardEigenBoundPassedCount = wardEigenBound,
        MaxExactIdentityRelativeResidual = directionRecords.Max(record => record.ExactIdentityRelativeResidual),
        MaxBracketClosureResidual = directionRecords.Max(record => record.BracketClosureResidual),
        MaxGlobalObstructionRelativeNorm = directionRecords.Where(record => record.IsGlobal).Max(record => record.ObstructionRelativeNorm),
        MaxObstructionToCommutatorRatio = directionRecords.Max(record => record.ObstructionToCommutatorRatio),
        MaxWardContractionConsistencyResidual = directionRecords.Max(record => record.MaxWardContractionConsistencyResidual),
        GlobalDirectionWardRows = directionRecords
            .Where(record => record.IsGlobal)
            .SelectMany(record => record.GlobalWardRows)
            .ToArray(),
        Directions = directionRecords,
    };
}

GaugeDirectionRecord ProbeDirection(
    GaugeDirectionSpec spec,
    double[,] dRe,
    double[,] dIm,
    double[] omega,
    ModeInfo[] modeInfos)
{
    // Gauge parameter X: vertex -> su(2) coefficient triple.
    var x = new double[vertexCount][];
    for (int vertex = 0; vertex < vertexCount; vertex++)
    {
        x[vertex] = new double[DimG];
        if (spec.IsGlobal || vertex == spec.Vertex)
            x[vertex][spec.Generator] = 1.0;
    }

    // Discrete covariant differential v(X) and obstruction blocks S_e.
    var v = new double[edgeCount * DimG];
    var sBlocks = new double[edgeCount][,];
    double bracketClosureResidual = 0.0;
    for (int edge = 0; edge < edgeCount; edge++)
    {
        int tail = cellsPerEdge[edge][0];
        int head = cellsPerEdge[edge][1];
        var deltaX = new double[DimG];
        var xBar = new double[DimG];
        for (int a = 0; a < DimG; a++)
        {
            deltaX[a] = x[head][a] - x[tail][a];
            xBar[a] = 0.5 * (x[head][a] + x[tail][a]);
        }

        var omegaEdge = new double[DimG];
        for (int a = 0; a < DimG; a++)
            omegaEdge[a] = omega[edge * DimG + a];

        var gOmega = AdjointRho(omegaEdge);
        var rhoXBar = AdjointRho(xBar);
        var rhoDeltaX = AdjointRho(deltaX);

        // Commutator [rho(omega_e), rho(Xbar)] projected back to coefficients,
        // with an exact closure check rho(k) == [rho(omega_e), rho(Xbar)].
        var commutator = new double[DimG, DimG];
        for (int b = 0; b < DimG; b++)
            for (int c = 0; c < DimG; c++)
            {
                double value = 0.0;
                for (int k = 0; k < DimG; k++)
                    value += gOmega[b, k] * rhoXBar[k, c] - rhoXBar[b, k] * gOmega[k, c];
                commutator[b, c] = value;
            }
        var bracket = ProjectToAdjointCoefficients(commutator);
        var rhoBracket = AdjointRho(bracket);
        double closure2 = 0.0;
        for (int b = 0; b < DimG; b++)
            for (int c = 0; c < DimG; c++)
            {
                double diff = rhoBracket[b, c] - commutator[b, c];
                closure2 += diff * diff;
            }
        bracketClosureResidual = Math.Max(bracketClosureResidual, Math.Sqrt(closure2));

        for (int a = 0; a < DimG; a++)
            v[edge * DimG + a] = deltaX[a] + bracket[a];

        // Symmetric obstruction S_e = {rho(omega_e), rho(DeltaX_e)} / 2.
        var s = new double[DimG, DimG];
        for (int b = 0; b < DimG; b++)
            for (int c = 0; c < DimG; c++)
            {
                double value = 0.0;
                for (int k = 0; k < DimG; k++)
                    value += gOmega[b, k] * rhoDeltaX[k, c] + rhoDeltaX[b, k] * gOmega[k, c];
                s[b, c] = 0.5 * value;
            }
        sBlocks[edge] = s;
    }

    // Commutator C = D X_hat - X_hat D using the vertex-block structure of X_hat.
    var (cRe, cIm) = BuildCommutator(dRe, dIm, x);

    // delta_D[v(X)] via the shared analytical variation computer.
    var (vdRe, vdIm) = DiracVariationComputer.ComputeAnalytical(
        v, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);

    // Predicted obstruction matrix R(X) with the same edge placement rules.
    var (rRe, rIm) = BuildObstructionMatrix(sBlocks);

    double commutatorNorm = FrobeniusNorm(cRe, cIm);
    double obstructionNorm = FrobeniusNorm(rRe, rIm);
    double residual2 = 0.0;
    for (int row = 0; row < totalDof; row++)
        for (int col = 0; col < totalDof; col++)
        {
            double diffRe = cRe[row, col] - vdRe[row, col] - rRe[row, col];
            double diffIm = cIm[row, col] - vdIm[row, col] - rIm[row, col];
            residual2 += diffRe * diffRe + diffIm * diffIm;
        }
    double scale = Math.Max(commutatorNorm, 1e-30);
    double exactIdentityRelativeResidual = Math.Sqrt(residual2) / scale;
    bool exactIdentityPassed = exactIdentityRelativeResidual <= ExactIdentityTolerance;
    double obstructionRelativeNorm = obstructionNorm / scale;
    double obstructionToCommutatorRatio = obstructionRelativeNorm;

    // Ward contraction rows over every persisted fermion mode.
    int wardConsistentCount = 0;
    int wardEigenBoundCount = 0;
    double maxWardConsistencyResidual = 0.0;
    var globalWardRows = new List<WardRow>();
    foreach (var info in modeInfos)
    {
        var psi = info.Mode.EigenvectorCoefficients;
        double commutatorCurrent = ComplexInnerProduct(psi, ApplyComplexMatrix(cRe, cIm, psi)).Real;
        double mixedBlockCurrent = ComplexInnerProduct(psi, ApplyComplexMatrix(vdRe, vdIm, psi)).Real;
        double obstructionCurrent = ComplexInnerProduct(psi, ApplyComplexMatrix(rRe, rIm, psi)).Real;
        double consistencyResidual = ScaleAwareResidual(commutatorCurrent, mixedBlockCurrent + obstructionCurrent);
        bool consistent = consistencyResidual <= WardContractionConsistencyTolerance;
        if (consistent)
            wardConsistentCount++;
        maxWardConsistencyResidual = Math.Max(maxWardConsistencyResidual, consistencyResidual);

        var xPsi = ApplyGaugeParameter(x, psi);
        double xPsiNorm = Math.Sqrt(ComplexInnerProduct(xPsi, xPsi).Real);
        double wardBound =
            WardEigenBoundSafetyFactor * 2.0 * info.EigenResidualNorm * xPsiNorm + WardAbsoluteFloor;
        bool eigenBoundPassed = Math.Abs(commutatorCurrent) <= wardBound;
        if (eigenBoundPassed)
            wardEigenBoundCount++;

        if (spec.IsGlobal)
            globalWardRows.Add(new WardRow
            {
                DirectionId = spec.DirectionId,
                ModeId = info.Mode.ModeId,
                ModeIndex = info.Mode.ModeIndex,
                RayleighEigenvalue = info.RayleighEigenvalue,
                EigenResidualNorm = info.EigenResidualNorm,
                CommutatorCurrent = commutatorCurrent,
                MixedBlockCurrent = mixedBlockCurrent,
                ObstructionCurrent = obstructionCurrent,
                ContractionConsistencyResidual = consistencyResidual,
                WardEigenBound = wardBound,
                WardEigenBoundPassed = eigenBoundPassed,
            });
    }

    return new GaugeDirectionRecord
    {
        DirectionId = spec.DirectionId,
        IsGlobal = spec.IsGlobal,
        Vertex = spec.Vertex,
        Generator = spec.Generator,
        BracketClosureResidual = bracketClosureResidual,
        BracketClosurePassed = bracketClosureResidual <= BracketClosureTolerance,
        CommutatorFrobeniusNorm = commutatorNorm,
        ObstructionFrobeniusNorm = obstructionNorm,
        ObstructionRelativeNorm = obstructionRelativeNorm,
        ObstructionToCommutatorRatio = obstructionToCommutatorRatio,
        ExactIdentityRelativeResidual = exactIdentityRelativeResidual,
        ExactIdentityPassed = exactIdentityPassed,
        WardContractionConsistentCount = wardConsistentCount,
        WardEigenBoundPassedCount = wardEigenBoundCount,
        MaxWardContractionConsistencyResidual = maxWardConsistencyResidual,
        GlobalWardRows = globalWardRows,
    };
}

(double[,] Re, double[,] Im) BuildCommutator(double[,] dRe, double[,] dIm, double[][] x)
{
    var cRe = new double[totalDof, totalDof];
    var cIm = new double[totalDof, totalDof];
    for (int vertex = 0; vertex < vertexCount; vertex++)
    {
        var rho = AdjointRho(x[vertex]);
        bool nonZero = false;
        for (int b = 0; b < DimG && !nonZero; b++)
            for (int c = 0; c < DimG && !nonZero; c++)
                if (rho[b, c] != 0.0)
                    nonZero = true;
        if (!nonZero)
            continue;

        // D X_hat: transform the columns of the vertex block (real rho).
        for (int row = 0; row < totalDof; row++)
            for (int gCol = 0; gCol < DimG; gCol++)
                for (int sCol = 0; sCol < spinorDim; sCol++)
                {
                    int col = vertex * dofsPerCell + gCol * spinorDim + sCol;
                    double sumRe = 0.0;
                    double sumIm = 0.0;
                    for (int g = 0; g < DimG; g++)
                    {
                        double weight = rho[g, gCol];
                        if (weight == 0.0)
                            continue;
                        int source = vertex * dofsPerCell + g * spinorDim + sCol;
                        sumRe += dRe[row, source] * weight;
                        sumIm += dIm[row, source] * weight;
                    }
                    cRe[row, col] += sumRe;
                    cIm[row, col] += sumIm;
                }

        // X_hat D: transform the rows of the vertex block.
        for (int gRow = 0; gRow < DimG; gRow++)
            for (int sRow = 0; sRow < spinorDim; sRow++)
            {
                int row = vertex * dofsPerCell + gRow * spinorDim + sRow;
                for (int col = 0; col < totalDof; col++)
                {
                    double sumRe = 0.0;
                    double sumIm = 0.0;
                    for (int g = 0; g < DimG; g++)
                    {
                        double weight = rho[gRow, g];
                        if (weight == 0.0)
                            continue;
                        int source = vertex * dofsPerCell + g * spinorDim + sRow;
                        sumRe += weight * dRe[source, col];
                        sumIm += weight * dIm[source, col];
                    }
                    cRe[row, col] -= sumRe;
                    cIm[row, col] -= sumIm;
                }
            }
    }

    return (cRe, cIm);
}

(double[,] Re, double[,] Im) BuildObstructionMatrix(double[][,] sBlocks)
{
    var rRe = new double[totalDof, totalDof];
    var rIm = new double[totalDof, totalDof];
    int nGammas = gammas.GammaMatrices.Length;
    for (int edge = 0; edge < edgeCount; edge++)
    {
        var cells = cellsPerEdge[edge];
        if (cells.Length < 2)
            continue;
        int tail = cells[0];
        int head = cells[1];
        double h = edgeLengths[edge];
        if (h < 1e-30)
            continue;
        int mu = DominantDirection(edgeDirections[edge]);
        if (mu >= nGammas)
            continue;
        double invH = 1.0 / h;
        var gamma = gammas.GammaMatrices[mu];
        var s = sBlocks[edge];

        for (int sRow = 0; sRow < spinorDim; sRow++)
            for (int sCol = 0; sCol < spinorDim; sCol++)
            {
                double forwardRe = gamma[sRow, sCol].Real;
                double forwardIm = gamma[sRow, sCol].Imaginary;
                double backwardRe = gamma[sCol, sRow].Real;
                double backwardIm = -gamma[sCol, sRow].Imaginary;
                for (int gRow = 0; gRow < DimG; gRow++)
                    for (int gCol = 0; gCol < DimG; gCol++)
                    {
                        double sValue = s[gRow, gCol];
                        if (sValue == 0.0)
                            continue;
                        int rowForward = tail * dofsPerCell + gRow * spinorDim + sRow;
                        int colForward = head * dofsPerCell + gCol * spinorDim + sCol;
                        rRe[rowForward, colForward] += invH * forwardRe * sValue;
                        rIm[rowForward, colForward] += invH * forwardIm * sValue;

                        int rowBackward = head * dofsPerCell + gRow * spinorDim + sRow;
                        int colBackward = tail * dofsPerCell + gCol * spinorDim + sCol;
                        rRe[rowBackward, colBackward] += invH * backwardRe * sValue;
                        rIm[rowBackward, colBackward] += invH * backwardIm * sValue;
                    }
            }
    }

    return (rRe, rIm);
}

double KineticStructureResidual(double[,] kinRe, double[,] kinIm)
{
    // The omega-independent kinetic operator must be gauge-diagonal and
    // gauge-replicated: blocks with gRow != gCol vanish and every (g, g)
    // spinor block equals the (0, 0) spinor block of the same vertex pair.
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int vRow = 0; vRow < vertexCount; vRow++)
        for (int vCol = 0; vCol < vertexCount; vCol++)
            for (int sRow = 0; sRow < spinorDim; sRow++)
                for (int sCol = 0; sCol < spinorDim; sCol++)
                {
                    int referenceRow = vRow * dofsPerCell + sRow;
                    int referenceCol = vCol * dofsPerCell + sCol;
                    double refRe = kinRe[referenceRow, referenceCol];
                    double refIm = kinIm[referenceRow, referenceCol];
                    for (int gRow = 0; gRow < DimG; gRow++)
                        for (int gCol = 0; gCol < DimG; gCol++)
                        {
                            int row = vRow * dofsPerCell + gRow * spinorDim + sRow;
                            int col = vCol * dofsPerCell + gCol * spinorDim + sCol;
                            double valueRe = kinRe[row, col];
                            double valueIm = kinIm[row, col];
                            norm2 += valueRe * valueRe + valueIm * valueIm;
                            double expectedRe = gRow == gCol ? refRe : 0.0;
                            double expectedIm = gRow == gCol ? refIm : 0.0;
                            double diffRe = valueRe - expectedRe;
                            double diffIm = valueIm - expectedIm;
                            residual2 += diffRe * diffRe + diffIm * diffIm;
                        }
                }
    return norm2 > 0.0 ? Math.Sqrt(residual2 / norm2) : 0.0;
}

double[] ApplyGaugeParameter(double[][] x, double[] psi)
{
    var result = new double[psi.Length];
    for (int vertex = 0; vertex < vertexCount; vertex++)
    {
        var rho = AdjointRho(x[vertex]);
        for (int gRow = 0; gRow < DimG; gRow++)
            for (int s = 0; s < spinorDim; s++)
            {
                int target = 2 * (vertex * dofsPerCell + gRow * spinorDim + s);
                double sumRe = 0.0;
                double sumIm = 0.0;
                for (int gCol = 0; gCol < DimG; gCol++)
                {
                    double weight = rho[gRow, gCol];
                    if (weight == 0.0)
                        continue;
                    int source = 2 * (vertex * dofsPerCell + gCol * spinorDim + s);
                    sumRe += weight * psi[source];
                    sumIm += weight * psi[source + 1];
                }
                result[target] = sumRe;
                result[target + 1] = sumIm;
            }
    }
    return result;
}

static double[,] AdjointRho(double[] coefficients)
{
    // su(2) adjoint representation: [rho(T_a)]_{bc} = eps_{abc}.
    var rho = new double[3, 3];
    for (int a = 0; a < 3; a++)
    {
        double value = coefficients[a];
        if (value == 0.0)
            continue;
        for (int b = 0; b < 3; b++)
            for (int c = 0; c < 3; c++)
                rho[b, c] += value * LeviCivita3(a, b, c);
    }
    return rho;
}

static double[] ProjectToAdjointCoefficients(double[,] matrix)
{
    // k^a = (1/2) sum_{bc} eps_{abc} K_{bc} inverts rho for matrices in its image.
    var coefficients = new double[3];
    for (int a = 0; a < 3; a++)
    {
        double sum = 0.0;
        for (int b = 0; b < 3; b++)
            for (int c = 0; c < 3; c++)
                sum += LeviCivita3(a, b, c) * matrix[b, c];
        coefficients[a] = 0.5 * sum;
    }
    return coefficients;
}

static double LeviCivita3(int a, int b, int c)
{
    if (a == b || b == c || a == c) return 0.0;
    if ((a == 0 && b == 1 && c == 2) ||
        (a == 1 && b == 2 && c == 0) ||
        (a == 2 && b == 0 && c == 1)) return 1.0;
    return -1.0;
}

static int DominantDirection(IReadOnlyList<double> direction)
{
    var mu = 0;
    var best = 0.0;
    for (int i = 0; i < direction.Count; i++)
    {
        var abs = Math.Abs(direction[i]);
        if (abs > best)
        {
            best = abs;
            mu = i;
        }
    }
    return mu;
}

static double FrobeniusNorm(double[,] re, double[,] im)
{
    double sum = 0.0;
    int rows = re.GetLength(0);
    int cols = re.GetLength(1);
    for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
            sum += re[row, col] * re[row, col] + im[row, col] * im[row, col];
    return Math.Sqrt(sum);
}

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
    element.TryGetProperty(propertyName, out var value) &&
    value.ValueKind == JsonValueKind.Number &&
    value.TryGetInt32(out var result)
        ? result
        : null;

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? throw new InvalidDataException($"{propertyName} must not be null.")
        : throw new InvalidDataException($"{propertyName} must be a string.");

static IReadOnlyDictionary<string, IReadOnlyList<FermionModeSnapshot>> LoadFermionModesByBackground(string fermionDir)
{
    var result = new Dictionary<string, IReadOnlyList<FermionModeSnapshot>>(StringComparer.Ordinal);
    foreach (string path in Directory.GetFiles(fermionDir, "fermion_modes_*.json").OrderBy(path => path, StringComparer.Ordinal))
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        string backgroundId = RequiredString(doc.RootElement, "fermionBackgroundId");
        var modes = doc.RootElement
            .GetProperty("modes")
            .EnumerateArray()
            .Select(mode => new FermionModeSnapshot
            {
                ModeId = RequiredString(mode, "modeId"),
                ModeIndex = mode.GetProperty("modeIndex").GetInt32(),
                EigenvectorCoefficients = mode
                    .GetProperty("eigenvectorCoefficients")
                    .EnumerateArray()
                    .Select(value => value.GetDouble())
                    .ToArray(),
            })
            .ToArray();
        result.Add(backgroundId, modes);
    }

    return result;
}

static (double[,] Re, double[,] Im) LoadFlatInterleavedMatrix(string path, int size)
{
    var values = JsonSerializer.Deserialize<double[]>(File.ReadAllText(path))
        ?? throw new InvalidDataException($"Failed to deserialize {path}.");
    if (values.Length != 2 * size * size)
        throw new InvalidDataException($"Expected {2 * size * size} interleaved matrix values in {path}, found {values.Length}.");
    var re = new double[size, size];
    var im = new double[size, size];
    for (int row = 0; row < size; row++)
        for (int col = 0; col < size; col++)
        {
            int index = 2 * (row * size + col);
            re[row, col] = values[index];
            im[row, col] = values[index + 1];
        }
    return (re, im);
}

static double[] ApplyComplexMatrix(double[,] re, double[,] im, double[] vector)
{
    int rows = re.GetLength(0);
    int cols = re.GetLength(1);
    if (vector.Length != 2 * cols)
        throw new InvalidDataException($"Expected complex interleaved vector length {2 * cols}, found {vector.Length}.");
    var result = new double[2 * rows];
    for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            double aRe = re[row, col];
            double aIm = im[row, col];
            double bRe = vector[2 * col];
            double bIm = vector[2 * col + 1];
            result[2 * row] += aRe * bRe - aIm * bIm;
            result[2 * row + 1] += aRe * bIm + aIm * bRe;
        }
    return result;
}

static (double Real, double Imaginary) ComplexInnerProduct(double[] left, double[] right)
{
    if (left.Length != right.Length || left.Length % 2 != 0)
        throw new InvalidDataException("Complex interleaved vectors must have equal even lengths.");
    double real = 0.0;
    double imaginary = 0.0;
    for (int index = 0; index < left.Length; index += 2)
    {
        double leftRe = left[index];
        double leftIm = left[index + 1];
        double rightRe = right[index];
        double rightIm = right[index + 1];
        real += leftRe * rightRe + leftIm * rightIm;
        imaginary += leftRe * rightIm - leftIm * rightRe;
    }
    return (real, imaginary);
}

static double ScaleAwareResidual(double left, double right) =>
    Math.Abs(left - right) / Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right)));

static double ComputeEdgeLength(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    double norm = 0.0;
    for (int k = 0; k < coords0.Length; k++)
    {
        double d = coords1[k] - coords0[k];
        norm += d * d;
    }
    return Math.Sqrt(norm);
}

static double[] ComputeEdgeDirection(SimplicialMesh mesh, int edgeIdx)
{
    int v0 = mesh.Edges[edgeIdx][0];
    int v1 = mesh.Edges[edgeIdx][1];
    int dim = mesh.EmbeddingDimension;
    var coords0 = mesh.GetVertexCoordinates(v0);
    var coords1 = mesh.GetVertexCoordinates(v1);
    var direction = new double[dim];
    double norm = 0.0;
    for (int k = 0; k < dim; k++)
    {
        direction[k] = coords1[k] - coords0[k];
        norm += direction[k] * direction[k];
    }
    norm = Math.Sqrt(norm);
    if (norm > 1e-14)
        for (int k = 0; k < dim; k++)
            direction[k] /= norm;
    return direction;
}

public sealed class FermionModeSnapshot
{
    public required string ModeId { get; init; }
    public required int ModeIndex { get; init; }
    public required double[] EigenvectorCoefficients { get; init; }
}

public sealed class ModeInfo
{
    public required FermionModeSnapshot Mode { get; init; }
    public required double RayleighEigenvalue { get; init; }
    public required double EigenResidualNorm { get; init; }
    public required double ModeNorm { get; init; }
}

public sealed class GaugeDirectionSpec
{
    public required string DirectionId { get; init; }
    public required bool IsGlobal { get; init; }
    public required int Vertex { get; init; }
    public required int Generator { get; init; }
}

public sealed class GaugeDirectionRecord
{
    public required string DirectionId { get; init; }
    public required bool IsGlobal { get; init; }
    public required int Vertex { get; init; }
    public required int Generator { get; init; }
    public required double BracketClosureResidual { get; init; }
    public required bool BracketClosurePassed { get; init; }
    public required double CommutatorFrobeniusNorm { get; init; }
    public required double ObstructionFrobeniusNorm { get; init; }
    public required double ObstructionRelativeNorm { get; init; }
    public required double ObstructionToCommutatorRatio { get; init; }
    public required double ExactIdentityRelativeResidual { get; init; }
    public required bool ExactIdentityPassed { get; init; }
    public required int WardContractionConsistentCount { get; init; }
    public required int WardEigenBoundPassedCount { get; init; }
    public required double MaxWardContractionConsistencyResidual { get; init; }
    public required IReadOnlyList<WardRow> GlobalWardRows { get; init; }
}

public sealed class WardRow
{
    public required string DirectionId { get; init; }
    public required string ModeId { get; init; }
    public required int ModeIndex { get; init; }
    public required double RayleighEigenvalue { get; init; }
    public required double EigenResidualNorm { get; init; }
    public required double CommutatorCurrent { get; init; }
    public required double MixedBlockCurrent { get; init; }
    public required double ObstructionCurrent { get; init; }
    public required double ContractionConsistencyResidual { get; init; }
    public required double WardEigenBound { get; init; }
    public required bool WardEigenBoundPassed { get; init; }
}

public sealed class BackgroundIdentityRecord
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMetadataPath { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required string PersistedOmegaPath { get; init; }
    public required double ReconstructionResidual { get; init; }
    public required bool ReconstructionPassed { get; init; }
    public required int ModeCount { get; init; }
    public required double MaxEigenResidual { get; init; }
    public required int DirectionCount { get; init; }
    public required int GlobalDirectionCount { get; init; }
    public required int ExactIdentityPassedCount { get; init; }
    public required int BracketClosurePassedCount { get; init; }
    public required int GlobalObstructionFreeCount { get; init; }
    public required int WardContractionConsistentCount { get; init; }
    public required int WardEigenBoundPassedCount { get; init; }
    public required double MaxExactIdentityRelativeResidual { get; init; }
    public required double MaxBracketClosureResidual { get; init; }
    public required double MaxGlobalObstructionRelativeNorm { get; init; }
    public required double MaxObstructionToCommutatorRatio { get; init; }
    public required double MaxWardContractionConsistencyResidual { get; init; }
    public required IReadOnlyList<WardRow> GlobalDirectionWardRows { get; init; }
    public required IReadOnlyList<GaugeDirectionRecord> Directions { get; init; }
}
