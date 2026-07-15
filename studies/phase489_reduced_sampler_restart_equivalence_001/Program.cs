using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

const string OutputDir = "studies/phase489_reduced_sampler_restart_equivalence_001/output";
const string Slug = "reduced_sampler_restart_equivalence";
const int TotalSteps = 120_000;
const int BurnInSteps = 20_000;
const int SplitStep = 47_321;
const int BatchCount = 50;
const double LocalAngleHalfWidth = 1.4;
const double TargetStrength = 1.25;
const double CrossProposalAbsoluteTolerance = 0.04;
const ulong BaseSeed = 0x489A6C0FFEE12345UL;
const string Phase487SummaryPath = "studies/phase487_independent_so3_haar_measure_control_001/output/independent_so3_haar_measure_control_summary.json";
const string Phase488SummaryPath = "studies/phase488_haar_proposal_invariance_control_001/output/haar_proposal_invariance_control_summary.json";

using var phase487 = JsonDocument.Parse(File.ReadAllText(Phase487SummaryPath));
using var phase488 = JsonDocument.Parse(File.ReadAllText(Phase488SummaryPath));
bool phase487PrecursorPassed = phase487.RootElement.GetProperty("allBatteriesPassed").GetBoolean()
    && !phase487.RootElement.GetProperty("o4Discharged").GetBoolean()
    && phase487.RootElement.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
bool phase488PrecursorPassed = phase488.RootElement.GetProperty("proposalInvarianceControlPassed").GetBoolean()
    && phase488.RootElement.GetProperty("phase487PrecursorPassed").GetBoolean()
    && !phase488.RootElement.GetProperty("o4Discharged").GetBoolean()
    && phase488.RootElement.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
bool precursorsPassed = phase487PrecursorPassed && phase488PrecursorPassed;

var specifications = new[]
{
    new ProposalSpecification("uniform-independence", BaseSeed + 0x101UL, ProposalKind.UniformIndependence),
    new ProposalSpecification("left-local-composition", BaseSeed + 0x202UL, ProposalKind.LeftLocalComposition),
    new ProposalSpecification("right-local-composition", BaseSeed + 0x303UL, ProposalKind.RightLocalComposition),
};

var comparisons = new List<object>();
var results = new List<ProposalResult>();
bool restartEqualityAll = true;
bool controlsValid = precursorsPassed && TotalSteps > BurnInSteps && SplitStep > 0 && SplitStep < TotalSteps &&
    (TotalSteps - BurnInSteps) % BatchCount == 0 && LocalAngleHalfWidth > 0.0 &&
    LocalAngleHalfWidth < System.Math.PI && CrossProposalAbsoluteTolerance > 0.0;

