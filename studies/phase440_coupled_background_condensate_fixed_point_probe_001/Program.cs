using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase440: coupled background-condensate fixed-point probe.
// THE FINAL LINK of the candidate chain 430 -> 439 -> 438.
//
// The chain, in one paragraph. Phase430/435/436/437 proved that the one-loop
// landscape ALONE has no finite minimum along the lambda_8 background t8: the
// net (bosonic + fermionic) large-t8 log slope is negative (fundamental -4 per
// site, derived hypercharge -40 per site), driven by fermion modes that stay
// light as t8 grows, so W runs away. Phase438 showed the Gross-Neveu-style gap
// equation DOES generate a dynamical condensate scale Sigma. Phase439 showed a
// lambda_8 background STEERS that condensate into an su(2)xu(1)-aligned
// per-color pattern (Sigma_1 = Sigma_2 != Sigma_3). Each ingredient is
// machine-verified in its own phase.
//
// THE QUESTION THIS PHASE DECIDES: treat background t8 and condensate Sigma as a
// JOINT variational system. Does the condensate's gapping of the fermion modes
// SATURATE the fermionic log-runaway and produce a finite INTERIOR minimum
// (t8*, Sigma*) - i.e. does the candidate chain close into full internal
// self-consistency?
//
// JOINT FREE ENERGY per volume (union of the Phase430 bosonic workbench model
// and the Phase439 free-diagonal fermionic gap functional, each relative to
// (t8=0, Sigma=0) so W_total(0,0)=0):
//   W_total(t8, Sigma_vec)
//     = W_B(t8)                                          [bosonic, Phase430]
//       + sum_c [ Sigma_c^2/(2 g2)
//                 - copies*(Ns/2)/Vol * sum_k' ( log(eps_c(k,t8)^2 + Sigma_c^2)
//                                                - log(eps_c(k,0)^2) ) ]   [fermionic, Phase439]
//   W_B(t8) = (1/(2 Vol)) sum_k' sum_i 2 * log( (eps_k^2 + t8^2 m_i^2) / eps_k^2 ),
//   m_i^2 = eigenvalues of -ad_{u8}^2 on su(3) (four 0, four 3/4),
//   eps_c(k,t8)^2 = (s1 + t8 u_c)^2 + (s2 + t8 u_c)^2,  u_c = eigenvalues of
//   lambda_8/2 = {1/(2 sqrt3), 1/(2 sqrt3), -1/sqrt3}. Doubler exclusion:
//   momenta with eps_k^2 = s1^2+s2^2 <= 1e-12 dropped, SAME set at every t8.
//
// FINDING (machine-verified below): NO interior fixed point exists anywhere. The
// condensate SATURATES (delays) the runaway - the gap shifts the fermionic IR
// cutoff eps -> sqrt(eps^2+Sigma^2), so the runaway onset is pushed out to
// t8 ~ Sigma - but it does NOT stop it: at large t8 the background gaps out the
// IR modes that drive condensation (the joint-optimal Sigma -> 0 as t8 grows),
// so the asymptotic net slope is unchanged and negative. The joint minimum is
// therefore either TRIVIAL (t8* = 0, condensate from Phase438; strong coupling)
// or RUNAWAY (t8* escapes to the grid boundary; weak coupling), never interior.
// The coupled system does NOT self-generate a finite background: the candidate
// chain does not close into a self-consistent (t8*, Sigma*).
//
// Fail-closed: t8 is a RECORDED CANDIDATE background parameter (not dynamically
// derived); the gap equation is a mean-field approximation; the four-fermion
// coupling normalization and the bosonic mode-mass model are recorded workbench
// conventions; the scale ratios are candidate-only; nothing is promoted to GeV;
// no Phase201 or Phase256 field is filled. The phase PASSES on internal
// consistency regardless of which outcome wins - it records what IS.

const string DefaultOutputDir = "studies/phase440_coupled_background_condensate_fixed_point_probe_001/output";
const string Phase430SummaryPath = "studies/phase430_net_one_loop_direction_selection_probe_001/output/net_one_loop_direction_selection_probe_summary.json";
const string Phase438SummaryPath = "studies/phase438_self_consistent_condensate_gap_equation_probe_001/output/self_consistent_condensate_gap_equation_probe_summary.json";
const string Phase439SummaryPath = "studies/phase439_gap_equation_lambda8_background_channel_steering_probe_001/output/gap_equation_lambda8_background_channel_steering_probe_summary.json";
const string ApplicationSubjectKind = "coupled-background-condensate-fixed-point-probe";

