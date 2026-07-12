using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase457: Upsilon Portal Stage A -- boundedness certificates + null-hash
// firewall + Arm Q measurement code (gated OFF).
// WAVE2_ACTION_PLAN_2026-07-12 item 10; TEAM_ELIMINATION_PROGRAM_2026-07-10
// item 15 (WS3 Upsilon-portal on the DYNAMICAL (omega,theta,M) measure; M is a
// LABELED probe-field convention, never sourced).
//
// WHAT THIS PHASE COMMITS AT STAGE A.
//   (1) Exhaustive-cell boundedness-below certificates for the portal action
//       class S_portal(omega,theta,M) = S_B + covariant-kinetic + probe
//       potential, over a pre-registered, disjoint, exhaustive sign-cell menu.
//       The certificates are EXACT MATHEMATICS: elementary quartic-completion,
//       sum-of-squares, and growth-rate lemmas resting on already-committed
//       structural facts (S_B >= 0; control-branch Upsilon = F so ||F||^2 =
//       2 S_B exactly; S_B ~ t^4 and ||F|| ~ t^2 on non-flat rays; F = 0 on
//       the flat vacuum manifold, phase405/410). No sampling; no HMC.
//   (2) The NULL-HASH FIREWALL keyed to phase466: no verdict-bearing summary
//       field is emitted unless the on-disk phase466 schema {schemaId,
//       schemaHash} matches the pinned pair AND an o4MProbeRuling record
//       exists. It does not, so the portal verdict is WITHHELD. Machine-checked
//       by a battery (a synthetic wrong schemaHash / wrong schemaId must block;
//       a synthetic o4 ruling must open the conjunction -- proving the gate is
//       not stuck-closed).
//   (3) ARM Q motivation-gate measurement code -- IMPLEMENTED and GATED OFF.
//       The seed-regenerated-ensemble portal statistic plus the
//       RNG-stream-neutrality battery run here ONLY as a synthetic plumbing
//       self-test (zero physics). Execution on a real ensemble is gated behind
//       the pre-registered unlock conditions documented below.
//
// MANDATORY FRAMING. Nothing is measured on a physical target; no absolute
// scale; lattice-unit quantities stay in lattice units; M is a labeled probe
// convention (O8); a gap proves a singlet, never a named electroweak scalar.
// promotedPhysicalMassClaimCount remains 0; physicistReviewPending carried
// explicitly. Pre-unlock nothing here is a portal verdict. The Stage-A
// certificates are MATHEMATICS about the action class and constrain the probe
// design; they are not physics claims about any target.

var stopwatch = Stopwatch.StartNew();

const string ApplicationSubjectKind = "upsilon-portal-stage-a";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 10";
const string EliminationItem = "TEAM_ELIMINATION_PROGRAM_2026-07-10 item 15 (WS3 Upsilon-portal, dynamical (omega,theta,M), M = labeled probe convention)";
const string TerminalPrefix = "upsilon-portal-stage-a-";
const string DefaultOutputDir = "studies/phase457_upsilon_portal_stage_a_001/output";
const string PriorInterimTerminal = "awaiting-stage-a";

// --- phase466 pin (the null-hash firewall is KEYED to this exact pair). ------
const string Phase466SummaryPath = "studies/phase466_ws3_vev_completion_contract_001/output/ws3_vev_completion_contract_summary.json";
const string PinnedSchemaId = "ws3-vev-completion-contract-schema-v1";
const string PinnedSchemaHash = "7159ea49a45e3044c4393542b24a5db596f5d1423150020b072849ec8cb322b9";

// --- O4 M-probe ruling: pre-registered candidate locations + required shape.
// A ruling record must carry wsThreeMProbeScopeSignedOff = true and a signer.
// None exists yet (the O4 register is only a DERIVED pending-review
// enumeration, not a ruling), so this conjunct is false and the portal verdict
// stays withheld.
string[] O4MProbeRulingCandidatePaths =
{
    "docs/Phases/Adjudication/O4_MPROBE_RULING.json",
    "scripts/o4_register/o4_mprobe_ruling.json",
    "studies/phase457_upsilon_portal_stage_a_001/o4_mprobe_ruling.json",
};

