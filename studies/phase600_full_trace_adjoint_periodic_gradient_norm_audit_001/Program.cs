using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Gu.ReferenceCpu;
using static Algebra;
using static Fourier;
using static Adjoint;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P598="studies/phase598_continuum_action_descent_ward_audit_001";
const string P599="studies/phase599_source_registered_residual_kernel_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=P599+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase600-a54-full-trace-adjoint-periodic-gradient-norm-v1";
const string Success="full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch";
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "background":"fixed flat reference and metric; normalized periodic Fourier density in x0; epsilon=I",
 "chiralities":[-1,1],"formalCSlots":["1","c"],"gammaEncodings":[1,2],"massKappa":[0,1],
 "phi1":"P gamma, P=1+h Omega","phi2":"(c-i h Omega)Gamma2","operator":"full typed CCA, both Hodge legs and factor1/2",
 "pairing":"real bilinear -ReTr/128 times signed exterior metric; zero-mode Fourier convolution",
 "adjoints":{"C":"minus coefficient commutator","A":"same i anticommutator, no conjugation of i","wedge":"sigma(I)*shuffle(I,J), I subset output and J=output xor I","star":"(-1)^(p*(14-p))*star_(14-p)","d":"-sum_i sigma_i contraction_i partial_i","DQ":"full derivative S wedge V+V wedge S; its adjoint has no extra factor2"},
 "S":"theta2 E1 sin(x0), E1=-(Gamma01+Gamma23)/4","V":"theta0 gamma3 cos(x0)",
 "F":"theta02 E1 cos(x0)","Q":0,"sourceLieGram":"1/8","sourceFieldPair":"1/16",
 "fullKF":"P(theta2 gamma1-theta0 gamma3)cos(x0)/2","innerForwardOnF":0,
 "innerAdjointDerivative":"-P theta3 gamma0 cos/2-(ic/2)P sum_(j!=0,3)theta_j gamma0 gamma_j gamma3 cos",
 "L":"P[-theta0 gamma3/4+theta2 gamma1/2-theta3 gamma0/4-(ic/4)sum_(j!=0,3)theta_j gamma0 gamma_j gamma3]cos",
 "N":"P[(theta0 gamma0+theta1 gamma1)/4+(ic/4)(theta0 Gamma123-theta1 Gamma023)]sin^2",
 "actualGradient":"G=L+gamma*N/3+kappa*S, N=(DQ_S)^dagger K^dagger S",
 "directionalVariation":"1/8","shortcutDirectionalPair":"1/4",
 "normPolynomialOrder":["1","c","c^2"],"traceNorm":"kappa^2/16","upper13TraceNorm":"-kappa^2/16",
 "coefficientNorm":"3/8+gamma^2/96+c^2*(3/4+gamma^2/96)+kappa^2/16",
 "slotTermCounts":{"KAdjoint":[56,312],"L":[12,48],"N":[12,12],"Gkappa0":[24,60],"Gkappa1":[28,60]},
 "support":{"KForms":91,"ambientCliffordMasks":16384,"KCandidateMasks":"7 xor form,8 xor form,and their full14 complements","KCandidatesPerContext":364,"KExcludedPerContext":1490580,"KProbeHarmonics":["sin(x0)","cos(x0)"],"KNonzeroSinPerSlot":[28,156],"DQCandidatePositionsPerSlot":[8,48],"DQProbeHarmonics":["1","cos(2x0)","sin(2x0)"],"DQNonzeroPerContext":8,"realPhase":"1 for H-anti real blades, i otherwise","KRawCandidateUpperBound":1560,"DQRawCandidateUpperBound":6240},
 "blockControls":["C positive pairing2","A positive pairing2 and wrong-i sign pairing-2","negative form signature/shuffle C pairing2","star1 pairing-1","star2 pairing-1","d adjoint pairing1/2","full DQ pairing2, erroneous half1"],
 "fourierKnownAnswers":{"sin2":"1/2","cos2":"1/2","sin4":"3/8","cosSin2":0},
 "core":{"api":"Lambda2Algebra.MemberEndomorphism; LieAlgebraFactory.CreateSu2WithTracePairing","member":"sd2/id0 coefficient1/2","map":"(I+star4)/4","identity":"id0/none","input":"theta02 T1 cos(x0)","halfSquares":["1/32","1/4"],"meshInsertionPerformed":false},
 "expectedCounts":{"wordCases":256,"hodgeCases":16384,"blockControls":7,"contexts":4,"KForwardPairingChecks":2912,"KNonzeroPairings":368,"DQForwardPairingChecks":336,"DQNonzeroPairings":32,"innerAdjointRejections":4,"gradientRows":16,"normRows":8,"shortcutRejections":2,"coreMatrixEntries":108,"coreGramEntries":9,"coreStructureEntries":27,"coreResidualEntries":36,"coreNormRows":2},
 "exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":15,"maximumEstimatedCpuSeconds":90,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":268435456}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-adjoint-control-failed","full-adjoint-support-control-failed","actual-gradient-control-failed","declared-norm-control-failed","registered-comparison-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["arithmetic-helper"]=Root+"/ExactArithmetic.cs",["fourier-helper"]=Root+"/FourierTensor.cs",["adjoint-helper"]=Root+"/TraceAdjoint.cs",["project"]=Root+"/Phase600FullTraceAdjointPeriodicGradientNormAudit.csproj",["study"]=Root+"/STUDY.md",["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["phase598-summary"]=P598+"/output/continuum_action_descent_ward_audit_summary.json",["phase598-contract"]=P598+"/preregistration/contract_v1.json",["phase598-program"]=P598+"/Program.cs",["phase598-arithmetic"]=P598+"/ExactArithmetic.cs",["phase598-fourier"]=P598+"/FourierTensor.cs",["phase599-summary"]=P599+"/output/source_registered_residual_kernel_audit_summary.json",["phase599-contract"]=P599+"/preregistration/contract_v1.json",["phase599-program"]=P599+"/Program.cs",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==600&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase598","continuum-action-descent-ward-controls-pass-source-choice-open"),("phase599","source-registered-residual-kernel-mismatch-scoped")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");int wordCases=0,hodgeCases=0,blockControls=0;
