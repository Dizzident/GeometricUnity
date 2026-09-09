using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using Tensor=System.Collections.Generic.Dictionary<(int Form,int Blade),Scalar>;
using PairMatrix=System.Collections.Generic.Dictionary<(int Row,int Column),long>;

const string Root="studies/phase592_companion_tensor_chirality_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=Root+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase592-a49-companion-tensor-chirality-v1";
const string Success="companion-tensor-chirality-controls-pass-source-choice-open";
const string Source="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt";
const string P590="studies/phase590_source_clifford_tensor_controls_001";
const string P591="studies/phase591_source_hodge_curvature_branch_audit_001";
const int Omega=(1<<14)-1;
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "phi1":"(a+b*Omega)*G","phi2":"(c+i*d*Omega)*Gamma2","parameters":"a,b,c,d formal real",
 "curvature":"F_ab=(1/2) sum_(c<d) R_abcd sigma_c sigma_d gamma_c gamma_d",
 "hodge":"star(theta^I)=shuffle(I,Icomplement)*product_sigma(I)*theta^Icomplement",
 "chain":"star1_inverse([Phi1 wedge star(F)]_first-(1/2)*star([Phi1 wedge star([Phi2 wedge star(F)]_inner)]_outer))",
 "brackets":{"C":"XY-YX","A":"i*(XY+YX)"},"branches":["CCC","CCA","AAC","AAA"],
 "formalMonomials":["a","b","ac","bc","ad","bd"],
 "basis":{"diagonalCount":91,"sharedIndexCount":1092,"quadrupleCount":1001,"bianchiRowCount":2002,"totalCount":3185,
  "quadrupleCoordinates":"t1=R_abcd,t2=R_acbd,t3=R_adbc; t1-t2+t3=0","quadrupleRows":[[1,1,0],[0,1,1]]},
 "anchors":["flat","positive-plane-01","mixed-plane-07","constant-K1","nonzero-Weyl-01021323"],
 "weylDiagonalPlanes":[[0,1,1],[0,2,-1],[1,3,-1],[2,3,1]],
 "parameterRows":[
  {"id":"chiral-d-plus","a":1,"b":-1,"c":2,"d":1,"matches":true,"invertible":false},
  {"id":"chiral-d-minus","a":1,"b":1,"c":-3,"d":-1,"matches":true,"invertible":false},
  {"id":"canonical-invertible","a":1,"b":0,"c":1,"d":0,"matches":false,"invertible":true},
  {"id":"mixed-invertible","a":2,"b":1,"c":-2,"d":1,"matches":false,"invertible":true},
  {"id":"null-wrong-d","a":1,"b":1,"c":0,"d":1,"matches":false,"invertible":false},
  {"id":"trivial-zero","a":0,"b":0,"c":7,"d":3,"matches":true,"invertible":false}
 ],
 "matchingEquations":["a+b*d=0","b+a*d=0"],"normPolynomial":"14*(-a^2+b^2)",
 "scalarFlatRicciPlanes":[[0,1,1],[0,2,-1]],
 "chiralitySigns":[-1,1],"axisCount":14,"expectedChiralityCases":28,
 "negativeControls":["opposite-companion-sign","same-input-output-projector","null-wrong-d","invertible-universal-match","self-pairing-implies-mixed-pairing-zero","scalar-flat-enforces-parameters"],
 "knownAnswerGrades":[0,1,2,12,13,14],"expectedCliffordWordCases":44944,"expectedHodgeCases":16384,
 "expectedBasisBranchCases":12740,"expectedBasisCoefficientCases":76440,"expectedAnchorCases":5,
 "exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":30,"maximumEstimatedCpuSeconds":60,"estimatedPeakBytes":268435456,"maximumEstimatedPeakBytes":536870912}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
 "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
 "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","riemann-basis-control-failed","companion-branch-control-failed","parameter-chirality-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["algebra-helper"]=Root+"/ExactAlgebra.cs",
 ["project"]=Root+"/Phase592CompanionTensorChiralityAudit.csproj",["study"]=Root+"/STUDY.md",["primary-source"]=Source,
 ["phase590-summary"]=P590+"/output/source_clifford_tensor_controls_summary.json",["phase590-contract"]=P590+"/preregistration/contract_v1.json",
 ["phase591-summary"]=P591+"/output/source_hodge_curvature_branch_audit_summary.json",["phase591-contract"]=P591+"/preregistration/contract_v1.json",
 ["phase591-program"]=P591+"/Program.cs",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var d=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=d.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==592
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


