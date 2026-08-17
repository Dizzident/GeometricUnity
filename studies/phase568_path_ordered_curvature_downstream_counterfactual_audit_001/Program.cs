using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Phase567;

const string Root = "studies/phase568_path_ordered_curvature_downstream_counterfactual_audit_001";
const string ContractPath = Root + "/preregistration/phase568_path_ordered_curvature_downstream_counterfactual_audit_contract_v3.json";
const string OutputPath = Root + "/output/path_ordered_curvature_downstream_counterfactual_audit.json";
const string SummaryPath = Root + "/output/path_ordered_curvature_downstream_counterfactual_audit_summary.json";
const string SpectraPath = Root + "/output/spectra/path_ordered_curvature_downstream_counterfactual_spectra.json";

using var contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
JsonElement target = contract.GetProperty("registeredTarget");
JsonElement controls = contract.GetProperty("implementationControls");
JsonElement spectral = contract.GetProperty("spectralComparison");
JsonElement resources = contract.GetProperty("resourceRefusal");
JsonElement interpretation = contract.GetProperty("interpretationRules");
JsonElement authority = contract.GetProperty("authorityFirewalls");
string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
var bindings = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindingRows = bindings.Select(x => new { x.Id, x.Path, expectedSha256 = x.Sha256, actualSha256 = File.Exists(x.Path) ? Sha(x.Path) : "missing", valid = File.Exists(x.Path) && Sha(x.Path) == x.Sha256 }).ToArray();
bool exactBindingsValid = bindingRows.All(x => x.valid);
using var v1StopDocument = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "phase568-v1-stop-artifact").Path));
JsonElement v1Stop = v1StopDocument.RootElement;
JsonElement predecessor = contract.GetProperty("predecessorAttemptGovernance");
string[] requiredControlFlags =
[
    "registeredReconstructionRequired", "candidateDifferentialRequired", "candidateTransposeDualityRequired",
    "candidateDirectionalGradientRequired", "candidateQuarticPolynomialityRequired", "originHessianEquivalenceRequired",
];

bool contractValid =
    contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase568-a35-path-ordered-curvature-downstream-counterfactual-audit-v3"
    && contract.GetProperty("supersedesForFutureExecution").EnumerateArray().Select(x => x.GetString()).SequenceEqual(new[]
    {
        "phase568-a35-path-ordered-curvature-downstream-counterfactual-audit-v1",
        "phase568-a35-path-ordered-curvature-downstream-counterfactual-audit-v2",
    })
    && contract.GetProperty("v1AndV2ContractsRemainImmutable").GetBoolean()
    && predecessor.GetProperty("v1TerminalOutputEmitted").ValueKind == JsonValueKind.False
    && predecessor.GetProperty("v1StopArtifactRequired").GetBoolean()
    && predecessor.GetProperty("v1StopArtifactBindingId").GetString() == "phase568-v1-stop-artifact"
    && predecessor.GetProperty("v1ContractBindingId").GetString() == "phase568-v1-contract"
    && predecessor.GetProperty("v2ContractBindingId").GetString() == "phase568-v2-contract"
    && !v1Stop.GetProperty("terminalOutputEmitted").GetBoolean()
    && v1Stop.GetProperty("stopReason").GetString()!.Contains("stopped", StringComparison.Ordinal)
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && contract.GetProperty("candidateSelectedOnlyByPhase567").GetBoolean()
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && interpretation.GetProperty("favorableResultIsWorkbenchEvidenceOnly").GetBoolean()
    && !interpretation.GetProperty("mayIdentifyPhase548ConvergenceCause").GetBoolean()
    && !interpretation.GetProperty("maySourceSelectCandidate").GetBoolean()
    && interpretation.GetProperty("noReductionClosesOnlyThisCandidateAgainstPhase550NegativeCurvaturePattern").GetBoolean()
    && interpretation.GetProperty("phase548AndPhase550RemainFrozen").GetBoolean()
    && authority.EnumerateObject().Select(x => x.Name).Order().SequenceEqual(new[]
    {
        "answersO4", "authorizesGevClaim", "authorizesLaunch", "authorizesPhysicalUnitClaim",
        "authorizesProduction", "authorizesSampling", "createsPhase481Pack", "mutatesRegisteredCurvatureAssembler",
        "opensPhase561", "repairsOrReinterpretsPhase548", "satisfiesPhase458",
    }.Order())
    && authority.EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && requiredControlFlags.All(name => controls.TryGetProperty(name, out JsonElement flag) && flag.ValueKind == JsonValueKind.True)
    && spectral.GetProperty("branchSpecificComparisonFloorsForbidden").GetBoolean()
    && spectral.GetProperty("fullCandidateSpectrumRequiredAtEveryCheckpoint").GetBoolean()
    && spectral.GetProperty("registeredSpectrumRecomputationForbidden").GetBoolean()
    && taxonomy.SequenceEqual([
        "invalid-or-drifted-input", "resource-refusal", "registered-control-reproduction-failed",
        "candidate-differential-or-adjoint-invalid", "origin-equivalence-failed",
        "hessian-or-spectrum-validation-failed", "candidate-removes-audited-negative-inertia",
        "candidate-uniformly-reduces-audited-negative-inertia",
        "candidate-mixed-audited-negative-inertia-response",
        "candidate-does-not-reduce-audited-negative-inertia"]);

