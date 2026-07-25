using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase541_complete_lattice_integrator_transfer_control_001";
const string ContractPath = Root + "/preregistration/phase541_complete_lattice_integrator_transfer_control_contract_v1.json";
const string OutputPath = Root + "/output/complete_lattice_integrator_transfer_control.json";
const string SummaryPath = Root + "/output/complete_lattice_integrator_transfer_control_summary.json";

byte[] contractBytes = File.ReadAllBytes(ContractPath);
using var contractDocument = JsonDocument.Parse(contractBytes);
JsonElement contract = contractDocument.RootElement;
var expectedBindings = new (string Id, string Path)[]
{
    ("phase533-contract", "studies/phase533_nested_validation_contract_001/preregistration/phase533_nested_validation_contract_v1.json"),
    ("phase534-contract", "studies/phase534_nested_control_battery_001/preregistration/phase534_nested_control_contract_v1.json"),
    ("phase534-summary", "studies/phase534_nested_control_battery_001/output/nested_control_battery_summary.json"),
    ("phase535-summary", "studies/phase535_bounded_registered_operator_pilot_adjudicator_001/output/bounded_registered_operator_pilot_adjudicator_summary.json"),
    ("phase537-summary", "studies/phase537_deterministic_leapfrog_correctness_stability_audit_001/output/deterministic_leapfrog_correctness_stability_audit_summary.json"),
    ("phase539-summary", "studies/phase539_independent_reduced_target_row_confirmation_001/output/independent_reduced_target_row_confirmation_summary.json"),
    ("phase540-contract", "studies/phase540_reduced_to_complete_lattice_transfer_readiness_001/preregistration/phase540_reduced_to_complete_lattice_transfer_readiness_contract_v1.json"),
    ("phase540-program", "studies/phase540_reduced_to_complete_lattice_transfer_readiness_001/Program.cs"),
    ("phase540-summary", "studies/phase540_reduced_to_complete_lattice_transfer_readiness_001/output/reduced_to_complete_lattice_transfer_readiness_summary.json"),
    ("registered-operator-source", "studies/phase452_scalar_channel_spectroscopy_probe_001/Program.cs"),
    ("complete-lattice-gradient-source", "src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs"),
};
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
bool bindingInventoryValid = bindingSpecs.Select(x => (x.Id, x.Path)).SequenceEqual(expectedBindings);
bool exactBindingsValid = bindingInventoryValid && bindings.All(x => x.HashMatches);

using var p533Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[0].Path));
using var p534ContractDocument = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[1].Path));
using var p534Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[2].Path));
using var p535Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[3].Path));
using var p537Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[4].Path));
using var p539Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[5].Path));
using var p540Document = JsonDocument.Parse(File.ReadAllBytes(expectedBindings[8].Path));
JsonElement p533 = p533Document.RootElement;
JsonElement p534Contract = p534ContractDocument.RootElement;
JsonElement p534 = p534Document.RootElement;
JsonElement p535 = p535Document.RootElement;
JsonElement p537 = p537Document.RootElement;
JsonElement p539 = p539Document.RootElement;
JsonElement p540 = p540Document.RootElement;

string[] expectedTaxonomy =
[
    "invalid-or-drifted-input", "resource-refusal", "embedding-action-reconstruction-failed",
    "embedding-kinetic-metric-mismatch", "analytic-force-parity-failed",
    "embedded-ray-not-force-invariant", "selected-row-complete-lattice-instability",
    "pilot-row-deterministic-oracle-failed", "deterministic-transfer-controls-passed",
];
string[] expectedFirewallKeys =
[
    "rngUsed", "hmcOrSamplingPerformed", "configurationsRetained",
    "phase535ExecutedReopenedOrMutated", "phase481PackCreatedOrMutated", "productionDefaultSelected",
    "phase458G3Satisfied", "phase458G4Satisfied", "phase458G5Satisfied", "o4Discharged",
    "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed", "productionOrLaunchAllowed",
];
JsonElement target = contract.GetProperty("registeredTarget");
JsonElement menu = contract.GetProperty("deterministicMenu");
JsonElement resource = contract.GetProperty("resourceRefusal");
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase541-a26-complete-lattice-integrator-transfer-control-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A26"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && exactBindingsValid
    && target.GetProperty("member").GetString() == "sd2-id0/c0.5"
    && target.GetProperty("beta").GetDouble() == 1.0
    && target.GetProperty("extent").GetInt32() == 3
    && target.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && Doubles(menu, "embeddingStates").SequenceEqual(new[] { -1.0, -0.5, 0.5, 1.0 })
    && ReadLadder(menu, "selectedRowLadder").SequenceEqual(new[] { (0.25, 8), (0.125, 16), (0.0625, 32) })
    && ReadLadder(menu, "pilotRowLadder").SequenceEqual(new[] { (0.0125, 6), (0.00625, 12), (0.003125, 24) })
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedTaxonomy)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

