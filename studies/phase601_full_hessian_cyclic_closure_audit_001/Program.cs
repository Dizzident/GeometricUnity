using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static Cyclic;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase601_full_hessian_cyclic_closure_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P597="studies/phase597_actual_quadratic_joint_null_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase592_companion_tensor_chirality_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase601-a54-full-hessian-cyclic-closure-v1";
const string Success="full-hessian-cyclic-closure-controls-pass-compression-not-spectrum";
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "background":"flat fixed metric/reference/epsilon/tensors; normalized periodic x0, other coordinates constant",
 "carrierScope":"full connection carrier only; not metric-plus-connection Hessian",
 "chiralities":[-1,1],"a":1,"kappa":0,"formalCSlots":["1","c"],
 "phi1":"P gamma,P=1+h Omega","phi2":"(c-i h Omega)Gamma2",
 "operator":"H=(K d+dAdjoint KAdjoint)/2; actual full literal and simplified reverse chains",
 "pairing":"-ReTr/128 with signed form metric and normalized Fourier zero-mode integral",
 "u":"theta0 gamma2 cos(x0)","v":"theta1 Gamma12 sin(x0)",
 "e":"[sum_(j!=0,2)theta_j gamma2 gamma_j+ic theta2 I]sin(x0)",
 "Z":"sum_(j!=0,2)theta_j gamma0 gamma_j gamma2 cos(x0)",
 "f":"P[-(12+c^2)theta0 gamma2 cos+(c^2-12)theta2 gamma0 cos-11ic Z]",
 "firstAdjointLeg":"e0-h theta2 Omega sin","secondAdjointLegConstant":"h theta2 Omega sin","secondAdjointLegC":"i theta2 I sin",
 "cyclicBasis":["u","e=Hu","f=H^2u"],"g":"(12+c^2)/2",
 "cyclicGram":[["-1/2","0","g"],["0","g","0"],["g","0","0"]],"cyclicDeterminant":"-g^3",
 "cyclicMatrix":[[0,0,0],[1,0,0],[0,1,0]],"cyclicPowerRanks":[2,1,0],"cyclicCharacteristic":[0,0,0,1],
 "fourCarrierC":0,"fourBasis":["u","v","e0","w=P(theta0 gamma2+theta2 gamma0)cos"],
 "fourMatrix":[[0,0,0,0],[0,0,0,0],[1,0,0,0],[0,1,-12,0]],
 "fourGram":[["-1/2","0","0","-1/2"],["0","1/2","-1/2","0"],["0","-1/2","6","0"],["-1/2","0","0","0"]],
 "fourDeterminant":"-11/16","fourPowerRanks":[2,1,0],"fourCharacteristic":[0,0,0,0,1],
 "compressionMatrix":[[0,1],[-1,0]],"compressionGram":[["-1/2","0"],["0","1/2"]],"compressionCharacteristic":[1,0,1],
 "compressionEigenvalues":"plus/minus i; exact polynomial only, no eigensolver",
 "leakage":["e0+v","w-u"],"leakageNormSquared":["11/2","1/2"],
 "compressionSquareDifference":[[-11,0],[0,1]],"feedback":"Pi H(1-Pi)H equals Pi H^2-(Pi H)^2",
 "actionBilinearOracle":"[B(X,KdY)+B(Y,KdX)]/2, computed independently from forward K",
 "constantControl":"H(theta0 gamma2)=0 in both formal c slots",
 "knownAnswerGrades":[0,1,2,12,13,14],"polynomialChecks":"(1+c)(1-c)=1-c^2; signed determinant, rank, inverse and characteristic known answers",
 "counts":{"wordCases":44944,"hodgeCases":16384,"chiralityRows":2,"operatorSlotApplications":40,"adjointLegComparisons":6,
  "cyclicRows":2,"cyclicGramEntries":18,"cyclicActionBilinearChecks":18,"fourRows":2,"fourGramEntries":32,"fourActionBilinearChecks":32,
  "compressionRows":2,"leakageRows":4,"leakageOrthogonalityChecks":8,"feedbackEntries":8,"constantRows":2},
 "exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":10,"maximumEstimatedCpuSeconds":60,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":268435456,
  "uTermBound":2,"eCoefficientTermBound":26,"fCoefficientTermBound":64,"maximumUntrimmedIterateDegree":3}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","full-source-application-control-failed","formal-cyclic-closure-control-failed","four-carrier-closure-control-failed","compression-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["polynomial-helper"]=Root+"/CyclicPolynomial.cs",["project"]=Root+"/Phase601FullHessianCyclicClosureAudit.csproj",["study"]=Root+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",
 ["phase597-summary"]=P597+"/output/actual_quadratic_joint_null_audit_summary.json",["phase597-contract"]=P597+"/preregistration/contract_v1.json",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==601&&contract.GetProperty("contractId").GetString()==ContractId
  &&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(v=>v.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(v=>new Binding(v.GetProperty("id").GetString()!,v.GetProperty("path").GetString()!,v.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(v=>v.id).Distinct().Count()==paths.Count&&bindings.Select(v=>v.path).Distinct().Count()==paths.Count&&bindings.All(v=>paths.TryGetValue(v.id,out var p)&&p==v.path&&v.hashMatches);
 if(contractValid&&exactBindingsValid)
 {using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var files=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&files.Select(v=>v.GetProperty("path").GetString()).SequenceEqual(live)&&files.All(v=>Sha(v.GetProperty("path").GetString()!)==v.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase597","actual-quadratic-joint-null-controls-pass-dynamics-unselected")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var upstream=ud.RootElement;bool ok=upstream.GetProperty("auditPassed").GetBoolean()&&upstream.GetProperty("contractValid").GetBoolean()&&upstream.GetProperty("exactBindingsValid").GetBoolean()&&upstream.GetProperty("coreSourceTreeValid").GetBoolean()&&upstream.GetProperty("verdictKind").GetString()==terminal&&upstream.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&upstream.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&upstream.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&upstream.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>upstream.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&upstream.GetProperty("externalReviewPending").GetBoolean()&&upstream.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchorsValid=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("counts");int wordCases=0,hodgeCases=0;bool knownAnswerPassed=true;
int[] masks=Enumerable.Range(0,16384).Where(m=>new[]{0,1,2,12,13,14}.Contains(Degree(m))).ToArray();foreach(int a in masks)foreach(int b in masks){wordCases++;knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);}
for(int m=0;m<16384;m++){hodgeCases++;int r=Degree(m);knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((r*(14-r)+7)%2==0?1:-1);}
CP c=CP.Variable;Rational[][] known=[[0,2],[3,0]];
knownAnswerPassed&=((c+1)*((CP)1-c)).Same(new CP(1,0,-1))&&Determinant(known.Select(row=>row.Select(v=>(CP)v).ToArray()).ToArray()).Same((CP)(-6))
 &&Rank(known)==2&&Rank(new Rational[][]{[1,2],[2,4]})==1&&SameMatrix(Multiply(known,Inverse(known)),Identity(2))&&Characteristic(known).Same(new CP(-6,0,1));
FT sin=Trig(0,true),cos=Trig(0,false),u=Trig(0,false,1,4),v=Trig(0,true,2,6);
knownAnswerPassed&=Equal(Partial(sin,0),cos)&&Equal(Partial(cos,0),Fourier.Scale(sin,-1))&&Fourier.Pair(sin,sin)==new Rational(-1,2)&&Fourier.Pair(cos,sin)==0&&Fourier.Pair(u,u)==new Rational(-1,2)&&Fourier.Pair(v,v)==new Rational(1,2)
 &&wordCases==N("wordCases")&&hodgeCases==N("hodgeCases");
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,wordCases,hodgeCases});return;}

FT gamma=new(),gamma2=new();for(int i=0;i<14;i++)gamma=Fourier.Add(gamma,One(1<<i,1<<i,1));for(int i=0;i<14;i++)for(int j=i+1;j<14;j++){int m=(1<<i)|(1<<j);gamma2=Fourier.Add(gamma2,One(m,m,1));}
int chiralityRows=0,operatorSlotApplications=0,adjointLegComparisons=0,cyclicRows=0,cyclicGramEntries=0,cyclicActionBilinearChecks=0,fourRows=0,fourGramEntries=0,fourActionBilinearChecks=0,compressionRows=0,leakageRows=0,leakageOrthogonalityChecks=0,feedbackEntries=0,constantRows=0;
bool sourceApplicationsPassed=true,cyclicPassed=true,fourPassed=true,compressionPassed=true,constantPassed=true,supportPassed=true;
var cyclicOutput=new List<object>();var fourOutput=new List<object>();var compressionOutput=new List<object>();var legsOutput=new List<object>();
foreach(int h in fx.GetProperty("chiralities").EnumerateArray().Select(q=>q.GetInt32()))
{
 chiralityRows++;FT p1=Fourier.Add(gamma,Fourier.Scale(Omega(gamma),h));FT[] p2=[Fourier.Scale(Omega(gamma2),Scalar.I*-h),gamma2];
 FT ProjectChiral(FT t)=>Fourier.Add(t,Fourier.Scale(Omega(t),h));
 FT HSlot(FT t,int slot)
 {operatorSlotApplications++;var literal=Adjoint.QuadraticHessian(t,p1,p2[slot],slot==0);var simplified=Adjoint.QuadraticHessianSimplified(t,p1,p2[slot],slot==0);sourceApplicationsPassed&=Equal(literal,simplified)&&Typed(literal,1)&&HAnti(t)&&HAnti(literal);return literal;}
 FT[] H(FT[] t)
 {var result=Enumerable.Range(0,t.Length+1).Select(_=>new FT()).ToArray();for(int i=0;i<t.Length;i++)for(int slot=0;slot<2;slot++)result[i+slot]=Fourier.Add(result[i+slot],HSlot(t[i],slot));return Normalize(result);}
 FT[] ForwardKd(FT[] t)
 {var result=Enumerable.Range(0,t.Length+1).Select(_=>new FT()).ToArray();for(int i=0;i<t.Length;i++)for(int slot=0;slot<2;slot++)result[i+slot]=Fourier.Add(result[i+slot],Chain(D(t[i]),p1,p2[slot],slot==0));return Normalize(result);}
 var e=H([u]);var f=H(e);var hf=H(f);var constant=H([One(1,4,1)]);constantRows++;constantPassed&=Zero(constant);
 FT e0=new(),e1=Fourier.Scale(Trig(0,true,4,0),Scalar.I),zz=new();for(int j=1;j<14;j++)if(j!=2)
 {e0=Fourier.Add(e0,Fourier.Scale(Trig(0,true,1<<j,4^(1<<j)),BladeSign(4,1<<j)));int m=1^(1<<j)^4;int sign=BladeSign(1,1<<j)*BladeSign(1^(1<<j),4);zz=Fourier.Add(zz,Fourier.Scale(Trig(0,false,1<<j,m),sign));}
 FT b=Trig(0,false,4,1);FT[] eExpected=[e0,e1];FT[] fExpected=[Fourier.Scale(ProjectChiral(Fourier.Add(u,b)),-12),Fourier.Scale(ProjectChiral(zz),Scalar.I*-11),ProjectChiral(Fourier.Add(Fourier.Scale(u,-1),b))];
 var firstLeg=Fourier.Scale(Adjoint.DAdjoint(Adjoint.KAdjointLiteral(u,p1,new FT(),true)),Fourier.Half);
 var secondLeg=Fourier.Scale(Adjoint.DAdjoint(Adjoint.KAdjointLiteral(u,p1,p2[0],false)),Fourier.Half);
 var secondC=Fourier.Scale(Adjoint.DAdjoint(Adjoint.KAdjointLiteral(u,p1,p2[1],false)),Fourier.Half);FT omegaMode=Fourier.Scale(Trig(0,true,4,Full),h);
 adjointLegComparisons+=3;bool legs=Equal(firstLeg,Fourier.Add(e0,Fourier.Scale(omegaMode,-1)))&&Equal(secondLeg,omegaMode)&&Equal(secondC,e1);sourceApplicationsPassed&=legs;
 legsOutput.Add(new{h,passed=legs,first=Fourier.Terms(firstLeg),secondConstant=Fourier.Terms(secondLeg),secondC=Fourier.Terms(secondC)});
 CP gg=((CP)12+c*c).Scale(new Rational(1,2));FT[][] basis3=[[u],e,f];FT[][] images3=[e,f,hf];CP[][] gram3=Gram(basis3),predictedGram3=[[(CP)new Rational(-1,2),0,gg],[0,gg,0],[gg,0,0]];
 cyclicRows++;cyclicGramEntries+=9;bool action3=true;var kd3=basis3.Select(ForwardKd).ToArray();
 for(int i=0;i<3;i++)for(int j=0;j<3;j++){cyclicActionBilinearChecks++;CP oracle=(Cyclic.Pair(basis3[i],kd3[j])+Cyclic.Pair(basis3[j],kd3[i])).Scale(new Rational(1,2));action3&=Cyclic.Pair(basis3[i],images3[j]).Same(oracle);}
 var j3=ReadMatrix("cyclicMatrix");bool cycle=Same(e,eExpected)&&Same(f,fExpected)&&Zero(hf)&&SameGram(gram3,predictedGram3)&&Determinant(gram3).Same((gg*gg*gg).Scale(-1))&&action3
  &&PowerRanks(j3).SequenceEqual(new Rational[]{2,1,0})&&Characteristic(j3).Same(new CP(0,0,0,1))
  &&e.All(t=>t.Keys.All(k=>(Degree(k.Blade)&1)==0))&&f.All(t=>t.Keys.All(k=>(Degree(k.Blade)&1)==1)&&Equal(Omega(t),Fourier.Scale(t,h)));
 cyclicPassed&=cycle;supportPassed&=u.Count<=2&&e.Sum(t=>t.Count)<=26&&f.Sum(t=>t.Count)<=64;
 cyclicOutput.Add(new{h,passed=cycle,actionBilinearPassed=action3,e=Cyclic.Terms(e),f=Cyclic.Terms(f),hF=Cyclic.Terms(hf),gram=GramText(gram3),gramDeterminant=Determinant(gram3).Text(),matrix=Cyclic.Text(j3),powerRanks=PowerRanks(j3).Select(q=>q.ToString()).ToArray(),characteristic=Characteristic(j3).Text(),cDependentCarrier=true,coefficientwiseOperatorClosureClaimed=false});

 FT w=ProjectChiral(Fourier.Add(u,b));FT[] basis4=[u,v,e[0],w],images4=basis4.Select(t=>HSlot(t,0)).ToArray();
 Rational[][] gram4=Gram(basis4),gram4Expected=[[new(-1,2),0,0,new(-1,2)],[0,new(1,2),new(-1,2),0],[0,new(-1,2),6,0],[new(-1,2),0,0,0]];
 Rational[][] cov4=basis4.Select(x=>images4.Select(y=>Fourier.Pair(x,y)).ToArray()).ToArray(),g4Inverse=Inverse(gram4),j4=Multiply(g4Inverse,cov4),j4Expected=ReadMatrix("fourMatrix");
 fourRows++;fourGramEntries+=16;bool action4=true;var kd4=basis4.Select(t=>Chain(D(t),p1,p2[0],true)).ToArray();
 for(int i=0;i<4;i++)for(int j=0;j<4;j++){fourActionBilinearChecks++;action4&=cov4[i][j]==(Fourier.Pair(basis4[i],kd4[j])+Fourier.Pair(basis4[j],kd4[i]))*new Rational(1,2);}
 bool fullColumns=true;for(int j=0;j<4;j++){FT recovered=new();for(int i=0;i<4;i++)recovered=Fourier.Add(recovered,Fourier.Scale(basis4[i],new Scalar(j4[i][j],0)));fullColumns&=Equal(recovered,images4[j]);}
 bool four=SameMatrix(gram4,gram4Expected)&&SameMatrix(Multiply(gram4,g4Inverse),Identity(4))&&SameMatrix(j4,j4Expected)&&fullColumns&&action4
  &&Determinant(gram4.Select(row=>row.Select(x=>(CP)x).ToArray()).ToArray()).Same((CP)new Rational(-11,16))&&PowerRanks(j4).SequenceEqual(new Rational[]{2,1,0})&&Characteristic(j4).Same(new CP(0,0,0,0,1))
  &&SameMatrix(cov4,Transpose(cov4))&&!SameMatrix(j4,Transpose(j4));fourPassed&=four;
 fourOutput.Add(new{h,passed=four,fullColumnsPassed=fullColumns,actionBilinearPassed=action4,gram=Cyclic.Text(gram4),gramDeterminant="-11/16",matrix=Cyclic.Text(j4),loweredHessian=Cyclic.Text(cov4),powerRanks=PowerRanks(j4).Select(q=>q.ToString()).ToArray(),characteristic=Characteristic(j4).Text(),images=images4.Select(Fourier.Terms).ToArray()});

 FT[] seeds=[u,v];Rational[] seedNorm=[Fourier.Pair(u,u),Fourier.Pair(v,v)];FT Pi(FT t){FT result=new();for(int i=0;i<2;i++)result=Fourier.Add(result,Fourier.Scale(seeds[i],new Scalar(Fourier.Pair(seeds[i],t)*Inv(seedNorm[i]),0)));return result;}
 FT[] projected=images4.Take(2).Select(Pi).ToArray(),leaks=Enumerable.Range(0,2).Select(i=>Fourier.Add(images4[i],Fourier.Scale(projected[i],-1))).ToArray(),hLeaks=leaks.Select(t=>HSlot(t,0)).ToArray();
 Rational[][] compressed=seeds.Select((s,i)=>projected.Select(t=>Fourier.Pair(s,t)*Inv(seedNorm[i])).ToArray()).ToArray(),c2=Multiply(compressed,compressed);
 FT[] fullH2=[images4[2],images4[3]];Rational[][] pH2=seeds.Select((s,i)=>fullH2.Select(t=>Fourier.Pair(s,t)*Inv(seedNorm[i])).ToArray()).ToArray(),feedback=seeds.Select((s,i)=>hLeaks.Select(t=>Fourier.Pair(s,t)*Inv(seedNorm[i])).ToArray()).ToArray();
 Rational[][] difference=pH2.Select((row,i)=>row.Select((x,j)=>x-c2[i][j]).ToArray()).ToArray();
 compressionRows++;bool leakOk=true;for(int j=0;j<2;j++){leakageRows++;leakOk&=leaks[j].Count>0&&Fourier.Pair(leaks[j],leaks[j])==(j==0?new Rational(11,2):new Rational(1,2));foreach(var s in seeds){leakageOrthogonalityChecks++;leakOk&=Fourier.Pair(s,leaks[j])==0;}}
 feedbackEntries+=4;bool compression=SameMatrix(compressed,ReadMatrix("compressionMatrix"))&&SameMatrix(c2,new Rational[][]{[-1,0],[0,-1]})&&Characteristic(compressed).Same(new CP(1,0,1))&&leakOk
  &&Equal(leaks[0],Fourier.Add(e[0],v))&&Equal(leaks[1],Fourier.Add(w,Fourier.Scale(u,-1)))&&SameMatrix(difference,ReadMatrix("compressionSquareDifference"))&&SameMatrix(feedback,difference);
 compressionPassed&=compression;compressionOutput.Add(new{h,passed=compression,compressedMatrix=Cyclic.Text(compressed),characteristic=Characteristic(compressed).Text(),squared=Cyclic.Text(c2),projectedFullSquared=Cyclic.Text(pH2),squareDifference=Cyclic.Text(difference),feedback=Cyclic.Text(feedback),leakage=leaks.Select(Fourier.Terms).ToArray(),leakageNormSquared=leaks.Select(t=>Fourier.Pair(t,t).ToString()).ToArray(),restrictedActionValid=true,fullSpectralModesClaimed=false});
}
var counts=new Dictionary<string,int>{["wordCases"]=wordCases,["hodgeCases"]=hodgeCases,["chiralityRows"]=chiralityRows,["operatorSlotApplications"]=operatorSlotApplications,["adjointLegComparisons"]=adjointLegComparisons,["cyclicRows"]=cyclicRows,["cyclicGramEntries"]=cyclicGramEntries,["cyclicActionBilinearChecks"]=cyclicActionBilinearChecks,["fourRows"]=fourRows,["fourGramEntries"]=fourGramEntries,["fourActionBilinearChecks"]=fourActionBilinearChecks,["compressionRows"]=compressionRows,["leakageRows"]=leakageRows,["leakageOrthogonalityChecks"]=leakageOrthogonalityChecks,["feedbackEntries"]=feedbackEntries,["constantRows"]=constantRows};
bool countsPassed=counts.Count==expected.EnumerateObject().Count()&&counts.All(k=>expected.GetProperty(k.Key).GetInt32()==k.Value);
bool controlsPassed=sourceApplicationsPassed&&constantPassed&&supportPassed&&cyclicPassed&&fourPassed&&compressionPassed&&countsPassed;
string verdict=!sourceApplicationsPassed||!constantPassed||!supportPassed?precedence[2]:!cyclicPassed?precedence[3]:!fourPassed?precedence[4]:!compressionPassed||!countsPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,sourceApplicationsPassed,constantPassed,supportPassed,cyclicPassed,fourPassed,compressionPassed,countsPassed,counts,adjointLegRows=legsOutput,cyclicRows=cyclicOutput,fourRows=fourOutput,compressionRows=compressionOutput,
 fixedGeometryConnectionCarrierOnly=true,combinedMetricConnectionHessianTested=false,generalNilpotencyIsWrittenProof=true,fullSpectrumFromCompressionClaimed=false,sourceNormSelected=false,physicalDynamicsSelected=false});

int N(string key)=>expected.GetProperty(key).GetInt32();
Rational[][] ReadMatrix(string key)=>fx.GetProperty(key).EnumerateArray().Select(row=>row.EnumerateArray().Select(v=>new Rational(v.GetInt64())).ToArray()).ToArray();
void Emit(string terminal,object evidence)
{
 var result=new{schemaVersion=1,phase=601,phaseId="phase601-full-hessian-cyclic-closure-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(v=>v.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(v=>v.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/full_hessian_cyclic_closure_audit.json",json);File.WriteAllText(Root+"/output/full_hessian_cyclic_closure_audit_summary.json",json);Console.WriteLine($"Phase601 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
