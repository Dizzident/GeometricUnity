using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using static NullControls;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase609_chiral_null_branch_stationarity_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P593="studies/phase593_companion_action_first_variation_audit_001";
const string P607="studies/phase607_source_induced_vertical_curvature_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase609-a58-chiral-null-branch-stationarity-v1";
const string Success="chiral-null-branch-controls-pass-full-stationarity-conditional";
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "carrier":"fixed flat oriented unit-volume14torus/frame,epsilon=I; curved even spin control B is NOT the induced Levi-Civita connection",
 "h":[-1,1],"kappa":[-1,1],"gamma":[1,2],"formalCSlots":[0,1],
 "phi1":"P Gamma1,P=1+hOmega","phi2":"(c-i h Omega)Gamma2","pairing":"real bilinear -ReTr/128 times signed exterior metric",
 "massConvention":"kappa Pair(S,S)/2; compact versus expanded9.4 factor is unresolved, no author normalization selected",
 "background":"theta0 Gamma01+theta1 Gamma12+theta2 Gamma23",
 "curvature":"2theta01 Gamma02+2theta12 Gamma13",
 "KFB":"-4P(theta1 gamma2+theta2 gamma3)","state":"S=-KFB/kappa",
 "DBS":"8theta12 Pgamma3/kappa","Q":"0","KdaggerS":"0",
 "innerDBS":["0","16i P Gamma123 volume/kappa"],"outerInnerDBS":"0",
 "gradientLegs":["KFB","KDBS/2","DBdagger KdaggerS/2","gamma KQ/3","gamma DQdagger KdaggerS/3","kappa S"],
 "sourceResidual":"K(FB+DBS+beta Q)+kappa S=0 for every beta because Q=0; not used to construct actual gradient",
 "V":"theta1 gamma2","variationLegs":["4","0","0","-4"],
 "actionPolynomials":{"branch":"-kappa t^2/2","wrongSign":"8t-kappa t^2/2","kappa0S0":"4t"},
 "wrongSignGradient":"2KFB","wrongSignAction":"0","wrongSignTraceGradientSquare":"0","wrongSignGradientPairV":"8",
 "coefficientNorms":{"curvature":8,"KFB":64,"SAtUnitKappa":64,"wrongSignGradient":256},
 "coefficientCounts":{"background":3,"curvature":2,"KFB":4,"S":4,"DBS":2,"innerC":2},
 "gradeBattery":{"masks":"all cyclic contiguous masks of lengths1..13 on14 axes, plus0/full","maskCount":184,"oddMaskCount":98,"KInputPlanes":[[0,1],[0,7],[7,8]],"KdaggerOutputAxes":[0,7,13],"scope":"bounded operator battery; general theorem proved separately, not full form-times-Clifford enumeration"},
 "complementPairs":"4096 unordered odd mask/complement pairs times2h; normalized real H-anti blade representatives",
 "adjointSupport":{"Y":"theta1 gamma2","candidate":"for each of91twoforms ab, blade=ab XOR bit1 XOR bit2, and its full complement","candidateCount":182,"constantNonzero":78,"linearCNonzero":67,"totalNonzeroProbes":290,"independentForward":"ordered-word NaiveChain paired with Y, full182-position reconstruction for eachh/slot"},
 "pointwiseJets":["value=V,dvalue0","value=theta0 Gamma01,dvalue0","value0,dvalue=theta01 Gamma02","value0,dvalue=theta12 gamma3"],
 "metricJets":["all coframe weights1:deltaKFB=-KFB,deltaS=-S","only coframe weight0=1:deltaKFB=4theta1 Pgamma2,deltaS=-4theta1 Pgamma2/kappa"],
 "metricHeldFixed":"coordinate B,curvature,and S for partial derivative; recomputed candidate deltaS=-deltaKFB/kappa for total branch derivative; orthonormal Clifford matrices and P fixed in this identification",
 "metricDerivative":"delta star_p=(sum m-2sum_form m)star_p; deltaPhi=sum_form m Phi; pairing includes same volume/inverse-form factor",
 "backgroundJet":"deltaB=B,deltaFB=2FB;partial deltaDBS=DBS;recomputed branch deltaS=2S",
 "identificationJet":"deltaS=[gamma2,S]=-8h theta1 Omega/kappa+8theta2 Gamma23/kappa, nonzero even variation outsideL",
 "covariantAdjointControls":["U=theta1 gamma2,Y=theta12 gamma3:DBU=2Y,DBdaggerY=2U,pairs-2","U=i theta1 I sinx0,Y=i theta01 I cosx0:centralBcommutator0,derivativepairs1/2"],
 "expectedCounts":{"realBladeRows":16384,"omegaOddRows":8192,"wordCases":44944,"generatorWordCases":458752,"omegaWordCases":32768,"hodgeCases":16384,"complementPairRows":8192,"genericK":2208,"genericKdagger":2208,"nullK":1176,"nullKdagger":1176,"adjointSupportProbes":728,"adjointNonzeroProbes":290,"covariantAdjointControls":2,"branchContexts":8,"branchAnchorChecks":64,"gradientSlots":16,"gradientLegs":96,"sourceResidualSlots":16,"innerCoefficientSlots":16,"innerNonzeroRows":8,"actionVariationSlots":16,"actionVariationLegs":64,"branchPolynomialSlots":16,"wrongSignSlots":16,"wrongSignPolynomialSlots":16,"wrongSignNonzeroRows":8,"pointwiseJetSlots":64,"metricRows":16,"metricSlots":32,"metricNonzeroBranchDeltas":16,"backgroundJetRows":8,"backgroundJetSlots":16,"identificationRows":8,"identificationSlots":16,"masslessRows":4,"masslessSlots":8},
 "exactTolerance":0,"coreFileCount":726,
 "resources":{"estimatedCpuSeconds":15,"maximumEstimatedCpuSeconds":90,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":536870912,"maximumCoefficientProducts":50000000,"maximumSparseTensorTerms":65536,"maximumActionPolynomialDegree":3},
 "scope":{"inducedLCBackgroundClaim":false,"sourceNormalizationSelected":false,"globalAdmissibilityEstablished":false,"physicalVacuumSelected":false,"secondActionNormSelected":false,"hessianOrPropagatorComputed":false,"conditionalPointwiseMetricDensityProof":true}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","null-ideal-image-control-failed","full-adjoint-control-failed","full-stationarity-control-failed","pointwise-variation-control-failed","metric-density-control-failed","nonzero-decoy-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase609ChiralNullBranchStationarityAudit.csproj",["study"]=Root+"/STUDY.md",["control-helper"]=Root+"/NullBranchControls.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["phase593-summary"]=P593+"/output/companion_action_first_variation_audit_summary.json",["phase593-contract"]=P593+"/preregistration/contract_v1.json",["phase593-program"]=P593+"/Program.cs",["phase593-study"]=P593+"/STUDY.md",
 ["phase607-summary"]=P607+"/output/source_induced_vertical_curvature_audit_summary.json",["phase607-contract"]=P607+"/preregistration/contract_v1.json",["phase607-study"]=P607+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==609&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase593","companion-action-variation-mismatch-certified-source-choice-open"),("phase607","source-induced-vertical-curvature-controls-pass-flat-reference-not-induced")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)","(3.17)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}

var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);
bool knownAnswerPassed=true,idealPassed=true,adjointPassed=true,stationaryPassed=true,variationPassed=true,metricPassed=true,decoysPassed=true;
var masks=new HashSet<int>{0,Full};for(int length=1;length<14;length++)for(int start=0;start<14;start++){int mask=0;for(int k=0;k<length;k++)mask|=1<<((start+k)%14);masks.Add(mask);}int[] battery=masks.Order().ToArray();knownAnswerPassed&=battery.Length==184&&battery.Count(m=>Degree(m)%2==1)==98;
for(int m=0;m<=Full;m++)
{
 var x=RealBasis(0,m);counts["realBladeRows"]++;knownAnswerPassed&=HAnti(x)&&Pair(x,x)!=0;counts["hodgeCases"]++;int d=Degree(m);knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((d*(14-d)+7)%2==0?1:-1);
 for(int axis=0;axis<14;axis++){int g=1<<axis;counts["generatorWordCases"]+=2;knownAnswerPassed&=BladeSign(g,m)==WordSign(g,m)&&BladeSign(m,g)==WordSign(m,g);}counts["omegaWordCases"]+=2;knownAnswerPassed&=BladeSign(Full,m)==WordSign(Full,m)&&BladeSign(m,Full)==WordSign(m,Full);
 if(d%2==1){counts["omegaOddRows"]++;knownAnswerPassed&=HAnti(Omega(x))&&Equal(Omega(Omega(x)),x)&&Pair(Omega(x),Omega(x))==Pair(x,x)*-1&&Pair(x,Omega(x))==0;}
}
int[] wordMasks=Enumerable.Range(0,16384).Where(m=>new[]{0,1,2,12,13,14}.Contains(Degree(m))).ToArray();foreach(int a in wordMasks)foreach(int b in wordMasks){counts["wordCases"]++;knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);}
FT one=new(),two=new();for(int i=0;i<14;i++)one=Add(one,One(1<<i,1<<i,1));for(int i=0;i<14;i++)for(int j=i+1;j<14;j++)two=Add(two,One((1<<i)|(1<<j),(1<<i)|(1<<j),1));
FT background=Add(Add(One(1,3,1),One(2,6,1)),One(4,12,1));FT curvature=Add(D(background),Product(background,background));FT curvatureExpected=Add(One(3,5,2),One(6,10,2));FT v=One(2,4,1);
var covU=One(2,4,1);var covY=One(6,8,1);counts["covariantAdjointControls"]++;adjointPassed&=Equal(Covariant(background,covU),Scale(covY,2))&&Equal(CovariantAdjoint(background,covY),Scale(covU,2))&&Pair(Covariant(background,covU),covY)==-2&&Pair(covU,CovariantAdjoint(background,covY))==-2;
var derivativeU=Scale(Trig(0,true,2,0),Scalar.I);var derivativeY=Scale(Trig(0,false,3,0),Scalar.I);counts["covariantAdjointControls"]++;adjointPassed&=Pair(Covariant(background,derivativeU),derivativeY)==new Rational(1,2)&&Pair(derivativeU,CovariantAdjoint(background,derivativeY))==new Rational(1,2);
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
var imageRows=new List<object>();var rows=new List<object>();var metricRows=new List<object>();var decoyRows=new List<object>();int maximumActionDegree=0;
foreach(int h in new[]{-1,1})
{
 FT p1=ProjectTwice(one,h),p20=Scale(Omega(two),Scalar.I*-h);FT[] phi2=[p20,two];
 FT K(FT x,int slot)=>Chain(x,p1,phi2[slot],slot==0);
 FT Kad(FT x,int slot){var a=Adjoint.KAdjointLiteral(x,p1,phi2[slot],slot==0);adjointPassed&=Equal(a,Adjoint.KAdjointSimplified(x,p1,phi2[slot],slot==0));return a;}
 for(int m=0;m<=Full;m++)if(Degree(m)%2==1&&m<(Full^m))
 {var x=RealBasis(0,m);var plus=ProjectTwice(x,h);var minus=ProjectTwice(x,-h);counts["complementPairRows"]++;idealPassed&=HAnti(plus)&&HAnti(minus)&&InNull(plus,h)&&InNull(minus,-h)&&Pair(plus,plus)==0&&Pair(minus,minus)==0&&Pair(plus,minus)==Pair(x,x)*2&&Equal(Scale(ProjectTwice(plus,h),Fourier.Half),plus)&&Product(plus,plus).Count==0;}
 for(int slot=0;slot<2;slot++)foreach(int m in battery)for(int location=0;location<3;location++)
 {
  int form=new[]{3,129,384}[location],axis=new[]{0,7,13}[location];var input=RealBasis(form,m);var output=RealBasis(1<<axis,m);var k=K(input,slot);var ka=Kad(output,slot);counts["genericK"]++;counts["genericKdagger"]++;
  idealPassed&=ConstantReal(k,1)&&ConstantReal(ka,2)&&(Degree(m)%2==0?InNull(k,h)&&InNull(ka,h):Parity(k,0)&&Parity(ka,0));
  if(Degree(m)%2==1){counts["nullK"]++;counts["nullKdagger"]++;idealPassed&=K(ProjectTwice(input,h),slot).Count==0&&Kad(ProjectTwice(output,h),slot).Count==0;}
 }
 for(int slot=0;slot<2;slot++)
 {
  var adj=Kad(v,slot);var reconstructed=new FT();int support=0;var candidate=new HashSet<(int Form,int Blade)>();
  for(int a=0;a<14;a++)for(int b=a+1;b<14;b++)for(int complement=0;complement<2;complement++)
  {
   int form=(1<<a)|(1<<b),blade=form^6;if(complement==1)blade^=Full;candidate.Add((form,blade));var x=RealBasis(form,blade);Rational left=Pair(v,NaiveChain(x,p1,phi2[slot],slot==0)),right=Pair(x,adj);counts["adjointSupportProbes"]++;adjointPassed&=left==right;
   if(left!=0){support++;counts["adjointNonzeroProbes"]++;reconstructed=Add(reconstructed,Scale(x,new Scalar(left*new Rational(Pair(x,x).Denominator,Pair(x,x).Numerator),0)));}
  }
  adjointPassed&=candidate.Count==182&&adj.Keys.All(k=>candidate.Contains((k.Form,k.Blade)))&&Equal(adj,reconstructed)&&support==(slot==0?78:67)&&Adjoint.CoefficientPair(adj,adj)==(slot==0?312:268);
  imageRows.Add(new{h,slot,support,coefficientNorm=Adjoint.CoefficientPair(adj,adj).ToString(),reconstructed=true});
 }
 FT sourceExpected=Scale(ProjectTwice(Add(One(2,4,1),One(4,8,1)),h),-4);
 foreach(int kappa in new[]{-1,1})foreach(int gamma in new[]{1,2})
 {
  counts["branchContexts"]++;var state=Scale(K(curvature,0),new Scalar(new Rational(-1,kappa),0));var db=Covariant(background,state);var q=Product(state,state);var dbExpected=Scale(ProjectTwice(One(6,8,1),h),new Scalar(new Rational(8,kappa),0));
  bool[] anchors=[Equal(curvature,curvatureExpected),ConstantReal(background,1)&&ConstantReal(curvature,2),Equal(K(curvature,0),sourceExpected)&&K(curvature,1).Count==0,ConstantReal(state,1)&&InNull(state,h),Equal(db,dbExpected)&&db.Count==2&&ConstantReal(db,2),q.Count==0,Adjoint.CoefficientPair(curvature,curvature)==8&&Adjoint.CoefficientPair(sourceExpected,sourceExpected)==64,Adjoint.CoefficientPair(state,state)==64&&Pair(state,state)==0];foreach(bool pass in anchors){counts["branchAnchorChecks"]++;stationaryPassed&=pass;}
  for(int slot=0;slot<2;slot++)
  {
   var y=Kad(state,slot);FT[] legs=[K(curvature,slot),Scale(K(db,slot),Fourier.Half),Scale(CovariantAdjoint(background,y),Fourier.Half),Scale(K(q,slot),new Scalar(new Rational(gamma,3),0)),Scale(Adjoint.DQAdjoint(state,y),new Scalar(new Rational(gamma,3),0)),slot==0?Scale(state,kappa):new()];
   var gradient=legs.Aggregate(new FT(),Add);counts["gradientSlots"]++;stationaryPassed&=gradient.Count==0&&y.Count==0;
   for(int leg=0;leg<6;leg++){counts["gradientLegs"]++;stationaryPassed&=Equal(legs[leg],slot==0&&leg==0?sourceExpected:slot==0&&leg==5?Scale(sourceExpected,-1):new FT());}
   counts["sourceResidualSlots"]++;stationaryPassed&=Add(K(Add(curvature,db),slot),slot==0?Scale(state,kappa):new FT()).Count==0&&K(q,slot).Count==0;
   var inner=Product(phi2[slot],Star(db),'A');var expectedInner=slot==1?Scale(ProjectTwice(One(Full,14,Scalar.I),h),new Scalar(new Rational(16,kappa),0)):new FT();var outer=Product(p1,Star(inner),'C');counts["innerCoefficientSlots"]++;stationaryPassed&=Equal(inner,expectedInner)&&outer.Count==0&&HAnti(inner);if(slot==1){counts["innerNonzeroRows"]++;stationaryPassed&=inner.Count==2;}
   var variation=VariationLegs(state,v,new(),background,curvature,p1,phi2[slot],slot==0,gamma,kappa);counts["actionVariationSlots"]++;for(int leg=0;leg<4;leg++){counts["actionVariationLegs"]++;variationPassed&=variation[leg]==(slot==0&&leg==0?4:slot==0&&leg==3?-4:0);}variationPassed&=variation.Aggregate((Rational)0,(a,b)=>a+b)==Pair(v,gradient);
   var polynomial=ActionPolynomial([state,v],background,curvature,p1,phi2[slot],slot==0,gamma,kappa);counts["branchPolynomialSlots"]++;variationPassed&=REqual(polynomial,slot==0?[0,0,new Rational(-kappa,2)]:[0]);maximumActionDegree=System.Math.Max(maximumActionDegree,polynomial.Length-1);
   var wrong=Scale(state,-1);var wy=Kad(wrong,slot);var wrongGradient=Add(Add(K(curvature,slot),Scale(Add(K(Covariant(background,wrong),slot),CovariantAdjoint(background,wy)),Fourier.Half)),Add(Scale(Add(K(Product(wrong,wrong),slot),Adjoint.DQAdjoint(wrong,wy)),new Scalar(new Rational(gamma,3),0)),slot==0?Scale(wrong,kappa):new FT()));
   counts["wrongSignSlots"]++;decoysPassed&=Equal(wrongGradient,slot==0?Scale(sourceExpected,2):new FT())&&Pair(wrongGradient,wrongGradient)==0;
   var wrongPolynomial=ActionPolynomial([wrong,v],background,curvature,p1,phi2[slot],slot==0,gamma,kappa);counts["wrongSignPolynomialSlots"]++;decoysPassed&=REqual(wrongPolynomial,slot==0?[0,8,new Rational(-kappa,2)]:[0]);if(slot==0){counts["wrongSignNonzeroRows"]++;decoysPassed&=wrongGradient.Count==4&&Pair(v,wrongGradient)==8&&Adjoint.CoefficientPair(wrongGradient,wrongGradient)==256;decoyRows.Add(new{h,kappa,gamma,id="wrong-sign-null-action-nonzero-gradient",pair="8",coefficientNorm="256",gradient=Terms(wrongGradient)});}
   foreach(var (value,dvalue) in new[]{(v,new FT()),(One(1,3,1),new FT()),(new FT(),One(3,5,1)),(new FT(),One(6,8,1))}){counts["pointwiseJetSlots"]++;var raw=VariationLegs(state,value,dvalue,background,curvature,p1,phi2[slot],slot==0,gamma,kappa);variationPassed&=raw.Aggregate((Rational)0,(a,b)=>a+b)==0;}
  }
  foreach(int[] weights in new[]{Enumerable.Repeat(1,14).ToArray(),Enumerable.Range(0,14).Select(i=>i==0?1:0).ToArray()})
  {
   counts["metricRows"]++;FT[] deltaK=new FT[2];for(int slot=0;slot<2;slot++){var kf=MetricChain(Jet.Fixed(curvature),new(p1,Weight(p1,weights,false)),new(phi2[slot],Weight(phi2[slot],weights,false)),weights,slot==0);deltaK[slot]=kf.Delta;metricPassed&=Equal(kf.Value,K(curvature,slot));}
   var deltaState=Scale(deltaK[0],new Scalar(new Rational(-1,kappa),0));var oracle=weights.Sum()==14?Scale(sourceExpected,-1):Scale(ProjectTwice(One(2,4,1),h),4);counts["metricNonzeroBranchDeltas"]++;metricPassed&=Equal(deltaK[0],oracle)&&deltaK[1].Count==0&&deltaState.Count>0&&InNull(deltaState,h)&&ConstantReal(deltaState,1);
   for(int slot=0;slot<2;slot++)
   {counts["metricSlots"]++;var jp1=new Jet(p1,Weight(p1,weights,false));var jp2=new Jet(phi2[slot],Weight(phi2[slot],weights,false));var partial=ActionJet(Jet.Fixed(state),Jet.Fixed(background),jp1,jp2,weights,slot==0,gamma,kappa);var total=ActionJet(new(state,deltaState),Jet.Fixed(background),jp1,jp2,weights,slot==0,gamma,kappa);metricPassed&=partial.All(x=>x.Value==0&&x.Delta==0)&&total.All(x=>x.Value==0&&x.Delta==0);}
   metricRows.Add(new{h,kappa,gamma,id=weights.Sum()==14?"homothety":"axis0",deltaK=Terms(deltaK[0]),deltaS=Terms(deltaState),partialDensityDerivative="0",totalBranchDensityDerivative="0"});
  }
  counts["backgroundJetRows"]++;var jb=new Jet(background,background);var jf=Jet.Add(Jet.D(jb),Jet.Product(jb,jb));metricPassed&=Equal(jf.Value,curvature)&&Equal(jf.Delta,Scale(curvature,2));
  for(int slot=0;slot<2;slot++){counts["backgroundJetSlots"]++;var partial=ActionJet(Jet.Fixed(state),jb,Jet.Fixed(p1),Jet.Fixed(phi2[slot]),new int[14],slot==0,gamma,kappa);var total=ActionJet(new(state,Scale(state,2)),jb,Jet.Fixed(p1),Jet.Fixed(phi2[slot]),new int[14],slot==0,gamma,kappa);metricPassed&=partial.All(x=>x.Value==0&&x.Delta==0)&&total.All(x=>x.Value==0&&x.Delta==0)&&Equal(Scale(K(jf.Delta,slot),new Scalar(new Rational(-1,kappa),0)),slot==0?Scale(state,2):new FT());}
  var identification=Product(One(0,4,1),state,'C');var identificationOracle=Add(One(2,Full,new Scalar(new Rational(-8*h,kappa),0)),One(4,12,new Scalar(new Rational(8,kappa),0)));counts["identificationRows"]++;metricPassed&=Equal(identification,identificationOracle)&&ConstantReal(identification,1)&&Parity(identification,0)&&identification.Count>0;
  for(int slot=0;slot<2;slot++){counts["identificationSlots"]++;metricPassed&=VariationLegs(state,identification,new(),background,curvature,p1,phi2[slot],slot==0,gamma,kappa).All(x=>x==0);}
  rows.Add(new{h,kappa,gamma,state=Terms(state),curvature=Terms(curvature),KFB=Terms(sourceExpected),DBS=Terms(db),Q="0",KdaggerS="0",actualGradient="0",sourceCurvatureResidual="0",variationLegs=new[]{"4","0","0","-4"},actionPolynomial=new[]{"0","0",new Rational(-kappa,2).ToString()}});
 }
 foreach(int gamma in new[]{1,2})
 {counts["masslessRows"]++;for(int slot=0;slot<2;slot++){counts["masslessSlots"]++;var gradient=K(curvature,slot);var polynomial=ActionPolynomial([new FT(),v],background,curvature,p1,phi2[slot],slot==0,gamma,0);decoysPassed&=Equal(gradient,slot==0?sourceExpected:new FT())&&REqual(polynomial,slot==0?[0,4]:[0])&&(slot==1||Pair(v,gradient)==4&&Adjoint.CoefficientPair(gradient,gradient)==64);}decoyRows.Add(new{h,gamma,id="kappa0-zero-state-nonzero-source",pair="4",coefficientNorm="64"});}
}
bool countsPassed=counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resource=fx.GetProperty("resources");bool resourcesPassed=CoefficientProducts<=resource.GetProperty("maximumCoefficientProducts").GetInt64()&&LargestTensor<=resource.GetProperty("maximumSparseTensorTerms").GetInt32()&&maximumActionDegree<=resource.GetProperty("maximumActionPolynomialDegree").GetInt32();
bool controlsPassed=knownAnswerPassed&&idealPassed&&adjointPassed&&stationaryPassed&&variationPassed&&metricPassed&&decoysPassed&&countsPassed&&resourcesPassed;string verdict=!knownAnswerPassed?precedence[1]:!idealPassed?precedence[2]:!adjointPassed?precedence[3]:!stationaryPassed?precedence[4]:!variationPassed?precedence[5]:!metricPassed?precedence[6]:!decoysPassed||!countsPassed||!resourcesPassed?precedence[7]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,idealPassed,adjointPassed,stationaryPassed,variationPassed,metricPassed,decoysPassed,countsPassed,resourcesPassed,counts,coefficientProducts=CoefficientProducts,largestTensorDuringAssembly=LargestTensor,maximumActionDegree,imageRows,rows,metricRows,decoyRows,inducedLCBackgroundClaim=false,sourceNormalizationSelected=false,globalAdmissibilityEstablished=false,physicalVacuumSelected=false,secondActionNormSelected=false,hessianOrPropagatorComputed=false,conditionalPointwiseMetricDensityProof=true});

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=609,phaseId="phase609-chiral-null-branch-stationarity-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/chiral_null_branch_stationarity_audit.json",json);File.WriteAllText(Root+"/output/chiral_null_branch_stationarity_audit_summary.json",json);Console.WriteLine($"Phase609 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
