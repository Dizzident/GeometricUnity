using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase449: VARIATIONAL GAUSSIAN (CJT-Hartree) effective potential probe on the
// draft-canonical Einsteinian Shiab (4D base, spinor-realized reduced Lambda^2
// slice, epsilon-conjugation). The 2026-07-04 review board's #1 named
// experiment after the positive-subspace saddle V_eff was RETIRED
// (unconditionally: not stationary in omega, discontinuous at negative-mode
// crossings, loop hierarchy inverted, unbounded below at t->0).
//
// THE QUESTION. Tree level has no scale (S_B = a2 t^2 + a4 t^4 with
// a2, a4 > 0 on invariant rays; a3 = 0 exactly on invariant rays by the
// omega -> -omega Z2). No perturbative Coleman-Weinberg scale exists
// (443/446/447/448). The NON-PERTURBATIVE question is open, and the 30-235
// transverse negative modes the retired convention discarded are exactly where
// a dynamical scale could hide. The variational Gaussian (CJT-Hartree)
// effective potential is the cheapest rigorous instrument that keeps them:
// for ANY Gaussian ansatz, V_G is a Feynman-Jensen UPPER BOUND on the
// constrained free energy, and the Hartree gap equation resums the quartic
// into a self-consistent positive mass. A null here (no member develops an
// interior structure) is STRONG evidence against a scale at ansatz level;
// a positive is MEAN-FIELD-ONLY and requires the phase450 constraint-EP HMC
// for confirmation. Neither promotes anything.
//
// CONSTRUCTION. At each composite point (omega = t*u, theta*(t*u)):
//   (1) joint FD Hessian (verbatim Phase443/446/447 stencil at HessStep 1e-4)
//       eigen-decomposed with eigenvectors (Jacobi);
//   (2) T4 pair table over ALL modes above the zero tolerance -- positive AND
//       negative (unlike Phase447's positive-only figure-eight): T4[a,a] via
//       D4Diag and T4[a,b] via D4MixedPair on unit eigenvectors at
//       StencilH = 0.1 (the LARGE-h-by-design convention with shared axis
//       evaluations; the omega sector is EXACTLY quartic so the stencils are
//       h-independent there, and the transcendental theta truncation is
//       controlled by the h-vs-h/2 Richardson battery -- Phase447's lessons);
//   (3) DIAGONAL GAUSSIAN ANSATZ in the H-eigenbasis,
//       Sigma = sum_a sigma_a v_a v_a^T with sigma_a > 0 (beta = 1 recorded
//       convention). The Gaussian expectation of the quartic action gives
//         V_G(t; sigma) = S_B + (1/2) sum_a lambda_a sigma_a
//                         + (1/8) sum_{a,b} T4[a,b] sigma_a sigma_b
//                         - (1/2) sum_a log sigma_a
//       (the additive constant (n_modes/2)(1 + log 2 pi) is DROPPED and
//       recorded per point). Stationarity in sigma is the HARTREE GAP EQUATION
//         1/sigma_a = m_a := lambda_a + (1/2) sum_b T4[a,b] sigma_b,
//       solved by damped fixed-point iteration (alpha ~ 0.3, adaptive halving
//       on divergence) from sigma_a = 1/|lambda_a| (clipped). FAIL-CLOSED: no
//       floor-clamping into fake convergence -- if any m_a <= 0 or no
//       convergence in 500 iterations, the point records
//       hartreeSelfConsistentSolutionExists = false.
//   (4) V_G(t) = the converged functional value; classification on the RAW
//       curve by the strict-local-minimum rule (no fits -- the Phase446
//       lesson). The rays are random open-mesh rays, so only t > 0 is probed;
//       the a3 = 0 Z2 (+-t degeneracy) note is recorded, not tested.
//
// BOUND CAVEAT (recorded honestly): the Feynman-Jensen bound applies to the
// Gaussian expectation of the EXACT action. S_B is exactly quartic in omega,
// so on the omega sector the functional is the exact bound; in theta S_B is
// transcendental (Ad = exp(ad_theta), the Phase442/447 structure result) and
// the quartic Taylor truncation used here is a RECORDED WORKBENCH CONVENTION.
//
// BATTERIES (fail-closed): theta point gate verbatim (relative 1e-8 OR
// absolute floor 1e-10); identity exact-quartic h-independence (h vs h/2
// <= 1e-6); Einsteinian Richardson (<= 5e-2); offset immunity (S_B + 1000
// leaves T4 unchanged, <= 1e-5); gap-equation residual <= 1e-8 per converged
// point with all m_a > 0; solver-plumbing exactness control (T4 := 0 on the
// identity anchor's positive modes must reproduce sigma_a lambda_a = 1 to
// 1e-10, and must FAIL on a negative-mode set); bound-direction descent
// (V_G(converged) <= V_G(initial), monotonicity recorded); ansatz-rotation
// honesty check at one anchor per Einsteinian member (diagonal ansatz in a
// seeded pairwise-rotated basis within the 40 softest modes -- both values
// RECORDED, basis dependence measured, not gated).
//
// MANDATORY FRAMING. Workbench-relative candidate data ONLY (su(2) toy
// algebra on the reduced Spin(4) slice, lattice units); the diagonal Gaussian
// ansatz and beta = 1 are workbench conventions pending physicist review; NO
// GeV/pole/VEV promotion either way. The phase PASSES on internal consistency
// REGARDLESS of the verdict.
//
// Fail-closed: target-blind; reduced-spin4-slice; no scale/pole/GeV lineage;
// no Phase201/Phase256 contract field filled; nothing promoted either way.

const string DefaultOutputDir = "studies/phase449_variational_gaussian_effective_potential_probe_001/output";
const string Phase447SummaryPath = "studies/phase447_two_loop_saturation_probe_001/output/two_loop_saturation_probe_summary.json";
const string Phase448SummaryPath = "studies/phase448_torus_mode_volume_saturation_probe_001/output/torus_mode_volume_saturation_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ReviewBoardSourcePath = "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md";
const string ApplicationSubjectKind = "variational-gaussian-effective-potential-probe";
const string TerminalPrefix = "variational-gaussian-effective-potential-probe-";

// Probe configuration (committed defaults; env overrides mirror Phase443/446/447).
int MeshRefinement = int.TryParse(Environment.GetEnvironmentVariable("PHASE449_MESH_REFINEMENT"), out int mr) ? mr : 1;
int RaySeedCount = int.TryParse(Environment.GetEnvironmentVariable("PHASE449_RAYS"), out int rs) ? rs : 2;
double TMax = double.TryParse(Environment.GetEnvironmentVariable("PHASE449_TMAX"), out double tm) ? tm : 3.0;
int GridN = int.TryParse(Environment.GetEnvironmentVariable("PHASE449_GRIDN"), out int gn) ? gn : 16;
const int RngSeed = 20260703; // MUST equal Phase445/446/447: identical ray directions.

// Directional FD step. LARGE by design (the Phase447 lesson): S_B is an exact
// quartic in omega, so 4th-order stencils are h-INDEPENDENT on the omega
// sector, and a large h lifts the stencil numerators far above the
// double-precision cancellation floor (Phase447 PROVED h = 5e-3 is
// noise-dominated via the offset-immunity battery). The transcendental theta
// truncation error is controlled by the h-vs-h/2 Richardson battery.
double StencilH = double.TryParse(Environment.GetEnvironmentVariable("PHASE449_STENCIL_H"), out double sh) ? sh : 0.1;
const double RichardsonHFactor = 0.5;  // battery: T4 at h and h/2 must agree
const double RichardsonRelTol = 5e-2;  // Einsteinian (transcendental) Richardson ceiling
const double IdentityExactRelTol = 1e-6; // identity (exact quartic) stencil h-independence
// Offset immunity is gated ENTRYWISE on the battery T4 table
// (max |T4_shift - T4| / max |T4|) -- the direct reading of "S_B + 1000
// leaves T4 unchanged"; the residual is pure FP cancellation from the +1000
// magnitude shift (measured ~1.6e-6 at h=0.1 in the smoke run). The
// 1/|lambda|^2-weighted battery scalar over the SOFTEST modes amplifies that
// benign absolute noise on near-zero entries (~4e-5) and is RECORDED, not
// gated (the Richardson gates stay on the weighted scalar: there the error
// being probed is the physical theta h-truncation of the load-bearing
// contraction, not FP noise).
const double OffsetImmunityRelTol = 1e-5;
const int BatterySubsetModes = 24;     // softest-mode subset for the h/offset batteries

// Hartree gap-equation solver knobs (fail-closed; no floor-clamping).
const int GapMaxIter = 500;
const double GapDampingInit = 0.3;
const double GapConvTol = 1e-8;        // max_a |sigma_a m_a - 1| at convergence
const double GapAlphaFloor = 1e-6;     // adaptive halving lower bound => stalled
const double SigmaInitClipLo = 1e-6;
const double SigmaInitClipHi = 1e3;
const double PlumbingTol = 1e-10;      // T4:=0 plumbing check: |sigma lambda - 1|
const double PlumbingSolveTol = 1e-12; // tighter solver tol for the plumbing run
const int RotationSoftModes = 40;      // ansatz-rotation honesty check span
const double BetaConvention = 1.0;     // recorded convention

// Theta-stationarity solver knobs (verbatim Phase443/446/447 hardened solver).
const int ThetaMaxIter = 1000;
const double ThetaTolRel = 1e-9;
const double ThetaResidualBatteryRel = 1e-8;
const double ThetaAbsGradFloor = 1e-10; // Phase446 small-t dimensional-gate convention
const double ThetaFdStep = 1e-6;

// Hessian / eigenvalue knobs (verbatim).
const double HessStep = 1e-4;
const double ZeroModeRelTol = 1e-8;
const double ZeroModeAbsFloor = 1e-10;
const double LinearizeThetaFdTol = 1e-6;

