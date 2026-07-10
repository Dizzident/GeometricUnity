using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase453: WHAM PARITY-ANTISYMMETRY ERROR-MODEL REPAIR (Team B, Wave-1 item 1
// of the 2026-07-10 three-team elimination program; binding spec
// docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md section 2).
//
// CHARGE. Phase450 found single-well-at-zero everywhere under all four binding
// conditions but is verdictKind=inconclusive-gates-failed on exactly ONE gate:
// sd2 parity antisymmetry at 5.06 sigma (committed antisymmetryMaxSigma
// = 5.0594) while the INDEPENDENT unconstrained tadpole is zero
// (-0.04 +- 0.10). The exact on-ray Z2 (S_B even in Phi; a3 == 0 identically)
// FORBIDS a true odd constraint-effective-potential, so the discrepancy is
// either (a) a WHAM error-model ARTIFACT -- the per-bin errors are treated as
// independent while U(+Phi) and U(-Phi) share every jointly-solved stitching
// constant f_i (committed tadpole-window tauIntPhi = 40.35 sd2 / 72.9 identity,
// N_eff ~ 105 / 62) -- or (b) genuine Z2 breaking, the very signature the probe
// exists to find. Phase453 DISCRIMINATES with two INDEPENDENT estimator arms on
// FRESH ensembles, both propagating the f_i stitching-constant covariance that
// the phase450 lower-bound error model neglected.
//
// STAGE 0 (pre-registration -- ALL committed BEFORE any production trajectory;
// this is what PHASE453_MODE=preregister emits and what the checkpoint commits):
//   1. Recompute the per-bin antisymmetry localization from the COMMITTED
//      phase450 cepCurve (only max fields are committed there) and record it as
//      committed provenance for the motivating discrepancy.
//   2. Output schema records per-bin antisymmetry AND per-window signed-bin
//      counts -- never just the max (the summaries-only lesson applied to the
//      gate under repair).
//   3. Pre-register the c = 0 / +-0.25 soft-spring sub-ladder straddling the
//      origin (spring from the committed phase450 auto-spring rule at the
//      recorded 2.0 softening factor); SMOKE-TEST the neighbor-overlap gate at
//      the junction to the phase450-style ladder before production; two
//      fix-and-rerun iterations budgeted, each recorded.
//   4. CALIBRATION (PHASE453_MODE=calibrate, allowed dev env use): synthetic
//      control ensembles from the fitted EVEN CEP of the committed phase450
//      record, matched per-window tauInt and the exact production window
//      layout; calibrate BOTH the max-over-bins agreement statistic and the
//      3 sigma / 5 sigma decision rules; the empirical 99th-percentile
//      thresholds are BAKED into the source as committed constants below
//      (a max over ~20 correlated bins false-flags at the few-percent level
//      even with honest errors).
//   5. Pre-register the S(k) structure-factor measurement hooks on the
//      unconstrained / released columns (Team B rank-2 Arm C needs them EX
//      ANTE -- slice-summed summaries cannot be retro-extracted); emit them in
//      the output schema.
//
// PRODUCTION (PHASE453_MODE=production; runs LATER, env-clean, NOT in this
// checkpoint): fresh identity-control + sd2 runs at n=3 under all four phase450
// binding conditions with the corrected ladder; two analysis arms:
//   Arm A: moving-block bootstrap of trajectories within each window, block
//     length L_w = ceil(2 tauIntPhi_w) from that window's FRESH chain, with a
//     FULL WHAM re-solve (f_i to the committed residual tolerance) on every one
//     of 400 pre-registered replicates -- the only construction that propagates
//     the f_i stitching-constant covariance into the antisymmetry errors.
//   Arm B: within-window antisymmetrized U_odd estimator; bins enter only with
//     minimum EFFECTIVE counts per sign bin (raw counts scaled by
//     1/(2 tauIntPhi_w), both bins N_eff >= 25).
//
// PRE-REGISTERED VERDICT TAXONOMY (three terminals, fail-closed; baked below):
//   T1 symmetric-phase-null: both arms green (all per-bin antisymmetry within
//     the calibrated thresholds) AND single-well-at-zero reproduced on both
//     members AND fresh tadpole zero within errors AND identity control clean.
//   T2 Z2-breaking-candidate: antisymmetry survives BOTH corrected arms above
//     the calibrated thresholds AND the fresh independent tadpole moves off
//     zero at >= 5 sigma with coherent sign on the same member.
//   T3 estimator-discordance-or-new-defect: arms disagree, or gates fail in a
//     new pattern -> no physics claim; the discordance pattern is committed as
//     the diagnostic.
//   Any phase450 hard-battery failure (axis-angle chart, su(2) projection,
//   <e^-dH>, virial, acceptance windows) => inconclusive-gates-failed
//   regardless of the arms.
//
// MANDATORY FRAMING (identical to phase450). Workbench-relative candidate data
// ONLY (su(2) toy algebra on the reduced Spin(4) slice, lattice units,
// lattice-canonical tori); beta, the umbrella springs, the Euclidean
// (non-mass-weighted) Phi inner product, the flat HMC kinetic mass, and the
// theta-Haar chart are WORKBENCH CONVENTIONS pending physicist review (Wave-0
// item 0.3 OPEN -> physicistReviewPending = true carried explicitly); NO
// GeV/pole/VEV promotion either way; no Phase201/Phase256 contract field is
// filled; nothing is promoted. The phase PASSES on internal consistency
// (pre-registration completeness + hard batteries) regardless of the eventual
// production verdict.

const string DefaultOutputDir = "studies/phase453_wham_parity_error_model_repair_001/output";
const string Phase448SummaryPath = "studies/phase448_torus_mode_volume_saturation_probe_001/output/torus_mode_volume_saturation_probe_summary.json";
const string Phase449SummaryPath = "studies/phase449_variational_gaussian_effective_potential_probe_001/output/variational_gaussian_effective_potential_probe_summary.json";
const string Phase450SummaryPath = "studies/phase450_constraint_effective_potential_hmc_probe_001/output/constraint_effective_potential_hmc_probe_summary.json";
const string DesignSourcePath = "docs/Phases/FOUR_D_PLATFORM_DESIGN.md";
const string PhysicsDecisionsSourcePath = "docs/Phases/FOUR_D_PLATFORM_PHYSICS_DECISIONS.md";
const string ProgramSourcePath = "docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md";
const string ApplicationSubjectKind = "wham-parity-error-model-repair";
const string TerminalPrefix = "wham-parity-error-model-repair-";

// --- Mode (committed default = preregister; env override is dev-only and is
// flagged by the incremental env-knob fingerprint, which is correct). ---------
// preregister : Stage-0 commit -- recompute the phase450 localization, emit the
//               ladder plan, run the hard batteries, emit the baked smoke +
//               calibration provenance, the S(k) schema, and the taxonomy. NO
//               production trajectory. This is the committed env-clean run.
// calibrate   : synthetic-ensemble calibration of the antisymmetry-statistic
//               thresholds (dev; the 99th-percentile numbers are baked below).
// smoke       : short-chain junction neighbor-overlap smoke of the corrected
//               ladder + a structural exercise of Arms A/B (dev).
// production  : the full fresh-ensemble run with both arms (runs LATER, by the
//               team; enabled by flipping DefaultMode -- env-clean, per the
//               phase452 reconciliation lesson).
const string DefaultMode = "preregister";
string Mode = (Environment.GetEnvironmentVariable("PHASE453_MODE") ?? DefaultMode).Trim().ToLowerInvariant();
if (Mode is not ("preregister" or "calibrate" or "smoke" or "production"))
    throw new InvalidOperationException($"PHASE453_MODE '{Mode}' invalid (preregister|calibrate|smoke|production).");

// --- Probe configuration (committed defaults; PHASE453_* env overrides). ------
int[] TorusSizes = (Environment.GetEnvironmentVariable("PHASE453_TORI") ?? "3")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    .Select(int.Parse).ToArray();
string[] MemberTags = (Environment.GetEnvironmentVariable("PHASE453_MEMBERS") ?? "identity,sd2")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToArray();
double TMax = EnvD("PHASE453_TMAX", 2.5);
int ChunkTraj = EnvI("PHASE453_TRAJ", 500);            // adaptive sampling chunk size
int MaxTraj = EnvI("PHASE453_MAXTRAJ", 9000);          // fail-closed trajectory cap per window
int WarmTraj = EnvI("PHASE453_WARM", 300);
int NeffTarget = EnvI("PHASE453_NEFF", 100);           // binding condition (iii)
double Beta = EnvD("PHASE453_BETA", 1.0);              // recorded convention (default 1)
double BetaLarge = EnvD("PHASE453_BETA_LARGE", 8.0);   // large-beta tree-shape control column
double KUmbEnv = EnvD("PHASE453_KUMB", 0.0);           // 0 = auto from window spacing + tree curvature
int NLeap = EnvI("PHASE453_NLEAP", 10);
int MinTraj = EnvI("PHASE453_MINTRAJ", 1500);          // statistics floor per window (WHAM bins)
int MaxParallel = EnvI("PHASE453_PARALLEL", System.Math.Min(16, Environment.ProcessorCount));
double EpsEnv = EnvD("PHASE453_EPS", 0.0);             // 0 = auto-tuned in warmup
int ThetaProps = EnvI("PHASE453_THETA_PROPS", 8);      // Haar-Metropolis proposals per trajectory
int SamplerSeed = EnvI("PHASE453_SEED", 453453);
const int RayRngSeed = 20260703;   // phase448 invariant-ray convention (per-type coefficients, seed 0)
const double BetaConvention = 1.0; // recorded convention (beta enters as e^{-beta S_B})

// Sub-ladder (Stage-0 item 3): finer windows straddling the origin. The
// phase450 sd2 ladder is spacing 0.5 / spring 16; the sub-ladder halves the
// near-origin spacing to resolve U(Phi) where the antisymmetry originates.
double SubLadderSpacing = EnvD("PHASE453_SUBSPACING", 0.25);   // |c| of the added windows
double SubLadderHalfSpan = EnvD("PHASE453_SUBSPAN", 0.25);     // add centers up to +-this
// The pre-registered starting spring from the phase450 auto-rule at the
// recorded 2.0 softening factor: k = (2.0/spacing)^2. Softening iterations
// (recorded below) multiply this by SubLadderSoftenFactor per fix-and-rerun.
const double AutoSpringSoftening = 2.0;                        // phase450 recorded factor
double SubLadderStartSpring = (AutoSpringSoftening / SubLadderSpacing) * (AutoSpringSoftening / SubLadderSpacing);

// --- Production analysis-arm configuration (pre-registered). ------------------
const int ArmABootstrapReplicates = 400;   // moving-block bootstrap replicates, full WHAM re-solve each
const double ArmBMinEffectiveSignBinCounts = 25.0; // both sign bins N_eff >= 25 to enter Arm B

// ===========================================================================
// BAKED CALIBRATION CONSTANTS (Stage-0 item 4). Produced by PHASE453_MODE=
// calibrate against the fitted EVEN CEP of the committed phase450 sd2 record
// with matched per-window tauInt and the exact production window layout; the
// empirical 99th-percentile thresholds of the max-over-bins antisymmetry
// statistic are frozen here as committed decision boundaries. A max over ~20
// correlated bins false-flags at the few-percent level even with honest errors,
// so the raw 5 sigma phase450 number must be read against THIS calibrated
// null, not the nominal Gaussian tail.
//   *CalibrationComplete gates whether the numbers below are the measured
//   ones; until a calibrate run fills them they are placeholders and the
//   pre-registration records calibrationComplete=false (fail-closed).
const bool CalibrationComplete = true;
const int CalibrationReplicates = 2000;                 // synthetic ensembles per calibrate run
const int CalibrationSyntheticTrajPerWindow = 1500;     // matched to the phase450 committed floor
// 99th-percentile of max-over-bins |U(+Phi)-U(-Phi)| / combined-error under the
// EVEN-CEP null (pure error-model noise; no true odd component) -- the
// calibrated decision boundary in units of the arm's own combined error.
// BAKED from PHASE453_MODE=calibrate: 2000 synthetic ensembles from the fitted
// even CEP with matched per-window tauInt (N_eff = N/(2 tau) deflation) and the
// exact production window layout (mean max 1.78, absolute max 4.22).
const double CalibratedMaxAntisymSigma99 = 3.1597;      // BAKED (calibrate run 2026-07-10)
// 99th-percentile of the max absolute antisymmetry |U(+Phi)-U(-Phi)| (lattice
// action units) under the same null -- recorded alongside for the abs channel.
const double CalibratedMaxAntisymAbs99 = 0.7773;        // BAKED
// Empirical false-flag rate at the NOMINAL 5 sigma cut under the even-CEP null
// (fraction of synthetic ensembles whose max-over-bins statistic exceeds 5) --
// 0 in 2000 replicates (<0.05%): the phase450-model max-statistic almost never
// reaches 5 sigma even on a true-even CEP, so the phase450 5.06 sigma is not a
// max-statistic false flag UNDER THAT MODEL -- the corrected arms lower it by
// propagating the f_i covariance the phase450 model omits, NOT this synthetic.
const double CalibratedFalseFlagRateAt5Sigma = 0.0000;  // BAKED (0/2000)
// Empirical false-flag rate at the nominal 3 sigma cut (the phase450-class
// per-bin threshold) -- the few-percent number the item 4 charge anticipates.
const double CalibratedFalseFlagRateAt3Sigma = 0.0170;  // BAKED
// The DECISION RULE for the corrected arms: an arm flags antisymmetry iff its
// max-over-bins statistic exceeds CalibratedMaxAntisymSigma99 (a calibrated 1%
// false-positive boundary). T2 additionally requires the fresh tadpole >= 5
// sigma with coherent sign; the fresh-tadpole 5 sigma is a genuine-Gaussian
// single-statistic cut and is NOT the correlated-max cut.

// ===========================================================================
// BAKED SMOKE PROVENANCE (Stage-0 item 3). Produced by PHASE453_MODE=smoke
// short-chain runs at the junction between the +-0.25 sub-ladder and the
// phase450-style +-0.5 ladder; each fix-and-rerun iteration recorded. The
// neighbor-overlap gate is >= 0.15 (phase450 OverlapMin). Two iterations were
// budgeted; the recorded iteration log and the final passing spring are frozen
// here and re-emitted in the pre-registration output.
const bool SmokeComplete = true;
const int SmokeBudgetedIterations = 2;
const int SmokeIterationsUsed = 0;                      // BAKED: the pre-registered auto-rule spring passed; no fix-and-rerun needed
const double SmokeFinalSubLadderSpring = 64.0;          // BAKED: the pre-registered auto-rule spring (2.0/0.25)^2
const double SmokeJunctionMinOverlap = 0.4250;          // BAKED min neighbor overlap at the junction (sd2, spring 64)
const int SmokeTrajPerWindow = 400;                     // BAKED smoke chain length (per window, warmup 80)
// Human-readable iteration log (frozen). Iteration 0 is the pre-registered
// auto-rule spring; each row records (spring, junction min overlap, pass).
string[] SmokeIterationLog = new[]
{
    "iter0 (pre-registered auto-rule spring (2.0/0.25)^2 = 64.0): sd2 junction min overlap 0.4250 vs 0.15 gate: PASS (windows at +-0.25 straddling the origin at spacing 0.25 overlap the 0 and +-0.5 neighbors comfortably) -- NO softening needed, 0/2 budgeted fix-and-reruns used",
    "iter1 (informational, softened spring 16.0 matching the phase450 main-ladder k): sd2 junction min overlap 0.5875 vs 0.15 gate: PASS (wider still) -- recorded as the softer alternative; the committed production spring stays at the auto-rule 64.0",
};

// Hard-battery tolerances (identical to phase450).
const double CovarianceTol = 1e-9;
const double ObjectiveConsistencyTol = 1e-9;
const double GradFdTol = 1e-4;
const double ThetaChartTol = 1e-9;
const double IdentityThetaGradTol = 1e-12;
const double ProjectorOverlapTol = 1e-12;
const double WhamPlumbingTol = 0.08;
const double WhamResidualTol = 1e-8;
const int WhamMaxIter = 200000;

// Sampling-gate tolerances (identical to phase450; failure => inconclusive).
const double OverlapMin = 0.15;
const double ExpDhAbsFloor = 0.03;
const double VirialRelFloor = 0.03;
const double CoverageHalfWidthFraction = 0.5;

// Classification conventions (pre-registered; identical to phase450).
const int MinBinCounts = 30;
const double SsbDepthSigma = 3.0;
const double FlatBottomMinHalfWidth = 1.0;
const double MonotoneSlackSigma = 2.0;

var outputDir = Environment.GetEnvironmentVariable("PHASE453_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ---------------------------------------------------------------------------
// Precursors: phase448 + phase449 (as phase450) AND phase450 itself (this phase
// repairs phase450's one failing gate, so phase450 must be a materialized,
// internally-consistent record).
// ---------------------------------------------------------------------------

using var phase448 = JsonDocument.Parse(File.ReadAllText(Phase448SummaryPath));
bool phase448PrecursorPassed = JsonBool(phase448.RootElement, "torusModeVolumeSaturationProbePassed") is true;
using var phase449 = JsonDocument.Parse(File.ReadAllText(Phase449SummaryPath));
bool phase449PrecursorPassed = JsonBool(phase449.RootElement, "variationalGaussianEffectivePotentialProbePassed") is true;
using var phase450 = JsonDocument.Parse(File.ReadAllText(Phase450SummaryPath));
bool phase450PrecursorPassed =
    JsonBool(phase450.RootElement, "constraintEffectivePotentialHmcProbePassed") is true
    && JsonString(phase450.RootElement, "verdictKind") == "inconclusive-gates-failed"
    && JsonBool(phase450.RootElement, "onlyAntisymmetryGatesFailed") is true;
bool precursorsPassed = phase448PrecursorPassed && phase449PrecursorPassed && phase450PrecursorPassed;

// ---------------------------------------------------------------------------
// STAGE-0 ITEM 1: recompute the per-bin antisymmetry localization of the
// committed phase450 sd2 discrepancy from its cepCurve (only the max fields are
// committed there). Recorded as committed provenance for the motivating
// discrepancy. Also fits the EVEN CEP (used by calibration and recorded).
// ---------------------------------------------------------------------------

var localization = RecomputePhase450Localization(phase450.RootElement, out var evenCepFit, out var phase450SdWindowTau);

var stopwatch = Stopwatch.StartNew();
long sbGradEvals = 0, sbObjEvals = 0;

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
var manifest = BuildManifest();
var geometry = BuildGeometry();
var rootRng = new Random(SamplerSeed);

// ---------------------------------------------------------------------------
// WHAM (log-space; shared by the plumbing battery, the calibration synthetic
// ensembles, and the production arms). Bias b_w(x) = (k/2)(x-c)^2 (NOT * beta).
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
        long m = 0;
        for (int s = 0; s < 60000; s++)
        {
            double x = mu + System.Math.Sqrt(var_) * Gauss(rng);
            int b = (int)System.Math.Round(x / bw) + nb / 2;
            if (b >= 0 && b < nb) { counts[b]++; m++; }
        }
        wins[w] = (counts, m, centers[w], kSyn);
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
Console.WriteLine($"# mode={Mode} wham plumbing: maxErr={whamPlumbingMaxErr:E2} (tol {WhamPlumbingTol}) pass={whamPlumbingBattery}");

// ===========================================================================
// CALIBRATION (Stage-0 item 4). Synthetic control ensembles from the fitted
// EVEN CEP with matched per-window tauInt and the exact production window
// layout. Returns the empirical distribution of the max-over-bins antisymmetry
// statistic under a TRUE-EVEN null; its 99th percentile is the calibrated
// decision boundary. Run with PHASE453_MODE=calibrate; the numbers are baked
// into the constants above.
// ---------------------------------------------------------------------------
CalibrationResult RunCalibration(WindowPlan plan, EvenCepFit even, double[] windowTau, double beta)
{
    var rng = new Random(SamplerSeed + 7777);
    int nb = plan.BinPhi.Length;
    var maxSigmas = new List<double>();
    var maxAbss = new List<double>();
    double tauMean = windowTau.DefaultIfEmpty(1.0).Average();
    for (int rep = 0; rep < CalibrationReplicates; rep++)
    {
        // Synthesize per-window histograms: within window w the biased density is
        // proportional to exp(-beta U_even(Phi) - (k/2)(Phi-c)^2). Draw
        // CalibrationSyntheticTrajPerWindow correlated samples (thinned by the
        // window's tauInt so the EFFECTIVE count matches the committed run).
        var wins = new (long[] Counts, long N, double Center, double K)[plan.Centers.Length];
        for (int w = 0; w < plan.Centers.Length; w++)
        {
            double c = plan.Centers[w], k = plan.Springs[w];
            double tau = w < windowTau.Length ? System.Math.Max(windowTau[w], 0.5) : tauMean;
            // Matched per-window tauInt: only N_eff = N/(2 tau) INDEPENDENT draws
            // carry information, so the histogram fluctuation must reflect N_eff
            // (an autocorrelated chain of length N has the noise of N_eff draws).
            // Draw N_eff samples from the biased even-CEP density and scale the
            // counts back to the nominal N so WHAM weighting is unchanged -- the
            // standard emulation of a correlated umbrella chain.
            int nEff = System.Math.Max(8, (int)System.Math.Round(CalibrationSyntheticTrajPerWindow / (2.0 * tau)));
            double scale = CalibrationSyntheticTrajPerWindow / (double)nEff;
            var counts = new long[nb];
            long n = 0;
            double mode = c; // the spring dominates the local shape
            double width = 1.0 / System.Math.Sqrt(System.Math.Max(k + beta * even.LocalCurvature(c), 1e-6));
            for (int s = 0; s < nEff; s++)
            {
                double x = mode + width * Gauss(rng);
                double logTarget = -beta * even.U(x) - 0.5 * k * (x - c) * (x - c);
                double logProp = -0.5 * (x - mode) * (x - mode) / (width * width);
                if (System.Math.Log(1.0 - rng.NextDouble()) < logTarget - logProp - (-beta * even.U(mode)))
                {
                    int b = (int)System.Math.Round(x / plan.BinW) + nb / 2;
                    if (b >= 0 && b < nb) { counts[b] += (long)System.Math.Round(scale); n += (long)System.Math.Round(scale); }
                }
            }
            wins[w] = (counts, System.Math.Max(n, 1), c, k);
        }
        var (lnP, _, resid, _) = WhamSolve(wins, plan.BinPhi);
        if (!double.IsFinite(resid) || resid > WhamResidualTol) continue;
        var (mSig, mAbs) = MaxAntisymmetryFromLnP(lnP, plan, wins, tauMean, beta);
        if (double.IsFinite(mSig)) maxSigmas.Add(mSig);
        if (double.IsFinite(mAbs)) maxAbss.Add(mAbs);
    }
    maxSigmas.Sort();
    maxAbss.Sort();
    double Pct(List<double> xs, double p) =>
        xs.Count == 0 ? double.NaN : xs[System.Math.Min(xs.Count - 1, (int)System.Math.Ceiling(p * xs.Count) - 1)];
    double sig99 = Pct(maxSigmas, 0.99), abs99 = Pct(maxAbss, 0.99);
    double ff5 = maxSigmas.Count == 0 ? double.NaN : maxSigmas.Count(x => x > 5.0) / (double)maxSigmas.Count;
    double ff3 = maxSigmas.Count == 0 ? double.NaN : maxSigmas.Count(x => x > 3.0) / (double)maxSigmas.Count;
    return new CalibrationResult(maxSigmas.Count, sig99, abs99, ff5, ff3,
        maxSigmas.Count == 0 ? double.NaN : maxSigmas.Average(),
        maxSigmas.Count == 0 ? double.NaN : maxSigmas[^1]);
}

// Max-over-bins antisymmetry of a WHAM lnP under the phase450 per-bin error
// model (used by calibration; the production arms use their own covariance
// propagation). Returns (maxSigma, maxAbs).
(double MaxSigma, double MaxAbs) MaxAntisymmetryFromLnP(
    double[] lnP, WindowPlan plan, (long[] Counts, long N, double Center, double K)[] wins, double tauMean, double beta)
{
    int nb = plan.BinPhi.Length;
    var u = new double[nb];
    var err = new double[nb];
    var have = new bool[nb];
    // Stitch-variance accumulation (phase450 model) from the central window out.
    int wCenter = 0;
    for (int w = 1; w < wins.Length; w++)
        if (System.Math.Abs(wins[w].Center) < System.Math.Abs(wins[wCenter].Center)) wCenter = w;
    var pairVar = new double[System.Math.Max(wins.Length - 1, 0)];
    for (int w = 0; w + 1 < wins.Length; w++)
    {
        long novl = 0;
        for (int b = 0; b < nb; b++) novl += System.Math.Min(wins[w].Counts[b], wins[w + 1].Counts[b]);
        double neffOvl = System.Math.Max(novl / (2.0 * tauMean), 1.0);
        pairVar[w] = 1.0 / (beta * beta * neffOvl);
    }
    double StitchVar(double phi)
    {
        int wb = 0;
        for (int w = 1; w < wins.Length; w++)
            if (System.Math.Abs(wins[w].Center - phi) < System.Math.Abs(wins[wb].Center - phi)) wb = w;
        double s = 0.0;
        for (int w = System.Math.Min(wb, wCenter); w < System.Math.Max(wb, wCenter); w++) s += pairVar[w];
        return s;
    }
    for (int b = 0; b < nb; b++)
    {
        long tot = 0; foreach (var w in wins) tot += w.Counts[b];
        if (tot < MinBinCounts || double.IsNegativeInfinity(lnP[b])) continue;
        u[b] = -lnP[b] / beta;
        double errCount = 1.0 / (beta * System.Math.Sqrt(System.Math.Max(tot / (2.0 * tauMean), 1.0)));
        err[b] = System.Math.Sqrt(errCount * errCount + StitchVar(plan.BinPhi[b]));
        have[b] = true;
    }
    double maxSig = 0, maxAbs = 0;
    int nbHalf = nb / 2;
    for (int b = nbHalf + 1; b < nb; b++)
    {
        int mirror = 2 * nbHalf - b;
        if (mirror < 0 || !have[b] || !have[mirror]) continue;
        double d = System.Math.Abs(u[b] - u[mirror]);
        double se = System.Math.Sqrt(err[b] * err[b] + err[mirror] * err[mirror]);
        maxAbs = System.Math.Max(maxAbs, d);
        maxSig = System.Math.Max(maxSig, d / System.Math.Max(se, 1e-12));
    }
    return (maxSig, maxAbs);
}

// ---------------------------------------------------------------------------
// Torus arms (the shared harness -- verbatim phase450 machinery, extended with
// per-window signed-bin counts and the retained phi series the production arms
// consume). Only preregister/smoke/production actually build the operators.
// ---------------------------------------------------------------------------

var torusRecords = new List<TorusArm>();
bool allHardBatteriesPassed = whamPlumbingBattery;
CalibrationResult? calibration = null;
SmokeResult? smokeResult = null;
var productionArms = new List<ProductionMemberArm>();
WindowPlan? recordedSd2Plan = null;

// The corrected production ladder for sd2 (analytic; no operators needed):
// the phase450 committed sd2 ladder (centers -2.5..2.5 step 0.5, spring 16)
// PLUS the Stage-0 item-3 sub-ladder (+-0.25) at the smoke-confirmed spring.
// The identity control already carries spacing 0.25 in phase450 so it needs no
// sub-ladder; sd2 (spacing 0.5) is the member the sub-ladder densifies.
WindowPlan BuildSd2ProductionLadder(double subLadderSpring)
{
    double mainSpacing = 0.5, mainSpring = 16.0;
    var centers = new List<(double C, double K)>();
    for (int i = -5; i <= 5; i++) centers.Add((i * mainSpacing, mainSpring)); // -2.5..2.5
    for (double s = SubLadderSpacing; s <= SubLadderHalfSpan + 1e-9; s += SubLadderSpacing)
    {
        centers.Add((+s, subLadderSpring));
        centers.Add((-s, subLadderSpring));
    }
    var ordered = centers.OrderBy(x => x.C).ToList();
    var cs = ordered.Select(x => x.C).ToArray();
    var ks = ordered.Select(x => x.K).ToArray();
    double bw = mainSpacing / 4.0; // 0.125, the phase450 sd2 bin width
    int nbHalf = (int)System.Math.Ceiling((TMax + 3.0 * mainSpacing) / bw);
    int nb = 2 * nbHalf + 1;
    var binPhi = new double[nb];
    for (int b = 0; b < nb; b++) binPhi[b] = (b - nbHalf) * bw;
    return new WindowPlan(cs, ks, binPhi, bw, mainSpacing);
}

// Dev-only sub-ladder spring override for the smoke fix-and-rerun iterations
// (env-flagged by the incremental fingerprint; 0 = use the committed spring).
double SubLadderSpringOverride = EnvD("PHASE453_SUBSPRING", 0.0);
double subLadderSpringUsed = SubLadderSpringOverride > 0.0 ? SubLadderSpringOverride
    : (SmokeComplete ? SmokeFinalSubLadderSpring : SubLadderStartSpring);
recordedSd2Plan = BuildSd2ProductionLadder(subLadderSpringUsed);

// The identity control keeps its OWN phase450 ladder (spacing 0.25, spring 64,
// 21 windows): it is ~12x stiffer than sd2 and already straddles the origin at
// +-0.25, so it needs no sub-ladder. Building it analytically avoids re-running
// the per-member tree-curvature planner.
WindowPlan BuildIdentityProductionLadder()
{
    double spacing = 0.25, spring = 64.0;
    int half = (int)System.Math.Round(TMax / spacing); // 10 -> centers -2.5..2.5
    var cs = new double[2 * half + 1];
    var ks = new double[2 * half + 1];
    for (int w = 0; w < cs.Length; w++) { cs[w] = -TMax + spacing * w; ks[w] = spring; }
    double bw = spacing / 4.0; // 0.0625
    int nbHalf = (int)System.Math.Ceiling((TMax + 3.0 * spacing) / bw);
    int nb = 2 * nbHalf + 1;
    var binPhi = new double[nb];
    for (int b = 0; b < nb; b++) binPhi[b] = (b - nbHalf) * bw;
    return new WindowPlan(cs, ks, binPhi, bw, spacing);
}
var recordedIdentityPlan = BuildIdentityProductionLadder();

// Align the phase450 sd2 per-window tauInt to the production ladder centers
// (nearest committed window; new +-0.25 windows inherit the nearest tau).
double[] AlignedTau(WindowPlan plan) => plan.Centers.Select(c =>
{
    double best = double.PositiveInfinity, tau = 1.0;
    foreach (var (wc, wt) in phase450SdWindowTau)
        if (System.Math.Abs(wc - c) < best) { best = System.Math.Abs(wc - c); tau = wt; }
    return tau;
}).ToArray();

// --- CALIBRATE short-circuit: synthetic-only, no operators. -----------------
if (Mode == "calibrate")
{
    calibration = RunCalibration(recordedSd2Plan, evenCepFit, AlignedTau(recordedSd2Plan), Beta);
    Console.WriteLine($"# CALIBRATION ({calibration.Replicates} ensembles): "
        + $"maxAntisymSigma99={calibration.MaxAntisymSigma99:F4} maxAntisymAbs99={calibration.MaxAntisymAbs99:F4} "
        + $"falseFlag@5s={calibration.FalseFlagAt5Sigma:F4} falseFlag@3s={calibration.FalseFlagAt3Sigma:F4} "
        + $"mean={calibration.MeanMaxSigma:F3} max={calibration.MaxMaxSigma:F3}");
    Console.WriteLine("# BAKE these into CalibratedMaxAntisymSigma99 / CalibratedMaxAntisymAbs99 / "
        + "CalibratedFalseFlagRateAt5Sigma / CalibratedFalseFlagRateAt3Sigma.");
}

bool needHarness = Mode is "preregister" or "smoke" or "production";
if (needHarness)
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

    // --- Orbit coordinates (verbatim phase448/phase450). --------------------
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
    var rayCoeffRng = new Random(RayRngSeed + 31 * 0);
    var typeCoeffs = new double[16 * dimG];
    for (int i = 0; i < typeCoeffs.Length; i++) typeCoeffs[i] = rayCoeffRng.NextDouble() - 0.5;
    var uInv = new double[nOmega];
    for (int e = 0; e < mesh.EdgeCount; e++)
        for (int l = 0; l < dimG; l++)
            uInv[e * dimG + l] = oSign[e] * typeCoeffs[edgeType[e] * dimG + l];
    double uNormRaw = Norm(uInv);
    for (int i = 0; i < nOmega; i++) uInv[i] /= uNormRaw;

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

    // --- Members. -----------------------------------------------------------
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
        Interlocked.Increment(ref sbObjEvals);
        return obj;
    }
    double SBObj(EinsteinianShiabOperator op, double[] omega, double[] theta) => SBObjM(op, mass, omega, theta);
    (FieldTensor F, FieldTensor Conn) CacheTensors(double[] omega)
    {
        var conn = new ConnectionField(mesh, algebra, omega);
        return (CurvatureAssembler.Assemble(conn).ToFieldTensor(), conn.ToFieldTensor());
    }
    double MassIp(double[] coeffs)
    {
        var s = FaceTensor(coeffs);
        return mass.InnerProduct(s, s);
    }
    double SBThetaFast(EinsteinianShiabOperator op, CpuMassMatrix mm, FieldTensor f, FieldTensor conn, double[] theta)
    {
        var s = FaceTensor(op.EvaluateWithTheta(f, conn, theta, manifest, geometry).Coefficients);
        Interlocked.Increment(ref sbObjEvals);
        return 0.5 * mm.InnerProduct(s, s);
    }

    // --- Hard batteries on this torus (verbatim phase450). ------------------
    var batRng = new Random(SamplerSeed + 977 * n);
    double covarianceMax = 0.0, objConsistencyMax = 0.0, gradFdMax = 0.0;
    double thetaUniformShiftMax = 0.0, thetaWrapChartMax = 0.0, thetaPeriodicityMax = 0.0;
    double identityThetaGradNorm = 0.0;
    foreach (var (name, spec, op, isControl) in members)
    {
        var xw = RandomVector(batRng, nOmega, 0.4);
        var xt = RandomVector(batRng, nTheta, 0.4);
        double s0 = SBObj(op, xw, xt);
        var (fT, cT) = CacheTensors(xw);
        double sDirect = isControl
            ? 0.5 * MassIp(op.Evaluate(fT, cT, manifest, geometry).Coefficients)
            : SBThetaFast(op, mass, fT, cT, xt);
        objConsistencyMax = System.Math.Max(objConsistencyMax, RelDiff(s0, sDirect));
        for (int rep = 0; rep < 3; rep++)
        {
            var a = new[] { batRng.Next(n), batRng.Next(n), batRng.Next(n), batRng.Next(n) };
            double sT = SBObj(op, TranslateOmega(xw, a), TranslateTheta(xt, a));
            covarianceMax = System.Math.Max(covarianceMax, RelDiff(s0, sT));
        }
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
            var cShift = RandomVector(batRng, dimG, 0.9);
            var tU = new double[nTheta];
            for (int v = 0; v < nVert; v++) for (int l = 0; l < dimG; l++) tU[v * dimG + l] = cShift[l];
            double sU = SBObj(op, xw, tU);
            double sZ = SBObj(op, xw, new double[nTheta]);
            thetaUniformShiftMax = System.Math.Max(thetaUniformShiftMax, RelDiff(sU, sZ));
            var tBig = new double[nTheta];
            var tWrap = new double[nTheta];
            for (int v = 0; v < nVert; v++)
            {
                var ax = RandomVector(batRng, dimG, 1.0);
                double an = Norm(ax);
                for (int l = 0; l < dimG; l++) ax[l] /= an;
                double angle = System.Math.PI + 0.4 + 1.2 * batRng.NextDouble();
                for (int l = 0; l < dimG; l++)
                {
                    tBig[v * dimG + l] = angle * ax[l];
                    tWrap[v * dimG + l] = -(2.0 * System.Math.PI - angle) * ax[l];
                }
            }
            thetaWrapChartMax = System.Math.Max(thetaWrapChartMax, RelDiff(SBObj(op, xw, tBig), SBObj(op, xw, tWrap)));
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

    bool covarianceBattery = orbitMapOk && covarianceMax <= CovarianceTol;
    bool objConsistencyBattery = objConsistencyMax <= ObjectiveConsistencyTol;
    bool gradFdBattery = gradFdMax <= GradFdTol;
    bool thetaChartBattery = thetaUniformShiftMax <= ThetaChartTol
        && thetaWrapChartMax <= ThetaChartTol && thetaPeriodicityMax <= ThetaChartTol;
    bool identityThetaIndependenceBattery = identityThetaGradNorm <= IdentityThetaGradTol;
    bool torusHardBatteries = precursorsPassed && projectorBattery && covarianceBattery
        && objConsistencyBattery && gradFdBattery && thetaChartBattery && identityThetaIndependenceBattery;
    allHardBatteriesPassed &= torusHardBatteries;
    Console.WriteLine($"# n={n} batteries: cov={covarianceMax:E2} obj={objConsistencyMax:E2} gradFd={gradFdMax:E2} "
        + $"chart(U/W/P)={thetaUniformShiftMax:E2}/{thetaWrapChartMax:E2}/{thetaPeriodicityMax:E2} "
        + $"idThetaGrad={identityThetaGradNorm:E2} projOverlap={projectorOverlapMaxAfter:E2} pass={torusHardBatteries}");

    // --- Tree curve along u_inv (theta = 0) + quartic fit. ------------------
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

    torusRecords.Add(new TorusArm(n, mesh.VertexCount, mesh.EdgeCount, mesh.FaceCount, mesh.CellCount,
        nOmega, nTheta, orbitMapOk, uNormRaw, uNormAfter, overlapsBefore, overlapsAfter,
        projectorOverlapMaxAfter, projectorBattery,
        covarianceMax, covarianceBattery, objConsistencyMax, objConsistencyBattery,
        gradFdMax, gradFdBattery, thetaUniformShiftMax, thetaWrapChartMax, thetaPeriodicityMax,
        thetaChartBattery, identityThetaGradNorm, identityThetaIndependenceBattery,
        torusHardBatteries, treeByMember));

    // ======================================================================
    // Production/smoke sampler. Only runs for PHASE453_MODE in {smoke,
    // production}; preregister returns after batteries+tree above. Smoke uses
    // the PHASE453_* reduced knobs (dev; env-fingerprint flags them, which is
    // correct); production is the env-clean full run (LATER, not this commit).
    // ======================================================================
    if (Mode is not ("smoke" or "production")) continue;

    // S(k) accumulator (Stage-0 item 5, Arm C hook): on the UNCONSTRAINED
    // (k=0) column only. The momenta are the (Z_n)^4 torus reciprocal lattice
    // k = 2*pi*m/n, m in {0..n-1}^4; the observable is the type-summed
    // per-algebra structure factor S(k) = < |sum_e u_type(e) e^{-i k.x_e} omega_e|^2 >
    // over the released joint ensemble. Accumulated ONLINE (configs are not
    // retained). Emitted as schema-only in preregister; filled in production.
    var kModes = new List<int[]>();
    for (int m0 = 0; m0 < n; m0++)
    for (int m1 = 0; m1 < n; m1++)
    for (int m2 = 0; m2 < n; m2++)
    for (int m3 = 0; m3 < n; m3++)
        kModes.Add(new[] { m0, m1, m2, m3 });

    (double[] SkReal, long SkSamples) AccumulateSk(double[] omega, double[] reC, double[] imC)
    {
        // reC/imC are per-mode running sums of the complex amplitude modulus^2;
        // called per retained config (the caller owns the running arrays).
        for (int km = 0; km < kModes.Count; km++)
        {
            var kv = kModes[km];
            double re = 0, im = 0;
            for (int e = 0; e < mesh.EdgeCount; e++)
            {
                var c0 = coords[edgeBase[e]];
                double phase = 2.0 * System.Math.PI * (
                    (double)kv[0] * c0[0] / n + (double)kv[1] * c0[1] / n
                    + (double)kv[2] * c0[2] / n + (double)kv[3] * c0[3] / n);
                double cph = System.Math.Cos(phase), sph = System.Math.Sin(phase);
                double amp = 0;
                for (int l = 0; l < dimG; l++) amp += uInv[e * dimG + l] * omega[e * dimG + l];
                re += amp * cph; im -= amp * sph;
            }
            reC[km] += re * re + im * im;
        }
        return (reC, 1);
    }

    double eps0 = EpsEnv > 0.0 ? EpsEnv : 0.05 / System.Math.Sqrt(System.Math.Max(Beta, 1.0));

    WindowResult RunWindow(EinsteinianShiabOperator op, CpuMassMatrix mm, bool isControl, double beta,
        double center, double kUmb, double[] binPhi, double binW, int warm, int sampleTarget,
        bool accumulateSk, double[]? skReC, Random rng)
    {
        var swWin = Stopwatch.StartNew();
        var omega = ScaleVec(uInv, center);
        for (int i = 0; i < nOmega; i++) omega[i] += 0.01 * Gauss(rng);
        var quat = new double[nVert][];
        for (int v = 0; v < nVert; v++) quat[v] = new[] { 1.0, 0.0, 0.0, 0.0 };
        var theta = new double[nTheta];
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

        double eps = eps0;
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
            double expNegDh = double.IsFinite(dh) ? System.Math.Exp(System.Math.Min(-dh, 50.0)) : 0.0;
            sumExpNegDh += expNegDh; sumExpNegDh2 += expNegDh * expNegDh; nTraj++;
            bool acc = double.IsFinite(dh) && (dh <= 0 || rng.NextDouble() < System.Math.Exp(-dh));
            if (acc) { nAcc++; curU = newU; curS = newS; curPhi = newPhi; Array.Copy(force, curForce, nOmega); }
            else { Array.Copy(w0, omega, nOmega); curU = PotAndForce(omega, theta, curForce, out curS, out curPhi); }
            return acc;
        }

        double thetaStep = 0.4;
        long thGlobProp = 0, thGlobAcc = 0, thSingProp = 0, thSingAcc = 0;
        var thetaCand = new double[nTheta];
        void ThetaSweep()
        {
            var (fT, cT) = CacheTensors(omega);
            double sCur = SBThetaFast(op, mm, fT, cT, theta);
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
                else { qCand = (double[][])quat.Clone(); qCand[rng.Next(nVert)] = QUniform(rng); }
                for (int v = 0; v < nVert; v++)
                {
                    var aa = QToAxisAngle(qCand[v]);
                    for (int l = 0; l < dimG; l++) thetaCand[v * dimG + l] = aa[l];
                }
                double sNew = SBThetaFast(op, mm, fT, cT, thetaCand);
                double dS = beta * (sNew - sCur);
                bool acc = double.IsFinite(dS) && (dS <= 0 || rng.NextDouble() < System.Math.Exp(-dS));
                if (global) { thGlobProp++; if (acc) thGlobAcc++; } else { thSingProp++; if (acc) thSingAcc++; }
                if (acc) { quat = qCand; Array.Copy(thetaCand, theta, nTheta); sCur = sNew; }
            }
            curU = PotAndForce(omega, theta, curForce, out curS, out curPhi);
        }

        int batch = 40; long bAcc = 0, bCnt = 0;
        for (int it = 0; it < warm; it++)
        {
            if (Trajectory()) bAcc++;
            bCnt++;
            if (!isControl) ThetaSweep();
            if (bCnt == batch)
            {
                double r = (double)bAcc / bCnt;
                if (EpsEnv <= 0.0) { if (r < 0.90) eps *= 0.65; else if (r > 0.98) eps *= 1.15; }
                if (!isControl && thGlobProp > 20)
                {
                    double rg = (double)thGlobAcc / System.Math.Max(1, thGlobProp);
                    if (rg < 0.25) thetaStep *= 0.7; else if (rg > 0.6) thetaStep *= 1.25;
                    thGlobProp = 0; thGlobAcc = 0;
                }
                bAcc = 0; bCnt = 0;
            }
        }

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
        long skSamples = 0;
        while (phiSeries.Count < MaxTraj)
        {
            int target = System.Math.Min(phiSeries.Count + ChunkTraj, sampleTarget);
            while (phiSeries.Count < target)
            {
                Trajectory();
                if (!isControl) ThetaSweep();
                phiSeries.Add(curPhi);
                sSeries.Add(curS);
                double vir = 0;
                for (int i = 0; i < nOmega; i++) vir -= omega[i] * curForce[i];
                virSeries.Add(vir);
                double o2 = 0; for (int i = 0; i < nOmega; i++) o2 += omega[i] * omega[i];
                omega2Series.Add(o2);
                int b = (int)System.Math.Round(curPhi / binW) + nbHalf;
                if (b >= 0 && b < counts.Length) counts[b]++;
                if (accumulateSk && skReC != null) { AccumulateSk(omega, skReC, skReC); skSamples++; }
            }
            tauPhi = TauInt(phiSeries);
            tauS = TauInt(sSeries);
            neffPhi = phiSeries.Count / (2.0 * System.Math.Max(tauPhi, 0.5));
            neffS = sSeries.Count / (2.0 * System.Math.Max(tauS, 0.5));
            if (phiSeries.Count >= sampleTarget) break;
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
        double rms1 = System.Math.Sqrt(omega2Series.Take(nS / 2).DefaultIfEmpty(0).Average());
        double rms2 = System.Math.Sqrt(omega2Series.Skip(nS / 2).DefaultIfEmpty(0).Average());
        double driftRel = System.Math.Abs(rms2 - rms1) / System.Math.Max(rms1, 1e-12);
        bool expDhGate = System.Math.Abs(expDh - 1.0) <= System.Math.Max(ExpDhAbsFloor, 4.0 * expDhSe);
        bool virialGate = System.Math.Abs(virialRel) <= System.Math.Max(VirialRelFloor, 4.0 * virSe / nOmega);
        bool ergodicityGate = neffPhi >= NeffTarget && neffS >= NeffTarget;

        return new WindowResult(center, kUmb, nS, eps, accRate,
            expDh, expDhSe, expDhGate, meanVir, virialRel, virSe, virialGate,
            tauPhi, tauS, neffPhi, neffS, ergodicityGate, meanPhi, sdPhi, meanS, driftRel,
            thGlobProp > 0 ? (double)thGlobAcc / thGlobProp : double.NaN,
            thSingProp > 0 ? (double)thSingAcc / thSingProp : double.NaN,
            thetaStep, counts, swWin.Elapsed.TotalMilliseconds / System.Math.Max(1, nS),
            phiSeries.ToArray(), skSamples);
    }

    // Per-member production/smoke arm: run the corrected ladder + the k=0
    // tadpole/S(k) column, then compute both antisymmetry arms and classify.
    ProductionMemberArm RunProductionArm(string name, EinsteinianShiabFamilyMember spec, bool isControl,
        double beta, WindowPlan plan, int warm, int sampleTarget)
    {
        int nw = plan.Centers.Length;
        var seeds = new int[nw + 1];
        for (int w = 0; w < nw + 1; w++) seeds[w] = rootRng.Next();
        var windowsArr = new WindowResult[nw + 1];
        var skReC = new double[kModes.Count];
        var consoleLock = new object();
        Parallel.For(0, nw + 1, new ParallelOptions { MaxDegreeOfParallelism = MaxParallel }, w =>
        {
            var opW = new EinsteinianShiabOperator(mesh, algebra, spec, latticePeriod: n);
            var mmW = new CpuMassMatrix(mesh, algebra);
            bool diag = w == nw;
            var res = RunWindow(opW, mmW, isControl, beta,
                diag ? 0.0 : plan.Centers[w], diag ? 0.0 : plan.Springs[w],
                plan.BinPhi, plan.BinW, warm, sampleTarget,
                diag, diag ? skReC : null, new Random(seeds[w]));
            windowsArr[w] = res;
            lock (consoleLock)
                Console.WriteLine($"#   n={n} {name} beta={beta:0.###} {(diag ? "TAD" : "win")} c={res.Center,7:F3} "
                    + $"k={res.KUmb,7:F2} traj={res.Trajectories} acc={res.AcceptRate:F3} neffPhi={res.NeffPhi:F0} "
                    + $"<Phi>={res.MeanPhi:F3} ms/traj={res.MsPerTraj:F1}");
        });
        var tadpole = windowsArr[nw];
        double tadpoleSe = tadpole.SdPhi / System.Math.Sqrt(System.Math.Max(tadpole.NeffPhi, 1.0));
        double tadpoleSigma = System.Math.Abs(tadpole.MeanPhi) / System.Math.Max(tadpoleSe, 1e-12);
        double tadpoleSign = System.Math.Sign(tadpole.MeanPhi);
        var windows = windowsArr.Take(nw).ToList();

        // Neighbor overlaps (junction gate) + minimum.
        var overlaps = new List<(double CLo, double CHi, double Overlap)>();
        double overlapMinObs = double.PositiveInfinity;
        for (int w = 0; w + 1 < windows.Count; w++)
        {
            double ov = 0;
            for (int b = 0; b < plan.BinPhi.Length; b++)
                ov += System.Math.Min(
                    windows[w].Counts[b] / (double)System.Math.Max(1, windows[w].Trajectories),
                    windows[w + 1].Counts[b] / (double)System.Math.Max(1, windows[w + 1].Trajectories));
            overlaps.Add((windows[w].Center, windows[w + 1].Center, ov));
            overlapMinObs = System.Math.Min(overlapMinObs, ov);
        }
        bool overlapGate = overlapMinObs >= OverlapMin;

        // Baseline WHAM (phase450 model) -- classification + baseline antisym.
        var whamWins = windows.Select(w => (w.Counts, (long)w.Trajectories, w.Center, w.KUmb)).ToArray();
        var (lnP, _, whamResid, whamIters) = WhamSolve(whamWins, plan.BinPhi);
        bool whamGate = whamResid <= WhamResidualTol;
        double tauMean = windows.Average(w => System.Math.Max(w.TauPhi, 0.5));
        var (baseMaxSig, baseMaxAbs) = MaxAntisymmetryFromLnP(lnP, plan, whamWins, tauMean, beta);
        var bins = BuildCepBins(lnP, plan, whamWins, tauMean, beta, out double sigmaRough);
        var perBinAntisym = PerBinAntisymmetry(bins, plan.BinW);
        var (cls, tStar, depth, depthSigma, flatHalfWidth) = ClassifyCep(bins, plan.BinW);
        double coverageHalfWidth = bins.Count > 0 ? System.Math.Min(-bins.Min(x => x.Phi), bins.Max(x => x.Phi)) : 0.0;
        bool coverageGate = coverageHalfWidth >= CoverageHalfWidthFraction * TMax;

        // ARM A: moving-block bootstrap over per-window trajectories, full WHAM
        // re-solve each replicate. Block length L_w = ceil(2 tauIntPhi_w).
        var armA = ArmABootstrap(windows, plan, beta);

        // ARM B: within-window antisymmetrized U_odd estimator.
        var armB = ArmBAntisymmetrized(windows, plan, beta);

        // Per-window signed-bin counts (Stage-0 item 2).
        var signedCounts = windows.Select(w =>
        {
            long pos = 0, neg = 0, zero = 0;
            for (int b = 0; b < plan.BinPhi.Length; b++)
            {
                if (plan.BinPhi[b] > 1e-12) pos += w.Counts[b];
                else if (plan.BinPhi[b] < -1e-12) neg += w.Counts[b];
                else zero += w.Counts[b];
            }
            return new SignedBinCount(w.Center, pos, neg, zero);
        }).ToArray();

        bool windowGatesAll = windows.All(w => w.ExpDhGate && w.VirialGate && w.ErgodicityGate);
        // S(k) on the k=0 column (released ensemble).
        var sk = tadpole.SkSamples > 0
            ? kModes.Select((kv, i) => new SkPoint(kv, skReC[i] / tadpole.SkSamples)).ToArray()
            : Array.Empty<SkPoint>();

        return new ProductionMemberArm(name, isControl, beta, plan, windows, signedCounts,
            overlaps.ToArray(), overlapMinObs, overlapGate, whamResid, whamIters, whamGate,
            coverageHalfWidth, coverageGate, baseMaxAbs, baseMaxSig, bins, perBinAntisym, sigmaRough,
            tadpole, tadpoleSe, tadpoleSigma, tadpoleSign, windowGatesAll,
            cls, tStar, depth, depthSigma, flatHalfWidth, armA, armB, sk, tadpole.SkSamples);
    }

    // Run smoke (reduced) or production (full) for each member.
    int prodWarm = Mode == "smoke" ? System.Math.Min(WarmTraj, 80) : WarmTraj;
    int prodSample = Mode == "smoke" ? SmokeTrajPerWindow : MinTraj;
    foreach (var (name, spec, op, isControl) in members)
    {
        // sd2 uses the corrected main+sub-ladder; the stiffer identity control
        // uses its own phase450 spacing-0.25 ladder (the junction gate is tested
        // on sd2, the member the sub-ladder densifies).
        var plan = isControl ? recordedIdentityPlan : recordedSd2Plan!;
        productionArms.Add(RunProductionArm(name, spec, isControl, Beta, plan, prodWarm, prodSample));
    }
}

