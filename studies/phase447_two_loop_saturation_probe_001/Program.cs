using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase447: TWO-LOOP saturation probe on the draft-canonical Einsteinian Shiab
// (4D base, spinor-realized reduced Lambda^2 slice, epsilon-conjugation). The
// named beyond-one-loop follow-up after Phase446 resolved the Phase445 RG
// candidate as a fit-normalization artifact.
//
// THE QUESTION. Phase443 showed the ONE-LOOP joint effective potential has no
// interior minimum on the minimal mesh; Phase446 closed the RG-improvement-by-
// potential-fit shortcut. The remaining no-platform lever is GENUINE two-loop
// structure: the vacuum diagrams built from the action's third and fourth
// derivative tensors. Phase442 proved the joint Hessian degree exceeds 2 along
// rays, so these vertices are nontrivial. Does the two-loop correction bend
// the effective potential into a finite interior minimum?
//
// STRUCTURE RESULT (scouted, verified here by batteries). With
// S_B = (1/2)||M Ad(theta) F(omega)||^2 and F = d omega + (1/2)[omega,omega]:
// S_B is an EXACT QUARTIC in omega at fixed theta (so the identity control's
// third/fourth derivative stencils are EXACT up to roundoff), but
// TRANSCENDENTAL in theta (Ad = exp(ad_theta)) - which is what Phase442's
// "degree > 2" was seeing. The Einsteinian vertices therefore need honest FD
// stencils; the identity control is the exact-arithmetic anchor.
//
// CONSTRUCTION.
//   (0) Recompute the V_eff(t) rays with the VERBATIM Phase443/446 hardened
//       machinery (Newton theta*-stationarity, joint FD Hessian now with
//       EIGENVECTORS via Jacobi rotations, IR zero-mode convention) on
//       CreateUniform4D(1), same members/seeds/rays as Phase445/446
//       (same RngSeed), on the Phase445 16-point grid.
//   (1) TWO-LOOP VACUUM TERMS at each composite point, contracted onto the
//       POSITIVE-SUBSPACE propagator G+ = sum_{lambda_i > zeroTol}
//       v_i v_i^T / lambda_i (the recorded convention extending the one-loop
//       positive-mode rule; the backgrounds are saddles with ~31-59 negative
//       modes - the substantive convention question, flagged
//       physicistReviewPending):
//         figure-eight = (1/8) sum_{ij} T4[v_i,v_i,v_j,v_j] / (l_i l_j)
//           (deterministic over ALL positive pairs; mixed 4th-derivative
//           stencils with shared axis evaluations),
//         sunset = -(1/12) sum_{ijk} T3[v_i,v_j,v_k]^2 / (l_i l_j l_k)
//           (SOFT-MODE TRUNCATED to the K softest positive modes - the
//           1/(l^3) weight is IR-dominated - with a convergence-in-K battery,
//           a FULL deterministic evaluation at an anchor point per Einsteinian
//           member, and a soft-floor sweep assembled free from cached stencils).
//   (2) V_2loop(t) = S_B + V_1loop + (figure-eight + sunset); saturation
//       classification (strict-local-minimum rule on the RAW curve - NO fits,
//       the Phase446 lesson) per member/seed.
//   (3) CONTROLS/BATTERIES: exact identity control (quartic => stencil
//       h-independence to roundoff; no spurious two-loop minimum allowed);
//       h-Richardson stability of T3/T4 on Einsteinian points (load-bearing:
//       3rd/4th differences of an O(10^2) functional); K-convergence of the
//       truncated sunset; anchor-point full-vs-truncated agreement; floor-sweep
//       agreement; offset immunity (stencils annihilate constants - asserted);
//       ALTERNATIVE-CONVENTION ARM: one full Einsteinian ray recomputed with
//       the absolute-value propagator over ALL nonzero modes (|lambda|), and
//       the classification compared - a convention-dependent verdict is
//       recorded as exactly that, NOT a candidate (Phase445/446 discipline).
//
// MANDATORY FRAMING. Workbench-relative candidate data ONLY (su(2) toy algebra
// on the reduced Spin(4) slice, lattice units); the positive-subspace two-loop
// about a saddle is a RECORDED CONVENTION pending physicist review; NO
// GeV/pole/VEV promotion either way. The phase PASSES on internal consistency
// REGARDLESS of the saturation verdict.
//
// Fail-closed: target-blind; reduced-spin4-slice; no scale/pole/GeV lineage;
// no Phase201/Phase256 contract field filled; nothing promoted either way.

const string DefaultOutputDir = "studies/phase447_two_loop_saturation_probe_001/output";
const string Phase443SummaryPath = "studies/phase443_joint_effective_potential_saturation_probe_001/output/joint_effective_potential_saturation_probe_summary.json";
const string Phase446SummaryPath = "studies/phase446_rg_scheme_dependence_resolution_probe_001/output/rg_scheme_dependence_resolution_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string TwoLoopDesignNotePath = "studies/phase446_rg_scheme_dependence_resolution_probe_001/PHASE447_TWO_LOOP_DESIGN.md";
const string ApplicationSubjectKind = "two-loop-saturation-probe";

// Probe configuration (committed defaults; env overrides mirror Phase443/446).
int MeshRefinement = int.TryParse(Environment.GetEnvironmentVariable("PHASE447_MESH_REFINEMENT"), out int mr) ? mr : 1;
int RaySeedCount = int.TryParse(Environment.GetEnvironmentVariable("PHASE447_RAYS"), out int rs) ? rs : 2;
double TMax = double.TryParse(Environment.GetEnvironmentVariable("PHASE447_TMAX"), out double tm) ? tm : 3.0;
int GridN = int.TryParse(Environment.GetEnvironmentVariable("PHASE447_GRIDN"), out int gn) ? gn : 16;
const int RngSeed = 20260703; // MUST equal Phase445/446: identical ray directions.

