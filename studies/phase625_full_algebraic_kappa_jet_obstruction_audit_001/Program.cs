using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001";
const string PointsDirectory="studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output/points";
const string OutputDirectory="studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase625-a66-full-algebraic-kappa-jet-obstruction-v1";
const string Success="full-algebraic-kappa-jet-controls-pass-six-carrier-obstruction";
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
 "domain": "full real u(64,64), all16384 Clifford blades; six-carrier coefficients only, NO final residual projection",
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
  "S": "S0=bB+cC; S(kappa)=sum(n=0..4) kappa^n Vn, V0=S0",
  "feedbackMenu": [
   "S0",
   "V1",
   "V2",
   "V3",
   "V4",
   "PHGamma",
   "PTGamma",
   "PtrGamma",
   "J",
   "B",
   "C",
   "W5",
   "central"
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
  "embeddingControls": 6,
  "parameterIdentities": 4,
  "contexts": 2,
  "coordinateConnectionEntries": 5488,
  "frameConnectionEntries": 5488,
  "metricSkewEntries": 5488,
  "spinLiftControls": 392,
  "fieldDefinitionControls": 4,
  "upstreamPointControls": 4,
  "isotropyControls": 132,
  "disconnectedControls": 22,
  "kineticResults": 78,
  "kineticDerivativeChecks": 1092,
  "kineticParallelAdjointChecks": 1092,
  "kineticStageChecks": 624,
  "kineticAdjointChecks": 78,
  "kineticFormChecks": 78,
  "feedbackRows": 26,
  "feedbackProductChecks": 26,
  "feedbackAdjointChecks": 26,
  "feedbackDqChecks": 26,
  "feedbackStageChecks": 208,
  "crossRows": 102,
  "crossProductChecks": 102,
  "crossAdjointChecks": 204,
  "crossStageChecks": 816,
  "responseColumns": 12,
  "responseMatrixEntries": 36,
  "responseDeterminants": 4,
  "inverseMatrixEntries": 36,
  "inverseIdentityEntries": 72,
  "linearJetChecks": 8,
  "solveRows": 8,
  "solveCoefficients": 24,
  "jetParityChecks": 10,
  "firstJetControls": 2,
  "bcOnlyObstructions": 2,
  "j2Controls": 2,
  "cyclicJControls": 2,
  "grade5Anchors": 2,
  "polynomialOrderedProducts": 50,
  "polynomialOrderedAdjoints": 50,
  "polynomialStages": 144,
  "polynomialCoefficientChecks": 18,
  "residualRows": 18,
  "zeroResidualRows": 8,
  "residualGradeRows": 18,
  "fourthOrderObstructions": 2,
  "actionDirections": 16,
  "actionDirectionNorms": 16,
  "originalCubicCoefficients": 448,
  "originalCubicDerivativeCoefficients": 240,
  "cubicParityControls": 240,
  "cubicZ3Controls": 16,
  "grade5DirectionalControls": 18,
  "localFirstVariationRows": 144,
  "transportRows": 43,
  "pointShards": 8,
  "offRootActionDecoys": 2,
  "offRootRestrictedZero": 2
 },
 "resources": {
  "estimatedCpuSeconds": 180,
  "estimatedPeakBytes": 2147483648,
  "conservativePlanningCpuSeconds": 1800,
  "conservativePlanningPeakBytes": 8589934592,
  "maximumTrackedCoefficientProducts": 500000000,
  "maximumTrackedMatrixProducts": 100000000,
  "maximumCubicProducts": 10000000,
  "maximumTensorTerms": 65536,
  "maximumRationalTextCharacters": 32768,
  "maximumFrequency": 0,
  "expectedKineticCalls": 78,
  "expectedDerivativeSlots": 1092,
  "expectedConnectionActions": 2580
 },
 "storage": {
  "schema": "eight-complete-expanded-algebraic-jet-shards-v1",
  "tensorEncoding": "numeric(form,blade,k0,k1), real[3], imaginary[3], reduced rational strings in basis1,r,r2",
  "categories": [
   "context",
   "kinetic",
   "feedback",
   "cross"
  ],
  "paths": [
   "studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output/points/point0_context.json",
   "studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output/points/point0_kinetic.json",
   "studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output/points/point0_feedback.json",
   "studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output/points/point0_cross.json",
   "studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output/points/point1_context.json",
   "studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output/points/point1_kinetic.json",
   "studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output/points/point1_feedback.json",
   "studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/output/points/point1_cross.json"
  ],
  "maximumPointBytes": 67108864,
  "maximumAggregatePointBytes": 536870912,
  "maximumManifestBytes": 1048576,
  "expectedAggregateBytes": 67108864,
  "conservativePlanningAggregateBytes": 268435456
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
  "gevNormalizationSelected": false,
  "fullInvariantInverseClaimed": false,
  "nonlinearSixCarrierClosureAssumed": false,
  "unrestrictedContinuationObstructed": false,
  "fullStationaryBranchConstructed": false
 },
 "jet": {
  "maximumDegree": 4,
  "residualMaximumDegree": 8,
  "fieldIds": [
   "S0",
   "V1",
   "V2",
   "V3",
   "V4",
   "PHGamma",
   "PTGamma",
   "PtrGamma",
   "J",
   "B",
   "C",
   "W5",
   "central"
  ],
  "jetPairs": "0<=i<j<=4 (10)",
  "jetProbePairs": "0<=i<=4,0<=v<=7 (40)",
  "oddBlockRows": [
   "J",
   "B",
   "C"
  ],
  "oddBlockColumns": [
   "PHGamma",
   "PTGamma",
   "PtrGamma"
  ],
  "evenBlockRows": [
   "PHGamma",
   "PTGamma",
   "PtrGamma"
  ],
  "evenBlockColumns": [
   "J",
   "B",
   "C"
  ],
  "oddBlock": [
   [
    "-1",
    "1",
    "0"
   ],
   [
    "1+8x-72rx",
    "36x-192rx",
    "-1+4x/3-24rx"
   ],
   [
    "-1-8x+128rx/3",
    "-64x/3+224rx/3",
    "1-8x/3+8rx"
   ]
  ],
  "evenBlock": [
   [
    "9/4",
    "-3/4+x(2-18r)",
    "27/4+x(-18+96r)"
   ],
   [
    "-1",
    "-2+x(4-64r/3)",
    "7+x(-64/3+224r/3)"
   ],
   [
    "0",
    "-1+x(4/3-24r)",
    "9+x(-24+72r)"
   ]
  ],
  "x": "gamma*b=3(9r-1)/(4P)",
  "oddDeterminant": "2q(414r^3+621r^2+224r-27)/P^2",
  "evenDeterminant": "-99/4+x(129-168r)+88x^2(2r+1)(9r-2)",
  "evenDeterminantUpperBound": "-553/256",
  "firstJet": "gamma*a=3U/(8K), gamma*t=-3W/(8K), e=a",
  "U": "198r^3+81r^2-4r+3",
  "W": "1746r^3+243r^2-272r+21",
  "K": "414r^3+621r^2+224r-27",
  "refinedLower": "6/7",
  "polynomialRefinedLower": "-3731/343",
  "bcCompatibility": "det[Lb,Lc,(f,f,g)]>0; f<0<g",
  "j2Sign": "positive",
  "residual4": "gamma*j2^2*grade5(N(J))",
  "grade5Witness": "gamma*j2^2/3",
  "residualGrades": [
   [
    1
   ],
   [
    2
   ],
   [
    1
   ],
   [
    2
   ],
   [
    5
   ],
   [
    2
   ],
   [
    1,
    5
   ],
   [
    2
   ],
   [
    1,
    5
   ]
  ],
  "originalCubicDegreesByProbePower": [
   12,
   8,
   4,
   0
  ],
  "originalCubicDerivativeDegrees": [
   8,
   4,
   0
  ],
  "firstVariationDegree": 8,
  "originalCubicZ3": [
   "-16gamma",
   "-336gamma",
   "0",
   "0",
   "0",
   "0",
   "0",
   "0"
  ],
  "grade5ProbeLinearCoefficients": [
   "0",
   "0",
   "0",
   "0",
   "gamma*j2^2/3",
   "0",
   "2gamma*j2*j4/3",
   "0",
   "gamma*j4^2/3"
  ],
  "offRootPair": {
   "leftIndex": 5,
   "rightIndex": 9,
   "left": "PHGamma",
   "right": "B",
   "gamma": 1,
   "kappa": 0,
   "source": "21",
   "kinetic": "5",
   "cubic": "-4",
   "localTotal": "22",
   "eulerPairing": "20",
   "divergence": "4",
   "restrictedDensity": "0"
  }
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","algebraic-field-control-failed","homogeneous-connection-control-failed","full-kinetic-control-failed","full-nonlinear-control-failed","original-action-control-failed","full-stationarity-control-failed","resource-census-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]="studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/Program.cs",
 ["project"]="studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/Phase625FullAlgebraicKappaJetObstructionAudit.csproj",
 ["study"]="studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/STUDY.md",
 ["algebraic-field-helper"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/AlgebraicField.cs",
 ["algebraic-tensor-helper"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/AlgebraicTensor.cs",
 ["jet-helper"]="studies/phase625_full_algebraic_kappa_jet_obstruction_audit_001/JetAudit.cs",
 ["phase624-program"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/Program.cs",
 ["phase624-project"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/Phase624ExactAlgebraicBcStationaryBackgroundAudit.csproj",
 ["phase624-study"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/STUDY.md",
 ["phase624-contract"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/preregistration/contract_v1.json",
 ["phase624-summary"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/output/exact_algebraic_bc_stationary_background_audit_summary.json",
 ["phase624-point0"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/output/points/point0.json",
 ["phase624-point1"]="studies/phase624_exact_algebraic_bc_stationary_background_audit_001/output/points/point1.json",
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
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==625&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 var compiled=Directory.EnumerateFiles(Root,"*.cs",SearchOption.AllDirectories).Where(p=>!p.Split('/').Any(x=>x is "bin" or "obj")).Concat(System.Text.RegularExpressions.Regex.Matches(File.ReadAllText(paths["project"]),"<Compile Include=\"([^\"]+)\"").Select(m=>Path.GetRelativePath(".",Path.GetFullPath(Path.Combine(Root,m.Groups[1].Value))))).ToArray();
 exactBindingsValid&=compiled.Length==13&&compiled.Distinct().Count()==13&&compiled.All(p=>bindings.Any(b=>b.path==p));
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase610","induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source"),("phase611","untied-caa-response-controls-pass-source-choice-open"),("phase618","homogeneous-connection-controls-pass-conditional-local-existence"),("phase622","full-homogeneous-kinetic-carrier-controls-pass-conditional-linear-branch"),("phase623","full-inverse-kappa-fifth-order-controls-pass-grade-five-branch-required"),("phase624","exact-algebraic-bc-controls-pass-conditional-stationary-background")})
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
foreach(bool pass in new[]{flo==new Rational(-3845,64),fhi==new Rational(11,512),derivative==new Rational(5423,16),second==new Rational(4193,2),lo.Numerator.Sign>0&&(9*lo-1).Numerator.Sign>0,Polynomial(new Rational(6,7))==new Rational(-3731,343)}){Inc("embeddingControls");fieldPassed&=pass;}
foreach(bool pass in new[]{2*q*d*b==7*p,c==r*b,14*p*p*gamma==3*q*q*d,gamma*b*b*d==(Cubic)new Rational(21,8)}){Inc("parameterIdentities");fieldPassed&=pass;}
if(!fieldPassed){Emit(precedence[2],new{knownAnswerPassed,fieldPassed,controlsPassed=false,counts,arithmeticRows});return;}

using var oldDocument=JsonDocument.Parse(File.ReadAllBytes(paths["phase618-summary"]));var oldRows=oldDocument.RootElement.GetProperty("evidence").GetProperty("rows");var transported=new List<AT>();var pointSummaries=new List<object>();
try
{
 for(int point=0;point<2;point++)
 {
  Inc("contexts");var old=oldRows[point];var frame=SpinGeometry.ReadMatrix(old.GetProperty("frame"));var inverse=SpinGeometry.HandInverse(point);connectionPassed&=frame.Same(SpinGeometry.Frame(point))&&(inverse*frame).Same(Matrix.Identity(14));
  var coordinate=new Matrix[14];var lambda=new Matrix[14];var coordinateRows=new List<object>();var spinRows=new List<object>();
  for(int a0=0;a0<14;a0++){var row=old.GetProperty("coordinateRows")[a0];var connection=SpinGeometry.ReadMatrix(row.GetProperty("coordinateConnection"));var motion=SpinGeometry.ReadMatrix(row.GetProperty("frameMotion"));coordinate[a0]=connection+motion;var nominal=SpinGeometry.ReadMatrix(row.GetProperty("nomizu"));for(int i=0;i<14;i++)for(int j=0;j<14;j++){Inc("coordinateConnectionEntries");connectionPassed&=coordinate[a0][i,j]==nominal[i,j];}coordinateRows.Add(new{axis=a0,connection=connection.Text(),motion=motion.Text(),sum=coordinate[a0].Text(),oracle=nominal.Text()});}
  for(int a0=0;a0<14;a0++){var value=new Matrix(14);for(int i=0;i<14;i++)value+=coordinate[i].Scale(frame[i,a0]);lambda[a0]=inverse*value*frame;var nominal=SpinGeometry.ReadMatrix(old.GetProperty("frameNomizu")[a0]);for(int i=0;i<14;i++)for(int j=0;j<14;j++){Inc("frameConnectionEntries");connectionPassed&=lambda[a0][i,j]==nominal[i,j];Inc("metricSkewEntries");connectionPassed&=Sigma(i)*lambda[a0][i,j]+Sigma(j)*lambda[a0][j,i]==0;}var spin=Homogeneous.SpinGenerator(lambda[a0]);var images=new List<object>();for(int j=0;j<14;j++){var actual=Product(spin,One(0,1<<j,1),'C');var target=new FT();for(int i=0;i<14;i++)Put(target,(0,1<<i,0,0),new Scalar(lambda[a0][i,j],0));Inc("spinLiftControls");connectionPassed&=Equal(actual,target);images.Add(new{axis=j,actual=Terms(actual),expected=Terms(target)});}spinRows.Add(new{axis=a0,spin=Terms(spin),images});}
  var oldFields=Kinetic.Fields(lambda);var tensorB=Scale(Kinetic.WedgeCl(oldFields[0],One(0,1<<10,1)),new Scalar(new Rational(-1,2),0));var tensorC=Kinetic.WedgeCl(oldFields[1],One(0,1<<10,1));Inc("fieldDefinitionControls");connectionPassed&=Equal(tensorB,oldFields[4]);Inc("fieldDefinitionControls");connectionPassed&=Equal(tensorC,oldFields[5]);
  AT[] probes=[AT.Lift(oldFields[0]),AT.Lift(oldFields[1]),AT.Lift(oldFields[2]),AT.Lift(oldFields[3]),AT.Lift(tensorB),AT.Lift(tensorC),AT.Lift(One(8,157,1)),AT.Lift(One(1,0,Scalar.I))];AT[] vectorBasis=probes.Take(3).ToArray(),bivectorBasis=probes.Skip(3).Take(3).ToArray();Rational[] vectorNorms=[-4,-9,-1],bivectorNorms=[9,-1,-9];
  var s0=AT.Combine([probes[4],probes[5]],b,c);var source=AT.Lift(Kinetic.Read(old.GetProperty("source")));connectionPassed&=AT.Equal(source,AT.Combine(vectorBasis,(Cubic)new Rational(-21,4),(Cubic)new Rational(-15,4),(Cubic)new Rational(-21,4)));
  using var passedDocument=JsonDocument.Parse(File.ReadAllBytes(paths["phase624-point"+point]));var passed=passedDocument.RootElement;Inc("upstreamPointControls");connectionPassed&=passed.GetProperty("point").GetInt32()==point&&AT.Equal(s0,ReadAlgebraic(passed.GetProperty("S")));Inc("upstreamPointControls");connectionPassed&=AT.Equal(source,ReadAlgebraic(passed.GetProperty("source")));
  var fields=new JetField?[13];var crosses=new Dictionary<(int,int),CrossResult>();
  JetField Row(int id)=>fields[id]??throw new InvalidOperationException("jet dependency not constructed");
  void Make(int id,AT input){if(fields[id]!=null)throw new InvalidOperationException("duplicate field row");var kinetic=input.Coefficients.Select(t=>Kinetic.Compute(lambda,t)).ToArray();foreach(var kr in kinetic)ValidateKinetic(kr);var feedback=AT.Feedback(input);ValidateFeedback(feedback);fields[id]=new(fx.GetProperty("jet").GetProperty("fieldIds")[id].GetString()!,input,kinetic,feedback);}
  CrossResult Cross(int left,int right){if(left==right)throw new InvalidOperationException("self is not a cross");var key=left<right?(left,right):(right,left);if(!crosses.TryGetValue(key,out var result)){result=AT.Cross(Row(key.Item1).Input,Row(key.Item2).Input,Row(key.Item1).Feedback.Adjoint,Row(key.Item2).Feedback.Adjoint);ValidateCross(result);crosses.Add(key,result);}return result;}
  for(int v=0;v<8;v++)Make(v+5,probes[v]);Make(0,s0);
  // Compute ALL full linearized columns first, with actual differential and
  // nonlinear terms. The displayed hand matrices are comparison targets only.
  var responses=new AT[6];var responseRows=new List<object>();for(int v=0;v<6;v++){var cross=Cross(0,v+5);responses[v]=AT.Add(Row(v+5).H,AT.Scale(cross.Full,gamma));Inc("responseColumns");CheckAT(responses[v],1);responseRows.Add(new{id=Row(v+5).Id,input=AT.Terms(probes[v]),h=AT.Terms(Row(v+5).H),cross=cross.Evidence(),full=AT.Terms(responses[v])});}
  var odd=JetMatrix.Columns(responses.Take(3).Select(t=>JetTools.Coordinates(t,bivectorBasis,bivectorNorms)).ToArray());var even=JetMatrix.Columns(responses.Skip(3).Select(t=>JetTools.Coordinates(t,vectorBasis,vectorNorms)).ToArray());
  Cubic x=gamma*b;var oddForecast=new JetMatrix();var evenForecast=new JetMatrix();
  Cubic[,] fo={{-1,1,0},{1+8*x-72*r*x,36*x-192*r*x,-1+(Cubic)new Rational(4,3)*x-24*r*x},{-1-8*x+(Cubic)new Rational(128,3)*r*x,(Cubic)new Rational(-64,3)*x+(Cubic)new Rational(224,3)*r*x,1-(Cubic)new Rational(8,3)*x+8*r*x}};
  Cubic[,] fe={{(Cubic)new Rational(9,4),(Cubic)new Rational(-3,4)+x*(2-18*r),(Cubic)new Rational(27,4)+x*(-18+96*r)},{-1,-2+x*(4-(Cubic)new Rational(64,3)*r),7+x*((Cubic)new Rational(-64,3)+(Cubic)new Rational(224,3)*r)},{0,-1+x*((Cubic)new Rational(4,3)-24*r),9+x*(-24+72*r)}};
  for(int i=0;i<3;i++)for(int j=0;j<3;j++){oddForecast[i,j]=fo[i,j];evenForecast[i,j]=fe[i,j];Inc("responseMatrixEntries");stationarityPassed&=odd[i,j]==fo[i,j];Inc("responseMatrixEntries");stationarityPassed&=even[i,j]==fe[i,j];}
  for(int j=0;j<3;j++){stationarityPassed&=AT.Equal(responses[j],JetTools.From(bivectorBasis,Enumerable.Range(0,3).Select(i=>odd[i,j]).ToArray()));stationarityPassed&=AT.Equal(responses[j+3],JetTools.From(vectorBasis,Enumerable.Range(0,3).Select(i=>even[i,j]).ToArray()));}
  Cubic bigK=414*r*r*r+621*r*r+224*r-27,bigU=198*r*r*r+81*r*r-4*r+3,bigW=1746*r*r*r+243*r*r-272*r+21;
  Cubic oddDet=odd.Determinant(),evenDet=even.Determinant(),oddDetForecast=2*q*bigK*(p*p).Inverse(),evenDetForecast=(Cubic)new Rational(-99,4)+x*(129-168*r)+88*x*x*(2*r+1)*(9*r-2);
  Inc("responseDeterminants");stationarityPassed&=!oddDet.Zero&&oddDet==oddDetForecast;Inc("responseDeterminants");stationarityPassed&=!evenDet.Zero&&evenDet==evenDetForecast;
  var oddInverse=odd.AdjugateInverse();var evenInverse=even.AdjugateInverse();var oddOracle=odd.GaussianInverse();var evenOracle=even.GaussianInverse();var oddLeft=odd*oddInverse;var oddRight=oddInverse*odd;var evenLeft=even*evenInverse;var evenRight=evenInverse*even;
  for(int i=0;i<3;i++)for(int j=0;j<3;j++){foreach(var pair in new[]{(oddInverse,oddOracle),(evenInverse,evenOracle)}){Inc("inverseMatrixEntries");fieldPassed&=pair.Item1[i,j]==pair.Item2[i,j];}foreach(var matrix in new[]{oddLeft,oddRight,evenLeft,evenRight}){Inc("inverseIdentityEntries");fieldPassed&=matrix[i,j]==(i==j?(Cubic)1:(Cubic)0);}}
  var solveRows=new List<object>();var jetCoordinates=new Cubic[5][];jetCoordinates[0]=[0,b,c];
  for(int n=1;n<=4;n++)
  {
   var forcing=n switch{1=>s0,2=>AT.Add(Row(1).Input,AT.Scale(Row(1).Feedback.Full,gamma)),3=>AT.Add(Row(2).Input,AT.Scale(Cross(1,2).Full,gamma)),_=>AT.Add(Row(3).Input,AT.Scale(AT.Add(Cross(1,3).Full,Row(2).Feedback.Full),gamma))};
   var projected=n==4?JetTools.Grade(forcing,1):forcing;var targetBasis=n%2==1?bivectorBasis:vectorBasis;var targetNorms=n%2==1?bivectorNorms:vectorNorms;var inputBasis=n%2==1?vectorBasis:bivectorBasis;var matrix=n%2==1?odd:even;var matrixInverse=n%2==1?oddInverse:evenInverse;var rhs=JetTools.Coordinates(AT.Scale(projected,-1),targetBasis,targetNorms);var coefficients=matrixInverse.Apply(rhs);var reconstructed=matrix.Apply(coefficients);
   Inc("solveRows");stationarityPassed&=AT.Equal(projected,AT.Scale(JetTools.From(targetBasis,rhs),-1));for(int i=0;i<3;i++){Inc("solveCoefficients");stationarityPassed&=reconstructed[i]==rhs[i];}jetCoordinates[n]=coefficients;Make(n,JetTools.From(inputBasis,coefficients));var actual=AT.Add(Row(n).H,AT.Scale(Cross(0,n).Full,gamma));Inc("linearJetChecks");stationarityPassed&=AT.Equal(actual,JetTools.From(targetBasis,reconstructed));
   solveRows.Add(new{order=n,forcing=AT.Terms(forcing),projectedForCoefficientSolveOnly=AT.Terms(projected),rightHandSide=rhs.Select(z=>z.Text()),coefficients=coefficients.Select(z=>z.Text()),actualLinearResponse=AT.Terms(actual),linearSolveResidual=AT.Terms(AT.Add(actual,projected))});
  }
  Cubic alpha=3*bigU*(8*bigK).Inverse(),tau=-3*bigW*(8*bigK).Inverse(),f=alpha+264*alpha*alpha+48*alpha*tau,g=tau+312*alpha*alpha;
  Inc("firstJetControls");stationarityPassed&=gamma*jetCoordinates[1][0]==alpha&&gamma*jetCoordinates[1][1]==alpha&&gamma*jetCoordinates[1][2]==tau;
  var bcMatrix=JetMatrix.Columns([Enumerable.Range(0,3).Select(i=>even[i,1]).ToArray(),Enumerable.Range(0,3).Select(i=>even[i,2]).ToArray(),new[]{f,f,g}]);var bcDet=bcMatrix.Determinant();Cubic u=even[0,1]-even[1,1],v0=even[0,2]-even[1,2],m=v0*even[1,1]-u*even[1,2],nn=v0*even[2,1]-u*even[2,2];Inc("bcOnlyObstructions");stationarityPassed&=!bcDet.Zero&&bcDet==nn*f-m*g;var j2=jetCoordinates[2][0];var j4=jetCoordinates[4][0];Inc("j2Controls");stationarityPassed&=!j2.Zero&&gamma*evenDet*j2+bcDet==(Cubic)0;
  var jFeedback=Row(8).Feedback;var nj5=JetTools.Grade(jFeedback.Full,5);Inc("cyclicJControls");nonlinearPassed&=AT.Linear(probes[3],Kinetic.Cyclic).IsZero&&AT.Grades(jFeedback.Adjoint,1)&&Cross(0,8).Full.IsZero;Inc("grade5Anchors");nonlinearPassed&=JetTools.Component(nj5,8,157)==(Cubic)new Rational(1,3)&&AT.Pair(probes[6],probes[6])==(Cubic)1&&!nj5.IsZero;
  for(int i=0;i<5;i++)for(int j=i+1;j<5;j++)Cross(i,j);for(int i=0;i<5;i++)for(int v=0;v<8;v++)Cross(i,v+5);
  var isotropyRows=new List<object>();for(int id=0;id<11;id++){var input=Row(id).Input;foreach(var iso in old.GetProperty("isotropyRows").EnumerateArray()){var matrix=SpinGeometry.ReadMatrix(iso.GetProperty("frameGenerator"));var action=AT.Linear(input,t=>Kinetic.Action(matrix,t));Inc("isotropyControls");connectionPassed&=action.IsZero;isotropyRows.Add(new{id=Row(id).Id,i=iso.GetProperty("i").GetInt32(),k=iso.GetProperty("k").GetInt32(),action=AT.Terms(action)});}Inc("disconnectedControls");connectionPassed&=AT.Equal(AT.Linear(input,Homogeneous.Disconnected),input);}
  for(int n=0;n<5;n++){Inc("jetParityChecks");stationarityPassed&=AT.Grades(Row(n).Input,n%2==0?2:1);}
  // Independent literal ordered polynomial multiplication, not reconstruction
  // from response forecasts or even from the cached self/cross table.
  var polyQ=Enumerable.Range(0,9).Select(_=>AT.Zero()).ToArray();var naiveQ=Enumerable.Range(0,9).Select(_=>AT.Zero()).ToArray();var polyDq=Enumerable.Range(0,9).Select(_=>AT.Zero()).ToArray();var wordDq=Enumerable.Range(0,9).Select(_=>AT.Zero()).ToArray();
  for(int i=0;i<5;i++)for(int j=0;j<5;j++){Inc("polynomialOrderedProducts");polyQ[i+j]=AT.Add(polyQ[i+j],AT.Product(Row(i).Input,Row(j).Input));naiveQ[i+j]=AT.Add(naiveQ[i+j],AT.Product(Row(i).Input,Row(j).Input,true));Inc("polynomialOrderedAdjoints");polyDq[i+j]=AT.Add(polyDq[i+j],AT.DQAdjoint(Row(i).Input,Row(j).Feedback.Adjoint));wordDq[i+j]=AT.Add(wordDq[i+j],AT.DQAdjoint(Row(i).Input,Row(j).Feedback.Adjoint,true));}
  var polyK=new AT[9];var polyN=new AT[9];var residuals=new AT[9];var polynomialRows=new List<object>();var residualRows=new List<object>();
  for(int n=0;n<9;n++)
  {
   var stages=AT.Stages(polyQ[n]);var naive=AT.Stages(naiveQ[n],true);ValidateStages(stages,naive,"polynomialStages");polyK[n]=stages[7];polyN[n]=AT.Scale(AT.Add(polyK[n],polyDq[n]),(Cubic)new Rational(1,3));var cache=AT.Zero();for(int i=0;i<5;i++){if(2*i==n)cache=AT.Add(cache,Row(i).Feedback.Full);for(int j=i+1;j<5;j++)if(i+j==n)cache=AT.Add(cache,Cross(i,j).Full);}
   Inc("polynomialCoefficientChecks");nonlinearPassed&=AT.Equal(polyQ[n],naiveQ[n])&&AT.Equal(polyDq[n],wordDq[n])&&AT.Equal(polyN[n],cache);CheckAT(polyQ[n],2);CheckAT(polyDq[n],1);CheckAT(polyN[n],1);
   var residual=AT.Scale(polyN[n],gamma);if(n==0)residual=AT.Add(residual,source);if(n<5)residual=AT.Add(residual,Row(n).H);if(n>0&&n<=5)residual=AT.Add(residual,Row(n-1).Input);residuals[n]=residual;Inc("residualRows");CheckAT(residual,1);if(n<4){Inc("zeroResidualRows");stationarityPassed&=residual.IsZero;}Inc("residualGradeRows");stationarityPassed&=AT.Grades(residual,fx.GetProperty("jet").GetProperty("residualGrades")[n].EnumerateArray().Select(z=>z.GetInt32()).ToArray());
   polynomialRows.Add(new{order=n,q=AT.Terms(polyQ[n]),naiveQ=AT.Terms(naiveQ[n]),stages=stages.Select(AT.Terms),naiveStages=naive.Select(AT.Terms),dq=AT.Terms(polyDq[n]),wordDq=AT.Terms(wordDq[n]),full=AT.Terms(polyN[n]),cacheOracle=AT.Terms(cache)});residualRows.Add(new{order=n,full=AT.Terms(residual)});
  }
  var expectedR4=AT.Scale(nj5,gamma*j2*j2);Inc("fourthOrderObstructions");stationarityPassed&=!residuals[4].IsZero&&AT.Equal(residuals[4],expectedR4)&&JetTools.Component(residuals[4],8,157)==gamma*j2*j2*(Cubic)new Rational(1,3);
  var actionRows=new List<object>();for(int pv=0;pv<8;pv++)
  {
   var probe=probes[pv];var probeRow=Row(pv+5);Inc("actionDirections");Inc("actionDirectionNorms");actionPassed&=AT.Pair(probe,probe)==(Cubic)SpinGeometry.Parse(fx.GetProperty("fields").GetProperty("actionDirectionNorms")[pv].GetString()!);
   var original=new[]{new Cubic[13],new Cubic[9],new Cubic[5],new Cubic[1]};Cubic third=gamma*(Cubic)new Rational(1,3);
   for(int n=0;n<13;n++)for(int i=0;i<5;i++)if(n-i>=0&&n-i<9)original[0][n]+=third*AT.Pair(Row(i).Input,polyK[n-i]);
   for(int n=0;n<9;n++){original[1][n]=third*AT.Pair(probe,polyK[n]);for(int i=0;i<5;i++)for(int j=0;j<5;j++)if(i+j==n)original[1][n]+=third*AT.Pair(Row(i).Input,Cross(j,pv+5).Stages[7]);}
   for(int n=0;n<5;n++)original[2][n]=third*(AT.Pair(probe,Cross(n,pv+5).Stages[7])+AT.Pair(Row(n).Input,probeRow.Feedback.Stages[7]));original[3][0]=third*AT.Pair(probe,probeRow.Feedback.Stages[7]);
   int probeParity=pv is 0 or 1 or 2 or 6?1:0;for(int z=0;z<4;z++)for(int n=0;n<original[z].Length;n++){Inc("originalCubicCoefficients");Track(original[z][n]);if((n+z*probeParity)%2==0){Inc("cubicParityControls");actionPassed&=original[z][n].Zero;}}
   var derivativeRows=new List<object>();for(int z=0;z<3;z++)for(int n=0;n<original[z+1].Length;n++){var cubicDerivative=(Cubic)(z+1)*original[z+1][n];var gradient=gamma*AT.Pair(probe,z==0?polyN[n]:z==1?Cross(n,pv+5).Full:probeRow.Feedback.Full);Inc("originalCubicDerivativeCoefficients");actionPassed&=cubicDerivative==gradient;derivativeRows.Add(new{probePower=z,order=n,derivative=cubicDerivative.Text(),gradient=gradient.Text()});}
   Inc("cubicZ3Controls");actionPassed&=original[3][0]==(pv==0?-16*gamma:pv==1?-336*gamma:(Cubic)0);
   if(pv==6){Cubic[] expectedWitness=[0,0,0,0,gamma*j2*j2*(Cubic)new Rational(1,3),0,2*gamma*j2*j4*(Cubic)new Rational(1,3),0,gamma*j4*j4*(Cubic)new Rational(1,3)];for(int n=0;n<9;n++){Inc("grade5DirectionalControls");actionPassed&=original[1][n]==expectedWitness[n];}}
   var firstRows=new List<object>();for(int n=0;n<9;n++){Cubic sourceFirst=n==0?AT.Pair(probe,source):(Cubic)0,kineticFirst=n<5?(AT.Pair(probe,Row(n).Forward)+AT.Pair(Row(n).Input,probeRow.Forward))*(Cubic)new Rational(1,2):(Cubic)0,massFirst=n>0&&n<=5?AT.Pair(probe,Row(n-1).Input):(Cubic)0;var current=n<5?JetTools.Current(Row(n).Adjoint,probe):new Cubic[14];var divergence=JetTools.Divergence(lambda,current);var total=sourceFirst+kineticFirst+original[1][n]+massFirst;var euler=AT.Pair(probe,residuals[n]);Inc("localFirstVariationRows");actionPassed&=total==euler+divergence*(Cubic)new Rational(1,2);firstRows.Add(new{order=n,source=sourceFirst.Text(),kinetic=kineticFirst.Text(),cubic=original[1][n].Text(),mass=massFirst.Text(),localTotal=total.Text(),eulerPairing=euler.Text(),current=current.Select(z=>z.Text()),divergence=divergence.Text()});}
   actionRows.Add(new{id=probeRow.Id,variation=AT.Terms(probe),signedNorm=AT.Pair(probe,probe).Text(),originalCubic=original.Select(row=>row.Select(z=>z.Text())),derivativeRows,firstVariationRows=firstRows});
  }
  var offCross=Cross(5,9);var offB=Row(9);var offProbe=Row(5);var offSource=AT.Pair(offProbe.Input,source);var offKinetic=(AT.Pair(offProbe.Input,offB.Forward)+AT.Pair(offB.Input,offProbe.Forward))*(Cubic)new Rational(1,2);var offCubic=(AT.Pair(offProbe.Input,offB.Feedback.Stages[7])+AT.Pair(offB.Input,offCross.Stages[7]))*(Cubic)new Rational(1,3);var offGradient=AT.Add(source,AT.Add(offB.H,offB.Feedback.Full));var offEuler=AT.Pair(offProbe.Input,offGradient);var offCurrent=JetTools.Current(offB.Adjoint,offProbe.Input);var offDivergence=JetTools.Divergence(lambda,offCurrent);Inc("offRootActionDecoys");actionPassed&=offSource==(Cubic)21&&offKinetic==(Cubic)5&&offCubic==(Cubic)(-4)&&offSource+offKinetic+offCubic==(Cubic)22&&offEuler==(Cubic)20&&offDivergence==(Cubic)4;Cubic[] restricted=[AT.Pair(offB.Input,source),AT.Pair(offB.Input,offB.Forward)*(Cubic)new Rational(1,2),AT.Pair(offB.Input,offB.Feedback.Stages[7])*(Cubic)new Rational(1,3)];Inc("offRootRestrictedZero");actionPassed&=restricted.All(z=>z.Zero)&&!offGradient.IsZero;var offRoot=new{field="B",variation="PHGamma",gamma=1,kappa=0,cross=offCross.Evidence(),source=offSource.Text(),kinetic=offKinetic.Text(),cubic=offCubic.Text(),localTotal=(offSource+offKinetic+offCubic).Text(),fullGradient=AT.Terms(offGradient),eulerPairing=offEuler.Text(),current=offCurrent.Select(z=>z.Text()),divergence=offDivergence.Text(),restrictedPieces=restricted.Select(z=>z.Text())};
  AT[] transport=Enumerable.Range(0,5).Select(i=>Row(i).Input).Concat(residuals).Concat(responses).Concat(polyN).Concat(polyQ).Concat(Enumerable.Range(0,5).Select(i=>Row(i).H)).ToArray();if(point==0)transported.AddRange(transport);else for(int i=0;i<transport.Length;i++){Inc("transportRows");connectionPassed&=AT.Equal(transported[i],transport[i]);}
  pointSummaries.Add(new{point,oddDeterminant=oddDet.Text(),evenDeterminant=evenDet.Text(),bcOnlyDeterminant=bcDet.Text(),j2=j2.Text(),j4=j4.Text(),grade5Witness=JetTools.Component(residuals[4],8,157).Text(),zeroResidualOrders=new[]{0,1,2,3},nonzeroFourthOrder=!residuals[4].IsZero});
  WriteShard(point,"context",new{schemaVersion=1,phase=625,point,category="context",fieldBasis=new[]{"1","r","r^2"},frame=frame.Text(),inverse=inverse.Text(),coordinateRows,frameConnection=lambda.Select(z=>z.Text()),spinRows,source=AT.Terms(source),isotropyRows,responseRows,oddMatrix=odd.Text(),evenMatrix=even.Text(),oddForecast=oddForecast.Text(),evenForecast=evenForecast.Text(),oddInverse=oddInverse.Text(),evenInverse=evenInverse.Text(),oddInverseOracle=oddOracle.Text(),evenInverseOracle=evenOracle.Text(),oddDeterminant=oddDet.Text(),evenDeterminant=evenDet.Text(),solveRows,jetRows=Enumerable.Range(0,5).Select(i=>new{order=i,id=Row(i).Id,full=AT.Terms(Row(i).Input),coordinates=jetCoordinates[i].Select(z=>z.Text())}),firstJet=new{alpha=alpha.Text(),tau=tau.Text(),f=f.Text(),g=g.Text()},bcOnly=new{matrix=bcMatrix.Text(),determinant=bcDet.Text(),u=u.Text(),v=v0.Text(),m=m.Text(),n=nn.Text()},j2=j2.Text(),j4=j4.Text(),grade5J=AT.Terms(nj5),expectedFourthOrder=AT.Terms(expectedR4),polynomialRows,residualRows,actionRows,offRoot});
  WriteShard(point,"kinetic",new{schemaVersion=1,phase=625,point,category="kinetic",fieldBasis=new[]{"1","r","r^2"},rows=Enumerable.Range(0,13).Select(i=>Row(i).KineticEvidence())});
  WriteShard(point,"feedback",new{schemaVersion=1,phase=625,point,category="feedback",fieldBasis=new[]{"1","r","r^2"},rows=Enumerable.Range(0,13).Select(i=>Row(i).FeedbackEvidence())});
  WriteShard(point,"cross",new{schemaVersion=1,phase=625,point,category="cross",fieldBasis=new[]{"1","r","r^2"},rows=crosses.OrderBy(z=>z.Key.Item1).ThenBy(z=>z.Key.Item2).Select(z=>new{left=Row(z.Key.Item1).Id,right=Row(z.Key.Item2).Id,leftIndex=z.Key.Item1,rightIndex=z.Key.Item2,result=z.Value.Evidence()})});
 }
}
catch(ResourceLimitException ex){Emit(ScientificVerdict()==Success?precedence[8]:ScientificVerdict(),new{knownAnswerPassed,fieldPassed,connectionPassed,kineticPassed,nonlinearPassed,actionPassed,stationarityPassed,controlsPassed=false,resourcesPassed=false,resourceFailure=ex.Message,counts,shards,totalShardBytes,fullEvidenceAvailable=false});return;}
catch(Exception ex) when(ex is InvalidOperationException or DivideByZeroException or ArgumentException){stationarityPassed=false;Emit(ScientificVerdict(),new{knownAnswerPassed,fieldPassed,connectionPassed,kineticPassed,nonlinearPassed,actionPassed,stationarityPassed,controlsPassed=false,error=ex.GetType().Name,detail=ex.Message,counts,shards,totalShardBytes,fullEvidenceAvailable=false});return;}
ScanRetainedRationals(JsonSerializer.SerializeToElement(new{arithmeticRows,parameterText=new[]{r.Text(),q.Text(),d.Text(),p.Text(),b.Text(),c.Text(),gamma.Text()},pointSummaries}));
var allowedOutputPaths=shards.Select(z=>z.path).Concat(new[]{OutputDirectory+"/full_algebraic_kappa_jet_obstruction_audit.json",OutputDirectory+"/full_algebraic_kappa_jet_obstruction_audit_summary.json"}).ToHashSet(StringComparer.Ordinal);
bool shardSetPassed=shards.Count==8&&shards.Select(z=>z.path).SequenceEqual(fx.GetProperty("storage").GetProperty("paths").EnumerateArray().Select(z=>z.GetString()))&&Directory.EnumerateFiles(PointsDirectory,"*",SearchOption.AllDirectories).Order(StringComparer.Ordinal).SequenceEqual(shards.Select(z=>z.path).Order(StringComparer.Ordinal))&&Directory.EnumerateFiles(OutputDirectory,"*",SearchOption.AllDirectories).All(allowedOutputPaths.Contains);
bool countsPassed=counts.All(z=>z.Value==expected.GetProperty(z.Key).GetInt32());bool resourcesPassed=shardSetPassed&&frequencyPassed&&CoefficientProducts<=500000000&&Matrix.Products<=100000000&&Cubic.Products<=10000000&&LargestTensor<=65536&&maximumRationalCharacters<=32768&&Kinetic.ComputeCalls==78&&Kinetic.DerivativeSlots==1092&&Kinetic.ConnectionActions==2580;
bool controlsPassed=ScientificVerdict()==Success&&countsPassed&&resourcesPassed;string verdict=ScientificVerdict()!=Success?ScientificVerdict():!countsPassed||!resourcesPassed?precedence[8]:Success;
Emit(verdict,new{knownAnswerPassed,fieldPassed,connectionPassed,kineticPassed,nonlinearPassed,actionPassed,stationarityPassed,controlsPassed,countsPassed,resourcesPassed,frequencyPassed,shardSetPassed,counts,parameters=new{r=r.Text(),q=q.Text(),D=d.Text(),P=p.Text(),b=b.Text(),c=c.Text(),gamma=gamma.Text(),kappaExpansionPoint=0},pointSummaries,arithmeticRows,trackedCoefficientProducts=CoefficientProducts,trackedMatrixProducts=Matrix.Products,trackedCubicProducts=Cubic.Products,largestTensor=LargestTensor,maximumRationalCharacters,kineticCalls=Kinetic.ComputeCalls,derivativeSlots=Kinetic.DerivativeSlots,connectionActions=Kinetic.ConnectionActions,storageSchema="eight-complete-expanded-algebraic-jet-shards-v1",shards,totalShardBytes,scope=fx.GetProperty("scope").Clone()});
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
void ValidateCross(CrossResult x){Inc("crossRows");Inc("crossProductChecks");nonlinearPassed&=AT.Equal(x.Q,x.NaiveQ);Inc("crossAdjointChecks");nonlinearPassed&=AT.Equal(x.First,x.FirstWord);Inc("crossAdjointChecks");nonlinearPassed&=AT.Equal(x.Second,x.SecondWord);ValidateStages(x.Stages,x.NaiveStages,"crossStageChecks");foreach(var t in new[]{x.First,x.Second,x.FirstWord,x.SecondWord,x.Full})CheckAT(t,1);CheckAT(x.Q,2);CheckAT(x.NaiveQ,2);}
void Track(Cubic x){foreach(var s0 in x.Text())maximumRationalCharacters=Math.Max(maximumRationalCharacters,s0.Length);}

static AT ReadAlgebraic(JsonElement rows)
{
 var result=AT.Zero();(int,int,int,int)? previous=null;foreach(var row in rows.EnumerateArray())
 {
  var key=(row.GetProperty("form").GetInt32(),row.GetProperty("blade").GetInt32(),row.GetProperty("k0").GetInt32(),row.GetProperty("k1").GetInt32());if(previous!=null&&previous.Value.CompareTo(key)>=0)throw new ArgumentException("ordered unique tensor rows");previous=key;
  if(key.Item1<0||key.Item1>Full||key.Item2<0||key.Item2>Full||key.Item3!=0||key.Item4!=0||row.GetProperty("real").GetArrayLength()!=3||row.GetProperty("imaginary").GetArrayLength()!=3)throw new ArgumentException("full algebraic input schema");bool nonzero=false;
  for(int k=0;k<3;k++){string re=row.GetProperty("real")[k].GetString()!,im=row.GetProperty("imaginary")[k].GetString()!;var a0=SpinGeometry.Parse(re);var b0=SpinGeometry.Parse(im);if(a0.ToString()!=re||b0.ToString()!=im)throw new ArgumentException("canonical rational input");var value=new Scalar(a0,b0);nonzero|=!value.IsZero;Put(result.Coefficients[k],key,value);}if(!nonzero)throw new ArgumentException("zero tensor row");
 }if(!AT.Valid(result,1))throw new ArgumentException("full real one-form input");return result;
}
static Rational Polynomial(Rational x)=>693*x*x*x-511*x*x-64*x-17;
void WriteShard(int point,string category,object value)
{
 byte[] bytes=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value)+"\n");if(bytes.LongLength>67108864||totalShardBytes+bytes.LongLength>536870912)throw new ResourceLimitException("point/aggregate byte ceiling");using(var document=JsonDocument.Parse(bytes))ScanRetainedRationals(document.RootElement);if(maximumRationalCharacters>32768)throw new ResourceLimitException("retained rational text ceiling");string path=Root+"/output/points/point"+point+"_"+category+".json";Directory.CreateDirectory(Root+"/output/points");File.WriteAllBytes(path,bytes);shards.Add(new(point,category,path,Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant(),bytes.LongLength));totalShardBytes+=bytes.LongLength;Inc("pointShards");
}
void ScanRetainedRationals(JsonElement value)
{if(value.ValueKind==JsonValueKind.Array){foreach(var x in value.EnumerateArray())ScanRetainedRationals(x);}else if(value.ValueKind==JsonValueKind.Object){foreach(var x in value.EnumerateObject())ScanRetainedRationals(x.Value);}else if(value.ValueKind==JsonValueKind.String){string s0=value.GetString()!;if(System.Text.RegularExpressions.Regex.IsMatch(s0,@"^-?[0-9]+(?:/[0-9]+)?$"))maximumRationalCharacters=Math.Max(maximumRationalCharacters,s0.Length);}}
void Emit(string terminal,object evidence)
{
 object Result()=>new{schemaVersion=1,phase=625,phaseId="phase625-full-algebraic-kappa-jet-obstruction-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(Result())+"\n";if(Encoding.UTF8.GetByteCount(json)>1048576){string originalTerminal=terminal;if(terminal==Success)terminal=precedence[8];var original=JsonSerializer.SerializeToElement(evidence);var bounded=new Dictionary<string,object?>();foreach(var x in original.EnumerateObject())if(x.Value.ValueKind is JsonValueKind.True or JsonValueKind.False or JsonValueKind.Number or JsonValueKind.String||x.Name is "counts" or "shards")bounded[x.Name]=x.Value.Clone();bounded["controlsPassed"]=false;bounded["resourcesPassed"]=false;bounded["resourceFailure"]="manifest byte ceiling";bounded["originalTerminal"]=originalTerminal;evidence=bounded;json=JsonSerializer.Serialize(Result())+"\n";}
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/full_algebraic_kappa_jet_obstruction_audit.json",json);File.WriteAllText(Root+"/output/full_algebraic_kappa_jet_obstruction_audit_summary.json",json);Console.WriteLine($"Phase625 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
internal sealed record Shard(int point,string category,string path,string sha256,long bytes);
internal sealed class ResourceLimitException(string message):Exception(message);
