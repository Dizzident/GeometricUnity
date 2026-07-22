using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase538_fixed_grid_interacting_hmc_retuning_001";
const string ContractPath = Root + "/preregistration/phase538_fixed_grid_interacting_hmc_retuning_contract_v2.json";
const string OutputPath = Root + "/output/fixed_grid_interacting_hmc_retuning.json";
const string SummaryPath = Root + "/output/fixed_grid_interacting_hmc_retuning_summary.json";

var fullTimer = Stopwatch.StartNew();
TimeSpan fullCpuStart = Process.GetCurrentProcess().TotalProcessorTime;

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;

var expectedBindings = new (string Id, string Path)[]
{
    ("phase533-contract", "studies/phase533_nested_validation_contract_001/preregistration/phase533_nested_validation_contract_v1.json"),
    ("phase533-summary", "studies/phase533_nested_validation_contract_001/output/nested_validation_contract_summary.json"),
    ("phase534-contract", "studies/phase534_nested_control_battery_001/preregistration/phase534_nested_control_contract_v1.json"),
    ("phase534-program", "studies/phase534_nested_control_battery_001/Program.cs"),
    ("phase534-summary", "studies/phase534_nested_control_battery_001/output/nested_control_battery_summary.json"),
    ("phase535-summary", "studies/phase535_bounded_registered_operator_pilot_adjudicator_001/output/bounded_registered_operator_pilot_adjudicator_summary.json"),
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

var grid = contract.GetProperty("fixedGrid").EnumerateArray().Select(x => new GridRow(
    x.GetProperty("rowId").GetString()!, x.GetProperty("stepSize").GetDouble(),
    x.GetProperty("trajectoryLength").GetDouble(), x.GetProperty("leapfrogSteps").GetInt32())).ToArray();
var families = contract.GetProperty("seedFamilies").EnumerateArray().Select(x => new SeedFamily(
    x.GetProperty("id").GetString()!, x.GetProperty("sourceTable").GetString()!, x.GetProperty("seedOffset").GetInt32(),
    x.GetProperty("seeds").EnumerateArray().Select(y => y.GetInt32()).ToArray(),
    x.GetProperty("initialScales").EnumerateArray().Select(y => y.GetDouble()).ToArray())).ToArray();
string[] expectedRowIds =
[
    "eps-0.20-len-1.60", "eps-0.25-len-2.00", "eps-0.30-len-1.80", "eps-0.35-len-2.10",
    "eps-0.40-len-2.40", "eps-0.45-len-2.70", "eps-0.50-len-2.50", "eps-0.55-len-2.20",
];
string[] expectedFamilyIds = ["original-table-a", "original-table-b", "preregistered-disjoint-third"];
bool gridValid = grid.Length == 8
    && grid.Select(x => x.RowId).SequenceEqual(expectedRowIds, StringComparer.Ordinal)
    && grid.Select(x => x.RowId).Distinct(StringComparer.Ordinal).Count() == 8
    && grid.All(x => x.StepSize > 0 && x.LeapfrogSteps > 0
        && System.Math.Abs(x.StepSize * x.LeapfrogSteps - x.TrajectoryLength) <= 1e-12);
bool familiesValid = families.Select(x => x.Id).SequenceEqual(expectedFamilyIds, StringComparer.Ordinal)
    && families.All(x => x.Seeds.Length == 4 && x.InitialScales.Length == 4)
    && families.SelectMany(x => x.Seeds).Distinct().Count() == 12
    && families.SelectMany(x => x.Seeds.Select(seed => seed + x.SeedOffset)).Distinct().Count() == 12;

JsonElement execution = contract.GetProperty("execution");
JsonElement gates = contract.GetProperty("gates");
JsonElement resources = contract.GetProperty("resourceRefusal");
string[] expectedPrecedence =
[
    "invalid-or-drifted-input", "resource-refusal", "no-stable-fixed-grid-row",
    "post-review-hardened-stable-fixed-grid-row-reduced-target-feasible",
];
string[] expectedFirewallKeys =
[
    "phase534ReinterpretedOrRelabeled", "phase535PilotExecutedOrReopened", "completeLatticeValidated",
    "productionDefaultSelected", "phase481PackCreatedOrMutated", "phase458G3Satisfied",
    "phase458G4Satisfied", "phase458G5Satisfied", "o4Discharged", "sourceContractApplicationAllowed",
    "physicalUnitOrGevClaimAllowed", "productionOrLaunchAllowed",
];
JsonElement lineage = contract.GetProperty("lineage");
bool lineageValid =
    lineage.GetProperty("originalContractPath").GetString() == Root + "/preregistration/pre_review_non_citable_phase538_fixed_grid_interacting_hmc_retuning_contract_v1.json"
    && lineage.GetProperty("originalContractSha256").GetString() == "b33f473ac2cd41d34ebb02876ddba234173f19c186260bc1c80089eee74a948a"
    && Sha256(lineage.GetProperty("originalContractPath").GetString()!) == lineage.GetProperty("originalContractSha256").GetString()
    && Sha256(lineage.GetProperty("originalFullOutputPath").GetString()!) == lineage.GetProperty("originalOutputSha256").GetString()
    && Sha256(lineage.GetProperty("originalSummaryOutputPath").GetString()!) == lineage.GetProperty("originalOutputSha256").GetString()
    && lineage.GetProperty("gridUnchanged").GetBoolean()
    && lineage.GetProperty("seedIntegersAndInitialScalesUnchanged").GetBoolean()
    && lineage.GetProperty("numericalThresholdsUnchanged").GetBoolean()
    && lineage.GetProperty("diagnosticDefinitionsStrengthenedAfterAdversarialReview").GetBoolean()
    && !lineage.GetProperty("originalArtifactsCitable").GetBoolean();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase538-a23-fixed-grid-interacting-hmc-retuning-v2-post-review-hardened"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A23"
    && !contract.GetProperty("frozenBeforeFirstRun").GetBoolean()
    && contract.GetProperty("frozenBeforeCorrectedRun").GetBoolean()
    && contract.GetProperty("postReviewDiagnosticHardening").GetBoolean()
    && lineageValid
    && exactBindingsValid && gridValid && familiesValid
    && !execution.GetProperty("adaptationEnabled").GetBoolean()
    && contract.GetProperty("selection").GetProperty("rule").GetString() == "lexicographically-first-fixed-grid-row-passing-every-gate-on-every-seed-family"
    && !contract.GetProperty("selection").GetProperty("observedMomentsMayReorderRows").GetBoolean()
    && contract.GetProperty("selection").GetProperty("allRowsExecutedBeforeSelection").GetBoolean()
    && contract.GetProperty("selection").GetProperty("thirdFamilyParticipatesInSelection").GetBoolean()
    && !contract.GetProperty("selection").GetProperty("thirdFamilyIsIndependentPostSelectionValidation").GetBoolean()
    && !contract.GetProperty("selection").GetProperty("physicalTargetsConsulted").GetBoolean()
    && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedPrecedence)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

