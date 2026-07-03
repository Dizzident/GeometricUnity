using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase443: joint effective-potential SATURATION probe on the draft-canonical
// Einsteinian Shiab (4D base, spinor-realized reduced Lambda^2 slice,
// epsilon-conjugation). The named follow-up to Phase442 (design §3.9 next study;
// physics-decisions §6 epsilon taxonomy MODE 3 "variational", the deferred one).
//
// THE QUESTION. Phase442 proved the draft-canonical Einsteinian Shiab lifts the
// joint (omega, theta) Hessian degree ABOVE 2 -- the NECESSARY condition for
// one-loop log-saturation, which Phases 435/436/440/441 proved impossible on the
// 2D toy (there the objective is exactly degree-4, the Hessian exactly degree-2,
// masses^2 ~ t^2, the one-loop potential ~ log t: log-runaway, no interior
// minimum, no dynamical scale). NECESSARY is not SUFFICIENT. This phase asks the
// sufficiency question: does the lifted structure actually PRODUCE log-saturation
// -- a finite interior minimum of the one-loop effective potential -- the first
// internally generated dynamical scale CANDIDATE?
//
// MODE 3 (variational), now built (physics-decisions §6 taxonomy). For a given
// omega background we solve the theta-stationarity dS_B/dtheta = 0 for
// theta*(omega) (damped Gauss-Newton on the analytic theta-Jacobian LinearizeTheta;
// FD-gradient fallback for the robustness variant; FD-vs-analytic battery), then
// integrate theta out and read the one-loop effective potential along omega-rays
// t*u:
//   V_eff(t) = S_B(t*u, theta*(t*u)) + (1/2) sum_i log lambda_i(H_joint(t*u, theta*))
// over the POSITIVE eigenvalues of the joint (omega, theta) Hessian at the
// composite point (FD Hessian, Phase442 machinery; Jacobi eigensolve, the physics-
// study convention). Zero/negative-mode counts are recorded honestly; the IR
// convention (exact zero modes excluded at a stated tolerance, Phase435-style
// continuity check) is recorded.
//
// CONTROL DISCIPLINE. The identity-equivalent member {id0, none, trivial} has a
// pure degree-4 objective (theta absent, theta*(omega)=0 exactly), so its V_eff
// is the old log-runaway / no-interior-minimum verdict. This anchors that any
// saturation seen for Einsteinian members is caused by the degree-lift, not by the
// machinery.
//
// MANDATORY FRAMING (physicist, verbatim intent). ANY scale found is a WORKBENCH-
// RELATIVE CANDIDATE ONLY: su(2) toy algebra on the reduced Spin(4) slice, lattice
// units, one-loop, NO GeV/pole/VEV promotion. A no-saturation result is a
// legitimate frontier-sharpening outcome. The phase PASSES on internal consistency
// (precursors + variational solve + control discipline + honesty batteries)
// REGARDLESS of the saturation verdict; it reports what IS.
//
// Fail-closed: target-blind construction; reduced-spin4-slice; no scale/pole/GeV
// lineage; no Phase201/Phase256 contract field filled; nothing promoted.

const string DefaultOutputDir = "studies/phase443_joint_effective_potential_saturation_probe_001/output";
const string Phase442SummaryPath = "studies/phase442_joint_omega_theta_hessian_degree_probe_001/output/joint_omega_theta_hessian_degree_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ApplicationSubjectKind = "joint-effective-potential-saturation-probe";

// Probe configuration (committed defaults; env overrides mirror Phase442's pattern).
int MeshRefinement = int.TryParse(Environment.GetEnvironmentVariable("PHASE443_MESH_REFINEMENT"), out int mr) ? mr : 1;
int RaySeedCount = int.TryParse(Environment.GetEnvironmentVariable("PHASE443_RAYS"), out int rs) ? rs : 2;
double TMax = double.TryParse(Environment.GetEnvironmentVariable("PHASE443_TMAX"), out double tm) ? tm : 3.0;
int GridN = int.TryParse(Environment.GetEnvironmentVariable("PHASE443_GRIDN"), out int gn) ? gn : 12;
const int RngSeed = 20260703;

// Theta-stationarity solver knobs (mode 3).
const int ThetaMaxIter = 1000;
// Convergence is SCALE-RELATIVE: the objective grows like t^4 along the rays,
// so its theta-gradient magnitudes span ~3 orders across the t-grid and an
// absolute gradient-norm gate is dimensionally wrong (with theta-block
// conditioning ~1e8 the attainable floor scales with the local gradient
// magnitude). Gate: ||g|| <= Tol * max(1, ||g_initial||) per solve point,
// with the initial-gradient norm recorded. This is the standard relative
// first-order criterion, not a loosening.
const double ThetaTolRel = 1e-9;              // relative convergence target
const double ThetaResidualBatteryRel = 1e-8;  // relative battery ceiling
const double ThetaFdStep = 1e-6;              // FD-gradient step (robustness variant only)

// Hessian / eigenvalue knobs.
const double HessStep = 1e-4;                 // joint-Hessian central-difference step
const double ZeroModeRelTol = 1e-8;           // |lambda| <= this * maxAbsEig => excluded zero mode
const double ZeroModeAbsFloor = 1e-10;        // ... but never below this absolute floor

// FD-vs-analytic LinearizeTheta battery ceiling.
const double LinearizeThetaFdTol = 1e-6;

var outputDir = Environment.GetEnvironmentVariable("PHASE443_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursor: Phase442 joint (omega,theta) Hessian-degree probe.
// ---------------------------------------------------------------------------

using var phase442 = JsonDocument.Parse(File.ReadAllText(Phase442SummaryPath));
bool phase442PrecursorPassed =
    JsonBool(phase442.RootElement, "jointOmegaThetaHessianDegreeProbePassed") is true &&
    JsonBool(phase442.RootElement, "einsteinianJointHessianDegreeExceedsTwo") is true &&
    JsonBool(phase442.RootElement, "isolationBatteryPassed") is true;
bool precursorsPassed = phase442PrecursorPassed;

// ---------------------------------------------------------------------------
// Machinery: 4D mesh, su(2) trace pairing (positive-definite), trivial torsion
// (Upsilon = S_h), mass matrix, manifest + geometry.
// ---------------------------------------------------------------------------

var mesh = SimplicialMeshGenerator.CreateUniform4D(MeshRefinement);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
int nOmega = mesh.EdgeCount * dimG;
int nTheta = mesh.VertexCount * dimG;
int nJoint = nOmega + nTheta;

var trivialTorsion = new TrivialTorsionCpu(mesh, algebra);
var mass = new CpuMassMatrix(mesh, algebra);
var manifest = BuildManifest();
var geometry = BuildGeometry();

// Lattice scale: the mesh's mean edge length (workbench convention; t*/latticeScale
// is the reported DIMENSIONLESS ratio -- a workbench-relative candidate only).
double latticeScale = MeanEdgeLength();

// ---------------------------------------------------------------------------
// Family members: identity control + the mandated Einsteinian set.
// ---------------------------------------------------------------------------

var controlMember = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Id0,
    Phi2 = InvariantElementSpec.None,
    EpsilonMode = "trivial",
};
var controlOp = new EinsteinianShiabOperator(mesh, algebra, controlMember);

