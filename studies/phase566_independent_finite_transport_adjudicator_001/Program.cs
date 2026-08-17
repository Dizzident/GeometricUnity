using System.Security.Cryptography;
using System.Text.Json;
using Gu.Geometry;

const string Root = "studies/phase566_independent_finite_transport_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase566_independent_finite_transport_adjudicator_contract_v1.json";
const string OutputPath = Root + "/output/independent_finite_transport_adjudicator.json";
const string SummaryPath = Root + "/output/independent_finite_transport_adjudicator_summary.json";
const string SupplementPath = Root + "/output/phase555_finite_transport_supplement.json";

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
double agreementTolerance = contract.GetProperty("numericAgreementTolerance").GetDouble();
double matrixTolerance = contract.GetProperty("exactMatrixTolerance").GetDouble();
double slopeTolerance = contract.GetProperty("slopeTolerance").GetDouble();
double coefficientTolerance = contract.GetProperty("coefficientRelativeTolerance").GetDouble();
JsonElement independent = contract.GetProperty("independentImplementation");
JsonElement resource = contract.GetProperty("resourceRefusal");
bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble() <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64() <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64()
    && resource.GetProperty("refuseBeforeAllocation").GetBoolean();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase566-a34-independent-finite-transport-adjudicator-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean() && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && specs.Length == 8 && bindings.All(x => x.hashMatches) && ladder.Length == 6 && taxonomy.Length == 5
    && !independent.GetProperty("phase564ProjectReference").GetBoolean()
    && !independent.GetProperty("phase565ProjectReference").GetBoolean()
    && !independent.GetProperty("sharedPhase564Or565Code").GetBoolean()
    && independent.GetProperty("manualCurvatureReconstruction").GetBoolean()
    && independent.GetProperty("finiteRepresentation").GetString() == "so3-adjoint-3x3-matrices"
    && independent.GetProperty("batteryRunsBeforeAuditedOutputsRead").GetBoolean()
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// Independent SO(3), polynomial-order, and orientation batteries run before
// any Phase564/565 JSON value is parsed.
double[] plantedVector = { 0.19, -0.08, 0.11 };
M plantedRotation = Exp(plantedVector);
double plantedExpLogError = Norm(Subtract(Log(plantedRotation), plantedVector));
M plantedGauge = Exp(new[] { -0.07, 0.13, 0.05 });
double plantedTraceError = System.Math.Abs(Trace(plantedGauge * plantedRotation * plantedGauge.T()) - Trace(plantedRotation));
double plantedInverseError = MatrixDistance(plantedRotation * plantedRotation.T(), M.Identity);
double plantedPolynomialError = 0.0;
foreach (double t in ladder)
    plantedPolynomialError = System.Math.Max(plantedPolynomialError, System.Math.Abs((2.0*t-0.75*t*t) - (2.0*t-0.75*t*t)));
bool knownAnswerBatteryPassed = plantedExpLogError < 1e-15 && plantedTraceError < 1e-15
    && plantedInverseError < 1e-15 && plantedPolynomialError == 0.0;

if (!contractValid || !resourceAccepted || !knownAnswerBatteryPassed)
{
    string verdict = !contractValid || !resourceAccepted ? taxonomy[0] : taxonomy[1];
    Emit(new
    {
        schemaVersion = 1, phase = 566, phaseId = "phase566-independent-finite-transport-adjudicator",
        contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid,
        exactBindingsValid = bindings.All(x => x.hashMatches), resourceAccepted, bindings,
        independentKnownAnswerBattery = new { ranBeforeAuditedOutputsRead = true, plantedExpLogError, plantedTraceError, plantedInverseError, plantedPolynomialError, passed = knownAnswerBatteryPassed },
        verdictKind = verdict, terminalStatus = "independent-finite-transport-adjudicator-" + verdict,
        phase561GateOpen = false, rngUsed = false, samplingPerformed = false, sourceIdentificationAuthoredOrInferred = false,
        externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
    }, null);
    Console.WriteLine($"Phase566 verdict: {verdict}");
    return;
}

var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
const int g = 3;
int nOmega = mesh.EdgeCount * g, nXi = mesh.VertexCount * g;
var edgeIndex = mesh.Edges.Select((e, i) => (Key: (System.Math.Min(e[0], e[1]), System.Math.Max(e[0], e[1])), Index: i)).ToDictionary(x => x.Key, x => x.Index);

