using System.Security.Cryptography;
using System.Text.Json;
using Gu.Geometry;
using Gu.Math;

const string Root = "studies/phase579_collective_coordinate_jacobian_self_check_001";
const string ContractPath = Root + "/preregistration/phase579_collective_coordinate_jacobian_self_check_contract_v2.json";
const string OutputPath = Root + "/output/collective_coordinate_jacobian_self_check.json";
const string SummaryPath = Root + "/output/collective_coordinate_jacobian_self_check_summary.json";

using JsonDocument contractDocument = JsonDocument.Parse(File.ReadAllBytes(ContractPath));
JsonElement contract = contractDocument.RootElement;
var specs = contract.GetProperty("exactBindings").EnumerateArray().Select(x => new Binding(
    x.GetProperty("id").GetString()!, x.GetProperty("path").GetString()!, x.GetProperty("sha256").GetString()!)).ToArray();
var bindings = specs.Select(x =>
{
    string actual = File.Exists(x.Path) ? Sha(x.Path) : "missing";
    return new { id = x.Id, path = x.Path, expectedSha256 = x.Hash, actualSha256 = actual, hashMatches = actual == x.Hash };
}).ToArray();
bool exactBindingsValid = bindings.Length == 7
    && contract.GetProperty("requiredExactBindingCount").GetInt32() == 7
    && specs.Select(x => x.Id).Distinct(StringComparer.Ordinal).Count() == 7
    && specs.Select(x => x.Path).Distinct(StringComparer.Ordinal).Count() == 7
    && bindings.All(x => x.hashMatches);

string[] taxonomy = contract.GetProperty("terminalTaxonomyInPrecedenceOrder").EnumerateArray().Select(x => x.GetString()!).ToArray();
string[] expectedTaxonomy =
[
    "invalid-or-drifted-input",
    "known-answer-battery-failed",
    "phase450-lineage-convention-defective-preserved-negative",
    "collective-coordinate-jacobian-self-check-passed-evidence-backed",
];
JsonElement reduced = contract.GetProperty("committedReducedSetting");
JsonElement jacobianSpec = contract.GetProperty("jacobianCheck");
JsonElement gaussianSpec = contract.GetProperty("solvableLimit");
JsonElement transformSpec = contract.GetProperty("registeredExactTransformations");
bool contractValid = contract.GetProperty("schemaVersion").GetInt32() == 1
    && contract.GetProperty("phase").GetInt32() == 579
    && contract.GetProperty("contractId").GetString() == "phase579-a42-collective-coordinate-jacobian-self-check-v2"
    && contract.GetProperty("frozenBeforeFirstExecution").GetBoolean()
    && contract.GetProperty("deterministic").GetBoolean()
    && !contract.GetProperty("newSamplingPerformed").GetBoolean()
    && contract.GetProperty("rngUsedOnlyToReconstructRegisteredPhase450Ray").GetBoolean()
    && contract.GetProperty("registeredRaySeed").GetInt32() == 20260703
    && reduced.GetProperty("torusSize").GetInt32() == 3
    && reduced.GetProperty("latticeCanonical").GetBoolean()
    && reduced.GetProperty("lieAlgebraId").GetString() == "su2-trace-pairing"
    && reduced.GetProperty("coordinate").GetString() == "Phi=<u_inv,omega>"
    && reduced.GetProperty("edgeTypesUsed").GetInt32() == 15
    && reduced.GetProperty("registeredCoefficientSlots").GetInt32() == 48
    && reduced.GetProperty("globalSu2DirectionsProjected").GetInt32() == 3
    && taxonomy.SequenceEqual(expectedTaxonomy, StringComparer.Ordinal)
    && contract.GetProperty("authorityFirewalls").EnumerateObject().All(x => x.Value.ValueKind == JsonValueKind.False)
    && contract.GetProperty("externalReviewPending").GetBoolean()
    && contract.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0
    && exactBindingsValid;