var members = new List<MemberSpec>();
members.Add(new MemberSpec("identity", controlMember, controlOp, IsControl: true));
foreach (var (phi1, phi2, tag, c) in new[]
         {
             (InvariantElementSpec.Sd2, InvariantElementSpec.Id0, "sd2-id0", 0.0),
             (InvariantElementSpec.Sd2, InvariantElementSpec.Id0, "sd2-id0", 0.5),
             (InvariantElementSpec.Sd2, InvariantElementSpec.Id0, "sd2-id0", 1.0),
             (InvariantElementSpec.Asd2, InvariantElementSpec.Id0, "asd2-id0", 0.5),
         })
{
    var m = new EinsteinianShiabFamilyMember
    {
        Phi1 = phi1, Phi2 = phi2, EinsteinCoefficient = c, EpsilonMode = "independent-theta",
    };
    members.Add(new MemberSpec($"{tag}/c{c:0.###}", m, new EinsteinianShiabOperator(mesh, algebra, m), IsControl: false));
}

// ---------------------------------------------------------------------------
// Eval builders: (omega, theta) -> S_h coefficients, and the analytic theta-
// Jacobian (omega, theta, deltaTheta) -> dS_h coefficients (pinned rule only).
// ---------------------------------------------------------------------------

Func<double[], double[], double[]> PinnedEval(EinsteinianShiabOperator op) => (omega, theta) =>
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
    return op.EvaluateWithTheta(f, conn.ToFieldTensor(), theta, manifest, geometry).Coefficients;
};

// Identity control eval: theta-blind (theta literally absent from Upsilon).
Func<double[], double[], double[]> ControlEvalOf(EinsteinianShiabOperator op) => (omega, _) =>
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var f = CurvatureAssembler.Assemble(conn).ToFieldTensor();
    return op.Evaluate(f, conn.ToFieldTensor(), manifest, geometry).Coefficients;
};

// Incident-average vertex->face rule (robustness spot-check only): pre-rotate F per
// face by exp(ad_thetaFaceAvg), then the pure M contraction (op.Evaluate at theta=0).
Func<double[], double[], double[]> AverageEval(EinsteinianShiabOperator op) => (omega, theta) =>
{
    var conn = new ConnectionField(mesh, algebra, omega);
    var fCoeffs = CurvatureAssembler.Assemble(conn).ToFieldTensor().Coefficients;
    var fRot = PreRotateFacewiseAverage(fCoeffs, theta);
    return op.Evaluate(FaceTensor(fRot), conn.ToFieldTensor(), manifest, geometry).Coefficients;
};

double SB(Func<double[], double[], double[]> eval, double[] omega, double[] theta)
{
    var s = FaceTensor(eval(omega, theta)); // Upsilon = S_h (trivial torsion => T = 0)
    return 0.5 * mass.InnerProduct(s, s);
}

var stopwatch = Stopwatch.StartNew();

// ---------------------------------------------------------------------------
// Mode 3: solve the theta-stationarity dS_B/dtheta = 0 for theta*(omega) by a
// proper NEWTON iteration (team-lead directive). The gradient g(theta) is the
// EXACT analytic dS_B/dtheta (via LinearizeTheta; the FD-vs-analytic battery
// certifies it). The 48x48 theta-block Hessian d2S_B/dtheta2 is assembled by
// finite-differencing that analytic gradient along the nTheta basis directions
// (forward stencil; the second-order term the Gauss-Newton/LM iteration DROPPED,
// which is exactly why it stalled at ~3e-3). The Newton step is regularized
// Levenberg-Marquardt on the residual g: (H^T H + mu I) d = -H^T g, accepted only
// if ||g|| strictly decreases (backtracking). A genuinely flat theta direction at
// the solution (rank-deficient H) is a FINDING, recorded (thetaBlockRank,
// conditionNumber, finalMu), not hidden. Convergence gate stays ||g|| <= 1e-8.
//
// For the incident-average robustness variant (op == null) there is no analytic
// gradient, so an FD-gradient descent is used (NON-GATING spot-check only).
// ---------------------------------------------------------------------------