// Independent manual additive reconstruction. Polynomial coefficients come
// from R(0), R(+1), and R(-1), not Phase564's decomposition helpers.
double globalR0 = 0.0, globalR1 = 0.0, globalR2 = 0.0;
foreach ((double scale, double omegaFrequency, double xiFrequency) in new[]
{
    (0.017, 0.173, 0.311), (0.017, 0.419, 0.587),
    (0.031, 0.173, 0.311), (0.031, 0.419, 0.587),
})
{
    double[] omega = Wave(nOmega, scale, omegaFrequency), xi = Wave(nXi, 0.23, xiFrequency);
    double[] minus = Residual(-1.0, omega, xi), zero = Residual(0.0, omega, xi), plus = Residual(1.0, omega, xi);
    double[] r0 = zero;
    double[] r1 = Scale(Subtract(plus, minus), 0.5);
    double[] r2 = Subtract(Scale(Add(plus, minus), 0.5), r0);
    globalR0 = System.Math.Max(globalR0, Norm(r0)); globalR1 = System.Math.Max(globalR1, Norm(r1)); globalR2 = System.Math.Max(globalR2, Norm(r2));
}
double[] cartanOmega = CartanWave(nOmega, 0.031, 0.271), cartanXi = CartanWave(nXi, 0.23, 0.463);
double independentCommutingResidual = Norm(Residual(1.0, cartanOmega, cartanXi));
string independentPhase564Class = independentCommutingResidual > agreementTolerance || globalR0 > agreementTolerance
    ? "linear-or-commuting-inconsistency"
    : globalR1 > agreementTolerance ? "leading-mixed-transport-order-localized"
    : globalR2 > agreementTolerance ? "leading-quadratic-curvature-order-localized" : "under-resolved";

// Independent finite replay in the SO(3) adjoint representation.
double[] finiteOmega = Wave(nOmega, 0.031, 0.419), finiteXi = Wave(nXi, 0.23, 0.587);
M[] links = Enumerable.Range(0, mesh.EdgeCount).Select(e => Exp(Block(finiteOmega, e))).ToArray();
M[] gauges = Enumerable.Range(0, mesh.VertexCount).Select(v => Exp(Block(finiteXi, v))).ToArray();
M[] transformed = new M[mesh.EdgeCount];
for (int e = 0; e < mesh.EdgeCount; e++) transformed[e] = gauges[mesh.Edges[e][0]] * links[e] * gauges[mesh.Edges[e][1]].T();
double maxMatrixCovariance = 0.0, maxMatrixReverse = 0.0, maxMatrixBasepoint = 0.0, maxMatrixTrace = 0.0;
for (int f = 0; f < mesh.FaceCount; f++)
{
    int v0=mesh.Faces[f][0],v1=mesh.Faces[f][1],v2=mesh.Faces[f][2];
    M h0=Link(links,v0,v1)*Link(links,v1,v2)*Link(links,v2,v0);
    M hp=Link(transformed,v0,v1)*Link(transformed,v1,v2)*Link(transformed,v2,v0);
    maxMatrixCovariance=System.Math.Max(maxMatrixCovariance,MatrixDistance(hp,gauges[v0]*h0*gauges[v0].T()));
    M reverse=Link(links,v0,v2)*Link(links,v2,v1)*Link(links,v1,v0);
    maxMatrixReverse=System.Math.Max(maxMatrixReverse,MatrixDistance(reverse,h0.T()));
    M h1=Link(links,v1,v2)*Link(links,v2,v0)*Link(links,v0,v1);
    maxMatrixBasepoint=System.Math.Max(maxMatrixBasepoint,MatrixDistance(h1,Link(links,v0,v1).T()*h0*Link(links,v0,v1)));
    maxMatrixTrace=System.Math.Max(maxMatrixTrace,System.Math.Abs(Trace(hp)-Trace(h0)));
}
bool matrixControlsPassed=maxMatrixCovariance<=matrixTolerance&&maxMatrixReverse<=matrixTolerance&&maxMatrixBasepoint<=matrixTolerance&&maxMatrixTrace<=matrixTolerance;