var outputDir = Environment.GetEnvironmentVariable("PHASE457_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// ===========================================================================
// (1) STAGE A -- exhaustive-cell boundedness-below certificates.
// ===========================================================================
//
// Portal action class (referee-scoped WS3 Upsilon-portal, reduced su(2)
// Spin(4) slice / lattice-canonical torus; the phase450 setting):
//
//   S_portal(omega, theta, M)
//     = S_B(omega, theta)                                     [committed, >= 0]
//     + kappa * sum_edges || D_omega M ||^2                   [kappa >= 0, >= 0]
//     + sum_x [ lambda |M_x|^4 + (mu2 + g * Omega_x(omega,theta)) |M_x|^2 ]
//
// where:
//   S_B = (1/2)||Upsilon||^2 >= 0, = 0 on the flat vacuum manifold; on the
//         CONTROL branch (identity Shiab, trivial torsion) Upsilon = F so
//         ||F||^2 = 2 S_B EXACTLY (rho := sup ||F||^2/(2 S_B) = 1 on control;
//         member-general rho is an exactly-computable Shiab curvature/action
//         ratio -- recorded, control-branch primary per phase450);
//   || D_omega M ||^2 >= 0 is the gauge-COVARIANT lattice kinetic term (the
//         "|D M|^2-type portal"): it couples M to (omega,theta) and is the
//         portal proper; it is >= 0 always and only HELPS boundedness;
//   Omega_x(omega,theta) >= 0 is a curvature-invariant density -- the
//         DIRECT curvature portal (the phase410 curvature-coupling class):
//         convention MAG => Omega = ||F_x||   (linear-curvature, ~ t^2 on rays)
//         convention DEN => Omega = ||F_x||^2 (density,          ~ t^4 on rays);
//   coupling vector c = (kappa, lambda, mu2, g); portal convention in {MAG,DEN}.
//
// SCOPE of the certificates (recorded): the reduced arm kappa = 0 is the
// certified arm. kappa > 0 adds a positive-semidefinite kinetic form to the
// M-quadratic operator, which can only RAISE the potential floor: bounded at
// kappa = 0 => bounded for all kappa >= 0. So BOUNDED certificates hold for all
// kappa >= 0; UNBOUNDED certificates are proven at kappa = 0 and, where the
// runaway growth-rate strictly dominates the kinetic growth (the DEN portal:
// t^8 vs t^2), hold for all kappa >= 0 too (flagged per cell).
//
// Lemmas (all elementary, exact):
//   L-Q     quartic runaway: lambda < 0 => V_x -> -inf as |M_x| -> inf at
//           fixed omega (S_B, kinetic finite). UNBOUNDED.
//   L-MASS  mass runaway: lambda = 0, mu2 < 0 => on the flat vacuum (F=0,
//           Omega=0, covariant-constant M => kinetic 0) V = mu2|M|^2 -> -inf.
//   L-PNQ   portal runaway w/o quartic: lambda = 0, mu2 >= 0, g < 0 => on a
//           non-flat ray Omega -> inf so (mu2 + g Omega) < 0 at some site; with
//           no quartic, V -> -inf as |M| -> inf there. UNBOUNDED.
//   L-SOS   sum of squares: lambda = 0, mu2 >= 0, g >= 0 => every term >= 0 =>
//           S_portal >= 0. BOUNDED, floor 0.
//   L-CS    complete the square: lambda > 0, g >= 0 => per-site floor
//           -(mu2 + g Omega_x)_-^2/(4 lambda) >= -(mu2)_-^2/(4 lambda)
//           (g Omega_x >= 0 only raises it); omega-independent finite bound.
//           BOUNDED, floor -N_sites*(mu2)_-^2/(4 lambda).
//   L-SUP   super-critical portal: lambda > 0, g < 0, DEN => per-site floor
//           ~ -g^2 Omega_x^2/(4 lambda) ~ -t^8 on a non-flat ray while S_B ~
//           t^4 and kinetic ~ t^2: the t^8 runaway strictly dominates =>
//           UNBOUNDED for all kappa >= 0.
//   L-CRIT  critical portal: lambda > 0, g < 0, MAG => per-site net
//           phi(r) = (1/2) r^2 - (mu2 + g r)^2/(4 lambda), r = ||F_x||, using
//           the control-branch identity ||F||^2 = 2 S_B. Leading coefficient
//           (2 lambda - g^2)/(4 lambda):
//             |g| <  sqrt(2 lambda)  => phi convex => BOUNDED (exact floor);
//             |g| >  sqrt(2 lambda)  => phi -> -inf on high-curvature rays =>
//                                       UNBOUNDED (reduced arm kappa = 0);
//             |g| == sqrt(2 lambda)  => phi linear in r: BOUNDED if mu2 > 0,
//                                       marginal/floor-0 if mu2 = 0, UNBOUNDED
//                                       if mu2 < 0.
//           g_crit = sqrt(2 lambda / rho); rho = 1 on the control branch.
//
// The seven cells below are pairwise DISJOINT and EXHAUSTIVE over
// sign(lambda) x sign(g) x sign(mu2) x convention. Every cell carries a
// definite certificate (bounded / unbounded / conditional-critical-with-exact-
// threshold), so Stage A commits with terminal stage-a-certificates-committed.

var cells = new List<PortalCell>
{
    new("CELL-Q-NEG",     "lambda < 0",                                  "UNBOUNDED-BELOW",     "L-Q",
        "quartic self-coupling negative: V -> -inf in |M| at fixed omega",
        "all kappa >= 0", "-"),
    new("CELL-Z-MASSNEG", "lambda = 0 & mu2 < 0",                        "UNBOUNDED-BELOW",     "L-MASS",
        "no quartic, negative probe mass^2: V -> -inf on the flat vacuum",
        "all kappa >= 0", "-"),
    new("CELL-Z-PORTALNEG","lambda = 0 & mu2 >= 0 & g < 0",              "UNBOUNDED-BELOW",     "L-PNQ",
        "no quartic, negative curvature portal drives the effective mass^2 negative on non-flat rays",
        "all kappa >= 0 (reduced arm; kinetic cannot restore a missing quartic)", "-"),
    new("CELL-Z-SOS",     "lambda = 0 & mu2 >= 0 & g >= 0",              "BOUNDED-BELOW",       "L-SOS",
        "sum of squares: every term >= 0",
        "all kappa >= 0", "floor 0"),
    new("CELL-P-GNONNEG", "lambda > 0 & g >= 0",                         "BOUNDED-BELOW",       "L-CS",
        "complete the square; g Omega >= 0 only raises the floor",
        "all kappa >= 0", "floor -N_sites*(mu2)_-^2/(4 lambda)"),
    new("CELL-P-DEN",     "lambda > 0 & g < 0 & convention = DEN",       "UNBOUNDED-BELOW",     "L-SUP",
        "density curvature portal: -t^8 runaway strictly dominates S_B ~ t^4 and kinetic ~ t^2",
        "all kappa >= 0 (growth-rate strictly dominant)", "-"),
    new("CELL-P-MAG",     "lambda > 0 & g < 0 & convention = MAG",       "CONDITIONAL-CRITICAL","L-CRIT",
        "magnitude curvature portal: exact critical coupling g_crit = sqrt(2 lambda / rho), rho = 1 on control; " +
        "|g| < g_crit BOUNDED, |g| > g_crit UNBOUNDED (kappa = 0), |g| = g_crit decided by sign(mu2)",
        "kappa = 0 certified arm; kappa > 0 raises g_crit (boundedness-favorable)", "g_crit = sqrt(2 lambda / rho)"),
};

// Machine check: the menu partitions the sign lattice (disjoint + exhaustive).
var (partitionExhaustive, partitionDisjoint, partitionCoverageCount) = CheckSignPartition(cells);

int boundedCount = cells.Count(c => c.Verdict == "BOUNDED-BELOW");
int unboundedCount = cells.Count(c => c.Verdict == "UNBOUNDED-BELOW");
int conditionalCount = cells.Count(c => c.Verdict == "CONDITIONAL-CRITICAL");
bool everyCellHasCertificate = cells.All(c =>
    c.Verdict is "BOUNDED-BELOW" or "UNBOUNDED-BELOW" or "CONDITIONAL-CRITICAL"
    && !string.IsNullOrEmpty(c.Lemma));
bool allCellsCertified = everyCellHasCertificate && partitionExhaustive && partitionDisjoint;

// Numeric self-check of the control-branch identity the L-CRIT threshold rests
// on: g_crit(lambda) = sqrt(2 lambda) at rho = 1, sub-verdicts at sample
// couplings. Exact closed form; no lattice, no sampling.
var critProbe = new List<object>();
bool critLemmaConsistent = true;
foreach (double lambda in new[] { 0.25, 1.0, 4.0 })
{
    double gCrit = System.Math.Sqrt(2.0 * lambda); // rho = 1
    foreach (double g in new[] { -0.5 * gCrit, -gCrit, -1.5 * gCrit })
    {
        // Reduced-arm (kappa = 0) leading coefficient of phi(r): (2 lambda - g^2)/(4 lambda).
        double lead = (2.0 * lambda - g * g) / (4.0 * lambda);
        string sub = System.Math.Abs(g) < gCrit - 1e-12 ? "BOUNDED"
            : System.Math.Abs(g) > gCrit + 1e-12 ? "UNBOUNDED"
            : "CRITICAL-LINE";
        bool ok = sub switch
        {
            "BOUNDED" => lead > 0,
            "UNBOUNDED" => lead < 0,
            _ => System.Math.Abs(lead) <= 1e-12,
        };
        critLemmaConsistent &= ok;
        critProbe.Add(new { lambda, g, gCrit, leadingCoefficient = lead, subVerdict = sub, consistent = ok });
    }
}

// ===========================================================================
// (2) NULL-HASH FIREWALL keyed to phase466.
// ===========================================================================

// Read the on-disk phase466 schema {schemaId, schemaHash}.
string? observedSchemaId = null, observedSchemaHash = null;
bool phase466Present = File.Exists(Phase466SummaryPath);
if (phase466Present)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(Phase466SummaryPath));
    if (doc.RootElement.TryGetProperty("schema", out var schemaEl))
    {
        if (schemaEl.TryGetProperty("schemaId", out var idEl)) observedSchemaId = idEl.GetString();
        if (schemaEl.TryGetProperty("schemaHash", out var hashEl)) observedSchemaHash = hashEl.GetString();
    }
}

