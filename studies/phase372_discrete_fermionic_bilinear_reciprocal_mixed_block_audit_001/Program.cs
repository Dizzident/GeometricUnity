using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase372_discrete_fermionic_bilinear_reciprocal_mixed_block_audit_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string VariationDir = $"{Phase12Root}/fermions/couplings/variations";
const string ModeDir = $"{Phase12Root}/spectra/modes";
const string FermionDir = $"{Phase12Root}/fermions";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase371SummaryPath = "studies/phase371_discrete_connection_dirac_first_variation_coverage_audit_001/output/discrete_connection_dirac_first_variation_coverage_audit_summary.json";

const int ExpectedVariationCount = 24;
const int ExpectedBackgroundCount = 2;
const int ExpectedModesPerBackground = 12;
const int ExpectedDirectionCheckCount = ExpectedVariationCount * ExpectedModesPerBackground;
const double MatrixParityTolerance = 1e-8;
const double DirectionalParityTolerance = 1e-8;
const double CentralDerivativeTolerance = 1e-8;
const double ReciprocalIdentityTolerance = 1e-10;
const double AdjointIdentityTolerance = 1e-10;
double[] epsilonLadder = [1e-2, 1e-3, 1e-4, 1e-5, 1e-6];

var outputDir = Environment.GetEnvironmentVariable("PHASE372_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);
var variationOutputDir = Path.Combine(outputDir, "variations");
Directory.CreateDirectory(variationOutputDir);

using var phase371 = JsonDocument.Parse(File.ReadAllText(Phase371SummaryPath));
bool phase371PassedPrecursor =
    JsonBool(phase371.RootElement, "discreteConnectionDiracFirstVariationCoverageAuditPassed") is true &&
    JsonBool(phase371.RootElement, "discreteConnectionToDiracFirstVariationCoverageMaterialized") is true &&
    JsonInt(phase371.RootElement, "variationCount") == ExpectedVariationCount &&
    JsonInt(phase371.RootElement, "backgroundCount") == ExpectedBackgroundCount;
bool phase371NonpromotionalBoundaryVerified =
    JsonBool(phase371.RootElement, "discreteConnectionToDiracFirstVariationIsVo7BuildingBlock") is true &&
    JsonBool(phase371.RootElement, "discreteConnectionToDiracFirstVariationCompletesVo7") is false &&
    JsonBool(phase371.RootElement, "routeProvidesCompletedFermionicAction") is false &&
    JsonBool(phase371.RootElement, "routeProvidesExplicitYukawaFunctional") is false &&
    JsonBool(phase371.RootElement, "routeProvidesCoupledResidual") is false &&
    JsonBool(phase371.RootElement, "routeProvidesCompletedMixedLinearizationBlocks") is false &&
    JsonBool(phase371.RootElement, "routeProvidesMixedLinearizationGaugeCompatibilityIdentities") is false &&
    JsonBool(phase371.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") is false &&
    JsonBool(phase371.RootElement, "routeProvidesHiggsScalarSourceOperator") is false &&
    JsonBool(phase371.RootElement, "routeProvidesScalarProjectionTheorem") is false &&
    JsonBool(phase371.RootElement, "routeProvidesGeVUnitNormalization") is false &&
    JsonBool(phase371.RootElement, "routeCompletesBosonPredictions") is false &&
    JsonBool(phase371.RootElement, "canFillPhase201WzContract") is false &&
    JsonBool(phase371.RootElement, "canFillPhase201HiggsContract") is false &&
    JsonBool(phase371.RootElement, "canFillPhase256ObservedFieldExtractionContract") is false;

using var spinorDoc = JsonDocument.Parse(File.ReadAllText(SpinorRepresentationPath));
var spinorSpec = spinorDoc.RootElement.Deserialize<SpinorRepresentationSpec>(JsonOptions())
    ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "phase372-discrete-fermionic-bilinear-reciprocal-mixed-block-audit",
        Branch = new() { BranchId = "phase372-discrete-fermionic-bilinear-reciprocal-mixed-block-audit", SchemaVersion = "1.0" },
        Backend = "cpu-reference",
    });

var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int dimG = 3;
int spinorDim = spinorSpec.SpinorComponents;
int dofsPerCell = spinorDim * dimG;
double[] identityMassPsiWeights = MassPsiWeightsBuilder.BuildIdentity(mesh.VertexCount, dofsPerCell);
double[] meshVolumeMassPsiWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerCell);
var edgeLengths = new double[mesh.EdgeCount];
var edgeDirections = new double[mesh.EdgeCount][];
var cellsPerEdge = new int[mesh.EdgeCount][];
for (int edge = 0; edge < mesh.EdgeCount; edge++)
{
    edgeLengths[edge] = ComputeEdgeLength(mesh, edge);
    edgeDirections[edge] = ComputeEdgeDirection(mesh, edge);
    cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
}

var fermionModesByBackground = LoadFermionModesByBackground(FermionDir);
var candidateBackgrounds = fermionModesByBackground
    .OrderBy(pair => pair.Key, StringComparer.Ordinal)
    .Select(pair => BuildCandidateBackground(pair.Key, pair.Value))
    .ToArray();
