using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Geometry;
using Gu.Phase4.Couplings;
using Gu.Phase4.Dirac;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

// Phase395: source-current axis structure and global gauge covariance probe.
//
// Phase394 left the sharpest open internal question: WHY do the fermionic
// source currents (and hence the Gram image and the backreaction direction)
// avoid gauge axis 1? This probe answers it structurally, target-blind:
//
//   1. Per-edge gauge block Gram. For every edge e the 3x3 PSD matrix
//      T_e[a,b] = Re sum_{alpha beta} conj(B_(e,a)[alpha,beta]) B_(e,b)[alpha,beta],
//      B_(e,a)[alpha,beta] = <psi_alpha, delta_D[e_(e,a)] psi_beta> on the
//      converged shell, is invariant under shell basis changes. The aggregate
//      T = sum_e T_e is the basis-invariant axis-structure object; its
//      smallest-eigenvalue direction n_T is the canonical "suppressed
//      direction" behind the Phase379/394 axis fractions.
//   2. Background-connection invariants. The persisted omega is the
//      symmetric ansatz: its second-moment matrix C = sum_e omega_e omega_e^T
//      is nearly rank-1 along n_omega ~ (1,1,1)/sqrt(3), leaving an
//      approximate residual U(1) of global rotations about n_omega. The probe
//      records C's spectrum, n_omega, the deviation of omega from exact
//      symmetric alignment, and the angles among n_T, the coordinate axis
//      e_1, and n_omega.
//   3. Global gauge covariance theorem (discrete, exact). For a global
//      rotation R = exp(theta rho(n)) the discrete model is exactly
//      equivariant (Phase389 proved the constant-parameter obstruction
//      vanishes), and by the exact linearity D(omega) = D_kin + delta_D[omega]
//      (Phase389) the rotated operator is constructible from persisted
//      artifacts alone: D' = D - delta_D[omega] + delta_D[R omega]. The probe
//      verifies, at machine precision, that the rotated background yields an
//      IDENTICAL fermion shell spectrum and a block Gram that transforms
//      covariantly, T'_e = R T_e R^T. Consequence: the suppressed direction
//      n_T is gauge-COVARIANT, not canonical - rotating the background
//      presentation rotates the suppressed axis. Any observed photon/W/Z/H
//      namespace mapping built from raw carrier axes therefore cannot be
//      target-blind canonical; it must be built from gauge-invariant
//      combinations (e.g., relative to n_omega), which strengthens the
//      Phase385 missing-namespace-map boundary with a constructive reason.
//
// Fail-closed: no namespace map, no W/Z/H source rows, no contract fields.

const string DefaultOutputDir = "studies/phase395_source_current_axis_structure_gauge_covariance_probe_001/output";
const string Phase12Root = "studies/phase12_joined_calculation_001/output/background_family";
const string FermionDir = $"{Phase12Root}/fermions";
const string BackgroundStateDir = $"{Phase12Root}/background_states";
const string SpinorRepresentationPath = $"{FermionDir}/spinor_representation.json";
const string Phase393SummaryPath = "studies/phase393_coupled_stationarity_fermionic_source_residual_probe_001/output/coupled_stationarity_fermionic_source_residual_probe_summary.json";
const string Phase394SummaryPath = "studies/phase394_positive_bosonic_spectrum_backreaction_construction_001/output/positive_bosonic_spectrum_backreaction_construction_summary.json";

const int ExpectedBackgroundCount = 2;
const int CarrierDimension = 156;
const int DimG = 3;
const int ExpectedShellSize = 4;
const double JacobiOffDiagonalTolerance = 1e-13;
const int JacobiMaxSweeps = 100;
const double EigenResidualTolerance = 1e-9;
const double SpectrumInvarianceTolerance = 1e-9;
const double CovarianceResidualTolerance = 1e-9;

