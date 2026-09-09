using System.Security.Cryptography;
using System.Text.Json;

const string Root="studies/phase585_quotient_section_measure_controls_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string SourcePath="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt";
const string PriorPath="studies/phase583_biconnection_convention_reconciliation_001/output/biconnection_convention_reconciliation_summary.json";
const string PilotPath="studies/phase548_bounded_complete_lattice_pilot_execution_001/Program.cs";
const string PackPath="studies/phase577_chain_pack_execution_001/Program.cs";
const string Success="conditional-section-measure-controls-pass-source-regulator-unresolved";
string[] forbidden=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged",
    "registeredMeasureSelected","physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened",
    "o4Discharged","phase458Satisfied","phase481Changed","samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
var fixtures=new {
    signs=new[]{-1,1},rotations="all 24 proper signed permutation 3x3 matrices",
    referenceLieVector=new[]{1,-2,1},pLieVector=new[]{2,1,-1},
    epsilonCovariantDerivativeLieVectors=new[]{new[]{1,2,-1},new[]{-2,1,3},new[]{3,-1,2}},
    hCovariantDerivativeLieVector=new[]{2,-1,3},leftOrdinaryDerivativeLieVector=new[]{2,-1,1},
    toyBackgroundLieVector=new[]{1,0,0},graphEndpointRotation="positive quarter turn about z at site 1; identity at site 0",
    cutoffPoint=new[]{-1,0,0},cutoffHalfWidth=1,expectedJetCaseCount=3456,expectedToyCocycleCaseCount=576,
    expectedJacobianCaseCount=48,exactTolerance=0,numericalDerivativeStep=1.0e-5,numericalDeterminantTolerance=1.0e-8,numericalJacobianEntryTolerance=1.0e-8,
    graphProductRuleExpectedMaxResidual=2,graphCocycleExpectedMaxResidual=2,projectedJacobianExpectedDeterminant=0,
    shiftedCutoffExpectedPoint=new[]{1,-2,0}
};
using var contractDoc=JsonDocument.Parse(File.ReadAllBytes(ContractPath));
var contract=contractDoc.RootElement;
string[] expectedIds=["program","project","primary-source","prior-sign-controls","sampled-pilot-slice","sampled-pack-slice","build-properties"];
string[] expectedPaths=[Root+"/Program.cs",Root+"/Phase585QuotientSectionMeasureControls.csproj",SourcePath,PriorPath,PilotPath,PackPath,"Directory.Build.props"];
var bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(x=>new {
    id=x.GetProperty("id").GetString()!,path=x.GetProperty("path").GetString()!,sha256=x.GetProperty("sha256").GetString()!
}).Select(x=>new {x.id,x.path,x.sha256,hashMatches=File.Exists(x.path)&&Sha(x.path)==x.sha256}).ToArray();
bool fixturesValid=JsonElement.DeepEquals(contract.GetProperty("fixtures"),JsonSerializer.SerializeToElement(fixtures));
bool contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==585
    &&contract.GetProperty("contractId").GetString()=="phase585-a46-quotient-section-measure-v1"
    &&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()&&fixturesValid
    &&contract.GetProperty("arithmetic").GetString()=="checked-int64-exact-and-declared-double-jacobian-control"
    &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
    &&forbidden.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)
    &&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
    &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(new[]{
        "invalid-or-drifted-input","known-answer-battery-failed","mathematical-control-failed",Success});
bool exactBindingsValid=bindings.Length==expectedIds.Length&&bindings.Select(x=>x.id).Distinct().Count()==expectedIds.Length
    &&bindings.Select(x=>x.path).Distinct().Count()==expectedIds.Length
    &&bindings.Select((x,i)=>x.id==expectedIds[i]&&x.path==expectedPaths[i]&&x.hashMatches).All(x=>x);
if(!contractValid||!exactBindingsValid){Emit("invalid-or-drifted-input",new {fixturesValid});return;}

