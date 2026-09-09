using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase603_homogeneous_invariant_stationary_family_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P595="studies/phase595_invariant_tensor_dimension_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase592_companion_tensor_chirality_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase603-a55-homogeneous-invariant-stationary-family-v1";
const string Success="homogeneous-invariant-stationary-family-controls-pass-scale-unselected";
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
 "background": "flat fixed metric/reference/epsilon/tensors; constant connection and normalized density",
 "carrierScope": "full connection gradient; closed two-dimensional local Spin-invariant Hessian only; no metric variations",
 "chiralities": [
  -1,
  1
 ],
 "variables": [
  "a",
  "b",
  "c",
  "gamma",
  "kappa"
 ],
 "phi1": "P Gamma1, P=1+h Omega",
 "phi2": "(c-i h Omega)Gamma2",
 "pairing": "-ReTr/128 with signed form metric",
 "S": "a Gamma1+b Omega Gamma1",
 "r": "a-h b",
 "s": "a+h b",
 "Q": "2 r s Gamma2",
 "KQ": "312 r s P Gamma1",
 "KAdjointS": "-24 r Gamma2-28 i h c r Omega Gamma2",
 "DQAdjointKAdjointS": "624 r S",
 "norm": "-14 r s",
 "rawCubic": "-4368 r^2 s",
 "action": "-1456 gamma r^2 s-7 kappa r s",
 "gradient": "104 gamma r s P Gamma1+(208 gamma r+kappa)S",
 "gramAB": [
  [
   -14,
   0
  ],
  [
   0,
   14
  ]
 ],
 "gramRS": [
  [
   0,
   -7
  ],
  [
   -7,
   0
  ]
 ],
 "raisedGradientRS": [
  "r(208 gamma r+kappa)",
  "s(416 gamma r+kappa)"
 ],
 "hessianRS": [
  [
   "416 gamma r+kappa",
   "0"
  ],
  [
   "416 gamma s",
   "416 gamma r+kappa"
  ]
 ],
 "hessianCharacteristic": "(t-416 gamma r-kappa)^2",
 "nonzeroBranchLoweredHessianAB": [["14 kappa","0"],["0","-14 kappa"]],
 "invariantCompleteness": "independent written equivariance plus bound595 full real invariant classification; no repeated census",
 "branches": [
  "both-zero/all-a-b",
  "mass-only/origin",
  "cubic-only/origin",
  "cubic-only/chiral-line",
  "both-nonzero/origin",
  "both-nonzero/opposite-chiral"
 ],
 "branchSubstitutions": "free symbolic t uses variable a; opposite branch a=-t,b=h t,kappa=416 gamma t; chiral a=t,b=h t,kappa=0",
 "branchHessians": [
  "zero",
  "kappa I",
  "zero",
  "nonzero rank-one square-zero for gamma*t!=0",
  "kappa I",
  "-kappa I"
 ],
 "decoys": [
  "opposite-chiral at kappa0 has Q0 but G=416 gamma t^2(1-h Omega)Gamma1",
  "chiral at nonzero kappa has G=kappa S",
  "at stationary opposite branch curvature-only gradient=kappa S!=0"
 ],
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
  "polynomialChecks": 5,
  "chiralityRows": 2,
  "fullIdentityChecks": 16,
  "actionDerivativeChecks": 4,
  "hessianColumns": 4,
  "hessianBilinears": 8,
  "stationaryRows": 12,
  "decoyRows": 6
 },
 "coefficientCountsPerChirality": {
  "Q": 182,
  "KQ": 56,
  "KAdjointS": 364,
  "DQAdjointKAdjointS": 56,
  "cubicGradient": 84,
  "massGradient": 28,
  "action": 6
 },
 "exactTolerance": 0,
 "coreFileCount": 726,
 "resource": {
  "estimatedCpuSeconds": 10,
  "maximumEstimatedCpuSeconds": 60,
  "estimatedPeakBytes": 134217728,
  "maximumEstimatedPeakBytes": 268435456,
  "maximumFinalTensorCoefficientCount": 364,
  "maximumPolynomialTotalDegree": 4
 },
 "sourceNormSelected": false,
 "physicalVacuumSelected": false,
 "physicalScaleSelected": false
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","full-symbolic-gradient-control-failed","invariant-hessian-control-failed","stationary-family-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["polynomial-helper"]=Root+"/InvariantPolynomial.cs",["project"]=Root+"/Phase603HomogeneousInvariantStationaryFamilyAudit.csproj",["study"]=Root+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",
 ["phase595-summary"]=P595+"/output/invariant_tensor_dimension_audit_summary.json",["phase595-contract"]=P595+"/preregistration/contract_v1.json",["phase595-study"]=P595+"/STUDY.md",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==603&&contract.GetProperty("contractId").GetString()==ContractId
  &&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
  &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(v=>v.GetString()).SequenceEqual(precedence)
  &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(v=>new Binding(v.GetProperty("id").GetString()!,v.GetProperty("path").GetString()!,v.GetProperty("sha256").GetString()!)).ToArray();
 exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(v=>v.id).Distinct().Count()==paths.Count&&bindings.Select(v=>v.path).Distinct().Count()==paths.Count&&bindings.All(v=>paths.TryGetValue(v.id,out var p)&&p==v.path&&v.hashMatches);
 if(contractValid&&exactBindingsValid)
 {using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var files=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&files.Select(v=>v.GetProperty("path").GetString()).SequenceEqual(live)&&files.All(v=>Sha(v.GetProperty("path").GetString()!)==v.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase595","invariant-tensor-dimensions-two-certified-source-choice-open")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var upstream=ud.RootElement;bool ok=upstream.GetProperty("auditPassed").GetBoolean()&&upstream.GetProperty("contractValid").GetBoolean()&&upstream.GetProperty("exactBindingsValid").GetBoolean()&&upstream.GetProperty("coreSourceTreeValid").GetBoolean()&&upstream.GetProperty("verdictKind").GetString()==terminal&&upstream.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&upstream.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&upstream.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&upstream.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>upstream.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&upstream.GetProperty("externalReviewPending").GetBoolean()&&upstream.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchorsValid=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}


var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("counts");
Poly a=Poly.Var(0),b=Poly.Var(1),c=Poly.Var(2),gamma=Poly.Var(3),kappa=Poly.Var(4);
int wordCases=0,hodgeCases=0,polynomialChecks=0;bool knownAnswerPassed=true;
int[] masks=Enumerable.Range(0,16384).Where(m=>new[]{0,1,2,12,13,14}.Contains(Degree(m))).ToArray();
foreach(int x in masks)foreach(int y in masks){wordCases++;knownAnswerPassed&=BladeSign(x,y)==WordSign(x,y);}
for(int m=0;m<16384;m++){hodgeCases++;int d=Degree(m);knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((d*(14-d)+7)%2==0?1:-1);}
bool[] polynomialKnown=[((a+b)*(a-b)).Same(a*a-b*b),(a*a*b).Derivative(0).Same((a*b).Scale(2)),
 (a*b).Substitute([a+b,a-b,c,gamma,kappa]).Same(a*a-b*b),(a-a).Zero,
 (a*a*b+a*b*b).Derivative(0).Derivative(1).Same((a+b).Scale(2))];
foreach(bool q in polynomialKnown){polynomialChecks++;knownAnswerPassed&=q;}
FT one=new(),two=new();for(int i=0;i<14;i++)one=Add(one,One(1<<i,1<<i,1));
for(int i=0;i<14;i++)for(int j=i+1;j<14;j++){int m=(1<<i)|(1<<j);two=Add(two,One(m,m,1));}
var e=TensorPoly.From(one);var o=TensorPoly.From(Omega(one));var z=TensorPoly.From(two);var oz=TensorPoly.From(Omega(two));
knownAnswerPassed&=Fourier.Pair(one,one)==-14&&Fourier.Pair(Omega(one),Omega(one))==14&&Fourier.Pair(one,Omega(one))==0
 &&Fourier.Pair(two,two)==91&&wordCases==N("wordCases")&&hodgeCases==N("hodgeCases")&&polynomialChecks==N("polynomialChecks");
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,wordCases,hodgeCases,polynomialChecks});return;}
int chiralityRows=0,fullIdentityChecks=0,actionDerivativeChecks=0,hessianColumns=0,hessianBilinears=0,stationaryRows=0,decoyRows=0;
bool fullGradientPassed=true,hessianPassed=true,stationaryPassed=true,decoysPassed=true,supportPassed=true,adjointAgreementPassed=true;
var rows=new List<object>();var branchRows=new List<object>();var negativeRows=new List<object>();
foreach(int h in new[]{-1,1})
{
 chiralityRows++;Poly r=a-b.Scale(h),s=a+b.Scale(h);
 FT p1=Add(one,Fourier.Scale(Omega(one),h)),p20=Fourier.Scale(Omega(two),Scalar.I*-h);
 TensorPoly K(TensorPoly t)=>t.Map(v=>Chain(v,p1,p20,true))+c*t.Map(v=>Chain(v,p1,two,false));
 TensorPoly Kad(TensorPoly t)
 {
  TensorPoly literal=t.Map(v=>Adjoint.KAdjointLiteral(v,p1,p20,true))+c*t.Map(v=>Adjoint.KAdjointLiteral(v,p1,two,false));
  TensorPoly simple=t.Map(v=>Adjoint.KAdjointSimplified(v,p1,p20,true))+c*t.Map(v=>Adjoint.KAdjointSimplified(v,p1,two,false));
  adjointAgreementPassed&=literal.Same(simple);return literal;
 }
 TensorPoly W(TensorPoly x,TensorPoly y)=>TensorPoly.Bilinear(x,y,(v,w)=>Product(v,w,'W'));
 TensorPoly Dqa(TensorPoly x,TensorPoly y)=>TensorPoly.Bilinear(x,y,Adjoint.DQAdjoint);
 var state=a*e+b*o;var pp=TensorPoly.From(p1);var q=W(state,state);var kq=K(q);var kad=Kad(state);var dq=Dqa(state,kad);
 var cubic=(gamma*(kq+dq)).Scale(new Rational(1,3));var mass=kappa*state;var gradient=cubic+mass;
 Poly norm=TensorPoly.Pair(state,state),raw=TensorPoly.Pair(state,kq),action=(gamma*raw).Scale(new Rational(1,3))+(kappa*norm).Scale(new Rational(1,2));
 var kadExpected=r.Scale(-24)*z+(c*r).Scale(-28*h)*oz.Map(v=>Fourier.Scale(v,Scalar.I));
 var gradientExpected=(gamma*r*s).Scale(104)*pp+((gamma*r).Scale(208)+kappa)*state;
 bool[] identities=[q.Same((r*s).Scale(2)*z),kq.Same((r*s).Scale(312)*pp),kad.Same(kadExpected),dq.Same(r.Scale(624)*state),
  gradient.Same(gradientExpected),norm.Same((r*s).Scale(-14)),raw.Same((r*r*s).Scale(-4368)),
  action.Same((gamma*r*r*s).Scale(-1456)+(kappa*r*s).Scale(-7))];
 foreach(bool pass in identities){fullIdentityChecks++;fullGradientPassed&=pass;}
 var cc=fx.GetProperty("coefficientCountsPerChirality");
 supportPassed&=q.CoefficientCount==cc.GetProperty("Q").GetInt32()&&kq.CoefficientCount==cc.GetProperty("KQ").GetInt32()
  &&kad.CoefficientCount==cc.GetProperty("KAdjointS").GetInt32()&&dq.CoefficientCount==cc.GetProperty("DQAdjointKAdjointS").GetInt32()
  &&cubic.CoefficientCount==cc.GetProperty("cubicGradient").GetInt32()&&mass.CoefficientCount==cc.GetProperty("massGradient").GetInt32()&&action.Terms.Count==cc.GetProperty("action").GetInt32();
 supportPassed&=new[]{state,q,kq,kad,dq,cubic,mass,gradient}.All(t=>t.CoefficientCount<=364&&t.Terms.Keys.All(m=>m.A+m.B+m.C+m.G+m.K<=4));
 fullGradientPassed&=state.TypedAnti(1)&&q.TypedAnti(2)&&kq.TypedAnti(1)&&kad.TypedAnti(2)&&dq.TypedAnti(1)&&gradient.TypedAnti(1)
  &&!kad.Terms.Where(t=>t.Key.C==1).All(t=>t.Value.Count==0)&&gradient.Terms.Keys.All(m=>m.C==0);
 TensorPoly[] basis=[e,o];var columns=new TensorPoly[2];var jab=new Poly[2][];
 for(int i=0;i<2;i++)
 {
  actionDerivativeChecks++;fullGradientPassed&=TensorPoly.Pair(basis[i],gradient).Same(action.Derivative(i));
  var v=basis[i];var dQ=W(state,v)+W(v,state);
  columns[i]=(gamma*(K(dQ)+Dqa(v,kad)+Dqa(state,Kad(v)))).Scale(new Rational(1,3))+kappa*v;
  hessianColumns++;hessianPassed&=columns[i].Same(gradient.Derivative(i))&&columns[i].Same(gradientExpected.Derivative(i))&&columns[i].TypedAnti(1);
 }
 for(int i=0;i<2;i++)jab[i]=columns.Select(t=>TensorPoly.Pair(basis[i],t).Scale(new Rational(i==0?-1:1,14))).ToArray();
 for(int j=0;j<2;j++)hessianPassed&=columns[j].Same(jab[0][j]*e+jab[1][j]*o);
 var lowered=new Poly[2][];for(int i=0;i<2;i++){lowered[i]=new Poly[2];for(int j=0;j<2;j++){hessianBilinears++;lowered[i][j]=TensorPoly.Pair(basis[i],columns[j]);hessianPassed&=lowered[i][j].Same(action.Derivative(i).Derivative(j));}}
 Poly[][] transform=[[(Poly)1,(Poly)(-h)],[(Poly)1,(Poly)h]];
 Poly[][] inverse=[[((Poly)1).Scale(new Rational(1,2)),((Poly)1).Scale(new Rational(1,2))],[((Poly)(-h)).Scale(new Rational(1,2)),((Poly)h).Scale(new Rational(1,2))]];
 var jrs=Multiply(Multiply(transform,jab),inverse);Poly diagonal=(gamma*r).Scale(416)+kappa,shear=(gamma*s).Scale(416);
 Poly[][] predicted=[[diagonal,0],[shear,diagonal]];
 var nil=Subtract(jrs,new Poly[][]{[diagonal,0],[0,diagonal]});
 hessianPassed&=SameMatrix(jrs,predicted)&&SameMatrix(Multiply(nil,nil),ZeroMatrix())&&!nil[1][0].Zero
  &&(jrs[0][0]+jrs[1][1]).Same(diagonal.Scale(2))&&(jrs[0][0]*jrs[1][1]-jrs[0][1]*jrs[1][0]).Same(diagonal*diagonal)
  &&lowered[0][1].Same(lowered[1][0])&&!jab[0][1].Same(jab[1][0]);
 // Simultaneous substitution: the surviving variable a is a fresh formal branch amplitude t.
 Poly[][] substitutions=[[a,b,c,0,0],[0,0,c,0,kappa],[0,0,c,gamma,0],[a,a.Scale(h),c,gamma,0],[0,0,c,gamma,kappa],[a.Scale(-1),a.Scale(h),c,gamma,(gamma*a).Scale(416)]];
 string[] names=fx.GetProperty("branches").EnumerateArray().Select(v=>v.GetString()!).ToArray();
 for(int branch=0;branch<substitutions.Length;branch++)
 {
  stationaryRows++;var sub=substitutions[branch];var gs=gradient.Substitute(sub);var js=jrs.Select(row=>row.Select(p=>p.Substitute(sub)).ToArray()).ToArray();
  var asub=action.Substitute(sub);bool pass=gs.Zero&&asub.Zero;
  Poly[][] expectedJ=branch switch{0 or 2=>ZeroMatrix(),1 or 4=>new Poly[][]{[kappa,0],[0,kappa]},3=>new Poly[][]{[0,0],[(gamma*a).Scale(832),0]},5=>new Poly[][]{[(gamma*a).Scale(-416),0],[0,(gamma*a).Scale(-416)]},_=>throw new InvalidOperationException()};
  pass&=SameMatrix(js,expectedJ);
  if(branch==3)pass&=!js[1][0].Zero&&SameMatrix(Multiply(js,js),ZeroMatrix());
  if(branch==5)
  {
   var st=state.Substitute(sub);var mt=mass.Substitute(sub);var ct=cubic.Substitute(sub);
   pass&=!st.Zero&&!mt.Zero&&q.Substitute(sub).Zero&&norm.Substitute(sub).Zero&&ct.Same(mt.Scale(-1));
   // Signed lowered Hessian: diag(14 kappa,-14 kappa), a saddle for real kappa!=0.
   pass&=lowered[0][0].Substitute(sub).Same((gamma*a).Scale(5824))&&lowered[1][1].Substitute(sub).Same((gamma*a).Scale(-5824))
    &&lowered[0][1].Substitute(sub).Zero&&lowered[1][0].Substitute(sub).Zero;
  }
  stationaryPassed&=pass;branchRows.Add(new{h,branch=names[branch],passed=pass,gradient=gs.Text(),action=asub.Text(),hessianRS=MatrixText(js)});
 }
 Poly[][] negativeSubs=[[a,a.Scale(-h),c,gamma,0],[a,a.Scale(h),c,gamma,kappa],substitutions[5]];
 for(int d=0;d<3;d++)
 {
  decoyRows++;var sub=negativeSubs[d];var actual=gradient.Substitute(sub);
  var tested=actual;
  // The shortcut is KQ+kappa S; no gamma convention can repair it on Q=0.
  if(d==2)tested=(kq+mass).Substitute(sub);
  TensorPoly predictedNegative=d switch{0=>(gamma*a*a).Scale(416)*(e-o.Scale(h)),1=>(kappa*a)*(e+o.Scale(h)),2=>mass.Substitute(sub),_=>throw new InvalidOperationException()};
  bool pass=q.Substitute(sub).Zero&&!tested.Zero&&tested.Same(predictedNegative)&&(d!=2||actual.Zero);
  decoysPassed&=pass;negativeRows.Add(new{h,kind=d,passed=pass,tested=tested.Text(),actual=actual.Text()});
 }
 rows.Add(new{h,passed=identities.All(v=>v),q=q.Text(),kQ=kq.Text(),kAdjointS=kad.Text(),dqAdjoint=dq.Text(),gradient=gradient.Text(),action=action.Text(),norm=norm.Text(),rawCubic=raw.Text(),fullHessianColumns=columns.Select(v=>v.Text()).ToArray(),hessianAB=MatrixText(jab),hessianRS=MatrixText(jrs),loweredHessian=MatrixText(lowered),cCanceledAfterFullChain=true});
}
var counts=new Dictionary<string,int>{["wordCases"]=wordCases,["hodgeCases"]=hodgeCases,["polynomialChecks"]=polynomialChecks,["chiralityRows"]=chiralityRows,["fullIdentityChecks"]=fullIdentityChecks,["actionDerivativeChecks"]=actionDerivativeChecks,["hessianColumns"]=hessianColumns,["hessianBilinears"]=hessianBilinears,["stationaryRows"]=stationaryRows,["decoyRows"]=decoyRows};
bool countsPassed=counts.Count==expected.EnumerateObject().Count()&&counts.All(v=>expected.GetProperty(v.Key).GetInt32()==v.Value);
bool controlsPassed=fullGradientPassed&&adjointAgreementPassed&&supportPassed&&hessianPassed&&stationaryPassed&&decoysPassed&&countsPassed;
string verdict=!fullGradientPassed||!adjointAgreementPassed||!supportPassed?precedence[2]:!hessianPassed?precedence[3]:!stationaryPassed||!decoysPassed||!countsPassed?precedence[4]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,fullGradientPassed,adjointAgreementPassed,supportPassed,hessianPassed,stationaryPassed,decoysPassed,countsPassed,counts,rows,branchRows,negativeRows,
 fullConnectionGradientChecked=true,invariantHessianClosed=true,fixedGeometryConnectionCarrierOnly=true,combinedMetricConnectionHessianTested=false,nonzeroBranchIsInvariantRealSaddle=true,
 sourceNormSelected=false,physicalVacuumSelected=false,physicalScaleSelected=false,physicalDynamicsSelected=false});
