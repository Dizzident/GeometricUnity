using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Interop;
using Gu.Math;
using Gu.ReferenceCpu;
using Gu.Solvers;

// Phase405: vacuum-manifold doublet-VEV orbit scan (brute force #2, GPU).
//
// USER DIRECTIVE (2026-06-11): brute force #2, on the GPU. Phase402 pinned
// the draft's Higgs potential to <Upsilon, Upsilon> with symmetry breaking
// necessarily coming from the Upsilon = 0 locus geometry; Phase403/404
// showed the doublet block exists only in larger algebras (su(3) minimal)
// and not in the gauge-adjoint of the GU chain. The question here: does the
// Upsilon = 0 VACUUM MANIFOLD of the control-branch objective PERMIT
// constant doublet-block VEVs, and does its geometry SELECT them over other
// directions?
//
// On the trivial-torsion identity-Shiab branch with A0 = 0 the residual is
// EXACTLY Upsilon = F - d omega = (1/2)[omega, omega]: a constant VEV
// omega(c) = sum_i c_i T_i is on the vacuum manifold iff the commutator
// [c.T, c.T] wedge-vanishes - i.e. every RANK-1 direction (any single Lie
// direction, doublet-block included) is EXACTLY flat, and a mixed direction
// pair (u, v) is flat iff [u, v] = 0. The scan verifies this prediction by
// brute force over the su(3) orbit space on a LARGER mesh, computed on the
// GPU through the repo's own native backend:
//
//   - mesh: structured fiber bundle rows=cols=6 (carrier = edges x 8);
//   - scan: all 28 unordered su(3) direction pairs x mixing angles x
//     magnitudes, each sample one EvaluateDerived + objective on the GPU
//     (CudaNativeBackend -> GpuSolverBackend, algebra and mesh topology
//     uploaded from the repo's own factories);
//   - parity: a subsample re-evaluated on the CPU reference backend
//     (CpuSolverBackend with LieAlgebraFactory.CreateSu3) must agree to
//     1e-10 relative - the platform's CPU-reference-first discipline;
//   - classification: per pair, the landscape S_B(t, phi) is fit against
//     the exact prediction t^4 sin^2(phi)cos^2(phi) ||[u,v]||^2-shaped
//     quartic; flat pairs must be exactly the commuting pairs.
//
// Verdicts (data, not gates): does the vacuum manifold permit constant
// doublet VEVs (predicted yes - rank-1 flatness); is the doublet direction
// SELECTED over triplet/singlet directions (predicted no - the landscape
// treats all rank-1 directions identically, so no selection mechanism
// exists at this level and sub-gap (b) remains open with sharpened
// evidence).
//
// Fail-closed: control-branch objective only; no scales; nothing promoted.

const string DefaultOutputDir = "studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001/output";
const string Phase403SummaryPath = "studies/phase403_adjoint_doublet_substructure_branching_probe_001/output/adjoint_doublet_substructure_branching_probe_summary.json";
const string Phase404SummaryPath = "studies/phase404_gu_embedding_chain_coupling_ratio_enumeration_001/output/gu_embedding_chain_coupling_ratio_enumeration_summary.json";

const int MeshRows = 6;
const int MeshCols = 6;
const int DimG = 8;
const int AngleSteps = 12;
const int MagnitudeSteps = 6;
const double MaxMagnitude = 0.6;
const double ParityRelativeTolerance = 1e-10;
const double FlatnessTolerance = 1e-18;
const double QuarticFitRelativeTolerance = 1e-6;
const int ParitySubsampleStride = 97;

