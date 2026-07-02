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

// Phase441: toy-branch family universality sweep (the last named internal experiment).
//
// Phases 435/436/440 pinned the bosonic scale gap on the CONTROL branch (identity
// Shiab, trivial torsion, A0 = 0), reached three independent ways: (i) the exact
// control-branch Hessian H(t) = H0 + t*H1 + t^2*H2 is degree-2 in the background
// amplitude with a positive-definite t^2 term, so the one-loop grows exactly as
// log(t) and NO log-saturation can arise (Phase436); (ii) the growing-mode counts
// 64 (lambda_8-type) / 96 (lambda_4-type) confirm Phase430's direction-selection
// arithmetic; (iii) the coupled boson-fermion system has no interior fixed point,
// so the fermionic runaway persists (Phase440).
//
// The platform admits a FAMILY of toy-realizable branch completions:
//   Shiab   in { identity-shiab, first-order-curvature, metric-scaled-shiab(lambda) }
//   Torsion in { trivial, augmented-torsion, local-algebraic }
//   A0      in { 0, const lambda_8 (amp 0.5, 1.0), mixed lambda_8+lambda_4 constant }
// This phase sweeps the full 3 x 3 x 4 = 36-member Cartesian product and asks
// whether the Phase436 no-saturation structure is UNIVERSAL across the toy family.
//
// KEY STRUCTURAL FACT (verified below, member by member): the residual is
//   Upsilon = S(F) - T,   F = d omega + (1/2)[omega wedge omega].
// Every quadratic-in-omega piece is a Lie bracket. Along a rank-1 background t*u
// with u a SINGLE Lie direction (exact-1-form profile), every bracket [u,u] = 0,
// so Upsilon(t*u) is AFFINE in t for EVERY family member. Therefore the exact
// Hessian H(t) = J(t)^T J(t) + <U(t), Hess U> is still exactly degree-2 in t
// (J affine, Hess U constant, <U(t),.> affine), and its t^2 coefficient reduces
// to H2 = J2^T J2 with J2 = J(u) - J(0) - EXACTLY Phase436's operator. Moreover
// A0 enters J(omega) only additively and constant, so it cancels from J2 = J(u) -
// J(0): the growing-mode structure is A0-INDEPENDENT. The consequences of these
// facts are recorded, not assumed - each is tested by finite differences.
//
// CANONICAL PHYSICAL SHIAB (draft Section 32.2, Sigma_mc): the draft's active
// Shiab branch requires a background algebraic contraction K_{A0} built from the
// Einsteinian projection and metric/bundle data, i.e. a Ricci/Weyl-like second-
// order structure that needs dimX >= 4 and a metric-spinor (Cl(7,7)/128) basis.
// It CANNOT be realized on the 2D toy (MetricScaledShiabOperator's own comment
// records this). That impossibility is a key recorded output of this phase.
//
// Fail-closed: toy family only; no scales; nothing promoted; no contract field
// can be filled. PASSES on internal consistency regardless of how the honest
// count/runaway verdicts fall.

const string DefaultOutputDir = "studies/phase441_toy_branch_family_universality_sweep_001/output";
const string Phase436SummaryPath = "studies/phase436_exact_hessian_saturation_no_go_probe_001/output/exact_hessian_saturation_no_go_probe_summary.json";
const string Phase440SummaryPath = "studies/phase440_coupled_background_condensate_fixed_point_probe_001/output/coupled_background_condensate_fixed_point_probe_summary.json";
const string MetricScaledShiabSourcePath = "src/Gu.ReferenceCpu/MetricScaledShiabOperator.cs";
const string DraftSourcePath = "TheoryCompletitionRevisions/Geometric_Unity_Completion_Reorganized_Updated_v29.md";
const string ApplicationSubjectKind = "toy-branch-family-universality-sweep";

const int MeshRows = 2;
const int MeshCols = 2;
const int DimG = 8;
const double JacobianStep = 1e-2;      // Upsilon is degree-2 => central diff exact
const double HessianStep = 1e-2;       // mixed 2nd diff of a quadratic is exact
const int SampledHessianPairs = 200;   // decomposition check on random (k,l) elements
const int DegreeProbeCount = 4;        // random (base, direction) probes for the degree test
const double DegreeTolerance = 1e-8;   // third-difference relative residual pass threshold
const double DecompositionRelativeTolerance = 1e-8;
const double OddTermVanishingTolerance = 1e-8;
const double PositiveEigenvalueTolerance = 1e-6;
const double MetricScale = 2.0;        // lambda != 1 so metric-scaled is numerically distinct
const int RngSeed = 20260702;

// Phase430/440 workbench fermionic content (recorded convention, reused honestly).
const int FermionicDerivedHyperchargeCoupledCount = 768;   // |phase440 fermionSDerivedSlope|
const int BosonPolarizationConvention = 2;                 // 2 physical polarizations

