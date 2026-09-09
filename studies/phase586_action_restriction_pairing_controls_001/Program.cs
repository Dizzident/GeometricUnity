using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root="studies/phase586_action_restriction_pairing_controls_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string Success="action-restriction-controls-pass-pairing-bridge-unresolved";
const string ContractId="phase586-a46-action-restriction-pairing-v1";
const string Source="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt";
const string Prior="studies/phase583_biconnection_convention_reconciliation_001/output/biconnection_convention_reconciliation_summary.json";
const string FixtureJson="""
{"amplitudes":[-2,-1,0,1,2],"couplings":[-2,0,2],"signs":[-1,1],"reference":[[1,0,1],[0,1,-1]],"relative":[[1,2,0],[-1,0,1]],"referenceCurl":[1,-1,2],"relativeCurl":[-2,1,1],"direction":[[0,1,1],[1,-1,0]],"directionCurl":[1,2,-1],"eta":[1,-1,1],"etaDerivative":[[0,1,0],[1,0,-1]],"step":0.0000152587890625,"exactTolerance":1e-12,"derivativeTolerance":2e-7,"nonzeroFloor":0.01,"meshExtent":1,"metric":"positive-euclidean","carrier":"R6: K(f)=(f,0), C(t1,t2)=(t1,t2)","linearMaps":[[[2,0],[0,1]],[[0,-1],[1,0]]],"fieldRedefinition":[[2,0],[0,3]],"hessian":[[2,1],[1,3]],"degreeAmplitudes":[0,1,2,3]}
""";
string[] forbidden=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged",
    "registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened",
    "o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] expectedPaths=[Root+"/Program.cs",Root+"/Phase586ActionRestrictionPairingControls.csproj",Source,Prior,
    "src/Gu.ReferenceCpu/CurvatureAssembler.cs","src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs",
    "src/Gu.ReferenceCpu/TrivialTorsionCpu.cs",Root+"/preregistration/core_source_manifest_v1.json","Directory.Build.props",Root+"/STUDY.md"];
string[] expectedIds=["program","project","primary-source","phase583-summary","curvature-core","shiab-core","torsion-core","core-source-manifest","build-props","study"];
using var document=JsonDocument.Parse(File.ReadAllBytes(ContractPath));
var contract=document.RootElement;
var bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(x=>new
{
    id=x.GetProperty("id").GetString()!,path=x.GetProperty("path").GetString()!,sha256=x.GetProperty("sha256").GetString()!
}).Select(x=>new {x.id,x.path,x.sha256,hashMatches=File.Exists(x.path)&&Sha(x.path)==x.sha256}).ToArray();
bool coreSourceTreeValid=false;
bool contractValid=contract.GetProperty("schemaVersion").GetInt32()==1 && contract.GetProperty("phase").GetInt32()==586
    && contract.GetProperty("contractId").GetString()==ContractId && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
    && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(
        ["invalid-or-drifted-input","known-answer-battery-failed","mathematical-control-failed",Success])
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
    && forbidden.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean() && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
bool exactBindingsValid=bindings.Length==expectedPaths.Length && bindings.Select(x=>x.id).Distinct().Count()==bindings.Length
    && bindings.Select(x=>x.path).Distinct().Count()==bindings.Length && bindings.All(x=>x.hashMatches)
    && expectedIds.Zip(expectedPaths).All(x=>bindings.Any(b=>b.id==x.First&&b.path==x.Second));
if(!contractValid||!exactBindingsValid) {Emit("invalid-or-drifted-input",new {knownAnswerPassed=false,controlsPassed=false});return;}
using var coreDocument=JsonDocument.Parse(File.ReadAllBytes(Root+"/preregistration/core_source_manifest_v1.json"));
var coreManifest=coreDocument.RootElement;
var livePaths=CorePaths();
var coreFiles=coreManifest.GetProperty("files").EnumerateArray().ToArray();
coreSourceTreeValid=coreManifest.GetProperty("schemaVersion").GetInt32()==1
    &&coreManifest.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
    &&coreFiles.Select(x=>x.GetProperty("path").GetString()).SequenceEqual(livePaths)
    &&coreFiles.All(x=>Sha(x.GetProperty("path").GetString()!)==x.GetProperty("sha256").GetString())
    &&TreeSha()==coreManifest.GetProperty("treeSha256").GetString();
