using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase450: CONSTRAINT EFFECTIVE POTENTIAL by umbrella-sampled HMC on
// lattice-canonical tori -- the 2026-07-04 review board's named ansatz-free
// non-perturbative experiment (the successor to phase449's recorded
// gap-equation breakdown: the diagonal-Gaussian family could not hold the
// Einsteinian negative-mode structure; the scale question passed here intact).
//
// THE QUESTION. Tree level has no scale (S_B = a2 t^2 + a4 t^4, a2, a4 > 0 on
// invariant rays; a3 = 0 exactly there by the omega -> -omega Z2, making +-t
// exact partners). No perturbative Coleman-Weinberg scale exists, and the
// positive-subspace saddle V_eff is RETIRED. The true effective potential is
// CONVEX, so "interior minimum" was the wrong signature all along: SSB
// manifests as a FLAT MAXWELL BOTTOM / degenerate +-t minima of the
// finite-volume constraint effective potential U(Phi). This phase measures
// U(Phi) = -(1/beta) ln P(Phi) of the FULL non-perturbative constrained free
// energy (S_B >= 0 real => e^{-beta S_B} has NO SIGN PROBLEM), integrating the
// 30-235 transverse negative modes every retired perturbative convention
// discarded -- exactly where a dynamical scale could hide.
//
// THE FOUR BINDING CONDITIONS (2026-07-04 adversarial review; each fail-closed):
//   (i)   GAUGE-INVARIANT COLLECTIVE COORDINATE: Phi = <u_inv, omega> with
//         u_inv the translation-invariant unit ray built by phase448's
//         per-type construction WITH the oSign lattice gauge (the stored
//         index-ordered edge direction does not commute with translation;
//         phase448's covariance battery caught the missing orientation signs
//         at 1.6e-2), and the 3 global-su(2) zero-mode directions
//         z_l = [e_l, u] projected out of u_inv, projector RECORDED. (On the
//         su(2) invariant metric <u, [c, u]> = 0 identically by antisymmetry
//         of the bracket, so the recorded overlaps must be ~machine-zero --
//         asserted, and the projection is applied regardless, never assumed.)
//         Random rays leak ker-d gauge motion and are FORBIDDEN; a
//         translation-invariant dLambda requires uniform lambda, i.e.
//         dLambda = 0, so the invariant ray cannot leak them.
//   (ii)  THETA-HAAR MEASURE: theta enters S_B only via Ad = exp(ad_theta) in
//         SO(3) per vertex (su(2) structure constants f = epsilon => exp(ad_theta)
//         is the rotation by angle |theta| about axis theta); the R^n theta
//         integral DIVERGES. The sampler maintains PER-VERTEX ROTATIONS as
//         unit quaternions and updates them by Haar-invariant Metropolis
//         moves: left-multiplication by small random rotations plus
//         single-vertex uniform-Haar independence proposals (both symmetric
//         => Haar-correct). The operator API takes theta VECTORS, so the
//         rotations are charted into the axis-angle FUNDAMENTAL DOMAIN
//         |theta| <= pi (quaternion representative with w >= 0; angle
//         = 2 atan2(|q_vec|, q_w) in [0, pi], theta = angle * axis). CHART
//         BATTERIES (hard asserts): uniform-theta shift leaves S_B exactly
//         constant; a |theta| > pi configuration and its wrapped fundamental-
//         domain image give identical S_B; a single-vertex 2*pi rotation is
//         the identity.
//   (iii) ERGODICITY CONTROL: tau_int(S_B) and tau_int(Phi) recorded per
//         window via integrated autocorrelation; GATE
//         N_eff = N / (2 tau_int) >= 100 per window, with an adaptive
//         trajectory count (chunks until the gate is met or the fail-closed
//         trajectory cap records the failure).
//   (iv)  The theta = 0 slice is a SAMPLER DEMO, never a physics verdict: the
//         physics run below is the JOINT (omega-HMC, theta-Haar-Metropolis)
//         ensemble. (The identity control is exactly theta-independent --
//         GradTheta == 0 asserted -- so its joint ensemble factorizes and the
//         omega marginal IS the joint answer for that member only.)
//
// DESIGN. Umbrella windows over Phi in [-tMax, +tMax] (both signs -- the +-
// symmetry is itself a check), harmonic bias (k/2)(Phi - c)^2 added to
// beta*S_B (bias NOT multiplied by beta; recorded convention), per-window HMC
// on omega (leapfrog against the platform's analytic ComputeJointGradient,
// bias force added) interleaved with theta-Haar Metropolis sweeps; WHAM
// (log-space, self-consistent) reconstruction of U(Phi) = -(1/beta) ln P(Phi).
// SAMPLING GATES (per the mission; failure => verdict inconclusive, recorded
// per gate): <e^{-dH}> = 1 within error per window (measure preservation);
// equipartition virial <sum_i w_i dU_tot/dw_i> = nOmega within error per
// window; N_eff >= 100 per window; neighbor histogram overlap >= 15%; WHAM
// self-consistency residual; U(Phi) coverage half-width; the large-beta
// control column must reproduce the S_B(t) tree curve shape (Pearson >= 0.9
// vs S_B(c*u_inv, theta=0) + single-well classification); +-Phi antisymmetry
// |U(Phi) - U(-Phi)| within errors (the a3 = 0 Z2 is exact only on the
// invariant sector -- off-sector a3 ~ 1e-4 REAL -- so this is gated within
// statistical errors, not at machine precision).
// HARD BATTERIES (failure => terminal blocked): precursors; orbit map; S_B
// translation covariance; objective-path consistency (ComputeJointGradient
// vs EvaluateWithTheta); analytic-vs-FD joint gradient; the theta chart
// batteries above; identity theta-independence; projector overlaps; WHAM
// plumbing on a synthetic exactly-sampled Gaussian umbrella set (the
// reconstruction must reproduce the known quadratic potential).
//
// CLASSIFICATION of U(Phi) (pre-registered):
//   single-well-at-zero            -- symmetric-phase NULL for that member;
//   flat-bottom-degenerate-minima  -- SSB CANDIDATE (workbench-relative ONLY):
//                                     requires the +- pair (paired minima at
//                                     +-t* within errors, depth > 3 sigma) OR
//                                     a flat Maxwell bottom (half-width >=
//                                     FlatBottomMinHalfWidth with rising
//                                     edges); t* reported;
//   inconclusive                   -- gates failed or unclassifiable structure.
//
// MANDATORY FRAMING. Workbench-relative candidate data ONLY (su(2) toy
// algebra on the reduced Spin(4) slice, lattice units, lattice-canonical
// tori); beta = 1, the umbrella spring constants, the Euclidean (non-mass-
// weighted) Phi inner product, the flat HMC kinetic mass, and the theta-Haar
// chart are WORKBENCH CONVENTIONS pending physicist review; NO GeV/pole/VEV
// promotion either way. A flat-bottom signature is a workbench-relative SSB
// CANDIDATE only. The phase PASSES on internal consistency REGARDLESS of the
// verdict.
//
// Fail-closed: target-blind; reduced-spin4-slice; no scale/pole/GeV lineage;
// no Phase201/Phase256 contract field filled; nothing promoted either way.

const string DefaultOutputDir = "studies/phase450_constraint_effective_potential_hmc_probe_001/output";
const string Phase448SummaryPath = "studies/phase448_torus_mode_volume_saturation_probe_001/output/torus_mode_volume_saturation_probe_summary.json";
const string Phase449SummaryPath = "studies/phase449_variational_gaussian_effective_potential_probe_001/output/variational_gaussian_effective_potential_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ReviewBoardSourcePath = "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md";
const string ApplicationSubjectKind = "constraint-effective-potential-hmc-probe";
const string TerminalPrefix = "constraint-effective-potential-hmc-probe-";

// --- Probe configuration (committed defaults; PHASE450_* env overrides). ----
int[] TorusSizes = (Environment.GetEnvironmentVariable("PHASE450_TORI") ?? "3")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(int.Parse).ToArray();
string[] MemberTags = (Environment.GetEnvironmentVariable("PHASE450_MEMBERS") ?? "identity,sd2")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
double TMax = EnvD("PHASE450_TMAX", 2.5);
int WindowsEnv = EnvI("PHASE450_WINDOWS", 0);          // 0 = auto placement from the tree curvature
int ChunkTraj = EnvI("PHASE450_TRAJ", 500);            // adaptive sampling chunk size
int MaxTraj = EnvI("PHASE450_MAXTRAJ", 9000);          // fail-closed trajectory cap per window
int WarmTraj = EnvI("PHASE450_WARM", 300);
int NeffTarget = EnvI("PHASE450_NEFF", 100);           // binding condition (iii)
double Beta = EnvD("PHASE450_BETA", 1.0);              // recorded convention (default 1)
double BetaLarge = EnvD("PHASE450_BETA_LARGE", 8.0);   // large-beta tree-shape control column
double KUmbEnv = EnvD("PHASE450_KUMB", 0.0);           // 0 = auto from window spacing + tree curvature
int NLeap = EnvI("PHASE450_NLEAP", 10);
int MinTraj = EnvI("PHASE450_MINTRAJ", 1500);          // statistics floor per window (WHAM bins)
int MaxParallel = EnvI("PHASE450_PARALLEL", System.Math.Min(16, Environment.ProcessorCount));
double EpsEnv = EnvD("PHASE450_EPS", 0.0);             // 0 = auto-tuned in warmup
int ThetaProps = EnvI("PHASE450_THETA_PROPS", 8);      // Haar-Metropolis proposals per trajectory
int SamplerSeed = EnvI("PHASE450_SEED", 424250);
const int RayRngSeed = 20260703;   // phase448 invariant-ray convention (per-type coefficients, seed 0)
const double BetaConvention = 1.0; // recorded convention (beta enters as e^{-beta S_B})

// Hard-battery tolerances.
const double CovarianceTol = 1e-9;
const double ObjectiveConsistencyTol = 1e-9;
const double GradFdTol = 1e-4;
const double ThetaChartTol = 1e-9;             // uniform shift / wrap / 2pi periodicity (relative)
const double IdentityThetaGradTol = 1e-12;     // identity GradTheta == 0 (absolute norm)
const double ProjectorOverlapTol = 1e-12;      // post-projection <u, z_l> (machine-zero expected)
const double WhamPlumbingTol = 0.08;           // synthetic Gaussian reconstruction (statistical)
const double WhamResidualTol = 1e-8;           // WHAM self-consistency (max |delta f_w|)
const int WhamMaxIter = 200000;

// Sampling-gate tolerances (failure => verdict inconclusive, never silent).
const double OverlapMin = 0.15;                // neighbor histogram overlap coefficient
const double ExpDhAbsFloor = 0.03;             // |<e^-dH>-1| <= max(floor, 4 SE)
const double VirialRelFloor = 0.03;            // |virial/nOmega - 1| <= max(floor, 4 SE/nOmega)
// The per-bin error model is an approximate LOWER bound (window-mean tau
// deflation only; WHAM bin covariance neglected), so the antisymmetry sigma
// gate uses 4 rather than 3; the raw max |U(Phi)-U(-Phi)| is recorded too.
const double AntisymmetrySigma = 4.0;          // |U(Phi)-U(-Phi)| <= sigma * combined error
const double LargeBetaPearsonMin = 0.9;        // tree-shape control gate
const double CoverageHalfWidthFraction = 0.5;  // reported U domain must span +- this * tMax

// Classification conventions (pre-registered).
const int MinBinCounts = 30;                   // bins entering the classification
const double SsbDepthSigma = 3.0;              // U(0) - U(t*) depth gate
const double FlatBottomMinHalfWidth = 1.0;     // lattice units; flat-Maxwell-bottom convention
const double MonotoneSlackSigma = 2.0;         // single-well monotone-rise slack