// Read-only provenance check: original methods/types match after the declared wrapper and visibility change.
string original=File.ReadAllText(P591+"/Program.cs"),helper=File.ReadAllText(Root+"/ExactAlgebra.cs");
bool helperParity=original[original.IndexOf("static int Degree",StringComparison.Ordinal)..original.IndexOf("sealed record BasisRow",StringComparison.Ordinal)].Trim()
 ==helper[helper.IndexOf("public static int Degree",StringComparison.Ordinal)..helper.IndexOf("\n}\nsealed record BasisRow",StringComparison.Ordinal)].Replace("public static ","static ",StringComparison.Ordinal).Trim()
 &&original[original.IndexOf("sealed record BasisRow",StringComparison.Ordinal)..].Trim()==helper[helper.IndexOf("sealed record BasisRow",StringComparison.Ordinal)..].Trim();
bool upstreamValid=true;
foreach(var row in new[]{(paths["phase590-summary"],"mixed-signature-clifford-tensor-controls-pass-source-choice-open"),
 (paths["phase591-summary"],"canonical-source-hodge-curvature-branches-pass-choice-open")})
{
 using var doc=JsonDocument.Parse(File.ReadAllBytes(row.Item1));var p=doc.RootElement;
 upstreamValid&=p.GetProperty("auditPassed").GetBoolean()&&p.GetProperty("verdictKind").GetString()==row.Item2
  &&p.GetProperty("coreSourceTreeValid").GetBoolean()&&p.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()
  &&p.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&p.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
}
string source=File.ReadAllText(Source);
bool sourceAnchorsPresent=new[]{"(8.1)","(8.5)","(8.6)","(8.7)","(9.3)","(9.4)"}.All(source.Contains);
if(!helperParity||!upstreamValid||!sourceAnchorsPresent){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,helperParity,upstreamValid,sourceAnchorsPresent});return;}

int[] grades=fx.GetProperty("knownAnswerGrades").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
int[] masks=Enumerable.Range(0,1<<14).Where(m=>grades.Contains(Degree(m))).ToArray();
int cliffordWordCases=0,hodgeCases=0;bool cliffordPassed=true,hodgePassed=true;
foreach(int a in masks)foreach(int b in masks){cliffordWordCases++;cliffordPassed&=BladeSign(a,b)==WordProductSign(a,b);}
for(int mask=0;mask<=Omega;mask++)
{hodgeCases++;int r=Degree(mask);hodgePassed&=HodgeSign(mask)*HodgeSign(Omega^mask)==((r*(14-r)+7)%2==0?1:-1);}
bool knownAnswerPassed=cliffordPassed&&hodgePassed&&cliffordWordCases==fx.GetProperty("expectedCliffordWordCases").GetInt32()
 &&hodgeCases==fx.GetProperty("expectedHodgeCases").GetInt32()&&BladeSign(Omega,Omega)==1&&HodgeSign(Omega)==-1
 &&new Rational(1,2)+new Rational(1,3)==new Rational(5,6)&&Scalar.I*Scalar.I==new Scalar(-1);
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,cliffordPassed,hodgePassed,cliffordWordCases,hodgeCases});return;}

var pairs=Pairs();var quads=Quadruples();var basis=RiemannBasis(pairs,quads);var pivots=basis.Select(x=>x.Pivot).ToHashSet();
bool independent=basis.Count==pivots.Count&&basis.All(x=>Get(x.Matrix,x.Pivot.Row,x.Pivot.Column)==1
 &&x.Matrix.Keys.Where(k=>k.Row<=k.Column&&pivots.Contains(k)).All(k=>k==x.Pivot));
bool pairSymmetryPassed=basis.All(x=>x.Matrix.All(v=>Get(x.Matrix,v.Key.Column,v.Key.Row)==v.Value));
long maxBianchiResidual=0;foreach(var row in basis)foreach(var q in quads)
 maxBianchiResidual=System.Math.Max(maxBianchiResidual,System.Math.Abs(R(row.Matrix,q.A,q.B,q.C,q.D)-R(row.Matrix,q.A,q.C,q.B,q.D)+R(row.Matrix,q.A,q.D,q.B,q.C)));
