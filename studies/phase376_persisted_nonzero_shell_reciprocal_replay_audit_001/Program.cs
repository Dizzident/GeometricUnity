using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase376_persisted_nonzero_shell_reciprocal_replay_audit_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string VariationDir = $"{FermionDir}/couplings/variations";
const string ModeDir = $"{Phase12Root}/spectra/modes";
const string GeometryManifestPath = $"{Phase12Root}/manifest/geometry.json";
const string OmegaMetadataPath = $"{Phase12Root}/background_states/bg-phase12-bg-a-20260315212202_omega.json";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase375SummaryPath = "studies/phase375_weighted_reciprocal_mixed_block_replay_audit_001/output/weighted_reciprocal_mixed_block_replay_audit_summary.json";
const int ExpectedBackgroundCount = 2;
const int ExpectedVariationCount = 24;
const int RequestedModeCount = 53;
const int ExpectedKernelModeCount = 48;
const int ExpectedShellSize = 4;
const int ExpectedSentinelIndex = 52;
const int ExpectedAmbientEdgeCount = 52;
const int ExpectedConnectionVectorLength = 156;
const string ExpectedConnectionCarrierType = "connection-1form";
const double AlgebraTolerance = 1e-11;
const double MatrixParityTolerance = 1e-8;
const double BlockParityTolerance = 1e-8;
const double GeneralizedResidualTolerance = 1e-6;
const double MNormTolerance = 1e-8;
const double MOrthonormalityTolerance = 1e-8;
const double KernelTolerance = 1e-12;
const double NonzeroBlockTolerance = 1e-12;
const double MatrixStructuralZeroTolerance = 1e-15;
int[] expectedIsolatedVertices = [19, 20, 23, 26];
int[] expectedShellIndices = [48, 49, 50, 51];

var options = JsonOptions();
var outputDir = Environment.GetEnvironmentVariable("PHASE376_OUTPUT_DIR") ?? DefaultOutputDir;
var backgroundOutputDir = Path.Combine(outputDir, "backgrounds");
var variationOutputDir = Path.Combine(outputDir, "variations");
Directory.CreateDirectory(backgroundOutputDir);
Directory.CreateDirectory(variationOutputDir);

using var phase375 = JsonDocument.Parse(File.ReadAllText(Phase375SummaryPath));
bool phase375DiscreteReplayPresent =
    JsonBool(phase375.RootElement, "weightedReciprocalMixedBlockReplayAuditPassed") &&
    JsonInt(phase375.RootElement, "backgroundCount") == ExpectedBackgroundCount &&
    JsonInt(phase375.RootElement, "variationCount") == ExpectedVariationCount;
bool phase375NonpromotionalBoundaryVerified =
    FalseFlag(phase375.RootElement, "routeProvidesPhysicalGuBranch") &&
    FalseFlag(phase375.RootElement, "routeProvidesCanonicalPhysicalMassPsi") &&
    FalseFlag(phase375.RootElement, "routeProvidesCompletedFermionicAction") &&
    FalseFlag(phase375.RootElement, "routeProvidesFixedFermionicOperatorBranch") &&
    FalseFlag(phase375.RootElement, "routeProvidesCompletedMixedLinearizationBlocks") &&
    FalseFlag(phase375.RootElement, "routeProvidesMixedLinearizationGaugeCompatibilityIdentities") &&
    FalseFlag(phase375.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") &&
    FalseFlag(phase375.RootElement, "routeProvidesHiggsScalarSourceOperator") &&
    FalseFlag(phase375.RootElement, "routeProvidesGeVUnitNormalization") &&
    FalseFlag(phase375.RootElement, "routeCompletesBosonPredictions") &&
    FalseFlag(phase375.RootElement, "canFillPhase201WzContract") &&
    FalseFlag(phase375.RootElement, "canFillPhase201HiggsContract") &&
    FalseFlag(phase375.RootElement, "canFillPhase256ObservedFieldExtractionContract");

var spinorSpec = JsonSerializer.Deserialize<SpinorRepresentationSpec>(
    File.ReadAllText(SpinorRepresentationPath),
    options) ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var provenance = new ProvenanceMeta
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "phase376-persisted-nonzero-shell-reciprocal-replay-audit",
    Branch = new() { BranchId = "phase376-discrete-nonzero-shell-replay-audit", SchemaVersion = "1.0" },
    Backend = "cpu-reference",
};
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    provenance);
var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int dimG = 3;
int spinorDim = spinorSpec.SpinorComponents;
int dofsPerVertex = spinorDim * dimG;
double[] massPsiWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerVertex);
var isolatedVertices = Enumerable.Range(0, mesh.VertexCount)
    .Where(vertex => !mesh.CellVertices.Any(cell => cell.Contains(vertex)))
    .ToArray();
var expectedZeroComplexDofs = isolatedVertices
    .SelectMany(vertex => Enumerable.Range(vertex * dofsPerVertex, dofsPerVertex))
    .ToArray();
int isolatedFallbackRealWeightCount = isolatedVertices.Sum(vertex =>
    massPsiWeights
        .Skip(vertex * dofsPerVertex * 2)
        .Take(dofsPerVertex * 2)
        .Count(weight => weight == 1.0));
bool isolatedFallbackMpsiWeightUsageVerified =
    isolatedFallbackRealWeightCount == isolatedVertices.Length * dofsPerVertex * 2;
bool meshVolumeMassPsiMaterialized =
    massPsiWeights.Length > 0 &&
    massPsiWeights.All(weight => double.IsFinite(weight) && weight > 0.0);
bool meshVolumeMassPsiNonuniform = massPsiWeights.Distinct().Skip(1).Any();
string persistedGeometryArtifactHash = HashFile(GeometryManifestPath);
string derivedMeshGeometryHash = HashMesh(mesh);
string massPsiWeightHash = HashDoubles(massPsiWeights);
var baselineFingerprint = CaptureFingerprint();
var carrierMetadata = LoadCarrierMetadata();
string targetBlindConstructionHash = HashText(string.Join(
    "\n",
    "phase376-target-blind-nonzero-shell-construction-v1",
    $"derivedMeshGeometryHash={derivedMeshGeometryHash}",
    $"massPsiWeightHash={massPsiWeightHash}",
    $"requestedModeCount={RequestedModeCount}",
    $"kernelTolerance={KernelTolerance:R}",
    "shellRule=lowest nonzero abs(lambda) shell grouped only by max(1e-12,1e-8*abs(lambda_min_nonzero))",
    "projectionRule=Psi_shell^dagger deltaK Psi_shell",
    "targetInputs=none"));
double[] edgeLengths = Enumerable.Range(0, mesh.EdgeCount).Select(edge => ComputeEdgeLength(mesh, edge)).ToArray();
double[][] edgeDirections = Enumerable.Range(0, mesh.EdgeCount).Select(edge => ComputeEdgeDirection(mesh, edge)).ToArray();
int[][] cellsPerEdge = Enumerable.Range(0, mesh.EdgeCount).Select(edge => new[] { mesh.Edges[edge][0], mesh.Edges[edge][1] }).ToArray();

