using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase445: RG-IMPROVED joint effective-potential probe on the draft-canonical
// Einsteinian Shiab (4D base, spinor-realized reduced Lambda^2 slice, epsilon-
// conjugation). The named NO-PLATFORM alternative to Phase444 (mode volume was
// tooling-blocked) and the RG-improvement follow-up to Phase443.
//
// THE QUESTION. Phase443 found the ONE-LOOP joint effective potential shows NO
// saturation on the minimal mesh (trivial-origin for every Einsteinian member):
// the fixed-coupling one-loop landscape has no interior minimum. The standard
// Coleman-Weinberg mechanism, however, lives in the RG-IMPROVED potential: the
// one-loop logs are resummed into a RUNNING effective coupling, and a minimum
// forms where the running balances the tree term -- a STRUCTURALLY DIFFERENT
// question. Does RG improvement of the MEASURED joint one-loop structure produce
// saturation (an interior minimum) that the unimproved potential lacks?
//
// CONSTRUCTION (honest workbench version -- every convention recorded).
//   (0) Recompute the Phase443-style V_eff(t) rays with the HARDENED machinery
//       (verbatim Newton theta*-stationarity solver, joint FD Hessian, Jacobi
//       eigenvalues, IR zero-mode convention) on CreateUniform4D(1).
//       V_eff(t) = S_B(t*u, theta*(t*u)) + (1/2) sum_i log lambda_i(H_joint>0).
//   (1) Extract the scale-dependent EFFECTIVE QUARTIC. The tree action S_B is an
//       exact quartic polynomial in the ray parameter t; fit S_B locally to
//       {t^2, t^3, t^4} over a sliding window -> lambda_tree(t) (its t^4 coeff)
//       plus the MEASURED SUBLEADING terms (t^2, t^3). Fit the one-loop correction
//       V_1loop(t) = V_eff - S_B locally to c(t)*t^4 (the leading-log resummation
//       target) -> delta_lambda(t). lambda_eff(t) = lambda_tree(t) + delta_lambda(t).
//       The workbench beta function is beta(t) = d lambda_eff / d ln t (central
//       difference over window centers) -- MEASURED from the potential, not assumed.
//   (2) RG-improve: V_RG(t) = lambda_eff(t)*t^4 + [measured subleading a*t^2+b*t^3].
//   (3) SATURATION analysis on V_RG per member: interior-minimum search with
//       verified descent (strict local minimum: both window neighbours strictly
//       above), classification {trivial-origin / interior-finite / runaway /
//       interior-inflection-not-verified}; if interior, the dimensionless
//       t*/latticeScale ratio recorded CANDIDATE-ONLY.
//   (4) CONTROL DISCIPLINE. Identity control {id0, none, trivial}: theta absent,
//       theta*=0 exact, S_B pure quartic, one-loop POSITIVE (old no-go: masses^2
//       ~ t^2, one-loop ~ log t). Its lambda_tree must be window-CONSTANT (pure
//       quartic) up to the one-loop log slope, and its V_RG must NOT develop a
//       spurious interior minimum. A spurious control minimum means the RG scheme
//       manufactures artifacts -- it KILLS the treatment verdicts and blocks the
//       phase (recorded honestly).
//   (5) SCHEME HONESTY. The window width, fit basis, and resummation prescription
//       are WORKBENCH CONVENTIONS. Scan the window width (3 values) and require the
//       verdict be scheme-stable (verdictSchemeStable). A scheme-dependent verdict
//       is recorded as exactly that. Per-window fit-quality (uncentered R^2 for the
//       through-origin one-loop t^4 fit; centered R^2 for the tree fit) is recorded.
//
// MANDATORY FRAMING. Any scale is a WORKBENCH-RELATIVE CANDIDATE ONLY: su(2) toy
// algebra on the reduced Spin(4) slice, lattice units, one loop, RG prescription a
// workbench convention pending physicist review. NO GeV/pole/VEV promotion. The
// phase PASSES on internal consistency (precursors + hardened theta solve + control
// discipline + honesty batteries) REGARDLESS of the saturation verdict.
//
// Fail-closed: target-blind; reduced-spin4-slice; no scale/pole/GeV lineage; no
// Phase201/Phase256 contract field filled; nothing promoted either way.

const string DefaultOutputDir = "studies/phase445_rg_improved_joint_potential_probe_001/output";
const string Phase443SummaryPath = "studies/phase443_joint_effective_potential_saturation_probe_001/output/joint_effective_potential_saturation_probe_summary.json";
const string Phase444SummaryPath = "studies/phase444_mode_volume_scaled_saturation_probe_001/output/mode_volume_scaled_saturation_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ApplicationSubjectKind = "rg-improved-joint-potential-probe";