long[,] identity={{1,0,0},{0,1,0},{0,0,1}};
long[,] quarter={{0,-1,0},{1,0,0},{0,0,1}};
var rotations=Rotations();
double[,] determinantFixture={{2,1,0},{1,3,1},{0,1,2}};
bool knownAnswerPassed=Eq(Com(Skew(1,0,0),Skew(0,1,0)),Skew(0,0,1))&&rotations.Count==24
    &&rotations.Select(Key).Distinct().Count()==24&&rotations.All(r=>Det(r)==1&&Eq(Mul(Tr(r),r),identity))
    &&rotations.All(r=>rotations.All(s=>rotations.Any(t=>Eq(t,Mul(r,s)))))
    &&DetPermutation(determinantFixture)==8&&DetElimination(determinantFixture)==8
    &&DetElimination(new double[,]{{0,1},{1,0}})==-1&&DetElimination(new double[,]{{1,1},{1,1}})==0;
if(!knownAnswerPassed){Emit("known-answer-battery-failed",new {knownAnswerPassed});return;}
using var prior=JsonDocument.Parse(File.ReadAllBytes(PriorPath));
string source=File.ReadAllText(SourcePath);
bool upstreamValid=prior.RootElement.GetProperty("auditPassed").GetBoolean()
    &&prior.RootElement.GetProperty("verdictKind").GetString()=="printed-sign-conflict-proved-two-compatible-families-registered-map-unresolved"
    &&new[]{"(6.2)","(6.11)","(6.18)","(6.20)"}.All(source.Contains);
bool sampledSliceAnchorsPresent=File.ReadAllText(PilotPath).Contains("double[] thetaZero = new double[mesh.VertexCount * algebra.Dimension];",StringComparison.Ordinal)
    &&File.ReadAllText(PilotPath).Contains("op.ComputeJointGradient(omega, thetaZero, massMatrix)",StringComparison.Ordinal)
    &&File.ReadAllText(PackPath).Contains("var thetaZero = new double[mesh.VertexCount * dimG];",StringComparison.Ordinal)
    &&File.ReadAllText(PackPath).Contains("op.ComputeJointGradient(omega, thetaZero, mass)",StringComparison.Ordinal);
upstreamValid&=sampledSliceAnchorsPresent;
if(!upstreamValid){Emit("invalid-or-drifted-input",new {knownAnswerPassed,upstreamValid});return;}

var a0=Skew(1,-2,1);var p=Skew(2,1,-1);var ch=Skew(2,-1,3);
long[][,] derivatives=[Skew(1,2,-1),Skew(-2,1,3),Skew(3,-1,2)];
long sectionResidual=0,rightResidual=0,leftResidual=0,backgroundResidual=0,cocycleResidual=0,inverseDerivativeResidual=0;
int jetCaseCount=0;
foreach(int c in fixtures.signs)foreach(var e in rotations)foreach(var h in rotations)foreach(var ce in derivatives)
{
    jetCaseCount++;
    var de=Sub(Add(Mul(e,ce),Mul(e,a0)),Mul(a0,e));
    var dh=Sub(Add(Mul(h,ch),Mul(h,a0)),Mul(a0,h));
    var phi=Phi(c,p,e,de,a0);var eh=Mul(e,h);var deh=Add(Mul(de,h),Mul(e,dh));
    var movedP=Add(AdInv(h,p),Scale(c,Mul(Tr(h),D0(h,dh,a0))));
    rightResidual=Max(rightResidual,Norm(Sub(Phi(c,movedP,eh,deh,a0),phi)));
    cocycleResidual=Max(cocycleResidual,Norm(Sub(Mul(Tr(eh),D0(eh,deh,a0)),Add(AdInv(h,ce),ch))));
    // Inverse first jet is constructed from the ordinary inverse derivative identity.
    var inverse=Tr(e);var inverseD=Scale(-1,Mul(Mul(inverse,de),inverse));
    var es=Mul(e,inverse);var des=Add(Mul(de,inverse),Mul(e,inverseD));
    var sectionP=Add(AdInv(inverse,p),Scale(c,Mul(Tr(inverse),D0(inverse,inverseD,a0))));
    var sectionB=Add(a0,Mul(Tr(es),D0(es,des,a0)));
    sectionResidual=Max(sectionResidual,Norm(Sub(es,identity)),Norm(Sub(sectionP,Scale(c,phi))),Norm(Sub(sectionB,a0)));
    inverseDerivativeResidual=Max(inverseDerivativeResidual,Norm(des));
    // Left seed (k,0) sends e to ke, p unchanged, A0 held fixed.
    var k=h;var dk=Mul(k,Skew(2,-1,1));var ke=Mul(k,e);var dke=Add(Mul(dk,e),Mul(k,de));
    var expectedLeft=Sub(Ad(k,phi),Mul(D0(k,dk,a0),Tr(k)));
    leftResidual=Max(leftResidual,Norm(Sub(Phi(c,p,ke,dke,a0),expectedLeft)));
    // A change of frame ALSO changes A0 and the ordinary derivative of e.
    var frameA0=Sub(Ad(k,a0),Mul(dk,Tr(k)));var frameE=Ad(k,e);
    var frameDe=Sub(Add(Mul(Mul(dk,e),Tr(k)),Ad(k,de)),Mul(Mul(frameE,dk),Tr(k)));
    backgroundResidual=Max(backgroundResidual,Norm(Sub(Phi(c,Ad(k,p),frameE,frameDe,frameA0),Ad(k,phi))));
}