using var p533ContractDocument = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[0].Path));
using var p533SummaryDocument = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[1].Path));
using var p534ContractDocument = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[2].Path));
using var p534SummaryDocument = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[4].Path));
using var p535SummaryDocument = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[5].Path));
JsonElement p533Contract = p533ContractDocument.RootElement;
JsonElement p533Summary = p533SummaryDocument.RootElement;
JsonElement p534Contract = p534ContractDocument.RootElement;
JsonElement p534Summary = p534SummaryDocument.RootElement;
JsonElement p535Summary = p535SummaryDocument.RootElement;
JsonElement reduced = p534Summary.GetProperty("reducedInteractingControl");

bool precursorSemanticsValid =
    p533Contract.GetProperty("contractId").GetString() == "phase533-a22-nested-validation-contract-v1"
    && p533Summary.GetProperty("verdictKind").GetString() == "nested-validation-contract-closed"
    && p534Contract.GetProperty("contractId").GetString() == "phase534-a22-nested-control-battery-v1"
    && p534Summary.GetProperty("verdictKind").GetString() == "reduced-interacting-control-failed"
    && reduced.GetProperty("polynomial").GetProperty("polynomialReplayPassed").GetBoolean()
    && reduced.GetProperty("quadrature").GetProperty("passed").GetBoolean()
    && reduced.GetProperty("reducedMomentAgreement").GetBoolean()
    && reduced.GetProperty("schwingerDysonPassed").GetBoolean()
    && !reduced.GetProperty("samplerDiagnosticsPassed").GetBoolean()
    && p535Summary.GetProperty("verdictKind").GetString() == "reduced-interacting-control-failed"
    && p535Summary.GetProperty("pilotRefusedByUpstreamControl").GetBoolean()
    && !p535Summary.GetProperty("pilotRun").GetBoolean();

