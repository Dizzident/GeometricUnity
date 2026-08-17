using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase564_additive_curvature_residual_power_decomposition_001";
const string ContractPath = Root + "/preregistration/phase564_additive_curvature_residual_power_decomposition_contract_v1.json";
const string OutputPath = Root + "/output/additive_curvature_residual_power_decomposition.json";
const string SummaryPath = Root + "/output/additive_curvature_residual_power_decomposition_summary.json";

using var contractDoc = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDoc.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
double coefficientTolerance = contract.GetProperty("absoluteCoefficientTolerance").GetDouble();
double reconstructionTolerance = contract.GetProperty("ladderReconstructionTolerance").GetDouble();
double commutingTolerance = contract.GetProperty("commutingNullTolerance").GetDouble();
double[] tValues = contract.GetProperty("amplitudes").GetProperty("t").EnumerateArray().Select(x => x.GetDouble()).ToArray();
double[] sValues = contract.GetProperty("amplitudes").GetProperty("s").EnumerateArray().Select(x => x.GetDouble()).ToArray();
var rows = contract.GetProperty("candidateRows").EnumerateArray().Select(x => new Candidate(
    x.GetProperty("id").GetString()!, x.GetProperty("derivativeSign").GetInt32(),
    x.GetProperty("bracketSign").GetInt32(), x.GetProperty("endpoint").GetString()!,
    x.GetProperty("covarianceSign").GetInt32())).ToArray();
JsonElement resource = contract.GetProperty("resourceRefusal");
bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble() <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64() <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64()
    && resource.GetProperty("refuseBeforeAllocation").GetBoolean();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase564-a34-additive-curvature-residual-power-decomposition-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && specs.Length == 8 && bindings.All(x => x.hashMatches)
    && rows.Length == 4 && rows.All(x => x.Endpoint == "tail")
    && tValues.SequenceEqual(new[] { 1.0, 0.5, 0.25, 0.125, 0.0625, 0.03125 })
    && sValues.SequenceEqual(new[] { 1.0, 0.5, 0.25, 0.125 })
    && taxonomy.Length == 7
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// Planted polynomial and orientation checks run before audited values are read.
double[] plantedR0 = { 0.0, 0.0 }, plantedR1 = { 2.0, -3.0 }, plantedR2 = { -0.5, 0.75 };
double plantedReconstructionError = 0.0;
foreach (double t in tValues)
foreach (double s in sValues)
{
    double[] direct = { s * (2.0 * t - 0.5 * t * t), s * (-3.0 * t + 0.75 * t * t) };
    plantedReconstructionError = System.Math.Max(plantedReconstructionError, MaxAbs(Subtract(direct, Reconstruct(plantedR0, plantedR1, plantedR2, t, s))));
}
double plantedCrossNorm = Norm(Cross(new[] { 1.0, 0.0, 0.0 }, new[] { 0.0, 1.0, 0.0 }));
double plantedCommutingNorm = Norm(Cross(new[] { 2.0, 0.0, 0.0 }, new[] { -3.0, 0.0, 0.0 }));
bool knownAnswerBatteryPassed = plantedReconstructionError == 0.0 && plantedCrossNorm == 1.0 && plantedCommutingNorm == 0.0;

if (!contractValid || !resourceAccepted || !knownAnswerBatteryPassed)
{
    string verdict = !contractValid || !resourceAccepted ? taxonomy[0] : taxonomy[1];
    Emit(new
    {
        schemaVersion = 1, phase = 564, phaseId = "phase564-additive-curvature-residual-power-decomposition",
        contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid,
        exactBindingsValid = bindings.All(x => x.hashMatches), resourceAccepted, bindings,
        knownAnswerBattery = new { ranBeforeAuditedData = true, plantedReconstructionError, plantedCrossNorm, plantedCommutingNorm, passed = knownAnswerBatteryPassed },
        verdictKind = verdict, terminalStatus = "additive-curvature-residual-power-decomposition-" + verdict,
        phase565GateOpen = false, phase561GateOpen = false, rngUsed = false, samplingPerformed = false,
        sourceIdentificationAuthoredOrInferred = false, externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
    });
    Console.WriteLine($"Phase564 verdict: {verdict}");
    return;
}

