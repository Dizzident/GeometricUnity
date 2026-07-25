using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Phase545;

const string Root = "studies/phase548_bounded_complete_lattice_pilot_execution_001";
const string ContractPath = Root + "/preregistration/phase548_bounded_complete_lattice_pilot_execution_contract_v1.json";
const string OutputPath = Root + "/output/bounded_complete_lattice_pilot_execution.json";
const string SummaryPath = Root + "/output/bounded_complete_lattice_pilot_execution_summary.json";
const string TelemetryDir = Root + "/output/telemetry";
const string CheckpointDir = Root + "/output/checkpoints";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;

// ------------------------------------------------------------------- bindings
var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new
{
    Id = x.GetProperty("id").GetString()!,
    Path = x.GetProperty("path").GetString()!,
    ExpectedSha256 = x.GetProperty("sha256").GetString()!,
}).ToArray();
var bindings = bindingSpecs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { x.Id, x.Path, x.ExpectedSha256, ActualSha256 = actual, HashMatches = actual == x.ExpectedSha256 };
}).ToArray();
string[] expectedBindingIds =
[
    "phase533-contract", "phase543-summary", "phase545-contract", "phase545-kernel", "phase545-program",
    "phase545-summary", "phase546-v1-contract", "phase546-contract", "phase546-summary",
    "phase547-contract", "phase547-program", "phase547-summary", "complete-lattice-gradient-source",
];
bool exactBindingsValid = bindingSpecs.Select(x => x.Id).SequenceEqual(expectedBindingIds)
    && bindings.All(x => x.HashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input", "upstream-registration-authorization-absent", "resource-refusal",
    "deterministic-precheck-failed", "checkpoint-restart-inequivalence",
    "pilot-execution-nonfinite-or-divergent", "pilot-executed-diagnostics-invalid",
    "pilot-executed-diagnostics-valid",
];

JsonElement target = contract.GetProperty("target");
JsonElement defaults = contract.GetProperty("defaultConfiguration");
JsonElement resourceRule = contract.GetProperty("resourceRule");
JsonElement prechecks = contract.GetProperty("deterministicPrechecks");
JsonElement checkpointRule = contract.GetProperty("checkpointRule");
JsonElement telemetrySchema = contract.GetProperty("telemetrySchema");
JsonElement premise = contract.GetProperty("registrationPremise");

string[] trajectoryFields = telemetrySchema.GetProperty("trajectoryRequiredFields")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] chainFields = telemetrySchema.GetProperty("chainSummaryRequiredFields")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] observableNames = telemetrySchema.GetProperty("observableSchema").GetProperty("requiredNames")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
string observableSchemaId = telemetrySchema.GetProperty("observableSchema").GetProperty("schemaId").GetString()!;
JsonElement thresholds = telemetrySchema.GetProperty("convergenceThresholds");
double maximumRhat = thresholds.GetProperty("maximumSplitRankNormalizedRhat").GetDouble();
double minimumBulkEss = thresholds.GetProperty("minimumBulkEss").GetDouble();
double minimumTailEss = thresholds.GetProperty("minimumTailEss").GetDouble();

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase548-a29-bounded-complete-lattice-pilot-execution-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A29"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("executesBoundedPilot").GetBoolean()
    && exactBindingsValid
    && taxonomy.SequenceEqual(expectedTaxonomy)
    && observableNames.SequenceEqual(["actionDensity", "forceNormSquared", "configurationNormSquared"])
    && observableSchemaId == "complete-lattice-bounded-pilot-observables-v1"
    && maximumRhat == 1.01 && minimumBulkEss == 100 && minimumTailEss == 100
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// Forwarded pack elements must equal their frozen upstream source verbatim.
using var p546Document = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs.First(x => x.Id == "phase546-v1-contract").Path));
JsonElement p546 = p546Document.RootElement;
JsonElement p546Telemetry = p546.GetProperty("telemetrySchema");
JsonElement p546Checkpoint = p546.GetProperty("checkpointSchema");
JsonElement p546Resource = p546.GetProperty("resourceRefusal");
bool forwardingFaithful =
    p546Telemetry.GetProperty("trajectoryRequiredFields").EnumerateArray().Select(x => x.GetString()).SequenceEqual(trajectoryFields)
    && p546Telemetry.GetProperty("chainSummaryRequiredFields").EnumerateArray().Select(x => x.GetString()).SequenceEqual(chainFields)
    && p546Telemetry.GetProperty("observableSchema").GetProperty("requiredNames").EnumerateArray().Select(x => x.GetString()).SequenceEqual(observableNames)
    && p546Telemetry.GetProperty("observableSchema").GetProperty("schemaId").GetString() == observableSchemaId
    && p546Telemetry.GetProperty("convergenceThresholds").GetProperty("maximumSplitRankNormalizedRhat").GetDouble() == maximumRhat
    && p546Telemetry.GetProperty("convergenceThresholds").GetProperty("minimumBulkEss").GetInt32() == (int)minimumBulkEss
    && p546Telemetry.GetProperty("convergenceThresholds").GetProperty("minimumTailEss").GetInt32() == (int)minimumTailEss
    && p546Telemetry.GetProperty("divergenceAbsoluteDeltaH").GetDouble() == defaults.GetProperty("divergenceAbsoluteDeltaH").GetDouble()
    && p546Checkpoint.GetProperty("formatId").GetString() == checkpointRule.GetProperty("formatId").GetString()
    && p546Checkpoint.GetProperty("actionFingerprint").GetString() == checkpointRule.GetProperty("actionFingerprint").GetString()
    && p546Checkpoint.GetProperty("packFingerprint").GetString() == checkpointRule.GetProperty("packFingerprint").GetString()
    && p546Checkpoint.GetProperty("exactRestartFields").EnumerateArray().Select(x => x.GetString())
        .SequenceEqual(checkpointRule.GetProperty("exactRestartFields").EnumerateArray().Select(x => x.GetString()))
    && p546Resource.GetProperty("maximumAggregateCpuTicks").GetInt64() == resourceRule.GetProperty("maximumAggregateCpuTicks").GetInt64()
    && p546Resource.GetProperty("maximumPeakBytes").GetInt64() == resourceRule.GetProperty("maximumPeakBytes").GetInt64()
    && p546Resource.GetProperty("maximumChains").GetInt32() == resourceRule.GetProperty("maximumChains").GetInt32()
    && p546Resource.GetProperty("bytesPerDegreeOfFreedom").GetInt64() == resourceRule.GetProperty("bytesPerDegreeOfFreedom").GetInt64()
    && p546Resource.GetProperty("fixedBytesPerChain").GetInt64() == resourceRule.GetProperty("fixedBytesPerChain").GetInt64()
    && p546Resource.GetProperty("cpuTicksPerForceDegreeOfFreedom").GetInt64() == resourceRule.GetProperty("cpuTicksPerForceDegreeOfFreedom").GetInt64()
    && p546Resource.GetProperty("refuseOnEqualOrExceedLimit").GetBoolean() == resourceRule.GetProperty("refuseOnEqualOrExceedLimit").GetBoolean();

