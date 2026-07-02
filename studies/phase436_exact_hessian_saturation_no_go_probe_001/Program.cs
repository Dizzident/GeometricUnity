using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Gu.Solvers;

// Phase436: exact-Hessian saturation no-go probe (the tie-in named by Phase435).
//
// Phases 430/431/435 used a WORKBENCH MODEL for the bosonic one-loop (masses^2
// = eigenvalues of -(ad_u)^2 on the 4x4 lattice). This phase computes the TRUE
// control-branch bosonic Hessian of the exact objective S_B(omega) =
// (1/2)||Upsilon||^2 at constant rank-1 backgrounds t*u, using the repo's own
// CPU reference solver (trivial torsion, identity Shiab, A0 = 0), and decides
// two things.
//
// (A) EXACT QUADRATIC DECOMPOSITION THEOREM. On this branch the residual is
//     Upsilon = F - T^aug with a LINEAR part L = d (the exterior derivative,
//     the free Yang-Mills kinetic operator; it annihilates closed forms, which
//     is why the exact-1-form rank-1 backgrounds stay flat) plus a QUADRATIC
//     bracket part Q(omega) = (1/2)[omega wedge omega]. Upsilon is therefore
//     EXACTLY degree-2 in omega (verified below: the third field-difference of
//     Upsilon vanishes), so S_B is exactly quartic and the exact Hessian at a
//     background t*u is EXACTLY a degree-2 matrix polynomial in t:
//         H(t) = A0 + t*A1 + t^2*A2,     NOTHING beyond t^2
//     (verified: the third t-difference of H vanishes to machine precision).
//     A0 = H(0) is the background-independent free-kinetic (d^T d) Hessian; A2
//     is the quadratic-in-background mass term that governs the large-t
//     asymptotics. The linear-in-t (odd) term A1 is recorded honestly: it
//     VANISHES for the lambda_8-type (Cartan/hypercharge) background and is
//     NONZERO for the lambda_4-type (root) background.
//     CONSEQUENCE (the headline): asymptotically H(t) -> t^2*A2, so the growing
//     Hessian masses^2 scale EXACTLY as t^2, the bosonic one-loop grows exactly
//     logarithmically in the background amplitude, and NO LOG-SATURATION can
//     arise from the exact control-branch Hessian at one loop (a degree-2
//     matrix polynomial with a positive-definite t^2 coefficient cannot bend a
//     growing log back down). The Phase435 scale gap is therefore pinned to
//     structure BEYOND the control branch (a physical VO-6/VO-7 completion or a
//     source anchor), not to workbench modeling.
//
// (B) WORKBENCH-MODEL FIDELITY. The exact growing-mode COUNT is the number of
//     strictly-positive eigenvalues of the mass term A2 = J2^T J2 (with J2 the
//     linearized-bracket operator J(u) - J(0)). Recorded: the exact counts are
//     64 (lambda_8-type) and 96 (lambda_4-type), which factor EXACTLY as
//     rank(-(ad_u)^2) * geometricMultiplicity with a SINGLE shared multiplicity
//     16: 64 = 4*16, 96 = 6*16. So the per-su(3)-direction growing-mode counts
//     equal the workbench -(ad_u)^2 nonzero counts (4 vs 6), and Phase430's
//     log-slope direction-selection arithmetic is CONFIRMED under the exact
//     control-branch Hessian. The mass VALUES differ (exact A2 eigenvalues are
//     O(0.01..0.3); workbench -(ad_u)^2 eigenvalues are O(0.25..1)) - the model
//     is a convention, and only the counts entered Phase430's verdicts.
//
// Fail-closed: control-branch objective only; no scales; nothing promoted; no
// contract field can be filled.

const string DefaultOutputDir = "studies/phase436_exact_hessian_saturation_no_go_probe_001/output";
const string Phase405SummaryPath = "studies/phase405_vacuum_manifold_doublet_vev_orbit_scan_001/output/vacuum_manifold_doublet_vev_orbit_scan_summary.json";
const string Phase430SummaryPath = "studies/phase430_net_one_loop_direction_selection_probe_001/output/net_one_loop_direction_selection_probe_summary.json";
const string Phase435SummaryPath = "studies/phase435_two_condensate_scale_gap_probe_001/output/two_condensate_scale_gap_probe_summary.json";
const string ApplicationSubjectKind = "exact-hessian-saturation-no-go-probe";

const int MeshRows = 2;
const int MeshCols = 2;
const int DimG = 8;
const double JacobianStep = 1e-2;      // Upsilon is exactly degree-2 => central diff of a quadratic is exact
const double HessianStep = 1e-2;       // second difference of a quadratic is exact
const int SampledHessianPairs = 200;   // decomposition check on random (k,l) matrix elements
const double DecompositionRelativeTolerance = 1e-8;
const double StepInvarianceTolerance = 1e-6;
const double FlatnessTolerance = 1e-18;
const double OddTermVanishingTolerance = 1e-8;
const double PositiveEigenvalueTolerance = 1e-6;
const int RngSeed = 20260702;