int diagonalCount=basis.Count(x=>x.Family=="diagonal"),sharedCount=basis.Count(x=>x.Family=="shared-index"),bianchiCount=basis.Count(x=>x.Family=="bianchi-quadruple");
bool basisPassed=independent&&pairSymmetryPassed&&maxBianchiResidual==0&&diagonalCount==91&&sharedCount==1092&&bianchiCount==2002
 &&quads.Count==1001&&basis.Count==3185&&pairs.Count*(pairs.Count+1)/2-quads.Count==basis.Count;
if(!basisPassed){Emit(precedence[2],new{knownAnswerPassed,controlsPassed=false,basisPassed,independent,pairSymmetryPassed,maxBianchiResidual});return;}

var g=new Tensor();for(int a=0;a<14;a++)Put(g,(1<<a,1<<a),1);
var gamma2=new Tensor();foreach(var (a,b) in pairs){int mask=(1<<a)|(1<<b);Put(gamma2,(mask,mask),1);}
Tensor[] phi1=[g,OmegaLeft(g)],phi2=[gamma2,ScaleTensor(OmegaLeft(gamma2),Scalar.I)];
string[] branches=fx.GetProperty("branches").EnumerateArray().Select(x=>x.GetString()!).ToArray();
string[] monomials=fx.GetProperty("formalMonomials").EnumerateArray().Select(x=>x.GetString()!).ToArray();
bool branchControlsPassed=true,typedControlsPassed=true;int basisBranchCases=0,basisCoefficientCases=0;
var basisRows=new List<object>();
foreach(var row in basis)
{
 var r=Audit(row.Matrix);branchControlsPassed&=r.Passed;typedControlsPassed&=r.Typed;
 basisBranchCases+=4;basisCoefficientCases+=24;
 basisRows.Add(new{id=row.Id,family=row.Family,pivot=new[]{row.Pivot.Row,row.Pivot.Column},r.Passed,r.Typed,r.Hash});
}
var anchors=new List<(string Id,PairMatrix Matrix)>{("flat",new()),("positive-plane-01",Diagonal(0,1,1)),("mixed-plane-07",Diagonal(0,7,1))};
var constant=new PairMatrix();foreach(var (a,b) in pairs)constant[(Pair(a,b),Pair(a,b))]=Sigma(a)*Sigma(b);anchors.Add(("constant-K1",constant));
var weyl=new PairMatrix();foreach(var row in fx.GetProperty("weylDiagonalPlanes").EnumerateArray())
 weyl[(Pair(row[0].GetInt32(),row[1].GetInt32()),Pair(row[0].GetInt32(),row[1].GetInt32()))]=row[2].GetInt32();
anchors.Add(("nonzero-Weyl-01021323",weyl));
bool anchorControlsPassed=true;var anchorRows=new List<object>();
foreach(var anchor in anchors)
{
 var r=Audit(anchor.Matrix);var (ricci,scalar)=RicciScalar(anchor.Matrix);bool expected=r.Passed&&r.Typed;
 if(anchor.Id=="flat")expected&=scalar==0&&r.Slots.All(x=>x.All(t=>t.Count==0));
 if(anchor.Id=="positive-plane-01")expected&=scalar==2&&ricci[0,0]==1&&ricci[1,1]==1;
 if(anchor.Id=="mixed-plane-07")expected&=scalar==-2&&ricci[0,0]==-1&&ricci[7,7]==1;
 if(anchor.Id=="constant-K1")expected&=scalar==182&&Enumerable.Range(0,14).All(a=>Enumerable.Range(0,14).All(b=>ricci[a,b]==(a==b?13*Sigma(a):0)));
 if(anchor.Id=="nonzero-Weyl-01021323")expected&=Curvature(anchor.Matrix,pairs).Count>0&&scalar==0&&ricci.Cast<long>().All(x=>x==0)&&r.Slots.All(x=>x.All(t=>t.Count==0));
 anchorControlsPassed&=expected;
 anchorRows.Add(new{id=anchor.Id,scalar,expectedPassed=expected,r.Hash,branches=r.Slots.Select((slots,i)=>new{branch=branches[i],coefficients=slots.Select((t,j)=>new{monomial=monomials[j],terms=Terms(t)}).ToArray()}).ToArray()});
}
branchControlsPassed&=basisBranchCases==fx.GetProperty("expectedBasisBranchCases").GetInt32()
 &&basisCoefficientCases==fx.GetProperty("expectedBasisCoefficientCases").GetInt32()&&anchors.Count==fx.GetProperty("expectedAnchorCases").GetInt32();