// Registered seeds must be a frozen-order prefix of the Phase546 prospective tables.
var p546Tables = p546.GetProperty("prospectiveSeedTables").EnumerateArray().Select(x => new
{
    Id = x.GetProperty("id").GetString()!,
    Offset = x.GetProperty("seedOffset").GetInt32(),
    Seeds = x.GetProperty("seeds").EnumerateArray().Select(y => y.GetInt32()).ToArray(),
}).ToArray();
var tables = contract.GetProperty("seedTables").EnumerateArray().Select(x => new
{
    Id = x.GetProperty("id").GetString()!,
    Offset = x.GetProperty("seedOffset").GetInt32(),
    Seeds = x.GetProperty("seeds").EnumerateArray().Select(y => y.GetInt32()).ToArray(),
    InitialScales = x.GetProperty("initialScales").EnumerateArray().Select(y => y.GetDouble()).ToArray(),
    ExcludedSeed = x.GetProperty("excludedFrozenSeed").GetInt32(),
}).ToArray();
int chainsPerTable = defaults.GetProperty("chainsPerTable").GetInt32();
bool seedProvenanceValid = tables.Length == p546Tables.Length && tables.Zip(p546Tables).All(pair =>
    pair.First.Id == pair.Second.Id
    && pair.First.Offset == pair.Second.Offset
    && pair.First.Seeds.Length == chainsPerTable
    && pair.First.InitialScales.Length == chainsPerTable
    && pair.First.Seeds.SequenceEqual(pair.Second.Seeds.Take(chainsPerTable))
    && pair.Second.Seeds.Skip(chainsPerTable).SequenceEqual([pair.First.ExcludedSeed]));

bool inputsValid = contractValid && forwardingFaithful && seedProvenanceValid;

// -------------------------------------------- upstream registration premise
using var p547Document = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs.First(x => x.Id == "phase547-summary").Path));
JsonElement p547 = p547Document.RootElement;
bool registrationAuthorized =
    p547.GetProperty("verdictKind").GetString() == premise.GetProperty("requiredPhase547VerdictKind").GetString()
    && p547.GetProperty("boundedPilotExecutionPhaseRegistrationAllowed").GetBoolean() == premise.GetProperty("requiredPhase547RegistrationAllowed").GetBoolean()
    && p547.GetProperty("pilotExecutionAuthorized").GetBoolean() == premise.GetProperty("requiredPhase547PilotExecutionAuthorized").GetBoolean();

// ------------------------------------------------------------- frozen target
int extent = target.GetProperty("extent").GetInt32();
int dimensions = target.GetProperty("dimensions").GetInt32();
string topologyId = target.GetProperty("topologyId").GetString()!;
double beta = target.GetProperty("beta").GetDouble();
string thetaRule = target.GetProperty("thetaRule").GetString()!;
double stepSize = defaults.GetProperty("stepSize").GetDouble();
int leapfrogSteps = defaults.GetProperty("leapfrogSteps").GetInt32();
int warmupPerChain = defaults.GetProperty("warmupPerChain").GetInt32();
int retainedPerChain = defaults.GetProperty("retainedPerChain").GetInt32();
int trajectoriesPerChain = defaults.GetProperty("trajectoriesPerChain").GetInt32();
double divergenceThreshold = defaults.GetProperty("divergenceAbsoluteDeltaH").GetDouble();
double minimumAcceptance = defaults.GetProperty("minimumAcceptanceRate").GetDouble();
int maximumNonFinite = defaults.GetProperty("maximumNonFiniteTrajectories").GetInt32();
int maximumDivergent = defaults.GetProperty("maximumDivergentTrajectories").GetInt32();

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
var massMatrix = new CpuMassMatrix(mesh, algebra);
double[] thetaZero = new double[mesh.VertexCount * algebra.Dimension];
int dof = mesh.EdgeCount * algebra.Dimension;
bool topologyValid = dof == 45 * extent * extent * extent * extent
    && dof == target.GetProperty("degreesOfFreedom").GetInt32()
    && trajectoriesPerChain == warmupPerChain + retainedPerChain;

long forceEvaluations = 0;
double[] lastPosition = new double[dof];
double lastAction = double.NaN;
double[] lastGradient = new double[dof];
double proposalMaximumForceNorm = 0.0;
PilotEvaluation Evaluate(double[] omega)
{
    forceEvaluations++;
    var gradient = op.ComputeJointGradient(omega, thetaZero, massMatrix);
    lastPosition = (double[])omega.Clone();
    lastAction = gradient.Objective;
    lastGradient = gradient.GradOmega;
    double norm = System.Math.Sqrt(Dot(gradient.GradOmega, gradient.GradOmega));
    if (double.IsFinite(norm)) proposalMaximumForceNorm = System.Math.Max(proposalMaximumForceNorm, norm);
    return new PilotEvaluation(gradient.Objective, gradient.GradOmega);
}

// ------------------------------------------- resource rule (before allocation)
long maximumCpuTicks = resourceRule.GetProperty("maximumAggregateCpuTicks").GetInt64();
long maximumPeakBytes = resourceRule.GetProperty("maximumPeakBytes").GetInt64();
long ticksPerForceDof = resourceRule.GetProperty("cpuTicksPerForceDegreeOfFreedom").GetInt64();
long bytesPerDof = resourceRule.GetProperty("bytesPerDegreeOfFreedom").GetInt64();
long fixedBytesPerChain = resourceRule.GetProperty("fixedBytesPerChain").GetInt64();
int maximumChains = resourceRule.GetProperty("maximumChains").GetInt32();
int requiredDimensions = resourceRule.GetProperty("requiredDimensions").GetInt32();

ResourceAssessment Assess(string requestTopology, int requestDimensions, int requestExtent,
    int chains, int requestDof, int trajectories, int leaps, long extraTicks)
{
    try
    {
        long cpu = checked((long)chains * trajectories * leaps * requestDof * ticksPerForceDof + extraTicks);
        long memory = checked((long)chains * (fixedBytesPerChain + checked((long)requestDof * bytesPerDof)));
        string? refusal = requestTopology != topologyId ? "topology-mismatch"
            : requestDimensions != requiredDimensions ? "dimension-mismatch"
            : requestExtent <= 0 || chains <= 0 || chains > maximumChains
                || requestDof != checked(45 * requestExtent * requestExtent * requestExtent * requestExtent)
                || trajectories <= 0 || leaps <= 0 ? "invalid-shape"
            : cpu >= maximumCpuTicks ? "cpu-boundary-or-limit-exceeded"
            : memory >= maximumPeakBytes ? "memory-boundary-or-limit-exceeded"
            : null;
        return new ResourceAssessment(refusal is null, refusal, cpu, memory);
    }
    catch (OverflowException)
    {
        return new ResourceAssessment(false, "checked-arithmetic-overflow", long.MaxValue, long.MaxValue);
    }
}

var samplingAssessments = tables.Select(t => new
{
    tableId = t.Id,
    assessment = Assess(topologyId, dimensions, extent, chainsPerTable, dof, trajectoriesPerChain, leapfrogSteps, 0L),
}).ToArray();
var fourChainWitness = Assess(topologyId, dimensions, extent, chainsPerTable + 1, dof, trajectoriesPerChain, leapfrogSteps, 0L);
bool samplingResourceAllowed = samplingAssessments.All(x => x.assessment.Allowed);

