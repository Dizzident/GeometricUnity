using System.Text.Json;

const string DefaultOutputDir = "studies/phase221_su2_casimir_wz_normalization_probe_001/output";
const string Phase63Path = "studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json";
const string Phase65Path = "studies/phase65_dimensionless_weak_coupling_amplitude_001/dimensionless_weak_coupling_amplitude.json";
const string Phase68Path = "studies/phase68_normalized_weak_coupling_candidate_promotion_001/normalized_weak_coupling_candidate_promotion.json";
const string Phase74Path = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";
const string Phase75Path = "studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json";
const string Phase76Path = "studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json";
const string Phase77Path = "studies/phase77_raw_weak_coupling_matrix_element_evidence_gate_001/raw_matrix_element_evidence_gate.json";
const string Phase197Path = "studies/phase197_electroweak_weak_coupling_wz_mass_closure_audit_001/output/electroweak_weak_coupling_wz_mass_closure_audit_summary.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE221_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase63 = JsonDocument.Parse(File.ReadAllText(Phase63Path));
using var phase65 = JsonDocument.Parse(File.ReadAllText(Phase65Path));
using var phase68 = JsonDocument.Parse(File.ReadAllText(Phase68Path));
using var phase74 = JsonDocument.Parse(File.ReadAllText(Phase74Path));
using var phase75 = JsonDocument.Parse(File.ReadAllText(Phase75Path));
using var phase76 = JsonDocument.Parse(File.ReadAllText(Phase76Path));
using var phase77 = JsonDocument.Parse(File.ReadAllText(Phase77Path));
using var phase197 = JsonDocument.Parse(File.ReadAllText(Phase197Path));

var currentScale = JsonDouble(phase65.RootElement, "generatorNormalizationScale") ?? JsonDouble(phase63.RootElement, "internalToPhysicalGeneratorScale");
var rawMatrixElement = JsonDouble(phase65.RootElement, "rawMatrixElementMagnitude");
var promotedWeakCoupling = phase68.RootElement.TryGetProperty("candidate", out var promotedCandidate)
    ? JsonDouble(promotedCandidate, "couplingValue")
    : null;
var targetImpliedWeakCoupling = JsonDouble(phase75.RootElement, "requiredWeakCoupling");
var currentWeakCoupling = JsonDouble(phase75.RootElement, "currentWeakCoupling") ?? promotedWeakCoupling;

const double SpinOneCasimir = 2.0;
const double Su2Dimension = 3.0;
var casimirRmsScale = Math.Sqrt(SpinOneCasimir / Su2Dimension);
var casimirToTraceHalfScale = currentScale is > 0.0 ? casimirRmsScale / currentScale.Value : (double?)null;
var casimirWeakCoupling = rawMatrixElement is > 0.0 ? rawMatrixElement.Value * casimirRmsScale : (double?)null;
var targetImpliedScaleRatio = currentWeakCoupling is > 0.0 && targetImpliedWeakCoupling is > 0.0
    ? targetImpliedWeakCoupling.Value / currentWeakCoupling.Value
    : (double?)null;
var casimirScaleRatioMiss = casimirToTraceHalfScale is not null && targetImpliedScaleRatio is not null
    ? casimirToTraceHalfScale.Value - targetImpliedScaleRatio.Value
    : (double?)null;
var casimirWeakCouplingDelta = casimirWeakCoupling is not null && targetImpliedWeakCoupling is not null
    ? casimirWeakCoupling.Value - targetImpliedWeakCoupling.Value
    : (double?)null;

var comparisons = phase74.RootElement.GetProperty("comparisons")
    .EnumerateArray()
    .Select(row =>
    {
        var predictedValue = JsonDouble(row, "predictedValue");
        var predictedUncertainty = JsonDouble(row, "predictedUncertainty");
        var targetValue = JsonDouble(row, "targetValue");
        var targetUncertainty = JsonDouble(row, "targetUncertainty");
        var rescaledPredictedValue = predictedValue is not null && casimirToTraceHalfScale is not null
            ? predictedValue.Value * casimirToTraceHalfScale.Value
            : (double?)null;
        var rescaledPredictedUncertainty = predictedUncertainty is not null && casimirToTraceHalfScale is not null
            ? predictedUncertainty.Value * casimirToTraceHalfScale.Value
            : (double?)null;
        var combinedUncertainty = rescaledPredictedUncertainty is not null && targetUncertainty is not null
            ? Math.Sqrt(Math.Pow(rescaledPredictedUncertainty.Value, 2.0) + Math.Pow(targetUncertainty.Value, 2.0))
            : (double?)null;
        var sigmaResidual = rescaledPredictedValue is not null && targetValue is not null && combinedUncertainty is > 0.0
            ? Math.Abs(rescaledPredictedValue.Value - targetValue.Value) / combinedUncertainty.Value
            : (double?)null;

        return new
        {
            observableId = JsonString(row, "observableId"),
            traceHalfPredictedValue = predictedValue,
            targetValue,
            casimirRescaledPredictedValue = rescaledPredictedValue,
            casimirRescaledPredictedUncertainty = rescaledPredictedUncertainty,
            targetUncertainty,
            sigmaResidual,
            passed = sigmaResidual is <= 3.0,
        };
    })
    .ToArray();