// Compact SO(3) toy only: C_h=Ad(h^-1)B-B, i.e. d_B h=[B,h].
// It has no independent spatial derivative and is not a source-field regulator.
var toyB=Skew(1,0,0);long toyCocycleResidual=0,toyAverageResidual=0;int toyCocycleCaseCount=0;
var averageNumerator=new long[3,3];
foreach(var e in rotations)
{
    averageNumerator=Add(averageNumerator,Sub(AdInv(e,toyB),toyB));
    foreach(var h in rotations)
    {
        toyCocycleCaseCount++;
        toyCocycleResidual=Max(toyCocycleResidual,Norm(Sub(Sub(AdInv(Mul(e,h),toyB),toyB),
            Add(AdInv(h,Sub(AdInv(e,toyB),toyB)),Sub(AdInv(h,toyB),toyB)))));
    }
}
toyAverageResidual=Norm(Add(averageNumerator,Scale(24,toyB)));

// Independent determinant controls evaluate the complete triangular coordinate map.
// Phi=c R p+B-R B, with e(t)=e Rz(tz) Ry(ty) Rx(tx), output (Phi,t).
double exactJacobianResidual=0,numericJacobianResidual=0,numericJacobianEntryResidual=0,jetJacobianResidual=0;int jacobianCaseCount=0;
var jacobianRows=new List<object>();
foreach(int c in fixtures.signs)foreach(var e in rotations)
{
    jacobianCaseCount++;
    var j=new double[6,6];var jetJ=new double[9,9];
    long[] pv=[2,1,-1],bv=[1,0,0],cv=[1,2,-1];
    long[] toyV=pv.Select((x,i)=>c*x-bv[i]).ToArray();long[] jetV=pv.Select((x,i)=>c*x-cv[i]).ToArray();
    for(int col=0;col<3;col++)
    {
        long[] axis=new long[3];axis[col]=1;
        var tangent=VecMul(e,Cross(axis,toyV));var jetTangent=VecMul(e,Cross(axis,jetV));
        for(int row=0;row<3;row++)
        {j[row,col]=c*e[row,col];j[row,col+3]=tangent[row];
         jetJ[row,col]=c*e[row,col];jetJ[row,col+3]=jetTangent[row];jetJ[row,col+6]=-e[row,col];}
        j[col+3,col+3]=1;jetJ[col+3,col+3]=1;jetJ[col+6,col+6]=1;
    }
    double permutationDeterminant=DetPermutation(j),eliminationDeterminant=DetElimination(j);
    double jetDeterminant=DetElimination(jetJ);
    var numericJ=new double[6,6];
    for(int col=0;col<6;col++)
    {
        var plus=new double[6];var minus=new double[6];plus[col]=fixtures.numericalDerivativeStep;minus[col]=-fixtures.numericalDerivativeStep;
        var a=ToyMap(plus,e,c);var b=ToyMap(minus,e,c);
        for(int row=0;row<6;row++)
        {numericJ[row,col]=(a[row]-b[row])/(2*fixtures.numericalDerivativeStep);
         numericJacobianEntryResidual=System.Math.Max(numericJacobianEntryResidual,System.Math.Abs(numericJ[row,col]-j[row,col]));}
    }
    double numericalDeterminant=DetElimination(numericJ);
    exactJacobianResidual=System.Math.Max(exactJacobianResidual,System.Math.Max(System.Math.Abs(permutationDeterminant-c),System.Math.Abs(eliminationDeterminant-c)));
    jetJacobianResidual=System.Math.Max(jetJacobianResidual,System.Math.Abs(jetDeterminant-c));
    numericJacobianResidual=System.Math.Max(numericJacobianResidual,System.Math.Abs(numericalDeterminant-c));
    jacobianRows.Add(new {c,rotation=Key(e),permutationDeterminant,eliminationDeterminant,numericalDeterminant,jetDeterminant});
}

