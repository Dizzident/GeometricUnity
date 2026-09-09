using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Geometry;
using static Ambient;
using static Splitting;

const string Root="studies/phase615_source_induced_splitting_volume_scope_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P607="studies/phase607_source_induced_vertical_curvature_audit_001";
const string P608="studies/phase608_source_induced_ambient_ricci_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase615-a60-source-induced-splitting-volume-scope-v1";
const string Success="induced-splitting-volume-controls-pass-homothety-outside-declared-image";
const string FixtureJson="""
{
 "baseDimension": 4,
 "verticalDimension": 10,
 "ambientDimension": 14,
 "jetDimension": 40,
 "basis": [
  "h0",
  "h1",
  "h2",
  "h3",
  "E00",
  "E11",
  "E22",
  "E33",
  "E01+E10",
  "E02+E20",
  "E03+E30",
  "E12+E21",
  "E13+E31",
  "E23+E32"
 ],
 "points": [
  [
   -1,
   1,
   1,
   1
  ],
  [
   -1,
   4,
   9,
   16
  ]
 ],
 "downstairsMetric": [
  -1,
  1,
  1,
  1
 ],
 "alpha": "1",
 "betas": [
  "0",
  "-1/2"
 ],
 "sigma": "-1",
 "splittingMultipliers": [
  "0",
  "1",
  "-2"
 ],
 "constantDownstairsScales": [
  "1/2",
  "2"
 ],
 "jetBasis": "J[index/10]=SymBasis[index%10]; all other derivative matrices zero",
 "seedJet": "J_i=(i+1) SymBasis[(3i+1)%10]",
 "connection": "C_i[k,j]=h^kl(J_i[j,l]+J_j[i,l]-J_l[i,j])/2",
 "horizontalLift": "L_i=C_i^T y+y C_i; graph(+L)",
 "inverseJet": "C=Koszul_y(L); J_i=C_i^T h+h C_i",
 "connectionMapDeterminant": "(det y/det h)^10",
 "connectionMapRank": 40,
 "metric": "G=P^-T D P^-1; D=diag(-y,V_y); P=I+N; N_VH=L",
 "blocks": "G_HH=-y+L^T V L; G_HV=-L^T V; G_VV=V",
 "inverse": "G^-1=P D^-1 P^T",
 "determinant": "64(1+4beta)/(det y)^4",
 "inertia": [
  [
   8,
   6,
   0
  ],
  [
   7,
   7,
   0
  ]
 ],
 "tangent": "delta G=-(delta N)^T D(I-N)-(I-N)^T D delta N",
 "inverseTangent": "delta G^-1=delta N D^-1 P^T+P D^-1(delta N)^T=-G^-1 delta G G^-1",
 "centralDifference": "[G(N+delta N)-G(N-delta N)]/2; exact since polynomial degree2",
 "secondDifference": "G(N+delta N)+G(N-delta N)-2G(N)=2 delta N^T D delta N",
 "volumeTrace": "Tr(G^-1 delta G)=0",
 "uniformHomothetyTrace": "28",
 "uniformHomothetyVerticalWitness": "(2G)[4,4]=2(1+beta)/(y00)^2 !=0",
 "wrongSign": "P^T D P has opposite nonzero mixed block at nonzero L; wrong tangent flips mixed block at every jet",
 "expectedCounts": {
  "arithmeticControls": 8,
  "contexts": 4,
  "connectionMapColumns": 160,
  "connectionTorsionEntries": 10240,
  "connectionReconstructionRows": 160,
  "jetRescalingRows": 320,
  "mapRanks": 4,
  "mapDeterminants": 4,
  "mapInverseProducts": 8,
  "metricRows": 12,
  "shearControls": 12,
  "metricBlockEntries": 2352,
  "inverseEntries": 2352,
  "inverseProducts": 24,
  "metricDeterminants": 12,
  "metricInertias": 12,
  "verticalMetricEntries": 1200,
  "homothetyVerticalEntries": 1200,
  "homothetyTraceRows": 12,
  "wrongMetricSignRows": 8,
  "metricRescalingRows": 24,
  "tangentRows": 480,
  "tangentBlockEntries": 94080,
  "centralDifferenceEntries": 94080,
  "secondDifferenceEntries": 94080,
  "inverseDerivativeEntries": 94080,
  "tangentDeterminants": 960,
  "verticalTangentEntries": 48000,
  "volumeTraceRows": 480,
  "mixedNonzeroRows": 480,
  "tangentReconstructionRows": 480,
  "wrongTangentSignRows": 480,
  "zeroSplittingDiagonalRows": 160
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "resources": {
  "estimatedCpuSeconds": 60,
  "maximumEstimatedCpuSeconds": 180,
  "estimatedPeakBytes": 134217728,
  "maximumEstimatedPeakBytes": 536870912,
  "maximumTrackedMatrixProducts": 200000000,
  "maximumMatrixDimension": 40,
  "largestArrayEntries": 38416,
  "maximumRetainedTangentRows": 480
 },
 "scope": {
  "sourceNormalizationSelected": false,
  "sourceHorizontalConventionUniquelySelected": false,
  "uniformHomothetySourceAdmissible": false,
  "movingObserverSectionIncluded": false,
  "pulledBackActionComputed": false,
  "fullMetricEulerComputed": false,
  "globalVacuumClaimed": false,
  "physicalSpectrumClaimed": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","connection-map-control-failed","metric-block-control-failed","tangent-volume-control-failed","scope-decoy-count-resource-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase615SourceInducedSplittingVolumeScopeAudit.csproj",["study"]=Root+"/STUDY.md",["splitting-helper"]=Root+"/SplittingControls.cs",
 ["ambient-helper"]=P608+"/AmbientGeometry.cs",["phase608-summary"]=P608+"/output/source_induced_ambient_ricci_audit_summary.json",["phase608-contract"]=P608+"/preregistration/contract_v1.json",["phase608-program"]=P608+"/Program.cs",["phase608-study"]=P608+"/STUDY.md",
 ["geometry-helper"]=P607+"/VerticalGeometry.cs",["phase607-summary"]=P607+"/output/source_induced_vertical_curvature_audit_summary.json",["phase607-contract"]=P607+"/preregistration/contract_v1.json",["phase607-program"]=P607+"/Program.cs",["phase607-study"]=P607+"/STUDY.md",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==615&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(9.1)","(9.4)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);
void Inc(string k)=>counts[k]++;
bool knownAnswerPassed=true,connectionPassed=true,metricPassed=true,tangentPassed=true,decoysPassed=true;
var eta=Matrix.Diagonal(-1,1,1,1);var transport=Matrix.Diagonal(1,2,3,4);Matrix[] points=[eta,transport*eta*transport];Rational[] betas=[0,new Rational(-1,2)],multipliers=[0,1,-2],scales=[new Rational(1,2),2];
var test=new Matrix(2);test[0,1]=1;test[1,0]=1;var singular=Matrix.Diagonal(1,0);var mixed=Matrix.Diagonal(2,-3);
bool[] arithmetic=[new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),eta.Inverse().Same(eta),(transport*transport.Inverse()).Same(Matrix.Identity(4)),test.Inertia()==(1,1,0)&&Rank(test)==2,singular.Inertia()==(1,0,1)&&Rank(singular)==1,mixed.Determinant()==-6,mixed.Inertia()==(1,1,0)];
foreach(bool pass in arithmetic){Inc("arithmeticControls");knownAnswerPassed&=pass;}if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
var rows=new List<object>();var connectionRows=new List<object>();
for(int point=0;point<2;point++)for(int bi=0;bi<2;bi++)
{
 Inc("contexts");var y=points[point];Rational beta=betas[bi];var ambient=new Ambient(y,1,beta,-1);var d=ambient.Gram;var di=d.Inverse();var map=ConnectionMap(eta,y);var mapInverse=map.Inverse();
 Inc("mapRanks");Inc("mapDeterminants");counts["mapInverseProducts"]+=2;
 var rank=Rank(map);var mapDet=map.Determinant();var expectedMapDet=Pow(y.Determinant()*Matrix.Inv(eta.Determinant()),10);
 connectionPassed&=rank==40&&mapDet==expectedMapDet&&(map*mapInverse).Same(Matrix.Identity(40))&&(mapInverse*map).Same(Matrix.Identity(40));
 var deltaNs=new Matrix[40];var jetRows=new List<object>();
 for(int index=0;index<40;index++)
 {
  Inc("connectionMapColumns");var jet=Jet(index);var c=Splitting.Koszul(eta,jet);var horizontal=Lower(y,c);var recoveredC=Splitting.Koszul(y,horizontal);var recoveredJet=Lower(eta,recoveredC);
  var coordinates=Flatten(horizontal);connectionPassed&=Enumerable.Range(0,40).All(i=>coordinates[i]==map[i,index]);
  for(int i=0;i<4;i++)for(int j=0;j<4;j++)for(int k=0;k<4;k++){Inc("connectionTorsionEntries");connectionPassed&=c[i][k,j]==c[j][k,i];}
  Inc("connectionReconstructionRows");connectionPassed&=c.Zip(recoveredC,(a,b)=>a.Same(b)).All(x=>x)&&jet.Zip(recoveredJet,(a,b)=>a.Same(b)).All(x=>x);
  foreach(var scale in scales)
  {
   Inc("jetRescalingRows");var scaled=Splitting.Koszul(eta.Scale(scale),jet.Select(a=>a.Scale(scale)).ToArray());
   connectionPassed&=scaled.Zip(c,(a,b)=>a.Same(b)).All(x=>x)&&Lower(y,scaled).Zip(horizontal,(a,b)=>a.Same(b)).All(x=>x);
  }
  deltaNs[index]=N(horizontal);jetRows.Add(new{index,jet=jet.Select(a=>a.Text()).ToArray(),connection=c.Select(a=>a.Text()).ToArray(),lift=horizontal.Select(a=>a.Text()).ToArray(),recoveredJet=recoveredJet.Select(a=>a.Text()).ToArray()});
 }
 connectionRows.Add(new{point,beta=beta.ToString(),map=map.Text(),mapInverse=mapInverse.Text(),rank,determinant=mapDet.ToString(),expectedDeterminant=expectedMapDet.ToString(),jetRows});
 var seed=Seed();var seedConnection=Splitting.Koszul(eta,seed);var seedLift=Lower(y,seedConnection);
 for(int mi=0;mi<3;mi++)
 {
  Inc("metricRows");Rational multiplier=multipliers[mi];var n=N(seedLift.Select(a=>a.Scale(multiplier)).ToArray());var p=Matrix.Identity(14)+n;var pi=Matrix.Identity(14)-n;
  Inc("shearControls");metricPassed&=(n*n).Zero&&(p*pi).Same(Matrix.Identity(14))&&(pi*p).Same(Matrix.Identity(14))&&p.Determinant()==1&&pi.Determinant()==1;
  var g=Splitting.Metric(d,n);var block=Blocks(d,n);var inverse=g.Inverse();var expectedInverse=p*di*Transpose(p);
  for(int a=0;a<14;a++)for(int b=0;b<14;b++){Inc("metricBlockEntries");Inc("inverseEntries");metricPassed&=g[a,b]==block[a,b]&&inverse[a,b]==expectedInverse[a,b];}
  counts["inverseProducts"]+=2;metricPassed&=(g*inverse).Same(Matrix.Identity(14))&&(inverse*g).Same(Matrix.Identity(14))&&g.Symmetric&&inverse.Symmetric;
  Inc("metricDeterminants");Inc("metricInertias");var determinant=g.Determinant();var inertia=g.Inertia();
  metricPassed&=determinant==ExpectedDeterminant(y,beta)&&determinant==d.Determinant()&&inertia==(bi==0?(8,6,0):(7,7,0));
  var homothety=g.Scale(2);
  for(int a=4;a<14;a++)for(int b=4;b<14;b++){Inc("verticalMetricEntries");Inc("homothetyVerticalEntries");metricPassed&=g[a,b]==d[a,b];decoysPassed&=homothety[a,b]==2*d[a,b];}
  Inc("homothetyTraceRows");var homothetyTrace=Matrix.TraceProduct(inverse,homothety);
  decoysPassed&=homothetyTrace==28&&homothety[4,4]==2*(1+beta)*Matrix.Inv(y[0,0]*y[0,0])&&homothety[4,4]!=0;
  if(mi!=0){Inc("wrongMetricSignRows");var wrong=Transpose(p)*d*p;decoysPassed&=!wrong.Same(g)&&MixedNonzero(g)&&Enumerable.Range(0,4).All(a=>Enumerable.Range(4,10).All(b=>wrong[a,b]==g[a,b]*-1));}
  foreach(var scale in scales)
  {
   Inc("metricRescalingRows");var scaledC=Splitting.Koszul(eta.Scale(scale),seed.Select(a=>a.Scale(scale*multiplier)).ToArray());
   var scaledN=N(Lower(y,scaledC));decoysPassed&=scaledN.Same(n)&&Splitting.Metric(d,scaledN).Same(g);
  }
  var tangentRows=new List<object>();
  for(int index=0;index<40;index++)
  {
   Inc("tangentRows");var dn=deltaNs[index];var dg=Tangent(d,n,dn);var expectedDg=TangentBlocks(d,n,dn);
   var plus=Splitting.Metric(d,n+dn);var minus=Splitting.Metric(d,n-dn);var central=(plus-minus).Scale(new Rational(1,2));var second=plus+minus-g.Scale(2);var expectedSecond=(Transpose(dn)*d*dn).Scale(2);
   var dInverse=(inverse*dg*inverse).Scale(-1);var closedDInverse=dn*di*Transpose(p)+p*di*Transpose(dn);
   // The inverse is also exactly quadratic in N. Independent Gaussian inverses
   // at both endpoints distinguish an inverse-sign error from a shared formula.
   var inverseCentral=(plus.Inverse()-minus.Inverse()).Scale(new Rational(1,2));
   for(int a=0;a<14;a++)for(int b=0;b<14;b++)
   {
    Inc("tangentBlockEntries");Inc("centralDifferenceEntries");Inc("secondDifferenceEntries");Inc("inverseDerivativeEntries");
    tangentPassed&=dg[a,b]==expectedDg[a,b]&&central[a,b]==dg[a,b]&&second[a,b]==expectedSecond[a,b]&&dInverse[a,b]==closedDInverse[a,b]&&dInverse[a,b]==inverseCentral[a,b];
   }
   counts["tangentDeterminants"]+=2;tangentPassed&=plus.Determinant()==determinant&&minus.Determinant()==determinant;
   for(int a=4;a<14;a++)for(int b=4;b<14;b++){Inc("verticalTangentEntries");tangentPassed&=dg[a,b]==0;}
   Inc("volumeTraceRows");var trace=Matrix.TraceProduct(inverse,dg);tangentPassed&=trace==0;
   Inc("mixedNonzeroRows");tangentPassed&=MixedNonzero(dg)&&dg.Symmetric;
   // deltaG_VH=-V deltaL, so recover every lift coefficient from its mixed block.
   var recoveredN=new Matrix(14);for(int a=4;a<14;a++)for(int i=0;i<4;i++)for(int b=4;b<14;b++)recoveredN[a,i]-=di[a,b]*dg[b,i];
   Inc("tangentReconstructionRows");tangentPassed&=recoveredN.Same(dn);
   Inc("wrongTangentSignRows");var wrongTangent=Tangent(d,n.Scale(-1),dn.Scale(-1));decoysPassed&=!wrongTangent.Same(dg)&&Enumerable.Range(0,4).All(a=>Enumerable.Range(4,10).All(b=>wrongTangent[a,b]==dg[a,b]*-1));
   if(mi==0){Inc("zeroSplittingDiagonalRows");tangentPassed&=HorizontalZero(dg);}
   tangentRows.Add(new{index,deltaN=dn.Text(),variation=dg.Text(),inverseVariation=dInverse.Text(),volumeTrace=trace.ToString(),verticalZero=Enumerable.Range(4,10).All(a=>Enumerable.Range(4,10).All(b=>dg[a,b]==0)),mixedNonzero=MixedNonzero(dg)});
  }
  rows.Add(new{point,beta=beta.ToString(),multiplier=multiplier.ToString(),N=n.Text(),metric=g.Text(),inverse=inverse.Text(),determinant=determinant.ToString(),inertia=new[]{inertia.Positive,inertia.Negative,inertia.Zero},homothety=homothety.Text(),homothetyTrace=homothetyTrace.ToString(),homothetyVerticalWitness=homothety[4,4].ToString(),tangentRows});
 }
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());bool resourcesPassed=Matrix.Products<=fx.GetProperty("resources").GetProperty("maximumTrackedMatrixProducts").GetInt64();
bool controlsPassed=knownAnswerPassed&&connectionPassed&&metricPassed&&tangentPassed&&decoysPassed&&countsPassed&&resourcesPassed;
string verdict=!knownAnswerPassed?precedence[1]:!connectionPassed?precedence[2]:!metricPassed?precedence[3]:!tangentPassed?precedence[4]:!decoysPassed||!countsPassed||!resourcesPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,connectionPassed,metricPassed,tangentPassed,decoysPassed,countsPassed,resourcesPassed,counts,trackedMatrixProducts=Matrix.Products,connectionRows,rows,scope=JsonNode.Parse(fx.GetProperty("scope").GetRawText())});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=615,phaseId="phase615-source-induced-splitting-volume-scope-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/source_induced_splitting_volume_scope_audit.json",json);File.WriteAllText(Root+"/output/source_induced_splitting_volume_scope_audit_summary.json",json);Console.WriteLine($"Phase615 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