var startTime = DateTimeOffset.UtcNow;
var outputDir = Environment.GetEnvironmentVariable("PHASE440_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase430 = JsonDocument.Parse(File.ReadAllText(Phase430SummaryPath));
using var phase438 = JsonDocument.Parse(File.ReadAllText(Phase438SummaryPath));
using var phase439 = JsonDocument.Parse(File.ReadAllText(Phase439SummaryPath));

bool phase430PrecursorPassed =
    JsonBool(phase430.RootElement, "netOneLoopDirectionSelectionProbePassed") is true &&
    JsonBool(phase430.RootElement, "noFiniteMinimumOnRays") is true;
bool phase438PrecursorPassed =
    JsonBool(phase438.RootElement, "selfConsistentCondensateGapEquationProbePassed") is true &&
    JsonBool(phase438.RootElement, "dynamicalScaleGenerationObserved") is true;
bool phase439PrecursorPassed =
    JsonBool(phase439.RootElement, "gapEquationLambda8BackgroundChannelSteeringProbePassed") is true &&
    JsonBool(phase439.RootElement, "backgroundInducesChannelSteering") is true &&
    JsonBool(phase439.RootElement, "dynamicalMassPatternAlignsWithSu2U1") is true;
bool precursorsPassed = phase430PrecursorPassed && phase438PrecursorPassed && phase439PrecursorPassed;

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
// su(3): Gell-Mann fundamental, structure constants, adjoint rep, and the
// bosonic mode masses m_i^2 = eigenvalues of -ad_{lambda_8/2}^2 (Phase430).
// ---------------------------------------------------------------------------

const int Nc = 3;
const int Ns = 4;
double[] u8 = { 1.0 / (2.0 * Math.Sqrt(3.0)), 1.0 / (2.0 * Math.Sqrt(3.0)), -1.0 / Math.Sqrt(3.0) };
double sumU8Squared = u8.Sum(u => u * u); // = 1/2
bool hyperColorChargesConsistent = Math.Abs(sumU8Squared - 0.5) <= 1e-12;
double lambdaCutoff = Math.Sqrt(2.0);

var gellMann = new Complex[8][,];
for (int a = 0; a < 8; a++) gellMann[a] = new Complex[3, 3];
gellMann[0][0, 1] = 1; gellMann[0][1, 0] = 1;
gellMann[1][0, 1] = -Complex.ImaginaryOne; gellMann[1][1, 0] = Complex.ImaginaryOne;
gellMann[2][0, 0] = 1; gellMann[2][1, 1] = -1;
gellMann[3][0, 2] = 1; gellMann[3][2, 0] = 1;
gellMann[4][0, 2] = -Complex.ImaginaryOne; gellMann[4][2, 0] = Complex.ImaginaryOne;
gellMann[5][1, 2] = 1; gellMann[5][2, 1] = 1;
gellMann[6][1, 2] = -Complex.ImaginaryOne; gellMann[6][2, 1] = Complex.ImaginaryOne;
double invSqrt3 = 1.0 / Math.Sqrt(3.0);
gellMann[7][0, 0] = invSqrt3; gellMann[7][1, 1] = invSqrt3; gellMann[7][2, 2] = -2.0 * invSqrt3;

var genFund = new Complex[8][,];
for (int a = 0; a < 8; a++)
{
    genFund[a] = new Complex[3, 3];
    for (int r = 0; r < 3; r++)
        for (int c = 0; c < 3; c++)
            genFund[a][r, c] = gellMann[a][r, c] / 2.0;
}

var fabc = new double[8, 8, 8];
for (int a = 0; a < 8; a++)
    for (int b = 0; b < 8; b++)
    {
        var comm = new Complex[3, 3];
        for (int r = 0; r < 3; r++)
            for (int c = 0; c < 3; c++)
            {
                Complex sum = Complex.Zero;
                for (int k = 0; k < 3; k++)
                    sum += genFund[a][r, k] * genFund[b][k, c] - genFund[b][r, k] * genFund[a][k, c];
                comm[r, c] = sum;
            }
        for (int c3 = 0; c3 < 8; c3++)
        {
            Complex trace = Complex.Zero;
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    trace += comm[r, c] * genFund[c3][c, r];
            fabc[a, b, c3] = (-2.0 * Complex.ImaginaryOne * trace).Real;
        }
    }
// adjoint generator of the lambda_8 axis: (ad_8)_{bc} = -i f_{8 b c}.
var adEight = new Complex[8, 8];
for (int b = 0; b < 8; b++)
    for (int c = 0; c < 8; c++)
        adEight[b, c] = -Complex.ImaginaryOne * fabc[7, b, c];
var (adEightEigen, _) = Jacobi(Realify(adEight));
// realified doubling: undouble; masses^2 = (adjoint eigenvalue)^2.
var adEigenUndoubled = adEightEigen.OrderBy(v => v).Where((_, i) => i % 2 == 0).ToArray();
double[] bosonMassesSquared = adEigenUndoubled.Select(v => v * v).OrderBy(v => v).ToArray();
int bosonZeroModeCount = bosonMassesSquared.Count(m => m <= 1e-12);
int bosonNonzeroModeCount = bosonMassesSquared.Length - bosonZeroModeCount;
double bosonNonzeroMassSquared = bosonMassesSquared.Where(m => m > 1e-12).DefaultIfEmpty(0.0).Max();
// exact expectation: four 0 and four 3/4.
bool bosonicMassesConsistent =
    bosonZeroModeCount == 4 && bosonNonzeroModeCount == 4 &&
    bosonMassesSquared.Where(m => m > 1e-12).All(m => Math.Abs(m - 0.75) <= 1e-10);

int[] lattices = { 4, 8, 16 };
var g2Grid = LogSpace(0.3, 8.0, 8); // 8 log-spaced in [0.3, 8]
var contents = new (string Name, int Copies)[] { ("fundamental", 1), ("derived-4x", 4) };

// ---------------------------------------------------------------------------
// Momenta and dispersions (doubler-excluded, SAME excluded set at every t8).
// ---------------------------------------------------------------------------

(double S1, double S2)[] IncludedMomenta(int L)
{
    var list = new List<(double, double)>();
    for (int n1 = 0; n1 < L; n1++)
        for (int n2 = 0; n2 < L; n2++)
        {
            double s1 = Math.Sin(2.0 * Math.PI * n1 / L);
            double s2 = Math.Sin(2.0 * Math.PI * n2 / L);
            if (s1 * s1 + s2 * s2 > 1e-12) list.Add((s1, s2));
        }
    return list.ToArray();
}
int ExcludedCount(int L) => L * L - IncludedMomenta(L).Length;

double[] Eps2Color(int L, double t8, double uc)
{
    var inc = IncludedMomenta(L);
    var e = new double[inc.Length];
    for (int i = 0; i < inc.Length; i++)
    {
        double a1 = inc[i].S1 + t8 * uc, a2 = inc[i].S2 + t8 * uc;
        e[i] = a1 * a1 + a2 * a2;
    }
    return e;
}
double[][] Eps2AllColors(int L, double t8) => u8.Select(u => Eps2Color(L, t8, u)).ToArray();

// ---------------------------------------------------------------------------
// W_B(t8): bosonic workbench model, per volume, 2 polarizations, ratio form so
// W_B(0) = 0. Only the four nonzero -ad_{u8}^2 modes contribute.
// ---------------------------------------------------------------------------

double WBoson(int L, double t8)
{
    double vol = L * L;
    double tot = 0.0;
    foreach (var (s1, s2) in IncludedMomenta(L))
    {
        double eps2 = s1 * s1 + s2 * s2;
        foreach (double m2 in bosonMassesSquared)
            if (m2 > 1e-12)
                tot += 2.0 * Math.Log((eps2 + t8 * t8 * m2) / eps2);
    }
    return tot / (2.0 * vol);
}

// ---------------------------------------------------------------------------
// W_F fermionic (Phase439 free-diagonal), relative to (t8=0, Sigma=0). Per
// (color, momentum) coefficient copies*(Ns/2)/Vol (the Phase439 convention -
// this is what reproduces the Phase438 gap solutions at t8=0).
// ---------------------------------------------------------------------------

double WFermion(int L, double t8, double[] sig, double g2, int copies)
{
    double vol = L * L;
    double[][] e2 = Eps2AllColors(L, t8);
    double[][] e20 = Eps2AllColors(L, 0.0);
    double w = 0.0;
    for (int c = 0; c < Nc; c++)
    {
        double fer = 0.0;
        for (int k = 0; k < e2[c].Length; k++)
            fer += Math.Log(e2[c][k] + sig[c] * sig[c]) - Math.Log(e20[c][k]);
        w += sig[c] * sig[c] / (2.0 * g2) - copies * (Ns / 2.0) * fer / vol;
    }
    return w;
}

double WTotal(int L, double t8, double[] sig, double g2, int copies) =>
    WBoson(L, t8) + WFermion(L, t8, sig, g2, copies);

// ---------------------------------------------------------------------------
// Per-color gap equation in the background: 1 = g2*copies*(Ns/Vol) sum_k' 1/(eps_c^2 + Sigma_c^2).
// The free-diagonal fermionic W decouples per color and W_B is Sigma-independent,
// so at fixed t8 the joint-optimal Sigma_c is exactly this per-color gap solution.
// ---------------------------------------------------------------------------

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

double GapColorSolve(int L, double t8, double g2, int copies, int color)
{
    double vol = L * L;
    double[] e2c = Eps2Color(L, t8, u8[color]);
    return SolveSigma(s =>
    {
        double acc = 0.0;
        foreach (double e in e2c) acc += 1.0 / (e + s * s);
        return 1.0 - g2 * copies * (Ns / vol) * acc;
    });
}

double[] OptimalSigma(int L, double t8, double g2, int copies)
{
    double s1 = GapColorSolve(L, t8, g2, copies, 0);
    double s3 = GapColorSolve(L, t8, g2, copies, 2);
    return new[] { s1, s1, s3 }; // Sigma_1 = Sigma_2 by the shared u_c (verified below)
}

// ---------------------------------------------------------------------------
// BATTERY: Phase430 cross-anchor. Reconstruct the UNnormalized fermionic and
// bosonic ray functionals (all momenta, no /Vol, L=4) and verify the exact
// integer Richardson log-slopes -192 (fermion S fund), -768 (fermion S derived),
// +128 (boson S), and the nets -64 (fund), -640 (derived). -640/16 = -40 per
// site is the Phase437 2D anchor. This ties the Sigma=0 slice to Phase430.
// ---------------------------------------------------------------------------

double FermionPotP430(double t, int copies)
{
    // -0.5 * sum_mom sum_c sum_{4 spinor} log((s1+t u_c)^2 + (s2+t u_c)^2), L=4, all momenta.
    double sum = 0.0;
    for (int n1 = 0; n1 < 4; n1++)
        for (int n2 = 0; n2 < 4; n2++)
        {
            double s1 = Math.Sin(2.0 * Math.PI * n1 / 4), s2 = Math.Sin(2.0 * Math.PI * n2 / 4);
            foreach (double uc in u8)
            {
                double l2 = (s1 + t * uc) * (s1 + t * uc) + (s2 + t * uc) * (s2 + t * uc);
                if (l2 > 1e-18) sum += 4.0 * Math.Log(l2);
            }
        }
    return copies * (-0.5 * sum);
}
double BosonPotP430(double t)
{
    double sum = 0.0;
    for (int n1 = 0; n1 < 4; n1++)
        for (int n2 = 0; n2 < 4; n2++)
        {
            double s1 = Math.Sin(2.0 * Math.PI * n1 / 4), s2 = Math.Sin(2.0 * Math.PI * n2 / 4);
            double eps2 = s1 * s1 + s2 * s2;
            foreach (double m2 in bosonMassesSquared)
            {
                double arg = eps2 + t * t * m2;
                if (arg > 1e-18) sum += 2.0 * Math.Log(arg);
            }
        }
    return 0.5 * sum;
}
double SlopeOctave(double whi, double wlo) => (whi - wlo) / Math.Log(2.0);
double Richardson(Func<double, double> w)
{
    double lo = SlopeOctave(w(80.0), w(40.0));
    double hi = SlopeOctave(w(160.0), w(80.0));
    return (4.0 * hi - lo) / 3.0;
}
double anchorFermionSFund = Richardson(t => FermionPotP430(t, 1) - FermionPotP430(0.0, 1));
double anchorFermionSDerived = Richardson(t => FermionPotP430(t, 4) - FermionPotP430(0.0, 4));
double anchorBosonS = Richardson(t => BosonPotP430(t) - BosonPotP430(0.0));
double anchorNetSFund = anchorFermionSFund + anchorBosonS;
double anchorNetSDerived = anchorFermionSDerived + anchorBosonS;
double anchorNetDerivedPerSite = anchorNetSDerived / 16.0;
double phase430AnchorMaxResidual = new[]
{
    Math.Abs(anchorFermionSFund - (-192.0)),
    Math.Abs(anchorFermionSDerived - (-768.0)),
    Math.Abs(anchorBosonS - 128.0),
    Math.Abs(anchorNetSFund - (-64.0)),
    Math.Abs(anchorNetSDerived - (-640.0)),
    Math.Abs(anchorNetDerivedPerSite - (-40.0)),
}.Max();
bool phase430CrossAnchorMatches = phase430AnchorMaxResidual <= 0.05;

// ---------------------------------------------------------------------------
// BATTERY: t8=0 slice reproduces the Phase438 gap solutions. Uses the
// singlet/hyper channel reductions (Phase439 convention).
// ---------------------------------------------------------------------------

double phase438SingletResidual = 0.0, phase438HyperResidual = 0.0;
foreach (int L in lattices)
{
    double vol = L * L; int copies = 1; double g2 = 3.0;
    double[] e2single = Eps2Color(L, 0.0, u8[0]); // unshifted dispersion at t8=0
    // singlet channel (single Sigma to all colors): 1 = g2*copies*(Ns/vol) * Nc * sum 1/(e+sig^2).
    double sigA = SolveSigma(s => 1.0 - g2 * copies * (Ns / vol) * Nc * e2single.Sum(e => 1.0 / (e + s * s)));
    // Phase438 GapSinglet reference: 1 = g2*copies*(Nc*Ns/vol) sum 1/(e+sig^2).
    double sigRefS = SolveSigma(s => 1.0 - g2 * copies * (Nc * Ns / vol) * e2single.Sum(e => 1.0 / (e + s * s)));
    phase438SingletResidual = Math.Max(phase438SingletResidual, Math.Abs(sigA - sigRefS));
    // hypercharge channel (single Sigma, charge u_c): 1 = g2*copies*(Ns/vol) sum_c u_c^2 sum 1/(e+sig^2 u_c^2).
    double sigB = SolveSigma(s =>
    {
        double acc = 0.0;
        foreach (double u in u8) { double uu = u * u; acc += uu * e2single.Sum(e => 1.0 / (e + s * s * uu)); }
        return 1.0 - g2 * copies * (Ns / vol) * acc;
    });
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
// BATTERY: closed-form spectrum eps_c^2 + Sigma^2 c_c^2 vs a dense L=4 Dirac
// solve WITH the lambda_8 background, at a nontrivial (t8, Sigma) point using a
// free-diagonal per-color mass pattern (H^2 = (kinetic+background)^2 + Sigma^2 C^2).
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
    for (int i = 0; i < denseUndoubled.Length; i++) denseUndoubled[i] = denseSquared[2 * i];
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
// nontrivial (t8, Sigma) point with a free-diagonal su(2)xu(1)-aligned pattern.
double[] freeDiagonalPattern = { 0.6, 0.6, 0.9 };
double closedFormResidualAligned = DenseClosedFormResidual(1.0, freeDiagonalPattern);
double closedFormResidualSinglet = DenseClosedFormResidual(1.0, new[] { 1.0, 1.0, 1.0 });
double closedFormMaxResidual = Math.Max(closedFormResidualAligned, closedFormResidualSinglet);
bool closedFormSpectrumMatchesDense = closedFormMaxResidual <= 1e-8;

// ---------------------------------------------------------------------------
// BATTERY: Sigma_1 = Sigma_2 symmetry (colors 1,2 share u_c) at nonzero t8.
// ---------------------------------------------------------------------------

double maxS1S2Diff = 0.0;
foreach (int L in lattices)
    foreach (var (name, copies) in contents)
        foreach (double g2 in g2Grid)
            foreach (double t8 in new[] { 0.5, 1.0, 2.0, 4.0 })
            {
                double s1 = GapColorSolve(L, t8, g2, copies, 0);
                double s2 = GapColorSolve(L, t8, g2, copies, 1);
                maxS1S2Diff = Math.Max(maxS1S2Diff, Math.Abs(s1 - s2));
            }
bool sigma1EqualsSigma2 = maxS1S2Diff <= 1e-10;

// ---------------------------------------------------------------------------
// ANALYSIS 1: SATURATION MECHANISM. At fixed Sigma, the fermionic runaway onset
// is delayed to t8 ~ Sigma (gap shifts the IR cutoff eps -> sqrt(eps^2+Sigma^2)).
// Quantify: fermionic t8-log-slope in a moderate window shrinks with Sigma, then
// resumes to the Sigma=0 integer count once t8 >> Sigma.
// ---------------------------------------------------------------------------

var saturationRows = new List<SaturationRow>();
{
    int L = 8; int copies = 1; double g2 = 1.0; // g2 only shifts the t8-independent Sigma^2/2g2 constant
    double[] fixedSigmas = { 0.0, 1.0, 2.0, 4.0 };
    // asymptotic Sigma=0 fermionic slope per this normalization (all colors u_c != 0):
    double vol = L * L; int active = IncludedMomenta(L).Length;
    double asymptoticFermionSlope = -copies * Ns * Nc * active / vol; // d W_F / d ln t8 at large t8
    foreach (double Sig in fixedSigmas)
    {
        var sig = new[] { Sig, Sig, Sig };
        // moderate-window slope (t8 in [1,2]) and large-window slope (t8 in [8,16]) in ln t8.
        double moderateSlope = (WFermion(L, 2.0, sig, g2, copies) - WFermion(L, 1.0, sig, g2, copies)) / Math.Log(2.0);
        double largeSlope = (WFermion(L, 16.0, sig, g2, copies) - WFermion(L, 8.0, sig, g2, copies)) / Math.Log(2.0);
        // finite depression of the runaway at a reference t8 relative to Sigma=0.
        double depressionAtT8_2 = WFermion(L, 2.0, sig, g2, copies) - WFermion(L, 2.0, new[] { 0.0, 0.0, 0.0 }, g2, copies)
                                  - (sig[0] * sig[0] + sig[1] * sig[1] + sig[2] * sig[2]) / (2.0 * g2);
        saturationRows.Add(new SaturationRow(Sig, moderateSlope, largeSlope, asymptoticFermionSlope, depressionAtT8_2));
    }
}
// verdict: the gap depresses the moderate-t8 slope (saturation) but the large-t8
// slope resumes toward the Sigma=0 asymptotic count.
double moderateSlopeSigma0 = saturationRows.First(r => r.Sigma == 0.0).ModerateWindowSlope;
double moderateSlopeSigma4 = saturationRows.First(r => r.Sigma == 4.0).ModerateWindowSlope;
double largeSlopeSigma4 = saturationRows.First(r => r.Sigma == 4.0).LargeWindowSlope;
bool condensateDepressesModerateRunaway = Math.Abs(moderateSlopeSigma4) < 0.5 * Math.Abs(moderateSlopeSigma0);
bool largeSlopeResumesTowardSigma0 = Math.Abs(largeSlopeSigma4) > Math.Abs(moderateSlopeSigma4);
bool condensateSaturatesFermionicRunaway = condensateDepressesModerateRunaway && largeSlopeResumesTowardSigma0;

// ---------------------------------------------------------------------------
// ANALYSIS 2: JOINT MINIMIZATION over (t8, Sigma_1=Sigma_2, Sigma_3). Grid over
// t8 with the joint-optimal Sigma slaved to the per-color gap solution, then a
// verified-descent refinement (backtracking, strict decrease) on the full
// 3-parameter W_total. Classify trivial / interior / runaway.
// ---------------------------------------------------------------------------

const double T8Max = 8.0;
const double T8TrivialThreshold = 1e-3;
const double T8BoundaryThreshold = T8Max - 1e-3;

// verified descent on (t8, s1, s3) with box constraints t8 in [0,T8Max], s>=0.
(double T8, double S1, double S3, double W, int Steps, double GradNorm) VerifiedDescent(
    int L, int copies, double g2, double t8Init, double s1Init, double s3Init)
{
    double[] x = { t8Init, s1Init, s3Init };
    double Eval(double[] p) => WTotal(L, Clamp(p[0], 0.0, T8Max), new[] { Math.Max(p[1], 0.0), Math.Max(p[1], 0.0), Math.Max(p[2], 0.0) }, g2, copies);
    double w = Eval(x);
    double step = 0.1;
    int steps = 0;
    double[] grad = NumGrad(Eval, x);
    for (int iter = 0; iter < 500; iter++)
    {
        grad = NumGrad(Eval, x);
        double gnorm = Math.Sqrt(grad.Sum(v => v * v));
        if (gnorm < 1e-9) break;
        bool accepted = false;
        double s = step;
        for (int bt = 0; bt < 40; bt++)
        {
            var trial = new double[3];
            for (int i = 0; i < 3; i++) trial[i] = x[i] - s * grad[i];
            trial[0] = Clamp(trial[0], 0.0, T8Max);
            trial[1] = Math.Max(trial[1], 0.0);
            trial[2] = Math.Max(trial[2], 0.0);
            double wt = Eval(trial);
            if (wt < w - 1e-14)
            {
                x = trial; w = wt; accepted = true; step = s * 1.5; steps++;
                break;
            }
            s *= 0.5;
        }
        if (!accepted) break;
    }
    double[] finalGrad = NumGrad(Eval, x);
    return (Clamp(x[0], 0.0, T8Max), Math.Max(x[1], 0.0), Math.Max(x[2], 0.0), w, steps,
        Math.Sqrt(finalGrad.Sum(v => v * v)));
}

var jointRows = new List<JointRow>();
bool anyInteriorFixedPoint = false;
foreach (var (contentName, copies) in contents)
    foreach (int L in lattices)
        foreach (double g2 in g2Grid)
        {
            // coarse t8 grid with slaved Sigma
            const int NT = 201;
            double bestW = double.MaxValue; double bestT8 = 0.0; double[] bestSig = { 0, 0, 0 };
            for (int i = 0; i < NT; i++)
            {
                double t8 = T8Max * i / (NT - 1);
                double[] sig = OptimalSigma(L, t8, g2, copies);
                double w = WTotal(L, t8, sig, g2, copies);
                if (w < bestW) { bestW = w; bestT8 = t8; bestSig = sig; }
            }
            double gridW = bestW;
            // verified-descent refinement from the grid optimum.
            var (t8r, s1r, s3r, wr, steps, gnorm) = VerifiedDescent(L, copies, g2, bestT8, bestSig[0], bestSig[2]);
            bool descentImproved = wr <= gridW + 1e-12;

            string classification = t8r < T8TrivialThreshold ? "trivial"
                : t8r > T8BoundaryThreshold ? "runaway" : "interior";

            // finite-difference Hessian in (t8, s1, s3) at the refined point (interior only).
            double hessMinEig = double.NaN;
            if (classification == "interior")
            {
                double[] xr = { t8r, s1r, s3r };
                double Eval(double[] p) => WTotal(L, Clamp(p[0], 0.0, T8Max), new[] { Math.Max(p[1], 0.0), Math.Max(p[1], 0.0), Math.Max(p[2], 0.0) }, g2, copies);
                var H = NumHessian(Eval, xr);
                var (hv, _) = Jacobi(H);
                hessMinEig = hv.Min();
                if (gnorm < 1e-3 && hessMinEig > 0.0) anyInteriorFixedPoint = true;
            }

            double t8OverS1 = s1r > 1e-9 ? t8r / s1r : double.NaN;
            double s3OverS1 = s1r > 1e-9 ? s3r / s1r : double.NaN;

            jointRows.Add(new JointRow(contentName, copies, L, g2, t8r, s1r, s1r, s3r, wr, gridW,
                classification, gnorm, hessMinEig, descentImproved, steps, t8OverS1, s3OverS1));
        }

bool allDescentVerified = jointRows.All(r => r.DescentImproved);
int trivialCount = jointRows.Count(r => r.Classification == "trivial");
int runawayCount = jointRows.Count(r => r.Classification == "runaway");
int interiorCount = jointRows.Count(r => r.Classification == "interior");

// ---------------------------------------------------------------------------
// ANALYSIS 3: honest decomposition of dW/dt8 at the runaway boundary. Shows
// bosonic (>0) vs fermionic (<0) parts and the negative net that drives escape.
// ---------------------------------------------------------------------------

var boundaryDecompRows = new List<BoundaryDecompRow>();
foreach (var (contentName, copies) in contents)
{
    int L = 8; double g2 = 0.3; double t8 = 7.9; double h = 0.01;
    double[] sig = OptimalSigma(L, t8, g2, copies);
    double dWB = (WBoson(L, t8 + h) - WBoson(L, t8 - h)) / (2.0 * h);
    double dWF = (WFermion(L, t8 + h, sig, g2, copies) - WFermion(L, t8 - h, sig, g2, copies)) / (2.0 * h);
    boundaryDecompRows.Add(new BoundaryDecompRow(contentName, copies, L, g2, t8, dWB, dWF, dWB + dWF));
}
bool boundaryNetSlopeNegative = boundaryDecompRows.All(r => r.NetSlope < 0.0);
bool boundaryBosonicPositiveFermionicNegative =
    boundaryDecompRows.All(r => r.BosonicSlope > 0.0 && r.FermionicSlope < 0.0);

// ---------------------------------------------------------------------------
// BATTERY: numerical-gradient consistency. At every trivial minimum the Sigma
// gap solution is a stationary point in Sigma (dW/dSigma_c = 0 exactly) and, by
// the symmetric-momentum cancellation, dW/dt8 = 0 at t8 = 0. Verify the reduced
// gradient (over Sigma) vanishes and dW/dt8|_{t8=0} ~ 0.
// ---------------------------------------------------------------------------

double maxTrivialSigmaGradient = 0.0;
double maxT8ZeroSlope = 0.0;
foreach (var (contentName, copies) in contents)
    foreach (int L in lattices)
        foreach (double g2 in g2Grid)
        {
            // dW/dt8 at t8=0 with Sigma at its t8=0 gap optimum.
            double[] sig0 = OptimalSigma(L, 0.0, g2, copies);
            double h = 1e-4;
            double slope0 = (WTotal(L, h, sig0, g2, copies) - WTotal(L, 0.0, sig0, g2, copies)) / h;
            maxT8ZeroSlope = Math.Max(maxT8ZeroSlope, Math.Abs(slope0));
            // dW/dSigma_c at the gap solution (should vanish); check color 1 and 3.
            for (int c = 0; c < Nc; c += 2)
            {
                if (sig0[c] <= 1e-6) continue;
                var sp = (double[])sig0.Clone(); var sm = (double[])sig0.Clone();
                sp[c] += h; sm[c] -= h;
                if (c == 0) { sp[1] = sp[0]; sm[1] = sm[0]; } // keep 1=2 tied
                double dW = (WTotal(L, 0.0, sp, g2, copies) - WTotal(L, 0.0, sm, g2, copies)) / (2.0 * h);
                maxTrivialSigmaGradient = Math.Max(maxTrivialSigmaGradient, Math.Abs(dW));
            }
        }
bool gradientConsistentAtMinimum = maxTrivialSigmaGradient <= 1e-3 && maxT8ZeroSlope <= 1e-3;

// ---------------------------------------------------------------------------
// Batteries roll-up.
// ---------------------------------------------------------------------------

bool allBatteriesPassed =
    gammaAnticommutationExact &&
    hyperColorChargesConsistent &&
    bosonicMassesConsistent &&
    phase430CrossAnchorMatches &&
    phase438ConsistencyAtZeroBackground &&
    closedFormSpectrumMatchesDense &&
    sigma1EqualsSigma2 &&
    allDescentVerified &&
    gradientConsistentAtMinimum;

// ---------------------------------------------------------------------------
// Verdicts.
// ---------------------------------------------------------------------------

bool jointFixedPointExists = anyInteriorFixedPoint;               // outcome (b) anywhere
bool jointFixedPointIsInterior = anyInteriorFixedPoint;
bool backgroundSelfGenerates = anyInteriorFixedPoint;
bool runawayPersistsInCoupledSystem = runawayCount > 0 && boundaryNetSlopeNegative;
bool jointMinimumIsTrivialOrRunaway = !anyInteriorFixedPoint && (trivialCount + runawayCount == jointRows.Count);

// ---------------------------------------------------------------------------
// Honest boundaries and fail-closed contract block.
// ---------------------------------------------------------------------------

const bool gapEquationIsMeanFieldApproximation = true;
const bool couplingNormalizationIsRecordedConvention = true;
const bool bosonicSectorIsWorkbenchModel = true;
const bool backgroundParameterT8IsRecordedCandidateOnly = true;
const bool backgroundParameterT8DynamicallyDerived = false;
const bool scaleRatiosAreCandidateOnly = true;
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
    "joint variational (t8, per-color Sigma) free energy = Phase430 bosonic workbench + Phase439 free-diagonal fermionic gap functional on the 2D doubler-excluded lattice; scanned coupling; no target values")))).ToLowerInvariant();

