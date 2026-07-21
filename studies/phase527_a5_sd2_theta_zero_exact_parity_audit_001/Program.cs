using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using Gu.Geometry;

const string Root = "studies/phase527_a5_sd2_theta_zero_exact_parity_audit_001";
const string ContractPath = Root + "/preregistration/phase527_a5_sd2_theta_zero_exact_parity_contract_v1.json";
const string OutputPath = Root + "/output/a5_sd2_theta_zero_exact_parity_audit.json";
const string SummaryPath = Root + "/output/a5_sd2_theta_zero_exact_parity_audit_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
JsonElement expected = contract.GetProperty("expectedBindings");

var specs = new[]
{
    Spec("phase452-program", "studies/phase452_scalar_channel_spectroscopy_probe_001/Program.cs", "phase452ProgramSha256"),
    Spec("phase452-summary", "studies/phase452_scalar_channel_spectroscopy_probe_001/output/scalar_channel_spectroscopy_probe_summary.json", "phase452SummarySha256"),
    Spec("curvature-assembler", "src/Gu.ReferenceCpu/CurvatureAssembler.cs", "curvatureAssemblerSha256"),
    Spec("shiab-operator", "src/Gu.ReferenceCpu/EinsteinianShiabOperator.cs", "shiabOperatorSha256"),
    Spec("shiab-family-spec", "src/Gu.ReferenceCpu/EinsteinianShiabFamilySpec.cs", "shiabFamilySpecSha256"),
    Spec("lambda2-algebra", "src/Gu.ReferenceCpu/Lambda2Algebra.cs", "lambda2AlgebraSha256"),
    Spec("cpu-mass-matrix", "src/Gu.ReferenceCpu/CpuMassMatrix.cs", "cpuMassMatrixSha256"),
    Spec("lie-algebra-factory", "src/Gu.Math/LieAlgebraFactory.cs", "lieAlgebraFactorySha256"),
    Spec("mesh-generator", "src/Gu.Geometry/SimplicialMeshGenerator.cs", "meshGeneratorSha256"),
    Spec("mesh-topology-builder", "src/Gu.Geometry/MeshTopologyBuilder.cs", "meshTopologyBuilderSha256"),
    Spec("a5-generator", "studies/phase456_consolidated_n4_launch_001/preregistration/a4a5_pack_generator/Program.cs", "a5GeneratorSha256"),
    Spec("serialized-a5", "studies/phase456_consolidated_n4_launch_001/preregistration/a5_gaussian_domination_stage_a.json", "serializedA5Sha256"),
    Spec("phase524-program", "studies/phase524_a5_exact_omega_parity_decomposition_001/Program.cs", "phase524ProgramSha256"),
    Spec("phase524-contract", "studies/phase524_a5_exact_omega_parity_decomposition_001/preregistration/phase524_a5_exact_omega_parity_decomposition_contract_v1.json", "phase524ContractSha256"),
    Spec("phase524-summary", "studies/phase524_a5_exact_omega_parity_decomposition_001/output/a5_exact_omega_parity_decomposition_summary.json", "phase524SummarySha256"),
    Spec("phase526-summary", "studies/phase526_a5_certificate_reducer_001/output/a5_certificate_reducer_summary.json", "phase526SummarySha256"),
};
var bindings = specs.Select(x => Bind(x.Id, x.Path, expected.GetProperty(x.HashKey).GetString()!)).ToArray();
bool exactBindingsValid = bindings.Length == 16
    && new HashSet<string>(bindings.Select(x => x.Id), StringComparer.Ordinal).Count == 16
    && bindings.All(x => x.HashMatches);
var source = specs.ToDictionary(x => x.Id, x => File.ReadAllText(x.Path), StringComparer.Ordinal);

