using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using static Algebra;
using static ExactTools;
using PT=System.Collections.Generic.Dictionary<(int Form,int Blade),Polynomial>;

const string Root="studies/phase594_actual_gradient_reciprocity_audit_001";
const string P592="studies/phase592_companion_tensor_chirality_audit_001";
const string P593="studies/phase593_companion_action_first_variation_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=P592+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase594-a50-actual-gradient-reciprocity-v1";
const string Success="actual-gradient-controls-pass-curvature-only-force-rejected";
const int Omega=(1<<14)-1;
const string FixtureJson="""
{
 "dimension":14,"signature":[1,1,1,1,1,1,1,-1,-1,-1,-1,-1,-1,-1],
 "background":"fixed flat constant unit-volume (7,7) fourteen-torus; epsilon=I; B=0; dT=0",
 "operator":"full CCA: K(F)=[Phi1 wedge star(F)]_C-(1/2)star([Phi1 wedge star([Phi2 wedge star(F)]_A)]_C)",
 "phi1":"(a+b Omega)gamma","phi2":"(c+i d Omega)Gamma2","formalCSlots":["1","c"],
 "pairing":"-ReTr/128 with signed exterior metric; finite Gram pullback after full K",
 "operatorRows":[{"id":"canonical","a":1,"b":0,"d":0},{"id":"chiral-minus","a":1,"b":-1,"d":1},{"id":"chiral-plus","a":1,"b":1,"d":-1}],
 "carriers":[
  {"id":"curvature-stationary","operatorIds":["canonical","chiral-minus","chiral-plus"],"gramDiagonal":[1,1,-1],"curvature":"2xy theta01 Gamma02","curvatureGram":1,"qRow":["y","x","0"],"desiredCovector":["0","0","4xy"],"desiredCurl":["0","4y","4x"],"collision":[[1,1,0],[1,1,1]]},
  {"id":"commuting-background","operatorIds":["chiral-minus","chiral-plus"],"gramDiagonal":[1,-1,-1],"curvature":"-2yz theta12 Omega gamma1","curvatureGram":1,"qRow":["0","-z","-y"],"desiredCovector":["4hyz","0","0"],"desiredCurl":["-4hz","-4hy","0"],"collision":[[0,1,1],[1,1,1]]}
 ],
 "gammaEncodings":[1,2],"actionPrefactor":"gamma/3 with Q=T wedge T fixed",
 "gradient":"gamma/3*(K_E Q+2 Q_T^dagger K_E^dagger T)",
 "covector":"4gamma/3*(yz,xz,xy), with extra h on second carrier",
 "curlOrder":["xy","xz","yz"],"curlConvention":"partial_i f_j-partial_j f_i, i<j",
 "quadraticMonomials":[[2,0,0],[1,1,0],[1,0,1],[0,2,0],[0,1,1],[0,0,2]],
 "symmetricSquare":"diagonal ei tensor ei; off-diagonal ei tensor ej+ej tensor ei, without half",
 "kernelColumnWitness":2,"sourceQRank":1,"sourceQKernelDimension":5,
 "projection":[["1/2","1/2","0"],["1/2","1/2","0"],["0","0","0"]],
 "projectionBasis":"theta01 Gamma02,theta02 Gamma01,theta12 Omega gamma1",
 "projectionExpected":"first desired covector halved; second curvature and projected force zero",
 "curlPoints":[[0,0,0],[1,1,1]],"massKappa":[-1,0,1],
 "chernSimons":{"nu11Mask":16376,"q":["2xy","-2xz","2yz"],"gramDiagonal":[1,1,1],"K":[[0,0,1],[0,-1,0],[1,0,0]],"action":"2xyz","gradient":["2yz","2xz","2xy"],"qRank":3,"qKernelDimension":3},
 "expectedCounts":{"carrierRows":5,"coefficientRows":10,"gradientRows":20,"gradientComponents":60,"jacobianEntries":180,"curlComponents":60,"desiredCurlRejections":10,"zeroCRows":10,"unweightedJacobianRejections":10,"linearFactorizationRejections":10,"nonlinearFactorizationRejections":10,"kAdjointChecks":30,"qAdjointChecks":30,"sourceKernelVectors":25,"projectionRows":10,"massRows":15,"csRows":2},
 "knownAnswer":{"maskCount":16,"wordCases":256,"hodgeCases":16384},"exactTolerance":0,"coreFileCount":726,
 "resource":{"estimatedCpuSeconds":5,"maximumEstimatedCpuSeconds":20,"estimatedPeakBytes":67108864,"maximumEstimatedPeakBytes":134217728}
}
""";
string[] firewalls=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
 "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
 "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-control-failed","carrier-adjoint-control-failed","gradient-reciprocity-control-failed","factorization-projection-control-failed","positive-control-failed",Success];
