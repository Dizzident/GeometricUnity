using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase539_independent_reduced_target_row_confirmation_001";
const string ContractPath = Root + "/preregistration/phase539_independent_reduced_target_row_confirmation_contract_v1.json";
const string OutputPath = Root + "/output/independent_reduced_target_row_confirmation.json";
const string SummaryPath = Root + "/output/independent_reduced_target_row_confirmation_summary.json";

var timer = Stopwatch.StartNew();
TimeSpan cpuStart = Process.GetCurrentProcess().TotalProcessorTime;
using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;

var expectedBindings = new (string Id, string Path)[]
{
    ("phase534-contract", "studies/phase534_nested_control_battery_001/preregistration/phase534_nested_control_contract_v1.json"),
    ("phase534-program", "studies/phase534_nested_control_battery_001/Program.cs"),
    ("phase534-summary", "studies/phase534_nested_control_battery_001/output/nested_control_battery_summary.json"),
    ("phase536-summary", "studies/phase536_trajectory_forensics_replay_001/output/trajectory_forensics_replay_summary.json"),
    ("phase537-summary", "studies/phase537_deterministic_leapfrog_correctness_stability_audit_001/output/deterministic_leapfrog_correctness_stability_audit_summary.json"),
    ("phase538-contract-v2", "studies/phase538_fixed_grid_interacting_hmc_retuning_001/preregistration/phase538_fixed_grid_interacting_hmc_retuning_contract_v2.json"),
    ("phase538-program", "studies/phase538_fixed_grid_interacting_hmc_retuning_001/Program.cs"),
    ("phase538-summary", "studies/phase538_fixed_grid_interacting_hmc_retuning_001/output/fixed_grid_interacting_hmc_retuning_summary.json"),
};
var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new BindingSpec(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = bindingSpecs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha256(x.Path) : "missing";
    return new Binding(x.Id, x.Path, x.ExpectedSha256, actual, actual == x.ExpectedSha256);
}).ToArray();
bool bindingInventoryValid = bindingSpecs.Select(x => (x.Id, x.Path)).SequenceEqual(expectedBindings);
bool exactBindingsValid = bindingInventoryValid && bindings.All(x => x.HashMatches);

using var p534Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[2].Path));
using var p536Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[3].Path));
using var p537Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[4].Path));
using var p538ContractDocument = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[5].Path));
using var p538Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[7].Path));
JsonElement p534 = p534Document.RootElement;
JsonElement p536 = p536Document.RootElement;
JsonElement p537 = p537Document.RootElement;
JsonElement p538Contract = p538ContractDocument.RootElement;
JsonElement p538 = p538Document.RootElement;

JsonElement row = contract.GetProperty("fixedSelectedRow");
JsonElement p538Selected = p538.GetProperty("selection");
bool precursorSemanticsValid =
    p534.GetProperty("verdictKind").GetString() == "reduced-interacting-control-failed"
    && p536.GetProperty("verdictKind").GetString() == "aggregate-failures-observed-warmup-and-retained"
    && !p536.GetProperty("failureCauseResolved").GetBoolean()
    && p537.GetProperty("verdictKind").GetString() == "deterministic-leapfrog-audit-passed"
    && p538.GetProperty("verdictKind").GetString() == "post-review-hardened-stable-fixed-grid-row-reduced-target-feasible"
    && p538.GetProperty("reducedTargetFeasibilityOnly").GetBoolean()
    && p538Selected.GetProperty("selectedRowId").GetString() == row.GetProperty("rowId").GetString()
    && p538Selected.GetProperty("thirdFamilyParticipatesInSelection").GetBoolean()
    && !p538Selected.GetProperty("thirdFamilyIsIndependentPostSelectionValidation").GetBoolean();

var families = contract.GetProperty("independentSeedFamilies").EnumerateArray().Select(x => new SeedFamily(
    x.GetProperty("id").GetString()!, x.GetProperty("seedOffset").GetInt32(),
    x.GetProperty("seeds").EnumerateArray().Select(y => y.GetInt32()).ToArray(),
    x.GetProperty("initialScales").EnumerateArray().Select(y => y.GetDouble()).ToArray())).ToArray();
int[] priorRawSeeds = p538Contract.GetProperty("seedFamilies").EnumerateArray()
    .SelectMany(x => x.GetProperty("seeds").EnumerateArray().Select(y => y.GetInt32())).ToArray();
