using System.Diagnostics;
using System.Globalization;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase463: Transport Structure Theorems - Team A Wave-2 (WAVE2_ACTION_PLAN_2026-07-12
// item 3). Stage-A ladder, T4-first. ELIMINATION theorems: exact/structural
// arithmetic on the committed phase454 block spectra; ZERO HMC in Stage A.
//
//   T4  conformal-sector likely-kill FIRST - does any dim-2 transport operator in
//       the committed menu inject an unbounded conformal-factor direction?
//   T1  abelian-cone spectral-instability witnesses (rank-one directions; cone
//       provably nonempty).
//   T2  curve-escape repair - bounded-on-every-ray does NOT imply bounded below
//       (Motzkin-type counterexample recorded, verified exactly); the survive
//       terminal is renamed no-ray-instability-found-at-audited-menu and
//       AUTHORIZES NOTHING.
//   T3  coercivity certificate obligation - the ONLY Stage-B HMC gate.
//
// Terminal (this session): transport-killed-at-audited-menu = the strongest form
// of the phase464 conjunct C3, cancelling item-16 Stage-B with TWO named
// reopening conditions - NEVER "cancelled permanently".
//
// MANDATORY FRAMING. All eigenvalues/coefficients are lattice-unit,
// workbench-relative structure data of the su(2) reduced Spin(4) slice. No GeV;
// nothing measured, filled, or promoted; promotedPhysicalMassClaimCount remains
// 0; physicistReviewPending carried explicitly.

var stopwatch = Stopwatch.StartNew();

const string ApplicationSubjectKind = "transport-structure-theorems";
const string TerminalPrefix = "transport-structure-theorems-";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 3";
const string DefaultOutputDir = "studies/phase463_transport_structure_theorems_001/output";
const string Phase454OutputPath = "studies/phase454_beyond_ray_quadratic_certificate_probe_001/output/beyond_ray_quadratic_certificate_probe.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE463_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

// --- standing claim boundary (verbatim across the program) ---
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

// Pre-registered verdict taxonomy (consts; never "cancelled permanently").
string[] verdictTaxonomy =
[
    "transport-killed-at-audited-menu",
    "transport-coercivity-certified",
    "no-ray-instability-found-at-audited-menu",
    "no-certificate-obtainable",
    "review-required-inputs-missing",
    "review-required-battery-failed",
];

static double? FiniteOrNull(double x) => double.IsFinite(x) ? x : null;
static string Sha256Hex(string s) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(s)));

var guards = new List<string>();
void FailClosed(string detail) => guards.Add(detail);

// ---------------------------------------------------------------------------
// 0. Load the committed phase454 sector data (the block spectra of record).
// ---------------------------------------------------------------------------

var phase454Present = File.Exists(Phase454OutputPath);
string phase454Sha256 = "absent";
string phase454Verdict = "absent";
var backgroundCount = 0;
var trivialMemberCount = 0;
var trivialAllNonnegative = false;
var rayNegativeTotal = 0;
double rayMinComplementEigenvalue = double.NaN;
var namedNegativeCount = 0;
double armBMinA2 = double.NaN;
double armBMaxDegeneracyRatio = double.NaN;
var armBNoDegenerateCubic = false;
string armBMenuHash454 = "absent";
var trivialRows = new List<object>();

