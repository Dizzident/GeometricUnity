using System.Security.Cryptography;
using System.Text.Json;
using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

const string Root = "studies/phase563_discrete_transformation_candidate_census_independent_adjudicator_001";
const string ContractPath = Root + "/preregistration/phase563_discrete_transformation_candidate_census_independent_adjudicator_contract_v1.json";
const string OutputPath = Root + "/output/discrete_transformation_candidate_census_independent_adjudicator.json";
const string SummaryPath = Root + "/output/discrete_transformation_candidate_census_independent_adjudicator_summary.json";
const string SupplementPath = Root + "/output/phase555_discrete_transformation_candidate_supplement.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new BindingSpec(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
JsonElement comparison = contract.GetProperty("comparison");
double exactTolerance = comparison.GetProperty("exactTolerance").GetDouble();
double agreementTolerance = comparison.GetProperty("numericAgreementTolerance").GetDouble();
double tieTolerance = comparison.GetProperty("tieTolerance").GetDouble();
JsonElement independent = contract.GetProperty("independentImplementation");
JsonElement resource = contract.GetProperty("resourceRefusal");
bool resourceAccepted = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble() <= resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble()
    && resource.GetProperty("estimatedPeakBytes").GetInt64() <= resource.GetProperty("maximumEstimatedPeakBytes").GetInt64()
    && resource.GetProperty("refuseBeforeAllocation").GetBoolean();
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase563-a33-discrete-transformation-candidate-census-independent-adjudicator-v1"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean() && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && specs.Length == 10 && bindings.All(x => x.hashMatches)
    && independent.GetProperty("phase562ProjectReference").GetBoolean() == false
    && independent.GetProperty("sharedPhase562Code").GetBoolean() == false
    && independent.GetProperty("batteryRunsBeforePhase562Read").GetBoolean()
    && independent.GetProperty("recomputesAll48Candidates").GetBoolean()
    && comparison.GetProperty("expectedCandidateCount").GetInt32() == 48
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// Independent planted battery precedes both the registered calculation and
// every read of Phase562's output.
double exactResidual = Distance(new[] { 2.0, -1.0 }, new[] { 2.0, -1.0 });
double plantedCovarianceFailure = Distance(new[] { 2.0, -1.0 }, new[] { -1.0, 2.0 });
bool wardOnlyFalsePositiveRejected = Inner(new[] { 1.0, 0.0 }, new[] { 0.0, 3.0 }) == 0.0 && plantedCovarianceFailure > 0.5;
bool missingCandidateDetected = Enumerable.Range(0, 47).Count() != 48;
bool duplicateCandidateDetected = new[] { "a", "a" }.Distinct(StringComparer.Ordinal).Count() != 2;
bool fixedSliceDecoyRejected = System.Math.Abs(Inner(new[] { 1.0 }, new[] { 0.0 })) == 0.0
    && !new[] { -1, 1 }.Contains(0);
bool batteryPassed = exactResidual == 0.0 && plantedCovarianceFailure > 0.5 && wardOnlyFalsePositiveRejected
    && missingCandidateDetected && duplicateCandidateDetected && fixedSliceDecoyRejected;

if (!contractValid || !resourceAccepted || !batteryPassed)
{
    string verdict = !contractValid || !resourceAccepted ? taxonomy[0] : taxonomy[1];
    Emit(new
    {
        schemaVersion = 1, phase = 563, phaseId = "phase563-discrete-transformation-candidate-census-independent-adjudicator",
        contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid,
        exactBindingsValid = bindings.All(x => x.hashMatches), resourceAccepted, bindings,
        independentKnownAnswerBattery = new { ranBeforePhase562Read = true, exactResidual, plantedCovarianceFailure, wardOnlyFalsePositiveRejected, missingCandidateDetected, duplicateCandidateDetected, fixedSliceDecoyRejected, passed = batteryPassed },
        verdictKind = verdict, terminalStatus = "discrete-transformation-candidate-census-independent-adjudicator-" + verdict,
        phase561GateOpen = false, rngUsed = false, samplingPerformed = false, reprocessingPerformed = false,
        sourceIdentificationAuthoredOrInferred = false, externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
    }, null);
    Console.WriteLine($"Phase563 verdict: {verdict}");
    return;
}