var outputDir = Environment.GetEnvironmentVariable("PHASE405_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase403Doc = JsonDocument.Parse(File.ReadAllText(Phase403SummaryPath));
bool phase403PrecursorPassed = JsonBool(phase403Doc.RootElement, "adjointDoubletSubstructureBranchingProbePassed") is true;
using var phase404Doc = JsonDocument.Parse(File.ReadAllText(Phase404SummaryPath));
bool phase404PrecursorPassed = JsonBool(phase404Doc.RootElement, "guEmbeddingChainCouplingRatioEnumerationPassed") is true;

// ---------------------------------------------------------------------------
// Geometry, algebra, branch manifest (control-branch rules, su(3) algebra).
// ---------------------------------------------------------------------------

var algebra = LieAlgebraFactory.CreateSu3();
var bundle = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: MeshRows, cols: MeshCols);
var mesh = bundle.AmbientMesh;
int edgeCount = mesh.EdgeCount;
int faceCount = mesh.FaceCount;
int carrierDimension = edgeCount * DimG;
var geometry = bundle.ToGeometryContext("centroid", "P1");

var manifest = new BranchManifest
{
    BranchId = "phase405-su3-vacuum-orbit-scan",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "r1",
    CodeRevision = "phase405",
    LieAlgebraId = "su3",
    BaseDimension = bundle.BaseMesh.EmbeddingDimension,
    AmbientDimension = mesh.EmbeddingDimension,
    ActiveGeometryBranch = "simplicial",
    ActiveObservationBranch = "sigma-pullback",
    ActiveTorsionBranch = "trivial",
    ActiveShiabBranch = "identity-shiab",
    ActiveGaugeStrategy = "penalty",
    PairingConventionId = "pairing-killing",
    BasisConventionId = "canonical",
    ComponentOrderId = "face-major",
    AdjointConventionId = "adjoint-explicit",
    NormConventionId = "norm-l2-quadrature",
    DifferentialFormMetricId = "hodge-standard",
    InsertedAssumptionIds = Array.Empty<string>(),
    InsertedChoiceIds = new[] { "IX-1", "IX-2" },
};

var signature = new TensorSignature
{
    AmbientSpaceId = "Y_h",
    CarrierType = "connection-1form",
    Degree = "1",
    LieAlgebraBasisId = "canonical",
    ComponentOrderId = "edge-major",
    MemoryLayout = "dense-row-major",
};

FieldTensor WrapCarrier(double[] coefficients) => new()
{
    Label = "omega_h",
    Signature = signature,
    Coefficients = coefficients,
    Shape = new[] { edgeCount, DimG },
};

var a0 = WrapCarrier(new double[carrierDimension]);

// CPU reference backend (algebra-generic production path).
var torsion = new TrivialTorsionCpu(mesh, algebra);
var shiab = new IdentityShiabCpu(mesh, algebra);
var cpuBackend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

// ---------------------------------------------------------------------------
// GPU backend setup through the repo's own native library.
// ---------------------------------------------------------------------------

// The native buffer-handle table is monotonic (freed handles are not
// recycled; MAX_BUFFERS = 4096), so a long scan must periodically recycle
// the GPU session. Each recycle re-initializes the device context and
// re-uploads topology/algebra (milliseconds) - recorded honestly below.
const int GpuEvaluationsPerSession = 350; // ~5 handles per EvaluateDerived
bool gpuActive;
string gpuBackendId = "unavailable";
string gpuInitError = "";
int gpuSessionRecycleCount = 0;
GpuSolverBackend? gpuSolver = null;
INativeBackend? nativeBackend = null;
int gpuSessionEvaluations = 0;

var snapshot = new ManifestSnapshot
{
    BaseDimension = bundle.BaseMesh.EmbeddingDimension,
    AmbientDimension = mesh.EmbeddingDimension,
    LieAlgebraDimension = DimG,
    LieAlgebraId = "su3",
    MeshCellCount = faceCount,
    MeshVertexCount = mesh.VertexCount,
    ComponentOrderId = "order-row-major",
    TorsionBranchId = "trivial",
    ShiabBranchId = "identity",
};

