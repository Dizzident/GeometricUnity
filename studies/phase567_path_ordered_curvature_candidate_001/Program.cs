using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Phase567;

const string Root = "studies/phase567_path_ordered_curvature_candidate_001";
const string ContractPath = Root + "/preregistration/phase567_path_ordered_curvature_candidate_contract_v1.json";
const string OutputPath = Root + "/output/path_ordered_curvature_candidate.json";
const string SummaryPath = Root + "/output/path_ordered_curvature_candidate_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
JsonElement tolerances = contract.GetProperty("tolerances");
JsonElement resource = contract.GetProperty("resourceRefusal");
double exactTolerance = tolerances.GetProperty("exactAlgebraic").GetDouble();
double groupTolerance = tolerances.GetProperty("exactGroup").GetDouble();
double differentialTolerance = tolerances.GetProperty("linearizationFiniteDifference").GetDouble();
double transposeTolerance = tolerances.GetProperty("transposeDuality").GetDouble();
double slopeTolerance = tolerances.GetProperty("slope").GetDouble();
double[] ladder = contract.GetProperty("amplitudeLadder").EnumerateArray().Select(x => x.GetDouble()).ToArray();
bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble() <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64() <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64()
    && resource.GetProperty("refuseBeforeAllocation").GetBoolean();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase567-a35-path-ordered-curvature-candidate-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && specs.Length == 14 && bindings.All(x => x.hashMatches)
    && taxonomy.Length == 8 && ladder.Length == 6
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
double[] p0 = [0.17, -0.08, 0.03], p1 = [-0.04, 0.13, 0.09], p2 = [0.07, 0.02, -0.11];
double[] plantedNew = Bch2(p0, p1, p2);
double[] plantedOld = Bch2(p0, p2, p1);
double[] plantedCorrection = algebra.Bracket(p1, p2);
double plantedCorrectionError = ScaledVectorError(Subtract(plantedNew, plantedOld), plantedCorrection);
double[] c0 = [0.2, 0.0, 0.0], c1 = [-0.07, 0.0, 0.0], c2 = [0.11, 0.0, 0.0];
double plantedCommutingError = Norm(Subtract(Bch2(c0, c1, c2), Bch2(c0, c2, c1)));
bool knownAnswerBatteryPassed = plantedCorrectionError <= exactTolerance && plantedCommutingError == 0.0
    && Norm(Subtract(plantedNew, plantedOld)) > 1e-4;

JsonElement p564 = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase564-summary").Path)).RootElement.Clone();
JsonElement p565 = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase565-summary").Path)).RootElement.Clone();
JsonElement p566 = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase566-summary").Path)).RootElement.Clone();
JsonElement required = contract.GetProperty("requiredUpstreamTerminals");
bool a34GateOpen = p564.GetProperty("verdictKind").GetString() == required.GetProperty("phase564").GetString()
    && p565.GetProperty("verdictKind").GetString() == required.GetProperty("phase565").GetString()
    && p566.GetProperty("verdictKind").GetString() == required.GetProperty("phase566").GetString();

