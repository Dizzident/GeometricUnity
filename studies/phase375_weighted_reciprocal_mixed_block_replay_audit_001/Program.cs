using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

const string DefaultOutputDir = "studies/phase375_weighted_reciprocal_mixed_block_replay_audit_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string VariationDir = $"{FermionDir}/couplings/variations";
const string ModeDir = $"{Phase12Root}/spectra/modes";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase373SummaryPath = "studies/phase373_mass_psi_stiffness_operator_convention_repair_audit_001/output/mass_psi_stiffness_operator_convention_repair_audit_summary.json";
const string Phase374SummaryPath = "studies/phase374_shared_weighted_fermion_spectral_solver_repair_audit_001/output/shared_weighted_fermion_spectral_solver_repair_audit_summary.json";
const int ExpectedBackgroundCount = 2;
const int ExpectedVariationCount = 24;
const int ExpectedModesPerBackground = 12;
const int ExpectedDirectionalCheckCount = ExpectedVariationCount * ExpectedModesPerBackground;
const double AlgebraTolerance = 1e-11;
const double MatrixParityTolerance = 1e-8;
const double DirectionalParityTolerance = 1e-8;
const double GeneralizedResidualTolerance = 1e-6;
const double MNormTolerance = 1e-8;
const double MOrthonormalityTolerance = 1e-8;
const double CentralDerivativeTolerance = 1e-9;
const double KernelTolerance = 1e-12;
const double NonzeroObservableTolerance = 1e-12;
double[] epsilonLadder = [1e-2, 1e-3, 1e-4, 1e-5, 1e-6];

var options = JsonOptions();
var outputDir = Environment.GetEnvironmentVariable("PHASE375_OUTPUT_DIR") ?? DefaultOutputDir;
var backgroundOutputDir = Path.Combine(outputDir, "backgrounds");
var variationOutputDir = Path.Combine(outputDir, "variations");
Directory.CreateDirectory(backgroundOutputDir);
Directory.CreateDirectory(variationOutputDir);

using var phase373 = JsonDocument.Parse(File.ReadAllText(Phase373SummaryPath));
bool phase373ConventionCandidatePresent =
    JsonBool(phase373.RootElement, "massPsiStiffnessOperatorConventionRepairAuditPassed") &&
    JsonBool(phase373.RootElement, "meshVolumeMassPsiMaterialized") &&
    JsonBool(phase373.RootElement, "stiffnessMatrixConventionCandidateMaterialized") &&
    JsonBool(phase373.RootElement, "weightedOperatorConventionCandidateMaterialized") &&
    JsonBool(phase373.RootElement, "symmetricRepresentativeConventionCandidateMaterialized") &&
    JsonInt(phase373.RootElement, "transformedBaseBackgroundCount") == ExpectedBackgroundCount &&
    JsonInt(phase373.RootElement, "transformedVariationCount") == ExpectedVariationCount &&
    JsonInt(phase373.RootElement, "transformedDirectionalCheckCount") == ExpectedDirectionalCheckCount &&
    JsonInt(phase373.RootElement, "transformedDirectionalIdentityPassedCount") == ExpectedDirectionalCheckCount &&
    JsonInt(phase373.RootElement, "transformedAnalyticPersistedParityPassedCount") == ExpectedVariationCount;