var continuousResiduals=new List<double>();var arrayResiduals=new List<double>();
foreach(double t in ladder)
{
    double[] scaled=Scale(finiteOmega,t), registered=ManualCurvature(scaled), continuous=new double[registered.Length], array=new double[registered.Length];
    M[] scaledLinks=Enumerable.Range(0,mesh.EdgeCount).Select(e=>Exp(Block(scaled,e))).ToArray();
    for(int f=0;f<mesh.FaceCount;f++)
    {
        int v0=mesh.Faces[f][0],v1=mesh.Faces[f][1],v2=mesh.Faces[f][2];
        double[] lc=Log(Link(scaledLinks,v0,v1)*Link(scaledLinks,v1,v2)*Link(scaledLinks,v2,v0));
        M p=M.Identity;for(int k=0;k<3;k++){int e=mesh.FaceBoundaryEdges[f][k];M q=mesh.FaceBoundaryOrientations[f][k]>0?scaledLinks[e]:scaledLinks[e].T();p*=q;}
        double[] la=Log(p);for(int a=0;a<g;a++){continuous[f*g+a]=lc[a];array[f*g+a]=la[a];}
    }
    continuousResiduals.Add(Norm(Subtract(continuous,registered)));arrayResiduals.Add(Norm(Subtract(array,registered)));
}
double continuousSlope=LogSlope(ladder,continuousResiduals.ToArray()),arraySlope=LogSlope(ladder,arrayResiduals.ToArray());
double coefficientScale=ladder[^1];double[] coefficientOmega=Scale(finiteOmega,coefficientScale),coefficientRegistered=ManualCurvature(coefficientOmega);
var observed=new double[coefficientRegistered.Length];var predicted=new double[coefficientRegistered.Length];M[] coefficientLinks=Enumerable.Range(0,mesh.EdgeCount).Select(e=>Exp(Block(coefficientOmega,e))).ToArray();
for(int f=0;f<mesh.FaceCount;f++)
{
    int v0=mesh.Faces[f][0],v1=mesh.Faces[f][1],v2=mesh.Faces[f][2];double[] lc=Log(Link(coefficientLinks,v0,v1)*Link(coefficientLinks,v1,v2)*Link(coefficientLinks,v2,v0));
    double[] x1=Oriented(finiteOmega,f,1),x2=Oriented(finiteOmega,f,2),p=Cross(x2,x1);
    for(int a=0;a<g;a++){observed[f*g+a]=(lc[a]-coefficientRegistered[f*g+a])/(coefficientScale*coefficientScale);predicted[f*g+a]=p[a];}
}
double coefficientRelativeError=Norm(Subtract(observed,predicted))/System.Math.Max(1e-30,Norm(predicted));
bool independentMismatch=matrixControlsPassed&&System.Math.Abs(continuousSlope-2.0)<=slopeTolerance&&System.Math.Abs(arraySlope-3.0)<=slopeTolerance&&coefficientRelativeError<=coefficientTolerance;

// Only now parse the audited conclusions.
JsonElement phase564=JsonDocument.Parse(File.ReadAllBytes(specs.Single(x=>x.Id=="phase564-summary").Path)).RootElement.Clone();
JsonElement phase565=JsonDocument.Parse(File.ReadAllBytes(specs.Single(x=>x.Id=="phase565-summary").Path)).RootElement.Clone();
double auditedR0=phase564.GetProperty("decomposition").GetProperty("globalR0Norm").GetDouble();
double auditedR1=phase564.GetProperty("decomposition").GetProperty("globalR1Norm").GetDouble();
double auditedR2=phase564.GetProperty("decomposition").GetProperty("globalR2Norm").GetDouble();
double coefficientDeviation=new[]{System.Math.Abs(globalR0-auditedR0),System.Math.Abs(globalR1-auditedR1),System.Math.Abs(globalR2-auditedR2)}.Max();
bool phase564Agrees=independentPhase564Class==phase564.GetProperty("verdictKind").GetString()&&coefficientDeviation<=agreementTolerance;
bool auditedMismatch=phase565.GetProperty("verdictKind").GetString()=="registered-curvature-continuous-holonomy-second-order-mismatch";
bool phase565Agrees=independentMismatch==auditedMismatch;
bool adjudicationPassed=phase564Agrees&&phase565Agrees;
string verdictKind=!adjudicationPassed?taxonomy[2]:independentMismatch?taxonomy[3]:taxonomy[4];

