using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static Adjoint;
using static Caa;
using static ProjectorGradient;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase617-a61-nonparallel-projector-two-weight-gradient-v1";
const string Success="nonparallel-projector-controls-pass-two-weight-ansatz-nonstationary";
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
 "operator": "canonical untied CAA: firstC outerA innerA; Phi1=Gamma1 Phi2=Gamma2",
 "pairing": "signed exterior metric times -ReTr(XY)/128, real bilinear; compact-support formal adjoint",
 "ansatz": "S=(aPT+b(I-PT))Gamma1; real constant weights; geometry and couplings held fixed",
 "projectorRank": 9,
 "projectorFrameIndices": [
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
 "covariantDerivative": "partial PT + Gamma PT - PT Gamma, all14 coordinate directions before tensor frame transport",
 "typedHorizontalHorizontal": "sigma/2[y Sym(u,v)y-(u^T y v)y/4]",
 "typedHorizontalVertical": "y^-1(PT A)u/2",
 "typedVertical": "zero",
 "differential": "(K DB S + DBdag Kdag S)/2; both legs separately J for PT",
 "witness": {
  "form": 1,
  "blade": 3,
  "coefficient": "-(a-b)/2"
 },
 "formalMonomials": [
  "a^2",
  "ab",
  "b^2"
 ],
 "KQCoefficientsPT": [
  112,
  160,
  40
 ],
 "KQCoefficientsComplement": [
  144,
  144,
  24
 ],
 "adjointComposite": "2KQ coefficientwise, full tensor not projected",
 "sourcePT": "-15/4",
 "sourceComplement": "-21/4",
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
 "weights": [
  -1,
  0,
  1
 ],
 "falseStationary": {
  "gamma": "1",
  "kappa": "11",
  "a": "3/4",
  "b": "-3/4",
  "KQPT": "-9/2",
  "KQComplement": "27/2",
  "algebraicResidual": "0",
  "differentialWitness": "-3/4"
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "expectedCounts": {
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "contexts": 2,
  "frameControls": 2,
  "projectorControls": 2,
  "connectionEntries": 5488,
  "coordinateDerivativeEntries": 5488,
  "derivativeTraceRows": 28,
  "projectorDerivativeRows": 28,
  "wrongParallelHorizontalRows": 8,
  "omittedPartialVerticalRows": 20,
  "frameDerivativeEntries": 5488,
  "transportDerivativeEntries": 2744,
  "divergenceEntries": 28,
  "horizontalAnchorRows": 2,
  "kineticStageComparisons": 16,
  "reverseDerivativeRows": 28,
  "reverseSimplifiedRows": 28,
  "kineticLegRows": 4,
  "kineticGradientRows": 2,
  "kineticWitnessRows": 2,
  "omittedKineticLegRows": 4,
  "transportKineticRows": 1,
  "spinInputRows": 2,
  "sourceStageComparisons": 16,
  "sourceRows": 2,
  "algebraicAdjointRows": 4,
  "forwardAdjointProbes": 364,
  "adjointReconstructions": 4,
  "quadraticSourceCoefficients": 6,
  "quadraticAdjointCoefficients": 6,
  "quadraticGradientCoefficients": 6,
  "originalActionVariations": 1176,
  "parameterRows": 162,
  "equalWeightRows": 54,
  "unequalWeightRows": 108,
  "falseStationaryRows": 2
 },
 "resources": {
  "estimatedCpuSeconds": 120,
  "estimatedPeakBytes": 268435456,
  "maximumTrackedCoefficientProducts": 100000000,
  "maximumTrackedMatrixProducts": 100000000,
  "maximumTensorTerms": 32768,
  "maximumMatrixDimension": 14,
  "largestArrayEntries": 38416,
  "maximumFormalDegree": 2,
  "maximumFormalCoefficients": 3,
  "maximumFrequency": 0,
  "maximumRetainedParameterRows": 162
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormSelected": false,
  "sourceMetricSelected": false,
  "physicalVacuumSelected": false,
  "globalVacuumRejected": false,
  "nonconstantWeightsIncluded": false,
  "additionalCliffordGradesInAnsatz": false,
  "fullMetricEulerComputed": false,
  "physicalSpectrumClaimed": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","coordinate-connection-control-failed","covariant-projector-control-failed","complete-kinetic-adjoint-control-failed","curved-source-control-failed","full-algebraic-gradient-control-failed","false-stationarity-control-failed","resource-census-control-failed","nonparallel-projector-controls-pass-two-weight-ansatz-nonstationary"];
var paths=new Dictionary<string,string>{
 ["program"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/Program.cs",
 ["project"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/Phase617NonparallelProjectorTwoWeightGradientAudit.csproj",
 ["study"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/STUDY.md",
 ["projector-helper"]="studies/phase617_nonparallel_projector_two_weight_gradient_audit_001/ProjectorGradient.cs",
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
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["core-source-manifest"]="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json",
 ["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==617&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase610","induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source"),("phase611","untied-caa-response-controls-pass-source-choice-open")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(9.1)","(9.4)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}


var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string k)=>counts[k]++;
bool knownAnswerPassed=true,connectionPassed=true,derivativePassed=true,kineticPassed=true,algebraicPassed=true,sourcePassed=true,decoysPassed=true,frequencyPassed=true;
foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("arithmeticControls");knownAnswerPassed&=pass;}
for(int a=0;a<16384;a++)
{foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
using var upstreamGeometry=JsonDocument.Parse(File.ReadAllBytes(paths["phase608-summary"]));
using var upstreamSpin=JsonDocument.Parse(File.ReadAllBytes(paths["phase610-summary"]));
var allGeometry=upstreamGeometry.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().ToArray();
var allSpin=upstreamSpin.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().ToArray();
var rows=new List<object>();Matrix[]? previousDerivatives=null;FT? previousJ=null;
for(int point=0;point<2;point++)
{
 Inc("contexts");var y=point==0?Matrix.Diagonal(-1,1,1,1):Matrix.Diagonal(-1,4,9,16);var g=new Ambient(y,1,new Rational(-1,2),-1);
 var old=allGeometry.Single(q=>q.GetProperty("point").GetInt32()==point&&q.GetProperty("beta").GetString()=="-1/2");var spin=allSpin.Single(q=>q.GetProperty("point").GetInt32()==point);
 var frame=SpinGeometry.Frame(point);var inverse=frame.Inverse();var eta=SpinGeometry.Eta();Inc("frameControls");connectionPassed&=frame.Same(SpinGeometry.ReadMatrix(spin.GetProperty("frame")))&&inverse.Same(SpinGeometry.HandInverse(point))&&(SpinGeometry.Transpose(frame)*g.Gram*frame).Same(eta)&&frame.Determinant().Numerator.Sign>0;
 var p=g.TracelessProjector();var pf=inverse*p*frame;var rf=Matrix.Identity(14)-pf;var diagonalP=new Matrix(14);foreach(int i in new[]{1,2,3,4,5,6,11,12,13})diagonalP[i,i]=1;
 Inc("projectorControls");connectionPassed&=p.Same(SpinGeometry.ReadMatrix(old.GetProperty("tracelessProjector")))&&(p*p).Same(p)&&p.Trace()==9&&pf.Same(diagonalP)&&(SpinGeometry.Transpose(p)*g.Gram).Same(g.Gram*p);
 var oldConnection=new Rational[14,14,14];var seen=new HashSet<(int,int,int)>();foreach(var v in old.GetProperty("connectionCoefficients").EnumerateArray()){int a=v.GetProperty("a").GetInt32(),b=v.GetProperty("b").GetInt32(),c=v.GetProperty("c").GetInt32();var z=SpinGeometry.Parse(v.GetProperty("value").GetString()!);if(new[]{a,b,c}.Any(t=>t<0||t>=14)||z==0||!seen.Add((a,b,c)))throw new InvalidOperationException("connection input");oldConnection[a,b,c]=z;}
 var coordinate=new Matrix[14];var oracleCoordinate=new Matrix[14];var derivativeRows=new List<object>();
 for(int axis=0;axis<14;axis++)
 {
  var connection=Connection(g,axis);var partial=PartialProjector(g,axis);var commutator=connection*p-p*connection;coordinate[axis]=partial+commutator;oracleCoordinate[axis]=Oracle(g,axis);
  for(int b=0;b<14;b++)for(int c=0;c<14;c++){Inc("connectionEntries");connectionPassed&=connection[c,b]==oldConnection[axis,b,c];Inc("coordinateDerivativeEntries");derivativePassed&=coordinate[axis][c,b]==oracleCoordinate[axis][c,b];}
  Inc("derivativeTraceRows");derivativePassed&=coordinate[axis].Trace()==0;
  Inc("projectorDerivativeRows");derivativePassed&=(coordinate[axis]*p+p*coordinate[axis]).Same(coordinate[axis])&&(SpinGeometry.Transpose(coordinate[axis])*g.Gram).Same(g.Gram*coordinate[axis]);
  if(axis<4){Inc("wrongParallelHorizontalRows");decoysPassed&=!coordinate[axis].Zero&&partial.Zero;}else{Inc("omittedPartialVerticalRows");decoysPassed&=coordinate[axis].Zero&&!commutator.Zero&&!partial.Zero;}
  derivativeRows.Add(new{axis,connection=connection.Text(),partialProjector=partial.Text(),connectionCommutator=commutator.Text(),covariantDerivative=coordinate[axis].Text(),typedOracle=oracleCoordinate[axis].Text()});
 }
 var derivatives=FrameDerivatives(coordinate,frame,inverse);var oracleDerivatives=FrameDerivatives(oracleCoordinate,frame,inverse);
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++){Inc("frameDerivativeEntries");derivativePassed&=derivatives[a][c,b]==oracleDerivatives[a][c,b];if(point==1){Inc("transportDerivativeEntries");derivativePassed&=derivatives[a][c,b]==previousDerivatives![a][c,b];}}
 var div=new Rational[14];for(int c=0;c<14;c++){for(int a=0;a<14;a++)div[c]+=Sigma(a)*derivatives[a][c,a];Inc("divergenceEntries");derivativePassed&=div[c]==0;}
 Inc("horizontalAnchorRows");derivativePassed&=derivatives[0][1,0]==new Rational(1,4)&&derivatives[0][2,0]==new Rational(1,4)&&derivatives[0][3,0]==new Rational(1,4)&&Enumerable.Range(0,14).Where(i=>i is not (1 or 2 or 3)).All(i=>derivatives[0][i,0]==0);
 var db=ExteriorDerivative(derivatives);var stages=ForwardStages(db);var naive=ForwardStages(db,true);for(int j=0;j<8;j++){Inc("kineticStageComparisons");kineticPassed&=Equal(stages[j],naive[j]);}
 var reverseFirst=new FT[14];var reverseSecond=new FT[14];var reverseRows=new List<object>();
 for(int a=0;a<14;a++)
 {var ds=SpinGeometry.WeightedGamma(derivatives[a]);var reverse=AdjointLegs(ds);reverseFirst[a]=reverse.First;reverseSecond[a]=reverse.Second;var sum=Add(reverse.First,reverse.Second);Inc("reverseDerivativeRows");kineticPassed&=ConstantRealType(sum,2);Inc("reverseSimplifiedRows");kineticPassed&=Equal(sum,AdjointSimplified(ds));reverseRows.Add(new{axis=a,derivative=Terms(ds),first=Terms(reverse.First),second=Terms(reverse.Second),full=Terms(sum)});}
 var adjFirst=Divergence(reverseFirst);var adjSecond=Divergence(reverseSecond);var adjTotal=Add(adjFirst,adjSecond);var kinetic=Scale(Add(stages[7],adjTotal),Fourier.Half);var jOracle=OracleJ(oracleDerivatives);
 foreach(var leg in new[]{stages[7],adjTotal}){Inc("kineticLegRows");kineticPassed&=Equal(leg,jOracle)&&ConstantRealType(leg,1);}
 Inc("kineticGradientRows");kineticPassed&=Equal(kinetic,jOracle)&&ConstantRealType(db,2)&&kinetic.Keys.All(q=>Degree(q.Blade)==2);
 Inc("kineticWitnessRows");kineticPassed&=kinetic.GetValueOrDefault((1,3,0,0))==new Scalar(new Rational(-1,2),0);
 foreach(var leg in new[]{stages[7],adjTotal}){Inc("omittedKineticLegRows");decoysPassed&=!Equal(Scale(leg,Fourier.Half),kinetic)&&leg.Count!=0;}
 if(point==1){Inc("transportKineticRows");kineticPassed&=Equal(kinetic,previousJ!);}
 var r=SpinGeometry.ReadCurvature(old.GetProperty("curvatureCoefficients"));var lowered=SpinGeometry.LoweredFrame(SpinGeometry.Lower(r,g.Gram),frame);var fb=SpinGeometry.Lift(lowered);Inc("spinInputRows");sourcePassed&=Equal(fb,ReadTerms(spin.GetProperty("spinCurvature")))&&ConstantRealType(fb,2);
 var sourceStages=ForwardStages(fb);var sourceNaive=ForwardStages(fb,true);for(int k=0;k<8;k++){Inc("sourceStageComparisons");sourcePassed&=Equal(sourceStages[k],sourceNaive[k]);}
 var fieldP=SpinGeometry.WeightedGamma(pf);var fieldR=SpinGeometry.WeightedGamma(rf);var sourceOracle=Add(Scale(fieldP,new Scalar(new Rational(-15,4),0)),Scale(fieldR,new Scalar(new Rational(-21,4),0)));
 Inc("sourceRows");sourcePassed&=Equal(sourceStages[7],sourceOracle)&&VectorOnly(sourceStages[7]);
 var kadP=AdjointLiteral(fieldP);var kadR=AdjointLiteral(fieldR);var basisFields=new[]{fieldP,fieldR};var basisMatrices=new[]{pf,rf};var adjoints=new[]{kadP,kadR};var adjointRows=new List<object>();
 for(int t=0;t<2;t++)
 {
  Inc("algebraicAdjointRows");algebraicPassed&=Equal(adjoints[t],DiagonalAdjoint(basisMatrices[t]))&&ConstantRealType(adjoints[t],2);
  var rebuilt=new FT();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++){int m=(1<<a)|(1<<b);var probe=One(m,m,1);var norm=Pair(probe,probe);var pairing=Pair(Forward(probe),basisFields[t]);Inc("forwardAdjointProbes");algebraicPassed&=norm!=0&&pairing==Pair(probe,adjoints[t]);rebuilt=Add(rebuilt,Scale(probe,new Scalar(pairing*Matrix.Inv(norm),0)));}
  Inc("adjointReconstructions");algebraicPassed&=Equal(rebuilt,adjoints[t]);adjointRows.Add(new{component=t,field=Terms(basisFields[t]),adjoint=Terms(adjoints[t]),reconstruction=Terms(rebuilt)});
 }
 FT[] qCoefficients=[Product(fieldP,fieldP),Add(Product(fieldP,fieldR),Product(fieldR,fieldP)),Product(fieldR,fieldR)];
 FT[] nCoefficients=[DQAdjoint(fieldP,kadP),Add(DQAdjoint(fieldP,kadR),DQAdjoint(fieldR,kadP)),DQAdjoint(fieldR,kadR)];
 int[] pCoefficients=[112,160,40],rCoefficients=[144,144,24];var kCoefficients=qCoefficients.Select(Forward).ToArray();var potentialCoefficients=new FT[3];var coefficientRows=new List<object>();
 for(int m=0;m<3;m++)
 {
  var oracle=Add(Scale(fieldP,pCoefficients[m]),Scale(fieldR,rCoefficients[m]));Inc("quadraticSourceCoefficients");algebraicPassed&=Equal(kCoefficients[m],oracle)&&VectorOnly(kCoefficients[m]);Inc("quadraticAdjointCoefficients");algebraicPassed&=Equal(nCoefficients[m],Scale(oracle,2))&&VectorOnly(nCoefficients[m]);
  potentialCoefficients[m]=Scale(Add(kCoefficients[m],nCoefficients[m]),new Scalar(new Rational(1,3),0));Inc("quadraticGradientCoefficients");algebraicPassed&=Equal(potentialCoefficients[m],oracle)&&ConstantRealType(potentialCoefficients[m],1);
  var actionVariations=new List<object>();
  for(int a=0;a<14;a++)for(int b=0;b<14;b++)
  {
   var v=One(1<<a,1<<b,1);var kp=Forward(Product(fieldP,v,'C'));var kr=Forward(Product(fieldR,v,'C'));Rational original=Pair(v,kCoefficients[m]);
   original+=m==0?Pair(fieldP,kp):m==1?Pair(fieldP,kr)+Pair(fieldR,kp):Pair(fieldR,kr);original*=new Rational(1,3);
   var pairing=Pair(v,potentialCoefficients[m]);Inc("originalActionVariations");algebraicPassed&=original==pairing;actionVariations.Add(new{a,b,original=original.ToString(),gradientPairing=pairing.ToString()});
  }
  coefficientRows.Add(new{monomial=m==0?"a^2":m==1?"ab":"b^2",Q=Terms(qCoefficients[m]),KQ=Terms(kCoefficients[m]),adjointComposite=Terms(nCoefficients[m]),potential=Terms(potentialCoefficients[m]),actionVariations});
 }
 var parameterRows=new List<object>();
 foreach(int gamma in new[]{0,1,2})foreach(int kappa in new[]{-1,0,1})foreach(int a in new[]{-1,0,1})foreach(int b in new[]{-1,0,1})
 {
  var s=Add(Scale(fieldP,a),Scale(fieldR,b));var algebraic=Add(Add(Scale(Evaluate(potentialCoefficients,a,b),gamma),Scale(s,kappa)),sourceStages[7]);var differential=Scale(kinetic,a-b);var full=Add(algebraic,differential);
  Rational ga=4*gamma*(28*a*a+40*a*b+10*b*b)+kappa*a-new Rational(15,4),gb=4*gamma*(36*a*a+36*a*b+6*b*b)+kappa*b-new Rational(21,4);
  Inc("parameterRows");algebraicPassed&=Equal(algebraic,Add(Scale(fieldP,new Scalar(ga,0)),Scale(fieldR,new Scalar(gb,0))))&&VectorOnly(algebraic)&&ConstantRealType(full,1)&&full.Count!=0;
  if(a==b){Inc("equalWeightRows");algebraicPassed&=ga-gb==new Rational(3,2)&&differential.Count==0;}else{Inc("unequalWeightRows");kineticPassed&=full.GetValueOrDefault((1,3,0,0))==new Scalar(new Rational(b-a,2),0);}
  parameterRows.Add(new{gamma,kappa,a,b,algebraic=Terms(algebraic),differential=Terms(differential),fullGradient=Terms(full),stationary=full.Count==0});
 }
 Rational ca=new(3,4),cb=new(-3,4);var cs=Add(Scale(fieldP,new Scalar(ca,0)),Scale(fieldR,new Scalar(cb,0)));var cq=Evaluate(kCoefficients,ca,cb);var cpotential=Evaluate(potentialCoefficients,ca,cb);var cvector=Add(Add(cpotential,Scale(cs,11)),sourceStages[7]);var cdifferential=Scale(kinetic,new Scalar(ca-cb,0));var cfull=Add(cvector,cdifferential);
 Inc("falseStationaryRows");decoysPassed&=Equal(cq,Add(Scale(fieldP,new Scalar(new Rational(-9,2),0)),Scale(fieldR,new Scalar(new Rational(27,2),0))))&&cvector.Count==0&&cfull.Count!=0&&cfull.GetValueOrDefault((1,3,0,0))==new Scalar(new Rational(-3,4),0);
 frequencyPassed&=new[]{db,kinetic,fb,sourceStages[7],cfull}.All(t=>t.Keys.All(q=>q.K0==0&&q.K1==0));
 rows.Add(new{point,frame=frame.Text(),projector=pf.Text(),coordinateDerivativeRows=derivativeRows,frameDerivatives=derivatives.Select(q=>q.Text()).ToArray(),divergence=div.Select(q=>q.ToString()).ToArray(),exteriorDerivative=Terms(db),kineticStages=stages.Select(Terms).ToArray(),reverseRows,adjointDivergenceFirst=Terms(adjFirst),adjointDivergenceSecond=Terms(adjSecond),adjointDivergence=Terms(adjTotal),kinetic=Terms(kinetic),kineticOracle=Terms(jOracle),spinCurvature=Terms(fb),sourceStages=sourceStages.Select(Terms).ToArray(),adjointRows,coefficientRows,parameterRows,falseStationaryControl=new{gamma=1,kappa=11,a=ca.ToString(),b=cb.ToString(),KQ=Terms(cq),algebraic=Terms(cvector),differential=Terms(cdifferential),fullGradient=Terms(cfull),witness=cfull.GetValueOrDefault((1,3,0,0)).Real.ToString()}});
 previousDerivatives=derivatives;previousJ=kinetic;
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");bool resourcesPassed=frequencyPassed&&CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&Matrix.Products<=resources.GetProperty("maximumTrackedMatrixProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumTensorTerms").GetInt32();
bool controlsPassed=knownAnswerPassed&&connectionPassed&&derivativePassed&&kineticPassed&&algebraicPassed&&sourcePassed&&decoysPassed&&countsPassed&&resourcesPassed;
string verdict=!knownAnswerPassed?precedence[1]:!connectionPassed?precedence[2]:!derivativePassed?precedence[3]:!kineticPassed?precedence[4]:!sourcePassed?precedence[5]:!algebraicPassed?precedence[6]:!decoysPassed?precedence[7]:!countsPassed||!resourcesPassed?precedence[8]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,connectionPassed,derivativePassed,kineticPassed,algebraicPassed,sourcePassed,decoysPassed,countsPassed,resourcesPassed,frequencyPassed,counts,trackedCoefficientProducts=CoefficientProducts,trackedMatrixProducts=Matrix.Products,largestTensor=LargestTensor,rows,scope=new{sourceOperatorSelected=false,sourceNormSelected=false,sourceMetricSelected=false,physicalVacuumSelected=false,globalVacuumRejected=false,nonconstantWeightsIncluded=false,additionalCliffordGradesInAnsatz=false,fullMetricEulerComputed=false,physicalSpectrumClaimed=false}});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=617,phaseId="phase617-nonparallel-projector-two-weight-gradient-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/nonparallel_projector_two_weight_gradient_audit.json",json);File.WriteAllText(Root+"/output/nonparallel_projector_two_weight_gradient_audit_summary.json",json);Console.WriteLine($"Phase617 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
