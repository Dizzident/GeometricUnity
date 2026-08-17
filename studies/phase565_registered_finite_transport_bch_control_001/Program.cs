using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase565_registered_finite_transport_bch_control_001";
const string ContractPath = Root + "/preregistration/phase565_registered_finite_transport_bch_control_contract_v1.json";
const string OutputPath = Root + "/output/registered_finite_transport_bch_control.json";
const string SummaryPath = Root + "/output/registered_finite_transport_bch_control_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
double[] ladder = contract.GetProperty("amplitudeLadder").EnumerateArray().Select(x => x.GetDouble()).ToArray();
double exactTolerance = contract.GetProperty("exactGroupTolerance").GetDouble();
double slopeTolerance = contract.GetProperty("slopeTolerance").GetDouble();
double coefficientTolerance = contract.GetProperty("coefficientRelativeTolerance").GetDouble();
JsonElement resource = contract.GetProperty("resourceRefusal");
bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble() <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64() <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64()
    && resource.GetProperty("refuseBeforeAllocation").GetBoolean();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase565-a34-registered-finite-transport-bch-control-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean() && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && specs.Length == 8 && bindings.All(x => x.hashMatches) && ladder.Length == 9
    && taxonomy.Length == 7
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// Quaternion known-answer battery executes before Phase564 is read.
Q plantedX = Exp(new[] { 0.2, -0.1, 0.05 });
double plantedExpLogError = Norm(Subtract(Log(plantedX), new[] { 0.2, -0.1, 0.05 }));
Q plantedG = Exp(new[] { -0.13, 0.07, 0.11 });
double plantedConjugationTraceError = System.Math.Abs(Conjugate(plantedG, plantedX).W - plantedX.W);
double plantedInverseError = QDistance(plantedX * plantedX.Inverse(), Q.Identity);
bool traceTooEarlyDecoyRejected = QDistance(Conjugate(plantedG, plantedX), plantedX) > 1e-5
    && plantedConjugationTraceError < 1e-15;
bool knownAnswerBatteryPassed = plantedExpLogError < 1e-15 && plantedConjugationTraceError < 1e-15
    && plantedInverseError < 1e-15 && traceTooEarlyDecoyRejected;

if (!contractValid || !resourceAccepted || !knownAnswerBatteryPassed)
{
    string verdict = !contractValid || !resourceAccepted ? taxonomy[0] : taxonomy[2];
    Emit(new
    {
        schemaVersion = 1, phase = 565, phaseId = "phase565-registered-finite-transport-bch-control",
        contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid,
        exactBindingsValid = bindings.All(x => x.hashMatches), resourceAccepted, bindings,
        knownAnswerBattery = new { ranBeforePhase564Read = true, plantedExpLogError, plantedConjugationTraceError, plantedInverseError, traceTooEarlyDecoyRejected, passed = knownAnswerBatteryPassed },
        verdictKind = verdict, terminalStatus = "registered-finite-transport-bch-control-" + verdict,
        phase561GateOpen = false, rngUsed = false, samplingPerformed = false, sourceIdentificationAuthoredOrInferred = false,
        externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
    });
    Console.WriteLine($"Phase565 verdict: {verdict}");
    return;
}

JsonElement phase564 = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase564-summary").Path)).RootElement.Clone();
string[] allowed = contract.GetProperty("phase564RequiredGate").GetProperty("allowedVerdicts").EnumerateArray().Select(x => x.GetString()!).ToArray();
bool upstreamGateOpen = phase564.GetProperty("phase565GateOpen").GetBoolean()
    && allowed.Contains(phase564.GetProperty("verdictKind").GetString(), StringComparer.Ordinal);
if (!upstreamGateOpen)
{
    Emit(new
    {
        schemaVersion = 1, phase = 565, phaseId = "phase565-registered-finite-transport-bch-control",
        contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid = true,
        exactBindingsValid = true, resourceAccepted, bindings,
        knownAnswerBattery = new { ranBeforePhase564Read = true, plantedExpLogError, plantedConjugationTraceError, plantedInverseError, traceTooEarlyDecoyRejected, passed = true },
        upstreamGateOpen = false, verdictKind = taxonomy[1], terminalStatus = "registered-finite-transport-bch-control-" + taxonomy[1],
        phase561GateOpen = false, rngUsed = false, samplingPerformed = false, sourceIdentificationAuthoredOrInferred = false,
        externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
    });
    Console.WriteLine($"Phase565 verdict: {taxonomy[1]}");
    return;
}

