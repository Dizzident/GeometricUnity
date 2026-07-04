using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase446: RG SCHEME-DEPENDENCE RESOLUTION probe on the draft-canonical
// Einsteinian Shiab (4D base, spinor-realized reduced Lambda^2 slice, epsilon-
// conjugation). The named follow-up to Phase445, which found THE FIRST INTERIOR
// FINITE MINIMA in the program's history under RG improvement -- but recorded
// them as a SCHEME-DEPENDENT CANDIDATE because the classification flipped at the
// widest fit window.
//
// THE QUESTION. Are the Phase445 interior minima genuine RG-resummation
// structure, or an artifact of the fit prescription? A pre-registered artifact
// MECHANISM is under test: Phase445 fit the one-loop correction V_1loop(t) to
// the single-basis model c*t^4. The Einsteinian one-loop curves are approximately
// a DEEP NEGATIVE CONSTANT (their t->0 limit) plus a slow log-like rise. A
// t-independent constant is pure vacuum-energy normalization -- it CANNOT move
// the minimum of the true V_eff -- yet it completely controls the fitted c,
// because fitting c*t^4 through (constant + slow rise) leaks the constant into
// c and forces the reconstructed V_RG = c(t)*t^4 to 0 at t->0, manufacturing an
// interior dip for ANY deeply negative, slowly rising curve. The identity
// control cannot catch this artifact class because its one-loop curve is
// POSITIVE. A prototype replay of the Phase445 prescription on the committed
// Phase443 raw rays confirmed the mechanism: constant subtraction kills every
// interior minimum, enriched bases give chaotic classifications, and the
// constant-immune direct measurement finds a NEGATIVE log coefficient.
//
// CONSTRUCTION (four decisive arms on recomputed rays).
//   (0) Recompute the V_eff(t) rays with the VERBATIM Phase443/445 hardened
//       machinery (Newton theta*-stationarity, joint FD Hessian, Jacobi
//       eigenvalues, IR zero-mode convention) on CreateUniform4D(1), with the
//       SAME members, seeds, and ray directions as Phase445 (same RngSeed), on
//       a DENSER uniform grid (GridN=48, TMax=3) whose GridN/16-strided subset is
//       EXACTLY the Phase445 16-point grid -- apples-to-apples replication plus
//       double interval density for derivative-based measurement.
//   (1) REPLICATION ARM: rerun the verbatim Phase445 prescription (one-loop fit
//       basis {t^4}, windows {3,5,7}) on the even-subset grid and compare the
//       classifications against the recorded Phase445 verdicts (parsed from its
//       committed summary).
//   (2) OFFSET-INVARIANCE ARM: repeat the verbatim prescription with the
//       one-loop curve shifted by t-independent constants (subtract its
//       smallest-t value; subtract its mean; add +1000). The argmin of the RAW
//       V_eff is exactly invariant under every such shift (asserted); any
//       classification change under a constant shift is therefore an
//       unphysical normalization artifact of the fit, not structure.
//   (3) FIT-BASIS-MENU ARM: repeat with enriched one-loop fit bases
//       ({1,t^4}, {1,ln t,t^4}, and the canonical CW form {t^4, t^4 ln t})
//       across windows and both grids, raw and constant-subtracted. Scheme-
//       stability of the classification across the full menu is the honest
//       measurement of fit-scheme dependence. The identity control's behavior
//       under enriched bases is recorded (a manufactured control minimum under
//       some scheme is direct evidence of prescription fragility).
//   (4) DIRECT (CONSTANT-IMMUNE) ARM: the log-derivative operator L = t d/dt
//       annihilates constants EXACTLY. Measure g(t) = dV_1loop/d ln t by
//       midpoint differences on the dense grid, jump-aware (only intervals with
//       unchanged Hessian mode counts enter; mode-transition intervals are
//       excluded and counted), then extract the genuine CW log coefficient cL
//       from the L-image basis {2t^2, 3t^3, 4t^4, 4 t^4 ln t + t^4}. The CW
//       resummed potential (c4 + cL ln t) t^4 has an interior minimum only if
//       cL > 0 with the implied t* in range. An offset-immunity battery
//       re-derives cL with V_1loop shifted by +1000 and requires agreement.
//   (5) VERDICT. The Phase445 candidate SURVIVES scheme control only if the
//       interior minima persist under constant subtraction AND across the full
//       basis menu AND the direct constant-immune log coefficient supports a
//       minimum. Otherwise the minima are resolved as a fit-normalization
//       artifact (or grid-fragile if replication itself fails) and the frontier
//       sharpens honestly back to the named levers.
//
// MANDATORY FRAMING. All scales are WORKBENCH-RELATIVE CANDIDATE data ONLY:
// su(2) toy algebra on the reduced Spin(4) slice, lattice units, one loop; every
// RG prescription here is a workbench convention pending physicist review. NO
// GeV/pole/VEV promotion either way. The phase PASSES on internal consistency
// (precursors + hardened theta solve + control discipline + honesty batteries)
// REGARDLESS of which way the resolution verdict goes.
//
// Fail-closed: target-blind; reduced-spin4-slice; no scale/pole/GeV lineage; no
// Phase201/Phase256 contract field filled; nothing promoted either way.

const string DefaultOutputDir = "studies/phase446_rg_scheme_dependence_resolution_probe_001/output";
const string Phase443SummaryPath = "studies/phase443_joint_effective_potential_saturation_probe_001/output/joint_effective_potential_saturation_probe_summary.json";
const string Phase445SummaryPath = "studies/phase445_rg_improved_joint_potential_probe_001/output/rg_improved_joint_potential_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ApplicationSubjectKind = "rg-scheme-dependence-resolution-probe";

// Probe configuration (committed defaults; env overrides mirror Phase445's pattern).
int MeshRefinement = int.TryParse(Environment.GetEnvironmentVariable("PHASE446_MESH_REFINEMENT"), out int mr) ? mr : 1;
int RaySeedCount = int.TryParse(Environment.GetEnvironmentVariable("PHASE446_RAYS"), out int rs) ? rs : 2;
double TMax = double.TryParse(Environment.GetEnvironmentVariable("PHASE446_TMAX"), out double tm) ? tm : 3.0;
int GridN = int.TryParse(Environment.GetEnvironmentVariable("PHASE446_GRIDN"), out int gn) ? gn : 48;
const int RngSeed = 20260703; // MUST equal Phase445's seed: same members/seeds => identical ray directions.

// Fit windows (odd point-widths) for the sliding-window schemes.
int[] WindowWidthsEven = { 3, 5, 7 };          // on the Phase445-matching even subset
int[] WindowWidthsFull = { 3, 5, 7, 9, 13 };   // on the dense grid ({5,9,13} ~ the same t-extents as {3,5,7} on the subset)

// One-loop fit bases (the scheme menu). "v445-t4" is the verbatim Phase445 basis.
string[] FitBases = { "v445-t4", "const-t4", "const-logt-t4", "cw-t4-t4logt" };

