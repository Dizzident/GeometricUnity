using System.Diagnostics;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

// Phase459: PHASE452 RECORD-RECONCILIATION ATTESTATION (Team A rank-1 "A0
// (GATING)"; Wave-0 item 0.2 of the 2026-07-10 three-team elimination
// program, docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md; phase number
// 459 = Team A's first registry number per
// docs/Phases/PHASE_NUMBER_REGISTRY.md).
//
// CHARGE. The phase452 record reconciliation CONTENT landed in the Wave-0
// ops checkpoint (e797defe): the committed default-budget 16000/10000-
// trajectory output is the SOLE canonical spectroscopy record, and the
// earlier journal/restart-prompt gap values (2.4352 / 2.4547) came from a
// never-committed reduced-budget env-override run and are superseded. This
// phase makes that reconciliation MACHINE-CHECKED AND STANDING: a
// zero-physics-compute, seconds-scale, fail-closed audit that runs in the
// generator forever and re-attests on every pass that
//   (1) the phase452 output carries the committed canonical CONFIGURATION
//       (trajProduction=16000, trajControl=10000, rngSeed=20260705), equal to
//       the committed phase452 Program.cs default literals (parsed from
//       source, phase202-style const + parse);
//   (2) the canonical gap values hold to tight tolerance (identity
//       a*m = 2.7132465417703235 with plateauChi2Dof null / window {0}
//       recorded as inconclusive-by-construction as a measurement; sd2
//       combined 2.5260 +- 0.0712 recomputed via the phase452 pre-registered
//       rho=1 combination rule from the stored per-interpolator gaps; the
//       0.56-sigma interpolator cross-check; the exact analytic free gaps
//       2.5509 / 2.5320 / 2.3570);
//   (3) the derived cross-action ratio sd2/identity, COMPUTED here from the
//       two stored gaps, is 0.931 and sits within 1.5 sigma of the exact
//       free-field ratio 0.9926 (the binding FREE-FIELD-COMPATIBLE label
//       condition; sigma recomputed from the stored errors);
//   (4) the superseded values 2.4352 / 2.4547 do NOT appear anywhere in the
//       phase452 output JSONs (they never did - asserted as standing tamper
//       protection, on volatile-field-scrubbed text because the generator
//       regenerates the outputs each pass);
//   (5) the standing claim boundary holds (target-blind, no contract fills,
//       no promotions, physicistReviewPending=true, lattice-units-only
//       language including the binding label caveat).
//
// PRE-REGISTERED VERDICT TAXONOMY (two terminals, fail-closed):
//   record-reconciled-canonical : ALL checks green.
//   config-mismatch-quarantine  : ANY check fails; every failing check is
//       named in failingChecks. Per the committed rule-out branch, this
//       demotes phase452 to unverified-output and quarantines every
//       spectroscopy row program-wide until a mandatory --full re-run.
// The phase always writes its verdict and exits 0 (the phase202/448
// pattern); scripts/verify_boson_claim_integrity.sh is what fail-closes the
// whole pass on the wrong verdict.
//
// VOLATILITY RULE. The generator REGENERATES the phase452 outputs each pass
// (fixed rngSeed => deterministic measurements). Only content that is stable
// across committed re-runs is asserted: configuration, seeded measurement
// values, structure, boundary flags, and prose. generatedAt /
// runtimeSeconds / msPerTrajectory are never read and are scrubbed before
// the tamper scan.
//
// ENV-CLEAN BY CONSTRUCTION: this phase reads NO environment variables (the
// phase452 reconciliation lesson applied to the attestor itself).
//
// MANDATORY FRAMING. This phase performs ZERO physics computation; it
// re-derives two numbers (the rho=1 combination and the cross-action ratio)
// from committed record values, in lattice units of the reduced spin-4
// slice workbench. Nothing here is a physical mass; the canonical gap is a
// pure-gauge-sector composite scale of THIS workbench action; NO
// GeV/pole/VEV promotion either way; no Phase201/Phase256 contract field is
// filled; nothing is promoted.

const string Phase452SummaryPath = "studies/phase452_scalar_channel_spectroscopy_probe_001/output/scalar_channel_spectroscopy_probe_summary.json";
const string Phase452FullOutputPath = "studies/phase452_scalar_channel_spectroscopy_probe_001/output/scalar_channel_spectroscopy_probe.json";
const string Phase452ProgramSourcePath = "studies/phase452_scalar_channel_spectroscopy_probe_001/Program.cs";
const string ProgramSourcePath = "docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md";
const string RegistrySourcePath = "docs/Phases/PHASE_NUMBER_REGISTRY.md";
const string DefaultOutputDir = "studies/phase459_spectroscopy_record_attestation_001/output";
const string ApplicationSubjectKind = "spectroscopy-record-attestation";
const string TerminalPrefix = "spectroscopy-record-attestation-";

// --- Pre-registered canonical configuration (the committed phase452
// Program.cs defaults; Wave-0 item 0.2). ------------------------------------
const int CanonTrajProduction = 16000;
const int CanonTrajControl = 10000;
const int CanonRngSeed = 20260705;
const int CanonTorusN = 3;

// --- Pre-registered canonical record values, pinned VERBATIM from the
// committed phase452 summary JSON (the Wave-0-reconciled canonical record;
// full stored double precision). --------------------------------------------
const double CanonIdentityMO1 = 2.7132465417703235;
const double CanonIdentityMO1Sigma = 0.18463902186135914;
const double CanonIdentityMO2 = 2.7132465417719;
const double CanonIdentityMO2Sigma = 0.18463902186055506;
const double CanonSd2MO1 = 2.555338250004908;
const double CanonSd2MO1Sigma = 0.07246835972150631;
const double CanonSd2MO2 = 2.4986465424511284;
const double CanonSd2MO2Sigma = 0.07007825117627481;
const double CanonSd2CrossCheckSigma = 0.5623628195265302;
const double CanonFreeGapIdentity = 2.550880306794233;    // exact analytic (block spectrum), identity, O1 = O2
const double CanonFreeGapSd2O1 = 2.532028376864343;       // exact analytic, sd2, O1
const double CanonFreeGapSd2O2 = 2.3569501678347393;      // exact analytic, sd2, O2

