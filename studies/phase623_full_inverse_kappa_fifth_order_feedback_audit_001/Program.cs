using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001";
const string ShardDirectory="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/output/points";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase623-a64-full-inverse-kappa-fifth-order-feedback-v1";
const string Success="full-inverse-kappa-fifth-order-controls-pass-grade-five-branch-required";
const string SummaryPath=Root+"/output/full_inverse_kappa_fifth_order_feedback_audit_summary.json";
const string FullPath=Root+"/output/full_inverse_kappa_fifth_order_feedback_audit.json";
bool replayMode=args.SequenceEqual(new[]{"--verify-evidence"});if(args.Length>0&&!replayMode)throw new ArgumentException("Only --verify-evidence is supported");
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
 "horizontalAxes": [
  0,
  7,
  8,
  9
 ],
 "tracelessAxes": [
  1,
  2,
  3,
  4,
  5,
  6,
  11,
  12,
  13
 ],
 "traceAxis": 10,
 "gammas": [
  -2,
  -1,
  0,
  1,
  2
 ],
 "lambdaOrder": 5,
 "maximumGammaOrder": 2,
 "halfPolarization": "Bcal(X,Y)=[N(X+Y)-N(X)-N(Y)]/2",
 "originalEquation": "A+H S+gamma N(S)+kappa S=0; lambda=1/kappa; finite nonzero lambda",
 "sourceWeights": [
  "-21/4",
  "-15/4",
  "-21/4"
 ],
 "JTerms": 36,
 "NJTerms": 614,
 "NJGradeFiveTerms": 600,
 "NJVectorWeights": [
  "3",
  "22/3",
  "6"
 ],
 "witness": {
  "form": 8,
  "blade": 157,
  "pairNorm": "1",
  "NJ": "1/3",
  "S5GammaCoefficient": "-3/4"
 },
 "carrier": {
  "basis": [
   "J",
   "B",
   "C"
  ],
  "norms": [
   "9",
   "-1",
   "-9"
  ],
  "B": "B_u=-gamma_u gamma_t/2 for horizontal u",
  "C": "C_A=gamma_A wedgeCl gamma_t for traceless A",
  "L": "-B+C",
  "completeOnlyInInvariantBivectorOneForms": true
 },
 "diagonalMixedCoefficients": [
  "-4/3",
  "-22/3",
  "-2/3"
 ],
 "mixedLiteralLegs": {
  "halfQ": "b*KdagJ/2",
  "KhalfQ": "-2b J",
  "DQ_V_dag_KdagJ": "-4b J",
  "DQ_J_dag_KdagV": "-4(2a+9b+c)J"
 },
 "N_A_Weights": [
  "11043/2",
  "11655/2",
  "11043/2"
 ],
 "series": {
  "S1": {
   "gamma0": [
    "21/4",
    "15/4",
    "21/4"
   ]
  },
  "S2": {
   "gamma0J": "3/2"
  },
  "S3": {
   "gamma0": [
    "-27/8",
    "3/2",
    "0"
   ],
   "gamma1": [
    "-11043/2",
    "-11655/2",
    "-11043/2"
   ]
  },
  "S4": {
   "gamma0J": "-39/8",
   "gamma0L": "-27/8",
   "gamma1J": "420"
  },
  "S5": {
   "gamma0": [
    "1161/32",
    "51/2",
    "135/4"
   ],
   "gamma1Vector": [
    "-1701",
    "1275/2",
    "-189/2"
   ],
   "gamma1GradeFive": "-9/4 times full passed619 N(J)grade5",
   "gamma2": [
    "15072318",
    "15422130",
    "15072318"
   ]
  }
 },
 "recursiveFeedback": {
  "N11": [
   "11043/2",
   "11655/2",
   "11043/2"
  ],
  "B12J": "-57",
  "B130": [
   "2997/8",
   "-117",
   "81/2"
  ],
  "B131": [
   "-7536159",
   "-7711065",
   "-7536159"
  ],
  "N22": "9/4 times full passed619 N(J)"
 },
 "actionDirections": [
  "PHGamma",
  "PEGamma",
  "PtGamma",
  "J",
  "B",
  "C",
  "W5",
  "central"
 ],
 "actionDirectionNorms": [
  "-4",
  "-9",
  "-1",
  "9",
  "-1",
  "-9",
  "1",
  "1"
 ],
 "actionControl": "coefficient of r*s*t in original Pair(rX+sY+tU,K Q(rX+sY+tU))/3 equals2 Pair(U,Bcal(X,Y)); eight full directions",
 "residualControl": "complete original lambda powers0..4 times gamma powers0..2 vanish; no HS5 execution",
 "storage": {
  "schema": "two-complete-expanded-point-shards-v1",
  "paths": [
   "studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/output/points/point0.json",
   "studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/output/points/point1.json"
  ],
  "eachMaximumBytes": 67108864,
  "aggregateMaximumBytes": 134217728,
  "encoding": "UTF8 deterministic indented JSON plus one LF; all primary and independent stage tensors fully expanded; canonical sorted masks and reduced rational strings",
  "readonlyVerification": "--verify-evidence validates frozen preflight, exact two paths, hashes, bytes, canonical tensor entries, all point census array lengths and unchanged manifests; no computation or writes"
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "expectedCounts": {
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "domainBladeControls": 16384,
  "reflectionHodgeControls": 16384,
  "centralDomainControls": 2,
  "contexts": 2,
  "inputRows": 6,
  "matrixMetricEntries": 5488,
  "matrixSupportRows": 28,
  "canonicalParallelRows": 56,
  "omittedSlotDecoys": 16,
  "carrierNormRows": 16,
  "carrierOrthogonalityRows": 56,
  "carrierIsotropyRows": 72,
  "carrierReflectionRows": 12,
  "canonicalReflectionRows": 4,
  "cyclicCarrierRows": 2,
  "signedMetricDecoys": 2,
  "kineticRows": 12,
  "derivativeComparisons": 168,
  "reverseComparisons": 168,
  "kineticStageComparisons": 96,
  "kineticAdjointChecks": 12,
  "kineticTypeRows": 12,
  "feedbackRows": 16,
  "feedbackProductComparisons": 16,
  "feedbackAdjointChecks": 32,
  "feedbackStageComparisons": 128,
  "feedbackOracleChecks": 16,
  "feedbackKReflectionChecks": 16,
  "feedbackAdjointReflectionChecks": 32,
  "mixedLiteralLegRows": 24,
  "polarizationFactorDecoys": 6,
  "seriesCoefficientRows": 30,
  "seriesGradeRows": 30,
  "convolutionOrderedTerms": 16,
  "convolutionStageComparisons": 64,
  "convolutionRows": 8,
  "originalResidualCoefficients": 30,
  "fullGradeFiveRows": 2,
  "gammaEvaluationRows": 50,
  "projectedFifthOrderDecoys": 8,
  "actionStageComparisons": 2048,
  "originalCubicDerivativeRows": 128,
  "potentialTrilinearityRows": 128,
  "transportCoefficientRows": 15,
  "evidenceShards": 2,
  "actionQuadraticComparisons": 256
 },
 "resources": {
  "estimatedCpuSeconds": 180,
  "maximumEstimatedCpuSeconds": 600,
  "estimatedPeakBytes": 536870912,
  "maximumEstimatedPeakBytes": 2147483648,
  "maximumTrackedCoefficientProducts": 500000000,
  "maximumTrackedSlotProducts": 100000000,
  "maximumTensorTerms": 65536,
  "maximumMatrixDimension": 14,
  "maximumFrequency": 0,
  "maximumRationalCharacters": 64,
  "maximumShardBytes": 67108864,
  "maximumAggregateShardBytes": 134217728
 },
 "scope": {
  "sourceOperatorSelected": false,
  "physicalCouplingSelected": false,
  "sourceNormSelected": false,
  "domainRestrictedToVectorBivector": false,
  "pointwiseKineticSelfAdjointnessAssumed": false,
  "physicalVacuumSelected": false,
  "physicalSpectrumClaimed": false,
  "branchAnalyticityConditional": true
 },
 "compiledSourceCount": 7
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","passed-input-carrier-control-failed","full-covariant-kinetic-control-failed","full-polarized-feedback-control-failed","full-fifth-order-recursion-control-failed","original-cubic-action-control-failed","nonzero-omission-control-failed","resource-census-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/Program.cs",
 ["project"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/Phase623FullInverseKappaFifthOrderFeedbackAudit.csproj",
 ["study"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/STUDY.md",
 ["inverse-helper"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/InverseFeedback.cs",
 ["arithmetic-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/ExactArithmetic.cs",
 ["fourier-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/FourierTensor.cs",
 ["adjoint-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/TraceAdjoint.cs",
 ["phase600-program"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/Program.cs",
 ["phase600-study"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/STUDY.md",
 ["phase600-contract"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/preregistration/contract_v1.json",
 ["phase600-summary"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",
 ["phase611-program"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/Program.cs",
 ["phase611-study"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/STUDY.md",
 ["phase611-contract"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/preregistration/contract_v1.json",
 ["phase611-summary"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/output/untied_caa_response_joint_gauge_audit_summary.json",
 ["phase611-helper"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/CaaOperator.cs",
 ["phase611-project"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/Phase611UntiedCaaResponseJointGaugeAudit.csproj",
 ["phase617-program"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/Program.cs",
 ["phase617-study"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/STUDY.md",
 ["phase617-contract"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/preregistration/contract_v1.json",
 ["phase617-summary"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/output/nonparallel_projector_two_weight_gradient_audit_summary.json",
 ["phase617-helper"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/ProjectorGradient.cs",
 ["phase617-project"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/Phase617NonparallelProjectorTwoWeightGradientAudit.csproj",
 ["phase618-program"]="studies/phase618_homogeneous_covariant_connection_audit_001/Program.cs",
 ["phase618-study"]="studies/phase618_homogeneous_covariant_connection_audit_001/STUDY.md",
 ["phase618-contract"]="studies/phase618_homogeneous_covariant_connection_audit_001/preregistration/contract_v1.json",
 ["phase618-summary"]="studies/phase618_homogeneous_covariant_connection_audit_001/output/homogeneous_covariant_connection_audit_summary.json",
 ["phase618-helper"]="studies/phase618_homogeneous_covariant_connection_audit_001/HomogeneousConnection.cs",
 ["phase618-project"]="studies/phase618_homogeneous_covariant_connection_audit_001/Phase618HomogeneousCovariantConnectionAudit.csproj",
 ["phase619-program"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/Program.cs",
 ["phase619-study"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/STUDY.md",
 ["phase619-contract"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/preregistration/contract_v1.json",
 ["phase619-summary"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/output/invariant_bivector_nonlinear_feedback_audit_summary.json",
 ["phase619-helper"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/BivectorFeedback.cs",
 ["phase619-project"]="studies/phase619_invariant_bivector_nonlinear_feedback_audit_001/Phase619InvariantBivectorNonlinearFeedbackAudit.csproj",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["core-source-manifest"]="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json",
 ["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false,compiledSourceClosureValid=false;string[] compiledSources=[];int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==623&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid)
 {
  var projectXml=System.Xml.Linq.XDocument.Load(paths["project"]);
  var includes=projectXml.Descendants("Compile").Select(q=>q.Attribute("Include")?.Value).Where(q=>q is not null).Select(q=>Path.GetRelativePath(".",Path.GetFullPath(Path.Combine(Root,q!))).Replace('\\','/'));
  var owned=Directory.EnumerateFiles(Root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Split('/').Any(v=>v is "bin" or "obj"));
  compiledSources=owned.Concat(includes).Order(StringComparer.Ordinal).ToArray();compiledSourceClosureValid=compiledSources.Length==7&&compiledSources.Distinct(StringComparer.Ordinal).Count()==7&&compiledSources.All(p=>bindings.Count(q=>q.path==p)==1);
 }
 if(contractValid&&exactBindingsValid&&compiledSourceClosureValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid||!compiledSourceClosureValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase611","untied-caa-response-controls-pass-source-choice-open"),("phase617","nonparallel-projector-controls-pass-two-weight-ansatz-nonstationary"),("phase618","homogeneous-connection-controls-pass-conditional-local-existence"),("phase619","invariant-bivector-controls-pass-nonlinear-grade-five-required")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.27)","(3.34)","(9.1)","(9.4)","(12.26)","(12.27)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

if(replayMode){VerifyEvidence();return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string key)=>counts[key]++;
bool knownAnswerPassed=true,inputPassed=true,kineticPassed=true,polarizationPassed=true,recursionPassed=true,actionPassed=true,decoysPassed=true,frequencyPassed=true;
foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("arithmeticControls");knownAnswerPassed&=pass;}
for(int a=0;a<16384;a++)
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);
 Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);
 Inc("domainBladeControls");knownAnswerPassed&=HAnti(One(0,a,AdjointSign(a)==-1?1:Scalar.I));
 var form=One(a,0,1);Inc("reflectionHodgeControls");knownAnswerPassed&=Equal(Fifth.Reflect(Star(form)),Scale(Star(Fifth.Reflect(form)),-1));
}
foreach(bool pass in new[]{HAnti(One(0,0,Scalar.I)),Equal(Product(One(0,1,1),One(0,1,1),'A'),One(0,0,Scalar.I*2))}){Inc("centralDomainControls");knownAnswerPassed&=pass;}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
using var p617=JsonDocument.Parse(File.ReadAllBytes(paths["phase617-summary"]));using var p618=JsonDocument.Parse(File.ReadAllBytes(paths["phase618-summary"]));using var p619=JsonDocument.Parse(File.ReadAllBytes(paths["phase619-summary"]));
var input617=p617.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().ToArray();var input618=p618.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().ToArray();var input619=p619.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().ToArray();
var shards=new List<Shard>();long aggregateBytes=0,maxShardBytes=0;FT[][]? previous=null;
try
{
for(int point=0;point<2;point++)
{
 Inc("contexts");var r617=input617.Single(q=>q.GetProperty("point").GetInt32()==point);var r618=input618.Single(q=>q.GetProperty("point").GetInt32()==point);var r619=input619.Single(q=>q.GetProperty("point").GetInt32()==point);
 var lambda=r618.GetProperty("frameNomizu").EnumerateArray().Select(Fifth.Matrix).ToArray();if(lambda.Length!=14)throw new ArgumentException("fourteen Nomizu matrices");
 var a=Feedback.Read(r617.GetProperty("sourceStages")[7]);var j=Feedback.Read(r617.GetProperty("kinetic"));var nj=Feedback.Read(r619.GetProperty("nonlinear"));var nj5=Feedback.Grade(nj,5);var b=Fifth.B();var c=Fifth.C();var l=Add(Scale(b,-1),c);var y=Caa.AdjointLiteral(j);
 foreach(bool pass in new[]{Equal(a,Fifth.Diagonal(new Rational(-21,4),new Rational(-15,4),new Rational(-21,4)))&&Equal(a,Feedback.Read(r618.GetProperty("source"))),j.Count==36&&Equal(j,Feedback.Read(r618.GetProperty("J")))&&Equal(j,Feedback.Read(r619.GetProperty("J"))),nj.Count==614&&nj5.Count==600&&Equal(Feedback.Grade(nj,1),Fifth.Diagonal(3,new Rational(22,3),6))&&Equal(nj,Feedback.Read(r619.GetProperty("nonlinearOracle")))}){Inc("inputRows");inputPassed&=pass;}
 var normizuRows=new List<object>();
 for(int axis=0;axis<14;axis++)
 {
  Rational pairSum=0;for(int u=0;u<14;u++)for(int v=0;v<14;v++){Inc("matrixMetricEntries");inputPassed&=Sigma(u)*lambda[axis][u,v]+Sigma(v)*lambda[axis][v,u]==0;if(u<v)pairSum+=Abs(lambda[axis][u,v]);}
  Inc("matrixSupportRows");inputPassed&=pairSum==(axis is 0 or 7 or 8 or 9?new Rational(5,2):0);
  foreach(var canonical in new[]{Caa.Gamma1,Caa.Gamma2}){Inc("canonicalParallelRows");inputPassed&=Fifth.Derivative(lambda[axis],canonical).Count==0&&Fifth.ExteriorSlots(lambda[axis],canonical).Count==0;}
  var spinOnly=Product(Fifth.Spin(lambda[axis]),Caa.Gamma1,'C');var formOnly=Fifth.Covector(lambda[axis],Caa.Gamma1);
  if(axis is 0 or 7 or 8 or 9)foreach(var omission in new[]{spinOnly,formOnly}){Inc("omittedSlotDecoys");decoysPassed&=omission.Count>0;}
  normizuRows.Add(new{axis,matrix=Fifth.MatrixText(lambda[axis]),spin=Terms(Fifth.Spin(lambda[axis])),pairSum=pairSum.ToString(),spinOnlyCanonicalDerivative=Terms(spinOnly),covectorOnlyCanonicalDerivative=Terms(formOnly)});
 }
 FT[] directions=[Fifth.Diagonal(1,0,0),Fifth.Diagonal(0,1,0),Fifth.Diagonal(0,0,1),j,b,c,One(8,157,1),One(1,0,Scalar.I)];
 Rational[] norms=[-4,-9,-1,9,-1,-9,1,1];string[] directionIds=["PHGamma","PEGamma","PtGamma","J","B","C","W5","central"];
 var directionRows=new List<object>();for(int u=0;u<8;u++){Inc("carrierNormRows");inputPassed&=Pair(directions[u],directions[u])==norms[u]&&Fifth.RealType(directions[u],1);for(int v=u+1;v<8;v++){Inc("carrierOrthogonalityRows");inputPassed&=Pair(directions[u],directions[v])==0;}directionRows.Add(new{id=directionIds[u],tensor=Terms(directions[u]),signedNorm=Pair(directions[u],directions[u]).ToString()});}
 var isotropyRows=new List<object>();foreach(var generator in r618.GetProperty("isotropyRows").EnumerateArray()){var matrix=Fifth.Matrix(generator.GetProperty("frameGenerator"));var tensors=new List<object>();for(int k=0;k<6;k++){var image=Fifth.Derivative(matrix,directions[k]);Inc("carrierIsotropyRows");inputPassed&=image.Count==0;tensors.Add(new{id=directionIds[k],image=Terms(image)});}isotropyRows.Add(new{i=generator.GetProperty("i").GetInt32(),k=generator.GetProperty("k").GetInt32(),tensors});}
 for(int k=0;k<6;k++){Inc("carrierReflectionRows");inputPassed&=Equal(Fifth.Reflect(directions[k]),Scale(directions[k],k is 4 or 5?-1:1));}
 foreach(var canonical in new[]{Caa.Gamma1,Caa.Gamma2}){Inc("canonicalReflectionRows");inputPassed&=Equal(Fifth.Reflect(canonical),canonical);}
 Inc("cyclicCarrierRows");inputPassed&=new[]{j,b,c}.All(t=>Feedback.Cyclic(t).Count==0);
 Inc("signedMetricDecoys");decoysPassed&=Pair(directions[0],directions[0])==-4&&Adjoint.CoefficientPair(directions[0],directions[0])==4;
 var kineticRows=new List<object>();var feedbackRows=new List<object>();var feedbacks=new List<(string Id,Bilinear Value,FT Target)>();var hResults=new Dictionary<(int N,int G),Kinetic>();
 Kinetic H(int n,int power,FT input,FT target)
 {
  var result=Fifth.KineticOperator(lambda,input);Inc("kineticRows");kineticPassed&=Equal(result.Full,target);
  for(int axis=0;axis<14;axis++){Inc("derivativeComparisons");kineticPassed&=Equal(result.Derivatives[axis],result.Alternate[axis]);Inc("reverseComparisons");kineticPassed&=Equal(result.ReverseDerivatives[axis],result.AlternateReverse[axis]);}
  for(int stage=0;stage<8;stage++){Inc("kineticStageComparisons");kineticPassed&=Equal(result.Stages[stage],result.NaiveStages[stage]);}
  Inc("kineticAdjointChecks");kineticPassed&=Equal(result.Adjoint,Caa.AdjointSimplified(input));
  Inc("kineticTypeRows");kineticPassed&=Fifth.RealType(result.Full,1)&&Fifth.RealType(result.Adjoint,2)&&result.Derivatives.All(t=>Fifth.RealType(t,1))&&result.ReverseDerivatives.All(t=>Fifth.RealType(t,2));
  kineticRows.Add(new{inputOrder=n,gammaPower=power,evidence=result.Evidence(),expected=Terms(target)});hResults[(n,power)]=result;return result;
 }
 Bilinear Bcal(string id,FT x,FT z,FT target)
 {
  var result=Fifth.Polarize(x,z);Inc("feedbackRows");Inc("feedbackProductComparisons");polarizationPassed&=Equal(result.Q,result.NaiveQ);
  foreach(var (field,kad) in new[]{(x,result.AdjointX),(z,result.AdjointY)}){Inc("feedbackAdjointChecks");polarizationPassed&=Equal(kad,Caa.AdjointSimplified(field));}
  for(int stage=0;stage<8;stage++){Inc("feedbackStageComparisons");polarizationPassed&=Equal(result.Stages[stage],result.NaiveStages[stage]);}
  Inc("feedbackOracleChecks");polarizationPassed&=Equal(result.Full,target)&&Fifth.RealType(result.Full,1);
  var reflectedK=Caa.Forward(Fifth.Reflect(result.Q));Inc("feedbackKReflectionChecks");polarizationPassed&=Equal(reflectedK,Fifth.Reflect(result.Stages[7]));
  var reflectedAdjoints=new[]{Caa.AdjointLiteral(Fifth.Reflect(x)),Caa.AdjointLiteral(Fifth.Reflect(z))};for(int k=0;k<2;k++){Inc("feedbackAdjointReflectionChecks");polarizationPassed&=Equal(reflectedAdjoints[k],Fifth.Reflect(k==0?result.AdjointX:result.AdjointY));}
  feedbackRows.Add(new{id,evidence=result.Evidence(),expected=Terms(target),reflectedForward=Terms(reflectedK),reflectedAdjoints=reflectedAdjoints.Select(Terms).ToArray()});feedbacks.Add((id,result,target));return result;
 }
 Rational[] basisCoefficients=[new Rational(-4,3),new Rational(-22,3),new Rational(-2,3)];
 for(int k=0;k<3;k++)
 {
  var result=Bcal("basis"+k,directions[k],j,Scale(j,new Scalar(basisCoefficients[k],0)));Rational aa=k==0?1:0,bb=k==1?1:0,cc=k==2?1:0;
  foreach(bool pass in new[]{Equal(result.Q,Scale(y,new Scalar(bb*new Rational(1,2),0))),Equal(result.Stages[7],Scale(j,new Scalar(-2*bb,0))),Equal(result.DqX,Scale(j,new Scalar(-4*bb,0))),Equal(result.DqY,Scale(j,new Scalar(-4*(2*aa+9*bb+cc),0)))}){Inc("mixedLiteralLegRows");polarizationPassed&=pass;}
  Inc("polarizationFactorDecoys");decoysPassed&=!Equal(result.Full,Scale(j,new Scalar(2*basisCoefficients[k],0)));
 }
 var s=Enumerable.Range(0,6).Select(_=>Fifth.Zeros(3)).ToArray();var forecast=Enumerable.Range(0,6).Select(_=>Fifth.Zeros(3)).ToArray();
 var na=Fifth.Diagonal(new Rational(11043,2),new Rational(11655,2),new Rational(11043,2));
 forecast[1][0]=Fifth.Diagonal(new Rational(21,4),new Rational(15,4),new Rational(21,4));forecast[2][0]=Scale(j,new Scalar(new Rational(3,2),0));
 forecast[3][0]=Fifth.Diagonal(new Rational(-27,8),new Rational(3,2),0);forecast[3][1]=Scale(na,-1);
 forecast[4][0]=Add(Scale(j,new Scalar(new Rational(-39,8),0)),Scale(l,new Scalar(new Rational(-27,8),0)));forecast[4][1]=Scale(j,420);
 forecast[5][0]=Fifth.Diagonal(new Rational(1161,32),new Rational(51,2),new Rational(135,4));forecast[5][1]=Add(Fifth.Diagonal(-1701,new Rational(1275,2),new Rational(-189,2)),Scale(nj5,new Scalar(new Rational(-9,4),0)));forecast[5][2]=Fifth.Diagonal(15072318,15422130,15072318);
 s[1][0]=Scale(a,-1);
 var h10=H(1,0,s[1][0],Scale(j,new Scalar(new Rational(-3,2),0)));s[2][0]=Scale(h10.Full,-1);
 var h20=H(2,0,s[2][0],Fifth.Diagonal(new Rational(27,8),new Rational(-3,2),0));var n11=Bcal("N11",s[1][0],s[1][0],na);
 s[3][0]=Scale(h20.Full,-1);s[3][1]=Scale(n11.Full,-1);
 var h30=H(3,0,s[3][0],Add(Scale(j,new Scalar(new Rational(39,8),0)),Scale(l,new Scalar(new Rational(27,8),0))));var h31=H(3,1,s[3][1],Scale(j,-306));
 var b12=Bcal("B12",s[1][0],s[2][0],Scale(j,-57));s[4][0]=Scale(h30.Full,-1);s[4][1]=Scale(Add(h31.Full,Scale(b12.Full,2)),-1);
 var h40=H(4,0,s[4][0],Fifth.Diagonal(new Rational(-1161,32),new Rational(-51,2),new Rational(-135,4)));var h41=H(4,1,s[4][1],Fifth.Diagonal(945,-420,0));
 var b130=Bcal("B130",s[1][0],s[3][0],Fifth.Diagonal(new Rational(2997,8),-117,new Rational(81,2)));var b131=Bcal("B131",s[1][0],s[3][1],Fifth.Diagonal(-7536159,-7711065,-7536159));var n22=Bcal("N22",s[2][0],s[2][0],Scale(nj,new Scalar(new Rational(9,4),0)));
 s[5][0]=Scale(h40.Full,-1);s[5][1]=Scale(Add(h41.Full,Add(Scale(b130.Full,2),n22.Full)),-1);s[5][2]=Scale(b131.Full,-2);
 var coefficientRows=new List<object>();for(int n=1;n<=5;n++)for(int power=0;power<3;power++){Inc("seriesCoefficientRows");recursionPassed&=Equal(s[n][power],forecast[n][power]);Inc("seriesGradeRows");recursionPassed&=Feedback.Grades(s[n][power],n is 2 or 4?[2]:n==5&&power==1?[1,5]:[1]);coefficientRows.Add(new{order=n,gammaPower=power,actual=Terms(s[n][power]),expected=Terms(forecast[n][power])});}
 // Independently assemble ORDERED full Q and DQ-adjoint convolutions; no
 // scalar restriction or multiplication of cached B results constructs them.
 var nonlinear=new Dictionary<(int Order,int Power),FT>();var convolutionRows=new List<object>();
 foreach(var (order,power) in new[]{(2,0),(3,0),(4,0),(4,1)})
 {
  var q=new FT();var naiveQ=new FT();var dq=new FT();var summands=new List<object>();
  for(int left=1;left<order;left++)for(int p=0;p<=power;p++)
  {
   int right=order-left;if((left<3&&p>0)||(right<3&&power-p>0))continue;
   var x=s[left][p];var z=s[right][power-p];var product=Product(x,z);var naiveProduct=NaiveProduct(x,z,'W');var kad=Caa.AdjointLiteral(z);var adjoint=Adjoint.DQAdjoint(x,kad);
   Inc("convolutionOrderedTerms");q=Add(q,product);naiveQ=Add(naiveQ,naiveProduct);dq=Add(dq,adjoint);summands.Add(new{left,leftPower=p,right,rightPower=power-p,product=Terms(product),naiveProduct=Terms(naiveProduct),rightAdjoint=Terms(kad),adjointComposite=Terms(adjoint)});
  }
  var stages=Caa.ForwardStages(q);var naiveStages=Caa.ForwardStages(q,true);for(int k=0;k<8;k++){Inc("convolutionStageComparisons");recursionPassed&=Equal(stages[k],naiveStages[k]);}
  var value=Scale(Add(stages[7],dq),new Scalar(new Rational(1,3),0));var target=(order,power) switch{(2,0)=>n11.Full,(3,0)=>Scale(b12.Full,2),(4,0)=>Add(Scale(b130.Full,2),n22.Full),_=>Scale(b131.Full,2)};
  Inc("convolutionRows");recursionPassed&=Equal(q,naiveQ)&&Equal(value,target);nonlinear[(order,power)]=value;convolutionRows.Add(new{order,gammaPower=power,summands,Q=Terms(q),naiveQ=Terms(naiveQ),adjointComposite=Terms(dq),stages=stages.Select(Terms).ToArray(),naiveStages=naiveStages.Select(Terms).ToArray(),full=Terms(value),expected=Terms(target)});
 }
 var residualRows=new List<object>();for(int order=0;order<=4;order++)for(int power=0;power<3;power++)
 {
  var residual=new FT(s[order+1][power]);if(order==0&&power==0)residual=Add(residual,a);if(hResults.TryGetValue((order,power),out var kinetic))residual=Add(residual,kinetic.Full);if(power>0&&nonlinear.TryGetValue((order,power-1),out var feedback))residual=Add(residual,feedback);
  Inc("originalResidualCoefficients");recursionPassed&=residual.Count==0;residualRows.Add(new{originalLambdaPower=order,gammaPower=power,residual=Terms(residual)});
 }
 var completeFive=Feedback.Grade(s[5][1],5);Inc("fullGradeFiveRows");recursionPassed&=completeFive.Count==600&&Equal(completeFive,Scale(nj5,new Scalar(new Rational(-9,4),0)))&&completeFive.GetValueOrDefault((8,157,0,0))==new Scalar(new Rational(-3,4),0);
 var evaluationRows=new List<object>();var projectionRows=new List<object>();foreach(int gamma in new[]{-2,-1,0,1,2})
 {
  for(int n=1;n<=5;n++){var evaluated=Fifth.Evaluate(s[n],gamma);Inc("gammaEvaluationRows");recursionPassed&=Equal(evaluated,Fifth.Evaluate(forecast[n],gamma));evaluationRows.Add(new{gamma,order=n,tensor=Terms(evaluated)});}
  if(gamma!=0){var full=Fifth.Evaluate(s[5],gamma);var projected=Feedback.Grade(full,1);var residual=Add(projected,Scale(full,-1));Inc("projectedFifthOrderDecoys");decoysPassed&=Feedback.Grade(residual,5).Count==600&&Pair(directions[6],residual)==new Rational(3*gamma,4)&&!Equal(projected,full);projectionRows.Add(new{gamma,projectedS5=Terms(projected),originalOrderFourResidual=Terms(residual),witnessDerivative=Pair(directions[6],residual).ToString()});}
 }
 // Coefficient of r*s*t in the ORIGINAL cubic P(rX+sY+tU).
 // It equals2 Pair(U,Bcal(X,Y)); both varying occurrences of Q are kept.
 var actionRows=new List<object>();foreach(var (id,result,target) in feedbacks)for(int direction=0;direction<8;direction++)
 {
  var u=directions[direction];var qUy=Fifth.CrossQ(u,result.Y);var qUx=Fifth.CrossQ(u,result.X);var naiveQUy=Add(NaiveProduct(u,result.Y,'W'),NaiveProduct(result.Y,u,'W'));var naiveQUx=Add(NaiveProduct(u,result.X,'W'),NaiveProduct(result.X,u,'W'));foreach(var pair in new[]{(qUy,naiveQUy),(qUx,naiveQUx)}){Inc("actionQuadraticComparisons");actionPassed&=Equal(pair.Item1,pair.Item2);}var uy=Caa.ForwardStages(qUy);var ux=Caa.ForwardStages(qUx);var uyNaive=Caa.ForwardStages(qUy,true);var uxNaive=Caa.ForwardStages(qUx,true);
  for(int stage=0;stage<8;stage++)foreach(var pair in new[]{(uy[stage],uyNaive[stage]),(ux[stage],uxNaive[stage])}){Inc("actionStageComparisons");actionPassed&=Equal(pair.Item1,pair.Item2);}
  var first=2*Pair(u,result.Stages[7]);var second=Pair(result.X,uy[7]);var third=Pair(result.Y,ux[7]);var derivative=(first+second+third)*new Rational(1,3);var gradient=2*Pair(u,result.Full);var oracle=2*Pair(u,target);
  Inc("originalCubicDerivativeRows");actionPassed&=derivative==gradient&&gradient==oracle;
  Inc("potentialTrilinearityRows");actionPassed&=Pair(u,result.Full)==(first+second+third)*new Rational(1,6);
  actionRows.Add(new{feedbackId=id,directionId=directionIds[direction],Q_UY=Terms(qUy),Q_UX=Terms(qUx),naiveQ_UY=Terms(naiveQUy),naiveQ_UX=Terms(naiveQUx),UYStages=uy.Select(Terms).ToArray(),UXStages=ux.Select(Terms).ToArray(),UYNaiveStages=uyNaive.Select(Terms).ToArray(),UXNaiveStages=uxNaive.Select(Terms).ToArray(),first=first.ToString(),second=second.ToString(),third=third.ToString(),originalCubicCoefficient=derivative.ToString(),gradientCoefficient=gradient.ToString(),expected=oracle.ToString()});
 }
 if(point==1)for(int n=1;n<=5;n++)for(int p=0;p<3;p++){Inc("transportCoefficientRows");recursionPassed&=Equal(s[n][p],previous![n][p]);}previous=s;
 frequencyPassed&=s.SelectMany(x=>x).Concat(feedbacks.Select(q=>q.Value.Full)).All(Fifth.Constant);
 var evidence=new{schemaVersion=1,point,frame=r618.GetProperty("frame").Clone(),nomizuRows=normizuRows,source=Terms(a),J=Terms(j),N_J=Terms(nj),N_J_gradeFive=Terms(nj5),KdagJ=Terms(y),directions=directionRows,isotropyRows,kineticRows,feedbackRows,coefficientRows,convolutionRows,residualRows,completeS5GradeFive=Terms(completeFive),evaluationRows,projectionRows,actionRows};
 var shard=SavePoint(point,evidence,aggregateBytes);shards.Add(shard);aggregateBytes+=shard.bytes;maxShardBytes=Math.Max(maxShardBytes,shard.bytes);Inc("evidenceShards");
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());bool shardSetPassed=ShardPaths().SequenceEqual(shards.Select(q=>q.path).Order(StringComparer.Ordinal));var resources=fx.GetProperty("resources");
bool resourcesPassed=frequencyPassed&&shardSetPassed&&CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&Fifth.SlotProducts<=resources.GetProperty("maximumTrackedSlotProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumTensorTerms").GetInt32();
bool controlsPassed=knownAnswerPassed&&inputPassed&&kineticPassed&&polarizationPassed&&recursionPassed&&actionPassed&&decoysPassed&&countsPassed&&resourcesPassed;
string verdict=!inputPassed?precedence[2]:!kineticPassed?precedence[3]:!polarizationPassed?precedence[4]:!recursionPassed?precedence[5]:!actionPassed?precedence[6]:!decoysPassed?precedence[7]:!countsPassed||!resourcesPassed?precedence[8]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,inputPassed,kineticPassed,polarizationPassed,recursionPassed,actionPassed,decoysPassed,countsPassed,resourcesPassed,frequencyPassed,shardSetPassed,counts,trackedCoefficientProducts=CoefficientProducts,trackedSlotProducts=Fifth.SlotProducts,largestTensor=LargestTensor,shards,aggregateShardBytes=aggregateBytes,maximumShardBytes=maxShardBytes,storageSchema="two-complete-expanded-point-shards-v1",scope=new{fullS1ThroughS5Computed=true,fullOriginalResidualThroughLambdaFourComputed=true,fullNonlinearGradeFiveRetained=true,analyticBranchConsequenceConditional=true,sourceOperatorSelected=false,physicalCouplingSelected=false,physicalVacuumSelected=false,physicalSpectrumClaimed=false,pointwiseKineticSelfAdjointnessAssumed=false}});
}
catch(ResourceFailure ex)
{
 string verdict=!inputPassed?precedence[2]:!kineticPassed?precedence[3]:!polarizationPassed?precedence[4]:!recursionPassed?precedence[5]:!actionPassed?precedence[6]:!decoysPassed?precedence[7]:precedence[8];
 Emit(verdict,new{knownAnswerPassed,controlsPassed=false,inputPassed,kineticPassed,polarizationPassed,recursionPassed,actionPassed,decoysPassed,resourceFailure=ex.Message,counts,shards,aggregateShardBytes=aggregateBytes,maximumShardBytes=maxShardBytes,trackedCoefficientProducts=CoefficientProducts,trackedSlotProducts=Fifth.SlotProducts,largestTensor=LargestTensor});
}

static Rational Abs(Rational q)=>q.Numerator.Sign<0?q*-1:q;
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
static string[] ShardPaths()=>Directory.Exists(ShardDirectory)?Directory.EnumerateFiles(ShardDirectory,"*",SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToArray():[];
static byte[] Encode(object value)
{byte[] content=JsonSerializer.SerializeToUtf8Bytes(value,new JsonSerializerOptions{WriteIndented=true});var bytes=new byte[content.Length+1];content.CopyTo(bytes,0);bytes[^1]=10;return bytes;}
static void CheckTensorRecords(JsonElement e)
{
 if(e.ValueKind==JsonValueKind.String){string text=e.GetString()!;if(text.Length>0&&text.All(ch=>char.IsAsciiDigit(ch)||ch is '-' or '/')){if(text.Length>64)throw new ResourceFailure("rational text exceeds64 characters");if(Feedback.Parse(text).ToString()!=text)throw new ArgumentException("noncanonical rational");}return;}
 if(e.ValueKind==JsonValueKind.Array)
 {
  if(e.GetArrayLength()>0&&e[0].ValueKind==JsonValueKind.Object&&e[0].TryGetProperty("form",out _))
  {var tensor=Feedback.Read(e);var keys=e.EnumerateArray().Select(q=>(q.GetProperty("form").GetInt32(),q.GetProperty("blade").GetInt32(),q.GetProperty("k0").GetInt32(),q.GetProperty("k1").GetInt32())).ToArray();if(!keys.SequenceEqual(keys.Order())||tensor.Count!=keys.Length)throw new ArgumentException("noncanonical tensor ordering");}
  foreach(var item in e.EnumerateArray())CheckTensorRecords(item);return;
 }
 if(e.ValueKind!=JsonValueKind.Object)return;
 if(e.TryGetProperty("form",out var form)&&e.TryGetProperty("blade",out var blade)&&e.TryGetProperty("real",out var real))
 {
  if(form.GetInt32()<0||form.GetInt32()>Full||blade.GetInt32()<0||blade.GetInt32()>Full||e.GetProperty("k0").GetInt32()!=0||e.GetProperty("k1").GetInt32()!=0)throw new ArgumentException("invalid retained tensor indices");
  string rs=real.GetString()!,im=e.GetProperty("imaginary").GetString()!;if(rs.Length>64||im.Length>64)throw new ResourceFailure("rational text exceeds64 characters");
  if(Feedback.Parse(rs).ToString()!=rs||Feedback.Parse(im).ToString()!=im||(rs=="0"&&im=="0"))throw new ArgumentException("noncanonical retained coefficient");
 }
 foreach(var property in e.EnumerateObject())CheckTensorRecords(property.Value);
}
static Shard SavePoint(int point,object evidence,long aggregate)
{
 byte[] bytes=Encode(evidence);if(bytes.LongLength>67108864||aggregate+bytes.LongLength>134217728)throw new ResourceFailure("point/aggregate serialized byte ceiling");
 using var doc=JsonDocument.Parse(bytes);CheckTensorRecords(doc.RootElement);
 string path=ShardDirectory+"/point"+point+".json";Directory.CreateDirectory(ShardDirectory);File.WriteAllBytes(path,bytes);return new(point,path,Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),bytes.LongLength);
}
void VerifyEvidence()
{
 byte[] summaryBytes=File.ReadAllBytes(SummaryPath);if(summaryBytes.Length==0||summaryBytes[^1]!=10||(summaryBytes.Length>1&&summaryBytes[^2] is 10 or 13))throw new InvalidOperationException("manifest EOF");if(!summaryBytes.SequenceEqual(File.ReadAllBytes(FullPath)))throw new InvalidOperationException("full/summary mismatch");
 using var sd=JsonDocument.Parse(summaryBytes);var root=sd.RootElement;var e=root.GetProperty("evidence");
 if(!root.GetProperty("auditPassed").GetBoolean()||root.GetProperty("contractSha256").GetString()!=Sha(ContractPath)||root.GetProperty("verdictKind").GetString()!=Success||!e.GetProperty("knownAnswerPassed").GetBoolean()||!e.GetProperty("controlsPassed").GetBoolean()||e.GetProperty("storageSchema").GetString()!="two-complete-expanded-point-shards-v1")throw new InvalidOperationException("manifest did not pass");
 if(root.GetProperty("phase").GetInt32()!=623||!root.GetProperty("contractValid").GetBoolean()||!root.GetProperty("exactBindingsValid").GetBoolean()||!root.GetProperty("coreSourceTreeValid").GetBoolean()||!root.GetProperty("compiledSourceClosureValid").GetBoolean()||root.GetProperty("compiledSourceCount").GetInt32()!=7||!root.GetProperty("compiledSources").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(compiledSources)||!root.GetProperty("externalReviewPending").GetBoolean()||root.GetProperty("promotedPhysicalMassClaimCount").GetInt32()!=0||root.GetProperty("authorityFirewalls").EnumerateObject().Count()!=14||!firewalls.All(k=>root.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False))throw new InvalidOperationException("manifest provenance/firewalls");
 var retainedBindings=root.GetProperty("bindings").EnumerateArray().ToArray();if(retainedBindings.Length!=bindings.Length||retainedBindings.Select(q=>q.GetProperty("id").GetString()).Distinct().Count()!=bindings.Length||!retainedBindings.All(q=>bindings.Any(b=>b.id==q.GetProperty("id").GetString()&&b.path==q.GetProperty("path").GetString()&&b.sha256==q.GetProperty("sha256").GetString())&&q.GetProperty("hashMatches").GetBoolean()))throw new InvalidOperationException("manifest binding closure");
 var rows=e.GetProperty("shards").EnumerateArray().ToArray();var expectedPaths=new[]{ShardDirectory+"/point0.json",ShardDirectory+"/point1.json"};
 if(rows.Length!=2||!rows.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(expectedPaths)||!ShardPaths().SequenceEqual(expectedPaths))throw new InvalidOperationException("exact two shard path set");
 long bytesSum=0,maxBytes=0;for(int point=0;point<2;point++)
 {
  var row=rows[point];string path=expectedPaths[point];byte[] bytes=File.ReadAllBytes(path);if(bytes.Length==0||bytes[^1]!=10||(bytes.Length>1&&bytes[^2] is 10 or 13)||bytes.LongLength>67108864||row.GetProperty("point").GetInt32()!=point||row.GetProperty("bytes").GetInt64()!=bytes.LongLength||row.GetProperty("sha256").GetString()!=Sha(path))throw new InvalidOperationException("shard hash/size/EOF");
  using var pd=JsonDocument.Parse(bytes);var p=pd.RootElement;CheckTensorRecords(p);
  if(p.GetProperty("schemaVersion").GetInt32()!=1||p.GetProperty("point").GetInt32()!=point)throw new InvalidOperationException("point metadata");
  foreach(var (key,count) in new[]{("nomizuRows",14),("directions",8),("isotropyRows",6),("kineticRows",6),("feedbackRows",8),("coefficientRows",15),("convolutionRows",4),("residualRows",15),("evaluationRows",25),("projectionRows",4),("actionRows",64)})if(p.GetProperty(key).GetArrayLength()!=count)throw new InvalidOperationException("point census "+key);
  foreach(var kinetic in p.GetProperty("kineticRows").EnumerateArray()){var k=kinetic.GetProperty("evidence");foreach(var key in new[]{"derivatives","alternateDerivatives","reverseDerivatives","alternateReverse"})if(k.GetProperty(key).GetArrayLength()!=14)throw new InvalidOperationException("derivative census");foreach(var key in new[]{"stages","naiveStages"})if(k.GetProperty(key).GetArrayLength()!=8)throw new InvalidOperationException("kinetic stage census");}
  foreach(var f in p.GetProperty("feedbackRows").EnumerateArray())foreach(var key in new[]{"stages","naiveStages"})if(f.GetProperty("evidence").GetProperty(key).GetArrayLength()!=8)throw new InvalidOperationException("feedback stage census");
  foreach(var a in p.GetProperty("actionRows").EnumerateArray())foreach(var key in new[]{"UYStages","UXStages","UYNaiveStages","UXNaiveStages"})if(a.GetProperty(key).GetArrayLength()!=8)throw new InvalidOperationException("action stage census");
  if(p.GetProperty("completeS5GradeFive").GetArrayLength()!=600)throw new InvalidOperationException("complete grade5 census");
  bytesSum+=bytes.LongLength;maxBytes=Math.Max(maxBytes,bytes.LongLength);
 }
 if(bytesSum>134217728||bytesSum!=e.GetProperty("aggregateShardBytes").GetInt64()||maxBytes!=e.GetProperty("maximumShardBytes").GetInt64())throw new InvalidOperationException("aggregate byte count");
 foreach(var q in contract.GetProperty("fixtures").GetProperty("expectedCounts").EnumerateObject())if(e.GetProperty("counts").GetProperty(q.Name).GetInt32()!=q.Value.GetInt32())throw new InvalidOperationException("manifest census");
 Console.WriteLine("Phase623 read-only evidence verified:2 complete expanded point shards,12 kinetic rows,16 feedback rows,128 original action derivatives.");
}
void Emit(string terminal,object evidence)
{
 if(replayMode)throw new InvalidOperationException("read-only verification preflight failed: "+terminal);
 var result=new{schemaVersion=1,phase=623,phaseId="phase623-full-inverse-kappa-fifth-order-feedback-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,compiledSourceClosureValid,compiledSourceCount=compiledSources.Length,compiledSources,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 byte[] bytes=Encode(result);Directory.CreateDirectory(Root+"/output");File.WriteAllBytes(FullPath,bytes);File.WriteAllBytes(SummaryPath,bytes);Console.WriteLine($"Phase623 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;
}
internal sealed record Shard(int point,string path,string sha256,long bytes);
internal sealed class ResourceFailure(string message):Exception(message);
