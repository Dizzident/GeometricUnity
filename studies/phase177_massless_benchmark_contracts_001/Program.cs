using System.Text.Json;

const string DefaultOutputDir = "studies/phase177_massless_benchmark_contracts_001/output";
const string Phase174Path = "studies/phase174_protected_massless_subspace_audit_001/output/protected_massless_subspace_audit.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE177_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase174 = JsonDocument.Parse(File.ReadAllText(Phase174Path));

bool protectedSubspaceReady = JsonBool(phase174.RootElement, "subspaceDiagnosticReady") is true;
int? protectedSubspaceDimension = JsonInt(phase174.RootElement, "protectedSubspaceDimension");

var contracts = new[]
{
    new MasslessBenchmarkContract(
        ParticleId: "photon",
        ObservableId: "physical-photon-masslessness",
        BenchmarkContractPresent: true,
        BenchmarkClass: "zero-masslessness-indicator",
        TargetValue: 0.0,
        TargetUncertainty: 0.0,
        Unit: "masslessness-indicator",
        RequiredIdentityArtifact: "target-independent U(1) photon identity rule",
        IdentityCandidatePresent: false,
        PredictionAllowed: false,
        Notes: "Contract only; not a photon mass upper-bound comparison and not an identity derivation."),
    new MasslessBenchmarkContract(
        ParticleId: "gluon",
        ObservableId: "physical-gluon-masslessness",
        BenchmarkContractPresent: true,
        BenchmarkClass: "zero-masslessness-indicator",
        TargetValue: 0.0,
        TargetUncertainty: 0.0,
        Unit: "masslessness-indicator",
        RequiredIdentityArtifact: "target-independent color-octet gluon identity rule",
        IdentityCandidatePresent: false,
        PredictionAllowed: false,
        Notes: "Contract only; not a confinement or pole-mass phenomenology claim and not a color-sector identity derivation."),
};

bool allBenchmarkContractsPresent = contracts.All(contract => contract.BenchmarkContractPresent);
bool anyKnownMasslessPredictionAllowed = contracts.Any(contract => contract.PredictionAllowed);
string terminalStatus = anyKnownMasslessPredictionAllowed
    ? "massless-benchmark-contracts-ready-for-identified-sector"
    : "massless-benchmark-contracts-ready-identity-blocked";

var result = new
{
    phaseId = "phase177-massless-benchmark-contracts",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    protectedSubspaceReady,
    protectedSubspaceDimension,
    allBenchmarkContractsPresent,
    anyKnownMasslessPredictionAllowed,
    contracts,
    blockers = anyKnownMasslessPredictionAllowed
        ? Array.Empty<string>()
        : new[]
        {
            "benchmark contracts are present, but no target-independent U(1) photon identity is present",
            "benchmark contracts are present, but no target-independent color-octet gluon identity is present",
            "current contracts therefore cannot promote the protected massless subspace to known photon/gluon predictions",
        },
    nextWork = anyKnownMasslessPredictionAllowed
        ? "wire identified massless-sector prediction rows into the all-boson comparison table"
        : "derive target-independent U(1) and color-octet identity artifacts; benchmark contracts are no longer the active blocker",
    sourceEvidence = new
    {
        phase174Path = Phase174Path,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "massless_benchmark_contracts.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "massless_benchmark_contracts_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.protectedSubspaceReady,
        result.protectedSubspaceDimension,
        result.allBenchmarkContractsPresent,
        result.anyKnownMasslessPredictionAllowed,
        result.contracts,
        result.blockers,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"allBenchmarkContractsPresent={allBenchmarkContractsPresent}");
Console.WriteLine($"anyKnownMasslessPredictionAllowed={anyKnownMasslessPredictionAllowed}");

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record MasslessBenchmarkContract(
    string ParticleId,
    string ObservableId,
    bool BenchmarkContractPresent,
    string BenchmarkClass,
    double TargetValue,
    double TargetUncertainty,
    string Unit,
    string RequiredIdentityArtifact,
    bool IdentityCandidatePresent,
    bool PredictionAllowed,
    string Notes);
