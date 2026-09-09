using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static SpinGeometry;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase610_induced_spin_curvature_contraction_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P607="studies/phase607_source_induced_vertical_curvature_audit_001";
const string P608="studies/phase608_source_induced_ambient_ricci_audit_001";
const string P592="studies/phase592_companion_tensor_chirality_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase610-a58-induced-spin-curvature-contraction-v1";
const string Success="induced-spin-curvature-contraction-controls-pass-nonzero-conditional-source";
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
 "chiralities": [
  -1,
  1
 ],
 "formalCSlots": [
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
 "phi1": "(1+hOmega)Gamma1",
 "phi2Slots": [
  "-ihOmega Gamma2",
  "Gamma2"
 ],
 "firstTermSlots": [
  true,
  false
 ],
 "innerSlots": [
  "5hOmega top",
  "5i top"
 ],
 "zeroSlots": [
  "-5hOmega",
  "-5i"
 ],
 "outerSlots": [
  "10P Gamma1",
  "0"
 ],
 "firstLowerSlots": [
  "-P RicciGamma",
  "0"
 ],
 "finalSlots": [
  "-P EinsteinGamma",
  "0"
 ],
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
 "finalNonzeroCoefficients": 28,
 "coefficientSquare": "2115/4",
 "gammaCrossPair": "60",
 "selfPair": "0",
 "chiralityDifferenceSquare": "2115/2",
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
  "chainRows": 8,
  "invariantTensorRows": 8,
  "chainStageComparisons": 56,
  "chainTypeChecks": 64,
  "chainRealityChecks": 64,
  "innerOracleRows": 8,
  "firstOracleRows": 8,
  "outerOracleRows": 8,
  "finalOracleRows": 8,
  "literalChainAgreementRows": 8,
  "nonzeroInnerRows": 8,
  "nonzeroKRows": 4,
  "zeroCSlots": 4,
  "finalCoefficientCountRows": 4,
  "coefficientNormRows": 4,
  "crossPairRows": 4,
  "isotropicRows": 4,
  "chiralityDifferenceRows": 2,
  "chainTransportRows": 4
 },
 "resources": {
  "estimatedCpuSeconds": 60,
  "maximumEstimatedCpuSeconds": 180,
  "estimatedPeakBytes": 268435456,
  "maximumEstimatedPeakBytes": 805306368,
  "maximumTrackedMatrixProducts": 100000000,
  "maximumTrackedCoefficientProducts": 50000000,
  "maximumFourierTerms": 8281,
  "maximumMatrixDimension": 14,
  "largestScalarArrayEntries": 38416
 },
 "scope": {
  "sourceNormalizationSelected": false,
  "sourceNormSelected": false,
  "connectionFrameTransformed": false,
  "covariantGradientComputed": false,
  "inducedStationaryBackgroundRejected": false,
  "physicalSpectrumIdentified": false
 }
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","frame-curvature-control-failed","typed-spin-control-failed","literal-contraction-control-failed","diagnostic-resource-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase610InducedSpinCurvatureContractionAudit.csproj",["study"]=Root+"/STUDY.md",["spin-helper"]=Root+"/SpinGeometry.cs",
 ["phase608-summary"]=P608+"/output/source_induced_ambient_ricci_audit_summary.json",["phase608-contract"]=P608+"/preregistration/contract_v1.json",["phase608-program"]=P608+"/Program.cs",["phase608-helper"]=P608+"/AmbientGeometry.cs",["phase608-study"]=P608+"/STUDY.md",["phase608-project"]=P608+"/Phase608SourceInducedAmbientRicciAudit.csproj",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["geometry-helper"]=P607+"/VerticalGeometry.cs",["phase607-summary"]=P607+"/output/source_induced_vertical_curvature_audit_summary.json",["phase607-contract"]=P607+"/preregistration/contract_v1.json",
 ["phase592-study"]=P592+"/STUDY.md",["phase592-summary"]=P592+"/output/companion_tensor_chirality_audit_summary.json",["phase592-contract"]=P592+"/preregistration/contract_v1.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==610&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced"),("phase608","source-induced-ambient-ricci-controls-pass-conditional-curved-background"),("phase592","companion-tensor-chirality-controls-pass-source-choice-open")})
 {
  using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
  if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}
 }
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(3.7)","(3.8)","(3.10)","(3.15)","(3.17)","(9.1)","(9.4)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}


