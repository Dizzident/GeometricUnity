using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

// Phase390: converged control-branch fermion mode rebuild and sharp pure-gauge
// Ward zero-current probe.
//
// Phase389 materialized the exact discrete gauge-compatibility identity for
// the VO-7 candidate mixed block, but left two boundary defects open:
//
//   1. The persisted Phase12 fermion modes are not tight eigenmodes of the
//      persisted explicit base Dirac matrix (mode artifacts record
//      residualNorm ~ 12), so the pure-gauge Ward zero-current statement was
//      not sharply testable.
//   2. The mesh-volume M_psi branch was left as a diagnostic with the
//      Phase372 actionable obstruction: "Rebuild an M_psi-compatible Dirac
//      branch and solve matching M_psi-compatible fermion modes."
//
// This probe removes both defects at the discrete control-branch level:
//
//   - It eigensolves the persisted base Dirac matrix D directly (identity
//     weight branch) with an in-study complex Hermitian Jacobi solver and
//     verifies converged eigenpairs (residual and orthonormality checks
//     against the original matrix).
//   - It eigensolves the symmetrized generalized pencil
//     B = M^{-1/2} D M^{-1/2} (mesh-volume M_psi from
//     MassPsiWeightsBuilder.BuildFromMesh) and reconstructs M-orthonormal
//     generalized modes v = M^{-1/2} w with D v = lambda M v verified.
//   - It verifies the structural fact [M, X_hat] = 0 (M_psi is scalar per
//     vertex block while gauge parameters act within vertex blocks), which
//     conjugates the Phase389 identity exactly onto the M_psi branch:
//     [B, X_hat] = M^{-1/2} (delta_D[v(X)] + R(X)) M^{-1/2}.
//   - It re-runs the pure-gauge Ward zero-current test SHARPLY on both
//     converged branches: |Re<psi, [D, X_hat] psi>| <= 2 ||r|| ||X_hat psi||
//     with ||r|| now at solver precision, and the analogous generalized
//     statement Re<v, [D, X_hat] v> for D v = lambda M v modes (valid because
//     [M, X_hat] = 0).
//   - It characterizes exactly how unconverged the persisted Phase12 mode
//     branch is (per-mode relative residuals and best overlaps with the
//     converged eigenbasis).
//
// This is a fail-closed control-branch rebuild. The mesh, backgrounds, and
// M_psi remain toy/study-defined objects. It does NOT provide a physical
// M_psi-compatible GU branch, a completed fermionic action, completed
// physical mixed blocks, a physical effective-action Hessian, or an observed
// electroweak namespace map, and it fills no Phase201/Phase256 contract
// field.

const string DefaultOutputDir = "studies/phase390_converged_control_branch_fermion_mode_rebuild_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string BackgroundStateDir = $"{Phase12Root}/background_states";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase372SummaryPath = "studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/output/discrete_fermionic_bilinear_reciprocal_mixed_block_audit_summary.json";
const string Phase389SummaryPath = "studies/phase389_vo7_mixed_linearization_gauge_compatibility_identity_probe_001/output/vo7_mixed_linearization_gauge_compatibility_identity_probe_summary.json";

const int ExpectedBackgroundCount = 2;
const int ExpectedModesPerBackground = 12;
const int RebuiltModesPerBranch = 12;
const int DimG = 3;
const double JacobiOffDiagonalTolerance = 1e-13;
const int JacobiMaxSweeps = 100;
const double EigenResidualTolerance = 1e-9;
const double OrthonormalityTolerance = 1e-10;
const double MCommutatorTolerance = 1e-14;
const double WardEigenBoundSafetyFactor = 10.0;
const double WardAbsoluteFloor = 1e-9;
const double PersistedUnconvergedThreshold = 1e-6;

var outputDir = Environment.GetEnvironmentVariable("PHASE390_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursor boundary checks.
// ---------------------------------------------------------------------------

using var phase372 = JsonDocument.Parse(File.ReadAllText(Phase372SummaryPath));
bool phase372PrecursorPassed =
    JsonBool(phase372.RootElement, "discreteFermionicBilinearReciprocalMixedBlockAuditPassed") is true &&
    JsonBool(phase372.RootElement, "meshVolumeWeightBranchCompatible") is false &&
    JsonBool(phase372.RootElement, "routeProvidesPhysicalMassPsiCompatibleBranch") is false &&
    JsonBool(phase372.RootElement, "canFillPhase201WzContract") is false;

using var phase389 = JsonDocument.Parse(File.ReadAllText(Phase389SummaryPath));
bool phase389PrecursorPassed =
    JsonBool(phase389.RootElement, "vo7MixedLinearizationGaugeCompatibilityIdentityProbePassed") is true &&
    JsonBool(phase389.RootElement, "discreteGaugeCompatibilityIdentityExact") is true &&
    JsonBool(phase389.RootElement, "wardEigenBoundSharp") is false &&
    JsonBool(phase389.RootElement, "persistedModeEigenResidualsLarge") is true &&
    JsonBool(phase389.RootElement, "canFillPhase201WzContract") is false;

// ---------------------------------------------------------------------------
// Shared geometry and representation data (Phase372/389 conventions).
// ---------------------------------------------------------------------------

using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");

var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int spinorDim = spinorSpec.SpinorComponents;
int dofsPerCell = spinorDim * DimG;
int vertexCount = mesh.VertexCount;
int totalDof = vertexCount * dofsPerCell;
int edgeCount = mesh.EdgeCount;

double[] meshVolumeMassPsiWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerCell);
double minMeshVolumeWeight = double.PositiveInfinity;
double maxMeshVolumeWeight = 0.0;
for (int index = 0; index < meshVolumeMassPsiWeights.Length; index += 2)
{
    minMeshVolumeWeight = Math.Min(minMeshVolumeWeight, meshVolumeMassPsiWeights[index]);
    maxMeshVolumeWeight = Math.Max(maxMeshVolumeWeight, meshVolumeMassPsiWeights[index]);
}
bool meshVolumeWeightsPositive = minMeshVolumeWeight > 0.0;