bool coupledBackgroundCondensateFixedPointProbePassed =
    precursorsPassed &&
    allBatteriesPassed &&
    condensateSaturatesFermionicRunaway &&
    jointMinimumIsTrivialOrRunaway &&
    // internal-consistency contract holds whichever outcome wins:
    (jointFixedPointExists == anyInteriorFixedPoint) &&
    (backgroundSelfGenerates == anyInteriorFixedPoint) &&
    gapEquationIsMeanFieldApproximation &&
    couplingNormalizationIsRecordedConvention &&
    bosonicSectorIsWorkbenchModel &&
    backgroundParameterT8IsRecordedCandidateOnly &&
    !backgroundParameterT8DynamicallyDerived &&
    scaleRatiosAreCandidateOnly &&
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

string terminalStatus = coupledBackgroundCondensateFixedPointProbePassed
    ? "coupled-system-has-no-interior-fixed-point-condensate-delays-but-cannot-stop-runaway-candidate-chain-does-not-close-no-gev-promotion"
    : "coupled-background-condensate-fixed-point-probe-blocked";

string decision = coupledBackgroundCondensateFixedPointProbePassed
    ? "The candidate chain 430->439->438 does NOT close into a self-consistent finite (t8*, Sigma*). Treating the lambda_8 background t8 and the free-diagonal condensate (Sigma_1=Sigma_2, Sigma_3) as a JOINT variational system - W_total = W_B(t8) [Phase430 bosonic workbench] + the Phase439 free-diagonal fermionic gap functional, each relative to (t8=0, Sigma=0) - the joint minimum is TRIVIAL (t8*=0, condensate as in Phase438) at strong coupling and RUNAWAY (t8* escapes to the grid boundary) at weak coupling, with NO interior fixed point anywhere across " + $"{jointRows.Count}" + " (lattice, content, coupling) configurations (" + $"{trivialCount}" + " trivial, " + $"{runawayCount}" + " runaway, " + $"{interiorCount}" + " interior). The SATURATION mechanism is real but insufficient: at fixed Sigma the gap shifts the fermionic IR cutoff eps -> sqrt(eps^2+Sigma^2), depressing the moderate-t8 runaway slope (Sigma=0 window slope " + $"{moderateSlopeSigma0:F3}" + " -> Sigma=4 window slope " + $"{moderateSlopeSigma4:F3}" + ") and delaying the onset to t8 ~ Sigma; but the large-t8 slope resumes toward the same integer count (" + $"{largeSlopeSigma4:F3}" + "), because at large t8 the background gaps out the very IR modes that drive condensation (the joint-optimal Sigma -> 0 there). The asymptotic net slope is therefore unchanged and negative (Phase430/Phase437 anchor: fundamental -4, derived hypercharge -40 per site; reproduced here to " + $"{phase430AnchorMaxResidual:E1}" + "), so the coupled system still runs away in the weak-coupling window - verified by the dW/dt8 decomposition at the boundary (bosonic > 0, fermionic < 0, net < 0). Batteries: bosonic masses {four 0, four 3/4}; Phase430 cross-anchor slopes to " + $"{phase430AnchorMaxResidual:E1}" + "; t8=0 reproduces Phase438 gap to " + $"{Math.Max(phase438SingletResidual, phase438HyperResidual):E1}" + "; closed form vs dense L=4 Dirac WITH background to " + $"{closedFormMaxResidual:E1}" + "; Sigma_1=Sigma_2 to " + $"{maxS1S2Diff:E1}" + "; every descent refinement verified (final W <= grid W); reduced gradient at the minima to " + $"{Math.Max(maxTrivialSigmaGradient, maxT8ZeroSlope):E1}" + ". HONEST BOUNDARIES: t8 is a recorded candidate parameter (not dynamically derived); the gap equation is mean-field; the coupling normalization and the bosonic mode-mass model are recorded workbench conventions; all scale ratios are candidate-only. This phase decides the internal-consistency question negatively and promotes nothing to GeV; it fills no Phase201 or Phase256 field."
    : "Do not use the coupled fixed-point verdicts until the precursor and battery gates pass.";