// --- Pre-registered derived values (computed FROM the pinned record values
// via the phase452 pre-registered rules; re-derived and re-checked here on
// every pass). ---------------------------------------------------------------
const double CanonSd2Combined = 2.526042101792096;        // rho=1 inverse-variance combination of sd2 mO1/mO2
const double CanonSd2CombinedSigma = 0.07123324126755279; // s1 s2 (s1+s2)/(s1^2+s2^2), correlation-conservative
const double CanonIdentityCombined = 2.713246541771112;   // same rule on the identity member (O2 = O1 exactly)
const double CanonIdentityCombinedSigma = 0.1846390218609571;
const double CanonCrossActionRatio = 0.9310035276570129;  // sd2 combined / identity combined
const double CanonCrossActionRatioSigma = 0.06857994094256506;
const double CanonFreeCrossActionRatio = 0.9926096375907257; // sd2 free O1 gap / identity free gap
const string CanonSd2CombinedReasonFragment = "2.5260 +- 0.0712";
const string CanonIdentityReasonFragment = "2.7132 +- 0.1846";
const string CanonTerminalStatus = "scalar-channel-spectroscopy-probe-passed-scalar-channel-gapped-measured-workbench-relative-no-gev";
const string CanonChannelVerdict = "scalar-channel-gapped-measured";

// --- Pre-registered tolerances and label conditions. -------------------------
const double TightTol = 1e-9;                 // charge: pinned stored values to 1e-9
const double CoarseRatioTol = 1e-3;           // the human-readable 0.931 statement
const double FreeCompatibleSigmaMax = 1.5;    // FREE-FIELD-COMPATIBLE label condition (measured: ~0.90 sigma)
const double CrossCheckSigmaGate = 3.0;       // phase452 pre-registered O1-vs-O2 compatibility gate

// --- Superseded values (tamper protection: must be ABSENT from the phase452
// output JSONs; they described a never-committed env-override run). -----------
const string SupersededGapA = "2.4352";
const string SupersededGapB = "2.4547";

// --- Pre-registered verdict taxonomy (two terminals, fail-closed). -----------
const string VerdictCanonical = "record-reconciled-canonical";
const string VerdictQuarantine = "config-mismatch-quarantine";

var stopwatch = Stopwatch.StartNew();
var checks = new List<AttestationCheck>();

void Check(string id, string requirement, Func<(bool Ok, string Observed)> evaluate)
{
    bool ok;
    string observed;
    try
    {
        (ok, observed) = evaluate();
    }
    catch (Exception ex)
    {
        ok = false;
        observed = $"structure error: {ex.GetType().Name}: {ex.Message}";
    }
    checks.Add(new AttestationCheck(id, requirement, ok, observed));
}

// ---------------------------------------------------------------------------
// Load the three attested artifacts (fail-closed: unreadable => named check
// failure => config-mismatch-quarantine; never a crash).
// ---------------------------------------------------------------------------
JsonDocument? summaryDoc = null;
string summaryText = "";
string summaryLoadError = "";
try
{
    summaryText = File.ReadAllText(Phase452SummaryPath);
    summaryDoc = JsonDocument.Parse(summaryText);
}
catch (Exception ex)
{
    summaryLoadError = $"{ex.GetType().Name}: {ex.Message}";
}

string fullText = "";
string fullLoadError = "";
try
{
    fullText = File.ReadAllText(Phase452FullOutputPath);
    using var probeParse = JsonDocument.Parse(fullText); // parse-validated, structurally read via the summary
}
catch (Exception ex)
{
    fullLoadError = $"{ex.GetType().Name}: {ex.Message}";
}

string programSource = "";
string programSourceLoadError = "";
try
{
    programSource = File.ReadAllText(Phase452ProgramSourcePath);
}
catch (Exception ex)
{
    programSourceLoadError = $"{ex.GetType().Name}: {ex.Message}";
}

Check("phase452-summary-readable",
    $"the committed phase452 summary JSON exists and parses ({Phase452SummaryPath})",
    () => (summaryDoc is not null, summaryDoc is not null ? "parsed" : summaryLoadError));
Check("phase452-full-output-readable",
    $"the committed phase452 full output JSON exists and parses ({Phase452FullOutputPath})",
    () => (fullLoadError.Length == 0 && fullText.Length > 0, fullLoadError.Length == 0 ? "parsed" : fullLoadError));
Check("phase452-program-source-readable",
    $"the committed phase452 Program.cs exists ({Phase452ProgramSourcePath})",
    () => (programSourceLoadError.Length == 0 && programSource.Length > 0, programSourceLoadError.Length == 0 ? "read" : programSourceLoadError));