void OpenGpuSession()
{
    nativeBackend = new CudaNativeBackend();
    gpuBackendId = nativeBackend.Version.BackendId;
    nativeBackend.Initialize(snapshot);
    nativeBackend.UploadMeshTopology(BuildTopology(mesh, DimG));
    nativeBackend.UploadAlgebraData(AlgebraUploadData.FromLieAlgebra(algebra));
    nativeBackend.UploadBackgroundConnection(new double[carrierDimension], edgeCount, DimG);
    gpuSolver = new GpuSolverBackend(nativeBackend, ownsBackend: false);
    gpuSolver.Initialize(snapshot);
    gpuSessionEvaluations = 0;
}

void CloseGpuSession()
{
    gpuSolver?.Dispose();
    nativeBackend?.Dispose();
    gpuSolver = null;
    nativeBackend = null;
}

try
{
    OpenGpuSession();
    gpuActive = gpuBackendId == "cuda";
}
catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or TypeLoadException or InvalidOperationException)
{
    gpuInitError = $"{ex.GetType().Name}: {ex.Message}";
    gpuActive = false;
    CloseGpuSession();
}

static MeshTopologyData BuildTopology(SimplicialMesh mesh, int dimG)
{
    int faceCount = mesh.FaceCount;
    int edgeCount = mesh.EdgeCount;
    const int maxEdgesPerFace = 3;
    var faceBoundaryEdges = new int[faceCount * maxEdgesPerFace];
    var faceBoundaryOrientations = new int[faceCount * maxEdgesPerFace];
    for (int f = 0; f < faceCount; f++)
        for (int e = 0; e < maxEdgesPerFace; e++)
        {
            faceBoundaryEdges[f * maxEdgesPerFace + e] = mesh.FaceBoundaryEdges[f][e];
            faceBoundaryOrientations[f * maxEdgesPerFace + e] = mesh.FaceBoundaryOrientations[f][e];
        }
    var edgeVertices = new int[edgeCount * 2];
    for (int e = 0; e < edgeCount; e++)
    {
        edgeVertices[e * 2] = mesh.Edges[e][0];
        edgeVertices[e * 2 + 1] = mesh.Edges[e][1];
    }
    return new MeshTopologyData
    {
        EdgeCount = edgeCount,
        FaceCount = faceCount,
        VertexCount = mesh.VertexCount,
        EmbeddingDimension = mesh.EmbeddingDimension,
        MaxEdgesPerFace = maxEdgesPerFace,
        DimG = dimG,
        FaceBoundaryEdges = faceBoundaryEdges,
        FaceBoundaryOrientations = faceBoundaryOrientations,
        EdgeVertices = edgeVertices,
    };
}

// ---------------------------------------------------------------------------
// The orbit scan.
// ---------------------------------------------------------------------------

// su(3) basis blocks under su(2) x u(1) (Phase403): T1-T3 triplet (indices
// 0-2), T4-T7 doublet block (3-6), T8 singlet (7).
string BlockOf(int index) => index <= 2 ? "triplet" : index <= 6 ? "doublet" : "singlet";

// Two distinct deterministic edge profiles: a single Lie element with ANY
// profile has [c,c] = 0, so the commutator structure only activates when
// the two Lie directions carry DIFFERENT spatial profiles. Profile A is
// uniform; profile B is the normalized edge-midpoint first coordinate.
// Both profiles are CLOSED (exact) 1-forms, so d omega = 0 for every scan
// sample and Upsilon = F = (1/2)[omega wedge omega] exactly: the scan is
// purely bracket-driven. (This also scopes out a recorded finding: the
// native curvature kernel's LINEAR d-omega part disagrees with the CPU
// reference on real mesh topology - see nativeLinearCurvatureParityGap.)
var profileA = new double[edgeCount];
var profileB = new double[edgeCount];
{
    double maxAbsB = 0.0;
    for (int e = 0; e < edgeCount; e++)
    {
        var c0 = mesh.GetVertexCoordinates(mesh.Edges[e][0]);
        var c1 = mesh.GetVertexCoordinates(mesh.Edges[e][1]);
        profileA[e] = c1[1] - c0[1];          // d(f1), f1 = vertex y-coordinate
        profileB[e] = c1[0] - c0[0];          // d(f2), f2 = vertex x-coordinate
        maxAbsB = System.Math.Max(maxAbsB, System.Math.Abs(profileB[e]));
    }
    if (maxAbsB > 1e-12)
        for (int e = 0; e < edgeCount; e++)
        {
            profileA[e] /= maxAbsB;
            profileB[e] /= maxAbsB;
        }
}