// Constant offsets (t-independent shifts of the one-loop curve).
string[] OffsetKinds = { "raw", "min-t-subtract", "mean-subtract", "plus-1000" };
string[] MenuOffsetKinds = { "raw", "min-t-subtract" };

// Direct-arm knobs.
const int DirectIntervalFloor = 10;             // minimum usable (jump-free) intervals (5-parameter fit needs real residuals)
const double DirectOffsetImmunityRelTol = 1e-9; // cL must agree under a +1000 shift to this relative tol

// Theta-stationarity solver knobs (mode 3) -- VERBATIM from Phase443/445 (hardened solver).
const int ThetaMaxIter = 1000;
const double ThetaTolRel = 1e-9;
const double ThetaResidualBatteryRel = 1e-8;
// Absolute-gradient acceptance floor for the theta point gate. At the smallest
// grid points (t <= 0.19, below anything Phase443/445 sampled) the warm-started
// initial gradient is already near zero, so the RELATIVE residual measure is
// ill-defined (the Phase443 dimensional-gate lesson at tiny t): a point also
// passes if its absolute gradient norm is at the solver floor.
const double ThetaAbsGradFloor = 1e-10;
const double ThetaFdStep = 1e-6;

// Hessian / eigenvalue knobs (verbatim from Phase443/445).
const double HessStep = 1e-4;
const double ZeroModeRelTol = 1e-8;
const double ZeroModeAbsFloor = 1e-10;

// FD-vs-analytic LinearizeTheta battery ceiling.
const double LinearizeThetaFdTol = 1e-6;

// Control / fit-quality thresholds (workbench conventions; recorded).
const double IdentityLamTreeRelVariationTol = 1e-3;