var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int g = algebra.Dimension, nOmega = mesh.EdgeCount * g, nXi = mesh.VertexCount * g;
double[] omega = Wave(nOmega, 0.031, 0.419);
double[] xi = Wave(nXi, 0.23, 0.587);
var edgeIndex = mesh.Edges.Select((e, i) => (Key: (System.Math.Min(e[0], e[1]), System.Math.Max(e[0], e[1])), Index: i)).ToDictionary(x => x.Key, x => x.Index);

// Exact finite group controls on the registered face tuples.
Q[] links = Enumerable.Range(0, mesh.EdgeCount).Select(e => Exp(Block(omega, e))).ToArray();
Q[] gauges = Enumerable.Range(0, mesh.VertexCount).Select(v => Exp(Block(xi, v))).ToArray();
Q[] transformed = new Q[mesh.EdgeCount];
for (int e = 0; e < mesh.EdgeCount; e++)
    transformed[e] = gauges[mesh.Edges[e][0]] * links[e] * gauges[mesh.Edges[e][1]].Inverse();
double maxHolonomyCovariance = 0.0, maxReverseIdentity = 0.0, maxBasepointTransport = 0.0, maxClassFunctionError = 0.0;
for (int f = 0; f < mesh.FaceCount; f++)
{
    int v0 = mesh.Faces[f][0], v1 = mesh.Faces[f][1], v2 = mesh.Faces[f][2];
    Q h0 = Link(links, v0, v1) * Link(links, v1, v2) * Link(links, v2, v0);
    Q hp = Link(transformed, v0, v1) * Link(transformed, v1, v2) * Link(transformed, v2, v0);
    Q expected = Conjugate(gauges[v0], h0);
    maxHolonomyCovariance = System.Math.Max(maxHolonomyCovariance, QDistance(hp, expected));
    Q reverse = Link(links, v0, v2) * Link(links, v2, v1) * Link(links, v1, v0);
    maxReverseIdentity = System.Math.Max(maxReverseIdentity, QDistance(reverse, h0.Inverse()));
    Q h1 = Link(links, v1, v2) * Link(links, v2, v0) * Link(links, v0, v1);
    Q transported = Link(links, v0, v1).Inverse() * h0 * Link(links, v0, v1);
    maxBasepointTransport = System.Math.Max(maxBasepointTransport, QDistance(h1, transported));
    maxClassFunctionError = System.Math.Max(maxClassFunctionError, System.Math.Abs(hp.W - h0.W));
}
bool exactGroupControlsPassed = maxHolonomyCovariance <= exactTolerance && maxReverseIdentity <= exactTolerance
    && maxBasepointTransport <= exactTolerance && maxClassFunctionError <= exactTolerance;

var weakRows = new List<object>();
var continuousResiduals = new List<double>();
var boundaryArrayResiduals = new List<double>();
foreach (double t in ladder)
{
    double[] scaled = Scale(omega, t);
    double[] registered = CurvatureAssembler.Assemble(new ConnectionField(mesh, algebra, scaled)).Coefficients;
    var continuous = new double[registered.Length];
    var boundaryArray = new double[registered.Length];
    Q[] scaledLinks = Enumerable.Range(0, mesh.EdgeCount).Select(e => Exp(Block(scaled, e))).ToArray();
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        int v0 = mesh.Faces[f][0], v1 = mesh.Faces[f][1], v2 = mesh.Faces[f][2];
        double[] logContinuous = Log(Link(scaledLinks, v0, v1) * Link(scaledLinks, v1, v2) * Link(scaledLinks, v2, v0));
        Q product = Q.Identity;
        for (int k = 0; k < mesh.FaceBoundaryEdges[f].Length; k++)
        {
            int e = mesh.FaceBoundaryEdges[f][k];
            Q oriented = mesh.FaceBoundaryOrientations[f][k] > 0 ? scaledLinks[e] : scaledLinks[e].Inverse();
            product *= oriented;
        }
        double[] logArray = Log(product);
        for (int a = 0; a < g; a++)
        {
            continuous[f * g + a] = logContinuous[a];
            boundaryArray[f * g + a] = logArray[a];
        }
    }
    double continuousResidual = Norm(Subtract(continuous, registered));
    double arrayResidual = Norm(Subtract(boundaryArray, registered));
    continuousResiduals.Add(continuousResidual); boundaryArrayResiduals.Add(arrayResidual);
    weakRows.Add(new { t, continuousLoopResidual = continuousResidual, registeredBoundaryArrayProductResidual = arrayResidual, registeredCurvatureNorm = Norm(registered) });
}
double continuousSlope = LogSlope(ladder.Take(6).ToArray(), continuousResiduals.Take(6).ToArray());
double boundaryArraySlope = LogSlope(ladder.Take(6).ToArray(), boundaryArrayResiduals.Take(6).ToArray());

