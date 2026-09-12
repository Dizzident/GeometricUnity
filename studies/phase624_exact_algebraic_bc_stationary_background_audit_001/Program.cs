using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase624_exact_algebraic_bc_stationary_background_audit_001";
const string PointsDirectory="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/output/points";
const string OutputDirectory="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/output";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase624-a65-exact-algebraic-bc-stationary-background-v1";
const string Success="exact-algebraic-bc-controls-pass-conditional-stationary-background";
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
 "fullDiracModuleDimension": 128,
 "chiralHalfDimension": 64,
 "normalizedTraceDenominator": 128,
 "operator": "canonical untied CAA: firstC outerA innerA; Phi1=Gamma1 Phi2=Gamma2",
 "domain": "full real u(64,64), all16384 Clifford blades, both parities and central iI; no invariant response projection",
 "field": {
  "modulusAscending": [
   -17,
   -64,
   -511,
   693
  ],
  "basis": [
   "1",
   "r",
   "r^2"
  ],
  "irreducibleModulo2Ascending": [
   1,
   0,
   1,
   1
  ],
  "interval": [
   "3/4",
   "7/8"
  ],
  "endpointPolynomial": [
   "-3845/64",
   "11/512"
  ],
  "lowerDerivative": "5423/16",
  "lowerSecondDerivative": "4193/2",
  "inverseMenu": [
   "1",
   "r",
   "r^2",
   "q",
   "D",
   "P",
   "2qD",
   "14P^2",
   "b",
   "c",
   "gamma"
  ]
 },
 "parameters": {
  "q": "9r-1",
  "D": "1+42r^2",
  "P": "1+18r+36r^2",
  "b": "7P/(2qD)",
  "c": "rb",
  "gamma": "3q^2D/(14P^2)",
  "kappa": 0
 },
 "fields": {
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
  "B": "-1/2 PHGamma wedgeCl gamma_t",
  "C": "PTGamma wedgeCl gamma_t",
  "S": "bB+cC",
  "feedbackMenu": [
   "S",
   "B",
   "C",
   "BplusC"
  ],
  "actionDirections": [
   "PHGamma",
   "PTGamma",
   "PtrGamma",
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
  "W5": {
   "form": 8,
   "blade": 157,
   "real": "1"
  },
  "central": {
   "form": 1,
   "blade": 0,
   "imaginary": "1"
  }
 },
 "sourceWeights": [
  "-21/4",
  "-15/4",
  "-21/4"
 ],
 "kineticWeights": {
  "B": {
   "forward": [
    "-9/4",
    "-4",
    "-3"
   ],
   "reverse": [
    "3/4",
    "0",
    "1"
   ],
   "full": [
    "-3/4",
    "-2",
    "-1"
   ]
  },
  "C": {
   "forward": [
    "27/2",
    "16",
    "18"
   ],
   "reverse": [
    "0",
    "-2",
    "0"
   ],
   "full": [
    "27/4",
    "7",
    "9"
   ]
  }
 },
 "nonlinearWeights": {
  "monomials": [
   "b^2",
   "bc",
   "c^2"
  ],
  "horizontal": [
   "1",
   "-18",
   "48"
  ],
  "traceless": [
   "2",
   "-64/3",
   "112/3"
  ],
  "trace": [
   "2/3",
   "-24",
   "36"
  ]
 },
 "oracles": {
  "dHorizontal": "-b/2",
  "dTraceless": "c",
  "dTrace": "0",
  "Qij": "2di dj Gamma_ij",
  "adjointIt": "-2di gamma_i",
  "KQi": "2[(sumd)^2-sumd2]-4di(sumd-di)",
  "dqTrace": "-4sumd2 gamma_t"
 },
 "actionForecast": {
  "coefficient0": "0",
  "coefficient1": "gamma Pair(V,N(S))",
  "coefficient2": "0",
  "coefficient3": [
   "-16gamma",
   "-336gamma",
   "0",
   "0",
   "0",
   "0",
   "0",
   "0"
  ],
  "offRoot": {
   "field": "B",
   "gamma": 1,
   "variation": "PHGamma",
   "source": "21",
   "kinetic": "5",
   "cubic": "-4",
   "localTotal": "22",
   "eulerPairing": "20",
   "divergence": "4",
   "restrictedDensity": "0"
  }
 },
 "coreFileCount": 726,
 "exactTolerance": 0,
 "expectedCounts": {
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "fullRealDomainMasks": 16384,
  "centralDomainControls": 2,
  "fieldBasisProducts": 9,
  "fieldAssociativity": 27,
  "fieldInverseControls": 11,
  "fieldIdentityControls": 4,
  "irreducibilityControls": 2,
  "embeddingControls": 5,
  "parameterIdentities": 4,
  "contexts": 2,
  "coordinateConnectionEntries": 5488,
  "frameConnectionEntries": 5488,
  "metricSkewEntries": 5488,
  "spinLiftControls": 392,
  "fieldDefinitionControls": 4,
  "isotropyControls": 36,
  "disconnectedControls": 6,
  "cyclicControls": 8,
  "kineticResults": 22,
  "kineticDerivativeChecks": 308,
  "kineticParallelAdjointChecks": 308,
  "kineticStageChecks": 176,
  "kineticAdjointChecks": 22,
  "kineticFormChecks": 22,
  "kineticForecastRows": 18,
  "unequalLegControls": 6,
  "feedbackRows": 24,
  "feedbackProductChecks": 24,
  "feedbackAdjointChecks": 24,
  "feedbackDqChecks": 24,
  "feedbackStageChecks": 192,
  "feedbackForecastRows": 8,
  "feedbackQOracleRows": 8,
  "feedbackAdjointOracleRows": 8,
  "feedbackKQOracleRows": 8,
  "feedbackDqOracleRows": 8,
  "nonlinearGradeRows": 8,
  "residualRows": 2,
  "omissionDecoys": 6,
  "actionDirections": 16,
  "actionDirectionNorms": 16,
  "crossProductChecks": 18,
  "crossAdjointChecks": 32,
  "crossStageChecks": 144,
  "originalCubicCoefficients": 64,
  "originalCubicDerivativeCoefficients": 48,
  "nonzeroCubicControls": 4,
  "localFirstVariationRows": 16,
  "offRootActionDecoys": 2,
  "offRootRestrictedZero": 2,
  "transportRows": 9,
  "pointShards": 2
 },
 "resources": {
  "estimatedCpuSeconds": 60,
  "estimatedPeakBytes": 1073741824,
  "conservativePlanningCpuSeconds": 900,
  "conservativePlanningPeakBytes": 8589934592,
  "maximumTrackedCoefficientProducts": 100000000,
  "maximumTrackedMatrixProducts": 100000000,
  "maximumCubicProducts": 1000000,
  "maximumTensorTerms": 65536,
  "maximumRationalTextCharacters": 1024,
  "maximumFrequency": 0,
  "expectedKineticCalls": 22,
  "expectedDerivativeSlots": 308,
  "expectedConnectionActions": 724
 },
 "storage": {
  "schema": "two-complete-expanded-algebraic-point-shards-v1",
  "tensorEncoding": "numeric(form,blade,k0,k1), real[3], imaginary[3], rational strings in basis1,r,r2",
  "paths": [
   "studies/phase624_exact_algebraic_bc_stationary_background_audit_001/output/points/point0.json",
   "studies/phase624_exact_algebraic_bc_stationary_background_audit_001/output/points/point1.json"
  ],
  "maximumPointBytes": 67108864,
  "maximumAggregatePointBytes": 134217728,
  "maximumManifestBytes": 1048576
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormSelected": false,
  "sourceMetricSelected": false,
  "physicalCouplingSelected": false,
  "arbitraryPreassignedGammaSolved": false,
  "physicalVacuumSelected": false,
  "smallLambdaContinuationClaimed": false,
  "fullInvariantSolutionClassified": false,
  "pointwiseSelfAdjointnessAssumed": false,
  "jointMetricEulerComputed": false,
  "fullMixedHessianComputed": false,
  "globalSpinorDescentProved": false,
  "globalFiniteActionProved": false,
  "physicalSpectrumClaimed": false,
  "gevNormalizationSelected": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","algebraic-field-control-failed","homogeneous-connection-control-failed","full-kinetic-control-failed","full-nonlinear-control-failed","original-action-control-failed","full-stationarity-control-failed","resource-census-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/Program.cs",
 ["project"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/Phase624ExactAlgebraicBcStationaryBackgroundAudit.csproj",
 ["study"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/STUDY.md",
 ["algebraic-field-helper"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/AlgebraicField.cs",
 ["algebraic-tensor-helper"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/AlgebraicTensor.cs",
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
 ["build-props"]="Directory.Build.props",
 ["phase622-program"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/Program.cs",
 ["phase622-project"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/Phase622FullHomogeneousKineticCarrierAudit.csproj",
 ["phase622-study"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/STUDY.md",
 ["phase622-contract"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/preregistration/contract_v1.json",
 ["phase622-summary"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/output/full_homogeneous_kinetic_carrier_audit_summary.json",
 ["phase622-kinetic-helper"]="studies/phase622_full_homogeneous_kinetic_carrier_audit_001/KineticCarrier.cs",
 ["phase623-program"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/Program.cs",
 ["phase623-project"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/Phase623FullInverseKappaFifthOrderFeedbackAudit.csproj",
 ["phase623-study"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/STUDY.md",
 ["phase623-contract"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/preregistration/contract_v1.json",
 ["phase623-summary"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/output/full_inverse_kappa_fifth_order_feedback_audit_summary.json",
 ["phase623-feedback-helper"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/InverseFeedback.cs",
 ["phase623-point0"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/output/points/point0.json",
 ["phase623-point1"]="studies/phase623_full_inverse_kappa_fifth_order_feedback_audit_001/output/points/point1.json"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==624&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 var compiled=Directory.EnumerateFiles(Root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Split('/').Any(x=>x is "bin" or "obj")).Concat(System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(paths["project"]),"<Compile Include=\"([^\"]+)\"").Select(m=>Path.GetRelativePath(".",Path.GetFullPath(Path.Combine(Root,m.Groups[1].Value))))).ToArray();
 exactBindingsValid&=compiled.Length==12&&compiled.Distinct().Count()==12&&compiled.All(p=>bindings.Any(b=>b.path==p));
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase610","induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source"),("phase611","untied-caa-response-controls-pass-source-choice-open"),("phase618","homogeneous-connection-controls-pass-conditional-local-existence"),("phase622","full-homogeneous-kinetic-carrier-controls-pass-conditional-linear-branch"),("phase623","full-inverse-kappa-fifth-order-controls-pass-grade-five-branch-required")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var upstreamSummary=ud.RootElement;bool ok=upstreamSummary.GetProperty("auditPassed").GetBoolean()&&upstreamSummary.GetProperty("contractValid").GetBoolean()&&upstreamSummary.GetProperty("exactBindingsValid").GetBoolean()&&upstreamSummary.GetProperty("coreSourceTreeValid").GetBoolean()&&upstreamSummary.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&upstreamSummary.GetProperty("verdictKind").GetString()==terminal&&upstreamSummary.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&upstreamSummary.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&upstreamSummary.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>upstreamSummary.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&upstreamSummary.GetProperty("externalReviewPending").GetBoolean()&&upstreamSummary.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(3.27)","(3.34)","(9.1)","(9.4)","(12.26)","(12.27)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
using var fixtureDocument=JsonDocument.Parse(FixtureJson);var fx=fixtureDocument.RootElement;var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string key)=>counts[key]++;
bool knownAnswerPassed=true,fieldPassed=true,connectionPassed=true,kineticPassed=true,nonlinearPassed=true,actionPassed=true,stationarityPassed=true,frequencyPassed=true;int maximumRationalCharacters=1;var shards=new List<Shard>();long totalShardBytes=0;var arithmeticRows=new List<object>();
foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("arithmeticControls");knownAnswerPassed&=pass;}
for(int a=0;a<=Full;a++)
{
 foreach(int b0 in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b0)==WordSign(a,b0);Inc("wordCases");knownAnswerPassed&=BladeSign(b0,a)==WordSign(b0,a);}Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);
 Inc("hodgeCases");int d0=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d0*(14-d0)+7)%2==0?1:-1);Inc("fullRealDomainMasks");knownAnswerPassed&=HAnti(One(0,a,AdjointSign(a)==-1?1:Scalar.I));
}
foreach(bool pass in new[]{HAnti(One(0,0,Scalar.I)),Equal(Product(One(0,1,1),One(0,1,1),'A'),One(0,0,Scalar.I*2))}){Inc("centralDomainControls");knownAnswerPassed&=pass;}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
Cubic r=Cubic.R,r2=r*r,q=9*r-1,d=1+42*r2,p=1+18*r+36*r2,b=7*p*(2*q*d).Inverse(),c=r*b,gamma=3*q*q*d*(14*p*p).Inverse();
for(int i=0;i<3;i++)for(int j=0;j<3;j++){var a=Cubic.Basis(i);var z=Cubic.Basis(j);var value=a*z;var oracle=Cubic.MultiplyCompanion(a,z);Inc("fieldBasisProducts");fieldPassed&=value==oracle;arithmeticRows.Add(new{kind="basis-product",i,j,value=value.Text(),oracle=oracle.Text()});}
for(int i=0;i<3;i++)for(int j=0;j<3;j++)for(int k=0;k<3;k++){Inc("fieldAssociativity");fieldPassed&=(Cubic.Basis(i)*Cubic.Basis(j))*Cubic.Basis(k)==Cubic.Basis(i)*(Cubic.Basis(j)*Cubic.Basis(k));}
Cubic[] inverseMenu=[1,r,r2,q,d,p,2*q*d,14*p*p,b,c,gamma];for(int i=0;i<inverseMenu.Length;i++){var a=inverseMenu[i];var inverse=a.Inverse();var oracle=a.EuclideanInverse();Inc("fieldInverseControls");fieldPassed&=!a.Zero&&a*inverse==(Cubic)1&&a*oracle==(Cubic)1&&inverse==oracle;arithmeticRows.Add(new{kind="inverse",id=fx.GetProperty("field").GetProperty("inverseMenu")[i].GetString(),value=a.Text(),inverse=inverse.Text(),oracle=oracle.Text()});}
bool zeroRejected=false;try{((Cubic)0).Inverse();}catch(DivideByZeroException){zeroRejected=true;}
foreach(bool pass in new[]{(693*r*r*r-511*r*r-64*r-17).Zero,Cubic.MultiplyCompanion(r,r)==r2,(Cubic)new Rational(1,2)+(Cubic)new Rational(1,2)==(Cubic)1,zeroRejected}){Inc("fieldIdentityControls");fieldPassed&=pass;}
for(int x=0;x<2;x++){int value=(x*x*x+x*x+1)%2;Inc("irreducibilityControls");fieldPassed&=value==1;}
Rational lo=new(3,4),hi=new(7,8),flo=Polynomial(lo),fhi=Polynomial(hi),derivative=2079*lo*lo-1022*lo-64,second=4158*lo-1022;
foreach(bool pass in new[]{flo==new Rational(-3845,64),fhi==new Rational(11,512),derivative==new Rational(5423,16),second==new Rational(4193,2),lo.Numerator.Sign>0&&(9*lo-1).Numerator.Sign>0}){Inc("embeddingControls");fieldPassed&=pass;}
foreach(bool pass in new[]{2*q*d*b==7*p,c==r*b,14*p*p*gamma==3*q*q*d,gamma*b*b*d==(Cubic)new Rational(21,8)}){Inc("parameterIdentities");fieldPassed&=pass;}
if(!fieldPassed){Emit(precedence[2],new{knownAnswerPassed,fieldPassed,controlsPassed=false,counts,arithmeticRows});return;}
using var oldDocument=JsonDocument.Parse(File.ReadAllBytes(paths["phase618-summary"]));var oldRows=oldDocument.RootElement.GetProperty("evidence").GetProperty("rows");var transported=new List<AT>();
try
{
 for(int point=0;point<2;point++)
 {
  Inc("contexts");var old=oldRows[point];var frame=SpinGeometry.ReadMatrix(old.GetProperty("frame"));var inverse=SpinGeometry.HandInverse(point);connectionPassed&=frame.Same(SpinGeometry.Frame(point))&&(inverse*frame).Same(Matrix.Identity(14));
  var coordinate=new Matrix[14];var lambda=new Matrix[14];var coordinateRows=new List<object>();var spinRows=new List<object>();
  for(int a=0;a<14;a++){var row=old.GetProperty("coordinateRows")[a];var connection=SpinGeometry.ReadMatrix(row.GetProperty("coordinateConnection"));var motion=SpinGeometry.ReadMatrix(row.GetProperty("frameMotion"));coordinate[a]=connection+motion;var nominal=SpinGeometry.ReadMatrix(row.GetProperty("nomizu"));for(int i=0;i<14;i++)for(int j=0;j<14;j++){Inc("coordinateConnectionEntries");connectionPassed&=coordinate[a][i,j]==nominal[i,j];}coordinateRows.Add(new{axis=a,connection=connection.Text(),motion=motion.Text(),sum=coordinate[a].Text(),oracle=nominal.Text()});}
  for(int a=0;a<14;a++){var value=new Matrix(14);for(int i=0;i<14;i++)value+=coordinate[i].Scale(frame[i,a]);lambda[a]=inverse*value*frame;var nominal=SpinGeometry.ReadMatrix(old.GetProperty("frameNomizu")[a]);for(int i=0;i<14;i++)for(int j=0;j<14;j++){Inc("frameConnectionEntries");connectionPassed&=lambda[a][i,j]==nominal[i,j];Inc("metricSkewEntries");connectionPassed&=Sigma(i)*lambda[a][i,j]+Sigma(j)*lambda[a][j,i]==0;}var spin=Homogeneous.SpinGenerator(lambda[a]);var images=new List<object>();for(int j=0;j<14;j++){var actual=Product(spin,One(0,1<<j,1),'C');var target=new FT();for(int i=0;i<14;i++)Put(target,(0,1<<i,0,0),new Scalar(lambda[a][i,j],0));Inc("spinLiftControls");connectionPassed&=Equal(actual,target);images.Add(new{axis=j,actual=Terms(actual),expected=Terms(target)});}spinRows.Add(new{axis=a,spin=Terms(spin),images});}
  var fields=Kinetic.Fields(lambda);var tensorB=Scale(Kinetic.WedgeCl(fields[0],One(0,1<<10,1)),new Scalar(new Rational(-1,2),0));var tensorC=Kinetic.WedgeCl(fields[1],One(0,1<<10,1));Inc("fieldDefinitionControls");connectionPassed&=Equal(tensorB,fields[4]);Inc("fieldDefinitionControls");connectionPassed&=Equal(tensorC,fields[5]);
  var aB=AT.Lift(tensorB);var aC=AT.Lift(tensorC);var s=AT.Add(AT.Scale(aB,b),AT.Scale(aC,c));var source=AT.Lift(Kinetic.Read(old.GetProperty("source")));connectionPassed&=AT.Equal(source,Diagonal((Cubic)new Rational(-21,4),(Cubic)new Rational(-15,4),(Cubic)new Rational(-21,4)));
  var isotropyRows=new List<object>();foreach(var (id,t) in new[]{("B",aB),("C",aC),("S",s)}){foreach(var iso in old.GetProperty("isotropyRows").EnumerateArray()){var matrix=SpinGeometry.ReadMatrix(iso.GetProperty("frameGenerator"));var action=AT.Linear(t,x=>Kinetic.Action(matrix,x));Inc("isotropyControls");connectionPassed&=action.IsZero;isotropyRows.Add(new{id,i=iso.GetProperty("i").GetInt32(),k=iso.GetProperty("k").GetInt32(),action=AT.Terms(action)});}Inc("disconnectedControls");connectionPassed&=AT.Equal(AT.Linear(t,Homogeneous.Disconnected),t);}
  FT[] directions=[fields[0],fields[1],fields[2],fields[3],tensorB,tensorC,One(8,157,1),One(1,0,Scalar.I)];var directionKinetic=directions.Select(t=>Kinetic.Compute(lambda,t)).ToArray();foreach(var result in directionKinetic)ValidateKinetic(result);
  var sKinetic=s.Coefficients.Select(t=>Kinetic.Compute(lambda,t)).ToArray();foreach(var result in sKinetic)ValidateKinetic(result);
  var forwardS=new AT(sKinetic.Select(x=>x.Forward).ToArray());var reverseS=new AT(sKinetic.Select(x=>x.Reverse).ToArray());var hS=new AT(sKinetic.Select(x=>x.Full).ToArray());var adjointS=new AT(sKinetic.Select(x=>x.Adjoint).ToArray());
  // Full actual stationarity residual is assembled BEFORE invariant forecasts
  // or Clifford-grade inspection of the nonlinear response.
  var mainFeedback=AT.Feedback(s);var residual=AT.Add(source,AT.Add(hS,AT.Scale(mainFeedback.Full,gamma)));Inc("residualRows");stationarityPassed&=residual.IsZero;
  var omittedH=AT.Add(source,AT.Scale(mainFeedback.Full,gamma));var omittedFeedback=AT.Add(source,hS);var omittedAdjoint=AT.Add(source,AT.Add(hS,AT.Scale(mainFeedback.Stages[7],gamma*(Cubic)new Rational(1,3))));
  foreach(var omission in new[]{omittedH,omittedFeedback,omittedAdjoint}){Inc("omissionDecoys");stationarityPassed&=!omission.IsZero;}
  foreach(var (id,x,y,forward,reverse,full) in new[]{("B",(Cubic)1,(Cubic)0,AT.Lift(directionKinetic[4].Forward),AT.Lift(directionKinetic[4].Reverse),AT.Lift(directionKinetic[4].Full)),("C",(Cubic)0,(Cubic)1,AT.Lift(directionKinetic[5].Forward),AT.Lift(directionKinetic[5].Reverse),AT.Lift(directionKinetic[5].Full)),("S",b,c,forwardS,reverseS,hS)})
  {foreach(var (leg,image) in new[]{("forward",forward),("reverse",reverse),("full",full)}){var target=AT.Add(AT.Scale(Weight("B",leg),x),AT.Scale(Weight("C",leg),y));Inc("kineticForecastRows");kineticPassed&=AT.Equal(image,target)&&AT.Grades(image,1);}Inc("unequalLegControls");kineticPassed&=!AT.Equal(forward,reverse);}
  var feedbackRows=new List<object>();FeedbackResult? bFeedback=null;foreach(var (id,x,y,t,result) in new[]{("S",b,c,s,mainFeedback),("B",(Cubic)1,(Cubic)0,aB,AT.Feedback(aB)),("C",(Cubic)0,(Cubic)1,aC,AT.Feedback(aC)),("BplusC",(Cubic)1,(Cubic)1,AT.Add(aB,aC),AT.Feedback(AT.Add(aB,aC)))})
  {
   ValidateFeedback(result);if(id=="B")bFeedback=result;var oracle=FeedbackOracle(x,y);var cyclic=AT.Linear(t,Kinetic.Cyclic);Inc("cyclicControls");nonlinearPassed&=cyclic.IsZero;Inc("feedbackQOracleRows");nonlinearPassed&=AT.Equal(result.Q,oracle.Q);Inc("feedbackAdjointOracleRows");nonlinearPassed&=AT.Equal(result.Adjoint,oracle.Adjoint);Inc("feedbackKQOracleRows");nonlinearPassed&=AT.Equal(result.Stages[7],oracle.Kq);Inc("feedbackDqOracleRows");nonlinearPassed&=AT.Equal(result.Dq,oracle.Dq);Inc("feedbackForecastRows");nonlinearPassed&=AT.Equal(result.Full,NonlinearForecast(x,y));Inc("nonlinearGradeRows");nonlinearPassed&=AT.Grades(result.Full,1)&&AT.Grades(result.Stages[7],1)&&AT.Grades(result.Dq,1);
   feedbackRows.Add(new{id,b=x.Text(),c=y.Text(),result=result.Evidence(),cyclic=AT.Terms(cyclic),expectedQ=AT.Terms(oracle.Q),expectedAdjoint=AT.Terms(oracle.Adjoint),expectedKq=AT.Terms(oracle.Kq),expectedDq=AT.Terms(oracle.Dq),expectedFull=AT.Terms(NonlinearForecast(x,y))});
  }
  var actionRows=new List<object>();for(int v=0;v<directions.Length;v++)
  {
   Inc("actionDirections");var variation=AT.Lift(directions[v]);var variationFeedback=AT.Feedback(variation);ValidateFeedback(variationFeedback);Inc("actionDirectionNorms");actionPassed&=AT.Pair(variation,variation)==(Cubic)SpinGeometry.Parse(fx.GetProperty("fields").GetProperty("actionDirectionNorms")[v].GetString()!);
   var cross=AT.Cross(s,variation,mainFeedback.Adjoint,variationFeedback.Adjoint);Inc("crossProductChecks");actionPassed&=AT.Equal(cross.Q,cross.NaiveQ);Inc("crossAdjointChecks");actionPassed&=AT.Equal(cross.First,cross.FirstWord);Inc("crossAdjointChecks");actionPassed&=AT.Equal(cross.Second,cross.SecondWord);ValidateStages(cross.Stages,cross.NaiveStages,"crossStageChecks");CheckAT(cross.Full,1);
   Cubic third=gamma*(Cubic)new Rational(1,3);Cubic[] polynomial=[third*AT.Pair(s,mainFeedback.Stages[7]),third*(AT.Pair(variation,mainFeedback.Stages[7])+AT.Pair(s,cross.Stages[7])),third*(AT.Pair(variation,cross.Stages[7])+AT.Pair(s,variationFeedback.Stages[7])),third*AT.Pair(variation,variationFeedback.Stages[7])];
   Cubic expectedLinear=gamma*AT.Pair(variation,NonlinearForecast(b,c));Cubic[] forecast=[0,expectedLinear,0,v==0?-16*gamma:v==1?-336*gamma:(Cubic)0];for(int power=0;power<4;power++){Inc("originalCubicCoefficients");actionPassed&=polynomial[power]==forecast[power];Track(polynomial[power]);}
   Cubic[] derivatives=[polynomial[1],2*polynomial[2],3*polynomial[3]],gradient=[gamma*AT.Pair(variation,mainFeedback.Full),gamma*AT.Pair(variation,cross.Full),gamma*AT.Pair(variation,variationFeedback.Full)];for(int power=0;power<3;power++){Inc("originalCubicDerivativeCoefficients");actionPassed&=derivatives[power]==gradient[power];}
   if(v<2){Inc("nonzeroCubicControls");actionPassed&=!polynomial[3].Zero;}
   Cubic sourceFirst=AT.Pair(variation,source),kineticFirst=(AT.Pair(variation,forwardS)+AT.Pair(s,AT.Lift(directionKinetic[v].Forward)))*(Cubic)new Rational(1,2),totalFirst=sourceFirst+kineticFirst+polynomial[1],euler=AT.Pair(variation,residual);var current=Current(adjointS,variation);var divergence=Divergence(lambda,current);Inc("localFirstVariationRows");actionPassed&=totalFirst==euler+divergence*(Cubic)new Rational(1,2);
   actionRows.Add(new{id=fx.GetProperty("fields").GetProperty("actionDirections")[v].GetString(),variation=AT.Terms(variation),signedNorm=AT.Pair(variation,variation).Text(),variationKinetic=Kinetic.Evidence(directionKinetic[v]),variationFeedback=variationFeedback.Evidence(),cross=cross.Evidence(),coefficients=polynomial.Select(x=>x.Text()),expectedCoefficients=forecast.Select(x=>x.Text()),derivativeCoefficients=derivatives.Select(x=>x.Text()),gradientCoefficients=gradient.Select(x=>x.Text()),sourceFirst=sourceFirst.Text(),kineticFirst=kineticFirst.Text(),cubicFirst=polynomial[1].Text(),totalFirst=totalFirst.Text(),eulerPairing=euler.Text(),current=current.Select(x=>x.Text()),divergence=divergence.Text()});
  }
  var oddProbe=AT.Lift(fields[0]);var kb=directionKinetic[4];var bp=AT.Pair(oddProbe,source);var bk=(AT.Pair(oddProbe,AT.Lift(kb.Forward))+AT.Pair(aB,AT.Lift(directionKinetic[0].Forward)))*(Cubic)new Rational(1,2);var offQ=AT.Add(AT.Product(aB,oddProbe),AT.Product(oddProbe,aB));var offNaiveQ=AT.Add(AT.Product(aB,oddProbe,true),AT.Product(oddProbe,aB,true));Inc("crossProductChecks");actionPassed&=AT.Equal(offQ,offNaiveQ);var offStages=AT.Stages(offQ);var offNaiveStages=AT.Stages(offQ,true);ValidateStages(offStages,offNaiveStages,"crossStageChecks");var bcubic=(AT.Pair(oddProbe,bFeedback!.Stages[7])+AT.Pair(aB,offStages[7]))*(Cubic)new Rational(1,3);var offGradient=AT.Add(source,AT.Add(AT.Lift(kb.Full),bFeedback.Full));var offCurrent=Current(AT.Lift(kb.Adjoint),oddProbe);var offDiv=Divergence(lambda,offCurrent);Inc("offRootActionDecoys");actionPassed&=bp==(Cubic)21&&bk==(Cubic)5&&bcubic==(Cubic)(-4)&&bp+bk+bcubic==(Cubic)22&&AT.Pair(oddProbe,offGradient)==(Cubic)20&&offDiv==(Cubic)4;
  var restricted=new[]{AT.Pair(aB,source),AT.Pair(aB,AT.Lift(kb.Forward))*(Cubic)new Rational(1,2),AT.Pair(aB,bFeedback.Stages[7])*(Cubic)new Rational(1,3)};Inc("offRootRestrictedZero");actionPassed&=restricted.All(x=>x.Zero)&&!offGradient.IsZero;
  AT[] transport=[aB,aC,s,hS,mainFeedback.Full,mainFeedback.Stages[7],mainFeedback.Dq,mainFeedback.Adjoint,mainFeedback.Q];if(point==0)transported.AddRange(transport);else for(int i=0;i<transport.Length;i++){Inc("transportRows");connectionPassed&=AT.Equal(transported[i],transport[i]);}
  WriteShard(point,new{schemaVersion=1,phase=624,point,fieldBasis=new[]{"1","r","r^2"},frame=frame.Text(),inverse=inverse.Text(),coordinateRows,frameConnection=lambda.Select(x=>x.Text()),spinRows,B=AT.Terms(aB),C=AT.Terms(aC),S=AT.Terms(s),source=AT.Terms(source),isotropyRows,kineticCoefficientResults=sKinetic.Select(Kinetic.Evidence),kinetic=new{forward=AT.Terms(forwardS),reverse=AT.Terms(reverseS),full=AT.Terms(hS),adjoint=AT.Terms(adjointS)},feedbackRows,residuals=new{full=AT.Terms(residual),omittedH=AT.Terms(omittedH),omittedFeedback=AT.Terms(omittedFeedback),omittedAdjoint=AT.Terms(omittedAdjoint)},actionRows,offRoot=new{crossQ=AT.Terms(offQ),naiveCrossQ=AT.Terms(offNaiveQ),stages=offStages.Select(AT.Terms),naiveStages=offNaiveStages.Select(AT.Terms),source=bp.Text(),kinetic=bk.Text(),cubic=bcubic.Text(),localTotal=(bp+bk+bcubic).Text(),eulerPairing=AT.Pair(oddProbe,offGradient).Text(),current=offCurrent.Select(x=>x.Text()),divergence=offDiv.Text(),restrictedPieces=restricted.Select(x=>x.Text()),fullGradient=AT.Terms(offGradient)}});
 }
}
catch(ResourceLimitException ex){Emit(ScientificVerdict()==Success?precedence[8]:ScientificVerdict(),new{knownAnswerPassed,fieldPassed,connectionPassed,kineticPassed,nonlinearPassed,actionPassed,stationarityPassed,controlsPassed=false,resourcesPassed=false,resourceFailure=ex.Message,counts,shards,totalShardBytes,fullEvidenceAvailable=false});return;}
ScanRetainedRationals(JsonSerializer.SerializeToElement(new{arithmeticRows,parameterText=new[]{r.Text(),q.Text(),d.Text(),p.Text(),b.Text(),c.Text(),gamma.Text()},embedding=new[]{lo.ToString(),hi.ToString(),flo.ToString(),fhi.ToString(),derivative.ToString(),second.ToString()}}));
var allowedOutputPaths=shards.Select(x=>x.path).Concat(new[]{OutputDirectory+"/exact_algebraic_bc_stationary_background_audit.json",OutputDirectory+"/exact_algebraic_bc_stationary_background_audit_summary.json"}).ToHashSet(StringComparer.Ordinal);
bool shardSetPassed=shards.Count==2&&shards.Select(x=>x.path).SequenceEqual(fx.GetProperty("storage").GetProperty("paths").EnumerateArray().Select(x=>x.GetString()))&&Directory.EnumerateFiles(PointsDirectory,"*",SearchOption.AllDirectories).Order(StringComparer.Ordinal).SequenceEqual(shards.Select(x=>x.path).Order(StringComparer.Ordinal))&&Directory.EnumerateFiles(OutputDirectory,"*",SearchOption.AllDirectories).All(allowedOutputPaths.Contains);
bool countsPassed=counts.All(x=>x.Value==expected.GetProperty(x.Key).GetInt32());bool resourcesPassed=shardSetPassed&&frequencyPassed&&CoefficientProducts<=100000000&&Matrix.Products<=100000000&&Cubic.Products<=1000000&&LargestTensor<=65536&&maximumRationalCharacters<=1024&&Kinetic.ComputeCalls==22&&Kinetic.DerivativeSlots==308&&Kinetic.ConnectionActions==724;
bool controlsPassed=ScientificVerdict()==Success&&countsPassed&&resourcesPassed;string verdict=ScientificVerdict()!=Success?ScientificVerdict():!countsPassed||!resourcesPassed?precedence[8]:Success;
Emit(verdict,new{knownAnswerPassed,fieldPassed,connectionPassed,kineticPassed,nonlinearPassed,actionPassed,stationarityPassed,controlsPassed,countsPassed,resourcesPassed,frequencyPassed,shardSetPassed,counts,parameters=new{r=r.Text(),q=q.Text(),D=d.Text(),P=p.Text(),b=b.Text(),c=c.Text(),gamma=gamma.Text(),kappa=0},embedding=new{lower=lo.ToString(),upper=hi.ToString(),polynomialLower=flo.ToString(),polynomialUpper=fhi.ToString(),derivativeLower=derivative.ToString(),secondDerivativeLower=second.ToString(),modulo2Values=new[]{1,1}},arithmeticRows,trackedCoefficientProducts=CoefficientProducts,trackedMatrixProducts=Matrix.Products,trackedCubicProducts=Cubic.Products,largestTensor=LargestTensor,maximumRationalCharacters,kineticCalls=Kinetic.ComputeCalls,derivativeSlots=Kinetic.DerivativeSlots,connectionActions=Kinetic.ConnectionActions,storageSchema="two-complete-expanded-algebraic-point-shards-v1",shards,totalShardBytes,scope=fx.GetProperty("scope").Clone()});
string ScientificVerdict()=>!knownAnswerPassed?precedence[1]:!fieldPassed?precedence[2]:!connectionPassed?precedence[3]:!kineticPassed?precedence[4]:!nonlinearPassed?precedence[5]:!actionPassed?precedence[6]:!stationarityPassed?precedence[7]:Success;
void ValidateKinetic(Kinetic.Result x)
{
 Inc("kineticResults");Inc("kineticAdjointChecks");kineticPassed&=Equal(x.Adjoint,x.SimplifiedAdjoint);Inc("kineticFormChecks");kineticPassed&=Typed(x.Input,1)&&Typed(x.ExteriorDerivative,2)&&Typed(x.Adjoint,2)&&Typed(x.Full,1)&&Equal(x.ExteriorDerivative,x.ExteriorOracle)&&Equal(x.Reverse,x.ParallelReverse);
 for(int a=0;a<14;a++){Inc("kineticDerivativeChecks");connectionPassed&=Equal(x.Derivatives[a],x.DerivativeOracles[a]);Inc("kineticParallelAdjointChecks");kineticPassed&=Equal(x.AdjointDerivatives[a],x.ParallelAdjoints[a]);}
 int[] degrees=[2,12,13,14,0,1,13,1];for(int i=0;i<8;i++){Inc("kineticStageChecks");kineticPassed&=Equal(x.Stages[0][i],x.Stages[1][i])&&Typed(x.Stages[0][i],degrees[i])&&HAnti(x.Stages[0][i]);}
 foreach(var t in new[]{x.Input,x.ExteriorDerivative,x.ExteriorOracle,x.AdjointFirst,x.AdjointSecond,x.Adjoint,x.SimplifiedAdjoint,x.Reverse,x.ParallelReverse,x.Forward,x.Full}.Concat(x.Derivatives).Concat(x.DerivativeOracles).Concat(x.AdjointDerivatives).Concat(x.ParallelAdjoints).Concat(x.Stages.SelectMany(t=>t)))CheckFT(t);
}
void ValidateStages(AT[] actual,AT[] oracle,string counter){int[] degrees=[2,12,13,14,0,1,13,1];for(int i=0;i<8;i++){Inc(counter);nonlinearPassed&=AT.Equal(actual[i],oracle[i])&&AT.Valid(actual[i],degrees[i])&&AT.Valid(oracle[i],degrees[i]);CheckAT(actual[i],degrees[i]);CheckAT(oracle[i],degrees[i]);}}
void ValidateFeedback(FeedbackResult x){Inc("feedbackRows");Inc("feedbackProductChecks");nonlinearPassed&=AT.Equal(x.Q,x.NaiveQ);Inc("feedbackAdjointChecks");nonlinearPassed&=AT.Equal(x.Adjoint,x.SimplifiedAdjoint);Inc("feedbackDqChecks");nonlinearPassed&=AT.Equal(x.Dq,x.WordDq);ValidateStages(x.Stages,x.NaiveStages,"feedbackStageChecks");foreach(var t in new[]{x.Input,x.Dq,x.WordDq,x.Full})CheckAT(t,1);foreach(var t in new[]{x.Q,x.NaiveQ,x.AdjointFirst,x.AdjointSecond,x.Adjoint,x.SimplifiedAdjoint})CheckAT(t,2);}
void CheckFT(FT t){frequencyPassed&=t.Keys.All(k=>k.Form>=0&&k.Form<=Full&&k.Blade>=0&&k.Blade<=Full&&k.K0==0&&k.K1==0);foreach(var z in t.Values){maximumRationalCharacters=Math.Max(maximumRationalCharacters,Math.Max(z.Real.ToString().Length,z.Imaginary.ToString().Length));}}
void CheckAT(AT t,int degree){frequencyPassed&=AT.Valid(t,degree);foreach(var c0 in t.Coefficients)CheckFT(c0);}
void Track(Cubic x){foreach(var s0 in x.Text())maximumRationalCharacters=Math.Max(maximumRationalCharacters,s0.Length);}
AT Weight(string field,string leg){var values=fx.GetProperty("kineticWeights").GetProperty(field).GetProperty(leg).EnumerateArray().Select(x=>(Cubic)SpinGeometry.Parse(x.GetString()!)).ToArray();return Diagonal(values[0],values[1],values[2]);}
static AT Diagonal(Cubic h,Cubic e,Cubic trace){var r=AT.Zero();for(int a=0;a<14;a++){var q0=a==10?trace:a is 0 or 7 or 8 or 9?h:e;for(int k=0;k<3;k++)Put(r.Coefficients[k],(1<<a,1<<a,0,0),new Scalar(q0[k],0));}return r;}
static AT NonlinearForecast(Cubic b0,Cubic c0)=>Diagonal(b0*b0-18*b0*c0+48*c0*c0,2*b0*b0-(Cubic)new Rational(64,3)*b0*c0+(Cubic)new Rational(112,3)*c0*c0,(Cubic)new Rational(2,3)*b0*b0-24*b0*c0+36*c0*c0);
static (AT Q,AT Adjoint,AT Kq,AT Dq) FeedbackOracle(Cubic b0,Cubic c0)
{
 var diagonal=Enumerable.Range(0,14).Select(a=>a==10?(Cubic)0:a is 0 or 7 or 8 or 9?(Cubic)new Rational(-1,2)*b0:c0).ToArray();Cubic sum=0,squares=0;foreach(var x in diagonal){sum+=x;squares+=x*x;}var q0=AT.Zero();var ad=AT.Zero();var kq=AT.Zero();var dq=AT.Zero();
 for(int i=0;i<14;i++){for(int j=i+1;j<14;j++){var value=2*diagonal[i]*diagonal[j];for(int k=0;k<3;k++)Put(q0.Coefficients[k],((1<<i)|(1<<j),(1<<i)|(1<<j),0,0),new Scalar(value[k],0));}if(i!=10){var value=-2*diagonal[i];for(int k=0;k<3;k++)Put(ad.Coefficients[k],((1<<i)|(1<<10),1<<i,0,0),new Scalar(value[k]*Shuffle(1<<i,1<<10),0));}var f=2*(sum*sum-squares)-4*diagonal[i]*(sum-diagonal[i]);for(int k=0;k<3;k++)Put(kq.Coefficients[k],(1<<i,1<<i,0,0),new Scalar(f[k],0));}
 for(int k=0;k<3;k++)Put(dq.Coefficients[k],(1<<10,1<<10,0,0),new Scalar(-4*squares[k],0));return(q0,ad,kq,dq);
}
static Cubic[] Current(AT adjoint,AT variation)=>Enumerable.Range(0,14).Select(a=>AT.Pair(AT.Linear(adjoint,x=>Kinetic.Contract(x,a)),variation)*(Cubic)Sigma(a)).ToArray();
static Cubic Divergence(Matrix[] lambda,Cubic[] current){Cubic sum=0;for(int a=0;a<14;a++)for(int b0=0;b0<14;b0++)sum+=(Cubic)lambda[a][a,b0]*current[b0];return sum;}
static Rational Polynomial(Rational x)=>693*x*x*x-511*x*x-64*x-17;
void WriteShard(int point,object value)
{
 byte[] bytes=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)+"\n");if(bytes.LongLength>67108864||totalShardBytes+bytes.LongLength>134217728)throw new ResourceLimitException("point/aggregate byte ceiling");using(var document=JsonDocument.Parse(bytes))ScanRetainedRationals(document.RootElement);if(maximumRationalCharacters>1024)throw new ResourceLimitException("retained rational text ceiling");string path=Root+"/output/points/point"+point+".json";Directory.CreateDirectory(Root+"/output/points");File.WriteAllBytes(path,bytes);shards.Add(new(point,path,Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),bytes.LongLength));totalShardBytes+=bytes.LongLength;Inc("pointShards");
}
void ScanRetainedRationals(JsonElement value)
{if(value.ValueKind==JsonValueKind.Array){foreach(var x in value.EnumerateArray())ScanRetainedRationals(x);}else if(value.ValueKind==JsonValueKind.Object){foreach(var x in value.EnumerateObject())ScanRetainedRationals(x.Value);}else if(value.ValueKind==JsonValueKind.String){string s0=value.GetString()!;if(System.Text.RegularExpressions.Regex.IsMatch(s0,@"^-?[0-9]+(?:/[0-9]+)?$"))maximumRationalCharacters=Math.Max(maximumRationalCharacters,s0.Length);}}
void Emit(string terminal,object evidence)
{
 object Result()=>new{schemaVersion=1,phase=624,phaseId="phase624-exact-algebraic-bc-stationary-background-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(Result())+"\n";if(Encoding.UTF8.GetByteCount(json)>1048576){string originalTerminal=terminal;if(terminal==Success)terminal=precedence[8];var original=JsonSerializer.SerializeToElement(evidence);var bounded=new Dictionary<string,object?>();foreach(var x in original.EnumerateObject())if(x.Value.ValueKind is JsonValueKind.True or JsonValueKind.False or JsonValueKind.Number or JsonValueKind.String||x.Name is "counts" or "shards")bounded[x.Name]=x.Value.Clone();bounded["controlsPassed"]=false;bounded["resourcesPassed"]=false;bounded["resourceFailure"]="manifest byte ceiling";bounded["originalTerminal"]=originalTerminal;evidence=bounded;json=JsonSerializer.Serialize(Result())+"\n";}
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/exact_algebraic_bc_stationary_background_audit.json",json);File.WriteAllText(Root+"/output/exact_algebraic_bc_stationary_background_audit_summary.json",json);Console.WriteLine($"Phase624 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
internal sealed record Shard(int point,string path,string sha256,long bytes);
internal sealed class ResourceLimitException(string message):Exception(message);
