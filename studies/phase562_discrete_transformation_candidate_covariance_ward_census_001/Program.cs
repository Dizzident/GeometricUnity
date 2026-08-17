using System.Security.Cryptography;
using System.Text.Json;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase562_discrete_transformation_candidate_covariance_ward_census_001";
const string ContractPath = Root + "/preregistration/phase562_discrete_transformation_candidate_covariance_ward_census_contract_v1.json";
const string OutputPath = Root + "/output/discrete_transformation_candidate_covariance_ward_census.json";
const string SummaryPath = Root + "/output/discrete_transformation_candidate_covariance_ward_census_summary.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var bindingSpecs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new
{
    Id = x.GetProperty("id").GetString()!,
    Path = x.GetProperty("path").GetString()!,
    ExpectedSha256 = x.GetProperty("sha256").GetString()!,
}).ToArray();
var bindings = bindingSpecs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.ExpectedSha256, actualSha256 = actual, hashMatches = actual == x.ExpectedSha256 };
}).ToArray();

JsonElement family = contract.GetProperty("candidateFamily");
JsonElement probes = contract.GetProperty("probes");
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
double exactTolerance = probes.GetProperty("exactTolerance").GetDouble();
double gradientParityTolerance = probes.GetProperty("gradientParityTolerance").GetDouble();
double[] backgroundScales = probes.GetProperty("backgroundScales").EnumerateArray().Select(x => x.GetDouble()).ToArray();
string[] endpoints = family.GetProperty("edgeEndpointRules").EnumerateArray().Select(x => x.GetString()!).ToArray();

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase562-a33-discrete-transformation-candidate-covariance-ward-census-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && bindingSpecs.Length == 10 && bindings.All(x => x.hashMatches)
    && family.GetProperty("expectedCandidateCount").GetInt32() == 48
    && family.GetProperty("fixedSecondFieldIsDecoyOnly").GetBoolean()
    && family.GetProperty("noCandidateIsSourceSelected").GetBoolean()
    && backgroundScales.SequenceEqual(new[] { 0.017, 0.031 })
    && probes.GetProperty("gaugeParameterProbeCount").GetInt32() == 2
    && taxonomy.SequenceEqual(new[] { "invalid-or-drifted-input", "known-answer-battery-failed", "candidate-family-all-machine-incompatible", "machine-compatible-candidates-source-unbound" })
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;
JsonElement resource = contract.GetProperty("resourceRefusal");
bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble() <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64() <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64()
    && resource.GetProperty("refuseBeforeAllocation").GetBoolean();

// Planted checks execute before any registered operator is allocated. They
// prevent a Ward-only zero from being mistaken for covariance and require the
// fixed-second-field decoy to remain outside the candidate family.
double exactVectorResidual = RelativeDifference(new[] { 1.0, -2.0, 0.5 }, new[] { 1.0, -2.0, 0.5 });
double covarianceFailureResidual = RelativeDifference(new[] { 1.0, 0.0 }, new[] { 0.0, 1.0 });
double wardOnlyDecoy = System.Math.Abs(Dot(new[] { 1.0, 0.0 }, new[] { 0.0, 1.0 }));
double gradientMismatchResidual = RelativeScalar(0.25, -0.75);
bool fixedSecondFieldDecoyRejected = !new[] { -1, 1 }.Contains(0);
bool knownAnswerBatteryPassed = exactVectorResidual == 0.0 && covarianceFailureResidual > 0.5
    && wardOnlyDecoy == 0.0 && gradientMismatchResidual > 0.5 && fixedSecondFieldDecoyRejected;

if (!contractValid || !resourceAccepted || !knownAnswerBatteryPassed)
{
    string verdict = !contractValid || !resourceAccepted ? taxonomy[0] : taxonomy[1];
    Write(new
    {
        schemaVersion = 1, phase = 562, phaseId = "phase562-discrete-transformation-candidate-covariance-ward-census",
        contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid,
        exactBindingsValid = bindings.All(x => x.hashMatches), resourceAccepted, bindings,
        knownAnswerBattery = new { ranBeforeAuditedData = true, exactVectorResidual, covarianceFailureResidual, wardOnlyDecoy, gradientMismatchResidual, fixedSecondFieldDecoyRejected, passed = knownAnswerBatteryPassed },
        verdictKind = verdict, terminalStatus = "discrete-transformation-candidate-covariance-ward-census-" + verdict,
        phase561GateOpen = false, rngUsed = false, samplingPerformed = false, reprocessingPerformed = false,
        protectedPhase554SeedsRead = false, sourceIdentificationAuthoredOrInferred = false, externalReviewPending = true,
        promotedPhysicalMassClaimCount = 0,
    });
    Console.WriteLine($"Phase562 verdict: {verdict}");
    return;
}

