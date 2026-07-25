using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase547_bounded_pilot_pack_readiness_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase547_bounded_pilot_pack_readiness_contract_v1.json";
const string OutputPath = Root + "/output/bounded_pilot_pack_readiness_adjudicator.json";
const string SummaryPath = Root + "/output/bounded_pilot_pack_readiness_adjudicator_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new BindingSpec(
    x.GetProperty("id").GetString()!,
    x.GetProperty("path").GetString()!,
    x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = bindingSpecs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new BindingResult(x.Id, x.Path, x.ExpectedSha256, actual, actual == x.ExpectedSha256);
}).ToArray();
string[] expectedBindingIds =
[
    "phase544-contract", "phase544-program", "phase544-summary",
    "phase545-v1-contract", "phase545-v1-program", "phase545-v1-kernel",
    "phase545-v1-output", "phase545-v1-summary",
    "phase545-v2-contract", "phase545-v2-program", "phase545-v2-kernel",
    "phase545-v2-output", "phase545-v2-summary",
    "phase545-v3-contract", "phase545-v3-program", "phase545-v3-kernel",
    "phase545-v3-output", "phase545-v3-summary",
    "phase546-v1-contract", "phase546-v1-program", "phase546-v1-codec",
    "phase546-v1-output", "phase546-v1-summary",
    "phase546-v2-contract", "phase546-v2-program", "phase546-v2-output", "phase546-v2-summary",
    "phase546-v3-contract", "phase546-v3-program", "phase546-v3-output", "phase546-v3-summary",
    "phase533-contract", "phase538-contract", "phase539-contract",
];
bool exactBindingsValid = bindingSpecs.Select(x => x.Id).SequenceEqual(expectedBindingIds)
    && bindings.All(x => x.HashMatches);
string PathOf(string id) => bindingSpecs.Single(x => x.Id == id).Path;

using var p544Doc = JsonDocument.Parse(File.ReadAllBytes(PathOf("phase544-summary")));
using var p545Doc = JsonDocument.Parse(File.ReadAllBytes(PathOf("phase545-v3-summary")));
using var p545v3ContractDoc = JsonDocument.Parse(File.ReadAllBytes(PathOf("phase545-v3-contract")));
using var p546v1Doc = JsonDocument.Parse(File.ReadAllBytes(PathOf("phase546-v1-summary")));
using var p546v1ContractDoc = JsonDocument.Parse(File.ReadAllBytes(PathOf("phase546-v1-contract")));
using var p546v3Doc = JsonDocument.Parse(File.ReadAllBytes(PathOf("phase546-v3-summary")));
using var p533Doc = JsonDocument.Parse(File.ReadAllBytes(PathOf("phase533-contract")));
using var p538Doc = JsonDocument.Parse(File.ReadAllBytes(PathOf("phase538-contract")));
using var p539Doc = JsonDocument.Parse(File.ReadAllBytes(PathOf("phase539-contract")));
JsonElement p544 = p544Doc.RootElement;
JsonElement p545 = p545Doc.RootElement;
JsonElement p545Contract = p545v3ContractDoc.RootElement;
JsonElement p546v1 = p546v1Doc.RootElement;
JsonElement p546v1Contract = p546v1ContractDoc.RootElement;
JsonElement p546v3 = p546v3Doc.RootElement;
JsonElement p533 = p533Doc.RootElement;
JsonElement p538 = p538Doc.RootElement;
JsonElement p539 = p539Doc.RootElement;