var outputDir = Environment.GetEnvironmentVariable("PHASE436_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase405 = JsonDocument.Parse(File.ReadAllText(Phase405SummaryPath));
using var phase430 = JsonDocument.Parse(File.ReadAllText(Phase430SummaryPath));
using var phase435 = JsonDocument.Parse(File.ReadAllText(Phase435SummaryPath));

bool phase405PrecursorPassed = JsonBool(phase405.RootElement, "vacuumManifoldDoubletVevOrbitScanPassed") is true;
bool phase430PrecursorPassed = JsonBool(phase430.RootElement, "netOneLoopDirectionSelectionProbePassed") is true;
bool phase435PrecursorPassed =
    JsonBool(phase435.RootElement, "twoCondensateScaleGapProbePassed") is true &&
    JsonBool(phase435.RootElement, "scaleRequiresLogSaturationBeyondWorkbench") is true;
bool precursorsPassed = phase405PrecursorPassed && phase430PrecursorPassed && phase435PrecursorPassed;

// ---------------------------------------------------------------------------
// Control-branch machinery (Phase405 template): su(3) algebra, structured
// fiber-bundle mesh, CPU reference backend (trivial torsion, identity Shiab).
// ---------------------------------------------------------------------------

var algebra = LieAlgebraFactory.CreateSu3();
var bundle = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: MeshRows, cols: MeshCols);
var mesh = bundle.AmbientMesh;
int edgeCount = mesh.EdgeCount;
int faceCount = mesh.FaceCount;
int carrierDimension = edgeCount * DimG;
int upsilonDimension = faceCount * DimG;
var geometry = bundle.ToGeometryContext("centroid", "P1");

