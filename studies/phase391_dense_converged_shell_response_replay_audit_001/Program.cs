using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

// Phase391: dense converged-shell shell-response replay audit.
//
// Phase390 proved the persisted Phase12 fermion modes are non-eigen mixtures
// and rebuilt converged modes densely. The Phase376-379 shell-response chain
// did NOT use those persisted modes directly: Phase378 ran its own weighted
// generalized solve (FermionSpectralSolver, K psi = lambda M_psi psi) with
// in-study residual checks, then built the full 156-coordinate carrier
// shell-response Gram on the lowest nonzero spectral shell. Its headline
// invariants - stable positive rank 3, two-axis gauge dominance with
// suppressed gauge axis 1, and failed strict background-to-background
// transport - are the active blockers behind the Phase307 W near-pass
// rejection (Phases 381/383/384) and two Phase388 theorem requirements.
//
// This probe replays the entire pipeline with an EXACT dense reference:
//
//   1. Dense complex Hermitian Jacobi solve of B = M^{-1/2} D M^{-1/2}
//      (Phase390 solver), reconstructing M-orthonormal generalized modes
//      v = M^{-1/2} w with D v = lambda M v at solver precision.
//   2. Lowest-nonzero-shell selection with the Phase378 grouping rule
//      (tolerance max(1e-12, 1e-8 * |lambda_min|)).
//   3. Per-coordinate blocks G_i = V_shell^dagger deltaK[e_i] V_shell for all
//      156 carrier coordinates, with deltaK from
//      DiracVariationComputer.ComputeAnalytical on unit basis vectors -
//      identical to Phase378.
//   4. The dual feature Gram, its positive rank with the Phase378 tolerance
//      formula, the 156x156 response Gram Q_ij = Re Tr(G_i^dagger G_j), its
//      top-rank eigenvectors, the gauge-axis projector fractions
//      (coordinate % 3), and the inter-background transport singular values -
//      identical to Phase379.
//   5. Quantitative comparison against the persisted Phase378/379 results.
//
// Because the Gram invariants depend only on the shell SUBSPACE (any
// M-orthonormal basis of the shell gives the same Q), this is a sharp test of
// whether the Phase378/379 rank-3 image and suppressed axis are properties of
// the discretized branch or artifacts of the iterative weighted solver.
//
// This is a fail-closed audit either way: it provides no physical branch, no
// observed namespace map, and no Phase201/Phase256 contract fields.

const string DefaultOutputDir = "studies/phase391_dense_converged_shell_response_replay_audit_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase378FullPath = "studies/phase378_full_connection_carrier_shell_response_gram_audit_001/output/full_connection_carrier_shell_response_gram_audit.json";
const string Phase378SummaryPath = "studies/phase378_full_connection_carrier_shell_response_gram_audit_001/output/full_connection_carrier_shell_response_gram_audit_summary.json";
const string Phase379SummaryPath = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const string Phase390SummaryPath = "studies/phase390_converged_control_branch_fermion_mode_rebuild_001/output/converged_control_branch_fermion_mode_rebuild_summary.json";

const int ExpectedBackgroundCount = 2;
const int CarrierDimension = 156;
const int DimG = 3;
const int ExpectedShellSize = 4;
const double JacobiOffDiagonalTolerance = 1e-13;
const int JacobiMaxSweeps = 100;
const double EigenResidualTolerance = 1e-9;
const double BlockHermiticityTolerance = 1e-10;
const double ShellEigenvalueComparisonTolerance = 1e-6;