// Probe configuration (committed defaults; env overrides mirror Phase443's pattern).
int MeshRefinement = int.TryParse(Environment.GetEnvironmentVariable("PHASE445_MESH_REFINEMENT"), out int mr) ? mr : 1;
int RaySeedCount = int.TryParse(Environment.GetEnvironmentVariable("PHASE445_RAYS"), out int rs) ? rs : 2;
double TMax = double.TryParse(Environment.GetEnvironmentVariable("PHASE445_TMAX"), out double tm) ? tm : 3.0;
int GridN = int.TryParse(Environment.GetEnvironmentVariable("PHASE445_GRIDN"), out int gn) ? gn : 16;
const int RngSeed = 20260703;

// RG-improvement scheme scan: sliding-window widths (odd) over the uniform t-grid.
int[] WindowWidths = { 3, 5, 7 };

// Theta-stationarity solver knobs (mode 3) -- VERBATIM from Phase443 (hardened solver).
const int ThetaMaxIter = 1000;
const double ThetaTolRel = 1e-9;              // relative convergence target
const double ThetaResidualBatteryRel = 1e-8;  // relative battery ceiling (reused verbatim)
const double ThetaFdStep = 1e-6;

// Hessian / eigenvalue knobs (verbatim from Phase443).
const double HessStep = 1e-4;
const double ZeroModeRelTol = 1e-8;
const double ZeroModeAbsFloor = 1e-10;

// FD-vs-analytic LinearizeTheta battery ceiling.
const double LinearizeThetaFdTol = 1e-6;

// Control / fit-quality thresholds (workbench conventions; recorded).
const double IdentityLamTreeRelVariationTol = 1e-3; // identity tree quartic coeff ~ window-constant
const double FitR2UncenteredFloor = 0.10;           // soft floor on the through-origin one-loop fit

var outputDir = Environment.GetEnvironmentVariable("PHASE445_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors: Phase443 (no one-loop saturation) and Phase444 (mode-volume study).
// ---------------------------------------------------------------------------

using var phase443 = JsonDocument.Parse(File.ReadAllText(Phase443SummaryPath));
bool phase443PrecursorPassed =
    JsonBool(phase443.RootElement, "jointEffectivePotentialSaturationProbePassed") is true &&
    JsonBool(phase443.RootElement, "einsteinianLogSaturationObserved") is false;

using var phase444 = JsonDocument.Parse(File.ReadAllText(Phase444SummaryPath));
bool phase444PrecursorPassed = JsonBool(phase444.RootElement, "phase444Passed") is true;

bool precursorsPassed = phase443PrecursorPassed && phase444PrecursorPassed;

// ---------------------------------------------------------------------------
// Machinery (verbatim Phase443): 4D mesh, su(2) trace pairing, trivial torsion.
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

// ---------------------------------------------------------------------------
// Members: identity control + the mandated Einsteinian set (Phase445 subset).
// ---------------------------------------------------------------------------

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

// ---------------------------------------------------------------------------
// Eval builders (verbatim Phase443).
// ---------------------------------------------------------------------------

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
    var s = FaceTensor(eval(omega, theta)); // Upsilon = S_h (trivial torsion => T = 0)
    return 0.5 * mass.InnerProduct(s, s);
}

var stopwatch = Stopwatch.StartNew();

// ---------------------------------------------------------------------------
// Mode 3 theta*-stationarity solve (VERBATIM hardened Newton from Phase443).
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
        return new ThetaSolveResult(theta, gnorm / g0, it, gnorm <= ThetaResidualBatteryRel * g0,
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

    var eigTheta = JacobiEigenvalues(hTheta);
    double maxAbsEig = 0.0, minAbsNonzero = double.PositiveInfinity;
    foreach (double e in eigTheta) maxAbsEig = System.Math.Max(maxAbsEig, System.Math.Abs(e));
    double rankTol = System.Math.Max(1e-12, 1e-9 * maxAbsEig);
    int rank = 0;
    foreach (double e in eigTheta)
        if (System.Math.Abs(e) > rankTol) { rank++; minAbsNonzero = System.Math.Min(minAbsNonzero, System.Math.Abs(e)); }
    double cond = (rank > 0 && minAbsNonzero > 0) ? maxAbsEig / minAbsNonzero : double.PositiveInfinity;

    return new ThetaSolveResult(theta, gradNorm / gradScale, iter,
        gradNorm <= System.Math.Max(ThetaResidualBatteryRel * gradScale, 1e-11), mu, rank, cond, traj.ToArray());
}

// ---------------------------------------------------------------------------
// One-loop effective potential at a composite point (verbatim Phase443).
// ---------------------------------------------------------------------------

VEffPoint EvalVEff(
    Func<double[], double[], double[]> eval,
    EinsteinianShiabOperator? op,
    double[] uOmega, double t, bool isControl,
    double[]? thetaWarmStart = null)
{
    var omega = Scale(uOmega, t);

    ThetaSolveResult th = isControl
        ? new ThetaSolveResult(new double[nTheta], 0.0, 0, true, double.NaN, 0, double.NaN, new[] { 0.0 })
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
        th.ResidualNorm, th.Converged, th.Iterations, th.ThetaBlockRank, th.Theta);
}

