using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root="studies/phase587_exact_residual_factorization_audit_001";
const string ContractPath=Root+"/preregistration/contract_v1.json";
const string ContractId="phase587-a47-exact-residual-factorization-v1";
const string Success="nonlinear-residual-factorization-obstructed-flat-tangent-control-passes";
const string Source="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt";
const string Prior="studies/phase586_action_restriction_pairing_controls_001/output/action_restriction_pairing_controls_summary.json";
const string FixtureJson="""
{"vertices":[[0,0,0,0],[1,0,0,0],[0,1,0,0],[0,0,1,0],[0,0,0,1]],"cell":[0,1,2,3,4],"edges":[[0,1],[0,2],[0,3],[0,4],[1,2],[1,3],[1,4],[2,3],[2,4],[3,4]],"faces":[[0,1,2],[0,1,3],[0,1,4],[0,2,3],[0,2,4],[0,3,4],[1,2,3],[1,2,4],[1,3,4],[2,3,4]],"boundaryEdges":[[0,1,4],[0,2,5],[0,3,6],[1,2,7],[1,3,8],[2,3,9],[4,5,7],[4,6,8],[5,6,9],[7,8,9]],"boundarySigns":[1,-1,1],"formPairs":[[0,1],[0,2],[0,3],[1,2],[1,3],[2,3]],"starRows":[[0,0,0,0,0,1],[0,0,0,0,-1,0],[0,0,0,1,0,0],[0,0,1,0,0,0],[0,-1,0,0,0,0],[1,0,0,0,0,0]],"generatorCycles":[[0,1,2],[1,2,0],[2,0,1]],"witnessFaceCoefficients":[-1,1,0,-1,0,0,1,0,0,0],"witnessLieIndex":2,"mixedCoefficient":"-1/2","member":{"phi1":"sd2","phi2":"id0","coefficient":0.5,"theta":0},"expected":{"wRank":6,"qRank":6,"pRank":6,"dRank":6,"mRank":7,"linearResidualRank":3,"mixedRankWithLie":30,"noncommutingCases":30,"commutingCases":30,"actualLinearColumns":30,"actualContractionColumns":30,"witnessNormSquared":4,"coreFileCount":726},"exactTolerance":0,"comparisonTolerance":1e-12,"estimatedSeconds":5,"maximumEstimatedSeconds":10,"estimatedPeakBytes":134217728,"maximumEstimatedPeakBytes":268435456}
""";
string[] forbidden=["authorIntentSelected","registeredTransformationBridgeEstablished","registeredActionChanged","registeredMeasureSelected",
    "physicalHiggsIdentified","sourceContractApplicationAllowed","phase561Opened","o4Discharged","phase458Satisfied","phase481Changed",
    "samplingPerformed","samplingAuthorized","productionAuthorized","gevClaimAllowed"];
string[] terminalMenu=["invalid-or-drifted-input","known-answer-control-failed","registered-map-or-polynomial-control-failed",Success,"expected-factorization-obstruction-not-confirmed"];
string[] expectedIds=["program","project","study","primary-source","phase586-summary","core-source-manifest","build-props"];
string[] expectedPaths=[Root+"/Program.cs",Root+"/Phase587ExactResidualFactorizationAudit.csproj",Root+"/STUDY.md",Source,Prior,
    Root+"/preregistration/core_source_manifest_v1.json","Directory.Build.props"];
using var document=JsonDocument.Parse(File.ReadAllBytes(ContractPath));
var contract=document.RootElement;
var bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(x=>new
    {id=x.GetProperty("id").GetString()!,path=x.GetProperty("path").GetString()!,sha256=x.GetProperty("sha256").GetString()!})
    .Select(x=>new {x.id,x.path,x.sha256,hashMatches=File.Exists(x.path)&&Sha(x.path)==x.sha256}).ToArray();
bool coreSourceTreeValid=false;
bool contractValid=contract.GetProperty("schemaVersion").GetInt32()==1&&contract.GetProperty("phase").GetInt32()==587
    &&contract.GetProperty("contractId").GetString()==ContractId&&contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    &&contract.GetProperty("deterministicZeroSampling").GetBoolean()
    &&JsonNode.DeepEquals(JsonNode.Parse(FixtureJson),JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()))
    &&contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(terminalMenu)
    &&contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==14
    &&forbidden.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False)
    &&contract.GetProperty("externalReviewPending").GetBoolean()&&contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