var outputDir = Environment.GetEnvironmentVariable("PHASE391_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors and persisted comparison targets.
// ---------------------------------------------------------------------------

using var phase378Summary = JsonDocument.Parse(File.ReadAllText(Phase378SummaryPath));
bool phase378PrecursorPassed =
    JsonBool(phase378Summary.RootElement, "fullConnectionCarrierShellResponseGramAuditPassed") is true &&
    JsonInt(phase378Summary.RootElement, "observedStableFullCarrierResponseRank") == 3 &&
    JsonBool(phase378Summary.RootElement, "routeProvidesPhysicalEffectiveActionHessian") is false;

using var phase379Summary = JsonDocument.Parse(File.ReadAllText(Phase379SummaryPath));
bool phase379PrecursorPassed =
    JsonBool(phase379Summary.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true &&
    JsonInt(phase379Summary.RootElement, "stableSuppressedGaugeAxis") == 1 &&
    JsonBool(phase379Summary.RootElement, "strictBackgroundImageTransportPassed") is false;
double phase379InterBackgroundMinimumSingularValue =
    phase379Summary.RootElement.GetProperty("interBackgroundMinimumSingularValue").GetDouble();

using var phase390Doc = JsonDocument.Parse(File.ReadAllText(Phase390SummaryPath));
bool phase390PrecursorPassed =
    JsonBool(phase390Doc.RootElement, "convergedControlBranchFermionModeRebuildPassed") is true &&
    JsonBool(phase390Doc.RootElement, "mPsiCompatibleGeneralizedControlBranchMaterialized") is true &&
    JsonBool(phase390Doc.RootElement, "persistedPhase12ModeBranchUnconverged") is true;

using var phase378Full = JsonDocument.Parse(File.ReadAllText(Phase378FullPath));
var phase378Backgrounds = phase378Full.RootElement
    .GetProperty("backgroundAudits")
    .EnumerateArray()
    .ToDictionary(
        element => RequiredString(element, "fermionBackgroundId"),
        element => element.GetProperty("shellModeEigenvalues").EnumerateArray().Select(value => value.GetDouble()).ToArray(),
        StringComparer.Ordinal);

var phase379Backgrounds = phase379Summary.RootElement
    .GetProperty("backgroundSummaries")
    .EnumerateArray()
    .ToDictionary(
        element => RequiredString(element, "fermionBackgroundId"),
        element => new Phase379BackgroundTarget
        {
            PositiveResponseRank = element.GetProperty("positiveResponseRank").GetInt32(),
            SuppressedGaugeAxisIndex = element.GetProperty("suppressedGaugeAxisIndex").GetInt32(),
            GaugeAxisProjectorFractions = element.GetProperty("gaugeAxisProjectorFractions").EnumerateArray().Select(value => value.GetDouble()).ToArray(),
        },
        StringComparer.Ordinal);

// ---------------------------------------------------------------------------
// Shared geometry and representation data.
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
        CodeRevision = "phase391-dense-converged-shell-response-replay-audit",
        Branch = new() { BranchId = "phase391-dense-converged-shell-response-replay-audit", SchemaVersion = "1.0" },
        Backend = "cpu-reference",
    });

var mesh = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: 2, cols: 2).AmbientMesh;
int spinorDim = spinorSpec.SpinorComponents;
int dofsPerCell = spinorDim * DimG;
int vertexCount = mesh.VertexCount;
int totalDof = vertexCount * dofsPerCell;
int edgeCount = mesh.EdgeCount;
if (edgeCount * DimG != CarrierDimension)
    throw new InvalidDataException($"Carrier dimension mismatch: {edgeCount * DimG} != {CarrierDimension}.");

var edgeLengths = new double[edgeCount];
var edgeDirections = new double[edgeCount][];
var cellsPerEdge = new int[edgeCount][];
for (int edge = 0; edge < edgeCount; edge++)
{
    edgeLengths[edge] = ComputeEdgeLength(mesh, edge);
    edgeDirections[edge] = ComputeEdgeDirection(mesh, edge);
    cellsPerEdge[edge] = [mesh.Edges[edge][0], mesh.Edges[edge][1]];
}

double[] meshVolumeMassPsiWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerCell);

// ---------------------------------------------------------------------------
// Per-background dense replay.
// ---------------------------------------------------------------------------