var outputDir = Environment.GetEnvironmentVariable("PHASE441_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors.
// ---------------------------------------------------------------------------

using var phase436 = JsonDocument.Parse(File.ReadAllText(Phase436SummaryPath));
using var phase440 = JsonDocument.Parse(File.ReadAllText(Phase440SummaryPath));

bool phase436PrecursorPassed =
    JsonBool(phase436.RootElement, "exactHessianSaturationNoGoProbePassed") is true &&
    JsonBool(phase436.RootElement, "scaleGapPinnedBeyondControlBranch") is true;
bool phase440PrecursorPassed =
    JsonBool(phase440.RootElement, "coupledBackgroundCondensateFixedPointProbePassed") is true &&
    JsonBool(phase440.RootElement, "jointFixedPointExists") is false;
bool precursorsPassed = phase436PrecursorPassed && phase440PrecursorPassed;

// ---------------------------------------------------------------------------
// Machinery (Phase436 template): su(3) algebra, structured fiber-bundle mesh.
// ---------------------------------------------------------------------------

var algebra = LieAlgebraFactory.CreateSu3();
var bundle = ToyGeometryFactory.CreateStructuredFiberBundle2D(rows: MeshRows, cols: MeshCols);
var mesh = bundle.AmbientMesh;
int edgeCount = mesh.EdgeCount;
int faceCount = mesh.FaceCount;
int carrierDimension = edgeCount * DimG;
int upsilonDimension = faceCount * DimG;
var geometry = bundle.ToGeometryContext("centroid", "P1");

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

int lambda8Index = 7;   // lambda_8-type Cartan (hypercharge) direction
int lambda4Index = 3;   // lambda_4-type root direction

// Exact 1-form profile (Phase436): d(vertex y-coordinate). Closed => annihilated
// by the linear part d; a single Lie direction has [c,c] = 0.
var profileExact = new double[edgeCount];
for (int e = 0; e < edgeCount; e++)
{
    var c0 = mesh.GetVertexCoordinates(mesh.Edges[e][0]);
    var c1 = mesh.GetVertexCoordinates(mesh.Edges[e][1]);
    profileExact[e] = c1[1] - c0[1];
}

double[] RankOneBackground(int direction, double t)
{
    var field = new double[carrierDimension];
    for (int e = 0; e < edgeCount; e++)
        field[e * DimG + direction] += t * profileExact[e];
    return field;
}

// Constant-amplitude A0 fields (value = amp on every edge in the given directions).
double[] ConstantA0(int[] directions, double[] amplitudes)
{
    var field = new double[carrierDimension];
    for (int e = 0; e < edgeCount; e++)
        for (int di = 0; di < directions.Length; di++)
            field[e * DimG + directions[di]] = amplitudes[di];
    return field;
}

// ---------------------------------------------------------------------------
// Manifest builder (per family member). Metric-scaled reads lambda from
// Parameters["metricScale"]; others ignore it.
// ---------------------------------------------------------------------------

BranchManifest BuildManifest(string shiabBranch, string torsionBranch, bool metricScaled) => new()
{
    BranchId = $"phase441-{shiabBranch}-{torsionBranch}",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "r1",
    CodeRevision = "phase441",
    LieAlgebraId = "su3",
    BaseDimension = bundle.BaseMesh.EmbeddingDimension,
    AmbientDimension = mesh.EmbeddingDimension,
    ActiveGeometryBranch = "simplicial",
    ActiveObservationBranch = "sigma-pullback",
    ActiveTorsionBranch = torsionBranch,
    ActiveShiabBranch = shiabBranch,
    ActiveGaugeStrategy = "penalty",
    PairingConventionId = "pairing-killing",
    BasisConventionId = "canonical",
    ComponentOrderId = "face-major",
    AdjointConventionId = "adjoint-explicit",
    NormConventionId = "norm-l2-quadrature",
    DifferentialFormMetricId = "hodge-standard",
    InsertedAssumptionIds = Array.Empty<string>(),
    InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    Parameters = metricScaled
        ? new Dictionary<string, string> { ["metricScale"] = MetricScale.ToString(System.Globalization.CultureInfo.InvariantCulture) }
        : null,
};

IShiabBranchOperator BuildShiab(string shiabBranch, BranchManifest manifest) => shiabBranch switch
{
    "identity-shiab" => new IdentityShiabCpu(mesh, algebra),
    "first-order-curvature" => new FirstOrderShiabOperator(mesh, algebra),
    "metric-scaled-shiab" => new MetricScaledShiabOperator(mesh, algebra, manifest),
    _ => throw new ArgumentException($"unknown shiab '{shiabBranch}'"),
};

ITorsionBranchOperator BuildTorsion(string torsionBranch) => torsionBranch switch
{
    "trivial" => new TrivialTorsionCpu(mesh, algebra),
    "augmented-torsion" => new AugmentedTorsionCpu(mesh, algebra),
    "local-algebraic" => new LocalAlgebraicTorsionOperator(mesh, algebra),
    _ => throw new ArgumentException($"unknown torsion '{torsionBranch}'"),
};

// ---------------------------------------------------------------------------
// Family enumeration.
// ---------------------------------------------------------------------------

var shiabBranches = new[] { "identity-shiab", "first-order-curvature", "metric-scaled-shiab" };
var torsionBranches = new[] { "trivial", "augmented-torsion", "local-algebraic" };
var a0Specs = new (string Label, double[] Field)[]
{
    ("zero", new double[carrierDimension]),
    ("const-lambda8-0.5", ConstantA0(new[] { lambda8Index }, new[] { 0.5 })),
    ("const-lambda8-1.0", ConstantA0(new[] { lambda8Index }, new[] { 1.0 })),
    ("mixed-lambda8+lambda4", ConstantA0(new[] { lambda8Index, lambda4Index }, new[] { 1.0, 1.0 })),
};

// ---------------------------------------------------------------------------
// Per-member analysis machinery (all finite-difference, Phase436 method).
// ---------------------------------------------------------------------------

double[] EvalUpsilon(ISolverBackend backend, FieldTensor a0T, BranchManifest manifest, double[] coeffs)
    => backend.EvaluateDerived(WrapCarrier(coeffs), a0T, manifest, geometry).ResidualUpsilon.Coefficients;

double[] JacobianColumn(ISolverBackend backend, FieldTensor a0T, BranchManifest manifest, double[] background, int k, double h)
{
    var plus = (double[])background.Clone(); plus[k] += h;
    var minus = (double[])background.Clone(); minus[k] -= h;
    var up = EvalUpsilon(backend, a0T, manifest, plus);
    var um = EvalUpsilon(backend, a0T, manifest, minus);
    var column = new double[up.Length];
    for (int a = 0; a < up.Length; a++)
        column[a] = (up[a] - um[a]) / (2.0 * h);
    return column;
}

double[] UpsilonHessianElement(ISolverBackend backend, FieldTensor a0T, BranchManifest manifest, int k, int l, double h)
{
    var pp = new double[carrierDimension]; pp[k] += h; pp[l] += h;
    var pm = new double[carrierDimension]; pm[k] += h; pm[l] -= h;
    var mp = new double[carrierDimension]; mp[k] -= h; mp[l] += h;
    var mm = new double[carrierDimension]; mm[k] -= h; mm[l] -= h;
    var upp = EvalUpsilon(backend, a0T, manifest, pp);
    var upm = EvalUpsilon(backend, a0T, manifest, pm);
    var ump = EvalUpsilon(backend, a0T, manifest, mp);
    var umm = EvalUpsilon(backend, a0T, manifest, mm);
    var result = new double[upp.Length];
    for (int a = 0; a < upp.Length; a++)
        result[a] = (upp[a] - upm[a] - ump[a] + umm[a]) / (4.0 * h * h);
    return result;
}

double ExactHessianElement(double[] uBackground, double[] jk, double[] jl, double[] hessU)
{
    double sum = 0.0;
    for (int a = 0; a < jk.Length; a++)
        sum += jk[a] * jl[a] + uBackground[a] * hessU[a];
    return sum;
}

// ---------------------------------------------------------------------------
// Sweep.
// ---------------------------------------------------------------------------

var stopwatch = Stopwatch.StartNew();
var members = new List<MemberRecord>();
int controlIndex = -1;

foreach (var shiabBranch in shiabBranches)
foreach (var torsionBranch in torsionBranches)
foreach (var (a0Label, a0Field) in a0Specs)
{
    bool metricScaled = shiabBranch == "metric-scaled-shiab";
    var manifest = BuildManifest(shiabBranch, torsionBranch, metricScaled);
    var shiab = BuildShiab(shiabBranch, manifest);
    var torsion = BuildTorsion(torsionBranch);

    // Battery: strict carrier-signature match between torsion and Shiab (throws on
    // mismatch). Success = no throw.
    bool validateCarrierMatchPassed;
    try { BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab); validateCarrierMatchPassed = true; }
    catch { validateCarrierMatchPassed = false; }

    var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);
    var a0T = WrapCarrier(a0Field);
    var rng = new Random(RngSeed + members.Count);

    // (1) DEGREE UNIVERSALITY: symmetric third difference of Upsilon at random
    //     (base, direction) pairs must vanish (Upsilon exactly degree-2 in omega).
    double degreeMaxThirdDiff = 0.0;
    for (int p = 0; p < DegreeProbeCount; p++)
    {
        var basePoint = new double[carrierDimension];
        var dir = new double[carrierDimension];
        for (int k = 0; k < carrierDimension; k++)
        {
            basePoint[k] = 0.3 * (rng.NextDouble() - 0.5);
            dir[k] = 0.3 * (rng.NextDouble() - 0.5);
        }
        var um2 = EvalUpsilon(backend, a0T, manifest, Combine(basePoint, dir, -2.0));
        var um1 = EvalUpsilon(backend, a0T, manifest, Combine(basePoint, dir, -1.0));
        var up1 = EvalUpsilon(backend, a0T, manifest, Combine(basePoint, dir, 1.0));
        var up2 = EvalUpsilon(backend, a0T, manifest, Combine(basePoint, dir, 2.0));
        double num = 0.0, den = 0.0;
        for (int a = 0; a < up1.Length; a++)
        {
            double third = up2[a] - 2.0 * up1[a] + 2.0 * um1[a] - um2[a];   // symmetric 3rd diff
            num += third * third;
            den += up1[a] * up1[a] + um1[a] * um1[a];
        }
        degreeMaxThirdDiff = Math.Max(degreeMaxThirdDiff, Math.Sqrt(num / Math.Max(den, 1e-300)));
    }
    bool upsilonDegreeTwo = degreeMaxThirdDiff <= DegreeTolerance;

    // Degree-0 (inhomogeneity from A0) and degree-1 (linear/free-kinetic) parts.
    var uAtZero = EvalUpsilon(backend, a0T, manifest, new double[carrierDimension]);
    double inhomogeneityNorm = Norm(uAtZero);
    double linearPartNormSq = 0.0;
    for (int k = 0; k < carrierDimension; k++)
    {
        var col = JacobianColumn(backend, a0T, manifest, new double[carrierDimension], k, JacobianStep);
        foreach (double v in col) linearPartNormSq += v * v;
    }
    double linearPartNorm = Math.Sqrt(linearPartNormSq);

    // Precompute J(0) columns once (shared across both Hessian-background directions).
    var j0Columns = new double[carrierDimension][];
    for (int k = 0; k < carrierDimension; k++)
        j0Columns[k] = JacobianColumn(backend, a0T, manifest, new double[carrierDimension], k, JacobianStep);

    var directionResults = new List<DirectionResult>();
    foreach (int direction in new[] { lambda8Index, lambda4Index })
    {
        // (2) HESSIAN-DEGREE UNIVERSALITY: H(t*u) = H0 + t*H1 + t^2*H2 (degree-2 in
        //     t). Sample random (k,l) elements of the exact Hessian at t in {0,1,2,3};
        //     third t-difference must vanish. Extract A1 (odd-in-t) relative to A0.
        // Fresh per-direction RNG: for the control member (index 0) this reduces to
        // Phase436's `new Random(RngSeed + direction)`, so the control cross-anchor
        // samples the same elements and reproduces its odd-term values exactly.
        var decompRng = new Random(RngSeed + members.Count * 100003 + direction);
        var tValues = new[] { 0.0, 1.0, 2.0, 3.0 };
        var backgrounds = tValues.Select(t => RankOneBackground(direction, t)).ToArray();
        double sumThirdSq = 0.0, sumRefSq = 0.0, sumA0Sq = 0.0, sumA1Sq = 0.0;
        for (int s = 0; s < SampledHessianPairs; s++)
        {
            int k = decompRng.Next(carrierDimension);
            int l = decompRng.Next(carrierDimension);
            var hessU = UpsilonHessianElement(backend, a0T, manifest, k, l, HessianStep);
            var h = new double[tValues.Length];
            for (int ti = 0; ti < tValues.Length; ti++)
            {
                var uBg = EvalUpsilon(backend, a0T, manifest, backgrounds[ti]);
                var jk = JacobianColumn(backend, a0T, manifest, backgrounds[ti], k, JacobianStep);
                var jl = k == l ? jk : JacobianColumn(backend, a0T, manifest, backgrounds[ti], l, JacobianStep);
                h[ti] = ExactHessianElement(uBg, jk, jl, hessU);
            }
            double h0 = h[0], h1 = h[1], h2 = h[2], h3 = h[3];
            double third = h3 - 3.0 * h2 + 3.0 * h1 - h0;
            // Global relative normalization (robust to sparse near-zero elements whose
            // rounding noise would otherwise dominate a per-element ratio).
            sumThirdSq += third * third;
            sumRefSq += h0 * h0 + h1 * h1 + h2 * h2 + h3 * h3;
            double a0el = h0;
            double a1el = (4.0 * h1 - 3.0 * h0 - h2) / 2.0;
            sumA0Sq += a0el * a0el;
            sumA1Sq += a1el * a1el;
        }
        double maxThirdTDiff = Math.Sqrt(sumThirdSq / Math.Max(sumRefSq, 1e-300));
        double relativeOddTerm = Math.Sqrt(sumA1Sq / Math.Max(sumA0Sq, 1e-300));

        // (3) H2 = J2^T J2 (valid family-wide: U(t*u) is affine in t so the U.HessU
        //     term contributes nothing to the t^2 coefficient). Growing-mode count =
        //     positive eigenvalues of the small Gram J2 J2^T; largest = top mass.
        var background = RankOneBackground(direction, 1.0);
        var j2Columns = new double[carrierDimension][];
        for (int k = 0; k < carrierDimension; k++)
        {
            var jk = JacobianColumn(backend, a0T, manifest, background, k, JacobianStep);
            var j2 = new double[upsilonDimension];
            for (int a = 0; a < upsilonDimension; a++)
                j2[a] = jk[a] - j0Columns[k][a];
            j2Columns[k] = j2;
        }
        var (eig, _) = Jacobi(GramFromColumns(j2Columns, upsilonDimension));
        var sorted = eig.OrderByDescending(v => v).ToArray();
        int growingCount = sorted.Count(v => v > PositiveEigenvalueTolerance);
        double maxH2Eigenvalue = sorted.FirstOrDefault();

        directionResults.Add(new DirectionResult(
            direction,
            direction == lambda8Index ? "lambda_8-type" : "lambda_4-type",
            maxThirdTDiff, relativeOddTerm, growingCount, maxH2Eigenvalue));
    }

    var r8 = directionResults[0];
    var r4 = directionResults[1];
    bool hessianQuadratic = r8.MaxThirdTDiff <= DecompositionRelativeTolerance && r4.MaxThirdTDiff <= DecompositionRelativeTolerance;

    // (4) BALANCE: net one-loop slope on the hypercharge (lambda_8) axis for derived
    //     content = bosonic growing-count * polarizations - fermionic count.
    int bosonicHyperchargeSlope = BosonPolarizationConvention * r8.GrowingModeCount;
    int netDerivedSlope = bosonicHyperchargeSlope - FermionicDerivedHyperchargeCoupledCount;
    bool runaway = netDerivedSlope < 0;

    if (shiabBranch == "identity-shiab" && torsionBranch == "trivial" && a0Label == "zero")
        controlIndex = members.Count;

    members.Add(new MemberRecord(
        members.Count, shiabBranch, torsionBranch, a0Label, metricScaled ? MetricScale : 1.0,
        validateCarrierMatchPassed, degreeMaxThirdDiff, upsilonDegreeTwo,
        inhomogeneityNorm, linearPartNorm,
        r8.MaxThirdTDiff, r4.MaxThirdTDiff, hessianQuadratic,
        r8.RelativeOddTerm, r4.RelativeOddTerm,
        r8.GrowingModeCount, r4.GrowingModeCount, r8.MaxH2Eigenvalue, r4.MaxH2Eigenvalue,
        bosonicHyperchargeSlope, netDerivedSlope, runaway));
}
stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// Universality verdicts.
// ---------------------------------------------------------------------------