double[,] BuildJointHessian(Func<double[], double[], double[]> eval, double[] omega, double[] theta)
{
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
        hess[i, i] = (sPlus[i] - 2.0 * s0 + Sx(Shift(x0, i, -h))) / (h * h);
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
// Recompute the V_eff(t) rays (the object Phase443 built; the RG input here).
// ---------------------------------------------------------------------------

// Uniform t-grid over (0, TMax] (the RG sliding windows require a uniform grid so
// the window-width scan is apples-to-apples across schemes).
var tGrid = new double[GridN];
for (int i = 0; i < GridN; i++) tGrid[i] = TMax * (i + 1) / GridN;

double linThetaFdMaxResidual = 0.0;
double maxThetaResidualNonIdentity = 0.0;
bool allThetaConverged = true;
bool identityThetaExactZero = true;

var memberCurves = new List<MemberCurves>();
for (int mi = 0; mi < members.Count; mi++)
{
    var spec = members[mi];
    var eval = spec.IsControl ? ControlEvalOf(spec.Op) : PinnedEval(spec.Op);
    EinsteinianShiabOperator? op = spec.IsControl ? null : spec.Op;

    var seedCurves = new List<VEffPoint[]>();
    for (int seed = 0; seed < RaySeedCount; seed++)
    {
        var uOmega = UnitRandom(new Random(RngSeed + mi * 1009 + seed * 31), nOmega);
        var pts = new List<VEffPoint>();
        double[]? thetaCarry = null;
        foreach (double t in tGrid)
        {
            var p = EvalVEff(eval, op, uOmega, t, spec.IsControl, thetaCarry);
            if (!spec.IsControl) thetaCarry = p.ThetaStar;
            pts.Add(p);
            if (spec.IsControl && p.ThetaResidual != 0.0) identityThetaExactZero = false;
        }
        // Backward continuation of the first (cold-started) point (verbatim Phase443).
        if (!spec.IsControl && pts.Count >= 2 && !pts[0].ThetaConverged)
            pts[0] = EvalVEff(eval, op, uOmega, pts[0].T, spec.IsControl, pts[1].ThetaStar);

        if (!spec.IsControl)
            foreach (var pf in pts)
            {
                maxThetaResidualNonIdentity = System.Math.Max(maxThetaResidualNonIdentity, pf.ThetaResidual);
                allThetaConverged &= pf.ThetaConverged;
            }

        // FD-vs-analytic LinearizeTheta battery at one interior composite point.
        if (!spec.IsControl && seed == 0)
        {
            int mid = pts.Count / 2;
            var omegaSample = Scale(uOmega, tGrid[mid]);
            var dth = UnitRandom(new Random(RngSeed + 777 + mi), nTheta);
            double resid = EinsteinianShiabBatteries.LinearizeThetaFdResidual(
                spec.Op, mesh, algebra, omegaSample, pts[mid].ThetaStar, dth, manifest, geometry);
            linThetaFdMaxResidual = System.Math.Max(linThetaFdMaxResidual, resid);
        }

        seedCurves.Add(pts.ToArray());
    }
    memberCurves.Add(new MemberCurves(mi, spec.Name, spec.IsControl,
        spec.Member.Phi1.InvariantElement, spec.Member.Phi2.InvariantElement, spec.Member.EinsteinCoefficient, seedCurves));
}

// ---------------------------------------------------------------------------
// RG-improvement analysis (post-processing on the recomputed V_eff rays).
// ---------------------------------------------------------------------------

var lnT = tGrid.Select(x => System.Math.Log(x)).ToArray();

RgSchemeResult RgAnalyze(VEffPoint[] curve, int w)
{
    int half = w / 2;
    var centers = new List<int>();
    var lamTree = new List<double>();
    var subQuad = new List<double>();   // a: t^2 coeff
    var subCube = new List<double>();   // b: t^3 coeff
    var dLam = new List<double>();      // one-loop t^4 coeff
    var lamEff = new List<double>();
    var r2Tree = new List<double>();
    var r2OneLoopCentered = new List<double>();
    var r2OneLoopUncentered = new List<double>();

    for (int k = half; k < curve.Length - half; k++)
    {
        int lo = k - half, hi = k + half;
        int m = hi - lo + 1;

        // Tree fit: S_B ~ a t^2 + b t^3 + c t^4  (S_B is an exact quartic in t).
        var At = new double[m][];
        var yt = new double[m];
        for (int r = 0; r < m; r++)
        {
            double x = curve[lo + r].T, x2 = x * x;
            At[r] = new[] { x2, x2 * x, x2 * x2 };
            yt[r] = curve[lo + r].SB;
        }
        var (ct, r2t, _) = LeastSquares(At, yt);

        // One-loop correction fit: V_1loop = V_eff - S_B ~ c*t^4 (leading-log resummation).
        var Ao = new double[m][];
        var yo = new double[m];
        for (int r = 0; r < m; r++)
        {
            double x = curve[lo + r].T;
            Ao[r] = new[] { x * x * x * x };
            yo[r] = curve[lo + r].OneLoop;
        }
        var (co, r2oC, r2oU) = LeastSquares(Ao, yo);

        centers.Add(k);
        lamTree.Add(ct[2]); subQuad.Add(ct[0]); subCube.Add(ct[1]);
        dLam.Add(co[0]);
        lamEff.Add(ct[2] + co[0]);
        r2Tree.Add(r2t); r2OneLoopCentered.Add(r2oC); r2OneLoopUncentered.Add(r2oU);
    }

    // Beta function beta(t) = d lambda_eff / d ln t (central difference over centers).
    int nc = centers.Count;
    var beta = new double[nc];
    for (int j = 0; j < nc; j++)
    {
        if (j == 0 || j == nc - 1) { beta[j] = double.NaN; continue; }
        beta[j] = (lamEff[j + 1] - lamEff[j - 1]) / (lnT[centers[j + 1]] - lnT[centers[j - 1]]);
    }

    // RG-improved potential V_RG(t) = lambda_eff(t) t^4 + [measured subleading a t^2 + b t^3].
    var vrg = new double[nc];
    var tcen = new double[nc];
    for (int j = 0; j < nc; j++)
    {
        double x = curve[centers[j]].T;
        tcen[j] = x;
        vrg[j] = lamEff[j] * x * x * x * x + subQuad[j] * x * x + subCube[j] * x * x * x;
    }

    // Saturation classification with verified descent (strict local minimum).
    string cls; double tStar = double.NaN, vStar = double.NaN;
    if (nc < 3) cls = "insufficient-window-centers";
    else
    {
        int argmin = 0;
        for (int j = 1; j < nc; j++) if (vrg[j] < vrg[argmin]) argmin = j;
        if (argmin == 0) cls = "trivial-origin";
        else if (argmin == nc - 1) cls = "runaway";
        else if (vrg[argmin - 1] > vrg[argmin] && vrg[argmin + 1] > vrg[argmin])
        { cls = "interior-finite"; tStar = tcen[argmin]; vStar = vrg[argmin]; }
        else cls = "interior-inflection-not-verified";
    }

    double lamTreeRelVar = lamTree.Count > 0
        ? (lamTree.Max() - lamTree.Min()) / System.Math.Max(1e-30, System.Math.Abs(lamTree.Average()))
        : double.NaN;

    return new RgSchemeResult(w, centers.ToArray(), tcen,
        lamTree.ToArray(), subQuad.ToArray(), subCube.ToArray(), dLam.ToArray(), lamEff.ToArray(),
        beta, vrg, r2Tree.ToArray(), r2OneLoopCentered.ToArray(), r2OneLoopUncentered.ToArray(),
        cls, tStar, vStar, lamTreeRelVar);
}

var memberRg = new List<MemberRgRecord>();
foreach (var mc in memberCurves)
{
    var perSeed = new List<Dictionary<int, RgSchemeResult>>();
    foreach (var curve in mc.SeedCurves)
    {
        var byWidth = new Dictionary<int, RgSchemeResult>();
        foreach (int w in WindowWidths) byWidth[w] = RgAnalyze(curve, w);
        perSeed.Add(byWidth);
    }

    // Primary ray = seed 0. Scheme stability across window widths (primary seed).
    var primary = perSeed[0];
    bool schemeStable = primary.Values.Select(r => r.Classification).Distinct().Count() == 1;
    // Seed stability: classification agrees across seeds at every width.
    bool seedStable = true;
    foreach (int w in WindowWidths)
        if (perSeed.Select(s => s[w].Classification).Distinct().Count() != 1) seedStable = false;

    // Identity one-loop log slope (the OLD no-go signature): mean d(oneLoop)/d ln t.
    double oneLoopLogSlope = double.NaN;
    if (mc.IsControl)
    {
        var c0 = mc.SeedCurves[0];
        double acc = 0; int cnt = 0;
        for (int k = 1; k < c0.Length; k++)
        {
            acc += (c0[k].OneLoop - c0[k - 1].OneLoop) / (lnT[k] - lnT[k - 1]); cnt++;
        }
        oneLoopLogSlope = cnt > 0 ? acc / cnt : double.NaN;
    }

    memberRg.Add(new MemberRgRecord(mc.Index, mc.Name, mc.IsControl, mc.Phi1, mc.Phi2, mc.EinsteinCoefficient,
        perSeed, primary, schemeStable, seedStable, oneLoopLogSlope));
}

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// Batteries + verdicts.
// ---------------------------------------------------------------------------

var identityRg = memberRg.First(m => m.IsControl);
var einsteinianRg = memberRg.Where(m => !m.IsControl).ToList();

// Theta*-stationarity gate (reused verbatim, relative <= 1e-8).
bool thetaStationarityGate = allThetaConverged && maxThetaResidualNonIdentity <= ThetaResidualBatteryRel && identityThetaExactZero;
bool linThetaFdBattery = linThetaFdMaxResidual <= LinearizeThetaFdTol;

// Identity control beta consistency: pure-quartic tree => lambda_tree window-constant
// (small relative variation) at EVERY scheme; quantified. And lambda_eff stays positive
// (its one-loop is positive), so no sign flip that would fake a minimum.
double identityMaxLamTreeRelVar = identityRg.Primary.Values.Max(r => r.LamTreeRelativeVariation);
double identityMinLamEff = identityRg.Primary.Values.Min(r => r.LambdaEff.Min());
bool identityBetaConsistent = identityMaxLamTreeRelVar <= IdentityLamTreeRelVariationTol && identityMinLamEff > 0.0;

// No spurious control minimum: identity must NOT classify interior-finite at any
// scheme/seed. A spurious control minimum kills the treatment verdicts AND blocks.
bool controlNoSpuriousMinimum = memberRg.Where(m => m.IsControl)
    .All(m => m.AllSchemes.All(s => s.Values.All(r => r.Classification != "interior-finite")));

// Fit-quality floor (recorded; soft): the through-origin one-loop t^4 fit uncentered R^2.
double minOneLoopR2Uncentered = memberRg
    .SelectMany(m => m.AllSchemes.SelectMany(s => s.Values.SelectMany(r => r.R2OneLoopUncentered)))
    .DefaultIfEmpty(double.NaN).Min();
bool fitQualityFloorMet = double.IsFinite(minOneLoopR2Uncentered) && minOneLoopR2Uncentered >= FitR2UncenteredFloor;

// Scheme / seed stability across Einsteinian members (primary seed for scheme).
bool verdictSchemeStable = einsteinianRg.All(m => m.SchemeStable);
bool verdictSeedStable = einsteinianRg.All(m => m.SeedStable);

// SATURATION verdict (candidate-only, does NOT gate the pass).
bool einsteinianRgSaturationObservedAll = einsteinianRg.Count > 0 &&
    einsteinianRg.All(m => m.Primary.Values.All(r => r.Classification == "interior-finite"));
bool einsteinianRgSaturationObservedAny = einsteinianRg.Any(m => m.Primary.Values.Any(r => r.Classification == "interior-finite"));

bool batteriesAllPassed =
    precursorsPassed &&
    thetaStationarityGate &&
    linThetaFdBattery &&
    identityBetaConsistent &&
    controlNoSpuriousMinimum;

// ---------------------------------------------------------------------------
// Framing + recorded boundary + fail-closed block (verbatim keys).
// ---------------------------------------------------------------------------

const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool rgPrescriptionIsWorkbenchConvention = true;
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
    "recompute V_eff(t)=S_B(t*u,theta*)+0.5*sum log lambda_i(H_joint>0); RG: lambda_tree from S_B~{t2,t3,t4}, delta_lambda from V_1loop~c*t^4, lambda_eff=sum, beta=d lambda_eff/d ln t; V_RG=lambda_eff*t^4+subleading; window-width scan {3,5,7}; no target values")))).ToLowerInvariant();

