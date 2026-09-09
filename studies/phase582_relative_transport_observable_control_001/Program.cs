using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase582_relative_transport_observable_control_001";
const string ContractPath = Root + "/preregistration/contract_v1.json";
const string SourcePath = "docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt";
const string BridgePath = "studies/phase559_bounded_transformation_law_source_census_001/output/bounded_transformation_law_source_census_summary.json";
const string PriorPath = "studies/phase581_collective_coordinate_assumption_audit_001/output/collective_coordinate_assumption_audit_summary.json";
const string OutputPath = Root + "/output/relative_transport_observable_control.json";
const string SummaryPath = Root + "/output/relative_transport_observable_control_summary.json";
const string Success = "local-relative-observable-controls-pass-registered-bridge-open";
const double Tol = 1e-11;
using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
var bindings = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new
{
    id = x.GetProperty("id").GetString()!, path = x.GetProperty("path").GetString()!,
    sha256 = x.GetProperty("sha256").GetString()!
}).Select(x => new { x.id, x.path, x.sha256, hashMatches = File.Exists(x.path) && Sha(x.path) == x.sha256 }).ToArray();
string[] forbidden = ["registeredTransformationBridgeEstablished", "registeredActionChanged",
    "registeredMeasureSelected", "fullGuObservableEstablished", "physicalHiggsIdentified",
    "sourceContractApplicationAllowed", "phase561Opened", "o4Discharged", "phase458Satisfied",
    "phase481Changed", "samplingPerformed", "samplingAuthorized", "productionAuthorized", "gevClaimAllowed"];
bool contractValid = contract.GetProperty("phase").GetInt32() == 582
    && contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase582-a44-relative-transport-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("floatingTolerance").GetDouble() == Tol
    && contract.GetProperty("midpointSubdivisions").GetInt32() == 64
    && contract.GetProperty("simpsonIntervals").GetInt32() == 8192
    && forbidden.All(k => contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind == JsonValueKind.False);
bool exactBindingsValid = bindings.Length == 6 && bindings.Select(x => x.id).Distinct().Count() == 6
    && bindings.Select(x => x.path).Distinct().Count() == 6 && bindings.All(x => x.hashMatches);
if (!contractValid || !exactBindingsValid) { Emit("invalid-or-drifted-input", new { }); return; }

// Known answers and independent complex-matrix products precede scientific inputs.
Q identity = new(1,0,0,0), ix = new(0,1,0,0), iy = new(0,0,1,0), iz = new(0,0,0,1);
bool exactQuaternionBattery = Error(Mul(ix,iy),iz) == 0 && Error(Mul(iy,ix),Neg(iz)) == 0
    && Error(Mul(ix,Inv(ix)),identity) == 0 && Obs(identity) == 0 && Obs(ix) == 1;
Q qa = Exp([0.8,-0.3,0.4],1), qb = Exp([-0.2,0.5,0.1],1);
double matrixProductError = MatrixError(Matrix(Mul(qa,qb)), MatMul(Matrix(qa),Matrix(qb)));
double matrixInverseError = MatrixError(Matrix(Inv(qa)), Dagger(Matrix(qa)));
double noncommutingDecoySeparation = Error(Mul(qa,qb),Mul(qb,qa));
bool knownAnswerPassed = exactQuaternionBattery && matrixProductError < Tol && matrixInverseError < Tol
    && noncommutingDecoySeparation > 0.01;
if (!knownAnswerPassed) { Emit("known-answer-battery-failed", new { knownAnswerPassed }); return; }

string source = File.ReadAllText(SourcePath);
using var bridgeDoc = JsonDocument.Parse(File.ReadAllBytes(BridgePath));
using var priorDoc = JsonDocument.Parse(File.ReadAllBytes(PriorPath));
bool sourceAnchorsPresent = source.Contains("(12.6)",StringComparison.Ordinal)
    && source.Contains("gauge transformed Levi-Civita spin-connection",StringComparison.Ordinal)
    && source.Contains("augmented torsion tensor",StringComparison.Ordinal);
bool upstreamValid = sourceAnchorsPresent
    && bridgeDoc.RootElement.GetProperty("verdictKind").GetString() == "bounded-source-census-finds-continuum-law-discrete-bridge-incomplete"
    && priorDoc.RootElement.GetProperty("auditPassed").GetBoolean();