var variationMetadataPaths = Directory
    .GetFiles(VariationDir, "variation-*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .ToArray();
var records = variationMetadataPaths.Select(BuildVariationAudit).ToArray();

int variationCount = records.Length;
int backgroundCount = records.Select(record => record.FermionBackgroundId).Distinct(StringComparer.Ordinal).Count();
int candidateBackgroundCount = candidateBackgrounds.Length;
int fermionDirectionCount = fermionModesByBackground.Values.Select(modes => modes.Count).Distinct().Single();
int directionalCheckCount = records.Sum(record => record.DirectionCheckCount);
int sidecarCount = records.Count(record => File.Exists(record.SidecarPath));
int matrixParityPassedCount = records.Count(record => record.MatrixParityPassed);
int directionalParityPassedCount = records.Sum(record => record.DirectionalParityPassedCount);
int reciprocalIdentityPassedCount = records.Sum(record => record.ReciprocalIdentityPassedCount);
int responsePairingParityPassedCount = records.Sum(record => record.ResponsePairingParityPassedCount);
int currentDirectionalDerivativeParityPassedCount = records.Sum(record => record.CurrentDirectionalDerivativeParityPassedCount);
int centralFiniteDifferenceConvergencePassedCount = records.Sum(record => record.CentralDerivativeConvergencePassedCount);
int hermitianAdjointIdentityPassedCount = records.Sum(record => record.AdjointIdentityPassedCount);
int meshVolumeWeightCentralFiniteDifferenceConvergencePassedCount =
    records.Sum(record => record.MeshVolumeWeightCentralDerivativeConvergencePassedCount);
bool expectedCoveragePresent =
    variationCount == ExpectedVariationCount &&
    backgroundCount == ExpectedBackgroundCount &&
    candidateBackgroundCount == ExpectedBackgroundCount &&
    fermionDirectionCount == ExpectedModesPerBackground &&
    directionalCheckCount == ExpectedDirectionCheckCount &&
    records.All(record => record.DirectionCheckCount == ExpectedModesPerBackground);
bool localDiscreteHermitianFermionicBilinearCandidateMaterialized =
    candidateBackgrounds.Length == ExpectedBackgroundCount &&
    candidateBackgrounds.All(record =>
        record.TargetBlindSourceSelection &&
        record.SourceModeIndex == 0 &&
        record.IdentityWeightBaseDiracMAdjointCompatible &&
        double.IsFinite(record.IdentityWeightControlCandidateBilinearValue));
bool reciprocalDiscreteBilinearSourceBlockCandidateMaterialized =
    localDiscreteHermitianFermionicBilinearCandidateMaterialized &&
    sidecarCount == ExpectedVariationCount;
bool analyticVsPersistedParityPassed =
    matrixParityPassedCount == ExpectedVariationCount &&
    directionalParityPassedCount == ExpectedDirectionCheckCount;
bool reciprocalDirectionalIdentityPassed = reciprocalIdentityPassedCount == ExpectedDirectionCheckCount;
bool centralDerivativeConvergencePassed = centralFiniteDifferenceConvergencePassedCount == ExpectedDirectionCheckCount;
bool hermitianAdjointParityPassed = hermitianAdjointIdentityPassedCount == ExpectedDirectionCheckCount;
bool identityWeightControlBranchPassed =
    expectedCoveragePresent &&
    reciprocalDiscreteBilinearSourceBlockCandidateMaterialized &&
    analyticVsPersistedParityPassed &&
    reciprocalDirectionalIdentityPassed &&
    centralDerivativeConvergencePassed &&
    hermitianAdjointParityPassed;
bool meshVolumeWeightDiagnosticMaterialized =
    records.Length == ExpectedVariationCount &&
    records.All(record => record.MeshVolumeWeightDiagnosticMaterialized) &&
    candidateBackgrounds.All(record => record.MeshVolumeWeightDiagnosticMaterialized);
bool meshVolumeWeightBranchCompatible =
    meshVolumeWeightDiagnosticMaterialized &&
    candidateBackgrounds.All(record => record.MeshVolumeWeightBaseDiracMAdjointCompatible) &&
    records.All(record => record.MeshVolumeWeightVariationMAdjointCompatible);
string? meshVolumeWeightActionableObstruction = meshVolumeWeightBranchCompatible
    ? null
    : "The mesh-volume M_psi diagnostic is not M-self-adjoint on the current toy branch. Rebuild an M_psi-compatible Dirac branch and solve matching M_psi-compatible fermion modes before making a physical weighted-pairing claim.";
double maxMatrixRelativeResidual = records.Max(record => record.MatrixRelativeResidual);
double maxDirectionalAnalyticVsPersistedScaleAwareResidual =
    records.Max(record => record.MaxDirectionalAnalyticVsPersistedScaleAwareResidual);
double maxReciprocalIdentityScaleAwareResidual =
    records.Max(record => record.MaxReciprocalIdentityScaleAwareResidual);
double maxCentralDerivativeScaleAwareResidual =
    records.Max(record => record.MaxCentralDerivativeScaleAwareResidual);
double maxAdjointIdentityScaleAwareResidual =
    records.Max(record => record.MaxAdjointIdentityScaleAwareResidual);
double maxMeshVolumeWeightVariationMAdjointRelativeResidual =
    records.Max(record => record.MaxMeshVolumeWeightVariationMAdjointRelativeResidual);
double maxMeshVolumeWeightCentralDerivativeScaleAwareResidual =
    records.Max(record => record.MaxMeshVolumeWeightCentralDerivativeScaleAwareResidual);
double maxMeshVolumeWeightBaseDiracMAdjointRelativeResidual =
    candidateBackgrounds.Max(record => record.MeshVolumeWeightBaseDiracMAdjointRelativeResidual);

const bool phase12PersistedSolveUsesIdentityMassPsi = true;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesFixedFermionicOperatorBranch = false;
const bool routeProvidesExplicitYukawaFunctional = false;
const bool routeProvidesSolvedYukawaCouplingMap = false;
const bool routeProvidesCoupledResidual = false;
const bool routeProvidesCompletedMixedLinearizationBlocks = false;
const bool routeProvidesMixedLinearizationGaugeCompatibilityIdentities = false;
const bool routeProvidesDirectTargetIndependentWzBridgeSourceLaw = false;
const bool routeProvidesHiggsScalarSourceOperator = false;
const bool routeProvidesScalarProjectionTheorem = false;
const bool routeProvidesGeVUnitNormalization = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

bool discreteFermionicBilinearReciprocalMixedBlockAuditPassed =
    phase371PassedPrecursor &&
    phase371NonpromotionalBoundaryVerified &&
    identityWeightControlBranchPassed &&
    meshVolumeWeightDiagnosticMaterialized &&
    phase12PersistedSolveUsesIdentityMassPsi &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesFixedFermionicOperatorBranch &&
    !routeProvidesExplicitYukawaFunctional &&
    !routeProvidesSolvedYukawaCouplingMap &&
    !routeProvidesCoupledResidual &&
    !routeProvidesCompletedMixedLinearizationBlocks &&
    !routeProvidesMixedLinearizationGaugeCompatibilityIdentities &&
    !routeProvidesDirectTargetIndependentWzBridgeSourceLaw &&
    !routeProvidesHiggsScalarSourceOperator &&
    !routeProvidesScalarProjectionTheorem &&
    !routeProvidesGeVUnitNormalization &&
    !routePromotesWzMasses &&
    !routePromotesHiggsMass &&
    !routeCompletesBosonPredictions &&
    !canFillPhase201WzContract &&
    !canFillPhase201HiggsContract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = discreteFermionicBilinearReciprocalMixedBlockAuditPassed
    ? "identity-weight-control-reciprocal-discrete-bilinear-source-block-candidate-materialized-vo7-building-block-only"
    : "reciprocal-discrete-bilinear-source-block-candidate-audit-blocked";
string decision = discreteFermionicBilinearReciprocalMixedBlockAuditPassed
    ? "The local reciprocal discrete bilinear source-block candidate is materialized across the persisted Phase12 identity-weight control branch. Its analytical and persisted first-variation matrices agree, all 288 branch-local directional checks satisfy the reciprocal central-derivative and Hermitian adjoint identities, and the Phase371 nonpromotional boundary is retained. Mesh-volume M_psi diagnostics are reported separately and do not establish a physical M_psi-compatible branch. This is a VO-7 building block, not completed VO-7 and not a physical prediction route."
    : "Do not promote the reciprocal discrete bilinear source-block candidate until the Phase371 boundary, 24 persisted variations, 288 direction checks, matrix parity, central-derivative ladder, and Hermitian adjoint checks all pass.";

bool phase371ConnectionToDiracPrecursorPresent = phase371PassedPrecursor;
var result = new
{
    phaseId = "phase372-discrete-fermionic-bilinear-reciprocal-mixed-block-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    discreteFermionicBilinearReciprocalMixedBlockAuditPassed,
    phase371ConnectionToDiracPrecursorPresent,
    phase371PassedPrecursor,
    phase371NonpromotionalBoundaryVerified,
    localDiscreteHermitianFermionicBilinearCandidateMaterialized,
    reciprocalDiscreteBilinearSourceBlockCandidateMaterialized,
    reciprocalDiscreteBilinearSourceBlockCandidateIsVo7BuildingBlock = true,
    reciprocalDiscreteBilinearSourceBlockCandidateCompletesVo7 = false,
    identityWeightControlBranchPassed,
    meshVolumeWeightDiagnosticMaterialized,
    meshVolumeWeightBranchCompatible,
    meshVolumeWeightActionableObstruction,
    phase12PersistedSolveUsesIdentityMassPsi,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    implementedObjectClassification = "reciprocal discrete bilinear source-block candidate / VO-7 building block",
    implementedObjectCompletesVo7 = false,
    candidateDefinition = "S_F^candidate(omega, psi) = Re<psi, D_h(omega) psi>",
    currentComponentDefinition = "J_k(psi) = Re<psi, delta_D[b_k] psi>",
    responsePairingDefinition = "2 Re<chi_i, delta_D[b_k] psi>",
    reciprocalCurrentDirectionalDerivativeDefinition = "Re<chi_i, delta_D[b_k] psi> + Re<psi, delta_D[b_k] chi_i>",
    centralDifferenceDefinition = "(J_k(psi + epsilon chi_i) - J_k(psi - epsilon chi_i)) / (2 epsilon)",
    adjointIdentityDefinition = "<chi_i, A psi> = conjugate(<psi, A chi_i>)",
    expectedCoveragePresent,
    analyticVsPersistedParityPassed,
    reciprocalDirectionalIdentityPassed,
    centralDerivativeConvergencePassed,
    hermitianAdjointParityPassed,
    variationCount,
    backgroundCount,
    candidateBackgroundCount,
    fermionDirectionCount,
    directionalCheckCount,
    sidecarCount,
    matrixParityPassedCount,
    directionalParityPassedCount,
    reciprocalIdentityPassedCount,
    responsePairingParityPassedCount,
    currentDirectionalDerivativeParityPassedCount,
    centralFiniteDifferenceConvergencePassedCount,
    hermitianAdjointIdentityPassedCount,
    meshVolumeWeightCentralFiniteDifferenceConvergencePassedCount,
    expectedVariationCount = ExpectedVariationCount,
    expectedBackgroundCount = ExpectedBackgroundCount,
    expectedModesPerBackground = ExpectedModesPerBackground,
    expectedDirectionCheckCount = ExpectedDirectionCheckCount,
    epsilonLadder,
    matrixParityTolerance = MatrixParityTolerance,
    directionalParityTolerance = DirectionalParityTolerance,
    reciprocalIdentityTolerance = ReciprocalIdentityTolerance,
    centralDerivativeTolerance = CentralDerivativeTolerance,
    adjointIdentityTolerance = AdjointIdentityTolerance,
    maxMatrixRelativeResidual,
    maxDirectionalAnalyticVsPersistedScaleAwareResidual,
    maxReciprocalIdentityScaleAwareResidual,
    maxCentralDerivativeScaleAwareResidual,
    maxAdjointIdentityScaleAwareResidual,
    maxMeshVolumeWeightVariationMAdjointRelativeResidual,
    maxMeshVolumeWeightCentralDerivativeScaleAwareResidual,
    maxMeshVolumeWeightBaseDiracMAdjointRelativeResidual,
    sourceSelection = new
    {
        targetBlind = true,
        rule = "select persisted branch-local fermion mode with minimum modeIndex; no physical boson target, observed mass, or external calibration consulted",
        directionRule = "evaluate every persisted branch-local fermion mode chi_i ordered by modeIndex",
    },
    pairingVariants = new
    {
        identityWeightControl = new
        {
            builder = "MassPsiWeightsBuilder.BuildIdentity(mesh.VertexCount, spinorDim * dimG)",
            requiredPassingCandidateAudit = true,
            presentedAsPhysical = false,
            phase12PersistedSolveUsesIdentityMassPsi,
        },
        meshVolumeWeightDiagnostic = new
        {
            builder = "MassPsiWeightsBuilder.BuildFromMesh(mesh, spinorDim * dimG)",
            diagnosticOnly = true,
            meshVolumeWeightBranchCompatible,
            meshVolumeWeightActionableObstruction,
        },
        independentSourceDerivativeConvention = "record independent psi-bar derivative <chi_i, A psi>_M and psi derivative <psi, A chi_i>_M separately from the Hermitian real-form shortcut 2 Re<chi_i, A psi>_M",
    },
    routeProvidesCompletedFermionicAction,
    routeProvidesFixedFermionicOperatorBranch,
    routeProvidesExplicitYukawaFunctional,
    routeProvidesSolvedYukawaCouplingMap,
    routeProvidesCoupledResidual,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
    routeProvidesHiggsScalarSourceOperator,
    routeProvidesScalarProjectionTheorem,
    routeProvidesGeVUnitNormalization,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
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
        "not a completed GU fermionic action",
        "not a fixed GU fermionic branch",
        "no explicit Yukawa",
        "no solved coupling map",
        "no coupled residual",
        "no completed mixed blocks",
        "no gauge compatibility identities",
        "no W/Z bridge law",
        "no Higgs scalar operator",
        "no scalar projection theorem",
        "no GeV normalization",
        "no physical predictions",
        "no Phase201 or Phase256 fill",
    },
    candidateBackgrounds,
    records,
    sourceEvidence = new
    {
        phase371SummaryPath = Phase371SummaryPath,
        phase12Root = Phase12Root,
        variationDir = VariationDir,
        modeDir = ModeDir,
        fermionDir = FermionDir,
        spinorRepresentationPath = SpinorRepresentationPath,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "discrete_fermionic_bilinear_reciprocal_mixed_block_audit.json");
string summaryPath = Path.Combine(outputDir, "discrete_fermionic_bilinear_reciprocal_mixed_block_audit_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    summaryPath,
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.generatedAt,
        terminalStatus,
        discreteFermionicBilinearReciprocalMixedBlockAuditPassed,
        phase371ConnectionToDiracPrecursorPresent,
        phase371PassedPrecursor,
        phase371NonpromotionalBoundaryVerified,
        localDiscreteHermitianFermionicBilinearCandidateMaterialized,
        reciprocalDiscreteBilinearSourceBlockCandidateMaterialized,
        reciprocalDiscreteBilinearSourceBlockCandidateIsVo7BuildingBlock = true,
        reciprocalDiscreteBilinearSourceBlockCandidateCompletesVo7 = false,
        identityWeightControlBranchPassed,
        meshVolumeWeightDiagnosticMaterialized,
        meshVolumeWeightBranchCompatible,
        meshVolumeWeightActionableObstruction,
        phase12PersistedSolveUsesIdentityMassPsi,
        routeProvidesPhysicalMassPsiCompatibleBranch,
        result.implementedObjectClassification,
        result.implementedObjectCompletesVo7,
        expectedCoveragePresent,
        analyticVsPersistedParityPassed,
        reciprocalDirectionalIdentityPassed,
        centralDerivativeConvergencePassed,
        hermitianAdjointParityPassed,
        variationCount,
        backgroundCount,
        candidateBackgroundCount,
        fermionDirectionCount,
        directionalCheckCount,
        sidecarCount,
        matrixParityPassedCount,
        directionalParityPassedCount,
        reciprocalIdentityPassedCount,
        responsePairingParityPassedCount,
        currentDirectionalDerivativeParityPassedCount,
        centralFiniteDifferenceConvergencePassedCount,
        hermitianAdjointIdentityPassedCount,
        meshVolumeWeightCentralFiniteDifferenceConvergencePassedCount,
        maxMatrixRelativeResidual,
        maxDirectionalAnalyticVsPersistedScaleAwareResidual,
        maxReciprocalIdentityScaleAwareResidual,
        maxCentralDerivativeScaleAwareResidual,
        maxAdjointIdentityScaleAwareResidual,
        maxMeshVolumeWeightVariationMAdjointRelativeResidual,
        maxMeshVolumeWeightCentralDerivativeScaleAwareResidual,
        maxMeshVolumeWeightBaseDiracMAdjointRelativeResidual,
        result.routeProvidesCompletedFermionicAction,
        result.routeProvidesFixedFermionicOperatorBranch,
        result.routeProvidesExplicitYukawaFunctional,
        result.routeProvidesSolvedYukawaCouplingMap,
        result.routeProvidesCoupledResidual,
        result.routeProvidesCompletedMixedLinearizationBlocks,
        result.routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
        result.routeProvidesDirectTargetIndependentWzBridgeSourceLaw,
        result.routeProvidesHiggsScalarSourceOperator,
        result.routeProvidesScalarProjectionTheorem,
        result.routeProvidesGeVUnitNormalization,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.predictionContractImpact,
        result.pairingVariants,
        result.explicitCandidateOnlyNonclaims,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"discreteFermionicBilinearReciprocalMixedBlockAuditPassed={discreteFermionicBilinearReciprocalMixedBlockAuditPassed}");
Console.WriteLine($"phase371PassedPrecursor={phase371PassedPrecursor}");
Console.WriteLine($"phase371NonpromotionalBoundaryVerified={phase371NonpromotionalBoundaryVerified}");
Console.WriteLine($"variationCount={variationCount}");
Console.WriteLine($"backgroundCount={backgroundCount}");
Console.WriteLine($"directionalCheckCount={directionalCheckCount}");
Console.WriteLine($"sidecarCount={sidecarCount}");
Console.WriteLine($"matrixParityPassedCount={matrixParityPassedCount}");
Console.WriteLine($"responsePairingParityPassedCount={responsePairingParityPassedCount}");
Console.WriteLine($"currentDirectionalDerivativeParityPassedCount={currentDirectionalDerivativeParityPassedCount}");
Console.WriteLine($"centralFiniteDifferenceConvergencePassedCount={centralFiniteDifferenceConvergencePassedCount}");
Console.WriteLine($"hermitianAdjointIdentityPassedCount={hermitianAdjointIdentityPassedCount}");
Console.WriteLine($"meshVolumeWeightBranchCompatible={meshVolumeWeightBranchCompatible}");
Console.WriteLine($"maxMatrixRelativeResidual={maxMatrixRelativeResidual:R}");
Console.WriteLine($"maxDirectionalAnalyticVsPersistedScaleAwareResidual={maxDirectionalAnalyticVsPersistedScaleAwareResidual:R}");
Console.WriteLine($"maxReciprocalIdentityScaleAwareResidual={maxReciprocalIdentityScaleAwareResidual:R}");
Console.WriteLine($"maxCentralDerivativeScaleAwareResidual={maxCentralDerivativeScaleAwareResidual:R}");
Console.WriteLine($"maxAdjointIdentityScaleAwareResidual={maxAdjointIdentityScaleAwareResidual:R}");
Console.WriteLine($"maxMeshVolumeWeightVariationMAdjointRelativeResidual={maxMeshVolumeWeightVariationMAdjointRelativeResidual:R}");
Console.WriteLine($"maxMeshVolumeWeightBaseDiracMAdjointRelativeResidual={maxMeshVolumeWeightBaseDiracMAdjointRelativeResidual:R}");
Console.WriteLine($"maxMeshVolumeWeightCentralDerivativeScaleAwareResidual={maxMeshVolumeWeightCentralDerivativeScaleAwareResidual:R}");
Console.WriteLine($"summaryPath={summaryPath}");

CandidateBackgroundRecord BuildCandidateBackground(string backgroundId, IReadOnlyList<FermionModeSnapshot> fermionModes)
{
    if (fermionModes.Count != ExpectedModesPerBackground)
        throw new InvalidDataException($"Expected {ExpectedModesPerBackground} fermion modes for {backgroundId}, found {fermionModes.Count}.");

    var sourceMode = fermionModes.OrderBy(mode => mode.ModeIndex).First();
    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    int[] shape = metadataDoc.RootElement.GetProperty("matrixShape").EnumerateArray().Select(value => value.GetInt32()).ToArray();
    if (shape.Length != 2 || shape[0] != shape[1])
        throw new InvalidDataException($"Expected a square base Dirac matrix for {backgroundId}.");
    string matrixPath = Path.Combine(FermionDir, matrixRef);
    var (re, im) = LoadFlatInterleavedMatrix(matrixPath, shape[0]);
    double identityWeightControlCandidateBilinearValue =
        CurrentValue(re, im, sourceMode.EigenvectorCoefficients, identityMassPsiWeights);
    double meshVolumeWeightDiagnosticCandidateBilinearValue =
        CurrentValue(re, im, sourceMode.EigenvectorCoefficients, meshVolumeMassPsiWeights);
    double identityWeightBaseDiracMAdjointRelativeResidual = MAdjointRelativeResidual(re, im, identityMassPsiWeights);
    double meshVolumeWeightBaseDiracMAdjointRelativeResidual = MAdjointRelativeResidual(re, im, meshVolumeMassPsiWeights);
    return new()
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMetadataPath = metadataPath,
        BaseDiracMatrixPath = matrixPath,
        SourceModeId = sourceMode.ModeId,
        SourceModeIndex = sourceMode.ModeIndex,
        TargetBlindSourceSelection = true,
        IdentityWeightControlCandidateBilinearValue = identityWeightControlCandidateBilinearValue,
        MeshVolumeWeightDiagnosticCandidateBilinearValue = meshVolumeWeightDiagnosticCandidateBilinearValue,
        IdentityWeightBaseDiracMAdjointRelativeResidual = identityWeightBaseDiracMAdjointRelativeResidual,
        IdentityWeightBaseDiracMAdjointCompatible = identityWeightBaseDiracMAdjointRelativeResidual <= AdjointIdentityTolerance,
        MeshVolumeWeightDiagnosticMaterialized = true,
        MeshVolumeWeightBaseDiracMAdjointRelativeResidual = meshVolumeWeightBaseDiracMAdjointRelativeResidual,
        MeshVolumeWeightBaseDiracMAdjointCompatible = meshVolumeWeightBaseDiracMAdjointRelativeResidual <= AdjointIdentityTolerance,
        CandidateDefinition = "S_F^candidate(omega, psi) = Re<psi, D_h(omega) psi>",
    };
}