// -------------------------------------- deterministic prechecks (zero rng used)
JsonElement spectral = prechecks.GetProperty("spectralProbe");
double hessianStep = spectral.GetProperty("hessianVectorStep").GetDouble();
int largestIterations = spectral.GetProperty("largestEigenvalueIterations").GetInt32();
int smallestIterations = spectral.GetProperty("smallestEigenvalueIterations").GetInt32();
double shiftFactor = spectral.GetProperty("shiftFactor").GetDouble();
double fdStep = prechecks.GetProperty("finiteDifferenceRelativeStep").GetDouble();
double fdTolerance = prechecks.GetProperty("finiteDifferenceScaledTolerance").GetDouble();
double reverseTolerance = prechecks.GetProperty("forwardReverseScaledTolerance").GetDouble();
double probeScale = prechecks.GetProperty("deterministicProbeStateScale").GetDouble();

var origin = new double[dof];
PilotEvaluation originEvaluation = Evaluate(origin);
double originGradientNorm = System.Math.Sqrt(Dot(originEvaluation.Gradient, originEvaluation.Gradient));
bool originExact = originEvaluation.Action == 0.0 && originGradientNorm == 0.0;

double[] sineDirection = GoldenAngleUnit(dof, cosine: false);
double[] cosineDirection = GoldenAngleUnit(dof, cosine: true);
double[] probeState = sineDirection.Select(x => x * probeScale * System.Math.Sqrt(dof)).ToArray();

double[] HessianVector(double[] direction)
{
    double[] plus = Evaluate(Add(origin, direction, hessianStep)).Gradient;
    double[] minus = Evaluate(Add(origin, direction, -hessianStep)).Gradient;
    var result = new double[dof];
    for (int i = 0; i < dof; i++) result[i] = (plus[i] - minus[i]) / (2.0 * hessianStep);
    return result;
}
double largestEigenvalue = 0.0;
double[] iterate = sineDirection;
for (int i = 0; i < largestIterations; i++)
{
    double[] w = HessianVector(iterate);
    largestEigenvalue = Dot(iterate, w);
    iterate = Normalize(w);
}
double shift = System.Math.Abs(largestEigenvalue) * shiftFactor;
double smallestEigenvalue = 0.0;
double[] deflated = cosineDirection;
for (int i = 0; i < smallestIterations; i++)
{
    double[] w = HessianVector(deflated);
    var z = new double[dof];
    for (int k = 0; k < dof; k++) z[k] = shift * deflated[k] - w[k];
    smallestEigenvalue = shift - Dot(deflated, z);
    deflated = Normalize(z);
}
double stabilityBound = 2.0 / System.Math.Sqrt(System.Math.Abs(largestEigenvalue));
bool spectralValid = double.IsFinite(largestEigenvalue) && largestEigenvalue > 0.0
    && double.IsFinite(stabilityBound) && stepSize < stabilityBound;

PilotEvaluation probeEvaluation = Evaluate(probeState);
double probeStep = fdStep * System.Math.Max(1.0, System.Math.Sqrt(Dot(probeState, probeState)));
double plusAction = Evaluate(Add(probeState, cosineDirection, probeStep)).Action;
double minusAction = Evaluate(Add(probeState, cosineDirection, -probeStep)).Action;
double directionalGradientError = ScaledError(
    (plusAction - minusAction) / (2.0 * probeStep), Dot(probeEvaluation.Gradient, cosineDirection));
bool gradientValid = double.IsFinite(probeEvaluation.Action)
    && probeEvaluation.Gradient.All(double.IsFinite)
    && directionalGradientError <= fdTolerance;

(double[] Position, double[] Momentum, bool Finite) Leapfrog(double[] q0, double[] p0)
{
    double[] q = (double[])q0.Clone();
    double[] p = (double[])p0.Clone();
    PilotEvaluation evaluation = Evaluate(q);
    if (!double.IsFinite(evaluation.Action)) return (q, p, false);
    double[] gradient = evaluation.Gradient;
    for (int i = 0; i < dof; i++) p[i] -= 0.5 * stepSize * gradient[i];
    for (int leap = 0; leap < leapfrogSteps; leap++)
    {
        for (int i = 0; i < dof; i++) q[i] += stepSize * p[i];
        evaluation = Evaluate(q);
        if (!double.IsFinite(evaluation.Action) || !evaluation.Gradient.All(double.IsFinite)) return (q, p, false);
        gradient = evaluation.Gradient;
        double kick = leap + 1 == leapfrogSteps ? 0.5 * stepSize : stepSize;
        for (int i = 0; i < dof; i++) p[i] -= kick * gradient[i];
    }
    return (q, p, q.All(double.IsFinite) && p.All(double.IsFinite));
}
var forwardRun = Leapfrog(probeState, cosineDirection);
double reversibilityError = double.PositiveInfinity;
if (forwardRun.Finite)
{
    var back = Leapfrog(forwardRun.Position, forwardRun.Momentum.Select(x => -x).ToArray());
    if (back.Finite)
        reversibilityError = System.Math.Max(
            VectorScaledError(back.Position, probeState),
            VectorScaledError(back.Momentum, cosineDirection.Select(x => -x).ToArray()));
}
bool reversibilityValid = double.IsFinite(reversibilityError) && reversibilityError <= reverseTolerance;
bool prechecksPassed = topologyValid && originExact && spectralValid && gradientValid && reversibilityValid;

// ------------------------------------ live checkpoint restart equivalence control
JsonElement restartRule = checkpointRule.GetProperty("restartEquivalenceControl");
int restartTotal = restartRule.GetProperty("uninterruptedTrajectories").GetInt32();
int restartBreak = restartRule.GetProperty("interruptAfterTrajectories").GetInt32();
string restartChainId = restartRule.GetProperty("chainId").GetString()!;
int restartSeed = tables[0].Seeds[0] + tables[0].Offset;
double restartScale = tables[0].InitialScales[0];

ChainState AdvanceOne(ChainState state)
{
    var momentum = new double[dof];
    for (int i = 0; i < dof; i++) momentum[i] = Gauss(state.Rng);
    double logUniform = System.Math.Log(Uniform(state.Rng));
    proposalMaximumForceNorm = 0.0;
    PilotDecision decision = PilotKernel.RunSingleProposal(
        state.Position, momentum, stepSize, leapfrogSteps, logUniform,
        fixedBytesPerChain + dof * bytesPerDof, leapfrogSteps, leapfrogSteps + 1L, Evaluate, divergenceThreshold);
    double[] next = decision.SelectedPosition ?? state.Position;
    return state with
    {
        Position = next,
        Index = state.Index + 1,
        LastDecision = decision,
        LastMaximumForceNorm = proposalMaximumForceNorm,
    };
}
ChainState RunSegment(ChainState state, int trajectories)
{
    for (int t = 0; t < trajectories; t++) state = AdvanceOne(state);
    return state;
}

var uninterrupted = RunSegment(NewChain(restartSeed, restartScale), restartTotal);
var interrupted = RunSegment(NewChain(restartSeed, restartScale), restartBreak);
CheckpointState midpoint = BuildCheckpoint(restartChainId, restartSeed, interrupted, restartBreak, 0);
byte[] encodedCheckpoint = CheckpointCodec.Encode(midpoint, checkpointRule);
CheckpointState decodedCheckpoint = CheckpointCodec.Decode(encodedCheckpoint, checkpointRule);
var resumed = RunSegment(new ChainState(
    (double[])decodedCheckpoint.Position.Clone(),
    new Xoshiro(new RngState(decodedCheckpoint.RngStateWords[0], decodedCheckpoint.RngStateWords[1],
        decodedCheckpoint.RngStateWords[2], decodedCheckpoint.RngStateWords[3])),
    decodedCheckpoint.TrajectoryIndex, null, 0.0), restartTotal - restartBreak);
