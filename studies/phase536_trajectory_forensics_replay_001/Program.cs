using System.Security.Cryptography;
using System.Text.Json;

const string PhaseDir = "studies/phase536_trajectory_forensics_replay_001";
const string ContractPath = PhaseDir + "/preregistration/phase536_trajectory_forensics_replay_contract_v2.json";
const string Phase533ContractPath = "studies/phase533_nested_validation_contract_001/preregistration/phase533_nested_validation_contract_v1.json";
const string Phase534SummaryPath = "studies/phase534_nested_control_battery_001/output/nested_control_battery_summary.json";
const string OutputPath = PhaseDir + "/output/trajectory_forensics_replay.json";
const string SummaryPath = PhaseDir + "/output/trajectory_forensics_replay_summary.json";

byte[] contractBytes = File.ReadAllBytes(ContractPath);
using JsonDocument contractDocument = JsonDocument.Parse(contractBytes);
JsonElement contract = contractDocument.RootElement;
Binding[] bindings = contract.GetProperty("expectedInputs").EnumerateArray().Select(x => Bind(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
JsonElement boundary = contract.GetProperty("replayBoundary");
JsonElement execution = contract.GetProperty("execution");
JsonElement reproductionChecks = contract.GetProperty("reproductionChecks");
var expectedBindingInventory = new (string Id, string Path)[]
{
    ("phase533-contract", "studies/phase533_nested_validation_contract_001/preregistration/phase533_nested_validation_contract_v1.json"),
    ("phase533-summary", "studies/phase533_nested_validation_contract_001/output/nested_validation_contract_summary.json"),
    ("phase534-contract", "studies/phase534_nested_control_battery_001/preregistration/phase534_nested_control_contract_v1.json"),
    ("phase534-program", "studies/phase534_nested_control_battery_001/Program.cs"),
    ("phase534-summary", "studies/phase534_nested_control_battery_001/output/nested_control_battery_summary.json"),
    ("phase535-summary", "studies/phase535_bounded_registered_operator_pilot_adjudicator_001/output/bounded_registered_operator_pilot_adjudicator_summary.json"),
};
string[] expectedStages = ["initial-gradient", "position", "intermediate-gradient", "final-gradient", "energy"];
string[] expectedTerminals =
[
    "invalid-or-drifted-input", "aggregate-behavior-not-reproduced", "telemetry-invalid",
    "aggregate-failures-observed-warmup-and-retained", "aggregate-failures-observed-warmup-only",
    "aggregate-failures-observed-retained-only", "aggregate-no-failures-observed",
];

using JsonDocument phase533Document = JsonDocument.Parse(File.ReadAllText(Phase533ContractPath));
JsonElement phase533 = phase533Document.RootElement;
JsonElement controls = phase533.GetProperty("controlConfiguration");
SeedTable[] seedTables = phase533.GetProperty("seedTables").EnumerateArray().Select(t => new SeedTable(
    t.GetProperty("id").GetString()!,
    t.GetProperty("seeds").EnumerateArray().Select(x => x.GetInt32()).ToArray(),
    t.GetProperty("initialScales").EnumerateArray().Select(x => x.GetDouble()).ToArray())).ToArray();

using JsonDocument phase534Document = JsonDocument.Parse(File.ReadAllText(Phase534SummaryPath));
JsonElement phase534 = phase534Document.RootElement;
JsonElement polynomial = phase534.GetProperty("reducedInteractingControl").GetProperty("polynomial");
double c2 = polynomial.GetProperty("c2").GetDouble();
double c3 = polynomial.GetProperty("c3").GetDouble();
double c4 = polynomial.GetProperty("c4").GetDouble();
double Potential(double x) => c2 * x * x + c3 * x * x * x + c4 * x * x * x * x;
double Gradient(double x) => 2 * c2 * x + 3 * c3 * x * x + 4 * c4 * x * x * x;

int warmup = controls.GetProperty("reducedWarmupPerSeed").GetInt32();
int retained = controls.GetProperty("reducedRetainedPerSeed").GetInt32();
int leapfrogSteps = controls.GetProperty("leapfrogSteps").GetInt32();
int seedOffset = boundary.GetProperty("seedOffset").GetInt32();
int maximumHistory = boundary.GetProperty("maximumRecordedAdaptationEventsPerChain").GetInt32();
int maximumFirstFailures = boundary.GetProperty("maximumRecordedFirstFailuresPerChain").GetInt32();
int adaptationWindow = boundary.GetProperty("adaptationWindow").GetInt32();
double adaptationUpper = boundary.GetProperty("adaptationUpperAcceptance").GetDouble();
double adaptationLower = boundary.GetProperty("adaptationLowerAcceptance").GetDouble();
double adaptationIncrease = boundary.GetProperty("adaptationIncreaseFactor").GetDouble();
double adaptationDecrease = boundary.GetProperty("adaptationDecreaseFactor").GetDouble();
bool bindingInventoryValid = bindings.Select(x => (x.Id, x.Path)).SequenceEqual(expectedBindingInventory);

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 2
    && contract.GetProperty("contractId").GetString() == "phase536-a23-trajectory-forensics-replay-v2"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A23"
    && contract.GetProperty("frozenBeforeRepairRun").GetBoolean()
    && contract.GetProperty("supersedesPreReviewContractSha256").GetString() == "e20698b88c43abb120491eee9c985889d6996e2f43736931981ca4d6c725b225"
    && bindingInventoryValid && bindings.Length == 6 && bindings.All(x => x.HashMatches)
    && boundary.GetProperty("sourceAlgorithm").GetString() == "phase534-run-scalar-hmc-source-matched"
    && !boundary.GetProperty("behaviorChangesAllowed").GetBoolean()
    && seedOffset == 900000
    && boundary.GetProperty("initialStepSize").GetDouble() == 0.12
    && adaptationWindow == 50
    && adaptationUpper == 0.90
    && adaptationLower == 0.65
    && adaptationIncrease == 1.15
    && adaptationDecrease == 0.85
    && boundary.GetProperty("divergenceDeltaH").GetDouble() == 100.0
    && maximumHistory == 20 && maximumHistory == warmup / adaptationWindow
    && maximumFirstFailures == 1
    && boundary.GetProperty("finiteMagnitudeRule").GetString() == "max-absolute-finite-value-by-warmup-retained-and-state-momentum-gradient-energy"
    && boundary.GetProperty("nonfiniteStagePrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedStages)
    && reproductionChecks.GetProperty("requireExactTableAcceptanceCounts").GetBoolean()
    && reproductionChecks.GetProperty("requireExactTableNonfiniteCounts").GetBoolean()
    && reproductionChecks.GetProperty("requireExactTableDivergenceCounts").GetBoolean()
    && reproductionChecks.GetProperty("requireExactFinalStepSizes").GetBoolean()
    && execution.GetProperty("sourceSeedTableIds").EnumerateArray().Select(x => x.GetString()).SequenceEqual(["table-a", "table-b"])
    && execution.GetProperty("chainsPerTable").GetInt32() == 4
    && execution.GetProperty("warmupPerChain").GetInt32() == 1000 && warmup == 1000
    && execution.GetProperty("retainedPerChain").GetInt32() == 4000 && retained == 4000
    && execution.GetProperty("leapfrogSteps").GetInt32() == 8 && leapfrogSteps == 8
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedTerminals)
    && seedTables.Length == 2
    && seedTables.Select(x => x.Id).SequenceEqual(["table-a", "table-b"])
    && seedTables.All(x => x.Seeds.Length == 4 && x.InitialScales.Length == 4)
    && ClaimBoundaryValid(contract.GetProperty("claimBoundary"));

var tableResults = new List<TableForensics>();
foreach (SeedTable table in seedTables)
{
    var chains = new List<ChainForensics>();
    for (int c = 0; c < table.Seeds.Length; c++)
        chains.Add(RunScalarHmcForensics(table.Seeds[c] + seedOffset, table.Seeds[c], table.InitialScales[c],
            warmup, retained, leapfrogSteps, maximumHistory, Potential, Gradient));
    tableResults.Add(new TableForensics(table.Id, chains));
}

JsonElement[] expectedTables = phase534.GetProperty("reducedInteractingControl").GetProperty("tables").EnumerateArray().ToArray();
var reproductionRows = new List<object>();
bool aggregateBehaviorReproduced = expectedTables.Length == tableResults.Count;
for (int t = 0; t < tableResults.Count; t++)
{
    TableForensics actual = tableResults[t];
    JsonElement expected = expectedTables[t];
    int expectedTotal = actual.Chains.Sum(x => x.Retained.Total);
    int expectedAccepted = checked((int)System.Math.Round(expected.GetProperty("acceptanceRate").GetDouble() * expectedTotal));
    int actualAccepted = actual.Chains.Sum(x => x.Retained.Accepted);
    int actualNonfinite = actual.Chains.Sum(x => x.Warmup.NonFinite + x.Retained.NonFinite);
    int actualDivergences = actual.Chains.Sum(x => x.Warmup.Divergences + x.Retained.Divergences);
    double[] actualStepSizes = actual.Chains.Select(x => x.FinalStepSize).ToArray();
    double[] expectedStepSizes = expected.GetProperty("stepSizes").EnumerateArray().Select(x => x.GetDouble()).ToArray();
    bool rowMatches = actual.Id == expected.GetProperty("id").GetString()
        && actualAccepted == expectedAccepted
        && actualNonfinite == expected.GetProperty("nonFiniteCount").GetInt32()
        && actualDivergences == expected.GetProperty("divergenceCount").GetInt32()
        && actualStepSizes.SequenceEqual(expectedStepSizes);
    aggregateBehaviorReproduced &= rowMatches;
    reproductionRows.Add(new
    {
        table = actual.Id, rowMatches, expectedAccepted, actualAccepted,
        expectedNonfinite = expected.GetProperty("nonFiniteCount").GetInt32(), actualNonfinite,
        expectedDivergences = expected.GetProperty("divergenceCount").GetInt32(), actualDivergences,
        expectedStepSizes, actualStepSizes
    });
}

int warmupNonfinite = tableResults.Sum(t => t.Chains.Sum(c => c.Warmup.NonFinite));
int retainedNonfinite = tableResults.Sum(t => t.Chains.Sum(c => c.Retained.NonFinite));
int warmupDivergences = tableResults.Sum(t => t.Chains.Sum(c => c.Warmup.Divergences));
int retainedDivergences = tableResults.Sum(t => t.Chains.Sum(c => c.Retained.Divergences));
bool warmupFailures = warmupNonfinite + warmupDivergences > 0;
bool retainedFailures = retainedNonfinite + retainedDivergences > 0;
bool telemetryBounded = tableResults.All(t => t.Chains.All(c => TelemetryValid(c, warmup, retained,
    maximumHistory, maximumFirstFailures, adaptationWindow, adaptationUpper, adaptationLower,
    adaptationIncrease, adaptationDecrease, expectedStages)));
string verdict = !contractValid ? "invalid-or-drifted-input"
    : !aggregateBehaviorReproduced ? "aggregate-behavior-not-reproduced"
    : !telemetryBounded ? "telemetry-invalid"
    : warmupFailures && retainedFailures ? "aggregate-failures-observed-warmup-and-retained"
    : warmupFailures ? "aggregate-failures-observed-warmup-only"
    : retainedFailures ? "aggregate-failures-observed-retained-only"
    : "aggregate-no-failures-observed";

var result = new
{
    schemaVersion = 1,
    phase = 536,
    phaseId = "phase536-trajectory-forensics-replay",
    contractSha256 = Sha(contractBytes),
    contractValid,
    bindingInventoryValid,
    bindings,
    replayConfiguration = new
    {
        scalarAlgorithm = "phase534-run-scalar-hmc-source-matched-replay",
        coefficientSource = Phase534SummaryPath,
        polynomial = new { c2, c3, c4 },
        warmup, retained, leapfrogSteps, seedOffset,
        initialStepSize = 0.12, adaptationWindow = 50,
        adaptationRule = "rate>0.90 multiply 1.15; rate<0.65 multiply 0.85"
    },
    reproduction = new
    {
        aggregateBehaviorReproduced,
        immutablePerTrajectoryReferenceAvailable = false,
        exactTrajectoryReproductionClaimed = false,
        rows = reproductionRows
    },
    failurePartition = new
    {
        warmup = new { nonFiniteCount = warmupNonfinite, divergenceCount = warmupDivergences },
        retained = new { nonFiniteCount = retainedNonfinite, divergenceCount = retainedDivergences }
    },
    tables = tableResults,
    telemetryBounded,
    verdictKind = verdict,
    terminalStatus = "trajectory-forensics-replay-" + verdict,
    phase534Repaired = false,
    phase534Relabeled = false,
    phase535PilotReopened = false,
    completeLatticeSd2Validated = false,
    productionValidated = false,
    physicalClaimAllowed = false,
    promotedPhysicalMassClaimCount = 0,
    failureCauseResolved = false,
    decision = verdict switch
    {
        "aggregate-failures-observed-warmup-and-retained" => "The source-matched replay agrees with Phase534 aggregate table metrics and observes failures in both warmup and retained trajectories. It does not prove per-trajectory identity or resolve the failure cause.",
        "aggregate-failures-observed-warmup-only" => "The source-matched replay agrees with Phase534 aggregate table metrics and observes failures only during warmup. It does not prove per-trajectory identity or resolve the failure cause.",
        "aggregate-failures-observed-retained-only" => "The source-matched replay agrees with Phase534 aggregate table metrics and observes failures only during retained sampling. It does not prove per-trajectory identity or resolve the failure cause.",
        "aggregate-no-failures-observed" => "The source-matched replay agrees with Phase534 aggregate table metrics but observes no failures. No cause is resolved.",
        _ => "The replay input, aggregate comparison, or bounded telemetry is invalid. No localization or cause is promoted."
    }
};

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase536 verdict: {verdict}");
Console.WriteLine($"warmup nonfinite/divergent: {warmupNonfinite}/{warmupDivergences}");
Console.WriteLine($"retained nonfinite/divergent: {retainedNonfinite}/{retainedDivergences}");

static ChainForensics RunScalarHmcForensics(int replaySeed, int sourceSeed, double initialScale,
    int warmup, int retained, int nLeap, int maximumHistory,
    Func<double, double> potential, Func<double, double> gradient)
{
    var rng = new Random(replaySeed); double x = initialScale, eps = 0.12;
    int wa = 0, wt = 0;
    var warmupCounts = new SegmentCounts();
    var retainedCounts = new SegmentCounts();
    var warmupMagnitude = new Magnitudes();
    var retainedMagnitude = new Magnitudes();
    var history = new List<AdaptationEvent>(maximumHistory);
    FirstFailure? firstFailure = null;
    for (int iter = 0; iter < warmup + retained; iter++)
    {
        bool isWarmup = iter < warmup;
        SegmentCounts counts = isWarmup ? warmupCounts : retainedCounts;
        Magnitudes magnitude = isWarmup ? warmupMagnitude : retainedMagnitude;
        counts.Total++;
        string? firstNonfiniteStage = null;
        int? firstNonfiniteLeapfrogIndex = null;

        double oldX = x, p = Gaussian(rng), oldP = p;
        magnitude.ObserveState(x); magnitude.ObserveMomentum(p);
        double g = gradient(x); magnitude.ObserveGradient(g);
        if ((!double.IsFinite(g) || !double.IsFinite(p)) && firstNonfiniteStage is null) firstNonfiniteStage = "initial-gradient";
        p -= 0.5 * eps * g; magnitude.ObserveMomentum(p);
        if (!double.IsFinite(p) && firstNonfiniteStage is null) firstNonfiniteStage = "initial-gradient";
        for (int l = 0; l < nLeap; l++)
        {
            x += eps * p; magnitude.ObserveState(x);
            if (!double.IsFinite(x) && firstNonfiniteStage is null)
            {
                firstNonfiniteStage = "position"; firstNonfiniteLeapfrogIndex = l;
            }
            if (l != nLeap - 1)
            {
                g = gradient(x); magnitude.ObserveGradient(g);
                if (!double.IsFinite(g) && firstNonfiniteStage is null)
                {
                    firstNonfiniteStage = "intermediate-gradient"; firstNonfiniteLeapfrogIndex = l;
                }
                p -= eps * g; magnitude.ObserveMomentum(p);
                if (!double.IsFinite(p) && firstNonfiniteStage is null)
                {
                    firstNonfiniteStage = "intermediate-gradient"; firstNonfiniteLeapfrogIndex = l;
                }
            }
        }
        g = gradient(x); magnitude.ObserveGradient(g);
        if (!double.IsFinite(g) && firstNonfiniteStage is null) firstNonfiniteStage = "final-gradient";
        p -= 0.5 * eps * g; magnitude.ObserveMomentum(p);
        if (!double.IsFinite(p) && firstNonfiniteStage is null) firstNonfiniteStage = "final-gradient";
        double newPotential = potential(x), newKinetic = 0.5 * p * p;
        double oldPotential = potential(oldX), oldKinetic = 0.5 * oldP * oldP;
        double dh = newPotential + newKinetic - oldPotential - oldKinetic;
        double proposedX = x;
        magnitude.ObserveEnergy(newPotential); magnitude.ObserveEnergy(newKinetic);
        magnitude.ObserveEnergy(oldPotential); magnitude.ObserveEnergy(oldKinetic); magnitude.ObserveEnergy(dh);
        bool finite = double.IsFinite(dh) && double.IsFinite(x);
        if (!finite && firstNonfiniteStage is null) firstNonfiniteStage = "energy";
        bool accept = finite && (dh <= 0 || rng.NextDouble() < System.Math.Exp(-dh));
        if (!accept) x = oldX;
        double postDecisionX = x;
        bool divergence = finite && System.Math.Abs(dh) > 100;
        if (!finite) counts.NonFinite++;
        if (divergence) counts.Divergences++;
        if (accept) counts.Accepted++;
        if (firstFailure is null && (!finite || divergence))
            firstFailure = new FirstFailure(
                !finite ? "nonfinite" : "divergence",
                isWarmup ? "warmup" : "retained",
                iter,
                isWarmup ? iter : iter - warmup,
                firstNonfiniteStage ?? "energy",
                firstNonfiniteLeapfrogIndex,
                FiniteOrNull(oldX), FiniteOrNull(oldP), FiniteOrNull(proposedX), FiniteOrNull(postDecisionX), FiniteOrNull(p),
                FiniteOrNull(g), FiniteOrNull(dh), eps);

        if (isWarmup)
        {
            wt++; if (accept) wa++;
            if ((iter + 1) % 50 == 0)
            {
                double rate = (double)wa / wt;
                double before = eps;
                if (rate > 0.90) eps *= 1.15; else if (rate < 0.65) eps *= 0.85;
                if (history.Count < maximumHistory)
                    history.Add(new AdaptationEvent(iter + 1, wa, wt, rate, before, eps));
                wa = 0; wt = 0;
            }
        }
    }
    return new ChainForensics(sourceSeed, replaySeed, initialScale, warmupCounts, retainedCounts,
        history.ToArray(), firstFailure is null ? 0 : 1, firstFailure,
        warmupMagnitude.Snapshot(), retainedMagnitude.Snapshot(), eps);
}

static bool ClaimBoundaryValid(JsonElement claim) =>
    !claim.GetProperty("phase534Repaired").GetBoolean()
    && !claim.GetProperty("phase534Relabeled").GetBoolean()
    && !claim.GetProperty("phase535PilotReopened").GetBoolean()
    && !claim.GetProperty("completeLatticeSd2Validated").GetBoolean()
    && !claim.GetProperty("productionValidated").GetBoolean()
    && !claim.GetProperty("physicalClaimAllowed").GetBoolean()
    && claim.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

static bool TelemetryValid(ChainForensics chain, int warmup, int retained, int maximumHistory,
    int maximumFirstFailures, int adaptationWindow, double adaptationUpper, double adaptationLower,
    double adaptationIncrease, double adaptationDecrease, string[] allowedStages)
{
    bool countsValid = chain.Warmup.Total == warmup && chain.Retained.Total == retained
        && chain.Warmup.Accepted >= 0 && chain.Warmup.Accepted <= warmup
        && chain.Retained.Accepted >= 0 && chain.Retained.Accepted <= retained
        && chain.Warmup.NonFinite >= 0 && chain.Retained.NonFinite >= 0
        && chain.Warmup.Divergences >= 0 && chain.Retained.Divergences >= 0;
    bool historyValid = chain.AdaptationHistory.Length == maximumHistory;
    double priorStep = 0.12;
    for (int i = 0; i < chain.AdaptationHistory.Length; i++)
    {
        AdaptationEvent item = chain.AdaptationHistory[i];
        double expectedRate = item.WindowAccepted / (double)adaptationWindow;
        double expectedAfter = expectedRate > adaptationUpper ? priorStep * adaptationIncrease
            : expectedRate < adaptationLower ? priorStep * adaptationDecrease : priorStep;
        historyValid &= item.CompletedWarmupIterations == (i + 1) * adaptationWindow
            && item.WindowTotal == adaptationWindow
            && item.WindowAccepted >= 0 && item.WindowAccepted <= adaptationWindow
            && item.WindowAcceptanceRate == expectedRate
            && item.StepSizeBefore == priorStep
            && item.StepSizeAfter == expectedAfter;
        priorStep = item.StepSizeAfter;
    }
    historyValid &= chain.FinalStepSize == priorStep;

    int observedFailures = chain.Warmup.NonFinite + chain.Warmup.Divergences
        + chain.Retained.NonFinite + chain.Retained.Divergences;
    bool firstFailureValid = chain.FirstFailureRecordCount == (chain.FirstFailure is null ? 0 : 1)
        && chain.FirstFailureRecordCount <= maximumFirstFailures
        && (observedFailures == 0 ? chain.FirstFailure is null : chain.FirstFailure is not null);
    if (chain.FirstFailure is { } failure)
    {
        firstFailureValid &= (failure.Kind == "nonfinite" || failure.Kind == "divergence")
            && allowedStages.Contains(failure.Stage, StringComparer.Ordinal)
            && (failure.Segment == "warmup" || failure.Segment == "retained")
            && failure.GlobalIteration >= 0 && failure.GlobalIteration < warmup + retained
            && failure.SegmentIteration >= 0
            && (failure.Segment == "warmup"
                ? failure.GlobalIteration < warmup && failure.SegmentIteration == failure.GlobalIteration
                : failure.GlobalIteration >= warmup && failure.SegmentIteration == failure.GlobalIteration - warmup)
            && failure.StepSize > 0 && double.IsFinite(failure.StepSize);
    }

    return countsValid && historyValid && firstFailureValid
        && MagnitudeValid(chain.WarmupFiniteMagnitudes)
        && MagnitudeValid(chain.RetainedFiniteMagnitudes);
}

static bool MagnitudeValid(MagnitudeSnapshot value) =>
    double.IsFinite(value.MaxAbsFiniteState) && value.MaxAbsFiniteState >= 0
    && double.IsFinite(value.MaxAbsFiniteMomentum) && value.MaxAbsFiniteMomentum >= 0
    && double.IsFinite(value.MaxAbsFiniteGradient) && value.MaxAbsFiniteGradient >= 0
    && double.IsFinite(value.MaxAbsFiniteEnergy) && value.MaxAbsFiniteEnergy >= 0;

static double? FiniteOrNull(double value) => double.IsFinite(value) ? value : null;
static double Gaussian(Random rng)
{
    double u1 = 1.0 - rng.NextDouble(), u2 = rng.NextDouble();
    return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
}
static Binding Bind(string id, string path, string expected)
{
    string actual = Sha(File.ReadAllBytes(path)); return new(id, path, expected, actual, actual == expected);
}
static string Sha(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

public sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
public sealed record SeedTable(string Id, int[] Seeds, double[] InitialScales);
public sealed record AdaptationEvent(int CompletedWarmupIterations, int WindowAccepted, int WindowTotal,
    double WindowAcceptanceRate, double StepSizeBefore, double StepSizeAfter);
public sealed record FirstFailure(string Kind, string Segment, int GlobalIteration, int SegmentIteration,
    string Stage, int? LeapfrogIndex, double? OldState, double? InitialMomentum, double? ProposedState,
    double? PostDecisionState, double? FinalMomentum, double? FinalGradient, double? DeltaH, double StepSize);
public sealed record MagnitudeSnapshot(double MaxAbsFiniteState, double MaxAbsFiniteMomentum,
    double MaxAbsFiniteGradient, double MaxAbsFiniteEnergy);
public sealed record ChainForensics(int SourceSeed, int ReplaySeed, double InitialScale,
    SegmentCounts Warmup, SegmentCounts Retained, AdaptationEvent[] AdaptationHistory,
    int FirstFailureRecordCount, FirstFailure? FirstFailure, MagnitudeSnapshot WarmupFiniteMagnitudes,
    MagnitudeSnapshot RetainedFiniteMagnitudes, double FinalStepSize);
public sealed record TableForensics(string Id, List<ChainForensics> Chains);

public sealed class SegmentCounts
{
    public int Total { get; set; }
    public int Accepted { get; set; }
    public int NonFinite { get; set; }
    public int Divergences { get; set; }
}

public sealed class Magnitudes
{
    private double state;
    private double momentum;
    private double gradient;
    private double energy;
    public void ObserveState(double value) => state = MaxFinite(state, value);
    public void ObserveMomentum(double value) => momentum = MaxFinite(momentum, value);
    public void ObserveGradient(double value) => gradient = MaxFinite(gradient, value);
    public void ObserveEnergy(double value) => energy = MaxFinite(energy, value);
    public MagnitudeSnapshot Snapshot() => new(state, momentum, gradient, energy);
    private static double MaxFinite(double current, double value) =>
        double.IsFinite(value) ? System.Math.Max(current, System.Math.Abs(value)) : current;
}