bool knownAnswerPassed=true;int[] masks=Enumerable.Range(0,8).Concat(Enumerable.Range(0,8).Select(q=>Full^q)).ToArray();foreach(int a in masks)foreach(int b in masks){wordCases++;knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);}for(int m=0;m<=Full;m++){hodgeCases++;int p=Degree(m);knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((p*(14-p)+7)%2==0?1:-1);}
var blockOutput=new List<object>();
void CheckBlock(string id,FT phi,FT x,FT y,char kind,FT expectedForward,FT expectedAdjoint,Rational pairing)
{blockControls++;FT forward=Product(phi,x,kind),adjoint=BracketAdjoint(phi,y,kind);bool ok=Equal(forward,expectedForward)&&Equal(adjoint,expectedAdjoint)&&Pair(forward,y)==pairing&&Pair(x,adjoint)==pairing;if(kind=='A')ok&=Pair(x,Scale(adjoint,-1))==-2;knownAnswerPassed&=ok;blockOutput.Add(new{id,passed=ok,pairing=Pair(forward,y).ToString(),adjoint=Terms(adjoint)});}
FT phi=One(1,1,1),x=One(2,2,1),y=One(3,3,1);
CheckBlock("C",phi,x,y,'C',Scale(y,2),Scale(x,-2),2);
FT ax=One(2,1,1),ay=One(3,0,Scalar.I);CheckBlock("A",phi,ax,ay,'A',Scale(ay,2),Scale(ax,-2),2);
FT ny=One(130,3,1);CheckBlock("negative-form",One(128,1,1),x,ny,'C',Scale(ny,-2),Scale(x,-2),2);
foreach(int degree in new[]{1,2}){blockControls++;FT hx=One(degree==1?1:3,0,Scalar.I),hy=Star(hx),ha=StarAdjoint(hy,degree);bool ok=Pair(hy,hy)==-1&&Equal(ha,Scale(hx,-1))&&Pair(hx,ha)==-1;knownAnswerPassed&=ok;blockOutput.Add(new{id="hodge-"+degree,passed=ok});}
blockControls++;FT dx=Trig(0,true,2,0),dy=Trig(0,false,3,0);dx=Scale(dx,Scalar.I);dy=Scale(dy,Scalar.I);bool dBlock=Pair(D(dx),dy)==new Rational(1,2)&&Equal(DAdjoint(dy),dx);knownAnswerPassed&=dBlock;blockOutput.Add(new{id="derivative",passed=dBlock});
blockControls++;FT fullDQ=Add(Product(phi,x),Product(x,phi));bool qBlock=Pair(fullDQ,y)==2&&Pair(x,DQAdjoint(phi,y))==2&&Pair(Scale(fullDQ,new Scalar(new Rational(1,2),0)),y)==1;knownAnswerPassed&=qBlock;blockOutput.Add(new{id="full-DQ-not-half",passed=qBlock});
FT sine=Trig(0,true),cosine=Trig(0,false),sinSquared=Product(sine,sine);knownAnswerPassed&=CoefficientPair(sine,sine)==new Rational(1,2)&&CoefficientPair(cosine,cosine)==new Rational(1,2)&&CoefficientPair(sinSquared,sinSquared)==new Rational(3,8)&&CoefficientPair(cosine,sinSquared)==0;
knownAnswerPassed&=wordCases==expected.GetProperty("wordCases").GetInt32()&&hodgeCases==expected.GetProperty("hodgeCases").GetInt32()&&blockControls==expected.GetProperty("blockControls").GetInt32();
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,wordCases,hodgeCases,blockOutput});return;}