using var p567Document = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "phase567-summary").Path));
JsonElement p567 = p567Document.RootElement;
using var p550Document = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "phase550-summary").Path));
JsonElement p550 = p550Document.RootElement;
using var p550SpectraDocument = JsonDocument.Parse(File.ReadAllBytes(bindings.Single(x => x.Id == "phase550-spectra").Path));
JsonElement p550Spectra = p550SpectraDocument.RootElement;

int[] candidateOrder = p567.GetProperty("candidateSpecification").GetProperty("boundaryPositionOrder")
    .EnumerateArray().Select(x => x.GetInt32()).ToArray();
bool candidateDefinitionValid =
    p567.GetProperty("verdictKind").GetString() == contract.GetProperty("phase567Premise").GetProperty("requiredVerdictKind").GetString()
    && p567.GetProperty("phase568EvaluationGateOpen").GetBoolean()
    && p567.GetProperty("candidateSpecification").GetProperty("candidateId").GetString() == contract.GetProperty("phase567Premise").GetProperty("candidateId").GetString()
    && candidateOrder.SequenceEqual(contract.GetProperty("phase567Premise").GetProperty("requiredOrientedBoundaryArrayPermutation").EnumerateArray().Select(x => x.GetInt32()))
    && candidateOrder.Order().SequenceEqual(new[] { 0, 1, 2 });

int extent = target.GetProperty("extent").GetInt32();
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
var member = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2,
    Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5,
    EpsilonMode = "independent-theta",
};
var op = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: extent);
var mass = new CpuMassMatrix(mesh, algebra);
int dimG = algebra.Dimension, n = mesh.EdgeCount * dimG, faceDof = mesh.FaceCount * dimG;
var thetaZero = new double[mesh.VertexCount * dimG];
bool geometryValid = mesh.VertexCount == target.GetProperty("expectedVertexCount").GetInt32()
    && mesh.EdgeCount == target.GetProperty("expectedEdgeCount").GetInt32()
    && mesh.FaceCount == target.GetProperty("expectedFaceCount").GetInt32()
    && n == target.GetProperty("degreesOfFreedom").GetInt32()
    && dimG == target.GetProperty("algebraDimension").GetInt32();

long estimatedSeconds = resources.GetProperty("estimatedAggregateCpuSeconds").GetInt64();
long maximumSeconds = resources.GetProperty("maximumEstimatedAggregateCpuSeconds").GetInt64();
long committedEstimatedBytes = resources.GetProperty("estimatedPeakBytes").GetInt64();
long runtimeEstimatedBytes = checked(2L * n * n * sizeof(double) + 64L * n * sizeof(double));
long maximumBytes = resources.GetProperty("maximumEstimatedPeakBytes").GetInt64();
long runtimeEstimatedSeconds = checked(
    (long)spectral.GetProperty("checkpointOrder").GetArrayLength()
    * resources.GetProperty("runtimeEstimateArithmetic").GetProperty("secondsPerDenseCheckpoint").GetInt64()
    + resources.GetProperty("runtimeEstimateArithmetic").GetProperty("fixedSeconds").GetInt64());
bool runtimeArithmeticValid =
    resources.GetProperty("runtimeEstimateArithmetic").GetProperty("checkpointCount").GetInt32()
        == spectral.GetProperty("checkpointOrder").GetArrayLength()
    && resources.GetProperty("runtimeEstimateArithmetic").GetProperty("computedAggregateCpuSeconds").GetInt64()
        == runtimeEstimatedSeconds;
bool resourceAccepted =
    resources.GetProperty("refuseBeforeAllocation").GetBoolean()
    && resources.GetProperty("oneDenseFormAtATime").GetBoolean()
    && resources.GetProperty("reuseExactBoundPhase550RegisteredSpectra").GetBoolean()
    && runtimeArithmeticValid
    && estimatedSeconds < maximumSeconds
    && runtimeEstimatedSeconds < maximumSeconds
    && committedEstimatedBytes < maximumBytes
    && runtimeEstimatedBytes < maximumBytes;

double[] RegisteredCurvature(double[] omega) => CurvatureAssembler.Assemble(new ConnectionField(mesh, algebra, omega)).Coefficients;
double[] CandidateCurvature(double[] omega) => PathOrderedCurvatureCandidate.Assemble(mesh, algebra, omega);
double[] CandidateLinearize(double[] omega, double[] delta) => PathOrderedCurvatureCandidate.Linearize(mesh, algebra, omega, delta);
double[] CandidateTranspose(double[] omega, double[] faceCovector) => PathOrderedCurvatureCandidate.LinearizeTranspose(mesh, algebra, omega, faceCovector);

double[] Contract(double[] faceField)
{
    double[] u = op.ApplyContractionWithTheta(faceField, thetaZero);
    double[] mu = mass.Apply(new FieldTensor
    {
        Label = "Upsilon", Signature = op.OutputSignature, Coefficients = u, Shape = [mesh.FaceCount, dimG],
    }).Coefficients;
    return op.ApplyContractionWithThetaTranspose(mu, thetaZero);
}

