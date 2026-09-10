using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P611="studies/phase611_untied_caa_response_joint_gauge_audit_001";
const string P617="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase619-a62-invariant-bivector-nonlinear-feedback-v1";
const string Success="invariant-bivector-controls-pass-nonlinear-grade-five-required";
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
 "points": [
  0,
  1
 ],
 "operator": "canonical untied CAA: firstC outerA innerA; Phi1=Gamma1 Phi2=Gamma2",
 "domain": "full real u(64,64), including central iI and both Clifford parities; no domain projection",
 "input": "passed617 complete36-term J=H(PT Gamma1), both transportedpoints",
 "inputFormAxes": [
  0,
  7,
  8,
  9
 ],
 "inputCliffordGrade": 2,
 "inputCoefficientCount": 36,
 "inputPair": "9",
 "cyclicTrace": "sum_b sigma_b gamma_b wedgeCl J_b=0; outer-A transpose is2i times cyclic trace",
 "adjointOracle": "KdagJ_ab=L_a e_b-L_b e_a; L_a gamma=[J_a,gamma]",
 "quadraticOracle": "Q_ab spin action=[L_a,L_b]; q_ab,cd=sigma_d [L_a,L_b]^c_d/2",
 "contractionOracle": "first_b=-sum_a sigma_a[gamma_a,F_ab]; second=2D Gamma1-2 sum_k theta^k gamma_k wedgeCl W4; D=sum_ab F_ab,ab; W4=sum_disjoint sigma_a sigma_b Shuffle(ab,cd)F_ab,cd Gamma_abcd",
 "dqOracle": "(DQ_JdagY)_b=-sum_a sigma_a L_a Y_ab, vectorY",
 "witness": {
  "form": 8,
  "blade": 157,
  "word": "theta3 Gamma02347",
  "pairNorm": "1",
  "quadraticForm": 129,
  "quadraticBlade": 20,
  "quadraticCoefficient": "1/2",
  "innerBlade": 149,
  "innerImaginary": "-1",
  "zeroImaginary": "1",
  "outerCoefficient": "-2",
  "KQCoefficient": "1",
  "adjointCompositeCoefficient": "0",
  "nonlinearCoefficient": "1/3"
 },
 "originalPotential": "kappa Pair(T,T)/2 + gamma Pair(T,K(T wedge T))/3; excludes HJ and complete covariant action",
 "fieldPolynomial": "T=uJ+vW",
 "polynomialOrder": 3,
 "polynomialVariables": [
  "u",
  "v"
 ],
 "actionMonomials": 10,
 "derivativeMonomials": 6,
 "gammas": [
  1,
  2
 ],
 "kappas": [
  -1,
  0,
  1
 ],
 "expectedPotential": "kappa*(9u^2+v^2)/2+gamma*u^2*v/3",
 "adjointProbeMenu": "all91 exteriorpairs times14 Cliffordvectors, fullsupportreconstruction ofKdagJ",
 "exactTolerance": 0,
 "coreFileCount": 726,
 "expectedCounts": {
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "domainBladeControls": 16384,
  "centralDomainControls": 1,
  "contexts": 2,
  "inputRows": 2,
  "inputActionRows": 392,
  "cyclicRows": 2,
  "adjointRows": 2,
  "adjointLegChecks": 4,
  "adjointSimplifiedRows": 2,
  "adjointMatrixEntries": 2548,
  "adjointSupportProbes": 2548,
  "adjointReconstructions": 2,
  "quadraticMatrixEntries": 16562,
  "quadraticNaiveRows": 6,
  "chainStageComparisons": 16,
  "chainTypeChecks": 16,
  "chainRealityChecks": 16,
  "contractionOracleChecks": 6,
  "nonlinearOracleRows": 2,
  "anchorContributionRows": 4,
  "stageAnchorRows": 14,
  "witnessPairingRows": 8,
  "fieldNormRows": 4,
  "retainedGradeRows": 12,
  "fieldAdjointChecks": 4,
  "quadraticCoefficientRows": 6,
  "potentialGradientTypeRows": 6,
  "actionRows": 12,
  "actionCoefficients": 120,
  "actionDerivativeCoefficients": 144,
  "projectedGradientDecoys": 12,
  "pointTransportRows": 7
 },
 "resources": {
  "estimatedCpuSeconds": 60,
  "maximumEstimatedCpuSeconds": 180,
  "estimatedPeakBytes": 134217728,
  "maximumEstimatedPeakBytes": 536870912,
  "maximumTrackedCoefficientProducts": 100000000,
  "maximumTrackedMatrixProducts": 10000000,
  "maximumTensorTerms": 65536,
  "maximumMatrixDimension": 14,
  "largestScalarArrayEntries": 2744,
  "maximumPolynomialOrder": 3,
  "maximumActionCoefficients": 10,
  "maximumFrequency": 0
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormSelected": false,
  "sourceMetricSelected": false,
  "domainRestrictedToVectorBivector": false,
  "HJComputed": false,
  "fullCovariantActionComputed": false,
  "smallCarrierClosureClaimed": false,
  "physicalVacuumSelected": false,
  "physicalSpectrumClaimed": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","full-bivector-input-control-failed","full-adjoint-cyclic-control-failed","literal-quadratic-contraction-control-failed","nonlinear-grade-five-control-failed","original-potential-variation-control-failed","resource-census-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase619InvariantBivectorNonlinearFeedbackAudit.csproj",["study"]=Root+"/STUDY.md",["feedback-helper"]=Root+"/BivectorFeedback.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",["phase600-program"]=P600+"/Program.cs",["phase600-study"]=P600+"/STUDY.md",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",
 ["caa-helper"]=P611+"/CaaOperator.cs",["phase611-program"]=P611+"/Program.cs",["phase611-project"]=P611+"/Phase611UntiedCaaResponseJointGaugeAudit.csproj",["phase611-study"]=P611+"/STUDY.md",["phase611-contract"]=P611+"/preregistration/contract_v1.json",["phase611-summary"]=P611+"/output/untied_caa_response_joint_gauge_audit_summary.json",
 ["phase617-helper"]=P617+"/ProjectorGradient.cs",["phase617-program"]=P617+"/Program.cs",["phase617-project"]=P617+"/Phase617NonparallelProjectorTwoWeightGradientAudit.csproj",["phase617-study"]=P617+"/STUDY.md",["phase617-contract"]=P617+"/preregistration/contract_v1.json",["phase617-summary"]=P617+"/output/nonparallel_projector_two_weight_gradient_audit_summary.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==619&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase611","untied-caa-response-controls-pass-source-choice-open"),("phase617","nonparallel-projector-controls-pass-two-weight-ansatz-nonstationary")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.27)","(3.34)","(9.1)","(9.4)","(12.26)","(12.27)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string key)=>counts[key]++;