var control = members[controlIndex];

bool upsilonDegreeTwoUniversalAcrossFamily = members.All(m => m.UpsilonDegreeTwo);
bool hessianQuadraticDecompositionUniversalAcrossFamily = members.All(m => m.HessianQuadratic);
bool noSaturationTheoremExtendsToEntireToyFamily =
    upsilonDegreeTwoUniversalAcrossFamily && hessianQuadraticDecompositionUniversalAcrossFamily;

bool growingModeCountUniversalAcrossFamily =
    members.All(m => m.Lambda8GrowingCount == control.Lambda8GrowingCount &&
                     m.Lambda4GrowingCount == control.Lambda4GrowingCount);

bool runawayVerdictUniversalAcrossFamily = members.All(m => m.Runaway);

bool allCarrierMatchesPassed = members.All(m => m.ValidateCarrierMatchPassed);

// Battery: control member reproduces Phase436 (counts 64/96; odd term vanishes for
// the Cartan/lambda_8 direction, nonzero for the lambda_4 root direction).
bool controlCountsMatchPhase436 = control.Lambda8GrowingCount == 64 && control.Lambda4GrowingCount == 96;
bool controlOddTermVanishesForCartan = control.Lambda8OddTerm <= OddTermVanishingTolerance;
bool controlOddTermNonzeroForRoot = control.Lambda4OddTerm > OddTermVanishingTolerance;
bool controlCrossAnchorMatchesPhase436 =
    controlCountsMatchPhase436 && controlOddTermVanishesForCartan && controlOddTermNonzeroForRoot;

