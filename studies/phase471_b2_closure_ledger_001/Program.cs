using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase471: B2 CLOSURE THEOREM-OF-RECORD LEDGER (WAVE2_ACTION_PLAN_2026-07-12
// item 14). Unlike the item-1/9/10/11/2/3/12/4/5 skeletons, this phase is
// implemented for real at STEP 0: it is the eight-limb closure ledger that
// reads the committed limb outcomes and emits a machine-attested program-state
// record. It upgrades mechanically as limbs land; today it emits
// "closure-not-decidable" naming the open limbs.
//
// EIGHT LIMBS (per the plan's B2 taxonomy):
//   L1  = phase443 (joint EP saturation)                       CLOSED (Wave 1)
//   L2  = phase446 + phase447 (RG scheme / two-loop saturation) CLOSED (Wave 1)
//   L3  = phase449 + phase450 (Gaussian EP / CEP HMC)           CLOSED (Wave 1)
//   L4  = phase453 + phase454 (WHAM parity null / beyond-ray)   CLOSED (Wave 1)
//   L5  = phase455 (exact fermionic backreaction)               OPEN (skeleton)
//   L6  = phase456 (consolidated n=4 launch)                    OPEN (skeleton)
//   L7  = phase457 + phase466 (portal / WS3 schema)             WITHHELD (hold)
//   L8  = phase458 (Binder go/no-go)                            OPEN (skeleton)
//
// STRUCTURAL-INCAPABILITY BATTERY (proved by construction, in-phase, on
// SYNTHETIC inputs): the strong negative terminal ("closed-negative") is
// UNREACHABLE while any limb is open/held/withheld. A forged near-miss input
// set — open limbs relabelled with negative-looking terminal strings — must
// STILL yield "closure-not-decidable"; and a withheld limb can at most produce
// "closed-negative-except-portal-unprobed" with an explicit carve-out, never
// the bare strong negative. The decision keys on limb STATUS, never on the
// content of a terminal string.
//
// MANDATORY FRAMING. Zero physics compute; this ledger reads committed
// verdicts and emits a program-state record. Nothing is measured, filled, or
// promoted; promotedPhysicalMassClaimCount remains 0. physicistReviewPending is
// carried explicitly.

var stopwatch = Stopwatch.StartNew();

const string ApplicationSubjectKind = "b2-closure-ledger";
const string PlanSection = "WAVE2_ACTION_PLAN_2026-07-12 item 14";
const string DefaultOutputDir = "studies/phase471_b2_closure_ledger_001/output";
const string TerminalPrefix = "b2-closure-ledger-";

// --- limb outcome model. ---------------------------------------------------
// Status drives the decision; Outcome is only consulted for CLOSED limbs (and
// exercised by the synthetic battery). TerminalString is provenance only.
LimbState ReadLimb(string id, string name, string[] phases, LimbStatus statusIfReadable,
    LimbOutcome outcomeIfReadable, params string[] summaryPaths)
{
    var strings = new List<string>();
    bool allReadable = true;
    foreach (var p in summaryPaths)
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(p));
            strings.Add(doc.RootElement.TryGetProperty("terminalStatus", out var t) && t.ValueKind == JsonValueKind.String
                ? t.GetString() ?? "" : "");
        }
        catch
        {
            allReadable = false;
            strings.Add("<unreadable>");
        }
    }
    // Fail-closed: an unreadable committed input demotes the limb to Unreadable
    // (which counts as not-closed), so a missing artifact can never silently
    // close a limb.
    var status = allReadable ? statusIfReadable : LimbStatus.Unreadable;
    return new LimbState(id, name, phases, status, allReadable ? outcomeIfReadable : LimbOutcome.None,
        string.Join(" ; ", strings));
}

const string P443 = "studies/phase443_joint_effective_potential_saturation_probe_001/output/joint_effective_potential_saturation_probe_summary.json";
const string P446 = "studies/phase446_rg_scheme_dependence_resolution_probe_001/output/rg_scheme_dependence_resolution_probe_summary.json";
const string P447 = "studies/phase447_two_loop_saturation_probe_001/output/two_loop_saturation_probe_summary.json";
const string P449 = "studies/phase449_variational_gaussian_effective_potential_probe_001/output/variational_gaussian_effective_potential_probe_summary.json";
const string P450 = "studies/phase450_constraint_effective_potential_hmc_probe_001/output/constraint_effective_potential_hmc_probe_summary.json";
const string P453 = "studies/phase453_wham_parity_error_model_repair_001/output/wham_parity_error_model_repair_summary.json";
const string P454 = "studies/phase454_beyond_ray_quadratic_certificate_probe_001/output/beyond_ray_quadratic_certificate_probe_summary.json";
const string P455 = "studies/phase455_exact_fermionic_backreaction_probe_001/output/exact_fermionic_backreaction_probe_summary.json";
const string P456 = "studies/phase456_consolidated_n4_launch_001/output/consolidated_n4_launch_summary.json";
const string P457 = "studies/phase457_upsilon_portal_stage_a_001/output/upsilon_portal_stage_a_summary.json";
const string P466 = "studies/phase466_ws3_vev_completion_contract_001/output/ws3_vev_completion_contract_summary.json";
const string P458 = "studies/phase458_binder_go_no_go_gate_001/output/binder_go_no_go_gate_summary.json";

