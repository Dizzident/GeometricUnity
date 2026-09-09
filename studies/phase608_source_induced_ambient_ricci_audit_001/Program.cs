using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Geometry;
using static Ambient;

const string Root="studies/phase608_source_induced_ambient_ricci_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P607="studies/phase607_source_induced_vertical_curvature_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase608-a58-source-induced-ambient-ricci-v1";
const string Success="source-induced-ambient-ricci-controls-pass-conditional-curved-background";
const string FixtureJson="""
{
 "baseDimension":4,"verticalDimension":10,"ambientDimension":14,
 "basis":["h0","h1","h2","h3","E00","E11","E22","E33","E01+E10","E02+E20","E03+E30","E12+E21","E13+E31","E23+E32"],
 "points":[[-1,1,1,1],[-1,4,9,16]],"transportL":[1,2,3,4],"horizontalTransport":"L^-T","verticalTransport":"L A L^T",
 "alpha":"1","betas":["0","-1/2"],"sigma":"-1","alternateSigma":"1",
 "metric":"GHH=sigma y, GHV=0, GVV=alpha Tr(p A p B)+beta Tr(p A)Tr(p B)",
 "generalConditions":"alpha sigma (alpha+4beta)!=0, y real invertible symmetric Lorentz",
 "W":"[y sym(uv^T)y-beta/(alpha+4beta)(u^T y v)y]/alpha",
 "connection":["GammaAB=-(A p B+B p A)/2","GammaAu=p A u/2","Gammauv=-sigma W(u,v)/2"],
 "curvatureConvention":"R(a,b)c=nabla_a nabla_b c-nabla_b nabla_a c",
 "typedCurvature":["RABC=-y[[pA,pB],pC]/4","RABu=-[pA,pB]u/4","RAuB=p B p A u/4","RAuv=-sigma W(p A u,v)/4","RuvA=-sigma[W(u,pAv)-W(v,pAu)]/4","Ruvw=sigma(alpha+6beta)/(8alpha d)[u<v,w>-v<u,w>]"],
 "independentRoute":"full metric jets; Koszul inverse solve; differentiated Koszul including -(DG)Gamma; recovered coefficient composition; ordinary trace and independent inverse-Gram lowered contraction",
 "ricciVV":"-5Tr(pA pB)/4+Tr(pA)Tr(pB)/4","ricciHH":"-GHH/(4d)","ricciHV":"0",
 "raisedRicci":"-5PT/(4alpha)-(I-PT)/(4d), rankPT9, rank(I-PT)5",
 "scalar":["-25/2","-10"],"ricciEigenvalues":[["-5/4","-1/4"],["-5/4","1/4"]],"einsteinEigenvalues":[["5","6"],["15/4","21/4"]],
 "determinant":"64 sigma^4 alpha^9 d/(det y)^4","inertia":[[8,6,0],[7,7,0]],"alternateInertia":[[10,4,0],[9,5,0]],
 "sectionalAnchors":["vertical A=diag(0,1,-1,0),B=E12+E21: -1/2","mixed y,e1: -1/(16d)","horizontal e1,e2: (1+6beta)/(8d)"],
 "productDecoys":["intrinsic Ric(A,A)=-2 vs full-5/2","product Ric(e1,e1)=0 vs full1/(4d)"],
 "traceRicciAnchor":"Ric(y,y)=-1 vs intrinsic0",
 "wrongMusicalDecoy":"independent sigma*y^-1 Gram jets produce Gamma(E11,e1)=-e1/2 vs +e1/2; transport both fields",
 "expectedCounts":{"arithmeticControls":8,"contexts":4,"inverseControls":8,"gramEntries":784,"metricFirstJets":10976,"metricSecondJets":153664,"metricSecondJetSymmetry":153664,"connectionOutputs":784,"koszulPairings":10976,"torsionControls":784,"metricCompatibility":10976,"connectionDerivativeOutputs":10976,"curvatureOutputs":10976,"firstPairSkew":10976,"bianchi":10976,"loweredEntries":153664,"lastPairSkew":153664,"pairExchange":153664,"ricciEntries":784,"inverseGramRicciEntries":784,"raisedRicciEntries":784,"einsteinEntries":784,"scalarRows":4,"projectorRows":4,"gramDeterminants":4,"gramInertias":4,"alternateSignatureControls":4,"congruenceMetric":392,"congruenceConnection":392,"congruenceCurvature":5488,"congruenceRicci":392,"sectionalAnchors":12,"traceRicciAnchors":4,"productRicciDecoys":8,"wrongMusicalDecoys":4},
 "exactTolerance":0,"coreFileCount":726,
 "resources":{"estimatedCpuSeconds":80,"maximumEstimatedCpuSeconds":240,"estimatedPeakBytes":268435456,"maximumEstimatedPeakBytes":805306368,"maximumTrackedMatrixProducts":200000000,"maximumMatrixDimension":14,"largestArrayEntries":38416},
 "scope":{"sourceNormalizationSelected":false,"sourceHorizontalConventionUniquelySelected":false,"spinLiftComputed":false,"shiabContractionNonzeroInferred":false,"inducedStationaryBackgroundRejected":false,"fullMetricEulerComputed":false,"historicalFlatControlsRewritten":false}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","metric-connection-control-failed","full-curvature-control-failed","ricci-projector-control-failed","congruence-decoy-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase608SourceInducedAmbientRicciAudit.csproj",["study"]=Root+"/STUDY.md",["ambient-helper"]=Root+"/AmbientGeometry.cs",
 ["geometry-helper"]=P607+"/VerticalGeometry.cs",["phase607-summary"]=P607+"/output/source_induced_vertical_curvature_audit_summary.json",["phase607-contract"]=P607+"/preregistration/contract_v1.json",["phase607-program"]=P607+"/Program.cs",["phase607-study"]=P607+"/STUDY.md",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==608&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(9.1)","(9.4)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);
void Inc(string k)=>counts[k]++;
bool knownAnswerPassed=true,connectionPassed=true,curvaturePassed=true,ricciPassed=true,decoysPassed=true,congruencePassed=true;
var eta=Matrix.Diagonal(-1,1,1,1);var l=Matrix.Diagonal(1,2,3,4);Matrix[] points=[eta,l*eta*l];Rational[] betas=[0,new Rational(-1,2)];
var test=new Matrix(2);test[0,1]=1;test[1,0]=1;var singular=Matrix.Diagonal(1,0);var mixed=Matrix.Diagonal(2,-3);
bool[] arithmetic=[new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),eta.Inverse().Same(eta),(l*l.Inverse()).Same(Matrix.Identity(4)),test.Inertia()==(1,1,0)&&Rank(test)==2,singular.Inertia()==(1,0,1)&&Rank(singular)==1,mixed.Determinant()==-6,mixed.Inertia()==(1,1,0)];
foreach(bool pass in arithmetic){Inc("arithmeticControls");knownAnswerPassed&=pass;}if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
var rows=new List<object>();var witnessRows=new List<object>();var saved=new Context[2,2];
for(int point=0;point<2;point++)for(int bi=0;bi<2;bi++)
{
 Inc("contexts");var y=points[point];Rational beta=betas[bi];var g=new Ambient(y,1,beta,-1);var gram=g.Gram;var gi=gram.Inverse();counts["inverseControls"]+=2;connectionPassed&=(y*g.P).Same(Matrix.Identity(4))&&(gram*gi).Same(Matrix.Identity(14));
 var gamma=new Rational[14,14][];var derivative=new Rational[14,14,14][];var r=new Rational[14,14,14,14];var low=new Rational[14,14,14,14];var gammaRows=new List<object>();var curvatureRows=new List<object>();
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)
 {
  Inc("gramEntries");connectionPassed&=gram[a,b]==g.MetricEntry(a,b);
  for(int x=0;x<14;x++)
  {
   Inc("metricFirstJets");connectionPassed&=g.D[x,a,b]==g.FirstEntry(x,a,b);
   for(int z=0;z<14;z++){Inc("metricSecondJets");Inc("metricSecondJetSymmetry");connectionPassed&=g.DD[x,z,a,b]==g.SecondEntry(x,z,a,b)&&g.DD[x,z,a,b]==g.DD[z,x,a,b];}
  }
  gamma[a,b]=Koszul(gi,g.D,a,b);Inc("connectionOutputs");connectionPassed&=gamma[a,b].SequenceEqual(g.GammaExpected(a,b));
  for(int c=0;c<14;c++){Inc("koszulPairings");connectionPassed&=Dot(gamma[a,b],Apply(gram,Unit(14,c)))==(g.D[a,b,c]+g.D[b,a,c]-g.D[c,a,b])*new Rational(1,2);if(gamma[a,b][c]!=0)gammaRows.Add(new{a,b,c,value=gamma[a,b][c].ToString()});}
 }
 Rational[] Compose(int a,Rational[] v){var output=new Rational[14];for(int b=0;b<14;b++)if(v[b]!=0)for(int d=0;d<14;d++)if(gamma[a,b][d]!=0)output[d]+=v[b]*gamma[a,b][d];return output;}
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)
 {
  Inc("torsionControls");connectionPassed&=gamma[a,b].SequenceEqual(gamma[b,a]);
  for(int c=0;c<14;c++){Inc("metricCompatibility");connectionPassed&=g.D[a,b,c]==Dot(gamma[a,b],Apply(gram,Unit(14,c)))+Dot(Unit(14,b),Apply(gram,gamma[a,c]));}
  for(int x=0;x<14;x++)
  {
   var cov=new Rational[14];for(int c=0;c<14;c++){cov[c]=(g.DD[x,a,b,c]+g.DD[x,b,a,c]-g.DD[x,c,a,b])*new Rational(1,2);for(int d=0;d<14;d++)if(g.D[x,d,c]!=0&&gamma[a,b][d]!=0)cov[c]-=g.D[x,d,c]*gamma[a,b][d];}
   derivative[x,a,b]=Solve(gi,cov);Inc("connectionDerivativeOutputs");connectionPassed&=derivative[x,a,b].SequenceEqual(g.DerivativeExpected(x,a,b));
  }
 }
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)
 {
  var value=Add(Add(derivative[a,b,c],Scale(derivative[b,a,c],-1)),Add(Compose(a,gamma[b,c]),Scale(Compose(b,gamma[a,c]),-1)));
  Inc("curvatureOutputs");curvaturePassed&=value.SequenceEqual(g.CurvatureExpected(a,b,c));var lowered=Apply(gram,value);
  for(int d=0;d<14;d++){r[a,b,c,d]=value[d];low[a,b,c,d]=lowered[d];Inc("loweredEntries");if(value[d]!=0)curvatureRows.Add(new{a,b,c,d,value=value[d].ToString()});}
 }
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)
 {
  Inc("firstPairSkew");Inc("bianchi");
  for(int d=0;d<14;d++){curvaturePassed&=r[a,b,c,d]+r[b,a,c,d]==0&&r[a,b,c,d]+r[b,c,a,d]+r[c,a,b,d]==0;Inc("lastPairSkew");Inc("pairExchange");curvaturePassed&=low[a,b,c,d]+low[a,b,d,c]==0&&low[a,b,c,d]==low[c,d,a,b];}
 }
 var ric=new Matrix(14);
 for(int b=0;b<14;b++)for(int c=0;c<14;c++)
 {
  Rational direct=0,inverse=0;for(int a=0;a<14;a++){direct+=r[a,b,c,a];for(int d=0;d<14;d++)if(gi[a,d]!=0&&low[a,b,c,d]!=0)inverse+=gi[a,d]*low[a,b,c,d];}
  ric[b,c]=direct;Inc("ricciEntries");Inc("inverseGramRicciEntries");ricciPassed&=direct==g.RicciExpected(b,c)&&direct==inverse;
 }
 var pt=g.TracelessProjector();var rest=Matrix.Identity(14)-pt;var raised=gi*ric;var raisedExpected=pt.Scale(new Rational(-5,4))+rest.Scale(Matrix.Inv(4*g.Delta)*-1);
 var einstein=raised-Matrix.Identity(14).Scale(raised.Trace()*new Rational(1,2));var eigen=fx.GetProperty("einsteinEigenvalues")[bi];Rational e1=bi==0?5:new Rational(15,4),e2=bi==0?6:new Rational(21,4);var einsteinExpected=pt.Scale(e1)+rest.Scale(e2);
 for(int a=0;a<14;a++)for(int b=0;b<14;b++){Inc("raisedRicciEntries");Inc("einsteinEntries");ricciPassed&=raised[a,b]==raisedExpected[a,b]&&einstein[a,b]==einsteinExpected[a,b];}
 Inc("projectorRows");ricciPassed&=(pt*pt).Same(pt)&&(rest*rest).Same(rest)&&(pt*rest).Zero&&Rank(pt)==9&&Rank(rest)==5&&(gram*pt).Symmetric;
 Inc("scalarRows");ricciPassed&=raised.Trace()==new Rational(-45,4)-5*Matrix.Inv(4*g.Delta)&&raised.Trace().ToString()==fx.GetProperty("scalar")[bi].GetString()&&e1.ToString()==eigen[0].GetString()&&e2.ToString()==eigen[1].GetString();
 Inc("gramDeterminants");Inc("gramInertias");var inertia=gram.Inertia();var det=gram.Determinant();connectionPassed&=det==64*g.Delta*Matrix.Inv(Pow(y.Determinant(),4))&&inertia==(bi==0?(8,6,0):(7,7,0));
 var alternative=new Ambient(y,1,beta,1);Inc("alternateSignatureControls");decoysPassed&=alternative.Gram.Inertia()==(bi==0?(10,4,0):(9,5,0))&&alternative.Gram.Inertia()!=(7,7,0)&&alternative.Gram.Determinant()==det;
 var transport=point==0?Matrix.Identity(4):l;var inverseTransport=transport.Inverse();var aa=V(transport*Matrix.Diagonal(0,1,-1,0)*transport);var bb=V(transport*Basis()[7]*transport);var yy=V(y);var uu=H(Apply(inverseTransport,Unit(4,1)));var vv=H(Apply(inverseTransport,Unit(4,2)));
 Rational[] Recovered(Rational[] a,Rational[] b,Rational[] c){var output=new Rational[14];for(int i=0;i<14;i++)if(a[i]!=0)for(int j=0;j<14;j++)if(b[j]!=0)for(int k=0;k<14;k++)if(c[k]!=0)for(int d=0;d<14;d++)if(r[i,j,k,d]!=0)output[d]+=a[i]*b[j]*c[k]*r[i,j,k,d];return output;}
 Rational Section(Rational[] a,Rational[] b){var norm=g.Pair(a,a)*g.Pair(b,b)-g.Pair(a,b)*g.Pair(a,b);return g.Pair(Recovered(a,b,b),a)*Matrix.Inv(norm);}
 Rational[] sections=[Section(aa,bb),Section(yy,uu),Section(uu,vv)];Rational[] expectedSections=[new Rational(-1,2),Matrix.Inv(16*g.Delta)*-1,(1+6*beta)*Matrix.Inv(8*g.Delta)];
 for(int s=0;s<3;s++){Inc("sectionalAnchors");decoysPassed&=sections[s]==expectedSections[s];}
 var ricAA=Dot(aa,Apply(ric,aa));var ricUU=Dot(uu,Apply(ric,uu));var ricYY=Dot(yy,Apply(ric,yy));Inc("traceRicciAnchors");decoysPassed&=ricYY==-1&&ricYY!=0;
 counts["productRicciDecoys"]+=2;decoysPassed&=ricAA==new Rational(-5,2)&&ricAA!=-2&&ricUU==Matrix.Inv(4*g.Delta)&&ricUU!=0;
 var wrong=new Ambient(y,1,beta,-1,true);var wrongInverse=wrong.Gram.Inverse();var inputA=V(transport*Basis()[1]*transport);var wrongGamma=new Rational[14];var rightGamma=new Rational[14];
 for(int a=0;a<14;a++)if(inputA[a]!=0)for(int b=0;b<14;b++)if(uu[b]!=0){wrongGamma=Add(wrongGamma,Scale(Koszul(wrongInverse,wrong.D,a,b),inputA[a]*uu[b]));rightGamma=Add(rightGamma,Scale(gamma[a,b],inputA[a]*uu[b]));}
 Inc("wrongMusicalDecoys");decoysPassed&=wrongGamma.SequenceEqual(Scale(uu,new Rational(-1,2)))&&rightGamma.SequenceEqual(Scale(uu,new Rational(1,2)))&&Add(wrongGamma,Scale(rightGamma,-1)).SequenceEqual(Scale(uu,-1));
 saved[point,bi]=new Context(gram,gamma,r,ric);witnessRows.Add(new{point,beta=beta.ToString(),sectional=sections.Select(x=>x.ToString()).ToArray(),ricciAA=ricAA.ToString(),ricciUU=ricUU.ToString(),ricciTrace=ricYY.ToString(),wrongGamma=wrongGamma.Select(x=>x.ToString()).ToArray(),rightGamma=rightGamma.Select(x=>x.ToString()).ToArray()});
 rows.Add(new{point,beta=beta.ToString(),sigma="-1",gram=gram.Text(),determinant=det.ToString(),inertia=new[]{inertia.Positive,inertia.Negative,inertia.Zero},ricci=ric.Text(),raisedRicci=raised.Text(),scalar=raised.Trace().ToString(),einstein=einstein.Text(),tracelessProjector=pt.Text(),projectorRanks=new[]{Rank(pt),Rank(rest)},connectionCoefficients=gammaRows,curvatureCoefficients=curvatureRows});
}
Rational[] factors=Enumerable.Range(0,4).Select(i=>Matrix.Inv(l[i,i])).Concat(Pairs.Select(p=>l[p.I,p.I]*l[p.J,p.J])).ToArray();
for(int bi=0;bi<2;bi++)for(int a=0;a<14;a++)for(int b=0;b<14;b++)
{
 var initial=saved[0,bi];var target=saved[1,bi];Inc("congruenceMetric");Inc("congruenceRicci");Inc("congruenceConnection");congruencePassed&=target.Gram[a,b]*factors[a]*factors[b]==initial.Gram[a,b]&&target.Ricci[a,b]*factors[a]*factors[b]==initial.Ricci[a,b];
 for(int d=0;d<14;d++)congruencePassed&=target.Gamma[a,b][d]*factors[a]*factors[b]==initial.Gamma[a,b][d]*factors[d];
 for(int c=0;c<14;c++){Inc("congruenceCurvature");for(int d=0;d<14;d++)congruencePassed&=target.R[a,b,c,d]*factors[a]*factors[b]*factors[c]==initial.R[a,b,c,d]*factors[d];}
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());bool resourcesPassed=Matrix.Products<=fx.GetProperty("resources").GetProperty("maximumTrackedMatrixProducts").GetInt64();
bool controlsPassed=knownAnswerPassed&&connectionPassed&&curvaturePassed&&ricciPassed&&decoysPassed&&congruencePassed&&countsPassed&&resourcesPassed;string verdict=!knownAnswerPassed?precedence[1]:!connectionPassed?precedence[2]:!curvaturePassed?precedence[3]:!ricciPassed?precedence[4]:!decoysPassed||!congruencePassed||!countsPassed||!resourcesPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,connectionPassed,curvaturePassed,ricciPassed,decoysPassed,congruencePassed,countsPassed,resourcesPassed,counts,trackedMatrixProducts=Matrix.Products,rows,witnessRows,sourceNormalizationSelected=false,sourceHorizontalConventionUniquelySelected=false,spinLiftComputed=false,shiabContractionNonzeroInferred=false,inducedStationaryBackgroundRejected=false,fullMetricEulerComputed=false,historicalFlatControlsRewritten=false});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=608,phaseId="phase608-source-induced-ambient-ricci-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/source_induced_ambient_ricci_audit.json",json);File.WriteAllText(Root+"/output/source_induced_ambient_ricci_audit_summary.json",json);Console.WriteLine($"Phase608 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
sealed record Context(Matrix Gram,Rational[,][] Gamma,Rational[,,,] R,Matrix Ricci);
