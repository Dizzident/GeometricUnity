using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static Adjoint;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase602_stationary_odd_background_closure_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P601="studies/phase601_full_hessian_cyclic_closure_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase602-a55-stationary-odd-background-closure-v1";
const string Success="stationary-odd-background-full-closure-controls-pass-no-physical-spectrum";
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "background":"fixed flat metric/reference, epsilon=I, constant fields, normalized density, kappa=0",
 "chiralities":[-1,1],"formalCSlots":["1","c"],"unitGamma":1,"unitLambda":1,
 "phi1":"P Gamma1, P=1+h Omega","phi2":"(c-i h Omega)Gamma2",
 "operator":"literal full CCA, both Hodge legs and printed factor1/2",
 "pairing":"real bilinear -ReTr/128 times signed exterior metric; no positive norm selected",
 "S0":"lambda theta0 gamma0","Q0":0,
 "actualGradient":"gamma*(KQ+DQ_S^dagger K^dagger S)/3",
 "DQ":"FULL derivative S wedge V+V wedge S; no extra factor2 in its adjoint",
 "stationaryAdjointSlots":["-2 sum_(a<b,a,b!=0)theta_ab Gamma_ab","-2 i h sum_(a<b)theta_ab Omega Gamma_ab"],
 "stationaryAdjointTermCounts":[78,91],
 "nonstationaryControl":"U=lambda theta0 gamma2; Q(U)=0 but G(U)=-(4 gamma lambda^2/3)sum_(j!=0,2)theta_j gamma_j",
 "nonstationaryUnitVariation":"theta1 gamma1; direct cubic-action derivative 4/3",
 "carrierOrder":"V1..V13,W1..W13; Vi=theta_i gamma_i,Wi=theta_i Omega gamma_i",
 "gram":"diag(-I13,+I13)",
 "hessian":"gamma/3*(K DQ_S V+DQ_V^dagger K^dagger S+DQ_S^dagger K^dagger V)",
 "unitUnweightedLegs":{"Vi":["4 sum_(j!=0,i)(Vj+h Wj)","4 sum_(j!=0,i)Vj","4 sum_(j!=0,i)Vj"],"Wi":["0","4 sum_(j!=0,i)Wj","-4h sum_(j!=0,i)Vj"],"cSlots":"each of all three legs identically zero"},
 "fullColumnTermsBySlot":[24,0],
 "actionPolarization":"coefficient st of gamma/3 B(S0+sA+tB,K((S0+sA+tB) wedge (S0+sA+tB))); six ordered forward-K words grouped in exchanged pairs before real pairing, no adjoints",
 "closedMatrix":"(4 gamma lambda/3)(J13-I13) tensor [[3,-h],[h,1]], displayed carrier is V-major",
 "indexBasis":"ones and Helmert za=(1 repeated a,-a,0 repeated 12-a), a=1..12; mutually orthogonal",
 "internalJordanBasis":"e=(1,h),g=(1,0); (M-2I)g=e,(M-2I)e=0",
 "unitJordanBlocks":"(4 mu/3)*[[2,1],[0,2]], mu=12 once and -1 twelve times",
 "unitCharacteristic":"(x-32)^2*(x+8/3)^24","unitMinimalPolynomial":"(x-32)^2*(x+8/3)^2",
 "unitRankOrder":["H","H-32I","(H-32I)^2","H+8I/3","(H+8I/3)^2","minimalPolynomial","lowerUniformExponent","lowerTracelessExponent"],
 "unitRanks":[26,25,24,14,2,0,1,12],
 "loweredHessianInertia":"13 positive and13 negative for gamma*lambda nonzero; stationary constant-action saddle, not a dynamics conclusion",
 "couplingRows":[{"lambda":-1,"gamma":1},{"lambda":1,"gamma":1},{"lambda":1,"gamma":2},{"lambda":0,"gamma":1},{"lambda":1,"gamma":0},{"lambda":0,"gamma":0}],
 "couplingInterpretation":"frozen known answers only; symbolic homogeneity proved independently, no interpolation or selected vacuum scale",
 "zeroCoupling":"H=0, rank0, characteristic x^26, minimal polynomial x",
 "negativeControls":["Q0 does not imply stationary","omitting either DQ-adjoint Hessian contribution fails","halving full DQ-adjoint fails","discarding W outputs fails full closure","Jordan square-zero is not zero Jordan block"],
 "expectedCounts":{"wordCases":256,"hodgeCases":16384,"gramEntries":676,"contexts":4,"stationaryAdjointChecks":4,"stationarityChecks":28,"nonstationaryChecks":4,"nonstationaryVariations":4,"fullColumns":104,"legChecks":312,"matrixEntries":2704,"actionBilinears":2704,"adjointComparisons":112,"jordanMatrixChecks":2,"rankCertificates":16,"chiralEigenvectors":26,"inertiaBlocks":26,"omittedLegRejections":130,"halfDQRejections":52,"discardedWRejections":26,"scalingRows":12,"scalingColumns":624},
 "exactTolerance":0,"coreFileCount":726,
 "resources":{"estimatedCpuSeconds":30,"maximumEstimatedCpuSeconds":180,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":536870912,"maximumTensorTerms":65536,"maximumCoefficientProducts":100000000}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","stationary-background-control-failed","full-hessian-closure-control-failed","action-polarization-control-failed","jordan-scaling-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase602StationaryOddBackgroundClosureAudit.csproj",["study"]=Root+"/STUDY.md",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",["matrix-helper"]=P601+"/CyclicPolynomial.cs",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["phase601-summary"]=P601+"/output/full_hessian_cyclic_closure_audit_summary.json",["phase601-contract"]=P601+"/preregistration/contract_v1.json",["phase601-program"]=P601+"/Program.cs",
 ["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==602&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase601","full-hessian-cyclic-closure-controls-pass-compression-not-spectrum")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}