if(!branchControlsPassed||!typedControlsPassed||!anchorControlsPassed)
{Emit(precedence[3],new{knownAnswerPassed,controlsPassed=false,branchControlsPassed,typedControlsPassed,anchorControlsPassed,basisBranchCases,basisCoefficientCases,basisRows,anchorRows});return;}

// Independent exact polynomial-ring certificates; no finite parameter scan is used as a universal proof.
var pa=Poly.Variable(0);var pb=Poly.Variable(1);var pc=Poly.Variable(2);var pd=Poly.Variable(3);
var e1=pa+pb*pd;var e2=pb+pa*pd;
var certificates=new[]{
 (Id:"dE2-E1",Actual:pd*e2-e1,Expected:pa*(pd*pd-1)),
 (Id:"dE1-E2",Actual:pd*e1-e2,Expected:pb*(pd*pd-1)),
 (Id:"aE1-bE2",Actual:pa*e1-pb*e2,Expected:pa*pa-pb*pb)};
bool polynomialCertificatesPassed=certificates.All(x=>x.Actual.Same(x.Expected));
bool symbolicSufficiencyPassed=new[]{-1,1}.All(d=>(pa+(pa*-d)*d).IsZero&&((pa*-d)+pa*d).IsZero);
polynomialCertificatesPassed&=symbolicSufficiencyPassed;
var monomialPolys=new[]{pa,pb,pa*pc,pb*pc,pa*pd,pb*pd};
var plane=Audit(anchors[1].Matrix);var planeJ=RicciTensor(anchors[1].Matrix);
var planeDifference=new Dictionary<(int Form,int Blade),Poly>();
for(int slot=0;slot<6;slot++)foreach(var x in plane.Slots[1][slot])PolyPut(planeDifference,x.Key,monomialPolys[slot]*x.Value.Real);
foreach(var x in planeJ)PolyPut(planeDifference,x.Key,pa*x.Value.Real);
foreach(var x in OmegaLeft(planeJ))PolyPut(planeDifference,x.Key,pb*x.Value.Real);
foreach(var x in g)PolyPut(planeDifference,x.Key,(pa*-1)*x.Value.Real);
foreach(var x in OmegaLeft(g))PolyPut(planeDifference,x.Key,(pb*-1)*x.Value.Real);
var expectedDifference=new Dictionary<(int Form,int Blade),Poly>();
foreach(var x in g)PolyPut(expectedDifference,x.Key,(e1*-1)*x.Value.Real);
foreach(var x in OmegaLeft(g))PolyPut(expectedDifference,x.Key,(e2*-1)*x.Value.Real);
bool matchingIdentityPassed=PolySame(planeDifference,expectedDifference)
 &&g.Keys.All(k=>Degree(k.Blade)==1)&&OmegaLeft(g).Keys.All(k=>Degree(k.Blade)==13)
 &&plane.Slots[1].All(t=>t.Values.All(x=>x.Imaginary==0));
Rational normAA=FormTracePair(phi1[0],phi1[0]),normAB=FormTracePair(phi1[0],phi1[1]),normBA=FormTracePair(phi1[1],phi1[0]),normBB=FormTracePair(phi1[1],phi1[1]);
var norm=pa*pa*normAA+pa*pb*(normAB+normBA)+pb*pb*normBB;
bool normPassed=normAA==-14&&normAB==0&&normBA==0&&normBB==14&&norm.Same((pb*pb-pa*pa)*14);

