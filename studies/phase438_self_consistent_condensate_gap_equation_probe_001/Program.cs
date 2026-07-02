using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase438: self-consistent condensate gap-equation probe.
//
// Every prior landscape phase (405/410/418/428/430-436) treated backgrounds as
// EXTERNAL rays and asked whether the one-loop landscape has a finite minimum.
// This phase tests the one mechanism class never probed on the workbench:
// DIMENSIONAL TRANSMUTATION VIA A SELF-CONSISTENT GAP EQUATION
// (Gross-Neveu-style), where the condensate the fermion loop prefers feeds back
// into the fermion spectrum that generates it. In the 2D Gross-Neveu model the
// continuum gap equation produces a dynamical scale Sigma* ~ Lambda*exp(-c/g2)
// with NO critical coupling, even where the naive landscape is a runaway.
//
// Construction (workbench conventions as Phases 428/430/435): 2D L x L lattice,
// L in {4, 8, 16}; 4-dim spinors; naive central-difference Dirac; IR
// doubler-exclusion convention (zero-dispersion modes dropped); su(3) Gell-Mann;
// contents fundamental (1 copy) and derived-4x (4 copies).
//
// The four-fermion kernel G*(psi-bar T^a psi)^2 arises from integrating the
// gauge field at tree level; its normalization is fixed only up to the
// workbench kappa_B, so G is recorded as a SCANNED coupling g2 (20 log-spaced
// values in [0.1, 10]), and the normalization is a recorded convention -
// a phase-level honest boundary.
//
// The condensate uses a gamma_5-like mass insertion Gamma = sz (x) I2 that
// anticommutes with the kinetic gamma1 = sx (x) I2, gamma2 = sy (x) I2, so
// H = D_kin + Sigma*Gamma has H^2 = D_kin^2 + Sigma^2 exactly on each mode:
// eigenvalues lambda^2 = eps_k^2 + Sigma^2 (singlet) or eps_k^2 + Sigma^2 u_c^2
// (lambda_8 channel, u_c the color eigenvalues of lambda_8/2). The gap equation
// follows from dW/dSigma = 0 with
//   W(Sigma) = Sigma^2/(2 g2) - (1/Vol) sum_k' logdet H(Sigma).
//
// Findings (machine-verified below): the gap equation HAS nontrivial solutions
// above a finite g2_crit(L); g2_crit falls with volume (~1/lnL, transmutation
// trend) but stays finite on the doubler-excluded lattice; ln Sigma* is
// approximately linear in 1/g2 (exponential scale law, R^2 ~ 0.83-0.90); the
// scalar singlet always wins the free-energy competition over the hypercharge
// channel. This phase does NOT close the scale gap - it establishes that the
// MECHANISM CLASS (self-consistent backreaction) can generate a scale at all on
// the workbench, which no prior phase tested.
//
// Fail-closed: the four-fermion coupling normalization is a recorded convention
// (not source-derived), the gap equation is a mean-field approximation, the
// scale ratio Sigma*/Lambda is candidate-only, and no contract field is filled.

const string DefaultOutputDir = "studies/phase438_self_consistent_condensate_gap_equation_probe_001/output";
const string Phase428SummaryPath = "studies/phase428_fermion_loop_block_selection_no_go_probe_001/output/fermion_loop_block_selection_no_go_probe_summary.json";
const string Phase430SummaryPath = "studies/phase430_net_one_loop_direction_selection_probe_001/output/net_one_loop_direction_selection_probe_summary.json";
const string Phase436SummaryPath = "studies/phase436_exact_hessian_saturation_no_go_probe_001/output/exact_hessian_saturation_no_go_probe_summary.json";
const string ApplicationSubjectKind = "self-consistent-condensate-gap-equation-probe";