var outputDir = Environment.GetEnvironmentVariable("PHASE446_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors: Phase443 (no unimproved one-loop saturation) and Phase445
// (scheme-dependent RG candidate: interior minima observed but window-fragile).
// ---------------------------------------------------------------------------

using var phase443 = JsonDocument.Parse(File.ReadAllText(Phase443SummaryPath));
bool phase443PrecursorPassed =
    JsonBool(phase443.RootElement, "jointEffectivePotentialSaturationProbePassed") is true &&
    JsonBool(phase443.RootElement, "einsteinianLogSaturationObserved") is false;

using var phase445 = JsonDocument.Parse(File.ReadAllText(Phase445SummaryPath));
bool phase445PrecursorPassed =
    JsonBool(phase445.RootElement, "rgImprovedJointPotentialProbePassed") is true &&
    JsonBool(phase445.RootElement, "verdictSchemeStable") is false &&
    JsonBool(phase445.RootElement, "einsteinianRgSaturationObservedAny") is true;

bool precursorsPassed = phase443PrecursorPassed && phase445PrecursorPassed;

// Recorded Phase445 primary-ray classifications (member, windowWidth) -> classification,
// plus its committed t-grid, for the replication comparison.
var p445Classifications = new Dictionary<(string Member, int Window), string>();
double[] p445TGrid = Array.Empty<double>();
if (phase445.RootElement.TryGetProperty("memberTable", out var p445Members))
    foreach (var m in p445Members.EnumerateArray())
    {
        string name = m.GetProperty("member").GetString() ?? "";
        foreach (var r in m.GetProperty("primaryRayByWindow").EnumerateArray())
            p445Classifications[(name, r.GetProperty("windowWidth").GetInt32())] =
                r.GetProperty("classification").GetString() ?? "";
    }
if (phase445.RootElement.TryGetProperty("probeConfiguration", out var p445Cfg) &&
    p445Cfg.TryGetProperty("tGrid", out var p445TGridEl))
    p445TGrid = p445TGridEl.EnumerateArray().Select(e => e.GetDouble()).ToArray();

// ---------------------------------------------------------------------------
// Machinery (verbatim Phase443/445): 4D mesh, su(2) trace pairing, trivial torsion.
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
// Members: identity control + the Phase445 Einsteinian subset (same order and
// naming so the per-member RNG stream reproduces the Phase445 ray directions).
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
// Eval builders (verbatim Phase443/445).
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
// Mode 3 theta*-stationarity solve (VERBATIM hardened Newton from Phase443/445).
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

    var eigTheta = JacobiEigenvalues(hTheta);
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
// One-loop effective potential at a composite point (verbatim Phase443/445).
// ---------------------------------------------------------------------------

VEffPoint EvalVEff(
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
        th.ResidualNorm, th.AbsoluteGradNorm, th.Converged, th.Iterations, th.ThetaBlockRank, th.Theta);
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
// Recompute the V_eff(t) rays on the dense grid.
// ---------------------------------------------------------------------------

var tGrid = new double[GridN];
for (int i = 0; i < GridN; i++) tGrid[i] = TMax * (i + 1) / GridN;

// Phase445-matching subset: when GridN is a multiple of 16 the points with
// (i+1) divisible by GridN/16 are EXACTLY the Phase445 16-point grid.
int subsetStep = GridN % 16 == 0 ? GridN / 16 : 2;
var evenIdx = Enumerable.Range(0, GridN).Where(i => (i + 1) % subsetStep == 0).ToArray();
var tGridEven = evenIdx.Select(i => tGrid[i]).ToArray();
bool evenSubsetMatchesPhase445Grid =
    p445TGrid.Length == tGridEven.Length &&
    p445TGrid.Zip(tGridEven, (a, b) => System.Math.Abs(a - b)).All(d => d <= 1e-12);

double linThetaFdMaxResidual = 0.0;
double maxThetaResidualNonIdentity = 0.0;
bool allThetaConverged = true;
bool identityThetaExactZero = true;
bool allThetaPointsPassGate = true;
int thetaPointsPassedByAbsFloor = 0;

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
        // Backward continuation of the first (cold-started) point (verbatim Phase443/445).
        if (!spec.IsControl && pts.Count >= 2 && !pts[0].ThetaConverged)
            pts[0] = EvalVEff(eval, op, uOmega, pts[0].T, spec.IsControl, pts[1].ThetaStar);

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
// Scheme analysis (post-processing on the recomputed V_eff rays).
// ---------------------------------------------------------------------------

double OffsetOf(VEffPoint[] curve, string kind) => kind switch
{
    "raw" => 0.0,
    "min-t-subtract" => -curve[0].OneLoop,
    "mean-subtract" => -curve.Average(p => p.OneLoop),
    "plus-1000" => 1000.0,
    _ => throw new InvalidOperationException($"unknown offset kind {kind}"),
};

// Generalized Phase445-style sliding-window analysis with a selectable one-loop
// fit basis and a constant offset applied to the one-loop curve before fitting.
SchemeResult Analyze(VEffPoint[] curve, int w, string basis, double offset)
{
    int half = w / 2;
    var tcenL = new List<double>();
    var lamTree = new List<double>();
    var subQuad = new List<double>();
    var subCube = new List<double>();
    var lamEff = new List<double>();
    var r2OneLoopUnc = new List<double>();

    for (int k = half; k < curve.Length - half; k++)
    {
        int lo = k - half, hi = k + half;
        int m = hi - lo + 1;

        // Tree fit: S_B ~ a t^2 + b t^3 + c t^4 (S_B is an exact quartic in t).
        var At = new double[m][];
        var yt = new double[m];
        for (int r = 0; r < m; r++)
        {
            double x = curve[lo + r].T, x2 = x * x;
            At[r] = new[] { x2, x2 * x, x2 * x2 };
            yt[r] = curve[lo + r].SB;
        }
        var (ct, _, _) = LeastSquares(At, yt);

        // One-loop correction fit in the selected basis (offset applied).
        var Ao = new double[m][];
        var yo = new double[m];
        for (int r = 0; r < m; r++)
        {
            double x = curve[lo + r].T;
            Ao[r] = basis switch
            {
                "v445-t4" => new[] { x * x * x * x },
                "const-t4" => new[] { 1.0, x * x * x * x },
                "const-logt-t4" => new[] { 1.0, System.Math.Log(x), x * x * x * x },
                "cw-t4-t4logt" => new[] { x * x * x * x, x * x * x * x * System.Math.Log(x) },
                _ => throw new InvalidOperationException($"unknown basis {basis}"),
            };
            yo[r] = curve[lo + r].OneLoop + offset;
        }
        var (co, _, r2oU) = LeastSquares(Ao, yo);
        double tc = curve[k].T;
        double dlam = basis switch
        {
            "v445-t4" => co[0],
            "const-t4" => co[1],
            "const-logt-t4" => co[2],
            "cw-t4-t4logt" => co[0] + co[1] * System.Math.Log(tc),
            _ => double.NaN,
        };

        tcenL.Add(tc);
        lamTree.Add(ct[2]); subQuad.Add(ct[0]); subCube.Add(ct[1]);
        lamEff.Add(ct[2] + dlam);
        r2OneLoopUnc.Add(r2oU);
    }

    int nc = tcenL.Count;
    var tcen = tcenL.ToArray();
    var vrg = new double[nc];
    for (int j = 0; j < nc; j++)
    {
        double x = tcen[j];
        vrg[j] = lamEff[j] * x * x * x * x + subQuad[j] * x * x + subCube[j] * x * x * x;
    }

    // Saturation classification with verified descent (verbatim Phase445 rule).
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

    return new SchemeResult(w, basis, tcen, lamTree.ToArray(), lamEff.ToArray(), vrg,
        r2OneLoopUnc.ToArray(), cls, tStar, vStar, lamTreeRelVar);
}

// Direct (constant-immune) measurement: jump-aware midpoint log-derivative of the
// one-loop curve, then LSQ in the L-image basis. L = t d/dt kills constants exactly.
// The basis includes a CONSTANT column = the L-image of s*ln t (the known fixed-
// coupling one-loop log asymptotic); without it that content aliases into the
// polynomial columns and biases cL (verified on a synthetic pure-log curve).
DirectResult DirectAnalyze(VEffPoint[] curve, double offset)
{
    var tmid = new List<double>();
    var gval = new List<double>();
    int excluded = 0;
    for (int i = 0; i + 1 < curve.Length; i++)
    {
        bool usable = curve[i].PosCount == curve[i + 1].PosCount && curve[i].ZeroCount == curve[i + 1].ZeroCount;
        if (!usable) { excluded++; continue; }
        double g = ((curve[i + 1].OneLoop + offset) - (curve[i].OneLoop + offset))
                   / (System.Math.Log(curve[i + 1].T) - System.Math.Log(curve[i].T));
        tmid.Add(System.Math.Sqrt(curve[i].T * curve[i + 1].T));
        gval.Add(g);
    }

    int nu = tmid.Count;
    if (nu < 6)
        return new DirectResult(nu, excluded, double.NaN, double.NaN, double.NaN, double.NaN,
            double.NaN, double.NaN, double.NaN, false, false, false, "insufficient-direct-intervals");

    var A = new double[nu][];
    var y = new double[nu];
    for (int i = 0; i < nu; i++)
    {
        double x = tmid[i], x2 = x * x, x4 = x2 * x2, lx = System.Math.Log(x);
        // L-images of {ln t, t^2, t^3, t^4, t^4 ln t}: {1, 2t^2, 3t^3, 4t^4, 4 t^4 ln t + t^4}.
        A[i] = new[] { 1.0, 2.0 * x2, 3.0 * x2 * x, 4.0 * x4, 4.0 * x4 * lx + x4 };
        y[i] = gval[i];
    }
    var (c, r2c, r2u) = LeastSquares(A, y);
    double sLog = c[0], c4 = c[3], cL = c[4];
    double residSs = 0.0, ySs = 0.0;
    for (int i = 0; i < nu; i++)
    {
        double pred = 0; for (int a = 0; a < 5; a++) pred += A[i][a] * c[a];
        residSs += (y[i] - pred) * (y[i] - pred);
        ySs += y[i] * y[i];
    }
    double residualRms = System.Math.Sqrt(residSs / nu);
    double gRms = System.Math.Sqrt(ySs / nu);
    // Fit-quality guard: centered R^2 must be positive UNLESS the fit is essentially
    // exact (a constant g makes centered R^2 meaningless 0/0).
    bool fitQualityOk = (double.IsFinite(r2c) && r2c > 0.0) || residualRms <= 1e-9 * System.Math.Max(1.0, gRms);
    double tStar = cL != 0.0 ? System.Math.Exp(-(4.0 * c4 + cL) / (4.0 * cL)) : double.PositiveInfinity;
    bool inRange = double.IsFinite(tStar) && tStar >= tGrid[0] && tStar <= tGrid[^1];
    bool supports = cL > 0.0 && inRange;
    string cls = supports ? "direct-cw-minimum-supported"
        : cL > 0.0 ? "direct-cw-log-positive-t-star-out-of-range"
        : "direct-cw-log-nonpositive-no-minimum";
    return new DirectResult(nu, excluded, c4, cL, sLog, tStar, r2c, r2u, residualRms, supports, cL > 0.0, fitQualityOk, cls);
}

var memberAnalyses = new List<MemberAnalysis>();
foreach (var mc in memberCurves)
{
    var perSeed = new List<SeedAnalysis>();
    foreach (var curve in mc.SeedCurves)
    {
        var curveEven = evenIdx.Select(i => curve[i]).ToArray();

        // Replication cells: even subset, verbatim basis, raw offset (full detail retained).
        var replication = WindowWidthsEven
            .Select(w => Analyze(curveEven, w, "v445-t4", 0.0))
            .ToArray();

        // Offset-invariance cells: even subset, verbatim basis, all offsets.
        var offsetCells = new List<SchemeCell>();
        foreach (string ok in OffsetKinds)
        {
            double off = OffsetOf(curve, ok);
            foreach (int w in WindowWidthsEven)
            {
                var r = Analyze(curveEven, w, "v445-t4", ok == "raw" ? 0.0 : off);
                offsetCells.Add(new SchemeCell("phase445-subset", "v445-t4", w, ok, r.Classification,
                    r.Classification == "interior-finite" ? r.TStar : double.NaN));
            }
        }

        // Basis-menu cells: both grids x bases x windows x {raw, min-t-subtract}.
        var menuCells = new List<SchemeCell>();
        foreach (string ok in MenuOffsetKinds)
        {
            double off = OffsetOf(curve, ok);
            foreach (string basis in FitBases)
            {
                foreach (int w in WindowWidthsEven)
                {
                    var r = Analyze(curveEven, w, basis, ok == "raw" ? 0.0 : off);
                    menuCells.Add(new SchemeCell("phase445-subset", basis, w, ok, r.Classification,
                        r.Classification == "interior-finite" ? r.TStar : double.NaN));
                }
                foreach (int w in WindowWidthsFull)
                {
                    var r = Analyze(curve, w, basis, ok == "raw" ? 0.0 : off);
                    menuCells.Add(new SchemeCell("full-grid", basis, w, ok, r.Classification,
                        r.Classification == "interior-finite" ? r.TStar : double.NaN));
                }
            }
        }

        // Raw-argmin offset invariance (exact principle): the argmin of V_eff + const
        // is the argmin of V_eff, for every probed constant.
        int ArgMin(Func<VEffPoint, double> f)
        {
            int am = 0;
            for (int i = 1; i < curve.Length; i++) if (f(curve[i]) < f(curve[am])) am = i;
            return am;
        }
        int rawArgmin = ArgMin(p => p.VEff);
        bool rawArgminInvariant = OffsetKinds.Where(k => k != "raw").All(k =>
        {
            double off = OffsetOf(curve, k);
            return ArgMin(p => p.VEff + off) == rawArgmin;
        });

        // Direct arm (raw + offset-immunity battery at +1000).
        var direct = DirectAnalyze(curve, 0.0);
        var directShifted = DirectAnalyze(curve, 1000.0);
        bool directOffsetImmune =
            double.IsFinite(direct.CL) && double.IsFinite(directShifted.CL) &&
            System.Math.Abs(direct.CL - directShifted.CL)
                <= DirectOffsetImmunityRelTol * System.Math.Max(1.0, System.Math.Abs(direct.CL));

        perSeed.Add(new SeedAnalysis(replication, offsetCells.ToArray(), menuCells.ToArray(),
            direct, directShifted, directOffsetImmune, rawArgminInvariant));
    }
    memberAnalyses.Add(new MemberAnalysis(mc.Index, mc.Name, mc.IsControl, mc.Phi1, mc.Phi2,
        mc.EinsteinCoefficient, perSeed));
}

// ---------------------------------------------------------------------------
// SYNTHETIC ARTIFACT-SENSITIVE CONTROL. The identity control is structurally
// blind to the constant-leakage artifact (its one-loop is positive), so it can
// never certify the prescription. Build a hand-made curve with the member SHAPE
// (positive-definite tree + deep negative constant + slow log rise) that is
// PROVABLY monotone increasing (a,s,lambda > 0 => dV/dt = 2at + 4*lam*t^3 + s/t > 0):
// it has no interior minimum by construction. Any scheme that classifies it
// interior-finite is manufacturing minima; if the DIRECT arm supports a minimum
// on it, the direct arm itself is broken and the phase fail-closes.
const double SynA = 0.05, SynLam = 0.006, SynC = -150.0, SynS = 40.0;
var synCurve = tGrid.Select(t => new VEffPoint(
    t,
    SynA * t * t + SynLam * t * t * t * t,
    SynC + SynS * System.Math.Log(t),
    SynA * t * t + SynLam * t * t * t * t + SynC + SynS * System.Math.Log(t),
    200, 0, 0, 0.0, 0.0, 0.0, true, 0, 0, new double[0])).ToArray();
bool syntheticMonotoneOnGrid = true;
for (int i = 0; i + 1 < synCurve.Length; i++)
    if (synCurve[i + 1].VEff <= synCurve[i].VEff) syntheticMonotoneOnGrid = false;

var synEven = evenIdx.Select(i => synCurve[i]).ToArray();
var synVerbatimCells = new List<SchemeCell>();
foreach (string ok in new[] { "raw", "min-t-subtract" })
{
    double off = ok == "raw" ? 0.0 : -synCurve[0].OneLoop;
    foreach (int w in WindowWidthsEven)
    {
        var r = Analyze(synEven, w, "v445-t4", off);
        synVerbatimCells.Add(new SchemeCell("phase445-subset", "v445-t4", w, ok, r.Classification,
            r.Classification == "interior-finite" ? r.TStar : double.NaN));
    }
    foreach (int w in WindowWidthsFull)
    {
        var r = Analyze(synCurve, w, "v445-t4", off);
        synVerbatimCells.Add(new SchemeCell("full-grid", "v445-t4", w, ok, r.Classification,
            r.Classification == "interior-finite" ? r.TStar : double.NaN));
    }
}
var synDirect = DirectAnalyze(synCurve, 0.0);
bool syntheticVerbatimSchemeManufacturesMinimum = synVerbatimCells
    .Any(c => c.Offset == "raw" && c.Classification == "interior-finite");
bool syntheticConstantSubtractionKillsManufacturedMinimum = synVerbatimCells
    .Where(c => c.Offset == "min-t-subtract").All(c => c.Classification != "interior-finite");
bool syntheticDirectArmClean = !synDirect.SupportsMinimum;

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// Batteries + verdicts.
// ---------------------------------------------------------------------------

var identityA = memberAnalyses.First(m => m.IsControl);
var einsteinianA = memberAnalyses.Where(m => !m.IsControl).ToList();

// Theta*-stationarity gate: every non-identity point passes RELATIVE <= 1e-8 OR
// absolute gradient norm <= the solver floor (small-t dimensional-gate convention).
bool thetaStationarityGate = allThetaConverged && allThetaPointsPassGate && identityThetaExactZero;
bool linThetaFdBattery = linThetaFdMaxResidual <= LinearizeThetaFdTol;

// Identity control discipline under the VERBATIM Phase445 scheme (raw, {t^4}):
// window-constant tree quartic, positive lambda_eff, no interior minimum.
double identityMaxLamTreeRelVar = identityA.Seeds[0].Replication.Max(r => r.LamTreeRelativeVariation);
double identityMinLamEff = identityA.Seeds[0].Replication
    .SelectMany(r => r.LambdaEff).DefaultIfEmpty(double.NaN).Min();
bool identityBetaConsistent = identityMaxLamTreeRelVar <= IdentityLamTreeRelVariationTol && identityMinLamEff > 0.0;
bool controlNoSpuriousMinimumVerbatimScheme = identityA.Seeds
    .All(s => s.Replication.All(r => r.Classification != "interior-finite"));

// Recorded (informational, NOT a blocker): does ANY menu scheme manufacture an
// interior minimum for the identity control? Direct evidence of prescription fragility.
bool identityEnrichedSchemeManufacturesMinimum = identityA.Seeds
    .Any(s => s.MenuCells.Any(c => c.Classification == "interior-finite"));

// Raw-argmin offset invariance (exact principle) across all members/seeds.
bool rawArgminOffsetInvarianceExact = memberAnalyses.All(m => m.Seeds.All(s => s.RawArgminOffsetInvariant));

// Direct-arm admissibility: enough jump-free intervals + offset immunity + fit
// quality (centered R^2 or exact fit) for every member/seed, and the direct arm
// must be clean on the provably-monotone synthetic control.
bool directIntervalFloorMet = memberAnalyses.All(m => m.Seeds.All(s => s.Direct.UsableIntervals >= DirectIntervalFloor));
bool directOffsetImmunityBatteryPassed = memberAnalyses.All(m => m.Seeds.All(s => s.DirectOffsetImmune));
bool directFitQualityMet = memberAnalyses.All(m => m.Seeds.All(s => s.Direct.FitQualityOk));
bool directArmAdmissible = directIntervalFloorMet && directOffsetImmunityBatteryPassed && directFitQualityMet && syntheticDirectArmClean;

// --- Resolution verdicts (primary seed 0; seed agreement recorded) ---

// (1) Replication vs the recorded Phase445 classifications.
bool replicationComparisonPerformed = evenSubsetMatchesPhase445Grid && p445Classifications.Count > 0;
bool replicationMatchesPhase445 = replicationComparisonPerformed && memberAnalyses.All(m =>
    m.Seeds[0].Replication.All(r =>
        p445Classifications.TryGetValue((m.Name, r.WindowWidth), out string? want) && want == r.Classification));

// Any interior minimum in-house under the verbatim raw scheme (needed for the artifact clauses).
bool einsteinianAnyInteriorVerbatimRaw = einsteinianA.Any(m =>
    m.Seeds[0].Replication.Any(r => r.Classification == "interior-finite"));

// (2) Offset sensitivity of the verbatim scheme (unphysical if present).
bool offsetSensitivityDemonstrated = einsteinianA.Any(m =>
    m.Seeds[0].OffsetCells.GroupBy(c => c.Window)
        .Any(g => g.Select(c => c.Classification).Distinct().Count() > 1));

// Constant subtraction kills the interior minima (the pre-registered artifact signature).
bool constantSubtractionKillsInteriorMinima = einsteinianA.All(m =>
    m.Seeds[0].OffsetCells.Where(c => c.Offset == "min-t-subtract")
        .All(c => c.Classification != "interior-finite") &&
    m.Seeds[0].MenuCells.Where(c => c.Offset == "min-t-subtract" && c.Basis == "v445-t4")
        .All(c => c.Classification != "interior-finite"));

// (3) Fit-scheme (ir)reducibility over the full menu.
bool fitSchemeClassificationIrreducible = einsteinianA.Any(m =>
    m.Seeds[0].MenuCells.Select(c => c.Classification).Distinct().Count() > 1);

// A member "survives the menu" only if EVERY menu cell is interior-finite.
bool anyMemberSurvivesBasisMenu = einsteinianA.Any(m =>
    m.Seeds[0].MenuCells.All(c => c.Classification == "interior-finite"));

// (4) Direct constant-immune support.
bool directCwLogCoefficientPositiveAnyEinsteinian = einsteinianA.Any(m => m.Seeds.All(s => s.Direct.CLPositive));
bool directRgMinimumSupported = einsteinianA.Any(m => m.Seeds.All(s => s.Direct.SupportsMinimum));

// Survival requires ALL of: menu survival, constant-subtraction survival, direct support.
bool candidateSurvivesSchemeControl =
    anyMemberSurvivesBasisMenu && !constantSubtractionKillsInteriorMinima && directRgMinimumSupported;

// Final resolution.
string resolutionKind;
if (!einsteinianAnyInteriorVerbatimRaw)
    resolutionKind = "grid-fragile"; // the Phase445 minima did not even replicate on the refined ray data
else if (candidateSurvivesSchemeControl)
    resolutionKind = "survives-scheme-control";
else if (directArmAdmissible && constantSubtractionKillsInteriorMinima)
    resolutionKind = "fit-normalization-artifact";
else
    resolutionKind = "scheme-dependence-recorded-unresolved";

bool phase445MinimaResolvedAsFitNormalizationArtifact = resolutionKind == "fit-normalization-artifact";
bool einsteinianRgSaturationObserved = resolutionKind == "survives-scheme-control";

bool seedAgreementOnResolution = einsteinianA.All(m =>
{
    var kinds = m.Seeds.Select(s =>
        s.MenuCells.All(c => c.Classification == "interior-finite") && s.Direct.SupportsMinimum).Distinct();
    return kinds.Count() == 1;
});

bool batteriesAllPassed =
    precursorsPassed &&
    thetaStationarityGate &&
    linThetaFdBattery &&
    identityBetaConsistent &&
    controlNoSpuriousMinimumVerbatimScheme &&
    rawArgminOffsetInvarianceExact &&
    directOffsetImmunityBatteryPassed &&
    syntheticMonotoneOnGrid &&
    syntheticDirectArmClean;

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
    "recompute V_eff rays dense grid; arms: verbatim replication vs recorded phase445; constant-offset invariance; one-loop fit-basis menu {t4},{1,t4},{1,logt,t4},{t4,t4logt} x windows x grids x normalizations; direct constant-immune L=t d/dt log-derivative CW coefficient in the L-image basis {1,2t2,3t3,4t4,4t4logt+t4} with jump-aware intervals + offset-immunity battery + provably-monotone synthetic artifact-sensitive control; no target values")))).ToLowerInvariant();

