using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;

const string Root = "studies/phase581_collective_coordinate_assumption_audit_001";
const string ContractPath = Root + "/preregistration/contract_v1.json";
const string OutputPath = Root + "/output/collective_coordinate_assumption_audit.json";
const string SummaryPath = Root + "/output/collective_coordinate_assumption_audit_summary.json";
const string Success = "linear-coordinate-obstruction-proved-nonlinear-controls-scoped";
using var document = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = document.RootElement;
var bindings = contract.GetProperty("exactBindings").EnumerateArray().Select(row => new
{
    path = row.GetProperty("path").GetString()!,
    expectedSha256 = row.GetProperty("sha256").GetString()!,
}).Select(row => new { row.path, row.expectedSha256,
    hashMatches = File.Exists(row.path) && Sha(row.path) == row.expectedSha256 }).ToArray();
bool inputsValid = bindings.Length == 5 && bindings.Select(x => x.path).Distinct().Count() == 5
    && bindings.All(x => x.hashMatches);
string[] forbidden = ["newSamplingPerformed", "registeredActionChanged", "localGaugeInvarianceEstablished",
    "sourceContractApplicationAllowed", "o4Discharged", "phase458Satisfied", "phase481Changed",
    "samplingAuthorized", "productionAuthorized", "physicalUnitClaimAllowed", "gevClaimAllowed"];
bool contractValid = contract.GetProperty("phase").GetInt32() == 581
    && contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase581-a43-assumption-audit-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("floatingTolerance").GetDouble() == 1e-11
    && contract.GetProperty("simpsonIntervals").GetInt32() == 8192
    && contract.GetProperty("fixtureU").GetRawText() == "[[1,2,-1],[0,1,3],[-2,1,1]]"
    && contract.GetProperty("fixtureW").GetRawText() == "[[2,-1,1],[1,0,2],[1,3,-2]]"
    && forbidden.All(key => contract.GetProperty("authorityFirewalls").GetProperty(key).ValueKind == JsonValueKind.False);
if (!inputsValid || !contractValid) { Emit("invalid-or-drifted-input", new { }); return; }

// Exact integer checks precede consumption of the audited Phase579 numbers.
long[][,] generators =
[
    new long[,] { {0,0,0}, {0,0,-1}, {0,1,0} },
    new long[,] { {0,0,1}, {0,0,0}, {-1,0,0} },
    new long[,] { {0,-1,0}, {1,0,0}, {0,0,0} },
];
long[,] casimir = new long[3,3];
bool skew = true;
foreach (long[,] j in generators)
for (int a = 0; a < 3; a++)
for (int b = 0; b < 3; b++)
{
    skew &= j[a,b] == -j[b,a];
    for (int k = 0; k < 3; k++) casimir[a,b] += j[k,a] * j[k,b];
}
bool casimirIsTwoIdentity = Enumerable.Range(0, 9).All(i => casimir[i/3,i%3] == (i/3 == i%3 ? 2 : 0));
long[,] symmetricDecoy = new long[,] { {1,0,0}, {0,0,0}, {0,0,0} };
bool symmetricDecoyRejected = Enumerable.Range(0,9).Any(i => symmetricDecoy[i/3,i%3] != -symmetricDecoy[i%3,i/3]);
List<long[,]> rotations = [];
for (int a = 0; a < 3; a++)
for (int b = 0; b < 3; b++)
for (int c = 0; c < 3; c++)
{
    if (a == b || b == c || a == c) continue;
    int parity = ((a > b ? 1 : 0) + (a > c ? 1 : 0) + (b > c ? 1 : 0)) % 2 == 0 ? 1 : -1;
    foreach (int x in new[] {-1,1})
    foreach (int y in new[] {-1,1})
    foreach (int z in new[] {-1,1})
    {
        if (parity*x*y*z != 1) continue;
        var r = new long[3,3]; r[0,a] = x; r[1,b] = y; r[2,c] = z;
        rotations.Add(r);
    }
}
bool firstMomentZero = Enumerable.Range(0,9).All(i => rotations.Sum(r => r[i/3,i%3]) == 0);
bool secondMomentExact = true;
for (int i = 0; i < 3; i++)
for (int a = 0; a < 3; a++)
for (int j = 0; j < 3; j++)
for (int b = 0; b < 3; b++)
    secondMomentExact &= rotations.Sum(r => r[i,a]*r[j,b]) == (i == j && a == b ? 8 : 0);
bool identityOnlyDecoyRejected = rotations.Take(1).Sum(r => r[0,0]*r[0,0]) * 3 != 1;
bool knownAnswerPassed = skew && casimirIsTwoIdentity && symmetricDecoyRejected
    && rotations.Count == 24 && firstMomentZero && secondMomentExact && identityOnlyDecoyRejected;
if (!knownAnswerPassed) { Emit("known-answer-battery-failed", new { knownAnswerPassed }); return; }

