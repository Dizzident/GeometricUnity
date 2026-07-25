using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase542_metric_normalized_force_closure_census_001";
const string ContractPath = Root + "/preregistration/phase542_metric_normalized_force_closure_contract_v1.json";
const string OutputPath = Root + "/output/metric_normalized_force_closure_census.json";
const string SummaryPath = Root + "/output/metric_normalized_force_closure_census_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new
{
    Id = x.GetProperty("id").GetString()!,
    Path = x.GetProperty("path").GetString()!,
    ExpectedSha256 = x.GetProperty("sha256").GetString()!,
}).ToArray();
var bindings = bindingSpecs.Select(x => new
{
    x.Id, x.Path, x.ExpectedSha256,
    ActualSha256 = File.Exists(x.Path) ? Sha(x.Path) : "missing",
    HashMatches = File.Exists(x.Path) && Sha(x.Path) == x.ExpectedSha256,
}).ToArray();
string[] expectedIds = ["phase534-contract", "phase541-contract", "phase541-program", "phase541-summary", "complete-lattice-gradient-source"];
bool exactBindingsValid = bindingSpecs.Select(x => x.Id).SequenceEqual(expectedIds) && bindings.All(x => x.HashMatches);

using var p534Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[0].Path));
using var p541Doc = JsonDocument.Parse(File.ReadAllBytes(bindingSpecs[3].Path));
JsonElement p534 = p534Doc.RootElement;
JsonElement p541 = p541Doc.RootElement;
JsonElement menu = contract.GetProperty("closureMenu");
JsonElement target = contract.GetProperty("registeredTarget");
JsonElement resource = contract.GetProperty("resourceRefusal");
string[] taxonomy =
[
    "invalid-or-drifted-input", "resource-refusal", "metric-normalized-coordinate-replay-failed",
    "force-closure-computation-invalid", "compact-force-closed-subspace-found",
    "force-closure-expands-beyond-compact-limit",
];
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase542-a27-metric-normalized-force-closure-census-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A27"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && exactBindingsValid
    && contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()).SequenceEqual(taxonomy)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
bool precursorSemanticsValid =
    p541.GetProperty("verdictKind").GetString() == "embedding-kinetic-metric-mismatch"
    && p541.GetProperty("analyticForceParity").GetProperty("passed").GetBoolean()
    && !p541.GetProperty("transverseForceAudit").GetProperty("passed").GetBoolean()
    && !p541.GetProperty("hmcOrSamplingPerformed").GetBoolean()
    && p541.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble() <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64() <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64();

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
JsonElement reduction = p534.GetProperty("registeredReduction");
int[] witnessEdges = Ints(reduction, "witnessEdges");
int[] witnessComponents = Ints(reduction, "witnessComponents");
int[] witnessValues = Ints(reduction, "witnessValues");
var rawRay = new double[mesh.EdgeCount * algebra.Dimension];
for (int i = 0; i < witnessEdges.Length; i++)
    rawRay[witnessEdges[i] * algebra.Dimension + witnessComponents[i]] = witnessValues[i];
double rawNormSquared = Dot(rawRay, rawRay);
double rawNorm = System.Math.Sqrt(rawNormSquared);
double[] normalizedRay = rawRay.Select(x => x / rawNorm).ToArray();

(double S, double[] G) Evaluate(double[] omega)
{
    var g = op.ComputeJointGradient(omega, thetaZero, mass);
    return (g.Objective, g.GradOmega);
}

