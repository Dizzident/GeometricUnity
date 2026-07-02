using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase439: gap-equation-in-lambda_8-background channel-steering probe.
//
// Two prior results set up the question this phase decides:
//  - Phase438 solved the mean-field (Gross-Neveu-style) gap equation on the 2D
//    doubler-excluded lattice and found it DOES generate a dynamical scale, but
//    the SCALAR SINGLET condensate beats the hypercharge-direction condensate
//    at every coupling: scale and breaking direction lived in different
//    mechanisms.
//  - Phase431 found that a constant lambda_8 gauge background
//    omega_bg = t8*(lambda_8/2) breaks the su(3) degeneracy of the fermion
//    spectrum (it commutes with the triplet, breaks the doublet).
//
// THE QUESTION: does solving the gap equation INSIDE the lambda_8 background
// STEER the condensate channel - i.e. does the background make a
// symmetry-breaking (non-singlet) condensate channel competitive with or
// preferred over the singlet, and does it split the condensate BY color
// component (a color-dependent gap = dynamically generated su(3)-breaking mass
// structure)?
//
// Construction (workbench conventions exactly as Phase438): 2D L x L lattice,
// L in {4,8,16}; 4-dim spinors; naive central-difference Dirac; IR
// doubler-exclusion convention (zero-dispersion t8=0 modes dropped, SAME
// excluded set at every t8 so the t8=0 limit is exactly Phase438); su(3)
// Gell-Mann; contents fundamental (1 copy) and derived-4x (4 copies). The
// Phase431 background omega_bg = t8*(lambda_8/2) is minimally coupled on BOTH
// lattice directions: per momentum the color-c block is
//   gamma_mu (x) (s_mu + t8*u_c),   u_c = eigenvalues of lambda_8/2,
// so the color-c dispersion is eps_c(k,t8)^2 = (s1+t8 u_c)^2 + (s2+t8 u_c)^2.
// Colors 1,2 have u_c = 1/(2 sqrt3); color 3 has u_c = -1/sqrt3.
//
// The mass insertion Gamma = sz (x) I2 (x) C anticommutes with the kinetic AND
// background gamma_mu terms on the spinor factor, and diagonal C commutes with
// U8 on color, so
//   H^2 = (kinetic + background)^2 + Sigma^2 C^2   EXACTLY,
// giving the closed form lambda^2 = eps_c(k,t8)^2 + Sigma^2 c_c^2 per color c
// (c_c the eigenvalues of C). Verified against a dense L=4 Dirac solve
// (residual reported) for singlet, hypercharge, and a general diagonal C.
//
// THE KEY NOVELTY - the FREE-DIAGONAL minimization: instead of a fixed channel
// matrix C, minimize the free energy over INDEPENDENT per-color gaps
// (Sigma_1, Sigma_2, Sigma_3) with penalty sum_c Sigma_c^2/(2 g2):
//   W(Sigma_vec) = sum_c [ Sigma_c^2/(2 g2)
//                          - copies*(Ns/2)/Vol sum_k' log(eps_c(k,t8)^2 + Sigma_c^2) ].
// This DECOUPLES per color: each Sigma_c solves its own gap equation with the
// background-shifted dispersion eps_c. At t8 > 0 the dispersions differ by
// color, so the gap equation gives DIFFERENT Sigma_c per color - and because
// colors 1,2 share u_c while color 3 differs, the induced pattern
// Sigma_1 = Sigma_2 != Sigma_3 is an su(3)->su(2)xu(1)-ALIGNED dynamical mass.
//
// Findings (machine-verified below): at t8 > 0 the free-diagonal optimum beats
// the singlet-constrained (all-equal) optimum in free energy by a strictly
// positive margin (channel steering); the free minimum has Sigma_1 = Sigma_2
// != Sigma_3 exactly (su(2)xu(1) alignment); the color gap splitting ratio
// (Sigma_1-Sigma_3)/(Sigma_1+Sigma_3) rises monotonically with t8; and the
// exponential scale law ln Sigma_c ~ -c/g2 survives the background per color.
//
// Fail-closed: t8 is a RECORDED CANDIDATE background parameter (not dynamically
// derived - the Phase430-chain caveat); the gap equation is a mean-field
// approximation; the four-fermion coupling normalization is a workbench
// convention; the color-gap scale and pattern are candidate-only; nothing is
// promoted to GeV and no Phase201 or Phase256 field is filled.

const string DefaultOutputDir = "studies/phase439_gap_equation_lambda8_background_channel_steering_probe_001/output";
const string Phase431SummaryPath = "studies/phase431_lambda8_background_doublet_reopening_probe_001/output/lambda8_background_doublet_reopening_probe_summary.json";
const string Phase438SummaryPath = "studies/phase438_self_consistent_condensate_gap_equation_probe_001/output/self_consistent_condensate_gap_equation_probe_summary.json";
const string ApplicationSubjectKind = "gap-equation-lambda8-background-channel-steering-probe";

