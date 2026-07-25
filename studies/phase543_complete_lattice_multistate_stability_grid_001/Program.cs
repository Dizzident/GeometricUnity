using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase543_complete_lattice_multistate_stability_grid_001";
const string ContractPath = Root + "/preregistration/phase543_branch_selected_deterministic_transfer_contract_v1.json";
const string OutputPath = Root + "/output/complete_lattice_multistate_stability_grid.json";
const string SummaryPath = Root + "/output/complete_lattice_multistate_stability_grid_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
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
string[] expectedIds = ["phase541-summary", "phase542-contract", "phase542-program", "phase542-summary", "complete-lattice-gradient-source"];
bool exactBindingsValid = bindingSpecs.Select(x => x.Id).SequenceEqual(expectedIds) && bindings.All(x => x.HashMatches);
using var p541Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[0].Path));
using var p542Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[3].Path));
JsonElement p541 = p541Doc.RootElement;
JsonElement p542 = p542Doc.RootElement;
JsonElement branchRules = contract.GetProperty("branchPrecedence");
JsonElement menu = contract.GetProperty("completeLatticeMenu");
JsonElement resource = contract.GetProperty("resourceRefusal");
string[] taxonomy =
[
    "invalid-or-drifted-input", "resource-refusal", "branch-selection-invalid",
    "deterministic-state-menu-invalid", "complete-lattice-gradient-or-finiteness-failed",
    "complete-lattice-multistate-stability-failed", "compact-shadowing-failed",
    "branch-selected-deterministic-controls-passed",
];
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase543-a27-branch-selected-deterministic-transfer-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A27"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && exactBindingsValid
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()).SequenceEqual(taxonomy)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
bool precursorSemanticsValid =
    p541.GetProperty("verdictKind").GetString() == "embedding-kinetic-metric-mismatch"
    && p542.GetProperty("verdictKind").GetString() == "force-closure-expands-beyond-compact-limit"
    && p542.GetProperty("metricNormalization").GetProperty("passed").GetBoolean()
    && p542.GetProperty("laterDeterministicExperimentAuthorized").GetBoolean()
    && !p542.GetProperty("hmcOrSamplingPerformed").GetBoolean();
string selectedBranch = p542.GetProperty("verdictKind").GetString() == branchRules.GetProperty("compactClosureVerdict").GetString()
    ? branchRules.GetProperty("compactBranch").GetString()!
    : branchRules.GetProperty("otherwiseBranch").GetString()!;
bool branchSelectionValid = selectedBranch == branchRules.GetProperty("expectedFrozenBranch").GetString();
bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble() <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64() <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64();

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int extent = menu.GetProperty("extent").GetInt32();
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
JsonElement map = p541.GetProperty("explicitEmbeddingMap");
int[] witnessEdges = Ints(map, "witnessEdges");
int[] witnessComponents = Ints(map, "witnessComponents");
int[] witnessValues = Ints(map, "witnessValues");
var rawRay = new double[mesh.EdgeCount * algebra.Dimension];
for (int i = 0; i < witnessEdges.Length; i++)
    rawRay[witnessEdges[i] * algebra.Dimension + witnessComponents[i]] = witnessValues[i];
double[] ray = Scale(rawRay, 1.0 / Norm(rawRay));
double[] denseDirection = DeterministicUnitVector(ray.Length, cosine: false);
double denseScale = menu.GetProperty("densePerturbationScale").GetDouble();
double[] stateScales = Doubles(menu, "stateScales");
var states = stateScales.Select((scale, index) =>
{
    double signedDense = index % 2 == 0 ? denseScale : -denseScale;
    return AddScaled(Scale(ray, scale), denseDirection, signedDense);
}).ToArray();
double minimumOffRay = menu.GetProperty("minimumOffRayStateFraction").GetDouble();
double minimumObservedOffRay = states.Min(state => OffRayFraction(state, ray));
bool stateMenuValid = states.Length == 3 && minimumObservedOffRay >= minimumOffRay;

(double S, double[] G) Evaluate(double[] omega)
{
    var g = op.ComputeJointGradient(omega, thetaZero, mass);
    return (g.Objective, g.GradOmega);
}

