using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static Adjoint;
using static Caa;
using static Isotropic;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase613_canonical_isotropic_stationarity_potential_hessian_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P611="studies/phase611_untied_caa_response_joint_gauge_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase613-a60-canonical-isotropic-stationarity-potential-hessian-v1";
const string Success="canonical-isotropic-stationarity-controls-pass-potential-not-spectrum";
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
 "operator": "canonical untied CAA: firstC outerA innerA; Phi1=Gamma1 Phi2=Gamma2",
 "pairing": "signed exterior metric times -ReTr(XY)/128, real bilinear",
 "constantFields": true,
 "polynomialVariables": [
  "s",
  "gamma",
  "kappa"
 ],
 "polynomialOrder": 4,
 "gammas": [
  0,
  1,
  2
 ],
 "kappas": [
  -1,
  0,
  1
 ],
 "fieldValues": [
  -1,
  0,
  1
 ],
 "isotropic": {
  "Q": "2s^2Gamma2",
  "KQ": "312s^2Gamma1",
  "adjoint": "-24sGamma2",
  "dqAdjoint": "624s^2Gamma1",
  "action": "-1456gamma s^3-7kappa s^2",
  "gradient": "(312gamma s^2+kappa s)Gamma1"
 },
 "carrier": {
  "basis": "E_ab=theta_a gamma_b,a,b0..13",
  "dimension": 196,
  "gram": "-sigma_a sigma_b delta_ac delta_bd",
  "transpose": "Vg_ab=sigma_a sigma_b V_ba",
  "legs": [
   "48(trV I-V)",
   "48(trV I-Vg)",
   "48(trV I-V)"
  ],
  "potential": "kappa V+16gamma s(3trV I-2V-Vg)",
  "sectorDimensions": [
   1,
   104,
   91
  ],
  "sectorCoefficients": [
   "kappa+624gamma s",
   "kappa-48gamma s",
   "kappa-16gamma s"
  ],
  "fullDifferentialClosureClaimed": false
 },
 "adjoint": {
  "first": "2theta_a wedge(gamma_b wedgeCl Gamma1)",
  "second": "-2delta_ab Gamma2",
  "outer": "2i delta_ab",
  "probeBlade": "formPair XORbit(a) XORbit(b)",
  "phase": "1 if H-adjoint-sign negative else i",
  "probesPerInput": 91
 },
 "branches": {
  "gammaNonzero": "s0 and s=-kappa/(312gamma); coincident ifkappa0",
  "gammaZeroKappaNonzero": "s0 only",
  "bothZero": "all real s",
  "nonzeroBranchSectorCoefficients": [
   "-kappa",
   "15kappa/13",
   "41kappa/39"
  ],
  "branchAction": "728gamma s^3",
  "shortcutAtBranch": "-208gamma s^2Gamma1"
 },
 "decoy": {
  "variables": [
   "x",
   "y",
   "t"
  ],
  "order": 3,
  "gammas": [
   1,
   2
  ],
  "T": "x theta0 gamma2+y theta1 gamma3",
  "variation": "theta4 Gamma01234",
  "Q": "2xy theta01 Gamma23, unchanged by tV",
  "KQ": "-4xy sum_j4..13 theta_j Gamma0123j",
  "inner": "4i xy Gamma0123 top",
  "outer": "8xy W",
  "adjointComposite": "4x^2(E00+E22-I)+4y^2(E11+E33-I)",
  "action": "4gamma xyt/3",
  "vectorClosureRejected": true
 },
 "knownWordMenu": "all16384blades times14singletons andOmega onbothsides plus square,31each",
 "exactTolerance": 0,
 "coreFileCount": 726,
 "expectedCounts": {
  "rationalControls": 4,
  "polynomialKnownAnswers": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "isotropicTensorRows": 5,
  "isotropicForwardStages": 8,
  "isotropicAdjointLegs": 2,
  "isotropicScalarCoefficients": 35,
  "isotropicGradientCoefficients": 35,
  "isotropicVariationCoefficients": 35,
  "adjointInputs": 196,
  "adjointLegChecks": 392,
  "adjointSimplifiedChecks": 196,
  "outerAdjointChecks": 196,
  "outerAdjointNonzero": 14,
  "firstAdjointPositions": 2366,
  "secondAdjointPositions": 1274,
  "fullAdjointPositions": 3276,
  "forwardAdjointProbes": 17836,
  "nonzeroAdjointProbes": 3276,
  "adjointReconstructions": 196,
  "potentialColumns": 196,
  "potentialLegs": 588,
  "potentialStageChecks": 1568,
  "carrierGramEntries": 38416,
  "originalActionBilinears": 38416,
  "weightedReciprocityEntries": 38416,
  "massIdentityEntries": 38416,
  "potentialMatrixEntries": 38416,
  "sectorVectors": 196,
  "traceSector": 1,
  "symmetricTracelessSector": 104,
  "skewSector": 91,
  "decompositionRows": 196,
  "parameterRows": 27,
  "couplingRows": 9,
  "originRows": 9,
  "nonzeroBranchRows": 4,
  "branchSectorChecks": 12,
  "shortcutRejected": 4,
  "decoyForwardStages": 8,
  "decoyQCoefficients": 20,
  "decoyKCoefficients": 20,
  "decoyAdjointCompositeCoefficients": 20,
  "decoyFullGradientCoefficients": 20,
  "decoyQUnchangedCoefficients": 20,
  "decoyActionRows": 2,
  "decoyActionCoefficients": 40,
  "decoyDerivativeCoefficients": 120,
  "decoyPairings": 1,
  "decoyGradeFiveTerms": 10
 },
 "resources": {
  "estimatedCpuSeconds": 120,
  "estimatedPeakBytes": 268435456,
  "maximumTrackedCoefficientProducts": 100000000,
  "maximumFourierTerms": 8192,
  "maximumFrequency": 0,
  "maximumPolynomialDegree": 4,
  "maximumPolynomialCoefficients": 35,
  "carrierDimension": 196
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormSelected": false,
  "sourceMetricSelected": false,
  "fullDifferentialClosureClaimed": false,
  "physicalSpectrumClaimed": false,
  "nonlinearVectorClosureClaimed": false,
  "physicalVacuumSelected": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","isotropic-gradient-control-failed","complete-adjoint-control-failed","potential-hessian-control-failed","stationary-branch-control-failed","nonlinear-grade-five-control-failed","resource-census-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase613CanonicalIsotropicStationarityPotentialHessianAudit.csproj",["study"]=Root+"/STUDY.md",["potential-helper"]=Root+"/IsotropicPotential.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",["phase600-program"]=P600+"/Program.cs",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",
 ["caa-helper"]=P611+"/CaaOperator.cs",["polynomial-helper"]=P611+"/PolynomialJets.cs",["phase611-program"]=P611+"/Program.cs",["phase611-study"]=P611+"/STUDY.md",["phase611-contract"]=P611+"/preregistration/contract_v1.json",["phase611-summary"]=P611+"/output/untied_caa_response_joint_gauge_audit_summary.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==613&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase611","untied-caa-response-controls-pass-source-choice-open")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(9.1)","(9.4)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}