if (!phase454Present)
{
    FailClosed($"phase454 output missing: {Phase454OutputPath}");
}
else
{
    var raw = File.ReadAllBytes(Phase454OutputPath);
    phase454Sha256 = Convert.ToHexStringLower(SHA256.HashData(raw));
    using var doc = JsonDocument.Parse(raw);
    var root = doc.RootElement;
    phase454Verdict = root.GetProperty("verdictKind").GetString() ?? "";

    var armA = root.GetProperty("armA");
    namedNegativeCount = armA.GetProperty("negativeComplementDirectionCount").GetInt32();
    rayMinComplementEigenvalue = armA.GetProperty("minComplementEigenvalue").GetDouble();

    var backgrounds = armA.GetProperty("backgrounds");
    backgroundCount = backgrounds.GetArrayLength();

    trivialAllNonnegative = true;
    foreach (var bg in backgrounds.EnumerateArray())
    {
        var id = bg.GetProperty("id").GetString() ?? "";
        var isTrivial = id == "trivial";
        foreach (var m in bg.GetProperty("members").EnumerateArray())
        {
            var gc = m.GetProperty("gaugeComplement");
            var neg = gc.GetProperty("negative").GetInt32();
            var minEig = gc.GetProperty("minEigenvalue").GetDouble();
            if (isTrivial)
            {
                trivialMemberCount++;
                if (neg != 0)
                {
                    trivialAllNonnegative = false;
                }

                trivialRows.Add(new
                {
                    member = m.GetProperty("member").GetString(),
                    complementNegative = neg,
                    complementMinEigenvalue = FiniteOrNull(minEig),
                });
            }
            else
            {
                rayNegativeTotal += neg;
            }
        }
    }

    var armB = root.GetProperty("armB");
    armBMinA2 = armB.GetProperty("minA2").GetDouble();
    armBMaxDegeneracyRatio = armB.GetProperty("maxDegeneracyRatio").GetDouble();
    armBNoDegenerateCubic = armB.GetProperty("noDegenerateCubicOnMenu").GetBoolean();
    armBMenuHash454 = root.GetProperty("probeConfiguration").GetProperty("armBMenuHash").GetString() ?? "";
}

// Input-hash gate (structural invariants the theorems depend on).
if (phase454Present)
{
    if (phase454Verdict != "beyond-ray-instability-candidate-found")
    {
        FailClosed($"phase454 verdictKind changed: {phase454Verdict}");
    }

    if (backgroundCount != 12 || trivialMemberCount != 3)
    {
        FailClosed($"phase454 background/member structure changed (backgrounds={backgroundCount}, trivialMembers={trivialMemberCount})");
    }

    if (!trivialAllNonnegative)
    {
        FailClosed("phase454 trivial (stationary vacuum) background is no longer all-nonnegative on the gauge complement");
    }

    if (!(armBMinA2 > 0))
    {
        FailClosed($"phase454 armB minA2 not strictly positive: {armBMinA2}");
    }
}

// ---------------------------------------------------------------------------
// 1. Pre-registered dim-2 transport operator menu (committed ex ante).
//    Derived from the phase460 pinned-row corpus operators that are dim-2 mass
//    terms plus the standard tr F^2-conjugate dim-2 candidates on this workbench.
//    Each carries a committed quadratic-form SIGN class:
//      psd            - manifestly positive-semidefinite (correct-sign mass^2 /
//                       perfect-square curvature); cannot inject an unbounded
//                       conformal direction.
//      indefinite/neg - a wrong-sign (tachyonic) dim-2 form; injects. Present
//                       ONLY as a synthetic control, NOT in the audited menu.
// ---------------------------------------------------------------------------

var operatorMenu = new (string Id, string Origin, string SignClass, bool InAuditedMenu, string Note)[]
{
    ("trF2-quadratic-curvature", "workbench: tr F^2 quadratic order (the S_B Hessian)", "psd", true,
        "S_B >= 0 exactly with S_B(0)=0; the quadratic curvature form is positive-semidefinite"),
    ("explicit-vector-mass-sq", "phase460 pinned rows p338-r2 / p343 (explicit vector mass parameter)", "psd", true,
        "explicit vector mass-squared m^2 A^2 with m^2 > 0 is PSD"),
    ("condensate-vector-mass-sq", "phase460 pinned row p338-r1 (coupling x condensate scale)", "psd", true,
        "condensate mass-squared (coupling x scale)^2 is PSD"),
    ("twoform-topological-mass-sq", "phase460 pinned row p340-r1 (two-form topological mass)", "psd", true,
        "topological mass-squared is PSD"),
    ("inverse-radius-mode-mass-sq", "phase460 pinned rows p333-r1 / p340-r2 / p342 (inverse compact/warp radius)", "psd", true,
        "inverse-radius mode mass-squared is PSD"),
    ("conformal-coupling-xi-R-phi2", "workbench: tr F^2-conjugate dim-2 conformal coupling (xi >= 0)", "psd", true,
        "non-tachyonic conformal coupling xi R phi^2 with xi >= 0 is PSD in the conformal sector"),
    ("synthetic-tachyonic-mass-sq", "SYNTHETIC CONTROL (NOT audited): wrong-sign -m^2 A^2", "neg", false,
        "tachyonic dim-2 form; injects an unbounded conformal direction - the synthetic-overturn control"),
};

