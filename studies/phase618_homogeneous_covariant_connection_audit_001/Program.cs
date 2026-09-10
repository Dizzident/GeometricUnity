using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static Adjoint;
using static Caa;
using static ProjectorGradient;
using static Homogeneous;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase618_homogeneous_covariant_connection_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase618-a62-homogeneous-covariant-connection-v1";
const string Success="homogeneous-connection-controls-pass-conditional-local-existence";
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
 "metrics": [
  [
   -1,
   1,
   1,
   1
  ],
  [
   -1,
   4,
   9,
   16
  ]
 ],
 "alpha": "1",
 "beta": "-1/2",
 "sigma": "-1",
 "groupAction": "(x,y)->(Lx+t,L^-T y L^-1), local affine GL+(4) action",
 "reductiveLift": "(u,A)->(u,-y^-1A/2)",
 "nomizu": {
  "vertical": "Lambda_A=0",
  "horizontalHorizontal": "Lambda_u v=-sigma W(u,v)/2",
  "horizontalVertical": "Lambda_u A=y^-1Au/2",
  "movingFrame": "coordinate Koszul plus rho(-y^-1A/2)",
  "curvature": "[Lambda_X,Lambda_Y]-Lambda_[X,Y]_m-rho([X,Y]_h)"
 },
 "operator": "canonical untied CAA: firstC outerA innerA; Phi1=Gamma1 Phi2=Gamma2",
 "pairing": "signed exterior metric times -ReTr(XY)/128; compact-support formal adjoint",
 "fullRealAlgebra": "u(64,64), all16384 Clifford masks with H-antiHermitian real phase; includes central iI",
 "fullOneFormDimension": 229376,
 "nomizuHorizontalFrameAxes": [
  0,
  7,
  8,
  9
 ],
 "nomizuPlaneAbsoluteSum": "5/2 on four horizontal directions;0 on ten vertical directions",
 "source": "-(21/4)Gamma1+(3/2)PTGamma1",
 "sourceAuxiliaryL1": "60",
 "isotropy": "all6 y-Lorentz generators; full Nomizu covariance and tensor actions",
 "isotropyTensors": [
  "Gamma1",
  "Gamma2",
  "PTGamma1",
  "A=K(FB)",
  "J"
 ],
 "disconnectedRepresentative": "-I4, induces -I_horizontal and +I_vertical, upstairs orientation+1",
 "majorants": {
  "K": 2576,
  "KAdjoint": 2576,
  "derivative": 20,
  "H": 51520,
  "quadraticBilinear": 2576,
  "auxiliaryNormOnly": true
 },
 "contraction": {
  "gammas": [
   -2,
   -1,
   0,
   1,
   2
  ],
  "lambdaSigns": [
   -1,
   0,
   1
  ],
  "lambda": "sign/[4(60+51520+2576(|gamma|+1))]",
  "radius": "120|lambda| <=1",
  "sufficientConditions": [
   "|lambda|51520 <=1/4",
   "|gamma|2576*60*lambda^2 <=1/16"
  ],
  "imageBound": "|lambda|60+|lambda|51520*r+|lambda gamma|2576*r^2 <=r",
  "lipschitz": "|lambda|51520+2|lambda gamma|2576*r <=1/2",
  "zeroLambda": "formal base only, not an original finite-kappa solution",
  "physicalCouplingSelected": false
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "expectedCounts": {
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "fullRealDomainMasks": 16384,
  "centralDomainControls": 2,
  "planeMajorantControls": 1456,
  "universalMajorantControls": 3,
  "contexts": 2,
  "frameControls": 2,
  "nomizuEntries": 5488,
  "metricCompatibilityEntries": 5488,
  "omittedFrameMotionRows": 20,
  "torsionEntries": 5488,
  "curvatureEntries": 76832,
  "omittedIsotropyRows": 2,
  "wrongBracketSignRows": 2,
  "nomizuPlaneNormRows": 28,
  "projectorDerivativeEntries": 5488,
  "transportNomizuEntries": 2744,
  "canonicalParallelRows": 56,
  "derivativeMajorantRows": 2,
  "kineticStageComparisons": 16,
  "reverseDerivativeRows": 28,
  "kineticLegRows": 4,
  "fullJRows": 2,
  "transportJRows": 1,
  "sourceRows": 2,
  "sourceNormRows": 2,
  "lorentzGeneratorEntries": 192,
  "isotropyMetricEntries": 2352,
  "isotropyProjectorEntries": 2352,
  "isotropyNomizuEntries": 32928,
  "isotropySpinCommutators": 168,
  "isotropyTensorRows": 60,
  "isotropyLieEntries": 14112,
  "disconnectedOrientationRows": 2,
  "disconnectedMetricEntries": 392,
  "disconnectedProjectorEntries": 392,
  "disconnectedNomizuEntries": 5488,
  "disconnectedTensorRows": 10,
  "contractionCertificateRows": 15,
  "zeroLambdaBaseRows": 5,
  "nonzeroLambdaCertificateRows": 10,
  "contractionInequalityChecks": 30,
  "ballImageChecks": 15,
  "lipschitzChecks": 15
 },
 "resources": {
  "estimatedCpuSeconds": 120,
  "estimatedPeakBytes": 268435456,
  "maximumTrackedCoefficientProducts": 100000000,
  "maximumTrackedMatrixProducts": 200000000,
  "maximumTensorTerms": 32768,
  "maximumMatrixDimension": 14,
  "largestArrayEntries": 38416,
  "maximumFrequency": 0,
  "maximumRetainedContractionRows": 15
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormSelected": false,
  "sourceMetricSelected": false,
  "physicalCouplingSelected": false,
  "physicalVacuumSelected": false,
  "globalSpinorDescentClaimed": false,
  "finiteTotalActionClaimed": false,
  "fullMetricEulerComputed": false,
  "epsilonStationarityClaimed": false,
  "physicalSpectrumClaimed": false,
  "invariantScalarActionUsedAsFullGradient": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-domain-control-failed","homogeneous-connection-control-failed","full-curvature-control-failed","projector-kinetic-control-failed","full-isotropy-control-failed","conditional-existence-majorant-control-failed","omission-decoy-control-failed","resource-census-control-failed","homogeneous-connection-controls-pass-conditional-local-existence"];
var paths=new Dictionary<string,string>{
 ["program"]="studies/phase618_homogeneous_covariant_connection_audit_001/Program.cs",
 ["project"]="studies/phase618_homogeneous_covariant_connection_audit_001/Phase618HomogeneousCovariantConnectionAudit.csproj",
 ["study"]="studies/phase618_homogeneous_covariant_connection_audit_001/STUDY.md",
 ["homogeneous-helper"]="studies/phase618_homogeneous_covariant_connection_audit_001/HomogeneousConnection.cs",
 ["phase600-exactarithmetic-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/ExactArithmetic.cs",
 ["phase600-fouriertensor-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/FourierTensor.cs",
 ["phase600-traceadjoint-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/TraceAdjoint.cs",
 ["phase600-program"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/Program.cs",
 ["phase600-study"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/STUDY.md",
 ["phase600-contract"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/preregistration/contract_v1.json",
 ["phase600-summary"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",
 ["phase607-verticalgeometry-helper"]="studies/phase607_source_induced_vertical_curvature_audit_001/VerticalGeometry.cs",
 ["phase607-program"]="studies/phase607_source_induced_vertical_curvature_audit_001/Program.cs",
 ["phase607-study"]="studies/phase607_source_induced_vertical_curvature_audit_001/STUDY.md",
 ["phase607-contract"]="studies/phase607_source_induced_vertical_curvature_audit_001/preregistration/contract_v1.json",
 ["phase607-summary"]="studies/phase607_source_induced_vertical_curvature_audit_001/output/source_induced_vertical_curvature_audit_summary.json",
 ["phase608-ambientgeometry-helper"]="studies/phase608_source_induced_ambient_ricci_audit_001/AmbientGeometry.cs",
 ["phase608-program"]="studies/phase608_source_induced_ambient_ricci_audit_001/Program.cs",
 ["phase608-study"]="studies/phase608_source_induced_ambient_ricci_audit_001/STUDY.md",
 ["phase608-contract"]="studies/phase608_source_induced_ambient_ricci_audit_001/preregistration/contract_v1.json",
 ["phase608-summary"]="studies/phase608_source_induced_ambient_ricci_audit_001/output/source_induced_ambient_ricci_audit_summary.json",
 ["phase610-spingeometry-helper"]="studies/phase610_induced_spin_curvature_contraction_audit_001/SpinGeometry.cs",
 ["phase610-program"]="studies/phase610_induced_spin_curvature_contraction_audit_001/Program.cs",
 ["phase610-study"]="studies/phase610_induced_spin_curvature_contraction_audit_001/STUDY.md",
 ["phase610-contract"]="studies/phase610_induced_spin_curvature_contraction_audit_001/preregistration/contract_v1.json",
 ["phase610-summary"]="studies/phase610_induced_spin_curvature_contraction_audit_001/output/induced_spin_curvature_contraction_audit_summary.json",
 ["phase611-caaoperator-helper"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/CaaOperator.cs",
 ["phase611-program"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/Program.cs",
 ["phase611-study"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/STUDY.md",
 ["phase611-contract"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/preregistration/contract_v1.json",
 ["phase611-summary"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/output/untied_caa_response_joint_gauge_audit_summary.json",
 ["phase617-projectorgradient-helper"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/ProjectorGradient.cs",
 ["phase617-program"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/Program.cs",
 ["phase617-study"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/STUDY.md",
 ["phase617-contract"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/preregistration/contract_v1.json",
 ["phase617-summary"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/output/nonparallel_projector_two_weight_gradient_audit_summary.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["core-source-manifest"]="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json",
 ["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==618&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase610","induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source"),("phase611","untied-caa-response-controls-pass-source-choice-open"),("phase617","nonparallel-projector-controls-pass-two-weight-ansatz-nonstationary")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(9.1)","(9.4)","(3.27)","(3.34)","(12.27)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}


var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string k)=>counts[k]++;
bool knownAnswerPassed=true,connectionPassed=true,curvaturePassed=true,projectorPassed=true,isotropyPassed=true,majorantPassed=true,decoysPassed=true,frequencyPassed=true;
foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("arithmeticControls");knownAnswerPassed&=pass;}
for(int a=0;a<16384;a++)
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);
 Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);
 var domain=One(0,a,AdjointSign(a)==-1?1:Scalar.I);Inc("fullRealDomainMasks");knownAnswerPassed&=HAnti(domain)&&domain.Count==1;
}
foreach(bool pass in new[]{HAnti(One(0,0,Scalar.I)),Equal(Product(One(0,1,1),One(0,1,1),'A'),One(0,0,Scalar.I*2))}){Inc("centralDomainControls");knownAnswerPassed&=pass;}
for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)
{
 var l=new Matrix(14);l[a,b]=1;l[b,a]=-Sigma(a)*Sigma(b);
 foreach(int form in new[]{0,1<<a,1<<b,(1<<a)|(1<<b)})foreach(int blade in new[]{0,1<<a,1<<b,(1<<a)|(1<<b)})
 {var input=One(form,blade,AdjointSign(blade)==-1?1:Scalar.I);var actual=Action(l,input);Inc("planeMajorantControls");knownAnswerPassed&=Le(L1(actual),2)&&HAnti(actual);}
}
foreach(bool pass in new[]{28+28*182/2==2576,2576*20==51520,(2576+2*2576)/3==2576}){Inc("universalMajorantControls");majorantPassed&=pass;}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
using var geomDoc=JsonDocument.Parse(File.ReadAllBytes(paths["phase608-summary"]));using var projectorDoc=JsonDocument.Parse(File.ReadAllBytes(paths["phase617-summary"]));
var geomRows=geomDoc.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().ToArray();var projectorRows=projectorDoc.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().ToArray();
var rows=new List<object>();Matrix[]? previousLambda=null;FT? previousJ=null;
for(int point=0;point<2;point++)
{
 Inc("contexts");var y=point==0?Matrix.Diagonal(-1,1,1,1):Matrix.Diagonal(-1,4,9,16);var g=new Ambient(y,1,new Rational(-1,2),-1);var eta=SpinGeometry.Eta();var frame=SpinGeometry.Frame(point);var inverse=frame.Inverse();
 var old=geomRows.Single(q=>q.GetProperty("point").GetInt32()==point&&q.GetProperty("beta").GetString()=="-1/2");var oldP=projectorRows.Single(q=>q.GetProperty("point").GetInt32()==point);
 Inc("frameControls");connectionPassed&=(SpinGeometry.Transpose(frame)*g.Gram*frame).Same(eta)&&inverse.Same(SpinGeometry.HandInverse(point))&&frame.Determinant().Numerator.Sign>0&&frame.Same(SpinGeometry.ReadMatrix(oldP.GetProperty("frame")));
 var p=g.TracelessProjector();var pf=inverse*p*frame;var coordinateLambda=new Matrix[14];var typedLambda=new Matrix[14];var coordinateRows=new List<object>();
 for(int a=0;a<14;a++)
 {
  var gamma=ProjectorGradient.Connection(g,a);var motion=Rho(Lift(g,a));coordinateLambda[a]=gamma+motion;typedLambda[a]=LambdaTyped(g,a);
  var compatibility=SpinGeometry.Transpose(coordinateLambda[a])*g.Gram+g.Gram*coordinateLambda[a];
  for(int b=0;b<14;b++)for(int c=0;c<14;c++){Inc("nomizuEntries");connectionPassed&=coordinateLambda[a][b,c]==typedLambda[a][b,c];Inc("metricCompatibilityEntries");connectionPassed&=compatibility[b,c]==0;}
  if(a>=4){Inc("omittedFrameMotionRows");decoysPassed&=!gamma.Zero&&!motion.Zero&&coordinateLambda[a].Zero;}
  coordinateRows.Add(new{axis=a,coordinateConnection=gamma.Text(),frameMotion=motion.Text(),nomizu=coordinateLambda[a].Text(),typed=typedLambda[a].Text()});
 }
 var coordinateCurvature=new Rational[14,14,14,14];var upstreamCurvature=SpinGeometry.ReadCurvature(old.GetProperty("curvatureCoefficients"));
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)
 {
  var bracket=BracketM(g,a,b);var isotropy=Rho(BracketH(g,a,b));var commutator=Geometry.Commutator(coordinateLambda[a],coordinateLambda[b]);var horizontalTerm=Combine(coordinateLambda,bracket);var curvature=commutator-horizontalTerm-isotropy;
  for(int c=0;c<14;c++){Inc("torsionEntries");connectionPassed&=coordinateLambda[a][c,b]-coordinateLambda[b][c,a]==bracket[c];for(int d=0;d<14;d++){Inc("curvatureEntries");coordinateCurvature[a,b,c,d]=curvature[d,c];curvaturePassed&=curvature[d,c]==upstreamCurvature[a,b,c,d];}}
  if(a==4&&b==8){Inc("omittedIsotropyRows");decoysPassed&=!isotropy.Zero&&!(commutator-horizontalTerm).Same(curvature);}
  if(a==4&&b==0){Inc("wrongBracketSignRows");decoysPassed&=!horizontalTerm.Zero&&!(commutator+horizontalTerm-isotropy).Same(curvature);}
 }
 var lambda=FrameDerivatives(coordinateLambda,frame,inverse);var derivative=lambda.Select(l=>Geometry.Commutator(l,pf)).ToArray();var normRows=new List<object>();Rational representationSum=0;
 for(int a=0;a<14;a++)
 {
  var oldDerivative=SpinGeometry.ReadMatrix(oldP.GetProperty("frameDerivatives")[a]);Rational pairSum=0;for(int b=0;b<14;b++)for(int c=b+1;c<14;c++)pairSum+=Abs(lambda[a][b,c]);
  bool horizontal=a is 0 or 7 or 8 or 9;Inc("nomizuPlaneNormRows");majorantPassed&=pairSum==(horizontal?new Rational(5,2):0);representationSum+=2*pairSum;normRows.Add(new{axis=a,pairCoefficientSum=pairSum.ToString(),representationBound=(2*pairSum).ToString()});
  for(int b=0;b<14;b++)for(int c=0;c<14;c++){Inc("projectorDerivativeEntries");projectorPassed&=derivative[a][b,c]==oldDerivative[b,c];if(point==1){Inc("transportNomizuEntries");connectionPassed&=lambda[a][b,c]==previousLambda![a][b,c];}}
  foreach(var canonical in new[]{Gamma1,Gamma2}){Inc("canonicalParallelRows");connectionPassed&=Action(lambda[a],canonical).Count==0;}
 }
 Inc("derivativeMajorantRows");majorantPassed&=representationSum==20;
 var db=ExteriorDerivative(derivative);var stages=ForwardStages(db);var naive=ForwardStages(db,true);for(int k=0;k<8;k++){Inc("kineticStageComparisons");projectorPassed&=Equal(stages[k],naive[k]);}
 var reverse=new FT[14];var reverseRows=new List<object>();
 for(int a=0;a<14;a++){var field=SpinGeometry.WeightedGamma(derivative[a]);var legs=AdjointLegs(field);reverse[a]=Add(legs.First,legs.Second);Inc("reverseDerivativeRows");projectorPassed&=Equal(reverse[a],AdjointSimplified(field))&&ConstantRealType(reverse[a],2);reverseRows.Add(new{axis=a,first=Terms(legs.First),second=Terms(legs.Second),full=Terms(reverse[a])});}
 var adjointDivergence=Divergence(reverse);var j=Scale(Add(stages[7],adjointDivergence),Fourier.Half);var oldJ=ReadTerms(oldP.GetProperty("kinetic"));
 foreach(var leg in new[]{stages[7],adjointDivergence}){Inc("kineticLegRows");projectorPassed&=Equal(leg,oldJ)&&ConstantRealType(leg,1);}
 Inc("fullJRows");projectorPassed&=Equal(j,oldJ)&&Equal(j,OracleJ(derivative))&&j.GetValueOrDefault((1,3,0,0))==new Scalar(new Rational(-1,2),0);
 if(point==1){Inc("transportJRows");projectorPassed&=Equal(j,previousJ!);}
 var source=ReadTerms(oldP.GetProperty("sourceStages")[7]);var pField=SpinGeometry.WeightedGamma(pf);var sourceOracle=Add(Scale(Gamma1,new Scalar(new Rational(-21,4),0)),Scale(pField,new Scalar(new Rational(3,2),0)));
 Inc("sourceRows");projectorPassed&=Equal(source,sourceOracle)&&ConstantRealType(source,1);Inc("sourceNormRows");majorantPassed&=L1(source)==60;
 var invariantTensors=new[]{Gamma1,Gamma2,pField,source,j};var isotropyRows=new List<object>();var zs=new List<Matrix>();
 for(int i=0;i<4;i++)for(int k=i+1;k<4;k++)
 {
  var z=Lorentz(point,i,k);zs.Add(z);var skew=SpinGeometry.Transpose(z)*y+y*z;for(int b=0;b<4;b++)for(int c=0;c<4;c++){Inc("lorentzGeneratorEntries");isotropyPassed&=skew[b,c]==0;}
  var rho=inverse*Rho(z)*frame;var metricSkew=SpinGeometry.Transpose(rho)*eta+eta*rho;var projectorCommutator=Geometry.Commutator(rho,pf);
  for(int b=0;b<14;b++)for(int c=0;c<14;c++){Inc("isotropyMetricEntries");isotropyPassed&=metricSkew[b,c]==0;Inc("isotropyProjectorEntries");isotropyPassed&=projectorCommutator[b,c]==0;}
  var covarianceRows=new List<object>();for(int a=0;a<14;a++){var left=Geometry.Commutator(rho,lambda[a]);var right=Combine(lambda,Column(rho,a));for(int b=0;b<14;b++)for(int c=0;c<14;c++){Inc("isotropyNomizuEntries");isotropyPassed&=left[b,c]==right[b,c];}covarianceRows.Add(new{axis=a,commutator=left.Text(),argumentAction=right.Text()});}
  var spinGenerator=SpinGenerator(rho);for(int a=0;a<14;a++){var input=One(0,1<<a,1);var commutator=Product(spinGenerator,input,'C');var expectedVector=new FT();for(int b=0;b<14;b++)Put(expectedVector,(0,1<<b,0,0),new Scalar(rho[b,a],0));Inc("isotropySpinCommutators");isotropyPassed&=Equal(commutator,expectedVector)&&Equal(Action(rho,input),expectedVector);}
  var tensorRows=new List<object>();for(int t=0;t<5;t++){var action=Action(rho,invariantTensors[t]);Inc("isotropyTensorRows");isotropyPassed&=action.Count==0;tensorRows.Add(new{tensor=t,action=Terms(action)});}
  isotropyRows.Add(new{i,k,baseGenerator=z.Text(),frameGenerator=rho.Text(),spinGenerator=Terms(spinGenerator),covarianceRows,tensorRows});
 }
 foreach(var z in zs)foreach(var w in zs){var left=Rho(Geometry.Commutator(z,w));var right=Geometry.Commutator(Rho(z),Rho(w));for(int b=0;b<14;b++)for(int c=0;c<14;c++){Inc("isotropyLieEntries");isotropyPassed&=left[b,c]==right[b,c];}}
 var disconnected=DisconnectedMatrix();var disconnectedMetric=SpinGeometry.Transpose(disconnected)*eta*disconnected;var disconnectedProjector=disconnected*pf*disconnected;
 Inc("disconnectedOrientationRows");isotropyPassed&=disconnected.Determinant()==1;
 for(int b=0;b<14;b++)for(int c=0;c<14;c++){Inc("disconnectedMetricEntries");isotropyPassed&=disconnectedMetric[b,c]==eta[b,c];Inc("disconnectedProjectorEntries");isotropyPassed&=disconnectedProjector[b,c]==pf[b,c];}
 for(int a=0;a<14;a++){var left=disconnected*lambda[a]*disconnected;var right=Combine(lambda,Column(disconnected,a));for(int b=0;b<14;b++)for(int c=0;c<14;c++){Inc("disconnectedNomizuEntries");isotropyPassed&=left[b,c]==right[b,c];}}
 for(int t=0;t<5;t++){Inc("disconnectedTensorRows");isotropyPassed&=Equal(Disconnected(invariantTensors[t]),invariantTensors[t]);}
 frequencyPassed&=invariantTensors.Append(db).All(t=>t.Keys.All(q=>q.K0==0&&q.K1==0));
 rows.Add(new{point,frame=frame.Text(),coordinateRows,curvature=SpinGeometry.Nonzero(coordinateCurvature),frameNomizu=lambda.Select(m=>m.Text()).ToArray(),projector=pf.Text(),projectorDerivatives=derivative.Select(m=>m.Text()).ToArray(),normRows,representationSum=representationSum.ToString(),exteriorDerivative=Terms(db),kineticStages=stages.Select(Terms).ToArray(),reverseRows,adjointDivergence=Terms(adjointDivergence),J=Terms(j),source=Terms(source),sourceNorm=L1(source).ToString(),isotropyRows,disconnected=disconnected.Text()});
 previousLambda=lambda;previousJ=j;
}
var certificateRows=new List<object>();Rational sourceBound=60,hBound=51520,cBound=2576;
foreach(int gamma in new[]{-2,-1,0,1,2})foreach(int sign in new[]{-1,0,1})
{
 Rational lambda=new(sign,4*(60+51520+2576*(Math.Abs(gamma)+1)));var magnitude=Abs(lambda);var radius=120*magnitude;var first=magnitude*hBound;var second=Math.Abs(gamma)*cBound*sourceBound*magnitude*magnitude;
 var imageBound=magnitude*sourceBound+first*radius+magnitude*Math.Abs(gamma)*cBound*radius*radius;var lipschitz=first+2*magnitude*Math.Abs(gamma)*cBound*radius;
 Inc("contractionCertificateRows");Inc(sign==0?"zeroLambdaBaseRows":"nonzeroLambdaCertificateRows");Inc("contractionInequalityChecks");majorantPassed&=Le(first,new Rational(1,4));Inc("contractionInequalityChecks");majorantPassed&=Le(second,new Rational(1,16));
 Inc("ballImageChecks");majorantPassed&=Le(imageBound,radius)&&Le(radius,1);Inc("lipschitzChecks");majorantPassed&=Le(lipschitz,new Rational(1,2));
 certificateRows.Add(new{gamma,lambda=lambda.ToString(),formalBaseOnly=sign==0,radius=radius.ToString(),linearBound=first.ToString(),quadraticBound=second.ToString(),imageBound=imageBound.ToString(),lipschitz=lipschitz.ToString(),physicalCouplingSelected=false});
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");bool resourcesPassed=frequencyPassed&&CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&Matrix.Products<=resources.GetProperty("maximumTrackedMatrixProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumTensorTerms").GetInt32();
bool controlsPassed=knownAnswerPassed&&connectionPassed&&curvaturePassed&&projectorPassed&&isotropyPassed&&majorantPassed&&decoysPassed&&countsPassed&&resourcesPassed;
string verdict=!knownAnswerPassed?precedence[1]:!connectionPassed?precedence[2]:!curvaturePassed?precedence[3]:!projectorPassed?precedence[4]:!isotropyPassed?precedence[5]:!majorantPassed?precedence[6]:!decoysPassed?precedence[7]:!countsPassed||!resourcesPassed?precedence[8]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,connectionPassed,curvaturePassed,projectorPassed,isotropyPassed,majorantPassed,decoysPassed,countsPassed,resourcesPassed,frequencyPassed,counts,trackedCoefficientProducts=CoefficientProducts,trackedMatrixProducts=Matrix.Products,largestTensor=LargestTensor,rows,certificateRows,conditionalExistenceCertificate=new{fullRealAlgebraDimension=16384,ambientOneFormDimension=229376,invariantDimensionComputed=false,sourceBound="60",KBound="2576",KAdjointBound="2576",differentialBound="51520",quadraticBilinearBound="2576",auxiliaryCoefficientNormOnly=true,fullGradientRestricted=true,localInvariantConnectionExistenceConditional=true},scope=new{sourceOperatorSelected=false,sourceNormSelected=false,sourceMetricSelected=false,physicalCouplingSelected=false,physicalVacuumSelected=false,globalSpinorDescentClaimed=false,finiteTotalActionClaimed=false,fullMetricEulerComputed=false,epsilonStationarityClaimed=false,physicalSpectrumClaimed=false,invariantScalarActionUsedAsFullGradient=false}});
static Rational Abs(Rational r)=>r.Numerator.Sign<0?r*-1:r;
static Rational L1(FT t)=>t.Values.Aggregate((Rational)0,(sum,z)=>sum+Abs(z.Real)+Abs(z.Imaginary));
static bool Le(Rational a,Rational b)=>a.Numerator*b.Denominator<=b.Numerator*a.Denominator;

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=618,phaseId="phase618-homogeneous-covariant-connection-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/homogeneous_covariant_connection_audit.json",json);File.WriteAllText(Root+"/output/homogeneous_covariant_connection_audit_summary.json",json);Console.WriteLine($"Phase618 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