// ---------------------------------------------------------------------------
// Structured checks against the summary record.
// ---------------------------------------------------------------------------
if (summaryDoc is not null)
{
    var root = summaryDoc.RootElement;

    JsonElement Ensemble() => root.GetProperty("probeConfiguration").GetProperty("ensemble");
    JsonElement Torus() => root.GetProperty("tori")[0];
    JsonElement MemberVerdict(string member)
    {
        foreach (var v in Torus().GetProperty("verdicts").EnumerateArray())
            if (v.GetProperty("member").GetString() == member)
                return v;
        throw new InvalidOperationException($"member verdict '{member}' not found");
    }
    JsonElement MemberSpectrum(string member)
    {
        foreach (var s in Torus().GetProperty("spectra").EnumerateArray())
            if (s.GetProperty("member").GetString() == member)
                return s;
        throw new InvalidOperationException($"member spectrum '{member}' not found");
    }

    // --- (1) canonical committed configuration. -----------------------------
    Check("config-traj-production-committed",
        $"probeConfiguration.ensemble.trajProduction == {CanonTrajProduction}",
        () =>
        {
            int v = Ensemble().GetProperty("trajProduction").GetInt32();
            return (v == CanonTrajProduction, Inv($"trajProduction={v}"));
        });
    Check("config-traj-control-committed",
        $"probeConfiguration.ensemble.trajControl == {CanonTrajControl}",
        () =>
        {
            int v = Ensemble().GetProperty("trajControl").GetInt32();
            return (v == CanonTrajControl, Inv($"trajControl={v}"));
        });
    Check("config-rng-seed-committed",
        $"probeConfiguration.rngSeed == {CanonRngSeed}",
        () =>
        {
            int v = root.GetProperty("probeConfiguration").GetProperty("rngSeed").GetInt32();
            return (v == CanonRngSeed, Inv($"rngSeed={v}"));
        });
    Check("config-production-columns-trajectories",
        $"every kind=production column records trajectories == {CanonTrajProduction}",
        () =>
        {
            var rows = new List<string>();
            bool ok = true;
            foreach (var c in Torus().GetProperty("columns").EnumerateArray())
            {
                if (c.GetProperty("kind").GetString() != "production")
                    continue;
                int t = c.GetProperty("trajectories").GetInt32();
                ok &= t == CanonTrajProduction;
                rows.Add(Inv($"{c.GetProperty("member").GetString()}:{t}"));
            }
            ok &= rows.Count == 2;
            return (ok, string.Join(", ", rows));
        });
    Check("config-control-columns-trajectories",
        $"every control column (free-field-control, higher-beta-control) records trajectories == {CanonTrajControl}",
        () =>
        {
            var rows = new List<string>();
            bool ok = true;
            foreach (var c in Torus().GetProperty("columns").EnumerateArray())
            {
                var kind = c.GetProperty("kind").GetString();
                if (kind is not ("free-field-control" or "higher-beta-control"))
                    continue;
                int t = c.GetProperty("trajectories").GetInt32();
                ok &= t == CanonTrajControl;
                rows.Add(Inv($"{c.GetProperty("member").GetString()}/{kind}:{t}"));
            }
            ok &= rows.Count == 3;
            return (ok, string.Join(", ", rows));
        });

    // --- (2) canonical record structure + gap values (tight tolerance). -----
    Check("record-single-n3-torus",
        $"exactly one torus record with torusN == {CanonTorusN}",
        () =>
        {
            int count = root.GetProperty("tori").GetArrayLength();
            int n = Torus().GetProperty("torusN").GetInt32();
            return (count == 1 && n == CanonTorusN, Inv($"tori={count}, torusN={n}"));
        });
    Check("record-window-single-point",
        "window == {0} and informativePoints == {0} and windowIncludesT0 == true (T=3: exactly one informative cosh point)",
        () =>
        {
            var w = Torus().GetProperty("window");
            var ip = Torus().GetProperty("informativePoints");
            bool ok = w.GetArrayLength() == 1 && w[0].GetInt32() == 0
                && ip.GetArrayLength() == 1 && ip[0].GetInt32() == 0
                && Torus().GetProperty("windowIncludesT0").GetBoolean();
            return (ok, Inv($"window.len={w.GetArrayLength()}, window[0]={w[0].GetInt32()}, informativePoints.len={ip.GetArrayLength()}"));
        });
    Check("record-identity-gap-o1",
        Inv($"identity mO1 == {CanonIdentityMO1:R} +- {CanonIdentityMO1Sigma:R} (|diff| < {TightTol:R})"),
        () =>
        {
            var v = MemberVerdict("identity");
            double m = v.GetProperty("mO1").GetDouble();
            double s = v.GetProperty("mO1Sigma").GetDouble();
            return (Close(m, CanonIdentityMO1) && Close(s, CanonIdentityMO1Sigma), Inv($"mO1={m:R}, mO1Sigma={s:R}"));
        });
    Check("record-identity-gap-o2",
        Inv($"identity mO2 == {CanonIdentityMO2:R} +- {CanonIdentityMO2Sigma:R} (|diff| < {TightTol:R})"),
        () =>
        {
            var v = MemberVerdict("identity");
            double m = v.GetProperty("mO2").GetDouble();
            double s = v.GetProperty("mO2Sigma").GetDouble();
            return (Close(m, CanonIdentityMO2) && Close(s, CanonIdentityMO2Sigma), Inv($"mO2={m:R}, mO2Sigma={s:R}"));
        });
    Check("record-identity-plateau-inconclusive-by-construction",
        "identity plateauChi2DofO1 and plateauChi2DofO2 are null (single-point window: inconclusive-by-construction as a measurement)",
        () =>
        {
            var v = MemberVerdict("identity");
            bool ok = v.GetProperty("plateauChi2DofO1").ValueKind == JsonValueKind.Null
                && v.GetProperty("plateauChi2DofO2").ValueKind == JsonValueKind.Null;
            return (ok, $"plateauChi2DofO1={v.GetProperty("plateauChi2DofO1").ValueKind}, plateauChi2DofO2={v.GetProperty("plateauChi2DofO2").ValueKind}");
        });
    Check("record-identity-verdict-reason",
        $"identity verdict == '{CanonChannelVerdict}', isControl == true, statisticsGatesPassed == true, reason quotes '{CanonIdentityReasonFragment}'",
        () =>
        {
            var v = MemberVerdict("identity");
            string reason = v.GetProperty("reason").GetString() ?? "";
            bool ok = v.GetProperty("verdict").GetString() == CanonChannelVerdict
                && v.GetProperty("isControl").GetBoolean()
                && v.GetProperty("statisticsGatesPassed").GetBoolean()
                && reason.Contains(CanonIdentityReasonFragment, StringComparison.Ordinal);
            return (ok, $"verdict={v.GetProperty("verdict").GetString()}, reason='{reason}'");
        });
    Check("record-sd2-gap-o1",
        Inv($"sd2 mO1 == {CanonSd2MO1:R} +- {CanonSd2MO1Sigma:R} (|diff| < {TightTol:R})"),
        () =>
        {
            var v = MemberVerdict("sd2-id0/c0.5");
            double m = v.GetProperty("mO1").GetDouble();
            double s = v.GetProperty("mO1Sigma").GetDouble();
            return (Close(m, CanonSd2MO1) && Close(s, CanonSd2MO1Sigma), Inv($"mO1={m:R}, mO1Sigma={s:R}"));
        });
    Check("record-sd2-gap-o2",
        Inv($"sd2 mO2 == {CanonSd2MO2:R} +- {CanonSd2MO2Sigma:R} (|diff| < {TightTol:R})"),
        () =>
        {
            var v = MemberVerdict("sd2-id0/c0.5");
            double m = v.GetProperty("mO2").GetDouble();
            double s = v.GetProperty("mO2Sigma").GetDouble();
            return (Close(m, CanonSd2MO2) && Close(s, CanonSd2MO2Sigma), Inv($"mO2={m:R}, mO2Sigma={s:R}"));
        });
    Check("record-sd2-combined-gap",
        Inv($"sd2 combined gap, recomputed via the phase452 pre-registered rho=1 rule from the stored mO1/mO2, == {CanonSd2Combined:R} +- {CanonSd2CombinedSigma:R} (|diff| < {TightTol:R}); verdict reason quotes '{CanonSd2CombinedReasonFragment}'"),
        () =>
        {
            var v = MemberVerdict("sd2-id0/c0.5");
            var (m, s) = CombineRho1(
                v.GetProperty("mO1").GetDouble(), v.GetProperty("mO1Sigma").GetDouble(),
                v.GetProperty("mO2").GetDouble(), v.GetProperty("mO2Sigma").GetDouble());
            string reason = v.GetProperty("reason").GetString() ?? "";
            bool ok = Close(m, CanonSd2Combined) && Close(s, CanonSd2CombinedSigma)
                && v.GetProperty("verdict").GetString() == CanonChannelVerdict
                && !v.GetProperty("isControl").GetBoolean()
                && v.GetProperty("statisticsGatesPassed").GetBoolean()
                && reason.Contains(CanonSd2CombinedReasonFragment, StringComparison.Ordinal);
            return (ok, Inv($"combined={m:R} +- {s:R}, reason='{reason}'"));
        });
    Check("record-sd2-interpolator-cross-check",
        Inv($"sd2 stored crossCheckSigma == {CanonSd2CrossCheckSigma:R} (|diff| < {TightTol:R}), matches its recomputation from the stored gaps, crossCheckCompatible == true, and sits below the pre-registered {CrossCheckSigmaGate} sigma gate (the 0.56-sigma cross-check)"),
        () =>
        {
            var v = MemberVerdict("sd2-id0/c0.5");
            double stored = v.GetProperty("crossCheckSigma").GetDouble();
            double m1 = v.GetProperty("mO1").GetDouble();
            double s1 = v.GetProperty("mO1Sigma").GetDouble();
            double m2 = v.GetProperty("mO2").GetDouble();
            double s2 = v.GetProperty("mO2Sigma").GetDouble();
            double recomputed = Math.Abs(m1 - m2) / Math.Sqrt((s1 * s1) + (s2 * s2));
            bool ok = Close(stored, CanonSd2CrossCheckSigma)
                && Close(recomputed, stored)
                && v.GetProperty("crossCheckCompatible").GetBoolean()
                && stored < CrossCheckSigmaGate;
            return (ok, Inv($"stored={stored:R}, recomputed={recomputed:R}"));
        });
    Check("record-exact-free-gaps",
        Inv($"exact analytic free gaps pinned: identity O1 == O2 == {CanonFreeGapIdentity:R}; sd2 O1 == {CanonFreeGapSd2O1:R}, O2 == {CanonFreeGapSd2O2:R} (|diff| < {TightTol:R})"),
        () =>
        {
            var id = MemberSpectrum("identity");
            var sd = MemberSpectrum("sd2-id0/c0.5");
            double idO1 = id.GetProperty("analyticMeffO1").GetDouble();
            double idO2 = id.GetProperty("analyticMeffO2").GetDouble();
            double sdO1 = sd.GetProperty("analyticMeffO1").GetDouble();
            double sdO2 = sd.GetProperty("analyticMeffO2").GetDouble();
            bool ok = Close(idO1, CanonFreeGapIdentity) && Close(idO2, CanonFreeGapIdentity)
                && Close(sdO1, CanonFreeGapSd2O1) && Close(sdO2, CanonFreeGapSd2O2);
            return (ok, Inv($"identity O1={idO1:R} O2={idO2:R}; sd2 O1={sdO1:R} O2={sdO2:R}"));
        });

    // --- (3) derived cross-action ratio + FREE-FIELD-COMPATIBLE label. ------
    Check("record-cross-action-ratio",
        Inv($"cross-action ratio sd2/identity, computed from the two stored combined gaps, == {CanonCrossActionRatio:R} +- {CanonCrossActionRatioSigma:R} (|diff| < {TightTol:R}) and equals 0.931 within {CoarseRatioTol:R} (CROSS-ACTION deliverable class, never folded into the spectrum-ratio table)"),
        () =>
        {
            var id = MemberVerdict("identity");
            var sd = MemberVerdict("sd2-id0/c0.5");
            var (idM, idS) = CombineRho1(
                id.GetProperty("mO1").GetDouble(), id.GetProperty("mO1Sigma").GetDouble(),
                id.GetProperty("mO2").GetDouble(), id.GetProperty("mO2Sigma").GetDouble());
            var (sdM, sdS) = CombineRho1(
                sd.GetProperty("mO1").GetDouble(), sd.GetProperty("mO1Sigma").GetDouble(),
                sd.GetProperty("mO2").GetDouble(), sd.GetProperty("mO2Sigma").GetDouble());
            double ratio = sdM / idM;
            double ratioSigma = Math.Sqrt(((sdS / idM) * (sdS / idM)) + ((sdM * idS / (idM * idM)) * (sdM * idS / (idM * idM))));
            bool ok = Close(idM, CanonIdentityCombined) && Close(idS, CanonIdentityCombinedSigma)
                && Close(ratio, CanonCrossActionRatio) && Close(ratioSigma, CanonCrossActionRatioSigma)
                && Math.Abs(ratio - 0.931) < CoarseRatioTol;
            return (ok, Inv($"ratio={ratio:R} +- {ratioSigma:R} (identity combined {idM:R} +- {idS:R})"));
        });
    Check("record-free-field-compatible-label",
        Inv($"the measured cross-action ratio sits within {FreeCompatibleSigmaMax} sigma of the exact free ratio {CanonFreeCrossActionRatio:R} (= sd2 free O1 gap / identity free gap, recomputed and pinned to {TightTol:R}) - the binding FREE-FIELD-COMPATIBLE label condition, sigma recomputed from the stored errors"),
        () =>
        {
            var idSpec = MemberSpectrum("identity");
            var sdSpec = MemberSpectrum("sd2-id0/c0.5");
            double freeRatio = sdSpec.GetProperty("analyticMeffO1").GetDouble() / idSpec.GetProperty("analyticMeffO1").GetDouble();
            var id = MemberVerdict("identity");
            var sd = MemberVerdict("sd2-id0/c0.5");
            var (idM, idS) = CombineRho1(
                id.GetProperty("mO1").GetDouble(), id.GetProperty("mO1Sigma").GetDouble(),
                id.GetProperty("mO2").GetDouble(), id.GetProperty("mO2Sigma").GetDouble());
            var (sdM, sdS) = CombineRho1(
                sd.GetProperty("mO1").GetDouble(), sd.GetProperty("mO1Sigma").GetDouble(),
                sd.GetProperty("mO2").GetDouble(), sd.GetProperty("mO2Sigma").GetDouble());
            double ratio = sdM / idM;
            double ratioSigma = Math.Sqrt(((sdS / idM) * (sdS / idM)) + ((sdM * idS / (idM * idM)) * (sdM * idS / (idM * idM))));
            double sigmaFromFree = Math.Abs(ratio - freeRatio) / ratioSigma;
            bool ok = Close(freeRatio, CanonFreeCrossActionRatio)
                && sigmaFromFree <= FreeCompatibleSigmaMax;
            return (ok, Inv($"freeRatio={freeRatio:R}, sigmaFromFree={sigmaFromFree:R}"));
        });

    // --- (5) standing claim boundary on the canonical record. ---------------
    Check("record-terminal-status-canonical",
        $"terminalStatus == '{CanonTerminalStatus}', scalarChannelVerdict == '{CanonChannelVerdict}', scalarChannelSpectroscopyProbePassed == true, batteries.batteriesAllPassed == true",
        () =>
        {
            bool ok = root.GetProperty("terminalStatus").GetString() == CanonTerminalStatus
                && root.GetProperty("scalarChannelVerdict").GetString() == CanonChannelVerdict
                && root.GetProperty("scalarChannelSpectroscopyProbePassed").GetBoolean()
                && root.GetProperty("batteries").GetProperty("batteriesAllPassed").GetBoolean();
            return (ok, $"terminalStatus={root.GetProperty("terminalStatus").GetString()}");
        });
    Check("boundary-target-blind",
        "targetBlindConstruction == true and physicalTargetsConsultedForConstruction == false",
        () =>
        {
            bool tb = root.GetProperty("targetBlindConstruction").GetBoolean();
            bool pt = root.GetProperty("physicalTargetsConsultedForConstruction").GetBoolean();
            return (tb && !pt, $"targetBlindConstruction={tb}, physicalTargetsConsultedForConstruction={pt}");
        });
    Check("boundary-no-contract-fills",
        "no contract fills: sourceContractApplicationAllowed/phase201TemplateMutated false; fieldsAppliedToPhase201TemplateCount/acceptedContractFieldCount 0; every canFill* field false",
        () =>
        {
            bool ok = !root.GetProperty("sourceContractApplicationAllowed").GetBoolean()
                && !root.GetProperty("phase201TemplateMutated").GetBoolean()
                && root.GetProperty("fieldsAppliedToPhase201TemplateCount").GetInt32() == 0
                && root.GetProperty("acceptedContractFieldCount").GetInt32() == 0
                && !root.GetProperty("canFillPhase201WzContract").GetBoolean()
                && !root.GetProperty("canFillPhase201HiggsContract").GetBoolean()
                && !root.GetProperty("canFillPhase256Contract").GetBoolean()
                && !root.GetProperty("canFillPhase256ObservedFieldExtractionContract").GetBoolean();
            return (ok, ok ? "all contract-fill fields at their fail-closed values" : "a contract-fill field departed from its fail-closed value");
        });
    Check("boundary-no-promotions",
        "no promotions: every route*/physicalCouplingProvided field false; noGevPromotion and scaleIsWorkbenchRelativeCandidateOnly true (the promotedPhysicalMassClaimCount-class booleans)",
        () =>
        {
            bool ok = !root.GetProperty("physicalCouplingProvided").GetBoolean()
                && !root.GetProperty("routeProvidesPhysicalEffectiveActionHessian").GetBoolean()
                && !root.GetProperty("routeProvidesVevOrSourceScaleLineage").GetBoolean()
                && !root.GetProperty("routeProvidesPoleExtractionAndGeVNormalization").GetBoolean()
                && !root.GetProperty("routeCompletesBosonPredictions").GetBoolean()
                && !root.GetProperty("routePromotesWzMasses").GetBoolean()
                && !root.GetProperty("routePromotesHiggsMass").GetBoolean()
                && root.GetProperty("noGevPromotion").GetBoolean()
                && root.GetProperty("scaleIsWorkbenchRelativeCandidateOnly").GetBoolean();
            return (ok, ok ? "all promotion fields at their fail-closed values" : "a promotion field departed from its fail-closed value");
        });
    Check("boundary-physicist-review-pending",
        "physicistReviewPending == true at root and in recordedBoundary (Wave-0 item 0.3 OPEN, carried explicitly)",
        () =>
        {
            bool a = root.GetProperty("physicistReviewPending").GetBoolean();
            bool b = root.GetProperty("recordedBoundary").GetProperty("physicistReviewPending").GetBoolean();
            return (a && b, $"root={a}, recordedBoundary={b}");
        });
    Check("boundary-lattice-units-language",
        "lattice-units-only language: pureGaugeRatioNote carries 'lattice units' and the binding 'NEVER m_H' label caveat; the WS2 binding conditions (coshCorrectedEffectiveMassesOnly, thetaHaarMeasureUsed, theta0SliceIsSamplerDemoOnly) all true",
        () =>
        {
            string note = root.GetProperty("pureGaugeRatioNote").GetString() ?? "";
            bool ok = note.Contains("lattice units", StringComparison.Ordinal)
                && note.Contains("NEVER m_H", StringComparison.Ordinal)
                && root.GetProperty("coshCorrectedEffectiveMassesOnly").GetBoolean()
                && root.GetProperty("thetaHaarMeasureUsed").GetBoolean()
                && root.GetProperty("theta0SliceIsSamplerDemoOnly").GetBoolean();
            return (ok, ok ? "label caveat and binding-condition flags intact" : "label caveat or a binding-condition flag missing");
        });
}

