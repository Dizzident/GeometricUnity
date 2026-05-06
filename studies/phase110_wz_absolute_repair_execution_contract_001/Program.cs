using System.Text.Json;

const string DefaultOutputDir = "studies/phase110_wz_absolute_repair_execution_contract_001/output";
const string Phase109RouteSelectionPath = "studies/phase109_post_candidate3_route_selection_001/output/post_candidate3_route_selection.json";
const string Phase76NormalizationAuditPath = "studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json";
const string Phase79ReplayHarnessPath = "studies/phase79_analytic_weak_coupling_replay_harness_001/analytic_weak_coupling_replay_harness.json";
const string Phase84ReplayProbePath = "studies/phase99_selector_eigenvector_full_lift_001/output/replay_probe_4x4/first_boson_prediction_attempt.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE110_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var route = JsonDocument.Parse(File.ReadAllText(Phase109RouteSelectionPath));
using var normalization = JsonDocument.Parse(File.ReadAllText(Phase76NormalizationAuditPath));
using var harness = JsonDocument.Parse(File.ReadAllText(Phase79ReplayHarnessPath));
using var replayProbe = JsonDocument.Parse(File.ReadAllText(Phase84ReplayProbePath));

var contract = new
{
    phaseId = "phase110-wz-absolute-repair-execution-contract",
    terminalStatus = "wz-absolute-repair-contract-ready",
    selectedRouteId = "wz-absolute-mass-repair",
    repairTarget = new
    {
        currentWeakCoupling = JsonDouble(normalization.RootElement, "currentWeakCoupling"),
        targetImpliedWeakCoupling = JsonDouble(normalization.RootElement, "targetImpliedWeakCoupling"),
        targetImpliedRawMatrixElementMagnitude = JsonDouble(normalization.RootElement, "targetImpliedRawMatrixElementMagnitude"),
        rawMatrixElementRequiredScale = JsonDouble(normalization.RootElement, "rawMatrixElementRequiredScale"),
    },
    allowedRepairStrategies = new[]
    {
        new
        {
            strategyId = "replayed-analytic-raw-matrix-element",
            requiredOutput = "validated raw matrix-element evidence replacing the historical Phase65 scalar input",
            targetValuesAllowed = false,
        },
        new
        {
            strategyId = "scalar-sector-relation-revision",
            requiredOutput = "target-independent scalar-sector relation revision with propagated uncertainty",
            targetValuesAllowed = false,
        },
    },
    disallowedStrategies = new[]
    {
        "fit a scale factor from W/Z target residuals",
        "declare the Phase99 candidate-3 internal coupling proxy to be a physical weak coupling without identity derivation",
        "promote absolute masses while Phase74-style target comparison fails",
    },
    requiredExecutionSteps = new[]
    {
        "select one allowed repair strategy",
        "emit target-independent repaired weak-coupling or scalar-relation evidence",
        "rerun absolute W/Z mass projection",
        "rerun absolute W/Z target comparison",
        "rerun readiness/promotion gates with target-scoped falsifier policy disclosed",
    },
    sourceEvidence = new
    {
        routeSelectionPath = Phase109RouteSelectionPath,
        weakCouplingNormalizationAuditPath = Phase76NormalizationAuditPath,
        analyticReplayHarnessPath = Phase79ReplayHarnessPath,
        currentReplayProbePath = Phase84ReplayProbePath,
        currentReplayTerminalStatus = JsonString(replayProbe.RootElement, "terminalStatus"),
        harnessStatus = JsonString(harness.RootElement, "terminalStatus"),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "wz_absolute_repair_execution_contract.json"),
    JsonSerializer.Serialize(contract, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_absolute_repair_execution_contract_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase110-wz-absolute-repair-execution-contract",
        contract.terminalStatus,
        contract.selectedRouteId,
        contract.repairTarget.targetImpliedWeakCoupling,
        contract.repairTarget.targetImpliedRawMatrixElementMagnitude,
    }, options));

Console.WriteLine(contract.terminalStatus);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;