bool rgImprovedJointPotentialProbePassed =
    precursorsPassed &&
    batteriesAllPassed &&
    scaleIsWorkbenchRelativeCandidateOnly &&
    noGevPromotion &&
    rgPrescriptionIsWorkbenchConvention &&
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

// Saturation verdict phrase, honestly conditioned on scheme stability + control.
string saturationVerdictPhrase;
if (!controlNoSpuriousMinimum)
    saturationVerdictPhrase = "the identity control developed a SPURIOUS interior minimum under RG improvement -- the workbench RG scheme manufactures artifacts on this mesh, so no treatment saturation verdict is admissible (recorded)";
else if (einsteinianRgSaturationObservedAll && verdictSchemeStable && verdictSeedStable)
    saturationVerdictPhrase = "RG improvement of the measured joint one-loop structure produces a scheme-stable, seed-stable interior minimum of V_RG for every Einsteinian member -- a workbench-relative dynamical-scale CANDIDATE (log-saturation) that the unimproved one-loop potential (Phase443) lacked, with the identity control staying non-saturating";
else if (einsteinianRgSaturationObservedAny && !verdictSchemeStable)
    saturationVerdictPhrase = "RG improvement produces an interior minimum for some Einsteinian schemes but the classification is SCHEME-DEPENDENT (window-width sensitive) -- recorded as scheme-dependent, not a stable candidate";
