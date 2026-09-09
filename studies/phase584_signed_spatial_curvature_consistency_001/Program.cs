using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase584_signed_spatial_curvature_consistency_001";
const string ContractPath = Root + "/preregistration/contract_v1.json";
const string ContractId = "phase584-a46-signed-spatial-curvature-v1";
const string Success = "paired-sign-smooth-spatial-consistency-fixed-mesh-obstruction-preserved";
const string PriorRoot = "studies/phase565_registered_finite_transport_bch_control_001";
string[] forbidden = ["authorIntentSelected", "registeredTransformationBridgeEstablished", "registeredActionChanged",
    "registeredMeasureSelected", "physicalHiggsIdentified", "sourceContractApplicationAllowed", "phase561Opened",
    "o4Discharged", "phase458Satisfied", "phase481Changed", "samplingPerformed", "samplingAuthorized",
    "productionAuthorized", "gevClaimAllowed"];
string[] precedence = ["invalid-or-drifted-input", "known-answer-battery-failed", "mathematical-control-failed", Success];
// All fixture values, including tolerances and case counts, are checked structurally before use.
const string FrozenFixtures = """
{
  "spatialLadder": [0.25,0.125,0.0625,0.03125,0.015625,0.0078125,0.00390625],
  "unitVertices": [[0,0],[1,0],[1,1]],
  "vertexPermutations": [[0,1,2],[0,2,1],[1,0,2],[1,2,0],[2,0,1],[2,1,0]],
  "dictionaries": [{"id":"same","inputSign":1,"outputSign":1},{"id":"paired","inputSign":-1,"outputSign":-1}],
  "profiles": [
    {"id":"constant-commuting","X":[1,0,0],"Y":[2,0,0],"P":[0,0,0],"Q":[0,0,0],"R":[0,0,0],"U":[0,0,0]},
    {"id":"constant-noncommuting","X":[1,0,0],"Y":[0,1,0],"P":[0,0,0],"Q":[0,0,0],"R":[0,0,0],"U":[0,0,0]},
    {"id":"affine-witness","X":[1,0,0],"Y":[0,1,0],"P":[0,0,0],"Q":[0,0,0],"R":[0,0,1],"U":[0,0,0]},
    {"id":"affine-general","X":[1,2,-1],"Y":[-1,1,2],"P":[1,0,1],"Q":[0,1,-1],"R":[2,-1,1],"U":[-1,2,0]}
  ],
  "algebra": "CreateSu2WithTracePairing; bracket independently checked against cross product",
  "edgeIntegration": "exact straight-edge affine integral by midpoint, no path-order claim",
  "spatialComparison": "signed-area-normalized curvature at origin; exact polynomial remainder",
  "spatialRowCount": 168,
  "dictionaryAssemblyCount": 336,
  "algebraIdentityMenu": "all triples of vectors in {-1,0,1}^3",
  "algebraIdentityCaseCount": 19683,
  "fixedMesh": "CreateUniform4D(1)",
  "fixedMeshWaveScale": 0.031,
  "fixedMeshWaveFrequency": 0.419,
  "fixedMeshWaveCosineWeight": 0.37,
  "fixedMeshWaveCosineFrequencyRatio": 1.7,
  "amplitudeLadder": [1,0.5,0.25,0.125,0.0625,0.03125,0.015625,0.0078125,0.00390625],
  "slopePrefixLength": 6,
  "coefficientIndex": 6,
  "exactTolerance": 0,
  "floatingTolerance": 2e-12,
  "slopeTolerance": 0.2,
  "coefficientRelativeTolerance": 0.003,
  "spatialEndpointErrorRatioMaximum": 0.03,
  "constantSameSignError": 2,
  "affineWitnessSameSignMinimumError": 1.9,
  "positiveDefectMinimum": 1e-8,
  "estimatedAggregateCpuSeconds": 10,
  "maximumEstimatedAggregateCpuSeconds": 60,
  "estimatedPeakBytes": 80000000,
  "maximumEstimatedPeakBytes": 400000000
}
""";
var requiredPaths = new Dictionary<string,string>
{
    ["program"]=Root+"/Program.cs", ["project"]=Root+"/Phase584SignedSpatialCurvatureConsistency.csproj",
    ["proof"]=Root+"/STUDY.md", ["primary-source"]="docs/Reference/ExperimentReferences/texts/GU-DRAFT-2021-TEXT.txt",
    ["phase583-summary"]="studies/phase583_biconnection_convention_reconciliation_001/output/biconnection_convention_reconciliation_summary.json",
    ["phase565-summary"]=PriorRoot+"/output/registered_finite_transport_bch_control_summary.json",
    ["phase565-program"]=PriorRoot+"/Program.cs",
    ["phase565-contract"]=PriorRoot+"/preregistration/phase565_registered_finite_transport_bch_control_contract_v1.json",
    ["curvature-assembler"]="src/Gu.ReferenceCpu/CurvatureAssembler.cs",
    ["mesh-builder"]="src/Gu.Geometry/MeshTopologyBuilder.cs",
    ["mesh-generator"]="src/Gu.Geometry/SimplicialMeshGenerator.cs",
    ["simplicial-mesh"]="src/Gu.Geometry/SimplicialMesh.cs",
    ["connection-field"]="src/Gu.ReferenceCpu/ConnectionField.cs",
    ["lie-algebra"]="src/Gu.Math/LieAlgebra.cs", ["lie-factory"]="src/Gu.Math/LieAlgebraFactory.cs",
    ["build-props"]="Directory.Build.props",
    ["core-project"]="src/Gu.Core/Gu.Core.csproj", ["geometry-project"]="src/Gu.Geometry/Gu.Geometry.csproj",
    ["math-project"]="src/Gu.Math/Gu.Math.csproj", ["cpu-project"]="src/Gu.ReferenceCpu/Gu.ReferenceCpu.csproj"
};
bool contractValid=false, exactBindingsValid=false;
Binding[] bindings=[];
JsonElement contract=default;
try
{
    using var doc=JsonDocument.Parse(File.ReadAllBytes(ContractPath)); contract=doc.RootElement.Clone();
    contractValid=contract.GetProperty("schemaVersion").GetInt32()==1 && contract.GetProperty("phase").GetInt32()==584
        && contract.GetProperty("contractId").GetString()==ContractId
        && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
        && contract.GetProperty("deterministicZeroSampling").GetBoolean()
        && contract.GetProperty("externalReviewPending").GetBoolean()
        && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0
        && JsonNode.DeepEquals(JsonNode.Parse(contract.GetProperty("fixtures").GetRawText()),JsonNode.Parse(FrozenFixtures))
        && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x=>x.GetString()).SequenceEqual(precedence)
        && contract.GetProperty("authorityFirewalls").EnumerateObject().Count()==forbidden.Length
        && forbidden.All(k=>contract.GetProperty("authorityFirewalls").GetProperty(k).ValueKind==JsonValueKind.False);
    bindings=contract.GetProperty("exactBindings").EnumerateArray().Select(x=>new Binding(
        x.GetProperty("id").GetString()!,x.GetProperty("path").GetString()!,x.GetProperty("sha256").GetString()!)).ToArray();
    exactBindingsValid=bindings.Length==requiredPaths.Count && bindings.Select(x=>x.id).Distinct().Count()==requiredPaths.Count
        && bindings.Select(x=>x.path).Distinct().Count()==requiredPaths.Count
        && bindings.All(x=>requiredPaths.TryGetValue(x.id,out string? p) && p==x.path && x.sha256.Length==64
            && x.sha256.All(c=>char.IsAsciiHexDigit(c)) && x.hashMatches);
}
catch(Exception ex) when(ex is IOException or JsonException or InvalidOperationException or KeyNotFoundException or ArgumentException)
{ Emit("invalid-or-drifted-input",new {knownAnswerPassed=false,controlsPassed=false,error=ex.GetType().Name}); return; }
if(!contractValid || !exactBindingsValid) {Emit(precedence[0],new {knownAnswerPassed=false,controlsPassed=false}); return;}
JsonElement fixtures=contract.GetProperty("fixtures");
double tolerance=fixtures.GetProperty("floatingTolerance").GetDouble();
var algebra=LieAlgebraFactory.CreateSu2WithTracePairing();
double[] ex1=[1,0,0], ex2=[0,1,0], ex3=[0,0,1];
double identityResidual=0, bracketResidual=0;
int identityCases=0;
var vectors=(from x in new[]{-1,0,1} from y in new[]{-1,0,1} from z in new[]{-1,0,1}
    select new double[]{x,y,z}).ToArray();
