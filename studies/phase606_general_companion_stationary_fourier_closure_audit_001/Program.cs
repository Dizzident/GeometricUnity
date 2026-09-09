using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static Cyclic;
using static Closure;
using static General;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase606_general_companion_stationary_fourier_closure_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P601="studies/phase601_full_hessian_cyclic_closure_audit_001";
const string P605="studies/phase605_stationary_background_fourier_closure_audit_001";
const string P592="studies/phase592_companion_tensor_chirality_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase592_companion_tensor_chirality_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase606-a57-general-companion-stationary-fourier-closure-v1";
const string Success="general-companion-fourier-closure-controls-pass-diagnostic-spectrum-nonunique";
const string FixtureJson="""
{
 "dimension": 14,
 "chiralities": [
  -1,
  1
 ],
 "frequencies": [
  0,
  1,
  2
 ],
 "kappas": [
  -1,
  0,
  1
 ],
 "cRationals": [
  [
   0,
   1
  ],
  [
   1,
   1
  ],
  [
   -1,
   1
  ],
  [
   5,
   7
  ],
  [
   -5,
   7
  ],
  [
   2,
   1
  ],
  [
   -2,
   1
  ]
 ],
 "gammaControl": 1,
 "gammaScope": "gamma!=0; gamma*S*=-kappa Pbar Gamma1/416 algebraically",
 "background": "flat fixed (7,7) metric/reference/epsilon/tensors; normalized periodic x0; source-induced metric vacuum not established",
 "basis": [
  "u",
  "b",
  "Omega u",
  "Omega b",
  "z=iZ_n",
  "oz=iOmega Z_n",
  "e",
  "e0",
  "q",
  "j=i theta2 I sin",
  "T=iOmega e",
  "T0=iOmega e0"
 ],
 "nonzeroGram": [
  "-1/2",
  "-1/2",
  "1/2",
  "1/2",
  "-6",
  "6",
  "6",
  "1/2",
  "-1/2",
  "1/2",
  "-6",
  "-1/2"
 ],
 "nonzeroDeterminant": "81/16",
 "constantBasis": "actual first six cosine fields at n0; sine fields absent",
 "constantGram": [
  -1,
  -1,
  1,
  1,
  -12,
  12
 ],
 "constantDeterminant": "-144",
 "phi1": "P Gamma1",
 "phi2": "(c-i h Omega)Gamma2",
 "P": "1+hOmega",
 "Pbar": "1-hOmega",
 "backgroundUnit": "Pbar Gamma1",
 "fullOperator": "D0+cD1+kappa(M0+cM1); Mslot=delta(slot0)I-(A+B+C)/1248",
 "rawLegOracle": "complete three-leg tensor table in bound GeneralClosure.LegsExpected and STUDY, both slots including nonzero canceled terms",
 "combinedOracle": "independent GeneralClosure.OriginExpected and PotentialExpected full12 column tables",
 "flagBasis": "Xplus,Aplus,Zplus; e,e0,q,j,T,T0; Xminus,Aminus,Zminus, minus=(1-hOmega)/2; n0 omit even6",
 "factorBasis": "Xplus,Aplus,Zplus; E=e+e0,q,U=T+T0,j,F=e-12e0,V=T-12T0; Xminus,Aminus,Zminus",
 "Qodd": "lambda^2-6kappa lambda/13+kappa^2(c^2-14)/26",
 "Qeven": "lambda^2-17kappa lambda/13+12kappa^2(5-c^2)/169",
 "Qhook": "lambda^2-30kappa lambda/13+kappa^2(8075+49c^2)/6084",
 "characteristic": "(lambda-15kappa/13)^2 Qodd^2 (lambda-kappa) lambda Qeven Qhook",
 "constantCharacteristic": "(lambda-15kappa/13)^2 Qodd^2",
 "certifiedStrata": "n0 or kappa0 or c0 only; remaining48 rows have no general minimal/Jordan or seed assertion",
 "constantMinimal": "(lambda-alpha)Qodd for kappa!=0, lambda otherwise",
 "zeroKappaMinimal": "lambda^3 at n!=0",
 "cZeroFullMinimal": "(lambda-alpha)^2(lambda+7kappa/13)^2(lambda-kappa)lambda(lambda-12kappa/13)(lambda-85kappa/78)(lambda-5kappa/13)(lambda-95kappa/78)",
 "seedDimensions": "n0:kappa0->1,c0->2,otherwise3; nonzero n:kappa0->3,c0 nonzero kappa->6; other rows unasserted",
 "nilpotentMinusPlus": [
  [
   "-24",
   "0",
   "-144c"
  ],
  [
   "0",
   "-2c^2",
   "12c"
  ],
  [
   "-24c",
   "2c",
   "4-156c^2"
  ]
 ],
 "nilpotentDeterminant": "192c^2(4-3c^2)n^6",
 "exceptionalMinor": "48c^2 n^4",
 "zeroKappaJordan": "c0:2J3+6J1; c^2=4/3:2J3+2J2+2J1; other realc:3J3+3J1; irrational stratum proof only, no rounded row",
 "cZeroShiftRanks": {
  "alpha": [
   11,
   10,
   10
  ],
  "beta": [
   11,
   10,
   10
  ],
  "kappa": [
   9,
   9,
   9
  ]
 },
 "hookDiscriminant": "kappa^2(25-49c^2)/1521",
 "hookCollision": "c=+/-5/7; even quotient block rank-one nilpotent part, no full collision Jordan assertion",
 "diagnosticComparison": "c0 vs c1 preserve bound592 conditional Riemann matching, but Qhook differs and c1 hook discriminant negative for kappa!=0",
 "knownAnswerGrades": [
  0,
  1,
  2,
  12,
  13,
  14
 ],
 "newGradeWordMenu": {
  "grades": [3, 4, 10, 11],
  "maskCount": 2730,
  "productsPerMask": 31,
  "products": "all 14 singleton generators on each side, Omega on each side, and square; selected products, not exhaustive pairs"
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "counts": {
  "wordCases": 44944,
  "newGradeWordCases": 84630,
  "hodgeCases": 16384,
  "arithmeticKnownAnswers": 4,
  "formalColumns": 120,
  "potentialLegChecks": 360,
  "originSlotChecks": 120,
  "affineSlotChecks": 120,
  "actionCoefficientChecks": 2592,
  "symbolicFlagRows": 6,
  "symbolicFactorRows": 6,
  "symbolicNilpotencyRows": 4,
  "rows": 126,
  "fullColumns": 1260,
  "gramEntries": 13608,
  "actionBilinears": 13608,
  "characteristicRows": 126,
  "powerRankRows": 126,
  "certifiedRows": 78,
  "unassertedMinimalRows": 48,
  "actualKrylovApplications": 238,
  "krylovRankChecks": 316,
  "cZeroJordanRows": 8,
  "hookRows": 84,
  "hookCollisionRows": 16,
  "diagnosticComparisonRows": 8
 },
 "resource": {
  "estimatedCpuSeconds": 60,
  "maximumEstimatedCpuSeconds": 180,
  "estimatedPeakBytes": 268435456,
  "maximumEstimatedPeakBytes": 536870912,
  "fullFourierTermBound": 112,
  "constantTermBound": 28,
  "maximumKrylovApplicationsPerRow": 7,
  "characteristicAlgorithm": "exact Faddeev-LeVerrier, degree<=12; no factorial12 determinant"
 },
 "allNewFilesSingleFinalNewline": true,
 "genericReachabilityClaimed": false,
 "physicalInstabilityClaimed": false,
 "sourceNormSelected": false
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","formal-source-column-control-failed","original-action-control-failed","symbolic-factor-control-failed","certified-stratum-control-failed","diagnostic-comparison-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["polynomial-helper"]=Root+"/GeneralClosure.cs",["project"]=Root+"/Phase606GeneralCompanionStationaryFourierClosureAudit.csproj",["study"]=Root+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",
 ["phase601-summary"]=P601+"/output/full_hessian_cyclic_closure_audit_summary.json",["phase601-contract"]=P601+"/preregistration/contract_v1.json",["matrix-helper"]=P601+"/CyclicPolynomial.cs",["phase605-summary"]=P605+"/output/stationary_background_fourier_closure_audit_summary.json",["phase605-contract"]=P605+"/preregistration/contract_v1.json",["source-hessian-helper"]=P605+"/FourierClosure.cs",["phase592-summary"]=P592+"/output/companion_tensor_chirality_audit_summary.json",["phase592-contract"]=P592+"/preregistration/contract_v1.json",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==606&&contract.GetProperty("contractId").GetString()==ContractId
  &&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(v=>v.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(v=>new Binding(v.GetProperty("id").GetString()!,v.GetProperty("path").GetString()!,v.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(v=>v.id).Distinct().Count()==paths.Count&&bindings.Select(v=>v.path).Distinct().Count()==paths.Count&&bindings.All(v=>paths.TryGetValue(v.id,out var p)&&p==v.path&&v.hashMatches);
 if(contractValid&&exactBindingsValid)
 {using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var files=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&files.Select(v=>v.GetProperty("path").GetString()).SequenceEqual(live)&&files.All(v=>Sha(v.GetProperty("path").GetString()!)==v.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase601","full-hessian-cyclic-closure-controls-pass-compression-not-spectrum"),("phase605","stationary-background-fourier-closure-controls-pass-no-physical-dispersion"),("phase592","companion-tensor-chirality-controls-pass-source-choice-open")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var upstream=ud.RootElement;bool ok=upstream.GetProperty("auditPassed").GetBoolean()&&upstream.GetProperty("contractValid").GetBoolean()&&upstream.GetProperty("exactBindingsValid").GetBoolean()&&upstream.GetProperty("coreSourceTreeValid").GetBoolean()&&upstream.GetProperty("verdictKind").GetString()==terminal&&upstream.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&upstream.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&upstream.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&upstream.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>upstream.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&upstream.GetProperty("externalReviewPending").GetBoolean()&&upstream.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchorsValid=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}



var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("counts");var count=expected.EnumerateObject().ToDictionary(v=>v.Name,_=>0);
bool knownAnswerPassed=true;int[] masks=Enumerable.Range(0,16384).Where(m=>new[]{0,1,2,12,13,14}.Contains(Degree(m))).ToArray();
foreach(int a in masks)foreach(int b in masks){Inc("wordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);}
foreach(int a in Enumerable.Range(0,16384).Where(m=>new[]{3,4,10,11}.Contains(Degree(m))))
{
 foreach(int b in Enumerable.Range(0,14).Select(i=>1<<i).Append(Full))
 {Inc("newGradeWordCases");knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);Inc("newGradeWordCases");knownAnswerPassed&=BladeSign(b,a)==WordSign(b,a);}
 Inc("newGradeWordCases");knownAnswerPassed&=BladeSign(a,a)==WordSign(a,a);
}
for(int m=0;m<16384;m++){Inc("hodgeCases");int d=Degree(m);knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((d*(14-d)+7)%2==0?1:-1);}
Rational[][] test=[[0,2,0],[3,0,0],[0,0,5]];CP cv=CP.Variable;
bool[] ka=[FastCharacteristic(test).Same(Characteristic(test)),SameMatrix(Multiply(test,Inverse(test)),Identity(3)),Rank(new Rational[][]{[1,2],[2,4]})==1,PSame(FastCharacteristic(new CP[][]{[cv,0],[0,cv+1]}),new CP[]{cv*(cv+1),(cv.Scale(-2)-1),1})];
foreach(bool v in ka){Inc("arithmeticKnownAnswers");knownAnswerPassed&=v;}
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts=count});return;}

bool formalPassed=true,actionPassed=true,factorPassed=true,strataPassed=true,diagnosticPassed=true,supportPassed=true,adjointPassed=true;
var formalOutput=new List<object>();var symbolicOutput=new List<object>();var rowOutput=new List<object>();
Rational[] cMenu=fx.GetProperty("cRationals").EnumerateArray().Select(v=>new Rational(v[0].GetInt64(),v[1].GetInt64())).ToArray();
foreach(int h in fx.GetProperty("chiralities").EnumerateArray().Select(v=>v.GetInt32()))
{
 var source=new SourceHessian(h);
 foreach(int n in fx.GetProperty("frequencies").EnumerateArray().Select(v=>v.GetInt32()))
 {
  FT[] basis=Basis12(n);int size=basis.Length;var gram=Gram(basis);var inverse=Inverse(gram);
  Rational[] diagonal=n==0?[-1,-1,1,1,-12,12]:[new(-1,2),new(-1,2),new(1,2),new(1,2),-6,6,6,new(1,2),new(-1,2),new(1,2),-6,new(-1,2)];
  var expectedGram=Zeros(size);for(int i=0;i<size;i++)expectedGram[i][i]=diagonal[i];
  bool gramOK=SameMatrix(gram,expectedGram)&&SameMatrix(Multiply(gram,inverse),Identity(size))&&diagonal.Aggregate(new Rational(1),(a,b)=>a*b)==(n==0?new Rational(-144):new Rational(81,16));
  formalPassed&=gramOK&&basis.All(t=>Typed(t,1)&&HAnti(t));
  var dTensors=new FT[2][];var mTensors=new FT[2][];var dMatrices=new Rational[2][][];var mMatrices=new Rational[2][][];var actionD=new Rational[2][][];var actionM=new Rational[2][][];
  for(int slot=0;slot<2;slot++)
  {
   dTensors[slot]=new FT[size];mTensors[slot]=new FT[size];
   var legExpected=LegsExpected(basis,h,slot);var dExpected=OriginExpected(basis,h,n,slot);var mExpected=PotentialExpected(basis,h,slot);
   for(int col=0;col<size;col++)
   {
    Inc("formalColumns");var legs=source.Legs(basis[col],slot);bool pass=true;
    for(int leg=0;leg<3;leg++){Inc("potentialLegChecks");pass&=Equal(legs[leg],legExpected[col][leg])&&Typed(legs[leg],1)&&HAnti(legs[leg]);}
    var dd=source.Origin(basis[col],slot);var mm=Sc(Sum(legs),new Rational(-1,1248));if(slot==0)mm=Sum(mm,basis[col]);
    dTensors[slot][col]=dd;mTensors[slot][col]=mm;Inc("originSlotChecks");Inc("affineSlotChecks");
    pass&=Equal(dd,dExpected[col])&&Equal(mm,mExpected[col])&&Equal(source.Apply(basis[col],1,slot),Sum(dd,mm))&&Typed(dd,1)&&HAnti(dd)&&Typed(mm,1)&&HAnti(mm);formalPassed&=pass;
    formalOutput.Add(new{h,n,slot,column=col,passed=pass,derivative=Fourier.Terms(dd),potential=Fourier.Terms(mm),legs=legs.Select(Fourier.Terms).ToArray()});
   }
   dMatrices[slot]=Columns(dTensors[slot].Select(t=>Coordinates(t,basis,inverse)));mMatrices[slot]=Columns(mTensors[slot].Select(t=>Coordinates(t,basis,inverse)));
   // Verify complete source-slot reconstruction before any finite parameter rows.
   for(int j=0;j<size;j++)formalPassed&=Equal(dTensors[slot][j],Linear(basis,dMatrices[slot].Select(row=>row[j]).ToArray()))&&Equal(mTensors[slot][j],Linear(basis,mMatrices[slot].Select(row=>row[j]).ToArray()));
   actionD[slot]=ActionSlot(basis,source,slot,false);var cubic=ActionSlot(basis,source,slot,true);
   actionM[slot]=ScaleMatrix(cubic,new Rational(-1,1248));if(slot==0)actionM[slot]=AddMatrix(actionM[slot],gram);
   var loweredD=Multiply(gram,dMatrices[slot]);var loweredM=Multiply(gram,mMatrices[slot]);
   for(int i=0;i<size;i++)for(int j=0;j<size;j++){Inc("actionCoefficientChecks",2);actionPassed&=actionD[slot][i][j]==loweredD[i][j]&&actionM[slot][i][j]==loweredM[i][j];}
  }
  var dPoly=PolynomialMatrix(dMatrices[0],dMatrices[1]);var mPoly=PolynomialMatrix(mMatrices[0],mMatrices[1]);
  var flagBasis=FlagBasis(basis,h);var flag=Columns(flagBasis.Select(t=>Coordinates(t,basis,inverse)));var flagInverse=Inverse(flag);
  var df=PTransform(flagInverse,dPoly,flag);var mf=PTransform(flagInverse,mPoly,flag);Inc("symbolicFlagRows");bool flagOK=true;
  for(int i=0;i<size;i++)for(int j=0;j<size;j++)
  {
   bool allowD=n!=0&&((i<3&&j>=3&&j<9)||(i>=3&&i<9&&j>=9));
   int Group(int x)=>x<3?0:n==0?2:x<9?1:2;
   if(!allowD)flagOK&=df[i][j].IsZero;if(Group(i)!=Group(j))flagOK&=mf[i][j].IsZero;
  }
  var factorBasis=FactorBasis(basis,h);var factorTransform=Columns(factorBasis.Select(t=>Coordinates(t,basis,inverse)));var factorInverse=Inverse(factorTransform);
  var mp=PTransform(factorInverse,mPoly,factorTransform);Inc("symbolicFactorRows");
  bool factors=flagOK&&PMatrixSame(mp,FactorMatrixExpected(h,n))&&PSame(FastCharacteristic(mp),Factors(n,1,CP.Variable));
  object? nilpotency=null;
  if(n!=0)
  {
   Inc("symbolicNilpotencyRows");var d2=PMultiply(df,df);var d3=PMultiply(d2,df);var squareExpected=PZero(12);CP c=CP.Variable;
   CP[][] small=[[-24,0,c.Scale(-144)],[0,(c*c).Scale(-2),c.Scale(12)],[c.Scale(-24),c.Scale(2),(CP)4-(c*c).Scale(156)]];
   for(int i=0;i<3;i++)for(int j=0;j<3;j++)squareExpected[i][j+9]=small[i][j].Scale(n*n);
   var actualSmall=Enumerable.Range(0,3).Select(i=>Enumerable.Range(9,3).Select(j=>d2[i][j]).ToArray()).ToArray();var determinant=Determinant(actualSmall);
   var minor=actualSmall[0][0]*actualSmall[1][1]-actualSmall[0][1]*actualSmall[1][0];
   bool nil=PMatrixSame(d2,squareExpected)&&PMatrixSame(d3,PZero(12))&&determinant.Same((c*c*((CP)4-(c*c).Scale(3))).Scale(192*n*n*n*n*n*n))&&minor.Same((c*c).Scale(48*n*n*n*n));
   factors&=nil;nilpotency=new{passed=nil,minusPlus=General.PText(actualSmall),determinant=determinant.Text(),nonzeroMinor=minor.Text(),irrationalExceptionalStratumIsProofOnly=true};
  }
  factorPassed&=factors;symbolicOutput.Add(new{h,n,passed=factors,flagDerivative=General.PText(df),flagPotential=General.PText(mf),factorPotential=General.PText(mp),characteristicCoefficientsInC=FastCharacteristic(mp).Select(v=>v.Text()).ToArray(),nilpotency});
  foreach(int kk in fx.GetProperty("kappas").EnumerateArray().Select(v=>v.GetInt32()))foreach(var c in cMenu)
  {
   Rational k=kk;Inc("rows");var images=Enumerable.Range(0,size).Select(j=>Sum(dTensors[0][j],Sc(dTensors[1][j],c),Sc(mTensors[0][j],k),Sc(mTensors[1][j],k*c))).ToArray();
   var matrix=Columns(images.Select(t=>Coordinates(t,basis,inverse)));bool full=gramOK;
   for(int j=0;j<size;j++){Inc("fullColumns");full&=Equal(images[j],Linear(basis,matrix.Select(row=>row[j]).ToArray()))&&Typed(images[j],1)&&HAnti(images[j]);supportPassed&=images[j].Count<=(n==0?28:112);}
   formalPassed&=full;var lowered=Multiply(gram,matrix);bool action=true;
   for(int i=0;i<size;i++)for(int j=0;j<size;j++){Inc("gramEntries");Inc("actionBilinears");action&=lowered[i][j]==actionD[0][i][j]+c*actionD[1][i][j]+k*actionM[0][i][j]+k*c*actionM[1][i][j];}
   action&=SameMatrix(lowered,Transpose(lowered));actionPassed&=action;
   Inc("characteristicRows");var characteristic=FastCharacteristic(matrix);bool characteristicOK=characteristic.Same(Factors(n,k,c));factorPassed&=characteristicOK;
   Inc("powerRankRows");var powerRanks=PowerRanks(matrix).Select(v=>(int)v.Numerator).ToArray();bool rankOK=powerRanks.SequenceEqual(ExpectedRanks(n,k,c));strataPassed&=rankOK;
   bool certified=n==0||kk==0||c==0;object? stratum=null;
   if(certified)
   {
    Inc("certifiedRows");var minimal=CertifiedMinimal(n,k,c);bool certifiedOK=SameMatrix(Evaluate(minimal,matrix),Zeros(size));
    int applications=n==0?(kk==0?1:3):kk==0?3:7;int dimension=n==0?(kk==0?1:c==0?2:3):kk==0?3:6;
    var iterates=new List<FT>{basis[0]};var vectors=new List<Rational[]>{Coordinates(basis[0],basis,inverse)};var prefixRanks=new List<int>{1};Inc("krylovRankChecks");
    for(int step=0;step<applications;step++)
    {
     Inc("actualKrylovApplications");var image=Sum(source.Apply(iterates[^1],k,0),Sc(source.Apply(iterates[^1],k,1),c));var vector=Coordinates(image,basis,inverse);
     certifiedOK&=Equal(image,Linear(basis,vector))&&vector.SequenceEqual(Multiply(matrix,vectors[^1].Select(v=>new[]{v}).ToArray()).Select(row=>row[0]))&&Typed(image,1)&&HAnti(image);
     supportPassed&=image.Count<=(n==0?28:112);iterates.Add(image);vectors.Add(vector);int rank=Rank(Columns(vectors));Inc("krylovRankChecks");prefixRanks.Add(rank);certifiedOK&=rank==System.Math.Min(step+2,dimension);
    }
    var cyclicGram=Gram(iterates.Take(dimension).ToArray());var cgdet=Determinant(cyclicGram.Select(row=>row.Select(v=>(CP)v).ToArray()).ToArray());certifiedOK&=!cgdet.IsZero;
    object? jordan=null;
    if(n!=0&&kk!=0)
    {
     Inc("cZeroJordanRows");var ar=PowerRanks(Shift(matrix,k*new Rational(15,13))).Select(v=>(int)v.Numerator).ToArray();var br=PowerRanks(Shift(matrix,k*new Rational(-7,13))).Select(v=>(int)v.Numerator).ToArray();var kr=PowerRanks(Shift(matrix,k)).Select(v=>(int)v.Numerator).ToArray();
     bool jo=ar.SequenceEqual(new[]{11,10,10})&&br.SequenceEqual(new[]{11,10,10})&&kr.SequenceEqual(new[]{9,9,9})&&minimal.Coefficients.Length==11&&dimension==6;certifiedOK&=jo;jordan=new{passed=jo,alphaRanks=ar,betaRanks=br,kappaRanks=kr};
    }
    strataPassed&=certifiedOK;stratum=new{passed=certifiedOK,minimalPolynomial=minimal.Text(),fullMinimalDegree=minimal.Coefficients.Length-1,seedDimension=dimension,seedRanks=prefixRanks,cyclicGramDeterminant=cgdet.Text(),actualSourceKrylovImages=iterates.Select(Fourier.Terms).ToArray(),jordan};
   }
   else Inc("unassertedMinimalRows");
   object? hook=null;
   if(n!=0)
   {
    Inc("hookRows");var hp=new Rational[][]{[k*new Rational(85,78),k*c*new Rational(-7*h,78)],[k*c*new Rational(7*h,78),k*new Rational(95,78)]};
    // Obtain the same quotient block independently from the FULL recovered matrix.
    var af=Multiply(Multiply(factorInverse,matrix),factorTransform);var actualHook=new Rational[][]{[af[7][7],af[7][8]],[af[8][7],af[8][8]]};
    Rational trace=actualHook[0][0]+actualHook[1][1],det=actualHook[0][0]*actualHook[1][1]-actualHook[0][1]*actualHook[1][0],disc=trace*trace-det*4;
    bool hok=SameMatrix(hp,actualHook)&&disc==k*k*(25-c*c*49)*new Rational(1,1521);
    if(kk!=0&&c*c==new Rational(25,49)){Inc("hookCollisionRows");var nil=Shift(actualHook,k*new Rational(15,13));hok&=Rank(nil)==1&&SameMatrix(Multiply(nil,nil),Zeros(2));}
    if(kk!=0&&c==1){Inc("diagnosticComparisonRows");hok&=disc.Numerator.Sign<0&&!FastCharacteristic(actualHook).Same(new CP(k*k*new Rational(8075,6084),k*new Rational(-30,13),1));}
    diagnosticPassed&=hok;hook=new{passed=hok,quotientMatrix=Cyclic.Text(actualHook),discriminant=disc.ToString(),nonrealPairWhenNegative=true,fullCollisionJordanClaimed=false};
   }
   rowOutput.Add(new{h,n,kappa=kk,c=c.ToString(),constantCarrier=n==0,passed=full&&action&&characteristicOK&&rankOK,gram=Cyclic.Text(gram),matrix=Cyclic.Text(matrix),loweredHessian=Cyclic.Text(lowered),characteristic=characteristic.Text(),powerRanks,fullSourceAffineColumnsReconstructed=true,minimalStratumCertified=certified,stratum,hook});
  }
 }
 adjointPassed&=source.AdjointPassed;
}
bool countsPassed=count.Count==expected.EnumerateObject().Count()&&count.All(v=>v.Value==expected.GetProperty(v.Key).GetInt32());
bool controlsPassed=formalPassed&&adjointPassed&&supportPassed&&actionPassed&&factorPassed&&strataPassed&&diagnosticPassed&&countsPassed;
string verdict=!formalPassed||!adjointPassed||!supportPassed?precedence[2]:!actionPassed?precedence[3]:!factorPassed?precedence[4]:!strataPassed||!countsPassed?precedence[5]:!diagnosticPassed?precedence[6]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,formalPassed,adjointPassed,supportPassed,actionPassed,factorPassed,strataPassed,diagnosticPassed,countsPassed,counts=count,formalColumns=formalOutput,symbolicRows=symbolicOutput,rows=rowOutput,
 fullSourceAffineSlotsChecked=true,independentOriginalActionBothSlots=true,symbolicAllCFactorChecked=true,actualConstantSixCarrier=true,genericMinimalClaimed=false,genericReachabilityClaimed=false,
 sameConditionalRiemannMatchingFromBound592=true,diagnosticSpectrumNonunique=true,physicalInstabilityClaimed=false,physicalDispersionClaimed=false,fixedGeometryConnectionCarrierOnly=true,sourceInducedMetricVacuumEstablished=false,sourceNormSelected=false,physicalScaleSelected=false});
void Inc(string key,int amount=1)=>count[key]+=amount;

void Emit(string terminal,object evidence)
{
 var result=new{schemaVersion=1,phase=606,phaseId="phase606-general-companion-stationary-fourier-closure-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(v=>v.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(v=>v.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/general_companion_stationary_fourier_closure_audit.json",json);File.WriteAllText(Root+"/output/general_companion_stationary_fourier_closure_audit_summary.json",json);Console.WriteLine($"Phase606 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