if(!coreSourceTreeValid) {Emit("invalid-or-drifted-input",new {knownAnswerPassed=false,controlsPassed=false,coreSourceTreeValid});return;}
var fx=contract.GetProperty("fixtures");
double tol=fx.GetProperty("exactTolerance").GetDouble(),dtol=fx.GetProperty("derivativeTolerance").GetDouble(),step=fx.GetProperty("step").GetDouble(),floor=fx.GetProperty("nonzeroFloor").GetDouble();
double[] amplitudes=Vector(fx.GetProperty("amplitudes")),couplings=Vector(fx.GetProperty("couplings"));
bool knownAnswerPassed=Error(Cross([1,0,0],[0,1,0]),[0,0,1])==0
    && Error(Unskew(Comm(Skew([1,0,0]),Skew([0,1,0]))),[0,0,1])==0
    && Error(Eigen([2,0,0,3]),[2,3])==0
    && Error(Adjoint(Exp([0,0,0]),[1,2,3]),[1,2,3])==0;
if(!knownAnswerPassed) {Emit("known-answer-battery-failed",new {knownAnswerPassed,controlsPassed=false});return;}
using var upstream=JsonDocument.Parse(File.ReadAllBytes(Prior));
bool upstreamValid=upstream.RootElement.GetProperty("auditPassed").GetBoolean()
    && upstream.RootElement.GetProperty("verdictKind").GetString()=="printed-sign-conflict-proved-two-compatible-families-registered-map-unresolved"
    && upstream.RootElement.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
string source=File.ReadAllText(Source);
bool sourceAnchorsPresent=new[]{"(9.2)","(9.7)","(9.11)","(12.6)","Without the quadratic potential term"}.All(source.Contains);
if(!upstreamValid||!sourceAnchorsPresent) {Emit("invalid-or-drifted-input",new {knownAnswerPassed,controlsPassed=false,upstreamValid,sourceAnchorsPresent});return;}

// Actual registered operator, on a declared OPEN minimal mesh (not the sampled periodic ensemble).
var mesh=SimplicialMeshGenerator.CreateUniform4D(fx.GetProperty("meshExtent").GetInt32());
var algebra=LieAlgebraFactory.CreateSu2WithTracePairing();
var member=new EinsteinianShiabFamilyMember {Phi1=InvariantElementSpec.Sd2,Phi2=InvariantElementSpec.Id0,EinsteinCoefficient=0.5,EpsilonMode="independent-theta"};
var op=new EinsteinianShiabOperator(mesh,algebra,member);
var mass=new CpuMassMatrix(mesh,algebra);
var theta=new double[mesh.VertexCount*algebra.Dimension];
var commutingRows=new List<object>(); bool commutingPassed=true;
foreach(double t in amplitudes)
{
    var omega=new double[mesh.EdgeCount*algebra.Dimension];
    for(int e=0;e<mesh.EdgeCount;e++) omega[e*algebra.Dimension]=t*(mesh.GetVertexCoordinates(mesh.Edges[e][1])[0]-mesh.GetVertexCoordinates(mesh.Edges[e][0])[0]);
    var curvature=CurvatureAssembler.Assemble(new ConnectionField(mesh,algebra,omega));
    var eval=op.ComputeJointGradient(omega,theta,mass);
    double curvatureNorm=Norm(curvature.Coefficients),gradientNorm=Norm(eval.GradOmega)+Norm(eval.GradTheta),coefficientNorm=Norm(omega);
    foreach(double kappa in couplings)
    {
        // K and C are maps into the same declared R6 carrier. No edge/face addition occurs.
        double[] toyT=[t,0,0,0,0,0],toyU=Scale(kappa,toyT);
        double toyObjective=Dot(toyU,toyU)/2,expected=kappa*kappa*t*t/2;
        bool rowPassed=curvatureNorm<=tol&&System.Math.Abs(eval.Objective)<=tol&&gradientNorm<=tol
            &&System.Math.Abs(toyObjective-expected)<=tol&&(t==0||coefficientNorm>floor);
        commutingPassed&=rowPassed;
        commutingRows.Add(new {t,kappa,coefficientNorm,curvatureNorm,registeredObjective=eval.Objective,gradientNorm,toyObjective,expected,rowPassed});
    }
}