else if (einsteinianRgSaturationObservedAny)
    saturationVerdictPhrase = "RG improvement produces an interior minimum for some (not all) Einsteinian members (recorded per member) -- a workbench-relative CANDIDATE where present, control non-saturating";
else
    saturationVerdictPhrase = "RG improvement of the measured joint one-loop structure does NOT produce an interior minimum for the Einsteinian members on this workbench -- a legitimate frontier-sharpening outcome, control non-saturating";

string terminalStatus = rgImprovedJointPotentialProbePassed
    ? (controlNoSpuriousMinimum && einsteinianRgSaturationObservedAll && verdictSchemeStable && verdictSeedStable
        ? "rg-improved-joint-potential-probe-passed-einsteinian-rg-saturation-candidate-scheme-stable-workbench-relative-no-gev"
        : einsteinianRgSaturationObservedAny
            ? "rg-improved-joint-potential-probe-passed-einsteinian-rg-saturation-candidate-scheme-dependent-recorded-no-gev"
            : "rg-improved-joint-potential-probe-passed-no-rg-saturation-frontier-sharpened")
    : "rg-improved-joint-potential-probe-blocked";

string decision = rgImprovedJointPotentialProbePassed
    ? $"The RG-improved joint effective-potential probe is decided on internal consistency. On CreateUniform4D({MeshRefinement}) (su(2) trace pairing; nOmega={nOmega}, nTheta={nTheta}, joint DOF {nJoint}) the Phase443-style V_eff(t) rays were recomputed with the hardened Newton theta*-stationarity solver (max non-identity RELATIVE residual {maxThetaResidualNonIdentity:E2} <= {ThetaResidualBatteryRel:E0}; identity theta*=0 exact; analytic LinearizeTheta matched FD to {linThetaFdMaxResidual:E2}). " +
      $"The workbench beta function was MEASURED from the potential (lambda_tree from S_B~{{t^2,t^3,t^4}}, delta_lambda from the one-loop correction ~c*t^4, lambda_eff=sum, beta=d lambda_eff/d ln t) and V_RG(t)=lambda_eff(t)*t^4+subleading built per member over window widths {{{string.Join(",", WindowWidths)}}}. " +
      $"CONTROL DISCIPLINE: the identity control's tree quartic is window-constant (max rel variation {identityMaxLamTreeRelVar:E2}) with lambda_eff>0 (min {identityMinLamEff:E2}); it develops NO spurious interior minimum. " +
      $"SATURATION VERDICT: {saturationVerdictPhrase}. " +
      $"MANDATORY FRAMING: any scale is a WORKBENCH-RELATIVE CANDIDATE ONLY (su(2) toy algebra, reduced Spin(4) slice, lattice units, one loop); the RG prescription is a WORKBENCH CONVENTION pending physicist review; NO GeV/pole/VEV promotion. Everything is target-blind, reduced-spin4-slice; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the saturation verdicts until the precursor, theta-stationarity, control-discipline, and honesty batteries pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