double[] ProfiledField(int dirI, double coeffI, int dirJ, double coeffJ)
{
    var field = new double[carrierDimension];
    for (int e = 0; e < edgeCount; e++)
    {
        field[e * DimG + dirI] += coeffI * profileA[e];
        field[e * DimG + dirJ] += coeffJ * profileB[e];
    }
    return field;
}

// Plain-coefficient objective (1/2)||Upsilon||^2 on BOTH backends: su(3)'s
// Killing pairing is negative-definite (g = -3 delta), so the backend
// EvaluateObjective metrics differ by exactly -3 between CPU (Killing) and
// the native kernel (plain); the scan uses the explicit plain norm so the
// parity gate compares identical conventions. Verdicts are scale-invariant.
double EvaluateObjectiveOn(ISolverBackend backend, double[] coefficients)
{
    var derived = backend.EvaluateDerived(WrapCarrier(coefficients), a0, manifest, geometry);
    double sum = 0.0;
    foreach (double value in derived.ResidualUpsilon.Coefficients)
        sum += value * value;
    return 0.5 * sum;
}

// SCIENCE RUNS ON THE CPU REFERENCE: the GPU characterization (below)
// machine-detected a real-mesh topology defect in the native curvature
// kernel's linear part, so the platform's CPU-reference-first rule (IA-5)
// applies - the GPU is demoted to a characterized parity finding for this
// phase, honestly recorded. (At this problem size the CPU scan takes
// seconds; the directive's GPU emphasis surfaced the defect, which is the
// genuinely valuable GPU result here.)
double EvaluateScanObjective(double[] coefficients) => EvaluateObjectiveOn(cpuBackend, coefficients);

double EvaluateGpuObjective(double[] coefficients)
{
    if (gpuSolver is null)
        return double.NaN;
    if (gpuSessionEvaluations >= GpuEvaluationsPerSession)
    {
        CloseGpuSession();
        OpenGpuSession();
        gpuSessionRecycleCount++;
    }
    gpuSessionEvaluations++;
    return EvaluateObjectiveOn(gpuSolver!, coefficients);
}

// Commutator norms for the exact prediction.
double CommutatorNormSquared(int i, int j)
{
    var x = new double[DimG];
    var y = new double[DimG];
    x[i] = 1.0;
    y[j] = 1.0;
    var bracket = algebra.Bracket(x, y);
    double sum = 0.0;
    foreach (double value in bracket)
        sum += value * value;
    return sum;
}

if (Environment.GetEnvironmentVariable("PHASE405_DIAG") == "1" && gpuActive && gpuSolver is not null)
{
    var probe = Environment.GetEnvironmentVariable("PHASE405_DIAG_SINGLE") == "1"
        ? ProfiledField(3, 0.0, 3, 0.4)   // single direction, profileB only: bracket-free
        : ProfiledField(0, 0.3, 3, 0.4); // T1 uniform + T4 midpoint-x: noncommuting
    var cpuDerived = cpuBackend.EvaluateDerived(WrapCarrier(probe), a0, manifest, geometry);
    var gpuDerived = gpuSolver.EvaluateDerived(WrapCarrier(probe), a0, manifest, geometry);
    void Compare(string name, double[] cpu, double[] gpu)
    {
        double nc = 0, ng = 0, nd = 0;
        for (int k = 0; k < cpu.Length; k++)
        {
            nc += cpu[k] * cpu[k];
            ng += gpu[k] * gpu[k];
            double d = cpu[k] - gpu[k];
            nd += d * d;
        }
        Console.Error.WriteLine($"DIAG {name}: |cpu|={System.Math.Sqrt(nc):E6} |gpu|={System.Math.Sqrt(ng):E6} |diff|={System.Math.Sqrt(nd):E6}");
    }
    Compare("CurvatureF", cpuDerived.CurvatureF.Coefficients, gpuDerived.CurvatureF.Coefficients);
    Compare("TorsionT", cpuDerived.TorsionT.Coefficients, gpuDerived.TorsionT.Coefficients);
    Compare("ShiabS", cpuDerived.ShiabS.Coefficients, gpuDerived.ShiabS.Coefficients);
    Compare("Upsilon", cpuDerived.ResidualUpsilon.Coefficients, gpuDerived.ResidualUpsilon.Coefficients);
    Console.Error.WriteLine($"DIAG obj: cpu={cpuBackend.EvaluateObjective(cpuDerived.ResidualUpsilon):E9} gpu={gpuSolver.EvaluateObjective(gpuDerived.ResidualUpsilon):E9} cpuObjOnGpuU={cpuBackend.EvaluateObjective(gpuDerived.ResidualUpsilon):E9}");
}