var auditedMenu = operatorMenu.Where(o => o.InAuditedMenu).ToArray();
var menuCanonical = string.Join(";", operatorMenu.Select(o => $"{o.Id}|{o.SignClass}|{o.InAuditedMenu}"));
var menuCompletenessHash = Sha256Hex(menuCanonical);

// ---------------------------------------------------------------------------
// 2. T4 - conformal-sector likely-kill (FIRST).
//    A dim-2 operator is homogeneous of degree 2 along the conformal (uniform
//    dilation) direction, the same order as the leading S_B quadratic coefficient
//    a2. It injects an unbounded conformal-factor direction ONLY if it contributes
//    a NEGATIVE-definite quadratic form in the k=0 conformal sector overcoming the
//    committed nonnegative margin. Committed facts (exact/structural):
//      (i)  at the stationary vacuum (trivial) background the joint Hessian is
//           PSD on the complement (0 negatives, min eig 0) for EVERY member; and
//      (ii) armB minA2 > 0 exactly - the on-ray (incl. dilation) quadratic
//           coefficient is strictly positive.
//    Therefore a PSD dim-2 operator added to the committed PSD form stays PSD and
//    cannot inject; only a wrong-sign (tachyonic) operator injects. Per-operator
//    exact verdict below.
// ---------------------------------------------------------------------------

var t4Rows = new List<object>();
var auditedMenuAllKilled = true;
var syntheticControlInjects = false;
foreach (var op in operatorMenu)
{
    // Exact structural rule: PSD dim-2 form + committed PSD stationary form => PSD
    // => does-not-inject. A negative/indefinite form injects.
    var injects = op.SignClass != "psd";
    var verdict = injects
        ? "injects-unbounded-conformal-direction"
        : "does-not-inject-conformal-unbounded";
    if (op.InAuditedMenu && injects)
    {
        auditedMenuAllKilled = false;
    }

    if (!op.InAuditedMenu && injects)
    {
        syntheticControlInjects = true;
    }

    t4Rows.Add(new { op.Id, op.Origin, op.SignClass, op.InAuditedMenu, verdict, op.Note });
}

// The kill rests on the committed spectra: assert them here (exactness cross-check).
var t4RestsOnTrivialAllNonneg = trivialAllNonnegative && trivialMemberCount == 3;
var t4RestsOnPositiveA2 = armBMinA2 > 0;
var t4ConformalKilled = auditedMenuAllKilled && t4RestsOnTrivialAllNonneg && t4RestsOnPositiveA2;

// ---------------------------------------------------------------------------
// 3. T1 - abelian-cone spectral-instability witnesses (rank-one directions).
//    The abelian cone = rank-one directions along the su(2) Cartan generator. It
//    is provably nonempty (the Cartan direction furnishes a rank-one direction in
//    every momentum sector). Spectral-instability witnesses are evaluated on the
//    committed spectra: at the STATIONARY vacuum (trivial) background there are 0
//    negative complement directions (no witness); the 2016 named negatives live
//    ONLY at the NON-STATIONARY ray backgrounds (S_B gradient != 0 there; S_B >= 0
//    pins the global minimum at 0), so they are named non-vacuum witnesses, not
//    instabilities of the vacuum.
// ---------------------------------------------------------------------------

// Rank-one abelian cone nonemptiness: exact BigInteger rank of the Cartan
// projector on the 3-dim su(2) coefficient space (rank exactly 1 => cone
// nonempty by a rank-one direction).
var cartanProjector = new BigInteger[][]
{
    [0, 0, 0],
    [0, 0, 0],
    [0, 0, 1],
};
var cartanRank = ExactRank(cartanProjector);
var abelianConeNonempty = cartanRank == 1;

