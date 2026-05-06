using System.Text.Json;

const string DefaultOutputDir = "studies/phase107_candidate3_target_independent_normalization_001/output";
const string Phase106IdentityPath = "studies/phase106_candidate3_observable_identity_derivation_001/output/candidate3_observable_identity_derivation.json";
const string Phase105PrerequisitesPath = "studies/phase105_candidate3_physical_derivation_prerequisites_001/output/candidate3_physical_derivation_prerequisites.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE107_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var identity = JsonDocument.Parse(File.ReadAllText(Phase106IdentityPath));
using var prerequisites = JsonDocument.Parse(File.ReadAllText(Phase105PrerequisitesPath));

bool identityValidated = JsonBool(identity.RootElement, "identityValidated") is true;
var closure = identityValidated
    ? Array.Empty<string>()
    :
    [
        "candidate-3 observable identity is not validated",
        "target-independent normalization cannot be derived for an internal-only coupling proxy",
    ];

var result = new
{
    phaseId = "phase107-candidate3-target-independent-normalization",
    terminalStatus = identityValidated
        ? "candidate3-target-independent-normalization-ready"
        : "candidate3-target-independent-normalization-blocked",
    candidateId = JsonString(identity.RootElement, "candidateId") ?? "candidate-3",
    sourceComputedObservableId = JsonString(identity.RootElement, "sourceComputedObservableId"),
    acceptedPhysicalObservableId = JsonString(identity.RootElement, "acceptedPhysicalObservableId"),
    normalizationValidated = identityValidated,
    targetIndependent = identityValidated,
    scaleFactor = (double?)null,
    scaleUncertainty = (double?)null,
    targetValuesUsed = false,
    closureRequirements = closure,
    failClosedRules = new[]
    {
        "No target residual may be used to define the normalization.",
        "No scale factor is emitted while candidate-3 identity remains internal-only.",
        "A future validated identity must declare unit family before this phase can emit a scale.",
    },
    sourceEvidence = new
    {
        observableIdentityDerivationPath = Phase106IdentityPath,
        derivationPrerequisitesPath = Phase105PrerequisitesPath,
        pipelineCompatibility = prerequisites.RootElement.GetProperty("existingPipelineCompatibility").Clone(),
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_target_independent_normalization.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "candidate3_target_independent_normalization_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase107-candidate3-target-independent-normalization",
        result.terminalStatus,
        result.candidateId,
        result.normalizationValidated,
        result.targetIndependent,
        result.closureRequirements,
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