// ---------------------------------------------------------------------------
// Smoke junction-overlap gate (Stage-0 item 3). Reads the neighbor overlaps
// straddling the origin (the sub-ladder +-0.25 windows against their 0 and
// +-0.5 neighbors) from the smoke arms and reports the minimum at the junction.
// ---------------------------------------------------------------------------
if (Mode == "smoke")
{
    double junctionMin = double.PositiveInfinity;
    var junctionRows = new List<string>();
    foreach (var a in productionArms)
        foreach (var (clo, chi, ov) in a.NeighborOverlaps)
        {
            bool atJunction = (System.Math.Abs(System.Math.Abs(clo) - 0.25) < 1e-9 || System.Math.Abs(System.Math.Abs(chi) - 0.25) < 1e-9);
            if (!atJunction) continue;
            junctionMin = System.Math.Min(junctionMin, ov);
            junctionRows.Add($"{a.Name}: c[{clo:+0.00;-0.00}]<->c[{chi:+0.00;-0.00}] overlap={ov:F4}");
        }
    smokeResult = new SmokeResult(SubLadderStartSpring, SmokeFinalSubLadderSpring, junctionMin,
        junctionMin >= OverlapMin, junctionRows.ToArray());
    Console.WriteLine($"# SMOKE junction min overlap = {junctionMin:F4} (gate {OverlapMin}) pass={junctionMin >= OverlapMin}");
    foreach (var r in junctionRows) Console.WriteLine($"#   {r}");
    Console.WriteLine("# BAKE junctionMin into SmokeJunctionMinOverlap and the final passing spring into SmokeFinalSubLadderSpring.");
}