foreach(var x in vectors) foreach(var y in vectors)
{
    bracketResidual=System.Math.Max(bracketResidual,Norm(Sub(algebra.Bracket(x,y),Cross(x,y))));
    foreach(var z in vectors)
    {
        identityCases++;
        identityResidual=System.Math.Max(identityResidual,Norm(Sub(Add(QReg([x,y,z]),QLoop([x,y,z])),Cross(x,Add(Add(x,y),z)))));
    }
}
double quaternionError=Norm(Sub(Log(Exp([0.2,-0.1,0.05])),new double[]{0.2,-0.1,0.05}));
bool knownAnswerPassed=Norm(Sub(algebra.Bracket(ex1,ex2),ex3))==0 && bracketResidual==0 && identityResidual==0
    && identityCases==fixtures.GetProperty("algebraIdentityCaseCount").GetInt32() && quaternionError<tolerance;
if(!knownAnswerPassed) {Emit(precedence[1],new {knownAnswerPassed,controlsPassed=false,identityCases,identityResidual,bracketResidual,quaternionError});return;}
using var p583=JsonDocument.Parse(File.ReadAllBytes(requiredPaths["phase583-summary"]));
using var p565=JsonDocument.Parse(File.ReadAllBytes(requiredPaths["phase565-summary"]));
bool upstreamValid=p583.RootElement.GetProperty("auditPassed").GetBoolean()
    && p583.RootElement.GetProperty("verdictKind").GetString()=="printed-sign-conflict-proved-two-compatible-families-registered-map-unresolved"
    && p565.RootElement.GetProperty("verdictKind").GetString()=="registered-curvature-continuous-holonomy-second-order-mismatch"
    && p565.RootElement.GetProperty("promotedPhysicalMassClaimCount").GetInt32()==0;