string[] gateIds =
[
    "new-executable-bounded-pilot-implementation",
    "hardened-complete-lattice-diagnostics",
    "disjoint-prospective-seeds-and-checkpoint-rules",
    "executable-resource-topology-refusal",
];
string[] taxonomy =
[
    "invalid-or-drifted-input",
    "phase544-readiness-premise-invalid",
    "executable-bounded-pilot-implementation-missing",
    "hardened-complete-lattice-diagnostics-missing",
    "prospective-seed-or-checkpoint-rules-missing",
    "resource-topology-refusal-evidence-missing",
    "bounded-pilot-pack-ready-for-separate-prospective-execution-registration",
];
string[] authorityKeys =
[
    "rngUsed", "markovChainAdvanced", "warmupPerformed", "adaptationPerformed",
    "samplingPerformed", "benchmarkPerformed", "configurationsRetained", "pilotExecuted",
    "pilotExecutionAuthorized", "launchAuthorized", "hmcEvidenceEstablished",
    "acceptanceRateEstablished", "stationarityEstablished", "detailedBalanceEstablished",
    "mixingEstablished", "convergenceEstablished", "observableEstimateProduced",
    "phase535ExecutedReopenedOrMutated", "phase481PackCreatedOrMutated",
    "productionDefaultSelected", "phase458G3Satisfied", "phase458G4Satisfied",
    "phase458G5Satisfied", "o4Discharged", "sourceContractApplicationAllowed",
    "physicalUnitOrGevClaimAllowed", "productionOrLaunchAllowed",
];
JsonElement boundaries = contract.GetProperty("evidenceBoundaries");
JsonElement firewalls = contract.GetProperty("authorityFirewalls");
bool contractValid =
    contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString()
        == "phase547-a28-bounded-pilot-pack-readiness-adjudicator-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A28"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && exactBindingsValid
    && contract.GetProperty("readinessGatesInOrder").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(gateIds)
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(taxonomy)
    && boundaries.GetProperty("phase545DeterministicFixtureCount").GetInt32() == 10
    && boundaries.GetProperty("injectedDeterministicSingleProposalFixturesPerformed").GetBoolean()
    && boundaries.GetProperty("phase545ResourceFixtureEstablishesOnlyEarlyRefusal").GetBoolean()
    && !boundaries.GetProperty("phase545CompletePeakMemoryBoundEstablished").GetBoolean()
    && !boundaries.GetProperty("phase545CombinedWorkFixtureIndependentlyTestsForceCap").GetBoolean()
    && boundaries.GetProperty("phase546AuthenticV1FixtureCount").GetInt32() == 13
    && !boundaries.GetProperty("phase546AuthenticV1DofBranchDynamicallyTested").GetBoolean()
    && boundaries.GetProperty("phase546V3ExecutedFixtureCount").GetInt32() == 1
    && boundaries.GetProperty("phase546CompositionalCorrectedFixtureCount").GetInt32() == 14
    && !boundaries.GetProperty("resourceArithmeticIsBenchmark").GetBoolean()
    && firewalls.EnumerateObject().Select(x => x.Name).SequenceEqual(authorityKeys)
    && firewalls.EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

string[] p544GateIds = p544.GetProperty("gates").EnumerateArray()
    .Select(x => x.GetProperty("id").GetString()!).ToArray();
bool[] p544GatePasses = p544.GetProperty("gates").EnumerateArray()
    .Select(x => x.GetProperty("passed").GetBoolean()).ToArray();
bool phase544PremiseValid =
    p544.GetProperty("contractValid").GetBoolean()
    && p544.GetProperty("exactBindingsValid").GetBoolean()
    && p544.GetProperty("historicalPhase535StayedClosed").GetBoolean()
    && p544.GetProperty("verdictKind").GetString() == "executable-bounded-pilot-implementation-missing"
    && p544.GetProperty("earliestMissingGate").GetString() == "new-executable-bounded-pilot-implementation"
    && p544.GetProperty("passedGateCount").GetInt32() == 3
    && p544.GetProperty("failedGateCount").GetInt32() == 4
    && p544GateIds.SequenceEqual([
        "resolved-force-closure-branch",
        "deterministic-complete-lattice-force-gradient-oracle",
        "deterministic-complete-lattice-multistate-integrator-controls",
        .. gateIds])
    && p544GatePasses.SequenceEqual([true, true, true, false, false, false, false])
    && !p544.GetProperty("rngUsed").GetBoolean()
    && !p544.GetProperty("hmcOrSamplingPerformed").GetBoolean()
    && !p544.GetProperty("configurationsRetained").GetBoolean()
    && !p544.GetProperty("phase535ExecutedReopenedOrMutated").GetBoolean()
    && !p544.GetProperty("productionAuthorized").GetBoolean()
    && !p544.GetProperty("launchAuthorized").GetBoolean()
    && p544.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