double c2 = reduced.GetProperty("polynomial").GetProperty("c2").GetDouble();
double c3 = reduced.GetProperty("polynomial").GetProperty("c3").GetDouble();
double c4 = reduced.GetProperty("polynomial").GetProperty("c4").GetDouble();
double expectedC3 = p534Contract.GetProperty("registeredReduction").GetProperty("expectedCubicCoefficient").GetDouble();
double coefficientTolerance = contract.GetProperty("target").GetProperty("coefficientTolerance").GetDouble();
bool targetValid = c2 > 0 && c4 > 0 && System.Math.Abs(c3 - expectedC3) <= coefficientTolerance
    && contract.GetProperty("target").GetProperty("member").GetString() == "sd2-id0/c0.5"
    && contract.GetProperty("target").GetProperty("extent").GetInt32() == 3
    && contract.GetProperty("target").GetProperty("thetaRule").GetString() == "theta-identically-zero";

double Potential(double x) => c2 * x * x + c3 * x * x * x + c4 * x * x * x * x;
double Gradient(double x) => 2 * c2 * x + 3 * c3 * x * x + 4 * c4 * x * x * x;
JsonElement quadratureSpec = contract.GetProperty("quadrature");
QuadratureResult reference = IntegrateMoments(Potential,
    quadratureSpec.GetProperty("halfWidth").GetDouble(), quadratureSpec.GetProperty("panels").GetInt32());
JsonElement p534Reference = reduced.GetProperty("quadrature").GetProperty("fine");
double referenceDifference = new[]
{
    Relative(reference.M1, p534Reference.GetProperty("m1").GetDouble()),
    Relative(reference.M2, p534Reference.GetProperty("m2").GetDouble()),
    Relative(reference.M3, p534Reference.GetProperty("m3").GetDouble()),
    Relative(reference.M4, p534Reference.GetProperty("m4").GetDouble()),
}.Max();
bool quadratureReferenceValid = double.IsFinite(reference.Z) && reference.Z > 0
    && referenceDifference <= quadratureSpec.GetProperty("referenceMomentTolerance").GetDouble();

double estimatedCpuSeconds = resources.GetProperty("estimatedAggregateCpuSeconds").GetDouble();
long estimatedPeakBytes = resources.GetProperty("estimatedPeakBytes").GetInt64();
double maximumCpuSeconds = resources.GetProperty("maximumAggregateCpuSeconds").GetDouble();
long maximumPeakBytes = resources.GetProperty("maximumPeakBytes").GetInt64();
bool resourceEstimateWithinBounds = estimatedCpuSeconds <= maximumCpuSeconds && estimatedPeakBytes <= maximumPeakBytes;
bool inputsValid = contractValid && precursorSemanticsValid && targetValid && quadratureReferenceValid;