bool exactBindingsValid=bindings.Length==expectedIds.Length&&bindings.Select(x=>x.id).Distinct().Count()==bindings.Length
    &&bindings.Select(x=>x.path).Distinct().Count()==bindings.Length&&bindings.All(x=>x.hashMatches)
    &&expectedIds.Zip(expectedPaths).All(x=>bindings.Any(b=>b.id==x.First&&b.path==x.Second));
if(!contractValid||!exactBindingsValid){Emit(terminalMenu[0],new {knownAnswerPassed=false,controlsPassed=false});return;}
var fx=contract.GetProperty("fixtures");var expected=fx.GetProperty("expected");
using var coreDocument=JsonDocument.Parse(File.ReadAllBytes(expectedPaths[5]));
var core=coreDocument.RootElement;var coreFiles=core.GetProperty("files").EnumerateArray().ToArray();
coreSourceTreeValid=core.GetProperty("schemaVersion").GetInt32()==1
    &&core.GetProperty("algorithm").GetString()=="sha256-sorted-path-space-sha256-newline-cs-csproj-excluding-bin-obj-v1"
    &&coreFiles.Length==expected.GetProperty("coreFileCount").GetInt32()
    &&coreFiles.Select(x=>x.GetProperty("path").GetString()).SequenceEqual(CorePaths())
    &&coreFiles.All(x=>Sha(x.GetProperty("path").GetString()!)==x.GetProperty("sha256").GetString())
    &&TreeSha()==core.GetProperty("treeSha256").GetString();
bool resourceAccepted=fx.GetProperty("estimatedSeconds").GetDouble()<=fx.GetProperty("maximumEstimatedSeconds").GetDouble()
    &&fx.GetProperty("estimatedPeakBytes").GetInt64()<=fx.GetProperty("maximumEstimatedPeakBytes").GetInt64();
if(!coreSourceTreeValid||!resourceAccepted){Emit(terminalMenu[0],new {knownAnswerPassed=false,controlsPassed=false,resourceAccepted});return;}

// Rational arithmetic, singular-rank and inverse known answers precede upstream interpretation.
Rat half=new(1,2);Rat[,] ka={{2,1},{1,1}};Rat[,] singular={{1,2},{2,4}};
bool knownAnswerPassed=new Rat(-2,-4)==half&&half+half==1&&half*new Rat(6,5)==new Rat(3,5)
    &&Equal(Multiply(ka,Inverse(ka)),Identity(2))&&Rank(singular)==1
    &&Cross([1,0,0],[0,1,0]).SequenceEqual(new Rat[]{0,0,1});
if(!knownAnswerPassed){Emit(terminalMenu[1],new {knownAnswerPassed,controlsPassed=false});return;}
using var upstream=JsonDocument.Parse(File.ReadAllBytes(Prior));
bool upstreamValid=upstream.RootElement.GetProperty("auditPassed").GetBoolean()
    &&upstream.RootElement.GetProperty("verdictKind").GetString()=="action-restriction-controls-pass-pairing-bridge-unresolved"
    &&upstream.RootElement.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
string source=File.ReadAllText(Source);
bool sourceAnchorsPresent=new[]{"(9.2)","(9.3)","(9.7)","(9.11)"}.All(source.Contains);
if(!upstreamValid||!sourceAnchorsPresent){Emit(terminalMenu[0],new {knownAnswerPassed,controlsPassed=false,upstreamValid,sourceAnchorsPresent});return;}

int[][] vertices=Rows(fx.GetProperty("vertices")),edges=Rows(fx.GetProperty("edges")),faces=Rows(fx.GetProperty("faces")),boundary=Rows(fx.GetProperty("boundaryEdges"));
int[] signs=Ints(fx.GetProperty("boundarySigns"));int[][] formPairs=Rows(fx.GetProperty("formPairs"));
var mesh=MeshTopologyBuilder.Build(4,4,vertices.SelectMany(v=>v.Select(x=>(double)x)).ToArray(),vertices.Length,[Ints(fx.GetProperty("cell"))]);
bool topologyPassed=SameRows(mesh.Edges,edges)&&SameRows(mesh.Faces,faces)&&SameRows(mesh.FaceBoundaryEdges,boundary)
    &&mesh.FaceBoundaryOrientations.All(row=>row.SequenceEqual(signs))&&mesh.CellCount==1
    &&mesh.CellEdges[0].SequenceEqual(Enumerable.Range(0,10))&&mesh.CellFaces[0].SequenceEqual(Enumerable.Range(0,10));