var meshRows = new List<object>();
bool pathControlsPassed = true, differentialControlsPassed = true;
double globalCorrectionError = 0.0, globalLinearTermError = 0.0, globalFiniteDifferenceError = 0.0, globalTransposeError = 0.0;
foreach ((string id, SimplicialMesh mesh) in new[]
{
    ("CreateUniform4D(1)", SimplicialMeshGenerator.CreateUniform4D(1)),
    ("CreateUniform4DPeriodic(3,latticeCanonical:true)", SimplicialMeshGenerator.CreateUniform4DPeriodic(3, latticeCanonical: true)),
})
{
    int dim = algebra.Dimension, n = mesh.EdgeCount * dim;
    double[] omega = Wave(n, 0.027, 0.419), direction = Wave(n, 0.019, 0.587);
    double[] candidate = PathOrderedCurvatureCandidate.Assemble(mesh, algebra, omega);
    double[] registered = CurvatureAssembler.Assemble(new ConnectionField(mesh, algebra, omega)).Coefficients;
    var predicted = new double[candidate.Length];
    bool everyFaceComposable = true;
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        int[] order = [0, 2, 1];
        (int Tail, int Head) previous = DirectedEdge(mesh, f, order[0]);
        (int Tail, int Head) second = DirectedEdge(mesh, f, order[1]);
        (int Tail, int Head) third = DirectedEdge(mesh, f, order[2]);
        everyFaceComposable &= previous.Head == second.Tail && second.Head == third.Tail && third.Head == previous.Tail
            && new[] { mesh.FaceBoundaryEdges[f][0], mesh.FaceBoundaryEdges[f][1], mesh.FaceBoundaryEdges[f][2] }.Distinct().Count() == 3;
        double[] b1 = Oriented(mesh, omega, f, 1, dim), b2 = Oriented(mesh, omega, f, 2, dim);
        double[] correction = algebra.Bracket(b2, b1);
        Array.Copy(correction, 0, predicted, f * dim, dim);
    }
    double correctionError = ScaledVectorError(Subtract(candidate, registered), predicted);
    double[] zero = new double[n];
    double[] linearCandidate = PathOrderedCurvatureCandidate.Linearize(mesh, algebra, zero, direction);
    var linearRegistered = new double[mesh.FaceCount * dim];
    for (int f = 0; f < mesh.FaceCount; f++)
        for (int k = 0; k < mesh.FaceBoundaryEdges[f].Length; k++)
            for (int a = 0; a < dim; a++)
                linearRegistered[f * dim + a] += mesh.FaceBoundaryOrientations[f][k]
                    * direction[mesh.FaceBoundaryEdges[f][k] * dim + a];
    double linearTermError = ScaledVectorError(linearCandidate, linearRegistered);
    const double h = 1e-6;
    double[] finiteDifference = Scale(Subtract(
        PathOrderedCurvatureCandidate.Assemble(mesh, algebra, AddScaled(omega, direction, h)),
        PathOrderedCurvatureCandidate.Assemble(mesh, algebra, AddScaled(omega, direction, -h))), 0.5 / h);
    double[] analytic = PathOrderedCurvatureCandidate.Linearize(mesh, algebra, omega, direction);
    double finiteDifferenceError = ScaledVectorError(analytic, finiteDifference);
    double[] faceCovector = Wave(mesh.FaceCount * dim, 0.031, 0.271);
    double transposeError = ScaledError(Dot(analytic, faceCovector), Dot(direction,
        PathOrderedCurvatureCandidate.LinearizeTranspose(mesh, algebra, omega, faceCovector)));
    pathControlsPassed &= everyFaceComposable && correctionError <= exactTolerance && linearTermError <= exactTolerance;
    differentialControlsPassed &= finiteDifferenceError <= differentialTolerance && transposeError <= transposeTolerance;
    globalCorrectionError = Math.Max(globalCorrectionError, correctionError);
    globalLinearTermError = Math.Max(globalLinearTermError, linearTermError);
    globalFiniteDifferenceError = Math.Max(globalFiniteDifferenceError, finiteDifferenceError);
    globalTransposeError = Math.Max(globalTransposeError, transposeError);
    meshRows.Add(new { mesh = id, mesh.VertexCount, mesh.EdgeCount, mesh.FaceCount, everyFaceComposable, correctionError, linearTermError, finiteDifferenceError, transposeError });
}

