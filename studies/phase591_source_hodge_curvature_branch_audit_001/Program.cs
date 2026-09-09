using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Tensor=System.Collections.Generic.Dictionary<(int Form,int Blade),Scalar>;
using PairMatrix=System.Collections.Generic.Dictionary<(int Row,int Column),long>;

const string Root="studies/phase591_source_hodge_curvature_branch_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=Root+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase591-a48-source-hodge-curvature-branch-v1";
const string Success="canonical-source-hodge-curvature-branches-pass-choice-open";
const string Source="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt";
const string PriorRoot="studies/phase590_source_clifford_tensor_controls_001";
const string Prior=PriorRoot+"/output/source_clifford_tensor_controls_summary.json";
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "canonicalPhi1":"lambda1 sum_a theta^a gamma_a","canonicalPhi2":"lambda2 sum_(a<b) theta^ab gamma_a gamma_b",
 "curvature":"F_ab=(1/2) sum_(c<d) R_abcd sigma_c sigma_d gamma_c gamma_d",
 "hodge":"star(theta^I)=shuffle(I,Icomplement)*product_sigma(I)*theta^Icomplement",
 "chain":"star1_inverse([Phi1 wedge star(F)]_first-(1/2)*star([Phi1 wedge star([Phi2 wedge star(F)]_inner)]_outer))",
 "brackets":{"C":"XY-YX","A":"i*(XY+YX)"},
 "primaryBranches":["CCC","CCA","AAC","AAA"],
 "enlargedBranches":["CCC","CCA","CAC","CAA","ACC","ACA","AAC","AAA"],
 "formalMonomials":["lambda1","lambda1*lambda2"],
 "basis":{"diagonalCount":91,"sharedIndexCount":1092,"quadrupleCount":1001,"bianchiRowsPerQuadruple":2,"bianchiRowCount":2002,"totalCount":3185,
   "quadrupleCoordinates":"t1=R_abcd,t2=R_acbd,t3=R_adbc; t1-t2+t3=0",
   "quadrupleRows":[[1,1,0],[0,1,1]],"pivotRule":"diagonal or shared coordinate; quadruple row1 t1 and row2 t3"},
 "anchors":["flat","positive-plane-01","mixed-plane-07","constant-K1","nonzero-Weyl-01021323"],
 "weylDiagonalPlanes":[[0,1,1],[0,2,-1],[1,3,-1],[2,3,1]],
 "constantCurvatureExpectedRaisedRicci":13,"constantCurvatureExpectedScalar":182,
 "offRiemannDecoys":["M_01_02=1 only; no symmetric transpose","M_01_23=M_23_01=1; other disjoint coordinates zero"],
 "expectedBasisBranchCases":25480,"expectedBasisPrimaryCases":12740,"expectedAnchorCases":5,
 "knownAnswerCliffordLeftMaximumGrade":2,"knownAnswerCliffordRightMaximumGrade":4,"expectedCliffordWordCases":155926,
 "expectedHodgeCases":16384,"exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":45,"maximumEstimatedCpuSeconds":60,"estimatedPeakBytes":268435456,"maximumEstimatedPeakBytes":536870912}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
 "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
 "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","riemann-basis-control-failed","literal-contraction-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["project"]=Root+"/Phase591SourceHodgeCurvatureBranchAudit.csproj",
 ["study"]=Root+"/STUDY.md",["primary-source"]=Source,["phase590-summary"]=Prior,["phase590-contract"]=PriorRoot+"/preregistration/contract_v1.json",
 ["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var d=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=d.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==591
  &&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
  &&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()
  &&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
  &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
  &&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(x=>new Binding(x.GetProperty("id").GetString()!,x.GetProperty("path").GetString()!,x.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(x=>x.id).Distinct().Count()==paths.Count&&bindings.Select(x=>x.path).Distinct().Count()==paths.Count
  &&bindings.All(x=>paths.TryGetValue(x.id,out var p)&&p==x.path&&x.hashMatches);
 if(contractValid&&exactBindingsValid)
 {
  using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;
  var entries=m.GetProperty("files").EnumerateArray().ToArray();
  coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1
   &&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
   &&live.Length==contract.GetProperty("fixtures").GetProperty("coreFileCount").GetInt32()
   &&entries.Select(x=>x.GetProperty("path").GetString()).SequenceEqual(live)
   &&entries.All(x=>Sha(x.GetProperty("path").GetString()!)==x.GetProperty("sha256").GetString())
   &&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();
 }
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false});return;}
var fx=contract.GetProperty("fixtures");