if(!upstreamValid) {Emit(precedence[0],new {knownAnswerPassed,controlsPassed=false,upstreamValid});return;}

double[] ladder=ReadVector(fixtures.GetProperty("spatialLadder"));
double[][] unitVertices=fixtures.GetProperty("unitVertices").EnumerateArray().Select(ReadVector).ToArray();
int[][] permutations=fixtures.GetProperty("vertexPermutations").EnumerateArray().Select(x=>x.EnumerateArray().Select(y=>y.GetInt32()).ToArray()).ToArray();
var rows=new List<object>(); var refinementRows=new List<object>();
double maxPolynomialResidual=0,maxLeadingResidual=0,maxAssemblyIdentityResidual=0,maxStokesResidual=0;
bool spatialPassed=true,constantControlsPassed=true,permutationControlsPassed=true,sameSignDecoyRejected=true;
int spatialRowCount=0,assemblyCount=0;
foreach(var profile in fixtures.GetProperty("profiles").EnumerateArray())
{
    string id=profile.GetProperty("id").GetString()!;
    double[] x0=ReadVector(profile.GetProperty("X")), y0=ReadVector(profile.GetProperty("Y"));
    double[] derivative=Sub(ReadVector(profile.GetProperty("R")),ReadVector(profile.GetProperty("Q")));
    double[] bracket=Cross(x0,y0), target=Add(derivative,bracket), sameLimit=Sub(derivative,bracket);
    foreach(var permutation in permutations)
    {
        double[][] uv=permutation.Select(i=>unitVertices[i]).ToArray();
        double unitArea=SignedArea(uv);
        double[][] linear=OrientedAnalyticEdges(uv,profile,false), quadratic=OrientedAnalyticEdges(uv,profile,true);
        double[] s2=Sum(quadratic), q2=QReg(linear);
        double[] q3=Scale(Sub(Sub(QReg(AddEdges(linear,quadratic)),q2),QReg(quadratic)),1/unitArea);
        double[] q4=Scale(QReg(quadratic),1/unitArea);
        double leadingResidual=Norm(Sub(Scale(Sub(s2,q2),1/unitArea),target));
        double sameLeadingResidual=Norm(Sub(Scale(Add(s2,q2),1/unitArea),sameLimit));
        maxLeadingResidual=System.Math.Max(maxLeadingResidual,System.Math.Max(leadingResidual,sameLeadingResidual));
        permutationControlsPassed &= Norm(Sum(linear))==0 && leadingResidual==0 && sameLeadingResidual==0;
        var errors=new List<double>();
        foreach(double a in ladder)
        {
            double[][] vertices=uv.Select(v=>Scale(v,a)).ToArray();
            var mesh=MeshTopologyBuilder.Build(2,2,vertices.SelectMany(v=>v).ToArray(),3,[new[]{0,1,2}]);
            double area=SignedArea(vertices);
            double[] cochain=mesh.Edges.SelectMany(e=>Integrate(vertices[e[0]],vertices[e[1]],profile)).ToArray();
            var signed=mesh.FaceBoundaryEdges[0].Select((edge,k)=>Scale(Block(cochain,edge),mesh.FaceBoundaryOrientations[0][k])).ToArray();
            bool topologyPassed=mesh.FaceCount==1 && mesh.EdgeCount==3 && mesh.Faces[0].SequenceEqual(new[]{0,1,2})
                && mesh.FaceBoundaryOrientations[0].SequenceEqual(new[]{1,-1,1});
            double[] same=[],paired=[];
            foreach(var dictionary in fixtures.GetProperty("dictionaries").EnumerateArray())
            {
                double[] actual=Scale(CurvatureAssembler.Assemble(new ConnectionField(mesh,algebra,
                    Scale(cochain,dictionary.GetProperty("inputSign").GetInt32()))).Coefficients,dictionary.GetProperty("outputSign").GetInt32());
                if(dictionary.GetProperty("id").GetString()=="same") same=actual; else paired=actual;
                assemblyCount++;
            }
            double[] sameNormalized=Scale(same,1/area),pairedNormalized=Scale(paired,1/area);
            double[] remainder=Add(Scale(q3,a),Scale(q4,a*a));
            double polynomialResidual=System.Math.Max(Norm(Sub(pairedNormalized,Sub(target,remainder))),Norm(Sub(sameNormalized,Add(sameLimit,remainder))));
            double[] s=Sum(signed);
            double assemblyIdentityResidual=Norm(Sub(Sub(Add(s,QLoop(signed)),paired),Cross(signed[0],s)));
            double stokesResidual=Norm(Sub(s,Scale(derivative,area)));
            double pairedError=Norm(Sub(pairedNormalized,target)),sameError=Norm(Sub(sameNormalized,target));
            double errorBound=a*Norm(q3)+a*a*Norm(q4);
            errors.Add(pairedError);
            maxPolynomialResidual=System.Math.Max(maxPolynomialResidual,polynomialResidual);
            maxAssemblyIdentityResidual=System.Math.Max(maxAssemblyIdentityResidual,assemblyIdentityResidual);
            maxStokesResidual=System.Math.Max(maxStokesResidual,stokesResidual);
            spatialPassed &= topologyPassed && polynomialResidual<=tolerance && assemblyIdentityResidual<=tolerance
                && stokesResidual<=tolerance && pairedError<=errorBound+tolerance;
            if(id.StartsWith("constant",StringComparison.Ordinal)) constantControlsPassed &= pairedError==0;
            if(id=="constant-commuting") constantControlsPassed &= sameError==0;
            if(id=="constant-noncommuting") sameSignDecoyRejected &= System.Math.Abs(sameError-fixtures.GetProperty("constantSameSignError").GetDouble())<=tolerance;
            if(id=="affine-witness") sameSignDecoyRejected &= sameError>=fixtures.GetProperty("affineWitnessSameSignMinimumError").GetDouble();
            rows.Add(new {profile=id,permutation,a,signedArea=area,target,sameLimit,sameNormalized,pairedNormalized,pairedError,sameError,errorBound,
                polynomialResidual,assemblyIdentityResidual,stokesResidual,topologyPassed});
            spatialRowCount++;
        }
        bool refinementPassed=errors.All(e=>e==0) || errors[^1]<=fixtures.GetProperty("spatialEndpointErrorRatioMaximum").GetDouble()*errors[0];
        spatialPassed &= refinementPassed;
        refinementRows.Add(new {profile=id,permutation,firstError=errors[0],lastError=errors[^1],refinementPassed,
            linearNormalizedRemainder=Scale(q3,-1),quadraticNormalizedRemainder=Scale(q4,-1)});
    }
}