// ---------------------------------------------------------------------------
// (1b) committed phase452 Program.cs default literals (phase202-style
// const + parse: the committed source must still carry the canonical
// defaults, so a source edit can never silently move the record).
// ---------------------------------------------------------------------------
if (programSource.Length > 0)
{
    Check("config-source-default-traj-production",
        Inv($"phase452 Program.cs PHASE452_TRAJ default literal == {CanonTrajProduction}"),
        () =>
        {
            var m = Regex.Match(programSource, "GetEnvironmentVariable\\(\"PHASE452_TRAJ\"\\),\\s*out\\s+int\\s+\\w+\\)\\s*\\?\\s*\\w+\\s*:\\s*(\\d+)");
            return (m.Success && m.Groups[1].Value == CanonTrajProduction.ToString(CultureInfo.InvariantCulture),
                m.Success ? $"default literal {m.Groups[1].Value}" : "default literal not found");
        });
    Check("config-source-default-traj-control",
        Inv($"phase452 Program.cs PHASE452_CTRL_TRAJ default literal == {CanonTrajControl}"),
        () =>
        {
            var m = Regex.Match(programSource, "GetEnvironmentVariable\\(\"PHASE452_CTRL_TRAJ\"\\),\\s*out\\s+int\\s+\\w+\\)\\s*\\?\\s*\\w+\\s*:\\s*(\\d+)");
            return (m.Success && m.Groups[1].Value == CanonTrajControl.ToString(CultureInfo.InvariantCulture),
                m.Success ? $"default literal {m.Groups[1].Value}" : "default literal not found");
        });
    Check("config-source-rng-seed-const",
        Inv($"phase452 Program.cs 'const int RngSeed' literal == {CanonRngSeed}"),
        () =>
        {
            var m = Regex.Match(programSource, "const\\s+int\\s+RngSeed\\s*=\\s*(\\d+)\\s*;");
            return (m.Success && m.Groups[1].Value == CanonRngSeed.ToString(CultureInfo.InvariantCulture),
                m.Success ? $"const literal {m.Groups[1].Value}" : "const literal not found");
        });
}
else
{
    Check("config-source-default-traj-production", "phase452 Program.cs readable for literal parse", () => (false, "source unavailable"));
    Check("config-source-default-traj-control", "phase452 Program.cs readable for literal parse", () => (false, "source unavailable"));
    Check("config-source-rng-seed-const", "phase452 Program.cs readable for literal parse", () => (false, "source unavailable"));
}