stopwatch.Stop();
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

// ---------------------------------------------------------------------------
// PRODUCTION verdict (T1/T2/T3), evaluated ONLY in production mode against the
// calibrated thresholds. Pre-register/smoke/calibrate carry verdictKind
// "pre-registration-committed" (no physics verdict is emitted before the fresh
// production run). Fail-closed: any hard-battery failure => inconclusive.
// ---------------------------------------------------------------------------
string verdictKind;
string verdictPhrase;
if (Mode != "production")
{
    verdictKind = "pre-registration-committed";
    verdictPhrase = "Stage-0 pre-registration committed: the phase450 per-bin antisymmetry localization is recorded as committed provenance; the corrected c=0/+-0.25 soft-spring sub-ladder and its smoke-confirmed junction overlap are pre-registered; the max-over-bins antisymmetry-statistic 99th-percentile thresholds are calibrated against the fitted even CEP and baked as committed constants; the S(k) structure-factor hooks and the two-arm (moving-block-bootstrap-WHAM-re-solve / within-window-antisymmetrized) analysis and the T1/T2/T3 verdict taxonomy are pre-registered. NO production trajectory is run in this checkpoint; the fresh-ensemble run executes later env-clean.";
}
else if (!allHardBatteriesPassed)
{
    verdictKind = "inconclusive-gates-failed";
    verdictPhrase = "a phase450 hard battery failed on the fresh run -> inconclusive regardless of the arms (fail-closed).";
}
else
{
    var einst = productionArms.Where(a => !a.IsControl).ToList();
    var ctrl = productionArms.Where(a => a.IsControl).ToList();
    bool windowGates = productionArms.All(a => a.WindowGatesAll && a.OverlapGate && a.WhamGate && a.CoverageGate);
    bool bothArmsGreen = einst.All(a => !a.ArmA.FlagsAntisymmetry && !a.ArmB.FlagsAntisymmetry);
    bool bothArmsFlag = einst.Any(a => a.ArmA.FlagsAntisymmetry && a.ArmB.FlagsAntisymmetry);
    bool singleWellBoth = productionArms.All(a => a.Classification is "single-well-at-zero" or "single-well-offset");
    bool controlClean = ctrl.All(a => a.Classification is "single-well-at-zero" or "single-well-offset");
    bool freshTadpoleZero = productionArms.All(a => a.TadpoleSigma < 5.0);
    bool anyTadpoleOffZero5 = einst.Any(a => a.TadpoleSigma >= 5.0);
    // arms concordant?
    bool armsConcordant = einst.All(a => a.ArmA.FlagsAntisymmetry == a.ArmB.FlagsAntisymmetry);
    if (!windowGates)
    {
        verdictKind = "inconclusive-gates-failed";
        verdictPhrase = "one or more sampling gates failed on the fresh run (per-window <e^-dH>/virial/N_eff, overlap, WHAM residual, coverage) -> inconclusive (fail-closed).";
    }
    else if (bothArmsGreen && singleWellBoth && controlClean && freshTadpoleZero)
    {
        verdictKind = "symmetric-phase-null";
        verdictPhrase = "T1: both corrected arms are within the calibrated antisymmetry thresholds, U(Phi) is single-well-at-zero on both members, the fresh independent tadpole is zero within errors, and the identity control is clean -> the symmetric-phase null IS claimed. The phase450 5.06-sigma antisymmetry was a WHAM stitching-error-model artifact; frontier statement upgrades to 'no non-perturbative scale along translation-invariant rays at n=3' (finite-volume, single-beta caveat; workbench-relative, no GeV).";
    }
    else if (bothArmsFlag && anyTadpoleOffZero5)
    {
        verdictKind = "z2-breaking-candidate";
        verdictPhrase = "T2: antisymmetry survives BOTH corrected arms above the calibrated thresholds AND the fresh independent tadpole moves off zero at >= 5 sigma with coherent sign on the same member -> a Z2-breaking CANDIDATE contradicting an exact symmetry: simultaneous program-level alarm (sampler/measure defect vs theta-sector physics); escalate to a volume scan at the located Phi* before anything else. Workbench-relative, no GeV.";
    }
    else
    {
        verdictKind = "estimator-discordance-or-new-defect";
        verdictPhrase = armsConcordant
            ? "T3: the arms are concordant but the null conjunction is incomplete (a gate or classification differs from T1 without the T2 tadpole coincidence) -> no physics claim; the pattern is committed as the diagnostic."
            : "T3: the two corrected arms DISAGREE on the antisymmetry decision -> no physics claim; the discordance pattern is committed as the diagnostic and fed to the second budgeted iteration.";
    }
}

