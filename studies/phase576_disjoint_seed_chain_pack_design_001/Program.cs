using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase576_disjoint_seed_chain_pack_design_001";
const string ContractPath = Root + "/preregistration/phase576_disjoint_seed_chain_pack_design_contract_v1.json";
const string OutputPath = Root + "/output/disjoint_seed_chain_pack_design.json";
const string SummaryPath = Root + "/output/disjoint_seed_chain_pack_design_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
bool exactBindingsValid = bindings.Length == 12
    && contract.GetProperty("requiredExactBindingCount").GetInt32() == 12
    && specs.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == 12
    && specs.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == 12
    && bindings.All(x => x.hashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input", "known-answer-battery-failed", "a40-upstream-gate-refused",
    "seed-disjointness-violated", "derived-configuration-out-of-ceiling",
    "chain-pack-design-frozen-execution-unauthorized",
];
JsonElement derivation = contract.GetProperty("derivationRules");
JsonElement seedPlan = contract.GetProperty("seedPlan");
JsonElement diagnostics = contract.GetProperty("diagnostics");
JsonElement resource = contract.GetProperty("resourceRefusal");
double coverageFactor = derivation.GetProperty("coverageFactor").GetDouble();
double warmupFraction = derivation.GetProperty("warmupFraction").GetDouble();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("phase").GetInt32() == 576
    && contract.GetProperty("contractId").GetString() == "phase576-a40-disjoint-seed-chain-pack-design-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministic").GetBoolean()
    && contract.GetProperty("zeroSampling").GetBoolean()
    && contract.GetProperty("rngUsed").GetBoolean() == false
    && contract.GetProperty("pristineSeedBlindPreregistration").GetBoolean()
    && taxonomy.SequenceEqual(expectedTaxonomy, StringComparer.Ordinal)
    && coverageFactor == 1.25 && warmupFraction == 0.15
    && derivation.GetProperty("expectedArm").GetProperty("stepSize").GetDouble() == 0.06
    && derivation.GetProperty("expectedArm").GetProperty("leapfrogSteps").GetInt32() == 32
    && derivation.GetProperty("expectedArm").GetProperty("trajectoryLength").GetDouble() == 1.92
    && seedPlan.GetProperty("tables").GetArrayLength() == 2
    && seedPlan.GetProperty("protectedPhase554SeedsRead").GetBoolean() == false
    && diagnostics.GetProperty("maximumRhat").GetDouble() == 1.01
    && diagnostics.GetProperty("minimumEss").GetDouble() == 100.0
    && diagnostics.GetProperty("rawDirectionalSeriesRetentionMandatory").GetBoolean()
    && diagnostics.GetProperty("checkpointEveryTrajectories").GetInt32() == 250
    && resource.GetProperty("maximumForceEvaluations").GetInt64() == 620000
    && resource.GetProperty("refuseBeforeFreeze").GetBoolean()
    && contract.GetProperty("authority").GetProperty("chainPackExecutionAuthorized").GetBoolean() == false
    && contract.GetProperty("authority").GetProperty("samplingAuthorized").GetBoolean() == false
    && contract.GetProperty("scope").GetProperty("packTargetsResolutionNotConvergence").GetBoolean()
    && contract.GetProperty("scope").GetProperty("phase572TerminalAndToleranceNeverRewritten").GetBoolean()
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && exactBindingsValid;

// Shared derivation helpers, exercised by the battery before audited reads.
static int DeriveRetained(double coverage, double slowest, double trajectoryLength)
    => checked((int)System.Math.Ceiling(coverage * slowest / trajectoryLength));
static int DeriveWarmup(double fraction, int retained)
    => checked((int)System.Math.Ceiling(fraction * retained));
static bool SeedsDisjoint(IEnumerable<int> packSeeds, IEnumerable<int> committedSeeds)
    => !packSeeds.Intersect(committedSeeds).Any();

string SelectTerminal(bool invalid, bool batteryFailed, bool gateRefused, bool seedViolated, bool outOfCeiling)
{
    if (invalid) return taxonomy[0];
    if (batteryFailed) return taxonomy[1];
    if (gateRefused) return taxonomy[2];
    if (seedViolated) return taxonomy[3];
    if (outOfCeiling) return taxonomy[4];
    return taxonomy[5];
}
var truthTable = new[]
{
    new { id = "invalid", actual = SelectTerminal(true, false, false, false, false), expected = taxonomy[0] },
    new { id = "battery", actual = SelectTerminal(false, true, false, false, false), expected = taxonomy[1] },
    new { id = "gate", actual = SelectTerminal(false, false, true, false, false), expected = taxonomy[2] },
    new { id = "seeds", actual = SelectTerminal(false, false, false, true, false), expected = taxonomy[3] },
    new { id = "ceiling", actual = SelectTerminal(false, false, false, false, true), expected = taxonomy[4] },
    new { id = "frozen", actual = SelectTerminal(false, false, false, false, false), expected = taxonomy[5] },
    new { id = "seed-precedes-ceiling", actual = SelectTerminal(false, false, false, true, true), expected = taxonomy[3] },
    new { id = "early-precedence", actual = SelectTerminal(true, true, true, true, true), expected = taxonomy[0] },
};
bool truthTablePassed = truthTable.All(x => x.actual == x.expected)
    && expectedTaxonomy.All(terminal => truthTable.Any(x => x.actual == terminal));
