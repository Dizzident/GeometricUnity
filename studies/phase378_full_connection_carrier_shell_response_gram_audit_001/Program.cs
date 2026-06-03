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

const string DefaultOutputDir = "studies/phase378_full_connection_carrier_shell_response_gram_audit_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string ModeDir = $"{Phase12Root}/spectra/modes";
const string GeometryManifestPath = $"{Phase12Root}/manifest/geometry.json";
const string OmegaMetadataPath = $"{Phase12Root}/background_states/bg-phase12-bg-a-20260315212202_omega.json";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase376FullPath = "studies/phase376_persisted_nonzero_shell_reciprocal_replay_audit_001/output/persisted_nonzero_shell_reciprocal_replay_audit.json";
const string Phase376SummaryPath = "studies/phase376_persisted_nonzero_shell_reciprocal_replay_audit_001/output/persisted_nonzero_shell_reciprocal_replay_audit_summary.json";
const string Phase377FullPath = "studies/phase377_selected_source_mode_shell_response_gram_audit_001/output/selected_source_mode_shell_response_gram_audit.json";
const string Phase377SummaryPath = "studies/phase377_selected_source_mode_shell_response_gram_audit_001/output/selected_source_mode_shell_response_gram_audit_summary.json";
const string Phase377BackgroundDir = "studies/phase377_selected_source_mode_shell_response_gram_audit_001/output/backgrounds";
const int ExpectedBackgroundCount = 2;
const int RequestedModeCount = 53;
const int ExpectedKernelModeCount = 48;
const int ExpectedShellSize = 4;
const int ExpectedSentinelIndex = 52;
const int ExpectedAmbientEdgeCount = 52;
const int ExpectedGaugeDimension = 3;
const int ExpectedConnectionVectorLength = 156;
const int ExpectedSelectedSourceModeCountPerBackground = 12;
const int ExpectedSelectedResponseRank = 3;
const int ExpectedFeatureDimension = 32;
const double AlgebraTolerance = 1e-11;
const double GeneralizedResidualTolerance = 1e-6;
const double MNormTolerance = 1e-8;
const double MOrthonormalityTolerance = 1e-8;
const double KernelTolerance = 1e-12;
const double ResponseTolerance = 1e-12;
const double MatrixTolerance = 1e-8;
const double SymmetryTolerance = 1e-12;
int[] expectedShellIndices = [48, 49, 50, 51];

var options = JsonOptions();
var outputDir = Environment.GetEnvironmentVariable("PHASE378_OUTPUT_DIR") ?? DefaultOutputDir;
var backgroundOutputDir = Path.Combine(outputDir, "backgrounds");
Directory.CreateDirectory(backgroundOutputDir);

using var phase376Full = JsonDocument.Parse(File.ReadAllText(Phase376FullPath));
using var phase376Summary = JsonDocument.Parse(File.ReadAllText(Phase376SummaryPath));
using var phase377Full = JsonDocument.Parse(File.ReadAllText(Phase377FullPath));
using var phase377Summary = JsonDocument.Parse(File.ReadAllText(Phase377SummaryPath));
var p376 = phase376Summary.RootElement;
var p377 = phase377Summary.RootElement;
bool phase376DiscreteShellReplayPresent =
    JsonBool(p376, "persistedNonzeroShellReciprocalReplayAuditPassed") &&
    JsonInt(p376, "backgroundCount") == ExpectedBackgroundCount &&
    JsonInt(p376, "variationPassedCount") == ExpectedBackgroundCount * ExpectedSelectedSourceModeCountPerBackground;
bool phase377SelectedShellResponsePresent =
    JsonBool(p377, "selectedSourceModeShellResponseGramAuditPassed") &&
    JsonInt(p377, "backgroundCount") == ExpectedBackgroundCount &&
    JsonInt(p377, "observedStableResponseRank") == ExpectedSelectedResponseRank;
