using System.Text.Json;

const string DefaultOutputDir = "studies/phase183_massless_sector_invariant_prediction_001/output";
const string Phase174Path = "studies/phase174_protected_massless_subspace_audit_001/output/protected_massless_subspace_audit.json";
const string Phase177Path = "studies/phase177_massless_benchmark_contracts_001/output/massless_benchmark_contracts.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE183_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase174 = JsonDocument.Parse(File.ReadAllText(Phase174Path));
using var phase177 = JsonDocument.Parse(File.ReadAllText(Phase177Path));

double masslessTolerance = JsonDouble(phase174.RootElement, "masslessTolerance") ?? 1e-12;
double gaugeLeakTolerance = JsonDouble(phase174.RootElement, "gaugeLeakTolerance") ?? 1e-12;
bool allConverged = JsonBool(phase174.RootElement, "allConverged") is true;
bool allModesMassless = JsonBool(phase174.RootElement, "allModesMassless") is true;
bool allModesGaugeLeakFree = JsonBool(phase174.RootElement, "allModesGaugeLeakFree") is true;
bool subspaceDimensionStable = JsonBool(phase174.RootElement, "subspaceDimensionStable") is true;
bool protectedSubspaceReady = JsonBool(phase174.RootElement, "subspaceDiagnosticReady") is true;
int protectedSubspaceDimension = JsonInt(phase174.RootElement, "protectedSubspaceDimension") ?? 0;
bool benchmarkContractsPresent = JsonBool(phase177.RootElement, "allBenchmarkContractsPresent") is true;

var contracts = phase177.RootElement.GetProperty("contracts").EnumerateArray()
    .Where(contract => JsonBool(contract, "benchmarkContractPresent") is true)
    .Select(contract => new MasslessContract(
        RequiredString(contract, "particleId"),
        RequiredString(contract, "observableId"),
        JsonDouble(contract, "targetValue") ?? double.NaN,
        JsonDouble(contract, "targetUncertainty") ?? 0.0,
        JsonString(contract, "unit") ?? "masslessness-indicator"))
    .Where(contract => contract.ParticleId is "photon" or "gluon")
    .OrderBy(contract => contract.ParticleId, StringComparer.Ordinal)
    .ToArray();

bool contractsCoverPhotonAndGluon = contracts.Select(contract => contract.ParticleId)
    .ToHashSet(StringComparer.Ordinal)
    .IsSupersetOf(new[] { "photon", "gluon" });
bool targetContractsAreZeroIndicators = contracts.Length > 0
    && contracts.All(contract => contract.TargetValue == 0.0 && contract.Unit == "masslessness-indicator");
bool sectorInvariantValidated = protectedSubspaceReady
    && allConverged
    && allModesMassless
    && allModesGaugeLeakFree
    && subspaceDimensionStable;
bool knownMasslessPredictionAllowed = sectorInvariantValidated
    && benchmarkContractsPresent
    && contractsCoverPhotonAndGluon
    && targetContractsAreZeroIndicators;

var predictions = contracts.Select(contract =>
{
    double predictedValue = 0.0;
    double predictedUncertainty = 0.0;
    double residual = Math.Abs(predictedValue - contract.TargetValue);
    bool passed = knownMasslessPredictionAllowed && residual <= Math.Max(contract.TargetUncertainty, 0.0);
    return new MasslessPrediction(
        contract.ParticleId,
        contract.ObservableId,
        predictedValue,
        predictedUncertainty,
        contract.TargetValue,
        contract.TargetUncertainty,
        contract.Unit,
        0.0,
        passed,
        passed);
}).ToArray();

string terminalStatus = knownMasslessPredictionAllowed
    ? "massless-sector-invariant-predictions-promoted"
    : "massless-sector-invariant-predictions-blocked";

var result = new
{
    phaseId = "phase183-massless-sector-invariant-prediction",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    masslessTolerance,
    gaugeLeakTolerance,
    allConverged,
    allModesMassless,
    allModesGaugeLeakFree,
    subspaceDimensionStable,
    protectedSubspaceReady,
    protectedSubspaceDimension,
    benchmarkContractsPresent,
    contractsCoverPhotonAndGluon,
    targetContractsAreZeroIndicators,
    sectorInvariantValidated,
    knownMasslessPredictionAllowed,
    promotedClaimBoundary = "Promotes only the zero masslessness-indicator observable inherited by every subspace of the protected massless gauge sector; does not promote photon U(1) identity, color-octet identity, confinement phenomenology, or individual registry-mode identity.",
    predictions,
    blockers = knownMasslessPredictionAllowed
        ? Array.Empty<string>()
        : new[]
        {
            protectedSubspaceReady ? "" : "protected massless subspace diagnostic is not ready",
            allConverged ? "" : "not every sibling spectrum sheet converged",
            allModesMassless ? "" : "not every protected-sector mode is within the massless tolerance",
            allModesGaugeLeakFree ? "" : "not every protected-sector mode is gauge-leak-free",
            subspaceDimensionStable ? "" : "protected massless subspace dimension is not stable across sibling backgrounds",
            benchmarkContractsPresent ? "" : "photon/gluon masslessness benchmark contracts are not both present",
            contractsCoverPhotonAndGluon ? "" : "masslessness contracts do not cover both photon and gluon",
            targetContractsAreZeroIndicators ? "" : "masslessness contracts are not zero indicator contracts",
        }.Where(blocker => blocker.Length > 0).ToArray(),
    sourceEvidence = new
    {
        phase174Path = Phase174Path,
        phase177Path = Phase177Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "massless_sector_invariant_prediction.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "massless_sector_invariant_prediction_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.sectorInvariantValidated,
        result.knownMasslessPredictionAllowed,
        result.protectedSubspaceDimension,
        result.predictions,
        result.promotedClaimBoundary,
        result.blockers,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"sectorInvariantValidated={sectorInvariantValidated}");
Console.WriteLine($"knownMasslessPredictionAllowed={knownMasslessPredictionAllowed}");
Console.WriteLine($"predictionCount={predictions.Length}");

static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;

sealed record MasslessContract(
    string ParticleId,
    string ObservableId,
    double TargetValue,
    double TargetUncertainty,
    string Unit);

sealed record MasslessPrediction(
    string ParticleId,
    string ObservableId,
    double PredictedValue,
    double PredictedUncertainty,
    double TargetValue,
    double TargetUncertainty,
    string Unit,
    double PullOrSigmaResidual,
    bool Passed,
    bool PromotionAllowed);