bool parameterRowsPassed=true;var parameterRows=new List<object>();
foreach(var fixture in fx.GetProperty("parameterRows").EnumerateArray())
{
 long a=fixture.GetProperty("a").GetInt32(),b=fixture.GetProperty("b").GetInt32(),c=fixture.GetProperty("c").GetInt32(),d=fixture.GetProperty("d").GetInt32();
 long[] values=[a,b,a*c,b*c,a*d,b*d];bool matchesAllAnchors=true,scalarFlatMatch=true;
 foreach(var anchor in anchors)
 {
  var r=Audit(anchor.Matrix);var actual=new Tensor();for(int j=0;j<6;j++)Add(actual,ScaleTensor(r.Slots[1][j],values[j]));
  var (ricci,scalar)=RicciScalar(anchor.Matrix);_ = ricci;
  var einstein=ScaleTensor(RicciTensor(anchor.Matrix),-1);Add(einstein,ScaleTensor(g,new Scalar(new Rational(scalar,2),0)));
  var target=ScaleTensor(einstein,a);Add(target,ScaleTensor(OmegaLeft(einstein),b));
  bool match=Same(actual,target);matchesAllAnchors&=match;if(scalar==0)scalarFlatMatch&=match;
 }
 bool equations=a+b*d==0&&b+a*d==0,invertible=a*a!=b*b;
 var p=AddCopy(Single(0,0,a),Single(0,Omega,b));var conjugate=AddCopy(Single(0,0,a),Single(0,Omega,-b));
 bool inverseIdentity=Same(Multiply(p,conjugate),Single(0,0,a*a-b*b));
 bool expected=matchesAllAnchors==fixture.GetProperty("matches").GetBoolean()&&matchesAllAnchors==equations
  &&invertible==fixture.GetProperty("invertible").GetBoolean()&&scalarFlatMatch&&inverseIdentity;
 parameterRowsPassed&=expected;
 parameterRows.Add(new{id=fixture.GetProperty("id").GetString(),a,b,c,d,matchesAllAnchors,equations,invertible,scalarFlatMatch,inverseIdentity,expectedPassed=expected});
}

// Exact chiral projector and trace controls, in the full Clifford algebra; no physical projection.
bool chiralityPassed=true,mixedPairingPassed=true,hIsotropyPassed=true;int chiralityCases=0,wrongInputProjectorDetected=0;
var chiralityRows=new List<object>();var hMatrix=Single(0,0x3f80,1);var omega=Single(0,Omega,1);
bool omegaHAdjointNegative=Same(Multiply(Dagger(omega),hMatrix),ScaleTensor(Multiply(hMatrix,omega),-1));
foreach(int sign in fx.GetProperty("chiralitySigns").EnumerateArray().Select(x=>x.GetInt32()))
{
 var outputProjector=Projector(-sign);var inputProjector=Projector(sign);
 bool projectorIdentities=Same(Multiply(outputProjector,outputProjector),outputProjector)
  &&Multiply(outputProjector,inputProjector).Count==0&&Same(AddCopy(outputProjector,inputProjector),Single(0,0,1));
 bool isotropic=Multiply(Multiply(Dagger(outputProjector),hMatrix),outputProjector).Count==0;
 hIsotropyPassed&=isotropic;
 for(int axis=0;axis<14;axis++)
 {
  chiralityCases++;var gamma=Single(0,1<<axis,1);
  var x=ScaleTensor(Multiply(outputProjector,gamma),2);
  bool outputSide=Same(Multiply(outputProjector,x),x)&&Multiply(inputProjector,x).Count==0;
  bool inputSide=Same(Multiply(x,inputProjector),x)&&Multiply(x,outputProjector).Count==0;
  bool wrongInputDetected=!Same(Multiply(x,outputProjector),x);if(wrongInputDetected)wrongInputProjectorDetected++;
  var sameProduct=Multiply(Multiply(outputProjector,gamma),Multiply(outputProjector,gamma));
  var oppositeProduct=Multiply(Multiply(outputProjector,gamma),Multiply(inputProjector,gamma));
  Scalar oppositeTrace=oppositeProduct.GetValueOrDefault((0,0))*128;
  bool pairControl=sameProduct.Count==0&&oppositeTrace==new Scalar(64*Sigma(axis));
  chiralityPassed&=projectorIdentities&&outputSide&&inputSide&&wrongInputDetected;
  mixedPairingPassed&=pairControl;
  chiralityRows.Add(new{d=sign,axis,outputChirality=-sign,inputChirality=sign,projectorIdentities,outputSide,inputSide,wrongInputDetected,
   sameProductZero=sameProduct.Count==0,oppositeTrace=oppositeTrace.Real.ToString(),pairControl,isotropic});
 }
}
chiralityPassed&=chiralityCases==28&&wrongInputProjectorDetected==28&&omegaHAdjointNegative;
var correctAd=plane.Slots[1][4];var oppositeSignAd=ScaleTensor(correctAd,-1);
var scalarFlatRicci=new PairMatrix();foreach(var row in fx.GetProperty("scalarFlatRicciPlanes").EnumerateArray())
 scalarFlatRicci[(Pair(row[0].GetInt32(),row[1].GetInt32()),Pair(row[0].GetInt32(),row[1].GetInt32()))]=row[2].GetInt32();
