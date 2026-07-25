using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase540_reduced_to_complete_lattice_transfer_readiness_001";
const string ContractPath = Root + "/preregistration/phase540_reduced_to_complete_lattice_transfer_readiness_contract_v1.json";
const string OutputPath = Root + "/output/reduced_to_complete_lattice_transfer_readiness.json";
const string SummaryPath = Root + "/output/reduced_to_complete_lattice_transfer_readiness_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var expectedBindings = new (string Id, string Path)[]
{
    ("phase533-contract", "studies/phase533_nested_validation_contract_001/preregistration/phase533_nested_validation_contract_v1.json"),
    ("phase533-summary", "studies/phase533_nested_validation_contract_001/output/nested_validation_contract_summary.json"),
    ("phase535-contract", "studies/phase535_bounded_registered_operator_pilot_adjudicator_001/preregistration/phase535_bounded_pilot_adjudicator_contract_v1.json"),
    ("phase535-program", "studies/phase535_bounded_registered_operator_pilot_adjudicator_001/Program.cs"),
    ("phase535-summary", "studies/phase535_bounded_registered_operator_pilot_adjudicator_001/output/bounded_registered_operator_pilot_adjudicator_summary.json"),
    ("phase537-summary", "studies/phase537_deterministic_leapfrog_correctness_stability_audit_001/output/deterministic_leapfrog_correctness_stability_audit_summary.json"),
    ("phase538-summary", "studies/phase538_fixed_grid_interacting_hmc_retuning_001/output/fixed_grid_interacting_hmc_retuning_summary.json"),
    ("phase539-contract", "studies/phase539_independent_reduced_target_row_confirmation_001/preregistration/phase539_independent_reduced_target_row_confirmation_contract_v1.json"),
    ("phase539-program", "studies/phase539_independent_reduced_target_row_confirmation_001/Program.cs"),
    ("phase539-summary", "studies/phase539_independent_reduced_target_row_confirmation_001/output/independent_reduced_target_row_confirmation_summary.json"),
};
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new BindingSpec(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new Binding(x.Id, x.Path, x.ExpectedSha256, actual, actual == x.ExpectedSha256);
}).ToArray();
bool bindingInventoryValid = specs.Select(x => (x.Id, x.Path)).SequenceEqual(expectedBindings);
bool exactBindingsValid = bindingInventoryValid && bindings.All(x => x.HashMatches);

using var p533ContractDocument = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[0].Path));
using var p533Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[1].Path));
using var p535Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[4].Path));
using var p537Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[5].Path));
using var p538Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[6].Path));
using var p539Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[9].Path));
JsonElement p533Contract = p533ContractDocument.RootElement;
JsonElement p533 = p533Document.RootElement;
JsonElement p535 = p535Document.RootElement;
JsonElement p537 = p537Document.RootElement;
JsonElement p538 = p538Document.RootElement;
JsonElement p539 = p539Document.RootElement;

