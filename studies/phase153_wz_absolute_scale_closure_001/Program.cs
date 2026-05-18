using System.Globalization;
using System.Text;
using System.Text.Json;

const string DefaultOutputDir = "studies/phase153_wz_absolute_scale_closure_001/output";
const string Phase152Path = "studies/phase152_boson_blocker_resolution_plan_001/output/boson_blocker_resolution_plan.json";
const string Phase72Path = "studies/phase72_wz_absolute_scale_calibration_001/wz_absolute_scale_calibration.json";
const string Phase73Path = "studies/phase73_wz_absolute_mass_projection_001/wz_absolute_mass_projection.json";
const string Phase74Path = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase75Path = "studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json";
const string Phase76Path = "studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase113Path = "studies/phase113_wz_absolute_repair_attempt_gate_001/output/wz_absolute_repair_attempt_gate.json";
const string Phase116Path = "studies/phase116_wz_absolute_projection_rerun_001/output/wz_absolute_projection_rerun.json";
const string Phase117Path = "studies/phase117_wz_repaired_pair_sweep_001/output/wz_repaired_pair_sweep.json";
const string Phase118Path = "studies/phase118_wz_matrix_element_normalization_diagnostic_001/output/wz_matrix_element_normalization_diagnostic.json";
const string Phase119Path = "studies/phase119_operator_source_scale_derivation_audit_001/output/operator_source_scale_derivation_audit.json";
const string Phase120Path = "studies/phase120_analytic_variation_measure_consistency_001/output/analytic_variation_measure_consistency.json";
const string Phase122Path = "studies/phase122_corrected_operator_selection_rule_sweep_001/output/corrected_operator_selection_rule_sweep.json";
const string Phase151Path = "studies/phase151_validated_boson_prediction_generator_001/output/validated_boson_predictions.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE153_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase152 = JsonDocument.Parse(File.ReadAllText(Phase152Path));
using var phase72 = JsonDocument.Parse(File.ReadAllText(Phase72Path));
using var phase73 = JsonDocument.Parse(File.ReadAllText(Phase73Path));
using var phase74 = JsonDocument.Parse(File.ReadAllText(Phase74Path));
using var phase75 = JsonDocument.Parse(File.ReadAllText(Phase75Path));
using var phase76 = JsonDocument.Parse(File.ReadAllText(Phase76Path));
using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));
using var phase113 = JsonDocument.Parse(File.ReadAllText(Phase113Path));
using var phase116 = JsonDocument.Parse(File.ReadAllText(Phase116Path));
using var phase117 = JsonDocument.Parse(File.ReadAllText(Phase117Path));
using var phase118 = JsonDocument.Parse(File.ReadAllText(Phase118Path));
using var phase119 = JsonDocument.Parse(File.ReadAllText(Phase119Path));
using var phase120 = JsonDocument.Parse(File.ReadAllText(Phase120Path));
using var phase122 = JsonDocument.Parse(File.ReadAllText(Phase122Path));
using var phase151 = JsonDocument.Parse(File.ReadAllText(Phase151Path));

var originalComparisons = ReadComparisons(phase74.RootElement.GetProperty("comparisons"));
var rerunComparisons = ReadComparisons(phase116.RootElement.GetProperty("targetComparison").GetProperty("comparisons"));
var originalPassed = string.Equals(JsonString(phase74.RootElement, "terminalStatus"), "wz-absolute-mass-target-comparison-passed", StringComparison.Ordinal);
var rerunPassed = string.Equals(JsonString(phase116.RootElement, "comparisonStatus"), "wz-absolute-mass-target-comparison-passed", StringComparison.Ordinal);
var projectionRerunAllowed = JsonBool(phase113.RootElement, "projectionRerunAllowed") is true;
var promotableOperatorScaleFound = JsonBool(phase119.RootElement, "promotableAmplitudeScaleFound") is true;
var analyticVariationMeasureFound = JsonBool(phase120.RootElement, "promotableAmplitudeMeasureFound") is true;
var correctedOperatorProjectionCandidateFound = string.Equals(
    JsonString(phase122.RootElement, "terminalStatus"),
    "corrected-operator-selection-rule-sweep-found-projection-candidate",
    StringComparison.Ordinal);