bool rgSchemeDependenceResolutionProbePassed =
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

string resolutionVerdictPhrase = resolutionKind switch
{
    "grid-fragile" =>
        "the Phase445 interior minima did NOT reproduce under the verbatim prescription on the refined ray data -- the candidate is GRID-FRAGILE and is resolved as a discretization artifact of the fit prescription, not structure",
    "survives-scheme-control" =>
        "the interior minima SURVIVE full scheme control (constant subtraction, the enriched fit-basis menu, and the constant-immune direct measurement agree) -- the first scheme-controlled workbench-relative dynamical-scale CANDIDATE (still workbench-relative, candidate-only, no GeV)",
    "fit-normalization-artifact" =>
        "the Phase445 interior minima are RESOLVED AS A FIT-NORMALIZATION ARTIFACT: a t-independent constant (pure vacuum normalization, provably unable to move the true V_eff minimum) controls the fitted t^4 coefficient; constant subtraction kills every interior minimum, the fit-basis menu gives irreducibly scheme-dependent classifications, and the constant-immune direct measurement finds a non-positive CW log coefficient (no resummation-driven minimum). The RG-improved potential-fit route on the minimal mesh is CLOSED; the frontier returns to the named levers (mode-volume unlock projects, beyond-one-loop structure, or a source anchor)",
    _ =>
        "the scheme dependence could not be fully resolved (recorded per-arm; see the admissibility and stability booleans) -- no candidate is promoted and no closure is claimed",
};