FT E1=Scale(Add(One(0,3,1),One(0,12,1)),new Scalar(new Rational(-1,4),0)),S=Product(One(4,0,1),Product(E1,sine)),V=Trig(0,false,1,8),F=D(S),Q=Product(S,S);
bool adjointPassed=HAnti(S)&&Q.Count==0&&Pair(E1,E1)==new Rational(1,8)&&Pair(S,S)==new Rational(1,16)&&D(V).Count==0,gradientPassed=true,normPassed=true;
FT gamma=new(),gamma2=new();for(int j=0;j<14;j++)gamma=Add(gamma,One(1<<j,1<<j,1));for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)gamma2=Add(gamma2,One((1<<a)|(1<<b),(1<<a)|(1<<b),1));
var pairs=Enumerable.Range(1,Full).Where(m=>Degree(m)==2).ToArray();
int contexts=0,KForwardPairingChecks=0,KNonzeroPairings=0,DQForwardPairingChecks=0,DQNonzeroPairings=0,innerAdjointRejections=0,gradientRows=0,normRows=0,shortcutRejections=0;
var contextOutput=new List<object>();var gradientOutput=new List<object>();var normOutput=new List<object>();
foreach(int h in fx.GetProperty("chiralities").EnumerateArray().Select(q=>q.GetInt32()))
{
 FT p1=Add(gamma,Scale(Omega(gamma),h));FT[] p2=[Scale(Omega(gamma2),Scalar.I*-h),gamma2];FT[] linear=new FT[2],nonlinear=new FT[2];
 for(int slot=0;slot<2;slot++)
 {
  contexts++;bool first=slot==0;FT actualY=KAdjointLiteral(S,p1,p2[slot],first),simplifiedY=KAdjointSimplified(S,p1,p2[slot],first),oracleY=ExpectedY(h,slot,sine);
  bool yPassed=Equal(actualY,simplifiedY)&&Equal(actualY,oracleY)&&actualY.Count==fx.GetProperty("slotTermCounts").GetProperty("KAdjoint")[slot].GetInt32()&&HAnti(actualY);
  var routed=SupportK(S,p1,p2[slot],first,pairs);var candidate=new HashSet<(int Form,int Blade)>();foreach(int form in pairs)foreach(int blade in new[]{7^form,8^form,Full^7^form,Full^8^form})candidate.Add((form,blade));
  bool support=routed.SetEquals(candidate)&&candidate.Count==364&&91*16384-candidate.Count==1490580&&actualY.Keys.All(k=>candidate.Contains((k.Form,k.Blade)))&&candidate.Count*2<=1560;
  int localNonzero=0;foreach(var position in candidate.OrderBy(q=>q.Form).ThenBy(q=>q.Blade))foreach(bool isSin in new[]{true,false})
  {FT probe=Trig(0,isSin,position.Form,position.Blade);probe=Scale(probe,AdjointSign(position.Blade)==-1?1:Scalar.I);Rational forward=Pair(S,Chain(probe,p1,p2[slot],first)),reverse=Pair(actualY,probe);KForwardPairingChecks++;yPassed&=forward==reverse;if(forward!=0){KNonzeroPairings++;localNonzero++;yPassed&=isSin;}}
  yPassed&=localNonzero==fx.GetProperty("support").GetProperty("KNonzeroSinPerSlot")[slot].GetInt32();adjointPassed&=yPassed&&support;
  // DQ closure depends on the fully certified Y above, not a predicted truncation.
  if(!adjointPassed){Emit(precedence[2],new{knownAnswerPassed,controlsPassed=false,h,slot,yPassed,support,localNonzero,KForwardPairingChecks,KNonzeroPairings});return;}
  FT n=DQAdjoint(S,actualY),oracleN=ExpectedN(h,slot,sinSquared);var qCandidates=SupportDQ(S,actualY);
  bool nPassed=Equal(n,oracleN)&&n.Count==fx.GetProperty("slotTermCounts").GetProperty("N")[slot].GetInt32()&&qCandidates.Count==fx.GetProperty("support").GetProperty("DQCandidatePositionsPerSlot")[slot].GetInt32()&&n.Keys.All(k=>qCandidates.Contains((k.Form,k.Blade)))&&S.Count*actualY.Count<=6240;
  int localQNonzero=0;foreach(var position in qCandidates.OrderBy(q=>q.Form).ThenBy(q=>q.Blade))for(int harmonic=0;harmonic<3;harmonic++)
  {FT probe=Product(One(position.Form,position.Blade,AdjointSign(position.Blade)==-1?1:Scalar.I),Harmonic(harmonic));Rational forward=Pair(Add(Product(S,probe),Product(probe,S)),actualY),reverse=Pair(probe,n);DQForwardPairingChecks++;nPassed&=forward==reverse;if(forward!=0){DQNonzeroPairings++;localQNonzero++;nPassed&=harmonic!=2;}}
  nPassed&=localQNonzero==8;adjointPassed&=nPassed;
  FT secondForward=Chain(F,p1,p2[slot],false),secondAdjoint=DAdjoint(KAdjointLiteral(S,p1,p2[slot],false));FT expectedSecond=ExpectedSecond(h,slot,cosine);
  bool inner=secondForward.Count==0&&Equal(secondAdjoint,expectedSecond)&&secondAdjoint.Count>0;innerAdjointRejections++;adjointPassed&=inner;
  FT l=QuadraticHessian(S,p1,p2[slot],first),oracleL=ExpectedL(h,slot,cosine);linear[slot]=l;nonlinear[slot]=n;
  FT kf=Chain(F,p1,p2[slot],first),oracleKF=first?Product(Scale(Add(POne(4,2,h),Scale(POne(1,8,h),-1)),new Scalar(new Rational(1,2),0)),cosine):new();
  bool lPassed=Equal(l,QuadraticHessianSimplified(S,p1,p2[slot],first))&&Equal(l,oracleL)&&l.Count==fx.GetProperty("slotTermCounts").GetProperty("L")[slot].GetInt32()&&Equal(kf,oracleKF);
  if(first){shortcutRejections++;lPassed&=Pair(V,kf)==new Rational(1,4)&&Pair(V,l)==new Rational(1,8);}
  gradientPassed&=lPassed;contextOutput.Add(new{h,slot,yPassed,support,nPassed,inner,lPassed,KCandidatePositions=candidate.Count,KExcludedPositions=1490580,KNonzero=localNonzero,DQCandidatePositions=qCandidates.Count,DQNonzero=localQNonzero,KAdjoint=Terms(actualY),linear=Terms(l),nonlinear=Terms(n),innerAdjointDerivative=Terms(secondAdjoint),KF=Terms(kf)});
 }
 foreach(int encoding in fx.GetProperty("gammaEncodings").EnumerateArray().Select(q=>q.GetInt32()))foreach(int kappa in fx.GetProperty("massKappa").EnumerateArray().Select(q=>q.GetInt32()))
 {
  FT[] gradients=new FT[2];for(int slot=0;slot<2;slot++)
  {
   gradientRows++;gradients[slot]=Add(Add(linear[slot],Scale(nonlinear[slot],new Scalar(new Rational(encoding,3),0))),slot==0?Scale(S,kappa):new());
   FT massless=Add(linear[slot],Scale(nonlinear[slot],new Scalar(new Rational(encoding,3),0)));
   FT dQ=Add(Product(S,V),Product(V,S));Rational direct=(Pair(V,Chain(F,p1,p2[slot],slot==0))+Pair(S,Chain(D(V),p1,p2[slot],slot==0)))*new Rational(1,2)+(Pair(V,Chain(Q,p1,p2[slot],slot==0))+Pair(S,Chain(dQ,p1,p2[slot],slot==0)))*new Rational(encoding,3)+(slot==0?Pair(S,V)*kappa:0);
   bool ok=HAnti(gradients[slot])&&Equal(Omega(massless),Scale(massless,h))&&massless.Keys.All(q=>Degree(q.Blade)%2==1)&&Pair(V,gradients[slot])==direct&&direct==(slot==0?new Rational(1,8):0)&&gradients[slot].Count==fx.GetProperty("slotTermCounts").GetProperty(kappa==0?"Gkappa0":"Gkappa1")[slot].GetInt32();
   gradientPassed&=ok;gradientOutput.Add(new{h,slot,encoding,kappa,passed=ok,directVariation=direct.ToString(),terms=Terms(gradients[slot])});
  }
  normRows++;Rational[] trace=NormPolynomial(gradients,Pair),positive=NormPolynomial(gradients,CoefficientPair),upper=NormPolynomial(gradients.Select(Star).ToArray(),Pair);
  Rational[] expectedTrace=[new(kappa*kappa,16),0,0],expectedPositive=[new Rational(3,8)+new Rational(encoding*encoding,96)+new Rational(kappa*kappa,16),0,new Rational(3,4)+new Rational(encoding*encoding,96)];
  bool norm=trace.SequenceEqual(expectedTrace)&&upper.SequenceEqual(expectedTrace.Select(q=>q*-1))&&positive.SequenceEqual(expectedPositive)&&positive[0]!=0&&positive[2]!=0;
  normPassed&=norm;normOutput.Add(new{h,encoding,kappa,passed=norm,tracePolynomial=Text(trace),upper13Polynomial=Text(upper),positiveCoefficientPolynomial=Text(positive)});
 }
}