foreach (var specification in specifications)
{
    var uninterrupted = RunUninterrupted(specification);
    var restarted = RunWithRestart(specification);
    bool rngExact = uninterrupted.FinalSnapshot.Rng.Equals(restarted.FinalSnapshot.Rng);
    bool sequenceExact = uninterrupted.AcceptanceSequence.SequenceEqual(restarted.AcceptanceSequence);
    bool stateExact = uninterrupted.FinalSnapshot.State.Equals(restarted.FinalSnapshot.State);
    bool countersExact = uninterrupted.FinalSnapshot.Proposed == restarted.FinalSnapshot.Proposed &&
        uninterrupted.FinalSnapshot.Accepted == restarted.FinalSnapshot.Accepted &&
        uninterrupted.FinalSnapshot.Measured == restarted.FinalSnapshot.Measured;
    bool accumulatorsExact = uninterrupted.FinalSnapshot.Accumulators.Equals(restarted.FinalSnapshot.Accumulators);
    bool observablesExact = uninterrupted.Observables.Equals(restarted.Observables);
    bool checksumExact = uninterrupted.StateChecksum == restarted.StateChecksum;
    bool acceptanceHashExact = uninterrupted.AcceptanceSequenceSha256 == restarted.AcceptanceSequenceSha256;
    bool restartExact = rngExact && sequenceExact && stateExact && countersExact && accumulatorsExact &&
        observablesExact && checksumExact && acceptanceHashExact;
    restartEqualityAll &= restartExact;

    comparisons.Add(new
    {
        proposalFamily = specification.Id,
        restartExact,
        rngExact,
        sequenceExact,
        stateExact,
        countersExact,
        accumulatorsExact,
        observablesExact,
        checksumExact,
        acceptanceHashExact,
        serializedCheckpointSha256 = restarted.SerializedCheckpointSha256,
        uninterruptedFinalRng = uninterrupted.FinalSnapshot.Rng,
        resumedFinalRng = restarted.FinalSnapshot.Rng,
        uninterruptedAcceptanceSequenceSha256 = uninterrupted.AcceptanceSequenceSha256,
        resumedAcceptanceSequenceSha256 = restarted.AcceptanceSequenceSha256,
        uninterruptedStateChecksum = uninterrupted.StateChecksum,
        resumedStateChecksum = restarted.StateChecksum,
    });

    results.Add(new ProposalResult(
        specification.Id,
        uninterrupted.FinalSnapshot.Proposed,
        uninterrupted.FinalSnapshot.Accepted,
        uninterrupted.FinalSnapshot.Accepted / (double)uninterrupted.FinalSnapshot.Proposed,
        uninterrupted.Observables,
        uninterrupted.BatchStandardErrors,
        uninterrupted.StateChecksum,
        uninterrupted.AcceptanceSequenceSha256,
        restartExact));
}

var pairwise = new List<object>();
bool crossProposalAgreement = true;
for (int i = 0; i < results.Count; i++)
{
    for (int j = i + 1; j < results.Count; j++)
    {
        var a = results[i];
        var b = results[j];
        var checks = new[]
        {
            CompareObservable("rotationTraceOverThree", a.Observables.RotationTraceOverThree, b.Observables.RotationTraceOverThree),
            CompareObservable("axisBlindQuadratic", a.Observables.AxisBlindQuadratic, b.Observables.AxisBlindQuadratic),
            CompareObservable("normalizedAction", a.Observables.NormalizedAction, b.Observables.NormalizedAction),
        };
        bool pairPassed = checks.All(x => x.Passed);
        crossProposalAgreement &= pairPassed;
        pairwise.Add(new { proposalA = a.ProposalFamily, proposalB = b.ProposalFamily, pairPassed, checks });
    }
}

bool samplerCounterControls = results.All(x => x.Proposed == TotalSteps && x.Accepted > 0 && x.Accepted < x.Proposed);
controlsValid &= samplerCounterControls;
string verdictKind = !controlsValid ? "reduced-control-invalid"
    : !restartEqualityAll ? "restart-equivalence-failed"
    : !crossProposalAgreement ? "cross-proposal-statistical-disagreement"
    : "restart-bit-exact-and-cross-proposal-agreement";

