using System.Globalization;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase154_wz_transition_bridge_root_cause_audit_001/output";
const string Phase120Path = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency.json";
const string Phase122Path = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase138Path = "studies/phase138_repaired_row_coupling_transition_graph_001/output/repaired_row_coupling_transition_graph.json";
const string Phase139Path = "studies/phase139_fermion_sector_label_route_closure_001/output/fermion_sector_label_route_closure.json";
const string Phase146Path = "studies/phase146_fermion_sector_evidence_census_001/output/fermion_sector_evidence_census.json";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";
const string Phase153Path = "studies/phase153_wz_absolute_scale_closure_001/output/wz_absolute_scale_closure.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE154_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase120 = JsonDocument.Parse(File.ReadAllText(Phase120Path));
using var phase122 = JsonDocument.Parse(File.ReadAllText(Phase122Path));
using var phase138 = JsonDocument.Parse(File.ReadAllText(Phase138Path));
using var phase139 = JsonDocument.Parse(File.ReadAllText(Phase139Path));
using var phase146 = JsonDocument.Parse(File.ReadAllText(Phase146Path));
using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));
using var phase153 = JsonDocument.Parse(File.ReadAllText(Phase153Path));

bool analyticMeasurePassed = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true;
bool projectionCandidateFound = !IsNull(phase122.RootElement, "projectionCandidate");
bool transitionGraphPromotable = JsonBool(phase138.RootElement, "transitionGraphPromotable") is true;
bool routeClosureReady = JsonBool(phase139.RootElement, "closureReady") is true;
bool sectorEvidencePresent = JsonBool(phase146.RootElement, "currentEvidencePresent") is true;

var strongest = phase122.RootElement.GetProperty("strongest");
var bestBridge = phase122.RootElement.GetProperty("bestBridge");
var selectedPair03 = phase122.RootElement.GetProperty("selectedPair03");

double? strongestMinRaw = JsonDouble(strongest, "minRawToTargetRatio");
double? bestBridgeSpread = JsonDouble(bestBridge, "commonBridgeRelativeSpread");
double? selectedPair03MinRaw = JsonDouble(selectedPair03, "minRawToTargetRatio");
double rawGate = 0.95;
double bridgeGate = 0.05;

string rootCause = !analyticMeasurePassed
    ? "operator-normalization-open"
    : projectionCandidateFound
        ? "projection-candidate-ready"
    : sectorEvidencePresent || transitionGraphPromotable || routeClosureReady
        ? "sector-evidence-present-rerun-required"
        : "missing-sector-evidence";

string terminalStatus = rootCause switch
{
    "projection-candidate-ready" => "wz-transition-bridge-root-cause-projection-candidate-ready",
    "sector-evidence-present-rerun-required" => "wz-transition-bridge-root-cause-rerun-required",
    "operator-normalization-open" => "wz-transition-bridge-root-cause-operator-normalization-open",
    _ => "wz-transition-bridge-root-cause-missing-sector-evidence",
};

var blockerFacts = new[]
{
    analyticMeasurePassed
        ? "Phase120 derives a target-independent analytic variation measure; operator normalization is not the active blocker."
        : "Phase120 does not provide a promotable analytic variation measure.",
    projectionCandidateFound
        ? "Phase122 has a corrected-operator projection candidate."
        : "Phase122 has no corrected-operator projection candidate.",
    $"Strongest quality transition min raw-to-target ratio is {FormatNullable(strongestMinRaw)} against gate {FormatDouble(rawGate)}.",
    $"Best bridge relative spread is {FormatNullable(bestBridgeSpread)} against gate {FormatDouble(bridgeGate)}.",
    $"Selected historical pair (0,3) min raw-to-target ratio is {FormatNullable(selectedPair03MinRaw)}.",
    sectorEvidencePresent
        ? "P146 found local fermion-sector evidence."
        : "P146 found no non-synthetic local fermion-sector evidence candidate.",
};

var closureRequirements = rootCause switch
{
    "projection-candidate-ready" => new[]
    {
        "rerun Phase116 using the Phase122 projection candidate",
        "rerun Phase150 and Phase151",
    },
    "sector-evidence-present-rerun-required" => new[]
    {
        "apply the present fermion-sector evidence through P140-P142",
        "rerun P122 corrected-operator transition sweep",
        "rerun Phase116 only if P122 emits a projection candidate",
    },
    "operator-normalization-open" => new[]
    {
        "repair Phase120 analytic variation measure before rerunning transition selection",
    },
    _ => new[]
    {
        "derive or supply target-independent fermion-sector transition evidence",
        "apply the evidence through P140-P146",
        "rerun Phase122 and require a projection candidate before another W/Z absolute projection rerun",
        "keep W and Z absolute mass rows unpromoted in Phase151 until passed=true and promotionAllowed=true",
    },
};

