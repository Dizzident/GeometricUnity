using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static Cyclic;
using static Closure;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase605_stationary_background_fourier_closure_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P601="studies/phase601_full_hessian_cyclic_closure_audit_001";
const string P603="studies/phase603_homogeneous_invariant_stationary_family_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase592_companion_tensor_chirality_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase605-a56-stationary-background-fourier-closure-v1";
const string Success="stationary-background-fourier-closure-controls-pass-no-physical-dispersion";
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
 "background": "fixed flat metric/reference/epsilon/tensors; normalized periodic x0; first action only",
 "carrierScope": "full source columns on a closed fixed-geometry connection carrier; no full-theory physical dispersion",
 "chiralities": [
  -1,
  1
 ],
 "nonzeroFrequencies": [
  1,
  2
 ],
 "constantFrequency": 0,
 "kappas": [
  -1,
  0,
  1
 ],
 "gammaControl": 1,
 "gammaScope": "all nonzero gamma by exact gamma*S*=-kappa Pbar Gamma1/416, not a fit or coupling choice",
 "cControl": 0,
 "phi1": "P Gamma1, P=1+h Omega",
 "phi2": "(c-i h Omega)Gamma2",
 "backgroundUnit": "Pbar Gamma1, Pbar=1-h Omega",
 "backgroundScale": "t=-kappa/(416 gamma)",
 "operator": "H=H0+kappa I-kappa(A+B+C)/1248 at backgroundUnit; A=K DQ, B=DQ_V^adj K^adj Sunit, C=DQ_Sunit^adj K^adj V",
 "basis": [
  "u=theta0 gamma2 cos(nx0)",
  "b=theta2 gamma0 cos(nx0)",
  "Omega u",
  "Omega b",
  "e=sum_(j!=0,2)theta_j gamma2 gamma_j sin(nx0)",
  "e0=theta0 Gamma20 sin(nx0)",
  "q=theta2 Omega sin(nx0)"
 ],
 "nonzeroGram": [
  "-1/2",
  "-1/2",
  "1/2",
  "1/2",
  "6",
  "1/2",
  "-1/2"
 ],
 "nonzeroGramDeterminant": "-3/32",
 "constantBasis": [
  "theta0 gamma2",
  "theta2 gamma0",
  "theta0 Omega gamma2",
  "theta2 Omega gamma0"
 ],
 "constantGram": [
  -1,
  -1,
  1,
  1
 ],
 "constantGramDeterminant": "1",
 "constantSineDirectionsExist": false,
 "originColumns": [
  "n e",
  "n e",
  "-h n e",
  "-h n e",
  "-12n P(u+b)",
  "0",
  "0"
 ],
 "potentialLegs": {
  "oddInput": "v=u,b,Omega u,Omega b: (-48P v,-96 swapped(v),-48Pbar v)",
  "e": [
   "8(12e0+11e)",
   "96(12e0+11e)",
   "96e0+88e+1152h q"
  ],
  "e0": [
   "8e",
   "96e",
   "8e+96h q"
  ],
  "q": [
   "-96h(e+e0)",
   "0",
   "0"
  ]
 },
 "fullColumns": [
  "n e+kappa(14u+b)/13",
  "n e+kappa(u+14b)/13",
  "-h n e+kappa(14Omega u+Omega b)/13",
  "-h n e+kappa(Omega u+14Omega b)/13",
  "-12n P(u+b)+kappa(e/78-14e0/13-12h q/13)",
  "kappa(e0-7e/78-h q/13)",
  "kappa(q+h(e+e0)/13)"
 ],
 "characteristic": "(z-kappa)^2(z-15kappa/13)^2 z(z-12kappa/13)(z-85kappa/78)",
 "constantCharacteristic": "(z-kappa)^2(z-15kappa/13)^2",
 "genericMinimal": "(z-kappa)(z-15kappa/13)^2 z(z-12kappa/13)(z-85kappa/78)",
 "zeroKappaMinimal": "z^3",
 "constantMinimal": "(z-kappa)(z-15kappa/13) for kappa!=0; z otherwise",
 "genericKrylovRanks": [
  1,
  2,
  3,
  4,
  5,
  6,
  6,
  6
 ],
 "zeroKappaKrylovRanks": [
  1,
  2,
  3,
  3,
  3,
  3,
  3,
  3
 ],
 "constantNonzeroKrylovRanks": [
  1,
  2,
  2,
  2,
  2
 ],
 "constantZeroKrylovRanks": [
  1,
  1,
  1,
  1,
  1
 ],
 "genericPowerRanks": [
  6,
  6,
  6
 ],
 "zeroKappaPowerRanks": [
  2,
  1,
  0
 ],
 "constantNonzeroPowerRanks": [
  4,
  4,
  4
 ],
 "constantZeroPowerRanks": [
  0,
  0,
  0
 ],
 "adaptedBasis": [
  "z=P(u+b)",
  "E=e+e0",
  "q",
  "F=e-12e0",
  "w=Pbar(u+b)/2",
  "u-b",
  "Omega(u-b)"
 ],
 "alpha": "15kappa/13",
 "alphaShiftPowerRanks": [
  6,
  5,
  5
 ],
 "evenResolventEE": "26/(15 kappa)",
 "jordanSchurCoefficient": "-208 n^2/(5 kappa)",
 "genericCyclicGramNondegenerate": true,
 "actionOracle": "six ordered placements of S,V,W in B(T,K(T wedge T)), exchanged word pairs grouped before real Pair; independent forward quadratic and mass terms",
 "formalCLeakage": "n i theta2 I sin(nx0)+i kappa(5+8h Omega)Z_n/156",
 "Z": "sum_(j!=0,2)theta_j Gamma0j gamma2 cos(nx0)",
 "formalCPotentialLegs": [
  "4iPZ",
  "-112ih Omega Z",
  "-44iPbar Z"
 ],
 "generalCClosureClaimed": false,
 "knownAnswerGrades": [
  0,
  1,
  2,
  12,
  13,
  14
 ],
 "counts": {
  "wordCases": 44944,
  "hodgeCases": 16384,
  "matrixKnownAnswers": 4,
  "frequencyKnownAnswers": 3,
  "baseColumns": 36,
  "potentialLegChecks": 108,
  "originChecks": 36,
  "rows": 18,
  "fullColumns": 108,
  "gramEntries": 684,
  "actionBilinears": 684,
  "krylovApplications": 108,
  "krylovRankChecks": 126,
  "polynomialRows": 18,
  "jordanRows": 8,
  "formalCLegChecks": 12,
  "formalCLeakageRows": 12
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "resource": {
  "estimatedCpuSeconds": 30,
  "maximumEstimatedCpuSeconds": 120,
  "estimatedPeakBytes": 134217728,
  "maximumEstimatedPeakBytes": 268435456,
  "fullFourierTermBound": 36,
  "constantTermBound": 4,
  "formalCLeakageTermBound": 50,
  "maximumKrylovApplicationsPerRow": 7
 },
 "sourceNormSelected": false,
 "physicalTimeSelected": false,
 "physicalScaleSelected": false
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","full-source-column-control-failed","original-action-control-failed","cyclic-jordan-control-failed","formal-c-leakage-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["polynomial-helper"]=Root+"/FourierClosure.cs",["project"]=Root+"/Phase605StationaryBackgroundFourierClosureAudit.csproj",["study"]=Root+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",
 ["phase601-summary"]=P601+"/output/full_hessian_cyclic_closure_audit_summary.json",["phase601-contract"]=P601+"/preregistration/contract_v1.json",["matrix-helper"]=P601+"/CyclicPolynomial.cs",["phase603-summary"]=P603+"/output/homogeneous_invariant_stationary_family_audit_summary.json",["phase603-contract"]=P603+"/preregistration/contract_v1.json",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==605&&contract.GetProperty("contractId").GetString()==ContractId
  &&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(v=>v.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(v=>new Binding(v.GetProperty("id").GetString()!,v.GetProperty("path").GetString()!,v.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(v=>v.id).Distinct().Count()==paths.Count&&bindings.Select(v=>v.path).Distinct().Count()==paths.Count&&bindings.All(v=>paths.TryGetValue(v.id,out var p)&&p==v.path&&v.hashMatches);
 if(contractValid&&exactBindingsValid)
 {using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var files=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&files.Select(v=>v.GetProperty("path").GetString()).SequenceEqual(live)&&files.All(v=>Sha(v.GetProperty("path").GetString()!)==v.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase601","full-hessian-cyclic-closure-controls-pass-compression-not-spectrum"),("phase603","homogeneous-invariant-stationary-family-controls-pass-scale-unselected")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var upstream=ud.RootElement;bool ok=upstream.GetProperty("auditPassed").GetBoolean()&&upstream.GetProperty("contractValid").GetBoolean()&&upstream.GetProperty("exactBindingsValid").GetBoolean()&&upstream.GetProperty("coreSourceTreeValid").GetBoolean()&&upstream.GetProperty("verdictKind").GetString()==terminal&&upstream.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&upstream.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&upstream.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&upstream.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>upstream.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&upstream.GetProperty("externalReviewPending").GetBoolean()&&upstream.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchorsValid=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}


var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("counts");
int wordCases=0,hodgeCases=0,matrixKnownAnswers=0,frequencyKnownAnswers=0;bool knownAnswerPassed=true;
int[] masks=Enumerable.Range(0,16384).Where(m=>new[]{0,1,2,12,13,14}.Contains(Degree(m))).ToArray();
foreach(int a in masks)foreach(int b in masks){wordCases++;knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);}
for(int m=0;m<16384;m++){hodgeCases++;int d=Degree(m);knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((d*(14-d)+7)%2==0?1:-1);}
Rational[][] test=[[0,2],[3,0]];CP variable=CP.Variable;
bool[] matrixKnown=[SameMatrix(Multiply(test,Inverse(test)),Identity(2)),Rank(new Rational[][]{[1,2],[2,4]})==1,Characteristic(test).Same(new CP(-6,0,1)),((variable+1)*((CP)1-variable)).Same(new CP(1,0,-1))];
foreach(bool v in matrixKnown){matrixKnownAnswers++;knownAnswerPassed&=v;}
foreach(int n in new[]{0,1,2}){frequencyKnownAnswers++;var sin=Mode(n,true,0,0,1);var cos=Mode(n,false,0,0,1);knownAnswerPassed&=Equal(Partial(sin,0),Fourier.Scale(cos,n))&&Equal(Partial(cos,0),Fourier.Scale(sin,-n))&&(n!=0||sin.Count==0&&cos.Count==1);}
knownAnswerPassed&=wordCases==N("wordCases")&&hodgeCases==N("hodgeCases")&&matrixKnownAnswers==N("matrixKnownAnswers")&&frequencyKnownAnswers==N("frequencyKnownAnswers");
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,wordCases,hodgeCases,matrixKnownAnswers,frequencyKnownAnswers});return;}

int baseColumns=0,potentialLegChecks=0,originChecks=0,rows=0,fullColumns=0,gramEntries=0,actionBilinears=0,krylovApplications=0,krylovRankChecks=0,polynomialRows=0,jordanRows=0,formalCLegChecks=0,formalCLeakageRows=0;
bool sourceColumnsPassed=true,actionPassed=true,cyclicPassed=true,jordanPassed=true,formalCPassed=true,supportPassed=true,adjointPassed=true;
var rowOutput=new List<object>();var baseOutput=new List<object>();var leakageOutput=new List<object>();
foreach(int h in fx.GetProperty("chiralities").EnumerateArray().Select(v=>v.GetInt32()))
{
 var source=new SourceHessian(h);
 foreach(int n in fx.GetProperty("nonzeroFrequencies").EnumerateArray().Select(v=>v.GetInt32()).Append(fx.GetProperty("constantFrequency").GetInt32()))
 {
  FT[] basis=Basis(n);int size=basis.Length;var gram=Gram(basis);var inverse=Inverse(gram);var predictedGram=Zeros(size);
  Rational[] diagonal=n==0?[-1,-1,1,1]:[new(-1,2),new(-1,2),new(1,2),new(1,2),6,new(1,2),new(-1,2)];
  for(int i=0;i<size;i++)predictedGram[i][i]=diagonal[i];
  bool gramPassed=SameMatrix(gram,predictedGram)&&SameMatrix(Multiply(gram,inverse),Identity(size))&&Determinant(gram.Select(row=>row.Select(v=>(CP)v).ToArray()).ToArray()).Same((CP)(n==0?new Rational(1):new Rational(-3,32)));
  sourceColumnsPassed&=gramPassed&&basis.All(v=>Typed(v,1)&&HAnti(v));
  var expectedLegs=ExpectedLegs(basis,h);var expectedOrigin=ExpectedOrigin(basis,h,n);
  for(int j=0;j<size;j++)
  {
   baseColumns++;var legs=source.Legs(basis[j]);var origin=source.Origin(basis[j]);bool pass=true;
   for(int leg=0;leg<3;leg++){potentialLegChecks++;pass&=Equal(legs[leg],expectedLegs[j][leg])&&Typed(legs[leg],1)&&HAnti(legs[leg]);}
   originChecks++;pass&=Equal(origin,expectedOrigin[j])&&Typed(origin,1)&&HAnti(origin);sourceColumnsPassed&=pass;
   baseOutput.Add(new{h,n,column=j,passed=pass,origin=Fourier.Terms(origin),legs=legs.Select(Fourier.Terms).ToArray()});
  }
  // Independent original-action expansion is cached before coupling substitution.
  var actionQuadratic=ActionHessianUnit(basis,source,false);var actionCubic=ActionHessianUnit(basis,source,true);
  FT[]? cLegs=null;FT? cOrigin=null;FT? zz=null;
  if(n!=0)
  {
   zz=Z(n);cLegs=source.Legs(basis[0],1);cOrigin=source.Origin(basis[0],1);
   FT[] prediction=[Fourier.Scale(Chiral(zz,h),Scalar.I*4),Fourier.Scale(Omega(zz),Scalar.I*(-112*h)),Fourier.Scale(Chiral(zz,-h),Scalar.I*-44)];
   for(int leg=0;leg<3;leg++){formalCLegChecks++;formalCPassed&=Equal(cLegs[leg],prediction[leg])&&!Equal(cLegs[leg],new FT());}
   formalCPassed&=Equal(cOrigin,Mode(n,true,4,0,Scalar.I*n));
  }
  foreach(int kk in fx.GetProperty("kappas").EnumerateArray().Select(v=>v.GetInt32()))
  {
   Rational k=kk;rows++;var images=basis.Select(v=>source.Apply(v,k)).ToArray();
   var coordinates=images.Select(v=>Coordinates(v,basis,inverse)).ToArray();var matrix=Columns(coordinates);var predicted=ExpectedMatrix(h,n,k);
   bool columns=SameMatrix(matrix,predicted)&&gramPassed;
   for(int j=0;j<size;j++){fullColumns++;columns&=Equal(images[j],Linear(basis,coordinates[j]))&&Typed(images[j],1)&&HAnti(images[j]);supportPassed&=images[j].Count<=(n==0?4:36);}
   sourceColumnsPassed&=columns;
   var lowered=Multiply(gram,matrix);bool action=true;
   for(int i=0;i<size;i++)for(int j=0;j<size;j++)
   {gramEntries++;actionBilinears++;Rational scalarMixed=actionQuadratic[i][j]+k*new Rational(-1,1248)*actionCubic[i][j]+k*gram[i][j];action&=lowered[i][j]==scalarMixed;}
   action&=SameMatrix(lowered,Transpose(lowered));actionPassed&=action;
   var minimal=Minimal(n,k);var characteristic=Characteristic(matrix);var charExpected=CharacteristicExpected(n,k);polynomialRows++;
   bool polynomial=characteristic.Same(charExpected)&&SameMatrix(Evaluate(minimal,matrix),Zeros(size));
   var powers=PowerRanks(matrix).Select(v=>(int)v.Numerator).ToArray();
   int[] expectedPower=n==0?(kk==0?[0,0,0]:[4,4,4]):(kk==0?[2,1,0]:[6,6,6]);polynomial&=powers.SequenceEqual(expectedPower);
   // These are actual full-source iterates, not matrix multiplication substituted for H.
   var iterates=new List<FT>{basis[0]};var vectorColumns=new List<Rational[]>{Coordinates(basis[0],basis,inverse)};
   int applications=n==0?4:7;
   for(int step=0;step<applications;step++)
   {
    krylovApplications++;var image=source.Apply(iterates[^1],k);var vector=Coordinates(image,basis,inverse);
    bool full=Equal(image,Linear(basis,vector));var matrixNext=Multiply(matrix,vectorColumns[^1].Select(v=>new[]{v}).ToArray()).Select(row=>row[0]).ToArray();
    polynomial&=full&&vector.SequenceEqual(matrixNext)&&Typed(image,1)&&HAnti(image);supportPassed&=image.Count<=(n==0?4:36);
    iterates.Add(image);vectorColumns.Add(vector);
   }
   string rankKey=n==0?(kk==0?"constantZeroKrylovRanks":"constantNonzeroKrylovRanks"):(kk==0?"zeroKappaKrylovRanks":"genericKrylovRanks");
   int[] rankExpected=fx.GetProperty(rankKey).EnumerateArray().Select(v=>v.GetInt32()).ToArray();var ranks=new List<int>();
   for(int count=1;count<=vectorColumns.Count;count++){krylovRankChecks++;int rank=Rank(Columns(vectorColumns.Take(count)));ranks.Add(rank);polynomial&=rank==rankExpected[count-1];}
   int dimension=rankExpected[^1];var cycleGram=Gram(iterates.Take(dimension).ToArray());var cycleDet=Determinant(cycleGram.Select(row=>row.Select(v=>(CP)v).ToArray()).ToArray());
   polynomial&=!cycleDet.IsZero&&minimal.Coefficients.Length-1==dimension;
   cyclicPassed&=polynomial;
   object? jordan=null;
   if(n!=0&&kk!=0)
   {
    jordanRows++;var x=Fourier.Add(basis[0],basis[1]);var y=Fourier.Add(basis[0],Fourier.Scale(basis[1],-1));
    FT[] adapted=[Chiral(x,h),Fourier.Add(basis[4],basis[5]),basis[6],Fourier.Add(basis[4],Fourier.Scale(basis[5],-12)),Fourier.Scale(Chiral(x,-h),Fourier.Half),y,Omega(y)];
    var transform=Columns(adapted.Select(v=>Coordinates(v,basis,inverse)));var adaptedMatrix=Multiply(Multiply(Inverse(transform),matrix),transform);
    Rational alpha=k*new Rational(15,13);var even=Enumerable.Range(1,3).Select(i=>Enumerable.Range(1,3).Select(j=>adaptedMatrix[i][j]).ToArray()).ToArray();
    var resolvent=Inverse(ScaleMatrix(Shift(even,alpha),-1));Rational[] eCoordinates=[new(12,13),0,new(1,13)];
    var re=Multiply(resolvent,eCoordinates.Select(v=>new[]{v}).ToArray());Rational ee=re[0][0]+re[2][0];
    var right=Enumerable.Range(1,3).Select(i=>new[]{adaptedMatrix[i][4]}).ToArray();var rr=Multiply(resolvent,right);
    Rational schur=adaptedMatrix[0][4];for(int i=0;i<3;i++)schur+=adaptedMatrix[0][i+1]*rr[i][0];
    var shiftRanks=PowerRanks(Shift(matrix,alpha)).Select(v=>(int)v.Numerator).ToArray();
    bool jp=ee==new Rational(26,15)*Inv(k)&&schur==new Rational(-208*n*n,5)*Inv(k)&&schur!=0&&shiftRanks.SequenceEqual(new[]{6,5,5});
    jordanPassed&=jp;jordan=new{passed=jp,adaptedMatrix=Cyclic.Text(adaptedMatrix),evenResolvent=Cyclic.Text(resolvent),ee=ee.ToString(),schur=schur.ToString(),alphaShiftPowerRanks=shiftRanks};
   }
   if(n!=0)
   {
    formalCLeakageRows++;var actual=source.Apply(basis[0],k,1);
    var predictedLeak=Fourier.Add(Mode(n,true,4,0,Scalar.I*n),Fourier.Scale(Fourier.Add(Fourier.Scale(zz!,5),Fourier.Scale(Omega(zz!),8*h)),new Scalar(0,k*new Rational(1,156))));
    var projected=Linear(basis,Coordinates(actual,basis,inverse));
    bool lp=Equal(actual,predictedLeak)&&actual.Count>0&&projected.Count==0&&Typed(actual,1)&&HAnti(actual);
    formalCPassed&=lp;supportPassed&=actual.Count<=50;leakageOutput.Add(new{h,n,kappa=kk,passed=lp,fullCoefficient=Fourier.Terms(actual),projection=Fourier.Terms(projected)});
   }
   rowOutput.Add(new{h,n,kappa=kk,constantCarrier=n==0,passed=columns&&action&&polynomial,fullColumnsPassed=columns,actionBilinearsPassed=action,cyclicPassed=polynomial,
    gram=Cyclic.Text(gram),matrix=Cyclic.Text(matrix),loweredHessian=Cyclic.Text(lowered),characteristic=characteristic.Text(),minimalPolynomial=minimal.Text(),powerRanks=powers,cyclicRanks=ranks,cyclicDimension=dimension,cyclicGramDeterminant=cycleDet.Text(),fullImages=images.Select(Fourier.Terms).ToArray(),fullKrylovImages=iterates.Select(Fourier.Terms).ToArray(),jordan});
  }
 }
 adjointPassed&=source.AdjointPassed;
}
var counts=new Dictionary<string,int>{["wordCases"]=wordCases,["hodgeCases"]=hodgeCases,["matrixKnownAnswers"]=matrixKnownAnswers,["frequencyKnownAnswers"]=frequencyKnownAnswers,["baseColumns"]=baseColumns,["potentialLegChecks"]=potentialLegChecks,["originChecks"]=originChecks,["rows"]=rows,["fullColumns"]=fullColumns,["gramEntries"]=gramEntries,["actionBilinears"]=actionBilinears,["krylovApplications"]=krylovApplications,["krylovRankChecks"]=krylovRankChecks,["polynomialRows"]=polynomialRows,["jordanRows"]=jordanRows,["formalCLegChecks"]=formalCLegChecks,["formalCLeakageRows"]=formalCLeakageRows};
bool countsPassed=counts.Count==expected.EnumerateObject().Count()&&counts.All(v=>expected.GetProperty(v.Key).GetInt32()==v.Value);
bool controlsPassed=sourceColumnsPassed&&adjointPassed&&supportPassed&&actionPassed&&cyclicPassed&&jordanPassed&&formalCPassed&&countsPassed;
string verdict=!sourceColumnsPassed||!adjointPassed||!supportPassed?precedence[2]:!actionPassed?precedence[3]:!cyclicPassed||!jordanPassed||!countsPassed?precedence[4]:!formalCPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,sourceColumnsPassed,adjointPassed,supportPassed,actionPassed,cyclicPassed,jordanPassed,formalCPassed,countsPassed,counts,baseColumns=baseOutput,rows=rowOutput,formalCLeakageRows=leakageOutput,
 fullSourceColumnsReconstructed=true,independentOriginalActionUsed=true,actualZeroFrequencyCarrierUsed=true,fullSourceKrylovUsed=true,fixedGeometryConnectionCarrierOnly=true,combinedMetricConnectionHessianTested=false,
 generalCClosureClaimed=false,physicalDispersionClaimed=false,physicalTimeSelected=false,sourceNormSelected=false,physicalScaleSelected=false});
int N(string key)=>expected.GetProperty(key).GetInt32();

void Emit(string terminal,object evidence)
{
 var result=new{schemaVersion=1,phase=605,phaseId="phase605-stationary-background-fourier-closure-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(v=>v.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(v=>v.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/stationary_background_fourier_closure_audit.json",json);File.WriteAllText(Root+"/output/stationary_background_fourier_closure_audit_summary.json",json);Console.WriteLine($"Phase605 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();