// Battery: augmented torsion + nonzero A0 has a nonzero degree-0 inhomogeneity;
// trivial torsion has exactly zero inhomogeneity everywhere.
bool augmentedInhomogeneityNonzeroWhereExpected =
    members.Where(m => m.Torsion == "augmented-torsion" && m.A0 != "zero").All(m => m.InhomogeneityNorm > 1e-6);
bool trivialInhomogeneityZeroWhereExpected =
    members.Where(m => m.Torsion == "trivial").All(m => m.InhomogeneityNorm <= 1e-12);

// Canonical physical Shiab impossibility on the 2D toy (a recorded output).
const bool dimXAtLeastFour = false;              // toy geometry is dimX = 2
const bool spinorRealizedInvariantBasis = false; // no Cl(7,7)/128 metric-spinor basis on toy
const bool canonicalPhysicalShiabRealizableOnToy = false;

// The terminal frontier statement: the scale gap requires structure beyond the toy
// family - a dimX>=4 spinor-realized Shiab (Ricci/Weyl-like Sigma_mc) or a source
// anchor. True iff the universality verdicts hold (the toy family cannot close it).
bool scaleGapRequiresDimFourSpinorShiabOrSourceAnchor =
    noSaturationTheoremExtendsToEntireToyFamily && !canonicalPhysicalShiabRealizableOnToy;