// ---------------------------------------------------------------------------
// (4) tamper protection: the superseded (never-committed env-override run)
// gap values must be ABSENT from both phase452 output JSONs. Volatile fields
// (generatedAt, runtimeSeconds, msPerTrajectory) are scrubbed first because
// the generator regenerates the outputs each pass and a timing digit string
// must never be able to flip this check either way.
// ---------------------------------------------------------------------------
Check("tamper-superseded-values-absent",
    $"the superseded gap literals '{SupersededGapA}' and '{SupersededGapB}' appear NOWHERE in the phase452 output JSONs (volatile-field-scrubbed)",
    () =>
    {
        if (summaryText.Length == 0 || fullText.Length == 0)
            return (false, "output text unavailable");
        string scrubbed = ScrubVolatile(summaryText) + "\n" + ScrubVolatile(fullText);
        bool hasA = scrubbed.Contains(SupersededGapA, StringComparison.Ordinal);
        bool hasB = scrubbed.Contains(SupersededGapB, StringComparison.Ordinal);
        return (!hasA && !hasB, $"contains {SupersededGapA}: {hasA}; contains {SupersededGapB}: {hasB}");
    });

// ---------------------------------------------------------------------------
// Verdict (pre-registered two-terminal taxonomy, fail-closed).
// ---------------------------------------------------------------------------
var failingChecks = checks.Where(c => !c.Passed).Select(c => c.Id).ToArray();
string verdictKind = failingChecks.Length == 0 ? VerdictCanonical : VerdictQuarantine;
string verdictPhrase = failingChecks.Length == 0
    ? "every pre-registered attestation check is green: the committed phase452 output IS the Wave-0-reconciled canonical record (committed 16000/10000/seed-20260705 configuration equal to the committed source defaults; canonical gap values, derived cross-action ratio, and FREE-FIELD-COMPATIBLE label condition all reproduced to tolerance; superseded values absent; claim boundary intact). Every spectroscopy citation program-wide may rely on the canonical numbers."
    : $"ATTESTATION FAILED on {failingChecks.Length} named check(s): [{string.Join(", ", failingChecks)}]. Per the committed rule-out branch (Wave-0 item 0.2), phase452 is demoted to unverified-output and ALL spectroscopy rows are quarantined program-wide; a mandatory --full re-run (and, if the mismatch survives it, a record investigation) is required before any pole citation.";

