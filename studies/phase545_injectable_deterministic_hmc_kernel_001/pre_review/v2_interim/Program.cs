using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Phase545;

const string Root = "studies/phase545_injectable_deterministic_hmc_kernel_001";
const string ContractPath = Root + "/preregistration/phase545_injectable_deterministic_hmc_kernel_repair_contract_v2.json";
const string KernelPath = Root + "/PilotKernel.cs";
const string OutputPath = Root + "/output/injectable_deterministic_hmc_kernel.json";
const string SummaryPath = Root + "/output/injectable_deterministic_hmc_kernel_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
BindingSpec[] preReviewSpecs = Bindings(contract, "preReviewBindings");
BindingSpec[] upstreamSpecs = Bindings(contract, "upstreamBindings");
BindingResult[] preReviewBindings = Verify(preReviewSpecs);
BindingResult[] upstreamBindings = Verify(upstreamSpecs);
string[] expectedPreReviewIds = ["v1-program", "v1-kernel", "v1-contract", "v1-output", "v1-summary"];
string[] expectedUpstreamIds =
[
    "phase543-summary", "phase544-contract", "phase544-program", "phase544-summary",
    "complete-lattice-gradient-source",
];
bool preReviewBindingsValid = preReviewSpecs.Select(x => x.Id).SequenceEqual(expectedPreReviewIds)
    && preReviewBindings.All(x => x.HashMatches);
bool upstreamBindingsValid = upstreamSpecs.Select(x => x.Id).SequenceEqual(expectedUpstreamIds)
    && upstreamBindings.All(x => x.HashMatches);

using var p543Doc = JsonDocument.Parse(File.ReadAllBytes(upstreamSpecs[0].Path));
using var p544Doc = JsonDocument.Parse(File.ReadAllBytes(upstreamSpecs[3].Path));
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
    "repair-contract-invalid",
    "resource-or-work-refusal-failed",
    "nonfinite-or-gradient-telemetry-failed",
    "divergence-acceptance-firewall-failed",
    "accept-or-reject-fixture-failed",
    "reversal-fixture-failed",
    "injectable-deterministic-hmc-kernel-v2-repaired",
];
string[] expectedDefects =
[
    "divergent-finite-proposal-could-be-accepted",
    "nonfinite-intermediate-position-could-reach-evaluator",
    "no-leapfrog-or-force-evaluation-work-refusal",
    "contract-firewall-and-rule-inventory-validation-was-permissive",
    "machine-readable-non-authority-firewalls-were-incomplete",
];
string[] expectedFirewallKeys =
[
    "rngUsed", "markovChainAdvanced", "warmupPerformed", "adaptationPerformed",
    "samplingPerformed", "configurationsRetained", "benchmarkPerformed", "pilotExecuted",
    "pilotExecutionAuthorized", "hmcEvidenceEstablished", "acceptanceRateEstablished",
    "stationarityEstablished", "detailedBalanceEstablished", "mixingEstablished",
    "convergenceEstablished", "observableEstimateProduced",
    "phase535ExecutedReopenedOrMutated", "phase481PackCreatedOrMutated",
    "productionDefaultSelected", "phase458G3Satisfied", "phase458G4Satisfied",
    "phase458G5Satisfied", "o4Discharged", "sourceContractApplicationAllowed",
    "physicalUnitOrGevClaimAllowed", "productionOrLaunchAllowed",
];
var expectedRules = new Dictionary<string, string>
{
    ["positionInput"] = "explicit-readonly-span",
    ["momentumInput"] = "explicit-readonly-span",
    ["acceptThresholdInput"] = "explicit-finite-log-uniform-less-than-or-equal-to-zero",
    ["proposal"] = "velocity-leapfrog-with-terminal-half-kick",
    ["hamiltonian"] = "action-plus-one-half-euclidean-momentum-norm-squared",
    ["acceptRule"] = "finite-and-nondivergent-and-log-threshold-less-than-or-equal-to-minimum-of-zero-and-negative-delta-h",
    ["rejectionState"] = "exact-clone-of-injected-position",
    ["divergenceRule"] = "non-finite-or-absolute-delta-h-greater-than-100",
    ["preallocationRefusal"] = "refuse-before-kernel-array-allocation-when-estimated-working-bytes-exceed-limit",
    ["workRefusal"] = "refuse-before-evaluator-call-and-array-allocation-when-leapfrog-steps-or-estimated-force-evaluations-exceed-limit",
    ["intermediateFiniteRule"] = "check-position-and-momentum-after-every-drift-before-evaluator-call",
};
JsonElement menu = contract.GetProperty("fixtureMenu");
JsonElement firewalls = contract.GetProperty("authorityFirewalls");
JsonElement rules = contract.GetProperty("kernelRules");
bool rulesValid = rules.EnumerateObject().Select(x => x.Name).SequenceEqual(expectedRules.Keys)
    && expectedRules.All(x => rules.GetProperty(x.Key).GetString() == x.Value);