var scalarFlatJ=RicciTensor(scalarFlatRicci);var scalarFlatAudit=Audit(scalarFlatRicci);
bool scalarFlatParameterDecoyPassed=RicciScalar(scalarFlatRicci).Scalar==0&&scalarFlatJ.Count==2
 &&Same(AddCopy(scalarFlatAudit.Slots[1][0],scalarFlatAudit.Slots[1][2]),ScaleTensor(scalarFlatJ,-1));
bool negativeControlsPassed=correctAd.Count>0&&!Same(correctAd,oppositeSignAd)&&wrongInputProjectorDetected==28
 &&parameterRowsPassed&&mixedPairingPassed&&matchingIdentityPassed&&scalarFlatParameterDecoyPassed;
bool controlsPassed=basisPassed&&branchControlsPassed&&typedControlsPassed&&anchorControlsPassed
 &&polynomialCertificatesPassed&&matchingIdentityPassed&&normPassed&&parameterRowsPassed&&chiralityPassed&&mixedPairingPassed&&hIsotropyPassed&&negativeControlsPassed;
Emit(controlsPassed?Success:precedence[4],new{
 knownAnswerPassed,controlsPassed,helperParity,upstreamValid,sourceAnchorsPresent,
 arithmeticControls=new{cliffordPassed,hodgePassed,cliffordWordCases,hodgeCases,exactTolerance=0,arithmetic="checked-int64-reduced-rational-complex",omegaSquared=1,topStar=-1},
 basisControls=new{basisPassed,independent,pairSymmetryPassed,diagonalCount,sharedCount,bianchiCount,quadrupleCount=quads.Count,basisCount=basis.Count,uniquePivotCount=pivots.Count,maxBianchiResidual,basisRows},
 literalBranches=new{branchControlsPassed,typedControlsPassed,basisBranchCases,basisCoefficientCases,branches,formalMonomials=monomials,
  ccc="-(a+b*Omega)*J",cca="-(a+b*Omega)*J-(d*R/2)*(b+a*Omega)*G",aac="0",aaa="(c*R/2)*(a+b*Omega)*G",
  entireAlgebraicRiemannSpaceOnly=true,tiedOccurrencesSourceRequired=false,invariantFamilyExhausted=false},
 anchors=new{anchorControlsPassed,anchorRows},
 parameterControls=new{polynomialCertificatesPassed,symbolicSufficiencyPassed,matchingIdentityPassed,equations=new[]{e1.Canonical(),e2.Canonical()},
  certificates=certificates.Select(x=>new{x.Id,actual=x.Actual.Canonical(),expected=x.Expected.Canonical(),passed=x.Actual.Same(x.Expected)}).ToArray(),
  universalMatchingQuantifier="all algebraic Riemann tensors",nontrivialSolutions="a!=0; d=+/-1; b=-d*a; c arbitrary",
  trivialSolutions="a=b=0; c,d arbitrary",normPassed,normPolynomial=norm.Canonical(),normAA=normAA.ToString(),normAB=normAB.ToString(),normBA=normBA.ToString(),normBB=normBB.ToString(),
  parameterRowsPassed,parameterRows},
 chiralityControls=new{chiralityPassed,mixedPairingPassed,hIsotropyPassed,omegaHAdjointNegative,chiralityCases,wrongInputProjectorDetected,chiralityRows,
  physicalChiralityProjectionSelected=false,nullNormImpliesSourceInadmissibility=false,selfPairingImpliesMixedActionZero=false},
 negativeControls=new{negativeControlsPassed,oppositeSignAd=Terms(oppositeSignAd),correctAd=Terms(correctAd),scalarFlatParameterDecoyPassed,scalarFlatJ=Terms(scalarFlatJ)},
 nextRequirement="Select source tensors, bracket occurrences, normalization/pairing, retained-field map and actual mixed action; conditional chirality/nullity does not supply these missing choices."
});