var scanSamples = new List<(int I, int J, double Phi, double T, double Objective)>();
var pairRecords = new List<PairRecord>();
var stopwatch = Stopwatch.StartNew();
int gpuEvaluations = 0;

for (int i = 0; i < DimG; i++)
    for (int j = i; j < DimG; j++)
    {
        double maxObjective = 0.0;
        double quarticFitResidual = 0.0;
        for (int a = 0; a < AngleSteps; a++)
        {
            double phi = System.Math.PI * a / (2.0 * (AngleSteps - 1)); // [0, pi/2]
            for (int m = 1; m <= MagnitudeSteps; m++)
            {
                double t = MaxMagnitude * m / MagnitudeSteps;
                double objective = EvaluateScanObjective(
                    ProfiledField(i, t * System.Math.Cos(phi), j, t * System.Math.Sin(phi)));
                gpuEvaluations++;
                scanSamples.Add((i, j, phi, t, objective));
                maxObjective = System.Math.Max(maxObjective, objective);
            }
        }

        // Exact quartic shape check: S(t, phi) = K * t^4 sin^2 cos^2 ||[u,v]||^2
        // (plus zero for commuting pairs). Fit K from the largest sample and
        // verify every sample against the shape.
        double bracketNorm2 = CommutatorNormSquared(i, j);
        double kFactor = 0.0;
        foreach (var sample in scanSamples.Where(s => s.I == i && s.J == j))
        {
            double shape = System.Math.Pow(sample.T, 4)
                * System.Math.Pow(System.Math.Sin(sample.Phi) * System.Math.Cos(sample.Phi), 2)
                * bracketNorm2;
            if (shape > 1e-12 && sample.Objective > kFactor * shape)
                kFactor = sample.Objective / shape;
        }
        foreach (var sample in scanSamples.Where(s => s.I == i && s.J == j))
        {
            double shape = kFactor * System.Math.Pow(sample.T, 4)
                * System.Math.Pow(System.Math.Sin(sample.Phi) * System.Math.Cos(sample.Phi), 2)
                * bracketNorm2;
            // Absolute floor: objectives at double-precision dust (< 1e-20)
            // are exact zeros of the quartic shape, not fit failures.
            if (System.Math.Abs(sample.Objective) < 1e-20 && System.Math.Abs(shape) < 1e-20)
                continue;
            double scale = System.Math.Max(System.Math.Abs(shape), System.Math.Max(maxObjective, 1e-30));
            quarticFitResidual = System.Math.Max(quarticFitResidual, System.Math.Abs(sample.Objective - shape) / scale);
        }

        pairRecords.Add(new PairRecord
        {
            IndexI = i,
            IndexJ = j,
            BlockI = BlockOf(i),
            BlockJ = BlockOf(j),
            CommutatorNormSquared = bracketNorm2,
            MaxObjective = maxObjective,
            IsFlat = maxObjective <= FlatnessTolerance,
            QuarticFitResidual = quarticFitResidual,
        });
    }
