using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase470: C-PERMANENCE five-limb implication ledger.
// This phase reads committed verdicts only. It cannot upgrade an open limb,
// infer the absent Phase469 representation object, or create a physics claim.

var stopwatch = Stopwatch.StartNew();

const string OutputDir = "studies/phase470_c_permanence_five_limb_ledger_001/output";
const string P459 = "studies/phase459_spectroscopy_record_attestation_001/output/spectroscopy_record_attestation_summary.json";
const string P465 = "studies/phase465_anomaly_consistency_variety_kernel_001/output/anomaly_consistency_variety_kernel_summary.json";
const string P467 = "studies/phase467_derived_operator_stabilizer_ray_census_001/output/derived_operator_stabilizer_ray_census_summary.json";
const string P468 = "studies/phase468_two_loop_content_row_closure_filter_001/output/two_loop_content_row_closure_filter_summary.json";
const string P469 = "studies/phase469_c_lift_representation_bookkeeping_gate_001/output/c_lift_representation_bookkeeping_gate_summary.json";
const string P459Hash = "218aeb42f63ea4a8a2e7b98a111d700aeda9d7944e9377d27d67204fe49a9a68";
const string P465Hash = "51ee4558001a50c42af7c003d92d2c4b4c83b8608a019b215816240e13ee8915";
const string P467Hash = "b4e6525dd0509b161b1aa941c2c0c547b0a41ebb51ee654a3cd5fbc18db8cf06";
const string P468MenuHash = "1fcff9e863df9b06d89b67802fad216976d8fbbdd8d160fe5f9c5d7f958867d5";
const string P469Hash = "9e1825ed51cf6204a3739e912af46164c85d0442574544a981b9294738653f3f";

Directory.CreateDirectory(OutputDir);

bool inputsPresent = new[] { P459, P465, P467, P468, P469 }.All(File.Exists);
using var d459 = inputsPresent ? JsonDocument.Parse(File.ReadAllText(P459)) : null;
using var d465 = inputsPresent ? JsonDocument.Parse(File.ReadAllText(P465)) : null;
using var d467 = inputsPresent ? JsonDocument.Parse(File.ReadAllText(P467)) : null;
using var d468 = inputsPresent ? JsonDocument.Parse(File.ReadAllText(P468)) : null;
using var d469 = inputsPresent ? JsonDocument.Parse(File.ReadAllText(P469)) : null;
JsonElement j459 = d459?.RootElement ?? default;
JsonElement j465 = d465?.RootElement ?? default;
JsonElement j467 = d467?.RootElement ?? default;
JsonElement j468 = d468?.RootElement ?? default;
JsonElement j469 = d469?.RootElement ?? default;

bool reconciliationGatePassed = inputsPresent
    && S(j459, "targetBlindConstructionHash") == P459Hash
    && S(j459, "terminalStatus") == "spectroscopy-record-attestation-passed-record-reconciled-canonical"
    && B(j459, "spectroscopyRecordAttestationPassed")
    && I(j459, "checkCount") == 31 && I(j459, "passedCheckCount") == 31;
bool p1ClosedNegative = inputsPresent
    && S(j465, "kernelConstructionHash") == P465Hash
    && S(j465, "verdictKind") == "anomaly-variety-positive-dimensional-route-closed"
    && B(j465, "routeClosedAsRuleOut");
bool p2ClosedNegative = inputsPresent
    && S(j467, "constructionHash") == P467Hash
    && S(j467, "verdictKind") == "all-symmetric-non-y-route-closed";
bool p3Open = inputsPresent
    && S(j469, "constructionHash") == P469Hash
    && S(j469, "verdictKind") == "bookkeeping-fails-source-object-unpinned-ws5-rescoped"
    && !j469.GetProperty("stageMinusOneSourcePin").GetProperty("sourceDefinedRepresentationObjectPinned").GetBoolean()
    && !j469.GetProperty("fundingDecision").GetProperty("ws5Funded").GetBoolean();
bool p4ClosedScoped = inputsPresent
    && S(j468, "verdictKind") == "full-menu-out-of-band-scoped-exhaustion"
    && S(j468.GetProperty("menu"), "menuHash") == P468MenuHash
    && j468.GetProperty("filter").GetProperty("cPermanenceP4ScopedExhaustion").GetBoolean();