// Retaining only the x coefficient is not closed under this rotation.
long projectedJacobianDeterminant=quarter[0,0];long closureLeak=VecMul(quarter,new long[]{1,0,0})[1];
long[] cutoffP=[-1,0,0],cutoffB=[1,0,0];
var cutoffPhi=VecAdd(cutoffB,VecMul(quarter,VecSub(cutoffP,cutoffB)));
var restoredP=VecAdd(cutoffB,VecMul(Tr(quarter),VecSub(cutoffPhi,cutoffB)));
bool cutoffDecoyPassed=cutoffP.All(x=>System.Math.Abs(x)<=1)&&cutoffPhi.SequenceEqual(fixtures.shiftedCutoffExpectedPoint.Select(x=>(long)x))
    &&cutoffPhi.Any(x=>System.Math.Abs(x)>1)&&restoredP.SequenceEqual(cutoffP);

// Independent compact site values h0=k0=I, h1=k1=quarter. Df=f1-f0.
// The ordinary same-site Leibniz rule and the source cocycle both fail.
var dhGraph=Sub(quarter,identity);var dkGraph=Sub(quarter,identity);
var dhkGraph=Sub(Mul(quarter,quarter),identity);
long graphProductRuleResidual=Norm(Sub(dhkGraph,Add(dhGraph,dkGraph)));
long graphCocycleResidual=Norm(Sub(dhkGraph,Add(AdInv(identity,dhGraph),dkGraph)));
long twistedProductRuleResidual=Norm(Sub(dhkGraph,Add(Mul(dhGraph,quarter),dkGraph)));
long graphLieAlgebraResidual=Norm(Add(dhGraph,Tr(dhGraph)));
bool controlsPassed=jetCaseCount==fixtures.expectedJetCaseCount&&toyCocycleCaseCount==fixtures.expectedToyCocycleCaseCount
    &&jacobianCaseCount==fixtures.expectedJacobianCaseCount&&Max(sectionResidual,rightResidual,leftResidual,backgroundResidual,
        cocycleResidual,inverseDerivativeResidual,toyCocycleResidual,toyAverageResidual)==0
    &&exactJacobianResidual==0&&jetJacobianResidual==0&&numericJacobianResidual<=fixtures.numericalDeterminantTolerance
    &&numericJacobianEntryResidual<=fixtures.numericalJacobianEntryTolerance
    &&projectedJacobianDeterminant==fixtures.projectedJacobianExpectedDeterminant&&closureLeak==1&&cutoffDecoyPassed
    &&graphProductRuleResidual==fixtures.graphProductRuleExpectedMaxResidual&&graphCocycleResidual==fixtures.graphCocycleExpectedMaxResidual
    &&twistedProductRuleResidual==0&&graphLieAlgebraResidual==2;