// Actual registered pointwise APIs, never a substituted matrix or mesh action.
var coreLie=Gu.Math.LieAlgebraFactory.CreateSu2WithTracePairing();bool corePassed=true;int coreGramEntries=0,coreStructureEntries=0,coreMatrixEntries=0,coreResidualEntries=0,coreNormRows=0;
for(int i=0;i<3;i++)for(int j=0;j<3;j++){coreGramEntries++;corePassed&=coreLie.InvariantMetric[3*i+j]==(i==j?1:0);for(int k=0;k<3;k++){coreStructureEntries++;corePassed&=coreLie.StructureConstants[(3*i+j)*3+k]==Epsilon(i,j,k);}}
var member=new EinsteinianShiabFamilyMember{Phi1=InvariantElementSpec.Sd2,Phi2=InvariantElementSpec.Id0,EinsteinCoefficient=0.5,EpsilonMode="independent-theta"};var control=new EinsteinianShiabFamilyMember{Phi1=InvariantElementSpec.Id0,Phi2=InvariantElementSpec.None};
double[,] core=Lambda2Algebra.MemberEndomorphism(member),identity=Lambda2Algebra.MemberEndomorphism(control),star=Lambda2Algebra.HodgeStar();int[] masks4=[3,5,9,6,10,12];
for(int i=0;i<6;i++)for(int j=0;j<6;j++){double oracle=masks4[i]==(15^masks4[j])?Shuffle(masks4[j],15^masks4[j]):0;coreMatrixEntries+=3;corePassed&=star[i,j]==oracle&&identity[i,j]==(i==j?1:0)&&core[i,j]==((i==j?1:0)+oracle)/4;}
var coreOutput=new List<object>();int operatorIndex=0;foreach(var matrix in new[]{core,identity})
 {double sum=0;var coefficients=new List<double>();for(int i=0;i<6;i++)for(int lie=0;lie<3;lie++){double value=lie==0?matrix[i,1]:0,oracle=operatorIndex==0?(lie==0?(i==1?0.25:i==4?-0.25:0):0):(lie==0&&i==1?1:0);coreResidualEntries++;corePassed&=value==oracle;sum+=value*value;coefficients.Add(value);}double halfSquare=sum/4;coreNormRows++;corePassed&=halfSquare==(operatorIndex==0?1.0/32:1.0/4);coreOutput.Add(new{operatorIndex,halfSquare,coefficients});operatorIndex++;}