bool firewallInventoryValid =
    contract.GetProperty("expectedAuthorityFirewallKeys").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(expectedFirewallKeys)
    && firewalls.EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && firewalls.EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False);
bool fixtureRulesValid =
    menu.GetProperty("extent").GetInt32() == 3
    && menu.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && menu.GetProperty("stateScale").GetDouble() == 0.04
    && menu.GetProperty("momentumScale").GetDouble() == 0.05
    && menu.GetProperty("maximumWorkingBytes").GetInt64() == 268435456
    && menu.GetProperty("maximumLeapfrogSteps").GetInt32() == 16
    && menu.GetProperty("maximumForceEvaluations").GetInt64() == 17
    && ObjectExact(menu, "accept",
        ("stepSize", 0.00625), ("leapfrogSteps", 4.0),
        ("injectedLogUniformThreshold", -100.0), ("expectedAccepted", true))
    && ObjectExact(menu, "reject",
        ("stepSize", 0.4), ("leapfrogSteps", 4.0),
        ("injectedLogUniformThreshold", 0.0), ("expectedAccepted", false))
    && ObjectExact(menu, "reversal",
        ("stepSize", 0.00625), ("leapfrogSteps", 4.0), ("scaledTolerance", 2e-9))
    && ObjectExact(menu, "corruption",
        ("expectedFinite", false), ("expectedDivergent", true), ("expectedAccepted", false))
    && ObjectExact(menu, "resourceRefusal",
        ("maximumWorkingBytes", 1.0), ("expectedRefused", true),
        ("expectedDivergent", false), ("expectedEvaluatorCalls", 0.0))
    && ObjectExact(menu, "workRefusal",
        ("requestedLeapfrogSteps", 17.0), ("maximumLeapfrogSteps", 16.0),
        ("maximumForceEvaluations", 17.0), ("expectedRefused", true),
        ("expectedEvaluatorCalls", 0.0))
    && ObjectExact(menu, "divergentNotAccepted",
        ("syntheticDeltaHamiltonian", -200.0), ("injectedLogUniformThreshold", -1.0),
        ("expectedFinite", true), ("expectedDivergent", true), ("expectedAccepted", false))
    && ObjectExact(menu, "intermediateOverflow",
        ("stepSize", 2.0), ("leapfrogSteps", 1.0),
        ("expectedFinite", false), ("expectedEvaluatorCalls", 1.0))
    && ObjectExact(menu, "invalidGradientLength",
        ("expectedFinite", false), ("expectedDivergent", true),
        ("expectedAccepted", false), ("expectedEvaluatorCalls", 1.0))
    && ObjectExact(menu, "exactAcceptBoundary",
        ("injectedLogUniformThreshold", 0.0), ("expectedDeltaHamiltonian", 0.0),
        ("expectedAccepted", true));
bool contractValid =
    contract.GetProperty("schemaVersion").GetInt32() == 2
    && contract.GetProperty("contractId").GetString() == "phase545-a28-injectable-deterministic-hmc-kernel-repair-v2"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A28"
    && contract.GetProperty("frozenBeforeCorrectedExecution").GetBoolean()
    && !contract.GetProperty("originalPositiveResultCitable").GetBoolean()
    && !contract.GetProperty("interimCorrectedExecutionCitable").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && preReviewBindingsValid
    && upstreamBindingsValid
    && contract.GetProperty("preReviewDefects").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(expectedDefects)
    && rulesValid
    && fixtureRulesValid
    && firewallInventoryValid
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray()
        .Select(x => x.GetString()).SequenceEqual(taxonomy)
    && contract.GetProperty("externalReviewPending").GetBoolean()
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
double[] position = DeterministicVector(degreesOfFreedom, menu.GetProperty("stateScale").GetDouble(), false);
double[] momentum = DeterministicVector(degreesOfFreedom, menu.GetProperty("momentumScale").GetDouble(), true);
long maximumWorkingBytes = menu.GetProperty("maximumWorkingBytes").GetInt64();
int maximumLeapfrogSteps = menu.GetProperty("maximumLeapfrogSteps").GetInt32();
long maximumForceEvaluations = menu.GetProperty("maximumForceEvaluations").GetInt64();