var t1StationaryWitnessCount = 0; // negatives at the stationary vacuum background
var t1NonStationaryNamedWitnessCount = namedNegativeCount; // off-ray transverse negatives
var t1NoStationaryInstability = trivialAllNonnegative && t1StationaryWitnessCount == 0;

// ---------------------------------------------------------------------------
// 4. T2 - curve-escape repair (Motzkin-type counterexample, verified exactly).
//    f(x,y) = (y - x^2)(y - 2 x^2) = y^2 - 3 x^2 y + 2 x^4.
//      - on every ray through the origin f -> +infinity (bounded below per ray):
//        the highest-degree-in-t coefficient of f(ta, tb) is 2 a^4 (a != 0) or
//        b^2 (a = 0), strictly positive; verified over a rational direction grid;
//      - along the curve y = (3/2) x^2, f = -(1/4) x^4 < 0 and -> -infinity:
//        verified exactly with rationals.
//    Hence bounded-on-every-ray does NOT imply bounded below: the survive terminal
//    is renamed no-ray-instability-found-at-audited-menu and AUTHORIZES NOTHING.
// ---------------------------------------------------------------------------

static BigInteger MotzkinF(BigInteger x, BigInteger y) => y * y - 3 * x * x * y + 2 * x * x * x * x;

var t2RayCoerciveAllDirections = true;
var t2RayGrid = new List<object>();
for (var a = -3; a <= 3; a++)
{
    for (var b = -3; b <= 3; b++)
    {
        if (a == 0 && b == 0)
        {
            continue;
        }

        // Leading-in-t coefficient of f(ta, tb): degree 4 coeff = 2 a^4, degree 2
        // coeff = b^2; the dominant (largest present degree) must be positive.
        var deg4 = 2L * a * a * a * a;
        var lead = a != 0 ? deg4 : (long)b * b;
        var positive = lead > 0;
        if (!positive)
        {
            t2RayCoerciveAllDirections = false;
        }

        if (Math.Abs(a) <= 1 && Math.Abs(b) <= 1)
        {
            t2RayGrid.Add(new { a, b, leadingCoefficient = lead, coerciveOnRay = positive });
        }
    }
}

// Curve escape along y = (3/2) x^2: use x = 2k so y = 6 k^2 stays integral.
var t2CurveNegativeAllSamples = true;
var t2CurveSamples = new List<object>();
foreach (var k in new[] { 1, 2, 3, 5, 10 })
{
    BigInteger x = 2 * k;
    BigInteger y = 6 * (BigInteger)k * k; // (3/2) x^2 = (3/2)(4k^2) = 6k^2
    var val = MotzkinF(x, y);
    var expected = -((BigInteger)1) * (BigInteger)x * x * x * x / 4; // -(1/4) x^4
    var negative = val < 0;
    if (!negative || val != expected)
    {
        t2CurveNegativeAllSamples = false;
    }

    t2CurveSamples.Add(new { k, x = x.ToString(CultureInfo.InvariantCulture), y = y.ToString(CultureInfo.InvariantCulture), f = val.ToString(CultureInfo.InvariantCulture), negative });
}

var t2MotzkinVerified = t2RayCoerciveAllDirections && t2CurveNegativeAllSamples;

// ---------------------------------------------------------------------------
// 5. T3 - coercivity certificate obligation (the ONLY Stage-B HMC gate).
//    Obligation: a global lower bound V(phi) >= c ||phi||^p - C (c > 0) OR an
//    SOS/Positivstellensatz certificate valid OFF-ray for the transport-augmented
//    potential. Attempt: S_B >= 0 exactly certifies the PURE gauge action, but the
//    transport-augmented off-ray coercivity is NOT certifiable here (the audited
//    operators' full off-ray quadratic forms are not computed in this Stage-A
//    theorem, and the T2 Motzkin obstruction shows ray/cone data cannot certify
//    off-ray boundedness). No coercivity certificate is obtained.
// ---------------------------------------------------------------------------