var result = new
{
    phaseId = "phase154-wz-transition-bridge-root-cause-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    rootCause,
    blockerFacts,
    gates = new
    {
        rawToTargetGate = rawGate,
        commonBridgeSpreadGate = bridgeGate,
        analyticMeasurePassed,
        projectionCandidateFound,
        transitionGraphPromotable,
        routeClosureReady,
        sectorEvidencePresent,
    },
    transitionDiagnostics = new
    {
        phase122Status = JsonString(phase122.RootElement, "terminalStatus"),
        transitionCount = JsonInt(phase122.RootElement, "transitionCount"),
        qualityTransitionCount = JsonInt(phase122.RootElement, "qualityTransitionCount"),
        strongestTransitionId = JsonString(strongest, "transitionId"),
        strongestMinRawToTargetRatio = strongestMinRaw,
        strongestCommonBridgeRelativeSpread = JsonDouble(strongest, "commonBridgeRelativeSpread"),
        bestBridgeTransitionId = JsonString(bestBridge, "transitionId"),
        bestBridgeMinRawToTargetRatio = JsonDouble(bestBridge, "minRawToTargetRatio"),
        bestBridgeCommonBridgeRelativeSpread = bestBridgeSpread,
        selectedPair03MinRawToTargetRatio = selectedPair03MinRaw,
        selectedPair03CommonBridgeRelativeSpread = JsonDouble(selectedPair03, "commonBridgeRelativeSpread"),
    },
    upstreamStatuses = new
    {
        phase120Status = JsonString(phase120.RootElement, "terminalStatus"),
        phase138Status = JsonString(phase138.RootElement, "terminalStatus"),
        phase139Status = JsonString(phase139.RootElement, "terminalStatus"),
        phase146Status = JsonString(phase146.RootElement, "terminalStatus"),
        phase151Status = JsonString(phase151.RootElement, "terminalStatus"),
        phase153Status = JsonString(phase153.RootElement, "terminalStatus"),
    },
    closureRequirements,
    nextPhaseRecommendation = new
    {
        phaseId = "phase155-fermion-sector-transition-evidence-derivation",
        objective = "derive target-independent fermion-sector transition evidence or a bridge revision that gives Phase122 a corrected-operator projection candidate",
        requiredOutputs = new[]
        {
            "sector or transition evidence accepted by P140-P146",
            "Phase122 projectionCandidate is non-null",
            "Phase116 rerun passes W/Z target comparison before Phase151 promotion",
        },
    },
    sourceEvidence = new
    {
        phase120Path = Phase120Path,
        phase122Path = Phase122Path,
        phase138Path = Phase138Path,
        phase139Path = Phase139Path,
        phase146Path = Phase146Path,
        phase151Path = Phase151Path,
        phase153Path = Phase153Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_transition_bridge_root_cause_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_transition_bridge_root_cause_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.rootCause,
        result.blockerFacts,
        result.gates,
        result.transitionDiagnostics,
        result.upstreamStatuses,
        result.closureRequirements,
        result.nextPhaseRecommendation,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_transition_bridge_root_cause_audit.md"),
    BuildMarkdown(terminalStatus, rootCause, blockerFacts, closureRequirements));

Console.WriteLine(terminalStatus);
Console.WriteLine($"rootCause={rootCause}");
Console.WriteLine($"projectionCandidateFound={projectionCandidateFound}");
Console.WriteLine($"sectorEvidencePresent={sectorEvidencePresent}");
Console.WriteLine("nextPhase=phase155-fermion-sector-transition-evidence-derivation");

static string BuildMarkdown(
    string terminalStatus,
    string rootCause,
    IReadOnlyList<string> blockerFacts,
    IReadOnlyList<string> closureRequirements)
{
    var builder = new StringBuilder();
    builder.AppendLine("# W/Z Transition Bridge Root-Cause Audit");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    builder.AppendLine();
    builder.AppendLine($"Root cause: `{rootCause}`");
    builder.AppendLine();
    builder.AppendLine("## Findings");
    foreach (var fact in blockerFacts)
        builder.AppendLine($"- {fact}");
    builder.AppendLine();
    builder.AppendLine("## Closure Requirements");
    foreach (var requirement in closureRequirements)
        builder.AppendLine($"- {requirement}");
    return builder.ToString();
}

static bool IsNull(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;
static string FormatNullable(double? value) => value is null ? "none" : FormatDouble(value.Value);
static string FormatDouble(double value) => value.ToString("G12", CultureInfo.InvariantCulture);
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