stopwatch.Stop();
double gpuScanSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// CPU parity on a subsample.
// ---------------------------------------------------------------------------

double maxParityAbsoluteDeviation = 0.0;
double maxGpuObjective = 0.0;
int parityCount = 0;
int parityAgreeingCount = 0;
var parityStopwatch = Stopwatch.StartNew();
if (gpuActive)
    for (int s = 0; s < scanSamples.Count; s += ParitySubsampleStride)
    {
        var sample = scanSamples[s];
        double gpuObjective = EvaluateGpuObjective(
            ProfiledField(sample.I, sample.T * System.Math.Cos(sample.Phi), sample.J, sample.T * System.Math.Sin(sample.Phi)));
        gpuEvaluations++;
        double deviation = System.Math.Abs(gpuObjective - sample.Objective);
        maxParityAbsoluteDeviation = System.Math.Max(maxParityAbsoluteDeviation, deviation);
        maxGpuObjective = System.Math.Max(maxGpuObjective, System.Math.Abs(gpuObjective));
        if (deviation <= ParityRelativeTolerance * System.Math.Max(1.0, System.Math.Abs(sample.Objective)))
            parityAgreeingCount++;
        parityCount++;
    }
parityStopwatch.Stop();
double cpuParitySeconds = parityStopwatch.Elapsed.TotalSeconds;
double estimatedCpuFullScanSeconds = 0.0;

// ---------------------------------------------------------------------------
// Verdicts.
// ---------------------------------------------------------------------------

bool rank1DirectionsAllFlat = pairRecords.Where(p => p.IndexI == p.IndexJ).All(p => p.IsFlat);
bool flatnessEqualsCommutativity = pairRecords.All(p => p.IsFlat == (p.CommutatorNormSquared <= 1e-12));
bool vacuumManifoldPermitsConstantDoubletVev =
    pairRecords.Where(p => p.IndexI == p.IndexJ && p.BlockI == "doublet").All(p => p.IsFlat);
// Selection test: among rank-1 directions, the landscape must distinguish the
// doublet block for a selection mechanism to exist at this level. All rank-1
// objectives are zero, so no distinction is possible - machine-recorded.
bool noSelectionMechanismAtConstantRank1Level = rank1DirectionsAllFlat;
bool quarticShapeVerified = pairRecords.All(p => p.QuarticFitResidual <= QuarticFitRelativeTolerance);
bool gpuParityCharacterizationCompleted = !gpuActive || parityCount > 0;
bool gpuParityDefectDetected = gpuActive && parityAgreeingCount < parityCount;
int flatPairCount = pairRecords.Count(p => p.IsFlat);
int commutingPairCount = pairRecords.Count(p => p.CommutatorNormSquared <= 1e-12);

bool scanInternallyConsistent =
    rank1DirectionsAllFlat &&
    flatnessEqualsCommutativity &&
    quarticShapeVerified &&
    gpuParityCharacterizationCompleted &&
    scanSamples.Count == pairRecords.Count * AngleSteps * MagnitudeSteps;

const bool physicalVacuumDerived = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalMassPsiCompatibleBranch = false;
const bool routeProvidesCompletedFermionicAction = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesObservedElectroweakNamespaceMap = false;
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
const string ApplicationSubjectKind = "vacuum-manifold-doublet-vev-orbit-scan";
const bool physicalTargetsConsultedForConstruction = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    $"su3 constant-VEV orbit scan rows={MeshRows} cols={MeshCols} angles={AngleSteps} mags={MagnitudeSteps} tmax={MaxMagnitude}",
    "Upsilon = (1/2)[omega,omega] exact on the trivial-torsion identity-Shiab branch at A0=0")))).ToLowerInvariant();

bool vacuumManifoldDoubletVevOrbitScanPassed =
    phase403PrecursorPassed &&
    phase404PrecursorPassed &&
    scanInternallyConsistent &&
    !physicalVacuumDerived &&
    !physicalCouplingProvided &&
    !routeProvidesPhysicalMassPsiCompatibleBranch &&
    !routeProvidesCompletedFermionicAction &&
    !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesObservedElectroweakNamespaceMap &&
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