// ---------------------------------------------------------------------------
// Framing + fail-closed boundary (phase450 discipline, verbatim keys).
// ---------------------------------------------------------------------------
const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool cepConventionsAreWorkbenchConventions = true;
const bool physicistReviewPending = true;   // Wave-0 item 0.3 OPEN (explicit)
const string epsilonRealization = "independent-theta-dof-haar-sampled";
const string epsilonTaxonomyMode = "mode-4-nonperturbative-cep";
const string definition81Scope = "reduced-spin4-slice";
const bool ambientSevenSevenRealized = false;
const bool internalGaugeContentRealized = false;
const bool weldRealized = false;
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
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256Contract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    $"su2 trace-pairing; CreateUniform4DPeriodic(latticeCanonical:true) tori [{string.Join(",", TorusSizes)}]",
    "WHAM parity-antisymmetry error-model repair of phase450: two independent estimator arms (moving-block bootstrap with full WHAM re-solve propagating the f_i stitching-constant covariance; within-window antisymmetrized U_odd) on fresh ensembles under the four phase450 binding conditions with a corrected c=0/+-0.25 soft-spring sub-ladder; max-over-bins antisymmetry statistic calibrated against the fitted even CEP (baked 99th-percentile thresholds); S(k) structure-factor hooks on the released columns; pre-registered T1/T2/T3 verdict taxonomy; no target values")))).ToLowerInvariant();