PilotEvaluation EvaluateComplete(double[] omega)
{
    var gradient = op.ComputeJointGradient(omega, thetaZero, mass);
    return new PilotEvaluation(gradient.Objective, gradient.GradOmega);
}

PilotProposal Propose(
    ReadOnlySpan<double> q,
    ReadOnlySpan<double> p,
    double step,
    int count,
    Func<double[], PilotEvaluation> evaluate) =>
    PilotKernel.ConstructProposal(q, p, step, count, maximumWorkingBytes,
        maximumLeapfrogSteps, maximumForceEvaluations, evaluate);

PilotDecision Decide(
    ReadOnlySpan<double> q,
    ReadOnlySpan<double> p,
    double step,
    int count,
    double logThreshold,
    Func<double[], PilotEvaluation> evaluate) =>
    PilotKernel.RunSingleProposal(q, p, step, count, logThreshold, maximumWorkingBytes,
        maximumLeapfrogSteps, maximumForceEvaluations, evaluate);

int resourceEvaluatorCalls = 0;
JsonElement resourceRule = menu.GetProperty("resourceRefusal");
PilotProposal resourceRefusal = PilotKernel.ConstructProposal(
    position, momentum, 0.00625, 1,
    resourceRule.GetProperty("maximumWorkingBytes").GetInt64(),
    maximumLeapfrogSteps, maximumForceEvaluations,
    q =>
    {
        resourceEvaluatorCalls++;
        return new PilotEvaluation(0.0, new double[q.Length]);
    });
bool resourceRefusalPassed =
    resourceRefusal.ResourceRefused == resourceRule.GetProperty("expectedRefused").GetBoolean()
    && resourceRefusal.Divergent == resourceRule.GetProperty("expectedDivergent").GetBoolean()
    && resourceEvaluatorCalls == resourceRule.GetProperty("expectedEvaluatorCalls").GetInt32()
    && resourceRefusal.ForceEvaluationCount == 0
    && resourceRefusal.Position is null
    && resourceRefusal.Momentum is null;

int workEvaluatorCalls = 0;
JsonElement workRule = menu.GetProperty("workRefusal");
PilotProposal workRefusal = PilotKernel.ConstructProposal(
    position, momentum, 0.00625,
    workRule.GetProperty("requestedLeapfrogSteps").GetInt32(),
    maximumWorkingBytes,
    workRule.GetProperty("maximumLeapfrogSteps").GetInt32(),
    workRule.GetProperty("maximumForceEvaluations").GetInt64(),
    q =>
    {
        workEvaluatorCalls++;
        return new PilotEvaluation(0.0, new double[q.Length]);
    });
bool workRefusalPassed =
    workRefusal.WorkRefused == workRule.GetProperty("expectedRefused").GetBoolean()
    && !workRefusal.Divergent
    && workEvaluatorCalls == workRule.GetProperty("expectedEvaluatorCalls").GetInt32()
    && workRefusal.ForceEvaluationCount == 0
    && workRefusal.Position is null
    && workRefusal.Momentum is null;

JsonElement corruptionRule = menu.GetProperty("corruption");
PilotDecision corrupted = Decide(position, momentum, 0.00625, 1, -1.0,
    q => new PilotEvaluation(double.NaN, Enumerable.Repeat(double.NaN, q.Length).ToArray()));
bool corruptionPassed =
    corrupted.Proposal.Finite == corruptionRule.GetProperty("expectedFinite").GetBoolean()
    && corrupted.Proposal.Divergent == corruptionRule.GetProperty("expectedDivergent").GetBoolean()
    && corrupted.Accepted == corruptionRule.GetProperty("expectedAccepted").GetBoolean()
    && double.IsNegativeInfinity(corrupted.LogAcceptanceThreshold);

int invalidGradientCalls = 0;
JsonElement invalidGradientRule = menu.GetProperty("invalidGradientLength");
PilotDecision invalidGradient = Decide([0.1, -0.1], [0.0, 0.0], 0.1, 1, -1.0,
    _ =>
    {
        invalidGradientCalls++;
        return new PilotEvaluation(0.0, [0.0]);
    });