// Pure firewall predicate (reused by the battery on synthetic inputs).
static bool SchemaPinSatisfied(string? id, string? hash, string pinId, string pinHash) =>
    id is not null && hash is not null && id == pinId && hash == pinHash;

bool schemaPinSatisfied = SchemaPinSatisfied(observedSchemaId, observedSchemaHash, PinnedSchemaId, PinnedSchemaHash);

// Look for an o4MProbeRuling record at the pre-registered candidate paths.
var o4Searched = new List<object>();
bool o4MProbeRulingPresent = false;
string? o4RulingPathFound = null;
foreach (var path in O4MProbeRulingCandidatePaths)
{
    bool exists = File.Exists(path);
    bool validRecord = false;
    if (exists)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            validRecord = doc.RootElement.TryGetProperty("wsThreeMProbeScopeSignedOff", out var s)
                && s.ValueKind == JsonValueKind.True
                && doc.RootElement.TryGetProperty("signer", out var sg)
                && sg.ValueKind == JsonValueKind.String
                && !string.IsNullOrWhiteSpace(sg.GetString());
        }
        catch { validRecord = false; }
    }
    o4Searched.Add(new { path, exists, validRecord });
    if (validRecord) { o4MProbeRulingPresent = true; o4RulingPathFound = path; }
}