VariationAuditRecord BuildVariationAudit(string metadataPath)
{
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    var metadata = metadataDoc.RootElement;
    string variationId = RequiredString(metadata, "variationId");
    string bosonModeId = RequiredString(metadata, "bosonModeId");
    string fermionBackgroundId = RequiredString(metadata, "fermionBackgroundId");
    string normalizationConvention = RequiredString(metadata, "normalizationConvention");
    string variationMethod = RequiredString(metadata, "variationMethod");
    double persistedFiniteDifferenceEpsilon = RequiredDouble(metadata, "finiteDifferenceEpsilon");
    string matrixPath = Path.Combine(VariationDir, RequiredString(metadata, "matrixArtifactRef"));
    string modePath = Path.Combine(ModeDir, $"{bosonModeId}.json");
    double[] modeVector = LoadModeVector(modePath);

    using var matrixDoc = JsonDocument.Parse(File.ReadAllText(matrixPath));
    var persistedRe = LoadMatrix(matrixDoc.RootElement.GetProperty("real"));
    var persistedIm = LoadMatrix(matrixDoc.RootElement.GetProperty("imag"));
    ValidateSameShape(persistedRe, persistedIm, matrixPath);
    var (analyticRe, analyticIm) = DiracVariationComputer.ComputeAnalytical(
        modeVector,
        gammas,
        mesh.VertexCount,
        spinorDim,
        dimG,
        edgeLengths,
        cellsPerEdge,
        edgeDirections);
    ValidateSameShape(persistedRe, analyticRe, matrixPath);
    ValidateSameShape(persistedIm, analyticIm, matrixPath);

    double matrixRelativeResidual = RelativeResidual(persistedRe, persistedIm, analyticRe, analyticIm);
    bool matrixParityPassed = matrixRelativeResidual <= MatrixParityTolerance;
    double identityWeightAnalyticVariationMAdjointRelativeResidual =
        MAdjointRelativeResidual(analyticRe, analyticIm, identityMassPsiWeights);
    double identityWeightPersistedVariationMAdjointRelativeResidual =
        MAdjointRelativeResidual(persistedRe, persistedIm, identityMassPsiWeights);
    double meshVolumeWeightAnalyticVariationMAdjointRelativeResidual =
        MAdjointRelativeResidual(analyticRe, analyticIm, meshVolumeMassPsiWeights);
    double meshVolumeWeightPersistedVariationMAdjointRelativeResidual =
        MAdjointRelativeResidual(persistedRe, persistedIm, meshVolumeMassPsiWeights);
    bool meshVolumeWeightVariationMAdjointCompatible =
        meshVolumeWeightAnalyticVariationMAdjointRelativeResidual <= AdjointIdentityTolerance &&
        meshVolumeWeightPersistedVariationMAdjointRelativeResidual <= AdjointIdentityTolerance;
    if (!fermionModesByBackground.TryGetValue(fermionBackgroundId, out var fermionModes))
        throw new InvalidDataException($"Missing fermion modes for {fermionBackgroundId}.");
    var sourceMode = fermionModes.OrderBy(mode => mode.ModeIndex).First();
    var rows = fermionModes
        .OrderBy(mode => mode.ModeIndex)
        .Select(mode => BuildDirectionRow(sourceMode, mode, analyticRe, analyticIm, persistedRe, persistedIm))
        .ToArray();

    int directionalParityPassedCount = rows.Count(row => row.AnalyticVsPersistedParityPassed);
    int reciprocalIdentityPassedCount = rows.Count(row => row.ReciprocalIdentityPassed);
    int responsePairingParityPassedCount = rows.Count(row => row.ResponsePairingParityPassed);
    int currentDirectionalDerivativeParityPassedCount = rows.Count(row => row.CurrentDirectionalDerivativeParityPassed);
    int centralDerivativeConvergencePassedCount = rows.Count(row => row.CentralDerivativeConvergencePassed);
    int adjointIdentityPassedCount = rows.Count(row => row.AdjointIdentityPassed);
    int meshVolumeWeightCentralDerivativeConvergencePassedCount =
        rows.Count(row => row.MeshVolumeWeightCentralDerivativeConvergencePassed);
    double maxDirectionalAnalyticVsPersistedScaleAwareResidual =
        rows.Max(row => row.AnalyticVsPersistedMaxScaleAwareResidual);
    double maxReciprocalIdentityScaleAwareResidual =
        rows.Max(row => Math.Max(row.Analytic.ReciprocalIdentityScaleAwareResidual, row.PersistedFiniteDifference.ReciprocalIdentityScaleAwareResidual));
    double maxCentralDerivativeScaleAwareResidual =
        rows.Max(row => Math.Max(row.Analytic.MaxCentralDerivativeScaleAwareResidual, row.PersistedFiniteDifference.MaxCentralDerivativeScaleAwareResidual));
    double maxAdjointIdentityScaleAwareResidual =
        rows.Max(row => Math.Max(row.Analytic.AdjointIdentityScaleAwareResidual, row.PersistedFiniteDifference.AdjointIdentityScaleAwareResidual));
    double maxMeshVolumeWeightVariationMAdjointRelativeResidual =
        Math.Max(meshVolumeWeightAnalyticVariationMAdjointRelativeResidual, meshVolumeWeightPersistedVariationMAdjointRelativeResidual);
    double maxMeshVolumeWeightCentralDerivativeScaleAwareResidual =
        rows.Max(row => Math.Max(row.MeshVolumeWeightAnalytic.MaxCentralDerivativeScaleAwareResidual, row.MeshVolumeWeightPersistedFiniteDifference.MaxCentralDerivativeScaleAwareResidual));
    bool variationPassed =
        matrixParityPassed &&
        rows.Length == ExpectedModesPerBackground &&
        directionalParityPassedCount == ExpectedModesPerBackground &&
        reciprocalIdentityPassedCount == ExpectedModesPerBackground &&
        centralDerivativeConvergencePassedCount == ExpectedModesPerBackground &&
        adjointIdentityPassedCount == ExpectedModesPerBackground;
    string sidecarPath = Path.Combine(variationOutputDir, $"{variationId}-reciprocal-source-block.json");

    File.WriteAllText(
        sidecarPath,
        JsonSerializer.Serialize(new
        {
            variationId,
            bosonModeId,
            fermionBackgroundId,
            implementedObjectClassification = "reciprocal discrete bilinear source-block candidate / VO-7 building block",
            implementedObjectCompletesVo7 = false,
            sourceModeId = sourceMode.ModeId,
            sourceModeIndex = sourceMode.ModeIndex,
            targetBlindSourceSelection = true,
            sourceSelectionRule = "select persisted branch-local fermion mode with minimum modeIndex; no physical boson target, observed mass, or external calibration consulted",
            directionSelectionRule = "evaluate every persisted branch-local fermion mode chi_i ordered by modeIndex",
            normalizationConvention,
            variationMethod,
            persistedFiniteDifferenceEpsilon,
            epsilonLadder,
            pairingVariants = new
            {
                identityWeightControl = "M_psi = I from MassPsiWeightsBuilder.BuildIdentity(mesh.VertexCount, spinorDim * dimG); required passing Phase12 control branch, not presented as physical",
                meshVolumeWeightDiagnostic = "M_psi from MassPsiWeightsBuilder.BuildFromMesh(mesh, spinorDim * dimG); diagnostic only until an M_psi-compatible Dirac branch and modes are rebuilt",
                independentSources = "independent psi-bar and psi source derivatives are recorded separately from the Hermitian real-form shortcut 2 Re<chi_i, A psi>",
            },
            analyticMatrixSource = "Gu.Phase4.Couplings.DiracVariationComputer.ComputeAnalytical",
            persistedFiniteDifferenceMatrixPath = matrixPath,
            matrixRelativeResidual,
            matrixParityTolerance = MatrixParityTolerance,
            matrixParityPassed,
            directionCheckCount = rows.Length,
            directionalParityPassedCount,
            reciprocalIdentityPassedCount,
            responsePairingParityPassedCount,
            currentDirectionalDerivativeParityPassedCount,
            centralDerivativeConvergencePassedCount,
            adjointIdentityPassedCount,
            identityWeightAnalyticVariationMAdjointRelativeResidual,
            identityWeightPersistedVariationMAdjointRelativeResidual,
            meshVolumeWeightDiagnosticMaterialized = true,
            meshVolumeWeightAnalyticVariationMAdjointRelativeResidual,
            meshVolumeWeightPersistedVariationMAdjointRelativeResidual,
            meshVolumeWeightVariationMAdjointCompatible,
            meshVolumeWeightCentralDerivativeConvergencePassedCount,
            maxDirectionalAnalyticVsPersistedScaleAwareResidual,
            maxReciprocalIdentityScaleAwareResidual,
            maxCentralDerivativeScaleAwareResidual,
            maxAdjointIdentityScaleAwareResidual,
            maxMeshVolumeWeightVariationMAdjointRelativeResidual,
            maxMeshVolumeWeightCentralDerivativeScaleAwareResidual,
            variationPassed,
            rows,
        }, JsonOptions()));

    return new()
    {
        VariationId = variationId,
        BosonModeId = bosonModeId,
        FermionBackgroundId = fermionBackgroundId,
        SourceModeId = sourceMode.ModeId,
        SourceModeIndex = sourceMode.ModeIndex,
        TargetBlindSourceSelection = true,
        SidecarPath = sidecarPath,
        MatrixRelativeResidual = matrixRelativeResidual,
        MatrixParityPassed = matrixParityPassed,
        DirectionCheckCount = rows.Length,
        DirectionalParityPassedCount = directionalParityPassedCount,
        ReciprocalIdentityPassedCount = reciprocalIdentityPassedCount,
        ResponsePairingParityPassedCount = responsePairingParityPassedCount,
        CurrentDirectionalDerivativeParityPassedCount = currentDirectionalDerivativeParityPassedCount,
        CentralDerivativeConvergencePassedCount = centralDerivativeConvergencePassedCount,
        AdjointIdentityPassedCount = adjointIdentityPassedCount,
        MeshVolumeWeightDiagnosticMaterialized = true,
        MeshVolumeWeightVariationMAdjointCompatible = meshVolumeWeightVariationMAdjointCompatible,
        MeshVolumeWeightCentralDerivativeConvergencePassedCount = meshVolumeWeightCentralDerivativeConvergencePassedCount,
        MaxDirectionalAnalyticVsPersistedScaleAwareResidual = maxDirectionalAnalyticVsPersistedScaleAwareResidual,
        MaxReciprocalIdentityScaleAwareResidual = maxReciprocalIdentityScaleAwareResidual,
        MaxCentralDerivativeScaleAwareResidual = maxCentralDerivativeScaleAwareResidual,
        MaxAdjointIdentityScaleAwareResidual = maxAdjointIdentityScaleAwareResidual,
        MaxMeshVolumeWeightVariationMAdjointRelativeResidual = maxMeshVolumeWeightVariationMAdjointRelativeResidual,
        MaxMeshVolumeWeightCentralDerivativeScaleAwareResidual = maxMeshVolumeWeightCentralDerivativeScaleAwareResidual,
        VariationPassed = variationPassed,
    };
}