bool knownAnswerPassed=true,inputPassed=true,adjointPassed=true,quadraticPassed=true,nonlinearPassed=true,actionPassed=true,frequencyPassed=true;
foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("arithmeticControls");knownAnswerPassed&=pass;}
for(int a=0;a<16384;a++)
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}
 Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);
 Scalar phase=AdjointSign(a)==-1?1:Scalar.I;Inc("domainBladeControls");knownAnswerPassed&=HAnti(One(0,a,phase));
}
Inc("centralDomainControls");knownAnswerPassed&=HAnti(One(0,0,Scalar.I))&&One(0,0,Scalar.I).Count==1;
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
using var upstream=JsonDocument.Parse(File.ReadAllBytes(paths["phase617-summary"]));
var upstreamRows=upstream.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().ToArray();var rows=new List<object>();FT[]? previous=null;
for(int point=0;point<2;point++)
{
 Inc("contexts");var input=upstreamRows.Single(x=>x.GetProperty("point").GetInt32()==point);var j=Feedback.Read(input.GetProperty("kinetic"));var w=One(8,157,1);
 Inc("inputRows");inputPassed&=j.Count==36&&Typed(j,1)&&HAnti(j)&&Feedback.Grades(j,2)&&j.Keys.All(k=>new[]{1,128,256,512}.Contains(k.Form))&&Equal(j,Feedback.Read(input.GetProperty("kineticOracle")))&&Equal(j,Feedback.Read(input.GetProperty("kineticStages")[7]))&&Equal(j,Feedback.Read(input.GetProperty("adjointDivergence")));
 var l=Feedback.Actions(j);
 for(int a=0;a<14;a++)for(int b=0;b<14;b++){var ja=Feedback.Slice(j,a);var gamma=One(0,1<<b,1);var image=Product(ja,gamma,'C');Inc("inputActionRows");inputPassed&=Equal(image,Feedback.VectorAction(l,a,b))&&Equal(image,NaiveProduct(ja,gamma,'C'));}
 var cyclic=Feedback.Cyclic(j);var outerAdjoint=Caa.OuterAdjoint(j);Inc("cyclicRows");adjointPassed&=cyclic.Count==0&&outerAdjoint.Count==0&&Equal(outerAdjoint,Scale(cyclic,Scalar.I*2));
 var legs=Caa.AdjointLegs(j);var kad=Add(legs.First,legs.Second);var kadOracle=Feedback.Adjoint(l);
 Inc("adjointRows");Inc("adjointSimplifiedRows");adjointPassed&=Equal(kad,kadOracle)&&Equal(kad,Caa.AdjointSimplified(j))&&Typed(kad,2)&&HAnti(kad);
 Inc("adjointLegChecks");Inc("adjointLegChecks");adjointPassed&=Equal(legs.First,kadOracle)&&legs.Second.Count==0;
 var rebuilt=new FT();var probes=new List<object>();
 for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)for(int c=0;c<14;c++)
 {
  int form=(1<<a)|(1<<b),blade=1<<c;var key=(form,blade,0,0);Inc("adjointMatrixEntries");adjointPassed&=kad.GetValueOrDefault(key)==kadOracle.GetValueOrDefault(key);
  var f=One(form,blade,1);var norm=Pair(f,f);var pairing=Pair(Caa.Forward(f),j);Inc("adjointSupportProbes");adjointPassed&=(norm==1||norm==-1)&&pairing==Pair(f,kad);var coefficient=pairing*norm;rebuilt=Add(rebuilt,Scale(f,new Scalar(coefficient,0)));probes.Add(new{a,b,c,norm=norm.ToString(),pairing=pairing.ToString(),coefficient=coefficient.ToString()});
 }
 Inc("adjointReconstructions");adjointPassed&=Equal(rebuilt,kad);
 var q=Product(j,j);var qOracle=Feedback.Quadratic(l);
 for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)for(int c=0;c<14;c++)for(int d=c+1;d<14;d++){var key=((1<<a)|(1<<b),(1<<c)|(1<<d),0,0);Inc("quadraticMatrixEntries");quadraticPassed&=q.GetValueOrDefault(key)==qOracle.GetValueOrDefault(key);}
 quadraticPassed&=Equal(q,qOracle);
 var stages=Caa.ForwardStages(q);var naiveStages=Caa.ForwardStages(q,true);int[] degrees=[2,12,13,14,0,1,13,1];
 for(int k=0;k<8;k++){Inc("chainStageComparisons");Inc("chainTypeChecks");Inc("chainRealityChecks");quadraticPassed&=Equal(stages[k],naiveStages[k])&&Typed(stages[k],degrees[k])&&HAnti(stages[k]);}
 var contraction=Feedback.Contraction(qOracle);var kq=stages[7];
 foreach(bool pass in new[]{Equal(Star(stages[2]),contraction.First),Equal(Scale(stages[5],Fourier.Half*-1),contraction.Second),Equal(kq,contraction.Full)}){Inc("contractionOracleChecks");quadraticPassed&=pass;}
 var dq=Adjoint.DQAdjoint(j,kad);var dqOracle=Feedback.DqAdjoint(l,kadOracle);var nonlinear=Scale(Add(kq,dq),new Scalar(new Rational(1,3),0));var nonlinearOracle=Scale(Add(contraction.Full,dqOracle),new Scalar(new Rational(1,3),0));
 Inc("nonlinearOracleRows");nonlinearPassed&=Equal(dq,dqOracle)&&Equal(nonlinear,nonlinearOracle);
 foreach(bool pass in new[]{Feedback.Grades(j,2),Feedback.Grades(q,2),Feedback.Grades(kad,1),Feedback.Grades(kq,1,5),Feedback.Grades(dq,1),Feedback.Grades(nonlinear,1,5)}){Inc("retainedGradeRows");nonlinearPassed&=pass;}
 var contribution1=Product(One(0,5,new Scalar(new Rational(-1,2),0)),One(0,17,new Scalar(new Rational(1,4),0)),'C');var contribution2=Product(One(0,144,new Scalar(new Rational(1,4),0)),One(0,132,new Scalar(new Rational(-1,2),0)),'C');
 foreach(var contribution in new[]{contribution1,contribution2}){Inc("anchorContributionRows");nonlinearPassed&=Equal(contribution,One(0,20,new Scalar(new Rational(1,4),0)));}
 foreach(bool pass in new[]{q.GetValueOrDefault((129,20,0,0))==new Scalar(new Rational(1,2),0),stages[3].GetValueOrDefault((Full,149,0,0))==Scalar.I*-1,stages[4].GetValueOrDefault((0,149,0,0))==Scalar.I,stages[5].GetValueOrDefault((8,157,0,0))==new Scalar(-2),kq.GetValueOrDefault((8,157,0,0))==new Scalar(1),dq.GetValueOrDefault((8,157,0,0)).IsZero,nonlinear.GetValueOrDefault((8,157,0,0))==new Scalar(new Rational(1,3),0)}){Inc("stageAnchorRows");nonlinearPassed&=pass;}
 foreach(bool pass in new[]{Pair(w,w)==1,Pair(w,kq)==1,Pair(w,dq)==0,Pair(w,nonlinear)==new Rational(1,3)}){Inc("witnessPairingRows");nonlinearPassed&=pass;}
 foreach(bool pass in new[]{Pair(j,j)==9,Pair(j,w)==0}){Inc("fieldNormRows");actionPassed&=pass;}
 FT[] fields=[j,w];var adjoints=fields.Select(Caa.AdjointLiteral).ToArray();for(int i=0;i<2;i++){Inc("fieldAdjointChecks");actionPassed&=Equal(adjoints[i],Caa.AdjointSimplified(fields[i]))&&Typed(adjoints[i],2)&&HAnti(adjoints[i]);}
 FT[] qCoefficients=[q,Add(Product(j,w),Product(w,j)),Product(w,w)];
 FT[] naiveQ=[NaiveProduct(j,j,'W'),Add(NaiveProduct(j,w,'W'),NaiveProduct(w,j,'W')),NaiveProduct(w,w,'W')];
 FT[] dqCoefficients=[dq,Add(Adjoint.DQAdjoint(j,adjoints[1]),Adjoint.DQAdjoint(w,kad)),Adjoint.DQAdjoint(w,adjoints[1])];
 var kCoefficients=qCoefficients.Select(Caa.Forward).ToArray();var nCoefficients=new FT[3];(int U,int V)[] powers=[(2,0),(1,1),(0,2)];(int U,int V)[] fieldPowers=[(1,0),(0,1)];var coefficientRows=new List<object>();
 for(int m=0;m<3;m++){Inc("quadraticNaiveRows");actionPassed&=Equal(qCoefficients[m],naiveQ[m]);nCoefficients[m]=Scale(Add(kCoefficients[m],dqCoefficients[m]),new Scalar(new Rational(1,3),0));Inc("quadraticCoefficientRows");Inc("potentialGradientTypeRows");actionPassed&=Typed(nCoefficients[m],1)&&HAnti(nCoefficients[m]);coefficientRows.Add(new{u=powers[m].U,v=powers[m].V,Q=Terms(qCoefficients[m]),KQ=Terms(kCoefficients[m]),adjointComposite=Terms(dqCoefficients[m]),nonlinear=Terms(nCoefficients[m])});}
 actionPassed&=qCoefficients[2].Count==0&&Pair(j,kq)==0&&Pair(j,kCoefficients[1])==0&&Pair(w,kCoefficients[1])==0&&HAnti(w);
 var actionRows=new List<object>();
 foreach(int gamma in new[]{1,2})foreach(int kappa in new[]{-1,0,1})
 {
  Inc("actionRows");var polynomial=new Dictionary<(int U,int V),Rational>();
  for(int i=0;i<2;i++)for(int k=0;k<2;k++)Feedback.PutScalar(polynomial,(fieldPowers[i].U+fieldPowers[k].U,fieldPowers[i].V+fieldPowers[k].V),Pair(fields[i],fields[k])*new Rational(kappa,2));
  for(int i=0;i<2;i++)for(int m=0;m<3;m++)Feedback.PutScalar(polynomial,(fieldPowers[i].U+powers[m].U,fieldPowers[i].V+powers[m].V),Pair(fields[i],kCoefficients[m])*new Rational(gamma,3));
  var actionCoefficients=new List<object>();foreach(var m in Feedback.Monomials(3)){Rational value=Feedback.Get(polynomial,m.U,m.V);Rational target=m==(2,0)?new Rational(9*kappa,2):m==(0,2)?new Rational(kappa,2):m==(2,1)?new Rational(gamma,3):0;Inc("actionCoefficients");actionPassed&=value==target;actionCoefficients.Add(new{u=m.U,v=m.V,value=value.ToString()});}
  var derivativeRows=new List<object>();
  for(int direction=0;direction<2;direction++)foreach(var m in Feedback.Monomials(2))
  {
   var fullGradient=new FT();for(int z=0;z<3;z++)if(powers[z]==m)fullGradient=Add(fullGradient,Scale(nCoefficients[z],gamma));for(int i=0;i<2;i++)if(fieldPowers[i]==m)fullGradient=Add(fullGradient,Scale(fields[i],kappa));
   Rational actionDerivative=direction==0?(m.U+1)*Feedback.Get(polynomial,m.U+1,m.V):(m.V+1)*Feedback.Get(polynomial,m.U,m.V+1);var gradientPairing=Pair(fields[direction],fullGradient);
   Inc("actionDerivativeCoefficients");actionPassed&=actionDerivative==gradientPairing;derivativeRows.Add(new{direction,u=m.U,v=m.V,actionDerivative=actionDerivative.ToString(),gradientPairing=gradientPairing.ToString(),fullGradient=Terms(fullGradient)});
  }
  var gradientAtJ=Add(Scale(nonlinear,gamma),Scale(j,kappa));var projected=Add(Scale(Feedback.Grade(nonlinear,1),gamma),Scale(j,kappa));
  Inc("projectedGradientDecoys");actionPassed&=Pair(w,gradientAtJ)==new Rational(gamma,3)&&Pair(w,projected)==0&&!Equal(gradientAtJ,projected);
  actionRows.Add(new{gamma,kappa,actionCoefficients,derivativeRows,gradientAtJ=Terms(gradientAtJ),projectedGradient=Terms(projected),allowedGradeFiveDerivative=Pair(w,gradientAtJ).ToString(),projectedGradeFiveDerivative=Pair(w,projected).ToString()});
 }
 FT[] transported=[j,kad,q,kq,dq,nonlinear,qCoefficients[1]];if(point==1)foreach(var (t,i) in transported.Select((t,i)=>(t,i))){Inc("pointTransportRows");nonlinearPassed&=Equal(t,previous![i]);}previous=transported;
 frequencyPassed&=transported.Concat(qCoefficients).Concat(nCoefficients).All(t=>t.Keys.All(k=>k.K0==0&&k.K1==0));
 rows.Add(new{point,J=Terms(j),vectorActionMatrices=Feedback.MatrixRows(l),cyclicTrace=Terms(cyclic),outerAdjoint=Terms(outerAdjoint),adjointFirst=Terms(legs.First),adjointSecond=Terms(legs.Second),adjoint=Terms(kad),adjointOracle=Terms(kadOracle),adjointReconstruction=Terms(rebuilt),adjointProbes=probes,Q=Terms(q),matrixQ=Terms(qOracle),stages=stages.Select(Terms).ToArray(),firstOracle=Terms(contraction.First),secondOracle=Terms(contraction.Second),KQ=Terms(kq),KQOracle=Terms(contraction.Full),adjointComposite=Terms(dq),adjointCompositeOracle=Terms(dqOracle),nonlinear=Terms(nonlinear),nonlinearOracle=Terms(nonlinearOracle),nonlinearVector=Terms(Feedback.Grade(nonlinear,1)),nonlinearGradeFive=Terms(Feedback.Grade(nonlinear,5)),witness=Terms(w),witnessNorm=Pair(w,w).ToString(),inputNorm=Pair(j,j).ToString(),witnessPair=Pair(w,nonlinear).ToString(),anchorContributions=new[]{Terms(contribution1),Terms(contribution2)},coefficientRows,actionRows});
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");bool resourcesPassed=frequencyPassed&&CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&Feedback.MatrixProducts<=resources.GetProperty("maximumTrackedMatrixProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumTensorTerms").GetInt32();
bool controlsPassed=knownAnswerPassed&&inputPassed&&adjointPassed&&quadraticPassed&&nonlinearPassed&&actionPassed&&countsPassed&&resourcesPassed;
string verdict=!knownAnswerPassed?precedence[1]:!inputPassed?precedence[2]:!adjointPassed?precedence[3]:!quadraticPassed?precedence[4]:!nonlinearPassed?precedence[5]:!actionPassed?precedence[6]:!countsPassed||!resourcesPassed?precedence[7]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,inputPassed,adjointPassed,quadraticPassed,nonlinearPassed,actionPassed,countsPassed,resourcesPassed,frequencyPassed,counts,trackedCoefficientProducts=CoefficientProducts,trackedMatrixProducts=Feedback.MatrixProducts,largestTensor=LargestTensor,rows,scope=new{sourceOperatorSelected=false,sourceNormSelected=false,sourceMetricSelected=false,domainRestrictedToVectorBivector=false,HJComputed=false,fullCovariantActionComputed=false,smallCarrierClosureClaimed=false,physicalVacuumSelected=false,physicalSpectrumClaimed=false}});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=619,phaseId="phase619-invariant-bivector-nonlinear-feedback-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/invariant_bivector_nonlinear_feedback_audit.json",json);File.WriteAllText(Root+"/output/invariant_bivector_nonlinear_feedback_audit_summary.json",json);Console.WriteLine($"Phase619 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
