using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static Fourier;
using FT=System.Collections.Generic.Dictionary<(int Form,int Blade,int K0,int K1),Scalar>;

const string Root="studies/phase604_squared_action_stationary_equivalence_audit_001";
const string P600="studies/phase600_full_trace_adjoint_periodic_gradient_norm_audit_001";
const string P603="studies/phase603_homogeneous_invariant_stationary_family_audit_001";
const string P595="studies/phase595_invariant_tensor_dimension_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath="studies/phase599_source_registered_residual_kernel_audit_001/preregistration/core_source_manifest_v1.json";
const string ContractId="phase604-a56-squared-action-stationary-equivalence-v1";
const string Success="squared-action-stationary-controls-pass-equations-not-equivalent";
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "background":"constant full connection tensors, fixed flat geometry/reference/density, epsilon=I",
 "variables":["r","s","c","beta","gamma","kappa"],"chiralities":[-1,1],
 "coordinateBridge":"r=a-hb,s=a+hb; S=r Er+s Es, Er=(1-hOmega)Gamma1/2,Es=(1+hOmega)Gamma1/2",
 "phi1":"P Gamma1,P=1+hOmega","phi2":"(c-i h Omega)Gamma2","operator":"full literal CCA with both Hodge legs and factor1/2",
 "pairing":"real bilinear -ReTr/128 times signed exterior metric; not a source-selected positive norm",
 "gramRS":[[0,-7],[-7,0]],
 "Q":"2rs Gamma2","KQ":"312rs P Gamma1","Y":"Kdagger S=-24r Gamma2-28ihcr OmegaGamma2",
 "DQAdjointY":"624rS","firstGradient":"G=104gamma rs P Gamma1+(208gamma r+kappa)S",
 "residual":"R=beta KQ+kappa S","residualCoefficientIndependent":true,
 "firstAction":"-7rs(208gamma r+kappa)",
 "JR":"-7kappa rs(624beta r+kappa)","JG":"-7s f(r),f=r(208gamma r+kappa)(416gamma r+kappa)",
 "trueJRGradient":"beta DQ_Sdagger Kdagger R+kappa R",
 "trueJGGradient":"H_I G; H_I[V]=gamma*(K DQ_SV+DQ_Vdagger Y+DQ_Sdagger Kdagger V)/3+kappa V",
 "middleTranspose":"M_Y[V]=DQ_Vdagger Y is self-adjoint since DQ_V U=DQ_U V",
 "sourceHessianLegsInvariantV":["624(s vR+r vS) Es","624r V","624vR S"],
 "raisedJR":["kappa r(624beta r+kappa)","kappa s(1248beta r+kappa)"],
 "raisedJG":["f","s fprime"],
 "hessianJR":[["kappa(1248beta r+kappa)","0"],["1248beta kappa s","kappa(1248beta r+kappa)"]],
 "hessianJG":[["fprime","0"],["s fsecond","fprime"]],
 "branchesJG":["generic-origin","generic-first-action-branch","generic-extra-branch","gamma0-mass-only-origin","kappa0-r0-line","both-zero-all-r-s"],
 "branchesJR":["generic-origin","generic-nonzero-branch","beta0-mass-only-origin","kappa0-all-r-s","both-zero-all-r-s"],
 "branchNormalization":"simultaneous polynomial substitutions; r=-t with t stored in surviving variable r, s=0; kappa=208gamma t,416gamma t,624beta t respectively; no symbolic division or fitting",
 "genericJGHessians":["kappa^2 I","kappa^2 I","-kappa^2 I/2"],"genericJRHessians":["kappa^2 I","-kappa^2 I"],
 "degenerateHessians":"mass-only origin kappa^2 I; JG massless r0 line zero invariant Hessian; JR kappa0 all-family zero invariant Hessian; no assertion of zero ambient Hessian",
 "extraJGOriginalGradient":"rG=-kappa^2/(832gamma),sG=0; dI[Es]=7kappa^2/(832gamma)",
 "extraJGPositiveCoefficientNorm":"7*(kappa^2/(832gamma))^2; diagnostic only",
 "extraJRResidual":"rR=-kappa^2/(624beta),sR=0; B(R,Es)=7kappa^2/(624beta)",
 "masslessJRDecoy":"kappa=0,r=s=1: R=624beta Es,Pair(R,Er)=-4368beta; JR gradient0 but JG gradient nonzero for gamma!=0",
 "firstStationaryComparison":"at r=-kappa/(208gamma),s0: dJR[Es]=7kappa^3/(208gamma)*(1-3beta/gamma),dJG[Es]=0",
 "exceptionalRelationControl":"gamma=3beta gives both square gradients0 at first-action branch but JR Hessian=-kappa^2 I,JG Hessian=+kappa^2 I; not an inferred coupling relation",
 "sourceNoHalfControl":"removing half in the square doubles gradients and Hessians",
 "coefficientCounts":{"S":56,"Q":91,"KQ":28,"Y":182,"DQY":56,"G":112,"R":84,"YR":182,"YG":364,"gradJR":112,"gradJG":168,"I":2,"JR":2,"JG":3,"HJRI":[84,56],"HJGI":[140,84]},
 "expectedCounts":{"wordCases":44944,"hodgeCases":16384,"polynomialControls":8,"gramEntries":8,"chiralityRows":2,"sourceIdentities":30,"scalarIdentities":6,"retainedCAdjoints":6,"adjointCalls":12,"firstActionDerivatives":4,"firstHessianColumns":4,"firstHessianLegs":12,"middleTransposePairings":4,"residualTransposePairings":4,"gradientTransposePairings":4,"squareDerivatives":8,"squareHessianColumns":8,"squareHessianBilinears":16,"sourceFactorControls":24,"stationaryRows":22,"genericRootCertificates":10,"decoyRows":10},
 "exactTolerance":0,"coreFileCount":726,
 "resources":{"estimatedCpuSeconds":15,"maximumEstimatedCpuSeconds":90,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":536870912,"maximumFinalTensorCoefficients":2048,"maximumScalarMonomials":256,"maximumTotalDegree":8,"maximumSparseTensorTerms":65536,"maximumCoefficientProducts":20000000},
 "physicalVacuumSelected":false,"sourceNormSelected":false,"physicalScaleSelected":false
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","full-square-transpose-control-failed","independent-square-derivative-control-failed","stationary-branch-control-failed","nonzero-equation-control-failed",Success];
var paths=new Dictionary<string,string>{
 ["program"]=Root+"/Program.cs",["project"]=Root+"/Phase604SquaredActionStationaryEquivalenceAudit.csproj",["study"]=Root+"/STUDY.md",["polynomial-helper"]=Root+"/SquaredPolynomial.cs",
 ["arithmetic-helper"]=P600+"/ExactArithmetic.cs",["fourier-helper"]=P600+"/FourierTensor.cs",["adjoint-helper"]=P600+"/TraceAdjoint.cs",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["phase600-summary"]=P600+"/output/full_trace_adjoint_periodic_gradient_norm_audit_summary.json",["phase600-contract"]=P600+"/preregistration/contract_v1.json",["phase600-program"]=P600+"/Program.cs",
 ["phase603-summary"]=P603+"/output/homogeneous_invariant_stationary_family_audit_summary.json",["phase603-contract"]=P603+"/preregistration/contract_v1.json",["phase603-program"]=P603+"/Program.cs",["phase603-polynomial"]=P603+"/InvariantPolynomial.cs",
 ["phase595-summary"]=P595+"/output/invariant_tensor_dimension_audit_summary.json",["phase595-contract"]=P595+"/preregistration/contract_v1.json",["phase595-study"]=P595+"/STUDY.md",
 ["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var cd=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=cd.RootElement.Clone();contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==604&&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&contract.GetProperty("deterministicZeroSampling").GetBoolean()&&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0&&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))&&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(q=>q.GetString()).SequenceEqual(precedence)&&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
 bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(q=>new Binding(q.GetProperty("id").GetString()!,q.GetProperty("path").GetString()!,q.GetProperty("sha256").GetString()!)).ToArray();exactBindingsValid=bindings.Length==paths.Count&&bindings.Select(q=>q.id).Distinct().Count()==paths.Count&&bindings.Select(q=>q.path).Distinct().Count()==paths.Count&&bindings.All(q=>paths.TryGetValue(q.id,out var p)&&p==q.path&&q.hashMatches);
 if(contractValid&&exactBindingsValid){using var md=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var m=md.RootElement;var live=CorePaths();coreFileCount=live.Length;var entries=m.GetProperty("files").EnumerateArray().ToArray();coreSourceTreeValid=m.GetProperty("schemaVersion").GetInt32()==1&&m.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"&&live.Length==726&&entries.Select(q=>q.GetProperty("path").GetString()).SequenceEqual(live)&&entries.All(q=>Sha(q.GetProperty("path").GetString()!)==q.GetProperty("sha256").GetString())&&Hash(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")))==m.GetProperty("treeSha256").GetString();}
 if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false});return;}
 foreach(var (id,terminal) in new[]{("phase600","full-trace-adjoint-periodic-controls-pass-declared-norm-mismatch"),("phase603","homogeneous-invariant-stationary-family-controls-pass-scale-unselected"),("phase595","invariant-tensor-dimensions-two-certified-source-choice-open")})
 {using var ud=JsonDocument.Parse(File.ReadAllBytes(paths[id+"-summary"]));var q=ud.RootElement;bool ok=q.GetProperty("auditPassed").GetBoolean()&&q.GetProperty("contractValid").GetBoolean()&&q.GetProperty("exactBindingsValid").GetBoolean()&&q.GetProperty("coreSourceTreeValid").GetBoolean()&&q.GetProperty("contractSha256").GetString()==Sha(paths[id+"-contract"])&&q.GetProperty("verdictKind").GetString()==terminal&&q.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&q.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()&&q.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>q.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)&&q.GetProperty("externalReviewPending").GetBoolean()&&q.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;if(!ok){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstream=id});return;}}
 string source=File.ReadAllText(paths["primary-source"]);if(!new[]{"(9.3)","(9.4)","(9.7)","(9.11)"}.All(source.Contains)){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,sourceAnchors=false});return;}
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}