// Reproduce the original Phase565 rough fixed-mesh wave and its amplitude limit.
// The paired dictionary is an additional control; historical Phase565 bytes are never changed.
var fixedMesh=SimplicialMeshGenerator.CreateUniform4D(1);
double[] wave=Enumerable.Range(0,fixedMesh.EdgeCount*3).Select(i=>fixtures.GetProperty("fixedMeshWaveScale").GetDouble()
    *(System.Math.Sin((i+1)*fixtures.GetProperty("fixedMeshWaveFrequency").GetDouble())
    +fixtures.GetProperty("fixedMeshWaveCosineWeight").GetDouble()*System.Math.Cos((i+1)*fixtures.GetProperty("fixedMeshWaveFrequency").GetDouble()
        *fixtures.GetProperty("fixedMeshWaveCosineFrequencyRatio").GetDouble()))).ToArray();
double[] amplitudes=ReadVector(fixtures.GetProperty("amplitudeLadder"));
var weakRows=new List<object>(); var sameResiduals=new List<double>(); var arrayResiduals=new List<double>(); var pairedResiduals=new List<double>();
double sameCoefficientError=0,pairedCoefficientError=0,sameDefectNorm=0,pairedDefectNorm=0;
foreach(double t in amplitudes)
{
    double[] field=Scale(wave,t);
    double[] same=CurvatureAssembler.Assemble(new ConnectionField(fixedMesh,algebra,field)).Coefficients;
    double[] paired=Scale(CurvatureAssembler.Assemble(new ConnectionField(fixedMesh,algebra,Scale(field,-1))).Coefficients,-1);
    var sameDifference=new List<double>(); var pairedDifference=new List<double>(); var arrayDifference=new List<double>();
    var predictedSame=new List<double>(); var predictedPaired=new List<double>();
    for(int f=0;f<fixedMesh.FaceCount;f++)
    {
        var unscaled=fixedMesh.FaceBoundaryEdges[f].Select((e,k)=>Scale(Block(wave,e),fixedMesh.FaceBoundaryOrientations[f][k])).ToArray();
        var oriented=unscaled.Select(e=>Scale(e,t)).ToArray();
        double[] logLoop=Log(Exp(oriented[0])*Exp(oriented[2])*Exp(oriented[1]));
        double[] logArray=Log(Exp(oriented[0])*Exp(oriented[1])*Exp(oriented[2]));
        sameDifference.AddRange(Sub(logLoop,Block(same,f)));
        pairedDifference.AddRange(Sub(logLoop,Block(paired,f)));
        arrayDifference.AddRange(Sub(logArray,Block(same,f)));
        predictedSame.AddRange(Cross(unscaled[2],unscaled[1]));
        predictedPaired.AddRange(Cross(unscaled[0],Sum(unscaled)));
    }
    double sameResidual=Norm(sameDifference.ToArray()),pairedResidual=Norm(pairedDifference.ToArray()),arrayResidual=Norm(arrayDifference.ToArray());
    sameResiduals.Add(sameResidual);pairedResiduals.Add(pairedResidual);arrayResiduals.Add(arrayResidual);
    if(t==amplitudes[fixtures.GetProperty("coefficientIndex").GetInt32()])
    {
        sameDefectNorm=Norm(predictedSame.ToArray()); pairedDefectNorm=Norm(predictedPaired.ToArray());
        sameCoefficientError=Norm(Sub(Scale(sameDifference.ToArray(),1/(t*t)),predictedSame.ToArray()))/sameDefectNorm;
        pairedCoefficientError=Norm(Sub(Scale(pairedDifference.ToArray(),1/(t*t)),predictedPaired.ToArray()))/pairedDefectNorm;
    }
    weakRows.Add(new {t,sameResidual,pairedResidual,arrayResidual});
}
int prefix=fixtures.GetProperty("slopePrefixLength").GetInt32();
double sameSlope=LogSlope(amplitudes.Take(prefix).ToArray(),sameResiduals.Take(prefix).ToArray());
double pairedSlope=LogSlope(amplitudes.Take(prefix).ToArray(),pairedResiduals.Take(prefix).ToArray());
double arraySlope=LogSlope(amplitudes.Take(prefix).ToArray(),arrayResiduals.Take(prefix).ToArray());
double slopeTolerance=fixtures.GetProperty("slopeTolerance").GetDouble(),coefficientTolerance=fixtures.GetProperty("coefficientRelativeTolerance").GetDouble();
bool fixedMeshObstructionPreserved=System.Math.Abs(sameSlope-2)<=slopeTolerance && System.Math.Abs(pairedSlope-2)<=slopeTolerance
    && System.Math.Abs(arraySlope-3)<=slopeTolerance && sameCoefficientError<=coefficientTolerance && pairedCoefficientError<=coefficientTolerance
    && sameDefectNorm>fixtures.GetProperty("positiveDefectMinimum").GetDouble() && pairedDefectNorm>fixtures.GetProperty("positiveDefectMinimum").GetDouble();