bool batteriesAllPassed =
    precursorsPassed &&
    allCarrierMatchesPassed &&
    controlCrossAnchorMatchesPhase436 &&
    augmentedInhomogeneityNonzeroWhereExpected &&
    trivialInhomogeneityZeroWhereExpected &&
    upsilonDegreeTwoUniversalAcrossFamily &&
    hessianQuadraticDecompositionUniversalAcrossFamily;

// ---------------------------------------------------------------------------
// Fail-closed boundary (identical policy to Phases 436/440).
// ---------------------------------------------------------------------------

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool toyFamilyOnly = true;
const bool workbenchPolarizationConventionReused = true;
const bool noGevPromotion = true;
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
    $"su3 toy family {shiabBranches.Length}x{torsionBranches.Length}x{a0Specs.Length}={members.Count}; mesh {MeshRows}x{MeshCols}; carrier={carrierDimension} upsilon={upsilonDimension}",
    "Upsilon = S(F) - T degree-2 in omega; H(t)=H0+tH1+t^2H2; H2=J2^T J2 family-wide; no target values")))).ToLowerInvariant();

bool toyBranchFamilyUniversalitySweepPassed =
    precursorsPassed &&
    batteriesAllPassed &&
    noSaturationTheoremExtendsToEntireToyFamily &&
    scaleGapRequiresDimFourSpinorShiabOrSourceAnchor &&
    !canonicalPhysicalShiabRealizableOnToy &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    toyFamilyOnly &&
    workbenchPolarizationConventionReused &&
    noGevPromotion &&
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