var outputDir = Environment.GetEnvironmentVariable("PHASE449_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors: Phase447 (two-loop lever decided non-perturbative; the saddle
// V_eff object was subsequently RETIRED by the review board) and Phase448
// (mode-volume lever decided negative). Gate on their internal-consistency
// passed booleans -- the board's binding record supersedes their verdict use.
// ---------------------------------------------------------------------------

using var phase447 = JsonDocument.Parse(File.ReadAllText(Phase447SummaryPath));
bool phase447PrecursorPassed =
    JsonBool(phase447.RootElement, "twoLoopSaturationProbePassed") is true;

using var phase448 = JsonDocument.Parse(File.ReadAllText(Phase448SummaryPath));
bool phase448PrecursorPassed =
    JsonBool(phase448.RootElement, "torusModeVolumeSaturationProbePassed") is true;

bool precursorsPassed = phase447PrecursorPassed && phase448PrecursorPassed;

// ---------------------------------------------------------------------------
// Machinery (verbatim Phase443/446/447): 4D mesh, su(2) trace pairing,
// trivial torsion.
// ---------------------------------------------------------------------------

var mesh = SimplicialMeshGenerator.CreateUniform4D(MeshRefinement);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
int nOmega = mesh.EdgeCount * dimG;
int nTheta = mesh.VertexCount * dimG;
int nJoint = nOmega + nTheta;

var mass = new CpuMassMatrix(mesh, algebra);
var manifest = BuildManifest();
var geometry = BuildGeometry();
double latticeScale = MeanEdgeLength();

var controlMember = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Id0,
    Phi2 = InvariantElementSpec.None,
    EpsilonMode = "trivial",
};
var controlOp = new EinsteinianShiabOperator(mesh, algebra, controlMember);

var members = new List<MemberSpec> { new("identity", controlMember, controlOp, IsControl: true) };
foreach (var (phi1, phi2, tag, c) in new[]
         {
             (InvariantElementSpec.Sd2, InvariantElementSpec.Id0, "sd2-id0", 0.5),
             (InvariantElementSpec.Asd2, InvariantElementSpec.Id0, "asd2-id0", 0.5),
         })
{
    var m = new EinsteinianShiabFamilyMember
    {
        Phi1 = phi1, Phi2 = phi2, EinsteinCoefficient = c, EpsilonMode = "independent-theta",
    };
    members.Add(new MemberSpec($"{tag}/c{c:0.###}", m, new EinsteinianShiabOperator(mesh, algebra, m), IsControl: false));
}

Func<double[], double[], double[]> PinnedEval(EinsteinianShiabOperator op) => (omega, theta) =>
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
    return op.EvaluateWithTheta(f, conn.ToFieldTensor(), theta, manifest, geometry).Coefficients;
};

Func<double[], double[], double[]> ControlEvalOf(EinsteinianShiabOperator op) => (omega, _) =>
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
    return op.Evaluate(f, conn.ToFieldTensor(), manifest, geometry).Coefficients;
};

double SB(Func<double[], double[], double[]> eval, double[] omega, double[] theta)
{
    var s = FaceTensor(eval(omega, theta));
    return 0.5 * mass.InnerProduct(s, s);
}

var stopwatch = Stopwatch.StartNew();
long sbEvalCount = 0;

// ---------------------------------------------------------------------------
// Theta*-stationarity solve (VERBATIM Phase443/446/447 hardened Newton).
// ---------------------------------------------------------------------------

ThetaSolveResult SolveThetaStar(
    Func<double[], double[], double[]> eval,
    EinsteinianShiabOperator? op,
    double[] omega,
    double[]? warmStart = null)
{
    var theta = warmStart is not null ? (double[])warmStart.Clone() : new double[nTheta];
    var traj = new List<double>();

    if (op == null)
    {
        double sbCur = SB(eval, omega, theta);
        double gnorm = double.PositiveInfinity; double g0norm = double.NaN; int it = 0;
        for (; it < ThetaMaxIter; it++)
        {
            var g = new double[nTheta];
            for (int k = 0; k < nTheta; k++)
            {
                var tp = (double[])theta.Clone(); tp[k] += ThetaFdStep;
                var tmn = (double[])theta.Clone(); tmn[k] -= ThetaFdStep;
                g[k] = (SB(eval, omega, tp) - SB(eval, omega, tmn)) / (2.0 * ThetaFdStep);
            }
            gnorm = Norm(g); traj.Add(gnorm);
            if (double.IsNaN(g0norm)) g0norm = System.Math.Max(1.0, gnorm);
            if (gnorm <= ThetaTolRel * g0norm) break;
            double stepLen = 0.5; bool stepped = false;
            while (stepLen > 1e-9)
            {
                var cand = new double[nTheta];
                for (int d = 0; d < nTheta; d++) cand[d] = theta[d] - stepLen * g[d] / gnorm;
                double sbNew = SB(eval, omega, cand);
                if (sbNew < sbCur - 1e-15) { theta = cand; sbCur = sbNew; stepped = true; break; }
                stepLen *= 0.5;
            }
            if (!stepped) break;
        }
        double g0 = System.Math.Max(1.0, double.IsNaN(g0norm) ? 1.0 : g0norm);
        return new ThetaSolveResult(theta, gnorm / g0, gnorm, it, gnorm <= ThetaResidualBatteryRel * g0,
            double.NaN, -1, double.NaN, traj.ToArray());
    }

    var conn0 = new ConnectionField(mesh, algebra, omega);
    var fTensor = CurvatureAssembler.Assemble(conn0).ToFieldTensor();
    var connTensor = conn0.ToFieldTensor();
    double[] Gradient(double[] th)
    {
        var sH = FaceTensor(op.EvaluateWithTheta(fTensor, connTensor, th, manifest, geometry).Coefficients);
        var g = new double[nTheta];
        for (int k = 0; k < nTheta; k++)
        {
            var e = new double[nTheta]; e[k] = 1.0;
            var jk = FaceTensor(op.LinearizeTheta(fTensor, connTensor, th, e, manifest, geometry).Coefficients);
            g[k] = mass.InnerProduct(sH, jk);
        }
        return g;
    }

    double gradNorm;
    int iter = 0;
    double mu = 1e-6;
    double[,] hTheta = new double[nTheta, nTheta];
    var gCur = Gradient(theta);
    gradNorm = Norm(gCur);
    double gradScale = System.Math.Max(gradNorm, 1e-12);
    double gateNorm = System.Math.Max(ThetaTolRel * gradScale, 1e-12);
    traj.Add(gradNorm);

    for (; iter < ThetaMaxIter && gradNorm > gateNorm; iter++)
    {
        double hFd = 1e-5;
        hTheta = new double[nTheta, nTheta];
        for (int j = 0; j < nTheta; j++)
        {
            var tj = (double[])theta.Clone(); tj[j] += hFd;
            var gj = Gradient(tj);
            for (int i = 0; i < nTheta; i++) hTheta[i, j] = (gj[i] - gCur[i]) / hFd;
        }
        for (int i = 0; i < nTheta; i++)
            for (int j = i + 1; j < nTheta; j++)
            {
                double s = 0.5 * (hTheta[i, j] + hTheta[j, i]);
                hTheta[i, j] = s; hTheta[j, i] = s;
            }

        var hh = new double[nTheta, nTheta];
        for (int i = 0; i < nTheta; i++)
            for (int j = 0; j < nTheta; j++)
            {
                double s = 0;
                for (int p = 0; p < nTheta; p++) s += hTheta[i, p] * hTheta[p, j];
                hh[i, j] = s;
            }
        var hg = new double[nTheta];
        for (int i = 0; i < nTheta; i++)
        {
            double s = 0;
            for (int p = 0; p < nTheta; p++) s += hTheta[i, p] * gCur[p];
            hg[i] = s;
        }
        if (iter == 0)
        {
            double trHH = 0;
            for (int i = 0; i < nTheta; i++) trHH += hh[i, i];
            mu = System.Math.Max(1e-14, 1e-6 * trHH / nTheta);
        }

        bool stepped = false;
        for (int tries = 0; tries < 40 && !stepped; tries++)
        {
            var aug = (double[,])hh.Clone();
            for (int d = 0; d < nTheta; d++) aug[d, d] += mu;
            var rhs = new double[nTheta];
            for (int d = 0; d < nTheta; d++) rhs[d] = -hg[d];
            var step = SolveSpd(aug, rhs);
            if (step == null) { mu *= 4.0; continue; }
            var cand = new double[nTheta];
            for (int d = 0; d < nTheta; d++) cand[d] = theta[d] + step[d];
            var gCand = Gradient(cand);
            double gCandNorm = Norm(gCand);
            if (gCandNorm < gradNorm)
            {
                theta = cand; gCur = gCand; gradNorm = gCandNorm;
                mu = System.Math.Max(mu * 0.4, 1e-12); stepped = true;
            }
            else mu *= 4.0;
        }
        traj.Add(gradNorm);
        if (!stepped) break;
    }

    var eigTheta = JacobiEigen(hTheta, wantVectors: false).Values;
    double maxAbsEig = 0.0, minAbsNonzero = double.PositiveInfinity;
    foreach (double e in eigTheta) maxAbsEig = System.Math.Max(maxAbsEig, System.Math.Abs(e));
    double rankTol = System.Math.Max(1e-12, 1e-9 * maxAbsEig);
    int rank = 0;
    foreach (double e in eigTheta)
        if (System.Math.Abs(e) > rankTol) { rank++; minAbsNonzero = System.Math.Min(minAbsNonzero, System.Math.Abs(e)); }
    double cond = (rank > 0 && minAbsNonzero > 0) ? maxAbsEig / minAbsNonzero : double.PositiveInfinity;

    return new ThetaSolveResult(theta, gradNorm / gradScale, gradNorm, iter,
        gradNorm <= System.Math.Max(ThetaResidualBatteryRel * gradScale, 1e-11), mu, rank, cond, traj.ToArray());
}