string[] p545FixtureNames =
[
    "resourceRefusal", "workRefusal", "corruption", "invalidGradientLength",
    "intermediateOverflow", "divergentNotAccepted", "exactAcceptBoundary",
    "accept", "reject", "reversal",
];
JsonElement p545Fixtures = p545.GetProperty("fixtures");
bool p545FixtureInventoryValid = p545Fixtures.EnumerateObject().Select(x => x.Name)
    .SequenceEqual(p545FixtureNames)
    && p545Fixtures.EnumerateObject().All(x => x.Value.GetProperty("passed").GetBoolean());
JsonElement p545Resource = p545Fixtures.GetProperty("resourceRefusal");
JsonElement p545Work = p545Fixtures.GetProperty("workRefusal");
bool p545LimitationsPreserved =
    p545Resource.GetProperty("resourceRefused").GetBoolean()
    && !p545Resource.GetProperty("workRefused").GetBoolean()
    && !p545Resource.GetProperty("divergent").GetBoolean()
    && p545Resource.GetProperty("refusalReason").GetString() == "maximum-working-bytes-exceeded"
    && p545Resource.GetProperty("evaluatorCalls").GetInt32() == 0
    && p545Resource.GetProperty("forceEvaluationCount").GetInt32() == 0
    && !p545Work.GetProperty("resourceRefused").GetBoolean()
    && p545Work.GetProperty("workRefused").GetBoolean()
    && !p545Work.GetProperty("divergent").GetBoolean()
    && p545Work.GetProperty("refusalReason").GetString() == "maximum-leapfrog-steps-exceeded"
    && p545Work.GetProperty("evaluatorCalls").GetInt32() == 0
    && p545Work.GetProperty("forceEvaluationCount").GetInt32() == 0
    && p545Work.GetProperty("estimatedForceEvaluations").GetInt64() == 18;
bool p545CoreFixturesValid =
    !p545Fixtures.GetProperty("corruption").GetProperty("finite").GetBoolean()
    && p545Fixtures.GetProperty("corruption").GetProperty("divergent").GetBoolean()
    && !p545Fixtures.GetProperty("corruption").GetProperty("accepted").GetBoolean()
    && p545Fixtures.GetProperty("corruption").GetProperty("forceEvaluationCount").GetInt32() == 1
    && !p545Fixtures.GetProperty("invalidGradientLength").GetProperty("finite").GetBoolean()
    && p545Fixtures.GetProperty("invalidGradientLength").GetProperty("divergent").GetBoolean()
    && !p545Fixtures.GetProperty("invalidGradientLength").GetProperty("accepted").GetBoolean()
    && p545Fixtures.GetProperty("invalidGradientLength").GetProperty("evaluatorCalls").GetInt32() == 1
    && !p545Fixtures.GetProperty("intermediateOverflow").GetProperty("finite").GetBoolean()
    && p545Fixtures.GetProperty("intermediateOverflow").GetProperty("divergent").GetBoolean()
    && p545Fixtures.GetProperty("intermediateOverflow").GetProperty("evaluatorCalls").GetInt32() == 1
    && p545Fixtures.GetProperty("divergentNotAccepted").GetProperty("finite").GetBoolean()
    && p545Fixtures.GetProperty("divergentNotAccepted").GetProperty("divergent").GetBoolean()
    && !p545Fixtures.GetProperty("divergentNotAccepted").GetProperty("accepted").GetBoolean()
    && p545Fixtures.GetProperty("divergentNotAccepted").GetProperty("deltaHamiltonian").GetDouble() == -200.0
    && p545Fixtures.GetProperty("divergentNotAccepted").GetProperty("logAcceptanceIsNegativeInfinity").GetBoolean()
    && p545Fixtures.GetProperty("exactAcceptBoundary").GetProperty("accepted").GetBoolean()
    && p545Fixtures.GetProperty("exactAcceptBoundary").GetProperty("deltaHamiltonian").GetDouble() == 0.0
    && p545Fixtures.GetProperty("accept").GetProperty("accepted").GetBoolean()
    && p545Fixtures.GetProperty("accept").GetProperty("finite").GetBoolean()
    && !p545Fixtures.GetProperty("accept").GetProperty("divergent").GetBoolean()
    && !p545Fixtures.GetProperty("reject").GetProperty("accepted").GetBoolean()
    && p545Fixtures.GetProperty("reject").GetProperty("finite").GetBoolean()
    && !p545Fixtures.GetProperty("reject").GetProperty("divergent").GetBoolean()
    && p545Fixtures.GetProperty("reject").GetProperty("rejectionReturnedInjectedPosition").GetBoolean()
    && p545Fixtures.GetProperty("reversal").GetProperty("forwardFinite").GetBoolean()
    && p545Fixtures.GetProperty("reversal").GetProperty("reverseFinite").GetBoolean()
    && p545Fixtures.GetProperty("reversal").GetProperty("scaledError").GetDouble()
        <= p545Fixtures.GetProperty("reversal").GetProperty("tolerance").GetDouble();