var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expectedCounts");var counts=expected.EnumerateObject().ToDictionary(q=>q.Name,_=>0);
Poly r=Poly.Var(0),s=Poly.Var(1),c=Poly.Var(2),beta=Poly.Var(3),gamma=Poly.Var(4),kappa=Poly.Var(5);Poly[] variables=[r,s,c,beta,gamma,kappa];
bool knownAnswerPassed=true,transposePassed=true,derivativesPassed=true,stationaryPassed=true,decoysPassed=true,resourcesPassed=true;
int[] masks=Enumerable.Range(0,16384).Where(m=>new[]{0,1,2,12,13,14}.Contains(Degree(m))).ToArray();
foreach(int a in masks)foreach(int b in masks){counts["wordCases"]++;knownAnswerPassed&=BladeSign(a,b)==WordSign(a,b);}
for(int m=0;m<=Full;m++){counts["hodgeCases"]++;int p=Degree(m);knownAnswerPassed&=HodgeSign(m)*HodgeSign(Full^m)==((p*(14-p)+7)%2==0?1:-1);}
bool[] polynomialKnown=[((r+s)*(r-s)).Same(r*r-s*s),(r*r*s).Derivative(0).Same((r*s).Scale(2)),(r*s).Substitute([r+s,r-s,c,beta,gamma,kappa]).Same(r*r-s*s),(r-r).Zero,(r*r*s+r*s*s).Derivative(0).Derivative(1).Same((r+s).Scale(2)),(r*s*c*beta*gamma*kappa).Derivative(5).Same(r*s*c*beta*gamma),(beta*beta*gamma).Derivative(3).Same((beta*gamma).Scale(2)),(c*beta+gamma*kappa).Substitute([r,s,kappa,gamma,beta,c]).Same(kappa*gamma+beta*c)];
foreach(bool pass in polynomialKnown){counts["polynomialControls"]++;knownAnswerPassed&=pass;}
FT one=new(),two=new();for(int i=0;i<14;i++)one=Add(one,One(1<<i,1<<i,1));for(int i=0;i<14;i++)for(int j=i+1;j<14;j++){int mask=(1<<i)|(1<<j);two=Add(two,One(mask,mask,1));}
knownAnswerPassed&=Pair(one,one)==-14&&Pair(Omega(one),Omega(one))==14&&Pair(one,Omega(one))==0&&Pair(two,two)==91;
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,counts});return;}
var rows=new List<object>();var branches=new List<object>();var decoys=new List<object>();
int maximumFinalTensorCoefficients=0,maximumScalarMonomials=0,maximumTotalDegree=0;
void TrackTensor(TensorPoly t){maximumFinalTensorCoefficients=System.Math.Max(maximumFinalTensorCoefficients,t.CoefficientCount);foreach(var m in t.Terms.Keys)maximumTotalDegree=System.Math.Max(maximumTotalDegree,m.Degree);}
void TrackScalar(Poly p){maximumScalarMonomials=System.Math.Max(maximumScalarMonomials,p.Terms.Count);foreach(var m in p.Terms.Keys)maximumTotalDegree=System.Math.Max(maximumTotalDegree,m.Degree);}
foreach(int h in fx.GetProperty("chiralities").EnumerateArray().Select(q=>q.GetInt32()))
{
 counts["chiralityRows"]++;FT p1=Add(one,Scale(Omega(one),h)),p20=Scale(Omega(two),Scalar.I*-h);
 var er=TensorPoly.From(Scale(Add(one,Scale(Omega(one),-h)),new Scalar(new Rational(1,2),0)));var es=TensorPoly.From(Scale(p1,new Scalar(new Rational(1,2),0)));TensorPoly[] basis=[er,es];
 var z=TensorPoly.From(two);var oz=TensorPoly.From(Omega(two));var pp=TensorPoly.From(p1);
 for(int i=0;i<2;i++)for(int j=0;j<2;j++){counts["gramEntries"]++;knownAnswerPassed&=TensorPoly.Pair(basis[i],basis[j]).Same(i==j?0:-7);}
 TensorPoly K(TensorPoly t)=>t.Map(v=>Chain(v,p1,p20,true))+c*t.Map(v=>Chain(v,p1,two,false));
 TensorPoly Kad(TensorPoly t)
 {counts["adjointCalls"]++;var literal=t.Map(v=>Adjoint.KAdjointLiteral(v,p1,p20,true))+c*t.Map(v=>Adjoint.KAdjointLiteral(v,p1,two,false));var simple=t.Map(v=>Adjoint.KAdjointSimplified(v,p1,p20,true))+c*t.Map(v=>Adjoint.KAdjointSimplified(v,p1,two,false));transposePassed&=literal.Same(simple);TrackTensor(literal);return literal;}
 TensorPoly W(TensorPoly a,TensorPoly b)=>TensorPoly.Bilinear(a,b,(u,v)=>Product(u,v));
 TensorPoly DQ(TensorPoly a,TensorPoly b)=>W(a,b)+W(b,a);
 TensorPoly Dqa(TensorPoly a,TensorPoly b)=>TensorPoly.Bilinear(a,b,Adjoint.DQAdjoint);
 TensorPoly Coordinates(Poly vr,Poly vs)=>vr*er+vs*es;
 var state=Coordinates(r,s);var q=W(state,state);var kq=K(q);var y=Kad(state);var dqy=Dqa(state,y);
 var gradient=(gamma*(kq+dqy)).Scale(new Rational(1,3))+kappa*state;var residual=beta*kq+kappa*state;
 var yr=Kad(residual);var yg=Kad(gradient);var yp=Kad(pp);var jrAdjoint=Dqa(state,yr);
 var gradJR=beta*jrAdjoint+kappa*residual;
 TensorPoly[] gLegs=[K(DQ(state,gradient)),Dqa(gradient,y),Dqa(state,yg)];
 var gradJG=(gamma*(gLegs[0]+gLegs[1]+gLegs[2])).Scale(new Rational(1,3))+kappa*gradient;
 // Reverse DG: first/third operators swap; the middle operator is its own real transpose.
 var reverseDG=(gamma*(Dqa(state,yg)+Dqa(gradient,y)+K(DQ(state,gradient)))).Scale(new Rational(1,3))+kappa*gradient;
 Poly aa=gamma.Scale(208),bb=beta.Scale(624),gr=r*(aa*r+kappa),gs=s*(aa*r.Scale(2)+kappa),rr=kappa*r,rs=s*(bb*r+kappa);
 Poly f=r*(aa*r+kappa)*(aa*r.Scale(2)+kappa),fp=f.Derivative(0),fpp=fp.Derivative(0);
 Poly[] rCoords=[kappa*r*(bb*r+kappa),kappa*s*(bb*r.Scale(2)+kappa)],gCoords=[f,s*fp];
 var yExpected=r.Scale(-24)*z+(c*r).Scale(-28*h)*oz.Map(t=>Scale(t,Scalar.I));
 var ygExpected=gr.Scale(-24)*z+(c*gr).Scale(-28*h)*oz.Map(t=>Scale(t,Scalar.I));
 bool[] sourceChecks=[q.Same((r*s).Scale(2)*z),kq.Same((r*s).Scale(312)*pp),y.Same(yExpected),dqy.Same(r.Scale(624)*state),gradient.Same(Coordinates(gr,gs)),residual.Same(Coordinates(rr,rs)),yp.Zero,yr.Same(kappa*yExpected),yg.Same(ygExpected),jrAdjoint.Same((kappa*r).Scale(624)*state),gradJR.Same(Coordinates(rCoords[0],rCoords[1])),gLegs[0].Same((s*gr+r*gs).Scale(624)*es),gLegs[1].Same(r.Scale(624)*Coordinates(gr,gs)),gLegs[2].Same(gr.Scale(624)*state),gradJG.Same(Coordinates(gCoords[0],gCoords[1]))];
 foreach(bool pass in sourceChecks){counts["sourceIdentities"]++;transposePassed&=pass;}
 transposePassed&=reverseDG.Same(gradJG)&&state.TypedAnti(1)&&q.TypedAnti(2)&&y.TypedAnti(2)&&new[]{gradient,residual,gradJR,gradJG}.All(t=>t.TypedAnti(1)&&t.Terms.Keys.All(m=>m.C==0));
 foreach(var retained in new[]{y,yr,yg}){counts["retainedCAdjoints"]++;transposePassed&=retained.Terms.Any(t=>t.Key.C==1&&t.Value.Count>0);}
 Poly actionI=(gamma*TensorPoly.Pair(state,kq)).Scale(new Rational(1,3))+(kappa*TensorPoly.Pair(state,state)).Scale(new Rational(1,2));
 Poly actionJR=TensorPoly.Pair(residual,residual).Scale(new Rational(1,2)),actionJG=TensorPoly.Pair(gradient,gradient).Scale(new Rational(1,2));
 Poly[] actions=[actionJR,actionJG];TensorPoly[] equations=[gradJR,gradJG];
 bool[] scalarChecks=[actionI.Same((r*s*(aa*r+kappa)).Scale(-7)),actionJR.Same((kappa*r*s*(bb*r+kappa)).Scale(-7)),actionJG.Same((s*f).Scale(-7))];
 foreach(bool pass in scalarChecks){counts["scalarIdentities"]++;derivativesPassed&=pass;}
 var cc=fx.GetProperty("coefficientCounts");var countedTensors=new Dictionary<string,TensorPoly>{{"S",state},{"Q",q},{"KQ",kq},{"Y",y},{"DQY",dqy},{"G",gradient},{"R",residual},{"YR",yr},{"YG",yg},{"gradJR",gradJR},{"gradJG",gradJG}};
 foreach(var item in countedTensors){TrackTensor(item.Value);transposePassed&=item.Value.CoefficientCount==cc.GetProperty(item.Key).GetInt32();}
 foreach(var item in new[]{("I",actionI),("JR",actionJR),("JG",actionJG)}){TrackScalar(item.Item2);derivativesPassed&=item.Item2.Terms.Count==cc.GetProperty(item.Item1).GetInt32();}
 TensorPoly[] hI=new TensorPoly[2];
 for(int i=0;i<2;i++)
 {
  counts["firstActionDerivatives"]++;derivativesPassed&=TensorPoly.Pair(basis[i],gradient).Same(actionI.Derivative(i));
  var v=basis[i];var yv=Kad(v);TensorPoly[] legs=[K(DQ(state,v)),Dqa(v,y),Dqa(state,yv)];Poly vr=i==0?1:0,vs=i==1?1:0;
  TensorPoly[] expectedLegs=[(s*vr+r*vs).Scale(624)*es,r.Scale(624)*v,vr.Scale(624)*state];
  for(int leg=0;leg<3;leg++){counts["firstHessianLegs"]++;transposePassed&=legs[leg].Same(expectedLegs[leg]);TrackTensor(legs[leg]);}
  hI[i]=(gamma*(legs[0]+legs[1]+legs[2])).Scale(new Rational(1,3))+kappa*v;counts["firstHessianColumns"]++;
  transposePassed&=hI[i].Same(gradient.Derivative(i))&&hI[i].Same(Coordinates(gr.Derivative(i),gs.Derivative(i)));
  counts["middleTransposePairings"]++;transposePassed&=TensorPoly.Pair(v,gLegs[1]).Same(TensorPoly.Pair(gradient,legs[1]));
  counts["residualTransposePairings"]++;transposePassed&=TensorPoly.Pair(v,gradJR).Same(TensorPoly.Pair(beta*K(DQ(state,v))+kappa*v,residual));
  counts["gradientTransposePairings"]++;transposePassed&=TensorPoly.Pair(v,gradJG).Same(TensorPoly.Pair(hI[i],gradient));
 }
 // A derivative of the full previously calculated tensor identity gives H_I acting on G.
 transposePassed&=gradJG.Same(gr*gradient.Derivative(0)+gs*gradient.Derivative(1));
 Poly[][][] squareHessians=new Poly[2][][];var fullSquareColumns=new TensorPoly[2][];
 for(int square=0;square<2;square++)
 {
  fullSquareColumns[square]=new TensorPoly[2];squareHessians[square]=ZeroMatrix();
  for(int i=0;i<2;i++)
  {
   counts["squareDerivatives"]++;derivativesPassed&=TensorPoly.Pair(basis[i],equations[square]).Same(actions[square].Derivative(i));
   counts["sourceFactorControls"]++;derivativesPassed&=actions[square].Scale(2).Derivative(i).Same(TensorPoly.Pair(basis[i],equations[square].Scale(2)));
   var column=equations[square].Derivative(i);fullSquareColumns[square][i]=column;counts["squareHessianColumns"]++;TrackTensor(column);
   Poly raisedR=TensorPoly.Pair(es,column).Scale(new Rational(-1,7)),raisedS=TensorPoly.Pair(er,column).Scale(new Rational(-1,7));squareHessians[square][0][i]=raisedR;squareHessians[square][1][i]=raisedS;
   derivativesPassed&=column.Same(Coordinates(raisedR,raisedS))&&column.CoefficientCount==cc.GetProperty(square==0?"HJRI":"HJGI")[i].GetInt32();
   for(int j=0;j<2;j++){counts["squareHessianBilinears"]++;derivativesPassed&=TensorPoly.Pair(basis[j],column).Same(actions[square].Derivative(i).Derivative(j));counts["sourceFactorControls"]++;derivativesPassed&=TensorPoly.Pair(basis[j],column.Scale(2)).Same(actions[square].Scale(2).Derivative(i).Derivative(j));}
  }
 }
 Poly dr=kappa*(bb*r.Scale(2)+kappa);Poly[][] expectedJR=[[dr,0],[(beta*kappa*s).Scale(1248),dr]],expectedJG=[[fp,0],[s*fpp,fp]];
 derivativesPassed&=SameMatrix(squareHessians[0],expectedJR)&&SameMatrix(squareHessians[1],expectedJG);
 Poly[][] subsJG=[[0,0,c,beta,gamma,kappa],[r.Scale(-1),0,c,beta,gamma,(gamma*r).Scale(208)],[r.Scale(-1),0,c,beta,gamma,(gamma*r).Scale(416)],[0,0,c,beta,0,kappa],[0,s,c,beta,gamma,0],[r,s,c,beta,0,0]];
 Poly[][] subsJR=[[0,0,c,beta,gamma,kappa],[r.Scale(-1),0,c,beta,gamma,(beta*r).Scale(624)],[0,0,c,0,gamma,kappa],[r,s,c,beta,gamma,0],[r,s,c,0,gamma,0]];
 for(int square=0;square<2;square++)
 {
  Poly[][] substitutions=square==0?subsJR:subsJG;var names=fx.GetProperty(square==0?"branchesJR":"branchesJG").EnumerateArray().Select(v=>v.GetString()!).ToArray();
  for(int b=0;b<substitutions.Length;b++)
  {
   counts["stationaryRows"]++;var sub=substitutions[b];var eq=equations[square].Substitute(sub);var jac=SubMatrix(squareHessians[square],sub);var act=actions[square].Substitute(sub);var ks=kappa.Substitute(sub);
   Poly diagonal=square==0?b switch{0 or 2=>ks*ks,1=>(ks*ks).Scale(-1),_=>0}:b switch{0 or 1 or 3=>ks*ks,2=>(ks*ks).Scale(new Rational(-1,2)),_=>0};
   bool pass=eq.Zero&&act.Zero&&SameMatrix(jac,Diagonal(diagonal));
   if(b<(square==0?2:3)){counts["genericRootCertificates"]++;pass&=!diagonal.Zero;}
   stationaryPassed&=pass;branches.Add(new{h,action=square==0?"JR":"JG",branch=names[b],passed=pass,gradient=eq.Text(),actionValue=act.Text(),invariantHessian=MatrixText(jac),ambientHessianZeroClaim=false});
   TrackTensor(eq);TrackScalar(act);
  }
 }
 // Nonzero original equations are checked as full tensors AND by nonzero directional pairings.
 var extraG=subsJG[2];var originalAtExtra=gradient.Substitute(extraG);Poly expectedGR=(gamma*r*r).Scale(-208),extraVariation=actionI.Derivative(1).Substitute(extraG);var positiveExtra=TensorPoly.PairWith(originalAtExtra,originalAtExtra,Adjoint.CoefficientPair);
 bool extraGPass=!originalAtExtra.Zero&&originalAtExtra.Same(expectedGR*er)&&extraVariation.Same(expectedGR.Scale(-7))&&!extraVariation.Zero&&TensorPoly.Pair(originalAtExtra,originalAtExtra).Zero&&positiveExtra.Same((expectedGR*expectedGR).Scale(7))&&!positiveExtra.Zero&&gradJG.Substitute(extraG).Zero;
 counts["decoyRows"]++;decoysPassed&=extraGPass;decoys.Add(new{h,id="JG-extra-stationary-original-G-nonzero",passed=extraGPass,original=originalAtExtra.Text(),directionalVariation=extraVariation.Text(),positiveCoefficientNorm=positiveExtra.Text()});
 var extraR=subsJR[1];var residualAtExtra=residual.Substitute(extraR);Poly expectedRR=(beta*r*r).Scale(-624),residualWitness=TensorPoly.Pair(es,residualAtExtra);var positiveR=TensorPoly.PairWith(residualAtExtra,residualAtExtra,Adjoint.CoefficientPair);
 bool extraRPass=!residualAtExtra.Zero&&residualAtExtra.Same(expectedRR*er)&&residualWitness.Same(expectedRR.Scale(-7))&&!residualWitness.Zero&&TensorPoly.Pair(residualAtExtra,residualAtExtra).Zero&&positiveR.Same((expectedRR*expectedRR).Scale(7))&&!positiveR.Zero&&gradJR.Substitute(extraR).Zero;
 counts["decoyRows"]++;decoysPassed&=extraRPass;decoys.Add(new{h,id="JR-extra-stationary-residual-nonzero",passed=extraRPass,residual=residualAtExtra.Text(),directionalPair=residualWitness.Text(),positiveCoefficientNorm=positiveR.Text()});
 Poly[] massless=[1,1,c,beta,gamma,0];var masslessR=residual.Substitute(massless);var masslessJG=gradJG.Substitute(massless);Poly masslessWitness=TensorPoly.Pair(er,masslessR);
 bool masslessPass=masslessR.Same(beta.Scale(624)*es)&&masslessWitness.Same(beta.Scale(-4368))&&!masslessR.Zero&&gradJR.Substitute(massless).Zero&&!masslessJG.Zero&&masslessJG.Same(Coordinates((aa*aa).Scale(2),(aa*aa).Scale(6)));
 counts["decoyRows"]++;decoysPassed&=masslessPass;decoys.Add(new{h,id="massless-JR-flat-but-JG-not-flat",passed=masslessPass,residual=masslessR.Text(),JRgradient=gradJR.Substitute(massless).Text(),JGgradient=masslessJG.Text(),residualPair=masslessWitness.Text()});
 var firstStationary=subsJG[1];Poly comparison=actionJR.Derivative(1).Substitute(firstStationary),comparisonOracle=(gamma*(gamma-beta.Scale(3))*r*r*r).Scale(302848);var hjrFirst=SubMatrix(squareHessians[0],firstStationary);var hjgFirst=SubMatrix(squareHessians[1],firstStationary);
 bool comparisonPass=gradient.Substitute(firstStationary).Zero&&gradJG.Substitute(firstStationary).Zero&&comparison.Same(comparisonOracle)&&!comparison.Zero&&SameMatrix(hjrFirst,Diagonal((gamma*(gamma-beta.Scale(6))*r*r).Scale(43264)))&&SameMatrix(hjgFirst,Diagonal((gamma*gamma*r*r).Scale(43264)));
 counts["decoyRows"]++;decoysPassed&=comparisonPass;decoys.Add(new{h,id="first-action-branch-squares-generically-disagree",passed=comparisonPass,JRvariation=comparison.Text(),JRHessian=MatrixText(hjrFirst),JGHessian=MatrixText(hjgFirst)});
 Poly[] exceptional=[r.Scale(-1),0,c,beta,beta.Scale(3),(beta*r).Scale(624)];Poly exk=(beta*r).Scale(624);var exR=SubMatrix(squareHessians[0],exceptional);var exG=SubMatrix(squareHessians[1],exceptional);
 bool exceptionalPass=gradient.Substitute(exceptional).Zero&&gradJR.Substitute(exceptional).Zero&&gradJG.Substitute(exceptional).Zero&&SameMatrix(exR,Diagonal((exk*exk).Scale(-1)))&&SameMatrix(exG,Diagonal(exk*exk))&&!SameMatrix(exR,exG);
 counts["decoyRows"]++;decoysPassed&=exceptionalPass;decoys.Add(new{h,id="explicit-exceptional-relation-opposite-Hessians",passed=exceptionalPass,JRHessian=MatrixText(exR),JGHessian=MatrixText(exG),couplingRelationInferred=false});
 foreach(var t in new[]{originalAtExtra,residualAtExtra,masslessR,masslessJG})TrackTensor(t);foreach(var scalar in new[]{positiveExtra,positiveR,comparison,extraVariation,masslessWitness})TrackScalar(scalar);
 rows.Add(new{h,passed=sourceChecks.All(v=>v)&&scalarChecks.All(v=>v),state=state.Text(),Q=q.Text(),KQ=kq.Text(),Y=y.Text(),YR=yr.Text(),YG=yg.Text(),firstGradient=gradient.Text(),residual=residual.Text(),JR=actionJR.Text(),JG=actionJG.Text(),gradJR=gradJR.Text(),gradJG=gradJG.Text(),actualHessianOnG=gLegs.Select(t=>t.Text()).ToArray(),firstHessianColumns=hI.Select(t=>t.Text()).ToArray(),squareHessianColumns=fullSquareColumns.Select(cols=>cols.Select(t=>t.Text()).ToArray()).ToArray(),JRHessian=MatrixText(squareHessians[0]),JGHessian=MatrixText(squareHessians[1])});
}
bool countsPassed=counts.Count==expected.EnumerateObject().Count()&&counts.All(q=>q.Value==expected.GetProperty(q.Key).GetInt32());var resource=fx.GetProperty("resources");
resourcesPassed&=maximumFinalTensorCoefficients<=resource.GetProperty("maximumFinalTensorCoefficients").GetInt32()&&maximumScalarMonomials<=resource.GetProperty("maximumScalarMonomials").GetInt32()&&maximumTotalDegree<=resource.GetProperty("maximumTotalDegree").GetInt32()&&LargestTensor<=resource.GetProperty("maximumSparseTensorTerms").GetInt32()&&CoefficientProducts<=resource.GetProperty("maximumCoefficientProducts").GetInt64();
bool controlsPassed=knownAnswerPassed&&transposePassed&&derivativesPassed&&stationaryPassed&&decoysPassed&&countsPassed&&resourcesPassed;
string verdict=!knownAnswerPassed?precedence[1]:!transposePassed?precedence[2]:!derivativesPassed?precedence[3]:!stationaryPassed?precedence[4]:!decoysPassed||!countsPassed||!resourcesPassed?precedence[5]:Success;
Emit(verdict,new{knownAnswerPassed,controlsPassed,transposePassed,derivativesPassed,stationaryPassed,decoysPassed,countsPassed,resourcesPassed,counts,maximumFinalTensorCoefficients,maximumScalarMonomials,maximumTotalDegree,coefficientProducts=CoefficientProducts,largestTensorDuringAssembly=LargestTensor,rows,branches,decoys,ambientConstantGradientsChecked=true,physicalVacuumSelected=false,sourceNormSelected=false,physicalScaleSelected=false});