double[][] a0=Rows(fx.GetProperty("reference")),relative=Rows(fx.GetProperty("relative")),v=Rows(fx.GetProperty("direction"));
double[] curl0=Vector(fx.GetProperty("referenceCurl")),curlT=Vector(fx.GetProperty("relativeCurl")),curlV=Vector(fx.GetProperty("directionCurl"));
double[] eta=Vector(fx.GetProperty("eta")); double[][] deta=Rows(fx.GetProperty("etaDerivative"));
var a=PairAdd(a0,relative);
// Independent direct matrix curvature versus vector-component expansion at an affine first jet.
double[] direct=MatrixCurvature(a,Add(curl0,curlT));
double[] f0=MatrixCurvature(a0,curl0);
double[] d0T=Add(curlT,Add(Cross(a0[0],relative[1]),Cross(relative[0],a0[1])));
double[] expanded=Add(f0,Add(d0T,Cross(relative[0],relative[1])));
double referenceIdentityError=Error(direct,expanded),referenceOmissionDefect=Error(direct,MatrixCurvature(relative,curlT));
var variationRows=new List<object>();bool variationsPassed=referenceIdentityError<=tol&&referenceOmissionDefect>floor&&Norm(f0)>floor;
foreach(int c in fx.GetProperty("signs").EnumerateArray().Select(x=>x.GetInt32())) foreach(double kappa in couplings)
{
    // Freeze A=A0+c*p with p=c*T on epsilon=I. eta=epsilon^-1 delta epsilon.
    var dv=v.Select(x=>Scale(c,x)).ToArray();double[] dcurl=Scale(c,curlV);
    double[] df=Add(dcurl,Add(Cross(dv[0],a[1]),Cross(a[0],dv[1])));
    double[] u=Residual(direct,relative,kappa);
    double[] duP=Residual(df,dv,kappa);
    double[] up=Residual(MatrixCurvature(PairAdd(a,PairScale(step,dv)),Add(Add(curl0,curlT),Scale(step,dcurl))),PairAdd(relative,PairScale(step,dv)),kappa);
    double[] um=Residual(MatrixCurvature(PairAdd(a,PairScale(-step,dv)),Add(Add(curl0,curlT),Scale(-step,dcurl))),PairAdd(relative,PairScale(-step,dv)),kappa);
    double pResidualDerivativeError=Error(Scale(1/(2*step),Sub(up,um)),duP);
    double pActionDerivativeError=System.Math.Abs((Dot(up,up)-Dot(um,um))/(4*step)-Dot(u,duP));
    var dB=new[]{Add(deta[0],Cross(a0[0],eta)),Add(deta[1],Cross(a0[1],eta))};
    double[] duE=Scale(-kappa,dB.SelectMany(x=>x).ToArray());
    double[] EpsilonResidual(double s)
    {
        var e=Exp(Scale(s,eta));
        var b=new[]{Add(Adjoint(Transpose(e),a0[0]),Scale(s,deta[0])),Add(Adjoint(Transpose(e),a0[1]),Scale(s,deta[1]))};
        return Residual(direct,new[]{Sub(a[0],b[0]),Sub(a[1],b[1])},kappa);
    }
    double[] ep=EpsilonResidual(step),em=EpsilonResidual(-step);
    double epsilonResidualDerivativeError=Error(Scale(1/(2*step),Sub(ep,em)),duE);
    double epsilonActionDerivativeError=System.Math.Abs((Dot(ep,ep)-Dot(em,em))/(4*step)-Dot(u,duE));
    // Vary the reference itself with T fixed: both A and B vary, so delta T=0.
    double[] rp=Residual(MatrixCurvature(PairAdd(a,PairScale(step,dv)),Add(Add(curl0,curlT),Scale(step,dcurl))),relative,kappa);
    double[] rm=Residual(MatrixCurvature(PairAdd(a,PairScale(-step,dv)),Add(Add(curl0,curlT),Scale(-step,dcurl))),relative,kappa);
    double[] duReference=Residual(df,PairScale(0,dv),kappa);
    double referenceResidualDerivativeError=Error(Scale(1/(2*step),Sub(rp,rm)),duReference);
    double referenceActionDerivativeError=System.Math.Abs((Dot(rp,rp)-Dot(rm,rm))/(4*step)-Dot(u,duReference));
    bool rowPassed=pResidualDerivativeError<=dtol&&pActionDerivativeError<=dtol&&epsilonResidualDerivativeError<=dtol&&epsilonActionDerivativeError<=dtol
        &&referenceResidualDerivativeError<=dtol&&referenceActionDerivativeError<=dtol;
    variationsPassed&=rowPassed;
    variationRows.Add(new {c,kappa,pResidualDerivativeError,pActionDerivativeError,epsilonResidualDerivativeError,epsilonActionDerivativeError,referenceResidualDerivativeError,referenceActionDerivativeError,rowPassed});
}