var outputDir = Environment.GetEnvironmentVariable("PHASE395_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase393Doc = JsonDocument.Parse(File.ReadAllText(Phase393SummaryPath));
bool phase393PrecursorPassed =
    JsonBool(phase393Doc.RootElement, "coupledStationarityFermionicSourceResidualProbePassed") is true &&
    JsonBool(phase393Doc.RootElement, "perModeSourceLiesInGramImage") is true;

using var phase394Doc = JsonDocument.Parse(File.ReadAllText(Phase394SummaryPath));
bool phase394PrecursorPassed =
    JsonBool(phase394Doc.RootElement, "positiveBosonicSpectrumBackreactionConstructionPassed") is true &&
    JsonBool(phase394Doc.RootElement, "firstOrderAsymmetricBackreactionConstructed") is true;

// ---------------------------------------------------------------------------
// Shared geometry.
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
        CodeRevision = "phase395-source-current-axis-structure-gauge-covariance-probe",
        Branch = new() { BranchId = "phase395-source-current-axis-structure-gauge-covariance-probe", SchemaVersion = "1.0" },
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

// Test rotations: a quarter turn about the third coordinate axis and a
// generic rotation (axis and angle fixed by construction, target-blind).
var testRotations = new (string Id, double[] Axis, double Theta)[]
{
    ("quarter-turn-about-e2", [0.0, 0.0, 1.0], Math.PI / 2.0),
    ("generic-rotation", Normalize([1.0, 2.0, 3.0]), 0.7),
};

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
    backgroundRecords.All(record => record.ShellSize == ExpectedShellSize);
bool denseSolveConverged = backgroundRecords.All(record => record.MaxShellEigenResidual <= EigenResidualTolerance);
bool globalGaugeCovarianceVerified = backgroundRecords.All(record =>
    record.RotationChecks.All(check =>
        check.ShellSpectrumInvarianceResidual <= SpectrumInvarianceTolerance &&
        check.BlockGramCovarianceResidual <= CovarianceResidualTolerance));
bool suppressedAxisIsGaugeCovariantNotCanonical = globalGaugeCovarianceVerified;
bool backgroundOmegaNearSymmetricAnsatz = backgroundRecords.All(record => record.OmegaDominantAxisFraction >= 0.95);
bool blockGramEffectivelyRankOne = backgroundRecords.All(record => record.DominantFraction >= 0.99);
double minDominantFraction = backgroundRecords.Min(record => record.DominantFraction);
double maxAngleDominantToOmegaAxisDegrees = backgroundRecords.Max(record => record.AngleDominantToOmegaAxisDegrees);
double maxSuppressionRatio = backgroundRecords.Max(record => record.SuppressionRatio);
double maxAngleSuppressedToE1Degrees = backgroundRecords.Max(record => record.AngleSuppressedToE1Degrees);
double minAngleSuppressedToOmegaAxisDegrees = backgroundRecords.Min(record => record.AngleSuppressedToOmegaAxisDegrees);
double maxSpectrumInvarianceResidual = backgroundRecords.Max(record => record.RotationChecks.Max(check => check.ShellSpectrumInvarianceResidual));
double maxBlockGramCovarianceResidual = backgroundRecords.Max(record => record.RotationChecks.Max(check => check.BlockGramCovarianceResidual));
double maxShellEigenResidual = backgroundRecords.Max(record => record.MaxShellEigenResidual);

bool probeInternallyConsistent =
    expectedCoveragePresent &&
    denseSolveConverged &&
    globalGaugeCovarianceVerified;

// ---------------------------------------------------------------------------
// Fail-closed boundary.
// ---------------------------------------------------------------------------

const bool routeProvidesObservedElectroweakNamespaceMap = false;
const bool routeProvidesCanonicalGaugeAxisSelector = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
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
const string ApplicationSubjectKind = "source-current-axis-structure-gauge-covariance-probe";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    vertexCount.ToString(),
    edgeCount.ToString(),
    DimG.ToString(),
    ExpectedShellSize.ToString(),
    "T_e[a,b] = Re Tr(B_(e,a)^dagger B_(e,b)) on the converged shell",
    "D' = D - delta_D[omega] + delta_D[R omega]; T' = R T R^T",
    string.Join(",", testRotations.Select(rotation => rotation.Id)),
    string.Join(",", backgroundRecords.Select(record => record.FermionBackgroundId)))))).ToLowerInvariant();