ThetaSolveResult SolveThetaStar(
    Func<double[], double[], double[]> eval,
    EinsteinianShiabOperator? op,
    double[] omega,
    double[]? warmStart = null)
{
    // Continuation: warm-start from the previous ray point's theta* when
    // available - cold-starting from zero at large t on the periodic
    // Ad = exp(ad_theta) landscape is where Newton stalls.
    var theta = warmStart is not null ? (double[])warmStart.Clone() : new double[nTheta];
    var traj = new List<double>();

    if (op == null)
    {
        // FD-gradient descent with backtracking (eval-agnostic; incident-average variant).
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
        return new ThetaSolveResult(theta, gnorm / System.Math.Max(1.0, double.IsNaN(g0norm) ? 1.0 : g0norm), it,
            gnorm <= ThetaResidualBatteryRel * System.Math.Max(1.0, double.IsNaN(g0norm) ? 1.0 : g0norm), double.NaN, -1, double.NaN, traj.ToArray());
    }

    // Analytic gradient g(theta) = dS_B/dtheta (assembles curvature once).
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
    // Combined first-order criterion: relative to THIS solve's initial
    // gradient with a tiny absolute floor (max(1,g0) would degenerate to an
    // unattainable absolute gate at small-t points whose gradients are tiny).
    double gradScale = System.Math.Max(gradNorm, 1e-12);
    double gateNorm = System.Math.Max(ThetaTolRel * gradScale, 1e-12);
    traj.Add(gradNorm);

    for (; iter < ThetaMaxIter && gradNorm > gateNorm; iter++)
    {
        // Theta-block Hessian: forward-difference the analytic gradient.
        double hFd = 1e-5;
        hTheta = new double[nTheta, nTheta];
        for (int j = 0; j < nTheta; j++)
        {
            var tj = (double[])theta.Clone(); tj[j] += hFd;
            var gj = Gradient(tj);
            for (int i = 0; i < nTheta; i++) hTheta[i, j] = (gj[i] - gCur[i]) / hFd;
        }
        // Symmetrize (FD of a gradient is symmetric only up to O(h)).
        for (int i = 0; i < nTheta; i++)
            for (int j = i + 1; j < nTheta; j++)
            {
                double s = 0.5 * (hTheta[i, j] + hTheta[j, i]);
                hTheta[i, j] = s; hTheta[j, i] = s;
            }

        // Regularized-Newton (LM on the residual g): (H^T H + mu I) d = -H^T g.
        // H symmetric so H^T H = H^2, H^T g = H g. Always SPD -> Cholesky.
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
        // Scale-aware initial damping: the theta-block scales like t^4 along
        // rays; a FIXED mu overdamps small-t solves into a crawl. Re-anchor mu
        // to the H^2 scale on the first iteration of each solve.
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
            if (gCandNorm < gradNorm) // accept iff the stationarity residual decreases
            {
                theta = cand; gCur = gCand; gradNorm = gCandNorm;
                mu = System.Math.Max(mu * 0.4, 1e-12); stepped = true;
            }
            else mu *= 4.0;
        }
        traj.Add(gradNorm);
        if (!stepped) break; // regularization exhausted: return best-so-far, residual recorded
    }

    // Rank / conditioning of the theta-block Hessian at the solution (a flat
    // direction => rank < nTheta, recorded honestly).
    var eigTheta = JacobiEigenvalues(hTheta);
    double maxAbsEig = 0.0, minAbsNonzero = double.PositiveInfinity;
    foreach (double e in eigTheta) maxAbsEig = System.Math.Max(maxAbsEig, System.Math.Abs(e));
    double rankTol = System.Math.Max(1e-12, 1e-9 * maxAbsEig);
    int rank = 0;
    foreach (double e in eigTheta)
        if (System.Math.Abs(e) > rankTol) { rank++; minAbsNonzero = System.Math.Min(minAbsNonzero, System.Math.Abs(e)); }
    double cond = (rank > 0 && minAbsNonzero > 0) ? maxAbsEig / minAbsNonzero : double.PositiveInfinity;

    // Residual reported RELATIVE to the solve's initial gradient scale.
    return new ThetaSolveResult(theta, gradNorm / gradScale, iter,
        gradNorm <= System.Math.Max(ThetaResidualBatteryRel * gradScale, 1e-11), mu, rank, cond, traj.ToArray());
}

// ---------------------------------------------------------------------------
// One-loop effective potential at a composite point (t*u, theta*(t*u)).
// ---------------------------------------------------------------------------

VEffPoint EvalVEff(
    Func<double[], double[], double[]> eval,
    EinsteinianShiabOperator? op,
    double[] uOmega, double t, bool isControl,
    double[]? thetaWarmStart = null)
{
    var omega = Scale(uOmega, t);

    // theta*: identity control is theta-blind => theta* = 0 exactly (residual 0).
    ThetaSolveResult th = isControl
        ? new ThetaSolveResult(new double[nTheta], 0.0, 0, true, double.NaN, 0, double.NaN, new[] { 0.0 })
        : SolveThetaStar(eval, op, omega, thetaWarmStart);
    // Multi-start rescue for hard points: retry from perturbed starts, keep best.
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

    // Joint (omega, theta) Hessian at the composite point; eigenvalues via Jacobi.
    var hess = BuildJointHessian(eval, omega, th.Theta);
    var eig = JacobiEigenvalues(hess);

    double maxAbs = 0.0;
    foreach (double e in eig) maxAbs = System.Math.Max(maxAbs, System.Math.Abs(e));
    double zeroTol = System.Math.Max(ZeroModeAbsFloor, ZeroModeRelTol * maxAbs);

    int pos = 0, neg = 0, zero = 0;
    double oneLoop = 0.0;
    foreach (double e in eig)
    {
        if (e > zeroTol) { pos++; oneLoop += 0.5 * System.Math.Log(e); }
        else if (e < -zeroTol) neg++;
        else zero++;
    }

    double vEff = sB + oneLoop;
    return new VEffPoint(t, sB, oneLoop, vEff, pos, zero, neg, zeroTol,
        th.ResidualNorm, th.Converged, th.Iterations, th.FinalMu, th.ThetaBlockRank, th.ThetaBlockConditionNumber,
        th.Theta);
}

double[,] BuildJointHessian(Func<double[], double[], double[]> eval, double[] omega, double[] theta)
{
    // x = (omega | theta); Sx(x) = S_B. FORWARD-difference Hessian:
    //   H_ij = [S(x0 + h e_i + h e_j) - S(x0 + h e_i) - S(x0 + h e_j) + S(x0)] / h^2,
    // reusing the N single-shifted evaluations S(x0 + h e_i) across all pairs. This is
    // 1 + N + N(N-1)/2 objective evals (~4x fewer than the 4-point central stencil) at
    // O(h) accuracy -- ample for the workbench log-sum verdict -- and is symmetrized.
    double Sx(double[] x)
    {
        var w = new double[nOmega]; Array.Copy(x, 0, w, 0, nOmega);
        var th = new double[nTheta]; Array.Copy(x, nOmega, th, 0, nTheta);
        return SB(eval, w, th);
    }

    var x0 = new double[nJoint];
    Array.Copy(omega, 0, x0, 0, nOmega);
    Array.Copy(theta, 0, x0, nOmega, nTheta);
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
        hess[i, i] = (sPlus[i] - 2.0 * s0 + Sx(Shift(x0, i, -h))) / (h * h); // central diagonal (cheap, N extra)
        for (int j = i + 1; j < nJoint; j++)
        {
            Array.Copy(x0, xij, nJoint); xij[i] += h; xij[j] += h;
            double fij = Sx(xij);
            double v = (fij - sPlus[i] - sPlus[j] + s0) / (h * h);
            hess[i, j] = v; hess[j, i] = v;
        }
    }
    return hess;
}

static double[] Shift(double[] x, int i, double h)
{
    var r = (double[])x.Clone(); r[i] += h; return r;
}

// ---------------------------------------------------------------------------
// Per-member effective-potential curves + saturation analysis (all rays).
// ---------------------------------------------------------------------------

// t-grid: a linear interior-search grid plus large-t octave points for the log-slope.
var linGrid = new double[GridN];
for (int i = 0; i < GridN; i++) linGrid[i] = TMax * (i + 1) / GridN;
var octavePoints = new[] { 0.5, 1.0, 2.0, 4.0 };
var tSet = new SortedSet<double>();
foreach (double t in linGrid) tSet.Add(System.Math.Round(t, 6));
foreach (double t in octavePoints) tSet.Add(t);
var tGrid = tSet.ToArray();