(double Action, double[] Gradient) CandidateValueGradient(double[] omega)
{
    double[] curvature = CandidateCurvature(omega);
    double[] u = op.ApplyContractionWithTheta(curvature, thetaZero);
    double[] mu = mass.Apply(new FieldTensor
    {
        Label = "Upsilon", Signature = op.OutputSignature, Coefficients = u, Shape = [mesh.FaceCount, dimG],
    }).Coefficients;
    return (0.5 * Dot(u, mu), CandidateTranspose(omega, op.ApplyContractionWithThetaTranspose(mu, thetaZero)));
}

Func<double[], double[]> CandidateHessian(double[] basePoint)
{
    double[] w0 = Contract(CandidateCurvature(basePoint));
    double[] zero = new double[n];
    double[] dTw0 = CandidateTranspose(zero, w0);
    return direction =>
    {
        double[] first = CandidateTranspose(basePoint, Contract(CandidateLinearize(basePoint, direction)));
        double[] second = CandidateTranspose(direction, w0);
        var result = new double[n];
        for (int i = 0; i < n; i++) result[i] = first[i] + second[i] - dTw0[i];
        return result;
    };
}

double[] OrderedCurvature(double[] omega, int[] order)
{
    var result = new double[faceDof];
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        double[][] x = OrientedBlocks(omega, f);
        var value = new double[dimG];
        for (int k = 0; k < 3; k++) AddInPlace(value, x[k], 1.0);
        for (int a = 0; a < 3; a++)
            for (int b = a + 1; b < 3; b++) AddInPlace(value, algebra.Bracket(x[order[a]], x[order[b]]), 0.5);
        Array.Copy(value, 0, result, f * dimG, dimG);
    }
    return result;
}

double[] OrderedTranspose(double[] omega, double[] faceCovector, int[] order)
{
    var result = new double[n];
    for (int f = 0; f < mesh.FaceCount; f++)
    {
        int[] edges = mesh.FaceBoundaryEdges[f], signs = mesh.FaceBoundaryOrientations[f];
        var w = Block(faceCovector, f, dimG);
        for (int k = 0; k < 3; k++) AddBlock(result, edges[k], w, signs[k]);
        double[][] x = OrientedBlocks(omega, f);
        for (int a = 0; a < 3; a++)
            for (int b = a + 1; b < 3; b++)
            {
                int i = order[a], j = order[b];
                double[] adIw = AdTranspose(x[i], w), adJw = AdTranspose(x[j], w);
                AddBlock(result, edges[j], adIw, 0.5 * signs[j]);
                AddBlock(result, edges[i], adJw, -0.5 * signs[i]);
            }
    }
    return result;
}

double[][] OrientedBlocks(double[] omega, int face)
{
    int[] edges = mesh.FaceBoundaryEdges[face], signs = mesh.FaceBoundaryOrientations[face];
    return Enumerable.Range(0, 3).Select(k => Block(omega, edges[k], dimG).Select(x => signs[k] * x).ToArray()).ToArray();
}

double[] AdTranspose(double[] x, double[] w)
{
    var result = new double[dimG];
    for (int b = 0; b < dimG; b++)
    {
        var basis = new double[dimG]; basis[b] = 1.0;
        result[b] = Dot(algebra.Bracket(x, basis), w);
    }
    return result;
}