var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());
var backgroundAudits = Directory
    .GetFiles(FermionDir, "dirac_bundle_*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(BuildBackgroundAudit)
    .ToArray();
var shellsByBackground = backgroundAudits.ToDictionary(
    row => row.FermionBackgroundId,
    row => (IReadOnlyList<WeightedModeAudit>)row.WeightedSolve.Shell.Modes,
    StringComparer.Ordinal);
var variationAudits = Directory
    .GetFiles(VariationDir, "variation-*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(BuildVariationAudit)
    .ToArray();

int backgroundCount = backgroundAudits.Length;
int weightedModeCount = backgroundAudits.Sum(row => row.WeightedSolve.ModeCount);
int filteredKernelModeCount = backgroundAudits.Sum(row => row.WeightedSolve.KernelModeCount);
int shellModeCount = backgroundAudits.Sum(row => row.WeightedSolve.Shell.ShellSize);
int sentinelOutsideShellPassedCount = backgroundAudits.Count(row => row.WeightedSolve.Shell.SentinelOutsideShell);
int zeroRowColumnComplexDofPassedBackgroundCount = backgroundAudits.Count(row => row.ZeroRowColumnStructurePassed);
int shellSelectionPassedBackgroundCount = backgroundAudits.Count(row => row.WeightedSolve.Shell.ShellSelectionPassed);
int weightedSolveQualityPassedBackgroundCount = backgroundAudits.Count(row => row.WeightedSolve.AllQualityChecksPassed);
int variationCount = variationAudits.Length;
int variationPassedCount = variationAudits.Count(row => row.VariationPassed);
int analyticPersistedDeltaKParityPassedCount = variationAudits.Count(row => row.AnalyticPersistedDeltaKParityPassed);
int persistedAnalyticBlockParityPassedCount = variationAudits.Count(row => row.PersistedAnalyticBlockParityPassed);
int projectedPairingIdentityPassedCount = variationAudits.Count(row => row.ProjectedPairingIdentityPassed);
int projectedBlockHermiticityPassedCount = variationAudits.Count(row => row.ProjectedBlockHermiticityPassed);
int nonzeroProjectedBlockFrobeniusPassedCount = variationAudits.Count(row => row.NonzeroProjectedBlockFrobeniusPassed);
int invariantParityPassedCount = variationAudits.Count(row => row.InvariantParity.Passed);
int unchangedGeometryHashReplayPassedCount = variationAudits.Count(row => row.GeometryHashUnchangedThroughReplay);
int unchangedMassWeightHashReplayPassedCount = variationAudits.Count(row => row.MassWeightHashUnchangedThroughReplay);
double maxAnalyticPersistedDeltaKRelativeResidual = variationAudits.Max(row => row.AnalyticPersistedDeltaKRelativeResidual);
double maxProjectedPairingIdentityRelativeResidual = variationAudits.Max(row => row.MaxProjectedPairingIdentityRelativeResidual);
double maxPersistedAnalyticBlockRelativeResidual = variationAudits.Max(row => row.PersistedAnalyticBlockRelativeResidual);
double maxProjectedBlockHermiticityRelativeResidual = variationAudits.Max(row => row.MaxProjectedBlockHermiticityRelativeResidual);
double minPersistedProjectedBlockFrobeniusNorm = variationAudits.Min(row => row.PersistedBlock.Metrics.FrobeniusNorm);
double minAnalyticProjectedBlockFrobeniusNorm = variationAudits.Min(row => row.AnalyticBlock.Metrics.FrobeniusNorm);
double maxInvariantParityScaleAwareResidual = variationAudits.Max(row => row.InvariantParity.MaxScaleAwareResidual);

const bool fixedMeshConnectionOnlyVariationReplay = true;
const bool deltaMpsiIsZeroQualification = true;
const bool kernelTopologyArtifact = true;
const bool persistedGeometryHashMetadataAvailable = false;
const bool persistedMassWeightHashMetadataAvailable = false;
const bool routeProvidesPhysicalGuBranch = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCanonicalPhysicalMassPsi = false;
const bool routeProvidesFixedGuBranch = false;
const bool routeProvidesFixedFermionicOperatorBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesExplicitYukawaFunctional = false;
const bool routeProvidesSolvedYukawaCouplingMap = false;
const bool routeProvidesCoupledResidual = false;
const bool routeProvidesCompletedMixedLinearizationBlocks = false;
const bool routeProvidesCorrectedGaugeIdentities = false;
const bool routeProvidesMixedLinearizationGaugeCompatibilityIdentities = false;
const bool routeProvidesDirectWzBridgeLaw = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool routeProvidesHiggsRow = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesScalarProjectionTheorem = false;
const bool routeProvidesGeVNormalization = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routeProvidesPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

bool persistedNonzeroShellReciprocalReplayAuditPassed =
    phase375DiscreteReplayPresent &&
    phase375NonpromotionalBoundaryVerified &&
    meshVolumeMassPsiMaterialized &&
    meshVolumeMassPsiNonuniform &&
    isolatedVertices.SequenceEqual(expectedIsolatedVertices) &&
    isolatedFallbackMpsiWeightUsageVerified &&
    carrierMetadata.Passed &&
    fixedMeshConnectionOnlyVariationReplay &&
    deltaMpsiIsZeroQualification &&
    kernelTopologyArtifact &&
    backgroundCount == ExpectedBackgroundCount &&
    weightedModeCount == ExpectedBackgroundCount * RequestedModeCount &&
    filteredKernelModeCount == ExpectedBackgroundCount * ExpectedKernelModeCount &&
    shellModeCount == ExpectedBackgroundCount * ExpectedShellSize &&
    sentinelOutsideShellPassedCount == ExpectedBackgroundCount &&
    zeroRowColumnComplexDofPassedBackgroundCount == ExpectedBackgroundCount &&
    shellSelectionPassedBackgroundCount == ExpectedBackgroundCount &&
    weightedSolveQualityPassedBackgroundCount == ExpectedBackgroundCount &&
    variationCount == ExpectedVariationCount &&
    variationPassedCount == ExpectedVariationCount &&
    analyticPersistedDeltaKParityPassedCount == ExpectedVariationCount &&
    persistedAnalyticBlockParityPassedCount == ExpectedVariationCount &&
    projectedPairingIdentityPassedCount == ExpectedVariationCount &&
    projectedBlockHermiticityPassedCount == ExpectedVariationCount &&
    nonzeroProjectedBlockFrobeniusPassedCount == ExpectedVariationCount &&
    invariantParityPassedCount == ExpectedVariationCount &&
    unchangedGeometryHashReplayPassedCount == ExpectedVariationCount &&
    unchangedMassWeightHashReplayPassedCount == ExpectedVariationCount &&
    !routeProvidesPhysicalGuBranch &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCanonicalPhysicalMassPsi &&
    !routeProvidesFixedGuBranch &&
    !routeProvidesFixedFermionicOperatorBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesExplicitYukawaFunctional &&
    !routeProvidesSolvedYukawaCouplingMap &&
    !routeProvidesCoupledResidual &&
    !routeProvidesCompletedMixedLinearizationBlocks &&
    !routeProvidesCorrectedGaugeIdentities &&
    !routeProvidesMixedLinearizationGaugeCompatibilityIdentities &&
    !routeProvidesDirectWzBridgeLaw &&
    !routeProvidesDirectTargetIndependentWzBridgeSourceLaw &&
    !routeProvidesHiggsRow &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesScalarProjectionTheorem &&
    !routeProvidesGeVNormalization &&
    !routeProvidesGeVUnitNormalization &&
    !routeProvidesPredictions &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !routeCompletesBosonPredictions &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = persistedNonzeroShellReciprocalReplayAuditPassed
    ? "persisted-nonzero-shell-reciprocal-replay-validated-discrete-only"
    : "persisted-nonzero-shell-reciprocal-replay-audit-blocked";
string decision = persistedNonzeroShellReciprocalReplayAuditPassed
    ? "Both persisted Phase12 K matrices expose the required topology-kernel boundary and complete lowest nonzero shell at indices 48..51, with index 52 as an outside-shell sentinel. All 24 persisted/analytic deltaK variations produce nonzero Hermitian projected shell blocks with weighted pairing identity, block parity, invariant parity, and fixed-mesh hash immutability. This is a nonphysical discrete-only boundary result."
    : "Do not promote the shell replay until both 53-mode solves, topology disclosures, complete-shell sentinel checks, 24 projected block replays, parity checks, and immutability hashes pass.";
var predictionContractImpact = new
{
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
};
var result = new
{
    phaseId = "phase376-persisted-nonzero-shell-reciprocal-replay-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    persistedNonzeroShellReciprocalReplayAuditPassed,
    phase375DiscreteReplayPresent,
    phase375NonpromotionalBoundaryVerified,
    implementedObjectClassification = "bounded discrete-only persisted nonzero-shell reciprocal projected-block replay audit",
    physicalInterpretationBoundary = "nonphysical discrete-only boundary",
    fixedMeshConnectionOnlyVariationReplay,
    deltaMpsiIsZeroQualification,
    kernelTopologyArtifact,
    targetBlindConstruction = new
    {
        targetBlind = true,
        constructionHash = targetBlindConstructionHash,
        shellRule = "filter abs(lambda)<=1e-12, then select complete lowest abs(lambda) nonzero shell using spectral-only grouping tolerance max(1e-12,1e-8*abs(lambda_min_nonzero))",
        projectionRule = "G_k = Psi_shell^dagger deltaK Psi_shell = weighted <psi_i, deltaA psi_j>_M",
        physicalTargetsConsulted = Array.Empty<string>(),
    },
    structuralDisclosure = new
    {
        ambientIsolatedVertices = isolatedVertices,
        expectedAmbientIsolatedVertices = expectedIsolatedVertices,
        isolatedVertexCount = isolatedVertices.Length,
        expectedIsolatedVertexCount = expectedIsolatedVertices.Length,
        expectedZeroRowZeroColumnComplexDofCount = expectedZeroComplexDofs.Length,
        fallbackIsolatedVertexMpsiWeight = 1.0,
        isolatedFallbackRealWeightCount,
        isolatedFallbackMpsiWeightUsageVerified,
        kernelTopologyStatement = "The four ambient isolated vertices induce 48 exact zero-row/zero-column complex fermion DOFs. The explicitly filtered 48-mode kernel is a mesh-topology artifact at this boundary.",
    },
    carrierMetadata,
    hashDisclosure = new
    {
        persistedGeometryHashMetadataAvailable,
        persistedMassWeightHashMetadataAvailable,
        unavailableMetadataExplanation = "Current Phase12 artifacts do not persist geometry or M_psi hashes. Phase376 computes deterministic study-local hashes and verifies they remain unchanged through every replay.",
        persistedGeometryArtifactHash,
        derivedMeshGeometryHash,
        massPsiWeightHash,
        targetBlindConstructionHash,
    },
    conventionDefinitions = new
    {
        stiffnessActionMatrix = "K = persisted Euclidean-Hermitian Phase12 assembled Dirac matrix",
        weightedOperator = "A = M_psi^-1 K",
        stiffnessVariation = "deltaK = persisted or analytic Euclidean-Hermitian first-variation stiffness matrix",
        weightedVariationOperator = "deltaA = M_psi^-1 deltaK",
        massPsiVariation = "delta M_psi = 0 for fixed-mesh connection-only perturbations",
        generalizedEigenproblem = "K psi = lambda M_psi psi",
        projectedBlock = "G_k[i,j] = psi_i^dagger deltaK psi_j = <psi_i, deltaA psi_j>_M",
    },
    backgroundCount,
    weightedModeCount,
    filteredKernelModeCount,
    shellModeCount,
    sentinelOutsideShellPassedCount,
    zeroRowColumnComplexDofPassedBackgroundCount,
    shellSelectionPassedBackgroundCount,
    weightedSolveQualityPassedBackgroundCount,
    variationCount,
    variationPassedCount,
    analyticPersistedDeltaKParityPassedCount,
    persistedAnalyticBlockParityPassedCount,
    projectedPairingIdentityPassedCount,
    projectedBlockHermiticityPassedCount,
    nonzeroProjectedBlockFrobeniusPassedCount,
    invariantParityPassedCount,
    unchangedGeometryHashReplayPassedCount,
    unchangedMassWeightHashReplayPassedCount,
    maxAnalyticPersistedDeltaKRelativeResidual,
    maxProjectedPairingIdentityRelativeResidual,
    maxPersistedAnalyticBlockRelativeResidual,
    maxProjectedBlockHermiticityRelativeResidual,
    minPersistedProjectedBlockFrobeniusNorm,
    minAnalyticProjectedBlockFrobeniusNorm,
    maxInvariantParityScaleAwareResidual,
    tolerances = new
    {
        algebraTolerance = AlgebraTolerance,
        matrixParityTolerance = MatrixParityTolerance,
        blockParityTolerance = BlockParityTolerance,
        generalizedResidualTolerance = GeneralizedResidualTolerance,
        mNormTolerance = MNormTolerance,
        mOrthonormalityTolerance = MOrthonormalityTolerance,
        explicitKernelTolerance = KernelTolerance,
        shellGroupingFormula = "max(1e-12,1e-8*abs(lambda_min_nonzero))",
        nonzeroBlockTolerance = NonzeroBlockTolerance,
        matrixStructuralZeroTolerance = MatrixStructuralZeroTolerance,
    },
    meshVolumeMassPsi = new
    {
        builder = "MassPsiWeightsBuilder.BuildFromMesh(mesh, spinorDim * dimG)",
        realWeightCount = massPsiWeights.Length,
        minimumWeight = massPsiWeights.Min(),
        maximumWeight = massPsiWeights.Max(),
        distinctWeights = massPsiWeights.Distinct().Order().ToArray(),
        massPsiWeightHash,
    },
    routeProvidesPhysicalGuBranch,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCanonicalPhysicalMassPsi,
    routeProvidesFixedGuBranch,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesCoupledResidual,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesCorrectedGaugeIdentities,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesDirectWzBridgeLaw,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesHiggsRow,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesScalarProjectionTheorem,
    routeProvidesGeVNormalization,
    routeProvidesGeVUnitNormalization,
    routeProvidesPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact,
    explicitDiscreteOnlyNonclaims = new[]
    {
        "nonphysical discrete-only boundary",
        "kernel is an isolated-vertex topology artifact",
        "no canonical physical M_psi",
        "no fixed GU branch",
        "no completed fermionic action",
        "no completed mixed blocks",
        "no corrected-gauge identities",
        "no direct W/Z bridge law",
        "no Higgs row",
        "no GeV normalization",
        "no predictions",
        "no contract fills",
    },
    backgroundAudits,
    variationAudits,
    sourceEvidence = new
    {
        phase375SummaryPath = Phase375SummaryPath,
        phase12Root = Phase12Root,
        fermionDir = FermionDir,
        variationDir = VariationDir,
        modeDir = ModeDir,
        geometryManifestPath = GeometryManifestPath,
        omegaMetadataPath = OmegaMetadataPath,
        spinorRepresentationPath = SpinorRepresentationPath,
    },
    decision,
};

string fullPath = Path.Combine(outputDir, "persisted_nonzero_shell_reciprocal_replay_audit.json");
string summaryPath = Path.Combine(outputDir, "persisted_nonzero_shell_reciprocal_replay_audit_summary.json");
File.WriteAllText(fullPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.generatedAt,
    terminalStatus,
    persistedNonzeroShellReciprocalReplayAuditPassed,
    phase375DiscreteReplayPresent,
    phase375NonpromotionalBoundaryVerified,
    result.implementedObjectClassification,
    result.physicalInterpretationBoundary,
    fixedMeshConnectionOnlyVariationReplay,
    deltaMpsiIsZeroQualification,
    kernelTopologyArtifact,
    targetBlindConstructionHash,
    ambientIsolatedVertices = isolatedVertices,
    isolatedVertexCount = isolatedVertices.Length,
    expectedZeroRowZeroColumnComplexDofCount = expectedZeroComplexDofs.Length,
    isolatedFallbackMpsiWeightUsageVerified,
    carrierMetadata,
    persistedGeometryHashMetadataAvailable,
    persistedMassWeightHashMetadataAvailable,
    persistedGeometryArtifactHash,
    derivedMeshGeometryHash,
    massPsiWeightHash,
    backgroundCount,
    weightedModeCount,
    filteredKernelModeCount,
    shellModeCount,
    sentinelOutsideShellPassedCount,
    zeroRowColumnComplexDofPassedBackgroundCount,
    shellSelectionPassedBackgroundCount,
    weightedSolveQualityPassedBackgroundCount,
    variationCount,
    variationPassedCount,
    analyticPersistedDeltaKParityPassedCount,
    persistedAnalyticBlockParityPassedCount,
    projectedPairingIdentityPassedCount,
    projectedBlockHermiticityPassedCount,
    nonzeroProjectedBlockFrobeniusPassedCount,
    invariantParityPassedCount,
    unchangedGeometryHashReplayPassedCount,
    unchangedMassWeightHashReplayPassedCount,
    maxAnalyticPersistedDeltaKRelativeResidual,
    maxProjectedPairingIdentityRelativeResidual,
    maxPersistedAnalyticBlockRelativeResidual,
    maxProjectedBlockHermiticityRelativeResidual,
    minPersistedProjectedBlockFrobeniusNorm,
    minAnalyticProjectedBlockFrobeniusNorm,
    maxInvariantParityScaleAwareResidual,
    routeProvidesPhysicalGuBranch,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCanonicalPhysicalMassPsi,
    routeProvidesFixedGuBranch,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesCoupledResidual,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesCorrectedGaugeIdentities,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesDirectWzBridgeLaw,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesHiggsRow,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesScalarProjectionTheorem,
    routeProvidesGeVNormalization,
    routeProvidesGeVUnitNormalization,
    routeProvidesPredictions,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact,
    result.explicitDiscreteOnlyNonclaims,
    decision,
}, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"persistedNonzeroShellReciprocalReplayAuditPassed={persistedNonzeroShellReciprocalReplayAuditPassed}");
Console.WriteLine($"backgroundCount={backgroundCount}/{ExpectedBackgroundCount}");
Console.WriteLine($"weightedModeCount={weightedModeCount}/{ExpectedBackgroundCount * RequestedModeCount}");
Console.WriteLine($"filteredKernelModeCount={filteredKernelModeCount}/{ExpectedBackgroundCount * ExpectedKernelModeCount}");
Console.WriteLine($"shellSelectionPassedBackgroundCount={shellSelectionPassedBackgroundCount}/{backgroundCount}");
Console.WriteLine($"variationPassedCount={variationPassedCount}/{variationCount}");
Console.WriteLine($"projectedPairingIdentityPassedCount={projectedPairingIdentityPassedCount}/{variationCount}");
Console.WriteLine($"persistedAnalyticBlockParityPassedCount={persistedAnalyticBlockParityPassedCount}/{variationCount}");
Console.WriteLine($"nonzeroProjectedBlockFrobeniusPassedCount={nonzeroProjectedBlockFrobeniusPassedCount}/{variationCount}");
Console.WriteLine($"minPersistedProjectedBlockFrobeniusNorm={minPersistedProjectedBlockFrobeniusNorm:R}");
Console.WriteLine($"maxPersistedAnalyticBlockRelativeResidual={maxPersistedAnalyticBlockRelativeResidual:R}");
Console.WriteLine($"maxProjectedPairingIdentityRelativeResidual={maxProjectedPairingIdentityRelativeResidual:R}");
Console.WriteLine($"summaryPath={summaryPath}");

BackgroundAudit BuildBackgroundAudit(string metadataPath)
{
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    var metadata = metadataDoc.RootElement;
    string backgroundId = RequiredString(metadata, "fermionBackgroundId");
    string layoutPath = Path.Combine(FermionDir, $"layout_{backgroundId}.json");
    var layout = JsonSerializer.Deserialize<FermionFieldLayout>(File.ReadAllText(layoutPath), options)
        ?? throw new InvalidDataException($"Failed to deserialize {layoutPath}.");
    int matrixSize = SquareMatrixSize(metadata);
    string matrixPath = Path.Combine(FermionDir, RequiredString(metadata, "explicitMatrixRef"));
    var k = LoadFlatInterleavedMatrix(matrixPath, matrixSize);
    var a = k.ScaleRows(massPsiWeights);
    int[] zeroDofs = ZeroRowColumnComplexDofs(k, MatrixStructuralZeroTolerance);
    var solve = solver.Solve(
        BuildBundle(backgroundId, layout.LayoutId, metadata, k.ToFlatInterleaved()),
        layout,
        BuildConfig(),
        provenance);
    var weightedSolve = EvaluateWeightedSolve(solve, k);
    string sidecarPath = Path.Combine(backgroundOutputDir, $"{backgroundId}-nonzero-shell-replay.json");
    var audit = new BackgroundAudit
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMetadataPath = metadataPath,
        BaseDiracMatrixPath = matrixPath,
        BaseMatrixHash = HashFile(matrixPath),
        LayoutPath = layoutPath,
        KIsEuclideanHermitian = EuclideanHermiticityResidual(k) <= AlgebraTolerance,
        KEuclideanHermiticityRelativeResidual = EuclideanHermiticityResidual(k),
        AIsMSelfAdjoint = MAdjointResidual(a, massPsiWeights) <= AlgebraTolerance,
        AMAdjointRelativeResidual = MAdjointResidual(a, massPsiWeights),
        ZeroRowColumnComplexDofs = zeroDofs,
        ZeroRowColumnComplexDofCount = zeroDofs.Length,
        ExpectedZeroRowColumnComplexDofs = expectedZeroComplexDofs,
        ZeroRowColumnStructurePassed =
            zeroDofs.Length == expectedZeroComplexDofs.Length &&
            zeroDofs.SequenceEqual(expectedZeroComplexDofs),
        WeightedSolve = weightedSolve,
        GeometryHash = CaptureFingerprint().DerivedMeshGeometryHash,
        MassWeightHash = CaptureFingerprint().MassPsiWeightHash,
        SidecarPath = sidecarPath,
    };
    File.WriteAllText(sidecarPath, JsonSerializer.Serialize(audit, options));
    return audit;
}