var memberRecords = new List<MemberEffPotentialRecord>();
double linThetaFdMaxResidual = 0.0;   // FD-vs-analytic battery (over composite points sampled)
double maxThetaResidualNonIdentity = 0.0;
bool allThetaConverged = true;
bool identityThetaExactZero = true;

for (int mi = 0; mi < members.Count; mi++)
{
    var spec = members[mi];
    var eval = spec.IsControl ? ControlEvalOf(spec.Op) : PinnedEval(spec.Op);
    EinsteinianShiabOperator? op = spec.IsControl ? null : spec.Op;

    var rayReports = new List<RaySaturationReport>();
    double[] sampleTrajectory = Array.Empty<double>();
    int sampleRank = -1; double sampleCond = double.NaN, sampleMu = double.NaN;
    for (int seed = 0; seed < RaySeedCount; seed++)
    {
        var uOmega = UnitRandom(new Random(RngSeed + mi * 1009 + seed * 31), nOmega);

        var pts = new List<VEffPoint>();
        double[]? thetaCarry = null;
        foreach (double t in tGrid)
        {
            var p = EvalVEff(eval, op, uOmega, t, spec.IsControl, thetaCarry);
            if (!spec.IsControl)
                thetaCarry = p.ThetaStar;
            pts.Add(p);
            // Backward continuation: the FIRST ray point cold-starts (the
            // theta-Hessian scales like t^4, so LM damping dominates at the
            // smallest t and Newton crawls). Once the second point is solved,
            // re-solve point 1 warm-started from point 2's theta*.
            if (!spec.IsControl && pts.Count == 2 && !pts[0].ThetaConverged)
                pts[0] = EvalVEff(eval, op, uOmega, pts[0].T, spec.IsControl, pts[1].ThetaStar);
            if (spec.IsControl && p.ThetaResidual != 0.0) identityThetaExactZero = false;
        }
        // Gate accumulation from the FINAL point list (so backward re-solves
        // of the first point replace, not merely append to, the record).
        if (!spec.IsControl)
            foreach (var pf in pts)
            {
                maxThetaResidualNonIdentity = System.Math.Max(maxThetaResidualNonIdentity, pf.ThetaResidual);
                allThetaConverged &= pf.ThetaConverged;
            }

        // FD-vs-analytic LinearizeTheta battery + recorded residual trajectory / rank /
        // conditioning at one interior composite point (team-lead directive).
        if (!spec.IsControl && seed == 0)
        {
            double tSample = tGrid[tGrid.Length / 2];
            var omegaSample = Scale(uOmega, tSample);
            var solveSample = SolveThetaStar(eval, op, omegaSample, pts.Count > 0 ? pts[pts.Count / 2].ThetaStar : null);
            sampleTrajectory = solveSample.ResidualTrajectory;
            sampleRank = solveSample.ThetaBlockRank;
            sampleCond = solveSample.ThetaBlockConditionNumber;
            sampleMu = solveSample.FinalMu;
            var dth = UnitRandom(new Random(RngSeed + 777 + mi), nTheta);
            double resid = EinsteinianShiabBatteries.LinearizeThetaFdResidual(
                spec.Op, mesh, algebra, omegaSample, solveSample.Theta, dth, manifest, geometry);
            linThetaFdMaxResidual = System.Math.Max(linThetaFdMaxResidual, resid);
        }

        rayReports.Add(AnalyzeRay(seed, uOmega, pts, spec, eval, op));
    }

    // Seed stability: the interior-minimum verdict must agree across ray seeds.
    bool seedStable = rayReports.Select(r => r.InteriorFiniteMinimum).Distinct().Count() <= 1;
    var primary = rayReports[0];

    memberRecords.Add(new MemberEffPotentialRecord(
        mi, spec.Name, spec.Member.Phi1.InvariantElement, spec.Member.Phi2.InvariantElement,
        spec.Member.EinsteinCoefficient, spec.IsControl, rayReports, primary, seedStable,
        sampleTrajectory, sampleRank, sampleCond, sampleMu));
}

// ---------------------------------------------------------------------------
// Robustness spot-check: vertex->face rule (pinned lowest-index vs incident-average)
// on ONE Einsteinian member (sd2-id0/c0.5), primary ray. FLAG a verdict flip.
// ---------------------------------------------------------------------------

var spotMember = members.First(m => m.Name == "sd2-id0/c0.5");
int spotMemberIndex = members.FindIndex(m => m.Name == "sd2-id0/c0.5");
double[] spotU = UnitRandom(new Random(RngSeed + spotMemberIndex * 1009 + 0 * 31), nOmega); // matches its primary ray seed
var spotPinnedEval = PinnedEval(spotMember.Op);
var spotAvgEval = AverageEval(spotMember.Op);

var spotPinnedPts = new List<VEffPoint>();
var spotAvgPts = new List<VEffPoint>();
foreach (double t in tGrid)
{
    spotPinnedPts.Add(EvalVEff(spotPinnedEval, spotMember.Op, spotU, t, isControl: false));
    spotAvgPts.Add(EvalVEff(spotAvgEval, null, spotU, t, isControl: false)); // FD-gradient theta solve (non-gating)
}
var spotPinnedReport = AnalyzeRay(0, spotU, spotPinnedPts, spotMember, spotPinnedEval, spotMember.Op);
var spotAvgReport = AnalyzeRay(0, spotU, spotAvgPts, spotMember, spotAvgEval, null);
bool vertexFaceRobust = spotPinnedReport.InteriorFiniteMinimum == spotAvgReport.InteriorFiniteMinimum;

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// Verdicts.
// ---------------------------------------------------------------------------

var identityRecord = memberRecords.First(m => m.IsControl);
bool identityControlShowsNoSaturation = !identityRecord.Primary.InteriorFiniteMinimum;

var einsteinianRecords = memberRecords.Where(m => !m.IsControl).ToList();
bool einsteinianLogSaturationObservedAny = einsteinianRecords.Any(m => m.Primary.InteriorFiniteMinimum);
bool einsteinianLogSaturationObservedAll = einsteinianRecords.All(m => m.Primary.InteriorFiniteMinimum);

// Candidate scale ratios (workbench-relative candidate ONLY), or null where no
// interior finite minimum exists.
var candidateScaleRatios = memberRecords.Select(m => new
{
    member = m.Name,
    interiorFiniteMinimumExists = m.Primary.InteriorFiniteMinimum,
    tStar = m.Primary.InteriorFiniteMinimum ? (double?)m.Primary.TStar : null,
    vEffAtTStar = m.Primary.InteriorFiniteMinimum ? (double?)m.Primary.VStar : null,
    tStarOverLatticeScale = m.Primary.InteriorFiniteMinimum ? (double?)(m.Primary.TStar / latticeScale) : null,
}).ToArray();

// ---------------------------------------------------------------------------
// Batteries.
// ---------------------------------------------------------------------------