Emit(controlsPassed?Success:"mathematical-control-failed",new {
    knownAnswerPassed,controlsPassed,upstreamValid,fixturesValid,
    firstJetControls=new {jetCaseCount,sectionResidual,rightResidual,leftResidual,backgroundResidual,cocycleResidual,inverseDerivativeResidual,
        sectionFormula="h=epsilon^-1; p_section=c*Phi; B_section=A0; Phi=c*Ad(epsilon)p-(d0 epsilon)epsilon^-1",
        uniqueSectionConditionalOnAllowedInverse=true,fullFirstJetGroupCompact=false,normalizedHaarOnDerivativeFibersAvailable=false,
        rightActionIsRedundancy=true,leftActionSymmetryEstablished=false},
    compactToy=new {toyCocycleCaseCount,toyCocycleResidual,toyAverageResidual,formula="C_h=Ad(h^-1)B-B",
        compactGroupCocyclesAreCoboundariesUnderContinuityAndFiniteDimension=true,sourceSpatialDerivativeImplemented=false,
        finiteRotationCensusIsHaarIntegration=false},
    finiteJacobian=new {jacobianCaseCount,exactJacobianResidual,numericJacobianResidual,numericJacobianEntryResidual,jetJacobianResidual,jacobianRows,
        retainedDimension=3,compactToyCoordinateDimension=6,firstJetCoordinateDimension=9,
        fullTriangularJacobianEvaluated=true,conditionalLebesguePreservation=true,continuumFunctionalDeterminantEstablished=false,
        normalizedCompactToyHaarMayIntegrateOutOnlyForDescendingIntegrands=true,exponentiatedCoefficientsAreHaar=false},
    adversarialControls=new {projectedJacobianDeterminant,closureLeak,cutoffPhi,restoredP,cutoffDecoyPassed,
        graphProductRuleResidual,graphCocycleResidual,twistedProductRuleResidual,graphLieAlgebraResidual,
        naiveIndependentSiteDifferenceRejected=true,allPossibleRegulatorsExcluded=false},
    sampledSliceBoundary=new {sampledSliceAnchorsPresent,phase548And577AlreadyFreezeThetaZero=true,offSliceThetaDependenceRejectsSampledSlice=false,
        sourceActionDescentEstablished=false,registeredMeasureSelected=false},
    nextRequirement="Supply an exact source-compatible regulator, allowed global section, retained-space and domain control, and descending action before identifying the registered measure."
});