// Independent arithmetic controls, before interpreting scientific upstream/source content.
bool cliffordPassed=true,hodgePassed=true;int cliffordWordCases=0,hodgeCases=0;
var leftMasks=Enumerable.Range(0,1<<14).Where(m=>BitOperations.PopCount((uint)m)<=2).ToArray();
var rightMasks=Enumerable.Range(0,1<<14).Where(m=>BitOperations.PopCount((uint)m)<=4).ToArray();
foreach(int a in leftMasks)foreach(int b in rightMasks)
{cliffordWordCases++;cliffordPassed&=BladeSign(a,b)==WordProductSign(a,b);}
for(int mask=0;mask<(1<<14);mask++)
{
 hodgeCases++;int degree=BitOperations.PopCount((uint)mask),complement=((1<<14)-1)^mask;
 int squared=HodgeSign(mask)*HodgeSign(complement),predicted=((degree*(14-degree)+7)%2==0)?1:-1;
 hodgePassed&=squared==predicted;
}
for(int a=0;a<14;a++)for(int b=0;b<14;b++)
 cliffordPassed&=BladeSign(1<<a,1<<b)+BladeSign(1<<b,1<<a)==(a==b?2*Sigma(a):0);
bool knownAnswerPassed=cliffordPassed&&hodgePassed&&HodgeSign((1<<14)-1)==-1&&HodgeSign(0)==1
 &&cliffordWordCases==fx.GetProperty("expectedCliffordWordCases").GetInt32()&&hodgeCases==fx.GetProperty("expectedHodgeCases").GetInt32()
 &&new Rational(1,2)+new Rational(1,3)==new Rational(5,6)&&Scalar.I*Scalar.I==new Scalar(-1)
 &&Enumerable.Range(0,14).All(a=>Same(UndoStarOne(Star(Single(1<<a,1<<a,1))),Single(1<<a,1<<a,1)));
if(!knownAnswerPassed){Emit(precedence[1],new {knownAnswerPassed,controlsPassed=false,cliffordPassed,hodgePassed,cliffordWordCases,hodgeCases});return;}
using var priorDoc=JsonDocument.Parse(File.ReadAllBytes(Prior));var prior=priorDoc.RootElement;
bool upstreamValid=prior.GetProperty("auditPassed").GetBoolean()
 &&prior.GetProperty("verdictKind").GetString()=="mixed-signature-clifford-tensor-controls-pass-source-choice-open"
 &&prior.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
string source=File.ReadAllText(Source);bool sourceAnchorsPresent=new[]{"(8.1)","(8.5)","(8.6)","(8.7)","(9.2)","(9.3)"}.All(source.Contains);
if(!upstreamValid||!sourceAnchorsPresent){Emit(precedence[0],new {knownAnswerPassed,controlsPassed=false,upstreamValid,sourceAnchorsPresent});return;}

var pairs=Pairs();var quadruples=Quadruples();var basis=RiemannBasis(pairs,quadruples);
var pivotSet=basis.Select(x=>x.Pivot).ToHashSet();
bool independent=basis.Count==pivotSet.Count&&basis.All(x=>x.Matrix.TryGetValue(x.Pivot,out long coefficient)&&coefficient==1
 &&x.Matrix.Keys.Where(k=>k.Row<=k.Column&&pivotSet.Contains(k)).All(k=>k==x.Pivot));