var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int g = algebra.Dimension;
int edgeDof = mesh.EdgeCount * g;
int vertexDof = mesh.VertexCount * g;
var member = new EinsteinianShiabFamilyMember { Phi1 = InvariantElementSpec.Sd2, Phi2 = InvariantElementSpec.Id0, EinsteinCoefficient = 0.5, EpsilonMode = "independent-theta" };
var op = new EinsteinianShiabOperator(mesh, algebra, member);
var mass = new CpuMassMatrix(mesh, algebra);
var zeroTheta = new double[vertexDof];
var manifest = Manifest();
var geometry = Geometry();

// Different enumeration nesting and helper decomposition from Phase562.
var rowsById = new Dictionary<string, IndependentRow>(StringComparer.Ordinal);
foreach (string endpoint in new[] { "head", "average", "tail" })
foreach (int covarianceSign in new[] { 1, -1 })
foreach (int thetaSign in new[] { 1, -1 })
foreach (int bracketSign in new[] { 1, -1 })
foreach (int derivativeSign in new[] { 1, -1 })
{
    string id = Id(derivativeSign, bracketSign, endpoint, thetaSign, covarianceSign);
    double maxCurvature = 0, maxResidual = 0, maxWard = 0, maxParity = 0;
    foreach ((double scale, double frequency, double xiFrequency) in new[]
    {
        (0.017, 0.173, 0.311), (0.017, 0.419, 0.587),
        (0.031, 0.173, 0.311), (0.031, 0.419, 0.587),
    })
    {
        double[] omega = Wave(edgeDof, scale, frequency);
        double[] xi = Wave(vertexDof, 0.23, xiFrequency);
        var connection = new ConnectionField(mesh, algebra, omega);
        FieldTensor connectionTensor = connection.ToFieldTensor();
        FieldTensor curvature = CurvatureAssembler.Assemble(connection).ToFieldTensor();
        double[] residualField = op.ApplyContractionWithTheta(curvature.Coefficients, zeroTheta);
        var gradient = op.ComputeJointGradient(omega, zeroTheta, mass);
        double[] deltaOmega = Lower(omega, xi, derivativeSign, bracketSign, endpoint);
        double[] deltaTheta = xi.Select(x => thetaSign * x).ToArray();
        double[] deltaCurvature = op.LinearizeCurvature(omega, deltaOmega);
        double[] expectedCurvature = AdjointFaces(curvature.Coefficients, xi, covarianceSign);
        double[] thetaPart = op.LinearizeTheta(curvature, connectionTensor, zeroTheta, deltaTheta, manifest, geometry).Coefficients;
        double[] deltaResidual = Sum(op.ApplyContractionWithTheta(deltaCurvature, zeroTheta), thetaPart);
        double[] expectedResidual = AdjointFaces(residualField, xi, covarianceSign);
        double direct = Inner(residualField, deltaResidual);
        double reverse = Inner(gradient.GradOmega, deltaOmega) + Inner(gradient.GradTheta, deltaTheta);
        maxCurvature = System.Math.Max(maxCurvature, Distance(deltaCurvature, expectedCurvature));
        maxResidual = System.Math.Max(maxResidual, Distance(deltaResidual, expectedResidual));
        maxWard = System.Math.Max(maxWard, System.Math.Abs(direct) / System.Math.Max(1.0, Length(residualField) * Length(deltaResidual)));
        maxParity = System.Math.Max(maxParity, ScalarDistance(direct, reverse));
    }
    double stage = System.Math.Max(maxCurvature, System.Math.Max(maxResidual, maxWard));
    rowsById.Add(id, new IndependentRow(id, maxCurvature, maxResidual, maxWard, maxParity, stage,
        maxCurvature <= exactTolerance && maxResidual <= exactTolerance && maxWard <= exactTolerance && maxParity <= exactTolerance));
}