int warmup = execution.GetProperty("warmupPerChain").GetInt32();
int retained = execution.GetProperty("retainedPerChain").GetInt32();
double divergenceThreshold = execution.GetProperty("divergenceAbsoluteDeltaH").GetDouble();
var kernelTimer = Stopwatch.StartNew();
TimeSpan kernelCpuStart = Process.GetCurrentProcess().TotalProcessorTime;
var rowResults = new List<GridResult>();
if (inputsValid && resourceEstimateWithinBounds)
{
    foreach (GridRow row in grid)
    {
        var familyResults = new List<FamilyResult>();
        foreach (SeedFamily family in families)
        {
            var chains = new List<ChainResult>();
            for (int i = 0; i < family.Seeds.Length; i++)
                chains.Add(RunScalarHmc(family.Seeds[i] + family.SeedOffset, family.InitialScales[i], row.StepSize,
                    warmup, retained, row.LeapfrogSteps, divergenceThreshold, Potential, Gradient));
            familyResults.Add(SummarizeFamily(family.Id, chains, reference, gates));
        }
        rowResults.Add(new GridResult(row.RowId, row.StepSize, row.TrajectoryLength, row.LeapfrogSteps,
            familyResults.All(x => x.Passed), familyResults.ToArray()));
    }
}
double kernelCpuSeconds = (Process.GetCurrentProcess().TotalProcessorTime - kernelCpuStart).TotalSeconds;
kernelTimer.Stop();
double aggregateCpuSeconds = (Process.GetCurrentProcess().TotalProcessorTime - fullCpuStart).TotalSeconds;
fullTimer.Stop();
long peakWorkingSetBytes = Process.GetCurrentProcess().PeakWorkingSet64;
bool measuredResourceBoundsPassed = aggregateCpuSeconds <= maximumCpuSeconds && peakWorkingSetBytes <= maximumPeakBytes;
GridResult? selected = rowResults.FirstOrDefault(x => x.Passed);

string verdict = !inputsValid ? "invalid-or-drifted-input"
    : !resourceEstimateWithinBounds || !measuredResourceBoundsPassed ? "resource-refusal"
    : selected is null ? "no-stable-fixed-grid-row"
    : "post-review-hardened-stable-fixed-grid-row-reduced-target-feasible";
