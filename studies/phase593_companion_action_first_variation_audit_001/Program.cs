using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using PT=System.Collections.Generic.Dictionary<(int Form,int Blade),Polynomial>;

const string Root="studies/phase593_companion_action_first_variation_audit_001";
const string Upstream="studies/phase592_companion_tensor_chirality_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=Upstream+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase593-a49-companion-action-first-variation-v1";
const string Success="companion-action-variation-mismatch-certified-source-choice-open";
const int Omega=(1<<14)-1;
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "background":"flat constant unit-volume (7,7) fourteen-torus; epsilon=I; B=0; dT=0",
 "branch":"CCA","phi1":"(a+b*Omega)*gamma","phi2":"(c+i*d*Omega)*Gamma2",
 "chain":"K(F)=[Phi1 wedge star(F)]_C-(1/2)*star([Phi1 wedge star([Phi2 wedge star(F)]_A)]_C)",
 "hodge":"star(theta^I)=shuffle(I,Icomplement)*product_sigma(I)*theta^Icomplement",
 "pairing":"-ReTr(U wedge K(F))/128; equivalent signed one-form trace pairing with star1_inverse K",
 "curvature":"Q(T)=T wedge T; F=Q; graded bracket equals 2Q",
 "bracketEncodings":[1,2],"sourceCubicPrefactor":"gamma/3 on B(T,L(Q))",
 "operatorRows":[{"id":"canonical","a":1,"b":0,"d":0},{"id":"chiral-minus","a":1,"b":-1,"d":1},{"id":"chiral-plus","a":1,"b":1,"d":-1}],
 "formalCSlots":["1","c"],
 "witnesses":[
  {"id":"curvature-stationary","operatorIds":["canonical","chiral-minus","chiral-plus"],"T":"x theta0 Gamma01+y theta1 Gamma12+z theta1 gamma2","V":"theta1 gamma2","Q":"2xy theta01 Gamma02","forceAtZ0":"4xy","action":"(4gamma/3)xyz","derivativeAtZ0":"(4gamma/3)xy"},
  {"id":"commuting-background","operatorIds":["chiral-minus","chiral-plus"],"T":"x theta0 Gamma02+y theta1 gamma1+z theta2 Omega","V":"theta2 Omega","Q":"-2yz theta12 Omega gamma1","forceAtZ0":"0","action":"(4hgamma/3)xyz","derivativeAtZ0":"(4hgamma/3)xy"}
 ],
 "amplitudes":[[1,1],[2,1],[-1,2],[0,1],[1,0]],"evaluationZ":0,"massKappa":[-1,0,1],
 "mass":"(kappa/2)B(T,T); derivative=-kappa*z; equals kappa*B(V,T)",
 "chernSimons":{"A":"x theta0 Gamma01+y theta1 Gamma12+z theta2 Gamma02","V":"theta2 Gamma02","nu11Mask":16376,"K":"F wedge nu11","normalizedPrefactor":"1/(3gamma) on bracket_gamma","normalizedAction":"2xyz","force":"2xy","literalSourceDerivative":"2gamma*xy"},
 "predictedCounts":{"operatorWitnessRows":5,"coefficientRows":10,"polynomialRows":20,"nonzeroMismatchRows":10,"zeroFormalCRows":10,"amplitudeChecks":100,"zeroAmplitudeChecks":40,"massChecks":15,"chernSimonsRows":2,"globalReweightingCertificates":2},
 "knownAnswer":{"carrierMasks":"subsets of axes0,1,2 and their Omega complements","maskCount":16,"wordProductCases":256,"hodgeCases":16384},
 "exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":5,"maximumEstimatedCpuSeconds":15,"estimatedPeakBytes":67108864,"maximumEstimatedPeakBytes":134217728}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
 "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
 "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","literal-chain-control-failed","independent-variation-control-failed","positive-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["project"]=Root+"/Phase593CompanionActionFirstVariationAudit.csproj",
 ["study"]=Root+"/STUDY.md",["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["phase592-summary"]=Upstream+"/output/companion_tensor_chirality_audit_summary.json",["phase592-contract"]=Upstream+"/preregistration/contract_v1.json",
 ["phase592-program"]=Upstream+"/Program.cs",["algebra-helper"]=Upstream+"/ExactAlgebra.cs",
 ["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var doc=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=doc.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==593
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
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
using(var ud=JsonDocument.Parse(File.ReadAllBytes(paths["phase592-summary"])))
{
 var u=ud.RootElement;bool valid=u.GetProperty("auditPassed").GetBoolean()&&u.GetProperty("contractValid").GetBoolean()
  &&u.GetProperty("exactBindingsValid").GetBoolean()&&u.GetProperty("coreSourceTreeValid").GetBoolean()
  &&u.GetProperty("verdictKind").GetString()=="companion-tensor-chirality-controls-pass-source-choice-open"
  &&u.GetProperty("contractSha256").GetString()==Sha(paths["phase592-contract"])
  &&u.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&u.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()
  &&u.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>u.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)
  &&u.GetProperty("externalReviewPending").GetBoolean()&&u.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
 string source=File.ReadAllText(paths["primary-source"]);
 valid&=new[]{"(8.5)","(9.3)","(9.4)","(9.7)","(12.26)","(12.27)"}.All(source.Contains);
 if(!valid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstreamAndSourceValid=false});return;}
}
var fx=contract.GetProperty("fixtures");var expectedCounts=fx.GetProperty("predictedCounts");
int[] masks=Enumerable.Range(0,8).Concat(Enumerable.Range(0,8).Select(x=>Omega^x)).ToArray();
bool wordsPassed=true,hodgePassed=true;int wordCases=0,hodgeCases=0;
foreach(int a in masks)foreach(int b in masks){wordCases++;wordsPassed&=BladeSign(a,b)==WordProductSign(a,b);}
for(int mask=0;mask<=Omega;mask++){hodgeCases++;int r=Degree(mask);hodgePassed&=HodgeSign(mask)*HodgeSign(Omega^mask)==((r*(14-r)+7)%2==0?1:-1);}
var x=Polynomial.Variable(0);var y=Polynomial.Variable(1);var z=Polynomial.Variable(2);
bool polynomialKnownAnswerPassed=(x*x*y+z*3).Derivative(0).Same(x*y*2)
 &&(x*x*y+z*3).Derivative(2).Same(Polynomial.Constant(3))
 &&(x*z+y).AtZZero().Same(y)&&Scalar.I*Scalar.I==new Scalar(-1)
 &&new Rational(1,2)+new Rational(1,3)==new Rational(5,6);