// Known-answer batteries run before any exact-bound scientific JSON is parsed.
double[] batteryU = Normalize([3.0, -4.0, 12.0]);
double[] batteryX = [0.25, -0.5, 0.125];
const double BatteryH = 1e-6;
double[] batteryFd = new double[batteryU.Length];
for (int i = 0; i < batteryU.Length; i++)
{
    double[] plus = (double[])batteryX.Clone(); plus[i] += BatteryH;
    double[] minus = (double[])batteryX.Clone(); minus[i] -= BatteryH;
    batteryFd[i] = (Dot(batteryU, plus) - Dot(batteryU, minus)) / (2.0 * BatteryH);
}
double batteryJacobianError = MaxAbsDifference(batteryU, batteryFd);
const double BatteryAlpha = 2.0;
double batteryGaussianError = new[] { -1.0, -0.25, 0.5, 1.25 }
    .Max(phi => System.Math.Abs((-System.Math.Log(System.Math.Exp(-0.5 * BatteryAlpha * phi * phi)))
        - 0.5 * BatteryAlpha * phi * phi));
bool checksumTamperDetected;
{
    byte[] original = System.Text.Encoding.UTF8.GetBytes("{\"phase\":579,\"fixture\":\"checksum\"}");
    byte[] tampered = (byte[])original.Clone();
    tampered[^2] ^= 1;
    checksumTamperDetected = !SHA256.HashData(original).SequenceEqual(SHA256.HashData(tampered));
}
bool knownAnswerPassed = batteryJacobianError <= 1e-10 && batteryGaussianError <= 1e-15 && checksumTamperDetected;
var knownAnswerBattery = new
{
    auditedScientificJsonParsedBeforeBattery = false,
    deterministicFixturesOnly = true,
    finiteDifferenceJacobianMaximumAbsoluteError = batteryJacobianError,
    gaussianRelativePotentialMaximumAbsoluteError = batteryGaussianError,
    checksumTamperDetected,
    passed = knownAnswerPassed,
};
if (!contractValid || !knownAnswerPassed)
{
    string early = !contractValid ? taxonomy[0] : taxonomy[1];
    Emit(Early(early, contractValid, exactBindingsValid, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase579 verdict: {early}");
    return;
}

// Only now consume the exact-bound Phase450 and Phase485 records.
using JsonDocument phase450Document = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase450-binding-condition-record-summary")));
using JsonDocument phase485Document = JsonDocument.Parse(File.ReadAllBytes(PathFor("phase485-falsifier-census-summary")));
JsonElement phase450 = phase450Document.RootElement;
JsonElement phase485 = phase485Document.RootElement;
JsonElement phase450Torus = phase450.GetProperty("torusArms")[0];
JsonElement phase450Battery = phase450.GetProperty("batteries").GetProperty("perTorus")[0];
string[] committedConditions = phase450.GetProperty("probeConfiguration").GetProperty("fourBindingConditions")
    .EnumerateArray().Select(x => x.GetString()!).ToArray();
JsonElement censusRow = phase485.GetProperty("rows").EnumerateArray()
    .Single(x => x.GetProperty("rulingId").GetString() == "O4-F1-COLLECTIVE-COORDINATE");

bool upstreamRecordValid = phase450.GetProperty("phaseId").GetString() == "phase450-constraint-effective-potential-hmc-probe"
    && phase450.GetProperty("constraintEffectivePotentialHmcProbePassed").GetBoolean()
    && phase450.GetProperty("recordedBoundary").GetProperty("cepConventionsAreWorkbenchConventions").GetBoolean()
    && phase450.GetProperty("probeConfiguration").GetProperty("rayRngSeed").GetInt32() == 20260703
    && phase450.GetProperty("probeConfiguration").GetProperty("latticeCanonical").GetBoolean()
    && phase450.GetProperty("probeConfiguration").GetProperty("lieAlgebraId").GetString() == "su2-trace-pairing"
    && committedConditions.Length == 4
    && committedConditions[0].StartsWith("(i) gauge-invariant invariant-ray collective coordinate", StringComparison.Ordinal)
    && phase450Battery.GetProperty("orbitMapOk").GetBoolean()
    && phase450Battery.GetProperty("projectorBattery").GetBoolean()
    && censusRow.GetProperty("selfCheckMethod").GetString() == "collective-coordinate Jacobian and solvable-limit comparison"
    && censusRow.GetProperty("falsifierDefined").GetBoolean()
    && !censusRow.GetProperty("externalInterpretationStillRequired").GetBoolean()
    && !censusRow.GetProperty("mayAuthorRuling").GetBoolean();
if (!upstreamRecordValid)
{
    Emit(Early(taxonomy[0], true, true, bindings, knownAnswerBattery));
    Console.WriteLine($"Phase579 verdict: {taxonomy[0]}");
    return;
}

int n = reduced.GetProperty("torusSize").GetInt32();
var mesh = SimplicialMeshGenerator.CreateUniform4DPeriodic(n, latticeCanonical: true);
var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
int dimG = algebra.Dimension;
int nOmega = mesh.EdgeCount * dimG;
int nVert = mesh.VertexCount;

// Verbatim Phase450 orbit map and oSign base-to-tip representation.
var coords = new int[nVert][];
double spacing0 = double.MaxValue;
for (int v = 0; v < nVert; v++)
{
    ReadOnlySpan<double> p = mesh.GetVertexCoordinates(v);
    for (int d = 0; d < 4; d++) if (p[d] > 1e-12) spacing0 = System.Math.Min(spacing0, p[d]);
    coords[v] = new int[4];
}
for (int v = 0; v < nVert; v++)
{
    ReadOnlySpan<double> p = mesh.GetVertexCoordinates(v);
    for (int d = 0; d < 4; d++) coords[v][d] = (int)System.Math.Round(p[d] / spacing0);
}
var vertexAt = new Dictionary<(int, int, int, int), int>();
for (int v = 0; v < nVert; v++) vertexAt[(coords[v][0], coords[v][1], coords[v][2], coords[v][3])] = v;
int TypeOf(int[] displacement) => ((displacement[0] << 3) | (displacement[1] << 2) | (displacement[2] << 1) | displacement[3]) - 1;
var edgeBase = new int[mesh.EdgeCount];
var edgeType = new int[mesh.EdgeCount];
bool orbitMapOk = true;
for (int e = 0; e < mesh.EdgeCount; e++)
{
    int a = mesh.Edges[e][0], b = mesh.Edges[e][1];
    int[]? displacement = MinImage01(coords[a], coords[b], n);
    if (displacement is not null) { edgeBase[e] = a; edgeType[e] = TypeOf(displacement); }
    else
    {
        displacement = MinImage01(coords[b], coords[a], n);
        if (displacement is not null) { edgeBase[e] = b; edgeType[e] = TypeOf(displacement); }
        else orbitMapOk = false;
    }
}
var edgeAt = new int[nVert, 15];
for (int e = 0; e < mesh.EdgeCount; e++) edgeAt[edgeBase[e], edgeType[e]] = e;
var oSign = new double[mesh.EdgeCount];
for (int e = 0; e < mesh.EdgeCount; e++) oSign[e] = mesh.Edges[e][0] == edgeBase[e] ? 1.0 : -1.0;

double[] Translate(double[] source, int[] shift)
{
    var translated = new double[nOmega];
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        int[] c0 = coords[edgeBase[e]];
        int tv = vertexAt[(Mod(c0[0] + shift[0], n), Mod(c0[1] + shift[1], n),
            Mod(c0[2] + shift[2], n), Mod(c0[3] + shift[3], n))];
        int te = edgeAt[tv, edgeType[e]];
        double sign = oSign[e] * oSign[te];
        for (int l = 0; l < dimG; l++) translated[te * dimG + l] = sign * source[e * dimG + l];
    }
    return translated;
}

// Verbatim registered PRNG reconstruction. This is construction, not sampling.
var rayCoefficientRng = new Random(contract.GetProperty("registeredRaySeed").GetInt32());
var typeCoefficients = new double[16 * dimG];
for (int i = 0; i < typeCoefficients.Length; i++) typeCoefficients[i] = rayCoefficientRng.NextDouble() - 0.5;
var uInv = new double[nOmega];
for (int e = 0; e < mesh.EdgeCount; e++)
    for (int l = 0; l < dimG; l++)
        uInv[e * dimG + l] = oSign[e] * typeCoefficients[edgeType[e] * dimG + l];
double normRaw = Norm(uInv);
ScaleInPlace(uInv, 1.0 / normRaw);

var zDirections = new double[dimG][];
var overlapsBefore = new double[dimG];
for (int l = 0; l < dimG; l++)
{
    var z = new double[nOmega];
    var basis = new double[dimG]; basis[l] = 1.0;
    for (int e = 0; e < mesh.EdgeCount; e++)
    {
        var ue = new double[dimG];
        for (int c = 0; c < dimG; c++) ue[c] = uInv[e * dimG + c];
        double[] bracket = algebra.Bracket(basis, ue);
        for (int c = 0; c < dimG; c++) z[e * dimG + c] = bracket[c];
    }
    ScaleInPlace(z, 1.0 / Norm(z));
    zDirections[l] = z;
    overlapsBefore[l] = Dot(uInv, z);
}
var zOrtho = new List<double[]>();
foreach (double[] z in zDirections)
{
    var w = (double[])z.Clone();
    foreach (double[] q in zOrtho) AddScaled(w, q, -Dot(w, q));
    double wn = Norm(w);
    if (wn > 1e-12) { ScaleInPlace(w, 1.0 / wn); zOrtho.Add(w); }
}
foreach (double[] q in zOrtho) AddScaled(uInv, q, -Dot(uInv, q));
double normAfterProjectionBeforeRenormalization = Norm(uInv);
ScaleInPlace(uInv, 1.0 / normAfterProjectionBeforeRenormalization);
double finalNorm = Norm(uInv);
double[] overlapsAfter = zOrtho.Select(q => Dot(uInv, q)).ToArray();

double Phi(double[] omega) => Dot(uInv, omega);
double committedNormRaw = phase450Torus.GetProperty("uInvNormRaw").GetDouble();
double committedNormAfter = phase450Torus.GetProperty("uInvNormAfterProjection").GetDouble();
bool reconstructionPassed = orbitMapOk
    && mesh.VertexCount == phase450Torus.GetProperty("vertexCount").GetInt32()
    && mesh.EdgeCount == phase450Torus.GetProperty("edgeCount").GetInt32()
    && nOmega == phase450Torus.GetProperty("nOmega").GetInt32()
    && zOrtho.Count == 3
    && System.Math.Abs(normRaw - committedNormRaw) <= 2e-14
    && System.Math.Abs(normAfterProjectionBeforeRenormalization - committedNormAfter) <= 2e-14;

// Analytic Jacobian and an independent centered finite-difference row.
double[] fdSteps = jacobianSpec.GetProperty("finiteDifferenceSteps").EnumerateArray().Select(x => x.GetDouble()).ToArray();
var finiteDifferenceRows = new List<object>();
double fdMaximumError = 0.0;
var fdState = new double[nOmega];
for (int i = 0; i < nOmega; i++) fdState[i] = 0.005 * System.Math.Sin(0.37 * i + 0.19);
foreach (double h in fdSteps)
{
    double maxError = 0.0;
    int worstIndex = -1;
    for (int i = 0; i < nOmega; i++)
    {
        double old = fdState[i];
        fdState[i] = old + h; double plus = Phi(fdState);
        fdState[i] = old - h; double minus = Phi(fdState);
        fdState[i] = old;
        double derivative = (plus - minus) / (2.0 * h);
        double error = System.Math.Abs(derivative - uInv[i]);
        if (error > maxError) { maxError = error; worstIndex = i; }
    }
    fdMaximumError = System.Math.Max(fdMaximumError, maxError);
    finiteDifferenceRows.Add(new { step = h, maximumAbsoluteError = maxError, worstIndex });
}
double coareaJacobian = Norm(uInv);
double constraintMeasureFactor = 1.0 / coareaJacobian;
bool jacobianPassed = System.Math.Abs(coareaJacobian - 1.0) <= jacobianSpec.GetProperty("unitNormMaximumAbsoluteError").GetDouble()
    && fdMaximumError <= jacobianSpec.GetProperty("finiteDifferenceMaximumAbsoluteError").GetDouble();

// Exactly solvable Gaussian/free comparison in an independently assembled 4D
// collective-plus-transverse coordinate system. Tensor Simpson quadrature is
// used only as a deterministic integration comparator.
double alpha = gaussianSpec.GetProperty("alpha").GetDouble();
double[] phiValues = gaussianSpec.GetProperty("coordinateValues").EnumerateArray().Select(x => x.GetDouble()).ToArray();
double[] fixtureU = Normalize(uInv.Take(4).ToArray());
double[][] fixtureBasis = CompleteBasis(fixtureU);
double fixtureGramMaximumError = GramMaximumError(fixtureBasis);
double transverseLimit = 6.0 / System.Math.Sqrt(alpha);
const int SimpsonIntervals = 24;
var gaussianIntegrals = new Dictionary<double, double>();
foreach (double phi in phiValues)
    gaussianIntegrals[phi] = ConstrainedGaussianIntegral(phi, alpha, fixtureBasis, transverseLimit, SimpsonIntervals);
double integralAtZero = gaussianIntegrals[0.0];
var gaussianRows = phiValues.Select(phi =>
{
    double constructed = -System.Math.Log(gaussianIntegrals[phi] / integralAtZero);
    double closedForm = 0.5 * alpha * phi * phi;
    return new { phi, constructedRelativePotential = constructed, closedFormRelativePotential = closedForm, absoluteError = System.Math.Abs(constructed - closedForm) };
}).ToArray();
double gaussianMaximumError = gaussianRows.Max(x => x.absoluteError);
double logClosedFormConstrainedNormalizationAtZero = 0.5 * (nOmega - 1) * System.Math.Log(2.0 * System.Math.PI / alpha)
    - System.Math.Log(coareaJacobian);
bool gaussianPassed = fixtureGramMaximumError <= 5e-15
    && gaussianMaximumError <= gaussianSpec.GetProperty("maximumAbsolutePotentialError").GetDouble();

// Every exact lattice translation in the committed reduced setting.
var transformState = new double[nOmega];
for (int i = 0; i < nOmega; i++) transformState[i] = 0.02 * System.Math.Sin(0.17 * i + 0.31) + 0.01 * System.Math.Cos(0.43 * i - 0.27);
double translationRayMaximumError = 0.0;
double translationCoordinateMaximumError = 0.0;
int translationCount = 0;
for (int a0 = 0; a0 < n; a0++)
for (int a1 = 0; a1 < n; a1++)
for (int a2 = 0; a2 < n; a2++)
for (int a3 = 0; a3 < n; a3++)
{
    int[] shift = [a0, a1, a2, a3];
    double[] translatedU = Translate(uInv, shift);
    double[] translatedState = Translate(transformState, shift);
    translationRayMaximumError = System.Math.Max(translationRayMaximumError, MaxAbsDifference(uInv, translatedU));
    translationCoordinateMaximumError = System.Math.Max(translationCoordinateMaximumError, System.Math.Abs(Phi(translatedState) - Phi(transformState)));
    translationCount++;
}

// Exact global adjoint transformations. The decisive test is ACTIVE: omega
// transforms while Phase450's registered u_inv remains fixed. Simultaneously
// rotating both is passive covariance only and is retained as a diagnostic.
var rotations = new[]
{
    (Axis: Normalize([1.0, 2.0, -1.0]), Angle: 0.37),
    (Axis: Normalize([-2.0, 0.5, 1.0]), Angle: 1.2),
    (Axis: Normalize([0.3, -0.7, 1.4]), Angle: System.Math.PI),
};
var onRayState = uInv.Select(value => 0.8 * value).ToArray();
double activeFixedRayCoordinateMaximumError = 0.0;
double activeFixedRayOnRayMaximumError = 0.0;
double passiveCovarianceCoordinateMaximumError = 0.0;
double globalAdjointNormMaximumError = 0.0;
foreach (var rotation in rotations)
{
    double[] rotatedU = RotateField(uInv, rotation.Axis, rotation.Angle);
    double[] rotatedState = RotateField(transformState, rotation.Axis, rotation.Angle);
    double[] rotatedOnRayState = RotateField(onRayState, rotation.Axis, rotation.Angle);
    activeFixedRayCoordinateMaximumError = System.Math.Max(activeFixedRayCoordinateMaximumError,
        System.Math.Abs(Phi(rotatedState) - Phi(transformState)));
    activeFixedRayOnRayMaximumError = System.Math.Max(activeFixedRayOnRayMaximumError,
        System.Math.Abs(Phi(rotatedOnRayState) - Phi(onRayState)));
    passiveCovarianceCoordinateMaximumError = System.Math.Max(passiveCovarianceCoordinateMaximumError,
        System.Math.Abs(Dot(rotatedU, rotatedState) - Phi(transformState)));
    globalAdjointNormMaximumError = System.Math.Max(globalAdjointNormMaximumError, System.Math.Abs(Norm(rotatedU) - finalNorm));
}
double projectedTangentMaximumOverlap = overlapsAfter.Max(System.Math.Abs);
double transformTolerance = transformSpec.GetProperty("maximumAbsoluteCoordinateError").GetDouble();
bool transformationsPassed = translationCount == 81
    && translationRayMaximumError <= transformTolerance
    && translationCoordinateMaximumError <= transformTolerance
    && activeFixedRayCoordinateMaximumError <= transformTolerance
    && activeFixedRayOnRayMaximumError <= transformTolerance
    && globalAdjointNormMaximumError <= transformTolerance
    && projectedTangentMaximumOverlap <= transformSpec.GetProperty("maximumProjectedTangentOverlap").GetDouble();

bool falsifierPassed = reconstructionPassed && jacobianPassed && gaussianPassed && transformationsPassed;
string verdict = falsifierPassed ? taxonomy[3] : taxonomy[2];
var result = new
{
    phaseId = "phase579-collective-coordinate-jacobian-self-check",
    schemaVersion = 1,
    phase = 579,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A42",
    amendmentOrder = 2,
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    terminalStatus = verdict,
    verdictKind = verdict,
    applicationSubjectKind = "o4-f1-collective-coordinate-falsifier",
    rulingId = "O4-F1-COLLECTIVE-COORDINATE",
    selfCheckMethod = censusRow.GetProperty("selfCheckMethod").GetString(),
    deterministic = true,
    newSamplingPerformed = false,
    registeredRayPrngReconstructionOnly = true,
    contractValid = true,
    exactBindingsValid = true,
    bindings,
    knownAnswerBattery,
    phase450BindingConditions = committedConditions,
    reconstruction = new
    {
        passed = reconstructionPassed,
        n,
        vertexCount = mesh.VertexCount,
        edgeCount = mesh.EdgeCount,
        nOmega,
        orbitMapOk,
        registeredCoefficientCount = typeCoefficients.Length,
        activeEdgeTypeCount = edgeType.Distinct().Count(),
        globalSu2OrthonormalDirectionCount = zOrtho.Count,
        uInvNormRaw = normRaw,
        committedUInvNormRaw = committedNormRaw,
        uInvNormAfterProjectionBeforeRenormalization = normAfterProjectionBeforeRenormalization,
        committedUInvNormAfterProjection = committedNormAfter,
        uInvFinalNorm = finalNorm,
        projectorOverlapsBefore = overlapsBefore,
        projectorOverlapsAfter = overlapsAfter,
    },
    jacobian = new
    {
        passed = jacobianPassed,
        analyticDefinition = "dPhi/domega=u_inv; sqrt(det(J J^T))=norm(u_inv)",
        finiteDifferenceConstruction = "centered coordinate-wise differences over all 3645 omega coefficients; no shared analytic derivative code",
        componentCount = nOmega,
        coareaJacobian,
        constraintMeasureFactor,
        finiteDifferenceRows,
        finiteDifferenceMaximumAbsoluteError = fdMaximumError,
        finiteDifferenceTolerance = jacobianSpec.GetProperty("finiteDifferenceMaximumAbsoluteError").GetDouble(),
    },
    exactlySolvableLimit = new
    {
        passed = gaussianPassed,
        target = "exp(-alpha*norm(omega)^2/2), with delta(Phi-<u,omega>)",
        alpha,
        fixtureDimension = 4,
        fixtureBasisGramMaximumAbsoluteError = fixtureGramMaximumError,
        deterministicQuadrature = new { rule = "tensor-composite-Simpson", transverseDimensions = 3, intervalsPerDimension = SimpsonIntervals, transverseLimit },
        gaussianRows,
        maximumAbsolutePotentialError = gaussianMaximumError,
        tolerance = gaussianSpec.GetProperty("maximumAbsolutePotentialError").GetDouble(),
        fullReducedSettingLogConstrainedNormalizationAtZero = logClosedFormConstrainedNormalizationAtZero,
        fullReducedSettingRelativePotentialFormula = "alpha*Phi^2/2",
        coareaFactorIncluded = true,
    },
    exactTransformationInvariance = new
    {
        passed = transformationsPassed,
        latticeTranslationCount = translationCount,
        latticeTranslationRayMaximumAbsoluteError = translationRayMaximumError,
        latticeTranslationCoordinateMaximumAbsoluteError = translationCoordinateMaximumError,
        globalAdjointRotationCount = rotations.Length,
        decisiveGlobalAdjointSemantics = "active exact SO(3) adjoint action on omega with the registered Phase450 u_inv fixed",
        activeFixedRayCoordinateMaximumAbsoluteError = activeFixedRayCoordinateMaximumError,
        activeFixedRayOnRegisteredRayMaximumAbsoluteError = activeFixedRayOnRayMaximumError,
        passiveCovarianceSemantics = "simultaneous SO(3) adjoint action on u_inv and omega; diagnostic only, not the gauge-invariance gate",
        passiveCovarianceCoordinateMaximumAbsoluteError = passiveCovarianceCoordinateMaximumError,
        globalAdjointNormMaximumAbsoluteError = globalAdjointNormMaximumError,
        projectedGlobalOrbitTangentMaximumAbsoluteOverlap = projectedTangentMaximumOverlap,
        tolerance = transformTolerance,
    },
    falsifierPassed,
    evidenceBackedInternalProposalAllowed = falsifierPassed,
    preservedFirstClassNegative = !falsifierPassed,
    phase450RecordMutated = false,
    mayAuthorRuling = false,
    humanRulingAuthored = false,
    o4Discharged = false,
    externalReviewPending = true,
    phase458Satisfied = false,
    sourceContractApplicationAllowed = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = falsifierPassed
        ? "The registered reduced-setting linear collective coordinate passes its Jacobian, exactly solvable free-Gaussian, and registered exact-transformation batteries. This supports an evidence-backed internal O4 proposal but authors no ruling."
        : "At least one frozen Jacobian, solvable-limit, reconstruction, or exact-transformation gate failed. The Phase450-lineage collective-coordinate convention is preserved as a first-class negative; Phase450 is unchanged and no ruling is authored.",
};
Emit(result);
Console.WriteLine($"Phase579 verdict: {verdict}");

object Early(string verdict, bool valid, bool bindingsValid, object bindingRows, object battery) => new
{
    phaseId = "phase579-collective-coordinate-jacobian-self-check",
    schemaVersion = 1,
    phase = 579,
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A42",
    amendmentOrder = 2,
    contractId = contract.GetProperty("contractId").GetString(),
    contractSha256 = Sha(ContractPath),
    terminalStatus = verdict,
    verdictKind = verdict,
    applicationSubjectKind = "o4-f1-collective-coordinate-falsifier",
    rulingId = "O4-F1-COLLECTIVE-COORDINATE",
    deterministic = true,
    newSamplingPerformed = false,
    contractValid = valid,
    exactBindingsValid = bindingsValid,
    bindings = bindingRows,
    knownAnswerBattery = battery,
    falsifierPassed = false,
    evidenceBackedInternalProposalAllowed = false,
    preservedFirstClassNegative = false,
    phase450RecordMutated = false,
    mayAuthorRuling = false,
    humanRulingAuthored = false,
    o4Discharged = false,
    externalReviewPending = true,
    phase458Satisfied = false,
    sourceContractApplicationAllowed = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
};

void Emit(object payload)
{
    Directory.CreateDirectory(Path.GetDirectoryName(OutputPath)!);
    var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    string json = JsonSerializer.Serialize(payload, options) + Environment.NewLine;
    File.WriteAllText(OutputPath, json);
    File.WriteAllText(SummaryPath, json);
}

string PathFor(string id) => specs.Single(x => x.Id == id).Path;
static string Sha(string path) => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
static double Dot(double[] a, double[] b)
{
    double sum = 0.0;
    for (int i = 0; i < a.Length; i++) sum += a[i] * b[i];
    return sum;
}
static double Norm(double[] a) => System.Math.Sqrt(Dot(a, a));
static double[] Normalize(double[] a)
{
    var result = (double[])a.Clone();
    ScaleInPlace(result, 1.0 / Norm(result));
    return result;
}
static void ScaleInPlace(double[] a, double scale)
{
    for (int i = 0; i < a.Length; i++) a[i] *= scale;
}
static void AddScaled(double[] target, double[] source, double scale)
{
    for (int i = 0; i < target.Length; i++) target[i] += scale * source[i];
}
static double MaxAbsDifference(double[] a, double[] b)
{
    double maximum = 0.0;
    for (int i = 0; i < a.Length; i++) maximum = System.Math.Max(maximum, System.Math.Abs(a[i] - b[i]));
    return maximum;
}
static int Mod(int x, int n) => (x % n + n) % n;
static int[]? MinImage01(int[] a, int[] b, int n)
{
    var displacement = new int[4];
    for (int d = 0; d < 4; d++)
    {
        int delta = Mod(b[d] - a[d], n);
        if (delta is not (0 or 1)) return null;
        displacement[d] = delta;
    }
    return displacement.Any(x => x != 0) ? displacement : null;
}
static double[][] CompleteBasis(double[] first)
{
    var basis = new List<double[]> { Normalize(first) };
    for (int axis = 0; axis < first.Length && basis.Count < first.Length; axis++)
    {
        var candidate = new double[first.Length]; candidate[axis] = 1.0;
        foreach (double[] q in basis) AddScaled(candidate, q, -Dot(candidate, q));
        double norm = Norm(candidate);
        if (norm > 1e-12) { ScaleInPlace(candidate, 1.0 / norm); basis.Add(candidate); }
    }
    return basis.ToArray();
}
static double GramMaximumError(double[][] basis)
{
    double maximum = 0.0;
    for (int i = 0; i < basis.Length; i++)
        for (int j = 0; j < basis.Length; j++)
            maximum = System.Math.Max(maximum, System.Math.Abs(Dot(basis[i], basis[j]) - (i == j ? 1.0 : 0.0)));
    return maximum;
}
static double ConstrainedGaussianIntegral(double phi, double alpha, double[][] basis, double limit, int intervals)
{
    double h = 2.0 * limit / intervals;
    double sum = 0.0;
    for (int i = 0; i <= intervals; i++)
    for (int j = 0; j <= intervals; j++)
    for (int k = 0; k <= intervals; k++)
    {
        double y1 = -limit + h * i, y2 = -limit + h * j, y3 = -limit + h * k;
        var x = new double[4];
        for (int d = 0; d < 4; d++) x[d] = phi * basis[0][d] + y1 * basis[1][d] + y2 * basis[2][d] + y3 * basis[3][d];
        int wi = i is 0 || i == intervals ? 1 : i % 2 == 0 ? 2 : 4;
        int wj = j is 0 || j == intervals ? 1 : j % 2 == 0 ? 2 : 4;
        int wk = k is 0 || k == intervals ? 1 : k % 2 == 0 ? 2 : 4;
        sum += wi * wj * wk * System.Math.Exp(-0.5 * alpha * Dot(x, x));
    }
    return sum * h * h * h / 27.0;
}
static double[] RotateField(double[] field, double[] axis, double angle)
{
    var result = new double[field.Length];
    double c = System.Math.Cos(angle), s = System.Math.Sin(angle), oneMinusC = 1.0 - c;
    for (int i = 0; i < field.Length; i += 3)
    {
        double x = field[i], y = field[i + 1], z = field[i + 2];
        double dot = axis[0] * x + axis[1] * y + axis[2] * z;
        double crossX = axis[1] * z - axis[2] * y;
        double crossY = axis[2] * x - axis[0] * z;
        double crossZ = axis[0] * y - axis[1] * x;
        result[i] = c * x + s * crossX + oneMinusC * dot * axis[0];
        result[i + 1] = c * y + s * crossY + oneMinusC * dot * axis[1];
        result[i + 2] = c * z + s * crossZ + oneMinusC * dot * axis[2];
    }
    return result;
}

sealed record Binding(string Id, string Path, string Hash);