JsonElement pilot = p533.GetProperty("pilotConfiguration");
JsonElement reduction = p534Contract.GetProperty("registeredReduction");
JsonElement selected = p539.GetProperty("fixedSelectedRow");
bool precursorSemanticsValid =
    p534.GetProperty("verdictKind").GetString() == "reduced-interacting-control-failed"
    && p535.GetProperty("verdictKind").GetString() == "reduced-interacting-control-failed"
    && !p535.GetProperty("pilotRun").GetBoolean()
    && p537.GetProperty("verdictKind").GetString() == "deterministic-leapfrog-audit-passed"
    && p539.GetProperty("verdictKind").GetString() == "selected-row-independently-confirmed-reduced-target-only"
    && p540.GetProperty("verdictKind").GetString() == "reduced-to-complete-lattice-transfer-map-missing"
    && pilot.GetProperty("member").GetString() == target.GetProperty("member").GetString()
    && pilot.GetProperty("extent").GetInt32() == target.GetProperty("extent").GetInt32()
    && pilot.GetProperty("thetaRule").GetString() == target.GetProperty("thetaRule").GetString()
    && selected.GetProperty("stepSize").GetDouble() == 0.25
    && selected.GetProperty("leapfrogSteps").GetInt32() == 8
    && pilot.GetProperty("stepSize").GetDouble() == 0.0125
    && pilot.GetProperty("leapfrogSteps").GetInt32() == 6;

bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble()
        <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64()
        <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64();

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int extent = target.GetProperty("extent").GetInt32();
var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
var member = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2,
    Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5,
    EpsilonMode = "independent-theta",
};
var op = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: extent);
var mass = new CpuMassMatrix(mesh, algebra);
var thetaZero = new double[mesh.VertexCount * algebra.Dimension];
var ray = new double[mesh.EdgeCount * algebra.Dimension];
int[] witnessEdges = Ints(reduction, "witnessEdges");
int[] witnessComponents = Ints(reduction, "witnessComponents");
int[] witnessValues = Ints(reduction, "witnessValues");
for (int i = 0; i < witnessEdges.Length; i++)
    ray[witnessEdges[i] * algebra.Dimension + witnessComponents[i]] = witnessValues[i];
double embeddingNormSquared = Dot(ray, ray);
double expectedEmbeddingNormSquared = target.GetProperty("expectedRawEmbeddingNormSquared").GetDouble();
double reducedScalarKineticMetric = target.GetProperty("reducedScalarKineticMetric").GetDouble();
double kineticMetricTolerance = menu.GetProperty("kineticMetricTolerance").GetDouble();
bool kineticMetricCompatible = System.Math.Abs(embeddingNormSquared - reducedScalarKineticMetric) <= kineticMetricTolerance;
double metricCompatibleStepRescaling = selected.GetProperty("stepSize").GetDouble() / System.Math.Sqrt(embeddingNormSquared);

(double S, double[] G) Evaluate(double[] omega)
{
    var g = op.ComputeJointGradient(omega, thetaZero, mass);
    return (g.Objective, g.GradOmega);
}
double[] Embed(double x)
{
    var omega = new double[ray.Length];
    for (int i = 0; i < ray.Length; i++) omega[i] = x * ray[i];
    return omega;
}

JsonElement polynomial = p534.GetProperty("reducedInteractingControl").GetProperty("polynomial");
double c2 = polynomial.GetProperty("c2").GetDouble();
double c3 = polynomial.GetProperty("c3").GetDouble();
double c4 = polynomial.GetProperty("c4").GetDouble();
double Potential(double x) => c2 * x * x + c3 * x * x * x + c4 * x * x * x * x;
double ScalarGradient(double x) => 2 * c2 * x + 3 * c3 * x * x + 4 * c4 * x * x * x;