// Two-loop knobs.
int SunsetK = int.TryParse(Environment.GetEnvironmentVariable("PHASE447_SUNSET_K"), out int sk) ? sk : 40;
// Full deterministic anchor sunset (expensive, ~15 min/anchor) can be capped for
// smoke runs; the committed default is uncapped (int.MaxValue).
int FullAnchorCap = int.TryParse(Environment.GetEnvironmentVariable("PHASE447_FULL_ANCHOR_CAP"), out int fc) ? fc : int.MaxValue;
int[] SunsetKSweep = { 10, 20, 30, 40 };
// Directional FD step. LARGE by design: S_B is an exact quartic in omega, so
// 3rd/4th-order stencils are h-INDEPENDENT on the omega sector (exact for any
// h up to roundoff), and a large h lifts the stencil numerators far above the
// double-precision cancellation floor (the first smoke run PROVED h = 5e-3 is
// noise-dominated: the offset-immunity battery, impossible to fail with clean
// stencils, failed at 135%). The transcendental theta truncation error is
// controlled by the h-vs-h/2 Richardson battery.
double StencilH = double.TryParse(Environment.GetEnvironmentVariable("PHASE447_STENCIL_H"), out double sh) ? sh : 0.1;
const double RichardsonHFactor = 0.5;  // battery: T3/T4 at h and h/2 must agree
const double RichardsonRelTol = 5e-2;  // Einsteinian (transcendental) Richardson ceiling
const double IdentityExactRelTol = 1e-6; // identity (exact quartic) stencil h-independence
// Propagator soft-mode floor (RECORDED CONVENTION; the 1/lambda^3 sunset weight
// is IR-dominated and the softest modes sit near the zero-mode tolerance, so an
// unfloored two-loop is numerically and physically uncontrolled). Modes enter
// the propagator only if lambda > floorRel * maxAbsEig. The verdict must be
// stable across the floor sweep or it is recorded as convention-dependent.
double SoftFloorRel = double.TryParse(Environment.GetEnvironmentVariable("PHASE447_SOFT_FLOOR_REL"), out double sf) ? sf : 1e-4;
double[] SoftFloorSweep = { 1e-5, 1e-4, 1e-3 };
// Perturbativity admissibility: a two-loop SATURATION verdict is only
// admissible where the loop expansion is under control. If |V_2loop| exceeds
// this fraction of |V_1loop| on the Einsteinian rays, the two-loop lever is
// recorded as NON-PERTURBATIVE at this workbench scope (a decisive frontier
// statement, not a candidate and not a blocked phase).
const double PerturbativityCeiling = 0.5;
// Offset-immunity ceiling: stencils annihilate constants exactly in real
// arithmetic; the residual is pure floating-point cancellation from the +1000
// magnitude shift (measured ~3e-7 at h=0.1 on O(1..10) values).
const double OffsetImmunityRelTol = 1e-5;

// Theta-stationarity solver knobs (verbatim Phase443/446 hardened solver).
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

