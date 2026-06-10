using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

// Phase392: coupled mixed-Hessian fermion-induced carrier response audit.
//
// After Phase391 confirmed the study-defined shell-response Gram invariants
// (rank 3, suppressed gauge axis 1) are solver-independent, the only
// remaining repo-local route that could change the carrier-image structure is
// an ACTION-DERIVED source operator built from the coupled boson-fermion
// second variation, replacing the Hilbert-Schmidt pullback Gram.
//
// Construction. Around the candidate fermionic action
// S_F(omega, psi) = Re<psi, D(omega) psi> with the M_psi-normalized
// generalized eigenmode background D psi_s = lambda_s M psi_s (Phase390
// converged dense branch), the coupled second variation in (b, chi) is
//
//   delta2 S = Re<chi, (D - lambda_s M) chi> + 2 Re<chi, delta_D[b] psi_s> + (omega-omega terms)
//
// The mixed blocks 2 delta_D[b] psi_s are exactly the VO-7 candidate mixed
// linearization blocks (Phase371/372) evaluated on converged modes.
// Eliminating the fermion fluctuation chi at fixed b (Schur complement /
// degenerate second-order perturbation theory) yields the fermion-induced
// carrier response operator
//
//   R^(s)_kl = Re< delta_D[e_k] psi_s, (D - lambda_s M)^+ delta_D[e_l] psi_s >
//
// with the pseudo-inverse taken on the M-orthogonal complement of the
// lambda_s shell group, computed exactly in the dense generalized eigenbasis:
// (D - lambda M)^+ = M^{-1/2} (B - lambda)^+ M^{-1/2},
// (B - lambda)^+ = sum_{j retained} w_j w_j^dagger / (lambda_j - lambda).
// The audit aggregates R = sum_s R^(s) over the 4-mode lowest nonzero shell
// and characterizes its rank, signature, and gauge-axis structure with the
// same rules as Phase378/379, then compares against the study-defined Gram
// invariants.
//
// Honest scope limits, declared fail-closed:
// - The background omega was solved by the bosonic objective alone; (omega,
//   psi_s) is NOT a coupled critical point, so this is a fixed-background
//   second-order response, not a self-consistent coupled Hessian.
// - S_F is the candidate bilinear, not a completed GU fermionic action; the
//   omega-omega bosonic Hessian block is not assembled here.
// - The branch is the toy control branch. No observed namespace map, W/Z/H
//   source rows, or Phase201/Phase256 contract fields are provided.

const string DefaultOutputDir = "studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string BackgroundStateDir = $"{Phase12Root}/background_states";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase379SummaryPath = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const string Phase390SummaryPath = "studies/phase390_converged_control_branch_fermion_mode_rebuild_001/output/converged_control_branch_fermion_mode_rebuild_summary.json";
const string Phase391SummaryPath = "studies/phase391_dense_converged_shell_response_replay_audit_001/output/dense_converged_shell_response_replay_audit_summary.json";

const int ExpectedBackgroundCount = 2;
const int CarrierDimension = 156;
const int DimG = 3;
const int ExpectedShellSize = 4;
const double JacobiOffDiagonalTolerance = 1e-13;
const int JacobiMaxSweeps = 100;
const double EigenResidualTolerance = 1e-9;
const double SymmetryTolerance = 1e-10;