(bool Passed,bool Typed,string Hash,Tensor[][] Slots) Audit(PairMatrix matrix)
{
 var dual=Star(Curvature(matrix,pairs));var first=new Tensor[2,2];var inner=new Tensor[2,2];
 for(int i=0;i<2;i++)for(int bracket=0;bracket<2;bracket++)
 {first[i,bracket]=UndoStarOne(BracketWedge(phi1[i],dual,bracket==0?'C':'A'));
  inner[i,bracket]=Star(BracketWedge(phi2[i],dual,bracket==0?'C':'A'));}
 var j=RicciTensor(matrix);var oj=OmegaLeft(j);long scalar=RicciScalar(matrix).Scalar;
 var halfG=ScaleTensor(g,new Scalar(new Rational(scalar,2),0));var halfOg=OmegaLeft(halfG);
 bool passed=Same(first[0,0],ScaleTensor(j,-1))&&Same(first[1,0],ScaleTensor(oj,-1))
  &&first[0,1].Count==0&&first[1,1].Count==0&&inner[0,0].Count==0&&inner[1,0].Count==0
  &&Same(inner[0,1],Single(0,0,new Scalar(0,new Rational(scalar,2))))
  &&Same(inner[1,1],Single(0,Omega,new Scalar(new Rational(-scalar,2),0)));
 bool typed=true;var all=new List<Tensor[]>();var canonical=new StringBuilder();
 foreach(string branch in branches)
 {
  var slots=new Tensor[6];slots[0]=first[0,branch[0]=='C'?0:1];slots[1]=first[1,branch[0]=='C'?0:1];
  for(int i=0;i<2;i++)for(int k=0;k<2;k++)
  {
   var rawOuter=Star(BracketWedge(phi1[i],inner[k,branch[2]=='C'?0:1],branch[1]));
   typed&=rawOuter.Keys.All(x=>Degree(x.Form)==13);
   slots[2+i+2*k]=ScaleTensor(UndoStarOne(rawOuter),new Scalar(new Rational(-1,2),0));
  }
  Tensor[] predicted=[new(),new(),new(),new(),new(),new()];
  if(branch[0]=='C'){predicted[0]=ScaleTensor(j,-1);predicted[1]=ScaleTensor(oj,-1);}
  if(branch=="CCA"){predicted[4]=ScaleTensor(halfOg,-1);predicted[5]=ScaleTensor(halfG,-1);}
  if(branch=="AAA"){predicted[2]=halfG;predicted[3]=halfOg;}
  for(int m=0;m<6;m++)
  {
   passed&=Same(slots[m],predicted[m]);typed&=slots[m].Keys.All(x=>Degree(x.Form)==1&&Degree(x.Blade) is 1 or 13);
   canonical.Append(branch).Append(':').Append(monomials[m]).Append(':').Append(Canonical(slots[m])).Append('\n');
  }
  all.Add(slots);
 }
 return(passed,typed,Hash(canonical.ToString()),all.ToArray());
}
static Tensor RicciTensor(PairMatrix matrix)
{
 var ricci=RicciScalar(matrix).Ricci;var t=new Tensor();
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)Put(t,(1<<a,1<<b),ricci[a,b]*Sigma(b));return t;
}
static Tensor OmegaLeft(Tensor t)
{var r=new Tensor();foreach(var x in t)Put(r,(x.Key.Form,Omega^x.Key.Blade),x.Value*BladeSign(Omega,x.Key.Blade));return r;}
static void Add(Tensor destination,Tensor source){foreach(var x in source)Put(destination,x.Key,x.Value);}
static Tensor AddCopy(Tensor a,Tensor b){var r=new Tensor(a);Add(r,b);return r;}
static Tensor Multiply(Tensor a,Tensor b)
{
 if(a.Keys.Concat(b.Keys).Any(x=>x.Form!=0))throw new InvalidOperationException("Clifford-only control expected");
 var r=new Tensor();foreach(var x in a)foreach(var y in b)Put(r,(0,x.Key.Blade^y.Key.Blade),x.Value*y.Value*BladeSign(x.Key.Blade,y.Key.Blade));return r;
}
static Tensor Dagger(Tensor t)
{
 var r=new Tensor();foreach(var x in t){int grade=Degree(x.Key.Blade);
  int sign=((grade*(grade-1)/2+Degree(x.Key.Blade&0x3f80))%2==0)?1:-1;
  Put(r,x.Key,new Scalar(x.Value.Real,x.Value.Imaginary*-1)*sign);}return r;
}
static Tensor Projector(int sign)=>AddCopy(Single(0,0,new Scalar(new Rational(1,2),0)),Single(0,Omega,new Scalar(new Rational(sign,2),0)));
static Rational FormTracePair(Tensor a,Tensor b)
{
 Rational value=0;foreach(var x in a)foreach(var y in b)if(x.Key.Form==y.Key.Form&&x.Key.Blade==y.Key.Blade)
  value+=(x.Value*y.Value).Real*(-BladeSign(x.Key.Blade,y.Key.Blade)*(Degree(x.Key.Form&0x3f80)%2==0?1:-1));return value;
}
static void PolyPut(Dictionary<(int Form,int Blade),Poly> map,(int Form,int Blade) key,Poly value)
{var sum=map.TryGetValue(key,out var old)?old+value:value;if(sum.IsZero)map.Remove(key);else map[key]=sum;}
static bool PolySame(Dictionary<(int Form,int Blade),Poly> a,Dictionary<(int Form,int Blade),Poly> b)=>a.Count==b.Count&&a.All(x=>b.TryGetValue(x.Key,out var y)&&x.Value.Same(y));
void Emit(string verdict,object evidence)
{
 var result=new{schemaVersion=1,phase=592,phaseId="phase592-companion-tensor-chirality-audit",contractId=ContractId,
 contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,
 bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,
 verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
 authorityFirewalls=firewalls.ToDictionary(x=>x,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/companion_tensor_chirality_audit.json",json);
 File.WriteAllText(Root+"/output/companion_tensor_chirality_audit_summary.json",json);
 Console.WriteLine($"Phase592 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();

sealed class Poly
{
 private readonly Dictionary<(int A,int B,int C,int D),Rational> terms=new();
 public bool IsZero=>terms.Count==0;
 public static Poly Variable(int i){var p=new Poly();p.terms[(i==0?1:0,i==1?1:0,i==2?1:0,i==3?1:0)]=1;return p;}
 public static implicit operator Poly(int value){var p=new Poly();if(value!=0)p.terms[(0,0,0,0)]=value;return p;}
 private void Put((int A,int B,int C,int D) key,Rational value){var sum=terms.GetValueOrDefault(key)+value;if(sum==0)terms.Remove(key);else terms[key]=sum;}
 public static Poly operator +(Poly a,Poly b){var p=new Poly();foreach(var x in a.terms)p.Put(x.Key,x.Value);foreach(var x in b.terms)p.Put(x.Key,x.Value);return p;}
 public static Poly operator -(Poly a,Poly b)=>a+b*new Rational(-1);
 public static Poly operator *(Poly a,Poly b)
 {var p=new Poly();foreach(var x in a.terms)foreach(var y in b.terms)p.Put((x.Key.A+y.Key.A,x.Key.B+y.Key.B,x.Key.C+y.Key.C,x.Key.D+y.Key.D),x.Value*y.Value);return p;}
 public static Poly operator *(Poly a,Rational b){var p=new Poly();foreach(var x in a.terms)p.Put(x.Key,x.Value*b);return p;}
 public static Poly operator *(Poly a,int b)=>a*new Rational(b);
 public bool Same(Poly p)=>terms.Count==p.terms.Count&&terms.All(x=>p.terms.TryGetValue(x.Key,out var y)&&x.Value==y);
 public string Canonical()=>string.Join(";",terms.OrderBy(x=>x.Key).Select(x=>$"{x.Key.A},{x.Key.B},{x.Key.C},{x.Key.D}:{x.Value}"));
}
