using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase620_curved_caa_action_descent_ward_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase620-a63-curved-caa-action-descent-ward-v1";
const string Success="curved-caa-action-descent-ward-controls-pass-conditional-epsilon-redundancy";
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
 "domain": "full real u(64,64), all16384 real phased blades including central iI; no carrier projection",
 "operator": "canonical untied CAA; independently conjugated firstPhi Gamma1, outerPhi Gamma1, innerPhi Gamma2; Hodge and1/2 retained",
 "background": "complete passed608 beta=-1/2 curvature reconstructed through immutable610 frame/lift; local normal reference A0(p)=0, partial_a A0_b=F_ab/2",
 "epsilonModes": [
  "identity including zero derivatives",
  "(1+fN)(1+gM),f=g=1,df=theta0,dg=theta1; f=1+x0,g=1+x1"
 ],
 "nilpotents": "N=gamma2+Gamma12,M=gamma2-Gamma12; N2=M2=0,[N,M]=4gamma1",
 "ordinaryMixedHessian": "epsilon01=epsilon10=NM; inverse01=inverse10=MN; other entrieszero",
 "covariantSquare": "ordinary symmetric Hessian plus [partial_a A0_b,epsilon], antisymmetrized; compare [F0,epsilon] only afterward",
 "fields": [
  {
   "id": "C",
   "S": "Gamma1",
   "DS": "0",
   "pieces": [
    "60",
    "0",
    "-4368",
    "-7"
   ],
   "variation": [
    "60",
    "0",
    "-13104",
    "-14"
   ]
  },
  {
   "id": "K",
   "S": "theta0 gamma1",
   "DS": "theta02 Gamma12",
   "realization": "partial2 S0=-Gamma12",
   "pieces": [
    "0",
    "1",
    "0",
    "-1/2"
   ],
   "variation": [
    "0",
    "2",
    "0",
    "-1"
   ]
  }
 ],
 "pieceOrder": [
  "source",
  "kineticWithHalf",
  "rawCubic",
  "massWithHalf"
 ],
 "gammas": [
  1,
  2
 ],
 "kappas": [
  0,
  1
 ],
 "directions": [
  "fixed epsilon: V=S,DV=DS",
  "plus Ward: eta=Gamma01,Deta=theta0gamma1,deltaomega=D_Aeta",
  "epsilon only: sameeta,deltaomega=0"
 ],
 "epsilonOnlyAnchor": [
  "21/2",
  "-21/16",
  "0",
  "1"
 ],
 "decoys": [
  {
   "id": "wrong-lift",
   "eta": "0",
   "Deta": "theta0gamma1",
   "pieces": [
    "0",
    "-2",
    "0",
    "2"
   ],
   "kappa": 0,
   "total": "-2"
  },
  {
   "id": "frozen-curvature",
   "eta": "Gamma01",
   "Deta": "0",
   "pieces": [
    "-5/8",
    "0",
    "0",
    "0"
   ],
   "kappa": 0,
   "total": "-5/8"
  },
  {
   "id": "frozen-all-Phi",
   "eta": "Gamma01",
   "Deta": "0",
   "pieces": [
    "-79/8",
    "0",
    "0",
    "0"
   ],
   "kappa": 0,
   "total": "-79/8"
  }
 ],
 "variation": "exact first dual-number product rules; no printed9.6/9.7 or covariance substituted for action",
 "expectedCounts": {
  "arithmeticControls": 4,
  "wordCases": 507904,
  "hodgeCases": 16384,
  "domainBladeControls": 16384,
  "centralDomainControls": 1,
  "nilpotentIdentities": 6,
  "kernelParityCases": 192,
  "points": 2,
  "frameEntries": 392,
  "curvatureEntries": 76832,
  "spinActionRows": 2548,
  "curvatureInputRows": 2,
  "sourceRows": 2,
  "pointTransportRows": 1,
  "epsilonContexts": 4,
  "inverseControls": 16,
  "inverseDerivativeControls": 8,
  "ordinaryHessianRows": 784,
  "normalSquareControls": 8,
  "contexts": 8,
  "noncommutingGeometryRows": 4,
  "operatorDescentRows": 24,
  "actionPieceRows": 32,
  "topTraceRows": 32,
  "actionRows": 32,
  "variationContexts": 24,
  "inverseVariationRows": 24,
  "reconstructedFieldRows": 24,
  "variationPieceRows": 96,
  "phiVariationRows": 72,
  "epsilonOnlyAnchorRows": 2,
  "fixedDirectionRows": 32,
  "wardRows": 32,
  "epsilonOnlyRows": 32,
  "decoyPieceRows": 6,
  "decoyRows": 12
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "resources": {
  "estimatedCpuSeconds": 30,
  "maximumEstimatedCpuSeconds": 300,
  "estimatedPeakBytes": 134217728,
  "maximumEstimatedPeakBytes": 1073741824,
  "maximumTrackedCoefficientProducts": 20000000000,
  "maximumTrackedMatrixProducts": 10000000,
  "maximumTensorTerms": 1490944,
  "normalSquareEntries": 9646,
  "dualChainStages": 912
 },
 "scope": {
  "sourceOperatorSelected": false,
  "sourceNormSelected": false,
  "sourceMetricSelected": false,
  "domainRestrictedToFixtures": false,
  "printedForceUsed": false,
  "invariantScalarActionUsedAsFullGradient": false,
  "globalCurvatureConstancyClaimed": false,
  "finiteTotalActionClaimed": false,
  "fullMetricEulerComputed": false,
  "physicalVacuumSelected": false,
  "physicalSpectrumClaimed": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","curved-reference-jet-control-failed","literal-action-descent-control-failed","actual-product-variation-ward-control-failed","nonzero-decoy-control-failed","resource-census-control-failed","curved-caa-action-descent-ward-controls-pass-conditional-epsilon-redundancy"];
var paths=new Dictionary<string,string>{
 ["program"]="studies/phase620_curved_caa_action_descent_ward_audit_001/Program.cs",
 ["project"]="studies/phase620_curved_caa_action_descent_ward_audit_001/Phase620CurvedCaaActionDescentWardAudit.csproj",
 ["study"]="studies/phase620_curved_caa_action_descent_ward_audit_001/STUDY.md",
 ["curved-ward-helper"]="studies/phase620_curved_caa_action_descent_ward_audit_001/CurvedWard.cs",
 ["phase598-program"]="studies/phase598_continuum_action_descent_ward_audit_001/Program.cs",
 ["phase598-project"]="studies/phase598_continuum_action_descent_ward_audit_001/Phase598ContinuumActionDescentWardAudit.csproj",
 ["phase598-study"]="studies/phase598_continuum_action_descent_ward_audit_001/STUDY.md",
 ["phase598-contract"]="studies/phase598_continuum_action_descent_ward_audit_001/preregistration/contract_v1.json",
 ["phase598-summary"]="studies/phase598_continuum_action_descent_ward_audit_001/output/continuum_action_descent_ward_audit_summary.json",
 ["phase598-exactarithmetic-helper"]="studies/phase598_continuum_action_descent_ward_audit_001/ExactArithmetic.cs",
 ["phase598-fouriertensor-helper"]="studies/phase598_continuum_action_descent_ward_audit_001/FourierTensor.cs",
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
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["core-source-manifest"]="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json",
 ["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==620&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var coreManifest=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=coreManifest.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=coreManifest.GetProperty("schemaVersion").GetInt32()==1&&coreManifest.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==coreManifest.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase598","continuum-action-descent-ward-controls-pass-source-choice-open"),("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase610","induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source"),("phase611","untied-caa-response-controls-pass-source-choice-open")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.27)","(3.34)","(9.1)","(9.4)","(12.26)","(12.27)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string key)=>counts[key]++;
bool knownAnswerPassed=true,inputPassed=true,geometryPassed=true,actionPassed=true,variationPassed=true,decoysPassed=true;
foreach(bool pass in new[]{new Rational(2,4)==new Rational(1,2),new Rational(-3,-6)==new Rational(1,2),new Rational(2,3)*new Rational(3,2)==1,Scalar.I*Scalar.I==new Scalar(-1)}){Inc("arithmeticControls");knownAnswerPassed&=pass;}
for(int a=0;a<16384;a++)
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full)){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("wordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}
 Inc("wordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);Inc("hodgeCases");int d=Degree(a);knownAnswerPassed&=HodgeSign(a)*HodgeSign(Full^a)==((d*(14-d)+7)%2==0?1:-1);
 Scalar phase=AdjointSign(a)==-1?1:Scalar.I;Inc("domainBladeControls");knownAnswerPassed&=HAnti(One(0,a,phase));
}
Inc("centralDomainControls");knownAnswerPassed&=HAnti(One(0,0,Scalar.I));
var one=One(0,0,1);var n=Add(One(0,4,1),One(0,6,1));var m=Add(One(0,4,1),One(0,6,-1));
foreach(bool pass in new[]{Product(n,n).Count==0,Product(m,m).Count==0,Equal(Product(n,m),Scale(Add(one,One(0,2,1)),2)),Equal(Product(m,n),Scale(Add(one,One(0,2,-1)),2)),Equal(Product(n,m,'C'),One(0,2,4)),Equal(Product(Product(m,n),m),Scale(m,4))}){Inc("nilpotentIdentities");knownAnswerPassed&=pass;}
FT[] kernel=[one,n,m,One(0,0,Scalar.I),One(1,2,1),One(5,6,1),Caa.Gamma1,Star(One(5,6,1))];
foreach(var a in kernel)foreach(var b in kernel)foreach(char kind in new[]{'W','C','A'}){Inc("kernelParityCases");knownAnswerPassed&=Equal(Product(a,b,kind),NaiveProduct(a,b,kind));}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
using var upstream=JsonDocument.Parse(File.ReadAllBytes(paths["phase608-summary"]));using var spinUpstream=JsonDocument.Parse(File.ReadAllBytes(paths["phase610-summary"]));
var upstreamRows=upstream.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().Where(x=>x.GetProperty("beta").GetString()=="-1/2").OrderBy(x=>x.GetProperty("point").GetInt32()).ToArray();
var pointRows=new List<object>();var rows=new List<object>();var decoyRows=new List<object>();FT? previous=null;
for(int point=0;point<2;point++)
{
 Inc("points");var input=upstreamRows.Single(x=>x.GetProperty("point").GetInt32()==point);var gram=SpinGeometry.ReadMatrix(input.GetProperty("gram"));var coordinate=SpinGeometry.ReadCurvature(input.GetProperty("curvatureCoefficients"));var frame=SpinGeometry.Frame(point);var inverseFrame=frame.Inverse();var frameGram=SpinGeometry.Transpose(frame)*gram*frame;
 for(int a=0;a<14;a++)for(int b=0;b<14;b++){Inc("frameEntries");inputPassed&=frameGram[a,b]==(a==b?Sigma(a):0);}
 var low=SpinGeometry.LoweredFrame(SpinGeometry.Lower(coordinate,gram),frame);var vectorR=SpinGeometry.EndomorphismFrame(coordinate,frame,inverseFrame);
 for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++){Inc("curvatureEntries");inputPassed&=low[a,b,c,d]==Sigma(d)*vectorR[a,b,c,d];}
 var f=SpinGeometry.Lift(low);
 for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)for(int c=0;c<14;c++){Inc("spinActionRows");inputPassed&=Equal(Product(SpinGeometry.Slice(f,a,b),One(0,1<<c,1),'C'),SpinGeometry.Vector(vectorR,a,b,c));}
 Inc("curvatureInputRows");inputPassed&=Equal(f,CurvedWard.Read(spinUpstream.RootElement.GetProperty("evidence").GetProperty("rows").EnumerateArray().Single(x=>x.GetProperty("point").GetInt32()==point).GetProperty("spinCurvature")))&&f.Count==822&&HAnti(f)&&Typed(f,2)&&f.GetValueOrDefault((3,3,0,0))==new Scalar(new Rational(-1,32),0);
 var source=Caa.Forward(f);Inc("sourceRows");inputPassed&=source.Count==14&&Pair(Caa.Gamma1,source)==60;
 if(previous is not null){Inc("pointTransportRows");inputPassed&=Equal(f,previous);}previous=f;
 pointRows.Add(new{point,frame=frame.Text(),curvature=Terms(f),source=Terms(source)});
 for(int mode=0;mode<2;mode++)
 {
  Inc("epsilonContexts");var ep=CurvedWard.Epsilon(mode);var e=ep.E;var de=ep.DE;var inv=ep.Inverse;var di=ep.DInverse;
  foreach(bool pass in new[]{Equal(Product(e,inv),one),Equal(Product(inv,e),one),Equal(HAdjoint(e),inv),Equal(HAdjoint(inv),e)}){Inc("inverseControls");geometryPassed&=pass;}
  foreach(var z in new[]{Add(Product(de,inv),Product(e,di)),Add(Product(di,e),Product(inv,de))}){Inc("inverseDerivativeControls");geometryPassed&=z.Count==0;}
  var hessianRows=new List<object>();var antisymE=new FT();var antisymI=new FT();
  for(int a=0;a<14;a++)for(int b=0;b<14;b++)
  {
   Inc("ordinaryHessianRows");var ordinaryE=mode==1&&((a==0&&b==1)||(a==1&&b==0))?Product(n,m):new FT();var ordinaryI=mode==1&&((a==0&&b==1)||(a==1&&b==0))?Product(m,n):new FT();
   FT Component(FT t,int axis){var c=new FT();foreach(var q in t)if(q.Key.Form==(1<<axis))Put(c,(0,q.Key.Blade,0,0),q.Value);return c;}
   var ea=Component(de,a);var eb=Component(de,b);var ia=Component(di,a);var ib=Component(di,b);
   var inverseHessian=Add(Add(Product(ordinaryE,inv),Product(ea,ib)),Add(Product(eb,ia),Product(e,ordinaryI)));
   var reverseHessian=Add(Add(Product(ordinaryI,e),Product(ia,eb)),Add(Product(ib,ea),Product(inv,ordinaryE)));
   var connectionJet=a==b?new FT():Scale(SpinGeometry.Slice(f,System.Math.Min(a,b),System.Math.Max(a,b)),Fourier.Half*(a<b?1:-1));
   var covariantE=Add(ordinaryE,CurvedWard.Comm(connectionJet,e));var covariantI=Add(ordinaryI,CurvedWard.Comm(connectionJet,inv));
   geometryPassed&=inverseHessian.Count==0&&reverseHessian.Count==0;
   if(a!=b){int mask=(1<<a)|(1<<b);antisymE=Add(antisymE,Product(One(mask,0,a<b?1:-1),covariantE));antisymI=Add(antisymI,Product(One(mask,0,a<b?1:-1),covariantI));}
   hessianRows.Add(new{a,b,ordinaryE=Terms(ordinaryE),ordinaryInverse=Terms(ordinaryI),connectionJet=Terms(connectionJet),covariantE=Terms(covariantE),covariantInverse=Terms(covariantI),inverseProductHessian=Terms(inverseHessian),reverseProductHessian=Terms(reverseHessian)});
  }
  var ee=CurvedWard.NormalSquare(f,e);var ii=CurvedWard.NormalSquare(f,inv);
  geometryPassed&=Equal(ee,antisymE)&&Equal(ii,antisymI);
  foreach(var pair in new[]{(ee,e),(ii,inv)}){Inc("normalSquareControls");geometryPassed&=Equal(pair.Item1,CurvedWard.Comm(f,pair.Item2));}
  for(int field=0;field<2;field++)
  {
   Inc("contexts");var s=field==0?Caa.Gamma1:One(1,2,1);var ds=field==0?new FT():One(5,6,1);var coord=CurvedWard.Coordinates(f,e,de,inv,di,s,ds);
   var baseline=CurvedWard.Evaluate(f,Jet.Fixed(e),Jet.Fixed(de),Jet.Fixed(inv),Jet.Fixed(di),Jet.Fixed(coord.Omega),Jet.Fixed(coord.DOmega));
   Rational[] predicted=field==0?[60,0,-4368,-7]:[0,1,0,new Rational(-1,2)];Rational[] fixedPredicted=field==0?[60,0,-13104,-14]:[0,2,0,-1];
   geometryPassed&=Equal(baseline.FB.Value,Conjugate(inv,f,e))&&Equal(baseline.Covariant.Value,Conjugate(inv,ds,e))&&Equal(baseline.T.Value,Conjugate(inv,s,e));
   if(mode==1){Inc("noncommutingGeometryRows");geometryPassed&=baseline.DB.Value.Count>0&&Product(coord.B,coord.B).Count>0&&ee.Count>0&&!Equal(baseline.FB.Value,f);}
   FT[] inputs=[f,ds,Product(s,s)];
   for(int k=0;k<3;k++){Inc("operatorDescentRows");actionPassed&=Equal(baseline.Stages[k][7].Value,Conjugate(inv,Caa.Forward(inputs[k]),e));}
   for(int k=0;k<4;k++){Inc("actionPieceRows");actionPassed&=baseline.Pieces[k]==predicted[k];}
   foreach(var pair in new[]{(baseline.T.Value,baseline.Stages[0][7].Value),(baseline.T.Value,baseline.Stages[1][7].Value),(baseline.T.Value,baseline.Stages[2][7].Value),(baseline.T.Value,baseline.T.Value)}){Inc("topTraceRows");actionPassed&=Pair(pair.Item1,pair.Item2)==Top(Product(pair.Item1,Star(pair.Item2)));}
   var parameterRows=new List<object>();foreach(int gamma in new[]{1,2})foreach(int kappa in new[]{0,1}){Inc("actionRows");var actual=CurvedWard.Total(baseline.Pieces,gamma,kappa);actionPassed&=actual==CurvedWard.Total(predicted,gamma,kappa)&&actual!=0;parameterRows.Add(new{gamma,kappa,action=actual.ToString()});}
   var variations=new List<object>();var eta=One(0,3,1);var deta=One(1,2,1);var ddeta=CurvedWard.NormalSquare(f,eta);
   var da=Add(deta,CurvedWard.Comm(coord.Omega,eta));var dda=Add(Add(ddeta,CurvedWard.Comm(coord.DOmega,eta)),Scale(CurvedWard.Anti(coord.Omega,deta),-1));
   for(int direction=0;direction<3;direction++)
   {
    Inc("variationContexts");var activeEta=direction==0?new FT():eta;var activeDeta=direction==0?new FT():deta;
    var deltaE=Product(e,activeEta);var deltaDE=Add(Product(de,activeEta),Product(e,activeDeta));var deltaI=Scale(Product(activeEta,inv),-1);var deltaDI=Scale(Add(Product(activeDeta,inv),Product(activeEta,di)),-1);
    var v=Conjugate(inv,s,e);var dv=Add(Add(Product(Product(di,s),e),Product(Product(inv,ds),e)),Scale(Product(Product(inv,s),de),-1));
    var deltaOmega=direction==0?v:direction==1?da:new FT();var deltaDOmega=direction==0?dv:direction==1?dda:new FT();
    var ej=new Jet(e,deltaE);var dej=new Jet(de,deltaDE);var ij=new Jet(inv,deltaI);var dij=new Jet(di,deltaDI);
    var actual=CurvedWard.Evaluate(f,ej,dej,ij,dij,new Jet(coord.Omega,deltaOmega),new Jet(coord.DOmega,deltaDOmega));
    Inc("inverseVariationRows");variationPassed&=Jet.Product(ej,ij).Delta.Count==0&&Jet.Product(ij,ej).Delta.Count==0&&Jet.Add(Jet.Product(dej,ij),Jet.Product(ej,dij)).Delta.Count==0&&Jet.Add(Jet.Product(dij,ej),Jet.Product(ij,dej)).Delta.Count==0;
    var pulled=CurvedWard.Conj(ej,actual.T,ij);
    var pulledD=Jet.Add(Jet.Add(Jet.Product(Jet.Product(dej,actual.T),ij),Jet.Product(Jet.Product(ej,actual.DT),ij)),Jet.Scale(Jet.Product(Jet.Product(ej,actual.T),dij),-1));
    // Independent coordinate formula for deltaS and its first covariant derivative.
    var z=direction==0?v:direction==1?new FT():Scale(da,-1);var dz=direction==0?dv:direction==1?new FT():Scale(dda,-1);
    var baseV=Conjugate(e,z,inv);var baseDV=Add(Add(Product(Product(de,z),inv),Product(Product(e,dz),inv)),Scale(Product(Product(e,z),di),-1));
    Inc("reconstructedFieldRows");variationPassed&=Equal(pulled.Value,s)&&Equal(pulled.Delta,baseV)&&Equal(pulledD.Value,ds)&&Equal(pulledD.Delta,baseDV);
    var expectedVariation=CurvedWard.BaseVariation(f,s,ds,baseV,baseDV);
    for(int k=0;k<4;k++){Inc("variationPieceRows");variationPassed&=actual.Derivatives[k]==expectedVariation[k];}
    if(direction==0)variationPassed&=CurvedWard.REqual(actual.Derivatives,fixedPredicted);
    if(direction==1)variationPassed&=actual.Derivatives.All(x=>x==0)&&baseV.Count==0&&baseDV.Count==0;
    foreach(var phi in new[]{(actual.FirstPhi,Caa.Gamma1),(actual.OuterPhi,Caa.Gamma1),(actual.InnerPhi,Caa.Gamma2)})
    {Inc("phiVariationRows");variationPassed&=Equal(phi.Item1.Delta,CurvedWard.Comm(Conjugate(inv,phi.Item2,e),activeEta));}
    if(direction==2&&mode==0&&field==1){Inc("epsilonOnlyAnchorRows");variationPassed&=CurvedWard.REqual(actual.Derivatives,[new Rational(21,2),new Rational(-21,16),0,1]);}
    var parameterVariations=new List<object>();foreach(int gamma in new[]{1,2})foreach(int kappa in new[]{0,1})
    {Inc(direction==0?"fixedDirectionRows":direction==1?"wardRows":"epsilonOnlyRows");var derivative=CurvedWard.Total(actual.Derivatives,gamma,kappa);variationPassed&=derivative==CurvedWard.Total(expectedVariation,gamma,kappa);parameterVariations.Add(new{gamma,kappa,derivative=derivative.ToString()});}
    variations.Add(new{direction,deltaE=Terms(deltaE),deltaDE=Terms(deltaDE),deltaInverse=Terms(deltaI),deltaDInverse=Terms(deltaDI),deltaOmega=Terms(deltaOmega),deltaDOmega=Terms(deltaDOmega),baseVariation=Terms(baseV),baseDerivativeVariation=Terms(baseDV),expectedPieces=expectedVariation.Select(x=>x.ToString()).ToArray(),actual=actual.Evidence(),parameterVariations});
   }
   rows.Add(new{point,mode,field,epsilon=Terms(e),epsilonDerivative=Terms(de),inverse=Terms(inv),inverseDerivative=Terms(di),normalSquare=Terms(ee),hessianRows,S=Terms(s),DS=Terms(ds),omega=Terms(coord.Omega),domega=Terms(coord.DOmega),baseline=baseline.Evidence(),parameterRows,variations});
  }
 }
 // Three independently forecast nonzero decoys, evaluated only at identity epsilon, K field, kappa0.
 var sk=One(1,2,1);var dsk=One(5,6,1);
 for(int decoy=0;decoy<3;decoy++)
 {
  var eta=decoy==0?new FT():One(0,3,1);var deta=decoy==0?sk:new FT();var ddeta=CurvedWard.NormalSquare(f,eta);
  var da=Add(deta,CurvedWard.Comm(sk,eta));var dda=Add(Add(ddeta,CurvedWard.Comm(dsk,eta)),Scale(CurvedWard.Anti(sk,deta),-1));
  var omegaDelta=decoy==0?Scale(da,-1):da;var domegaDelta=decoy==0?Scale(dda,-1):dda;
  var result=CurvedWard.Evaluate(f,new Jet(one,eta),new Jet(new(),deta),new Jet(one,Scale(eta,-1)),new Jet(new(),Scale(deta,-1)),new Jet(sk,omegaDelta),new Jet(dsk,domegaDelta),decoy==1,decoy==2);
  Rational target=decoy==0?-2:decoy==1?new Rational(-5,8):new Rational(-79,8);
  Rational[] pieces=decoy==0?[0,-2,0,2]:[target,0,0,0];
  Inc("decoyPieceRows");decoysPassed&=CurvedWard.REqual(result.Derivatives,pieces);
  foreach(int gamma in new[]{1,2}){Inc("decoyRows");var derivative=CurvedWard.Total(result.Derivatives,gamma,0);decoysPassed&=derivative==target&&derivative!=0;decoyRows.Add(new{point,decoy,gamma,kappa=0,expected=target.ToString(),derivative=derivative.ToString(),actual=result.Evidence()});}
 }
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");bool frequencyPassed=CurvedWard.FrequencyPassed;
bool resourcesPassed=frequencyPassed&&Matrix.Products<=resources.GetProperty("maximumTrackedMatrixProducts").GetInt64()&&CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumTensorTerms").GetInt32()&&CurvedWard.SquareEntries==resources.GetProperty("normalSquareEntries").GetInt64()&&CurvedWard.ChainStages==resources.GetProperty("dualChainStages").GetInt64();
geometryPassed&=CurvedWard.CarrierPassed;
bool controlsPassed=knownAnswerPassed&&inputPassed&&geometryPassed&&actionPassed&&variationPassed&&decoysPassed&&countsPassed&&resourcesPassed;
string verdict=!knownAnswerPassed?precedence[1]:!inputPassed||!geometryPassed?precedence[2]:!actionPassed?precedence[3]:!variationPassed?precedence[4]:!decoysPassed?precedence[5]:!countsPassed||!resourcesPassed?precedence[6]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,inputPassed,geometryPassed,actionPassed,variationPassed,decoysPassed,countsPassed,resourcesPassed,frequencyPassed,counts,trackedCoefficientProducts=CoefficientProducts,trackedMatrixProducts=Matrix.Products,largestTensor=LargestTensor,normalSquareEntries=CurvedWard.SquareEntries,dualChainStages=CurvedWard.ChainStages,pointRows,rows,decoyRows,scope=fx.GetProperty("scope")});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=620,phaseId="phase620-curved-caa-action-descent-ward-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/curved_caa_action_descent_ward_audit.json",json);File.WriteAllText(Root+"/output/curved_caa_action_descent_ward_audit_summary.json",json);Console.WriteLine($"Phase620 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