var vertexDegrees = new int[vertexCount];
for (int edge = 0; edge < edgeCount; edge++)
{
    vertexDegrees[mesh.Edges[edge][0]]++;
    vertexDegrees[mesh.Edges[edge][1]]++;
}
int isolatedVertexCount = vertexDegrees.Count(degree => degree == 0);

var fermionModesByBackground = LoadFermionModesByBackground(FermionDir);

// ---------------------------------------------------------------------------
// Per-background rebuild.
// ---------------------------------------------------------------------------

var backgroundRecords = fermionModesByBackground
    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
    .Select(pair => RebuildBackground(pair.Key, pair.Value))
    .ToArray();

int backgroundCount = backgroundRecords.Length;
bool expectedCoveragePresent =
    backgroundCount == ExpectedBackgroundCount &&
    backgroundRecords.All(record =>
        record.PersistedModeCount == ExpectedModesPerBackground &&
        record.IdentityBranch.SelectedModeCount == RebuiltModesPerBranch &&
        record.MeshVolumeBranch.SelectedModeCount == RebuiltModesPerBranch);
bool identityBranchConverged = backgroundRecords.All(record => record.IdentityBranch.Converged);
bool meshVolumeBranchConverged = backgroundRecords.All(record => record.MeshVolumeBranch.Converged);
bool mPsiWeightCommutesWithGaugeAction = backgroundRecords.All(record => record.MPsiGaugeCommutatorResidual <= MCommutatorTolerance);
bool persistedPhase12ModeBranchUnconverged = backgroundRecords.All(record => record.PersistedBranchUnconverged);
bool identityBranchWardSharplyPassed = backgroundRecords.All(record => record.IdentityBranch.WardSharpPassedCount == record.IdentityBranch.WardRowCount);
bool meshVolumeBranchWardSharplyPassed = backgroundRecords.All(record => record.MeshVolumeBranch.WardSharpPassedCount == record.MeshVolumeBranch.WardRowCount);
bool wardZeroCurrentSharplyTested = identityBranchWardSharplyPassed && meshVolumeBranchWardSharplyPassed;
bool mPsiCompatibleGeneralizedControlBranchMaterialized =
    meshVolumeWeightsPositive &&
    meshVolumeBranchConverged &&
    mPsiWeightCommutesWithGaugeAction &&
    meshVolumeBranchWardSharplyPassed;
bool convergedControlBranchModesRebuilt =
    expectedCoveragePresent &&
    identityBranchConverged &&
    meshVolumeBranchConverged &&
    wardZeroCurrentSharplyTested;

double maxIdentityEigenResidual = backgroundRecords.Max(record => record.IdentityBranch.MaxEigenResidual);
double maxMeshVolumeEigenResidual = backgroundRecords.Max(record => record.MeshVolumeBranch.MaxEigenResidual);
double maxIdentityOrthonormalityResidual = backgroundRecords.Max(record => record.IdentityBranch.MaxOrthonormalityResidual);
double maxMeshVolumeOrthonormalityResidual = backgroundRecords.Max(record => record.MeshVolumeBranch.MaxOrthonormalityResidual);
double maxIdentityWardCurrent = backgroundRecords.Max(record => record.IdentityBranch.MaxWardCurrentMagnitude);
double maxMeshVolumeWardCurrent = backgroundRecords.Max(record => record.MeshVolumeBranch.MaxWardCurrentMagnitude);
double maxMPsiGaugeCommutatorResidual = backgroundRecords.Max(record => record.MPsiGaugeCommutatorResidual);
double minPersistedModeRelativeResidual = backgroundRecords.Min(record => record.MinPersistedModeRelativeResidual);
double maxPersistedModeRelativeResidual = backgroundRecords.Max(record => record.MaxPersistedModeRelativeResidual);
double maxPersistedModeBestOverlap = backgroundRecords.Max(record => record.MaxPersistedModeBestOverlap);
int totalWardRowCount = backgroundRecords.Sum(record => record.IdentityBranch.WardRowCount + record.MeshVolumeBranch.WardRowCount);
int totalGaugeDirectionCount = backgroundRecords.Sum(record => record.GaugeDirectionCount);

// ---------------------------------------------------------------------------
// Fail-closed VO-7 and contract boundary (unchanged by this rebuild).
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
const string ApplicationSubjectKind = "converged-control-branch-fermion-mode-rebuild-sharp-ward-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    vertexCount.ToString(),
    edgeCount.ToString(),
    DimG.ToString(),
    spinorDim.ToString(),
    RebuiltModesPerBranch.ToString(),
    "selection: kernel-excluded twelve smallest |lambda| nonzero converged eigenpairs per branch",
    "B = M^{-1/2} D M^{-1/2}",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool convergedControlBranchFermionModeRebuildPassed =
    phase372PrecursorPassed &&
    phase389PrecursorPassed &&
    convergedControlBranchModesRebuilt &&
    mPsiCompatibleGeneralizedControlBranchMaterialized &&
    persistedPhase12ModeBranchUnconverged &&
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

string terminalStatus = convergedControlBranchFermionModeRebuildPassed
    ? "converged-control-branch-modes-rebuilt-sharp-ward-verified-vo7-still-incomplete"
    : "converged-control-branch-fermion-mode-rebuild-blocked";