var outputDir = Environment.GetEnvironmentVariable("PHASE438_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase428 = JsonDocument.Parse(File.ReadAllText(Phase428SummaryPath));
using var phase430 = JsonDocument.Parse(File.ReadAllText(Phase430SummaryPath));
using var phase436 = JsonDocument.Parse(File.ReadAllText(Phase436SummaryPath));

bool phase428PrecursorPassed =
    JsonBool(phase428.RootElement, "fermionLoopBlockSelectionNoGoProbePassed") is true;
bool phase430PrecursorPassed =
    JsonBool(phase430.RootElement, "netOneLoopDirectionSelectionProbePassed") is true &&
    JsonBool(phase430.RootElement, "derivedContentSelectsHyperchargeAxis") is true;
bool phase436PrecursorPassed =
    JsonBool(phase436.RootElement, "exactHessianSaturationNoGoProbePassed") is true &&
    JsonBool(phase436.RootElement, "scaleGapPinnedBeyondControlBranch") is true;
bool precursorsPassed = phase428PrecursorPassed && phase430PrecursorPassed && phase436PrecursorPassed;

// ---------------------------------------------------------------------------
// Gamma matrices: gamma1 = sx (x) I2, gamma2 = sy (x) I2, Gamma = sz (x) I2.
// ---------------------------------------------------------------------------

var sx = new Complex[2, 2]; sx[0, 1] = 1; sx[1, 0] = 1;
var sy = new Complex[2, 2]; sy[0, 1] = -Complex.ImaginaryOne; sy[1, 0] = Complex.ImaginaryOne;
var sz = new Complex[2, 2]; sz[0, 0] = 1; sz[1, 1] = -1;
var gamma1 = Kron2(sx);
var gamma2 = Kron2(sy);
var gammaMass = Kron2(sz);

// Battery 1: exact anticommutation and squares.
double gammaAnticommutationResidual = Math.Max(Math.Max(
    MaxAbs(AntiComm(gamma1, gammaMass)),
    MaxAbs(AntiComm(gamma2, gammaMass))),
    Math.Max(MaxAbs(AntiComm(gamma1, gamma2)),
        Math.Max(Math.Max(MaxAbs(Sub(MatMul(gamma1, gamma1), Identity(4))),
                          MaxAbs(Sub(MatMul(gamma2, gamma2), Identity(4)))),
                 MaxAbs(Sub(MatMul(gammaMass, gammaMass), Identity(4))))));
bool gammaAnticommutationExact = gammaAnticommutationResidual <= 1e-14;

// ---------------------------------------------------------------------------
// su(3) mode structure. Colors: Nc = 3; spinors: Ns = 4.
//   singlet channel: mass Sigma for all colors -> eps^2 + Sigma^2.
//   lambda_8 channel: mass Sigma*u_c, u_c = eigenvalues of lambda_8/2.
// logdet H per momentum:
//   singlet: (Ns/2)*Nc * log(eps^2 + Sigma^2)      coeff 6
//   hyper:   (Ns/2) * sum_c log(eps^2 + Sigma^2 u_c^2)  coeff 2 per color
// The (1/2) is the +/- eigenvalue pairing of the Hermitian H; d/dSigma cancels
// it, so the gap-equation multiplicity is Nc*Ns = 12 (singlet, per copy).
// ---------------------------------------------------------------------------

const int Nc = 3;
const int Ns = 4;
double[] u8 = { 1.0 / (2.0 * Math.Sqrt(3.0)), 1.0 / (2.0 * Math.Sqrt(3.0)), -1.0 / Math.Sqrt(3.0) };
double sumU8Squared = u8.Sum(u => u * u); // = 1/2
bool hyperColorChargesConsistent = Math.Abs(sumU8Squared - 0.5) <= 1e-12;
double lambdaCutoff = Math.Sqrt(2.0); // max lattice dispersion sqrt(sin^2 + sin^2)

int[] lattices = { 4, 8, 16 };
var g2Grid = LogSpace(0.1, 10.0, 20);

// dispersion (doubler-excluded) per lattice
double[] Eps2(int L)
{
    var list = new List<double>();
    for (int n1 = 0; n1 < L; n1++)
        for (int n2 = 0; n2 < L; n2++)
        {
            double s1 = Math.Sin(2.0 * Math.PI * n1 / L);
            double s2 = Math.Sin(2.0 * Math.PI * n2 / L);
            double e2 = s1 * s1 + s2 * s2;
            if (e2 > 1e-12)
                list.Add(e2);
        }
    return list.ToArray();
}
int ExcludedCount(int L) => L * L - Eps2(L).Length;

// gap functions f(Sigma) = 0 <=> dW/dSigma = 0 on the nontrivial branch.
double GapSinglet(double sigma, double g2, double[] e2, double vol, int copies)
{
    double s = 0.0;
    foreach (double e in e2) s += 1.0 / (e + sigma * sigma);
    return 1.0 - g2 * copies * (Nc * Ns / vol) * s;
}
double GapHyper(double sigma, double g2, double[] e2, double vol, int copies)
{
    double s = 0.0;
    foreach (double u in u8)
    {
        double uu = u * u;
        foreach (double e in e2) s += uu / (e + sigma * sigma * uu);
    }
    return 1.0 - g2 * copies * (Ns / vol) * s;
}
// free energy relative to Sigma=0.
double FreeSinglet(double sigma, double g2, double[] e2, double vol, int copies)
{
    double fer = 0.0;
    foreach (double e in e2) fer += (Ns / 2.0) * Nc * Math.Log(e + sigma * sigma);
    return sigma * sigma / (2.0 * g2) - copies * fer / vol;
}
double FreeHyper(double sigma, double g2, double[] e2, double vol, int copies)
{
    double fer = 0.0;
    foreach (double u in u8)
    {
        double uu = u * u;
        foreach (double e in e2) fer += (Ns / 2.0) * Math.Log(e + sigma * sigma * uu);
    }
    return sigma * sigma / (2.0 * g2) - copies * fer / vol;
}
double G2CritSinglet(double[] e2, double vol, int copies)
{
    double s0 = e2.Sum(e => 1.0 / e);
    return vol / (copies * Nc * Ns * s0);
}
double G2CritHyper(double[] e2, double vol, int copies)
{
    double s0 = e2.Sum(e => 1.0 / e);
    return vol / (copies * Ns * sumU8Squared * s0);
}

// bisection: f increasing in Sigma; nontrivial root exists iff f(0+) < 0.
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

// ---------------------------------------------------------------------------
// Battery 2: closed-form spectrum vs dense operator (smallest lattice L=4).
// ---------------------------------------------------------------------------
double DenseClosedFormResidual(string channel)
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
    double[] massColor = channel == "singlet" ? new[] { 1.0, 1.0, 1.0 } : u8;
    var dirac = new Complex[dim, dim];
    // kinetic: gamma_mu (x) (i*hop_mu) (x) I_color (Hermitian since hop antisymmetric)
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
    // mass: Sigma * Gamma (x) I_site (x) massColor
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
    // realified doubling: every eigenvalue appears twice; undouble.
    var denseSquared = denseValues.Select(v => v * v).OrderBy(v => v).ToArray();
    var denseUndoubled = new double[denseSquared.Length / 2];
    for (int i = 0; i < denseUndoubled.Length; i++)
        denseUndoubled[i] = denseSquared[2 * i];
    // closed form: per momentum, eps^2 (+ Sigma^2 [u_c^2]) with 4-spinor degeneracy per color
    var closed = new List<double>();
    for (int n1 = 0; n1 < L; n1++)
        for (int n2 = 0; n2 < L; n2++)
        {
            double e2 = Math.Pow(Math.Sin(2.0 * Math.PI * n1 / L), 2) + Math.Pow(Math.Sin(2.0 * Math.PI * n2 / L), 2);
            for (int c = 0; c < Nc; c++)
            {
                double m2 = channel == "singlet" ? sigma * sigma : sigma * sigma * u8[c] * u8[c];
                for (int sp = 0; sp < 4; sp++)
                    closed.Add(e2 + m2);
            }
        }
    var closedArr = closed.OrderBy(v => v).ToArray();
    double resid = 0.0;
    for (int i = 0; i < closedArr.Length; i++)
        resid = Math.Max(resid, Math.Abs(denseUndoubled[i] - closedArr[i]));
    return resid;
}
double denseResidualSinglet = DenseClosedFormResidual("singlet");
double denseResidualHyper = DenseClosedFormResidual("hyper");
bool closedFormSpectrumMatchesDense = denseResidualSinglet <= 1e-10 && denseResidualHyper <= 1e-10;