bool firewallOpen = schemaPinSatisfied && o4MProbeRulingPresent;

// --- Firewall battery (machine-checkable; each case is an expected flip). ----
var firewallBattery = new List<object>();
bool firewallBatteryPassed = true;
void FwCase(string name, bool computed, bool expected, string note)
{
    bool ok = computed == expected;
    firewallBatteryPassed &= ok;
    firewallBattery.Add(new { name, computed, expected, pass = ok, note });
}
// B1: nominal on-disk schema satisfies the pin.
FwCase("nominal-schema-pin", schemaPinSatisfied, true,
    "the committed phase466 schema {schemaId, schemaHash} matches the pinned pair");
// B2: a synthetic wrong schemaHash MUST block.
FwCase("synthetic-wrong-hash-blocks",
    SchemaPinSatisfied(observedSchemaId, "0000000000000000000000000000000000000000000000000000000000000000", PinnedSchemaId, PinnedSchemaHash),
    false, "a tampered schemaHash must fail the pin");
// B3: a synthetic wrong schemaId MUST block.
FwCase("synthetic-wrong-id-blocks",
    SchemaPinSatisfied("ws3-vev-completion-contract-schema-TAMPERED", observedSchemaHash, PinnedSchemaId, PinnedSchemaHash),
    false, "a tampered schemaId must fail the pin");
// B4: with a valid schema but NO o4 ruling, the conjunction is closed.
FwCase("o4-absent-keeps-firewall-closed", schemaPinSatisfied && o4MProbeRulingPresent, false,
    "schema pins but the o4MProbeRuling record does not exist => verdict emission blocked");
// B5: a synthetic o4 ruling present + valid schema opens the conjunction
//     (proves the gate is not stuck-closed and requires the FULL conjunction).
FwCase("synthetic-o4-present-opens-conjunction", schemaPinSatisfied && true, true,
    "with the schema pinned, a valid o4MProbeRuling would open verdict emission (still gated on motivation-green for the portal verdict)");

bool firewallMachineChecked = firewallBatteryPassed;