if(!topologyPassed){Emit(terminalMenu[2],new {knownAnswerPassed,controlsPassed=false,topologyPassed});return;}
var algebra=LieAlgebraFactory.CreateSu2WithTracePairing();
var member=new EinsteinianShiabFamilyMember {Phi1=InvariantElementSpec.Sd2,Phi2=InvariantElementSpec.Id0,
    EinsteinCoefficient=fx.GetProperty("member").GetProperty("coefficient").GetDouble(),EpsilonMode="independent-theta"};
var op=new EinsteinianShiabOperator(mesh,algebra,member);var mass=new CpuMassMatrix(mesh,algebra);
double tolerance=fx.GetProperty("comparisonTolerance").GetDouble();

// Independently construct W from the explicit integer vertices and D from the frozen incidence.
var w=new Rat[6,10];var d=new Rat[10,10];
for(int f=0;f<10;f++)
{
    var a=vertices[faces[f][0]];var b=vertices[faces[f][1]];var c=vertices[faces[f][2]];
    for(int k=0;k<6;k++){int i=formPairs[k][0],j=formPairs[k][1];w[k,f]=(b[i]-a[i])*(c[j]-a[j])-(b[j]-a[j])*(c[i]-a[i]);}
    for(int k=0;k<3;k++)d[f,boundary[f][k]]=signs[k];
}
Rat[,] wt=Transpose(w),q=Multiply(Inverse(Multiply(w,wt)),w),p=Multiply(wt,q);
Rat[,] star=ToMatrix(Rows(fx.GetProperty("starRows"))),r=Scale(new Rat(1,4),Add(Identity(6),star));
Rat[,] m=Add(Identity(10),Multiply(wt,Multiply(Subtract(r,Identity(6)),q)));
Rat[,] complement=Subtract(Identity(10),p),represented=Multiply(wt,Multiply(r,q));
bool matrixIdentitiesPassed=Equal(Multiply(q,wt),Identity(6))&&Equal(Multiply(p,p),p)&&Equal(Transpose(p),p)
    &&Equal(Multiply(star,star),Identity(6))&&Equal(Multiply(p,d),d)
    &&Equal(Multiply(m,d),Multiply(represented,d))&&Equal(Multiply(m,p),represented)
    &&Equal(m,Add(complement,represented));
int wRank=Rank(w),qRank=Rank(q),pRank=Rank(p),dRank=Rank(d),mRank=Rank(m),linearResidualRank=Rank(Multiply(m,d));
bool ranksPassed=wRank==expected.GetProperty("wRank").GetInt32()&&qRank==expected.GetProperty("qRank").GetInt32()
    &&pRank==expected.GetProperty("pRank").GetInt32()&&dRank==expected.GetProperty("dRank").GetInt32()
    &&mRank==expected.GetProperty("mRank").GetInt32()&&linearResidualRank==expected.GetProperty("linearResidualRank").GetInt32();

double actualLinearMaximumError=0,actualContractionMaximumError=0,actualMemberMaximumError=0;
for(int i=0;i<6;i++)for(int j=0;j<6;j++)actualMemberMaximumError=System.Math.Max(actualMemberMaximumError,System.Math.Abs(op.Lambda2Endomorphism[i,j]-r[i,j].Double));
for(int col=0;col<30;col++)
{
    var unit=new Rat[30];unit[col]=1;
    double[] actualCurvature=CurvatureAssembler.Assemble(new ConnectionField(mesh,algebra,Doubles(unit))).Coefficients;
    actualLinearMaximumError=System.Math.Max(actualLinearMaximumError,Error(actualCurvature,ApplyLie(d,unit)));
    actualContractionMaximumError=System.Math.Max(actualContractionMaximumError,Error(op.ApplyContractionWithTheta(Doubles(unit)),ApplyLie(m,unit)));
}