var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");
var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);
bool knownAnswerPassed=true,stationaryPassed=true,closurePassed=true,polarizationPassed=true,jordanPassed=true;
int[] masks=Enumerable.Range(0,8).Concat(Enumerable.Range(0,8).Select(q=>Full^q)).ToArray();
foreach(int a in masks)foreach(int b in masks){counts["wordCases"]++;knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);}
for(int m=0;m<=Full;m++){counts["hodgeCases"]++;int p=Degree(m);knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((p*(14-p)+7)%2==0?1:-1);}
FT[] basis=Enumerable.Range(1,13).Select(i=>One(1<<i,1<<i,1)).Concat(Enumerable.Range(1,13).Select(i=>Omega(One(1<<i,1<<i,1)))).ToArray();
Rational[][] gram=Cyclic.Gram(basis);for(int i=0;i<26;i++)for(int j=0;j<26;j++){counts["gramEntries"]++;knownAnswerPassed&=gram[i][j]==(i==j?(i<13?-1:1):0);}
knownAnswerPassed&=basis.All(t=>HAnti(t)&&D(t).Count==0)&&Cyclic.Rank(gram)==26;
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
FT gamma1=new(),gamma2=new();for(int i=0;i<14;i++)gamma1=Add(gamma1,One(1<<i,1<<i,1));for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)gamma2=Add(gamma2,One((1<<a)|(1<<b),(1<<a)|(1<<b),1));
FT s0=One(1,1,1),unaligned=One(1,4,1),variation=One(2,2,1);
var contexts=new List<object>();var jordanRows=new List<object>();var scalingRows=new List<object>();
foreach(int h in fx.GetProperty("chiralities").EnumerateArray().Select(q=>q.GetInt32()))
{
 FT p1=Add(gamma1,Scale(Omega(gamma1),h));FT[] p2=[Scale(Omega(gamma2),Scalar.I*-h),gamma2];
 FT[][] images=new FT[2][],adjoints=new FT[2][];FT[] ys=new FT[2];Rational[][][] matrices=new Rational[2][][];
 for(int slot=0;slot<2;slot++)
 {
  counts["contexts"]++;bool first=slot==0;FT y=KAdjointLiteral(s0,p1,p2[slot],first);ys[slot]=y;
  counts["adjointComparisons"]++;bool adjoint=Equal(y,KAdjointSimplified(s0,p1,p2[slot],first));
  FT oracleY=ExpectedStationaryAdjoint(h,slot);counts["stationaryAdjointChecks"]++;
  stationaryPassed&=adjoint&&Equal(y,oracleY)&&y.Count==fx.GetProperty("stationaryAdjointTermCounts")[slot].GetInt32()&&HAnti(y);
  FT gradient=ConstantGradient(s0,p1,p2[slot],first,1);counts["stationarityChecks"]++;
  stationaryPassed&=Product(s0,s0).Count==0&&gradient.Count==0;
  FT uy=KAdjointLiteral(unaligned,p1,p2[slot],first);counts["adjointComparisons"]++;
  stationaryPassed&=Equal(uy,KAdjointSimplified(unaligned,p1,p2[slot],first));
  FT ug=ConstantGradient(unaligned,p1,p2[slot],first,1),uo=new();if(first)for(int i=1;i<14;i++)if(i!=2)uo=Add(uo,Scale(One(1<<i,1<<i,1),new Scalar(new Rational(-4,3),0)));
  counts["nonstationaryChecks"]++;counts["nonstationaryVariations"]++;
  Rational uv=ActionDerivative(unaligned,variation,p1,p2[slot],first,1);
  stationaryPassed&=Product(unaligned,unaligned).Count==0&&Equal(ug,uo)&&Pair(variation,ug)==uv&&uv==(first?new Rational(4,3):0)&&(!first||ug.Count==12);
  images[slot]=new FT[26];adjoints[slot]=new FT[26];matrices[slot]=Cyclic.Zeros(26);var columns=new List<object>();
  for(int column=0;column<26;column++)
  {
   FT v=basis[column],yv=KAdjointLiteral(v,p1,p2[slot],first);adjoints[slot][column]=yv;counts["adjointComparisons"]++;
   closurePassed&=Equal(yv,KAdjointSimplified(v,p1,p2[slot],first));
   FT[] legs=HessianLegs(s0,v,y,yv,p1,p2[slot],first),oracles=ExpectedLegs(column,h,slot,basis);
   for(int leg=0;leg<3;leg++){counts["legChecks"]++;closurePassed&=Equal(legs[leg],oracles[leg])&&HAnti(legs[leg])&&Typed(legs[leg],1);}
   FT image=Scale(Sum(legs),new Scalar(new Rational(1,3),0));images[slot][column]=image;counts["fullColumns"]++;
   closurePassed&=image.Count==fx.GetProperty("fullColumnTermsBySlot")[slot].GetInt32();
   FT reconstruction=new();for(int row=0;row<26;row++)
   {
    Rational coefficient=Pair(basis[row],image)*(row<13?-1:1);matrices[slot][row][column]=coefficient;
    reconstruction=Add(reconstruction,Scale(basis[row],new Scalar(coefficient,0)));counts["matrixEntries"]++;
    closurePassed&=coefficient==ExpectedEntry(row,column,h,slot);
   }
   closurePassed&=Equal(image,reconstruction);
   if(first)
   {
    for(int leg=0;leg<3;leg++)if(oracles[leg].Count>0){counts["omittedLegRejections"]++;closurePassed&=!Equal(Sum(legs),Add(Sum(legs),Scale(legs[leg],-1)));}
    counts["halfDQRejections"]++;closurePassed&=!Equal(Sum(legs),Add(legs[0],Scale(Add(legs[1],legs[2]),new Scalar(new Rational(1,2),0))));
    if(column<13){counts["discardedWRejections"]++;FT projected=new();for(int row=0;row<13;row++)projected=Add(projected,Scale(basis[row],new Scalar(matrices[slot][row][column],0)));closurePassed&=!Equal(projected,image);}
   }
   columns.Add(new{column,passed=Equal(image,reconstruction),legs=legs.Select(Terms).ToArray(),fullImage=Terms(image)});
  }
  var actionMatrix=Cyclic.Zeros(26);
  for(int a=0;a<26;a++)for(int b=0;b<26;b++)
  {
   // All six ordered cubic words: no adjoint, Hessian or projected matrix enters this oracle.
   Rational direct=ActionPolarization(s0,basis[a],basis[b],p1,p2[slot],first,1);
   actionMatrix[a][b]=direct;counts["actionBilinears"]++;
   polarizationPassed&=direct==Pair(basis[a],images[slot][b])&&direct==gram[a][a]*ExpectedEntry(a,b,h,slot);
  }
  polarizationPassed&=Cyclic.SameMatrix(actionMatrix,Cyclic.Transpose(actionMatrix))&&Cyclic.SameMatrix(actionMatrix,Cyclic.Multiply(gram,matrices[slot]));
  contexts.Add(new{h,slot,stationaryAdjoint=Terms(y),stationaryGradient=Terms(gradient),nonstationaryGradient=Terms(ug),nonstationaryVariation=uv.ToString(),columns,matrix=Cyclic.Text(matrices[slot]),actionMatrix=Cyclic.Text(actionMatrix)});
 }
 Rational[][] matrix=matrices[0],similarity=JordanBasis(h),inverse=Cyclic.Inverse(similarity),actualJordan=Cyclic.Multiply(Cyclic.Multiply(inverse,matrix),similarity),expectedJordan=JordanOracle();
 counts["jordanMatrixChecks"]++;jordanPassed&=Cyclic.Rank(similarity)==26&&Cyclic.SameMatrix(actualJordan,expectedJordan)&&Cyclic.SameMatrix(Cyclic.Multiply(inverse,similarity),Cyclic.Identity(26));
 Rational[][] uniform=Shift(matrix,-32),traceless=Shift(matrix,new Rational(8,3)),uniform2=Cyclic.Multiply(uniform,uniform),traceless2=Cyclic.Multiply(traceless,traceless);
 Rational[][][] rankMatrices=[matrix,uniform,uniform2,traceless,traceless2,Cyclic.Multiply(uniform2,traceless2),Cyclic.Multiply(uniform,traceless2),Cyclic.Multiply(uniform2,traceless)];
 int[] ranks=rankMatrices.Select(Cyclic.Rank).ToArray();counts["rankCertificates"]+=ranks.Length;jordanPassed&=ranks.SequenceEqual(fx.GetProperty("unitRanks").EnumerateArray().Select(q=>q.GetInt32()));
 Rational[][] lowered=Cyclic.Multiply(Cyclic.Multiply(Cyclic.Transpose(similarity),Cyclic.Multiply(gram,matrix)),similarity);
 jordanPassed&=Cyclic.SameMatrix(lowered,Cyclic.Transpose(lowered));for(int i=0;i<26;i++)for(int j=0;j<26;j++)if(i/2!=j/2)jordanPassed&=lowered[i][j]==0;
 for(int sector=0;sector<13;sector++)
 {
  counts["inertiaBlocks"]++;int e=2*sector;Rational determinant=lowered[e][e]*lowered[e+1][e+1]-lowered[e][e+1]*lowered[e+1][e];jordanPassed&=determinant.Numerator.Sign<0;
  FT eigen=new(),sourceImage=new();for(int i=0;i<26;i++){eigen=Add(eigen,Scale(basis[i],new Scalar(similarity[i][e],0)));sourceImage=Add(sourceImage,Scale(images[0][i],new Scalar(similarity[i][e],0)));}
  counts["chiralEigenvectors"]++;Rational eigenvalue=sector==0?32:new Rational(-8,3);
  jordanPassed&=eigen.Count>0&&Equal(Omega(eigen),Scale(eigen,h))&&Equal(sourceImage,Scale(eigen,new Scalar(eigenvalue,0)))&&Pair(eigen,eigen)==0;
 }
 jordanRows.Add(new{h,passed=Cyclic.SameMatrix(actualJordan,expectedJordan),similarity=Cyclic.Text(similarity),jordan=Cyclic.Text(actualJordan),loweredCongruence=Cyclic.Text(lowered),ranks,characteristic="(x-32)^2*(x+8/3)^24",minimalPolynomial="(x-32)^2*(x+8/3)^2",inertia=new{positive=13,negative=13,zero=0}});
 foreach(var row in fx.GetProperty("couplingRows").EnumerateArray())
 {
  int lambda=row.GetProperty("lambda").GetInt32(),encoding=row.GetProperty("gamma").GetInt32();Rational factor=lambda*encoding;FT background=Scale(s0,lambda);counts["scalingRows"]++;bool ok=true;
  for(int slot=0;slot<2;slot++)
  {
   counts["stationarityChecks"]++;ok&=ConstantGradient(background,p1,p2[slot],slot==0,encoding).Count==0;
   FT y=KAdjointLiteral(background,p1,p2[slot],slot==0);ok&=Equal(y,Scale(ys[slot],lambda));
   for(int column=0;column<26;column++)
   {counts["scalingColumns"]++;FT image=Scale(Sum(HessianLegs(background,basis[column],y,adjoints[slot][column],p1,p2[slot],slot==0)),new Scalar(new Rational(encoding,3),0));ok&=Equal(image,Scale(images[slot][column],new Scalar(factor,0)));}
  }
  Rational[][] scaled=matrix.Select(r=>r.Select(q=>q*factor).ToArray()).ToArray();int rank=Cyclic.Rank(scaled);ok&=rank==(factor==0?0:26);
  jordanPassed&=ok;scalingRows.Add(new{h,lambda,gamma=encoding,passed=ok,rank,uniformEigenvalue=(factor*32).ToString(),tracelessEigenvalue=(factor*new Rational(-8,3)).ToString(),minimalPolynomial=factor==0?"x":"(x-32*gamma*lambda)^2*(x+8*gamma*lambda/3)^2",physicalParameterSelection=false});
 }
}
bool countsPassed=counts.Count==expected.EnumerateObject().Count()&&counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());
bool resourcesPassed=LargestTensor<=fx.GetProperty("resources").GetProperty("maximumTensorTerms").GetInt32()&&CoefficientProducts<=fx.GetProperty("resources").GetProperty("maximumCoefficientProducts").GetInt64();
bool controlsPassed=stationaryPassed&&closurePassed&&polarizationPassed&&jordanPassed&&countsPassed&&resourcesPassed;
string verdict=!stationaryPassed?precedence[2]:!closurePassed?precedence[3]:!polarizationPassed?precedence[4]:!jordanPassed||!countsPassed||!resourcesPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,stationaryPassed,closurePassed,polarizationPassed,jordanPassed,countsPassed,resourcesPassed,counts,coefficientProducts=CoefficientProducts,largestTensorDuringAssembly=LargestTensor,gram=Cyclic.Text(gram),contexts,jordanRows,scalingRows});