string terminalStatus = rgSchemeDependenceResolutionProbePassed
    ? resolutionKind switch
    {
        "fit-normalization-artifact" => "rg-scheme-dependence-resolution-probe-passed-phase445-minima-resolved-as-fit-normalization-artifact-frontier-sharpened",
        "grid-fragile" => "rg-scheme-dependence-resolution-probe-passed-phase445-minima-grid-fragile-resolved-as-artifact-frontier-sharpened",
        "survives-scheme-control" => "rg-scheme-dependence-resolution-probe-passed-rg-saturation-survives-scheme-control-workbench-relative-candidate-no-gev",
        _ => "rg-scheme-dependence-resolution-probe-passed-scheme-dependence-recorded-unresolved",
    }
    : "rg-scheme-dependence-resolution-probe-blocked";

string decision = rgSchemeDependenceResolutionProbePassed
    ? $"The RG scheme-dependence resolution probe is decided on internal consistency. On CreateUniform4D({MeshRefinement}) (su(2) trace pairing; nOmega={nOmega}, nTheta={nTheta}, joint DOF {nJoint}) the V_eff(t) rays were recomputed with the verbatim Phase443/445 hardened machinery on a {GridN}-point grid whose even subset {(evenSubsetMatchesPhase445Grid ? "EXACTLY matches" : "does NOT match")} the committed Phase445 grid (every non-identity theta point passes RELATIVE residual <= {ThetaResidualBatteryRel:E0} or the absolute-gradient floor {ThetaAbsGradFloor:E0} [{thetaPointsPassedByAbsFloor} small-t points via the floor; max relative {maxThetaResidualNonIdentity:E2}]; identity theta*=0 exact; analytic LinearizeTheta matched FD to {linThetaFdMaxResidual:E2}). " +
      $"ARM 1 (replication): comparisonPerformed={replicationComparisonPerformed}, matchesPhase445={replicationMatchesPhase445}. " +
      $"ARM 2 (offset invariance): the raw V_eff argmin is EXACTLY constant-shift invariant (asserted); the verbatim fitted scheme is offset-sensitive={offsetSensitivityDemonstrated}; constant subtraction kills the interior minima={constantSubtractionKillsInteriorMinima}. " +
      $"ARM 3 (fit-basis menu, {FitBases.Length} bases x windows x 2 grids x 2 normalizations): classification irreducibly scheme-dependent={fitSchemeClassificationIrreducible}; any member survives the full menu={anyMemberSurvivesBasisMenu}; the identity control is itself given a manufactured minimum by some enriched scheme={identityEnrichedSchemeManufacturesMinimum}. " +
      $"SYNTHETIC CONTROL (provably monotone; tree {SynA} t^2 + {SynLam} t^4, one-loop {SynC} + {SynS} ln t): the verbatim scheme manufactures an interior minimum on it={syntheticVerbatimSchemeManufacturesMinimum}; constant subtraction kills that manufactured minimum={syntheticConstantSubtractionKillsManufacturedMinimum}; the direct arm stays clean on it={syntheticDirectArmClean}. " +
      $"ARM 4 (direct, constant-immune, jump-aware, L-image basis incl. the fixed-log column): admissible={directArmAdmissible} (interval floor {DirectIntervalFloor}, offset-immunity battery passed={directOffsetImmunityBatteryPassed}, fit quality met={directFitQualityMet}); CW log coefficient positive for any Einsteinian member={directCwLogCoefficientPositiveAnyEinsteinian}; direct minimum supported={directRgMinimumSupported}. " +
      $"RESOLUTION: {resolutionVerdictPhrase}. " +
      $"MANDATORY FRAMING: all scales are WORKBENCH-RELATIVE CANDIDATE data ONLY (su(2) toy algebra, reduced Spin(4) slice, lattice units, one loop); every RG prescription here is a WORKBENCH CONVENTION pending physicist review; NO GeV/pole/VEV promotion either way. Everything is target-blind, reduced-spin4-slice; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the resolution verdicts until the precursor, theta-stationarity, control-discipline, offset-invariance, and direct-arm honesty batteries pass.";