// Mixed coefficients are independently recovered by polarization, by direct rational bracket
// assembly, and by the analytic isolated-face prediction. They are NOT sampled curvatures.
var cycles=Rows(fx.GetProperty("generatorCycles"));var mixedColumns=new Rat[30,30];var mixedRows=new List<object>();
double polarizationMaximumError=0,commutingMaximumError=0;bool exactPolarizationPassed=true;int cases=0,commutingCases=0;
foreach(var (face,f) in faces.Select((face,f)=>(face,f)))foreach(var cycle in cycles)
{
    var u=new Rat[30];var v=new Rat[30];u[3*boundary[f][0]+cycle[0]]=1;v[3*boundary[f][1]+cycle[1]]=1;
    Rat[] exact=VectorSubtract(VectorSubtract(ExactCurvature(VectorAdd(u,v),boundary,signs),ExactCurvature(u,boundary,signs)),ExactCurvature(v,boundary,signs));
    var predicted=new Rat[30];predicted[3*f+cycle[2]]=-half;
    double[] actual=Polarize(u,v);
    double error=Error(actual,predicted);polarizationMaximumError=System.Math.Max(polarizationMaximumError,error);
    bool exactPassed=exact.SequenceEqual(predicted);exactPolarizationPassed&=exactPassed;
    int column=cases++;for(int row=0;row<30;row++)mixedColumns[row,column]=exact[row];
    mixedRows.Add(new {faceIndex=f,face,firstGenerator=cycle[0],secondGenerator=cycle[1],outputGenerator=cycle[2],predictedCoefficient="-1/2",exactPassed,actualError=error});
    var commutingV=new Rat[30];commutingV[3*boundary[f][1]+cycle[0]]=1;
    Rat[] commutingExact=VectorSubtract(VectorSubtract(ExactCurvature(VectorAdd(u,commutingV),boundary,signs),ExactCurvature(u,boundary,signs)),ExactCurvature(commutingV,boundary,signs));
    exactPolarizationPassed&=commutingExact.All(x=>x.IsZero);
    commutingMaximumError=System.Math.Max(commutingMaximumError,Error(Polarize(u,commutingV),new Rat[30]));commutingCases++;
}
int mixedRank=Rank(mixedColumns);
var n=new Rat[30];int[] witness=Ints(fx.GetProperty("witnessFaceCoefficients"));int witnessLie=fx.GetProperty("witnessLieIndex").GetInt32();
for(int f=0;f<10;f++)n[3*f+witnessLie]=witness[f];
// Build the witness ONLY as the frozen linear combination of polarized coefficients.
var combined=new Rat[30];
for(int f=0;f<10;f++)
{
    int cycleIndex=Array.FindIndex(cycles,cycle=>cycle[2]==witnessLie),column=f*3+cycleIndex;
    for(int row=0;row<30;row++)combined[row]+=(-2*witness[f])*mixedColumns[row,column];
}
Rat[] qn=ApplyLie(q,n),mn=ApplyLie(m,n),projectedN=ApplyLie(Multiply(m,p),n);
Rat witnessNormSquared=Dot(n,n);
bool witnessPassed=combined.SequenceEqual(n)&&qn.All(x=>x.IsZero)&&mn.SequenceEqual(n)
    &&projectedN.All(x=>x.IsZero)&&witnessNormSquared==expected.GetProperty("witnessNormSquared").GetInt32();
// Matrix Gram identity checks all cross terms, not merely norms of basis vectors.
Rat[,] gram=Multiply(Transpose(m),m);
Rat[,] splitGram=Add(Multiply(Transpose(complement),complement),Multiply(Transpose(represented),represented));
bool quadraticSplitPassed=Equal(gram,splitGram);
double actualWitnessError=Error(op.ApplyContractionWithTheta(Doubles(n)),n);
var tensor=new CurvatureField(mesh,algebra,Doubles(n)).ToFieldTensor();
double actualWitnessPairing=mass.InnerProduct(tensor,tensor);
bool actualControlsPassed=actualLinearMaximumError<=tolerance&&actualContractionMaximumError<=tolerance&&actualMemberMaximumError<=tolerance
    &&polarizationMaximumError<=tolerance&&commutingMaximumError<=tolerance&&actualWitnessError<=tolerance
    &&System.Math.Abs(actualWitnessPairing-witnessNormSquared.Double)<=tolerance;
bool coefficientControlsPassed=exactPolarizationPassed&&mixedRank==expected.GetProperty("mixedRankWithLie").GetInt32()
    &&cases==expected.GetProperty("noncommutingCases").GetInt32()&&commutingCases==expected.GetProperty("commutingCases").GetInt32();