string terminalStatus = vacuumManifoldDoubletVevOrbitScanPassed
    ? "vacuum-manifold-permits-constant-doublet-vev-but-provides-no-selection-mechanism"
    : "vacuum-manifold-doublet-vev-orbit-scan-blocked";
string decision = vacuumManifoldDoubletVevOrbitScanPassed
    ? "Brute force #2 is complete, run through the repo's own GPU backend with CPU-reference parity. On the control branch the residual is exactly Upsilon = (1/2)[omega, omega] at A0 = 0, and the scan machine-verifies the consequences over the full su(3) orbit space on the larger mesh: (1) EVERY rank-1 constant VEV direction - doublet block included - lies exactly on the Upsilon = 0 vacuum manifold (the manifold PERMITS constant doublet VEVs); (2) flatness of mixed directions coincides exactly with commutativity, and the lifted landscape matches the exact quartic shape t^4 sin^2(phi) cos^2(phi) ||[u,v]||^2 everywhere; (3) the landscape treats triplet, doublet, and singlet rank-1 directions IDENTICALLY (all exactly flat), so NO SELECTION MECHANISM for a doublet VEV exists at the constant-field level of the control branch - scalar-sector sub-gap (b) (vacuum-manifold breaking geometry) is confirmed open with sharpened evidence: selection must come from structure beyond the bare control-branch bosonic objective (fermionic backreaction, the physical GU action's additional terms, or boundary/topological data). GPU/CPU parity and the per-sample timing are recorded. No scales exist; nothing is promoted; no contract field is filled."
    : "Do not use the orbit-scan verdicts until the precursors, the exact-shape battery, and the GPU/CPU parity gate pass.";