DirectionRow BuildDirectionRow(
    FermionModeSnapshot sourceMode,
    FermionModeSnapshot directionMode,
    double[,] analyticRe,
    double[,] analyticIm,
    double[,] persistedRe,
    double[,] persistedIm)
{
    var analytic = BuildMatrixDirectionDiagnostic(analyticRe, analyticIm, sourceMode.EigenvectorCoefficients, directionMode.EigenvectorCoefficients, identityMassPsiWeights);
    var persisted = BuildMatrixDirectionDiagnostic(persistedRe, persistedIm, sourceMode.EigenvectorCoefficients, directionMode.EigenvectorCoefficients, identityMassPsiWeights);
    var meshVolumeWeightAnalytic = BuildMatrixDirectionDiagnostic(analyticRe, analyticIm, sourceMode.EigenvectorCoefficients, directionMode.EigenvectorCoefficients, meshVolumeMassPsiWeights);
    var meshVolumeWeightPersisted = BuildMatrixDirectionDiagnostic(persistedRe, persistedIm, sourceMode.EigenvectorCoefficients, directionMode.EigenvectorCoefficients, meshVolumeMassPsiWeights);
    double responsePairingResidual = ScaleAwareResidual(analytic.ResponsePairing, persisted.ResponsePairing);
    double reciprocalDerivativeResidual = ScaleAwareResidual(analytic.ReciprocalCurrentDirectionalDerivative, persisted.ReciprocalCurrentDirectionalDerivative);
    double centralDerivativeResidual = analytic.CentralDifferenceLadder
        .Zip(persisted.CentralDifferenceLadder)
        .Max(pair => ScaleAwareResidual(pair.First.CentralDifferenceDerivative, pair.Second.CentralDifferenceDerivative));
    double analyticVsPersistedMaxScaleAwareResidual =
        Math.Max(responsePairingResidual, Math.Max(reciprocalDerivativeResidual, centralDerivativeResidual));
    bool responsePairingParityPassed = responsePairingResidual <= DirectionalParityTolerance;
    bool currentDirectionalDerivativeParityPassed = reciprocalDerivativeResidual <= DirectionalParityTolerance;
    bool analyticVsPersistedParityPassed = analyticVsPersistedMaxScaleAwareResidual <= DirectionalParityTolerance;
    bool reciprocalIdentityPassed =
        analytic.ReciprocalIdentityPassed &&
        persisted.ReciprocalIdentityPassed;
    bool centralDerivativeConvergencePassed =
        analytic.CentralDerivativeConvergencePassed &&
        persisted.CentralDerivativeConvergencePassed;
    bool adjointIdentityPassed =
        analytic.AdjointIdentityPassed &&
        persisted.AdjointIdentityPassed;
    return new()
    {
        ChiModeId = directionMode.ModeId,
        ChiModeIndex = directionMode.ModeIndex,
        PsiSourceModeId = sourceMode.ModeId,
        PsiSourceModeIndex = sourceMode.ModeIndex,
        Analytic = analytic,
        PersistedFiniteDifference = persisted,
        MeshVolumeWeightAnalytic = meshVolumeWeightAnalytic,
        MeshVolumeWeightPersistedFiniteDifference = meshVolumeWeightPersisted,
        AnalyticVsPersistedResponsePairingScaleAwareResidual = responsePairingResidual,
        AnalyticVsPersistedReciprocalDerivativeScaleAwareResidual = reciprocalDerivativeResidual,
        AnalyticVsPersistedCentralDerivativeMaxScaleAwareResidual = centralDerivativeResidual,
        AnalyticVsPersistedMaxScaleAwareResidual = analyticVsPersistedMaxScaleAwareResidual,
        ResponsePairingParityPassed = responsePairingParityPassed,
        CurrentDirectionalDerivativeParityPassed = currentDirectionalDerivativeParityPassed,
        AnalyticVsPersistedParityPassed = analyticVsPersistedParityPassed,
        ReciprocalIdentityPassed = reciprocalIdentityPassed,
        CentralDerivativeConvergencePassed = centralDerivativeConvergencePassed,
        AdjointIdentityPassed = adjointIdentityPassed,
        MeshVolumeWeightCentralDerivativeConvergencePassed =
            meshVolumeWeightAnalytic.CentralDerivativeConvergencePassed &&
            meshVolumeWeightPersisted.CentralDerivativeConvergencePassed,
        DirectionCheckPassed =
            analyticVsPersistedParityPassed &&
            reciprocalIdentityPassed &&
            centralDerivativeConvergencePassed &&
            adjointIdentityPassed,
    };
}