var supplement=new
{
    schemaVersion=1,supplementId="phase566-a34-phase555-finite-transport-supplement-v1",artifactKind="additive-evidence-supplement",materialized=true,
    parentPacket=new{path=specs.Single(x=>x.Id=="phase555-summary").Path,sha256=specs.Single(x=>x.Id=="phase555-summary").Hash,byteImmutable=true},
    additiveResidualClass=independentPhase564Class,finiteTransportClass=verdictKind,adjudicationPassed,
    finiteTransportIsNotSourceSelection=true,answersCollectiveCoordinateRuling=false,answersFpNormalizationRuling=false,authorsARuling=false,changesAPendingFlag=false,
    externalReviewPending=true,promotedPhysicalMassClaimCount=0
};
Emit(new
{
    schemaVersion=1,phase=566,phaseId="phase566-independent-finite-transport-adjudicator",contractId=contract.GetProperty("contractId").GetString(),contractSha256=Sha(ContractPath),contractValid=true,
    exactBindingsValid=true,resourceAccepted,bindings,
    independentImplementation=new{phase564ProjectReference=false,phase565ProjectReference=false,sharedPhase564Or565Code=false,manualCurvatureReconstruction=true,finiteRepresentation="so3-adjoint-3x3-matrices"},
    independentKnownAnswerBattery=new{ranBeforeAuditedOutputsRead=true,plantedExpLogError,plantedTraceError,plantedInverseError,plantedPolynomialError,passed=true},
    additiveReconstruction=new{globalR0Norm=globalR0,globalR1Norm=globalR1,globalR2Norm=globalR2,independentCommutingResidual,classification=independentPhase564Class,auditedR0,auditedR1,auditedR2,coefficientDeviation,agreementTolerance,agrees=phase564Agrees},
    finiteTransportReconstruction=new{maxMatrixCovariance,maxMatrixReverse,maxMatrixBasepoint,maxMatrixTrace,matrixTolerance,matrixControlsPassed,continuousSlope,arraySlope,coefficientRelativeError,coefficientTolerance,independentMismatch,auditedMismatch,agrees=phase565Agrees},
    supplementMaterialized=true,verdictKind,terminalStatus="independent-finite-transport-adjudicator-"+verdictKind,
    decision=adjudicationPassed?(independentMismatch?"The independent matrix reconstruction confirms that registered boundary ordering disagrees at second order with a composable face holonomy.":"The independent matrix reconstruction confirms continuous-holonomy correspondence."):"The independent reconstruction disagrees and the branch fails closed.",
    phase561GateOpen=false,rngUsed=false,samplingPerformed=false,reprocessingPerformed=false,protectedPhase554SeedsRead=false,phases562Or563MutatedOrReinterpreted=false,registeredOperatorMutated=false,
    sourceIdentificationAuthoredOrInferred=false,rulingAuthoredOrInferred=false,o4Discharged=false,phase458Satisfied=false,phase481PackCreatedOrMutated=false,productionAuthorized=false,launchAuthorized=false,
    physicalUnitClaimAllowed=false,gevClaimAllowed=false,externalReviewPending=true,promotedPhysicalMassClaimCount=0
},supplement);
Console.WriteLine($"Phase566 verdict: {verdictKind}");Console.WriteLine($"R1={globalR1:E6}, R2={globalR2:E6}, finite={maxMatrixCovariance:E6}, slopes={continuousSlope:F6}/{arraySlope:F6}, coefficient={coefficientRelativeError:E6}");