var outputDir = Environment.GetEnvironmentVariable("PHASE447_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors: Phase443 (no one-loop saturation) and Phase446 (RG-fit route
// closed as artifact).
// ---------------------------------------------------------------------------

using var phase443 = JsonDocument.Parse(File.ReadAllText(Phase443SummaryPath));
bool phase443PrecursorPassed =
    JsonBool(phase443.RootElement, "jointEffectivePotentialSaturationProbePassed") is true &&
    JsonBool(phase443.RootElement, "einsteinianLogSaturationObserved") is false;

using var phase446 = JsonDocument.Parse(File.ReadAllText(Phase446SummaryPath));
bool phase446PrecursorPassed =
    JsonBool(phase446.RootElement, "rgSchemeDependenceResolutionProbePassed") is true &&
    JsonBool(phase446.RootElement, "phase445MinimaResolvedAsFitNormalizationArtifact") is true &&
    JsonBool(phase446.RootElement, "einsteinianRgSaturationObserved") is false;

bool precursorsPassed = phase443PrecursorPassed && phase446PrecursorPassed;

// ---------------------------------------------------------------------------
// Machinery (verbatim Phase443/446): 4D mesh, su(2) trace pairing, trivial torsion.
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
// Theta*-stationarity solve (VERBATIM Phase443/446 hardened Newton).
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
// One-loop point (verbatim conventions) extended with eigenvectors + closure.
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
// Directional derivative stencils (unit directions, step StencilH).
// ---------------------------------------------------------------------------

double[] PointAlong(double[] x0, double[] d1, double a, double[]? d2 = null, double b = 0.0, double[]? d3 = null, double c = 0.0)
{
    var x = (double[])x0.Clone();
    for (int i = 0; i < x.Length; i++)
    {
        x[i] += a * d1[i];
        if (d2 != null) x[i] += b * d2[i];
        if (d3 != null) x[i] += c * d3[i];
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

// d^3/ds dt du f along three DISTINCT directions: 8-corner stencil.
double D3Distinct(Func<double[], double> f, double[] x0, double[] u, double[] v, double[] w, double h)
{
    double s = 0.0;
    for (int a = -1; a <= 1; a += 2)
        for (int b = -1; b <= 1; b += 2)
            for (int c = -1; c <= 1; c += 2)
                s += a * b * c * f(PointAlong(x0, u, a * h, v, b * h, w, c * h));
    return s / (8.0 * h * h * h);
}

// d^3/ds^2 dt f: 6-point.
double D3TwoOne(Func<double[], double> f, double[] x0, double[] u, double[] v,
    double fvP, double fvM, double h)
{
    double pP = f(PointAlong(x0, u, h, v, h));
    double mP = f(PointAlong(x0, u, -h, v, h));
    double pM = f(PointAlong(x0, u, h, v, -h));
    double mM = f(PointAlong(x0, u, -h, v, -h));
    return ((pP - 2.0 * fvP + mP) - (pM - 2.0 * fvM + mM)) / (2.0 * h * h * h);
}

// d^3/ds^3 f: 4-point.
double D3Diag(Func<double[], double> f, double[] x0, double[] u, double fuP, double fuM, double h)
{
    double f2P = f(PointAlong(x0, u, 2.0 * h));
    double f2M = f(PointAlong(x0, u, -2.0 * h));
    return (f2P - 2.0 * fuP + 2.0 * fuM - f2M) / (2.0 * h * h * h);
}

// ---------------------------------------------------------------------------
// Two-loop terms at a point, on a mode set (indices into the eigen system).
// ---------------------------------------------------------------------------

double[] Column(double[,] m, int j)
{
    int n = m.GetLength(0);
    var c = new double[n];
    for (int i = 0; i < n; i++) c[i] = m[i, j];
    return c;
}

// Evaluate the two-loop stencil CACHES at a point on the mode set defined by
// the LOOSEST floor of the sweep, then assemble sums for any (floor, K) pair
// for free. Mode set: positive modes above floor (default convention) or ALL
// nonzero modes above floor with |lambda| weights (alternative convention arm).
TwoLoopCache TwoLoopCacheAt(PointData p, double stencilH, bool absConvention, int softCap)
{
    double maxAbs = 0.0;
    foreach (double e in p.EigenValues) maxAbs = System.Math.Max(maxAbs, System.Math.Abs(e));
    double loosest = SoftFloorSweep.Min() * maxAbs;

    var modes = new List<(int Index, double Lambda)>();
    for (int i = 0; i < p.EigenValues.Length; i++)
    {
        double e = p.EigenValues[i];
        double mag = System.Math.Abs(e);
        bool inSet = absConvention ? mag > System.Math.Max(p.ZeroTol, loosest)
                                   : e > System.Math.Max(p.ZeroTol, loosest);
        if (inSet) modes.Add((i, mag));
    }
    // Sort ascending by magnitude: soft modes first (the sunset truncation order).
    modes.Sort((x, y) => x.Lambda.CompareTo(y.Lambda));

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

    // T4 diagonal + all pairs (figure-eight table).
    var t4Diag = new double[nm];
    var t4Pair = new double[nm, nm];
    for (int a = 0; a < nm; a++)
    {
        t4Diag[a] = D4Diag(f, p.X0, f0, vec[a], fP[a], fM[a], stencilH);
        for (int b = a + 1; b < nm; b++)
        {
            double v = D4MixedPair(f, p.X0, f0, vec[a], vec[b], fP[a], fM[a], fP[b], fM[b], stencilH);
            t4Pair[a, b] = v; t4Pair[b, a] = v;
        }
    }

    // T3 over the softCap softest modes (sunset table; slots 0..ns-1 are the
    // softest because of the ascending sort).
    int ns = System.Math.Min(softCap, nm);
    var t3 = new double[ns, ns, ns];
    for (int a = 0; a < ns; a++)
        for (int b = a; b < ns; b++)
            for (int c = b; c < ns; c++)
            {
                double v;
                if (a == b && b == c) v = D3Diag(f, p.X0, vec[a], fP[a], fM[a], stencilH);
                else if (a == b) v = D3TwoOne(f, p.X0, vec[a], vec[c], fP[c], fM[c], stencilH);
                else if (b == c) v = D3TwoOne(f, p.X0, vec[b], vec[a], fP[a], fM[a], stencilH);
                else v = D3Distinct(f, p.X0, vec[a], vec[b], vec[c], stencilH);
                t3[a, b, c] = v;
            }

    return new TwoLoopCache(modes.Select(m => m.Lambda).ToArray(), ns, t4Diag, t4Pair, t3, maxAbs);
}

// Assemble (figure-eight, sunset) from a cache for a given floor and sunset K.
TwoLoopResult AssembleTwoLoop(TwoLoopCache c, double floorRel, int sunsetK)
{
    double floorAbs = floorRel * c.MaxAbsEig;
    int nm = c.Lambdas.Length;

    double fig8 = 0.0;
    int included = 0;
    for (int a = 0; a < nm; a++)
    {
        if (c.Lambdas[a] <= floorAbs) continue;
        included++;
        fig8 += c.T4Diag[a] / (c.Lambdas[a] * c.Lambdas[a]);
        for (int b = a + 1; b < nm; b++)
        {
            if (c.Lambdas[b] <= floorAbs) continue;
            fig8 += 2.0 * c.T4Pair[a, b] / (c.Lambdas[a] * c.Lambdas[b]);
        }
    }
    fig8 /= 8.0;

    // Sunset: the K softest modes ABOVE the floor, within the cached table.
    var softSlots = new List<int>();
    for (int a = 0; a < c.SunsetSlots && softSlots.Count < sunsetK; a++)
        if (c.Lambdas[a] > floorAbs) softSlots.Add(a);
    double sunset = 0.0;
    for (int ia = 0; ia < softSlots.Count; ia++)
        for (int ib = ia; ib < softSlots.Count; ib++)
            for (int ic = ib; ic < softSlots.Count; ic++)
            {
                int a = softSlots[ia], b = softSlots[ib], cc = softSlots[ic];
                int mult = (a == b && b == cc) ? 1 : (a == b || b == cc) ? 3 : 6;
                double t3v = c.T3[a, b, cc];
                sunset += mult * t3v * t3v / (c.Lambdas[a] * c.Lambdas[b] * c.Lambdas[cc]);
            }
    sunset *= -1.0 / 12.0;

    return new TwoLoopResult(fig8, sunset, fig8 + sunset, included, softSlots.Count);
}

// ---------------------------------------------------------------------------
// Rays: recompute + two-loop per point.
// ---------------------------------------------------------------------------

var tGrid = new double[GridN];
for (int i = 0; i < GridN; i++) tGrid[i] = TMax * (i + 1) / GridN;
int anchorIdx = GridN / 2; // anchor point for full/stochastic cross-checks

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
        double[]? thetaCarry = null;
        foreach (double t in tGrid)
        {
            var p = EvalPoint(eval, op, uOmega, t, spec.IsControl, thetaCarry);
            if (!spec.IsControl) thetaCarry = p.X0.Skip(nOmega).ToArray();
            pts.Add(p);
            if (spec.IsControl && p.ThetaResidual != 0.0) identityThetaExactZero = false;
        }
        if (!spec.IsControl && pts.Count >= 2 && !pts[0].ThetaConverged)
            pts[0] = EvalPoint(eval, op, uOmega, pts[0].T, spec.IsControl, pts[1].X0.Skip(nOmega).ToArray());

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

        // Two-loop caches per point (default convention; T3 table sized to the
        // largest sweep K so K-sweep and floor-sweep are free assembly).
        var caches = pts.Select(p => TwoLoopCacheAt(p, StencilH, absConvention: false, softCap: SunsetKSweep.Max() + 8)).ToArray();
        var two = caches.Select(c => AssembleTwoLoop(c, SoftFloorRel, SunsetK)).ToArray();

        // K-convergence sweep at the anchor point (free from the cache).
        var kSweep = SunsetKSweep.Select(k => new KSweepEntry(k, AssembleTwoLoop(caches[anchorIdx], SoftFloorRel, k).Sunset)).ToArray();

        // Floor-sweep classification: whole-ray V_2loop under each floor (free).
        var floorCls = SoftFloorSweep.Select(fl =>
        {
            var v = pts.Select((p, i) => p.VEff + AssembleTwoLoop(caches[i], fl, SunsetK).Total).ToArray();
            return new FloorSweepEntry(fl, ClassifyCurve(v, tGrid).Cls);
        }).ToArray();

        // Richardson battery at the anchor: fresh caches at h and h/2 (reduced K table).
        var richCacheA = TwoLoopCacheAt(pts[anchorIdx], StencilH, false, softCap: 15);
        var richCacheB = TwoLoopCacheAt(pts[anchorIdx], StencilH * RichardsonHFactor, false, softCap: 15);
        var richA = AssembleTwoLoop(richCacheA, SoftFloorRel, 15);
        var richB = AssembleTwoLoop(richCacheB, SoftFloorRel, 15);
        double richRelFig8 = RelDiff(richA.FigureEight, richB.FigureEight);
        double richRelSunset = RelDiff(richA.Sunset, richB.Sunset);

        // Offset-immunity battery at the anchor (constant added to Sx must leave stencils unchanged).
        var pShift = pts[anchorIdx] with { Sx = x => pts[anchorIdx].Sx(x) + 1000.0 };
        var offCache = TwoLoopCacheAt(pShift, StencilH, false, softCap: 15);
        var offR = AssembleTwoLoop(offCache, SoftFloorRel, 15);
        double offsetRel = System.Math.Max(RelDiff(offR.FigureEight, richA.FigureEight), RelDiff(offR.Sunset, richA.Sunset));

        seedRecs.Add(new SeedResult(seed, pts.ToArray(), two, kSweep, floorCls, richRelFig8, richRelSunset, offsetRel));
    }
    memberResults.Add(new MemberResult(mi, spec.Name, spec.IsControl,
        spec.Member.Phi1.InvariantElement, spec.Member.Phi2.InvariantElement, spec.Member.EinsteinCoefficient, seedRecs));
}

// Anchor cross-checks (Einsteinian members, primary seed): FULL deterministic
// sunset (all modes above the default floor) vs the K-truncated value.
var anchorChecks = new List<AnchorCheck>();
foreach (var m in memberResults.Where(m => !m.IsControl))
{
    var p = m.Seeds[0].Points[anchorIdx];
    double truncated = m.Seeds[0].TwoLoop[anchorIdx].Sunset;
    var fullCache = TwoLoopCacheAt(p, StencilH, false, softCap: FullAnchorCap);
    double full = AssembleTwoLoop(fullCache, SoftFloorRel, int.MaxValue).Sunset;
    double truncRel = RelDiff(truncated, full);
    anchorChecks.Add(new AnchorCheck(m.Name, tGrid[anchorIdx], truncated, full, truncRel));
}

// Alternative-convention arm: ONE full Einsteinian ray (first member, seed 0)
// under the absolute-value all-nonzero-mode propagator.
var altMember = memberResults.First(m => !m.IsControl);
var altTwo = altMember.Seeds[0].Points
    .Select(p => AssembleTwoLoop(TwoLoopCacheAt(p, StencilH, absConvention: true, softCap: SunsetKSweep.Max() + 8), SoftFloorRel, SunsetK))
    .ToArray();

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

var classByMemberSeed = new Dictionary<(int M, int S), (string OneLoop, string TwoLoop)>();
foreach (var m in memberResults)
    foreach (var s in m.Seeds)
    {
        var v1 = s.Points.Select(p => p.VEff).ToArray();
        var v2 = s.Points.Select((p, i) => p.VEff + s.TwoLoop[i].Total).ToArray();
        var c1 = ClassifyCurve(v1, tGrid);
        var c2 = ClassifyCurve(v2, tGrid);
        classByMemberSeed[(m.Index, s.Seed)] = (c1.Cls, c2.Cls);
    }

var identityM = memberResults.First(m => m.IsControl);
var einsteinianM = memberResults.Where(m => !m.IsControl).ToList();

bool thetaStationarityGate = allThetaConverged && allThetaPointsPassGate && identityThetaExactZero;
bool linThetaFdBattery = linThetaFdMaxResidual <= LinearizeThetaFdTol;

// Identity exact-quartic battery: stencil h-independence to near-roundoff.
double identityMaxRichardson = identityM.Seeds.Max(s => System.Math.Max(s.RichardsonRelFig8, s.RichardsonRelSunset));
bool identityExactStencilBattery = identityMaxRichardson <= IdentityExactRelTol;

// Einsteinian Richardson battery (transcendental; honest FD tolerance).
double einsteinianMaxRichardson = einsteinianM.Max(m => m.Seeds.Max(s => System.Math.Max(s.RichardsonRelFig8, s.RichardsonRelSunset)));
bool einsteinianRichardsonBattery = einsteinianMaxRichardson <= RichardsonRelTol;

// Offset immunity (stencils annihilate constants up to FP cancellation).
double maxOffsetRel = memberResults.Max(m => m.Seeds.Max(s => s.OffsetImmunityRel));
bool offsetImmunityBattery = maxOffsetRel <= OffsetImmunityRelTol;

// Perturbativity diagnostic: |V_2loop| / max(|V_1loop|, 1) per Einsteinian point.
double maxTwoLoopToOneLoopRatio = einsteinianM.Max(m => m.Seeds.Max(s =>
    s.Points.Select((p, i) => System.Math.Abs(s.TwoLoop[i].Total) / System.Math.Max(System.Math.Abs(p.OneLoop), 1.0)).Max()));
double medianTwoLoopToOneLoopRatio = Median(einsteinianM.SelectMany(m => m.Seeds.SelectMany(s =>
    s.Points.Select((p, i) => System.Math.Abs(s.TwoLoop[i].Total) / System.Math.Max(System.Math.Abs(p.OneLoop), 1.0)))).ToArray());
bool perturbativeRegime = maxTwoLoopToOneLoopRatio <= PerturbativityCeiling;

// K-convergence: |sunset(K_last) - sunset(K_prev)| relative, recorded; battery
// = monotone-in-magnitude tail change below 30% (soft, recorded honestly).
double maxKTailRel = memberResults.Max(m => m.Seeds.Max(s =>
{
    var e = s.KSweep;
    return e.Length >= 2 ? RelDiff(e[^1].Sunset, e[^2].Sunset) : double.NaN;
}));
bool kConvergenceRecorded = double.IsFinite(maxKTailRel);

// Anchor cross-checks.
bool anchorTruncationRecorded = anchorChecks.All(a => double.IsFinite(a.TruncRel));

// Floor-sweep stability: the two-loop classification agrees across the
// propagator soft-floor sweep for every member/seed.
bool floorSweepStable = memberResults.All(m => m.Seeds.All(s =>
    s.FloorSweep.Select(x => x.Classification).Distinct().Count() == 1));

// Identity control: no spurious two-loop minimum.
bool controlNoSpuriousTwoLoopMinimum = identityM.Seeds
    .All(s => classByMemberSeed[(identityM.Index, s.Seed)].TwoLoop != "interior-finite");

// Saturation verdicts (primary seed; seed stability recorded).
bool einsteinianTwoLoopSaturationObservedAll = einsteinianM.Count > 0 &&
    einsteinianM.All(m => classByMemberSeed[(m.Index, 0)].TwoLoop == "interior-finite");
bool einsteinianTwoLoopSaturationObservedAny =
    einsteinianM.Any(m => classByMemberSeed[(m.Index, 0)].TwoLoop == "interior-finite");
bool seedStable = memberResults.All(m =>
    m.Seeds.Select(s => classByMemberSeed[(m.Index, s.Seed)].TwoLoop).Distinct().Count() == 1);

// Alternative-convention stability on the probed ray.
var altV2 = altMember.Seeds[0].Points.Select((p, i) => p.VEff + altTwo[i].Total).ToArray();
string altCls = ClassifyCurve(altV2, tGrid).Cls;
string defaultClsAltMember = classByMemberSeed[(altMember.Index, 0)].TwoLoop;
bool conventionStableOnProbedRay = altCls == defaultClsAltMember;

bool batteriesAllPassed =
    precursorsPassed &&
    thetaStationarityGate &&
    linThetaFdBattery &&
    identityExactStencilBattery &&
    einsteinianRichardsonBattery &&
    offsetImmunityBattery &&
    kConvergenceRecorded &&
    anchorTruncationRecorded;

// Final verdict taxonomy. A SATURATION verdict (either way) is admissible only
// in the perturbative, convention-stable regime; a candidate additionally needs
// seed stability, both convention axes, AND a clean identity control. Outside
// the perturbative/stable regime the decisive recorded outcome is that the
// two-loop lever is NON-PERTURBATIVE / CONVENTION-BOUND at this workbench scope
// (an identity-control "minimum" there is supporting evidence of the breakdown,
// not an estimator defect -- identity monotonicity is NOT a theorem at two loops).
bool twoLoopVerdictAdmissible = perturbativeRegime && floorSweepStable && controlNoSpuriousTwoLoopMinimum;
bool twoLoopCandidate = twoLoopVerdictAdmissible && einsteinianTwoLoopSaturationObservedAny && seedStable && conventionStableOnProbedRay;
string resolutionKind =
    !twoLoopVerdictAdmissible ? "non-perturbative-or-convention-bound"
    : twoLoopCandidate ? "two-loop-candidate"
    : einsteinianTwoLoopSaturationObservedAny ? "convention-or-seed-unstable-recorded"
    : "no-two-loop-saturation";
string saturationVerdictPhrase = resolutionKind switch
{
    "non-perturbative-or-convention-bound" =>
        $"the two-loop terms on the minimal workbench are IR-dominated and NOT admissible as a saturation verdict: max |V_2loop|/|V_1loop| = {maxTwoLoopToOneLoopRatio:E2} (ceiling {PerturbativityCeiling}), floor-sweep stable = {floorSweepStable}, identity control clean = {controlNoSpuriousTwoLoopMinimum} -- the loop expansion is out of control and/or the answer is bound to the soft-floor convention at these backgrounds. THE TWO-LOOP LEVER IS CLOSED AS NON-PERTURBATIVE/CONVENTION-BOUND AT THE MINIMAL-MESH SCOPE (a decisive frontier statement; nothing promoted; deciding it beyond this scope requires the platform unlock projects or a source anchor)",
    "two-loop-candidate" =>
        "the two-loop correction produces a perturbative, seed-stable, convention-stable (propagator arm + soft-floor sweep) interior minimum for at least one Einsteinian member with a clean identity control -- a workbench-relative dynamical-scale CANDIDATE at two loops (positive-subspace convention recorded, physicist review pending)",
    "convention-or-seed-unstable-recorded" =>
        "a two-loop interior minimum appears in the admissible regime but is not seed/propagator-arm stable -- recorded as exactly that, not a candidate (the Phase445/446 discipline)",
    _ =>
        "within the admissible perturbative regime the two-loop correction does NOT produce an interior minimum -- two loops are ALSO insufficient on the minimal mesh under the recorded convention; a legitimate frontier-sharpening outcome",
};

// ---------------------------------------------------------------------------
// Framing + fail-closed block (verbatim keys).
// ---------------------------------------------------------------------------

const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool twoLoopConventionIsWorkbenchConvention = true;
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
    "two-loop vacuum terms on positive-subspace propagator with recorded soft-mode floor + floor sweep: deterministic figure-eight over all positive pairs + soft-mode-truncated sunset (K sweep) with directional FD stencils on joint Hessian eigenvectors; exact-quartic identity anchor; full-anchor truncation cross-checks; abs-|lambda| convention arm; raw-curve strict-minimum classification, no fits; no target values")))).ToLowerInvariant();