string terminalStatus = toyBranchFamilyUniversalitySweepPassed
    ? "no-saturation-theorem-extends-to-entire-toy-family-scale-gap-requires-dim-four-spinor-shiab-or-source-anchor"
    : "toy-branch-family-universality-sweep-blocked";

string countVerdictPhrase = growingModeCountUniversalAcrossFamily
    ? "the growing-mode COUNT is invariant across the entire family (only mass VALUES rescale)"
    : "at least one family member changes the growing-mode COUNT (recorded honestly in the member table)";
string runawayVerdictPhrase = runawayVerdictUniversalAcrossFamily
    ? "the hypercharge-axis runaway verdict (net negative for derived content) holds for EVERY family member"
    : "at least one family member flips the hypercharge-axis runaway verdict (recorded honestly)";

string decision = toyBranchFamilyUniversalitySweepPassed
    ? $"The toy-branch family universality sweep is decided. Over the full {members.Count}-member Cartesian product of Shiab in {{identity-shiab, first-order-curvature, metric-scaled-shiab(lambda={MetricScale})}}, torsion in {{trivial, augmented-torsion, local-algebraic}}, and A0 in {{0, const lambda_8 (0.5, 1.0), mixed lambda_8+lambda_4}}, the residual Upsilon = S(F) - T is EXACTLY degree-2 in omega for every member (symmetric third field-difference vanishes to <= {DegreeTolerance:E0}), including the augmented-torsion members whose degree-0 A0 inhomogeneity and degree-1 free-kinetic part are recorded nonzero. Consequently the exact Hessian at every rank-1 background t*u is EXACTLY a degree-2 matrix polynomial H(t) = H0 + t*H1 + t^2*H2 (third t-difference of the exact Hessian vanishes to <= {DecompositionRelativeTolerance:E0} on sampled elements), so the Phase436 no-log-saturation theorem EXTENDS to the entire toy family: no realizable toy completion can produce a dynamical scale at one loop. This holds because along a single-Lie-direction background every quadratic bracket [u,u] vanishes, making U(t*u) affine in t so H2 = J2^T J2 exactly, and A0 cancels from J2 = J(u)-J(0) so the growing-mode structure is A0-independent. On the honest count/runaway questions: {countVerdictPhrase}; {runawayVerdictPhrase}. The control member (identity-shiab, trivial, A0=0) reproduces Phase436 exactly (counts 64/96; odd-in-t term vanishing for the lambda_8 Cartan direction and nonzero for the lambda_4 root direction). The draft's canonical physical Shiab (Sigma_mc, draft Section 32.2: a background algebraic contraction K_{{A0}} of Ricci/Weyl type) is NOT realizable on the 2D toy - it requires dimX >= 4 and a metric-spinor (Cl(7,7)/128) invariant basis, as MetricScaledShiabOperator's own comment records - so the terminal frontier statement holds: the scale gap requires structure beyond the toy family (a dimX>=4 spinor-realized Shiab or a source anchor). Everything is toy-family structure recorded blind against the workbench polarization/fermion-count convention; no scale, pole, or GeV lineage exists; no Phase201 or Phase256 field is filled; nothing is promoted."
    : "Do not use the family-universality verdicts until the precursors, carrier-match, control cross-anchor, inhomogeneity, and degree/decomposition batteries pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