VariationAudit BuildVariationAudit(string metadataPath)
{
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    var metadata = metadataDoc.RootElement;
    string variationId = RequiredString(metadata, "variationId");
    string backgroundId = RequiredString(metadata, "fermionBackgroundId");
    string bosonModeId = RequiredString(metadata, "bosonModeId");
    string matrixPath = Path.Combine(VariationDir, RequiredString(metadata, "matrixArtifactRef"));
    string modePath = Path.Combine(ModeDir, $"{bosonModeId}.json");
    var before = CaptureFingerprint();
    var persistedDeltaK = LoadNestedMatrix(matrixPath);
    var bosonMode = LoadBosonMode(modePath);
    var (analyticRe, analyticIm) = DiracVariationComputer.ComputeAnalytical(
        bosonMode.ModeVector,
        gammas,
        mesh.VertexCount,
        spinorDim,
        dimG,
        edgeLengths,
        cellsPerEdge,
        edgeDirections);
    var analyticDeltaK = new ComplexMatrix(analyticRe, analyticIm);
    var persisted = BuildVariationOperatorAudit(persistedDeltaK);
    var analytic = BuildVariationOperatorAudit(analyticDeltaK);
    if (!shellsByBackground.TryGetValue(backgroundId, out var shell))
        throw new InvalidDataException($"Missing nonzero shell for {backgroundId}.");
    var persistedBlock = BuildProjectedBlockAudit(shell, persisted);
    var analyticBlock = BuildProjectedBlockAudit(shell, analytic);
    double matrixParityResidual = MatrixRelativeResidual(persistedDeltaK, analyticDeltaK);
    double blockParityResidual = BlockRelativeResidual(
        persistedBlock.StiffnessProjectedBlock,
        analyticBlock.StiffnessProjectedBlock);
    var invariantParity = BuildInvariantParity(persistedBlock.Metrics, analyticBlock.Metrics);
    var after = CaptureFingerprint();
    string sidecarPath = Path.Combine(variationOutputDir, $"{variationId}-nonzero-shell-replay.json");
    var audit = new VariationAudit
    {
        VariationId = variationId,
        FermionBackgroundId = backgroundId,
        BosonModeId = bosonModeId,
        PersistedVariationMetadataPath = metadataPath,
        PersistedVariationMatrixPath = matrixPath,
        AnalyticVariationSource = "Gu.Phase4.Couplings.DiracVariationComputer.ComputeAnalytical",
        ConnectionCarrierType = bosonMode.ConnectionCarrierType,
        ConnectionCarrierTypeSupported = bosonMode.ConnectionCarrierTypeSupported,
        ConnectionVectorLength = bosonMode.ModeVector.Length,
        ExpectedConnectionVectorLength = ExpectedConnectionVectorLength,
        ConnectionVectorLengthPassed = bosonMode.ModeVector.Length == ExpectedConnectionVectorLength,
        PersistedOperator = persisted.Record,
        AnalyticOperator = analytic.Record,
        AnalyticPersistedDeltaKRelativeResidual = matrixParityResidual,
        AnalyticPersistedDeltaKParityPassed = matrixParityResidual <= MatrixParityTolerance,
        PersistedBlock = persistedBlock,
        AnalyticBlock = analyticBlock,
        PersistedAnalyticBlockRelativeResidual = blockParityResidual,
        PersistedAnalyticBlockParityPassed = blockParityResidual <= BlockParityTolerance,
        ProjectedPairingIdentityPassed =
            persistedBlock.PairingIdentityPassed &&
            analyticBlock.PairingIdentityPassed,
        MaxProjectedPairingIdentityRelativeResidual = Math.Max(
            persistedBlock.PairingIdentityRelativeResidual,
            analyticBlock.PairingIdentityRelativeResidual),
        ProjectedBlockHermiticityPassed =
            persistedBlock.Metrics.IsHermitian &&
            analyticBlock.Metrics.IsHermitian,
        MaxProjectedBlockHermiticityRelativeResidual = Math.Max(
            persistedBlock.Metrics.HermiticityRelativeResidual,
            analyticBlock.Metrics.HermiticityRelativeResidual),
        NonzeroProjectedBlockFrobeniusPassed =
            persistedBlock.Metrics.FrobeniusNorm > NonzeroBlockTolerance &&
            analyticBlock.Metrics.FrobeniusNorm > NonzeroBlockTolerance,
        InvariantParity = invariantParity,
        FingerprintBeforeReplay = before,
        FingerprintAfterReplay = after,
        GeometryHashUnchangedThroughReplay =
            before.PersistedGeometryArtifactHash == baselineFingerprint.PersistedGeometryArtifactHash &&
            after.PersistedGeometryArtifactHash == baselineFingerprint.PersistedGeometryArtifactHash &&
            before.DerivedMeshGeometryHash == baselineFingerprint.DerivedMeshGeometryHash &&
            after.DerivedMeshGeometryHash == baselineFingerprint.DerivedMeshGeometryHash,
        MassWeightHashUnchangedThroughReplay =
            before.MassPsiWeightHash == baselineFingerprint.MassPsiWeightHash &&
            after.MassPsiWeightHash == baselineFingerprint.MassPsiWeightHash,
        SidecarPath = sidecarPath,
    };
    audit.VariationPassed =
        audit.ConnectionCarrierTypeSupported &&
        audit.ConnectionVectorLengthPassed &&
        audit.PersistedOperator.AllOperatorIdentitiesPassed &&
        audit.AnalyticOperator.AllOperatorIdentitiesPassed &&
        audit.AnalyticPersistedDeltaKParityPassed &&
        audit.PersistedAnalyticBlockParityPassed &&
        audit.ProjectedPairingIdentityPassed &&
        audit.ProjectedBlockHermiticityPassed &&
        audit.NonzeroProjectedBlockFrobeniusPassed &&
        audit.InvariantParity.Passed &&
        audit.GeometryHashUnchangedThroughReplay &&
        audit.MassWeightHashUnchangedThroughReplay;
    File.WriteAllText(sidecarPath, JsonSerializer.Serialize(audit, options));
    return audit;
}