bool controlsPassed=matrixIdentitiesPassed&&ranksPassed&&actualControlsPassed&&coefficientControlsPassed&&quadraticSplitPassed;
bool obstructionConfirmed=controlsPassed&&witnessPassed;
string verdict=!controlsPassed?terminalMenu[2]:obstructionConfirmed?Success:terminalMenu[4];
Emit(verdict,new
{
    knownAnswerPassed,controlsPassed,upstreamValid,sourceAnchorsPresent,resourceAccepted,topologyPassed,
    exactMatrices=new {matrixIdentitiesPassed,ranksPassed,wRank,qRank,pRank,dRank,mRank,linearResidualRank,
        w=MatrixStrings(w),q=MatrixStrings(q),p=MatrixStrings(p),r=MatrixStrings(r),m=MatrixStrings(m),d=MatrixStrings(d),arithmetic="normalized-BigInteger-rationals",exactTolerance=0},
    actualOperators=new {actualControlsPassed,actualLinearColumns=30,actualContractionColumns=30,actualLinearMaximumError,
        actualContractionMaximumError,actualMemberMaximumError,polarizationMaximumError,commutingMaximumError,actualWitnessError,actualWitnessPairing,tolerance},
    polynomialCoefficients=new {coefficientControlsPassed,cases,commutingCases,mixedRank,exactPolarizationPassed,mixedRows,
        everyLinearCombinationClaimedRealizableAsSingleCurvature=false},
    factorization=new {obstructionConfirmed,witnessPassed,witness=Strings(n),assembledWitness=Strings(combined),qWitness=Strings(qn),mWitness=Strings(mn),projectedWitness=Strings(projectedN),witnessNormSquared=witnessNormSquared.ToString(),
        direction="reconstruct full registered M F(A) from Q F(A)",excludedMapClass="field-independent linear N, with M F(A)=N Q F(A) for every retained A",
        flatTangentFactorizationPassed=Equal(Multiply(m,d),Multiply(represented,d)),projectedControlPassed=Equal(Multiply(m,p),represented),quadraticSplitPassed,
        allMapsFromRegisteredToSourceExcluded=false,nonlinearMapExcluded=false,allScalarActionEquivalencesExcluded=false,sourceOperatorSelected=false},
    nextRequirement="Determine a source-compatible retained reconstruction and literal carrier/pairing; this coefficient obstruction does not select one or establish physical spectral equivalence."
});