object MemberJson(MemberRgRecord m) => new
{
    index = m.Index,
    member = m.Name,
    phi1 = m.Phi1,
    phi2 = m.Phi2,
    einsteinCoefficient = m.EinsteinCoefficient,
    isControl = m.IsControl,
    schemeStable = m.SchemeStable,
    seedStable = m.SeedStable,
    identityOneLoopLogSlope = m.IsControl && double.IsFinite(m.OneLoopLogSlope) ? (double?)m.OneLoopLogSlope : null,
    primaryRayByWindow = WindowWidths.Select(w =>
    {
        var r = m.Primary[w];
        return new
        {
            windowWidth = w,
            classification = r.Classification,
            interiorFiniteMinimum = r.Classification == "interior-finite",
            tStar = r.Classification == "interior-finite" ? (double?)r.TStar : null,
            vRgAtTStar = r.Classification == "interior-finite" ? (double?)r.VStar : null,
            tStarOverLatticeScale = r.Classification == "interior-finite" ? (double?)(r.TStar / latticeScale) : null,
            lamTreeRelativeVariation = r.LamTreeRelativeVariation,
            minR2OneLoopUncentered = r.R2OneLoopUncentered.DefaultIfEmpty(double.NaN).Min(),
            minR2OneLoopCentered = r.R2OneLoopCentered.DefaultIfEmpty(double.NaN).Min(),
            minR2Tree = r.R2Tree.DefaultIfEmpty(double.NaN).Min(),
            tCenters = r.TCenters,
            lambdaTree = r.LambdaTree,
            deltaLambdaOneLoop = r.DeltaLambda,
            lambdaEff = r.LambdaEff,
            betaFunction = r.Beta.Select(b => double.IsFinite(b) ? (double?)b : null).ToArray(),
            vRg = r.Vrg,
        };
    }).ToArray(),
    seeds = m.AllSchemes.Select((byW, si) => new
    {
        seed = si,
        byWindow = WindowWidths.Select(w => new
        {
            windowWidth = w,
            classification = byW[w].Classification,
            interiorFiniteMinimum = byW[w].Classification == "interior-finite",
            tStar = byW[w].Classification == "interior-finite" ? (double?)byW[w].TStar : null,
        }).ToArray(),
    }).ToArray(),
};