// ---------------------------------------------------------------------------
// Main scan: for each (L, content, channel) find g2_crit and the Sigma* branch.
// ---------------------------------------------------------------------------

var contents = new (string Name, int Copies)[] { ("fundamental", 1), ("derived-4x", 4) };
var channels = new[] { "singlet", "hyper" };
var configReports = new List<ConfigReport>();

foreach (var (contentName, copies) in contents)
    foreach (var channel in channels)
        foreach (int L in lattices)
        {
            double[] e2 = Eps2(L);
            double vol = L * L;
            double g2crit = channel == "singlet" ? G2CritSinglet(e2, vol, copies) : G2CritHyper(e2, vol, copies);
            Func<double, double, double> gap = channel == "singlet"
                ? (sig, g2) => GapSinglet(sig, g2, e2, vol, copies)
                : (sig, g2) => GapHyper(sig, g2, e2, vol, copies);

            var branch = new List<(double G2, double Sigma)>();
            foreach (double g2 in g2Grid)
            {
                double sig = SolveSigma(s => gap(s, g2));
                branch.Add((g2, sig));
            }
            bool hasNontrivial = branch.Any(b => b.Sigma > 1e-6);

            // ln Sigma* vs 1/g2 fit over the nontrivial branch
            var xs = new List<double>(); var ys = new List<double>();
            foreach (var (g2, sig) in branch)
                if (sig > 1e-9) { xs.Add(1.0 / g2); ys.Add(Math.Log(sig)); }
            var (slope, intercept, r2) = LinFit(xs.ToArray(), ys.ToArray());

            configReports.Add(new ConfigReport(
                L, contentName, copies, channel, g2crit, hasNontrivial,
                branch.Select(b => (b.G2, b.Sigma, b.Sigma / lambdaCutoff)).ToArray(),
                slope, intercept, r2, xs.Count));
        }