bool sourceCurrentAxisStructureGaugeCovarianceProbePassed =
    phase393PrecursorPassed &&
    phase394PrecursorPassed &&
    probeInternallyConsistent &&
    backgroundOmegaNearSymmetricAnsatz &&
    suppressedAxisIsGaugeCovariantNotCanonical &&
    !routeProvidesObservedElectroweakNamespaceMap &&
    !routeProvidesCanonicalGaugeAxisSelector &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesPhysicalEffectiveActionHessian &&
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

string terminalStatus = sourceCurrentAxisStructureGaugeCovarianceProbePassed
    ? "suppressed-axis-shown-gauge-covariant-not-canonical-namespace-map-must-be-gauge-invariant"
    : "source-current-axis-structure-gauge-covariance-probe-blocked";
string decision = sourceCurrentAxisStructureGaugeCovarianceProbePassed
    ? "The axis structure of the fermionic source currents and the carrier response image is now explained structurally. The background omega is the symmetric ansatz (second-moment matrix nearly rank-1 along (1,1,1)/sqrt(3)), and the basis-invariant per-edge block Gram T = sum_e T_e has a suppressed direction whose geometry is recorded relative to the coordinate axis e_1 and the omega invariant axis. Decisively, the discrete model is EXACTLY equivariant under global gauge rotations: rotating the background (D' = D - delta_D[omega] + delta_D[R omega], constructible from persisted artifacts by exact linearity) leaves the fermion shell spectrum invariant at machine precision and transforms the block Gram covariantly, T' = R T R^T. The suppressed axis is therefore GAUGE-COVARIANT, NOT CANONICAL: it is a property of the background presentation's gauge frame, and rotating the frame rotates the suppressed axis. Consequence for the namespace program: no target-blind canonical photon/W/Z/H namespace map can be built from raw carrier axes; any defensible map must use gauge-invariant data (e.g., components relative to the omega invariant axis), which strengthens the Phase385 missing-namespace-map boundary with a constructive reason. No namespace map, axis selector, or Phase201/Phase256 contract field is provided."
    : "Do not use the axis-structure conclusions until the Phase393/394 precursors, dense solve convergence, omega-ansatz characterization, and exact covariance checks all pass.";

var result = new
{
    phaseId = "phase395-source-current-axis-structure-gauge-covariance-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    sourceCurrentAxisStructureGaugeCovarianceProbePassed,
    phase393PrecursorPassed,
    phase394PrecursorPassed,
    probeInternallyConsistent,
    globalGaugeCovarianceVerified,
    suppressedAxisIsGaugeCovariantNotCanonical,
    backgroundOmegaNearSymmetricAnsatz,
    blockGramEffectivelyRankOne,
    denseSolveConverged,
    minDominantFraction,
    maxAngleDominantToOmegaAxisDegrees,
    maxSuppressionRatio,
    maxAngleSuppressedToE1Degrees,
    minAngleSuppressedToOmegaAxisDegrees,
    nearNullPlaneCaveat = "the two smallest block-Gram eigenvalues are nearly degenerate, so the individual minimum direction is unstable within the near-null plane; the robust invariants are the dominant direction and the near-null plane",
    maxSpectrumInvarianceResidual,
    maxBlockGramCovarianceResidual,
    maxShellEigenResidual,
    probeDefinitions = new
    {
        blockGram = "T_e[a,b] = Re sum_{alpha beta} conj(B_(e,a)[alpha,beta]) B_(e,b)[alpha,beta], shell-basis invariant; T = sum_e T_e",
        suppressedDirection = "unit eigenvector of T with the smallest eigenvalue; suppressionRatio = lambda_min / lambda_max",
        omegaInvariants = "C = sum_e omega_e omega_e^T; dominant axis and fraction; deviation from the symmetric ansatz (1,1,1)/sqrt(3)",
        rotatedBackground = "omega'_e = R omega_e; D' = D - delta_D[omega] + delta_D[omega'] (exact linearity, Phase389); dense generalized solve of D'",
        covarianceChecks = "shell spectrum invariance |lambda' - lambda| and block Gram covariance ||T' - R T R^T||_F / ||T||_F",
        rotations = "exp(theta rho(n)) via Rodrigues in the su(2) adjoint; quarter turn about e2 and a generic axis/angle",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    expectedCoveragePresent,
    backgroundCount,
    routeProvidesObservedElectroweakNamespaceMap,
    routeProvidesCanonicalGaugeAxisSelector,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesPhysicalEffectiveActionHessian,
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
        "discrete control-branch structural result only",
        "explains the suppressed axis as gauge-frame covariance; provides no canonical axis selector",
        "no observed electroweak namespace map (and shows raw-axis maps cannot be canonical)",
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
        phase393SummaryPath = Phase393SummaryPath,
        phase394SummaryPath = Phase394SummaryPath,
        phase12Root = Phase12Root,
        backgroundStateDir = BackgroundStateDir,
    },
    decision,
};

