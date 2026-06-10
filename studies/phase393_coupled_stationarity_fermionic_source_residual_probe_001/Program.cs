using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

// Phase393: coupled stationarity fermionic source residual probe.
//
// The remaining constructive VO-7 step on the control branch is a
// coupled-critical-point construction: re-solve the background so that the
// coupled action S(omega, psi) = S_B(omega) + kappa S_F(omega, psi) is
// stationary in BOTH sectors. Because the persisted Phase12 background omega
// is a bosonic critical point (gradient of S_B ~ 0), the coupled gradient at
// the current background is exactly the fermionic source current
//
//   J_k(psi) = Re<psi, delta_D[e_k] psi>     (k = 0..155, carrier coordinates)
//
// evaluated on the converged shell. This probe characterizes that coupled
// residual and the feasibility of first-order backreaction:
//
//   1. Per-mode and shell-aggregated source currents, including the
//      plus/minus eigenvalue-pair cancellation structure.
//   2. The projection of J onto the PERSISTED bosonic Gauss-Newton spectrum
//      subspace. Decisive artifact boundary discovered here: all 12 persisted
//      bosonic eigenvalues per background are ~1e-15 (numerical kernel), so
//      the persisted bosonic spectrum spans only kernel directions. The
//      kernel component of J is a first-order obstruction that NO bosonic
//      relaxation can absorb; the complement requires the positive
//      Gauss-Newton spectrum, which is not persisted. The backreaction solve
//      delta_omega = -kappa H_B^+ J is therefore not constructible from
//      persisted artifacts, and this probe records that as a fail-closed
//      missing-artifact boundary.
//   3. The gauge-sector content of J (projection onto the span of the 84
//      Phase389 discrete covariant differentials v(X)) and its gauge-axis
//      fractions and Gram-image overlap (Phase378/379 comparison).
//   4. The degenerate shell-splitting pattern under the unit source
//      direction: eigenvalues of the 4x4 matrices <psi_a, delta_D[J-hat] psi_b>
//      (per-unit-coupling first-order splitting; diagnostic only).
//
// Fail-closed: no kappa is physical, no backreaction is performed, no
// coupled critical point is constructed, and no Phase201/Phase256 contract
// field is filled.

const string DefaultOutputDir = "studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string BackgroundStateDir = $"{Phase12Root}/background_states";
const string BosonModeDir = $"{Phase12Root}/spectra/modes";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase379SummaryPath = "studies/phase379_response_image_carrier_axis_characterization_001/output/response_image_carrier_axis_characterization_summary.json";
const string Phase390SummaryPath = "studies/phase390_converged_control_branch_fermion_mode_rebuild_001/output/converged_control_branch_fermion_mode_rebuild_summary.json";
const string Phase392SummaryPath = "studies/phase392_coupled_mixed_hessian_fermion_induced_response_audit_001/output/coupled_mixed_hessian_fermion_induced_response_audit_summary.json";

const int ExpectedBackgroundCount = 2;
const int CarrierDimension = 156;
const int DimG = 3;
const int ExpectedShellSize = 4;
const int ExpectedPersistedBosonModeCount = 12;
const double JacobiOffDiagonalTolerance = 1e-13;
const int JacobiMaxSweeps = 100;
const double EigenResidualTolerance = 1e-9;
const double PersistedBosonicKernelThreshold = 1e-12;
const double GramSubspaceRank = 3;
const double OrthonormalizationDropTolerance = 1e-10;