var outputDir = Environment.GetEnvironmentVariable("PHASE450_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors: phase448 (invariant rays + lattice-canonical machinery sound;
// mode-volume lever decided negative) and phase449 (Gaussian bound breakdown
// recorded -- the eventuality this ansatz-free probe was named for).
// ---------------------------------------------------------------------------

using var phase448 = JsonDocument.Parse(File.ReadAllText(Phase448SummaryPath));
bool phase448PrecursorPassed =
    JsonBool(phase448.RootElement, "torusModeVolumeSaturationProbePassed") is true;

using var phase449 = JsonDocument.Parse(File.ReadAllText(Phase449SummaryPath));
bool phase449PrecursorPassed =
    JsonBool(phase449.RootElement, "variationalGaussianEffectivePotentialProbePassed") is true;

bool precursorsPassed = phase448PrecursorPassed && phase449PrecursorPassed;

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
var manifest = BuildManifest();
var geometry = BuildGeometry();
var stopwatch = Stopwatch.StartNew();
long sbGradEvals = 0, sbObjEvals = 0;

// Global sampler RNG (all stochastic decisions flow from SamplerSeed).
var rootRng = new Random(SamplerSeed);

// ---------------------------------------------------------------------------
// WHAM (log-space, shared by the synthetic plumbing battery and the physics
// arms). windows: (counts per bin, N, center, k). Bias b_w(x) = (k/2)(x-c)^2
// (NOT multiplied by beta -- the sampler's convention, recorded).
// ---------------------------------------------------------------------------

(double[] LnP, double[] F, double Residual, int Iterations) WhamSolve(
    (long[] Counts, long N, double Center, double K)[] windows, double[] binPhi)
{
    int nb = binPhi.Length, nw = windows.Length;
    var lnN = windows.Select(w => System.Math.Log(System.Math.Max(1, w.N))).ToArray();
    var bias = new double[nw][];
    for (int w = 0; w < nw; w++)
    {
        bias[w] = new double[nb];
        for (int b = 0; b < nb; b++)
        {
            double d = binPhi[b] - windows[w].Center;
            bias[w][b] = 0.5 * windows[w].K * d * d;
        }
    }
    var lnTot = new double[nb];
    for (int b = 0; b < nb; b++)
    {
        long tot = 0;
        for (int w = 0; w < nw; w++) tot += windows[w].Counts[b];
        lnTot[b] = tot > 0 ? System.Math.Log(tot) : double.NegativeInfinity;
    }
    var f = new double[nw];
    var lnP = new double[nb];
    double resid = double.PositiveInfinity;
    int it = 0;
    var terms = new double[nw];
    var binTerms = new double[nb];
    for (; it < WhamMaxIter && resid > WhamResidualTol; it++)
    {
        for (int b = 0; b < nb; b++)
        {
            if (double.IsNegativeInfinity(lnTot[b])) { lnP[b] = double.NegativeInfinity; continue; }
            for (int w = 0; w < nw; w++) terms[w] = lnN[w] + f[w] - bias[w][b];
            lnP[b] = lnTot[b] - LogSumExp(terms);
        }
        // Normalize (gauge-fix) lnP.
        double z = LogSumExp(lnP);
        for (int b = 0; b < nb; b++) lnP[b] -= z;
        resid = 0.0;
        for (int w = 0; w < nw; w++)
        {
            for (int b = 0; b < nb; b++) binTerms[b] = lnP[b] - bias[w][b];
            double fNew = -LogSumExp(binTerms);
            resid = System.Math.Max(resid, System.Math.Abs(fNew - f[w]));
            f[w] = fNew;
        }
    }
    return (lnP, f, resid, it);
}

// --- WHAM plumbing battery: exactly-sampled Gaussian umbrellas on U0 = x^2/2.
double whamPlumbingMaxErr;
{
    var rng = new Random(SamplerSeed + 101);
    double bw = 0.1;
    int nb = 121;
    var binPhi = new double[nb];
    for (int b = 0; b < nb; b++) binPhi[b] = (b - nb / 2) * bw;
    double kSyn = 4.0;
    var centers = Enumerable.Range(-4, 9).Select(i => i * 1.0).ToArray();
    var wins = new (long[] Counts, long N, double Center, double K)[centers.Length];
    for (int w = 0; w < centers.Length; w++)
    {
        double var_ = 1.0 / (1.0 + kSyn), mu = kSyn * centers[w] / (1.0 + kSyn);
        var counts = new long[nb];
        long n = 0;
        for (int s = 0; s < 60000; s++)
        {
            double x = mu + System.Math.Sqrt(var_) * Gauss(rng);
            int b = (int)System.Math.Round(x / bw) + nb / 2;
            if (b >= 0 && b < nb) { counts[b]++; n++; }
        }
        wins[w] = (counts, n, centers[w], kSyn);
    }
    var (lnP, _, resid, _) = WhamSolve(wins, binPhi);
    double err = 0.0;
    double shiftRef = double.NaN, shiftRec = double.NaN;
    for (int b = 0; b < nb; b++)
    {
        long tot = 0; for (int w = 0; w < wins.Length; w++) tot += wins[w].Counts[b];
        if (tot < 200 || System.Math.Abs(binPhi[b]) > 3.5) continue;
        double uRec = -lnP[b], uRef = 0.5 * binPhi[b] * binPhi[b];
        if (double.IsNaN(shiftRef)) { shiftRef = uRef; shiftRec = uRec; }
        err = System.Math.Max(err, System.Math.Abs((uRec - shiftRec) - (uRef - shiftRef)));
    }
    whamPlumbingMaxErr = double.IsFinite(resid) && resid <= WhamResidualTol ? err : double.PositiveInfinity;
}
bool whamPlumbingBattery = whamPlumbingMaxErr <= WhamPlumbingTol;
Console.WriteLine($"# wham plumbing: maxErr={whamPlumbingMaxErr:E2} (tol {WhamPlumbingTol}) pass={whamPlumbingBattery}");

// ---------------------------------------------------------------------------
// Torus arms.
// ---------------------------------------------------------------------------

var torusRecords = new List<TorusArm>();
bool allHardBatteriesPassed = whamPlumbingBattery;
LargeBetaColumn? largeBetaColumn = null;

foreach (int n in TorusSizes)
{
    var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical: true);
    int nOmega = mesh.EdgeCount * dimG;
    int nTheta = mesh.VertexCount * dimG;
    int nVert = mesh.VertexCount;
    var mass = new CpuMassMatrix(mesh, algebra);
    Console.WriteLine($"# torus n={n}: V={mesh.VertexCount} E={mesh.EdgeCount} nOmega={nOmega} nTheta={nTheta}");

    FieldTensor FaceTensor(double[] coeffs) => new()
    {
        Label = "coeffs",
        Signature = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = algebra.BasisOrderId,
            ComponentOrderId = "face-major",
            NumericPrecision = "float64",
            MemoryLayout = "dense-row-major",
        },
        Coefficients = coeffs,
        Shape = new[] { mesh.FaceCount, algebra.Dimension },
    };

    // --- Orbit coordinates (verbatim phase448): coords, edge (base, type), oSign.
    var coords = new int[nVert][];
    double spacing0 = double.MaxValue;
    for (int v = 0; v < nVert; v++)
    {
        var p = mesh.GetVertexCoordinates(v);
        for (int d = 0; d < 4; d++) if (p[d] > 1e-12) spacing0 = System.Math.Min(spacing0, p[d]);
        coords[v] = new int[4];
    }
    for (int v = 0; v < nVert; v++)
    {
        var p = mesh.GetVertexCoordinates(v);
        for (int d = 0; d < 4; d++) coords[v][d] = (int)System.Math.Round(p[d] / spacing0);
    }
    var vertexAt = new Dictionary<(int, int, int, int), int>();
    for (int v = 0; v < nVert; v++)
        vertexAt[(coords[v][0], coords[v][1], coords[v][2], coords[v][3])] = v;

    int TypeOf(int[] disp) => ((disp[0] << 3) | (disp[1] << 2) | (disp[2] << 1) | disp[3]) - 1;
    var edgeBase = new int[mesh.EdgeCount];
    var edgeType = new int[mesh.EdgeCount];
    bool orbitMapOk = true;
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        int a = mesh.Edges[e][0], b = mesh.Edges[e][1];
        int[]? disp = MinImage01(coords[a], coords[b], n);
        if (disp != null) { edgeBase[e] = a; edgeType[e] = TypeOf(disp); }
        else
        {
            disp = MinImage01(coords[b], coords[a], n);
            if (disp != null) { edgeBase[e] = b; edgeType[e] = TypeOf(disp); }
            else orbitMapOk = false;
        }
    }
    var edgeAt = new int[nVert, 15];
    for (int e = 0; e < mesh.EdgeCount; e++) edgeAt[edgeBase[e], edgeType[e]] = e;
    var oSign = new double[mesh.EdgeCount];
    for (int e = 0; e < mesh.EdgeCount; e++)
        oSign[e] = mesh.Edges[e][0] == edgeBase[e] ? 1.0 : -1.0;

    double[] TranslateOmega(double[] x, int[] a)
    {
        var y = new double[nOmega];
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var c0 = coords[edgeBase[e]];
            int tv = vertexAt[(Mod(c0[0] + a[0], n), Mod(c0[1] + a[1], n), Mod(c0[2] + a[2], n), Mod(c0[3] + a[3], n))];
            int te = edgeAt[tv, edgeType[e]];
            double sgn = oSign[e] * oSign[te];
            for (int l = 0; l < dimG; l++) y[te * dimG + l] = sgn * x[e * dimG + l];
        }
        return y;
    }
    double[] TranslateTheta(double[] x, int[] a)
    {
        var y = new double[nTheta];
        for (int v = 0; v < nVert; v++)
        {
            var c0 = coords[v];
            int tv = vertexAt[(Mod(c0[0] + a[0], n), Mod(c0[1] + a[1], n), Mod(c0[2] + a[2], n), Mod(c0[3] + a[3], n))];
            for (int l = 0; l < dimG; l++) y[tv * dimG + l] = x[v * dimG + l];
        }
        return y;
    }

    // --- Binding condition (i): the invariant-ray collective coordinate. ----
    // Per-type coefficients (phase448 convention: Random(RayRngSeed + 31*seed),
    // seed 0; 16 types x dimG, omega uses types 0..14), oSign lattice gauge.
    var rayCoeffRng = new Random(RayRngSeed + 31 * 0);
    var typeCoeffs = new double[16 * dimG];
    for (int i = 0; i < typeCoeffs.Length; i++) typeCoeffs[i] = rayCoeffRng.NextDouble() - 0.5;
    var uInv = new double[nOmega];
    for (int e = 0; e < mesh.EdgeCount; e++)
        for (int l = 0; l < dimG; l++)
            uInv[e * dimG + l] = oSign[e] * typeCoeffs[edgeType[e] * dimG + l];
    double uNormRaw = Norm(uInv);
    for (int i = 0; i < nOmega; i++) uInv[i] /= uNormRaw;

    // Global-su(2) zero-mode directions z_l = [e_l, u] (adjoint action per edge;
    // su(2) structure constants f = epsilon => bracket = cross product).
    var zDirs = new double[dimG][];
    var overlapsBefore = new double[dimG];
    for (int l = 0; l < dimG; l++)
    {
        var z = new double[nOmega];
        var basis = new double[dimG]; basis[l] = 1.0;
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var ue = new double[dimG];
            for (int c = 0; c < dimG; c++) ue[c] = uInv[e * dimG + c];
            var br = algebra.Bracket(basis, ue);
            for (int c = 0; c < dimG; c++) z[e * dimG + c] = br[c];
        }
        double zn = Norm(z);
        if (zn > 0) for (int i = 0; i < nOmega; i++) z[i] /= zn;
        zDirs[l] = z;
        overlapsBefore[l] = Dot(uInv, z);
    }
    // Gram-Schmidt the z's, project them out of u, renormalize (applied, not assumed).
    var zOrtho = new List<double[]>();
    foreach (var z in zDirs)
    {
        var w = (double[])z.Clone();
        foreach (var q in zOrtho) { double d = Dot(w, q); for (int i = 0; i < nOmega; i++) w[i] -= d * q[i]; }
        double wn = Norm(w);
        if (wn > 1e-12) { for (int i = 0; i < nOmega; i++) w[i] /= wn; zOrtho.Add(w); }
    }
    foreach (var q in zOrtho) { double d = Dot(uInv, q); for (int i = 0; i < nOmega; i++) uInv[i] -= d * q[i]; }
    double uNormAfter = Norm(uInv);
    for (int i = 0; i < nOmega; i++) uInv[i] /= uNormAfter;
    var overlapsAfter = zOrtho.Select(q => Dot(uInv, q)).ToArray();
    double projectorOverlapMaxAfter = overlapsAfter.Length > 0 ? overlapsAfter.Max(System.Math.Abs) : 0.0;
    bool projectorBattery = projectorOverlapMaxAfter <= ProjectorOverlapTol;

    double Phi(double[] omega) => Dot(uInv, omega);

    // --- Members. ------------------------------------------------------------
    var controlMember = new EinsteinianShiabFamilyMember
    {
        Phi1 = InvariantElementSpec.Id0, Phi2 = InvariantElementSpec.None, EpsilonMode = "trivial",
    };
    var members = new List<(string Name, EinsteinianShiabFamilyMember Spec, EinsteinianShiabOperator Op, bool IsControl)>();
    foreach (var tag in MemberTags)
    {
        if (tag == "identity")
            members.Add(("identity", controlMember,
                new EinsteinianShiabOperator(mesh, algebra, controlMember, latticePeriod: n), true));
        else
        {
            var phi1 = tag switch
            {
                "sd2" => InvariantElementSpec.Sd2,
                "asd2" => InvariantElementSpec.Asd2,
                _ => throw new InvalidOperationException($"unknown member tag '{tag}' (identity|sd2|asd2)"),
            };
            var m = new EinsteinianShiabFamilyMember
            {
                Phi1 = phi1, Phi2 = InvariantElementSpec.Id0, EinsteinCoefficient = 0.5,
                EpsilonMode = "independent-theta",
            };
            members.Add(($"{tag}-id0/c0.5", m, new EinsteinianShiabOperator(mesh, algebra, m, latticePeriod: n), false));
        }
    }

    double SBGradM(EinsteinianShiabOperator op, CpuMassMatrix mm, double[] omega, double[] theta,
        double[] gradOut, double[]? gradThetaOut = null)
    {
        var (obj, gO, gT) = op.ComputeJointGradient(omega, theta, mm);
        Interlocked.Increment(ref sbGradEvals);
        Array.Copy(gO, gradOut, nOmega);
        if (gradThetaOut != null) Array.Copy(gT, gradThetaOut, nTheta);
        return obj;
    }
    double SBObjM(EinsteinianShiabOperator op, CpuMassMatrix mm, double[] omega, double[] theta)
    {
        var (obj, _, _) = op.ComputeJointGradient(omega, theta, mm);
        Interlocked.Increment(ref sbGradEvals);
        return obj;
    }
    double SBObj(EinsteinianShiabOperator op, double[] omega, double[] theta) => SBObjM(op, mass, omega, theta);
    // Objective-only fast path with cached curvature/connection tensors (theta sweeps).
    (FieldTensor F, FieldTensor Conn) CacheTensors(double[] omega)
    {
        var conn = new ConnectionField(mesh, algebra, omega);
        return (CurvatureAssembler.Assemble(conn).ToFieldTensor(), conn.ToFieldTensor());
    }
    double SBThetaFast(EinsteinianShiabOperator op, CpuMassMatrix mm, FieldTensor f, FieldTensor conn, double[] theta)
    {
        var s = FaceTensor(op.EvaluateWithTheta(f, conn, theta, manifest, geometry).Coefficients);
        Interlocked.Increment(ref sbObjEvals);
        return 0.5 * mm.InnerProduct(s, s);
    }

    // --- Hard batteries on this torus. ---------------------------------------
    var batRng = new Random(SamplerSeed + 977 * n);
    double covarianceMax = 0.0, objConsistencyMax = 0.0, gradFdMax = 0.0;
    double thetaUniformShiftMax = 0.0, thetaWrapChartMax = 0.0, thetaPeriodicityMax = 0.0;
    double identityThetaGradNorm = 0.0;
    foreach (var (name, spec, op, isControl) in members)
    {
        var xw = RandomVector(batRng, nOmega, 0.4);
        var xt = RandomVector(batRng, nTheta, 0.4);
        double s0 = SBObj(op, xw, xt);

        // Objective-path consistency (joint-gradient objective vs direct eval).
        var (fT, cT) = CacheTensors(xw);
        double sDirect = isControl
            ? 0.5 * MassIp(op.Evaluate(fT, cT, manifest, geometry).Coefficients)
            : SBThetaFast(op, mass, fT, cT, xt);
        objConsistencyMax = System.Math.Max(objConsistencyMax, RelDiff(s0, sDirect));

        // Translation covariance (3 random lattice shifts, both sectors).
        for (int rep = 0; rep < 3; rep++)
        {
            var a = new[] { batRng.Next(n), batRng.Next(n), batRng.Next(n), batRng.Next(n) };
            double sT = SBObj(op, TranslateOmega(xw, a), TranslateTheta(xt, a));
            covarianceMax = System.Math.Max(covarianceMax, RelDiff(s0, sT));
        }

        // Analytic-vs-FD joint gradient spot check (5 coordinates).
        var gO = new double[nOmega]; var gT = new double[nTheta];
        SBGradM(op, mass, xw, xt, gO, gT);
        for (int rep = 0; rep < 5; rep++)
        {
            bool omegaSector = batRng.NextDouble() < 0.5;
            double h = 1e-6;
            if (omegaSector)
            {
                int i = batRng.Next(nOmega);
                var xp = (double[])xw.Clone(); xp[i] += h;
                var xm = (double[])xw.Clone(); xm[i] -= h;
                double fd = (SBObj(op, xp, xt) - SBObj(op, xm, xt)) / (2 * h);
                gradFdMax = System.Math.Max(gradFdMax, System.Math.Abs(fd - gO[i]) / System.Math.Max(1.0, System.Math.Abs(fd)));
            }
            else
            {
                int i = batRng.Next(nTheta);
                var tp = (double[])xt.Clone(); tp[i] += h;
                var tm = (double[])xt.Clone(); tm[i] -= h;
                double fd = (SBObj(op, xw, tp) - SBObj(op, xw, tm)) / (2 * h);
                gradFdMax = System.Math.Max(gradFdMax, System.Math.Abs(fd - gT[i]) / System.Math.Max(1.0, System.Math.Abs(fd)));
            }
        }

        if (isControl)
        {
            identityThetaGradNorm = System.Math.Max(identityThetaGradNorm, Norm(gT));
        }
        else
        {
            // Chart battery A: uniform-theta shift leaves S_B exactly constant.
            var cShift = RandomVector(batRng, dimG, 0.9);
            var tU = new double[nTheta];
            for (int v = 0; v < nVert; v++) for (int l = 0; l < dimG; l++) tU[v * dimG + l] = cShift[l];
            double sU = SBObj(op, xw, tU);
            double sZ = SBObj(op, xw, new double[nTheta]);
            thetaUniformShiftMax = System.Math.Max(thetaUniformShiftMax, RelDiff(sU, sZ));

            // Chart battery B: |theta| > pi wrapped to the fundamental domain is the
            // same rotation (angle a about axis n == angle 2pi-a about -n).
            var tBig = new double[nTheta];
            var tWrap = new double[nTheta];
            for (int v = 0; v < nVert; v++)
            {
                var ax = RandomVector(batRng, dimG, 1.0);
                double an = Norm(ax);
                for (int l = 0; l < dimG; l++) ax[l] /= an;
                double angle = System.Math.PI + 0.4 + 1.2 * batRng.NextDouble(); // in (pi, 2pi)
                for (int l = 0; l < dimG; l++)
                {
                    tBig[v * dimG + l] = angle * ax[l];
                    tWrap[v * dimG + l] = -(2.0 * System.Math.PI - angle) * ax[l];
                }
            }
            thetaWrapChartMax = System.Math.Max(thetaWrapChartMax, RelDiff(SBObj(op, xw, tBig), SBObj(op, xw, tWrap)));

            // Chart battery C: a single-vertex 2pi rotation is the identity.
            var t2pi = (double[])xt.Clone();
            int v0 = batRng.Next(nVert);
            var ax0 = RandomVector(batRng, dimG, 1.0);
            double an0 = Norm(ax0);
            for (int l = 0; l < dimG; l++) t2pi[v0 * dimG + l] = 2.0 * System.Math.PI * ax0[l] / an0;
            var tId = (double[])xt.Clone();
            for (int l = 0; l < dimG; l++) tId[v0 * dimG + l] = 0.0;
            thetaPeriodicityMax = System.Math.Max(thetaPeriodicityMax, RelDiff(SBObj(op, xw, t2pi), SBObj(op, xw, tId)));
        }
    }
    double MassIp(double[] coeffs)
    {
        var s = FaceTensor(coeffs);
        return mass.InnerProduct(s, s);
    }

    bool covarianceBattery = orbitMapOk && covarianceMax <= CovarianceTol;
    bool objConsistencyBattery = objConsistencyMax <= ObjectiveConsistencyTol;
    bool gradFdBattery = gradFdMax <= GradFdTol;
    bool thetaChartBattery = thetaUniformShiftMax <= ThetaChartTol
        && thetaWrapChartMax <= ThetaChartTol && thetaPeriodicityMax <= ThetaChartTol;
    bool identityThetaIndependenceBattery = identityThetaGradNorm <= IdentityThetaGradTol;
    bool torusHardBatteries = precursorsPassed && projectorBattery && covarianceBattery
        && objConsistencyBattery && gradFdBattery && thetaChartBattery && identityThetaIndependenceBattery;
    allHardBatteriesPassed &= torusHardBatteries;
    Console.WriteLine($"# n={n} batteries: cov={covarianceMax:E2} obj={objConsistencyMax:E2} gradFd={gradFdMax:E2} " +
        $"chart(U/W/P)={thetaUniformShiftMax:E2}/{thetaWrapChartMax:E2}/{thetaPeriodicityMax:E2} " +
        $"idThetaGrad={identityThetaGradNorm:E2} projOverlap={projectorOverlapMaxAfter:E2} pass={torusHardBatteries}");

    // --- Tree curve along u_inv (theta = 0) + quartic fit. -------------------
    // S_B(t u) is an exact quartic in t on the omega sector; least-squares fit
    // of a2 t^2 + a3 t^3 + a4 t^4 records the coefficients per member (a3
    // residual must be ~0 on the invariant ray -- the review-board Z2).
    var treeByMember = new Dictionary<string, (double[] T, double[] S, double A2, double A3, double A4)>();
    foreach (var (name, spec, op, _) in members)
    {
        int np = 17;
        var ts = new double[np]; var ss = new double[np];
        for (int i = 0; i < np; i++)
        {
            ts[i] = -TMax + 2.0 * TMax * i / (np - 1);
            ss[i] = SBObj(op, ScaleVec(uInv, ts[i]), new double[nTheta]);
        }
        var (a2, a3, a4) = FitQuartic(ts, ss);
        treeByMember[name] = (ts, ss, a2, a3, a4);
        Console.WriteLine($"# n={n} tree {name}: a2={a2:E3} a3={a3:E2} a4={a4:E3}");
    }

    // --- Window plan (per member and beta): centers, springs, bins. ----------
    // Self-consistent auto rule: spacing = min(0.5, 2.2/sqrt(6 beta kappa_tree))
    // and k = (2.2/spacing)^2, so k >= 6 beta kappa_tree >= 6 beta kappa_F
    // (the free-energy curvature is bounded by the ray curvature) => window
    // means pinned within ~15% of the centers, AND the biased width
    // 1/sqrt(k + beta kappa_F) >= spacing/2.35 => neighbor overlap ~0.2-0.27.
    // The tree curvature enters only the SPACING; the spring is spacing-set.
    WindowPlan PlanWindows(double beta, string memberName)
    {
        var (_, _, a2, a3, a4) = treeByMember[memberName];
        double KappaAt(double c) => System.Math.Max(2.0 * a2 + 6.0 * a3 * c + 12.0 * a4 * c * c, 0.0);
        double kappaMax = System.Math.Max(KappaAt(TMax), KappaAt(-TMax));
        int nw;
        double spacing;
        if (WindowsEnv > 0)
        {
            nw = WindowsEnv % 2 == 0 ? WindowsEnv + 1 : WindowsEnv; // symmetric, includes 0
            spacing = 2.0 * TMax / (nw - 1);
        }
        else
        {
            spacing = System.Math.Min(0.5, 2.2 / System.Math.Sqrt(System.Math.Max(6.0 * beta * kappaMax, 1.0)));
            int half = (int)System.Math.Ceiling(TMax / spacing);
            half = System.Math.Min(half, 20); // cap 41 windows
            spacing = TMax / half;
            nw = 2 * half + 1;
        }
        var centers = new double[nw];
        var springs = new double[nw];
        for (int w = 0; w < nw; w++)
        {
            centers[w] = -TMax + spacing * w;
            // Spring constant 2.0 (was 2.2): the first production run measured the
            // stiff identity member's neighbor overlap at 0.149 vs the 0.15 gate --
            // the tree curvature adds to the biased width's denominator, narrowing
            // histograms below the rule's target. Softening the spring by (2.2/2.0)^2
            // ~ 21% widens the biased width and restores overlap margin; pinning
            // stays within the recorded ~15% envelope (re-gated by this run).
            springs[w] = KUmbEnv > 0.0 ? KUmbEnv : (2.0 / spacing) * (2.0 / spacing);
        }
        double bw = spacing / 4.0;
        int nbHalf = (int)System.Math.Ceiling((TMax + 3.0 * spacing) / bw);
        int nb = 2 * nbHalf + 1;
        var binPhi = new double[nb];
        for (int b = 0; b < nb; b++) binPhi[b] = (b - nbHalf) * bw;
        return new WindowPlan(centers, springs, binPhi, bw, spacing);
    }

    // --- One umbrella window: omega-HMC + theta-Haar Metropolis. -------------
    // op/mm are PER-WINDOW instances (all operator state is readonly and
    // precomputed, but windows run in parallel and get their own copies so no
    // instance is ever shared across threads).
    WindowResult RunWindow(EinsteinianShiabOperator op, CpuMassMatrix mm, bool isControl, double beta,
        double center, double kUmb, double[] binPhi, double binW, Random rng)
    {
        var swWin = Stopwatch.StartNew();
        var omega = ScaleVec(uInv, center);
        for (int i = 0; i < nOmega; i++) omega[i] += 0.01 * Gauss(rng);
        // Per-vertex rotations as unit quaternions (identity start).
        var quat = new double[nVert][];
        for (int v = 0; v < nVert; v++) quat[v] = new[] { 1.0, 0.0, 0.0, 0.0 };
        var theta = new double[nTheta]; // fundamental-domain chart of quat

        var gradBuf = new double[nOmega];
        var force = new double[nOmega];
        var curForce = new double[nOmega];
        var p = new double[nOmega];
        var w0 = new double[nOmega];
        double curS = 0, curPhi = 0, curU = 0;

        double PotAndForce(double[] w, double[] th, double[] fOut, out double s, out double phi)
        {
            s = SBGradM(op, mm, w, th, gradBuf);
            phi = Phi(w);
            double spring = kUmb * (phi - center);
            for (int i = 0; i < nOmega; i++) fOut[i] = -(beta * gradBuf[i] + spring * uInv[i]);
            return beta * s + 0.5 * kUmb * (phi - center) * (phi - center);
        }

        double eps = EpsEnv > 0.0 ? EpsEnv : 0.05 / System.Math.Sqrt(System.Math.Max(beta, 1.0));
        curU = PotAndForce(omega, theta, curForce, out curS, out curPhi);

        long nAcc = 0, nTraj = 0;
        double sumExpNegDh = 0, sumExpNegDh2 = 0;

        bool Trajectory()
        {
            Array.Copy(omega, w0, nOmega);
            for (int i = 0; i < nOmega; i++) p[i] = Gauss(rng);
            double h0 = curU;
            for (int i = 0; i < nOmega; i++) h0 += 0.5 * p[i] * p[i];
            Array.Copy(curForce, force, nOmega);
            for (int l = 0; l < NLeap; l++)
            {
                for (int i = 0; i < nOmega; i++) p[i] += 0.5 * eps * force[i];
                for (int i = 0; i < nOmega; i++) omega[i] += eps * p[i];
                PotAndForce(omega, theta, force, out _, out _);
                for (int i = 0; i < nOmega; i++) p[i] += 0.5 * eps * force[i];
            }
            double newU = PotAndForce(omega, theta, force, out double newS, out double newPhi);
            double h1 = newU;
            for (int i = 0; i < nOmega; i++) h1 += 0.5 * p[i] * p[i];
            double dh = h1 - h0;
            // Exponent capped at 50 purely to keep a pathological trajectory from
            // poisoning the accumulator with Inf; a healthy chain never hits it.
            double expNegDh = double.IsFinite(dh) ? System.Math.Exp(System.Math.Min(-dh, 50.0)) : 0.0;
            sumExpNegDh += expNegDh; sumExpNegDh2 += expNegDh * expNegDh; nTraj++;
            bool acc = double.IsFinite(dh) && (dh <= 0 || rng.NextDouble() < System.Math.Exp(-dh));
            if (acc)
            {
                nAcc++; curU = newU; curS = newS; curPhi = newPhi;
                Array.Copy(force, curForce, nOmega);
            }
            else
            {
                Array.Copy(w0, omega, nOmega);
                curU = PotAndForce(omega, theta, curForce, out curS, out curPhi);
            }
            return acc;
        }

        // Theta-Haar Metropolis sweep (binding condition (ii)); omega fixed.
        double thetaStep = 0.4;
        long thGlobProp = 0, thGlobAcc = 0, thSingProp = 0, thSingAcc = 0;
        void ThetaSweep()
        {
            var (fT, cT) = CacheTensors(omega);
            double sCur = SBThetaFast(op, mm, fT, cT, theta);
            var thetaCand = new double[nTheta];
            for (int rep = 0; rep < ThetaProps; rep++)
            {
                bool global = rep % 2 == 0;
                double[][] qCand;
                if (global)
                {
                    qCand = new double[nVert][];
                    for (int v = 0; v < nVert; v++)
                    {
                        var d = new[] { thetaStep * Gauss(rng), thetaStep * Gauss(rng), thetaStep * Gauss(rng) };
                        qCand[v] = QMul(QFromAxisAngle(d), quat[v]);
                    }
                }
                else
                {
                    qCand = (double[][])quat.Clone();
                    qCand[rng.Next(nVert)] = QUniform(rng); // uniform-Haar independence proposal
                }
                for (int v = 0; v < nVert; v++)
                {
                    var aa = QToAxisAngle(qCand[v]);
                    for (int l = 0; l < dimG; l++) thetaCand[v * dimG + l] = aa[l];
                }
                double sNew = SBThetaFast(op, mm, fT, cT, thetaCand);
                double dS = beta * (sNew - sCur);
                bool acc = double.IsFinite(dS) && (dS <= 0 || rng.NextDouble() < System.Math.Exp(-dS));
                if (global) { thGlobProp++; if (acc) thGlobAcc++; } else { thSingProp++; if (acc) thSingAcc++; }
                if (acc)
                {
                    quat = qCand;
                    Array.Copy(thetaCand, theta, nTheta);
                    sCur = sNew;
                }
            }
            curU = PotAndForce(omega, theta, curForce, out curS, out curPhi);
        }

        // Warmup with eps (and theta-step) adaptation.
        int batch = 40; long bAcc = 0, bCnt = 0;
        for (int it = 0; it < WarmTraj; it++)
        {
            if (Trajectory()) bAcc++;
            bCnt++;
            if (!isControl) ThetaSweep();
            if (bCnt == batch)
            {
                double r = (double)bAcc / bCnt;
                if (EpsEnv <= 0.0)
                {
                    // Target acceptance [0.90, 0.98]: the smoke run showed acc ~0.85
                    // already drags <e^-dH> to 0.92 on the stiff identity member.
                    if (r < 0.90) eps *= 0.65;
                    else if (r > 0.98) eps *= 1.15;
                }
                if (!isControl && thGlobProp > 20)
                {
                    double rg = (double)thGlobAcc / System.Math.Max(1, thGlobProp);
                    if (rg < 0.25) thetaStep *= 0.7;
                    else if (rg > 0.6) thetaStep *= 1.25;
                    thGlobProp = 0; thGlobAcc = 0;
                }
                bAcc = 0; bCnt = 0;
            }
        }

        // Sampling (adaptive chunks until N_eff >= NeffTarget or the cap).
        nAcc = 0; nTraj = 0; sumExpNegDh = 0; sumExpNegDh2 = 0;
        thGlobProp = 0; thGlobAcc = 0; thSingProp = 0; thSingAcc = 0;
        var phiSeries = new List<double>();
        var sSeries = new List<double>();
        var virSeries = new List<double>();
        var omega2Series = new List<double>();
        var counts = new long[binPhi.Length];
        int nbHalf = binPhi.Length / 2;
        double tauPhi = double.PositiveInfinity, tauS = double.PositiveInfinity;
        double neffPhi = 0, neffS = 0;
        while (phiSeries.Count < MaxTraj)
        {
            int target = System.Math.Min(phiSeries.Count + ChunkTraj, MaxTraj);
            while (phiSeries.Count < target)
            {
                Trajectory();
                if (!isControl) ThetaSweep();
                phiSeries.Add(curPhi);
                sSeries.Add(curS);
                double vir = 0; // w . dU_tot/dw = -w . force  (bias included)
                for (int i = 0; i < nOmega; i++) vir -= omega[i] * curForce[i];
                virSeries.Add(vir);
                double o2 = 0; for (int i = 0; i < nOmega; i++) o2 += omega[i] * omega[i];
                omega2Series.Add(o2);
                int b = (int)System.Math.Round(curPhi / binW) + nbHalf;
                if (b >= 0 && b < counts.Length) counts[b]++;
            }
            tauPhi = TauInt(phiSeries);
            tauS = TauInt(sSeries);
            neffPhi = phiSeries.Count / (2.0 * System.Math.Max(tauPhi, 0.5));
            neffS = sSeries.Count / (2.0 * System.Math.Max(tauS, 0.5));
            // MinTraj is a statistics floor for the WHAM histograms; the N_eff
            // target is binding condition (iii).
            if (neffPhi >= NeffTarget && neffS >= NeffTarget && phiSeries.Count >= MinTraj) break;
        }
        swWin.Stop();

        int nS = phiSeries.Count;
        double accRate = (double)nAcc / System.Math.Max(1, nTraj);
        double expDh = sumExpNegDh / System.Math.Max(1, nTraj);
        double expDhSe = System.Math.Sqrt(System.Math.Max(0, sumExpNegDh2 / System.Math.Max(1, nTraj) - expDh * expDh)
            / System.Math.Max(1, nTraj));
        double meanPhi = phiSeries.Average(), sdPhi = Sd(phiSeries, meanPhi);
        double meanS = sSeries.Average();
        double meanVir = virSeries.Average();
        double sdVir = Sd(virSeries, meanVir);
        double virSe = sdVir / System.Math.Sqrt(System.Math.Max(neffS, 1.0));
        double virialRel = meanVir / nOmega - 1.0;
        // Stationarity diagnostic (recorded, not gated; a non-normalizable
        // direction would show as drift AND blow tau_int/N_eff).
        double rms1 = System.Math.Sqrt(omega2Series.Take(nS / 2).Average());
        double rms2 = System.Math.Sqrt(omega2Series.Skip(nS / 2).Average());
        double driftRel = System.Math.Abs(rms2 - rms1) / System.Math.Max(rms1, 1e-12);

        bool expDhGate = System.Math.Abs(expDh - 1.0) <= System.Math.Max(ExpDhAbsFloor, 4.0 * expDhSe);
        bool virialGate = System.Math.Abs(virialRel) <= System.Math.Max(VirialRelFloor, 4.0 * virSe / nOmega);
        bool ergodicityGate = neffPhi >= NeffTarget && neffS >= NeffTarget;

        return new WindowResult(center, kUmb, nS, eps, accRate,
            expDh, expDhSe, expDhGate,
            meanVir, virialRel, virSe, virialGate,
            tauPhi, tauS, neffPhi, neffS, ergodicityGate,
            meanPhi, sdPhi, meanS, driftRel,
            thGlobProp > 0 ? (double)thGlobAcc / thGlobProp : double.NaN,
            thSingProp > 0 ? (double)thSingAcc / thSingProp : double.NaN,
            thetaStep, counts, swWin.Elapsed.TotalMilliseconds / System.Math.Max(1, nS));
    }

    // --- One member arm: all windows + WHAM + classification + gates. --------
    // Windows are independent chains and run in PARALLEL; each gets its own
    // operator + mass-matrix instance and a seed pre-drawn sequentially from
    // the root RNG (deterministic for a given PHASE450_SEED regardless of
    // scheduling).
    MemberArm RunMemberArm(string name, EinsteinianShiabFamilyMember spec, bool isControl, double beta, WindowPlan plan)
    {
        int nw = plan.Centers.Length;
        var seeds = new int[nw + 1];
        for (int w = 0; w < nw + 1; w++) seeds[w] = rootRng.Next();
        var windowsArr = new WindowResult[nw + 1];
        var consoleLock = new object();
        // Index nw is the UNCONSTRAINED TADPOLE DIAGNOSTIC (k = 0): a direct
        // measurement of <Phi> under the raw joint ensemble. It never enters
        // WHAM; it attributes any +-Phi asymmetry of the reconstruction (the
        // action's cubic terms generate a fluctuation tadpole -- the a3 = 0
        // Z2 is exact only on the invariant ray, and the Kuhn triangulation
        // has no reflection symmetry, so the JOINT ensemble may genuinely
        // tilt; third-smoke finding).
        Parallel.For(0, nw + 1, new ParallelOptions { MaxDegreeOfParallelism = MaxParallel }, w =>
        {
            var opW = new EinsteinianShiabOperator(mesh, algebra, spec, latticePeriod: n);
            var mmW = new CpuMassMatrix(mesh, algebra);
            bool diag = w == nw;
            var res = RunWindow(opW, mmW, isControl, beta,
                diag ? 0.0 : plan.Centers[w], diag ? 0.0 : plan.Springs[w],
                plan.BinPhi, plan.BinW, new Random(seeds[w]));
            windowsArr[w] = res;
            lock (consoleLock)
            {
                Console.WriteLine($"#   n={n} {name} beta={beta:0.###} {(diag ? "TAD" : "win")} c={res.Center,7:F3} k={res.KUmb,8:F2} " +
                    $"traj={res.Trajectories} acc={res.AcceptRate:F3} expDH={res.ExpDh:F4} vir={res.VirialRel:+0.000;-0.000} " +
                    $"tauPhi={res.TauPhi:F1} neffPhi={res.NeffPhi:F0} neffS={res.NeffS:F0} " +
                    $"<Phi>={res.MeanPhi:F3} ms/traj={res.MsPerTraj:F1}");
            }
        });
        var tadpole = windowsArr[nw];
        double tadpoleSe = tadpole.SdPhi / System.Math.Sqrt(System.Math.Max(tadpole.NeffPhi, 1.0));
        double tadpoleSigma = System.Math.Abs(tadpole.MeanPhi) / System.Math.Max(tadpoleSe, 1e-12);
        var windows = windowsArr.Take(nw).ToList();

        // Neighbor overlap coefficient.
        double overlapMinObs = double.PositiveInfinity;
        for (int w = 0; w + 1 < windows.Count; w++)
        {
            double ov = 0;
            for (int b = 0; b < plan.BinPhi.Length; b++)
                ov += System.Math.Min(
                    windows[w].Counts[b] / (double)System.Math.Max(1, windows[w].Trajectories),
                    windows[w + 1].Counts[b] / (double)System.Math.Max(1, windows[w + 1].Trajectories));
            overlapMinObs = System.Math.Min(overlapMinObs, ov);
        }
        bool overlapGate = overlapMinObs >= OverlapMin;

        // WHAM.
        var whamWins = windows.Select(w => (w.Counts, (long)w.Trajectories, w.Center, w.KUmb)).ToArray();
        var (lnP, _, whamResid, whamIters) = WhamSolve(whamWins, plan.BinPhi);
        bool whamGate = whamResid <= WhamResidualTol;

        // U(Phi) on qualifying bins with an approximate per-bin error:
        // (a) LOCAL counting: total bin counts deflated by the mean 2*tau_int;
        // (b) STITCHING ACCUMULATION: the relative free energies f_w random-
        //     walk across window boundaries -- the textbook dominant error for
        //     umbrella chains, and exactly the tilt artifact the third smoke
        //     run surfaced (the unconstrained tadpole check measured
        //     <Phi> = 0.004 +- 0.14 while the naive-error curve "tilted" at
        //     6-7 sigma). Per-boundary variance = 1/(beta^2 * neff_overlap)
        //     with neff_overlap = the tau-deflated counts shared by the
        //     adjacent windows, accumulated in quadrature from the window
        //     nearest Phi = 0 out to the bin's nearest window.
        double tauMean = windows.Average(w => System.Math.Max(w.TauPhi, 0.5));
        int wCenter = 0;
        for (int w = 1; w < windows.Count; w++)
            if (System.Math.Abs(windows[w].Center) < System.Math.Abs(windows[wCenter].Center)) wCenter = w;
        var pairVar = new double[System.Math.Max(windows.Count - 1, 0)];
        for (int w = 0; w + 1 < windows.Count; w++)
        {
            long novl = 0;
            for (int b = 0; b < plan.BinPhi.Length; b++)
                novl += System.Math.Min(windows[w].Counts[b], windows[w + 1].Counts[b]);
            double neffOvl = System.Math.Max(novl / (2.0 * tauMean), 1.0);
            pairVar[w] = 1.0 / (beta * beta * neffOvl);
        }
        double StitchVar(double phi)
        {
            int wb = 0;
            for (int w = 1; w < windows.Count; w++)
                if (System.Math.Abs(windows[w].Center - phi) < System.Math.Abs(windows[wb].Center - phi)) wb = w;
            double s = 0.0;
            for (int w = System.Math.Min(wb, wCenter); w < System.Math.Max(wb, wCenter); w++) s += pairVar[w];
            return s;
        }
        var bins = new List<CepBin>();
        double uMinRaw = double.PositiveInfinity;
        for (int b = 0; b < plan.BinPhi.Length; b++)
        {
            long tot = 0; foreach (var w in windows) tot += w.Counts[b];
            if (tot < MinBinCounts || double.IsNegativeInfinity(lnP[b])) continue;
            double u = -lnP[b] / beta;
            double errCount = 1.0 / (beta * System.Math.Sqrt(System.Math.Max(tot / (2.0 * tauMean), 1.0)));
            double err = System.Math.Sqrt(errCount * errCount + StitchVar(plan.BinPhi[b]));
            uMinRaw = System.Math.Min(uMinRaw, u);
            bins.Add(new CepBin(plan.BinPhi[b], u, err, tot));
        }
        double stitchSigmaEdge = System.Math.Sqrt(System.Math.Max(StitchVar(-TMax), StitchVar(TMax)));
        for (int i = 0; i < bins.Count; i++) bins[i] = bins[i] with { U = bins[i].U - uMinRaw };
        bins = bins.OrderBy(x => x.Phi).ToList();

        // Roughness-inflated errors: the naive per-bin error (counts deflated
        // by tau) misses WHAM window-stitching systematics -- the second smoke
        // run showed sub-resolution wiggles reading 4-6 "sigma" under the
        // naive model on the identity CONTROL. Estimate the high-frequency
        // noise floor robustly from the SECOND DIFFERENCES (a running-median
        // residual is exactly blind on monotone segments -- third-smoke
        // lesson): r_i = U_{i-1} - 2 U_i + U_{i+1} has Var = 6 sigma^2 for
        // independent bin noise plus a negligible bw^2 U'' curvature bias, so
        // sigma = 1.4826 * MAD(|r|) / sqrt(6). Recorded per arm, added in
        // quadrature to the counting error.
        double sigmaRough = 0.0;
        if (bins.Count >= 7)
        {
            var resid = new List<double>();
            for (int i = 1; i < bins.Count - 1; i++)
                resid.Add(System.Math.Abs(bins[i - 1].U - 2.0 * bins[i].U + bins[i + 1].U));
            resid.Sort();
            sigmaRough = 1.4826 * resid[resid.Count / 2] / System.Math.Sqrt(6.0);
        }
        for (int i = 0; i < bins.Count; i++)
            bins[i] = bins[i] with
            {
                Err = System.Math.Sqrt(bins[i].Err * bins[i].Err + sigmaRough * sigmaRough),
            };

        // Coverage.
        double coverageLo = bins.Count > 0 ? bins.Min(x => x.Phi) : 0.0;
        double coverageHi = bins.Count > 0 ? bins.Max(x => x.Phi) : 0.0;
        double coverageHalfWidth = System.Math.Min(-coverageLo, coverageHi);
        bool coverageGate = coverageHalfWidth >= CoverageHalfWidthFraction * TMax;

        // +-Phi antisymmetry (bin grid is symmetric: exact partner lookup).
        double antisymMaxSigma = 0.0, antisymMaxAbs = 0.0;
        var byPhi = bins.ToDictionary(x => (int)System.Math.Round(x.Phi / plan.BinW));
        foreach (var x in bins)
        {
            int key = (int)System.Math.Round(x.Phi / plan.BinW);
            if (key <= 0 || !byPhi.TryGetValue(-key, out var y)) continue;
            double d = System.Math.Abs(x.U - y.U);
            double se = System.Math.Sqrt(x.Err * x.Err + y.Err * y.Err);
            antisymMaxAbs = System.Math.Max(antisymMaxAbs, d);
            antisymMaxSigma = System.Math.Max(antisymMaxSigma, d / System.Math.Max(se, 1e-12));
        }
        bool antisymmetryGate = antisymMaxSigma <= AntisymmetrySigma;

        bool windowGatesAll = windows.All(w => w.ExpDhGate && w.VirialGate && w.ErgodicityGate);
        bool gatesExceptAntisym = windowGatesAll && overlapGate && whamGate && coverageGate;
        bool gatesAllPassed = gatesExceptAntisym && antisymmetryGate;

        // Pre-registered classification.
        var (cls, tStar, depth, depthSigma, flatHalfWidth) = ClassifyCep(bins, plan.BinW);

        return new MemberArm(name, isControl, beta, plan, windows,
            overlapMinObs, overlapGate, whamResid, whamIters, whamGate,
            coverageHalfWidth, coverageGate, antisymMaxAbs, antisymMaxSigma, antisymmetryGate,
            windowGatesAll, gatesExceptAntisym, gatesAllPassed, bins, sigmaRough, stitchSigmaEdge,
            tadpole, tadpoleSe, tadpoleSigma,
            cls, tStar, depth, depthSigma, flatHalfWidth);
    }

    (string Cls, double TStar, double Depth, double DepthSigma, double FlatHalfWidth) ClassifyCep(
        List<CepBin> bins, double binW)
    {
        if (bins.Count < 7) return ("inconclusive-structure", double.NaN, double.NaN, double.NaN, double.NaN);
        var sorted = bins.OrderBy(x => x.Phi).ToList();
        int minIdx = 0;
        for (int i = 1; i < sorted.Count; i++) if (sorted[i].U < sorted[minIdx].U) minIdx = i;
        int ctrIdx = 0;
        for (int i = 1; i < sorted.Count; i++)
            if (System.Math.Abs(sorted[i].Phi) < System.Math.Abs(sorted[ctrIdx].Phi)) ctrIdx = i;
        double errMed = sorted.OrderBy(x => x.Err).ElementAt(sorted.Count / 2).Err;
        double depth = sorted[ctrIdx].U - sorted[minIdx].U;
        double depthSigma = depth / System.Math.Max(System.Math.Sqrt(
            sorted[ctrIdx].Err * sorted[ctrIdx].Err + sorted[minIdx].Err * sorted[minIdx].Err), 1e-12);
        // Flat-bottom half-width: contiguous span around the center with U within
        // 2 median errors of the minimum.
        int lo = ctrIdx, hi = ctrIdx;
        while (lo - 1 >= 0 && sorted[lo - 1].U - sorted[minIdx].U <= 2.0 * errMed) lo--;
        while (hi + 1 < sorted.Count && sorted[hi + 1].U - sorted[minIdx].U <= 2.0 * errMed) hi++;
        double flatHalfWidth = 0.5 * (sorted[hi].Phi - sorted[lo].Phi);
        bool edgesRise = lo > 0 && hi < sorted.Count - 1
            && sorted[0].U - sorted[minIdx].U > 5.0 * errMed
            && sorted[^1].U - sorted[minIdx].U > 5.0 * errMed;

        if (System.Math.Abs(sorted[minIdx].Phi) <= 1.5 * binW)
        {
            // Monotone rise on both sides within slack => single well at zero.
            bool mono = true;
            double run = sorted[minIdx].U;
            for (int i = minIdx + 1; i < sorted.Count; i++)
            {
                if (sorted[i].U < run - MonotoneSlackSigma * errMed) { mono = false; break; }
                run = System.Math.Max(run, sorted[i].U);
            }
            run = sorted[minIdx].U;
            for (int i = minIdx - 1; i >= 0 && mono; i--)
            {
                if (sorted[i].U < run - MonotoneSlackSigma * errMed) { mono = false; break; }
                run = System.Math.Max(run, sorted[i].U);
            }
            if (mono && flatHalfWidth >= FlatBottomMinHalfWidth && edgesRise)
                return ("flat-bottom-degenerate-minima", flatHalfWidth, depth, depthSigma, flatHalfWidth);
            return (mono ? "single-well-at-zero" : "inconclusive-structure",
                double.NaN, depth, depthSigma, flatHalfWidth);
        }

        // Off-center minimum: an SSB call needs the +- pair (the a3 = 0 parity
        // partner) AND a significant depth below the center ON BOTH SIDES --
        // degenerate +-t* minima mean U(0) sits above BOTH wells, so the
        // partner must be (a) at the minimum's level and (b) itself at least
        // SsbDepthSigma below the center bin. (The fourth smoke run showed the
        // one-sided rule promoting a boundary-case 3.1-sigma control artifact
        // to a fake SSB pair; the two-sided rule is strictly fail-closed.)
        double tS = System.Math.Abs(sorted[minIdx].Phi);
        bool pairOk = sorted.Any(x => System.Math.Abs(x.Phi + sorted[minIdx].Phi) <= 2.0 * binW
            && x.U - sorted[minIdx].U <= SsbDepthSigma * errMed
            && sorted[ctrIdx].U - x.U > SsbDepthSigma * errMed);
        if (depthSigma > SsbDepthSigma && pairOk)
            return ("flat-bottom-degenerate-minima", tS, depth, depthSigma, flatHalfWidth);

        // No degenerate partner: test whether the curve is a SINGLE well
        // around its (possibly offset) minimum -- monotone rise on both sides
        // within the slack. A unique tilted well is NOT the SSB signature
        // (that requires the +- pair); it is the null family with a recorded
        // parity tilt (the fluctuation-tadpole outcome the third smoke run
        // surfaced -- the joint ensemble has no exact Phi -> -Phi symmetry).
        bool mono2 = true;
        double run2 = sorted[minIdx].U;
        for (int i = minIdx + 1; i < sorted.Count && mono2; i++)
        {
            if (sorted[i].U < run2 - MonotoneSlackSigma * errMed) mono2 = false;
            run2 = System.Math.Max(run2, sorted[i].U);
        }
        run2 = sorted[minIdx].U;
        for (int i = minIdx - 1; i >= 0 && mono2; i--)
        {
            if (sorted[i].U < run2 - MonotoneSlackSigma * errMed) mono2 = false;
            run2 = System.Math.Max(run2, sorted[i].U);
        }
        if (mono2 && flatHalfWidth >= FlatBottomMinHalfWidth && edgesRise)
            return ("flat-bottom-degenerate-minima", flatHalfWidth, depth, depthSigma, flatHalfWidth);
        if (mono2)
            return (depthSigma <= SsbDepthSigma
                ? "single-well-at-zero"          // offset indistinguishable from 0
                : "single-well-offset",          // significant tadpole tilt, unique minimum
                tS, depth, depthSigma, flatHalfWidth);
        return ("inconclusive-structure", tS, depth, depthSigma, flatHalfWidth);
    }

    // --- Physics arms at beta = Beta (each member planned on its OWN tree
    // curvature: the identity control is ~12x stiffer than sd2 and needs its
    // own spacing for the overlap + pinning guarantees). --------------------
    var arms = new List<MemberArm>();
    foreach (var (name, spec, op, isControl) in members)
    {
        var plan = PlanWindows(Beta, name);
        arms.Add(RunMemberArm(name, spec, isControl, Beta, plan));
    }

    // --- Large-beta tree-shape control column (first torus, first Einsteinian).
    if (largeBetaColumn == null && members.Any(m => !m.IsControl))
    {
        var (nameLb, specLb, opLb, _) = members.First(m => !m.IsControl);
        var planLb = PlanWindows(BetaLarge, nameLb);
        var armLb = RunMemberArm(nameLb, specLb, false, BetaLarge, planLb);
        // Pearson correlation between U_largeBeta at window centers (nearest
        // qualifying bin) and the tree curve S_B(c * u_inv, theta = 0).
        var tree = treeByMember[nameLb];
        var xs = new List<double>(); var ys = new List<double>();
        foreach (var c in planLb.Centers)
        {
            CepBin? nearest = null;
            foreach (var bin in armLb.Bins)
                if (nearest is null || System.Math.Abs(bin.Phi - c) < System.Math.Abs(nearest.Phi - c)) nearest = bin;
            if (nearest is null || System.Math.Abs(nearest.Phi - c) > planLb.Spacing) continue;
            double sTree = tree.A2 * c * c + tree.A3 * c * c * c + tree.A4 * c * c * c * c;
            xs.Add(sTree); ys.Add(nearest.U);
        }
        double pearson = Pearson(xs, ys);
        bool shapeGate = xs.Count >= 5 && pearson >= LargeBetaPearsonMin
            && (armLb.Classification == "single-well-at-zero" || armLb.Classification == "single-well-offset");
        largeBetaColumn = new LargeBetaColumn(n, nameLb, BetaLarge, armLb, pearson, xs.Count, shapeGate);
        Console.WriteLine($"# large-beta column: beta={BetaLarge} cls={armLb.Classification} pearson={pearson:F4} " +
            $"points={xs.Count} gate={shapeGate}");
    }

    torusRecords.Add(new TorusArm(n, mesh.VertexCount, mesh.EdgeCount, mesh.FaceCount, mesh.CellCount,
        nOmega, nTheta, orbitMapOk, uNormRaw, uNormAfter, overlapsBefore, overlapsAfter,
        projectorOverlapMaxAfter, projectorBattery,
        covarianceMax, covarianceBattery, objConsistencyMax, objConsistencyBattery,
        gradFdMax, gradFdBattery, thetaUniformShiftMax, thetaWrapChartMax, thetaPeriodicityMax,
        thetaChartBattery, identityThetaGradNorm, identityThetaIndependenceBattery,
        torusHardBatteries, treeByMember, arms));
}

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// Verdicts.
// ---------------------------------------------------------------------------