var result = new
{
    phaseId = "phase445-rg-improved-joint-potential-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    rgImprovedJointPotentialProbePassed,

    // Precursors.
    phase443PrecursorPassed,
    phase444PrecursorPassed,
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
        windowWidths = WindowWidths,
        latticeScale,
        latticeScaleBasis = "mean mesh edge length (workbench convention)",
        hessStep = HessStep,
        zeroModeRelTol = ZeroModeRelTol,
        zeroModeAbsFloor = ZeroModeAbsFloor,
        thetaTol = ThetaTolRel,
        thetaResidualBattery = ThetaResidualBatteryRel,
        epsilonRealization,
        epsilonTaxonomyMode,
        vEffDefinition = "V_eff(t) = S_B(t*u, theta*(t*u)) + (1/2) sum_i log lambda_i over positive eigenvalues of the joint (omega,theta) FD Hessian at the composite point (recomputed with the Phase443 hardened machinery)",
        thetaStationarity = "dS_B/dtheta = 0 solved by the VERBATIM Phase443 hardened Newton iteration (exact analytic gradient via LinearizeTheta; theta-block Hessian by forward-differencing that gradient; regularized-Newton step accepted only if ||g|| decreases); relative gate <= 1e-8; identity theta*=0 exact",
        rgPrescription = "WORKBENCH CONVENTION: lambda_tree(t) = local t^4 coefficient of S_B fit to {t^2,t^3,t^4}; delta_lambda(t) = local t^4 coefficient of the one-loop correction V_1loop=V_eff-S_B fit to c*t^4 (leading-log resummation target); lambda_eff = lambda_tree + delta_lambda; beta(t) = d lambda_eff / d ln t (central difference over window centers); V_RG(t) = lambda_eff(t)*t^4 + measured subleading (a t^2 + b t^3). Window width scanned over {3,5,7}",
        irConvention = "exact zero modes excluded at zeroTol = max(absFloor, relTol*maxAbsEig); positive eigenvalues enter the one-loop log sum; negative modes recorded",
    },

    // Headline verdicts.
    einsteinianRgSaturationObserved = einsteinianRgSaturationObservedAll,
    einsteinianRgSaturationObservedAny,
    verdictSchemeStable,
    verdictSeedStable,
    controlNoSpuriousMinimum,
    identityBetaConsistent,
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    rgPrescriptionIsWorkbenchConvention,
    physicistReviewPending,

    candidateScaleRatios = memberRg.Where(m => !m.IsControl).Select(m =>
    {
        var r = m.Primary[WindowWidths[0]];
        bool interior = m.Primary.Values.All(v => v.Classification == "interior-finite") && m.SchemeStable;
        // Report the median-window t* when scheme-stable interior; else candidate range.
        var interiorStars = m.Primary.Values.Where(v => v.Classification == "interior-finite").Select(v => v.TStar).ToArray();
        double? tStar = interiorStars.Length > 0 ? (double?)interiorStars.Average() : null;
        return new
        {
            member = m.Name,
            interiorFiniteMinimumSchemeStable = interior,
            tStarMeanOverSchemes = tStar,
            tStarOverLatticeScale = tStar.HasValue ? (double?)(tStar.Value / latticeScale) : null,
            perSchemeTStar = WindowWidths.Select(w => new { windowWidth = w, classification = m.Primary[w].Classification,
                tStar = m.Primary[w].Classification == "interior-finite" ? (double?)m.Primary[w].TStar : null }).ToArray(),
        };
    }).ToArray(),

    // Batteries.
    batteries = new
    {
        batteriesAllPassed,
        thetaStationarityGate,
        maxThetaResidualNonIdentity,
        allThetaConverged,
        identityThetaExactZero,
        thetaResidualBatteryRel = ThetaResidualBatteryRel,
        linThetaFdBattery,
        linThetaFdMaxResidual,
        identityBetaConsistent,
        identityMaxLamTreeRelVariation = identityMaxLamTreeRelVar,
        identityMinLambdaEff = identityMinLamEff,
        identityOneLoopLogSlope = double.IsFinite(identityRg.OneLoopLogSlope) ? (double?)identityRg.OneLoopLogSlope : null,
        controlNoSpuriousMinimum,
        verdictSchemeStable,
        verdictSeedStable,
        fitQualityFloorMet,
        fitR2UncenteredFloor = FitR2UncenteredFloor,
        minOneLoopR2Uncentered,
    },

    // Per-member RG detail.
    memberTable = memberRg.Select(MemberJson).ToArray(),

    saturationVerdict = saturationVerdictPhrase,

    // Recorded boundary (six verbatim keys + the §3.9 wall set).
    recordedBoundary = new
    {
        definition81Scope,
        ambientSevenSevenRealized,
        internalGaugeContentRealized,
        weldRealized,
        canFillPhase201WzContract,
        canFillPhase256Contract,
        physicistReviewPending,
        rgPrescriptionIsWorkbenchConvention,
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
        phase443SummaryPath = Phase443SummaryPath,
        phase444SummaryPath = Phase444SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "V_RG, its interior minima, t*, and t*/latticeScale are candidate-only structure data of the reduced spin-4 slice, NOT physical masses, VEVs, or a scale in GeV.",
        "The RG-improvement prescription (window fit basis, resummation target, window width) is a WORKBENCH CONVENTION pending physicist review; it is scanned for scheme stability and recorded as scheme-dependent where it is.",
        "The measured beta function is the log-derivative of the locally-fit effective quartic of the su(2)-toy one-loop potential; it is not a physical renormalization-group beta function.",
        "An RG-saturation (interior minimum) result, where observed and scheme-stable, is a workbench-relative dynamical-scale CANDIDATE only; a no-saturation or scheme-dependent result sharpens the frontier. Neither promotes anything.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "rg_improved_joint_potential_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "rg_improved_joint_potential_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rgImprovedJointPotentialProbePassed={rgImprovedJointPotentialProbePassed}");