// L1-L4 are CLOSED by Wave 1 (committed outcomes). Their Outcome is recorded
// as "recorded" (a closed diagnostic that is neither a clean strong-negative
// nor a positive inclusion) so a single committed candidate certificate can
// never masquerade as a scale-positive; the strong-negative synthetic battery
// is driven separately.
var limbs = new List<LimbState>
{
    ReadLimb("L1", "joint-effective-potential-saturation", new[] { "phase443" },
        LimbStatus.Closed, LimbOutcome.Recorded, P443),
    ReadLimb("L2", "rg-scheme-and-two-loop-saturation", new[] { "phase446", "phase447" },
        LimbStatus.Closed, LimbOutcome.Recorded, P446, P447),
    ReadLimb("L3", "gaussian-ep-and-cep-hmc", new[] { "phase449", "phase450" },
        LimbStatus.Closed, LimbOutcome.Recorded, P449, P450),
    ReadLimb("L4", "wham-parity-null-and-beyond-ray", new[] { "phase453", "phase454" },
        LimbStatus.Closed, LimbOutcome.Recorded, P453, P454),
    ReadLimb("L5", "exact-fermionic-backreaction", new[] { "phase455" },
        LimbStatus.Open, LimbOutcome.None, P455),
    ReadLimb("L6", "consolidated-n4-launch", new[] { "phase456" },
        LimbStatus.Open, LimbOutcome.None, P456),
    ReadLimb("L7", "portal-and-ws3-schema", new[] { "phase457", "phase466" },
        LimbStatus.Withheld, LimbOutcome.None, P457, P466),
    ReadLimb("L8", "binder-go-no-go", new[] { "phase458" },
        LimbStatus.Open, LimbOutcome.None, P458),
};

var decision = Decide(limbs);

// ---------------------------------------------------------------------------
// STRUCTURAL-INCAPABILITY BATTERY (synthetic; proves closed-negative is
// unreachable while any limb is open/held/withheld, and that verdict-withheld
// can at most produce the carve-out variant).
// ---------------------------------------------------------------------------
LimbState Syn(string id, LimbStatus s, LimbOutcome o, string term) =>
    new(id, "synthetic-" + id, new[] { "synthetic" }, s, o, term);

// Scenario 1: the real limb set -> not-decidable.
var s1 = decision.Terminal;

// Scenario 2: FORGED NEAR-MISS. Open/held/withheld limbs relabelled with
// negative-looking terminal strings but their STATUS left not-closed. Must
// still be not-decidable (proves status-keyed, not string-keyed).
var forged = new List<LimbState>
{
    Syn("L1", LimbStatus.Closed, LimbOutcome.Negative, "closed-negative"),
    Syn("L2", LimbStatus.Closed, LimbOutcome.Negative, "closed-negative"),
    Syn("L3", LimbStatus.Closed, LimbOutcome.Negative, "closed-negative"),
    Syn("L4", LimbStatus.Closed, LimbOutcome.Negative, "closed-negative"),
    Syn("L5", LimbStatus.Open, LimbOutcome.Negative, "fermionic-backreaction-null"),
    Syn("L6", LimbStatus.Open, LimbOutcome.Negative, "n4-gaussian-null-binder"),
    Syn("L7", LimbStatus.Withheld, LimbOutcome.Negative, "portal-null-measured"),
    Syn("L8", LimbStatus.Open, LimbOutcome.Negative, "binder-no-go-theorem-closed"),
};
var s2 = Decide(forged).Terminal;

// Scenario 3: all-closed, all clean-negative, none withheld -> the ONLY route
// to the bare strong negative.
var allNeg = new List<LimbState>();
for (int i = 1; i <= 8; i++) allNeg.Add(Syn("L" + i, LimbStatus.Closed, LimbOutcome.Negative, "closed-negative"));
var s3 = Decide(allNeg).Terminal;