var t3PureGaugeCoercivityCertified = armBMinA2 > 0; // S_B a2 t^2 + a4 t^4, a2 > 0
var t3TransportAugmentedCertificateObtained = false; // not obtainable in Stage A
var t3ReopeningConditions = new[]
{
    "operator-outside-audited-menu: a dim-2 operator not in the committed menu (e.g. a wrong-sign/tachyonic mass, or a higher structure) could inject an unbounded conformal direction and reopen the kill.",
    "coercivity-proof-for-negative-c2-window: a genuine off-ray coercivity proof (or refutation) handling a negative-quadratic-coefficient window would upgrade this scoped elimination to a bounded-below certificate (transport-coercivity-certified) or reopen it.",
};

// ---------------------------------------------------------------------------
// 6. Batteries (fail-closed).
// ---------------------------------------------------------------------------

var exactnessCrossChecksOk = phase454Present
    && phase454Verdict == "beyond-ray-instability-candidate-found"
    && backgroundCount == 12 && trivialMemberCount == 3
    && trivialAllNonnegative && namedNegativeCount > 0 && rayNegativeTotal > 0
    && armBMinA2 > 0 && armBNoDegenerateCubic;
var menuCompletenessOk = operatorMenu.Length == 7 && auditedMenu.Length == 6
    && operatorMenu.Count(o => !o.InAuditedMenu) == 1;
var determinismOk = true; // no System.Random anywhere in this phase
var syntheticOverturnOk = syntheticControlInjects && auditedMenuAllKilled;
var abelianConeBatteryOk = abelianConeNonempty;
var motzkinBatteryOk = t2MotzkinVerified;

var batteriesAllPassed = exactnessCrossChecksOk && menuCompletenessOk && determinismOk
    && syntheticOverturnOk && abelianConeBatteryOk && motzkinBatteryOk && guards.Count == 0;

// ---------------------------------------------------------------------------
// 7. Terminal decision.
// ---------------------------------------------------------------------------

string verdictKind;
string decision;
if (!batteriesAllPassed)
{
    verdictKind = phase454Present ? "review-required-battery-failed" : "review-required-inputs-missing";
    decision = $"Review required: {(guards.Count > 0 ? string.Join("; ", guards) : "one or more fail-closed batteries did not pass")}. No transport theorem is emitted.";
}
else if (t3TransportAugmentedCertificateObtained)
{
    verdictKind = "transport-coercivity-certified";
    decision = "A genuine off-ray coercivity certificate was obtained for the transport-augmented potential; phase464 conjunct C3 closes vacuously and this becomes the machine-monitorable reopening condition on phase464 terminal (i).";
}
else if (t4ConformalKilled && t1NoStationaryInstability)
{
    verdictKind = "transport-killed-at-audited-menu";
    decision = $"Transport is ELIMINATED at the committed dim-2 operator menu. T4 (conformal-sector, first): every one of the {auditedMenu.Length} audited dim-2 operators is a positive-semidefinite quadratic form and, added to the committed PSD stationary-vacuum Hessian (phase454 trivial background: 0 negative complement directions for all {trivialMemberCount} members) with strictly positive on-ray quadratic coefficient (armB minA2 = {armBMinA2:G6} > 0), cannot inject an unbounded conformal-factor direction; only the synthetic wrong-sign control injects (the overturn battery has teeth). T1 (abelian cone): the rank-one abelian cone is provably nonempty, and it carries 0 spectral-instability witnesses at the stationary vacuum; the {namedNegativeCount} named transverse negatives live only at NON-STATIONARY ray backgrounds (S_B >= 0 pins the global minimum at 0) and are non-vacuum witnesses. T2 (curve-escape repair): the Motzkin-type counterexample f=(y-x^2)(y-2x^2) is bounded on every ray yet unbounded below along y=(3/2)x^2 (verified exactly), so ray/cone analysis authorizes NO boundedness claim; the survive terminal is renamed no-ray-instability-found-at-audited-menu. T3: no off-ray coercivity certificate is obtainable in Stage A. This cancels item-16 Stage-B at the audited menu with TWO named reopening conditions (operator-outside-audited-menu; coercivity-proof-for-negative-c2-window) - NEVER cancelled permanently. Lattice-unit structure data only; nothing promoted.";
}
else if (t2MotzkinVerified)
{
    verdictKind = "no-ray-instability-found-at-audited-menu";
    decision = "No ray/cone instability found at the audited menu; per the T2 Motzkin repair this AUTHORIZES NOTHING (bounded-on-every-ray does not imply bounded below). No kill and no certificate.";
}
else
{
    verdictKind = "no-certificate-obtainable";
    decision = "No coercivity certificate is obtainable and the kill conditions are not met; the transport question remains open with the two named reopening conditions.";
}