var result = new
{
    phaseId = "phase441-toy-branch-family-universality-sweep",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    toyBranchFamilyUniversalitySweepPassed,
    phase436PrecursorPassed,
    phase440PrecursorPassed,
    precursorsPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    familyDefinition = new
    {
        lieAlgebraId = "su3",
        meshRows = MeshRows,
        meshCols = MeshCols,
        edgeCount,
        faceCount,
        carrierDimension,
        upsilonDimension,
        shiabBranches,
        torsionBranches,
        a0Backgrounds = a0Specs.Select(s => s.Label).ToArray(),
        metricScaleLambda = MetricScale,
        memberCount = members.Count,
        residualIdentity = "Upsilon = S(F) - T; F = d omega + (1/2)[omega wedge omega]; exactly degree-2 in omega for every member",
        rankOneBackground = "exact 1-form d(vertex y-coordinate) in a single Lie direction; [u,u]=0 => U(t*u) affine in t => H2 = J2^T J2; A0 cancels from J2",
    },
    universalityVerdicts = new
    {
        upsilonDegreeTwoUniversalAcrossFamily,
        hessianQuadraticDecompositionUniversalAcrossFamily,
        noSaturationTheoremExtendsToEntireToyFamily,
        growingModeCountUniversalAcrossFamily,
        runawayVerdictUniversalAcrossFamily,
        canonicalPhysicalShiabRealizableOnToy,
        scaleGapRequiresDimFourSpinorShiabOrSourceAnchor,
    },
    batteries = new
    {
        allCarrierMatchesPassed,
        controlCrossAnchorMatchesPhase436,
        controlCountsMatchPhase436,
        controlOddTermVanishesForCartan,
        controlOddTermNonzeroForRoot,
        controlLambda8GrowingCount = control.Lambda8GrowingCount,
        controlLambda4GrowingCount = control.Lambda4GrowingCount,
        controlLambda8OddTerm = control.Lambda8OddTerm,
        controlLambda4OddTerm = control.Lambda4OddTerm,
        augmentedInhomogeneityNonzeroWhereExpected,
        trivialInhomogeneityZeroWhereExpected,
        degreeTolerance = DegreeTolerance,
        decompositionRelativeTolerance = DecompositionRelativeTolerance,
        sampledHessianPairs = SampledHessianPairs,
        degreeProbeCount = DegreeProbeCount,
        batteriesAllPassed,
    },
    familyTable = members.Select(m => new
    {
        index = m.Index,
        shiab = m.Shiab,
        torsion = m.Torsion,
        a0 = m.A0,
        metricScaleLambda = m.MetricScaleLambda,
        validateCarrierMatchPassed = m.ValidateCarrierMatchPassed,
        upsilonDegreeMaxThirdDifference = m.DegreeMaxThirdDiff,
        upsilonDegreeTwo = m.UpsilonDegreeTwo,
        degree0InhomogeneityNorm = m.InhomogeneityNorm,
        degree1LinearPartNorm = m.LinearPartNorm,
        hessianLambda8MaxThirdTDifference = m.Lambda8MaxThirdTDiff,
        hessianLambda4MaxThirdTDifference = m.Lambda4MaxThirdTDiff,
        hessianQuadraticDecomposition = m.HessianQuadratic,
        lambda8OddInTTerm = m.Lambda8OddTerm,
        lambda4OddInTTerm = m.Lambda4OddTerm,
        lambda8GrowingModeCount = m.Lambda8GrowingCount,
        lambda4GrowingModeCount = m.Lambda4GrowingCount,
        lambda8MaxH2Eigenvalue = m.Lambda8MaxH2Eig,
        lambda4MaxH2Eigenvalue = m.Lambda4MaxH2Eig,
        bosonicHyperchargeSlope = m.BosonicHyperchargeSlope,
        netDerivedSlope = m.NetDerivedSlope,
        runaway = m.Runaway,
    }).ToArray(),
    balanceConvention = new
    {
        fermionicDerivedHyperchargeCoupledCount = FermionicDerivedHyperchargeCoupledCount,
        bosonPolarizationConvention = BosonPolarizationConvention,
        netSlopeFormula = "netDerivedSlope = bosonPolarizations * lambda8GrowingCount - fermionicDerivedCount",
        controlNetDerivedSlope = control.NetDerivedSlope,
        phase440ReferenceNetSDerived = -640,
        note = "Reuses the Phase430/440 workbench polarization and derived fermion-count convention; per-volume slope sign only.",
    },
    canonicalShiabImpossibility = new
    {
        canonicalPhysicalShiabRealizableOnToy,
        dimXAtLeastFour,
        spinorRealizedInvariantBasis,
        requirements = new[] { "dimXAtLeastFour", "spinorRealizedInvariantBasis" },
        sourceEvidence = new[]
        {
            $"{MetricScaledShiabSourcePath} lines 15-18: 'On the current toy geometry (dimX=2), Lambda^2(T*X) is 1-dimensional, so scalar scaling is the only available distinguishing Shiab variant. Richer contractions (Ricci/Weyl decompositions) require dimX >= 4 per physicist guidance.'",
            $"{DraftSourcePath} Section 32.2, lines 1605-1627 (Inserted Choice IX.32.2.1): the canonical active Shiab branch Sigma_mc := Pi_T(K_{{A0}}(d_{{B_omega}} Xi)) + L_{{A0}}(T^aug, Xi) requires a background algebraic contraction K_{{A0}} from the Einsteinian projection + metric/bundle data (Ricci/Weyl-like, second-order), not realizable on the 2D toy where Lambda^2 is 1-dimensional and no Cl(7,7)/128 metric-spinor invariant basis exists.",
        },
    },
    honestBoundaries = new
    {
        toyFamilyOnly,
        workbenchPolarizationConventionReused,
        noGevPromotion,
    },
    runtimeSeconds,
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
        phase436SummaryPath = Phase436SummaryPath,
        phase440SummaryPath = Phase440SummaryPath,
        metricScaledShiabSourcePath = MetricScaledShiabSourcePath,
        draftSourcePath = DraftSourcePath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The family Hessians, H2 spectra, growing-mode counts, and net-slope signs are toy-family structure data, not physical mass spectra.",
        "The bosonic polarization convention and the derived fermion count (768) are recorded Phase430/440 conventions reused for the balance sign only.",
        "The no-saturation result extends across the toy family; a finite scale still requires structure beyond the toy family (a dimX>=4 spinor-realized canonical Shiab or a source anchor) that no reviewed source realizes on the toy.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 fill; no physical predictions.",
    },
    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "toy_branch_family_universality_sweep.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "toy_branch_family_universality_sweep_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"toyBranchFamilyUniversalitySweepPassed={toyBranchFamilyUniversalitySweepPassed}");