int[] priorExecutionSeeds = p538Contract.GetProperty("seedFamilies").EnumerateArray()
    .SelectMany(x =>
    {
        int offset = x.GetProperty("seedOffset").GetInt32();
        return x.GetProperty("seeds").EnumerateArray().Select(y => y.GetInt32() + offset);
    }).ToArray();
int[] newRawSeeds = families.SelectMany(x => x.Seeds).ToArray();
int[] newExecutionSeeds = families.SelectMany(x => x.Seeds.Select(seed => seed + x.SeedOffset)).ToArray();
bool familiesValid = families.Select(x => x.Id).SequenceEqual(
        new[] { "independent-post-selection-a", "independent-post-selection-b" }, StringComparer.Ordinal)
    && families.All(x => x.Seeds.Length == 4 && x.InitialScales.Length == 4)
    && newRawSeeds.Distinct().Count() == 8 && newExecutionSeeds.Distinct().Count() == 8
    && !newRawSeeds.Intersect(priorRawSeeds).Any()
    && !newExecutionSeeds.Intersect(priorExecutionSeeds).Any();

JsonElement execution = contract.GetProperty("execution");
JsonElement gates = contract.GetProperty("gates");
JsonElement oldGates = p538Contract.GetProperty("gates");
string[] gateNames =
[
    "maximumNonFiniteTrajectoriesPerFamily", "maximumDivergencesPerFamily", "acceptanceMinimum",
    "acceptanceMaximum", "rhatMaximum", "bulkEssMinimum", "expMinusDeltaHAbsoluteFloor",
    "expMinusDeltaHStandardErrorMultiplier", "momentStandardErrorMultiplier",
    "quadratureAgreementTolerance", "integrationByPartsTarget",
];
bool gatesUnweakened = gates.GetProperty("copiedUnweakenedFromPhase538V2").GetBoolean()
    && gateNames.All(name => gates.GetProperty(name).GetDouble() == oldGates.GetProperty(name).GetDouble());
string[] expectedPrecedence =
[
    "invalid-or-drifted-input", "resource-refusal", "selected-row-not-independently-confirmed",
    "selected-row-independently-confirmed-reduced-target-only",
];
string[] expectedFirewallKeys =
[
    "phase534ReinterpretedOrRelabeled", "phase538UpgradedToPristinePreregistration",
    "phase535PilotExecutedOrReopened", "reducedToCompleteLatticeTransferValidated", "completeLatticeValidated",
    "productionDefaultSelected", "phase481PackCreatedOrMutated", "phase458G3Satisfied",
    "phase458G4Satisfied", "phase458G5Satisfied", "o4Discharged", "sourceContractApplicationAllowed",
    "physicalUnitOrGevClaimAllowed", "productionOrLaunchAllowed",
];
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase539-a24-independent-reduced-target-row-confirmation-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A24"
    && contract.GetProperty("frozenBeforeFirstRun").GetBoolean()
    && exactBindingsValid && precursorSemanticsValid && familiesValid && gatesUnweakened
    && row.GetProperty("rowId").GetString() == "eps-0.25-len-2.00"
    && row.GetProperty("stepSize").GetDouble() == 0.25
    && row.GetProperty("trajectoryLength").GetDouble() == 2.0
    && row.GetProperty("leapfrogSteps").GetInt32() == 8
    && !row.GetProperty("rowSearchAllowed").GetBoolean()
    && !row.GetProperty("retuningAllowed").GetBoolean()
    && !execution.GetProperty("adaptationEnabled").GetBoolean()
    && contract.GetProperty("confirmation").GetProperty("everyRegisteredFamilyMustPassEveryGate").GetBoolean()
    && !contract.GetProperty("confirmation").GetProperty("phase538SeedsMayBeReused").GetBoolean()
    && !contract.GetProperty("confirmation").GetProperty("observedResultsMayAlterRowSeedsThresholdsOrDiagnostics").GetBoolean()
    && !contract.GetProperty("confirmation").GetProperty("physicalTargetsConsulted").GetBoolean()
    && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedPrecedence)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