int diagonalCount=basis.Count(x=>x.Family=="diagonal"),sharedCount=basis.Count(x=>x.Family=="shared-index"),bianchiCount=basis.Count(x=>x.Family=="bianchi-quadruple");
bool pairSymmetryPassed=true,bianchiPassed=true;long maxBianchiResidual=0;
foreach(var item in basis)
{
 pairSymmetryPassed&=item.Matrix.All(x=>Get(item.Matrix,x.Key.Column,x.Key.Row)==x.Value);
 foreach(var q in quadruples)
 {long value=checked(R(item.Matrix,q.A,q.B,q.C,q.D)-R(item.Matrix,q.A,q.C,q.B,q.D)+R(item.Matrix,q.A,q.D,q.B,q.C));
  maxBianchiResidual=System.Math.Max(maxBianchiResidual,System.Math.Abs(value));}
}
bianchiPassed=maxBianchiResidual==0;
bool basisPassed=independent&&pairSymmetryPassed&&bianchiPassed&&diagonalCount==91&&sharedCount==1092&&bianchiCount==2002
 &&quadruples.Count==1001&&basis.Count==3185&&pairs.Count*(pairs.Count+1)/2-quadruples.Count==basis.Count;
if(!basisPassed){Emit(precedence[2],new {knownAnswerPassed,controlsPassed=false,basisPassed,independent,pairSymmetryPassed,bianchiPassed,
 diagonalCount,sharedCount,bianchiCount,maxBianchiResidual});return;}

// The two canonical tensor directions only. Formal lambdas are retained as separate output monomials.
var phi1=new Tensor();for(int a=0;a<14;a++)Put(phi1,(1<<a,1<<a),1);
var phi2=new Tensor();foreach(var (a,b) in pairs){int mask=(1<<a)|(1<<b);Put(phi2,(mask,mask),1);}
string[] branches=fx.GetProperty("enlargedBranches").EnumerateArray().Select(x=>x.GetString()!).ToArray();
string[] primary=fx.GetProperty("primaryBranches").EnumerateArray().Select(x=>x.GetString()!).ToArray();
var basisRows=new List<object>();bool branchControlsPassed=true,typedControlsPassed=true;int basisBranchCases=0,basisPrimaryCases=0;
foreach(var item in basis)
{
 var result=Audit(item.Matrix);branchControlsPassed&=result.Passed;typedControlsPassed&=result.Typed;
 basisBranchCases+=branches.Length;basisPrimaryCases+=primary.Length;
 basisRows.Add(new {id=item.Id,family=item.Family,pivot=new[]{item.Pivot.Row,item.Pivot.Column},supportCount=item.Matrix.Count,
  scalar=result.Scalar,ricciNonzeroCount=result.Ricci.Cast<long>().Count(x=>x!=0),result.Passed,result.Typed,result.Hash});
}
var anchors=new List<(string Id,PairMatrix Matrix)>();
anchors.Add(("flat",new PairMatrix()));anchors.Add(("positive-plane-01",Diagonal(0,1,1)));anchors.Add(("mixed-plane-07",Diagonal(0,7,1)));
var constant=new PairMatrix();foreach(var (a,b) in pairs)constant[(Pair(a,b),Pair(a,b))]=Sigma(a)*Sigma(b);
anchors.Add(("constant-K1",constant));var weyl=new PairMatrix();foreach(var row in fx.GetProperty("weylDiagonalPlanes").EnumerateArray())
 weyl[(Pair(row[0].GetInt32(),row[1].GetInt32()),Pair(row[0].GetInt32(),row[1].GetInt32()))]=row[2].GetInt32();