bool knownAnswerPassed=wordsPassed&&hodgePassed&&polynomialKnownAnswerPassed&&BladeSign(Omega,Omega)==1&&HodgeSign(Omega)==-1
 &&masks.Length==fx.GetProperty("knownAnswer").GetProperty("maskCount").GetInt32()
 &&wordCases==fx.GetProperty("knownAnswer").GetProperty("wordProductCases").GetInt32()
 &&hodgeCases==fx.GetProperty("knownAnswer").GetProperty("hodgeCases").GetInt32();
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,wordsPassed,hodgePassed,polynomialKnownAnswerPassed,wordCases,hodgeCases});return;}

var gamma=new PT();for(int j=0;j<14;j++)Pput(gamma,(1<<j,1<<j),Polynomial.Constant(1));
var gamma2=new PT();foreach(var (a,b) in Pairs())Pput(gamma2,((1<<a)|(1<<b),(1<<a)|(1<<b)),Polynomial.Constant(1));
var rows=new List<object>();var tensorRows=new List<object>();var massRows=new List<object>();
var actualReweighting=new Dictionary<(string Witness,string Operator),(Polynomial Slope,Polynomial Target)>();
bool chainPassed=true,variationPassed=true,massPassed=true,realFormPassed=true,dualPairingPassed=true;
int operatorWitnessRows=0,coefficientRows=0,polynomialRows=0,nonzeroMismatchRows=0,zeroFormalCRows=0,amplitudeChecks=0,zeroAmplitudeChecks=0,massChecks=0;
foreach(var witness in fx.GetProperty("witnesses").EnumerateArray())
foreach(var op in fx.GetProperty("operatorRows").EnumerateArray())
{
 string witnessId=witness.GetProperty("id").GetString()!,operatorId=op.GetProperty("id").GetString()!;
 if(!witness.GetProperty("operatorIds").EnumerateArray().Any(i=>i.GetString()==operatorId))continue;
 operatorWitnessRows++;bool first=witnessId=="curvature-stationary";
 int a=op.GetProperty("a").GetInt32(),b=op.GetProperty("b").GetInt32(),d=op.GetProperty("d").GetInt32();
 var torsion=new PT();PT direction;
 if(first){Pput(torsion,(1,3),x);Pput(torsion,(2,6),y);Pput(torsion,(2,4),z);direction=PSingle(2,4,Polynomial.Constant(1));}
 else{Pput(torsion,(1,5),x);Pput(torsion,(2,2),y);Pput(torsion,(4,Omega),z);direction=PSingle(4,Omega,Polynomial.Constant(1));}
 realFormPassed&=Hanti(torsion)&&Hanti(direction);
 PT curvature=PWedge(torsion,torsion);
 PT expectedCurvature=first?PSingle(3,5,x*y*2):PSingle(6,Omega^2,y*z*(-2*BladeSign(Omega,2)));
 bool curvaturePassed=PSame(curvature,expectedCurvature)&&PSame(PDerivative(curvature,2),PDerivative(expectedCurvature,2));
 if(first)curvaturePassed&=PDerivative(curvature,2).Count==0;else curvaturePassed&=PAtZZero(curvature).Count==0;
 chainPassed&=curvaturePassed;
 var phi1=PAdd(PScale(gamma,a),PScale(POmega(gamma),b));
 PT[] phi2=[PScale(POmega(gamma2),Scalar.I*d),gamma2];
 for(int cslot=0;cslot<2;cslot++)
 {
  coefficientRows++;
  PT firstTerm=cslot==0?PBracket(phi1,PStar(curvature),'C'):new PT();
  PT inner=PBracket(phi2[cslot],PStar(curvature),'A');
  PT innerLower=PStar(inner);PT outer=PBracket(phi1,innerLower,'C');
  PT upper=PAdd(firstTerm,PScale(PStar(outer),new Scalar(new Rational(-1,2),0)));
  PT lowered=PStar(upper);
  bool typed=HasDegree(curvature,2)&&HasDegree(firstTerm,13)&&HasDegree(inner,14)&&HasDegree(innerLower,0)&&HasDegree(outer,1)&&HasDegree(upper,13)&&HasDegree(lowered,1);
  var predicted=new PT();var predictedInner=new PT();
  if(cslot==0)
  {
   if(first){Pput(predicted,(2,4),x*y*(-4*a));Pput(predicted,(2,Omega^4),x*y*(-4*b*BladeSign(Omega,4)));}
   else
   {
    for(int j=0;j<14;j++)if(j!=1&&j!=2)Pput(predicted,(1<<j,(1<<j)^4),y*z*(4*b*BladeSign(1<<j,4)));
    Pput(predictedInner,(0,4),y*z*(-4*b));
   }
  }
  bool fullTensorPassed=PSame(lowered,predicted)&&PSame(innerLower,predictedInner);
  chainPassed&=typed&&fullTensorPassed;realFormPassed&=Hanti(lowered);
  Polynomial force=PairOne(direction,lowered);Polynomial forceWedge=TopTrace(PWedge(direction,upper));
  dualPairingPassed&=force.Same(forceWedge);
  Polynomial rawAction=TopTrace(PWedge(torsion,upper));
  if(cslot==0)actualReweighting[(witnessId,operatorId)]=(rawAction.Derivative(2).AtZZero(),force.AtZZero());
  Polynomial predictedRaw=cslot==0?x*y*z*(first?4*a:4*b):Polynomial.Constant(0);
  Polynomial predictedForce=cslot==0&&first?x*y*(4*a):Polynomial.Constant(0);
  variationPassed&=rawAction.Same(predictedRaw)&&force.AtZZero().Same(predictedForce);
  tensorRows.Add(new{witness=witnessId,op=operatorId,cSlot=cslot,curvaturePassed,typed,fullTensorPassed,
   curvature=PTerms(curvature),innerLower=PTerms(innerLower),lowered=PTerms(lowered),rawAction=rawAction.Terms(),force=force.Terms()});
  foreach(int encoding in fx.GetProperty("bracketEncodings").EnumerateArray().Select(v=>v.GetInt32()))
  {
   polynomialRows++;Scalar prefactor=new(new Rational(encoding,3),0);
   Polynomial action=rawAction*prefactor;
   Polynomial derivative=action.Derivative(2).AtZZero();
   Polynomial candidate=force.AtZZero();Polynomial mismatch=derivative+candidate*-1;
   Polynomial predictedDerivative=(cslot==0?x*y*(first?4*a:4*b):Polynomial.Constant(0))*prefactor;
   bool expected=derivative.Same(predictedDerivative)&&candidate.Same(predictedForce)
    &&mismatch.Same(predictedDerivative+predictedForce*-1);
   if(cslot==0){nonzeroMismatchRows++;expected&=!mismatch.IsZero;}
   else{zeroFormalCRows++;expected&=action.IsZero&&derivative.IsZero&&candidate.IsZero&&mismatch.IsZero;}
   var amplitudeRows=new List<object>();
   foreach(var amp in fx.GetProperty("amplitudes").EnumerateArray())
   {
    int ax=amp[0].GetInt32(),ay=amp[1].GetInt32();amplitudeChecks++;
    bool zero=ax==0||ay==0;if(zero)zeroAmplitudeChecks++;
    Scalar measured=derivative.Evaluate(ax,ay,0),target=candidate.Evaluate(ax,ay,0);
    Scalar oracle=new Scalar(cslot==0?(first?4*a:4*b)*ax*ay:0)*prefactor;
    bool ok=measured==oracle&&target==new Scalar(cslot==0&&first?4*a*ax*ay:0)&&(!zero||(measured.IsZero&&target.IsZero));
    expected&=ok;amplitudeRows.Add(new{x=ax,y=ay,zero,passed=ok,derivative=ScalarText(measured),force=ScalarText(target)});
   }
   variationPassed&=expected;
   rows.Add(new{witness=witnessId,op=operatorId,cSlot=cslot,encoding,expectedPassed=expected,action=action.Terms(),derivativeAtZ0=derivative.Terms(),candidateAtZ0=candidate.Terms(),mismatch=mismatch.Terms(),amplitudeRows});
  }
 }
 foreach(int kappa in fx.GetProperty("massKappa").EnumerateArray().Select(v=>v.GetInt32()))
 {
  massChecks++;Polynomial action=PairOne(torsion,torsion)*new Scalar(new Rational(kappa,2),0);
  Polynomial derivative=action.Derivative(2),force=PairOne(direction,torsion)*kappa;
  bool passed=derivative.Same(force)&&derivative.Same(z*(-kappa))&&derivative.AtZZero().IsZero;
  massPassed&=passed;massRows.Add(new{witness=witnessId,op=operatorId,kappa,passed,action=action.Terms(),derivative=derivative.Terms(),force=force.Terms()});
 }
}
chainPassed&=operatorWitnessRows==expectedCounts.GetProperty("operatorWitnessRows").GetInt32()&&coefficientRows==expectedCounts.GetProperty("coefficientRows").GetInt32();
if(!chainPassed||!realFormPassed){Emit(precedence[2],new{knownAnswerPassed,controlsPassed=false,chainPassed,realFormPassed,operatorWitnessRows,coefficientRows,tensorRows});return;}
variationPassed&=dualPairingPassed&&polynomialRows==expectedCounts.GetProperty("polynomialRows").GetInt32()
 &&nonzeroMismatchRows==expectedCounts.GetProperty("nonzeroMismatchRows").GetInt32()&&zeroFormalCRows==expectedCounts.GetProperty("zeroFormalCRows").GetInt32()
 &&amplitudeChecks==expectedCounts.GetProperty("amplitudeChecks").GetInt32()&&zeroAmplitudeChecks==expectedCounts.GetProperty("zeroAmplitudeChecks").GetInt32();