var pairRepairFound = string.Equals(JsonString(phase117.RootElement, "terminalStatus"), "wz-repaired-pair-sweep-repair-found", StringComparison.Ordinal);
var commonScaleGatePassed = JsonBool(phase119.RootElement.GetProperty("amplitudeScaleRequirement"), "passesCommonScaleGate") is true;

string terminalStatus = originalPassed || rerunPassed
    ? "wz-absolute-scale-closure-promotable"
    : correctedOperatorProjectionCandidateFound
        ? "wz-absolute-scale-closure-ready-for-corrected-projection-rerun"
    : analyticVariationMeasureFound
        ? "wz-absolute-scale-closure-blocked-transition-bridge"
    : promotableOperatorScaleFound && commonScaleGatePassed
        ? "wz-absolute-scale-closure-ready-for-rerun"
        : "wz-absolute-scale-closure-blocked-operator-source-scale";

var closureRequirements = terminalStatus switch
{
    "wz-absolute-scale-closure-promotable" => Array.Empty<string>(),
    "wz-absolute-scale-closure-ready-for-rerun" =>
    [
        "rerun Phase115 using the promotable operator/source amplitude scale",
        "rerun Phase116 W/Z absolute projection and target comparison",
        "rerun Phase150 and Phase151 before promoting W/Z rows",
    ],
    "wz-absolute-scale-closure-ready-for-corrected-projection-rerun" =>
    [
        "rerun Phase115 and Phase116 with the Phase122 corrected-operator projection candidate",
        "rerun Phase150 and Phase151 before promoting W/Z rows",
    ],
    "wz-absolute-scale-closure-blocked-transition-bridge" =>
    [
        "use Phase120 as the accepted analytic variation measure instead of reopening the operator normalization issue",
        "derive the physical fermion transition/sector selection rule for W/Z source modes",
        "or revise the W/Z bridge construction so a target-independent common bridge is available",
        "rerun Phase122 only after transition/sector or bridge evidence changes",
        "do not promote W/Z absolute masses while Phase151 records them as failed-comparison-attempt-not-promoted",
    ],
    _ => new[]
    {
        "derive the connection-mode-to-Dirac-variation amplitude measure from the analytic Dirac operator discretization",
        "materialize a target-independent operator/source-scale artifact with value, uncertainty, unit convention, and W/Z source-mode scope",
        "validate a common W/Z amplitude scale without using physical W/Z target masses",
        "rerun Phase115 and Phase116 only after the derived scale passes the common-scale gate",
        "do not promote W/Z absolute masses while Phase151 records them as failed-comparison-attempt-not-promoted",
    },
};

var decision = new
{
    originalProjectionStatus = JsonString(phase73.RootElement, "status"),
    originalComparisonStatus = JsonString(phase74.RootElement, "terminalStatus"),
    repairedProjectionStatus = JsonString(phase116.RootElement, "projectionStatus"),
    repairedComparisonStatus = JsonString(phase116.RootElement, "comparisonStatus"),
    projectionRerunAllowedByRepairGate = projectionRerunAllowed,
    repairedPairSweepStatus = JsonString(phase117.RootElement, "terminalStatus"),
    pairRepairFound,
    operatorScaleAuditStatus = JsonString(phase119.RootElement, "terminalStatus"),
    promotableOperatorScaleFound,
    analyticVariationMeasureStatus = JsonString(phase120.RootElement, "terminalStatus"),
    analyticVariationMeasureFound,
    correctedOperatorSweepStatus = JsonString(phase122.RootElement, "terminalStatus"),
    correctedOperatorProjectionCandidateFound,
    commonScaleGatePassed,
    promotionAllowed = terminalStatus == "wz-absolute-scale-closure-promotable",
    reason = terminalStatus == "wz-absolute-scale-closure-promotable"
        ? "at least one W/Z absolute-mass comparison path passed"
        : analyticVariationMeasureFound
            ? "analytic operator normalization is closed by Phase120, but Phase122 finds no target-independent fermion transition/bridge candidate that can support W/Z absolute mass projection"
            : "available W/Z absolute-mass projection paths fail target comparison and no target-independent operator/source amplitude scale is promotable",
};