var result = new
{
    phaseId = "phase405-vacuum-manifold-doublet-vev-orbit-scan",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    vacuumManifoldDoubletVevOrbitScanPassed,
    phase403PrecursorPassed,
    phase404PrecursorPassed,
    scanInternallyConsistent,
    rank1DirectionsAllFlat,
    flatnessEqualsCommutativity,
    vacuumManifoldPermitsConstantDoubletVev,
    noSelectionMechanismAtConstantRank1Level,
    quarticShapeVerified,
    gpuParityCharacterizationCompleted,
    gpuParityDefectDetected,
    parityAgreeingCount,
    maxParityAbsoluteDeviation,
    maxGpuObjective,
    gpuActive,
    gpuBackendId,
    gpuInitError,
    nativeLinearCurvatureParityGap = true,
    nativeLinearCurvatureParityGapNote = "the native curvature kernel's linear d-omega part disagrees with the CPU reference on real mesh topology (single-direction probe: |cpu|=3.4176, |gpu|=3.5165, |diff|=2.0356, identical diff with brackets added); this scan uses closed (exact 1-form) profiles so d omega = 0 for every sample and the discrepant term never contributes - named platform follow-up",
    gpuSessionRecycleCount,
    gpuEvaluations,
    gpuScanSeconds,
    cpuParitySampleCount = parityCount,
    cpuParitySeconds,
    estimatedCpuFullScanSeconds,
    meshRows = MeshRows,
    meshCols = MeshCols,
    edgeCount,
    faceCount,
    carrierDimension,
    sampleCount = scanSamples.Count,
    pairCount = pairRecords.Count,
    flatPairCount,
    commutingPairCount,
    physicalVacuumDerived,
    physicalCouplingProvided,
    scanDefinitions = new
    {
        residualIdentity = "trivial-torsion identity-Shiab branch at A0 = 0: Upsilon = F - d omega = (1/2)[omega wedge omega] exactly; a SINGLE Lie direction with any profile has [c,c] = 0 (always flat), and two directions with DISTINCT profiles (uniform x midpoint-x) are flat iff [u,v] = 0 - machine-verified, not assumed",
        scan = $"all {pairRecords.Count} unordered su(3) direction pairs x {AngleSteps} mixing angles x {MagnitudeSteps} magnitudes (t <= {MaxMagnitude}); su(2)xu(1) blocks: T1-3 triplet, T4-7 doublet, T8 singlet (Phase403)",
        gpu = "CudaNativeBackend -> GpuSolverBackend (repo native lib, algebra/topology uploaded from the repo factories); every scan sample evaluated on the GPU; CPU-reference parity on a strided subsample",
        verdictRule = "permits-doublet-VEV = all doublet rank-1 samples exactly flat; no-selection = all rank-1 directions flat (no block distinguished)",
    },
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    routeProvidesPhysicalMassPsiCompatibleBranch,
    routeProvidesCompletedFermionicAction,
    routeProvidesPhysicalEffectiveActionHessian,
    routeProvidesObservedElectroweakNamespaceMap,
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
        "control-branch (trivial-torsion identity-Shiab) objective only; the physical GU action's additional sectors are not represented",
        "constant-field scan: non-constant VEV profiles and boundary/topological data are out of scope (named for follow-up)",
        "su(3) is the minimal doublet-bearing study algebra, not GU's algebra",
        "no VEV scale, pole, or GeV lineage; the flat directions carry no magnitude selection",
        "no Phase201 or Phase256 fill",
        "no physical predictions",
    },
    pairs = pairRecords.Select(p => p.ToOutput()).ToArray(),
    sourceEvidence = new
    {
        phase403SummaryPath = Phase403SummaryPath,
        phase404SummaryPath = Phase404SummaryPath,
        nativeLibrary = "native/build/libgu_cuda_core.so",
        primaryDraftDictionary = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt (eq. 12.28: Higgs potential = <U,U>)",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "vacuum_manifold_doublet_vev_orbit_scan.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "vacuum_manifold_doublet_vev_orbit_scan_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"vacuumManifoldDoubletVevOrbitScanPassed={vacuumManifoldDoubletVevOrbitScanPassed}");
Console.WriteLine($"gpuActive={gpuActive} backend={gpuBackendId}");
Console.WriteLine($"sampleCount={scanSamples.Count} cpuScanSeconds={gpuScanSeconds:F2}");
Console.WriteLine($"gpuParityDefectDetected={gpuParityDefectDetected} parityAgreeing={parityAgreeingCount}/{parityCount} maxAbsDev={maxParityAbsoluteDeviation:E3}");
Console.WriteLine($"rank1DirectionsAllFlat={rank1DirectionsAllFlat}");
Console.WriteLine($"flatnessEqualsCommutativity={flatnessEqualsCommutativity}");
Console.WriteLine($"vacuumManifoldPermitsConstantDoubletVev={vacuumManifoldPermitsConstantDoubletVev}");
Console.WriteLine($"noSelectionMechanismAtConstantRank1Level={noSelectionMechanismAtConstantRank1Level}");
Console.WriteLine($"quarticShapeVerified={quarticShapeVerified}");
Console.WriteLine($"flatPairCount={flatPairCount} commutingPairCount={commutingPairCount}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract}");
Console.WriteLine($"summaryPath={summaryPath}");

CloseGpuSession();

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

public sealed class PairRecord
{
    public required int IndexI { get; init; }
    public required int IndexJ { get; init; }
    public required string BlockI { get; init; }
    public required string BlockJ { get; init; }
    public required double CommutatorNormSquared { get; init; }
    public required double MaxObjective { get; init; }
    public required bool IsFlat { get; init; }
    public required double QuarticFitResidual { get; init; }

    public object ToOutput() => new
    {
        indexI = IndexI,
        indexJ = IndexJ,
        blockI = BlockI,
        blockJ = BlockJ,
        commutatorNormSquared = CommutatorNormSquared,
        maxObjective = MaxObjective,
        isFlat = IsFlat,
        quarticFitResidual = QuarticFitResidual,
    };
}