bool whamParityErrorModelRepairPassed =
    precursorsPassed &&
    allHardBatteriesPassed &&
    CalibrationComplete && SmokeComplete &&
    scaleIsWorkbenchRelativeCandidateOnly && noGevPromotion &&
    cepConventionsAreWorkbenchConventions && physicistReviewPending &&
    definition81Scope == "reduced-spin4-slice" &&
    !ambientSevenSevenRealized && !internalGaugeContentRealized && !weldRealized &&
    targetBlindConstruction && !physicalTargetsConsultedForConstruction &&
    !physicalCouplingProvided && !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesVevOrSourceScaleLineage && !routeProvidesPoleExtractionAndGeVNormalization &&
    !routeCompletesBosonPredictions && !routePromotesWzMasses && !routePromotesHiggsMass &&
    !sourceContractApplicationAllowed && !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 && acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract && !canFillPhase201HiggsContract &&
    !canFillPhase256Contract && !canFillPhase256ObservedFieldExtractionContract;

string terminalStatus = whamParityErrorModelRepairPassed
    ? verdictKind switch
    {
        "pre-registration-committed" => TerminalPrefix + "passed-pre-registration-committed",
        "symmetric-phase-null" => TerminalPrefix + "passed-symmetric-phase-null-single-well",
        "z2-breaking-candidate" => TerminalPrefix + "passed-z2-breaking-candidate-workbench-relative-no-gev",
        "estimator-discordance-or-new-defect" => TerminalPrefix + "passed-estimator-discordance-recorded",
        _ => TerminalPrefix + "passed-inconclusive-gates-failed-recorded",
    }
    : TerminalPrefix + "blocked";

string decision = whamParityErrorModelRepairPassed
    ? $"The WHAM parity-antisymmetry error-model repair is decided on internal consistency (mode={Mode}). "
      + $"Stage-0 pre-registration is committed BEFORE any production trajectory: (1) the phase450 sd2 per-bin antisymmetry localization is recomputed from the committed cepCurve and recorded (max {localization.MaxSigma:F3} sigma at |Phi|={localization.MaxAbsPhi:F3}); (2) the output schema records per-bin antisymmetry and per-window signed-bin counts; (3) the c=0/+-0.25 soft-spring sub-ladder is pre-registered (start spring {SubLadderStartSpring:F1}, smoke-confirmed spring {SmokeFinalSubLadderSpring:F1}, junction overlap {SmokeJunctionMinOverlap:F3} >= {OverlapMin}); (4) the max-over-bins antisymmetry statistic is calibrated against the fitted even CEP (99th-pct sigma {CalibratedMaxAntisymSigma99:F3}, abs {CalibratedMaxAntisymAbs99:F3}; false-flag at 5 sigma {CalibratedFalseFlagRateAt5Sigma:P2}, at 3 sigma {CalibratedFalseFlagRateAt3Sigma:P2}); (5) the S(k) structure-factor hooks on the released columns and the two-arm analysis + T1/T2/T3 taxonomy are pre-registered. "
      + $"VERDICT ({verdictKind}): {verdictPhrase} "
      + "MANDATORY FRAMING: workbench-relative candidate data ONLY (su(2) toy algebra, reduced Spin(4) slice, lattice units); beta, umbrella springs, the Phi inner product, the HMC kinetic mass, and the theta-Haar chart are WORKBENCH CONVENTIONS pending physicist review (Wave-0 item 0.3 OPEN); NO GeV/pole/VEV promotion; no Phase201/Phase256 contract field is filled; nothing is promoted."
    : "Do not use the pre-registration until the precursors (448/449/450), the hard batteries, and the calibration + smoke completeness flags pass.";

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
    virialRelDeviation = FiniteOrNull(w.VirialRel),
    virialGate = w.VirialGate,
    tauIntPhi = FiniteOrNull(w.TauPhi),
    tauIntSb = FiniteOrNull(w.TauS),
    neffPhi = FiniteOrNull(w.NeffPhi),
    neffSb = FiniteOrNull(w.NeffS),
    ergodicityGate = w.ErgodicityGate,
    meanPhi = FiniteOrNull(w.MeanPhi),
    sdPhi = FiniteOrNull(w.SdPhi),
    msPerTrajectory = FiniteOrNull(w.MsPerTraj),
};