var controlMesh = SimplicialMeshGenerator.CreateUniform4D(1);
int g = algebra.Dimension, nOmega = controlMesh.EdgeCount * g;
double[] baseOmega = Wave(nOmega, 0.031, 0.419);
var edgeIndex = controlMesh.Edges.Select((e, i) => (Key: (Math.Min(e[0], e[1]), Math.Max(e[0], e[1])), Index: i)).ToDictionary(x => x.Key, x => x.Index);
Q[] links = Enumerable.Range(0, controlMesh.EdgeCount).Select(e => Exp(Block(baseOmega, e, g))).ToArray();
double[] xi = Wave(controlMesh.VertexCount * g, 0.23, 0.587);
Q[] gauges = Enumerable.Range(0, controlMesh.VertexCount).Select(v => Exp(Block(xi, v, g))).ToArray();
Q[] transformed = new Q[controlMesh.EdgeCount];
for (int e = 0; e < controlMesh.EdgeCount; e++)
    transformed[e] = gauges[controlMesh.Edges[e][0]] * links[e] * gauges[controlMesh.Edges[e][1]].Inverse();
double maxHolonomyCovariance = 0.0, maxReverseIdentity = 0.0, maxBasepointTransport = 0.0, maxClassFunctionError = 0.0;
for (int f = 0; f < controlMesh.FaceCount; f++)
{
    int v0 = controlMesh.Faces[f][0], v1 = controlMesh.Faces[f][1], v2 = controlMesh.Faces[f][2];
    Q h0 = Link(links, v0, v1) * Link(links, v1, v2) * Link(links, v2, v0);
    Q hp = Link(transformed, v0, v1) * Link(transformed, v1, v2) * Link(transformed, v2, v0);
    maxHolonomyCovariance = Math.Max(maxHolonomyCovariance, QDistance(hp, gauges[v0] * h0 * gauges[v0].Inverse()));
    Q reverse = Link(links, v0, v2) * Link(links, v2, v1) * Link(links, v1, v0);
    maxReverseIdentity = Math.Max(maxReverseIdentity, QDistance(reverse, h0.Inverse()));
    Q shifted = Link(links, v1, v2) * Link(links, v2, v0) * Link(links, v0, v1);
    maxBasepointTransport = Math.Max(maxBasepointTransport, QDistance(shifted, Link(links, v0, v1).Inverse() * h0 * Link(links, v0, v1)));
    maxClassFunctionError = Math.Max(maxClassFunctionError, Math.Abs(hp.W - h0.W));
}
bool finiteTransportControlsPassed = maxHolonomyCovariance <= groupTolerance && maxReverseIdentity <= groupTolerance
    && maxBasepointTransport <= groupTolerance && maxClassFunctionError <= groupTolerance;

var candidateResiduals = new List<double>();
var registeredResiduals = new List<double>();
var weakRows = new List<object>();
foreach (double t in ladder)
{
    double[] scaled = Scale(baseOmega, t);
    double[] candidate = PathOrderedCurvatureCandidate.Assemble(controlMesh, algebra, scaled);
    double[] registered = CurvatureAssembler.Assemble(new ConnectionField(controlMesh, algebra, scaled)).Coefficients;
    var logHolonomy = new double[candidate.Length];
    Q[] scaledLinks = Enumerable.Range(0, controlMesh.EdgeCount).Select(e => Exp(Block(scaled, e, g))).ToArray();
    for (int f = 0; f < controlMesh.FaceCount; f++)
    {
        int v0 = controlMesh.Faces[f][0], v1 = controlMesh.Faces[f][1], v2 = controlMesh.Faces[f][2];
        double[] log = Log(Link(scaledLinks, v0, v1) * Link(scaledLinks, v1, v2) * Link(scaledLinks, v2, v0));
        Array.Copy(log, 0, logHolonomy, f * g, g);
    }
    double candidateResidual = Norm(Subtract(logHolonomy, candidate));
    double registeredResidual = Norm(Subtract(logHolonomy, registered));
    candidateResiduals.Add(candidateResidual); registeredResiduals.Add(registeredResidual);
    weakRows.Add(new { t, candidateResidual, registeredResidual });
}
double candidateSlope = LogSlope(ladder, candidateResiduals.ToArray());
double registeredSlope = LogSlope(ladder, registeredResiduals.ToArray());
bool secondOrderCorrespondencePassed = Math.Abs(candidateSlope - 3.0) <= slopeTolerance
    && Math.Abs(registeredSlope - 2.0) <= slopeTolerance;