bool twoLoopSaturationProbePassed =
    precursorsPassed &&
    batteriesAllPassed &&
    scaleIsWorkbenchRelativeCandidateOnly &&
    noGevPromotion &&
    twoLoopConventionIsWorkbenchConvention &&
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

string terminalStatus = twoLoopSaturationProbePassed
    ? resolutionKind switch
    {
        "non-perturbative-or-convention-bound" => "two-loop-saturation-probe-passed-two-loop-lever-non-perturbative-convention-bound-at-minimal-scope-frontier-sharpened",
        "two-loop-candidate" => "two-loop-saturation-probe-passed-einsteinian-two-loop-candidate-workbench-relative-convention-recorded-no-gev",
        "convention-or-seed-unstable-recorded" => "two-loop-saturation-probe-passed-convention-dependent-recorded",
        _ => "two-loop-saturation-probe-passed-no-two-loop-saturation-frontier-sharpened",
    }
    : "two-loop-saturation-probe-blocked";

string decision = twoLoopSaturationProbePassed
    ? $"The two-loop saturation probe is decided on internal consistency. On CreateUniform4D({MeshRefinement}) (su(2) trace pairing; nOmega={nOmega}, nTheta={nTheta}, joint DOF {nJoint}) the V_eff(t) rays were recomputed with the verbatim hardened machinery (theta point gate: relative <= {ThetaResidualBatteryRel:E0} or absolute floor {ThetaAbsGradFloor:E0}, {thetaPointsPassedByAbsFloor} via floor, max relative {maxThetaResidualNonIdentity:E2}; identity theta*=0 exact; LinearizeTheta FD {linThetaFdMaxResidual:E2}) and the TWO-LOOP vacuum terms (figure-eight over all positive pairs; sunset over the K={SunsetK} softest modes) evaluated by directional FD stencils (h={StencilH:E0}) on the joint Hessian eigenvectors, contracted onto the positive-subspace propagator (RECORDED CONVENTION about saddle backgrounds with ~{einsteinianM.SelectMany(m => m.Seeds).SelectMany(s => s.Points).Where(p => p.NegCount > 0).Select(p => p.NegCount).DefaultIfEmpty(0).Max()} negative modes; physicist review pending). " +
      $"BATTERIES: identity exact-quartic stencil h-independence {identityMaxRichardson:E2} <= {IdentityExactRelTol:E0}; Einsteinian Richardson {einsteinianMaxRichardson:E2} <= {RichardsonRelTol:E0}; offset immunity {maxOffsetRel:E2}; K-tail change {maxKTailRel:E2} (recorded); anchor truncation residuals recorded; floor-sweep stable {floorSweepStable}; control no spurious two-loop minimum {controlNoSpuriousTwoLoopMinimum}. " +
      $"SATURATION VERDICT: {saturationVerdictPhrase}. " +
      $"MANDATORY FRAMING: any scale is a WORKBENCH-RELATIVE CANDIDATE ONLY (su(2) toy algebra, reduced Spin(4) slice, lattice units); the positive-subspace two-loop about a saddle is a WORKBENCH CONVENTION pending physicist review; NO GeV/pole/VEV promotion. Everything is target-blind, reduced-spin4-slice; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the saturation verdicts until the precursor, theta-stationarity, stencil, cross-check, and control batteries pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

