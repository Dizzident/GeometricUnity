using System.Security.Cryptography;
using System.Text.Json;

const string PhaseDir = "studies/phase533_nested_validation_contract_001";
const string ContractPath = PhaseDir + "/preregistration/phase533_nested_validation_contract_v1.json";
const string OutputPath = PhaseDir + "/output/nested_validation_contract.json";
const string SummaryPath = PhaseDir + "/output/nested_validation_contract_summary.json";

byte[] contractBytes = File.ReadAllBytes(ContractPath);
using JsonDocument document = JsonDocument.Parse(contractBytes);
JsonElement contract = document.RootElement;
var bindings = contract.GetProperty("expectedInputs").EnumerateArray().Select(x =>
{
    string path = x.GetProperty("path").GetString()!;
    string expected = x.GetProperty("sha256").GetString()!;
    string actual = Sha(File.ReadAllBytes(path));
    return new { id = x.GetProperty("id").GetString(), path, expectedSha256 = expected, actualSha256 = actual, hashMatches = actual == expected };
}).ToArray();
JsonElement resources = contract.GetProperty("resourceRefusal");
JsonElement controls = contract.GetProperty("controlConfiguration");
JsonElement pilot = contract.GetProperty("pilotConfiguration");
JsonElement boundary = contract.GetProperty("claimBoundary");
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase533-a22-nested-validation-contract-v1"
    && contract.GetProperty("frozenBeforeExperiment").GetBoolean()
    && bindings.Length == 5 && bindings.Select(x => x.id).Distinct().Count() == 5
    && contract.GetProperty("seedTables").GetArrayLength() == 2
    && controls.GetProperty("rhatMaximum").GetDouble() == 1.01
    && controls.GetProperty("bulkEssMinimum").GetInt32() == 100
    && pilot.GetProperty("member").GetString() == "sd2-id0/c0.5"
    && pilot.GetProperty("beta").GetDouble() == 1.0
    && pilot.GetProperty("extent").GetInt32() == 3
    && pilot.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && taxonomy.Length == 8 && taxonomy.Distinct().Count() == 8
    && !boundary.GetProperty("phase481PackCreated").GetBoolean()
    && !boundary.GetProperty("productionValidated").GetBoolean()
    && !boundary.GetProperty("dynamicalThetaValidated").GetBoolean()
    && !boundary.GetProperty("temporalAcquisitionValidated").GetBoolean()
    && !boundary.GetProperty("physicalClaimAllowed").GetBoolean()
    && boundary.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
bool exactBindingsValid = bindings.All(x => x.hashMatches);
long maxCpu = resources.GetProperty("maximumAggregateCpuSeconds").GetInt64();
long estimatedCpu = resources.GetProperty("estimatedAggregateCpuSeconds").GetInt64();
long maxBytes = resources.GetProperty("maximumPeakBytes").GetInt64();
long estimatedBytes = resources.GetProperty("estimatedPeakBytes").GetInt64();
bool resourceEnvelopeAccepted = estimatedCpu <= maxCpu && estimatedBytes <= maxBytes;
string verdict = !contractValid || !exactBindingsValid ? "invalid-or-drifted-input"
    : !resourceEnvelopeAccepted ? "resource-refusal"
    : "nested-validation-contract-closed";
bool phase533Passed = verdict == "nested-validation-contract-closed";
var result = new
{
    schemaVersion = 1, phase = 533, phaseId = "phase533-nested-validation-contract",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(contractBytes),
    contractValid, exactBindingsValid, bindings, resourceEnvelopeAccepted,
    resourceEnvelope = new { maximumAggregateCpuSeconds = maxCpu, estimatedAggregateCpuSeconds = estimatedCpu, maximumPeakBytes = maxBytes, estimatedPeakBytes = estimatedBytes },
    seedTableCount = 2, frozenTerminalCount = taxonomy.Length, verdictKind = verdict,
    terminalStatus = "nested-validation-contract-" + verdict, phase533Passed,
    deterministicZeroSampling = true, hmcRun = false, productionValidated = false,
    phase481PackCreated = false, phase458G3Satisfied = false, phase458G4Satisfied = false,
    phase458G5Satisfied = false, o4Discharged = false, physicalClaimAllowed = false,
    promotedPhysicalMassClaimCount = 0,
    decision = phase533Passed
        ? "The prospective nested experiment, independent controls, seed tables, thresholds, narrow terminals, and resource refusal are frozen. No sampling occurred."
        : "The experiment refuses before sampling because the contract or an exact binding drifted, or the resource estimate exceeds its ceiling."
};
Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json); File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase533 verdict: {verdict}");
if (!phase533Passed) Environment.ExitCode = 1;
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