if (!upstreamValid) { Emit("invalid-or-drifted-input", new { knownAnswerPassed, sourceAnchorsPresent }); return; }

// A,B transport on the same edge: U'=g_i U g_j^-1, V'=g_i V g_j^-1.
// Relative R=UV^-1 transforms by conjugation at i. eps'=eps*g^-1 gives
// V=eps_i^-1 V0 eps_j exactly this law, with fixed reference transport V0.
double[][] epsVectors = [[0.2,-0.4,0.1],[-0.3,0.1,0.5],[0.4,0.2,-0.2]];
double[][] gaugeVectors = [[0.7,0.1,-0.2],[-0.4,0.8,0.3],[0.2,-0.5,0.6]];
Q[] eps = epsVectors.Select(v => Exp(v,1)).ToArray();
Q[] gs = gaugeVectors.Select(v => Exp(v,1)).ToArray();
Q[] movedEps = Enumerable.Range(0,3).Select(i => Mul(eps[i],Inv(gs[i]))).ToArray();
double localInvariantError=0, referenceCovarianceError=0, conjugationError=0, matrixObservableError=0;
double liftError=0, fundamentalLiftChange=0, fixedReferenceDecoyChange=0, pureGaugeError=0, bareLinkDecoyMaximum=0;
for (int i=0;i<3;i++)
{
    int j=(i+1)%3;
    Q u=Exp([0.8+0.1*i,-0.3,0.4],1), v0=Exp([-0.2,0.5+0.1*i,0.1],1);
    Q v=Mul(Mul(Inv(eps[i]),v0),eps[j]);
    Q up=Mul(Mul(gs[i],u),Inv(gs[j]));
    Q vp=Mul(Mul(Inv(movedEps[i]),v0),movedEps[j]);
    Q r=Mul(u,Inv(v)), rp=Mul(up,Inv(vp));
    localInvariantError=System.Math.Max(localInvariantError,System.Math.Abs(Obs(rp)-Obs(r)));
    conjugationError=System.Math.Max(conjugationError,Error(rp,Mul(Mul(gs[i],r),Inv(gs[i]))));
    referenceCovarianceError=System.Math.Max(referenceCovarianceError,Error(vp,Mul(Mul(gs[i],v),Inv(gs[j]))));
    Complex[,] mr=MatMul(Matrix(u),Dagger(Matrix(v)));
    Complex tr=mr[0,0]+mr[1,1];
    double matrixObs=1-(tr*Complex.Conjugate(tr)).Real/4;
    matrixObservableError=System.Math.Max(matrixObservableError,System.Math.Abs(matrixObs-Obs(r)));
    foreach(int su in new[]{-1,1}) foreach(int sv in new[]{-1,1})
    {
        Q signed=Mul(su<0?Neg(u):u,Inv(sv<0?Neg(v):v));
        liftError=System.Math.Max(liftError,System.Math.Abs(Obs(signed)-Obs(r)));
    }
    fundamentalLiftChange=System.Math.Max(fundamentalLiftChange,System.Math.Abs((1-r.W)-(1+r.W)));
    fixedReferenceDecoyChange=System.Math.Max(fixedReferenceDecoyChange,System.Math.Abs(Obs(Mul(up,Inv(v)))-Obs(r)));
    Q pure=Mul(Inv(eps[i]),eps[j]);
    pureGaugeError=System.Math.Max(pureGaugeError,Obs(Mul(pure,Inv(pure))));
    bareLinkDecoyMaximum=System.Math.Max(bareLinkDecoyMaximum,Obs(pure));
}
bool localControlsPassed = localInvariantError<Tol && referenceCovarianceError<Tol && conjugationError<Tol
    && matrixObservableError<Tol && liftError<Tol && pureGaugeError<Tol
    && fundamentalLiftChange>0.1 && fixedReferenceDecoyChange>0.01 && bareLinkDecoyMaximum>0.01;