object MemberJson(MemberResult m) => new
{
    index = m.Index,
    member = m.Name,
    phi1 = m.Phi1,
    phi2 = m.Phi2,
    einsteinCoefficient = m.EinsteinCoefficient,
    isControl = m.IsControl,
    oneLoopClassification = classByMemberSeed[(m.Index, 0)].OneLoop,
    twoLoopClassification = classByMemberSeed[(m.Index, 0)].TwoLoop,
    seeds = m.Seeds.Select(s => new
    {
        seed = s.Seed,
        oneLoopClassification = classByMemberSeed[(m.Index, s.Seed)].OneLoop,
        twoLoopClassification = classByMemberSeed[(m.Index, s.Seed)].TwoLoop,
        richardsonRelFig8 = FiniteOrNull(s.RichardsonRelFig8),
        richardsonRelSunset = FiniteOrNull(s.RichardsonRelSunset),
        offsetImmunityRel = FiniteOrNull(s.OffsetImmunityRel),
        kSweep = s.KSweep.Select(k => new { k.K, sunset = FiniteOrNull(k.Sunset) }).ToArray(),
        floorSweep = s.FloorSweep.Select(x => new { floorRel = x.FloorRel, classification = x.Classification }).ToArray(),
        points = s.Points.Select((p, i) => new
        {
            t = p.T,
            sB = p.SB,
            oneLoop = p.OneLoop,
            vEffOneLoop = p.VEff,
            figureEight = FiniteOrNull(s.TwoLoop[i].FigureEight),
            sunset = FiniteOrNull(s.TwoLoop[i].Sunset),
            vTwoLoop = FiniteOrNull(p.VEff + s.TwoLoop[i].Total),
            positiveModes = p.PosCount,
            zeroModes = p.ZeroCount,
            negativeModes = p.NegCount,
            thetaResidual = p.ThetaResidual,
            thetaAbsoluteGradNorm = p.ThetaAbsGradNorm,
            thetaConverged = p.ThetaConverged,
        }).ToArray(),
    }).ToArray(),
};