var paths=new Dictionary<string,string>{["program"]=Root+"/Program.cs",["polynomial-helper"]=Root+"/PolynomialTensor.cs",
 ["project"]=Root+"/Phase594ActualGradientReciprocityAudit.csproj",["study"]=Root+"/STUDY.md",
 ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
 ["phase593-summary"]=P593+"/output/companion_action_first_variation_audit_summary.json",["phase593-contract"]=P593+"/preregistration/contract_v1.json",
 ["phase593-program"]=P593+"/Program.cs",["phase593-study"]=P593+"/STUDY.md",["algebra-helper"]=P592+"/ExactAlgebra.cs",
 ["core-source-manifest"]=ManifestPath,["build-props"]="Directory.Build.props"};
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;int coreFileCount=0;Binding[] bindings=[];JsonElement contract=default;
try
{
 using var doc=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=doc.RootElement.Clone();
 contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==594
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
using(var ud=JsonDocument.Parse(File.ReadAllBytes(paths["phase593-summary"])))
{
 var u=ud.RootElement;bool valid=u.GetProperty("auditPassed").GetBoolean()&&u.GetProperty("contractValid").GetBoolean()
  &&u.GetProperty("exactBindingsValid").GetBoolean()&&u.GetProperty("coreSourceTreeValid").GetBoolean()
  &&u.GetProperty("verdictKind").GetString()=="companion-action-variation-mismatch-certified-source-choice-open"
  &&u.GetProperty("contractSha256").GetString()==Sha(paths["phase593-contract"])
  &&u.GetProperty("evidence").GetProperty("knownAnswerPassed").GetBoolean()&&u.GetProperty("evidence").GetProperty("controlsPassed").GetBoolean()
  &&u.GetProperty("authorityFirewalls").EnumerateObject().Count()==14&&firewalls.All(k=>u.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)
  &&u.GetProperty("externalReviewPending").GetBoolean()&&u.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
 string source=File.ReadAllText(paths["primary-source"]);valid&=new[]{"(8.5)","(9.3)","(9.4)","(9.7)","(12.26)","(12.27)"}.All(source.Contains);
 if(!valid){Emit(precedence[0],new{knownAnswerPassed=false,controlsPassed=false,upstreamAndSourceValid=false});return;}
}
var fx=contract.GetProperty("fixtures");var counts=fx.GetProperty("expectedCounts");
int[][] monomials=fx.GetProperty("quadraticMonomials").EnumerateArray().Select(v=>v.EnumerateArray().Select(w=>w.GetInt32()).ToArray()).ToArray();
var x=Polynomial.Variable(0);var y=Polynomial.Variable(1);var z=Polynomial.Variable(2);Polynomial[] t=[x,y,z];
int[] masks=Enumerable.Range(0,8).Concat(Enumerable.Range(0,8).Select(v=>Omega^v)).ToArray();
bool wordPassed=true,hodgePassed=true;int wordCases=0,hodgeCases=0;
foreach(int a in masks)foreach(int b in masks){wordCases++;wordPassed&=BladeSign(a,b)==WordProductSign(a,b);}
for(int mask=0;mask<=Omega;mask++){hodgeCases++;int r=Degree(mask);hodgePassed&=HodgeSign(mask)*HodgeSign(Omega^mask)==((r*(14-r)+7)%2==0?1:-1);}
Rational[][] matrixKnown=[[1,2],[0,-1]];var inverseKnown=Inverse(matrixKnown);
var knownKernel=Kernel([[1,2,0],[0,0,1]]);
bool arithmeticPassed=RMatrixSame(MatMul(matrixKnown,inverseKnown),Identity(2))&&knownKernel.Rank==2&&knownKernel.Vectors.Length==1
 &&RVectorZero(MatVec([[1,2,0],[0,0,1]],knownKernel.Vectors[0]))
 &&(x*x*y+z*3).Derivative(0).Same(x*y*2)&&(x*x*y+z*3).Derivative(2).Same(Polynomial.Constant(3))
 &&x.Coefficient(1,0,0)==new Scalar(1)&&Scalar.I*Scalar.I==new Scalar(-1)&&BladeSign(Omega,Omega)==1;
bool knownAnswerPassed=wordPassed&&hodgePassed&&arithmeticPassed&&masks.Length==fx.GetProperty("knownAnswer").GetProperty("maskCount").GetInt32()
 &&wordCases==fx.GetProperty("knownAnswer").GetProperty("wordCases").GetInt32()&&hodgeCases==fx.GetProperty("knownAnswer").GetProperty("hodgeCases").GetInt32();
if(!knownAnswerPassed){Emit(precedence[1],new{knownAnswerPassed,controlsPassed=false,wordPassed,hodgePassed,arithmeticPassed,wordCases,hodgeCases});return;}

var gamma=new PT();for(int j=0;j<14;j++)Pput(gamma,(1<<j,1<<j),Polynomial.Constant(1));
var gamma2=new PT();foreach(var (a,b) in Pairs())Pput(gamma2,((1<<a)|(1<<b),(1<<a)|(1<<b)),Polynomial.Constant(1));
PT f1=PSingle(3,5,Polynomial.Constant(1)),fTranspose=PSingle(5,3,Polynomial.Constant(1)),f2=PSingle(6,Omega^2,Polynomial.Constant(BladeSign(Omega,2)));
PT[] projectionBasis=[f1,fTranspose,f2];
Rational half=new(1,2);Rational[][] projection=[[half,half,0],[half,half,0],[0,0,0]];
bool projectionStructurePassed=RMatrixSame(Gram(projectionBasis),Identity(3))&&RMatrixSame(MatMul(projection,projection),projection)
 &&RMatrixSame(Transpose(projection),projection)&&Kernel(projection).Rank==1
 &&MatVec(projection,[1,1,0]).SequenceEqual(new Rational[]{1,1,0})&&RVectorZero(MatVec(projection,[1,-1,0]))&&RVectorZero(MatVec(projection,[0,0,1]));
bool carrierPassed=true,adjointPassed=true,gradientPassed=true,reciprocityPassed=true,eulerPassed=true,factorizationPassed=true,projectionPassed=projectionStructurePassed,massPassed=true;
int carrierRows=0,coefficientRows=0,gradientRows=0,gradientComponents=0,jacobianEntries=0,curlComponents=0,desiredCurlRejections=0,zeroCRows=0,
 unweightedJacobianRejections=0,linearFactorizationRejections=0,nonlinearFactorizationRejections=0,kAdjointChecks=0,qAdjointChecks=0,sourceKernelVectors=0,projectionRows=0,massRows=0;
var carriersOut=new List<object>();var rowsOut=new List<object>();var projectionsOut=new List<object>();var massesOut=new List<object>();
foreach(var carrier in fx.GetProperty("carriers").EnumerateArray())
foreach(var op in fx.GetProperty("operatorRows").EnumerateArray())
{
 string cid=carrier.GetProperty("id").GetString()!,oid=op.GetProperty("id").GetString()!;
 if(!carrier.GetProperty("operatorIds").EnumerateArray().Any(v=>v.GetString()==oid))continue;
 carrierRows++;bool first=cid=="curvature-stationary";int a=op.GetProperty("a").GetInt32(),b=op.GetProperty("b").GetInt32(),d=op.GetProperty("d").GetInt32();
 PT[] e=first?[PSingle(1,3,Polynomial.Constant(1)),PSingle(2,6,Polynomial.Constant(1)),PSingle(2,4,Polynomial.Constant(1))]
  :[PSingle(1,5,Polynomial.Constant(1)),PSingle(2,2,Polynomial.Constant(1)),PSingle(4,Omega,Polynomial.Constant(1))];
 PT f=first?f1:f2;Rational[][] ge=Gram(e),gie=Inverse(ge);Rational gf=ConstantReal(PairForms(f,f));
 int[] signs=carrier.GetProperty("gramDiagonal").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
 bool gramPassed=RMatrixSame(ge,Diagonal(signs))&&RMatrixSame(MatMul(ge,gie),Identity(3))&&gf==1&&e.All(Hanti);
 PT torsion=Combine(e,t),curvature=PWedge(torsion,torsion);Polynomial qc=PairForms(f,curvature)*Reciprocal(gf);
 Polynomial expectedQ=first?x*y*2:y*z*-2;Polynomial[] qt=e.Select(v=>PairForms(f,PScale(PAdd(PWedge(torsion,v),PWedge(v,torsion)),new Scalar(half,0)))*Reciprocal(gf)).ToArray();
 Polynomial[] expectedQt=first?[y,x,Zero()]:[Zero(),z*-1,y*-1];
 bool qPassed=qc.Same(expectedQ)&&VectorSame(qt,expectedQt)&&PSame(curvature,PPolyScale(f,qc));
 var qmap=Coefficients([qc],monomials);var qkernel=Kernel(qmap);sourceKernelVectors+=qkernel.Vectors.Length;
 qPassed&=qkernel.Rank==fx.GetProperty("sourceQRank").GetInt32()&&qkernel.Vectors.Length==fx.GetProperty("sourceQKernelDimension").GetInt32()
  &&qkernel.Vectors.All(v=>RVectorZero(MatVec(qmap,v)));
 carrierPassed&=gramPassed&&qPassed;
 var phi1=PAdd(PScale(gamma,a),PScale(POmega(gamma),b));PT[] phi2=[PScale(POmega(gamma2),Scalar.I*d),gamma2];
 carriersOut.Add(new{carrier=cid,op=oid,gramPassed,qPassed,gram=RMatrixTerms(ge),curvatureGram=gf.ToString(),q=qc.Terms(),qT=VectorTerms(qt),qMap=RMatrixTerms(qmap),rank=qkernel.Rank,kernel=qkernel.Vectors.Select(RVectorTerms).ToArray()});
 for(int slot=0;slot<2;slot++)
 {
  coefficientRows++;var unit=Chain(f,phi1,phi2[slot],slot==0);var actual=Chain(curvature,phi1,phi2[slot],slot==0);
  var predictedFull=new PT();if(slot==0)
  {
   if(first){Pput(predictedFull,(2,4),Polynomial.Constant(-2*a));Pput(predictedFull,(2,Omega^4),Polynomial.Constant(-2*b*BladeSign(Omega,4)));}
   else for(int j=0;j<14;j++)if(j!=1&&j!=2)Pput(predictedFull,(1<<j,(1<<j)^4),Polynomial.Constant(-2*b*BladeSign(1<<j,4)));
  }
  carrierPassed&=unit.Typed&&actual.Typed&&PSame(unit.Lowered,predictedFull)&&PSame(actual.Lowered,PPolyScale(unit.Lowered,qc))&&Hanti(unit.Lowered);
  Polynomial[] kp=e.Select(v=>PairForms(v,unit.Lowered)).ToArray();Polynomial[] ke=MatPoly(gie,kp);
  Rational[] kc=ke.Select(ConstantReal).ToArray();Rational[] expectedK=slot==0?(first?[0,0,-2*a]:[-2*b,0,0]):[0,0,0];
  carrierPassed&=kc.SequenceEqual(expectedK);
  // Kdagger and Q_Tdagger are computed from actual Grams and checked by their defining pairings.
  Rational[] kdag=Enumerable.Range(0,3).Select(j=>Div(Enumerable.Range(0,3).Aggregate(new Rational(0),(s,i)=>s+kc[i]*ge[i][j]),gf)).ToArray();
  Polynomial kadjT=RDot(kdag,t);Polynomial[] qadj=MatPoly(gie,qt.Select(v=>v*new Scalar(gf,0)).ToArray());
  for(int i=0;i<3;i++)
  {
   kAdjointChecks++;qAdjointChecks++;
   adjointPassed&=PairForms(e[i],unit.Lowered).Same(Polynomial.Constant(new Scalar(kdag[i]*gf,0)));
   PT qadjTensor=Combine(e,qadj);PT actualQTV=PScale(PAdd(PWedge(torsion,e[i]),PWedge(e[i],torsion)),new Scalar(half,0));
   adjointPassed&=PairForms(e[i],qadjTensor).Same(PairForms(actualQTV,f));
  }
  Polynomial[] desiredRaised=kc.Select(v=>qc*new Scalar(v,0)).ToArray();Polynomial[] desiredCov=MatPoly(ge,desiredRaised);
  Polynomial[] expectedDesired=slot==0?(first?[Zero(),Zero(),x*y*(4*a)]:[y*z*(4*b),Zero(),Zero()]):Zeros(3);
  carrierPassed&=VectorSame(desiredCov,expectedDesired);
  Polynomial rawAction=TopTrace(PWedge(torsion,actual.Upper));
  carrierPassed&=rawAction.Same(Dot(t,desiredCov));
  // Projection acts on the original full curvature before applying the complete source chain.
  Polynomial[] pcoords=projectionBasis.Select(v=>PairForms(v,curvature)).ToArray();PT projectedQ=Combine(projectionBasis,MatPoly(projection,pcoords));
  var projectedK=Chain(projectedQ,phi1,phi2[slot],slot==0);Polynomial[] projectedCov=e.Select(v=>PairForms(v,projectedK.Lowered)).ToArray();
  Polynomial[] expectedProjected=first?expectedDesired.Select(v=>v*new Scalar(half,0)).ToArray():Zeros(3);
  bool pPassed=projectedK.Typed&&VectorSame(projectedCov,expectedProjected)&&!PSame(projectedQ,curvature)&&(first||projectedQ.Count==0);
  projectionRows++;projectionPassed&=pPassed;
  projectionsOut.Add(new{carrier=cid,op=oid,slot,passed=pPassed,input=PTerms(curvature),projectedInput=PTerms(projectedQ),projectedCovector=VectorTerms(projectedCov)});
  foreach(int encoding in fx.GetProperty("gammaEncodings").EnumerateArray().Select(v=>v.GetInt32()))
  {
   gradientRows++;Scalar prefactor=new(new Rational(encoding,3),0);Polynomial action=rawAction*prefactor;
   Polynomial[] directCov=Enumerable.Range(0,3).Select(action.Derivative).ToArray();
   Polynomial[] adjointGradient=Enumerable.Range(0,3).Select(i=>(desiredRaised[i]+qadj[i]*kadjT*2)*prefactor).ToArray();
   Polynomial[] raisedFromDirect=MatPoly(gie,directCov);
   Polynomial[] expectedCov=slot==0?new[]{y*z,x*z,x*y}.Select(v=>v*(4*(first?a:b))*prefactor).ToArray():Zeros(3);
   bool gradients=VectorSame(adjointGradient,raisedFromDirect)&&VectorSame(directCov,expectedCov);
   gradientComponents+=3;gradientPassed&=gradients;
   Polynomial[][] raisedJac=Jacobian(adjointGradient),weightedJac=MatPolyMatrix(ge,raisedJac),hessian=Jacobian(directCov);
   Polynomial[] desiredCurl=Curl(desiredCov),actualCurl=Curl(MatPoly(ge,adjointGradient));
   Polynomial[] curlOracle=slot==0?(first?[Zero(),y*(4*a),x*(4*a)]:[z*(-4*b),y*(-4*b),Zero()]):Zeros(3);
   bool curls=VectorSame(desiredCurl,curlOracle)&&VectorZero(actualCurl)&&PMatrixSame(weightedJac,hessian)&&PMatrixSame(weightedJac,PTranspose(weightedJac));
   jacobianEntries+=9;curlComponents+=3;
   bool unweightedNonsymmetric=!PMatrixSame(raisedJac,PTranspose(raisedJac));
   bool unweightedAtOne=Enumerable.Range(0,3).Any(i=>Enumerable.Range(0,3).Any(j=>raisedJac[i][j].Evaluate(1,1,1)!=raisedJac[j][i].Evaluate(1,1,1)));
   if(slot==0){desiredCurlRejections++;unweightedJacobianRejections++;curls&=!VectorZero(desiredCurl)&&unweightedNonsymmetric&&unweightedAtOne;}
   else{zeroCRows++;curls&=VectorZero(desiredCurl)&&!unweightedNonsymmetric;}
   curls&=desiredCurl.All(v=>v.Evaluate(0,0,0).IsZero);
   reciprocityPassed&=curls;
   Polynomial eulerPotential=Dot(t,desiredCov)*new Scalar(new Rational(1,3),0);
   Polynomial[] eulerDerivative=Enumerable.Range(0,3).Select(eulerPotential.Derivative).ToArray();
   bool euler=Dot(t,directCov).Same(action*3)&&(VectorSame(eulerDerivative,desiredCov)==(slot==1));eulerPassed&=euler;
   Rational[][] gradientMap=Coefficients(adjointGradient,monomials);
   bool annihilates=qkernel.Vectors.All(v=>RVectorZero(MatVec(gradientMap,v)));
   int column=fx.GetProperty("kernelColumnWitness").GetInt32();bool explicitKernel=qmap.All(row=>row[column]==0);
   bool explicitImage=gradientMap.Any(row=>row[column]!=0);
   int[][] pair=carrier.GetProperty("collision").EnumerateArray().Select(v=>v.EnumerateArray().Select(w=>w.GetInt32()).ToArray()).ToArray();
   bool sameCurvature=PSame(PEvaluate(curvature,pair[0]),PEvaluate(curvature,pair[1]));
   bool sameGradient=Evaluate(adjointGradient,pair[0]).SequenceEqual(Evaluate(adjointGradient,pair[1]));
   bool factor=explicitKernel&&annihilates==(slot==1)&&explicitImage==(slot==0)&&sameCurvature&&sameGradient==(slot==1);
   if(slot==0){linearFactorizationRejections++;nonlinearFactorizationRejections++;}
   factorizationPassed&=factor;
   rowsOut.Add(new{carrier=cid,op=oid,slot,encoding,gradients,curls,euler,factor,action=action.Terms(),directCovector=VectorTerms(directCov),adjointRaisedGradient=VectorTerms(adjointGradient),desiredCovector=VectorTerms(desiredCov),
    desiredCurl=VectorTerms(desiredCurl),actualCurl=VectorTerms(actualCurl),weightedJacobian=PMatrixTerms(weightedJac),unweightedJacobian=PMatrixTerms(raisedJac),eulerPotential=eulerPotential.Terms(),eulerDerivative=VectorTerms(eulerDerivative),
    gradientMap=RMatrixTerms(gradientMap),annihilatesCurvatureKernel=annihilates,explicitKernel,explicitImage,sameCurvature,sameGradient,collisionPoints=pair,collisionGradients=pair.Select(v=>Evaluate(adjointGradient,v).Select(ScalarText).ToArray()).ToArray()});
  }
 }
 foreach(int kappa in fx.GetProperty("massKappa").EnumerateArray().Select(v=>v.GetInt32()))
 {
  massRows++;Polynomial mass=PairForms(torsion,torsion)*new Scalar(new Rational(kappa,2),0);
  Polynomial[] direct=Enumerable.Range(0,3).Select(mass.Derivative).ToArray(),raised=MatPoly(gie,direct),expected=t.Select(v=>v*kappa).ToArray();
  Polynomial[] desired=first?[Zero(),Zero(),x*y*4]:[y*z*(4*b),Zero(),Zero()];
  bool pass=VectorSame(raised,expected)&&VectorZero(Curl(direct))&&Dot(t,direct).Same(mass*2)
   &&VectorSame(Curl(desired.Zip(direct,(u,v)=>u+v).ToArray()),Curl(desired));
  massPassed&=pass;massesOut.Add(new{carrier=cid,op=oid,kappa,passed=pass,action=mass.Terms(),covector=VectorTerms(direct),raised=VectorTerms(raised)});
 }
}
carrierPassed&=carrierRows==Count("carrierRows")&&coefficientRows==Count("coefficientRows")&&sourceKernelVectors==Count("sourceKernelVectors");
adjointPassed&=kAdjointChecks==Count("kAdjointChecks")&&qAdjointChecks==Count("qAdjointChecks");
if(!carrierPassed||!adjointPassed){Emit(precedence[2],new{knownAnswerPassed,controlsPassed=false,carrierPassed,adjointPassed,carriersOut,rowsOut,projectionsOut});return;}
gradientPassed&=gradientRows==Count("gradientRows")&&gradientComponents==Count("gradientComponents");
reciprocityPassed&=jacobianEntries==Count("jacobianEntries")&&curlComponents==Count("curlComponents")&&desiredCurlRejections==Count("desiredCurlRejections")&&zeroCRows==Count("zeroCRows")&&unweightedJacobianRejections==Count("unweightedJacobianRejections");
if(!gradientPassed||!reciprocityPassed||!eulerPassed){Emit(precedence[3],new{knownAnswerPassed,controlsPassed=false,gradientPassed,reciprocityPassed,eulerPassed,rowsOut});return;}
factorizationPassed&=linearFactorizationRejections==Count("linearFactorizationRejections")&&nonlinearFactorizationRejections==Count("nonlinearFactorizationRejections");
projectionPassed&=projectionRows==Count("projectionRows");
if(!factorizationPassed||!projectionPassed){Emit(precedence[4],new{knownAnswerPassed,controlsPassed=false,factorizationPassed,projectionPassed,rowsOut,projectionsOut});return;}

PT[] csE=[PSingle(1,3,Polynomial.Constant(1)),PSingle(2,6,Polynomial.Constant(1)),PSingle(4,5,Polynomial.Constant(1))];
PT[] csF=[PSingle(3,5,Polynomial.Constant(1)),PSingle(5,6,Polynomial.Constant(1)),PSingle(6,3,Polynomial.Constant(1))];
PT csT=Combine(csE,t),csQ=PWedge(csT,csT),nu=PSingle(fx.GetProperty("chernSimons").GetProperty("nu11Mask").GetInt32(),0,Polynomial.Constant(1));
Rational[][] csGram=Gram(csE),csFGram=Gram(csF);Rational[][] csK=Enumerable.Range(0,3).Select(i=>csF.Select(v=>ConstantReal(PairForms(csE[i],PStar(PWedge(v,nu))))).ToArray()).ToArray();
Polynomial[] csQc=csF.Select(v=>PairForms(v,csQ)).ToArray();Rational[][] csQMap=Coefficients(csQc,monomials);var csKernel=Kernel(csQMap);
bool csPassed=RMatrixSame(csGram,Identity(3))&&RMatrixSame(csFGram,Identity(3))&&RMatrixSame(csK,[[0,0,1],[0,-1,0],[1,0,0]])
 &&VectorSame(csQc,[x*y*2,x*z*-2,y*z*2])&&csKernel.Rank==3&&csKernel.Vectors.Length==3;
Polynomial[][] csQt=csF.Select(v=>csE.Select(e=>PairForms(v,PScale(PAdd(PWedge(csT,e),PWedge(e,csT)),new Scalar(half,0)))).ToArray()).ToArray();
Polynomial[] csKadjT=MatPoly(Transpose(csK),t),csDesired=MatPoly(csK,csQc);
Polynomial rawCs=TopTrace(PWedge(csT,PWedge(csQ,nu)));var csRows=new List<object>();
foreach(int encoding in fx.GetProperty("gammaEncodings").EnumerateArray().Select(v=>v.GetInt32()))
{
 Polynomial action=rawCs*encoding*new Scalar(new Rational(1,3*encoding),0);
 Polynomial[] direct=Enumerable.Range(0,3).Select(action.Derivative).ToArray();Polynomial[] csAdjTerm=PMatVec(PTranspose(csQt),csKadjT);
 Polynomial[] viaAdjoint=csDesired.Zip(csAdjTerm,(u,v)=>(u+v*2)*new Scalar(new Rational(1,3),0)).ToArray();
 Rational[][] map=Coefficients(viaAdjoint,monomials);
 bool pass=action.Same(x*y*z*2)&&VectorSame(direct,[y*z*2,x*z*2,x*y*2])&&VectorSame(direct,viaAdjoint)&&VectorSame(direct,csDesired)
  &&VectorZero(Curl(direct))&&PMatrixSame(Jacobian(viaAdjoint),PTranspose(Jacobian(viaAdjoint)))&&Dot(t,direct).Same(action*3)
  &&csKernel.Vectors.All(v=>RVectorZero(MatVec(map,v)))&&RMatrixSame(map,MatMul(csK,csQMap));
 csPassed&=pass;csRows.Add(new{encoding,passed=pass,action=action.Terms(),gradient=VectorTerms(direct),adjointGradient=VectorTerms(viaAdjoint),desired=VectorTerms(csDesired),curvatureMap=RMatrixTerms(csQMap),gradientMap=RMatrixTerms(map),rank=csKernel.Rank,kernel=csKernel.Vectors.Select(RVectorTerms).ToArray()});
}
bool controlsPassed=carrierPassed&&adjointPassed&&gradientPassed&&reciprocityPassed&&eulerPassed&&factorizationPassed&&projectionPassed&&massPassed&&csPassed&&massRows==Count("massRows")&&csRows.Count==Count("csRows");
Emit(controlsPassed?Success:precedence[5],new{knownAnswerPassed,controlsPassed,wordPassed,hodgePassed,arithmeticPassed,wordCases,hodgeCases,
 carrierPassed,adjointPassed,gradientPassed,reciprocityPassed,eulerPassed,factorizationPassed,projectionPassed,massPassed,csPassed,
 carrierRows,coefficientRows,gradientRows,gradientComponents,jacobianEntries,curlComponents,desiredCurlRejections,zeroCRows,unweightedJacobianRejections,
 linearFactorizationRejections,nonlinearFactorizationRejections,kAdjointChecks,qAdjointChecks,sourceKernelVectors,projectionRows,massRows,
 carriersOut,rowsOut,projectionsOut,massesOut,csRows,sourceChoicesOpen=true,actionChanged=false,ambientClosureClaimed=false,registeredCoreChanged=false});

int Count(string key)=>counts.GetProperty(key).GetInt32();
static Polynomial Zero()=>Polynomial.Constant(0);
static Polynomial[] Zeros(int n)=>Enumerable.Range(0,n).Select(_=>Zero()).ToArray();
static Rational ReciprocalR(Rational a)=>a.Numerator==0?throw new DivideByZeroException():new(a.Denominator,a.Numerator);
static Scalar Reciprocal(Rational a)=>new(ReciprocalR(a),0);
static Rational Div(Rational a,Rational b)=>a*ReciprocalR(b);
static Rational ConstantReal(Polynomial p)
{Scalar value=p.Coefficient(0,0,0);if(value.Imaginary!=0||!p.Same(Polynomial.Constant(value)))throw new InvalidOperationException("Expected real constant");return value.Real;}
static PT PPolyScale(PT a,Polynomial p){var r=new PT();foreach(var v in a)Pput(r,v.Key,v.Value*p);return r;}
static PT Combine(PT[] basis,Polynomial[] coefficients){var r=new PT();for(int i=0;i<basis.Length;i++)r=PAdd(r,PPolyScale(basis[i],coefficients[i]));return r;}
static Polynomial PairForms(PT a,PT b)
{
 Polynomial p=Zero();foreach(var u in a)foreach(var v in b)if(u.Key==v.Key)
  p+=(u.Value*v.Value).RealPart()*(-BladeSign(u.Key.Blade,v.Key.Blade)*(Degree(u.Key.Form&0x3f80)%2==0?1:-1));return p;
}
static Rational[][] Gram(PT[] basis)=>basis.Select(a=>basis.Select(b=>ConstantReal(PairForms(a,b))).ToArray()).ToArray();
static (PT Upper,PT Lowered,bool Typed) Chain(PT f,PT p1,PT p2,bool includeFirst)
{
 PT first=includeFirst?PBracket(p1,PStar(f),'C'):new PT();PT inner=PBracket(p2,PStar(f),'A'),innerLower=PStar(inner),outer=PBracket(p1,innerLower,'C');
 PT upper=PAdd(first,PScale(PStar(outer),new Scalar(new Rational(-1,2),0))),lowered=PStar(upper);
 return(upper,lowered,HasDegree(f,2)&&HasDegree(first,13)&&HasDegree(inner,14)&&HasDegree(innerLower,0)&&HasDegree(outer,1)&&HasDegree(upper,13)&&HasDegree(lowered,1));
}
static Polynomial Dot(Polynomial[] a,Polynomial[] b){Polynomial p=Zero();for(int i=0;i<a.Length;i++)p+=a[i]*b[i];return p;}
static Polynomial RDot(Rational[] a,Polynomial[] b){Polynomial p=Zero();for(int i=0;i<a.Length;i++)p+=b[i]*new Scalar(a[i],0);return p;}
static Polynomial[] MatPoly(Rational[][] a,Polynomial[] b)=>a.Select(row=>RDot(row,b)).ToArray();
static Polynomial[] PMatVec(Polynomial[][] a,Polynomial[] b)=>a.Select(row=>Dot(row,b)).ToArray();
static Polynomial[][] MatPolyMatrix(Rational[][] a,Polynomial[][] b)=>a.Select(row=>Enumerable.Range(0,b[0].Length).Select(j=>RDot(row,b.Select(v=>v[j]).ToArray())).ToArray()).ToArray();
static Polynomial[][] Jacobian(Polynomial[] f)=>f.Select(p=>Enumerable.Range(0,3).Select(p.Derivative).ToArray()).ToArray();
static Polynomial[][] PTranspose(Polynomial[][] a)=>Enumerable.Range(0,a[0].Length).Select(j=>a.Select(row=>row[j]).ToArray()).ToArray();
static Polynomial[] Curl(Polynomial[] f)=>[f[1].Derivative(0)+f[0].Derivative(1)*-1,f[2].Derivative(0)+f[0].Derivative(2)*-1,f[2].Derivative(1)+f[1].Derivative(2)*-1];
static bool VectorZero(Polynomial[] p)=>p.All(v=>v.IsZero);
static bool VectorSame(Polynomial[] a,Polynomial[] b)=>a.Length==b.Length&&a.Zip(b,(u,v)=>u.Same(v)).All(v=>v);
static bool PMatrixSame(Polynomial[][] a,Polynomial[][] b)=>a.Length==b.Length&&a.Zip(b,VectorSame).All(v=>v);
static Rational[][] Coefficients(Polynomial[] p,int[][] monomials)=>p.Select(v=>monomials.Select(m=>
 {Scalar c=v.Coefficient(m[0],m[1],m[2]);if(c.Imaginary!=0)throw new InvalidOperationException("Real coefficient required");return c.Real;}).ToArray()).ToArray();
static Scalar[] Evaluate(Polynomial[] p,int[] at)=>p.Select(v=>v.Evaluate(at[0],at[1],at[2])).ToArray();
static PT PEvaluate(PT p,int[] at){var r=new PT();foreach(var v in p)Pput(r,v.Key,Polynomial.Constant(v.Value.Evaluate(at[0],at[1],at[2])));return r;}
static object[] VectorTerms(Polynomial[] a)=>a.Select(v=>(object)v.Terms()).ToArray();
static object[] PMatrixTerms(Polynomial[][] a)=>a.Select(v=>(object)VectorTerms(v)).ToArray();
static string[] RVectorTerms(Rational[] a)=>a.Select(v=>v.ToString()).ToArray();
static string[][] RMatrixTerms(Rational[][] a)=>a.Select(RVectorTerms).ToArray();
static Rational[][] Identity(int n)=>Enumerable.Range(0,n).Select(i=>Enumerable.Range(0,n).Select(j=>new Rational(i==j?1:0)).ToArray()).ToArray();
static Rational[][] Diagonal(int[] d)=>Enumerable.Range(0,d.Length).Select(i=>Enumerable.Range(0,d.Length).Select(j=>new Rational(i==j?d[i]:0)).ToArray()).ToArray();
static Rational[][] Transpose(Rational[][] a)=>Enumerable.Range(0,a[0].Length).Select(j=>a.Select(row=>row[j]).ToArray()).ToArray();
static Rational[] MatVec(Rational[][] a,Rational[] b)=>a.Select(row=>row.Zip(b,(u,v)=>u*v).Aggregate(new Rational(0),(s,v)=>s+v)).ToArray();
static Rational[][] MatMul(Rational[][] a,Rational[][] b)=>Transpose(b).Select(col=>MatVec(a,col)).ToArray() is var columns?Transpose(columns):throw new InvalidOperationException();
static bool RVectorZero(Rational[] a)=>a.All(v=>v==0);
static bool RMatrixSame(Rational[][] a,Rational[][] b)=>a.Length==b.Length&&a.Zip(b,(u,v)=>u.SequenceEqual(v)).All(v=>v);
static (int Rank,Rational[][] Vectors) Kernel(Rational[][] input)
{
 var a=input.Select(v=>v.ToArray()).ToArray();int n=a[0].Length,row=0;var pivots=new List<int>();
 for(int col=0;col<n&&row<a.Length;col++)
 {
  int pivot=Enumerable.Range(row,a.Length-row).FirstOrDefault(i=>a[i][col]!=0,-1);if(pivot<0)continue;
  (a[row],a[pivot])=(a[pivot],a[row]);Rational value=a[row][col];for(int j=0;j<n;j++)a[row][j]=Div(a[row][j],value);
  for(int i=0;i<a.Length;i++)if(i!=row){Rational factor=a[i][col];for(int j=0;j<n;j++)a[i][j]=a[i][j]-factor*a[row][j];}
  pivots.Add(col);row++;
 }
 var vectors=new List<Rational[]>();foreach(int col in Enumerable.Range(0,n).Where(j=>!pivots.Contains(j)))
 {var v=new Rational[n];v[col]=1;for(int i=0;i<pivots.Count;i++)v[pivots[i]]=a[i][col]*-1;vectors.Add(v);}return(pivots.Count,vectors.ToArray());
}
static Rational[][] Inverse(Rational[][] input)
{
 int n=input.Length;var a=input.Select((v,i)=>v.Concat(Identity(n)[i]).ToArray()).ToArray();
 for(int col=0;col<n;col++)
 {
  int pivot=Enumerable.Range(col,n-col).FirstOrDefault(i=>a[i][col]!=0,-1);if(pivot<0)throw new InvalidOperationException("Degenerate Gram");
  (a[col],a[pivot])=(a[pivot],a[col]);Rational value=a[col][col];for(int j=0;j<2*n;j++)a[col][j]=Div(a[col][j],value);
  for(int i=0;i<n;i++)if(i!=col){Rational factor=a[i][col];for(int j=0;j<2*n;j++)a[i][j]=a[i][j]-factor*a[col][j];}
 }return a.Select(row=>row.Skip(n).ToArray()).ToArray();
}
void Emit(string verdict,object evidence)
{
 var result=new{schemaVersion=1,phase=594,phaseId="phase594-actual-gradient-reciprocity-audit",contractId=ContractId,
  contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,
  bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(v=>v.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(v=>v.path).Distinct().Count(),bindings,
  verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
  authorityFirewalls=firewalls.ToDictionary(v=>v,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
 string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;
 Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/actual_gradient_reciprocity_audit.json",json);File.WriteAllText(Root+"/output/actual_gradient_reciprocity_audit_summary.json",json);
 Console.WriteLine($"Phase594 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string p)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))).ToLowerInvariant();
static string Hash(string s)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(s))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(v=>v is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