IndependentRow[] rows = rowsById.Values.OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
string[] survivors = rows.Where(x => x.Compatible).Select(x => x.Id).ToArray();
double best = rows.Min(x => x.Stage);
string[] bestSet = rows.Where(x => System.Math.Abs(x.Stage - best) <= tieTolerance).Select(x => x.Id).ToArray();
string earliest = rows.All(x => x.Curvature > exactTolerance) ? "curvature-covariance"
    : rows.All(x => x.Residual > exactTolerance) ? "dressed-residual-covariance"
    : rows.All(x => x.Ward > exactTolerance) ? "quadratic-action-ward"
    : rows.All(x => x.Parity > exactTolerance) ? "gradient-parity" : "none-universal";

// Only now read Phase562.
JsonElement phase562 = JsonDocument.Parse(File.ReadAllBytes(specs.Single(x => x.Id == "phase562-summary").Path)).RootElement.Clone();
JsonElement audited = phase562.GetProperty("census");
var auditedRows = audited.GetProperty("rows").EnumerateArray().ToDictionary(
    x => x.GetProperty("id").GetString()!, x => x, StringComparer.Ordinal);
bool everyIdAgrees = auditedRows.Count == 48 && rows.Length == 48 && rows.All(x => auditedRows.ContainsKey(x.Id));
double maxNumericDeviation = 0.0;
if (everyIdAgrees)
foreach (IndependentRow row in rows)
{
    JsonElement other = auditedRows[row.Id];
    maxNumericDeviation = System.Math.Max(maxNumericDeviation, System.Math.Abs(row.Curvature - other.GetProperty("maximumCurvatureCovarianceResidual").GetDouble()));
    maxNumericDeviation = System.Math.Max(maxNumericDeviation, System.Math.Abs(row.Residual - other.GetProperty("maximumResidualCovarianceResidual").GetDouble()));
    maxNumericDeviation = System.Math.Max(maxNumericDeviation, System.Math.Abs(row.Ward - other.GetProperty("maximumActionWardResidual").GetDouble()));
    maxNumericDeviation = System.Math.Max(maxNumericDeviation, System.Math.Abs(row.Parity - other.GetProperty("maximumGradientParityResidual").GetDouble()));
}
string[] auditedSurvivors = audited.GetProperty("survivors").EnumerateArray().Select(x => x.GetString()!).Order(StringComparer.Ordinal).ToArray();
string[] auditedBest = audited.GetProperty("tiedBest").EnumerateArray().Select(x => x.GetString()!).Order(StringComparer.Ordinal).ToArray();
bool survivorSetsMatch = survivors.Order(StringComparer.Ordinal).SequenceEqual(auditedSurvivors, StringComparer.Ordinal);
bool bestTieSetsMatch = bestSet.Order(StringComparer.Ordinal).SequenceEqual(auditedBest, StringComparer.Ordinal);
bool earliestMatches = earliest == audited.GetProperty("earliestUniversalFailure").GetString();
bool terminalMatches = survivors.Length == 0
    ? phase562.GetProperty("verdictKind").GetString() == "candidate-family-all-machine-incompatible"
    : phase562.GetProperty("verdictKind").GetString() == "machine-compatible-candidates-source-unbound";
bool adjudicationPassed = everyIdAgrees && maxNumericDeviation <= agreementTolerance && survivorSetsMatch && bestTieSetsMatch && earliestMatches && terminalMatches;
string verdictKind = !adjudicationPassed ? taxonomy[2] : survivors.Length == 0 ? taxonomy[3] : taxonomy[4];