bool invalidGradientPassed =
    invalidGradient.Proposal.Finite == invalidGradientRule.GetProperty("expectedFinite").GetBoolean()
    && invalidGradient.Proposal.Divergent == invalidGradientRule.GetProperty("expectedDivergent").GetBoolean()
    && invalidGradient.Accepted == invalidGradientRule.GetProperty("expectedAccepted").GetBoolean()
    && invalidGradientCalls == invalidGradientRule.GetProperty("expectedEvaluatorCalls").GetInt32();

int overflowEvaluatorCalls = 0;
JsonElement overflowRule = menu.GetProperty("intermediateOverflow");
PilotProposal overflow = Propose(
    [double.MaxValue / 2.0],
    [double.MaxValue / 2.0],
    overflowRule.GetProperty("stepSize").GetDouble(),
    overflowRule.GetProperty("leapfrogSteps").GetInt32(),
    q =>
    {
        overflowEvaluatorCalls++;
        return new PilotEvaluation(0.0, new double[q.Length]);
    });
bool overflowPassed =
    overflow.Finite == overflowRule.GetProperty("expectedFinite").GetBoolean()
    && overflow.Divergent
    && overflowEvaluatorCalls == overflowRule.GetProperty("expectedEvaluatorCalls").GetInt32()
    && overflow.ForceEvaluationCount == overflowEvaluatorCalls;

int divergentEvaluatorCalls = 0;
JsonElement divergentRule = menu.GetProperty("divergentNotAccepted");
PilotDecision divergent = Decide([0.0], [1.0], 1.0, 1,
    divergentRule.GetProperty("injectedLogUniformThreshold").GetDouble(),
    q =>
    {
        divergentEvaluatorCalls++;
        double action = divergentEvaluatorCalls == 1
            ? 0.0
            : divergentRule.GetProperty("syntheticDeltaHamiltonian").GetDouble();
        return new PilotEvaluation(action, new double[q.Length]);
    });
bool divergentNotAcceptedPassed =
    divergent.Proposal.Finite == divergentRule.GetProperty("expectedFinite").GetBoolean()
    && divergent.Proposal.Divergent == divergentRule.GetProperty("expectedDivergent").GetBoolean()
    && divergent.Accepted == divergentRule.GetProperty("expectedAccepted").GetBoolean()
    && divergent.Proposal.DeltaHamiltonian == divergentRule.GetProperty("syntheticDeltaHamiltonian").GetDouble()
    && double.IsNegativeInfinity(divergent.LogAcceptanceThreshold);

JsonElement boundaryRule = menu.GetProperty("exactAcceptBoundary");
PilotDecision boundary = Decide([0.0], [0.0], 0.1, 1,
    boundaryRule.GetProperty("injectedLogUniformThreshold").GetDouble(),
    q => new PilotEvaluation(0.0, new double[q.Length]));
bool exactAcceptBoundaryPassed =
    boundary.Proposal.DeltaHamiltonian == boundaryRule.GetProperty("expectedDeltaHamiltonian").GetDouble()
    && boundary.Accepted == boundaryRule.GetProperty("expectedAccepted").GetBoolean()
    && boundary.LogAcceptanceThreshold == boundary.InjectedLogUniformThreshold;

JsonElement acceptRule = menu.GetProperty("accept");
PilotDecision accepted = Decide(
    position, momentum,
    acceptRule.GetProperty("stepSize").GetDouble(),
    acceptRule.GetProperty("leapfrogSteps").GetInt32(),
    acceptRule.GetProperty("injectedLogUniformThreshold").GetDouble(),
    EvaluateComplete);
bool acceptPassed =
    accepted.Accepted == acceptRule.GetProperty("expectedAccepted").GetBoolean()
    && accepted.Proposal.Finite
    && !accepted.Proposal.Divergent
    && !accepted.Proposal.Refused
    && accepted.SelectedPosition is not null;

JsonElement rejectRule = menu.GetProperty("reject");
PilotDecision rejected = Decide(
    position, momentum,
    rejectRule.GetProperty("stepSize").GetDouble(),
    rejectRule.GetProperty("leapfrogSteps").GetInt32(),
    rejectRule.GetProperty("injectedLogUniformThreshold").GetDouble(),
    EvaluateComplete);
bool rejectionReturnedInjectedPosition = rejected.SelectedPosition is not null
    && VectorScaledError(rejected.SelectedPosition, position) == 0.0;
bool rejectPassed =
    rejected.Accepted == rejectRule.GetProperty("expectedAccepted").GetBoolean()
    && rejected.Proposal.Finite
    && !rejected.Proposal.Divergent
    && rejected.Proposal.DeltaHamiltonian > 0.0
    && rejectionReturnedInjectedPosition;

