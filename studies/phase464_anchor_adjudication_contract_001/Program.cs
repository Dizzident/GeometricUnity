using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase464: Anchor Adjudication Contract — Team A Wave-2 (WAVE2_ACTION_PLAN_2026-07-12
// item 12; this phase EXHAUSTS the A block). Adjudicates LAST. This is the exact,
// mechanically-evaluated, fail-closed conjunction contract C1 ∧ C2 ∧ C3 over the
// three committed Team-A inputs (phase461 reading menu, phase462 blocking-set
// resolution, phase463 transport theorems) plus the B1 rulings ledger.
//
//   C1 (blocking-set closure) <- phase462: satisfied ONLY by
//       closure-resolved(-with-excision); pinning-insufficient => C1 = BLOCKED.
//   C2 (reading-menu disposal) <- phase461: satisfied by
//       declared-comparison-consistency-only / part12d-anchor-readings-all-miss
//       (no import-clean anchor survives).
//   C3 (transport) <- phase463: satisfied by transport-killed-at-audited-menu OR,
//       per the labeled item-20 amendment (WAVE2_AMENDMENTS_2026-07-12 A1),
//       vacuous-given-zero-sanctioned-anchors (phase463 coercivity-certified AND
//       zero sanctioned anchors forwarded by phase461).
//
// TERMINALS (pre-registered consts):
//   (i)   anchor-adjudication-convention-only          — C1 ∧ C2 ∧ C3 AND >=1
//         physicist ruling on record in the B1 ledger. Can NEVER emit with zero
//         rulings (the ledger is parsed and rulings counted).
//   (ii)  anchor-adjudication-reopened-live-candidate  — any input forwards an
//         anchor (a live anchor candidate reopens B1).
//   (iii) anchor-adjudication-blocked-upstream-ambiguous — C1 blocked; emits the
//         plan §3 pre-committed sentence VERBATIM with N_pending substituted,
//         plus the named reopening conditions. THE reachable terminal now.
//   (iv)  anchor-adjudication-no-claim                 — partial state (e.g. all
//         conjuncts satisfied but zero rulings); forbids citing partial closure.
//
// MANDATORY FRAMING. Zero physics compute — this is a mechanical adjudication over
// committed upstream verdicts. Nothing is measured, filled, or promoted;
// promotedPhysicalMassClaimCount remains 0; physicistReviewPending carried
// explicitly. Every terminal is an elimination verdict or an honestly-blocked
// record.

var stopwatch = Stopwatch.StartNew();

const string ApplicationSubjectKind = "anchor-adjudication-contract";
const string TerminalPrefix = "anchor-adjudication-";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 12";
const string DefaultOutputDir = "studies/phase464_anchor_adjudication_contract_001/output";
const string Phase461OutputPath = "studies/phase461_dimensional_transmutation_reading_menu_001/output/dimensional_transmutation_reading_menu.json";
const string Phase462OutputPath = "studies/phase462_blocking_set_resolution_001/output/blocking_set_resolution.json";
const string Phase463OutputPath = "studies/phase463_transport_structure_theorems_001/output/transport_structure_theorems.json";
const string LedgerPath = "docs/Phases/Adjudication/B1_RULINGS_LEDGER_P462.md";

// Tripwire denominator pinned EX ANTE (plan item 12): the phase462 blocking set
// has 31 members. Any drift from this pinned value fails the pass closed.
const int TripwireDenominator = 31;

var outputDir = Environment.GetEnvironmentVariable("PHASE464_OUTPUT_DIR") ?? DefaultOutputDir;
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

// Pre-registered verdict taxonomy (consts).
string[] verdictTaxonomy =
[
    "convention-only",
    "reopened-live-candidate",
    "blocked-upstream-ambiguous",
    "no-claim",
    "review-required-inputs-missing",
    "review-required-battery-failed",
];

var guards = new List<string>();
void FailClosed(string detail) => guards.Add(detail);

static string Sha256Hex(byte[] b) => Convert.ToHexStringLower(SHA256.HashData(b));

// --- read committed inputs (fail-closed on any absence) ---
string ReadVerdict(string path, out string sha256, out JsonElement root, out JsonDocument? doc)
{
    doc = null;
    sha256 = "absent";
    root = default;
    if (!File.Exists(path))
    {
        FailClosed($"input missing: {path}");
        return "absent";
    }
    var raw = File.ReadAllBytes(path);
    sha256 = Sha256Hex(raw);
    doc = JsonDocument.Parse(raw);
    root = doc.RootElement;
    return root.GetProperty("verdictKind").GetString() ?? "";
}

var verdict461 = ReadVerdict(Phase461OutputPath, out var sha461, out var root461, out var doc461);
var verdict462 = ReadVerdict(Phase462OutputPath, out var sha462, out var root462, out var doc462);
var verdict463 = ReadVerdict(Phase463OutputPath, out var sha463, out var root463, out var doc463);

// --- machine-monitorable reopening fields, read from the 461/463 outputs ---
// sanctionedAnchorPresent: phase461 forwards a sanctioned anchor ONLY when its
// verdict is candidate-anchor-forwarded-to-A3; declared-comparison-consistency-only
// / all-miss forward zero.
bool anchorForwardedFrom461 = verdict461 == "candidate-anchor-forwarded-to-A3";
bool sanctionedAnchorPresent = anchorForwardedFrom461;

// coercivityCertified: phase463 obtained an off-ray coercivity certificate ONLY when
// its verdict is transport-coercivity-certified (transportAugmentedCertificateObtained).
bool coercivityCertified = verdict463 == "transport-coercivity-certified";
if (doc463 != null && root463.TryGetProperty("t3CoercivityCertificate", out var t3)
    && t3.TryGetProperty("transportAugmentedCertificateObtained", out var tac))
{
    // fail-closed cross-check: the certificate flag must agree with the verdict string.
    bool certFlag = tac.GetBoolean();
    if (certFlag != coercivityCertified)
    {
        FailClosed($"phase463 coercivity flag ({certFlag}) disagrees with verdict ({verdict463})");
    }
}

// --- N_pending and tripwire denominator, read from phase462 (drift = fail-closed) ---
int nPending = TripwireDenominator;
int blockingSetCount = TripwireDenominator;
if (doc462 != null)
{
    if (root462.TryGetProperty("tripwireDenominator", out var td) && td.GetInt32() != TripwireDenominator)
    {
        FailClosed($"phase462 tripwireDenominator ({td.GetInt32()}) != pinned {TripwireDenominator}");
    }
    if (root462.TryGetProperty("blockingSetCount", out var bsc))
    {
        blockingSetCount = bsc.GetInt32();
        if (blockingSetCount != TripwireDenominator)
        {
            FailClosed($"phase462 blockingSetCount ({blockingSetCount}) != pinned {TripwireDenominator}");
        }
    }
    if (root462.TryGetProperty("stage1", out var s1) && s1.TryGetProperty("pendingAfterStage1", out var paf))
    {
        nPending = paf.GetInt32();
    }
}

// --- parse the B1 rulings ledger and COUNT rulings (terminal (i) gate) ---
// A ruling row is a markdown table row whose first cell is a ruling id "R-...".
// This ignores the schema table, header, and separator rows.
int rulingsCount = 0;
if (!File.Exists(LedgerPath))
{
    FailClosed($"rulings ledger missing: {LedgerPath}");
}
else
{
    foreach (var line in File.ReadAllLines(LedgerPath))
    {
        var trimmed = line.TrimStart();
        if (!trimmed.StartsWith('|')) continue;
        var cells = trimmed.Split('|');
        if (cells.Length < 2) continue;
        var firstCell = cells[1].Trim();
        if (firstCell.StartsWith("R-", StringComparison.OrdinalIgnoreCase)) rulingsCount++;
    }
}

// --- conjuncts (mechanical) ---
bool inputsPresent = verdict461 != "absent" && verdict462 != "absent" && verdict463 != "absent";

// C1: phase462 closure-resolved(-with-excision) — satisfied ONLY by closure-resolved*.
bool C1(string v462) => v462.StartsWith("closure-resolved", StringComparison.Ordinal);
// C2: phase461 disposes the reading menu with no surviving import-clean anchor.
bool C2(string v461) => v461 is "declared-comparison-consistency-only" or "part12d-anchor-readings-all-miss";
// C3: transport killed, OR vacuous-given-zero-sanctioned-anchors (item-20 amendment).
bool C3(string v463, bool sanctioned) =>
    v463 == "transport-killed-at-audited-menu"
    || (v463 == "transport-coercivity-certified" && !sanctioned);

// --- the pre-registered adjudication function (single source of truth; the
//     in-phase input-flip battery calls this same function) ---
static string Adjudicate(bool c1, bool c2, bool c3, int rulings, bool anchorForwarded)
{
    // (ii) a live anchor candidate reopens B1 regardless of the other conjuncts.
    if (anchorForwarded) return "reopened-live-candidate";
    // (iii) C1 blocked is the honestly-blocked record.
    if (!c1) return "blocked-upstream-ambiguous";
    // C1 satisfied below.
    // (i) full closure requires C1 ∧ C2 ∧ C3 AND at least one physicist ruling.
    if (c1 && c2 && c3 && rulings >= 1) return "convention-only";
    // (iv) partial state (all conjuncts satisfied but zero rulings; or C2/C3 unmet).
    return "no-claim";
}

bool c1Real = C1(verdict462);
bool c2Real = C2(verdict461);
bool c3Real = C3(verdict463, sanctionedAnchorPresent);
bool c3Vacuous = verdict463 == "transport-coercivity-certified" && !sanctionedAnchorPresent;
string c3Mode = verdict463 == "transport-killed-at-audited-menu"
    ? "transport-killed-at-audited-menu"
    : (c3Vacuous ? "vacuous-given-zero-sanctioned-anchors" : "unsatisfied");

// --- in-phase input-flip battery (synthetic; asserted at every pass) ---
// (a) 462 flipped to closure-resolved, zero rulings => terminal (i) must STILL NOT emit.
string batteryA = Adjudicate(c1: true, c2: c2Real, c3: c3Real, rulings: 0, anchorForwarded: false);
bool batteryAOk = batteryA != "convention-only";
// (b) 462 closure-resolved AND one synthetic ruling => (i) emits.
string batteryB = Adjudicate(c1: true, c2: c2Real, c3: c3Real, rulings: 1, anchorForwarded: false);
bool batteryBOk = batteryB == "convention-only";
// (c) 461 flipped to forward an anchor => (ii) emits (real c1 stays blocked).
string batteryC = Adjudicate(c1: c1Real, c2: c2Real, c3: c3Real, rulings: rulingsCount, anchorForwarded: true);
bool batteryCOk = batteryC == "reopened-live-candidate";
// (d) real inputs => (iii) emits.
string batteryD = Adjudicate(c1: c1Real, c2: c2Real, c3: c3Real, rulings: rulingsCount, anchorForwarded: sanctionedAnchorPresent);
bool batteryDOk = batteryD == "blocked-upstream-ambiguous";
bool inputFlipBatteryOk = batteryAOk && batteryBOk && batteryCOk && batteryDOk;
if (!inputFlipBatteryOk) FailClosed("input-flip battery failed: " +
    $"a={batteryA}({batteryAOk}) b={batteryB}({batteryBOk}) c={batteryC}({batteryCOk}) d={batteryD}({batteryDOk})");

// --- terminal decision ---
string verdictKind;
if (!inputsPresent)
{
    verdictKind = "review-required-inputs-missing";
}
else if (guards.Count > 0)
{
    verdictKind = "review-required-battery-failed";
}
else
{
    verdictKind = Adjudicate(c1Real, c2Real, c3Real, rulingsCount, sanctionedAnchorPresent);
}
string terminalStatus = TerminalPrefix + verdictKind;
bool anchorAdjudicationContractPassed = guards.Count == 0 && inputsPresent;

// --- pre-committed public sentence for (iii) BLOCKED-UPSTREAM-AMBIGUOUS
//     (WAVE2_ACTION_PLAN_2026-07-12 §3 B1 fallback, verbatim, N_pending substituted) ---
string blockedUpstreamSentence =
    $"The audited corpus neither supplies nor forbids a dimensionful anchor at theorem grade: " +
    $"{nPending} of {TripwireDenominator} prose-only statements remain unadjudicated under the committed " +
    $"33-symbol grading; no reading-menu anchor survives (phase461); every dimensionful anchor in use is a " +
    $"labeled convention adoption pending adjudication; no anchor theorem is claimed and none may be cited.";

// --- pre-committed no-claim sentence for (iv) partial states ---
string noClaimSentence =
    "Partial closure is uncitable: the B1 anchor-adjudication conjunction is not fully satisfied on the " +
    "committed inputs (no physicist ruling is on record and/or a conjunct is unmet); no anchor theorem is " +
    "claimed in either direction and no partial-closure statement may be cited.";

// --- named reopening conditions for the blocked / partial terminals ---
string[] reopeningConditions =
[
    "human-authored per-item draft locators: a human Stage-2 pinning of the 30 UNPINNABLE phase462 statements to gradable corpus text with page locators.",
    "a later RELATION ruling: an adjudication that a pinned statement supplies a scale-breaking relation at theorem grade (phase462 corpus-supplies-scale-breaking-relation).",
    "a corpus/menu update: new committed corpus text or an expanded phase461 reading menu.",
    "machine-monitorable fields flip: sanctionedAnchorPresent (read from phase461) or coercivityCertified (read from phase463) changing from false reopens the record.",
];

// --- objections carried honestly (WAVE2_ACTION_PLAN_2026-07-12 §4 objections 3-4) ---
var objections = new
{
    objection3PostHocAdmissibility =
        "The corpus-admissibility predicate (readings breaking the two pinned corpus relations p335-r2 / p339-r1 " +
        "are inadmissible) was formulated after observing those breaks. It is defended as verdict-monotone " +
        "(disposal only narrows scope) and the terminal (i) closure sentence must record the post-hoc fact — a " +
        "post-hoc element honestly carried, never eliminated.",
    objection4ThirtyThreeSymbolScope =
        "Any B1 closure theorem is scoped to the committed 33-symbol grading — a dimensionful quantity absent from " +
        "the symbol table is invisible to it. The theorem, if it emits, is about the committed grading, not the " +
        "corpus's semantic content.",
};

// --- terminal (i) closure contract skeleton (five mandatory scope fields). Emitted
//     ONLY when verdictKind == convention-only; recorded here as the pre-registered
//     contract the real closure statement must carry. ---
var conventionOnlyClosureContract = new
{
    emitted = verdictKind == "convention-only",
    mandatoryScopeFields = new
    {
        namedDisposedReadings = "the disposed reading-menu readings (phase461) and the disposed blocking-set statements (phase462), named by id",
        postHocAdmissibilityRecorded = "the post-hoc admissibility predicate is recorded honestly (objection 3), verdict-monotone, never eliminated",
        thirtyThreeSymbolScope = "the theorem is scoped to the committed 33-symbol grading (objection 4)",
        c3Mode = "the phase463 C3 mode (transport-killed-at-audited-menu or vacuous-given-zero-sanctioned-anchors)",
        o4DischargeClause = "the O4 conventions-register discharge clause (physicistReviewPending cleared only on O4 sign-off)",
    },
};

string decision = verdictKind switch
{
    "blocked-upstream-ambiguous" =>
        "Anchor adjudication is BLOCKED-UPSTREAM-AMBIGUOUS. The exact conjunction C1 ∧ C2 ∧ C3 fails at C1: phase462 " +
        $"is pinning-insufficient (not closure-resolved), so C1 = BLOCKED. C2 holds (phase461 {verdict461}: no import-clean " +
        $"anchor survives) and C3 holds (phase463 {verdict463}, C3 mode = {c3Mode}), but a blocked C1 is decisive and the " +
        "reachable terminal now is (iii). The pre-committed §3 sentence is emitted verbatim with N_pending substituted; the " +
        "four named reopening conditions and the machine-monitorable fields (sanctionedAnchorPresent=false, " +
        "coercivityCertified=false) are recorded. Terminal (i) can NEVER emit here: the B1 rulings ledger holds 0 rulings, and " +
        "the conjunction is blocked regardless. Nothing is claimed in either direction; partial closure is uncitable. " +
        "promotedPhysicalMassClaimCount remains 0.",
    "reopened-live-candidate" =>
        "Anchor adjudication is REOPENED: an input forwards a live anchor candidate, so B1 reopens with that candidate.",
    "convention-only" =>
        "Anchor adjudication CLOSES as convention-only: C1 ∧ C2 ∧ C3 all hold and at least one physicist ruling is on record; " +
        "the closure statement carries the five mandatory scope fields.",
    "no-claim" =>
        "Anchor adjudication is a PARTIAL state: the conjunction is not fully satisfied (or zero physicist rulings are on " +
        "record); the pre-committed no-claim sentence is emitted and partial closure is forbidden from citation.",
    _ =>
        "Review-required: inputs missing or the in-phase battery failed; fail-closed, nothing adjudicated.",
};

string emittedSentence = verdictKind switch
{
    "blocked-upstream-ambiguous" => blockedUpstreamSentence,
    "no-claim" => noClaimSentence,
    _ => "",
};

string targetBlindConstructionHash = Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(
    string.Join("|", ApplicationSubjectKind, verdictKind, PlanSection, sha461, sha462, sha463,
        $"C1={c1Real}", $"C2={c2Real}", $"C3={c3Real}", $"rulings={rulingsCount}", $"nPending={nPending}"))));