// ---------------------------------------------------------------------------
// Composite point (verbatim conventions): theta*, joint FD Hessian, Jacobi
// eigen-decomposition WITH eigenvectors, one-loop diagnostic counts.
// ---------------------------------------------------------------------------

PointData EvalPoint(
    Func<double[], double[], double[]> eval,
    EinsteinianShiabOperator? op,
    double[] uOmega, double t, bool isControl,
    double[]? thetaWarmStart = null)
{
    var omega = Scale(uOmega, t);

    ThetaSolveResult th = isControl
        ? new ThetaSolveResult(new double[nTheta], 0.0, 0.0, 0, true, double.NaN, 0, double.NaN, new[] { 0.0 })
        : SolveThetaStar(eval, op, omega, thetaWarmStart);
    if (!isControl && !th.Converged)
    {
        var rescueRng = new Random(97);
        for (int attempt = 0; attempt < 3 && !th.Converged; attempt++)
        {
            var start = new double[nTheta];
            double amp = 0.05 * (attempt + 1);
            for (int i = 0; i < nTheta; i++)
                start[i] = (thetaWarmStart is not null ? thetaWarmStart[i] : th.Theta[i]) + amp * (2.0 * rescueRng.NextDouble() - 1.0);
            var retry = SolveThetaStar(eval, op, omega, start);
            if (retry.Converged || retry.ResidualNorm < th.ResidualNorm) th = retry;
        }
    }

    double sB = SB(eval, omega, th.Theta);

    // Joint composite closure Sx(x): x = (omega, theta).
    double Sx(double[] x)
    {
        var w = new double[nOmega]; Array.Copy(x, 0, w, 0, nOmega);
        var tt = new double[nTheta]; Array.Copy(x, nOmega, tt, 0, nTheta);
        sbEvalCount++;
        return SB(eval, w, tt);
    }

    var x0 = new double[nJoint];
    Array.Copy(omega, 0, x0, 0, nOmega);
    Array.Copy(th.Theta, 0, x0, nOmega, nTheta);

    // Joint FD Hessian (verbatim stencil), then Jacobi WITH eigenvectors.
    double s0 = Sx(x0);
    double h = HessStep;
    var sPlus = new double[nJoint];
    for (int i = 0; i < nJoint; i++)
    {
        var xp = (double[])x0.Clone(); xp[i] += h; sPlus[i] = Sx(xp);
    }
    var hess = new double[nJoint, nJoint];
    var xij = new double[nJoint];
    for (int i = 0; i < nJoint; i++)
    {
        var xm = (double[])x0.Clone(); xm[i] -= h;
        hess[i, i] = (sPlus[i] - 2.0 * s0 + Sx(xm)) / (h * h);
        for (int j = i + 1; j < nJoint; j++)
        {
            Array.Copy(x0, xij, nJoint); xij[i] += h; xij[j] += h;
            double fij = Sx(xij);
            double v = (fij - sPlus[i] - sPlus[j] + s0) / (h * h);
            hess[i, j] = v; hess[j, i] = v;
        }
    }

    var eig = JacobiEigen(hess, wantVectors: true);

    double maxAbs = 0.0;
    foreach (double e in eig.Values) maxAbs = System.Math.Max(maxAbs, System.Math.Abs(e));
    double zeroTol = System.Math.Max(ZeroModeAbsFloor, ZeroModeRelTol * maxAbs);

    int pos = 0, neg = 0, zero = 0;
    double oneLoop = 0.0;
    foreach (double e in eig.Values)
    {
        if (e > zeroTol) { pos++; oneLoop += 0.5 * System.Math.Log(e); }
        else if (e < -zeroTol) neg++;
        else zero++;
    }

    return new PointData(t, sB, oneLoop, sB + oneLoop, pos, zero, neg, zeroTol,
        th.ResidualNorm, th.AbsoluteGradNorm, th.Converged, x0, Sx, eig.Values, eig.Vectors!);
}

// ---------------------------------------------------------------------------
// Directional derivative stencils (unit directions, step StencilH) --
// verbatim Phase447 D4 machinery (the T3 stencils are not needed at Hartree
// order: odd moments of a mean-zero Gaussian vanish).
// ---------------------------------------------------------------------------

double[] PointAlong(double[] x0, double[] d1, double a, double[]? d2 = null, double b = 0.0)
{
    var x = (double[])x0.Clone();
    for (int i = 0; i < x.Length; i++)
    {
        x[i] += a * d1[i];
        if (d2 != null) x[i] += b * d2[i];
    }
    return x;
}

// d^4/ds^2 dt^2 f(x0 + s u + t v): 9-point (4 corners + 4 axis + center).
double D4MixedPair(Func<double[], double> f, double[] x0, double f0, double[] u, double[] v,
    double fuP, double fuM, double fvP, double fvM, double h)
{
    double cPP = f(PointAlong(x0, u, h, v, h));
    double cPM = f(PointAlong(x0, u, h, v, -h));
    double cMP = f(PointAlong(x0, u, -h, v, h));
    double cMM = f(PointAlong(x0, u, -h, v, -h));
    return (cPP + cPM + cMP + cMM - 2.0 * (fuP + fuM + fvP + fvM) + 4.0 * f0) / (h * h * h * h);
}

// d^4/ds^4 f(x0 + s u): 5-point.
double D4Diag(Func<double[], double> f, double[] x0, double f0, double[] u,
    double fuP, double fuM, double h)
{
    double f2P = f(PointAlong(x0, u, 2.0 * h));
    double f2M = f(PointAlong(x0, u, -2.0 * h));
    return (f2P - 4.0 * fuP + 6.0 * f0 - 4.0 * fuM + f2M) / (h * h * h * h);
}

// ---------------------------------------------------------------------------
// T4 pair table over ALL modes above the zero tolerance (positive AND
// negative -- the Hartree resummation keeps the transverse negative modes the
// retired convention discarded). Sorted softest-first by |lambda| (ties by
// index) so the rotation check's soft span and the battery subsets are
// deterministic slices. Shared axis evaluations (Phase447's caching pattern).
// ---------------------------------------------------------------------------

GaussianCache BuildT4Cache(PointData p, double stencilH, int? onlySoftest)
{
    var modes = new List<(int Index, double Lambda)>();
    for (int i = 0; i < p.EigenValues.Length; i++)
        if (System.Math.Abs(p.EigenValues[i]) > p.ZeroTol)
            modes.Add((i, p.EigenValues[i]));
    modes.Sort((x, y) =>
    {
        int cmp = System.Math.Abs(x.Lambda).CompareTo(System.Math.Abs(y.Lambda));
        return cmp != 0 ? cmp : x.Index.CompareTo(y.Index);
    });
    int excludedZero = p.EigenValues.Length - modes.Count;
    if (onlySoftest is int cap && modes.Count > cap) modes = modes.Take(cap).ToList();

    var f = p.Sx;
    double f0 = f(p.X0);
    int nm = modes.Count;
    var vec = new double[nm][];
    var fP = new double[nm]; var fM = new double[nm];
    for (int a = 0; a < nm; a++)
    {
        vec[a] = Column(p.EigenVectors, modes[a].Index);
        fP[a] = f(PointAlong(p.X0, vec[a], stencilH));
        fM[a] = f(PointAlong(p.X0, vec[a], -stencilH));
    }

    var t4 = new double[nm, nm];
    for (int a = 0; a < nm; a++)
    {
        t4[a, a] = D4Diag(f, p.X0, f0, vec[a], fP[a], fM[a], stencilH);
        for (int b = a + 1; b < nm; b++)
        {
            double v = D4MixedPair(f, p.X0, f0, vec[a], vec[b], fP[a], fM[a], fP[b], fM[b], stencilH);
            t4[a, b] = v; t4[b, a] = v;
        }
    }

    int posN = 0;
    foreach (var m in modes) if (m.Lambda > 0) posN++;
    return new GaussianCache(modes.Select(m => m.Lambda).ToArray(), vec, fP, fM, f0, t4,
        excludedZero, posN, nm - posN);
}

// ---------------------------------------------------------------------------
// Hartree gap-equation solver (fail-closed). Damped fixed-point iteration
// sigma <- (1 - alpha) sigma + alpha / m(sigma), m_a = lambda_a
// + (1/2) sum_b T4[a,b] sigma_b, from sigma_a = 1/|lambda_a| (clipped),
// with adaptive halving on divergence. NO floor-clamping: if any m_a <= 0 the
// step is undefined and the point records exists = false honestly.
// ---------------------------------------------------------------------------