static Poly[][] ZeroMatrix()=>new Poly[][]{[0,0],[0,0]};
static Poly[][] Diagonal(Poly p)=>new Poly[][]{[p,0],[0,p]};
static bool SameMatrix(Poly[][] a,Poly[][] b)=>a.SelectMany((row,i)=>row.Select((v,j)=>v.Same(b[i][j]))).All(v=>v);
static Poly[][] SubMatrix(Poly[][] a,Poly[] values)=>a.Select(row=>row.Select(p=>p.Substitute(values)).ToArray()).ToArray();
static object[][][] MatrixText(Poly[][] a)=>a.Select(row=>row.Select(p=>p.Text()).ToArray()).ToArray();

void Emit(string terminal,object evidence)
{var result=new{schemaVersion=1,phase=604,phaseId="phase604-squared-action-stationary-equivalence-audit",contractId=ContractId,contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(q=>q.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(q=>q.path).Distinct().Count(),bindings,verdictKind=terminal,terminalStatus=terminal,auditPassed=terminal==Success,evidence,deterministic=true,authorityFirewalls=firewalls.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/squared_action_stationary_equivalence_audit.json",json);File.WriteAllText(Root+"/output/squared_action_stationary_equivalence_audit_summary.json",json);Console.WriteLine($"Phase604 verdict: {terminal}");if(terminal!=Success)Environment.ExitCode=1;}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
