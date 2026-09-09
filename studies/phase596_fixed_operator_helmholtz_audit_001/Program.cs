using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Linear;

const string Root="studies/phase596_fixed_operator_helmholtz_audit_001";
const string P594="studies/phase594_actual_gradient_reciprocity_audit_001";
const string P592="studies/phase592_companion_tensor_chirality_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=Root+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase596-a51-fixed-operator-helmholtz-v1";
const string Success="fixed-operator-helmholtz-controls-pass-conditional-classification-only";
const string FixtureJson="""
{
 "spatialDimension":3,"gaugeRealDimension":4,"matrixDimension":2,
 "gaugeBasis":["i I","sigma1","sigma2","i sigma3"],"hermitianForm":"diag(1,-1)",
 "gaugeGram":[1,-1,-1,1],"pairing":"-ReTr(XY)/2; spatial axes positive; output coefficients lowered with this pairing",
 "brackets":[[1,2,3,2],[2,3,1,-2],[3,1,2,-2]],
 "fieldIndex":"4*spatialIndex+gaugeIndex","curvaturePairs":[[0,1],[0,2],[1,2]],
 "operatorIndex":"12*outputIndex+curvatureIndex","coefficientConvention":"E_ia=sum_(j<k,b) L_ia,jkb F_jk^b; antisymmetric extension equals half of the all-index sum",
 "quadraticMonomials":"t_i*t_j for0<=i<=j<12; off-diagonal symmetric tensor has no half",
 "principalRows":"derivative axis0..2; output pair I<=J; M_axis[I,J]+M_axis[J,I]",
 "cubicRows":"output pair I<J; coefficient variable0..11; partial_J E_I-partial_I E_J",
 "reducedRows":"generator z=0..3; Lie pair a<=b; h([ez,ea],eb)+h(ea,[ez,eb])",
 "reducedBilinearBasis":"symmetric4x4 coordinate matrices, row-major upper triangle",
 "predictedCounts":{"operatorVariables":144,"principalRows":234,"principalRank":134,"principalNullity":10,
  "reducedRows":40,"reducedRank":8,"reducedNullity":2,"cubicRows":792,"combinedRows":1026,"combinedRank":142,"combinedNullity":2,
  "quadraticMonomials":78,"qRows":12,"qEntries":936,"qRank":9,"qKernelDimension":69,
  "orderedBrackets":16,"jacobiTriples":64,"gramEntries":16,"realFormChecks":4,
  "spatialThreeMasks":364,"sameBlockFlips":42,"spatialCharacterChecks":15288,"spatialThreeSurvivors":0,
  "subgroupControlMasks":4,"subgroupControlCharacterChecks":168},
 "survivingBilinears":[[1,0,0,0],[0,-1,-1,1]],
 "survivingOperator":"L_ia,jkb=epsilon_ijk*h_ab; raised K uses G_internal^-1*h, not h alone",
 "negativeControls":{"repeatedSpatial":"L_(0,0),(01,0)=1 only","principalMaximumDefect":2,
  "noninvariantSymmetricBilinear":[1,1,1,1],"reducedMaximumDefect":4,"cubicMaximumDefect":4},
 "nonabelianPositive":{"A":["x theta0 sigma1","y theta1 sigma2","z theta2 i sigma3"],
  "action":"2xyz","forceCovector":["2yz","2xz","2xy"]},
 "fourier":{"period":"2pi","integration":"normalized average (1/2pi) integral_0^(2pi)","coefficientRange":"finite exact Gaussian-rational Fourier modes",
  "positive":{"lambda":"1","A1":"p cos(theta) iI","A2":"q sin(theta) iI","action":"-pq/2","force":["-q/2","-p/2"]},
  "closednessDecoy":{"lambda":"cos(theta)","A1":"p sin(theta) iI","A2":"q iI","action":"pq/4","actualDerivative":["q/4","p/4"],"desiredForce":["0","p/2"]},
  "actualEuler":"one half of L+L_adjoint; contains +(1/2)*d(lambda) wedge A"},
 "subgroupControlMasks":[0,127,16256,16383],
 "hypotheses":["fixed nondegenerate pairing and background","unrestricted bosonic u(H)-valued connection variations","zero-order linear K independent of varied connection","curvature-only target K(dA+A wedge A)","Spin-intertwining additionally required only for the conditional14D zero conclusion"],
 "fullU128CoefficientCensusPerformed":false,"sourceOperatorSelected":false,
 "exactTolerance":0,"rankControl":{"finiteFieldPrime":1000003,"primeTrialDivisors":"all integers2..1000","knownRanks":[1,2,0],"noninvertibleDenominatorRejected":true,"independentRankLowerBound":true,"rationalKernelAndRrefConsistencyIsNotStandaloneRankCertificate":true},"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":20,"maximumEstimatedCpuSeconds":60,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":268435456}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
 "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
 "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","principal-rank-control-failed","cubic-classification-control-failed","positive-or-decoy-control-failed","conditional-spin-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["controls-helper"]=Root+"/ExactControls.cs",
 ["project"]=Root+"/Phase596FixedOperatorHelmholtzAudit.csproj",["study"]=Root+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["phase594-summary"]=P594+"/output/actual_gradient_reciprocity_audit_summary.json",["phase594-contract"]=P594+"/preregistration/contract_v1.json",
 ["polynomial-helper"]=P594+"/PolynomialTensor.cs",["algebra-helper"]=P592+"/ExactAlgebra.cs",
 ["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var doc=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=doc.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==596
  &&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
  &&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()
  &&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
  &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(v=>v.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
  &&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(v=>new Binding(v.GetProperty("id").GetString()!,v.GetProperty("path").GetString()!,v.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(v=>v.id).Distinct().Count()==paths.Count&&bindings.Select(v=>v.path).Distinct().Count()==paths.Count
  &&bindings.All(v=>paths.TryGetValue(v.id,out var p)&&p==v.path&&v.hashMatches);
 if(contractValid&&exactBindingsValid)
 {
  using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;
  var entries=m.GetProperty("files").EnumerateArray().ToArray();
  coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1
   &&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
   &&live.Length==contract.GetProperty("fixtures").GetProperty("coreFileCount").GetInt32()
   &&entries.Select(v=>v.GetProperty("path").GetString()).SequenceEqual(live)
   &&entries.All(v=>Sha(v.GetProperty("path").GetString()!)==v.GetProperty("sha256").GetString())
   &&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();
 }
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}

using(var ud=JsonDocument.Parse(File.ReadAllBytes(paths["phase594-summary"])))
{
 var u=ud.RootElement;bool valid=u.GetProperty("auditPassed").GetBoolean()&&u.GetProperty("exactBindingsValid").GetBoolean()
 &&u.GetProperty("coreSourceTreeValid").GetBoolean()&&u.GetProperty("contractSha256").GetString()==Sha(paths["phase594-contract"])
 &&u.GetProperty("verdictKind").GetString()=="actual-gradient-controls-pass-curvature-only-force-rejected"
 &&u.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&u.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()
 &&u.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>u.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)
 &&u.GetProperty("externalReviewPending").GetBoolean()&&u.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
 string source=File.ReadAllText(paths["primary-source"]);valid&=new[]{"(8.1)","(8.7)","(9.3)","(9.4)","(9.7)"}.All(source.Contains);
 if(!valid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstreamAndSourceValid=false});return;}
}
var fx=contract.GetProperty("fixtures");var counts=fx.GetProperty("predictedCounts");
Scalar zero=0,one=1,minus=-1,imaginary=Scalar.I;
Scalar[][,] basis=[
 new Scalar[,]{{imaginary,zero},{zero,imaginary}},
 new Scalar[,]{{zero,one},{one,zero}},
 new Scalar[,]{{zero,imaginary*-1},{imaginary,zero}},
 new Scalar[,]{{imaginary,zero},{zero,imaginary*-1}}];
Scalar[,] h={{one,zero},{zero,minus}};
Rational[] gram=[1,-1,-1,1];var structure=new Rational[4,4,4];var oracle=new Rational[4,4,4];
foreach(var row in fx.GetProperty("brackets").EnumerateArray())
{int a=row[0].GetInt32(),b=row[1].GetInt32(),c=row[2].GetInt32(),v=row[3].GetInt32();oracle[a,b,c]=v;oracle[b,a,c]=-v;}
int gramEntries=0,realFormChecks=0,orderedBrackets=0,jacobiTriples=0;
bool gramPassed=true,realFormPassed=true,bracketsPassed=true,jacobiPassed=true;
var bracketRows=new List<object>();
for(int a=0;a<4;a++)
{
 realFormChecks++;realFormPassed&=Matrix.Zero(Matrix.Add(Matrix.Multiply(Matrix.Dagger(basis[a]),h),Matrix.Multiply(h,basis[a])));
 for(int b=0;b<4;b++)
 {
  gramEntries++;gramPassed&=Matrix.Pair(basis[a],basis[b])==(a==b?gram[a]:0);
  orderedBrackets++;var bracket=Matrix.Bracket(basis[a],basis[b]);var coordinates=new Rational[4];
  for(int c=0;c<4;c++){coordinates[c]=Matrix.Pair(basis[c],bracket)*Inv(gram[c]);structure[a,b,c]=coordinates[c];bracketsPassed&=coordinates[c]==oracle[a,b,c];}
  bracketsPassed&=Matrix.Same(bracket,Matrix.Combine(basis,coordinates));bracketRows.Add(new{a,b,coefficients=coordinates.Select(x=>x.ToString()).ToArray()});
 }
}
for(int a=0;a<4;a++)for(int b=0;b<4;b++)for(int c=0;c<4;c++)
{
 jacobiTriples++;var jacobi=Matrix.Add(Matrix.Add(Matrix.Bracket(basis[a],Matrix.Bracket(basis[b],basis[c])),
  Matrix.Bracket(basis[b],Matrix.Bracket(basis[c],basis[a]))),Matrix.Bracket(basis[c],Matrix.Bracket(basis[a],basis[b])));
 jacobiPassed&=Matrix.Zero(jacobi);
 for(int d=0;d<4;d++){Rational v=0;for(int e=0;e<4;e++)v+=structure[b,c,e]*structure[a,e,d]+structure[c,a,e]*structure[b,e,d]+structure[a,b,e]*structure[c,e,d];jacobiPassed&=v==0;}
}
var linearKnown=new Rational[][]{[1,2,0],[0,0,1]};var solvedKnown=Solve(linearKnown);
var cos=Fourier.Cos();var sin=Fourier.Sin();var unit=Fourier.Constant(Polynomial.Constant(1));
bool fourierKnown=cos.Derivative().Same(sin.Scale(-1))&&sin.Derivative().Same(cos)
 &&(cos*cos).Average().Same(Polynomial.Constant(new Scalar(new Rational(1,2),0)))
 &&(sin*sin).Average().Same(Polynomial.Constant(new Scalar(new Rational(1,2),0)))&&(cos*sin).Average().IsZero
 &&(sin*cos).Derivative().Same(cos*cos+(sin*sin).Scale(-1));
bool knownAnswerPassed=gramPassed&&realFormPassed&&bracketsPassed&&jacobiPassed&&fourierKnown
 &&KernelRrefConsistency(linearKnown,solvedKnown)&&solvedKnown.Rank==2&&solvedKnown.Kernel.Length==1
 &&PrimeKnownAnswer()&&ModularKnownAnswers()&&Prime==fx.GetProperty("rankControl").GetProperty("finiteFieldPrime").GetInt64()&&ModularRank(linearKnown)==2
 &&new Rational(1,2)+new Rational(1,3)==new Rational(5,6)&&Scalar.I*Scalar.I==new Scalar(-1)
 &&gramEntries==N("gramEntries")&&realFormChecks==N("realFormChecks")&&orderedBrackets==N("orderedBrackets")&&jacobiTriples==N("jacobiTriples");
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,gramPassed,realFormPassed,bracketsPassed,jacobiPassed,fourierKnown});return;}

(int A,int B)[] pairs=[(0,1),(0,2),(1,2)];
var symmetric=new List<(int A,int B)>();for(int a=0;a<4;a++)for(int b=a;b<4;b++)symmetric.Add((a,b));
var monomials=new List<(int A,int B)>();for(int a=0;a<12;a++)for(int b=a;b<12;b++)monomials.Add((a,b));
var monoIndex=monomials.Select((m,i)=>(m,i)).ToDictionary(x=>x.m,x=>x.i);
var principal=new List<Rational[]>();var principalLabels=new List<object>();
for(int derivative=0;derivative<3;derivative++)for(int i=0;i<12;i++)for(int j=i;j<12;j++)
{
 var row=new Rational[144];AddSymbol(row,i,derivative,j,1);AddSymbol(row,j,derivative,i,1);
 principal.Add(row);principalLabels.Add(new{derivative,i,j});
}
var principalMatrix=principal.ToArray();var principalSolve=Solve(principalMatrix);
var symmetricMatrices=symmetric.Select(s=>{var b=Zeros(4,4);b[s.A][s.B]=1;b[s.B][s.A]=1;return b;}).ToArray();
var lifts=symmetricMatrices.Select(Lift).ToArray();
bool principalPassed=principal.Count==N("principalRows")&&principalSolve.Rank==N("principalRank")&&principalSolve.Kernel.Length==N("principalNullity")
 &&KernelRrefConsistency(principalMatrix,principalSolve)&&ModularRank(principalMatrix)==N("principalRank")&&lifts.All(v=>IsZero(Product(principalMatrix,v)))&&Solve(lifts).Rank==10
 &&principalSolve.Kernel.All(v=>{var b=ExtractB(v);return Symmetric(b)&&Lift(b).SequenceEqual(v);});
if(!principalPassed){Emit(precedence[2],new{knownAnswerPassed,controlsPassed=false,principalPassed,principal=RankEvidence(principalMatrix,principalSolve)});return;}

// Complete curvature coefficient map: literal matrix brackets are not replaced by the invariant-pairing prediction.
var qmap=Zeros(12,78);
for(int pair=0;pair<3;pair++)for(int a=0;a<4;a++)for(int b=0;b<4;b++)for(int c=0;c<4;c++)
{
 int first=4*pairs[pair].A+a,second=4*pairs[pair].B+b;
 qmap[4*pair+c][monoIndex[(first,second)]]+=structure[a,b,c];
}
var qSolve=Solve(qmap);
bool qPassed=qmap.Length==N("qRows")&&monomials.Count==N("quadraticMonomials")&&qmap.Sum(row=>row.Length)==N("qEntries")
 &&qSolve.Rank==N("qRank")&&qSolve.Kernel.Length==N("qKernelDimension")&&KernelRrefConsistency(qmap,qSolve)&&ModularRank(qmap)==N("qRank")
 &&Enumerable.Range(0,3).All(p=>IsZero(qmap[4*p]));
var reduced=new List<Rational[]>();var reducedLabels=new List<object>();
for(int generator=0;generator<4;generator++)foreach(var pair in symmetric)
{
 var row=new Rational[10];for(int k=0;k<10;k++)for(int d=0;d<4;d++)
  row[k]+=structure[generator,pair.A,d]*symmetricMatrices[k][d][pair.B]+structure[generator,pair.B,d]*symmetricMatrices[k][pair.A][d];
 reduced.Add(row);reducedLabels.Add(new{generator,a=pair.A,b=pair.B});
}
var reducedMatrix=reduced.ToArray();var reducedSolve=Solve(reducedMatrix);
// Eight independent equations derived directly from the matrix-unit/simple-ideal proof.
var reducedOracle=Zeros(8,10);int[] killed=[1,2,3,5,6,8];for(int i=0;i<6;i++)reducedOracle[i][killed[i]]=1;
reducedOracle[6][4]=1;reducedOracle[6][9]=1;reducedOracle[7][7]=1;reducedOracle[7][9]=1;
var oracleSolve=Solve(reducedOracle);
bool reducedPassed=reduced.Count==N("reducedRows")&&reducedSolve.Rank==N("reducedRank")&&reducedSolve.Kernel.Length==N("reducedNullity")
 &&KernelRrefConsistency(reducedMatrix,reducedSolve)&&ModularRank(reducedMatrix)==N("reducedRank")&&oracleSolve.Rank==8
 &&reducedSolve.Rows.Take(8).Zip(oracleSolve.Rows,(a,b)=>a.SequenceEqual(b)).All(x=>x);

// Independently differentiate every output of L*Q; no principal reduction is substituted into these rows.
var cubic=new List<Rational[]>();var cubicLabels=new List<object>();
for(int i=0;i<12;i++)for(int j=i+1;j<12;j++)for(int coefficient=0;coefficient<12;coefficient++)
{
 var row=new Rational[144];
 int mj=monoIndex[(System.Math.Min(j,coefficient),System.Math.Max(j,coefficient))],mi=monoIndex[(System.Math.Min(i,coefficient),System.Math.Max(i,coefficient))];
 for(int w=0;w<12;w++)
 {row[12*i+w]+=qmap[w][mj]*(j==coefficient?2:1);row[12*j+w]=row[12*j+w]-qmap[w][mi]*(i==coefficient?2:1);}
 cubic.Add(row);cubicLabels.Add(new{i,j,coefficient});
}
var cubicMatrix=cubic.ToArray();var combined=principalMatrix.Concat(cubicMatrix).ToArray();var combinedSolve=Solve(combined);
Rational[][] center=Zeros(4,4),simple=Zeros(4,4);center[0][0]=1;simple[1][1]=-1;simple[2][2]=-1;simple[3][3]=1;
Rational[][] survivorOperators=[Lift(center),Lift(simple)];
bool combinedPassed=cubic.Count==N("cubicRows")&&combined.Length==N("combinedRows")&&combinedSolve.Rank==N("combinedRank")
 &&combinedSolve.Kernel.Length==N("combinedNullity")&&KernelRrefConsistency(combined,combinedSolve)&&ModularRank(combined)==N("combinedRank")
 &&survivorOperators.All(v=>IsZero(Product(combined,v)))&&Solve(survivorOperators).Rank==2
 &&combinedSolve.Kernel.All(v=>{var b=ExtractB(v);var expected=Zeros(4,4);expected[0][0]=b[0][0];expected[1][1]=b[3][3]*-1;expected[2][2]=b[3][3]*-1;expected[3][3]=b[3][3];return SameMatrix(b,expected)&&Lift(b).SequenceEqual(v);});
if(!qPassed||!reducedPassed||!combinedPassed)
{Emit(precedence[3],new{knownAnswerPassed,controlsPassed=false,qPassed,reducedPassed,combinedPassed,q=RankEvidence(qmap,qSolve),reduced=RankEvidence(reducedMatrix,reducedSolve),combined=RankEvidence(combined,combinedSolve)});return;}

// Constant-mode nonabelian positive: compute the scalar action directly with 2x2 polynomial matrices.
var x=Polynomial.Variable(0);var y=Polynomial.Variable(1);var z=Polynomial.Variable(2);
var a0=Matrix.PScale(basis[1],x);var a1=Matrix.PScale(basis[2],y);var a2=Matrix.PScale(basis[3],z);
var f01=Matrix.PBracket(a0,a1);var f02=Matrix.PBracket(a0,a2);var f12=Matrix.PBracket(a1,a2);
var minusF02=new[,]{{f02[0,0]*-1,f02[0,1]*-1},{f02[1,0]*-1,f02[1,1]*-1}};
var action=(Matrix.PPair(a0,f12)+Matrix.PPair(a1,minusF02)+Matrix.PPair(a2,f01))*new Scalar(new Rational(1,3),0);
Polynomial[] force=[Matrix.PPair(Matrix.PScale(basis[1],Polynomial.Constant(1)),f12),
 Matrix.PPair(Matrix.PScale(basis[2],Polynomial.Constant(1)),minusF02),Matrix.PPair(Matrix.PScale(basis[3],Polynomial.Constant(1)),f01)];
Polynomial[] direct=Enumerable.Range(0,3).Select(action.Derivative).ToArray();
bool nonabelianPassed=action.Same(x*y*z*2)&&SamePolys(force,[y*z*2,x*z*2,x*y*2])&&SamePolys(force,direct)
 &&Enumerable.Range(0,3).All(i=>Enumerable.Range(0,3).All(j=>force[i].Derivative(j).Same(force[j].Derivative(i))));

var positiveFourier=FourierAudit(false);var variableFourier=FourierAudit(true);
bool centerPositivePassed=positiveFourier.Passed;
bool closednessDecoyPassed=variableFourier.Passed;
var repeated=new Rational[144];repeated[0]=1;
Rational principalDecoy=Maximum(Product(principalMatrix,repeated));
var badB=Zeros(4,4);for(int i=0;i<4;i++)badB[i][i]=1;var badL=Lift(badB);
Rational badPrincipal=Maximum(Product(principalMatrix,badL)),badReduced=Maximum(Product(reducedMatrix,BVector(badB))),badCubic=Maximum(Product(cubicMatrix,badL));
bool negativePassed=principalDecoy==fx.GetProperty("negativeControls").GetProperty("principalMaximumDefect").GetInt32()
 &&badPrincipal==0&&badReduced==fx.GetProperty("negativeControls").GetProperty("reducedMaximumDefect").GetInt32()
 &&badCubic==fx.GetProperty("negativeControls").GetProperty("cubicMaximumDefect").GetInt32();
bool positivesPassed=nonabelianPassed&&centerPositivePassed&&closednessDecoyPassed&&negativePassed;
if(!positivesPassed){Emit(precedence[4],new{knownAnswerPassed,controlsPassed=false,nonabelianPassed,centerPositivePassed,closednessDecoyPassed,negativePassed,
 action=action.Terms(),force=force.Select(p=>p.Terms()).ToArray(),positiveFourier=positiveFourier.Evidence,variableFourier=variableFourier.Evidence,
 principalDecoy=principalDecoy.ToString(),badPrincipal=badPrincipal.ToString(),badReduced=badReduced.ToString(),badCubic=badCubic.ToString()});return;}

// This subgroup test controls only the spatial factor of a separately proved conditional14D statement.
var flips=new List<int>();for(int start=0;start<=7;start+=7)for(int i=start;i<start+7;i++)for(int j=i+1;j<start+7;j++)flips.Add((1<<i)|(1<<j));
int[] spatialMasks=Enumerable.Range(0,1<<14).Where(m=>Algebra.Degree(m)==3).ToArray();
int characterChecks=0,survivors=0;var spatialRows=new List<object>();
foreach(int mask in spatialMasks)
{
 bool invariant=true;int firstNegative=-1;for(int j=0;j<flips.Count;j++)
 {characterChecks++;bool plus=(Algebra.Degree(mask&flips[j])&1)==0;invariant&=plus;if(!plus&&firstNegative<0)firstNegative=j;}
 if(invariant)survivors++;spatialRows.Add(new{mask,firstNegative,invariant});
}
int subgroupControlChecks=0;bool subgroupControlsPassed=true;int[] subgroupMasks=fx.GetProperty("subgroupControlMasks").EnumerateArray().Select(p=>p.GetInt32()).ToArray();
foreach(int mask in subgroupMasks)foreach(int flip in flips){subgroupControlChecks++;subgroupControlsPassed&=(Algebra.Degree(mask&flip)&1)==0;}
bool spinPassed=spatialMasks.Length==N("spatialThreeMasks")&&flips.Count==N("sameBlockFlips")&&characterChecks==N("spatialCharacterChecks")
 &&survivors==N("spatialThreeSurvivors")&&subgroupMasks.Length==N("subgroupControlMasks")&&subgroupControlChecks==N("subgroupControlCharacterChecks")&&subgroupControlsPassed;
bool controlsPassed=knownAnswerPassed&&principalPassed&&qPassed&&reducedPassed&&combinedPassed&&positivesPassed&&spinPassed;
Emit(controlsPassed?Success:precedence[5],new{
 knownAnswerPassed,controlsPassed,
 matrixControls=new{gramPassed,realFormPassed,bracketsPassed,jacobiPassed,fourierKnown,gramEntries,realFormChecks,orderedBrackets,jacobiTriples,bracketRows},
 principalControls=new{principalPassed,principal=RankEvidence(principalMatrix,principalSolve),predictedTensorForm="epsilon_ijk times symmetric internal bilinear",symmetricBilinearDimension=10},
 curvatureControls=new{qPassed,q=RankEvidence(qmap,qSolve),monomials=monomials.Select(m=>new[]{m.A,m.B}).ToArray(),qMap=Text(qmap)},
 cubicControls=new{reducedPassed,combinedPassed,reduced=RankEvidence(reducedMatrix,reducedSolve),independentEightEquationRref=Text(oracleSolve.Rows),
  cubicRows=cubic.Count,cubicMatrixSha256=Digest(cubicMatrix),combined=RankEvidence(combined,combinedSolve),
  survivingBilinears=new[]{Text(center),Text(simple)},survivingOperators=Text(survivorOperators),raisedOperatorRequiresInverseGram=true},
 positiveControls=new{nonabelianPassed,centerPositivePassed,nonabelianAction=action.Terms(),nonabelianForceCovector=force.Select(p=>p.Terms()).ToArray(),positiveFourier=positiveFourier.Evidence},
 negativeControls=new{negativePassed,principalDecoy=principalDecoy.ToString(),badPrincipal=badPrincipal.ToString(),badReduced=badReduced.ToString(),badCubic=badCubic.ToString(),
  closednessDecoyPassed,variableFourier=variableFourier.Evidence,pointwiseConditionsAloneSufficient=false},
 conditionalSpinControls=new{spinPassed,spatialThreeMaskCount=spatialMasks.Length,flipCount=flips.Count,characterChecks,survivors,spatialRows,
  subgroupControlChecks,subgroupControlsPassed,subgroupControlMasks=subgroupMasks,degreeSevenControlsClaimedFullSpinInvariant=false,
  fullU128CoefficientCensusPerformed=false,u128BilinearClassificationIsWrittenProof=true,sourceOperatorSelected=false,frameCovarianceAloneTreatedAsInvariant=false},
 nextRequirement="Use the actual action-derived adjoint/cyclic Euler operator or state additional structures/variation restrictions explicitly; the conditional fixed-operator classification does not choose the missing source operator."
});

int N(string key)=>counts.GetProperty(key).GetInt32();
static int PairIndex(int a,int b)=>a==0?(b==1?0:1):2;
static int Epsilon(int a,int b,int c)=>a==b||a==c||b==c?0:((a>b?1:0)+(a>c?1:0)+(b>c?1:0))%2==0?1:-1;
static void AddSymbol(Rational[] row,int output,int derivative,int input,int scale)
{
 int axis=input/4,gauge=input%4;if(axis==derivative)return;
 int sign=derivative<axis?1:-1,pair=PairIndex(System.Math.Min(derivative,axis),System.Math.Max(derivative,axis));
 row[12*output+4*pair+gauge]+=scale*sign;
}
Rational[] Lift(Rational[][] b)
{
 var v=new Rational[144];for(int i=0;i<3;i++)for(int a=0;a<4;a++)for(int p=0;p<3;p++)for(int c=0;c<4;c++)
  v[12*(4*i+a)+4*p+c]=Epsilon(i,pairs[p].A,pairs[p].B)*b[a][c];return v;
}
static Rational[][] ExtractB(Rational[] v)=>Enumerable.Range(0,4).Select(a=>Enumerable.Range(0,4).Select(b=>v[12*a+8+b]).ToArray()).ToArray();
static bool Symmetric(Rational[][] b)=>Enumerable.Range(0,b.Length).All(i=>Enumerable.Range(0,b.Length).All(j=>b[i][j]==b[j][i]));
static bool SameMatrix(Rational[][] a,Rational[][] b)=>a.Length==b.Length&&a.Zip(b,(x,y)=>x.SequenceEqual(y)).All(x=>x);
Rational[] BVector(Rational[][] b)=>symmetric.Select(s=>b[s.A][s.B]).ToArray();
static bool SamePolys(Polynomial[] a,Polynomial[] b)=>a.Length==b.Length&&a.Zip(b,(p,q)=>p.Same(q)).All(x=>x);
static object RankEvidence(Rational[][] a,Echelon s)=>new{rows=a.Length,columns=a[0].Length,s.Rank,nullity=s.Kernel.Length,pivots=s.Pivots,
 matrixSha256=Digest(a),rrefSha256=Digest(s.Rows),kernel=Text(s.Kernel),kernelRrefConsistencyPassed=KernelRrefConsistency(a,s),
 modularPrime=Prime,independentModularRankLowerBound=ModularRank(a),modularRankMatchesRationalRank=ModularRank(a)==s.Rank};
(bool Passed,object Evidence) FourierAudit(bool variable)
{
 var p=Polynomial.Variable(0);var q=Polynomial.Variable(1);
 var lambda=variable?cos:unit;var mode1=variable?sin:cos;var mode2=variable?unit:sin;
 var a1=mode1.Amplitude(p);var a2=mode2.Amplitude(q);
 var actionF=(lambda*(a2*a1.Derivative()+(a1*a2.Derivative()).Scale(-1))).Scale(new Scalar(new Rational(1,2),0));
 var potential=actionF.Average();Polynomial[] actual=[potential.Derivative(0),potential.Derivative(1)];
 var desired1=(lambda*a2.Derivative()).Scale(-1);var desired2=lambda*a1.Derivative();
 Polynomial[] desired=[(mode1*desired1).Average(),(mode2*desired2).Average()];
 var adjoint1=(lambda*a2.Derivative()+(lambda*a2).Derivative()).Scale(new Scalar(new Rational(-1,2),0));
 var adjoint2=(lambda*a1.Derivative()+(lambda*a1).Derivative()).Scale(new Scalar(new Rational(1,2),0));
 Polynomial[] viaAdjoint=[(mode1*adjoint1).Average(),(mode2*adjoint2).Average()];
 var expectedPotential=p*q*new Scalar(variable?new Rational(1,4):new Rational(-1,2),0);
 Polynomial[] expectedActual=variable?[q*new Scalar(new Rational(1,4),0),p*new Scalar(new Rational(1,4),0)]
  :[q*new Scalar(new Rational(-1,2),0),p*new Scalar(new Rational(-1,2),0)];
 Polynomial[] expectedDesired=variable?[Polynomial.Constant(0),p*new Scalar(new Rational(1,2),0)]:expectedActual;
 bool hessianSymmetric=actual[0].Derivative(1).Same(actual[1].Derivative(0));
 bool desiredSymmetric=desired[0].Derivative(1).Same(desired[1].Derivative(0));
 bool passed=potential.Same(expectedPotential)&&SamePolys(actual,expectedActual)&&SamePolys(actual,viaAdjoint)&&SamePolys(desired,expectedDesired)
 &&hessianSymmetric&&desiredSymmetric==!variable&&lambda.Derivative().IsZero==!variable;
 return(passed,new{variableCoefficient=variable,passed,action=potential.Terms(),actualDerivative=actual.Select(x=>x.Terms()).ToArray(),
  adjointDerivative=viaAdjoint.Select(x=>x.Terms()).ToArray(),desiredForce=desired.Select(x=>x.Terms()).ToArray(),hessianSymmetric,desiredSymmetric,closed=lambda.Derivative().IsZero});
}
void Emit(string verdict,object evidence)
{
 var result=new{schemaVersion=1,phase=596,phaseId="phase596-fixed-operator-helmholtz-audit",contractId=ContractId,
 contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,
 bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,
 verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
 authorityFirewalls=firewalls.ToDictionary(x=>x,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/fixed_operator_helmholtz_audit.json",json);
 File.WriteAllText(Root+"/output/fixed_operator_helmholtz_audit_summary.json",json);
 Console.WriteLine($"Phase596 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