string[] expectedPrecedence =
[
    "invalid-or-drifted-input", "exact-sd2-theta-zero-odd-witness",
    "exact-reduction-unavailable", "frozen-search-survives-no-even-theorem",
];
string[] expectedFirewallKeys =
[
    "actionMemberSelectionAllowed", "actionOrGeometryRegistrationAllowed",
    "phase515Or516UnlockAllowed", "phase458EvaluationOrG1Allowed",
    "limbL8ClosureAllowed", "theoremOrTargetCounterexampleAllowed",
    "reflectionPositivityRulingAllowed", "samplingOrReprocessingAllowed",
    "hmcOrBenchmarkAllowed", "productionOrLaunchAllowed",
    "sourceContractApplicationAllowed", "physicalUnitOrGevClaimAllowed",
];
bool contractValid =
    contract.GetProperty("contractId").GetString() == "phase527-a20-sd2-theta-zero-exact-parity-v1"
    && contract.GetProperty("planSection").GetString() == "WAVE2_AMENDMENTS_2026-07-12 A20"
    && contract.GetProperty("frozenBeforeEvaluation").GetBoolean()
    && expected.EnumerateObject().Count() == specs.Length
    && contract.GetProperty("extent").GetInt32() == 3
    && contract.GetProperty("latticeCanonical").GetBoolean()
    && contract.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && contract.GetProperty("memberId").GetString() == "sd2-id0/c0.5"
    && contract.GetProperty("maximumWitnessRows").GetInt32() == 768
    && contract.GetProperty("terminalPrecedence").EnumerateArray().Select(x => x.GetString()).SequenceEqual(expectedPrecedence)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().Select(x => x.Name).SequenceEqual(expectedFirewallKeys)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

bool sourceSemanticsValid =
    source["phase452-program"].Contains("Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0", StringComparison.Ordinal)
    && source["phase452-program"].Contains("EinsteinCoefficient = 0.5, EpsilonMode = \"independent-theta\"", StringComparison.Ordinal)
    && source["shiab-family-spec"].Contains("theta=0 slice", StringComparison.Ordinal)
    && source["shiab-operator"].Contains("theta = 0 => Ad = I", StringComparison.Ordinal)
    && source["shiab-operator"].Contains("var wwtInv = Lambda2Algebra.Invert(wwt)", StringComparison.Ordinal)
    && source["shiab-operator"].Contains("acc[f * _dimG + a] *= inv", StringComparison.Ordinal)
    && source["lambda2-algebra"].Contains("SelfDualProjector() => ScaleAdd(Identity(Dim), 0.5, HodgeStar(), 0.5)", StringComparison.Ordinal)
    && source["lambda2-algebra"].Contains("var inner = ScaleAdd(Identity(Dim), 1.0, a2, -member.EinsteinCoefficient)", StringComparison.Ordinal)
    && source["curvature-assembler"].Contains("dOmega[a] + 0.5 * wedgeTerm[a]", StringComparison.Ordinal)
    && source["cpu-mass-matrix"].Contains("return 0.5 * InnerProduct(upsilon, upsilon)", StringComparison.Ordinal);

using var p524 = JsonDocument.Parse(source["phase524-summary"]);
using var p526 = JsonDocument.Parse(source["phase526-summary"]);
bool precursorSemanticsValid =
    p524.RootElement.GetProperty("verdictKind").GetString() == "exact-omega-parity-refuted-identity-member"
    && p524.RootElement.GetProperty("memberDecompositions")[1].GetProperty("memberId").GetString() == "sd2-id0/c0.5"
    && !p524.RootElement.GetProperty("memberDecompositions")[1].GetProperty("exactOddCancellationResolved").GetBoolean()
    && p526.RootElement.GetProperty("verdictKind").GetString() == "unresolved-member-dependence-for-named-a5-prerequisites"
    && p526.RootElement.GetProperty("omegaParityReduction").GetProperty("sd2Member").GetProperty("unresolved").GetBoolean();