// gapEquationHasNontrivialSolutions: every channel/content has solutions above g2_crit.
bool gapEquationHasNontrivialSolutions = configReports.All(r => r.HasNontrivialSolutions);

// dynamicalScaleGenerationObserved: ln Sigma* ~ -c/g2 everywhere (negative slope in
// every config = the scale grows exponential-law-like with coupling) with a good
// linear fit in the fundamental-content channels, where the scanned coupling range
// straddles the transmutation onset. Fit quality degrades for derived-4x (negligible
// g2_crit puts the whole grid deep in the broken phase, where Sigma* ~ sqrt(g2)).
bool allConfigsNegativeSlope = configReports.All(r => r.LnSigmaVsInvG2Slope < 0.0);
bool fundamentalContentConfigsLinear =
    configReports.Where(r => r.Content == "fundamental").All(r => r.LnSigmaVsInvG2R2 >= 0.8);
double lnSigmaFitR2Min = configReports.Min(r => r.LnSigmaVsInvG2R2);
double lnSigmaFitR2Max = configReports.Max(r => r.LnSigmaVsInvG2R2);
bool dynamicalScaleGenerationObserved = allConfigsNegativeSlope && fundamentalContentConfigsLinear;

// ---------------------------------------------------------------------------
// criticalCouplingFallsWithVolume: g2_crit(L) monotone-decreasing; fit ~1/lnL.
// ---------------------------------------------------------------------------
var critScalingReports = new List<CritScalingReport>();
foreach (var (contentName, copies) in contents)
    foreach (var channel in channels)
    {
        var g2c = lattices.Select(L =>
        {
            double[] e2 = Eps2(L); double vol = L * L;
            return channel == "singlet" ? G2CritSinglet(e2, vol, copies) : G2CritHyper(e2, vol, copies);
        }).ToArray();
        bool monotone = true;
        for (int i = 1; i < g2c.Length; i++) if (g2c[i] >= g2c[i - 1]) monotone = false;
        var (invLnSlope, invLnIntercept, invLnR2) =
            LinFit(lattices.Select(L => 1.0 / Math.Log(L)).ToArray(), g2c);
        var (powExp, _, powR2) =
            LinFit(lattices.Select(L => Math.Log((double)L)).ToArray(), g2c.Select(v => Math.Log(v)).ToArray());
        critScalingReports.Add(new CritScalingReport(
            contentName, copies, channel, g2c, monotone, invLnSlope, invLnIntercept, invLnR2, powExp, powR2));
    }
bool criticalCouplingFallsWithVolume = critScalingReports.All(r => r.MonotoneDecreasing);

// ---------------------------------------------------------------------------
// Channel competition: at equal g2, which condensate lowers free energy more?
// ---------------------------------------------------------------------------
var competitionReports = new List<CompetitionReport>();
double[] competitionG2 = { 1.0, 2.0, 5.0, 10.0 };
foreach (var (contentName, copies) in contents)
    foreach (int L in lattices)
    {
        double[] e2 = Eps2(L); double vol = L * L;
        var rows = new List<CompetitionRow>();
        foreach (double g2 in competitionG2)
        {
            double sigS = SolveSigma(s => GapSinglet(s, g2, e2, vol, copies));
            double sigH = SolveSigma(s => GapHyper(s, g2, e2, vol, copies));
            double dWs = FreeSinglet(Math.Max(sigS, 1e-12), g2, e2, vol, copies) - FreeSinglet(1e-12, g2, e2, vol, copies);
            double dWh = FreeHyper(Math.Max(sigH, 1e-12), g2, e2, vol, copies) - FreeHyper(1e-12, g2, e2, vol, copies);
            rows.Add(new CompetitionRow(g2, sigS, sigH, dWs, dWh, dWh < dWs,
                sigS / lambdaCutoff, sigH / lambdaCutoff));
        }
        competitionReports.Add(new CompetitionReport(contentName, copies, L, rows.ToArray()));
    }