// ---------------------------------------------------------------------------
// Serialize.
// ---------------------------------------------------------------------------

object CellJson(SchemeCell c) => new
{
    grid = c.Grid,
    basis = c.Basis,
    windowWidth = c.Window,
    offset = c.Offset,
    classification = c.Classification,
    tStar = double.IsFinite(c.TStar) ? (double?)c.TStar : null,
};

object DirectJson(DirectResult d) => new
{
    usableIntervals = d.UsableIntervals,
    excludedModeTransitionIntervals = d.ExcludedIntervals,
    c4 = double.IsFinite(d.C4) ? (double?)d.C4 : null,
    cwLogCoefficient = double.IsFinite(d.CL) ? (double?)d.CL : null,
    fixedLogSlopeCoefficient = double.IsFinite(d.SLog) ? (double?)d.SLog : null,
    impliedTStar = double.IsFinite(d.TStar) ? (double?)d.TStar : null,
    r2Centered = double.IsFinite(d.R2Centered) ? (double?)d.R2Centered : null,
    r2Uncentered = double.IsFinite(d.R2Uncentered) ? (double?)d.R2Uncentered : null,
    residualRms = double.IsFinite(d.ResidualRms) ? (double?)d.ResidualRms : null,
    fitQualityOk = d.FitQualityOk,
    cwLogCoefficientPositive = d.CLPositive,
    supportsMinimum = d.SupportsMinimum,
    classification = d.Classification,
};

object MemberJson(MemberAnalysis m) => new
{
    index = m.Index,
    member = m.Name,
    phi1 = m.Phi1,
    phi2 = m.Phi2,
    einsteinCoefficient = m.EinsteinCoefficient,
    isControl = m.IsControl,
    replicationByWindow = m.Seeds[0].Replication.Select(r => new
    {
        windowWidth = r.WindowWidth,
        classification = r.Classification,
        phase445RecordedClassification = p445Classifications.TryGetValue((m.Name, r.WindowWidth), out string? v) ? v : null,
        interiorFiniteMinimum = r.Classification == "interior-finite",
        tStar = r.Classification == "interior-finite" ? (double?)r.TStar : null,
        vRgAtTStar = r.Classification == "interior-finite" ? (double?)r.VStar : null,
        lamTreeRelativeVariation = double.IsFinite(r.LamTreeRelativeVariation) ? (double?)r.LamTreeRelativeVariation : null,
        minR2OneLoopUncentered = FiniteOrNull(r.R2OneLoopUncentered.DefaultIfEmpty(double.NaN).Min()),
        tCenters = r.TCenters,
        lambdaEff = r.LambdaEff,
        vRg = r.Vrg,
    }).ToArray(),
    offsetCells = m.Seeds[0].OffsetCells.Select(CellJson).ToArray(),
    menuCells = m.Seeds[0].MenuCells.Select(CellJson).ToArray(),
    direct = DirectJson(m.Seeds[0].Direct),
    directShiftedPlus1000 = DirectJson(m.Seeds[0].DirectShifted),
    directOffsetImmune = m.Seeds[0].DirectOffsetImmune,
    rawArgminOffsetInvariant = m.Seeds[0].RawArgminOffsetInvariant,
    seeds = m.Seeds.Select((s, si) => new
    {
        seed = si,
        replication = s.Replication.Select(r => new { windowWidth = r.WindowWidth, classification = r.Classification }).ToArray(),
        menuAllInteriorFinite = s.MenuCells.All(c => c.Classification == "interior-finite"),
        direct = DirectJson(s.Direct),
        directOffsetImmune = s.DirectOffsetImmune,
    }).ToArray(),
};