var result = new
{
    schemaVersion = 1,
    phase = 538,
    phaseId = "phase538-fixed-grid-interacting-hmc-retuning",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha256(ContractPath),
    planSection = contract.GetProperty("planSection").GetString(),
    inputsValid,
    contractValid,
    lineageValid,
    postReviewDiagnosticHardening = true,
    pristinePreregistrationClaimed = false,
    bindingInventoryValid,
    exactBindingsValid,
    gridValid,
    familiesValid,
    precursorSemanticsValid,
    targetValid,
    quadratureReferenceValid,
    referenceDifference,
    target = new { c2, c3, c4, quadrature = reference },
    execution = new
    {
        adaptationEnabled = false,
        gridRowCount = grid.Length,
        seedFamilyCount = families.Length,
        chainCountPerFamily = 4,
        warmupPerChain = warmup,
        retainedPerChain = retained,
        allRowsExecutedBeforeSelection = true,
    },
    resource = new
    {
        resourceEstimateWithinBounds,
        measuredResourceBoundsPassed,
        estimatedCpuSeconds,
        maximumCpuSeconds,
        estimatedPeakBytes,
        maximumPeakBytes,
        volatileMeasurementsSerialized = false,
    },
    rowResults,
    selection = new
    {
        rule = "lexicographically-first-fixed-grid-row-passing-every-gate-on-every-seed-family",
        stableRowCount = rowResults.Count(x => x.Passed),
        selectedRowId = selected?.RowId,
        physicalTargetsConsulted = false,
        observationsReorderedRows = false,
        thirdFamilyParticipatesInSelection = true,
        thirdFamilyIsIndependentPostSelectionValidation = false,
    },
    verdictKind = verdict,
    terminalStatus = "fixed-grid-interacting-hmc-retuning-" + verdict,
    decision = selected is not null
        ? "Under the post-review-hardened v2 diagnostic definitions, at least one fixed non-adaptive row passes every execution, convergence, quadrature-moment, and integration-by-parts gate on both original seed tables and the preregistered disjoint third family. The grid, seeds, and thresholds are unchanged from v1, but this is not pristine preregistration and establishes reduced-target feasibility only."
        : "The post-review-hardened v2 diagnostics preserve a negative or refusal terminal without adapting the unchanged grid, seeds, thresholds, or resources.",
    preReviewArtifacts = new
    {
        citable = false,
        purpose = "preserved first-run bytes superseded by adversarial diagnostic repair",
        contractPath = Root + "/preregistration/pre_review_non_citable_phase538_fixed_grid_interacting_hmc_retuning_contract_v1.json",
        contractSha256 = "b33f473ac2cd41d34ebb02876ddba234173f19c186260bc1c80089eee74a948a",
        fullPath = Root + "/output/pre_review_non_citable_fixed_grid_interacting_hmc_retuning.json",
        summaryPath = Root + "/output/pre_review_non_citable_fixed_grid_interacting_hmc_retuning_summary.json",
        sha256 = "f4dd6c92cf4dbaa3e08a40d10f835faf8f6584d45de12ff1098a3a4aff6754ef",
    },
    protocolLineage = new
    {
        v1FrozenBeforeFirstRun = true,
        v2FrozenBeforeCorrectedRun = true,
        v2FrozenBeforeFirstRun = false,
        gridUnchanged = true,
        seedsAndInitialScalesUnchanged = true,
        numericalThresholdsUnchanged = true,
        diagnosticDefinitionsStrengthenedAfterAdversarialReview = true,
    },
    reducedTargetFeasibilityOnly = selected is not null,
    phase534ReinterpretedOrRelabeled = false,
    phase535PilotExecutedOrReopened = false,
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

Require(result.promotedPhysicalMassClaimCount == 0 && !result.allDownstreamAuthority, "Phase538 authority firewall failed.");

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase538 verdict: {verdict}");
Console.WriteLine($"stable rows: {rowResults.Count(x => x.Passed)}/{grid.Length}; selected={selected?.RowId ?? "none"}");
Console.WriteLine($"aggregate cpu seconds: {aggregateCpuSeconds:F3}; kernel cpu seconds: {kernelCpuSeconds:F3}; peak bytes: {peakWorkingSetBytes}");

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
    double[] samples = chains.SelectMany(x => x.Samples).ToArray();
    double[] second = chains.SelectMany(x => x.Second).ToArray();
    double[] xGradient = chains.SelectMany(x => x.XGradient).ToArray();
    double[] finiteDeltaH = chains.SelectMany(x => x.DeltaH).Where(double.IsFinite).ToArray();
    double[] expMinusDeltaH = finiteDeltaH.Select(x => System.Math.Exp(-x)).ToArray();
    bool energyFinite = expMinusDeltaH.Length == chains.Sum(x => x.Total) && expMinusDeltaH.All(double.IsFinite);
    double expMean = energyFinite ? Mean(expMinusDeltaH) : double.NaN;
    double expSe = energyFinite ? StdDev(expMinusDeltaH) / System.Math.Sqrt(expMinusDeltaH.Length) : double.NaN;
    double[][] secondChains = chains.Select(x => x.Second).ToArray();
    double[][] xGradientChains = chains.Select(x => x.XGradient).ToArray();
    double stateEss = PairedInitialPositiveMonotoneEss(sampleChains);
    double secondMomentEss = PairedInitialPositiveMonotoneEss(secondChains);
    double xGradientEss = PairedInitialPositiveMonotoneEss(xGradientChains);
    double mean = Mean(samples), meanSe = StdDev(samples) / System.Math.Sqrt(System.Math.Max(1, stateEss));
    double moment2 = Mean(second), moment2Se = StdDev(second) / System.Math.Sqrt(System.Math.Max(1, secondMomentEss));
    double ibp = Mean(xGradient), ibpSe = StdDev(xGradient) / System.Math.Sqrt(System.Math.Max(1, xGradientEss));
    double stateRhat = SplitRankNormalizedRhat(sampleChains);
    double secondMomentRhat = SplitRankNormalizedRhat(secondChains);
    double xGradientRhat = SplitRankNormalizedRhat(xGradientChains);
    double acceptance = chains.Sum(x => x.Accepted) / (double)chains.Sum(x => x.Total);
    int nonfinite = chains.Sum(x => x.NonFinite);
    int divergences = chains.Sum(x => x.Divergences);
    double quadratureTolerance = gates.GetProperty("quadratureAgreementTolerance").GetDouble();
    double momentMultiplier = gates.GetProperty("momentStandardErrorMultiplier").GetDouble();
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
    bool secondMomentConvergenceGate = double.IsFinite(secondMomentRhat) && secondMomentRhat <= rhatMaximum;
    bool xGradientConvergenceGate = double.IsFinite(xGradientRhat) && xGradientRhat <= rhatMaximum;
    bool stateEssGate = double.IsFinite(stateEss) && stateEss >= essMinimum;
    bool secondMomentEssGate = double.IsFinite(secondMomentEss) && secondMomentEss >= essMinimum;
    bool xGradientEssGate = double.IsFinite(xGradientEss) && xGradientEss >= essMinimum;
    bool quadratureMomentGate = System.Math.Abs(mean - reference.M1) <= momentMultiplier * meanSe + quadratureTolerance
        && System.Math.Abs(moment2 - reference.M2) <= momentMultiplier * moment2Se + quadratureTolerance;
    bool integrationByPartsGate = System.Math.Abs(ibp - gates.GetProperty("integrationByPartsTarget").GetDouble())
        <= momentMultiplier * ibpSe + quadratureTolerance;
    bool passed = finiteGate && divergenceGate && acceptanceGate && energyGate
        && stateConvergenceGate && secondMomentConvergenceGate && xGradientConvergenceGate
        && stateEssGate && secondMomentEssGate && xGradientEssGate
        && quadratureMomentGate && integrationByPartsGate;
    return new(id, passed, acceptance, nonfinite, divergences, expMean, expSe,
        stateRhat, secondMomentRhat, xGradientRhat, stateEss, secondMomentEss, xGradientEss,
        mean, meanSe, moment2, moment2Se, ibp, ibpSe, finiteGate, divergenceGate, acceptanceGate,
        energyGate, stateConvergenceGate, secondMomentConvergenceGate, xGradientConvergenceGate,
        stateEssGate, secondMomentEssGate, xGradientEssGate, quadratureMomentGate, integrationByPartsGate);
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
    double[] foldedPooled = foldedSource.SelectMany(x => x).ToArray();
    double[] foldedRanked = RankNormalize(foldedPooled);
    double[][] folded = Enumerable.Range(0, split.Length).Select(i => foldedRanked.Skip(i * n).Take(n).ToArray()).ToArray();
    return System.Math.Max(BasicRhat(normalized), BasicRhat(folded));
}