bool phase376NonpromotionalBoundaryVerified =
    FalseFlag(p376, "routeProvidesPhysicalGuBranch") &&
    FalseFlag(p376, "routeProvidesCanonicalPhysicalMassPsi") &&
    FalseFlag(p376, "routeProvidesCompletedFermionicAction") &&
    FalseFlag(p376, "routeProvidesCompletedMixedLinearizationBlocks") &&
    FalseFlag(p376, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") &&
    FalseFlag(p376, "routeProvidesHiggsScalarSourceOperator") &&
    FalseFlag(p376, "routeProvidesGeVUnitNormalization") &&
    FalseFlag(p376, "routeCompletesBosonPredictions");
bool phase377NonpromotionalBoundaryVerified =
    FalseFlag(p377, "routeProvidesPhysicalGuBranch") &&
    FalseFlag(p377, "routeProvidesCanonicalPhysicalMassPsi") &&
    FalseFlag(p377, "routeProvidesCompletedFermionicAction") &&
    FalseFlag(p377, "routeProvidesCompletedMixedLinearizationBlocks") &&
    FalseFlag(p377, "routeProvidesDirectTargetIndependentWzBridgeSourceLaw") &&
    FalseFlag(p377, "routeProvidesHiggsScalarSourceOperator") &&
    FalseFlag(p377, "routeProvidesGeVUnitNormalization") &&
    FalseFlag(p377, "routeCompletesBosonPredictions");

var selectedRows = phase376Full.RootElement
    .GetProperty("variationAudits")
    .EnumerateArray()
    .Select(ParseSelectedVariation)
    .ToArray();
var selectedRowsByBackground = selectedRows
    .GroupBy(row => row.FermionBackgroundId, StringComparer.Ordinal)
    .ToDictionary(
        group => group.Key,
        group => group.OrderBy(row => row.VariationId, StringComparer.Ordinal).ToArray(),
        StringComparer.Ordinal);

var spinorSpec = JsonSerializer.Deserialize<SpinorRepresentationSpec>(
    File.ReadAllText(SpinorRepresentationPath),
    options) ?? throw new InvalidDataException($"Failed to deserialize {SpinorRepresentationPath}.");
var provenance = new ProvenanceMeta
{
    CreatedAt = DateTimeOffset.UtcNow,
    CodeRevision = "phase378-full-connection-carrier-shell-response-gram-audit",
    Branch = new() { BranchId = "phase378-discrete-full-carrier-response-audit", SchemaVersion = "1.0" },
    Backend = "cpu-reference",
};
var gammas = new GammaMatrixBuilder().Build(
    spinorSpec.CliffordSignature,
    spinorSpec.GammaConvention,
    provenance);
var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int dimG = ExpectedGaugeDimension;
int spinorDim = spinorSpec.SpinorComponents;
int dofsPerVertex = spinorDim * dimG;
double[] massPsiWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerVertex);
double[] edgeLengths = Enumerable.Range(0, mesh.EdgeCount).Select(edge => ComputeEdgeLength(mesh, edge)).ToArray();
double[][] edgeDirections = Enumerable.Range(0, mesh.EdgeCount).Select(edge => ComputeEdgeDirection(mesh, edge)).ToArray();
int[][] cellsPerEdge = Enumerable.Range(0, mesh.EdgeCount).Select(edge => new[] { mesh.Edges[edge][0], mesh.Edges[edge][1] }).ToArray();
var carrierMetadata = LoadCarrierMetadata();
var solver = new FermionSpectralSolver(new CpuDiracOperatorAssembler());
string targetBlindConstructionHash = HashText(string.Join(
    "\n",
    "phase378-full-connection-carrier-shell-response-gram-v1",
    $"phase376TargetBlindConstructionHash={JsonString(p376, "targetBlindConstructionHash")}",
    $"phase377TargetBlindConstructionHash={JsonString(p377, "targetBlindConstructionHash")}",
    $"connectionVectorLength={ExpectedConnectionVectorLength}",
    "coordinateBasis=e_i in fixed Phase12 connection-1form carrier",
    "projectedBlock=G_i=Psi_shell^dagger deltaK[e_i] Psi_shell",
    "responseGram=Q_ij=Re Tr(G_i^dagger G_j)",
    "selectedReconstruction=sum_i b_a[i] G_i",
    "physicalTargetsConsultedForConstruction=false"));