var output = new
{
    phaseId = "phase489-reduced-sampler-restart-equivalence",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus = $"reduced-sampler-restart-equivalence-{verdictKind}",
    verdictKind,
    applicationSubjectKind = "reduced-sampler-restart-equivalence-control",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A6; CONVENTION_ROBUSTNESS_TRANCHE_PLAN_2026-07-15 Phase489",
    precursorBindings = new { phase487SummaryPath = Phase487SummaryPath, phase488SummaryPath = Phase488SummaryPath, phase487PrecursorPassed, phase488PrecursorPassed, precursorsPassed },
    explorationLane = true,
    targetBlindConstruction = true,
    physicalTargetValuesConsulted = false,
    physicalTargetsConsultedForConstruction = false,
    frozenSpecification = new
    {
        totalSteps = TotalSteps,
        burnInSteps = BurnInSteps,
        splitStep = SplitStep,
        batchCount = BatchCount,
        localAngleHalfWidth = LocalAngleHalfWidth,
        targetStrength = TargetStrength,
        crossProposalAbsoluteTolerance = CrossProposalAbsoluteTolerance,
        baseSeedHex = $"0x{BaseSeed:x16}",
        rngAlgorithm = "xoshiro256starstar-v1",
        stateRepresentation = "canonical unit quaternion with nonnegative scalar component",
        reducedTarget = "exp(-targetStrength*(1-q0^2)); bounded synthetic rotation control",
        proposalFamilies = specifications.Select(x => new { x.Id, seedHex = $"0x{x.Seed:x16}", kind = x.Kind.ToString() }).ToArray(),
        observables = new[] { "rotationTraceOverThree", "axisBlindQuadratic", "normalizedAction" },
    },
    controlsValid,
    samplerCounterControls,
    restartEquality = new
    {
        bitExactRequired = true,
        restartEqualityAll,
        comparisonMeaning = "Exact equality after JSON serialization/resume of quaternion bits, all four RNG words, counters, accumulated sums, accept/reject sequence, checksums, and observables.",
        comparisons,
    },
    crossProposalComparison = new
    {
        statisticalOnly = true,
        crossProposalAgreement,
        tolerance = CrossProposalAbsoluteTolerance,
        comparisonMeaning = "Independent proposal families need statistical agreement on bounded synthetic observables; bit equality across different proposal families is neither expected nor claimed.",
        pairwise,
    },
    proposalResults = results,
    reducedDeterministicWorkload = true,
    nonphysicalBoundedObservablesOnly = true,
    humanRulingAuthored = false,
    o4Discharged = false,
    satisfiesPhase458G4 = false,
    satisfiesPhase458G5 = false,
    phase458EvaluationAuthorized = false,
    binderLaunchAuthorized = false,
    productionAuthorized = false,
    sourceContractApplicationAllowed = false,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = verdictKind == "restart-bit-exact-and-cross-proposal-agreement"
        ? "Reduced exploration control passed: each proposal family resumed bit-exactly and the independent families agree statistically on bounded synthetic observables. This does not validate a production sampler or discharge any scientific gate."
        : "Reduced exploration control failed closed. The exact restart and statistical proposal-agreement branches remain separately recorded; no downstream gate or production action is authorized.",
};

Directory.CreateDirectory(OutputDir);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(output, jsonOptions);
File.WriteAllText(Path.Combine(OutputDir, $"{Slug}.json"), json);
File.WriteAllText(Path.Combine(OutputDir, $"{Slug}_summary.json"), json);
Console.WriteLine(output.terminalStatus);
Console.WriteLine($"restartExact={restartEqualityAll} crossProposalAgreement={crossProposalAgreement} promotedPhysicalMassClaimCount=0");

ObservableCheck CompareObservable(string name, double a, double b)
{
    double difference = System.Math.Abs(a - b);
    return new ObservableCheck(name, a, b, difference, CrossProposalAbsoluteTolerance,
        difference <= CrossProposalAbsoluteTolerance);
}

RunRecord RunUninterrupted(ProposalSpecification specification)
{
    var chain = ChainState.Create(specification.Seed);
    Advance(chain, specification, TotalSteps);
    return Finish(chain, null);
}

RunRecord RunWithRestart(ProposalSpecification specification)
{
    var chain = ChainState.Create(specification.Seed);
    Advance(chain, specification, SplitStep);
    var checkpoint = chain.ToSnapshot();
    string serialized = JsonSerializer.Serialize(checkpoint);
    string serializedSha = Sha256Text(serialized);
    var restoredSnapshot = JsonSerializer.Deserialize<ChainSnapshot>(serialized)
        ?? throw new InvalidOperationException("Serialized checkpoint did not deserialize.");
    var restored = ChainState.FromSnapshot(restoredSnapshot);
    Advance(restored, specification, TotalSteps - SplitStep);
    return Finish(restored, serializedSha);
}

