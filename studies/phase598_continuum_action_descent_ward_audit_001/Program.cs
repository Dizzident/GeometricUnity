using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Numerics;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase598_continuum_action_descent_ward_audit_001";
const string P597="studies/phase597_actual_quadratic_joint_null_audit_001";
const string P594="studies/phase594_actual_gradient_reciprocity_audit_001";
const string P592="studies/phase592_companion_tensor_chirality_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=P592+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase598-a52-continuum-action-descent-ward-v1";
const string Success="continuum-action-descent-ward-controls-pass-source-choice-open";
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "background":"fixed flat reference, metric and normalized periodic Fourier density; no physical decomposition",
 "a":1,"chiralities":[-1,1],"formalCSlots":["1","c"],"gammaEncodings":[1,2],"massKappa":[0,1],
 "phi1":"(1+h Omega)gamma","phi2":"(c-i h Omega)Gamma2","brackets":"full tied CCA, outer Hodge and factor1/2 retained",
 "pairing":"-ReTr/128 with signed exterior metric; indefinite; all real parts taken exactly",
 "nilpotents":{"N":"gamma2+Gamma12","M":"gamma2-Gamma12","N2":0,"M2":0,"NM":"2(1+gamma1)","MN":"2(1-gamma1)","commutator":"4gamma1","MNM":"4M"},
 "f":"sin(x0)","g":"sin(x1)","epsilon":"(1+fN)(1+gM)","inverse":"(1-gM)(1-fN)",
 "B":"(N+4g gamma1-4g^2 M)df+Mdg","FB":"zero with nonzero cancelling dB and B^2",
 "fixtures":[
  {"id":"constant-cubic","S":"theta0 Gamma01+theta1 Gamma12+theta1 gamma2","V":"theta1 gamma2","actionPieces":["0","0","4","1/2"],"directionPieces":["0","0","4","-1"]},
  {"id":"fourier-quadratic","S":"u+v; u=theta0 gamma2 cos(x0),v=theta1 Gamma12 sin(x0)","V":"u","actionPieces":["0","-1/2","0","0"],"directionPieces":["0","-1/2","0","-1/2"]}
 ],
 "pieceOrder":["background","quadraticWithHalf","rawCubic","massWithHalf"],"action":"background+quadratic+gamma*rawCubic/3+kappa*mass",
 "coordinates":"T=epsilon^-1 S epsilon; omega=B+T; both tensors conjugated independently",
 "fixedEpsilonDirection":"delta omega=epsilon^-1 V epsilon; delta epsilon=0",
 "rightAlpha":"eta=N sin(x0)","wardDirection":"delta epsilon=epsilon eta,delta omega=d eta+[omega,eta]",
 "epsilonOnlyDirection":"delta epsilon=epsilon eta,delta omega=0; compare actual variation with deltaS=-Ad(epsilon)D_omega eta",
 "variationMethod":"independent exact dual-number product rules through inverse, B, T, both Phi tensors, curvature, covariant derivative and all action terms",
 "finiteOrbit":"right g_rho=1+rho eta and inverse1-rho eta; exact coefficientwise inverse/unitarity and reconstructed S invariance through polynomial degree4; not a separately executed full action polynomial",
 "decoyAlpha":"gamma2 sin(x0)","decoyS":"lambda(u+v)","decoyKappa":0,
 "decoys":{"wrongLift":"delta omega=-D_S alpha,delta epsilon=alpha; residual lambda-2gamma lambda^3/3","frozenTensors":"correct plus lift but deltaPhi=0; residual gamma lambda^3/3","correctLift":"full product variation residual zero","tensorCancellation":"-gamma lambda^3/3","Ktheta01gamma1":"2 sum_(j=2..13)theta_j Gamma0j","meanCos2Sin2":"1/8","lambda1Wrong":["1/3","-1/3"],"lambda1Frozen":["1/3","2/3"]},
 "knownAnswerMenu":{"masks":"0..7 and Omega xor(0..7)","kernelTensors":"I,N,M,sin(x0),cos(x1),theta0 gamma2 cos(x0),theta1 Gamma12 sin(x0),star(theta01 Gamma12)","products":["wedge","C","A"],"bigInteger":"2^100 exact rational reduction"},
 "expectedCounts":{"wordCases":256,"hodgeCases":16384,"kernelParityCases":192,"nilpotentIdentities":6,"finiteOrbitRows":2,"contexts":8,"actionRows":32,"fixedDirectionRows":32,"wardRows":32,"epsilonOnlyRows":32,"operatorDescentComparisons":16,"topTraceComparisons":24,"decoyRows":8,"wrongLiftRejections":4,"frozenTensorRejections":4,"nonzeroBaseActionContexts":4},
 "exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":60,"maximumEstimatedCpuSeconds":300,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":536870912,"epsilonTermUpperBound":17,"constantTTermUpperBound":200,"fourierTTermUpperBound":280,"phi1TermUpperBound":2800,"phi2TermUpperBound":18200}
}
""";
// Exactly the same fourteen authority keys as the preceding frozen phases.
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","nilpotent-background-orbit-control-failed","full-action-descent-control-failed","actual-variation-ward-control-failed","nonzero-decoy-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["arithmetic-helper"]=Root+"/ExactArithmetic.cs",["fourier-helper"]=Root+"/FourierTensor.cs",["project"]=Root+"/Phase598ContinuumActionDescentWardAudit.csproj",["study"]=Root+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["phase597-summary"]=P597+"/output/actual_quadratic_joint_null_audit_summary.json",["phase597-contract"]=P597+"/preregistration/contract_v1.json",["phase597-program"]=P597+"/Program.cs",["phase597-fourier-helper"]=P597+"/FourierTensor.cs",
 ["phase594-summary"]=P594+"/output/actual_gradient_reciprocity_audit_summary.json",["phase594-contract"]=P594+"/preregistration/contract_v1.json",["phase594-program"]=P594+"/Program.cs",["phase592-algebra-helper"]=P592+"/ExactAlgebra.cs",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==598&&contract.GetProperty("contractId").GetString()==ContractId
  &&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(v=>v.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(v=>new Binding(v.GetProperty("id").GetString()!,v.GetProperty("path").GetString()!,v.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(v=>v.id).Distinct().Count()==paths.Count&&bindings.Select(v=>v.path).Distinct().Count()==paths.Count&&bindings.All(v=>paths.TryGetValue(v.id,out var p)&&p==v.path&&v.hashMatches);
 if(contractValid&&exactBindingsValid)
 {using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(v=>v.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(v=>Sha(v.GetProperty("path").GetString()!)==v.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase597","actual-quadratic-joint-null-controls-pass-dynamics-unselected"),("phase594","actual-gradient-controls-pass-curvature-only-force-rejected")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var u=ud.RootElement;bool ok=u.GetProperty("auditPassed").GetBoolean()&&u.GetProperty("contractValid").GetBoolean()&&u.GetProperty("exactBindingsValid").GetBoolean()&&u.GetProperty("coreSourceTreeValid").GetBoolean()&&u.GetProperty("verdictKind").GetString()==terminal&&u.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&u.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&u.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&u.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>u.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&u.GetProperty("externalReviewPending").GetBoolean()&&u.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(6.20)","(7.3)","(7.4)","(9.3)","(9.4)","(9.7)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchorsValid=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var counts=fx.GetProperty("expectedCounts");int wordCases=0,hodgeCases=0,kernelParityCases=0;
bool knownAnswerPassed=true;int[] masks=Enumerable.Range(0,8).Concat(Enumerable.Range(0,8).Select(m=>Full^m)).ToArray();foreach(int m in masks)foreach(int n in masks){wordCases++;knownAnswerPassed&=BladeSign(m,n)==WordSign(m,n);}for(int m=0;m<=Full;m++){hodgeCases++;int d=Degree(m);knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((d*(14-d)+7)%2==0?1:-1);}
FT identity=One(0,0,1),N=Add(One(0,4,1),One(0,6,1)),M=Add(One(0,4,1),One(0,6,-1));FT f=Trig(0,true),g=Trig(1,true),cos=Trig(0,false),uMode=Trig(0,false,1,4),vMode=Trig(0,true,2,6);
FT[] kernelMenu=[identity,N,M,f,Trig(1,false),uMode,vMode,Star(One(3,6,1))];foreach(var a in kernelMenu)foreach(var b in kernelMenu)foreach(char kind in new[]{'W','C','A'}){kernelParityCases++;knownAnswerPassed&=Equal(Product(a,b,kind),NaiveProduct(a,b,kind));}
BigInteger large=BigInteger.One<<100;knownAnswerPassed&=new Rational(large*3,large*7)==new Rational(3,7)&&new Rational(large,3)*new Rational(3,large)==1;
knownAnswerPassed&=Equal(Partial(f,0),cos)&&Equal(Partial(cos,0),Scale(f,-1))&&D(D(Product(f,g))).Count==0&&Pair(Product(cos,cos),Product(f,f))==new Rational(-1,8);
knownAnswerPassed&=wordCases==counts.GetProperty("wordCases").GetInt32()&&hodgeCases==counts.GetProperty("hodgeCases").GetInt32()&&kernelParityCases==counts.GetProperty("kernelParityCases").GetInt32();
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,wordCases,hodgeCases,kernelParityCases});return;}

FT epsilon=Product(Add(identity,Product(f,N)),Add(identity,Product(g,M))),inverse=Product(Add(identity,Scale(Product(g,M),-1)),Add(identity,Scale(Product(f,N),-1)));
bool[] nilChecks=[Product(N,N).Count==0,Product(M,M).Count==0,Equal(Product(N,M),Scale(Add(identity,One(0,2,1)),2)),Equal(Product(M,N),Scale(Add(identity,One(0,2,-1)),2)),Equal(Product(N,M,'C'),One(0,2,4)),Equal(Product(Product(M,N),M),Scale(M,4))];int nilpotentIdentities=nilChecks.Length;
FT bkg=Product(inverse,D(epsilon)),predB=Add(Product(Add(Add(N,Scale(Product(g,One(0,2,1)),4)),Scale(Product(Product(g,g),M),-4)),D(f)),Product(M,D(g)));
FT db=D(bkg),bb=Product(bkg,bkg),fb=Add(db,bb);
bool backgroundPassed=nilChecks.All(q=>q)&&HAnti(N)&&HAnti(M)&&Equal(HAdjoint(epsilon),inverse)&&Equal(Product(inverse,epsilon),identity)&&Equal(Product(epsilon,inverse),identity)&&Equal(Product(HAdjoint(epsilon),epsilon),identity)&&Equal(Product(epsilon,HAdjoint(epsilon)),identity)&&Equal(bkg,predB)&&HAnti(bkg)&&db.Count>0&&bb.Count>0&&fb.Count==0;
bool supportPassed=epsilon.Count<=17&&inverse.Count<=17;
FT gamma=new(),gamma2=new();for(int j=0;j<14;j++)gamma=Add(gamma,One(1<<j,1<<j,1));for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)gamma2=Add(gamma2,One((1<<a)|(1<<b),(1<<a)|(1<<b),1));
FT sC=Add(Add(One(1,3,1),One(2,6,1)),One(2,4,1)),sQ=Add(uMode,vMode),eta=Product(f,N);
int finiteOrbitRows=0,contexts=0,actionRows=0,fixedDirectionRows=0,wardRows=0,epsilonOnlyRows=0,operatorDescentComparisons=0,topTraceComparisons=0,decoyRows=0,wrongLiftRejections=0,frozenTensorRejections=0,nonzeroBaseActionContexts=0;
bool orbitPassed=true,descentPassed=true,variationPassed=true,wardPassed=true,decoysPassed=true;var orbitOutput=new List<object>();var actionOutput=new List<object>();var contextOutput=new List<object>();var decoyOutput=new List<object>();
foreach(var (id,sBase,vBase) in new[]{("constant-cubic",sC,One(2,4,1)),("fourier-quadratic",sQ,uMode)})
{
 FT torsion=Conjugate(inverse,sBase,epsilon),omega=Add(bkg,torsion),dAeta=Add(D(eta),Product(omega,eta,'C'));
 supportPassed&=torsion.Count<=(id=="constant-cubic"?200:280);backgroundPassed&=HAnti(torsion)&&HAnti(omega);
 FT[] right=[identity,eta],rightInverse=[identity,Scale(eta,-1)],ep=PMul([epsilon],right),ei=PMul(rightInverse,[inverse]);
 FT[] omegaP=PAdd(PMul(PMul(rightInverse,[omega]),right),PMul(rightInverse,PD(right))),bP=PMul(ei,PD(ep));
 FT[] tP=PAdd(omegaP,PScale(bP,-1)),sP=PMul(PMul(ep,tP),ei);
 bool orbit=PConstant(PMul(right,rightInverse),identity)&&PConstant(PMul(rightInverse,right),identity)&&PConstant(PMul(right.Select(HAdjoint).ToArray(),right),identity)&&PConstant(sP,sBase)&&Equal(omegaP[1],dAeta)&&omegaP.Skip(1).Any(q=>q.Count>0);
 finiteOrbitRows++;orbitPassed&=orbit;orbitOutput.Add(new{id,passed=orbit,reconstructedDegree=sP.Length-1,nonconstantReconstructedTermCounts=sP.Skip(1).Select(q=>q.Count).ToArray(),rightOmegaTermCounts=omegaP.Select(q=>q.Count).ToArray()});
 foreach(int h in fx.GetProperty("chiralities").EnumerateArray().Select(q=>q.GetInt32()))
 {
  FT p1=Add(gamma,Scale(Omega(gamma),h));FT[] p2=[Scale(Omega(gamma2),Scalar.I*-h),gamma2];
  FT transformedP1=Conjugate(inverse,p1,epsilon);supportPassed&=transformedP1.Count<=2800;
  for(int slot=0;slot<2;slot++)
  {
   contexts++;bool first=slot==0;FT transformedP2=Conjugate(inverse,p2[slot],epsilon);supportPassed&=transformedP2.Count<=18200;
   backgroundPassed&=HAnti(p1)&&HAnti(p2[slot])&&HAnti(transformedP1)&&HAnti(transformedP2);
   FT fixedV=Conjugate(inverse,vBase,epsilon);
   var actual=EvaluateAction(new Jet(epsilon,new()),new Jet(inverse,new()),new Jet(omega,fixedV),p1,p2[slot],first);
   var baseEval=EvaluateAction(Jet.Fixed(identity),Jet.Fixed(identity),new Jet(sBase,vBase),p1,p2[slot],first);
   FT dS=D(sBase),qS=Product(sBase,sBase);FT kd=Chain(dS,p1,p2[slot],first),kq=Chain(qS,p1,p2[slot],first);
   bool operators=Equal(actual.DT.Value,Conjugate(inverse,dS,epsilon))&&Equal(actual.Q.Value,Conjugate(inverse,qS,epsilon))&&Equal(actual.KD.Value,Conjugate(inverse,kd,epsilon))&&Equal(actual.KQ.Value,Conjugate(inverse,kq,epsilon));operatorDescentComparisons+=2;
   bool literal=Equal(actual.T.Value,torsion)&&Equal(actual.B.Value,bkg)&&actual.FB.Value.Count==0&&Equal(actual.P1.Value,transformedP1)&&Equal(actual.P2.Value,transformedP2)&&Equal(actual.KD.Value,Chain(actual.DT.Value,transformedP1,transformedP2,first))&&Equal(actual.KQ.Value,Chain(actual.Q.Value,transformedP1,transformedP2,first));
   Rational[] predictedBase=first?(id=="constant-cubic"?[0,0,4,new(1,2)]:[0,new(-1,2),0,0]):[0,0,0,0];
   Rational[] predictedDirection=first?(id=="constant-cubic"?[0,0,4,-1]:[0,new(-1,2),0,new(-1,2)]):[0,0,0,0];
   Rational[] directFixed=BaseDerivative(sBase,vBase,p1,p2[slot],first);
   bool topTrace=Top(Product(actual.T.Value,Star(actual.KD.Value)))*new Rational(1,2)==actual.Values[1]&&Top(Product(actual.T.Value,Star(actual.KQ.Value)))==actual.Values[2]&&Top(Product(actual.T.Value,Star(actual.T.Value)))*new Rational(1,2)==Pair(actual.T.Value,actual.T.Value)*new Rational(1,2);topTraceComparisons+=3;
   bool action=operators&&literal&&topTrace&&actual.Values.SequenceEqual(baseEval.Values)&&baseEval.Values.SequenceEqual(predictedBase);
   if(first){nonzeroBaseActionContexts++;action&=predictedBase.Any(q=>q!=0);}
   bool fixedDirection=actual.Deltas.SequenceEqual(baseEval.Deltas)&&baseEval.Deltas.SequenceEqual(directFixed)&&directFixed.SequenceEqual(predictedDirection);
   var eJet=new Jet(epsilon,Product(epsilon,eta));var iJet=new Jet(inverse,Scale(Product(eta,inverse),-1));
   backgroundPassed&=Jet.Product(eJet,iJet).Delta.Count==0&&Jet.Product(iJet,eJet).Delta.Count==0;
   var ward=EvaluateAction(eJet,iJet,new Jet(omega,dAeta),p1,p2[slot],first);
   var vertical=EvaluateAction(eJet,iJet,Jet.Fixed(omega),p1,p2[slot],first);
   FT pulledVertical=Scale(Conjugate(epsilon,dAeta,inverse),-1);
   Rational[] verticalOracle=BaseDerivative(sBase,pulledVertical,p1,p2[slot],first);
   FT reconstructedWard=Add(Add(Product(Product(eJet.Delta,ward.T.Value),inverse),Product(Product(epsilon,ward.T.Delta),inverse)),Product(Product(epsilon,ward.T.Value),iJet.Delta));
   bool wardOk=ward.Deltas.All(q=>q==0)&&reconstructedWard.Count==0&&Equal(ward.B.Delta,Add(D(eta),Product(bkg,eta,'C')))&&Equal(ward.T.Delta,Product(torsion,eta,'C'))&&ward.FB.Delta.Count==0&&Equal(ward.P1.Delta,Product(transformedP1,eta,'C'))&&Equal(ward.P2.Delta,Product(transformedP2,eta,'C'));
   bool verticalOk=vertical.Deltas.SequenceEqual(verticalOracle)&&vertical.T.Delta.Count>0&&eJet.Delta.Count>0;
   descentPassed&=action;variationPassed&=fixedDirection&&verticalOk;wardPassed&=wardOk;
   contextOutput.Add(new{id,h,slot,actionPassed=action,fixedDirectionPassed=fixedDirection,wardPassed=wardOk,epsilonOnlyPassed=verticalOk,torsionTerms=torsion.Count,phi1Terms=transformedP1.Count,phi2Terms=transformedP2.Count,basePieces=Text(baseEval.Values),actualPieces=Text(actual.Values),fixedDerivativePieces=Text(actual.Deltas),wardDerivativePieces=Text(ward.Deltas),epsilonOnlyPieces=Text(vertical.Deltas),epsilonOnlyOracle=Text(verticalOracle),baseKD=Terms(kd),baseKQ=Terms(kq)});
   foreach(int encoding in fx.GetProperty("gammaEncodings").EnumerateArray().Select(q=>q.GetInt32()))foreach(int kappa in fx.GetProperty("massKappa").EnumerateArray().Select(q=>q.GetInt32()))
   {actionRows++;fixedDirectionRows++;wardRows++;epsilonOnlyRows++;Rational value=Combine(actual.Values,encoding,kappa),derivative=Combine(actual.Deltas,encoding,kappa);bool ok=value==Combine(predictedBase,encoding,kappa)&&derivative==Combine(predictedDirection,encoding,kappa)&&Combine(ward.Deltas,encoding,kappa)==0&&Combine(vertical.Deltas,encoding,kappa)==Combine(verticalOracle,encoding,kappa);descentPassed&=ok;actionOutput.Add(new{id,h,slot,encoding,kappa,passed=ok,value=value.ToString(),fixedDerivative=derivative.ToString(),wardDerivative=Combine(ward.Deltas,encoding,kappa).ToString(),epsilonOnlyDerivative=Combine(vertical.Deltas,encoding,kappa).ToString()});}
  }
 }
}

// Both Ward decoys are exact formal lambda polynomials at epsilon=I, not fits.
FT alpha=Trig(0,true,0,4),dAlpha=D(alpha),beta=Product(sQ,alpha,'C');
FT[] sPoly=[new(),sQ],dSPoly=PD(sPoly),qPoly=PMul(sPoly,sPoly),plusOmega=[dAlpha,beta],minusOmega=PScale(plusOmega,-1);
bool decoyGeometry=Equal(dAlpha,uMode)&&Equal(beta,Scale(Product(One(2,2,1),Product(f,f)),2));
foreach(int h in fx.GetProperty("chiralities").EnumerateArray().Select(q=>q.GetInt32()))
{
 FT p1=Add(gamma,Scale(Omega(gamma),h));FT[] p2=[Scale(Omega(gamma2),Scalar.I*-h),gamma2];FT extraExpected=new();for(int j=2;j<14;j++)extraExpected=Add(extraExpected,One(1<<j,1|(1<<j),2));
 for(int slot=0;slot<2;slot++)
 {
  bool first=slot==0;FT dp1=Product(p1,alpha,'C'),dp2=Product(p2[slot],alpha,'C');
  FT[] DK(FT[] input)=>input.Select(z=>Add(Chain(z,dp1,p2[slot],first),Chain(z,p1,dp2,false))).ToArray();
  FT[] kD=PChain(dSPoly,p1,p2[slot],first),kQ=PChain(qPoly,p1,p2[slot],first);
  Rational[][] Variation(FT[] deltaOmega,bool moveTensors)
  {
   FT[] deltaT=PAdd(deltaOmega,[Scale(dAlpha,-1)]),deltaDT=PAdd(PD(deltaT),PAdd(PMul([dAlpha],sPoly),PMul(sPoly,[dAlpha]))),deltaQ=PAdd(PMul(deltaT,sPoly),PMul(sPoly,deltaT));
   FT[] deltaKD=PChain(deltaDT,p1,p2[slot],first),deltaKQ=PChain(deltaQ,p1,p2[slot],first);
   if(moveTensors){deltaKD=PAdd(deltaKD,DK(dSPoly));deltaKQ=PAdd(deltaKQ,DK(qPoly));}
   return[RScale(RAdd(PPair(deltaT,kD),PPair(sPoly,deltaKD)),new Rational(1,2)),RAdd(PPair(deltaT,kQ),PPair(sPoly,deltaKQ))];
  }
  var wrong=Variation(minusOmega,true);var frozen=Variation(plusOmega,false);var correct=Variation(plusOmega,true);
  Rational[] tensorQuad=RScale(PPair(sPoly,DK(dSPoly)),new Rational(1,2)),tensorCubic=PPair(sPoly,DK(qPoly));
  FT[] pullbackWrong=[Scale(uMode,-2),Scale(beta,-2)];
  Rational[] independentWrongQuad=RScale(RAdd(PPair(pullbackWrong,kD),PPair(sPoly,PChain(PD(pullbackWrong),p1,p2[slot],first))),new Rational(1,2));
  Rational[] independentWrongCubic=RAdd(PPair(pullbackWrong,kQ),PPair(sPoly,PChain(PAdd(PMul(pullbackWrong,sPoly),PMul(sPoly,pullbackWrong)),p1,p2[slot],first)));
  FT actualExtra=Chain(One(3,2,1),p1,p2[slot],first);bool support=Equal(actualExtra,first?extraExpected:new());
  foreach(int encoding in fx.GetProperty("gammaEncodings").EnumerateArray().Select(q=>q.GetInt32()))
  {
   decoyRows++;Rational weight=new(encoding,3);Rational[] wrongTotal=RAdd(wrong[0],RScale(wrong[1],weight)),frozenTotal=RAdd(frozen[0],RScale(frozen[1],weight)),correctTotal=RAdd(correct[0],RScale(correct[1],weight)),tensorTotal=RAdd(tensorQuad,RScale(tensorCubic,weight));
   Rational[] wp=first?[0,1,0,new(-2*encoding,3)]:[0],fp=first?[0,0,0,new(encoding,3)]:[0],tp=first?[0,0,0,new(-encoding,3)]:[0];
   bool ok=decoyGeometry&&support&&REqual(wrongTotal,wp)&&REqual(frozenTotal,fp)&&REqual(correctTotal,[0])&&REqual(tensorTotal,tp)&&REqual(wrong[0],independentWrongQuad)&&REqual(wrong[1],independentWrongCubic);
   if(first){wrongLiftRejections++;frozenTensorRejections++;ok&=wrongTotal.Aggregate(new Rational(0),(s,z)=>s+z)!=0&&frozenTotal.Aggregate(new Rational(0),(s,z)=>s+z)!=0;}
   decoysPassed&=ok;decoyOutput.Add(new{h,slot,encoding,passed=ok,wrongLift=Text(wrongTotal),frozenTensors=Text(frozenTotal),correctLift=Text(correctTotal),tensorCancellation=Text(tensorTotal),wrongQuadratic=Text(wrong[0]),wrongRawCubic=Text(wrong[1]),frozenQuadratic=Text(frozen[0]),frozenRawCubic=Text(frozen[1]),extraOutput=Terms(actualExtra)});
  }
 }
}
var observed=new Dictionary<string,int>{["wordCases"]=wordCases,["hodgeCases"]=hodgeCases,["kernelParityCases"]=kernelParityCases,["nilpotentIdentities"]=nilpotentIdentities,["finiteOrbitRows"]=finiteOrbitRows,["contexts"]=contexts,["actionRows"]=actionRows,["fixedDirectionRows"]=fixedDirectionRows,["wardRows"]=wardRows,["epsilonOnlyRows"]=epsilonOnlyRows,["operatorDescentComparisons"]=operatorDescentComparisons,["topTraceComparisons"]=topTraceComparisons,["decoyRows"]=decoyRows,["wrongLiftRejections"]=wrongLiftRejections,["frozenTensorRejections"]=frozenTensorRejections,["nonzeroBaseActionContexts"]=nonzeroBaseActionContexts};
bool countsPassed=counts.EnumerateObject().Count()==observed.Count&&observed.All(q=>counts.GetProperty(q.Key).GetInt32()==q.Value);
bool controlsPassed=backgroundPassed&&orbitPassed&&supportPassed&&descentPassed&&variationPassed&&wardPassed&&decoysPassed&&countsPassed;
string verdict=!backgroundPassed||!orbitPassed||!supportPassed?precedence[2]:!descentPassed?precedence[3]:!variationPassed||!wardPassed?precedence[4]:!decoysPassed||!countsPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,backgroundPassed,orbitPassed,supportPassed,descentPassed,variationPassed,wardPassed,decoysPassed,countsPassed,counts=observed,coefficientProducts=CoefficientProducts,largestTensorDuringAssembly=LargestTensor,epsilon=Terms(epsilon),inverse=Terms(inverse),B=Terms(bkg),dB=Terms(db),BSquared=Terms(bb),orbitRows=orbitOutput,contexts=contextOutput,actionRows=actionOutput,decoyRows=decoyOutput});

static ActionResult EvaluateAction(Jet epsilon,Jet inverse,Jet omega,FT p1,FT p2,bool first)
{
 Jet b=Jet.Product(inverse,Jet.D(epsilon)),t=Jet.Add(omega,Jet.Scale(b,-1));Jet phi1=Jet.Product(Jet.Product(inverse,Jet.Fixed(p1)),epsilon),phi2=Jet.Product(Jet.Product(inverse,Jet.Fixed(p2)),epsilon);
 Jet fb=Jet.Add(Jet.D(b),Jet.Product(b,b)),dt=Jet.Add(Jet.D(t),Jet.Add(Jet.Product(b,t),Jet.Product(t,b))),q=Jet.Product(t,t);
 Jet kfb=Jet.Chain(fb,phi1,phi2,first),kd=Jet.Chain(dt,phi1,phi2,first),kq=Jet.Chain(q,phi1,phi2,first);
 var bg=Jet.Pair(t,kfb);var quadratic=Jet.Pair(t,kd);var cubic=Jet.Pair(t,kq);var mass=Jet.Pair(t,t);
 Rational[] values=[bg.Value,quadratic.Value*new Rational(1,2),cubic.Value,first?mass.Value*new Rational(1,2):0];Rational[] deltas=[bg.Delta,quadratic.Delta*new Rational(1,2),cubic.Delta,first?mass.Delta*new Rational(1,2):0];
 return new(values,deltas,b,t,phi1,phi2,fb,dt,q,kd,kq);
}
static Rational[] BaseDerivative(FT s,FT v,FT p1,FT p2,bool first)
{
 FT ds=D(s),dv=D(v),q=Product(s,s),dq=Add(Product(v,s),Product(s,v));
 Rational quadratic=(Pair(v,Chain(ds,p1,p2,first))+Pair(s,Chain(dv,p1,p2,first)))*new Rational(1,2),cubic=Pair(v,Chain(q,p1,p2,first))+Pair(s,Chain(dq,p1,p2,first));return[0,quadratic,cubic,first?Pair(s,v):0];
}
static Rational Combine(Rational[] p,int encoding,int kappa)=>p[0]+p[1]+p[2]*new Rational(encoding,3)+p[3]*kappa;
static string[] Text(Rational[] p)=>p.Select(q=>q.ToString()).ToArray();
void Emit(string terminal,object evidence)
{
 var result=new{schemaVersion=1,phase=598,phaseId="phase598-continuum-action-descent-ward-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(v=>v.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(v=>v.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/continuum_action_descent_ward_audit.json",json);File.WriteAllText(Root+"/output/continuum_action_descent_ward_audit_summary.json",json);Console.WriteLine($"Phase598 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
sealed record ActionResult(Rational[] Values,Rational[] Deltas,Jet B,Jet T,Jet P1,Jet P2,Jet FB,Jet DT,Jet Q,Jet KD,Jet KQ);