var rayDataJson = memberCurves.Select(mc => new
{
    member = mc.Name,
    isControl = mc.IsControl,
    seeds = mc.SeedCurves.Select((curve, si) => new
    {
        seed = si,
        points = curve.Select(p => new
        {
            t = p.T,
            sB = p.SB,
            oneLoop = p.OneLoop,
            vEff = p.VEff,
            positiveModes = p.PosCount,
            zeroModes = p.ZeroCount,
            negativeModes = p.NegCount,
            thetaResidual = p.ThetaResidual,
            thetaAbsoluteGradNorm = p.ThetaAbsGradNorm,
            thetaConverged = p.ThetaConverged,
        }).ToArray(),
    }).ToArray(),
}).ToArray();

var result = new
{
    phaseId = "phase446-rg-scheme-dependence-resolution-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    rgSchemeDependenceResolutionProbePassed,

    // Precursors.
    phase443PrecursorPassed,
    phase445PrecursorPassed,
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
        evenSubsetMatchesPhase445Grid,
        windowWidthsEvenSubset = WindowWidthsEven,
        windowWidthsFullGrid = WindowWidthsFull,
        fitBases = FitBases,
        offsetKinds = OffsetKinds,
        menuOffsetKinds = MenuOffsetKinds,
        directIntervalFloor = DirectIntervalFloor,
        directOffsetImmunityRelTol = DirectOffsetImmunityRelTol,
        latticeScale,
        latticeScaleBasis = "mean mesh edge length (workbench convention)",
        hessStep = HessStep,
        zeroModeRelTol = ZeroModeRelTol,
        zeroModeAbsFloor = ZeroModeAbsFloor,
        thetaTol = ThetaTolRel,
        thetaResidualBattery = ThetaResidualBatteryRel,
        thetaAbsGradFloor = ThetaAbsGradFloor,
        thetaPointGate = "per-point: relative residual <= 1e-8 OR absolute gradient norm <= 1e-10 (the small-t region has near-zero warm-started initial gradients, making the relative measure ill-defined -- the Phase443 dimensional-gate lesson applied at tiny t; both measures recorded per point)",
        epsilonRealization,
        epsilonTaxonomyMode,
        vEffDefinition = "V_eff(t) = S_B(t*u, theta*(t*u)) + (1/2) sum_i log lambda_i over positive eigenvalues of the joint (omega,theta) FD Hessian at the composite point (verbatim Phase443/445 machinery)",
        thetaStationarity = "dS_B/dtheta = 0 solved by the VERBATIM Phase443 hardened Newton iteration (exact analytic gradient via LinearizeTheta; theta-block Hessian by forward-differencing that gradient; regularized-Newton step accepted only if ||g|| decreases); relative gate <= 1e-8; identity theta*=0 exact",
        artifactMechanismUnderTest = "PRE-REGISTERED: the Phase445 one-loop fit basis {t^4} leaks the t-independent one-loop constant (deeply negative for Einsteinian members; pure vacuum normalization that provably cannot move the true V_eff minimum) into the fitted coefficient, forcing V_RG -> 0 at t -> 0 and manufacturing an interior dip for any deeply negative, slowly rising one-loop curve; the positive-one-loop identity control is structurally blind to this artifact class",
        directPrescription = "g(t) = dV_1loop/d ln t by midpoint differences (geometric midpoints), jump-aware (only intervals with unchanged positive/zero Hessian mode counts enter; excluded intervals counted); LSQ of g in the L-image basis {2t^2, 3t^3, 4t^4, 4 t^4 ln t + t^4} where L = t d/dt annihilates constants exactly; CW log coefficient cL supports a resummation minimum only if cL > 0 with implied t* in-range; offset-immunity battery re-derives cL under V_1loop + 1000",
        irConvention = "exact zero modes excluded at zeroTol = max(absFloor, relTol*maxAbsEig); positive eigenvalues enter the one-loop log sum; negative modes recorded",
    },

    // Headline verdicts.
    resolutionKind,
    phase445MinimaResolvedAsFitNormalizationArtifact,
    einsteinianRgSaturationObserved,
    candidateSurvivesSchemeControl,
    replicationComparisonPerformed,
    replicationMatchesPhase445,
    einsteinianAnyInteriorVerbatimRaw,
    offsetSensitivityDemonstrated,
    constantSubtractionKillsInteriorMinima,
    fitSchemeClassificationIrreducible,
    anyMemberSurvivesBasisMenu,
    identityEnrichedSchemeManufacturesMinimum,
    syntheticMonotoneOnGrid,
    syntheticVerbatimSchemeManufacturesMinimum,
    syntheticConstantSubtractionKillsManufacturedMinimum,
    syntheticDirectArmClean,
    directArmAdmissible,
    directCwLogCoefficientPositiveAnyEinsteinian,
    directRgMinimumSupported,
    seedAgreementOnResolution,
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    rgPrescriptionIsWorkbenchConvention,
    physicistReviewPending,

    // Batteries.
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
        thetaResidualBatteryRel = ThetaResidualBatteryRel,
        linThetaFdBattery,
        linThetaFdMaxResidual,
        identityBetaConsistent,
        identityMaxLamTreeRelVariation = FiniteOrNull(identityMaxLamTreeRelVar),
        identityMinLambdaEff = FiniteOrNull(identityMinLamEff),
        controlNoSpuriousMinimumVerbatimScheme,
        rawArgminOffsetInvarianceExact,
        directIntervalFloorMet,
        directOffsetImmunityBatteryPassed,
        directFitQualityMet,
        syntheticMonotoneOnGrid,
        syntheticDirectArmClean,
        evenSubsetMatchesPhase445Grid,
    },

    // Synthetic artifact-sensitive control (provably monotone by construction:
    // tree a t^2 + lam t^4 with a,lam > 0, one-loop C + s ln t with s > 0).
    syntheticControl = new
    {
        treeQuadratic = SynA,
        treeQuartic = SynLam,
        oneLoopConstant = SynC,
        oneLoopLogSlope = SynS,
        provablyMonotone = syntheticMonotoneOnGrid,
        verbatimSchemeManufacturesMinimum = syntheticVerbatimSchemeManufacturesMinimum,
        constantSubtractionKillsManufacturedMinimum = syntheticConstantSubtractionKillsManufacturedMinimum,
        directArmClean = syntheticDirectArmClean,
        cells = synVerbatimCells.Select(CellJson).ToArray(),
        direct = DirectJson(synDirect),
    },

    // Per-member analysis detail.
    memberTable = memberAnalyses.Select(MemberJson).ToArray(),

    // Raw recomputed ray data (persisted so later phases can post-process without recompute).
    rayData = rayDataJson,

    resolutionVerdict = resolutionVerdictPhrase,

    // Recorded boundary (six verbatim keys + the wall set).
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
        phase445SummaryPath = Phase445SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
    },

    explicitCandidateOnlyNonclaims = new[]
    {
        "All V_RG variants, interior minima, t*, and t*/latticeScale values here are candidate-only structure data of the reduced spin-4 slice, NOT physical masses, VEVs, or a scale in GeV.",
        "Every RG-improvement prescription probed here (fit basis, window, grid, normalization) is a WORKBENCH CONVENTION pending physicist review; the phase's purpose is to measure their scheme dependence, not to promote any of them.",
        "The direct CW log coefficient is the constant-immune log-derivative structure of the su(2)-toy one-loop potential; it is not a physical renormalization-group beta function.",
        "A survives-scheme-control outcome would be a workbench-relative dynamical-scale CANDIDATE only; an artifact or unresolved outcome sharpens the frontier. Neither promotes anything.",
        "No VEV scale, pole, or GeV lineage; no Phase201 or Phase256 contract field is filled; the reduced slice does not realize the ambient 7,7 / internal gauge / weld content.",
    },

    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "rg_scheme_dependence_resolution_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "rg_scheme_dependence_resolution_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rgSchemeDependenceResolutionProbePassed={rgSchemeDependenceResolutionProbePassed}");