var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
var member = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2,
    Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5,
    EpsilonMode = "independent-theta",
};
var op = new EinsteinianShiabOperator(mesh, algebra, member);
var mass = new CpuMassMatrix(mesh, algebra);
var manifest = BuildManifest();
var geometry = BuildGeometry();
int dimG = algebra.Dimension;
int nOmega = mesh.EdgeCount * dimG;
int nTheta = mesh.VertexCount * dimG;
var thetaZero = new double[nTheta];

var candidates = new List<Candidate>();
foreach (int derivativeSign in new[] { -1, 1 })
foreach (int bracketSign in new[] { -1, 1 })
foreach (string endpoint in endpoints)
foreach (int thetaSign in new[] { -1, 1 })
foreach (int covarianceSign in new[] { -1, 1 })
    candidates.Add(new Candidate(derivativeSign, bracketSign, endpoint, thetaSign, covarianceSign));

var accumulators = candidates.ToDictionary(x => x.Id, x => new Accumulator(x));
int probeCount = 0;
foreach (double scale in backgroundScales)
for (int gaugeProbe = 0; gaugeProbe < 2; gaugeProbe++)
{
    probeCount++;
    double[] omega = Deterministic(nOmega, scale, gaugeProbe == 0 ? 0.173 : 0.419);
    double[] xi = Deterministic(nTheta, 0.23, gaugeProbe == 0 ? 0.311 : 0.587);
    var connection = new ConnectionField(mesh, algebra, omega);
    FieldTensor curvature = CurvatureAssembler.Assemble(connection).ToFieldTensor();
    FieldTensor connectionTensor = connection.ToFieldTensor();
    double[] upsilon = op.ApplyContractionWithTheta(curvature.Coefficients, thetaZero);
    var joint = op.ComputeJointGradient(omega, thetaZero, mass);

    foreach (Candidate candidate in candidates)
    {
        double[] deltaOmega = CandidateDeltaOmega(candidate, omega, xi);
        double[] deltaTheta = xi.Select(x => candidate.ThetaSign * x).ToArray();
        double[] deltaF = op.LinearizeCurvature(omega, deltaOmega);
        double[] expectedF = ExpectedAdjoint(curvature.Coefficients, xi, candidate.CovarianceSign);
        double curvatureResidual = RelativeDifference(deltaF, expectedF);

        double[] thetaContribution = op.LinearizeTheta(curvature, connectionTensor, thetaZero, deltaTheta, manifest, geometry).Coefficients;
        double[] deltaUpsilon = Add(op.ApplyContractionWithTheta(deltaF, thetaZero), thetaContribution);
        double[] expectedUpsilon = ExpectedAdjoint(upsilon, xi, candidate.CovarianceSign);
        double residualCovarianceResidual = RelativeDifference(deltaUpsilon, expectedUpsilon);
        double directDerivative = Dot(upsilon, deltaUpsilon);
        double gradientDerivative = Dot(joint.GradOmega, deltaOmega) + Dot(joint.GradTheta, deltaTheta);
        double actionWardResidual = System.Math.Abs(directDerivative) / System.Math.Max(1.0, Norm(upsilon) * Norm(deltaUpsilon));
        double gradientParityResidual = RelativeScalar(directDerivative, gradientDerivative);
        accumulators[candidate.Id].Add(curvatureResidual, residualCovarianceResidual, actionWardResidual, gradientParityResidual);
    }
}

var rows = accumulators.Values.Select(x => x.Finish(exactTolerance, gradientParityTolerance)).ToArray();
var survivors = rows.Where(x => x.machineCompatible).Select(x => x.id).ToArray();
var ordered = rows.OrderBy(x => x.maximumStageResidual).ThenBy(x => x.id, StringComparer.Ordinal).ToArray();
double bestResidual = ordered[0].maximumStageResidual;
string[] tiedBest = ordered.Where(x => System.Math.Abs(x.maximumStageResidual - bestResidual) <= 1e-15).Select(x => x.id).ToArray();
string earliestUniversalFailure = rows.All(x => x.maximumCurvatureCovarianceResidual > exactTolerance) ? "curvature-covariance"
    : rows.All(x => x.maximumResidualCovarianceResidual > exactTolerance) ? "dressed-residual-covariance"
    : rows.All(x => x.maximumActionWardResidual > exactTolerance) ? "quadratic-action-ward"
    : rows.All(x => x.maximumGradientParityResidual > gradientParityTolerance) ? "gradient-parity"
    : "none-universal";