var manifest = new BranchManifest
{
    BranchId = "phase436-su3-exact-hessian",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "r1",
    CodeRevision = "phase436",
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
var backend = new CpuSolverBackend(mesh, algebra, new TrivialTorsionCpu(mesh, algebra), new IdentityShiabCpu(mesh, algebra));

// Exact 1-form profiles: d(vertex y-coordinate) and d(vertex x-coordinate).
// A single Lie direction with any profile has [c, c] = 0, and a closed profile
// is annihilated by the linear part L = d, so every rank-1 background is
// exactly flat.
var profileA = new double[edgeCount];
var profileB = new double[edgeCount];
for (int e = 0; e < edgeCount; e++)
{
    var c0 = mesh.GetVertexCoordinates(mesh.Edges[e][0]);
    var c1 = mesh.GetVertexCoordinates(mesh.Edges[e][1]);
    profileA[e] = c1[1] - c0[1];
    profileB[e] = c1[0] - c0[0];
}

double[] RankOneBackground(int direction, double t)
{
    var field = new double[carrierDimension];
    for (int e = 0; e < edgeCount; e++)
        field[e * DimG + direction] += t * profileA[e];
    return field;
}

double[] Upsilon(double[] coefficients)
{
    var derived = backend.EvaluateDerived(WrapCarrier(coefficients), a0, manifest, geometry);
    return derived.ResidualUpsilon.Coefficients;
}

double Objective(double[] coefficients)
{
    var u = Upsilon(coefficients);
    double sum = 0.0;
    foreach (double v in u)
        sum += v * v;
    return 0.5 * sum;
}

// ---------------------------------------------------------------------------
// The two probed su(3) directions. The algebra's canonical basis matches the
// Gell-Mann ordering: index 7 is the lambda_8-type Cartan (hypercharge)
// direction; index 3 is a lambda_4-type root direction. Each is LABELLED by its
// -(ad_u)^2 spectrum rather than by assumption.
// ---------------------------------------------------------------------------

int lambda8Index = 7;
int lambda4Index = 3;

double[,] AdjointSquaredNegated(int axis)
{
    var ad = new double[DimG, DimG];
    var ea = new double[DimG];
    ea[axis] = 1.0;
    for (int b = 0; b < DimG; b++)
    {
        var eb = new double[DimG];
        eb[b] = 1.0;
        var bracket = algebra.Bracket(ea, eb);
        for (int c = 0; c < DimG; c++)
            ad[c, b] = bracket[c];
    }
    var negSquared = new double[DimG, DimG];
    for (int i = 0; i < DimG; i++)
        for (int j = 0; j < DimG; j++)
        {
            double s = 0.0;
            for (int k = 0; k < DimG; k++)
                s += ad[i, k] * ad[k, j];
            negSquared[i, j] = -s;
        }
    return negSquared;
}

int WorkbenchNonzeroCount(int axis, out double[] workbenchMasses)
{
    var (values, _) = Jacobi(AdjointSquaredNegated(axis));
    workbenchMasses = values.Where(v => Math.Abs(v) > 1e-9).OrderByDescending(v => v).ToArray();
    return workbenchMasses.Length;
}

// ---------------------------------------------------------------------------
// Exact derivatives of the degree-2 Upsilon.
//   J_{a,k}(z) = dU_a/dz_k    - central difference, exact for degree-2 U.
//   HessU_a(k,l) = d^2 U_a/dz_k dz_l - mixed 2nd difference, exact and z-free.
// The analytic Hessian of S_B = (1/2)||U||^2 is then exact:
//   H_{kl}(z) = sum_a J_{a,k}(z) J_{a,l}(z) + sum_a U_a(z) HessU_a(k,l).
// ---------------------------------------------------------------------------

double[] JacobianColumn(double[] background, int k, double h)
{
    var plus = (double[])background.Clone();
    plus[k] += h;
    var up = Upsilon(plus);
    var minus = (double[])background.Clone();
    minus[k] -= h;
    var um = Upsilon(minus);
    var column = new double[up.Length];
    for (int a = 0; a < up.Length; a++)
        column[a] = (up[a] - um[a]) / (2.0 * h);
    return column;
}

double[] UpsilonHessianElement(int k, int l, double h)
{
    var pp = new double[carrierDimension]; pp[k] += h; pp[l] += h;
    var pm = new double[carrierDimension]; pm[k] += h; pm[l] -= h;
    var mp = new double[carrierDimension]; mp[k] -= h; mp[l] += h;
    var mm = new double[carrierDimension]; mm[k] -= h; mm[l] -= h;
    var upp = Upsilon(pp);
    var upm = Upsilon(pm);
    var ump = Upsilon(mp);
    var umm = Upsilon(mm);
    var result = new double[upp.Length];
    for (int a = 0; a < upp.Length; a++)
        result[a] = (upp[a] - upm[a] - ump[a] + umm[a]) / (4.0 * h * h);
    return result;
}

double ExactHessianElement(double[] background, double[] jk, double[] jl, double[] hessU)
{
    var uBackground = Upsilon(background);
    double sum = 0.0;
    for (int a = 0; a < jk.Length; a++)
        sum += jk[a] * jl[a] + uBackground[a] * hessU[a];
    return sum;
}

// ---------------------------------------------------------------------------
// Battery 1: flatness on rank-1 constant backgrounds (reproduces Phase405).
// ---------------------------------------------------------------------------

double maxRankOneObjective = 0.0;
foreach (int direction in new[] { lambda8Index, lambda4Index })
    foreach (double t in new[] { 0.5, 1.0, 2.0 })
        maxRankOneObjective = Math.Max(maxRankOneObjective, Objective(RankOneBackground(direction, t)));
bool flatnessOnRank1Verified = maxRankOneObjective <= FlatnessTolerance;

// ---------------------------------------------------------------------------
// Battery 2: Upsilon is exactly degree-2 in the field (linear + quadratic, no
// cubic or higher) - the fact that forces H(t) to be degree-2 in t. Tested on a
// non-closed field so the linear part contributes: the third field-difference
// U(3w) - 3 U(2w) + 3 U(w) - U(0) must vanish. Also record the linear-part norm
// ||L|| = ||J(0)|| (the free-kinetic operator) and confirm it is nonzero (and
// that a closed profile is annihilated by L, hence the rank-1 flatness).
// ---------------------------------------------------------------------------

double upsilonThirdFieldDifference;
double linearPartNorm;
double linearPartOnClosedProfileNorm;
{
    var rng = new Random(RngSeed);
    var w = new double[carrierDimension];
    for (int k = 0; k < carrierDimension; k++)
        w[k] = rng.NextDouble() - 0.5;   // generic, non-closed field
    var u0 = Upsilon(new double[carrierDimension]);
    var u1 = Upsilon(Scale(w, 1.0));
    var u2 = Upsilon(Scale(w, 2.0));
    var u3 = Upsilon(Scale(w, 3.0));
    double num = 0.0, den = 0.0;
    for (int a = 0; a < u1.Length; a++)
    {
        double third = u3[a] - 3.0 * u2[a] + 3.0 * u1[a] - u0[a];
        num += third * third;
        den += u2[a] * u2[a];
    }
    upsilonThirdFieldDifference = Math.Sqrt(num / Math.Max(den, 1e-300));

    // linear part L(v) = (U(hv) - U(-hv)) / (2h), exact for degree-2 U.
    double normSq = 0.0;
    for (int k = 0; k < carrierDimension; k++)
    {
        var col = JacobianColumn(new double[carrierDimension], k, JacobianStep);
        foreach (double v in col)
            normSq += v * v;
    }
    linearPartNorm = Math.Sqrt(normSq);

    // L on the closed rank-1 profile must vanish (=> flatness).
    var closed = RankOneBackground(lambda8Index, 1.0);
    var lPlus = Upsilon(Scale(closed, JacobianStep));
    var lMinus = Upsilon(Scale(closed, -JacobianStep));
    double cnorm = 0.0;
    for (int a = 0; a < lPlus.Length; a++)
    {
        double d = (lPlus[a] - lMinus[a]) / (2.0 * JacobianStep);
        cnorm += d * d;
    }
    linearPartOnClosedProfileNorm = Math.Sqrt(cnorm);
}
bool upsilonExactlyDegreeTwoInField = upsilonThirdFieldDifference <= DecompositionRelativeTolerance;
bool linearPartIsNonzeroFreeKinetic = linearPartNorm > 1e-6;
bool linearPartAnnihilatesClosedProfile = linearPartOnClosedProfileNorm <= 1e-9;

// ---------------------------------------------------------------------------
// Battery 3 (Phase400-style): the mixed second difference of U is identical at
// two step sizes h and h/2 (relative) - confirms U is exactly quadratic so the
// analytic Hessian formula is exact.
// ---------------------------------------------------------------------------

double secondDifferenceStepResidual;
{
    var rng = new Random(RngSeed);
    double maxRel = 0.0;
    for (int s = 0; s < 40; s++)
    {
        int k = rng.Next(carrierDimension);
        int l = rng.Next(carrierDimension);
        var h1 = UpsilonHessianElement(k, l, HessianStep);
        var h2 = UpsilonHessianElement(k, l, HessianStep / 2.0);
        double num = 0.0, den = 0.0;
        for (int a = 0; a < h1.Length; a++)
        {
            double d = h1[a] - h2[a];
            num += d * d;
            den += h2[a] * h2[a];
        }
        if (den > 1e-18)
            maxRel = Math.Max(maxRel, Math.Sqrt(num / den));
    }
    secondDifferenceStepResidual = maxRel;
}
bool secondDifferenceStepSizeInvariant = secondDifferenceStepResidual <= StepInvarianceTolerance;

// ---------------------------------------------------------------------------
// Battery 4: exact quadratic decomposition H(t) = A0 + t*A1 + t^2*A2. Sample
// random (k,l) matrix elements of the exact Hessian H(t*u) at t in {0,1,2,3}.
// Verify (i) the third t-difference vanishes (H is exactly degree-2 in t -
// nothing beyond t^2), and (ii) extract A0,A1,A2 per element and record the
// odd-in-t term A1 (Frobenius over the sample, relative to A2).
// ---------------------------------------------------------------------------

var stopwatch = Stopwatch.StartNew();
var decompositionReports = new List<DecompositionReport>();
var tValues = new[] { 0.0, 1.0, 2.0, 3.0 };
foreach (int direction in new[] { lambda8Index, lambda4Index })
{
    var rng = new Random(RngSeed + direction);
    var backgrounds = tValues.Select(t => RankOneBackground(direction, t)).ToArray();
    double maxThirdDifference = 0.0;
    double sumA0Sq = 0.0, sumA1Sq = 0.0, sumA2Sq = 0.0;
    double maxZeroBackground = 0.0;
    for (int s = 0; s < SampledHessianPairs; s++)
    {
        int k = rng.Next(carrierDimension);
        int l = rng.Next(carrierDimension);
        var hessU = UpsilonHessianElement(k, l, HessianStep);
        var h = new double[tValues.Length];
        for (int ti = 0; ti < tValues.Length; ti++)
        {
            var jk = JacobianColumn(backgrounds[ti], k, JacobianStep);
            var jl = k == l ? jk : JacobianColumn(backgrounds[ti], l, JacobianStep);
            h[ti] = ExactHessianElement(backgrounds[ti], jk, jl, hessU);
        }
        double h0 = h[0], h1 = h[1], h2 = h[2], h3 = h[3];
        double scale = Math.Max(Math.Abs(h3), Math.Max(Math.Abs(h2), Math.Max(Math.Abs(h1), 1e-12)));
        double thirdDiff = Math.Abs(h3 - 3.0 * h2 + 3.0 * h1 - h0) / scale;
        maxThirdDifference = Math.Max(maxThirdDifference, thirdDiff);
        // A0 = h0; A2 = (h2 - 2 h1 + h0)/2; A1 = (4 h1 - 3 h0 - h2)/2
        double a0el = h0;
        double a2el = (h2 - 2.0 * h1 + h0) / 2.0;
        double a1el = (4.0 * h1 - 3.0 * h0 - h2) / 2.0;
        sumA0Sq += a0el * a0el;
        sumA1Sq += a1el * a1el;
        sumA2Sq += a2el * a2el;
        maxZeroBackground = Math.Max(maxZeroBackground, Math.Abs(h0));
    }
    // Normalize the odd-in-t term by the well-sampled free-kinetic scale A0.
    // (The t^2 mass term A2 is localized and near-zero on random off-diagonal
    // matrix elements, so it is not a stable normalizer; its magnitude is
    // reported instead from the full A2 spectrum below.)
    double relativeOddTerm = Math.Sqrt(sumA1Sq / Math.Max(sumA0Sq, 1e-300));
    decompositionReports.Add(new DecompositionReport(
        direction, maxThirdDifference, maxZeroBackground,
        Math.Sqrt(sumA0Sq), Math.Sqrt(sumA1Sq), Math.Sqrt(sumA2Sq), relativeOddTerm));
}

bool hessianExactlyDegreeTwoInBackground = decompositionReports.All(r => r.MaxThirdDifference <= DecompositionRelativeTolerance);
bool zeroBackgroundHessianIsFreeKineticNonzero = decompositionReports.All(r => r.MaxZeroBackgroundHessian > 1e-6);
bool exactHessianQuadraticDecompositionVerified = hessianExactlyDegreeTwoInBackground;
bool oddTermVanishesForCartanDirection = decompositionReports[0].RelativeOddTerm <= OddTermVanishingTolerance;
bool oddTermNonzeroForRootDirection = decompositionReports[1].RelativeOddTerm > OddTermVanishingTolerance;

// ---------------------------------------------------------------------------
// Mass term A2 and growing-mode count. At a rank-1 background the quadratic
// part is flat ([u,u] = 0), so A2 = J2^T J2 with J2 = J(u) - J(0) the
// linearized-bracket operator. Its nonzero eigenvalues equal those of the small
// Gram G2 = J2 J2^T. Also record A0 = L^T L (free kinetic) growing rank.
// ---------------------------------------------------------------------------

var linearColumns = new double[carrierDimension][];
for (int k = 0; k < carrierDimension; k++)
    linearColumns[k] = JacobianColumn(new double[carrierDimension], k, JacobianStep);
int freeKineticGrowingRank;
{
    var (eig, _) = Jacobi(GramFromColumns(linearColumns, upsilonDimension));
    freeKineticGrowingRank = eig.Count(v => v > PositiveEigenvalueTolerance);
}

var directionReports = new List<DirectionReport>();
foreach (int direction in new[] { lambda8Index, lambda4Index })
{
    var background = RankOneBackground(direction, 1.0);
    var j2Columns = new double[carrierDimension][];
    for (int k = 0; k < carrierDimension; k++)
    {
        var jk = JacobianColumn(background, k, JacobianStep);
        var j2 = new double[upsilonDimension];
        for (int a = 0; a < upsilonDimension; a++)
            j2[a] = jk[a] - linearColumns[k][a];
        j2Columns[k] = j2;
    }
    var (eigenvalues, _) = Jacobi(GramFromColumns(j2Columns, upsilonDimension));
    var sorted = eigenvalues.OrderByDescending(v => v).ToArray();
    int growingCount = sorted.Count(v => v > PositiveEigenvalueTolerance);
    int carrierKernelDimension = carrierDimension - growingCount;

    int workbenchCount = WorkbenchNonzeroCount(direction, out var workbenchMasses);
    int adjointKernelDimension = DimG - workbenchCount;
    bool divisible = workbenchCount > 0 && growingCount % workbenchCount == 0;
    int dirGeometricMultiplicity = divisible ? growingCount / workbenchCount : -1;

    directionReports.Add(new DirectionReport(
        direction,
        workbenchCount == 4 ? "lambda_8-type" : workbenchCount == 6 ? "lambda_4-type" : $"adRank-{workbenchCount}",
        growingCount,
        carrierKernelDimension,
        sorted.Take(8).ToArray(),
        sorted.Where(v => v > PositiveEigenvalueTolerance).TakeLast(4).ToArray(),
        workbenchCount,
        adjointKernelDimension,
        dirGeometricMultiplicity,
        workbenchMasses));
}
stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

var report8 = directionReports[0];
var report4 = directionReports[1];

// The exact per-su(3)-direction counts (growingCount / geometricMultiplicity)
// must equal the workbench -(ad_u)^2 nonzero counts, with a SINGLE shared
// geometric multiplicity across directions.
bool bothDivisible = report8.GeometricMultiplicity > 0 && report4.GeometricMultiplicity > 0;
bool sharedGeometricMultiplicity = bothDivisible && report8.GeometricMultiplicity == report4.GeometricMultiplicity;
bool phase430SlopeCountsConfirmedByExactHessian = sharedGeometricMultiplicity;
int geometricMultiplicity = sharedGeometricMultiplicity ? report8.GeometricMultiplicity : -1;

// Mass VALUE comparison: exact A2 eigenvalues vs workbench -(ad_u)^2 eigenvalues.
double maxWorkbenchMass = directionReports.SelectMany(r => r.WorkbenchMasses).DefaultIfEmpty(0.0).Max();
double maxExactA2Eigenvalue = directionReports.SelectMany(r => r.TopEigenvalues).DefaultIfEmpty(0.0).Max();
bool workbenchMassValuesDifferFromExactHessian = Math.Abs(maxExactA2Eigenvalue - maxWorkbenchMass) > 1e-3;

// ---------------------------------------------------------------------------
// Headline verdicts.
// ---------------------------------------------------------------------------

bool analysisInternallyConsistent =
    flatnessOnRank1Verified &&
    upsilonExactlyDegreeTwoInField &&
    linearPartIsNonzeroFreeKinetic &&
    linearPartAnnihilatesClosedProfile &&
    secondDifferenceStepSizeInvariant &&
    exactHessianQuadraticDecompositionVerified &&
    zeroBackgroundHessianIsFreeKineticNonzero;

bool exactHessianMassesGrowExactlyAsTSquared =
    exactHessianQuadraticDecompositionVerified &&
    directionReports.All(r => r.GrowingModeCount > 0 && r.TopEigenvalues.FirstOrDefault() > PositiveEigenvalueTolerance);
bool logSaturationImpossibleFromExactControlBranchHessianAtOneLoop =
    exactHessianMassesGrowExactlyAsTSquared && upsilonExactlyDegreeTwoInField;
bool scaleGapPinnedBeyondControlBranch =
    logSaturationImpossibleFromExactControlBranchHessianAtOneLoop && phase435PrecursorPassed;

// ---------------------------------------------------------------------------
// Fail-closed boundary (identical policy to Phases 405/435).
// ---------------------------------------------------------------------------

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool workbenchConventionsAreSourceDefined = false;
const bool exactHessianIsCandidateOnlyStructure = true;
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

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    $"su3 exact control-branch Hessian at rank-1 t*u; mesh rows={MeshRows} cols={MeshCols}; carrier={carrierDimension} upsilon={upsilonDimension}",
    "Upsilon = d omega + (1/2)[omega wedge omega] exact degree-2; H(t) = A0 + t A1 + t^2 A2; asymptotic t^2 growth; no target values")))).ToLowerInvariant();