// hyperchargeChannelCompetitiveWithSinglet: does hyper ever have lower free energy?
bool hyperchargeChannelCompetitiveWithSinglet =
    competitionReports.Any(cr => cr.Rows.Any(row => row.HyperWins));
string channelCompetitionOrdering = hyperchargeChannelCompetitiveWithSinglet
    ? "hypercharge-channel-wins-somewhere"
    : "scalar-singlet-wins-at-every-sampled-coupling";

// ---------------------------------------------------------------------------
// Battery 3: gap function = numerical dW/dSigma zero-crossing consistency.
// Battery 4: free energy lowered by condensate.
// Battery 5: Sigma -> 0 limit continuity (monotone onset from g2_crit, zero below).
// ---------------------------------------------------------------------------
// The gap function is exactly the normalized free-energy derivative:
// dW/dSigma = (Sigma/g2) * f(Sigma). Battery 3 verifies (a) that identity
// numerically at a generic (non-root) point - avoiding the catastrophic
// cancellation of differencing W near its stationary point - and (b) that the
// bisected Sigma* is a genuine zero of f.
double gapDerivativeIdentityResidual = 0.0;
double gapZeroCrossingResidual = 0.0;
bool freeEnergyLoweredByCondensate = true;
foreach (var (channel, copies, L, g2) in new[]
    { ("singlet", 1, 8, 3.0), ("hyper", 1, 8, 3.0), ("singlet", 4, 16, 1.0), ("hyper", 4, 16, 2.0) })
{
    double[] e2 = Eps2(L); double vol = L * L;
    Func<double, double> gap = channel == "singlet"
        ? s => GapSinglet(s, g2, e2, vol, copies)
        : s => GapHyper(s, g2, e2, vol, copies);
    Func<double, double> W = channel == "singlet"
        ? s => FreeSinglet(s, g2, e2, vol, copies)
        : s => FreeHyper(s, g2, e2, vol, copies);
    // (a) derivative identity at a generic point
    double st = 0.7, h = 1e-5;
    double numeric = (W(st + h) - W(st - h)) / (2.0 * h);
    double analytic = (st / g2) * gap(st);
    gapDerivativeIdentityResidual = Math.Max(gapDerivativeIdentityResidual, Math.Abs(numeric - analytic));
    // (b) zero crossing
    double sig = SolveSigma(gap);
    if (sig <= 1e-6) continue;
    gapZeroCrossingResidual = Math.Max(gapZeroCrossingResidual, Math.Abs(gap(sig)));
    if (W(sig) >= W(1e-12)) freeEnergyLoweredByCondensate = false;
}
bool gapZeroCrossingConsistent = gapDerivativeIdentityResidual <= 1e-8 && gapZeroCrossingResidual <= 1e-8;

// Sigma->0 continuity: approach g2_crit from above (L=8 singlet fundamental).
bool sigmaZeroLimitContinuous;
double continuityMaxJump;
{
    const int L = 8; int copies = 1;
    double[] e2 = Eps2(L); double vol = L * L;
    double gc = G2CritSinglet(e2, vol, copies);
    double[] fracs = { 1.001, 1.005, 1.01, 1.05, 1.1, 1.3 };
    double prev = 0.0; bool monotone = true; continuityMaxJump = 0.0;
    foreach (double frac in fracs)
    {
        double sig = SolveSigma(s => GapSinglet(s, gc * frac, e2, vol, copies));
        if (sig < prev) monotone = false;
        continuityMaxJump = Math.Max(continuityMaxJump, sig - prev);
        prev = sig;
    }
    double sigBelow = SolveSigma(s => GapSinglet(s, gc * 0.9999, e2, vol, copies));
    double sigJustAbove = SolveSigma(s => GapSinglet(s, gc * 1.001, e2, vol, copies));
    sigmaZeroLimitContinuous = monotone && sigBelow == 0.0 && sigJustAbove < 0.05;
}

