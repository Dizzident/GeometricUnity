using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root="studies/phase589_section_density_joint_lift_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ManifestPath=Root+"/preregistration/core_source_manifest_v1.json";
const string ContractId="phase589-a47-section-density-joint-lift-v1";
const string Rejected="density-nonselection-proved-literal-joint-lift-rejected-section-first-open";
const string Survives="density-nonselection-proved-joint-lift-survives-bounded-test-section-first-open";
const string Source="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt";
const string P583="studies/phase583_biconnection_convention_reconciliation_001/output/biconnection_convention_reconciliation_summary.json";
const string P585="studies/phase585_quotient_section_measure_controls_001/output/quotient_section_measure_controls_summary.json";
const string P586="studies/phase586_action_restriction_pairing_controls_001/output/action_restriction_pairing_controls_summary.json";
const string Pilot="studies/phase548_bounded_complete_lattice_pilot_execution_001/";
const string Pack="studies/phase577_chain_pack_execution_001/";
const string FixtureJson="""
{
  "signs":[-1,1],"profiles":["zero","commuting-constant","periodic-noncommuting"],
  "profileDefinition":"p0=0; p1=(Jx/8)dx0; p2=(Jx/8+cos(k*x0)Jy/16)dx0+(Jy/4+sin(k*x0)Jz/8)dx1",
  "generators":["constant-z","periodic-z"],
  "generatorDefinition":"alpha0=Jz; alpha1=(sin(k*x0)+cos(k*x1)/2)Jz; k=2*pi/3",
  "meshExtent":3,"expectedVertices":81,"expectedEdges":1215,"expectedFaces":4050,"expectedCells":1944,
  "member":"sd2/id0; EinsteinCoefficient=0.5; independent-theta; LowestIndex; default CpuMassMatrix",
  "expectedWardCases":12,"expectedGlobalCases":6,"expectedOriginCases":4,
  "ordinaryReference":"A0=0; proposed omega=p; theta tangent equals alpha at vertices",
  "steps":[0.0000152587890625,0.00000762939453125],
  "arithmeticTolerance":1e-12,"integralTolerance":2e-12,"derivativeTolerance":2e-7,
  "wardZeroMaximum":1e-10,"wardDecisiveMinimumExclusive":1e-6,
  "gaussOrder":16,"gaussNewtonIterations":20,
  "gaussianHalfWidth":12,"gaussianSimpsonPanels":4096,"gaussianTolerance":2e-10,
  "gaussianPrecisions":[1,2],"gaussianExpectedRadialMoments":[3,1.5],
  "chartRadiiPiMultipliers":[0,0.5,1],"toyBackground":[1,0,0],"toyPoint":[2,1,-1],
  "toyAnglePiMultiplier":0.3333333333333333,"toyGaugeAxisAnglePiMultipliers":[0,0.5,0],"toyWrongShiftMinimumDefect":1,
  "trigControlFrequencyPairs":[[0,0],[1,0],[0,1],[1,1],[1,-1],[2,0],[0,2]],
  "closedPeriodIntegralExpected":0,"equalEndpointCosineIntegralExpected":3,
  "coreFileCount":726,"estimatedCpuSeconds":45,"maximumEstimatedCpuSeconds":60,
  "estimatedPeakBytes":268435456,"maximumEstimatedPeakBytes":536870912
}
""";
string[] forbidden=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged",
    "registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened",
    "o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] precedence=["invalid-or-drifted-input","known-answer-battery-failed","mandatory-control-failed",
    "numerical-derivative-control-failed",Rejected,"unresolved-numerical-sensitivity",Survives];
string[] ids=["program","project","proof","primary-source","phase583-summary","phase585-summary","phase586-summary",
    "pilot-program","pilot-contract","pack-program","pack-contract","core-source-manifest","build-props"];
string[] paths=[Root+"/Program.cs",Root+"/Phase589SectionDensityJointLiftAudit.csproj",Root+"/STUDY.md",Source,P583,P585,P586,
    Pilot+"Program.cs",Pilot+"preregistration/phase548_bounded_complete_lattice_pilot_execution_contract_v1.json",
    Pack+"Program.cs",Pack+"preregistration/phase577_chain_pack_execution_contract_v1.json",ManifestPath,"Directory.Build.props"];
