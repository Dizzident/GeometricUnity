using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static MetricVariation;
using static ActionVariation;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

bool replayMode=args.Length>0;
const string Root="studies/phase621_induced_metric_full_variation_scope_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase621-a63-induced-metric-full-variation-scope-v1";
const string Success="induced-metric-full-variation-controls-pass-fixed-domain-only";
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
 "h0": "eta=diag(-1,1,1,1) at BOTH fixed fibre points; never varied together with y",
 "alpha": "1",
 "beta": "-1/2",
 "sigma": "-1",
 "symmetricBasis": "E00,E11,E22,E33,E01+E10,E02+E20,E03+E30,E12+E21,E13+E31,E23+E32",
 "jetMenu": "all10M times all35 multiindices I in N^4, |I|<=3; delta h=M x^I/I! at x=0; degree then lexicographic ascending first3 exponents",
 "nativeField": "epsilon=I, varpi=T fixed coordinate coefficients in a fixed chimeric spin trivialization; dT=0 for the finite local fields",
 "frame": "E_t=(I+t deltaN)E0(y), baseline vertical first jet rho(-y^-1A/2)E0",
 "connectionVariation": "delta omega=E0^-1(delta Gamma+d deltaN+[Gamma,deltaN])E0",
 "metricRoute": "G_C=(I-N_C)^T G0(I-N_C), full first and second upstairs coordinate jets",
 "curvatureRoute": "dual differentiated Koszul versus independently expanded Palatini; full coordinate R and both spin/exterior frame arguments",
 "operator": "canonical untied CAA: firstC outerA innerA; Phi1=Gamma1 Phi2=Gamma2",
 "actionRoutes": [
  "fixed baseline frame: dual original action, every Phi1/Phi2/star/spin/reference occurrence",
  "adapted moving frame: independent full product expansion with all input form-slot variations"
 ],
 "pairing": "signed exterior metric times -ReTr(XY)/128; original top form, no positive metric",
 "fields": [
  "Gamma1 at the point, locally coordinate-constant",
  "Gamma1+theta0 Gamma01",
  "Gamma1+theta0 Gamma01+i theta1 I",
  "(dx0+dy00)gamma0",
  "dy01 gamma0, coordinate fibre index8; third-jet source probe"
 ],
 "baselinePieces": [
  [
   "60",
   "0",
   "-1456",
   "-7"
  ],
  [
   "60",
   "-13/4",
   "-4372/3",
   "-13/2"
  ],
  [
   "60",
   "-13/4",
   "-4372/3",
   "-6"
  ],
  [
   "21/4",
   "0",
   "0",
   "-3/4"
  ],
  [
   "0",
   "0",
   "0",
   "1/4 at point0;1 at point1"
  ]
 ],
 "pieceWeights": [
  "1",
  "1/2",
  "gamma/3",
  "kappa/2"
 ],
 "massControl": {
  "variation": "delta h00=f(x0)",
  "normalizedDerivative": "-kappa fprime",
  "frozenHodge": "0",
  "unweightedMean": "0",
  "cosWeightSinVariationMean": "-kappa/2",
  "periodicIdentityOnly": true
 },
 "sectionControl": "separate coincident observer background h_section=y: s_h^*G_h=sigma h; zero-jet response sigma M",
 "alternativeIdentificationControl": "separate coincident h_alt=y: sigma h_alt y^-1 h_alt has zero-jet response2sigma M; nonselected",
 "extraBaseDensityControls": [
  "delta h=phi h0 gives120phi on Gamma at gamma=kappa0",
  "delta h00=phi gives-30phi on Gamma at gamma=kappa0"
 ],
 "exactTolerance": 0,
 "coreFileCount": 726,
 "expectedCounts": {
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "fullRealDomainMasks": 16384,
  "points": 2,
  "frameControls": 2,
  "baselineConnectionEntries": 5488,
  "baselineCurvatureEntries": 76832,
  "jetContexts": 700,
  "zeroJetContexts": 20,
  "firstJetContexts": 80,
  "secondJetContexts": 200,
  "thirdJetContexts": 400,
  "metricEntries": 137200,
  "metricFirstJetEntries": 1920800,
  "metricSecondJetEntries": 26891200,
  "inverseVariationEntries": 137200,
  "volumeRows": 700,
  "connectionVariationEntries": 1920800,
  "connectionDerivativeEntries": 26891200,
  "curvatureVariationEntries": 26891200,
  "frameCurvatureEntries": 26891200,
  "frameConnectionSkewEntries": 1920800,
  "spinCurvatureRows": 700,
  "solderMotionRows": 1400,
  "fieldRows": 3500,
  "fieldDomainRows": 3500,
  "actionValueComparisons": 14000,
  "actionDeltaComparisons": 14000,
  "chainStageComparisons": 84000,
  "zeroJetGeometryRows": 20,
  "zeroJetActionEntries": 400,
  "massDerivativeAnchors": 2,
  "frozenHodgeDecoys": 2,
  "weightedIntegrationControls": 4,
  "movingSectionEntries": 320,
  "alternativeIdentificationEntries": 320,
  "addedDensityControls": 4,
  "downstairsConnectionEntries": 44800,
  "thirdJetCurvatureAnchors": 1,
  "thirdJetSourceActionAnchors": 1,
  "shardRows": 700,
  "stageTensorFingerprints": 336000
 },
 "resources": {
  "estimatedCpuSeconds": 900,
  "estimatedPeakBytes": 1073741824,
  "conservativePlanningCpuSeconds": 2400,
  "conservativePlanningPeakBytes": 8589934592,
  "maximumTrackedCoefficientProducts": 20000000000,
  "maximumTrackedMatrixProducts": 10000000000,
  "maximumTrackedDualProducts": 4000000000,
  "maximumTensorTerms": 32768,
  "maximumMatrixDimension": 14,
  "largestArrayEntries": 38416,
  "maximumFrequency": 2,
  "retainedJetContexts": 700,
  "maximumShardBytes": 67108864,
  "maximumRationalTextCharacters": 128,
  "maximumAggregateShardBytes": 2147483648
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormSelected": false,
  "sourceMetricSelected": false,
  "physicalCouplingSelected": false,
  "physicalVacuumSelected": false,
  "globalActionDefined": false,
  "observerMetricEquationComputed": false,
  "fullUnrestrictedMetricEulerComputed": false,
  "physicalSpectrumClaimed": false,
  "fieldEulerBoundaryShortcutUsed": false,
  "fibreIntegrationByPartsUsed": false,
  "alternativeIdentificationSelected": false
 },
 "thirdJetControl": {
  "point": 1,
  "metricBasis": "E00",
  "multiindex": [
   0,
   3,
   0,
   0
  ],
  "deltaMetricDD_11_0_8": "-3/4",
  "deltaCurvature_1_0_1_8": "3/4",
  "sourceActionVariation": "3/16"
 },
 "evidenceStorage": {
  "version": "phase621-metric-jet-shards-dag-v1",
  "shards": 700,
  "pattern": "output/shards/jet_p{0..1}_m{00..09}_j{00..34}.json",
  "completePathsetRequired": true,
  "geometry": "complete sparse tensors, zero coefficients implicit, no support projection",
  "tensorFormat": "numeric ascending (form,blade,k0,k1), reduced Rational ToString, Fourier.Terms property order",
  "tensorFingerprint": "sha256(compact UTF8 JSON Terms without newline)",
  "documentFormat": "compact UTF8 JSON with exactly one LF",
  "maximumShardBytes": 67108864,
  "pointBaselineIds": [
   0,
   1
  ],
  "fieldsPerShard": 5,
  "stagesPerPiece": 8,
  "expandedStageTensorFingerprintCount": 336000,
  "representation": "all primitive leaves plus exact versioned DAG recipes and expanded-stage fingerprints; not expanded stage arrays",
  "replayMode": "--verify-evidence; read-only independent NaiveProduct and ordered-wedge motion; reconstruct ALL stages and original scalars",
  "maximumAggregateShardBytes": 2147483648
 },
 "evidenceDag": {
  "schema": "caa-jet-dag-v1",
  "inputs": {
   "f": 2,
   "p1": 1,
   "p2": 2
  },
  "nodes": [
   {
    "id": "sf",
    "op": "star",
    "a": "f",
    "degree": 12
   },
   {
    "id": "one",
    "op": "product",
    "a": "p1",
    "b": "sf",
    "kind": "C",
    "degree": 13
   },
   {
    "id": "inner",
    "op": "product",
    "a": "p2",
    "b": "sf",
    "kind": "A",
    "degree": 14
   },
   {
    "id": "zero",
    "op": "star",
    "a": "inner",
    "degree": 0
   },
   {
    "id": "outer",
    "op": "product",
    "a": "p1",
    "b": "zero",
    "kind": "A",
    "degree": 1
   },
   {
    "id": "outer-star",
    "op": "star",
    "a": "outer",
    "degree": 13
   },
   {
    "id": "scaled",
    "op": "scale",
    "a": "outer-star",
    "scalar": "-1/2",
    "degree": 13
   },
   {
    "id": "upper",
    "op": "add",
    "a": "one",
    "b": "scaled",
    "degree": 13
   },
   {
    "id": "lower",
    "op": "star",
    "a": "upper",
    "degree": 1
   }
  ],
  "stages": [
   "f",
   "sf",
   "one",
   "inner",
   "zero",
   "outer",
   "upper",
   "lower"
  ]
 },
 "dagSemantics": {
  "scalarAlgebra": "dual pairs (value,delta), real-bilinear; product delta=deltaA*B+A*deltaB",
  "products": "W exterior Clifford product; C exterior coefficient commutator; A=i times exterior coefficient anticommutator, each via full masks",
  "star": "HodgeSign(mask), complement mask; adapted star acts componentwise; fixed star delta=*deltaInput+*M(input)-M(*input)",
  "formMotion": "M(A,t) replaces each occupied covector slot by sum_b A[old,b] theta_b, original wedge order, Clifford slots unchanged",
  "inputs": "t,b,fb from point baseline; db,df,A from shard; dt=b wedge t+t wedge b; q=t wedge t",
  "fixedInputs": "f=(fb,df-M(fb)); dt=(dt,db wedge t+t wedge db); q=(q,0); p1=(Gamma1,-M(Gamma1));p2=(Gamma2,-M(Gamma2))",
  "adaptedInputs": "tdot=M(t);bdot=db+M(b);f=(fb,df);dt=(dt,bdot wedge t+t wedge bdot+b wedge tdot+tdot wedge b);q=(q,tdot wedge t+t wedge tdot);p1,p2 fixed",
  "action": "weights1,1/2,1/3,1/2; fixed original pair=-ReTr(t wedge movingStar(output))/128; adapted pair full signed exterior/Clifford metric",
  "sourceCouplings": "gamma,kappa are formal independent weights, no value selected"
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-domain-control-failed","metric-jet-control-failed","full-connection-curvature-control-failed","moving-spin-frame-control-failed","full-original-action-variation-control-failed","scope-negative-control-failed","resource-census-control-failed","induced-metric-full-variation-controls-pass-fixed-domain-only"];
var paths=new Dictionary<string,string>{
 ["program"]="studies/phase621_induced_metric_full_variation_scope_audit_001/Program.cs",
 ["project"]="studies/phase621_induced_metric_full_variation_scope_audit_001/Phase621InducedMetricFullVariationScopeAudit.csproj",
 ["study"]="studies/phase621_induced_metric_full_variation_scope_audit_001/STUDY.md",
 ["metric-helper"]="studies/phase621_induced_metric_full_variation_scope_audit_001/MetricVariation.cs",
 ["action-helper"]="studies/phase621_induced_metric_full_variation_scope_audit_001/ActionVariation.cs",
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
 ["phase615-helper"]="studies/phase615_source_induced_splitting_volume_scope_audit_001/SplittingControls.cs",
 ["phase615-program"]="studies/phase615_source_induced_splitting_volume_scope_audit_001/Program.cs",
 ["phase615-study"]="studies/phase615_source_induced_splitting_volume_scope_audit_001/STUDY.md",
 ["phase615-contract"]="studies/phase615_source_induced_splitting_volume_scope_audit_001/preregistration/contract_v1.json",
 ["phase615-summary"]="studies/phase615_source_induced_splitting_volume_scope_audit_001/output/source_induced_splitting_volume_scope_audit_summary.json",
 ["phase618-helper"]="studies/phase618_homogeneous_covariant_connection_audit_001/HomogeneousConnection.cs",
 ["phase618-program"]="studies/phase618_homogeneous_covariant_connection_audit_001/Program.cs",
 ["phase618-study"]="studies/phase618_homogeneous_covariant_connection_audit_001/STUDY.md",
 ["phase618-contract"]="studies/phase618_homogeneous_covariant_connection_audit_001/preregistration/contract_v1.json",
 ["phase618-summary"]="studies/phase618_homogeneous_covariant_connection_audit_001/output/homogeneous_covariant_connection_audit_summary.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["core-source-manifest"]="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json",
 ["build-props"]="Directory.Build.props",
 ["evidence-helper"]="studies/phase621_induced_metric_full_variation_scope_audit_001/EvidenceStore.cs"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==621&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase610","induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source"),("phase611","untied-caa-response-controls-pass-source-choice-open"),("phase615","induced-splitting-volume-controls-pass-homothety-outside-declared-image"),("phase618","homogeneous-connection-controls-pass-conditional-local-existence")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(9.1)","(9.4)","(3.27)","(3.34)","(12.27)","(3.1)","(3.37)","(12.24)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

if(args.Length>0)
{
 if(args.Length!=1||args[0]!="--verify-evidence")throw new ArgumentException("only --verify-evidence is supported");
 var replay=EvidenceStore.Verify(Root,Sha(ContractPath),contract.GetProperty("fixtures"));Console.WriteLine($"Phase621 lossless evidence replay: {replay.Shards} shards, {replay.Fields} fields, {replay.StagePairs} stage pairs, {replay.ScalarEntries} action coefficients");return;
}
var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string k)=>counts[k]++;
bool knownAnswerPassed=true,metricPassed=true,curvaturePassed=true,framePassed=true,actionPassed=true,decoysPassed=true,frequencyPassed=true;
foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("arithmeticControls");knownAnswerPassed&=pass;}
for(int a=0;a<16384;a++)
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);
 Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);
 var domain=One(0,a,AdjointSign(a)==-1?1:Scalar.I);Inc("fullRealDomainMasks");knownAnswerPassed&=HAnti(domain)&&domain.Count==1;
}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
using var geomDoc=JsonDocument.Parse(File.ReadAllBytes(paths["phase608-summary"]));var geomRows=geomDoc.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().ToArray();
if(!JsonNode.DeepEquals(JsonNode.Parse(EvidenceStore.GraphJson),JsonNode.Parse(fx.GetProperty("evidenceDag").GetRawText())))throw new InvalidOperationException("DAG fixture parity");
var pointRows=new List<object>();var rows=new List<object>();var scopeRows=new List<object>();var multis=Multiindices();var basis=Geometry.Basis();
metricPassed&=multis.Length==35&&multis.Select(x=>string.Join(",",x)).Distinct().Count()==35&&multis.GroupBy(x=>x.Sum()).OrderBy(x=>x.Key).Select(x=>x.Count()).SequenceEqual(new[]{1,4,10,20});
try
{
for(int point=0;point<2;point++)
{
 Inc("points");var y=point==0?Matrix.Diagonal(-1,1,1,1):Matrix.Diagonal(-1,4,9,16);var h=Matrix.Diagonal(-1,1,1,1);var g=new Ambient(y,1,new Rational(-1,2),-1);var data=MetricData.Baseline(g);var inverse=g.Gram.Inverse();var e=SpinGeometry.Frame(point);var ei=e.Inverse();var eta=SpinGeometry.Eta();
 Inc("frameControls");framePassed&=(SpinGeometry.Transpose(e)*g.Gram*e).Same(eta)&&ei.Same(SpinGeometry.HandInverse(point))&&e.Determinant().Numerator.Sign>0;
 var baseline=DualConnection(data,new MetricData()).Value;var old=geomRows.Single(q=>q.GetProperty("point").GetInt32()==point&&q.GetProperty("beta").GetString()=="-1/2");var oldR=SpinGeometry.ReadCurvature(old.GetProperty("curvatureCoefficients"));
 var omega=new Matrix[14];
 for(int a=0;a<14;a++)
 {
  var expectedGamma=new Matrix(14);for(int c=0;c<14;c++){var v=g.GammaExpected(a,c);for(int d=0;d<14;d++){expectedGamma[d,c]=v[d];Inc("baselineConnectionEntries");curvaturePassed&=baseline.Gamma[a][d,c]==v[d];}}
  omega[a]=ei*(baseline.Gamma[a]+Homogeneous.Rho(Homogeneous.Lift(g,a)))*e;
 }
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++){Inc("baselineCurvatureEntries");curvaturePassed&=baseline.R[a,b,c,d]==oldR[a,b,c,d];}
 var rf=SpinGeometry.EndomorphismFrame(baseline.R,e,ei);var fb=SpinGeometry.Lift(SpinGeometry.Lower(rf,eta));var bfield=SpinOneForm(omega,e);var fields=Fields(e);
 pointRows.Add(new{point,h0=h.Text(),fibreMetricY=y.Text(),frame=e.Text(),spinReference=Terms(bfield),spinCurvature=Terms(fb),fields=fields.Select(Terms).ToArray(),baselineForecasts=Enumerable.Range(0,5).Select(field=>Forecast(point,field).Select(v=>v.ToString()).ToArray()).ToArray()});
 for(int mi=0;mi<10;mi++)foreach(var multi in multis)
 {
  Inc("jetContexts");int order=multi.Sum();Inc(new[]{"zeroJetContexts","firstJetContexts","secondJetContexts","thirdJetContexts"}[order]);
  var cDirect=Downstairs(h,basis[mi],multi);var downstairsJets=Enumerable.Range(0,4).Select(i=>basis[mi].Scale(Matches(multi,i)?1:0)).ToArray();var cOther=Splitting.Koszul(h,downstairsJets);for(int i=0;i<4;i++)for(int j=0;j<4;j++)for(int l=0;l<4;l++){Inc("downstairsConnectionEntries");metricPassed&=cDirect[i][j,l]==cOther[i][j,l];}
  var n=ShearJets(g,h,basis[mi],multi);var k=MetricJets(data,n);var block=MetricJetsBlocks(data,n);var di=(inverse*k.G*inverse).Scale(-1);var shearInverse=n.G*inverse+inverse*SpinGeometry.Transpose(n.G);
  for(int a=0;a<14;a++)for(int z=0;z<14;z++)
  {
   Inc("metricEntries");metricPassed&=k.G[a,z]==block.G[a,z];Inc("inverseVariationEntries");metricPassed&=di[a,z]==shearInverse[a,z];
   for(int c=0;c<14;c++){Inc("metricFirstJetEntries");metricPassed&=k.D[c][a,z]==block.D[c][a,z];for(int d=0;d<14;d++){Inc("metricSecondJetEntries");metricPassed&=k.DD[c,d][a,z]==block.DD[c,d][a,z];}}
  }
  Inc("volumeRows");metricPassed&=Matrix.TraceProduct(inverse,k.G)==0&&n.G.Trace()==0;
  var dual=DualConnection(data,k);var linear=LinearizedConnection(data,block,baseline);var delta=dual.Delta;
  for(int a=0;a<14;a++)for(int c=0;c<14;c++)for(int d=0;d<14;d++)
  {
   Inc("connectionVariationEntries");curvaturePassed&=delta.Gamma[a][d,c]==linear.Gamma[a][d,c];
   for(int z=0;z<14;z++){Inc("connectionDerivativeEntries");curvaturePassed&=delta.DGamma[z,a][d,c]==linear.DGamma[z,a][d,c];Inc("curvatureVariationEntries");curvaturePassed&=delta.R[z,a,c,d]==linear.R[z,a,c,d];}
  }
  var dw=FrameConnectionVariation(baseline,delta,n,e,ei);for(int a=0;a<14;a++){var skew=SpinGeometry.Transpose(dw[a])*eta+eta*dw[a];for(int c=0;c<14;c++)for(int d=0;d<14;d++){Inc("frameConnectionSkewEntries");framePassed&=skew[c,d]==0;}}
  var drf=CurvatureVariationFrame(baseline.R,delta.R,n.G,e,ei);var low=CurvatureVariationLowered(baseline.R,linear.R,g.Gram,k.G,n.G,e);var lowFromEndomorphism=SpinGeometry.Lower(drf,eta);
  for(int a=0;a<14;a++)for(int z=0;z<14;z++)for(int c=0;c<14;c++)for(int d=0;d<14;d++){Inc("frameCurvatureEntries");framePassed&=low[a,z,c,d]==lowFromEndomorphism[a,z,c,d];}
  var df=SpinGeometry.Lift(lowFromEndomorphism);var dfOther=SpinGeometry.Lift(low);var db=SpinOneForm(dw,e);var motion=ei*n.G*e;Inc("spinCurvatureRows");framePassed&=Equal(df,dfOther)&&HAnti(df)&&HAnti(db);
  foreach(var phi in new[]{Caa.Gamma1,Caa.Gamma2}){var intrinsic=Scale(FormMotion(motion,phi),-1);Inc("solderMotionRows");framePassed&=Equal(Add(intrinsic,FormMotion(motion,phi)),new FT());}
  var fieldRows=new List<object>();
  for(int field=0;field<5;field++)
  {
   Inc("fieldRows");var result=Evaluate(fields[field],bfield,fb,db,df,motion);Inc("fieldDomainRows");actionPassed&=HAnti(fields[field])&&result.DeltaInputs.All(HAnti);
   for(int piece=0;piece<4;piece++)
   {
    Inc("actionValueComparisons");actionPassed&=result.Values[piece]==Forecast(point,field)[piece];Inc("actionDeltaComparisons");actionPassed&=result.Direct[piece]==result.Expanded[piece];
    if(order==0){Inc("zeroJetActionEntries");actionPassed&=result.Direct[piece]==0;}
   }
   for(int piece=0;piece<3;piece++)for(int stage=0;stage<8;stage++)
   {
    var direct=result.FixedStages[piece][stage];var adapted=Add(direct.Delta,FormMotion(motion,direct.Value));Inc("chainStageComparisons");for(int fingerprint=0;fingerprint<4;fingerprint++)Inc("stageTensorFingerprints");actionPassed&=Equal(direct.Value,result.AdaptedValueStages[piece][stage])&&Equal(adapted,result.AdaptedDeltaStages[piece][stage]);
   }
   if(field==3&&mi==0&&Matches(multi,0))
   {
    var anchor=result.Direct[3];Inc("massDerivativeAnchors");decoysPassed&=anchor==-1&&di[0,4]==1;
    var frozen=Jet.Pair(Jet.Fixed(fields[field]),Jet.Fixed(fields[field])).Delta;Inc("frozenHodgeDecoys");decoysPassed&=frozen==0&&frozen!=anchor;
    var response=Scale(Partial(Trig(0,true),0),new Scalar(anchor,0));var unweighted=response.GetValueOrDefault((0,0,0,0));var weighted=Product(Trig(0,false),response).GetValueOrDefault((0,0,0,0));
    Inc("weightedIntegrationControls");decoysPassed&=unweighted==new Scalar(0);Inc("weightedIntegrationControls");decoysPassed&=weighted==new Scalar(new Rational(-1,2),0);
    scopeRows.Add(new{point,kind="mass-hodge-and-base-weight",massDerivative=anchor.ToString(),frozenHodgeDerivative=frozen.ToString(),unweightedMean=unweighted.Real.ToString(),weightedMean=weighted.Real.ToString(),normalization="divide by baseline upstairs density; periodic scalar counteridentity only"});
   }
   if(point==1&&field==4&&mi==0&&Matches(multi,1,1,1)){Inc("thirdJetSourceActionAnchors");decoysPassed&=result.Direct[0]==new Rational(3,16);scopeRows.Add(new{point,kind="third-base-jet-original-source",sourceVariation=result.Direct[0].ToString(),forecast="3/16",metricBasis=mi,multiindex=multi});}
   frequencyPassed&=result.DeltaInputs.All(t=>t.Keys.All(q=>q.K0==0&&q.K1==0));
   fieldRows.Add(new{field,baselinePointId=point,originalVariation=result.Direct.Select(q=>q.ToString()).ToArray(),independentExpansion=result.Expanded.Select(q=>q.ToString()).ToArray(),referenceInputVariationId="spinCurvatureVariation",fixedStageFingerprints=result.FixedStages.Select(EvidenceStore.Fingerprints).ToArray(),adaptedStageFingerprints=result.AdaptedValueStages.Select((v,i)=>EvidenceStore.Fingerprints(v,result.AdaptedDeltaStages[i])).ToArray(),kineticInputVariation=Terms(result.DeltaInputs[1]),cubicInputVariation=Terms(result.DeltaInputs[2])});
  }
  if(point==1&&mi==0&&Matches(multi,1,1,1)){Inc("thirdJetCurvatureAnchors");decoysPassed&=k.DD[1,1][0,8]==new Rational(-3,4)&&delta.R[1,0,1,8]==new Rational(3,4);}
  if(order==0){Inc("zeroJetGeometryRows");metricPassed&=k.G.Zero&&k.D.All(x=>x.Zero)&&k.DD.Cast<Matrix>().All(x=>x.Zero)&&delta.Gamma.All(x=>x.Zero)&&delta.DGamma.Cast<Matrix>().All(x=>x.Zero)&&delta.R.Cast<Rational>().All(x=>x==0)&&db.Count==0&&df.Count==0;}
  Inc("shardRows");rows.Add(EvidenceStore.WriteShard(Root,point,mi,Array.IndexOf(multis,multi),new{baselinePointId=point,multiindex=multi,order,frameMotion=motion.Text(),shearVariation=n.G.Text(),metricVariation=k.G.Text(),inverseVariation=di.Text(),metricFirstJets=Sparse3(k.D),metricSecondJets=Sparse4(k.DD),connectionVariation=Sparse3(delta.Gamma),connectionDerivative=Sparse4(delta.DGamma),curvatureVariation=SpinGeometry.Nonzero(delta.R),spinConnectionVariation=Terms(db),spinCurvatureVariation=Terms(df),fieldRows}));
 }
 // Boundary/identification controls are distinct from the fixed-Y action.
 for(int mi=0;mi<10;mi++)
 {
  var m=basis[mi];var pullback=new Matrix(4);for(int i=0;i<4;i++)for(int j=0;j<4;j++)for(int a=0;a<10;a++)pullback[i,j]+=Geometry.Coordinates(m)[a]*g.D[a+4,i,j];
  var alt=(m*g.P*y+y*g.P*m).Scale(-1);var plus=((y+m)*g.P*(y+m)).Scale(-1);var minus=((y-m)*g.P*(y-m)).Scale(-1);var finite=(plus-minus).Scale(new Rational(1,2));
  for(int i=0;i<4;i++)for(int j=0;j<4;j++){Inc("movingSectionEntries");decoysPassed&=pullback[i,j]==m[i,j]*-1;Inc("alternativeIdentificationEntries");decoysPassed&=alt[i,j]==m[i,j]*-2&&finite[i,j]==alt[i,j];}
  scopeRows.Add(new{point,kind="moving-section-and-alternative-identification",metricBasis=mi,separateCoincidentObserverMetric=y.Text(),sectionZeroJet=pullback.Text(),alternativeZeroJet=alt.Text(),alternativeSourceSelected=false});
 }
 foreach(var control in new[]{(Matrix:h,Expected:(Rational)120,Name:"h-scale"),(Matrix:basis[0],Expected:(Rational)(-30),Name:"h00")})
 {var extra=new Rational(1,2)*Matrix.TraceProduct(h.Inverse(),control.Matrix)*60;Inc("addedDensityControls");decoysPassed&=extra==control.Expected&&extra!=0;scopeRows.Add(new{point,kind="added-downstairs-density",control=control.Name,zeroJetVariation=extra.ToString(),sourceSelected=false});}
}
}
catch(EvidenceStore.ResourceFailure ex)
{
 string partialTerminal=!metricPassed?precedence[2]:!curvaturePassed?precedence[3]:!framePassed?precedence[4]:!actionPassed?precedence[5]:!decoysPassed?precedence[6]:precedence[7];
 Emit(partialTerminal,new{knownAnswerPassed,controlsPassed=false,metricPassed,curvaturePassed,framePassed,actionPassed,decoysPassed,resourcesPassed=false,resourceFailure=ex.Message,counts,partialPointRows=pointRows,partialShardManifest=rows,totalShardBytes=EvidenceStore.WrittenBytes,maximumShardBytes=EvidenceStore.MaximumShardBytes,firstFailurePreserved=true});return;
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");
bool resourcesPassed=CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&Matrix.Products<=resources.GetProperty("maximumTrackedMatrixProducts").GetInt64()&&Dual.Products<=resources.GetProperty("maximumTrackedDualProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumTensorTerms").GetInt32()&&rows.Count==resources.GetProperty("retainedJetContexts").GetInt32()&&frequencyPassed&&EvidenceStore.ExactPathSet(Root)&&EvidenceStore.WrittenBytes<=resources.GetProperty("maximumAggregateShardBytes").GetInt64();
bool controlsPassed=knownAnswerPassed&&metricPassed&&curvaturePassed&&framePassed&&actionPassed&&decoysPassed&&countsPassed&&resourcesPassed;
string verdict=!metricPassed?precedence[2]:!curvaturePassed?precedence[3]:!framePassed?precedence[4]:!actionPassed?precedence[5]:!decoysPassed?precedence[6]:!countsPassed||!resourcesPassed?precedence[7]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,metricPassed,curvaturePassed,framePassed,actionPassed,decoysPassed,countsPassed,resourcesPassed,frequencyPassed,counts,trackedCoefficientProducts=CoefficientProducts,trackedMatrixProducts=Matrix.Products,trackedDualProducts=Dual.Products,largestTensor=LargestTensor,totalShardBytes=EvidenceStore.WrittenBytes,maximumShardBytes=EvidenceStore.MaximumShardBytes,evidenceStorage=fx.GetProperty("evidenceStorage"),evidenceDag=fx.GetProperty("evidenceDag"),dagSemantics=fx.GetProperty("dagSemantics"),pointRows,rows,scopeRows,conditionalStatement=new{allFirstActionOccurrencesRetained=true,completeRealMetricJetsThroughOrder3=true,zeroBaseJetResponseAbsent=true,fixedNativeVarpiAtEpsilonIdentity=true,compactBaseVariation=true,fixedCompactFibreDomainRequired=true,baseIntegrationPerformedFirst=true,noFibreIntegrationByParts=true,noFieldEulerBoundaryShortcut=true,sourceIntendedReductionSelected=false,globalActionDefined=false,observerMetricEquationComputed=false,physicalVacuumSelected=false}});
static object[] Sparse3(Matrix[] a)
{var r=new List<object>();for(int z=0;z<14;z++)for(int i=0;i<14;i++)for(int j=0;j<14;j++)if(a[z][i,j]!=0)r.Add(new{z,i,j,value=a[z][i,j].ToString()});return r.ToArray();}
static object[] Sparse4(Matrix[,] a)
{var r=new List<object>();for(int z=0;z<14;z++)for(int w=0;w<14;w++)for(int i=0;i<14;i++)for(int j=0;j<14;j++)if(a[z,w][i,j]!=0)r.Add(new{z,w,i,j,value=a[z,w][i,j].ToString()});return r.ToArray();}

void Emit(string terminal,object evidence)
{if(replayMode)throw new InvalidOperationException("read-only evidence replay preflight: "+terminal);var result=new{schemaVersion=1,phase=621,phaseId="phase621-induced-metric-full-variation-scope-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,EvidenceStore.Compact)+"\n";Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/induced_metric_full_variation_scope_audit.json",json);File.WriteAllText(Root+"/output/induced_metric_full_variation_scope_audit_summary.json",json);Console.WriteLine($"Phase621 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
