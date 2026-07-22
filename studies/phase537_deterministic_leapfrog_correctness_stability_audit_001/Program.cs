using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase537_deterministic_leapfrog_correctness_stability_audit_001";
const string ContractPath = Root + "/preregistration/phase537_deterministic_leapfrog_correctness_stability_audit_contract_v1.json";
const string OutputPath = Root + "/output/deterministic_leapfrog_correctness_stability_audit.json";
const string SummaryPath = Root + "/output/deterministic_leapfrog_correctness_stability_audit_summary.json";

static string Sha256(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string S(JsonElement element, string name) => element.GetProperty(name).GetString() ?? "";
static bool B(JsonElement element, string name) => element.GetProperty(name).ValueKind == JsonValueKind.True;
static string[] Strings(JsonElement element, string name) => element.GetProperty(name).EnumerateArray().Select(x => x.GetString()!).ToArray();
static double[] Doubles(JsonElement element, string name) => element.GetProperty(name).EnumerateArray().Select(x => x.GetDouble()).ToArray();
static int[] Ints(JsonElement element, string name) => element.GetProperty(name).EnumerateArray().Select(x => x.GetInt32()).ToArray();
static bool SameDouble(double left, double right) => BitConverter.DoubleToInt64Bits(left) == BitConverter.DoubleToInt64Bits(right);
static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

using var contractDoc = JsonDocument.Parse(File.ReadAllText(ContractPath));
JsonElement contract = contractDoc.RootElement;
var expectedBindings = new[]
{
    ("phase533-contract", "studies/phase533_nested_validation_contract_001/preregistration/phase533_nested_validation_contract_v1.json"),
    ("phase533-summary", "studies/phase533_nested_validation_contract_001/output/nested_validation_contract_summary.json"),
    ("phase534-contract", "studies/phase534_nested_control_battery_001/preregistration/phase534_nested_control_contract_v1.json"),
    ("phase534-program", "studies/phase534_nested_control_battery_001/Program.cs"),
    ("phase534-summary", "studies/phase534_nested_control_battery_001/output/nested_control_battery_summary.json"),
    ("phase535-summary", "studies/phase535_bounded_registered_operator_pilot_adjudicator_001/output/bounded_registered_operator_pilot_adjudicator_summary.json"),
};
var bindingRows = contract.GetProperty("exactBindings").EnumerateArray().ToArray();
var bindings = bindingRows.Select(row =>
{
    string path = S(row, "path");
    string expected = S(row, "sha256");
    string actual = File.Exists(path) ? Sha256(path) : "missing";
    return new { id = S(row, "id"), path, expectedSha256 = expected, actualSha256 = actual, hashMatches = actual == expected };
}).ToArray();
bool bindingInventoryValid = bindings.Length == expectedBindings.Length
    && bindings.Select(x => (x.id, x.path)).SequenceEqual(expectedBindings)
    && bindings.All(x => x.expectedSha256.Length == 64);
bool exactBindingsValid = bindingInventoryValid && bindings.All(x => x.hashMatches);

using var phase533Doc = JsonDocument.Parse(File.ReadAllText(bindings.Single(x => x.id == "phase533-summary").path));
using var phase534Doc = JsonDocument.Parse(File.ReadAllText(bindings.Single(x => x.id == "phase534-summary").path));
using var phase535Doc = JsonDocument.Parse(File.ReadAllText(bindings.Single(x => x.id == "phase535-summary").path));
JsonElement p533 = phase533Doc.RootElement;
JsonElement p534 = phase534Doc.RootElement;
JsonElement p535 = phase535Doc.RootElement;

string[] expectedTaxonomy =
{
    "invalid-or-drifted-input",
    "phase534-polynomial-mismatch",
    "analytic-gradient-disagrees-with-finite-difference",
    "forward-reverse-recovery-failed",
    "energy-step-halving-order-failed",
    "deterministic-leapfrog-audit-passed",
};
string[] expectedFirewallNames =
{
    "phase534RepairedOrRelabeled", "phase535PilotReopened",
    "samplingOrMixingValidityEstablished", "phase481PackConstructedOrMutated",
    "phase458G3Satisfied", "phase458G4Satisfied", "phase458G5Satisfied",
    "o4Satisfied", "samplingOrReprocessingAllowed", "productionOrLaunchAllowed",
    "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed",
};
JsonElement firewalls = contract.GetProperty("authorityFirewalls");
JsonElement frozenPolynomial = contract.GetProperty("exactPhase534Polynomial");
JsonElement frozenGradient = contract.GetProperty("gradientAudit");
JsonElement frozenLeapfrog = contract.GetProperty("leapfrogAudit");
double[] expectedGradientStates = { -3.0, -2.0, -1.0, -0.5, -0.125, 0.0, 0.125, 0.5, 1.0, 2.0, 3.0 };
double[] expectedLeapfrogStates = { -1.5, -0.75, 0.0, 0.75, 1.5 };
double[] expectedMomenta = { -1.25, -0.5, 0.5, 1.25 };
double[] expectedStepSizes = { 0.1, 0.05, 0.025, 0.0125 };
int[] expectedStepCounts = { 8, 16, 32, 64 };
bool frozenNumericalMenuValid =
    SameDouble(frozenPolynomial.GetProperty("c2").GetDouble(), 1.7916666666666679)
    && SameDouble(frozenPolynomial.GetProperty("c3").GetDouble(), 0.036111111111111316)
    && SameDouble(frozenPolynomial.GetProperty("c4").GetDouble(), 0.1704947916666657)
    && frozenPolynomial.GetProperty("c2Binary64Bits").GetInt64() == 4610747768505019056L
    && frozenPolynomial.GetProperty("c3Binary64Bits").GetInt64() == 4585364980605200368L
    && frozenPolynomial.GetProperty("c4Binary64Bits").GetInt64() == 4595310742532284272L
    && Doubles(frozenGradient, "states").SequenceEqual(expectedGradientStates)
    && SameDouble(frozenGradient.GetProperty("relativeStep").GetDouble(), 1e-6)
    && SameDouble(frozenGradient.GetProperty("scaledTolerance").GetDouble(), 5e-9)
    && Doubles(frozenLeapfrog, "states").SequenceEqual(expectedLeapfrogStates)
    && Doubles(frozenLeapfrog, "momenta").SequenceEqual(expectedMomenta)
    && SameDouble(frozenLeapfrog.GetProperty("trajectoryLength").GetDouble(), 0.8)
    && Doubles(frozenLeapfrog, "stepSizes").SequenceEqual(expectedStepSizes)
    && Ints(frozenLeapfrog, "stepCounts").SequenceEqual(expectedStepCounts)
    && SameDouble(frozenLeapfrog.GetProperty("forwardReverseScaledTolerance").GetDouble(), 5e-11)
    && SameDouble(frozenLeapfrog.GetProperty("maximumAdjacentRmsErrorRatio").GetDouble(), 0.45)
    && SameDouble(frozenLeapfrog.GetProperty("minimumCoarseToFineRmsImprovement").GetDouble(), 12.0)
    && SameDouble(frozenLeapfrog.GetProperty("minimumAdjacentObservedOrder").GetDouble(), 1.15);
bool contractValid =
    contract.GetProperty("schemaVersion").GetInt32() == 1
    && S(contract, "contractId") == "phase537-a23-deterministic-leapfrog-correctness-stability-audit-v1"
    && S(contract, "planSection") == "WAVE2_AMENDMENTS_2026-07-12 A23"
    && B(contract, "frozenBeforeFirstExecution")
    && Strings(contract, "terminalTaxonomyInPrecedenceOrder").SequenceEqual(expectedTaxonomy)
    && S(contract, "expectedCurrentVerdict") == "deterministic-leapfrog-audit-passed"
    && S(contract, "expectedCurrentTerminalStatus") == "deterministic-leapfrog-correctness-stability-audit-deterministic-leapfrog-audit-passed"
    && firewalls.EnumerateObject().Select(x => x.Name).Order(StringComparer.Ordinal)
        .SequenceEqual(expectedFirewallNames.Order(StringComparer.Ordinal), StringComparer.Ordinal)
    && expectedFirewallNames.All(name => firewalls.GetProperty(name).ValueKind == JsonValueKind.False)
    && frozenNumericalMenuValid
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

bool upstreamSemanticsValid =
    S(p533, "verdictKind") == "nested-validation-contract-closed"
    && S(p534, "verdictKind") == "reduced-interacting-control-failed"
    && S(p535, "verdictKind") == "reduced-interacting-control-failed"
    && !B(p534, "phase534Passed")
    && !B(p535, "pilotRun")
    && B(p535, "pilotRefusedByUpstreamControl");
bool inputsValid = contractValid && exactBindingsValid && upstreamSemanticsValid;

JsonElement polynomialSpec = contract.GetProperty("exactPhase534Polynomial");
double c2 = polynomialSpec.GetProperty("c2").GetDouble();
double c3 = polynomialSpec.GetProperty("c3").GetDouble();
double c4 = polynomialSpec.GetProperty("c4").GetDouble();
JsonElement phase534Polynomial = p534.GetProperty("reducedInteractingControl").GetProperty("polynomial");
bool polynomialBitsMatch =
    BitConverter.DoubleToInt64Bits(c2) == polynomialSpec.GetProperty("c2Binary64Bits").GetInt64()
    && BitConverter.DoubleToInt64Bits(c3) == polynomialSpec.GetProperty("c3Binary64Bits").GetInt64()
    && BitConverter.DoubleToInt64Bits(c4) == polynomialSpec.GetProperty("c4Binary64Bits").GetInt64()
    && BitConverter.DoubleToInt64Bits(c2) == BitConverter.DoubleToInt64Bits(phase534Polynomial.GetProperty("c2").GetDouble())
    && BitConverter.DoubleToInt64Bits(c3) == BitConverter.DoubleToInt64Bits(phase534Polynomial.GetProperty("c3").GetDouble())
    && BitConverter.DoubleToInt64Bits(c4) == BitConverter.DoubleToInt64Bits(phase534Polynomial.GetProperty("c4").GetDouble());
bool phase534FormulaAnchorsPresent = File.ReadAllText(bindings.Single(x => x.id == "phase534-program").path)
    .Contains("double Potential(double x) => c2 * x * x + c3 * x * x * x + c4 * x * x * x * x;", StringComparison.Ordinal)
    && File.ReadAllText(bindings.Single(x => x.id == "phase534-program").path)
        .Contains("double Gradient(double x) => 2 * c2 * x + 3 * c3 * x * x + 4 * c4 * x * x * x;", StringComparison.Ordinal);
bool polynomialValid = polynomialBitsMatch && phase534FormulaAnchorsPresent && c2 > 0.0 && c4 > 0.0;

double Potential(double x) => c2 * x * x + c3 * x * x * x + c4 * x * x * x * x;
double Gradient(double x) => 2.0 * c2 * x + 3.0 * c3 * x * x + 4.0 * c4 * x * x * x;
double Hamiltonian(double x, double p) => Potential(x) + 0.5 * p * p;

JsonElement gradientSpec = contract.GetProperty("gradientAudit");
double relativeStep = gradientSpec.GetProperty("relativeStep").GetDouble();
double gradientTolerance = gradientSpec.GetProperty("scaledTolerance").GetDouble();
var gradientRows = Doubles(gradientSpec, "states").Select(x =>
{
    double h = relativeStep * System.Math.Max(1.0, System.Math.Abs(x));
    double analytic = Gradient(x);
    double finiteDifference = (Potential(x + h) - Potential(x - h)) / (2.0 * h);
    double scaledError = System.Math.Abs(analytic - finiteDifference) / System.Math.Max(1.0, System.Math.Abs(analytic));
    return new { x, h, analytic, finiteDifference, scaledError, passed = double.IsFinite(scaledError) && scaledError <= gradientTolerance };
}).ToArray();
bool gradientAuditPassed = gradientRows.Length == 11 && gradientRows.All(x => x.passed);

JsonElement leapfrogSpec = contract.GetProperty("leapfrogAudit");
double[] states = Doubles(leapfrogSpec, "states");
double[] momenta = Doubles(leapfrogSpec, "momenta");
double[] stepSizes = Doubles(leapfrogSpec, "stepSizes");
int[] stepCounts = Ints(leapfrogSpec, "stepCounts");
double trajectoryLength = leapfrogSpec.GetProperty("trajectoryLength").GetDouble();
bool ladderShapeValid = stepSizes.Length == 4 && stepCounts.Length == 4
    && stepSizes.Zip(stepCounts).All(pair => System.Math.Abs(pair.First * pair.Second - trajectoryLength) <= 1e-15)
    && Enumerable.Range(0, 3).All(i => stepSizes[i + 1] == 0.5 * stepSizes[i] && stepCounts[i + 1] == 2 * stepCounts[i]);

var reversibilityRows = new List<object>();
double maximumRecoveryError = 0.0;
foreach (double x0 in states)
foreach (double p0 in momenta)
foreach ((double step, int count) in stepSizes.Zip(stepCounts))
{
    (double x1, double p1) = Leapfrog(x0, p0, step, count, Gradient);
    (double xr, double pr) = Leapfrog(x1, p1, -step, count, Gradient);
    double scaledError = System.Math.Max(System.Math.Abs(xr - x0), System.Math.Abs(pr - p0))
        / System.Math.Max(1.0, System.Math.Max(System.Math.Abs(x0), System.Math.Abs(p0)));
    maximumRecoveryError = System.Math.Max(maximumRecoveryError, scaledError);
    reversibilityRows.Add(new { x0, p0, stepSize = step, stepCount = count, x1, p1, recoveredX = xr, recoveredP = pr, scaledError });
}
double recoveryTolerance = leapfrogSpec.GetProperty("forwardReverseScaledTolerance").GetDouble();
bool forwardReversePassed = ladderShapeValid && reversibilityRows.Count == states.Length * momenta.Length * stepSizes.Length
    && reversibilityRows.All(row => double.IsFinite(JsonSerializer.SerializeToElement(row).GetProperty("scaledError").GetDouble()))
    && maximumRecoveryError <= recoveryTolerance;

var energyLadder = new List<EnergyLevel>();
for (int level = 0; level < stepSizes.Length; level++)
{
    double step = stepSizes[level];
    int count = stepCounts[level];
    var errors = new List<double>();
    foreach (double x0 in states)
    foreach (double p0 in momenta)
    {
        (double x1, double p1) = Leapfrog(x0, p0, step, count, Gradient);
        errors.Add(System.Math.Abs(Hamiltonian(x1, p1) - Hamiltonian(x0, p0)));
    }
    double rms = System.Math.Sqrt(errors.Select(x => x * x).Average());
    energyLadder.Add(new EnergyLevel(step, count, errors.Count, rms, errors.Max(), errors.All(double.IsFinite)));
}
double maximumAdjacentRatio = leapfrogSpec.GetProperty("maximumAdjacentRmsErrorRatio").GetDouble();
double minimumImprovement = leapfrogSpec.GetProperty("minimumCoarseToFineRmsImprovement").GetDouble();
double minimumOrder = leapfrogSpec.GetProperty("minimumAdjacentObservedOrder").GetDouble();
var adjacentOrders = Enumerable.Range(0, energyLadder.Count - 1).Select(i =>
    System.Math.Log(energyLadder[i].RmsAbsoluteEnergyError / energyLadder[i + 1].RmsAbsoluteEnergyError, 2.0)).ToArray();
bool energyStepHalvingPassed = ladderShapeValid && energyLadder.All(x => x.AllFinite && x.RmsAbsoluteEnergyError > 0.0)
    && Enumerable.Range(0, energyLadder.Count - 1).All(i =>
        energyLadder[i + 1].RmsAbsoluteEnergyError <= maximumAdjacentRatio * energyLadder[i].RmsAbsoluteEnergyError)
    && energyLadder[0].RmsAbsoluteEnergyError / energyLadder[^1].RmsAbsoluteEnergyError >= minimumImprovement
    && adjacentOrders.All(x => double.IsFinite(x) && x >= minimumOrder);

string verdict = !inputsValid ? "invalid-or-drifted-input"
    : !polynomialValid ? "phase534-polynomial-mismatch"
    : !gradientAuditPassed ? "analytic-gradient-disagrees-with-finite-difference"
    : !forwardReversePassed ? "forward-reverse-recovery-failed"
    : !energyStepHalvingPassed ? "energy-step-halving-order-failed"
    : "deterministic-leapfrog-audit-passed";
string terminalStatus = "deterministic-leapfrog-correctness-stability-audit-" + verdict;

var result = new
{
    schemaVersion = 1,
    phase = 537,
    phaseId = "phase537-deterministic-leapfrog-correctness-stability-audit",
    contractId = S(contract, "contractId"),
    contractSha256 = Sha256(ContractPath),
    planSection = S(contract, "planSection"),
    inputsValid,
    contractValid,
    frozenNumericalMenuValid,
    bindingInventoryValid,
    exactBindingsValid,
    upstreamSemanticsValid,
    noRngUsed = true,
    polynomial = new { c2, c3, c4, polynomialBitsMatch, phase534FormulaAnchorsPresent, polynomialValid },
    gradientAudit = new { passed = gradientAuditPassed, relativeStep, tolerance = gradientTolerance, rows = gradientRows },
    reversibilityAudit = new { passed = forwardReversePassed, tolerance = recoveryTolerance, maximumRecoveryError, rowCount = reversibilityRows.Count, rows = reversibilityRows },
    energyStepHalvingAudit = new
    {
        passed = energyStepHalvingPassed,
        trajectoryLength,
        maximumAdjacentRmsErrorRatio = maximumAdjacentRatio,
        minimumCoarseToFineRmsImprovement = minimumImprovement,
        minimumAdjacentObservedOrder = minimumOrder,
        observedCoarseToFineRmsImprovement = energyLadder[0].RmsAbsoluteEnergyError / energyLadder[^1].RmsAbsoluteEnergyError,
        adjacentObservedOrders = adjacentOrders,
        ladder = energyLadder,
    },
    verdictKind = verdict,
    terminalStatus,
    matchesPreregisteredExpectedCurrentVerdict = verdict == S(contract, "expectedCurrentVerdict"),
    matchesPreregisteredExpectedCurrentTerminalStatus = terminalStatus == S(contract, "expectedCurrentTerminalStatus"),
    decision = verdict == "deterministic-leapfrog-audit-passed"
        ? "The analytic gradient, deterministic forward/reverse recovery, and frozen energy step-halving checks pass on the exact Phase534 polynomial. This localizes no Phase534 stochastic failure and establishes neither sampling nor mixing validity."
        : "The earliest deterministic leapfrog audit failure is preserved. Phase534 is not repaired or relabeled and no downstream authority follows.",
    bindings,
    deterministicOnly = true,
    zeroPhysicsCompute = true,
    phase534RepairedOrRelabeled = false,
    phase535PilotReopened = false,
    samplingOrMixingValidityEstablished = false,
    phase481PackConstructedOrMutated = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    o4Satisfied = false,
    samplingPerformed = false,
    reprocessingPerformed = false,
    hmcPerformed = false,
    productionAuthorized = false,
    launchAuthorized = false,
    sourceContractApplicationPerformed = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    allDownstreamAuthorities = false,
    externalReviewPending = true,
    promotedPhysicalMassClaimCount = 0,
};

Console.WriteLine($"diagnostic verdict={verdict} inputs={inputsValid} polynomial={polynomialValid} gradient={gradientAuditPassed} reverse={forwardReversePassed} energyHalving={energyStepHalvingPassed}");
Console.WriteLine($"diagnostic reverseMax={maximumRecoveryError:R} energyRms={string.Join(',', energyLadder.Select(x => x.RmsAbsoluteEnergyError.ToString("R")))} orders={string.Join(',', adjacentOrders.Select(x => x.ToString("R")))}");
Require(expectedTaxonomy.Contains(verdict, StringComparer.Ordinal), "Phase537 emitted a terminal outside the frozen taxonomy.");
Require(result.noRngUsed && !result.hmcPerformed && !result.samplingOrMixingValidityEstablished, "Phase537 deterministic boundary failed.");
Require(!result.allDownstreamAuthorities && result.promotedPhysicalMassClaimCount == 0, "Phase537 authority firewall failed.");

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, options);
File.WriteAllText(OutputPath, json);
File.WriteAllText(SummaryPath, json);
Console.WriteLine(terminalStatus);
Console.WriteLine($"gradient={gradientAuditPassed} reverse={forwardReversePassed} energyHalving={energyStepHalvingPassed} rng=False promotedPhysicalMassClaimCount=0");

static (double X, double P) Leapfrog(double x, double p, double step, int steps, Func<double, double> gradient)
{
    double position = x;
    double momentum = p - 0.5 * step * gradient(position);
    for (int i = 0; i < steps; i++)
    {
        position += step * momentum;
        if (i + 1 < steps) momentum -= step * gradient(position);
    }
    momentum -= 0.5 * step * gradient(position);
    return (position, momentum);
}

sealed class EnergyLevel
{
    public EnergyLevel(double stepSize, int stepCount, int rowCount, double rmsAbsoluteEnergyError, double maximumAbsoluteEnergyError, bool allFinite) =>
        (StepSize, StepCount, RowCount, RmsAbsoluteEnergyError, MaximumAbsoluteEnergyError, AllFinite) =
        (stepSize, stepCount, rowCount, rmsAbsoluteEnergyError, maximumAbsoluteEnergyError, allFinite);

    public double StepSize { get; }
    public int StepCount { get; }
    public int RowCount { get; }
    public double RmsAbsoluteEnergyError { get; }
    public double MaximumAbsoluteEnergyError { get; }
    public bool AllFinite { get; }
}