MatrixDirectionDiagnostic BuildMatrixDirectionDiagnostic(
    double[,] re,
    double[,] im,
    double[] psi,
    double[] chi,
    double[] massPsiWeights)
{
    var aPsi = ApplyComplexMatrix(re, im, psi);
    var aChi = ApplyComplexMatrix(re, im, chi);
    var chiAPsi = ComplexInnerProduct(chi, aPsi, massPsiWeights);
    var psiAChi = ComplexInnerProduct(psi, aChi, massPsiWeights);
    double responsePairing = 2.0 * chiAPsi.Real;
    double reciprocalCurrentDirectionalDerivative = chiAPsi.Real + psiAChi.Real;
    double reciprocalIdentityScaleAwareResidual = ScaleAwareResidual(responsePairing, reciprocalCurrentDirectionalDerivative);
    double adjointIdentityScaleAwareResidual = ComplexScaleAwareResidual(chiAPsi, Conjugate(psiAChi));
    double currentAtPsi = CurrentValue(re, im, psi, massPsiWeights);
    var ladder = epsilonLadder
        .Select(epsilon =>
        {
            double plus = CurrentValue(re, im, AddScaled(psi, chi, epsilon), massPsiWeights);
            double minus = CurrentValue(re, im, AddScaled(psi, chi, -epsilon), massPsiWeights);
            double derivative = (plus - minus) / (2.0 * epsilon);
            double residual = ScaleAwareResidual(reciprocalCurrentDirectionalDerivative, derivative);
            return new CentralDifferenceDiagnostic
            {
                Epsilon = epsilon,
                CurrentAtPsiPlusEpsilonChi = plus,
                CurrentAtPsiMinusEpsilonChi = minus,
                CentralDifferenceDerivative = derivative,
                ExpectedReciprocalDirectionalDerivative = reciprocalCurrentDirectionalDerivative,
                ScaleAwareResidual = residual,
                Passed = residual <= CentralDerivativeTolerance,
            };
        })
        .ToArray();
    double maxCentralDerivativeScaleAwareResidual = ladder.Max(item => item.ScaleAwareResidual);
    return new()
    {
        IndependentPsiBarSourceDerivativeReal = chiAPsi.Real,
        IndependentPsiBarSourceDerivativeImaginary = chiAPsi.Imaginary,
        IndependentPsiSourceDerivativeReal = psiAChi.Real,
        IndependentPsiSourceDerivativeImaginary = psiAChi.Imaginary,
        ResponsePairing = responsePairing,
        HermitianRealFormShortcutResponsePairing = responsePairing,
        ReciprocalCurrentDirectionalDerivative = reciprocalCurrentDirectionalDerivative,
        ReciprocalIdentityScaleAwareResidual = reciprocalIdentityScaleAwareResidual,
        ReciprocalIdentityPassed = reciprocalIdentityScaleAwareResidual <= ReciprocalIdentityTolerance,
        AdjointIdentityScaleAwareResidual = adjointIdentityScaleAwareResidual,
        AdjointIdentityPassed = adjointIdentityScaleAwareResidual <= AdjointIdentityTolerance,
        CurrentAtPsi = currentAtPsi,
        CentralDifferenceLadder = ladder,
        MaxCentralDerivativeScaleAwareResidual = maxCentralDerivativeScaleAwareResidual,
        CentralDerivativeConvergencePassed = ladder.All(item => item.Passed),
    };
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

static double RequiredDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetDouble()
        : throw new InvalidDataException($"{propertyName} must be a number.");

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

static double[] LoadModeVector(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray();
}

static double[,] LoadMatrix(JsonElement array)
{
    var rows = array.EnumerateArray().ToArray();
    if (rows.Length == 0)
        throw new InvalidDataException("Matrix must contain at least one row.");
    int colCount = rows[0].GetArrayLength();
    var matrix = new double[rows.Length, colCount];
    for (int row = 0; row < rows.Length; row++)
    {
        var cols = rows[row].EnumerateArray().ToArray();
        if (cols.Length != colCount)
            throw new InvalidDataException("Matrix rows must have consistent lengths.");
        for (int col = 0; col < colCount; col++)
            matrix[row, col] = cols[col].GetDouble();
    }

    return matrix;
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

static void ValidateSameShape(double[,] left, double[,] right, string path)
{
    if (left.GetLength(0) != right.GetLength(0) || left.GetLength(1) != right.GetLength(1))
        throw new InvalidDataException($"Matrix shape mismatch while processing {path}.");
}

static double RelativeResidual(double[,] targetRe, double[,] targetIm, double[,] sourceRe, double[,] sourceIm)
{
    double residual2 = 0.0;
    double target2 = 0.0;
    int rows = targetRe.GetLength(0);
    int cols = targetRe.GetLength(1);
    for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            double dRe = targetRe[row, col] - sourceRe[row, col];
            double dIm = targetIm[row, col] - sourceIm[row, col];
            residual2 += dRe * dRe + dIm * dIm;
            target2 += targetRe[row, col] * targetRe[row, col] + targetIm[row, col] * targetIm[row, col];
        }
    return target2 > 0.0 ? Math.Sqrt(residual2 / target2) : double.NaN;
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

static (double Real, double Imaginary) ComplexInnerProduct(double[] left, double[] right, double[] massPsiWeights)
{
    if (left.Length != right.Length || left.Length % 2 != 0)
        throw new InvalidDataException("Complex interleaved vectors must have equal even lengths.");
    if (massPsiWeights.Length != left.Length)
        throw new InvalidDataException("M_psi weights must match the complex-interleaved vector length.");
    double real = 0.0;
    double imaginary = 0.0;
    for (int index = 0; index < left.Length; index += 2)
    {
        double leftRe = left[index];
        double leftIm = left[index + 1];
        double rightRe = right[index];
        double rightIm = right[index + 1];
        double weight = massPsiWeights[index];
        real += weight * (leftRe * rightRe + leftIm * rightIm);
        imaginary += weight * (leftRe * rightIm - leftIm * rightRe);
    }
    return (real, imaginary);
}

static double CurrentValue(double[,] re, double[,] im, double[] vector, double[] massPsiWeights) =>
    ComplexInnerProduct(vector, ApplyComplexMatrix(re, im, vector), massPsiWeights).Real;

static double MAdjointRelativeResidual(double[,] re, double[,] im, double[] massPsiWeights)
{
    int rows = re.GetLength(0);
    int cols = re.GetLength(1);
    if (rows != cols)
        return double.PositiveInfinity;
    if (massPsiWeights.Length != 2 * rows)
        throw new InvalidDataException("M_psi weights must match the matrix complex dimension.");
    double residual2 = 0.0;
    double weightedNorm2 = 0.0;
    for (int row = 0; row < rows; row++)
        for (int col = 0; col < cols; col++)
        {
            double rowWeight = massPsiWeights[2 * row];
            double colWeight = massPsiWeights[2 * col];
            double weightedRe = rowWeight * re[row, col];
            double weightedIm = rowWeight * im[row, col];
            double adjointWeightedRe = colWeight * re[col, row];
            double adjointWeightedIm = -colWeight * im[col, row];
            double differenceRe = weightedRe - adjointWeightedRe;
            double differenceIm = weightedIm - adjointWeightedIm;
            residual2 += differenceRe * differenceRe + differenceIm * differenceIm;
            weightedNorm2 += weightedRe * weightedRe + weightedIm * weightedIm;
        }
    return weightedNorm2 > 0.0 ? Math.Sqrt(residual2 / weightedNorm2) : double.NaN;
}

static double[] AddScaled(double[] vector, double[] direction, double scale)
{
    if (vector.Length != direction.Length)
        throw new InvalidDataException("Vectors must have equal lengths.");
    var result = new double[vector.Length];
    for (int index = 0; index < vector.Length; index++)
        result[index] = vector[index] + scale * direction[index];
    return result;
}

static (double Real, double Imaginary) Conjugate((double Real, double Imaginary) value) =>
    (value.Real, -value.Imaginary);

static double ScaleAwareResidual(double left, double right) =>
    Math.Abs(left - right) / Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right)));