string[] p545FalseEvidenceFields =
[
    "rngUsed", "markovChainAdvanced", "warmupPerformed", "adaptationPerformed",
    "samplingPerformed", "configurationsRetained", "benchmarkPerformed", "pilotExecuted",
    "pilotExecutionAuthorized", "hmcEvidenceEstablished", "acceptanceRateEstablished",
    "stationarityEstablished", "detailedBalanceEstablished", "mixingEstablished",
    "convergenceEstablished", "observableEstimateProduced",
];
bool p545AuthorityValid = p545FalseEvidenceFields.All(x => !p545.GetProperty(x).GetBoolean())
    && !p545.GetProperty("phase535ExecutedReopenedOrMutated").GetBoolean()
    && !p545.GetProperty("phase481PackCreatedOrMutated").GetBoolean()
    && !p545.GetProperty("productionAuthorized").GetBoolean()
    && !p545.GetProperty("launchAuthorized").GetBoolean()
    && p545.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
bool executableImplementation =
    p545.GetProperty("schemaVersion").GetInt32() == 3
    && p545.GetProperty("contractValid").GetBoolean()
    && p545.GetProperty("rulesValid").GetBoolean()
    && p545.GetProperty("fixtureRulesValid").GetBoolean()
    && p545.GetProperty("firewallInventoryValid").GetBoolean()
    && p545.GetProperty("preReviewBindingsValid").GetBoolean()
    && p545.GetProperty("interimV2BindingsValid").GetBoolean()
    && p545.GetProperty("upstreamBindingsValid").GetBoolean()
    && p545.GetProperty("precursorSemanticsValid").GetBoolean()
    && !p545.GetProperty("originalPositiveResultCitable").GetBoolean()
    && !p545.GetProperty("interimCorrectedExecutionCitable").GetBoolean()
    && !p545.GetProperty("v2RepairResultCitable").GetBoolean()
    && p545.GetProperty("verdictKind").GetString() == "injectable-deterministic-hmc-kernel-v3-repaired"
    && p545.GetProperty("reusableKernelV3Repaired").GetBoolean()
    && p545.GetProperty("laterDependentPackReadinessAdjudicationAllowed").GetBoolean()
    && p545.GetProperty("completeLattice").GetProperty("extent").GetInt32() == 3
    && p545.GetProperty("completeLattice").GetProperty("omegaDegreesOfFreedom").GetInt32() == 3645
    && p545.GetProperty("kernelSha256").GetString()
        == p545Contract.GetProperty("interimV2Bindings").EnumerateArray()
            .Single(x => x.GetProperty("id").GetString() == "v2-kernel")
            .GetProperty("sha256").GetString()
    && p545FixtureInventoryValid && p545LimitationsPreserved && p545CoreFixturesValid && p545AuthorityValid;