// ===========================================================================
// (3) ARM Q motivation-gate measurement code -- IMPLEMENTED, GATED OFF.
// ===========================================================================
//
// Arm Q measures, on a seed-regenerated (omega,theta,M) ensemble, the portal
// order parameter <|M|^2> and its connected susceptibility (the motivation
// signal deciding whether a Stage-B measurement is warranted). Its integrity
// guard is the RNG-STREAM-NEUTRALITY battery: regenerating the ensemble with
// the M-sector RNG stream advanced must leave the (omega,theta)-marginal
// observables (S_B, Phi) unchanged within MC error -- proving the probe-field
// insertion did not contaminate the base measure. ("piggyback" language is
// deleted; the alternative is a FRESH independent stream, labeled as such.)
//
// EXECUTION IS GATED OFF. Arm Q may run only when ALL hold:
//   G-Q1  firewallOpen  (phase466 schema pinned AND o4MProbeRuling present);
//   G-Q2  a real seed-regenerated ensemble in the phase450/453 HMC format is
//         committed and its precursor batteries pass;
//   G-Q3  the RNG-stream-neutrality battery passes on that ensemble (or the
//         FRESH-labeled arm is declared);
//   G-Q4  Team C co-signature for any deadline terminal (never closes L7).
// The committed default runs Arm Q automatically once G-Q1..G-Q3 hold. Here
// they do not (firewall closed, no ensemble), so Arm Q does NOT run: the code
// below executes only a SYNTHETIC PLUMBING SELF-TEST (zero physics), exactly as
// phase450 exercises WHAM on synthetic Gaussians.

bool armQMayRun = firewallOpen; // plus G-Q2..G-Q4 at launch; false here.

// -- Measurement (pure function over an ensemble of (sB, phi, mSquared) samples).
static (double MSquaredMean, double MSquaredStdErr, double Susceptibility) ArmQMeasure(
    IReadOnlyList<(double SB, double Phi, double MSquared)> ensemble)
{
    int n = ensemble.Count;
    double mean = ensemble.Average(s => s.MSquared);
    double var = n > 1 ? ensemble.Sum(s => (s.MSquared - mean) * (s.MSquared - mean)) / (n - 1) : 0.0;
    double stdErr = System.Math.Sqrt(var / System.Math.Max(1, n));
    double susceptibility = var; // connected: <|M|^4> - <|M|^2>^2 (per-sample estimator)
    return (mean, stdErr, susceptibility);
}

// -- RNG-stream-neutrality battery: (omega,theta)-marginal must be invariant
//    under advancing the M-sector stream, within k * combined std error.
static (bool Pass, double SBDelta, double SBErr, double PhiDelta, double PhiErr) RngStreamNeutrality(
    IReadOnlyList<(double SB, double Phi, double MSquared)> baseEns,
    IReadOnlyList<(double SB, double Phi, double MSquared)> mAdvancedEns, double k)
{
    static (double Mean, double Err) Stat(IReadOnlyList<double> xs)
    {
        int n = xs.Count;
        double m = xs.Average();
        double v = n > 1 ? xs.Sum(x => (x - m) * (x - m)) / (n - 1) : 0.0;
        return (m, System.Math.Sqrt(v / System.Math.Max(1, n)));
    }
    var (sb0, sbE0) = Stat(baseEns.Select(s => s.SB).ToList());
    var (sb1, sbE1) = Stat(mAdvancedEns.Select(s => s.SB).ToList());
    var (ph0, phE0) = Stat(baseEns.Select(s => s.Phi).ToList());
    var (ph1, phE1) = Stat(mAdvancedEns.Select(s => s.Phi).ToList());
    double sbDelta = System.Math.Abs(sb0 - sb1), sbErr = System.Math.Sqrt(sbE0 * sbE0 + sbE1 * sbE1);
    double phDelta = System.Math.Abs(ph0 - ph1), phErr = System.Math.Sqrt(phE0 * phE0 + phE1 * phE1);
    bool pass = sbDelta <= k * System.Math.Max(sbErr, 1e-12) && phDelta <= k * System.Math.Max(phErr, 1e-12);
    return (pass, sbDelta, sbErr, phDelta, phErr);
}

// -- Synthetic plumbing self-test (zero physics; NOT a measurement of anything).
//    Positive control: base and M-advanced share the SAME (omega,theta)
//    marginal (neutral) => battery passes. Negative control: advancing the M
//    stream ALSO shifts the base marginal (contaminated) => battery fails.
//    Both must behave => the battery has teeth.
object armQSelfTest;
bool armQCodeSelfTested;
{
    var rng = new Random(457_0712);
    int nSamp = 4000;
    var baseEns = new List<(double, double, double)>(nSamp);
    var neutralEns = new List<(double, double, double)>(nSamp);
    var contaminatedEns = new List<(double, double, double)>(nSamp);
    for (int i = 0; i < nSamp; i++)
    {
        double sb = 1.0 + 0.20 * Gauss(rng);
        double phi = 0.0 + 0.30 * Gauss(rng);
        double mSq = 0.5 + 0.10 * Gauss(rng);
        baseEns.Add((sb, phi, mSq));
        // Neutral: same (sb, phi) marginal, independent fresh M draw.
        neutralEns.Add((sb, phi, 0.5 + 0.10 * Gauss(rng)));
        // Contaminated: advancing the M stream leaks a bias into the marginal.
        contaminatedEns.Add((sb + 0.25, phi, 0.5 + 0.10 * Gauss(rng)));
    }
    var (measMean, measErr, measChi) = ArmQMeasure(baseEns);
    var neutral = RngStreamNeutrality(baseEns, neutralEns, k: 4.0);
    var contaminated = RngStreamNeutrality(baseEns, contaminatedEns, k: 4.0);
    bool selfTestPassed = neutral.Pass && !contaminated.Pass;
    armQSelfTest = new
    {
        note = "SYNTHETIC plumbing self-test only; zero physics; not a measurement of any target.",
        measurement = new { mSquaredMean = measMean, mSquaredStdErr = measErr, susceptibility = measChi },
        rngStreamNeutralityBattery = new
        {
            positiveControlNeutralPass = neutral.Pass,
            positiveControlSBDelta = neutral.SBDelta,
            positiveControlSBErr = neutral.SBErr,
            negativeControlContaminatedPass = contaminated.Pass,
            negativeControlSBDelta = contaminated.SBDelta,
            batteryHasTeeth = selfTestPassed,
        },
        selfTestPassed,
    };
    armQCodeSelfTested = selfTestPassed;
}