int N(string key)=>expected.GetProperty(key).GetInt32();
static Poly[][] ZeroMatrix()=>new Poly[][]{[0,0],[0,0]};
static Poly[][] Multiply(Poly[][] x,Poly[][] y){var z=ZeroMatrix();for(int i=0;i<2;i++)for(int j=0;j<2;j++)for(int k=0;k<2;k++)z[i][j]+=x[i][k]*y[k][j];return z;}
static Poly[][] Subtract(Poly[][] x,Poly[][] y)=>x.Select((row,i)=>row.Select((v,j)=>v-y[i][j]).ToArray()).ToArray();
static bool SameMatrix(Poly[][] x,Poly[][] y)=>x.SelectMany((row,i)=>row.Select((v,j)=>v.Same(y[i][j]))).All(v=>v);
static object[][][] MatrixText(Poly[][] x)=>x.Select(row=>row.Select(v=>v.Text()).ToArray()).ToArray();

void Emit(string terminal,object evidence)
{
 var result=new{schemaVersion=1,phase=603,phaseId="phase603-homogeneous-invariant-stationary-family-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(v=>v.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(v=>v.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/homogeneous_invariant_stationary_family_audit.json",json);File.WriteAllText(Root+"/output/homogeneous_invariant_stationary_family_audit_summary.json",json);Console.WriteLine($"Phase603 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