var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);void Inc(string k)=>counts[k]++;
bool knownAnswerPassed=true,framePassed=true,spinPassed=true,chainPassed=true,diagnosticsPassed=true;
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
var gammaOne=GammaOne();var gammaTwo=GammaTwo();var resultRows=new List<object>();var chainRows=new List<object>();var framed=new Rational[2][,,,];var savedK=new FT[2,2,2];
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
 for(int hi=0;hi<2;hi++)for(int slot=0;slot<2;slot++)
 {
  int h=hi==0?-1:1;var phi1=Chiral(gammaOne,h);var phi2=slot==0?Scale(Omega(gammaTwo),Scalar.I*(-h)):gammaTwo;
  Inc("chainRows");Inc("invariantTensorRows");chainPassed&=Typed(phi1,1)&&Typed(phi2,2)&&HAnti(phi1)&&HAnti(phi2);
  var actual=Stages(f,phi1,phi2,slot==0,false);var naive=Stages(f,phi1,phi2,slot==0,true);
  int[] degrees=[2,12,13,14,0,1,13,1];for(int stage=0;stage<8;stage++){Inc("chainTypeChecks");Inc("chainRealityChecks");chainPassed&=Typed(actual[stage],degrees[stage])&&HAnti(actual[stage]);if(stage>0){Inc("chainStageComparisons");chainPassed&=Equal(actual[stage],naive[stage]);}}
  var innerExpected=slot==0?One(Full,Full,5*h):One(Full,0,Scalar.I*5);var firstExpected=slot==0?Scale(Chiral(WeightedGamma(oracleRicci),h),-1):new FT();var outerExpected=slot==0?Scale(phi1,10):new FT();var finalExpected=slot==0?Scale(Chiral(WeightedGamma(oracleEinstein),h),-1):new FT();
  Inc("innerOracleRows");Inc("firstOracleRows");Inc("outerOracleRows");Inc("finalOracleRows");Inc("literalChainAgreementRows");Inc("nonzeroInnerRows");
  chainPassed&=Equal(actual[3],innerExpected)&&Equal(actual[4],Star(innerExpected))&&Equal(Star(actual[2]),firstExpected)&&Equal(actual[5],outerExpected)&&Equal(actual[7],finalExpected)&&Equal(actual[7],Chain(f,phi1,phi2,slot==0))&&actual[3].Count==1;
  var output=actual[7];savedK[point,hi,slot]=output;
  if(slot==0)
  {
   Inc("nonzeroKRows");Inc("finalCoefficientCountRows");Inc("coefficientNormRows");Inc("crossPairRows");Inc("isotropicRows");diagnosticsPassed&=output.Count>0&&output.Count==28&&CoefficientSquare(output)==new Rational(2115,4)&&Pair(gammaOne,output)==60&&Pair(output,output)==0;
  }
  else{Inc("zeroCSlots");diagnosticsPassed&=output.Count==0;}
  chainRows.Add(new{point,h,slot,firstEnabled=slot==0,stages=actual.Select(Terms).ToArray(),coefficientSquare=CoefficientSquare(output).ToString(),gammaCrossPair=Pair(gammaOne,output).ToString(),selfPair=Pair(output,output).ToString()});
 }
 Inc("chiralityDifferenceRows");var difference=Add(savedK[point,0,0],Scale(savedK[point,1,0],-1));diagnosticsPassed&=difference.Count==14&&CoefficientSquare(difference)==new Rational(2115,2);
 resultRows.Add(new{point,frame=e.Text(),inverse=inverse.Text(),frameDeterminant=determinant.ToString(),frameGram=metric.Text(),ricci=ricci.Text(),raisedRicci=raisedRicci.Text(),einstein=einstein.Text(),scalar=raisedRicci.Trace().ToString(),loweredCurvature=Nonzero(lowered),spinCurvature=Terms(f),mixedAnchor=Terms(anchor),wrongMetric=Terms(wrongMetric),wrongFactor=Terms(wrongFactor),wrongSign=Terms(wrongSign)});
}
for(int a=0;a<14;a++)for(int b=0;b<14;b++)for(int c=0;c<14;c++)for(int d=0;d<14;d++){Inc("pointTransportEntries");framePassed&=framed[0][a,b,c,d]==framed[1][a,b,c,d];}
for(int h=0;h<2;h++)for(int slot=0;slot<2;slot++){Inc("chainTransportRows");chainPassed&=Equal(savedK[0,h,slot],savedK[1,h,slot]);}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resources=fx.GetProperty("resources");bool resourcesPassed=Matrix.Products<=resources.GetProperty("maximumTrackedMatrixProducts").GetInt64()&&CoefficientProducts<=resources.GetProperty("maximumTrackedCoefficientProducts").GetInt64()&&LargestTensor<=resources.GetProperty("maximumFourierTerms").GetInt32();
bool controlsPassed=knownAnswerPassed&&framePassed&&spinPassed&&chainPassed&&diagnosticsPassed&&countsPassed&&resourcesPassed;string verdict=!knownAnswerPassed?precedence[1]:!framePassed?precedence[2]:!spinPassed?precedence[3]:!chainPassed?precedence[4]:!diagnosticsPassed||!countsPassed||!resourcesPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,framePassed,spinPassed,chainPassed,diagnosticsPassed,countsPassed,resourcesPassed,counts,trackedMatrixProducts=Matrix.Products,trackedCoefficientProducts=CoefficientProducts,largestTensor=LargestTensor,rows=resultRows,chainRows,sourceNormalizationSelected=false,sourceNormSelected=false,connectionFrameTransformed=false,covariantGradientComputed=false,inducedStationaryBackgroundRejected=false,physicalSpectrumIdentified=false});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=610,phaseId="phase610-induced-spin-curvature-contraction-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/induced_spin_curvature_contraction_audit.json",json);File.WriteAllText(Root+"/output/induced_spin_curvature_contraction_audit_summary.json",json);Console.WriteLine($"Phase610 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