WeightedSolveAudit EvaluateWeightedSolve(FermionSpectralResult solve, ComplexMatrix k)
{
    var modes = solve.Modes.Select(mode =>
    {
        double[] psi = mode.EigenvectorCoefficients
            ?? throw new InvalidDataException($"Solver mode {mode.ModeId} has no coefficients.");
        double generalizedResidual = RelativeVectorResidual(
            k.Apply(psi),
            Scale(ApplyWeights(psi, massPsiWeights), mode.EigenvalueRe));
        double mNorm = Math.Sqrt(ComplexInnerProduct(psi, psi, massPsiWeights).Real);
        return new WeightedModeAudit
        {
            ModeId = mode.ModeId,
            ModeIndex = mode.ModeIndex,
            Eigenvalue = mode.EigenvalueRe,
            IsKernel = Math.Abs(mode.EigenvalueRe) <= KernelTolerance,
            SolverReportedResidualNorm = mode.ResidualNorm,
            GeneralizedKPsiEqualsLambdaMPsiRelativeResidual = generalizedResidual,
            GeneralizedResidualPassed = generalizedResidual <= GeneralizedResidualTolerance,
            MNorm = mNorm,
            MNormResidual = Math.Abs(mNorm - 1.0),
            MNormPassed = Math.Abs(mNorm - 1.0) <= MNormTolerance,
            Coefficients = psi,
        };
    }).ToArray();
    double maxOrthResidual = 0.0;
    for (int left = 0; left < modes.Length; left++)
        for (int right = 0; right < modes.Length; right++)
        {
            var inner = ComplexInnerProduct(modes[left].Coefficients, modes[right].Coefficients, massPsiWeights);
            double expected = left == right ? 1.0 : 0.0;
            maxOrthResidual = Math.Max(maxOrthResidual, ComplexMagnitude(inner.Real - expected, inner.Imaginary));
        }
    var shell = SelectLowestNonzeroShell(modes);
    bool qualityChecksPassed =
        solve.Diagnostics.Converged &&
        modes.Length == RequestedModeCount &&
        modes.All(mode => mode.GeneralizedResidualPassed) &&
        modes.All(mode => mode.MNormPassed) &&
        maxOrthResidual <= MOrthonormalityTolerance &&
        shell.ShellSelectionPassed;
    return new()
    {
        SolverName = solve.Diagnostics.SolverName,
        SolverConverged = solve.Diagnostics.Converged,
        SolverIterations = solve.Diagnostics.Iterations,
        SolverNotes = solve.Diagnostics.Notes,
        RequestedModeCount = RequestedModeCount,
        ModeCount = modes.Length,
        KernelFilter = "abs(lambda)<=1e-12",
        KernelModeCount = modes.Count(mode => mode.IsKernel),
        GeneralizedResidualPassedCount = modes.Count(mode => mode.GeneralizedResidualPassed),
        MNormPassedCount = modes.Count(mode => mode.MNormPassed),
        MaxGeneralizedRelativeResidual = modes.Max(mode => mode.GeneralizedKPsiEqualsLambdaMPsiRelativeResidual),
        MaxMNormResidual = modes.Max(mode => mode.MNormResidual),
        MaxMOrthonormalityResidual = maxOrthResidual,
        MOrthonormalityPassed = maxOrthResidual <= MOrthonormalityTolerance,
        AllQualityChecksPassed = qualityChecksPassed,
        Shell = shell,
        Modes = modes,
    };
}