// ---------------------------------------------------------------------------
// Framing + fail-closed boundary of THIS phase (standard keys, verbatim).
// ---------------------------------------------------------------------------
const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool physicistReviewPending = true;   // Wave-0 item 0.3 OPEN (explicit)
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
const bool envKnobsRead = false;            // env-clean by construction: this phase reads no environment variables

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join(
    "|",
    ApplicationSubjectKind,
    "phase452 record-reconciliation attestation: canonical committed configuration (16000/10000/seed 20260705) asserted against output AND source defaults; pinned canonical gap record to 1e-9; rho=1 combination and cross-action ratio re-derived from stored values; FREE-FIELD-COMPATIBLE label condition re-checked; superseded 2.4352/2.4547 absence tamper scan on volatile-scrubbed text; standing claim boundary re-asserted; two-terminal fail-closed taxonomy {record-reconciled-canonical, config-mismatch-quarantine}; zero physics compute; no target values")))).ToLowerInvariant();

bool spectroscopyRecordAttestationPassed =
    failingChecks.Length == 0 &&
    scaleIsWorkbenchRelativeCandidateOnly && noGevPromotion && physicistReviewPending &&
    definition81Scope == "reduced-spin4-slice" &&
    !ambientSevenSevenRealized && !internalGaugeContentRealized && !weldRealized &&
    targetBlindConstruction && !physicalTargetsConsultedForConstruction &&
    !physicalCouplingProvided && !routeProvidesPhysicalEffectiveActionHessian &&
    !routeProvidesVevOrSourceScaleLineage && !routeProvidesPoleExtractionAndGeVNormalization &&
    !routeCompletesBosonPredictions && !routePromotesWzMasses && !routePromotesHiggsMass &&
    !sourceContractApplicationAllowed && !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 && acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract && !canFillPhase201HiggsContract &&
    !canFillPhase256Contract && !canFillPhase256ObservedFieldExtractionContract &&
    !envKnobsRead;

