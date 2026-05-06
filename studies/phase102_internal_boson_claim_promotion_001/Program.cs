using System.Text.Json;
using System.Text.Json.Serialization;

const string DefaultOutputDir = "studies/phase102_internal_boson_claim_promotion_001/output";
const string Phase100ReadinessPath = "studies/phase100_boson_prediction_readiness_001/output/boson_prediction_readiness.json";
const string Phase91EvidencePath = "studies/phase91_branch_stability_evidence_promotion_001/output/projected_branch_stability_evidence.json";
const string Phase95SummaryPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/target_blind_refinement_mode_matching_summary.json";
const string Phase99EvidencePath = "studies/phase99_selector_eigenvector_full_lift_001/output/selector_eigenvector_full_lift_evidence.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE102_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var readiness = JsonDocument.Parse(File.ReadAllText(Phase100ReadinessPath));
using var branch = JsonDocument.Parse(File.ReadAllText(Phase91EvidencePath));
using var refinement = JsonDocument.Parse(File.ReadAllText(Phase95SummaryPath));
using var lift = JsonDocument.Parse(File.ReadAllText(Phase99EvidencePath));

var gates = new GateResult[]
{
    Gate("replay-integrity", JsonBool(readiness.RootElement, "sourceBackedReplayReady") is true,
        "Phase100 source-backed replay readiness"),
    Gate("full-connection-lift", JsonInt(lift.RootElement.GetProperty("fullConnectionLift"), "modeVectorLength") == 576,
        "Phase99 selected selector eigenvector has a 576-length full connection-space lift"),
    Gate("target-blind-branch-stability", PassesThreshold(JsonDouble(branch.RootElement, "branchStabilityScore"), 0.5),
        "Phase91 target-blind branch stability evidence"),
    Gate("target-blind-refinement-stability", PassesThreshold(JsonDouble(refinement.RootElement, "refinementStabilityScore"), 0.5),
        "Phase95 target-blind refinement stability evidence"),
};

bool allPassed = gates.All(g => g.Passed);
string proposedClaimClass = allPassed ? "C1_LocalPersistentMode" : "C0_NumericalMode";
var promotion = new
{
    phaseId = "phase102-internal-boson-claim-promotion",
    terminalStatus = allPassed
        ? "internal-boson-claim-promoted"
        : "internal-boson-claim-promotion-blocked",
    candidateId = JsonString(readiness.RootElement, "candidateId") ?? "candidate-3",
    selectedBosonModeId = JsonString(readiness.RootElement, "selectedBosonModeId"),
    previousClaimClass = "C0_NumericalMode",
    proposedClaimClass,
    claimScope = "internal-boson-replay-prediction",
    physicalClaimAllowed = false,
    gates,
    evidence = new
    {
        readinessPath = Phase100ReadinessPath,
        branchStabilityEvidencePath = Phase91EvidencePath,
        refinementEvidencePath = Phase95SummaryPath,
        selectorFullLiftEvidencePath = Phase99EvidencePath,
    },
    doesNotClaim = new[]
    {
        "external W/Z or Standard Model boson identity",
        "candidate-specific physical mapping",
        "candidate-specific physical calibration",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "internal_boson_claim_promotion.json"),
    JsonSerializer.Serialize(promotion, options));
File.WriteAllText(
    Path.Combine(outputDir, "internal_boson_claim_promotion_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase102-internal-boson-claim-promotion",
        promotion.terminalStatus,
        promotion.candidateId,
        promotion.previousClaimClass,
        promotion.proposedClaimClass,
        promotion.physicalClaimAllowed,
    }, options));

Console.WriteLine(promotion.terminalStatus);
Console.WriteLine($"proposedClaimClass={proposedClaimClass}");

static GateResult Gate(string gateId, bool passed, string evidence) => new(gateId, passed, evidence, true);

static bool PassesThreshold(double? value, double threshold) => value is { } d && double.IsFinite(d) && d >= threshold;

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

public sealed class GateResult(string gateId, bool passed, string evidence, bool required)
{
    [JsonPropertyName("gateId")]
    public string GateId { get; } = gateId;

    [JsonPropertyName("passed")]
    public bool Passed { get; } = passed;

    [JsonPropertyName("evidence")]
    public string Evidence { get; } = evidence;

    [JsonPropertyName("required")]
    public bool Required { get; } = required;
}