var backgroundIds = Directory.GetFiles(FermionDir, "dirac_bundle_*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .Select(path => Path.GetFileNameWithoutExtension(path)["dirac_bundle_".Length..])
    .OrderBy(id => id, StringComparer.Ordinal)
    .ToArray();

var backgroundRecords = backgroundIds.Select(ReplayBackground).ToArray();

int backgroundCount = backgroundRecords.Length;
bool expectedCoveragePresent =
    backgroundCount == ExpectedBackgroundCount &&
    backgroundRecords.All(record => record.ShellSize == ExpectedShellSize && record.CoordinateCount == CarrierDimension);
bool denseSolveConverged = backgroundRecords.All(record => record.MaxShellEigenResidual <= EigenResidualTolerance);
bool blockHermiticityPassed = backgroundRecords.All(record => record.MaxBlockHermiticityResidual <= BlockHermiticityTolerance);
bool shellEigenvaluesMatchPhase378 = backgroundRecords.All(record => record.ShellEigenvalueMaxRelativeDelta <= ShellEigenvalueComparisonTolerance);
bool denseReplayConfirmsRankThree = backgroundRecords.All(record => record.PositiveRank == 3 && record.RankMatchesPhase379);
bool denseReplayConfirmsSuppressedAxis = backgroundRecords.All(record => record.SuppressedAxisIndex == 1 && record.SuppressedAxisMatchesPhase379);
double maxAxisFractionAbsDelta = backgroundRecords.Max(record => record.AxisFractionMaxAbsDeltaVsPhase379);
double maxShellEigenvalueRelativeDelta = backgroundRecords.Max(record => record.ShellEigenvalueMaxRelativeDelta);
double maxShellEigenResidual = backgroundRecords.Max(record => record.MaxShellEigenResidual);
double maxBlockHermiticityResidual = backgroundRecords.Max(record => record.MaxBlockHermiticityResidual);

// Inter-background transport of the dense positive eigenspaces.
double denseInterBackgroundMinimumSingularValue = double.NaN;
double denseInterBackgroundMaximumPrincipalAngleDegrees = double.NaN;
double transportDeltaVsPhase379 = double.NaN;
bool denseStrictTransportPassed = false;
if (backgroundRecords.Length == 2 &&
    backgroundRecords[0].PositiveRank == backgroundRecords[1].PositiveRank &&
    backgroundRecords[0].PositiveRank > 0)
{
    int rank = backgroundRecords[0].PositiveRank;
    var overlap = new double[rank, rank];
    for (int i = 0; i < rank; i++)
        for (int j = 0; j < rank; j++)
        {
            double sum = 0.0;
            for (int c = 0; c < CarrierDimension; c++)
                sum += backgroundRecords[0].PositiveEigenvectors[i][c] * backgroundRecords[1].PositiveEigenvectors[j][c];
            overlap[i, j] = sum;
        }
    var overlapGram = new double[rank, rank];
    for (int i = 0; i < rank; i++)
        for (int j = 0; j < rank; j++)
        {
            double sum = 0.0;
            for (int k = 0; k < rank; k++)
                sum += overlap[i, k] * overlap[j, k];
            overlapGram[i, j] = sum;
        }
    var (gramEigenvalues, _, _, _, _) = JacobiHermitian(overlapGram, new double[rank, rank]);
    var singularValues = gramEigenvalues.Select(value => Math.Sqrt(Math.Max(value, 0.0))).OrderBy(value => value).ToArray();
    denseInterBackgroundMinimumSingularValue = singularValues[0];
    denseInterBackgroundMaximumPrincipalAngleDegrees =
        Math.Acos(Math.Clamp(singularValues[0], -1.0, 1.0)) * 180.0 / Math.PI;
    transportDeltaVsPhase379 = Math.Abs(denseInterBackgroundMinimumSingularValue - phase379InterBackgroundMinimumSingularValue);
    denseStrictTransportPassed = denseInterBackgroundMinimumSingularValue >= 0.99;
}

bool denseReplayInternallyConsistent =
    expectedCoveragePresent &&
    denseSolveConverged &&
    blockHermiticityPassed &&
    !double.IsNaN(denseInterBackgroundMinimumSingularValue);

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesCanonicalGaugeAxisSelector = false;
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
const string ApplicationSubjectKind = "dense-converged-shell-response-replay-audit";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    vertexCount.ToString(),
    edgeCount.ToString(),
    DimG.ToString(),
    spinorDim.ToString(),
    ExpectedShellSize.ToString(),
    "G_i = V_shell^dagger deltaK[e_i] V_shell on the dense lowest-nonzero shell",
    "Q_ij = Re Tr(G_i^dagger G_j); axis = coordinate % 3",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool denseConvergedShellResponseReplayAuditPassed =
    phase378PrecursorPassed &&
    phase379PrecursorPassed &&
    phase390PrecursorPassed &&
    denseReplayInternallyConsistent &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeProvidesCanonicalGaugeAxisSelector &&
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

string verdict = denseReplayConfirmsRankThree && denseReplayConfirmsSuppressedAxis && shellEigenvaluesMatchPhase378
    ? "confirmed"
    : "diverged";
string terminalStatus = denseConvergedShellResponseReplayAuditPassed
    ? $"dense-converged-shell-response-replay-{verdict}-discrete-diagnostic-only"
    : "dense-converged-shell-response-replay-audit-blocked";
string decision = !denseConvergedShellResponseReplayAuditPassed
    ? "Do not use the dense replay until the Phase378/379/390 precursors, dense solve convergence, block Hermiticity, and transport computation all pass."
    : verdict == "confirmed"
        ? "The exact dense generalized eigensolve reproduces the Phase378/379 shell-response invariants: the lowest nonzero shell matches the Phase378 weighted-solver shell eigenvalues, the full-carrier response Gram has stable positive rank 3, the gauge-axis projector fractions show the same two-axis dominance with suppressed gauge axis 1, and the inter-background transport remains loose (strict transport still fails). The rank-3 image and the suppressed axis are therefore properties of the discretized control branch, NOT artifacts of the Phase378 iterative weighted solver, and the Phase381/383/384 suppressed-axis blockers against the Phase307 W near-pass stand on solver-independent ground. This remains a study-defined discrete diagnostic: no physical Hessian, observed namespace map, canonical axis selector, W/Z/H source rows, or Phase201/Phase256 contract fields are provided."
        : "The exact dense generalized eigensolve does NOT reproduce the Phase378/379 shell-response invariants; the recorded deltas quantify the divergence. Treat the Phase378/379 rank/axis conclusions and every downstream suppressed-axis blocker as solver-sensitive until re-derived, and do not promote anything from this chain.";

var result = new
{
    phaseId = "phase391-dense-converged-shell-response-replay-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    denseConvergedShellResponseReplayAuditPassed,
    phase378PrecursorPassed,
    phase379PrecursorPassed,
    phase390PrecursorPassed,
    denseReplayInternallyConsistent,
    denseReplayVerdict = verdict,
    denseReplayConfirmsRankThree,
    denseReplayConfirmsSuppressedAxis,
    shellEigenvaluesMatchPhase378,
    denseSolveConverged,
    blockHermiticityPassed,
    denseStrictTransportPassed,
    denseInterBackgroundMinimumSingularValue,
    denseInterBackgroundMaximumPrincipalAngleDegrees,
    phase379InterBackgroundMinimumSingularValue,
    transportDeltaVsPhase379,
    maxAxisFractionAbsDelta,
    maxShellEigenvalueRelativeDelta,
    maxShellEigenResidual,
    maxBlockHermiticityResidual,
    replayDefinitions = new
    {
        denseSolve = "B = M^{-1/2} D M^{-1/2} via complex Hermitian Jacobi; v = M^{-1/2} w; D v = lambda M_psi v",
        shellSelection = "lowest nonzero |lambda| group with tolerance max(1e-12, 1e-8 * |lambda_min|) (Phase378 rule)",
        coordinateBlock = "G_i = V_shell^dagger deltaK[e_i] V_shell, deltaK from DiracVariationComputer.ComputeAnalytical on unit carrier basis vectors",
        responseGram = "Q_ij = Re Tr(G_i^dagger G_j) via 32-dimensional real feature vectors",
        positiveRank = "eigenvalues of the 32x32 dual feature Gram above max(1e-14, 1e-10 * max|eigenvalue|) (Phase378 rule)",
        axisFractions = "sum of squared top-rank Q-eigenvector components grouped by coordinate % 3, normalized by rank (Phase379 rule)",
        transport = "singular values of the 3x3 overlap between background positive eigenspaces (Phase379 rule)",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    expectedCoveragePresent,
    backgroundCount,
    carrierDimension = CarrierDimension,
    expectedShellSize = ExpectedShellSize,
    eigenResidualTolerance = EigenResidualTolerance,
    blockHermiticityTolerance = BlockHermiticityTolerance,
    shellEigenvalueComparisonTolerance = ShellEigenvalueComparisonTolerance,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesCanonicalGaugeAxisSelector,
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
        "study-defined discrete shell-response diagnostic only",
        "not a physical effective-action Hessian",
        "not an observed electroweak namespace map",
        "not a canonical gauge-axis selector",
        "no W/Z bridge law",
        "no Higgs scalar operator",
        "no weak-angle or coupling lineage",
        "no VEV or source-scale lineage",
        "no pole extraction or GeV normalization",
        "no physical predictions",
        "no Phase201 or Phase256 fill",
    },
    backgrounds = backgroundRecords.Select(record => record.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase378FullPath = Phase378FullPath,
        phase378SummaryPath = Phase378SummaryPath,
        phase379SummaryPath = Phase379SummaryPath,
        phase390SummaryPath = Phase390SummaryPath,
        phase12Root = Phase12Root,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "dense_converged_shell_response_replay_audit.json");
string summaryPath = Path.Combine(outputDir, "dense_converged_shell_response_replay_audit_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    summaryPath,
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.generatedAt,
        terminalStatus,
        denseConvergedShellResponseReplayAuditPassed,
        phase378PrecursorPassed,
        phase379PrecursorPassed,
        phase390PrecursorPassed,
        denseReplayInternallyConsistent,
        result.denseReplayVerdict,
        denseReplayConfirmsRankThree,
        denseReplayConfirmsSuppressedAxis,
        shellEigenvaluesMatchPhase378,
        denseSolveConverged,
        blockHermiticityPassed,
        denseStrictTransportPassed,
        denseInterBackgroundMinimumSingularValue,
        denseInterBackgroundMaximumPrincipalAngleDegrees,
        phase379InterBackgroundMinimumSingularValue,
        transportDeltaVsPhase379,
        maxAxisFractionAbsDelta,
        maxShellEigenvalueRelativeDelta,
        maxShellEigenResidual,
        maxBlockHermiticityResidual,
        result.replayDefinitions,
        result.applicationSubjectKind,
        result.targetBlindConstruction,
        physicalTargetsConsultedForConstruction,
        targetBlindConstructionHash,
        expectedCoveragePresent,
        backgroundCount,
        routeProvidesPhysicalMassPsiCompatibleBranch,
        routeProvidesPhysicalEffectiveActionHessian,
        routeProvidesObservedElectroweakNamespaceMap,
        routeProvidesCanonicalGaugeAxisSelector,
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
        backgrounds = backgroundRecords.Select(record => record.ToOutput()).ToArray(),
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"denseConvergedShellResponseReplayAuditPassed={denseConvergedShellResponseReplayAuditPassed}");
Console.WriteLine($"denseReplayVerdict={verdict}");
Console.WriteLine($"denseReplayConfirmsRankThree={denseReplayConfirmsRankThree}");
Console.WriteLine($"denseReplayConfirmsSuppressedAxis={denseReplayConfirmsSuppressedAxis}");
Console.WriteLine($"shellEigenvaluesMatchPhase378={shellEigenvaluesMatchPhase378}");
Console.WriteLine($"denseStrictTransportPassed={denseStrictTransportPassed}");
Console.WriteLine($"denseInterBackgroundMinimumSingularValue={denseInterBackgroundMinimumSingularValue:R}");
Console.WriteLine($"transportDeltaVsPhase379={transportDeltaVsPhase379:R}");
Console.WriteLine($"maxAxisFractionAbsDelta={maxAxisFractionAbsDelta:R}");
Console.WriteLine($"maxShellEigenvalueRelativeDelta={maxShellEigenvalueRelativeDelta:R}");
Console.WriteLine($"maxShellEigenResidual={maxShellEigenResidual:R}");
Console.WriteLine($"maxBlockHermiticityResidual={maxBlockHermiticityResidual:R}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Background replay implementation.
// ---------------------------------------------------------------------------

BackgroundReplayRecord ReplayBackground(string backgroundId)
{
    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    string matrixPath = Path.Combine(FermionDir, matrixRef);
    var (dRe, dIm) = LoadFlatInterleavedMatrix(matrixPath, totalDof);

    // Dense generalized eigensolve via B = M^{-1/2} D M^{-1/2}.
    var invSqrtM = new double[totalDof];
    for (int index = 0; index < totalDof; index++)
        invSqrtM[index] = 1.0 / Math.Sqrt(meshVolumeMassPsiWeights[2 * index]);
    var bRe = new double[totalDof, totalDof];
    var bIm = new double[totalDof, totalDof];
    for (int row = 0; row < totalDof; row++)
        for (int col = 0; col < totalDof; col++)
        {
            double scale = invSqrtM[row] * invSqrtM[col];
            bRe[row, col] = scale * dRe[row, col];
            bIm[row, col] = scale * dIm[row, col];
        }
    var (eigenvalues, vecRe, vecIm, sweeps, offDiagonal) = JacobiHermitian(bRe, bIm);

    // Shell selection with the Phase378 grouping rule.
    double maxAbsEigenvalue = eigenvalues.Max(Math.Abs);
    double kernelThreshold = 1e-10 * Math.Max(maxAbsEigenvalue, 1e-30);
    var nonzero = Enumerable.Range(0, totalDof)
        .Where(k => Math.Abs(eigenvalues[k]) > kernelThreshold)
        .OrderBy(k => Math.Abs(eigenvalues[k]))
        .ToArray();
    double lambdaMinMagnitude = Math.Abs(eigenvalues[nonzero[0]]);
    double groupingTolerance = Math.Max(1e-12, 1e-8 * lambdaMinMagnitude);
    var shellIndices = nonzero
        .Where(k => Math.Abs(Math.Abs(eigenvalues[k]) - lambdaMinMagnitude) <= groupingTolerance)
        .ToArray();

    // Reconstruct M-orthonormal generalized shell modes v = M^{-1/2} w.
    var shellModes = shellIndices.Select(k =>
    {
        var mode = new double[2 * totalDof];
        for (int index = 0; index < totalDof; index++)
        {
            mode[2 * index] = invSqrtM[index] * vecRe[index, k];
            mode[2 * index + 1] = invSqrtM[index] * vecIm[index, k];
        }
        return mode;
    }).ToArray();
    var shellEigenvalues = shellIndices.Select(k => eigenvalues[k]).ToArray();

    double maxShellResidual = 0.0;
    for (int s = 0; s < shellModes.Length; s++)
    {
        var dMode = ApplyComplexMatrix(dRe, dIm, shellModes[s]);
        double residual2 = 0.0;
        double norm2 = 0.0;
        for (int index = 0; index < totalDof; index++)
        {
            double weight = meshVolumeMassPsiWeights[2 * index];
            double rRe = dMode[2 * index] - shellEigenvalues[s] * weight * shellModes[s][2 * index];
            double rIm = dMode[2 * index + 1] - shellEigenvalues[s] * weight * shellModes[s][2 * index + 1];
            residual2 += rRe * rRe + rIm * rIm;
            norm2 += shellModes[s][2 * index] * shellModes[s][2 * index] + shellModes[s][2 * index + 1] * shellModes[s][2 * index + 1];
        }
        maxShellResidual = Math.Max(maxShellResidual, Math.Sqrt(residual2) / Math.Max(Math.Sqrt(norm2), 1e-30));
    }

    // Compare shell eigenvalues with the persisted Phase378 weighted-solver shell.
    double shellEigenvalueMaxRelativeDelta = double.PositiveInfinity;
    if (phase378Backgrounds.TryGetValue(backgroundId, out var phase378ShellEigenvalues) &&
        phase378ShellEigenvalues.Length == shellEigenvalues.Length)
    {
        var denseSorted = shellEigenvalues.OrderBy(value => value).ToArray();
        var persistedSorted = phase378ShellEigenvalues.OrderBy(value => value).ToArray();
        shellEigenvalueMaxRelativeDelta = denseSorted
            .Zip(persistedSorted, (dense, persisted) =>
                Math.Abs(dense - persisted) / Math.Max(Math.Abs(persisted), 1e-30))
            .Max();
    }

    // Per-coordinate blocks and 32-dimensional features (Phase378 pipeline).
    int shellSize = shellModes.Length;
    int featureLength = 2 * shellSize * shellSize;
    var features = new double[CarrierDimension][];
    double maxBlockHermiticity = 0.0;
    for (int coordinate = 0; coordinate < CarrierDimension; coordinate++)
    {
        var basis = new double[CarrierDimension];
        basis[coordinate] = 1.0;
        var (deltaRe, deltaIm) = DiracVariationComputer.ComputeAnalytical(
            basis, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);

        var feature = new double[featureLength];
        var block = new (double Re, double Im)[shellSize, shellSize];
        for (int b = 0; b < shellSize; b++)
        {
            var deltaPsi = ApplyComplexMatrix(deltaRe, deltaIm, shellModes[b]);
            for (int a = 0; a < shellSize; a++)
            {
                var inner = ComplexInnerProduct(shellModes[a], deltaPsi);
                block[a, b] = (inner.Real, inner.Imaginary);
                int featureIndex = 2 * (a * shellSize + b);
                feature[featureIndex] = inner.Real;
                feature[featureIndex + 1] = inner.Imaginary;
            }
        }
        for (int a = 0; a < shellSize; a++)
            for (int b = 0; b < shellSize; b++)
            {
                double dReH = block[a, b].Re - block[b, a].Re;
                double dImH = block[a, b].Im + block[b, a].Im;
                maxBlockHermiticity = Math.Max(maxBlockHermiticity, Math.Sqrt(dReH * dReH + dImH * dImH));
            }
        features[coordinate] = feature;
    }

    // Dual feature Gram (featureLength x featureLength) and positive rank.
    var dualGram = new double[featureLength, featureLength];
    for (int coordinate = 0; coordinate < CarrierDimension; coordinate++)
        for (int a = 0; a < featureLength; a++)
            for (int b = 0; b < featureLength; b++)
                dualGram[a, b] += features[coordinate][a] * features[coordinate][b];
    var (dualEigenvalues, _, _, _, _) = JacobiHermitian(dualGram, new double[featureLength, featureLength]);
    double dualMax = dualEigenvalues.Max(Math.Abs);
    double rankTolerance = Math.Max(1e-14, 1e-10 * dualMax);
    int positiveRank = dualEigenvalues.Count(value => value > rankTolerance);

    // Full response Gram Q (156x156) and its top-rank eigenvectors.
    var q = new double[CarrierDimension, CarrierDimension];
    for (int i = 0; i < CarrierDimension; i++)
        for (int j = 0; j < CarrierDimension; j++)
        {
            double sum = 0.0;
            for (int a = 0; a < featureLength; a++)
                sum += features[i][a] * features[j][a];
            q[i, j] = sum;
        }
    var (qEigenvalues, qVecRe, _, _, _) = JacobiHermitian(q, new double[CarrierDimension, CarrierDimension]);
    var topIndices = Enumerable.Range(0, CarrierDimension)
        .Where(k => qEigenvalues[k] > rankTolerance)
        .OrderByDescending(k => qEigenvalues[k])
        .Take(Math.Max(positiveRank, 1))
        .ToArray();
    var positiveEigenvectors = topIndices.Select(k =>
    {
        var vector = new double[CarrierDimension];
        double norm = 0.0;
        for (int c = 0; c < CarrierDimension; c++)
        {
            vector[c] = qVecRe[c, k];
            norm += vector[c] * vector[c];
        }
        norm = Math.Sqrt(norm);
        for (int c = 0; c < CarrierDimension; c++)
            vector[c] /= norm;
        return vector;
    }).ToArray();

    // Gauge-axis projector fractions (Phase379 rule).
    var axisCapture = new double[DimG];
    foreach (var vector in positiveEigenvectors)
        for (int c = 0; c < CarrierDimension; c++)
            axisCapture[c % DimG] += vector[c] * vector[c];
    var axisFractions = axisCapture.Select(value => value / Math.Max(positiveRank, 1)).ToArray();
    int suppressedAxis = Array.IndexOf(axisFractions, axisFractions.Min());

    var phase379Target = phase379Backgrounds[backgroundId];
    double axisDelta = axisFractions
        .Zip(phase379Target.GaugeAxisProjectorFractions, (dense, persisted) => Math.Abs(dense - persisted))
        .Max();

    return new BackgroundReplayRecord
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMatrixPath = matrixPath,
        JacobiSweeps = sweeps,
        FinalOffDiagonalNorm = offDiagonal,
        ShellSize = shellSize,
        ShellEigenvalues = shellEigenvalues,
        MaxShellEigenResidual = maxShellResidual,
        ShellEigenvalueMaxRelativeDelta = shellEigenvalueMaxRelativeDelta,
        CoordinateCount = CarrierDimension,
        MaxBlockHermiticityResidual = maxBlockHermiticity,
        PositiveRank = positiveRank,
        RankTolerance = rankTolerance,
        PositiveDualEigenvalues = dualEigenvalues.Where(value => value > rankTolerance).OrderByDescending(value => value).ToArray(),
        GaugeAxisProjectorFractions = axisFractions,
        SuppressedAxisIndex = suppressedAxis,
        Phase379GaugeAxisProjectorFractions = phase379Target.GaugeAxisProjectorFractions,
        Phase379SuppressedAxisIndex = phase379Target.SuppressedGaugeAxisIndex,
        AxisFractionMaxAbsDeltaVsPhase379 = axisDelta,
        RankMatchesPhase379 = positiveRank == phase379Target.PositiveResponseRank,
        SuppressedAxisMatchesPhase379 = suppressedAxis == phase379Target.SuppressedGaugeAxisIndex,
        PositiveEigenvectors = positiveEigenvectors,
    };
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

                double phaseRe = gRe / gAbs;
                double phaseIm = gIm / gAbs;
                double alpha = aRe[p, p];
                double beta = aRe[q, q];
                double theta = 0.5 * Math.Atan2(2.0 * gAbs, alpha - beta);
                double c = Math.Cos(theta);
                double s = Math.Sin(theta);

                double upqRe = -s;
                double uqpRe = s * phaseRe;
                double uqpIm = -s * phaseIm;
                double uqqRe = c * phaseRe;
                double uqqIm = -c * phaseIm;

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

public sealed class Phase379BackgroundTarget
{
    public required int PositiveResponseRank { get; init; }
    public required int SuppressedGaugeAxisIndex { get; init; }
    public required double[] GaugeAxisProjectorFractions { get; init; }
}

public sealed class BackgroundReplayRecord
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required int JacobiSweeps { get; init; }
    public required double FinalOffDiagonalNorm { get; init; }
    public required int ShellSize { get; init; }
    public required double[] ShellEigenvalues { get; init; }
    public required double MaxShellEigenResidual { get; init; }
    public required double ShellEigenvalueMaxRelativeDelta { get; init; }
    public required int CoordinateCount { get; init; }
    public required double MaxBlockHermiticityResidual { get; init; }
    public required int PositiveRank { get; init; }
    public required double RankTolerance { get; init; }
    public required double[] PositiveDualEigenvalues { get; init; }
    public required double[] GaugeAxisProjectorFractions { get; init; }
    public required int SuppressedAxisIndex { get; init; }
    public required double[] Phase379GaugeAxisProjectorFractions { get; init; }
    public required int Phase379SuppressedAxisIndex { get; init; }
    public required double AxisFractionMaxAbsDeltaVsPhase379 { get; init; }
    public required bool RankMatchesPhase379 { get; init; }
    public required bool SuppressedAxisMatchesPhase379 { get; init; }
    public required double[][] PositiveEigenvectors { get; init; }

    public object ToOutput() => new
    {
        fermionBackgroundId = FermionBackgroundId,
        baseDiracMatrixPath = BaseDiracMatrixPath,
        jacobiSweeps = JacobiSweeps,
        finalOffDiagonalNorm = FinalOffDiagonalNorm,
        shellSize = ShellSize,
        shellEigenvalues = ShellEigenvalues,
        maxShellEigenResidual = MaxShellEigenResidual,
        shellEigenvalueMaxRelativeDelta = ShellEigenvalueMaxRelativeDelta,
        coordinateCount = CoordinateCount,
        maxBlockHermiticityResidual = MaxBlockHermiticityResidual,
        positiveRank = PositiveRank,
        rankTolerance = RankTolerance,
        positiveDualEigenvalues = PositiveDualEigenvalues,
        gaugeAxisProjectorFractions = GaugeAxisProjectorFractions,
        suppressedAxisIndex = SuppressedAxisIndex,
        phase379GaugeAxisProjectorFractions = Phase379GaugeAxisProjectorFractions,
        phase379SuppressedAxisIndex = Phase379SuppressedAxisIndex,
        axisFractionMaxAbsDeltaVsPhase379 = AxisFractionMaxAbsDeltaVsPhase379,
        rankMatchesPhase379 = RankMatchesPhase379,
        suppressedAxisMatchesPhase379 = SuppressedAxisMatchesPhase379,
    };
}