var startTime = DateTimeOffset.UtcNow;
var outputDir = Environment.GetEnvironmentVariable("PHASE439_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase431 = JsonDocument.Parse(File.ReadAllText(Phase431SummaryPath));
using var phase438 = JsonDocument.Parse(File.ReadAllText(Phase438SummaryPath));

bool phase431PrecursorPassed =
    JsonBool(phase431.RootElement, "lambda8BackgroundDoubletReopeningProbePassed") is true &&
    JsonBool(phase431.RootElement, "tdDegeneracyBrokenByBackground") is true;
bool phase438PrecursorPassed =
    JsonBool(phase438.RootElement, "selfConsistentCondensateGapEquationProbePassed") is true &&
    JsonBool(phase438.RootElement, "dynamicalScaleGenerationObserved") is true &&
    JsonBool(phase438.RootElement, "hyperchargeChannelCompetitiveWithSinglet") is false;
bool precursorsPassed = phase431PrecursorPassed && phase438PrecursorPassed;

// ---------------------------------------------------------------------------
// Gamma matrices: gamma1 = sx (x) I2, gamma2 = sy (x) I2, Gamma = sz (x) I2.
// ---------------------------------------------------------------------------

var sx = new Complex[2, 2]; sx[0, 1] = 1; sx[1, 0] = 1;
var sy = new Complex[2, 2]; sy[0, 1] = -Complex.ImaginaryOne; sy[1, 0] = Complex.ImaginaryOne;
var sz = new Complex[2, 2]; sz[0, 0] = 1; sz[1, 1] = -1;
var gamma1 = Kron2(sx);
var gamma2 = Kron2(sy);
var gammaMass = Kron2(sz);

double gammaAnticommutationResidual = Math.Max(Math.Max(
    MaxAbs(AntiComm(gamma1, gammaMass)),
    MaxAbs(AntiComm(gamma2, gammaMass))),
    Math.Max(MaxAbs(AntiComm(gamma1, gamma2)),
        Math.Max(Math.Max(MaxAbs(Sub(MatMul(gamma1, gamma1), Identity(4))),
                          MaxAbs(Sub(MatMul(gamma2, gamma2), Identity(4)))),
                 MaxAbs(Sub(MatMul(gammaMass, gammaMass), Identity(4))))));
bool gammaAnticommutationExact = gammaAnticommutationResidual <= 1e-14;

// ---------------------------------------------------------------------------
// su(3) mode structure and the lambda_8 color eigenvalues u_c.
// ---------------------------------------------------------------------------

const int Nc = 3;
const int Ns = 4;
double[] u8 = { 1.0 / (2.0 * Math.Sqrt(3.0)), 1.0 / (2.0 * Math.Sqrt(3.0)), -1.0 / Math.Sqrt(3.0) };
double sumU8Squared = u8.Sum(u => u * u); // = 1/2
bool hyperColorChargesConsistent = Math.Abs(sumU8Squared - 0.5) <= 1e-12;
double lambdaCutoff = Math.Sqrt(2.0);

int[] lattices = { 4, 8, 16 };
double[] t8Grid = { 0.0, 0.5, 1.0, 2.0 };
var g2Grid = LogSpace(0.1, 10.0, 20);
double[] competitionG2 = { 1.0, 2.0, 5.0, 10.0 };
var contents = new (string Name, int Copies)[] { ("fundamental", 1), ("derived-4x", 4) };

// ---------------------------------------------------------------------------
// Background-shifted color dispersions (doubler-excluded, SAME excluded set at
// every t8: exclude momenta whose t8=0 dispersion s1^2+s2^2 <= 1e-12).
// ---------------------------------------------------------------------------

double[] Eps2Color(int L, double t8, double uc)
{
    var list = new List<double>();
    for (int n1 = 0; n1 < L; n1++)
        for (int n2 = 0; n2 < L; n2++)
        {
            double s1 = Math.Sin(2.0 * Math.PI * n1 / L);
            double s2 = Math.Sin(2.0 * Math.PI * n2 / L);
            if (s1 * s1 + s2 * s2 <= 1e-12) continue; // doubler-exclusion at t8=0, kept for all t8
            double a1 = s1 + t8 * uc, a2 = s2 + t8 * uc;
            list.Add(a1 * a1 + a2 * a2);
        }
    return list.ToArray();
}
int ExcludedCount(int L)
{
    int excl = 0;
    for (int n1 = 0; n1 < L; n1++)
        for (int n2 = 0; n2 < L; n2++)
        {
            double s1 = Math.Sin(2.0 * Math.PI * n1 / L);
            double s2 = Math.Sin(2.0 * Math.PI * n2 / L);
            if (s1 * s1 + s2 * s2 <= 1e-12) excl++;
        }
    return excl;
}

double[][] Eps2AllColors(int L, double t8) => u8.Select(u => Eps2Color(L, t8, u)).ToArray();

// ---------------------------------------------------------------------------
// Free-diagonal gap machinery. Each color c solves its own gap equation:
//   1 = g2*copies*(Ns/Vol) sum_k' 1/(eps_c^2 + Sigma_c^2).
// ---------------------------------------------------------------------------

double GapColor(double sigma, double g2, double[] e2c, double vol, int copies)
{
    double s = 0.0;
    foreach (double e in e2c) s += 1.0 / (e + sigma * sigma);
    return 1.0 - g2 * copies * (Ns / vol) * s;
}
double G2CritColor(double[] e2c, double vol, int copies) =>
    vol / (copies * Ns * e2c.Sum(e => 1.0 / e));

// singlet-constrained (Sigma_1=Sigma_2=Sigma_3=Sigma) within the SAME free-diagonal
// functional; its gap: 3 = g2*copies*(Ns/Vol) sum_c sum_k' 1/(eps_c^2 + Sigma^2).
double GapConstrained(double sigma, double g2, double[][] e2, double vol, int copies)
{
    double s = 0.0;
    foreach (var e2c in e2)
        foreach (double e in e2c) s += 1.0 / (e + sigma * sigma);
    return 3.0 - g2 * copies * (Ns / vol) * s;
}

// free-diagonal free energy per volume (coeff copies*(Ns/2) per (color, momentum)).
double WFreeDiagonal(double[] sigVec, double g2, double[][] e2, double vol, int copies)
{
    double w = 0.0;
    for (int c = 0; c < Nc; c++)
    {
        double fer = 0.0;
        foreach (double e in e2[c]) fer += Math.Log(e + sigVec[c] * sigVec[c]);
        w += sigVec[c] * sigVec[c] / (2.0 * g2) - copies * (Ns / 2.0) * fer / vol;
    }
    return w;
}

// channel (a) singlet single-Sigma (Phase438-reduction consistency at t8=0), c_c=1.
double GapSingletA(double sigma, double g2, double[][] e2, double vol, int copies)
{
    double s = 0.0;
    foreach (var e2c in e2)
        foreach (double e in e2c) s += 1.0 / (e + sigma * sigma);
    return 1.0 - g2 * copies * (Ns / vol) * s;
}
// channel (b) hypercharge single-Sigma, c_c = u_c.
double GapHyperB(double sigma, double g2, double[][] e2, double vol, int copies)
{
    double s = 0.0;
    for (int c = 0; c < Nc; c++)
    {
        double uu = u8[c] * u8[c];
        foreach (double e in e2[c]) s += uu / (e + sigma * sigma * uu);
    }
    return 1.0 - g2 * copies * (Ns / vol) * s;
}

double SolveSigma(Func<double, double> f)
{
    if (f(1e-9) >= 0.0) return 0.0;
    double lo = 1e-9, hi = 1e-3;
    while (f(hi) < 0.0) { hi *= 2.0; if (hi > 1e6) break; }
    for (int i = 0; i < 200; i++)
    {
        double mid = 0.5 * (lo + hi);
        if (f(mid) < 0.0) lo = mid; else hi = mid;
    }
    return 0.5 * (lo + hi);
}

double[] SolveColorGaps(double g2, double[][] e2, double vol, int copies) =>
    Enumerable.Range(0, Nc)
        .Select(c => SolveSigma(s => GapColor(s, g2, e2[c], vol, copies)))
        .ToArray();

// ---------------------------------------------------------------------------
// Battery: closed-form spectrum eps_c^2 + Sigma^2 c_c^2 vs a dense L=4 Dirac
// solve including the lambda_8 background, for singlet / hyper / general C.
// ---------------------------------------------------------------------------

double DenseClosedFormResidual(double t8, double[] massColor)
{
    const int L = 4;
    int vol = L * L, dim = 4 * vol * Nc;
    double sigma = 0.7;
    var hop = new double[2][,];
    for (int mu = 0; mu < 2; mu++)
    {
        hop[mu] = new double[vol, vol];
        for (int x = 0; x < L; x++)
            for (int y = 0; y < L; y++)
            {
                int i = x + L * y;
                int j = mu == 0 ? ((x + 1) % L) + L * y : x + L * ((y + 1) % L);
                hop[mu][j, i] += 0.5;
                hop[mu][i, j] -= 0.5;
            }
    }
    var gam = new[] { gamma1, gamma2 };
    var dirac = new Complex[dim, dim];
    // kinetic: gamma_mu (x) (i hop_mu) (x) I_color
    for (int mu = 0; mu < 2; mu++)
        for (int s = 0; s < 4; s++)
            for (int sp = 0; sp < 4; sp++)
            {
                if (gam[mu][s, sp] == Complex.Zero) continue;
                for (int v = 0; v < vol; v++)
                    for (int vp = 0; vp < vol; vp++)
                        if (Math.Abs(hop[mu][v, vp]) > 1e-15)
                            for (int g = 0; g < Nc; g++)
                                dirac[(s * vol + v) * Nc + g, (sp * vol + vp) * Nc + g] +=
                                    gam[mu][s, sp] * Complex.ImaginaryOne * hop[mu][v, vp];
            }
    // background: gamma_mu (x) (t8 U8) (x) I_site, U8 = diag(u8), both directions.
    for (int mu = 0; mu < 2; mu++)
        for (int s = 0; s < 4; s++)
            for (int sp = 0; sp < 4; sp++)
            {
                if (gam[mu][s, sp] == Complex.Zero) continue;
                for (int v = 0; v < vol; v++)
                    for (int g = 0; g < Nc; g++)
                        dirac[(s * vol + v) * Nc + g, (sp * vol + v) * Nc + g] +=
                            gam[mu][s, sp] * t8 * u8[g];
            }
    // mass: Sigma * Gamma (x) I_site (x) C  (C diagonal = massColor).
    for (int s = 0; s < 4; s++)
        for (int sp = 0; sp < 4; sp++)
        {
            if (gammaMass[s, sp] == Complex.Zero) continue;
            for (int v = 0; v < vol; v++)
                for (int g = 0; g < Nc; g++)
                    dirac[(s * vol + v) * Nc + g, (sp * vol + v) * Nc + g] +=
                        sigma * gammaMass[s, sp] * massColor[g];
        }
    var (denseValues, _) = Jacobi(Realify(dirac));
    var denseSquared = denseValues.Select(v => v * v).OrderBy(v => v).ToArray();
    var denseUndoubled = new double[denseSquared.Length / 2];
    for (int i = 0; i < denseUndoubled.Length; i++)
        denseUndoubled[i] = denseSquared[2 * i];
    var closed = new List<double>();
    for (int n1 = 0; n1 < L; n1++)
        for (int n2 = 0; n2 < L; n2++)
        {
            double s1 = Math.Sin(2.0 * Math.PI * n1 / L), s2 = Math.Sin(2.0 * Math.PI * n2 / L);
            for (int c = 0; c < Nc; c++)
            {
                double a1 = s1 + t8 * u8[c], a2 = s2 + t8 * u8[c];
                double l2 = a1 * a1 + a2 * a2 + sigma * sigma * massColor[c] * massColor[c];
                for (int sp = 0; sp < 4; sp++) closed.Add(l2);
            }
        }
    var closedArr = closed.OrderBy(v => v).ToArray();
    double resid = 0.0;
    for (int i = 0; i < closedArr.Length; i++)
        resid = Math.Max(resid, Math.Abs(denseUndoubled[i] - closedArr[i]));
    return resid;
}

double[] singletC = { 1.0, 1.0, 1.0 };
double[] generalC = { 0.3, 0.3, 0.9 };
var denseBatteryRows = new List<(string Channel, double T8, double Residual)>
{
    ("singlet", 0.0, DenseClosedFormResidual(0.0, singletC)),
    ("singlet", 1.0, DenseClosedFormResidual(1.0, singletC)),
    ("hyper",   1.0, DenseClosedFormResidual(1.0, u8)),
    ("general", 1.0, DenseClosedFormResidual(1.0, generalC)),
};
double closedFormMaxResidual = denseBatteryRows.Max(r => r.Residual);
bool closedFormSpectrumMatchesDense = closedFormMaxResidual <= 1e-9;

// ---------------------------------------------------------------------------
// Battery: t8=0 Phase438 consistency. Channel (a) singlet and (b) hyper
// single-Sigma solutions must reproduce the Phase438 gap solutions exactly.
// ---------------------------------------------------------------------------

double phase438SingletResidual = 0.0, phase438HyperResidual = 0.0;
foreach (int L in lattices)
{
    double vol = L * L; int copies = 1; double g2 = 3.0;
    double[][] e2 = Eps2AllColors(L, 0.0);
    double[] e2single = Eps2Color(L, 0.0, u8[0]); // unshifted dispersion
    double sigA = SolveSigma(s => GapSingletA(s, g2, e2, vol, copies));
    // Phase438 GapSinglet reference: 1 = g2*copies*(Nc*Ns/vol) sum_e 1/(e+sig^2).
    double sigRefS = SolveSigma(s => 1.0 - g2 * copies * (Nc * Ns / vol) * e2single.Sum(e => 1.0 / (e + s * s)));
    phase438SingletResidual = Math.Max(phase438SingletResidual, Math.Abs(sigA - sigRefS));
    double sigB = SolveSigma(s => GapHyperB(s, g2, e2, vol, copies));
    // Phase438 GapHyper reference: 1 = g2*copies*(Ns/vol) sum_c u_c^2 sum_e 1/(e+sig^2 u_c^2).
    double sigRefH = SolveSigma(s =>
    {
        double acc = 0.0;
        foreach (double u in u8) { double uu = u * u; acc += uu * e2single.Sum(e => 1.0 / (e + s * s * uu)); }
        return 1.0 - g2 * copies * (Ns / vol) * acc;
    });
    phase438HyperResidual = Math.Max(phase438HyperResidual, Math.Abs(sigB - sigRefH));
}
bool phase438ConsistencyAtZeroBackground = phase438SingletResidual <= 1e-10 && phase438HyperResidual <= 1e-10;

// ---------------------------------------------------------------------------
// Core scan: per (content, L, g2 in competitionG2, t8) solve the free-diagonal
// color gaps and the singlet-constrained gap, record free energies, steering
// margin, and the color-splitting ratio.
// ---------------------------------------------------------------------------

var steeringRows = new List<SteeringRow>();
double maxS1S2Diff = 0.0;
double minSteeringMarginAtNonzeroT8 = double.MaxValue;
double maxMarginAtZeroBackground = 0.0;
double maxSplittingRatioAtNonzeroT8 = 0.0;
bool splittingMonotoneEverywhere = true;

foreach (var (contentName, copies) in contents)
    foreach (int L in lattices)
        foreach (double g2 in competitionG2)
        {
            double vol = L * L;
            var ratiosByT8 = new List<double>();
            foreach (double t8 in t8Grid)
            {
                double[][] e2 = Eps2AllColors(L, t8);
                double[] sig = SolveColorGaps(g2, e2, vol, copies);
                double sigConstr = SolveSigma(s => GapConstrained(s, g2, e2, vol, copies));
                double wFree = WFreeDiagonal(sig, g2, e2, vol, copies);
                double wConstr = WFreeDiagonal(new[] { sigConstr, sigConstr, sigConstr }, g2, e2, vol, copies);
                double margin = wConstr - wFree;
                double denom = sig[0] + sig[2];
                double ratio = denom > 0 ? (sig[0] - sig[2]) / denom : 0.0;
                bool nonzero = sig[0] > 1e-6;
                bool steers = margin > 1e-9;

                maxS1S2Diff = Math.Max(maxS1S2Diff, Math.Abs(sig[0] - sig[1]));
                ratiosByT8.Add(ratio);
                if (t8 == 0.0)
                    maxMarginAtZeroBackground = Math.Max(maxMarginAtZeroBackground, Math.Abs(margin));
                else
                {
                    maxSplittingRatioAtNonzeroT8 = Math.Max(maxSplittingRatioAtNonzeroT8, ratio);
                    if (nonzero) minSteeringMarginAtNonzeroT8 = Math.Min(minSteeringMarginAtNonzeroT8, margin);
                }

                steeringRows.Add(new SteeringRow(contentName, copies, L, g2, t8,
                    sig[0], sig[1], sig[2], sigConstr, wFree, wConstr, margin, ratio, nonzero, steers));
            }
            for (int i = 1; i < ratiosByT8.Count; i++)
                if (ratiosByT8[i] < ratiosByT8[i - 1] - 1e-12) splittingMonotoneEverywhere = false;
        }

bool freeDiagonalAlignmentExact = maxS1S2Diff <= 1e-10;

// verdict: strict steering at t8>0 with quantified margin, zero at t8=0.
bool backgroundInducesChannelSteering =
    minSteeringMarginAtNonzeroT8 > 1e-8 && maxMarginAtZeroBackground <= 1e-10;
// verdict: Sigma_1=Sigma_2 (alignment) AND a genuine split from Sigma_3 at t8>0.
bool dynamicalMassPatternAlignsWithSu2U1 =
    freeDiagonalAlignmentExact && maxSplittingRatioAtNonzeroT8 >= 1e-4;
bool colorGapSplittingMonotoneInBackground = splittingMonotoneEverywhere;

// ---------------------------------------------------------------------------
// Per-color critical coupling vs t8 (does the background raise or lower it?).
// ---------------------------------------------------------------------------

var critRows = new List<CritRow>();
foreach (var (contentName, copies) in contents)
    foreach (int L in lattices)
        foreach (double t8 in t8Grid)
        {
            double vol = L * L;
            double[][] e2 = Eps2AllColors(L, t8);
            double gc1 = G2CritColor(e2[0], vol, copies);
            double gc3 = G2CritColor(e2[2], vol, copies);
            critRows.Add(new CritRow(contentName, copies, L, t8, gc1, gc3));
        }
// honest recorded signature: at moderate t8=1.0 the larger-|u_c| color (color 3)
// reaches a LOWER critical coupling than color 1, and both are below the t8=0 value.
bool backgroundLowersColorCriticalCouplingAtModerateT8 = true;
foreach (var (contentName, copies) in contents)
    foreach (int L in lattices)
    {
        double gc0 = critRows.First(r => r.Content == contentName && r.L == L && r.T8 == 0.0).G2Crit1;
        var r1 = critRows.First(r => r.Content == contentName && r.L == L && r.T8 == 1.0);
        backgroundLowersColorCriticalCouplingAtModerateT8 &=
            r1.G2Crit3 < r1.G2Crit1 && r1.G2Crit1 < gc0;
    }

// ---------------------------------------------------------------------------
// Scale-law survival: ln Sigma_c vs 1/g2 per (content, L, t8>0, color) over the
// full g2 grid.
// ---------------------------------------------------------------------------

var scaleLawRows = new List<ScaleLawRow>();
foreach (var (contentName, copies) in contents)
    foreach (int L in lattices)
        foreach (double t8 in t8Grid.Where(t => t > 0.0))
            foreach (int color in new[] { 0, 2 }) // color 1 (=2) and color 3 representatives
            {
                double vol = L * L;
                double[] e2c = Eps2Color(L, t8, u8[color]);
                var xs = new List<double>(); var ys = new List<double>();
                foreach (double g2 in g2Grid)
                {
                    double sig = SolveSigma(s => GapColor(s, g2, e2c, vol, copies));
                    if (sig > 1e-9) { xs.Add(1.0 / g2); ys.Add(Math.Log(sig)); }
                }
                var (slope, intercept, r2) = LinFit(xs.ToArray(), ys.ToArray());
                scaleLawRows.Add(new ScaleLawRow(contentName, copies, L, t8, color + 1, slope, intercept, r2, xs.Count));
            }
bool allScaleLawNegativeSlope = scaleLawRows.All(r => r.Slope < 0.0);
bool fundamentalScaleLawLinear =
    scaleLawRows.Where(r => r.Content == "fundamental").All(r => r.R2 >= 0.8);
double scaleLawR2Min = scaleLawRows.Min(r => r.R2);
double scaleLawR2Max = scaleLawRows.Max(r => r.R2);
bool transmutationSignatureSurvivesBackground = allScaleLawNegativeSlope && fundamentalScaleLawLinear;

// ---------------------------------------------------------------------------
// Batteries roll-up.
// ---------------------------------------------------------------------------

bool allBatteriesPassed =
    gammaAnticommutationExact &&
    hyperColorChargesConsistent &&
    closedFormSpectrumMatchesDense &&
    phase438ConsistencyAtZeroBackground &&
    freeDiagonalAlignmentExact;

// ---------------------------------------------------------------------------
// Honest boundaries and fail-closed contract block.
// ---------------------------------------------------------------------------

const bool gapEquationIsMeanFieldApproximation = true;
const bool fourFermionCouplingNormalizationIsRecordedConvention = true;
const bool backgroundParameterT8IsRecordedCandidateOnly = true;
const bool backgroundParameterT8DynamicallyDerived = false;
const bool scaleAndPatternAreCandidateOnly = true;
const bool noGevPromotion = true;

const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool workbenchConventionsAreSourceDefined = false;
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
    "2D doubler-excluded lattice Gross-Neveu gap equation solved inside a candidate lambda_8 background t8*(lambda_8/2); free-diagonal per-color gaps; singlet/hyper/general channels; scanned coupling; no target values")))).ToLowerInvariant();