ShellAudit SelectLowestNonzeroShell(IReadOnlyList<WeightedModeAudit> modes)
{
    var nonzeroModes = modes
        .Where(mode => Math.Abs(mode.Eigenvalue) > KernelTolerance)
        .OrderBy(mode => Math.Abs(mode.Eigenvalue))
        .ThenBy(mode => mode.ModeIndex)
        .ToArray();
    if (nonzeroModes.Length == 0)
        throw new InvalidDataException("The explicitly filtered spectrum has no nonzero sentinel-backed shell.");
    double lambdaMinMagnitude = Math.Abs(nonzeroModes[0].Eigenvalue);
    double groupingTolerance = Math.Max(1e-12, 1e-8 * lambdaMinMagnitude);
    var shellModes = nonzeroModes
        .Where(mode => Math.Abs(Math.Abs(mode.Eigenvalue) - lambdaMinMagnitude) <= groupingTolerance)
        .OrderBy(mode => mode.ModeIndex)
        .ToArray();
    var sentinel = modes.Single(mode => mode.ModeIndex == ExpectedSentinelIndex);
    bool sentinelOutsideShell =
        !shellModes.Any(mode => mode.ModeIndex == sentinel.ModeIndex) &&
        Math.Abs(Math.Abs(sentinel.Eigenvalue) - lambdaMinMagnitude) > groupingTolerance;
    bool selectionPassed =
        modes.Count(mode => mode.IsKernel) == ExpectedKernelModeCount &&
        shellModes.Select(mode => mode.ModeIndex).SequenceEqual(expectedShellIndices) &&
        shellModes.Length == ExpectedShellSize &&
        sentinelOutsideShell;
    return new()
    {
        ExplicitKernelTolerance = KernelTolerance,
        GroupingToleranceFormula = "max(1e-12,1e-8*abs(lambda_min_nonzero))",
        LambdaMinNonzeroMagnitude = lambdaMinMagnitude,
        GroupingTolerance = groupingTolerance,
        ShellIndices = shellModes.Select(mode => mode.ModeIndex).ToArray(),
        ExpectedShellIndices = expectedShellIndices,
        ShellSize = shellModes.Length,
        ExpectedShellSize = ExpectedShellSize,
        SentinelIndex = sentinel.ModeIndex,
        SentinelEigenvalue = sentinel.Eigenvalue,
        SentinelOutsideShell = sentinelOutsideShell,
        ShellSelectionPassed = selectionPassed,
        Modes = shellModes,
    };
}

VariationOperatorAudit BuildVariationOperatorAudit(ComplexMatrix deltaK)
{
    var deltaA = deltaK.ScaleRows(massPsiWeights);
    double deltaKHermiticity = EuclideanHermiticityResidual(deltaK);
    double deltaAMAdjoint = MAdjointResidual(deltaA, massPsiWeights);
    return new(deltaK, deltaA, new()
    {
        DeltaKIsEuclideanHermitian = deltaKHermiticity <= AlgebraTolerance,
        DeltaKEuclideanHermiticityRelativeResidual = deltaKHermiticity,
        DeltaAIsMSelfAdjoint = deltaAMAdjoint <= AlgebraTolerance,
        DeltaAMAdjointRelativeResidual = deltaAMAdjoint,
        AllOperatorIdentitiesPassed =
            deltaKHermiticity <= AlgebraTolerance &&
            deltaAMAdjoint <= AlgebraTolerance,
    });
}

ProjectedBlockAudit BuildProjectedBlockAudit(
    IReadOnlyList<WeightedModeAudit> shell,
    VariationOperatorAudit variation)
{
    ComplexValue[][] stiffnessBlock = ProjectBlock(shell, variation.DeltaK, null);
    ComplexValue[][] weightedBlock = ProjectBlock(shell, variation.DeltaA, massPsiWeights);
    double pairingResidual = BlockRelativeResidual(stiffnessBlock, weightedBlock);
    return new()
    {
        Construction = "G_k[i,j] = psi_i^dagger deltaK psi_j = <psi_i, deltaA psi_j>_M",
        ShellModeIndices = shell.Select(mode => mode.ModeIndex).ToArray(),
        StiffnessProjectedBlock = stiffnessBlock,
        WeightedProjectedBlock = weightedBlock,
        PairingIdentityRelativeResidual = pairingResidual,
        PairingIdentityPassed = pairingResidual <= AlgebraTolerance,
        Metrics = BuildBlockMetrics(stiffnessBlock),
    };
}

ComplexValue[][] ProjectBlock(
    IReadOnlyList<WeightedModeAudit> shell,
    ComplexMatrix matrix,
    double[]? weights)
{
    return shell.Select(left => shell.Select(right =>
        ComplexInnerProduct(left.Coefficients, matrix.Apply(right.Coefficients), weights)).ToArray()).ToArray();
}