bool contractValid=false,exactBindingsValid=false,coreSourceTreeValid=false;
int coreFileCount=0;string coreTreeSha256="";Binding[] bindings=[];JsonElement contract=default;
try
{
    using var document=JsonDocument.Parse(File.ReadAllBytes(ContractPath));contract=document.RootElement.Clone();
    contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==589
        &&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
        &&contract.GetProperty("deterministicZeroSampling").GetBoolean()
        &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
        &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(precedence)
        &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
        &&forbidden.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)
        &&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
    bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(x=>new Binding(x.GetProperty("id").GetString()!,
        x.GetProperty("path").GetString()!,x.GetProperty("sha256").GetString()!)).ToArray();
    exactBindingsValid=bindings.Length==ids.Length&&bindings.Select(x=>x.id).Distinct().Count()==ids.Length
        &&bindings.Select(x=>x.path).Distinct().Count()==ids.Length
        &&ids.Zip(paths).All(x=>bindings.Any(b=>b.id==x.First&&b.path==x.Second&&b.hashMatches));
    if(contractValid&&exactBindingsValid)
    {
        using var manifestDoc=JsonDocument.Parse(File.ReadAllBytes(ManifestPath));var manifest=manifestDoc.RootElement;
        var live=CorePaths();var rows=manifest.GetProperty("files").EnumerateArray().ToArray();coreFileCount=live.Length;
        coreTreeSha256=HashText(string.Concat(live.Select(p=>p+" "+Sha(p)+"\n")));
        coreSourceTreeValid=manifest.GetProperty("schemaVersion").GetInt32()==1
            &&manifest.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
            &&coreFileCount==contract.GetProperty("fixtures").GetProperty("coreFileCount").GetInt32()
            &&rows.Select(x=>x.GetProperty("path").GetString()).SequenceEqual(live)
            &&rows.All(x=>Sha(x.GetProperty("path").GetString()!)==x.GetProperty("sha256").GetString())
            &&coreTreeSha256==manifest.GetProperty("treeSha256").GetString();
    }
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name});return;}
if(!contractValid||!exactBindingsValid||!coreSourceTreeValid){Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false});return;}
var fx=contract.GetProperty("fixtures");double tol=fx.GetProperty("arithmeticTolerance").GetDouble();
double itol=fx.GetProperty("integralTolerance").GetDouble(),dtol=fx.GetProperty("derivativeTolerance").GetDouble();
double zeroBand=fx.GetProperty("wardZeroMaximum").GetDouble(),decisiveBand=fx.GetProperty("wardDecisiveMinimumExclusive").GetDouble();
double[] steps=Vector(fx.GetProperty("steps"));
var gauss=GaussLegendre(fx.GetProperty("gaussOrder").GetInt32(),fx.GetProperty("gaussNewtonIterations").GetInt32());
double quadraturePolynomialError=Enumerable.Range(0,32).Max(power=>System.Math.Abs(gauss.Sum(q=>q.Weight*System.Math.Pow(q.X,power))-1.0/(power+1)));
var algebra=LieAlgebraFactory.CreateSu2WithTracePairing();
bool knownAnswerPassed=Norm(Sub(algebra.Bracket([1,0,0],[0,1,0]),[0,0,1]))==0
    &&Det(new double[,]{{2,1,0},{1,3,1},{0,1,2}})==8&&quadraturePolynomialError<=tol
    &&Norm(Sub(LogRotation(Rotation([0.2,-0.1,0.05])),[0.2,-0.1,0.05]))<=tol
    &&Classify([0,2e-8,2e-6],zeroBand,decisiveBand)=="rejected"
    &&Classify([0,2e-8],zeroBand,decisiveBand)=="unresolved"
    &&Classify([0,zeroBand],zeroBand,decisiveBand)=="survives"
    &&Classify([decisiveBand],zeroBand,decisiveBand)=="unresolved";
if(!knownAnswerPassed){Emit(precedence[1],new {knownAnswerPassed,controlsPassed=false,quadraturePolynomialError});return;}
bool upstreamValid=CheckUpstream(P583,"printed-sign-conflict-proved-two-compatible-families-registered-map-unresolved")
    &&CheckUpstream(P585,"conditional-section-measure-controls-pass-source-regulator-unresolved")
    &&CheckUpstream(P586,"action-restriction-controls-pass-pairing-bridge-unresolved");
string source=File.ReadAllText(Source),pilot=File.ReadAllText(Pilot+"Program.cs"),pack=File.ReadAllText(Pack+"Program.cs");
bool sourceAnchorsPresent=new[]{"(6.11)","(6.13)","(9.7)","(9.11)","(12.6)"}.All(source.Contains);
bool sampledSliceAnchorsPresent=pilot.Contains("double[] thetaZero = new double[mesh.VertexCount * algebra.Dimension];",StringComparison.Ordinal)
    &&pilot.Contains("op.ComputeJointGradient(omega, thetaZero, massMatrix)",StringComparison.Ordinal)
    &&pack.Contains("var thetaZero = new double[mesh.VertexCount * dimG];",StringComparison.Ordinal)
    &&pack.Contains("op.ComputeJointGradient(omega, thetaZero, mass)",StringComparison.Ordinal)
    &&pack.Contains("current.Action + 0.5 * Dot(momentum, momentum)",StringComparison.Ordinal);
if(!upstreamValid||!sourceAnchorsPresent||!sampledSliceAnchorsPresent)
{Emit(precedence[0],new {knownAnswerPassed,controlsPassed=false,upstreamValid,sourceAnchorsPresent,sampledSliceAnchorsPresent});return;}