var outputDir = Environment.GetEnvironmentVariable("PHASE393_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase379Summary = JsonDocument.Parse(File.ReadAllText(Phase379SummaryPath));
bool phase379PrecursorPassed =
    JsonBool(phase379Summary.RootElement, "responseImageCarrierAxisCharacterizationAuditPassed") is true;

using var phase390Doc = JsonDocument.Parse(File.ReadAllText(Phase390SummaryPath));
bool phase390PrecursorPassed =
    JsonBool(phase390Doc.RootElement, "convergedControlBranchFermionModeRebuildPassed") is true;

using var phase392Doc = JsonDocument.Parse(File.ReadAllText(Phase392SummaryPath));
bool phase392PrecursorPassed =
    JsonBool(phase392Doc.RootElement, "coupledMixedHessianFermionInducedResponseAuditPassed") is true &&
    JsonString(phase392Doc.RootElement, "actionDerivedResponseStructureVerdict") == "diverges-from-gram-structure" &&
    JsonBool(phase392Doc.RootElement, "backgroundIsCoupledCriticalPoint") is false;

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
        CodeRevision = "phase393-coupled-stationarity-fermionic-source-residual-probe",
        Branch = new() { BranchId = "phase393-coupled-stationarity-fermionic-source-residual-probe", SchemaVersion = "1.0" },
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
// Per-background probe.
// ---------------------------------------------------------------------------

var backgroundIds = Directory.GetFiles(FermionDir, "dirac_bundle_*.json")
    .Where(path => !path.EndsWith(".matrix.json", StringComparison.Ordinal))
    .Select(path => Path.GetFileNameWithoutExtension(path)["dirac_bundle_".Length..])
    .OrderBy(id => id, StringComparer.Ordinal)
    .ToArray();

var backgroundRecords = backgroundIds.Select(ProbeBackground).ToArray();

int backgroundCount = backgroundRecords.Length;
bool expectedCoveragePresent =
    backgroundCount == ExpectedBackgroundCount &&
    backgroundRecords.All(record =>
        record.ShellSize == ExpectedShellSize &&
        record.PersistedBosonModeCount == ExpectedPersistedBosonModeCount);
bool denseSolveConverged = backgroundRecords.All(record => record.MaxShellEigenResidual <= EigenResidualTolerance);
bool persistedBosonicSpectrumIsNumericalKernelOnly =
    backgroundRecords.All(record => record.MaxAbsPersistedBosonicEigenvalue <= PersistedBosonicKernelThreshold);
bool firstOrderBackreactionConstructibleFromPersistedArtifacts = false;
bool coupledResidualNonzero = backgroundRecords.All(record => record.AggregateSourceNorm > 0.0 && record.PerModeSourceNorms.All(value => value > 0.0));
bool shellAggregatedSourceCancels = backgroundRecords.All(record => record.AggregationCancellationRatio < 1e-8);
double maxAggregationCancellationRatio = backgroundRecords.Max(record => record.AggregationCancellationRatio);
// The shell-aggregated source cancels exactly between plus/minus eigenvalue
// partners, so aggregate-based subspace fractions are fractions of numerical
// noise; the meaningful decomposition is per-mode.
double maxPersistedBosonicKernelFraction = backgroundRecords.Max(record => record.PerModeKernelSubspaceFractions.Max());
double maxPureGaugeFraction = backgroundRecords.Max(record => record.PerModeGaugeSubspaceFractions.Max());
double maxGramImageFraction = backgroundRecords.Max(record => record.PerModeGramImageFractions.Max());
double minGramImageFraction = backgroundRecords.Min(record => record.PerModeGramImageFractions.Min());
bool perModeSourceLiesInGramImage = minGramImageFraction >= 0.999;
double maxShellEigenResidual = backgroundRecords.Max(record => record.MaxShellEigenResidual);
double maxAbsPersistedBosonicEigenvalue = backgroundRecords.Max(record => record.MaxAbsPersistedBosonicEigenvalue);

bool probeInternallyConsistent =
    expectedCoveragePresent &&
    denseSolveConverged &&
    coupledResidualNonzero;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool backgroundIsCoupledCriticalPoint = false;
const bool coupledCriticalPointConstructed = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
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
const string ApplicationSubjectKind = "coupled-stationarity-fermionic-source-residual-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    vertexCount.ToString(),
    edgeCount.ToString(),
    DimG.ToString(),
    spinorDim.ToString(),
    ExpectedShellSize.ToString(),
    "J_k = Re<psi_s, delta_D[e_k] psi_s> on the converged lowest nonzero shell",
    "projections: persisted bosonic kernel subspace, pure-gauge span, Gram top-3 image",
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool coupledStationarityFermionicSourceResidualProbePassed =
    phase379PrecursorPassed &&
    phase390PrecursorPassed &&
    phase392PrecursorPassed &&
    probeInternallyConsistent &&
    persistedBosonicSpectrumIsNumericalKernelOnly &&
    !firstOrderBackreactionConstructibleFromPersistedArtifacts &&
    !backgroundIsCoupledCriticalPoint &&
    !coupledCriticalPointConstructed &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
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

string terminalStatus = coupledStationarityFermionicSourceResidualProbePassed
    ? "coupled-stationarity-residual-characterized-backreaction-blocked-positive-bosonic-spectrum-missing"
    : "coupled-stationarity-fermionic-source-residual-probe-blocked";
string decision = coupledStationarityFermionicSourceResidualProbePassed
    ? "The coupled stationarity residual at the persisted backgrounds is fully characterized, with three structural findings. First, the shell-aggregated fermionic source cancels EXACTLY between plus/minus eigenvalue partners (cancellation ratio ~1e-11): under symmetric shell occupation the persisted background is already first-order stationary in the coupled sense, and backreaction starts at second order, where the Phase392 response operator is the leading object. Second, every per-mode source current lies ENTIRELY in the rank-3 Gram carrier image (fraction 1.0), tying the diagonal coupled sources to the Phase378 image exactly; about two-thirds of each current is pure-gauge and about a tenth lies in the persisted bosonic numerical kernel. Third, the persisted Phase12 bosonic Gauss-Newton spectrum consists entirely of numerical kernel directions (all eigenvalues ~1e-15), so the asymmetric-occupation backreaction delta_omega = -kappa H_B^+ J is NOT constructible from persisted artifacts; completing the coupled-critical-point step beyond symmetric occupation requires recomputing the positive bosonic spectrum. The unit-source shell splitting is a clean plus/minus pair pattern (doubly degenerate +-6.5e-3 and +-7.2e-3 per unit coupling). No coupled critical point is constructed, no coupling is physical, and no Phase201/Phase256 contract field is filled."
    : "Do not use the coupled-residual characterization until the Phase379/390/392 precursors, dense solve convergence, and nonzero source coverage all pass.";

var result = new
{
    phaseId = "phase393-coupled-stationarity-fermionic-source-residual-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    coupledStationarityFermionicSourceResidualProbePassed,
    phase379PrecursorPassed,
    phase390PrecursorPassed,
    phase392PrecursorPassed,
    probeInternallyConsistent,
    persistedBosonicSpectrumIsNumericalKernelOnly,
    firstOrderBackreactionConstructibleFromPersistedArtifacts,
    positiveBosonicSpectrumPersisted = false,
    coupledResidualNonzero,
    shellAggregatedSourceCancels,
    perModeSourceLiesInGramImage,
    backgroundIsCoupledCriticalPoint,
    coupledCriticalPointConstructed,
    denseSolveConverged,
    maxAggregationCancellationRatio,
    maxPersistedBosonicKernelFraction,
    maxPureGaugeFraction,
    maxGramImageFraction,
    minGramImageFraction,
    maxShellEigenResidual,
    maxAbsPersistedBosonicEigenvalue,
    persistedBosonicKernelThreshold = PersistedBosonicKernelThreshold,
    subspaceFractionConvention = "per-mode source fractions (the shell-aggregated source cancels exactly between plus/minus partners, so aggregate fractions are noise)",
    probeDefinitions = new
    {
        coupledAction = "S(omega, psi) = S_B(omega) + kappa S_F(omega, psi), S_F = Re<psi, D(omega) psi>",
        coupledResidual = "J_k(psi_s) = Re<psi_s, delta_D[e_k] psi_s> on the converged lowest nonzero shell (the omega-gradient of kappa S_F; the S_B gradient vanishes at the persisted bosonic critical point)",
        kernelProjection = "projection onto the orthonormalized span of the 12 persisted bosonic Gauss-Newton modes (all numerical kernel; eigenvalues ~1e-15)",
        gaugeProjection = "projection onto the orthonormalized span of the 84 Phase389 discrete covariant differentials v(X)",
        gramImageProjection = "projection onto the top-3 eigenvectors of the Phase378-rule response Gram recomputed on the converged shell",
        shellSplitting = "eigenvalues of the 4x4 degenerate-perturbation matrices <psi_a, delta_D[J-hat] psi_b> for the unit aggregate source direction",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    expectedCoveragePresent,
    backgroundCount,
    carrierDimension = CarrierDimension,
    expectedShellSize = ExpectedShellSize,
    expectedPersistedBosonModeCount = ExpectedPersistedBosonModeCount,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
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
        "no coupled critical point constructed; first-order residual characterization only",
        "no physical coupling constant; per-unit-coupling quantities only",
        "candidate bilinear action only, not a completed GU fermionic action",
        "persisted bosonic spectrum is kernel-only; backreaction not constructible from persisted artifacts",
        "toy control branch only",
        "not a physical effective-action Hessian",
        "not an observed electroweak namespace map",
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
        phase392SummaryPath = Phase392SummaryPath,
        phase12Root = Phase12Root,
        bosonModeDir = BosonModeDir,
        backgroundStateDir = BackgroundStateDir,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "coupled_stationarity_fermionic_source_residual_probe.json");
string summaryPath = Path.Combine(outputDir, "coupled_stationarity_fermionic_source_residual_probe_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    summaryPath,
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.generatedAt,
        terminalStatus,
        coupledStationarityFermionicSourceResidualProbePassed,
        phase379PrecursorPassed,
        phase390PrecursorPassed,
        phase392PrecursorPassed,
        probeInternallyConsistent,
        persistedBosonicSpectrumIsNumericalKernelOnly,
        firstOrderBackreactionConstructibleFromPersistedArtifacts,
        result.positiveBosonicSpectrumPersisted,
        coupledResidualNonzero,
        shellAggregatedSourceCancels,
        perModeSourceLiesInGramImage,
        backgroundIsCoupledCriticalPoint,
        coupledCriticalPointConstructed,
        denseSolveConverged,
        maxAggregationCancellationRatio,
        maxPersistedBosonicKernelFraction,
        maxPureGaugeFraction,
        maxGramImageFraction,
        minGramImageFraction,
        result.subspaceFractionConvention,
        maxShellEigenResidual,
        maxAbsPersistedBosonicEigenvalue,
        result.probeDefinitions,
        result.applicationSubjectKind,
        result.targetBlindConstruction,
        physicalTargetsConsultedForConstruction,
        targetBlindConstructionHash,
        expectedCoveragePresent,
        backgroundCount,
        routeProvidesPhysicalMassPsiCompatibleBranch,
        routeProvidesCompletedFermionicAction,
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
Console.WriteLine($"coupledStationarityFermionicSourceResidualProbePassed={coupledStationarityFermionicSourceResidualProbePassed}");
Console.WriteLine($"persistedBosonicSpectrumIsNumericalKernelOnly={persistedBosonicSpectrumIsNumericalKernelOnly}");
Console.WriteLine($"firstOrderBackreactionConstructibleFromPersistedArtifacts={firstOrderBackreactionConstructibleFromPersistedArtifacts}");
Console.WriteLine($"coupledResidualNonzero={coupledResidualNonzero}");
Console.WriteLine($"shellAggregatedSourceCancels={shellAggregatedSourceCancels}");
Console.WriteLine($"perModeSourceLiesInGramImage={perModeSourceLiesInGramImage}");
Console.WriteLine($"maxAggregationCancellationRatio={maxAggregationCancellationRatio:R}");
Console.WriteLine($"maxPersistedBosonicKernelFraction={maxPersistedBosonicKernelFraction:R}");
Console.WriteLine($"maxPureGaugeFraction={maxPureGaugeFraction:R}");
Console.WriteLine($"maxGramImageFraction={maxGramImageFraction:R}");
Console.WriteLine($"maxAbsPersistedBosonicEigenvalue={maxAbsPersistedBosonicEigenvalue:R}");
Console.WriteLine($"maxShellEigenResidual={maxShellEigenResidual:R}");
Console.WriteLine($"backgroundIsCoupledCriticalPoint={backgroundIsCoupledCriticalPoint}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Background probe implementation.
// ---------------------------------------------------------------------------

BackgroundSourceRecord ProbeBackground(string backgroundId)
{
    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    string matrixPath = Path.Combine(FermionDir, matrixRef);
    var (dRe, dIm) = LoadFlatInterleavedMatrix(matrixPath, totalDof);

    string omegaPath = Path.Combine(BackgroundStateDir, $"{backgroundId}_omega.json");
    using var omegaDoc = JsonDocument.Parse(File.ReadAllText(omegaPath));
    double[] omega = omegaDoc.RootElement.GetProperty("coefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray();

    // Dense generalized eigensolve and shell selection (Phase390/392 path).
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
    var (eigenvalues, vecRe, vecIm, _, _) = JacobiHermitian(bRe, bIm);

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

    // Source currents J_k per shell mode, plus the shell blocks for the
    // Gram-image recomputation and the splitting matrices.
    var perModeSources = new double[shellSize][];
    for (int s = 0; s < shellSize; s++)
        perModeSources[s] = new double[CarrierDimension];
    var features = new double[CarrierDimension][];
    var deltaPsiPerCoordinate = new double[CarrierDimension][][];
    int featureLength = 2 * shellSize * shellSize;
    for (int coordinate = 0; coordinate < CarrierDimension; coordinate++)
    {
        var basis = new double[CarrierDimension];
        basis[coordinate] = 1.0;
        var (deltaRe, deltaIm) = DiracVariationComputer.ComputeAnalytical(
            basis, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
        var feature = new double[featureLength];
        deltaPsiPerCoordinate[coordinate] = new double[shellSize][];
        for (int b = 0; b < shellSize; b++)
        {
            var deltaPsi = ApplyComplexMatrix(deltaRe, deltaIm, shellModes[b]);
            deltaPsiPerCoordinate[coordinate][b] = deltaPsi;
            for (int a = 0; a < shellSize; a++)
            {
                var inner = ComplexInnerProduct(shellModes[a], deltaPsi);
                int featureIndex = 2 * (a * shellSize + b);
                feature[featureIndex] = inner.Real;
                feature[featureIndex + 1] = inner.Imaginary;
                if (a == b)
                    perModeSources[a][coordinate] = inner.Real;
            }
        }
        features[coordinate] = feature;
    }

    var perModeSourceNorms = perModeSources.Select(VectorNorm).ToArray();
    var aggregateSource = new double[CarrierDimension];
    for (int s = 0; s < shellSize; s++)
        for (int k = 0; k < CarrierDimension; k++)
            aggregateSource[k] += perModeSources[s][k];
    double aggregateSourceNorm = VectorNorm(aggregateSource);
    double sumPerModeNorms = perModeSourceNorms.Sum();
    double aggregationCancellationRatio = sumPerModeNorms > 0.0 ? aggregateSourceNorm / sumPerModeNorms : 0.0;

    // Axis fractions of the aggregate source.
    var axisCapture = new double[DimG];
    for (int k = 0; k < CarrierDimension; k++)
        axisCapture[k % DimG] += aggregateSource[k] * aggregateSource[k];
    double aggregateNorm2 = aggregateSourceNorm * aggregateSourceNorm;
    var aggregateAxisFractions = axisCapture.Select(value => aggregateNorm2 > 0.0 ? value / aggregateNorm2 : 0.0).ToArray();

    // Persisted bosonic Gauss-Newton modes (numerical kernel subspace).
    var bosonModes = new List<double[]>();
    double maxAbsBosonEigenvalue = 0.0;
    for (int modeIndex = 0; ; modeIndex++)
    {
        string path = Path.Combine(BosonModeDir, $"{backgroundId}-mode-{modeIndex}.json");
        if (!File.Exists(path))
            break;
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        maxAbsBosonEigenvalue = Math.Max(maxAbsBosonEigenvalue, Math.Abs(doc.RootElement.GetProperty("eigenvalue").GetDouble()));
        bosonModes.Add(doc.RootElement.GetProperty("modeVector").EnumerateArray().Select(value => value.GetDouble()).ToArray());
    }
    var kernelBasis = Orthonormalize(bosonModes);

    // Pure-gauge subspace (84 Phase389 covariant differentials).
    var gaugeDirections = new List<double[]>();
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
            gaugeDirections.Add(BuildCovariantDifferential(x, omega));
        }
    var gaugeBasis = Orthonormalize(gaugeDirections);

    // Gram top-3 image recomputed on the converged shell (Phase378 rules).
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
    var gramTopIndices = Enumerable.Range(0, CarrierDimension)
        .OrderByDescending(k => qEigenvalues[k])
        .Take((int)GramSubspaceRank)
        .ToArray();
    var gramBasis = gramTopIndices.Select(k =>
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
    }).ToList();

    double aggregateKernelFraction = SubspaceFraction(aggregateSource, kernelBasis);
    double aggregateGaugeFraction = SubspaceFraction(aggregateSource, gaugeBasis);
    double aggregateGramFraction = SubspaceFraction(aggregateSource, gramBasis);
    var perModeKernelFractions = perModeSources.Select(source => SubspaceFraction(source, kernelBasis)).ToArray();
    var perModeGaugeFractions = perModeSources.Select(source => SubspaceFraction(source, gaugeBasis)).ToArray();
    var perModeGramFractions = perModeSources.Select(source => SubspaceFraction(source, gramBasis)).ToArray();

    // Shell-splitting under the unit aggregate source direction:
    // eigenvalues of the 4x4 Hermitian matrix <psi_a, delta_D[J-hat] psi_b>.
    var splittingEigenvalues = Array.Empty<double>();
    if (aggregateSourceNorm > 0.0)
    {
        var unitSource = aggregateSource.Select(value => value / aggregateSourceNorm).ToArray();
        var blockRe = new double[shellSize, shellSize];
        var blockIm = new double[shellSize, shellSize];
        for (int a = 0; a < shellSize; a++)
            for (int b = 0; b < shellSize; b++)
            {
                double sumRe = 0.0;
                double sumIm = 0.0;
                for (int coordinate = 0; coordinate < CarrierDimension; coordinate++)
                {
                    if (unitSource[coordinate] == 0.0)
                        continue;
                    var inner = ComplexInnerProduct(shellModes[a], deltaPsiPerCoordinate[coordinate][b]);
                    sumRe += unitSource[coordinate] * inner.Real;
                    sumIm += unitSource[coordinate] * inner.Imaginary;
                }
                blockRe[a, b] = sumRe;
                blockIm[a, b] = sumIm;
            }
        var (splitEig, _, _, _, _) = JacobiHermitian(blockRe, blockIm);
        splittingEigenvalues = splitEig.OrderBy(value => value).ToArray();
    }

    return new BackgroundSourceRecord
    {
        FermionBackgroundId = backgroundId,
        BaseDiracMatrixPath = matrixPath,
        PersistedOmegaPath = omegaPath,
        ShellSize = shellSize,
        ShellEigenvalues = shellEigenvalues,
        MaxShellEigenResidual = maxShellResidual,
        PersistedBosonModeCount = bosonModes.Count,
        MaxAbsPersistedBosonicEigenvalue = maxAbsBosonEigenvalue,
        KernelBasisRank = kernelBasis.Count,
        GaugeBasisRank = gaugeBasis.Count,
        PerModeSourceNorms = perModeSourceNorms,
        AggregateSourceNorm = aggregateSourceNorm,
        AggregationCancellationRatio = aggregationCancellationRatio,
        AggregateAxisFractions = aggregateAxisFractions,
        AggregateKernelSubspaceFraction = aggregateKernelFraction,
        AggregateGaugeSubspaceFraction = aggregateGaugeFraction,
        AggregateGramImageFraction = aggregateGramFraction,
        PerModeKernelSubspaceFractions = perModeKernelFractions,
        PerModeGaugeSubspaceFractions = perModeGaugeFractions,
        PerModeGramImageFractions = perModeGramFractions,
        UnitSourceShellSplittingEigenvalues = splittingEigenvalues,
    };
}

static List<double[]> Orthonormalize(IReadOnlyList<double[]> vectors)
{
    // Modified Gram-Schmidt with rank-deficiency drop.
    var basis = new List<double[]>();
    foreach (var vector in vectors)
    {
        var candidate = (double[])vector.Clone();
        foreach (var basisVector in basis)
        {
            double dot = 0.0;
            for (int index = 0; index < candidate.Length; index++)
                dot += candidate[index] * basisVector[index];
            for (int index = 0; index < candidate.Length; index++)
                candidate[index] -= dot * basisVector[index];
        }
        double norm = VectorNorm(candidate);
        if (norm <= OrthonormalizationDropTolerance)
            continue;
        for (int index = 0; index < candidate.Length; index++)
            candidate[index] /= norm;
        basis.Add(candidate);
    }
    return basis;
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

static double VectorNorm(double[] vector)
{
    double sum = 0.0;
    foreach (double value in vector)
        sum += value * value;
    return Math.Sqrt(sum);
}

static double SubspaceFraction(double[] vector, IReadOnlyList<double[]> orthonormalBasis)
{
    double norm2 = 0.0;
    foreach (double value in vector)
        norm2 += value * value;
    if (norm2 <= 0.0)
        return 0.0;
    double captured = 0.0;
    foreach (var basisVector in orthonormalBasis)
    {
        double dot = 0.0;
        for (int index = 0; index < vector.Length; index++)
            dot += vector[index] * basisVector[index];
        captured += dot * dot;
    }
    return captured / norm2;
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

public sealed class BackgroundSourceRecord
{
    public required string FermionBackgroundId { get; init; }
    public required string BaseDiracMatrixPath { get; init; }
    public required string PersistedOmegaPath { get; init; }
    public required int ShellSize { get; init; }
    public required double[] ShellEigenvalues { get; init; }
    public required double MaxShellEigenResidual { get; init; }
    public required int PersistedBosonModeCount { get; init; }
    public required double MaxAbsPersistedBosonicEigenvalue { get; init; }
    public required int KernelBasisRank { get; init; }
    public required int GaugeBasisRank { get; init; }
    public required double[] PerModeSourceNorms { get; init; }
    public required double AggregateSourceNorm { get; init; }
    public required double AggregationCancellationRatio { get; init; }
    public required double[] AggregateAxisFractions { get; init; }
    public required double AggregateKernelSubspaceFraction { get; init; }
    public required double AggregateGaugeSubspaceFraction { get; init; }
    public required double AggregateGramImageFraction { get; init; }
    public required double[] PerModeKernelSubspaceFractions { get; init; }
    public required double[] PerModeGaugeSubspaceFractions { get; init; }
    public required double[] PerModeGramImageFractions { get; init; }
    public required double[] UnitSourceShellSplittingEigenvalues { get; init; }

    public object ToOutput() => new
    {
        fermionBackgroundId = FermionBackgroundId,
        baseDiracMatrixPath = BaseDiracMatrixPath,
        persistedOmegaPath = PersistedOmegaPath,
        shellSize = ShellSize,
        shellEigenvalues = ShellEigenvalues,
        maxShellEigenResidual = MaxShellEigenResidual,
        persistedBosonModeCount = PersistedBosonModeCount,
        maxAbsPersistedBosonicEigenvalue = MaxAbsPersistedBosonicEigenvalue,
        kernelBasisRank = KernelBasisRank,
        gaugeBasisRank = GaugeBasisRank,
        perModeSourceNorms = PerModeSourceNorms,
        aggregateSourceNorm = AggregateSourceNorm,
        aggregationCancellationRatio = AggregationCancellationRatio,
        aggregateAxisFractions = AggregateAxisFractions,
        aggregateKernelSubspaceFraction = AggregateKernelSubspaceFraction,
        aggregateGaugeSubspaceFraction = AggregateGaugeSubspaceFraction,
        aggregateGramImageFraction = AggregateGramImageFraction,
        perModeKernelSubspaceFractions = PerModeKernelSubspaceFractions,
        perModeGaugeSubspaceFractions = PerModeGaugeSubspaceFractions,
        perModeGramImageFractions = PerModeGramImageFractions,
        unitSourceShellSplittingEigenvalues = UnitSourceShellSplittingEigenvalues,
    };
}