var outputDir = Environment.GetEnvironmentVariable("PHASE392_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors and persisted comparison targets.
// ---------------------------------------------------------------------------

using var phase379Summary = JsonDocument.Parse(File.ReadAllText(Phase379SummaryPath));
bool phase379PrecursorPassed =
    JsonBool(phase379Summary.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true &&
    JsonInt(phase379Summary.RootElement, "stableSuppressedGaugeAxis") == 1;
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

using var phase390Doc = JsonDocument.Parse(File.ReadAllText(Phase390SummaryPath));
bool phase390PrecursorPassed =
    JsonBool(phase390Doc.RootElement, "convergedControlBranchFermionModeRebuildPassed") is true &&
    JsonBool(phase390Doc.RootElement, "mPsiCompatibleGeneralizedControlBranchMaterialized") is true;

using var phase391Doc = JsonDocument.Parse(File.ReadAllText(Phase391SummaryPath));
bool phase391PrecursorPassed =
    JsonBool(phase391Doc.RootElement, "denseConvergedShellResponseReplayAuditPassed") is true &&
    JsonString(phase391Doc.RootElement, "denseReplayVerdict") == "confirmed" &&
    JsonBool(phase391Doc.RootElement, "denseReplayConfirmsSuppressedAxis") is true;

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
        CodeRevision = "phase392-coupled-mixed-hessian-fermion-induced-response-audit",
        Branch = new() { BranchId = "phase392-coupled-mixed-hessian-fermion-induced-response-audit", SchemaVersion = "1.0" },
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

double[] meshVolumeMassPsiWeights = MassPsiWeightsBuilder.BuildFromMesh(mesh, dofsPerCell);

// ---------------------------------------------------------------------------
// Per-background construction.
// ---------------------------------------------------------------------------

var backgroundIds = Directory.GetFiles(FermionDir, "dirac_bundle_*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .Select(path => Path.GetFileNameWithoutExtension(path)["dirac_bundle_".Length..])
    .OrderBy(id => id, StringComparer.Ordinal)
    .ToArray();

var backgroundRecords = backgroundIds.Select(BuildBackground).ToArray();

int backgroundCount = backgroundRecords.Length;
bool expectedCoveragePresent =
    backgroundCount == ExpectedBackgroundCount &&
    backgroundRecords.All(record => record.ShellSize == ExpectedShellSize);
bool denseSolveConverged = backgroundRecords.All(record => record.MaxShellEigenResidual <= EigenResidualTolerance);
bool responseSymmetryPassed = backgroundRecords.All(record => record.ResponseAsymmetryResidual <= SymmetryTolerance);
bool mixedBlocksMaterializedOnConvergedModes = backgroundRecords.All(record => record.MixedBlockCount == CarrierDimension * ExpectedShellSize);
bool actionDerivedResponseSharesRankThree = backgroundRecords.All(record => record.SignificantRank == 3 && record.RankMatchesPhase379);
bool actionDerivedResponseSharesSuppressedAxis = backgroundRecords.All(record => record.SuppressedAxisIndex == 1 && record.SuppressedAxisMatchesPhase379);
bool stableActionDerivedSuppressedAxis =
    backgroundRecords.Select(record => record.SuppressedAxisIndex).Distinct().Count() == 1;
int stableSuppressedAxisIndex = stableActionDerivedSuppressedAxis ? backgroundRecords[0].SuppressedAxisIndex : -1;
double maxAxisFractionAbsDeltaVsPhase379 = backgroundRecords.Max(record => record.AxisFractionMaxAbsDeltaVsPhase379);
double maxShellEigenResidual = backgroundRecords.Max(record => record.MaxShellEigenResidual);
double maxResponseAsymmetryResidual = backgroundRecords.Max(record => record.ResponseAsymmetryResidual);
double minRetainedDenominator = backgroundRecords.Min(record => record.MinRetainedDenominatorMagnitude);
double maxPureGaugeToGenericResponseRatio = backgroundRecords.Max(record => record.PureGaugeToGenericResponseRatio);

bool constructionInternallyConsistent =
    expectedCoveragePresent &&
    denseSolveConverged &&
    responseSymmetryPassed &&
    mixedBlocksMaterializedOnConvergedModes;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool backgroundIsCoupledCriticalPoint = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesCompletedMixedLinearizationBlocks = false;
const bool routeProvidesMixedLinearizationGaugeCompatibilityIdentities = false;
const bool routeProvidesCoupledResidual = false;
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
const string ApplicationSubjectKind = "coupled-mixed-hessian-fermion-induced-response-audit";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    vertexCount.ToString(),
    edgeCount.ToString(),
    DimG.ToString(),
    spinorDim.ToString(),
    ExpectedShellSize.ToString(),
    "R^(s)_kl = Re<deltaD[e_k] psi_s, (D - lambda_s M)^+ deltaD[e_l] psi_s>",
    "aggregate over lowest nonzero shell; axis = coordinate % 3",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool coupledMixedHessianFermionInducedResponseAuditPassed =
    phase379PrecursorPassed &&
    phase390PrecursorPassed &&
    phase391PrecursorPassed &&
    constructionInternallyConsistent &&
    !backgroundIsCoupledCriticalPoint &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesCompletedMixedLinearizationBlocks &&
    !routeProvidesMixedLinearizationGaugeCompatibilityIdentities &&
    !routeProvidesCoupledResidual &&
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

string structureVerdict = actionDerivedResponseSharesRankThree && actionDerivedResponseSharesSuppressedAxis
    ? "shares-gram-structure"
    : "diverges-from-gram-structure";
string terminalStatus = coupledMixedHessianFermionInducedResponseAuditPassed
    ? $"action-derived-fermion-induced-response-{structureVerdict}-discrete-diagnostic-only"
    : "coupled-mixed-hessian-fermion-induced-response-audit-blocked";
string decision = !coupledMixedHessianFermionInducedResponseAuditPassed
    ? "Do not use the action-derived response operator until the Phase379/390/391 precursors, dense solve convergence, response symmetry, and mixed-block coverage all pass."
    : structureVerdict == "shares-gram-structure"
        ? "The action-derived fermion-induced carrier response operator R (Schur complement of the coupled candidate mixed Hessian over the converged lowest nonzero shell) reproduces the study-defined Gram structure: significant rank 3 and the same suppressed gauge axis on both backgrounds. The suppressed-axis obstruction is therefore not an artifact of the Hilbert-Schmidt pullback metric either - it persists in the second-order perturbation response of the candidate action. Every repo-local response route now exhibits the suppressed axis, so the remaining theorem requirements (observed namespace map, suppressed-axis W-row theorem, W/Z/H source package) cannot be discharged by another internal response construction. This remains a fixed-background, candidate-action, toy-branch diagnostic: no coupled critical point, no physical Hessian, no contract fields."
        : "The action-derived fermion-induced carrier response operator R does NOT share the study-defined Gram structure; the recorded spectra, signatures, and axis fractions quantify the divergence. This reopens the carrier-image question: the suppressed-axis obstruction is metric-dependent, and the Phase381/383/384 blockers should be re-examined against the action-derived response before any further suppressed-axis theorem work.";

var result = new
{
    phaseId = "phase392-coupled-mixed-hessian-fermion-induced-response-audit",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    coupledMixedHessianFermionInducedResponseAuditPassed,
    phase379PrecursorPassed,
    phase390PrecursorPassed,
    phase391PrecursorPassed,
    constructionInternallyConsistent,
    actionDerivedResponseStructureVerdict = structureVerdict,
    actionDerivedResponseSharesRankThree,
    actionDerivedResponseSharesSuppressedAxis,
    stableActionDerivedSuppressedAxis,
    stableSuppressedAxisIndex,
    denseSolveConverged,
    responseSymmetryPassed,
    mixedBlocksMaterializedOnConvergedModes,
    backgroundIsCoupledCriticalPoint,
    maxAxisFractionAbsDeltaVsPhase379,
    maxShellEigenResidual,
    maxResponseAsymmetryResidual,
    minRetainedDenominator,
    maxPureGaugeToGenericResponseRatio,
    constructionDefinitions = new
    {
        candidateAction = "S_F(omega, psi) = Re<psi, D(omega) psi> with M_psi-normalized generalized eigenmode background",
        mixedHessianBlock = "2 delta_D[e_k] psi_s (VO-7 candidate mixed linearization block on converged modes)",
        fermionFluctuationOperator = "D - lambda_s M_psi on the M-orthogonal complement of the lambda_s shell group",
        responseOperator = "R^(s)_kl = Re<delta_D[e_k] psi_s, (D - lambda_s M)^+ delta_D[e_l] psi_s>; R = sum over the 4-mode lowest nonzero shell",
        pseudoInverse = "(D - lambda M)^+ = M^{-1/2} (B - lambda)^+ M^{-1/2} in the exact dense generalized eigenbasis, excluding the lambda_s shell group",
        rankRule = "eigenvalues of R with |eig| > max(1e-14, 1e-10 * max|eig|) (signature recorded separately)",
        axisRule = "squared components of significant eigenvectors grouped by coordinate % 3, normalized by significant rank (Phase379 rule)",
        pureGaugeDiagnostic = "v(X)^T R v(X) for all vertex-local and global su(2) gauge directions v(X) = DeltaX + [omega, Xbar] (Phase389 covariant differential), compared to the generic carrier response scale",
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
    symmetryTolerance = SymmetryTolerance,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesCompletedMixedLinearizationBlocks,
    routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
    routeProvidesCoupledResidual,
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
        "fixed-background second-order response, not a self-consistent coupled Hessian",
        "candidate bilinear action only, not a completed GU fermionic action",
        "omega-omega bosonic Hessian block not assembled",
        "toy control branch only",
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
        phase379SummaryPath = Phase379SummaryPath,
        phase390SummaryPath = Phase390SummaryPath,
        phase391SummaryPath = Phase391SummaryPath,
        phase12Root = Phase12Root,
        backgroundStateDir = BackgroundStateDir,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "coupled_mixed_hessian_fermion_induced_response_audit.json");
string summaryPath = Path.Combine(outputDir, "coupled_mixed_hessian_fermion_induced_response_audit_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    summaryPath,
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.generatedAt,
        terminalStatus,
        coupledMixedHessianFermionInducedResponseAuditPassed,
        phase379PrecursorPassed,
        phase390PrecursorPassed,
        phase391PrecursorPassed,
        constructionInternallyConsistent,
        result.actionDerivedResponseStructureVerdict,
        actionDerivedResponseSharesRankThree,
        actionDerivedResponseSharesSuppressedAxis,
        stableActionDerivedSuppressedAxis,
        stableSuppressedAxisIndex,
        denseSolveConverged,
        responseSymmetryPassed,
        mixedBlocksMaterializedOnConvergedModes,
        backgroundIsCoupledCriticalPoint,
        maxAxisFractionAbsDeltaVsPhase379,
        maxShellEigenResidual,
        maxResponseAsymmetryResidual,
        minRetainedDenominator,
        maxPureGaugeToGenericResponseRatio,
        result.constructionDefinitions,
        result.applicationSubjectKind,
        result.targetBlindConstruction,
        physicalTargetsConsultedForConstruction,
        targetBlindConstructionHash,
        expectedCoveragePresent,
        backgroundCount,
        routeProvidesPhysicalMassPsiCompatibleBranch,
        routeProvidesCompletedFermionicAction,
        routeProvidesCompletedMixedLinearizationBlocks,
        routeProvidesMixedLinearizationGaugeCompatibilityIdentities,
        routeProvidesCoupledResidual,
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
Console.WriteLine($"coupledMixedHessianFermionInducedResponseAuditPassed={coupledMixedHessianFermionInducedResponseAuditPassed}");
Console.WriteLine($"actionDerivedResponseStructureVerdict={structureVerdict}");
Console.WriteLine($"actionDerivedResponseSharesRankThree={actionDerivedResponseSharesRankThree}");
Console.WriteLine($"actionDerivedResponseSharesSuppressedAxis={actionDerivedResponseSharesSuppressedAxis}");
Console.WriteLine($"stableSuppressedAxisIndex={stableSuppressedAxisIndex}");
Console.WriteLine($"maxAxisFractionAbsDeltaVsPhase379={maxAxisFractionAbsDeltaVsPhase379:R}");
Console.WriteLine($"maxShellEigenResidual={maxShellEigenResidual:R}");
Console.WriteLine($"maxResponseAsymmetryResidual={maxResponseAsymmetryResidual:R}");
Console.WriteLine($"minRetainedDenominator={minRetainedDenominator:R}");
Console.WriteLine($"maxPureGaugeToGenericResponseRatio={maxPureGaugeToGenericResponseRatio:R}");
Console.WriteLine($"backgroundIsCoupledCriticalPoint={backgroundIsCoupledCriticalPoint}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Background construction implementation.
// ---------------------------------------------------------------------------

BackgroundResponseRecord BuildBackground(string backgroundId)
{
    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    string matrixPath = Path.Combine(FermionDir, matrixRef);
    var (dRe, dIm) = LoadFlatInterleavedMatrix(matrixPath, totalDof);

    string omegaPath = Path.Combine(BackgroundStateDir, $"{backgroundId}_omega.json");
    using var omegaDoc = JsonDocument.Parse(File.ReadAllText(omegaPath));
    double[] omega = omegaDoc.RootElement.GetProperty("coefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray();

    // Dense generalized eigensolve via B = M^{-1/2} D M^{-1/2}.
    var invSqrtM = new double[totalDof];
    var sqrtM = new double[totalDof];
    for (int index = 0; index < totalDof; index++)
    {
        sqrtM[index] = Math.Sqrt(meshVolumeMassPsiWeights[2 * index]);
        invSqrtM[index] = 1.0 / sqrtM[index];
    }
    var bRe = new double[totalDof, totalDof];
    var bIm = new double[totalDof, totalDof];
    for (int row = 0; row < totalDof; row++)
        for (int col = 0; col < totalDof; col++)
        {
            double scale = invSqrtM[row] * invSqrtM[col];
            bRe[row, col] = scale * dRe[row, col];
            bIm[row, col] = scale * dIm[row, col];
        }
    var (eigenvalues, vecRe, vecIm, sweeps, _) = JacobiHermitian(bRe, bIm);

    // Shell selection (Phase378 grouping rule).
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
    int shellSize = shellIndices.Length;

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
    for (int s = 0; s < shellSize; s++)
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

    // Mixed blocks y_{k,s} = deltaD[e_k] psi_s and their eigenbasis
    // coefficients c_{k,s} = W^dagger M^{-1/2} y_{k,s}.
    var coefficients = new double[CarrierDimension][][];
    int mixedBlockCount = 0;
    for (int coordinate = 0; coordinate < CarrierDimension; coordinate++)
    {
        var basis = new double[CarrierDimension];
        basis[coordinate] = 1.0;
        var (deltaRe, deltaIm) = DiracVariationComputer.ComputeAnalytical(
            basis, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
        coefficients[coordinate] = new double[shellSize][];
        for (int s = 0; s < shellSize; s++)
        {
            var y = ApplyComplexMatrix(deltaRe, deltaIm, shellModes[s]);
            // z = M^{-1/2} y, then c[j] = sum_i conj(W[i,j]) z[i].
            var c = new double[2 * totalDof];
            for (int j = 0; j < totalDof; j++)
            {
                double sumRe = 0.0;
                double sumIm = 0.0;
                for (int i = 0; i < totalDof; i++)
                {
                    double zRe = invSqrtM[i] * y[2 * i];
                    double zIm = invSqrtM[i] * y[2 * i + 1];
                    double wRe = vecRe[i, j];
                    double wIm = vecIm[i, j];
                    sumRe += wRe * zRe + wIm * zIm;
                    sumIm += wRe * zIm - wIm * zRe;
                }
                c[2 * j] = sumRe;
                c[2 * j + 1] = sumIm;
            }
            coefficients[coordinate][s] = c;
            mixedBlockCount++;
        }
    }

    // Fermion-induced response operator R = sum_s R^(s).
    var response = new double[CarrierDimension, CarrierDimension];
    double minRetainedDenominatorMagnitude = double.PositiveInfinity;
    for (int s = 0; s < shellSize; s++)
    {
        double lambdaS = shellEigenvalues[s];
        for (int j = 0; j < totalDof; j++)
        {
            double denominator = eigenvalues[j] - lambdaS;
            if (Math.Abs(Math.Abs(eigenvalues[j]) - lambdaMinMagnitude) <= groupingTolerance)
                continue; // exclude the whole shell group (degenerate subspace)
            if (Math.Abs(denominator) <= groupingTolerance)
                continue;
            minRetainedDenominatorMagnitude = Math.Min(minRetainedDenominatorMagnitude, Math.Abs(denominator));
            double invDenominator = 1.0 / denominator;
            for (int k = 0; k < CarrierDimension; k++)
            {
                double ckRe = coefficients[k][s][2 * j];
                double ckIm = coefficients[k][s][2 * j + 1];
                if (ckRe == 0.0 && ckIm == 0.0)
                    continue;
                for (int l = k; l < CarrierDimension; l++)
                {
                    double clRe = coefficients[l][s][2 * j];
                    double clIm = coefficients[l][s][2 * j + 1];
                    double value = (ckRe * clRe + ckIm * clIm) * invDenominator;
                    response[k, l] += value;
                    if (l != k)
                        response[l, k] += value;
                }
            }
        }
    }

    double responseNorm = 0.0;
    double asymmetry2 = 0.0;
    for (int k = 0; k < CarrierDimension; k++)
        for (int l = 0; l < CarrierDimension; l++)
        {
            responseNorm += response[k, l] * response[k, l];
            double diff = response[k, l] - response[l, k];
            asymmetry2 += diff * diff;
        }
    responseNorm = Math.Sqrt(responseNorm);
    double responseAsymmetryResidual = responseNorm > 0.0 ? Math.Sqrt(asymmetry2) / responseNorm : 0.0;

    // Spectrum, signature, significant rank, and axis structure of R.
    var (rEigenvalues, rVecRe, _, _, _) = JacobiHermitian(response, new double[CarrierDimension, CarrierDimension]);
    double rMax = rEigenvalues.Max(Math.Abs);
    double rankTolerance = Math.Max(1e-14, 1e-10 * rMax);
    int positiveRank = rEigenvalues.Count(value => value > rankTolerance);
    int negativeRank = rEigenvalues.Count(value => value < -rankTolerance);
    int significantRank = positiveRank + negativeRank;
    var significantIndices = Enumerable.Range(0, CarrierDimension)
        .Where(k => Math.Abs(rEigenvalues[k]) > rankTolerance)
        .OrderByDescending(k => Math.Abs(rEigenvalues[k]))
        .ToArray();

    var axisCapture = new double[DimG];
    foreach (int k in significantIndices)
    {
        double norm2 = 0.0;
        for (int c = 0; c < CarrierDimension; c++)
            norm2 += rVecRe[c, k] * rVecRe[c, k];
        for (int c = 0; c < CarrierDimension; c++)
            axisCapture[c % DimG] += rVecRe[c, k] * rVecRe[c, k] / norm2;
    }
    var axisFractions = axisCapture.Select(value => value / Math.Max(significantRank, 1)).ToArray();
    int suppressedAxis = Array.IndexOf(axisFractions, axisFractions.Min());

    var phase379Target = phase379Backgrounds[backgroundId];
    double axisDelta = axisFractions
        .Zip(phase379Target.GaugeAxisProjectorFractions, (current, persisted) => Math.Abs(current - persisted))
        .Max();

    // Pure-gauge response diagnostic: v(X)^T R v(X) for all gauge directions,
    // compared to the mean generic carrier-coordinate response scale.
    double genericScale = 0.0;
    for (int k = 0; k < CarrierDimension; k++)
        genericScale += Math.Abs(response[k, k]);
    genericScale /= CarrierDimension;
    double maxPureGaugeResponse = 0.0;
    int gaugeDirectionCount = 0;
    for (int vertex = -1; vertex < vertexCount; vertex++)
        for (int generator = 0; generator < DimG; generator++)
        {
            var x = new double[vertexCount][];
            for (int v = 0; v < vertexCount; v++)
            {
                x[v] = new double[DimG];
                if (vertex < 0 || v == vertex)
                    x[v][generator] = 1.0;
            }
            var direction = BuildCovariantDifferential(x, omega);
            double value = 0.0;
            for (int k = 0; k < CarrierDimension; k++)
            {
                if (direction[k] == 0.0)
                    continue;
                double row = 0.0;
                for (int l = 0; l < CarrierDimension; l++)
                    row += response[k, l] * direction[l];
                value += direction[k] * row;
            }
            maxPureGaugeResponse = Math.Max(maxPureGaugeResponse, Math.Abs(value));
            gaugeDirectionCount++;
        }
    double pureGaugeRatio = genericScale > 0.0 ? maxPureGaugeResponse / genericScale : double.NaN;

    return new BackgroundResponseRecord
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMatrixPath = matrixPath,
        PersistedOmegaPath = omegaPath,
        JacobiSweeps = sweeps,
        ShellSize = shellSize,
        ShellEigenvalues = shellEigenvalues,
        MaxShellEigenResidual = maxShellResidual,
        MixedBlockCount = mixedBlockCount,
        ResponseFrobeniusNorm = responseNorm,
        ResponseAsymmetryResidual = responseAsymmetryResidual,
        MinRetainedDenominatorMagnitude = minRetainedDenominatorMagnitude,
        PositiveRank = positiveRank,
        NegativeRank = negativeRank,
        SignificantRank = significantRank,
        RankTolerance = rankTolerance,
        SignificantEigenvalues = significantIndices.Select(k => rEigenvalues[k]).ToArray(),
        GaugeAxisProjectorFractions = axisFractions,
        SuppressedAxisIndex = suppressedAxis,
        Phase379GaugeAxisProjectorFractions = phase379Target.GaugeAxisProjectorFractions,
        Phase379SuppressedAxisIndex = phase379Target.SuppressedGaugeAxisIndex,
        AxisFractionMaxAbsDeltaVsPhase379 = axisDelta,
        RankMatchesPhase379 = significantRank == phase379Target.PositiveResponseRank,
        SuppressedAxisMatchesPhase379 = suppressedAxis == phase379Target.SuppressedGaugeAxisIndex,
        GaugeDirectionCount = gaugeDirectionCount,
        GenericResponseScale = genericScale,
        MaxPureGaugeResponseMagnitude = maxPureGaugeResponse,
        PureGaugeToGenericResponseRatio = pureGaugeRatio,
    };
}

double[] BuildCovariantDifferential(double[][] x, double[] omega)
{
    var direction = new double[CarrierDimension];
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
        // bracket = omega_e x Xbar (su(2) adjoint cross product).
        var omegaEdge = new double[DimG];
        for (int a = 0; a < DimG; a++)
            omegaEdge[a] = omega[edge * DimG + a];
        var bracket = new double[DimG];
        bracket[0] = omegaEdge[1] * xBar[2] - omegaEdge[2] * xBar[1];
        bracket[1] = omegaEdge[2] * xBar[0] - omegaEdge[0] * xBar[2];
        bracket[2] = omegaEdge[0] * xBar[1] - omegaEdge[1] * xBar[0];
        for (int a = 0; a < DimG; a++)
            direction[edge * DimG + a] = deltaX[a] + bracket[a];
    }
    return direction;
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

    var outEigenvalues = new double[n];
    for (int i = 0; i < n; i++)
        outEigenvalues[i] = aRe[i, i];
    return (outEigenvalues, vRe, vIm, sweeps, offDiagonal);
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

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName)
        ?? throw new InvalidDataException($"{propertyName} must be a string.");

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

public sealed class BackgroundResponseRecord
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required string PersistedOmegaPath { get; init; }
    public required int JacobiSweeps { get; init; }
    public required int ShellSize { get; init; }
    public required double[] ShellEigenvalues { get; init; }
    public required double MaxShellEigenResidual { get; init; }
    public required int MixedBlockCount { get; init; }
    public required double ResponseFrobeniusNorm { get; init; }
    public required double ResponseAsymmetryResidual { get; init; }
    public required double MinRetainedDenominatorMagnitude { get; init; }
    public required int PositiveRank { get; init; }
    public required int NegativeRank { get; init; }
    public required int SignificantRank { get; init; }
    public required double RankTolerance { get; init; }
    public required double[] SignificantEigenvalues { get; init; }
    public required double[] GaugeAxisProjectorFractions { get; init; }
    public required int SuppressedAxisIndex { get; init; }
    public required double[] Phase379GaugeAxisProjectorFractions { get; init; }
    public required int Phase379SuppressedAxisIndex { get; init; }
    public required double AxisFractionMaxAbsDeltaVsPhase379 { get; init; }
    public required bool RankMatchesPhase379 { get; init; }
    public required bool SuppressedAxisMatchesPhase379 { get; init; }
    public required int GaugeDirectionCount { get; init; }
    public required double GenericResponseScale { get; init; }
    public required double MaxPureGaugeResponseMagnitude { get; init; }
    public required double PureGaugeToGenericResponseRatio { get; init; }

    public object ToOutput() => new
    {
        fermionBackgroundId = FermionBackgroundId,
        baseDiracMatrixPath = BaseDiracMatrixPath,
        persistedOmegaPath = PersistedOmegaPath,
        jacobiSweeps = JacobiSweeps,
        shellSize = ShellSize,
        shellEigenvalues = ShellEigenvalues,
        maxShellEigenResidual = MaxShellEigenResidual,
        mixedBlockCount = MixedBlockCount,
        responseFrobeniusNorm = ResponseFrobeniusNorm,
        responseAsymmetryResidual = ResponseAsymmetryResidual,
        minRetainedDenominatorMagnitude = MinRetainedDenominatorMagnitude,
        positiveRank = PositiveRank,
        negativeRank = NegativeRank,
        significantRank = SignificantRank,
        rankTolerance = RankTolerance,
        significantEigenvalues = SignificantEigenvalues,
        gaugeAxisProjectorFractions = GaugeAxisProjectorFractions,
        suppressedAxisIndex = SuppressedAxisIndex,
        phase379GaugeAxisProjectorFractions = Phase379GaugeAxisProjectorFractions,
        phase379SuppressedAxisIndex = Phase379SuppressedAxisIndex,
        axisFractionMaxAbsDeltaVsPhase379 = AxisFractionMaxAbsDeltaVsPhase379,
        rankMatchesPhase379 = RankMatchesPhase379,
        suppressedAxisMatchesPhase379 = SuppressedAxisMatchesPhase379,
        gaugeDirectionCount = GaugeDirectionCount,
        genericResponseScale = GenericResponseScale,
        maxPureGaugeResponseMagnitude = MaxPureGaugeResponseMagnitude,
        pureGaugeToGenericResponseRatio = PureGaugeToGenericResponseRatio,
    };
}