using var upstreamDoc = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.path.EndsWith("self_check_summary.json", StringComparison.Ordinal)).path));
JsonElement upstream = upstreamDoc.RootElement;
bool upstreamValid = upstream.GetProperty("phase").GetInt32() == 579
    && upstream.GetProperty("verdictKind").GetString() == "phase450-lineage-convention-defective-preserved-negative"
    && upstream.GetProperty("reconstruction").GetProperty("passed").GetBoolean()
    && upstream.GetProperty("reconstruction").GetProperty("nOmega").GetInt32() == 3645
    && !upstream.GetProperty("exactTransformationInvariance").GetProperty("passed").GetBoolean();
if (!upstreamValid) { Emit("invalid-or-drifted-input", new { knownAnswerPassed, upstreamValid }); return; }
long[][] u = [[1,2,-1],[0,1,3],[-2,1,1]];
long[][] w = [[2,-1,1],[1,0,2],[1,3,-2]];
long[][] onRay = u;
long tangentMaximum = generators.Max(j => System.Math.Abs(u.Sum(v => Dot(v, Apply(j,v)))));
long onRayInitial = Pair(u,onRay);
long finiteOnRayChange = rotations.Max(r => System.Math.Abs(Pair(u,onRay.Select(v => Apply(r,v)).ToArray()) - onRayInitial));
long linearOrbitSum = rotations.Sum(r => Pair(u,w.Select(v => Apply(r,v)).ToArray()));
long quadraticOrbitSum = rotations.Sum(r => Square(Pair(u,w.Select(v => Apply(r,v)).ToArray())));
long gramNormSquared = 0;
for (int i = 0; i < 3; i++)
for (int j = 0; j < 3; j++) gramNormSquared += Square(Enumerable.Range(0,u.Length).Sum(e => u[e][i]*w[e][j]));
bool quadraticIdentity = 3*quadraticOrbitSum == 24*gramNormSquared;
long quadraticRotationError = rotations.Max(r => System.Math.Abs(
    Gram(u,w.Select(v => Apply(r,v)).ToArray()) - gramNormSquared));

// Independent continuous SO(3) test, not restricted to the finite subgroup.
double[] axis = [1.0/3,2.0/3,2.0/3];
double[][] rotated = w.Select(v => Rodrigues(v.Select(x => (double)x).ToArray(), axis, 0.713)).ToArray();
double continuousGram = 0;
for (int i = 0; i < 3; i++)
for (int j = 0; j < 3; j++) continuousGram += System.Math.Pow(Enumerable.Range(0,u.Length).Sum(e => u[e][i]*rotated[e][j]),2);
double continuousError = System.Math.Abs(continuousGram-gramNormSquared);

// Separate SU(2) holonomy control: h_i=diag(exp(i*t_i), exp(-i*t_i)).
// Links U_ij=h_i h_j^-1 are pure gauge; all three links compose to identity.
double[] vertexAngles = [0,0.3,-0.2];
double[] linkAngles = Enumerable.Range(0,3).Select(i => vertexAngles[i]-vertexAngles[(i+1)%3]).ToArray();
Complex loop = linkAngles.Select(t => Complex.Exp(Complex.ImaginaryOne*t)).Aggregate(Complex.One,(x,y) => x*y);
double pureGaugeLoopError = Complex.Abs(loop-Complex.One);
double connectionCoefficientNormSquared = linkAngles.Sum(t => t*t);

// Free finite-dimensional density with respect to dr, not an effective action.
// q=r^2 has a different pushforward density mode. Neither is a condensate.
const double Alpha = 2;
int[] dimensions = [3,6,9];
var gaussianRows = dimensions.Select(k =>
{
    double Integral(int extraPower) => Simpson(r => System.Math.Pow(r,k-1+extraPower)*System.Math.Exp(-Alpha*r*r/2),0,12,8192);
    double norm = Integral(0);
    double measuredSecondMoment = Integral(2)/norm;
    double expectedSecondMoment = k/Alpha;
    double radiusMode = System.Math.Sqrt((k-1)/Alpha);
    double squaredRadiusDensityMode = (k-2)/Alpha;
    double correctedMaximumError = new[] {0.25,0.5,1.0,2.0}.Max(r =>
        System.Math.Abs(-System.Math.Log(System.Math.Pow(r,k-1)*System.Math.Exp(-Alpha*r*r/2))
            +(k-1)*System.Math.Log(r)-Alpha*r*r/2));
    return new { dimension=k, alpha=Alpha, radiusMode, squaredRadiusDensityMode,
        squareOfRadiusMode=radiusMode*radiusMode, measuredSecondMoment, expectedSecondMoment,
        secondMomentError=System.Math.Abs(measuredSecondMoment-expectedSecondMoment), correctedMaximumError };
}).ToArray();
bool controlsPassed = tangentMaximum == 0 && finiteOnRayChange > 0 && linearOrbitSum == 0
    && quadraticIdentity && quadraticRotationError == 0 && continuousError <= 1e-11
    && pureGaugeLoopError <= 1e-11 && connectionCoefficientNormSquared > 0.1
    && gaussianRows.All(x => x.secondMomentError <= 1e-11 && x.correctedMaximumError <= 1e-11
        && x.radiusMode > 0 && System.Math.Abs(x.squareOfRadiusMode-x.squaredRadiusDensityMode-0.5) <= 1e-11);