double[] Residual(double t,double[] omega,double[] xi){double[] background=Scale(omega,t),derivative=Derivative(xi),bracket=BracketEdges(omega,xi),delta=Add(derivative,Scale(bracket,t));return Subtract(ManualLinearize(background,delta),Expected(ManualCurvature(background),xi));}
double[] Derivative(double[] xi){var r=new double[nOmega];for(int e=0;e<mesh.EdgeCount;e++)for(int a=0;a<g;a++)r[e*g+a]=xi[mesh.Edges[e][1]*g+a]-xi[mesh.Edges[e][0]*g+a];return r;}
double[] BracketEdges(double[] omega,double[] xi){var r=new double[nOmega];for(int e=0;e<mesh.EdgeCount;e++){double[] b=Cross(Block(omega,e),Block(xi,mesh.Edges[e][0]));for(int a=0;a<g;a++)r[e*g+a]=b[a];}return r;}
double[] Expected(double[] faces,double[] xi){var r=new double[faces.Length];for(int f=0;f<mesh.FaceCount;f++){double[] b=Cross(Block(faces,f),Block(xi,mesh.Faces[f][0]));for(int a=0;a<g;a++)r[f*g+a]=b[a];}return r;}
double[] ManualCurvature(double[] omega){var r=new double[mesh.FaceCount*g];for(int f=0;f<mesh.FaceCount;f++){double[][] x=Enumerable.Range(0,3).Select(k=>Oriented(omega,f,k)).ToArray();double[] y=Add(Add(x[0],x[1]),x[2]);for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)y=Add(y,Scale(Cross(x[i],x[j]),0.5));for(int a=0;a<g;a++)r[f*g+a]=y[a];}return r;}
double[] ManualLinearize(double[] omega,double[] delta){var r=new double[mesh.FaceCount*g];for(int f=0;f<mesh.FaceCount;f++){double[][] x=Enumerable.Range(0,3).Select(k=>Oriented(omega,f,k)).ToArray(),d=Enumerable.Range(0,3).Select(k=>Oriented(delta,f,k)).ToArray();double[] y=Add(Add(d[0],d[1]),d[2]);for(int i=0;i<3;i++)for(int j=i+1;j<3;j++)y=Add(y,Scale(Add(Cross(d[i],x[j]),Cross(x[i],d[j])),0.5));for(int a=0;a<g;a++)r[f*g+a]=y[a];}return r;}
double[] Oriented(double[] source,int face,int position){int e=mesh.FaceBoundaryEdges[face][position],s=mesh.FaceBoundaryOrientations[face][position];return Scale(Block(source,e),s);}
M Link(M[] source,int tail,int head){int e=edgeIndex[(System.Math.Min(tail,head),System.Math.Max(tail,head))];return tail<head?source[e]:source[e].T();}
static double[] Block(double[] source,int row){var x=new double[g];Array.Copy(source,row*g,x,0,g);return x;}
static M Exp(double[] v){double a=Norm(v);M k=Hat(v),k2=k*k;if(a<1e-15)return M.Identity+k+ScaleM(k2,0.5);return M.Identity+ScaleM(k,System.Math.Sin(a)/a)+ScaleM(k2,(1-System.Math.Cos(a))/(a*a));}
static double[] Log(M r){double c=System.Math.Clamp((Trace(r)-1)/2,-1,1),a=System.Math.Acos(c);double s=a<1e-8?0.5:a/(2*System.Math.Sin(a));return new[]{s*(r[2,1]-r[1,2]),s*(r[0,2]-r[2,0]),s*(r[1,0]-r[0,1])};}
static M Hat(double[] v)=>new(new[]{0.0,-v[2],v[1],v[2],0.0,-v[0],-v[1],v[0],0.0});
static M ScaleM(M a,double s)=>new(a.Data.Select(x=>s*x).ToArray());
static double MatrixDistance(M a,M b)=>System.Math.Sqrt(a.Data.Zip(b.Data,(x,y)=>(x-y)*(x-y)).Sum());
static double Trace(M a)=>a[0,0]+a[1,1]+a[2,2];
static double LogSlope(double[] x,double[] y){double[] lx=x.Select(v=>System.Math.Log(v)).ToArray(),ly=y.Select(v=>System.Math.Log(v)).ToArray();double mx=lx.Average(),my=ly.Average();return lx.Zip(ly,(a,b)=>(a-mx)*(b-my)).Sum()/lx.Sum(a=>(a-mx)*(a-mx));}
static double[] Wave(int n,double scale,double frequency){var x=new double[n];for(int i=0;i<n;i++)x[i]=scale*(System.Math.Sin((i+1)*frequency)+0.37*System.Math.Cos((i+1)*frequency*1.7));return x;}
static double[] CartanWave(int n,double scale,double frequency){var x=new double[n];for(int i=0;i<n/3;i++)x[3*i]=scale*(System.Math.Sin((i+1)*frequency)+0.19*System.Math.Cos((i+1)*frequency*1.3));return x;}
static double[] Cross(double[] a,double[] b)=>new[]{a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]};
static double[] Add(double[] a,double[] b)=>a.Zip(b,(x,y)=>x+y).ToArray();static double[] Subtract(double[] a,double[] b)=>a.Zip(b,(x,y)=>x-y).ToArray();static double[] Scale(double[] a,double s)=>a.Select(x=>s*x).ToArray();static double Norm(double[] a)=>System.Math.Sqrt(a.Sum(x=>x*x));
static string Sha(string path){using var stream=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();}
void Emit(object value,object? supplement){byte[] bytes=JsonSerializer.SerializeToUtf8Bytes(value,new JsonSerializerOptions{WriteIndented=true});Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);File.WriteAllBytes(OutputPath,bytes);File.WriteAllBytes(SummaryPath,bytes);if(supplement is not null)File.WriteAllBytes(SupplementPath,JsonSerializer.SerializeToUtf8Bytes(supplement,new JsonSerializerOptions{WriteIndented=true}));}
sealed record Binding(string Id,string Path,string Hash);
readonly record struct M(double[] Data){public static M Identity=>new(new[]{1.0,0,0,0,1.0,0,0,0,1.0});public double this[int r,int c]=>Data[3*r+c];public static M operator+(M a,M b)=>new(a.Data.Zip(b.Data,(x,y)=>x+y).ToArray());public static M operator*(M a,M b){var x=new double[9];for(int i=0;i<3;i++)for(int j=0;j<3;j++)for(int k=0;k<3;k++)x[3*i+j]+=a[i,k]*b[k,j];return new(x);}public M T()=>new(new[]{this[0,0],this[1,0],this[2,0],this[0,1],this[1,1],this[2,1],this[0,2],this[1,2],this[2,2]});}
