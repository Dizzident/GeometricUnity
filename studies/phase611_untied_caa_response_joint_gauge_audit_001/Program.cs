using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static Caa;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase611_untied_caa_response_joint_gauge_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P591="studies/phase591_source_hodge_curvature_branch_audit_001";
const string P593="studies/phase593_companion_action_first_variation_audit_001";
const string P601="studies/phase601_full_hessian_cyclic_closure_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase611-a59-untied-caa-response-joint-gauge-v1";
const string Success="untied-caa-response-controls-pass-source-choice-open";
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
 "physicalAxes": [
  0,
  7
 ],
 "frequencies": [
  1,
  2
 ],
 "constantFrequency": 0,
 "kappa": 0,
 "operator": "canonical untied CAA: firstC,outerA,innerA; Phi1=Gamma1,Phi2=Gamma2",
 "pairing": "normalized Fourier integral, signed exterior metric, -ReTr(XY)/128; real bilinear not Hermitian",
 "phase": "K0 is abstract phase; physical d_t=theta_t wedge partial_phase; d_t^dag=-sigma_t contraction_t partial_phase",
 "basis": [
  "u=theta_t gamma2 cos(nx_t)",
  "e=sum_j!=t,2 theta_j gamma2 gamma_j sin(nx_t), ordered words"
 ],
 "adjointU": "2sum_j!=t,2 theta_t wedge theta_j gamma2 gamma_j cos",
 "adjointE": "2sum_j!=t,2 theta2 wedge theta_j gamma_j sin",
 "outerAdjointSeeds": "zero termwise, but full first adjoints retained before divergence",
 "probeMasks": {
  "u": "formPair XORbit(t) XORbit2",
  "e": "formPair XORbit2",
  "gamma1": "formPair",
  "phase": "1 if H-adjoint-sign negative else i",
  "pairsPerInput": 91,
  "total": 1001
 },
 "hessian": [
  "H u=sigma_t n e",
  "H e=-12n u"
 ],
 "gram": [
  "-sigma_t/2",
  "6"
 ],
 "action": "6sigma_t nxy",
 "matrix": "[[0,-12n],[sigma_t n,0]]",
 "minimal": "lambda^2+12sigma_t n^2 on this closed carrier only",
 "constant": {
  "basis": "u=theta_t gamma2, e=0",
  "gram": "-sigma_t",
  "hessian": "0",
  "periodicAlphaClaimed": false
 },
 "unitPlane": {
  "F": "theta01 Gamma01/2",
  "firstLower": "-theta0 gamma0-theta1 gamma1",
  "inner": "-i top",
  "zero": "+i I",
  "outer": "-2Gamma1",
  "secondLower": "Gamma1",
  "final": "sum_j!=0,1 theta_j gamma_j",
  "adjointFirst": "4Gamma2",
  "adjointSecond": "-28Gamma2",
  "adjointFull": "-24Gamma2",
  "pairings": "-12"
 },
 "joint": {
  "order": 2,
  "variables": [
   "s",
   "y",
   "r"
  ],
  "alpha": "gamma2 sin(nx_t)/n",
  "epsilon": "1+r alpha+r^2 alpha^2/2 modulo degree>2",
  "inverse": "1-r alpha+r^2 alpha^2/2 modulo degree>2",
  "omega": "s u+y e",
  "B": "epsilon^-1 d epsilon=r u",
  "FB": "dB+B wedge B=0",
  "T": "omega-B",
  "phi": "epsilon^-1 Phi epsilon, both occurrences transported",
  "tensorFirstJets": "[Phi,alpha], each26 nonzero Fourier coefficients",
  "action": "6sigma_t n(s-r)y",
  "hessian": "6sigma_t n [[0,1,0],[1,0,-1],[0,-1,0]]",
  "null": [
   1,
   0,
   1
  ],
  "rank": 2,
  "wrongT": "omega+B",
  "wrongAction": "6sigma_t n(s+r)y",
  "wrongNullDefect": "(0,12sigma_t n,0)",
  "finiteOrbitClosureClaimed": false
 },
 "cubic": {
  "order": 3,
  "variables": [
   "x",
   "y",
   "z"
  ],
  "T": "x theta0 Gamma01+y theta1 Gamma12+z theta1 gamma2",
  "Q": "2xy theta01 Gamma02",
  "KQ": "-4xy theta1 gamma2",
  "inner": "0",
  "gammas": [
   1,
   2
  ],
  "action": "4gamma xyz/3",
  "gradient": "4gamma/3*(yz,xz,xy)",
  "claimedCovector": "(0,0,4xy)",
  "mismatchPreserved": true
 },
 "knownWordMenu": "all16384blades times14singletons andOmega onbothsides plus square,31each",
 "exactTolerance": 0,
 "coreFileCount": 726,
 "expectedCounts": {
  "rationalControls": 4,
  "polynomialKnownAnswers": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "rows": 6,
  "fourierRows": 4,
  "constantRows": 2,
  "carrierGramEntries": 18,
  "fullAdjointRows": 11,
  "adjointLegRows": 11,
  "outerAdjointZeroRows": 10,
  "forwardAdjointProbes": 1001,
  "adjointReconstructions": 11,
  "fullAdjointNonzeroRows": 10,
  "hessianColumns": 10,
  "hessianStageComparisons": 70,
  "hessianSquaredColumns": 8,
  "actionBilinears": 18,
  "closedMatrixEntries": 18,
  "reciprocityEntries": 18,
  "minimalPolynomialRows": 4,
  "constantZeroRows": 2,
  "fixedEpsilonNonNullRows": 4,
  "unitPlaneStageControls": 8,
  "unitAdjointLegChecks": 3,
  "unitAdjointPairings": 2,
  "unitNonzeroSecondChecks": 2,
  "epsilonInverseRows": 4,
  "referenceConnectionRows": 4,
  "referenceCurvatureZeroRows": 4,
  "transportedTensorJetRows": 8,
  "jointTypedRows": 4,
  "jointActionCoefficients": 40,
  "jointWrongActionCoefficients": 40,
  "jointGradientCoefficients": 120,
  "jointHessianEntries": 36,
  "jointWrongHessianEntries": 36,
  "jointNullRows": 4,
  "jointRankRows": 4,
  "jointWrongLiftRows": 4,
  "cubicRows": 2,
  "cubicQRows": 1,
  "cubicKRows": 1,
  "cubicInnerRows": 1,
  "cubicOuterAdjointZeroRows": 3,
  "cubicCoefficientChecks": 40,
  "cubicGradientChecks": 120,
  "cubicTargetChecks": 120,
  "cubicMismatchRows": 2
 },
 "resources": {
  "estimatedCpuSeconds": 60,
  "maximumEstimatedCpuSeconds": 180,
  "estimatedPeakBytes": 268435456,
  "maximumEstimatedPeakBytes": 805306368,
  "maximumTrackedCoefficientProducts": 50000000,
  "maximumFourierTerms": 8192,
  "maximumPolynomialDegree": 3,
  "maximumPolynomialCoefficients": 20,
  "maximumCarrierDimension": 3
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormSelected": false,
  "tiedFamilyTheoremOverturned": false,
  "fullOperatorDeterminantClaimed": false,
  "periodicZeroFrequencyGaugeClaimed": false,
  "finiteGaugeOrbitClosed": false,
  "physicalPropagationClaimed": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","full-adjoint-control-failed","full-hessian-action-control-failed","joint-quadratic-control-failed","cubic-boundary-control-failed","resource-census-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase611UntiedCaaResponseJointGaugeAudit.csproj",["study"]=Root+"/STUDY.md",["caa-helper"]=Root+"/CaaOperator.cs",["polynomial-helper"]=Root+"/PolynomialJets.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["phase591-study"]=P591+"/STUDY.md",["phase591-summary"]=P591+"/output/source_hodge_curvature_branch_audit_summary.json",["phase591-contract"]=P591+"/preregistration/contract_v1.json",
 ["phase593-study"]=P593+"/STUDY.md",["phase593-summary"]=P593+"/output/companion_action_first_variation_audit_summary.json",["phase593-contract"]=P593+"/preregistration/contract_v1.json",
 ["phase601-study"]=P601+"/STUDY.md",["phase601-summary"]=P601+"/output/full_hessian_cyclic_closure_audit_summary.json",["phase601-contract"]=P601+"/preregistration/contract_v1.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==611&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase591","canonical-source-hodge-curvature-branches-pass-choice-open"),("phase593","companion-action-variation-mismatch-certified-source-choice-open"),("phase601","full-hessian-cyclic-closure-controls-pass-compression-not-spectrum")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(9.1)","(9.4)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}