// P5 means exhaustiveness relative to the declared C-PERMANENCE evidence
// surface, not exhaustiveness over unknown future sources. Every named limb is
// present, hash/terminal pinned, and the mandatory Phase452 reconciliation
// gate is carried through Phase459.
bool p5ClosedRelativeToAuditedInputs = reconciliationGatePassed
    && p1ClosedNegative && p2ClosedNegative && p3Open && p4ClosedScoped;
bool inputPinsGreen = inputsPresent && p5ClosedRelativeToAuditedInputs;

var limbs = new[]
{
    new Limb("P1", "C-KERNEL anomaly-selection route", p1ClosedNegative ? "closed-negative" : "unreadable-or-drifted", "phase465"),
    new Limb("P2", "C-STABILIZER derived-operator selection", p2ClosedNegative ? "closed-negative" : "unreadable-or-drifted", "phase467"),
    new Limb("P3", "C-LIFT representation bookkeeping / WS5", p3Open ? "open-source-object-required" : "unreadable-or-drifted", "phase469"),
    new Limb("P4", "C-CLOSURE two-loop content menu", p4ClosedScoped ? "closed-scoped" : "unreadable-or-drifted", "phase468"),
    new Limb("P5", "audit exhaustiveness", p5ClosedRelativeToAuditedInputs ? "closed-relative-to-audited-inputs" : "open", "phase459+phase465+phase467+phase468+phase469"),
};

string verdictKind = Decide(limbs);
string[] openLimbs = limbs.Where(x => x.Status.StartsWith("open", StringComparison.Ordinal)
    || x.Status.Contains("drifted", StringComparison.Ordinal)).Select(x => x.Id).ToArray();

// Structural taxonomy battery: permanent requires every limb closed;
// positive inclusion stays only partially open; missing/drifted or an ordinary
// open limb stays not-decidable and names that limb.
Limb L(string id, string status) => new(id, "synthetic", status, "synthetic");
var allNegative = new[] { L("P1", "closed-negative"), L("P2", "closed-negative"), L("P3", "closed-negative"), L("P4", "closed-scoped"), L("P5", "closed-relative-to-audited-inputs") };
var oneOpen = allNegative.Select(x => x.Id == "P3" ? L("P3", "open-source-object-required") : x).ToArray();
var onePositive = allNegative.Select(x => x.Id == "P2" ? L("P2", "positive-inclusion") : x).ToArray();
var oneDrifted = allNegative.Select(x => x.Id == "P4" ? L("P4", "unreadable-or-drifted") : x).ToArray();
bool allClosedReachesPermanent = Decide(allNegative) == "hypercharge-lineage-gap-permanent-relative-to-audited-sources";
bool openLimbForcesNotDecidable = Decide(oneOpen) == "permanence-not-decidable";
bool positiveInclusionForcesPartialOpen = Decide(onePositive) == "b3-partially-open";
bool driftForcesNotDecidable = Decide(oneDrifted) == "permanence-not-decidable";
bool currentOpenLimbNamed = verdictKind == "permanence-not-decidable" && openLimbs.SequenceEqual(new[] { "P3" });
bool structuralIncapabilityBatteryPassed = allClosedReachesPermanent && openLimbForcesNotDecidable
    && positiveInclusionForcesPartialOpen && driftForcesNotDecidable && currentOpenLimbNamed;

bool cPermanenceLedgerExecuted = inputPinsGreen && structuralIncapabilityBatteryPassed;
string terminalStatus = cPermanenceLedgerExecuted
    ? "c-permanence-five-limb-ledger-permanence-not-decidable-open-p3"
    : "c-permanence-five-limb-ledger-input-or-battery-failed-review-required";
const bool targetBlindConstruction = true;
const bool physicalTargetsConsultedForConstruction = false;
const bool noGevPromotion = true;
const int promotedPhysicalMassClaimCount = 0;
const bool sourceContractApplicationAllowed = false;
const bool canFillPhase201WzContract = false;
const bool canFillPhase201HiggsContract = false;
const bool canFillPhase256ObservedFieldExtractionContract = false;
const bool routePromotesWzMasses = false;
const bool routePromotesHiggsMass = false;
const bool routeCompletesBosonPredictions = false;
string constructionHash = Sha256(string.Join("|", P459Hash, P465Hash, P467Hash, P468MenuHash, P469Hash,
    string.Join(";", limbs.Select(x => $"{x.Id}:{x.Status}")), verdictKind));