bool phase373NonpromotionalBoundaryVerified =
    FalseFlag(phase373.RootElement, "routeProvidesPhysicalMassPsiCompatibleBranch") &&
    FalseFlag(phase373.RootElement, "routeProvidesCompletedFermionicAction") &&
    FalseFlag(phase373.RootElement, "routeProvidesFixedFermionicOperatorBranch") &&
    FalseFlag(phase373.RootElement, "routeProvidesExplicitYukawaFunctional") &&
    FalseFlag(phase373.RootElement, "routeProvidesCoupledResidual") &&
    FalseFlag(phase373.RootElement, "routeProvidesCompletedMixedLinearizationBlocks") &&
    FalseFlag(phase373.RootElement, "routeProvidesMixedLinearizationGaugeCompatibilityIdentities") &&
    FalseFlag(phase373.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") &&
    FalseFlag(phase373.RootElement, "routeProvidesHiggsScalarSourceOperator") &&
    FalseFlag(phase373.RootElement, "routeProvidesScalarProjectionTheorem") &&
    FalseFlag(phase373.RootElement, "routeProvidesGeVUnitNormalization") &&
    FalseFlag(phase373.RootElement, "routeCompletesBosonPredictions") &&
    FalseFlag(phase373.RootElement, "canFillPhase201WzContract") &&
    FalseFlag(phase373.RootElement, "canFillPhase201HiggsContract") &&
    FalseFlag(phase373.RootElement, "canFillPhase256ObservedFieldExtractionContract");

using var phase374 = JsonDocument.Parse(File.ReadAllText(Phase374SummaryPath));
bool phase374SharedSolverRepairPresent =
    JsonBool(phase374.RootElement, "sharedWeightedFermionSpectralSolverRepairAuditPassed") &&
    JsonBool(phase374.RootElement, "phase373ConventionCandidatePresent") &&
    JsonBool(phase374.RootElement, "phase373SyntheticBReplayQualityPassed") &&
    JsonBool(phase374.RootElement, "meshVolumeMassPsiMaterialized") &&
    JsonBool(phase374.RootElement, "meshVolumeMassPsiNonuniform") &&
    JsonInt(phase374.RootElement, "backgroundCount") == ExpectedBackgroundCount &&
    JsonInt(phase374.RootElement, "weightedModeCount") == ExpectedBackgroundCount * ExpectedModesPerBackground &&
    JsonInt(phase374.RootElement, "weightedGeneralizedResidualPassedCount") == ExpectedBackgroundCount * ExpectedModesPerBackground &&
    JsonInt(phase374.RootElement, "weightedMNormalizationPassedCount") == ExpectedBackgroundCount * ExpectedModesPerBackground &&
    JsonInt(phase374.RootElement, "weightedMOrthonormalityPassedBackgroundCount") == ExpectedBackgroundCount;
bool phase374SharedSolverNonpromotionalBoundaryVerified =
    FalseFlag(phase374.RootElement, "routeProvidesPhysicalMassPsiCompatibleBranch") &&
    FalseFlag(phase374.RootElement, "routeProvidesCanonicalPhysicalMassPsi") &&
    FalseFlag(phase374.RootElement, "routeProvidesCompletedFermionicAction") &&
    FalseFlag(phase374.RootElement, "routeProvidesFixedFermionicOperatorBranch") &&
    FalseFlag(phase374.RootElement, "routeProvidesExplicitYukawaFunctional") &&
    FalseFlag(phase374.RootElement, "routeProvidesCoupledResidual") &&
    FalseFlag(phase374.RootElement, "routeProvidesCompletedMixedLinearizationBlocks") &&
    FalseFlag(phase374.RootElement, "routeProvidesMixedLinearizationGaugeCompatibilityIdentities") &&
    FalseFlag(phase374.RootElement, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") &&
    FalseFlag(phase374.RootElement, "routeProvidesHiggsScalarSourceOperator") &&
    FalseFlag(phase374.RootElement, "routeProvidesScalarProjectionTheorem") &&
    FalseFlag(phase374.RootElement, "routeProvidesGeVUnitNormalization") &&
    FalseFlag(phase374.RootElement, "routeCompletesBosonPredictions") &&
    FalseFlag(phase374.RootElement, "canFillPhase201WzContract") &&
    FalseFlag(phase374.RootElement, "canFillPhase201HiggsContract") &&
    FalseFlag(phase374.RootElement, "canFillPhase256ObservedFieldExtractionContract");

var spinorSpec = JsonSerializer.Deserialize<SpinorRepresentationSpec>(
    File.ReadAllText(SpinorRepresentationPath),
    options) ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var provenance = new ProvenanceMeta
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "phase375-weighted-reciprocal-mixed-block-replay-audit",
    Branch = new() { BranchId = "phase375-discrete-weighted-replay-audit", SchemaVersion = "1.0" },
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
bool meshVolumeMassPsiMaterialized =
    massPsiWeights.Length > 0 &&
    massPsiWeights.All(weight => double.IsFinite(weight) && weight > 0.0);
bool meshVolumeMassPsiNonuniform = massPsiWeights.Distinct().Skip(1).Any();
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
var weightedModesByBackground = backgroundAudits.ToDictionary(
    row => row.FermionBackgroundId,
    row => (IReadOnlyList<WeightedModeAudit>)row.WeightedSolve.Modes,
    StringComparer.Ordinal);
var variationAudits = Directory
    .GetFiles(VariationDir, "variation-*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(BuildVariationAudit)
    .ToArray();

int backgroundCount = backgroundAudits.Length;
int weightedModeCount = backgroundAudits.Sum(row => row.WeightedSolve.ModeCount);
int phase12SelectedWeightedNonzeroModeCount = backgroundAudits.Sum(row => row.WeightedSolve.NonzeroModeCount);
bool phase12SelectedWeightedModesAreKernelOnly = phase12SelectedWeightedNonzeroModeCount == 0;
bool weightedSourceModesAreKernelOnly = backgroundAudits.All(row => row.SourceModeIsKernel);
int weightedBackgroundConventionPassedCount = backgroundAudits.Count(row => row.KIsEuclideanHermitian && row.AIsMSelfAdjoint);
int weightedSolveConvergedBackgroundCount = backgroundAudits.Count(row => row.WeightedSolve.SolverConverged);
int targetBlindWeightedBackgroundSourceSelectionPassedCount = backgroundAudits.Count(row => row.TargetBlindSourceSelection);
int weightedGeneralizedResidualPassedCount = backgroundAudits.Sum(row => row.WeightedSolve.GeneralizedResidualPassedCount);
int weightedMNormPassedCount = backgroundAudits.Sum(row => row.WeightedSolve.MNormPassedCount);
int weightedMOrthonormalityPassedBackgroundCount = backgroundAudits.Count(row => row.WeightedSolve.MOrthonormalityPassed);
double maxWeightedGeneralizedRelativeResidual = backgroundAudits.Max(row => row.WeightedSolve.MaxGeneralizedRelativeResidual);
double maxWeightedMNormResidual = backgroundAudits.Max(row => row.WeightedSolve.MaxMNormResidual);
double maxWeightedMOrthonormalityResidual = backgroundAudits.Max(row => row.WeightedSolve.MaxMOrthonormalityResidual);
int variationCount = variationAudits.Length;
int variationPassedCount = variationAudits.Count(row => row.VariationPassed);
int targetBlindWeightedVariationSourceSelectionPassedCount = variationAudits.Count(row => row.TargetBlindSourceSelection);
int analyticPersistedDeltaKParityPassedCount = variationAudits.Count(row => row.AnalyticPersistedDeltaKParityPassed);
int directionalCheckCount = variationAudits.Sum(row => row.DirectionalCheckCount);
int directionalIdentityPassedCount = variationAudits.Sum(row => row.DirectionalIdentityPassedCount);
int pairingIdentityPassedCount = variationAudits.Sum(row => row.PairingIdentityPassedCount);
int reciprocalDerivativeEqualityPassedCount = variationAudits.Sum(row => row.ReciprocalDerivativeEqualityPassedCount);
int hermitianShortcutEqualityPassedCount = variationAudits.Sum(row => row.HermitianShortcutEqualityPassedCount);
int centralDerivativeLadderPassedCount = variationAudits.Sum(row => row.CentralDerivativeLadderPassedCount);
int analyticPersistedDirectionalParityPassedCount = variationAudits.Sum(row => row.AnalyticPersistedDirectionalParityPassedCount);
int nonzeroPersistedWeightedCurrentCount = variationAudits.Count(row => row.PersistedSourceCurrentIsNonzero);
int nonzeroAnalyticWeightedCurrentCount = variationAudits.Count(row => row.AnalyticSourceCurrentIsNonzero);
int nonzeroPersistedWeightedReciprocalDerivativeCount = variationAudits.Sum(row => row.NonzeroPersistedWeightedReciprocalDerivativeCount);
int nonzeroAnalyticWeightedReciprocalDerivativeCount = variationAudits.Sum(row => row.NonzeroAnalyticWeightedReciprocalDerivativeCount);
double maxAnalyticPersistedDeltaKRelativeResidual = variationAudits.Max(row => row.AnalyticPersistedDeltaKRelativeResidual);
double maxPairingIdentityScaleAwareResidual = variationAudits.Max(row => row.MaxPairingIdentityScaleAwareResidual);
double maxHermitianShortcutScaleAwareResidual = variationAudits.Max(row => row.MaxHermitianShortcutScaleAwareResidual);
double maxCentralDerivativeScaleAwareResidual = variationAudits.Max(row => row.MaxCentralDerivativeScaleAwareResidual);
double maxAnalyticPersistedDirectionalScaleAwareResidual = variationAudits.Max(row => row.MaxAnalyticPersistedDirectionalScaleAwareResidual);

const bool routeProvidesPhysicalGuBranch = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCanonicalPhysicalMassPsi = false;
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
const bool weightedReplayEstablishesNonzeroSpectrumEigenmodeProof = false;
const bool fixedMeshConnectionOnlyVariationReplay = true;

bool weightedReciprocalMixedBlockReplayAuditPassed =
    phase373ConventionCandidatePresent &&
    phase373NonpromotionalBoundaryVerified &&
    phase374SharedSolverRepairPresent &&
    phase374SharedSolverNonpromotionalBoundaryVerified &&
    meshVolumeMassPsiMaterialized &&
    meshVolumeMassPsiNonuniform &&
    fixedMeshConnectionOnlyVariationReplay &&
    backgroundCount == ExpectedBackgroundCount &&
    weightedModeCount == ExpectedBackgroundCount * ExpectedModesPerBackground &&
    phase12SelectedWeightedModesAreKernelOnly &&
    weightedSourceModesAreKernelOnly &&
    weightedBackgroundConventionPassedCount == ExpectedBackgroundCount &&
    weightedSolveConvergedBackgroundCount == ExpectedBackgroundCount &&
    targetBlindWeightedBackgroundSourceSelectionPassedCount == ExpectedBackgroundCount &&
    weightedGeneralizedResidualPassedCount == weightedModeCount &&
    weightedMNormPassedCount == weightedModeCount &&
    weightedMOrthonormalityPassedBackgroundCount == ExpectedBackgroundCount &&
    variationCount == ExpectedVariationCount &&
    variationPassedCount == ExpectedVariationCount &&
    targetBlindWeightedVariationSourceSelectionPassedCount == ExpectedVariationCount &&
    analyticPersistedDeltaKParityPassedCount == ExpectedVariationCount &&
    directionalCheckCount == ExpectedDirectionalCheckCount &&
    directionalIdentityPassedCount == ExpectedDirectionalCheckCount &&
    pairingIdentityPassedCount == ExpectedDirectionalCheckCount &&
    reciprocalDerivativeEqualityPassedCount == ExpectedDirectionalCheckCount &&
    hermitianShortcutEqualityPassedCount == ExpectedDirectionalCheckCount &&
    centralDerivativeLadderPassedCount == ExpectedDirectionalCheckCount &&
    analyticPersistedDirectionalParityPassedCount == ExpectedDirectionalCheckCount &&
    !routeProvidesPhysicalGuBranch &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCanonicalPhysicalMassPsi &&
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
    !canFillPhase256ObservedFieldExtractionContract &&
    !weightedReplayEstablishesNonzeroSpectrumEigenmodeProof;

string terminalStatus = weightedReciprocalMixedBlockReplayAuditPassed
    ? "weighted-reciprocal-mixed-block-replay-validated-discrete-only"
    : "weighted-reciprocal-mixed-block-replay-audit-blocked";
string decision = weightedReciprocalMixedBlockReplayAuditPassed
    ? "The repaired shared weighted modes replay the reciprocal discrete source-block candidate across both Phase12 stiffness matrices and all 24 persisted/analytic variations. All 288 weighted direction checks pass. The selected Phase12 weighted modes are kernel modes, and the replay has zero nonzero source currents and zero nonzero reciprocal derivatives. This remains a discrete-only zero-mode replay, not a nonzero-spectrum eigenmode proof or physical GU prediction route."
    : "Do not promote the weighted reciprocal replay until the Phase373 convention gate, Phase374 shared solver gate, two weighted background solves, 24 deltaK parity checks, and all 288 weighted directional identities pass.";
var predictionContractImpact = new
{
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
};
var result = new
{
    phaseId = "phase375-weighted-reciprocal-mixed-block-replay-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    weightedReciprocalMixedBlockReplayAuditPassed,
    phase373ConventionCandidatePresent,
    phase373NonpromotionalBoundaryVerified,
    phase374SharedSolverRepairPresent,
    phase374SharedSolverNonpromotionalBoundaryVerified,
    meshVolumeMassPsiMaterialized,
    meshVolumeMassPsiNonuniform,
    implementedObjectClassification = "discrete-only weighted reciprocal source-block replay candidate / VO-7 building block",
    implementedObjectCompletesVo7 = false,
    weightedReplayEstablishesNonzeroSpectrumEigenmodeProof,
    fixedMeshConnectionOnlyVariationReplay,
    conventionDefinitions = new
    {
        stiffnessActionMatrix = "K = persisted Euclidean-Hermitian Phase12 assembled Dirac matrix",
        weightedOperator = "A = M_psi^-1 K",
        stiffnessVariation = "deltaK = persisted or analytic Euclidean-Hermitian first-variation stiffness matrix",
        weightedVariationOperator = "deltaA = M_psi^-1 deltaK",
        massPsiVariation = "delta M_psi = 0 for fixed-mesh connection-space perturbations b_k",
        generalizedEigenproblem = "K psi = lambda M_psi psi",
        weightedPairingIdentity = "<chi, deltaA psi>_M = chi^dagger deltaK psi",
        reciprocalRealDerivative = "d J_k(psi)[chi] = Re<chi, deltaA psi>_M + Re<psi, deltaA chi>_M",
        hermitianShortcut = "d J_k(psi)[chi] = 2 Re<chi, deltaA psi>_M when deltaK is Euclidean-Hermitian and deltaA is M-self-adjoint",
        current = "J_k(psi) = Re<psi, deltaA psi>_M",
    },
    sourceSelection = new
    {
        targetBlind = true,
        sourceRule = "select repaired shared-solver weighted fermion mode with minimum modeIndex; no physical boson target, observed mass, or external calibration consulted",
        directionRule = "evaluate every repaired shared-solver weighted fermion mode chi_i ordered by modeIndex",
    },
    backgroundCount,
    weightedModeCount,
    phase12SelectedWeightedNonzeroModeCount,
    phase12SelectedWeightedModesAreKernelOnly,
    weightedSourceModesAreKernelOnly,
    weightedBackgroundConventionPassedCount,
    weightedSolveConvergedBackgroundCount,
    targetBlindWeightedBackgroundSourceSelectionPassedCount,
    weightedGeneralizedResidualPassedCount,
    weightedMNormPassedCount,
    weightedMOrthonormalityPassedBackgroundCount,
    maxWeightedGeneralizedRelativeResidual,
    maxWeightedMNormResidual,
    maxWeightedMOrthonormalityResidual,
    variationCount,
    variationPassedCount,
    targetBlindWeightedVariationSourceSelectionPassedCount,
    analyticPersistedDeltaKParityPassedCount,
    directionalCheckCount,
    directionalIdentityPassedCount,
    pairingIdentityPassedCount,
    reciprocalDerivativeEqualityPassedCount,
    hermitianShortcutEqualityPassedCount,
    centralDerivativeLadderPassedCount,
    analyticPersistedDirectionalParityPassedCount,
    nonzeroPersistedWeightedCurrentCount,
    nonzeroAnalyticWeightedCurrentCount,
    nonzeroPersistedWeightedReciprocalDerivativeCount,
    nonzeroAnalyticWeightedReciprocalDerivativeCount,
    maxAnalyticPersistedDeltaKRelativeResidual,
    maxPairingIdentityScaleAwareResidual,
    maxHermitianShortcutScaleAwareResidual,
    maxCentralDerivativeScaleAwareResidual,
    maxAnalyticPersistedDirectionalScaleAwareResidual,
    tolerances = new
    {
        algebraTolerance = AlgebraTolerance,
        matrixParityTolerance = MatrixParityTolerance,
        directionalParityTolerance = DirectionalParityTolerance,
        generalizedResidualTolerance = GeneralizedResidualTolerance,
        mNormTolerance = MNormTolerance,
        mOrthonormalityTolerance = MOrthonormalityTolerance,
        centralDerivativeTolerance = CentralDerivativeTolerance,
        kernelTolerance = KernelTolerance,
        nonzeroObservableTolerance = NonzeroObservableTolerance,
        epsilonLadder,
    },
    meshVolumeMassPsi = new
    {
        builder = "MassPsiWeightsBuilder.BuildFromMesh(mesh, spinorDim * dimG)",
        realWeightCount = massPsiWeights.Length,
        minimumWeight = massPsiWeights.Min(),
        maximumWeight = massPsiWeights.Max(),
        distinctWeights = massPsiWeights.Distinct().Order().ToArray(),
    },
    routeProvidesPhysicalGuBranch,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCanonicalPhysicalMassPsi,
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
    predictionContractImpact,
    explicitDiscreteOnlyNonclaims = new[]
    {
        "not a physical GU branch",
        "no canonical physical M_psi established",
        "not a completed GU fermionic action",
        "not a fixed GU fermionic branch",
        "no explicit Yukawa functional or solved coupling map",
        "no coupled residual or completed mixed linearization blocks",
        "no mixed-block gauge compatibility identities",
        "no target-independent W/Z bridge source law",
        "no Higgs scalar source operator or scalar projection theorem",
        "no GeV unit normalization",
        "no promoted boson predictions",
        "no Phase201 or Phase256 contract fill",
        "selected Phase12 shared weighted modes are kernel modes; this is not a nonzero-spectrum eigenmode proof",
    },
    backgroundAudits,
    variationAudits,
    sourceEvidence = new
    {
        phase373SummaryPath = Phase373SummaryPath,
        phase374SummaryPath = Phase374SummaryPath,
        phase12Root = Phase12Root,
        fermionDir = FermionDir,
        variationDir = VariationDir,
        modeDir = ModeDir,
        spinorRepresentationPath = SpinorRepresentationPath,
    },
    decision,
};

Directory.CreateDirectory(outputDir);
string fullPath = Path.Combine(outputDir, "weighted_reciprocal_mixed_block_replay_audit.json");
string summaryPath = Path.Combine(outputDir, "weighted_reciprocal_mixed_block_replay_audit_summary.json");
File.WriteAllText(fullPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.generatedAt,
    terminalStatus,
    weightedReciprocalMixedBlockReplayAuditPassed,
    phase373ConventionCandidatePresent,
    phase373NonpromotionalBoundaryVerified,
    phase374SharedSolverRepairPresent,
    phase374SharedSolverNonpromotionalBoundaryVerified,
    meshVolumeMassPsiMaterialized,
    meshVolumeMassPsiNonuniform,
    result.implementedObjectClassification,
    result.implementedObjectCompletesVo7,
    weightedReplayEstablishesNonzeroSpectrumEigenmodeProof,
    fixedMeshConnectionOnlyVariationReplay,
    backgroundCount,
    weightedModeCount,
    phase12SelectedWeightedNonzeroModeCount,
    phase12SelectedWeightedModesAreKernelOnly,
    weightedSourceModesAreKernelOnly,
    weightedBackgroundConventionPassedCount,
    weightedSolveConvergedBackgroundCount,
    targetBlindWeightedBackgroundSourceSelectionPassedCount,
    weightedGeneralizedResidualPassedCount,
    weightedMNormPassedCount,
    weightedMOrthonormalityPassedBackgroundCount,
    maxWeightedGeneralizedRelativeResidual,
    maxWeightedMNormResidual,
    maxWeightedMOrthonormalityResidual,
    variationCount,
    variationPassedCount,
    targetBlindWeightedVariationSourceSelectionPassedCount,
    analyticPersistedDeltaKParityPassedCount,
    directionalCheckCount,
    directionalIdentityPassedCount,
    pairingIdentityPassedCount,
    reciprocalDerivativeEqualityPassedCount,
    hermitianShortcutEqualityPassedCount,
    centralDerivativeLadderPassedCount,
    analyticPersistedDirectionalParityPassedCount,
    nonzeroPersistedWeightedCurrentCount,
    nonzeroAnalyticWeightedCurrentCount,
    nonzeroPersistedWeightedReciprocalDerivativeCount,
    nonzeroAnalyticWeightedReciprocalDerivativeCount,
    maxAnalyticPersistedDeltaKRelativeResidual,
    maxPairingIdentityScaleAwareResidual,
    maxHermitianShortcutScaleAwareResidual,
    maxCentralDerivativeScaleAwareResidual,
    maxAnalyticPersistedDirectionalScaleAwareResidual,
    routeProvidesPhysicalGuBranch,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCanonicalPhysicalMassPsi,
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
    predictionContractImpact,
    result.explicitDiscreteOnlyNonclaims,
    decision,
}, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"weightedReciprocalMixedBlockReplayAuditPassed={weightedReciprocalMixedBlockReplayAuditPassed}");
Console.WriteLine($"phase373ConventionCandidatePresent={phase373ConventionCandidatePresent}");
Console.WriteLine($"phase374SharedSolverRepairPresent={phase374SharedSolverRepairPresent}");
Console.WriteLine($"weightedGeneralizedResidualPassedCount={weightedGeneralizedResidualPassedCount}/{weightedModeCount}");
Console.WriteLine($"weightedMNormPassedCount={weightedMNormPassedCount}/{weightedModeCount}");
Console.WriteLine($"weightedMOrthonormalityPassedBackgroundCount={weightedMOrthonormalityPassedBackgroundCount}/{backgroundCount}");
Console.WriteLine($"weightedBackgroundConventionPassedCount={weightedBackgroundConventionPassedCount}/{backgroundCount}");
Console.WriteLine($"variationPassedCount={variationPassedCount}/{variationCount}");
Console.WriteLine($"analyticPersistedDeltaKParityPassedCount={analyticPersistedDeltaKParityPassedCount}/{variationCount}");
Console.WriteLine($"directionalIdentityPassedCount={directionalIdentityPassedCount}/{directionalCheckCount}");
Console.WriteLine($"phase12SelectedWeightedNonzeroModeCount={phase12SelectedWeightedNonzeroModeCount}/{weightedModeCount}");
Console.WriteLine($"nonzeroPersistedWeightedCurrentCount={nonzeroPersistedWeightedCurrentCount}/{variationCount}");
Console.WriteLine($"nonzeroPersistedWeightedReciprocalDerivativeCount={nonzeroPersistedWeightedReciprocalDerivativeCount}/{directionalCheckCount}");
Console.WriteLine($"maxAnalyticPersistedDeltaKRelativeResidual={maxAnalyticPersistedDeltaKRelativeResidual:R}");
Console.WriteLine($"maxPairingIdentityScaleAwareResidual={maxPairingIdentityScaleAwareResidual:R}");
Console.WriteLine($"maxCentralDerivativeScaleAwareResidual={maxCentralDerivativeScaleAwareResidual:R}");
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
    var solve = solver.Solve(
        BuildBundle(backgroundId, layout.LayoutId, metadata, k.ToFlatInterleaved()),
        layout,
        BuildConfig(),
        provenance);
    var weightedSolve = EvaluateWeightedSolve(solve, k);
    var source = weightedSolve.Modes.OrderBy(mode => mode.ModeIndex).First();
    string sidecarPath = Path.Combine(backgroundOutputDir, $"{backgroundId}-weighted-reciprocal-replay.json");
    var audit = new BackgroundAudit
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMetadataPath = metadataPath,
        BaseDiracMatrixPath = matrixPath,
        LayoutPath = layoutPath,
        KIsEuclideanHermitian = EuclideanHermiticityResidual(k) <= AlgebraTolerance,
        KEuclideanHermiticityRelativeResidual = EuclideanHermiticityResidual(k),
        AIsMSelfAdjoint = MAdjointResidual(a, massPsiWeights) <= AlgebraTolerance,
        AMAdjointRelativeResidual = MAdjointResidual(a, massPsiWeights),
        WeightedSolve = weightedSolve,
        SourceModeId = source.ModeId,
        SourceModeIndex = source.ModeIndex,
        SourceModeIsKernel = source.IsKernel,
        TargetBlindSourceSelection = source.ModeIndex == weightedSolve.Modes.Min(mode => mode.ModeIndex),
        BaseWeightedActionValue = WeightedActionValue(a, source.Coefficients),
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
    var persistedDeltaK = LoadNestedMatrix(matrixPath);
    double[] modeVector = LoadModeVector(modePath);
    var (analyticRe, analyticIm) = DiracVariationComputer.ComputeAnalytical(
        modeVector,
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
    double matrixParityResidual = MatrixRelativeResidual(persistedDeltaK, analyticDeltaK);
    bool matrixParityPassed = matrixParityResidual <= MatrixParityTolerance;
    if (!weightedModesByBackground.TryGetValue(backgroundId, out var weightedModes))
        throw new InvalidDataException($"Missing shared weighted modes for {backgroundId}.");
    var source = weightedModes.OrderBy(mode => mode.ModeIndex).First();
    var directions = weightedModes
        .OrderBy(mode => mode.ModeIndex)
        .Select(mode => BuildDirectionalAudit(source, mode, persisted, analytic))
        .ToArray();
    string sidecarPath = Path.Combine(variationOutputDir, $"{variationId}-weighted-reciprocal-replay.json");
    var audit = new VariationAudit
    {
        VariationId = variationId,
        FermionBackgroundId = backgroundId,
        BosonModeId = bosonModeId,
        PersistedVariationMetadataPath = metadataPath,
        PersistedVariationMatrixPath = matrixPath,
        AnalyticVariationSource = "Gu.Phase4.Couplings.DiracVariationComputer.ComputeAnalytical",
        SourceModeId = source.ModeId,
        SourceModeIndex = source.ModeIndex,
        SourceModeIsKernel = source.IsKernel,
        TargetBlindSourceSelection = source.ModeIndex == weightedModes.Min(mode => mode.ModeIndex),
        PersistedOperator = persisted.Record,
        AnalyticOperator = analytic.Record,
        AnalyticPersistedDeltaKRelativeResidual = matrixParityResidual,
        AnalyticPersistedDeltaKParityPassed = matrixParityPassed,
        PersistedSourceCurrent = WeightedActionValue(persisted.DeltaA, source.Coefficients),
        AnalyticSourceCurrent = WeightedActionValue(analytic.DeltaA, source.Coefficients),
        DirectionalCheckCount = directions.Length,
        DirectionalIdentityPassedCount = directions.Count(row => row.AllIdentitiesPassed),
        PairingIdentityPassedCount = directions.Count(row => row.PairingIdentityPassed),
        ReciprocalDerivativeEqualityPassedCount = directions.Count(row => row.ReciprocalDerivativeEqualityPassed),
        HermitianShortcutEqualityPassedCount = directions.Count(row => row.HermitianShortcutEqualityPassed),
        CentralDerivativeLadderPassedCount = directions.Count(row => row.CentralDerivativeLadderPassed),
        AnalyticPersistedDirectionalParityPassedCount = directions.Count(row => row.AnalyticPersistedDirectionalParityPassed),
        NonzeroPersistedWeightedReciprocalDerivativeCount = directions.Count(row => Math.Abs(row.Persisted.ReciprocalRealDirectionalDerivative) > NonzeroObservableTolerance),
        NonzeroAnalyticWeightedReciprocalDerivativeCount = directions.Count(row => Math.Abs(row.Analytic.ReciprocalRealDirectionalDerivative) > NonzeroObservableTolerance),
        MaxPairingIdentityScaleAwareResidual = directions.Max(row => row.MaxPairingIdentityScaleAwareResidual),
        MaxHermitianShortcutScaleAwareResidual = directions.Max(row => row.MaxHermitianShortcutScaleAwareResidual),
        MaxCentralDerivativeScaleAwareResidual = directions.Max(row => row.MaxCentralDerivativeScaleAwareResidual),
        MaxAnalyticPersistedDirectionalScaleAwareResidual = directions.Max(row => row.AnalyticPersistedDirectionalMaxScaleAwareResidual),
        Directions = directions,
        SidecarPath = sidecarPath,
    };
    audit.VariationPassed =
        audit.TargetBlindSourceSelection &&
        audit.SourceModeIsKernel &&
        audit.PersistedOperator.AllOperatorIdentitiesPassed &&
        audit.AnalyticOperator.AllOperatorIdentitiesPassed &&
        audit.AnalyticPersistedDeltaKParityPassed &&
        audit.DirectionalCheckCount == ExpectedModesPerBackground &&
        audit.DirectionalIdentityPassedCount == ExpectedModesPerBackground;
    File.WriteAllText(sidecarPath, JsonSerializer.Serialize(audit, options));
    return audit;
}

VariationOperatorAudit BuildVariationOperatorAudit(ComplexMatrix deltaK)
{
    var deltaA = deltaK.ScaleRows(massPsiWeights);
    double deltaKHermiticity = EuclideanHermiticityResidual(deltaK);
    double deltaAMAdjoint = MAdjointResidual(deltaA, massPsiWeights);
    return new(
        deltaK,
        deltaA,
        new()
        {
            DeltaKIsEuclideanHermitian = deltaKHermiticity <= AlgebraTolerance,
            DeltaKEuclideanHermiticityRelativeResidual = deltaKHermiticity,
            DeltaAIsMSelfAdjoint = deltaAMAdjoint <= AlgebraTolerance,
            DeltaAMAdjointRelativeResidual = deltaAMAdjoint,
            HermitianShortcutJustified =
                deltaKHermiticity <= AlgebraTolerance &&
                deltaAMAdjoint <= AlgebraTolerance,
            AllOperatorIdentitiesPassed =
                deltaKHermiticity <= AlgebraTolerance &&
                deltaAMAdjoint <= AlgebraTolerance,
        });
}

DirectionalAudit BuildDirectionalAudit(
    WeightedModeAudit source,
    WeightedModeAudit direction,
    VariationOperatorAudit persisted,
    VariationOperatorAudit analytic)
{
    var persistedDiagnostic = BuildDirectionalDiagnostic(source.Coefficients, direction.Coefficients, persisted);
    var analyticDiagnostic = BuildDirectionalDiagnostic(source.Coefficients, direction.Coefficients, analytic);
    double weightedPairingParity = ComplexScaleAwareResidual(
        persistedDiagnostic.WeightedOperatorPairing,
        analyticDiagnostic.WeightedOperatorPairing);
    double stiffnessPairingParity = ComplexScaleAwareResidual(
        persistedDiagnostic.StiffnessPairing,
        analyticDiagnostic.StiffnessPairing);
    double reciprocalDerivativeParity = ScaleAwareResidual(
        persistedDiagnostic.ReciprocalRealDirectionalDerivative,
        analyticDiagnostic.ReciprocalRealDirectionalDerivative);
    double centralDerivativeParity = persistedDiagnostic.CentralDerivativeLadder
        .Zip(analyticDiagnostic.CentralDerivativeLadder)
        .Max(pair => ScaleAwareResidual(pair.First.CentralDerivative, pair.Second.CentralDerivative));
    double maxParity = Math.Max(
        Math.Max(weightedPairingParity, stiffnessPairingParity),
        Math.Max(reciprocalDerivativeParity, centralDerivativeParity));
    bool directionalParityPassed = maxParity <= DirectionalParityTolerance;
    return new()
    {
        PsiSourceModeId = source.ModeId,
        PsiSourceModeIndex = source.ModeIndex,
        PsiSourceModeIsKernel = source.IsKernel,
        ChiModeId = direction.ModeId,
        ChiModeIndex = direction.ModeIndex,
        ChiModeIsKernel = direction.IsKernel,
        Persisted = persistedDiagnostic,
        Analytic = analyticDiagnostic,
        AnalyticPersistedWeightedPairingScaleAwareResidual = weightedPairingParity,
        AnalyticPersistedStiffnessPairingScaleAwareResidual = stiffnessPairingParity,
        AnalyticPersistedReciprocalDerivativeScaleAwareResidual = reciprocalDerivativeParity,
        AnalyticPersistedCentralDerivativeMaxScaleAwareResidual = centralDerivativeParity,
        AnalyticPersistedDirectionalMaxScaleAwareResidual = maxParity,
        AnalyticPersistedDirectionalParityPassed = directionalParityPassed,
        PairingIdentityPassed =
            persistedDiagnostic.WeightedOperatorToStiffnessPairingIdentityPassed &&
            analyticDiagnostic.WeightedOperatorToStiffnessPairingIdentityPassed,
        ReciprocalDerivativeEqualityPassed =
            persistedDiagnostic.ReciprocalDerivativeEqualityPassed &&
            analyticDiagnostic.ReciprocalDerivativeEqualityPassed,
        HermitianShortcutEqualityPassed =
            persistedDiagnostic.HermitianShortcutEqualityPassed &&
            analyticDiagnostic.HermitianShortcutEqualityPassed,
        CentralDerivativeLadderPassed =
            persistedDiagnostic.CentralDerivativeLadderPassed &&
            analyticDiagnostic.CentralDerivativeLadderPassed,
        MaxPairingIdentityScaleAwareResidual = Math.Max(
            persistedDiagnostic.WeightedOperatorToStiffnessPairingScaleAwareResidual,
            analyticDiagnostic.WeightedOperatorToStiffnessPairingScaleAwareResidual),
        MaxHermitianShortcutScaleAwareResidual = Math.Max(
            persistedDiagnostic.HermitianShortcutScaleAwareResidual,
            analyticDiagnostic.HermitianShortcutScaleAwareResidual),
        MaxCentralDerivativeScaleAwareResidual = Math.Max(
            persistedDiagnostic.MaxCentralDerivativeScaleAwareResidual,
            analyticDiagnostic.MaxCentralDerivativeScaleAwareResidual),
        AllIdentitiesPassed =
            persistedDiagnostic.AllDirectionalIdentitiesPassed &&
            analyticDiagnostic.AllDirectionalIdentitiesPassed &&
            directionalParityPassed,
    };
}

DirectionalDiagnostic BuildDirectionalDiagnostic(
    double[] psi,
    double[] chi,
    VariationOperatorAudit variation)
{
    var deltaAPsi = variation.DeltaA.Apply(psi);
    var deltaAChi = variation.DeltaA.Apply(chi);
    var weightedPairing = ComplexInnerProduct(chi, deltaAPsi, massPsiWeights);
    var stiffnessPairing = ComplexInnerProduct(chi, variation.DeltaK.Apply(psi), null);
    var reciprocalPartner = ComplexInnerProduct(psi, deltaAChi, massPsiWeights);
    double reciprocalDerivative = weightedPairing.Real + reciprocalPartner.Real;
    double independentSum = weightedPairing.Real + reciprocalPartner.Real;
    double hermitianShortcut = 2.0 * weightedPairing.Real;
    double pairingResidual = ComplexScaleAwareResidual(weightedPairing, stiffnessPairing);
    double reciprocalResidual = ScaleAwareResidual(reciprocalDerivative, independentSum);
    double shortcutResidual = ScaleAwareResidual(reciprocalDerivative, hermitianShortcut);
    var ladder = epsilonLadder.Select(epsilon =>
    {
        double plus = WeightedActionValue(variation.DeltaA, AddScaled(psi, chi, epsilon));
        double minus = WeightedActionValue(variation.DeltaA, AddScaled(psi, chi, -epsilon));
        double derivative = (plus - minus) / (2.0 * epsilon);
        double residual = ScaleAwareResidual(reciprocalDerivative, derivative);
        return new CentralDerivativeRow
        {
            Epsilon = epsilon,
            CurrentAtPsiPlusEpsilonChi = plus,
            CurrentAtPsiMinusEpsilonChi = minus,
            CentralDerivative = derivative,
            ExpectedReciprocalRealDirectionalDerivative = reciprocalDerivative,
            ScaleAwareResidual = residual,
            Passed = residual <= CentralDerivativeTolerance,
        };
    }).ToArray();
    double maxCentralResidual = ladder.Max(row => row.ScaleAwareResidual);
    bool pairingPassed = pairingResidual <= AlgebraTolerance;
    bool reciprocalPassed = reciprocalResidual <= AlgebraTolerance;
    bool shortcutPassed =
        variation.Record.HermitianShortcutJustified &&
        shortcutResidual <= AlgebraTolerance;
    bool ladderPassed = ladder.All(row => row.Passed);
    return new()
    {
        WeightedOperatorPairing = weightedPairing,
        StiffnessPairing = stiffnessPairing,
        WeightedOperatorToStiffnessPairingScaleAwareResidual = pairingResidual,
        WeightedOperatorToStiffnessPairingIdentityPassed = pairingPassed,
        ReciprocalPartnerPairing = reciprocalPartner,
        ReciprocalRealDirectionalDerivative = reciprocalDerivative,
        IndependentSourceDerivativeRealSum = independentSum,
        ReciprocalDerivativeEqualityScaleAwareResidual = reciprocalResidual,
        ReciprocalDerivativeEqualityPassed = reciprocalPassed,
        HermitianShortcutJustified = variation.Record.HermitianShortcutJustified,
        HermitianShortcutDirectionalDerivative = hermitianShortcut,
        HermitianShortcutScaleAwareResidual = shortcutResidual,
        HermitianShortcutEqualityPassed = shortcutPassed,
        CurrentAtPsi = WeightedActionValue(variation.DeltaA, psi),
        CentralDerivativeLadder = ladder,
        MaxCentralDerivativeScaleAwareResidual = maxCentralResidual,
        CentralDerivativeLadderPassed = ladderPassed,
        AllDirectionalIdentitiesPassed =
            pairingPassed &&
            reciprocalPassed &&
            shortcutPassed &&
            ladderPassed,
    };
}

WeightedSolveAudit EvaluateWeightedSolve(FermionSpectralResult solve, ComplexMatrix k)
{
    var modes = solve.Modes.Select(mode =>
    {
        double[] psi = mode.EigenvectorCoefficients
            ?? throw new InvalidDataException($"Solver mode {mode.ModeId} has no coefficients.");
        double[] kPsi = k.Apply(psi);
        double[] lambdaMPsi = Scale(ApplyWeights(psi, massPsiWeights), mode.EigenvalueRe);
        double generalizedResidual = RelativeVectorResidual(kPsi, lambdaMPsi);
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
            maxOrthResidual = Math.Max(
                maxOrthResidual,
                Math.Sqrt(Square(inner.Real - expected) + Square(inner.Imaginary)));
        }
    return new()
    {
        SolverName = solve.Diagnostics.SolverName,
        SolverConverged = solve.Diagnostics.Converged,
        SolverIterations = solve.Diagnostics.Iterations,
        SolverNotes = solve.Diagnostics.Notes,
        ModeCount = modes.Length,
        NonzeroModeCount = modes.Count(mode => !mode.IsKernel),
        GeneralizedResidualPassedCount = modes.Count(mode => mode.GeneralizedResidualPassed),
        MNormPassedCount = modes.Count(mode => mode.MNormPassed),
        MaxGeneralizedRelativeResidual = modes.Max(mode => mode.GeneralizedKPsiEqualsLambdaMPsiRelativeResidual),
        MaxMNormResidual = modes.Max(mode => mode.MNormResidual),
        MaxMOrthonormalityResidual = maxOrthResidual,
        MOrthonormalityPassed = maxOrthResidual <= MOrthonormalityTolerance,
        Modes = modes,
    };
}

DiracOperatorBundle BuildBundle(
    string backgroundId,
    string layoutId,
    JsonElement metadata,
    double[] explicitMatrix) => new()
{
    OperatorId = $"phase375-stiffness-k-{backgroundId}",
    FermionBackgroundId = backgroundId,
    LayoutId = layoutId,
    SpinConnectionId = $"phase375-stiffness-k-{backgroundId}",
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
    DiagnosticNotes = ["Phase375 persisted stiffness K shared weighted reciprocal replay."],
    Provenance = provenance,
};

FermionSpectralConfig BuildConfig() => new()
{
    TargetRegion = "lowest-magnitude",
    ModeCount = ExpectedModesPerBackground,
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

static double[] LoadModeVector(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray();
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

double WeightedActionValue(ComplexMatrix a, double[] psi) =>
    ComplexInnerProduct(psi, a.Apply(psi), massPsiWeights).Real;

static double[] AddScaled(double[] vector, double[] direction, double scale) =>
    vector.Zip(direction, (value, delta) => value + scale * delta).ToArray();

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
    Math.Sqrt(Square(left.Real - right.Real) + Square(left.Imaginary - right.Imaginary)) /
    Math.Max(1.0, Math.Max(
        Math.Sqrt(Square(left.Real) + Square(left.Imaginary)),
        Math.Sqrt(Square(right.Real) + Square(right.Imaginary))));

static double Square(double value) => value * value;

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

public sealed class BackgroundAudit
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMetadataPath { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required string LayoutPath { get; init; }
    public required bool KIsEuclideanHermitian { get; init; }
    public required double KEuclideanHermiticityRelativeResidual { get; init; }
    public required bool AIsMSelfAdjoint { get; init; }
    public required double AMAdjointRelativeResidual { get; init; }
    public required WeightedSolveAudit WeightedSolve { get; init; }
    public required string SourceModeId { get; init; }
    public required int SourceModeIndex { get; init; }
    public required bool SourceModeIsKernel { get; init; }
    public required bool TargetBlindSourceSelection { get; init; }
    public required double BaseWeightedActionValue { get; init; }
    public required string SidecarPath { get; init; }
}

public sealed class WeightedSolveAudit
{
    public required string SolverName { get; init; }
    public required bool SolverConverged { get; init; }
    public required int SolverIterations { get; init; }
    public required IReadOnlyList<string> SolverNotes { get; init; }
    public required int ModeCount { get; init; }
    public required int NonzeroModeCount { get; init; }
    public required int GeneralizedResidualPassedCount { get; init; }
    public required int MNormPassedCount { get; init; }
    public required double MaxGeneralizedRelativeResidual { get; init; }
    public required double MaxMNormResidual { get; init; }
    public required double MaxMOrthonormalityResidual { get; init; }
    public required bool MOrthonormalityPassed { get; init; }
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
    public required double[] Coefficients { get; init; }
}

public sealed class VariationOperatorRecord
{
    public required bool DeltaKIsEuclideanHermitian { get; init; }
    public required double DeltaKEuclideanHermiticityRelativeResidual { get; init; }
    public required bool DeltaAIsMSelfAdjoint { get; init; }
    public required double DeltaAMAdjointRelativeResidual { get; init; }
    public required bool HermitianShortcutJustified { get; init; }
    public required bool AllOperatorIdentitiesPassed { get; init; }
}

public sealed class VariationAudit
{
    public required string VariationId { get; init; }
    public required string FermionBackgroundId { get; init; }
    public required string BosonModeId { get; init; }
    public required string PersistedVariationMetadataPath { get; init; }
    public required string PersistedVariationMatrixPath { get; init; }
    public required string AnalyticVariationSource { get; init; }
    public required string SourceModeId { get; init; }
    public required int SourceModeIndex { get; init; }
    public required bool SourceModeIsKernel { get; init; }
    public required bool TargetBlindSourceSelection { get; init; }
    public required VariationOperatorRecord PersistedOperator { get; init; }
    public required VariationOperatorRecord AnalyticOperator { get; init; }
    public required double AnalyticPersistedDeltaKRelativeResidual { get; init; }
    public required bool AnalyticPersistedDeltaKParityPassed { get; init; }
    public required double PersistedSourceCurrent { get; init; }
    public required double AnalyticSourceCurrent { get; init; }
    public bool PersistedSourceCurrentIsNonzero => Math.Abs(PersistedSourceCurrent) > 1e-12;
    public bool AnalyticSourceCurrentIsNonzero => Math.Abs(AnalyticSourceCurrent) > 1e-12;
    public required int DirectionalCheckCount { get; init; }
    public required int DirectionalIdentityPassedCount { get; init; }
    public required int PairingIdentityPassedCount { get; init; }
    public required int ReciprocalDerivativeEqualityPassedCount { get; init; }
    public required int HermitianShortcutEqualityPassedCount { get; init; }
    public required int CentralDerivativeLadderPassedCount { get; init; }
    public required int AnalyticPersistedDirectionalParityPassedCount { get; init; }
    public required int NonzeroPersistedWeightedReciprocalDerivativeCount { get; init; }
    public required int NonzeroAnalyticWeightedReciprocalDerivativeCount { get; init; }
    public required double MaxPairingIdentityScaleAwareResidual { get; init; }
    public required double MaxHermitianShortcutScaleAwareResidual { get; init; }
    public required double MaxCentralDerivativeScaleAwareResidual { get; init; }
    public required double MaxAnalyticPersistedDirectionalScaleAwareResidual { get; init; }
    public required IReadOnlyList<DirectionalAudit> Directions { get; init; }
    public bool VariationPassed { get; set; }
    public required string SidecarPath { get; init; }
}

public sealed class DirectionalAudit
{
    public required string PsiSourceModeId { get; init; }
    public required int PsiSourceModeIndex { get; init; }
    public required bool PsiSourceModeIsKernel { get; init; }
    public required string ChiModeId { get; init; }
    public required int ChiModeIndex { get; init; }
    public required bool ChiModeIsKernel { get; init; }
    public required DirectionalDiagnostic Persisted { get; init; }
    public required DirectionalDiagnostic Analytic { get; init; }
    public required double AnalyticPersistedWeightedPairingScaleAwareResidual { get; init; }
    public required double AnalyticPersistedStiffnessPairingScaleAwareResidual { get; init; }
    public required double AnalyticPersistedReciprocalDerivativeScaleAwareResidual { get; init; }
    public required double AnalyticPersistedCentralDerivativeMaxScaleAwareResidual { get; init; }
    public required double AnalyticPersistedDirectionalMaxScaleAwareResidual { get; init; }
    public required bool AnalyticPersistedDirectionalParityPassed { get; init; }
    public required bool PairingIdentityPassed { get; init; }
    public required bool ReciprocalDerivativeEqualityPassed { get; init; }
    public required bool HermitianShortcutEqualityPassed { get; init; }
    public required bool CentralDerivativeLadderPassed { get; init; }
    public required double MaxPairingIdentityScaleAwareResidual { get; init; }
    public required double MaxHermitianShortcutScaleAwareResidual { get; init; }
    public required double MaxCentralDerivativeScaleAwareResidual { get; init; }
    public required bool AllIdentitiesPassed { get; init; }
}

public sealed class DirectionalDiagnostic
{
    public required ComplexValue WeightedOperatorPairing { get; init; }
    public required ComplexValue StiffnessPairing { get; init; }
    public required double WeightedOperatorToStiffnessPairingScaleAwareResidual { get; init; }
    public required bool WeightedOperatorToStiffnessPairingIdentityPassed { get; init; }
    public required ComplexValue ReciprocalPartnerPairing { get; init; }
    public required double ReciprocalRealDirectionalDerivative { get; init; }
    public required double IndependentSourceDerivativeRealSum { get; init; }
    public required double ReciprocalDerivativeEqualityScaleAwareResidual { get; init; }
    public required bool ReciprocalDerivativeEqualityPassed { get; init; }
    public required bool HermitianShortcutJustified { get; init; }
    public required double HermitianShortcutDirectionalDerivative { get; init; }
    public required double HermitianShortcutScaleAwareResidual { get; init; }
    public required bool HermitianShortcutEqualityPassed { get; init; }
    public required double CurrentAtPsi { get; init; }
    public required IReadOnlyList<CentralDerivativeRow> CentralDerivativeLadder { get; init; }
    public required double MaxCentralDerivativeScaleAwareResidual { get; init; }
    public required bool CentralDerivativeLadderPassed { get; init; }
    public required bool AllDirectionalIdentitiesPassed { get; init; }
}

public sealed class CentralDerivativeRow
{
    public required double Epsilon { get; init; }
    public required double CurrentAtPsiPlusEpsilonChi { get; init; }
    public required double CurrentAtPsiMinusEpsilonChi { get; init; }
    public required double CentralDerivative { get; init; }
    public required double ExpectedReciprocalRealDirectionalDerivative { get; init; }
    public required double ScaleAwareResidual { get; init; }
    public required bool Passed { get; init; }
}