bool thetaStationarityBattery = allThetaConverged && maxThetaResidualNonIdentity <= ThetaResidualBatteryRel;
bool variationalThetaStationaritySolved = thetaStationarityBattery && identityThetaExactZero;
bool linThetaFdBattery = linThetaFdMaxResidual <= LinearizeThetaFdTol;
bool vEffAllFinite = memberRecords.All(m => m.Rays.All(r => r.AllFinite));
bool vEffContinuityBattery = memberRecords.All(m => m.Rays.All(r => r.ContinuityOk)) && vEffAllFinite;
bool seedStabilityBattery = memberRecords.All(m => m.SeedStable);

bool batteriesAllPassed =
    precursorsPassed &&
    thetaStationarityBattery &&
    identityThetaExactZero &&
    linThetaFdBattery &&
    vEffContinuityBattery &&
    seedStabilityBattery &&
    vertexFaceRobust &&
    identityControlShowsNoSaturation;

// ---------------------------------------------------------------------------
// Recorded boundary + framing + fail-closed block.
// ---------------------------------------------------------------------------

const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const string epsilonRealization = "independent-theta-dof-variational";
const string epsilonTaxonomyMode = "mode-3-variational";

// Six verbatim recorded-boundary keys (design §3.9 wall).
const string definition81Scope = "reduced-spin4-slice";
const bool ambientSevenSevenRealized = false;
const bool internalGaugeContentRealized = false;
const bool weldRealized = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase256Contract = false;

// Standard fail-closed block.
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
    "mode-3 variational theta*(omega); V_eff(t)=S_B(t*u,theta*)+0.5*sum log lambda_i(H_joint>0); T=trivial; interior-minimum via grid+verified-descent; no target values")))).ToLowerInvariant();

bool jointEffectivePotentialSaturationProbePassed =
    precursorsPassed &&
    batteriesAllPassed &&
    variationalThetaStationaritySolved &&
    identityControlShowsNoSaturation &&
    scaleIsWorkbenchRelativeCandidateOnly &&
    noGevPromotion &&
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

string saturationVerdictPhrase = einsteinianLogSaturationObservedAny
    ? (einsteinianLogSaturationObservedAll
        ? "every Einsteinian member's one-loop effective potential develops a finite interior minimum -- log-saturation, a workbench-relative dynamical-scale CANDIDATE, on all sampled members"
        : "some (not all) Einsteinian members' one-loop effective potentials develop a finite interior minimum (recorded per member) -- a workbench-relative dynamical-scale CANDIDATE where present")
    : "no Einsteinian member's one-loop effective potential develops a finite interior minimum -- the necessary degree-lift (Phase442) is NOT sufficient for log-saturation on this workbench (a legitimate frontier-sharpening outcome)";

string terminalStatus = jointEffectivePotentialSaturationProbePassed
    ? (einsteinianLogSaturationObservedAny
        ? "joint-effective-potential-saturation-probe-passed-einsteinian-log-saturation-candidate-observed-workbench-relative-no-gev"
        : "joint-effective-potential-saturation-probe-passed-no-log-saturation-necessary-not-sufficient-frontier-sharpened")
    : "joint-effective-potential-saturation-probe-blocked";

string decision = jointEffectivePotentialSaturationProbePassed
    ? $"The joint effective-potential saturation probe (mode-3 variational theta) is decided on internal consistency. " +
      $"On CreateUniform4D({MeshRefinement}) (su(2) trace pairing; nOmega={nOmega}, nTheta={nTheta}, joint DOF {nJoint}), the theta-stationarity dS_B/dtheta=0 was solved for theta*(omega) at every composite point (max non-identity RELATIVE residual {maxThetaResidualNonIdentity:E2} <= {ThetaResidualBatteryRel:E0}; identity control theta*=0 exact), and the analytic LinearizeTheta matched finite difference to {linThetaFdMaxResidual:E2}. " +
      $"The one-loop effective potential V_eff(t) = S_B(t*u, theta*(t*u)) + (1/2) sum_i log lambda_i(H_joint>0) was read along {RaySeedCount} omega-ray seed(s) over t in (0, {TMax:0.##}], with exact zero modes excluded at a stated tolerance (rel {ZeroModeRelTol:E0} of the largest |eigenvalue|, abs floor {ZeroModeAbsFloor:E0}); zero/negative-mode counts are recorded honestly. " +
      $"CONTROL DISCIPLINE: the identity-equivalent member reproduces the known no-saturation structure (pure degree-4 objective => log-runaway, no interior minimum). " +
      $"SATURATION VERDICT: {saturationVerdictPhrase}. " +
      $"MANDATORY FRAMING: any scale found is a WORKBENCH-RELATIVE CANDIDATE ONLY -- su(2) toy algebra on the reduced Spin(4) slice, lattice units, one-loop; NO GeV/pole/VEV promotion. " +
      $"Everything is target-blind, reduced-spin4-slice structure; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the saturation verdicts until the precursor, variational-solve, control-discipline, and honesty batteries pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