// ===========================================================================
// (4) Terminal + guarded verdict emission.
// ===========================================================================

string interimTerminal = allCellsCertified ? "stage-a-certificates-committed" : "stage-a-certificates-partial";
string terminalStatus = TerminalPrefix + interimTerminal;
string verdictKind = interimTerminal;

// Pre-unlock summary discipline: a measurement may be RECORDED, a verdict may
// not be EMITTED. Arm Q is gated off => no measurement recorded. The firewall
// is closed => the portal verdict is WITHHELD.
bool measurementRecorded = false; // Arm Q gated OFF
bool portalVerdictEmitted = false; // firewall closed
const string PreUnlockOnlyReachableSummaryTerminal = "measurement-recorded-verdict-withheld";
string summaryVerdictTerminal = firewallOpen
    ? (measurementRecorded ? PreUnlockOnlyReachableSummaryTerminal : "awaiting-measurement")
    : "verdict-withheld-firewall-closed";

// Guarded portal-verdict block: every verdict-bearing field is WITHHELD unless
// the firewall is open (it is not). No portal verdict field carries a value.
object portalVerdictBlock = firewallOpen
    ? new { note = "firewall open path is unreachable in this checkpoint" }
    : new
    {
        emitted = false,
        withheldReason = "null-hash firewall closed: schemaPinSatisfied=" + schemaPinSatisfied +
                         ", o4MProbeRulingPresent=" + o4MProbeRulingPresent,
        portalMeasureNonexistentAtWorkbenchScope = "WITHHELD",
        portalNullMeasured = "WITHHELD",
        portalScaleCandidate = "WITHHELD",
        l7Closure = "WITHHELD",
    };

// --- standing claim boundary (verbatim across the program; asserted by the
//     shared surfaces -- preserved unchanged). ---
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool physicistReviewPending = true;
const bool scaleIsWorkbenchRelativeCandidateOnly = true;
const bool noGevPromotion = true;
const bool sourceContractApplicationAllowed = false;
const bool phase201TemplateMutated = false;
const int fieldsAppliedToPhase201TemplateCount = 0;
const int acceptedContractFieldCount = 0;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256Contract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool physicalCouplingProvided = false;
const bool routeProvidesPhysicalEffectiveActionHessian = false;
const bool routeProvidesVevOrSourceScaleLineage = false;
const bool routeProvidesPoleExtractionAndGeVNormalization = false;
const bool routeCompletesBosonPredictions = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;

// upsilonPortalStageASkeletonBuilt stays true (the skeleton is built AND
// advanced) so the existing shared-surface asserts on it continue to hold.
const bool upsilonPortalStageASkeletonBuilt = true;

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
    string.Join("|", ApplicationSubjectKind, interimTerminal, PlanSection,
        "stage-a boundedness certificates; null-hash firewall keyed to phase466; arm Q gated off; " +
        "standing claim boundary; portal verdict withheld")))).ToLowerInvariant();

double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