Console.WriteLine($"precursors: p443={phase443PrecursorPassed} p444={phase444PrecursorPassed}");
Console.WriteLine($"mesh: V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} C={mesh.CellCount}; nOmega={nOmega} nTheta={nTheta} jointDof={nJoint}");
Console.WriteLine($"THETA: gate={thetaStationarityGate} maxResid(non-id)={maxThetaResidualNonIdentity:E2} identityThetaExactZero={identityThetaExactZero} linThetaFd={linThetaFdMaxResidual:E2}");
Console.WriteLine($"CONTROL: noSpuriousMin={controlNoSpuriousMinimum} betaConsistent={identityBetaConsistent} lamTreeRelVar={identityMaxLamTreeRelVar:E2} minLamEff={identityMinLamEff:E2} oneLoopLogSlope={identityRg.OneLoopLogSlope:F3}");
Console.WriteLine($"SCHEME/SEED: schemeStable={verdictSchemeStable} seedStable={verdictSeedStable} minR2unc={minOneLoopR2Uncentered:F3} floorMet={fitQualityFloorMet}");
Console.WriteLine($"SATURATION: einsteinianRgSaturation(all/any)={einsteinianRgSaturationObservedAll}/{einsteinianRgSaturationObservedAny}");
Console.WriteLine("--- member x window (primary ray) ---");
foreach (var m in memberRg)
    foreach (int w in WindowWidths)
    {
        var r = m.Primary[w];
        Console.WriteLine($"  {m.Name,-16} w={w} class={r.Classification,-34} tStar={(r.Classification == "interior-finite" ? r.TStar.ToString("F3") : "  -  ")} betaRange=[{r.Beta.Where(double.IsFinite).DefaultIfEmpty(double.NaN).Min():F2},{r.Beta.Where(double.IsFinite).DefaultIfEmpty(double.NaN).Max():F2}]");
    }
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");

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

// Ordinary least squares c = (X^T X)^-1 X^T y; returns (coeffs, centered R^2, uncentered R^2).
static (double[] Coeffs, double R2Centered, double R2Uncentered) LeastSquares(double[][] x, double[] y)
{
    int m = x.Length, k = x[0].Length;
    var ata = new double[k, k];
    var aty = new double[k];
    for (int i = 0; i < m; i++)
        for (int a = 0; a < k; a++)
        {
            aty[a] += x[i][a] * y[i];
            for (int b = 0; b < k; b++) ata[a, b] += x[i][a] * x[i][b];
        }
    var c = SolveSpd(ata, aty) ?? new double[k];
    double ybar = 0; for (int i = 0; i < m; i++) ybar += y[i]; ybar /= m;
    double ssTot = 0, ssTot0 = 0, ssRes = 0;
    for (int i = 0; i < m; i++)
    {
        double pred = 0; for (int a = 0; a < k; a++) pred += x[i][a] * c[a];
        ssRes += (y[i] - pred) * (y[i] - pred);
        ssTot += (y[i] - ybar) * (y[i] - ybar);
        ssTot0 += y[i] * y[i];
    }
    double r2c = ssTot > 1e-30 ? 1.0 - ssRes / ssTot : double.NaN;
    double r2u = ssTot0 > 1e-30 ? 1.0 - ssRes / ssTot0 : double.NaN;
    return (c, r2c, r2u);
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
            }
    }
    var values = new double[n];
    for (int i = 0; i < n; i++) values[i] = a[i, i];
    return values;
}

BranchManifest BuildManifest() => new()
{
    BranchId = "phase445-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase445",
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
    double ThetaResidual, bool ThetaConverged, int ThetaIterations, int ThetaBlockRank,
    double[] ThetaStar);

public sealed record MemberCurves(
    int Index, string Name, bool IsControl, string Phi1, string Phi2, double EinsteinCoefficient,
    List<VEffPoint[]> SeedCurves);

public sealed record RgSchemeResult(
    int WindowWidth, int[] CenterIndices, double[] TCenters,
    double[] LambdaTree, double[] SubQuad, double[] SubCube, double[] DeltaLambda, double[] LambdaEff,
    double[] Beta, double[] Vrg, double[] R2Tree, double[] R2OneLoopCentered, double[] R2OneLoopUncentered,
    string Classification, double TStar, double VStar, double LamTreeRelativeVariation);

public sealed record MemberRgRecord(
    int Index, string Name, bool IsControl, string Phi1, string Phi2, double EinsteinCoefficient,
    List<Dictionary<int, RgSchemeResult>> AllSchemes, Dictionary<int, RgSchemeResult> Primary,
    bool SchemeStable, bool SeedStable, double OneLoopLogSlope);