// A smooth affine field also retains a quadratic amplitude defect at fixed a.
var witness=fixtures.GetProperty("profiles")[2];
double fixedA=ladder[0];
double[][] witnessVertices=unitVertices.Select(v=>Scale(v,fixedA)).ToArray();
double[][] witnessEdges=AddEdges(OrientedAnalyticEdges(witnessVertices,witness,false),OrientedAnalyticEdges(witnessVertices,witness,true));
double[] witnessDefect=Cross(witnessEdges[0],Sum(witnessEdges));
double[] expectedWitnessDefect=[0,-fixedA*fixedA*fixedA/2,0];
double witnessResidual=Norm(Sub(witnessDefect,expectedWitnessDefect));
bool fixedSmoothAmplitudePassed=witnessResidual==0 && Norm(witnessDefect)>0;
foreach(double t in amplitudes)
{
    var scaled=witnessEdges.Select(e=>Scale(e,t)).ToArray();
    fixedSmoothAmplitudePassed &= Norm(Sub(Add(QReg(scaled),QLoop(scaled)),Scale(witnessDefect,t*t)))==0;
}
bool controlsPassed=spatialPassed && constantControlsPassed && permutationControlsPassed && sameSignDecoyRejected
    && fixedMeshObstructionPreserved && fixedSmoothAmplitudePassed && maxLeadingResidual==0
    && spatialRowCount==fixtures.GetProperty("spatialRowCount").GetInt32() && assemblyCount==fixtures.GetProperty("dictionaryAssemblyCount").GetInt32();