object ProductionArmJson(ProductionMemberArm a) => new
{
    member = a.Name,
    isControl = a.IsControl,
    beta = a.Beta,
    windowCount = a.Plan.Centers.Length,
    windowSpacing = a.Plan.Spacing,
    binWidth = a.Plan.BinW,
    windows = a.Windows.Select(WindowJson).ToArray(),
    perWindowSignedBinCounts = a.SignedCounts.Select(s => new
    {
        center = s.Center, positiveBinCounts = s.Pos, negativeBinCounts = s.Neg, zeroBinCounts = s.Zero,
    }).ToArray(),
    neighborOverlaps = a.NeighborOverlaps.Select(o => new { centerLo = o.CLo, centerHi = o.CHi, overlap = FiniteOrNull(o.Overlap) }).ToArray(),
    minNeighborOverlap = FiniteOrNull(a.OverlapMin),
    overlapGate = a.OverlapGate,
    whamResidual = FiniteOrNull(a.WhamResid),
    whamGate = a.WhamGate,
    coverageHalfWidth = FiniteOrNull(a.CoverageHalfWidth),
    coverageGate = a.CoverageGate,
    baselineAntisymmetryMaxAbs = FiniteOrNull(a.BaseMaxAbs),
    baselineAntisymmetryMaxSigma = FiniteOrNull(a.BaseMaxSig),
    perBinAntisymmetry = a.PerBinAntisym.Select(p => new
    {
        phi = p.Phi, deltaU = FiniteOrNull(p.DeltaU), combinedError = FiniteOrNull(p.Se),
        sigma = FiniteOrNull(p.Sigma), countPos = p.CountPos, countNeg = p.CountNeg,
    }).ToArray(),
    tadpoleDiagnostic = new
    {
        note = "UNCONSTRAINED (k=0) released column; direct <Phi> of the raw joint ensemble, independent of WHAM",
        meanPhi = FiniteOrNull(a.Tadpole.MeanPhi), standardError = FiniteOrNull(a.TadpoleSe),
        significanceSigma = FiniteOrNull(a.TadpoleSigma), sign = a.TadpoleSign,
    },
    armA = new
    {
        note = "moving-block bootstrap over per-window trajectories with a FULL WHAM re-solve on each replicate (propagates the f_i stitching-constant covariance)",
        replicates = a.ArmA.Replicates, maxBlockLength = a.ArmA.BlockLenMax,
        maxAntisymmetrySigma = FiniteOrNull(a.ArmA.MaxAntisymSigma),
        maxAntisymmetryAbs = FiniteOrNull(a.ArmA.MaxAntisymAbs),
        bootstrapSeAtMax = FiniteOrNull(a.ArmA.BootstrapSeAtMax),
        flagsAntisymmetry = a.ArmA.FlagsAntisymmetry,
    },
    armB = new
    {
        note = "within-window antisymmetrized U_odd estimator; both sign bins N_eff >= 25 to enter",
        binsUsed = a.ArmB.BinsUsed, maxAntisymmetrySigma = FiniteOrNull(a.ArmB.MaxAntisymSigma),
        maxAntisymmetryAbs = FiniteOrNull(a.ArmB.MaxAntisymAbs), flagsAntisymmetry = a.ArmB.FlagsAntisymmetry,
    },
    structureFactorSamples = a.SkSamples,
    structureFactor = a.Sk.Select(p => new { k = p.K, s = FiniteOrNull(p.S) }).ToArray(),
    classification = a.Classification,
    tStar = FiniteOrNull(a.TStar),
    depthSigma = FiniteOrNull(a.DepthSigma),
    flatBottomHalfWidth = FiniteOrNull(a.FlatHalfWidth),
};

var result = new
{
    phaseId = "phase453-wham-parity-error-model-repair",
    generatedAt = DateTimeOffset.UtcNow,
    mode = Mode,
    terminalStatus,
    whamParityErrorModelRepairPassed,
    applicationSubjectKind = ApplicationSubjectKind,

    phase448PrecursorPassed,
    phase449PrecursorPassed,
    phase450PrecursorPassed,
    precursorsPassed,

    verdictKind,
    verdictPhrase,

    // Stage-0 item 1: recomputed phase450 per-bin antisymmetry localization.
    phase450DiscrepancyLocalization = new
    {
        note = "recomputed from the COMMITTED phase450 sd2 cepCurve (only max fields are committed there); committed provenance for the motivating discrepancy",
        member = localization.Member,
        binWidth = localization.BinWidth,
        maxSigma = FiniteOrNull(localization.MaxSigma),
        maxAbs = FiniteOrNull(localization.MaxAbs),
        maxAbsPhi = FiniteOrNull(localization.MaxAbsPhi),
        committedAntisymmetryMaxSigma = FiniteOrNull(localization.CommittedMaxSigma),
        reproducesCommitted = localization.ReproducesCommitted,
        signedBinCountAsymmetryNote = "signal grows monotonically toward the edges and the max sits at the far edge |Phi|~2.375 -- the WHAM stitch-error signature (edge windows accumulate the most f_i random-walk variance), while the independent unconstrained tadpole is -0.04 +- 0.10 (0.44 sigma): consistent with an error-model artifact, the hypothesis this phase discriminates",
        perBin = localization.PerBin.Select(p => new
        {
            phi = p.Phi, deltaU = FiniteOrNull(p.DeltaU), combinedError = FiniteOrNull(p.Se),
            sigma = FiniteOrNull(p.Sigma), countPos = p.CountPos, countNeg = p.CountNeg,
        }).ToArray(),
    },

    // Stage-0 item 4: baked calibration constants + (if just run) the fresh numbers.
    calibration = new
    {
        note = "synthetic control ensembles from the fitted EVEN CEP of the committed phase450 sd2 record with matched per-window tauInt and the exact production window layout; 99th-percentile thresholds baked as committed decision boundaries",
        calibrationComplete = CalibrationComplete,
        replicates = CalibrationReplicates,
        syntheticTrajPerWindow = CalibrationSyntheticTrajPerWindow,
        bakedMaxAntisymSigma99 = CalibratedMaxAntisymSigma99,
        bakedMaxAntisymAbs99 = CalibratedMaxAntisymAbs99,
        bakedFalseFlagRateAt5Sigma = CalibratedFalseFlagRateAt5Sigma,
        bakedFalseFlagRateAt3Sigma = CalibratedFalseFlagRateAt3Sigma,
        evenCepFit = new { a2 = FiniteOrNull(evenCepFit.A2), a4 = FiniteOrNull(evenCepFit.A4), a6 = FiniteOrNull(evenCepFit.A6) },
        freshRun = calibration is null ? null : (object)new
        {
            replicates = calibration.Replicates,
            maxAntisymSigma99 = FiniteOrNull(calibration.MaxAntisymSigma99),
            maxAntisymAbs99 = FiniteOrNull(calibration.MaxAntisymAbs99),
            falseFlagAt5Sigma = FiniteOrNull(calibration.FalseFlagAt5Sigma),
            falseFlagAt3Sigma = FiniteOrNull(calibration.FalseFlagAt3Sigma),
            meanMaxSigma = FiniteOrNull(calibration.MeanMaxSigma),
            maxMaxSigma = FiniteOrNull(calibration.MaxMaxSigma),
        },
    },

    // Stage-0 item 3: the corrected sub-ladder + smoke junction-overlap gate.
    correctedLadder = new
    {
        note = "c=0/+-0.25 soft-spring sub-ladder straddling the origin, spring from the phase450 auto-spring rule at the recorded 2.0 softening factor; junction neighbor-overlap smoke-tested before production",
        mainLadderSpacing = 0.5,
        mainLadderSpring = 16.0,
        subLadderSpacing = SubLadderSpacing,
        subLadderHalfSpan = SubLadderHalfSpan,
        autoSpringSoftening = AutoSpringSoftening,
        subLadderStartSpring = SubLadderStartSpring,
        smokeComplete = SmokeComplete,
        smokeBudgetedIterations = SmokeBudgetedIterations,
        smokeIterationsUsed = SmokeIterationsUsed,
        smokeFinalSubLadderSpring = SmokeFinalSubLadderSpring,
        smokeJunctionMinOverlap = SmokeJunctionMinOverlap,
        smokeTrajPerWindow = SmokeTrajPerWindow,
        neighborOverlapGate = OverlapMin,
        smokeIterationLog = SmokeIterationLog,
        productionWindowCenters = recordedSd2Plan!.Centers,
        productionWindowSprings = recordedSd2Plan!.Springs,
        freshSmoke = smokeResult is null ? null : (object)new
        {
            junctionMinOverlap = FiniteOrNull(smokeResult.JunctionMinOverlap),
            junctionGatePass = smokeResult.JunctionGatePass,
            rows = smokeResult.Rows,
        },
    },

    // Stage-0 item 5: S(k) structure-factor hooks (schema; filled in production).
    structureFactorHooks = new
    {
        note = "Team B rank-2 Arm C: S(k) on the UNCONSTRAINED/released (k=0) columns only, pre-registered EX ANTE (slice-summed summaries cannot be retro-extracted)",
        momenta = "k = 2*pi*m/n, m in {0..n-1}^4 on the (Z_n)^4 reciprocal lattice",
        observable = "S(k) = < | sum_e <u_inv,omega>_e exp(-i k . x_edgeBase(e)) |^2 > over the released joint ensemble (per-config, accumulated online)",
        columns = "measured on the k=0 tadpole/released window of each member arm",
        emittedInProduction = Mode == "production",
    },

    // Pre-registered verdict taxonomy (baked strings).
    verdictTaxonomy = new
    {
        T1 = "symmetric-phase-null: both arms green (all per-bin antisymmetry within the calibrated thresholds) AND single-well-at-zero on both members AND fresh tadpole zero within errors AND identity control clean -> the null IS claimed",
        T2 = "z2-breaking-candidate: antisymmetry survives BOTH corrected arms above the calibrated thresholds AND the fresh independent tadpole moves off zero at >= 5 sigma with coherent sign on the same member -> simultaneous program-level alarm; escalate to a volume scan at Phi*",
        T3 = "estimator-discordance-or-new-defect: arms disagree, or gates fail in a new pattern -> no physics claim; commit the discordance pattern as the diagnostic",
        hardBatteryRule = "any phase450 hard-battery failure (axis-angle chart, su(2) projection, <e^-dH>, virial, acceptance windows) => inconclusive-gates-failed regardless of the arms",
    },

    productionArms = productionArms.Select(ProductionArmJson).ToArray(),

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
            thetaChartBattery = t.ThetaChartBattery,
            identityThetaGradNorm = FiniteOrNull(t.IdentityThetaGradNorm),
            identityThetaIndependenceBattery = t.IdentityThetaIndependenceBattery,
            projectorOverlapMaxAfter = FiniteOrNull(t.ProjectorOverlapMaxAfter),
            projectorBattery = t.ProjectorBattery,
            hardBatteriesPassed = t.HardBatteriesPassed,
        }).ToArray(),
    },

    probeConfiguration = new
    {
        torusSizes = TorusSizes,
        latticeCanonical = true,
        lieAlgebraId = "su2-trace-pairing",
        memberTags = MemberTags,
        tMax = TMax,
        beta = Beta,
        betaConvention = BetaConvention,
        neffTarget = NeffTarget,
        leapfrogSteps = NLeap,
        thetaProposalsPerTrajectory = ThetaProps,
        samplerSeed = SamplerSeed,
        rayRngSeed = RayRngSeed,
        armABootstrapReplicates = ArmABootstrapReplicates,
        armBMinEffectiveSignBinCounts = ArmBMinEffectiveSignBinCounts,
        defaultMode = DefaultMode,
        productionLaunchCommand = "PHASE453_MODE=production dotnet run --no-build -c Release --project studies/phase453_wham_parity_error_model_repair_001/Phase453WhamParityErrorModelRepair.csproj  (env-clean once the committed DefaultMode is flipped to production; ~2.5-4.5 h from the phase450 per-window rates)",
        fourBindingConditions = new[]
        {
            "(i) gauge-invariant invariant-ray collective coordinate (oSign lattice gauge; global-su(2) zero modes projected; projector recorded)",
            "(ii) theta-Haar measure (per-vertex SO(3) rotations, symmetric Metropolis; axis-angle fundamental-domain chart)",
            "(iii) ergodicity control (tau_int per window; N_eff >= 100 gate; adaptive trajectories)",
            "(iv) theta=0 slice is never a verdict -- the physics run is the joint (omega, theta-Haar) ensemble",
        },
        faddeevPopovCaveat = "cross-member normalization comparisons barred; only within-member Phi-dependence is classified",
        sbGradEvals,
        sbObjEvals,
    },

    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    cepConventionsAreWorkbenchConventions,
    physicistReviewPending,
    recordedBoundary = new
    {
        definition81Scope, ambientSevenSevenRealized, internalGaugeContentRealized, weldRealized,
        canFillPhase201WzContract, canFillPhase256Contract, physicistReviewPending,
        cepConventionsAreWorkbenchConventions, epsilonRealization, epsilonTaxonomyMode,
        baseSignature = "Cl(4,0)-euclidean-slice", spinorUsedAsShiabCarrierNotFiber = true,
    },

    runtimeSeconds,
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    targetBlindConstructionHash,
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
        canFillPhase201WzContract, canFillPhase201HiggsContract,
        canFillPhase256Contract, canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    sourceEvidence = new
    {
        phase448SummaryPath = Phase448SummaryPath,
        phase449SummaryPath = Phase449SummaryPath,
        phase450SummaryPath = Phase450SummaryPath,
        designSourcePath = DesignSourcePath,
        physicsDecisionsSourcePath = PhysicsDecisionsSourcePath,
        programSourcePath = ProgramSourcePath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "This is a Stage-0 PRE-REGISTRATION; no production trajectory is run in this checkpoint and no physics verdict (T1/T2/T3) is emitted here. The fresh-ensemble production run executes later, env-clean.",
        "A T1 symmetric-phase null, if reached in production, is finite-volume single-beta evidence along one gauge-invariant collective coordinate -- workbench-relative, su(2) toy algebra, reduced Spin(4) slice, lattice units; NOT a theorem; NO GeV/pole/VEV.",
        "A T2 Z2-breaking candidate, if reached, is a program-level ALARM (an exact symmetry contradicted), never a physical SSB claim; it triggers a volume scan, not a promotion.",
        "beta, the umbrella springs, the Euclidean Phi inner product, the flat HMC kinetic mass, the theta-Haar chart, and the WHAM bin/error conventions are WORKBENCH CONVENTIONS pending physicist review (Wave-0 item 0.3 OPEN).",
        "No VEV scale, pole, or GeV lineage; no Phase201/Phase256 contract field is filled; nothing is promoted.",
    },
    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wham_parity_error_model_repair.json"), JsonSerializer.Serialize(result, options));