var scaleDiagnostics = new
{
    phase72ScaleFactorGeVPerInternalMassUnit = JsonDouble(phase72.RootElement, "scaleFactorGeVPerInternalMassUnit"),
    phase72ScaleUncertaintyGeVPerInternalMassUnit = JsonDouble(phase72.RootElement, "scaleUncertaintyGeVPerInternalMassUnit"),
    phase75MeanRequiredScaleFactor = JsonDouble(phase75.RootElement, "meanRequiredScaleFactor"),
    phase75RelativeRequiredScaleSpread = JsonDouble(phase75.RootElement, "relativeRequiredScaleSpread"),
    phase76RawMatrixElementRequiredScale = JsonDouble(phase76.RootElement, "rawMatrixElementRequiredScale"),
    phase118RawRequiredScaleMean = JsonDouble(phase118.RootElement.GetProperty("repairedReplay"), "rawRequiredScaleMean"),
    phase118RawRequiredScaleRelativeSpread = JsonDouble(phase118.RootElement.GetProperty("repairedReplay"), "rawRequiredScaleRelativeSpread"),
    phase119RequiredRawScaleMean = JsonDouble(phase119.RootElement.GetProperty("amplitudeScaleRequirement"), "requiredRawScaleMean"),
    phase119RequiredRawScaleRelativeSpread = JsonDouble(phase119.RootElement.GetProperty("amplitudeScaleRequirement"), "requiredRawScaleRelativeSpread"),
    phase119SpreadTolerance = JsonDouble(phase119.RootElement.GetProperty("amplitudeScaleRequirement"), "spreadTolerance"),
    phase120AcceptedFiniteDifferenceToAnalyticScale = phase120.RootElement.TryGetProperty("acceptedScale", out var acceptedScale)
        ? JsonDouble(acceptedScale, "finiteDifferenceToAnalyticScale")
        : null,
    phase120CommonScaleRelativeSpread = phase120.RootElement.TryGetProperty("validationGate", out var validationGate)
        ? JsonDouble(validationGate, "commonScaleRelativeSpread")
        : null,
    phase122StrongestMinRawToTargetRatio = phase122.RootElement.TryGetProperty("strongest", out var strongest)
        ? JsonDouble(strongest, "minRawToTargetRatio")
        : null,
    phase122BestBridgeRelativeSpread = phase122.RootElement.TryGetProperty("bestBridge", out var bestBridge)
        ? JsonDouble(bestBridge, "commonBridgeRelativeSpread")
        : null,
};

var result = new
{
    phaseId = "phase153-wz-absolute-scale-closure",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    phase152Status = JsonString(phase152.RootElement, "terminalStatus"),
    phase151Status = JsonString(phase151.RootElement, "terminalStatus"),
    originalComparisons,
    rerunComparisons,
    decision,
    scaleDiagnostics,
    closureRequirements,
    nextPhaseRecommendation = new
    {
        phaseId = analyticVariationMeasureFound
            ? "phase154-wz-transition-bridge-root-cause-audit"
            : "phase154-operator-source-amplitude-scale-derivation",
        objective = analyticVariationMeasureFound
            ? "derive the target-independent W/Z fermion transition/sector rule or bridge revision required after Phase120 closes analytic operator normalization"
            : "derive and materialize the missing target-independent operator/source amplitude scale required before W/Z absolute projection can rerun promotably",
        acceptanceGates = new[]
        {
            "new evidence is derived from target-independent source, transition, or bridge structure, not from W/Z target residuals",
            "Phase122 finds a corrected-operator projection candidate or a revised bridge candidate",
            "Phase116 rerun no longer fails due to transition/bridge inconsistency",
            "Phase151 promotes W and Z only after passed=true and promotionAllowed=true",
        },
    },
    sourceEvidence = new
    {
        phase152Path = Phase152Path,
        phase72Path = Phase72Path,
        phase73Path = Phase73Path,
        phase74Path = Phase74Path,
        phase75Path = Phase75Path,
        phase76Path = Phase76Path,
        phase110Path = Phase110Path,
        phase113Path = Phase113Path,
        phase116Path = Phase116Path,
        phase117Path = Phase117Path,
        phase118Path = Phase118Path,
        phase119Path = Phase119Path,
        phase120Path = Phase120Path,
        phase122Path = Phase122Path,
        phase151Path = Phase151Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_absolute_scale_closure.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_absolute_scale_closure_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.phase151Status,
        result.originalComparisons,
        result.rerunComparisons,
        result.decision,
        result.scaleDiagnostics,
        result.closureRequirements,
        result.nextPhaseRecommendation,
    }, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_absolute_scale_closure.md"),
    BuildMarkdown(terminalStatus, originalComparisons, rerunComparisons, decision, scaleDiagnostics, closureRequirements));