// Local dLog_I identity, independently implemented with Rodrigues matrices and atan2 logarithms.
var fpRows=new List<object>();bool compactControlsPassed=true;double fpError=0,fpStepAgreement=0;
double[,]? previousFp=null;
foreach(double step in steps)
{
    var jac=new double[3,3];
    for(int col=0;col<3;col++)
    {
        double[] axis=new double[3];axis[col]=step;
        var difference=Scale(1/(2*step),Sub(LogRotation(Rotation(axis)),LogRotation(Rotation(Scale(-1,axis)))));
        for(int row=0;row<3;row++)jac[row,col]=difference[row];
    }
    double entryError=MatrixError(jac,Identity()),determinant=Det(jac);
    fpError=System.Math.Max(fpError,System.Math.Max(entryError,System.Math.Abs(determinant-1)));
    if(previousFp is not null)fpStepAgreement=MatrixError(jac,previousFp);
    previousFp=jac;fpRows.Add(new {step,entryError,determinant});
}
var haarRows=new List<object>();double dexpError=0;
foreach(double multiplier in Vector(fx.GetProperty("chartRadiiPiMultipliers")))
{
    double r=System.Math.PI*multiplier;double[] theta=[0,0,r];
    var integral=new double[3,3];foreach(var q in gauss)AddTo(integral,Rotation(Scale(-q.X,theta)),q.Weight);
    var k=Skew(theta);var closed=Identity();
    if(r!=0){AddTo(closed,k,-(1-System.Math.Cos(r))/(r*r));AddTo(closed,Multiply(k,k),(r-System.Math.Sin(r))/(r*r*r));}
    double predicted=r==0?1:multiplier==0.5?8/(System.Math.PI*System.Math.PI):4/(System.Math.PI*System.Math.PI);
    double integratedDeterminant=Det(integral),closedDeterminant=Det(closed),error=MatrixError(integral,closed);
    dexpError=System.Math.Max(dexpError,System.Math.Max(error,System.Math.Max(System.Math.Abs(integratedDeterminant-predicted),System.Math.Abs(closedDeterminant-predicted))));
    haarRows.Add(new {r,multiplier,predicted,integratedDeterminant,closedDeterminant,error,piIsChartBoundary=multiplier==1});
}
var densityRows=new List<object>();double densityError=0;double[] expectedMoments=Vector(fx.GetProperty("gaussianExpectedRadialMoments"));
double[] precisions=Vector(fx.GetProperty("gaussianPrecisions"));
for(int i=0;i<precisions.Length;i++)
{
    double precision=precisions[i];var moments=GaussianIntegrals(precision,fx.GetProperty("gaussianHalfWidth").GetDouble(),fx.GetProperty("gaussianSimpsonPanels").GetInt32());
    double normalization=System.Math.Sqrt(2*System.Math.PI/precision),radialMoment=3*moments.Second/moments.Z;
    densityError=System.Math.Max(densityError,System.Math.Max(System.Math.Abs(moments.Z-normalization),System.Math.Abs(radialMoment-expectedMoments[i])));
    densityRows.Add(new {precision,oneDimensionalIntegral=moments.Z,normalization,radialMoment,expectedRadialMoment=expectedMoments[i]});
}
// Both invariant densities use this same affine-coordinate Jacobian, regardless of c.
double toyJacobianError=0,toyInvariantError=0,toyWrongShiftMinimumDefect=double.PositiveInfinity;var toyRows=new List<object>();
var toyRotation=Rotation([0,0,System.Math.PI*fx.GetProperty("toyAnglePiMultiplier").GetDouble()]);
var toyGauge=Rotation(Scale(System.Math.PI,Vector(fx.GetProperty("toyGaugeAxisAnglePiMultipliers"))));
foreach(int c in fx.GetProperty("signs").EnumerateArray().Select(x=>x.GetInt32()))
{
    double[] p=Vector(fx.GetProperty("toyPoint")),b=Vector(fx.GetProperty("toyBackground"));
    // C_h=R(h)^-1 B-B fixes the affine sign: Phi=R(e)(c*p+B)-B.
    double[] phi=Sub(Apply(toyRotation,Add(Scale(c,p),b)),b);
    double[] cocycle=Sub(Apply(Transpose(toyGauge),b),b);
    double[] movedP=Add(Apply(Transpose(toyGauge),p),Scale(c,cocycle));
    double[] movedPhi=Sub(Apply(Multiply(toyRotation,toyGauge),Add(Scale(c,movedP),b)),b);
    toyInvariantError=System.Math.Max(toyInvariantError,Norm(Sub(phi,movedPhi)));
    double[] wrongPhi=Add(Apply(toyRotation,Sub(Scale(c,p),b)),b);
    double[] wrongMovedPhi=Add(Apply(Multiply(toyRotation,toyGauge),Sub(Scale(c,movedP),b)),b);
    double wrongShiftDefect=Norm(Sub(wrongPhi,wrongMovedPhi));toyWrongShiftMinimumDefect=System.Math.Min(toyWrongShiftMinimumDefect,wrongShiftDefect);
    double jacobian=Det(MatrixScale(c,toyRotation));toyJacobianError=System.Math.Max(toyJacobianError,System.Math.Abs(jacobian-c));
    toyRows.Add(new {c,phi,movedPhi,jacobian,wrongShiftDefect,flatDensity=1.0,tiltedInvariantDensity=System.Math.Exp(-Dot(phi,phi)/2)});
}
compactControlsPassed=fpError<=dtol&&fpStepAgreement<=dtol&&dexpError<=tol&&toyJacobianError<=tol&&toyInvariantError<=tol
    &&toyWrongShiftMinimumDefect>fx.GetProperty("toyWrongShiftMinimumDefect").GetDouble()
    &&densityError<=fx.GetProperty("gaussianTolerance").GetDouble();