Emit(controlsPassed?Success:precedence[2],new
{
    knownAnswerPassed,controlsPassed,upstreamValid,
    exactAlgebra=new {identityCases,identityResidual,bracketResidual,quaternionError,formula="Q_reg+Q_loop=[x,x+y+z]"},
    spatial=new {spatialPassed,constantControlsPassed,permutationControlsPassed,sameSignDecoyRejected,spatialRowCount,assemblyCount,
        maxLeadingResidual,maxPolynomialResidual,maxAssemblyIdentityResidual,maxStokesResidual,rows,refinementRows,
        smoothClassicalLeadingConsistencyOnly=true,finiteMeshPermutationInvarianceClaimed=false},
    fixedMesh=new {fixedMesh.VertexCount,fixedMesh.EdgeCount,fixedMesh.FaceCount,rows=weakRows,sameSlope,pairedSlope,arraySlope,
        sameCoefficientError,pairedCoefficientError,sameDefectNorm,pairedDefectNorm,fixedMeshObstructionPreserved,
        originalPhase565NegativePreserved=true},
    fixedSmoothAmplitude=new {fixedA,witnessDefect,expectedWitnessDefect,witnessResidual,fixedSmoothAmplitudePassed,defectOrderInAmplitude=2},
    interpretation=new {pairedInput="omega=-A",pairedOutput="F_physical=-F_registered",pairedTransport="exp(-omega)",
        sameSignLeading="dA-A wedge A",pairedSignLeading="dA+A wedge A",unnormalizedSpatialDefectOrder=3,areaNormalizedSpatialErrorOrder=1,
        signFlipIsLieAlgebraAutomorphism=false,fullSourceActionBridgeEstablished=false,roughFieldQuantumLimitEstablished=false,
        sourceChoosesDictionary=false,oldSampleOutputsReinterpreted=false},
    nextRequirement="Propagate the complete signed variable, residual, derivative and pairing dictionary through the action; classical smooth consistency does not validate rough-field ensembles or poles."
});

