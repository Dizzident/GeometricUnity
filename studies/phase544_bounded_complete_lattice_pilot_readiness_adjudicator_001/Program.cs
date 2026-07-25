using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase544_bounded_complete_lattice_pilot_readiness_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase544_bounded_pilot_readiness_contract_v1.json";
const string OutputPath = Root + "/output/bounded_complete_lattice_pilot_readiness_adjudicator.json";
const string SummaryPath = Root + "/output/bounded_complete_lattice_pilot_readiness_adjudicator_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new
{
    Id = x.GetProperty("id").GetString()!, Path = x.GetProperty("path").GetString()!,
    ExpectedSha256 = x.GetProperty("sha256").GetString()!,
}).ToArray();
var bindings = bindingSpecs.Select(x => new
{
    x.Id, x.Path, x.ExpectedSha256,
    ActualSha256 = File.Exists(x.Path) ? Sha(x.Path) : "missing",
    HashMatches = File.Exists(x.Path) && Sha(x.Path) == x.ExpectedSha256,
}).ToArray();
string[] expectedIds =
[
    "phase535-program", "phase535-summary", "phase540-contract", "phase540-summary",
    "phase542-summary", "phase543-contract", "phase543-program", "phase543-summary",
];
bool exactBindingsValid = bindingSpecs.Select(x => x.Id).SequenceEqual(expectedIds) && bindings.All(x => x.HashMatches);
using var p535Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[1].Path));
using var p540Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[3].Path));
using var p542Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[4].Path));
using var p543Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[7].Path));
JsonElement p535 = p535Doc.RootElement;
JsonElement p540 = p540Doc.RootElement;
JsonElement p542 = p542Doc.RootElement;
JsonElement p543 = p543Doc.RootElement;
string[] gateIds =
[
    "resolved-force-closure-branch",
    "deterministic-complete-lattice-force-gradient-oracle",
    "deterministic-complete-lattice-multistate-integrator-controls",
    "new-executable-bounded-pilot-implementation",
    "hardened-complete-lattice-diagnostics",
    "disjoint-prospective-seeds-and-checkpoint-rules",
    "executable-resource-topology-refusal",
];
string[] taxonomy =
[
    "invalid-or-drifted-input", "force-closure-branch-unresolved",
    "deterministic-force-oracle-missing", "deterministic-multistate-integrator-controls-failed",
    "executable-bounded-pilot-implementation-missing", "hardened-complete-lattice-diagnostics-missing",
    "prospective-seed-or-checkpoint-rules-missing", "resource-topology-refusal-evidence-missing",
    "later-bounded-pilot-pack-construction-ready",
];
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase544-a27-bounded-complete-lattice-pilot-readiness-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A27"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && exactBindingsValid
    && contract.GetProperty("readinessGatesInOrder").EnumerateArray().Select(x => x.GetString()).SequenceEqual(gateIds)
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()).SequenceEqual(taxonomy)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

bool resolvedClosureBranch =
    p542.GetProperty("verdictKind").GetString() is "compact-force-closed-subspace-found" or "force-closure-expands-beyond-compact-limit"
    && p543.GetProperty("branchSelectionValid").GetBoolean();
bool deterministicForceOracle =
    p542.GetProperty("metricNormalization").GetProperty("passed").GetBoolean()
    && p543.GetProperty("gradientControl").GetProperty("passed").GetBoolean();
bool deterministicMultistateControls =
    p543.GetProperty("verdictKind").GetString() == "branch-selected-deterministic-controls-passed"
    && p543.GetProperty("deterministicControlsPassed").GetBoolean();
bool historicalPilotStayedClosed = !p535.GetProperty("pilotRun").GetBoolean()
    && p535.GetProperty("pilotRefusedByUpstreamControl").GetBoolean();