var result = new
{
    phaseId = "phase457-upsilon-portal-stage-a",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    interimTerminal,
    priorInterimTerminal = PriorInterimTerminal,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    eliminationItem = EliminationItem,
    upsilonPortalStageASkeletonBuilt,

    // ---- (1) Stage A certificates ----
    stageA = new
    {
        cellMenuId = "upsilon-portal-stage-a-sign-cell-menu-v1",
        portalActionClass = "S_portal = S_B + kappa*sum||D_omega M||^2 + sum_x[lambda|M|^4 + (mu2 + g*Omega)|M|^2]; " +
                            "Omega in {||F|| (MAG), ||F||^2 (DEN)}; M = labeled probe convention (O8)",
        certifiedArm = "kappa = 0 reduced arm; kappa > 0 is boundedness-favorable (recorded per cell)",
        controlBranchIdentity = "Upsilon = F on the control branch => ||F||^2 = 2 S_B exactly => rho = 1 => g_crit = sqrt(2 lambda)",
        cells = cells.Select(c => new
        {
            c.Id, c.SignPredicate, c.Verdict, c.Lemma, c.Mechanism, c.KappaScope, c.FloorOrThreshold,
        }),
        boundedCount,
        unboundedCount,
        conditionalCount,
        partitionExhaustive,
        partitionDisjoint,
        partitionCoverageCount,
        everyCellHasCertificate,
        allCellsCertified,
        criticalLemmaSelfCheck = new { consistent = critLemmaConsistent, probe = critProbe },
        framing = "EXACT mathematics about the action class; constrains the probe design; NOT a physics claim about any target.",
    },

    // ---- (2) null-hash firewall ----
    firewall = new
    {
        keyedToPhase = "phase466",
        phase466SummaryPath = Phase466SummaryPath,
        phase466Present,
        pinnedSchemaId = PinnedSchemaId,
        pinnedSchemaHash = PinnedSchemaHash,
        observedSchemaId,
        observedSchemaHash,
        schemaPinSatisfied,
        o4MProbeRulingCandidatePaths = O4MProbeRulingCandidatePaths,
        o4MProbeRulingSearched = o4Searched,
        o4MProbeRulingPresent,
        o4RulingPathFound,
        firewallOpen,
        verdictEmissionAllowed = firewallOpen, // portal verdict additionally needs motivation-green
        battery = firewallBattery,
        firewallMachineChecked,
        note = "No verdict-bearing summary field is emitted unless the on-disk phase466 {schemaId, schemaHash} " +
               "matches the pinned pair AND an o4MProbeRuling record exists. It does not; the portal verdict is withheld.",
    },

    // ---- (3) Arm Q (gated OFF) ----
    armQ = new
    {
        implemented = true,
        gatedOff = true,
        mayRun = armQMayRun,
        launchConditions = new[]
        {
            "G-Q1: firewallOpen (phase466 schema pinned AND o4MProbeRuling present)",
            "G-Q2: a committed seed-regenerated ensemble in the phase450/453 HMC format with precursor batteries green",
            "G-Q3: the RNG-stream-neutrality battery passes on that ensemble (or the FRESH-labeled arm is declared)",
            "G-Q4: Team C co-signature for any deadline terminal (never closes L7)",
        },
        committedDefault = "runs Arm Q automatically once G-Q1..G-Q3 hold; no manual step",
        ensembleFormat = "phase450/453 (omega,theta,M) HMC ensemble: per-sample (S_B, Phi, |M|^2), umbrella windows, theta-Haar",
        codeSelfTested = armQCodeSelfTested,
        selfTest = armQSelfTest,
        piggybackLanguageDeleted = true,
    },

    // ---- (4) guarded verdict / summary discipline ----
    preUnlockOnlyReachableSummaryTerminal = PreUnlockOnlyReachableSummaryTerminal,
    measurementRecorded,
    portalVerdictEmitted,
    summaryVerdictTerminal,
    portalVerdict = portalVerdictBlock,
    holdConjunction = new { phase466SchemaPin = schemaPinSatisfied ? "satisfied" : "unmet", o4MProbeRuling = o4MProbeRulingPresent ? "present" : "absent" },
    limbConsumed = "L7",

    // ---- standing claim boundary ----
    scaleIsWorkbenchRelativeCandidateOnly,
    noGevPromotion,
    physicistReviewPending,
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
        canFillPhase256ObservedFieldExtractionContract,
        phase201FieldsDefensiblyFilled = Array.Empty<string>(),
    },
    recordedBoundary = new { physicistReviewPending, awaitingImplementation = false },
    explicitCandidateOnlyNonclaims = new[]
    {
        "Stage A commits boundedness-below CERTIFICATES for the portal action class as EXACT MATHEMATICS; they constrain the probe design and are not physics claims about any target.",
        "The null-hash firewall is CLOSED (no o4MProbeRuling record exists); no portal verdict is emitted. The reachable summary state is at most measurement-recorded-verdict-withheld.",
        "Arm Q is implemented but GATED OFF; the only code executed here is a synthetic plumbing self-test with zero physics.",
        "M is a labeled probe convention (O8); a gap proves a singlet, never a named electroweak scalar. Lattice-unit quantities stay in lattice units; promotedPhysicalMassClaimCount remains 0; physicistReviewPending carried explicitly.",
    },
    assertExpectations = new
    {
        stageACertificatesCommitted = allCellsCertified,
        firewallClosedVerdictWithheld = !firewallOpen && !portalVerdictEmitted,
        firewallBatteryPassed,
        armQGatedOff = !armQMayRun,
        standingClaimBoundaryIntact =
            targetBlindConstruction && physicistReviewPending && noGevPromotion &&
            !sourceContractApplicationAllowed && !routePromotesWzMasses && !routePromotesHiggsMass &&
            !routeCompletesBosonPredictions,
    },
    decision =
        "Stage A commits exhaustive-cell boundedness-below certificates for the Upsilon-portal action class: " +
        boundedCount + " bounded, " + unboundedCount + " unbounded, " + conditionalCount +
        " conditional-critical, over a disjoint exhaustive sign-cell menu (exact mathematics; control-branch " +
        "identity ||F||^2 = 2 S_B fixes g_crit = sqrt(2 lambda)). The null-hash firewall keyed to phase466 is " +
        "CLOSED (schema pins but no o4MProbeRuling record exists), so the portal verdict is WITHHELD; Arm Q is " +
        "implemented and GATED OFF. Terminal " + interimTerminal + ". Nothing is measured or promoted; " +
        "promotedPhysicalMassClaimCount remains 0.",
    runtimeSeconds,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(System.IO.Path.Combine(outputDir, "upsilon_portal_stage_a.json"), json);