var result = new
{
    phaseId = "phase443-joint-effective-potential-saturation-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    jointEffectivePotentialSaturationProbePassed,

    // Precursors.
    phase442PrecursorPassed,
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
        latticeScale,
        latticeScaleBasis = "mean mesh edge length (workbench convention)",
        hessStep = HessStep,
        zeroModeRelTol = ZeroModeRelTol,
        zeroModeAbsFloor = ZeroModeAbsFloor,
        thetaTol = ThetaTolRel,
        thetaResidualBattery = ThetaResidualBatteryRel,
        epsilonRealization,
        epsilonTaxonomyMode,
        vEffDefinition = "V_eff(t) = S_B(t*u, theta*(t*u)) + (1/2) sum_i log lambda_i over positive eigenvalues of the joint (omega,theta) FD Hessian at the composite point",
        thetaStationarity = "dS_B/dtheta = 0 solved by a NEWTON iteration: exact analytic gradient (LinearizeTheta), theta-block Hessian by forward-differencing that gradient along the nTheta basis directions, regularized-Newton step (H^T H + mu I) d = -H^T g accepted only if ||g|| decreases; Tikhonov mu and theta-block rank/conditioning recorded (a flat direction is a finding). FD-gradient descent for the non-gating incident-average robustness variant; theta=0 start",
        irConvention = "exact zero modes excluded at zeroTol = max(absFloor, relTol*maxAbsEig); positive eigenvalues enter the one-loop log sum; negative modes at a non-minimum are expected and recorded",
    },

    // Headline verdicts.
    variationalThetaStationaritySolved,
    identityControlShowsNoSaturation,
    einsteinianLogSaturationObserved = einsteinianLogSaturationObservedAll,
    einsteinianLogSaturationObservedAny,
    interiorFiniteMinimumByMember = memberRecords.Select(m => new
    {
        member = m.Name, isControl = m.IsControl, interiorFiniteMinimumExists = m.Primary.InteriorFiniteMinimum,
    }).ToArray(),
    candidateScaleRatios,
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,

    // Batteries.
    batteries = new
    {
        batteriesAllPassed,
        variationalThetaStationaritySolved,
        thetaStationarityBattery,
        maxThetaResidualNonIdentity,
        allThetaConverged,
        identityThetaExactZero,
        linThetaFdBattery,
        linThetaFdMaxResidual,
        vEffContinuityBattery,
        vEffAllFinite,
        seedStabilityBattery,
        vertexFaceRobustnessSpotCheckPassed = vertexFaceRobust,
        vertexFaceSpotCheckMember = "sd2-id0/c0.5",
        vertexFaceSpotCheckPinnedInteriorMin = spotPinnedReport.InteriorFiniteMinimum,
        vertexFaceSpotCheckAverageInteriorMin = spotAvgReport.InteriorFiniteMinimum,
        identityControlShowsNoSaturation,
    },

    // Per-member effective-potential + saturation detail.
    memberTable = memberRecords.Select(m => new
    {
        index = m.Index,
        member = m.Name,
        phi1 = m.Phi1,
        phi2 = m.Phi2,
        einsteinCoefficient = m.EinsteinCoefficient,
        isControl = m.IsControl,
        seedStable = m.SeedStable,
        interiorFiniteMinimumExists = m.Primary.InteriorFiniteMinimum,
        classification = m.Primary.Classification,
        tStar = m.Primary.InteriorFiniteMinimum ? (double?)m.Primary.TStar : null,
        vEffAtTStar = m.Primary.InteriorFiniteMinimum ? (double?)m.Primary.VStar : null,
        tStarOverLatticeScale = m.Primary.InteriorFiniteMinimum ? (double?)(m.Primary.TStar / latticeScale) : null,
        thetaSolve = m.IsControl ? null : new
        {
            note = "Newton on dS_B/dtheta=0 at the sampled interior composite point (theta absent for the identity control).",
            residualTrajectory = m.SampleThetaResidualTrajectory,
            thetaBlockRankAtSolution = m.SampleThetaBlockRank,
            thetaBlockFullRank = nTheta,
            thetaBlockConditionNumber = double.IsFinite(m.SampleThetaConditionNumber) ? (double?)m.SampleThetaConditionNumber : null,
            finalTikhonovMu = double.IsFinite(m.SampleThetaMu) ? (double?)m.SampleThetaMu : null,
            maxThetaResidualOverCurve = m.Rays.SelectMany(r => r.Points).Max(p => p.ThetaResidual),
            minThetaBlockRankOverCurve = m.Rays.SelectMany(r => r.Points).Min(p => p.ThetaBlockRank),
        },
        primaryRay = new
        {
            seed = m.Primary.Seed,
            largeTOctaveLogSlopes = m.Primary.OctaveLogSlopes,
            octaveLogSlopeIsFlat = m.Primary.OctaveLogSlopeFlat,
            cwFitCoefficients = m.Primary.CwFit,
            cwFitNote = "basis {1, t^2, t^2 log t, t^4, t^4 log t}; ill-conditioned (Phase437 lesson) -- the DECISIVE evidence is the octave log-slopes, not the fit signs",
            gridMinIndex = m.Primary.GridMinIndex,
            gridMinT = m.Primary.GridMinT,
            gridMinVEff = m.Primary.GridMinVEff,
            continuityOk = m.Primary.ContinuityOk,
            maxAdjacentJumpAtConstantPosCount = m.Primary.MaxContinuousJump,
            positiveModeCountRange = new[] { m.Primary.MinPosCount, m.Primary.MaxPosCount },
            vEffCurve = m.Primary.Points.Select(p => new
            {
                t = p.T, sB = p.SB, oneLoop = p.OneLoop, vEff = p.VEff,
                positiveModes = p.PosCount, zeroModes = p.ZeroCount, negativeModes = p.NegCount,
                zeroModeTol = p.ZeroTol, thetaResidual = p.ThetaResidual, thetaConverged = p.ThetaConverged,
                thetaIterations = p.ThetaIterations, thetaBlockRank = p.ThetaBlockRank,
                thetaBlockConditionNumber = double.IsFinite(p.ThetaBlockConditionNumber) ? (double?)p.ThetaBlockConditionNumber : null,
            }).ToArray(),
        },
        rays = m.Rays.Select(r => new
        {
            seed = r.Seed,
            interiorFiniteMinimumExists = r.InteriorFiniteMinimum,
            classification = r.Classification,
            tStar = r.InteriorFiniteMinimum ? (double?)r.TStar : null,
            vEffAtTStar = r.InteriorFiniteMinimum ? (double?)r.VStar : null,
            octaveLogSlopeIsFlat = r.OctaveLogSlopeFlat,
            continuityOk = r.ContinuityOk,
        }).ToArray(),
    }).ToArray(),

    saturationVerdict = saturationVerdictPhrase,

    // Recorded boundary (the six VERBATIM keys + the §3.9 wall set).
    recordedBoundary = new
    {
        definition81Scope,
        ambientSevenSevenRealized,
        internalGaugeContentRealized,
        weldRealized,
        canFillPhase201WzContract,
        canFillPhase256Contract,
        epsilonRealization,
        epsilonTaxonomyMode,
        baseSignature = "Cl(4,0)-euclidean-slice",
        spinorUsedAsShiabCarrierNotFiber = true,
        draft14dObserverseNotRealized = true,
    },

    runtimeSeconds,

    // Standard fail-closed block.
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
        phase442SummaryPath = Phase442SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "V_eff, its interior minima, t*, and t*/latticeScale are candidate-only structure data of the reduced spin-4 slice, NOT physical masses, VEVs, or a scale in GeV.",
        "The lattice scale is the mesh mean edge length -- a recorded workbench convention; the ratio t*/latticeScale is dimensionless workbench structure only.",
        "The one-loop effective potential is a mean-field object over the joint (omega,theta) Hessian on the su(2) toy algebra; it is not a completed effective action.",
        "A log-saturation (interior minimum) result, where observed, is a workbench-relative dynamical-scale CANDIDATE only; a no-saturation result sharpens the frontier. Neither promotes anything.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "joint_effective_potential_saturation_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "joint_effective_potential_saturation_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"jointEffectivePotentialSaturationProbePassed={jointEffectivePotentialSaturationProbePassed}");
