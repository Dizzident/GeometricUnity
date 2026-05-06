using System.Text.Json;

const string DefaultOutputDir = "studies/phase108_candidate3_physical_comparison_closure_001/output";
const string Phase100ReadinessPath = "studies/phase100_boson_prediction_readiness_001/output/boson_prediction_readiness.json";
const string Phase101PackagePath = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package.json";
const string Phase106IdentityPath = "studies/phase106_candidate3_observable_identity_derivation_001/output/candidate3_observable_identity_derivation.json";
const string Phase107NormalizationPath = "studies/phase107_candidate3_target_independent_normalization_001/output/candidate3_target_independent_normalization.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE108_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var readiness = JsonDocument.Parse(File.ReadAllText(Phase100ReadinessPath));
using var package = JsonDocument.Parse(File.ReadAllText(Phase101PackagePath));
using var identity = JsonDocument.Parse(File.ReadAllText(Phase106IdentityPath));
using var normalization = JsonDocument.Parse(File.ReadAllText(Phase107NormalizationPath));

bool identityValidated = JsonBool(identity.RootElement, "identityValidated") is true;
bool normalizationValidated = JsonBool(normalization.RootElement, "normalizationValidated") is true;
bool readinessExternal = JsonBool(readiness.RootElement, "externalPhysicalComparisonReady") is true;
bool comparisonRunnable = identityValidated && normalizationValidated && readinessExternal;

var blockers = new List<string>();
if (!identityValidated)
    blockers.Add("candidate-3 observable identity is rejected or not validated");
if (!normalizationValidated)
    blockers.Add("candidate-3 target-independent normalization is blocked");
if (!readinessExternal)
    blockers.Add("Phase100 external physical comparison gate is not ready");

var result = new
{
    phaseId = "phase108-candidate3-physical-comparison-closure",
    terminalStatus = comparisonRunnable
        ? "candidate3-physical-comparison-ready"
        : "candidate3-physical-comparison-closed-internal-only",
    candidateId = JsonString(package.RootElement, "candidateId") ?? "candidate-3",
    comparisonRunnable,
    finalAllowedPredictionLevel = comparisonRunnable
        ? "external-physical-boson-prediction"
        : "internal-boson-replay-prediction",
    finalPackagePath = Phase101PackagePath,
    blockers,
    closureDecision = comparisonRunnable
        ? "run calibrated target comparison"
        : "do not run physical target comparison for the current candidate-3 evidence set",
    restartConditions = new[]
    {
        "new candidate-3 target-independent observable identity derivation validates a named physical observable",
        "target-independent normalization validates for that observable",
        "Phase100 external physical comparison gates pass after consuming the new evidence",
    },
    sourceEvidence = new
    {
        readinessPath = Phase100ReadinessPath,
        predictionPackagePath = Phase101PackagePath,
        observableIdentityDerivationPath = Phase106IdentityPath,
        targetIndependentNormalizationPath = Phase107NormalizationPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_physical_comparison_closure.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_physical_comparison_closure_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase108-candidate3-physical-comparison-closure",
        result.terminalStatus,
        result.candidateId,
        result.comparisonRunnable,
        result.finalAllowedPredictionLevel,
        result.blockers,
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