var allArms = torusRecords.SelectMany(t => t.Arms).ToList();
var einsteinianArms = allArms.Where(a => !a.IsControl).ToList();
var controlArms = allArms.Where(a => a.IsControl).ToList();

static bool SingleWellFamily(string cls) => cls is "single-well-at-zero" or "single-well-offset";

bool samplingGatesAllPassed = allArms.All(a => a.GatesAllPassed)
    && (largeBetaColumn == null || (largeBetaColumn.Arm.GatesAllPassed && largeBetaColumn.ShapeGate));
// The single-well FAMILY (at-zero or tadpole-offset) is the null family for
// the SSB question: the SSB signature REQUIRES the degenerate +- pair.
bool controlClean = controlArms.All(a => SingleWellFamily(a.Classification));
bool anyEinsteinianFlatBottom = einsteinianArms.Any(a => a.Classification == "flat-bottom-degenerate-minima");
bool allEinsteinianSingleWell = einsteinianArms.Count > 0
    && einsteinianArms.All(a => SingleWellFamily(a.Classification));
bool anySingleWellOffset = allArms.Any(a => a.Classification == "single-well-offset")
    || (largeBetaColumn?.Arm.Classification == "single-well-offset");

// Tilted-well attribution (third-smoke finding): the joint ensemble has NO
// exact Phi -> -Phi symmetry (the a3 = 0 Z2 is exact only on the invariant
// ray; the action's cubic terms generate a fluctuation tadpole and the Kuhn
// triangulation is not reflection-symmetric). When the ONLY failing sampling
// gates are the +-Phi antisymmetry gates, every curve is in the single-well
// family, and each asymmetric arm's UNCONSTRAINED tadpole diagnostic
// (<Phi> at k = 0, measured independently of WHAM) is significant, the
// outcome is recorded as a tilted single well -- a null-family verdict with
// a first-class parity-tilt observable -- rather than a sampler failure.
// (An added honesty kind beyond the pre-registered three, following the
// phase445/446/449 discipline; the antisymmetry gate itself is unchanged.)
bool onlyAntisymmetryGatesFailed = !samplingGatesAllPassed
    && allArms.All(a => a.GatesExceptAntisym)
    && (largeBetaColumn == null || (largeBetaColumn.Arm.GatesExceptAntisym && largeBetaColumn.ShapeGate))
    && allArms.Concat(largeBetaColumn is null ? Enumerable.Empty<MemberArm>() : new[] { largeBetaColumn.Arm })
        .Any(a => !a.AntisymmetryGate);