string[] expectedTrajectoryFields =
[
    "chainId", "trajectoryIndex", "phase", "accepted", "initialHamiltonian", "proposedHamiltonian",
    "deltaH", "acceptanceProbability", "injectedThreshold", "nonFinite", "divergent",
    "divergenceThreshold", "forceEvaluationCount", "maximumForceNorm", "proposalElapsedCpuTicks",
];
string[] expectedChainFields =
[
    "chainId", "seed", "warmupCount", "retainedCount", "acceptanceRate", "nonFiniteCount",
    "divergenceCount", "maximumAbsoluteDeltaH", "splitRankNormalizedRhat", "bulkEss", "tailEss",
    "observableSchemaId", "observableMeans", "observableStandardErrors", "observableRhat",
    "observableBulkEss", "checkpointSha256",
];
JsonElement telemetry = p546v1.GetProperty("telemetrySchema");
bool hardenedDiagnostics =
    p546v1.GetProperty("contractValid").GetBoolean()
    && p546v1.GetProperty("exactBindingsValid").GetBoolean()
    && p546v1.GetProperty("schemaValid").GetBoolean()
    && p546v1.GetProperty("allFixturesPassed").GetBoolean()
    && telemetry.GetProperty("trajectoryRequiredFields").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(expectedTrajectoryFields)
    && telemetry.GetProperty("chainSummaryRequiredFields").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(expectedChainFields)
    && telemetry.GetProperty("convergenceThresholdsFrozen").GetBoolean()
    && telemetry.GetProperty("observableSchemaValid").GetBoolean()
    && telemetry.GetProperty("completeLatticeTelemetryFrozen").GetBoolean();

static int[] Seeds(JsonElement families) => families.EnumerateArray()
    .SelectMany(x => x.GetProperty("seeds").EnumerateArray().Select(y => y.GetInt32())).ToArray();
static int[] ExecutionSeeds(JsonElement families) => families.EnumerateArray()
    .SelectMany(x =>
    {
        int offset = x.GetProperty("seedOffset").GetInt32();
        return x.GetProperty("seeds").EnumerateArray().Select(y => checked(y.GetInt32() + offset));
    }).ToArray();
int[] p533Raw = Seeds(p533.GetProperty("seedTables"));
JsonElement p538Families = p538.GetProperty("seedFamilies");
JsonElement p539Families = p539.GetProperty("independentSeedFamilies");
int[] historicalRaw = Seeds(p538Families).Concat(Seeds(p539Families)).Distinct().Order().ToArray();
int[] historicalExecution = ExecutionSeeds(p538Families).Concat(ExecutionSeeds(p539Families))
    .Distinct().Order().ToArray();
JsonElement priorCensus = p546v1Contract.GetProperty("priorSeedCensus");
int[] frozenHistoricalRaw = priorCensus.GetProperty("rawSeeds").EnumerateArray()
    .Select(x => x.GetInt32()).Order().ToArray();
int[] frozenHistoricalExecution = priorCensus.GetProperty("executionSeeds").EnumerateArray()
    .Select(x => x.GetInt32()).Order().ToArray();
JsonElement prospectiveFamilies = p546v1Contract.GetProperty("prospectiveSeedTables");
int[] prospectiveRaw = Seeds(prospectiveFamilies);
int[] prospectiveExecution = ExecutionSeeds(prospectiveFamilies);
bool historicalSeedCensusValid =
    p533Raw.All(x => Seeds(p538Families).Contains(x))
    && historicalRaw.SequenceEqual(frozenHistoricalRaw)
    && historicalExecution.SequenceEqual(frozenHistoricalExecution);