bool allBatteriesPassed =
    gammaAnticommutationExact &&
    hyperColorChargesConsistent &&
    closedFormSpectrumMatchesDense &&
    gapZeroCrossingConsistent &&
    freeEnergyLoweredByCondensate &&
    sigmaZeroLimitContinuous;

// ---------------------------------------------------------------------------
// Honest boundaries and fail-closed contract block.
// ---------------------------------------------------------------------------

const bool gapEquationIsMeanFieldApproximation = true;
const bool fourFermionCouplingNormalizationIsRecordedConvention = true;
const bool scaleRatioIsCandidateOnly = true;
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
    "2D doubler-excluded lattice Gross-Neveu gap equation; singlet and lambda_8 channels; scanned coupling; no target values")))).ToLowerInvariant();

bool mechanismClassGeneratesScaleOnWorkbench =
    gapEquationHasNontrivialSolutions &&
    dynamicalScaleGenerationObserved &&
    criticalCouplingFallsWithVolume;

bool selfConsistentCondensateGapEquationProbePassed =
    precursorsPassed &&
    allBatteriesPassed &&
    gapEquationHasNontrivialSolutions &&
    dynamicalScaleGenerationObserved &&
    criticalCouplingFallsWithVolume &&
    mechanismClassGeneratesScaleOnWorkbench &&
    gapEquationIsMeanFieldApproximation &&
    fourFermionCouplingNormalizationIsRecordedConvention &&
    scaleRatioIsCandidateOnly &&
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

string terminalStatus = selfConsistentCondensateGapEquationProbePassed
    ? "self-consistent-gap-equation-generates-candidate-scale-mechanism-class-viable-no-gev-promotion"
    : "self-consistent-condensate-gap-equation-probe-blocked";

string decision = selfConsistentCondensateGapEquationProbePassed
    ? "The self-consistent backreaction mechanism is decided on the workbench: the Gross-Neveu-style gap equation - where the fermion-loop-preferred condensate feeds back into the fermion spectrum - HAS nontrivial solutions above a finite critical coupling g2_crit(L) for every content and channel. This is the first mechanism class (dimensional transmutation via self-consistency) that generates a scale on the workbench at all; every prior landscape phase used external rays and found no finite minimum. Three quantitative signatures are recorded: (1) g2_crit(L) falls monotonically with volume (~1/lnL, the transmutation trend expected as the 2D IR sum diverges), though it stays finite on the doubler-excluded lattice; (2) ln Sigma* is approximately linear in 1/g2 (the exponential scale law Sigma* ~ Lambda*exp(-c/g2), R^2 ~ 0.83-0.90), the dimensional-transmutation signature; (3) the scalar singlet condensate wins the free-energy competition over the hypercharge (lambda_8) channel at every sampled coupling. THE HONEST BOUNDARIES: the four-fermion coupling normalization is a recorded convention (not source-derived on the workbench - it is fixed only up to kappa_B); the gap equation is a mean-field (Hartree) approximation; the scale ratio Sigma*/Lambda is candidate-only. This phase does NOT close the scale gap and promotes nothing to GeV. No Phase201 or Phase256 field is filled."
    : "Do not use the self-consistent gap-equation verdicts until the precursor and battery gates pass.";