void Emit(string verdict,object evidence)
{
    var result=new {schemaVersion=1,phase=584,phaseId="phase584-signed-spatial-curvature-consistency",contractId=ContractId,
        contractSha256=File.Exists(ContractPath)?Sha(ContractPath):"missing",contractValid,exactBindingsValid,bindings,
        verdictKind=verdict,terminalStatus=verdict,auditPassed=verdict==Success,evidence,deterministic=true,
        authorityFirewalls=forbidden.ToDictionary(k=>k,_=>false),externalReviewPending=true,promotedPhysicalMassClaimCount=0};
    string json=JsonSerializer.Serialize(result,new JsonSerializerOptions {WriteIndented=true})+Environment.NewLine;
    Directory.CreateDirectory(Root+"/output");
    File.WriteAllText(Root+"/output/signed_spatial_curvature_consistency.json",json);
    File.WriteAllText(Root+"/output/signed_spatial_curvature_consistency_summary.json",json);
    Console.WriteLine($"Phase584 verdict: {verdict}");if(verdict!=Success) Environment.ExitCode=1;
}
static string Sha(string path)=>Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static double[] ReadVector(JsonElement e)=>e.EnumerateArray().Select(x=>x.GetDouble()).ToArray();
static double[] Add(double[] a,double[] b)=>a.Zip(b,(x,y)=>x+y).ToArray();
static double[] Sub(double[] a,double[] b)=>a.Zip(b,(x,y)=>x-y).ToArray();
static double[] Scale(double[] a,double t)=>a.Select(x=>x*t).ToArray();
static double[] Cross(double[] a,double[] b)=>[a[1]*b[2]-a[2]*b[1],a[2]*b[0]-a[0]*b[2],a[0]*b[1]-a[1]*b[0]];
static double Norm(double[] a)=>System.Math.Sqrt(a.Sum(x=>x*x));
static double[] Sum(double[][] a)=>a.Aggregate(new double[3],Add);
static double[] Block(double[] a,int i)=>[a[3*i],a[3*i+1],a[3*i+2]];
static double[][] AddEdges(double[][] a,double[][] b)=>a.Zip(b,Add).ToArray();
static double[] QReg(double[][] e)=>Scale(Add(Add(Cross(e[0],e[1]),Cross(e[0],e[2])),Cross(e[1],e[2])),0.5);
static double[] QLoop(double[][] e)=>Scale(Add(Add(Cross(e[0],e[2]),Cross(e[0],e[1])),Cross(e[2],e[1])),0.5);
static double SignedArea(double[][] v)=>((v[1][0]-v[0][0])*(v[2][1]-v[0][1])-(v[1][1]-v[0][1])*(v[2][0]-v[0][0]))/2;
static double[] IntegralPart(double[] u,double[] v,JsonElement p,bool varying)
{
    double dx=v[0]-u[0],dy=v[1]-u[1],mx=(u[0]+v[0])/2,my=(u[1]+v[1])/2;
    double[] first=varying?Add(Scale(ReadVector(p.GetProperty("P")),mx),Scale(ReadVector(p.GetProperty("Q")),my)):ReadVector(p.GetProperty("X"));
    double[] second=varying?Add(Scale(ReadVector(p.GetProperty("R")),mx),Scale(ReadVector(p.GetProperty("U")),my)):ReadVector(p.GetProperty("Y"));
    return Add(Scale(first,dx),Scale(second,dy));
}
static double[] Integrate(double[] u,double[] v,JsonElement p)=>Add(IntegralPart(u,v,p,false),IntegralPart(u,v,p,true));
static double[][] OrientedAnalyticEdges(double[][] v,JsonElement p,bool varying)=>
    [IntegralPart(v[0],v[1],p,varying),IntegralPart(v[2],v[0],p,varying),IntegralPart(v[1],v[2],p,varying)];