BlockMetrics BuildBlockMetrics(ComplexValue[][] block)
{
    Complex trace = Complex.Zero;
    Complex traceSquare = Complex.Zero;
    double norm2 = 0.0;
    double hermitianResidual2 = 0.0;
    for (int row = 0; row < block.Length; row++)
        for (int col = 0; col < block.Length; col++)
        {
            Complex entry = ToComplex(block[row][col]);
            Complex adjointEntry = Complex.Conjugate(ToComplex(block[col][row]));
            norm2 += entry.Magnitude * entry.Magnitude;
            hermitianResidual2 += (entry - adjointEntry).Magnitude * (entry - adjointEntry).Magnitude;
            traceSquare += entry * ToComplex(block[col][row]);
            if (row == col)
                trace += entry;
        }
    double norm = Math.Sqrt(norm2);
    double hermiticityResidual = Math.Sqrt(hermitianResidual2 / Math.Max(norm2, 1e-300));
    return new()
    {
        Dimension = block.Length,
        FrobeniusNorm = norm,
        FrobeniusNormSquared = norm2,
        Trace = FromComplex(trace),
        TraceSquare = FromComplex(traceSquare),
        Determinant = FromComplex(Determinant(block)),
        HermiticityRelativeResidual = hermiticityResidual,
        IsHermitian = hermiticityResidual <= AlgebraTolerance,
        NonzeroFrobeniusNorm = norm > NonzeroBlockTolerance,
        BasisInvariantDisclosure = "Frobenius norm, trace, trace(G^2), and determinant are invariant under unitary changes of shell basis.",
    };
}

InvariantParityAudit BuildInvariantParity(BlockMetrics persisted, BlockMetrics analytic)
{
    double frobenius = ScaleAwareResidual(persisted.FrobeniusNorm, analytic.FrobeniusNorm);
    double trace = ComplexScaleAwareResidual(persisted.Trace, analytic.Trace);
    double traceSquare = ComplexScaleAwareResidual(persisted.TraceSquare, analytic.TraceSquare);
    double determinant = ComplexScaleAwareResidual(persisted.Determinant, analytic.Determinant);
    double max = new[] { frobenius, trace, traceSquare, determinant }.Max();
    return new()
    {
        FrobeniusNormScaleAwareResidual = frobenius,
        TraceScaleAwareResidual = trace,
        TraceSquareScaleAwareResidual = traceSquare,
        DeterminantScaleAwareResidual = determinant,
        MaxScaleAwareResidual = max,
        Passed = max <= BlockParityTolerance,
    };
}

CarrierMetadataAudit LoadCarrierMetadata()
{
    using var geometryDoc = JsonDocument.Parse(File.ReadAllText(GeometryManifestPath));
    using var omegaDoc = JsonDocument.Parse(File.ReadAllText(OmegaMetadataPath));
    var omega = omegaDoc.RootElement;
    string carrierType = RequiredString(omega.GetProperty("signature"), "carrierType");
    int[] shape = omega.GetProperty("shape").EnumerateArray().Select(value => value.GetInt32()).ToArray();
    int coefficientCount = omega.GetProperty("coefficients").GetArrayLength();
    int ambientEdgeCount = geometryDoc.RootElement.GetProperty("ambientSpace").GetProperty("edgeCount").GetInt32();
    bool passed =
        carrierType == ExpectedConnectionCarrierType &&
        ambientEdgeCount == ExpectedAmbientEdgeCount &&
        shape.SequenceEqual([ExpectedAmbientEdgeCount, dimG]) &&
        coefficientCount == ExpectedConnectionVectorLength;
    return new()
    {
        MetadataAvailable = true,
        CarrierTypeSourcePath = OmegaMetadataPath,
        GeometrySourcePath = GeometryManifestPath,
        CarrierType = carrierType,
        ExpectedCarrierType = ExpectedConnectionCarrierType,
        AmbientEdgeCount = ambientEdgeCount,
        ExpectedAmbientEdgeCount = ExpectedAmbientEdgeCount,
        GaugeDimension = dimG,
        ConnectionShape = shape,
        ConnectionVectorLength = coefficientCount,
        ExpectedConnectionVectorLength = ExpectedConnectionVectorLength,
        Passed = passed,
    };
}

BosonModeAudit LoadBosonMode(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var root = doc.RootElement;
    var fractions = root.GetProperty("tensorEnergyFractions");
    bool supported =
        fractions.TryGetProperty(ExpectedConnectionCarrierType, out var fraction) &&
        fraction.GetDouble() == 1.0;
    return new()
    {
        ModePath = path,
        ConnectionCarrierType = supported ? ExpectedConnectionCarrierType : "unavailable",
        ConnectionCarrierTypeSupported = supported,
        ModeVector = root.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray(),
    };
}

ReplayFingerprint CaptureFingerprint() => new()
{
    PersistedGeometryArtifactHash = HashFile(GeometryManifestPath),
    DerivedMeshGeometryHash = HashMesh(mesh),
    MassPsiWeightHash = HashDoubles(massPsiWeights),
};

DiracOperatorBundle BuildBundle(
    string backgroundId,
    string layoutId,
    JsonElement metadata,
    double[] explicitMatrix) => new()
{
    OperatorId = $"phase376-stiffness-k-{backgroundId}",
    FermionBackgroundId = backgroundId,
    LayoutId = layoutId,
    SpinConnectionId = $"phase376-stiffness-k-{backgroundId}",
    MatrixShape = metadata.GetProperty("matrixShape").EnumerateArray().Select(value => value.GetInt32()).ToArray(),
    HasExplicitMatrix = true,
    ExplicitMatrix = explicitMatrix,
    IsHermitian = metadata.GetProperty("isHermitian").GetBoolean(),
    HermiticityResidual = metadata.GetProperty("hermiticityResidual").GetDouble(),
    HermiticityTolerance = metadata.GetProperty("hermiticityTolerance").GetDouble(),
    MassBranchTermIncluded = metadata.GetProperty("massBranchTermIncluded").GetBoolean(),
    CorrectionTermIncluded = metadata.GetProperty("correctionTermIncluded").GetBoolean(),
    GaugeReductionApplied = metadata.GetProperty("gaugeReductionApplied").GetBoolean(),
    CellCount = metadata.GetProperty("cellCount").GetInt32(),
    DofsPerCell = metadata.GetProperty("dofsPerCell").GetInt32(),
    DiagnosticNotes = ["Phase376 persisted stiffness K bounded nonzero-shell replay."],
    Provenance = provenance,
};

FermionSpectralConfig BuildConfig() => new()
{
    TargetRegion = "lowest-magnitude",
    ModeCount = RequestedModeCount,
    GaugeReduction = false,
    NullspaceDeflation = false,
    ConvergenceTolerance = 1e-10,
    MaxIterations = 1000,
    Seed = 42,
    MassPsiWeights = massPsiWeights,
};

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
};

static bool JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False) &&
    value.GetBoolean();

static bool FalseFlag(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False) &&
    !value.GetBoolean();

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    value.ValueKind == JsonValueKind.Number &&
    value.TryGetInt32(out int result)
        ? result
        : null;

static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() ?? throw new InvalidDataException($"{propertyName} must not be null.")
        : throw new InvalidDataException($"{propertyName} must be a string.");

static int SquareMatrixSize(JsonElement metadata)
{
    int[] shape = metadata.GetProperty("matrixShape").EnumerateArray().Select(value => value.GetInt32()).ToArray();
    if (shape.Length != 2 || shape[0] != shape[1])
        throw new InvalidDataException("Expected a square matrix shape.");
    return shape[0];
}

static ComplexMatrix LoadFlatInterleavedMatrix(string path, int size)
{
    double[] values = JsonSerializer.Deserialize<double[]>(File.ReadAllText(path))
        ?? throw new InvalidDataException($"Failed to deserialize {path}.");
    if (values.Length != 2 * size * size)
        throw new InvalidDataException($"Expected {2 * size * size} matrix values in {path}, found {values.Length}.");
    return ComplexMatrix.FromFlatInterleaved(values, size);
}

static ComplexMatrix LoadNestedMatrix(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return new(LoadNestedArray(doc.RootElement.GetProperty("real")), LoadNestedArray(doc.RootElement.GetProperty("imag")));
}

static double[,] LoadNestedArray(JsonElement element)
{
    var rows = element.EnumerateArray().ToArray();
    if (rows.Length == 0)
        throw new InvalidDataException("Matrix must contain at least one row.");
    int cols = rows[0].GetArrayLength();
    var result = new double[rows.Length, cols];
    for (int row = 0; row < rows.Length; row++)
    {
        var values = rows[row].EnumerateArray().ToArray();
        if (values.Length != cols)
            throw new InvalidDataException("Matrix rows must have equal lengths.");
        for (int col = 0; col < cols; col++)
            result[row, col] = values[col].GetDouble();
    }
    return result;
}

static int[] ZeroRowColumnComplexDofs(ComplexMatrix matrix, double tolerance)
{
    return Enumerable.Range(0, matrix.Size).Where(index =>
    {
        for (int other = 0; other < matrix.Size; other++)
            if (ComplexMagnitude(matrix.Re[index, other], matrix.Im[index, other]) > tolerance ||
                ComplexMagnitude(matrix.Re[other, index], matrix.Im[other, index]) > tolerance)
                return false;
        return true;
    }).ToArray();
}

static double EuclideanHermiticityResidual(ComplexMatrix matrix) =>
    AdjointResidual(matrix, null);

static double MAdjointResidual(ComplexMatrix matrix, double[] weights) =>
    AdjointResidual(matrix, weights);