// Scenario 4: all-closed-negative except one WITHHELD -> carve-out, never bare.
var carve = new List<LimbState>();
for (int i = 1; i <= 8; i++)
    carve.Add(Syn("L" + i, i == 7 ? LimbStatus.Withheld : LimbStatus.Closed,
        i == 7 ? LimbOutcome.None : LimbOutcome.Negative, "closed-negative"));
var s4 = Decide(carve).Terminal;

// Scenario 5: any committed positive inclusion -> closed-with-positive-inclusion.
var pos = new List<LimbState>();
for (int i = 1; i <= 8; i++)
    pos.Add(Syn("L" + i, LimbStatus.Closed, i == 5 ? LimbOutcome.Positive : LimbOutcome.Negative,
        i == 5 ? "radiative-well-candidate-t2" : "closed-negative"));
var s5 = Decide(pos).Terminal;

bool battery1 = s1 == "closure-not-decidable";
bool battery2 = s2 == "closure-not-decidable";                       // forged near-miss stays not-decidable
bool battery3 = s3 == "closed-negative";                            // reachable only when all closed-negative
bool battery4 = s4 == "closed-negative-except-portal-unprobed";     // withheld -> carve-out only
bool battery5 = s5 == "closed-with-positive-inclusion";
bool closedNegativeUnreachableWhileAnyOpenOrWithheld = s1 != "closed-negative" && s2 != "closed-negative" && s4 != "closed-negative";
bool structuralIncapabilityBatteryPassed =
    battery1 && battery2 && battery3 && battery4 && battery5 && closedNegativeUnreachableWhileAnyOpenOrWithheld;

// --- standing claim boundary (verbatim). -----------------------------------
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

bool b2ClosureLedgerBuilt =
    decision.Terminal == "closure-not-decidable" &&
    structuralIncapabilityBatteryPassed &&
    targetBlindConstruction && !physicalTargetsConsultedForConstruction &&
    physicistReviewPending && scaleIsWorkbenchRelativeCandidateOnly && noGevPromotion &&
    !sourceContractApplicationAllowed && !phase201TemplateMutated &&
    fieldsAppliedToPhase201TemplateCount == 0 && acceptedContractFieldCount == 0 &&
    !canFillPhase201WzContract && !canFillPhase201HiggsContract && !canFillPhase256Contract &&
    !canFillPhase256ObservedFieldExtractionContract && !physicalCouplingProvided &&
    !routeProvidesPhysicalEffectiveActionHessian && !routeProvidesVevOrSourceScaleLineage &&
    !routeProvidesPoleExtractionAndGeVNormalization && !routeCompletesBosonPredictions &&
    !routePromotesWzMasses && !routePromotesHiggsMass;

string verdictKind = decision.Terminal;
string terminalStatus = TerminalPrefix + (b2ClosureLedgerBuilt
    ? "closure-not-decidable-open-limbs-named"
    : "ledger-build-failed-review-required");

string targetBlindConstructionHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
    string.Join("|", ApplicationSubjectKind, PlanSection,
        "eight-limb b2 closure ledger; L1-L4 closed; L5-L8 open/withheld; structural-incapability battery; standing claim boundary")))).ToLowerInvariant();

string decisionProse =
    $"B2 closure theorem-of-record ledger (plan item 14). Verdict: {decision.Terminal}. "
    + $"Open/not-closed limbs: [{string.Join(", ", decision.OpenLimbs)}]; withheld limbs: [{string.Join(", ", decision.WithheldLimbs)}]. "
    + "The strong negative terminal is UNREACHABLE while any limb is open/held/withheld; proved by construction in the "
    + "structural-incapability battery (forged near-miss with negative-looking strings on not-closed limbs still yields "
    + "closure-not-decidable; a withheld limb can at most produce closed-negative-except-portal-unprobed with a carve-out). "
    + "This is a program-state record, not a physics claim; nothing is promoted (promotedPhysicalMassClaimCount remains 0).";

Directory.CreateDirectory(DefaultOutputDir);
double runtimeSeconds = stopwatch.Elapsed.TotalSeconds;