bool backgroundSteersCondensateChannelOnWorkbench =
    backgroundInducesChannelSteering &&
    dynamicalMassPatternAlignsWithSu2U1 &&
    colorGapSplittingMonotoneInBackground &&
    transmutationSignatureSurvivesBackground;

bool gapEquationLambda8BackgroundChannelSteeringProbePassed =
    precursorsPassed &&
    allBatteriesPassed &&
    backgroundInducesChannelSteering &&
    dynamicalMassPatternAlignsWithSu2U1 &&
    colorGapSplittingMonotoneInBackground &&
    transmutationSignatureSurvivesBackground &&
    backgroundSteersCondensateChannelOnWorkbench &&
    gapEquationIsMeanFieldApproximation &&
    fourFermionCouplingNormalizationIsRecordedConvention &&
    backgroundParameterT8IsRecordedCandidateOnly &&
    !backgroundParameterT8DynamicallyDerived &&
    scaleAndPatternAreCandidateOnly &&
    noGevPromotion &&
    targetBlindConstruction &&
    !physicalTargetsConsultedForConstruction &&
    !workbenchConventionsAreSourceDefined &&
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

string terminalStatus = gapEquationLambda8BackgroundChannelSteeringProbePassed
    ? "lambda8-background-steers-gap-equation-into-su2u1-aligned-color-split-condensate-candidate-only-no-gev-promotion"
    : "gap-equation-lambda8-background-channel-steering-probe-blocked";

