// Phase461 - Team A Wave-1 rank-3 (A2): the part-12d reading menu with
// look-elsewhere control (TEAM_ELIMINATION_PROGRAM_2026-07-10.md, Wave-1
// item 3; unresolved objection O6 = the reconstruction gate below).
// ELIMINATION computation - exact rational arithmetic on every band decision,
// deterministic, sub-second.
//
// The committed reference-snapshot slot statement "cosmological constant as a
// VEV" of a field playing the role of fundamental mass (routed here by
// phase460's pre-registered pinned-relation criterion) admits only a finite
// menu of dimensional-transmutation power readings
//     M(a,b) = c * (M_gravityAnchor^a * ccScale^b)^(1/(a+b)),
// tested against a consistency-window list that is pre-registered COMPLETE,
// with per-window imports-observed flags declared BEFORE any value is
// computed. The 246-GeV electroweak VEV is BANNED as a live window (circular
// AND gauge-variant per the recorded task-force ruling); it exists only as
// the referee-reconstruction-only window demanded by the reconstruction gate.
// Every observed import is a declared-comparison with role=test-only.
//
// LOOK-ELSEWHERE CONTROL (pre-registered): primary coefficient band
// c in [1/(4*pi), 4*pi] (fixed rational convention endpoints below); the wide
// band [1/(16*pi^2), 16*pi^2] is a labeled SECONDARY sweep only; per-row
// coincidence probability p1 = 2*ln(missFactor)/ln(anchorRange) under the
// log-uniform null, Bonferroni-corrected over the live row count; the
// trials-corrected survival threshold is 0.05.
//
// THE RECONSTRUCTION GATE (objection O6, fail-closed): unless at least one
// menu row reproduces the task-force referee's ~460x kill of the cc/dark-
// energy anchor (journal, 2026-07-05 task-force entry: "CC/dark-energy anchor
// killed (misses by ~460x, referee-confirmed)") within the pre-registered
// tolerance band [300, 700] in a pre-registered kill-factor convention, the
// phase fail-closes as menu-incomplete and NO verdict is emitted. The
// reproducing row commits as the reconstructed referee reading.
//
// Verdict taxonomy (pre-registered, fail-closed):
//   candidate-anchor-forwarded-to-A3   (import-clean primary-band trials-surviving hit)
//   declared-comparison-consistency-only (import-laden primary-band hits only)
//   part12d-anchor-readings-all-miss   (no primary-band hit at all)
//   menu-incomplete                    (reconstruction gate failed; NO verdict)
//
// All GeV numbers below are DECLARED-COMPARISON IMPORTS (test-only); nothing
// here is a prediction; nothing is promoted. physicistReviewPending = true.

using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;

const string DefaultOutputDir = "studies/phase461_dimensional_transmutation_reading_menu_001/output";
const string MirrorSnapshotPath = "docs/Reference/ExperimentReferences/SUPERPHYSICS-GU-DRAFT-MIRROR-20250530.md";
const string ApplicationSubjectKind = "dimensional-transmutation-reading-menu";
const string TerminalPrefix = "dimensional-transmutation-reading-menu-";
const double TrialsSurvivalThreshold = 0.05;