void Advance(ChainState chain, ProposalSpecification specification, int count)
{
    for (int iteration = 0; iteration < count; iteration++)
    {
        var proposal = specification.Kind switch
        {
            ProposalKind.UniformIndependence => UniformRotation(chain.Rng),
            ProposalKind.LeftLocalComposition => Quaternion.Multiply(LocalIncrement(chain.Rng), chain.State),
            ProposalKind.RightLocalComposition => Quaternion.Multiply(chain.State, LocalIncrement(chain.Rng)),
            _ => throw new InvalidOperationException("Unknown proposal family."),
        };
        proposal = proposal.NormalizedCanonical();
        double logRatio = -Action(proposal) + Action(chain.State);
        bool accepted = logRatio >= 0.0 || System.Math.Log(chain.Rng.NextOpenUnit()) < logRatio;
        chain.AcceptanceSequence.Add(accepted);
        chain.Proposed++;
        if (accepted)
        {
            chain.State = proposal;
            chain.Accepted++;
        }

        if (chain.Proposed > BurnInSteps)
        {
            var observables = Observe(chain.State);
            chain.Accumulators = chain.Accumulators.Add(observables);
            int measuredIndex = chain.Measured;
            int batch = measuredIndex * BatchCount / (TotalSteps - BurnInSteps);
            chain.BatchSums[batch] = chain.BatchSums[batch].Add(observables);
            chain.BatchCounts[batch]++;
            chain.Measured++;
        }
    }
}

RunRecord Finish(ChainState chain, string? serializedCheckpointSha256)
{
    if (chain.Proposed != TotalSteps || chain.Measured != TotalSteps - BurnInSteps)
        throw new InvalidOperationException("Reduced sampler counters drifted from the frozen specification.");
    var observableMeans = chain.Accumulators.Divide(chain.Measured);
    var batchMeans = chain.BatchSums.Select((sum, index) => sum.Divide(chain.BatchCounts[index])).ToArray();
    var standardErrors = ObservableVector.StandardErrors(batchMeans);
    byte[] packedSequence = PackBits(chain.AcceptanceSequence);
    string sequenceHash = Sha256Bytes(packedSequence);
    string stateChecksum = StateChecksum(chain.ToSnapshot());
    return new RunRecord(chain.ToSnapshot(), chain.AcceptanceSequence.ToArray(), observableMeans,
        standardErrors, stateChecksum, sequenceHash, serializedCheckpointSha256);
}

double Action(Quaternion q) => TargetStrength * (1.0 - q.W * q.W);

ObservableVector Observe(Quaternion q)
{
    double w2 = q.W * q.W;
    return new ObservableVector((4.0 * w2 - 1.0) / 3.0, 4.0 * w2 * (1.0 - w2), 1.0 - w2);
}

Quaternion UniformRotation(DeterministicRng rng)
{
    double u1 = rng.NextOpenUnit();
    double u2 = rng.NextOpenUnit();
    double u3 = rng.NextOpenUnit();
    double r1 = System.Math.Sqrt(1.0 - u1);
    double r2 = System.Math.Sqrt(u1);
    double a = 2.0 * System.Math.PI * u2;
    double b = 2.0 * System.Math.PI * u3;
    return new Quaternion(r2 * System.Math.Cos(b), r1 * System.Math.Sin(a),
        r1 * System.Math.Cos(a), r2 * System.Math.Sin(b)).NormalizedCanonical();
}

Quaternion LocalIncrement(DeterministicRng rng)
{
    double z = 2.0 * rng.NextOpenUnit() - 1.0;
    double phi = 2.0 * System.Math.PI * rng.NextOpenUnit();
    double radius = System.Math.Sqrt(System.Math.Max(0.0, 1.0 - z * z));
    double angle = LocalAngleHalfWidth * (2.0 * rng.NextOpenUnit() - 1.0);
    double half = 0.5 * angle;
    double s = System.Math.Sin(half);
    return new Quaternion(System.Math.Cos(half), s * radius * System.Math.Cos(phi),
        s * radius * System.Math.Sin(phi), s * z).NormalizedCanonical();
}

static byte[] PackBits(IReadOnlyList<bool> values)
{
    var bytes = new byte[(values.Count + 7) / 8];
    for (int i = 0; i < values.Count; i++) if (values[i]) bytes[i >> 3] |= (byte)(1 << (i & 7));
    return bytes;
}