double replayTolerance = menu.GetProperty("actionReplayRelativeTolerance").GetDouble();
double fdStepScale = menu.GetProperty("finiteDifferenceRelativeStep").GetDouble();
double fdTolerance = menu.GetProperty("finiteDifferenceScaledTolerance").GetDouble();
double transverseTolerance = menu.GetProperty("transverseForceRelativeTolerance").GetDouble();
var stateRows = new List<object>();
double maxActionReplayError = 0.0;
double maxScalarGradientError = 0.0;
double maxDirectionalGradientError = 0.0;
double maxTransverseForceFraction = 0.0;
var probeDirection = DeterministicUnitVector(ray.Length);
for (int stateIndex = 0; stateIndex < Doubles(menu, "embeddingStates").Length; stateIndex++)
{
    double x = Doubles(menu, "embeddingStates")[stateIndex];
    double[] omega = Embed(x);
    (double action, double[] gradient) = Evaluate(omega);
    double replayError = ScaledError(action, Potential(x));
    double scalarFromFull = Dot(gradient, ray);
    double scalarGradientError = ScaledError(scalarFromFull, ScalarGradient(x));
    double projectionScale = scalarFromFull / embeddingNormSquared;
    double transverseNormSquared = 0.0;
    for (int i = 0; i < gradient.Length; i++)
    {
        double transverse = gradient[i] - projectionScale * ray[i];
        transverseNormSquared += transverse * transverse;
    }
    double gradientNorm = System.Math.Sqrt(Dot(gradient, gradient));
    double transverseFraction = System.Math.Sqrt(transverseNormSquared) / System.Math.Max(1.0, gradientNorm);
    double h = fdStepScale * System.Math.Max(1.0, Norm(omega));
    double[] plus = AddScaled(omega, probeDirection, h);
    double[] minus = AddScaled(omega, probeDirection, -h);
    double fd = (Evaluate(plus).S - Evaluate(minus).S) / (2.0 * h);
    double analytic = Dot(gradient, probeDirection);
    double directionalError = ScaledError(fd, analytic);
    maxActionReplayError = System.Math.Max(maxActionReplayError, replayError);
    maxScalarGradientError = System.Math.Max(maxScalarGradientError, scalarGradientError);
    maxDirectionalGradientError = System.Math.Max(maxDirectionalGradientError, directionalError);
    maxTransverseForceFraction = System.Math.Max(maxTransverseForceFraction, transverseFraction);
    stateRows.Add(new { x, action, polynomialAction = Potential(x), replayError, scalarFromFull,
        scalarAnalytic = ScalarGradient(x), scalarGradientError, transverseFraction,
        finiteDifference = fd, analyticDirectionalDerivative = analytic, directionalError });
}
bool actionReconstructionPassed = maxActionReplayError <= replayTolerance
    && System.Math.Abs(embeddingNormSquared - expectedEmbeddingNormSquared) <= kineticMetricTolerance;
bool analyticForceParityPassed = maxScalarGradientError <= fdTolerance && maxDirectionalGradientError <= fdTolerance;
bool embeddedRayForceInvariant = maxTransverseForceFraction <= transverseTolerance;

double[] initialPosition = Embed(0.1);
double[] initialMomentum = DeterministicUnitVector(ray.Length);
var selectedLadder = RunLadder(ReadLadder(menu, "selectedRowLadder"), initialPosition, initialMomentum, Evaluate,
    menu.GetProperty("forwardReverseScaledTolerance").GetDouble(), menu.GetProperty("minimumAdjacentEnergyImprovement").GetDouble());
var pilotLadder = RunLadder(ReadLadder(menu, "pilotRowLadder"), initialPosition, initialMomentum, Evaluate,
    menu.GetProperty("forwardReverseScaledTolerance").GetDouble(), menu.GetProperty("minimumAdjacentEnergyImprovement").GetDouble());

string verdict = !contractValid || !precursorSemanticsValid ? expectedTaxonomy[0]
    : !resourceAccepted ? expectedTaxonomy[1]
    : !actionReconstructionPassed ? expectedTaxonomy[2]
    : !kineticMetricCompatible ? expectedTaxonomy[3]
    : !analyticForceParityPassed ? expectedTaxonomy[4]
    : !embeddedRayForceInvariant ? expectedTaxonomy[5]
    : !selectedLadder.Passed ? expectedTaxonomy[6]
    : !pilotLadder.Passed ? expectedTaxonomy[7]
    : expectedTaxonomy[8];