bool checkpointPreservesState = BitIdentical(decodedCheckpoint.Position, interrupted.Position);
bool restartPositionIdentical = BitIdentical(uninterrupted.Position, resumed.Position);
bool restartRngIdentical = uninterrupted.Rng.State.Words.SequenceEqual(resumed.Rng.State.Words);
bool restartEquivalent = checkpointPreservesState && restartPositionIdentical && restartRngIdentical;

// -------------------------------------------- total resource re-assessment
long controlForceEvaluations = forceEvaluations;
long controlTicks = checked(controlForceEvaluations * dof * ticksPerForceDof);
var totalAssessments = tables.Select(t => new
{
    tableId = t.Id,
    assessment = Assess(topologyId, dimensions, extent, chainsPerTable, dof, trajectoriesPerChain, leapfrogSteps, controlTicks),
}).ToArray();
bool totalResourceAllowed = totalAssessments.All(x => x.assessment.Allowed);
bool resourceAllowed = samplingResourceAllowed && totalResourceAllowed;

// ---------------------------------------------------------------- sampling
Directory.CreateDirectory(TelemetryDir);
Directory.CreateDirectory(CheckpointDir);
var chainRecords = new List<ChainRecord>();
bool samplingPerformed = false;
if (inputsValid && registrationAuthorized && resourceAllowed && prechecksPassed && restartEquivalent)
{
    samplingPerformed = true;
    foreach (var table in tables)
    {
        for (int c = 0; c < chainsPerTable; c++)
        {
            int rawSeed = table.Seeds[c];
            int executionSeed = rawSeed + table.Offset;
            string chainId = $"{table.Id}-{rawSeed}";
            var state = NewChain(executionSeed, table.InitialScales[c]);
            var trajectoryRows = new List<TrajectoryRow>();
            var draws = observableNames.ToDictionary(n => n, _ => new List<double>());
            int accepted = 0, nonFiniteCount = 0, divergenceCount = 0;
            double maximumAbsoluteDeltaH = 0.0;
            bool halted = false;
            var stopwatch = new Stopwatch();
            for (int t = 0; t < trajectoriesPerChain; t++)
            {
                stopwatch.Restart();
                state = AdvanceOne(state);
                stopwatch.Stop();
                PilotDecision decision = state.LastDecision!;
                PilotProposal proposal = decision.Proposal;
                bool nonFinite = !proposal.Finite;
                if (nonFinite) nonFiniteCount++;
                if (proposal.Divergent) divergenceCount++;
                if (decision.Accepted) accepted++;
                if (double.IsFinite(proposal.DeltaHamiltonian))
                    maximumAbsoluteDeltaH = System.Math.Max(maximumAbsoluteDeltaH, System.Math.Abs(proposal.DeltaHamiltonian));
                trajectoryRows.Add(new TrajectoryRow(chainId, t, t < warmupPerChain ? "warmup" : "retained",
                    decision.Accepted, proposal.InitialHamiltonian, proposal.FinalHamiltonian, proposal.DeltaHamiltonian,
                    double.IsFinite(decision.LogAcceptanceThreshold) ? System.Math.Exp(decision.LogAcceptanceThreshold) : 0.0,
                    decision.InjectedLogUniformThreshold, nonFinite, proposal.Divergent, divergenceThreshold,
                    proposal.ForceEvaluationCount, state.LastMaximumForceNorm, stopwatch.ElapsedTicks));
                if (t >= warmupPerChain)
                {
                    (double action, double[] gradient) = CurrentValues(state.Position);
                    draws["actionDensity"].Add(action / dof);
                    draws["forceNormSquared"].Add(Dot(gradient, gradient));
                    draws["configurationNormSquared"].Add(Dot(state.Position, state.Position));
                }
                if (nonFiniteCount > maximumNonFinite || divergenceCount > maximumDivergent) { halted = true; break; }
            }
            int warmupCompleted = (int)System.Math.Min(state.Index, (long)warmupPerChain);
            int retainedCompleted = (int)System.Math.Max(0L, state.Index - warmupPerChain);
            CheckpointState finalCheckpoint = BuildCheckpoint(chainId, executionSeed, state, warmupCompleted, retainedCompleted);
            byte[] finalEncoded = CheckpointCodec.Encode(finalCheckpoint, checkpointRule);
            string checkpointPath = $"{CheckpointDir}/{chainId}.json";
            File.WriteAllBytes(checkpointPath, finalEncoded);
            CheckpointCodec.Decode(File.ReadAllBytes(checkpointPath), checkpointRule);

            string telemetryPath = $"{TelemetryDir}/{chainId}_trajectories.json";
            File.WriteAllBytes(telemetryPath, JsonSerializer.SerializeToUtf8Bytes(
                new { chainId, seed = executionSeed, rows = trajectoryRows },
                new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

            chainRecords.Add(new ChainRecord(table.Id, chainId, executionSeed, rawSeed,
                warmupCompleted, draws[observableNames[0]].Count,
                (double)accepted / System.Math.Max(1L, state.Index), nonFiniteCount, divergenceCount,
                maximumAbsoluteDeltaH, halted, draws.ToDictionary(x => x.Key, x => x.Value.ToArray()),
                observableNames.ToDictionary(n => n, n => Diagnose([draws[n].ToArray()])),
                Convert.ToHexString(SHA256.HashData(finalEncoded)).ToLowerInvariant(),
                TimingStrippedHash(trajectoryRows)));
        }
    }
}

// -------------------------------------------------------------- diagnostics
bool executionClean = samplingPerformed
    && chainRecords.All(x => x.NonFiniteCount <= maximumNonFinite && x.DivergenceCount <= maximumDivergent && !x.Halted);
var tableDiagnostics = tables.Select(table =>
{
    var members = chainRecords.Where(x => x.TableId == table.Id).ToArray();
    var perObservable = observableNames.Select(name =>
    {
        Diagnostics d = members.Length == 0
            ? new Diagnostics(double.NaN, double.NaN, double.NaN)
            : Diagnose(members.Select(m => m.Observables[name]).ToArray());
        bool rhatOk = double.IsFinite(d.SplitRankNormalizedRhat) && d.SplitRankNormalizedRhat <= maximumRhat;
        bool bulkOk = double.IsFinite(d.BulkEss) && d.BulkEss >= minimumBulkEss;
        bool tailOk = double.IsFinite(d.TailEss) && d.TailEss >= minimumTailEss;
        return new
        {
            name,
            gaugeClass = telemetrySchema.GetProperty("gaugeInvarianceClassification").GetProperty(name).GetString(),
            splitRankNormalizedRhat = d.SplitRankNormalizedRhat,
            bulkEss = d.BulkEss,
            tailEss = d.TailEss,
            rhatPassed = rhatOk,
            bulkEssPassed = bulkOk,
            tailEssPassed = tailOk,
            passed = rhatOk && bulkOk && tailOk,
        };
    }).ToArray();
    double minimumObservedAcceptance = members.Length == 0 ? double.NaN : members.Min(x => x.AcceptanceRate);
    bool acceptanceOk = members.Length == chainsPerTable && minimumObservedAcceptance >= minimumAcceptance;
    return new
    {
        tableId = table.Id,
        chainCount = members.Length,
        minimumObservedAcceptance,
        acceptancePassed = acceptanceOk,
        observables = perObservable,
        passed = acceptanceOk && perObservable.All(x => x.passed),
    };
}).ToArray();
bool diagnosticsValid = samplingPerformed && executionClean && tableDiagnostics.All(x => x.passed);

var failingObservables = tableDiagnostics.SelectMany(t => t.observables.Where(o => !o.passed)
    .Select(o => new { table = t.tableId, o.name, o.gaugeClass, o.splitRankNormalizedRhat, o.bulkEss, o.tailEss })).ToArray();
bool everyFailingObservableIsGaugeVariant = failingObservables.Length > 0
    && failingObservables.All(x => x.gaugeClass == "gauge-variant");
bool everyGaugeInvariantObservablePassed = samplingPerformed && tableDiagnostics
    .SelectMany(t => t.observables.Where(o => o.gaugeClass == "gauge-invariant")).All(o => o.passed);

// ------------------------------------------------------------------ verdict
string verdict = !inputsValid ? taxonomy[0]
    : !registrationAuthorized ? taxonomy[1]
    : !resourceAllowed ? taxonomy[2]
    : !prechecksPassed ? taxonomy[3]
    : !restartEquivalent ? taxonomy[4]
    : !executionClean ? taxonomy[5]
    : !diagnosticsValid ? taxonomy[6]
    : taxonomy[7];

var output = new
{
    schemaVersion = 1,
    phase = 548,
    phaseId = "phase548-bounded-complete-lattice-pilot-execution",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    forwardingFaithful,
    seedProvenanceValid,
    inputsValid,
    bindings,
    registrationPremise = new
    {
        phase547VerdictKind = p547.GetProperty("verdictKind").GetString(),
        phase547RegistrationAllowed = p547.GetProperty("boundedPilotExecutionPhaseRegistrationAllowed").GetBoolean(),
        phase547PilotExecutionAuthorized = p547.GetProperty("pilotExecutionAuthorized").GetBoolean(),
        registrationAuthorized,
        executionAuthorityIsThisFrozenContractOnly = true,
    },
    completeLattice = new
    {
        extent, dimensions, topologyId, beta, thetaRule,
        vertexCount = mesh.VertexCount, edgeCount = mesh.EdgeCount,
        degreesOfFreedom = dof, topologyValid,
        member = target.GetProperty("member").GetString(),
        kineticMetric = "identity",
    },
    frozenDefault = new
    {
        stepSize, leapfrogSteps, trajectoryLength = stepSize * leapfrogSteps,
        warmupPerChain, retainedPerChain, trajectoriesPerChain, chainsPerTable,
        tableCount = tables.Length, divergenceThreshold, minimumAcceptance,
        maximumNonFinite, maximumDivergent, adaptationPerformed = false,
        inheritedFromPhase533 = false,
        selectionMethod = contract.GetProperty("defaultSelectionProvenance").GetProperty("method").GetString(),
        pristineSeedBlindPreregistration = false,
        nonRegisteredCalibrationSeeds = contract.GetProperty("defaultSelectionProvenance")
            .GetProperty("nonRegisteredCalibrationSeeds").EnumerateArray().Select(x => x.GetInt32()).ToArray(),
    },
    seedUse = new
    {
        tables = tables.Select(t => new
        {
            tableId = t.Id, rawSeeds = t.Seeds,
            executionSeeds = t.Seeds.Select(s => s + t.Offset).ToArray(),
            initialScales = t.InitialScales, excludedFrozenSeed = t.ExcludedSeed,
        }).ToArray(),
        exclusionReason = contract.GetProperty("seedExclusionRule").GetProperty("reason").GetString(),
        ceilingModified = false,
        fourChainWouldBeRefused = !fourChainWitness.Allowed,
        fourChainRefusalReason = fourChainWitness.RefusalReason,
        fourChainEstimatedPeakBytes = fourChainWitness.EstimatedPeakBytes,
    },
    resource = new
    {
        maximumAggregateCpuTicks = maximumCpuTicks,
        maximumPeakBytes,
        samplingAssessments,
        controlForceEvaluations,
        controlTicks,
        totalAssessments,
        samplingResourceAllowed,
        totalResourceAllowed,
        resourceAllowed,
        refusedBeforeAllocation = !resourceAllowed,
    },
    deterministicPrechecks = new
    {
        zeroRngUsed = true,
        originAction = originEvaluation.Action,
        originGradientNorm,
        originExact,
        largestEigenvalue,
        smallestEigenvalue,
        conditionNumberEstimate = System.Math.Abs(largestEigenvalue) / System.Math.Max(System.Math.Abs(smallestEigenvalue), 1e-300),
        stabilityBound,
        stepSizeBelowStabilityBound = stepSize < stabilityBound,
        spectralValid,
        directionalGradientError,
        gradientValid,
        reversibilityError,
        reversibilityValid,
        prechecksPassed,
        spectralProbeIsIterativeEstimateNotEigendecomposition = true,
        slowestModeTrajectoryLengthEstimate = System.Math.PI / System.Math.Sqrt(System.Math.Max(System.Math.Abs(smallestEigenvalue), 1e-300)),
        totalTrajectoryLengthPerChain = stepSize * leapfrogSteps * trajectoriesPerChain,
    },
    checkpointRestartEquivalence = new
    {
        chainId = restartChainId,
        uninterruptedTrajectories = restartTotal,
        interruptAfterTrajectories = restartBreak,
        encodedCheckpointSha256 = Convert.ToHexString(SHA256.HashData(encodedCheckpoint)).ToLowerInvariant(),
        checkpointPreservesState,
        restartPositionIdentical,
        restartRngIdentical,
        restartEquivalent,
        reducedPrefixOnly = true,
        isNotFullLengthChainEquivalence = true,
    },
    execution = new
    {
        samplingPerformed,
        executionClean,
        chains = chainRecords.Select(x => new
        {
            chainId = x.ChainId, tableId = x.TableId, seed = x.ExecutionSeed, rawSeed = x.RawSeed,
            warmupCount = x.WarmupCount, retainedCount = x.RetainedCount,
            acceptanceRate = x.AcceptanceRate, nonFiniteCount = x.NonFiniteCount,
            divergenceCount = x.DivergenceCount, maximumAbsoluteDeltaH = x.MaximumAbsoluteDeltaH,
            halted = x.Halted, observableSchemaId,
            observableMeans = observableNames.ToDictionary(n => n, n => x.Observables[n].Length == 0 ? double.NaN : x.Observables[n].Average()),
            observableStandardErrors = observableNames.ToDictionary(n => n, n => StandardError(x.Observables[n], x.PerObservable[n].BulkEss)),
            splitRankNormalizedRhat = observableNames.ToDictionary(n => n, n => x.PerObservable[n].SplitRankNormalizedRhat),
            bulkEss = observableNames.ToDictionary(n => n, n => x.PerObservable[n].BulkEss),
            tailEss = observableNames.ToDictionary(n => n, n => x.PerObservable[n].TailEss),
            checkpointSha256 = x.CheckpointSha256,
            telemetryDeterministicSha256 = x.TelemetryDeterministicSha256,
        }).ToArray(),
        totalForceEvaluations = forceEvaluations,
    },
    diagnostics = new
    {
        definitions = new
        {
            splitRankNormalizedRhat = "maximum of rank-normalized split R-hat and rank-normalized folded split R-hat",
            bulkEss = "effective sample size of rank-normalized draws using the Geyer initial monotone positive sequence",
            tailEss = "effective sample size of the rank-normalized folded absolute deviation from the median",
        },
        thresholds = new { maximumRhat, minimumBulkEss, minimumTailEss, minimumAcceptance },
        tables = tableDiagnostics,
        diagnosticsValid,
        failingObservables,
        everyGaugeInvariantObservablePassed,
        everyFailingObservableIsGaugeVariant,
        gaugeSectorSplitObserved = everyGaugeInvariantObservablePassed && everyFailingObservableIsGaugeVariant,
    },
    telemetry = new
    {
        schemaSatisfied = true,
        trajectoryRequiredFields = trajectoryFields,
        chainSummaryRequiredFields = chainFields,
        observableSchemaId,
        observableNames,
        timingFieldIsVolatile = "proposalElapsedCpuTicks",
        deterministicOutputExcludesTiming = true,
        telemetryFilesAreNotByteReproducible = true,
    },
    verdictKind = verdict,
    terminalStatus = "bounded-complete-lattice-pilot-execution-" + verdict,
    decision = Decision(verdict),
    inferenceScope = new
    {
        workbenchRelativeLatticeUnitsOnly = true,
        establishesStationarityOfRegisteredTarget = false,
        establishesSamplingCorrectness = false,
        establishesTransferToLargerExtent = false,
        establishesSpectralOrPhysicalQuantity = false,
        isProductionBenchmark = false,
    },
    rngUsed = samplingPerformed,
    markovChainAdvanced = samplingPerformed,
    warmupPerformed = samplingPerformed,
    samplingPerformed,
    adaptationPerformed = false,
    benchmarkPerformed = false,
    configurationsRetained = false,
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
};

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var writeOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] outputBytes = JsonSerializer.SerializeToUtf8Bytes(output, writeOptions);
File.WriteAllBytes(OutputPath, outputBytes);
File.WriteAllBytes(SummaryPath, outputBytes);
Console.WriteLine($"Phase548 verdict: {verdict}");
Console.WriteLine($"bindings={bindings.Count(x => x.HashMatches)}/{bindings.Length}, prechecks={prechecksPassed}, restart={restartEquivalent}, clean={executionClean}, diagnosticsValid={diagnosticsValid}");
Console.WriteLine($"lambdaMax={largestEigenvalue:R}, lambdaMin={smallestEigenvalue:R}, stabilityBound={stabilityBound:R}");
Console.WriteLine($"forceEvaluations={forceEvaluations}, chains={chainRecords.Count}");

string Decision(string kind) => kind switch
{
    "pilot-executed-diagnostics-valid" =>
        "The bounded complete-lattice pilot ran within every frozen gate. This establishes only that this bounded configuration executed cleanly on this lattice in lattice units; it establishes no stationarity, sampling correctness, transfer, spectral quantity, or physical claim.",
    "pilot-executed-diagnostics-invalid" =>
        "The bounded complete-lattice pilot executed with no non-finite or divergent trajectory, but at least one frozen convergence gate failed. The negative result is the preserved terminal; no favorable rerun replaces it and no downstream authority follows.",
    "pilot-execution-nonfinite-or-divergent" =>
        "The bounded complete-lattice pilot produced non-finite or divergent trajectories against a frozen zero tolerance and halted fail-closed. The failure artifacts are preserved.",
    "checkpoint-restart-inequivalence" =>
        "Live checkpoint restart did not reproduce the uninterrupted chain bit for bit, so no sampling was performed.",
    "deterministic-precheck-failed" =>
        "A frozen deterministic zero-sampling precheck failed, so no sampling was performed.",
    "resource-refusal" =>
        "The frozen Phase546 resource rule refused the request before allocation, so no sampling was performed.",
    "upstream-registration-authorization-absent" =>
        "The Phase547 registration premise is not satisfied, so no sampling was performed.",
    _ => "An exact-binding, forwarding, or seed-provenance input is invalid or drifted; no sampling was performed.",
};

ChainState NewChain(int executionSeed, double initialScale)
{
    var rng = new Xoshiro(ExpandSeed((ulong)executionSeed));
    var position = new double[dof];
    for (int i = 0; i < dof; i++) position[i] = initialScale * Gauss(rng);
    return new ChainState(position, rng, 0, null, 0.0);
}

CheckpointState BuildCheckpoint(string chainId, int seed, ChainState state, int warmupCompleted, int retainedCompleted) => new()
{
    FormatId = checkpointRule.GetProperty("formatId").GetString()!,
    ActionFingerprint = checkpointRule.GetProperty("actionFingerprint").GetString()!,
    PackFingerprint = checkpointRule.GetProperty("packFingerprint").GetString()!,
    TopologyId = topologyId,
    Extent = extent,
    Dimensions = dimensions,
    DegreesOfFreedom = dof,
    Beta = beta,
    ThetaRule = thetaRule,
    ChainId = chainId,
    Seed = seed,
    TrajectoryIndex = state.Index,
    WarmupCompleted = warmupCompleted,
    RetainedCompleted = retainedCompleted,
    StepSize = stepSize,
    LeapfrogSteps = leapfrogSteps,
    RngAlgorithm = "xoshiro256-starstar",
    RngStateWords = state.Rng.State.Words,
    Position = state.Position,
};

(double Action, double[] Gradient) CurrentValues(double[] position)
{
    if (BitIdentical(lastPosition, position)) return (lastAction, lastGradient);
    PilotEvaluation evaluation = Evaluate(position);
    return (evaluation.Action, evaluation.Gradient);
}

static bool BitIdentical(double[] a, double[] b) =>
    a.Length == b.Length && a.Zip(b).All(pair =>
        BitConverter.DoubleToInt64Bits(pair.First) == BitConverter.DoubleToInt64Bits(pair.Second));

static double StandardError(double[] draws, double ess)
{
    if (draws.Length < 2 || !double.IsFinite(ess) || ess <= 0) return double.NaN;
    double mean = draws.Average();
    double variance = draws.Sum(x => (x - mean) * (x - mean)) / (draws.Length - 1);
    return System.Math.Sqrt(variance / ess);
}

static Diagnostics Diagnose(double[][] chains)
{
    double[][] usable = chains.Where(x => x.Length >= 8).ToArray();
    if (usable.Length == 0) return new Diagnostics(double.NaN, double.NaN, double.NaN);
    int n = usable.Min(x => x.Length);
    double[][] trimmed = usable.Select(x => x.Take(n).ToArray()).ToArray();
    double[] pooled = trimmed.SelectMany(x => x).ToArray();
    if (pooled.Distinct().Count() <= 1) return new Diagnostics(double.NaN, double.NaN, double.NaN);
    double[] ranked = RankNormalize(pooled);
    double median = Median(pooled);
    double[] folded = RankNormalize(pooled.Select(x => System.Math.Abs(x - median)).ToArray());
    double[][] rankedChains = Regroup(ranked, trimmed.Length, n);
    double[][] foldedChains = Regroup(folded, trimmed.Length, n);
    double rhat = System.Math.Max(SplitRhat(rankedChains), SplitRhat(foldedChains));
    return new Diagnostics(rhat, Ess(SplitChains(rankedChains)), Ess(SplitChains(foldedChains)));
}

static double[][] Regroup(double[] flat, int chains, int n) =>
    Enumerable.Range(0, chains).Select(c => flat.Skip(c * n).Take(n).ToArray()).ToArray();

static double[][] SplitChains(double[][] chains) =>
    chains.SelectMany(x => new[] { x.Take(x.Length / 2).ToArray(), x.Skip(x.Length - x.Length / 2).ToArray() }).ToArray();

static double SplitRhat(double[][] chains)
{
    double[][] split = SplitChains(chains);
    int m = split.Length;
    int n = split.Min(x => x.Length);
    if (m < 2 || n < 2) return double.NaN;
    double[] means = split.Select(x => x.Take(n).Average()).ToArray();
    double[] variances = split.Select(x =>
    {
        double mean = x.Take(n).Average();
        return x.Take(n).Sum(v => (v - mean) * (v - mean)) / (n - 1);
    }).ToArray();
    double within = variances.Average();
    if (within <= 0) return double.NaN;
    double grand = means.Average();
    double between = n * means.Sum(x => (x - grand) * (x - grand)) / (m - 1);
    double varPlus = ((n - 1.0) / n) * within + between / n;
    return System.Math.Sqrt(varPlus / within);
}

static double Ess(double[][] chains)
{
    int m = chains.Length;
    int n = chains.Min(x => x.Length);
    if (m < 2 || n < 4) return double.NaN;
    double[][] trimmed = chains.Select(x => x.Take(n).ToArray()).ToArray();
    double[] means = trimmed.Select(x => x.Average()).ToArray();
    double[] variances = trimmed.Select(x =>
    {
        double mean = x.Average();
        return x.Sum(v => (v - mean) * (v - mean)) / (n - 1);
    }).ToArray();
    double within = variances.Average();
    if (within <= 0) return double.NaN;
    double grand = means.Average();
    double between = n * means.Sum(x => (x - grand) * (x - grand)) / (m - 1);
    double varPlus = ((n - 1.0) / n) * within + between / n;
    if (varPlus <= 0) return double.NaN;
    var rho = new double[n];
    for (int lag = 1; lag < n; lag++)
    {
        double covariance = 0.0;
        for (int c = 0; c < m; c++)
        {
            double mean = means[c];
            double sum = 0.0;
            for (int i = 0; i + lag < n; i++) sum += (trimmed[c][i] - mean) * (trimmed[c][i + lag] - mean);
            covariance += sum / n;
        }
        rho[lag] = 1.0 - (within - covariance / m) / varPlus;
    }
    double tau = -1.0;
    double previousPair = double.PositiveInfinity;
    for (int k = 0; 2 * k + 2 < n; k++)
    {
        double pair = rho[2 * k + 1] + rho[2 * k + 2];
        if (pair < 0) break;
        pair = System.Math.Min(pair, previousPair);
        previousPair = pair;
        tau += 2.0 * pair;
    }
    return tau > 0 ? m * n / tau : double.NaN;
}

static double[] RankNormalize(double[] values)
{
    int n = values.Length;
    int[] order = Enumerable.Range(0, n).OrderBy(i => values[i]).ToArray();
    var ranks = new double[n];
    int index = 0;
    while (index < n)
    {
        int run = index;
        while (run + 1 < n && values[order[run + 1]] == values[order[index]]) run++;
        double average = (index + run) / 2.0 + 1.0;
        for (int k = index; k <= run; k++) ranks[order[k]] = average;
        index = run + 1;
    }
    return ranks.Select(r => InverseNormalCdf((r - 0.375) / (n + 0.25))).ToArray();
}

static double Median(double[] values)
{
    double[] sorted = values.OrderBy(x => x).ToArray();
    int n = sorted.Length;
    return n % 2 == 1 ? sorted[n / 2] : 0.5 * (sorted[n / 2 - 1] + sorted[n / 2]);
}

static double InverseNormalCdf(double p)
{
    double[] a = [-3.969683028665376e+01, 2.209460984245205e+02, -2.759285104469687e+02, 1.383577518672690e+02, -3.066479806614716e+01, 2.506628277459239e+00];
    double[] b = [-5.447609879822406e+01, 1.615858368580409e+02, -1.556989798598866e+02, 6.680131188771972e+01, -1.328068155288572e+01];
    double[] c = [-7.784894002430293e-03, -3.223964580411365e-01, -2.400758277161838e+00, -2.549732539343734e+00, 4.374664141464968e+00, 2.938163982698783e+00];
    double[] d = [7.784695709041462e-03, 3.224671290700398e-01, 2.445134137142996e+00, 3.754408661907416e+00];
    const double low = 0.02425;
    const double high = 1.0 - low;
    if (p <= 0 || p >= 1) return double.NaN;
    if (p < low)
    {
        double q = System.Math.Sqrt(-2 * System.Math.Log(p));
        return (((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) / ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
    }
    if (p > high)
    {
        double q = System.Math.Sqrt(-2 * System.Math.Log(1 - p));
        return -(((((c[0] * q + c[1]) * q + c[2]) * q + c[3]) * q + c[4]) * q + c[5]) / ((((d[0] * q + d[1]) * q + d[2]) * q + d[3]) * q + 1);
    }
    double r = p - 0.5;
    double s = r * r;
    return (((((a[0] * s + a[1]) * s + a[2]) * s + a[3]) * s + a[4]) * s + a[5]) * r
        / (((((b[0] * s + b[1]) * s + b[2]) * s + b[3]) * s + b[4]) * s + 1);
}

static string TimingStrippedHash(List<TrajectoryRow> rows)
{
    var builder = new StringBuilder();
    foreach (TrajectoryRow row in rows)
        builder.Append(row.ChainId).Append('|').Append(row.TrajectoryIndex).Append('|').Append(row.Phase).Append('|')
            .Append(row.Accepted).Append('|').Append(row.InitialHamiltonian.ToString("R")).Append('|')
            .Append(row.ProposedHamiltonian.ToString("R")).Append('|').Append(row.DeltaH.ToString("R")).Append('|')
            .Append(row.AcceptanceProbability.ToString("R")).Append('|').Append(row.InjectedThreshold.ToString("R")).Append('|')
            .Append(row.NonFinite).Append('|').Append(row.Divergent).Append('|').Append(row.DivergenceThreshold.ToString("R")).Append('|')
            .Append(row.ForceEvaluationCount).Append('|').Append(row.MaximumForceNorm.ToString("R")).Append('\n');
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString()))).ToLowerInvariant();
}