double[] Polarize(Rat[] u,Rat[] v)
{
    double[] Curvature(Rat[] a)=>CurvatureAssembler.Assemble(new ConnectionField(mesh,algebra,Doubles(a))).Coefficients;
    var uv=Curvature(VectorAdd(u,v));var fu=Curvature(u);var fv=Curvature(v);var zero=Curvature(new Rat[30]);
    return uv.Select((x,i)=>x-fu[i]-fv[i]+zero[i]).ToArray();
}
void Emit(string verdictKind,object evidence)
{
    var result=new {schemaVersion=1,phase=587,phaseId="phase587-exact-residual-factorization-audit",contractId=ContractId,
        contractSha256=Sha(ContractPath),contractValid,exactBindingsValid,coreSourceTreeValid,bindingCount=bindings.Length,
        uniqueBindingIdCount=bindings.Select(x=>x.id).Distinct().Count(),uniqueBindingPathCount=bindings.Select(x=>x.path).Distinct().Count(),bindings,
        verdictKind,terminalStatus=verdictKind,auditPassed=verdictKind==Success,evidence,deterministic=true,
        authorityFirewalls=forbidden.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
    string json=JsonSerializer.Serialize(result,new JsonSerializerOptions {WriteIndented=true})+Environment.NewLine;
    Directory.CreateDirectory(Root+"/output");File.WriteAllText(Root+"/output/exact_residual_factorization_audit.json",json);
    File.WriteAllText(Root+"/output/exact_residual_factorization_audit_summary.json",json);
    Console.WriteLine($"Phase587 verdict: {verdictKind}");if(verdictKind!=Success)Environment.ExitCode=1;
}
static string Sha(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static string[] CorePaths()=>Directory.EnumerateFiles("src","*",SearchOption.AllDirectories).Where(p=>(p.EndsWith(".cs",StringComparison.Ordinal)||p.EndsWith(".csproj",StringComparison.Ordinal))&&!p.Split('/').Any(x=>x is "bin" or "obj")).Order(StringComparer.Ordinal).ToArray();
static string TreeSha()=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Concat(CorePaths().Select(p=>p+" "+Sha(p)+"\n"))))).ToLowerInvariant();
static int[] Ints(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetInt32()).ToArray();
static int[][] Rows(JsonElement e)=>e.EnumerateArray().Select(Ints).ToArray();
static bool SameRows(int[][] a,int[][] b)=>a.Length==b.Length&&a.Zip(b).All(x=>x.First.SequenceEqual(x.Second));
static Rat[,] ToMatrix(int[][] rows){var a=new Rat[rows.Length,rows[0].Length];for(int i=0;i<rows.Length;i++)for(int j=0;j<rows[i].Length;j++)a[i,j]=rows[i][j];return a;}
static Rat[,] Identity(int n){var a=new Rat[n,n];for(int i=0;i<n;i++)a[i,i]=1;return a;}
static Rat[,] Add(Rat[,] a,Rat[,] b){var c=new Rat[a.GetLength(0),a.GetLength(1)];for(int i=0;i<c.GetLength(0);i++)for(int j=0;j<c.GetLength(1);j++)c[i,j]=a[i,j]+b[i,j];return c;}
static Rat[,] Scale(Rat s,Rat[,] a){var c=new Rat[a.GetLength(0),a.GetLength(1)];for(int i=0;i<c.GetLength(0);i++)for(int j=0;j<c.GetLength(1);j++)c[i,j]=s*a[i,j];return c;}
static Rat[,] Subtract(Rat[,] a,Rat[,] b)=>Add(a,Scale(-1,b));
static Rat[,] Transpose(Rat[,] a){var c=new Rat[a.GetLength(1),a.GetLength(0)];for(int i=0;i<c.GetLength(0);i++)for(int j=0;j<c.GetLength(1);j++)c[i,j]=a[j,i];return c;}
static Rat[,] Multiply(Rat[,] a,Rat[,] b){var c=new Rat[a.GetLength(0),b.GetLength(1)];for(int i=0;i<c.GetLength(0);i++)for(int j=0;j<c.GetLength(1);j++)for(int k=0;k<a.GetLength(1);k++)c[i,j]+=a[i,k]*b[k,j];return c;}
static bool Equal(Rat[,] a,Rat[,] b)=>a.GetLength(0)==b.GetLength(0)&&a.GetLength(1)==b.GetLength(1)&&a.Cast<Rat>().SequenceEqual(b.Cast<Rat>());
static Rat[,] Inverse(Rat[,] a)
{
    int n=a.GetLength(0);var aug=new Rat[n,2*n];for(int i=0;i<n;i++)for(int j=0;j<n;j++){aug[i,j]=a[i,j];aug[i,n+j]=i==j?1:0;}
    for(int col=0;col<n;col++)
    {
        int pivot=col;while(pivot<n&&aug[pivot,col].IsZero)pivot++;if(pivot==n)throw new InvalidOperationException("Singular exact inverse");
        for(int j=0;j<2*n;j++)(aug[pivot,j],aug[col,j])=(aug[col,j],aug[pivot,j]);
        Rat divisor=aug[col,col];for(int j=0;j<2*n;j++)aug[col,j]/=divisor;
        for(int row=0;row<n;row++)if(row!=col){Rat factor=aug[row,col];for(int j=0;j<2*n;j++)aug[row,j]-=factor*aug[col,j];}
    }
    var inverse=new Rat[n,n];for(int i=0;i<n;i++)for(int j=0;j<n;j++)inverse[i,j]=aug[i,n+j];return inverse;
}
static int Rank(Rat[,] input)
{
    var a=(Rat[,])input.Clone();int rows=a.GetLength(0),cols=a.GetLength(1),rank=0;
    for(int col=0;col<cols&&rank<rows;col++)
    {
        int pivot=rank;while(pivot<rows&&a[pivot,col].IsZero)pivot++;if(pivot==rows)continue;
        for(int j=col;j<cols;j++)(a[pivot,j],a[rank,j])=(a[rank,j],a[pivot,j]);
        Rat divisor=a[rank,col];for(int j=col;j<cols;j++)a[rank,j]/=divisor;
        for(int row=rank+1;row<rows;row++){Rat factor=a[row,col];for(int j=col;j<cols;j++)a[row,j]-=factor*a[rank,j];}rank++;
    }
    return rank;
}
static Rat[] VectorAdd(Rat[] a,Rat[] b)=>a.Zip(b,(x,y)=>x+y).ToArray();
static Rat[] VectorSubtract(Rat[] a,Rat[] b)=>a.Zip(b,(x,y)=>x-y).ToArray();
static Rat Dot(Rat[] a,Rat[] b){Rat sum=0;for(int i=0;i<a.Length;i++)sum+=a[i]*b[i];return sum;}
static Rat[] Cross(Rat[] a,Rat[] b)=>[a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]];
static Rat[] ApplyLie(Rat[,] a,Rat[] v){var r=new Rat[3*a.GetLength(0)];for(int i=0;i<a.GetLength(0);i++)for(int j=0;j<a.GetLength(1);j++)for(int k=0;k<3;k++)r[3*i+k]+=a[i,j]*v[3*j+k];return r;}
static Rat[] ExactCurvature(Rat[] omega,int[][] boundary,int[] signs)
{
    var result=new Rat[boundary.Length*3];
    for(int f=0;f<boundary.Length;f++)
    {
        var oriented=boundary[f].Select((e,i)=>Enumerable.Range(0,3).Select(g=>signs[i]*omega[3*e+g]).ToArray()).ToArray();
        for(int g=0;g<3;g++)result[3*f+g]=oriented.Aggregate((Rat)0,(sum,x)=>sum+x[g]);
        for(int i=0;i<3;i++)for(int j=i+1;j<3;j++){var bracket=Cross(oriented[i],oriented[j]);for(int g=0;g<3;g++)result[3*f+g]+=new Rat(1,2)*bracket[g];}
    }
    return result;
}
static double[] Doubles(Rat[] a)=>a.Select(x=>x.Double).ToArray();
static double Error(double[] a,Rat[] b)=>a.Select((x,i)=>System.Math.Abs(x-b[i].Double)).Max();
static string[] Strings(Rat[] a)=>a.Select(x=>x.ToString()).ToArray();
static string[][] MatrixStrings(Rat[,] a)=>Enumerable.Range(0,a.GetLength(0)).Select(i=>Enumerable.Range(0,a.GetLength(1)).Select(j=>a[i,j].ToString()).ToArray()).ToArray();

