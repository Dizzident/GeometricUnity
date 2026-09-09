using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static Covariant;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase612_covariant_caa_symbol_null_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P611="studies/phase611_untied_caa_response_joint_gauge_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase612-a59-covariant-caa-symbol-null-v1";
const string Success="covariant-caa-symbol-controls-pass-carrier-not-full-spectrum";
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "operator":"canonical untied CAA: first C, outer A, inner A; Phi1=Gamma1,Phi2=Gamma2; no source choice",
 "pairing":"real -ReTr(XY)/128 times signed exterior metric, normalized periodic average",
 "momentumPairs":[[2,1],[1,2],[1,1],[1,-1]],"n":[1,2],"vAxis":2,"vSquared":1,"kappa":0,
 "momentum":"q=n(a theta0+b theta7),Q=n(a gamma0-b gamma7),qSquared=n^2(a^2-b^2)",
 "fourierConvention":"K0 stores abstract unit phase phi=q.x; all physical n,a,b are in q; Dq=q wedge partial_phase,Dqdag=-i_qsharp partial_phase",
 "inputOrder":["U=qV cos","A=(V wedgeCl Gamma1)sin","qC=q(V wedgeCl Q)sin","E=(qSquared A-qC)sin","Y=qQ cos"],
 "leakage":"Z=theta2 Q cos,retained before cancellation",
 "adjointFirst":["2q wedge A cos","2theta2 wedge Gamma1 sin","2theta2 wedge qQ sin","2theta2 wedge(qSquared Gamma1-qQ)sin","2q wedge(Q wedgeCl Gamma1)cos"],
 "adjointSecond":["0","0","0","0","-2qSquared Gamma2 cos"],
 "divergenceFirst":["2E","2Z","2qSquared Z","0","2qSquared(Q wedgeCl Gamma1)sin"],
 "divergenceSecond":["0","0","0","0","-2qSquared(Q wedgeCl Gamma1)sin"],
 "forwardKd":["0","-24U","0","-24qSquared U","0"],
 "hessianColumns":["E","-12U+Z","qSquared Z","-12qSquared U","0"],
 "carrierMatrix":[["0","-12qSquared"],["1","0"]],"carrierSquare":"-12qSquared I","carrierRankNonNull":2,"carrierRankNull":1,
 "carrierGram":[["-qSquared/2","0"],["0","6qSquared^2"]],"originalAction":"6qSquared^2 xy","actionHessianOffDiagonal":"6qSquared^2",
 "nullControl":"q nonzero,qSquared0,U andE nonzero,HU=E,HE0; restricted Gram and action zero, not full response zero",
 "nullProbes":["theta0 gamma2 cos pairs U=-n/2","theta0 Gamma02 sin pairs E=n^2/2"],
 "longitudinal":"HY=0 allq; Pair(Y,Y)=-qSquared^2/2,nonnull first/second divergence nonzero and cancel",
 "supports":{"inputsNonNull":[2,13,4,15,4,2],"inputsNull":[2,13,4,4,4,2],"adjointsNonNull":[24,13,4,15,114],"adjointsNull":[24,13,4,4,48],"YFirstNonNull":49,"YFirstNull":48,"YSecondNonNull":91,"YSecondNull":0,"YNonNullDivergencePositions":26},
 "supportCandidates":{"U":["m XOR bit0 XOR bit2","m XOR bit7 XOR bit2"],"A":["m XOR bit2"],"qCAndE":["m XOR bit2","m XOR bit2 XOR bit0 XOR bit7"],"Y":["m","m XOR bit0 XOR bit7"]},
 "independentAdjointReconstruction":"all91 twoforms times fixedcandidate masks, real Hanti phase1/ioracle, same sine/cosine, NaiveProduct ordered-word full forward chain; entire tensor reconstructed",
 "derivativeControls":["X=i theta2 cos,Y=i theta0 wedge theta2 sin,pair=-na/2","X=i theta2 cos,Y=i theta7 wedge theta2 sin,pair=nb/2"],
 "negativeControls":["positive metric contraction substituted on axis7 rejects full HU in all8rows","deleting second Y adjoint rejects HY in all4nonnullrows","zero null Gram/action does not erase nonzero full E; fixed probes detect it"],
 "expectedCounts":{"hodgeCases":16384,"wordCases":44944,"generatorWordCases":458752,"rows":8,"inputCoefficientChecks":48,"derivativePairings":16,"derivativeSquareZeros":48,"adjointInputs":40,"adjointLegs":80,"adjointDivergenceLegs":80,"adjointSupportProbes":6552,"adjointNonzeroProbes":1052,"hessianColumns":40,"fullSecondApplications":16,"leakageRows":8,"gramEntries":32,"actionPolarizations":32,"actionCoefficients":24,"carrierMatrixEntries":32,"nullRows":4,"nonNullRows":4,"nullProbePairs":8,"longitudinalRows":8,"longitudinalNonzeroDivergenceRows":4,"wrongMetricRows":8,"wrongDeletedAdjointRows":4},
 "exactTolerance":0,"coreFileCount":726,
 "resources":{"estimatedCpuSeconds":20,"maximumEstimatedCpuSeconds":120,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":536870912,"maximumCoefficientProducts":100000000,"maximumSparseTensorTerms":65536,"maximumStoredFrequency":1,"maximumCarrierDimension":2,"maximumActionDegree":2},
 "scope":{"fullSymbolRankComputed":false,"fullDeterminantComputed":false,"physicalCharacteristicConeSelected":false,"physicalTimeSelected":false,"globalGaugeQuotientEstablished":false,"sourceOperatorSelected":false,"nullProbeExtendedCarrierClosureClaimed":false}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","full-adjoint-control-failed","full-column-control-failed","action-gram-control-failed","null-response-control-failed","longitudinal-control-failed","count-or-resource-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase612CovariantCaaSymbolNullAudit.csproj",["study"]=Root+"/STUDY.md",["control-helper"]=Root+"/CovariantControls.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["caa-helper"]=P611+"/CaaOperator.cs",["phase611-program"]=P611+"/Program.cs",["phase611-study"]=P611+"/STUDY.md",["phase611-summary"]=P611+"/output/untied_caa_response_joint_gauge_audit_summary.json",["phase611-contract"]=P611+"/preregistration/contract_v1.json",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==612&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase611","untied-caa-response-controls-pass-source-choice-open")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);
bool knownAnswerPassed=true,adjointPassed=true,columnsPassed=true,actionGramPassed=true,nullPassed=true,longitudinalPassed=true;int maximumFrequency=0;var rows=new List<object>();
for(int m=0;m<=Full;m++){int d=Degree(m);counts["hodgeCases"]++;knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((d*(14-d)+7)%2==0?1:-1);for(int axis=0;axis<14;axis++){int bit=1<<axis;counts["generatorWordCases"]+=2;knownAnswerPassed&=BladeSign(bit,m)==WordSign(bit,m)&&BladeSign(m,bit)==WordSign(m,bit);}}
int[] wordMasks=Enumerable.Range(0,16384).Where(m=>new[]{0,1,2,12,13,14}.Contains(Degree(m))).ToArray();foreach(int a in wordMasks)foreach(int b in wordMasks){counts["wordCases"]++;knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);}
knownAnswerPassed&=Pair(Trig(0,false,0,0),Trig(0,false,0,0))==new Rational(-1,2)&&Pair(Trig(0,true,0,0),Trig(0,true,0,0))==new Rational(-1,2)&&Pair(Trig(0,false,0,0),Trig(0,true,0,0))==0;
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
foreach(var (a,b) in new[]{(2,1),(1,2),(1,1),(1,-1)})foreach(int n in new[]{1,2})
{
 counts["rows"]++;int s=n*n*(a*a-b*b);bool isNull=s==0;FT q=Covector(a,b,n),Q=RaisedClifford(a,b,n);FT[] coefficients=Coefficients(a,b,n);FT[] inputs=coefficients.Select((t,i)=>Phase(t,i is 1 or 2 or 3)).ToArray();FT U=inputs[0],A=inputs[1],qC=inputs[2],E=inputs[3],Y=inputs[4],Z=inputs[5];
 knownAnswerPassed&=Equal(coefficients[3],ExplicitE(a,b,n))&&Equal(NaiveProduct(Q,Q,'W'),One(0,0,s))&&Equal(Contract(q,q),One(0,0,s))&&Contract(q,One(4,0,1)).Count==0;
 int[] inputSupports=isNull?[2,13,4,4,4,2]:[2,13,4,15,4,2];for(int i=0;i<6;i++){counts["inputCoefficientChecks"]++;knownAnswerPassed&=FourierReal(inputs[i],1)&&Positions(inputs[i])==inputSupports[i]&&inputs[i].Count==2*inputSupports[i];counts["derivativeSquareZeros"]++;knownAnswerPassed&=Dq(q,Dq(q,inputs[i])).Count==0;}
 foreach(int axis in new[]{0,7}){var dx=Scale(Trig(0,false,4,0),Scalar.I);var dy=Phase(Wedge(One(1<<axis,0,1),One(4,0,Scalar.I)),true);Rational value=new Rational(axis==0?-n*a:n*b,2);counts["derivativePairings"]++;knownAnswerPassed&=Pair(Dq(q,dx),dy)==value&&Pair(dx,DqAdjoint(q,dy))==value&&value!=0;}
 FT[] firstCoefficients=ExpectedAdjoints(q,Q,coefficients,s);FT B=Phase(CliffordWedge(Q,Caa.Gamma1),true);FT[] forwardExpected=[new(),Scale(U,-24),new(),Scale(U,-24*s),new()];FT[] divFirstExpected=[Scale(E,2),Scale(Z,2),Scale(Z,2*s),new(),Scale(B,2*s)];FT[] hExpected=[E,Add(Scale(U,-12),Z),Scale(Z,s),Scale(U,-12*s),new()];var hActual=new FT[5];var actualAdjoints=new FT[5];var actualFirst=new FT[5];var actualSecond=new FT[5];var adjointEvidence=new List<object>();int[] adjointSupports=isNull?[24,13,4,4,48]:[24,13,4,15,114];
 for(int i=0;i<5;i++)
 {
  var legs=Caa.AdjointLegs(inputs[i]);actualFirst[i]=legs.First;actualSecond[i]=legs.Second;var adj=Add(legs.First,legs.Second);actualAdjoints[i]=adj;counts["adjointInputs"]++;adjointPassed&=Equal(adj,Caa.AdjointLiteral(inputs[i]))&&Equal(adj,Caa.AdjointSimplified(inputs[i]))&&FourierReal(adj,2)&&Positions(adj)==adjointSupports[i]&&adj.Count==2*adjointSupports[i];
  FT first=Phase(firstCoefficients[i],i is 1 or 2 or 3),second=i==4?Phase(Scale(Caa.Gamma2,-2*s),false):new();foreach(var (actual,oracle) in new[]{(legs.First,first),(legs.Second,second)}){counts["adjointLegs"]++;adjointPassed&=Equal(actual,oracle)&&FourierReal(actual,2);}
  var divFirst=DqAdjoint(q,legs.First);var divSecond=DqAdjoint(q,legs.Second);foreach(var (actual,oracle) in new[]{(divFirst,divFirstExpected[i]),(divSecond,i==4?Scale(B,-2*s):new FT())}){counts["adjointDivergenceLegs"]++;adjointPassed&=Equal(actual,oracle)&&FourierReal(actual,1);}
  FT outerExpected=i==4?Phase(One(0,0,Scalar.I*(2*s)),false):new();adjointPassed&=Equal(Caa.OuterAdjoint(inputs[i]),outerExpected);
  int nonzero=0;var reconstructed=Reconstruct(inputs[i],i,(pass,nz)=>{counts["adjointSupportProbes"]++;adjointPassed&=pass;if(nz){counts["adjointNonzeroProbes"]++;nonzero++;}});adjointPassed&=Equal(reconstructed,adj)&&nonzero==adjointSupports[i];
  var forward=Caa.ForwardStages(Dq(q,inputs[i]));columnsPassed&=Equal(forward[7],forwardExpected[i])&&Equal(Star(forward[2]),forwardExpected[i])&&forward[3].Count==0&&forward[5].Count==0;
  hActual[i]=H(q,inputs[i]);counts["hessianColumns"]++;columnsPassed&=Equal(hActual[i],hExpected[i])&&Equal(hActual[i],Scale(Add(forward[7],Add(divFirst,divSecond)),Fourier.Half))&&FourierReal(hActual[i],1);maximumFrequency=System.Math.Max(maximumFrequency,adj.Keys.Concat(hActual[i].Keys).Select(k=>System.Math.Abs(k.K0)).DefaultIfEmpty().Max());
  adjointEvidence.Add(new{input=fx.GetProperty("inputOrder")[i].GetString(),positions=Positions(adj),first=Terms(legs.First),second=Terms(legs.Second),divergenceFirst=Terms(divFirst),divergenceSecond=Terms(divSecond),hessian=Terms(hActual[i])});
 }
 foreach(var (input,oracle) in new[]{(hActual[0],Scale(U,-12*s)),(hActual[3],Scale(E,-12*s))}){counts["fullSecondApplications"]++;columnsPassed&=Equal(H(q,input),oracle);}
 counts["leakageRows"]++;columnsPassed&=Z.Count==4&&Equal(hActual[1],Add(Scale(U,-12),Z))&&Equal(Add(Scale(hActual[1],s),Scale(hActual[2],-1)),hActual[3]);
 FT[] carrier=[U,E];FT[] carrierH=[hActual[0],hActual[3]];Rational[,] gram=new Rational[2,2],matrix=new Rational[2,2];var probeU=Trig(0,false,1,4);var probeE=Trig(0,true,1,5);Rational denomU=Pair(probeU,U),denomE=Pair(probeE,E);actionGramPassed&=denomU==new Rational(-n*a,2)&&denomE==new Rational(n*n*b*b,2)&&denomU!=0&&denomE!=0&&HAnti(probeU)&&HAnti(probeE)&&Pair(probeU,probeU)==new Rational(-1,2)&&Pair(probeE,probeE)==new Rational(1,2);
 for(int i=0;i<2;i++)for(int j=0;j<2;j++)
 {counts["gramEntries"]++;gram[i,j]=Pair(carrier[i],carrier[j]);Rational gramExpected=i==j?(i==0?new Rational(-s,2):6*s*s):0;actionGramPassed&=gram[i,j]==gramExpected;counts["actionPolarizations"]++;Rational polarized=(Pair(carrier[i],Caa.Forward(Dq(q,carrier[j])))+Pair(carrier[j],Caa.Forward(Dq(q,carrier[i]))))*new Rational(1,2);actionGramPassed&=polarized==(i==j?0:6*s*s)&&polarized==Pair(carrier[i],carrierH[j]);counts["carrierMatrixEntries"]++;Rational denom=i==0?denomU:denomE;matrix[i,j]=Pair(i==0?probeU:probeE,carrierH[j])*new Rational(denom.Denominator,denom.Numerator);columnsPassed&=matrix[i,j]==(i==1&&j==0?1:i==0&&j==1?-12*s:0);}
 for(int j=0;j<2;j++)columnsPassed&=Equal(carrierH[j],Add(Scale(U,new Scalar(matrix[0,j],0)),Scale(E,new Scalar(matrix[1,j],0))));
 Rational[] actionCoefficients=[Pair(U,Caa.Forward(Dq(q,U)))*new Rational(1,2),(Pair(U,Caa.Forward(Dq(q,E)))+Pair(E,Caa.Forward(Dq(q,U))))*new Rational(1,2),Pair(E,Caa.Forward(Dq(q,E)))*new Rational(1,2)];for(int j=0;j<3;j++){counts["actionCoefficients"]++;actionGramPassed&=actionCoefficients[j]==(j==1?6*s*s:0);}
 Rational determinant=matrix[0,0]*matrix[1,1]-matrix[0,1]*matrix[1,0];int rank=determinant!=0?2:matrix.Cast<Rational>().Any(t=>t!=0)?1:0;columnsPassed&=determinant==12*s&&rank==(isNull?1:2);
 if(isNull){counts["nullRows"]++;counts["nullProbePairs"]+=2;nullPassed&=U.Count==4&&E.Count==8&&hActual[3].Count==0&&Equal(hActual[0],E)&&gram.Cast<Rational>().All(t=>t==0)&&actionCoefficients.All(t=>t==0)&&denomU==new Rational(-n,2)&&denomE==new Rational(n*n,2)&&Adjoint.CoefficientPair(E,E)!=0;}else{counts["nonNullRows"]++;actionGramPassed&=gram[0,0].Numerator.Sign==(s>0?-1:1)&&gram[1,1].Numerator.Sign==1&&hActual[3].Count==4;}
 counts["longitudinalRows"]++;longitudinalPassed&=Dq(q,Y).Count==0&&hActual[4].Count==0&&Pair(Y,Y)==new Rational(-s*s,2)&&Positions(actualFirst[4])==(isNull?48:49)&&Positions(actualSecond[4])==(isNull?0:91);
 if(!isNull){counts["longitudinalNonzeroDivergenceRows"]++;counts["wrongDeletedAdjointRows"]++;var df=DqAdjoint(q,actualFirst[4]);var ds=DqAdjoint(q,actualSecond[4]);longitudinalPassed&=Positions(df)==26&&Positions(ds)==26&&Add(df,ds).Count==0&&Equal(Scale(df,Fourier.Half),Scale(B,s))&&Scale(df,Fourier.Half).Count!=0&&Pair(Y,Y)!=0;}
 counts["wrongMetricRows"]++;var wrongMetric=Scale(Add(Caa.Forward(Dq(q,U)),Scale(Contract(q,Partial(actualAdjoints[0],0),true),-1)),Fourier.Half);columnsPassed&=!Equal(wrongMetric,E);
 rows.Add(new{a,b,n,qSquared=s,isNull,carrierRank=rank,carrierDeterminant=determinant.ToString(),gram=new[]{new[]{gram[0,0].ToString(),gram[0,1].ToString()},new[]{gram[1,0].ToString(),gram[1,1].ToString()}},matrix=new[]{new[]{matrix[0,0].ToString(),matrix[0,1].ToString()},new[]{matrix[1,0].ToString(),matrix[1,1].ToString()}},originalActionCoefficients=actionCoefficients.Select(t=>t.ToString()).ToArray(),probeUPair=denomU.ToString(),probeEPair=denomE.ToString(),longitudinalNorm=Pair(Y,Y).ToString(),U=Terms(U),E=Terms(E),Y=Terms(Y),adjointEvidence});
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resource=fx.GetProperty("resources");bool resourcesPassed=CoefficientProducts<=resource.GetProperty("maximumCoefficientProducts").GetInt64()&&LargestTensor<=resource.GetProperty("maximumSparseTensorTerms").GetInt32()&&maximumFrequency<=resource.GetProperty("maximumStoredFrequency").GetInt32();
bool controlsPassed=knownAnswerPassed&&adjointPassed&&columnsPassed&&actionGramPassed&&nullPassed&&longitudinalPassed&&countsPassed&&resourcesPassed;string verdict=!knownAnswerPassed?precedence[1]:!adjointPassed?precedence[2]:!columnsPassed?precedence[3]:!actionGramPassed?precedence[4]:!nullPassed?precedence[5]:!longitudinalPassed?precedence[6]:!countsPassed||!resourcesPassed?precedence[7]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,adjointPassed,columnsPassed,actionGramPassed,nullPassed,longitudinalPassed,countsPassed,resourcesPassed,counts,coefficientProducts=CoefficientProducts,largestTensorDuringAssembly=LargestTensor,maximumFrequency,rows,fullSymbolRankComputed=false,fullDeterminantComputed=false,physicalCharacteristicConeSelected=false,physicalTimeSelected=false,globalGaugeQuotientEstablished=false,sourceOperatorSelected=false,nullProbeExtendedCarrierClosureClaimed=false});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=612,phaseId="phase612-covariant-caa-symbol-null-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/covariant_caa_symbol_null_audit.json",json);File.WriteAllText(Root+"/output/covariant_caa_symbol_null_audit_summary.json",json);Console.WriteLine($"Phase612 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