static double[] GoldenAngleUnit(int length, bool cosine)
{
    var result = new double[length];
    for (int i = 0; i < length; i++)
    {
        double angle = (i + 1) * 0.6180339887498948;
        result[i] = cosine ? System.Math.Cos(angle) : System.Math.Sin(angle);
    }
    return Normalize(result);
}
static double[] Normalize(double[] values)
{
    double norm = System.Math.Sqrt(Dot(values, values));
    return values.Select(x => x / norm).ToArray();
}
static double[] Add(double[] values, double[] direction, double scale)
{
    var result = new double[values.Length];
    for (int i = 0; i < values.Length; i++) result[i] = values[i] + scale * direction[i];
    return result;
}
static double Dot(double[] a, double[] b)
{
    double sum = 0.0;
    for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
    return sum;
}
static double ScaledError(double a, double b) =>
    System.Math.Abs(a - b) / System.Math.Max(1.0, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
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
static double Uniform(Xoshiro rng) => ((rng.Next() >> 11) + 0.5) / 9007199254740992.0;
static double Gauss(Xoshiro rng)
{
    double u1 = Uniform(rng), u2 = Uniform(rng);
    return System.Math.Sqrt(-2.0 * System.Math.Log(u1)) * System.Math.Cos(2.0 * System.Math.PI * u2);
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
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

sealed record ResourceAssessment(bool Allowed, string? RefusalReason, long EstimatedCpuTicks, long EstimatedPeakBytes);
sealed record Diagnostics(double SplitRankNormalizedRhat, double BulkEss, double TailEss);
sealed record ChainState(double[] Position, Xoshiro Rng, long Index, PilotDecision? LastDecision, double LastMaximumForceNorm);
sealed record ChainRecord(string TableId, string ChainId, int ExecutionSeed, int RawSeed, int WarmupCount,
    int RetainedCount, double AcceptanceRate, int NonFiniteCount, int DivergenceCount, double MaximumAbsoluteDeltaH,
    bool Halted, Dictionary<string, double[]> Observables, Dictionary<string, Diagnostics> PerObservable,
    string CheckpointSha256, string TelemetryDeterministicSha256);
sealed record TrajectoryRow(string ChainId, int TrajectoryIndex, string Phase, bool Accepted,
    double InitialHamiltonian, double ProposedHamiltonian, double DeltaH, double AcceptanceProbability,
    double InjectedThreshold, bool NonFinite, bool Divergent, double DivergenceThreshold,
    int ForceEvaluationCount, double MaximumForceNorm, long ProposalElapsedCpuTicks);
sealed record RngState(ulong S0, ulong S1, ulong S2, ulong S3)
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

sealed class CheckpointState
{
    public string FormatId { get; init; } = "";
    public string ActionFingerprint { get; init; } = "";
    public string PackFingerprint { get; init; } = "";
    public string TopologyId { get; init; } = "";
    public int Extent { get; init; }
    public int Dimensions { get; init; }
    public int DegreesOfFreedom { get; init; }
    public double Beta { get; init; }
    public string ThetaRule { get; init; } = "";
    public string ChainId { get; init; } = "";
    public int Seed { get; init; }
    public long TrajectoryIndex { get; init; }
    public int WarmupCompleted { get; init; }
    public int RetainedCompleted { get; init; }
    public double StepSize { get; init; }
    public int LeapfrogSteps { get; init; }
    public string RngAlgorithm { get; init; } = "";
    public ulong[] RngStateWords { get; init; } = [];
    public double[] Position { get; init; } = [];
}

static class CheckpointCodec
{
    private static readonly JsonSerializerOptions CamelCase = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public static byte[] Encode(CheckpointState state, JsonElement rule)
    {
        byte[] payload = CanonicalPayload(state, rule);
        string checksum = Convert.ToHexString(SHA256.HashData(payload)).ToLowerInvariant();
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("checksumAlgorithm", "SHA-256");
            writer.WriteString("payloadSha256", checksum);
            writer.WritePropertyName("payload");
            writer.WriteRawValue(payload, skipInputValidation: false);
            writer.WriteEndObject();
        }
        return stream.ToArray();
    }

    public static CheckpointState Decode(byte[] encoded, JsonElement rule)
    {
        using var document = JsonDocument.Parse(encoded);
        JsonElement root = document.RootElement;
        if (root.GetProperty("checksumAlgorithm").GetString() != "SHA-256")
            throw new InvalidDataException("Unsupported checkpoint checksum algorithm.");
        JsonElement payloadElement = root.GetProperty("payload");
        byte[] observed = Encoding.UTF8.GetBytes(payloadElement.GetRawText());
        string expected = root.GetProperty("payloadSha256").GetString() ?? "";
        string actual = Convert.ToHexString(SHA256.HashData(observed)).ToLowerInvariant();
        if (!CryptographicOperations.FixedTimeEquals(Encoding.ASCII.GetBytes(expected), Encoding.ASCII.GetBytes(actual)))
            throw new InvalidDataException("Checkpoint checksum mismatch.");
        CheckpointState state = payloadElement.Deserialize<CheckpointState>(CamelCase)
            ?? throw new InvalidDataException("Checkpoint payload is absent.");
        if (!observed.AsSpan().SequenceEqual(CanonicalPayload(state, rule)))
            throw new InvalidDataException("Checkpoint payload is not canonical.");
        return state;
    }

    public static byte[] CanonicalPayload(CheckpointState state, JsonElement rule)
    {
        Validate(state, rule);
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("formatId", state.FormatId);
            writer.WriteString("actionFingerprint", state.ActionFingerprint);
            writer.WriteString("packFingerprint", state.PackFingerprint);
            writer.WriteString("topologyId", state.TopologyId);
            writer.WriteNumber("extent", state.Extent);
            writer.WriteNumber("dimensions", state.Dimensions);
            writer.WriteNumber("degreesOfFreedom", state.DegreesOfFreedom);
            writer.WriteNumber("beta", state.Beta);
            writer.WriteString("thetaRule", state.ThetaRule);
            writer.WriteString("chainId", state.ChainId);
            writer.WriteNumber("seed", state.Seed);
            writer.WriteNumber("trajectoryIndex", state.TrajectoryIndex);
            writer.WriteNumber("warmupCompleted", state.WarmupCompleted);
            writer.WriteNumber("retainedCompleted", state.RetainedCompleted);
            writer.WriteNumber("stepSize", state.StepSize);
            writer.WriteNumber("leapfrogSteps", state.LeapfrogSteps);
            writer.WriteString("rngAlgorithm", state.RngAlgorithm);
            writer.WritePropertyName("rngStateWords");
            writer.WriteStartArray();
            foreach (ulong word in state.RngStateWords) writer.WriteNumberValue(word);
            writer.WriteEndArray();
            writer.WritePropertyName("position");
            writer.WriteStartArray();
            foreach (double value in state.Position) writer.WriteNumberValue(value);
            writer.WriteEndArray();
            writer.WriteEndObject();
        }
        return stream.ToArray();
    }

    private static void Validate(CheckpointState state, JsonElement rule)
    {
        if (state.FormatId != rule.GetProperty("formatId").GetString()
            || state.ActionFingerprint != rule.GetProperty("actionFingerprint").GetString()
            || state.PackFingerprint != rule.GetProperty("packFingerprint").GetString()
            || state.TopologyId != "periodic-hypercubic-4d"
            || state.Extent <= 0 || state.Dimensions != 4
            || state.DegreesOfFreedom != checked(45 * state.Extent * state.Extent * state.Extent * state.Extent)
            || state.Position.Length != state.DegreesOfFreedom || state.RngStateWords.Length != 4
            || state.ChainId.Length == 0 || state.Seed <= 0 || state.TrajectoryIndex < 0
            || state.WarmupCompleted < 0 || state.RetainedCompleted < 0
            || !double.IsFinite(state.Beta) || !double.IsFinite(state.StepSize) || state.StepSize <= 0
            || state.ThetaRule != "theta-identically-zero"
            || state.RngAlgorithm != "xoshiro256-starstar"
            || state.RngStateWords.All(x => x == 0)
            || state.LeapfrogSteps <= 0 || state.Position.Any(x => !double.IsFinite(x)))
            throw new InvalidDataException("Checkpoint restart state is invalid.");
    }
}
