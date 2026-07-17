using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string Phase508Path = "studies/phase508_phase481_acquisition_geometry_closure_001/output/phase481_acquisition_geometry_closure_summary.json";
const string Phase509Path = "studies/phase509_phase481_anisotropic_cpu_reference_feasibility_001/output/phase481_anisotropic_cpu_reference_feasibility_summary.json";
const string Phase489Path = "studies/phase489_reduced_sampler_restart_equivalence_001/output/reduced_sampler_restart_equivalence_summary.json";
const string Phase481PlanPath = "studies/phase481_phase456_prospective_repair_preregistration_001/planning/phase481_pack_construction_plan_v1.json";
const string ContractPath = "studies/phase510_phase481_execution_readiness_adjudicator_001/preregistration/phase510_execution_readiness_contract_v1.json";
const string OutputDir = "studies/phase510_phase481_execution_readiness_adjudicator_001/output";

using var contractDoc = JsonDocument.Parse(File.ReadAllText(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement expected = contract.GetProperty("expectedBindings");
var bindings = new[]
{
    Bind("phase508", Phase508Path, expected.GetProperty("phase508Sha256").GetString()!),
    Bind("phase509", Phase509Path, expected.GetProperty("phase509Sha256").GetString()!),
    Bind("phase489", Phase489Path, expected.GetProperty("phase489Sha256").GetString()!),
    Bind("phase481-plan", Phase481PlanPath, expected.GetProperty("phase481PlanSha256").GetString()!),
};

bool precursorSchemasValid = bindings.All(x => x.HashMatches)
    && JsonBool(Phase508Path, "inputsValid") && JsonString(Phase508Path, "verdictKind") == "spatial-four-temporal-design-selected"
    && JsonBool(Phase509Path, "inputsValid") && JsonString(Phase509Path, "verdictKind") == "anisotropic-cpu-reference-controls-ready"
    && JsonBool(Phase509Path, "allControlsPassed")
    && JsonNestedBool(Phase489Path, "restartEquality", "restartEqualityAll")
    && JsonBool(Phase481PlanPath, "planOnly") && !JsonBool(Phase481PlanPath, "phase481PackCreated");

ulong[] seeds = [5021601, 5021602, 5021603, 5021604];
string[][] expectedStates =
[
    ["149bc7cb02b09168", "078b33f76afbbfd8", "e03b00a74da3adac", "3889d1f9d2ecb045"],
    ["33597891ce6329d6", "e10951d8a96c8c6e", "014ca3b1f9266cfe", "7c23b5efa86f9f42"],
    ["8dfd97ef2eb0a62c", "c3bf3b1d8c92e782", "2d24e38f44685270", "d0bb382501db174a"],
    ["860d58d7cc698d52", "82caa059b350d832", "76a828107acd1e65", "ab401bc46ecd4a52"],
];
var rngKnownAnswers = seeds.Select((seed, i) =>
{
    RngState state = ExpandSeed(seed);
    string[] actual = state.Words.Select(x => x.ToString("x16")).ToArray();
    return new { chainId = i, seed, expected = expectedStates[i], actual, passed = actual.SequenceEqual(expectedStates[i]) };
}).ToArray();

var restartTests = seeds.Select((seed, chainId) =>
{
    var uninterrupted = new Xoshiro(ExpandSeed(seed));
    ulong[] expectedStream = Enumerable.Range(0, 96).Select(_ => uninterrupted.Next()).ToArray();
    var split = new Xoshiro(ExpandSeed(seed));
    ulong[] prefix = Enumerable.Range(0, 37).Select(_ => split.Next()).ToArray();
    byte[] checkpoint = SerializeCheckpoint(chainId, 37, split.State);
    var restored = new Xoshiro(DeserializeCheckpoint(checkpoint, chainId, 37));
    ulong[] suffix = Enumerable.Range(37, 59).Select(_ => restored.Next()).ToArray();
    bool passed = prefix.Concat(suffix).SequenceEqual(expectedStream);
    return new { chainId, checkpointSha256 = Sha(checkpoint), passed };
}).ToArray();

var resources = new[] { Resource(16, 8192), Resource(32, 16384) };
bool resourceArithmeticValid = resources[0].Sites == 1024 && resources[0].Edges == 15360 && resources[0].Faces == 51200
    && resources[0].MaximumRawBytes == 26843545600L && resources[1].Sites == 2048
    && resources[1].Edges == 30720 && resources[1].Faces == 102400 && resources[1].MaximumRawBytes == 107374182400L;

var baseline = new FingerprintSurface("4x4x4x16", "xoshiro256**-v1", "5021601,5021602,5021603,5021604", 4,
    "float64-cpu-reference-fixed-order", "site-face-type-channel-v1", 1.01, 2048, 1209600, 1.25, "phase508|phase509|phase489|phase481-plan");
var mutations = new[]
{
    Mutate("geometry", baseline with { Geometry = "16x16x16x16" }),
    Mutate("rng-algorithm", baseline with { Rng = "System.Random" }),
    Mutate("seed", baseline with { Seeds = "5021601,5021602,5021603,0" }),
    Mutate("chain-count", baseline with { Chains = 3 }),
    Mutate("precision-backend", baseline with { Backend = "float32-gpu" }),
    Mutate("retained-schema", baseline with { RetainedSchema = "site-channel-v1" }),
    Mutate("split-rhat", baseline with { SplitRhat = 1.02 }),
    Mutate("ess", baseline with { Ess = 2047 }),
    Mutate("cost-ceiling", baseline with { CpuSecondCeiling = 1209601 }),
    Mutate("forecast-factor", baseline with { ForecastFactor = 1.2 }),
    Mutate("precursor-membership", baseline with { Precursors = "phase508|phase509|phase481-plan" }),
};
bool prospectiveMutationDesignPassed = mutations.All(x => x.FingerprintMismatchDetected);

string[] precedence = contract.GetProperty("precedence").EnumerateArray().Select(x => x.GetString()!).ToArray();
string Decide(bool invalid, bool rngInvalid, bool resourceInvalid, bool mutationFailure, bool costFailure) =>
    invalid ? precedence[0] : rngInvalid ? precedence[1] : resourceInvalid ? precedence[2]
    : mutationFailure ? precedence[3] : costFailure ? precedence[4] : precedence[5];
var precedenceBattery = new[]
{
    (id: "invalid-dominates", actual: Decide(true, true, true, true, true), expected: precedence[0]),
    (id: "rng-dominates-lower", actual: Decide(false, true, true, true, true), expected: precedence[1]),
    (id: "resource-dominates-lower", actual: Decide(false, false, true, true, true), expected: precedence[2]),
    (id: "mutation-dominates-cost", actual: Decide(false, false, false, true, true), expected: precedence[3]),
    (id: "cost-failure", actual: Decide(false, false, false, false, true), expected: precedence[4]),
    (id: "ready", actual: Decide(false, false, false, false, false), expected: precedence[5]),
}.Select(x => new { x.id, x.actual, x.expected, passed = x.actual == x.expected }).ToArray();

bool rngReducedControlsReady = rngKnownAnswers.All(x => x.passed) && restartTests.All(x => x.passed);
bool inputsValid = precursorSchemasValid;
bool admissibleThroughputEvidencePresent = false;
bool costEnvelopeReady = admissibleThroughputEvidencePresent;
string verdict = Decide(!inputsValid, !rngReducedControlsReady, !resourceArithmeticValid, !prospectiveMutationDesignPassed, !costEnvelopeReady);
bool technicalReady = verdict == "preconstruction-controls-ready-pending-phase480";

var result = new
{
    phaseId = "phase510-phase481-execution-readiness-adjudicator",
    terminalStatus = $"phase481-execution-readiness-adjudicator-{verdict}",
    verdictKind = verdict,
    inputsValid,
    technicalReadyForLaterPackConstruction = technicalReady,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A14",
    contract = new { path = ContractPath, sha256 = Sha(File.ReadAllBytes(ContractPath)), frozenBeforePrecursorConsumption = true },
    exactInputBindings = bindings,
    rngContract = new
    {
        algorithm = "xoshiro256**-v1", seedExpansion = "splitmix64-four-sequential-outputs",
        chainSeeds = seeds, oneRngPerChain = true, crossChainStateSharingAllowed = false,
        checkpointEncoding = "versioned-canonical-little-endian-binary-ieee754-sha256",
        uniformGeneration = "((next-u64-right-shift-11)+0.5)/2^53",
        normalGeneration = "box-muller-two-uniforms-no-spare-cache",
        acceptanceDrawRule = "always-consume-one-uniform-per-proposal",
        rngKnownAnswers, restartTests, knownAnswerPassed = rngKnownAnswers.All(x => x.passed),
        reducedRngStateRestartExact = restartTests.All(x => x.passed),
        reducedControlOnly = true, productionHmcCheckpointCodecImplemented = false,
    },
    retentionAndResources = new
    {
        schema = "one-lossless-double-per-configuration-site-face-type-channel",
        temporalSeparationsReconstructedNotDuplicated = true, faceTypes = 50, channels = 2,
        resources, resourceArithmeticValid, compressionMayReduceRefusalBound = false,
        prelaunchFreeSpaceFactor = 1.25, fullT16OrT32AllocationPerformed = false,
    },
    costRefusal = new
    {
        cpuWeekCeiling = 2, aggregateProcessCpuSecondCeiling = 1209600,
        forecastSafetyFactor = 1.25, checkpointConfigurations = 4096, nextCheckpointIndivisible = true,
        refusalFormula = "measuredCpuSeconds + 1.25 * nextCheckpointCpuForecast > 1209600",
        wallClockSubstitutionAllowed = false, missingOrNonpositiveThroughputAction = "refuse",
        admissibleThroughputEvidencePresent, costEnvelopeReady,
    },
    prospectiveFingerprintMutationDesign = new
    {
        baselineSha256 = Fingerprint(baseline), cases = mutations, caseCount = mutations.Length,
        declaredCasesDistinguishable = prospectiveMutationDesignPassed,
        productionPackLoaderExists = false, productionRefusalProved = false,
        productionRefusalProofDeferredUntilPackConstructionStepP4 = true,
    },
    precedenceBattery = new { cases = precedenceBattery, caseCount = precedenceBattery.Length, passed = precedenceBattery.All(x => x.passed) },
    runtimeTaxonomyPreserved = contract.GetProperty("runtimeTaxonomyPreserved").EnumerateArray().Select(x => x.GetString()).ToArray(),
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    phase480Satisfied = false,
    phase481PackCreated = false,
    phase481PackMutated = false,
    samplingOrReprocessingRun = false,
    hmcRun = false,
    benchmarkRun = false,
    productionAuthorized = false,
    phase458G3Satisfied = false,
    phase458G5Satisfied = false,
    o4Discharged = false,
    sourceContractApplicationAllowed = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    a14BoundaryHeld = true,
    adversarialReviewAccepted = true,
    initialPrematureReadyTerminalRejected = true,
    decision = technicalReady
        ? "Geometry, reduced anisotropic CPU controls, portable RNG/restart semantics, retention arithmetic, cost refusal, and fingerprint mutation controls are ready for later independent Phase481 pack construction. Phase480 and an exact future pack remain mandatory; no run is authorized."
        : verdict == "cost-envelope-failure"
            ? "Geometry and reduced anisotropic controls are ready, and the RNG, retention, cost, and future mutation contracts are frozen. No admissible production-throughput evidence exists, so the cost envelope fails closed before pack construction; Phase480 and every launch gate also remain closed."
            : "The preconstruction controls remain fail-closed at the first invalid or incomplete requirement; no pack or run is authorized.",
};

Directory.CreateDirectory(OutputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(OutputDir, "phase481_execution_readiness_adjudicator.json"), json);
File.WriteAllText(Path.Combine(OutputDir, "phase481_execution_readiness_adjudicator_summary.json"), json);
Console.WriteLine(result.terminalStatus);
Console.WriteLine($"inputs={inputsValid} rngReduced={rngReducedControlsReady} resources={resourceArithmeticValid} mutationDesign={prospectiveMutationDesignPassed} cost={costEnvelopeReady} ready={technicalReady}");

static Binding Bind(string id, string path, string expected)
{
    string actual = File.Exists(path) ? Sha(File.ReadAllBytes(path)) : "missing";
    return new Binding(id, path, expected, actual, actual == expected);
}

static bool JsonBool(string path, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty(property).GetBoolean();
}

static string JsonString(string path, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty(property).GetString() ?? string.Empty;
}

static bool JsonNestedBool(string path, string parent, string property)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty(parent).GetProperty(property).GetBoolean();
}

static ResourceRow Resource(int temporalExtent, int perChainConfigurations)
{
    long sites = 64L * temporalExtent;
    long allChains = 4L * perChainConfigurations;
    return new ResourceRow(temporalExtent, sites, 15L * sites, 50L * sites,
        45L * sites, 3L * sites, allChains, checked(100L * sites * sizeof(double) * allChains),
        checked(531L * sites * sizeof(double)));
}

static RngState ExpandSeed(ulong seed)
{
    ulong state = seed;
    ulong Next()
    {
        state += 0x9E3779B97F4A7C15UL;
        ulong z = state;
        z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
        z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
        return z ^ (z >> 31);
    }
    return new RngState(Next(), Next(), Next(), Next());
}

static byte[] SerializeCheckpoint(int chainId, long drawCount, RngState state)
{
    byte[] bytes = new byte[56];
    Encoding.ASCII.GetBytes("P510RNG1").CopyTo(bytes, 0);
    BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(8), chainId);
    BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(12), 1);
    BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(16), drawCount);
    for (int i = 0; i < 4; i++) BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(24 + 8 * i), state.Words[i]);
    return bytes;
}

