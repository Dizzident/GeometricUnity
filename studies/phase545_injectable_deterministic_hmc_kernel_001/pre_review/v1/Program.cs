using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Phase545;

const string Root = "studies/phase545_injectable_deterministic_hmc_kernel_001";
const string ContractPath = Root + "/preregistration/phase545_injectable_deterministic_hmc_kernel_contract_v1.json";
const string KernelPath = Root + "/PilotKernel.cs";
const string OutputPath = Root + "/output/injectable_deterministic_hmc_kernel.json";
const string SummaryPath = Root + "/output/injectable_deterministic_hmc_kernel_summary.json";

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
string[] expectedIds =
[
    "phase543-summary", "phase544-contract", "phase544-program", "phase544-summary",
    "complete-lattice-gradient-source",
];
bool exactBindingsValid = bindingSpecs.Select(x => x.Id).SequenceEqual(expectedIds)
    && bindings.All(x => x.HashMatches);

using var p543Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[0].Path));
using var p544Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[3].Path));
JsonElement p543 = p543Doc.RootElement;
JsonElement p544 = p544Doc.RootElement;
bool precursorSemanticsValid =
    p543.GetProperty("verdictKind").GetString() == "branch-selected-deterministic-controls-passed"
    && p543.GetProperty("deterministicControlsPassed").GetBoolean()
    && p544.GetProperty("verdictKind").GetString() == "executable-bounded-pilot-implementation-missing"
    && p544.GetProperty("earliestMissingGate").GetString() == "new-executable-bounded-pilot-implementation"
    && !p544.GetProperty("hmcOrSamplingPerformed").GetBoolean();

string[] taxonomy =
[
    "invalid-or-drifted-input",
    "kernel-contract-invalid",
    "preallocation-refusal-failed",
    "nonfinite-telemetry-failed",
    "accept-fixture-failed",
    "reject-fixture-failed",
    "reversal-fixture-failed",
    "injectable-deterministic-hmc-kernel-materialized",
];
JsonElement menu = contract.GetProperty("fixtureMenu");
JsonElement rules = contract.GetProperty("kernelRules");
bool contractValid =
    contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase545-a28-injectable-deterministic-hmc-kernel-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A28"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && exactBindingsValid
    && rules.GetProperty("proposal").GetString() == "velocity-leapfrog-with-terminal-half-kick"
    && rules.GetProperty("preallocationRefusal").GetString() ==
        "refuse-before-kernel-array-allocation-when-estimated-working-bytes-exceed-limit"
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(taxonomy)
    && contract.GetProperty("authorityFirewalls").EnumerateObject()
        .All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

int extent = menu.GetProperty("extent").GetInt32();
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
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
int degreesOfFreedom = mesh.EdgeCount * algebra.Dimension;
double[] position = DeterministicVector(degreesOfFreedom, menu.GetProperty("stateScale").GetDouble(), cosine: false);
double[] momentum = DeterministicVector(degreesOfFreedom, menu.GetProperty("momentumScale").GetDouble(), cosine: true);

PilotEvaluation EvaluateComplete(double[] omega)
{
    var gradient = op.ComputeJointGradient(omega, thetaZero, mass);
    return new PilotEvaluation(gradient.Objective, gradient.GradOmega);
}

JsonElement resourceFixture = menu.GetProperty("resourceBoundary");
long allowedBytes = resourceFixture.GetProperty("allowedWorkingBytes").GetInt64();
long refusedBytes = resourceFixture.GetProperty("refusedWorkingBytes").GetInt64();
PilotProposal allowedResource = PilotKernel.ConstructProposal(
    position, momentum, 0.00625, 1, allowedBytes, EvaluateComplete);
bool evaluatorCalledOnRefusal = false;
PilotProposal refusedResource = PilotKernel.ConstructProposal(
    position, momentum, 0.00625, 1, refusedBytes, _ =>
    {
        evaluatorCalledOnRefusal = true;
        return new PilotEvaluation(double.NaN, []);
    });
bool resourceFixturePassed =
    allowedResource.ResourceRefused == resourceFixture.GetProperty("expectedAllowedRefused").GetBoolean()
    && refusedResource.ResourceRefused == resourceFixture.GetProperty("expectedBoundaryRefused").GetBoolean()
    && refusedResource.Divergent == resourceFixture.GetProperty("expectedRefusalDivergent").GetBoolean()
    && !evaluatorCalledOnRefusal
    && refusedResource.ForceEvaluationCount == 0
    && refusedResource.Position is null
    && refusedResource.Momentum is null;