GapSolveOutcome SolveGap(double[] lam, double[,] t4, double sB, double convTol)
{
    int nm = lam.Length;

    double[] MOf(double[] s)
    {
        var m = new double[nm];
        for (int a = 0; a < nm; a++)
        {
            double acc = 0.0;
            for (int b = 0; b < nm; b++) acc += t4[a, b] * s[b];
            m[a] = lam[a] + 0.5 * acc;
        }
        return m;
    }

    double Resid(double[] s, double[] m)
    {
        double r = 0.0;
        for (int a = 0; a < nm; a++) r = System.Math.Max(r, System.Math.Abs(s[a] * m[a] - 1.0));
        return r;
    }

    double VOf(double[] s)
    {
        double v = sB;
        for (int a = 0; a < nm; a++) v += 0.5 * lam[a] * s[a] - 0.5 * System.Math.Log(s[a]);
        double q = 0.0;
        for (int a = 0; a < nm; a++)
        {
            q += t4[a, a] * s[a] * s[a];
            for (int b = a + 1; b < nm; b++) q += 2.0 * t4[a, b] * s[a] * s[b];
        }
        return v + q / 8.0;
    }

    var sigma = new double[nm];
    for (int a = 0; a < nm; a++)
        sigma[a] = System.Math.Clamp(1.0 / System.Math.Abs(lam[a]), SigmaInitClipLo, SigmaInitClipHi);

    var m0 = MOf(sigma);
    double resid = Resid(sigma, m0);
    double vInit = VOf(sigma);
    double vPrev = vInit;
    bool monotone = true;
    double alpha = GapDampingInit;
    var mCur = m0;
    int it = 0;
    bool converged = resid <= convTol;
    string reason = converged ? "converged-at-start" : "max-iterations";

    while (!converged && it < GapMaxIter)
    {
        it++;
        double minM = mCur.Min();
        if (minM <= 0.0) { reason = "hartree-mass-nonpositive-during-iteration"; break; }

        double aTry = alpha;
        bool stepped = false;
        while (aTry >= GapAlphaFloor && !stepped)
        {
            var cand = new double[nm];
            bool valid = true;
            for (int a = 0; a < nm; a++)
            {
                cand[a] = (1.0 - aTry) * sigma[a] + aTry / mCur[a];
                if (!(cand[a] > 0.0) || !double.IsFinite(cand[a])) { valid = false; break; }
            }
            if (valid)
            {
                var mC = MOf(cand);
                double rC = Resid(cand, mC);
                if (double.IsFinite(rC) && rC < resid)
                {
                    sigma = cand; mCur = mC; resid = rC; stepped = true;
                }
                else aTry *= 0.5;
            }
            else aTry *= 0.5;
        }
        if (!stepped) { reason = "damping-floor-stalled"; break; }
        alpha = System.Math.Min(GapDampingInit, aTry * 2.0);

        double vNow = VOf(sigma);
        if (vNow > vPrev + 1e-12 * System.Math.Max(1.0, System.Math.Abs(vPrev))) monotone = false;
        vPrev = vNow;

        if (resid <= convTol) { converged = true; reason = "converged"; }
    }

    var mFinal = MOf(sigma);
    double minMFinal = nm > 0 ? mFinal.Min() : double.PositiveInfinity;
    bool exists = converged && minMFinal > 0.0;
    double vFinal = VOf(sigma);
    bool descended = vFinal <= vInit + 1e-9 * System.Math.Max(1.0, System.Math.Abs(vInit));
    double sMin = nm > 0 ? sigma.Min() : double.NaN;
    double sMax = nm > 0 ? sigma.Max() : double.NaN;
    return new GapSolveOutcome(exists, converged, it, resid, minMFinal, alpha, sigma,
        vInit, vFinal, monotone, descended, sMin, sMax, reason);
}

// Battery scalar over a (subset) cache: the figure-eight-like contraction
// (1/8) sum_{a,b} T4[a,b] / (|l_a| |l_b|) -- the same class of assembled
// scalar Phase447's Richardson/offset tolerances were calibrated on.
double BatteryScalar(GaussianCache c)
{
    int nm = c.Lambdas.Length;
    double q = 0.0;
    for (int a = 0; a < nm; a++)
    {
        double la = System.Math.Abs(c.Lambdas[a]);
        q += c.T4[a, a] / (la * la);
        for (int b = a + 1; b < nm; b++)
            q += 2.0 * c.T4[a, b] / (la * System.Math.Abs(c.Lambdas[b]));
    }
    return q / 8.0;
}

// ---------------------------------------------------------------------------
// Rays: composite points + T4 cache + gap solve per point.
// ---------------------------------------------------------------------------

var tGrid = new double[GridN];
for (int i = 0; i < GridN; i++) tGrid[i] = TMax * (i + 1) / GridN;
int anchorIdx = GridN / 2; // anchor point for the h/offset/rotation/plumbing checks

double linThetaFdMaxResidual = 0.0;
double maxThetaResidualNonIdentity = 0.0;
bool allThetaConverged = true;
bool identityThetaExactZero = true;
bool allThetaPointsPassGate = true;
int thetaPointsPassedByAbsFloor = 0;

var memberResults = new List<MemberResult>();
foreach (var (spec, mi) in members.Select((s, i) => (s, i)))
{
    var eval = spec.IsControl ? ControlEvalOf(spec.Op) : PinnedEval(spec.Op);
    EinsteinianShiabOperator? op = spec.IsControl ? null : spec.Op;

    var seedRecs = new List<SeedResult>();
    for (int seed = 0; seed < RaySeedCount; seed++)
    {
        var uOmega = UnitRandom(new Random(RngSeed + mi * 1009 + seed * 31), nOmega);
        var pts = new List<PointData>();
        var evalPointSbEvals = new List<long>();
        double[]? thetaCarry = null;
        foreach (double t in tGrid)
        {
            long before = sbEvalCount;
            var p = EvalPoint(eval, op, uOmega, t, spec.IsControl, thetaCarry);
            evalPointSbEvals.Add(sbEvalCount - before);
            if (!spec.IsControl) thetaCarry = p.X0.Skip(nOmega).ToArray();
            pts.Add(p);
            if (spec.IsControl && p.ThetaResidual != 0.0) identityThetaExactZero = false;
        }
        if (!spec.IsControl && pts.Count >= 2 && !pts[0].ThetaConverged)
        {
            long before = sbEvalCount;
            pts[0] = EvalPoint(eval, op, uOmega, pts[0].T, spec.IsControl, pts[1].X0.Skip(nOmega).ToArray());
            evalPointSbEvals[0] += sbEvalCount - before;
        }

        if (!spec.IsControl)
            foreach (var pf in pts)
            {
                maxThetaResidualNonIdentity = System.Math.Max(maxThetaResidualNonIdentity, pf.ThetaResidual);
                allThetaConverged &= pf.ThetaConverged;
                bool relOk = pf.ThetaResidual <= ThetaResidualBatteryRel;
                bool absOk = pf.ThetaAbsGradNorm <= ThetaAbsGradFloor;
                if (!relOk && absOk) thetaPointsPassedByAbsFloor++;
                allThetaPointsPassGate &= relOk || absOk;
            }

        if (!spec.IsControl && seed == 0)
        {
            int mid = pts.Count / 2;
            var omegaSample = Scale(uOmega, tGrid[mid]);
            var dth = UnitRandom(new Random(RngSeed + 777 + mi), nTheta);
            double resid = EinsteinianShiabBatteries.LinearizeThetaFdResidual(
                spec.Op, mesh, algebra, omegaSample, pts[mid].X0.Skip(nOmega).ToArray(), dth, manifest, geometry);
            linThetaFdMaxResidual = System.Math.Max(linThetaFdMaxResidual, resid);
        }

        // T4 pair table + Hartree gap solve per point.
        GaussianCache? anchorCache = null;
        var gaps = new GapSolveOutcome[pts.Count];
        var gaussianSbEvals = new long[pts.Count];
        var modeCounts = new int[pts.Count];
        var negInAnsatz = new int[pts.Count];
        var excludedZeros = new int[pts.Count];
        for (int i = 0; i < pts.Count; i++)
        {
            long before = sbEvalCount;
            var c = BuildT4Cache(pts[i], StencilH, onlySoftest: null);
            gaps[i] = SolveGap(c.Lambdas, c.T4, pts[i].SB, GapConvTol);
            gaussianSbEvals[i] = sbEvalCount - before;
            modeCounts[i] = c.Lambdas.Length;
            negInAnsatz[i] = c.NegCount;
            excludedZeros[i] = c.ExcludedZeroModes;
            if (i == anchorIdx) anchorCache = c;
        }

        // Richardson battery at the anchor: fresh softest-subset caches at h and h/2.
        var richA = BuildT4Cache(pts[anchorIdx], StencilH, BatterySubsetModes);
        var richB = BuildT4Cache(pts[anchorIdx], StencilH * RichardsonHFactor, BatterySubsetModes);
        double qA = BatteryScalar(richA);
        double richRel = RelDiff(qA, BatteryScalar(richB));
        double richEntrywise = EntrywiseRel(richA.T4, richB.T4);

        // Offset-immunity battery at the anchor (constant added to Sx must leave T4 unchanged).
        var pShift = pts[anchorIdx] with { Sx = x => pts[anchorIdx].Sx(x) + 1000.0 };
        var offC = BuildT4Cache(pShift, StencilH, BatterySubsetModes);
        double offsetRel = RelDiff(BatteryScalar(offC), qA);
        double offsetEntrywise = EntrywiseRel(offC.T4, richA.T4);

        seedRecs.Add(new SeedResult(seed, pts.ToArray(), gaps, anchorCache!,
            modeCounts, negInAnsatz, excludedZeros,
            evalPointSbEvals.ToArray(), gaussianSbEvals,
            richRel, richEntrywise, offsetRel, offsetEntrywise));
    }
    memberResults.Add(new MemberResult(mi, spec.Name, spec.IsControl,
        spec.Member.Phi1.InvariantElement, spec.Member.Phi2.InvariantElement, spec.Member.EinsteinCoefficient, seedRecs));
}

var identityM = memberResults.First(m => m.IsControl);
var einsteinianM = memberResults.Where(m => !m.IsControl).ToList();

// ---------------------------------------------------------------------------
// Solver-plumbing exactness control (T4 := 0). On the identity anchor's
// POSITIVE modes the gap equation must reproduce sigma_a lambda_a = 1 to
// 1e-10; on a mode set containing NEGATIVE eigenvalues (Einsteinian anchor)
// it must FAIL (m_a = lambda_a < 0) -- validating the fail-closed path.
// ---------------------------------------------------------------------------