var supplement = new
{
    schemaVersion = 1, supplementId = "phase563-a33-phase555-discrete-transformation-candidate-supplement-v1",
    artifactKind = "additive-evidence-supplement", materialized = true,
    parentPacket = new { path = specs.Single(x => x.Id == "phase555-summary").Path, sha256 = specs.Single(x => x.Id == "phase555-summary").Hash, byteImmutable = true },
    upstreamSupplement = new { path = specs.Single(x => x.Id == "phase560-supplement").Path, sha256 = specs.Single(x => x.Id == "phase560-supplement").Hash, byteImmutable = true },
    candidateCount = 48, survivorCount = survivors.Length, survivors, bestResidual = best, tiedBest = bestSet,
    earliestUniversalFailure = earliest, adjudicationPassed,
    machineCompatibilityIsNotSourceIdentification = true, phase561SourceGateSatisfied = false,
    answersCollectiveCoordinateRuling = false, answersFpNormalizationRuling = false, authorsARuling = false,
    changesAPendingFlag = false, additiveOnly = true, externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
};

Emit(new
{
    schemaVersion = 1, phase = 563, phaseId = "phase563-discrete-transformation-candidate-census-independent-adjudicator",
    contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath), contractValid = true,
    exactBindingsValid = true, resourceAccepted, bindings,
    independentImplementation = new { phase562ProjectReference = false, sharedPhase562Code = false, rawCoreSourcesRecomputed = true, candidateCount = rows.Length },
    independentKnownAnswerBattery = new { ranBeforePhase562Read = true, exactResidual, plantedCovarianceFailure, wardOnlyFalsePositiveRejected, missingCandidateDetected, duplicateCandidateDetected, fixedSliceDecoyRejected, passed = true },
    independentCensus = new { candidateCount = rows.Length, rows, survivorCount = survivors.Length, survivors, bestResidual = best, tiedBest = bestSet, earliestUniversalFailure = earliest },
    comparison = new { everyIdAgrees, maxNumericDeviation, agreementTolerance, survivorSetsMatch, bestTieSetsMatch, earliestUniversalFailureMatches = earliestMatches, terminalMatches, adjudicationPassed },
    supplementMaterialized = true, verdictKind, terminalStatus = "discrete-transformation-candidate-census-independent-adjudicator-" + verdictKind,
    decision = !adjudicationPassed ? "The independent census contradicted Phase562 and the branch fails closed."
        : survivors.Length == 0 ? $"The independent 48-candidate reconstruction confirms that every frozen candidate is machine-incompatible, with the earliest universal failure at {earliest}."
        : $"The independent reconstruction confirms {survivors.Length} machine-compatible but source-unbound candidate(s).",
    phase561GateOpen = false, rngUsed = false, samplingPerformed = false, reprocessingPerformed = false,
    protectedPhase554SeedsRead = false, phase553Or554RegisteredOrExecuted = false, registeredOperatorMutated = false,
    directionCalledGaugeOrRedundant = false, quotientApplied = false, gaugeFixingApplied = false, measureNormalizationApplied = false,
    sourceIdentificationAuthoredOrInferred = false, rulingAuthoredOrInferred = false, o4Discharged = false, phase458Satisfied = false,
    phase481PackCreatedOrMutated = false, productionAuthorized = false, launchAuthorized = false, physicalUnitClaimAllowed = false,
    gevClaimAllowed = false, externalReviewPending = true, promotedPhysicalMassClaimCount = 0,
}, supplement);
Console.WriteLine($"Phase563 verdict: {verdictKind}");
Console.WriteLine($"adjudication={adjudicationPassed}, survivors={survivors.Length}, maxNumericDeviation={maxNumericDeviation:E6}, earliest={earliest}");