double[] amplitudes = Doubles(menu, "amplitudesInNormalizedCoordinate");
JsonElement[] p541StateRows = p541.GetProperty("actionReconstruction").GetProperty("stateRows").EnumerateArray().ToArray();
double coordinateTolerance = menu.GetProperty("coordinateReplayTolerance").GetDouble();
double maxActionReplayError = 0.0;
double maxForceTransformError = 0.0;
for (int i = 0; i < amplitudes.Length; i++)
{
    double q = amplitudes[i];
    double[] omega = Scale(normalizedRay, q);
    (double action, double[] gradient) = Evaluate(omega);
    double expectedAction = p541StateRows[i].GetProperty("action").GetDouble();
    double expectedDsdq = p541StateRows[i].GetProperty("scalarFromFull").GetDouble() / rawNorm;
    maxActionReplayError = System.Math.Max(maxActionReplayError, ScaledError(action, expectedAction));
    maxForceTransformError = System.Math.Max(maxForceTransformError, ScaledError(Dot(gradient, normalizedRay), expectedDsdq));
}
bool coordinateReplayPassed = System.Math.Abs(Dot(normalizedRay, normalizedRay) - 1.0) <= 1e-12
    && System.Math.Abs(rawNormSquared - menu.GetProperty("rawEmbeddingNormSquared").GetDouble()) <= 1e-12
    && maxActionReplayError <= coordinateTolerance && maxForceTransformError <= coordinateTolerance;

int maximumDimension = menu.GetProperty("maximumClosureDimension").GetInt32();
int maximumRounds = menu.GetProperty("maximumExpansionRounds").GetInt32();
int maximumCompactDimension = menu.GetProperty("maximumCompactDimension").GetInt32();
double rankTolerance = menu.GetProperty("orthogonalRankRelativeTolerance").GetDouble();
double closureTolerance = menu.GetProperty("closureResidualRelativeTolerance").GetDouble();
double jacobianStep = menu.GetProperty("jacobianRelativeStep").GetDouble();
var basis = new List<double[]> { normalizedRay };
var roundRows = new List<object>();
bool computationFinite = true;
bool stabilized = false;
for (int round = 0; round < maximumRounds && basis.Count < maximumDimension; round++)
{
    int startDimension = basis.Count;
    var active = basis.ToArray();
    foreach (double amplitude in amplitudes)
    {
        double[] state = Scale(normalizedRay, amplitude);
        (double action, double[] force) = Evaluate(state);
        computationFinite &= double.IsFinite(action) && force.All(double.IsFinite);
        TryAdd(force, basis, rankTolerance, maximumDimension);
        foreach (double[] direction in active)
        {
            double h = jacobianStep * System.Math.Max(1.0, Norm(state));
            double[] plus = AddScaled(state, direction, h);
            double[] minus = AddScaled(state, direction, -h);
            double[] gp = Evaluate(plus).G;
            double[] gm = Evaluate(minus).G;
            var jv = new double[force.Length];
            for (int i = 0; i < jv.Length; i++) jv[i] = (gp[i] - gm[i]) / (2.0 * h);
            computationFinite &= jv.All(double.IsFinite);
            TryAdd(jv, basis, rankTolerance, maximumDimension);
            if (basis.Count >= maximumDimension) break;
        }
        if (basis.Count >= maximumDimension) break;
    }
    int added = basis.Count - startDimension;
    roundRows.Add(new { round, startDimension, added, endDimension = basis.Count });
    if (added == 0) { stabilized = true; break; }
}

double maxForceResidual = 0.0;
double maxJacobianResidual = 0.0;
foreach (double amplitude in amplitudes)
{
    double[] state = Scale(normalizedRay, amplitude);
    double[] force = Evaluate(state).G;
    maxForceResidual = System.Math.Max(maxForceResidual, RelativeResidual(force, basis));
    foreach (double[] direction in basis)
    {
        double h = jacobianStep * System.Math.Max(1.0, Norm(state));
        double[] gp = Evaluate(AddScaled(state, direction, h)).G;
        double[] gm = Evaluate(AddScaled(state, direction, -h)).G;
        var jv = new double[force.Length];
        for (int i = 0; i < jv.Length; i++) jv[i] = (gp[i] - gm[i]) / (2.0 * h);
        maxJacobianResidual = System.Math.Max(maxJacobianResidual, RelativeResidual(jv, basis));
    }
}
computationFinite &= double.IsFinite(maxForceResidual) && double.IsFinite(maxJacobianResidual);
bool residualClosed = maxForceResidual <= closureTolerance && maxJacobianResidual <= closureTolerance;
bool compactClosureFound = stabilized && residualClosed && basis.Count <= maximumCompactDimension;
string verdict = !contractValid || !precursorSemanticsValid ? taxonomy[0]
    : !resourceAccepted ? taxonomy[1]
    : !coordinateReplayPassed ? taxonomy[2]
    : !computationFinite ? taxonomy[3]
    : compactClosureFound ? taxonomy[4] : taxonomy[5];