string decision = gapEquationLambda8BackgroundChannelSteeringProbePassed
    ? "Solving the mean-field gap equation INSIDE a candidate lambda_8 background steers the condensate channel on the workbench. When the per-color gaps (Sigma_1, Sigma_2, Sigma_3) are minimized independently in the background-shifted dispersion eps_c(k,t8)^2 = (s1+t8 u_c)^2+(s2+t8 u_c)^2, the free-diagonal optimum beats the singlet-constrained (all-equal) optimum in free energy by a STRICTLY POSITIVE margin at every t8 > 0 (minimum sampled margin " + $"{minSteeringMarginAtNonzeroT8:E3}" + "), while the margin is exactly zero at t8 = 0 (max " + $"{maxMarginAtZeroBackground:E3}" + ") - so the steering is caused by the background, not the machinery. The induced pattern is Sigma_1 = Sigma_2 != Sigma_3 to machine precision (|Sigma_1-Sigma_2| <= " + $"{maxS1S2Diff:E1}" + "): a dynamically generated su(3)->su(2)xu(1)-ALIGNED mass structure, since colors 1,2 share u_c = 1/(2 sqrt3) while color 3 has u_c = -1/sqrt3. The color-gap splitting ratio (Sigma_1-Sigma_3)/(Sigma_1+Sigma_3) rises MONOTONICALLY with t8 across every content/lattice/coupling sampled, and the exponential dimensional-transmutation scale law ln Sigma_c ~ -c/g2 SURVIVES the background per color (negative slope in every config; fundamental-content R^2 in [" + $"{scaleLawR2Min:F3}, {scaleLawR2Max:F3}" + "]). Batteries: the closed form eps_c^2 + Sigma^2 c_c^2 matches a dense L=4 Dirac solve WITH the background to " + $"{closedFormMaxResidual:E1}" + " for singlet, hypercharge, and general diagonal channels; at t8 = 0 the singlet and hypercharge channels reproduce the Phase438 gap solutions to " + $"{Math.Max(phase438SingletResidual, phase438HyperResidual):E1}" + ". HONEST BOUNDARIES: t8 is a RECORDED CANDIDATE background parameter, not a dynamically derived scale (deriving it is the Phase430 chain's job); the gap equation is a mean-field approximation; the four-fermion coupling normalization is a workbench convention; the color-gap scale and the su(2)xu(1) pattern are candidate-only. This phase decides that the background CAN steer the channel and align the pattern on the workbench; it closes no scale gap, promotes nothing to GeV, and fills no Phase201 or Phase256 field."
    : "Do not use the channel-steering verdicts until the precursor and battery gates pass.";