bool prospectiveSeedsValid =
    prospectiveFamilies.GetArrayLength() == 2
    && prospectiveRaw.Length == 8 && prospectiveExecution.Length == 8
    && prospectiveRaw.Distinct().Count() == 8
    && prospectiveExecution.Distinct().Count() == 8
    && !prospectiveRaw.Intersect(prospectiveExecution).Any()
    && !prospectiveRaw.Intersect(historicalRaw).Any()
    && !prospectiveRaw.Intersect(historicalExecution).Any()
    && !prospectiveExecution.Intersect(historicalRaw).Any()
    && !prospectiveExecution.Intersect(historicalExecution).Any()
    && p546v1.GetProperty("seedPack")
        .GetProperty("disjointFromAllA22ThroughA24SeedFamilies").GetBoolean();
string[] restartFields =
[
    "formatId", "actionFingerprint", "packFingerprint", "topologyId", "extent", "dimensions",
    "degreesOfFreedom", "beta", "thetaRule", "chainId", "seed", "trajectoryIndex",
    "warmupCompleted", "retainedCompleted", "stepSize", "leapfrogSteps", "rngAlgorithm",
    "rngStateWords", "position",
];
JsonElement checkpointContract = p546v1Contract.GetProperty("checkpointSchema");
JsonElement checkpointEvidence = p546v1.GetProperty("checkpointCodec");
bool checkpointRulesValid =
    checkpointContract.GetProperty("formatId").GetString() == "gu-complete-lattice-checkpoint-v1"
    && checkpointContract.GetProperty("checksumAlgorithm").GetString() == "SHA-256"
    && checkpointContract.GetProperty("exactRestartFields").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(restartFields)
    && !string.IsNullOrWhiteSpace(checkpointContract.GetProperty("actionFingerprint").GetString())
    && !string.IsNullOrWhiteSpace(checkpointContract.GetProperty("packFingerprint").GetString())
    && checkpointContract.GetProperty("rejectChecksumMismatch").GetBoolean()
    && checkpointContract.GetProperty("rejectNonCanonicalPayload").GetBoolean()
    && checkpointEvidence.GetProperty("roundtripPassed").GetBoolean()
    && checkpointEvidence.GetProperty("mutationRefused").GetBoolean()
    && checkpointEvidence.GetProperty("nonCanonicalPayloadRefused").GetBoolean();
bool seedsAndCheckpoint = historicalSeedCensusValid && prospectiveSeedsValid && checkpointRulesValid;

string[] authenticV1FixtureIds =
[
    "schema-valid", "observable-schema-valid", "checkpoint-roundtrip",
    "checkpoint-mutation-refused", "checkpoint-noncanonical-refused", "seeds-disjoint",
    "resource-below-boundary-accepted", "resource-cpu-boundary-refused",
    "resource-memory-boundary-refused", "resource-topology-refused",
    "resource-dimension-refused", "resource-max-chain-refused", "resource-overflow-refused",
];
JsonElement v1Fixtures = p546v1.GetProperty("fixtures");
bool authenticV1FixturesValid =
    v1Fixtures.GetArrayLength() == 13
    && v1Fixtures.EnumerateArray().Select(x => x.GetProperty("id").GetString())
        .SequenceEqual(authenticV1FixtureIds)
    && v1Fixtures.EnumerateArray().All(x => x.GetProperty("passed").GetBoolean())
    && p546v1.GetProperty("passedFixtureCount").GetInt32() == 13
    && p546v1.GetProperty("resourceRefusal").GetProperty("allBoundaryFixturesPassed").GetBoolean();