static FT Sum(IEnumerable<FT> values)=>values.Aggregate(new FT(),Add);
static FT DQ(FT s,FT v)=>Add(Product(s,v),Product(v,s));
static FT ConstantGradient(FT s,FT p1,FT p2,bool first,int encoding)=>Scale(Add(Chain(Product(s,s),p1,p2,first),DQAdjoint(s,KAdjointLiteral(s,p1,p2,first))),new Scalar(new Rational(encoding,3),0));
static FT[] HessianLegs(FT s,FT v,FT ys,FT yv,FT p1,FT p2,bool first)=>[Chain(DQ(s,v),p1,p2,first),DQAdjoint(v,ys),DQAdjoint(s,yv)];
static Rational ActionDerivative(FT s,FT v,FT p1,FT p2,bool first,int encoding)=>(Pair(v,Chain(Product(s,s),p1,p2,first))+Pair(s,Chain(DQ(s,v),p1,p2,first)))*new Rational(encoding,3);
static Rational ActionPolarization(FT s,FT a,FT b,FT p1,FT p2,bool first,int encoding)
{FT[] fields=[s,a,b];Rational result=0;foreach(var order in new[]{new[]{0,1,2},new[]{1,0,2},new[]{2,0,1}}){FT left=Chain(Product(fields[order[1]],fields[order[2]]),p1,p2,first),right=Chain(Product(fields[order[2]],fields[order[1]]),p1,p2,first);result+=Pair(fields[order[0]],Add(left,right));}return result*new Rational(encoding,3);}
static FT ExpectedStationaryAdjoint(int h,int slot)
{FT result=new();for(int a=0;a<14;a++)for(int b=a+1;b<14;b++){int form=(1<<a)|(1<<b);if(slot==0&&a!=0)result=Add(result,One(form,form,-2));if(slot==1)result=Add(result,Scale(Omega(One(form,form,1)),Scalar.I*(-2*h)));}return result;}
static FT[] ExpectedLegs(int column,int h,int slot,FT[] basis)
{if(slot==1)return [new(),new(),new()];FT v=new(),w=new();for(int j=0;j<13;j++)if(j!=column%13){v=Add(v,basis[j]);w=Add(w,basis[j+13]);}return column<13?[Scale(Add(v,Scale(w,h)),4),Scale(v,4),Scale(v,4)]:[new(),Scale(w,4),Scale(v,-4*h)];}
static Rational ExpectedEntry(int row,int column,int h,int slot)
{if(slot==1||row%13==column%13)return 0;int m=row<13?(column<13?3:-h):(column<13?h:1);return new Rational(4*m,3);}
static Rational[][] JordanBasis(int h)
{var t=Cyclic.Zeros(26);for(int sector=0;sector<13;sector++)for(int i=0;i<13;i++){int z=sector==0?1:i<sector?1:i==sector?-sector:0;t[i][2*sector]=z;t[i+13][2*sector]=h*z;t[i][2*sector+1]=z;}return t;}
static Rational[][] JordanOracle()
{var j=Cyclic.Zeros(26);for(int sector=0;sector<13;sector++){Rational q=new Rational(4*(sector==0?12:-1),3);j[2*sector][2*sector]=q*2;j[2*sector][2*sector+1]=q;j[2*sector+1][2*sector+1]=q*2;}return j;}
static Rational[][] Shift(Rational[][] a,Rational shift)=>a.Select((row,i)=>row.Select((v,j)=>v+(i==j?shift:0)).ToArray()).ToArray();

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=602,phaseId="phase602-stationary-odd-background-closure-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/stationary_odd_background_closure_audit.json",json);File.WriteAllText(Root+"/output/stationary_odd_background_closure_audit_summary.json",json);Console.WriteLine($"Phase602 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