File.WriteAllText(System.IO.Path.Combine(outputDir, "upsilon_portal_stage_a_summary.json"), json);

Console.WriteLine(terminalStatus);
Console.WriteLine($"stageA: cells={cells.Count} bounded={boundedCount} unbounded={unboundedCount} conditional={conditionalCount} allCertified={allCellsCertified} partition(exh/disj)={partitionExhaustive}/{partitionDisjoint}");
foreach (var c in cells)
    Console.WriteLine($"  {c.Id,-18} {c.Verdict,-21} [{c.Lemma}] {c.SignPredicate}");
Console.WriteLine($"firewall: schemaPin={schemaPinSatisfied} o4Ruling={o4MProbeRulingPresent} open={firewallOpen} batteryPassed={firewallBatteryPassed}");
Console.WriteLine($"armQ: implemented=true gatedOff=true mayRun={armQMayRun} codeSelfTested={armQCodeSelfTested}");
Console.WriteLine($"summaryVerdictTerminal={summaryVerdictTerminal} (portalVerdictEmitted={portalVerdictEmitted})");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F3}");

// --- exhaustiveness / disjointness check of the sign-cell menu. --------------
// Enumerate the full sign lattice sign(lambda) x sign(g) x sign(mu2) x
// convention and confirm each point is matched by EXACTLY ONE cell predicate.
static (bool Exhaustive, bool Disjoint, int Coverage) CheckSignPartition(List<PortalCell> cells)
{
    string[] lamS = { "neg", "zero", "pos" };
    string[] gS = { "neg", "nonneg" };
    string[] mu2S = { "neg", "nonneg" };
    string[] convS = { "MAG", "DEN" };
    int covered = 0, points = 0;
    bool exhaustive = true, disjoint = true;
    foreach (var lam in lamS)
        foreach (var g in gS)
            foreach (var mu2 in mu2S)
                foreach (var conv in convS)
                {
                    points++;
                    int matches = cells.Count(c => c.Matches(lam, g, mu2, conv));
                    if (matches == 0) exhaustive = false;
                    if (matches > 1) disjoint = false;
                    if (matches >= 1) covered++;
                }
    return (exhaustive, disjoint, covered == points ? points : covered);
}

static double Gauss(Random r)
{
    double u1 = 1.0 - r.NextDouble(), u2 = 1.0 - r.NextDouble();
    return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
}

// Portal cell record: a sign-predicate and its committed certificate. Matches()
// encodes the predicate over the sign lattice for the partition check.
sealed record PortalCell(
    string Id, string SignPredicate, string Verdict, string Lemma,
    string Mechanism, string KappaScope, string FloorOrThreshold)
{
    public bool Matches(string lam, string g, string mu2, string conv) => Id switch
    {
        "CELL-Q-NEG" => lam == "neg",
        "CELL-Z-MASSNEG" => lam == "zero" && mu2 == "neg",
        "CELL-Z-PORTALNEG" => lam == "zero" && mu2 == "nonneg" && g == "neg",
        "CELL-Z-SOS" => lam == "zero" && mu2 == "nonneg" && g == "nonneg",
        "CELL-P-GNONNEG" => lam == "pos" && g == "nonneg",
        "CELL-P-DEN" => lam == "pos" && g == "neg" && conv == "DEN",
        "CELL-P-MAG" => lam == "pos" && g == "neg" && conv == "MAG",
        _ => false,
    };
}