var idAnchorCache = identityM.Seeds[0].AnchorCache;
var idPosLam = idAnchorCache.Lambdas.Where(l => l > 0.0).ToArray();
var gPlumbPos = SolveGap(idPosLam, new double[idPosLam.Length, idPosLam.Length], 0.0, PlumbingSolveTol);
double plumbingMaxDeviation = 0.0;
for (int a = 0; a < idPosLam.Length; a++)
    plumbingMaxDeviation = System.Math.Max(plumbingMaxDeviation,
        System.Math.Abs(gPlumbPos.Sigma[a] * idPosLam[a] - 1.0));
bool gapPlumbingPositiveBattery = gPlumbPos.Exists && plumbingMaxDeviation <= PlumbingTol;

var einAnchorCache = einsteinianM[0].Seeds[0].AnchorCache;
bool einAnchorHasNegativeModes = einAnchorCache.Lambdas.Any(l => l < 0.0);
var gPlumbNeg = SolveGap(einAnchorCache.Lambdas,
    new double[einAnchorCache.Lambdas.Length, einAnchorCache.Lambdas.Length],
    einsteinianM[0].Seeds[0].Points[anchorIdx].SB, PlumbingSolveTol);
bool gapPlumbingNegativeExpectedFailureBattery = !einAnchorHasNegativeModes || !gPlumbNeg.Exists;

// ---------------------------------------------------------------------------
// Ansatz-rotation honesty check at ONE anchor point per Einsteinian member:
// the diagonal ansatz re-solved in a seeded pairwise-rotated basis within the
// span of the RotationSoftModes softest modes. Both converged V_G values are
// RECORDED (basis dependence measured, not gated).
// ---------------------------------------------------------------------------

RotationCheck DoRotationCheck(int memberIndex, string memberName, PointData p, GaussianCache c, GapSolveOutcome gapEig)
{
    int nm = c.Lambdas.Length;
    int nsRot = System.Math.Min(RotationSoftModes, nm);
    nsRot -= nsRot % 2;
    var rng = new Random(RngSeed + 555 + memberIndex);
    var vecR = new double[nm][];
    var lamR = (double[])c.Lambdas.Clone();
    for (int a = nsRot; a < nm; a++) vecR[a] = c.Vec[a];
    for (int k = 0; k + 1 < nsRot; k += 2)
    {
        double ang = 2.0 * System.Math.PI * rng.NextDouble();
        double cc = System.Math.Cos(ang), sn = System.Math.Sin(ang);
        int i = k, j = k + 1;
        int dim = c.Vec[i].Length;
        var w1 = new double[dim];
        var w2 = new double[dim];
        for (int d = 0; d < dim; d++)
        {
            w1[d] = cc * c.Vec[i][d] + sn * c.Vec[j][d];
            w2[d] = -sn * c.Vec[i][d] + cc * c.Vec[j][d];
        }
        vecR[i] = w1; vecR[j] = w2;
        // Exact quadratic form of the rotated unit vectors in the eigenbasis.
        lamR[i] = cc * cc * c.Lambdas[i] + sn * sn * c.Lambdas[j];
        lamR[j] = sn * sn * c.Lambdas[i] + cc * cc * c.Lambdas[j];
    }

    var f = p.Sx;
    double f0 = c.F0;
    double h = StencilH;
    var fPr = (double[])c.FP.Clone();
    var fMr = (double[])c.FM.Clone();
    for (int a = 0; a < nsRot; a++)
    {
        fPr[a] = f(PointAlong(p.X0, vecR[a], h));
        fMr[a] = f(PointAlong(p.X0, vecR[a], -h));
    }
    var t4R = (double[,])c.T4.Clone();
    for (int a = 0; a < nsRot; a++)
    {
        t4R[a, a] = D4Diag(f, p.X0, f0, vecR[a], fPr[a], fMr[a], h);
        for (int b = a + 1; b < nm; b++)
        {
            double v = D4MixedPair(f, p.X0, f0, vecR[a], vecR[b], fPr[a], fMr[a], fPr[b], fMr[b], h);
            t4R[a, b] = v; t4R[b, a] = v;
        }
    }

    var gapRot = SolveGap(lamR, t4R, p.SB, GapConvTol);
    double vgEig = gapEig.VFinal, vgRot = gapRot.VFinal;
    return new RotationCheck(memberName, p.T, nsRot, gapEig.Exists, gapRot.Exists,
        vgEig, vgRot, System.Math.Abs(vgRot - vgEig), RelDiff(vgRot, vgEig));
}

var rotationChecks = new List<RotationCheck>();
foreach (var m in einsteinianM)
{
    var s0 = m.Seeds[0];
    rotationChecks.Add(DoRotationCheck(m.Index, m.Name, s0.Points[anchorIdx], s0.AnchorCache, s0.Gaps[anchorIdx]));
}

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// Classification + batteries + verdicts.
// ---------------------------------------------------------------------------

(string Cls, double TStar, double VStar) ClassifyCurve(double[] v, double[] ts)
{
    if (v.Length < 3) return ("insufficient-points", double.NaN, double.NaN);
    int am = 0;
    for (int j = 1; j < v.Length; j++) if (v[j] < v[am]) am = j;
    if (am == 0) return ("trivial-origin", double.NaN, double.NaN);
    if (am == v.Length - 1) return ("runaway", double.NaN, double.NaN);
    if (v[am - 1] > v[am] && v[am + 1] > v[am]) return ("interior-finite", ts[am], v[am]);
    return ("interior-inflection-not-verified", double.NaN, double.NaN);
}

string ClassifyGaussianRay(SeedResult s)
{
    if (s.Gaps.Any(g => !g.Exists)) return "gap-equation-breakdown";
    var v = s.Gaps.Select(g => g.VFinal).ToArray();
    if (v.Any(x => !double.IsFinite(x))) return "gap-equation-breakdown";
    return ClassifyCurve(v, tGrid).Cls;
}

var classByMemberSeed = new Dictionary<(int M, int S), string>();
foreach (var m in memberResults)
    foreach (var s in m.Seeds)
        classByMemberSeed[(m.Index, s.Seed)] = ClassifyGaussianRay(s);

bool thetaStationarityGate = allThetaConverged && allThetaPointsPassGate && identityThetaExactZero;
bool linThetaFdBattery = linThetaFdMaxResidual <= LinearizeThetaFdTol;

// Identity exact-quartic battery: stencil h-independence to near-roundoff.
double identityMaxRichardson = identityM.Seeds.Max(s => s.RichardsonRel);
bool identityExactStencilBattery = identityMaxRichardson <= IdentityExactRelTol;

// Einsteinian Richardson battery (transcendental theta; honest FD tolerance).
double einsteinianMaxRichardson = einsteinianM.Max(m => m.Seeds.Max(s => s.RichardsonRel));
bool einsteinianRichardsonBattery = einsteinianMaxRichardson <= RichardsonRelTol;

// Offset immunity (stencils annihilate constants up to FP cancellation):
// gated ENTRYWISE on the T4 table; the soft-mode 1/lambda^2-weighted scalar
// is recorded, not gated (see the tolerance note above).
double maxOffsetEntrywise = memberResults.Max(m => m.Seeds.Max(s => s.OffsetEntrywiseRel));
bool offsetImmunityBattery = maxOffsetEntrywise <= OffsetImmunityRelTol;
double maxOffsetWeightedScalar = memberResults.Max(m => m.Seeds.Max(s => s.OffsetImmunityRel));
double maxRichardsonEntrywise = memberResults.Max(m => m.Seeds.Max(s => s.RichardsonEntrywiseRel));

// Gap-equation batteries.
var allGaps = memberResults.SelectMany(m => m.Seeds).SelectMany(s => s.Gaps).ToArray();
var existingGaps = allGaps.Where(g => g.Exists).ToArray();
bool hartreeSelfConsistentSolutionExistsEverywhere = allGaps.All(g => g.Exists);
int gapBreakdownPointCount = allGaps.Count(g => !g.Exists);
double maxGapResidualAmongExisting = existingGaps.Length > 0 ? existingGaps.Max(g => g.Residual) : double.NaN;
int maxGapIterations = allGaps.Max(g => g.Iterations);
double minFinalDamping = allGaps.Min(g => g.FinalAlpha);
double monotoneDecreaseFraction = existingGaps.Length > 0
    ? existingGaps.Count(g => g.MonotoneDecrease) / (double)existingGaps.Length
    : double.NaN;
bool gapInternalConsistencyBattery = allGaps.All(g =>
    !g.Exists || (g.Converged && g.Residual <= GapConvTol && g.MinHartreeMass > 0.0));
bool descentBattery = existingGaps.All(g => g.Descended);

bool batteriesAllPassed =
    precursorsPassed &&
    thetaStationarityGate &&
    linThetaFdBattery &&
    identityExactStencilBattery &&
    einsteinianRichardsonBattery &&
    offsetImmunityBattery &&
    gapPlumbingPositiveBattery &&
    gapPlumbingNegativeExpectedFailureBattery &&
    gapInternalConsistencyBattery &&
    descentBattery;

// Verdicts (the phase passes on internal consistency regardless).
bool seedStable = memberResults.All(m =>
    m.Seeds.Select(s => classByMemberSeed[(m.Index, s.Seed)]).Distinct().Count() == 1);
bool controlNoSpuriousGaussianMinimum = identityM.Seeds
    .All(s => classByMemberSeed[(identityM.Index, s.Seed)] != "interior-finite");
bool anyEinsteinianInteriorFiniteAnySeed = einsteinianM.Any(m =>
    m.Seeds.Any(s => classByMemberSeed[(m.Index, s.Seed)] == "interior-finite"));
// Headline: any Einsteinian member interior-finite on ALL its seeds.
bool einsteinianGaussianSaturationObserved = einsteinianM.Any(m =>
    m.Seeds.All(s => classByMemberSeed[(m.Index, s.Seed)] == "interior-finite"));