if(!variationPassed){Emit(precedence[3],new{knownAnswerPassed,controlsPassed=false,chainPassed,variationPassed,dualPairingPassed,rows,tensorRows});return;}

// An independently typed ordinary three-dimensional Chern-Simons control.
var cs=new PT();Pput(cs,(1,3),x);Pput(cs,(2,6),y);Pput(cs,(4,5),z);
var csV=PSingle(4,5,Polynomial.Constant(1));var nu=PSingle(fx.GetProperty("chernSimons").GetProperty("nu11Mask").GetInt32(),0,Polynomial.Constant(1));
PT csQ=PWedge(cs,cs),csK=PWedge(csQ,nu);
Polynomial csRaw=TopTrace(PWedge(cs,csK)),csForce=TopTrace(PWedge(csV,csK));
bool csPassed=HasDegree(csK,13)&&csRaw.Same(x*y*z*6)&&csForce.Same(x*y*2)&&Hanti(cs);
var csRows=new List<object>();
foreach(int encoding in fx.GetProperty("bracketEncodings").EnumerateArray().Select(v=>v.GetInt32()))
{
 Polynomial encodedRaw=csRaw*encoding;
 Polynomial normalized=encodedRaw*new Scalar(new Rational(1,3*encoding),0);
 Polynomial directDerivative=normalized.Derivative(2);
 Polynomial literalDerivative=(encodedRaw*new Scalar(new Rational(1,3),0)).Derivative(2);
 bool passed=directDerivative.Same(csForce)&&literalDerivative.Same(x*y*(2*encoding))&&(literalDerivative.Same(csForce)==(encoding==1));
 csPassed&=passed;csRows.Add(new{encoding,passed,normalizedAction=normalized.Terms(),directDerivative=directDerivative.Terms(),force=csForce.Terms(),literalOneThirdDerivative=literalDerivative.Terms()});
}
// Two exact contradictory requirements on one common raw cubic prefactor alpha.
var reweightingRows=new List<object>();
foreach(int h in new[]{-1,1})
{
 string id=h==-1?"chiral-minus":"chiral-plus";
 var first=actualReweighting[("curvature-stationary",id)];var second=actualReweighting[("commuting-background",id)];
 bool passed=first.Slope.Same(first.Target)&&!first.Slope.IsZero&&!second.Slope.IsZero&&second.Target.IsZero;
 reweightingRows.Add(new{h,passed,firstSlope=first.Slope.Terms(),firstTarget=first.Target.Terms(),secondSlope=second.Slope.Terms(),secondTarget=second.Target.Terms(),firstRequiredAlpha="1",secondRequiredAlpha="0",compatible=false});
 variationPassed&=passed;
}
bool controlsPassed=chainPassed&&realFormPassed&&variationPassed&&dualPairingPassed&&massPassed&&csPassed
 &&massChecks==expectedCounts.GetProperty("massChecks").GetInt32()&&csRows.Count==expectedCounts.GetProperty("chernSimonsRows").GetInt32()
 &&reweightingRows.Count==expectedCounts.GetProperty("globalReweightingCertificates").GetInt32();