bool v1DofBranchDynamicallyTested = authenticV1FixtureIds.Contains("resource-dof-mismatch-refused");
JsonElement repair = p546v3.GetProperty("repairEvidence");
bool v3OneFixtureRepairValid =
    p546v3.GetProperty("contractValid").GetBoolean()
    && p546v3.GetProperty("exactBindingsValid").GetBoolean()
    && p546v3.GetProperty("lineage").GetProperty("authenticV1Valid").GetBoolean()
    && p546v3.GetProperty("lineage").GetProperty("v1PositiveResultIncomplete").GetBoolean()
    && !p546v3.GetProperty("lineage").GetProperty("v1CitableForPhase547ResourceGate").GetBoolean()
    && p546v3.GetProperty("lineage").GetProperty("v2PositiveResultNonCitable").GetBoolean()
    && repair.GetProperty("fixtureId").GetString() == "resource-dof-mismatch-refused"
    && repair.GetProperty("dofMismatchFixtureTested").GetBoolean()
    && repair.GetProperty("dofMismatchFixturePassed").GetBoolean()
    && !repair.GetProperty("assessment").GetProperty("allowed").GetBoolean()
    && repair.GetProperty("assessment").GetProperty("refusalReason").GetString() == "invalid-shape"
    && repair.GetProperty("assessment").GetProperty("expectedDegreesOfFreedom").GetInt32() == 3645
    && repair.GetProperty("assessment").GetProperty("observedDegreesOfFreedom").GetInt32() == 3644
    && p546v3.GetProperty("inheritedAuthenticV1FixtureCount").GetInt32() == 13
    && p546v3.GetProperty("newDedicatedFixtureCount").GetInt32() == 1
    && p546v3.GetProperty("allCorrectedFixturesPassed").GetBoolean()
    && p546v3.GetProperty("verdictKind").GetString()
        == "pilot-support-pack-materialized-with-authentic-lineage-dof-repair";
bool resourceTopologyRefusal =
    authenticV1FixturesValid && !v1DofBranchDynamicallyTested && v3OneFixtureRepairValid
    && !p546v1.GetProperty("benchmarkPerformed").GetBoolean()
    && !p546v3.GetProperty("benchmarkPerformed").GetBoolean()
    && !p546v3.GetProperty("registeredOperatorProposalPerformed").GetBoolean();

var gates = new[]
{
    new Gate(gateIds[0], executableImplementation,
        "Phase545 v3 repairs the deterministic injectable kernel; ten deterministic fixtures include pre-proposal refusals and injected proposal controls, but no HMC evidence follows."),
    new Gate(gateIds[1], hardenedDiagnostics,
        "Authentic Phase546 v1 freezes complete-lattice trajectory, chain, convergence, and operational-observable telemetry."),
    new Gate(gateIds[2], seedsAndCheckpoint,
        "Historical seeds are independently reconstructed; prospective seeds are disjoint and the canonical checkpoint rules pass synthetic controls."),
    new Gate(gateIds[3], resourceTopologyRefusal,
        "Thirteen authentic v1 fixtures compose with one v3 DOF fixture; v3 executed one fixture, not fourteen, and no benchmark ran."),
};

string Evaluate(bool inputs, bool premise, bool implementation, bool diagnostics, bool seeds, bool resources) =>
    !inputs ? taxonomy[0]
    : !premise ? taxonomy[1]
    : !implementation ? taxonomy[2]
    : !diagnostics ? taxonomy[3]
    : !seeds ? taxonomy[4]
    : !resources ? taxonomy[5]
    : taxonomy[6];
var precedenceBattery = new[]
{
    Case("invalid-dominates", false, false, false, false, false, false, taxonomy[0]),
    Case("phase544-dominates", true, false, false, false, false, false, taxonomy[1]),
    Case("implementation-dominates", true, true, false, false, false, false, taxonomy[2]),
    Case("diagnostics-dominates", true, true, true, false, false, false, taxonomy[3]),
    Case("seeds-checkpoint-dominates", true, true, true, true, false, false, taxonomy[4]),
    Case("resources", true, true, true, true, true, false, taxonomy[5]),
    Case("ready", true, true, true, true, true, true, taxonomy[6]),
};
bool precedenceBatteryPassed = precedenceBattery.All(x => x.Passed);
bool inputsValid = contractValid && exactBindingsValid && precedenceBatteryPassed;
string verdict = Evaluate(inputsValid, phase544PremiseValid, executableImplementation,
    hardenedDiagnostics, seedsAndCheckpoint, resourceTopologyRefusal);