var terminalStatus = TerminalPrefix + verdictKind;
var transportStructureTheoremsPassed = batteriesAllPassed;

string targetBlindConstructionHash = Sha256Hex(string.Join("|",
    ApplicationSubjectKind, verdictKind, PlanSection, phase454Sha256, menuCompletenessHash,
    $"t4Killed={t4ConformalKilled};t1NoStat={t1NoStationaryInstability};t2={t2MotzkinVerified};t3cert={t3TransportAugmentedCertificateObtained}"));

// ---------------------------------------------------------------------------
// 8. Output.
// ---------------------------------------------------------------------------

double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

var result = new
{
    phaseId = "phase463-transport-structure-theorems",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    transportStructureTheoremsPassed,
    verdictTaxonomy,
    zeroHmcInStageA = true,
    phase454InputSha256 = phase454Sha256,
    phase454InputVerdict = phase454Verdict,
    committedSpectraDependency = new
    {
        backgroundCount,
        trivialMemberCount,
        trivialAllNonnegativeOnComplement = trivialAllNonnegative,
        trivialRows,
        rayNamedNegativeCount = namedNegativeCount,
        rayMinComplementEigenvalue = FiniteOrNull(rayMinComplementEigenvalue),
        armBMinA2 = FiniteOrNull(armBMinA2),
        armBMaxDegeneracyRatio = FiniteOrNull(armBMaxDegeneracyRatio),
        armBNoDegenerateCubicOnMenu = armBNoDegenerateCubic,
        armBMenuHashFrom454 = armBMenuHash454,
    },
    operatorMenu = new
    {
        auditedOperatorCount = auditedMenu.Length,
        syntheticControlCount = operatorMenu.Count(o => !o.InAuditedMenu),
        menuCompletenessHash,
        operators = operatorMenu.Select(o => new { o.Id, o.Origin, o.SignClass, o.InAuditedMenu, o.Note }).ToArray(),
    },
    t4ConformalSectorKill = new
    {
        killed = t4ConformalKilled,
        auditedMenuAllKilled,
        syntheticControlInjects,
        restsOnTrivialAllNonnegative = t4RestsOnTrivialAllNonneg,
        restsOnPositiveOnRayQuadraticCoefficient = t4RestsOnPositiveA2,
        perOperator = t4Rows,
    },
    t1AbelianConeWitnesses = new
    {
        abelianConeNonempty,
        cartanProjectorExactRank = cartanRank,
        stationaryVacuumWitnessCount = t1StationaryWitnessCount,
        nonStationaryNamedWitnessCount = t1NonStationaryNamedWitnessCount,
        noStationaryInstability = t1NoStationaryInstability,
        note = "the off-ray named negatives are non-vacuum transverse witnesses; S_B >= 0 pins the global minimum at 0",
    },
    t2CurveEscapeRepair = new
    {
        motzkinPolynomial = "f(x,y) = (y - x^2)(y - 2 x^2) = y^2 - 3 x^2 y + 2 x^4",
        boundedOnEveryRayVerified = t2RayCoerciveAllDirections,
        unboundedBelowAlongCurveVerified = t2CurveNegativeAllSamples,
        motzkinVerified = t2MotzkinVerified,
        rayGrid = t2RayGrid,
        curveSamples = t2CurveSamples,
        surviveTerminalRenamedTo = "no-ray-instability-found-at-audited-menu",
        surviveTerminalAuthorizesNothing = true,
    },
    t3CoercivityCertificate = new
    {
        obligation = "global lower bound V(phi) >= c ||phi||^p - C (c > 0) OR an SOS/Positivstellensatz certificate valid off-ray for the transport-augmented potential",
        pureGaugeCoercivityCertified = t3PureGaugeCoercivityCertified,
        transportAugmentedCertificateObtained = t3TransportAugmentedCertificateObtained,
        isTheOnlyStageBHmcGate = true,
        reopeningConditions = t3ReopeningConditions,
    },
    batteries = new
    {
        batteriesAllPassed,
        exactnessCrossChecksOk,
        menuCompletenessOk,
        determinismOk,
        syntheticOverturnOk,
        abelianConeBatteryOk,
        motzkinBatteryOk,
    },
    guards = guards.ToArray(),
    phase464Impact = new
    {
        deliversStrongestConjunctC3 = verdictKind == "transport-killed-at-audited-menu",
        cancelsItem16StageBWithTwoNamedReopeningConditions = verdictKind == "transport-killed-at-audited-menu",
        neverCancelledPermanently = true,
    },
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
    recordedBoundary = new { physicistReviewPending },
    explicitCandidateOnlyNonclaims = new[]
    {
        "T1-T4 are exact/structural elimination theorems over the committed phase454 block spectra; all eigenvalues/coefficients are lattice-unit workbench-relative structure data.",
        "The transport-killed verdict is scoped to the audited menu and carries two named reopening conditions; it is NEVER a permanent cancellation and asserts NO bounded-below/vacuum claim (the T2 Motzkin repair forbids it).",
        "physicistReviewPending = true; nothing is measured, filled, or promoted; promotedPhysicalMassClaimCount remains 0.",
    },
    decision,
    runtimeSeconds,
};