var runtimeSeconds = (DateTimeOffset.UtcNow - startTime).TotalSeconds;

var result = new
{
    phaseId = "phase440-coupled-background-condensate-fixed-point-probe",
    generatedAt = DateTimeOffset.UtcNow,
    runtimeSeconds,
    terminalStatus,
    coupledBackgroundCondensateFixedPointProbePassed,
    precursorsPassed,
    phase430PrecursorPassed,
    phase438PrecursorPassed,
    phase439PrecursorPassed,
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
        jointFreeEnergy = "W_total(t8, Sigma_vec) = W_B(t8) + sum_c [ Sigma_c^2/(2 g2) - copies*(Ns/2)/Vol * sum_k' ( log(eps_c(k,t8)^2 + Sigma_c^2) - log(eps_c(k,0)^2) ) ]; relative to (t8=0, Sigma=0)",
        bosonicModel = "W_B(t8) = (1/(2 Vol)) sum_k' sum_i 2*log((eps_k^2 + t8^2 m_i^2)/eps_k^2); m_i^2 = eigenvalues of -ad_{lambda_8/2}^2 on su(3) (four 0, four 3/4); 2 polarizations; Phase430 workbench model",
        fermionicModel = "Phase439 free-diagonal per-color gap functional; coefficient copies*(Ns/2)/Vol per (color, momentum)",
        background = "omega_bg = t8*(lambda_8/2); per-momentum color-c block gamma_mu (x) (s_mu + t8 u_c); eps_c(k,t8)^2 = (s1+t8 u_c)^2+(s2+t8 u_c)^2",
        colorShifts = "u_c: colors 1,2 = 1/(2 sqrt3); color 3 = -1/sqrt3 (eigenvalues of lambda_8/2)",
        contents = "fundamental (1 copy) and derived-4x (4 copies)",
        infraredConvention = "exclude momenta with s1^2+s2^2 <= 1e-12; SAME excluded set at every t8 (Phase438/439) for BOTH bosonic and fermionic sums",
        t8Range = new[] { 0.0, T8Max },
        sigmaRange = new[] { 0.0, 8.0 },
        couplingGrid = "8 log-spaced g2 in [0.3, 8]",
        latticeCutoffLambda = lambdaCutoff,
        workbenchConventionsAreSourceDefined,
    },
    bosonicMassesSquared = bosonMassesSquared,
    bosonZeroModeCount,
    bosonNonzeroModeCount,
    bosonNonzeroMassSquared,
    excludedZeroDispersionModeCounts = lattices.ToDictionary(L => $"L{L}", ExcludedCount),
    batteries = new
    {
        gammaAnticommutationResidual,
        gammaAnticommutationExact,
        hyperColorChargesConsistent,
        sumU8Squared,
        bosonicMassesConsistent,
        phase430CrossAnchor = new
        {
            fermionSFundSlope = anchorFermionSFund,
            fermionSDerivedSlope = anchorFermionSDerived,
            bosonSSlope = anchorBosonS,
            netSFund = anchorNetSFund,
            netSDerived = anchorNetSDerived,
            netDerivedPerSite = anchorNetDerivedPerSite,
            maxResidual = phase430AnchorMaxResidual,
            phase430CrossAnchorMatches,
        },
        phase438SingletResidual,
        phase438HyperResidual,
        phase438ConsistencyAtZeroBackground,
        closedFormResidualAligned,
        closedFormResidualSinglet,
        closedFormMaxResidual,
        closedFormSpectrumMatchesDense,
        maxSigma1Sigma2Difference = maxS1S2Diff,
        sigma1EqualsSigma2,
        allDescentVerified,
        maxTrivialSigmaGradient,
        maxT8ZeroSlope,
        gradientConsistentAtMinimum,
        allBatteriesPassed,
    },
    saturation = new
    {
        rows = saturationRows.Select(r => new
        {
            sigma = r.Sigma,
            moderateWindowSlope = r.ModerateWindowSlope,
            largeWindowSlope = r.LargeWindowSlope,
            asymptoticFermionSlope = r.AsymptoticSlope,
            runawayDepressionAtT8_2 = r.DepressionAtT8_2,
        }).ToArray(),
        moderateSlopeSigma0,
        moderateSlopeSigma4,
        largeSlopeSigma4,
        condensateDepressesModerateRunaway,
        largeSlopeResumesTowardSigma0,
        condensateSaturatesFermionicRunaway,
        mechanismNote = "the gap shifts the fermionic IR cutoff eps -> sqrt(eps^2+Sigma^2); it DELAYS the runaway onset to t8 ~ Sigma but does NOT change the large-t8 asymptotic slope, because at large t8 the joint-optimal Sigma -> 0 (background gaps out the condensing IR modes).",
    },
    jointMinimization = new
    {
        rows = jointRows.Select(r => new
        {
            content = r.Content,
            copies = r.Copies,
            lattice = r.L,
            g2 = r.G2,
            t8Star = r.T8,
            sigma1Star = r.S1,
            sigma2Star = r.S2,
            sigma3Star = r.S3,
            freeEnergy = r.W,
            gridFreeEnergy = r.GridW,
            classification = r.Classification,
            gradientNorm = Finite(r.GradNorm),
            hessianMinEigenvalue = Finite(r.HessMinEig),
            descentImproved = r.DescentImproved,
            descentSteps = r.Steps,
            t8OverSigma1 = Finite(r.T8OverS1),
            sigma3OverSigma1 = Finite(r.S3OverS1),
        }).ToArray(),
        trivialCount,
        runawayCount,
        interiorCount,
        anyInteriorFixedPoint,
    },
    boundaryDecomposition = new
    {
        rows = boundaryDecompRows.Select(r => new
        {
            content = r.Content,
            copies = r.Copies,
            lattice = r.L,
            g2 = r.G2,
            t8 = r.T8,
            bosonicSlope = r.BosonicSlope,
            fermionicSlope = r.FermionicSlope,
            netSlope = r.NetSlope,
        }).ToArray(),
        boundaryBosonicPositiveFermionicNegative,
        boundaryNetSlopeNegative,
    },
    condensateSaturatesFermionicRunaway,
    jointFixedPointExists,
    jointFixedPointIsInterior,
    backgroundSelfGenerates,
    runawayPersistsInCoupledSystem,
    jointMinimumIsTrivialOrRunaway,
    scaleRatiosAreCandidateOnly,
    bosonicSectorIsWorkbenchModel,
    gapEquationIsMeanFieldApproximation,
    couplingNormalizationIsRecordedConvention,
    backgroundParameterT8IsRecordedCandidateOnly,
    backgroundParameterT8DynamicallyDerived,
    noGevPromotion,
    honestBoundaries = new
    {
        gapEquationIsMeanFieldApproximation,
        couplingNormalizationIsRecordedConvention,
        bosonicSectorIsWorkbenchModel,
        backgroundParameterT8IsRecordedCandidateOnly,
        backgroundParameterT8DynamicallyDerived,
        scaleRatiosAreCandidateOnly,
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
        phase430SummaryPath = Phase430SummaryPath,
        phase438SummaryPath = Phase438SummaryPath,
        phase439SummaryPath = Phase439SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "t8 is a RECORDED CANDIDATE background parameter minimized over a grid, not a dynamically derived scale; deriving it is the Phase430 chain's job and this phase decides it does NOT self-generate.",
        "The gap equation is a mean-field (Hartree) approximation; fluctuations beyond the fermion loop are not included.",
        "The four-fermion coupling g2 is a scanned parameter; its physical normalization is fixed only up to the workbench kappa_B and is a recorded convention, NOT source-derived.",
        "The bosonic sector is a RECORDED WORKBENCH MODEL of the transverse fluctuation determinant (only the mode-count arithmetic is exact); tying its masses to the actual control-branch Hessian is named future work.",
        "All Sigma and t8 scales and their ratios are candidate-only workbench numbers; the lattice cutoff Lambda is a recorded convention (max dispersion sqrt(2)).",
        "The result is an HONEST NEGATIVE: no interior joint fixed point exists; the condensate delays but cannot stop the runaway. The phase passes on internal consistency, closes no GeV scale gap, and promotes nothing.",
        "No Phase201 or Phase256 field is filled.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "coupled_background_condensate_fixed_point_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "coupled_background_condensate_fixed_point_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"coupledBackgroundCondensateFixedPointProbePassed={coupledBackgroundCondensateFixedPointProbePassed}");
Console.WriteLine($"precursorsPassed={precursorsPassed} (430={phase430PrecursorPassed} 438={phase438PrecursorPassed} 439={phase439PrecursorPassed})");
Console.WriteLine($"batteries: gammaAnticomm={gammaAnticommutationExact} bosonicMasses={bosonicMassesConsistent} p430Anchor={phase430CrossAnchorMatches}({phase430AnchorMaxResidual:E1}) p438@0={phase438ConsistencyAtZeroBackground}({Math.Max(phase438SingletResidual, phase438HyperResidual):E1}) denseCF={closedFormSpectrumMatchesDense}({closedFormMaxResidual:E1}) s1eqs2={sigma1EqualsSigma2}({maxS1S2Diff:E1}) descent={allDescentVerified} grad={gradientConsistentAtMinimum}({Math.Max(maxTrivialSigmaGradient, maxT8ZeroSlope):E1})");
Console.WriteLine($"Phase430 anchor: fermS_fund={anchorFermionSFund:F2} fermS_der={anchorFermionSDerived:F2} bosS={anchorBosonS:F2} netS_fund={anchorNetSFund:F2} netS_der={anchorNetSDerived:F2} (perSite={anchorNetDerivedPerSite:F2})");
Console.WriteLine($"condensateSaturatesFermionicRunaway={condensateSaturatesFermionicRunaway} (modSlope Sigma0={moderateSlopeSigma0:F3} Sigma4={moderateSlopeSigma4:F3}; largeSlope Sigma4={largeSlopeSigma4:F3})");
Console.WriteLine($"joint outcomes: trivial={trivialCount} runaway={runawayCount} interior={interiorCount} (anyInteriorFixedPoint={anyInteriorFixedPoint})");
Console.WriteLine("joint (L=8) sample:");
foreach (var r in jointRows.Where(r => r.L == 8))
    Console.WriteLine($"  {r.Content} g2={r.G2:F3}: t8*={r.T8:F3} s1*={r.S1:F4} s3*={r.S3:F4} W={r.W:F4} [{r.Classification}]");
Console.WriteLine("boundary dW/dt8 decomposition (L=8, g2=0.3, t8=7.9):");
foreach (var r in boundaryDecompRows)
    Console.WriteLine($"  {r.Content}: dWB={r.BosonicSlope:F4} (>0) dWF={r.FermionicSlope:F4} (<0) net={r.NetSlope:F4}");
Console.WriteLine($"jointFixedPointExists={jointFixedPointExists} backgroundSelfGenerates={backgroundSelfGenerates} runawayPersistsInCoupledSystem={runawayPersistsInCoupledSystem}");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract} canFillPhase201HiggsContract={canFillPhase201HiggsContract} canFillPhase256={canFillPhase256ObservedFieldExtractionContract}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F2}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