// Only after the battery, read the immutable A33 summaries.
JsonElement phase562 = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase562-summary").Path)).RootElement.Clone();
JsonElement phase563 = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase563-summary").Path)).RootElement.Clone();
string[] auditedBest = phase562.GetProperty("census").GetProperty("tiedBest").EnumerateArray().Select(x => x.GetString()!).Order(StringComparer.Ordinal).ToArray();
bool upstreamTerminalValid = phase562.GetProperty("verdictKind").GetString() == "candidate-family-all-machine-incompatible"
    && phase563.GetProperty("verdictKind").GetString() == "adjudication-confirms-all-candidates-machine-incompatible"
    && rows.Select(x => x.Id).Order(StringComparer.Ordinal).SequenceEqual(auditedBest, StringComparer.Ordinal);

var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
var member = new EinsteinianShiabFamilyMember { Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0, EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta" };
var op = new EinsteinianShiabOperator(mesh, algebra, member);
int g = algebra.Dimension, nOmega = mesh.EdgeCount * g, nXi = mesh.VertexCount * g;

var decompositions = new List<object>();
double globalR0 = 0.0, globalR1 = 0.0, globalR2 = 0.0, maxReconstructionError = 0.0;
foreach (Candidate candidate in rows)
foreach ((double scale, double omegaFrequency, double xiFrequency) in new[]
{
    (0.017, 0.173, 0.311), (0.017, 0.419, 0.587),
    (0.031, 0.173, 0.311), (0.031, 0.419, 0.587),
})
{
    double[] omega = Wave(nOmega, scale, omegaFrequency);
    double[] xi = Wave(nXi, 0.23, xiFrequency);
    double[] derivative = Derivative(candidate, xi);
    double[] bracket = BracketPart(candidate, omega, xi);
    double[] zero = new double[nOmega];
    double[] a0 = op.LinearizeCurvature(zero, derivative);
    double[] a1 = Subtract(op.LinearizeCurvature(omega, derivative), a0);
    double[] b1 = op.LinearizeCurvature(zero, bracket);
    double[] b2 = Subtract(op.LinearizeCurvature(omega, bracket), b1);
    double[] fPlus = Curvature(omega), fMinus = Curvature(Scale(omega, -1.0));
    double[] f1 = Scale(Subtract(fPlus, fMinus), 0.5);
    double[] f2 = Scale(Add(fPlus, fMinus), 0.5);
    double[] c1 = Adjoint(f1, xi, candidate.CovarianceSign);
    double[] c2 = Adjoint(f2, xi, candidate.CovarianceSign);
    double[] r0 = a0;
    double[] r1 = Subtract(Add(a1, b1), c1);
    double[] r2 = Subtract(b2, c2);
    double localError = 0.0;
    foreach (double t in tValues)
    foreach (double s in sValues)
    {
        double[] omegaTs = Scale(omega, t);
        double[] delta = Add(Scale(derivative, s), Scale(bracket, t * s));
        double[] actual = Subtract(op.LinearizeCurvature(omegaTs, delta), Adjoint(Curvature(omegaTs), Scale(xi, s), candidate.CovarianceSign));
        localError = System.Math.Max(localError, MaxAbs(Subtract(actual, Reconstruct(r0, r1, r2, t, s))));
    }
    double n0 = Norm(r0), n1 = Norm(r1), n2 = Norm(r2);
    globalR0 = System.Math.Max(globalR0, n0); globalR1 = System.Math.Max(globalR1, n1); globalR2 = System.Math.Max(globalR2, n2);
    maxReconstructionError = System.Math.Max(maxReconstructionError, localError);
    decompositions.Add(new { candidateId = candidate.Id, scale, omegaFrequency, xiFrequency, r0Norm = n0, r1Norm = n1, r2Norm = n2, maximumSignedTensorReconstructionError = localError });
}

// Separately governed deterministic Cartan control: all brackets vanish, while
// the identical registered linearization must preserve d(d xi)=0.
double[] cartanOmega = CartanWave(nOmega, 0.031, 0.271);
double[] cartanXi = CartanWave(nXi, 0.23, 0.463);
double maxCommutingBracket = 0.0, maxCommutingResidual = 0.0, maxOrientedDd = 0.0;
foreach (Candidate candidate in rows)
{
    double[] derivative = Derivative(candidate, cartanXi);
    double[] bracket = BracketPart(candidate, cartanOmega, cartanXi);
    maxCommutingBracket = System.Math.Max(maxCommutingBracket, MaxAbs(bracket));
    maxCommutingResidual = System.Math.Max(maxCommutingResidual, MaxAbs(op.LinearizeCurvature(cartanOmega, derivative)));
    for (int f = 0; f < mesh.FaceCount; f++)
    for (int a = 0; a < g; a++)
    {
        double dd = 0.0;
        for (int k = 0; k < mesh.FaceBoundaryEdges[f].Length; k++)
            dd += mesh.FaceBoundaryOrientations[f][k] * derivative[mesh.FaceBoundaryEdges[f][k] * g + a];
        maxOrientedDd = System.Math.Max(maxOrientedDd, System.Math.Abs(dd));
    }
}
bool commutingNullPassed = maxCommutingBracket <= commutingTolerance && maxCommutingResidual <= commutingTolerance && maxOrientedDd <= commutingTolerance;
bool decompositionExact = maxReconstructionError <= reconstructionTolerance;
string verdictKind = !upstreamTerminalValid ? taxonomy[0]
    : !commutingNullPassed ? taxonomy[2]
    : globalR0 > coefficientTolerance ? taxonomy[3]
    : !decompositionExact || (globalR1 <= coefficientTolerance && globalR2 <= coefficientTolerance) ? taxonomy[4]
    : globalR1 > coefficientTolerance ? taxonomy[5] : taxonomy[6];
bool mechanismLocalized = verdictKind == taxonomy[5] || verdictKind == taxonomy[6];

Emit(new
{
    schemaVersion = 1, phase = 564, phaseId = "phase564-additive-curvature-residual-power-decomposition",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid = true,
    exactBindingsValid = true, resourceAccepted, bindings,
    knownAnswerBattery = new { ranBeforeAuditedData = true, plantedReconstructionError, plantedCrossNorm, plantedCommutingNorm, passed = true },
    upstream = new { phase562Verdict = phase562.GetProperty("verdictKind").GetString(), phase563Verdict = phase563.GetProperty("verdictKind").GetString(), bestRowsMatch = upstreamTerminalValid },
    coefficientModel = "residual(t,s)=s*(R0+t*R1+t^2*R2)",
    decomposition = new { candidateCount = rows.Length, probeCount = decompositions.Count, coefficientTolerance, globalR0Norm = globalR0, globalR1Norm = globalR1, globalR2Norm = globalR2, maxReconstructionError, reconstructionTolerance, decompositionExact, rows = decompositions },
    commutingControl = new { separatelyGovernedDeterministicControl = true, fixedCartanGenerator = 0, maxCommutingBracket, maxOrientedDd, maxRegisteredResidual = maxCommutingResidual, tolerance = commutingTolerance, passed = commutingNullPassed },
    interpretation = new { mechanismLocalized, earliestNonzeroConnectionPower = globalR0 > coefficientTolerance ? 0 : globalR1 > coefficientTolerance ? 1 : globalR2 > coefficientTolerance ? 2 : -1, scalarPhase562ResidualUsedAsAcceptanceTarget = false, resultIsSourceSelection = false },
    verdictKind, terminalStatus = "additive-curvature-residual-power-decomposition-" + verdictKind,
    phase565GateOpen = mechanismLocalized && commutingNullPassed && decompositionExact,
    phase561GateOpen = false, rngUsed = false, samplingPerformed = false, reprocessingPerformed = false,
    protectedPhase554SeedsRead = false, phases562Or563MutatedOrReinterpreted = false, registeredOperatorMutated = false,
    sourceIdentificationAuthoredOrInferred = false, rulingAuthoredOrInferred = false, o4Discharged = false,
    phase458Satisfied = false, phase481PackCreatedOrMutated = false, productionAuthorized = false, launchAuthorized = false,
    physicalUnitClaimAllowed = false, gevClaimAllowed = false, externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
});
Console.WriteLine($"Phase564 verdict: {verdictKind}");
Console.WriteLine($"R0={globalR0:E6}, R1={globalR1:E6}, R2={globalR2:E6}, reconstruction={maxReconstructionError:E6}, commuting={maxCommutingResidual:E6}");

double[] Derivative(Candidate c, double[] xi)
{
    var result = new double[nOmega];
    for (int e = 0; e < mesh.EdgeCount; e++)
    for (int a = 0; a < g; a++)
        result[e * g + a] = c.DerivativeSign * (xi[mesh.Edges[e][1] * g + a] - xi[mesh.Edges[e][0] * g + a]);
    return result;
}
double[] BracketPart(Candidate c, double[] omega, double[] xi)
{
    var result = new double[nOmega];
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        int vertex = c.Endpoint == "head" ? mesh.Edges[e][1] : mesh.Edges[e][0];
        double[] value = algebra.Bracket(Block(omega, e), Block(xi, vertex));
        for (int a = 0; a < g; a++) result[e * g + a] = c.BracketSign * value[a];
    }
    return result;
}
double[] Adjoint(double[] faceField, double[] xi, int sign)
{
    var result = new double[faceField.Length];
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        double[] value = algebra.Bracket(Block(faceField, f), Block(xi, mesh.Faces[f][0]));
        for (int a = 0; a < g; a++) result[f * g + a] = sign * value[a];
    }
    return result;
}
double[] Curvature(double[] omega) => CurvatureAssembler.Assemble(new ConnectionField(mesh, algebra, omega)).Coefficients;
double[] Block(double[] source, int row) { var x = new double[g]; Array.Copy(source, row * g, x, 0, g); return x; }
static double[] Wave(int n, double scale, double frequency) { var x = new double[n]; for (int i = 0; i < n; i++) x[i] = scale * (System.Math.Sin((i + 1) * frequency) + 0.37 * System.Math.Cos((i + 1) * frequency * 1.7)); return x; }
static double[] CartanWave(int n, double scale, double frequency) { var x = new double[n]; for (int i = 0; i < n / 3; i++) x[3 * i] = scale * (System.Math.Sin((i + 1) * frequency) + 0.19 * System.Math.Cos((i + 1) * frequency * 1.3)); return x; }
static double[] Cross(double[] a, double[] b) => new[] { a[1] * b[2] - a[2] * b[1], a[2] * b[0] - a[0] * b[2], a[0] * b[1] - a[1] * b[0] };
static double[] Add(double[] a, double[] b) => a.Zip(b, (x, y) => x + y).ToArray();
static double[] Subtract(double[] a, double[] b) => a.Zip(b, (x, y) => x - y).ToArray();
static double[] Scale(double[] a, double s) => a.Select(x => s * x).ToArray();
static double[] Reconstruct(double[] r0, double[] r1, double[] r2, double t, double s) => r0.Zip(r1, (a, b) => a + t * b).Zip(r2, (a, b) => s * (a + t * t * b)).ToArray();
static double Norm(double[] a) => System.Math.Sqrt(a.Sum(x => x * x));
static double MaxAbs(double[] a) => a.Length == 0 ? 0.0 : a.Max(System.Math.Abs);
static string Sha(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
void Emit(object value) { byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions { WriteIndented = true }); Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!); File.WriteAllBytes(OutputPath, bytes); File.WriteAllBytes(SummaryPath, bytes); }
sealed record Binding(string Id, string Path, string Hash);
sealed record Candidate(string Id, int DerivativeSign, int BracketSign, string Endpoint, int CovarianceSign);