string verdict = !contractValid || !resourceAccepted ? taxonomy[0]
    : !knownAnswerBatteryPassed ? taxonomy[1]
    : !a34GateOpen ? taxonomy[2]
    : !pathControlsPassed ? taxonomy[3]
    : !differentialControlsPassed ? taxonomy[4]
    : !finiteTransportControlsPassed ? taxonomy[5]
    : !secondOrderCorrespondencePassed ? taxonomy[6] : taxonomy[7];
bool phase568EvaluationGateOpen = verdict == taxonomy[7];

Emit(new
{
    schemaVersion = 1, phase = 567, phaseId = "phase567-path-ordered-curvature-candidate",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid,
    exactBindingsValid = bindings.All(x => x.hashMatches), resourceAccepted, bindings, a34GateOpen,
    knownAnswerBattery = new { ranBeforeAuditedOutputsRead = true, plantedCorrectionError, plantedCommutingError, passed = knownAnswerBatteryPassed },
    candidateSpecification = new { candidateId = PathOrderedCurvatureCandidate.CandidateId, boundaryPositionOrder = new[] { 0, 2, 1 }, directedPath = "v0-to-v1,v1-to-v2,v2-to-v0", exactCandidateMinusRegistered = "[b2,b1]", truncatedBch2NotExactlyFiniteCovariant = true },
    topologyAndDifferentialControls = new { meshRows, globalCorrectionError, globalLinearTermError, globalFiniteDifferenceError, globalTransposeError, pathControlsPassed, differentialControlsPassed },
    finiteTransportControls = new { maxHolonomyCovariance, maxReverseIdentity, maxBasepointTransport, maxClassFunctionError, passed = finiteTransportControlsPassed },
    weakFieldCorrespondence = new { rows = weakRows, candidateResidualSlope = candidateSlope, expectedCandidateSlope = 3.0, registeredResidualSlope = registeredSlope, expectedRegisteredSlope = 2.0, passed = secondOrderCorrespondencePassed },
    verdictKind = verdict, terminalStatus = "path-ordered-curvature-candidate-" + verdict, phase568EvaluationGateOpen,
    rngUsed = false, samplingPerformed = false, reprocessingPerformed = false, protectedPhase554SeedsRead = false,
    registeredOperatorMutated = false, phase548RepairedOrReinterpreted = false, sourceIdentificationAuthoredOrInferred = false,
    phase561GateOpen = false, rulingAuthoredOrInferred = false, o4Discharged = false, phase458Satisfied = false,
    phase481PackCreatedOrMutated = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false, externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
});
Console.WriteLine($"Phase567 verdict: {verdict}");
Console.WriteLine($"correction={globalCorrectionError:E6}, J={globalFiniteDifferenceError:E6}, JT={globalTransposeError:E6}, slopes={candidateSlope:F6}/{registeredSlope:F6}");