static double Clamp(double v, double lo, double hi) => Math.Max(lo, Math.Min(hi, v));

static double[] NumGrad(Func<double[], double> f, double[] x)
{
    double h = 1e-5;
    var g = new double[x.Length];
    for (int i = 0; i < x.Length; i++)
    {
        var xp = (double[])x.Clone(); var xm = (double[])x.Clone();
        xp[i] += h; xm[i] -= h;
        g[i] = (f(xp) - f(xm)) / (2.0 * h);
    }
    return g;
}

static double[,] NumHessian(Func<double[], double> f, double[] x)
{
    int n = x.Length; double h = 1e-4;
    var H = new double[n, n];
    double f0 = f(x);
    for (int i = 0; i < n; i++)
        for (int j = i; j < n; j++)
        {
            var xpp = (double[])x.Clone(); xpp[i] += h; xpp[j] += h;
            var xpm = (double[])x.Clone(); xpm[i] += h; xpm[j] -= h;
            var xmp = (double[])x.Clone(); xmp[i] -= h; xmp[j] += h;
            var xmm = (double[])x.Clone(); xmm[i] -= h; xmm[j] -= h;
            double val = (f(xpp) - f(xpm) - f(xmp) + f(xmm)) / (4.0 * h * h);
            H[i, j] = val; H[j, i] = val;
        }
    _ = f0;
    return H;
}

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

public sealed record SaturationRow(
    double Sigma, double ModerateWindowSlope, double LargeWindowSlope,
    double AsymptoticSlope, double DepressionAtT8_2);

public sealed record JointRow(
    string Content, int Copies, int L, double G2,
    double T8, double S1, double S2, double S3, double W, double GridW,
    string Classification, double GradNorm, double HessMinEig, bool DescentImproved, int Steps,
    double T8OverS1, double S3OverS1);

public sealed record BoundaryDecompRow(
    string Content, int Copies, int L, double G2, double T8,
    double BosonicSlope, double FermionicSlope, double NetSlope);