var checkpointOrder = spectral.GetProperty("checkpointOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
var positions = checkpointOrder.Select(id =>
{
    string path = bindings.Single(x => x.Id == "phase548-checkpoint-" + id).Path;
    using var document = JsonDocument.Parse(File.ReadAllBytes(path));
    return (Id: id, Position: document.RootElement.GetProperty("payload").GetProperty("position").EnumerateArray().Select(x => x.GetDouble()).ToArray());
}).ToArray();
bool checkpointShapesValid = positions.All(x => x.Position.Length == n);

double derivativeStep = controls.GetProperty("derivativeStep").GetDouble();
double differentialTolerance = controls.GetProperty("differentialScaledTolerance").GetDouble();
double adjointTolerance = controls.GetProperty("adjointScaledTolerance").GetDouble();
double gradientTolerance = controls.GetProperty("gradientScaledTolerance").GetDouble();
double originTolerance = controls.GetProperty("originScaledTolerance").GetDouble();
double polynomialTolerance = controls.GetProperty("polynomialRelativeTolerance").GetDouble();
int directionCount = controls.GetProperty("directionCount").GetInt32();
double maxRegisteredCurvatureDeviation = 0.0, maxRegisteredActionDeviation = 0.0, maxRegisteredGradientDeviation = 0.0;
double maxDifferentialDeviation = 0.0, maxAdjointDeviation = 0.0, maxGradientDeviation = 0.0, maxPolynomialResidual = 0.0;

var probeStates = new List<double[]> { new double[n], Deterministic(n, 0.05, 0.6180339887498948), positions[0].Position };
foreach (double[] state in probeStates)
{
    double[] localOld = OrderedCurvature(state, [0, 1, 2]);
    maxRegisteredCurvatureDeviation = System.Math.Max(maxRegisteredCurvatureDeviation, VectorScaledError(localOld, RegisteredCurvature(state)));
    (double localAction, double[] localGradient) = OrderedValueGradient(state, [0, 1, 2]);
    var core = op.ComputeJointGradient(state, thetaZero, mass);
    maxRegisteredActionDeviation = System.Math.Max(maxRegisteredActionDeviation, ScaledError(localAction, core.Objective));
    maxRegisteredGradientDeviation = System.Math.Max(maxRegisteredGradientDeviation, VectorScaledError(localGradient, core.GradOmega));
}
bool registeredReproductionPassed = maxRegisteredCurvatureDeviation <= differentialTolerance
    && maxRegisteredActionDeviation <= gradientTolerance && maxRegisteredGradientDeviation <= gradientTolerance;

for (int k = 0; k < directionCount; k++)
{
    double[] state = Deterministic(n, 0.04 + 0.01 * k, 0.414213562373095 + k * 0.071);
    double[] direction = Unit(Deterministic(n, 1.0, 0.2718281828459045 + k * 0.053));
    double[] faceTest = Deterministic(faceDof, 0.2, 0.5772156649015329 + k * 0.047);
    double[] plus = Add(state, direction, derivativeStep), minus = Add(state, direction, -derivativeStep);
    double[] finiteDifference = Scale(Subtract(CandidateCurvature(plus), CandidateCurvature(minus)), 0.5 / derivativeStep);
    double[] analytic = CandidateLinearize(state, direction);
    maxDifferentialDeviation = System.Math.Max(maxDifferentialDeviation, VectorScaledError(analytic, finiteDifference));
    maxAdjointDeviation = System.Math.Max(maxAdjointDeviation, ScaledError(Dot(analytic, faceTest), Dot(direction, CandidateTranspose(state, faceTest))));

    var vg = CandidateValueGradient(state);
    double actionPlus = CandidateValueGradient(plus).Action, actionMinus = CandidateValueGradient(minus).Action;
    maxGradientDeviation = System.Math.Max(maxGradientDeviation, ScaledError((actionPlus - actionMinus) / (2.0 * derivativeStep), Dot(vg.Gradient, direction)));

    double ScaledValue(double t) => CandidateValueGradient(Scale(state, t)).Action;
    double s1 = ScaledValue(1), sm1 = ScaledValue(-1), s2 = ScaledValue(2), s3 = ScaledValue(3);
    (double degree2, double degree3, double degree4) = Homogeneous(s1, sm1, s2);
    double predicted = 9 * degree2 + 27 * degree3 + 81 * degree4;
    maxPolynomialResidual = System.Math.Max(maxPolynomialResidual, ScaledError(predicted, s3));
}
bool candidateDifferentialPassed = maxDifferentialDeviation <= differentialTolerance;
bool candidateAdjointPassed = maxAdjointDeviation <= adjointTolerance;
bool candidateGradientPassed = maxGradientDeviation <= gradientTolerance;
bool candidatePolynomialityPassed = maxPolynomialResidual <= polynomialTolerance;

double maxOriginHessianDeviation = 0.0;
Func<double[], double[]> candidateOriginHessian = CandidateHessian(new double[n]);
for (int k = 0; k < directionCount; k++)
{
    double[] direction = Unit(Deterministic(n, 1.0, 0.1414213562373095 + k * 0.061));
    double[] candidate = candidateOriginHessian(direction);
    double[] registered = op.LinearizeCurvatureTranspose(new double[n], Contract(op.LinearizeCurvature(new double[n], direction)));
    maxOriginHessianDeviation = System.Math.Max(maxOriginHessianDeviation, VectorScaledError(candidate, registered));
}

double[][] nullBasis = BuildClosedNullBasis();
double maxOriginNullResidual = nullBasis.Max(v => Norm(candidateOriginHessian(v)));
bool originEquivalencePassed = maxOriginHessianDeviation <= originTolerance
    && maxOriginNullResidual <= controls.GetProperty("originNullResidualTolerance").GetDouble()
    && nullBasis.Length == target.GetProperty("expectedOriginFlatSectorDimension").GetInt32();

var registeredSummaryRows = p550.GetProperty("spectralCensus").GetProperty("rows").EnumerateArray()
    .Where(x => x.GetProperty("kind").GetString() == "preserved-checkpoint-position")
    .ToDictionary(x => x.GetProperty("id").GetString()!, x => x.Clone());
var registeredSpectrumRows = p550Spectra.GetProperty("basePoints").EnumerateArray()
    .Where(x => x.GetProperty("kind").GetString() == "preserved-checkpoint-position")
    .ToDictionary(x => x.GetProperty("id").GetString()!, x => x.Clone());
var registeredHomogeneousRows = p550.GetProperty("homogeneousDecomposition").GetProperty("rows").EnumerateArray()
    .ToDictionary(x => x.GetProperty("id").GetString()!, x => x.Clone());

var candidateSpectra = new List<object>();
var counterfactualRows = new List<CounterfactualRow>();
bool hessianSpectrumValidationPassed = true;
double symmetryTolerance = spectral.GetProperty("hessianRelativeSymmetryTolerance").GetDouble();
double traceTolerance = spectral.GetProperty("traceRelativeTolerance").GetDouble();
double frobeniusTolerance = spectral.GetProperty("frobeniusRelativeTolerance").GetDouble();
if (contractValid && exactBindingsValid && candidateDefinitionValid && geometryValid && checkpointShapesValid && resourceAccepted
    && registeredReproductionPassed && candidateDifferentialPassed && candidateAdjointPassed && candidateGradientPassed
    && candidatePolynomialityPassed && originEquivalencePassed)
{
    foreach ((string id, double[] position) in positions)
    {
        Func<double[], double[]> hessian = CandidateHessian(position);
        var dense = new double[(long)n * n];
        var column = new double[n];
        for (int j = 0; j < n; j++)
        {
            Array.Clear(column); column[j] = 1.0;
            double[] image = hessian(column);
            for (int i = 0; i < n; i++) dense[(long)i * n + j] = image[i];
        }
        double trace = 0.0, frobeniusSquared = 0.0, maximumAsymmetry = 0.0;
        for (int i = 0; i < n; i++)
        {
            trace += dense[(long)i * n + i];
            for (int j = 0; j < n; j++)
            {
                double value = dense[(long)i * n + j];
                frobeniusSquared += value * value;
                if (j > i) maximumAsymmetry = System.Math.Max(maximumAsymmetry, System.Math.Abs(value - dense[(long)j * n + i]));
            }
        }
        double frobenius = System.Math.Sqrt(frobeniusSquared);
        bool symmetric = maximumAsymmetry <= symmetryTolerance * System.Math.Max(1.0, frobenius);
        for (int i = 0; i < n; i++)
            for (int j = i + 1; j < n; j++)
            {
                double average = 0.5 * (dense[(long)i * n + j] + dense[(long)j * n + i]);
                dense[(long)i * n + j] = average; dense[(long)j * n + i] = average;
            }
        var working = (double[])dense.Clone();
        Tridiagonalize(working, n, out double[] diagonal, out double[] offDiagonal);
        double[] candidateEigenvalues = TridiagonalEigenvalues(diagonal, offDiagonal, n, out bool eigenSolverConverged);
        Array.Sort(candidateEigenvalues);
        double eigenvalueSum = candidateEigenvalues.Sum(), eigenvalueSquareSum = candidateEigenvalues.Sum(x => x * x);
        bool traceOk = System.Math.Abs(eigenvalueSum - trace) <= traceTolerance * System.Math.Max(1.0, System.Math.Abs(trace));
        bool frobeniusOk = System.Math.Abs(eigenvalueSquareSum - frobeniusSquared) <= frobeniusTolerance * System.Math.Max(1.0, frobeniusSquared);
        bool spectrumValidated = symmetric && eigenSolverConverged && traceOk && frobeniusOk && candidateEigenvalues.All(double.IsFinite);
        hessianSpectrumValidationPassed &= spectrumValidated;

        double[] registeredEigenvalues = registeredSpectrumRows[id].GetProperty("eigenvalues").EnumerateArray().Select(x => x.GetDouble()).ToArray();
        double scale = System.Math.Max(registeredEigenvalues.Max(x => System.Math.Abs(x)), candidateEigenvalues.Max(x => System.Math.Abs(x)));
        double sharedFloor = n * 2.220446049250313e-16 * scale;
        int registeredNegative = registeredEigenvalues.Count(x => x < -sharedFloor);
        int candidateNegative = candidateEigenvalues.Count(x => x < -sharedFloor);
        var registeredVg = op.ComputeJointGradient(position, thetaZero, mass);
        var candidateVg = CandidateValueGradient(position);
        (double c2, double c3, double c4) = DecomposeCandidate(position);
        JsonElement oldHomogeneous = registeredHomogeneousRows[id];
        double oldC2 = oldHomogeneous.GetProperty("degree2").GetDouble();
        double oldC3 = oldHomogeneous.GetProperty("degree3").GetDouble();
        double oldC4 = oldHomogeneous.GetProperty("degree4").GetDouble();
        double degree2Deviation = ScaledError(c2, oldC2);
        hessianSpectrumValidationPassed &= degree2Deviation <= polynomialTolerance;
        counterfactualRows.Add(new CounterfactualRow(
            id, registeredVg.Objective, candidateVg.Action, Dot(registeredVg.GradOmega, registeredVg.GradOmega), Dot(candidateVg.Gradient, candidateVg.Gradient),
            Cosine(registeredVg.GradOmega, candidateVg.Gradient), ScaledError(candidateVg.Action, registeredVg.Objective), VectorScaledError(candidateVg.Gradient, registeredVg.GradOmega),
            oldC2, oldC3, oldC4, c2, c3, c4, degree2Deviation, sharedFloor, registeredNegative, candidateNegative,
            registeredEigenvalues[0], candidateEigenvalues[0], registeredEigenvalues[^1], candidateEigenvalues[^1], symmetric, spectrumValidated));
        candidateSpectra.Add(new { id, eigenvalues = candidateEigenvalues });
        dense = []; working = [];
        GC.Collect(); GC.WaitForPendingFinalizers();
        Console.WriteLine($"  {id}: registered negative={registeredNegative}, candidate negative={candidateNegative}");
    }
}

bool allRemoved = counterfactualRows.Count == positions.Length && counterfactualRows.All(x => x.CandidateNegativeInertiaCount == 0);
bool uniformlyReduced = counterfactualRows.Count == positions.Length
    && counterfactualRows.All(x => x.CandidateNegativeInertiaCount <= x.RegisteredNegativeInertiaCount)
    && counterfactualRows.Any(x => x.CandidateNegativeInertiaCount < x.RegisteredNegativeInertiaCount);
bool uniformlyNonDecreasing = counterfactualRows.Count == positions.Length
    && counterfactualRows.All(x => x.CandidateNegativeInertiaCount >= x.RegisteredNegativeInertiaCount);
bool mixedResponse = counterfactualRows.Count == positions.Length && !allRemoved && !uniformlyReduced && !uniformlyNonDecreasing;

string verdict = !contractValid || !exactBindingsValid || !candidateDefinitionValid || !geometryValid || !checkpointShapesValid ? taxonomy[0]
    : !resourceAccepted ? taxonomy[1]
    : !registeredReproductionPassed ? taxonomy[2]
    : !candidateDifferentialPassed || !candidateAdjointPassed || !candidateGradientPassed || !candidatePolynomialityPassed ? taxonomy[3]
    : !originEquivalencePassed ? taxonomy[4]
    : !hessianSpectrumValidationPassed || counterfactualRows.Count != positions.Length ? taxonomy[5]
    : allRemoved ? taxonomy[6]
    : uniformlyReduced ? taxonomy[7]
    : mixedResponse ? taxonomy[8]
    : taxonomy[9];

var result = new
{
    schemaVersion = 1,
    phase = 568,
    phaseId = "phase568-path-ordered-curvature-downstream-counterfactual-audit",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath), contractValid, exactBindingsValid, bindingRows,
    candidateDefinitionValid,
    candidateDefinition = new { source = "phase567-only", orientedBoundaryArrayPermutation = candidateOrder, frozenRegisteredOperatorMutated = false },
    registeredTarget = new { extent, dimensions = 4, degreesOfFreedom = n, mesh.VertexCount, mesh.EdgeCount, mesh.FaceCount, member = "sd2-id0/c0.5", thetaRule = "theta-identically-zero" },
    resourceAccepted,
    resource = new { committedEstimatedAggregateCpuSeconds = estimatedSeconds, runtimeEstimatedAggregateCpuSeconds = runtimeEstimatedSeconds, maximumEstimatedAggregateCpuSeconds = maximumSeconds, committedEstimatedPeakBytes = committedEstimatedBytes, runtimeEstimatedPeakBytes = runtimeEstimatedBytes, maximumEstimatedPeakBytes = maximumBytes, runtimeArithmeticValid, accepted = resourceAccepted, refuseBeforeAllocation = true, oneDenseFormAtATime = true, phase550RegisteredSpectraReused = true },
    implementationControls = new
    {
        registeredReproductionPassed, maxRegisteredCurvatureDeviation, maxRegisteredActionDeviation, maxRegisteredGradientDeviation,
        candidateDifferentialPassed, maxDifferentialDeviation, candidateAdjointPassed, maxAdjointDeviation,
        candidateGradientPassed, maxGradientDeviation, candidatePolynomialityPassed, maxPolynomialResidual,
    },
    originEquivalence = new { passed = originEquivalencePassed, maxHessianMatvecScaledDeviation = maxOriginHessianDeviation, exactClosedNullBasisDimension = nullBasis.Length, maxClosedNullBasisResidual = maxOriginNullResidual, registeredAndCandidateShareLinearCoboundary = true },
    counterfactual = new { checkpointCount = positions.Length, rows = counterfactualRows, allCandidateNegativeInertiaRemoved = allRemoved, uniformlyNonIncreasingWithOneStrictReduction = uniformlyReduced, uniformlyNonDecreasing, mixedResponse, sharedRoundoffFloorUsedForEachPair = true },
    verdictKind = verdict,
    terminalStatus = "path-ordered-curvature-downstream-counterfactual-audit-" + verdict,
    decision = Decision(verdict),
    externalReviewPending = true,
    rngUsed = false, hmcOrSamplingPerformed = false, configurationsRetained = false, registeredSeedTouched = false,
    frozenRegisteredOperatorMutated = false, phase548Or550Reinterpreted = false, candidateSourceSelected = false,
    phase561Opened = false, o4Discharged = false, phase458Satisfied = false, phase481PackCreatedOrMutated = false,
    productionAuthorized = false, launchAuthorized = false, physicalUnitClaimAllowed = false, gevClaimAllowed = false,
    promotedPhysicalMassClaimCount = 0,
};

Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
Directory.CreateDirectory(Path.GetDirectoryName(SpectraPath)!);
byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(result, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllBytes(OutputPath, bytes); File.WriteAllBytes(SummaryPath, bytes);
File.WriteAllBytes(SpectraPath, JsonSerializer.SerializeToUtf8Bytes(new { schemaVersion = 1, phase = 568, rows = candidateSpectra }, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine(result.terminalStatus);

(double Action, double[] Gradient) OrderedValueGradient(double[] omega, int[] order)
{
    double[] curvature = OrderedCurvature(omega, order);
    double[] u = op.ApplyContractionWithTheta(curvature, thetaZero);
    double[] mu = mass.Apply(new FieldTensor { Label = "Upsilon", Signature = op.OutputSignature, Coefficients = u, Shape = [mesh.FaceCount, dimG] }).Coefficients;
    double[] w = op.ApplyContractionWithThetaTranspose(mu, thetaZero);
    return (0.5 * Dot(u, mu), OrderedTranspose(omega, w, order));
}
(double D2, double D3, double D4) DecomposeCandidate(double[] x)
{
    double S(double t) => CandidateValueGradient(Scale(x, t)).Action;
    return Homogeneous(S(1), S(-1), S(2));
}
static (double D2, double D3, double D4) Homogeneous(double s1, double sm1, double s2)
{
    double d3 = 0.5 * (s1 - sm1), average = 0.5 * (s1 + sm1), reduced = s2 - 8 * d3;
    return ((16 * average - reduced) / 12, d3, (reduced - 4 * average) / 12);
}
double[][] BuildClosedNullBasis()
{
    var generators = new List<int[]>();
    for (int v = 0; v < mesh.VertexCount; v++)
    {
        var row = new int[mesh.EdgeCount];
        for (int e = 0; e < mesh.EdgeCount; e++) row[e] = (mesh.Edges[e][1] == v ? 1 : 0) - (mesh.Edges[e][0] == v ? 1 : 0);
        generators.Add(row);
    }
    for (int axis = 0; axis < 4; axis++)
    {
        var row = new int[mesh.EdgeCount];
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var c0 = mesh.GetVertexCoordinates(mesh.Edges[e][0]); var c1 = mesh.GetVertexCoordinates(mesh.Edges[e][1]);
            int d = (int)System.Math.Round(c1[axis] - c0[axis]); int wrapped = ((d % extent) + extent) % extent;
            row[e] = wrapped == extent - 1 ? -1 : wrapped;
        }
        generators.Add(row);
    }
    var basis = new List<double[]>();
    foreach (int[] generator in generators)
        for (int a = 0; a < dimG; a++)
        {
            var candidate = new double[n];
            for (int e = 0; e < mesh.EdgeCount; e++) candidate[e * dimG + a] = generator[e];
            for (int pass = 0; pass < 2; pass++) foreach (double[] old in basis) AddInPlace(candidate, old, -Dot(candidate, old));
            double norm = Norm(candidate); if (norm <= 1e-8) continue;
            for (int i = 0; i < n; i++) candidate[i] /= norm;
            basis.Add(candidate);
        }
    return [.. basis];
}

static string Decision(string verdict) => verdict switch
{
    "candidate-removes-audited-negative-inertia" => "The study-local candidate removes negative inertia at all six preserved positions under a shared roundoff floor. This is workbench evidence for independent counterfactual review only; it does not identify the cause of Phase548's convergence failure or source-select an operator.",
    "candidate-uniformly-reduces-audited-negative-inertia" => "The study-local candidate strictly reduces the negative-inertia count at at least one preserved position and increases it at none. This is scoped workbench evidence for independent review only.",
    "candidate-mixed-audited-negative-inertia-response" => "The study-local candidate has a mixed negative-inertia response across the six preserved positions. Boundary ordering remains unresolved as a downstream explanation.",
    "candidate-does-not-reduce-audited-negative-inertia" => "The study-local candidate reduces negative inertia at none of the six preserved positions under the shared comparison rule. This closes only this candidate as an explanation of the registered negative-curvature pattern.",
    _ => "A prerequisite, implementation-control, resource, or spectral-validation gate failed. No downstream comparison may be cited.",
};

static double[] Deterministic(int length, double scale, double frequency) => Enumerable.Range(0, length).Select(i => scale * (System.Math.Sin((i + 1) * frequency) + 0.31 * System.Math.Cos((i + 1) * (frequency + 0.117)))).ToArray();
static double[] Block(double[] x, int block, int width) { var r = new double[width]; Array.Copy(x, block * width, r, 0, width); return r; }
static void AddBlock(double[] x, int block, double[] value, double scale) { for (int a = 0; a < value.Length; a++) x[block * value.Length + a] += scale * value[a]; }
static void AddInPlace(double[] x, double[] y, double scale) { for (int i = 0; i < x.Length; i++) x[i] += scale * y[i]; }
static double[] Add(double[] x, double[] y, double scale) { var r = new double[x.Length]; for (int i = 0; i < x.Length; i++) r[i] = x[i] + scale * y[i]; return r; }
static double[] Subtract(double[] x, double[] y) { var r = new double[x.Length]; for (int i = 0; i < x.Length; i++) r[i] = x[i] - y[i]; return r; }
static double[] Scale(double[] x, double scale) { var r = new double[x.Length]; for (int i = 0; i < x.Length; i++) r[i] = scale * x[i]; return r; }
static double Dot(double[] x, double[] y) { double s = 0; for (int i = 0; i < x.Length; i++) s += x[i] * y[i]; return s; }
static double Norm(double[] x) => System.Math.Sqrt(Dot(x, x));
static double[] Unit(double[] x) { double n = Norm(x); return x.Select(v => v / n).ToArray(); }
static double Cosine(double[] x, double[] y) => Dot(x, y) / System.Math.Max(double.Epsilon, Norm(x) * Norm(y));
static double ScaledError(double actual, double expected) => System.Math.Abs(actual - expected) / System.Math.Max(1.0, System.Math.Max(System.Math.Abs(actual), System.Math.Abs(expected)));
static double VectorScaledError(double[] actual, double[] expected) => Norm(Subtract(actual, expected)) / System.Math.Max(1.0, System.Math.Max(Norm(actual), Norm(expected)));
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

// Householder reduction and implicit-shift QL are copied study-locally from
// Phase550 so this phase can independently validate each complete spectrum.
static void Tridiagonalize(double[] a, int n, out double[] d, out double[] e)
{
    d = new double[n]; e = new double[n];
    for (int i = n - 1; i >= 1; i--)
    {
        int l = i - 1; double h = 0, scale = 0;
        if (l > 0)
        {
            for (int k = 0; k <= l; k++) scale += System.Math.Abs(a[(long)i * n + k]);
            if (scale == 0) e[i] = a[(long)i * n + l];
            else
            {
                for (int k = 0; k <= l; k++) { a[(long)i * n + k] /= scale; h += a[(long)i * n + k] * a[(long)i * n + k]; }
                double f = a[(long)i * n + l], g = f >= 0 ? -System.Math.Sqrt(h) : System.Math.Sqrt(h);
                e[i] = scale * g; h -= f * g; a[(long)i * n + l] = f - g; f = 0;
                for (int j = 0; j <= l; j++)
                {
                    g = 0; for (int k = 0; k <= j; k++) g += a[(long)j * n + k] * a[(long)i * n + k];
                    for (int k = j + 1; k <= l; k++) g += a[(long)k * n + j] * a[(long)i * n + k];
                    e[j] = g / h; f += e[j] * a[(long)i * n + j];
                }
                double hh = f / (h + h);
                for (int j = 0; j <= l; j++)
                {
                    f = a[(long)i * n + j]; e[j] = g = e[j] - hh * f;
                    for (int k = 0; k <= j; k++) a[(long)j * n + k] -= f * e[k] + g * a[(long)i * n + k];
                }
            }
        }
        else e[i] = a[(long)i * n + l];
        d[i] = h;
    }
    e[0] = 0; for (int i = 0; i < n; i++) d[i] = a[(long)i * n + i];
}
static double[] TridiagonalEigenvalues(double[] d, double[] e, int n, out bool converged)
{
    converged = true; for (int i = 1; i < n; i++) e[i - 1] = e[i]; e[n - 1] = 0;
    double deflation = 2.220446049250313e-16 * (d.Max(x => System.Math.Abs(x)) + 2 * e.Max(x => System.Math.Abs(x)));
    for (int l = 0; l < n; l++)
    {
        int iteration = 0, m;
        do
        {
            for (m = l; m < n - 1; m++) { double dd = System.Math.Abs(d[m]) + System.Math.Abs(d[m + 1]); if (System.Math.Abs(e[m]) <= System.Math.Max(2.220446049250313e-16 * dd, deflation)) break; }
            if (m == l) continue;
            if (iteration++ == 100) { converged = false; e[m] = 0; break; }
            double g = (d[l + 1] - d[l]) / (2 * e[l]), r = Hypotenuse(g, 1);
            g = d[m] - d[l] + e[l] / (g + (g >= 0 ? System.Math.Abs(r) : -System.Math.Abs(r)));
            double s = 1, c = 1, p = 0; int i;
            for (i = m - 1; i >= l; i--)
            {
                double f = s * e[i], b = c * e[i]; e[i + 1] = r = Hypotenuse(f, g);
                if (r == 0) { d[i + 1] -= p; e[m] = 0; break; }
                s = f / r; c = g / r; g = d[i + 1] - p; r = (d[i] - g) * s + 2 * c * b; d[i + 1] = g + (p = s * r); g = c * r - b;
            }
            if (r == 0 && i >= l) continue; d[l] -= p; e[l] = g; e[m] = 0;
        } while (m != l);
    }
    return d;
}
static double Hypotenuse(double a, double b) { double aa = System.Math.Abs(a), bb = System.Math.Abs(b); if (aa > bb) return aa * System.Math.Sqrt(1 + bb / aa * (bb / aa)); return bb == 0 ? 0 : bb * System.Math.Sqrt(1 + aa / bb * (aa / bb)); }

sealed record Binding(string Id, string Path, string Sha256);
sealed record CounterfactualRow(
    string Id, double RegisteredAction, double CandidateAction, double RegisteredGradientNormSquared, double CandidateGradientNormSquared,
    double GradientCosine, double ActionScaledDifference, double GradientScaledDifference,
    double RegisteredDegree2, double RegisteredDegree3, double RegisteredDegree4,
    double CandidateDegree2, double CandidateDegree3, double CandidateDegree4, double Degree2ScaledDeviation,
    double SharedRoundoffFloor, int RegisteredNegativeInertiaCount, int CandidateNegativeInertiaCount,
    double RegisteredSmallestEigenvalue, double CandidateSmallestEigenvalue,
    double RegisteredLargestEigenvalue, double CandidateLargestEigenvalue,
    bool CandidateHessianSymmetric, bool CandidateSpectrumValidated);
