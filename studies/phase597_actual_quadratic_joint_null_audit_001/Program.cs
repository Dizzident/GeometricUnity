using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static FourierTools;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;
using PM=System.Collections.Generic.Dictionary<(int Row,int Column),long>;

const string Root="studies/phase597_actual_quadratic_joint_null_audit_001";
const string P592="studies/phase592_companion_tensor_chirality_audit_001";
const string P594="studies/phase594_actual_gradient_reciprocity_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=P592+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase597-a51-actual-quadratic-joint-null-v1";
const string Success="actual-quadratic-joint-null-controls-pass-dynamics-unselected";
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "background":"flat fourteen-torus; periodic x0,x1; normalized Fourier integration; other coordinates constant",
 "operators":[{"a":1,"h":-1},{"a":1,"h":1},{"a":-1,"h":-1},{"a":-1,"h":1}],
 "phi1":"a(1+h Omega)gamma","phi2":"(c-i h Omega)Gamma2","formalCSlots":["1","c"],
 "chain":"full tied CCA; upper=[Phi1 wedge star F]_C-(1/2)star[Phi1 wedge star[Phi2 wedge star F]_A]_C; lower=inverse star1 upper",
 "pairing":"-ReTr/128 times signed exterior metric; no positivity assumption",
 "carrier":["u=theta0 gamma2 cos(x0)","v=theta1 Gamma12 sin(x0)"],
 "curvatureBasis":"w=theta01 Gamma12 cos(x0)","du":"0","dv":"w","dAdjointW":"v",
 "gram":[["-1/2","0"],["0","1/2"]],"curvatureGram":"1/2",
 "fullKw":"2a theta0 (1+h Omega)gamma2 cos(x0)","innerA":"zero in both formal c slots",
 "rawCovectorMatrix":[["0","-a"],["0","0"]],
 "quadraticAction":"-a*s*t/2+kappa*(-s*s+t*t)/4","massKappa":[-1,0,1],
 "actualHessian":[["-kappa/2","-a/2"],["-a/2","kappa/2"]],
 "adjointScope":"full chain followed by exact finite Fourier Gram pullback; finite K adjoint, not claimed ambient K adjoint",
 "jointCoordinates":["s","t","r"],"jointJ":[[1,0,-1],[0,1,0]],"jointNull":[1,0,1],"wrongLiftJ":[[1,0,1],[0,1,0]],
 "epsilon":"exp(r gamma2 sin(x0)); B=epsilon^-1 d epsilon=r u exactly; T=(s-r)u+t v",
 "epsilonTensorJet":"Phi_epsilon=Phi+r[Phi,alpha]+O(r^2), alpha=gamma2 sin(x0); metric and trace fixed",
 "connectionJet":"D_B T=dT+r(u wedge T+T wedge u); K and connection corrections have total amplitude degree at least3 in the quadratic action",
 "epsilonJetAverages":"all first-order tensor/connection correction matrix entries vanish by Fourier parity; unaveraged connection correction on v is nonzero; tensor jet independently equals K[alpha,F]-[alpha,KF]",
 "maurerCartan":{"f":"sin(x0)","g":"sin(x1)","X":"gamma2","Y":"Gamma12","commutator":"-2 gamma1","epsilon":"exp(r f X)exp(r g Y)","E1":"fX+gY","E2":"f^2 X^2/2+fgXY+g^2 Y^2/2","B1":"dE1","B2":"dE2-E1 dE1=g df[X,Y]","dB2":"-df wedge dg[X,Y]","B1Squared":"df wedge dg[X,Y]","wrongMinusCurvature":"4 df wedge dg gamma1"},
 "gradeControl":"P gamma_j has distinct nonzero grades1 and13; grade1 projection=a gamma_j; all14 axes and four operators",
 "stationarityBackgrounds":["flat","Weyl R0101=R2323=1,R0202=R1313=-1","constant sectional curvature1, R_abab=sigma_a sigma_b"],
 "stationarityScope":"separate pointwise algebraic Riemann anchors, not the curvature of the Fourier flat-torus background",
 "stationarityPredictions":{"scalar":[0,0,182],"ricciConstant":"13 eta","fullForceConstant":"78 P gamma","forceSelfPairing":0,"mixedVariation":"theta0 (1-h Omega)gamma0/2","mixedPairingConstant":"-78a"},
 "expectedCounts":{"wordCases":256,"hodgeCases":16384,"operatorRows":4,"chainSlots":8,"rawReciprocityRejections":4,"massRows":12,"jointRows":12,"wrongLiftRejections":12,"fixedEpsilonNonNullRows":12,"epsilonJetRows":8,"adjointPairingChecks":32,"gradeRows":56,"stationarityRows":24,"maurerCartanRows":2},
 "exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":3,"maximumEstimatedCpuSeconds":15,"estimatedPeakBytes":67108864,"maximumEstimatedPeakBytes":134217728}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","full-chain-adjoint-control-failed","quadratic-joint-control-failed","background-flatness-jet-control-failed","grade-stationarity-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["fourier-helper"]=Root+"/FourierTensor.cs",["project"]=Root+"/Phase597ActualQuadraticJointNullAudit.csproj",["study"]=Root+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["phase594-summary"]=P594+"/output/actual_gradient_reciprocity_audit_summary.json",["phase594-contract"]=P594+"/preregistration/contract_v1.json",["phase594-program"]=P594+"/Program.cs",["polynomial-helper"]=P594+"/PolynomialTensor.cs",
 ["phase592-summary"]=P592+"/output/companion_tensor_chirality_audit_summary.json",["phase592-contract"]=P592+"/preregistration/contract_v1.json",["phase592-program"]=P592+"/Program.cs",["algebra-helper"]=P592+"/ExactAlgebra.cs",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var doc=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=doc.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==597&&contract.GetProperty("contractId").GetString()==ContractId
  &&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()
  &&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
  &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(v=>v.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(v=>new Binding(v.GetProperty("id").GetString()!,v.GetProperty("path").GetString()!,v.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(v=>v.id).Distinct().Count()==paths.Count&&bindings.Select(v=>v.path).Distinct().Count()==paths.Count&&bindings.All(v=>paths.TryGetValue(v.id,out var p)&&p==v.path&&v.hashMatches);
 if(contractValid&&exactBindingsValid)
 {
  using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();
  coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
   &&live.Length==contract.GetProperty("fixtures").GetProperty("coreFileCount").GetInt32()&&entries.Select(v=>v.GetProperty("path").GetString()).SequenceEqual(live)
   &&entries.All(v=>Sha(v.GetProperty("path").GetString()!)==v.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();
 }
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,upstreamTerminal) in new[]{("phase594","actual-gradient-controls-pass-curvature-only-force-rejected"),("phase592","companion-tensor-chirality-controls-pass-source-choice-open")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var upstream=ud.RootElement;
  bool ok=upstream.GetProperty("auditPassed").GetBoolean()&&upstream.GetProperty("contractValid").GetBoolean()&&upstream.GetProperty("exactBindingsValid").GetBoolean()&&upstream.GetProperty("coreSourceTreeValid").GetBoolean()
   &&upstream.GetProperty("verdictKind").GetString()==upstreamTerminal&&upstream.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])
   &&upstream.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&upstream.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()
   &&upstream.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>upstream.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)
   &&upstream.GetProperty("externalReviewPending").GetBoolean()&&upstream.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(6.20)","(7.3)","(7.4)","(9.3)","(9.4)","(9.7)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchorsValid=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");
int wordCases=0,hodgeCases=0;bool wordPassed=true,hodgePassed=true;
int[] masks=Enumerable.Range(0,8).Concat(Enumerable.Range(0,8).Select(v=>Full^v)).ToArray();
foreach(int a in masks)foreach(int b in masks){wordCases++;wordPassed&=BladeSign(a,b)==WordProductSign(a,b);}
for(int m=0;m<=Full;m++){hodgeCases++;int r=Degree(m);hodgePassed&=HodgeSign(m)*HodgeSign(Full^m)==((r*(14-r)+7)%2==0?1:-1);}
FT sin=Trig(0,true),cos=Trig(0,false),sin1=Trig(1,true),cos1=Trig(1,false);
bool fourierPassed=Equal(Partial(sin,0),cos)&&Equal(Partial(cos,0),Scale(sin,-1))&&Partial(sin,1).Count==0
 &&Pair(sin,sin)==new Rational(-1,2)&&Pair(cos,cos)==new Rational(-1,2)&&Pair(sin,cos)==0
 &&Equal(Add(Wedge(sin,sin),Wedge(cos,cos)),One(0,0,1))&&ExteriorD(ExteriorD(Wedge(sin,sin1))).Count==0;
var s=Polynomial.Variable(0);var t=Polynomial.Variable(1);var rvar=Polynomial.Variable(2);Polynomial[] amplitudes=[s,t];
bool polynomialPassed=EqualM(Hessian(s*t*new Scalar(new Rational(-1,2),0),2),new Rational[][]{[0,new(-1,2)],[new(-1,2),0]});
bool knownAnswerPassed=wordPassed&&hodgePassed&&fourierPassed&&polynomialPassed&&wordCases==expected.GetProperty("wordCases").GetInt32()&&hodgeCases==expected.GetProperty("hodgeCases").GetInt32();
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,wordPassed,hodgePassed,fourierPassed,polynomialPassed,wordCases,hodgeCases});return;}