var result = new
{
    schemaVersion = 1,
    phase = 542,
    phaseId = "phase542-metric-normalized-force-closure-census",
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
    completeLattice = new { extent, vertexCount = mesh.VertexCount, edgeCount = mesh.EdgeCount, omegaDegreesOfFreedom = rawRay.Length },
    metricNormalization = new { rawNormSquared, normalizedNormSquared = Dot(normalizedRay, normalizedRay), coordinateRule = "q=sqrt(3)*x; omega=q*r/sqrt(3)", maxActionReplayError, maxForceTransformError, passed = coordinateReplayPassed },
    forceClosure = new { basisDimension = basis.Count, maximumDimension, maximumCompactDimension, maximumRounds, stabilized, residualClosed, maxForceResidual, maxJacobianResidual, closureTolerance, compactClosureFound, roundRows },
    basisDigest = ShaBytes(JsonSerializer.SerializeToUtf8Bytes(basis)),
    verdictKind = verdict,
    terminalStatus = "metric-normalized-force-closure-census-" + verdict,
    decision = compactClosureFound
        ? "A compact metric-normalized force/Jacobian closure survived the frozen census; only an independent deterministic shadowing experiment may consume it."
        : "Metric normalization is exact, but the frozen force/Jacobian closure did not stabilize within the compact dimension and round limits; scalar or compact-surrogate transfer is not supported.",
    laterDeterministicExperimentAuthorized = contractValid && precursorSemanticsValid && resourceAccepted && coordinateReplayPassed && computationFinite,
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
Console.WriteLine($"Phase542 verdict: {verdict}");
Console.WriteLine($"basisDimension={basis.Count}, stabilized={stabilized}, forceResidual={maxForceResidual:R}, jacobianResidual={maxJacobianResidual:R}");
Console.WriteLine("rng=False, sampling=False");

static bool TryAdd(double[] candidate, List<double[]> basis, double relativeTolerance, int maximumDimension)
{
    if (basis.Count >= maximumDimension) return false;
    double originalNorm = Norm(candidate);
    if (!double.IsFinite(originalNorm) || originalNorm == 0.0) return false;
    double[] residual = (double[])candidate.Clone();
    for (int pass = 0; pass < 2; pass++)
        foreach (double[] vector in basis)
        {
            double projection = Dot(residual, vector);
            for (int i = 0; i < residual.Length; i++) residual[i] -= projection * vector[i];
        }
    double residualNorm = Norm(residual);
    if (residualNorm <= relativeTolerance * System.Math.Max(1.0, originalNorm)) return false;
    for (int i = 0; i < residual.Length; i++) residual[i] /= residualNorm;
    int pivot = 0;
    for (int i = 1; i < residual.Length; i++)
        if (System.Math.Abs(residual[i]) > System.Math.Abs(residual[pivot])) pivot = i;
    if (residual[pivot] < 0.0)
        for (int i = 0; i < residual.Length; i++) residual[i] = -residual[i];
    basis.Add(residual);
    return true;
}

static double RelativeResidual(double[] candidate, List<double[]> basis)
{
    double[] residual = (double[])candidate.Clone();
    foreach (double[] vector in basis)
    {
        double projection = Dot(residual, vector);
        for (int i = 0; i < residual.Length; i++) residual[i] -= projection * vector[i];
    }
    return Norm(residual) / System.Math.Max(1.0, Norm(candidate));
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
static string Sha(string path) => ShaBytes(File.ReadAllBytes(path));
static string ShaBytes(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