var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string k)=>counts[k]++;
bool knownAnswerPassed=true,adjointPassed=true,hessianPassed=true,jointPassed=true,cubicPassed=true;
Rational Inv(Rational x)=>new(x.Denominator,x.Numerator);
bool[] arithmetic=[new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)];
foreach(bool pass in arithmetic){Inc("rationalControls");knownAnswerPassed&=pass;}
for(int a=0;a<16384;a++)
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}
 Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);
}
var polynomialX=FP.Variable(2,0,One(0,1,1));var square=FP.Product(polynomialX,polynomialX);var scalarPoly=new SP(2);scalarPoly.Put(new(2,0,0),3);
bool[] polyControls=[FP.Product(square,polynomialX).Zero,Equal(square.Get(new(2,0,0)),One(0,0,1)),scalarPoly.Derivative(0).Derivative(0).Get(new(0,0,0))==6,FP.Pair(polynomialX,polynomialX).Get(new(2,0,0))==-1];
foreach(bool pass in polyControls){Inc("polynomialKnownAnswers");knownAnswerPassed&=pass;}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
var rows=new List<object>();var jointRows=new List<object>();var cubicRows=new List<object>();
// Positive control exercises both nonzero A legs independently of the seeds.
var unitF=One(3,3,Fourier.Half);var unitStages=ForwardStages(unitF);var unitNaive=ForwardStages(unitF,true);var firstUnit=Scale(Add(One(1,1,1),One(2,2,1)),-1);var unitFinal=Add(Gamma1,firstUnit);
FT[] unitExpected=[unitF,One(Full^3,3,Fourier.Half),Star(firstUnit),One(Full,0,Scalar.I*-1),One(0,0,Scalar.I),Scale(Gamma1,-2),Star(unitFinal),unitFinal];
for(int s=0;s<8;s++){Inc("unitPlaneStageControls");adjointPassed&=Equal(unitStages[s],unitExpected[s])&&Equal(unitStages[s],unitNaive[s])&&HAnti(unitStages[s]);}
var unitAdjoint=AdjointLegs(Gamma1);var unitKad=Add(unitAdjoint.First,unitAdjoint.Second);Inc("fullAdjointRows");Inc("adjointLegRows");counts["unitAdjointLegChecks"]+=3;
adjointPassed&=Equal(unitAdjoint.First,Scale(Gamma2,4))&&Equal(unitAdjoint.Second,Scale(Gamma2,-28))&&Equal(unitKad,Scale(Gamma2,-24))&&Equal(unitKad,AdjointSimplified(Gamma1));
var unitReconstruction=ReconstructAdjoint(Gamma1,0,0,2,pass=>{Inc("forwardAdjointProbes");adjointPassed&=pass;});Inc("adjointReconstructions");adjointPassed&=Equal(unitReconstruction,unitKad);
counts["unitAdjointPairings"]+=2;counts["unitNonzeroSecondChecks"]+=2;adjointPassed&=Pair(Gamma1,unitStages[7])==-12&&Pair(unitF,unitKad)==-12&&Equal(Scale(unitStages[5],Fourier.Half*-1),Gamma1)&&unitAdjoint.Second.Count==91;
foreach(int t in new[]{0,7})foreach(int n in new[]{1,2,0})
{
 Inc("rows");if(n==0)Inc("constantRows");else Inc("fourierRows");var u=U(t,n);var e=E(t,n);FT[] basis=n==0?[u]:[u,e];int dim=basis.Length;Rational[] gram=n==0?[(Rational)(-Sigma(t))]:[new Rational(-Sigma(t),2),6];var adjoints=new FT[dim];var images=new FT[dim];var matrix=new Rational[dim,dim];var lowered=new Rational[dim,dim];var adjointTerms=new List<object>();
 for(int i=0;i<dim;i++)for(int j=0;j<dim;j++){Inc("carrierGramEntries");hessianPassed&=Pair(basis[i],basis[j])==(i==j?gram[i]:0);}
 for(int j=0;j<dim;j++)
 {
  var legs=AdjointLegs(basis[j]);var actual=Add(legs.First,legs.Second);var expectedKad=j==0?ExpectedAdjointU(t,n):ExpectedAdjointE(t,n);adjoints[j]=actual;
  Inc("fullAdjointRows");Inc("adjointLegRows");Inc("outerAdjointZeroRows");Inc("fullAdjointNonzeroRows");adjointPassed&=Equal(actual,expectedKad)&&Equal(actual,AdjointSimplified(basis[j]))&&Equal(legs.First,expectedKad)&&legs.Second.Count==0&&OuterAdjoint(basis[j]).Count==0&&actual.Count==(n==0?12:24)&&HAnti(actual);
  var reconstruction=ReconstructAdjoint(basis[j],t,n,j,pass=>{Inc("forwardAdjointProbes");adjointPassed&=pass;});Inc("adjointReconstructions");adjointPassed&=Equal(reconstruction,actual);
  var derivative=Derivative(basis[j],t);var stages=ForwardStages(derivative);var naive=ForwardStages(derivative,true);
  for(int stage=1;stage<8;stage++){Inc("hessianStageComparisons");hessianPassed&=Equal(stages[stage],naive[stage])&&HAnti(stages[stage]);}
  var div=DerivativeAdjoint(actual,t);var image=Scale(Add(stages[7],div),Fourier.Half);images[j]=image;Inc("hessianColumns");
  var predicted=n==0?new FT():j==0?Scale(e,Sigma(t)*n):Scale(u,-12*n);
  hessianPassed&=Equal(image,predicted)&&Equal(image,Hessian(basis[j],t))&&Equal(stages[7],j==1?Scale(u,-24*n):new FT())&&Equal(div,j==0?Scale(e,2*Sigma(t)*n):new FT())&&Typed(image,1)&&HAnti(image);
  FT rebuilt=new();for(int i=0;i<dim;i++){matrix[i,j]=Pair(basis[i],image)*Inv(gram[i]);rebuilt=Add(rebuilt,Scale(basis[i],new Scalar(matrix[i,j],0)));}hessianPassed&=Equal(rebuilt,image);
  if(n!=0){Inc("hessianSquaredColumns");hessianPassed&=Equal(Hessian(image,t),Scale(basis[j],-12*Sigma(t)*n*n));}
  adjointTerms.Add(new{input=j,first=Terms(legs.First),second=Terms(legs.Second),full=Terms(actual),divergence=Terms(div),dInput=Terms(derivative),kD=Terms(stages[7]),hessian=Terms(image)});
 }
 for(int i=0;i<dim;i++)for(int j=0;j<dim;j++)
 {
  Inc("closedMatrixEntries");Inc("actionBilinears");Inc("reciprocityEntries");Rational target=n==0?0:i==0&&j==1?-12*n:i==1&&j==0?Sigma(t)*n:0;
  Rational action=(Pair(basis[i],Forward(Derivative(basis[j],t)))+Pair(basis[j],Forward(Derivative(basis[i],t))))*new Rational(1,2);lowered[i,j]=action;
  hessianPassed&=matrix[i,j]==target&&action==gram[i]*matrix[i,j]&&gram[i]*matrix[i,j]==gram[j]*matrix[j,i];
 }
 if(n==0){Inc("constantZeroRows");hessianPassed&=e.Count==0&&images[0].Count==0&&Pair(u,u)==-Sigma(t);}
 else
 {
  Inc("minimalPolynomialRows");Inc("fixedEpsilonNonNullRows");hessianPassed&=matrix[0,0]==0&&matrix[1,1]==0&&matrix[0,1]*matrix[1,0]==-12*Sigma(t)*n*n&&matrix[0,1]!=0&&matrix[1,0]!=0&&images[0].Count>0;
  // Full retained degree-two epsilon/connection computation; no pulled-back
  // expected action or matrix is inserted into this side.
  var alpha=Scale(Mode(n,true,0,1<<2,1),new Scalar(new Rational(1,n),0));var identity=FP.Constant(2,One(0,0,1));var ra=FP.Variable(2,2,alpha);var ra2=FP.Scale(FP.Product(ra,ra),Fourier.Half);
  var epsilon=FP.Add(FP.Add(identity,ra),ra2);var inverse=FP.Add(FP.Add(identity,FP.Scale(ra,-1)),ra2);var omega=FP.Add(FP.Variable(2,0,u),FP.Variable(2,1,e));var b=FP.Product(inverse,FP.D(epsilon,t));var fb=FP.Add(FP.D(b,t),FP.Product(b,b));var torsion=FP.Add(omega,FP.Scale(b,-1));var wrongT=FP.Add(omega,b);
  var phi1=FP.Product(FP.Product(inverse,FP.Constant(2,Gamma1)),epsilon);var phi2=FP.Product(FP.Product(inverse,FP.Constant(2,Gamma2)),epsilon);
  Inc("epsilonInverseRows");Inc("referenceConnectionRows");Inc("referenceCurvatureZeroRows");
  jointPassed&=FP.Equal(FP.Product(inverse,epsilon),identity)&&FP.Equal(FP.Product(epsilon,inverse),identity)&&FP.Equal(FP.HAdjoint(epsilon),inverse)&&FP.Equal(b,FP.Variable(2,2,u))&&fb.Zero&&Equal(Derivative(alpha,t),u);
  foreach(var (phi,canonical) in new[]{(phi1,Gamma1),(phi2,Gamma2)})
  {Inc("transportedTensorJetRows");var jet=phi.Get(new(0,0,1));jointPassed&=Equal(jet,Product(canonical,alpha,'C'))&&jet.Count==26;}
  SP Action(FP tt)
  {
   var dbt=FP.Add(FP.D(tt,t),FP.Add(FP.Product(b,tt),FP.Product(tt,b)));var q=FP.Product(tt,tt);var argument=FP.Add(fb,FP.Add(FP.Scale(dbt,Fourier.Half),FP.Scale(q,new Scalar(new Rational(1,3),0))));
   var k=FP.Chain(argument,phi1,phi2);jointPassed&=FP.Typed(tt,1)&&FP.Typed(dbt,2)&&FP.Typed(q,2)&&FP.Typed(k,1)&&FP.Anti(tt)&&FP.Anti(dbt)&&FP.Anti(q)&&FP.Anti(k);return FP.Pair(tt,k);
  }
  var action=Action(torsion);var wrongAction=Action(wrongT);Inc("jointTypedRows");jointPassed&=FP.Typed(b,1)&&FP.Typed(fb,2)&&FP.Typed(phi1,1)&&FP.Typed(phi2,2)&&FP.Anti(b)&&FP.Anti(fb)&&FP.Anti(phi1)&&FP.Anti(phi2);
  Rational a=6*Sigma(t)*n;var actionOracle=new SP(2);actionOracle.Put(new(1,1,0),a);actionOracle.Put(new(0,1,1),a*-1);var wrongOracle=new SP(2);wrongOracle.Put(new(1,1,0),a);wrongOracle.Put(new(0,1,1),a);
  foreach(var m in Monomial.Menu(2)){Inc("jointActionCoefficients");Inc("jointWrongActionCoefficients");jointPassed&=action.Get(m)==actionOracle.Get(m)&&wrongAction.Get(m)==wrongOracle.Get(m);}
  for(int variable=0;variable<3;variable++)foreach(var m in Monomial.Menu(2)){Inc("jointGradientCoefficients");jointPassed&=action.Derivative(variable).Get(m)==actionOracle.Derivative(variable).Get(m);}
  var hessian=new Rational[3,3];var wrongHessian=new Rational[3,3];int[,] pattern={{0,1,0},{1,0,-1},{0,-1,0}},wrongPattern={{0,1,0},{1,0,1},{0,1,0}};
  for(int i=0;i<3;i++)for(int j=0;j<3;j++){hessian[i,j]=action.Derivative(i).Derivative(j).Get(new(0,0,0));wrongHessian[i,j]=wrongAction.Derivative(i).Derivative(j).Get(new(0,0,0));Inc("jointHessianEntries");Inc("jointWrongHessianEntries");jointPassed&=hessian[i,j]==a*pattern[i,j]&&wrongHessian[i,j]==a*wrongPattern[i,j];}
  Inc("jointNullRows");Inc("jointRankRows");Inc("jointWrongLiftRows");jointPassed&=Enumerable.Range(0,3).All(i=>hessian[i,0]+hessian[i,2]==0)&&hessian[0,1]!=0&&hessian[1,0]!=0&&wrongHessian[0,0]+wrongHessian[0,2]==0&&wrongHessian[1,0]+wrongHessian[1,2]==2*a&&wrongHessian[2,0]+wrongHessian[2,2]==0&&a!=0;
  jointRows.Add(new{t,n,alpha=Terms(alpha),epsilon=epsilon.Terms(),inverse=inverse.Terms(),B=b.Terms(),FB=fb.Terms(),phi1=phi1.Terms(),phi2=phi2.Terms(),torsion=torsion.Terms(),action=action.Terms(),wrongAction=wrongAction.Terms(),hessian=MatrixText(hessian),wrongHessian=MatrixText(wrongHessian),nullVector=new[]{1,0,1},rank=2,finiteOrbitClosed=false});
 }
 rows.Add(new{t,n,constant=n==0,gram=gram.Select(v=>v.ToString()).ToArray(),basis=basis.Select(Terms).ToArray(),adjointRows=adjointTerms,matrix=MatrixText(matrix),lowered=MatrixText(lowered),closedCarrierOnly=true,periodicAlphaClaimed=n!=0});
}
// The old first cubic witness is evaluated through the new literal CAA,
// not inherited as an expected scalar value.
FT[] torsionBasis=[One(1,3,1),One(2,6,1),One(2,4,1)];var cubicT=new FP(3);for(int i=0;i<3;i++)cubicT=FP.Add(cubicT,FP.Variable(3,i,torsionBasis[i]));
var qCubic=FP.Product(cubicT,cubicT);var kCubic=FP.Chain(qCubic,FP.Constant(3,Gamma1),FP.Constant(3,Gamma2));var qOracle=new FP(3);qOracle.Put(new(1,1,0),One(3,5,2));var kOracle=new FP(3);kOracle.Put(new(1,1,0),One(2,4,-4));
Inc("cubicQRows");Inc("cubicKRows");Inc("cubicInnerRows");cubicPassed&=FP.Equal(qCubic,qOracle)&&FP.Equal(kCubic,kOracle)&&FP.Product(FP.Constant(3,Gamma2),FP.Star(qCubic),'A').Zero;
foreach(var v in torsionBasis){Inc("cubicOuterAdjointZeroRows");cubicPassed&=OuterAdjoint(v).Count==0;}
var candidate=Enumerable.Range(0,3).Select(i=>FP.Pair(FP.Constant(3,torsionBasis[i]),kCubic)).ToArray();var baseAction=FP.Pair(cubicT,kCubic);
foreach(int gamma in new[]{1,2})
{
 Inc("cubicRows");var action=baseAction.Scale(new Rational(gamma,3));var oracle=new SP(3);oracle.Put(new(1,1,1),new Rational(4*gamma,3));var force=new SP[3];for(int i=0;i<3;i++)force[i]=new SP(3);force[2].Put(new(1,1,0),4);
 foreach(var m in Monomial.Menu(3)){Inc("cubicCoefficientChecks");cubicPassed&=action.Get(m)==oracle.Get(m);}
 bool mismatch=false;for(int i=0;i<3;i++)foreach(var m in Monomial.Menu(3)){Inc("cubicGradientChecks");Inc("cubicTargetChecks");var value=action.Derivative(i).Get(m);cubicPassed&=value==oracle.Derivative(i).Get(m)&&candidate[i].Get(m)==force[i].Get(m);mismatch|=value!=candidate[i].Get(m);}
 Inc("cubicMismatchRows");cubicPassed&=mismatch&&action.Derivative(2).Get(new(1,1,0))==new Rational(4*gamma,3)&&action.Derivative(2).Get(new(1,1,0))!=4;
 cubicRows.Add(new{gamma,Q=qCubic.Terms(),KQ=kCubic.Terms(),action=action.Terms(),gradient=Enumerable.Range(0,3).Select(i=>action.Derivative(i).Terms()).ToArray(),claimedCovector=candidate.Select(v=>v.Terms()).ToArray(),mismatchCertified=mismatch});
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");bool resourcesPassed=CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumFourierTerms").GetInt32();
bool controlsPassed=knownAnswerPassed&&adjointPassed&&hessianPassed&&jointPassed&&cubicPassed&&countsPassed&&resourcesPassed;string verdict=!knownAnswerPassed?precedence[1]:!adjointPassed?precedence[2]:!hessianPassed?precedence[3]:!jointPassed?precedence[4]:!cubicPassed?precedence[5]:!countsPassed||!resourcesPassed?precedence[6]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,adjointPassed,hessianPassed,jointPassed,cubicPassed,countsPassed,resourcesPassed,counts,trackedCoefficientProducts=CoefficientProducts,largestTensor=LargestTensor,rows,jointRows,cubicRows,unitPlane=new{stages=unitStages.Select(Terms).ToArray(),adjointFirst=Terms(unitAdjoint.First),adjointSecond=Terms(unitAdjoint.Second),adjointFull=Terms(unitKad),pairingForward=Pair(Gamma1,unitStages[7]).ToString(),pairingAdjoint=Pair(unitF,unitKad).ToString()},sourceOperatorSelected=false,sourceNormSelected=false,tiedFamilyTheoremOverturned=false,fullOperatorDeterminantClaimed=false,periodicZeroFrequencyGaugeClaimed=false,finiteGaugeOrbitClosed=false,physicalPropagationClaimed=false});

static string[][] MatrixText(Rational[,] m)=>Enumerable.Range(0,m.GetLength(0)).Select(i=>Enumerable.Range(0,m.GetLength(1)).Select(j=>m[i,j].ToString()).ToArray()).ToArray();
void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=611,phaseId="phase611-untied-caa-response-joint-gauge-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/untied_caa_response_joint_gauge_audit.json",json);File.WriteAllText(Root+"/output/untied_caa_response_joint_gauge_audit_summary.json",json);Console.WriteLine($"Phase611 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