static Q Exp(double[] x) {double a=Norm(x);if(a==0)return Q.Identity;double s=System.Math.Sin(a/2)/a;return new(System.Math.Cos(a/2),s*x[0],s*x[1],s*x[2]);}
static double[] Log(Q q) {q=q.Normalized();if(q.W<0)q=-q;double v=System.Math.Sqrt(q.X*q.X+q.Y*q.Y+q.Z*q.Z);double s=v<1e-30?2:2*System.Math.Atan2(v,q.W)/v;return [s*q.X,s*q.Y,s*q.Z];}
static double LogSlope(double[] x,double[] y) {var lx=x.Select(v=>System.Math.Log(v)).ToArray();var ly=y.Select(v=>System.Math.Log(v)).ToArray();double mx=lx.Average(),my=ly.Average();return lx.Zip(ly,(a,b)=>(a-mx)*(b-my)).Sum()/lx.Sum(a=>(a-mx)*(a-mx));}
sealed record Binding(string id,string path,string sha256)
{
    public string actualSha256=>File.Exists(path)?Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant():"missing";
    public bool hashMatches=>actualSha256==sha256;
}
readonly record struct Q(double W,double X,double Y,double Z)
{
    public static Q Identity=>new(1,0,0,0);
    public static Q operator *(Q a,Q b)=>new(a.W*b.W-a.X*b.X-a.Y*b.Y-a.Z*b.Z,a.W*b.X+a.X*b.W+a.Y*b.Z-a.Z*b.Y,
        a.W*b.Y-a.X*b.Z+a.Y*b.W+a.Z*b.X,a.W*b.Z+a.X*b.Y-a.Y*b.X+a.Z*b.W);
    public static Q operator -(Q a)=>new(-a.W,-a.X,-a.Y,-a.Z);
    public Q Normalized(){double n=System.Math.Sqrt(W*W+X*X+Y*Y+Z*Z);return new(W/n,X/n,Y/n,Z/n);}
}