var observed=new Dictionary<string,int>{["wordCases"]=wordCases,["hodgeCases"]=hodgeCases,["blockControls"]=blockControls,["contexts"]=contexts,["KForwardPairingChecks"]=KForwardPairingChecks,["KNonzeroPairings"]=KNonzeroPairings,["DQForwardPairingChecks"]=DQForwardPairingChecks,["DQNonzeroPairings"]=DQNonzeroPairings,["innerAdjointRejections"]=innerAdjointRejections,["gradientRows"]=gradientRows,["normRows"]=normRows,["shortcutRejections"]=shortcutRejections,["coreMatrixEntries"]=coreMatrixEntries,["coreGramEntries"]=coreGramEntries,["coreStructureEntries"]=coreStructureEntries,["coreResidualEntries"]=coreResidualEntries,["coreNormRows"]=coreNormRows};
bool countsPassed=expected.EnumerateObject().Count()==observed.Count&&observed.All(q=>expected.GetProperty(q.Key).GetInt32()==q.Value),controlsPassed=adjointPassed&&gradientPassed&&normPassed&&corePassed&&countsPassed;
string verdict=!adjointPassed?precedence[2]:!gradientPassed?precedence[3]:!normPassed?precedence[4]:!corePassed||!countsPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,adjointPassed,gradientPassed,normPassed,corePassed,countsPassed,counts=observed,coefficientProducts=CoefficientProducts,largestTensorDuringAssembly=LargestTensor,blockOutput,contexts=contextOutput,gradientRows=gradientOutput,normRows=normOutput,registeredRows=coreOutput});

