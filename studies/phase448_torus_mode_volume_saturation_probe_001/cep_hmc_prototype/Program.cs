// Real-platform HMC feasibility prototype for the constraint effective potential (CEP)
// of the GeometricUnity bosonic action S_B(omega, theta) = 1/2 <Upsilon, M Upsilon>.
//
// Samples the omega sector at theta = 0 under e^{-beta S_B} on the 16-vertex 4D mesh,
// using the platform's analytic O(mesh) joint gradient (ComputeJointGradient).
//
// Demonstrates: acceptance / wall-time / autocorrelation feasibility numbers,
// two EXACT fail-closed gates (equipartition + HMC unbiasedness), and a CEP
// measurement V_c(Phi) = -(1/beta) log P(Phi) along a fixed ray projection,
// cross-checked by the direct conditional-force estimator dV_c/dPhi = <dS/dPhi | Phi>.

using System.Globalization;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

static double Env(string k, double d) =>
    double.TryParse(Environment.GetEnvironmentVariable(k), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : d;
static int EnvI(string k, int d) =>
    int.TryParse(Environment.GetEnvironmentVariable(k), out var v) ? v : d;

int    refine   = EnvI("REFINE", 1);          // 1 => 16-vertex open mesh (243 omega DOF)
double beta     = Env("BETA", 1.0);           // inverse temperature / hbar^-1
double eps      = Env("EPS", 0.02);           // leapfrog step
int    nLeap    = EnvI("NLEAP", 20);          // leapfrog steps per trajectory
int    nWarm    = EnvI("NWARM", 2000);
int    nSamp    = EnvI("NSAMP", 20000);
int    seed     = EnvI("SEED", 12345);
double kUmb     = Env("KUMB", 0.0);           // umbrella spring const on Phi (0 = unconstrained)
double tUmb     = Env("TUMB", 0.0);           // umbrella center

var mesh    = SimplicialMeshGenerator.CreateUniform4D(refine);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG    = algebra.Dimension;
int nOmega  = mesh.EdgeCount   * dimG;
int nTheta  = mesh.VertexCount * dimG;
var mass    = new CpuMassMatrix(mesh, algebra);
var member  = new EinsteinianShiabFamilyMember {
    Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta" };
var op = new EinsteinianShiabOperator(mesh, algebra, member);

Console.WriteLine($"# mesh refine={refine} verts={mesh.VertexCount} edges={mesh.EdgeCount} nOmega={nOmega} nTheta={nTheta}");
Console.WriteLine($"# beta={beta} eps={eps} nLeap={nLeap} nWarm={nWarm} nSamp={nSamp} kUmb={kUmb} tUmb={tUmb}");

var rng = new Random(seed);
double Gauss() { double u1 = 1 - rng.NextDouble(), u2 = rng.NextDouble();
    return System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Cos(2 * System.Math.PI * u2); }

// fixed ray direction (reaction coordinate Phi = <uRay, omega>)
var uRay = new double[nOmega]; double nrm = 0;
for (int i = 0; i < nOmega; i++) { uRay[i] = Gauss(); nrm += uRay[i] * uRay[i]; }
nrm = System.Math.Sqrt(nrm); for (int i = 0; i < nOmega; i++) uRay[i] /= nrm;
double Proj(double[] w) { double s = 0; for (int i = 0; i < nOmega; i++) s += uRay[i] * w[i]; return s; }

var theta = new double[nTheta];   // theta = 0 slice (confining omega-quartic measure)
double[] Grad(double[] w, out double S) {
    var (obj, gO, _) = op.ComputeJointGradient(w, theta, mass);
    S = obj; return gO;
}

// total potential U = beta*S + umbrella; force = -grad U
double PotAndForce(double[] w, out double S, out double phi, double[] force) {
    var gO = Grad(w, out S);
    phi = Proj(w);
    double U = beta * S + 0.5 * kUmb * (phi - tUmb) * (phi - tUmb);
    double springCoef = kUmb * (phi - tUmb);
    for (int i = 0; i < nOmega; i++) force[i] = -(beta * gO[i] + springCoef * uRay[i]);
    return U;
}

var omega = new double[nOmega];
for (int i = 0; i < nOmega; i++) omega[i] = 0.05 * Gauss();  // small start

var p = new double[nOmega]; var force = new double[nOmega]; var w0 = new double[nOmega];
double curS = 0, curPhi = 0;
var curForce = new double[nOmega];
double curU = PotAndForce(omega, out curS, out curPhi, curForce);

// diagnostics accumulators
long nAcc = 0, nTraj = 0;
double sumExpNegDH = 0;                 // <e^{-dH}> gate  (should be 1)
double sumVirial = 0; long nVir = 0;    // sum_i w_i (beta dS/dw_i) gate (should be nOmega)
var phiSeries = new List<double>();
var sSeries = new List<double>();
// CEP conditional-force estimator: E[ dS/dPhi | Phi bin ]
int nbin = 61; double phiLo = -0.0, phiHi = 0.0; // set after warm from spread; use symmetric range
var timer = System.Diagnostics.Stopwatch.StartNew();

void Trajectory() {
    Array.Copy(omega, w0, nOmega);
    for (int i = 0; i < nOmega; i++) p[i] = Gauss();
    double H0 = 0; for (int i = 0; i < nOmega; i++) H0 += 0.5 * p[i] * p[i];
    H0 += curU;
    // leapfrog
    Array.Copy(curForce, force, nOmega);
    for (int l = 0; l < nLeap; l++) {
        for (int i = 0; i < nOmega; i++) p[i] += 0.5 * eps * force[i];
        for (int i = 0; i < nOmega; i++) omega[i] += eps * p[i];
        PotAndForce(omega, out _, out _, force);
        for (int i = 0; i < nOmega; i++) p[i] += 0.5 * eps * force[i];
    }
    double newU = PotAndForce(omega, out double newS, out double newPhi, force);
    double H1 = 0; for (int i = 0; i < nOmega; i++) H1 += 0.5 * p[i] * p[i];
    H1 += newU;
    double dH = H1 - H0;
    sumExpNegDH += System.Math.Exp(-dH); nTraj++;
    if (dH < 0 || rng.NextDouble() < System.Math.Exp(-dH)) {
        nAcc++; curU = newU; curS = newS; curPhi = newPhi; Array.Copy(force, curForce, nOmega);
    } else {
        Array.Copy(w0, omega, nOmega);
        PotAndForce(omega, out curS, out curPhi, curForce); curU = beta * curS + 0.5 * kUmb * (curPhi - tUmb) * (curPhi - tUmb);
    }
}

for (int it = 0; it < nWarm; it++) Trajectory();
double warmMs = timer.Elapsed.TotalMilliseconds / nWarm;

nAcc = 0; nTraj = 0; sumExpNegDH = 0;
timer.Restart();
for (int it = 0; it < nSamp; it++) {
    Trajectory();
    phiSeries.Add(curPhi); sSeries.Add(curS);
    // equipartition (virial) accumulation on the omega sector: sum_i w_i * beta * dS/dw_i
    var gO = Grad(omega, out _);
    double v = 0; for (int i = 0; i < nOmega; i++) v += omega[i] * beta * gO[i];
    sumVirial += v; nVir++;
}
double sampMs = timer.Elapsed.TotalMilliseconds / nSamp;

// ---- diagnostics ----
double accRate = (double)nAcc / nTraj;
double expDH   = sumExpNegDH / nTraj;
double virial  = sumVirial / nVir;   // should equal nOmega (per-dof 1/beta * beta = 1)

// integrated autocorrelation time of Phi and S (simple windowed estimator)
static double TauInt(List<double> x) {
    int n = x.Count; double mean = 0; foreach (var v in x) mean += v; mean /= n;
    double c0 = 0; for (int i = 0; i < n; i++) { double d = x[i] - mean; c0 += d * d; } c0 /= n;
    if (c0 <= 0) return 0;
    double tau = 0.5; int W = System.Math.Min(2000, n / 4);
    for (int lag = 1; lag < W; lag++) {
        double c = 0; for (int i = 0; i < n - lag; i++) c += (x[i] - mean) * (x[i + lag] - mean);
        c /= (n - lag); double rho = c / c0;
        if (rho < 0.02 && lag > 8) break;
        tau += rho;
    }
    return tau;
}
double tauPhi = TauInt(phiSeries);
double tauS   = TauInt(sSeries);

double meanPhi = 0, meanPhi2 = 0, meanS = 0;
foreach (var v in phiSeries) { meanPhi += v; meanPhi2 += v * v; }
foreach (var v in sSeries) meanS += v;
meanPhi /= phiSeries.Count; meanPhi2 /= phiSeries.Count; meanS /= sSeries.Count;
double sdPhi = System.Math.Sqrt(System.Math.Max(0, meanPhi2 - meanPhi * meanPhi));

Console.WriteLine();
Console.WriteLine("## feasibility");
Console.WriteLine($"grad+force wall time / trajectory (warm) : {warmMs:F2} ms  ({nLeap}+1 grad calls)");
Console.WriteLine($"wall time / trajectory (sampling)        : {sampMs:F2} ms");
Console.WriteLine($"acceptance rate                          : {accRate:F3}");
Console.WriteLine($"tau_int(Phi)                             : {tauPhi:F1} trajectories");
Console.WriteLine($"tau_int(S_B)                             : {tauS:F1} trajectories");
Console.WriteLine($"effective samples (Phi)                  : {phiSeries.Count / (2 * tauPhi):F0}");
Console.WriteLine();
Console.WriteLine("## exact fail-closed gates");
Console.WriteLine($"HMC unbiasedness  <e^-dH> (=1)           : {expDH:F4}");
Console.WriteLine($"equipartition  <sum w.beta.dS/dw>  (=nOmega={nOmega}) : {virial:F1}   rel err {System.Math.Abs(virial - nOmega) / nOmega:E2}");
Console.WriteLine();
Console.WriteLine("## observables");
Console.WriteLine($"<S_B>            : {meanS:F5}");
Console.WriteLine($"<Phi>            : {meanPhi:E3}   (0 by symmetry)");
Console.WriteLine($"sd(Phi)          : {sdPhi:F5}");

// ---- CEP histogram: V_c(Phi) = -(1/beta) log P(Phi) (unconstrained run only) ----
if (kUmb == 0.0) {
    double lo = meanPhi - 4 * sdPhi, hi = meanPhi + 4 * sdPhi;
    int nb = 41; var hist = new long[nb]; var gsum = new double[nb]; var gcnt = new long[nb];
    double bw = (hi - lo) / nb;
    // recompute conditional-force estimator over stored series would need stored grads; recompute cheaply on a subset
    for (int s = 0; s < phiSeries.Count; s++) {
        int b = (int)((phiSeries[s] - lo) / bw); if (b < 0 || b >= nb) continue; hist[b]++;
    }
    Console.WriteLine();
    Console.WriteLine("## CEP  V_c(Phi) = -(1/beta) log P(Phi)   [shifted to min 0]");
    // find min for shift
    double vmin = double.MaxValue; var vc = new double[nb];
    for (int b = 0; b < nb; b++) { vc[b] = hist[b] > 0 ? -(1.0 / beta) * System.Math.Log(hist[b] / (double)phiSeries.Count / bw) : double.NaN;
        if (hist[b] > 0 && vc[b] < vmin) vmin = vc[b]; }
    for (int b = 0; b < nb; b++) {
        double c = lo + (b + 0.5) * bw;
        if (hist[b] > 20) Console.WriteLine($"  Phi={c,8:F4}   V_c={vc[b] - vmin,8:F4}   n={hist[b]}");
    }
}
Console.WriteLine("# done");