anchors.Add(("nonzero-Weyl-01021323",weyl));
var anchorRows=new List<object>();bool anchorControlsPassed=true;
foreach(var anchor in anchors)
{
 var result=Audit(anchor.Matrix);bool expected=result.Passed&&result.Typed;
 if(anchor.Id=="flat")expected&=Curvature(anchor.Matrix,pairs).Count==0&&result.Scalar==0&&result.Ricci.Cast<long>().All(x=>x==0);
 if(anchor.Id=="positive-plane-01")expected&=result.Scalar==2&&result.Ricci[0,0]==1&&result.Ricci[1,1]==1&&result.Ricci.Cast<long>().Count(x=>x!=0)==2;
 if(anchor.Id=="mixed-plane-07")expected&=result.Scalar==-2&&result.Ricci[0,0]==-1&&result.Ricci[7,7]==1&&result.Ricci.Cast<long>().Count(x=>x!=0)==2;
 if(anchor.Id=="constant-K1")expected&=result.Scalar==182&&Enumerable.Range(0,14).All(a=>Enumerable.Range(0,14).All(b=>result.Ricci[a,b]==(a==b?13*Sigma(a):0)));
 if(anchor.Id.StartsWith("nonzero-Weyl",StringComparison.Ordinal))expected&=Curvature(anchor.Matrix,pairs).Count>0&&result.Scalar==0&&result.Ricci.Cast<long>().All(x=>x==0);
 anchorControlsPassed&=expected;
 anchorRows.Add(new {id=anchor.Id,expectedPassed=expected,scalar=result.Scalar,ricci=Enumerable.Range(0,14).Select(a=>Enumerable.Range(0,14).Select(b=>result.Ricci[a,b]).ToArray()).ToArray(),
  result.Passed,result.Typed,result.Hash,branches=result.Rows});
}

// Off-Riemann controls keep antisymmetry within each pair, but break distinct additional premises.
var asymmetric=new PairMatrix{[(Pair(0,1),Pair(0,2))]=1};
var bianchiWrong=new PairMatrix();SetSym(bianchiWrong,Pair(0,1),Pair(2,3),1);
var aF=Curvature(asymmetric,pairs);var bF=Curvature(bianchiWrong,pairs);
var asymmetricInner=Star(BracketWedge(phi2,Star(aF),'C'));
var bianchiFirst=UndoStarOne(BracketWedge(phi1,Star(bF),'A'));
var bianchiInner=Star(BracketWedge(phi2,Star(bF),'A'));
long asymmetricPairDefect=Get(asymmetric,Pair(0,1),Pair(0,2))-Get(asymmetric,Pair(0,2),Pair(0,1));
long bianchiDefect=R(bianchiWrong,0,1,2,3)-R(bianchiWrong,0,2,1,3)+R(bianchiWrong,0,3,1,2);
// A wrong top-star sign flips the nonzero scalar inner contraction on the positive plane.
var planeF=Curvature(anchors[1].Matrix,pairs);var trueInner=Star(BracketWedge(phi2,Star(planeF),'A'));
var wrongTopInner=ScaleTensor(trueInner,-1);
bool decoysPassed=asymmetricPairDefect==1&&asymmetricInner.Count>0&&asymmetricInner.Keys.All(k=>BitOperations.PopCount((uint)k.Blade)==2)
 &&bianchiDefect==1&&bianchiFirst.Count>0&&bianchiFirst.Keys.All(k=>BitOperations.PopCount((uint)k.Blade)==3)
 &&bianchiInner.Count>0&&bianchiInner.Keys.All(k=>BitOperations.PopCount((uint)k.Blade)==4)
 &&Same(trueInner,Single(0,0,Scalar.I))&&!Same(wrongTopInner,trueInner);
bool controlsPassed=basisPassed&&branchControlsPassed&&typedControlsPassed&&anchorControlsPassed&&decoysPassed
 &&basisBranchCases==fx.GetProperty("expectedBasisBranchCases").GetInt32()&&basisPrimaryCases==fx.GetProperty("expectedBasisPrimaryCases").GetInt32()
 &&anchors.Count==fx.GetProperty("expectedAnchorCases").GetInt32();