JsonElement corruptFixture = menu.GetProperty("corruption");
PilotDecision corrupted = PilotKernel.RunSingleProposal(
    position, momentum, 0.00625, 1, -1.0, allowedBytes,
    q => new PilotEvaluation(double.NaN, Enumerable.Repeat(double.NaN, q.Length).ToArray()));
bool corruptionFixturePassed =
    corrupted.Proposal.Finite == corruptFixture.GetProperty("expectedFinite").GetBoolean()
    && corrupted.Proposal.Divergent == corruptFixture.GetProperty("expectedDivergent").GetBoolean()
    && corrupted.Accepted == corruptFixture.GetProperty("expectedAccepted").GetBoolean()
    && double.IsNegativeInfinity(corrupted.LogAcceptanceThreshold);

JsonElement acceptFixture = menu.GetProperty("accept");
PilotDecision accepted = PilotKernel.RunSingleProposal(
    position,
    momentum,
    acceptFixture.GetProperty("stepSize").GetDouble(),
    acceptFixture.GetProperty("leapfrogSteps").GetInt32(),
    acceptFixture.GetProperty("injectedLogUniformThreshold").GetDouble(),
    allowedBytes,
    EvaluateComplete);
bool acceptFixturePassed =
    accepted.Accepted == acceptFixture.GetProperty("expectedAccepted").GetBoolean()
    && accepted.Proposal.Finite
    && !accepted.Proposal.ResourceRefused
    && accepted.SelectedPosition is not null;

JsonElement rejectFixture = menu.GetProperty("reject");
PilotDecision rejected = PilotKernel.RunSingleProposal(
    position,
    momentum,
    rejectFixture.GetProperty("stepSize").GetDouble(),
    rejectFixture.GetProperty("leapfrogSteps").GetInt32(),
    rejectFixture.GetProperty("injectedLogUniformThreshold").GetDouble(),
    allowedBytes,
    EvaluateComplete);
bool rejectionReturnedInjectedPosition = rejected.SelectedPosition is not null
    && VectorScaledError(rejected.SelectedPosition, position) == 0.0;
bool rejectFixturePassed =
    rejected.Accepted == rejectFixture.GetProperty("expectedAccepted").GetBoolean()
    && rejected.Proposal.Finite
    && rejected.Proposal.DeltaHamiltonian > 0.0
    && rejectionReturnedInjectedPosition;

JsonElement reversalFixture = menu.GetProperty("reversal");
PilotProposal forward = PilotKernel.ConstructProposal(
    position,
    momentum,
    reversalFixture.GetProperty("stepSize").GetDouble(),
    reversalFixture.GetProperty("leapfrogSteps").GetInt32(),
    allowedBytes,
    EvaluateComplete);
PilotProposal reverse = forward.Position is not null && forward.Momentum is not null
    ? PilotKernel.ConstructProposal(
        forward.Position,
        forward.Momentum.Select(x => -x).ToArray(),
        reversalFixture.GetProperty("stepSize").GetDouble(),
        reversalFixture.GetProperty("leapfrogSteps").GetInt32(),
        allowedBytes,
        EvaluateComplete)
    : new PilotProposal(false, false, true, double.NaN, double.NaN, double.NaN, null, null, 0, 0);
double reversalError = reverse.Position is not null && reverse.Momentum is not null
    ? System.Math.Max(
        VectorScaledError(reverse.Position, position),
        VectorScaledError(reverse.Momentum, momentum.Select(x => -x).ToArray()))
    : double.PositiveInfinity;
bool reversalFixturePassed = forward.Finite && reverse.Finite
    && reversalError <= reversalFixture.GetProperty("scaledTolerance").GetDouble();

string verdict = !exactBindingsValid || !precursorSemanticsValid ? taxonomy[0]
    : !contractValid ? taxonomy[1]
    : !resourceFixturePassed ? taxonomy[2]
    : !corruptionFixturePassed ? taxonomy[3]
    : !acceptFixturePassed ? taxonomy[4]
    : !rejectFixturePassed ? taxonomy[5]
    : !reversalFixturePassed ? taxonomy[6]
    : taxonomy[7];
bool materialized = verdict == taxonomy[7];