int extent = contract.GetProperty("extent").GetInt32();
int[] seedFaces = contract.GetProperty("seedFaceMenu").EnumerateArray().Select(x => x.GetInt32()).ToArray();
string[] permutations = contract.GetProperty("basisPermutationOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
int[] signMasks = contract.GetProperty("signMaskOrder").EnumerateArray().Select(x => x.GetInt32()).ToArray();
bool frozenMenuValid = seedFaces.SequenceEqual(Enumerable.Range(0, 16))
    && permutations.SequenceEqual(new[] { "012", "021", "102", "120", "201", "210" }, StringComparer.Ordinal)
    && signMasks.SequenceEqual(Enumerable.Range(0, 8))
    && seedFaces.Length * permutations.Length * signMasks.Length == 768;

SimplicialMesh mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
var exactContraction = ExactContraction.Build(mesh, extent);
bool exactReductionAvailable = exactContraction.Valid
    && mesh.VertexCount == 81 && mesh.EdgeCount == 1215 && mesh.FaceCount == 4050
    && mesh.CellFaces.All(x => x.Length == 10);

Witness? witness = null;
int rowsEvaluated = 0;
if (exactReductionAvailable)
{
    foreach (int face in seedFaces)
    {
        foreach (string permutation in permutations)
        {
            foreach (int signMask in signMasks)
            {
                rowsEvaluated++;
                var trial = EvaluateWitness(mesh, exactContraction, face, permutation, signMask);
                if (!trial.ActionPlusMinusDifference.IsZero)
                {
                    witness = trial;
                    goto SearchDone;
                }
            }
        }
    }
}
SearchDone:

bool inputsValid = contractValid && exactBindingsValid && sourceSemanticsValid
    && precursorSemanticsValid && frozenMenuValid;
bool exactOddWitness = inputsValid && exactReductionAvailable && witness is not null;
string verdict = EvaluateVerdict(!inputsValid, exactOddWitness, exactReductionAvailable);
var precedenceBattery = new[]
{
    Case("invalid", true, true, true, "invalid-or-drifted-input"),
    Case("odd-witness", false, true, true, "exact-sd2-theta-zero-odd-witness"),
    Case("reduction-unavailable", false, false, false, "exact-reduction-unavailable"),
    Case("search-survives", false, false, true, "frozen-search-survives-no-even-theorem"),
};
bool precedenceBatteryPassed = precedenceBattery.All(x => x.Passed);

var result = new
{
    schemaVersion = 1,
    phase = 527,
    phaseId = "phase527-a5-sd2-theta-zero-exact-parity-audit",
    contractId = contract.GetProperty("contractId").GetString(),
    planSection = contract.GetProperty("planSection").GetString(),
    inputsValid,
    contractValid,
    exactBindingsValid,
    sourceSemanticsValid,
    precursorSemanticsValid,
    frozenMenuValid,
    exactReductionAvailable,
    exactRationalArithmeticOnly = true,
    binary64ReplayUsedAsEvidence = false,
    rowsEvaluated,
    maximumWitnessRows = 768,
    stoppedAtFirstExactNonzeroWitness = witness is not null,
    precedenceBatteryPassed,
    precedenceBattery,
    verdictKind = verdict,
    terminalStatus = "a5-sd2-theta-zero-exact-parity-audit-" + verdict,
    decision = exactOddWitness
        ? "The exact theta-zero reduction of the registered SD2 member has a nonzero full-action cubic coefficient on the frozen n=3 signed-edge menu. Omega-sign evenness is refuted for this member and slice; no general-theta, reflection-positivity, or target theorem follows."
        : "No exact odd witness is promoted. Search survival would be limited to the frozen menu and would not establish evenness.",
    member = new
    {
        memberId = "sd2-id0/c0.5",
        thetaRule = "theta-identically-zero",
        exactLambda2Endomorphism = "R=(I+star)/4",
        exactFaceContraction = "C=cell-incidence-average of W^T R (W W^T)^-1 W",
        scope = "exact rational action implied by the bound Phase452 SD2 theta-zero formulas on the complete lattice-canonical n=3 complex",
        omegaParityStatus = exactOddWitness ? "refuted-by-exact-theta-zero-full-action-witness" : "unresolved",
    },
    witness = witness is null ? null : new
    {
        witness.SeedFaceIndex,
        witness.Permutation,
        witness.SignMask,
        witness.WitnessEdges,
        witness.WitnessBoundaryOrientations,
        totalContractedLdotW = witness.ActionPlusMinusDifference.ToString(),
        cubicCoefficient = (witness.ActionPlusMinusDifference / new Q(2)).ToString(),
        actionPlusMinusDifference = witness.ActionPlusMinusDifference.ToString(),
        witness.NonzeroContractedFaceCount,
        allFacesIncluded = true,
    },
    proofPolicy = new
    {
        generalThetaParityClaimed = false,
        frozenSearchSurvivalTreatedAsEvennessProof = false,
        commentsOrSerializedProseUsedAsProof = false,
        floatingPointUsedAsProof = false,
        actionMemberSelected = false,
    },
    bindings,
    deterministicZeroSampling = true,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    externalReviewPending = true,
    actionMemberSelected = false,
    actionOrGeometryRegistered = false,
    phase515MayBeCreated = false,
    phase516MayBeCreated = false,
    phase458EvaluationPerformed = false,
    phase458G1Satisfied = false,
    closesLimbL8 = false,
    theoremClaimed = false,
    targetCounterexampleClaimed = false,
    reflectionPositivityEstablished = false,
    reflectionPositivityRefuted = false,
    samplingPerformed = false,
    reprocessingPerformed = false,
    hmcPerformed = false,
    benchmarkPerformed = false,
    productionAuthorized = false,
    launchAuthorized = false,
    sourceContractApplicationPerformed = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    allExecutionAndPromotionAuthorities = false,
    promotedPhysicalMassClaimCount = 0,
};

Console.WriteLine($"Phase527 gates: contract={contractValid}, bindings={exactBindingsValid}, source={sourceSemanticsValid}, precursor={precursorSemanticsValid}, menu={frozenMenuValid}, reduction={exactReductionAvailable}");
Require(contractValid, "Phase527 frozen contract invalid.");
Require(inputsValid, "Phase527 inputs invalid or drifted.");
Require(precedenceBatteryPassed, "Phase527 precedence battery failed.");
Require(verdict == contract.GetProperty("expectedCurrentVerdict").GetString(), "Phase527 verdict differs from the frozen expected branch.");
Require("a5-sd2-theta-zero-exact-parity-audit-" + verdict == contract.GetProperty("expectedCurrentTerminalStatus").GetString(), "Phase527 terminal drifted.");

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
byte[] json = JsonSerializer.SerializeToUtf8Bytes(result, jsonOptions);
File.WriteAllBytes(OutputPath, json);
File.WriteAllBytes(SummaryPath, json);
Console.WriteLine($"Phase527 verdict: {verdict}");
Console.WriteLine($"rows evaluated: {rowsEvaluated}/768");
Console.WriteLine($"exact action difference: {witness?.ActionPlusMinusDifference.ToString() ?? "none"}");

static Witness EvaluateWitness(SimplicialMesh mesh, ExactContraction contraction, int seedFace, string permutation, int signMask)
{
    var omega = new long[mesh.EdgeCount, 3];
    int[] edges = mesh.FaceBoundaryEdges[seedFace];
    int[] orientations = mesh.FaceBoundaryOrientations[seedFace];
    for (int i = 0; i < 3; i++)
    {
        int basis = permutation[i] - '0';
        int sign = ((signMask >> i) & 1) == 0 ? 1 : -1;
        omega[edges[i], basis] = orientations[i] * sign;
    }

    var linear = new Q[mesh.FaceCount, 3];
    var wedge = new Q[mesh.FaceCount, 3];
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        var x = new long[3, 3];
        for (int i = 0; i < 3; i++)
            for (int a = 0; a < 3; a++)
                x[i, a] = mesh.FaceBoundaryOrientations[f][i] * omega[mesh.FaceBoundaryEdges[f][i], a];
        for (int a = 0; a < 3; a++)
            linear[f, a] = new Q(x[0, a] + x[1, a] + x[2, a]);
        for (int i = 0; i < 3; i++)
            for (int j = i + 1; j < 3; j++)
            {
                wedge[f, 0] += new Q(x[i, 1] * x[j, 2] - x[i, 2] * x[j, 1]);
                wedge[f, 1] += new Q(x[i, 2] * x[j, 0] - x[i, 0] * x[j, 2]);
                wedge[f, 2] += new Q(x[i, 0] * x[j, 1] - x[i, 1] * x[j, 0]);
            }
    }
    Q[,] cl = contraction.Apply(linear);
    Q[,] cw = contraction.Apply(wedge);
    Q dot = Q.Zero;
    int nonzeroFaces = 0;
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        Q faceDot = Q.Zero;
        for (int a = 0; a < 3; a++) faceDot += cl[f, a] * cw[f, a];
        if (!faceDot.IsZero) nonzeroFaces++;
        dot += faceDot;
    }
    return new Witness(seedFace, permutation, signMask, edges, orientations, dot, nonzeroFaces);
}