string verdictKind =
    !hartreeSelfConsistentSolutionExistsEverywhere ? "gap-equation-breakdown"
    : einsteinianGaussianSaturationObserved && controlNoSpuriousGaussianMinimum ? "gaussian-mean-field-candidate"
    : anyEinsteinianInteriorFiniteAnySeed || !controlNoSpuriousGaussianMinimum ? "gaussian-unstable-or-control-contaminated-recorded"
    : "gaussian-bound-null";

string verdictPhrase = verdictKind switch
{
    "gap-equation-breakdown" =>
        "the Hartree self-consistency FAILS at one or more composite points (some m_a <= 0 or no convergence in the iteration budget) -- recorded honestly, fail-closed, with per-point failure reasons; the diagonal-Gaussian instrument does not cover those points and no saturation statement is made there",
    "gaussian-mean-field-candidate" =>
        "the converged variational-Gaussian upper bound V_G(t) develops a seed-stable strict interior minimum for at least one Einsteinian member with a clean identity control -- a MEAN-FIELD-ONLY structure at ansatz level; NOT a candidate promotion: the phase450 constraint-EP HMC is REQUIRED for confirmation (mean-field minima can be artifacts of the diagonal restriction and the theta quartic truncation)",
    "gaussian-unstable-or-control-contaminated-recorded" =>
        "an interior minimum appears but is not seed-stable across members, or the identity control itself classifies interior-finite -- recorded as exactly that (the Phase445/446 discipline), not a candidate and not a null",
    _ =>
        "no member develops an interior structure in the converged variational-Gaussian upper bound along the probed rays: the rigorous-bound family (diagonal ansatz, all modes retained, negative modes resummed through the Hartree mass) shows no symmetry breaking -- per the review board, STRONG evidence against a dynamical scale at ansatz level (not a theorem; the phase450 constraint-EP HMC remains the confirmatory instrument)",
};

// ---------------------------------------------------------------------------
// Framing + fail-closed block (verbatim keys).
// ---------------------------------------------------------------------------

const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool gaussianAnsatzIsWorkbenchConvention = true;
const bool physicistReviewPending = true;
const string epsilonRealization = "independent-theta-dof-variational";
const string epsilonTaxonomyMode = "mode-3-variational";

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
    $"su2 trace-pairing; CreateUniform4D({MeshRefinement}); V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} C={mesh.CellCount}; nOmega={nOmega} nTheta={nTheta}",
    "variational Gaussian (CJT-Hartree) effective potential: diagonal ansatz in the joint-Hessian eigenbasis over ALL modes above zero tolerance (positive AND negative); T4 pair table via D4 directional FD stencils on unit eigenvectors; damped fixed-point Hartree gap equation, fail-closed (no floor clamping); Feynman-Jensen upper-bound framing with recorded theta quartic-truncation caveat; beta=1 recorded; raw-curve strict-minimum classification, no fits; ansatz-rotation honesty check recorded; no target values")))).ToLowerInvariant();

bool variationalGaussianEffectivePotentialProbePassed =
    precursorsPassed &&
    batteriesAllPassed &&
    scaleIsWorkbenchRelativeCandidateOnly &&
    noGevPromotion &&
    gaussianAnsatzIsWorkbenchConvention &&
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

string terminalStatus = variationalGaussianEffectivePotentialProbePassed
    ? verdictKind switch
    {
        "gap-equation-breakdown" => TerminalPrefix + "passed-gap-equation-breakdown-recorded",
        "gaussian-mean-field-candidate" => TerminalPrefix + "passed-gaussian-mean-field-candidate-workbench-relative-phase450-cep-required-no-gev",
        "gaussian-unstable-or-control-contaminated-recorded" => TerminalPrefix + "passed-unstable-or-control-contaminated-recorded",
        _ => TerminalPrefix + "passed-gaussian-bound-null-frontier-sharpened",
    }
    : TerminalPrefix + "blocked";

string decision = variationalGaussianEffectivePotentialProbePassed
    ? $"The variational Gaussian (CJT-Hartree) effective potential probe is decided on internal consistency. On CreateUniform4D({MeshRefinement}) (su(2) trace pairing; nOmega={nOmega}, nTheta={nTheta}, joint DOF {nJoint}) the composite points (omega = t*u, theta*(t*u)) were recomputed with the verbatim hardened machinery (theta point gate: relative <= {ThetaResidualBatteryRel:E0} or absolute floor {ThetaAbsGradFloor:E0}, {thetaPointsPassedByAbsFloor} via floor, max relative {maxThetaResidualNonIdentity:E2}; identity theta*=0 exact; LinearizeTheta FD {linThetaFdMaxResidual:E2}); at each point the T4 pair table over ALL modes above the zero tolerance (positive AND negative) was built by directional FD stencils (h={StencilH:E0}) on the joint Hessian eigenvectors and the diagonal-Gaussian Hartree gap equation 1/sigma_a = lambda_a + (1/2) sum_b T4[a,b] sigma_b was solved by damped fixed-point iteration (fail-closed: no floor clamping; max residual among existing points {maxGapResidualAmongExisting:E2} <= {GapConvTol:E0}). " +
      $"BATTERIES: identity exact-quartic stencil h-independence {identityMaxRichardson:E2} <= {IdentityExactRelTol:E0}; Einsteinian Richardson {einsteinianMaxRichardson:E2} <= {RichardsonRelTol:E0}; offset immunity (entrywise on T4) {maxOffsetEntrywise:E2} <= {OffsetImmunityRelTol:E0} (soft-mode-weighted scalar {maxOffsetWeightedScalar:E2} recorded, not gated); gap plumbing (T4:=0) max |sigma lambda - 1| = {plumbingMaxDeviation:E2} <= {PlumbingTol:E0} with expected failure on the negative-mode set = {gapPlumbingNegativeExpectedFailureBattery}; descent battery {descentBattery} (monotone fraction {monotoneDecreaseFraction:F3}); ansatz-rotation basis dependence recorded (not gated). " +
      $"HARTREE EXISTENCE: self-consistent solution everywhere = {hartreeSelfConsistentSolutionExistsEverywhere}. VERDICT ({verdictKind}): {verdictPhrase}. " +
      $"MANDATORY FRAMING: any structure is a WORKBENCH-RELATIVE CANDIDATE ONLY (su(2) toy algebra, reduced Spin(4) slice, lattice units); the diagonal Gaussian ansatz, beta = 1, and the theta quartic truncation are WORKBENCH CONVENTIONS pending physicist review; NO GeV/pole/VEV promotion. Everything is target-blind, reduced-spin4-slice; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the verdicts until the precursor, theta-stationarity, stencil, gap-equation, and plumbing batteries pass.";

// ---------------------------------------------------------------------------
// Serialize (fail-closed: FiniteOrNull on every possibly-non-finite value).
// ---------------------------------------------------------------------------

object MemberJson(MemberResult m) => new
{
    index = m.Index,
    member = m.Name,
    phi1 = m.Phi1,
    phi2 = m.Phi2,
    einsteinCoefficient = m.EinsteinCoefficient,
    isControl = m.IsControl,
    gaussianClassification = classByMemberSeed[(m.Index, 0)],
    seeds = m.Seeds.Select(s => new
    {
        seed = s.Seed,
        gaussianClassification = classByMemberSeed[(m.Index, s.Seed)],
        modeCountStableAlongRay = s.ModeCounts.Distinct().Count() == 1,
        richardsonRel = FiniteOrNull(s.RichardsonRel),
        richardsonEntrywiseRel = FiniteOrNull(s.RichardsonEntrywiseRel),
        offsetEntrywiseRel = FiniteOrNull(s.OffsetEntrywiseRel),
        offsetWeightedScalarRel = FiniteOrNull(s.OffsetImmunityRel),
        points = s.Points.Select((p, i) => new
        {
            t = p.T,
            sB = p.SB,
            retiredOneLoopDiagnostic = FiniteOrNull(p.OneLoop),
            positiveModes = p.PosCount,
            zeroModes = p.ZeroCount,
            negativeModes = p.NegCount,
            modeCountInAnsatz = s.ModeCounts[i],
            negativeModesInAnsatz = s.NegModesInAnsatz[i],
            excludedZeroModes = s.ExcludedZeroModes[i],
            hartreeSelfConsistentSolutionExists = s.Gaps[i].Exists,
            gapConverged = s.Gaps[i].Converged,
            gapIterations = s.Gaps[i].Iterations,
            gapResidual = FiniteOrNull(s.Gaps[i].Residual),
            minHartreeMass = FiniteOrNull(s.Gaps[i].MinHartreeMass),
            finalDamping = FiniteOrNull(s.Gaps[i].FinalAlpha),
            gapFailureReason = s.Gaps[i].Reason,
            vGInitial = FiniteOrNull(s.Gaps[i].VInitial),
            vG = s.Gaps[i].Exists ? FiniteOrNull(s.Gaps[i].VFinal) : null,
            vGMinusTree = s.Gaps[i].Exists ? FiniteOrNull(s.Gaps[i].VFinal - p.SB) : null,
            gaussianEntropyDroppedConstant = FiniteOrNull(0.5 * s.ModeCounts[i] * (1.0 + System.Math.Log(2.0 * System.Math.PI))),
            functionalDecreaseMonotone = s.Gaps[i].MonotoneDecrease,
            descended = s.Gaps[i].Descended,
            sigmaMin = FiniteOrNull(s.Gaps[i].SigmaMin),
            sigmaMax = FiniteOrNull(s.Gaps[i].SigmaMax),
            thetaResidual = p.ThetaResidual,
            thetaAbsoluteGradNorm = p.ThetaAbsGradNorm,
            thetaConverged = p.ThetaConverged,
            evalPointSbEvals = s.EvalPointSbEvals[i],
            gaussianStageSbEvals = s.GaussianSbEvals[i],
        }).ToArray(),
    }).ToArray(),
};