var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string k)=>counts[k]++;
bool knownAnswerPassed=true,isotropicPassed=true,adjointPassed=true,potentialPassed=true,branchPassed=true,decoyPassed=true,frequencyPassed=true;
bool CheckField(FT t,int degree){frequencyPassed&=t.Keys.All(k=>k.K0==0&&k.K1==0);return Typed(t,degree)&&HAnti(t);}
foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("rationalControls");knownAnswerPassed&=pass;}
for(int a=0;a<16384;a++)
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}
 Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);
}
var polyX=FP.Variable(2,0,One(0,1,1));var square=FP.Product(polyX,polyX);var scalarPoly=new SP(2);scalarPoly.Put(new(2,0,0),3);
foreach(bool pass in new[]{FP.Product(square,polyX).Zero,Equal(square.Get(new(2,0,0)),One(0,0,1)),scalarPoly.Derivative(0).Derivative(0).Get(new(0,0,0))==6,FP.Pair(polyX,polyX).Get(new(2,0,0))==-1}){Inc("polynomialKnownAnswers");knownAnswerPassed&=pass;}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
// Full isotropic source tensors first; no scalar ansatz enters these products.
var identity=Gamma1;var gamma2=Gamma2;var isoQ=Product(identity,identity);var isoStages=ForwardStages(isoQ);var isoNaive=ForwardStages(isoQ,true);var isoKadLegs=AdjointLegs(identity);var isoKad=AdjointLiteral(identity);var isoN=DQAdjoint(identity,isoKad);var isoG=Scale(Add(isoStages[7],isoN),new Scalar(new Rational(1,3),0));
foreach(var (actual,oracle,d) in new[]{(isoQ,Scale(gamma2,2),2),(isoStages[7],Scale(identity,312),1),(isoKad,Scale(gamma2,-24),2),(isoN,Scale(identity,624),1),(isoG,Scale(identity,312),1)}){Inc("isotropicTensorRows");isotropicPassed&=Equal(actual,oracle)&&CheckField(actual,d);}
for(int i=0;i<8;i++){Inc("isotropicForwardStages");isotropicPassed&=Equal(isoStages[i],isoNaive[i]);}
foreach(var (actual,oracle) in new[]{(isoKadLegs.First,Scale(gamma2,4)),(isoKadLegs.Second,Scale(gamma2,-28))}){Inc("isotropicAdjointLegs");isotropicPassed&=Equal(actual,oracle)&&CheckField(actual,2);}
var sField=FP.Variable(4,0,identity);var qPoly=FP.Product(sField,sField);var kPoly=FP.Chain(qPoly,FP.Constant(4,identity),FP.Constant(4,gamma2));var kadPoly=Map(sField,AdjointLiteral);var nPoly=AdjProduct(sField,kadPoly);
var gPoly=FP.Add(Shift(FP.Scale(FP.Add(kPoly,nPoly),new Scalar(new Rational(1,3),0)),new(0,1,0)),Shift(sField,new(0,0,1)));
var actionPoly=Sum(Shift(FP.Pair(sField,kPoly).Scale(new Rational(1,3)),new(0,1,0)),Shift(FP.Pair(sField,sField).Scale(new Rational(1,2)),new(0,0,1)));
var actionOracle=new SP(4);actionOracle.Put(new(3,1,0),-1456);actionOracle.Put(new(2,0,1),-7);var gOracle=new FP(4);gOracle.Put(new(2,1,0),Scale(identity,312));gOracle.Put(new(1,0,1),identity);var variation=FP.Pair(FP.Constant(4,identity),gPoly);
foreach(var m in Monomial.Menu(4)){Inc("isotropicScalarCoefficients");Inc("isotropicGradientCoefficients");Inc("isotropicVariationCoefficients");isotropicPassed&=actionPoly.Get(m)==actionOracle.Get(m)&&Equal(gPoly.Get(m),gOracle.Get(m))&&CheckField(gPoly.Get(m),1)&&variation.Get(m)==actionPoly.Derivative(0).Get(m);}
// All196 complete literal adjoints and constant-field potential columns.
var columns=new FT[196];var forwardColumns=new FT[196];var columnRows=new List<object>();var basis=Enumerable.Range(0,196).Select(i=>Unit(i/14,i%14)).ToArray();
for(int a=0;a<14;a++)for(int b=0;b<14;b++)
{
 int index=14*a+b;var v=basis[index];Inc("adjointInputs");var legs=AdjointLegs(v);var kad=Add(legs.First,legs.Second);var oracle=OracleAdjoint(a,b);
 foreach(var (value,predicted) in new[]{(legs.First,oracle.First),(legs.Second,oracle.Second)}){Inc("adjointLegChecks");adjointPassed&=Equal(value,predicted)&&CheckField(value,2);}
 Inc("adjointSimplifiedChecks");adjointPassed&=Equal(kad,AdjointSimplified(v))&&CheckField(kad,2);
 Inc("outerAdjointChecks");var outer=OuterAdjoint(v);adjointPassed&=Equal(outer,a==b?One(0,0,Scalar.I*2):new FT());if(outer.Count!=0)Inc("outerAdjointNonzero");
 counts["firstAdjointPositions"]+=legs.First.Count;counts["secondAdjointPositions"]+=legs.Second.Count;counts["fullAdjointPositions"]+=kad.Count;
 var reconstructed=Reconstruct(a,b,kad,(pass,nonzero)=>{Inc("forwardAdjointProbes");if(nonzero)Inc("nonzeroAdjointProbes");adjointPassed&=pass;});Inc("adjointReconstructions");adjointPassed&=Equal(reconstructed,kad);
 var actualLegs=ActualLegs(v,isoKad);var expectedLegs=OracleLegs(v);for(int j=0;j<3;j++){Inc("potentialLegs");potentialPassed&=Equal(actualLegs[j],expectedLegs[j])&&CheckField(actualLegs[j],1);}
 var dq=Product(identity,v,'C');var stage=ForwardStages(dq);var naive=ForwardStages(dq,true);for(int j=0;j<8;j++){Inc("potentialStageChecks");potentialPassed&=Equal(stage[j],naive[j]);}
 columns[index]=Potential(actualLegs);forwardColumns[index]=actualLegs[0];Inc("potentialColumns");potentialPassed&=Equal(columns[index],Potential(expectedLegs))&&CheckField(columns[index],1);
 columnRows.Add(new{a,b,adjointFirst=Terms(legs.First),adjointSecond=Terms(legs.Second),adjointFull=Terms(kad),potentialLegs=actualLegs.Select(Terms).ToArray(),potential=Terms(columns[index])});
}
var lowered=new Rational[196,196];var scalarBilinears=new Rational[196,196];
for(int i=0;i<196;i++)for(int j=0;j<196;j++)
{
 var u=basis[i];var v=basis[j];var gram=Pair(u,v);Rational expectedGram=i==j?-Sigma(i/14)*Sigma(i%14):0;Inc("carrierGramEntries");potentialPassed&=gram==expectedGram;
 var forwardQ=Product(u,v,'C');var forwardK=Forward(forwardQ);var original=(Pair(u,forwardColumns[j])+Pair(v,forwardColumns[i])+Pair(identity,forwardK))*new Rational(1,3);
 Inc("originalActionBilinears");Inc("weightedReciprocityEntries");Inc("massIdentityEntries");Inc("potentialMatrixEntries");var value=Pair(u,columns[j]);lowered[i,j]=value;scalarBilinears[i,j]=original;
 potentialPassed&=original==value&&value==Pair(v,columns[i])&&Pair(u,v)==expectedGram&&value==Pair(u,Potential(OracleLegs(v)))&&CheckField(forwardK,1);
}
var sectorRows=new List<object>();
foreach(var (sector,v,coefficient) in Sectors())
{
 Inc("sectorVectors");Inc(sector=="trace"?"traceSector":sector=="metric-skew"?"skewSector":"symmetricTracelessSector");var actual=ApplyColumns(v,columns);potentialPassed&=Equal(actual,Scale(v,coefficient))&&CheckField(actual,1);
 sectorRows.Add(new{sector,coefficient,vector=Terms(v),image=Terms(actual)});
}
foreach(var v in basis)
{
 Inc("decompositionRows");var trace=Scale(identity,new Scalar(Trace(v)*new Rational(1,14),0));var symmetric=Add(Scale(Add(v,Transpose(v)),Fourier.Half),Scale(trace,-1));var skew=Scale(Add(v,Scale(Transpose(v),-1)),Fourier.Half);
 potentialPassed&=Equal(Add(Add(trace,symmetric),skew),v)&&Trace(symmetric)==0&&Equal(Transpose(symmetric),symmetric)&&Equal(Transpose(skew),Scale(skew,-1));
}
var parameterRows=new List<object>();var branchRows=new List<object>();
foreach(int gamma in new[]{0,1,2})foreach(int kappa in new[]{-1,0,1})
{
 Inc("couplingRows");Inc("originRows");branchPassed&=Evaluate(gPoly,0,gamma,kappa).Count==0&&Evaluate(actionPoly,0,gamma,kappa)==0;
 string classification=gamma==0?(kappa==0?"all-real-s":"origin-only-linear"):(kappa==0?"origin-double-root":"origin-and-nonzero-root");
 foreach(int s in new[]{-1,0,1}){Inc("parameterRows");var actual=Evaluate(gPoly,s,gamma,kappa);var action=Evaluate(actionPoly,s,gamma,kappa);branchPassed&=Equal(actual,Scale(identity,312*gamma*s*s+kappa*s))&&action==-1456*gamma*s*s*s-7*kappa*s*s;parameterRows.Add(new{s,gamma,kappa,classification,gradient=Terms(actual),action=action.ToString(),stationary=actual.Count==0});}
 if(gamma!=0&&kappa!=0)
 {
  Inc("nonzeroBranchRows");Rational s=new(-kappa,312*gamma);var actual=Evaluate(gPoly,s,gamma,kappa);var action=Evaluate(actionPoly,s,gamma,kappa);branchPassed&=actual.Count==0&&action==728*gamma*Pow(s,3);
  Rational[] predicted=[-kappa,new Rational(15*kappa,13),new Rational(41*kappa,39)];int[] unit=[624,-48,-16];for(int i=0;i<3;i++){Inc("branchSectorChecks");branchPassed&=kappa+gamma*s*unit[i]==predicted[i];}
  var shortcut=Add(Scale(isoStages[7],new Scalar(gamma*s*s*new Rational(1,3),0)),Scale(identity,new Scalar(kappa*s,0)));Inc("shortcutRejected");branchPassed&=Equal(shortcut,Scale(identity,new Scalar(-208*gamma*s*s,0)))&&shortcut.Count!=0;
  branchRows.Add(new{gamma,kappa,s=s.ToString(),action=action.ToString(),sectorCoefficients=predicted.Select(x=>x.ToString()).ToArray(),fullGradient=Terms(actual),omittedAdjointShortcut=Terms(shortcut)});
 }
}
// Independent degree-three nonlinear closure falsifier, retaining all grades.
var decoyX=Unit(0,2);var decoyY=Unit(1,3);var allowed=One(1<<4,31,1);var w=new FT();for(int j=4;j<14;j++)w=Add(w,One(1<<j,15|(1<<j),1));
var td=FP.Add(FP.Variable(3,0,decoyX),FP.Variable(3,1,decoyY));var tdExtended=FP.Add(td,FP.Variable(3,2,allowed));var qd=FP.Product(td,td);var kd=FP.Chain(qd,FP.Constant(3,identity),FP.Constant(3,gamma2));var nd=AdjProduct(td,Map(td,AdjointLiteral));var gd=FP.Scale(FP.Add(kd,nd),new Scalar(new Rational(1,3),0));var qext=FP.Product(tdExtended,tdExtended);
var qdOracle=new FP(3);qdOracle.Put(new(1,1,0),One(3,12,2));var kdOracle=new FP(3);kdOracle.Put(new(1,1,0),Scale(w,-4));var ndOracle=new FP(3);ndOracle.Put(new(2,0,0),Scale(Add(Add(Unit(0,0),Unit(2,2)),Scale(identity,-1)),4));ndOracle.Put(new(0,2,0),Scale(Add(Add(Unit(1,1),Unit(3,3)),Scale(identity,-1)),4));var gdOracle=FP.Scale(FP.Add(kdOracle,ndOracle),new Scalar(new Rational(1,3),0));
foreach(var m in Monomial.Menu(3)){Inc("decoyQCoefficients");Inc("decoyKCoefficients");Inc("decoyAdjointCompositeCoefficients");Inc("decoyFullGradientCoefficients");Inc("decoyQUnchangedCoefficients");decoyPassed&=Equal(qd.Get(m),qdOracle.Get(m))&&Equal(kd.Get(m),kdOracle.Get(m))&&Equal(nd.Get(m),ndOracle.Get(m))&&Equal(gd.Get(m),gdOracle.Get(m))&&Equal(qext.Get(m),qd.Get(m))&&CheckField(gd.Get(m),1)&&nd.Get(m).Keys.All(k=>Degree(k.Blade)==1);}
var dStages=ForwardStages(One(3,12,2));FT[] stageOracle=[One(3,12,2),Star(One(3,12,2)),new FT(),One(Full,15,Scalar.I*4),One(0,15,Scalar.I*-4),Scale(w,8),Star(Scale(w,-4)),Scale(w,-4)];
for(int i=0;i<8;i++){Inc("decoyForwardStages");decoyPassed&=Equal(dStages[i],stageOracle[i]);}
foreach(var q in kd.Get(new(1,1,0))){Inc("decoyGradeFiveTerms");decoyPassed&=Degree(q.Key.Blade)==5&&q.Value==new Scalar(-4);}
Inc("decoyPairings");decoyPassed&=Pair(allowed,kd.Get(new(1,1,0)))==4&&Pair(allowed,allowed)==-1&&Product(decoyX,allowed,'C').Count==0&&Product(decoyY,allowed,'C').Count==0;
var actionDecoy=FP.Pair(tdExtended,FP.Chain(qext,FP.Constant(3,identity),FP.Constant(3,gamma2)));var decoyRows=new List<object>();
foreach(int gamma in new[]{1,2})
{
 Inc("decoyActionRows");var actual=actionDecoy.Scale(new Rational(gamma,3));var oracle=new SP(3);oracle.Put(new(1,1,1),new Rational(4*gamma,3));foreach(var m in Monomial.Menu(3)){Inc("decoyActionCoefficients");decoyPassed&=actual.Get(m)==oracle.Get(m);for(int i=0;i<3;i++){Inc("decoyDerivativeCoefficients");decoyPassed&=actual.Derivative(i).Get(m)==oracle.Derivative(i).Get(m);}}
 decoyRows.Add(new{gamma,action=actual.Terms(),gradient=Enumerable.Range(0,3).Select(i=>actual.Derivative(i).Terms()).ToArray(),gradeFiveVariationDerivative=actual.Derivative(2).Get(new(1,1,0)).ToString()});
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");bool resourcesPassed=frequencyPassed&&CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumFourierTerms").GetInt32();
bool controlsPassed=knownAnswerPassed&&isotropicPassed&&adjointPassed&&potentialPassed&&branchPassed&&decoyPassed&&countsPassed&&resourcesPassed;
string verdict=!knownAnswerPassed?precedence[1]:!isotropicPassed?precedence[2]:!adjointPassed?precedence[3]:!potentialPassed?precedence[4]:!branchPassed?precedence[5]:!decoyPassed?precedence[6]:!countsPassed||!resourcesPassed?precedence[7]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,isotropicPassed,adjointPassed,potentialPassed,branchPassed,decoyPassed,countsPassed,resourcesPassed,frequencyPassed,counts,trackedCoefficientProducts=CoefficientProducts,largestTensor=LargestTensor,isotropic=new{Q=qPoly.Terms(),KQ=kPoly.Terms(),adjoint=kadPoly.Terms(),dqAdjoint=nPoly.Terms(),gradient=gPoly.Terms(),action=actionPoly.Terms()},columnRows,lowered=MatrixText(lowered),originalActionBilinears=MatrixText(scalarBilinears),sectorRows,parameterRows,branchRows,decoy=new{Q=qd.Terms(),KQ=kd.Terms(),adjointComposite=nd.Terms(),fullCubicGradient=gd.Terms(),extendedQ=qext.Terms(),stages=dStages.Select(Terms).ToArray(),actionRows=decoyRows},sourceOperatorSelected=false,sourceNormSelected=false,sourceMetricSelected=false,fullDifferentialClosureClaimed=false,physicalSpectrumClaimed=false,nonlinearVectorClosureClaimed=false,physicalVacuumSelected=false});

static string[][] MatrixText(Rational[,] m)=>Enumerable.Range(0,m.GetLength(0)).Select(i=>Enumerable.Range(0,m.GetLength(1)).Select(j=>m[i,j].ToString()).ToArray()).ToArray();
void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=613,phaseId="phase613-canonical-isotropic-stationarity-potential-hessian-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/canonical_isotropic_stationarity_potential_hessian_audit.json",json);File.WriteAllText(Root+"/output/canonical_isotropic_stationarity_potential_hessian_audit_summary.json",json);Console.WriteLine($"Phase613 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