double fdStep = menu.GetProperty("finiteDifferenceRelativeStep").GetDouble();
double fdTolerance = menu.GetProperty("finiteDifferenceScaledTolerance").GetDouble();
double maxDirectionalGradientError = 0.0;
bool allFinite = true;
var probe = DeterministicUnitVector(ray.Length, cosine: true);
foreach (double[] state in states)
{
    (double action, double[] gradient) = Evaluate(state);
    double h = fdStep * System.Math.Max(1.0, Norm(state));
    double plus = Evaluate(AddScaled(state, probe, h)).S;
    double minus = Evaluate(AddScaled(state, probe, -h)).S;
    double error = ScaledError((plus - minus) / (2.0 * h), Dot(gradient, probe));
    maxDirectionalGradientError = System.Math.Max(maxDirectionalGradientError, error);
    allFinite &= double.IsFinite(action) && gradient.All(double.IsFinite) && double.IsFinite(error);
}
bool gradientControlPassed = allFinite && maxDirectionalGradientError <= fdTolerance;

(double Step, int Count)[] ladder = menu.GetProperty("ladder").EnumerateArray()
    .Select(x => (x.GetProperty("stepSize").GetDouble(), x.GetProperty("leapfrogSteps").GetInt32())).ToArray();
double reverseTolerance = menu.GetProperty("forwardReverseScaledTolerance").GetDouble();
double minimumImprovement = menu.GetProperty("minimumAdjacentEnergyImprovement").GetDouble();
var gridRows = new List<object>();
bool gridPassed = true;
for (int stateIndex = 0; stateIndex < states.Length; stateIndex++)
{
    foreach (bool cosine in new[] { false, true })
    {
        double[] momentum = DeterministicUnitVector(ray.Length, cosine);
        LadderResult result = RunLadder(ladder, states[stateIndex], momentum, Evaluate, reverseTolerance, minimumImprovement);
        gridPassed &= result.Passed;
        gridRows.Add(new { stateIndex, momentumFamily = cosine ? "cosine" : "sine", result });
    }
}
bool compactBranchSelected = selectedBranch == branchRules.GetProperty("compactBranch").GetString();
bool compactShadowingPassed = false;
string verdict = !contractValid || !precursorSemanticsValid ? taxonomy[0]
    : !resourceAccepted ? taxonomy[1]
    : !branchSelectionValid ? taxonomy[2]
    : !stateMenuValid ? taxonomy[3]
    : !gradientControlPassed ? taxonomy[4]
    : !compactBranchSelected && !gridPassed ? taxonomy[5]
    : compactBranchSelected && !compactShadowingPassed ? taxonomy[6]
    : taxonomy[7];
bool deterministicControlsPassed = verdict == taxonomy[7];