string[] expectedGateOrder =
[
    "independent-reduced-row-confirmed", "explicit-reduced-to-complete-lattice-transfer-map",
    "deterministic-complete-lattice-integrator-oracle", "executable-bounded-pilot-branch",
    "hardened-complete-lattice-diagnostics-and-independent-seeds", "bounded-resource-and-execution-topology",
];
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input", "independent-reduced-row-not-confirmed",
    "reduced-to-complete-lattice-transfer-map-missing", "deterministic-complete-lattice-integrator-oracle-missing",
    "executable-bounded-pilot-branch-missing", "hardened-complete-lattice-diagnostics-missing",
    "bounded-resource-or-execution-topology-missing", "ready-for-prospective-bounded-control-construction",
];
string[] expectedFirewallKeys =
[
    "phase535PilotExecutedOrReopened", "configurationsRetained", "hmcOrSamplingPerformed",
    "phase481PackCreatedOrMutated", "productionDefaultSelected", "phase458G3Satisfied",
    "phase458G4Satisfied", "phase458G5Satisfied", "o4Discharged", "sourceContractApplicationAllowed",
    "physicalUnitOrGevClaimAllowed", "productionOrLaunchAllowed",
];
JsonElement transfer = contract.GetProperty("requiredTransferMap");
JsonElement successor = contract.GetProperty("successorOnTransferMapMissing");
string[] expectedTransferKeys =
[
    "mustMapTargetDegreesOfFreedom", "mustMapForceAndGradient", "mustMapStepSizeAndLeapfrogCount",
    "mustJustifyAnyParameterDifference", "scalarSuccessMayNotImplyCompleteLatticeSuccess",
];
string[] expectedSuccessorChecks =
[
    "exact-force-or-finite-difference-gradient-parity", "forward-reverse-recovery",
    "finite-energy-step-halving", "explicit-reduced-row-to-pilot-parameter-map",
    "resource-refusal-before-allocation",
];
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase540-a25-reduced-to-complete-lattice-transfer-readiness-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A25"
    && contract.GetProperty("frozenBeforePrecursorConsumption").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && exactBindingsValid
    && contract.GetProperty("gateOrder").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedGateOrder)
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedTaxonomy)
    && transfer.EnumerateObject().Select(x => x.Name).SequenceEqual(expectedTransferKeys)
    && transfer.EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.True)
    && successor.GetProperty("kind").GetString() == "prospective-deterministic-complete-lattice-integrator-transfer-control"
    && !successor.GetProperty("samplingAllowed").GetBoolean()
    && successor.GetProperty("requiredChecks").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedSuccessorChecks)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

JsonElement pilot = p533Contract.GetProperty("pilotConfiguration");
JsonElement reducedRow = p539.GetProperty("fixedSelectedRow");
bool precursorSemanticsValid =
    p533.GetProperty("verdictKind").GetString() == "nested-validation-contract-closed"
    && p535.GetProperty("verdictKind").GetString() == "reduced-interacting-control-failed"
    && p537.GetProperty("verdictKind").GetString() == "deterministic-leapfrog-audit-passed"
    && p538.GetProperty("verdictKind").GetString() == "post-review-hardened-stable-fixed-grid-row-reduced-target-feasible"
    && p539.GetProperty("verdictKind").GetString() == "selected-row-independently-confirmed-reduced-target-only";
bool independentReducedRowConfirmed = precursorSemanticsValid
    && p539.GetProperty("independentPostSelectionConfirmation").GetBoolean()
    && p539.GetProperty("everyRegisteredFamilyPassedEveryGate").GetBoolean();

bool targetIdentityMetadataAligned =
    pilot.GetProperty("member").GetString() == "sd2-id0/c0.5"
    && pilot.GetProperty("extent").GetInt32() == 3
    && pilot.GetProperty("thetaRule").GetString() == "theta-identically-zero";
bool integratorParametersIdentical =
    pilot.GetProperty("stepSize").GetDouble() == reducedRow.GetProperty("stepSize").GetDouble()
    && pilot.GetProperty("leapfrogSteps").GetInt32() == reducedRow.GetProperty("leapfrogSteps").GetInt32();
bool explicitTransferMapPresent = p539.GetProperty("reducedToCompleteLatticeTransferValidated").GetBoolean();
bool deterministicCompleteLatticeIntegratorOraclePresent = false;
bool executableBoundedPilotBranchPresent = p535.GetProperty("pilotRun").GetBoolean();
bool hardenedCompleteLatticeDiagnosticsAndIndependentSeedsPresent = false;
bool boundedResourceAndExecutionTopologyPresent = p533.GetProperty("resourceEnvelopeAccepted").GetBoolean()
    && executableBoundedPilotBranchPresent;