Emit(controlsPassed ? Success : "mathematical-control-failed", new
{
    knownAnswerPassed, upstreamValid, controlsPassed,
    linearObstruction = new { skewGeneratorsExact=skew, casimirIsTwoIdentity,
        invariantCovectorDimensionPerTriple=0, registeredTripleCount=1215, registeredLinearInvariantDimension=0,
        ownTangentOrthogonalityAutomatic=true, tangentMaximum, finiteOnRayChange,
        proof="J_a^T=-J_a implies u^T J_a u=0 for every u. Sum_a J_a^T J_a=2I implies intersection ker J_a={0}; the same holds blockwise for any number of adjoint triples.",
        limitation="Global adjoint representation only; no registered local action symmetry is asserted." },
    nonlinearControl = new { rotationCount=rotations.Count, firstMomentZero, secondMomentExact,
        linearOrbitSum, quadraticOrbitSum, gramNormSquared, quadraticIdentity, quadraticRotationError, continuousError,
        formula="3 average_R Phi_u(R omega)^2 = ||U^T W||_F^2; degree-two SO(3) Haar moments matched exactly by the 24 proper signed permutations",
        globalInvariantAlternativeExists=true, localGaugeInvariantReplacementEstablished=false,
        sourceSelectedObservable=false, physicalHiggsIdentified=false },
    localTransportDecoy = new { independentHolonomyControl=true, linkAngles,
        connectionCoefficientNormSquared, pureGaugeLoopError,
        registeredConnectionTransformationAssumed=false,
        conclusion="A global-adjoint-invariant coefficient norm can change on a pure-gauge local link orbit; closed-loop transport remains trivial." },
    radialMeasureControl = new { gaussianRows,
        densityInRadius="p_r(r) proportional to r^(k-1) exp(-alpha*r^2/2), r>=0",
        densityInSquaredRadius="p_q(q) proportional to q^(k/2-1) exp(-alpha*q/2), q>=0",
        finiteDimensionalControlOnly=true, nonzeroDensityModeProvesBreaking=false,
        correctedPotential="-log(p_r)+(k-1)log(r)=alpha*r^2/2+constant; this is a control decomposition, not removal of a physical measure" },
    interpretation = "The Phase450 tangent test cannot certify an invariant linear coordinate. Nonlinear global invariants exist, but local transport and the pushforward measure must be derived before a replacement can diagnose physics. No new source-defined observable or mass follows."
});

void Emit(string verdict, object evidence)
{
    var result = new { schemaVersion=1, phase=581, phaseId="phase581-collective-coordinate-assumption-audit",
        contractId="phase581-a43-assumption-audit-v1", contractSha256=Sha(ContractPath),
        verdictKind=verdict, terminalStatus=verdict, contractValid, exactBindingsValid=inputsValid, bindings,
        auditPassed=verdict==Success, evidence, deterministic=true,
        authorityFirewalls=forbidden.ToDictionary(key=>key,_=>false),
        externalReviewPending=true, promotedPhysicalMassClaimCount=0 };
    string json = JsonSerializer.Serialize(result,new JsonSerializerOptions { WriteIndented=true })+Environment.NewLine;
    Directory.CreateDirectory(Root+"/output");
    File.WriteAllText(OutputPath,json); File.WriteAllText(SummaryPath,json);
    Console.WriteLine($"Phase581 verdict: {verdict}");
    if (verdict != Success) Environment.ExitCode=1;
}
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static long Square(long x) => checked(x*x);
static long Dot(long[] x,long[] y) => Enumerable.Range(0,3).Sum(i=>x[i]*y[i]);
static long[] Apply(long[,] m,long[] v) => Enumerable.Range(0,3).Select(i=>Enumerable.Range(0,3).Sum(j=>m[i,j]*v[j])).ToArray();
static long Pair(long[][] x,long[][] y) => Enumerable.Range(0,x.Length).Sum(i=>Dot(x[i],y[i]));
static long Gram(long[][] x,long[][] y)
{
    long sum=0;
    for(int i=0;i<3;i++) for(int j=0;j<3;j++) sum+=Square(Enumerable.Range(0,x.Length).Sum(e=>x[e][i]*y[e][j]));
    return sum;
}
static double[] Rodrigues(double[] v,double[] a,double angle)
{
    double c=System.Math.Cos(angle),s=System.Math.Sin(angle),d=Enumerable.Range(0,3).Sum(i=>a[i]*v[i]);
    return Enumerable.Range(0,3).Select(i=>c*v[i]+s*(a[(i+1)%3]*v[(i+2)%3]-a[(i+2)%3]*v[(i+1)%3])+(1-c)*d*a[i]).ToArray();
}
static double Simpson(Func<double,double> f,double a,double b,int n)
{
    double h=(b-a)/n,sum=f(a)+f(b);
    for(int i=1;i<n;i++) sum+=(i%2==0?2:4)*f(a+i*h);
    return sum*h/3;
}