FT u=Trig(0,false,1,4),v=Trig(0,true,2,6),w=Trig(0,false,3,6),alpha=Trig(0,true,0,4);
FT[] e=[u,v];Rational[][] gram=e.Select(q=>e.Select(z=>Pair(q,z)).ToArray()).ToArray();Rational[][] gi=[[ -2,0],[0,2]];
Rational gf=Pair(w,w);Rational[][] dm=[[0,1]],dAdj=MM(MM(gi,Transpose(dm)),new Rational[][]{[gf]});
bool chainPassed=EqualM(gram,new Rational[][]{[new(-1,2),0],[0,new(1,2)]})&&gf==new Rational(1,2)&&ExteriorD(u).Count==0&&Equal(ExteriorD(v),w)&&Equal(AdjointD(w),v)&&e.All(HAnti)&&HAnti(w);
bool adjointPassed=true,quadraticPassed=true,jointPassed=true,jetPassed=true,gradePassed=true,stationarityPassed=true;
int operatorRows=0,chainSlots=0,rawReciprocityRejections=0,massRows=0,jointRows=0,wrongLiftRejections=0,fixedEpsilonNonNullRows=0,epsilonJetRows=0,adjointPairingChecks=0,gradeRows=0,stationarityRows=0;
var chainRows=new List<object>();var massOutput=new List<object>();var jetOutput=new List<object>();var stationarityOutput=new List<object>();
FT gamma=new(),gamma2=new();for(int j=0;j<14;j++)gamma=Add(gamma,One(1<<j,1<<j,1));foreach(var (a,b) in Pairs())gamma2=Add(gamma2,One((1<<a)|(1<<b),(1<<a)|(1<<b),1));
Rational[][] jointJ=fx.GetProperty("jointJ").EnumerateArray().Select(row=>row.EnumerateArray().Select(q=>new Rational(q.GetInt32())).ToArray()).ToArray();
Rational[][] wrongJ=fx.GetProperty("wrongLiftJ").EnumerateArray().Select(row=>row.EnumerateArray().Select(q=>new Rational(q.GetInt32())).ToArray()).ToArray();Rational[] nullVector=[1,0,1];
PM flat=new(),weyl=new();foreach(var q in new[]{(0,1,1L),(2,3,1L),(0,2,-1L),(1,3,-1L)})weyl[(Algebra.Pair(q.Item1,q.Item2),Algebra.Pair(q.Item1,q.Item2))]=q.Item3;
PM constant=new();foreach(var (a,b) in Pairs())constant[(Algebra.Pair(a,b),Algebra.Pair(a,b))]=Sigma(a)*Sigma(b);
foreach(var op in fx.GetProperty("operators").EnumerateArray())
{
 operatorRows++;int a=op.GetProperty("a").GetInt32(),h=op.GetProperty("h").GetInt32();FT p1=Scale(Add(gamma,Scale(OmegaF(gamma),h)),a);FT[] p2=[Scale(OmegaF(gamma2),Scalar.I*-h),gamma2];
 Rational[][] kinetic=[];
 for(int slot=0;slot<2;slot++)
 {
  chainSlots++;var result=Chain(w,p1,p2[slot],slot==0);FT predicted=slot==0?Scale(Add(u,Scale(OmegaF(u),h)),2*a):new();
  bool literal=result.Types&&Equal(result.Lower,predicted)&&result.Inner.Count==0&&HAnti(result.Lower);
  Rational[] k=e.Select(q=>Pair(q,result.Lower)).ToArray();Rational[][] km=MM(gi,k.Select(z=>new[]{z}).ToArray());
  Rational[][] kadj=Times(MM(Transpose(km),gram),new Rational(gf.Denominator,gf.Numerator));
  Rational[][] raw=MM(gram,MM(km,dm)),oracle=slot==0?new Rational[][]{[0,-a],[0,0]}:new Rational[][]{[0,0],[0,0]};
  Rational[][] adjointH=Times(MM(gram,Plus(MM(km,dm),MM(dAdj,kadj))),new Rational(1,2));
  for(int i=0;i<2;i++){adjointPairingChecks+=2;adjointPassed&=k[i]==gf*kadj[0][i]&&Pair(ExteriorD(e[i]),w)==Pair(e[i],AdjointD(w));}
  Rational[][] upperRaw=e.Select(q=>e.Select(z=>Top(Wedge(q,Chain(ExteriorD(z),p1,p2[slot],slot==0).Upper))).ToArray()).ToArray();
  Polynomial action=Quad(raw,amplitudes)*new Scalar(new Rational(1,2),0);Rational[][] directH=Hessian(action,2);
  bool ok=literal&&EqualM(raw,oracle)&&EqualM(upperRaw,raw)&&EqualM(directH,adjointH)&&EqualM(directH,Transpose(directH));
  if(slot==0){rawReciprocityRejections++;ok&=!EqualM(raw,Transpose(raw));kinetic=directH;}else ok&=action.IsZero;
  chainPassed&=ok;chainRows.Add(new{a,h,slot,passed=ok,fullK=TermsF(result.Lower),raw=MatrixText(raw),finiteK=MatrixText(km),finiteKAdjoint=MatrixText(kadj),action=action.Terms(),directHessian=MatrixText(directH),adjointHessian=MatrixText(adjointH)});
  // First derivative of the declared simultaneous conjugation of both tensors.
  FT dp1=Bracket(p1,alpha,'C'),dp2=Bracket(p2[slot],alpha,'C');
  FT[] connection=e.Select(z=>Add(Wedge(u,z),Wedge(z,u))).ToArray();
  FT[] dk=e.Select(z=>Add(Chain(ExteriorD(z),dp1,p2[slot],slot==0).Lower,Chain(ExteriorD(z),p1,dp2,false).Lower)).ToArray();
  FT[] dkOracle=e.Select(z=>Add(Scale(Chain(Bracket(ExteriorD(z),alpha,'C'),p1,p2[slot],slot==0).Lower,-1),Bracket(Chain(ExteriorD(z),p1,p2[slot],slot==0).Lower,alpha,'C'))).ToArray();
  Rational[][] correction=e.Select(q=>Enumerable.Range(0,2).Select(j=>Pair(q,Add(dk[j],Chain(connection[j],p1,p2[slot],slot==0).Lower))).ToArray()).ToArray();
  Polynomial cubic=Quad(correction,[s+rvar*-1,t])*rvar*new Scalar(new Rational(1,2),0);
  bool jet=connection[0].Count==0&&connection[1].Count>0&&dk.Zip(dkOracle,Equal).All(q=>q)&&EqualM(correction,new Rational[][]{[0,0],[0,0]})&&cubic.IsZero;
  epsilonJetRows++;jetPassed&=jet;jetOutput.Add(new{a,h,slot,passed=jet,connectionV=TermsF(connection[1]),tensorJetOnDv=TermsF(dk[1]),integratedCorrection=MatrixText(correction),cubicAction=cubic.Terms(),minimumUnaveragedAmplitudeDegree=3});
 }
 foreach(int kappa in fx.GetProperty("massKappa").EnumerateArray().Select(q=>q.GetInt32()))
 {
  massRows++;Rational[][] actual=Plus(kinetic,Times(gram,kappa));Polynomial action=Quad(actual,amplitudes)*new Scalar(new Rational(1,2),0);
  Polynomial predicted=s*t*new Scalar(new Rational(-a,2),0)+(s*s*-1+t*t)*new Scalar(new Rational(kappa,4),0);
  Rational[][] raised=MM(gi,actual);bool quad=action.Same(predicted)&&EqualM(Hessian(action,2),actual)&&EqualM(MM(gram,raised),Transpose(MM(gram,raised)))&&!EqualM(raised,Transpose(raised));quadraticPassed&=quad;
  Polynomial jointAction=Quad(actual,[s+rvar*-1,t])*new Scalar(new Rational(1,2),0);Rational[][] joint=Hessian(jointAction,3),pullback=MM(MM(Transpose(jointJ),actual),jointJ),wrong=MM(MM(Transpose(wrongJ),actual),wrongJ);
  bool nullOk=MV(joint,nullVector).All(q=>q==0),wrongRejected=MV(wrong,nullVector).Any(q=>q!=0),fixedNotNull=MV(actual,[1,0]).Any(q=>q!=0);
  bool jointOk=EqualM(joint,pullback)&&EqualM(joint,Transpose(joint))&&nullOk&&wrongRejected&&fixedNotNull;jointPassed&=jointOk;jointRows++;if(wrongRejected)wrongLiftRejections++;if(fixedNotNull)fixedEpsilonNonNullRows++;
  massOutput.Add(new{a,h,kappa,quadraticPassed=quad,jointPassed=jointOk,action=action.Terms(),hessian=MatrixText(actual),raisedJacobian=MatrixText(raised),jointAction=jointAction.Terms(),jointHessian=MatrixText(joint),wrongLiftHessian=MatrixText(wrong),nullOk,wrongRejected,fixedNotNull});
 }
 for(int j=0;j<14;j++)
 {gradeRows++;FT g=One(1<<j,1<<j,1),pg=Scale(Add(g,Scale(OmegaF(g),h)),a);var gradeOne=new FT(pg.Where(q=>Degree(q.Key.Blade)==1));gradePassed&=pg.Count==2&&Equal(gradeOne,Scale(g,a))&&pg.Keys.All(q=>Degree(q.Blade) is 1 or 13)&&Pair(pg,pg)==0;}
 foreach(var (id,curv) in new[]{("flat",flat),("weyl",weyl),("constant",constant)})
 {
  var rs=RicciScalar(curv);FT f=new();foreach(var q in Curvature(curv,Pairs()))f=Add(f,One(q.Key.Form,q.Key.Blade,q.Value));
  FT jform=new();for(int b=0;b<14;b++)for(int d=0;d<14;d++)jform=Add(jform,One(1<<b,1<<d,rs.Ricci[b,d]*Sigma(d)));
  FT einstein=Add(Scale(jform,-1),Scale(gamma,new Scalar(new Rational(rs.Scalar,2),0)));FT prediction=Scale(Add(einstein,Scale(OmegaF(einstein),h)),a);
  FT mixed=Scale(Add(One(1,1,1),Scale(OmegaF(One(1,1,1)),-h)),FourierTools.Half);
  for(int slot=0;slot<2;slot++)
  {
   stationarityRows++;var actual=Chain(f,p1,p2[slot],slot==0);FT target=slot==0?prediction:new();bool isConstant=id=="constant";Rational pairing=Pair(mixed,actual.Lower);
   bool ok=actual.Types&&Equal(actual.Lower,target)&&Pair(actual.Lower,actual.Lower)==0&&pairing==(isConstant&&slot==0?-78*a:0)
    &&rs.Scalar==(isConstant?182:0)&&(isConstant?actual.Lower.Count==(slot==0?28:0):actual.Lower.Count==0);
   if(isConstant)for(int b=0;b<14;b++)for(int d=0;d<14;d++)ok&=rs.Ricci[b,d]==(b==d?13*Sigma(b):0);
   else ok&=rs.Ricci.Cast<long>().All(q=>q==0)&&(id=="flat"?f.Count==0:f.Count>0);
   stationarityPassed&=ok;stationarityOutput.Add(new{a,h,background=id,slot,passed=ok,scalar=rs.Scalar,curvatureTerms=f.Count,force=TermsF(actual.Lower),forceSelfPairing=Pair(actual.Lower,actual.Lower).ToString(),mixedPairing=pairing.ToString()});
  }
 }
}