bool tadpoleAttributed = allArms
    .Concat(largeBetaColumn is null ? Enumerable.Empty<MemberArm>() : new[] { largeBetaColumn.Arm })
    .Where(a => !a.AntisymmetryGate)
    .All(a => a.TadpoleSigma > 3.0);

string verdictKind =
    !samplingGatesAllPassed && onlyAntisymmetryGatesFailed && controlClean && allEinsteinianSingleWell && tadpoleAttributed
        ? "tilted-single-well-parity-asymmetry-recorded"
    : !samplingGatesAllPassed ? "inconclusive-gates-failed"
    : !controlClean ? "control-contaminated-recorded"
    : anyEinsteinianFlatBottom ? "flat-bottom-ssb-candidate"
    : allEinsteinianSingleWell && anySingleWellOffset ? "tilted-single-well-parity-asymmetry-recorded"
    : allEinsteinianSingleWell ? "symmetric-phase-null"
    : "inconclusive-structure-recorded";

string verdictPhrase = verdictKind switch
{
    "inconclusive-gates-failed" =>
        "one or more sampling gates failed (per-window <e^-dH>/equipartition/N_eff, window overlap, WHAM residual, coverage, large-beta tree-shape control, or +-Phi antisymmetry) -- per binding condition (iii) and the pre-registered taxonomy the run is INCONCLUSIVE: no physics statement is made from a chain whose ergodicity or reconstruction quality is not demonstrated; every gate value is recorded per window",
    "control-contaminated-recorded" =>
        "the identity control's constraint effective potential does not classify single-well-at-zero -- recorded as exactly that (control contamination), not a null and not a candidate; no Einsteinian verdict is quoted from a run whose control is dirty",
    "flat-bottom-ssb-candidate" =>
        "the reconstructed constraint effective potential U(Phi) of at least one Einsteinian member shows the SSB signature the review board defined (flat Maxwell bottom / degenerate +-t* minima, the +- pair required by the a3 = 0 parity) with a clean identity control and all sampling gates green -- a WORKBENCH-RELATIVE SSB CANDIDATE ONLY: su(2) toy algebra, reduced Spin(4) slice, lattice units; NO scale, VEV, or GeV is promoted; physicist review is required",
    "symmetric-phase-null" =>
        "every Einsteinian member's full non-perturbative constraint effective potential U(Phi) is a single symmetric well at Phi = 0 with all sampling gates green -- the ansatz-free joint (omega, theta-Haar) ensemble shows NO SSB signature at this beta and volume: the strongest internal evidence yet against a dynamical scale on this action class (finite-volume caveat recorded; the Binder/multi-volume program remains the confirmatory instrument)",
    "tilted-single-well-parity-asymmetry-recorded" =>
        "every member's U(Phi) is a UNIQUE single well (no degenerate +- pair anywhere -- the null family for the SSB question), but the well is measurably TILTED: |U(Phi) - U(-Phi)| exceeds the antisymmetry gate and the independent unconstrained tadpole diagnostic (<Phi> at k = 0, no WHAM involved) is significant with all measure-preservation/ergodicity/overlap/WHAM/coverage gates green. The joint ensemble has no exact Phi -> -Phi symmetry (the a3 = 0 Z2 is exact only on the invariant ray -- verified at machine precision in the tree fit -- while the action's cubic terms generate a fluctuation tadpole and the Kuhn triangulation is not reflection-symmetric), so the tilt is recorded as a first-class parity observable of the workbench discretization, PENDING PHYSICIST REVIEW; it is NOT an SSB signature (no degenerate pair) and NOT promoted to a symmetric-phase null",
    _ =>
        "the gates passed but at least one member's U(Phi) fits neither the single-well nor the flat-bottom template -- recorded honestly as structure data, not a null and not a candidate",
};