static RngState DeserializeCheckpoint(byte[] bytes, int chainId, long drawCount)
{
    if (bytes.Length != 56 || Encoding.ASCII.GetString(bytes, 0, 8) != "P510RNG1"
        || BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(8)) != chainId
        || BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(12)) != 1
        || BinaryPrimitives.ReadInt64LittleEndian(bytes.AsSpan(16)) != drawCount)
        throw new InvalidOperationException("checkpoint identity mismatch");
    ulong[] words = Enumerable.Range(0, 4).Select(i => BinaryPrimitives.ReadUInt64LittleEndian(bytes.AsSpan(24 + 8 * i))).ToArray();
    if (words.All(x => x == 0)) throw new InvalidOperationException("all-zero RNG state");
    return new RngState(words[0], words[1], words[2], words[3]);
}

static MutationResult Mutate(string id, FingerprintSurface mutated)
{
    var baseline = new FingerprintSurface("4x4x4x16", "xoshiro256**-v1", "5021601,5021602,5021603,5021604", 4,
        "float64-cpu-reference-fixed-order", "site-face-type-channel-v1", 1.01, 2048, 1209600, 1.25, "phase508|phase509|phase489|phase481-plan");
    return new MutationResult(id, Fingerprint(baseline), Fingerprint(mutated), Fingerprint(baseline) != Fingerprint(mutated));
}