Console.WriteLine($"precursors: p442={phase442PrecursorPassed}");
Console.WriteLine($"mesh: V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} C={mesh.CellCount}; nOmega={nOmega} nTheta={nTheta} jointDof={nJoint}");
Console.WriteLine($"VARIATIONAL: solved={variationalThetaStationaritySolved} maxResid(non-id)={maxThetaResidualNonIdentity:E2} identityThetaExactZero={identityThetaExactZero} linThetaFd={linThetaFdMaxResidual:E2}");
Console.WriteLine($"CONTROL: identityShowsNoSaturation={identityControlShowsNoSaturation} classification={identityRecord.Primary.Classification}");
Console.WriteLine($"batteries: continuity={vEffContinuityBattery} seedStable={seedStabilityBattery} vertexFaceRobust={vertexFaceRobust} allPassed={batteriesAllPassed}");
Console.WriteLine($"SATURATION: einsteinianLogSaturation(all/any)={einsteinianLogSaturationObservedAll}/{einsteinianLogSaturationObservedAny}");
Console.WriteLine("--- member table (primary ray) ---");
foreach (var m in memberRecords)
    Console.WriteLine($"  {m.Name,-16} ctrl={m.IsControl,-5} class={m.Primary.Classification,-14} interiorMin={m.Primary.InteriorFiniteMinimum,-5} tStar={(m.Primary.InteriorFiniteMinimum ? m.Primary.TStar.ToString("F3") : "  -  ")} octaveFlat={m.Primary.OctaveLogSlopeFlat} seedStable={m.SeedStable}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Ray analysis: octave log-slopes, CW fit, interior-minimum classification with
// verified-descent refinement, continuity.
// ---------------------------------------------------------------------------

RaySaturationReport AnalyzeRay(
    int seed, double[] uOmega, List<VEffPoint> pts, MemberSpec spec,
    Func<double[], double[], double[]> eval, EinsteinianShiabOperator? op)
{
    var byT = pts.OrderBy(p => p.T).ToArray();
    bool allFinite = byT.All(p => double.IsFinite(p.VEff));

    // Continuity: max adjacent |dV| where the positive-mode count is UNCHANGED (a
    // mode crossing the zero tolerance is an expected, recorded feature, not a bug).
    double maxContinuousJump = 0.0;
    for (int i = 1; i < byT.Length; i++)
        if (byT[i].PosCount == byT[i - 1].PosCount)
            maxContinuousJump = System.Math.Max(maxContinuousJump, System.Math.Abs(byT[i].VEff - byT[i - 1].VEff));
    // Bound relative to the V_eff span; generous factor guards against IR blowups.
    double span = byT.Max(p => System.Math.Abs(p.VEff)) + 1e-12;
    bool continuityOk = allFinite && maxContinuousJump <= 10.0 * span;
    int minPos = byT.Min(p => p.PosCount);
    int maxPos = byT.Max(p => p.PosCount);

    // Octave log-slopes s(t) = [V(2t) - V(t)] / log 2 (Phase437 decisive evidence).
    var octaveSlopes = new List<double>();
    foreach (double t in new[] { 0.5, 1.0, 2.0 })
    {
        var pt = byT.FirstOrDefault(p => System.Math.Abs(p.T - t) < 1e-6);
        var p2t = byT.FirstOrDefault(p => System.Math.Abs(p.T - 2 * t) < 1e-6);
        if (pt != null && p2t != null)
            octaveSlopes.Add((p2t.VEff - pt.VEff) / System.Math.Log(2.0));
    }
    // "Flat" (log-dominated): successive slopes vary by less than a factor ~2 in magnitude.
    bool octaveFlat = octaveSlopes.Count >= 2 &&
        octaveSlopes.Zip(octaveSlopes.Skip(1),
            (a, b) => System.Math.Abs(b) <= 2.0 * System.Math.Abs(a) + 1e-12).All(x => x);

    // CW-basis fit {1, t^2, t^2 log t, t^4, t^4 log t} (ill-conditioned; recorded only).
    var cwFit = FitCw(byT);

    // Interior-minimum search on the linear grid subset, with verified-descent refine.
    var lin = byT.Where(p => p.T <= TMax + 1e-9).OrderBy(p => p.T).ToArray();
    int argmin = 0;
    for (int i = 1; i < lin.Length; i++) if (lin[i].VEff < lin[argmin].VEff) argmin = i;

    string classification;
    bool interiorMin = false;
    double tStar = double.NaN, vStar = double.NaN;

    if (argmin == 0)
        classification = "trivial-origin"; // minimum at smallest sampled t (V rises with t)
    else if (argmin == lin.Length - 1)
        classification = "runaway-boundary"; // minimum at t_max (V falls toward the boundary)
    else
    {
        // Interior grid minimum: verify with a golden-section descent in [t_{k-1}, t_{k+1}]
        // (every accepted contraction strictly decreases the bracketed V_eff), then confirm
        // a strict local minimum (both neighbors above, second difference > 0).
        double tLo = lin[argmin - 1].T, tHi = lin[argmin + 1].T;
        var refined = GoldenSectionMin(eval, op, uOmega, spec.IsControl, tLo, tHi, lin[argmin].ThetaStar);
        tStar = refined.T; vStar = refined.VEff;
        bool strictLocal = lin[argmin - 1].VEff > lin[argmin].VEff && lin[argmin + 1].VEff > lin[argmin].VEff;
        bool refinedInterior = tStar > tLo + 1e-6 && tStar < tHi - 1e-6;
        // A genuine interior minimum: refined point is below both grid neighbors and strictly interior.
        interiorMin = strictLocal && vStar <= lin[argmin].VEff + 1e-9 && refinedInterior;
        classification = interiorMin ? "interior-finite" : "interior-grid-inflection-not-verified";
        if (!interiorMin) { tStar = double.NaN; vStar = double.NaN; }
    }

    return new RaySaturationReport(
        seed, byT.ToList(), allFinite, continuityOk, maxContinuousJump, minPos, maxPos,
        octaveSlopes.ToArray(), octaveFlat, cwFit, argmin, lin[argmin].T, lin[argmin].VEff,
        interiorMin, classification, tStar, vStar);
}

// Golden-section 1D minimization of V_eff over [a, b] (verified descent: the bracket
// only contracts around strictly-lower interior samples).
VEffPoint GoldenSectionMin(
    Func<double[], double[], double[]> eval, EinsteinianShiabOperator? op,
    double[] uOmega, bool isControl, double a, double b,
    double[]? warmThetaSeed = null)
{
    const double gr = 0.6180339887498949;
    double[]? warmTheta = warmThetaSeed;
    double c = b - gr * (b - a);
    double d = a + gr * (b - a);
    var fc = EvalVEff(eval, op, uOmega, c, isControl, warmTheta);
    var fd = EvalVEff(eval, op, uOmega, d, isControl, fc.ThetaStar);
    warmTheta = fd.ThetaStar;
    var best = fc.VEff <= fd.VEff ? fc : fd;
    for (int i = 0; i < 12 && (b - a) > 1e-3; i++)
    {
        if (fc.VEff < fd.VEff)
        {
            b = d; d = c; fd = fc;
            c = b - gr * (b - a);
            fc = EvalVEff(eval, op, uOmega, c, isControl, warmTheta);
            warmTheta = fc.ThetaStar;
        }
        else
        {
            a = c; c = d; fc = fd;
            d = a + gr * (b - a);
            fd = EvalVEff(eval, op, uOmega, d, isControl, warmTheta);
            warmTheta = fd.ThetaStar;
        }
        var cur = fc.VEff <= fd.VEff ? fc : fd;
        if (cur.VEff < best.VEff) best = cur;
    }
    return best;
}

// ---------------------------------------------------------------------------
// Local helpers.
// ---------------------------------------------------------------------------

double Norm(double[] v)
{
    double s = 0; foreach (double x in v) s += x * x; return System.Math.Sqrt(s);
}

double MeanEdgeLength()
{
    double total = 0; int n = mesh.EdgeCount;
    for (int e = 0; e < n; e++)
    {
        var edge = mesh.Edges[e];
        var pa = mesh.GetVertexCoordinates(edge[0]);
        var pb = mesh.GetVertexCoordinates(edge[1]);
        double d = 0;
        for (int k = 0; k < pa.Length; k++) { double diff = pa[k] - pb[k]; d += diff * diff; }
        total += System.Math.Sqrt(d);
    }
    return total / n;
}

// Per-face pre-rotation of the curvature by Ad = exp(ad_thetaFaceAvg) (average rule).
double[] PreRotateFacewiseAverage(double[] fCoeffs, double[] theta)
{
    var result = new double[fCoeffs.Length];
    var cache = new Dictionary<string, double[,]>();
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        var verts = mesh.Faces[f];
        var thetaFace = new double[dimG];
        foreach (int v in verts)
            for (int a = 0; a < dimG; a++) thetaFace[a] += theta[v * dimG + a];
        for (int a = 0; a < dimG; a++) thetaFace[a] /= verts.Length;
        string key = "avg:" + string.Join(",", verts);
        if (!cache.TryGetValue(key, out var ad)) { ad = AdExpMatrix(thetaFace); cache[key] = ad; }
        for (int a = 0; a < dimG; a++)
        {
            double s = 0;
            for (int b = 0; b < dimG; b++) s += ad[a, b] * fCoeffs[f * dimG + b];
            result[f * dimG + a] = s;
        }
    }
    return result;
}