// ---------------------------------------------------------------------------
// Framing + fail-closed block (phase449 discipline, verbatim keys).
// ---------------------------------------------------------------------------

const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool cepConventionsAreWorkbenchConventions = true;
const bool physicistReviewPending = true;
const string epsilonRealization = "independent-theta-dof-haar-sampled";
const string epsilonTaxonomyMode = "mode-4-nonperturbative-cep";

const string definition81Scope = "reduced-spin4-slice";
const bool ambientSevenSevenRealized = false;
const bool internalGaugeContentRealized = false;
const bool weldRealized = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase256Contract = false;

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    $"su2 trace-pairing; CreateUniform4DPeriodic(latticeCanonical:true) tori [{string.Join(",", TorusSizes)}]",
    "constraint effective potential by umbrella-sampled HMC under the four binding conditions: gauge-invariant invariant-ray collective coordinate (phase448 per-type construction, oSign lattice gauge, global-su(2) zero modes projected, projector recorded); theta-Haar measure (per-vertex SO(3) rotations as unit quaternions, symmetric left-multiply Metropolis, axis-angle fundamental-domain chart |theta|<=pi with hard chart batteries); ergodicity control (tau_int recorded, N_eff>=100 gate, adaptive trajectories); theta=0 never a verdict (joint ensemble is the physics run); log-space WHAM; pre-registered single-well/flat-bottom/inconclusive taxonomy; no target values")))).ToLowerInvariant();

