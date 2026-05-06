using System.Text.Json;

const string DefaultOutputDir = "studies/phase105_candidate3_physical_derivation_prerequisites_001/output";
const string Phase101PackagePath = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package.json";
const string Phase104MappingAttemptPath = "studies/phase104_candidate3_physical_mapping_attempt_001/output/candidate3_physical_mapping_attempt.json";
const string Phase75MissDiagnosticPath = "studies/phase75_wz_absolute_mass_miss_diagnostic_001/wz_absolute_mass_miss_diagnostic.json";
const string Phase76NormalizationAuditPath = "studies/phase76_weak_coupling_amplitude_normalization_audit_001/weak_coupling_amplitude_normalization_audit.json";
const string Phase72CalibrationPath = "studies/phase72_wz_absolute_scale_calibration_001/wz_absolute_scale_calibration.json";
const string Phase74ComparisonPath = "studies/phase74_wz_absolute_mass_target_comparison_001/wz_absolute_mass_target_comparison.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE105_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var package = JsonDocument.Parse(File.ReadAllText(Phase101PackagePath));
using var mappingAttempt = JsonDocument.Parse(File.ReadAllText(Phase104MappingAttemptPath));
using var miss = JsonDocument.Parse(File.ReadAllText(Phase75MissDiagnosticPath));
using var normalization = JsonDocument.Parse(File.ReadAllText(Phase76NormalizationAuditPath));
using var calibration = JsonDocument.Parse(File.ReadAllText(Phase72CalibrationPath));
using var comparison = JsonDocument.Parse(File.ReadAllText(Phase74ComparisonPath));

var internalPrediction = package.RootElement.GetProperty("internalPrediction");
double? candidateProxy = JsonDouble(internalPrediction, "value");
double? currentWeakCoupling = JsonDouble(miss.RootElement, "currentWeakCoupling");
double? requiredWeakCoupling = JsonDouble(miss.RootElement, "requiredWeakCoupling");
double? targetImpliedRaw = JsonDouble(normalization.RootElement, "targetImpliedRawMatrixElementMagnitude");
double? rawRequiredScale = JsonDouble(normalization.RootElement, "rawMatrixElementRequiredScale");

var diagnostics = new
{
    candidateProxyMagnitude = candidateProxy,
    currentWeakCoupling,
    requiredWeakCoupling,
    targetImpliedRawMatrixElementMagnitude = targetImpliedRaw,
    rawMatrixElementRequiredScale = rawRequiredScale,
    candidateProxyToCurrentWeakCouplingRatio = Ratio(candidateProxy, currentWeakCoupling),
    candidateProxyToRequiredWeakCouplingRatio = Ratio(candidateProxy, requiredWeakCoupling),
    multiplierNeededIfCandidateProxyWereUsedAsWeakCoupling = Ratio(requiredWeakCoupling, candidateProxy),
    multiplierNeededIfCandidateProxyWereUsedAsRawMatrixElement = Ratio(targetImpliedRaw, candidateProxy),
};

var prerequisite = new
{
    phaseId = "phase105-candidate3-physical-derivation-prerequisites",
    terminalStatus = "candidate3-physical-derivation-prerequisites-complete",
    candidateId = JsonString(package.RootElement, "candidateId") ?? "candidate-3",
    sourceComputedObservableId = JsonString(internalPrediction, "observableId"),
    sourceObservableType = JsonString(internalPrediction, "observableType"),
    existingPipelineCompatibility = new
    {
        status = "not-plug-compatible",
        reason = "existing absolute W/Z mass projection consumes validated mass-energy W/Z modes and a GeV-per-internal-mass calibration; candidate-3 currently supplies only an internal coupling-proxy magnitude",
        phase72CalibrationStatus = JsonString(calibration.RootElement, "status"),
        phase74ComparisonStatus = JsonString(comparison.RootElement, "terminalStatus"),
        phase104MappingStatus = JsonString(mappingAttempt.RootElement, "terminalStatus"),
    },
    diagnostics,
    requiredDerivationArtifacts = new[]
    {
        new
        {
            artifactId = "candidate3-observable-identity-derivation",
            requiredStatus = "validated",
            description = "derive whether the candidate-3 replay proxy maps to a named physical boson observable, or reject it as internal-only",
        },
        new
        {
            artifactId = "candidate3-target-independent-normalization",
            requiredStatus = "validated",
            description = "derive a normalization from internal units to the mapped observable without using the target comparison to tune the value",
        },
        new
        {
            artifactId = "candidate3-uncertainty-propagation",
            requiredStatus = "validated",
            description = "propagate branch, refinement, replay, and calibration uncertainty into the mapped observable",
        },
        new
        {
            artifactId = "candidate3-calibrated-target-comparison",
            requiredStatus = "passed",
            description = "run target comparison only after mapping and calibration validate",
        },
    },
    failClosedRules = new[]
    {
        "Do not compare the internal candidate-3 coupling proxy directly to physical boson masses.",
        "Do not tune a scale factor from the W/Z target residual and call it a calibration.",
        "Do not reuse candidate-0/2 W/Z mappings for candidate-3 without a candidate-3 source derivation.",
        "If the candidate-3 observable identity cannot be derived, keep Phase101 as the final internal prediction artifact.",
    },
    sourceEvidence = new
    {
        predictionPackagePath = Phase101PackagePath,
        mappingAttemptPath = Phase104MappingAttemptPath,
        absoluteMassMissDiagnosticPath = Phase75MissDiagnosticPath,
        weakCouplingNormalizationAuditPath = Phase76NormalizationAuditPath,
        absoluteScaleCalibrationPath = Phase72CalibrationPath,
        absoluteMassComparisonPath = Phase74ComparisonPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_physical_derivation_prerequisites.json"),
    JsonSerializer.Serialize(prerequisite, options));
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_physical_derivation_prerequisites_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase105-candidate3-physical-derivation-prerequisites",
        prerequisite.terminalStatus,
        prerequisite.candidateId,
        prerequisite.existingPipelineCompatibility.status,
        diagnostics.candidateProxyMagnitude,
        diagnostics.multiplierNeededIfCandidateProxyWereUsedAsWeakCoupling,
        diagnostics.multiplierNeededIfCandidateProxyWereUsedAsRawMatrixElement,
    }, options));

Console.WriteLine(prerequisite.terminalStatus);
Console.WriteLine($"candidateProxyMagnitude={candidateProxy:R}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static double? Ratio(double? numerator, double? denominator) =>
    numerator is { } n && denominator is { } d && double.IsFinite(n) && double.IsFinite(d) && d != 0.0
        ? n / d
        : null;