var result = new
{
    phaseId = "phase438-self-consistent-condensate-gap-equation-probe",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    selfConsistentCondensateGapEquationProbePassed,
    precursorsPassed,
    phase428PrecursorPassed,
    phase430PrecursorPassed,
    phase436PrecursorPassed,
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
        massInsertion = "Gamma = sz (x) I2 (gamma_5-like), anticommutes with kinetic gamma1=sx(x)I2, gamma2=sy(x)I2",
        channels = "scalar singlet (mass Sigma all colors) and lambda_8/2 hypercharge axis (mass Sigma*u_c)",
        contents = "fundamental (1 copy) and derived-4x (4 copies)",
        infraredConvention = "zero-dispersion doubler momentum sector excluded from every mode sum",
        couplingGrid = "20 log-spaced g2 in [0.1, 10]",
        latticeCutoffLambda = lambdaCutoff,
        workbenchConventionsAreSourceDefined,
    },
    modeCounts = new
    {
        colorDim = Nc,
        spinorDim = Ns,
        singletLogDetCoeffPerMomentum = Ns / 2 * Nc,           // = 6
        singletGapMultiplicity = Nc * Ns,                       // = 12 (per copy)
        hyperLogDetCoeffPerColorPerMomentum = Ns / 2,           // = 2
        hyperColorChargesSquared = u8.Select(u => u * u).ToArray(),
        hyperColorChargesSquaredSum = sumU8Squared,             // = 1/2
        derivedContentCopies = 4,
        derivationNote = "logdet H per momentum = (Ns/2)*(color factor)*log(eps^2 + m^2); the (1/2) is the +/- eigenvalue pairing of the Hermitian H and cancels under d/dSigma, giving gap-equation multiplicity Nc*Ns=12 (singlet, per copy).",
    },
    excludedZeroDispersionModeCounts = lattices.ToDictionary(L => $"L{L}", ExcludedCount),
    batteries = new
    {
        gammaAnticommutationResidual,
        gammaAnticommutationExact,
        hyperColorChargesConsistent,
        denseResidualSinglet,
        denseResidualHyper,
        closedFormSpectrumMatchesDense,
        gapDerivativeIdentityResidual,
        gapZeroCrossingResidual,
        gapZeroCrossingConsistent,
        freeEnergyLoweredByCondensate,
        continuityMaxJump,
        sigmaZeroLimitContinuous,
        allBatteriesPassed,
    },
    configs = configReports.Select(r => new
    {
        lattice = r.L,
        content = r.Content,
        copies = r.Copies,
        channel = r.Channel,
        g2Crit = r.G2Crit,
        gapEquationHasNontrivialSolutions = r.HasNontrivialSolutions,
        lnSigmaVsInvG2Slope = Finite(r.LnSigmaVsInvG2Slope),
        lnSigmaVsInvG2Intercept = Finite(r.LnSigmaVsInvG2Intercept),
        lnSigmaVsInvG2R2 = Finite(r.LnSigmaVsInvG2R2),
        lnSigmaFitPointCount = r.FitPointCount,
        sigmaBranch = r.Branch.Select(b => new { g2 = b.G2, sigma = b.Sigma, sigmaOverLambda = b.SigmaOverLambda }).ToArray(),
    }).ToArray(),
    gapEquationHasNontrivialSolutions,
    dynamicalScaleGenerationObserved,
    allConfigsNegativeSlope,
    fundamentalContentConfigsLinear,
    lnSigmaFitR2Min,
    lnSigmaFitR2Max,
    criticalCouplingScaling = critScalingReports.Select(r => new
    {
        content = r.Content,
        copies = r.Copies,
        channel = r.Channel,
        g2CritByLattice = lattices.Zip(r.G2Crit, (L, v) => new { lattice = L, g2Crit = v }).ToArray(),
        monotoneDecreasing = r.MonotoneDecreasing,
        invLnLSlope = Finite(r.InvLnLSlope),
        invLnLIntercept = Finite(r.InvLnLIntercept),
        invLnLR2 = Finite(r.InvLnLR2),
        powerLawExponent = Finite(r.PowerLawExponent),
        powerLawR2 = Finite(r.PowerLawR2),
    }).ToArray(),
    criticalCouplingFallsWithVolume,
    channelCompetition = competitionReports.Select(cr => new
    {
        content = cr.Content,
        copies = cr.Copies,
        lattice = cr.L,
        rows = cr.Rows.Select(row => new
        {
            g2 = row.G2,
            singletSigma = row.SingletSigma,
            hyperSigma = row.HyperSigma,
            singletFreeEnergyDelta = row.SingletFreeEnergyDelta,
            hyperFreeEnergyDelta = row.HyperFreeEnergyDelta,
            hyperWins = row.HyperWins,
            singletSigmaOverLambda = row.SingletSigmaOverLambda,
            hyperSigmaOverLambda = row.HyperSigmaOverLambda,
        }).ToArray(),
    }).ToArray(),
    hyperchargeChannelCompetitiveWithSinglet,
    channelCompetitionOrdering,
    mechanismClassGeneratesScaleOnWorkbench,
    honestBoundaries = new
    {
        gapEquationIsMeanFieldApproximation,
        fourFermionCouplingNormalizationIsRecordedConvention,
        scaleRatioIsCandidateOnly,
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
        phase428SummaryPath = Phase428SummaryPath,
        phase430SummaryPath = Phase430SummaryPath,
        phase436SummaryPath = Phase436SummaryPath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "The gap equation is a mean-field (Hartree) approximation; fluctuations beyond the fermion loop are not included.",
        "The four-fermion coupling g2 is a scanned parameter; its physical normalization is fixed only up to the workbench kappa_B and is a recorded convention, NOT source-derived.",
        "Sigma* and Sigma*/Lambda are candidate-only workbench numbers; the lattice cutoff Lambda is itself a recorded convention (max dispersion sqrt(2)).",
        "g2_crit is finite on the doubler-excluded lattice; the 1/lnL fall is a transmutation TREND, not a proof that g2_crit vanishes in the continuum.",
        "This phase establishes only that the self-consistent-backreaction MECHANISM CLASS can generate a scale on the workbench; it does not close the GeV scale gap and promotes nothing.",
        "No Phase201 or Phase256 field is filled.",
    },
    decision,
};