static string Fingerprint(FingerprintSurface value) => Sha(JsonSerializer.SerializeToUtf8Bytes(value));
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record ResourceRow(int TemporalExtent, long Sites, long Edges, long Faces, long OmegaDoubles, long ThetaDoubles,
    long MaximumConfigurationsAllChains, long MaximumRawBytes, long LiveArrayLowerBoundBytes);
sealed record FingerprintSurface(string Geometry, string Rng, string Seeds, int Chains, string Backend, string RetainedSchema,
    double SplitRhat, int Ess, long CpuSecondCeiling, double ForecastFactor, string Precursors);
sealed record MutationResult(string Id, string BaselineSha256, string MutatedSha256, bool FingerprintMismatchDetected);
readonly record struct RngState(ulong S0, ulong S1, ulong S2, ulong S3)
{
    public ulong[] Words => [S0, S1, S2, S3];
}
sealed class Xoshiro(RngState state)
{
    private ulong _s0 = state.S0, _s1 = state.S1, _s2 = state.S2, _s3 = state.S3;
    public RngState State => new(_s0, _s1, _s2, _s3);
    public ulong Next()
    {
        ulong result = RotateLeft(_s1 * 5, 7) * 9;
        ulong t = _s1 << 17;
        _s2 ^= _s0; _s3 ^= _s1; _s1 ^= _s2; _s0 ^= _s3; _s2 ^= t; _s3 = RotateLeft(_s3, 45);
        return result;
    }
    private static ulong RotateLeft(ulong x, int k) => (x << k) | (x >> (64 - k));
}
