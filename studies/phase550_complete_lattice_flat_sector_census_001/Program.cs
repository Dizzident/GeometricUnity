using System.Security.Cryptography;
using System.Text.Json;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

// Phase550 is a deterministic, zero-sampling census of the second-order form of
// the registered complete-lattice action at the origin, at the six preserved
// Phase548 checkpoint positions, and along one flat ray. It constructs no RNG,
// touches no registered seed, runs no sampler, and interprets no measured null
// direction as gauge volume.

const string Root = "studies/phase550_complete_lattice_flat_sector_census_001";
const string ContractPath = Root + "/preregistration/phase550_complete_lattice_flat_sector_census_contract_v1.json";
const string OutputPath = Root + "/output/complete_lattice_flat_sector_census.json";
const string SummaryPath = Root + "/output/complete_lattice_flat_sector_census_summary.json";
const string SpectraPath = Root + "/output/spectra/complete_lattice_spectra.json";

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
    return new { x.Id, x.Path, x.ExpectedSha256, ActualSha256 = actual, HashMatches = actual == x.ExpectedSha256 };
}).ToArray();
bool exactBindingsValid = bindings.Length == 12 && bindings.All(x => x.HashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
JsonElement target = contract.GetProperty("registeredTarget");
JsonElement armA = contract.GetProperty("armA");
JsonElement armB = contract.GetProperty("armB");
JsonElement armC = contract.GetProperty("armC");
JsonElement armD = contract.GetProperty("armD");
JsonElement armE = contract.GetProperty("armE");
JsonElement armF = contract.GetProperty("armF");
JsonElement armG = contract.GetProperty("armG");
JsonElement armH = contract.GetProperty("armH");
JsonElement resource = contract.GetProperty("resourceRefusal");

bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("contractId").GetString() == "phase550-a30-complete-lattice-flat-sector-census-v1"
    && contract.GetProperty("planSection").GetString() == "COMPLETE_LATTICE_FLAT_SECTOR_PLAN_2026-07-25 A30"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministicZeroSampling").GetBoolean()
    && contract.GetProperty("terminalIsKeyedToCertificationQualityNotOutcome").GetBoolean()
    && contract.GetProperty("betaSemantics").GetProperty("entersExecutedValue").GetBoolean() == false
    && contract.GetProperty("oneSidedUpstreamNumbers").GetProperty("mayBeCitedAsMeasurement").GetBoolean() == false
    && contract.GetProperty("structuralExpectationRecordedNotAssumed").GetProperty("isGated").GetBoolean() == false
    && exactBindingsValid
    && taxonomy.Length == 6
    && taxonomy[0] == "invalid-or-drifted-input"
    && taxonomy[5] == "origin-and-configuration-spectrum-characterized"
    && target.GetProperty("extent").GetInt32() == 3
    && target.GetProperty("degreesOfFreedom").GetInt32() == 3645
    && target.GetProperty("thetaRule").GetString() == "theta-identically-zero"
    && contract.GetProperty("samplingFirewall").EnumerateObject()
        .Where(x => x.Value.ValueKind is JsonValueKind.True or JsonValueKind.False)
        .All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

// -------------------------------------------------- resource, before allocation
double estimatedSeconds = resource.GetProperty("estimatedAggregateCpuSeconds").GetDouble();
double maximumSeconds = resource.GetProperty("maximumEstimatedAggregateCpuSeconds").GetDouble();
long estimatedBytes = resource.GetProperty("estimatedPeakBytes").GetInt64();
long maximumBytes = resource.GetProperty("maximumEstimatedPeakBytes").GetInt64();
bool resourceAccepted = estimatedSeconds <= maximumSeconds && estimatedBytes <= maximumBytes
    && resource.GetProperty("refuseBeforeAllocation").GetBoolean()
    && resource.GetProperty("phase546SamplerCeilingUntouched").GetBoolean()
    && resource.GetProperty("noSamplerRunsUnderThisCeiling").GetBoolean();

if (!contractValid || !resourceAccepted)
{
    string earlyVerdict = !contractValid ? taxonomy[0] : taxonomy[1];
    WriteResult(new
    {
        schemaVersion = 1, phase = 550, phaseId = "phase550-complete-lattice-flat-sector-census",
        contractId = contract.GetProperty("contractId").GetString(), contractSha256 = Sha(ContractPath),
        contractValid, exactBindingsValid, resourceAccepted, bindings,
        verdictKind = earlyVerdict,
        terminalStatus = "complete-lattice-flat-sector-census-" + earlyVerdict,
        decision = "The phase refused before allocating the dense second-order form.",
        rngUsed = false, hmcOrSamplingPerformed = false, configurationsRetained = false,
        nullSpaceInterpretedAsGaugeVolume = false, quotientApplied = false,
        phase535ExecutedReopenedOrMutated = false, phase481PackCreatedOrMutated = false,
        productionDefaultSelected = false, phase458G3Satisfied = false, phase458G4Satisfied = false,
        phase458G5Satisfied = false, o4Discharged = false, sourceContractApplicationAllowed = false,
        physicalUnitClaimAllowed = false, gevClaimAllowed = false, productionAuthorized = false,
        launchAuthorized = false, externalReviewPending = true, allDownstreamAuthority = false,
        promotedPhysicalMassClaimCount = 0,
    });
    Console.WriteLine($"Phase550 verdict: {earlyVerdict}");
    return;
}

// ------------------------------------------------------------ registered target
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int extent = target.GetProperty("extent").GetInt32();
var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(extent, latticeCanonical: true);
var member = new EinsteinianShiabFamilyMember
{
    Phi1 = InvariantElementSpec.Sd2,
    Phi2 = InvariantElementSpec.Id0,
    EinsteinCoefficient = 0.5,
    EpsilonMode = "independent-theta",
};
var op = new EinsteinianShiabOperator(mesh, algebra, member, latticePeriod: extent);
var massMatrix = new CpuMassMatrix(mesh, algebra);
int dimG = algebra.Dimension;
int vertexCount = mesh.VertexCount, edgeCount = mesh.EdgeCount, faceCount = mesh.FaceCount;
int n = edgeCount * dimG;
var thetaZero = new double[vertexCount * dimG];
var zeroOmega = new double[n];
var outputSignature = op.OutputSignature;
int eulerCharacteristic = vertexCount - edgeCount + faceCount - mesh.VolumeCount + mesh.CellCount;

bool geometryMatchesContract = vertexCount == target.GetProperty("expectedVertexCount").GetInt32()
    && edgeCount == target.GetProperty("expectedEdgeCount").GetInt32()
    && faceCount == target.GetProperty("expectedFaceCount").GetInt32()
    && eulerCharacteristic == target.GetProperty("expectedEulerCharacteristic").GetInt32()
    && n == target.GetProperty("degreesOfFreedom").GetInt32();

double[] Contract(double[] faceField)
{
    double[] c = op.ApplyContractionWithTheta(faceField, thetaZero);
    double[] m = massMatrix.Apply(new FieldTensor
    {
        Label = "Upsilon", Signature = outputSignature, Coefficients = c, Shape = [faceCount, dimG],
    }).Coefficients;
    return op.ApplyContractionWithThetaTranspose(m, thetaZero);
}
double[] Curvature(double[] omega) =>
    CurvatureAssembler.Assemble(new ConnectionField(mesh, algebra, omega)).Coefficients;
double Value(double[] omega) => op.ComputeJointGradient(omega, thetaZero, massMatrix).Objective;
double[] Gradient(double[] omega) => op.ComputeJointGradient(omega, thetaZero, massMatrix).GradOmega;

// ------------------------------------------------- arm A: structural prechecks
// A1. d.d = 0 on the actual incidence and orientations, in exact integer arithmetic.
long ddNonZeroCount = 0;
var faceEdges = mesh.FaceBoundaryEdges;
var faceSigns = mesh.FaceBoundaryOrientations;
for (int v = 0; v < vertexCount; v++)
{
    for (int f = 0; f < faceCount; f++)
    {
        int sum = 0;
        int[] be = faceEdges[f];
        int[] bo = faceSigns[f];
        for (int i = 0; i < be.Length; i++)
        {
            int[] ends = mesh.Edges[be[i]];
            sum += bo[i] * ((ends[1] == v ? 1 : 0) - (ends[0] == v ? 1 : 0));
        }
        if (sum != 0) ddNonZeroCount++;
    }
}
bool ddIdentityExact = ddNonZeroCount == armA.GetProperty("ddIdentity").GetProperty("requiredNonZeroCount").GetInt64();

// A2. Exact-integer rank of the scalar coboundary, two-sided.
int[][] scalarExactGenerators = Enumerable.Range(0, vertexCount).Select(v =>
{
    var g = new int[edgeCount];
    for (int e = 0; e < edgeCount; e++)
        g[e] = (mesh.Edges[e][1] == v ? 1 : 0) - (mesh.Edges[e][0] == v ? 1 : 0);
    return g;
}).ToArray();
int[][] scalarWindingGenerators = Enumerable.Range(0, 4).Select(axis =>
{
    var g = new int[edgeCount];
    for (int e = 0; e < edgeCount; e++)
    {
        var c0 = mesh.GetVertexCoordinates(mesh.Edges[e][0]);
        var c1 = mesh.GetVertexCoordinates(mesh.Edges[e][1]);
        int d = (int)System.Math.Round(c1[axis] - c0[axis]);
        int wrapped = ((d % extent) + extent) % extent;
        g[e] = wrapped == extent - 1 ? -1 : wrapped;
    }
    return g;
}).ToArray();
int[][] scalarGenerators = [.. scalarExactGenerators, .. scalarWindingGenerators];
int generatorsNotClosed = scalarGenerators.Count(g => !IsClosed(g));
int generatorsWithZeroNorm = scalarGenerators.Count(g => g.All(x => x == 0));
bool IsClosed(int[] scalarForm)
{
    for (int f = 0; f < faceCount; f++)
    {
        int sum = 0;
        int[] be = faceEdges[f];
        int[] bo = faceSigns[f];
        for (int i = 0; i < be.Length; i++) sum += bo[i] * scalarForm[be[i]];
        if (sum != 0) return false;
    }
    return true;
}

long[] rankPrimes = armA.GetProperty("incidenceRank").GetProperty("primes")
    .EnumerateArray().Select(x => x.GetInt64()).ToArray();
var coboundaryRows = Enumerable.Range(0, faceCount).Select(f =>
{
    var row = new int[edgeCount];
    int[] be = faceEdges[f];
    int[] bo = faceSigns[f];
    for (int i = 0; i < be.Length; i++) row[be[i]] += bo[i];
    return row;
}).ToArray();
var rankRows = rankPrimes.Select(p => new
{
    prime = p,
    coboundaryRank = ModularRank(coboundaryRows, edgeCount, p),
    generatorRank = ModularRank(scalarGenerators, edgeCount, p),
}).ToArray();
int scalarCoboundaryRank = rankRows[0].coboundaryRank;
int scalarGeneratorRank = rankRows[0].generatorRank;
bool ranksAgreeAcrossPrimes = rankRows.All(r => r.coboundaryRank == scalarCoboundaryRank && r.generatorRank == scalarGeneratorRank);
int scalarNullityUpperBound = edgeCount - scalarCoboundaryRank;
bool scalarNullityExactlyDetermined = ranksAgreeAcrossPrimes
    && generatorsNotClosed == 0 && generatorsWithZeroNorm == 0
    && scalarGeneratorRank == scalarNullityUpperBound;
int scalarNullityExact = scalarNullityExactlyDetermined ? scalarGeneratorRank : -1;
int flatSectorLowerBoundExact = scalarNullityExactlyDetermined ? scalarNullityExact * dimG : -1;

// A3. F(e_i) = d e_i on single-edge basis directions, bit for bit.
int singleEdgeSubsetSize = armA.GetProperty("singleEdgeCurvature").GetProperty("subsetSize").GetInt32();
double singleEdgeMaxDeviation = 0.0;
for (int k = 0; k < singleEdgeSubsetSize; k++)
{
    int index = (int)((long)k * n / singleEdgeSubsetSize);
    var basis = new double[n];
    basis[index] = 1.0;
    double[] assembled = Curvature(basis);
    double[] linear = op.LinearizeCurvature(zeroOmega, basis);
    for (int i = 0; i < assembled.Length; i++)
        singleEdgeMaxDeviation = System.Math.Max(singleEdgeMaxDeviation, System.Math.Abs(assembled[i] - linear[i]));
}
bool singleEdgeCurvatureExact = singleEdgeMaxDeviation
    <= armA.GetProperty("singleEdgeCurvature").GetProperty("requiredMaximumAbsoluteDeviation").GetDouble();

// -------------------------------------- exact second-order form of the operator
Func<double[], double[]> HessianAction(double[] basePoint)
{
    double[] w0 = Contract(Curvature(basePoint));
    double[] dTw0 = op.LinearizeCurvatureTranspose(zeroOmega, w0);
    return u =>
    {
        double[] first = op.LinearizeCurvatureTranspose(basePoint, Contract(op.LinearizeCurvature(basePoint, u)));
        double[] second = op.LinearizeCurvatureTranspose(u, w0);
        var result = new double[n];
        for (int i = 0; i < n; i++) result[i] = first[i] + second[i] - dTw0[i];
        return result;
    };
}

// A4/A5. Cross-check the exact primitive route against four-point and six-point
// antisymmetric gradient extraction, which also tests the cubic premise.
JsonElement agreementSpec = armA.GetProperty("extractionAgreement");
double fourPointStep = agreementSpec.GetProperty("fourPointStep").GetDouble();
double[] sixPointSteps = agreementSpec.GetProperty("sixPointSteps").EnumerateArray().Select(x => x.GetDouble()).ToArray();
var agreementBase = new double[n];
for (int i = 0; i < n; i++) agreementBase[i] = 0.05 * System.Math.Sin((i + 1) * 0.6180339887498948);
Func<double[], double[]> agreementHessian = HessianAction(agreementBase);
double maxExactVersusFourPoint = 0.0;
double maxCubicPolynomialityResidual = 0.0;
var extractionRows = new List<object>();
for (int k = 0; k < agreementSpec.GetProperty("directionCount").GetInt32(); k++)
{
    var direction = new double[n];
    for (int i = 0; i < n; i++) direction[i] = System.Math.Sin((i + 1) * (k + 1) * 0.6180339887498948);
    Normalize(direction);
    double[] exact = agreementHessian(direction);
    double[] antisymmetric1 = AntisymmetricGradient(agreementBase, direction, fourPointStep);
    double[] antisymmetric2 = AntisymmetricGradient(agreementBase, direction, 2.0 * fourPointStep);
    var fourPoint = new double[n];
    for (int i = 0; i < n; i++)
        fourPoint[i] = (8.0 * antisymmetric1[i] - antisymmetric2[i]) / (6.0 * fourPointStep);
    double exactVersusFourPoint = RelativeVectorDeviation(exact, fourPoint);

    // Cubic premise: the antisymmetric part is exactly t*g1 + t^3*g3. Determine
    // (g1, g3) from the first two frozen steps and PREDICT the third.
    double s0 = sixPointSteps[0], s1 = sixPointSteps[1], s2 = sixPointSteps[2];
    double[] a0 = AntisymmetricGradient(agreementBase, direction, s0);
    double[] a1 = AntisymmetricGradient(agreementBase, direction, s1);
    double[] a2 = AntisymmetricGradient(agreementBase, direction, s2);
    double determinant = s0 * s1 * s1 * s1 - s1 * s0 * s0 * s0;
    var predicted = new double[n];
    for (int i = 0; i < n; i++)
    {
        double g1 = (a0[i] * s1 * s1 * s1 - a1[i] * s0 * s0 * s0) / determinant;
        double g3 = (s0 * a1[i] - s1 * a0[i]) / determinant;
        predicted[i] = s2 * g1 + s2 * s2 * s2 * g3;
    }
    double cubicResidual = RelativeVectorDeviation(predicted, a2);
    maxExactVersusFourPoint = System.Math.Max(maxExactVersusFourPoint, exactVersusFourPoint);
    maxCubicPolynomialityResidual = System.Math.Max(maxCubicPolynomialityResidual, cubicResidual);
    extractionRows.Add(new { directionIndex = k, exactVersusFourPointRelativeDeviation = exactVersusFourPoint, cubicPolynomialityRelativeResidual = cubicResidual });
}
double[] AntisymmetricGradient(double[] basePoint, double[] direction, double step)
{
    var plus = new double[n];
    var minus = new double[n];
    for (int i = 0; i < n; i++)
    {
        plus[i] = basePoint[i] + step * direction[i];
        minus[i] = basePoint[i] - step * direction[i];
    }
    double[] gp = Gradient(plus);
    double[] gm = Gradient(minus);
    var result = new double[n];
    for (int i = 0; i < n; i++) result[i] = 0.5 * (gp[i] - gm[i]);
    return result;
}
bool extractionAgreementPassed =
    maxExactVersusFourPoint <= agreementSpec.GetProperty("exactVersusFourPointRelativeTolerance").GetDouble()
    && maxCubicPolynomialityResidual <= agreementSpec.GetProperty("cubicPolynomialityResidualTolerance").GetDouble();

// ------------------------------------------- orthonormalized measured null basis
double orthonormalizationRejection = armC.GetProperty("lowerBound").GetProperty("orthonormalizationRejectionThreshold").GetDouble();
var nullBasis = new List<double[]>();
foreach (int[] generator in scalarGenerators)
{
    for (int a = 0; a < dimG; a++)
    {
        var candidate = new double[n];
        for (int e = 0; e < edgeCount; e++) candidate[e * dimG + a] = generator[e];
        for (int pass = 0; pass < 2; pass++)
            foreach (double[] existing in nullBasis)
            {
                double projection = Dot(candidate, existing);
                for (int i = 0; i < n; i++) candidate[i] -= projection * existing[i];
            }
        double norm = System.Math.Sqrt(Dot(candidate, candidate));
        if (norm <= orthonormalizationRejection) continue;
        for (int i = 0; i < n; i++) candidate[i] /= norm;
        nullBasis.Add(candidate);
    }
}
int measuredNullBasisDimension = nullBasis.Count;
double[][] nullBasisMatrix = [.. nullBasis];

// -------------------------------------------------------------- base point menu
var checkpointOrder = armE.GetProperty("chainOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
var basePoints = new List<(string Id, string Kind, double[] Position)> { ("origin", "origin", new double[n]) };
foreach (string chainId in checkpointOrder)
{
    string path = $"studies/phase548_bounded_complete_lattice_pilot_execution_001/output/checkpoints/{chainId}.json";
    using var checkpoint = JsonDocument.Parse(File.ReadAllBytes(path));
    double[] position = checkpoint.RootElement.GetProperty("payload").GetProperty("position")
        .EnumerateArray().Select(x => x.GetDouble()).ToArray();
    basePoints.Add((chainId, "preserved-checkpoint-position", position));
}

// ------------------------------------------------------- arm B: exact flatness
double[] BuildRow(JsonElement row)
{
    var vector = new double[n];
    int[] vertices = row.GetProperty("vertices").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    int[] latticeAxes = row.GetProperty("latticeAxes").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    int[] algebraAxes = row.GetProperty("algebraAxes").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    int[] weights = row.GetProperty("weights").EnumerateArray().Select(x => x.GetInt32()).ToArray();
    int component = 0;
    foreach (int vertex in vertices)
    {
        int[] generator = scalarExactGenerators[vertex];
        int axis = algebraAxes[component];
        int weight = weights[component];
        for (int e = 0; e < edgeCount; e++) vector[e * dimG + axis] += weight * generator[e];
        component++;
    }
    foreach (int latticeAxis in latticeAxes)
    {
        int[] generator = scalarWindingGenerators[latticeAxis];
        int axis = algebraAxes[component];
        int weight = weights[component];
        for (int e = 0; e < edgeCount; e++) vector[e * dimG + axis] += weight * generator[e];
        component++;
    }
    return vector;
}
double[] flatnessLadder = armB.GetProperty("ladder").EnumerateArray().Select(x => x.GetDouble()).ToArray();
double quarticTolerance = armB.GetProperty("quarticScalingRelativeTolerance").GetDouble();
var flatnessRows = new List<object>();
bool flatnessConstructionValid = true;
bool negativeControlsValid = true;
bool exactFlatSectorObserved = true;
double[]? flatRayDirection = null;
foreach (JsonElement row in armB.GetProperty("rows").EnumerateArray())
{
    string id = row.GetProperty("id").GetString()!;
    string role = row.GetProperty("role").GetString()!;
    bool declaredSelfBracketZero = row.GetProperty("declaredSelfBracketZero").GetBoolean();
    double[] vector = BuildRow(row);
    double normSquared = Dot(vector, vector);
    double[] curvature = Curvature(vector);
    double[] linear = op.LinearizeCurvature(zeroOmega, vector);
    double maximumCoboundary = 0.0, maximumSelfBracket = 0.0;
    for (int i = 0; i < curvature.Length; i++)
    {
        maximumCoboundary = System.Math.Max(maximumCoboundary, System.Math.Abs(linear[i]));
        maximumSelfBracket = System.Math.Max(maximumSelfBracket, System.Math.Abs(curvature[i] - linear[i]));
    }
    bool closedExactly = maximumCoboundary == 0.0;
    bool selfBracketZero = maximumSelfBracket == 0.0;
    var ladderValues = new List<double>();
    foreach (double t in flatnessLadder)
    {
        var scaled = new double[n];
        for (int i = 0; i < n; i++) scaled[i] = t * vector[i];
        ladderValues.Add(Value(scaled));
    }
    bool constructionValid = normSquared > 0.0 && closedExactly && selfBracketZero == declaredSelfBracketZero;
    bool measuredExactlyFlat = ladderValues.All(x => x == 0.0);
    bool controlStrictlyPositive = ladderValues.All(x => x > 0.0);
    double worstQuarticDeviation = 0.0;
    for (int i = 0; i + 1 < ladderValues.Count; i++)
    {
        double expectedRatio = System.Math.Pow(flatnessLadder[i + 1] / flatnessLadder[i], 4);
        double observedRatio = ladderValues[i] == 0.0 ? double.NaN : ladderValues[i + 1] / ladderValues[i];
        double deviation = double.IsFinite(observedRatio)
            ? System.Math.Abs(observedRatio - expectedRatio) / expectedRatio
            : double.PositiveInfinity;
        worstQuarticDeviation = System.Math.Max(worstQuarticDeviation, deviation);
    }
    bool quarticScalingHolds = worstQuarticDeviation <= quarticTolerance;
    flatnessConstructionValid &= constructionValid;
    if (role == "negative-control") negativeControlsValid &= controlStrictlyPositive && quarticScalingHolds;
    else
    {
        exactFlatSectorObserved &= measuredExactlyFlat;
        if (id == armG.GetProperty("rayRow").GetString() && flatRayDirection is null)
        {
            flatRayDirection = (double[])vector.Clone();
            Normalize(flatRayDirection);
        }
    }
    flatnessRows.Add(new
    {
        id, role, declaredSelfBracketZero, normSquared, closedExactly, selfBracketZero,
        maximumCoboundaryMagnitude = maximumCoboundary, maximumSelfBracketMagnitude = maximumSelfBracket,
        constructionValid, ladder = flatnessLadder, ladderValues,
        measuredExactlyFlat, controlStrictlyPositive,
        worstQuarticScalingRelativeDeviation = Reportable(worstQuarticDeviation), quarticScalingHolds,
    });
}

// ---------------------------------------------------- arm G ray point insertion
double[] rayLadder = armG.GetProperty("ladder").EnumerateArray().Select(x => x.GetDouble()).ToArray();
if (flatRayDirection is not null)
    foreach (double t in rayLadder)
    {
        var position = new double[n];
        for (int i = 0; i < n; i++) position[i] = t * flatRayDirection[i];
        basePoints.Add(($"flat-ray-t{t:0.###}", "flat-ray-point", position));
    }

// -------------------------------------- dense census over the frozen base points
double[] shiftLadder = armC.GetProperty("upperBound").GetProperty("shiftLadder")
    .EnumerateArray().Select(x => x.GetDouble()).ToArray();
int plateauRungCount = armC.GetProperty("upperBound").GetProperty("plateauRungCount").GetInt32();
double nullResidualTolerance = armC.GetProperty("lowerBound").GetProperty("measuredResidualRelativeTolerance").GetDouble();
double traceTolerance = armD.GetProperty("spectralConsistency").GetProperty("traceRelativeTolerance").GetDouble();
double frobeniusTolerance = armD.GetProperty("spectralConsistency").GetProperty("frobeniusRelativeTolerance").GetDouble();
int inertiaCountDifferenceAllowance = armD.GetProperty("spectralConsistency").GetProperty("inertiaVersusEigenvalueCountMaximumDifference").GetInt32();
double residualBoundTolerance = armD.GetProperty("residualBoundRelativeTolerance").GetDouble();
double symmetryTolerance = armA.GetProperty("symmetry").GetProperty("relativeFrobeniusTolerance").GetDouble();
int lanczosSteps = armD.GetProperty("largestEigenvalue").GetProperty("steps").GetInt32();

var dense = new double[(long)n * n];
var working = new double[(long)n * n];
var censusRows = new List<object>();
var rayCensusRows = new List<object>();
var spectra = new List<object>();
bool symmetryPassed = true, spectralConsistencyPassed = true, residualBoundsPassed = true;
bool inertiaLaddersConsistent = true;
bool originPlateauPresent = false, originBoundsConsistent = false, originNullResidualPassed = false;
double originLargestEigenvalue = double.NaN;
int originNullityUpperBound = -1;
foreach ((string id, string kind, double[] position) in basePoints)
{
    Func<double[], double[]> action = HessianAction(position);
    var column = new double[n];
    for (int j = 0; j < n; j++)
    {
        Array.Clear(column);
        column[j] = 1.0;
        double[] image = action(column);
        for (int i = 0; i < n; i++) dense[(long)i * n + j] = image[i];
    }
    double frobeniusSquared = 0.0, trace = 0.0, maximumAsymmetry = 0.0;
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
    symmetryPassed &= symmetric;
    // Symmetrize before factorization so the tridiagonal route sees an exactly
    // symmetric matrix; the asymmetry itself is reported above, not hidden.
    for (int i = 0; i < n; i++)
        for (int j = i + 1; j < n; j++)
        {
            double average = 0.5 * (dense[(long)i * n + j] + dense[(long)j * n + i]);
            dense[(long)i * n + j] = average;
            dense[(long)j * n + i] = average;
        }

    Array.Copy(dense, working, dense.LongLength);
    Tridiagonalize(working, n, out double[] diagonal, out double[] offDiagonal);
    var sturmDiagonal = (double[])diagonal.Clone();
    var sturmOffDiagonal = (double[])offDiagonal.Clone();
    double[] eigenvalues = TridiagonalEigenvalues(diagonal, offDiagonal, n, out bool eigenSolverConverged);
    Array.Sort(eigenvalues);
    spectralConsistencyPassed &= eigenSolverConverged;

    double eigenvalueSum = eigenvalues.Sum();
    double eigenvalueSquareSum = eigenvalues.Sum(x => x * x);
    bool traceOk = System.Math.Abs(eigenvalueSum - trace) <= traceTolerance * System.Math.Max(1.0, System.Math.Abs(trace));
    bool frobeniusOk = System.Math.Abs(eigenvalueSquareSum - frobeniusSquared) <= frobeniusTolerance * System.Math.Max(1.0, frobeniusSquared);
    spectralConsistencyPassed &= traceOk && frobeniusOk;

    double largest = eigenvalues[n - 1];
    double roundoffFloor = n * 2.220446049250313e-16 * System.Math.Max(System.Math.Abs(largest), System.Math.Abs(eigenvalues[0]));
    var inertia = shiftLadder.Select(shift => new
    {
        shift,
        countBelowShift = SturmCount(sturmDiagonal, sturmOffDiagonal, n, shift),
        countBelowNegativeShift = SturmCount(sturmDiagonal, sturmOffDiagonal, n, -shift),
        eigenvalueCountBelowShift = eigenvalues.Count(x => x < shift),
        thresholdConditional = true,
        aboveRoundoffFloor = shift > roundoffFloor,
    }).ToArray();
    bool inertiaMonotone = inertia.Zip(inertia.Skip(1)).All(pair => pair.Second.countBelowShift >= pair.First.countBelowShift);
    int worstInertiaCountDifference = inertia.Max(x => System.Math.Abs(x.countBelowShift - x.eigenvalueCountBelowShift));
    bool inertiaMatchesEigenvalueCounts = worstInertiaCountDifference <= inertiaCountDifferenceAllowance;
    bool plateauPresent = inertia.Take(plateauRungCount).Select(x => x.countBelowShift).Distinct().Count() == 1;
    int nullityUpperBound = inertia[0].countBelowShift;
    int negativeInertia = SturmCount(sturmDiagonal, sturmOffDiagonal, n, -roundoffFloor);
    inertiaLaddersConsistent &= inertiaMonotone && inertiaMatchesEigenvalueCounts;

    // Arm D: Lanczos Ritz pairs with a-posteriori residual certification.
    (double Rayleigh, double Residual) largestBound = LanczosExtreme(dense, n, lanczosSteps, null, false);
    double[][]? deflation = kind == "origin" ? nullBasisMatrix : null;
    (double Rayleigh, double Residual) smallestBound = LanczosExtreme(dense, n, lanczosSteps, deflation, true);
    double tridiagonalSmallestReference = kind == "origin" && measuredNullBasisDimension < n
        ? eigenvalues[measuredNullBasisDimension]
        : eigenvalues[0];
    double boundSlack = 1e-08 * System.Math.Max(1.0, System.Math.Abs(largest));
    bool largestBracketsAnEigenvalue = BracketsAnEigenvalue(eigenvalues, largestBound.Rayleigh, largestBound.Residual + boundSlack);
    bool smallestBracketsAnEigenvalue = BracketsAnEigenvalue(eigenvalues, smallestBound.Rayleigh, smallestBound.Residual + boundSlack);
    double largestDistanceToReference = System.Math.Abs(largest - largestBound.Rayleigh);
    double smallestDistanceToReference = System.Math.Abs(tridiagonalSmallestReference - smallestBound.Rayleigh);
    bool boundsCertified = double.IsFinite(largestBound.Residual) && double.IsFinite(smallestBound.Residual)
        && largestBound.Residual <= residualBoundTolerance * System.Math.Max(1.0, System.Math.Abs(largestBound.Rayleigh))
        && largestBracketsAnEigenvalue && smallestBracketsAnEigenvalue;
    residualBoundsPassed &= boundsCertified;

    // Measured null-basis residuals (exact-integer lower bound at the origin only).
    double worstNullResidual = 0.0;
    foreach (double[] basisVector in nullBasisMatrix)
    {
        double[] image = action(basisVector);
        worstNullResidual = System.Math.Max(worstNullResidual, System.Math.Sqrt(Dot(image, image)));
    }
    double relativeNullResidual = worstNullResidual / System.Math.Max(1.0, System.Math.Abs(largest));

    // Flat-block restriction P^T H P and its spectrum.
    double[] flatBlockEigenvalues = RestrictedSpectrum(dense, n, nullBasisMatrix);
    double flatBlockLogDeterminant = flatBlockEigenvalues.All(x => x > 0.0)
        ? flatBlockEigenvalues.Sum(x => System.Math.Log(x))
        : double.NaN;
    double fullLogDeterminantAboveFloor = eigenvalues.Where(x => x > roundoffFloor).Sum(x => System.Math.Log(x));

    if (kind == "origin")
    {
        originPlateauPresent = plateauPresent;
        originNullityUpperBound = nullityUpperBound;
        originBoundsConsistent = flatSectorLowerBoundExact >= 0 && nullityUpperBound >= flatSectorLowerBoundExact;
        originNullResidualPassed = relativeNullResidual <= nullResidualTolerance;
        originLargestEigenvalue = largest;
    }

    spectra.Add(new { id, kind, eigenvalues, flatBlockEigenvalues });
    var censusRow = new
    {
        id, kind,
        positionNormSquared = Dot(position, position),
        trace, frobeniusNorm = frobenius,
        maximumAsymmetry, symmetric,
        eigenSolverConverged,
        eigenvalueSum, eigenvalueSquareSum, traceOk, frobeniusOk,
        largestEigenvalue = largest,
        smallestEigenvalue = eigenvalues[0],
        roundoffFloor,
        smallestFortyEigenvalues = eigenvalues.Take(40).ToArray(),
        largestEightEigenvalues = eigenvalues.Skip(n - 8).ToArray(),
        inertiaLadder = inertia,
        inertiaMonotone, inertiaMatchesEigenvalueCounts, worstInertiaCountDifference, plateauPresent,
        nullityUpperBoundThresholdConditional = nullityUpperBound,
        negativeInertiaAtRoundoffFloor = negativeInertia,
        largestEigenvalueInterval = new
        {
            rayleigh = largestBound.Rayleigh, residualBound = Reportable(largestBound.Residual),
            lower = largestBound.Rayleigh - largestBound.Residual, upper = largestBound.Rayleigh + largestBound.Residual,
            tridiagonalReference = largest, bracketsAnEigenvalue = largestBracketsAnEigenvalue,
            distanceToReference = largestDistanceToReference, lanczosSteps,
        },
        smallestEigenvalueInterval = new
        {
            deflatedAgainstMeasuredNullBasis = deflation is not null,
            rayleigh = smallestBound.Rayleigh, residualBound = Reportable(smallestBound.Residual),
            lower = smallestBound.Rayleigh - smallestBound.Residual, upper = smallestBound.Rayleigh + smallestBound.Residual,
            tridiagonalReference = tridiagonalSmallestReference, bracketsAnEigenvalue = smallestBracketsAnEigenvalue,
            distanceToReference = smallestDistanceToReference, lanczosSteps,
        },
        boundsCertified,
        worstMeasuredNullBasisResidual = worstNullResidual,
        relativeMeasuredNullBasisResidual = relativeNullResidual,
        flatBlockDimension = flatBlockEigenvalues.Length,
        flatBlockSmallestEigenvalue = flatBlockEigenvalues.Length == 0 ? (double?)null : flatBlockEigenvalues[0],
        flatBlockLargestEigenvalue = flatBlockEigenvalues.Length == 0 ? (double?)null : flatBlockEigenvalues[^1],
        flatBlockLogDeterminant = Reportable(flatBlockLogDeterminant),
        fullLogDeterminantAboveRoundoffFloor = Reportable(fullLogDeterminantAboveFloor),
        modelBasedNotCertified = true,
    };
    censusRows.Add(censusRow);
    if (kind == "flat-ray-point") rayCensusRows.Add(censusRow);
    Console.WriteLine($"  base point {id}: largest={largest:E6}, nullityUpper={nullityUpperBound}, plateau={plateauPresent}");
}

// ----------------------------------- arm F: exact homogeneous decomposition
JsonElement consistencyProbe = armF.GetProperty("consistencyProbe");
double probeT = consistencyProbe.GetProperty("t").GetDouble();
double probeTolerance = consistencyProbe.GetProperty("relativeTolerance").GetDouble();
var homogeneousRows = new List<object>();
bool homogeneousConsistent = true;
foreach ((string id, string kind, double[] position) in basePoints.Where(x => x.Kind == "preserved-checkpoint-position"))
{
    double Scaled(double t)
    {
        var scaled = new double[n];
        for (int i = 0; i < n; i++) scaled[i] = t * position[i];
        return Value(scaled);
    }
    double sPlus = Scaled(1.0), sMinus = Scaled(-1.0), sTwo = Scaled(2.0);
    double degree3 = 0.5 * (sPlus - sMinus);
    double average = 0.5 * (sPlus + sMinus);
    double reduced = sTwo - 8.0 * degree3;
    double degree2 = (16.0 * average - reduced) / 12.0;
    double degree4 = (reduced - 4.0 * average) / 12.0;
    double predicted = probeT * probeT * degree2 + probeT * probeT * probeT * degree3
        + probeT * probeT * probeT * probeT * degree4;
    double observed = Scaled(probeT);
    double residual = System.Math.Abs(predicted - observed) / System.Math.Max(1.0, System.Math.Abs(observed));
    bool consistent = residual <= probeTolerance;
    homogeneousConsistent &= consistent;
    homogeneousRows.Add(new
    {
        id, valueAtPosition = sPlus, degree2, degree3, degree4,
        degree2Fraction = degree2 / sPlus, degree3Fraction = degree3 / sPlus, degree4Fraction = degree4 / sPlus,
        consistencyProbeT = probeT, predicted, observed, relativeResidual = residual, consistent,
    });
}

// ------------------------------------- arm H: measured observable invariance
int directionCount = armH.GetProperty("directionCount").GetInt32();
double[] displacements = armH.GetProperty("displacements").EnumerateArray().Select(x => x.GetDouble()).ToArray();
double invarianceTolerance = armH.GetProperty("invarianceRelativeTolerance").GetDouble();
string[] observableNames = armH.GetProperty("observables").EnumerateArray().Select(x => x.GetString()!).ToArray();
JsonElement declaredClasses = JsonDocument.Parse(File.ReadAllBytes(
    bindingSpecs.First(x => x.Id == "phase548-contract").Path)).RootElement
    .GetProperty("telemetrySchema").GetProperty("gaugeInvarianceClassification");
double[] Observables(double[] omega)
{
    var joint = op.ComputeJointGradient(omega, thetaZero, massMatrix);
    return [joint.Objective / n, Dot(joint.GradOmega, joint.GradOmega), Dot(omega, omega)];
}
var invarianceRows = new List<object>();
var worstByObservable = observableNames.ToDictionary(x => x, _ => 0.0);
bool anyDeclaredClassContradictedAtSomeBasePoint = false;
foreach ((string id, string kind, double[] position) in basePoints.Where(x => x.Kind != "flat-ray-point"))
{
    double[] baseline = Observables(position);
    var worst = new double[observableNames.Length];
    for (int k = 0; k < directionCount; k++)
    {
        int index = (int)((long)k * measuredNullBasisDimension / directionCount);
        double[] direction = nullBasisMatrix[index];
        foreach (double displacement in displacements)
        {
            var shifted = new double[n];
            for (int i = 0; i < n; i++) shifted[i] = position[i] + displacement * direction[i];
            double[] values = Observables(shifted);
            for (int o = 0; o < observableNames.Length; o++)
            {
                double deviation = System.Math.Abs(values[o] - baseline[o])
                    / System.Math.Max(1.0, System.Math.Abs(baseline[o]));
                worst[o] = System.Math.Max(worst[o], deviation);
            }
        }
    }
    for (int o = 0; o < observableNames.Length; o++)
    {
        worstByObservable[observableNames[o]] = System.Math.Max(worstByObservable[observableNames[o]], worst[o]);
        bool invariantHere = worst[o] <= invarianceTolerance;
        bool declaredInvariant = declaredClasses.GetProperty(observableNames[o]).GetString() == "gauge-invariant";
        if (invariantHere != declaredInvariant) anyDeclaredClassContradictedAtSomeBasePoint = true;
        invarianceRows.Add(new
        {
            basePoint = id, observable = observableNames[o],
            baselineValue = baseline[o], worstRelativeDeviation = worst[o],
            measuredFlatSectorInvariant = invariantHere,
            phase548DeclaredClass = declaredClasses.GetProperty(observableNames[o]).GetString(),
            declaredClassMatchesMeasuredBehaviourHere = invariantHere == declaredInvariant,
        });
    }
}
var invarianceSummary = observableNames.Select(name => new
{
    observable = name,
    phase548DeclaredClass = declaredClasses.GetProperty(name).GetString(),
    worstRelativeDeviationAcrossBasePoints = worstByObservable[name],
    measuredFlatSectorInvariant = worstByObservable[name] <= invarianceTolerance,
}).ToArray();

// --------------------------------------------------------- terminal selection
bool structuralPrechecksPassed = geometryMatchesContract && ddIdentityExact && scalarNullityExactlyDetermined
    && singleEdgeCurvatureExact && extractionAgreementPassed
    && flatnessConstructionValid && negativeControlsValid
    && measuredNullBasisDimension == flatSectorLowerBoundExact;
bool factorizationStable = symmetryPassed && spectralConsistencyPassed && residualBoundsPassed
    && inertiaLaddersConsistent && homogeneousConsistent;
bool nullityCertified = originBoundsConsistent && originPlateauPresent && originNullResidualPassed;

string verdict = !contractValid || !exactBindingsValid ? taxonomy[0]
    : !resourceAccepted ? taxonomy[1]
    : !structuralPrechecksPassed ? taxonomy[2]
    : !factorizationStable ? taxonomy[3]
    : !nullityCertified ? taxonomy[4]
    : taxonomy[5];

Directory.CreateDirectory(Path.GetDirectoryName(SpectraPath)!);
var spectraOptions = new JsonSerializerOptions { WriteIndented = false, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllBytes(SpectraPath, JsonSerializer.SerializeToUtf8Bytes(new
{
    schemaVersion = 1, phase = 550, degreesOfFreedom = n, basePoints = spectra,
}, spectraOptions));

var result = new
{
    schemaVersion = 1,
    phase = 550,
    phaseId = "phase550-complete-lattice-flat-sector-census",
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    contractValid,
    exactBindingsValid,
    resourceAccepted,
    bindings,
    completeLattice = new
    {
        member = "sd2-id0/c0.5", extent, thetaRule = "theta-identically-zero",
        vertexCount, edgeCount, faceCount, volumeCount = mesh.VolumeCount, cellCount = mesh.CellCount,
        eulerCharacteristic, degreesOfFreedom = n, geometryMatchesContract,
        betaIsARecordedLabelOnly = true, betaEntersExecutedValue = false,
    },
    structuralPrechecks = new
    {
        passed = structuralPrechecksPassed,
        ddIdentityExact, ddNonZeroCount,
        exactIntegerIncidence = new
        {
            rows = rankRows, ranksAgreeAcrossPrimes,
            scalarCoboundaryRank, scalarNullityUpperBound, scalarGeneratorRank,
            generatorsNotClosed, generatorsWithZeroNorm,
            scalarNullityExactlyDetermined, scalarNullityExact,
            flatSectorLowerBoundExact,
            isThresholdFree = true,
            twoSidedArgument = "the finite-field rank lower-bounds the rational rank, so the nullity is at most edges minus rank; the exhibited closed generator set of equal rank supplies the matching lower bound",
        },
        singleEdgeCurvature = new { subsetSize = singleEdgeSubsetSize, maximumAbsoluteDeviation = singleEdgeMaxDeviation, exact = singleEdgeCurvatureExact },
        extractionAgreement = new
        {
            passed = extractionAgreementPassed, rows = extractionRows,
            maximumExactVersusFourPointRelativeDeviation = maxExactVersusFourPoint,
            maximumCubicPolynomialityRelativeResidual = maxCubicPolynomialityResidual,
            note = "The dense form is assembled from the operator's own exact linearization primitives; the gradient extractions are independent cross-checks and also test the cubic premise instead of assuming it.",
        },
        measuredNullBasisDimension,
        nullBasisMatchesExactLowerBound = measuredNullBasisDimension == flatSectorLowerBoundExact,
    },
    exactFlatness = new
    {
        constructionValid = flatnessConstructionValid,
        negativeControlsValid,
        exactFlatSectorObserved,
        gatedOnConstructionAndControlsOnly = true,
        rows = flatnessRows,
    },
    spectralCensus = new
    {
        basePointCount = basePoints.Count,
        shiftLadder,
        plateauRungCount,
        everyRungIsThresholdConditional = true,
        noRungProvesAnExactZero = true,
        rows = censusRows,
        spectraFile = SpectraPath,
        spectraSha256 = Sha(SpectraPath),
    },
    nullityTwoSided = new
    {
        certified = nullityCertified,
        lowerBoundExactInteger = flatSectorLowerBoundExact,
        lowerBoundIsThresholdFree = true,
        upperBoundThresholdConditional = originNullityUpperBound,
        boundsConsistent = originBoundsConsistent,
        boundsCoincide = originBoundsConsistent && originNullityUpperBound == flatSectorLowerBoundExact,
        originPlateauPresent,
        originNullBasisResidualPassed = originNullResidualPassed,
        originLargestEigenvalue,
        interpretation = "A measured nullity is a fact about a matrix. This phase does not classify any measured null direction as gauge volume and applies no quotient and no measure normalization.",
    },
    homogeneousDecomposition = new { consistent = homogeneousConsistent, isFitted = false, rows = homogeneousRows },
    transverseScaleAlongFlatRay = new
    {
        rayRow = armG.GetProperty("rayRow").GetString(),
        ladder = rayLadder,
        rayConstructed = flatRayDirection is not null,
        isModelBased = true,
        isCertifiedProperty = false,
        rows = rayCensusRows,
    },
    measuredObservableInvariance = new
    {
        classificationIsMeasuredNotDeclared = true,
        isAGaugeOrbitStatement = false,
        directionCount, displacements, invarianceTolerance,
        summary = invarianceSummary,
        anyDeclaredClassContradictedAtSomeBasePoint,
        rows = invarianceRows,
        note = "Deviation is measured along the MEASURED flat sector of the second-order form. No gauge group, orbit or quotient is constructed, so this is not a gauge-invariance statement.",
    },
    resource = new
    {
        estimatedAggregateCpuSeconds = estimatedSeconds, maximumEstimatedAggregateCpuSeconds = maximumSeconds,
        estimatedPeakBytes = estimatedBytes, maximumEstimatedPeakBytes = maximumBytes,
        accepted = resourceAccepted, refuseBeforeAllocation = true,
        phase546SamplerCeilingUntouched = true, noSamplerRanUnderThisCeiling = true,
        ceilingRaisedAtRunTime = false,
    },
    verdictKind = verdict,
    terminalStatus = "complete-lattice-flat-sector-census-" + verdict,
    decision = verdict == taxonomy[5]
        ? "The second-order form of the registered complete-lattice action is characterized at the origin, at the six preserved pilot positions and along one flat ray, with a threshold-free integer lower bound on the flat-sector dimension, threshold-conditional inertia counts above the roundoff floor, certified extremal intervals, an exact homogeneous decomposition of the value at real configurations, and a measured observable invariance classification. Every quantity is a property of a discrete operator on a 3645-dimensional lattice-unit configuration space."
        : "The earliest frozen certification failure is preserved.",
    inferenceScope = new
    {
        workbenchRelativeLatticeUnitsOnly = true,
        establishesStationarity = false,
        establishesSamplingCorrectness = false,
        establishesMixingOrConvergence = false,
        establishesTransferToLargerExtent = false,
        establishesGaugeInterpretationOfANullDirection = false,
        establishesAQuotientOrMeasureNormalization = false,
        establishesAProductionDefault = false,
        establishesAPhysicalOrUnitCarryingQuantity = false,
        terminalKeyedToCertificationQualityNotOutcome = true,
    },
    rngUsed = false,
    hmcOrSamplingPerformed = false,
    configurationsRetained = false,
    registeredSeedTouched = false,
    nullSpaceInterpretedAsGaugeVolume = false,
    quotientApplied = false,
    gaugeFixingApplied = false,
    measureNormalizationApplied = false,
    phase548Or549Reinterpreted = false,
    phase535ExecutedReopenedOrMutated = false,
    phase481PackCreatedOrMutated = false,
    productionDefaultSelected = false,
    phase458G3Satisfied = false,
    phase458G4Satisfied = false,
    phase458G5Satisfied = false,
    o4Discharged = false,
    sourceContractApplicationAllowed = false,
    physicalUnitClaimAllowed = false,
    gevClaimAllowed = false,
    productionAuthorized = false,
    launchAuthorized = false,
    externalReviewPending = true,
    allDownstreamAuthority = false,
    promotedPhysicalMassClaimCount = 0,
};
WriteResult(result);
Console.WriteLine($"Phase550 verdict: {verdict}");
Console.WriteLine($"flat-sector exact lower bound = {flatSectorLowerBoundExact}, threshold-conditional upper bound = {originNullityUpperBound}");
Console.WriteLine($"exactFlatSectorObserved={exactFlatSectorObserved}, controls={negativeControlsValid}, declaredClassContradicted={anyDeclaredClassContradictedAtSomeBasePoint}");

void WriteResult(object payload)
{
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(payload,
        new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
    File.WriteAllBytes(OutputPath, bytes);
    File.WriteAllBytes(SummaryPath, bytes);
}

static double? Reportable(double value) => double.IsFinite(value) ? value : null;
static double Dot(double[] a, double[] b)
{
    double sum = 0.0;
    for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
    return sum;
}
static void Normalize(double[] x)
{
    double norm = System.Math.Sqrt(Dot(x, x));
    for (int i = 0; i < x.Length; i++) x[i] /= norm;
}
static double RelativeVectorDeviation(double[] actual, double[] expected)
{
    double numerator = 0.0, denominator = 0.0;
    for (int i = 0; i < actual.Length; i++)
    {
        double difference = actual[i] - expected[i];
        numerator += difference * difference;
        denominator += expected[i] * expected[i];
    }
    return System.Math.Sqrt(numerator / System.Math.Max(denominator, double.Epsilon));
}
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

// Row-echelon rank over a finite field. The rank over a finite field lower-bounds
// the rank over the rationals, which is the direction this phase relies on.
static int ModularRank(int[][] rows, int columns, long prime)
{
    var pivotOf = new int[columns];
    Array.Fill(pivotOf, -1);
    var basis = new List<long[]>();
    foreach (int[] source in rows)
    {
        if (basis.Count == columns) break;
        var row = new long[columns];
        for (int j = 0; j < columns; j++) row[j] = ((source[j] % prime) + prime) % prime;
        for (int c = 0; c < columns; c++)
        {
            if (row[c] == 0) continue;
            if (pivotOf[c] < 0)
            {
                long inverse = ModularPower(row[c], prime - 2, prime);
                for (int j = c; j < columns; j++) row[j] = row[j] * inverse % prime;
                pivotOf[c] = basis.Count;
                basis.Add(row);
                break;
            }
            long[] pivot = basis[pivotOf[c]];
            long factor = row[c];
            for (int j = c; j < columns; j++)
            {
                long value = row[j] - factor * pivot[j] % prime;
                row[j] = value < 0 ? value + prime : value;
            }
        }
    }
    return basis.Count;
}
static long ModularPower(long baseValue, long exponent, long modulus)
{
    long result = 1, current = baseValue % modulus;
    while (exponent > 0)
    {
        if ((exponent & 1) == 1) result = (long)((System.Int128)result * current % modulus);
        current = (long)((System.Int128)current * current % modulus);
        exponent >>= 1;
    }
    return result;
}

// Householder reduction of a dense symmetric matrix to tridiagonal form, without
// accumulating the transformation (eigenvalues only).
static void Tridiagonalize(double[] a, int n, out double[] d, out double[] e)
{
    d = new double[n];
    e = new double[n];
    for (int i = n - 1; i >= 1; i--)
    {
        int l = i - 1;
        double h = 0.0, scale = 0.0;
        if (l > 0)
        {
            for (int k = 0; k <= l; k++) scale += System.Math.Abs(a[(long)i * n + k]);
            if (scale == 0.0) e[i] = a[(long)i * n + l];
            else
            {
                for (int k = 0; k <= l; k++)
                {
                    a[(long)i * n + k] /= scale;
                    h += a[(long)i * n + k] * a[(long)i * n + k];
                }
                double f = a[(long)i * n + l];
                double g = f >= 0.0 ? -System.Math.Sqrt(h) : System.Math.Sqrt(h);
                e[i] = scale * g;
                h -= f * g;
                a[(long)i * n + l] = f - g;
                f = 0.0;
                for (int j = 0; j <= l; j++)
                {
                    g = 0.0;
                    for (int k = 0; k <= j; k++) g += a[(long)j * n + k] * a[(long)i * n + k];
                    for (int k = j + 1; k <= l; k++) g += a[(long)k * n + j] * a[(long)i * n + k];
                    e[j] = g / h;
                    f += e[j] * a[(long)i * n + j];
                }
                double hh = f / (h + h);
                for (int j = 0; j <= l; j++)
                {
                    f = a[(long)i * n + j];
                    e[j] = g = e[j] - hh * f;
                    for (int k = 0; k <= j; k++) a[(long)j * n + k] -= f * e[k] + g * a[(long)i * n + k];
                }
            }
        }
        else e[i] = a[(long)i * n + l];
        d[i] = h;
    }
    e[0] = 0.0;
    for (int i = 0; i < n; i++) d[i] = a[(long)i * n + i];
}

// QL with implicit shifts on the tridiagonal, eigenvalues only. The inputs are
// consumed. The subdiagonal deflation test is absolute against the norm of the
// tridiagonal, because a form with a large exactly degenerate null block forces
// near-zero subdiagonal entries that a purely relative test never deflates.
static double[] TridiagonalEigenvalues(double[] d, double[] e, int n, out bool converged)
{
    converged = true;
    for (int i = 1; i < n; i++) e[i - 1] = e[i];
    e[n - 1] = 0.0;
    double deflationThreshold = 2.220446049250313e-16
        * (d.Max(x => System.Math.Abs(x)) + 2.0 * e.Max(x => System.Math.Abs(x)));
    for (int l = 0; l < n; l++)
    {
        int iteration = 0, m;
        do
        {
            for (m = l; m < n - 1; m++)
            {
                double dd = System.Math.Abs(d[m]) + System.Math.Abs(d[m + 1]);
                if (System.Math.Abs(e[m]) <= System.Math.Max(2.220446049250313e-16 * dd, deflationThreshold)) break;
            }
            if (m == l) continue;
            if (iteration++ == 100) { converged = false; e[m] = 0.0; break; }
            double g = (d[l + 1] - d[l]) / (2.0 * e[l]);
            double r = Hypotenuse(g, 1.0);
            g = d[m] - d[l] + e[l] / (g + (g >= 0.0 ? System.Math.Abs(r) : -System.Math.Abs(r)));
            double s = 1.0, c = 1.0, p = 0.0;
            int i;
            for (i = m - 1; i >= l; i--)
            {
                double f = s * e[i];
                double b = c * e[i];
                e[i + 1] = r = Hypotenuse(f, g);
                if (r == 0.0)
                {
                    d[i + 1] -= p;
                    e[m] = 0.0;
                    break;
                }
                s = f / r;
                c = g / r;
                g = d[i + 1] - p;
                r = (d[i] - g) * s + 2.0 * c * b;
                d[i + 1] = g + (p = s * r);
                g = c * r - b;
            }
            if (r == 0.0 && i >= l) continue;
            d[l] -= p;
            e[l] = g;
            e[m] = 0.0;
        }
        while (m != l);
    }
    return d;
}
static double Hypotenuse(double a, double b)
{
    double absA = System.Math.Abs(a), absB = System.Math.Abs(b);
    if (absA > absB) return absA * System.Math.Sqrt(1.0 + absB / absA * (absB / absA));
    return absB == 0.0 ? 0.0 : absB * System.Math.Sqrt(1.0 + absA / absB * (absA / absB));
}

// Sturm-sequence (LDL^T) inertia count on the shifted tridiagonal: the number of
// eigenvalues strictly below the shift. Threshold-conditional by construction.
static int SturmCount(double[] d, double[] e, int n, double shift)
{
    int count = 0;
    double q = d[0] - shift;
    if (q < 0.0) count++;
    for (int i = 1; i < n; i++)
    {
        if (q == 0.0) q = -System.Math.Abs(e[i]) * 2.220446049250313e-16;
        q = d[i] - shift - e[i] * e[i] / q;
        if (q < 0.0) count++;
    }
    return count;
}

// Lanczos with full reorthogonalization and optional deflation against an
// orthonormal set. Returns the Rayleigh quotient of A on the extreme Ritz vector
// together with the a-posteriori residual bound ||A v - rho v||, which brackets a
// true eigenvalue of A restricted to the deflated subspace.
static (double Rayleigh, double Residual) LanczosExtreme(
    double[] a, int n, int steps, double[][]? deflation, bool wantSmallest)
{
    var basis = new List<double[]>();
    var alpha = new List<double>();
    var beta = new List<double>();
    var v = new double[n];
    for (int i = 0; i < n; i++) v[i] = System.Math.Cos((i + 1) * 0.6180339887498948);
    Project(v, deflation);
    double norm = System.Math.Sqrt(DotLocal(v, v));
    if (norm == 0.0) return (double.NaN, double.PositiveInfinity);
    for (int i = 0; i < n; i++) v[i] /= norm;
    basis.Add(v);
    var w = new double[n];
    for (int k = 0; k < steps; k++)
    {
        Multiply(a, n, basis[k], w);
        Project(w, deflation);
        double diagonal = DotLocal(basis[k], w);
        alpha.Add(diagonal);
        for (int pass = 0; pass < 2; pass++)
            foreach (double[] previous in basis)
            {
                double projection = DotLocal(w, previous);
                for (int i = 0; i < n; i++) w[i] -= projection * previous[i];
            }
        double next = System.Math.Sqrt(DotLocal(w, w));
        if (next <= 1e-13 || k + 1 == steps) break;
        beta.Add(next);
        var direction = new double[n];
        for (int i = 0; i < n; i++) direction[i] = w[i] / next;
        basis.Add(direction);
    }
    int m = alpha.Count;
    var small = new double[m * m];
    for (int i = 0; i < m; i++)
    {
        small[i * m + i] = alpha[i];
        if (i + 1 < m && i < beta.Count)
        {
            small[i * m + i + 1] = beta[i];
            small[(i + 1) * m + i] = beta[i];
        }
    }
    double[] vectors = JacobiEigen(small, m, out double[] values);
    int selected = 0;
    for (int i = 1; i < m; i++)
        if (wantSmallest ? values[i] < values[selected] : values[i] > values[selected]) selected = i;
    var ritz = new double[n];
    for (int j = 0; j < m; j++)
    {
        double weight = vectors[j * m + selected];
        double[] basisVector = basis[j];
        for (int i = 0; i < n; i++) ritz[i] += weight * basisVector[i];
    }
    double ritzNorm = System.Math.Sqrt(DotLocal(ritz, ritz));
    for (int i = 0; i < n; i++) ritz[i] /= ritzNorm;
    Multiply(a, n, ritz, w);
    double rayleigh = DotLocal(ritz, w);
    double residualSquared = 0.0;
    for (int i = 0; i < n; i++)
    {
        double difference = w[i] - rayleigh * ritz[i];
        residualSquared += difference * difference;
    }
    return (rayleigh, System.Math.Sqrt(residualSquared));

    static void Project(double[] x, double[][]? basis)
    {
        if (basis is null) return;
        foreach (double[] b in basis)
        {
            double projection = DotLocal(x, b);
            for (int i = 0; i < x.Length; i++) x[i] -= projection * b[i];
        }
    }
    static double DotLocal(double[] x, double[] y)
    {
        double sum = 0.0;
        for (int i = 0; i < x.Length; i++) sum += x[i] * y[i];
        return sum;
    }
}
static bool BracketsAnEigenvalue(double[] sortedEigenvalues, double centre, double radius)
{
    if (!double.IsFinite(centre) || !double.IsFinite(radius)) return false;
    foreach (double value in sortedEigenvalues)
        if (value >= centre - radius && value <= centre + radius) return true;
    return false;
}

// Cyclic Jacobi eigen-decomposition of a small dense symmetric matrix. Returns
// the eigenvector matrix in column-major-by-eigenvalue layout.
static double[] JacobiEigen(double[] matrix, int k, out double[] values)
{
    var a = (double[])matrix.Clone();
    var vectors = new double[k * k];
    for (int i = 0; i < k; i++) vectors[i * k + i] = 1.0;
    for (int sweep = 0; sweep < 100; sweep++)
    {
        double offDiagonal = 0.0;
        for (int p = 0; p < k; p++)
            for (int q = p + 1; q < k; q++) offDiagonal += a[p * k + q] * a[p * k + q];
        if (offDiagonal <= 1e-30) break;
        for (int p = 0; p < k; p++)
            for (int q = p + 1; q < k; q++)
            {
                double apq = a[p * k + q];
                if (apq == 0.0) continue;
                double theta = (a[q * k + q] - a[p * k + p]) / (2.0 * apq);
                double t = theta == 0.0
                    ? 1.0
                    : System.Math.Sign(theta) / (System.Math.Abs(theta) + System.Math.Sqrt(theta * theta + 1.0));
                double c = 1.0 / System.Math.Sqrt(t * t + 1.0);
                double s = t * c;
                for (int i = 0; i < k; i++)
                {
                    double aip = a[i * k + p], aiq = a[i * k + q];
                    a[i * k + p] = c * aip - s * aiq;
                    a[i * k + q] = s * aip + c * aiq;
                }
                for (int i = 0; i < k; i++)
                {
                    double api = a[p * k + i], aqi = a[q * k + i];
                    a[p * k + i] = c * api - s * aqi;
                    a[q * k + i] = s * api + c * aqi;
                }
                for (int i = 0; i < k; i++)
                {
                    double vip = vectors[i * k + p], viq = vectors[i * k + q];
                    vectors[i * k + p] = c * vip - s * viq;
                    vectors[i * k + q] = s * vip + c * viq;
                }
            }
    }
    values = new double[k];
    for (int i = 0; i < k; i++) values[i] = a[i * k + i];
    return vectors;
}
static void Multiply(double[] a, int n, double[] x, double[] y)
{
    for (int i = 0; i < n; i++)
    {
        double sum = 0.0;
        long rowOffset = (long)i * n;
        for (int j = 0; j < n; j++) sum += a[rowOffset + j] * x[j];
        y[i] = sum;
    }
}

// Eigenvalues of the restricted block P^T A P by cyclic Jacobi.
static double[] RestrictedSpectrum(double[] a, int n, double[][] basis)
{
    int k = basis.Length;
    if (k == 0) return [];
    var images = new double[k][];
    for (int c = 0; c < k; c++)
    {
        images[c] = new double[n];
        Multiply(a, n, basis[c], images[c]);
    }
    var block = new double[k * k];
    for (int r = 0; r < k; r++)
        for (int c = r; c < k; c++)
        {
            double sum = 0.0;
            double[] left = basis[r], right = images[c];
            for (int i = 0; i < n; i++) sum += left[i] * right[i];
            block[r * k + c] = sum;
            block[c * k + r] = sum;
        }
    _ = JacobiEigen(block, k, out double[] eigenvalues);
    Array.Sort(eigenvalues);
    return eigenvalues;
}