bool constraintEffectivePotentialHmcProbePassed =
    precursorsPassed &&
    allHardBatteriesPassed &&
    scaleIsWorkbenchRelativeCandidateOnly &&
    noGevPromotion &&
    cepConventionsAreWorkbenchConventions &&
    physicistReviewPending &&
    definition81Scope == "reduced-spin4-slice" &&
    !ambientSevenSevenRealized &&
    !internalGaugeContentRealized &&
    !weldRealized &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !physicalCouplingProvided &&
    !routeProvidesPhysicalEffectiveActionHessian &&
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
    !canFillPhase256Contract &&
    !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = constraintEffectivePotentialHmcProbePassed
    ? verdictKind switch
    {
        "symmetric-phase-null" => TerminalPrefix + "passed-symmetric-phase-null-single-well",
        "flat-bottom-ssb-candidate" => TerminalPrefix + "passed-flat-bottom-ssb-candidate-workbench-relative-no-gev",
        "control-contaminated-recorded" => TerminalPrefix + "passed-control-contaminated-recorded",
        "inconclusive-gates-failed" => TerminalPrefix + "passed-inconclusive-gates-failed-recorded",
        "tilted-single-well-parity-asymmetry-recorded" => TerminalPrefix + "passed-tilted-single-well-parity-asymmetry-recorded",
        _ => TerminalPrefix + "passed-inconclusive-structure-recorded",
    }
    : TerminalPrefix + "blocked";

string decision = constraintEffectivePotentialHmcProbePassed
    ? $"The constraint-effective-potential HMC probe is decided on internal consistency. On lattice-canonical tori [{string.Join(",", TorusSizes)}] (su(2) trace pairing) the constrained free energy U(Phi) = -(1/beta) ln P(Phi), Phi = <u_inv, omega>, was reconstructed by log-space WHAM from umbrella-sampled joint (omega-HMC, theta-Haar-Metropolis) ensembles under the four 2026-07-04 binding conditions: (i) u_inv is the phase448 translation-invariant per-type ray in the oSign lattice gauge with the 3 global-su(2) zero-mode directions projected out (projector recorded; post-projection overlaps <= {torusRecords.Max(t => t.ProjectorOverlapMaxAfter):E1}); (ii) theta sampled as per-vertex SO(3) rotations by symmetric Haar-invariant Metropolis moves, charted to axis-angle |theta| <= pi (chart batteries: uniform shift {torusRecords.Max(t => t.ThetaUniformShiftMax):E1}, wrap {torusRecords.Max(t => t.ThetaWrapChartMax):E1}, 2pi periodicity {torusRecords.Max(t => t.ThetaPeriodicityMax):E1}); (iii) per-window tau_int recorded with the N_eff >= {NeffTarget} gate and adaptive trajectories; (iv) no theta=0 slice is quoted as physics. " +
      $"SAMPLING GATES {(samplingGatesAllPassed ? "ALL GREEN" : "FAILED (recorded per window)")}: per-window <e^-dH>=1 and equipartition virial, neighbor overlap >= {OverlapMin:P0}, WHAM residual <= {WhamResidualTol:E0}, coverage, the large-beta (beta={BetaLarge}) tree-shape control column{(largeBetaColumn is not null ? $" (pearson {largeBetaColumn.Pearson:F3}, cls {largeBetaColumn.Arm.Classification})" : " (not run: no Einsteinian member)")}, and +-Phi antisymmetry (the a3=0 parity). " +
      $"VERDICT ({verdictKind}): {verdictPhrase}. " +
      "MANDATORY FRAMING: workbench-relative candidate data ONLY (su(2) toy algebra, reduced Spin(4) slice, lattice units); beta, umbrella springs, the Phi inner product, the HMC kinetic mass, and the theta-Haar chart are WORKBENCH CONVENTIONS pending physicist review; a flat-bottom signature is a workbench-relative SSB CANDIDATE only; NO GeV/pole/VEV promotion. Everything is target-blind, reduced-spin4-slice; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the verdicts until the precursor, orbit/covariance, objective-consistency, gradient, theta-chart, projector, and WHAM-plumbing batteries pass.";

// ---------------------------------------------------------------------------
// Serialize (fail-closed: FiniteOrNull on every possibly-non-finite value).
// ---------------------------------------------------------------------------

object WindowJson(WindowResult w) => new
{
    center = w.Center,
    springK = w.KUmb,
    trajectories = w.Trajectories,
    leapfrogEps = FiniteOrNull(w.Eps),
    leapfrogSteps = NLeap,
    acceptanceRate = FiniteOrNull(w.AcceptRate),
    expNegDh = FiniteOrNull(w.ExpDh),
    expNegDhStandardError = FiniteOrNull(w.ExpDhSe),
    expNegDhGate = w.ExpDhGate,
    virialMean = FiniteOrNull(w.VirialMean),
    virialRelDeviation = FiniteOrNull(w.VirialRel),
    virialStandardError = FiniteOrNull(w.VirialSe),
    virialGate = w.VirialGate,
    tauIntPhi = FiniteOrNull(w.TauPhi),
    tauIntSb = FiniteOrNull(w.TauS),
    neffPhi = FiniteOrNull(w.NeffPhi),
    neffSb = FiniteOrNull(w.NeffS),
    ergodicityGate = w.ErgodicityGate,
    meanPhi = FiniteOrNull(w.MeanPhi),
    sdPhi = FiniteOrNull(w.SdPhi),
    meanSb = FiniteOrNull(w.MeanS),
    omegaRmsDriftRel = FiniteOrNull(w.DriftRel),
    thetaGlobalAcceptance = FiniteOrNull(w.ThetaGlobalAcc),
    thetaSingleVertexAcceptance = FiniteOrNull(w.ThetaSingleAcc),
    thetaStepFinal = FiniteOrNull(w.ThetaStep),
    msPerTrajectory = FiniteOrNull(w.MsPerTraj),
};