bool deterministicTransferControlsPassed = verdict == expectedTaxonomy[8];
var result = new
{
    schemaVersion = 1,
    phase = 541,
    phaseId = "phase541-complete-lattice-integrator-transfer-control",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    bindingInventoryValid,
    exactBindingsValid,
    precursorSemanticsValid,
    resourceAccepted,
    deterministicZeroSampling = true,
    rngUsed = false,
    hmcOrSamplingPerformed = false,
    configurationsRetained = false,
    completeLattice = new { member = "sd2-id0/c0.5", beta = 1.0, extent, thetaRule = "theta-identically-zero",
        vertexCount = mesh.VertexCount, edgeCount = mesh.EdgeCount, omegaDegreesOfFreedom = ray.Length,
        thetaDegreesOfFreedom = thetaZero.Length },
    explicitEmbeddingMap = new
    {
        map = "omega=x*r with the three Phase527 witness coefficients; theta=0",
        witnessEdges, witnessComponents, witnessValues,
        embeddingNormSquared,
        reducedScalarKineticMetric,
        kineticMetricCompatible,
        selectedReducedStepSize = selected.GetProperty("stepSize").GetDouble(),
        selectedReducedLeapfrogSteps = selected.GetProperty("leapfrogSteps").GetInt32(),
        metricCompatibleStepRescaling,
        frozenPilotStepSize = pilot.GetProperty("stepSize").GetDouble(),
        frozenPilotLeapfrogSteps = pilot.GetProperty("leapfrogSteps").GetInt32(),
        parameterRowsIdentical = false,
    },
    actionReconstruction = new { passed = actionReconstructionPassed, c2, c3, c4, maxActionReplayError, stateRows },
    analyticForceParity = new { passed = analyticForceParityPassed, maxScalarGradientError, maxDirectionalGradientError, tolerance = fdTolerance },
    transverseForceAudit = new { passed = embeddedRayForceInvariant, maxTransverseForceFraction, tolerance = transverseTolerance },
    selectedRowCompleteLatticeLadder = selectedLadder,
    pilotRowCompleteLatticeLadder = pilotLadder,
    deterministicTransferControlsPassed,
    laterControlOrPilotAuthorized = false,
    verdictKind = verdict,
    terminalStatus = "complete-lattice-integrator-transfer-control-" + verdict,
    decision = verdict == "embedding-kinetic-metric-mismatch"
        ? "The scalar witness embeds exactly into the complete-lattice action, but the raw witness has induced Euclidean kinetic metric three while the reduced sampler used metric one. The selected scalar row therefore has no direct symplectic transfer to the frozen pilot row."
        : "The earliest deterministic transfer-control failure is preserved. No pilot or sampling authority follows.",
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
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase541 verdict: {verdict}");
Console.WriteLine($"embedding metric: reduced={reducedScalarKineticMetric:R}, induced={embeddingNormSquared:R}");
Console.WriteLine($"force parity={analyticForceParityPassed}, ray invariant={embeddedRayForceInvariant}, rng=False, sampling=False");

static LadderResult RunLadder((double Step, int Count)[] ladder, double[] position, double[] momentum,
    Func<double[], (double S, double[] G)> evaluate, double reverseTolerance, double minimumImprovement)
{
    var rows = new List<TrajectoryRow>();
    foreach ((double step, int count) in ladder)
    {
        TrajectoryState forward = Leapfrog(position, momentum, step, count, evaluate);
        double initialEnergy = Hamiltonian(position, momentum, evaluate(position).S);
        double finalEnergy = forward.Finite ? Hamiltonian(forward.Position, forward.Momentum, forward.Action) : double.PositiveInfinity;
        double energyError = System.Math.Abs(finalEnergy - initialEnergy);
        double reverseError = double.PositiveInfinity;
        if (forward.Finite)
        {
            double[] reversedMomentum = forward.Momentum.Select(x => -x).ToArray();
            TrajectoryState reverse = Leapfrog(forward.Position, reversedMomentum, step, count, evaluate);
            if (reverse.Finite)
                reverseError = System.Math.Max(VectorScaledError(reverse.Position, position),
                    VectorScaledError(reverse.Momentum, momentum.Select(x => -x).ToArray()));
        }
        rows.Add(new TrajectoryRow(step, count, step * count, forward.Finite, energyError, reverseError,
            forward.Finite && reverseError <= reverseTolerance));
    }
    bool energyImproves = rows.Zip(rows.Skip(1)).All(pair =>
        pair.Second.AbsoluteEnergyError == 0.0
        || pair.First.AbsoluteEnergyError / pair.Second.AbsoluteEnergyError >= minimumImprovement);
    bool passed = rows.All(x => x.Finite && x.ReversibilityPassed) && energyImproves;
    return new LadderResult(passed, energyImproves, minimumImprovement, rows.ToArray());
}

static TrajectoryState Leapfrog(double[] q0, double[] p0, double step, int count,
    Func<double[], (double S, double[] G)> evaluate)
{
    double[] q = (double[])q0.Clone();
    double[] p = (double[])p0.Clone();
    (double action, double[] gradient) = evaluate(q);
    if (!Finite(action, gradient)) return new TrajectoryState(q, p, action, false);
    for (int i = 0; i < p.Length; i++) p[i] -= 0.5 * step * gradient[i];
    for (int leap = 0; leap < count; leap++)
    {
        for (int i = 0; i < q.Length; i++) q[i] += step * p[i];
        (action, gradient) = evaluate(q);
        if (!Finite(action, gradient) || !p.All(double.IsFinite) || !q.All(double.IsFinite))
            return new TrajectoryState(q, p, action, false);
        double kick = leap + 1 == count ? 0.5 * step : step;
        for (int i = 0; i < p.Length; i++) p[i] -= kick * gradient[i];
    }
    return new TrajectoryState(q, p, action, p.All(double.IsFinite));
}

static bool Finite(double action, double[] gradient) => double.IsFinite(action) && gradient.All(double.IsFinite);
static double Hamiltonian(double[] q, double[] p, double action) => action + 0.5 * Dot(p, p);
static double[] AddScaled(double[] x, double[] direction, double scale)
{
    var result = new double[x.Length];
    for (int i = 0; i < x.Length; i++) result[i] = x[i] + scale * direction[i];
    return result;
}
static double[] DeterministicUnitVector(int length)
{
    var result = new double[length];
    for (int i = 0; i < length; i++) result[i] = System.Math.Sin((i + 1) * 0.6180339887498948);
    double norm = Norm(result);
    for (int i = 0; i < length; i++) result[i] /= norm;
    return result;
}
static double VectorScaledError(double[] actual, double[] expected)
{
    double numerator = 0.0, denominator = 1.0;
    for (int i = 0; i < actual.Length; i++)
    {
        double d = actual[i] - expected[i];
        numerator += d * d;
        denominator += expected[i] * expected[i];
    }
    return System.Math.Sqrt(numerator / denominator);
}
static double ScaledError(double a, double b) => System.Math.Abs(a - b) / System.Math.Max(1.0, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
static double Norm(double[] x) => System.Math.Sqrt(Dot(x, x));
static double Dot(double[] a, double[] b)
{
    double sum = 0.0;
    for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
    return sum;
}
static double[] Doubles(JsonElement parent, string name) => parent.GetProperty(name).EnumerateArray().Select(x => x.GetDouble()).ToArray();
static int[] Ints(JsonElement parent, string name) => parent.GetProperty(name).EnumerateArray().Select(x => x.GetInt32()).ToArray();
static (double Step, int Count)[] ReadLadder(JsonElement parent, string name) => parent.GetProperty(name).EnumerateArray()
    .Select(x => (x.GetProperty("stepSize").GetDouble(), x.GetProperty("leapfrogSteps").GetInt32())).ToArray();
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

sealed class TrajectoryState(double[] position, double[] momentum, double action, bool finite)
{
    public double[] Position { get; } = position;
    public double[] Momentum { get; } = momentum;
    public double Action { get; } = action;
    public bool Finite { get; } = finite;
}
sealed class TrajectoryRow(double stepSize, int leapfrogSteps, double trajectoryLength, bool finite,
    double absoluteEnergyError, double reversibilityScaledError, bool reversibilityPassed)
{
    public double StepSize { get; } = stepSize;
    public int LeapfrogSteps { get; } = leapfrogSteps;
    public double TrajectoryLength { get; } = trajectoryLength;
    public bool Finite { get; } = finite;
    public double AbsoluteEnergyError { get; } = absoluteEnergyError;
    public double ReversibilityScaledError { get; } = reversibilityScaledError;
    public bool ReversibilityPassed { get; } = reversibilityPassed;
}
sealed class LadderResult(bool passed, bool energyImproves, double minimumAdjacentEnergyImprovement, TrajectoryRow[] rows)
{
    public bool Passed { get; } = passed;
    public bool EnergyImproves { get; } = energyImproves;
    public double MinimumAdjacentEnergyImprovement { get; } = minimumAdjacentEnergyImprovement;
    public TrajectoryRow[] Rows { get; } = rows;
}