Console.WriteLine($"precursors: p436={phase436PrecursorPassed} p440={phase440PrecursorPassed}");
Console.WriteLine($"mesh: edges={edgeCount} faces={faceCount} carrier={carrierDimension} upsilon={upsilonDimension}; members={members.Count}");
Console.WriteLine($"upsilonDegreeTwoUniversal={upsilonDegreeTwoUniversalAcrossFamily} hessianQuadraticUniversal={hessianQuadraticDecompositionUniversalAcrossFamily}");
Console.WriteLine($"noSaturationTheoremExtendsToEntireToyFamily={noSaturationTheoremExtendsToEntireToyFamily}");
Console.WriteLine($"growingModeCountUniversal={growingModeCountUniversalAcrossFamily} runawayVerdictUniversal={runawayVerdictUniversalAcrossFamily}");
Console.WriteLine($"control cross-anchor (64/96, odd8~0, odd4>0)={controlCrossAnchorMatchesPhase436} counts={control.Lambda8GrowingCount}/{control.Lambda4GrowingCount} odd8={control.Lambda8OddTerm:E2} odd4={control.Lambda4OddTerm:E2}");
Console.WriteLine($"augmentedInhomogNonzero={augmentedInhomogeneityNonzeroWhereExpected} trivialInhomogZero={trivialInhomogeneityZeroWhereExpected}");
Console.WriteLine($"canonicalPhysicalShiabRealizableOnToy={canonicalPhysicalShiabRealizableOnToy} scaleGapRequiresDimFourSpinorShiabOrSourceAnchor={scaleGapRequiresDimFourSpinorShiabOrSourceAnchor}");
Console.WriteLine($"batteriesAllPassed={batteriesAllPassed} runtimeSeconds={runtimeSeconds:F2}");
Console.WriteLine("--- distinct (shiab,torsion) count signatures (A0-independent) ---");
foreach (var g in members.GroupBy(m => (m.Shiab, m.Torsion)))
{
    var m = g.First();
    Console.WriteLine($"  {m.Shiab,-22} {m.Torsion,-18} counts={m.Lambda8GrowingCount}/{m.Lambda4GrowingCount} maxH2={m.Lambda8MaxH2Eig:F4}/{m.Lambda4MaxH2Eig:F4} net8={m.NetDerivedSlope} runaway={m.Runaway}");
}
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

static double Norm(double[] v)
{
    double s = 0.0;
    foreach (double x in v) s += x * x;
    return Math.Sqrt(s);
}

double[] Combine(double[] basePoint, double[] dir, double s)
{
    var r = new double[basePoint.Length];
    for (int i = 0; i < r.Length; i++)
        r[i] = basePoint[i] + s * dir[i];
    return r;
}

static double[,] GramFromColumns(double[][] columns, int dim)
{
    var g = new double[dim, dim];
    foreach (var col in columns)
        for (int a = 0; a < dim; a++)
        {
            double va = col[a];
            if (va == 0.0) continue;
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
    for (int i = 0; i < n; i++) vectors[i, i] = 1.0;
    for (int sweep = 0; sweep < 400; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (Math.Sqrt(off) < 1e-12) break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double apq = a[p, q];
                if (Math.Abs(apq) < 1e-15) continue;
                double app = a[p, p], aqq = a[q, q];
                double tau = (aqq - app) / (2.0 * apq);
                double t = Math.Sign(tau == 0 ? 1.0 : tau) / (Math.Abs(tau) + Math.Sqrt(1.0 + tau * tau));
                double c = 1.0 / Math.Sqrt(1.0 + t * t);
                double s = t * c;
                for (int k = 0; k < n; k++)
                {
                    if (k == p || k == q) continue;
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
    for (int i = 0; i < n; i++) values[i] = a[i, i];
    return (values, vectors);
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

public sealed record DirectionResult(
    int Direction, string Label, double MaxThirdTDiff, double RelativeOddTerm,
    int GrowingModeCount, double MaxH2Eigenvalue);

public sealed record MemberRecord(
    int Index, string Shiab, string Torsion, string A0, double MetricScaleLambda,
    bool ValidateCarrierMatchPassed, double DegreeMaxThirdDiff, bool UpsilonDegreeTwo,
    double InhomogeneityNorm, double LinearPartNorm,
    double Lambda8MaxThirdTDiff, double Lambda4MaxThirdTDiff, bool HessianQuadratic,
    double Lambda8OddTerm, double Lambda4OddTerm,
    int Lambda8GrowingCount, int Lambda4GrowingCount, double Lambda8MaxH2Eig, double Lambda4MaxH2Eig,
    int BosonicHyperchargeSlope, int NetDerivedSlope, bool Runaway);
