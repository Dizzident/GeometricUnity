using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase614_conditional_metric_homothety_stationarity_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P611="studies/phase611_untied_caa_response_joint_gauge_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase614-a60-conditional-metric-homothety-stationarity-v1";
const string Success="metric-homothety-controls-pass-source-admissibility-open";
const string FixtureJson="""
{
 "dimension": 14,
 "signature": [
  1,
  1,
  1,
  1,
  1,
  1,
  1,
  -1,
  -1,
  -1,
  -1,
  -1,
  -1,
  -1
 ],
 "operator": "canonical untied CAA, firstC/outerA/innerA, fixed gamma and kappa, no extra action terms",
 "metric": "independent declared g_lambda=lambda^2 g; source MET(X)-induced admissibility NOT established",
 "reference": "B=0,FB=0,epsilon=I,constant coordinate fields",
 "lambda": [
  "1/2",
  "1",
  "2"
 ],
 "cases": [
  {
   "id": "branch-s-1-g1",
   "s": -1,
   "gamma": 1,
   "kappa": 312,
   "branch": true
  },
  {
   "id": "branch-s-1-g2",
   "s": -1,
   "gamma": 2,
   "kappa": 624,
   "branch": true
  },
  {
   "id": "branch-s1-g1",
   "s": 1,
   "gamma": 1,
   "kappa": -312,
   "branch": true
  },
  {
   "id": "branch-s1-g2",
   "s": 1,
   "gamma": 2,
   "kappa": -624,
   "branch": true
  },
  {
   "id": "offbranch-s-1-g1",
   "s": -1,
   "gamma": 1,
   "kappa": 1,
   "branch": false
  },
  {
   "id": "offbranch-s-1-g2",
   "s": -1,
   "gamma": 2,
   "kappa": 1,
   "branch": false
  },
  {
   "id": "offbranch-s1-g1",
   "s": 1,
   "gamma": 1,
   "kappa": 1,
   "branch": false
  },
  {
   "id": "offbranch-s1-g2",
   "s": 1,
   "gamma": 2,
   "kappa": 1,
   "branch": false
  },
  {
   "id": "zero-g1-k1",
   "s": 0,
   "gamma": 1,
   "kappa": 1,
   "branch": false
  },
  {
   "id": "zero-g0-k0",
   "s": 0,
   "gamma": 0,
   "kappa": 0,
   "branch": false
  },
  {
   "id": "mass-only-k-1",
   "s": 1,
   "gamma": 0,
   "kappa": -1,
   "branch": false
  },
  {
   "id": "mass-only-k1",
   "s": 1,
   "gamma": 0,
   "kappa": 1,
   "branch": false
  },
  {
   "id": "cubic-only-s-1",
   "s": -1,
   "gamma": 1,
   "kappa": 0,
   "branch": false
  },
  {
   "id": "cubic-only-s1",
   "s": 1,
   "gamma": 1,
   "kappa": 0,
   "branch": false
  },
  {
   "id": "zero-action-s-1",
   "s": -1,
   "gamma": 0,
   "kappa": 0,
   "branch": false
  },
  {
   "id": "zero-action-s1",
   "s": 1,
   "gamma": 0,
   "kappa": 0,
   "branch": false
  }
 ],
 "phiPowers": [
  1,
  2
 ],
 "hodgePower": "14-2r",
 "scalarPairingPower": "-2r",
 "volumePower": 14,
 "forwardStagePowers": [
  0,
  10,
  11,
  12,
  -2,
  -1,
  11,
  -1
 ],
 "reverseStagePowers": [
  0,
  12,
  1,
  0,
  -1,
  13,
  11,
  1
 ],
 "KFixedPower": -1,
 "KdagFixedPower": 1,
 "DQdagFixedPower": -2,
 "curvatureFixtures": [
  "Gamma2",
  "theta01 Gamma01/2",
  "theta01 Gamma23",
  "theta01 Gamma12"
 ],
 "forwardFirst": [
  "-26Gamma1",
  "-theta0 gamma0-theta1 gamma1",
  "0",
  "2theta0 gamma2"
 ],
 "forwardSecond": [
  "182Gamma1",
  "Gamma1",
  "-2sum(j4..13)theta_j Gamma0123j",
  "0"
 ],
 "adjointInputs": [
  "Gamma1",
  "theta0 gamma2"
 ],
 "adjointAnswers": [
  "-24Gamma2",
  "2theta0 wedge(gamma2 wedgeCl Gamma1)"
 ],
 "nonzeroAdjointBasePairings": [
  -2184,
  -12,
  -2
 ],
 "maskMenu": "empty/full and prefix/7-axis-rotated prefix masks of lengths1..13;28 distinct",
 "paths": [
  "fixed-coordinate T=sGamma1",
  "transported T_lambda=s lambda Gamma1"
 ],
 "sourceAnchors": [
  "Q=2s^2 lambda^(2t)Gamma2",
  "KQ=312s^2 lambda^(2t-1)Gamma1",
  "KdagT=-24s lambda^(t+1)Gamma2",
  "DQdagKdagT=624s^2 lambda^(2t-1)Gamma1"
 ],
 "pathExponent": "t=0 fixed,t=1 transported",
 "gradient": "312gamma s^2 lambda^(2t-1)Gamma1+kappa s lambda^t Gamma1",
 "action": "C lambda^(11+3t)+M lambda^(12+2t),C=-1456gamma s^3,M=-7kappa s^2",
 "originalActionRoute": "full top-form T wedge metricStar(KQ) gamma/3 plus mass; independent density-weighted signed pairing",
 "branchDerivativeAt1": "10192gamma s^3",
 "chainRuleAt1": "movingDerivative-fixedDerivative=Pair(G,S)=-4368gamma s^3-14kappa s^2",
 "wrongPhiPowers": [
  -2,
  -4
 ],
 "wrongPairAction": "C lambda13+M lambda14",
 "wrongPairDerivativeDifferenceAt1": "2(C+M)",
 "expectedCounts": {
  "hodgeCases": 16384,
  "wordCases": 44944,
  "generatorWordCases": 458752,
  "scaledHodge": 84,
  "scaledPairings": 84,
  "forwardFixtures": 4,
  "forwardStagePolynomials": 32,
  "forwardEvaluations": 12,
  "reverseFixtures": 2,
  "reverseStagePolynomials": 16,
  "reverseEvaluations": 6,
  "adjointPairings": 24,
  "nonzeroAdjointPairings": 9,
  "wrongPhiScaleRejections": 8,
  "wrongPhiDerivativeRejections": 4,
  "omittedFirstRejections": 9,
  "omittedSecondRejections": 9,
  "fieldCases": 16,
  "fieldPaths": 32,
  "sourceAnchorPolynomials": 128,
  "sourceGradientLegPolynomials": 96,
  "gradientPolynomials": 32,
  "originalActionLegPolynomials": 128,
  "actionPolynomials": 32,
  "actionDerivativePolynomials": 32,
  "caseScaleContexts": 48,
  "actionValues": 96,
  "actionDerivativeValues": 96,
  "gradientValues": 96,
  "chainRuleRows": 16,
  "offBranchChainRuleRows": 8,
  "fieldStationaryRows": 8,
  "nonzeroMetricBranchRows": 4,
  "fixedTransportBranchRejections": 8,
  "wrongPairDerivativeRejections": 12,
  "wrongPairScaleRejections": 24
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "resources": {
  "estimatedCpuSeconds": 20,
  "maximumEstimatedCpuSeconds": 120,
  "estimatedPeakBytes": 134217728,
  "maximumEstimatedPeakBytes": 536870912,
  "maximumCoefficientProducts": 50000000,
  "maximumSparseTensorTerms": 65536,
  "maximumLaurentMonomials": 4,
  "maximumAbsoluteExponent": 28,
  "maximumFrequency": 0
 },
 "scope": {
  "sourceMetricVariationAdmissible": false,
  "sourceOperatorSelected": false,
  "additionalActionTermsIncluded": false,
  "couplingsVaried": false,
  "globalVacuumRejected": false,
  "physicalMetricEquationSelected": false,
  "fullDifferentialSpectrumComputed": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","metric-chain-control-failed","full-adjoint-control-failed","original-action-control-failed","field-gradient-control-failed","chain-rule-control-failed","metric-nonstationarity-control-failed","count-or-resource-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase614ConditionalMetricHomothetyStationarityAudit.csproj",["study"]=Root+"/STUDY.md",["control-helper"]=Root+"/MetricLaurent.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["caa-helper"]=P611+"/CaaOperator.cs",["phase611-program"]=P611+"/Program.cs",["phase611-study"]=P611+"/STUDY.md",["phase611-summary"]=P611+"/output/untied_caa_response_joint_gauge_audit_summary.json",["phase611-contract"]=P611+"/preregistration/contract_v1.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==614&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase611","untied-caa-response-controls-pass-source-choice-open")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);Rational[] scales=[new(1,2),1,2];
bool knownAnswerPassed=true,chainsPassed=true,adjointsPassed=true,actionPassed=true,gradientPassed=true,chainRulePassed=true,metricPassed=true;var operatorRows=new List<object>();var fieldRows=new List<object>();
for(int m=0;m<=Full;m++){int d=Degree(m);counts["hodgeCases"]++;knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((d*(14-d)+7)%2==0?1:-1);for(int i=0;i<14;i++){int g=1<<i;counts["generatorWordCases"]+=2;knownAnswerPassed&=BladeSign(m,g)==WordSign(m,g)&&BladeSign(g,m)==WordSign(g,m);}}
int[] wordMasks=Enumerable.Range(0,16384).Where(m=>new[]{0,1,2,12,13,14}.Contains(Degree(m))).ToArray();foreach(int a in wordMasks)foreach(int b in wordMasks){counts["wordCases"]++;knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);}
var masks=new HashSet<int>{0,Full};for(int r=1;r<14;r++){int prefix=(1<<r)-1,rotated=0;for(int i=0;i<r;i++)rotated|=1<<((i+7)%14);masks.Add(prefix);masks.Add(rotated);}knownAnswerPassed&=masks.Count==28;
foreach(int mask in masks)foreach(Rational lambda in scales)
{var x=One(mask,0,Scalar.I);int degree=Degree(mask);counts["scaledHodge"]++;knownAnswerPassed&=Equal(LP.Evaluate(LP.Star(LP.Mono(x),degree),lambda),Scale(Star(x),new Scalar(MetricResources.Pow(lambda,14-2*degree),0)));counts["scaledPairings"]++;knownAnswerPassed&=SP.Evaluate(LP.Pair(LP.Mono(x),LP.Mono(x),degree,false),lambda)==Pair(x,x)*MetricResources.Pow(lambda,-2*degree)&&Pair(x,x)!=0;}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
FT gamma1=Caa.Gamma1,gamma2=Caa.Gamma2,ends=Add(One(1,1,1),One(2,2,1)),w=new();for(int j=4;j<14;j++)w=Add(w,One(1<<j,15|(1<<j),1));
FT[] curvatures=[gamma2,One(3,3,Fourier.Half),One(3,12,1),One(3,6,1)];
FT[] firstOracles=[Scale(gamma1,-26),Scale(ends,-1),new(),One(1,4,2)],secondOracles=[Scale(gamma1,182),gamma1,Scale(w,-2),new()];
int[] forwardPowers=[0,10,11,12,-2,-1,11,-1],reversePowers=[0,12,1,0,-1,13,11,1];
for(int index=0;index<4;index++)
{
 counts["forwardFixtures"]++;var f=curvatures[index];var stages=LP.ForwardStages(LP.Mono(f));var baseStages=Caa.ForwardStages(f);var naive=Caa.ForwardStages(f,true);var first=LP.Star(stages[2],13);var second=LP.Scale(LP.Star(LP.Star(stages[5],1),13),Fourier.Half*-1);var expectedK=Add(firstOracles[index],secondOracles[index]);
 for(int stage=0;stage<8;stage++){counts["forwardStagePolynomials"]++;chainsPassed&=LP.Equal(stages[stage],LP.Mono(baseStages[stage],forwardPowers[stage]))&&Equal(baseStages[stage],naive[stage]);}
 chainsPassed&=LP.Equal(first,LP.Mono(firstOracles[index],-1))&&LP.Equal(second,LP.Mono(secondOracles[index],-1))&&LP.Equal(stages[7],LP.Mono(expectedK,-1));
 var wrong=LP.ForwardStages(LP.Mono(f),true)[7];var wrongOracle=LP.Add(LP.Mono(firstOracles[index],-2),LP.Mono(secondOracles[index],-4));counts["wrongPhiDerivativeRejections"]++;chainsPassed&=LP.Equal(wrong,wrongOracle)&&!Equal(LP.Evaluate(LP.Derivative(wrong),1),Scale(expectedK,-1))&&Equal(LP.Evaluate(wrong,1),expectedK);
 foreach(Rational lambda in scales)
 {counts["forwardEvaluations"]++;chainsPassed&=Equal(LP.Evaluate(stages[7],lambda),Scale(expectedK,new Scalar(MetricResources.Pow(lambda,-1),0)));if(lambda!=1){counts["wrongPhiScaleRejections"]++;chainsPassed&=!Equal(LP.Evaluate(wrong,lambda),LP.Evaluate(stages[7],lambda));}if(firstOracles[index].Count!=0){counts["omittedFirstRejections"]++;chainsPassed&=!Equal(LP.Evaluate(second,lambda),LP.Evaluate(stages[7],lambda));}if(secondOracles[index].Count!=0){counts["omittedSecondRejections"]++;chainsPassed&=!Equal(LP.Evaluate(first,lambda),LP.Evaluate(stages[7],lambda));}}
 operatorRows.Add(new{kind="forward",index,first=LP.Terms(first),second=LP.Terms(second),wrongFrozenPhi=LP.Terms(wrong)});
}
FT[] adjointInputs=[gamma1,One(1,4,1)];FT expectedOff=new();for(int j=0;j<14;j++)if(j!=0&&j!=2)expectedOff=Add(expectedOff,One(1|(1<<j),4^(1<<j),2*Shuffle(1,1<<j)*WordSign(4,1<<j)));
for(int index=0;index<2;index++)
{
 counts["reverseFixtures"]++;FT y=adjointInputs[index];var stages=LP.ReverseStages(LP.Mono(y));var baseStages=BaseReverse(y);var kad=LP.Add(stages[2],stages[7]);FT oracle=index==0?Scale(gamma2,-24):expectedOff;FT firstOracle=index==0?Scale(gamma2,4):expectedOff,secondOracle=index==0?Scale(gamma2,-28):new();
 for(int stage=0;stage<8;stage++){counts["reverseStagePolynomials"]++;adjointsPassed&=LP.Equal(stages[stage],LP.Mono(baseStages[stage],reversePowers[stage]));}
 adjointsPassed&=LP.Equal(kad,LP.Mono(oracle,1))&&Equal(Caa.AdjointLiteral(y),oracle)&&Equal(Caa.AdjointSimplified(y),oracle)&&LP.Equal(stages[2],LP.Mono(firstOracle,1))&&LP.Equal(stages[7],LP.Mono(secondOracle,1));
 foreach(Rational lambda in scales)
 {counts["reverseEvaluations"]++;adjointsPassed&=Equal(LP.Evaluate(kad,lambda),Scale(oracle,new Scalar(lambda,0)));for(int f=0;f<4;f++){counts["adjointPairings"]++;Rational basisPair=index==0?(f==0?-2184:f==1?-12:0):f==3?-2:0;Rational left=SP.Evaluate(LP.Pair(LP.Mono(y),LP.K(LP.Mono(curvatures[f])),1,false),lambda),right=SP.Evaluate(LP.Pair(LP.Mono(curvatures[f]),kad,2,false),lambda);adjointsPassed&=left==right&&left==basisPair*MetricResources.Pow(lambda,-3);if(basisPair!=0){counts["nonzeroAdjointPairings"]++;adjointsPassed&=left!=0;}}}
 operatorRows.Add(new{kind="reverse",index,first=LP.Terms(stages[2]),second=LP.Terms(stages[7])});
}
foreach(var item in fx.GetProperty("cases").EnumerateArray())
{
 string id=item.GetProperty("id").GetString()!;int s=item.GetProperty("s").GetInt32(),gamma=item.GetProperty("gamma").GetInt32(),kappa=item.GetProperty("kappa").GetInt32();bool branch=item.GetProperty("branch").GetBoolean();counts["fieldCases"]++;
 Rational cubic=-1456*gamma*s*s*s,mass=-7*kappa*s*s;var actionPaths=new SP[2];var gradientPaths=new LP[2];var pathRows=new List<object>();bool stationary=312*gamma*s*s+kappa*s==0;
 for(int path=0;path<2;path++)
 {
  counts["fieldPaths"]++;var t=LP.Mono(Scale(gamma1,s),path);var q=LP.Product(t,t);var kq=LP.K(q);var y=LP.Kad(t);var n=LP.DQdag(t,y);LP[] anchors=[q,kq,y,n],anchorOracles=[LP.Mono(Scale(gamma2,2*s*s),2*path),LP.Mono(Scale(gamma1,312*s*s),2*path-1),LP.Mono(Scale(gamma2,-24*s),path+1),LP.Mono(Scale(gamma1,624*s*s),2*path-1)];
  for(int i=0;i<4;i++){counts["sourceAnchorPolynomials"]++;gradientPassed&=LP.Equal(anchors[i],anchorOracles[i])&&LP.RealConstant(anchors[i],i==0||i==2?2:1);}
  gradientPassed&=LP.D(t).Coefficients.Count==0&&LP.Ddag(y).Coefficients.Count==0;
  LP[] gradientLegs=[LP.Scale(kq,new Scalar(new Rational(gamma,3),0)),LP.Scale(n,new Scalar(new Rational(gamma,3),0)),LP.Scale(t,kappa)],gradientOracles=[LP.Mono(Scale(gamma1,104*gamma*s*s),2*path-1),LP.Mono(Scale(gamma1,208*gamma*s*s),2*path-1),LP.Mono(Scale(gamma1,kappa*s),path)];
  for(int i=0;i<3;i++){counts["sourceGradientLegPolynomials"]++;gradientPassed&=LP.Equal(gradientLegs[i],gradientOracles[i]);}
  var gradient=gradientLegs.Aggregate(LP.Zero,LP.Add);var gradientOracle=LP.Add(LP.Mono(Scale(gamma1,312*gamma*s*s),2*path-1),LP.Mono(Scale(gamma1,kappa*s),path));gradientPaths[path]=gradient;counts["gradientPolynomials"]++;gradientPassed&=LP.Equal(gradient,gradientOracle)&&LP.RealConstant(gradient,1);
  var fb=LP.Zero;var dt=LP.D(t);SP[] actionLegs=[LP.Top(LP.Product(t,LP.Star(LP.K(fb),1),'W',true)),SP.Scale(LP.Top(LP.Product(t,LP.Star(LP.K(dt),1),'W',true)),new Rational(1,2)),SP.Scale(LP.Top(LP.Product(t,LP.Star(kq,1),'W',true)),new Rational(gamma,3)),SP.Scale(LP.Top(LP.Product(t,LP.Star(t,1),'W',true)),new Rational(kappa,2))];
  SP[] actionOracles=[SP.Mono(0),SP.Mono(0),SP.Mono(cubic,11+3*path),SP.Mono(mass,12+2*path)];
  for(int i=0;i<4;i++){counts["originalActionLegPolynomials"]++;actionPassed&=SP.Equal(actionLegs[i],actionOracles[i]);}
  var action=actionLegs.Aggregate(SP.Mono(0),SP.Add);var oracle=SP.Add(actionOracles[2],actionOracles[3]);actionPaths[path]=action;var densityPair=SP.Add(SP.Scale(LP.Pair(t,kq,1,true),new Rational(gamma,3)),SP.Scale(LP.Pair(t,t,1,true),new Rational(kappa,2)));counts["actionPolynomials"]++;actionPassed&=SP.Equal(action,oracle)&&SP.Equal(action,densityPair);
  var derivative=SP.Derivative(action);var derivativeOracle=SP.Add(SP.Mono(cubic*(11+3*path),10+3*path),SP.Mono(mass*(12+2*path),11+2*path));counts["actionDerivativePolynomials"]++;actionPassed&=SP.Equal(derivative,derivativeOracle);
  var values=new List<object>();foreach(Rational lambda in scales)
  {if(path==0)counts["caseScaleContexts"]++;counts["actionValues"]++;counts["actionDerivativeValues"]++;counts["gradientValues"]++;Rational value=SP.Evaluate(action,lambda),dv=SP.Evaluate(derivative,lambda);FT gv=LP.Evaluate(gradient,lambda);Rational gCoefficient=312*gamma*s*s*MetricResources.Pow(lambda,2*path-1)+kappa*s*MetricResources.Pow(lambda,path);actionPassed&=value==cubic*MetricResources.Pow(lambda,11+3*path)+mass*MetricResources.Pow(lambda,12+2*path)&&dv==cubic*(11+3*path)*MetricResources.Pow(lambda,10+3*path)+mass*(12+2*path)*MetricResources.Pow(lambda,11+2*path);gradientPassed&=Equal(gv,Scale(gamma1,new Scalar(gCoefficient,0)));if(branch&&path==1)gradientPassed&=gv.Count==0;if(branch&&path==0&&lambda!=1){counts["fixedTransportBranchRejections"]++;gradientPassed&=gv.Count!=0;}values.Add(new{lambda=lambda.ToString(),action=value.ToString(),derivative=dv.ToString(),gradientCoefficient=gCoefficient.ToString()});}
  pathRows.Add(new{path,sourceAnchors=anchors.Select(LP.Terms).ToArray(),gradient=LP.Terms(gradient),action=SP.Terms(action),derivative=SP.Terms(derivative),values});
 }
 var fixedDerivative=SP.Evaluate(SP.Derivative(actionPaths[0]),1);var movingDerivative=SP.Evaluate(SP.Derivative(actionPaths[1]),1);var actualGradient=LP.Evaluate(gradientPaths[0],1);Rational movingContribution=Pair(actualGradient,Scale(gamma1,s));counts["chainRuleRows"]++;chainRulePassed&=movingDerivative-fixedDerivative==movingContribution&&movingContribution==-4368*gamma*s*s*s-14*kappa*s*s&&Equal(actualGradient,LP.Evaluate(gradientPaths[1],1));
 if(stationary){counts["fieldStationaryRows"]++;gradientPassed&=actualGradient.Count==0&&movingContribution==0;}else{counts["offBranchChainRuleRows"]++;chainRulePassed&=movingContribution!=0&&actualGradient.Count!=0;}
 if(branch){counts["nonzeroMetricBranchRows"]++;Rational derivative=10192*gamma*s*s*s;metricPassed&=stationary&&s!=0&&gamma!=0&&kappa==-312*gamma*s&&fixedDerivative==derivative&&movingDerivative==derivative&&derivative!=0&&cubic+mass==728*gamma*s*s*s;}else if(stationary)metricPassed&=fixedDerivative==0&&movingDerivative==0;
 var wrongPair=SP.Shift(actionPaths[0],2);if(cubic+mass!=0){counts["wrongPairDerivativeRejections"]++;Rational defect=SP.Evaluate(SP.Derivative(wrongPair),1)-fixedDerivative;actionPassed&=defect==2*(cubic+mass)&&defect!=0&&SP.Evaluate(wrongPair,1)==SP.Evaluate(actionPaths[0],1);foreach(Rational lambda in scales)if(lambda!=1){counts["wrongPairScaleRejections"]++;actionPassed&=SP.Evaluate(wrongPair,lambda)!=SP.Evaluate(actionPaths[0],lambda);}}
 fieldRows.Add(new{id,s,gamma,kappa,branch,fieldStationary=stationary,fixedDerivative=fixedDerivative.ToString(),movingDerivative=movingDerivative.ToString(),movingFieldContribution=movingContribution.ToString(),pathRows});
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resource=fx.GetProperty("resources");bool resourcesPassed=CoefficientProducts<=resource.GetProperty("maximumCoefficientProducts").GetInt64()&&LargestTensor<=resource.GetProperty("maximumSparseTensorTerms").GetInt32()&&MetricResources.MaximumMonomials<=resource.GetProperty("maximumLaurentMonomials").GetInt32()&&MetricResources.MaximumAbsoluteExponent<=resource.GetProperty("maximumAbsoluteExponent").GetInt32();
bool controlsPassed=knownAnswerPassed&&chainsPassed&&adjointsPassed&&actionPassed&&gradientPassed&&chainRulePassed&&metricPassed&&countsPassed&&resourcesPassed;string verdict=!knownAnswerPassed?precedence[1]:!chainsPassed?precedence[2]:!adjointsPassed?precedence[3]:!actionPassed?precedence[4]:!gradientPassed?precedence[5]:!chainRulePassed?precedence[6]:!metricPassed?precedence[7]:!countsPassed||!resourcesPassed?precedence[8]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,chainsPassed,adjointsPassed,actionPassed,gradientPassed,chainRulePassed,metricPassed,countsPassed,resourcesPassed,counts,coefficientProducts=CoefficientProducts,largestTensorDuringAssembly=LargestTensor,maximumLaurentMonomials=MetricResources.MaximumMonomials,maximumAbsoluteExponent=MetricResources.MaximumAbsoluteExponent,operatorRows,fieldRows,sourceMetricVariationAdmissible=false,sourceOperatorSelected=false,additionalActionTermsIncluded=false,couplingsVaried=false,globalVacuumRejected=false,physicalMetricEquationSelected=false,fullDifferentialSpectrumComputed=false});

static FT[] BaseReverse(FT y)
{var upper=Adjoint.StarAdjoint(y,13);var first=Adjoint.StarAdjoint(Adjoint.BracketAdjoint(Caa.Gamma1,upper,'C'),2);var outerOne=Adjoint.StarAdjoint(upper,1);var zero=Adjoint.BracketAdjoint(Caa.Gamma1,outerOne,'A');var top=Adjoint.StarAdjoint(zero,14);var twelve=Adjoint.BracketAdjoint(Caa.Gamma2,top,'A');var second=Scale(Adjoint.StarAdjoint(twelve,2),Fourier.Half*-1);return[y,upper,first,outerOne,zero,top,twelve,second];}

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=614,phaseId="phase614-conditional-metric-homothety-stationarity-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/conditional_metric_homothety_stationarity_audit.json",json);File.WriteAllText(Root+"/output/conditional_metric_homothety_stationarity_audit_summary.json",json);Console.WriteLine($"Phase614 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