static FT POne(int form,int blade,int h)=>Add(One(form,blade,1),Scale(Omega(One(form,blade,1)),h));
static FT ExpectedY(int h,int slot,FT sine)
{
 FT t=new();if(slot==0){foreach(var q in new[]{(5,2,-1),(6,1,1),(9,1,1),(10,2,1)})t=Add(t,Scale(POne(q.Item1,q.Item2,h),q.Item3));for(int j=4;j<14;j++)t=Add(t,Scale(POne(8|(1<<j),1<<j,h),-1));}
 else for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)if(a!=3&&b!=3){int form=(1<<a)|(1<<b);t=Add(t,Scale(POne(form,form^8,h),Scalar.I*WordSign(form,8)));}
 return Scale(Product(t,sine),new Scalar(new Rational(1,2),0));
}
static FT ExpectedSecond(int h,int slot,FT cosine)
{FT t=new();if(slot==0)t=POne(8,1,h);else for(int j=1;j<14;j++)if(j!=3)t=Add(t,Scale(POne(1<<j,1^(1<<j)^8,h),Scalar.I*WordSign(1,1<<j)*WordSign(1^(1<<j),8)));return Scale(Product(t,cosine),new Scalar(new Rational(-1,2),0));}
static FT ExpectedL(int h,int slot,FT cosine)
{FT t=new();if(slot==0)t=Add(Add(Scale(POne(1,8,h),-1),Scale(POne(4,2,h),2)),Scale(POne(8,1,h),-1));else for(int j=1;j<14;j++)if(j!=3)t=Add(t,Scale(POne(1<<j,1^(1<<j)^8,h),Scalar.I*(-WordSign(1,1<<j)*WordSign(1^(1<<j),8))));return Scale(Product(t,cosine),new Scalar(new Rational(1,4),0));}
static FT ExpectedN(int h,int slot,FT sinSquared)=>Scale(Product(slot==0?Add(POne(1,1,h),POne(2,2,h)):Add(Scale(POne(1,14,h),Scalar.I),Scale(POne(2,13,h),Scalar.I*-1)),sinSquared),new Scalar(new Rational(1,4),0));
static HashSet<(int Form,int Blade)> SupportK(FT s,FT p1,FT p2,bool first,int[] forms)
{var result=new HashSet<(int,int)>();foreach(int form in forms)foreach(var y in s.Keys)foreach(var a in p1.Keys){if(first&&(a.Form&form)==a.Form&&(form^a.Form)==y.Form)result.Add((form,y.Blade^a.Blade));if(a.Form==y.Form)foreach(var b in p2.Keys)if(b.Form==form)result.Add((form,y.Blade^a.Blade^b.Blade));}return result;}
static HashSet<(int Form,int Blade)> SupportDQ(FT s,FT y)
{var result=new HashSet<(int,int)>();foreach(var a in s.Keys)foreach(var b in y.Keys)if((a.Form&b.Form)==a.Form)result.Add((b.Form^a.Form,b.Blade^a.Blade));return result;}
static FT Harmonic(int index)
{if(index==0)return One(0,0,1);var t=new FT();foreach(var q in Trig(0,index==2))Put(t,(q.Key.Form,q.Key.Blade,2*q.Key.K0,q.Key.K1),q.Value);return t;}
static Rational[] NormPolynomial(FT[] coefficients,Func<FT,FT,Rational> pairing)=>[pairing(coefficients[0],coefficients[0]),pairing(coefficients[0],coefficients[1])*2,pairing(coefficients[1],coefficients[1])];
static int Epsilon(int a,int b,int c)=>a==b||b==c||a==c?0:((a-b)*(b-c)*(c-a)>0?1:-1);
static string[] Text(Rational[] q)=>q.Select(v=>v.ToString()).ToArray();
void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=600,phaseId="phase600-full-trace-adjoint-periodic-gradient-norm-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/full_trace_adjoint_periodic_gradient_norm_audit.json",json);File.WriteAllText(Root+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",json);Console.WriteLine($"Phase600 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
