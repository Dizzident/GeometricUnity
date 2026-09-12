using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase622_full_homogeneous_kinetic_carrier_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase622-a64-full-homogeneous-kinetic-carrier-v1";
const string Success="full-homogeneous-kinetic-carrier-controls-pass-conditional-linear-branch";
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
 "domain": "full real u(64,64), both Clifford parities and central iI; no support projection",
 "geometry": "passed618 complete coordinate connection plus frame motion, transformed in derivative and both endomorphism slots to the same oriented608/610 frame",
 "fields": [
  "PHGamma",
  "PTGamma",
  "PtrGamma",
  "J",
  "B",
  "C",
  "L"
 ],
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
 "basisNorms": [
  "-4",
  "-9",
  "-1",
  "9",
  "-1",
  "-9"
 ],
 "forwardWeights": [
  [
   "0",
   "0",
   "0",
   "-1",
   "-1",
   "0"
  ],
  [
   "0",
   "0",
   "0",
   "1",
   "0",
   "0"
  ],
  [
   "0",
   "0",
   "0",
   "0",
   "1",
   "0"
  ],
  [
   "9/4",
   "-1",
   "0",
   "0",
   "0",
   "0"
  ],
  [
   "-9/4",
   "-4",
   "-3",
   "0",
   "0",
   "0"
  ],
  [
   "27/2",
   "16",
   "18",
   "0",
   "0",
   "0"
  ],
  [
   "63/4",
   "20",
   "21",
   "0",
   "0",
   "0"
  ]
 ],
 "reverseWeights": [
  [
   "0",
   "0",
   "0",
   "-1",
   "3",
   "-2"
  ],
  [
   "0",
   "0",
   "0",
   "1",
   "0",
   "0"
  ],
  [
   "0",
   "0",
   "0",
   "0",
   "-3",
   "2"
  ],
  [
   "9/4",
   "-1",
   "0",
   "0",
   "0",
   "0"
  ],
  [
   "3/4",
   "0",
   "1",
   "0",
   "0",
   "0"
  ],
  [
   "0",
   "-2",
   "0",
   "0",
   "0",
   "0"
  ],
  [
   "-3/4",
   "-2",
   "-1",
   "0",
   "0",
   "0"
  ]
 ],
 "fullWeights": [
  [
   "0",
   "0",
   "0",
   "-1",
   "1",
   "-1"
  ],
  [
   "0",
   "0",
   "0",
   "1",
   "0",
   "0"
  ],
  [
   "0",
   "0",
   "0",
   "0",
   "-1",
   "1"
  ],
  [
   "9/4",
   "-1",
   "0",
   "0",
   "0",
   "0"
  ],
  [
   "-3/4",
   "-2",
   "-1",
   "0",
   "0",
   "0"
  ],
  [
   "27/4",
   "7",
   "9",
   "0",
   "0",
   "0"
  ],
  [
   "15/2",
   "9",
   "10",
   "0",
   "0",
   "0"
  ]
 ],
 "sourceWeights": [
  "-21/4",
  "-15/4",
  "-21/4"
 ],
 "sourceKinetic": "3J/2",
 "hSquaredColumns": [
  [
   "-13/4",
   "-9/4"
  ],
  [
   "3/2",
   "5/2"
  ]
 ],
 "linearKappas": [
  -3,
  -2,
  -1,
  1,
  2,
  3
 ],
 "linearGamma": 0,
 "linearFormula": {
  "denominator": "D=4kappa^4+3kappa^2-19",
  "j": "(6kappa^2-15)/D",
  "l": "-27/(2D)",
  "vH": "(21/4-9j/4-15l/2)/kappa",
  "vT": "(15/4+j-9l)/kappa",
  "vt": "(21/4-10l)/kappa",
  "excluded": "kappa=0 or D=0; denominator is not a momentum-space pole"
 },
 "greenFormula": "delta kinetic=Pair(V,HS)+div(j)/2, j^a=sigma_a Pair(i_a Kdag S,V); div(j)=sum_ab Lambda_a[a,b] j^b",
 "greenAnchor": {
  "field": "B",
  "variation": "PHGamma",
  "currentTrace": "-4",
  "divergence": "4",
  "originalVariation": "5",
  "gradientPairing": "3",
  "pointwiseHAsymmetry": "4"
 },
 "omittedConnectionAnchor": {
  "axis": 0,
  "form": 1,
  "blade": 2,
  "spinOnly": "-1/4",
  "covectorOnly": "1/4",
  "full": "0"
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "expectedCounts": {
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "fullRealDomainMasks": 16384,
  "centralDomainControls": 2,
  "planeRepresentationControls": 1456,
  "contexts": 2,
  "coordinateConnectionEntries": 5488,
  "frameConnectionEntries": 5488,
  "metricSkewEntries": 5488,
  "spinLiftControls": 392,
  "fieldContexts": 14,
  "fieldInputControls": 14,
  "fieldCyclicControls": 28,
  "bivectorOuterAdjointControls": 8,
  "gradientForecastRows": 42,
  "gradientGradeRows": 42,
  "isotropyControls": 84,
  "disconnectedControls": 14,
  "gramRows": 98,
  "algebraicAdjointPairRows": 98,
  "originalKineticGreenRows": 98,
  "greenAnchorRows": 2,
  "pointwiseAsymmetryRows": 2,
  "unequalKineticLegRows": 6,
  "omittedConnectionRows": 4,
  "parallelGammaRows": 2,
  "sourceResponseRows": 2,
  "hSquaredRows": 4,
  "linearRows": 12,
  "linearResidualRows": 12,
  "omittedKineticDecoys": 12,
  "excludedZeroKappaRows": 2,
  "transportRows": 30,
  "validatedResults": 38,
  "derivativeComparisons": 532,
  "parallelAdjointComparisons": 532,
  "chainComparisons": 304,
  "chainTypeChecks": 304,
  "chainRealityChecks": 304,
  "adjointComparisons": 38,
  "exteriorComparisons": 38,
  "codifferentialComparisons": 38,
  "resultDomainChecks": 38,
  "jordanRows": 2,
  "zeroCouplingRows": 2
 },
 "resources": {
  "estimatedCpuSeconds": 60,
  "estimatedPeakBytes": 536870912,
  "conservativePlanningCpuSeconds": 900,
  "conservativePlanningPeakBytes": 8589934592,
  "maximumTrackedCoefficientProducts": 2000000000,
  "maximumTrackedMatrixProducts": 100000000,
  "maximumTensorTerms": 32768,
  "maximumMatrixDimension": 14,
  "maximumFrequency": 0,
  "expectedComputeCalls": 38,
  "expectedConnectionActions": 2604,
  "expectedDerivativeSlots": 532,
  "maximumOutputBytes": 134217728,
  "maximumRationalTextCharacters": 128,
  "maximumOutputFileBytes": 67108864
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormSelected": false,
  "sourceMetricSelected": false,
  "physicalCouplingSelected": false,
  "physicalVacuumSelected": false,
  "nonlinearSmallCarrierClosureClaimed": false,
  "pointwiseSelfAdjointnessAssumed": false,
  "homogeneousDenominatorCalledPhysicalPole": false,
  "globalFiniteActionProved": false,
  "physicalSpectrumClaimed": false,
  "gevNormalizationSelected": false
 },
 "jordanControl": {
  "Z": "(11J-27B+8C)/99",
  "HZ": "Gamma1",
  "HGamma": "0",
  "scope": "nontrivial zero Jordan chain on the six-dimensional invariant vector/bivector carrier only; not a physical zero mode or a full-domain kernel"
 },
 "zeroCouplingControl": {
  "S": "7J/12-9B/44+37C/66",
  "HS": "-A",
  "family": "S+a Gamma1",
  "scope": "gamma=kappa=0 diagnostic in the six-dimensional invariant vector/bivector carrier, not a kappa-to-zero continuation of the source-generated five-carrier solution"
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","homogeneous-connection-control-failed","full-kinetic-response-control-failed","invariant-carrier-control-failed","original-kinetic-green-control-failed","exact-linear-branch-control-failed","resource-census-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/Program.cs",
 ["project"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/Phase622FullHomogeneousKineticCarrierAudit.csproj",
 ["study"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/STUDY.md",
 ["kinetic-helper"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/KineticCarrier.cs",
 ["phase600-program"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/Program.cs",
 ["phase600-project"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/Phase600FullTraceAdjointPeriodicGradientNormAudit.csproj",
 ["phase600-study"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/STUDY.md",
 ["phase600-contract"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/preregistration/contract_v1.json",
 ["phase600-summary"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",
 ["phase600-exactarithmetic-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/ExactArithmetic.cs",
 ["phase600-fouriertensor-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/FourierTensor.cs",
 ["phase600-traceadjoint-helper"]="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001/TraceAdjoint.cs",
 ["phase607-program"]="studies/phase607_source_induced_vertical_curvature_audit_001/Program.cs",
 ["phase607-project"]="studies/phase607_source_induced_vertical_curvature_audit_001/Phase607SourceInducedVerticalCurvatureAudit.csproj",
 ["phase607-study"]="studies/phase607_source_induced_vertical_curvature_audit_001/STUDY.md",
 ["phase607-contract"]="studies/phase607_source_induced_vertical_curvature_audit_001/preregistration/contract_v1.json",
 ["phase607-summary"]="studies/phase607_source_induced_vertical_curvature_audit_001/output/source_induced_vertical_curvature_audit_summary.json",
 ["phase607-verticalgeometry-helper"]="studies/phase607_source_induced_vertical_curvature_audit_001/VerticalGeometry.cs",
 ["phase608-program"]="studies/phase608_source_induced_ambient_ricci_audit_001/Program.cs",
 ["phase608-project"]="studies/phase608_source_induced_ambient_ricci_audit_001/Phase608SourceInducedAmbientRicciAudit.csproj",
 ["phase608-study"]="studies/phase608_source_induced_ambient_ricci_audit_001/STUDY.md",
 ["phase608-contract"]="studies/phase608_source_induced_ambient_ricci_audit_001/preregistration/contract_v1.json",
 ["phase608-summary"]="studies/phase608_source_induced_ambient_ricci_audit_001/output/source_induced_ambient_ricci_audit_summary.json",
 ["phase608-ambientgeometry-helper"]="studies/phase608_source_induced_ambient_ricci_audit_001/AmbientGeometry.cs",
 ["phase610-program"]="studies/phase610_induced_spin_curvature_contraction_audit_001/Program.cs",
 ["phase610-project"]="studies/phase610_induced_spin_curvature_contraction_audit_001/Phase610InducedSpinCurvatureContractionAudit.csproj",
 ["phase610-study"]="studies/phase610_induced_spin_curvature_contraction_audit_001/STUDY.md",
 ["phase610-contract"]="studies/phase610_induced_spin_curvature_contraction_audit_001/preregistration/contract_v1.json",
 ["phase610-summary"]="studies/phase610_induced_spin_curvature_contraction_audit_001/output/induced_spin_curvature_contraction_audit_summary.json",
 ["phase610-spingeometry-helper"]="studies/phase610_induced_spin_curvature_contraction_audit_001/SpinGeometry.cs",
 ["phase611-program"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/Program.cs",
 ["phase611-project"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/Phase611UntiedCaaResponseJointGaugeAudit.csproj",
 ["phase611-study"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/STUDY.md",
 ["phase611-contract"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/preregistration/contract_v1.json",
 ["phase611-summary"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/output/untied_caa_response_joint_gauge_audit_summary.json",
 ["phase611-caaoperator-helper"]="studies/phase611_untied_caa_response_joint_gauge_audit_001/CaaOperator.cs",
 ["phase618-program"]="studies/phase618_homogeneous_covariant_connection_audit_001/Program.cs",
 ["phase618-project"]="studies/phase618_homogeneous_covariant_connection_audit_001/Phase618HomogeneousCovariantConnectionAudit.csproj",
 ["phase618-study"]="studies/phase618_homogeneous_covariant_connection_audit_001/STUDY.md",
 ["phase618-contract"]="studies/phase618_homogeneous_covariant_connection_audit_001/preregistration/contract_v1.json",
 ["phase618-summary"]="studies/phase618_homogeneous_covariant_connection_audit_001/output/homogeneous_covariant_connection_audit_summary.json",
 ["phase618-homogeneousconnection-helper"]="studies/phase618_homogeneous_covariant_connection_audit_001/HomogeneousConnection.cs",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["core-source-manifest"]="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json",
 ["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==622&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 var compiled=Directory.EnumerateFiles(Root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Split('/').Any(x=>x is "bin" or "obj")).Concat(System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(paths["project"]),"<Compile Include=\"([^\"]+)\"").Select(m=>Path.GetRelativePath(".",Path.GetFullPath(Path.Combine(Root,m.Groups[1].Value))))).ToArray();
 exactBindingsValid&=compiled.Length==10&&compiled.Distinct().Count()==10&&compiled.All(p=>bindings.Any(b=>b.path==p));
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase610","induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source"),("phase611","untied-caa-response-controls-pass-source-choice-open"),("phase618","homogeneous-connection-controls-pass-conditional-local-existence")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(3.27)","(3.34)","(9.1)","(9.4)","(12.26)","(12.27)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
using var fd=JsonDocument.Parse(FixtureJson);var fx=fd.RootElement;var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string key)=>counts[key]++;
bool knownAnswerPassed=true,connectionPassed=true,kineticPassed=true,carrierPassed=true,greenPassed=true,linearPassed=true,frequencyPassed=true;
int maximumRationalCharacters=1;var rows=new List<object>();var linearRows=new List<object>();var transport=new List<FT>();var planeRows=new List<object>();
foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("arithmeticControls");knownAnswerPassed&=pass;}
for(int a=0;a<=Full;a++)
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);
 Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);
 Inc("fullRealDomainMasks");var domain=One(0,a,AdjointSign(a)==-1?1:Scalar.I);knownAnswerPassed&=HAnti(domain)&&domain.Count==1;
}
foreach(bool pass in new[]{HAnti(One(0,0,Scalar.I)),Equal(Product(One(0,1,1),One(0,1,1),'A'),One(0,0,Scalar.I*2))}){Inc("centralDomainControls");knownAnswerPassed&=pass;}
for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)
{
 var l=new Matrix(14);l[a,b]=1;l[b,a]=-Sigma(a)*Sigma(b);int[] masks=[0,1<<a,1<<b,(1<<a)|(1<<b)];
 foreach(int form in masks)foreach(int blade in masks){var input=One(form,blade,AdjointSign(blade)==-1?1:Scalar.I);var actual=Kinetic.Action(l,input);var oracle=Homogeneous.Action(l,input);Inc("planeRepresentationControls");knownAnswerPassed&=Equal(actual,oracle)&&HAnti(actual);planeRows.Add(new{a,b,form,blade,actual=Terms(actual),oracle=Terms(oracle)});}
}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
using var upstream=JsonDocument.Parse(File.ReadAllBytes(paths["phase618-summary"]));var upstreamRows=upstream.RootElement.GetProperty("evidence").GetProperty("rows");
Rational[] ParseRow(JsonElement e)=>e.EnumerateArray().Select(q=>SpinGeometry.Parse(q.GetString()!)).ToArray();
FT Weights(FT[] basis,string key,int row)=>Kinetic.LinearCombination(basis,ParseRow(fx.GetProperty(key)[row]));
for(int point=0;point<2;point++)
{
 Inc("contexts");var old=upstreamRows[point];var frame=SpinGeometry.ReadMatrix(old.GetProperty("frame"));var inverse=SpinGeometry.HandInverse(point);connectionPassed&=frame.Same(SpinGeometry.Frame(point))&&(inverse*frame).Same(Matrix.Identity(14));
 var coordinateLambda=new Matrix[14];var lambda=new Matrix[14];var coordinateEvidence=new List<object>();
 for(int a=0;a<14;a++)
 {
  var q=old.GetProperty("coordinateRows")[a];var gamma=SpinGeometry.ReadMatrix(q.GetProperty("coordinateConnection"));var motion=SpinGeometry.ReadMatrix(q.GetProperty("frameMotion"));var nomizu=SpinGeometry.ReadMatrix(q.GetProperty("nomizu"));coordinateLambda[a]=gamma+motion;
  for(int i=0;i<14;i++)for(int j=0;j<14;j++){Inc("coordinateConnectionEntries");connectionPassed&=coordinateLambda[a][i,j]==nomizu[i,j];}
  coordinateEvidence.Add(new{axis=a,connection=gamma.Text(),frameMotion=motion.Text(),sum=coordinateLambda[a].Text(),oracle=nomizu.Text()});
 }
 for(int a=0;a<14;a++)
 {
  var transformed=new Matrix(14);for(int i=0;i<14;i++)transformed+=coordinateLambda[i].Scale(frame[i,a]);lambda[a]=inverse*transformed*frame;var oracle=SpinGeometry.ReadMatrix(old.GetProperty("frameNomizu")[a]);
  for(int i=0;i<14;i++)for(int j=0;j<14;j++){Inc("frameConnectionEntries");connectionPassed&=lambda[a][i,j]==oracle[i,j];Inc("metricSkewEntries");connectionPassed&=Sigma(i)*lambda[a][i,j]+Sigma(j)*lambda[a][j,i]==0;}
  var spin=Homogeneous.SpinGenerator(lambda[a]);for(int b=0;b<14;b++){var image=new FT();for(int c=0;c<14;c++)Put(image,(0,1<<c,0,0),new Scalar(lambda[a][c,b],0));Inc("spinLiftControls");connectionPassed&=Equal(Product(spin,One(0,1<<b,1),'C'),image);}
 }
 var fields=Kinetic.Fields(lambda);var basis=fields.Take(6).ToArray();var results=new Kinetic.Result[7];var fieldRows=new List<object>();var isotropyRows=new List<object>();
 carrierPassed&=Equal(fields[1],Kinetic.Vector(SpinGeometry.ReadMatrix(old.GetProperty("projector"))))&&Equal(fields[3],Kinetic.Read(old.GetProperty("J")));
 for(int i=0;i<7;i++)
 {
  Inc("fieldContexts");Inc("fieldInputControls");carrierPassed&=Typed(fields[i],1)&&HAnti(fields[i])&&Kinetic.Grades(fields[i],i<3?1:2);results[i]=Kinetic.Compute(lambda,fields[i]);Validate(results[i]);
  FT[] images=[results[i].Forward,results[i].Reverse,results[i].Full];string[] keys=["forwardWeights","reverseWeights","fullWeights"];
  for(int leg=0;leg<3;leg++){Inc("gradientForecastRows");kineticPassed&=Equal(images[leg],Weights(basis,keys[leg],i));Inc("gradientGradeRows");kineticPassed&=Kinetic.Grades(images[leg],i<3?2:1);}
  var cyclic=Kinetic.Cyclic(fields[i]);var exteriorCyclic=Kinetic.Cyclic(results[i].ExteriorDerivative);Inc("fieldCyclicControls");carrierPassed&=cyclic.Count==0;Inc("fieldCyclicControls");carrierPassed&=exteriorCyclic.Count==0;
  var outer=Caa.OuterAdjoint(fields[i]);if(i>=3){Inc("bivectorOuterAdjointControls");carrierPassed&=outer.Count==0&&Equal(outer,Scale(cyclic,Scalar.I*2));}
  foreach(var isotropy in old.GetProperty("isotropyRows").EnumerateArray()){var rho=SpinGeometry.ReadMatrix(isotropy.GetProperty("frameGenerator"));var action=Kinetic.Action(rho,fields[i]);Inc("isotropyControls");carrierPassed&=action.Count==0;isotropyRows.Add(new{field=i,i=isotropy.GetProperty("i").GetInt32(),k=isotropy.GetProperty("k").GetInt32(),action=Terms(action)});}
  var disconnected=Homogeneous.Disconnected(fields[i]);Inc("disconnectedControls");carrierPassed&=Equal(disconnected,fields[i]);
  if(i>=4){Inc("unequalKineticLegRows");kineticPassed&=!Equal(results[i].Forward,results[i].Reverse);}
  fieldRows.Add(new{field=i,name=fx.GetProperty("fields")[i].GetString(),result=Kinetic.Evidence(results[i]),cyclic=Terms(cyclic),exteriorCyclic=Terms(exteriorCyclic),outerAdjoint=Terms(outer),disconnected=Terms(disconnected)});
 }
 var gramRows=new List<object>();var greenRows=new List<object>();var norms=ParseRow(fx.GetProperty("basisNorms"));
 Rational[] Coeff(int i){var c=new Rational[6];if(i<6)c[i]=1;else{c[4]=-1;c[5]=1;}return c;}
 for(int s=0;s<7;s++)for(int v=0;v<7;v++)
 {
  var cs=Coeff(s);var cv=Coeff(v);Rational norm=0;for(int i=0;i<6;i++)norm+=cs[i]*cv[i]*norms[i];Rational gram=Pair(fields[s],fields[v]);Inc("gramRows");carrierPassed&=gram==norm;gramRows.Add(new{s,v,actual=gram.ToString(),expected=norm.ToString()});
  Rational pairForward=Pair(results[v].Forward,fields[s]),pairAdjoint=Pair(results[v].ExteriorDerivative,results[s].Adjoint);Inc("algebraicAdjointPairRows");kineticPassed&=pairForward==pairAdjoint;
  Rational original=(Pair(fields[v],results[s].Forward)+Pair(fields[s],results[v].Forward))*Fourier.Half.Real,gradient=Pair(fields[v],results[s].Full);var current=Kinetic.Current(results[s].Adjoint,fields[v]);Rational divergence=Kinetic.Divergence(lambda,current);Inc("originalKineticGreenRows");greenPassed&=original==gradient+divergence*Fourier.Half.Real;
  if(s==4&&v==0){Inc("greenAnchorRows");greenPassed&=original==5&&gradient==3&&divergence==4&&current[10]==-4&&current.Where((_,a)=>a!=10).All(q=>q==0);}
  greenRows.Add(new{s,v,pairForward=pairForward.ToString(),pairAdjoint=pairAdjoint.ToString(),originalVariation=original.ToString(),gradientPairing=gradient.ToString(),current=current.Select(q=>q.ToString()),divergence=divergence.ToString()});
 }
 Rational asymmetry=Pair(fields[0],results[4].Full)-Pair(results[0].Full,fields[4]);Inc("pointwiseAsymmetryRows");greenPassed&=asymmetry==4;
 var gammaResult=Kinetic.Compute(lambda,Caa.Gamma1);Validate(gammaResult);Inc("parallelGammaRows");connectionPassed&=gammaResult.Derivatives.All(t=>t.Count==0)&&gammaResult.Forward.Count==0&&gammaResult.Reverse.Count==0&&gammaResult.Full.Count==0;
 var spinOnly=Kinetic.SpinAction(lambda[0],Caa.Gamma1);var covectorOnly=Kinetic.FormAction(lambda[0],Caa.Gamma1);Inc("omittedConnectionRows");connectionPassed&=spinOnly.Count>0&&spinOnly.GetValueOrDefault((1,2,0,0))==new Scalar(new Rational(-1,4),0);Inc("omittedConnectionRows");connectionPassed&=covectorOnly.Count>0&&covectorOnly.GetValueOrDefault((1,2,0,0))==new Scalar(new Rational(1,4),0)&&Add(spinOnly,covectorOnly).Count==0;
 var source=Kinetic.Read(old.GetProperty("source"));var sourceResult=Kinetic.Compute(lambda,source);Validate(sourceResult);Inc("sourceResponseRows");kineticPassed&=Equal(source,Kinetic.LinearCombination(basis,ParseRow(fx.GetProperty("sourceWeights"))))&&Equal(sourceResult.Forward,Scale(fields[3],new Scalar(new Rational(3,2),0)))&&Equal(sourceResult.Reverse,sourceResult.Forward);
 var hSquaredRows=new List<object>();for(int i=0;i<2;i++){int f=i==0?3:6;var r=Kinetic.Compute(lambda,results[f].Full);Validate(r);var w=ParseRow(fx.GetProperty("hSquaredColumns")[i]);Inc("hSquaredRows");linearPassed&=Equal(r.Full,Kinetic.LinearCombination([fields[3],fields[6]],w));hSquaredRows.Add(new{field=f,result=Kinetic.Evidence(r),coefficients=w.Select(q=>q.ToString())});}
 var z=Kinetic.LinearCombination([fields[3],fields[4],fields[5]],new Rational(11,99),new Rational(-27,99),new Rational(8,99));var zr=Kinetic.Compute(lambda,z);Validate(zr);Inc("jordanRows");linearPassed&=z.Count>0&&Equal(zr.Full,Caa.Gamma1)&&gammaResult.Full.Count==0;
 var zeroField=Kinetic.LinearCombination([fields[3],fields[4],fields[5]],new Rational(7,12),new Rational(-9,44),new Rational(37,66));var zeroResult=Kinetic.Compute(lambda,zeroField);Validate(zeroResult);var zeroResidual=Add(source,zeroResult.Full);Inc("zeroCouplingRows");linearPassed&=zeroResidual.Count==0&&new Rational(-9,44)+new Rational(37,66)==new Rational(47,132);
 foreach(int kappa in fx.GetProperty("linearKappas").EnumerateArray().Select(q=>q.GetInt32()))
 {
  Rational k=kappa,k2=k*k,d=4*k2*k2+3*k2-19;bool admissible=k!=0&&d!=0;if(!admissible)throw new InvalidOperationException("declared nonzero denominator menu");Rational j=(6*k2-15)*Matrix.Inv(d),l=new Rational(-27,2)*Matrix.Inv(d);
  Rational vh=(new Rational(21,4)-new Rational(9,4)*j-new Rational(15,2)*l)*Matrix.Inv(k),vt=(new Rational(15,4)+j-9*l)*Matrix.Inv(k),vr=(new Rational(21,4)-10*l)*Matrix.Inv(k);
  var input=Kinetic.LinearCombination([fields[0],fields[1],fields[2],fields[3],fields[6]],vh,vt,vr,j,l);var r=Kinetic.Compute(lambda,input);Validate(r);var residual=Add(Add(source,r.Full),Scale(input,new Scalar(k,0)));var omitted=Add(source,Scale(input,new Scalar(k,0)));
  Rational determinant=(k2+new Rational(13,4))*(k2-new Rational(5,2))+new Rational(27,8);Inc("linearRows");linearPassed&=determinant==d*new Rational(1,4);Inc("linearResidualRows");linearPassed&=residual.Count==0;Inc("omittedKineticDecoys");linearPassed&=omitted.Count>0&&Equal(omitted,Scale(r.Full,-1));
  linearRows.Add(new{point,kappa,gamma=0,denominator=d.ToString(),determinant=determinant.ToString(),coefficients=new[]{vh,vt,vr,j,l}.Select(q=>q.ToString()),result=Kinetic.Evidence(r),residual=Terms(residual),omittedKineticResidual=Terms(omitted)});
 }
 Inc("excludedZeroKappaRows");linearPassed&=!LinearAdmissible(0);
 var transported=results.SelectMany(r=>new[]{r.Input,r.Forward,r.Reverse,r.Full}).Concat(new[]{z,zr.Full}).ToArray();if(point==0)transport.AddRange(transported);else for(int i=0;i<transported.Length;i++){Inc("transportRows");carrierPassed&=Equal(transport[i],transported[i]);}
 rows.Add(new{point,frame=frame.Text(),inverse=inverse.Text(),coordinateEvidence,frameConnection=lambda.Select(q=>q.Text()),fields=fieldRows,isotropyRows,gramRows,greenRows,pointwiseAsymmetry=asymmetry.ToString(),parallelGamma=Kinetic.Evidence(gammaResult),spinOnly=Terms(spinOnly),covectorOnly=Terms(covectorOnly),source=Kinetic.Evidence(sourceResult),hSquaredRows,jordan=new{coefficients=new[]{"11/99","-27/99","8/99"},result=Kinetic.Evidence(zr)},zeroCoupling=new{coefficients=new[]{"7/12","-9/44","37/66"},result=Kinetic.Evidence(zeroResult),residual=Terms(zeroResidual),outsideSourceGeneratedCarrier="47/132"}});
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");bool resourcesPassed=frequencyPassed&&CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&Matrix.Products<=resources.GetProperty("maximumTrackedMatrixProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumTensorTerms").GetInt32()&&maximumRationalCharacters<=resources.GetProperty("maximumRationalTextCharacters").GetInt32()&&Kinetic.ComputeCalls==38&&Kinetic.ConnectionActions==2604&&Kinetic.DerivativeSlots==532;
bool controlsPassed=knownAnswerPassed&&connectionPassed&&kineticPassed&&carrierPassed&&greenPassed&&linearPassed&&countsPassed&&resourcesPassed;
string verdict=!knownAnswerPassed?precedence[1]:!connectionPassed?precedence[2]:!kineticPassed?precedence[3]:!carrierPassed?precedence[4]:!greenPassed?precedence[5]:!linearPassed?precedence[6]:!countsPassed||!resourcesPassed?precedence[7]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,connectionPassed,kineticPassed,carrierPassed,greenPassed,linearPassed,countsPassed,resourcesPassed,frequencyPassed,counts,trackedCoefficientProducts=CoefficientProducts,trackedMatrixProducts=Matrix.Products,largestTensor=LargestTensor,maximumRationalCharacters,computeCalls=Kinetic.ComputeCalls,connectionActions=Kinetic.ConnectionActions,derivativeSlots=Kinetic.DerivativeSlots,planeRows,rows,linearRows,scope=fx.GetProperty("scope").Clone()});

void Validate(Kinetic.Result r)
{
 Inc("validatedResults");Inc("adjointComparisons");kineticPassed&=Equal(r.Adjoint,r.SimplifiedAdjoint);Inc("exteriorComparisons");kineticPassed&=Equal(r.ExteriorDerivative,r.ExteriorOracle);Inc("codifferentialComparisons");kineticPassed&=Equal(r.Reverse,r.ParallelReverse);
 for(int a=0;a<14;a++){Inc("derivativeComparisons");connectionPassed&=Equal(r.Derivatives[a],r.DerivativeOracles[a]);Inc("parallelAdjointComparisons");kineticPassed&=Equal(r.AdjointDerivatives[a],r.ParallelAdjoints[a]);}
 int[] degrees=[2,12,13,14,0,1,13,1];for(int i=0;i<8;i++){Inc("chainComparisons");kineticPassed&=Equal(r.Stages[0][i],r.Stages[1][i]);Inc("chainTypeChecks");kineticPassed&=Typed(r.Stages[0][i],degrees[i])&&Typed(r.Stages[1][i],degrees[i]);Inc("chainRealityChecks");kineticPassed&=HAnti(r.Stages[0][i])&&HAnti(r.Stages[1][i]);}
 var tensors=new[]{r.Input,r.ExteriorDerivative,r.ExteriorOracle,r.AdjointFirst,r.AdjointSecond,r.Adjoint,r.SimplifiedAdjoint,r.Reverse,r.ParallelReverse,r.Forward,r.Full}.Concat(r.Derivatives).Concat(r.DerivativeOracles).Concat(r.AdjointDerivatives).Concat(r.ParallelAdjoints).Concat(r.Stages.SelectMany(c=>c));Inc("resultDomainChecks");
 foreach(var t in tensors){frequencyPassed&=t.Keys.All(k=>k.Form>=0&&k.Form<=Full&&k.Blade>=0&&k.Blade<=Full&&k.K0==0&&k.K1==0)&&HAnti(t);foreach(var z in t.Values)maximumRationalCharacters=Math.Max(maximumRationalCharacters,Math.Max(z.Real.ToString().Length,z.Imaginary.ToString().Length));}
}
static bool LinearAdmissible(Rational k)=>k!=0&&4*k*k*k*k+3*k*k-19!=0;
void Emit(string terminal,object evidence)
{
 object Result()=>new{schemaVersion=1,phase=622,phaseId="phase622-full-homogeneous-kinetic-carrier-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(Result())+"\n";if(Encoding.UTF8.GetByteCount(json)>67108864){string originalTerminal=terminal;if(terminal==Success)terminal=precedence[7];var original=JsonSerializer.SerializeToElement(evidence);var bounded=new Dictionary<string,object?>();foreach(var p in original.EnumerateObject())if(p.Value.ValueKind is JsonValueKind.True or JsonValueKind.False or JsonValueKind.Number or JsonValueKind.String||p.Name=="counts")bounded[p.Name]=p.Value.Clone();bounded["controlsPassed"]=false;bounded["resourcesPassed"]=false;bounded["resourceFailure"]="per-file64MiB or aggregate128MiB output byte ceiling";bounded["originalTerminal"]=originalTerminal;bounded["fullTensorEvidenceAvailable"]=false;evidence=bounded;json=JsonSerializer.Serialize(Result())+"\n";}
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/full_homogeneous_kinetic_carrier_audit.json",json);File.WriteAllText(Root+"/output/full_homogeneous_kinetic_carrier_audit_summary.json",json);Console.WriteLine($"Phase622 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