// Exact analytic Fourier integrals of continuum fields on actual shortest lifted edges.
var mesh=SimplicialMeshGenerator.CreateUniform4DPeriodic(3,latticeCanonical:true);
bool topologyPassed=mesh.VertexCount==fx.GetProperty("expectedVertices").GetInt32()&&mesh.EdgeCount==fx.GetProperty("expectedEdges").GetInt32()
    &&mesh.FaceCount==fx.GetProperty("expectedFaces").GetInt32()&&mesh.CellCount==fx.GetProperty("expectedCells").GetInt32();
double[][] starts=mesh.Edges.Select(edge=>mesh.GetVertexCoordinates(edge[0]).ToArray()).ToArray();
double[][] displacements=mesh.Edges.Select(edge=>Lift(mesh.GetVertexCoordinates(edge[0]).ToArray(),mesh.GetVertexCoordinates(edge[1]).ToArray())).ToArray();
int seamEdgeCount=0;double closureError=0;
for(int e=0;e<mesh.EdgeCount;e++)
{
    var end=mesh.GetVertexCoordinates(mesh.Edges[e][1]).ToArray();var raw=Sub(end,starts[e]);
    if(Norm(Sub(raw,displacements[e]))>0)seamEdgeCount++;
    topologyPassed&=displacements[e].All(x=>x is -1 or 0 or 1)&&Norm(displacements[e])>0;
    topologyPassed&=Norm(Add(displacements[e],Lift(end,starts[e])))==0;
}
for(int f=0;f<mesh.FaceCount;f++)
{
    double[] sum=new double[4];for(int k=0;k<3;k++)sum=Add(sum,Scale(mesh.FaceBoundaryOrientations[f][k],displacements[mesh.FaceBoundaryEdges[f][k]]));
    closureError=System.Math.Max(closureError,Norm(sum));
}
topologyPassed&=seamEdgeCount>0&&closureError==0;
double trigError=0,periodIntegralError=0;
// Nonzero constant frequency along a transverse loop: equal endpoints do NOT imply zero one-form integral.
double equalEndpointCosineIntegral=3*Integrate(CosMode(0,1),[0,0,0,0],[3,0,0,0]).Real;
foreach(var pair in fx.GetProperty("trigControlFrequencyPairs").EnumerateArray())
{
    int m0=pair[0].GetInt32(),m1=pair[1].GetInt32();var mode=Mode(m0,m1,Complex.One);
    double[] start=[0.25,-0.5,0,0],delta=[1,-1,0,0];var exact=Integrate(mode,start,delta);
    Complex quadrature=Complex.Zero;foreach(var q in gauss)quadrature+=q.Weight*Complex.Exp(Complex.ImaginaryOne*(2*System.Math.PI/3)*(m0*(start[0]+q.X*delta[0])+m1*(start[1]+q.X*delta[1])));
    trigError=System.Math.Max(trigError,Complex.Abs(exact-quadrature));
}
periodIntegralError=Complex.Abs(3*Integrate(SinMode(1,0),[0,0,0,0],[3,0,0,0]));
bool elementaryIntegralControlsPassed=trigError<=itol&&periodIntegralError<=itol
    &&System.Math.Abs(equalEndpointCosineIntegral-fx.GetProperty("equalEndpointCosineIntegralExpected").GetDouble())<=itol;

var member=new EinsteinianShiabFamilyMember{Phi1=InvariantElementSpec.Sd2,Phi2=InvariantElementSpec.Id0,EinsteinCoefficient=0.5,
    EpsilonMode="independent-theta",VertexFaceRule=VertexFaceRule.LowestIndex};