var output = new
{
    schemaVersion = 1,
    phase = 545,
    phaseId = "phase545-injectable-deterministic-hmc-kernel",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    kernelSha256 = Sha(KernelPath),
    contractValid,
    exactBindingsValid,
    precursorSemanticsValid,
    deterministicZeroSampling = true,
    rngUsed = false,
    markovChainAdvanced = false,
    warmupPerformed = false,
    adaptationPerformed = false,
    samplingPerformed = false,
    configurationsRetained = false,
    completeLattice = new
    {
        extent,
        vertexCount = mesh.VertexCount,
        edgeCount = mesh.EdgeCount,
        omegaDegreesOfFreedom = degreesOfFreedom,
    },
    fixtures = new
    {
        resource = new
        {
            passed = resourceFixturePassed,
            allowedResource.ResourceRefused,
            refusedResource.Divergent,
            refusedResource.EstimatedWorkingBytes,
            evaluatorCalledOnRefusal,
            refusedResource.ForceEvaluationCount,
        },
        corruption = new
        {
            passed = corruptionFixturePassed,
            corrupted.Proposal.Finite,
            corrupted.Proposal.Divergent,
            corrupted.Accepted,
            corrupted.Proposal.ForceEvaluationCount,
        },
        accept = new
        {
            passed = acceptFixturePassed,
            accepted.Accepted,
            accepted.Proposal.Finite,
            accepted.Proposal.Divergent,
            accepted.Proposal.InitialHamiltonian,
            accepted.Proposal.FinalHamiltonian,
            accepted.Proposal.DeltaHamiltonian,
            accepted.InjectedLogUniformThreshold,
            accepted.LogAcceptanceThreshold,
            accepted.Proposal.ForceEvaluationCount,
        },
        reject = new
        {
            passed = rejectFixturePassed,
            rejected.Accepted,
            rejected.Proposal.Finite,
            rejected.Proposal.Divergent,
            rejected.Proposal.InitialHamiltonian,
            rejected.Proposal.FinalHamiltonian,
            rejected.Proposal.DeltaHamiltonian,
            rejected.InjectedLogUniformThreshold,
            rejected.LogAcceptanceThreshold,
            rejectionReturnedInjectedPosition,
            rejected.Proposal.ForceEvaluationCount,
        },
        reversal = new
        {
            passed = reversalFixturePassed,
            forwardFinite = forward.Finite,
            reverseFinite = reverse.Finite,
            scaledError = reversalError,
            tolerance = reversalFixture.GetProperty("scaledTolerance").GetDouble(),
        },
    },
    reusableKernelMaterialized = materialized,
    verdictKind = verdict,
    terminalStatus = "injectable-deterministic-hmc-kernel-" + verdict,
    decision = materialized
        ? "The prospectively frozen injected single-proposal fixtures pass. This is reusable kernel implementation evidence only, not HMC, sampling, acceptance-rate, mixing, or convergence evidence."
        : "The earliest frozen Phase545 failure is preserved. No later pack or execution authority follows.",
    laterDependentPackReadinessAdjudicationAllowed = materialized,
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
Console.WriteLine($"Phase545 verdict: {verdict}");
Console.WriteLine(
    $"accept={acceptFixturePassed}, reject={rejectFixturePassed}, reversal={reversalFixturePassed}, " +
    $"corruption={corruptionFixturePassed}, resource={resourceFixturePassed}");
Console.WriteLine("rng=False, chain=False, warmup=False, sampling=False");

static double[] DeterministicVector(int length, double norm, bool cosine)
{
    var result = new double[length];
    double squaredNorm = 0.0;
    for (int i = 0; i < result.Length; i++)
    {
        double angle = (i + 1) * 0.6180339887498948;
        result[i] = cosine ? System.Math.Cos(angle) : System.Math.Sin(angle);
        squaredNorm += result[i] * result[i];
    }
    double scale = norm / System.Math.Sqrt(squaredNorm);
    for (int i = 0; i < result.Length; i++)
        result[i] *= scale;
    return result;
}

static double VectorScaledError(ReadOnlySpan<double> actual, ReadOnlySpan<double> expected)
{
    if (actual.Length != expected.Length)
        return double.PositiveInfinity;
    double numerator = 0.0;
    double denominator = 1.0;
    for (int i = 0; i < actual.Length; i++)
    {
        double difference = actual[i] - expected[i];
        numerator += difference * difference;
        denominator += expected[i] * expected[i];
    }
    return System.Math.Sqrt(numerator / denominator);
}

static string Sha(string path) =>
    Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

sealed class BindingSpec(string id, string path, string expectedSha256)
{
    public string Id { get; } = id;
    public string Path { get; } = path;
    public string ExpectedSha256 { get; } = expectedSha256;
}

sealed class BindingResult(
    string id,
    string path,
    string expectedSha256,
    string actualSha256,
    bool hashMatches)
{
    public string Id { get; } = id;
    public string Path { get; } = path;
    public string ExpectedSha256 { get; } = expectedSha256;
    public string ActualSha256 { get; } = actualSha256;
    public bool HashMatches { get; } = hashMatches;
}