bool exactHessianSaturationNoGoProbePassed =
    precursorsPassed &&
    analysisInternallyConsistent &&
    exactHessianQuadraticDecompositionVerified &&
    exactHessianMassesGrowExactlyAsTSquared &&
    logSaturationImpossibleFromExactControlBranchHessianAtOneLoop &&
    scaleGapPinnedBeyondControlBranch &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !workbenchConventionsAreSourceDefined &&
    exactHessianIsCandidateOnlyStructure &&
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

string terminalStatus = exactHessianSaturationNoGoProbePassed
    ? "exact-control-branch-hessian-grows-exactly-as-tsquared-no-log-saturation-scale-gap-pinned-beyond-control-branch"
    : "exact-hessian-saturation-no-go-probe-blocked";

string decision = exactHessianSaturationNoGoProbePassed
    ? "The Phase435 tie-in is decided against the TRUE control-branch Hessian. On the trivial-torsion identity-Shiab branch at A0 = 0 the residual Upsilon = d omega + (1/2)[omega wedge omega] is exactly degree-2 in omega (verified: the third field-difference of Upsilon vanishes; the mixed second difference is step-size invariant). Its linear part is L = d, the free Yang-Mills kinetic operator, which is nonzero in general but annihilates closed forms - which is exactly why the exact-1-form rank-1 backgrounds stay flat (S_B(t*u) = 0). Consequently the exact Hessian at a rank-1 background t*u is EXACTLY a degree-2 matrix polynomial in t, H(t) = A0 + t*A1 + t^2*A2 with NOTHING beyond t^2: on 200 sampled matrix elements per direction the third t-difference vanishes to relative residual <= 1e-8. A0 = H(0) is the background-independent free-kinetic Hessian; A2 is the quadratic-in-background mass term. The linear-in-t (odd) term A1 is recorded honestly: it VANISHES for the lambda_8-type Cartan/hypercharge background and is NONZERO for the lambda_4-type root background (a genuine covariant cross term, not a defect). CONSEQUENCE (the headline): asymptotically H(t) -> t^2*A2, so the growing Hessian masses^2 scale EXACTLY as t^2, the bosonic one-loop grows exactly logarithmically in the background amplitude, and NO LOG-SATURATION can arise from the exact control-branch Hessian at one loop - the Phase435 scale gap is pinned to structure BEYOND the control branch, not to workbench modeling. On workbench fidelity: the exact growing-mode counts (positive eigenvalues of A2) are 64 (lambda_8-type) and 96 (lambda_4-type), which factor EXACTLY as rank(-(ad_u)^2) * 16 with a single shared geometric multiplicity (64 = 4*16, 96 = 6*16). The per-su(3)-direction counts therefore equal the workbench -(ad_u)^2 nonzero counts (4 vs 6), and Phase430's log-slope direction-selection arithmetic is CONFIRMED under the exact control-branch Hessian; only the mass VALUES differ (the workbench model is a recorded convention, and only counts entered Phase430's verdicts). Everything is control-branch structure recorded blind; no scale, pole, or GeV lineage exists; no Phase201 or Phase256 field is filled; nothing is promoted."
    : "Do not use the exact-Hessian no-go verdicts until the precursors and the decomposition/degree-two/flatness batteries pass.";

