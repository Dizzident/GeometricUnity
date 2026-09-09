using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Gu.Geometry;
using Gu.ReferenceCpu;

const string Root="studies/phase588_fixed_domain_action_force_consistency_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ContractId="phase588-a47-fixed-domain-action-force-v1";
const string Success="fixed-domain-action-force-controls-pass-induced-pairing-scoped";
const string FixtureJson="""
{
 "sizes":[1,2,4],"members":["identity","sd2-id0-half"],"theta":"zero-fixed","vertexFaceRule":"IncidentAverage",
 "massWeights":"default-one","dictionary":{"omegaSign":-1,"physicalResidualSign":-1,"physicalDirectionalSign":-1},
 "constantForms":"six basis vectors followed by fifteen lexicographic pair sums in [01,02,03,12,13,23]",
 "constantCount":21,"directBasisCount":6,"crossDirectionsAtN1":6,"anchors":["zero","constant-flat-Jx-dx0"],
 "homothetyScales":[1,0.5,0.25,0.125],"homothetyForms":[0,6],"homothetyProbe":"face-major component ((index mod 7)-3)/8",
 "profiles":[
  {"id":"affine-witness","constant":[[1,0,0],[0,1,0],[0,0,0],[0,0,0]],"derivatives":[{"mu":1,"nu":0,"vector":[0,0,1]}]},
  {"id":"affine-four-component","constant":[[1,0,0],[0,1,0],[0.5,0.5,0],[0,0,0.5]],"derivatives":[{"mu":1,"nu":0,"vector":[0,0,1]},{"mu":2,"nu":3,"vector":[0.25,0,0]},{"mu":3,"nu":2,"vector":[0,0.25,0]}]}
 ],
 "directions":["self",{"id":"independent-affine","constant":[[0,1,0],[0,0,0],[0,0,0.5],[0,0,0]],"derivatives":[{"mu":1,"nu":0,"vector":[1,0,0]},{"mu":3,"nu":2,"vector":[0.25,0,0]}]}],
 "directionalSteps":[0.00390625,0.0009765625,0.000244140625],"richardsonWeights":[16,-1,15],
 "exactTolerance":0,"scaledTolerance":1e-10,"forceTolerance":1e-8,"zeroTolerance":1e-12,"nonzeroFloor":1e-8,
 "bound":{"cellRowL1":132,"edgeFieldFactor":1,"localCurvatureFactor":3,"localQuadraticFactor":1.5,"globalFaceFactor":110,"bulkTypeCount":50,"boundaryFaceFactor":60},
 "counts":{"constantRows":126,"directResidualRows":36,"crossRows":252,"anchorRows":12,"homothetyRows":16,"smoothRows":24,"richardsonPairs":48,"largestVertices":625,"largestEdges":5936,"largestFaces":16064,"largestCells":6144},
 "resource":{"estimatedCpuSeconds":120,"maximumEstimatedCpuSeconds":180,"estimatedPeakBytes":268435456,"maximumEstimatedPeakBytes":536870912,"maximumN":4},
 "coreFileCount":726
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
 "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
 "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","analytic-force-parity-failed",
 "exact-action-pairing-control-failed","smooth-global-bound-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["project"]=Root+"/Phase588FixedDomainActionForceConsistency.csproj",
 ["study"]=Root+"/STUDY.md",["core-source-manifest"]=Root+"/preregistration/core_source_manifest_v1.json",["build-props"]="Directory.Build.props",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["phase583-summary"]="studies/phase583_biconnection_convention_reconciliation_001/output/biconnection_convention_reconciliation_summary.json",
 ["phase583-contract"]="studies/phase583_biconnection_convention_reconciliation_001/preregistration/contract_v1.json",
 ["phase584-summary"]="studies/phase584_signed_spatial_curvature_consistency_001/output/signed_spatial_curvature_consistency_summary.json",
 ["phase584-contract"]="studies/phase584_signed_spatial_curvature_consistency_001/preregistration/contract_v1.json",
 ["phase586-summary"]="studies/phase586_action_restriction_pairing_controls_001/output/action_restriction_pairing_controls_summary.json",
 ["phase586-contract"]="studies/phase586_action_restriction_pairing_controls_001/preregistration/contract_v1.json"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;
Binding[] bindings=[];JsonElement contract=default;
try
{
 using var doc=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=doc.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==588
  &&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
  &&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()
  &&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
  &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
  &&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(x=>new Binding(x.GetProperty("id").GetString()!,x.GetProperty("path").GetString()!,x.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(x=>x.id).Distinct().Count()==paths.Count
  &&bindings.Select(x=>x.path).Distinct().Count()==paths.Count&&bindings.All(x=>paths.TryGetValue(x.id,out var p)&&p==x.path&&x.hashMatches);
 if(contractValid&&exactBindingsValid)
 {
  using var cm=JsonDocument.Parse(File.ReadAllBytes(paths["core-source-manifest"]));var m=cm.RootElement;
  string[] live=CorePaths();var entries=m.GetProperty("files").EnumerateArray().ToArray();
  coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
   &&live.Length==contract.GetProperty("fixtures").GetProperty("coreFileCount").GetInt32()
   &&entries.Select(x=>x.GetProperty("path").GetString()).SequenceEqual(live)
   &&entries.All(x=>Sha(x.GetProperty("path").GetString()!)==x.GetProperty("sha256").GetString())
   &&HashText(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();
 }
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false});return;}
var fx=contract.GetProperty("fixtures");double tol=fx.GetProperty("scaledTolerance").GetDouble(),ftol=fx.GetProperty("forceTolerance").GetDouble();
bool upstreamValid=new[]{583,584,586}.All(n=>{using var d=JsonDocument.Parse(File.ReadAllBytes(paths[$"phase{n}-summary"]));return d.RootElement.GetProperty("auditPassed").GetBoolean()&&d.RootElement.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;});
if(!upstreamValid){Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false,upstreamValid});return;}
var types=new List<FaceType>();
for(int u=1;u<16;u++)for(int v=1;v<16;v++)if((u&v)==0)types.Add(new(u,v,H.Wedge(H.Mask(u),H.Mask(v)),System.Numerics.BitOperations.PopCount((uint)(u|v))));
double[,] incidence=new double[4,6];for(int k=0;k<6;k++){incidence[H.Pairs[k].i,k]=-1;incidence[H.Pairs[k].j,k]=1;}
double[,] btb=H.Mul(H.Tr(incidence),incidence),pplus=H.Projector(),g=H.Gram(0,btb);
double metricIdentityError=0;foreach(int n in fx.GetProperty("sizes").EnumerateArray().Select(x=>x.GetInt32()))
{
 double[,] census=new double[6,6];foreach(var t in types)for(int i=0;i<6;i++)for(int j=0;j<6;j++)census[i,j]+=0.25*System.Math.Pow(1+1.0/n,4-t.m)*t.b[i]*t.b[j];
 metricIdentityError=System.Math.Max(metricIdentityError,H.MError(census,H.Gram(1.0/n,btb)));
 metricIdentityError=System.Math.Max(metricIdentityError,H.MError(H.Mul(pplus,H.Mul(census,pplus)),H.MScale(pplus,4.5+3.0/n+0.5/(n*n))));
}
double[] electric=[1,1,1,0,0,0],magnetic=[1,-1,0,1,0,0];
bool eigenspacesPassed=H.Error(H.MV(g,electric),H.Scale(electric,7.5))==0&&H.Error(H.MV(g,magnetic),H.Scale(magnetic,1.5))==0;
var testPoly=Poly.Coordinate(0)*Poly.Coordinate(1);
double momentError=System.Math.Abs(testPoly.CubeIntegral()-0.25)+System.Math.Abs(testPoly.TriangleAverage([new double[4],new double[]{1,0,0,0},new double[]{0,1,0,0}])-1.0/12);
bool knownAnswerPassed=types.Count==50&&metricIdentityError==0&&eigenspacesPassed&&momentError<=tol
 &&H.MError(H.Mul(pplus,pplus),pplus)==0;
if(!knownAnswerPassed){Emit(precedence[1],new {knownAnswerPassed,controlsPassed=false,metricIdentityError,eigenspacesPassed,momentError});return;}

var constantForms=Enumerable.Range(0,6).Select(H.Basis).ToList();
for(int i=0;i<6;i++)for(int j=i+1;j<6;j++)constantForms.Add(H.Add(H.Basis(i),H.Basis(j)));
var constantRows=new List<object>();var directRows=new List<object>();var crossRows=new List<object>();var anchorRows=new List<object>();var smoothRows=new List<object>();var topologyRows=new List<object>();
bool exactPassed=true,forcePassed=true,smoothPassed=true,topologyPassed=true;
int richardsonPairs=0;
foreach(int n in fx.GetProperty("sizes").EnumerateArray().Select(x=>x.GetInt32()))
{
 double h=1.0/n;var mesh=H.Mesh(n,h);double[][] coordinates=H.Coordinates(mesh);
 var counts=types.ToDictionary(t=>(t.u,t.v),_=>0);
 foreach(var face in mesh.Faces)
 {
  int u=H.DisplacementMask(coordinates[face[0]],coordinates[face[1]],h),v=H.DisplacementMask(coordinates[face[1]],coordinates[face[2]],h);
  if(!counts.ContainsKey((u,v))){topologyPassed=false;continue;}counts[(u,v)]++;
 }
 bool censusPassed=types.All(t=>counts[(t.u,t.v)]==System.Math.Pow(n,t.m)*System.Math.Pow(n+1,4-t.m));
 bool countPassed=mesh.VertexCount==System.Math.Pow(n+1,4)&&mesh.EdgeCount==15*n*n*n*n+28*n*n*n+18*n*n+4*n
  &&mesh.FaceCount==50*n*n*n*n+48*n*n*n+12*n*n&&mesh.CellCount==24*n*n*n*n;
 topologyPassed&=censusPassed&&countPassed;
 topologyRows.Add(new {n,mesh.VertexCount,mesh.EdgeCount,mesh.FaceCount,mesh.CellCount,censusPassed,countPassed,physicalDomain="[0,1]^4"});
 foreach(string member in fx.GetProperty("members").EnumerateArray().Select(x=>x.GetString()!))
 {
  var engine=new Engine(mesh,member);double[,] r=H.MemberMatrix(member);double[,] qform=H.Mul(H.Tr(r),H.Mul(H.Gram(h,btb),r));
  for(int index=0;index<constantForms.Count;index++)
  {
   var form=constantForms[index];var a=Affine.Abelian(form);double[] edges=a.Edges(mesh);
   var ev=engine.Gradient(H.Scale(edges,-1));double predicted=0.5*H.Dot(form,H.MV(qform,form));
   double physicalDerivative=-H.Dot(ev.GradOmega,edges),derivativePrediction=2*predicted;
   double actionError=H.ScaledError(ev.Objective,predicted),derivativeError=H.ScaledError(physicalDerivative,derivativePrediction);
   bool decoyRejected=System.Math.Abs(derivativePrediction)<=fx.GetProperty("nonzeroFloor").GetDouble()
    ||System.Math.Abs(-physicalDerivative-derivativePrediction)>fx.GetProperty("nonzeroFloor").GetDouble();
   bool rowPassed=actionError<=tol&&derivativeError<=tol&&decoyRejected;exactPassed&=rowPassed;
   constantRows.Add(new {n,member,index,predicted,actual=ev.Objective,physicalDerivative,derivativePrediction,actionError,derivativeError,decoyRejected,rowPassed});
   if(index<6)
   {
    double[] independent=H.ConstantFaces(mesh,form),expected=H.ConstantFaces(mesh,H.MV(r,form)),actual=engine.Contract(independent);
    double[] assembledPhysical=H.Scale(engine.Contract(engine.Curvature(H.Scale(edges,-1))),-1);
    double residualError=H.MaxError(actual,expected),assembledResidualError=H.MaxError(assembledPhysical,expected);
    bool directPassed=residualError<=tol&&assembledResidualError<=tol;exactPassed&=directPassed;
    directRows.Add(new {n,member,index,residualError,assembledResidualError,directPassed});
   }
   if(n==1)for(int direction=0;direction<6;direction++)
   {
    double[] v=Affine.Abelian(H.Basis(direction)).Edges(mesh);double observed=-H.Dot(ev.GradOmega,v),expected=H.Dot(form,H.MV(qform,H.Basis(direction)));
    double error=H.ScaledError(observed,expected);bool passed=error<=tol;exactPassed&=passed;
    crossRows.Add(new {member,index,direction,observed,expected,error,passed});
   }
  }
  foreach(string anchor in fx.GetProperty("anchors").EnumerateArray().Select(x=>x.GetString()!))
  {
   var a=new Affine();if(anchor!="zero")a.c[0,0]=1;
   double[] edges=a.Edges(mesh);var ev=engine.Gradient(H.Scale(edges,-1));double curvature=H.Norm(engine.Curvature(H.Scale(edges,-1)));
   double gradient=H.Norm(ev.GradOmega)+H.Norm(ev.GradTheta);
   bool passed=curvature<=fx.GetProperty("zeroTolerance").GetDouble()&&System.Math.Abs(ev.Objective)<=fx.GetProperty("zeroTolerance").GetDouble()&&gradient<=fx.GetProperty("zeroTolerance").GetDouble();
   exactPassed&=passed;anchorRows.Add(new {n,member,anchor,curvature,objective=ev.Objective,gradient,passed});
  }
  foreach(var profile in fx.GetProperty("profiles").EnumerateArray())
  {
   var a=Affine.Read(profile);var f=a.Curvature();double[] edges=a.Edges(mesh),omega=H.Scale(edges,-1);var ev=engine.Gradient(omega);
   double[] oracleF=H.PolynomialFaces(mesh,f),oracleU=engine.Contract(oracleF),actualU=H.Scale(engine.Contract(engine.Curvature(omega)),-1);
   double oracleS=0.5*H.Dot(oracleU,oracleU),limitS=0.5*H.IntegratedPairing(f,f,H.Mul(H.Tr(r),H.Mul(g,r)));
   foreach(var directionSpec in fx.GetProperty("directions").EnumerateArray())
   {
    var v=directionSpec.ValueKind==JsonValueKind.String?a:Affine.Read(directionSpec);
    string direction=directionSpec.ValueKind==JsonValueKind.String?"self":directionSpec.GetProperty("id").GetString()!;
    var df=a.Variation(v);double[] vedges=v.Edges(mesh),oracleDF=H.PolynomialFaces(mesh,df),oracleDU=engine.Contract(oracleDF);
    double observedD=-H.Dot(ev.GradOmega,vedges),oracleD=H.Dot(oracleU,oracleDU);
    double limitD=H.IntegratedPairing(f,df,H.Mul(H.Tr(r),H.Mul(g,r)));
    // These coefficients use only fixture norms, proved K and incidence constants; no measured error enters.
    var bounds=H.Bounds(a,v,r,fx.GetProperty("bound"));
    double actionError=System.Math.Abs(ev.Objective-limitS),directionError=System.Math.Abs(observedD-limitD);
    double[] anchoredF=H.AnchoredFaces(mesh,f,r),anchoredDF=H.AnchoredFaces(mesh,df,r);
    double localResidual=H.MaxLieL1(H.Sub(actualU,anchoredF));
    // Differentiate the curvature polynomial by exact central polarization at unit amplitude.
    // Quadratic curvature means this is exact in real arithmetic, independent of the gradient reverse pass.
    double[] dfRegistered=H.Scale(H.Sub(engine.Curvature(H.Scale(H.Add(edges,vedges),-1)),engine.Curvature(H.Scale(H.Sub(edges,vedges),-1))),-0.5);
    double[] actualDU=engine.Contract(dfRegistered);double localDirectionalResidual=H.MaxLieL1(H.Sub(actualDU,anchoredDF));
    double directDerivative=H.Dot(actualU,actualDU),directDerivativeError=H.ScaledError(directDerivative,observedD);
    var derivatives=new List<double>();
    foreach(double step in H.Vector(fx.GetProperty("directionalSteps")))
    {
     double plus=engine.Objective(H.Scale(H.Add(edges,H.Scale(vedges,step)),-1)),minus=engine.Objective(H.Scale(H.Sub(edges,H.Scale(vedges,step)),-1));
     derivatives.Add((plus-minus)/(2*step));
    }
    var extrapolated=new List<double>();var richardsonErrors=new List<double>();
    for(int i=0;i<2;i++){double d=(16*derivatives[i+1]-derivatives[i])/15;extrapolated.Add(d);richardsonErrors.Add(H.ScaledError(d,observedD));richardsonPairs++;}
    bool parityPassed=directDerivativeError<=ftol&&richardsonErrors.All(e=>e<=ftol);forcePassed&=parityPassed;
    bool boundPassed=actionError<=h*bounds.action+tol&&directionError<=h*bounds.direction+tol
     &&localResidual<=h*h*h*bounds.residual+tol&&localDirectionalResidual<=h*h*h*bounds.residualDirection+tol;
    smoothPassed&=boundPassed;
    smoothRows.Add(new {n,h,member,profile=profile.GetProperty("id").GetString(),direction,actualAction=ev.Objective,oracleS,limitS,observedD,oracleD,limitD,
     actionError,directionError,actionBound=h*bounds.action,directionBound=h*bounds.direction,localResidual,localDirectionalResidual,
     localBound=h*h*h*bounds.residual,localDirectionalBound=h*h*h*bounds.residualDirection,boundCoefficients=bounds,
     curvatureInsertionActionError=System.Math.Abs(ev.Objective-oracleS),curvatureInsertionDirectionalError=System.Math.Abs(observedD-oracleD),
     directDerivativeError,centralDerivatives=derivatives,extrapolated,richardsonErrors,parityPassed,boundPassed});
   }
  }
 }
}

var homothetyRows=new List<object>();double maxHomothetyMapError=0;
foreach(string member in fx.GetProperty("members").EnumerateArray().Select(x=>x.GetString()!))
{
 var referenceMesh=H.Mesh(1,1);var referenceEngine=new Engine(referenceMesh,member);
 double[] probe=Enumerable.Range(0,referenceMesh.FaceCount*3).Select(i=>((i%7)-3)/8.0).ToArray();var referenceProbe=referenceEngine.Contract(probe);
 foreach(double scale in H.Vector(fx.GetProperty("homothetyScales")))
 {
  var mesh=H.Mesh(1,scale);var engine=new Engine(mesh,member);double mapError=H.MaxError(referenceProbe,engine.Contract(probe));
  maxHomothetyMapError=System.Math.Max(maxHomothetyMapError,mapError);exactPassed&=mapError<=tol;
  foreach(int index in fx.GetProperty("homothetyForms").EnumerateArray().Select(x=>x.GetInt32()))
  {
   var field=Affine.Abelian(constantForms[index]);double[] a=field.Edges(mesh);var ev=engine.Gradient(H.Scale(a,-1));
   double[,] r=H.MemberMatrix(member);double referenceAction=0.5*H.Dot(constantForms[index],H.MV(H.Mul(H.Tr(r),H.Mul(H.Gram(1,btb),r)),constantForms[index]));
   double expected=System.Math.Pow(scale,4)*referenceAction,derivative=-H.Dot(ev.GradOmega,a);
   bool passed=H.ScaledError(ev.Objective,expected)<=tol&&H.ScaledError(derivative,2*expected)<=tol&&mesh.CellCount==24&&mesh.FaceCount==110;
   exactPassed&=passed;homothetyRows.Add(new {member,index,scale,domainVolume=System.Math.Pow(scale,4),mesh.CellCount,mesh.FaceCount,
    objective=ev.Objective,expected,derivative,mapError,passed,isFixedDomainRefinement=false});
  }
 }
}
var countSpec=fx.GetProperty("counts");
bool countControlsPassed=constantRows.Count==countSpec.GetProperty("constantRows").GetInt32()&&directRows.Count==countSpec.GetProperty("directResidualRows").GetInt32()
 &&crossRows.Count==countSpec.GetProperty("crossRows").GetInt32()&&anchorRows.Count==countSpec.GetProperty("anchorRows").GetInt32()
 &&homothetyRows.Count==countSpec.GetProperty("homothetyRows").GetInt32()&&smoothRows.Count==countSpec.GetProperty("smoothRows").GetInt32()
 &&richardsonPairs==countSpec.GetProperty("richardsonPairs").GetInt32();
knownAnswerPassed&=topologyPassed&&countControlsPassed;
bool controlsPassed=knownAnswerPassed&&exactPassed&&forcePassed&&smoothPassed;
string verdict=!knownAnswerPassed?precedence[1]:!forcePassed?precedence[2]:!exactPassed?precedence[3]:!smoothPassed?precedence[4]:Success;
Emit(verdict,new {knownAnswerPassed,controlsPassed,upstreamValid,coreSourceTreeValid,metricIdentityError,eigenspacesPassed,momentError,topologyPassed,countControlsPassed,
 topologyRows,exactPassed,constantRows,directRows,crossRows,anchorRows,homothetyRows,maxHomothetyMapError,forcePassed,smoothPassed,smoothRows,richardsonPairs,
 pairing=new {identityFormAnisotropic=true,selfDualRestrictedScalarFactor=4.5,finiteSelfDualFactor="4.5+3h+0.5h^2",metric="G_h=(1.5+2h+0.5h^2)I+(1.5+0.5h)B^T B",sourcePairingIdentified=false},
 scope=new {fixedPhysicalDomainRefined=true,homothetySeparated=true,smoothDirectionalCovectorsOnly=true,pointwiseForceConvergenceClaimed=false,
  sourceActionBridgeEstablished=false,quantumLimitEstablished=false,oldSampleOutputsReinterpreted=false,thetaHeldFixed=true}});

void Emit(string verdict,object evidence)
{
 var result=new {schemaVersion=1,phase=588,phaseId="phase588-fixed-domain-action-force-consistency",contractId=ContractId,
  contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,bindingCount=bindings.Length,bindings,
  verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
  authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/fixed_domain_action_force_consistency.json",json);
 File.WriteAllText(Root+"/output/fixed_domain_action_force_consistency_summary.json",json);
 Console.WriteLine($"Phase588 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string HashText(string t)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(t))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
sealed record Binding(string id,string path,string sha256){public bool hashMatches=>File.Exists(path)&&Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()==sha256;}
sealed record FaceType(int u,int v,double[] b,int m);
sealed record BoundValues(double action,double direction,double residual,double residualDirection,double fieldNorm,double fieldDerivativeNorm,double directionNorm,double directionDerivativeNorm,double cellRowL1);

sealed class Engine
{
 public readonly SimplicialMesh mesh;readonly Gu.Math.LieAlgebra algebra=Gu.Math.LieAlgebraFactory.CreateSu2WithTracePairing();
 readonly EinsteinianShiabOperator op;readonly CpuMassMatrix mass;readonly double[] theta;readonly ConnectionField zero;
 public Engine(SimplicialMesh m,string member){mesh=m;op=new(m,algebra,new EinsteinianShiabFamilyMember{Phi1=member=="identity"?InvariantElementSpec.Id0:InvariantElementSpec.Sd2,
  Phi2=member=="identity"?InvariantElementSpec.None:InvariantElementSpec.Id0,EinsteinCoefficient=0.5,EpsilonMode="independent-theta",VertexFaceRule=VertexFaceRule.IncidentAverage});
  mass=new(m,algebra);theta=new double[m.VertexCount*3];zero=new(m,algebra);}
 public double[] Curvature(double[] omega)=>CurvatureAssembler.Assemble(new ConnectionField(mesh,algebra,omega)).Coefficients;
 public double[] Contract(double[] f)=>op.Evaluate(new CurvatureField(mesh,algebra,f).ToFieldTensor(),zero.ToFieldTensor(),null!,null!).Coefficients;
 public double Objective(double[] omega){var residual=op.Evaluate(new CurvatureField(mesh,algebra,Curvature(omega)).ToFieldTensor(),zero.ToFieldTensor(),null!,null!);return mass.EvaluateObjective(residual);}
 public (double Objective,double[] GradOmega,double[] GradTheta) Gradient(double[] omega)=>op.ComputeJointGradient(omega,theta,mass);
}

sealed class Affine
{
 public readonly double[,] c=new double[4,3];public readonly double[,,] d=new double[4,4,3];
 public static Affine Read(JsonElement e){var a=new Affine();int mu=0;foreach(var row in e.GetProperty("constant").EnumerateArray()){int k=0;foreach(var z in row.EnumerateArray())a.c[mu,k++]=z.GetDouble();mu++;}
  foreach(var row in e.GetProperty("derivatives").EnumerateArray()){int i=row.GetProperty("mu").GetInt32(),j=row.GetProperty("nu").GetInt32();var v=H.Vector(row.GetProperty("vector"));for(int k=0;k<3;k++)a.d[i,j,k]=v[k];}return a;}
 public static Affine Abelian(double[] f){var a=new Affine();for(int k=0;k<6;k++){var (i,j)=H.Pairs[k];a.d[i,j,2]=-0.5*f[k];a.d[j,i,2]=0.5*f[k];}return a;}
 public double[] Edges(SimplicialMesh mesh){var result=new double[mesh.EdgeCount*3];for(int e=0;e<mesh.EdgeCount;e++){var u=mesh.GetVertexCoordinates(mesh.Edges[e][0]);var v=mesh.GetVertexCoordinates(mesh.Edges[e][1]);
  for(int mu=0;mu<4;mu++)for(int k=0;k<3;k++){double value=c[mu,k];for(int nu=0;nu<4;nu++)value+=d[mu,nu,k]*(u[nu]+v[nu])/2;result[e*3+k]+=(v[mu]-u[mu])*value;}}return result;}
 public double HNorm=>d.Cast<double>().Sum(System.Math.Abs);public double ANorm=>c.Cast<double>().Sum(System.Math.Abs)+HNorm;
 public Poly[,] Polynomials(){var p=new Poly[4,3];for(int mu=0;mu<4;mu++)for(int k=0;k<3;k++){p[mu,k]=new(c[mu,k]);for(int nu=0;nu<4;nu++)p[mu,k]+=Poly.Coordinate(nu)*d[mu,nu,k];}return p;}
 public Poly[] Curvature(){var p=Polynomials();var result=new Poly[18];for(int pair=0;pair<6;pair++){var (i,j)=H.Pairs[pair];for(int k=0;k<3;k++){int u=(k+1)%3,v=(k+2)%3;result[3*pair+k]=new Poly(d[j,i,k]-d[i,j,k])+p[i,u]*p[j,v]-p[i,v]*p[j,u];}}return result;}
 public Poly[] Variation(Affine v){var p=Polynomials();var q=v.Polynomials();var result=new Poly[18];for(int pair=0;pair<6;pair++){var(i,j)=H.Pairs[pair];for(int k=0;k<3;k++){int u=(k+1)%3,w=(k+2)%3;
  result[3*pair+k]=new Poly(v.d[j,i,k]-v.d[i,j,k])+q[i,u]*p[j,w]+p[i,u]*q[j,w]-q[i,w]*p[j,u]-p[i,w]*q[j,u];}}return result;}
}

sealed class Poly
{
 public readonly SortedDictionary<int,double> terms=new();
 public Poly(double constant=0){if(constant!=0)terms[0]=constant;}
 public static Poly Coordinate(int i){var p=new Poly();p.terms[1<<(4*i)]=1;return p;}
 public static Poly operator +(Poly a,Poly b){var p=new Poly();foreach(var (key,v)in a.terms)p.terms[key]=v;foreach(var(key,v)in b.terms){p.terms.TryGetValue(key,out double old);p.terms[key]=old+v;}return p;}
 public static Poly operator -(Poly a,Poly b)=>a+b*(-1);
 public static Poly operator *(Poly a,double s){var p=new Poly();foreach(var(k,v)in a.terms)p.terms[k]=v*s;return p;}
 public static Poly operator *(Poly a,Poly b){var p=new Poly();foreach(var(i,x)in a.terms)foreach(var(j,y)in b.terms){p.terms.TryGetValue(i+j,out double old);p.terms[i+j]=old+x*y;}return p;}
 public double Value(double[] x){double sum=0;foreach(var(key,v)in terms){double term=v;for(int i=0;i<4;i++)term*=System.Math.Pow(x[i],(key>>(4*i))&15);sum+=term;}return sum;}
 public double CubeIntegral(){double sum=0;foreach(var(key,v)in terms){double value=v;for(int i=0;i<4;i++)value/=1+((key>>(4*i))&15);sum+=value;}return sum;}
 public double TriangleAverage(double[][] vertices){double sum=0;foreach(var(key,v)in terms){var indices=new List<int>();for(int i=0;i<4;i++)for(int k=0;k<((key>>(4*i))&15);k++)indices.Add(i);
  double moment=indices.Count switch{0=>1,1=>vertices.Sum(x=>x[indices[0]])/3,2=>(vertices.Sum(x=>x[indices[0]])*vertices.Sum(x=>x[indices[1]])+vertices.Sum(x=>x[indices[0]]*x[indices[1]]))/12,_=>throw new InvalidOperationException("Triangle oracle requires degree <=2")};sum+=v*moment;}return sum;}
}

static class H
{
 public static readonly (int i,int j)[] Pairs=[(0,1),(0,2),(0,3),(1,2),(1,3),(2,3)];
 public static double[] Vector(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetDouble()).ToArray();
 public static double[] Add(double[] a,double[] b)=>a.Zip(b,(x,y)=>x+y).ToArray();public static double[] Sub(double[] a,double[] b)=>a.Zip(b,(x,y)=>x-y).ToArray();
 public static double[] Scale(double[] a,double s)=>a.Select(x=>s*x).ToArray();public static double Dot(double[] a,double[] b)=>a.Zip(b,(x,y)=>x*y).Sum();
 public static double Norm(double[] a)=>System.Math.Sqrt(Dot(a,a));public static double Error(double[] a,double[] b)=>Norm(Sub(a,b));
 public static double MaxError(double[] a,double[] b)=>a.Zip(b,(x,y)=>System.Math.Abs(x-y)).DefaultIfEmpty(0).Max();
 public static double ScaledError(double a,double b)=>System.Math.Abs(a-b)/System.Math.Max(1,System.Math.Max(System.Math.Abs(a),System.Math.Abs(b)));
 public static double MaxLieL1(double[] a)=>Enumerable.Range(0,a.Length/3).Max(i=>System.Math.Abs(a[3*i])+System.Math.Abs(a[3*i+1])+System.Math.Abs(a[3*i+2]));
 public static double[] Basis(int i){var x=new double[6];x[i]=1;return x;}
 public static double[] Mask(int u)=>Enumerable.Range(0,4).Select(i=>(double)((u>>i)&1)).ToArray();
 public static double[] Wedge(double[] u,double[] v)=>Pairs.Select(p=>u[p.i]*v[p.j]-u[p.j]*v[p.i]).ToArray();
 public static double[,] Identity(){var a=new double[6,6];for(int i=0;i<6;i++)a[i,i]=1;return a;}
 public static double[,] Projector(){var p=MScale(Identity(),0.5);p[0,5]=p[5,0]=0.5;p[1,4]=p[4,1]=-0.5;p[2,3]=p[3,2]=0.5;return p;}
 public static double[,] MemberMatrix(string member)=>member=="identity"?Identity():MScale(Projector(),0.5);
 public static double[,] Gram(double h,double[,] btb){var g=MScale(btb,1.5+0.5*h);for(int i=0;i<6;i++)g[i,i]+=1.5+2*h+0.5*h*h;return g;}
 public static double[,] Tr(double[,] a){var r=new double[a.GetLength(1),a.GetLength(0)];for(int i=0;i<a.GetLength(0);i++)for(int j=0;j<a.GetLength(1);j++)r[j,i]=a[i,j];return r;}
 public static double[,] MScale(double[,] a,double t){var r=(double[,])a.Clone();for(int i=0;i<r.GetLength(0);i++)for(int j=0;j<r.GetLength(1);j++)r[i,j]*=t;return r;}
 public static double[,] Mul(double[,] a,double[,] b){var r=new double[a.GetLength(0),b.GetLength(1)];for(int i=0;i<r.GetLength(0);i++)for(int j=0;j<r.GetLength(1);j++)for(int k=0;k<a.GetLength(1);k++)r[i,j]+=a[i,k]*b[k,j];return r;}
 public static double[] MV(double[,] a,double[] v){var r=new double[a.GetLength(0)];for(int i=0;i<r.Length;i++)for(int j=0;j<v.Length;j++)r[i]+=a[i,j]*v[j];return r;}
 public static double MError(double[,] a,double[,] b)=>a.Cast<double>().Zip(b.Cast<double>(),(x,y)=>System.Math.Abs(x-y)).Max();
 public static SimplicialMesh Mesh(int n,double scale){var m=SimplicialMeshGenerator.CreateUniform4D(n);return MeshTopologyBuilder.Build(4,4,Scale(m.VertexCoordinates,scale),m.VertexCount,m.CellVertices);}
 public static double[][] Coordinates(SimplicialMesh m)=>Enumerable.Range(0,m.VertexCount).Select(i=>m.GetVertexCoordinates(i).ToArray()).ToArray();
 public static int DisplacementMask(double[] u,double[] v,double h){int mask=0;for(int i=0;i<4;i++){double d=(v[i]-u[i])/h;if(d==1)mask|=1<<i;else if(d!=0)return -1;}return mask;}
 public static double[] ConstantFaces(SimplicialMesh m,double[] f){var output=new double[m.FaceCount*3];for(int i=0;i<m.FaceCount;i++){var face=m.Faces[i];var a=m.GetVertexCoordinates(face[0]).ToArray();var b=Wedge(Sub(m.GetVertexCoordinates(face[1]).ToArray(),a),Sub(m.GetVertexCoordinates(face[2]).ToArray(),a));output[3*i+2]=Dot(b,f)/2;}return output;}
 public static double[] PolynomialFaces(SimplicialMesh m,Poly[] f){var result=new double[m.FaceCount*3];for(int face=0;face<m.FaceCount;face++){var v=m.Faces[face].Select(i=>m.GetVertexCoordinates(i).ToArray()).ToArray();var b=Wedge(Sub(v[1],v[0]),Sub(v[2],v[0]));for(int pair=0;pair<6;pair++)for(int k=0;k<3;k++)result[3*face+k]+=b[pair]*f[3*pair+k].TriangleAverage(v)/2;}return result;}
 public static double[] AnchoredFaces(SimplicialMesh m,Poly[] f,double[,] r){var output=new double[m.FaceCount*3];for(int face=0;face<m.FaceCount;face++){var v=m.Faces[face].Select(i=>m.GetVertexCoordinates(i).ToArray()).ToArray();var b=Wedge(Sub(v[1],v[0]),Sub(v[2],v[0]));for(int k=0;k<3;k++){double[] value=Enumerable.Range(0,6).Select(i=>f[3*i+k].Value(v[0])).ToArray();output[3*face+k]=Dot(b,MV(r,value))/2;}}return output;}
 public static double IntegratedPairing(Poly[] f,Poly[] df,double[,] metric){var p=new Poly();for(int i=0;i<6;i++)for(int j=0;j<6;j++)for(int k=0;k<3;k++)p+=(f[3*i+k]*df[3*j+k])*metric[i,j];return p.CubeIntegral();}
 public static BoundValues Bounds(Affine a,Affine v,double[,] r,JsonElement b)
 {
  double A=a.ANorm,H=a.HNorm,V=v.ANorm,J=v.HNorm,K=b.GetProperty("cellRowL1").GetDouble();
  double rn=Enumerable.Range(0,6).Max(j=>Enumerable.Range(0,6).Sum(i=>System.Math.Abs(r[i,j])));
  double T=2*H+A*A,DT=2*J+2*A*V,L=2*A*H,DL=2*(H*V+A*J);
  double E=b.GetProperty("localCurvatureFactor").GetDouble()*A*H+b.GetProperty("localQuadraticFactor").GetDouble()*H*H;
  double DE=b.GetProperty("localCurvatureFactor").GetDouble()*(A*J+V*H+H*J);
  double C=K*(E+L/2),DC=K*(DE+DL/2),U=rn*T/2,DU=rn*DT/2,LU=rn*L/2,LDU=rn*DL/2;
  double faces=b.GetProperty("globalFaceFactor").GetDouble(),bulk=b.GetProperty("bulkTypeCount").GetDouble(),boundary=b.GetProperty("boundaryFaceFactor").GetDouble();
  return new(faces*(U*C+C*C/2)+bulk*U*LU+boundary*U*U/2,
   faces*(U*DC+DU*C+C*DC)+bulk*(U*LDU+DU*LU)+boundary*U*DU,C,DC,A,H,V,J,K);
 }
}