Emit(controlsPassed?Success:precedence[4],new{knownAnswerPassed,controlsPassed,wordsPassed,hodgePassed,polynomialKnownAnswerPassed,
 wordCases,hodgeCases,chainPassed,realFormPassed,variationPassed,dualPairingPassed,massPassed,csPassed,
 operatorWitnessRows,coefficientRows,polynomialRows,nonzeroMismatchRows,zeroFormalCRows,amplitudeChecks,zeroAmplitudeChecks,massChecks,
 tensorRows,rows,massRows,csRows,reweightingRows,sourceChoiceOpen=true,registeredCoreChanged=false});

static void Pput(PT t,(int Form,int Blade) key,Polynomial value)
{Polynomial sum=t.TryGetValue(key,out var old)?old+value:value;if(sum.IsZero)t.Remove(key);else t[key]=sum;}
static PT PSingle(int form,int blade,Polynomial value){var t=new PT();Pput(t,(form,blade),value);return t;}
static PT PAdd(PT a,PT b){var r=new PT(a);foreach(var q in b)Pput(r,q.Key,q.Value);return r;}
static PT PScale(PT a,Scalar s){var r=new PT();foreach(var q in a)Pput(r,q.Key,q.Value*s);return r;}
static PT POmega(PT t){var r=new PT();foreach(var q in t)Pput(r,(q.Key.Form,Omega^q.Key.Blade),q.Value*BladeSign(Omega,q.Key.Blade));return r;}
static PT PStar(PT t){var r=new PT();foreach(var q in t)Pput(r,(Omega^q.Key.Form,q.Key.Blade),q.Value*HodgeSign(q.Key.Form));return r;}
static PT PWedge(PT a,PT b)
{
 var r=new PT();foreach(var q in a)foreach(var s in b)if((q.Key.Form&s.Key.Form)==0)
  Pput(r,(q.Key.Form|s.Key.Form,q.Key.Blade^s.Key.Blade),q.Value*s.Value*(Shuffle(q.Key.Form,s.Key.Form)*BladeSign(q.Key.Blade,s.Key.Blade)));return r;
}
static PT PBracket(PT a,PT b,char bracket)
{
 var r=new PT();foreach(var q in a)foreach(var s in b)if((q.Key.Form&s.Key.Form)==0)
 {
  int ab=BladeSign(q.Key.Blade,s.Key.Blade),ba=BladeSign(s.Key.Blade,q.Key.Blade);
  Scalar sign=new(Shuffle(q.Key.Form,s.Key.Form)*(bracket=='C'?ab-ba:ab+ba));if(bracket=='A')sign*=Scalar.I;
  Pput(r,(q.Key.Form|s.Key.Form,q.Key.Blade^s.Key.Blade),q.Value*s.Value*sign);
 }return r;
}
static PT PDerivative(PT a,int variable){var r=new PT();foreach(var q in a)Pput(r,q.Key,q.Value.Derivative(variable));return r;}
static PT PAtZZero(PT a){var r=new PT();foreach(var q in a)Pput(r,q.Key,q.Value.AtZZero());return r;}
static bool PSame(PT a,PT b)=>a.Count==b.Count&&a.All(q=>b.TryGetValue(q.Key,out var v)&&q.Value.Same(v));
static bool HasDegree(PT a,int degree)=>a.Keys.All(q=>Degree(q.Form)==degree);
static bool Hanti(PT a)=>a.All(q=>q.Value.Coefficients.All(v=>
 ((Degree(q.Key.Blade)*(Degree(q.Key.Blade)+1)/2)%2==1?v.Imaginary.Numerator==0:v.Real.Numerator==0)));