double[] Bch2(double[] q0, double[] q1, double[] q2)
{
    double[] value = Add(Add(q0, q1), q2);
    foreach (double[] bracket in new[] { algebra.Bracket(q0, q1), algebra.Bracket(q0, q2), algebra.Bracket(q1, q2) })
        for (int a = 0; a < value.Length; a++) value[a] += 0.5 * bracket[a];
    return value;
}
(int Tail, int Head) DirectedEdge(SimplicialMesh mesh, int face, int position)
{
    int edge = mesh.FaceBoundaryEdges[face][position], sign = mesh.FaceBoundaryOrientations[face][position];
    int[] ends = mesh.Edges[edge];
    return sign > 0 ? (ends[0], ends[1]) : (ends[1], ends[0]);
}
double[] Oriented(SimplicialMesh mesh, double[] source, int face, int position, int dim)
{
    int edge = mesh.FaceBoundaryEdges[face][position], sign = mesh.FaceBoundaryOrientations[face][position];
    return Scale(Block(source, edge, dim), sign);
}
Q Link(Q[] source, int tail, int head)
{
    int index = edgeIndex[(Math.Min(tail, head), Math.Max(tail, head))];
    return tail < head ? source[index] : source[index].Inverse();
}
static double[] Block(double[] source, int row, int dim) { var x = new double[dim]; Array.Copy(source, row * dim, x, 0, dim); return x; }
static Q Exp(double[] x) { double angle = Norm(x); if (angle == 0.0) return Q.Identity; double s = Math.Sin(0.5 * angle) / angle; return new Q(Math.Cos(0.5 * angle), s*x[0], s*x[1], s*x[2]); }
static double[] Log(Q input) { Q q=input.Normalized(); if(q.W<0.0) q=-q; double v=Math.Sqrt(q.X*q.X+q.Y*q.Y+q.Z*q.Z); if(v<1e-30)return [2*q.X,2*q.Y,2*q.Z]; double s=2*Math.Atan2(v,q.W)/v; return [s*q.X,s*q.Y,s*q.Z]; }
static double QDistance(Q a,Q b)=>Math.Sqrt((a.W-b.W)*(a.W-b.W)+(a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y)+(a.Z-b.Z)*(a.Z-b.Z));
static double LogSlope(double[] x,double[] y){double[] lx=x.Select(v=>Math.Log(v)).ToArray(),ly=y.Select(v=>Math.Log(v)).ToArray();double mx=lx.Average(),my=ly.Average();return lx.Zip(ly,(a,b)=>(a-mx)*(b-my)).Sum()/lx.Sum(a=>(a-mx)*(a-mx));}
static double[] Wave(int n,double scale,double frequency){var x=new double[n];for(int i=0;i<n;i++)x[i]=scale*(Math.Sin((i+1)*frequency)+0.37*Math.Cos((i+1)*frequency*1.7));return x;}
static double[] Add(double[] a,double[] b)=>a.Zip(b,(x,y)=>x+y).ToArray();
static double[] AddScaled(double[] a,double[] b,double scale)=>a.Zip(b,(x,y)=>x+scale*y).ToArray();
static double[] Subtract(double[] a,double[] b)=>a.Zip(b,(x,y)=>x-y).ToArray();
static double[] Scale(double[] a,double s)=>a.Select(x=>s*x).ToArray();
static double Dot(double[] a,double[] b)=>a.Zip(b,(x,y)=>x*y).Sum();
static double Norm(double[] a)=>Math.Sqrt(Dot(a,a));
static double ScaledError(double a,double b)=>Math.Abs(a-b)/Math.Max(1.0,Math.Max(Math.Abs(a),Math.Abs(b)));
static double ScaledVectorError(double[] a,double[] b)=>Norm(Subtract(a,b))/Math.Max(1.0,Math.Max(Norm(a),Norm(b)));
static string Sha(string path){using var stream=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();}
void Emit(object value){byte[] bytes=JsonSerializer.SerializeToUtf8Bytes(value,new JsonSerializerOptions{WriteIndented=true});Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);File.WriteAllBytes(OutputPath,bytes);File.WriteAllBytes(SummaryPath,bytes);}
sealed record Binding(string Id,string Path,string Hash);
readonly record struct Q(double W,double X,double Y,double Z)
{
    public static Q Identity=>new(1,0,0,0);
    public static Q operator *(Q a,Q b)=>new(a.W*b.W-a.X*b.X-a.Y*b.Y-a.Z*b.Z,a.W*b.X+a.X*b.W+a.Y*b.Z-a.Z*b.Y,a.W*b.Y-a.X*b.Z+a.Y*b.W+a.Z*b.X,a.W*b.Z+a.X*b.Y-a.Y*b.X+a.Z*b.W);
    public static Q operator -(Q a)=>new(-a.W,-a.X,-a.Y,-a.Z);
    public Q Inverse(){double n=W*W+X*X+Y*Y+Z*Z;return new(W/n,-X/n,-Y/n,-Z/n);}
    public Q Normalized(){double n=Math.Sqrt(W*W+X*X+Y*Y+Z*Z);return new(W/n,X/n,Y/n,Z/n);}
}