static double BasicRhat(double[][] chains)
{
    int m = chains.Length, n = chains.Min(x => x.Length);
    double[] means = chains.Select(x => x.Take(n).Average()).ToArray();
    double within = chains.Select(x => Variance(x.Take(n).ToArray())).Average();
    if (!(within > 0)) return double.PositiveInfinity;
    double between = n * Variance(means);
    return System.Math.Sqrt((((n - 1.0) / n) * within + between / n) / within);
}

static double[][] SplitChains(double[][] chains)
{
    int half = chains.Min(x => x.Length) / 2;
    if (half < 2) return chains;
    return chains.SelectMany(chain => new[] { chain.Take(half).ToArray(), chain.Skip(chain.Length - half).Take(half).ToArray() }).ToArray();
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
    double pairSum = 0;
    double previousPair = double.PositiveInfinity;
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
static double Relative(double a, double b) => System.Math.Abs(a - b) / System.Math.Max(1, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }

// Acklam's rational approximation, used only to rank-normalize diagnostics.
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
    double r = p - 0.5;
    double s = r * r;
    return (((((a[0] * s + a[1]) * s + a[2]) * s + a[3]) * s + a[4]) * s + a[5]) * r /
        (((((b[0] * s + b[1]) * s + b[2]) * s + b[3]) * s + b[4]) * s + 1);
}

sealed record BindingSpec(string Id, string Path, string ExpectedSha256);
sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record GridRow(string RowId, double StepSize, double TrajectoryLength, int LeapfrogSteps);
sealed record SeedFamily(string Id, string SourceTable, int SeedOffset, int[] Seeds, double[] InitialScales);
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
sealed record GridResult(string RowId, double StepSize, double TrajectoryLength, int LeapfrogSteps,
    bool Passed, FamilyResult[] Families);
readonly record struct QuadratureResult(double Z, double M1, double M2, double M3, double M4);