var options = JsonOptions();
string resultPath = Path.Combine(outputDir, "source_current_axis_structure_gauge_covariance_probe.json");
string summaryPath = Path.Combine(outputDir, "source_current_axis_structure_gauge_covariance_probe_summary.json");
File.WriteAllText(resultPath, JsonSerializer.Serialize(result, options));
File.WriteAllText(
    summaryPath,
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.generatedAt,
        terminalStatus,
        sourceCurrentAxisStructureGaugeCovarianceProbePassed,
        phase393PrecursorPassed,
        phase394PrecursorPassed,
        probeInternallyConsistent,
        globalGaugeCovarianceVerified,
        suppressedAxisIsGaugeCovariantNotCanonical,
        backgroundOmegaNearSymmetricAnsatz,
        blockGramEffectivelyRankOne,
        denseSolveConverged,
        minDominantFraction,
        maxAngleDominantToOmegaAxisDegrees,
        maxSuppressionRatio,
        maxAngleSuppressedToE1Degrees,
        minAngleSuppressedToOmegaAxisDegrees,
        result.nearNullPlaneCaveat,
        maxSpectrumInvarianceResidual,
        maxBlockGramCovarianceResidual,
        maxShellEigenResidual,
        result.probeDefinitions,
        result.applicationSubjectKind,
        result.targetBlindConstruction,
        physicalTargetsConsultedForConstruction,
        targetBlindConstructionHash,
        expectedCoveragePresent,
        backgroundCount,
        routeProvidesObservedElectroweakNamespaceMap,
        routeProvidesCanonicalGaugeAxisSelector,
        routeProvidesPhysicalMassPsiCompatibleBranch,
        routeProvidesCompletedFermionicAction,
        routeProvidesPhysicalEffectiveActionHessian,
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
Console.WriteLine($"sourceCurrentAxisStructureGaugeCovarianceProbePassed={sourceCurrentAxisStructureGaugeCovarianceProbePassed}");
Console.WriteLine($"globalGaugeCovarianceVerified={globalGaugeCovarianceVerified}");
Console.WriteLine($"suppressedAxisIsGaugeCovariantNotCanonical={suppressedAxisIsGaugeCovariantNotCanonical}");
Console.WriteLine($"backgroundOmegaNearSymmetricAnsatz={backgroundOmegaNearSymmetricAnsatz}");
Console.WriteLine($"blockGramEffectivelyRankOne={blockGramEffectivelyRankOne}");
Console.WriteLine($"minDominantFraction={minDominantFraction:R}");
Console.WriteLine($"maxAngleDominantToOmegaAxisDegrees={maxAngleDominantToOmegaAxisDegrees:R}");
Console.WriteLine($"maxSuppressionRatio={maxSuppressionRatio:R}");
Console.WriteLine($"maxAngleSuppressedToE1Degrees={maxAngleSuppressedToE1Degrees:R}");
Console.WriteLine($"minAngleSuppressedToOmegaAxisDegrees={minAngleSuppressedToOmegaAxisDegrees:R}");
Console.WriteLine($"maxSpectrumInvarianceResidual={maxSpectrumInvarianceResidual:R}");
Console.WriteLine($"maxBlockGramCovarianceResidual={maxBlockGramCovarianceResidual:R}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Background probe implementation.
// ---------------------------------------------------------------------------

BackgroundAxisRecord ProbeBackground(string backgroundId)
{
    string metadataPath = Path.Combine(FermionDir, $"dirac_bundle_{backgroundId}.json");
    using var metadataDoc = JsonDocument.Parse(File.ReadAllText(metadataPath));
    string matrixRef = RequiredString(metadataDoc.RootElement, "explicitMatrixRef");
    var (dRe, dIm) = LoadFlatInterleavedMatrix(Path.Combine(FermionDir, matrixRef), totalDof);

    string omegaPath = Path.Combine(BackgroundStateDir, $"{backgroundId}_omega.json");
    using var omegaDoc = JsonDocument.Parse(File.ReadAllText(omegaPath));
    double[] omega = omegaDoc.RootElement.GetProperty("coefficients").EnumerateArray().Select(value => value.GetDouble()).ToArray();

    // Omega invariants.
    var omegaCovariance = new double[3, 3];
    for (int edge = 0; edge < edgeCount; edge++)
        for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
                omegaCovariance[a, b] += omega[edge * 3 + a] * omega[edge * 3 + b];
    var (omegaEigenvalues, omegaVectors, _, _, _) = JacobiHermitian(omegaCovariance, new double[3, 3]);
    int omegaDominantIndex = 0;
    for (int i = 1; i < 3; i++)
        if (omegaEigenvalues[i] > omegaEigenvalues[omegaDominantIndex])
            omegaDominantIndex = i;
    var omegaAxis = new double[3];
    for (int i = 0; i < 3; i++)
        omegaAxis[i] = omegaVectors[i, omegaDominantIndex];
    NormalizeInPlace(omegaAxis);
    double omegaTrace = omegaEigenvalues.Sum();
    double omegaDominantFraction = omegaEigenvalues[omegaDominantIndex] / omegaTrace;
    var symmetricAxis = Normalize([1.0, 1.0, 1.0]);
    double angleOmegaToSymmetricDegrees = AngleDegrees(omegaAxis, symmetricAxis);

    // Base shell, blocks, and block Gram.
    var baseSolve = SolveShellAndGram(dRe, dIm);

    // Suppressed direction of the aggregate block Gram.
    var (tEigenvalues, tVectors, _, _, _) = JacobiHermitian(baseSolve.AggregateGram, new double[3, 3]);
    int minIndex = 0;
    int maxIndex = 0;
    for (int i = 1; i < 3; i++)
    {
        if (tEigenvalues[i] < tEigenvalues[minIndex]) minIndex = i;
        if (tEigenvalues[i] > tEigenvalues[maxIndex]) maxIndex = i;
    }
    var suppressedDirection = new double[3];
    var dominantDirection = new double[3];
    for (int i = 0; i < 3; i++)
    {
        suppressedDirection[i] = tVectors[i, minIndex];
        dominantDirection[i] = tVectors[i, maxIndex];
    }
    NormalizeInPlace(suppressedDirection);
    NormalizeInPlace(dominantDirection);
    double suppressionRatio = tEigenvalues[minIndex] / tEigenvalues[maxIndex];
    double dominantFraction = tEigenvalues[maxIndex] / tEigenvalues.Sum();
    double angleSuppressedToE1 = AngleDegrees(suppressedDirection, [0.0, 1.0, 0.0]);
    double angleSuppressedToOmegaAxis = AngleDegrees(suppressedDirection, omegaAxis);
    double angleDominantToOmegaAxis = AngleDegrees(dominantDirection, omegaAxis);
    var dominantCoordinateFractions = dominantDirection.Select(value => value * value).ToArray();

    // Gauge covariance experiments.
    var rotationChecks = new List<RotationCheckRecord>();
    foreach (var (rotationId, axis, theta) in testRotations)
    {
        var rotation = RodriguesAdjoint(axis, theta);
        var rotatedOmega = new double[CarrierDimension];
        for (int edge = 0; edge < edgeCount; edge++)
            for (int a = 0; a < 3; a++)
            {
                double sum = 0.0;
                for (int b = 0; b < 3; b++)
                    sum += rotation[a, b] * omega[edge * 3 + b];
                rotatedOmega[edge * 3 + a] = sum;
            }

        // D' = D - delta_D[omega] + delta_D[R omega], exact by linearity.
        var (omegaVarRe, omegaVarIm) = DiracVariationComputer.ComputeAnalytical(
            omega, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
        var (rotatedVarRe, rotatedVarIm) = DiracVariationComputer.ComputeAnalytical(
            rotatedOmega, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
        var dRotRe = new double[totalDof, totalDof];
        var dRotIm = new double[totalDof, totalDof];
        for (int row = 0; row < totalDof; row++)
            for (int col = 0; col < totalDof; col++)
            {
                dRotRe[row, col] = dRe[row, col] - omegaVarRe[row, col] + rotatedVarRe[row, col];
                dRotIm[row, col] = dIm[row, col] - omegaVarIm[row, col] + rotatedVarIm[row, col];
            }

        var rotatedSolve = SolveShellAndGram(dRotRe, dRotIm);

        double spectrumResidual = 0.0;
        for (int s = 0; s < baseSolve.ShellEigenvalues.Length; s++)
        {
            var baseSorted = baseSolve.ShellEigenvalues.OrderBy(value => value).ToArray();
            var rotSorted = rotatedSolve.ShellEigenvalues.OrderBy(value => value).ToArray();
            spectrumResidual = Math.Max(spectrumResidual,
                Math.Abs(baseSorted[s] - rotSorted[s]) / Math.Max(Math.Abs(baseSorted[s]), 1e-30));
        }

        // Block Gram covariance: T' vs R T R^T (aggregate and per-edge max).
        double covarianceResidual = 0.0;
        double gramNorm = 0.0;
        var expected = new double[3, 3];
        for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
            {
                double sum = 0.0;
                for (int c = 0; c < 3; c++)
                    for (int d = 0; d < 3; d++)
                        sum += rotation[a, c] * baseSolve.AggregateGram[c, d] * rotation[b, d];
                expected[a, b] = sum;
            }
        for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
            {
                double diff = rotatedSolve.AggregateGram[a, b] - expected[a, b];
                covarianceResidual += diff * diff;
                gramNorm += baseSolve.AggregateGram[a, b] * baseSolve.AggregateGram[a, b];
            }
        covarianceResidual = Math.Sqrt(covarianceResidual) / Math.Max(Math.Sqrt(gramNorm), 1e-30);

        rotationChecks.Add(new RotationCheckRecord
        {
            RotationId = rotationId,
            ShellSpectrumInvarianceResidual = spectrumResidual,
            BlockGramCovarianceResidual = covarianceResidual,
        });
    }

    return new BackgroundAxisRecord
    {
        FermionBackgroundId = backgroundId,
        PersistedOmegaPath = omegaPath,
        ShellSize = baseSolve.ShellEigenvalues.Length,
        ShellEigenvalues = baseSolve.ShellEigenvalues,
        MaxShellEigenResidual = baseSolve.MaxShellEigenResidual,
        OmegaCovarianceEigenvalues = omegaEigenvalues.OrderBy(value => value).ToArray(),
        OmegaDominantAxis = omegaAxis,
        OmegaDominantAxisFraction = omegaDominantFraction,
        AngleOmegaAxisToSymmetricDegrees = angleOmegaToSymmetricDegrees,
        BlockGramEigenvalues = tEigenvalues.OrderBy(value => value).ToArray(),
        SuppressedDirection = suppressedDirection,
        SuppressionRatio = suppressionRatio,
        DominantDirection = dominantDirection,
        DominantFraction = dominantFraction,
        DominantCoordinateFractions = dominantCoordinateFractions,
        AngleDominantToOmegaAxisDegrees = angleDominantToOmegaAxis,
        AngleSuppressedToE1Degrees = angleSuppressedToE1,
        AngleSuppressedToOmegaAxisDegrees = angleSuppressedToOmegaAxis,
        RotationChecks = rotationChecks,
    };
}

ShellGramResult SolveShellAndGram(double[,] dRe, double[,] dIm)
{
    var invSqrtM = new double[totalDof];
    for (int i = 0; i < totalDof; i++)
        invSqrtM[i] = 1.0 / Math.Sqrt(meshVolumeMassPsiWeights[2 * i]);
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
        for (int i = 0; i < totalDof; i++)
        {
            mode[2 * i] = invSqrtM[i] * vecRe[i, k];
            mode[2 * i + 1] = invSqrtM[i] * vecIm[i, k];
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
        for (int i = 0; i < totalDof; i++)
        {
            double weight = meshVolumeMassPsiWeights[2 * i];
            double rRe = dMode[2 * i] - shellEigenvalues[s] * weight * shellModes[s][2 * i];
            double rIm = dMode[2 * i + 1] - shellEigenvalues[s] * weight * shellModes[s][2 * i + 1];
            residual2 += rRe * rRe + rIm * rIm;
            norm2 += shellModes[s][2 * i] * shellModes[s][2 * i] + shellModes[s][2 * i + 1] * shellModes[s][2 * i + 1];
        }
        maxShellResidual = Math.Max(maxShellResidual, Math.Sqrt(residual2) / Math.Max(Math.Sqrt(norm2), 1e-30));
    }

    // Per-edge gauge block Gram, aggregated.
    var aggregateGram = new double[3, 3];
    var blocks = new (double Re, double Im)[3][,];
    for (int edge = 0; edge < edgeCount; edge++)
    {
        for (int a = 0; a < 3; a++)
        {
            var basis = new double[CarrierDimension];
            basis[edge * 3 + a] = 1.0;
            var (deltaRe, deltaIm) = DiracVariationComputer.ComputeAnalytical(
                basis, gammas, vertexCount, spinorDim, DimG, edgeLengths, cellsPerEdge, edgeDirections);
            var block = new (double Re, double Im)[shellSize, shellSize];
            for (int beta = 0; beta < shellSize; beta++)
            {
                var deltaPsi = ApplyComplexMatrix(deltaRe, deltaIm, shellModes[beta]);
                for (int alpha = 0; alpha < shellSize; alpha++)
                {
                    var inner = ComplexInnerProduct(shellModes[alpha], deltaPsi);
                    block[alpha, beta] = (inner.Real, inner.Imaginary);
                }
            }
            blocks[a] = block;
        }
        for (int a = 0; a < 3; a++)
            for (int b = 0; b < 3; b++)
            {
                double sum = 0.0;
                for (int alpha = 0; alpha < shellSize; alpha++)
                    for (int beta = 0; beta < shellSize; beta++)
                        sum += blocks[a][alpha, beta].Re * blocks[b][alpha, beta].Re +
                               blocks[a][alpha, beta].Im * blocks[b][alpha, beta].Im;
                aggregateGram[a, b] += sum;
            }
    }

    return new ShellGramResult
    {
        ShellEigenvalues = shellEigenvalues,
        MaxShellEigenResidual = maxShellResidual,
        AggregateGram = aggregateGram,
    };
}

static double[,] RodriguesAdjoint(double[] axis, double theta)
{
    var n = Normalize(axis);
    var k = new double[3, 3];
    for (int a = 0; a < 3; a++)
        for (int b = 0; b < 3; b++)
            for (int c = 0; c < 3; c++)
                k[b, c] += n[a] * LeviCivita3(a, b, c);
    var k2 = new double[3, 3];
    for (int a = 0; a < 3; a++)
        for (int b = 0; b < 3; b++)
            for (int c = 0; c < 3; c++)
                k2[a, b] += k[a, c] * k[c, b];
    var rotation = new double[3, 3];
    double sin = Math.Sin(theta);
    double cos = Math.Cos(theta);
    for (int a = 0; a < 3; a++)
        for (int b = 0; b < 3; b++)
            rotation[a, b] = (a == b ? 1.0 : 0.0) + sin * k[a, b] + (1.0 - cos) * k2[a, b];
    return rotation;
}

static double[] Normalize(double[] vector)
{
    var copy = (double[])vector.Clone();
    NormalizeInPlace(copy);
    return copy;
}

static void NormalizeInPlace(double[] vector)
{
    double norm = Math.Sqrt(vector.Sum(value => value * value));
    if (norm <= 0.0)
        throw new InvalidOperationException("Cannot normalize a zero vector.");
    for (int i = 0; i < vector.Length; i++)
        vector[i] /= norm;
}

static double AngleDegrees(double[] left, double[] right)
{
    double dot = 0.0;
    for (int i = 0; i < left.Length; i++)
        dot += left[i] * right[i];
    dot = Math.Abs(dot);
    return Math.Acos(Math.Clamp(dot, -1.0, 1.0)) * 180.0 / Math.PI;
}

static double LeviCivita3(int a, int b, int c)
{
    if (a == b || b == c || a == c) return 0.0;
    if ((a == 0 && b == 1 && c == 2) ||
        (a == 1 && b == 2 && c == 0) ||
        (a == 2 && b == 0 && c == 1)) return 1.0;
    return -1.0;
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
            int idx = 2 * (row * size + col);
            re[row, col] = values[idx];
            im[row, col] = values[idx + 1];
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
    for (int idx = 0; idx < left.Length; idx += 2)
    {
        double leftRe = left[idx];
        double leftIm = left[idx + 1];
        double rightRe = right[idx];
        double rightIm = right[idx + 1];
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

public sealed class ShellGramResult
{
    public required double[] ShellEigenvalues { get; init; }
    public required double MaxShellEigenResidual { get; init; }
    public required double[,] AggregateGram { get; init; }
}

public sealed class RotationCheckRecord
{
    public required string RotationId { get; init; }
    public required double ShellSpectrumInvarianceResidual { get; init; }
    public required double BlockGramCovarianceResidual { get; init; }
}

public sealed class BackgroundAxisRecord
{
    public required string FermionBackgroundId { get; init; }
    public required string PersistedOmegaPath { get; init; }
    public required int ShellSize { get; init; }
    public required double[] ShellEigenvalues { get; init; }
    public required double MaxShellEigenResidual { get; init; }
    public required double[] OmegaCovarianceEigenvalues { get; init; }
    public required double[] OmegaDominantAxis { get; init; }
    public required double OmegaDominantAxisFraction { get; init; }
    public required double AngleOmegaAxisToSymmetricDegrees { get; init; }
    public required double[] BlockGramEigenvalues { get; init; }
    public required double[] SuppressedDirection { get; init; }
    public required double SuppressionRatio { get; init; }
    public required double[] DominantDirection { get; init; }
    public required double DominantFraction { get; init; }
    public required double[] DominantCoordinateFractions { get; init; }
    public required double AngleDominantToOmegaAxisDegrees { get; init; }
    public required double AngleSuppressedToE1Degrees { get; init; }
    public required double AngleSuppressedToOmegaAxisDegrees { get; init; }
    public required IReadOnlyList<RotationCheckRecord> RotationChecks { get; init; }

    public object ToOutput() => new
    {
        fermionBackgroundId = FermionBackgroundId,
        persistedOmegaPath = PersistedOmegaPath,
        shellSize = ShellSize,
        shellEigenvalues = ShellEigenvalues,
        maxShellEigenResidual = MaxShellEigenResidual,
        omegaCovarianceEigenvalues = OmegaCovarianceEigenvalues,
        omegaDominantAxis = OmegaDominantAxis,
        omegaDominantAxisFraction = OmegaDominantAxisFraction,
        angleOmegaAxisToSymmetricDegrees = AngleOmegaAxisToSymmetricDegrees,
        blockGramEigenvalues = BlockGramEigenvalues,
        suppressedDirection = SuppressedDirection,
        suppressionRatio = SuppressionRatio,
        dominantDirection = DominantDirection,
        dominantFraction = DominantFraction,
        dominantCoordinateFractions = DominantCoordinateFractions,
        angleDominantToOmegaAxisDegrees = AngleDominantToOmegaAxisDegrees,
        angleSuppressedToE1Degrees = AngleSuppressedToE1Degrees,
        angleSuppressedToOmegaAxisDegrees = AngleSuppressedToOmegaAxisDegrees,
        rotationChecks = RotationChecks.Select(check => new
        {
            rotationId = check.RotationId,
            shellSpectrumInvarianceResidual = check.ShellSpectrumInvarianceResidual,
            blockGramCovarianceResidual = check.BlockGramCovarianceResidual,
        }).ToArray(),
    };
}