static string StateChecksum(ChainSnapshot snapshot)
{
    var bytes = new byte[8 * 14];
    int offset = 0;
    void Put(ulong value) { BinaryPrimitives.WriteUInt64LittleEndian(bytes.AsSpan(offset), value); offset += 8; }
    Put((ulong)BitConverter.DoubleToInt64Bits(snapshot.State.W));
    Put((ulong)BitConverter.DoubleToInt64Bits(snapshot.State.X));
    Put((ulong)BitConverter.DoubleToInt64Bits(snapshot.State.Y));
    Put((ulong)BitConverter.DoubleToInt64Bits(snapshot.State.Z));
    Put(snapshot.Rng.S0); Put(snapshot.Rng.S1); Put(snapshot.Rng.S2); Put(snapshot.Rng.S3);
    Put((ulong)snapshot.Proposed); Put((ulong)snapshot.Accepted); Put((ulong)snapshot.Measured);
    Put((ulong)BitConverter.DoubleToInt64Bits(snapshot.Accumulators.RotationTraceOverThree));
    Put((ulong)BitConverter.DoubleToInt64Bits(snapshot.Accumulators.AxisBlindQuadratic));
    Put((ulong)BitConverter.DoubleToInt64Bits(snapshot.Accumulators.NormalizedAction));
    return Sha256Bytes(bytes[..offset]);
}