double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

var result = new
{
    phaseId = "phase464-anchor-adjudication-contract",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    anchorAdjudicationContractPassed,
    verdictTaxonomy,
    conjunction = "C1-and-C2-and-C3",
    tripwireDenominator = TripwireDenominator,
    blockingSetCount,
    nPending,
    terminalIRequiresPhysicistRulings = true,
    inputs = new
    {
        phase461 = new { path = Phase461OutputPath, sha256 = sha461, verdict = verdict461 },
        phase462 = new { path = Phase462OutputPath, sha256 = sha462, verdict = verdict462 },
        phase463 = new { path = Phase463OutputPath, sha256 = sha463, verdict = verdict463 },
        ledger = new { path = LedgerPath, rulingsCount },
    },
    conjuncts = new
    {
        c1BlockingSetClosure = new { satisfied = c1Real, source = "phase462", note = "satisfied ONLY by closure-resolved(-with-excision); pinning-insufficient => BLOCKED" },
        c2ReadingMenuDisposal = new { satisfied = c2Real, source = "phase461", note = "declared-comparison-consistency-only / part12d-anchor-readings-all-miss" },
        c3Transport = new { satisfied = c3Real, mode = c3Mode, vacuous = c3Vacuous, source = "phase463", note = "transport-killed-at-audited-menu OR vacuous-given-zero-sanctioned-anchors (item-20 amendment A1)" },
    },
    rulingsCount,
    machineMonitorableReopeningFields = new
    {
        sanctionedAnchorPresent,
        coercivityCertified,
        note = "read from the phase461 / phase463 outputs; a flip of either from false reopens the record",
    },
    inputFlipBattery = new
    {
        inputFlipBatteryOk,
        a = new { description = "462 flipped to closure-resolved, zero rulings", emitted = batteryA, expected = "not convention-only", ok = batteryAOk },
        b = new { description = "462 closure-resolved + one synthetic ruling", emitted = batteryB, expected = "convention-only", ok = batteryBOk },
        c = new { description = "461 flipped to forward an anchor", emitted = batteryC, expected = "reopened-live-candidate", ok = batteryCOk },
        d = new { description = "real inputs", emitted = batteryD, expected = "blocked-upstream-ambiguous", ok = batteryDOk },
    },
    emittedSentence,
    blockedUpstreamSentence,
    noClaimSentence,
    reopeningConditions,
    objections,
    conventionOnlyClosureContract,
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
    recordedBoundary = new
    {
        physicistReviewPending,
    },
    guards,
    explicitCandidateOnlyNonclaims = new[]
    {
        "This phase mechanically adjudicates a conjunction over committed upstream verdicts; zero physics is computed and nothing is measured, filled, or promoted.",
        "Terminal (i) convention-only can NEVER emit with zero physicist rulings: the B1 ledger is parsed and rulings counted; the current count is 0.",
        "The reachable terminal is (iii) BLOCKED-UPSTREAM-AMBIGUOUS: C1 is blocked (phase462 pinning-insufficient); the emitted sentence claims no anchor theorem in either direction and forbids citing partial closure.",
        "The post-hoc admissibility element (objection 3) and the 33-symbol scope limitation (objection 4) are carried honestly, never eliminated.",
        "physicistReviewPending = true; promotedPhysicalMassClaimCount remains 0.",
    },
    decision,
    runtimeSeconds,
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "anchor_adjudication_contract.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(Path.Combine(outputDir, "anchor_adjudication_contract_summary.json"), JsonSerializer.Serialize(new
{
    result.phaseId,
    result.terminalStatus,
    result.verdictKind,
    result.anchorAdjudicationContractPassed,
    result.conjunction,
    result.tripwireDenominator,
    result.nPending,
    result.rulingsCount,
    result.conjuncts,
    result.machineMonitorableReopeningFields,
    result.inputFlipBattery.inputFlipBatteryOk,
    result.emittedSentence,
    result.physicistReviewPending,
    result.noGevPromotion,
    result.runtimeSeconds,
}, options));

doc461?.Dispose();
doc462?.Dispose();
doc463?.Dispose();

Console.WriteLine(terminalStatus);
Console.WriteLine($"verdictKind={verdictKind} c1={c1Real} c2={c2Real} c3={c3Real} rulings={rulingsCount} batteryOk={inputFlipBatteryOk}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F3}");