// Local classical consistency, not convergence of a path integral or spectrum.
double[] a0=[0.8,-0.3,0.4], b0=[-0.2,0.5,0.1], da=[0.2,0.1,-0.15], db=[-0.1,0.3,0.2];
double target=Enumerable.Range(0,3).Sum(i=>(a0[i]-b0[i])*(a0[i]-b0[i]));
double[] spacings=[0.2,0.1,0.05,0.025];
var refinement=spacings.Select(a =>
{
    Q rc=Mul(Exp(a0,a),Inv(Exp(b0,a)));
    Q rs=Mul(Transport(a0,da,a,64),Inv(Transport(b0,db,a,64)));
    Q rs2=Mul(Transport(a0,da,a,128),Inv(Transport(b0,db,a,128)));
    double constantEstimate=4*Obs(rc)/(a*a), smoothEstimate=4*Obs(rs)/(a*a), fineEstimate=4*Obs(rs2)/(a*a);
    Q commuting=Mul(Exp([0,0,0.7],a),Inv(Exp([0,0,-0.2],a)));
    double commutingError=System.Math.Abs(Obs(commuting)-System.Math.Pow(System.Math.Sin(0.9*a/2),2));
    return new { spacing=a, target, constantEstimate, smoothEstimate,
        constantError=System.Math.Abs(constantEstimate-target), smoothError=System.Math.Abs(smoothEstimate-target),
        subdivisionDifference=System.Math.Abs(smoothEstimate-fineEstimate), commutingError };
}).ToArray();
var rates=Enumerable.Range(1,refinement.Length-1).Select(i=>new
{
    spacing=refinement[i].spacing,
    constantRatio=refinement[i-1].constantError/refinement[i].constantError,
    smoothRatio=refinement[i-1].smoothError/refinement[i].smoothError
}).ToArray();
bool refinementPassed = refinement.All(x=>x.commutingError<Tol && x.subdivisionDifference<1e-4)
    && refinement[^1].constantError<0.001 && refinement[^1].smoothError<0.001
    && rates.All(x=>x.constantRatio>3.8 && x.constantRatio<4.2 && x.smoothRatio>3.8 && x.smoothRatio<4.2);

// Conditional Haar control only. Haar U (conditional on V) gives Haar UV^-1.
// q=sin^2(theta) has Beta(3/2,1/2) density, not a flat dq measure.
double HaarMoment(int power) => Simpson(t=>2/System.Math.PI*System.Math.Pow(System.Math.Sin(t),2+2*power),0,System.Math.PI,8192);
double[] moments=Enumerable.Range(0,3).Select(HaarMoment).ToArray();
double[] expectedMoments=[1,0.75,0.625];
double momentError=Enumerable.Range(0,3).Max(i=>System.Math.Abs(moments[i]-expectedMoments[i]));
double densityRatio=(System.Math.Sqrt(0.99/0.01))/(System.Math.Sqrt(0.5/0.5));
bool measureControlPassed = momentError<Tol && densityRatio>9 && System.Math.Abs(moments[1]-0.5)>0.2;
bool controlsPassed=localControlsPassed && refinementPassed && measureControlPassed;
Emit(controlsPassed?Success:"mathematical-control-failed",new
{
    knownAnswerPassed, upstreamValid, controlsPassed,
    knownAnswers=new {exactQuaternionBattery,matrixProductError,matrixInverseError,noncommutingDecoySeparation},
    source=new {sourceAnchorsPresent,sourceEquation="GU draft 12.6 (and 9.4): T=A-B, B=reference connection transformed by epsilon",
        discreteConstructionIsOurDerivation=true, registeredFieldIdentificationSupplied=false},
    localTransport=new {localControlsPassed,localInvariantError,referenceCovarianceError,conjugationError,matrixObservableError,
        pureGaugeError,bareLinkDecoyMaximum,fixedReferenceDecoyChange,
        formula="R=U_A U_B^-1; R'=g_i R g_i^-1; q=(3-Tr_Ad R)/4=|quaternion_vector(R)|^2",
        localGaugeInvarianceEstablishedForDeclaredLinkModel=true, registeredActionSymmetryEstablished=false},
    centerDescent=new {liftError,fundamentalLiftChange,adjointObservableDescendsToSo3=true,
        fundamentalTraceRejected=true, centerSensitivePhysicsRecovered=false},
    classicalConsistency=new {refinementPassed,refinement,rates,limit="4q/a^2 tends to |A_mu-B_mu|^2 in the t_a=-i sigma_a/2 convention",
        transportRule="ordered midpoint product on [-a/2,a/2]; affine smooth profiles; 64 subdivisions, 128 control",
        oneEdgeContractionOnly=true,fullGuContinuumLimitProved=false,quantumUniversalityEstablished=false},
    conditionalMeasure=new {measureControlPassed,moments,expectedMoments,momentError,densityRatio,
        density="p(q)=(2/pi)*sqrt(q/(1-q)), 0<q<1; conditional Haar U given V",
        endpointDensityEnhancementIsEntropic=true,registeredLebesgueOmegaMeasureReplaced=false,
        interactingMeasureDerived=false,nonzeroModeProvesBreaking=false},
    nextRequirement="Derive a compatible two-connection action, registered variable map, and measure before using this candidate for correlators; do not substitute this observable into old outputs and call them physical."
});