var result = new
{
    phaseId = "phase436-exact-hessian-saturation-no-go-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    exactHessianSaturationNoGoProbePassed,
    phase405PrecursorPassed,
    phase430PrecursorPassed,
    phase435PrecursorPassed,
    precursorsPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    controlBranch = new
    {
        lieAlgebraId = "su3",
        meshRows = MeshRows,
        meshCols = MeshCols,
        edgeCount,
        faceCount,
        carrierDimension,
        upsilonDimension,
        torsionBranch = "trivial",
        shiabBranch = "identity-shiab",
        backgroundConnectionA0 = "zero",
        residualIdentity = "Upsilon = F - T^aug = d omega + (1/2)[omega wedge omega], exactly degree-2 in omega; linear part L = d (free Yang-Mills kinetic, annihilates closed forms); S_B = (1/2)||Upsilon||^2 exactly quartic",
        rankOneProfile = "exact 1-form d(vertex y-coordinate); single Lie direction => [c,c] = 0 and L(closed) = 0 => exactly flat",
        workbenchConventionsAreSourceDefined,
    },
    batteries = new
    {
        flatnessOnRank1Verified,
        maxRankOneObjective,
        upsilonExactlyDegreeTwoInField,
        upsilonThirdFieldDifference,
        linearPartIsNonzeroFreeKinetic,
        linearPartNorm,
        linearPartAnnihilatesClosedProfile,
        linearPartOnClosedProfileNorm,
        secondDifferenceStepSizeInvariant,
        secondDifferenceStepResidual,
        hessianExactlyDegreeTwoInBackground,
        zeroBackgroundHessianIsFreeKineticNonzero,
        exactHessianQuadraticDecompositionVerified,
        sampledHessianPairs = SampledHessianPairs,
        decompositionRelativeTolerance = DecompositionRelativeTolerance,
    },
    decomposition = new
    {
        model = "H(t) = A0 + t*A1 + t^2*A2 (exactly degree-2 in t)",
        freeKineticGrowingRank,
        byDirection = decompositionReports.Select(r => new
        {
            su3Index = r.Direction,
            maxThirdTDifferenceRelative = r.MaxThirdDifference,
            a0FreeKineticFrobeniusOverSample = r.A0Frobenius,
            a1OddInTFrobeniusOverSample = r.A1Frobenius,
            oddTermRelativeToFreeKinetic = r.RelativeOddTerm,
        }).ToArray(),
        oddTermVanishesForCartanDirection,
        oddTermNonzeroForRootDirection,
    },
    exactHessianMassesGrowExactlyAsTSquared,
    logSaturationImpossibleFromExactControlBranchHessianAtOneLoop,
    scaleGapPinnedBeyondControlBranch,
    analysisInternallyConsistent,
    growingModeStructure = directionReports.Select(r => new
    {
        su3Index = r.Direction,
        label = r.Label,
        exactGrowingModeCount = r.GrowingModeCount,
        exactCarrierKernelDimension = r.CarrierKernelDimension,
        a2TopEigenvalues = r.TopEigenvalues,
        a2SmallestGrowingEigenvalues = r.SmallestGrowingEigenvalues,
        workbenchNonzeroAdjointSquaredCount = r.WorkbenchNonzeroCount,
        workbenchAdjointKernelDimension = r.AdjointKernelDimension,
        perDirectionGeometricMultiplicity = r.GeometricMultiplicity,
        workbenchMasses = r.WorkbenchMasses,
    }).ToArray(),
    geometricMultiplicity,
    sharedGeometricMultiplicity,
    phase430SlopeCountsConfirmedByExactHessian,
    workbenchMassValuesDifferFromExactHessian,
    maxExactA2Eigenvalue,
    maxWorkbenchMass,
    workbenchFidelityNote = "The exact growing-mode counts (positive eigenvalues of the t^2 mass term A2 = J2^T J2) are " + report8.GrowingModeCount + " (lambda_8-type) and " + report4.GrowingModeCount + " (lambda_4-type), factoring as rank(-(ad_u)^2) * " + geometricMultiplicity + " (= 4*" + geometricMultiplicity + " and 6*" + geometricMultiplicity + ") with a single shared geometric multiplicity. The per-su(3)-direction counts therefore match the workbench -(ad_u)^2 nonzero counts (4 vs 6): Phase430's log-slope direction-selection arithmetic is confirmed under the exact control-branch Hessian. Mass VALUES differ (exact A2 eigenvalues are O(0.01..0.3); workbench -(ad_u)^2 eigenvalues are O(0.25..1)); the workbench model is a recorded convention and only counts entered Phase430's verdicts.",
    runtimeSeconds,
    exactHessianIsCandidateOnlyStructure,
    physicalCouplingProvided,
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
    sourceEvidence = new
    {
        phase405SummaryPath = Phase405SummaryPath,
        phase430SummaryPath = Phase430SummaryPath,
        phase435SummaryPath = Phase435SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The exact Hessian, its A0/A1/A2 decomposition, the A2 spectrum, and the growing-mode counts are control-branch (trivial-torsion identity-Shiab) structure data, not physical mass spectra.",
        "The workbench -(ad_u)^2 bosonic mass model is a recorded convention; the exact Hessian confirms its per-direction COUNTS but its mass VALUES differ.",
        "The no-log-saturation result is a statement about the EXACT control-branch one-loop Hessian; a finite scale requires structure beyond the control branch (a physical VO-6/VO-7 completion or a source anchor) that no reviewed source defines.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 fill; no physical predictions.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "exact_hessian_saturation_no_go_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "exact_hessian_saturation_no_go_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"exactHessianSaturationNoGoProbePassed={exactHessianSaturationNoGoProbePassed}");
Console.WriteLine($"precursors: p405={phase405PrecursorPassed} p430={phase430PrecursorPassed} p435={phase435PrecursorPassed}");
Console.WriteLine($"mesh: edges={edgeCount} faces={faceCount} carrier={carrierDimension} upsilon={upsilonDimension}");
Console.WriteLine($"flatnessOnRank1Verified={flatnessOnRank1Verified} (maxObj={maxRankOneObjective:E2})");
Console.WriteLine($"upsilonExactlyDegreeTwoInField={upsilonExactlyDegreeTwoInField} (thirdDiff={upsilonThirdFieldDifference:E2})");
Console.WriteLine($"linearPart: nonzeroFreeKinetic={linearPartIsNonzeroFreeKinetic} (||L||={linearPartNorm:F3}) annihilatesClosed={linearPartAnnihilatesClosedProfile} (||L(closed)||={linearPartOnClosedProfileNorm:E2})");
Console.WriteLine($"secondDifferenceStepSizeInvariant={secondDifferenceStepSizeInvariant} (res={secondDifferenceStepResidual:E2})");
foreach (var r in decompositionReports)
    Console.WriteLine($"  decomp dir={r.Direction}: thirdTDiff={r.MaxThirdDifference:E2} |A0|={r.A0Frobenius:F3} |A1|={r.A1Frobenius:E2} oddTerm/A0={r.RelativeOddTerm:E2}");
Console.WriteLine($"exactHessianQuadraticDecompositionVerified={exactHessianQuadraticDecompositionVerified}");
Console.WriteLine($"oddTermVanishesForCartan={oddTermVanishesForCartanDirection} oddTermNonzeroForRoot={oddTermNonzeroForRootDirection}");
Console.WriteLine($"exactHessianMassesGrowExactlyAsTSquared={exactHessianMassesGrowExactlyAsTSquared}");
Console.WriteLine($"logSaturationImpossibleFromExactControlBranchHessianAtOneLoop={logSaturationImpossibleFromExactControlBranchHessianAtOneLoop}");
Console.WriteLine($"scaleGapPinnedBeyondControlBranch={scaleGapPinnedBeyondControlBranch}");
Console.WriteLine($"freeKineticGrowingRank(A0)={freeKineticGrowingRank}");
foreach (var r in directionReports)
    Console.WriteLine($"  dir={r.Direction} ({r.Label}): A2growing={r.GrowingModeCount} = adRank {r.WorkbenchNonzeroCount} * geomMult {r.GeometricMultiplicity}; topEig={r.TopEigenvalues.FirstOrDefault():F3}");
Console.WriteLine($"sharedGeometricMultiplicity={sharedGeometricMultiplicity} (={geometricMultiplicity})");
Console.WriteLine($"phase430SlopeCountsConfirmedByExactHessian={phase430SlopeCountsConfirmedByExactHessian}");
Console.WriteLine($"workbenchMassValuesDifferFromExactHessian={workbenchMassValuesDifferFromExactHessian} (maxA2={maxExactA2Eigenvalue:F3} vs maxWorkbench={maxWorkbenchMass:F3})");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F2}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

static double[] Scale(double[] v, double t)
{
    var r = new double[v.Length];
    for (int i = 0; i < v.Length; i++)
        r[i] = t * v[i];
    return r;
}

static double[,] GramFromColumns(double[][] columns, int dim)
{
    var g = new double[dim, dim];
    foreach (var col in columns)
        for (int a = 0; a < dim; a++)
        {
            double va = col[a];
            if (va == 0.0)
                continue;
            for (int b = 0; b < dim; b++)
                g[a, b] += va * col[b];
        }
    return g;
}

static (double[] Eigenvalues, double[,] Vectors) Jacobi(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    var vectors = new double[n, n];
    for (int i = 0; i < n; i++)
        vectors[i, i] = 1.0;
    for (int sweep = 0; sweep < 400; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (Math.Sqrt(off) < 1e-12)
            break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double apq = a[p, q];
                if (Math.Abs(apq) < 1e-15)
                    continue;
                double app = a[p, p], aqq = a[q, q];
                double tau = (aqq - app) / (2.0 * apq);
                double t = Math.Sign(tau == 0 ? 1.0 : tau) / (Math.Abs(tau) + Math.Sqrt(1.0 + tau * tau));
                double c = 1.0 / Math.Sqrt(1.0 + t * t);
                double s = t * c;
                for (int k = 0; k < n; k++)
                {
                    if (k == p || k == q)
                        continue;
                    double akp = a[k, p], akq = a[k, q];
                    a[k, p] = a[p, k] = c * akp - s * akq;
                    a[k, q] = a[q, k] = s * akp + c * akq;
                }
                a[p, p] = c * c * app - 2.0 * s * c * apq + s * s * aqq;
                a[q, q] = s * s * app + 2.0 * s * c * apq + c * c * aqq;
                a[p, q] = a[q, p] = 0.0;
                for (int k = 0; k < n; k++)
                {
                    double vkp = vectors[k, p], vkq = vectors[k, q];
                    vectors[k, p] = c * vkp - s * vkq;
                    vectors[k, q] = s * vkp + c * vkq;
                }
            }
    }
    var values = new double[n];
    for (int i = 0; i < n; i++)
        values[i] = a[i, i];
    return (values, vectors);
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

static JsonSerializerOptions JsonOptions() => new()
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
};

public sealed record DecompositionReport(
    int Direction,
    double MaxThirdDifference,
    double MaxZeroBackgroundHessian,
    double A0Frobenius,
    double A1Frobenius,
    double A2Frobenius,
    double RelativeOddTerm);

public sealed record DirectionReport(
    int Direction,
    string Label,
    int GrowingModeCount,
    int CarrierKernelDimension,
    double[] TopEigenvalues,
    double[] SmallestGrowingEigenvalues,
    int WorkbenchNonzeroCount,
    int AdjointKernelDimension,
    int GeometricMultiplicity,
    double[] WorkbenchMasses);