// Residual maps L are illustrative, field-independent maps; the actual source L remains unspecified.
double[][][] maps=fx.GetProperty("linearMaps").EnumerateArray().Select(Rows).ToArray();
var pairingRows=new List<object>();bool pairingPassed=true;
for(int m=0;m<maps.Length;m++) foreach(double q in new[]{0.0,1.0})
{
    double[] l=maps[m].SelectMany(x=>x).ToArray();
    double[] w=Product2(Transpose2(l),l),y=[q*q,q],j=[2*q,1],ySecond=[2,0];
    double sourceHessian=Dot(j,Apply2(w,j))+Dot(y,Apply2(w,ySecond));
    double registeredHessian=Dot(j,j)+Dot(y,ySecond);
    // Quadratic-form action sampled exactly on a dyadic stencil, independently of derivative formula.
    double Action(double z) {double[] ly=Apply2(l,[z*z,z]);return Dot(ly,ly)/2;}
    double finiteHessian=(Action(q+step)-2*Action(q)+Action(q-step))/(step*step);
    bool isometry=m==1;
    bool rowPassed=System.Math.Abs(finiteHessian-sourceHessian)<=dtol
        &&(!isometry||System.Math.Abs(sourceHessian-registeredHessian)<=tol)
        &&(isometry||q==0||System.Math.Abs(sourceHessian-registeredHessian)>floor);
    pairingPassed&=rowPassed;
    pairingRows.Add(new {mapIndex=m,q,isometry,sourceHessian,registeredHessian,finiteHessian,rowPassed});
}
double[] jLinear=[1,0,0,1],nonIso=maps[0].SelectMany(x=>x).ToArray(),iso=maps[1].SelectMany(x=>x).ToArray();
double[] referenceEigen=Eigen(Product2(Transpose2(jLinear),jLinear));
double[] nonIsoEigen=Eigen(Product2(Transpose2(nonIso),nonIso)),isoEigen=Eigen(Product2(Transpose2(iso),iso));
double[] h=Rows(fx.GetProperty("hessian")).SelectMany(x=>x).ToArray(),r=Rows(fx.GetProperty("fieldRedefinition")).SelectMany(x=>x).ToArray();
double[] transformedH=Product2(Transpose2(r),Product2(h,r)),kinetic=Product2(Transpose2(r),r);
double[] inverseSqrt=[1/System.Math.Sqrt(kinetic[0]),0,0,1/System.Math.Sqrt(kinetic[3])];
double[] originalEigen=Eigen(h),ordinaryTransformedEigen=Eigen(transformedH),generalizedEigen=Eigen(Product2(inverseSqrt,Product2(transformedH,inverseSqrt)));
bool spectrumControlsPassed=Error(nonIsoEigen,referenceEigen)>floor&&Error(isoEigen,referenceEigen)<=tol
    &&Error(originalEigen,ordinaryTransformedEigen)>floor&&Error(originalEigen,generalizedEigen)<=tol;