static Polynomial TopTrace(PT t)
{if(!HasDegree(t,14))throw new InvalidOperationException("Top trace requires degree14");return t.TryGetValue((Omega,0),out var p)?p.RealPart()*-1:Polynomial.Constant(0);}
static Polynomial PairOne(PT a,PT b)
{
 if(!HasDegree(a,1)||!HasDegree(b,1))throw new InvalidOperationException("One-form pairing expected");
 Polynomial p=Polynomial.Constant(0);foreach(var q in a)foreach(var s in b)if(q.Key==s.Key)
 {int axis=BitOperations.TrailingZeroCount((uint)q.Key.Form);p+= (q.Value*s.Value).RealPart()*(-Sigma(axis)*BladeSign(q.Key.Blade,s.Key.Blade));}return p;
}
static object[] PTerms(PT t)=>t.OrderBy(q=>q.Key.Form).ThenBy(q=>q.Key.Blade).Select(q=>(object)new{formMask=q.Key.Form,cliffordMask=q.Key.Blade,polynomial=q.Value.Terms()}).ToArray();
static object ScalarText(Scalar q)=>new{real=q.Real.ToString(),imaginary=q.Imaginary.ToString()};
void Emit(string verdict,object evidence)
{
 var result=new{schemaVersion=1,phase=593,phaseId="phase593-companion-action-first-variation-audit",contractId=ContractId,
  contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,
  bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,
  verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
  authorityFirewalls=firewalls.ToDictionary(q=>q,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/companion_action_first_variation_audit.json",json);
 File.WriteAllText(Root+"/output/companion_action_first_variation_audit_summary.json",json);
 Console.WriteLine($"Phase593 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(q=>q is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();

sealed class Polynomial
{
 private readonly Dictionary<(int X,int Y,int Z),Scalar> terms=new();
 public bool IsZero=>terms.Count==0;
 public IEnumerable<Scalar> Coefficients=>terms.Values;
 public static Polynomial Constant(Scalar value){var p=new Polynomial();p.Put((0,0,0),value);return p;}
 public static Polynomial Variable(int axis){var p=new Polynomial();p.Put((axis==0?1:0,axis==1?1:0,axis==2?1:0),1);return p;}
 private void Put((int X,int Y,int Z) key,Scalar value){Scalar sum=terms.GetValueOrDefault(key)+value;if(sum.IsZero)terms.Remove(key);else terms[key]=sum;}
 public static Polynomial operator +(Polynomial a,Polynomial b){var p=new Polynomial();foreach(var q in a.terms)p.Put(q.Key,q.Value);foreach(var q in b.terms)p.Put(q.Key,q.Value);return p;}
 public static Polynomial operator *(Polynomial a,Polynomial b){var p=new Polynomial();foreach(var q in a.terms)foreach(var s in b.terms)p.Put((q.Key.X+s.Key.X,q.Key.Y+s.Key.Y,q.Key.Z+s.Key.Z),q.Value*s.Value);return p;}
 public static Polynomial operator *(Polynomial a,Scalar b){var p=new Polynomial();foreach(var q in a.terms)p.Put(q.Key,q.Value*b);return p;}
 public static Polynomial operator *(Polynomial a,int b)=>a*new Scalar(b);
 public Polynomial Derivative(int axis)
 {var p=new Polynomial();foreach(var q in terms){int n=axis==0?q.Key.X:axis==1?q.Key.Y:q.Key.Z;if(n>0)p.Put((q.Key.X-(axis==0?1:0),q.Key.Y-(axis==1?1:0),q.Key.Z-(axis==2?1:0)),q.Value*n);}return p;}
 public Polynomial AtZZero(){var p=new Polynomial();foreach(var q in terms)if(q.Key.Z==0)p.Put(q.Key,q.Value);return p;}
 public Polynomial RealPart(){var p=new Polynomial();foreach(var q in terms)p.Put(q.Key,new Scalar(q.Value.Real,0));return p;}
 public Scalar Evaluate(int x,int y,int z){Scalar s=0;foreach(var q in terms)s+=q.Value*Pow(x,q.Key.X)*Pow(y,q.Key.Y)*Pow(z,q.Key.Z);return s;}
 private static long Pow(int b,int exponent){long v=1;for(int i=0;i<exponent;i++)v=checked(v*b);return v;}
 public bool Same(Polynomial p)=>terms.Count==p.terms.Count&&terms.All(q=>p.terms.TryGetValue(q.Key,out var s)&&q.Value==s);
 public object[] Terms()=>terms.OrderBy(q=>q.Key).Select(q=>(object)new{x=q.Key.X,y=q.Key.Y,z=q.Key.Z,real=q.Value.Real.ToString(),imaginary=q.Value.Imaginary.ToString()}).ToArray();
}