var result = new
{
    phaseId = "phase449-variational-gaussian-effective-potential-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    variationalGaussianEffectivePotentialProbePassed,

    phase447PrecursorPassed,
    phase448PrecursorPassed,
    precursorsPassed,

    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,

    probeConfiguration = new
    {
        meshRefinement = MeshRefinement,
        lieAlgebraId = "su2-trace-pairing",
        vertexCount = mesh.VertexCount,
        edgeCount = mesh.EdgeCount,
        faceCount = mesh.FaceCount,
        cellCount = mesh.CellCount,
        nOmega,
        nTheta,
        jointDof = nJoint,
        latticeScale,
        raySeedCount = RaySeedCount,
        tMax = TMax,
        gridN = GridN,
        tGrid,
        anchorT = tGrid[anchorIdx],
        stencilH = StencilH,
        stencilHNote = "LARGE by design (the Phase447 lesson): S_B is an exact quartic in omega so the D4 stencils are h-independent on the omega sector; a large h lifts the numerators above the double-precision cancellation floor (Phase447 proved h=5e-3 noise-dominated via the offset-immunity battery); theta truncation controlled by the h-vs-h/2 Richardson battery",
        richardsonHFactor = RichardsonHFactor,
        richardsonRelTol = RichardsonRelTol,
        identityExactRelTol = IdentityExactRelTol,
        offsetImmunityRelTol = OffsetImmunityRelTol,
        offsetImmunityGating = "ENTRYWISE on the battery T4 table (max |T4_shift - T4| / max |T4|) -- the direct reading of 'S_B + 1000 leaves T4 unchanged'; the 1/|lambda|^2-weighted battery scalar over the softest modes amplifies benign absolute FP noise on near-zero entries and is recorded, not gated (the Richardson gates stay on the weighted scalar, where the probed error is the physical theta h-truncation of the load-bearing contraction)",
        batterySubsetModes = BatterySubsetModes,
        hessStep = HessStep,
        zeroModeRelTol = ZeroModeRelTol,
        zeroModeAbsFloor = ZeroModeAbsFloor,
        thetaTol = ThetaTolRel,
        thetaResidualBattery = ThetaResidualBatteryRel,
        thetaAbsGradFloor = ThetaAbsGradFloor,
        epsilonRealization,
        epsilonTaxonomyMode,
        sbEvalCount,
        betaConvention = BetaConvention,
        gapMaxIter = GapMaxIter,
        gapDampingInit = GapDampingInit,
        gapConvTol = GapConvTol,
        gapAlphaFloor = GapAlphaFloor,
        sigmaInitClip = new[] { SigmaInitClipLo, SigmaInitClipHi },
        rotationSoftModes = RotationSoftModes,
        gaussianFunctional = "V_G(t; sigma) = S_B + (1/2) sum_a lambda_a sigma_a + (1/8) sum_{a,b} T4[a,b] sigma_a sigma_b - (1/2) sum_a log sigma_a; diagonal ansatz Sigma = sum_a sigma_a v_a v_a^T in the joint-Hessian eigenbasis over ALL modes above the zero tolerance (positive AND negative); the additive constant (n_modes/2)(1+log 2pi) is dropped and recorded per point (mode-count changes along a ray are recorded via modeCountStableAlongRay)",
        gapEquation = "1/sigma_a = m_a := lambda_a + (1/2) sum_b T4[a,b] sigma_b; damped fixed-point sigma <- (1-alpha) sigma + alpha/m with alpha0=0.3 and adaptive halving on divergence, from sigma_a = 1/|lambda_a| clipped to [1e-6, 1e3]; converged when max_a |sigma_a m_a - 1| <= 1e-8; FAIL-CLOSED: any m_a <= 0 or non-convergence within 500 iterations records hartreeSelfConsistentSolutionExists=false (no floor clamping into fake convergence)",
        upperBoundFraming = "Feynman-Jensen: V_G is a rigorous UPPER BOUND on the constrained free energy for ANY sigma > 0 (the diagonal restriction only loosens the minimization). CAVEAT (recorded): the bound applies to the Gaussian expectation of the EXACT action; S_B is exactly quartic in omega (bound exact on the omega sector) but transcendental in theta, so the quartic Taylor truncation in theta is a recorded workbench convention",
        z2ReflectionNote = "a3 = 0 exactly on invariant rays by the omega -> -omega Z2 (review-board exact fact), so V_G(t) and V_G(-t) would be degenerate there; these are random rays on the open mesh, so only t > 0 is probed and the +-t degeneracy is noted, not tested",
        rotationCheckNote = "ansatz-rotation honesty check at one anchor point per Einsteinian member: the softest RotationSoftModes modes are mixed pairwise by seeded rotations, the T4 entries touching rotated vectors recomputed by fresh stencils, the gap equation re-solved; both converged V_G values RECORDED (basis dependence measured, not gated)",
        plumbingNote = "solver-plumbing exactness control: with T4 := 0 the gap equation must give sigma_a = 1/lambda_a on positive modes (verified to 1e-10 on the identity anchor) and must FAIL on a set containing negative modes (verified on the Einsteinian anchor)",
        structureResult = "S_B is an EXACT QUARTIC in omega at fixed theta (identity stencils exact to roundoff) and TRANSCENDENTAL in theta via Ad = exp(ad_theta) -- the Phase442/447 structure result; the Einsteinian T4 requires honest FD stencils and the identity control anchors them",
    },

    // Headline verdicts.
    verdictKind,
    einsteinianGaussianSaturationObserved,
    hartreeSelfConsistentSolutionExistsEverywhere,
    gapBreakdownPointCount,
    seedStable,
    anyEinsteinianInteriorFiniteAnySeed,
    controlNoSpuriousGaussianMinimum,
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    gaussianAnsatzIsWorkbenchConvention,
    physicistReviewPending,

    batteries = new
    {
        batteriesAllPassed,
        thetaStationarityGate,
        maxThetaResidualNonIdentity,
        allThetaConverged,
        allThetaPointsPassGate,
        thetaPointsPassedByAbsFloor,
        thetaAbsGradFloor = ThetaAbsGradFloor,
        identityThetaExactZero,
        linThetaFdBattery,
        linThetaFdMaxResidual,
        identityExactStencilBattery,
        identityMaxRichardson = FiniteOrNull(identityMaxRichardson),
        einsteinianRichardsonBattery,
        einsteinianMaxRichardson = FiniteOrNull(einsteinianMaxRichardson),
        offsetImmunityBattery,
        maxOffsetEntrywise = FiniteOrNull(maxOffsetEntrywise),
        maxOffsetWeightedScalarRecorded = FiniteOrNull(maxOffsetWeightedScalar),
        maxRichardsonEntrywiseRecorded = FiniteOrNull(maxRichardsonEntrywise),
        gapPlumbingPositiveBattery,
        gapPlumbingMaxDeviation = FiniteOrNull(plumbingMaxDeviation),
        gapPlumbingNegativeExpectedFailureBattery,
        einAnchorHasNegativeModes,
        gapInternalConsistencyBattery,
        maxGapResidualAmongExisting = FiniteOrNull(maxGapResidualAmongExisting),
        maxGapIterations,
        minFinalDamping = FiniteOrNull(minFinalDamping),
        descentBattery,
        monotoneDecreaseFraction = FiniteOrNull(monotoneDecreaseFraction),
    },

    ansatzRotationChecks = rotationChecks.Select(r => new
    {
        member = r.Member,
        t = r.T,
        rotatedModes = r.RotatedModes,
        eigenbasisSolutionExists = r.EigExists,
        rotatedBasisSolutionExists = r.RotExists,
        vGEigenbasis = FiniteOrNull(r.VGEig),
        vGRotatedBasis = FiniteOrNull(r.VGRot),
        absDiff = FiniteOrNull(r.AbsDiff),
        relDiff = FiniteOrNull(r.RelDiffValue),
    }).ToArray(),

    memberTable = memberResults.Select(MemberJson).ToArray(),

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
        gaussianAnsatzIsWorkbenchConvention,
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
        phase447SummaryPath = Phase447SummaryPath,
        phase448SummaryPath = Phase448SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
        reviewBoardSourcePath = ReviewBoardSourcePath,
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "The diagonal-Gaussian ansatz, the beta = 1 convention, the theta quartic-truncation, and the sigma-solver conventions are WORKBENCH CONVENTIONS pending physicist review; V_G, sigma_a, m_a, and any interior minima are candidate-only structure data of the reduced spin-4 slice, NOT physical masses, VEVs, or a scale in GeV.",
        "A NULL here is evidence against a dynamical scale at ANSATZ level along the probed rays, not a theorem: the diagonal restriction, the random open-mesh rays, and the theta truncation all limit the family minimized over.",
        "A POSITIVE here is MEAN-FIELD-ONLY and is NOT a candidate promotion: the phase450 constraint-EP HMC (gauge-invariant collective coordinate, theta-Haar measure, ergodicity control) is required for confirmation.",
        "This null-is-strong-evidence / positive-is-mean-field-only-pending-CEP framing follows the 2026-07-04 review board outcome and is reinforced by the 2026-07-05 task-force referee ruling (PARALLEL WORK PLAN section of docs/BOSON_PREDICTION_AGENT_RESTART_PROMPT.md; that section postdates this worktree snapshot and is cited per the coordinating lead).",
        "The T4 pair table is built from the su(2)-toy joint action's fourth derivative tensor; it is not a physical electroweak vertex.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "variational_gaussian_effective_potential_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "variational_gaussian_effective_potential_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"variationalGaussianEffectivePotentialProbePassed={variationalGaussianEffectivePotentialProbePassed}");
Console.WriteLine($"precursors: p447={phase447PrecursorPassed} p448={phase448PrecursorPassed}");
Console.WriteLine($"mesh: V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} C={mesh.CellCount}; nOmega={nOmega} nTheta={nTheta} jointDof={nJoint}");
Console.WriteLine($"THETA: gate={thetaStationarityGate} maxResid(non-id)={maxThetaResidualNonIdentity:E2} viaAbsFloor={thetaPointsPassedByAbsFloor} linThetaFd={linThetaFdMaxResidual:E2}");
Console.WriteLine($"STENCILS: idExact={identityExactStencilBattery} ({identityMaxRichardson:E2}) einRich={einsteinianRichardsonBattery} ({einsteinianMaxRichardson:E2}) offsetImm={offsetImmunityBattery} (entrywise {maxOffsetEntrywise:E2}; weighted {maxOffsetWeightedScalar:E2} recorded)");
Console.WriteLine($"GAP: existsEverywhere={hartreeSelfConsistentSolutionExistsEverywhere} breakdownPoints={gapBreakdownPointCount} maxResid(existing)={maxGapResidualAmongExisting:E2} maxIter={maxGapIterations} minAlpha={minFinalDamping:E2} internalConsistency={gapInternalConsistencyBattery}");
Console.WriteLine($"PLUMBING: positive={gapPlumbingPositiveBattery} (maxDev={plumbingMaxDeviation:E2}) negativeExpectedFailure={gapPlumbingNegativeExpectedFailureBattery} (einAnchorHasNeg={einAnchorHasNegativeModes})");
Console.WriteLine($"BOUND-DIRECTION: descent={descentBattery} monotoneFraction={monotoneDecreaseFraction:F3}");
foreach (var r in rotationChecks)
    Console.WriteLine($"  rotation {r.Member,-16} t={r.T:F3} vG(eig)={r.VGEig:E6} vG(rot)={r.VGRot:E6} absDiff={r.AbsDiff:E2} (exists eig={r.EigExists} rot={r.RotExists})");
Console.WriteLine($"VERDICT: kind={verdictKind} saturationObserved={einsteinianGaussianSaturationObserved} anyInteriorAnySeed={anyEinsteinianInteriorFiniteAnySeed} seedStable={seedStable} controlClean={controlNoSpuriousGaussianMinimum}");
Console.WriteLine("--- member classifications (primary seed) ---");
foreach (var m in memberResults)
    Console.WriteLine($"  {m.Name,-16} gaussian={classByMemberSeed[(m.Index, 0)]}");
Console.WriteLine($"sbEvalCount={sbEvalCount} runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Local helpers.
// ---------------------------------------------------------------------------

double[] Column(double[,] m, int j)
{
    int n = m.GetLength(0);
    var c = new double[n];
    for (int i = 0; i < n; i++) c[i] = m[i, j];
    return c;
}

double Norm(double[] v)
{
    double s = 0; foreach (double x in v) s += x * x; return System.Math.Sqrt(s);
}

static double RelDiff(double a, double b) =>
    System.Math.Abs(a - b) / System.Math.Max(1e-30, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));

static double EntrywiseRel(double[,] a, double[,] b)
{
    int n = a.GetLength(0);
    double maxAbs = 0.0, maxDiff = 0.0;
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            maxAbs = System.Math.Max(maxAbs, System.Math.Abs(a[i, j]));
            maxDiff = System.Math.Max(maxDiff, System.Math.Abs(a[i, j] - b[i, j]));
        }
    return maxDiff / System.Math.Max(1e-30, maxAbs);
}