static string Sha256Text(string text) => Sha256Bytes(Encoding.UTF8.GetBytes(text));
static string Sha256Bytes(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

enum ProposalKind { UniformIndependence, LeftLocalComposition, RightLocalComposition }

sealed record ProposalSpecification(string Id, ulong Seed, ProposalKind Kind);
sealed record ObservableCheck(string Observable, double A, double B, double AbsoluteDifference, double Tolerance, bool Passed);
sealed record ProposalResult(string ProposalFamily, long Proposed, long Accepted, double AcceptanceRate,
    ObservableVector Observables, ObservableVector BatchStandardErrors, string StateChecksum,
    string AcceptanceSequenceSha256, bool RestartExact);
sealed record RunRecord(ChainSnapshot FinalSnapshot, bool[] AcceptanceSequence, ObservableVector Observables,
    ObservableVector BatchStandardErrors, string StateChecksum, string AcceptanceSequenceSha256,
    string? SerializedCheckpointSha256);

readonly record struct Quaternion(double W, double X, double Y, double Z)
{
    public static Quaternion Identity => new(1.0, 0.0, 0.0, 0.0);

    public Quaternion NormalizedCanonical()
    {
        double norm = System.Math.Sqrt(W * W + X * X + Y * Y + Z * Z);
        if (!(norm > 0.0) || !double.IsFinite(norm)) throw new InvalidOperationException("Invalid quaternion norm.");
        double sign = W < 0.0 ? -1.0 : 1.0;
        return new Quaternion(sign * W / norm, sign * X / norm, sign * Y / norm, sign * Z / norm);
    }

    public static Quaternion Multiply(Quaternion a, Quaternion b) => new(
        a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
        a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
        a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
        a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W);
}

readonly record struct ObservableVector(double RotationTraceOverThree, double AxisBlindQuadratic, double NormalizedAction)
{
    public static ObservableVector Zero => new(0.0, 0.0, 0.0);
    public ObservableVector Add(ObservableVector other) => new(
        RotationTraceOverThree + other.RotationTraceOverThree,
        AxisBlindQuadratic + other.AxisBlindQuadratic,
        NormalizedAction + other.NormalizedAction);
    public ObservableVector Divide(long divisor) => new(
        RotationTraceOverThree / divisor, AxisBlindQuadratic / divisor, NormalizedAction / divisor);

    public static ObservableVector StandardErrors(IReadOnlyList<ObservableVector> batches)
    {
        var mean = batches.Aggregate(Zero, (sum, item) => sum.Add(item)).Divide(batches.Count);
        double Se(Func<ObservableVector, double> select)
        {
            double m = select(mean);
            double variance = batches.Sum(x => System.Math.Pow(select(x) - m, 2)) / (batches.Count - 1);
            return System.Math.Sqrt(variance / batches.Count);
        }
        return new ObservableVector(Se(x => x.RotationTraceOverThree), Se(x => x.AxisBlindQuadratic),
            Se(x => x.NormalizedAction));
    }
}

readonly record struct RngSnapshot(ulong S0, ulong S1, ulong S2, ulong S3);

sealed record ChainSnapshot(Quaternion State, RngSnapshot Rng, long Proposed, long Accepted, int Measured,
    ObservableVector Accumulators, ObservableVector[] BatchSums, int[] BatchCounts, bool[] AcceptanceSequence);

sealed class ChainState
{
    public required Quaternion State { get; set; }
    public required DeterministicRng Rng { get; init; }
    public long Proposed { get; set; }
    public long Accepted { get; set; }
    public int Measured { get; set; }
    public ObservableVector Accumulators { get; set; }
    public required ObservableVector[] BatchSums { get; init; }
    public required int[] BatchCounts { get; init; }
    public required List<bool> AcceptanceSequence { get; init; }

    public static ChainState Create(ulong seed) => new()
    {
        State = Quaternion.Identity,
        Rng = DeterministicRng.FromSeed(seed),
        BatchSums = Enumerable.Repeat(ObservableVector.Zero, 50).ToArray(),
        BatchCounts = new int[50],
        AcceptanceSequence = new List<bool>(120_000),
    };

    public ChainSnapshot ToSnapshot() => new(State, Rng.Snapshot(), Proposed, Accepted, Measured, Accumulators,
        BatchSums.ToArray(), BatchCounts.ToArray(), AcceptanceSequence.ToArray());

    public static ChainState FromSnapshot(ChainSnapshot snapshot) => new()
    {
        State = snapshot.State,
        Rng = DeterministicRng.FromSnapshot(snapshot.Rng),
        Proposed = snapshot.Proposed,
        Accepted = snapshot.Accepted,
        Measured = snapshot.Measured,
        Accumulators = snapshot.Accumulators,
        BatchSums = snapshot.BatchSums.ToArray(),
        BatchCounts = snapshot.BatchCounts.ToArray(),
        AcceptanceSequence = snapshot.AcceptanceSequence.ToList(),
    };
}

sealed class DeterministicRng
{
    private ulong _s0;
    private ulong _s1;
    private ulong _s2;
    private ulong _s3;

    private DeterministicRng(RngSnapshot state)
    {
        if ((state.S0 | state.S1 | state.S2 | state.S3) == 0) throw new ArgumentException("RNG state cannot be all zero.");
        (_s0, _s1, _s2, _s3) = (state.S0, state.S1, state.S2, state.S3);
    }

    public static DeterministicRng FromSeed(ulong seed)
    {
        ulong state = seed;
        ulong NextSplitMix()
        {
            state += 0x9E3779B97F4A7C15UL;
            ulong z = state;
            z = (z ^ (z >> 30)) * 0xBF58476D1CE4E5B9UL;
            z = (z ^ (z >> 27)) * 0x94D049BB133111EBUL;
            return z ^ (z >> 31);
        }
        return new DeterministicRng(new RngSnapshot(NextSplitMix(), NextSplitMix(), NextSplitMix(), NextSplitMix()));
    }

    public static DeterministicRng FromSnapshot(RngSnapshot snapshot) => new(snapshot);
    public RngSnapshot Snapshot() => new(_s0, _s1, _s2, _s3);

    public ulong NextUInt64()
    {
        ulong result = RotateLeft(_s1 * 5, 7) * 9;
        ulong t = _s1 << 17;
        _s2 ^= _s0;
        _s3 ^= _s1;
        _s1 ^= _s2;
        _s0 ^= _s3;
        _s2 ^= t;
        _s3 = RotateLeft(_s3, 45);
        return result;
    }

    public double NextOpenUnit() => ((NextUInt64() >> 11) + 0.5) * (1.0 / 9007199254740992.0);
    private static ulong RotateLeft(ulong value, int shift) => (value << shift) | (value >> (64 - shift));
}