var backgroundAudits = Directory
    .GetFiles(FermionDir, "dirac_bundle_*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(BuildBackgroundAudit)
    .ToArray();

int backgroundCount = backgroundAudits.Length;
int backgroundPassedCount = backgroundAudits.Count(row => row.BackgroundPassed);
int fullCarrierResponsePsdPassedCount = backgroundAudits.Count(row => row.FullCarrierResponse.PsdPassed);
int fullCarrierResponseNonzeroPassedCount = backgroundAudits.Count(row => row.FullCarrierResponse.NonzeroResponsePassed);
int fullCarrierCoordinatePairingPassedCount = backgroundAudits.Count(row => row.CoordinatePairingIdentityPassed);
int selectedReconstructionPassedCount = backgroundAudits.Count(row => row.SelectedRestriction.SelectedBlockReconstructionPassed);
int selectedGramParityPassedCount = backgroundAudits.Count(row => row.SelectedRestriction.SelectedResponseGramMatchesPhase377Passed);
bool stableFullCarrierResponseRankAcrossBackgrounds =
    backgroundAudits.Select(row => row.FullCarrierResponse.PositiveResponseRank).Distinct().Count() == 1;
int observedStableFullCarrierResponseRank = stableFullCarrierResponseRankAcrossBackgrounds
    ? backgroundAudits[0].FullCarrierResponse.PositiveResponseRank
    : -1;
int minFullCarrierResponseRank = backgroundAudits.Min(row => row.FullCarrierResponse.PositiveResponseRank);
int maxFullCarrierResponseRank = backgroundAudits.Max(row => row.FullCarrierResponse.PositiveResponseRank);
int minFullCarrierResponseNullity = backgroundAudits.Min(row => row.FullCarrierResponse.ResponseNullity);
int maxFullCarrierResponseNullity = backgroundAudits.Max(row => row.FullCarrierResponse.ResponseNullity);
double maxCoordinatePairingIdentityRelativeResidual = backgroundAudits.Max(row => row.MaxCoordinatePairingIdentityRelativeResidual);
double maxCoordinateBlockHermiticityRelativeResidual = backgroundAudits.Max(row => row.MaxCoordinateBlockHermiticityRelativeResidual);
double maxSelectedBlockReconstructionRelativeResidual = backgroundAudits.Max(row => row.SelectedRestriction.MaxSelectedBlockReconstructionRelativeResidual);
double maxSelectedResponseGramVsPhase377RelativeResidual = backgroundAudits.Max(row => row.SelectedRestriction.SelectedResponseGramVsPhase377RelativeResidual);
double minFullCarrierResponseTrace = backgroundAudits.Min(row => row.FullCarrierResponse.Trace);
double minFullCarrierDualEigenvalue = backgroundAudits.Min(row => row.FullCarrierResponse.MinimumDualEigenvalue);

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool fullConnectionCarrierCoordinateBasisCovered = true;
const bool studyDefinedHilbertSchmidtPullbackMetric = true;
const bool routeProvidesDiscreteFullConnectionCarrierShellResponseGramPrecursor = true;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
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

bool fullConnectionCarrierShellResponseGramAuditPassed =
    phase376DiscreteShellReplayPresent &&
    phase377SelectedShellResponsePresent &&
    phase376NonpromotionalBoundaryVerified &&
    phase377NonpromotionalBoundaryVerified &&
    carrierMetadata.Passed &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    fullConnectionCarrierCoordinateBasisCovered &&
    studyDefinedHilbertSchmidtPullbackMetric &&
    routeProvidesDiscreteFullConnectionCarrierShellResponseGramPrecursor &&
    backgroundCount == ExpectedBackgroundCount &&
    backgroundPassedCount == ExpectedBackgroundCount &&
    fullCarrierResponsePsdPassedCount == ExpectedBackgroundCount &&
    fullCarrierResponseNonzeroPassedCount == ExpectedBackgroundCount &&
    fullCarrierCoordinatePairingPassedCount == ExpectedBackgroundCount &&
    selectedReconstructionPassedCount == ExpectedBackgroundCount &&
    selectedGramParityPassedCount == ExpectedBackgroundCount &&
    minFullCarrierResponseRank >= ExpectedSelectedResponseRank &&
    maxFullCarrierResponseRank <= ExpectedFeatureDimension &&
    !routeProvidesPhysicalEffectiveActionHessian &&
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

string terminalStatus = fullConnectionCarrierShellResponseGramAuditPassed
    ? "full-connection-carrier-shell-response-gram-validated-discrete-only"
    : "full-connection-carrier-shell-response-gram-audit-blocked";
string decision = fullConnectionCarrierShellResponseGramAuditPassed
    ? $"The fixed Phase12 connection-1form carrier has a target-blind full-coordinate shell-response Gram map on both persisted backgrounds. The selected Phase377 response is exactly recovered as a restriction of the full map, while all physical W/Z/H source contracts remain unfilled. Observed full-carrier response rank range: {minFullCarrierResponseRank}..{maxFullCarrierResponseRank}."
    : "Do not use the full-carrier response precursor until Phase376/377 inheritance, coordinate-basis coverage, PSD/nonzero response, selected-source reconstruction, Phase377 Gram parity, and explicit physical nonclaims all pass.";
var predictionContractImpact = new
{
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    phase201FieldsDefensiblyFilled = Array.Empty<string>(),
};
var result = new
{
    phaseId = "phase378-full-connection-carrier-shell-response-gram-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    fullConnectionCarrierShellResponseGramAuditPassed,
    phase376DiscreteShellReplayPresent,
    phase377SelectedShellResponsePresent,
    phase376NonpromotionalBoundaryVerified,
    phase377NonpromotionalBoundaryVerified,
    implementedObjectClassification = "bounded discrete-only full connection-carrier shell-response Hilbert-Schmidt pullback Gram audit",
    physicalInterpretationBoundary = "study-defined discrete response metric only",
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    fullConnectionCarrierCoordinateBasisCovered,
    studyDefinedHilbertSchmidtPullbackMetric,
    routeProvidesDiscreteFullConnectionCarrierShellResponseGramPrecursor,
    routeProvidesPhysicalEffectiveActionHessian,
    conventionDefinitions = new
    {
        coordinateProjectedBlock = "G_i = Psi_shell^dagger deltaK[e_i] Psi_shell",
        fullCarrierResponseGram = "Q_ij = Re Tr(G_i^dagger G_j)",
        responseQuadraticForm = "c^T Q c = ||sum_i c_i G_i||_F^2 >= 0",
        selectedRestriction = "G[b_a] = sum_i b_a[i] G_i",
    },
    carrierMetadata,
    backgroundCount,
    backgroundPassedCount,
    fullCarrierResponsePsdPassedCount,
    fullCarrierResponseNonzeroPassedCount,
    fullCarrierCoordinatePairingPassedCount,
    selectedReconstructionPassedCount,
    selectedGramParityPassedCount,
    stableFullCarrierResponseRankAcrossBackgrounds,
    observedStableFullCarrierResponseRank,
    minFullCarrierResponseRank,
    maxFullCarrierResponseRank,
    minFullCarrierResponseNullity,
    maxFullCarrierResponseNullity,
    maxCoordinatePairingIdentityRelativeResidual,
    maxCoordinateBlockHermiticityRelativeResidual,
    maxSelectedBlockReconstructionRelativeResidual,
    maxSelectedResponseGramVsPhase377RelativeResidual,
    minFullCarrierResponseTrace,
    minFullCarrierDualEigenvalue,
    tolerances = new
    {
        algebraTolerance = AlgebraTolerance,
        generalizedResidualTolerance = GeneralizedResidualTolerance,
        mNormTolerance = MNormTolerance,
        mOrthonormalityTolerance = MOrthonormalityTolerance,
        explicitKernelTolerance = KernelTolerance,
        matrixTolerance = MatrixTolerance,
        symmetryTolerance = SymmetryTolerance,
        responseTolerance = ResponseTolerance,
        positiveRankToleranceFormula = "max(1e-14, 1e-10 * max(abs(dual response eigenvalue)))",
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
        "full connection-carrier coordinate shell-response Gram precursor only",
        "study-defined Hilbert-Schmidt pullback metric only",
        "not a GU action Hessian",
        "not a regularized fermion-determinant effective-action Hessian",
        "not a gauge-reduced observed-field operator",
        "no physical GU branch",
        "no canonical physical M_psi",
        "no direct W/Z bridge law",
        "no Higgs row",
        "no GeV normalization",
        "no predictions",
        "no contract fills",
    },
    backgroundAudits,
    sourceEvidence = new
    {
        phase376FullPath = Phase376FullPath,
        phase376SummaryPath = Phase376SummaryPath,
        phase377FullPath = Phase377FullPath,
        phase377SummaryPath = Phase377SummaryPath,
        phase12Root = Phase12Root,
        fermionDir = FermionDir,
        modeDir = ModeDir,
        geometryManifestPath = GeometryManifestPath,
        omegaMetadataPath = OmegaMetadataPath,
        spinorRepresentationPath = SpinorRepresentationPath,
    },
    decision,
};

string fullPath = Path.Combine(outputDir, "full_connection_carrier_shell_response_gram_audit.json");
string summaryPath = Path.Combine(outputDir, "full_connection_carrier_shell_response_gram_audit_summary.json");
File.WriteAllText(fullPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(summaryPath, JsonSerializer.Serialize(new
{
    result.phaseId,
    result.generatedAt,
    terminalStatus,
    fullConnectionCarrierShellResponseGramAuditPassed,
    phase376DiscreteShellReplayPresent,
    phase377SelectedShellResponsePresent,
    phase376NonpromotionalBoundaryVerified,
    phase377NonpromotionalBoundaryVerified,
    result.implementedObjectClassification,
    result.physicalInterpretationBoundary,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    fullConnectionCarrierCoordinateBasisCovered,
    studyDefinedHilbertSchmidtPullbackMetric,
    routeProvidesDiscreteFullConnectionCarrierShellResponseGramPrecursor,
    routeProvidesPhysicalEffectiveActionHessian,
    carrierMetadata,
    backgroundCount,
    backgroundPassedCount,
    fullCarrierResponsePsdPassedCount,
    fullCarrierResponseNonzeroPassedCount,
    fullCarrierCoordinatePairingPassedCount,
    selectedReconstructionPassedCount,
    selectedGramParityPassedCount,
    stableFullCarrierResponseRankAcrossBackgrounds,
    observedStableFullCarrierResponseRank,
    minFullCarrierResponseRank,
    maxFullCarrierResponseRank,
    minFullCarrierResponseNullity,
    maxFullCarrierResponseNullity,
    maxCoordinatePairingIdentityRelativeResidual,
    maxCoordinateBlockHermiticityRelativeResidual,
    maxSelectedBlockReconstructionRelativeResidual,
    maxSelectedResponseGramVsPhase377RelativeResidual,
    minFullCarrierResponseTrace,
    minFullCarrierDualEigenvalue,
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
Console.WriteLine($"fullConnectionCarrierShellResponseGramAuditPassed={fullConnectionCarrierShellResponseGramAuditPassed}");
Console.WriteLine($"backgroundPassedCount={backgroundPassedCount}/{backgroundCount}");
Console.WriteLine($"fullCarrierResponsePsdPassedCount={fullCarrierResponsePsdPassedCount}/{backgroundCount}");
Console.WriteLine($"selectedReconstructionPassedCount={selectedReconstructionPassedCount}/{backgroundCount}");
Console.WriteLine($"selectedGramParityPassedCount={selectedGramParityPassedCount}/{backgroundCount}");
Console.WriteLine($"minFullCarrierResponseRank={minFullCarrierResponseRank}");
Console.WriteLine($"maxFullCarrierResponseRank={maxFullCarrierResponseRank}");
Console.WriteLine($"observedStableFullCarrierResponseRank={observedStableFullCarrierResponseRank}");
Console.WriteLine($"maxSelectedBlockReconstructionRelativeResidual={maxSelectedBlockReconstructionRelativeResidual:R}");
Console.WriteLine($"maxSelectedResponseGramVsPhase377RelativeResidual={maxSelectedResponseGramVsPhase377RelativeResidual:R}");
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
    var solve = solver.Solve(
        BuildBundle(backgroundId, layout.LayoutId, metadata, k.ToFlatInterleaved()),
        layout,
        BuildConfig(),
        provenance);
    var weightedSolve = EvaluateWeightedSolve(solve, k);
    var shell = weightedSolve.Shell.Modes;
    var coordinateBlocks = new ComplexValue[ExpectedConnectionVectorLength][][];
    var coordinateBlockMetrics = new BlockMetrics[ExpectedConnectionVectorLength];
    double maxPairingResidual = 0.0;
    double maxHermiticityResidual = 0.0;
    int coordinatePairingPassedCount = 0;
    int coordinateHermiticityPassedCount = 0;
    for (int coordinate = 0; coordinate < ExpectedConnectionVectorLength; coordinate++)
    {
        double[] basis = new double[ExpectedConnectionVectorLength];
        basis[coordinate] = 1.0;
        var (analyticRe, analyticIm) = DiracVariationComputer.ComputeAnalytical(
            basis,
            gammas,
            mesh.VertexCount,
            spinorDim,
            dimG,
            edgeLengths,
            cellsPerEdge,
            edgeDirections);
        var deltaK = new ComplexMatrix(analyticRe, analyticIm);
        var deltaA = deltaK.ScaleRows(massPsiWeights);
        var stiffnessBlock = ProjectBlock(shell, deltaK, null);
        var weightedBlock = ProjectBlock(shell, deltaA, massPsiWeights);
        double pairingResidual = BlockRelativeResidual(stiffnessBlock, weightedBlock);
        var metrics = BuildBlockMetrics(stiffnessBlock);
        coordinateBlocks[coordinate] = stiffnessBlock;
        coordinateBlockMetrics[coordinate] = metrics;
        maxPairingResidual = Math.Max(maxPairingResidual, pairingResidual);
        maxHermiticityResidual = Math.Max(maxHermiticityResidual, metrics.HermiticityRelativeResidual);
        if (pairingResidual <= AlgebraTolerance)
            coordinatePairingPassedCount++;
        if (metrics.IsHermitian)
            coordinateHermiticityPassedCount++;
    }
    var fullCarrierResponse = BuildResponseGramAudit(
        coordinateBlocks,
        coordinateBlockMetrics.Select(metric => metric.FrobeniusNormSquared).ToArray(),
        ExpectedConnectionVectorLength);
    if (!selectedRowsByBackground.TryGetValue(backgroundId, out var selected))
        throw new InvalidDataException($"No Phase376 selected rows found for {backgroundId}.");
    var selectedRestriction = BuildSelectedRestrictionAudit(backgroundId, selected, coordinateBlocks);
    var audit = new BackgroundAudit
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMetadataPath = metadataPath,
        BaseDiracMatrixPath = matrixPath,
        ShellModeIndices = shell.Select(mode => mode.ModeIndex).ToArray(),
        ShellModeEigenvalues = shell.Select(mode => mode.Eigenvalue).ToArray(),
        ShellSelectionPassed = weightedSolve.Shell.ShellSelectionPassed,
        WeightedSolveQualityPassed = weightedSolve.AllQualityChecksPassed,
        FullCarrierCoordinateCount = ExpectedConnectionVectorLength,
        ExpectedConnectionVectorLength = ExpectedConnectionVectorLength,
        CoordinatePairingIdentityPassedCount = coordinatePairingPassedCount,
        CoordinateHermiticityPassedCount = coordinateHermiticityPassedCount,
        CoordinatePairingIdentityPassed = coordinatePairingPassedCount == ExpectedConnectionVectorLength,
        CoordinateBlockHermiticityPassed = coordinateHermiticityPassedCount == ExpectedConnectionVectorLength,
        MaxCoordinatePairingIdentityRelativeResidual = maxPairingResidual,
        MaxCoordinateBlockHermiticityRelativeResidual = maxHermiticityResidual,
        FullCarrierResponse = fullCarrierResponse,
        SelectedRestriction = selectedRestriction,
        BackgroundPassed =
            weightedSolve.AllQualityChecksPassed &&
            weightedSolve.Shell.ShellSelectionPassed &&
            coordinatePairingPassedCount == ExpectedConnectionVectorLength &&
            coordinateHermiticityPassedCount == ExpectedConnectionVectorLength &&
            fullCarrierResponse.PsdPassed &&
            fullCarrierResponse.NonzeroResponsePassed &&
            fullCarrierResponse.DiagonalBlockNormIdentityPassed &&
            selectedRestriction.SelectedBlockReconstructionPassed &&
            selectedRestriction.SelectedResponseGramMatchesPhase377Passed &&
            selectedRestriction.SelectedResponseRank == ExpectedSelectedResponseRank &&
            fullCarrierResponse.PositiveResponseRank >= selectedRestriction.SelectedResponseRank,
    };
    File.WriteAllText(
        Path.Combine(backgroundOutputDir, $"{backgroundId}-full-carrier-shell-response-gram.json"),
        JsonSerializer.Serialize(audit, options));
    return audit;
}

SelectedRestrictionAudit BuildSelectedRestrictionAudit(
    string backgroundId,
    SelectedVariation[] selected,
    ComplexValue[][][] coordinateBlocks)
{
    var reconstructedBlocks = selected
        .Select(row => LinearCombinationBlock(coordinateBlocks, LoadModeVector(row.BosonModeId)))
        .ToArray();
    double maxBlockResidual = selected
        .Select((row, index) => BlockRelativeResidual(reconstructedBlocks[index], row.AnalyticBlock))
        .Max();
    var selectedResponse = BuildResponseGramAudit(
        reconstructedBlocks,
        reconstructedBlocks.Select(block => BuildBlockMetrics(block).FrobeniusNormSquared).ToArray(),
        selected.Length);
    double[][] p377AnalyticGram = LoadPhase377AnalyticResponseGram(backgroundId);
    double gramResidual = MatrixRelativeResidual(selectedResponse.ResponseGram, p377AnalyticGram);
    return new()
    {
        SelectedSourceModeCount = selected.Length,
        ExpectedSelectedSourceModeCount = ExpectedSelectedSourceModeCountPerBackground,
        VariationIds = selected.Select(row => row.VariationId).ToArray(),
        BosonModeIds = selected.Select(row => row.BosonModeId).ToArray(),
        MaxSelectedBlockReconstructionRelativeResidual = maxBlockResidual,
        SelectedBlockReconstructionPassed = maxBlockResidual <= MatrixTolerance,
        SelectedResponseRank = selectedResponse.PositiveResponseRank,
        ExpectedSelectedResponseRank = ExpectedSelectedResponseRank,
        SelectedResponseGram = selectedResponse.ResponseGram,
        SelectedResponseGramVsPhase377RelativeResidual = gramResidual,
        SelectedResponseGramMatchesPhase377Passed = gramResidual <= MatrixTolerance,
    };
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
        modes.Count(mode => mode.IsKernel) == ExpectedKernelModeCount &&
        modes.All(mode => mode.GeneralizedResidualPassed) &&
        modes.All(mode => mode.MNormPassed) &&
        maxOrthResidual <= MOrthonormalityTolerance &&
        shell.ShellSelectionPassed;
    return new()
    {
        SolverConverged = solve.Diagnostics.Converged,
        RequestedModeCount = RequestedModeCount,
        ModeCount = modes.Length,
        KernelModeCount = modes.Count(mode => mode.IsKernel),
        MaxGeneralizedRelativeResidual = modes.Max(mode => mode.GeneralizedKPsiEqualsLambdaMPsiRelativeResidual),
        MaxMNormResidual = modes.Max(mode => mode.MNormResidual),
        MaxMOrthonormalityResidual = maxOrthResidual,
        AllQualityChecksPassed = qualityChecksPassed,
        Shell = shell,
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
        LambdaMinNonzeroMagnitude = lambdaMinMagnitude,
        GroupingTolerance = groupingTolerance,
        ShellIndices = shellModes.Select(mode => mode.ModeIndex).ToArray(),
        ShellSize = shellModes.Length,
        SentinelIndex = sentinel.ModeIndex,
        SentinelEigenvalue = sentinel.Eigenvalue,
        SentinelOutsideShell = sentinelOutsideShell,
        ShellSelectionPassed = selectionPassed,
        Modes = shellModes,
    };
}

ResponseGramAudit BuildResponseGramAudit(
    ComplexValue[][][] blocks,
    double[] expectedDiagonal,
    int carrierDimension)
{
    double[][] features = blocks.Select(BlockFeature).ToArray();
    double[][] responseGram = RealGram(features);
    double[][] dualFeatureGram = FeatureDualGram(features);
    double symmetryResidual = SymmetryRelativeResidual(responseGram);
    double diagonalResidual = Enumerable.Range(0, responseGram.Length)
        .Max(index => ScaleAwareResidual(responseGram[index][index], expectedDiagonal[index]));
    double[] dualEigenvalues = SymmetricEigenvalues(dualFeatureGram);
    double maxMagnitude = dualEigenvalues.Max(value => Math.Abs(value));
    double rankTolerance = Math.Max(1e-14, 1e-10 * maxMagnitude);
    int positiveRank = dualEigenvalues.Count(value => value > rankTolerance);
    double trace = Enumerable.Range(0, responseGram.Length).Sum(index => responseGram[index][index]);
    var topCoordinateResponses = Enumerable.Range(0, responseGram.Length)
        .Select(index => new CoordinateResponse(index, responseGram[index][index]))
        .OrderByDescending(row => row.FrobeniusNormSquared)
        .Take(12)
        .ToArray();
    return new()
    {
        Construction = "Q_ij = Re Tr(G_i^dagger G_j)",
        ResponseGram = responseGram,
        DualFeatureGram = dualFeatureGram,
        DualEigenvaluesAscending = dualEigenvalues,
        PositiveRankTolerance = rankTolerance,
        PositiveResponseRank = positiveRank,
        ResponseNullity = carrierDimension - positiveRank,
        MinimumDualEigenvalue = dualEigenvalues[0],
        MaximumDualEigenvalue = dualEigenvalues[^1],
        Trace = trace,
        SymmetryRelativeResidual = symmetryResidual,
        SymmetryPassed = symmetryResidual <= SymmetryTolerance,
        DiagonalBlockNormIdentityRelativeResidual = diagonalResidual,
        DiagonalBlockNormIdentityPassed = diagonalResidual <= MatrixTolerance,
        PsdByConstruction = true,
        PsdDualEigenvaluePassed = dualEigenvalues[0] >= -rankTolerance,
        PsdPassed = dualEigenvalues[0] >= -rankTolerance,
        NonzeroResponsePassed = trace > ResponseTolerance,
        MaxCoordinateFrobeniusNormSquared = expectedDiagonal.Max(),
        ZeroCoordinateResponseCount = expectedDiagonal.Count(value => value <= ResponseTolerance),
        TopCoordinateResponses = topCoordinateResponses,
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

ComplexValue[][] LinearCombinationBlock(ComplexValue[][][] basisBlocks, double[] coefficients)
{
    if (coefficients.Length != basisBlocks.Length)
        throw new InvalidDataException("Full-carrier coefficient vector length must match coordinate block count.");
    var result = Enumerable.Range(0, ExpectedShellSize)
        .Select(_ => Enumerable.Range(0, ExpectedShellSize).Select(_ => Complex.Zero).ToArray())
        .ToArray();
    for (int coordinate = 0; coordinate < basisBlocks.Length; coordinate++)
        if (Math.Abs(coefficients[coordinate]) > 0.0)
            for (int row = 0; row < ExpectedShellSize; row++)
                for (int col = 0; col < ExpectedShellSize; col++)
                    result[row][col] += coefficients[coordinate] * ToComplex(basisBlocks[coordinate][row][col]);
    return result
        .Select(row => row.Select(FromComplex).ToArray())
        .ToArray();
}

BlockMetrics BuildBlockMetrics(ComplexValue[][] block)
{
    double norm2 = 0.0;
    double hermitianResidual2 = 0.0;
    for (int row = 0; row < block.Length; row++)
        for (int col = 0; col < block[row].Length; col++)
        {
            Complex entry = ToComplex(block[row][col]);
            Complex adjointEntry = Complex.Conjugate(ToComplex(block[col][row]));
            norm2 += entry.Magnitude * entry.Magnitude;
            hermitianResidual2 += (entry - adjointEntry).Magnitude * (entry - adjointEntry).Magnitude;
        }
    double hermiticityResidual = Math.Sqrt(hermitianResidual2 / Math.Max(norm2, 1e-300));
    return new()
    {
        FrobeniusNormSquared = norm2,
        HermiticityRelativeResidual = hermiticityResidual,
        IsHermitian = hermiticityResidual <= AlgebraTolerance,
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
        carrierType == "connection-1form" &&
        ambientEdgeCount == ExpectedAmbientEdgeCount &&
        shape.SequenceEqual([ExpectedAmbientEdgeCount, ExpectedGaugeDimension]) &&
        coefficientCount == ExpectedConnectionVectorLength;
    return new()
    {
        MetadataAvailable = true,
        CarrierTypeSourcePath = OmegaMetadataPath,
        GeometrySourcePath = GeometryManifestPath,
        CarrierType = carrierType,
        AmbientEdgeCount = ambientEdgeCount,
        GaugeDimension = ExpectedGaugeDimension,
        ConnectionShape = shape,
        ConnectionVectorLength = coefficientCount,
        ExpectedConnectionVectorLength = ExpectedConnectionVectorLength,
        Passed = passed,
    };
}

DiracOperatorBundle BuildBundle(
    string backgroundId,
    string layoutId,
    JsonElement metadata,
    double[] explicitMatrix) => new()
{
    OperatorId = $"phase378-stiffness-k-{backgroundId}",
    FermionBackgroundId = backgroundId,
    LayoutId = layoutId,
    SpinConnectionId = $"phase378-stiffness-k-{backgroundId}",
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
    DiagnosticNotes = ["Phase378 full-carrier shell response audit."],
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

SelectedVariation ParseSelectedVariation(JsonElement row) => new()
{
    VariationId = JsonString(row, "variationId"),
    FermionBackgroundId = JsonString(row, "fermionBackgroundId"),
    BosonModeId = JsonString(row, "bosonModeId"),
    AnalyticBlock = ParseBlock(row.GetProperty("analyticBlock").GetProperty("stiffnessProjectedBlock")),
};

static ComplexValue[][] ParseBlock(JsonElement element) => element
    .EnumerateArray()
    .Select(row => row.EnumerateArray().Select(value =>
        new ComplexValue(JsonDouble(value, "real"), JsonDouble(value, "imaginary"))).ToArray())
    .ToArray();

static double[] LoadModeVector(string bosonModeId)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(ModeDir, $"{bosonModeId}.json")));
    return doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray();
}

static double[][] LoadPhase377AnalyticResponseGram(string backgroundId)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(Path.Combine(Phase377BackgroundDir, $"{backgroundId}-shell-response-gram.json")));
    return doc.RootElement
        .GetProperty("analyticResponseGram")
        .GetProperty("responseGram")
        .EnumerateArray()
        .Select(row => row.EnumerateArray().Select(value => value.GetDouble()).ToArray())
        .ToArray();
}

static double[] BlockFeature(ComplexValue[][] block)
{
    var result = new double[2 * block.Length * block[0].Length];
    int index = 0;
    for (int row = 0; row < block.Length; row++)
        for (int col = 0; col < block[row].Length; col++)
        {
            result[index++] = block[row][col].Real;
            result[index++] = block[row][col].Imaginary;
        }
    return result;
}

static double[][] RealGram(double[][] features) => features
    .Select(left => features.Select(right => left.Zip(right, (l, r) => l * r).Sum()).ToArray())
    .ToArray();

static double[][] FeatureDualGram(double[][] features)
{
    int dim = features[0].Length;
    var result = Enumerable.Range(0, dim).Select(_ => new double[dim]).ToArray();
    foreach (double[] feature in features)
        for (int row = 0; row < dim; row++)
            for (int col = 0; col < dim; col++)
                result[row][col] += feature[row] * feature[col];
    return result;
}

static double[] SymmetricEigenvalues(double[][] input)
{
    int n = input.Length;
    var matrix = input.Select(row => row.ToArray()).ToArray();
    for (int iteration = 0; iteration < 100 * n * n; iteration++)
    {
        int pivotRow = 0;
        int pivotCol = 1;
        double maxOffDiagonal = 0.0;
        for (int row = 0; row < n; row++)
            for (int col = row + 1; col < n; col++)
                if (Math.Abs(matrix[row][col]) > maxOffDiagonal)
                {
                    maxOffDiagonal = Math.Abs(matrix[row][col]);
                    pivotRow = row;
                    pivotCol = col;
                }
        if (maxOffDiagonal <= 1e-16)
            break;
        double angle = 0.5 * Math.Atan2(
            2.0 * matrix[pivotRow][pivotCol],
            matrix[pivotCol][pivotCol] - matrix[pivotRow][pivotRow]);
        double cosine = Math.Cos(angle);
        double sine = Math.Sin(angle);
        for (int index = 0; index < n; index++)
            if (index != pivotRow && index != pivotCol)
            {
                double left = matrix[index][pivotRow];
                double right = matrix[index][pivotCol];
                matrix[index][pivotRow] = matrix[pivotRow][index] = cosine * left - sine * right;
                matrix[index][pivotCol] = matrix[pivotCol][index] = sine * left + cosine * right;
            }
        double diagonalLeft = matrix[pivotRow][pivotRow];
        double diagonalRight = matrix[pivotCol][pivotCol];
        double offDiagonal = matrix[pivotRow][pivotCol];
        matrix[pivotRow][pivotRow] =
            cosine * cosine * diagonalLeft -
            2.0 * sine * cosine * offDiagonal +
            sine * sine * diagonalRight;
        matrix[pivotCol][pivotCol] =
            sine * sine * diagonalLeft +
            2.0 * sine * cosine * offDiagonal +
            cosine * cosine * diagonalRight;
        matrix[pivotRow][pivotCol] = matrix[pivotCol][pivotRow] = 0.0;
    }
    return Enumerable.Range(0, n).Select(index => matrix[index][index]).Order().ToArray();
}

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

static string JsonString(JsonElement element, string propertyName) =>
    RequiredString(element, propertyName);

static double JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
        ? value.GetDouble()
        : throw new InvalidDataException($"{propertyName} must be a number.");

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

static double MatrixRelativeResidual(double[][] left, double[][] right)
{
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < left.Length; row++)
        for (int col = 0; col < left[row].Length; col++)
        {
            residual2 += Square(left[row][col] - right[row][col]);
            norm2 += Square(left[row][col]);
        }
    return Math.Sqrt(residual2 / Math.Max(norm2, 1e-300));
}

static double SymmetryRelativeResidual(double[][] matrix)
{
    double residual2 = 0.0;
    double norm2 = 0.0;
    for (int row = 0; row < matrix.Length; row++)
        for (int col = 0; col < matrix[row].Length; col++)
        {
            residual2 += Square(matrix[row][col] - matrix[col][row]);
            norm2 += Square(matrix[row][col]);
        }
    return Math.Sqrt(residual2 / Math.Max(norm2, 1e-300));
}

static double RelativeVectorResidual(double[] left, double[] right) =>
    EuclideanNorm(Subtract(left, right)) / Math.Max(1e-300, Math.Max(EuclideanNorm(left), EuclideanNorm(right)));

static double[] Scale(double[] vector, double scale) =>
    vector.Select(value => scale * value).ToArray();

static double[] Subtract(double[] left, double[] right) =>
    left.Zip(right, (l, r) => l - r).ToArray();

static double[] ApplyWeights(double[] vector, double[] weights) =>
    vector.Zip(weights, (value, weight) => value * weight).ToArray();

static double EuclideanNorm(double[] vector) =>
    Math.Sqrt(vector.Sum(Square));

static double ScaleAwareResidual(double left, double right) =>
    Math.Abs(left - right) / Math.Max(1.0, Math.Max(Math.Abs(left), Math.Abs(right)));

static double ComplexMagnitude(double real, double imaginary) =>
    Math.Sqrt(Square(real) + Square(imaginary));

static double Square(double value) => value * value;

static Complex ToComplex(ComplexValue value) => new(value.Real, value.Imaginary);

static ComplexValue FromComplex(Complex value) => new(value.Real, value.Imaginary);

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

static string HashText(string text) =>
    Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();

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
public sealed record CoordinateResponse(int CoordinateIndex, double FrobeniusNormSquared);

public sealed class CarrierMetadataAudit
{
    public required bool MetadataAvailable { get; init; }
    public required string CarrierTypeSourcePath { get; init; }
    public required string GeometrySourcePath { get; init; }
    public required string CarrierType { get; init; }
    public required int AmbientEdgeCount { get; init; }
    public required int GaugeDimension { get; init; }
    public required int[] ConnectionShape { get; init; }
    public required int ConnectionVectorLength { get; init; }
    public required int ExpectedConnectionVectorLength { get; init; }
    public required bool Passed { get; init; }
}

public sealed class WeightedModeAudit
{
    public required string ModeId { get; init; }
    public required int ModeIndex { get; init; }
    public required double Eigenvalue { get; init; }
    public required bool IsKernel { get; init; }
    public required double GeneralizedKPsiEqualsLambdaMPsiRelativeResidual { get; init; }
    public required bool GeneralizedResidualPassed { get; init; }
    public required double MNorm { get; init; }
    public required double MNormResidual { get; init; }
    public required bool MNormPassed { get; init; }
    [JsonIgnore]
    public double[] Coefficients { get; init; } = [];
}

public sealed class ShellAudit
{
    public required double LambdaMinNonzeroMagnitude { get; init; }
    public required double GroupingTolerance { get; init; }
    public required int[] ShellIndices { get; init; }
    public required int ShellSize { get; init; }
    public required int SentinelIndex { get; init; }
    public required double SentinelEigenvalue { get; init; }
    public required bool SentinelOutsideShell { get; init; }
    public required bool ShellSelectionPassed { get; init; }
    public required IReadOnlyList<WeightedModeAudit> Modes { get; init; }
}

public sealed class WeightedSolveAudit
{
    public required bool SolverConverged { get; init; }
    public required int RequestedModeCount { get; init; }
    public required int ModeCount { get; init; }
    public required int KernelModeCount { get; init; }
    public required double MaxGeneralizedRelativeResidual { get; init; }
    public required double MaxMNormResidual { get; init; }
    public required double MaxMOrthonormalityResidual { get; init; }
    public required bool AllQualityChecksPassed { get; init; }
    public required ShellAudit Shell { get; init; }
}

public sealed class BlockMetrics
{
    public required double FrobeniusNormSquared { get; init; }
    public required double HermiticityRelativeResidual { get; init; }
    public required bool IsHermitian { get; init; }
}

public sealed class ResponseGramAudit
{
    public required string Construction { get; init; }
    public required double[][] ResponseGram { get; init; }
    public required double[][] DualFeatureGram { get; init; }
    public required double[] DualEigenvaluesAscending { get; init; }
    public required double PositiveRankTolerance { get; init; }
    public required int PositiveResponseRank { get; init; }
    public required int ResponseNullity { get; init; }
    public required double MinimumDualEigenvalue { get; init; }
    public required double MaximumDualEigenvalue { get; init; }
    public required double Trace { get; init; }
    public required double SymmetryRelativeResidual { get; init; }
    public required bool SymmetryPassed { get; init; }
    public required double DiagonalBlockNormIdentityRelativeResidual { get; init; }
    public required bool DiagonalBlockNormIdentityPassed { get; init; }
    public required bool PsdByConstruction { get; init; }
    public required bool PsdDualEigenvaluePassed { get; init; }
    public required bool PsdPassed { get; init; }
    public required bool NonzeroResponsePassed { get; init; }
    public required double MaxCoordinateFrobeniusNormSquared { get; init; }
    public required int ZeroCoordinateResponseCount { get; init; }
    public required CoordinateResponse[] TopCoordinateResponses { get; init; }
}

public sealed class SelectedVariation
{
    public required string VariationId { get; init; }
    public required string FermionBackgroundId { get; init; }
    public required string BosonModeId { get; init; }
    public required ComplexValue[][] AnalyticBlock { get; init; }
}

public sealed class SelectedRestrictionAudit
{
    public required int SelectedSourceModeCount { get; init; }
    public required int ExpectedSelectedSourceModeCount { get; init; }
    public required string[] VariationIds { get; init; }
    public required string[] BosonModeIds { get; init; }
    public required double MaxSelectedBlockReconstructionRelativeResidual { get; init; }
    public required bool SelectedBlockReconstructionPassed { get; init; }
    public required int SelectedResponseRank { get; init; }
    public required int ExpectedSelectedResponseRank { get; init; }
    public required double[][] SelectedResponseGram { get; init; }
    public required double SelectedResponseGramVsPhase377RelativeResidual { get; init; }
    public required bool SelectedResponseGramMatchesPhase377Passed { get; init; }
}

public sealed class BackgroundAudit
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMetadataPath { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required int[] ShellModeIndices { get; init; }
    public required double[] ShellModeEigenvalues { get; init; }
    public required bool ShellSelectionPassed { get; init; }
    public required bool WeightedSolveQualityPassed { get; init; }
    public required int FullCarrierCoordinateCount { get; init; }
    public required int ExpectedConnectionVectorLength { get; init; }
    public required int CoordinatePairingIdentityPassedCount { get; init; }
    public required int CoordinateHermiticityPassedCount { get; init; }
    public required bool CoordinatePairingIdentityPassed { get; init; }
    public required bool CoordinateBlockHermiticityPassed { get; init; }
    public required double MaxCoordinatePairingIdentityRelativeResidual { get; init; }
    public required double MaxCoordinateBlockHermiticityRelativeResidual { get; init; }
    public required ResponseGramAudit FullCarrierResponse { get; init; }
    public required SelectedRestrictionAudit SelectedRestriction { get; init; }
    public required bool BackgroundPassed { get; init; }
}