Console.WriteLine(terminalStatus);
Console.WriteLine($"originalPassed={originalPassed}");
Console.WriteLine($"rerunPassed={rerunPassed}");
Console.WriteLine($"promotableOperatorScaleFound={promotableOperatorScaleFound}");
Console.WriteLine($"nextPhase={(analyticVariationMeasureFound ? "phase154-wz-transition-bridge-root-cause-audit" : "phase154-operator-source-amplitude-scale-derivation")}");

static IReadOnlyList<ComparisonRow> ReadComparisons(JsonElement comparisons) =>
    comparisons.EnumerateArray()
        .Select(row => new ComparisonRow(
            ObservableId: RequiredString(row, "observableId"),
            PredictedValue: RequiredDouble(row, "predictedValue"),
            PredictedUncertainty: RequiredDouble(row, "predictedUncertainty"),
            TargetValue: RequiredDouble(row, "targetValue"),
            TargetUncertainty: RequiredDouble(row, "targetUncertainty"),
            SigmaResidual: RequiredDouble(row, "sigmaResidual"),
            Passed: JsonBool(row, "passed") is true))
        .ToArray();

static string BuildMarkdown(
    string terminalStatus,
    IReadOnlyList<ComparisonRow> originalComparisons,
    IReadOnlyList<ComparisonRow> rerunComparisons,
    object decision,
    object scaleDiagnostics,
    IReadOnlyList<string> closureRequirements)
{
    var builder = new StringBuilder();
    builder.AppendLine("# W/Z Absolute Scale Closure");
    builder.AppendLine();
    builder.AppendLine($"Terminal status: `{terminalStatus}`");
    builder.AppendLine();
    builder.AppendLine("## Original Projection");
    AppendComparisonTable(builder, originalComparisons);
    builder.AppendLine();
    builder.AppendLine("## Repaired Projection Rerun");
    AppendComparisonTable(builder, rerunComparisons);
    builder.AppendLine();
    builder.AppendLine("## Decision");
    builder.AppendLine("W/Z absolute masses are not promotable unless a comparison path passes and Phase151 can mark the rows `promotionAllowed=true` and `passed=true`.");
    builder.AppendLine();
    builder.AppendLine("```json");
    builder.AppendLine(JsonSerializer.Serialize(decision, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    builder.AppendLine("```");
    builder.AppendLine();
    builder.AppendLine("## Scale Diagnostics");
    builder.AppendLine("```json");
    builder.AppendLine(JsonSerializer.Serialize(scaleDiagnostics, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    builder.AppendLine("```");
    builder.AppendLine();
    builder.AppendLine("## Closure Requirements");
    foreach (var requirement in closureRequirements)
        builder.AppendLine($"- {requirement}");
    return builder.ToString();
}

static void AppendComparisonTable(StringBuilder builder, IReadOnlyList<ComparisonRow> comparisons)
{
    builder.AppendLine("| Observable | Prediction | Target | Sigma | Passed |");
    builder.AppendLine("|---|---:|---:|---:|---|");
    foreach (var comparison in comparisons)
    {
        builder.Append("| ");
        builder.Append(comparison.ObservableId);
        builder.Append(" | ");
        builder.Append(FormatValue(comparison.PredictedValue, comparison.PredictedUncertainty));
        builder.Append(" | ");
        builder.Append(FormatValue(comparison.TargetValue, comparison.TargetUncertainty));
        builder.Append(" | ");
        builder.Append(FormatDouble(comparison.SigmaResidual));
        builder.Append(" | ");
        builder.Append(comparison.Passed ? "yes" : "no");
        builder.AppendLine(" |");
    }
}

static string FormatValue(double value, double uncertainty) => $"{FormatDouble(value)} +/- {FormatDouble(uncertainty)}";
static string FormatDouble(double value) => value.ToString("G12", CultureInfo.InvariantCulture);
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record ComparisonRow(
    string ObservableId,
    double PredictedValue,
    double PredictedUncertainty,
    double TargetValue,
    double TargetUncertainty,
    double SigmaResidual,
    bool Passed);