bool newExecutablePilotImplementation = false;
bool hardenedDiagnostics = false;
bool disjointSeedsAndCheckpointRules = false;
bool executableResourceTopologyRefusal = false;
var gates = new[]
{
    new Gate(gateIds[0], resolvedClosureBranch, "Phase542 fixes the expanding-closure branch and Phase543 consumes the complete-lattice branch."),
    new Gate(gateIds[1], deterministicForceOracle, "Phase542 metric normalization and Phase543 independent directional-gradient control."),
    new Gate(gateIds[2], deterministicMultistateControls, "Phase543 multi-state/two-momentum step-halving grid."),
    new Gate(gateIds[3], newExecutablePilotImplementation, "No exact-bound A27 artifact implements a new HMC accept/reject branch distinct from closed Phase535."),
    new Gate(gateIds[4], hardenedDiagnostics, "No exact-bound A27 artifact freezes complete-lattice acceptance, divergence, convergence, and observable telemetry."),
    new Gate(gateIds[5], disjointSeedsAndCheckpointRules, "No exact-bound A27 artifact freezes new disjoint seeds and a configuration-level checkpoint codec."),
    new Gate(gateIds[6], executableResourceTopologyRefusal, "No executable pilot exists against which to validate CPU, memory, topology, and refusal arithmetic."),
};
string verdict = !contractValid || !historicalPilotStayedClosed ? taxonomy[0]
    : !resolvedClosureBranch ? taxonomy[1]
    : !deterministicForceOracle ? taxonomy[2]
    : !deterministicMultistateControls ? taxonomy[3]
    : !newExecutablePilotImplementation ? taxonomy[4]
    : !hardenedDiagnostics ? taxonomy[5]
    : !disjointSeedsAndCheckpointRules ? taxonomy[6]
    : !executableResourceTopologyRefusal ? taxonomy[7]
    : taxonomy[8];
string? earliestMissingGate = gates.FirstOrDefault(x => !x.Passed)?.Id;
bool laterPackConstructionReady = verdict == taxonomy[8];

var output = new
{
    schemaVersion = 1,
    phase = 544,
    phaseId = "phase544-bounded-complete-lattice-pilot-readiness-adjudicator",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    deterministicZeroSampling = true,
    rngUsed = false,
    hmcOrSamplingPerformed = false,
    configurationsRetained = false,
    historicalPhase535StayedClosed = historicalPilotStayedClosed,
    gates,
    passedGateCount = gates.Count(x => x.Passed),
    failedGateCount = gates.Count(x => !x.Passed),
    earliestMissingGate,
    allReadinessGatesClosed = gates.All(x => x.Passed),
    laterBoundedPilotPackConstructionReady = laterPackConstructionReady,
    verdictKind = verdict,
    terminalStatus = "bounded-complete-lattice-pilot-readiness-" + verdict,
    decision = verdict == "executable-bounded-pilot-implementation-missing"
        ? "The deterministic complete-lattice geometry, force, and integrator controls now pass, but no new prospectively frozen executable HMC pilot exists. The smallest successor is a non-executing pack-construction phase that binds implementation, telemetry, disjoint seeds, checkpoint codec, and resource refusal rules."
        : "The earliest readiness failure is preserved. No sampling or launch authority follows.",
    smallestAdmissibleSuccessor = "separately-registered-nonexecuting-bounded-pilot-pack-construction",
    phase535ExecutedReopenedOrMutated = false,
    phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    o4Discharged = false,
    sourceContractApplicationAllowed = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    productionAuthorized = false,
    launchAuthorized = false,
    externalReviewPending = true,
    allDownstreamAuthority = false,
    promotedPhysicalMassClaimCount = 0,
    bindings,
};
Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(output, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase544 verdict: {verdict}");
Console.WriteLine($"passedGates={gates.Count(x => x.Passed)}, failedGates={gates.Count(x => !x.Passed)}, earliestMissing={earliestMissingGate}");
Console.WriteLine("rng=False, sampling=False");

static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

sealed class Gate(string id, bool passed, string evidence)
{
    public string Id { get; } = id;
    public bool Passed { get; } = passed;
    public string Evidence { get; } = evidence;
}