var runtimeSeconds = (DateTimeOffset.UtcNow - startTime).TotalSeconds;

var result = new
{
    phaseId = "phase439-gap-equation-lambda8-background-channel-steering-probe",
    generatedAt = DateTimeOffset.UtcNow,
    runtimeSeconds,
    terminalStatus,
    gapEquationLambda8BackgroundChannelSteeringProbePassed,
    precursorsPassed,
    phase431PrecursorPassed,
    phase438PrecursorPassed,
    applicationSubjectKind = ApplicationSubjectKind,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
    workbenchConventions = new
    {
        lattices,
        spinorDimension = Ns,
        colorDimension = Nc,
        diracDiscretization = "naive-central-difference-hermitian",
        massInsertion = "Gamma = sz (x) I2 (x) C; anticommutes with kinetic AND background gamma_mu; diagonal C commutes with U8 -> H^2 = (kinetic+background)^2 + Sigma^2 C^2",
        background = "omega_bg = t8*(lambda_8/2) minimally coupled on BOTH directions; per-momentum color-c block gamma_mu (x) (s_mu + t8 u_c)",
        colorShifts = "u_c: colors 1,2 = 1/(2 sqrt3); color 3 = -1/sqrt3 (eigenvalues of lambda_8/2)",
        channels = "singlet C=I3, hypercharge C=lambda_8/2, general diagonal C, and FREE-DIAGONAL per-color gaps (Sigma_1,Sigma_2,Sigma_3)",
        contents = "fundamental (1 copy) and derived-4x (4 copies)",
        infraredConvention = "exclude momenta with s1=s2=0 (t8=0 zero-dispersion); SAME excluded set at every t8 so the t8=0 limit is exactly Phase438",
        t8Grid,
        couplingGrid = "20 log-spaced g2 in [0.1, 10]",
        competitionCouplings = competitionG2,
        latticeCutoffLambda = lambdaCutoff,
        workbenchConventionsAreSourceDefined,
    },
    excludedZeroDispersionModeCounts = lattices.ToDictionary(L => $"L{L}", ExcludedCount),
    batteries = new
    {
        gammaAnticommutationResidual,
        gammaAnticommutationExact,
        hyperColorChargesConsistent,
        sumU8Squared,
        denseClosedFormRows = denseBatteryRows.Select(r => new { channel = r.Channel, t8 = r.T8, residual = r.Residual }).ToArray(),
        closedFormMaxResidual,
        closedFormSpectrumMatchesDense,
        phase438SingletResidual,
        phase438HyperResidual,
        phase438ConsistencyAtZeroBackground,
        maxSigma1Sigma2Difference = maxS1S2Diff,
        freeDiagonalAlignmentExact,
        allBatteriesPassed,
    },
    steeringRows = steeringRows.Select(r => new
    {
        content = r.Content,
        copies = r.Copies,
        lattice = r.L,
        g2 = r.G2,
        t8 = r.T8,
        sigma1 = r.Sigma1,
        sigma2 = r.Sigma2,
        sigma3 = r.Sigma3,
        sigmaSingletConstrained = r.SigmaConstrained,
        freeDiagonalFreeEnergy = r.WFree,
        singletConstrainedFreeEnergy = r.WConstrained,
        steeringMargin = r.Margin,
        colorSplittingRatio = r.Ratio,
        condensateNonzero = r.Nonzero,
        steers = r.Steers,
    }).ToArray(),
    minSteeringMarginAtNonzeroT8 = minSteeringMarginAtNonzeroT8 == double.MaxValue ? 0.0 : minSteeringMarginAtNonzeroT8,
    maxMarginAtZeroBackground,
    maxSplittingRatioAtNonzeroT8,
    backgroundInducesChannelSteering,
    dynamicalMassPatternAlignsWithSu2U1,
    colorGapSplittingMonotoneInBackground,
    criticalCouplingRows = critRows.Select(r => new
    {
        content = r.Content,
        copies = r.Copies,
        lattice = r.L,
        t8 = r.T8,
        g2CritColor1 = r.G2Crit1,
        g2CritColor3 = r.G2Crit3,
    }).ToArray(),
    backgroundLowersColorCriticalCouplingAtModerateT8,
    scaleLawRows = scaleLawRows.Select(r => new
    {
        content = r.Content,
        copies = r.Copies,
        lattice = r.L,
        t8 = r.T8,
        color = r.Color,
        lnSigmaVsInvG2Slope = Finite(r.Slope),
        lnSigmaVsInvG2Intercept = Finite(r.Intercept),
        lnSigmaVsInvG2R2 = Finite(r.R2),
        fitPointCount = r.FitPointCount,
    }).ToArray(),
    allScaleLawNegativeSlope,
    fundamentalScaleLawLinear,
    scaleLawR2Min,
    scaleLawR2Max,
    transmutationSignatureSurvivesBackground,
    backgroundSteersCondensateChannelOnWorkbench,
    gapEquationIsMeanFieldApproximation,
    fourFermionCouplingNormalizationIsRecordedConvention,
    backgroundParameterT8IsRecordedCandidateOnly,
    backgroundParameterT8DynamicallyDerived,
    scaleAndPatternAreCandidateOnly,
    noGevPromotion,
    honestBoundaries = new
    {
        gapEquationIsMeanFieldApproximation,
        fourFermionCouplingNormalizationIsRecordedConvention,
        backgroundParameterT8IsRecordedCandidateOnly,
        backgroundParameterT8DynamicallyDerived,
        scaleAndPatternAreCandidateOnly,
        noGevPromotion,
    },
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
        phase431SummaryPath = Phase431SummaryPath,
        phase438SummaryPath = Phase438SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "t8 is a RECORDED CANDIDATE background parameter swept over a grid, not a dynamically derived scale; deriving it is the Phase430 chain's job.",
        "The gap equation is a mean-field (Hartree) approximation; fluctuations beyond the fermion loop are not included.",
        "The four-fermion coupling g2 is a scanned parameter; its physical normalization is fixed only up to the workbench kappa_B and is a recorded convention, NOT source-derived.",
        "The free-diagonal color gaps Sigma_c and the su(2)xu(1) splitting pattern are candidate-only workbench numbers; the lattice cutoff Lambda is a recorded convention (max dispersion sqrt(2)).",
        "The steering margin W_freeDiag < W_singlet is <= 0 by construction (singlet is feasible for the free-diagonal); the phase reports the STRICT positive margin at t8>0 as the steering verdict, not a completed vacuum.",
        "This phase decides only that a candidate lambda_8 background CAN steer the condensate channel and align its pattern on the workbench; it closes no GeV scale gap and promotes nothing.",
        "No Phase201 or Phase256 field is filled.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "gap_equation_lambda8_background_channel_steering_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "gap_equation_lambda8_background_channel_steering_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"gapEquationLambda8BackgroundChannelSteeringProbePassed={gapEquationLambda8BackgroundChannelSteeringProbePassed}");
Console.WriteLine($"precursorsPassed={precursorsPassed} (431={phase431PrecursorPassed} 438={phase438PrecursorPassed})");
Console.WriteLine($"batteries: gammaAnticomm={gammaAnticommutationExact} denseCF={closedFormSpectrumMatchesDense} ({closedFormMaxResidual:E1}) phase438Consistency={phase438ConsistencyAtZeroBackground} (S {phase438SingletResidual:E1}, H {phase438HyperResidual:E1}) alignment={freeDiagonalAlignmentExact} (|S1-S2|<={maxS1S2Diff:E1})");
Console.WriteLine($"backgroundInducesChannelSteering={backgroundInducesChannelSteering} (minMargin@t8>0={minSteeringMarginAtNonzeroT8:E3}, maxMargin@t8=0={maxMarginAtZeroBackground:E3})");
Console.WriteLine($"dynamicalMassPatternAlignsWithSu2U1={dynamicalMassPatternAlignsWithSu2U1} (maxSplitRatio@t8>0={maxSplittingRatioAtNonzeroT8:F5})");
Console.WriteLine($"colorGapSplittingMonotoneInBackground={colorGapSplittingMonotoneInBackground}");
Console.WriteLine($"transmutationSignatureSurvivesBackground={transmutationSignatureSurvivesBackground} (fundamental R2 in [{scaleLawR2Min:F3},{scaleLawR2Max:F3}])");
Console.WriteLine("free-diagonal gaps (fundamental L=8 g2=5):");
foreach (var r in steeringRows.Where(r => r.Content == "fundamental" && r.L == 8 && r.G2 == 5.0))
    Console.WriteLine($"  t8={r.T8}: S1={r.Sigma1:F6} S3={r.Sigma3:F6} ratio={r.Ratio:F6} margin={r.Margin:E3}");
Console.WriteLine("g2_crit per color (fundamental L=8):");
foreach (var r in critRows.Where(r => r.Content == "fundamental" && r.L == 8))
    Console.WriteLine($"  t8={r.T8}: gc1={r.G2Crit1:F5} gc3={r.G2Crit3:F5}");
Console.WriteLine($"backgroundLowersColorCriticalCouplingAtModerateT8={backgroundLowersColorCriticalCouplingAtModerateT8}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract} canFillPhase201HiggsContract={canFillPhase201HiggsContract}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F2}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

static Complex[,] Kron2(Complex[,] a)
{
    var r = new Complex[4, 4];
    for (int i = 0; i < 2; i++)
        for (int j = 0; j < 2; j++)
            for (int k = 0; k < 2; k++)
                r[i * 2 + k, j * 2 + k] = a[i, j];
    return r;
}
static Complex[,] MatMul(Complex[,] a, Complex[,] b)
{
    int n = a.GetLength(0);
    var r = new Complex[n, n];
    for (int i = 0; i < n; i++)
        for (int j = 0; j < n; j++)
        {
            Complex s = Complex.Zero;
            for (int k = 0; k < n; k++) s += a[i, k] * b[k, j];
            r[i, j] = s;
        }
    return r;
}
static Complex[,] AntiComm(Complex[,] a, Complex[,] b) => Add(MatMul(a, b), MatMul(b, a));
static Complex[,] Add(Complex[,] a, Complex[,] b)
{
    int n = a.GetLength(0); var r = new Complex[n, n];
    for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) r[i, j] = a[i, j] + b[i, j];
    return r;
}
static Complex[,] Sub(Complex[,] a, Complex[,] b)
{
    int n = a.GetLength(0); var r = new Complex[n, n];
    for (int i = 0; i < n; i++) for (int j = 0; j < n; j++) r[i, j] = a[i, j] - b[i, j];
    return r;
}
static Complex[,] Identity(int n)
{
    var r = new Complex[n, n];
    for (int i = 0; i < n; i++) r[i, i] = 1;
    return r;
}
static double MaxAbs(Complex[,] a)
{
    double m = 0.0;
    foreach (var v in a) m = Math.Max(m, v.Magnitude);
    return m;
}
static double[] LogSpace(double lo, double hi, int n)
{
    double a = Math.Log10(lo), b = Math.Log10(hi);
    return Enumerable.Range(0, n).Select(i => Math.Pow(10.0, a + (b - a) * i / (n - 1))).ToArray();
}
static (double Slope, double Intercept, double R2) LinFit(double[] x, double[] y)
{
    int n = x.Length;
    if (n < 2) return (double.NaN, double.NaN, double.NaN);
    double sx = x.Sum(), sy = y.Sum(), sxx = x.Sum(v => v * v), sxy = 0.0;
    for (int i = 0; i < n; i++) sxy += x[i] * y[i];
    double denom = n * sxx - sx * sx;
    double slope = (n * sxy - sx * sy) / denom;
    double intercept = (sy - slope * sx) / n;
    double mean = sy / n, ssTot = 0.0, ssRes = 0.0;
    for (int i = 0; i < n; i++)
    {
        double pred = slope * x[i] + intercept;
        ssRes += (y[i] - pred) * (y[i] - pred);
        ssTot += (y[i] - mean) * (y[i] - mean);
    }
    double r2 = ssTot > 0 ? 1.0 - ssRes / ssTot : 1.0;
    return (slope, intercept, r2);
}

static double[,] Realify(Complex[,] m)
{
    int n = m.GetLength(0);
    var result = new double[2 * n, 2 * n];
    for (int r = 0; r < n; r++)
        for (int c = 0; c < n; c++)
        {
            result[r, c] = m[r, c].Real;
            result[r, c + n] = -m[r, c].Imaginary;
            result[r + n, c] = m[r, c].Imaginary;
            result[r + n, c + n] = m[r, c].Real;
        }
    return result;
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

static double? Finite(double value) => double.IsFinite(value) ? value : null;

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

public sealed record SteeringRow(
    string Content, int Copies, int L, double G2, double T8,
    double Sigma1, double Sigma2, double Sigma3, double SigmaConstrained,
    double WFree, double WConstrained, double Margin, double Ratio, bool Nonzero, bool Steers);

public sealed record CritRow(string Content, int Copies, int L, double T8, double G2Crit1, double G2Crit3);

public sealed record ScaleLawRow(
    string Content, int Copies, int L, double T8, int Color,
    double Slope, double Intercept, double R2, int FitPointCount);