var op=new EinsteinianShiabOperator(mesh,algebra,member,latticePeriod:3);var mass=new CpuMassMatrix(mesh,algebra);
double[] thetaZero=new double[mesh.VertexCount*3];
var wardRows=new List<object>();var ratios=new List<double>();
double maxIntegralError=0,maxReversalError=0,maxPeriodicShiftError=0,maxExactDerivativeError=0,maxImaginaryResidual=0,maxDerivativeError=0;
bool derivativeControlsPassed=true,originControlsPassed=true,globalControlsPassed=true;int globalCases=0,originCases=0;
for(int profile=0;profile<3;profile++)for(int generator=0;generator<2;generator++)foreach(int c in new[]{-1,1})
{
    var fields=ProfileModes(profile);var alpha=generator==0?Constant(1):SumModes(SinMode(1,0),ScaleModes(0.5,CosMode(0,1)));
    double[] omega=new double[mesh.EdgeCount*3],direction=new double[mesh.EdgeCount*3],alphaVertices=new double[mesh.VertexCount*3];
    for(int v=0;v<mesh.VertexCount;v++)alphaVertices[3*v+2]=AlphaAt(mesh.GetVertexCoordinates(v).ToArray(),generator);
    for(int e=0;e<mesh.EdgeCount;e++)
    {
        var start=starts[e];var delta=displacements[e];var actualEnd=mesh.GetVertexCoordinates(mesh.Edges[e][1]).ToArray();
        var integrated=IntegrateFields(fields,alpha,start,delta,c);var reversed=IntegrateFields(fields,alpha,actualEnd,Scale(-1,delta),c);
        double[] shift=(double[])start.Clone();shift[0]+=3;shift[1]-=3;var shifted=IntegrateFields(fields,alpha,shift,delta,c);
        maxImaginaryResidual=System.Math.Max(maxImaginaryResidual,integrated.ImaginaryResidual);
        var numerical=NumericalIntegral(profile,generator,start,delta,c,gauss);
        maxIntegralError=System.Math.Max(maxIntegralError,System.Math.Max(Norm(Sub(integrated.P,numerical.P)),Norm(Sub(integrated.Direction,numerical.Direction))));
        maxReversalError=System.Math.Max(maxReversalError,System.Math.Max(Norm(Add(integrated.P,reversed.P)),Norm(Add(integrated.Direction,reversed.Direction))));
        maxPeriodicShiftError=System.Math.Max(maxPeriodicShiftError,System.Math.Max(Norm(Sub(integrated.P,shifted.P)),Norm(Sub(integrated.Direction,shifted.Direction))));
        maxExactDerivativeError=System.Math.Max(maxExactDerivativeError,System.Math.Abs(integrated.Direction[2]-c*(AlphaAt(actualEnd,generator)-AlphaAt(start,generator))));
        for(int k=0;k<3;k++){omega[3*e+k]=integrated.P[k];direction[3*e+k]=integrated.Direction[k];}
    }
    var evaluation=op.ComputeJointGradient(omega,thetaZero,mass);
    double omegaTerm=Dot(evaluation.GradOmega,direction),thetaTerm=Dot(evaluation.GradTheta,alphaVertices),ward=omegaTerm+thetaTerm;
    double denominator=1+Norm(evaluation.GradOmega)*Norm(direction)+Norm(evaluation.GradTheta)*Norm(alphaVertices);
    double ratio=System.Math.Abs(ward)/denominator;ratios.Add(ratio);
    var fdRows=new List<object>();bool rowDerivativePassed=true;
    foreach(double step in steps)
    {
        double plus=op.ComputeJointGradient(Add(omega,Scale(step,direction)),Scale(step,alphaVertices),mass).Objective;
        double minus=op.ComputeJointGradient(Add(omega,Scale(-step,direction)),Scale(-step,alphaVertices),mass).Objective;
        double centered=(plus-minus)/(2*step),error=System.Math.Abs(centered-ward)/(1+System.Math.Abs(ward));
        bool passed=double.IsFinite(centered)&&double.IsFinite(error)&&error<=dtol;rowDerivativePassed&=passed;
        maxDerivativeError=System.Math.Max(maxDerivativeError,error);fdRows.Add(new {step,plus,minus,centered,error,passed});
    }
    bool finite=double.IsFinite(evaluation.Objective)&&double.IsFinite(ward)&&double.IsFinite(ratio)
        &&evaluation.GradOmega.All(double.IsFinite)&&evaluation.GradTheta.All(double.IsFinite);
    derivativeControlsPassed&=rowDerivativePassed&&finite;
    if(profile==0){originCases++;originControlsPassed&=System.Math.Abs(evaluation.Objective)<=tol&&Norm(evaluation.GradOmega)<=tol&&Norm(evaluation.GradTheta)<=tol;}
    if(generator==0){globalCases++;globalControlsPassed&=finite&&ratio<=zeroBand;}
    string classification=ratio<=zeroBand?"zero-band":ratio>decisiveBand?"decisive-witness":"intermediate-unresolved";
    wardRows.Add(new {profile,generator,c,objective=evaluation.Objective,omegaTerm,thetaTerm,ward,denominator,ratio,classification,
        omegaNorm=Norm(omega),directionNorm=Norm(direction),thetaDirectionNorm=Norm(alphaVertices),rowDerivativePassed,finite,fdRows});
}
bool integralControlsPassed=elementaryIntegralControlsPassed&&maxIntegralError<=itol&&maxReversalError<=itol
    &&maxPeriodicShiftError<=itol&&maxExactDerivativeError<=itol&&maxImaginaryResidual<=itol;