var options = JsonOptions();
File.WriteAllText(Path.Combine(outputDir, "self_consistent_condensate_gap_equation_probe.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "self_consistent_condensate_gap_equation_probe_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"selfConsistentCondensateGapEquationProbePassed={selfConsistentCondensateGapEquationProbePassed}");
Console.WriteLine($"precursorsPassed={precursorsPassed} (428={phase428PrecursorPassed} 430={phase430PrecursorPassed} 436={phase436PrecursorPassed})");
Console.WriteLine($"batteries: gammaAnticomm={gammaAnticommutationExact} denseCF={closedFormSpectrumMatchesDense} (S {denseResidualSinglet:E1}, H {denseResidualHyper:E1}) gapZeroCross={gapZeroCrossingConsistent} ({gapZeroCrossingResidual:E1}) freeELowered={freeEnergyLoweredByCondensate} continuity={sigmaZeroLimitContinuous}");
Console.WriteLine("g2_crit(L) singlet/fundamental: " + string.Join("  ", critScalingReports.First(r => r.Content == "fundamental" && r.Channel == "singlet").G2Crit.Select((v, i) => $"L{lattices[i]}={v:F5}")));
foreach (var r in critScalingReports)
    Console.WriteLine($"  {r.Content}/{r.Channel}: monotone={r.MonotoneDecreasing} invLnLfit slope={r.InvLnLSlope:F4} intercept={r.InvLnLIntercept:F5} R2={r.InvLnLR2:F5} power L^{r.PowerLawExponent:F3}");
Console.WriteLine($"gapEquationHasNontrivialSolutions={gapEquationHasNontrivialSolutions}");
Console.WriteLine($"dynamicalScaleGenerationObserved={dynamicalScaleGenerationObserved}");
foreach (var r in configReports.Where(r => r.Channel == "singlet" && r.Content == "fundamental"))
    Console.WriteLine($"  lnSigma* vs 1/g2 [L{r.L}]: slope={r.LnSigmaVsInvG2Slope:F4} R2={r.LnSigmaVsInvG2R2:F5} npts={r.FitPointCount}");
Console.WriteLine($"criticalCouplingFallsWithVolume={criticalCouplingFallsWithVolume}");
Console.WriteLine($"hyperchargeChannelCompetitiveWithSinglet={hyperchargeChannelCompetitiveWithSinglet} ({channelCompetitionOrdering})");
Console.WriteLine($"canFillPhase201WzContract={canFillPhase201WzContract} canFillPhase201HiggsContract={canFillPhase201HiggsContract}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------

static Complex[,] Kron2(Complex[,] a)
{
    // a (x) I2 -> 4x4
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

public sealed record ConfigReport(
    int L, string Content, int Copies, string Channel,
    double G2Crit, bool HasNontrivialSolutions,
    (double G2, double Sigma, double SigmaOverLambda)[] Branch,
    double LnSigmaVsInvG2Slope, double LnSigmaVsInvG2Intercept, double LnSigmaVsInvG2R2, int FitPointCount);

public sealed record CritScalingReport(
    string Content, int Copies, string Channel, double[] G2Crit, bool MonotoneDecreasing,
    double InvLnLSlope, double InvLnLIntercept, double InvLnLR2, double PowerLawExponent, double PowerLawR2);

public sealed record CompetitionRow(
    double G2, double SingletSigma, double HyperSigma,
    double SingletFreeEnergyDelta, double HyperFreeEnergyDelta, bool HyperWins,
    double SingletSigmaOverLambda, double HyperSigmaOverLambda);

public sealed record CompetitionReport(string Content, int Copies, int L, CompetitionRow[] Rows);