readonly struct Rat : IEquatable<Rat>
{
    private readonly BigInteger numerator;
    private readonly BigInteger denominator;
    private BigInteger Denominator=>denominator.IsZero?BigInteger.One:denominator;
    public Rat(BigInteger n,BigInteger d)
    {
        if(d.IsZero)throw new DivideByZeroException();if(d.Sign<0){n=-n;d=-d;}
        BigInteger gcd=BigInteger.GreatestCommonDivisor(BigInteger.Abs(n),d);numerator=n/gcd;denominator=d/gcd;
    }
    public bool IsZero=>numerator.IsZero;
    public double Double=>(double)numerator/(double)Denominator;
    public static implicit operator Rat(int n)=>new(n,1);
    public static Rat operator +(Rat a,Rat b)=>new(a.numerator*b.Denominator+b.numerator*a.Denominator,a.Denominator*b.Denominator);
    public static Rat operator -(Rat a,Rat b)=>new(a.numerator*b.Denominator-b.numerator*a.Denominator,a.Denominator*b.Denominator);
    public static Rat operator -(Rat a)=>new(-a.numerator,a.Denominator);
    public static Rat operator *(Rat a,Rat b)=>new(a.numerator*b.numerator,a.Denominator*b.Denominator);
    public static Rat operator /(Rat a,Rat b)=>new(a.numerator*b.Denominator,a.Denominator*b.numerator);
    public bool Equals(Rat other)=>numerator==other.numerator&&Denominator==other.Denominator;
    public override bool Equals(object? other)=>other is Rat value&&Equals(value);
    public override int GetHashCode()=>HashCode.Combine(numerator,Denominator);
    public static bool operator ==(Rat a,Rat b)=>a.Equals(b);
    public static bool operator !=(Rat a,Rat b)=>!a.Equals(b);
    public override string ToString()=>Denominator==1?numerator.ToString(System.Globalization.CultureInfo.InvariantCulture):numerator.ToString(System.Globalization.CultureInfo.InvariantCulture)+"/"+Denominator.ToString(System.Globalization.CultureInfo.InvariantCulture);
}