static double AdjointResidual(ComplexMatrix matrix, double[]? weights)
{
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < matrix.Size; row++)
        for (int col = 0; col < matrix.Size; col++)
        {
            double rowWeight = weights is null ? 1.0 : weights[2 * row];
            double colWeight = weights is null ? 1.0 : weights[2 * col];
            double leftRe = rowWeight * matrix.Re[row, col];
            double leftIm = rowWeight * matrix.Im[row, col];
            double rightRe = colWeight * matrix.Re[col, row];
            double rightIm = -colWeight * matrix.Im[col, row];
            residual2 += Square(leftRe - rightRe) + Square(leftIm - rightIm);
            norm2 += Square(leftRe) + Square(leftIm);
        }
    return Math.Sqrt(residual2 / Math.Max(norm2, 1e-300));
}

static double MatrixRelativeResidual(ComplexMatrix left, ComplexMatrix right)
{
    if (left.Size != right.Size)
        throw new InvalidDataException("Matrix sizes must match.");
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < left.Size; row++)
        for (int col = 0; col < left.Size; col++)
        {
            residual2 += Square(left.Re[row, col] - right.Re[row, col]) + Square(left.Im[row, col] - right.Im[row, col]);
            norm2 += Square(left.Re[row, col]) + Square(left.Im[row, col]);
        }
    return Math.Sqrt(residual2 / Math.Max(norm2, 1e-300));
}

static double BlockRelativeResidual(ComplexValue[][] left, ComplexValue[][] right)
{
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < left.Length; row++)
        for (int col = 0; col < left[row].Length; col++)
        {
            residual2 += Square(left[row][col].Real - right[row][col].Real) +
                         Square(left[row][col].Imaginary - right[row][col].Imaginary);
            norm2 += Square(left[row][col].Real) + Square(left[row][col].Imaginary);
        }
    return Math.Sqrt(residual2 / Math.Max(norm2, 1e-300));
}

static ComplexValue ComplexInnerProduct(double[] left, double[] right, double[]? weights)
{
    if (left.Length != right.Length || left.Length % 2 != 0)
        throw new InvalidDataException("Complex-interleaved vector lengths must match.");
    if (weights is not null && weights.Length != left.Length)
        throw new InvalidDataException("Weight and vector lengths must match.");
    double real = 0.0;
    double imaginary = 0.0;
    for (int index = 0; index < left.Length; index += 2)
    {
        double weight = weights is null ? 1.0 : weights[index];
        real += weight * (left[index] * right[index] + left[index + 1] * right[index + 1]);
        imaginary += weight * (left[index] * right[index + 1] - left[index + 1] * right[index]);
    }
    return new(real, imaginary);
}

static double[] Scale(double[] vector, double scale) =>
    vector.Select(value => scale * value).ToArray();

static double[] Subtract(double[] left, double[] right) =>
    left.Zip(right, (l, r) => l - r).ToArray();

static double[] ApplyWeights(double[] vector, double[] weights) =>
    vector.Zip(weights, (value, weight) => value * weight).ToArray();

static double RelativeVectorResidual(double[] left, double[] right) =>
    EuclideanNorm(Subtract(left, right)) / Math.Max(1e-300, Math.Max(EuclideanNorm(left), EuclideanNorm(right)));

static double EuclideanNorm(double[] vector) =>
    Math.Sqrt(vector.Sum(Square));

static double ScaleAwareResidual(double left, double right) =>
    Math.Abs(left - right) / Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right)));

static double ComplexScaleAwareResidual(ComplexValue left, ComplexValue right) =>
    ComplexMagnitude(left.Real - right.Real, left.Imaginary - right.Imaginary) /
    Math.Max(1.0, Math.Max(
        ComplexMagnitude(left.Real, left.Imaginary),
        ComplexMagnitude(right.Real, right.Imaginary)));

static double ComplexMagnitude(double real, double imaginary) =>
    Math.Sqrt(Square(real) + Square(imaginary));

static double Square(double value) => value * value;

static Complex ToComplex(ComplexValue value) => new(value.Real, value.Imaginary);

static ComplexValue FromComplex(Complex value) => new(value.Real, value.Imaginary);

static Complex Determinant(ComplexValue[][] block)
{
    int n = block.Length;
    var matrix = new Complex[n, n];
    for (int row = 0; row < n; row++)
        for (int col = 0; col < n; col++)
            matrix[row, col] = ToComplex(block[row][col]);
    Complex determinant = Complex.One;
    for (int pivot = 0; pivot < n; pivot++)
    {
        int best = pivot;
        for (int row = pivot + 1; row < n; row++)
            if (matrix[row, pivot].Magnitude > matrix[best, pivot].Magnitude)
                best = row;
        if (matrix[best, pivot].Magnitude <= 1e-300)
            return Complex.Zero;
        if (best != pivot)
        {
            for (int col = pivot; col < n; col++)
                (matrix[pivot, col], matrix[best, col]) = (matrix[best, col], matrix[pivot, col]);
            determinant = -determinant;
        }
        Complex diagonal = matrix[pivot, pivot];
        determinant *= diagonal;
        for (int row = pivot + 1; row < n; row++)
        {
            Complex factor = matrix[row, pivot] / diagonal;
            for (int col = pivot + 1; col < n; col++)
                matrix[row, col] -= factor * matrix[pivot, col];
        }
    }
    return determinant;
}

static double ComputeEdgeLength(SimplicialMesh mesh, int edge)
{
    var left = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
    var right = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
    double sum = 0.0;
    for (int index = 0; index < left.Length; index++)
        sum += Square(right[index] - left[index]);
    return Math.Sqrt(sum);
}

static double[] ComputeEdgeDirection(SimplicialMesh mesh, int edge)
{
    var left = mesh.GetVertexCoordinates(mesh.Edges[edge][0]);
    var right = mesh.GetVertexCoordinates(mesh.Edges[edge][1]);
    var result = new double[left.Length];
    for (int index = 0; index < left.Length; index++)
        result[index] = right[index] - left[index];
    double norm = Math.Sqrt(result.Sum(Square));
    return norm > 1e-14 ? result.Select(value => value / norm).ToArray() : result;
}

static string HashFile(string path) =>
    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

static string HashText(string text) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

static string HashDoubles(double[] values)
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    foreach (double value in values)
        writer.Write(value);
    writer.Flush();
    return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
}

static string HashMesh(SimplicialMesh mesh)
{
    using var stream = new MemoryStream();
    using var writer = new BinaryWriter(stream);
    writer.Write(mesh.EmbeddingDimension);
    writer.Write(mesh.SimplicialDimension);
    writer.Write(mesh.VertexCount);
    foreach (double value in mesh.VertexCoordinates)
        writer.Write(value);
    writer.Write(mesh.CellCount);
    foreach (int[] cell in mesh.CellVertices)
    {
        writer.Write(cell.Length);
        foreach (int vertex in cell)
            writer.Write(vertex);
    }
    writer.Write(mesh.EdgeCount);
    foreach (int[] edge in mesh.Edges)
    {
        writer.Write(edge.Length);
        foreach (int vertex in edge)
            writer.Write(vertex);
    }
    writer.Flush();
    return Convert.ToHexString(SHA256.HashData(stream.ToArray())).ToLowerInvariant();
}

public sealed class ComplexMatrix
{
    public ComplexMatrix(double[,] re, double[,] im)
    {
        if (re.GetLength(0) == 0 ||
            re.GetLength(0) != re.GetLength(1) ||
            re.GetLength(0) != im.GetLength(0) ||
            re.GetLength(1) != im.GetLength(1))
            throw new InvalidDataException("Complex matrix must be non-empty and square with matching shapes.");
        Re = re;
        Im = im;
    }

    [JsonIgnore]
    public double[,] Re { get; }
    [JsonIgnore]
    public double[,] Im { get; }
    public int Size => Re.GetLength(0);

    public static ComplexMatrix FromFlatInterleaved(double[] values, int size)
    {
        var re = new double[size, size];
        var im = new double[size, size];
        for (int row = 0; row < size; row++)
            for (int col = 0; col < size; col++)
            {
                int index = 2 * (row * size + col);
                re[row, col] = values[index];
                im[row, col] = values[index + 1];
            }
        return new(re, im);
    }