Emit(controlsPassed?Success:precedence[3],new {
 knownAnswerPassed,controlsPassed,upstreamValid,sourceAnchorsPresent,
 arithmeticControls=new {cliffordPassed,hodgePassed,cliffordWordCases,hodgeCases,topStar=-1,inverseStarOneIsStarThirteen=true,
  arithmetic="checked-int64-reduced-rational-complex",exactTolerance=0,denseOperatorDiagonalizationPerformed=false},
 basisControls=new {basisPassed,independent,pairSymmetryPassed,bianchiPassed,diagonalCount,sharedCount,bianchiCount,quadrupleCount=quadruples.Count,
  basisCount=basis.Count,uniquePivotCount=pivotSet.Count,maxBianchiResidual,basisRows},
 literalBranches=new {branchControlsPassed,typedControlsPassed,basisBranchCases,basisPrimaryCases,primaryBranches=primary,enlargedDiagnosticBranches=branches,
  formalMonomials=new[]{"lambda1","lambda1*lambda2"},firstCommutator="-sum_d Ric_jd*sigma_d*gamma_d",
  firstIAnticommutator="0 by algebraic Bianchi",innerCommutator="0 by pair symmetry",innerIAnticommutator="(i/2)*Scal*I",
  primaryPhi1OccurrenceChoicesTied=true,enlargedCaaAuthorPermittedOrSelected=false,canonicalInvariantSubfamilyOnly=true,
  noncanonicalVolumeDualInvariantDirectionsRemainOpen=true,zeroBranchesClaimedOnAllAdValuedTwoForms=false},
 anchors=new {anchorControlsPassed,anchorRows},
 offRiemannControls=new {decoysPassed,asymmetricPairDefect,bianchiDefect,asymmetricInner=Terms(asymmetricInner),bianchiFirst=Terms(bianchiFirst),
  bianchiInner=Terms(bianchiInner),trueInner=Terms(trueInner),wrongTopInner=Terms(wrongTopInner),offSubspacePreservationEstablished=false},
 nextRequirement="Retain noncanonical invariant tensor directions and bracket/normalization freedom; specify the actual source operator, dimensional reduction, action and pairing before any registered or physical interpretation."
});