Console.WriteLine($"precursors: p443={phase443PrecursorPassed} p445={phase445PrecursorPassed}");
Console.WriteLine($"mesh: V={mesh.VertexCount} E={mesh.EdgeCount} F={mesh.FaceCount} C={mesh.CellCount}; nOmega={nOmega} nTheta={nTheta} jointDof={nJoint}");
Console.WriteLine($"grid: N={GridN} tMax={TMax} evenSubsetMatchesPhase445Grid={evenSubsetMatchesPhase445Grid}");
Console.WriteLine($"THETA: gate={thetaStationarityGate} maxResid(non-id)={maxThetaResidualNonIdentity:E2} viaAbsFloor={thetaPointsPassedByAbsFloor} identityThetaExactZero={identityThetaExactZero} linThetaFd={linThetaFdMaxResidual:E2}");
Console.WriteLine($"CONTROL: noSpuriousMinVerbatim={controlNoSpuriousMinimumVerbatimScheme} betaConsistent={identityBetaConsistent} enrichedSchemeManufacturesMin={identityEnrichedSchemeManufacturesMinimum}");
Console.WriteLine($"ARM1 replication: performed={replicationComparisonPerformed} matches={replicationMatchesPhase445} anyInteriorVerbatimRaw={einsteinianAnyInteriorVerbatimRaw}");
Console.WriteLine($"ARM2 offsets: argminInvariantExact={rawArgminOffsetInvarianceExact} fitOffsetSensitive={offsetSensitivityDemonstrated} constSubtractKills={constantSubtractionKillsInteriorMinima}");
Console.WriteLine($"ARM3 menu: irreducible={fitSchemeClassificationIrreducible} anySurvivesMenu={anyMemberSurvivesBasisMenu}");
Console.WriteLine($"SYNTHETIC: monotone={syntheticMonotoneOnGrid} verbatimManufactures={syntheticVerbatimSchemeManufacturesMinimum} constSubKills={syntheticConstantSubtractionKillsManufacturedMinimum} directClean={syntheticDirectArmClean}");
Console.WriteLine($"ARM4 direct: admissible={directArmAdmissible} (floor={directIntervalFloorMet} immune={directOffsetImmunityBatteryPassed} fitQ={directFitQualityMet}) cLPositiveAny={directCwLogCoefficientPositiveAnyEinsteinian} supported={directRgMinimumSupported}");
Console.WriteLine($"RESOLUTION: kind={resolutionKind} artifact={phase445MinimaResolvedAsFitNormalizationArtifact} survives={candidateSurvivesSchemeControl} seedAgreement={seedAgreementOnResolution}");
Console.WriteLine("--- member direct (primary seed) ---");
foreach (var m in memberAnalyses)
{
    var dr = m.Seeds[0].Direct;
    Console.WriteLine($"  {m.Name,-16} usable={dr.UsableIntervals,2} excl={dr.ExcludedIntervals,2} cL={(double.IsFinite(dr.CL) ? dr.CL.ToString("F3") : "n/a"),10} c4={(double.IsFinite(dr.C4) ? dr.C4.ToString("F3") : "n/a"),10} cls={dr.Classification}");
}
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F1}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Local helpers (verbatim Phase443/445).
// ---------------------------------------------------------------------------

double Norm(double[] v)
{
    double s = 0; foreach (double x in v) s += x * x; return System.Math.Sqrt(s);
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
    BranchId = "phase446-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase446",
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

public sealed record VEffPoint(
    double T, double SB, double OneLoop, double VEff,
    int PosCount, int ZeroCount, int NegCount, double ZeroTol,
    double ThetaResidual, double ThetaAbsGradNorm, bool ThetaConverged, int ThetaIterations, int ThetaBlockRank,
    double[] ThetaStar);

public sealed record MemberCurves(
    int Index, string Name, bool IsControl, string Phi1, string Phi2, double EinsteinCoefficient,
    List<VEffPoint[]> SeedCurves);

public sealed record SchemeResult(
    int WindowWidth, string Basis, double[] TCenters,
    double[] LambdaTree, double[] LambdaEff, double[] Vrg, double[] R2OneLoopUncentered,
    string Classification, double TStar, double VStar, double LamTreeRelativeVariation);

public sealed record SchemeCell(
    string Grid, string Basis, int Window, string Offset, string Classification, double TStar);

public sealed record DirectResult(
    int UsableIntervals, int ExcludedIntervals,
    double C4, double CL, double SLog, double TStar, double R2Centered, double R2Uncentered,
    double ResidualRms, bool SupportsMinimum, bool CLPositive, bool FitQualityOk, string Classification);

public sealed record SeedAnalysis(
    SchemeResult[] Replication, SchemeCell[] OffsetCells, SchemeCell[] MenuCells,
    DirectResult Direct, DirectResult DirectShifted, bool DirectOffsetImmune, bool RawArgminOffsetInvariant);

public sealed record MemberAnalysis(
    int Index, string Name, bool IsControl, string Phi1, string Phi2, double EinsteinCoefficient,
    List<SeedAnalysis> Seeds);