bool registrationAllowed = verdict == taxonomy[6];
string? earliestMissingGate = gates.FirstOrDefault(x => !x.Passed)?.Id;

var output = new
{
    schemaVersion = 1,
    phase = 547,
    phaseId = "phase547-bounded-pilot-pack-readiness-adjudicator",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    inputsValid,
    phase544PremiseValid,
    precedenceBatteryPassed,
    precedenceBattery,
    gates,
    passedGateCount = gates.Count(x => x.Passed),
    failedGateCount = gates.Count(x => !x.Passed),
    earliestMissingGate,
    allFourMissingPhase544GatesClosed = gates.All(x => x.Passed),
    verdictKind = verdict,
    terminalStatus = "bounded-pilot-pack-readiness-adjudicator-" + verdict,
    boundedPilotExecutionPhaseRegistrationAllowed = registrationAllowed,
    injectedDeterministicSingleProposalFixturesPerformedByPhase545 = true,
    phase547ProposalFixturesPerformed = false,
    phase545DeterministicFixtureCount = 10,
    phase545ResourceFixtureEstablishesOnlyEarlyRefusal = true,
    phase545CompletePeakMemoryBoundEstablished = false,
    phase545CombinedWorkFixtureIndependentlyTestsForceCap = false,
    phase546AuthenticV1FixtureCount = 13,
    phase546AuthenticV1DofBranchDynamicallyTested = v1DofBranchDynamicallyTested,
    phase546V3ExecutedFixtureCount = 1,
    phase546CompositionalCorrectedFixtureCount = 14,
    resourceArithmeticIsBenchmark = false,
    historicalSeedCensusValid,
    prospectiveSeedsValid,
    checkpointRulesValid,
    rngUsed = false,
    markovChainAdvanced = false,
    warmupPerformed = false,
    adaptationPerformed = false,
    samplingPerformed = false,
    benchmarkPerformed = false,
    configurationsRetained = false,
    pilotExecuted = false,
    pilotExecutionAuthorized = false,
    launchAuthorized = false,
    hmcEvidenceEstablished = false,
    acceptanceRateEstablished = false,
    stationarityEstablished = false,
    detailedBalanceEstablished = false,
    mixingEstablished = false,
    convergenceEstablished = false,
    observableEstimateProduced = false,
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
    externalReviewPending = true,
    allDownstreamAuthority = false,
    promotedPhysicalMassClaimCount = 0,
    decision = registrationAllowed
        ? "The four missing Phase544 pack-construction gates close narrowly. Only registration of a separate prospectively frozen bounded-pilot execution phase is allowed; execution and launch remain forbidden."
        : "The earliest frozen Phase547 failure is preserved. No execution, launch, or downstream scientific authority follows.",
    bindings,
};
Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(output, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase547 verdict: {verdict}");
Console.WriteLine($"gates={gates.Count(x => x.Passed)}/4, precedence={precedenceBatteryPassed}, registration={registrationAllowed}");
Console.WriteLine("rng=False, proposal=False, pilot=False, sampling=False, launch=False");

PrecedenceCase Case(
    string id, bool inputs, bool premise, bool implementation, bool diagnostics,
    bool seeds, bool resources, string expected)
{
    string actual = Evaluate(inputs, premise, implementation, diagnostics, seeds, resources);
    return new PrecedenceCase(id, expected, actual, actual == expected);
}

static string Sha(string path) =>
    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

sealed record BindingSpec(string Id, string Path, string ExpectedSha256);
sealed record BindingResult(
    string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record Gate(string Id, bool Passed, string Evidence);
sealed record PrecedenceCase(string Id, string Expected, string Actual, bool Passed);