static double? FiniteOrNull(double x) => double.IsFinite(x) ? x : null;

double MeanEdgeLength()
{
    double total = 0; int n = mesh.EdgeCount;
    for (int e = 0; e < n; e++)
    {
        var edge = mesh.Edges[e];
        var pa = mesh.GetVertexCoordinates(edge[0]);
        var pb = mesh.GetVertexCoordinates(edge[1]);
        double dd = 0;
        for (int k = 0; k < pa.Length; k++) { double diff = pa[k] - pb[k]; dd += diff * diff; }
        total += System.Math.Sqrt(dd);
    }
    return total / n;
}

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

static double[] Scale(double[] a, double s)
{
    var r = new double[a.Length];
    for (int i = 0; i < a.Length; i++) r[i] = s * a[i];
    return r;
}

static double[] UnitRandom(Random rng, int n)
{
    var v = new double[n];
    double norm = 0;
    for (int i = 0; i < n; i++) { v[i] = rng.NextDouble() - 0.5; norm += v[i] * v[i]; }
    norm = System.Math.Sqrt(norm);
    for (int i = 0; i < n; i++) v[i] /= norm;
    return v;
}

static double[]? SolveSpd(double[,] a, double[] b)
{
    int n = b.Length;
    var l = new double[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j <= i; j++)
        {
            double sum = a[i, j];
            for (int k = 0; k < j; k++) sum -= l[i, k] * l[j, k];
            if (i == j)
            {
                if (sum <= 0) return null;
                l[i, j] = System.Math.Sqrt(sum);
            }
            else l[i, j] = sum / l[j, j];
        }
    var y = new double[n];
    for (int i = 0; i < n; i++)
    {
        double sum = b[i];
        for (int k = 0; k < i; k++) sum -= l[i, k] * y[k];
        y[i] = sum / l[i, i];
    }
    var xx = new double[n];
    for (int i = n - 1; i >= 0; i--)
    {
        double sum = y[i];
        for (int k = i + 1; k < n; k++) sum -= l[k, i] * xx[k];
        xx[i] = sum / l[i, i];
    }
    return xx;
}

// Jacobi eigen-decomposition; optionally accumulates eigenvectors.
static (double[] Values, double[,]? Vectors) JacobiEigen(double[,] input, bool wantVectors)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    double[,]? vv = null;
    if (wantVectors)
    {
        vv = new double[n, n];
        for (int i = 0; i < n; i++) vv[i, i] = 1.0;
    }
    for (int sweep = 0; sweep < 100; sweep++)
    {
        double off = 0.0;
        for (int p = 0; p < n; p++)
            for (int q = p + 1; q < n; q++)
                off += a[p, q] * a[p, q];
        if (System.Math.Sqrt(off) < 1e-11) break;
        for (int p = 0; p < n - 1; p++)
            for (int q = p + 1; q < n; q++)
            {
                double apq = a[p, q];
                if (System.Math.Abs(apq) < 1e-15) continue;
                double app = a[p, p], aqq = a[q, q];
                double tau = (aqq - app) / (2.0 * apq);
                double tt = System.Math.Sign(tau == 0 ? 1.0 : tau) / (System.Math.Abs(tau) + System.Math.Sqrt(1.0 + tau * tau));
                double cc = 1.0 / System.Math.Sqrt(1.0 + tt * tt);
                double ss = tt * cc;
                for (int k = 0; k < n; k++)
                {
                    if (k == p || k == q) continue;
                    double akp = a[k, p], akq = a[k, q];
                    a[k, p] = a[p, k] = cc * akp - ss * akq;
                    a[k, q] = a[q, k] = ss * akp + cc * akq;
                }
                a[p, p] = cc * cc * app - 2.0 * ss * cc * apq + ss * ss * aqq;
                a[q, q] = ss * ss * app + 2.0 * ss * cc * apq + cc * cc * aqq;
                a[p, q] = a[q, p] = 0.0;
                if (vv != null)
                    for (int k = 0; k < n; k++)
                    {
                        double vkp = vv[k, p], vkq = vv[k, q];
                        vv[k, p] = cc * vkp - ss * vkq;
                        vv[k, q] = ss * vkp + cc * vkq;
                    }
            }
    }
    var values = new double[n];
    for (int i = 0; i < n; i++) values[i] = a[i, i];
    return (values, vv);
}

BranchManifest BuildManifest() => new()
{
    BranchId = "phase449-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase449",
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

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) &&
    (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
        ? value.GetBoolean()
        : null;

public sealed record MemberSpec(string Name, EinsteinianShiabFamilyMember Member, EinsteinianShiabOperator Op, bool IsControl);

public sealed record ThetaSolveResult(
    double[] Theta, double ResidualNorm, double AbsoluteGradNorm, int Iterations, bool Converged,
    double FinalMu, int ThetaBlockRank, double ThetaBlockConditionNumber, double[] ResidualTrajectory);

public sealed record PointData(
    double T, double SB, double OneLoop, double VEff,
    int PosCount, int ZeroCount, int NegCount, double ZeroTol,
    double ThetaResidual, double ThetaAbsGradNorm, bool ThetaConverged,
    double[] X0, Func<double[], double> Sx, double[] EigenValues, double[,] EigenVectors);

public sealed record GaussianCache(
    double[] Lambdas, double[][] Vec, double[] FP, double[] FM, double F0, double[,] T4,
    int ExcludedZeroModes, int PosCount, int NegCount);

public sealed record GapSolveOutcome(
    bool Exists, bool Converged, int Iterations, double Residual, double MinHartreeMass,
    double FinalAlpha, double[] Sigma, double VInitial, double VFinal,
    bool MonotoneDecrease, bool Descended, double SigmaMin, double SigmaMax, string Reason);

public sealed record RotationCheck(
    string Member, double T, int RotatedModes, bool EigExists, bool RotExists,
    double VGEig, double VGRot, double AbsDiff, double RelDiffValue);

public sealed record SeedResult(
    int Seed, PointData[] Points, GapSolveOutcome[] Gaps, GaussianCache AnchorCache,
    int[] ModeCounts, int[] NegModesInAnsatz, int[] ExcludedZeroModes,
    long[] EvalPointSbEvals, long[] GaussianSbEvals,
    double RichardsonRel, double RichardsonEntrywiseRel, double OffsetImmunityRel, double OffsetEntrywiseRel);

public sealed record MemberResult(
    int Index, string Name, bool IsControl, string Phi1, string Phi2, double EinsteinCoefficient,
    List<SeedResult> Seeds);