JsonElement targetSpec = contract.GetProperty("target");
JsonElement p538Target = p538.GetProperty("target");
double c2 = p538Target.GetProperty("c2").GetDouble();
double c3 = p538Target.GetProperty("c3").GetDouble();
double c4 = p538Target.GetProperty("c4").GetDouble();
bool targetValid = targetSpec.GetProperty("kind").GetString() == "phase534-reduced-theta-zero-sd2-witness-ray-polynomial"
    && targetSpec.GetProperty("member").GetString() == "sd2-id0/c0.5"
    && targetSpec.GetProperty("extent").GetInt32() == 3
    && targetSpec.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && c2 > 0 && c4 > 0 && double.IsFinite(c3);
double Potential(double x) => c2 * x * x + c3 * x * x * x + c4 * x * x * x * x;
double Gradient(double x) => 2 * c2 * x + 3 * c3 * x * x + 4 * c4 * x * x * x;
JsonElement quadratureSpec = contract.GetProperty("quadrature");
QuadratureResult reference = IntegrateMoments(Potential,
    quadratureSpec.GetProperty("halfWidth").GetDouble(), quadratureSpec.GetProperty("panels").GetInt32());
JsonElement priorReference = p538Target.GetProperty("quadrature");
double referenceDifference = new[]
{
    Relative(reference.M1, priorReference.GetProperty("m1").GetDouble()),
    Relative(reference.M2, priorReference.GetProperty("m2").GetDouble()),
    Relative(reference.M3, priorReference.GetProperty("m3").GetDouble()),
    Relative(reference.M4, priorReference.GetProperty("m4").GetDouble()),
}.Max();
bool quadratureReferenceValid = double.IsFinite(reference.Z) && reference.Z > 0
    && referenceDifference <= quadratureSpec.GetProperty("referenceMomentTolerance").GetDouble();

JsonElement resources = contract.GetProperty("resourceRefusal");
double estimatedCpuSeconds = resources.GetProperty("estimatedAggregateCpuSeconds").GetDouble();
long estimatedPeakBytes = resources.GetProperty("estimatedPeakBytes").GetInt64();
double maximumCpuSeconds = resources.GetProperty("maximumAggregateCpuSeconds").GetDouble();
long maximumPeakBytes = resources.GetProperty("maximumPeakBytes").GetInt64();
bool resourceEstimateWithinBounds = estimatedCpuSeconds <= maximumCpuSeconds && estimatedPeakBytes <= maximumPeakBytes;
bool inputsValid = contractValid && targetValid && quadratureReferenceValid;

int warmup = execution.GetProperty("warmupPerChain").GetInt32();
int retained = execution.GetProperty("retainedPerChain").GetInt32();
double divergenceThreshold = execution.GetProperty("divergenceAbsoluteDeltaH").GetDouble();
double eps = row.GetProperty("stepSize").GetDouble();
int leapfrogSteps = row.GetProperty("leapfrogSteps").GetInt32();
var familyResults = new List<FamilyResult>();
if (inputsValid && resourceEstimateWithinBounds)
{
    foreach (SeedFamily family in families)
    {
        var chains = new List<ChainResult>();
        for (int i = 0; i < family.Seeds.Length; i++)
            chains.Add(RunScalarHmc(family.Seeds[i] + family.SeedOffset, family.InitialScales[i], eps,
                warmup, retained, leapfrogSteps, divergenceThreshold, Potential, Gradient));
        familyResults.Add(SummarizeFamily(family.Id, chains, reference, gates));
    }
}
double aggregateCpuSeconds = (Process.GetCurrentProcess().TotalProcessorTime - cpuStart).TotalSeconds;
timer.Stop();
long peakWorkingSetBytes = Process.GetCurrentProcess().PeakWorkingSet64;
bool measuredResourceBoundsPassed = aggregateCpuSeconds <= maximumCpuSeconds && peakWorkingSetBytes <= maximumPeakBytes;
bool everyFamilyPassed = familyResults.Count == families.Length && familyResults.All(x => x.Passed);
string verdict = !inputsValid ? "invalid-or-drifted-input"
    : !resourceEstimateWithinBounds || !measuredResourceBoundsPassed ? "resource-refusal"
    : !everyFamilyPassed ? "selected-row-not-independently-confirmed"
    : "selected-row-independently-confirmed-reduced-target-only";