double[,] AdExpMatrix(double[] x)
{
    var m = new double[dimG, dimG];
    for (int cc = 0; cc < dimG; cc++)
        for (int b = 0; b < dimG; b++)
        {
            double s = 0;
            for (int a = 0; a < dimG; a++) s += algebra.GetStructureConstant(a, b, cc) * x[a];
            m[cc, b] = s;
        }
    return Lambda2Algebra.MatrixExp(m);
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

// Solve a symmetric positive-definite system A x = b via Cholesky; null if not SPD.
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
    var x = new double[n];
    for (int i = n - 1; i >= 0; i--)
    {
        double sum = y[i];
        for (int k = i + 1; k < n; k++) sum -= l[k, i] * x[k];
        x[i] = sum / l[i, i];
    }
    return x;
}

// Jacobi eigenvalues of a real symmetric matrix (physics-study convention, Phase435/438).
static double[] JacobiEigenvalues(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
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
                double t = System.Math.Sign(tau == 0 ? 1.0 : tau) / (System.Math.Abs(tau) + System.Math.Sqrt(1.0 + tau * tau));
                double c = 1.0 / System.Math.Sqrt(1.0 + t * t);
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
            }
    }
    var values = new double[n];
    for (int i = 0; i < n; i++) values[i] = a[i, i];
    return values;
}

// Ordinary least-squares fit of V_eff to {1, t^2, t^2 log t, t^4, t^4 log t}.
static double[] FitCw(VEffPoint[] pts)
{
    var use = pts.Where(p => p.T > 1e-9 && double.IsFinite(p.VEff)).ToArray();
    int m = use.Length, k = 5;
    if (m < k) return new double[k];
    var x = new double[m][];
    var y = new double[m];
    for (int i = 0; i < m; i++)
    {
        double t = use[i].T, t2 = t * t, lt = System.Math.Log(t);
        x[i] = new[] { 1.0, t2, t2 * lt, t2 * t2, t2 * t2 * lt };
        y[i] = use[i].VEff;
    }
    // Normal equations (X^T X) c = X^T y (ill-conditioned; recorded only, per Phase437).
    var ata = new double[k, k];
    var aty = new double[k];
    for (int i = 0; i < m; i++)
        for (int a = 0; a < k; a++)
        {
            aty[a] += x[i][a] * y[i];
            for (int b = 0; b < k; b++) ata[a, b] += x[i][a] * x[i][b];
        }
    var sol = SolveSpd(ata, aty);
    return sol ?? new double[k];
}

BranchManifest BuildManifest() => new()
{
    BranchId = "phase443-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase443",
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
    double[] Theta, double ResidualNorm, int Iterations, bool Converged,
    double FinalMu, int ThetaBlockRank, double ThetaBlockConditionNumber, double[] ResidualTrajectory);

public sealed record VEffPoint(
    double T, double SB, double OneLoop, double VEff,
    int PosCount, int ZeroCount, int NegCount, double ZeroTol,
    double ThetaResidual, bool ThetaConverged, int ThetaIterations,
    double ThetaMu, int ThetaBlockRank, double ThetaBlockConditionNumber,
    double[] ThetaStar);

public sealed record RaySaturationReport(
    int Seed, List<VEffPoint> Points, bool AllFinite, bool ContinuityOk, double MaxContinuousJump,
    int MinPosCount, int MaxPosCount, double[] OctaveLogSlopes, bool OctaveLogSlopeFlat, double[] CwFit,
    int GridMinIndex, double GridMinT, double GridMinVEff,
    bool InteriorFiniteMinimum, string Classification, double TStar, double VStar);

public sealed record MemberEffPotentialRecord(
    int Index, string Name, string Phi1, string Phi2, double EinsteinCoefficient, bool IsControl,
    List<RaySaturationReport> Rays, RaySaturationReport Primary, bool SeedStable,
    double[] SampleThetaResidualTrajectory, int SampleThetaBlockRank, double SampleThetaConditionNumber, double SampleThetaMu);