string decision = convergedControlBranchFermionModeRebuildPassed
    ? "Converged fermion modes were rebuilt on both persisted Phase12 backgrounds for both the identity-weight branch (direct eigensolve of the persisted base Dirac matrix) and the mesh-volume M_psi branch (symmetrized generalized pencil B = M^{-1/2} D M^{-1/2}), with eigen-residuals and orthonormality at solver precision. The structural identity [M_psi, X_hat] = 0 holds exactly, so the Phase389 discrete gauge-compatibility identity conjugates exactly onto the M_psi-compatible branch, and the pure-gauge Ward zero-current statement now holds SHARPLY on both converged branches across every gauge direction and rebuilt mode. The persisted Phase12 mode branch is confirmed unconverged (relative residuals far above threshold), which resolves the Phase389 sharpness caveat and closes the Phase372 actionable obstruction at the control-branch level. This remains a toy control-branch result: the mesh, backgrounds, and M_psi are study-defined, so no physical M_psi-compatible GU branch, completed fermionic action, physical mixed blocks, physical Hessian, observed namespace map, or Phase201/Phase256 contract field is provided. VO-7 remains incomplete and no physical W/Z/H promotion is possible from this route."
    : "Do not use the rebuilt control-branch modes until the Phase372/Phase389 precursors, eigen-residual and orthonormality checks, M_psi gauge-commutation, sharp Ward tests, and persisted-branch unconvergence characterization all pass.";

var result = new
{
    phaseId = "phase390-converged-control-branch-fermion-mode-rebuild",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    convergedControlBranchFermionModeRebuildPassed,
    phase372PrecursorPassed,
    phase389PrecursorPassed,
    convergedControlBranchModesRebuilt,
    convergedControlBranchModesRebuiltIsVo7BuildingBlock = true,
    convergedControlBranchModesRebuiltCompletesVo7 = false,
    mPsiCompatibleGeneralizedControlBranchMaterialized,
    mPsiWeightCommutesWithGaugeAction,
    persistedPhase12ModeBranchUnconverged,
    wardZeroCurrentSharplyTested,
    identityBranchWardSharplyPassed,
    meshVolumeBranchWardSharplyPassed,
    identityBranchConverged,
    meshVolumeBranchConverged,
    meshVolumeWeightsPositive,
    minMeshVolumeWeight,
    maxMeshVolumeWeight,
    isolatedVertexCount,
    identityBranchDefinition = "D psi = lambda psi via complex Hermitian Jacobi on the persisted explicit base Dirac matrix",
    meshVolumeBranchDefinition = "D v = lambda M_psi v via B = M^{-1/2} D M^{-1/2}, v = M^{-1/2} w, M_psi from MassPsiWeightsBuilder.BuildFromMesh",
    modeSelectionRule = "exclude the near-zero structural kernel (isolated-vertex topology), then take the twelve smallest |lambda| nonzero converged eigenpairs per branch; no physical boson target, observed mass, or external calibration consulted",
    wardIdentityDefinition = "|Re<psi, [D, X_hat] psi>| <= safety * 2 ||D psi - lambda (M) psi|| ||X_hat psi|| + floor, valid on the M_psi branch because [M_psi, X_hat] = 0",
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    expectedCoveragePresent,
    backgroundCount,
    vertexCount,
    edgeCount,
    spinorDim,
    dimG = DimG,
    totalDof,
    rebuiltModesPerBranch = RebuiltModesPerBranch,
    totalGaugeDirectionCount,
    totalWardRowCount,
    jacobiOffDiagonalTolerance = JacobiOffDiagonalTolerance,
    eigenResidualTolerance = EigenResidualTolerance,
    orthonormalityTolerance = OrthonormalityTolerance,
    mCommutatorTolerance = MCommutatorTolerance,
    wardEigenBoundSafetyFactor = WardEigenBoundSafetyFactor,
    wardAbsoluteFloor = WardAbsoluteFloor,
    persistedUnconvergedThreshold = PersistedUnconvergedThreshold,
    maxIdentityEigenResidual,
    maxMeshVolumeEigenResidual,
    maxIdentityOrthonormalityResidual,
    maxMeshVolumeOrthonormalityResidual,
    maxIdentityWardCurrent,
    maxMeshVolumeWardCurrent,
    maxMPsiGaugeCommutatorResidual,
    minPersistedModeRelativeResidual,
    maxPersistedModeRelativeResidual,
    maxPersistedModeBestOverlap,
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
        "toy control-branch rebuild only: mesh, backgrounds, and M_psi remain study-defined",
        "not a physical M_psi-compatible GU branch",
        "not a completed GU fermionic action",
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
        phase389SummaryPath = Phase389SummaryPath,
        phase12Root = Phase12Root,
        fermionDir = FermionDir,
        backgroundStateDir = BackgroundStateDir,
        spinorRepresentationPath = SpinorRepresentationPath,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "converged_control_branch_fermion_mode_rebuild.json");