void Emit(string verdict,object evidence)
{
    var result=new {schemaVersion=1,phase=585,phaseId="phase585-quotient-section-measure-controls",
        contractId="phase585-a46-quotient-section-measure-v1",contractSha256=Sha(ContractPath),contractValid,exactBindingsValid,
        bindingCount=bindings.Length,uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),
        uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,
        verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
        authorityFirewalls=forbidden.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
    string json=JsonSerializer.Serialize(result,new JsonSerializerOptions{WriteIndented=true})+Environment.NewLine;
    Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/quotient_section_measure_controls.json",json);
    File.WriteAllText(Root+"/output/quotient_section_measure_controls_summary.json",json);
    Console.WriteLine($"Phase585 verdict: {verdict}");if(verdict!=Success)Environment.ExitCode=1;
}
static string Sha(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static long[,] Skew(long x,long y,long z)=>new long[,]{{0,-z,y},{z,0,-x},{-y,x,0}};
static long[,] Add(long[,] a,long[,] b){var o=new long[3,3];for(int i=0;i<3;i++)for(int j=0;j<3;j++)o[i,j]=checked(a[i,j]+b[i,j]);return o;}
static long[,] Scale(long s,long[,] a){var o=new long[3,3];for(int i=0;i<3;i++)for(int j=0;j<3;j++)o[i,j]=checked(s*a[i,j]);return o;}
static long[,] Sub(long[,] a,long[,] b)=>Add(a,Scale(-1,b));
static long[,] Mul(long[,] a,long[,] b){var o=new long[3,3];for(int i=0;i<3;i++)for(int j=0;j<3;j++)for(int k=0;k<3;k++)o[i,j]=checked(o[i,j]+a[i,k]*b[k,j]);return o;}
static long[,] Tr(long[,] a){var o=new long[3,3];for(int i=0;i<3;i++)for(int j=0;j<3;j++)o[i,j]=a[j,i];return o;}
static long Norm(long[,] a)=>a.Cast<long>().Max(x=>System.Math.Abs(x));
static bool Eq(long[,] a,long[,] b)=>Norm(Sub(a,b))==0;
static string Key(long[,] a)=>string.Join(",",a.Cast<long>());
static long Max(params long[] x)=>x.Max();
static long Det(long[,] a)=>checked(a[0,0]*(a[1,1]*a[2,2]-a[1,2]*a[2,1])-a[0,1]*(a[1,0]*a[2,2]-a[1,2]*a[2,0])+a[0,2]*(a[1,0]*a[2,1]-a[1,1]*a[2,0]));
static long[,] Ad(long[,] h,long[,] a)=>Mul(Mul(h,a),Tr(h));
static long[,] AdInv(long[,] h,long[,] a)=>Ad(Tr(h),a);
static long[,] Com(long[,] a,long[,] b)=>Sub(Mul(a,b),Mul(b,a));
static long[,] D0(long[,] h,long[,] dh,long[,] a0)=>Sub(Add(dh,Mul(a0,h)),Mul(h,a0));
static long[,] Phi(int c,long[,] p,long[,] e,long[,] de,long[,] a0)=>Sub(Scale(c,Ad(e,p)),Mul(D0(e,de,a0),Tr(e)));
static long[] VecMul(long[,] r,long[] v)=>Enumerable.Range(0,3).Select(i=>checked(r[i,0]*v[0]+r[i,1]*v[1]+r[i,2]*v[2])).ToArray();
static long[] VecAdd(long[] a,long[] b)=>a.Select((x,i)=>checked(x+b[i])).ToArray();
static long[] VecSub(long[] a,long[] b)=>a.Select((x,i)=>checked(x-b[i])).ToArray();
static long[] Cross(long[] a,long[] b)=>new[]{checked(a[1]*b[2]-a[2]*b[1]),checked(a[2]*b[0]-a[0]*b[2]),checked(a[0]*b[1]-a[1]*b[0])};
static List<long[,]> Rotations()
{
    var o=new List<long[,]>();for(int i=0;i<3;i++)for(int j=0;j<3;j++)for(int k=0;k<3;k++)
    {if(i==j||i==k||j==k)continue;foreach(int a in new[]{-1,1})foreach(int b in new[]{-1,1})foreach(int c in new[]{-1,1})
    {var r=new long[3,3];r[0,i]=a;r[1,j]=b;r[2,k]=c;if(Det(r)==1)o.Add(r);}}return o;
}
static double DetPermutation(double[,] a)
{
    int n=a.GetLength(0);var used=new bool[n];var selected=new int[n];double sum=0;
    void Expand(int row,double product)
    {if(row==n){int inversions=0;for(int i=0;i<n;i++)for(int j=i+1;j<n;j++)if(selected[i]>selected[j])inversions++;
        sum+=(inversions%2==0?1:-1)*product;return;}
     for(int col=0;col<n;col++){if(used[col])continue;used[col]=true;selected[row]=col;Expand(row+1,product*a[row,col]);used[col]=false;}}
    Expand(0,1);return sum;
}
static double DetElimination(double[,] input)
{
    var a=(double[,])input.Clone();int n=a.GetLength(0);double determinant=1;
    for(int col=0;col<n;col++)
    {int pivot=col;for(int row=col+1;row<n;row++)if(System.Math.Abs(a[row,col])>System.Math.Abs(a[pivot,col]))pivot=row;
     if(a[pivot,col]==0)return 0;if(pivot!=col){for(int j=0;j<n;j++)(a[col,j],a[pivot,j])=(a[pivot,j],a[col,j]);determinant=-determinant;}
     determinant*=a[col,col];for(int row=col+1;row<n;row++){double ratio=a[row,col]/a[col,col];for(int j=col+1;j<n;j++)a[row,j]-=ratio*a[col,j];}}
    return determinant;
}
static double[] ToyMap(double[] x,long[,] e,int c)
{
    double[] v=[c*(2+x[0])-1,c*(1+x[1]),c*(-1+x[2])];
    for(int axis=0;axis<3;axis++)
    {int j=(axis+1)%3,k=(axis+2)%3;double co=System.Math.Cos(x[axis+3]),si=System.Math.Sin(x[axis+3]);
     (v[j],v[k])=(co*v[j]-si*v[k],si*v[j]+co*v[k]);}
    return new[]{1+e[0,0]*v[0]+e[0,1]*v[1]+e[0,2]*v[2],e[1,0]*v[0]+e[1,1]*v[1]+e[1,2]*v[2],
        e[2,0]*v[0]+e[2,1]*v[1]+e[2,2]*v[2],x[3],x[4],x[5]};
}