    public double[] ToFlatInterleaved()
    {
        var result = new double[2 * Size * Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
            {
                int index = 2 * (row * Size + col);
                result[index] = Re[row, col];
                result[index + 1] = Im[row, col];
            }
        return result;
    }

    public double[] Apply(double[] vector)
    {
        if (vector.Length != 2 * Size)
            throw new InvalidDataException("Matrix and vector sizes must match.");
        var result = new double[vector.Length];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
            {
                result[2 * row] += Re[row, col] * vector[2 * col] - Im[row, col] * vector[2 * col + 1];
                result[2 * row + 1] += Re[row, col] * vector[2 * col + 1] + Im[row, col] * vector[2 * col];
            }
        return result;
    }

    public ComplexMatrix ScaleRows(double[] weights)
    {
        if (weights.Length != 2 * Size)
            throw new InvalidDataException("Weight and matrix dimensions must match.");
        var re = new double[Size, Size];
        var im = new double[Size, Size];
        for (int row = 0; row < Size; row++)
            for (int col = 0; col < Size; col++)
            {
                re[row, col] = Re[row, col] / weights[2 * row];
                im[row, col] = Im[row, col] / weights[2 * row];
            }
        return new(re, im);
    }
}

public sealed record ComplexValue(double Real, double Imaginary);
public sealed record VariationOperatorAudit(ComplexMatrix DeltaK, ComplexMatrix DeltaA, VariationOperatorRecord Record);

public sealed class ReplayFingerprint
{
    public required string PersistedGeometryArtifactHash { get; init; }
    public required string DerivedMeshGeometryHash { get; init; }
    public required string MassPsiWeightHash { get; init; }
}

public sealed class CarrierMetadataAudit
{
    public required bool MetadataAvailable { get; init; }
    public required string CarrierTypeSourcePath { get; init; }
    public required string GeometrySourcePath { get; init; }
    public required string CarrierType { get; init; }
    public required string ExpectedCarrierType { get; init; }
    public required int AmbientEdgeCount { get; init; }
    public required int ExpectedAmbientEdgeCount { get; init; }
    public required int GaugeDimension { get; init; }
    public required int[] ConnectionShape { get; init; }
    public required int ConnectionVectorLength { get; init; }
    public required int ExpectedConnectionVectorLength { get; init; }
    public required bool Passed { get; init; }
}

public sealed class BosonModeAudit
{
    public required string ModePath { get; init; }
    public required string ConnectionCarrierType { get; init; }
    public required bool ConnectionCarrierTypeSupported { get; init; }
    public required double[] ModeVector { get; init; }
}

public sealed class BackgroundAudit
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMetadataPath { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required string BaseMatrixHash { get; init; }
    public required string LayoutPath { get; init; }
    public required bool KIsEuclideanHermitian { get; init; }
    public required double KEuclideanHermiticityRelativeResidual { get; init; }
    public required bool AIsMSelfAdjoint { get; init; }
    public required double AMAdjointRelativeResidual { get; init; }
    public required int[] ZeroRowColumnComplexDofs { get; init; }
    public required int ZeroRowColumnComplexDofCount { get; init; }
    public required int[] ExpectedZeroRowColumnComplexDofs { get; init; }
    public required bool ZeroRowColumnStructurePassed { get; init; }
    public required WeightedSolveAudit WeightedSolve { get; init; }
    public required string GeometryHash { get; init; }
    public required string MassWeightHash { get; init; }
    public required string SidecarPath { get; init; }
}

public sealed class WeightedSolveAudit
{
    public required string SolverName { get; init; }
    public required bool SolverConverged { get; init; }
    public required int SolverIterations { get; init; }
    public required IReadOnlyList<string> SolverNotes { get; init; }
    public required int RequestedModeCount { get; init; }
    public required int ModeCount { get; init; }
    public required string KernelFilter { get; init; }
    public required int KernelModeCount { get; init; }
    public required int GeneralizedResidualPassedCount { get; init; }
    public required int MNormPassedCount { get; init; }
    public required double MaxGeneralizedRelativeResidual { get; init; }
    public required double MaxMNormResidual { get; init; }
    public required double MaxMOrthonormalityResidual { get; init; }
    public required bool MOrthonormalityPassed { get; init; }
    public required bool AllQualityChecksPassed { get; init; }
    public required ShellAudit Shell { get; init; }
    public required IReadOnlyList<WeightedModeAudit> Modes { get; init; }
}

public sealed class ShellAudit
{
    public required double ExplicitKernelTolerance { get; init; }
    public required string GroupingToleranceFormula { get; init; }
    public required double LambdaMinNonzeroMagnitude { get; init; }
    public required double GroupingTolerance { get; init; }
    public required int[] ShellIndices { get; init; }
    public required int[] ExpectedShellIndices { get; init; }
    public required int ShellSize { get; init; }
    public required int ExpectedShellSize { get; init; }
    public required int SentinelIndex { get; init; }
    public required double SentinelEigenvalue { get; init; }
    public required bool SentinelOutsideShell { get; init; }
    public required bool ShellSelectionPassed { get; init; }
    public required IReadOnlyList<WeightedModeAudit> Modes { get; init; }
}

public sealed class WeightedModeAudit
{
    public required string ModeId { get; init; }
    public required int ModeIndex { get; init; }
    public required double Eigenvalue { get; init; }
    public required bool IsKernel { get; init; }
    public required double SolverReportedResidualNorm { get; init; }
    public required double GeneralizedKPsiEqualsLambdaMPsiRelativeResidual { get; init; }
    public required bool GeneralizedResidualPassed { get; init; }
    public required double MNorm { get; init; }
    public required double MNormResidual { get; init; }
    public required bool MNormPassed { get; init; }
    [JsonIgnore]
    public double[] Coefficients { get; init; } = [];
}

public sealed class VariationOperatorRecord
{
    public required bool DeltaKIsEuclideanHermitian { get; init; }
    public required double DeltaKEuclideanHermiticityRelativeResidual { get; init; }
    public required bool DeltaAIsMSelfAdjoint { get; init; }
    public required double DeltaAMAdjointRelativeResidual { get; init; }
    public required bool AllOperatorIdentitiesPassed { get; init; }
}

public sealed class ProjectedBlockAudit
{
    public required string Construction { get; init; }
    public required int[] ShellModeIndices { get; init; }
    public required ComplexValue[][] StiffnessProjectedBlock { get; init; }
    public required ComplexValue[][] WeightedProjectedBlock { get; init; }
    public required double PairingIdentityRelativeResidual { get; init; }
    public required bool PairingIdentityPassed { get; init; }
    public required BlockMetrics Metrics { get; init; }
}

public sealed class BlockMetrics
{
    public required int Dimension { get; init; }
    public required double FrobeniusNorm { get; init; }
    public required double FrobeniusNormSquared { get; init; }
    public required ComplexValue Trace { get; init; }
    public required ComplexValue TraceSquare { get; init; }
    public required ComplexValue Determinant { get; init; }
    public required double HermiticityRelativeResidual { get; init; }
    public required bool IsHermitian { get; init; }
    public required bool NonzeroFrobeniusNorm { get; init; }
    public required string BasisInvariantDisclosure { get; init; }
}

public sealed class InvariantParityAudit
{
    public required double FrobeniusNormScaleAwareResidual { get; init; }
    public required double TraceScaleAwareResidual { get; init; }
    public required double TraceSquareScaleAwareResidual { get; init; }
    public required double DeterminantScaleAwareResidual { get; init; }
    public required double MaxScaleAwareResidual { get; init; }
    public required bool Passed { get; init; }
}

public sealed class VariationAudit
{
    public required string VariationId { get; init; }
    public required string FermionBackgroundId { get; init; }
    public required string BosonModeId { get; init; }
    public required string PersistedVariationMetadataPath { get; init; }
    public required string PersistedVariationMatrixPath { get; init; }
    public required string AnalyticVariationSource { get; init; }
    public required string ConnectionCarrierType { get; init; }
    public required bool ConnectionCarrierTypeSupported { get; init; }
    public required int ConnectionVectorLength { get; init; }
    public required int ExpectedConnectionVectorLength { get; init; }
    public required bool ConnectionVectorLengthPassed { get; init; }
    public required VariationOperatorRecord PersistedOperator { get; init; }
    public required VariationOperatorRecord AnalyticOperator { get; init; }
    public required double AnalyticPersistedDeltaKRelativeResidual { get; init; }
    public required bool AnalyticPersistedDeltaKParityPassed { get; init; }
    public required ProjectedBlockAudit PersistedBlock { get; init; }
    public required ProjectedBlockAudit AnalyticBlock { get; init; }
    public required double PersistedAnalyticBlockRelativeResidual { get; init; }
    public required bool PersistedAnalyticBlockParityPassed { get; init; }
    public required bool ProjectedPairingIdentityPassed { get; init; }
    public required double MaxProjectedPairingIdentityRelativeResidual { get; init; }
    public required bool ProjectedBlockHermiticityPassed { get; init; }
    public required double MaxProjectedBlockHermiticityRelativeResidual { get; init; }
    public required bool NonzeroProjectedBlockFrobeniusPassed { get; init; }
    public required InvariantParityAudit InvariantParity { get; init; }
    public required ReplayFingerprint FingerprintBeforeReplay { get; init; }
    public required ReplayFingerprint FingerprintAfterReplay { get; init; }
    public required bool GeometryHashUnchangedThroughReplay { get; init; }
    public required bool MassWeightHashUnchangedThroughReplay { get; init; }
    public bool VariationPassed { get; set; }
    public required string SidecarPath { get; init; }
}