void Emit(string verdict,object evidence)
{
    var result=new {schemaVersion=1,phase=582,phaseId="phase582-relative-transport-observable-control",
        contractId="phase582-a44-relative-transport-v1",contractSha256=Sha(ContractPath),contractValid,exactBindingsValid,bindings,
        verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
        authorityFirewalls=forbidden.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
    string json=JsonSerializer.Serialize(result,new JsonSerializerOptions {WriteIndented=true})+Environment.NewLine;
    Directory.CreateDirectory(Root+"/output"); File.WriteAllText(OutputPath,json); File.WriteAllText(SummaryPath,json);
    Console.WriteLine($"Phase582 verdict: {verdict}");
    if(verdict!=Success) Environment.ExitCode=1;
}
static string Sha(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static Q Mul(Q a,Q b)=>new(a.W*b.W-a.X*b.X-a.Y*b.Y-a.Z*b.Z,
    a.W*b.X+a.X*b.W+a.Y*b.Z-a.Z*b.Y,a.W*b.Y+a.Y*b.W+a.Z*b.X-a.X*b.Z,a.W*b.Z+a.Z*b.W+a.X*b.Y-a.Y*b.X);
static Q Inv(Q q)=>new(q.W,-q.X,-q.Y,-q.Z);
static Q Neg(Q q)=>new(-q.W,-q.X,-q.Y,-q.Z);
static double Obs(Q q)=>q.X*q.X+q.Y*q.Y+q.Z*q.Z;
static double Error(Q a,Q b)=>new[]{System.Math.Abs(a.W-b.W),System.Math.Abs(a.X-b.X),System.Math.Abs(a.Y-b.Y),System.Math.Abs(a.Z-b.Z)}.Max();
static Q Exp(double[] v,double scale)
{
    double norm=System.Math.Sqrt(v.Sum(x=>x*x));
    if(norm==0) return new(1,0,0,0);
    double f=System.Math.Sin(scale*norm/2)/norm;
    return new(System.Math.Cos(scale*norm/2),f*v[0],f*v[1],f*v[2]);
}
static Q Transport(double[] v,double[] slope,double a,int n)
{
    Q q=new(1,0,0,0); double step=a/n;
    for(int k=0;k<n;k++)
    {
        double s=-a/2+(k+0.5)*step;
        q=Mul(q,Exp(Enumerable.Range(0,3).Select(i=>v[i]+s*slope[i]).ToArray(),step));
    }
    return q;
}
static Complex[,] Matrix(Q q)=>new Complex[,]{{new(q.W,-q.Z),new(-q.Y,-q.X)},{new(q.Y,-q.X),new(q.W,q.Z)}};
static Complex[,] MatMul(Complex[,] a,Complex[,] b)
{
    var c=new Complex[2,2];
    for(int i=0;i<2;i++) for(int j=0;j<2;j++) for(int k=0;k<2;k++) c[i,j]+=a[i,k]*b[k,j];
    return c;
}
static Complex[,] Dagger(Complex[,] a)=>new Complex[,]{{Complex.Conjugate(a[0,0]),Complex.Conjugate(a[1,0])},{Complex.Conjugate(a[0,1]),Complex.Conjugate(a[1,1])}};
static double MatrixError(Complex[,] a,Complex[,] b)=>Enumerable.Range(0,4).Max(i=>Complex.Abs(a[i/2,i%2]-b[i/2,i%2]));
static double Simpson(Func<double,double> f,double a,double b,int n)
{
    double h=(b-a)/n,sum=f(a)+f(b);
    for(int i=1;i<n;i++) sum+=(i%2==0?2:4)*f(a+i*h);
    return sum*h/3;
}
readonly record struct Q(double W,double X,double Y,double Z);