bool derivationFixturePassed = DeriveRetained(1.25, 100.0, 1.92) == 66
    && DeriveWarmup(0.15, 66) == 10
    && DeriveRetained(1.25, 2835.7692118162986, 1.92) == 1847
    && DeriveWarmup(0.15, 1847) == 278;
bool seedFixturePassed = !SeedsDisjoint([1, 2], [2, 3]) && SeedsDisjoint([1, 2], [3, 4]);
bool ceilingFixturePassed = 700000L > resource.GetProperty("maximumForceEvaluations").GetInt64()
    && 561000L <= resource.GetProperty("maximumForceEvaluations").GetInt64();
byte[] checksumFixture = System.Text.Encoding.UTF8.GetBytes("{\"phase\":576,\"fixture\":\"checksum\"}");
byte[] tamperedFixture = (byte[])checksumFixture.Clone();
tamperedFixture[^2] ^= 1;
bool checksumTamperDetected = Convert.ToHexString(SHA256.HashData(checksumFixture))
    != Convert.ToHexString(SHA256.HashData(tamperedFixture));
bool knownAnswerPassed = truthTablePassed && derivationFixturePassed && seedFixturePassed
    && ceilingFixturePassed && checksumTamperDetected;
var knownAnswerBattery = new
{
    auditedNumericDataParsedBeforeBattery = false,
    derivationArithmetic = new { passed = derivationFixturePassed },
    seedCollisionDetection = new { passed = seedFixturePassed },
    ceilingRefusal = new { passed = ceilingFixturePassed },
    classificationTruthTable = new { rows = truthTable, everyTerminalReached = expectedTaxonomy.All(t => truthTable.Any(x => x.actual == t)), passed = truthTablePassed },
    checksumTamperDetected, passed = knownAnswerPassed,
};
if (!contractValid || !knownAnswerPassed)
{
    string early = !contractValid ? taxonomy[0] : taxonomy[1];
    Emit(Early(early, contractValid, exactBindingsValid, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase576 verdict: {early}");
    return;
}

// Only now parse the exact-bound upstream scientific records.
JsonElement p548Summary = ReadBinding("phase548-summary");
JsonElement p548Contract = ReadBinding("phase548-contract");
JsonElement p571Contract = ReadBinding("phase571-contract-v4");
JsonElement p571 = ReadBinding("phase571-full");
JsonElement p575 = ReadBinding("phase575-full");
JsonElement requiredVerdicts = contract.GetProperty("requiredUpstreamVerdicts");
bool upstreamGateOpen = p548Summary.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase548").GetString()
    && p571.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase571").GetString()
    && p575.GetProperty("verdictKind").GetString() == requiredVerdicts.GetProperty("phase575").GetString()
    && p575.GetProperty("prospectiveChainPackPlanningGateOpen").GetBoolean()
    && p575.GetProperty("phase571LeverIndependentlyConfirmedUnderRegisteredFoldConvention").GetBoolean();
if (!upstreamGateOpen)
{
    Emit(Early(taxonomy[2], true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase576 verdict: {taxonomy[2]}");
    return;
}

// Derive every pack quantity from committed bytes.
JsonElement longArm = p571Contract.GetProperty("proposalArms").EnumerateArray()
    .Single(arm => arm.GetProperty("id").GetString() == "long");
double stepSize = longArm.GetProperty("stepSize").GetDouble();
int leapfrogSteps = longArm.GetProperty("leapfrogSteps").GetInt32();
double trajectoryLength = longArm.GetProperty("trajectoryLength").GetDouble();
double slowestMode = p548Summary.GetProperty("deterministicPrechecks").GetProperty("slowestModeTrajectoryLengthEstimate").GetDouble();
double divergenceThreshold = p548Contract.GetProperty("defaultConfiguration").GetProperty("divergenceAbsoluteDeltaH").GetDouble();
bool armMatchesContract = stepSize == derivation.GetProperty("expectedArm").GetProperty("stepSize").GetDouble()
    && leapfrogSteps == derivation.GetProperty("expectedArm").GetProperty("leapfrogSteps").GetInt32()
    && trajectoryLength == derivation.GetProperty("expectedArm").GetProperty("trajectoryLength").GetDouble();
int retainedPerChain = DeriveRetained(coverageFactor, slowestMode, trajectoryLength);
int warmupPerChain = DeriveWarmup(warmupFraction, retainedPerChain);
int trajectoriesPerChain = checked(warmupPerChain + retainedPerChain);
double retainedIntegrationTimePerChain = retainedPerChain * trajectoryLength;

var packTables = seedPlan.GetProperty("tables").EnumerateArray().Select(table => new
{
    id = table.GetProperty("id").GetString()!,
    seedOffset = table.GetProperty("seedOffset").GetInt32(),
    seeds = table.GetProperty("seeds").EnumerateArray().Select(x => x.GetInt32()).ToArray(),
    initialScales = table.GetProperty("initialScales").EnumerateArray().Select(x => x.GetDouble()).ToArray(),
}).ToArray();
int chainCount = packTables.Sum(x => x.seeds.Length);
var committedSeeds = new List<int>();
foreach (JsonElement table in p548Contract.GetProperty("seedTables").EnumerateArray())
{
    committedSeeds.AddRange(table.GetProperty("seeds").EnumerateArray().Select(x => x.GetInt32()));
    committedSeeds.Add(table.GetProperty("excludedFrozenSeed").GetInt32());
}
foreach (JsonElement property in p571Contract.EnumerateObject()
    .Where(x => x.Name.Contains("momentumSeed", StringComparison.OrdinalIgnoreCase)).Select(x => x.Value))
    if (property.ValueKind == JsonValueKind.Array)
        committedSeeds.AddRange(property.EnumerateArray().Select(x => x.GetInt32()));
    else if (property.ValueKind == JsonValueKind.Number)
        committedSeeds.Add(property.GetInt32());
int[] packSeeds = packTables.SelectMany(x => x.seeds).ToArray();
bool seedsDisjoint = SeedsDisjoint(packSeeds, committedSeeds)
    && packSeeds.Distinct().Count() == packSeeds.Length
    && packSeeds.All(seed => seed is >= 900000 and < 910000);
if (!seedsDisjoint)
{
    Emit(Early(taxonomy[3], true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase576 verdict: {taxonomy[3]}");
    return;
}

long estimatedForceEvaluations = checked((long)chainCount * trajectoriesPerChain * (leapfrogSteps + 1));
long rawSeriesRetentionBytes = checked(2L * chainCount * 18 * retainedPerChain * sizeof(double));
long checkpointBytes = checked((long)chainCount * (trajectoriesPerChain / 250 + 1) * 3645 * sizeof(double));
bool withinCeiling = armMatchesContract
    && estimatedForceEvaluations <= resource.GetProperty("maximumForceEvaluations").GetInt64()
    && rawSeriesRetentionBytes + checkpointBytes <= resource.GetProperty("maximumPeakBytes").GetInt64()
    && retainedIntegrationTimePerChain >= coverageFactor * slowestMode - trajectoryLength;
if (!withinCeiling)
{
    Emit(Early(taxonomy[4], true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase576 verdict: {taxonomy[4]}");
    return;
}

var frozenChainPack = new
{
    packId = "a40-disjoint-seed-chain-pack-v1",
    target = new
    {
        registeredOperator = "EinsteinianShiabOperator SD2/Id0, EinsteinCoefficient 0.5, independent-theta, extent 3, lattice-canonical, su2 trace pairing",
        unchangedFromPhase548 = true,
    },
    proposal = new
    {
        source = "phase571 long arm, independently confirmed under the A39 registered fold convention",
        stepSize, leapfrogSteps, trajectoryLength,
        divergenceAbsoluteDeltaH = divergenceThreshold,
    },
    schedule = new
    {
        chainCount, tables = packTables, warmupPerChain, retainedPerChain, trajectoriesPerChain,
        retainedIntegrationTimePerChain,
        slowestModeTrajectoryLengthEstimate = slowestMode,
        coverageFactor, warmupFraction,
        derivationIsFromCommittedBytesOnly = true,
        pristineSeedBlindPreregistration = true,
    },
    retention = new
    {
        rawDirectionalSeriesRetentionMandatory = true,
        directionalSeriesCount = 18,
        checkpointEveryTrajectories = 250,
        finalCheckpointMandatory = true,
        estimatedRawSeriesBytes = rawSeriesRetentionBytes,
        estimatedCheckpointBytes = checkpointBytes,
    },
    diagnostics = new
    {
        estimator = diagnostics.GetProperty("estimator").GetString(),
        maximumRhat = 1.01, minimumEss = 100.0,
        adjudicationConvention = "A39 registered fold convention (phase575-contract-v1 rowConvention)",
        essFloorFailureIsFirstClassNegative = true,
    },
    resource = new
    {
        estimatedForceEvaluations,
        maximumForceEvaluations = resource.GetProperty("maximumForceEvaluations").GetInt64(),
        maximumPeakBytes = resource.GetProperty("maximumPeakBytes").GetInt64(),
        refusalRule = "the execution phase must refuse before allocation on any ceiling breach",
    },
    executionAuthority = new
    {
        chainPackExecutionAuthorized = false,
        executionRequires = "separately registered prospectively frozen phase in a future registry extension plus explicit written user sampling authorization",
    },
};

var result = new
{
    schemaVersion = 1, phase = 576, phaseId = "phase576-disjoint-seed-chain-pack-design",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath),
    contractValid = true, exactBindingsValid = true, bindings, knownAnswerBattery,
    upstream = new
    {
        gatePassed = true,
        phase548Verdict = p548Summary.GetProperty("verdictKind").GetString(),
        phase571Verdict = p571.GetProperty("verdictKind").GetString(),
        phase575Verdict = p575.GetProperty("verdictKind").GetString(),
        phase575PlanningGateOpen = true,
    },
    derivation = new
    {
        armMatchesContract, slowestModeSourceValue = slowestMode,
        retainedPerChain, warmupPerChain, trajectoriesPerChain, retainedIntegrationTimePerChain,
        seedsDisjoint = true, committedSeedCountChecked = committedSeeds.Distinct().Count(),
        estimatedForceEvaluations, withinCeiling = true,
    },
    frozenChainPack,
    verdictKind = taxonomy[5],
    terminalStatus = "disjoint-seed-chain-pack-design-" + taxonomy[5],
    chainPackExecutionAuthorized = false,
    packTargetsResolutionNotConvergence = true,
    phase571LeverScopeUnchangedLocalMovementOnly = true,
    phase572TerminalAndToleranceNeverRewritten = true,
    newSamplingPerformed = false, replayPerformed = false, rngUsed = false, markovChainAdvanced = false,
    configurationsRetained = false, phase548Or549TerminalChanged = false,
    phase570Or571Or572Reinterpreted = false, phase572ToleranceRelaxed = false,
    registeredBlindSeedTouched = false, protectedPhase554SeedsRead = false, registeredTargetChanged = false,
    quotientApplied = false, gaugeFixingApplied = false, measureNormalizationApplied = false,
    sourceOrModelSelected = false, phase561Opened = false, o4Discharged = false,
    phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};
Emit(result);
Console.WriteLine($"Phase576 verdict: {taxonomy[5]}");
Console.WriteLine($"retainedPerChain={retainedPerChain}");
Console.WriteLine($"estimatedForceEvaluations={estimatedForceEvaluations}");
Console.WriteLine($"chainPackExecutionAuthorized=False");
Console.WriteLine($"promotedPhysicalMassClaimCount=0");

static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
JsonElement ReadBinding(string id) => JsonDocument.Parse(File.ReadAllBytes(PathFor(id))).RootElement.Clone();
string PathFor(string id) => specs.Single(x => x.Id == id).Path;
void Emit(object payload)
{
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(payload, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    File.WriteAllBytes(OutputPath, bytes); File.WriteAllBytes(SummaryPath, bytes);
}
object Early(string verdict, bool valid, bool bindingsValid, object bindingRows, object battery) => new
{
    schemaVersion = 1, phase = 576, phaseId = "phase576-disjoint-seed-chain-pack-design",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid = valid,
    exactBindingsValid = bindingsValid, bindings = bindingRows, knownAnswerBattery = battery,
    upstream = (object?)null, derivation = (object?)null, frozenChainPack = (object?)null,
    verdictKind = verdict, terminalStatus = "disjoint-seed-chain-pack-design-" + verdict,
    chainPackExecutionAuthorized = false,
    packTargetsResolutionNotConvergence = true,
    phase571LeverScopeUnchangedLocalMovementOnly = true,
    phase572TerminalAndToleranceNeverRewritten = true,
    newSamplingPerformed = false, replayPerformed = false, rngUsed = false, markovChainAdvanced = false,
    configurationsRetained = false, phase548Or549TerminalChanged = false,
    phase570Or571Or572Reinterpreted = false, phase572ToleranceRelaxed = false,
    registeredBlindSeedTouched = false, protectedPhase554SeedsRead = false, registeredTargetChanged = false,
    quotientApplied = false, gaugeFixingApplied = false, measureNormalizationApplied = false,
    sourceOrModelSelected = false, phase561Opened = false, o4Discharged = false,
    phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};

sealed record Binding(string Id, string Path, string Hash);