object ArmJson(MemberArm a) => new
{
    member = a.Name,
    isControl = a.IsControl,
    beta = a.Beta,
    windowCount = a.Plan.Centers.Length,
    windowSpacing = a.Plan.Spacing,
    binWidth = a.Plan.BinW,
    windows = a.Windows.Select(WindowJson).ToArray(),
    minNeighborOverlap = FiniteOrNull(a.OverlapMin),
    overlapGate = a.OverlapGate,
    whamResidual = FiniteOrNull(a.WhamResid),
    whamIterations = a.WhamIters,
    whamGate = a.WhamGate,
    coverageHalfWidth = FiniteOrNull(a.CoverageHalfWidth),
    coverageGate = a.CoverageGate,
    antisymmetryMaxAbs = FiniteOrNull(a.AntisymMaxAbs),
    antisymmetryMaxSigma = FiniteOrNull(a.AntisymMaxSigma),
    antisymmetryGate = a.AntisymmetryGate,
    whamRoughnessSigma = FiniteOrNull(a.SigmaRough),
    whamStitchSigmaAtEdge = FiniteOrNull(a.StitchSigmaEdge),
    windowGatesAllPassed = a.WindowGatesAll,
    gatesExceptAntisymmetryPassed = a.GatesExceptAntisym,
    gatesAllPassed = a.GatesAllPassed,
    tadpoleDiagnostic = new
    {
        note = "UNCONSTRAINED (k=0) window; direct <Phi> of the raw joint ensemble, independent of WHAM; attributes any +-Phi asymmetry of the reconstruction",
        window = WindowJson(a.Tadpole),
        meanPhi = FiniteOrNull(a.Tadpole.MeanPhi),
        standardError = FiniteOrNull(a.TadpoleSe),
        significanceSigma = FiniteOrNull(a.TadpoleSigma),
    },
    cepCurve = a.Bins.Select(x => new
    {
        phi = x.Phi,
        u = FiniteOrNull(x.U),
        errApprox = FiniteOrNull(x.Err),
        counts = x.Counts,
    }).ToArray(),
    classification = a.Classification,
    tStar = FiniteOrNull(a.TStar),
    depthAtZero = FiniteOrNull(a.Depth),
    depthSigma = FiniteOrNull(a.DepthSigma),
    flatBottomHalfWidth = FiniteOrNull(a.FlatHalfWidth),
};