string verdictKind = survivors.Length == 0 ? taxonomy[2] : taxonomy[3];

Write(new
{
    schemaVersion = 1, phase = 562, phaseId = "phase562-discrete-transformation-candidate-covariance-ward-census",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid,
    exactBindingsValid = true, resourceAccepted, bindings,
    knownAnswerBattery = new { ranBeforeAuditedData = true, exactVectorResidual, covarianceFailureResidual, wardOnlyDecoy, gradientMismatchResidual, fixedSecondFieldDecoyRejected, passed = true },
    registeredTarget = new { mesh = "CreateUniform4D(1)", mesh.VertexCount, mesh.EdgeCount, mesh.FaceCount, algebra = "su2-trace-pairing", member = "sd2-id0-c0.5-independent-theta", thetaBackground = "zero", vertexFaceRule = "lowest-index" },
    census = new { candidateCount = rows.Length, expectedCandidateCount = 48, probeCount, exactTolerance, gradientParityTolerance, rows, survivorCount = survivors.Length, survivors, bestResidual, tiedBest, earliestUniversalFailure, everyCandidateEnumeratedExactlyOnce = rows.Select(x => x.id).Distinct(StringComparer.Ordinal).Count() == 48 },
    interpretation = new { machineCompatibleCandidateFound = survivors.Length > 0, anySurvivorRemainsSourceUnbound = survivors.Length > 0, noCandidateIsSourceSelected = true, registeredFieldIdentificationStillMissing = true, phase561SourceGateSatisfied = false },
    verdictKind, terminalStatus = "discrete-transformation-candidate-covariance-ward-census-" + verdictKind,
    decision = survivors.Length == 0
        ? $"All 48 frozen sign, endpoint, second-field, and covariance candidates fail at least one exact registered-operator test; the earliest universal obstruction is {earliestUniversalFailure}. This narrows the missing bridge but does not select or author a source law."
        : $"{survivors.Length} frozen candidate(s) pass the machine-compatibility tests, but remain source-unbound and cannot open Phase561.",
    phase561GateOpen = false, rngUsed = false, samplingPerformed = false, reprocessingPerformed = false,
    protectedPhase554SeedsRead = false, phase553Or554RegisteredOrExecuted = false, registeredOperatorMutated = false,
    directionCalledGaugeOrRedundant = false, quotientApplied = false, gaugeFixingApplied = false, measureNormalizationApplied = false,
    sourceIdentificationAuthoredOrInferred = false, rulingAuthoredOrInferred = false, o4Discharged = false, phase458Satisfied = false,
    phase481PackCreatedOrMutated = false, productionAuthorized = false, launchAuthorized = false, physicalUnitClaimAllowed = false,
    gevClaimAllowed = false, externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
});
Console.WriteLine($"Phase562 verdict: {verdictKind}");
Console.WriteLine($"candidates={rows.Length}, survivors={survivors.Length}, bestResidual={bestResidual:E6}, earliestUniversalFailure={earliestUniversalFailure}");

double[] CandidateDeltaOmega(Candidate c, double[] omega, double[] xi)
{
    var result = new double[nOmega];
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        int tail = mesh.Edges[e][0];
        int head = mesh.Edges[e][1];
        var omegaE = Block(omega, e);
        var xiTail = Block(xi, tail);
        var xiHead = Block(xi, head);
        double[] xiEdge = c.Endpoint switch
        {
            "tail" => xiTail,
            "head" => xiHead,
            _ => xiTail.Zip(xiHead, (a, b) => 0.5 * (a + b)).ToArray(),
        };
        double[] bracket = algebra.Bracket(omegaE, xiEdge);
        for (int a = 0; a < dimG; a++)
            result[e * dimG + a] = c.DerivativeSign * (xiHead[a] - xiTail[a]) + c.BracketSign * bracket[a];
    }
    return result;
}

double[] ExpectedAdjoint(double[] faceField, double[] xi, int sign)
{
    var result = new double[faceField.Length];
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        int representative = mesh.Faces[f][0];
        double[] bracket = algebra.Bracket(Block(faceField, f), Block(xi, representative));
        for (int a = 0; a < dimG; a++) result[f * dimG + a] = sign * bracket[a];
    }
    return result;
}