static string EvaluateVerdict(bool invalid, bool witness, bool exactReduction) =>
    invalid ? "invalid-or-drifted-input"
    : witness ? "exact-sd2-theta-zero-odd-witness"
    : !exactReduction ? "exact-reduction-unavailable"
    : "frozen-search-survives-no-even-theorem";

static PrecedenceCase Case(string id, bool invalid, bool witness, bool exactReduction, string expected)
{
    string actual = EvaluateVerdict(invalid, witness, exactReduction);
    return new(id, expected, actual, actual == expected);
}

static (string Id, string Path, string HashKey) Spec(string id, string path, string hashKey) => (id, path, hashKey);

static Binding Bind(string id, string path, string expectedSha256)
{
    string actual = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
    return new(id, path, expectedSha256, actual, actual == expectedSha256);
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

sealed class ExactContraction
{
    private readonly SimplicialMesh _mesh;
    private readonly Q[][,] _transforms;
    private readonly int[] _incidence;

    private ExactContraction(SimplicialMesh mesh, Q[][,] transforms, int[] incidence, bool valid)
    {
        _mesh = mesh;
        _transforms = transforms;
        _incidence = incidence;
        Valid = valid;
    }

    internal bool Valid { get; }

    internal static ExactContraction Build(SimplicialMesh mesh, int period)
    {
        var transforms = new Q[mesh.CellCount][,];
        var incidence = new int[mesh.FaceCount];
        bool valid = true;
        for (int c = 0; c < mesh.CellCount; c++)
        {
            int[] faces = mesh.CellFaces[c];
            int n = faces.Length;
            var w = Matrix(6, n);
            for (int j = 0; j < n; j++)
            {
                int[] verts = mesh.Faces[faces[j]];
                ReadOnlySpan<double> pa = mesh.GetVertexCoordinates(verts[0]);
                ReadOnlySpan<double> pb = mesh.GetVertexCoordinates(verts[1]);
                ReadOnlySpan<double> pc = mesh.GetVertexCoordinates(verts[2]);
                var u = new int[4];
                var v = new int[4];
                for (int d = 0; d < 4; d++)
                {
                    u[d] = MinimalImage(checked((int)System.Math.Round(pb[d] - pa[d])), period);
                    v[d] = MinimalImage(checked((int)System.Math.Round(pc[d] - pa[d])), period);
                }
                int[] biv =
                [
                    u[0] * v[1] - u[1] * v[0], u[0] * v[2] - u[2] * v[0],
                    u[0] * v[3] - u[3] * v[0], u[1] * v[2] - u[2] * v[1],
                    u[1] * v[3] - u[3] * v[1], u[2] * v[3] - u[3] * v[2],
                ];
                for (int k = 0; k < 6; k++) w[k, j] = new Q(biv[k]);
                incidence[faces[j]]++;
            }
            Q[,] wt = Transpose(w);
            Q[,] wwt = Multiply(w, wt);
            if (!TryInvert(wwt, out Q[,] inverse))
            {
                valid = false;
                transforms[c] = Matrix(n, n);
                continue;
            }
            Q[,] rMinusI = RMinusIdentity();
            Q[,] faceMap = Multiply(Multiply(Multiply(wt, rMinusI), inverse), w);
            for (int i = 0; i < n; i++) faceMap[i, i] += Q.One;
            transforms[c] = faceMap;
        }
        valid &= incidence.All(x => x > 0);
        return new ExactContraction(mesh, transforms, incidence, valid);
    }

    internal Q[,] Apply(Q[,] input)
    {
        var output = new Q[_mesh.FaceCount, 3];
        for (int c = 0; c < _mesh.CellCount; c++)
        {
            int[] faces = _mesh.CellFaces[c];
            Q[,] transform = _transforms[c];
            for (int j = 0; j < faces.Length; j++)
                for (int k = 0; k < faces.Length; k++)
                    if (!transform[j, k].IsZero)
                        for (int a = 0; a < 3; a++)
                            output[faces[j], a] += transform[j, k] * input[faces[k], a];
        }
        for (int f = 0; f < _mesh.FaceCount; f++)
            for (int a = 0; a < 3; a++)
                output[f, a] /= new Q(_incidence[f]);
        return output;
    }

    private static int MinimalImage(int value, int period)
    {
        int half = period / 2;
        if (value > half) value -= period;
        if (value < -half) value += period;
        return value;
    }

    private static Q[,] RMinusIdentity()
    {
        var r = Matrix(6, 6);
        for (int i = 0; i < 6; i++) r[i, i] = new Q(-3, 4);
        int[] mate = [5, 4, 3, 2, 1, 0];
        int[] sign = [1, -1, 1, 1, -1, 1];
        for (int col = 0; col < 6; col++) r[mate[col], col] += new Q(sign[col], 4);
        return r;
    }

    private static Q[,] Matrix(int rows, int cols) => new Q[rows, cols];

    private static Q[,] Transpose(Q[,] a)
    {
        var t = Matrix(a.GetLength(1), a.GetLength(0));
        for (int i = 0; i < a.GetLength(0); i++)
            for (int j = 0; j < a.GetLength(1); j++) t[j, i] = a[i, j];
        return t;
    }

    private static Q[,] Multiply(Q[,] a, Q[,] b)
    {
        int rows = a.GetLength(0), inner = a.GetLength(1), cols = b.GetLength(1);
        if (b.GetLength(0) != inner) throw new InvalidOperationException("Exact matrix dimensions disagree.");
        var result = Matrix(rows, cols);
        for (int i = 0; i < rows; i++)
            for (int k = 0; k < inner; k++)
                if (!a[i, k].IsZero)
                    for (int j = 0; j < cols; j++) result[i, j] += a[i, k] * b[k, j];
        return result;
    }

    private static bool TryInvert(Q[,] input, out Q[,] inverse)
    {
        int n = input.GetLength(0);
        var a = (Q[,])input.Clone();
        inverse = Matrix(n, n);
        for (int i = 0; i < n; i++) inverse[i, i] = Q.One;
        for (int col = 0; col < n; col++)
        {
            int pivot = Enumerable.Range(col, n - col).FirstOrDefault(r => !a[r, col].IsZero, -1);
            if (pivot < 0) return false;
            if (pivot != col) { SwapRows(a, pivot, col); SwapRows(inverse, pivot, col); }
            Q diagonal = a[col, col];
            for (int j = 0; j < n; j++) { a[col, j] /= diagonal; inverse[col, j] /= diagonal; }
            for (int r = 0; r < n; r++)
            {
                if (r == col || a[r, col].IsZero) continue;
                Q factor = a[r, col];
                for (int j = 0; j < n; j++)
                {
                    a[r, j] -= factor * a[col, j];
                    inverse[r, j] -= factor * inverse[col, j];
                }
            }
        }
        return true;
    }

    private static void SwapRows(Q[,] a, int x, int y)
    {
        for (int j = 0; j < a.GetLength(1); j++) (a[x, j], a[y, j]) = (a[y, j], a[x, j]);
    }
}

readonly struct Q : IEquatable<Q>
{
    internal static Q Zero => new(BigInteger.Zero, BigInteger.One);
    internal static Q One => new(BigInteger.One, BigInteger.One);
    internal Q(long numerator) : this(new BigInteger(numerator), BigInteger.One) { }
    internal Q(long numerator, long denominator) : this(new BigInteger(numerator), new BigInteger(denominator)) { }
    private Q(BigInteger numerator, BigInteger denominator)
    {
        if (denominator.IsZero) throw new DivideByZeroException();
        if (denominator.Sign < 0) { numerator = -numerator; denominator = -denominator; }
        BigInteger gcd = BigInteger.GreatestCommonDivisor(BigInteger.Abs(numerator), denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }
    private BigInteger Numerator { get; }
    private BigInteger Denominator { get; }
    private BigInteger D => Denominator.IsZero ? BigInteger.One : Denominator;
    internal bool IsZero => Numerator.IsZero;
    public static Q operator +(Q x, Q y) => new(x.Numerator * y.D + y.Numerator * x.D, x.D * y.D);
    public static Q operator -(Q x, Q y) => new(x.Numerator * y.D - y.Numerator * x.D, x.D * y.D);
    public static Q operator *(Q x, Q y) => new(x.Numerator * y.Numerator, x.D * y.D);
    public static Q operator /(Q x, Q y) => new(x.Numerator * y.D, x.D * y.Numerator);
    public bool Equals(Q other) => Numerator == other.Numerator && D == other.D;
    public override bool Equals(object? obj) => obj is Q other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Numerator, D);
    public override string ToString() => D == BigInteger.One ? Numerator.ToString() : $"{Numerator}/{D}";
}

sealed record Binding(string Id, string Path, string ExpectedSha256, string ActualSha256, bool HashMatches);
sealed record PrecedenceCase(string Id, string Expected, string Actual, bool Passed);
sealed record Witness(int SeedFaceIndex, string Permutation, int SignMask, int[] WitnessEdges,
    int[] WitnessBoundaryOrientations, Q ActionPlusMinusDifference, int NonzeroContractedFaceCount);