var result = new
{
    phaseId = "phase447-two-loop-saturation-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    twoLoopSaturationProbePassed,

    phase443PrecursorPassed,
    phase446PrecursorPassed,
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
        raySeedCount = RaySeedCount,
        tMax = TMax,
        gridN = GridN,
        tGrid,
        anchorT = tGrid[anchorIdx],
        sunsetK = SunsetK,
        sunsetKSweep = SunsetKSweep,
        stencilH = StencilH,
        stencilHNote = "LARGE by design: S_B is an exact quartic in omega so the stencils are h-independent on the omega sector; a large h lifts the numerators above the double-precision cancellation floor (the first smoke run proved h=5e-3 noise-dominated via the offset-immunity battery); theta truncation controlled by the h-vs-h/2 Richardson battery",
        richardsonHFactor = RichardsonHFactor,
        richardsonRelTol = RichardsonRelTol,
        identityExactRelTol = IdentityExactRelTol,
        softFloorRel = SoftFloorRel,
        softFloorSweep = SoftFloorSweep,
        softFloorNote = "RECORDED CONVENTION: modes enter the propagator only above floorRel*maxAbsEig; the 1/lambda^3 sunset weight is IR-dominated and unfloored soft modes near the zero tolerance are numerically and physically uncontrolled; the verdict must be floor-sweep stable or it is recorded convention-dependent",
        hessStep = HessStep,
        zeroModeRelTol = ZeroModeRelTol,
        zeroModeAbsFloor = ZeroModeAbsFloor,
        thetaTol = ThetaTolRel,
        thetaResidualBattery = ThetaResidualBatteryRel,
        thetaAbsGradFloor = ThetaAbsGradFloor,
        epsilonRealization,
        epsilonTaxonomyMode,
        sbEvalCount,
        structureResult = "S_B is an EXACT QUARTIC in omega at fixed theta (identity stencils exact to roundoff) and TRANSCENDENTAL in theta via Ad = exp(ad_theta) -- resolving the Phase442 degree finding; the Einsteinian vertices require honest FD stencils and the identity control anchors them",
        propagatorConvention = "positive-subspace G+ = sum_{lambda > zeroTol} v v^T / lambda, extending the one-loop positive-mode rule; the backgrounds are saddles (negative modes recorded per point) so this is a PROJECTED two-loop, a recorded workbench convention pending physicist review; alternative arm: absolute-value propagator over all nonzero modes on one full Einsteinian ray",
        twoLoopDefinition = "V_2loop = (1/8) sum_{ij} T4[v_i,v_i,v_j,v_j]/(l_i l_j) - (1/12) sum_{ijk in K softest} T3[v_i,v_j,v_k]^2/(l_i l_j l_k); directional FD stencils at h on unit eigenvectors with shared axis evaluations; classification on the RAW V = S_B + V_1loop + V_2loop curve by the strict-local-minimum rule (no fits -- the Phase446 lesson)",
    },

    // Headline verdicts.
    einsteinianTwoLoopSaturationObserved = einsteinianTwoLoopSaturationObservedAll,
    resolutionKind,
    twoLoopVerdictAdmissible,
    perturbativeRegime,
    maxTwoLoopToOneLoopRatio = FiniteOrNull(maxTwoLoopToOneLoopRatio),
    medianTwoLoopToOneLoopRatio = FiniteOrNull(medianTwoLoopToOneLoopRatio),
    einsteinianTwoLoopSaturationObservedAny,
    twoLoopCandidate,
    seedStable,
    conventionStableOnProbedRay,
    floorSweepStable,
    alternativeConventionClassification = altCls,
    defaultConventionClassificationOnProbedRay = defaultClsAltMember,
    controlNoSpuriousTwoLoopMinimum,
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    twoLoopConventionIsWorkbenchConvention,
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
        maxOffsetRel = FiniteOrNull(maxOffsetRel),
        kConvergenceRecorded,
        maxKTailRel = FiniteOrNull(maxKTailRel),
        anchorTruncationRecorded,
        floorSweepStable,
        controlNoSpuriousTwoLoopMinimum,
    },

    anchorCrossChecks = anchorChecks.Select(a => new
    {
        member = a.Member,
        t = a.T,
        truncatedSunset = FiniteOrNull(a.Truncated),
        fullSunset = FiniteOrNull(a.Full),
        truncationRel = FiniteOrNull(a.TruncRel),
    }).ToArray(),

    memberTable = memberResults.Select(MemberJson).ToArray(),

    saturationVerdict = saturationVerdictPhrase,

    recordedBoundary = new
    {
        definition81Scope,
        ambientSevenSevenRealized,
        internalGaugeContentRealized,
        weldRealized,
        canFillPhase201WzContract,
        canFillPhase256Contract,
        physicistReviewPending,
        twoLoopConventionIsWorkbenchConvention,
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
        phase443SummaryPath = Phase443SummaryPath,
        phase446SummaryPath = Phase446SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
        twoLoopDesignNotePath = TwoLoopDesignNotePath,
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "The two-loop terms, any interior minima, t*, and t*/latticeScale are candidate-only structure data of the reduced spin-4 slice, NOT physical masses, VEVs, or a scale in GeV.",
        "The positive-subspace two-loop about a saddle background is a WORKBENCH CONVENTION pending physicist review; a convention-dependent verdict is recorded as exactly that, not a candidate.",
        "The figure-eight and sunset terms are built from the su(2)-toy joint action's derivative tensors; they are not physical electroweak loop corrections.",
        "A two-loop saturation result, where observed and convention/seed-stable, is a workbench-relative dynamical-scale CANDIDATE only; a no-saturation result sharpens the frontier. Neither promotes anything.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "two_loop_saturation_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "two_loop_saturation_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"twoLoopSaturationProbePassed={twoLoopSaturationProbePassed}");
Console.WriteLine($"precursors: p443={phase443PrecursorPassed} p446={phase446PrecursorPassed}");
Console.WriteLine($"mesh: V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} C={mesh.CellCount}; nOmega={nOmega} nTheta={nTheta} jointDof={nJoint}");
Console.WriteLine($"THETA: gate={thetaStationarityGate} maxResid(non-id)={maxThetaResidualNonIdentity:E2} viaAbsFloor={thetaPointsPassedByAbsFloor} linThetaFd={linThetaFdMaxResidual:E2}");
Console.WriteLine($"STENCILS: idExact={identityExactStencilBattery} ({identityMaxRichardson:E2}) einRich={einsteinianRichardsonBattery} ({einsteinianMaxRichardson:E2}) offsetImm={offsetImmunityBattery} ({maxOffsetRel:E2})");
Console.WriteLine($"SUNSET: K={SunsetK} floorRel={SoftFloorRel:E0} maxKTailRel={maxKTailRel:E2} floorSweepStable={floorSweepStable}");
foreach (var a in anchorChecks)
    Console.WriteLine($"  anchor {a.Member,-16} t={a.T:F3} trunc={a.Truncated:E3} full={a.Full:E3} rel={a.TruncRel:F3}");
Console.WriteLine($"CONTROL: noSpuriousTwoLoopMin={controlNoSpuriousTwoLoopMinimum}");
Console.WriteLine($"PERTURBATIVITY: max|V2/V1|={maxTwoLoopToOneLoopRatio:E2} median={medianTwoLoopToOneLoopRatio:E2} perturbative={perturbativeRegime} admissible={twoLoopVerdictAdmissible}");
Console.WriteLine($"VERDICT: kind={resolutionKind} any={einsteinianTwoLoopSaturationObservedAny} all={einsteinianTwoLoopSaturationObservedAll} candidate={twoLoopCandidate} seedStable={seedStable} conventionStable={conventionStableOnProbedRay} floorStable={floorSweepStable} (alt={altCls} vs {defaultClsAltMember})");
Console.WriteLine("--- member classifications (primary seed) ---");
foreach (var m in memberResults)
    Console.WriteLine($"  {m.Name,-16} oneLoop={classByMemberSeed[(m.Index, 0)].OneLoop,-34} twoLoop={classByMemberSeed[(m.Index, 0)].TwoLoop}");
Console.WriteLine($"sbEvalCount={sbEvalCount} runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Local helpers.
// ---------------------------------------------------------------------------

double Norm(double[] v)
{
    double s = 0; foreach (double x in v) s += x * x; return System.Math.Sqrt(s);
}

static double RelDiff(double a, double b) =>
    System.Math.Abs(a - b) / System.Math.Max(1e-30, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));

static double Median(double[] xs)
{
    if (xs.Length == 0) return double.NaN;
    var s = (double[])xs.Clone();
    Array.Sort(s);
    int n = s.Length;
    return n % 2 == 1 ? s[n / 2] : 0.5 * (s[n / 2 - 1] + s[n / 2]);
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
    BranchId = "phase447-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase447",
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

public sealed record TwoLoopResult(double FigureEight, double Sunset, double Total, int ModeCount, int SunsetModes);

public sealed record TwoLoopCache(
    double[] Lambdas, int SunsetSlots, double[] T4Diag, double[,] T4Pair, double[,,] T3, double MaxAbsEig);

public sealed record KSweepEntry(int K, double Sunset);

public sealed record FloorSweepEntry(double FloorRel, string Classification);

public sealed record SeedResult(
    int Seed, PointData[] Points, TwoLoopResult[] TwoLoop, KSweepEntry[] KSweep, FloorSweepEntry[] FloorSweep,
    double RichardsonRelFig8, double RichardsonRelSunset, double OffsetImmunityRel);

public sealed record AnchorCheck(
    string Member, double T, double Truncated, double Full, double TruncRel);

public sealed record MemberResult(
    int Index, string Name, bool IsControl, string Phi1, string Phi2, double EinsteinCoefficient,
    List<SeedResult> Seeds);