string summaryPath = Path.Combine(outputDir, "converged_control_branch_fermion_mode_rebuild_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    summaryPath,
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.generatedAt,
        terminalStatus,
        convergedControlBranchFermionModeRebuildPassed,
        phase372PrecursorPassed,
        phase389PrecursorPassed,
        convergedControlBranchModesRebuilt,
        result.convergedControlBranchModesRebuiltIsVo7BuildingBlock,
        result.convergedControlBranchModesRebuiltCompletesVo7,
        mPsiCompatibleGeneralizedControlBranchMaterialized,
        mPsiWeightCommutesWithGaugeAction,
        persistedPhase12ModeBranchUnconverged,
        wardZeroCurrentSharplyTested,
        identityBranchWardSharplyPassed,
        meshVolumeBranchWardSharplyPassed,
        identityBranchConverged,
        meshVolumeBranchConverged,
        meshVolumeWeightsPositive,
        isolatedVertexCount,
        result.identityBranchDefinition,
        result.meshVolumeBranchDefinition,
        result.modeSelectionRule,
        result.wardIdentityDefinition,
        result.applicationSubjectKind,
        result.targetBlindConstruction,
        physicalTargetsConsultedForConstruction,
        targetBlindConstructionHash,
        expectedCoveragePresent,
        backgroundCount,
        rebuiltModesPerBranch = RebuiltModesPerBranch,
        totalGaugeDirectionCount,
        totalWardRowCount,
        maxIdentityEigenResidual,
        maxMeshVolumeEigenResidual,
        maxIdentityOrthonormalityResidual,
        maxMeshVolumeOrthonormalityResidual,
        maxIdentityWardCurrent,
        maxMeshVolumeWardCurrent,
        maxMPsiGaugeCommutatorResidual,
        minPersistedModeRelativeResidual,
        maxPersistedModeRelativeResidual,
        maxPersistedModeBestOverlap,
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
Console.WriteLine($"convergedControlBranchFermionModeRebuildPassed={convergedControlBranchFermionModeRebuildPassed}");
Console.WriteLine($"phase372PrecursorPassed={phase372PrecursorPassed}");
Console.WriteLine($"phase389PrecursorPassed={phase389PrecursorPassed}");
Console.WriteLine($"convergedControlBranchModesRebuilt={convergedControlBranchModesRebuilt}");
Console.WriteLine($"mPsiCompatibleGeneralizedControlBranchMaterialized={mPsiCompatibleGeneralizedControlBranchMaterialized}");
Console.WriteLine($"mPsiWeightCommutesWithGaugeAction={mPsiWeightCommutesWithGaugeAction}");
Console.WriteLine($"persistedPhase12ModeBranchUnconverged={persistedPhase12ModeBranchUnconverged}");
Console.WriteLine($"wardZeroCurrentSharplyTested={wardZeroCurrentSharplyTested}");
Console.WriteLine($"identityBranchConverged={identityBranchConverged}");
Console.WriteLine($"meshVolumeBranchConverged={meshVolumeBranchConverged}");
Console.WriteLine($"isolatedVertexCount={isolatedVertexCount}");
Console.WriteLine($"maxIdentityEigenResidual={maxIdentityEigenResidual:R}");
Console.WriteLine($"maxMeshVolumeEigenResidual={maxMeshVolumeEigenResidual:R}");
Console.WriteLine($"maxIdentityWardCurrent={maxIdentityWardCurrent:R}");
Console.WriteLine($"maxMeshVolumeWardCurrent={maxMeshVolumeWardCurrent:R}");
Console.WriteLine($"maxMPsiGaugeCommutatorResidual={maxMPsiGaugeCommutatorResidual:R}");
Console.WriteLine($"minPersistedModeRelativeResidual={minPersistedModeRelativeResidual:R}");
Console.WriteLine($"maxPersistedModeBestOverlap={maxPersistedModeBestOverlap:R}");
Console.WriteLine($"routeProvidesPhysicalMassPsiCompatibleBranch={routeProvidesPhysicalMassPsiCompatibleBranch}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Background rebuild implementation.
// ---------------------------------------------------------------------------

BackgroundRebuildRecord RebuildBackground(string backgroundId, IReadOnlyList<PersistedModeSnapshot> persistedModes)
{
    if (persistedModes.Count != ExpectedModesPerBackground)
        throw new InvalidDataException($"Expected {ExpectedModesPerBackground} fermion modes for {backgroundId}, found {persistedModes.Count}.");

    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    string matrixPath = Path.Combine(FermionDir, matrixRef);
    var (dRe, dIm) = LoadFlatInterleavedMatrix(matrixPath, totalDof);

    // Identity-weight branch: direct eigensolve of D.
    var identityBranch = SolveBranch(
        "identity-weight",
        dRe, dIm,
        massWeights: null);

    // Mesh-volume M_psi branch: B = M^{-1/2} D M^{-1/2}, v = M^{-1/2} w.
    var meshVolumeBranch = SolveBranch(
        "mesh-volume-m-psi",
        dRe, dIm,
        massWeights: meshVolumeMassPsiWeights);

    // Structural [M_psi, X_hat] = 0 check with a generic dense gauge parameter.
    double mPsiCommutatorResidual = ComputeMPsiGaugeCommutatorResidual();

    // Persisted-branch characterization against the converged identity basis.
    var persistedRows = persistedModes
        .OrderBy(mode => mode.ModeIndex)
        .Select(mode =>
        {
            var dPsi = ApplyComplexMatrix(dRe, dIm, mode.EigenvectorCoefficients);
            double norm = Math.Sqrt(ComplexInnerProduct(mode.EigenvectorCoefficients, mode.EigenvectorCoefficients).Real);
            double lambda = ComplexInnerProduct(mode.EigenvectorCoefficients, dPsi).Real / (norm * norm);
            double residual2 = 0.0;
            for (int index = 0; index < dPsi.Length; index += 2)
            {
                double rRe = dPsi[index] - lambda * mode.EigenvectorCoefficients[index];
                double rIm = dPsi[index + 1] - lambda * mode.EigenvectorCoefficients[index + 1];
                residual2 += rRe * rRe + rIm * rIm;
            }
            double relativeResidual = Math.Sqrt(residual2) / Math.Max(norm, 1e-30);
            double bestOverlap = 0.0;
            int bestIndex = -1;
            for (int k = 0; k < identityBranch.AllEigenvalues.Length; k++)
            {
                var (re, im) = ComplexInnerProduct(identityBranch.AllEigenvectors[k], mode.EigenvectorCoefficients);
                double overlap = Math.Sqrt(re * re + im * im) / Math.Max(norm, 1e-30);
                if (overlap > bestOverlap)
                {
                    bestOverlap = overlap;
                    bestIndex = k;
                }
            }
            return new PersistedModeRow
            {
                ModeId = mode.ModeId,
                ModeIndex = mode.ModeIndex,
                PersistedEigenvalueRe = mode.PersistedEigenvalueRe,
                PersistedResidualNorm = mode.PersistedResidualNorm,
                RayleighEigenvalue = lambda,
                RelativeEigenResidual = relativeResidual,
                Unconverged = relativeResidual > PersistedUnconvergedThreshold,
                BestOverlapWithConvergedBasis = bestOverlap,
                BestOverlapConvergedEigenvalue = bestIndex >= 0 ? identityBranch.AllEigenvalues[bestIndex] : double.NaN,
            };
        })
        .ToArray();

    // Sharp pure-gauge Ward test on both converged branches.
    string omegaPath = Path.Combine(BackgroundStateDir, $"{backgroundId}_omega.json");
    var gaugeDirections = BuildGaugeDirections();
    var identityWard = RunWardProbe(dRe, dIm, identityBranch, gaugeDirections, massWeights: null);
    var meshVolumeWard = RunWardProbe(dRe, dIm, meshVolumeBranch, gaugeDirections, massWeights: meshVolumeMassPsiWeights);

    return new BackgroundRebuildRecord
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMetadataPath = metadataPath,
        BaseDiracMatrixPath = matrixPath,
        PersistedOmegaPath = omegaPath,
        PersistedModeCount = persistedRows.Length,
        PersistedBranchUnconverged = persistedRows.All(row => row.Unconverged),
        MinPersistedModeRelativeResidual = persistedRows.Min(row => row.RelativeEigenResidual),
        MaxPersistedModeRelativeResidual = persistedRows.Max(row => row.RelativeEigenResidual),
        MaxPersistedModeBestOverlap = persistedRows.Max(row => row.BestOverlapWithConvergedBasis),
        MPsiGaugeCommutatorResidual = mPsiCommutatorResidual,
        GaugeDirectionCount = gaugeDirections.Count,
        IdentityBranch = identityBranch.ToRecord(identityWard),
        MeshVolumeBranch = meshVolumeBranch.ToRecord(meshVolumeWard),
        PersistedModeRows = persistedRows,
    };
}

BranchSolveResult SolveBranch(string branchId, double[,] dRe, double[,] dIm, double[]? massWeights)
{
    // Form the matrix to eigensolve: D itself, or B = M^{-1/2} D M^{-1/2}.
    var aRe = new double[totalDof, totalDof];
    var aIm = new double[totalDof, totalDof];
    double[]? invSqrtM = null;
    if (massWeights is not null)
    {
        invSqrtM = new double[totalDof];
        for (int index = 0; index < totalDof; index++)
            invSqrtM[index] = 1.0 / Math.Sqrt(massWeights[2 * index]);
    }
    for (int row = 0; row < totalDof; row++)
        for (int col = 0; col < totalDof; col++)
        {
            double scale = invSqrtM is null ? 1.0 : invSqrtM[row] * invSqrtM[col];
            aRe[row, col] = scale * dRe[row, col];
            aIm[row, col] = scale * dIm[row, col];
        }

    var (eigenvalues, eigenvectorsRe, eigenvectorsIm, sweeps, offDiagonal) = JacobiHermitian(aRe, aIm);

    // Reconstruct branch modes as interleaved complex vectors; for the
    // generalized branch v = M^{-1/2} w (then M-orthonormal).
    var allModes = new double[totalDof][];
    for (int k = 0; k < totalDof; k++)
    {
        var mode = new double[2 * totalDof];
        for (int index = 0; index < totalDof; index++)
        {
            double scale = invSqrtM is null ? 1.0 : invSqrtM[index];
            mode[2 * index] = scale * eigenvectorsRe[index, k];
            mode[2 * index + 1] = scale * eigenvectorsIm[index, k];
        }
        allModes[k] = mode;
    }

    // Kernel accounting: the toy mesh has isolated vertices whose Dirac rows
    // are identically zero, producing an exact structural kernel. Selecting
    // those zero modes would make the sharp Ward test vacuous (D psi = 0
    // exactly), so the target-blind selection rule is: count and exclude the
    // near-zero kernel, then take the smallest-|lambda| NONZERO eigenpairs.
    double maxAbsEigenvalue = 0.0;
    for (int k = 0; k < totalDof; k++)
        maxAbsEigenvalue = Math.Max(maxAbsEigenvalue, Math.Abs(eigenvalues[k]));
    double zeroThreshold = 1e-10 * Math.Max(maxAbsEigenvalue, 1e-30);
    int zeroModeCount = eigenvalues.Count(value => Math.Abs(value) <= zeroThreshold);

    // Verify residuals and (M-)orthonormality against the ORIGINAL D matrix.
    double maxEigenResidual = 0.0;
    var selectedIndices = Enumerable.Range(0, totalDof)
        .Where(k => Math.Abs(eigenvalues[k]) > zeroThreshold)
        .OrderBy(k => Math.Abs(eigenvalues[k]))
        .ThenBy(k => eigenvalues[k])
        .Take(RebuiltModesPerBranch)
        .ToArray();
    foreach (int k in selectedIndices)
    {
        var dMode = ApplyComplexMatrix(dRe, dIm, allModes[k]);
        double residual2 = 0.0;
        double norm2 = 0.0;
        for (int index = 0; index < totalDof; index++)
        {
            double weight = massWeights is null ? 1.0 : massWeights[2 * index];
            double rRe = dMode[2 * index] - eigenvalues[k] * weight * allModes[k][2 * index];
            double rIm = dMode[2 * index + 1] - eigenvalues[k] * weight * allModes[k][2 * index + 1];
            residual2 += rRe * rRe + rIm * rIm;
            norm2 += allModes[k][2 * index] * allModes[k][2 * index] + allModes[k][2 * index + 1] * allModes[k][2 * index + 1];
        }
        maxEigenResidual = Math.Max(maxEigenResidual, Math.Sqrt(residual2) / Math.Max(Math.Sqrt(norm2), 1e-30));
    }

    double maxOrthonormalityResidual = 0.0;
    for (int i = 0; i < selectedIndices.Length; i++)
        for (int j = i; j < selectedIndices.Length; j++)
        {
            var (re, im) = WeightedComplexInnerProduct(allModes[selectedIndices[i]], allModes[selectedIndices[j]], massWeights);
            double expected = i == j ? 1.0 : 0.0;
            double residual = Math.Sqrt((re - expected) * (re - expected) + im * im);
            maxOrthonormalityResidual = Math.Max(maxOrthonormalityResidual, residual);
        }

    return new BranchSolveResult
    {
        BranchId = branchId,
        AllEigenvalues = eigenvalues,
        AllEigenvectors = allModes,
        SelectedIndices = selectedIndices,
        JacobiSweeps = sweeps,
        FinalOffDiagonalNorm = offDiagonal,
        ZeroModeCount = zeroModeCount,
        ZeroModeThreshold = zeroThreshold,
        MaxEigenResidual = maxEigenResidual,
        MaxOrthonormalityResidual = maxOrthonormalityResidual,
        MassWeights = massWeights,
    };
}

WardProbeResult RunWardProbe(
    double[,] dRe,
    double[,] dIm,
    BranchSolveResult branch,
    IReadOnlyList<GaugeDirection> directions,
    double[]? massWeights)
{
    int wardRowCount = 0;
    int wardSharpPassedCount = 0;
    double maxWardCurrent = 0.0;
    double maxWardBound = 0.0;
    foreach (var direction in directions)
    {
        foreach (int k in branch.SelectedIndices)
        {
            var psi = branch.AllEigenvectors[k];
            double lambda = branch.AllEigenvalues[k];
            var dPsi = ApplyComplexMatrix(dRe, dIm, psi);
            var xPsi = ApplyGaugeParameter(direction.X, psi);

            // Ward current Re<psi, [D, X_hat] psi> = Re(<D psi, X psi> - <psi, X D psi>)
            // computed in the UNWEIGHTED pairing; valid on the M_psi branch
            // because [M_psi, X_hat] = 0 (see derivation in STUDY.md).
            var dPsiXPsi = ComplexInnerProduct(dPsi, xPsi);
            var xDPsi = ApplyGaugeParameter(direction.X, dPsi);
            var psiXDPsi = ComplexInnerProduct(psi, xDPsi);
            double wardCurrent = dPsiXPsi.Real - psiXDPsi.Real;

            // Residual r = D psi - lambda (M) psi in the unweighted norm.
            double residual2 = 0.0;
            double xPsiNorm2 = 0.0;
            for (int index = 0; index < totalDof; index++)
            {
                double weight = massWeights is null ? 1.0 : massWeights[2 * index];
                double rRe = dPsi[2 * index] - lambda * weight * psi[2 * index];
                double rIm = dPsi[2 * index + 1] - lambda * weight * psi[2 * index + 1];
                residual2 += rRe * rRe + rIm * rIm;
                xPsiNorm2 += xPsi[2 * index] * xPsi[2 * index] + xPsi[2 * index + 1] * xPsi[2 * index + 1];
            }
            double bound = WardEigenBoundSafetyFactor * 2.0 * Math.Sqrt(residual2) * Math.Sqrt(xPsiNorm2) + WardAbsoluteFloor;

            wardRowCount++;
            if (Math.Abs(wardCurrent) <= bound)
                wardSharpPassedCount++;
            maxWardCurrent = Math.Max(maxWardCurrent, Math.Abs(wardCurrent));
            maxWardBound = Math.Max(maxWardBound, bound);
        }
    }

    return new WardProbeResult
    {
        WardRowCount = wardRowCount,
        WardSharpPassedCount = wardSharpPassedCount,
        MaxWardCurrentMagnitude = maxWardCurrent,
        MaxWardBound = maxWardBound,
    };
}

List<GaugeDirection> BuildGaugeDirections()
{
    var directions = new List<GaugeDirection>();
    for (int vertex = 0; vertex < vertexCount; vertex++)
        for (int generator = 0; generator < DimG; generator++)
        {
            var x = new double[vertexCount][];
            for (int v = 0; v < vertexCount; v++)
            {
                x[v] = new double[DimG];
                if (v == vertex)
                    x[v][generator] = 1.0;
            }
            directions.Add(new GaugeDirection { DirectionId = $"vertex-{vertex}-generator-{generator}", X = x });
        }
    for (int generator = 0; generator < DimG; generator++)
    {
        var x = new double[vertexCount][];
        for (int v = 0; v < vertexCount; v++)
        {
            x[v] = new double[DimG];
            x[v][generator] = 1.0;
        }
        directions.Add(new GaugeDirection { DirectionId = $"global-generator-{generator}", X = x });
    }
    return directions;
}

double ComputeMPsiGaugeCommutatorResidual()
{
    // [M_psi, X_hat] entry-wise: M is diagonal with one scalar weight per
    // vertex block; X_hat mixes gauge components only WITHIN a vertex block.
    // The commutator entry between dofs (v, g, s) and (v, g', s) is
    // (m_{v,g,s} - m_{v,g',s}) * rho(X_v)[g, g'], so the residual is the
    // maximum within-vertex weight spread times the gauge action.
    double maxResidual = 0.0;
    for (int vertex = 0; vertex < vertexCount; vertex++)
    {
        double first = meshVolumeMassPsiWeights[2 * vertex * dofsPerCell];
        for (int k = 0; k < dofsPerCell; k++)
        {
            double weight = meshVolumeMassPsiWeights[2 * (vertex * dofsPerCell + k)];
            maxResidual = Math.Max(maxResidual, Math.Abs(weight - first));
        }
    }
    return maxResidual;
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

(double[] Eigenvalues, double[,] VecRe, double[,] VecIm, int Sweeps, double OffDiagonal) JacobiHermitian(double[,] inRe, double[,] inIm)
{
    int n = inRe.GetLength(0);
    var aRe = (double[,])inRe.Clone();
    var aIm = (double[,])inIm.Clone();
    var vRe = new double[n, n];
    var vIm = new double[n, n];
    for (int i = 0; i < n; i++)
        vRe[i, i] = 1.0;

    double matrixNorm = 0.0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
            matrixNorm += aRe[i, j] * aRe[i, j] + aIm[i, j] * aIm[i, j];
    matrixNorm = Math.Sqrt(matrixNorm);
    double threshold = JacobiOffDiagonalTolerance * Math.Max(matrixNorm, 1e-30);

    int sweeps = 0;
    double offDiagonal = OffDiagonalNorm(aRe, aIm, n);
    while (offDiagonal > threshold && sweeps < JacobiMaxSweeps)
    {
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double gRe = aRe[p, q];
                double gIm = aIm[p, q];
                double gAbs = Math.Sqrt(gRe * gRe + gIm * gIm);
                if (gAbs <= threshold / n)
                    continue;

                // Phase that makes the off-diagonal real: e^{-i phi}.
                double phaseRe = gRe / gAbs;
                double phaseIm = gIm / gAbs;

                double alpha = aRe[p, p];
                double beta = aRe[q, q];
                double theta = 0.5 * Math.Atan2(2.0 * gAbs, alpha - beta);
                double c = Math.Cos(theta);
                double s = Math.Sin(theta);

                // Unitary U on columns (p, q):
                //   U[p,p] = c            U[p,q] = -s
                //   U[q,p] = s e^{-i phi} U[q,q] = c e^{-i phi}
                double upqRe = -s;
                double uqpRe = s * phaseRe;
                double uqpIm = -s * phaseIm;
                double uqqRe = c * phaseRe;
                double uqqIm = -c * phaseIm;

                // Column update A <- A U, V <- V U.
                for (int k = 0; k < n; k++)
                {
                    double apRe = aRe[k, p];
                    double apIm = aIm[k, p];
                    double aqRe = aRe[k, q];
                    double aqIm = aIm[k, q];
                    aRe[k, p] = c * apRe + uqpRe * aqRe - uqpIm * aqIm;
                    aIm[k, p] = c * apIm + uqpRe * aqIm + uqpIm * aqRe;
                    aRe[k, q] = upqRe * apRe + uqqRe * aqRe - uqqIm * aqIm;
                    aIm[k, q] = upqRe * apIm + uqqRe * aqIm + uqqIm * aqRe;

                    double vpRe = vRe[k, p];
                    double vpIm = vIm[k, p];
                    double vqRe = vRe[k, q];
                    double vqIm = vIm[k, q];
                    vRe[k, p] = c * vpRe + uqpRe * vqRe - uqpIm * vqIm;
                    vIm[k, p] = c * vpIm + uqpRe * vqIm + uqpIm * vqRe;
                    vRe[k, q] = upqRe * vpRe + uqqRe * vqRe - uqqIm * vqIm;
                    vIm[k, q] = upqRe * vpIm + uqqRe * vqIm + uqqIm * vqRe;
                }

                // Row update A <- U^dagger A.
                for (int k = 0; k < n; k++)
                {
                    double apRe = aRe[p, k];
                    double apIm = aIm[p, k];
                    double aqRe = aRe[q, k];
                    double aqIm = aIm[q, k];
                    aRe[p, k] = c * apRe + uqpRe * aqRe + uqpIm * aqIm;
                    aIm[p, k] = c * apIm + uqpRe * aqIm - uqpIm * aqRe;
                    aRe[q, k] = upqRe * apRe + uqqRe * aqRe + uqqIm * aqIm;
                    aIm[q, k] = upqRe * apIm + uqqRe * aqIm - uqqIm * aqRe;
                }
            }

        sweeps++;
        offDiagonal = OffDiagonalNorm(aRe, aIm, n);
    }

    var eigenvalues = new double[n];
    for (int i = 0; i < n; i++)
        eigenvalues[i] = aRe[i, i];
    return (eigenvalues, vRe, vIm, sweeps, offDiagonal);
}

static double OffDiagonalNorm(double[,] aRe, double[,] aIm, int n)
{
    double sum = 0.0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            if (i == j)
                continue;
            sum += aRe[i, j] * aRe[i, j] + aIm[i, j] * aIm[i, j];
        }
    return Math.Sqrt(sum);
}