bool mandatoryControlsPassed=compactControlsPassed&&topologyPassed&&integralControlsPassed&&originControlsPassed&&globalControlsPassed
    &&wardRows.Count==fx.GetProperty("expectedWardCases").GetInt32()&&globalCases==fx.GetProperty("expectedGlobalCases").GetInt32()
    &&originCases==fx.GetProperty("expectedOriginCases").GetInt32();
bool controlsPassed=mandatoryControlsPassed&&derivativeControlsPassed;
string outcome=Classify(ratios.ToArray(),zeroBand,decisiveBand);
string verdict=!mandatoryControlsPassed?precedence[2]:!derivativeControlsPassed?precedence[3]:outcome=="rejected"?Rejected:outcome=="unresolved"?precedence[5]:Survives;
Emit(verdict,new {
    knownAnswerPassed,controlsPassed,upstreamValid,sourceAnchorsPresent,sampledSliceAnchorsPresent,mandatoryControlsPassed,derivativeControlsPassed,
    compactLocalControls=new {compactControlsPassed,fpRows,fpError,fpStepAgreement,haarRows,dexpError,toyRows,toyJacobianError,toyInvariantError,toyWrongShiftMinimumDefect,
        densityRows,densityError,unitJacobianDoesNotSelectDensity=true,localDerivativeIdentityRequiresActionInvariance=false,
        interpretingDeterminantAsFpMeasureRequiresGaugePremises=true,globalSectionEstablished=false,firstJetGroupCompact=false},
    geometryControls=new {topologyPassed,mesh.VertexCount,mesh.EdgeCount,mesh.FaceCount,mesh.CellCount,seamEdgeCount,closureError},
    integralControls=new {integralControlsPassed,quadraturePolynomialError,trigError,periodIntegralError,equalEndpointCosineIntegral,
        maxIntegralError,maxReversalError,maxPeriodicShiftError,maxExactDerivativeError,maxImaginaryResidual,
        continuumDerivativeIntegratedExactly=true,naiveIndependentSiteProductRuleUsed=false},
    jointLift=new {caseCount=wardRows.Count,globalCases,originCases,originControlsPassed,globalControlsPassed,derivativeControlsPassed,maxDerivativeError,
        zeroBand,decisiveBand,zeroCount=ratios.Count(x=>x<=zeroBand),intermediateCount=ratios.Count(x=>x>zeroBand&&x<=decisiveBand),
        decisiveCount=ratios.Count(x=>x>decisiveBand),outcome,wardRows,
        testedMap="omega=p; deltaOmega=integral([p,alpha]+c*dalpha); deltaTheta=alpha(vertices); A0=0",
        literalProjectedCompletionRejected=controlsPassed&&outcome=="rejected",existentialWitnessDominatesIntermediateRows=true,
        exactFiniteGroupActionProved=false},
    boundaries=new {sectionBeforeDiscretizationRemainsOpen=true,offSliceFailureRejectsExistingSampledSlice=false,
        registeredThetaSliceChanged=false,sourceMeasureSelected=false,phase561SubstitutedOrOpened=false,
        invariantDensityMayBeArbitraryPositiveFunctionOfPhi=true},
    nextRequirement="Supply source-compatible carrier/pairing/reference and induced density plus global field class; a literal joint-lift Ward test neither supplies them nor rejects section-before-discretization."
});

