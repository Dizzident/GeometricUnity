using System.Text.Json;

const string DefaultOutputDir = "studies/phase113_wz_absolute_repair_attempt_gate_001/output";
const string Phase111RawAttemptPath = "studies/phase111_replayed_matrix_element_repair_attempt_001/output/replayed_matrix_element_repair_attempt.json";
const string Phase112ScalarAttemptPath = "studies/phase112_scalar_sector_relation_revision_attempt_001/output/scalar_sector_relation_revision_attempt.json";
const string Phase110ContractPath = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase114WzRouteEvidencePath = "studies/phase114_wz_route_replayed_matrix_element_evidence_001/output/wz_route_replayed_matrix_element_evidence.json";
const string Phase115WzRouteFermionQualityReplayPath = "studies/phase115_wz_route_fermion_quality_replay_001/output/wz_route_fermion_quality_replay.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE113_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var raw = JsonDocument.Parse(File.ReadAllText(Phase111RawAttemptPath));
using var scalar = JsonDocument.Parse(File.ReadAllText(Phase112ScalarAttemptPath));
using var contract = JsonDocument.Parse(File.ReadAllText(Phase110ContractPath));
using var phase114WzRoute = TryParseJson(Phase114WzRouteEvidencePath);
using var phase115WzRoute = TryParseJson(Phase115WzRouteFermionQualityReplayPath);
var preferredWzRoute = phase115WzRoute ?? phase114WzRoute;

bool candidate3RawAccepted = JsonBool(raw.RootElement, "repairAccepted") is true;
bool wzRouteRawAccepted = preferredWzRoute is not null && JsonBool(preferredWzRoute.RootElement, "repairAccepted") is true;
bool rawAccepted = candidate3RawAccepted || wzRouteRawAccepted;
bool scalarAccepted = JsonBool(scalar.RootElement, "repairAccepted") is true;
bool repairAccepted = rawAccepted || scalarAccepted;

var result = new
{
    phaseId = "phase113-wz-absolute-repair-attempt-gate",
    terminalStatus = repairAccepted
        ? "wz-absolute-repair-evidence-ready"
        : "wz-absolute-repair-evidence-blocked",
    projectionRerunAllowed = repairAccepted,
    acceptedStrategies = new[]
    {
        rawAccepted ? "replayed-analytic-raw-matrix-element" : null,
        scalarAccepted ? "scalar-sector-relation-revision" : null,
    }.Where(x => x is not null).ToArray(),
    strategyResults = new[]
    {
        new
        {
            strategyId = "replayed-analytic-raw-matrix-element",
            terminalStatus = preferredWzRoute is null
                ? JsonString(raw.RootElement, "terminalStatus")
                : JsonString(preferredWzRoute.RootElement, "terminalStatus"),
            accepted = rawAccepted,
        },
        new
        {
            strategyId = "scalar-sector-relation-revision",
            terminalStatus = JsonString(scalar.RootElement, "terminalStatus"),
            accepted = scalarAccepted,
        },
    },
    remainingEvidenceGap = repairAccepted
        ? Array.Empty<string>()
        : RemainingEvidenceGaps(preferredWzRoute),
    nextAction = repairAccepted
        ? "rerun absolute W/Z projection and target comparison"
        : "do not rerun absolute W/Z projection; produce target-independent repair evidence first",
    sourceEvidence = new
    {
        contractPath = Phase110ContractPath,
        rawMatrixElementAttemptPath = Phase111RawAttemptPath,
        wzRouteMatrixElementEvidencePath = phase114WzRoute is null ? null : Phase114WzRouteEvidencePath,
        wzRouteFermionQualityReplayPath = phase115WzRoute is null ? null : Phase115WzRouteFermionQualityReplayPath,
        scalarRelationAttemptPath = Phase112ScalarAttemptPath,
        repairTarget = contract.RootElement.GetProperty("repairTarget").Clone(),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "wz_absolute_repair_attempt_gate.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_absolute_repair_attempt_gate_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase113-wz-absolute-repair-attempt-gate",
        result.terminalStatus,
        result.projectionRerunAllowed,
        result.acceptedStrategies,
        result.remainingEvidenceGap,
        result.nextAction,
    }, options));

Console.WriteLine(result.terminalStatus);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static JsonDocument? TryParseJson(string path) =>
    File.Exists(path) ? JsonDocument.Parse(File.ReadAllText(path)) : null;

static IReadOnlyList<string> RemainingEvidenceGaps(JsonDocument? wzRoute)
{
    var gaps = new List<string>();
    if (wzRoute is null)
    {
        gaps.Add("W/Z-route-compatible replayed analytic raw matrix-element evidence is missing");
    }
    else if (JsonBool(wzRoute.RootElement, "fermionQualityPassed") is not true)
    {
        gaps.Add("W/Z-route replayed analytic raw matrix-element evidence exists but is blocked by fermion-mode quality");
    }
    else if (JsonBool(wzRoute.RootElement, "rawEvidenceValidated") is not true)
    {
        gaps.Add("W/Z-route replayed analytic raw matrix-element evidence is present but not validated");
    }

    gaps.Add("target-independent scalar-sector relation revision evidence is missing");
    return gaps;
}