static (double Real, double Imaginary) WeightedComplexInnerProduct(double[] left, double[] right, double[]? massWeights)
{
    double real = 0.0;
    double imaginary = 0.0;
    for (int index = 0; index < left.Length; index += 2)
    {
        double weight = massWeights is null ? 1.0 : massWeights[index];
        double leftRe = left[index];
        double leftIm = left[index + 1];
        double rightRe = right[index];
        double rightIm = right[index + 1];
        real += weight * (leftRe * rightRe + leftIm * rightIm);
        imaginary += weight * (leftRe * rightIm - leftIm * rightRe);
    }
    return (real, imaginary);
}

static double[,] AdjointRho(double[] coefficients)
{
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

static double LeviCivita3(int a, int b, int c)
{
    if (a == b || b == c || a == c) return 0.0;
    if ((a == 0 && b == 1 && c == 2) ||
        (a == 1 && b == 2 && c == 0) ||
        (a == 2 && b == 0 && c == 1)) return 1.0;
    return -1.0;
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

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? throw new InvalidDataException($"{propertyName} must not be null.")
        : throw new InvalidDataException($"{propertyName} must be a string.");

static IReadOnlyDictionary<string, IReadOnlyList<PersistedModeSnapshot>> LoadFermionModesByBackground(string fermionDir)
{
    var result = new Dictionary<string, IReadOnlyList<PersistedModeSnapshot>>(StringComparer.Ordinal);
    foreach (string path in Directory.GetFiles(fermionDir, "fermion_modes_*.json").OrderBy(path => path, StringComparer.Ordinal))
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        string backgroundId = RequiredString(doc.RootElement, "fermionBackgroundId");
        var modes = doc.RootElement
            .GetProperty("modes")
            .EnumerateArray()
            .Select(mode => new PersistedModeSnapshot
            {
                ModeId = RequiredString(mode, "modeId"),
                ModeIndex = mode.GetProperty("modeIndex").GetInt32(),
                PersistedEigenvalueRe = mode.TryGetProperty("eigenvalueRe", out var ev) ? ev.GetDouble() : double.NaN,
                PersistedResidualNorm = mode.TryGetProperty("residualNorm", out var rn) ? rn.GetDouble() : double.NaN,
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

public sealed class PersistedModeSnapshot
{
    public required string ModeId { get; init; }
    public required int ModeIndex { get; init; }
    public required double PersistedEigenvalueRe { get; init; }
    public required double PersistedResidualNorm { get; init; }
    public required double[] EigenvectorCoefficients { get; init; }
}

public sealed class GaugeDirection
{
    public required string DirectionId { get; init; }
    public required double[][] X { get; init; }
}

public sealed class BranchSolveResult
{
    public required string BranchId { get; init; }
    public required double[] AllEigenvalues { get; init; }
    public required double[][] AllEigenvectors { get; init; }
    public required int[] SelectedIndices { get; init; }
    public required int JacobiSweeps { get; init; }
    public required double FinalOffDiagonalNorm { get; init; }
    public required int ZeroModeCount { get; init; }
    public required double ZeroModeThreshold { get; init; }
    public required double MaxEigenResidual { get; init; }
    public required double MaxOrthonormalityResidual { get; init; }
    public required double[]? MassWeights { get; init; }

    public bool Converged =>
        MaxEigenResidual <= 1e-9 &&
        MaxOrthonormalityResidual <= 1e-10;

    public BranchRecord ToRecord(WardProbeResult ward) => new()
    {
        BranchId = BranchId,
        JacobiSweeps = JacobiSweeps,
        FinalOffDiagonalNorm = FinalOffDiagonalNorm,
        ZeroModeCount = ZeroModeCount,
        ZeroModeThreshold = ZeroModeThreshold,
        SelectedModeCount = SelectedIndices.Length,
        SelectedEigenvalues = SelectedIndices.Select(k => AllEigenvalues[k]).ToArray(),
        MaxEigenResidual = MaxEigenResidual,
        MaxOrthonormalityResidual = MaxOrthonormalityResidual,
        Converged = Converged,
        WardRowCount = ward.WardRowCount,
        WardSharpPassedCount = ward.WardSharpPassedCount,
        MaxWardCurrentMagnitude = ward.MaxWardCurrentMagnitude,
        MaxWardBound = ward.MaxWardBound,
    };
}

public sealed class WardProbeResult
{
    public required int WardRowCount { get; init; }
    public required int WardSharpPassedCount { get; init; }
    public required double MaxWardCurrentMagnitude { get; init; }
    public required double MaxWardBound { get; init; }
}

public sealed class BranchRecord
{
    public required string BranchId { get; init; }
    public required int JacobiSweeps { get; init; }
    public required double FinalOffDiagonalNorm { get; init; }
    public required int ZeroModeCount { get; init; }
    public required double ZeroModeThreshold { get; init; }
    public required int SelectedModeCount { get; init; }
    public required double[] SelectedEigenvalues { get; init; }
    public required double MaxEigenResidual { get; init; }
    public required double MaxOrthonormalityResidual { get; init; }
    public required bool Converged { get; init; }
    public required int WardRowCount { get; init; }
    public required int WardSharpPassedCount { get; init; }
    public required double MaxWardCurrentMagnitude { get; init; }
    public required double MaxWardBound { get; init; }
}

public sealed class PersistedModeRow
{
    public required string ModeId { get; init; }
    public required int ModeIndex { get; init; }
    public required double PersistedEigenvalueRe { get; init; }
    public required double PersistedResidualNorm { get; init; }
    public required double RayleighEigenvalue { get; init; }
    public required double RelativeEigenResidual { get; init; }
    public required bool Unconverged { get; init; }
    public required double BestOverlapWithConvergedBasis { get; init; }
    public required double BestOverlapConvergedEigenvalue { get; init; }
}

public sealed class BackgroundRebuildRecord
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMetadataPath { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required string PersistedOmegaPath { get; init; }
    public required int PersistedModeCount { get; init; }
    public required bool PersistedBranchUnconverged { get; init; }
    public required double MinPersistedModeRelativeResidual { get; init; }
    public required double MaxPersistedModeRelativeResidual { get; init; }
    public required double MaxPersistedModeBestOverlap { get; init; }
    public required double MPsiGaugeCommutatorResidual { get; init; }
    public required int GaugeDirectionCount { get; init; }
    public required BranchRecord IdentityBranch { get; init; }
    public required BranchRecord MeshVolumeBranch { get; init; }
    public required IReadOnlyList<PersistedModeRow> PersistedModeRows { get; init; }
}