(bool Passed,bool Typed,long Scalar,long[,] Ricci,string Hash,object[] Rows) Audit(PairMatrix matrix)
{
 var curvature=Curvature(matrix,pairs);var dual=Star(curvature);
 var firstC=UndoStarOne(BracketWedge(phi1,dual,'C'));var firstA=UndoStarOne(BracketWedge(phi1,dual,'A'));
 var innerC=Star(BracketWedge(phi2,dual,'C'));var innerA=Star(BracketWedge(phi2,dual,'A'));
 var (ricci,scalar)=RicciScalar(matrix);var ricciTarget=new Tensor();
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)Put(ricciTarget,(1<<a,1<<b),-ricci[a,b]*Sigma(b));
 var scalarTarget=new Tensor();for(int a=0;a<14;a++)Put(scalarTarget,(1<<a,1<<a),new Scalar(new Rational(scalar,2),0));
 var innerTarget=Single(0,0,new Scalar(0,new Rational(scalar,2)));
 bool passed=Same(firstC,ricciTarget)&&firstA.Count==0&&innerC.Count==0&&Same(innerA,innerTarget);
 bool typed=curvature.Keys.All(k=>Degree(k.Form)==2&&Degree(k.Blade)==2)&&dual.Keys.All(k=>Degree(k.Form)==12);
 var rows=new List<object>();var canonical=new StringBuilder();
 foreach(string branch in branches)
 {
  var first=branch[0]=='C'?firstC:firstA;var inner=branch[2]=='C'?innerC:innerA;
  // Literal outer star produces degree13; inverse of star on degree1 returns the final one-form.
  var rawOuter=Star(BracketWedge(phi1,inner,branch[1]));var second=ScaleTensor(UndoStarOne(rawOuter),new Scalar(new Rational(-1,2),0));
  var predictedFirst=branch[0]=='C'?ricciTarget:new Tensor();var predictedSecond=branch[1]=='A'&&branch[2]=='A'?scalarTarget:new Tensor();
  bool rowPassed=Same(first,predictedFirst)&&Same(second,predictedSecond);
  bool rowTyped=first.Concat(second).All(x=>Degree(x.Key.Form)==1&&Degree(x.Key.Blade)==1)
   &&rawOuter.Keys.All(k=>Degree(k.Form)==13);
  passed&=rowPassed;typed&=rowTyped;canonical.Append(branch).Append(':').Append(Canonical(first)).Append('|').Append(Canonical(second)).Append('\n');
  rows.Add(new {branch,primary=primary.Contains(branch),lambda1=Terms(first),lambda1Lambda2=Terms(second),rowPassed,rowTyped});
 }
 return (passed,typed,scalar,ricci,Hash(canonical.ToString()),rows.ToArray());
}
void Emit(string verdict,object evidence)
{
 var result=new {schemaVersion=1,phase=591,phaseId="phase591-source-hodge-curvature-branch-audit",contractId=ContractId,
  contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,
  bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,
  verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
  authorityFirewalls=firewalls.ToDictionary(x=>x,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/source_hodge_curvature_branch_audit.json",json);
 File.WriteAllText(Root+"/output/source_hodge_curvature_branch_audit_summary.json",json);
 Console.WriteLine($"Phase591 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
static int Degree(int mask)=>BitOperations.PopCount((uint)mask);
static int Sigma(int a)=>a<7?1:-1;
static int Shuffle(int a,int b){int inversions=0;for(int i=0;i<14;i++)if((a&(1<<i))!=0)inversions+=Degree(b&((1<<i)-1));return inversions%2==0?1:-1;}
static int BladeSign(int a,int b)=>Shuffle(a,b)*(Degree(a&b&0x3f80)%2==0?1:-1);
static int WordProductSign(int a,int b)
{
 var word=Enumerable.Range(0,14).Where(i=>(a&(1<<i))!=0).ToList();int sign=1;
 foreach(int i in Enumerable.Range(0,14).Where(i=>(b&(1<<i))!=0))
 {int larger=word.Count(j=>j>i);if(larger%2!=0)sign=-sign;if(word.Contains(i)){word.Remove(i);sign*=Sigma(i);}else {word.Add(i);word.Sort();}}
 return sign;
}
static int HodgeSign(int mask)=>Shuffle(mask,((1<<14)-1)^mask)*(Degree(mask&0x3f80)%2==0?1:-1);
static void Put(Tensor t,(int Form,int Blade) key,Scalar value)
{if(value.IsZero)return;Scalar sum=t.GetValueOrDefault(key)+value;if(sum.IsZero)t.Remove(key);else t[key]=sum;}
static Tensor Single(int form,int blade,Scalar value){var t=new Tensor();Put(t,(form,blade),value);return t;}
static Tensor ScaleTensor(Tensor a,Scalar scale){var r=new Tensor();foreach(var x in a)Put(r,x.Key,x.Value*scale);return r;}
static Tensor Star(Tensor a){var r=new Tensor();foreach(var x in a)Put(r,(((1<<14)-1)^x.Key.Form,x.Key.Blade),x.Value*HodgeSign(x.Key.Form));return r;}
static Tensor UndoStarOne(Tensor a)
{if(a.Keys.Any(k=>Degree(k.Form)!=13))throw new InvalidOperationException("Expected degree13 before inverse star1");return Star(a);}
static Tensor BracketWedge(Tensor a,Tensor b,char bracket)
{
 var r=new Tensor();foreach(var x in a)foreach(var y in b)
 {
  if((x.Key.Form&y.Key.Form)!=0)continue;
  int xy=BladeSign(x.Key.Blade,y.Key.Blade),yx=BladeSign(y.Key.Blade,x.Key.Blade);
  int coefficient=Shuffle(x.Key.Form,y.Key.Form)*(bracket=='C'?xy-yx:xy+yx);
  Scalar value=x.Value*y.Value*coefficient;if(bracket=='A')value*=Scalar.I;
  Put(r,(x.Key.Form|y.Key.Form,x.Key.Blade^y.Key.Blade),value);
 }
 return r;
}
static bool Same(Tensor a,Tensor b)=>a.Count==b.Count&&a.All(x=>b.TryGetValue(x.Key,out var y)&&x.Value==y);
static string Canonical(Tensor t)=>string.Join(";",t.OrderBy(x=>x.Key.Form).ThenBy(x=>x.Key.Blade).Select(x=>$"{x.Key.Form},{x.Key.Blade}:{x.Value.Real}:{x.Value.Imaginary}"));
static object[] Terms(Tensor t)=>t.OrderBy(x=>x.Key.Form).ThenBy(x=>x.Key.Blade).Select(x=>(object)new {formMask=x.Key.Form,cliffordMask=x.Key.Blade,real=x.Value.Real.ToString(),imaginary=x.Value.Imaginary.ToString()}).ToArray();
static List<(int A,int B)> Pairs(){var p=new List<(int,int)>();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)p.Add((a,b));return p;}
static List<(int A,int B,int C,int D)> Quadruples(){var q=new List<(int,int,int,int)>();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)for(int c=b+1;c<14;c++)for(int d=c+1;d<14;d++)q.Add((a,b,c,d));return q;}
static int Pair(int a,int b)=>a*(27-a)/2+b-a-1;
static long Get(PairMatrix m,int i,int j)=>m.GetValueOrDefault((i,j));
static long R(PairMatrix m,int a,int b,int c,int d)
{if(a==b||c==d)return 0;int sign=1;if(a>b){(a,b)=(b,a);sign=-sign;}if(c>d){(c,d)=(d,c);sign=-sign;}return checked(sign*Get(m,Pair(a,b),Pair(c,d)));}
static void SetSym(PairMatrix m,int a,int b,long value){m[(a,b)]=value;m[(b,a)]=value;}
static PairMatrix Diagonal(int a,int b,long value)=>new(){[(Pair(a,b),Pair(a,b))]=value};
static List<BasisRow> RiemannBasis(List<(int A,int B)> pairs,List<(int A,int B,int C,int D)> quads)
{
 var rows=new List<BasisRow>();
 for(int i=0;i<pairs.Count;i++)rows.Add(new($"d-{i}","diagonal",(i,i),new PairMatrix{[(i,i)]=1}));
 for(int i=0;i<pairs.Count;i++)for(int j=i+1;j<pairs.Count;j++)
 {
  var a=pairs[i];var b=pairs[j];if(a.A!=b.A&&a.A!=b.B&&a.B!=b.A&&a.B!=b.B)continue;
  var m=new PairMatrix();SetSym(m,i,j,1);rows.Add(new($"s-{i}-{j}","shared-index",(i,j),m));
 }
 foreach(var q in quads)
 {
  var t1=(Pair(q.A,q.B),Pair(q.C,q.D));var t2=(Pair(q.A,q.C),Pair(q.B,q.D));var t3=(Pair(q.A,q.D),Pair(q.B,q.C));
  var m1=new PairMatrix();SetSym(m1,t1.Item1,t1.Item2,1);SetSym(m1,t2.Item1,t2.Item2,1);
  var m2=new PairMatrix();SetSym(m2,t2.Item1,t2.Item2,1);SetSym(m2,t3.Item1,t3.Item2,1);
  rows.Add(new($"q-{q.A}-{q.B}-{q.C}-{q.D}-1","bianchi-quadruple",t1,m1));
  rows.Add(new($"q-{q.A}-{q.B}-{q.C}-{q.D}-2","bianchi-quadruple",t3,m2));
 }
 return rows;
}
static Tensor Curvature(PairMatrix matrix,List<(int A,int B)> pairs)
{
 var t=new Tensor();foreach(var entry in matrix)
 {var ab=pairs[entry.Key.Row];var cd=pairs[entry.Key.Column];Put(t,((1<<ab.A)|(1<<ab.B),(1<<cd.A)|(1<<cd.B)),
  new Scalar(new Rational(checked(entry.Value*Sigma(cd.A)*Sigma(cd.B)),2),0));}return t;
}
static (long[,] Ricci,long Scalar) RicciScalar(PairMatrix m)
{
 var ricci=new long[14,14];for(int b=0;b<14;b++)for(int d=0;d<14;d++)for(int a=0;a<14;a++)ricci[b,d]=checked(ricci[b,d]+Sigma(a)*R(m,a,b,a,d));
 long scalar=0;for(int a=0;a<14;a++)scalar=checked(scalar+Sigma(a)*ricci[a,a]);return (ricci,scalar);
}
sealed record BasisRow(string Id,string Family,(int Row,int Column) Pivot,PairMatrix Matrix);
readonly struct Rational : IEquatable<Rational>
{
 private readonly long n,d;public long Numerator=>n;public long Denominator=>d==0?1:d;
 public Rational(long numerator,long denominator=1)
 {if(denominator==0)throw new DivideByZeroException();if(denominator<0){numerator=checked(-numerator);denominator=checked(-denominator);}
  long a=System.Math.Abs(numerator),b=denominator;while(b!=0){long r=a%b;a=b;b=r;}n=numerator/a;d=denominator/a;}
 public static implicit operator Rational(long n)=>new(n);
 public static Rational operator +(Rational a,Rational b)=>new(checked(a.n*b.Denominator+b.n*a.Denominator),checked(a.Denominator*b.Denominator));
 public static Rational operator -(Rational a,Rational b)=>new(checked(a.n*b.Denominator-b.n*a.Denominator),checked(a.Denominator*b.Denominator));
 public static Rational operator *(Rational a,Rational b)=>new(checked(a.n*b.n),checked(a.Denominator*b.Denominator));
 public bool Equals(Rational x)=>n==x.n&&Denominator==x.Denominator;public override bool Equals(object? x)=>x is Rational r&&Equals(r);
 public override int GetHashCode()=>HashCode.Combine(n,Denominator);public static bool operator ==(Rational a,Rational b)=>a.Equals(b);public static bool operator !=(Rational a,Rational b)=>!a.Equals(b);
 public override string ToString()=>Denominator==1?n.ToString(System.Globalization.CultureInfo.InvariantCulture):$"{n}/{Denominator}";
}
readonly struct Scalar(Rational real,Rational imaginary) : IEquatable<Scalar>
{
 public Rational Real{get;}=real;public Rational Imaginary{get;}=imaginary;
 public Scalar(long real):this(new Rational(real),0){}public static Scalar I=>new(0,1);public bool IsZero=>Real.Numerator==0&&Imaginary.Numerator==0;
 public static implicit operator Scalar(long value)=>new(value);
 public static Scalar operator +(Scalar a,Scalar b)=>new(a.Real+b.Real,a.Imaginary+b.Imaginary);
 public static Scalar operator *(Scalar a,Scalar b)=>new(a.Real*b.Real-a.Imaginary*b.Imaginary,a.Real*b.Imaginary+a.Imaginary*b.Real);
 public bool Equals(Scalar x)=>Real==x.Real&&Imaginary==x.Imaginary;public override bool Equals(object? x)=>x is Scalar s&&Equals(s);public override int GetHashCode()=>HashCode.Combine(Real,Imaginary);
 public static bool operator ==(Scalar a,Scalar b)=>a.Equals(b);public static bool operator !=(Scalar a,Scalar b)=>!a.Equals(b);
}
sealed class Binding(string idValue,string pathValue,string hashValue)
{
 public string id{get;}=idValue;public string path{get;}=pathValue;public string sha256{get;}=hashValue;
 public bool hashMatches=>File.Exists(path)&&Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()==sha256;
}