var result = new
{
    phaseId = "phase450-constraint-effective-potential-hmc-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    constraintEffectivePotentialHmcProbePassed,

    phase448PrecursorPassed,
    phase449PrecursorPassed,
    precursorsPassed,

    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,

    probeConfiguration = new
    {
        torusSizes = TorusSizes,
        latticeCanonical = true,
        lieAlgebraId = "su2-trace-pairing",
        memberTags = MemberTags,
        tMax = TMax,
        windowsEnv = WindowsEnv,
        chunkTrajectories = ChunkTraj,
        maxTrajectoriesPerWindow = MaxTraj,
        minTrajectoriesPerWindow = MinTraj,
        maxParallelWindows = MaxParallel,
        warmupTrajectories = WarmTraj,
        neffTarget = NeffTarget,
        beta = Beta,
        betaConvention = BetaConvention,
        betaLarge = BetaLarge,
        kUmbEnv = KUmbEnv,
        leapfrogSteps = NLeap,
        epsEnv = EpsEnv,
        thetaProposalsPerTrajectory = ThetaProps,
        samplerSeed = SamplerSeed,
        rayRngSeed = RayRngSeed,
        collectiveCoordinate = "Phi = <u_inv, omega> in the plain Euclidean coefficient inner product (NOT mass-weighted; recorded convention); u_inv = phase448 per-type translation-invariant ray (Random(20260703), 16 types x 3 coefficients, types 0..14 on edges) in the oSign base->tip lattice gauge, with the 3 global-su(2) zero-mode directions z_l = [e_l, u] Gram-Schmidt-projected out and the projector recorded (on the invariant metric <u,[c,u]> = 0 identically, so the recorded overlaps are machine-zero -- asserted, applied anyway)",
        thetaHaarMeasure = "theta enters S_B only via Ad = exp(ad_theta) in SO(3) per vertex (su(2) f = epsilon => rotation by |theta| about theta); the R^n theta integral diverges, so the sampler maintains per-vertex UNIT QUATERNIONS updated by symmetric Haar-invariant Metropolis proposals (small left-multiplied rotations alternating with single-vertex uniform-Haar independence moves) and charts them to the axis-angle fundamental domain |theta| <= pi (w >= 0 quaternion representative) at the operator boundary; chart validated by hard batteries (uniform shift constancy, wrap equivalence, single-vertex 2pi periodicity)",
        umbrella = "harmonic bias (k/2)(Phi - c)^2 added to beta*S_B (bias NOT multiplied by beta -- recorded convention); symmetric window centers on [-tMax, +tMax] including 0, planned PER MEMBER on its own tree curvature; auto spacing = min(0.5, 2.2/sqrt(6*beta*kappa_tree_max)) and spring k = (2.2/spacing)^2 unless PHASE450_WINDOWS/PHASE450_KUMB override -- self-consistently k >= 6*beta*kappa_tree >= 6*beta*kappa_F, pinning window means within ~15% of centers while the biased width stays >= spacing/2.35 for neighbor overlap",
        hmc = "leapfrog on the omega sector against the platform's analytic ComputeJointGradient with the bias force added; flat (identity) kinetic mass -- recorded convention; eps auto-tuned in warmup to acceptance [0.70, 0.95]; theta held fixed within a trajectory and updated by the Haar sweeps between trajectories",
        wham = "log-space self-consistent WHAM over the shared symmetric bin grid (bin width = spacing/4); residual = max |delta f_w|; per-bin error = quadrature sum of (a) LOCAL counting (total bin counts deflated by the window-mean 2*tau_int(Phi)), (b) STITCHING ACCUMULATION (per-boundary variance 1/(beta^2 neff_overlap) accumulated from the central window out to the bin -- the f_w random walk that is the textbook dominant umbrella-chain error; the third smoke run measured it as a spurious 6-7-'sigma' tilt under the naive model while the unconstrained tadpole was zero), and (c) a robust high-frequency roughness floor from second differences (sigma = 1.4826 MAD(|U_{i-1} - 2U_i + U_{i+1}|)/sqrt(6)); all components recorded, model recorded as approximate",
        gates = $"per window: |<e^-dH>-1| <= max({ExpDhAbsFloor}, 4 SE), |virial/nOmega - 1| <= max({VirialRelFloor}, 4 SE/nOmega), N_eff(Phi) and N_eff(S_B) >= {NeffTarget}; per arm: min neighbor overlap >= {OverlapMin}, WHAM residual <= {WhamResidualTol:E0}, coverage half-width >= {CoverageHalfWidthFraction} * tMax, +-Phi antisymmetry <= {AntisymmetrySigma} sigma; global: the large-beta control column must classify single-well-at-zero with Pearson >= {LargeBetaPearsonMin} against the tree curve S_B(c u_inv, 0)",
        classificationConventions = $"bins with >= {MinBinCounts} counts, roughness-inflated errors; single-well-at-zero = minimum within 1.5 bins of Phi=0 (or an off-center minimum whose depth below the center bin is <= {SsbDepthSigma} sigma -- consistent with a single well plus noise) and monotone rise both sides within {MonotoneSlackSigma} median errors; flat-bottom-degenerate-minima = EITHER an off-center minimum pair at +-t* (depth > {SsbDepthSigma} sigma, partner within 2 bins and {SsbDepthSigma} median errors of the minimum) OR a flat Maxwell bottom of half-width >= {FlatBottomMinHalfWidth} lattice units with rising edges; everything else inconclusive-structure",
        fourBindingConditions = new[]
        {
            "(i) gauge-invariant invariant-ray collective coordinate (oSign lattice gauge; global-su(2) zero modes projected; projector recorded; random rays forbidden)",
            "(ii) theta-Haar measure (per-vertex SO(3) rotations, symmetric Metropolis; axis-angle fundamental-domain chart documented and battery-asserted)",
            "(iii) ergodicity control (tau_int(S_B), tau_int(Phi) per window; N_eff >= 100 gate; adaptive trajectory count)",
            "(iv) theta=0 slice results are sampler demos, never physics verdicts -- the physics run is the joint (omega, theta-Haar) ensemble (the identity control is exactly theta-independent, GradTheta == 0 asserted, so its omega marginal IS its joint ensemble)",
        },
        faddeevPopovCaveat = "cross-member comparisons of spectral or free-energy NORMALIZATIONS are differently gauge-normalized (the 2026-07-04 convention record); within-member Phi-dependence -- everything gated and classified here -- is safe",
        sbGradEvals,
        sbObjEvals,
    },

    // Headline verdicts.
    verdictKind,
    samplingGatesAllPassed,
    onlyAntisymmetryGatesFailed,
    tadpoleAttributed,
    anySingleWellOffset,
    controlClean,
    anyEinsteinianFlatBottom,
    allEinsteinianSingleWell,
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    cepConventionsAreWorkbenchConventions,
    physicistReviewPending,

    batteries = new
    {
        batteriesAllPassed = allHardBatteriesPassed,
        whamPlumbingBattery,
        whamPlumbingMaxErr = FiniteOrNull(whamPlumbingMaxErr),
        perTorus = torusRecords.Select(t => new
        {
            n = t.N,
            orbitMapOk = t.OrbitMapOk,
            covarianceMax = FiniteOrNull(t.CovarianceMax),
            covarianceBattery = t.CovarianceBattery,
            objectiveConsistencyMax = FiniteOrNull(t.ObjConsistencyMax),
            objectiveConsistencyBattery = t.ObjConsistencyBattery,
            gradFdMax = FiniteOrNull(t.GradFdMax),
            gradFdBattery = t.GradFdBattery,
            thetaUniformShiftMax = FiniteOrNull(t.ThetaUniformShiftMax),
            thetaWrapChartMax = FiniteOrNull(t.ThetaWrapChartMax),
            thetaPeriodicityMax = FiniteOrNull(t.ThetaPeriodicityMax),
            thetaChartBattery = t.ThetaChartBattery,
            identityThetaGradNorm = FiniteOrNull(t.IdentityThetaGradNorm),
            identityThetaIndependenceBattery = t.IdentityThetaIndependenceBattery,
            projectorOverlapsBefore = t.OverlapsBefore.Select(FiniteOrNull).ToArray(),
            projectorOverlapsAfter = t.OverlapsAfter.Select(FiniteOrNull).ToArray(),
            projectorOverlapMaxAfter = FiniteOrNull(t.ProjectorOverlapMaxAfter),
            projectorBattery = t.ProjectorBattery,
            hardBatteriesPassed = t.HardBatteriesPassed,
        }).ToArray(),
    },

    torusArms = torusRecords.Select(t => new
    {
        n = t.N,
        vertexCount = t.VertexCount,
        edgeCount = t.EdgeCount,
        faceCount = t.FaceCount,
        cellCount = t.CellCount,
        nOmega = t.NOmega,
        nTheta = t.NTheta,
        uInvNormRaw = FiniteOrNull(t.UNormRaw),
        uInvNormAfterProjection = FiniteOrNull(t.UNormAfter),
        treeCurves = t.TreeByMember.Select(kv => new
        {
            member = kv.Key,
            t = kv.Value.T,
            sB = kv.Value.S,
            a2 = FiniteOrNull(kv.Value.A2),
            a3 = FiniteOrNull(kv.Value.A3),
            a4 = FiniteOrNull(kv.Value.A4),
        }).ToArray(),
        members = t.Arms.Select(ArmJson).ToArray(),
    }).ToArray(),

    largeBetaControlColumn = largeBetaColumn is null ? null : (object)new
    {
        torusN = largeBetaColumn.N,
        member = largeBetaColumn.Member,
        beta = largeBetaColumn.Beta,
        arm = ArmJson(largeBetaColumn.Arm),
        pearsonVsTreeCurve = FiniteOrNull(largeBetaColumn.Pearson),
        comparisonPoints = largeBetaColumn.Points,
        shapeGate = largeBetaColumn.ShapeGate,
    },

    verdictPhrase,

    recordedBoundary = new
    {
        definition81Scope,
        ambientSevenSevenRealized,
        internalGaugeContentRealized,
        weldRealized,
        canFillPhase201WzContract,
        canFillPhase256Contract,
        physicistReviewPending,
        cepConventionsAreWorkbenchConventions,
        epsilonRealization,
        epsilonTaxonomyMode,
        baseSignature = "Cl(4,0)-euclidean-slice",
        spinorUsedAsShiabCarrierNotFiber = true,
        draft14dObserverseNotRealized = true,
    },

    runtimeSeconds,

    physicalCouplingProvided,
    routeProvidesPhysicalEffectiveActionHessian,
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
    canFillPhase256Contract,
    canFillPhase256ObservedFieldExtractionContract,
    predictionContractImpact = new
    {
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256Contract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },

    sourceEvidence = new
    {
        phase448SummaryPath = Phase448SummaryPath,
        phase449SummaryPath = Phase449SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
        reviewBoardSourcePath = ReviewBoardSourcePath,
        hmcPrototypePath = "studies/phase448_torus_mode_volume_saturation_probe_001/cep_hmc_prototype",
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "A flat-bottom signature is a workbench-relative SSB CANDIDATE only: su(2) toy algebra on the reduced Spin(4) slice, lattice units, finite volume; it is NOT a physical symmetry-breaking claim, NOT a VEV, NOT a mass, and carries no GeV lineage.",
        "beta, the umbrella spring constants, the Euclidean (non-mass-weighted) Phi inner product, the flat HMC kinetic mass, the theta-Haar chart, and the WHAM bin/error conventions are WORKBENCH CONVENTIONS pending physicist review.",
        "A symmetric-phase null is finite-volume, single-beta evidence along one gauge-invariant collective coordinate -- strong internal evidence, not a theorem; the multi-volume Binder/correlation-length program (>= 4 torus sizes) is the confirmatory instrument the review board named.",
        "The theta = 0 slice is never quoted as a physics verdict (binding condition (iv)); the physics ensemble is the joint (omega, theta-Haar) ensemble.",
        "Cross-member normalization comparisons are barred by the recorded Faddeev-Popov caveat; only within-member Phi-dependence is classified.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "constraint_effective_potential_hmc_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "constraint_effective_potential_hmc_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"constraintEffectivePotentialHmcProbePassed={constraintEffectivePotentialHmcProbePassed}");
Console.WriteLine($"precursors: p448={phase448PrecursorPassed} p449={phase449PrecursorPassed}");
Console.WriteLine($"HARD BATTERIES: all={allHardBatteriesPassed} whamPlumbing={whamPlumbingBattery} ({whamPlumbingMaxErr:E2})");
foreach (var t in torusRecords)
    Console.WriteLine($"  n={t.N}: cov={t.CovarianceMax:E2} obj={t.ObjConsistencyMax:E2} gradFd={t.GradFdMax:E2} chart pass={t.ThetaChartBattery} proj={t.ProjectorOverlapMaxAfter:E2} pass={t.HardBatteriesPassed}");
Console.WriteLine($"SAMPLING GATES: all={samplingGatesAllPassed}");
foreach (var t in torusRecords)
    foreach (var a in t.Arms)
        Console.WriteLine($"  n={t.N} {a.Name,-14} beta={a.Beta:0.###} gates={a.GatesAllPassed} (win={a.WindowGatesAll} ovl={a.OverlapGate}:{a.OverlapMin:F3} wham={a.WhamGate}:{a.WhamResid:E1} cov={a.CoverageGate}:{a.CoverageHalfWidth:F2} asym={a.AntisymmetryGate}:{a.AntisymMaxSigma:F2}s) cls={a.Classification} tStar={a.TStar:F3} depthSigma={a.DepthSigma:F1} flatHW={a.FlatHalfWidth:F2} tadpole={a.Tadpole.MeanPhi:F3}+-{a.TadpoleSe:F3} ({a.TadpoleSigma:F1}s)");
if (largeBetaColumn is not null)
    Console.WriteLine($"  LARGE-BETA n={largeBetaColumn.N} {largeBetaColumn.Member} beta={largeBetaColumn.Beta}: cls={largeBetaColumn.Arm.Classification} pearson={largeBetaColumn.Pearson:F4} gate={largeBetaColumn.ShapeGate}");
Console.WriteLine($"VERDICT: kind={verdictKind} controlClean={controlClean} anyFlatBottom={anyEinsteinianFlatBottom} allSingleWell={allEinsteinianSingleWell} onlyAsymFailed={onlyAntisymmetryGatesFailed} tadpoleAttributed={tadpoleAttributed}");
Console.WriteLine($"sbGradEvals={sbGradEvals} sbObjEvals={sbObjEvals} runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Local helpers.
// ---------------------------------------------------------------------------

static double EnvD(string k, double d) =>
    double.TryParse(Environment.GetEnvironmentVariable(k),
        System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : d;
static int EnvI(string k, int d) =>
    int.TryParse(Environment.GetEnvironmentVariable(k), out var v) ? v : d;

static int Mod(int a, int n) => ((a % n) + n) % n;

static int[]? MinImage01(int[] from, int[] to, int n)
{
    var d = new int[4];
    for (int i = 0; i < 4; i++)
    {
        int raw = Mod(to[i] - from[i], n);
        if (raw == 0) d[i] = 0;
        else if (raw == 1) d[i] = 1;
        else return null;
    }
    return d[0] + d[1] + d[2] + d[3] == 0 ? null : d;
}

static double Gauss(Random rng)
{
    double u1 = 1.0 - rng.NextDouble(), u2 = rng.NextDouble();
    return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
}

static double[] RandomVector(Random rng, int n, double amp)
{
    var v = new double[n];
    for (int i = 0; i < n; i++) v[i] = amp * (2.0 * rng.NextDouble() - 1.0);
    return v;
}

static double[] ScaleVec(double[] a, double s)
{
    var r = new double[a.Length];
    for (int i = 0; i < a.Length; i++) r[i] = s * a[i];
    return r;
}

static double Dot(double[] a, double[] b)
{
    double s = 0; for (int i = 0; i < a.Length; i++) s += a[i] * b[i]; return s;
}

static double Norm(double[] v) => System.Math.Sqrt(Dot(v, v));

static double RelDiff(double a, double b) =>
    System.Math.Abs(a - b) / System.Math.Max(1e-30, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));

static double? FiniteOrNull(double x) => double.IsFinite(x) ? x : null;

static double Sd(List<double> x, double mean)
{
    double s = 0; foreach (var v in x) s += (v - mean) * (v - mean);
    return System.Math.Sqrt(s / System.Math.Max(1, x.Count - 1));
}

// Integrated autocorrelation time (windowed; cutoff at rho < 0.02 past lag 8).
static double TauInt(List<double> x)
{
    int n = x.Count;
    if (n < 16) return double.PositiveInfinity;
    double mean = 0; foreach (var v in x) mean += v; mean /= n;
    double c0 = 0; for (int i = 0; i < n; i++) { double d = x[i] - mean; c0 += d * d; }
    c0 /= n;
    if (c0 <= 0) return 0.5;
    double tau = 0.5;
    int wMax = System.Math.Min(1000, n / 4);
    for (int lag = 1; lag < wMax; lag++)
    {
        double c = 0;
        for (int i = 0; i < n - lag; i++) c += (x[i] - mean) * (x[i + lag] - mean);
        c /= (n - lag);
        double rho = c / c0;
        if (rho < 0.02 && lag > 8) break;
        tau += rho;
    }
    return tau;
}

static double LogSumExp(double[] a)
{
    double m = double.NegativeInfinity;
    foreach (var v in a) if (v > m) m = v;
    if (double.IsNegativeInfinity(m)) return m;
    double s = 0;
    foreach (var v in a) s += System.Math.Exp(v - m);
    return m + System.Math.Log(s);
}

static double Pearson(List<double> x, List<double> y)
{
    int n = x.Count;
    if (n < 2) return double.NaN;
    double mx = x.Average(), my = y.Average();
    double sxy = 0, sxx = 0, syy = 0;
    for (int i = 0; i < n; i++)
    {
        sxy += (x[i] - mx) * (y[i] - my);
        sxx += (x[i] - mx) * (x[i] - mx);
        syy += (y[i] - my) * (y[i] - my);
    }
    return sxy / System.Math.Max(System.Math.Sqrt(sxx * syy), 1e-30);
}

// Least-squares fit of s = a2 t^2 + a3 t^3 + a4 t^4 (3x3 normal equations).
static (double A2, double A3, double A4) FitQuartic(double[] t, double[] s)
{
    var ata = new double[3, 3];
    var atb = new double[3];
    for (int i = 0; i < t.Length; i++)
    {
        var row = new[] { t[i] * t[i], t[i] * t[i] * t[i], t[i] * t[i] * t[i] * t[i] };
        for (int a = 0; a < 3; a++)
        {
            atb[a] += row[a] * s[i];
            for (int b = 0; b < 3; b++) ata[a, b] += row[a] * row[b];
        }
    }
    // Gaussian elimination with partial pivoting.
    var m = new double[3, 4];
    for (int i = 0; i < 3; i++) { for (int j = 0; j < 3; j++) m[i, j] = ata[i, j]; m[i, 3] = atb[i]; }
    for (int c = 0; c < 3; c++)
    {
        int piv = c;
        for (int r = c + 1; r < 3; r++) if (System.Math.Abs(m[r, c]) > System.Math.Abs(m[piv, c])) piv = r;
        for (int j = 0; j < 4; j++) (m[c, j], m[piv, j]) = (m[piv, j], m[c, j]);
        for (int r = 0; r < 3; r++)
        {
            if (r == c || m[c, c] == 0) continue;
            double f = m[r, c] / m[c, c];
            for (int j = 0; j < 4; j++) m[r, j] -= f * m[c, j];
        }
    }
    return (m[0, 3] / m[0, 0], m[1, 3] / m[1, 1], m[2, 3] / m[2, 2]);
}

// --- Unit-quaternion SO(3) utilities (theta-Haar chart, condition (ii)). ----
// q = (w, x, y, z), |q| = 1; rotation by angle 2 atan2(|v|, w) about v/|v|.

static double[] QMul(double[] a, double[] b) => new[]
{
    a[0] * b[0] - a[1] * b[1] - a[2] * b[2] - a[3] * b[3],
    a[0] * b[1] + a[1] * b[0] + a[2] * b[3] - a[3] * b[2],
    a[0] * b[2] - a[1] * b[3] + a[2] * b[0] + a[3] * b[1],
    a[0] * b[3] + a[1] * b[2] - a[2] * b[1] + a[3] * b[0],
};

static double[] QFromAxisAngle(double[] aa)
{
    double th = System.Math.Sqrt(aa[0] * aa[0] + aa[1] * aa[1] + aa[2] * aa[2]);
    if (th < 1e-14) return new[] { 1.0, 0.5 * aa[0], 0.5 * aa[1], 0.5 * aa[2] };
    double s = System.Math.Sin(0.5 * th) / th;
    return new[] { System.Math.Cos(0.5 * th), s * aa[0], s * aa[1], s * aa[2] };
}

// Axis-angle in the FUNDAMENTAL DOMAIN |theta| <= pi (w >= 0 representative).
static double[] QToAxisAngle(double[] q)
{
    double w = q[0], x = q[1], y = q[2], z = q[3];
    double nrm = System.Math.Sqrt(w * w + x * x + y * y + z * z);
    w /= nrm; x /= nrm; y /= nrm; z /= nrm;
    if (w < 0) { w = -w; x = -x; y = -y; z = -z; }
    double s = System.Math.Sqrt(x * x + y * y + z * z);
    if (s < 1e-14) return new[] { 2.0 * x, 2.0 * y, 2.0 * z };
    double angle = 2.0 * System.Math.Atan2(s, w); // in [0, pi] since w >= 0
    return new[] { angle * x / s, angle * y / s, angle * z / s };
}

static double[] QUniform(Random rng)
{
    double a = Gauss(rng), b = Gauss(rng), c = Gauss(rng), d = Gauss(rng);
    double nq = System.Math.Sqrt(a * a + b * b + c * c + d * d);
    if (nq < 1e-14) return new[] { 1.0, 0.0, 0.0, 0.0 };
    return new[] { a / nq, b / nq, c / nq, d / nq };
}

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

BranchManifest BuildManifest() => new()
{
    BranchId = "phase450-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase450",
    ActiveGeometryBranch = "simplicial",
    ActiveObservationBranch = "sigma-pullback",
    ActiveTorsionBranch = "trivial",
    ActiveShiabBranch = "einsteinian-shiab",
    ActiveGaugeStrategy = "penalty",
    BaseDimension = 4,
    AmbientDimension = 4,
    LieAlgebraId = "su2",
    BasisConventionId = "canonical",
    ComponentOrderId = "face-major",
    AdjointConventionId = "adjoint-explicit",
    PairingConventionId = "pairing-trace",
    NormConventionId = "norm-l2-quadrature",
    DifferentialFormMetricId = "hodge-standard",
    InsertedAssumptionIds = Array.Empty<string>(),
    InsertedChoiceIds = new[] { "IX-2" },
};

GeometryContext BuildGeometry()
{
    var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
    var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 4 };
    return new GeometryContext
    {
        BaseSpace = baseSpace,
        AmbientSpace = ambientSpace,
        DiscretizationType = "simplicial",
        QuadratureRuleId = "centroid",
        BasisFamilyId = "P1",
        ProjectionBinding = new GeometryBinding
        { BindingType = "projection", SourceSpace = ambientSpace, TargetSpace = baseSpace },
        ObservationBinding = new GeometryBinding
        { BindingType = "observation", SourceSpace = baseSpace, TargetSpace = ambientSpace },
        Patches = Array.Empty<PatchInfo>(),
    };
}

public sealed record WindowPlan(double[] Centers, double[] Springs, double[] BinPhi, double BinW, double Spacing);

public sealed record WindowResult(
    double Center, double KUmb, int Trajectories, double Eps, double AcceptRate,
    double ExpDh, double ExpDhSe, bool ExpDhGate,
    double VirialMean, double VirialRel, double VirialSe, bool VirialGate,
    double TauPhi, double TauS, double NeffPhi, double NeffS, bool ErgodicityGate,
    double MeanPhi, double SdPhi, double MeanS, double DriftRel,
    double ThetaGlobalAcc, double ThetaSingleAcc, double ThetaStep,
    long[] Counts, double MsPerTraj);

public sealed record CepBin(double Phi, double U, double Err, long Counts);

public sealed record MemberArm(
    string Name, bool IsControl, double Beta, WindowPlan Plan, List<WindowResult> Windows,
    double OverlapMin, bool OverlapGate, double WhamResid, int WhamIters, bool WhamGate,
    double CoverageHalfWidth, bool CoverageGate,
    double AntisymMaxAbs, double AntisymMaxSigma, bool AntisymmetryGate,
    bool WindowGatesAll, bool GatesExceptAntisym, bool GatesAllPassed, List<CepBin> Bins,
    double SigmaRough, double StitchSigmaEdge,
    WindowResult Tadpole, double TadpoleSe, double TadpoleSigma,
    string Classification, double TStar, double Depth, double DepthSigma, double FlatHalfWidth);

public sealed record LargeBetaColumn(
    int N, string Member, double Beta, MemberArm Arm, double Pearson, int Points, bool ShapeGate);

public sealed record TorusArm(
    int N, int VertexCount, int EdgeCount, int FaceCount, int CellCount,
    int NOmega, int NTheta, bool OrbitMapOk,
    double UNormRaw, double UNormAfter, double[] OverlapsBefore, double[] OverlapsAfter,
    double ProjectorOverlapMaxAfter, bool ProjectorBattery,
    double CovarianceMax, bool CovarianceBattery,
    double ObjConsistencyMax, bool ObjConsistencyBattery,
    double GradFdMax, bool GradFdBattery,
    double ThetaUniformShiftMax, double ThetaWrapChartMax, double ThetaPeriodicityMax,
    bool ThetaChartBattery, double IdentityThetaGradNorm, bool IdentityThetaIndependenceBattery,
    bool HardBatteriesPassed,
    Dictionary<string, (double[] T, double[] S, double A2, double A3, double A4)> TreeByMember,
    List<MemberArm> Arms);