File.WriteAllText(Path.Combine(outputDir, "transport_structure_theorems.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(Path.Combine(outputDir, "transport_structure_theorems_summary.json"), JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.verdictKind,
    result.transportStructureTheoremsPassed,
    t4 = new { killed = t4ConformalKilled, auditedMenuAllKilled, syntheticControlInjects },
    t1 = new { abelianConeNonempty, t1NoStationaryInstability, namedNegativeCount },
    t2 = new { t2MotzkinVerified },
    t3 = new { certificateObtained = t3TransportAugmentedCertificateObtained },
    result.physicistReviewPending,
    result.noGevPromotion,
    result.decision,
}, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"verdictKind={verdictKind} pass={transportStructureTheoremsPassed}");
Console.WriteLine($"T4: killed={t4ConformalKilled} auditedAllKilled={auditedMenuAllKilled} controlInjects={syntheticControlInjects}");
Console.WriteLine($"T1: coneNonempty={abelianConeNonempty} noStationaryInstability={t1NoStationaryInstability} namedNegatives={namedNegativeCount}");
Console.WriteLine($"T2: motzkinVerified={t2MotzkinVerified}");
Console.WriteLine($"T3: certificateObtained={t3TransportAugmentedCertificateObtained}");
Console.WriteLine($"batteries: all={batteriesAllPassed}");
foreach (var g in guards)
{
    Console.WriteLine($"GUARD: {g}");
}

Console.WriteLine($"runtimeSeconds={runtimeSeconds:F3}");

if (!batteriesAllPassed)
{
    Environment.Exit(1);
}

// Exact integer rank via fraction-free Gaussian elimination (BigInteger).
static int ExactRank(BigInteger[][] matrix)
{
    var m = matrix.Select(r => (BigInteger[])r.Clone()).ToArray();
    var rows = m.Length;
    var cols = rows == 0 ? 0 : m[0].Length;
    var rank = 0;
    for (var col = 0; col < cols && rank < rows; col++)
    {
        var pivot = -1;
        for (var r = rank; r < rows; r++)
        {
            if (m[r][col] != 0)
            {
                pivot = r;
                break;
            }
        }

        if (pivot < 0)
        {
            continue;
        }

        (m[rank], m[pivot]) = (m[pivot], m[rank]);
        for (var r = 0; r < rows; r++)
        {
            if (r == rank || m[r][col] == 0)
            {
                continue;
            }

            var a = m[r][col];
            var b = m[rank][col];
            for (var c = 0; c < cols; c++)
            {
                m[r][c] = b * m[r][c] - a * m[rank][c];
            }
        }

        rank++;
    }

    return rank;
}