// The registered boundary array is [e01,+], [e02,-], [e12,+], whereas a
// composable loop is [e01,+], [e12,+], [e02,-]. Their second BCH coefficients
// differ by [x2,x1], evaluated here as a signed tensor rather than a norm-only
// signature.
double coefficientScale = ladder[6];
double[] scaledForCoefficient = Scale(omega, coefficientScale);
double[] registeredCoefficientPoint = CurvatureAssembler.Assemble(new ConnectionField(mesh, algebra, scaledForCoefficient)).Coefficients;
var observedCoefficient = new double[registeredCoefficientPoint.Length];
var predictedCoefficient = new double[registeredCoefficientPoint.Length];
Q[] coefficientLinks = Enumerable.Range(0, mesh.EdgeCount).Select(e => Exp(Block(scaledForCoefficient, e))).ToArray();
for (int f = 0; f < mesh.FaceCount; f++)
{
    int v0 = mesh.Faces[f][0], v1 = mesh.Faces[f][1], v2 = mesh.Faces[f][2];
    double[] logContinuous = Log(Link(coefficientLinks, v0, v1) * Link(coefficientLinks, v1, v2) * Link(coefficientLinks, v2, v0));
    double[] x1 = OrientedBoundaryValue(omega, f, 1);
    double[] x2 = OrientedBoundaryValue(omega, f, 2);
    double[] predicted = algebra.Bracket(x2, x1);
    for (int a = 0; a < g; a++)
    {
        observedCoefficient[f * g + a] = (logContinuous[a] - registeredCoefficientPoint[f * g + a]) / (coefficientScale * coefficientScale);
        predictedCoefficient[f * g + a] = predicted[a];
    }
}
double coefficientRelativeError = Norm(Subtract(observedCoefficient, predictedCoefficient)) / System.Math.Max(1e-30, Norm(predictedCoefficient));
double coefficientCosine = Dot(observedCoefficient, predictedCoefficient) / System.Math.Max(1e-30, Norm(observedCoefficient) * Norm(predictedCoefficient));
bool secondOrderMismatchLocalized = System.Math.Abs(continuousSlope - 2.0) <= slopeTolerance
    && System.Math.Abs(boundaryArraySlope - 3.0) <= slopeTolerance
    && coefficientRelativeError <= coefficientTolerance && coefficientCosine > 0.999;
bool correspondenceSupported = System.Math.Abs(continuousSlope - 3.0) <= slopeTolerance
    && continuousResiduals.Zip(boundaryArrayResiduals, (a, b) => a <= 2.0 * b).All(x => x);
string verdictKind = !exactGroupControlsPassed ? taxonomy[3]
    : secondOrderMismatchLocalized ? taxonomy[4]
    : correspondenceSupported ? taxonomy[5] : taxonomy[6];

Emit(new
{
    schemaVersion = 1, phase = 565, phaseId = "phase565-registered-finite-transport-bch-control",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid = true,
    exactBindingsValid = true, resourceAccepted, bindings, upstreamGateOpen = true,
    phase564Verdict = phase564.GetProperty("verdictKind").GetString(),
    knownAnswerBattery = new { ranBeforePhase564Read = true, plantedExpLogError, plantedConjugationTraceError, plantedInverseError, traceTooEarlyDecoyRejected, passed = true },
    registeredTarget = new { mesh = "CreateUniform4D(1)", mesh.VertexCount, mesh.EdgeCount, mesh.FaceCount, representation = "unit-quaternion-su2", faceBasepoint = "Faces[f][0]", composableLoop = "v0-v1-v2-v0" },
    exactFiniteControls = new { maxHolonomyCovariance, maxReverseIdentity, maxBasepointTransport, maxClassFunctionError, tolerance = exactTolerance, passed = exactGroupControlsPassed, traceWasNotUsedForAlgebraicCovariance = true },
    weakFieldComparison = new { rows = weakRows, continuousLoopSlope = continuousSlope, expectedMismatchSlope = 2.0, registeredBoundaryArrayProductSlope = boundaryArraySlope, expectedArrayRemainderSlope = 3.0, slopeTolerance, coefficientScale, coefficientRelativeError, coefficientCosine, coefficientRelativeTolerance = coefficientTolerance, predictedSignedMismatch = "[oriented-boundary-edge-2,oriented-boundary-edge-1]", secondOrderMismatchLocalized, correspondenceSupported },
    interpretation = new { registeredBoundaryArrayOrder = "e01,e02-reversed,e12", composableLoopOrder = "e01,e12,e02-reversed", exactFiniteCovarianceIsPrerequisiteOnly = true, scalarPhase562ResidualUsedAsAcceptanceTarget = false, finiteTransportSourceSelected = false },
    verdictKind, terminalStatus = "registered-finite-transport-bch-control-" + verdictKind,
    phase561GateOpen = false, rngUsed = false, samplingPerformed = false, reprocessingPerformed = false,
    protectedPhase554SeedsRead = false, phases562Or563MutatedOrReinterpreted = false, registeredOperatorMutated = false,
    sourceIdentificationAuthoredOrInferred = false, rulingAuthoredOrInferred = false, o4Discharged = false,
    phase458Satisfied = false, phase481PackCreatedOrMutated = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false, externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
});
Console.WriteLine($"Phase565 verdict: {verdictKind}");
Console.WriteLine($"finite={maxHolonomyCovariance:E6}, continuousSlope={continuousSlope:F6}, arraySlope={boundaryArraySlope:F6}, coefficientError={coefficientRelativeError:E6}");