var result = new
{
    schemaVersion = 1,
    phase = 539,
    phaseId = "phase539-independent-reduced-target-row-confirmation",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha256(ContractPath),
    planSection = contract.GetProperty("planSection").GetString(),
    pristinePreregistration = true,
    inputsValid,
    contractValid,
    bindingInventoryValid,
    exactBindingsValid,
    precursorSemanticsValid,
    familiesValid,
    priorSeedsReused = false,
    gatesUnweakened,
    targetValid,
    quadratureReferenceValid,
    referenceDifference,
    target = new { c2, c3, c4, quadrature = reference },
    fixedSelectedRow = new
    {
        rowId = row.GetProperty("rowId").GetString(), stepSize = eps,
        trajectoryLength = row.GetProperty("trajectoryLength").GetDouble(), leapfrogSteps,
        rowSearchPerformed = false, retuningPerformed = false,
    },
    execution = new
    {
        adaptationEnabled = false, seedFamilyCount = families.Length, chainCountPerFamily = 4,
        warmupPerChain = warmup, retainedPerChain = retained,
    },
    resource = new
    {
        resourceEstimateWithinBounds, measuredResourceBoundsPassed, estimatedCpuSeconds, maximumCpuSeconds,
        estimatedPeakBytes, maximumPeakBytes, volatileMeasurementsSerialized = false,
    },
    familyResults,
    everyRegisteredFamilyPassedEveryGate = everyFamilyPassed,
    independentPostSelectionConfirmation = everyFamilyPassed,
    verdictKind = verdict,
    terminalStatus = "independent-reduced-target-row-confirmation-" + verdict,
    decision = everyFamilyPassed
        ? "The Phase538-selected fixed row passes every unchanged hardened diagnostic gate on both newly preregistered post-selection seed families. This independently confirms only the one-dimensional reduced-target operating row."
        : "The fixed selected row was not independently confirmed under the pristine A24 protocol, or execution was refused. No row substitution or retuning is permitted.",
    phase534ReinterpretedOrRelabeled = false,
    phase538UpgradedToPristinePreregistration = false,
    phase535PilotExecutedOrReopened = false,
    reducedToCompleteLatticeTransferValidated = false,
    completeLatticeValidated = false,
    productionDefaultSelected = false,
    phase481PackCreatedOrMutated = false,
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
Require(result.promotedPhysicalMassClaimCount == 0 && !result.allDownstreamAuthority,
    "Phase539 authority firewall failed.");
Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase539 verdict: {verdict}");
Console.WriteLine($"families passing: {familyResults.Count(x => x.Passed)}/{families.Length}");
Console.WriteLine($"aggregate cpu seconds: {aggregateCpuSeconds:F3}; elapsed seconds: {timer.Elapsed.TotalSeconds:F3}; peak bytes: {peakWorkingSetBytes}");

static ChainResult RunScalarHmc(int seed, double initialX, double eps, int warmup, int retained, int steps,
    double divergenceThreshold, Func<double, double> potential, Func<double, double> gradient)
{
    var rng = new Random(seed);
    double x = initialX;
    var samples = new double[retained];
    var second = new double[retained];
    var xGradient = new double[retained];
    var deltaH = new double[retained];
    int accepted = 0, nonfinite = 0, divergences = 0;
    for (int iteration = 0; iteration < warmup + retained; iteration++)
    {
        double oldX = x;
        double p = Gaussian(rng), oldP = p;
        p -= 0.5 * eps * gradient(x);
        for (int leap = 0; leap < steps; leap++)
        {
            x += eps * p;
            if (leap != steps - 1) p -= eps * gradient(x);
        }
        p -= 0.5 * eps * gradient(x);
        double dh = potential(x) + 0.5 * p * p - potential(oldX) - 0.5 * oldP * oldP;
        bool finite = double.IsFinite(x) && double.IsFinite(p) && double.IsFinite(dh);
        bool divergent = finite && System.Math.Abs(dh) > divergenceThreshold;
        bool accept = finite && !divergent && (dh <= 0 || rng.NextDouble() < System.Math.Exp(-dh));
        if (!accept) x = oldX;
        if (!finite) nonfinite++;
        if (divergent) divergences++;
        if (iteration >= warmup)
        {
            int i = iteration - warmup;
            if (accept) accepted++;
            samples[i] = x;
            second[i] = x * x;
            xGradient[i] = x * gradient(x);
            deltaH[i] = dh;
        }
    }
    return new(samples, second, xGradient, deltaH, accepted, retained, nonfinite, divergences);
}

static FamilyResult SummarizeFamily(string id, List<ChainResult> chains, QuadratureResult reference, JsonElement gates)
{
    double[][] sampleChains = chains.Select(x => x.Samples).ToArray();
    double[][] secondChains = chains.Select(x => x.Second).ToArray();
    double[][] xGradientChains = chains.Select(x => x.XGradient).ToArray();
    double[] samples = sampleChains.SelectMany(x => x).ToArray();
    double[] second = secondChains.SelectMany(x => x).ToArray();
    double[] xGradient = xGradientChains.SelectMany(x => x).ToArray();
    double[] finiteDeltaH = chains.SelectMany(x => x.DeltaH).Where(double.IsFinite).ToArray();
    double[] expMinusDeltaH = finiteDeltaH.Select(x => System.Math.Exp(-x)).ToArray();
    bool energyFinite = expMinusDeltaH.Length == chains.Sum(x => x.Total) && expMinusDeltaH.All(double.IsFinite);
    double expMean = energyFinite ? Mean(expMinusDeltaH) : double.NaN;
    double expSe = energyFinite ? StdDev(expMinusDeltaH) / System.Math.Sqrt(expMinusDeltaH.Length) : double.NaN;
    double stateEss = PairedInitialPositiveMonotoneEss(sampleChains);
    double secondEss = PairedInitialPositiveMonotoneEss(secondChains);
    double gradientEss = PairedInitialPositiveMonotoneEss(xGradientChains);
    double mean = Mean(samples), meanSe = StdDev(samples) / System.Math.Sqrt(System.Math.Max(1, stateEss));
    double moment2 = Mean(second), moment2Se = StdDev(second) / System.Math.Sqrt(System.Math.Max(1, secondEss));
    double ibp = Mean(xGradient), ibpSe = StdDev(xGradient) / System.Math.Sqrt(System.Math.Max(1, gradientEss));
    double stateRhat = SplitRankNormalizedRhat(sampleChains);
    double secondRhat = SplitRankNormalizedRhat(secondChains);
    double gradientRhat = SplitRankNormalizedRhat(xGradientChains);
    double acceptance = chains.Sum(x => x.Accepted) / (double)chains.Sum(x => x.Total);
    int nonfinite = chains.Sum(x => x.NonFinite), divergences = chains.Sum(x => x.Divergences);
    double tolerance = gates.GetProperty("quadratureAgreementTolerance").GetDouble();
    double multiplier = gates.GetProperty("momentStandardErrorMultiplier").GetDouble();
    bool finiteGate = nonfinite <= gates.GetProperty("maximumNonFiniteTrajectoriesPerFamily").GetInt32();
    bool divergenceGate = divergences <= gates.GetProperty("maximumDivergencesPerFamily").GetInt32();
    bool acceptanceGate = acceptance >= gates.GetProperty("acceptanceMinimum").GetDouble()
        && acceptance <= gates.GetProperty("acceptanceMaximum").GetDouble();
    bool energyGate = energyFinite && System.Math.Abs(expMean - 1) <= System.Math.Max(
        gates.GetProperty("expMinusDeltaHAbsoluteFloor").GetDouble(),
        gates.GetProperty("expMinusDeltaHStandardErrorMultiplier").GetDouble() * expSe);
    double rhatMaximum = gates.GetProperty("rhatMaximum").GetDouble();
    double essMinimum = gates.GetProperty("bulkEssMinimum").GetDouble();
    bool stateConvergenceGate = double.IsFinite(stateRhat) && stateRhat <= rhatMaximum;
    bool secondConvergenceGate = double.IsFinite(secondRhat) && secondRhat <= rhatMaximum;
    bool gradientConvergenceGate = double.IsFinite(gradientRhat) && gradientRhat <= rhatMaximum;
    bool stateEssGate = double.IsFinite(stateEss) && stateEss >= essMinimum;
    bool secondEssGate = double.IsFinite(secondEss) && secondEss >= essMinimum;
    bool gradientEssGate = double.IsFinite(gradientEss) && gradientEss >= essMinimum;
    bool quadratureMomentGate = System.Math.Abs(mean - reference.M1) <= multiplier * meanSe + tolerance
        && System.Math.Abs(moment2 - reference.M2) <= multiplier * moment2Se + tolerance;
    bool integrationByPartsGate = System.Math.Abs(ibp - gates.GetProperty("integrationByPartsTarget").GetDouble())
        <= multiplier * ibpSe + tolerance;
    bool passed = finiteGate && divergenceGate && acceptanceGate && energyGate
        && stateConvergenceGate && secondConvergenceGate && gradientConvergenceGate
        && stateEssGate && secondEssGate && gradientEssGate && quadratureMomentGate && integrationByPartsGate;
    return new(id, passed, acceptance, nonfinite, divergences, expMean, expSe,
        stateRhat, secondRhat, gradientRhat, stateEss, secondEss, gradientEss, mean, meanSe,
        moment2, moment2Se, ibp, ibpSe, finiteGate, divergenceGate, acceptanceGate, energyGate,
        stateConvergenceGate, secondConvergenceGate, gradientConvergenceGate, stateEssGate,
        secondEssGate, gradientEssGate, quadratureMomentGate, integrationByPartsGate);
}

static QuadratureResult IntegrateMoments(Func<double, double> potential, double halfWidth, int panels)
{
    double h = 2 * halfWidth / panels;
    var sums = new double[5];
    for (int i = 0; i <= panels; i++)
    {
        double x = -halfWidth + i * h;
        double weight = i == 0 || i == panels ? 1 : i % 2 == 0 ? 2 : 4;
        double density = System.Math.Exp(-potential(x));
        double power = 1;
        for (int k = 0; k < sums.Length; k++) { sums[k] += weight * density * power; power *= x; }
    }
    for (int k = 0; k < sums.Length; k++) sums[k] *= h / 3;
    return new(sums[0], sums[1] / sums[0], sums[2] / sums[0], sums[3] / sums[0], sums[4] / sums[0]);
}

static double SplitRankNormalizedRhat(double[][] chains)
{
    double[][] split = SplitChains(chains);
    double[] pooled = split.SelectMany(x => x).ToArray();
    double[] ranked = RankNormalize(pooled);
    int n = split[0].Length;
    double[][] normalized = Enumerable.Range(0, split.Length).Select(i => ranked.Skip(i * n).Take(n).ToArray()).ToArray();
    double median = pooled.OrderBy(x => x).ElementAt(pooled.Length / 2);
    double[][] foldedSource = split.Select(chain => chain.Select(x => System.Math.Abs(x - median)).ToArray()).ToArray();
    double[] foldedRanked = RankNormalize(foldedSource.SelectMany(x => x).ToArray());
    double[][] folded = Enumerable.Range(0, split.Length).Select(i => foldedRanked.Skip(i * n).Take(n).ToArray()).ToArray();
    return System.Math.Max(BasicRhat(normalized), BasicRhat(folded));
}

static double BasicRhat(double[][] chains)
{
    int n = chains.Min(x => x.Length);
    double[] means = chains.Select(x => x.Take(n).Average()).ToArray();
    double within = chains.Select(x => Variance(x.Take(n).ToArray())).Average();
    if (!(within > 0)) return double.PositiveInfinity;
    double between = n * Variance(means);
    return System.Math.Sqrt((((n - 1.0) / n) * within + between / n) / within);
}

static double[][] SplitChains(double[][] chains)
{
    int half = chains.Min(x => x.Length) / 2;
    return half < 2 ? chains : chains.SelectMany(chain =>
        new[] { chain.Take(half).ToArray(), chain.Skip(chain.Length - half).Take(half).ToArray() }).ToArray();
}

static double[] RankNormalize(double[] values)
{
    var indexed = values.Select((value, index) => (value, index)).OrderBy(x => x.value).ToArray();
    var ranks = new double[values.Length];
    int start = 0;
    while (start < indexed.Length)
    {
        int end = start + 1;
        while (end < indexed.Length && indexed[end].value == indexed[start].value) end++;
        double averageRank = 0.5 * (start + 1 + end);
        for (int i = start; i < end; i++) ranks[indexed[i].index] = averageRank;
        start = end;
    }
    return ranks.Select(rank => InverseNormalCdf((rank - 0.375) / (values.Length + 0.25))).ToArray();
}

static double PairedInitialPositiveMonotoneEss(double[][] chains)
{
    int m = chains.Length, n = chains.Min(x => x.Length);
    int maximumLag = System.Math.Min(n - 1, 1000);
    double pairSum = 0, previousPair = double.PositiveInfinity;
    for (int evenLag = 0; evenLag + 1 <= maximumLag; evenLag += 2)
    {
        double rhoEven = evenLag == 0 ? 1 : chains.Select(x => AutoCorrelation(x, evenLag)).Average();
        double rhoOdd = chains.Select(x => AutoCorrelation(x, evenLag + 1)).Average();
        double pair = rhoEven + rhoOdd;
        if (!double.IsFinite(pair) || pair <= 0) break;
        pair = System.Math.Min(pair, previousPair);
        pairSum += pair;
        previousPair = pair;
    }
    double tau = System.Math.Max(1, -1 + 2 * pairSum);
    return System.Math.Min(m * n, m * n / tau);
}

static double AutoCorrelation(double[] values, int lag)
{
    double mean = Mean(values);
    double denominator = values.Sum(x => (x - mean) * (x - mean));
    if (!(denominator > 0)) return 0;
    double numerator = 0;
    for (int i = 0; i < values.Length - lag; i++) numerator += (values[i] - mean) * (values[i + lag] - mean);
    return numerator / denominator;
}

static double Gaussian(Random rng)
{
    double u1 = 1 - rng.NextDouble(), u2 = rng.NextDouble();
    return System.Math.Sqrt(-2 * System.Math.Log(u1)) * System.Math.Cos(2 * System.Math.PI * u2);
}

static double Mean(double[] values) => values.Length == 0 ? double.NaN : values.Average();
static double Variance(double[] values)
{
    if (values.Length < 2) return double.NaN;
    double mean = Mean(values);
    return values.Sum(x => (x - mean) * (x - mean)) / (values.Length - 1);
}
static double StdDev(double[] values) => System.Math.Sqrt(Variance(values));
static double Relative(double a, double b) => System.Math.Abs(a - b) /
    System.Math.Max(1, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

static double InverseNormalCdf(double p)
{
    double[] a = [-3.969683028665376e+01, 2.209460984245205e+02, -2.759285104469687e+02, 1.383577518672690e+02, -3.066479806614716e+01, 2.506628277459239e+00];
    double[] b = [-5.447609879822406e+01, 1.615858368580409e+02, -1.556989798598866e+02, 6.680131188771972e+01, -1.328068155288572e+01];
    double[] c = [-7.784894002430293e-03, -3.223964580411365e-01, -2.400758277161838e+00, -2.549732539343734e+00, 4.374664141464968e+00, 2.938163982698783e+00];
    double[] d = [7.784695709041462e-03, 3.224671290700398e-01, 2.445134137142996e+00, 3.754408661907416e+00];
    const double lower = 0.02425;
    const double upper = 1 - lower;
    if (p < lower)
    {
        double q = System.Math.Sqrt(-2 * System.Math.Log(p));
        return (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
            ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
    }
    if (p > upper)
    {
        double q = System.Math.Sqrt(-2 * System.Math.Log(1 - p));
        return -(((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) /
            ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
    }
    double r = p - 0.5, s = r * r;
    return (((((a[0] * s + a[1]) * s + a[2]) * s + a[3]) * s + a[4]) * s + a[5]) * r /
        (((((b[0] * s + b[1]) * s + b[2]) * s + b[3]) * s + b[4]) * s + 1);
}

sealed record BindingSpec(string Id, string Path, string ExpectedSha256);
sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record SeedFamily(string Id, int SeedOffset, int[] Seeds, double[] InitialScales);
sealed record ChainResult(double[] Samples, double[] Second, double[] XGradient, double[] DeltaH,
    int Accepted, int Total, int NonFinite, int Divergences);
sealed record FamilyResult(string Id, bool Passed, double AcceptanceRate, int NonFiniteCount, int DivergenceCount,
    double ExpMinusDeltaHMean, double ExpMinusDeltaHSe, double StateSplitRankNormalizedRhat,
    double SecondMomentSplitRankNormalizedRhat, double XGradientSplitRankNormalizedRhat,
    double StatePairedInitialPositiveMonotoneEss, double SecondMomentPairedInitialPositiveMonotoneEss,
    double XGradientPairedInitialPositiveMonotoneEss, double Mean, double MeanSe,
    double SecondMoment, double SecondMomentSe, double XGradientMean, double XGradientSe, bool FiniteGate,
    bool DivergenceGate, bool AcceptanceGate, bool EnergyGate, bool StateConvergenceGate,
    bool SecondMomentConvergenceGate, bool XGradientConvergenceGate, bool StateEssGate,
    bool SecondMomentEssGate, bool XGradientEssGate, bool QuadratureMomentGate, bool IntegrationByPartsGate);
readonly record struct QuadratureResult(double Z, double M1, double M2, double M3, double M4);