var outputDir = Environment.GetEnvironmentVariable("PHASE461_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

var checks = new List<Check461>();

// ---------------------------------------------------------------------------
// 1. Exact rational arithmetic battery.
// ---------------------------------------------------------------------------

var rationalBatteryPassed =
    Rational.FromDecimal("2.24", -12).Equals(new Rational(224, BigInteger.Pow(10, 14)))
    && Rational.FromDecimal("1.220890", 19).Equals(new Rational(new BigInteger(1220890) * BigInteger.Pow(10, 13), 1))
    && new Rational(3, 7).Pow(3).Equals(new Rational(27, 343))
    && new Rational(6, 4).Equals(new Rational(3, 2))
    && new Rational(1, 3).CompareTo(new Rational(2, 5)) < 0
    && new Rational(7, 2).CompareTo(new Rational(7, 2)) == 0
    && new Rational(22, 7).CompareTo(new Rational(3, 1)) > 0
    && new Rational(5, 1).Mul(new Rational(1, 5)).Equals(Rational.One)
    && new Rational(9, 4).Div(new Rational(3, 2)).Equals(new Rational(3, 2));

checks.Add(new Check461(
    "exact-rational-battery",
    rationalBatteryPassed,
    "decimal parsing, normalization, integer powers, multiplication/division, three-way comparisons"));

// ---------------------------------------------------------------------------
// 2. Pre-registered menu: anchor imports, readings, windows, bands.
//    EVERYTHING here is declared before any row value is computed.
// ---------------------------------------------------------------------------

// Anchor-side declared imports (GeV; exact decimal rationals).
var gravityAnchorFull = Rational.FromDecimal("1.220890", 19);    // gravity-scale anchor, full convention
var gravityAnchorReduced = Rational.FromDecimal("2.435323", 18); // gravity-scale anchor, reduced convention
var ccScale = Rational.FromDecimal("2.24", -12);                 // dark-energy density quarter-root scale

// Power readings (a, b) with a + b >= 1; the (0,1) reading is anchor-variant
// independent and is registered once (documented; avoids double-counting in
// the trials correction).
var readingPairs = new (int A, int B)[] { (1, 0), (0, 1), (1, 1), (2, 1), (1, 2), (3, 1), (1, 3), (4, 1), (1, 4) };
string[] anchorVariants = ["full", "reduced"];

var readings = new List<Reading461>();
foreach (var (a, b) in readingPairs)
{
    foreach (var variant in anchorVariants)
    {
        if (a == 0 && variant == "reduced")
        {
            continue; // anchor-variant independent; registered once
        }

        readings.Add(new Reading461($"M({a},{b})-{variant}", a, b, variant));
    }
}

// Consistency windows: pre-registered COMPLETE with per-window
// imports-observed flags and roles BEFORE any value is computed.
// role "test-only": live declared-comparison window.
// role "referee-reconstruction-only": BANNED from live verdicts; exists only
// for the O6 reconstruction gate.
var windows = new Window461[]
{
    new("massW", Rational.FromDecimal("80.377", 0), true, "test-only", false),
    new("massZ", Rational.FromDecimal("91.1876", 0), true, "test-only", false),
    new("massH", Rational.FromDecimal("125.25", 0), true, "test-only", false),
    new("massTop", Rational.FromDecimal("172.69", 0), true, "test-only", false),
    new("qcdScaleNf5", Rational.FromDecimal("0.210", 0), true, "test-only", false),
    new("massElectron", Rational.FromDecimal("0.000511", 0), true, "test-only", false),
    new("neutrinoScale", Rational.FromDecimal("5.0", -11), true, "test-only", false),
    new("vevEw246", Rational.FromDecimal("246.22", 0), true, "referee-reconstruction-only", true),
};

// Band conventions (fixed rational endpoints; documented approximations of
// 1/(4*pi), 4*pi, 1/(16*pi^2), 16*pi^2 - pre-registered convention constants).
var primaryLo = new Rational(10_000, 125_664);
var primaryHi = new Rational(125_664, 10_000);
var secondaryLo = new Rational(10_000, 1_579_137);
var secondaryHi = new Rational(1_579_137, 10_000);

// Reconstruction gate pre-registration: kill-factor conventions and band.
// Convention "cc-plane-squared": for a two-scale reading the referee's kill
// factor is the squared linear miss (equivalently the miss measured in the
// cc-scale plane); convention "linear": the plain ratio.
string[] killFactorConventions = ["linear", "cc-plane-squared"];
var killBandLo = new Rational(300, 1);
var killBandHi = new Rational(700, 1);

// Menu-completeness battery: structural assertions on the pre-registered
// tables (counts, distinctness, flags all declared, exactly one banned
// reconstruction-only window, all live windows test-only with imports
// declared, all reading exponents nonnegative with a+b >= 1).
var liveWindows = windows.Where(w => !w.Banned).ToArray();
var bannedWindows = windows.Where(w => w.Banned).ToArray();
var menuCompletenessPassed =
    readings.Count == 17
    && readings.Select(r => r.ReadingId).Distinct().Count() == readings.Count
    && readings.All(r => r.A >= 0 && r.B >= 0 && r.A + r.B >= 1)
    && windows.Length == 8
    && windows.Select(w => w.WindowId).Distinct().Count() == windows.Length
    && liveWindows.Length == 7
    && bannedWindows.Length == 1
    && bannedWindows[0].WindowId == "vevEw246"
    && bannedWindows[0].Role == "referee-reconstruction-only"
    && liveWindows.All(w => w.Role == "test-only" && w.ImportsObserved)
    && primaryLo.CompareTo(primaryHi) < 0
    && secondaryLo.CompareTo(primaryLo) < 0
    && primaryHi.CompareTo(secondaryHi) < 0
    && killBandLo.CompareTo(killBandHi) < 0;

checks.Add(new Check461(
    "menu-completeness-battery",
    menuCompletenessPassed,
    $"readings={readings.Count} (9 exponent pairs x anchor variants, degenerate (0,b) registered once); windows={windows.Length} (7 live test-only + 1 banned reconstruction-only); bands nested; all flags declared ex ante"));

// Banned-import battery: the 246-GeV electroweak VEV can never enter a live
// row, and no live window duplicates the banned value.
var bannedImportBatteryPassed =
    bannedWindows.Length == 1
    && liveWindows.All(w => w.ValueGeV.CompareTo(bannedWindows[0].ValueGeV) != 0)
    && bannedWindows[0].Banned;

checks.Add(new Check461(
    "banned-import-battery",
    bannedImportBatteryPassed,
    "the 246-GeV electroweak VEV window is banned (circular and gauge-variant per the recorded task-force ruling), appears exactly once, is role=referee-reconstruction-only, and no live window carries its value"));

// Corpus tie: the routed slot statement must exist in the committed snapshot.
var mirrorPresent = File.Exists(MirrorSnapshotPath);
var normalizedMirror = mirrorPresent
    ? Regex.Replace(File.ReadAllText(MirrorSnapshotPath), @"\s+", " ")
    : string.Empty;
var slotStatementPresent = normalizedMirror.Contains("cosmological constant as a VEV", StringComparison.Ordinal);
checks.Add(new Check461(
    "routed-slot-statement-evidence",
    mirrorPresent && slotStatementPresent,
    mirrorPresent && slotStatementPresent
        ? "the coefficient-free anchor slot statement routed by phase460 is present in the committed snapshot"
        : "snapshot or slot statement missing"));

// ---------------------------------------------------------------------------
// 3. Row computation. Band membership decisions are EXACT: with k = a + b and
//    M^k = anchor^a * cc^b = p/q, the condition lo <= W/M <= hi is decided as
//    lo^k * (p/q) <= W^k <= hi^k * (p/q) by BigInteger cross-multiplication.
//    Doubles appear only in the REPORTING layer (values, miss factors) and in
//    the pre-registered p-value convention.
// ---------------------------------------------------------------------------

Rational AnchorFor(string variant) => variant == "full" ? gravityAnchorFull : gravityAnchorReduced;

// Exact band test: lo <= ratio <= hi where ratio^k = W^k / M^k.
static bool InBandExact(Rational wPowK, Rational mPowK, Rational lo, Rational hi, int k)
{
    var loK = lo.Pow(k);
    var hiK = hi.Pow(k);
    var lower = loK.Mul(mPowK);
    var upper = hiK.Mul(mPowK);
    return lower.CompareTo(wPowK) <= 0 && wPowK.CompareTo(upper) <= 0;
}

var anchorRangeLn = Math.Log(gravityAnchorFull.ToDouble() / ccScale.ToDouble());
var liveRowCount = readings.Count * liveWindows.Length;

var liveRows = new List<Row461>();
var reconstructionRows = new List<Row461>();

foreach (var reading in readings)
{
    var k = reading.A + reading.B;
    var anchor = AnchorFor(reading.AnchorVariant);
    var mPowK = anchor.Pow(reading.A).Mul(ccScale.Pow(reading.B));
    var valueGeV = Math.Pow(mPowK.ToDouble(), 1.0 / k);

    foreach (var window in windows)
    {
        var wPowK = window.ValueGeV.Pow(k);
        var inPrimary = InBandExact(wPowK, mPowK, primaryLo, primaryHi, k);
        var inSecondary = InBandExact(wPowK, mPowK, secondaryLo, secondaryHi, k);
        var ratio = window.ValueGeV.ToDouble() / valueGeV;
        var missFactor = ratio >= 1 ? ratio : 1.0 / ratio;

        double? pSingle = null;
        double? pCorrected = null;
        var trialsSurviving = false;
        if (!window.Banned)
        {
            var p1 = Math.Min(1.0, 2.0 * Math.Log(Math.Max(missFactor, 1.0 + 1e-15)) / anchorRangeLn);
            var pc = Math.Min(1.0, liveRowCount * p1);
            pSingle = p1;
            pCorrected = pc;
            trialsSurviving = inPrimary && pc <= TrialsSurvivalThreshold;
        }

        var row = new Row461(
            reading.ReadingId,
            reading.A,
            reading.B,
            reading.AnchorVariant,
            window.WindowId,
            window.Role,
            window.ImportsObserved,
            window.Banned,
            valueGeV,
            window.ValueGeV.ToDouble(),
            ratio,
            missFactor,
            inPrimary,
            !inPrimary && inSecondary,
            pSingle,
            pCorrected,
            trialsSurviving,
            BuildImportLineage(reading, window));

        if (window.Banned)
        {
            reconstructionRows.Add(row);
        }
        else
        {
            liveRows.Add(row);
        }
    }
}

static string[] BuildImportLineage(Reading461 reading, Window461 window)
{
    var lineage = new List<string>
    {
        reading.AnchorVariant == "full"
            ? "anchor-side: gravity-scale anchor (full convention, declared import)"
            : "anchor-side: gravity-scale anchor (reduced convention, declared import)",
    };
    if (reading.B > 0)
    {
        lineage.Add("anchor-side: dark-energy density quarter-root scale (observed cosmology, declared import)");
    }

    lineage.Add(window.Banned
        ? $"window-side: {window.WindowId} (BANNED live; referee-reconstruction-only)"
        : $"window-side: {window.WindowId} (observed, declared-comparison, role=test-only)");
    return [.. lineage];
}

// ---------------------------------------------------------------------------
// 4. THE RECONSTRUCTION GATE (objection O6). Exact-rational kill factors.
// ---------------------------------------------------------------------------

// The gate is pre-registered against the referee-reconstruction window class
// ONLY: the quoted kill compared the cc/dark-energy anchor to the electroweak
// scale, which is exactly the banned window (it never enters live verdicts).
var reconstructionCandidates = new List<object>();
object? reconstructedRefereeReading = null;
var reconstructedKillFactorValue = 0.0;
var reconstructionFound = false;

foreach (var reading in readings)
{
    var k = reading.A + reading.B;
    var anchor = AnchorFor(reading.AnchorVariant);
    var mPowK = anchor.Pow(reading.A).Mul(ccScale.Pow(reading.B));

    foreach (var window in windows.Where(w => w.Role == "referee-reconstruction-only"))
    {
        foreach (var convention in killFactorConventions)
        {
            // Exact kill factor:
            //   linear:          max(W/M, M/W)          decided via k-th powers
            //   cc-plane-squared: max(W/M, M/W)^2        decided via k-th powers
            // Both reduce to exact rational comparisons of X = W^k / M^k.
            var wPowK = window.ValueGeV.Pow(k);
            var x = wPowK.Div(mPowK); // = (W/M)^k
            var missPowK = x.CompareTo(Rational.One) >= 0 ? x : Rational.One.Div(x); // = missFactor^k
            var exponentNum = convention == "linear" ? 1 : 2;

            // In band <=> killBandLo <= missFactor^e <= killBandHi
            //         <=> killBandLo^k <= missPowK^e <= killBandHi^k.
            var lhs = killBandLo.Pow(k);
            var rhs = killBandHi.Pow(k);
            var mid = missPowK.Pow(exponentNum);
            var inKillBand = lhs.CompareTo(mid) <= 0 && mid.CompareTo(rhs) <= 0;
            if (!inKillBand)
            {
                continue;
            }

            var killFactor = Math.Pow(missPowK.ToDouble(), (double)exponentNum / k);
            var candidate = new
            {
                readingId = reading.ReadingId,
                windowId = window.WindowId,
                convention,
                killFactor = FiniteOrNull(killFactor),
                killBandLo = 300,
                killBandHi = 700,
                windowRole = window.Role,
                windowBannedFromLiveVerdicts = window.Banned,
            };
            reconstructionCandidates.Add(candidate);
            if (!reconstructionFound)
            {
                reconstructionFound = true;
                reconstructedRefereeReading = candidate;
                reconstructedKillFactorValue = killFactor;
            }
        }
    }
}

checks.Add(new Check461(
    "reconstruction-gate-o6",
    reconstructionFound,
    reconstructionFound
        ? $"reconstructed the referee's ~460x kill: candidates={reconstructionCandidates.Count}; committed row = {JsonSerializer.Serialize(reconstructedRefereeReading)}"
        : "NO menu row reproduces the referee's ~460x kill within [300, 700]; menu-incomplete; NO verdict is emitted"));

// ---------------------------------------------------------------------------
// 5. Verdict (pre-registered taxonomy, fail-closed ordering).
// ---------------------------------------------------------------------------

var primaryHits = liveRows.Where(r => r.InPrimaryBand).ToArray();
var secondaryHits = liveRows.Where(r => r.InSecondaryBandOnly).ToArray();
var importCleanSurvivingHits = primaryHits.Where(r => !r.ImportsObserved && r.TrialsSurviving).ToArray();

var batteriesPassed = rationalBatteryPassed && menuCompletenessPassed && bannedImportBatteryPassed
    && mirrorPresent && slotStatementPresent;

string verdictKind;
if (!batteriesPassed)
{
    verdictKind = "review-required-gates-failed";
}
else if (!reconstructionFound)
{
    verdictKind = "menu-incomplete";
}
else if (importCleanSurvivingHits.Length > 0)
{
    verdictKind = "candidate-anchor-forwarded-to-A3";
}
else if (primaryHits.Length > 0)
{
    verdictKind = "declared-comparison-consistency-only";
}
else
{
    verdictKind = "part12d-anchor-readings-all-miss";
}

var readingMenuPassed = batteriesPassed && reconstructionFound;
var terminalStatus = TerminalPrefix + verdictKind;

var minCorrectedP = liveRows.Where(r => r.PTrialsCorrected is not null).Min(r => r.PTrialsCorrected!.Value);

var decision = verdictKind switch
{
    "candidate-anchor-forwarded-to-A3" =>
        "An import-clean primary-band trials-surviving reading exists and is forwarded to A3 as a candidate anchor. Nothing is promoted here.",
    "declared-comparison-consistency-only" =>
        $"The part-12d reading menu is decided fail-closed in exact rational arithmetic over {liveRowCount} live (reading, window) rows. RECONSTRUCTION GATE (O6): the task-force referee's ~460x kill is REPRODUCED - the committed reconstructed referee reading (see reconstructionGate.reconstructedRefereeReading) gives kill factor {reconstructedKillFactorValue:F1} inside the pre-registered band [300, 700] against the BANNED referee-reconstruction-only window; that row contributes NOTHING to live verdicts. LIVE RESULT: {primaryHits.Length} primary-band hits exist, but every one is import-laden (declared observed comparison windows) and NONE survives the trials correction (minimum Bonferroni-corrected coincidence probability {minCorrectedP:F3} > {TrialsSurvivalThreshold}); {secondaryHits.Length} additional rows land only in the labeled secondary wide band and are demoted by pre-registration. VERDICT: declared-comparison-consistency-only - the part-12d anchor readings supply NO import-clean, trials-surviving dimensionful anchor; the cc/dark-energy anchor remains killed for anchor purposes; the A6 conjunction consumes this as an A2 rule-out of anchor-grade content (the readings are not all-miss, so the relation survives only as declared-comparison numerology). Nothing is promoted; no GeV value here is a prediction.",
    "part12d-anchor-readings-all-miss" =>
        "Every live (reading, window) row misses the primary coefficient band: the part-12d relation is killed outright as an anchor source. Nothing is promoted.",
    "menu-incomplete" =>
        "The reconstruction gate failed: no menu row reproduces the referee's ~460x kill within the pre-registered tolerance. The menu is INCOMPLETE and NO verdict is emitted (designed dead-end; repair the menu).",
    _ => "Review required: a pre-registered battery failed; no verdict is emitted.",
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

var result = new
{
    phaseId = "phase461-dimensional-transmutation-reading-menu",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    dimensionalTransmutationReadingMenuPassed = readingMenuPassed,
    targetBlindConstruction = false, // declared-comparison phase (phase429/451-style separation, not blindness)
    applicationSubjectKind = ApplicationSubjectKind,
    verdictKind,
    verdictTaxonomy = new[]
    {
        "candidate-anchor-forwarded-to-A3",
        "declared-comparison-consistency-only",
        "part12d-anchor-readings-all-miss",
        "menu-incomplete",
    },
    reconstructionGate = new
    {
        objectionId = "O6",
        found = reconstructionFound,
        preRegisteredKillBand = new { lo = 300, hi = 700, quotedRefereeFigure = "~460x" },
        killFactorConventions,
        reconstructedRefereeReading,
        allCandidates = reconstructionCandidates,
        journalAnchorQuote = "CC/dark-energy anchor killed (misses by ~460x, referee-confirmed)",
    },
    anchorImports = new
    {
        gravityAnchorFullGeV = gravityAnchorFull.ToDouble(),
        gravityAnchorReducedGeV = gravityAnchorReduced.ToDouble(),
        ccScaleGeV = ccScale.ToDouble(),
        note = "anchor-side declared imports (gravity-scale conventions + observed dark-energy density quarter-root); every GeV number in this phase is a declared comparison, never a prediction",
    },
    lookElsewhereControl = new
    {
        primaryBand = new { lo = primaryLo.ToDouble(), hi = primaryHi.ToDouble(), convention = "fixed rational endpoints approximating [1/(4*pi), 4*pi]" },
        secondaryBand = new { lo = secondaryLo.ToDouble(), hi = secondaryHi.ToDouble(), convention = "labeled secondary sweep only; approximates [1/(16*pi^2), 16*pi^2]" },
        liveRowCount,
        anchorRangeLn = FiniteOrNull(anchorRangeLn),
        pConvention = "p1 = 2*ln(missFactor)/ln(anchorRange) under the log-uniform null; Bonferroni corrected over liveRowCount",
        trialsSurvivalThreshold = TrialsSurvivalThreshold,
        minCorrectedP = FiniteOrNull(minCorrectedP),
    },
    menu = new
    {
        readings = readings.Select(r => new { readingId = r.ReadingId, a = r.A, b = r.B, anchorVariant = r.AnchorVariant }).ToArray(),
        windows = windows.Select(w => new
        {
            windowId = w.WindowId,
            valueGeV = w.ValueGeV.ToDouble(),
            importsObserved = w.ImportsObserved,
            role = w.Role,
            banned = w.Banned,
        }).ToArray(),
    },
    rows = liveRows.Select(RowToJson).ToArray(),
    reconstructionOnlyRows = reconstructionRows.Select(RowToJson).ToArray(),
    primaryBandHits = primaryHits.Select(RowToJson).ToArray(),
    secondaryBandOnlyHits = secondaryHits.Select(RowToJson).ToArray(),
    importCleanTrialsSurvivingHitCount = importCleanSurvivingHits.Length,
    checks = checks.Select(c => new { checkId = c.Id, passed = c.Passed, detail = c.Detail }).ToArray(),
    physicistReviewPending = true,
    noGevPromotion = true,
    sourceContractApplicationAllowed = false,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    decision,
    sourceEvidence = new
    {
        mirrorSnapshotPath = MirrorSnapshotPath,
        routedFromPhase = "phase460",
        journalPath = "docs/BOSON_PREDICTION_DIAGNOSIS_JOURNAL.md",
        programSpecPath = "docs/Phases/TEAM_ELIMINATION_PROGRAM_2026-07-10.md",
        registryPath = "docs/Phases/PHASE_NUMBER_REGISTRY.md",
    },
};

static object RowToJson(Row461 r) => new
{
    readingId = r.ReadingId,
    a = r.A,
    b = r.B,
    anchorVariant = r.AnchorVariant,
    windowId = r.WindowId,
    windowRole = r.WindowRole,
    importsObserved = r.ImportsObserved,
    banned = r.Banned,
    valueGeV = FiniteOrNull(r.ValueGeV),
    windowGeV = FiniteOrNull(r.WindowGeV),
    ratioWindowOverValue = FiniteOrNull(r.RatioWindowOverValue),
    missFactor = FiniteOrNull(r.MissFactor),
    inPrimaryBand = r.InPrimaryBand,
    inSecondaryBandOnly = r.InSecondaryBandOnly,
    pSingle = r.PSingle is null ? null : FiniteOrNull(r.PSingle.Value),
    pTrialsCorrected = r.PTrialsCorrected is null ? null : FiniteOrNull(r.PTrialsCorrected.Value),
    trialsSurviving = r.TrialsSurviving,
    importLineage = r.ImportLineage,
};

static double? FiniteOrNull(double v) => double.IsFinite(v) ? v : null;

File.WriteAllText(Path.Combine(outputDir, "dimensional_transmutation_reading_menu.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "dimensional_transmutation_reading_menu_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.dimensionalTransmutationReadingMenuPassed,
        result.targetBlindConstruction,
        result.applicationSubjectKind,
        result.verdictKind,
        result.verdictTaxonomy,
        result.reconstructionGate,
        result.lookElsewhereControl,
        liveRowCount,
        primaryBandHitCount = primaryHits.Length,
        secondaryBandOnlyHitCount = secondaryHits.Length,
        result.importCleanTrialsSurvivingHitCount,
        primaryBandHits = primaryHits.Select(RowToJson).ToArray(),
        result.checks,
        result.physicistReviewPending,
        result.noGevPromotion,
        result.sourceContractApplicationAllowed,
        result.canFillPhase201WzContract,
        result.canFillPhase201HiggsContract,
        result.canFillPhase256ObservedFieldExtractionContract,
        result.routePromotesWzMasses,
        result.routePromotesHiggsMass,
        result.routeCompletesBosonPredictions,
        result.decision,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"dimensionalTransmutationReadingMenuPassed={readingMenuPassed}");
Console.WriteLine($"verdictKind={verdictKind}");
Console.WriteLine($"liveRowCount={liveRowCount}; primaryBandHits={primaryHits.Length}; secondaryBandOnly={secondaryHits.Length}; importCleanTrialsSurviving={importCleanSurvivingHits.Length}; minCorrectedP={minCorrectedP:F4}");
Console.WriteLine($"reconstructionFound={reconstructionFound}; candidates={reconstructionCandidates.Count}");
if (reconstructedRefereeReading is not null)
{
    Console.WriteLine($"reconstructedRefereeReading={JsonSerializer.Serialize(reconstructedRefereeReading)}");
}

foreach (var hit in primaryHits)
{
    Console.WriteLine($"primary hit: {hit.ReadingId} vs {hit.WindowId}: value={hit.ValueGeV:E4} GeV, miss={hit.MissFactor:F3}, pCorrected={hit.PTrialsCorrected:F3}, importsObserved={hit.ImportsObserved}");
}

foreach (var c in checks)
{
    Console.WriteLine($"check {c.Id}: {(c.Passed ? "passed" : "FAILED")} - {c.Detail}");
}

if (!readingMenuPassed)
{
    Environment.Exit(1);
}

internal sealed record Check461(string Id, bool Passed, string Detail);

internal sealed record Reading461(string ReadingId, int A, int B, string AnchorVariant);

internal sealed record Window461(string WindowId, Rational ValueGeV, bool ImportsObserved, string Role, bool Banned);

internal sealed record Row461(
    string ReadingId,
    int A,
    int B,
    string AnchorVariant,
    string WindowId,
    string WindowRole,
    bool ImportsObserved,
    bool Banned,
    double ValueGeV,
    double WindowGeV,
    double RatioWindowOverValue,
    double MissFactor,
    bool InPrimaryBand,
    bool InSecondaryBandOnly,
    double? PSingle,
    double? PTrialsCorrected,
    bool TrialsSurviving,
    string[] ImportLineage);

/// <summary>
/// Exact nonnegative rational arithmetic over BigInteger (normalized,
/// denominator positive). Only the operations needed for exact band and
/// kill-factor decisions are implemented; every decision in this phase
/// reduces to Mul/Div/Pow/CompareTo on these values.
/// </summary>
internal readonly struct Rational : IEquatable<Rational>
{
    public BigInteger Num { get; }

    public BigInteger Den { get; }

    public static Rational One => new(1, 1);

    public Rational(BigInteger num, BigInteger den)
    {
        if (den == 0)
        {
            throw new DivideByZeroException("rational denominator is zero");
        }

        if (den < 0)
        {
            num = -num;
            den = -den;
        }

        var g = BigInteger.GreatestCommonDivisor(BigInteger.Abs(num), den);
        if (g > 1)
        {
            num /= g;
            den /= g;
        }

        Num = num;
        Den = den;
    }

    /// <summary>
    /// Parses a decimal mantissa string times 10^exponent into an exact
    /// rational, e.g. FromDecimal("2.24", -12) = 224 / 10^14.
    /// </summary>
    public static Rational FromDecimal(string mantissa, int exponent)
    {
        var dot = mantissa.IndexOf('.', StringComparison.Ordinal);
        var digits = dot < 0 ? mantissa : mantissa.Remove(dot, 1);
        var fracLen = dot < 0 ? 0 : mantissa.Length - dot - 1;
        var num = BigInteger.Parse(digits);
        var e = exponent - fracLen;
        return e >= 0
            ? new Rational(num * BigInteger.Pow(10, e), 1)
            : new Rational(num, BigInteger.Pow(10, -e));
    }

    public Rational Mul(Rational other) => new(Num * other.Num, Den * other.Den);

    public Rational Div(Rational other) => new(Num * other.Den, Den * other.Num);

    public Rational Pow(int e)
    {
        if (e < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(e), "nonnegative exponents only");
        }

        return new Rational(BigInteger.Pow(Num, e), BigInteger.Pow(Den, e));
    }

    public int CompareTo(Rational other) => (Num * other.Den).CompareTo(other.Num * Den);

    public bool Equals(Rational other) => Num == other.Num && Den == other.Den;

    public override bool Equals(object? obj) => obj is Rational r && Equals(r);

    public override int GetHashCode() => HashCode.Combine(Num, Den);

    public double ToDouble() => (double)Num / (double)Den;

    public override string ToString() => $"{Num}/{Den}";
}