string summaryPath = Path.Combine(outputDir, "wham_parity_error_model_repair_summary.json");
File.WriteAllText(summaryPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"whamParityErrorModelRepairPassed={whamParityErrorModelRepairPassed} mode={Mode} verdictKind={verdictKind}");
Console.WriteLine($"precursors: 448={phase448PrecursorPassed} 449={phase449PrecursorPassed} 450={phase450PrecursorPassed}");
Console.WriteLine($"localization: max {localization.MaxSigma:F3} sigma at |Phi|={localization.MaxAbsPhi:F3} (committed {localization.CommittedMaxSigma:F3}; reproduces={localization.ReproducesCommitted})");
Console.WriteLine($"HARD BATTERIES: all={allHardBatteriesPassed} whamPlumbing={whamPlumbingBattery}");
Console.WriteLine($"calibration baked: sigma99={CalibratedMaxAntisymSigma99:F3} abs99={CalibratedMaxAntisymAbs99:F3} ff5={CalibratedFalseFlagRateAt5Sigma:P2} ff3={CalibratedFalseFlagRateAt3Sigma:P2}");
Console.WriteLine($"smoke baked: junctionOverlap={SmokeJunctionMinOverlap:F3} iters={SmokeIterationsUsed}/{SmokeBudgetedIterations} finalSpring={SmokeFinalSubLadderSpring:F1}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F1} sbGradEvals={sbGradEvals} sbObjEvals={sbObjEvals}");
Console.WriteLine($"summaryPath={summaryPath}");

// ---------------------------------------------------------------------------
// Stage-0 item 1: recompute the phase450 sd2 per-bin antisymmetry localization
// from the committed cepCurve; fit the EVEN CEP; return the sd2 window taus.
// ---------------------------------------------------------------------------
Localization RecomputePhase450Localization(JsonElement root, out EvenCepFit even,
    out List<(double Center, double Tau)> sdTau)
{
    var torusArms = root.GetProperty("torusArms");
    JsonElement sd = default; bool found = false;
    foreach (var t in torusArms.EnumerateArray())
    {
        foreach (var mem in t.GetProperty("members").EnumerateArray())
        {
            if (mem.GetProperty("isControl").GetBoolean()) continue;
            sd = mem; found = true; break;
        }
        if (found) break;
    }
    if (!found) throw new InvalidOperationException("phase450 sd2 member not found.");
    double binW = sd.GetProperty("binWidth").GetDouble();
    var byKey = new Dictionary<int, (double Phi, double U, double Err, long Counts)>();
    foreach (var b in sd.GetProperty("cepCurve").EnumerateArray())
    {
        double phi = b.GetProperty("phi").GetDouble();
        double u = b.GetProperty("u").GetDouble();
        double err = b.GetProperty("errApprox").GetDouble();
        long cnt = b.GetProperty("counts").GetInt64();
        byKey[(int)System.Math.Round(phi / binW)] = (phi, u, err, cnt);
    }
    var perBin = new List<PerBinAntisym>();
    double maxSig = 0, maxAbs = 0, maxAbsPhi = 0;
    foreach (var kv in byKey.OrderBy(x => x.Key))
    {
        int key = kv.Key;
        if (key <= 0) continue;
        if (!byKey.TryGetValue(-key, out var mirror)) continue;
        double d = System.Math.Abs(kv.Value.U - mirror.U);
        double se = System.Math.Sqrt(kv.Value.Err * kv.Value.Err + mirror.Err * mirror.Err);
        double sig = d / System.Math.Max(se, 1e-12);
        perBin.Add(new PerBinAntisym(kv.Value.Phi, kv.Value.U - mirror.U, se, sig, kv.Value.Counts, mirror.Counts));
        if (sig > maxSig) { maxSig = sig; maxAbs = d; maxAbsPhi = kv.Value.Phi; }
    }
    double committed = sd.GetProperty("antisymmetryMaxSigma").GetDouble();
    bool reproduces = System.Math.Abs(maxSig - committed) <= 1e-3 * System.Math.Max(1.0, committed);

    // Even CEP fit a2 x^2 + a4 x^4 + a6 x^6 on x=|Phi|, y=(U(Phi)+U(-Phi))/2.
    var xs = new List<double>(); var ys = new List<double>();
    foreach (var kv in byKey.OrderBy(x => x.Key))
    {
        int key = kv.Key;
        if (key < 0) continue;
        if (key == 0) { xs.Add(0.0); ys.Add(kv.Value.U); continue; }
        if (!byKey.TryGetValue(-key, out var mirror)) continue;
        xs.Add(System.Math.Abs(kv.Value.Phi));
        ys.Add(0.5 * (kv.Value.U + mirror.U));
    }
    even = FitEvenCep(xs.ToArray(), ys.ToArray());

    sdTau = new List<(double, double)>();
    foreach (var w in sd.GetProperty("windows").EnumerateArray())
    {
        double c = w.GetProperty("center").GetDouble();
        double tau = w.TryGetProperty("tauIntPhi", out var te) && te.ValueKind == JsonValueKind.Number ? te.GetDouble() : 1.0;
        sdTau.Add((c, tau));
    }
    return new Localization("sd2-id0/c0.5", binW, maxSig, maxAbs, maxAbsPhi, committed, reproduces, perBin);
}

static EvenCepFit FitEvenCep(double[] x, double[] y)
{
    // Normal equations for [x^2, x^4, x^6].
    var ata = new double[3, 3];
    var atb = new double[3];
    for (int i = 0; i < x.Length; i++)
    {
        double x2 = x[i] * x[i];
        var row = new[] { x2, x2 * x2, x2 * x2 * x2 };
        for (int a = 0; a < 3; a++)
        {
            atb[a] += row[a] * y[i];
            for (int b = 0; b < 3; b++) ata[a, b] += row[a] * row[b];
        }
    }
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
    return new EvenCepFit(m[0, 0] != 0 ? m[0, 3] / m[0, 0] : 0.0,
        m[1, 1] != 0 ? m[1, 3] / m[1, 1] : 0.0, m[2, 2] != 0 ? m[2, 3] / m[2, 2] : 0.0);
}

// --- CEP bin construction (phase450 error model; baseline curve). -----------
List<CepBin> BuildCepBins(double[] lnP, WindowPlan plan, (long[] Counts, long N, double Center, double K)[] wins,
    double tauMean, double beta, out double sigmaRough)
{
    int wCenter = 0;
    for (int w = 1; w < wins.Length; w++)
        if (System.Math.Abs(wins[w].Center) < System.Math.Abs(wins[wCenter].Center)) wCenter = w;
    var pairVar = new double[System.Math.Max(wins.Length - 1, 0)];
    for (int w = 0; w + 1 < wins.Length; w++)
    {
        long novl = 0;
        for (int b = 0; b < plan.BinPhi.Length; b++) novl += System.Math.Min(wins[w].Counts[b], wins[w + 1].Counts[b]);
        double neffOvl = System.Math.Max(novl / (2.0 * tauMean), 1.0);
        pairVar[w] = 1.0 / (beta * beta * neffOvl);
    }
    double StitchVar(double phi)
    {
        int wb = 0;
        for (int w = 1; w < wins.Length; w++)
            if (System.Math.Abs(wins[w].Center - phi) < System.Math.Abs(wins[wb].Center - phi)) wb = w;
        double s = 0.0;
        for (int w = System.Math.Min(wb, wCenter); w < System.Math.Max(wb, wCenter); w++) s += pairVar[w];
        return s;
    }
    var bins = new List<CepBin>();
    double uMinRaw = double.PositiveInfinity;
    for (int b = 0; b < plan.BinPhi.Length; b++)
    {
        long tot = 0; foreach (var w in wins) tot += w.Counts[b];
        if (tot < MinBinCounts || double.IsNegativeInfinity(lnP[b])) continue;
        double u = -lnP[b] / beta;
        double errCount = 1.0 / (beta * System.Math.Sqrt(System.Math.Max(tot / (2.0 * tauMean), 1.0)));
        double err = System.Math.Sqrt(errCount * errCount + StitchVar(plan.BinPhi[b]));
        uMinRaw = System.Math.Min(uMinRaw, u);
        bins.Add(new CepBin(plan.BinPhi[b], u, err, tot));
    }
    for (int i = 0; i < bins.Count; i++) bins[i] = bins[i] with { U = bins[i].U - uMinRaw };
    bins = bins.OrderBy(x => x.Phi).ToList();
    sigmaRough = 0.0;
    if (bins.Count >= 7)
    {
        var resid = new List<double>();
        for (int i = 1; i < bins.Count - 1; i++)
            resid.Add(System.Math.Abs(bins[i - 1].U - 2.0 * bins[i].U + bins[i + 1].U));
        resid.Sort();
        sigmaRough = 1.4826 * resid[resid.Count / 2] / System.Math.Sqrt(6.0);
    }
    double sr = sigmaRough;
    for (int i = 0; i < bins.Count; i++)
        bins[i] = bins[i] with { Err = System.Math.Sqrt(bins[i].Err * bins[i].Err + sr * sr) };
    return bins;
}

PerBinAntisym[] PerBinAntisymmetry(List<CepBin> bins, double binW)
{
    var byKey = bins.ToDictionary(x => (int)System.Math.Round(x.Phi / binW));
    var outp = new List<PerBinAntisym>();
    foreach (var x in bins.OrderBy(b => b.Phi))
    {
        int key = (int)System.Math.Round(x.Phi / binW);
        if (key <= 0 || !byKey.TryGetValue(-key, out var y)) continue;
        double se = System.Math.Sqrt(x.Err * x.Err + y.Err * y.Err);
        outp.Add(new PerBinAntisym(x.Phi, x.U - y.U, se, System.Math.Abs(x.U - y.U) / System.Math.Max(se, 1e-12), x.Counts, y.Counts));
    }
    return outp.ToArray();
}

(string Cls, double TStar, double Depth, double DepthSigma, double FlatHalfWidth) ClassifyCep(List<CepBin> bins, double binW)
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
    int lo = ctrIdx, hi = ctrIdx;
    while (lo - 1 >= 0 && sorted[lo - 1].U - sorted[minIdx].U <= 2.0 * errMed) lo--;
    while (hi + 1 < sorted.Count && sorted[hi + 1].U - sorted[minIdx].U <= 2.0 * errMed) hi++;
    double flatHalfWidth = 0.5 * (sorted[hi].Phi - sorted[lo].Phi);
    bool edgesRise = lo > 0 && hi < sorted.Count - 1
        && sorted[0].U - sorted[minIdx].U > 5.0 * errMed && sorted[^1].U - sorted[minIdx].U > 5.0 * errMed;
    if (System.Math.Abs(sorted[minIdx].Phi) <= 1.5 * binW)
    {
        bool mono = true; double run = sorted[minIdx].U;
        for (int i = minIdx + 1; i < sorted.Count; i++) { if (sorted[i].U < run - MonotoneSlackSigma * errMed) { mono = false; break; } run = System.Math.Max(run, sorted[i].U); }
        run = sorted[minIdx].U;
        for (int i = minIdx - 1; i >= 0 && mono; i--) { if (sorted[i].U < run - MonotoneSlackSigma * errMed) { mono = false; break; } run = System.Math.Max(run, sorted[i].U); }
        if (mono && flatHalfWidth >= FlatBottomMinHalfWidth && edgesRise)
            return ("flat-bottom-degenerate-minima", flatHalfWidth, depth, depthSigma, flatHalfWidth);
        return (mono ? "single-well-at-zero" : "inconclusive-structure", double.NaN, depth, depthSigma, flatHalfWidth);
    }
    double tS = System.Math.Abs(sorted[minIdx].Phi);
    bool pairOk = sorted.Any(x => System.Math.Abs(x.Phi + sorted[minIdx].Phi) <= 2.0 * binW
        && x.U - sorted[minIdx].U <= SsbDepthSigma * errMed && sorted[ctrIdx].U - x.U > SsbDepthSigma * errMed);
    if (depthSigma > SsbDepthSigma && pairOk)
        return ("flat-bottom-degenerate-minima", tS, depth, depthSigma, flatHalfWidth);
    bool mono2 = true; double run2 = sorted[minIdx].U;
    for (int i = minIdx + 1; i < sorted.Count && mono2; i++) { if (sorted[i].U < run2 - MonotoneSlackSigma * errMed) mono2 = false; run2 = System.Math.Max(run2, sorted[i].U); }
    run2 = sorted[minIdx].U;
    for (int i = minIdx - 1; i >= 0 && mono2; i--) { if (sorted[i].U < run2 - MonotoneSlackSigma * errMed) mono2 = false; run2 = System.Math.Max(run2, sorted[i].U); }
    if (mono2 && flatHalfWidth >= FlatBottomMinHalfWidth && edgesRise)
        return ("flat-bottom-degenerate-minima", flatHalfWidth, depth, depthSigma, flatHalfWidth);
    if (mono2)
        return (depthSigma <= SsbDepthSigma ? "single-well-at-zero" : "single-well-offset", tS, depth, depthSigma, flatHalfWidth);
    return ("inconclusive-structure", tS, depth, depthSigma, flatHalfWidth);
}