string terminalStatus = spectroscopyRecordAttestationPassed
    ? TerminalPrefix + "passed-record-reconciled-canonical"
    : TerminalPrefix + "config-mismatch-quarantine-review-required";

string decision = spectroscopyRecordAttestationPassed
    ? "The phase452 record-reconciliation attestation is decided on machine checks against the committed record (Team A rank-1 A0, Wave-0 item 0.2; zero physics compute). "
      + Inv($"ATTESTED: the committed phase452 output carries the canonical configuration (trajProduction={CanonTrajProduction}, trajControl={CanonTrajControl}, rngSeed={CanonRngSeed}), equal to the committed phase452 Program.cs default literals (parsed from source); the canonical gap record holds to {TightTol:R} (identity a*m = 2.7132 +- 0.1846 with plateauChi2Dof null / window {{0}} recorded as inconclusive-by-construction as a measurement; sd2 combined 2.5260 +- 0.0712 re-derived via the pre-registered rho=1 rule from the stored per-interpolator gaps, interpolator cross-check 0.56 sigma; exact analytic free gaps 2.5509 / 2.5320 / 2.3570); the derived cross-action ratio sd2/identity computed from the stored gaps is 0.931 +- 0.069 (CROSS-ACTION deliverable class, never folded into the spectrum-ratio table) and sits ~0.90 sigma from the exact free ratio 0.9926 - within the {FreeCompatibleSigmaMax}-sigma FREE-FIELD-COMPATIBLE label condition, which therefore REMAINS BINDING on every one of these rows; the superseded never-committed values 2.4352 / 2.4547 appear nowhere in the output JSONs (tamper scan on volatile-scrubbed text); and the standing claim boundary is intact (target-blind, no contract fills, no promotions, physicistReviewPending carried, lattice-units-only language with the binding label caveat). ")
      + $"VERDICT ({verdictKind}): {verdictPhrase} "
      + "MANDATORY FRAMING: this attestation performs zero physics computation and promotes nothing; the attested numbers are workbench-relative structure data of the reduced spin-4 slice (su(2) toy algebra, lattice-canonical n=3 torus, lattice units, beta = 1 convention recorded), NEVER physical masses; NO GeV/pole/VEV promotion; no Phase201/Phase256 contract field is filled."
    : $"VERDICT ({verdictKind}): {verdictPhrase} Do not cite any phase452 spectroscopy number until the attestation is green again; failed runs and mismatch records are first-class artifacts and are preserved.";

// ---------------------------------------------------------------------------
// Serialize + write (always writes the verdict; always exits 0 - the
// integrity verifier fail-closes the pass on the wrong verdict).
// ---------------------------------------------------------------------------
Directory.CreateDirectory(DefaultOutputDir);
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