void Emit(string verdict,object evidence)
{
    bool accepted=verdict is Rejected or Survives;
    var result=new {schemaVersion=1,phase=589,phaseId="phase589-section-density-joint-lift-audit",contractId=ContractId,
        contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,coreSourceTreeValid,coreFileCount,coreTreeSha256,
        bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,
        verdictKind=verdict,terminalStatus=verdict,auditPassed=accepted,evidence,deterministic=true,
        authorityFirewalls=forbidden.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
    string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true,NumberHandling=JsonNumberHandling.AllowNamedFloatingPointLiterals})+Environment.NewLine;
    Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/section_density_joint_lift_audit.json",json);
    File.WriteAllText(Root+"/output/section_density_joint_lift_audit_summary.json",json);
    Console.WriteLine($"Phase589 verdict: {verdict}");if(!accepted)Environment.ExitCode=1;
}
static string Sha(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string HashText(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories)
    .Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
static bool CheckUpstream(string path,string terminal)
{using var d=JsonDocument.Parse(File.ReadAllBytes(path));var r=d.RootElement;return r.GetProperty("auditPassed").GetBoolean()
    &&r.GetProperty("verdictKind").GetString()==terminal&&r.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;}
static string Classify(double[] ratios,double zero,double decisive)=>ratios.Any(x=>x>decisive)?"rejected":ratios.Any(x=>x>zero)?"unresolved":"survives";
static double[] Vector(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetDouble()).ToArray();
static double[] Add(double[] a,double[] b)=>a.Zip(b,(x,y)=>x+y).ToArray();
static double[] Sub(double[] a,double[] b)=>a.Zip(b,(x,y)=>x-y).ToArray();
static double[] Scale(double s,double[] a)=>a.Select(x=>s*x).ToArray();
static double Dot(double[] a,double[] b)=>a.Zip(b,(x,y)=>x*y).Sum();
static double Norm(double[] a)=>System.Math.Sqrt(Dot(a,a));
static double[] Lift(double[] start,double[] end)=>end.Select((x,i)=>x-start[i]-3*System.Math.Round((x-start[i])/3)).ToArray();
static double[,] Identity()=>new double[,]{{1,0,0},{0,1,0},{0,0,1}};
static double[,] Skew(double[] x)=>new double[,]{{0,-x[2],x[1]},{x[2],0,-x[0]},{-x[1],x[0],0}};
static double[,] MatrixScale(double c,double[,] a){var r=new double[3,3];for(int i=0;i<3;i++)for(int j=0;j<3;j++)r[i,j]=c*a[i,j];return r;}
static double[,] Transpose(double[,] a){var r=new double[3,3];for(int i=0;i<3;i++)for(int j=0;j<3;j++)r[i,j]=a[j,i];return r;}
static void AddTo(double[,] a,double[,] b,double c){for(int i=0;i<3;i++)for(int j=0;j<3;j++)a[i,j]+=c*b[i,j];}
static double[,] Multiply(double[,] a,double[,] b){var r=new double[3,3];for(int i=0;i<3;i++)for(int j=0;j<3;j++)for(int k=0;k<3;k++)r[i,j]+=a[i,k]*b[k,j];return r;}
static double[] Apply(double[,] a,double[] x)=>Enumerable.Range(0,3).Select(i=>Enumerable.Range(0,3).Sum(j=>a[i,j]*x[j])).ToArray();
static double MatrixError(double[,] a,double[,] b)=>Enumerable.Range(0,3).Max(i=>Enumerable.Range(0,3).Max(j=>System.Math.Abs(a[i,j]-b[i,j])));
static double Det(double[,] a)=>a[0,0]*(a[1,1]*a[2,2]-a[1,2]*a[2,1])-a[0,1]*(a[1,0]*a[2,2]-a[1,2]*a[2,0])+a[0,2]*(a[1,0]*a[2,1]-a[1,1]*a[2,0]);
static double[,] Rotation(double[] x)
{double r=Norm(x);var o=Identity();if(r==0)return o;var k=Skew(x);AddTo(o,k,System.Math.Sin(r)/r);AddTo(o,Multiply(k,k),(1-System.Math.Cos(r))/(r*r));return o;}
static double[] LogRotation(double[,] r)
{double[] v=[(r[2,1]-r[1,2])/2,(r[0,2]-r[2,0])/2,(r[1,0]-r[0,1])/2];double s=Norm(v);if(s==0)return new double[3];return Scale(System.Math.Atan2(s,(r[0,0]+r[1,1]+r[2,2]-1)/2)/s,v);}
static (double X,double Weight)[] GaussLegendre(int n,int iterations)
{
    var result=new (double X,double Weight)[n];
    for(int i=0;i<n;i++)
    {
        double z=System.Math.Cos(System.Math.PI*(i+0.75)/(n+0.5)),derivative=0;
        for(int iteration=0;iteration<iterations;iteration++)
        {double p0=1,p1=z;for(int k=2;k<=n;k++){double next=((2*k-1)*z*p1-(k-1)*p0)/k;p0=p1;p1=next;}
         derivative=n*(z*p1-p0)/(z*z-1);z-=p1/derivative;}
        // Recompute the derivative at the final node; no stale Newton weight.
        double a=1,b=z;for(int k=2;k<=n;k++){double next=((2*k-1)*z*b-(k-1)*a)/k;a=b;b=next;}
        derivative=n*(z*b-a)/(z*z-1);result[i]=((1+z)/2,1/((1-z*z)*derivative*derivative));
    }
    return result;
}
static (double Z,double Second) GaussianIntegrals(double precision,double bound,int panels)
{
    double step=2*bound/panels,z=0,second=0;
    for(int i=0;i<=panels;i++){double x=-bound+i*step,w=i is 0||i==panels?1:i%2==0?2:4;
        double y=w*System.Math.Exp(-precision*x*x/2);z+=y;second+=x*x*y;}
    return (z*step/3,second*step/3);
}
// Finite Fourier series in (x0,x1): each entry denotes coefficient*exp(i*k*(m0*x0+m1*x1)).
static FourierTerm[] Mode(int m0,int m1,Complex coefficient)=>[new(m0,m1,coefficient)];
static FourierTerm[] Constant(double a)=>Mode(0,0,a);
static FourierTerm[] SinMode(int m0,int m1)=>[new(m0,m1,new Complex(0,-0.5)),new(-m0,-m1,new Complex(0,0.5))];
static FourierTerm[] CosMode(int m0,int m1)=>[new(m0,m1,0.5),new(-m0,-m1,0.5)];
static FourierTerm[] ScaleModes(double a,FourierTerm[] x)=>x.Select(t=>new FourierTerm(t.M0,t.M1,a*t.Coefficient)).ToArray();
static FourierTerm[] SumModes(FourierTerm[] a,FourierTerm[] b)=>a.Concat(b).ToArray();
static FourierTerm[] ProductModes(FourierTerm[] a,FourierTerm[] b)=>(from x in a from y in b select new FourierTerm(x.M0+y.M0,x.M1+y.M1,x.Coefficient*y.Coefficient)).ToArray();
static FourierTerm[] DerivativeModes(FourierTerm[] a,int axis)=>a.Select(t=>new FourierTerm(t.M0,t.M1,t.Coefficient*Complex.ImaginaryOne*(2*System.Math.PI/3)*(axis==0?t.M0:axis==1?t.M1:0))).ToArray();
static FourierTerm[][][] ProfileModes(int profile)
{
    var p=Enumerable.Range(0,4).Select(_=>Enumerable.Range(0,3).Select(_=>Array.Empty<FourierTerm>()).ToArray()).ToArray();
    if(profile>=1)p[0][0]=Constant(0.125);
    if(profile==2){p[0][1]=ScaleModes(1.0/16,CosMode(1,0));p[1][1]=Constant(0.25);p[1][2]=ScaleModes(0.125,SinMode(1,0));}
    return p;
}
static Complex Integrate(FourierTerm[] terms,double[] start,double[] delta)
{
    Complex sum=Complex.Zero;double k=2*System.Math.PI/3;
    foreach(var t in terms){double frequency=k*(t.M0*delta[0]+t.M1*delta[1]);double half=frequency/2;
        double sinc=half==0?1:System.Math.Sin(half)/half;
        double phase=k*(t.M0*(start[0]+delta[0]/2)+t.M1*(start[1]+delta[1]/2));
        sum+=t.Coefficient*Complex.Exp(Complex.ImaginaryOne*phase)*sinc;}
    return sum;
}
static (double[] P,double[] Direction,double ImaginaryResidual) IntegrateFields(FourierTerm[][][] p,FourierTerm[] alpha,double[] start,double[] delta,int c)
{
    Complex[] omega=new Complex[3],direction=new Complex[3];
    for(int mu=0;mu<4;mu++)
    {
        for(int a=0;a<3;a++)omega[a]+=delta[mu]*Integrate(p[mu][a],start,delta);
        direction[0]+=delta[mu]*Integrate(ProductModes(p[mu][1],alpha),start,delta);
        direction[1]-=delta[mu]*Integrate(ProductModes(p[mu][0],alpha),start,delta);
        direction[2]+=c*delta[mu]*Integrate(DerivativeModes(alpha,mu),start,delta);
    }
    return (omega.Select(z=>z.Real).ToArray(),direction.Select(z=>z.Real).ToArray(),omega.Concat(direction).Max(z=>System.Math.Abs(z.Imaginary)));
}
static double AlphaAt(double[] x,int generator)=>generator==0?1:System.Math.Sin(2*System.Math.PI*x[0]/3)+System.Math.Cos(2*System.Math.PI*x[1]/3)/2;
static (double[] P,double[] Direction) NumericalIntegral(int profile,int generator,double[] start,double[] delta,int c,(double X,double Weight)[] nodes)
{
    double[] omega=new double[3],direction=new double[3];double k=2*System.Math.PI/3;
    foreach(var q in nodes)
    {
        var x=Add(start,Scale(q.X,delta));double[] oneForm=new double[3];
        if(profile>=1)oneForm[0]=delta[0]/8;
        if(profile==2){oneForm[1]=delta[0]*System.Math.Cos(k*x[0])/16+delta[1]/4;oneForm[2]=delta[1]*System.Math.Sin(k*x[0])/8;}
        double alpha=AlphaAt(x,generator),dalpha=generator==0?0:k*(delta[0]*System.Math.Cos(k*x[0])-delta[1]*System.Math.Sin(k*x[1])/2);
        double[] variation=[oneForm[1]*alpha,-oneForm[0]*alpha,c*dalpha];
        omega=Add(omega,Scale(q.Weight,oneForm));direction=Add(direction,Scale(q.Weight,variation));
    }
    return (omega,direction);
}
sealed record FourierTerm(int M0,int M1,Complex Coefficient);
sealed class Binding(string idValue,string pathValue,string hashValue)
{
    public string id{get;}=idValue;public string path{get;}=pathValue;public string sha256{get;}=hashValue;
    public bool hashMatches=>File.Exists(path)&&Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant()==sha256;
}
