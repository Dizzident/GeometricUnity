using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static SpinGeometry;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase616_curved_canonical_isotropic_gradient_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P607="studies/phase607_source_induced_vertical_curvature_audit_001";
const string P608="studies/phase608_source_induced_ambient_ricci_audit_001";
const string P610="studies/phase610_induced_spin_curvature_contraction_audit_001";
const string P611="studies/phase611_untied_caa_response_joint_gauge_audit_001";
const string P613="studies/phase613_canonical_isotropic_stationarity_potential_hessian_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase616-a61-curved-canonical-isotropic-gradient-v1";
const string Success="curved-canonical-isotropic-controls-pass-no-isotropic-branch";
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
 "upstreamBeta": "-1/2",
 "upstreamSigma": "-1",
 "points": [
  0,
  1
 ],
 "frameOrder": [
  "H0",
  "A1",
  "A2",
  "A3",
  "p1",
  "p2",
  "p3",
  "H1",
  "H2",
  "H3",
  "T",
  "n1",
  "n2",
  "n3"
 ],
 "diagonalRows": [
  [
   1,
   1,
   -1,
   -1
  ],
  [
   1,
   -1,
   1,
   -1
  ],
  [
   1,
   -1,
   -1,
   1
  ]
 ],
 "diagonalDenominator": 2,
 "trace": "-eta/2",
 "positiveOffDiagonal": "(3P+N)/4",
 "negativeOffDiagonal": "(P+3N)/4",
 "frameDeterminants": [
  "1/8",
  "41472"
 ],
 "inverse": "explicit Hadamard diagonal and paired inverse; E^-1=eta14 E^T G",
 "transport": "diag(L^-T,SymL), L=diag(1,2,3,4)",
 "curvatureRoutes": [
  "two-input endomorphism mixing then E^-1 R E",
  "coordinate Gram lowering then independent four covariant slots"
 ],
 "riemannConvention": "R592_abcd=-G(R(a,b)c,d)",
 "spin": "F_ab=1/2 sum_c<d R592_abcd sigma_c sigma_d gamma_c gamma_d",
 "commutator": "[F_ab,gamma_e]=gamma(R(a,b)e)",
 "knownPlanes": {
  "pairs": 91,
  "mixed": 49,
  "sameSign": 42,
  "vectorDirections": 14,
  "metricError": "omit sigma_c sigma_d",
  "factorError": "drop ordered-pair half",
  "signError": "use +L instead of -L"
 },
 "actualMixedAnchor": {
  "pair": [
   0,
   7
  ],
  "inputGamma": 7,
  "correct": "-gamma0/4",
  "wrongMetric": "gamma0/4",
  "wrongFactor": "-gamma0/2",
  "wrongSign": "gamma0/4"
 },
 "ricciTraceless": "-5/4",
 "ricciOther": "1/4",
 "scalar": "-10",
 "einsteinTraceless": "15/4",
 "einsteinOther": "21/4",
 "tracelessFrameAxes": [
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
 "wordMenu": "all 16384 blades; left/right by each14 singleton and Omega, plus square:31 selected products each",
 "exactTolerance": 0,
 "coreFileCount": 726,
 "expectedCounts": {
  "arithmeticControls": 8,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "planeCases": 91,
  "planeCommutators": 1274,
  "planeMetricMixedRejected": 49,
  "planeMetricSameAccepted": 42,
  "planeFactorRejected": 91,
  "planeSignRejected": 91,
  "contexts": 2,
  "frameMatrixEntries": 392,
  "inverseEntries": 392,
  "orientationControls": 2,
  "orientationDecoys": 2,
  "frameCurvatureEntries": 76832,
  "frameSymmetryEntries": 76832,
  "ricciEntries": 392,
  "einsteinEntries": 392,
  "pointTransportEntries": 38416,
  "spinCommutators": 2548,
  "spinWordAgreement": 2548,
  "liftTypeRows": 2,
  "actualMetricDecoys": 2,
  "actualFactorDecoys": 2,
  "actualSignDecoys": 2,
  "chainRows": 2,
  "chainStageComparisons": 16,
  "chainTypeChecks": 16,
  "chainRealityChecks": 16,
  "chainOracleRows": 10,
  "sourceDiagnosticRows": 2,
  "sourceDecoyRows": 6,
  "adjointRows": 392,
  "adjointLegPairings": 784,
  "parallelGenerators": 91,
  "parallelTensorRows": 273,
  "connectionOmissionDecoys": 546,
  "derivativeDirections": 1274,
  "kineticLegs": 2548,
  "parameterRows": 90,
  "potentialTensorChecks": 360,
  "gradientRows": 90,
  "gradientCoefficientEntries": 17640,
  "anisotropicWitnessRows": 90,
  "flatSourceDecoys": 90,
  "missingCubicAdjointDecoys": 48,
  "polynomialCoefficients": 40,
  "couplingClasses": 6,
  "sourceTransportRows": 1,
  "gradientTransportRows": 45
 },
 "resources": {
  "estimatedCpuSeconds": 90,
  "maximumEstimatedCpuSeconds": 240,
  "estimatedPeakBytes": 268435456,
  "maximumEstimatedPeakBytes": 805306368,
  "maximumTrackedMatrixProducts": 100000000,
  "maximumTrackedCoefficientProducts": 100000000,
  "maximumFourierTerms": 16384,
  "maximumMatrixDimension": 14,
  "largestScalarArrayEntries": 38416,
  "maximumPolynomialDegree": 3,
  "maximumPolynomialCoefficients": 20
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormalizationSelected": false,
  "sourceNormSelected": false,
  "physicalVacuumRejected": false,
  "metricEulerEquationComputed": false,
  "nonconstantFieldsExcluded": false,
  "anisotropicFieldsExcluded": false,
  "physicalSpectrumComputed": false
 },
 "operator": "canonical untied CAA: first C, outer A, inner A; Phi1=Gamma1, Phi2=Gamma2",
 "sourceFirst": "-RicciGamma",
 "sourceInner": "5i top",
 "sourceZero": "-5i",
 "sourceOuter": "10Gamma1",
 "sourceSecond": "-5Gamma1",
 "sourceFinal": "-EinsteinGamma",
 "sourceCoefficientSquare": "2115/8",
 "sourceSelfPair": "-2115/8",
 "sourceCrossPair": "60",
 "isotropicQ": "2s^2 Gamma2",
 "isotropicKQ": "312s^2 Gamma1",
 "isotropicAdjoint": "-24s Gamma2",
 "isotropicDqAdjoint": "624s^2 Gamma1",
 "fullGradient": "[(312gamma s^2+kappa s-21/4)I+(3/2)PT]Gamma1",
 "anisotropicDifference": "3/2",
 "sMenu": [
  -2,
  -1,
  0,
  1,
  2
 ],
 "gammaMenu": [
  0,
  1,
  2
 ],
 "kappaMenu": [
  -1,
  0,
  1
 ],
 "polynomialVariables": [
  "s",
  "gamma",
  "kappa"
 ],
 "polynomialDegree": 3,
 "polynomialMonomials": 20,
 "parallelism": "all91 so(7,7) generators; complete Clifford and covector actions; Gamma1,Gamma2,KdagGamma1",
 "derivativeDirections": [
  0,
  1,
  2,
  3,
  4,
  5,
  6,
  7,
  8,
  9,
  10,
  11,
  12,
  13
 ],
 "adjointProbes": "all196 constant vector one-form units, both individual forward/reverse legs",
 "decoys": [
  "omit curvature source",
  "replace Einstein by scalar average30/7",
  "omit second forward curvature leg",
  "omit DQadjoint cubic leg",
  "drop covector connection action",
  "drop Clifford connection action"
 ],
 "couplingClasses": [
  "gamma!=0,kappa arbitrary,s arbitrary",
  "gamma=0,kappa!=0,s arbitrary",
  "gamma=0,kappa=0,s arbitrary"
 ]
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","frame-curvature-control-failed","typed-spin-control-failed","literal-contraction-adjoint-control-failed","canonical-parallelism-control-failed","full-isotropic-gradient-control-failed","resource-census-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase616CurvedCanonicalIsotropicGradientAudit.csproj",["study"]=Root+"/STUDY.md",["parallel-helper"]=Root+"/ParallelControls.cs",
 ["phase608-summary"]=P608+"/output/source_induced_ambient_ricci_audit_summary.json",["phase608-contract"]=P608+"/preregistration/contract_v1.json",["phase608-program"]=P608+"/Program.cs",["phase608-helper"]=P608+"/AmbientGeometry.cs",["phase608-study"]=P608+"/STUDY.md",["phase608-project"]=P608+"/Phase608SourceInducedAmbientRicciAudit.csproj",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["geometry-helper"]=P607+"/VerticalGeometry.cs",["phase607-summary"]=P607+"/output/source_induced_vertical_curvature_audit_summary.json",["phase607-contract"]=P607+"/preregistration/contract_v1.json",
 ["spin-helper"]=P610+"/SpinGeometry.cs",["phase610-summary"]=P610+"/output/induced_spin_curvature_contraction_audit_summary.json",["phase610-contract"]=P610+"/preregistration/contract_v1.json",["phase610-program"]=P610+"/Program.cs",["phase610-study"]=P610+"/STUDY.md",["phase610-project"]=P610+"/Phase610InducedSpinCurvatureContractionAudit.csproj",
 ["caa-helper"]=P611+"/CaaOperator.cs",["phase611-summary"]=P611+"/output/untied_caa_response_joint_gauge_audit_summary.json",["phase611-contract"]=P611+"/preregistration/contract_v1.json",["phase611-program"]=P611+"/Program.cs",["phase611-study"]=P611+"/STUDY.md",["phase611-project"]=P611+"/Phase611UntiedCaaResponseJointGaugeAudit.csproj",
 ["phase613-summary"]=P613+"/output/canonical_isotropic_stationarity_potential_hessian_audit_summary.json",["phase613-contract"]=P613+"/preregistration/contract_v1.json",["phase613-study"]=P613+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==616&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase610","induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source"),("phase611","untied-caa-response-controls-pass-source-choice-open"),("phase613","canonical-isotropic-stationarity-controls-pass-potential-not-spectrum")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(9.1)","(9.4)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}


var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string k)=>counts[k]++;
bool knownAnswerPassed=true,framePassed=true,spinPassed=true,chainPassed=true,adjointPassed=true,parallelPassed=true,gradientPassed=true;
var eta4=Matrix.Diagonal(-1,1,1,1);var l=Matrix.Diagonal(1,2,3,4);var off=new Matrix(2);off[0,1]=1;off[1,0]=1;var singular=Matrix.Diagonal(1,0);var mixed=Matrix.Diagonal(2,-3);
bool[] arithmetic=[Parse("2/4")==new Rational(1,2),Parse("-3/-6")==new Rational(1,2),eta4.Inverse().Same(eta4),(l*l.Inverse()).Same(Matrix.Identity(4)),off.Inertia()==(1,1,0),singular.Inertia()==(1,0,1),mixed.Determinant()==-6,mixed.Inertia()==(1,1,0)];
foreach(bool pass in arithmetic){Inc("arithmeticControls");knownAnswerPassed&=pass;}
for(int a=0;a<16384;a++)
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}
 Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);Inc("hodgeCases");int degree=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((degree*(14-degree)+7)%2==0?1:-1);
}
for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)
{
 Inc("planeCases");var f=One(0,(1<<a)|(1<<b),Fourier.Half);bool mixedError=false,factorError=false,signError=false,metricSame=true;
 for(int e=0;e<14;e++)
 {
  var gamma=One(0,1<<e,1);FT target=new();if(e==b)target=One(0,1<<a,Sigma(b));if(e==a)target=One(0,1<<b,-Sigma(a));
  var actual=Product(f,gamma,'C');Inc("planeCommutators");knownAnswerPassed&=Equal(actual,target)&&Equal(actual,NaiveProduct(f,gamma,'C'));
  bool metricOK=Equal(Product(Scale(f,Sigma(a)*Sigma(b)),gamma,'C'),target);mixedError|=!metricOK;metricSame&=metricOK;factorError|=!Equal(Product(Scale(f,2),gamma,'C'),target);signError|=!Equal(Product(Scale(f,-1),gamma,'C'),target);
 }
 if(Sigma(a)!=Sigma(b)){Inc("planeMetricMixedRejected");knownAnswerPassed&=mixedError;}else{Inc("planeMetricSameAccepted");knownAnswerPassed&=metricSame;}
 Inc("planeFactorRejected");Inc("planeSignRejected");knownAnswerPassed&=factorError&&signError;
}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
JsonElement[] upstreamRows;
try
{
 using var input=JsonDocument.Parse(File.ReadAllBytes(paths["phase608-summary"]));upstreamRows=input.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().Where(v=>v.GetProperty("beta").GetString()=="-1/2").OrderBy(v=>v.GetProperty("point").GetInt32()).Select(v=>v.Clone()).ToArray();
 if(upstreamRows.Length!=2||upstreamRows.Where((v,i)=>v.GetProperty("point").GetInt32()!=i||v.GetProperty("sigma").GetString()!="-1"||!v.GetProperty("inertia").EnumerateArray().Select(x=>x.GetInt32()).SequenceEqual(new[]{7,7,0})).Any())throw new ArgumentException("upstream rows");
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed,controlsPassed=false,error=ex.GetType().Name});return;}
var gammaOne=GammaOne();var gammaTwo=GammaTwo();var resultRows=new List<object>();var framed=new Rational[2][,,,];var savedK=new FT[2];var savedGradients=new FT[2,45];
var parallelRows=new List<object>();var kadGamma=Caa.AdjointLiteral(gammaOne);
for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)
{
 Inc("parallelGenerators");var tensors=new[]{gammaOne,gammaTwo,kadGamma};var actions=tensors.Select(t=>ParallelControls.Action(t,a,b)).ToArray();
 for(int ti=0;ti<3;ti++)
 {
  var action=actions[ti];Inc("parallelTensorRows");parallelPassed&=action.Total.Count==0&&Equal(action.Clifford,Scale(action.Covector,-1));
  Inc("connectionOmissionDecoys");Inc("connectionOmissionDecoys");parallelPassed&=action.Clifford.Count>0&&action.Covector.Count>0;
 }
 for(int axis=0;axis<14;axis++)
 {
  Inc("derivativeDirections");var derivative=ParallelControls.ExteriorAt(actions[0].Total,axis);var first=Caa.Forward(derivative);var second=ParallelControls.DaggerAt(actions[2].Total,axis);
  Inc("kineticLegs");Inc("kineticLegs");parallelPassed&=derivative.Count==0&&first.Count==0&&second.Count==0;
 }
 parallelRows.Add(new{a,b,actions=actions.Select((v,i)=>new{tensor=i,clifford=Terms(v.Clifford),covector=Terms(v.Covector),total=Terms(v.Total)}).ToArray()});
}
for(int point=0;point<2;point++)
{
 Inc("contexts");var row=upstreamRows[point];var gram=ReadMatrix(row.GetProperty("gram"));var ricciCoordinate=ReadMatrix(row.GetProperty("ricci"));var einsteinCoordinate=ReadMatrix(row.GetProperty("einstein"));var coordinate=ReadCurvature(row.GetProperty("curvatureCoefficients"));
 var e=Frame(point);var inverse=e.Inverse();var hand=HandInverse(point);var metric=Transpose(e)*gram*e;var expectedEta=Eta();var left=inverse*e;var right=e*inverse;var metricInverse=expectedEta*Transpose(e)*gram;
 for(int a=0;a<14;a++)for(int b=0;b<14;b++){Inc("frameMatrixEntries");Inc("inverseEntries");framePassed&=metric[a,b]==expectedEta[a,b]&&inverse[a,b]==hand[a,b]&&inverse[a,b]==metricInverse[a,b]&&left[a,b]==(a==b?1:0)&&right[a,b]==(a==b?1:0);}
 var determinant=e.Determinant();Inc("orientationControls");framePassed&=determinant.ToString()==fx.GetProperty("frameDeterminants")[point].GetString()&&determinant.Numerator.Sign>0;
 var flipped=e.Copy();for(int a=0;a<14;a++)flipped[a,10]*=-1;Inc("orientationDecoys");framePassed&=flipped.Determinant()==determinant*-1&&(Transpose(flipped)*gram*flipped).Same(expectedEta);
 var vectorR=EndomorphismFrame(coordinate,e,inverse);var lowered=LoweredFrame(Lower(coordinate,gram),e);framed[point]=lowered;
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++)
 {
  Inc("frameCurvatureEntries");Inc("frameSymmetryEntries");framePassed&=lowered[a,b,c,d]==Sigma(d)*vectorR[a,b,c,d]&&lowered[a,b,c,d]+lowered[b,a,c,d]==0&&lowered[a,b,c,d]+lowered[a,b,d,c]==0&&lowered[a,b,c,d]==lowered[c,d,a,b]&&lowered[a,b,c,d]+lowered[b,c,a,d]+lowered[c,a,b,d]==0;
 }
 var ricci=Ricci(vectorR);var transportedRicci=Transpose(e)*ricciCoordinate*e;var raisedRicci=expectedEta*ricci;var einstein=raisedRicci-Matrix.Identity(14).Scale(raisedRicci.Trace()*new Rational(1,2));var transportedEinstein=inverse*einsteinCoordinate*e;
 int[] traceless=[1,2,3,4,5,6,11,12,13];var oracleRicci=new Matrix(14);var oracleEinstein=new Matrix(14);
 for(int a=0;a<14;a++){oracleRicci[a,a]=traceless.Contains(a)?new Rational(-5,4):new Rational(1,4);oracleEinstein[a,a]=traceless.Contains(a)?new Rational(15,4):new Rational(21,4);}
 for(int b=0;b<14;b++)for(int c=0;c<14;c++)
 {
  Rational contracted=0;for(int a=0;a<14;a++)contracted+=Sigma(a)*lowered[a,b,a,c]*-1;
  Inc("ricciEntries");Inc("einsteinEntries");framePassed&=ricci[b,c]==transportedRicci[b,c]&&ricci[b,c]==contracted&&raisedRicci[b,c]==oracleRicci[b,c]&&einstein[b,c]==oracleEinstein[b,c]&&einstein[b,c]==transportedEinstein[b,c]&&raisedRicci.Trace()==-10;
 }
 var f=Lift(lowered);Inc("liftTypeRows");spinPassed&=Typed(f,2)&&HAnti(f)&&f.Keys.All(k=>Degree(k.Blade)==2&&k.K0==0&&k.K1==0)&&f.Values.All(v=>v.Imaginary==0);
 for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)
 {
  var fab=Slice(f,a,b);for(int g=0;g<14;g++){var gamma=One(0,1<<g,1);var action=Product(fab,gamma,'C');Inc("spinCommutators");Inc("spinWordAgreement");spinPassed&=Equal(action,Vector(vectorR,a,b,g))&&Equal(action,NaiveProduct(fab,gamma,'C'));}
 }
 var anchor=Product(Slice(f,0,7),One(0,1<<7,1),'C');spinPassed&=Equal(anchor,One(0,1,new Scalar(new Rational(-1,4),0)));
 var wrongMetric=Product(Slice(Lift(lowered,true),0,7),One(0,1<<7,1),'C');var wrongFactor=Product(Slice(Lift(lowered,false,1),0,7),One(0,1<<7,1),'C');var wrongSign=Product(Slice(Lift(lowered,false,null,1),0,7),One(0,1<<7,1),'C');
 Inc("actualMetricDecoys");Inc("actualFactorDecoys");Inc("actualSignDecoys");spinPassed&=Equal(wrongMetric,One(0,1,new Scalar(new Rational(1,4),0)))&&Equal(wrongFactor,One(0,1,new Scalar(new Rational(-1,2),0)))&&Equal(wrongSign,One(0,1,new Scalar(new Rational(1,4),0)))&&!Equal(anchor,wrongMetric)&&!Equal(anchor,wrongFactor)&&!Equal(anchor,wrongSign);

 var actual=Caa.ForwardStages(f);var naive=Caa.ForwardStages(f,true);
 int[] degrees=[2,12,13,14,0,1,13,1];Inc("chainRows");
 for(int stage=0;stage<8;stage++){Inc("chainStageComparisons");Inc("chainTypeChecks");Inc("chainRealityChecks");chainPassed&=Equal(actual[stage],naive[stage])&&Typed(actual[stage],degrees[stage])&&HAnti(actual[stage]);}
 var first=Star(actual[2]);var second=Scale(actual[5],Fourier.Half*-1);var output=actual[7];savedK[point]=output;
 foreach(bool pass in new[]{Equal(first,Scale(WeightedGamma(oracleRicci),-1)),Equal(actual[3],One(Full,0,Scalar.I*5)),Equal(actual[4],One(0,0,Scalar.I*-5)),Equal(actual[5],Scale(gammaOne,10)),Equal(output,Scale(WeightedGamma(oracleEinstein),-1))}){Inc("chainOracleRows");chainPassed&=pass;}
 Inc("sourceDiagnosticRows");chainPassed&=output.Count==14&&CoefficientSquare(output)==new Rational(2115,8)&&Pair(output,output)==new Rational(-2115,8)&&Pair(gammaOne,output)==60;
 var averaged=Scale(gammaOne,new Scalar(new Rational(-30,7),0));
 foreach(bool pass in new[]{!Equal(output,new FT()),!Equal(output,averaged)&&Pair(gammaOne,averaged)==60,!Equal(output,first)&&Equal(Add(output,Scale(first,-1)),Scale(gammaOne,-5))}){Inc("sourceDecoyRows");chainPassed&=pass;}
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)
 {
  var unit=One(1<<a,1<<b,1);var legs=Caa.AdjointLegs(unit);var full=Add(legs.First,legs.Second);
  Inc("adjointRows");adjointPassed&=Equal(full,Caa.AdjointSimplified(unit))&&Pair(f,full)==Pair(output,unit);
  Inc("adjointLegPairings");Inc("adjointLegPairings");adjointPassed&=Pair(f,legs.First)==Pair(first,unit)&&Pair(f,legs.Second)==Pair(second,unit);
 }
 var unitQ=Product(gammaOne,gammaOne);var unitKQ=Caa.Forward(unitQ);var unitAdj=Caa.AdjointLiteral(gammaOne);var unitDq=Adjoint.DQAdjoint(gammaOne,unitAdj);
 var potentialCoefficient=Scale(Add(unitKQ,unitDq),new Scalar(new Rational(1,3),0));var polynomialRows=new List<object>();
 foreach(var m in ParallelControls.Monomials())
 {
  var coefficient=m==(0,0,0)?output:m==(2,1,0)?potentialCoefficient:m==(1,0,1)?gammaOne:new FT();
  var oracle=m==(0,0,0)?Scale(WeightedGamma(oracleEinstein),-1):m==(2,1,0)?Scale(gammaOne,312):m==(1,0,1)?gammaOne:new FT();
  Inc("polynomialCoefficients");gradientPassed&=Equal(coefficient,oracle);polynomialRows.Add(new{s=m.S,gamma=m.Gamma,kappa=m.Kappa,tensor=Terms(coefficient)});
 }
 // The polynomial identity has a parameter-independent anisotropic defect.
 // These cases are a complete logical partition, not a scan over real roots.
 foreach(string couplingClass in fx.GetProperty("couplingClasses").EnumerateArray().Select(q=>q.GetString()!))
 {Inc("couplingClasses");gradientPassed&=ParallelControls.Coefficient(output,1,1)-ParallelControls.Coefficient(output,0,0)==new Rational(3,2)&&Equal(potentialCoefficient,Scale(gammaOne,312))&&couplingClass.Length>0;}
 var parameterRows=new List<object>();int parameterIndex=0;
 foreach(int s in fx.GetProperty("sMenu").EnumerateArray().Select(q=>q.GetInt32()))foreach(int gamma in fx.GetProperty("gammaMenu").EnumerateArray().Select(q=>q.GetInt32()))foreach(int kappa in fx.GetProperty("kappaMenu").EnumerateArray().Select(q=>q.GetInt32()))
 {
  Inc("parameterRows");var field=Scale(gammaOne,s);var q=Product(field,field);var kq=Caa.Forward(q);var kad=Caa.AdjointLiteral(field);var dq=Adjoint.DQAdjoint(field,kad);
  foreach(bool pass in new[]{Equal(q,Scale(gammaTwo,2*s*s)),Equal(kq,Scale(gammaOne,312*s*s)),Equal(kad,Scale(gammaTwo,-24*s)),Equal(dq,Scale(gammaOne,624*s*s))}){Inc("potentialTensorChecks");gradientPassed&=pass;}
  var potential=Add(Scale(Add(kq,dq),new Scalar(new Rational(gamma,3),0)),Scale(field,kappa));var gradient=Add(output,potential);
  Rational scalarCoefficient=312*gamma*s*s+kappa*s;var predicted=Add(Scale(gammaOne,new Scalar(scalarCoefficient,0)),Scale(WeightedGamma(oracleEinstein),-1));
  Inc("gradientRows");gradientPassed&=Equal(gradient,predicted)&&Typed(gradient,1)&&HAnti(gradient)&&gradient.Keys.All(k=>Degree(k.Blade)==1&&k.K0==0&&k.K1==0)&&gradient.Count>0;
  for(int a=0;a<14;a++)for(int b=0;b<14;b++){Inc("gradientCoefficientEntries");gradientPassed&=ParallelControls.Coefficient(gradient,a,b)==(a==b?scalarCoefficient-oracleEinstein[a,a]:0);}
  var defect=ParallelControls.Coefficient(gradient,1,1)-ParallelControls.Coefficient(gradient,0,0);Inc("anisotropicWitnessRows");gradientPassed&=defect==new Rational(3,2);
  Inc("flatSourceDecoys");gradientPassed&=!Equal(gradient,potential)&&Equal(Add(gradient,Scale(potential,-1)),output);
  var wrongCubic=Add(output,Add(Scale(kq,new Scalar(new Rational(gamma,3),0)),Scale(field,kappa)));
  if(gamma!=0&&s!=0){Inc("missingCubicAdjointDecoys");gradientPassed&=!Equal(gradient,wrongCubic)&&Equal(Add(gradient,Scale(wrongCubic,-1)),Scale(gammaOne,208*gamma*s*s));}
  savedGradients[point,parameterIndex++]=gradient;
  parameterRows.Add(new{s,gamma,kappa,Q=Terms(q),KQ=Terms(kq),adjoint=Terms(kad),dqAdjoint=Terms(dq),potential=Terms(potential),gradient=Terms(gradient),anisotropicDifference=defect.ToString(),wrongCubic=Terms(wrongCubic)});
 }
 resultRows.Add(new{point,frame=e.Text(),inverse=inverse.Text(),frameDeterminant=determinant.ToString(),frameGram=metric.Text(),ricci=ricci.Text(),raisedRicci=raisedRicci.Text(),einstein=einstein.Text(),scalar=raisedRicci.Trace().ToString(),loweredCurvature=Nonzero(lowered),spinCurvature=Terms(f),mixedAnchor=Terms(anchor),wrongMetric=Terms(wrongMetric),wrongFactor=Terms(wrongFactor),wrongSign=Terms(wrongSign),stages=actual.Select(Terms).ToArray(),first=Terms(first),second=Terms(second),source=Terms(output),averagedSource=Terms(averaged),polynomialRows,parameterRows});
}
for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++){Inc("pointTransportEntries");framePassed&=framed[0][a,b,c,d]==framed[1][a,b,c,d];}
Inc("sourceTransportRows");chainPassed&=Equal(savedK[0],savedK[1]);for(int i=0;i<45;i++){Inc("gradientTransportRows");gradientPassed&=Equal(savedGradients[0,i],savedGradients[1,i]);}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");bool resourcesPassed=Matrix.Products<=resources.GetProperty("maximumTrackedMatrixProducts").GetInt64()&&CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumFourierTerms").GetInt32();
bool controlsPassed=knownAnswerPassed&&framePassed&&spinPassed&&chainPassed&&adjointPassed&&parallelPassed&&gradientPassed&&countsPassed&&resourcesPassed;string verdict=!knownAnswerPassed?precedence[1]:!framePassed?precedence[2]:!spinPassed?precedence[3]:!chainPassed||!adjointPassed?precedence[4]:!parallelPassed?precedence[5]:!gradientPassed?precedence[6]:!countsPassed||!resourcesPassed?precedence[7]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,framePassed,spinPassed,chainPassed,adjointPassed,parallelPassed,gradientPassed,countsPassed,resourcesPassed,counts,trackedMatrixProducts=Matrix.Products,trackedCoefficientProducts=CoefficientProducts,largestTensor=LargestTensor,parallelRows,rows=resultRows,scope=new{sourceOperatorSelected=false,sourceNormalizationSelected=false,sourceNormSelected=false,physicalVacuumRejected=false,metricEulerEquationComputed=false,nonconstantFieldsExcluded=false,anisotropicFieldsExcluded=false,physicalSpectrumComputed=false}});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=616,phaseId="phase616-curved-canonical-isotropic-gradient-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/curved_canonical_isotropic_gradient_audit.json",json);File.WriteAllText(Root+"/output/curved_canonical_isotropic_gradient_audit_summary.json",json);Console.WriteLine($"Phase616 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
