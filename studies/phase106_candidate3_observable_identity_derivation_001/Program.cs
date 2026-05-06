using System.Text.Json;

const string DefaultOutputDir = "studies/phase106_candidate3_observable_identity_derivation_001/output";
const string Phase101PackagePath = "studies/phase101_boson_prediction_package_001/output/boson_prediction_package.json";
const string Phase105PrerequisitesPath = "studies/phase105_candidate3_physical_derivation_prerequisites_001/output/candidate3_physical_derivation_prerequisites.json";
const string Phase99EvidencePath = "studies/phase99_selector_eigenvector_full_lift_001/output/selector_eigenvector_full_lift_evidence.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE106_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var package = JsonDocument.Parse(File.ReadAllText(Phase101PackagePath));
using var prerequisites = JsonDocument.Parse(File.ReadAllText(Phase105PrerequisitesPath));
using var phase99 = JsonDocument.Parse(File.ReadAllText(Phase99EvidencePath));

var internalPrediction = package.RootElement.GetProperty("internalPrediction");
string sourceObservableType = JsonString(internalPrediction, "observableType") ?? "unknown";
string sourceObservableId = JsonString(internalPrediction, "observableId") ?? "unknown";
bool isInternalCouplingProxy = string.Equals(sourceObservableType, "internal-coupling-proxy", StringComparison.Ordinal);

var candidateIdentities = new[]
{
    PhysicalIdentityCandidate("w-boson-mass", "rejected", "candidate-3 source is a single internal coupling proxy, not a validated W mass-energy mode"),
    PhysicalIdentityCandidate("z-boson-mass", "rejected", "candidate-3 source is a single internal coupling proxy, not a validated Z mass-energy mode"),
    PhysicalIdentityCandidate("w-z-mass-ratio", "rejected", "candidate-3 source is not a validated W/Z mode pair"),
    PhysicalIdentityCandidate("higgs-mass", "rejected", "no scalar-sector identity derivation links candidate-3 replay proxy to a Higgs mass observable"),
    PhysicalIdentityCandidate("photon-masslessness", "rejected", "candidate-3 replay proxy is not a protected zero-mode or gauge-mode masslessness indicator"),
};

bool anyValidated = candidateIdentities.Any(c => string.Equals((string)c.GetType().GetProperty("status")!.GetValue(c)!, "validated", StringComparison.Ordinal));
var result = new
{
    phaseId = "phase106-candidate3-observable-identity-derivation",
    terminalStatus = anyValidated
        ? "candidate3-observable-identity-validated"
        : "candidate3-observable-identity-rejected-internal-only",
    candidateId = JsonString(package.RootElement, "candidateId") ?? "candidate-3",
    sourceComputedObservableId = sourceObservableId,
    sourceObservableType,
    identityValidated = anyValidated,
    acceptedPhysicalObservableId = (string?)null,
    acceptedPhysicalObservableType = (string?)null,
    identityCandidates = candidateIdentities,
    diagnosis = new[]
    {
        isInternalCouplingProxy
            ? "source observable is explicitly classified as an internal coupling proxy"
            : "source observable is not classified as a validated physical mode",
        "Phase99 validates full connection-space replay materialization, not physical particle identity",
        "Phase105 confirms the candidate-3 source is not plug-compatible with the existing absolute W/Z mass projection contract",
    },
    closureRequirements = new[]
    {
        "provide a new target-independent derivation linking candidate-3 replay proxy to a named physical observable",
        "or retain candidate-3 as an internal-only replay prediction",
    },
    sourceEvidence = new
    {
        predictionPackagePath = Phase101PackagePath,
        derivationPrerequisitesPath = Phase105PrerequisitesPath,
        selectorFullLiftEvidencePath = Phase99EvidencePath,
        selectorEigenmode = phase99.RootElement.GetProperty("selectorEigenmode").Clone(),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_observable_identity_derivation.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_observable_identity_derivation_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase106-candidate3-observable-identity-derivation",
        result.terminalStatus,
        result.candidateId,
        result.sourceComputedObservableId,
        result.sourceObservableType,
        result.identityValidated,
        result.closureRequirements,
    }, options));

Console.WriteLine(result.terminalStatus);

static object PhysicalIdentityCandidate(string physicalObservableId, string status, string reason) => new
{
    physicalObservableId,
    status,
    reason,
};

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;