static double ComplexScaleAwareResidual(
    (double Real, double Imaginary) left,
    (double Real, double Imaginary) right)
{
    double differenceReal = left.Real - right.Real;
    double differenceImaginary = left.Imaginary - right.Imaginary;
    double differenceNorm = Math.Sqrt(differenceReal * differenceReal + differenceImaginary * differenceImaginary);
    double leftNorm = Math.Sqrt(left.Real * left.Real + left.Imaginary * left.Imaginary);
    double rightNorm = Math.Sqrt(right.Real * right.Real + right.Imaginary * right.Imaginary);
    return differenceNorm / Math.Max(1.0, Math.Max(leftNorm, rightNorm));
}

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

public sealed class CandidateBackgroundRecord
{
    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMetadataPath { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required string SourceModeId { get; init; }
    public required int SourceModeIndex { get; init; }
    public required bool TargetBlindSourceSelection { get; init; }
    public required double IdentityWeightControlCandidateBilinearValue { get; init; }
    public required double MeshVolumeWeightDiagnosticCandidateBilinearValue { get; init; }
    public required double IdentityWeightBaseDiracMAdjointRelativeResidual { get; init; }
    public required bool IdentityWeightBaseDiracMAdjointCompatible { get; init; }
    public required bool MeshVolumeWeightDiagnosticMaterialized { get; init; }
    public required double MeshVolumeWeightBaseDiracMAdjointRelativeResidual { get; init; }
    public required bool MeshVolumeWeightBaseDiracMAdjointCompatible { get; init; }
    public required string CandidateDefinition { get; init; }
}

public sealed class VariationAuditRecord
{
    public required string VariationId { get; init; }
    public required string BosonModeId { get; init; }
    public required string FermionBackgroundId { get; init; }
    public required string SourceModeId { get; init; }
    public required int SourceModeIndex { get; init; }
    public required bool TargetBlindSourceSelection { get; init; }
    public required string SidecarPath { get; init; }
    public required double MatrixRelativeResidual { get; init; }
    public required bool MatrixParityPassed { get; init; }
    public required int DirectionCheckCount { get; init; }
    public required int DirectionalParityPassedCount { get; init; }
    public required int ReciprocalIdentityPassedCount { get; init; }
    public required int ResponsePairingParityPassedCount { get; init; }
    public required int CurrentDirectionalDerivativeParityPassedCount { get; init; }
    public required int CentralDerivativeConvergencePassedCount { get; init; }
    public required int AdjointIdentityPassedCount { get; init; }
    public required bool MeshVolumeWeightDiagnosticMaterialized { get; init; }
    public required bool MeshVolumeWeightVariationMAdjointCompatible { get; init; }
    public required int MeshVolumeWeightCentralDerivativeConvergencePassedCount { get; init; }
    public required double MaxDirectionalAnalyticVsPersistedScaleAwareResidual { get; init; }
    public required double MaxReciprocalIdentityScaleAwareResidual { get; init; }
    public required double MaxCentralDerivativeScaleAwareResidual { get; init; }
    public required double MaxAdjointIdentityScaleAwareResidual { get; init; }
    public required double MaxMeshVolumeWeightVariationMAdjointRelativeResidual { get; init; }
    public required double MaxMeshVolumeWeightCentralDerivativeScaleAwareResidual { get; init; }
    public required bool VariationPassed { get; init; }
}

public sealed class DirectionRow
{
    public required string ChiModeId { get; init; }
    public required int ChiModeIndex { get; init; }
    public required string PsiSourceModeId { get; init; }
    public required int PsiSourceModeIndex { get; init; }
    public required MatrixDirectionDiagnostic Analytic { get; init; }
    public required MatrixDirectionDiagnostic PersistedFiniteDifference { get; init; }
    public required MatrixDirectionDiagnostic MeshVolumeWeightAnalytic { get; init; }
    public required MatrixDirectionDiagnostic MeshVolumeWeightPersistedFiniteDifference { get; init; }
    public required double AnalyticVsPersistedResponsePairingScaleAwareResidual { get; init; }
    public required double AnalyticVsPersistedReciprocalDerivativeScaleAwareResidual { get; init; }
    public required double AnalyticVsPersistedCentralDerivativeMaxScaleAwareResidual { get; init; }
    public required double AnalyticVsPersistedMaxScaleAwareResidual { get; init; }
    public required bool ResponsePairingParityPassed { get; init; }
    public required bool CurrentDirectionalDerivativeParityPassed { get; init; }
    public required bool AnalyticVsPersistedParityPassed { get; init; }
    public required bool ReciprocalIdentityPassed { get; init; }
    public required bool CentralDerivativeConvergencePassed { get; init; }
    public required bool AdjointIdentityPassed { get; init; }
    public required bool MeshVolumeWeightCentralDerivativeConvergencePassed { get; init; }
    public required bool DirectionCheckPassed { get; init; }
}

public sealed class MatrixDirectionDiagnostic
{
    public required double IndependentPsiBarSourceDerivativeReal { get; init; }
    public required double IndependentPsiBarSourceDerivativeImaginary { get; init; }
    public required double IndependentPsiSourceDerivativeReal { get; init; }
    public required double IndependentPsiSourceDerivativeImaginary { get; init; }
    public required double ResponsePairing { get; init; }
    public required double HermitianRealFormShortcutResponsePairing { get; init; }
    public required double ReciprocalCurrentDirectionalDerivative { get; init; }
    public required double ReciprocalIdentityScaleAwareResidual { get; init; }
    public required bool ReciprocalIdentityPassed { get; init; }
    public required double AdjointIdentityScaleAwareResidual { get; init; }
    public required bool AdjointIdentityPassed { get; init; }
    public required double CurrentAtPsi { get; init; }
    public required IReadOnlyList<CentralDifferenceDiagnostic> CentralDifferenceLadder { get; init; }
    public required double MaxCentralDerivativeScaleAwareResidual { get; init; }
    public required bool CentralDerivativeConvergencePassed { get; init; }
}

public sealed class CentralDifferenceDiagnostic
{
    public required double Epsilon { get; init; }
    public required double CurrentAtPsiPlusEpsilonChi { get; init; }
    public required double CurrentAtPsiMinusEpsilonChi { get; init; }
    public required double CentralDifferenceDerivative { get; init; }
    public required double ExpectedReciprocalDirectionalDerivative { get; init; }
    public required double ScaleAwareResidual { get; init; }
    public required bool Passed { get; init; }
}