// y(t)=(t^2,t), L=(0,1): the leading vector is annihilated and degree falls 4->2.
var degreeRows=Vector(fx.GetProperty("degreeAmplitudes")).Select(t=>new {t,registeredObjective=(t*t*t*t+t*t)/2,mappedObjective=t*t/2}).ToArray();
double fourthDifference=degreeRows[2].registeredObjective-4*degreeRows[1].registeredObjective+3*degreeRows[0].registeredObjective;
// For samples 0,1,2, this combination is 12*a4 for an even quartic a4*t^4+a2*t^2+a0.
double mappedFourthDifference=degreeRows[2].mappedObjective-4*degreeRows[1].mappedObjective+3*degreeRows[0].mappedObjective;
bool degreeControlsPassed=System.Math.Abs(fourthDifference-6)<=tol&&System.Math.Abs(mappedFourthDifference)<=tol
    &&System.Math.Abs(degreeRows[3].registeredObjective-45)<=tol&&System.Math.Abs(degreeRows[3].mappedObjective-4.5)<=tol;
bool controlsPassed=commutingPassed&&variationsPassed&&pairingPassed&&spectrumControlsPassed&&degreeControlsPassed;
Emit(controlsPassed?Success:"mathematical-control-failed",new
{
    knownAnswerPassed,controlsPassed,upstreamValid,sourceAnchorsPresent,
    actionRestriction=new {commutingPassed,commutingRows,registeredFixture="open n=1 mesh, theta=0, sd2/id0, no periodic ensemble equivalence",torsionContributionOmissionIsNotRelativeFieldConstraint=true,sourceDiscussesZeroCoupling=true,sourceCouplingSelected=false},
    referenceVariation=new {variationsPassed,referenceIdentityError,referenceOmissionDefect,referenceCurvatureNorm=Norm(f0),variationRows,carrier="R6: K(F12)=(F12,0); C(T1,T2)=(T1,T2)",carrierIsIllustrative=true,epsilonDependentSourceKNotInstantiated=true},
    quadraticForms=new {pairingPassed,pairingRows,spectrumControlsPassed,referenceEigen,nonIsoEigen,isoEigen,originalEigen,ordinaryTransformedEigen,generalizedEigen,degreeControlsPassed,degreeRows,fourthDifference,mappedFourthDifference,sourceLinearMapSpecified=false,physicalPoleExtractionPerformed=false},
    nextRequirement="Specify actual source carrier maps and pulled-back pairing, reference/epsilon dependence, and compatible first-order variational identities before any action or spectral bridge."
});