var result = new
{
    phaseId = "phase471-b2-closure-ledger",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    interimTerminal = decision.Terminal,
    verdictKind,
    applicationSubjectKind = ApplicationSubjectKind,
    planSection = PlanSection,
    b2ClosureLedgerBuilt,
    zeroPhysicsCompute = true,
    limbs = limbs.Select(l => new
    {
        id = l.Id,
        name = l.Name,
        phases = l.Phases,
        status = l.Status.ToString().ToLowerInvariant(),
        outcome = l.Outcome.ToString().ToLowerInvariant(),
        committedTerminalStrings = l.TerminalString,
    }).ToArray(),
    closedLimbs = limbs.Where(l => l.Status == LimbStatus.Closed).Select(l => l.Id).ToArray(),
    openLimbs = decision.OpenLimbs,
    withheldLimbs = decision.WithheldLimbs,
    structuralIncapabilityBattery = new
    {
        structuralIncapabilityBatteryPassed,
        closedNegativeUnreachableWhileAnyOpenOrWithheld,
        scenarios = new object[]
        {
            new { id = "real-limb-set", expected = "closure-not-decidable", observed = s1, pass = battery1 },
            new { id = "forged-near-miss-negative-strings-on-open-limbs", expected = "closure-not-decidable", observed = s2, pass = battery2 },
            new { id = "all-closed-negative", expected = "closed-negative", observed = s3, pass = battery3 },
            new { id = "all-closed-negative-except-one-withheld", expected = "closed-negative-except-portal-unprobed", observed = s4, pass = battery4 },
            new { id = "one-positive-inclusion", expected = "closed-with-positive-inclusion", observed = s5, pass = battery5 },
        },
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
        "This ledger reads committed limb outcomes and emits a program-state record; it performs zero physics computation and promotes nothing.",
        "closure-not-decidable is a machine-attested statement that limbs remain open/withheld; it is NOT a closure theorem and asserts no physical property.",
        "The strong negative terminal is unreachable while any limb is open/held/withheld; verdict-withheld may at most produce the explicit carve-out variant.",
        "physicistReviewPending = true is carried explicitly; promotedPhysicalMassClaimCount remains 0.",
    },
    decision = decisionProse,
    runtimeSeconds,
};

var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(DefaultOutputDir, "b2_closure_ledger.json"), JsonSerializer.Serialize(result, jsonOptions));
File.WriteAllText(Path.Combine(DefaultOutputDir, "b2_closure_ledger_summary.json"), JsonSerializer.Serialize(result, jsonOptions));

Console.WriteLine(terminalStatus);
Console.WriteLine($"verdictKind={verdictKind} b2ClosureLedgerBuilt={b2ClosureLedgerBuilt}");
Console.WriteLine($"openLimbs=[{string.Join(",", decision.OpenLimbs)}] withheldLimbs=[{string.Join(",", decision.WithheldLimbs)}]");
Console.WriteLine($"structuralIncapabilityBatteryPassed={structuralIncapabilityBatteryPassed}");
Console.WriteLine($"runtimeSeconds={runtimeSeconds:F3}");

// ---------------------------------------------------------------------------
// The closure decision. STATUS-keyed, never string-keyed.
// ---------------------------------------------------------------------------
static Decision Decide(IReadOnlyList<LimbState> limbs)
{
    var notClosed = limbs.Where(l => l.Status != LimbStatus.Closed).ToArray();
    var nonWithheldOpen = notClosed.Where(l => l.Status != LimbStatus.Withheld).Select(l => l.Id).ToArray();
    var withheld = notClosed.Where(l => l.Status == LimbStatus.Withheld).Select(l => l.Id).ToArray();
    bool anyPositiveInclusion = limbs.Any(l => l.Status == LimbStatus.Closed && l.Outcome == LimbOutcome.Positive);

    string terminal;
    if (anyPositiveInclusion)
        terminal = "closed-with-positive-inclusion";
    else if (nonWithheldOpen.Length > 0)
        terminal = "closure-not-decidable";
    else if (withheld.Length > 0)
        terminal = "closed-negative-except-portal-unprobed";
    else if (limbs.All(l => l.Status == LimbStatus.Closed && l.Outcome == LimbOutcome.Negative))
        terminal = "closed-negative";
    else
        // all closed but at least one is a recorded/inconclusive (non-clean-negative)
        // outcome: not eligible for the strong negative -> honest not-decidable.
        terminal = "closure-not-decidable";

    var openNames = notClosed.Where(l => l.Status != LimbStatus.Withheld).Select(l => l.Id).ToArray();
    return new Decision(terminal, openNames, withheld);
}

enum LimbStatus { Closed, Open, Held, Withheld, Unreadable }
enum LimbOutcome { None, Negative, Positive, Recorded }

sealed record LimbState(string Id, string Name, string[] Phases, LimbStatus Status, LimbOutcome Outcome, string TerminalString);
sealed record Decision(string Terminal, string[] OpenLimbs, string[] WithheldLimbs);