JsonElement reversalRule = menu.GetProperty("reversal");
PilotProposal forward = Propose(
    position, momentum,
    reversalRule.GetProperty("stepSize").GetDouble(),
    reversalRule.GetProperty("leapfrogSteps").GetInt32(),
    EvaluateComplete);
PilotProposal? reverse = forward.Position is not null && forward.Momentum is not null
    ? Propose(
        forward.Position,
        forward.Momentum.Select(x => -x).ToArray(),
        reversalRule.GetProperty("stepSize").GetDouble(),
        reversalRule.GetProperty("leapfrogSteps").GetInt32(),
        EvaluateComplete)
    : null;
double reversalError = reverse?.Position is not null && reverse.Momentum is not null
    ? System.Math.Max(
        VectorScaledError(reverse.Position, position),
        VectorScaledError(reverse.Momentum, momentum.Select(x => -x).ToArray()))
    : double.PositiveInfinity;
bool reversalPassed = forward.Finite && reverse?.Finite == true
    && reversalError <= reversalRule.GetProperty("scaledTolerance").GetDouble();

string verdict = !preReviewBindingsValid || !upstreamBindingsValid || !precursorSemanticsValid
    ? taxonomy[0]
    : !contractValid ? taxonomy[1]
    : !resourceRefusalPassed || !workRefusalPassed ? taxonomy[2]
    : !corruptionPassed || !invalidGradientPassed || !overflowPassed ? taxonomy[3]
    : !divergentNotAcceptedPassed || !exactAcceptBoundaryPassed ? taxonomy[4]
    : !acceptPassed || !rejectPassed ? taxonomy[5]
    : !reversalPassed ? taxonomy[6]
    : taxonomy[7];
bool repaired = verdict == taxonomy[7];