Q Link(Q[] source, int tail, int head)
{
    int index = edgeIndex[(System.Math.Min(tail, head), System.Math.Max(tail, head))];
    return tail < head ? source[index] : source[index].Inverse();
}
double[] OrientedBoundaryValue(double[] source, int face, int position)
{
    int edge = mesh.FaceBoundaryEdges[face][position], sign = mesh.FaceBoundaryOrientations[face][position];
    return Scale(Block(source, edge), sign);
}
double[] Block(double[] source, int row) { var x = new double[g]; Array.Copy(source, row * g, x, 0, g); return x; }
static Q Conjugate(Q q, Q x) => q * x * q.Inverse();
static Q Exp(double[] x) { double angle = Norm(x); if (angle == 0.0) return Q.Identity; double s = System.Math.Sin(0.5 * angle) / angle; return new Q(System.Math.Cos(0.5 * angle), s * x[0], s * x[1], s * x[2]); }
static double[] Log(Q input) { Q q = input.Normalized(); if (q.W < 0.0) q = -q; double v = System.Math.Sqrt(q.X * q.X + q.Y * q.Y + q.Z * q.Z); if (v < 1e-30) return new[] { 2.0 * q.X, 2.0 * q.Y, 2.0 * q.Z }; double angle = 2.0 * System.Math.Atan2(v, q.W); double s = angle / v; return new[] { s * q.X, s * q.Y, s * q.Z }; }
static double QDistance(Q a, Q b) => System.Math.Sqrt((a.W-b.W)*(a.W-b.W)+(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z));
static double LogSlope(double[] x, double[] y) { double[] lx=x.Select(v => System.Math.Log(v)).ToArray(), ly=y.Select(v => System.Math.Log(v)).ToArray(); double mx=lx.Average(), my=ly.Average(); return lx.Zip(ly,(a,b)=>(a-mx)*(b-my)).Sum()/lx.Sum(a=>(a-mx)*(a-mx)); }
static double[] Wave(int n, double scale, double frequency) { var x = new double[n]; for (int i = 0; i < n; i++) x[i] = scale * (System.Math.Sin((i + 1) * frequency) + 0.37 * System.Math.Cos((i + 1) * frequency * 1.7)); return x; }
static double[] Subtract(double[] a, double[] b) => a.Zip(b, (x, y) => x - y).ToArray();
static double[] Scale(double[] a, double s) => a.Select(x => s * x).ToArray();
static double Dot(double[] a, double[] b) => a.Zip(b, (x, y) => x * y).Sum();
static double Norm(double[] a) => System.Math.Sqrt(Dot(a, a));
static string Sha(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
void Emit(object value) { byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions { WriteIndented = true }); Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!); File.WriteAllBytes(OutputPath, bytes); File.WriteAllBytes(SummaryPath, bytes); }
sealed record Binding(string Id, string Path, string Hash);
readonly record struct Q(double W, double X, double Y, double Z)
{
    public static Q Identity => new(1,0,0,0);
    public static Q operator *(Q a,Q b)=>new(a.W*b.W-a.X*b.X-a.Y*b.Y-a.Z*b.Z,a.W*b.X+a.X*b.W+a.Y*b.Z-a.Z*b.Y,a.W*b.Y-a.X*b.Z+a.Y*b.W+a.Z*b.X,a.W*b.Z+a.X*b.Y-a.Y*b.X+a.Z*b.W);
    public static Q operator -(Q a)=>new(-a.W,-a.X,-a.Y,-a.Z);
    public Q Inverse(){double n=W*W+X*X+Y*Y+Z*Z;return new(W/n,-X/n,-Y/n,-Z/n);}
    public Q Normalized(){double n=System.Math.Sqrt(W*W+X*X+Y*Y+Z*Z);return new(W/n,X/n,Y/n,Z/n);}
}