var output = new
{
    schemaVersion = 1,
    phase = 543,
    phaseId = "phase543-complete-lattice-multistate-stability-grid",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    precursorSemanticsValid,
    resourceAccepted,
    deterministicZeroSampling = true,
    rngUsed = false,
    hmcOrSamplingPerformed = false,
    configurationsRetained = false,
    selectedBranch,
    branchSelectionValid,
    compactBranchSelected,
    completeLattice = new { extent, vertexCount = mesh.VertexCount, edgeCount = mesh.EdgeCount, omegaDegreesOfFreedom = ray.Length },
    deterministicStateMenu = new { stateCount = states.Length, momentumFamilyCount = 2, stateScales, denseScale, minimumObservedOffRay, minimumRequiredOffRay = minimumOffRay, passed = stateMenuValid },
    gradientControl = new { passed = gradientControlPassed, maxDirectionalGradientError, tolerance = fdTolerance },
    completeLatticeStabilityGrid = new { passed = gridPassed, ladder, rows = gridRows },
    compactShadowing = new { selected = compactBranchSelected, passed = compactShadowingPassed, notExecutedBecauseClosureExpanded = !compactBranchSelected },
    deterministicControlsPassed,
    verdictKind = verdict,
    terminalStatus = "branch-selected-deterministic-transfer-" + verdict,
    decision = deterministicControlsPassed
        ? "The compact closure was rejected upstream, and the preregistered complete-lattice branch passed its multi-state deterministic gradient, reversibility, and step-halving controls. This is not an HMC tuning or mixing result."
        : "The earliest branch-selected deterministic-control failure is preserved; no pilot authority follows.",
    laterReadinessAdjudicationAuthorized = deterministicControlsPassed,
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
byte[] json = JsonSerializer.SerializeToUtf8Bytes(output, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase543 verdict: {verdict}");
Console.WriteLine($"branch={selectedBranch}, gridPassed={gridPassed}, maxGradientError={maxDirectionalGradientError:R}");
Console.WriteLine("rng=False, sampling=False");

static LadderResult RunLadder((double Step, int Count)[] ladder, double[] position, double[] momentum,
    Func<double[], (double S, double[] G)> evaluate, double reverseTolerance, double minimumImprovement)
{
    var rows = new List<TrajectoryRow>();
    foreach ((double step, int count) in ladder)
    {
        TrajectoryState forward = Leapfrog(position, momentum, step, count, evaluate);
        double initialEnergy = Hamiltonian(position, momentum, evaluate(position).S);
        double finalEnergy = forward.Finite ? Hamiltonian(forward.Position, forward.Momentum, forward.Action) : double.PositiveInfinity;
        double reverseError = double.PositiveInfinity;
        if (forward.Finite)
        {
            double[] reversedMomentum = forward.Momentum.Select(x => -x).ToArray();
            TrajectoryState reverse = Leapfrog(forward.Position, reversedMomentum, step, count, evaluate);
            if (reverse.Finite)
                reverseError = System.Math.Max(VectorScaledError(reverse.Position, position),
                    VectorScaledError(reverse.Momentum, momentum.Select(x => -x).ToArray()));
        }
        rows.Add(new TrajectoryRow(step, count, step * count, forward.Finite,
            System.Math.Abs(finalEnergy - initialEnergy), reverseError, forward.Finite && reverseError <= reverseTolerance));
    }
    bool energyImproves = rows.Zip(rows.Skip(1)).All(pair =>
        pair.Second.AbsoluteEnergyError == 0.0 || pair.First.AbsoluteEnergyError / pair.Second.AbsoluteEnergyError >= minimumImprovement);
    return new LadderResult(rows.All(x => x.Finite && x.ReversibilityPassed) && energyImproves, energyImproves, minimumImprovement, rows.ToArray());
}
static TrajectoryState Leapfrog(double[] q0, double[] p0, double step, int count, Func<double[], (double S, double[] G)> evaluate)
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
static double OffRayFraction(double[] state, double[] ray)
{
    double projection = Dot(state, ray);
    double residualSquared = 0.0;
    for (int i = 0; i < state.Length; i++)
    {
        double residual = state[i] - projection * ray[i];
        residualSquared += residual * residual;
    }
    return System.Math.Sqrt(residualSquared) / System.Math.Max(1.0, Norm(state));
}
static double[] DeterministicUnitVector(int length, bool cosine)
{
    var result = new double[length];
    for (int i = 0; i < length; i++)
    {
        double angle = (i + 1) * 0.6180339887498948;
        result[i] = cosine ? System.Math.Cos(angle) : System.Math.Sin(angle);
    }
    double norm = Norm(result);
    for (int i = 0; i < length; i++) result[i] /= norm;
    return result;
}
static double VectorScaledError(double[] actual, double[] expected)
{
    double numerator = 0.0, denominator = 1.0;
    for (int i = 0; i < actual.Length; i++)
    {
        double difference = actual[i] - expected[i];
        numerator += difference * difference;
        denominator += expected[i] * expected[i];
    }
    return System.Math.Sqrt(numerator / denominator);
}
static double[] Scale(double[] vector, double scale) => vector.Select(x => x * scale).ToArray();
static double[] AddScaled(double[] vector, double[] direction, double scale)
{
    var result = new double[vector.Length];
    for (int i = 0; i < result.Length; i++) result[i] = vector[i] + scale * direction[i];
    return result;
}
static double ScaledError(double a, double b) => System.Math.Abs(a - b) / System.Math.Max(1.0, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
static double Norm(double[] vector) => System.Math.Sqrt(Dot(vector, vector));
static double Dot(double[] a, double[] b)
{
    double sum = 0.0;
    for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
    return sum;
}
static double[] Doubles(JsonElement parent, string name) => parent.GetProperty(name).EnumerateArray().Select(x => x.GetDouble()).ToArray();
static int[] Ints(JsonElement parent, string name) => parent.GetProperty(name).EnumerateArray().Select(x => x.GetInt32()).ToArray();
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