var result = new
{
    phaseId = "phase459-spectroscopy-record-attestation",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    spectroscopyRecordAttestationPassed,
    applicationSubjectKind = ApplicationSubjectKind,

    verdictKind,
    verdictPhrase,
    verdictTaxonomy = new
    {
        recordReconciledCanonical = "record-reconciled-canonical: ALL pre-registered checks green - the committed phase452 output is attested as the sole canonical spectroscopy record and program-wide citations may rely on it",
        configMismatchQuarantine = "config-mismatch-quarantine: ANY check fails (each failing check named in failingChecks) - phase452 is demoted to unverified-output, all spectroscopy rows are quarantined program-wide, and a mandatory --full re-run precedes any pole citation",
        failClosedRule = "the phase always writes its verdict and exits 0; scripts/verify_boson_claim_integrity.sh asserts verdictKind == record-reconciled-canonical and fail-closes the whole pass otherwise",
    },
    checkCount = checks.Count,
    passedCheckCount = checks.Count(c => c.Passed),
    failingChecks,
    checks = checks.Select(c => new { id = c.Id, requirement = c.Requirement, passed = c.Passed, observed = c.Observed }).ToArray(),

    canonicalRecord = new
    {
        note = "pinned VERBATIM from the committed phase452 summary at the Wave-0 reconciliation (item 0.2); tight tolerance " + TightTol.ToString("R", CultureInfo.InvariantCulture),
        configuration = new
        {
            trajProduction = CanonTrajProduction,
            trajControl = CanonTrajControl,
            rngSeed = CanonRngSeed,
            torusN = CanonTorusN,
        },
        identity = new
        {
            mO1 = CanonIdentityMO1,
            mO1Sigma = CanonIdentityMO1Sigma,
            mO2 = CanonIdentityMO2,
            mO2Sigma = CanonIdentityMO2Sigma,
            plateauChi2Dof = (object?)null,
            window = new[] { 0 },
            windowNote = "T = 3 admits exactly one informative cosh point: the plateau statistic is null and the window is {0} - inconclusive-by-construction as a measurement (upper-bound-flavored estimate), asserted as such",
            reasonFragment = CanonIdentityReasonFragment,
        },
        sd2 = new
        {
            mO1 = CanonSd2MO1,
            mO1Sigma = CanonSd2MO1Sigma,
            mO2 = CanonSd2MO2,
            mO2Sigma = CanonSd2MO2Sigma,
            crossCheckSigma = CanonSd2CrossCheckSigma,
            combined = CanonSd2Combined,
            combinedSigma = CanonSd2CombinedSigma,
            combinationRule = "phase452 pre-registered: inverse-variance weighted mean with the correlation-conservative rho=1 error s1 s2 (s1+s2)/(s1^2+s2^2)",
            reasonFragment = CanonSd2CombinedReasonFragment,
        },
        exactFreeGaps = new
        {
            identity = CanonFreeGapIdentity,
            sd2O1 = CanonFreeGapSd2O1,
            sd2O2 = CanonFreeGapSd2O2,
        },
        derived = new
        {
            identityCombined = CanonIdentityCombined,
            identityCombinedSigma = CanonIdentityCombinedSigma,
            crossActionRatio = CanonCrossActionRatio,
            crossActionRatioSigma = CanonCrossActionRatioSigma,
            crossActionNote = "CROSS-ACTION deliverable class (sd2 combined / identity combined), never folded into the spectrum-ratio table",
            freeCrossActionRatio = CanonFreeCrossActionRatio,
            freeCompatibleSigmaMax = FreeCompatibleSigmaMax,
            freeFieldCompatibleNote = "the measured cross-action ratio sits ~0.90 sigma from the exact free ratio: the FREE-FIELD-COMPATIBLE label REMAINS BINDING on all attested rows",
        },
        supersededValues = new
        {
            values = new[] { SupersededGapA, SupersededGapB },
            note = "described a never-committed reduced-budget env-override run; superseded by the Wave-0 reconciliation and asserted ABSENT from the phase452 output JSONs as standing tamper protection",
        },
    },

    volatilityRule = "asserted content is restricted to configuration, seeded-deterministic measurement values, structure, boundary flags, and prose; generatedAt / runtimeSeconds / msPerTrajectory are never read and are scrubbed before the tamper scan (the generator regenerates the phase452 outputs each pass)",
    envKnobsRead,
    zeroPhysicsCompute = true,

    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    physicistReviewPending,
    recordedBoundary = new
    {
        definition81Scope,
        ambientSevenSevenRealized,
        internalGaugeContentRealized,
        weldRealized,
        canFillPhase201WzContract,
        canFillPhase256Contract,
        physicistReviewPending,
        baseSignature = "Cl(4,0)-euclidean-slice",
        spinorUsedAsShiabCarrierNotFiber = true,
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
        canFillPhase201WzContract,
        canFillPhase201HiggsContract,
        canFillPhase256Contract,
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    sourceEvidence = new
    {
        phase452SummaryPath = Phase452SummaryPath,
        phase452FullOutputPath = Phase452FullOutputPath,
        phase452ProgramSourcePath = Phase452ProgramSourcePath,
        programSourcePath = ProgramSourcePath,
        registrySourcePath = RegistrySourcePath,
    },
    explicitCandidateOnlyNonclaims = new[]
    {
        "This phase performs ZERO physics computation: it attests a committed record and re-derives two numbers (the rho=1 combination and the cross-action ratio) from stored values. A green attestation adds no physics content of its own.",
        "Every attested gap is workbench-relative structure data of the reduced spin-4 slice (su(2) toy algebra, lattice-canonical n=3 torus, lattice units, beta = 1 convention recorded): NOT a physical mass, NOT a scale in GeV; the binding label caveat holds - no W/Z/H label attaches to anything here.",
        "The cross-action ratio 0.931 +- 0.069 is a CROSS-ACTION deliverable-class row and is never folded into the spectrum-ratio table; it carries the FREE-FIELD-COMPATIBLE label (~0.90 sigma from the exact free ratio), so no dynamical-structure claim attaches to it.",
        "The identity single-window gap is inconclusive-by-construction as a measurement (plateauChi2Dof null, window {0}); it is attested AS THAT, never as a validated plateau.",
        "physicistReviewPending = true is carried explicitly (Wave-0 item 0.3 OPEN): a convention overturned in review invalidates dependent verdicts fail-closed, including the record attested here.",
        "No Phase201 or Phase256 contract field is filled; nothing is promoted either way; promotedPhysicalMassClaimCount remains 0.",
    },
    decision,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(DefaultOutputDir, "spectroscopy_record_attestation.json"), JsonSerializer.Serialize(result, options));
string summaryOutPath = Path.Combine(DefaultOutputDir, "spectroscopy_record_attestation_summary.json");
File.WriteAllText(summaryOutPath, JsonSerializer.Serialize(result, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"spectroscopyRecordAttestationPassed={spectroscopyRecordAttestationPassed} verdictKind={verdictKind}");
Console.WriteLine(Inv($"checks: {checks.Count(c => c.Passed)}/{checks.Count} green; failing=[{string.Join(", ", failingChecks)}]"));
Console.WriteLine(Inv($"runtimeSeconds={runtimeSeconds:F2}"));
Console.WriteLine($"summaryPath={summaryOutPath}");

// ---------------------------------------------------------------------------
// Helpers.
// ---------------------------------------------------------------------------
static bool Close(double actual, double expected) => Math.Abs(actual - expected) < TightTol;

static (double M, double S) CombineRho1(double m1, double s1, double m2, double s2)
{
    double w1 = 1.0 / (s1 * s1);
    double w2 = 1.0 / (s2 * s2);
    double m = ((w1 * m1) + (w2 * m2)) / (w1 + w2);
    double s = s1 * s2 * (s1 + s2) / ((s1 * s1) + (s2 * s2));
    return (m, s);
}

static string ScrubVolatile(string jsonText)
{
    jsonText = Regex.Replace(jsonText, "\"generatedAt\"\\s*:\\s*\"[^\"]*\"", "\"generatedAt\":\"SCRUBBED\"");
    jsonText = Regex.Replace(jsonText, "\"(runtimeSeconds|msPerTrajectory)\"\\s*:\\s*[-+0-9.eE]+", "\"$1\":0");
    return jsonText;
}

static string Inv(FormattableString formattable) => FormattableString.Invariant(formattable);

sealed record AttestationCheck(string Id, string Requirement, bool Passed, string Observed);