var gates = new[]
{
    Gate(expectedGateOrder[0], independentReducedRowConfirmed,
        "Phase539 must independently confirm the fixed reduced row."),
    Gate(expectedGateOrder[1], explicitTransferMapPresent,
        "No exact-bound A25 input maps the one-dimensional target, force, and 0.25 x 8 row to the complete-lattice pilot; the frozen pilot instead specifies 0.0125 x 6."),
    Gate(expectedGateOrder[2], deterministicCompleteLatticeIntegratorOraclePresent,
        "The exact-bound Phase537 input audits only the scalar polynomial and cannot certify the complete-lattice force or map."),
    Gate(expectedGateOrder[3], executableBoundedPilotBranchPresent,
        "Phase535 deliberately contains no executable pilot branch after its upstream refusal."),
    Gate(expectedGateOrder[4], hardenedCompleteLatticeDiagnosticsAndIndependentSeedsPresent,
        "No exact-bound A25 input supplies a prospective complete-lattice analogue of the hardened Phase538/539 diagnostics and post-selection seed rules."),
    Gate(expectedGateOrder[5], boundedResourceAndExecutionTopologyPresent,
        "The Phase533 estimate is bounded, but no executable branch/topology exists against which to validate it."),
};
int earliestMissingIndex = Array.FindIndex(gates, x => !x.Passed);
string verdict = !contractValid || !precursorSemanticsValid ? expectedTaxonomy[0]
    : !independentReducedRowConfirmed ? expectedTaxonomy[1]
    : !explicitTransferMapPresent ? expectedTaxonomy[2]
    : !deterministicCompleteLatticeIntegratorOraclePresent ? expectedTaxonomy[3]
    : !executableBoundedPilotBranchPresent ? expectedTaxonomy[4]
    : !hardenedCompleteLatticeDiagnosticsAndIndependentSeedsPresent ? expectedTaxonomy[5]
    : !boundedResourceAndExecutionTopologyPresent ? expectedTaxonomy[6]
    : expectedTaxonomy[7];
bool ready = verdict == expectedTaxonomy[7];

var result = new
{
    schemaVersion = 1,
    phase = 540,
    phaseId = "phase540-reduced-to-complete-lattice-transfer-readiness",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    bindingInventoryValid,
    exactBindingsValid,
    precursorSemanticsValid,
    deterministicZeroSampling = true,
    hmcOrSamplingPerformed = false,
    configurationsRetained = false,
    targetIdentityMetadataAligned,
    integratorParametersIdentical,
    independentReducedRowConfirmed,
    explicitTransferMapPresent,
    deterministicCompleteLatticeIntegratorOraclePresent,
    executableBoundedPilotBranchPresent,
    hardenedCompleteLatticeDiagnosticsAndIndependentSeedsPresent,
    boundedResourceAndExecutionTopologyPresent,
    reducedRowParameters = new
    {
        stepSize = reducedRow.GetProperty("stepSize").GetDouble(),
        leapfrogSteps = reducedRow.GetProperty("leapfrogSteps").GetInt32(),
    },
    completeLatticePilotParameters = new
    {
        stepSize = pilot.GetProperty("stepSize").GetDouble(),
        leapfrogSteps = pilot.GetProperty("leapfrogSteps").GetInt32(),
    },
    gates,
    earliestMissingGate = earliestMissingIndex < 0 ? null : gates[earliestMissingIndex].Id,
    allTransferReadinessGatesClosed = gates.All(x => x.Passed),
    laterControlConstructionAuthorized = ready,
    successor = ready ? null : new
    {
        kind = successor.GetProperty("kind").GetString(),
        samplingAllowed = false,
        requiredChecks = successor.GetProperty("requiredChecks").EnumerateArray().Select(x => x.GetString()).ToArray(),
        separatelyRegisteredAndProspectivelyFrozenPhaseRequired = true,
    },
    verdictKind = verdict,
    terminalStatus = "reduced-to-complete-lattice-transfer-readiness-" + verdict,
    decision = verdict == expectedTaxonomy[2]
        ? "The reduced row is independently confirmed, but no explicit target/force/integrator mapping connects it to the differently parameterized complete-lattice pilot. A deterministic transfer-control phase is the smallest admissible successor; Phase535 remains closed."
        : "The adjudicator preserves the earliest fail-closed readiness terminal. No sampling or downstream authority follows.",
    phase535PilotExecutedOrReopened = false,
    phase535Mutated = false,
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
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase540 verdict: {verdict}");
Console.WriteLine($"earliest missing gate: {result.earliestMissingGate ?? "none"}");

static GateResult Gate(string id, bool passed, string evidence) => new(id, passed, evidence);
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
sealed record BindingSpec(string Id, string Path, string ExpectedSha256);
sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record GateResult(string Id, bool Passed, string Evidence);