stopwatch.Stop();
var result = new
{
    phaseId = "phase470-c-permanence-five-limb-ledger",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    cPermanenceLedgerExecuted,
    applicationSubjectKind = "c-permanence-five-limb-implication-ledger",
    planSection = "WAVE2_ACTION_PLAN_2026-07-12 item 15",
    constructionHash,
    inputs = new { inputsPresent, reconciliationGatePassed, inputPinsGreen, phase459HashPin = P459Hash, phase465HashPin = P465Hash, phase467HashPin = P467Hash, phase468MenuHashPin = P468MenuHash, phase469HashPin = P469Hash },
    limbs,
    openLimbs,
    p1ClosedNegative,
    p2ClosedNegative,
    p3Open,
    p4ClosedScoped,
    p5ClosedRelativeToAuditedInputs,
    auditScope = "Committed Phase459 reconciliation plus the exact Phase465/467/468/469 C-lineage artifacts. This is relative exhaustiveness only; unknown future sources are outside scope.",
    reopeningCondition = "Commit the source-defined Cl(7,7)/128 representation object named by Phase469, including its embedding, 64+/- isotypics, Casimirs, Y-weight map, and epsilon-conjugation operator; rerun Phase469 and then Phase470.",
    batteries = new { allClosedReachesPermanent, openLimbForcesNotDecidable, positiveInclusionForcesPartialOpen, driftForcesNotDecidable, currentOpenLimbNamed, structuralIncapabilityBatteryPassed },
    targetBlindConstruction,
    physicalTargetsConsultedForConstruction,
    noGevPromotion,
    promotedPhysicalMassClaimCount,
    sourceContractApplicationAllowed,
    canFillPhase201WzContract,
    canFillPhase201HiggsContract,
    canFillPhase256ObservedFieldExtractionContract,
    routePromotesWzMasses,
    routePromotesHiggsMass,
    routeCompletesBosonPredictions,
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
    decision = "C-PERMANENCE is not decidable: P1 and P2 are closed negative, P4 is closed only over its committed menu, and P5 is closed only relative to the pinned audited inputs, while P3 remains open because the source-defined representation object required by C-LIFT is absent. No human ruling is simulated, no permanent theorem is claimed, and nothing is promoted."
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, "c_permanence_five_limb_ledger.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "c_permanence_five_limb_ledger_summary.json"), json);
Console.WriteLine(terminalStatus);
Console.WriteLine($"verdictKind={verdictKind}");
Console.WriteLine($"openLimbs=[{string.Join(",", openLimbs)}]");
Console.WriteLine($"structuralIncapabilityBatteryPassed={structuralIncapabilityBatteryPassed}");
Console.WriteLine($"promotedPhysicalMassClaimCount={promotedPhysicalMassClaimCount}");
Environment.ExitCode = cPermanenceLedgerExecuted ? 0 : 2;

static string Decide(IReadOnlyList<Limb> limbs)
{
    if (limbs.Any(x => x.Status == "positive-inclusion")) return "b3-partially-open";
    if (limbs.Any(x => x.Status.StartsWith("open", StringComparison.Ordinal)
        || x.Status.Contains("drifted", StringComparison.Ordinal))) return "permanence-not-decidable";
    bool completeNegative = limbs.All(x => x.Status is "closed-negative" or "closed-scoped" or "closed-relative-to-audited-inputs");
    return completeNegative ? "hypercharge-lineage-gap-permanent-relative-to-audited-sources" : "permanence-not-decidable";
}

static string S(JsonElement e, string name) => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) ? v.GetString() ?? "" : "";
static bool B(JsonElement e, string name) => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.True;
static int I(JsonElement e, string name) => e.ValueKind == JsonValueKind.Object && e.TryGetProperty(name, out var v) && v.TryGetInt32(out int n) ? n : -1;
static string Sha256(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

sealed record Limb(string Id, string Name, string Status, string Evidence);