var numericalTargetComparisonPassed = comparisons.Length > 0 && comparisons.All(row => row.passed);
var traceHalfConventionDerived = string.Equals(JsonString(phase63.RootElement, "terminalStatus"), "su2-generator-normalization-derived", StringComparison.Ordinal)
    && string.Equals(JsonString(phase63.RootElement, "normalizationConventionId"), "physical-weak-coupling-normalization:su2-canonical-trace-half-v1", StringComparison.Ordinal);
var phase76RequiresRawOrScalarRevision = JsonStringArray(phase76.RootElement, "closureRequirements")
    .Any(item => item.Contains("replace the Phase65 scalar raw matrix-element input", StringComparison.Ordinal));
var phase77BlocksRawEvidence = string.Equals(JsonString(phase77.RootElement, "terminalStatus"), "raw-weak-coupling-matrix-element-evidence-blocked", StringComparison.Ordinal);
var currentClosureFailed = JsonBool(phase197.RootElement, "canPromoteWzFromWeakCouplingMassRelation") is false;

var blockers = new List<string>();
if (!traceHalfConventionDerived)
    blockers.Add("Phase63 trace-half SU(2) convention is not derived, so there is no baseline normalization to compare.");
if (rawMatrixElement is null || rawMatrixElement <= 0.0)
    blockers.Add("Phase65 raw matrix-element magnitude is missing.");
if (!numericalTargetComparisonPassed)
    blockers.Add("Casimir/RMS rescaled W/Z values do not pass target comparison.");
blockers.Add("No upstream GU source artifact derives replacing the Phase63 trace-half single-generator normalization with an SU(2) spin-1 Casimir/RMS triplet normalization for the Phase64 matrix element.");
blockers.Add("Phase64 matrix-element formula is a single bosonic perturbation matrix element, not a proven isotropic SU(2)-triplet RMS amplitude.");
if (phase76RequiresRawOrScalarRevision)
    blockers.Add("Phase76 requires a replayed raw matrix-element or scalar-sector revision; a post-hoc normalization swap alone is not accepted.");
if (phase77BlocksRawEvidence)
    blockers.Add("Phase77 blocks the Phase65 raw 0.8 amplitude as scalar-study-input with unknown normalization rather than replayed analytic matrix-element evidence.");
if (currentClosureFailed)
    blockers.Add("Phase197 current weak-coupling W/Z closure fails and remains the active promoted-lineage gate.");

var sourceLineagePromotable = blockers.Count == 0;
var terminalStatus = numericalTargetComparisonPassed
    ? "su2-casimir-wz-normalization-probe-numerically-successful-not-promotable"
    : "su2-casimir-wz-normalization-probe-numerically-failed";

var result = new
{
    phaseId = "phase221-su2-casimir-wz-normalization-probe",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    numericalTargetComparisonPassed,
    sourceLineagePromotable,
    probeKind = "su2-spin-one-casimir-rms-normalization",
    derivationHypothesis = "Replace the single-generator trace-half scale sqrt(1/2) with the spin-1 SU(2) isotropic RMS generator scale sqrt(C2(adj)/dim su2)=sqrt(2/3), equivalently multiplying the Phase65 weak coupling by 2/sqrt(3).",
    currentTraceHalfScale = currentScale,
    casimirRmsScale,
    casimirToTraceHalfScale,
    targetImpliedScaleRatio,
    casimirScaleRatioMiss,
    rawMatrixElement,
    currentWeakCoupling,
    targetImpliedWeakCoupling,
    casimirWeakCoupling,
    casimirWeakCouplingDelta,
    comparisons,
    blockers,
    decision = numericalTargetComparisonPassed
        ? "The SU(2) Casimir/RMS factor is a strong numerical lead for W/Z absolute masses, but it is not scientifically promotable without a derivation that the Phase64 amplitude should use triplet RMS normalization rather than the Phase63 trace-half single-generator convention."
        : "The SU(2) Casimir/RMS factor does not solve the W/Z absolute target comparison.",
    nextRequiredArtifact = "A target-independent normalization/source-lineage derivation showing that the Phase64 weak current matrix element is an isotropic SU(2) spin-1 triplet RMS amplitude, plus application through Phase201/P209/P210/P213 gates.",
    sourceEvidence = new
    {
        phase63Path = Phase63Path,
        phase65Path = Phase65Path,
        phase68Path = Phase68Path,
        phase74Path = Phase74Path,
        phase75Path = Phase75Path,
        phase76Path = Phase76Path,
        phase77Path = Phase77Path,
        phase197Path = Phase197Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "su2_casimir_wz_normalization_probe.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "su2_casimir_wz_normalization_probe_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.numericalTargetComparisonPassed,
        result.sourceLineagePromotable,
        result.probeKind,
        result.derivationHypothesis,
        result.currentTraceHalfScale,
        result.casimirRmsScale,
        result.casimirToTraceHalfScale,
        result.targetImpliedScaleRatio,
        result.casimirScaleRatioMiss,
        result.currentWeakCoupling,
        result.targetImpliedWeakCoupling,
        result.casimirWeakCoupling,
        result.casimirWeakCouplingDelta,
        result.comparisons,
        result.blockers,
        result.decision,
        result.nextRequiredArtifact,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"numericalTargetComparisonPassed={numericalTargetComparisonPassed}");
Console.WriteLine($"sourceLineagePromotable={sourceLineagePromotable}");
Console.WriteLine($"casimirWeakCoupling={casimirWeakCoupling:R}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

static string[] JsonStringArray(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array
        ? property.EnumerateArray()
            .Where(item => item.ValueKind == JsonValueKind.String)
            .Select(item => item.GetString()!)
            .ToArray()
        : Array.Empty<string>();