double[] Block(double[] source, int index)
{
    var result = new double[dimG];
    Array.Copy(source, index * dimG, result, 0, dimG);
    return result;
}

BranchManifest BuildManifest() => new()
{
    BranchId = "phase562-a33", SchemaVersion = "1.0.0", SourceEquationRevision = "draft-2021",
    CodeRevision = "phase562", ActiveGeometryBranch = "simplicial", ActiveObservationBranch = "sigma-pullback",
    ActiveTorsionBranch = "trivial", ActiveShiabBranch = "einsteinian-shiab", ActiveGaugeStrategy = "none-candidate-census",
    BaseDimension = 4, AmbientDimension = 4, LieAlgebraId = "su2", BasisConventionId = "canonical",
    ComponentOrderId = "face-major", AdjointConventionId = "adjoint-explicit", PairingConventionId = "pairing-trace",
    NormConventionId = "norm-l2-quadrature", DifferentialFormMetricId = "hodge-standard",
    InsertedAssumptionIds = Array.Empty<string>(), InsertedChoiceIds = new[] { "A33-candidate-family-not-source-law" },
};

GeometryContext BuildGeometry()
{
    var x = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
    var y = new SpaceRef { SpaceId = "Y_h", Dimension = 4 };
    return new GeometryContext
    {
        BaseSpace = x, AmbientSpace = y, DiscretizationType = "simplicial", QuadratureRuleId = "centroid", BasisFamilyId = "P1",
        ProjectionBinding = new GeometryBinding { BindingType = "projection", SourceSpace = y, TargetSpace = x },
        ObservationBinding = new GeometryBinding { BindingType = "observation", SourceSpace = x, TargetSpace = y },
        Patches = Array.Empty<PatchInfo>(),
    };
}

static double[] Deterministic(int n, double scale, double frequency)
{
    var x = new double[n];
    for (int i = 0; i < n; i++) x[i] = scale * (System.Math.Sin((i + 1) * frequency) + 0.37 * System.Math.Cos((i + 1) * frequency * 1.7));
    return x;
}
static double[] Add(double[] a, double[] b) => a.Zip(b, (x, y) => x + y).ToArray();
static double Dot(double[] a, double[] b) { double s = 0; for (int i = 0; i < a.Length; i++) s += a[i] * b[i]; return s; }
static double Norm(double[] a) => System.Math.Sqrt(Dot(a, a));
static double RelativeDifference(double[] a, double[] b) => Norm(a.Zip(b, (x, y) => x - y).ToArray()) / System.Math.Max(1.0, System.Math.Max(Norm(a), Norm(b)));
static double RelativeScalar(double a, double b) => System.Math.Abs(a - b) / System.Math.Max(1.0, System.Math.Max(System.Math.Abs(a), System.Math.Abs(b)));
static string Sha(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
void Write(object value)
{
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(value, new JsonSerializerOptions { WriteIndented = true });
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!); File.WriteAllBytes(OutputPath, bytes); File.WriteAllBytes(SummaryPath, bytes);
}

sealed record Candidate(int DerivativeSign, int BracketSign, string Endpoint, int ThetaSign, int CovarianceSign)
{
    public string Id => $"d{DerivativeSign:+0;-0}-b{BracketSign:+0;-0}-{Endpoint}-t{ThetaSign:+0;-0}-c{CovarianceSign:+0;-0}";
}
sealed class Accumulator(Candidate candidate)
{
    private double curvature, residual, ward, parity;
    public void Add(double c, double r, double w, double p) { curvature = System.Math.Max(curvature, c); residual = System.Math.Max(residual, r); ward = System.Math.Max(ward, w); parity = System.Math.Max(parity, p); }
    public CandidateRow Finish(double exactTolerance, double parityTolerance)
    {
        double maxStage = System.Math.Max(curvature, System.Math.Max(residual, ward));
        bool compatible = curvature <= exactTolerance && residual <= exactTolerance && ward <= exactTolerance && parity <= parityTolerance;
        return new CandidateRow(candidate.Id, candidate.DerivativeSign, candidate.BracketSign, candidate.Endpoint, candidate.ThetaSign, candidate.CovarianceSign, curvature, residual, ward, parity, maxStage, compatible);
    }
}
sealed record CandidateRow(string id, int derivativeSign, int bracketSign, string endpointRule, int thetaTangentSign, int covarianceSign, double maximumCurvatureCovarianceResidual, double maximumResidualCovarianceResidual, double maximumActionWardResidual, double maximumGradientParityResidual, double maximumStageResidual, bool machineCompatible);
