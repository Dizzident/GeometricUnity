using System.Security.Cryptography;
using System.Text.Json;

const string PhaseDir = "studies/phase535_bounded_registered_operator_pilot_adjudicator_001";
const string ContractPath = PhaseDir + "/preregistration/phase535_bounded_pilot_adjudicator_contract_v1.json";
const string OutputPath = PhaseDir + "/output/bounded_registered_operator_pilot_adjudicator.json";
const string SummaryPath = PhaseDir + "/output/bounded_registered_operator_pilot_adjudicator_summary.json";

byte[] contractBytes = File.ReadAllBytes(ContractPath);
using JsonDocument contractDocument = JsonDocument.Parse(contractBytes);
JsonElement contract = contractDocument.RootElement;
var bindings = contract.GetProperty("expectedInputs").EnumerateArray().Select(x => Bind(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
using JsonDocument phase533Document = JsonDocument.Parse(File.ReadAllText(
    bindings.Single(x => x.Id == "phase533-summary").Path));
using JsonDocument phase534Document = JsonDocument.Parse(File.ReadAllText(
    bindings.Single(x => x.Id == "phase534-summary").Path));
JsonElement phase533 = phase533Document.RootElement;
JsonElement phase534 = phase534Document.RootElement;
JsonElement pilot = contract.GetProperty("pilotIdentity");
JsonElement boundary = contract.GetProperty("claimBoundary");
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase535-a22-bounded-registered-operator-pilot-adjudicator-v1"
    && contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean()
    && bindings.Length == 4 && bindings.Select(x => x.Id).Distinct().Count() == 4 && bindings.All(x => x.HashMatches)
    && pilot.GetProperty("member").GetString() == "sd2-id0/c0.5"
    && pilot.GetProperty("beta").GetDouble() == 1.0 && pilot.GetProperty("extent").GetInt32() == 3
    && pilot.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && pilot.GetProperty("twoSeedTablesRequired").GetBoolean()
    && pilot.GetProperty("configurationRetentionRequired").GetBoolean()
    && boundary.GetProperty("pilotMayRunOnlyAfterControlsClose").GetBoolean()
    && !boundary.GetProperty("phase481PackCreated").GetBoolean()
    && !boundary.GetProperty("productionValidated").GetBoolean()
    && !boundary.GetProperty("dynamicalThetaValidated").GetBoolean()
    && !boundary.GetProperty("physicalClaimAllowed").GetBoolean()
    && boundary.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && taxonomy.Length == 8 && taxonomy.Distinct().Count() == 8;
string phase533Verdict = phase533.GetProperty("verdictKind").GetString()!;
string phase534Verdict = phase534.GetProperty("verdictKind").GetString()!;
bool phase533Closed = phase533Verdict == "nested-validation-contract-closed";
bool analysisControlPassed = phase534.GetProperty("analysisControl").GetProperty("passed").GetBoolean();
bool freeSamplerPassed = phase534.GetProperty("freeSamplerControl").GetProperty("passed").GetBoolean();
bool reducedInteractingPassed = phase534.GetProperty("reducedInteractingControl").GetProperty("passed").GetBoolean();
bool allControlsClosed = phase534Verdict == "nested-controls-closed"
    && analysisControlPassed && freeSamplerPassed && reducedInteractingPassed;

string Evaluate(bool valid, bool p533, bool analysis, bool free, bool reduced, string? pilotTerminal) =>
    !valid ? "invalid-or-drifted-input"
    : !p533 ? "phase533-contract-not-closed"
    : !analysis ? "analysis-control-failed"
    : !free ? "free-sampler-control-failed"
    : !reduced ? "reduced-interacting-control-failed"
    : pilotTerminal ?? "pilot-execution-topology-inadequate";
var precedenceBattery = new[]
{
    Case("invalid", false, true, true, true, true, null, "invalid-or-drifted-input"),
    Case("phase533", true, false, true, true, true, null, "phase533-contract-not-closed"),
    Case("analysis", true, true, false, true, true, null, "analysis-control-failed"),
    Case("free", true, true, true, false, true, null, "free-sampler-control-failed"),
    Case("reduced", true, true, true, true, false, null, "reduced-interacting-control-failed"),
    Case("topology", true, true, true, true, true, "pilot-execution-topology-inadequate", "pilot-execution-topology-inadequate"),
    Case("unresolved", true, true, true, true, true, "pilot-execution-diagnostics-valid-unresolved", "pilot-execution-diagnostics-valid-unresolved"),
    Case("resolved", true, true, true, true, true, "pilot-execution-diagnostics-valid-resolved", "pilot-execution-diagnostics-valid-resolved"),
};
bool precedenceBatteryPassed = precedenceBattery.All(x => x.Passed);

// The current exact-bound Phase534 artifact failed its reduced interacting
// control. The pilot branch is therefore forbidden and deliberately absent:
// adding or executing it after seeing that failure would bypass preregistration.
bool pilotRun = false;
bool configurationsRetained = false;
string verdict = Evaluate(contractValid && precedenceBatteryPassed, phase533Closed,
    analysisControlPassed, freeSamplerPassed, reducedInteractingPassed, null);
bool pilotRefusedByUpstreamControl = !allControlsClosed;
var result = new
{
    schemaVersion = 1, phase = 535, phaseId = "phase535-bounded-registered-operator-pilot-adjudicator",
    contractSha256 = Sha(contractBytes), contractValid, bindings, precedenceBatteryPassed, precedenceBattery,
    phase533Verdict, phase534Verdict, phase533Closed, analysisControlPassed, freeSamplerPassed,
    reducedInteractingPassed, allControlsClosed, pilotRefusedByUpstreamControl,
    pilotRun, configurationsRetained, pilotIdentity = new
    {
        member = "sd2-id0/c0.5", beta = 1.0, extent = 3, thetaRule = "theta-identically-zero"
    },
    verdictKind = verdict, terminalStatus = "bounded-registered-operator-pilot-adjudicator-" + verdict,
    phase535Passed = contractValid && precedenceBatteryPassed && pilotRefusedByUpstreamControl
        && verdict == "reduced-interacting-control-failed",
    phase481PackCreated = false, productionValidated = false, dynamicalThetaValidated = false,
    temporalAcquisitionValidated = false, phase458G3Satisfied = false, phase458G4Satisfied = false,
    phase458G5Satisfied = false, o4Discharged = false, physicalClaimAllowed = false,
    promotedPhysicalMassClaimCount = 0,
    decision = verdict == "reduced-interacting-control-failed"
        ? "The Gaussian and free controls passed, but the reduced interacting HMC control recorded non-finite or divergent trajectories. The bounded complete-lattice pilot is refused. The result localizes the next experiment to prospectively retuned interacting integrator controls; it does not diagnose the deeper cause."
        : "The adjudicator preserves the earliest fail-closed terminal. No physical or production conclusion follows."
};
Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json); File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase535 verdict: {verdict}");

PrecedenceCase Case(string id, bool valid, bool p533, bool analysis, bool free, bool reduced, string? pilotTerminal, string expected)
{
    string actual = Evaluate(valid, p533, analysis, free, reduced, pilotTerminal);
    return new(id, expected, actual, actual == expected);
}
static Binding Bind(string id, string path, string expected)
{
    string actual = Sha(File.ReadAllBytes(path)); return new(id, path, expected, actual, actual == expected);
}
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
public sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
public sealed record PrecedenceCase(string Id, string Expected, string Actual, bool Passed);