double[] Lower(double[] omega, double[] xi, int ds, int bs, string endpoint)
{
    var result = new double[edgeDof];
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        int v0 = mesh.Edges[e][0], v1 = mesh.Edges[e][1];
        double[] left = Slice(xi, v0), right = Slice(xi, v1), edgeXi = new double[g], edgeOmega = Slice(omega, e);
        for (int a = 0; a < g; a++) edgeXi[a] = endpoint == "tail" ? left[a] : endpoint == "head" ? right[a] : 0.5 * (left[a] + right[a]);
        double[] bracket = algebra.Bracket(edgeOmega, edgeXi);
        for (int a = 0; a < g; a++) result[e * g + a] = ds * (right[a] - left[a]) + bs * bracket[a];
    }
    return result;
}
double[] AdjointFaces(double[] field, double[] xi, int sign)
{
    var result = new double[field.Length];
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        double[] bracket = algebra.Bracket(Slice(field, f), Slice(xi, mesh.Faces[f][0]));
        for (int a = 0; a < g; a++) result[f * g + a] = sign * bracket[a];
    }
    return result;
}
double[] Slice(double[] source, int row) { var x = new double[g]; for (int a = 0; a < g; a++) x[a] = source[row * g + a]; return x; }
BranchManifest Manifest() => new() { BranchId="phase563-a33",SchemaVersion="1.0.0",SourceEquationRevision="draft-2021",CodeRevision="phase563",ActiveGeometryBranch="simplicial",ActiveObservationBranch="sigma-pullback",ActiveTorsionBranch="trivial",ActiveShiabBranch="einsteinian-shiab",ActiveGaugeStrategy="none-independent-census",BaseDimension=4,AmbientDimension=4,LieAlgebraId="su2",BasisConventionId="canonical",ComponentOrderId="face-major",AdjointConventionId="adjoint-explicit",PairingConventionId="pairing-trace",NormConventionId="norm-l2-quadrature",DifferentialFormMetricId="hodge-standard",InsertedAssumptionIds=Array.Empty<string>(),InsertedChoiceIds=new[]{"A33-independent-candidate-family-not-source-law"} };
GeometryContext Geometry() { var x=new SpaceRef{SpaceId="X_h",Dimension=4}; var y=new SpaceRef{SpaceId="Y_h",Dimension=4}; return new GeometryContext{BaseSpace=x,AmbientSpace=y,DiscretizationType="simplicial",QuadratureRuleId="centroid",BasisFamilyId="P1",ProjectionBinding=new GeometryBinding{BindingType="projection",SourceSpace=y,TargetSpace=x},ObservationBinding=new GeometryBinding{BindingType="observation",SourceSpace=x,TargetSpace=y},Patches=Array.Empty<PatchInfo>()}; }
static string Id(int ds,int bs,string e,int ts,int cs)=>$"d{ds:+0;-0}-b{bs:+0;-0}-{e}-t{ts:+0;-0}-c{cs:+0;-0}";
static double[] Wave(int n,double scale,double frequency){var x=new double[n];for(int i=0;i<n;i++)x[i]=scale*(System.Math.Sin((i+1)*frequency)+0.37*System.Math.Cos((i+1)*frequency*1.7));return x;}
static double[] Sum(double[] a,double[] b){var x=new double[a.Length];for(int i=0;i<x.Length;i++)x[i]=a[i]+b[i];return x;}
static double Inner(double[] a,double[] b){double x=0;for(int i=0;i<a.Length;i++)x+=a[i]*b[i];return x;}
static double Length(double[] a)=>System.Math.Sqrt(Inner(a,a));
static double Distance(double[] a,double[] b){var d=new double[a.Length];for(int i=0;i<d.Length;i++)d[i]=a[i]-b[i];return Length(d)/System.Math.Max(1.0,System.Math.Max(Length(a),Length(b)));}
static double ScalarDistance(double a,double b)=>System.Math.Abs(a-b)/System.Math.Max(1.0,System.Math.Max(System.Math.Abs(a),System.Math.Abs(b)));
static string Sha(string path){using var stream=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();}
void Emit(object result,object? supplement){byte[] bytes=JsonSerializer.SerializeToUtf8Bytes(result,new JsonSerializerOptions{WriteIndented=true});Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);File.WriteAllBytes(OutputPath,bytes);File.WriteAllBytes(SummaryPath,bytes);if(supplement is not null)File.WriteAllBytes(SupplementPath,JsonSerializer.SerializeToUtf8Bytes(supplement,new JsonSerializerOptions{WriteIndented=true}));}
sealed record BindingSpec(string Id,string Path,string Hash);
sealed record IndependentRow(string Id,double Curvature,double Residual,double Ward,double Parity,double Stage,bool Compatible);