// --- ARM A: moving-block bootstrap + full WHAM re-solve. --------------------
ArmAResult ArmABootstrap(List<WindowResult> windows, WindowPlan plan, double beta)
{
    var rng = new Random(SamplerSeed + 991);
    int nb = plan.BinPhi.Length, nbHalf = nb / 2;
    double binW = plan.BinW;
    // Block length per window from its own tauIntPhi.
    var blockLen = windows.Select(w => System.Math.Max(1, (int)System.Math.Ceiling(2.0 * System.Math.Max(w.TauPhi, 0.5)))).ToArray();
    int blockMax = blockLen.DefaultIfEmpty(1).Max();
    double tauMean = windows.Average(w => System.Math.Max(w.TauPhi, 0.5));
    // For each replicate: resample blocks of trajectories per window, rebuild
    // histograms, re-solve WHAM, record the max-over-bins antisymmetry.
    var maxSigs = new List<double>();
    var maxAbss = new List<double>();
    int nrep = System.Math.Min(ArmABootstrapReplicates, System.Math.Max(1, ArmABootstrapReplicates));
    for (int rep = 0; rep < nrep; rep++)
    {
        var wins = new (long[] Counts, long N, double Center, double K)[windows.Count];
        for (int w = 0; w < windows.Count; w++)
        {
            var series = windows[w].PhiSeries;
            int nT = series.Length;
            var counts = new long[nb];
            long n = 0;
            if (nT == 0) { wins[w] = (counts, 1, windows[w].Center, windows[w].KUmb); continue; }
            int L = System.Math.Min(blockLen[w], nT);
            int nBlocks = System.Math.Max(1, nT / L);
            for (int bl = 0; bl < nBlocks; bl++)
            {
                int start = rng.Next(nT);
                for (int j = 0; j < L; j++)
                {
                    double phi = series[(start + j) % nT];
                    int b = (int)System.Math.Round(phi / binW) + nbHalf;
                    if (b >= 0 && b < nb) { counts[b]++; n++; }
                }
            }
            wins[w] = (counts, System.Math.Max(n, 1), windows[w].Center, windows[w].KUmb);
        }
        var (lnP, _, resid, _) = WhamSolve(wins, plan.BinPhi);
        if (!double.IsFinite(resid) || resid > WhamResidualTol) continue;
        var (mSig, mAbs) = MaxAntisymmetryFromLnP(lnP, plan, wins, tauMean, beta);
        if (double.IsFinite(mSig)) maxSigs.Add(mSig);
        if (double.IsFinite(mAbs)) maxAbss.Add(mAbs);
    }
    // The bootstrap distribution of the max antisymmetry directly gives the
    // covariance-propagated significance; compare its MEAN to the calibrated
    // null 99th percentile. SE at max = sd of the bootstrap max distribution.
    double meanSig = maxSigs.DefaultIfEmpty(0).Average();
    double meanAbs = maxAbss.DefaultIfEmpty(0).Average();
    double se = maxSigs.Count > 1 ? Sd(maxSigs, meanSig) : double.NaN;
    bool flags = meanSig > CalibratedMaxAntisymSigma99;
    return new ArmAResult(maxSigs.Count, blockMax, meanSig, meanAbs, se, flags);
}

// --- ARM B: within-window antisymmetrized U_odd estimator. ------------------
ArmBResult ArmBAntisymmetrized(List<WindowResult> windows, WindowPlan plan, double beta)
{
    int nb = plan.BinPhi.Length, nbHalf = nb / 2;
    double binW = plan.BinW;
    double maxSig = 0, maxAbs = 0; int used = 0;
    // For each positive bin, combine the single-window U_odd estimates from
    // every window that covers BOTH +-Phi with sufficient effective counts.
    for (int b = nbHalf + 1; b < nb; b++)
    {
        int mb = 2 * nbHalf - b;
        if (mb < 0) continue;
        double phi = plan.BinPhi[b];
        double wsum = 0, wnum = 0, varAcc = 0; int contrib = 0;
        foreach (var w in windows)
        {
            double tau = System.Math.Max(w.TauPhi, 0.5);
            double nEffPos = w.Counts[b] / (2.0 * tau);
            double nEffNeg = w.Counts[mb] / (2.0 * tau);
            if (nEffPos < ArmBMinEffectiveSignBinCounts || nEffNeg < ArmBMinEffectiveSignBinCounts) continue;
            double bPos = 0.5 * w.KUmb * (phi - w.Center) * (phi - w.Center);
            double bNeg = 0.5 * w.KUmb * (-phi - w.Center) * (-phi - w.Center);
            // U(Phi)-U(-Phi) = -(1/beta)[ ln n_w(Phi) - ln n_w(-Phi) + b_w(Phi) - b_w(-Phi) ]
            double uOdd = -(1.0 / beta) * (System.Math.Log(w.Counts[b]) - System.Math.Log(w.Counts[mb]) + bPos - bNeg);
            double var = (1.0 / (beta * beta)) * (1.0 / nEffPos + 1.0 / nEffNeg);
            double wt = 1.0 / System.Math.Max(var, 1e-12);
            wnum += wt * uOdd; wsum += wt; varAcc += wt; contrib++;
        }
        if (contrib == 0 || wsum <= 0) continue;
        double uComb = wnum / wsum;
        double seComb = System.Math.Sqrt(1.0 / wsum);
        double sig = System.Math.Abs(uComb) / System.Math.Max(seComb, 1e-12);
        used++;
        maxAbs = System.Math.Max(maxAbs, System.Math.Abs(uComb));
        maxSig = System.Math.Max(maxSig, sig);
    }
    bool flags = maxSig > CalibratedMaxAntisymSigma99;
    return new ArmBResult(used, maxSig, maxAbs, flags);
}

// ---------------------------------------------------------------------------
// Local helpers (verbatim phase450 where shared).
// ---------------------------------------------------------------------------
static double EnvD(string k, double d) =>
    double.TryParse(Environment.GetEnvironmentVariable(k),
        System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : d;
static int EnvI(string k, int d) => int.TryParse(Environment.GetEnvironmentVariable(k), out var v) ? v : d;
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
static double Dot(double[] a, double[] b) { double s = 0; for (int i = 0; i < a.Length; i++) s += a[i] * b[i]; return s; }
static double Norm(double[] v) => System.Math.Sqrt(Dot(v, v));
static double RelDiff(double a, double b) =>
    System.Math.Abs(a - b) / System.Math.Max(1e-30, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
static double? FiniteOrNull(double x) => double.IsFinite(x) ? x : null;
static double Sd(List<double> x, double mean)
{
    double s = 0; foreach (var v in x) s += (v - mean) * (v - mean);
    return System.Math.Sqrt(s / System.Math.Max(1, x.Count - 1));
}
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
static double[] QToAxisAngle(double[] q)
{
    double w = q[0], x = q[1], y = q[2], z = q[3];
    double nrm = System.Math.Sqrt(w * w + x * x + y * y + z * z);
    w /= nrm; x /= nrm; y /= nrm; z /= nrm;
    if (w < 0) { w = -w; x = -x; y = -y; z = -z; }
    double s = System.Math.Sqrt(x * x + y * y + z * z);
    if (s < 1e-14) return new[] { 2.0 * x, 2.0 * y, 2.0 * z };
    double angle = 2.0 * System.Math.Atan2(s, w);
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
        ? value.GetBoolean() : null;
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString() : null;

BranchManifest BuildManifest() => new()
{
    BranchId = "phase453-einsteinian-shiab",
    SchemaVersion = "1.0.0",
    SourceEquationRevision = "draft-2021",
    CodeRevision = "phase453",
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
        ProjectionBinding = new GeometryBinding { BindingType = "projection", SourceSpace = ambientSpace, TargetSpace = baseSpace },
        ObservationBinding = new GeometryBinding { BindingType = "observation", SourceSpace = baseSpace, TargetSpace = ambientSpace },
        Patches = Array.Empty<PatchInfo>(),
    };
}

// ---------------------------------------------------------------------------
// Record types.
// ---------------------------------------------------------------------------
public sealed record WindowPlan(double[] Centers, double[] Springs, double[] BinPhi, double BinW, double Spacing);

public sealed record WindowResult(
    double Center, double KUmb, int Trajectories, double Eps, double AcceptRate,
    double ExpDh, double ExpDhSe, bool ExpDhGate,
    double VirialMean, double VirialRel, double VirialSe, bool VirialGate,
    double TauPhi, double TauS, double NeffPhi, double NeffS, bool ErgodicityGate,
    double MeanPhi, double SdPhi, double MeanS, double DriftRel,
    double ThetaGlobalAcc, double ThetaSingleAcc, double ThetaStep,
    long[] Counts, double MsPerTraj, double[] PhiSeries, long SkSamples);

public sealed record CepBin(double Phi, double U, double Err, long Counts);
public sealed record SignedBinCount(double Center, long Pos, long Neg, long Zero);
public sealed record SkPoint(int[] K, double S);
public sealed record PerBinAntisym(double Phi, double DeltaU, double Se, double Sigma, long CountPos, long CountNeg);
public sealed record ArmAResult(int Replicates, int BlockLenMax, double MaxAntisymSigma, double MaxAntisymAbs, double BootstrapSeAtMax, bool FlagsAntisymmetry);
public sealed record ArmBResult(int BinsUsed, double MaxAntisymSigma, double MaxAntisymAbs, bool FlagsAntisymmetry);
public sealed record CalibrationResult(int Replicates, double MaxAntisymSigma99, double MaxAntisymAbs99,
    double FalseFlagAt5Sigma, double FalseFlagAt3Sigma, double MeanMaxSigma, double MaxMaxSigma);
public sealed record SmokeResult(double StartSpring, double FinalSpring, double JunctionMinOverlap, bool JunctionGatePass, string[] Rows);
public sealed record Localization(string Member, double BinWidth, double MaxSigma, double MaxAbs, double MaxAbsPhi,
    double CommittedMaxSigma, bool ReproducesCommitted, List<PerBinAntisym> PerBin);

public sealed class EvenCepFit
{
    public double A2 { get; }
    public double A4 { get; }
    public double A6 { get; }
    public EvenCepFit(double a2, double a4, double a6) { A2 = a2; A4 = a4; A6 = a6; }
    public double U(double x) { double x2 = x * x; return A2 * x2 + A4 * x2 * x2 + A6 * x2 * x2 * x2; }
    public double LocalCurvature(double c)
    {
        double c2 = c * c;
        return System.Math.Max(2.0 * A2 + 12.0 * A4 * c2 + 30.0 * A6 * c2 * c2, 1e-6);
    }
}

public sealed record ProductionMemberArm(
    string Name, bool IsControl, double Beta, WindowPlan Plan, List<WindowResult> Windows,
    SignedBinCount[] SignedCounts, (double CLo, double CHi, double Overlap)[] NeighborOverlaps,
    double OverlapMin, bool OverlapGate, double WhamResid, int WhamIters, bool WhamGate,
    double CoverageHalfWidth, bool CoverageGate, double BaseMaxAbs, double BaseMaxSig,
    List<CepBin> Bins, PerBinAntisym[] PerBinAntisym, double SigmaRough,
    WindowResult Tadpole, double TadpoleSe, double TadpoleSigma, double TadpoleSign, bool WindowGatesAll,
    string Classification, double TStar, double Depth, double DepthSigma, double FlatHalfWidth,
    ArmAResult ArmA, ArmBResult ArmB, SkPoint[] Sk, long SkSamples);

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
    Dictionary<string, (double[] T, double[] S, double A2, double A3, double A4)> TreeByMember);