// Independently assemble epsilon and inverse through second order before Maurer-Cartan.
FT X=One(0,4,1),Y=One(0,6,1),fxX=Wedge(sin,X),gyY=Wedge(sin1,Y);
FT e1=Add(fxX,gyY),e2=Add(Add(Scale(Wedge(fxX,fxX),FourierTools.Half),Wedge(fxX,gyY)),Scale(Wedge(gyY,gyY),FourierTools.Half));
FT inverse1=Scale(e1,-1),inverse2=Add(Wedge(e1,e1),Scale(e2,-1));
bool inverseJet=Add(e1,inverse1).Count==0&&Add(Add(e2,Wedge(inverse1,e1)),inverse2).Count==0&&Add(Add(e2,Wedge(e1,inverse1)),inverse2).Count==0;
FT b1=ExteriorD(e1),b2=Add(ExteriorD(e2),Wedge(inverse1,ExteriorD(e1))),comm=Bracket(X,Y,'C');
FT b2Expected=Wedge(Wedge(sin1,ExteriorD(sin)),comm),db2=ExteriorD(b2),square=Wedge(b1,b1),dfdg=Wedge(ExteriorD(sin),ExteriorD(sin1));
FT minus=Add(db2,Scale(square,-1)),wrongExpected=Scale(Wedge(dfdg,One(0,2,1)),4);
bool twoGenerator=inverseJet&&Equal(comm,One(0,2,-2))&&Equal(b2,b2Expected)&&ExteriorD(b1).Count==0&&db2.Count>0&&square.Count>0&&Equal(db2,Scale(Wedge(dfdg,comm),-1))&&Equal(square,Wedge(dfdg,comm))&&Add(db2,square).Count==0&&Equal(minus,wrongExpected)&&minus.Count>0;
FT oneE2=Scale(Wedge(alpha,alpha),FourierTools.Half),oneB2=Add(ExteriorD(oneE2),Scale(Wedge(alpha,ExteriorD(alpha)),-1));
bool oneGenerator=Equal(ExteriorD(alpha),u)&&Bracket(alpha,ExteriorD(alpha),'C').Count==0&&oneB2.Count==0&&ExteriorD(u).Count==0&&Wedge(u,u).Count==0;
int maurerCartanRows=2;jetPassed&=oneGenerator&&twoGenerator;
var actualCounts=new Dictionary<string,int>{["wordCases"]=wordCases,["hodgeCases"]=hodgeCases,["operatorRows"]=operatorRows,["chainSlots"]=chainSlots,["rawReciprocityRejections"]=rawReciprocityRejections,["massRows"]=massRows,["jointRows"]=jointRows,["wrongLiftRejections"]=wrongLiftRejections,["fixedEpsilonNonNullRows"]=fixedEpsilonNonNullRows,["epsilonJetRows"]=epsilonJetRows,["adjointPairingChecks"]=adjointPairingChecks,["gradeRows"]=gradeRows,["stationarityRows"]=stationarityRows,["maurerCartanRows"]=maurerCartanRows};
bool countsPassed=expected.EnumerateObject().Count()==actualCounts.Count&&actualCounts.All(q=>expected.GetProperty(q.Key).GetInt32()==q.Value);
bool controlsPassed=chainPassed&&adjointPassed&&quadraticPassed&&jointPassed&&jetPassed&&gradePassed&&stationarityPassed&&countsPassed;
string terminal=!chainPassed||!adjointPassed?precedence[2]:!quadraticPassed||!jointPassed?precedence[3]:!jetPassed?precedence[4]:!gradePassed||!stationarityPassed||!countsPassed?precedence[5]:Success;
Emit(terminal,new{knownAnswerPassed,controlsPassed,chainPassed,adjointPassed,quadraticPassed,jointPassed,jetPassed,gradePassed,stationarityPassed,countsPassed,counts=actualCounts,gram=MatrixText(gram),curvatureGram=gf.ToString(),dMatrix=MatrixText(dm),dAdjointMatrix=MatrixText(dAdj),chainRows,massRows=massOutput,epsilonJetRows=jetOutput,stationarityRows=stationarityOutput,maurerCartan=new{oneGenerator,twoGenerator,inverseJet,b1=TermsF(b1),b2=TermsF(b2),db2=TermsF(db2),b1Squared=TermsF(square),wrongMinus=TermsF(minus)}});

void Emit(string verdict,object evidence)
{
 var result=new{schemaVersion=1,phase=597,phaseId="phase597-actual-quadratic-joint-null-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(v=>v.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(v=>v.path).Distinct().Count(),bindings,verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/actual_quadratic_joint_null_audit.json",json);File.WriteAllText(Root+"/output/actual_quadratic_joint_null_audit_summary.json",json);Console.WriteLine($"Phase597 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