var output = new
{
    schemaVersion = 2,
    phase = 545,
    phaseId = "phase545-injectable-deterministic-hmc-kernel",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    kernelSha256 = Sha(KernelPath),
    contractValid,
    rulesValid,
    fixtureRulesValid,
    firewallInventoryValid,
    preReviewBindingsValid,
    upstreamBindingsValid,
    precursorSemanticsValid,
    originalPositiveResultCitable = false,
    interimCorrectedExecutionCitable = false,
    frozenBeforeCorrectedExecution = true,
    deterministicZeroSampling = true,
    rngUsed = false,
    markovChainAdvanced = false,
    warmupPerformed = false,
    adaptationPerformed = false,
    samplingPerformed = false,
    configurationsRetained = false,
    benchmarkPerformed = false,
    pilotExecuted = false,
    pilotExecutionAuthorized = false,
    hmcEvidenceEstablished = false,
    acceptanceRateEstablished = false,
    stationarityEstablished = false,
    detailedBalanceEstablished = false,
    mixingEstablished = false,
    convergenceEstablished = false,
    observableEstimateProduced = false,
    completeLattice = new
    {
        extent,
        vertexCount = mesh.VertexCount,
        edgeCount = mesh.EdgeCount,
        omegaDegreesOfFreedom = degreesOfFreedom,
    },
    fixtures = new
    {
        resourceRefusal = new
        {
            passed = resourceRefusalPassed,
            resourceRefusal.ResourceRefused,
            resourceRefusal.WorkRefused,
            resourceRefusal.Divergent,
            resourceRefusal.RefusalReason,
            evaluatorCalls = resourceEvaluatorCalls,
            resourceRefusal.ForceEvaluationCount,
            resourceRefusal.EstimatedWorkingBytes,
        },
        workRefusal = new
        {
            passed = workRefusalPassed,
            workRefusal.ResourceRefused,
            workRefusal.WorkRefused,
            workRefusal.Divergent,
            workRefusal.RefusalReason,
            evaluatorCalls = workEvaluatorCalls,
            workRefusal.ForceEvaluationCount,
            workRefusal.EstimatedForceEvaluations,
        },
        corruption = new
        {
            passed = corruptionPassed,
            corrupted.Proposal.Finite,
            corrupted.Proposal.Divergent,
            corrupted.Accepted,
            corrupted.Proposal.ForceEvaluationCount,
        },
        invalidGradientLength = new
        {
            passed = invalidGradientPassed,
            invalidGradient.Proposal.Finite,
            invalidGradient.Proposal.Divergent,
            invalidGradient.Accepted,
            evaluatorCalls = invalidGradientCalls,
        },
        intermediateOverflow = new
        {
            passed = overflowPassed,
            overflow.Finite,
            overflow.Divergent,
            evaluatorCalls = overflowEvaluatorCalls,
            overflow.ForceEvaluationCount,
        },
        divergentNotAccepted = new
        {
            passed = divergentNotAcceptedPassed,
            divergent.Proposal.Finite,
            divergent.Proposal.Divergent,
            divergent.Accepted,
            divergent.Proposal.DeltaHamiltonian,
            logAcceptanceIsNegativeInfinity =
                double.IsNegativeInfinity(divergent.LogAcceptanceThreshold),
            evaluatorCalls = divergentEvaluatorCalls,
        },
        exactAcceptBoundary = new
        {
            passed = exactAcceptBoundaryPassed,
            boundary.Accepted,
            boundary.Proposal.DeltaHamiltonian,
            boundary.InjectedLogUniformThreshold,
            boundary.LogAcceptanceThreshold,
        },
        accept = new
        {
            passed = acceptPassed,
            accepted.Accepted,
            accepted.Proposal.Finite,
            accepted.Proposal.Divergent,
            accepted.Proposal.DeltaHamiltonian,
            accepted.InjectedLogUniformThreshold,
            accepted.LogAcceptanceThreshold,
            accepted.Proposal.ForceEvaluationCount,
        },
        reject = new
        {
            passed = rejectPassed,
            rejected.Accepted,
            rejected.Proposal.Finite,
            rejected.Proposal.Divergent,
            rejected.Proposal.DeltaHamiltonian,
            rejected.InjectedLogUniformThreshold,
            rejected.LogAcceptanceThreshold,
            rejectionReturnedInjectedPosition,
            rejected.Proposal.ForceEvaluationCount,
        },
        reversal = new
        {
            passed = reversalPassed,
            forwardFinite = forward.Finite,
            reverseFinite = reverse?.Finite == true,
            scaledError = reversalError,
            tolerance = reversalRule.GetProperty("scaledTolerance").GetDouble(),
        },
    },
    reusableKernelV2Repaired = repaired,
    verdictKind = verdict,
    terminalStatus = "injectable-deterministic-hmc-kernel-" + verdict,
    decision = repaired
        ? "The fail-closed v2 repair fixtures pass. This remains deterministic kernel implementation evidence only; no pilot, HMC evidence, acceptance rate, stationarity, detailed balance, mixing, convergence, or observable estimate is established."
        : "The earliest frozen Phase545 v2 repair failure is preserved. The original positive result remains non-citable and no later authority follows.",
    laterDependentPackReadinessAdjudicationAllowed = repaired,
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
    preReviewBindings,
    upstreamBindings,
};

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(output, options);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase545 v2 verdict: {verdict}");
Console.WriteLine(
    $"resource={resourceRefusalPassed}, work={workRefusalPassed}, corrupt={corruptionPassed}, " +
    $"gradient={invalidGradientPassed}, overflow={overflowPassed}, divergent={divergentNotAcceptedPassed}, " +
    $"boundary={exactAcceptBoundaryPassed}, accept={acceptPassed}, reject={rejectPassed}, reversal={reversalPassed}");
Console.WriteLine("rng=False, chain=False, warmup=False, sampling=False, pilot=False");

static BindingSpec[] Bindings(JsonElement contract, string property) =>
    contract.GetProperty(property).EnumerateArray().Select(x => new BindingSpec(
        x.GetProperty("id").GetString()!,
        x.GetProperty("path").GetString()!,
        x.GetProperty("sha256").GetString()!)).ToArray();

static BindingResult[] Verify(BindingSpec[] specs) =>
    specs.Select(x =>
    {
        string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
        return new BindingResult(x.Id, x.Path, x.ExpectedSha256, actual, actual == x.ExpectedSha256);
    }).ToArray();

static bool ObjectExact(JsonElement parent, string name, params (string Name, object Value)[] expected)
{
    JsonElement value = parent.GetProperty(name);
    if (!value.EnumerateObject().Select(x => x.Name).SequenceEqual(expected.Select(x => x.Name)))
        return false;
    foreach ((string property, object target) in expected)
    {
        JsonElement actual = value.GetProperty(property);
        bool matches = target switch
        {
            bool boolean => actual.ValueKind is JsonValueKind.True or JsonValueKind.False
                && actual.GetBoolean() == boolean,
            double number => actual.ValueKind == JsonValueKind.Number && actual.GetDouble() == number,
            string text => actual.ValueKind == JsonValueKind.String && actual.GetString() == text,
            _ => false,
        };
        if (!matches)
            return false;
    }
    return true;
}

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