void Emit(string verdict,object evidence)
{
    var result=new {schemaVersion=1,phase=586,phaseId="phase586-action-restriction-pairing-controls",contractId=ContractId,
        contractSha256=Sha(ContractPath),contractValid,exactBindingsValid,coreSourceTreeValid,bindingCount=bindings.Length,bindings,
        verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
        authorityFirewalls=forbidden.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
    string json=JsonSerializer.Serialize(result,new JsonSerializerOptions {WriteIndented=true})+Environment.NewLine;
    Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/action_restriction_pairing_controls.json",json);
    File.WriteAllText(Root+"/output/action_restriction_pairing_controls_summary.json",json);
    Console.WriteLine($"Phase586 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string TreeSha()
{
    string text=string.Concat(CorePaths().Select(p=>p+" "+Sha(p)+"\n"));
    return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
}
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
static double[] Vector(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetDouble()).ToArray();
static double[][] Rows(JsonElement e)=>e.EnumerateArray().Select(Vector).ToArray();
static double[] Add(double[] a,double[] b)=>a.Zip(b,(x,y)=>x+y).ToArray();
static double[] Sub(double[] a,double[] b)=>a.Zip(b,(x,y)=>x-y).ToArray();
static double[] Scale(double s,double[] a)=>a.Select(x=>s*x).ToArray();
static double Dot(double[] a,double[] b)=>a.Zip(b,(x,y)=>x*y).Sum();
static double Norm(double[] a)=>System.Math.Sqrt(Dot(a,a));
static double Error(double[] a,double[] b)=>Norm(Sub(a,b));
static double[] Cross(double[] a,double[] b)=>[a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]];
static double[][] PairAdd(double[][] a,double[][] b)=>[Add(a[0],b[0]),Add(a[1],b[1])];
static double[][] PairScale(double s,double[][] a)=>[Scale(s,a[0]),Scale(s,a[1])];
static double[,] Skew(double[] a)=>new double[,]{{0,-a[2],a[1]},{a[2],0,-a[0]},{-a[1],a[0],0}};
static double[] Unskew(double[,] a)=>[a[2,1],a[0,2],a[1,0]];
static double[,] Multiply(double[,] a,double[,] b) {var o=new double[3,3];for(int i=0;i<3;i++)for(int j=0;j<3;j++)for(int k=0;k<3;k++)o[i,j]+=a[i,k]*b[k,j];return o;}
static double[,] Transpose(double[,] a) {var o=new double[3,3];for(int i=0;i<3;i++)for(int j=0;j<3;j++)o[i,j]=a[j,i];return o;}
static double[,] Comm(double[,] a,double[,] b) {var ab=Multiply(a,b);var ba=Multiply(b,a);for(int i=0;i<3;i++)for(int j=0;j<3;j++)ab[i,j]-=ba[i,j];return ab;}
static double[] Adjoint(double[,] e,double[] v)=>Unskew(Multiply(Multiply(e,Skew(v)),Transpose(e)));
static double[,] Exp(double[] v)
{
    double n=Norm(v);var k=Skew(v);var kk=Multiply(k,k);var o=new double[3,3];
    double s=n==0?1:System.Math.Sin(n)/n,c=n==0?0.5:2*System.Math.Pow(System.Math.Sin(n/2)/n,2);
    for(int i=0;i<3;i++)for(int j=0;j<3;j++)o[i,j]=(i==j?1:0)+s*k[i,j]+c*kk[i,j];return o;
}
static double[] MatrixCurvature(double[][] a,double[] curl)=>Add(curl,Unskew(Comm(Skew(a[0]),Skew(a[1]))));
static double[] Residual(double[] f,double[][] t,double kappa)=>Add([f[0],f[1],f[2],0,0,0],Scale(kappa,t.SelectMany(x=>x).ToArray()));
static double[] Product2(double[] a,double[] b)=>[a[0]*b[0]+a[1]*b[2],a[0]*b[1]+a[1]*b[3],a[2]*b[0]+a[3]*b[2],a[2]*b[1]+a[3]*b[3]];
static double[] Transpose2(double[] a)=>[a[0],a[2],a[1],a[3]];
static double[] Apply2(double[] a,double[] v)=>[a[0]*v[0]+a[1]*v[1],a[2]*v[0]+a[3]*v[1]];
static double[] Eigen(double[] a) {double d=System.Math.Sqrt((a[0]-a[3])*(a[0]-a[3])+4*a[1]*a[2]);return [(a[0]+a[3]-d)/2,(a[0]+a[3]+d)/2];}
