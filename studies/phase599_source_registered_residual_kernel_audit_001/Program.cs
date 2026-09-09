using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Gu.ReferenceCpu;
using static Algebra;
using Tensor=System.Collections.Generic.Dictionary<(int Form,int Blade),Scalar>;
using PairMatrix=System.Collections.Generic.Dictionary<(int Row,int Column),long>;

const string Root="studies/phase599_source_registered_residual_kernel_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=Root+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase599-a53-source-registered-residual-kernel-v1";
const string Success="source-registered-residual-kernel-mismatch-scoped";
const string P588="studies/phase588_fixed_domain_action_force_consistency_001";
const string P591="studies/phase591_source_hodge_curvature_branch_audit_001";
const string P592="studies/phase592_companion_tensor_chirality_audit_001";
const int Omega=16383;
const string FixtureJson="""
{
 "sourceDimension":14,"sourceSignature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "registeredDimension":4,"registeredPairOrder":[[0,1],[0,2],[0,3],[1,2],[1,3],[2,3]],
 "sigmaPlus":[[1,0,0,0,0,1],[0,1,0,0,-1,0],[0,0,1,1,0,0]],
 "sigmaMinus":[[1,0,0,0,0,-1],[0,1,0,0,1,0],[0,0,1,-1,0,0]],
 "embedding":{"J":"Gamma(Sigma)/2","plusBracketFactor":-2,"minusBracketFactor":2,
  "plusCoreEmbedding":"E=-J/2","minusCoreEmbedding":"E=J/2","sourceJGram":"1/2","sourceEGram":"1/8","coreGram":1},
 "curvatureRows":["selfdual-Weyl","antiselfdual-Weyl","zero","full-so4-plane-01"],
 "weylFormula":"R=Sigma1 tensor Sigma1-Sigma2 tensor Sigma2",
 "curvatureConvention":"F_ab=(1/2) sum_(c<d) R_abcd sigma_c sigma_d gamma_c gamma_d",
 "branches":["CCC","CCA","AAC","AAA"],"formalMonomials":["a","b","ac","bc","ad","bd"],
 "phi1":"(a+b Omega)gamma","phi2":"(c+i d Omega)Gamma2",
 "chain":"star1_inverse([Phi1 wedge star14(F)]first-(1/2)*star14([Phi1 wedge star14([Phi2 wedge star14(F)]inner)]outer))",
 "planePrediction":{"CCC":"-a J-b Omega J","CCA":"-a J-b Omega J-ad Omega G-bd G","AAC":"0","AAA":"ac G+bc Omega G",
  "J":"theta0 gamma0+theta1 gamma1","G":"sum_j=0..13 theta_j gamma_j"},
 "planeNumericalRows":[{"id":"canonical","a":1,"b":0,"c":1,"d":0,"traceSelfPair":"-2","termCount":2},
  {"id":"matched-minus","a":1,"b":-1,"c":1,"d":1,"traceSelfPair":"0","termCount":24},
  {"id":"matched-plus","a":1,"b":1,"c":1,"d":-1,"traceSelfPair":"0","termCount":24}],
 "registered":{"phi1":"sd2","phi2":"id0","coefficient":0.5,"operator":"(I+star4)/4","identityControl":"id0/none",
  "api":"Gu.ReferenceCpu.Lambda2Algebra.MemberEndomorphism","meshInsertionPerformed":false,
  "expectedInputNormSquared":[16,16,0],"expectedOutputNormSquared":[4,0,0],"expectedHalfSquare":[2,0,0]},
 "knownAnswerGrades":[0,1,2,12,13,14],
 "counts":{"cliffordWordCases":44944,"hodgeCases":16384,"rawBracketChecks":18,"embeddedBracketChecks":18,
  "rawGramChecks":18,"embeddedGramChecks":18,"hAntiChecks":6,"coreStructureEntries":27,"coreGramEntries":9,
  "riemannComponentChecks":153664,"ricciEntries":784,"curvatureDictionaryChecks":4,
  "sourceFormalRows":96,"sourceNonzeroRows":8,"sourceZeroRows":88,"planeNumericalRows":3,
  "registeredMatrixEntries":108,"registeredResidualEntries":108,"registeredNormRows":3},
 "hodgeDomainsSeparate":true,"sourcePairing":"-ReTr(XY)/128 times signed form metric; not source-selected positive norm",
 "primaryConclusion":"pointwise kernel mismatch under declared Lie embeddings; not action equivalence modulo boundary terms",
 "globalConnectionLiftClaimed":false,"torsionZeroOnlyPointwise":true,
 "exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":5,"maximumEstimatedCpuSeconds":30,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":268435456}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
 "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
 "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","embedding-or-curvature-control-failed",
 "literal-source-control-failed","registered-control-failed","kernel-witness-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["project"]=Root+"/Phase599SourceRegisteredResidualKernelAudit.csproj",
 ["study"]=Root+"/STUDY.md",["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["algebra-helper"]=P592+"/ExactAlgebra.cs",["phase588-summary"]=P588+"/output/fixed_domain_action_force_consistency_summary.json",
 ["phase588-contract"]=P588+"/preregistration/contract_v1.json",["phase591-summary"]=P591+"/output/source_hodge_curvature_branch_audit_summary.json",
 ["phase591-contract"]=P591+"/preregistration/contract_v1.json",["phase592-summary"]=P592+"/output/companion_tensor_chirality_audit_summary.json",
 ["phase592-contract"]=P592+"/preregistration/contract_v1.json",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var doc=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=doc.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==599
  &&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
  &&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()
  &&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
  &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(v=>v.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
  &&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(v=>new Binding(v.GetProperty("id").GetString()!,v.GetProperty("path").GetString()!,v.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(v=>v.id).Distinct().Count()==paths.Count&&bindings.Select(v=>v.path).Distinct().Count()==paths.Count
  &&bindings.All(v=>paths.TryGetValue(v.id,out var p)&&p==v.path&&v.hashMatches);
 if(contractValid&&exactBindingsValid)
 {
  using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;
  var entries=m.GetProperty("files").EnumerateArray().ToArray();
  coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1
   &&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
   &&live.Length==contract.GetProperty("fixtures").GetProperty("coreFileCount").GetInt32()
   &&entries.Select(v=>v.GetProperty("path").GetString()).SequenceEqual(live)
   &&entries.All(v=>Sha(v.GetProperty("path").GetString()!)==v.GetProperty("sha256").GetString())
   &&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();
 }
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}


string[] upstreamTerminals=["fixed-domain-action-force-controls-pass-induced-pairing-scoped","canonical-source-hodge-curvature-branches-pass-choice-open","companion-tensor-chirality-controls-pass-source-choice-open"];
int[] upstreamPhases=[588,591,592];bool upstreamValid=true;
for(int index=0;index<3;index++)
{
 string prefix="phase"+upstreamPhases[index];using var doc=JsonDocument.Parse(File.ReadAllBytes(paths[prefix+"-summary"]));var u=doc.RootElement;
 upstreamValid&=u.GetProperty("auditPassed").GetBoolean()&&u.GetProperty("exactBindingsValid").GetBoolean()&&u.GetProperty("coreSourceTreeValid").GetBoolean()
  &&u.GetProperty("contractSha256").GetString()==Sha(paths[prefix+"-contract"])&&u.GetProperty("verdictKind").GetString()==upstreamTerminals[index]
  &&u.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&u.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()
  &&u.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>u.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)
  &&u.GetProperty("externalReviewPending").GetBoolean()&&u.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
}
string source=File.ReadAllText(paths["primary-source"]);upstreamValid&=new[]{"(8.1)","(8.7)","(9.3)","(9.7)","(9.11)","(9.12)"}.All(source.Contains);
if(!upstreamValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstreamValid});return;}
var fx=contract.GetProperty("fixtures");var counts=fx.GetProperty("counts");
int[] masks=Enumerable.Range(0,16384).Where(m=>fx.GetProperty("knownAnswerGrades").EnumerateArray().Any(g=>g.GetInt32()==Degree(m))).ToArray();
int cliffordWordCases=0,hodgeCases=0;bool arithmeticPassed=true;
foreach(int a in masks)foreach(int b in masks){cliffordWordCases++;arithmeticPassed&=BladeSign(a,b)==WordProductSign(a,b);}
for(int m=0;m<16384;m++){hodgeCases++;int degree=Degree(m);arithmeticPassed&=HodgeSign(m)*HodgeSign(Omega^m)==((degree*(14-degree)+7)%2==0?1:-1);}
bool knownAnswerPassed=arithmeticPassed&&cliffordWordCases==N("cliffordWordCases")&&hodgeCases==N("hodgeCases")
 &&HodgeSign(Omega)==-1&&BladeSign(Omega,Omega)==1&&new Rational(1,2)+new Rational(1,3)==new Rational(5,6)&&Scalar.I*Scalar.I==new Scalar(-1);
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,cliffordWordCases,hodgeCases});return;}

var pairs=Pairs();var pairs4=fx.GetProperty("registeredPairOrder").EnumerateArray().Select(v=>(A:v[0].GetInt32(),B:v[1].GetInt32())).ToArray();
long[][][] sigmas=new[]{"sigmaPlus","sigmaMinus"}.Select(key=>fx.GetProperty(key).EnumerateArray().Select(row=>row.EnumerateArray().Select(x=>x.GetInt64()).ToArray()).ToArray()).ToArray();
var js=new Tensor[2][];var es=new Tensor[2][];var h=Single(0,0x3f80,1);
int rawBracketChecks=0,embeddedBracketChecks=0,rawGramChecks=0,embeddedGramChecks=0,hAntiChecks=0,coreStructureEntries=0,coreGramEntries=0;
bool embeddingPassed=true;var embeddingRows=new List<object>();
var coreLie=Gu.Math.LieAlgebraFactory.CreateSu2WithTracePairing();
for(int a=0;a<3;a++)for(int b=0;b<3;b++)
{
 coreGramEntries++;embeddingPassed&=coreLie.InvariantMetric[3*a+b]==(a==b?1:0);
 for(int c=0;c<3;c++){coreStructureEntries++;embeddingPassed&=coreLie.StructureConstants[(3*a+b)*3+c]==Epsilon(a,b,c);}
}
for(int sign=0;sign<2;sign++)
{
 js[sign]=new Tensor[3];es[sign]=new Tensor[3];
 for(int axis=0;axis<3;axis++)
 {
  var j=new Tensor();for(int p=0;p<6;p++)Put(j,(0,(1<<pairs4[p].A)|(1<<pairs4[p].B)),new Scalar(new Rational(sigmas[sign][axis][p],2),0));
  js[sign][axis]=j;es[sign][axis]=ScaleTensor(j,new Scalar(new Rational(sign==0?-1:1,2),0));
  hAntiChecks++;embeddingPassed&=Add(Multiply(Dagger(es[sign][axis]),h),Multiply(h,es[sign][axis])).Count==0;
 }
 for(int a=0;a<3;a++)for(int b=0;b<3;b++)
 {
  var rawExpected=new Tensor();var embeddedExpected=new Tensor();for(int c=0;c<3;c++)
  {Append(rawExpected,ScaleTensor(js[sign][c],Epsilon(a,b,c)*(sign==0?-2:2)));Append(embeddedExpected,ScaleTensor(es[sign][c],Epsilon(a,b,c)));}
  rawBracketChecks++;embeddedBracketChecks++;rawGramChecks++;embeddedGramChecks++;
  embeddingPassed&=Same(Commutator(js[sign][a],js[sign][b]),rawExpected)&&Same(Commutator(es[sign][a],es[sign][b]),embeddedExpected)
   &&Pairing(js[sign][a],js[sign][b])==(a==b?new Rational(1,2):0)&&Pairing(es[sign][a],es[sign][b])==(a==b?new Rational(1,8):0);
 }
 embeddingRows.Add(new{embedding=sign==0?"selfdual":"antiselfdual",J=js[sign].Select(Terms).ToArray(),E=es[sign].Select(Terms).ToArray()});
}
embeddingPassed&=rawBracketChecks==N("rawBracketChecks")&&embeddedBracketChecks==N("embeddedBracketChecks")&&rawGramChecks==N("rawGramChecks")
 &&embeddedGramChecks==N("embeddedGramChecks")&&hAntiChecks==N("hAntiChecks")&&coreStructureEntries==N("coreStructureEntries")&&coreGramEntries==N("coreGramEntries");

PairMatrix[] riemann=[Weyl(sigmas[0]),Weyl(sigmas[1]),new(),Diagonal(0,1,1)];
string[] fixtureIds=fx.GetProperty("curvatureRows").EnumerateArray().Select(v=>v.GetString()!).ToArray();
int riemannComponentChecks=0,ricciEntries=0,curvatureDictionaryChecks=0;bool curvaturePassed=true;var curvatureRows=new List<object>();var fs=riemann.Select(m=>Curvature(m,pairs)).ToArray();
for(int row=0;row<4;row++)
{
 var matrix=riemann[row];
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++)
 {
  riemannComponentChecks++;long value=R(matrix,a,b,c,d);
  curvaturePassed&=value==-R(matrix,b,a,c,d)&&value==-R(matrix,a,b,d,c)&&value==R(matrix,c,d,a,b)
   &&value-R(matrix,a,c,b,d)+R(matrix,a,d,b,c)==0;
 }
 var (ricci,scalar)=RicciScalar(matrix);
 for(int a=0;a<14;a++)for(int b=0;b<14;b++){ricciEntries++;curvaturePassed&=ricci[a,b]==(row==3&&a==b&&a<2?1:0);}
 curvaturePassed&=scalar==(row==3?2:0)&&fs[row].Count==(row<2?8:row==2?0:1);
 if(row<2)
 {
  var reconstructed=new Tensor();for(int p=0;p<6;p++)for(int axis=0;axis<2;axis++)foreach(var term in js[row][axis])
   Put(reconstructed,((1<<pairs4[p].A)|(1<<pairs4[p].B),term.Key.Blade),term.Value*(axis==0?1:-1)*sigmas[row][axis][p]);
  curvatureDictionaryChecks++;curvaturePassed&=Same(reconstructed,fs[row]);
  var embedded=new Tensor();for(int p=0;p<6;p++)for(int axis=0;axis<2;axis++)foreach(var term in es[row][axis])
   Put(embedded,((1<<pairs4[p].A)|(1<<pairs4[p].B),term.Key.Blade),term.Value*(row==0?-2:2)*(axis==0?1:-1)*sigmas[row][axis][p]);
  curvatureDictionaryChecks++;curvaturePassed&=Same(embedded,fs[row]);
 }
 curvatureRows.Add(new{id=fixtureIds[row],scalar,ricci=Enumerable.Range(0,14).Select(a=>Enumerable.Range(0,14).Select(b=>ricci[a,b]).ToArray()).ToArray(),
  pairMatrix=matrix.OrderBy(x=>x.Key.Row).ThenBy(x=>x.Key.Column).Select(x=>new{row=x.Key.Row,column=x.Key.Column,value=x.Value}).ToArray(),curvature=Terms(fs[row])});
}
curvaturePassed&=riemannComponentChecks==N("riemannComponentChecks")&&ricciEntries==N("ricciEntries")&&curvatureDictionaryChecks==N("curvatureDictionaryChecks");
if(!embeddingPassed||!curvaturePassed){Emit(precedence[2],new{knownAnswerPassed,controlsPassed=false,embeddingPassed,curvaturePassed,embeddingRows,curvatureRows});return;}

var g=new Tensor();for(int a=0;a<14;a++)Put(g,(1<<a,1<<a),1);
var gamma2=new Tensor();foreach(var (a,b) in pairs){int m=(1<<a)|(1<<b);Put(gamma2,(m,m),1);}
Tensor[] phi1=[g,OmegaLeft(g)],phi2=[gamma2,ScaleTensor(OmegaLeft(gamma2),Scalar.I)];
string[] branches=fx.GetProperty("branches").EnumerateArray().Select(v=>v.GetString()!).ToArray(),monomials=fx.GetProperty("formalMonomials").EnumerateArray().Select(v=>v.GetString()!).ToArray();
int sourceFormalRows=0,sourceNonzeroRows=0,sourceZeroRows=0,planeNumericalRows=0;
bool sourcePassed=true;var sourceRows=new List<object>();Tensor[][][] sourceResults=new Tensor[4][][];
for(int row=0;row<4;row++)
{
 sourceResults[row]=new Tensor[4][];
 for(int branch=0;branch<4;branch++)
 {
  Tensor[] actual=Literal(fs[row],branches[branch]);sourceResults[row][branch]=actual;
  Tensor[] predicted=Predicted(row,branches[branch]);
  for(int slot=0;slot<6;slot++)
  {
   sourceFormalRows++;if(actual[slot].Count==0)sourceZeroRows++;else sourceNonzeroRows++;
   bool passed=Same(actual[slot],predicted[slot])&&actual[slot].Keys.All(k=>Degree(k.Form)==1&&Degree(k.Blade) is 1 or 13);
   sourcePassed&=passed;sourceRows.Add(new{fixture=fixtureIds[row],branch=branches[branch],monomial=monomials[slot],passed,terms=Terms(actual[slot])});
  }
 }
}
var planeRows=new List<object>();
foreach(var v in fx.GetProperty("planeNumericalRows").EnumerateArray())
{
 long a=v.GetProperty("a").GetInt64(),b=v.GetProperty("b").GetInt64(),c=v.GetProperty("c").GetInt64(),d=v.GetProperty("d").GetInt64();
 long[] coefficients=[a,b,a*c,b*c,a*d,b*d];var actual=new Tensor();for(int k=0;k<6;k++)Append(actual,ScaleTensor(sourceResults[3][1][k],coefficients[k]));
 var expected=new Tensor();for(int j=0;j<14;j++)
 {
  if(b==0&&j<2)Put(expected,(1<<j,1<<j),-1);
  if(b!=0&&j>=2){Put(expected,(1<<j,1<<j),1);Append(expected,ScaleTensor(OmegaLeft(Single(1<<j,1<<j,1)),b));}
 }
 planeNumericalRows++;Rational self=Pairing(actual,actual);
 bool passed=Same(actual,expected)&&actual.Count==v.GetProperty("termCount").GetInt32()&&self.ToString()==v.GetProperty("traceSelfPair").GetString();
 sourcePassed&=passed;planeRows.Add(new{id=v.GetProperty("id").GetString(),passed,termCount=actual.Count,traceSelfPair=self.ToString(),terms=Terms(actual)});
}
sourcePassed&=sourceFormalRows==N("sourceFormalRows")&&sourceNonzeroRows==N("sourceNonzeroRows")&&sourceZeroRows==N("sourceZeroRows")&&planeNumericalRows==N("planeNumericalRows");
if(!sourcePassed){Emit(precedence[3],new{knownAnswerPassed,controlsPassed=false,sourcePassed,sourceFormalRows,sourceNonzeroRows,sourceZeroRows,planeNumericalRows,sourceRows,planeRows});return;}

// Actual core API used by EinsteinianShiabOperator; dyadic operations here are exact in binary64.
var member=new EinsteinianShiabFamilyMember{Phi1=InvariantElementSpec.Sd2,Phi2=InvariantElementSpec.Id0,EinsteinCoefficient=0.5,EpsilonMode="independent-theta"};
var identityMember=new EinsteinianShiabFamilyMember{Phi1=InvariantElementSpec.Id0,Phi2=InvariantElementSpec.None};
double[,] registered=Lambda2Algebra.MemberEndomorphism(member),identity=Lambda2Algebra.MemberEndomorphism(identityMember),star=Lambda2Algebra.HodgeStar();
var expectedStar=new double[6,6];for(int p=0;p<6;p++)
{
 int mask=(1<<pairs4[p].A)|(1<<pairs4[p].B),complement=15^mask;
 int q=Array.FindIndex(pairs4,pair=>((1<<pair.A)|(1<<pair.B))==complement);expectedStar[q,p]=Shuffle(mask,complement);
}
int registeredMatrixEntries=0,registeredResidualEntries=0,registeredNormRows=0;bool registeredPassed=true;
for(int i=0;i<6;i++)for(int j=0;j<6;j++)
{
 registeredMatrixEntries+=3;registeredPassed&=star[i,j]==expectedStar[i,j]&&identity[i,j]==(i==j?1:0)
  &&registered[i,j]==((i==j?1:0)+expectedStar[i,j])/4;
}
var registeredRows=new List<object>();var residuals=new List<double[][]>();
for(int row=0;row<3;row++)
{
 var input=Enumerable.Range(0,6).Select(_=>new double[3]).ToArray();
 if(row<2)for(int p=0;p<6;p++)for(int axis=0;axis<2;axis++)input[p][axis]=(row==0?-2:2)*(axis==0?1:-1)*sigmas[row][axis][p];
 var output=Apply(registered,input);var identityOutput=Apply(identity,input);residuals.Add(output);
 for(int p=0;p<6;p++)for(int axis=0;axis<3;axis++)
 {registeredResidualEntries+=2;registeredPassed&=output[p][axis]==(row==0?input[p][axis]/2:0)&&identityOutput[p][axis]==input[p][axis];}
 double inputNorm=input.SelectMany(x=>x).Sum(x=>x*x),outputNorm=output.SelectMany(x=>x).Sum(x=>x*x);
 registeredNormRows++;registeredPassed&=inputNorm==fx.GetProperty("registered").GetProperty("expectedInputNormSquared")[row].GetDouble()
  &&outputNorm==fx.GetProperty("registered").GetProperty("expectedOutputNormSquared")[row].GetDouble()
  &&outputNorm/2==fx.GetProperty("registered").GetProperty("expectedHalfSquare")[row].GetDouble();
 registeredRows.Add(new{id=fixtureIds[row],input,output,identityOutput,inputNormSquared=inputNorm,outputNormSquared=outputNorm,halfSquare=outputNorm/2});
}
registeredPassed&=registeredMatrixEntries==N("registeredMatrixEntries")&&registeredResidualEntries==N("registeredResidualEntries")&&registeredNormRows==N("registeredNormRows");
if(!registeredPassed){Emit(precedence[4],new{knownAnswerPassed,controlsPassed=false,registeredPassed,registeredRows});return;}
bool kernelWitnessPassed=sourceResults[0].SelectMany(x=>x).All(t=>t.Count==0)&&sourceResults[2].SelectMany(x=>x).All(t=>t.Count==0)
 &&residuals[0].SelectMany(x=>x).Any(v=>v!=0)&&residuals[2].SelectMany(x=>x).All(v=>v==0);
bool controlsPassed=knownAnswerPassed&&embeddingPassed&&curvaturePassed&&sourcePassed&&registeredPassed&&kernelWitnessPassed;
Emit(controlsPassed?Success:precedence[5],new{knownAnswerPassed,controlsPassed,cliffordWordCases,hodgeCases,
 embeddingControls=new{embeddingPassed,rawBracketChecks,embeddedBracketChecks,rawGramChecks,embeddedGramChecks,hAntiChecks,coreStructureEntries,coreGramEntries,embeddingRows,pairingIsometryClaimed=false},
 curvatureControls=new{curvaturePassed,riemannComponentChecks,ricciEntries,curvatureDictionaryChecks,curvatureRows,globallyConstantConnectionCurvatureClaimed=false},
 sourceControls=new{sourcePassed,sourceFormalRows,sourceNonzeroRows,sourceZeroRows,planeNumericalRows,sourceRows,planeRows,sourceNormSelected=false},
 registeredControls=new{registeredPassed,registeredMatrixEntries,registeredResidualEntries,registeredNormRows,registeredRows,registeredMatrix=Rows(registered),star4=Rows(star),identityMatrix=Rows(identity),meshInsertionPerformed=false},
 kernelControls=new{kernelWitnessPassed,sourceResidualSameAtZeroAndWitness=true,registeredResidualDistinctAtZeroAndWitness=true,injectiveResidualIdentificationRejected=kernelWitnessPassed,
 actionModuloBoundaryEquivalenceTested=false,globalFieldLiftEstablished=false,physicalReductionSelected=false},
 nextRequirement="Specify and test a different retained-field/residual map or source operator; separately declare source action, torsion and pairing before any action/measure equivalence claim."});

int N(string key)=>counts.GetProperty(key).GetInt32();
static int Epsilon(int a,int b,int c)=>a==b||a==c||b==c?0:((a>b?1:0)+(a>c?1:0)+(b>c?1:0))%2==0?1:-1;
PairMatrix Weyl(long[][] sigma)
{
 var matrix=new PairMatrix();for(int i=0;i<6;i++)for(int j=0;j<6;j++)
 {long value=sigma[0][i]*sigma[0][j]-sigma[1][i]*sigma[1][j];if(value!=0)matrix[(Pair(pairs4[i].A,pairs4[i].B),Pair(pairs4[j].A,pairs4[j].B))]=value;}return matrix;
}
Tensor[] Literal(Tensor f,string branch)
{
 var dual=Star(f);var result=new Tensor[6];
 for(int i=0;i<2;i++)result[i]=UndoStarOne(BracketWedge(phi1[i],dual,branch[0]));
 for(int k=0;k<2;k++)
 {
  var inner=Star(BracketWedge(phi2[k],dual,branch[2]));
  for(int i=0;i<2;i++)result[2+i+2*k]=ScaleTensor(UndoStarOne(Star(BracketWedge(phi1[i],inner,branch[1]))),new Scalar(new Rational(-1,2),0));
 }
 return result;
}
Tensor[] Predicted(int row,string branch)
{
 Tensor[] result=[new(),new(),new(),new(),new(),new()];if(row!=3)return result;
 var j=Add(Single(1,1,1),Single(2,2,1));
 if(branch[0]=='C'){result[0]=ScaleTensor(j,-1);result[1]=ScaleTensor(OmegaLeft(j),-1);}
 if(branch=="CCA"){result[4]=ScaleTensor(OmegaLeft(g),-1);result[5]=ScaleTensor(g,-1);}
 if(branch=="AAA"){result[2]=g;result[3]=OmegaLeft(g);}
 return result;
}
static void Append(Tensor a,Tensor b){foreach(var x in b)Put(a,x.Key,x.Value);}
static Tensor Add(Tensor a,Tensor b){var r=new Tensor(a);Append(r,b);return r;}
static Tensor OmegaLeft(Tensor a){var r=new Tensor();foreach(var x in a)Put(r,(x.Key.Form,Omega^x.Key.Blade),x.Value*BladeSign(Omega,x.Key.Blade));return r;}
static Tensor Multiply(Tensor a,Tensor b)
{var r=new Tensor();foreach(var x in a)foreach(var y in b){if(x.Key.Form!=0||y.Key.Form!=0)throw new InvalidOperationException("Clifford-only multiplication");Put(r,(0,x.Key.Blade^y.Key.Blade),x.Value*y.Value*BladeSign(x.Key.Blade,y.Key.Blade));}return r;}
static Tensor Commutator(Tensor a,Tensor b)=>Add(Multiply(a,b),ScaleTensor(Multiply(b,a),-1));
static Tensor Dagger(Tensor a)
{var r=new Tensor();foreach(var x in a){int n=Degree(x.Key.Blade),sign=(n*(n-1)/2+Degree(x.Key.Blade&0x3f80))%2==0?1:-1;Put(r,x.Key,new Scalar(x.Value.Real,x.Value.Imaginary*-1)*sign);}return r;}
static Rational Pairing(Tensor a,Tensor b)
{Rational r=0;foreach(var x in a)foreach(var y in b)if(x.Key==y.Key)r+=(x.Value*y.Value).Real*(-BladeSign(x.Key.Blade,y.Key.Blade)*(Degree(x.Key.Form&0x3f80)%2==0?1:-1));return r;}
static double[][] Apply(double[,] matrix,double[][] input)=>Enumerable.Range(0,6).Select(i=>Enumerable.Range(0,3).Select(a=>Enumerable.Range(0,6).Sum(j=>matrix[i,j]*input[j][a])).ToArray()).ToArray();
static double[][] Rows(double[,] matrix)=>Enumerable.Range(0,matrix.GetLength(0)).Select(i=>Enumerable.Range(0,matrix.GetLength(1)).Select(j=>matrix[i,j]).ToArray()).ToArray();
void Emit(string verdict,object evidence)
{
 var result=new{schemaVersion=1,phase=599,phaseId="phase599-source-registered-residual-kernel-audit",contractId=ContractId,
 contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,
 bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,
 verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
 authorityFirewalls=firewalls.ToDictionary(x=>x,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/source_registered_residual_kernel_audit.json",json);
 File.WriteAllText(Root+"/output/source_registered_residual_kernel_audit_summary.json",json);
 Console.WriteLine($"Phase599 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
