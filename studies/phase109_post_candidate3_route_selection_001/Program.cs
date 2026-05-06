using System.Text.Json;

const string DefaultOutputDir = "studies/phase109_post_candidate3_route_selection_001/output";
const string Phase51ReadinessPath = "studies/phase51_broad_boson_prediction_readiness_001/broad_boson_prediction_readiness.json";
const string Phase74ComparisonPath = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase75MissDiagnosticPath = "studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json";
const string Phase76NormalizationAuditPath = "studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json";
const string Phase108ClosurePath = "studies/phase108_candidate3_physical_comparison_closure_001/output/candidate3_physical_comparison_closure.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE109_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase51 = JsonDocument.Parse(File.ReadAllText(Phase51ReadinessPath));
using var phase74 = JsonDocument.Parse(File.ReadAllText(Phase74ComparisonPath));
using var phase75 = JsonDocument.Parse(File.ReadAllText(Phase75MissDiagnosticPath));
using var phase76 = JsonDocument.Parse(File.ReadAllText(Phase76NormalizationAuditPath));
using var phase108 = JsonDocument.Parse(File.ReadAllText(Phase108ClosurePath));

var records = phase51.RootElement.GetProperty("records").EnumerateArray().ToList();
var wzRatio = records.Single(r => JsonString(r, "recordId") == "phase51-wz-ratio");
var wMass = records.Single(r => JsonString(r, "recordId") == "phase51-w-absolute-mass");
var zMass = records.Single(r => JsonString(r, "recordId") == "phase51-z-absolute-mass");

var result = new
{
    phaseId = "phase109-post-candidate3-route-selection",
    terminalStatus = "post-candidate3-next-route-selected",
    closedRoute = new
    {
        routeId = "phase99-candidate3-internal-replay",
        closureStatus = JsonString(phase108.RootElement, "terminalStatus"),
        allowedPredictionLevel = JsonString(phase108.RootElement, "finalAllowedPredictionLevel"),
        reopenCondition = "new target-independent observable identity evidence for candidate-3",
    },
    activeValidatedRoute = new
    {
        routeId = JsonString(wzRatio, "recordId"),
        targetObservableId = JsonString(wzRatio, "targetObservableId"),
        readinessStatus = JsonString(wzRatio, "readinessStatus"),
        computedValue = JsonDouble(wzRatio, "computedValue"),
        computedUncertainty = JsonDouble(wzRatio, "computedUncertainty"),
        targetValue = JsonDouble(wzRatio, "targetValue"),
        targetUncertainty = JsonDouble(wzRatio, "targetUncertainty"),
        pull = JsonDouble(wzRatio, "pull"),
        role = "current tangible physical-ratio prediction route",
    },
    selectedNextRoute = new
    {
        routeId = "wz-absolute-mass-repair",
        routeStatus = "blocked-but-localized",
        targetObservableIds = new[]
        {
            JsonString(wMass, "targetObservableId"),
            JsonString(zMass, "targetObservableId"),
        },
        phase74Status = JsonString(phase74.RootElement, "terminalStatus"),
        meanRequiredScaleFactor = JsonDouble(phase75.RootElement, "meanRequiredScaleFactor"),
        relativeRequiredScaleSpread = JsonDouble(phase75.RootElement, "relativeRequiredScaleSpread"),
        requiredWeakCoupling = JsonDouble(phase75.RootElement, "requiredWeakCoupling"),
        targetImpliedRawMatrixElementMagnitude = JsonDouble(phase76.RootElement, "targetImpliedRawMatrixElementMagnitude"),
        diagnosis = "absolute W/Z miss is coherent; repair weak-coupling amplitude normalization or scalar-sector relation before rerunning absolute comparison",
    },
    nextPhaseInputsReady = new[]
    {
        Phase51ReadinessPath,
        Phase74ComparisonPath,
        Phase75MissDiagnosticPath,
        Phase76NormalizationAuditPath,
    },
    nextPhaseAcceptanceCriteria = new[]
    {
        "produce a replayed analytic raw matrix element or revised scalar-sector relation without fitting W/Z mass targets",
        "rerun absolute W/Z mass projection",
        "rerun W/Z absolute target comparison",
        "if comparison still fails, emit a miss diagnostic; if it passes, promote only with disclosed target-scoped falsifier policy",
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "post_candidate3_route_selection.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "post_candidate3_route_selection_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase109-post-candidate3-route-selection",
        result.terminalStatus,
        activeRoute = result.activeValidatedRoute.routeId,
        selectedNextRoute = result.selectedNextRoute.routeId,
        result.selectedNextRoute.routeStatus,
        result.selectedNextRoute.requiredWeakCoupling,
        result.selectedNextRoute.targetImpliedRawMatrixElementMagnitude,
    }, options));

Console.WriteLine(result.terminalStatus);

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;
