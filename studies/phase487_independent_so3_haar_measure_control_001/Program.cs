using System.Diagnostics;
using System.Text.Json;

var stopwatch = Stopwatch.StartNew();
const string outputDir = "studies/phase487_independent_so3_haar_measure_control_001/output";
const string passedTerminal = "independent-so3-haar-measure-control-passed-all-batteries-exploration-only";
const string failedTerminal = "independent-so3-haar-measure-control-failed-one-or-more-batteries-exploration-only";
const int thetaOrder = 96;
const int axisZOrder = 24;
const int axisPhiCount = 64;
const double radialTolerance = 2e-13;
const double characterTolerance = 2e-12;
const double matrixTolerance = 3e-13;
const double compositionTolerance = 5e-13;
const double boundaryTolerance = 2e-14;

var (thetaNodes, thetaWeightsRaw) = GaussLegendre(thetaOrder, 0.0, Math.PI);
var thetaWeights = thetaNodes.Select((theta, i) => thetaWeightsRaw[i] * RadialDensity(theta)).ToArray();
double normalization = thetaWeights.Sum();
double numericalMeanTheta = Weighted(thetaNodes, thetaWeights, theta => theta);
double numericalMeanThetaSquared = Weighted(thetaNodes, thetaWeights, theta => theta * theta);
double numericalMeanCosTheta = Weighted(thetaNodes, thetaWeights, Math.Cos);
double numericalMeanCosThetaSquared = Weighted(thetaNodes, thetaWeights, theta => Math.Cos(theta) * Math.Cos(theta));
double analyticMeanTheta = Math.PI / 2.0 + 2.0 / Math.PI;
double analyticMeanThetaSquared = Math.PI * Math.PI / 3.0 + 2.0;

double cdfMaxError = 0.0;
for (int i = 0; i <= 16; i++)
{
    double theta = Math.PI * i / 16.0;
    var (nodes, weights) = GaussLegendre(thetaOrder, 0.0, theta);
    double numerical = nodes.Select((x, j) => weights[j] * RadialDensity(x)).Sum();
    cdfMaxError = Math.Max(cdfMaxError, Math.Abs(numerical - RadialCdf(theta)));
}

const int characterMaxL = 4;
var characterMeans = new double[characterMaxL + 1];
var characterGram = new double[characterMaxL + 1][];
for (int l = 0; l <= characterMaxL; l++)
{
    characterMeans[l] = Weighted(thetaNodes, thetaWeights, theta => Character(l, theta));
    characterGram[l] = new double[characterMaxL + 1];
    for (int m = 0; m <= characterMaxL; m++)
        characterGram[l][m] = Weighted(thetaNodes, thetaWeights, theta => Character(l, theta) * Character(m, theta));
}
double characterMeanMaxError = Enumerable.Range(0, characterMaxL + 1)
    .Max(l => Math.Abs(characterMeans[l] - (l == 0 ? 1.0 : 0.0)));
double characterGramMaxError = Enumerable.Range(0, characterMaxL + 1)
    .SelectMany(l => Enumerable.Range(0, characterMaxL + 1).Select(m => Math.Abs(characterGram[l][m] - (l == m ? 1.0 : 0.0))))
    .Max();

var (zNodes, zWeights) = GaussLegendre(axisZOrder, -1.0, 1.0);
var left = QuaternionFromAxisAngle(Normalize(new Vec3(1.0, -2.0, 0.5)), 0.731);
var right = QuaternionFromAxisAngle(Normalize(new Vec3(-0.25, 0.75, 1.0)), 1.137);
var baseMoments = new MatrixMoments();
var leftMoments = new MatrixMoments();
var rightMoments = new MatrixMoments();

for (int ti = 0; ti < thetaNodes.Length; ti++)
{
    double theta = thetaNodes[ti];
    for (int zi = 0; zi < zNodes.Length; zi++)
    {
        double z = zNodes[zi];
        double rho = Math.Sqrt(Math.Max(0.0, 1.0 - z * z));
        for (int pi = 0; pi < axisPhiCount; pi++)
        {
            double phi = 2.0 * Math.PI * pi / axisPhiCount;
            var axis = new Vec3(rho * Math.Cos(phi), rho * Math.Sin(phi), z);
            var q = QuaternionFromAxisAngle(axis, theta);
            double weight = thetaWeights[ti] * (zWeights[zi] / 2.0) / axisPhiCount;
            baseMoments.Add(RotationMatrix(q), weight);
            leftMoments.Add(RotationMatrix(Multiply(left, q)), weight);
            rightMoments.Add(RotationMatrix(Multiply(q, right)), weight);
        }
    }
}

var baseErrors = baseMoments.Errors();
var leftErrors = leftMoments.Errors();
var rightErrors = rightMoments.Errors();
double leftFirstDifference = MaxDifference(baseMoments.First, leftMoments.First);
double rightFirstDifference = MaxDifference(baseMoments.First, rightMoments.First);
double leftSecondDifference = MaxTensorDifference(baseMoments.Second, leftMoments.Second);
double rightSecondDifference = MaxTensorDifference(baseMoments.Second, rightMoments.Second);

var identityA = RotationMatrix(QuaternionFromAxisAngle(new Vec3(1.0, 0.0, 0.0), 0.0));
var identityB = RotationMatrix(QuaternionFromAxisAngle(Normalize(new Vec3(-2.0, 5.0, 7.0)), 0.0));
var piAxis = Normalize(new Vec3(2.0, -1.0, 3.0));
var piQ = QuaternionFromAxisAngle(piAxis, Math.PI);
var piMinusAxisQ = QuaternionFromAxisAngle(-piAxis, Math.PI);
var piMatrix = RotationMatrix(piQ);
var piMinusAxisMatrix = RotationMatrix(piMinusAxisQ);
var negativeQuaternionMatrix = RotationMatrix(-piQ);
double identityMaxError = Math.Max(MaxDifference(identityA, IdentityMatrix()), MaxDifference(identityA, identityB));
double piTraceError = Math.Abs(Trace(piMatrix) + 1.0);
double piDeterminantError = Math.Abs(Determinant(piMatrix) - 1.0);
double piOrthogonalityError = OrthogonalityError(piMatrix);
double piRepresentativeError = Math.Max(MaxDifference(piMatrix, piMinusAxisMatrix), MaxDifference(piMatrix, negativeQuaternionMatrix));
double cdfBoundaryError = Math.Max(Math.Abs(RadialCdf(0.0)), Math.Abs(RadialCdf(Math.PI) - 1.0));
double densityBoundaryError = Math.Max(Math.Abs(RadialDensity(0.0)), Math.Abs(RadialDensity(Math.PI) - 2.0 / Math.PI));

bool radialBattery = Math.Abs(normalization - 1.0) <= radialTolerance
    && cdfMaxError <= radialTolerance
    && Math.Abs(numericalMeanTheta - analyticMeanTheta) <= radialTolerance
    && Math.Abs(numericalMeanThetaSquared - analyticMeanThetaSquared) <= radialTolerance
    && Math.Abs(numericalMeanCosTheta + 0.5) <= radialTolerance
    && Math.Abs(numericalMeanCosThetaSquared - 0.5) <= radialTolerance;
bool characterBattery = characterMeanMaxError <= characterTolerance && characterGramMaxError <= characterTolerance;
bool matrixMomentBattery = baseErrors.FirstMaxError <= matrixTolerance && baseErrors.SecondMaxError <= matrixTolerance;
bool compositionBattery = leftErrors.FirstMaxError <= compositionTolerance
    && leftErrors.SecondMaxError <= compositionTolerance
    && rightErrors.FirstMaxError <= compositionTolerance
    && rightErrors.SecondMaxError <= compositionTolerance
    && leftFirstDifference <= compositionTolerance
    && rightFirstDifference <= compositionTolerance
    && leftSecondDifference <= compositionTolerance
    && rightSecondDifference <= compositionTolerance;
bool boundaryBattery = identityMaxError <= boundaryTolerance
    && piTraceError <= boundaryTolerance
    && piDeterminantError <= boundaryTolerance
    && piOrthogonalityError <= boundaryTolerance
    && piRepresentativeError <= boundaryTolerance
    && cdfBoundaryError <= boundaryTolerance
    && densityBoundaryError <= boundaryTolerance;
bool allBatteriesPassed = radialBattery && characterBattery && matrixMomentBattery && compositionBattery && boundaryBattery;
string terminalStatus = allBatteriesPassed ? passedTerminal : failedTerminal;

var result = new
{
    phaseId = "phase487-independent-so3-haar-measure-control",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    allowedTerminalStatuses = new[] { passedTerminal, failedTerminal },
    verdictKind = allBatteriesPassed ? "all-independent-haar-batteries-passed" : "independent-haar-battery-failed",
    applicationSubjectKind = "independent-so3-haar-measure-control",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A6; CONVENTION_ROBUSTNESS_TRANCHE_PLAN_2026-07-15 Phase487",
    lane = "exploration",
    explorationOnly = true,
    retrospectiveConfirmationRelabelingAllowed = false,
    independentOfPhase450Utilities = true,
    phase450ProjectReferenceCount = 0,
    deterministic = true,
    quadrature = new { thetaOrder, axisZOrder, axisPhiCount, totalSo3Nodes = thetaOrder * axisZOrder * axisPhiCount },
    analyticRadialLaw = new
    {
        density = "p(theta)=(1-cos(theta))/pi, 0<=theta<=pi",
        cdf = "F(theta)=(theta-sin(theta))/pi",
        normalization,
        cdfMaxError,
        moments = new
        {
            numericalMeanTheta,
            analyticMeanTheta,
            numericalMeanThetaSquared,
            analyticMeanThetaSquared,
            numericalMeanCosTheta,
            analyticMeanCosTheta = -0.5,
            numericalMeanCosThetaSquared,
            analyticMeanCosThetaSquared = 0.5,
        },
        tolerance = radialTolerance,
        passed = radialBattery,
    },
    characters = new
    {
        convention = "chi_l(theta)=1+2*sum_{k=1}^l cos(k theta)",
        characterMaxL,
        means = characterMeans,
        gram = characterGram,
        meanMaxError = characterMeanMaxError,
        gramMaxError = characterGramMaxError,
        tolerance = characterTolerance,
        passed = characterBattery,
    },
    matrixMoments = new
    {
        analyticFirstMoment = "E[R_ij]=0",
        analyticSecondMoment = "E[R_ij R_kl]=delta_ik delta_jl/3",
        firstMomentMaxError = baseErrors.FirstMaxError,
        secondMomentMaxError = baseErrors.SecondMaxError,
        tolerance = matrixTolerance,
        passed = matrixMomentBattery,
    },
    compositionInvariance = new
    {
        fixedLeftQuaternion = left,
        fixedRightQuaternion = right,
        leftFirstMomentMaxError = leftErrors.FirstMaxError,
        leftSecondMomentMaxError = leftErrors.SecondMaxError,
        rightFirstMomentMaxError = rightErrors.FirstMaxError,
        rightSecondMomentMaxError = rightErrors.SecondMaxError,
        leftFirstDifference,
        leftSecondDifference,
        rightFirstDifference,
        rightSecondDifference,
        tolerance = compositionTolerance,
        passed = compositionBattery,
    },
    boundaryCases = new
    {
        identityMaxError,
        piTraceError,
        piDeterminantError,
        piOrthogonalityError,
        piAxisAndQuaternionRepresentativeMaxError = piRepresentativeError,
        cdfBoundaryError,
        densityBoundaryError,
        tolerance = boundaryTolerance,
        passed = boundaryBattery,
    },
    allBatteriesPassed,
    failedBatteryCount = new[] { radialBattery, characterBattery, matrixMomentBattery, compositionBattery, boundaryBattery }.Count(x => !x),
    humanRulingAuthored = false,
    o4Discharged = false,
    satisfiesPhase458G4 = false,
    satisfiesPhase458G5 = false,
    phase458EvaluationAuthorized = false,
    binderLaunchAuthorized = false,
    productionAuthorized = false,
    sourceContractApplicationAllowed = false,
    canFillPhase201WzContract = false,
    canFillPhase201HiggsContract = false,
    canFillPhase256ObservedFieldExtractionContract = false,
    routePromotesWzMasses = false,
    routePromotesHiggsMass = false,
    routeCompletesBosonPredictions = false,
    targetBlindConstruction = true,
    physicalTargetsConsultedForConstruction = false,
    noGevPromotion = true,
    promotedPhysicalMassClaimCount = 0,
    decision = allBatteriesPassed
        ? "The independent deterministic SO(3) Haar theorem/quadrature batteries pass. This is exploration-only robustness evidence; it does not author a human ruling, discharge O4, satisfy Phase458, or authorize production."
        : "At least one independent deterministic SO(3) Haar theorem/quadrature battery failed. The phase fails closed and provides no downstream authorization.",
    runtimeSeconds = stopwatch.Elapsed.TotalSeconds,
};

Directory.CreateDirectory(outputDir);
var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var json = JsonSerializer.Serialize(result, options);
File.WriteAllText(Path.Combine(outputDir, "independent_so3_haar_measure_control.json"), json);
File.WriteAllText(Path.Combine(outputDir, "independent_so3_haar_measure_control_summary.json"), json);

Require(terminalStatus is passedTerminal or failedTerminal, "terminal taxonomy drifted");
Require(result.explorationOnly, "exploration-only firewall failed");
Require(!result.humanRulingAuthored && !result.o4Discharged, "human/O4 firewall failed");
Require(!result.phase458EvaluationAuthorized && !result.productionAuthorized, "launch firewall failed");
Require(!result.sourceContractApplicationAllowed && !result.canFillPhase201WzContract && !result.canFillPhase201HiggsContract && !result.canFillPhase256ObservedFieldExtractionContract, "source-contract firewall failed");
Require(result.noGevPromotion && result.promotedPhysicalMassClaimCount == 0, "claim firewall failed");
Require(allBatteriesPassed, terminalStatus);
Console.WriteLine(terminalStatus);

static double RadialDensity(double theta) => (1.0 - Math.Cos(theta)) / Math.PI;

static double RadialCdf(double theta) => (theta - Math.Sin(theta)) / Math.PI;

static double Character(int l, double theta)
{
    double value = 1.0;
    for (int k = 1; k <= l; k++) value += 2.0 * Math.Cos(k * theta);
    return value;
}

static double Weighted(double[] nodes, double[] weights, Func<double, double> function)
    => nodes.Select((node, i) => weights[i] * function(node)).Sum();

static (double[] Nodes, double[] Weights) GaussLegendre(int order, double lower, double upper)
{
    var nodes = new double[order];
    var weights = new double[order];
    int half = (order + 1) / 2;
    double midpoint = (lower + upper) / 2.0;
    double halfWidth = (upper - lower) / 2.0;
    for (int i = 0; i < half; i++)
    {
        double x = Math.Cos(Math.PI * (i + 0.75) / (order + 0.5));
        double derivative = 0.0;
        for (int iteration = 0; iteration < 64; iteration++)
        {
            (double polynomial, derivative) = Legendre(order, x);
            double next = x - polynomial / derivative;
            if (Math.Abs(next - x) <= 4.0 * double.Epsilon * Math.Max(1.0, Math.Abs(x)))
            {
                x = next;
                break;
            }
            x = next;
        }
        (_, derivative) = Legendre(order, x);
        double weight = 2.0 / ((1.0 - x * x) * derivative * derivative);
        nodes[i] = midpoint - halfWidth * x;
        nodes[order - 1 - i] = midpoint + halfWidth * x;
        weights[i] = halfWidth * weight;
        weights[order - 1 - i] = halfWidth * weight;
    }
    return (nodes, weights);
}

static (double Value, double Derivative) Legendre(int order, double x)
{
    double p0 = 1.0;
    double p1 = x;
    if (order == 0) return (p0, 0.0);
    for (int n = 2; n <= order; n++)
    {
        double pn = ((2.0 * n - 1.0) * x * p1 - (n - 1.0) * p0) / n;
        p0 = p1;
        p1 = pn;
    }
    double derivative = order * (x * p1 - p0) / (x * x - 1.0);
    return (p1, derivative);
}

static Vec3 Normalize(Vec3 vector)
{
    double norm = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
    Require(norm > 0.0, "axis must be nonzero");
    return new Vec3(vector.X / norm, vector.Y / norm, vector.Z / norm);
}

static Quat QuaternionFromAxisAngle(Vec3 unitAxis, double theta)
{
    double half = theta / 2.0;
    double sine = Math.Sin(half);
    return new Quat(Math.Cos(half), sine * unitAxis.X, sine * unitAxis.Y, sine * unitAxis.Z);
}

static Quat Multiply(Quat a, Quat b) => new(
    a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
    a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
    a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
    a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W);

static double[,] RotationMatrix(Quat q)
{
    var conjugate = new Quat(q.W, -q.X, -q.Y, -q.Z);
    var matrix = new double[3, 3];
    for (int column = 0; column < 3; column++)
    {
        var basis = column switch
        {
            0 => new Quat(0.0, 1.0, 0.0, 0.0),
            1 => new Quat(0.0, 0.0, 1.0, 0.0),
            _ => new Quat(0.0, 0.0, 0.0, 1.0),
        };
        var rotated = Multiply(Multiply(q, basis), conjugate);
        matrix[0, column] = rotated.X;
        matrix[1, column] = rotated.Y;
        matrix[2, column] = rotated.Z;
    }
    return matrix;
}

static double[,] IdentityMatrix() => new double[,] { { 1.0, 0.0, 0.0 }, { 0.0, 1.0, 0.0 }, { 0.0, 0.0, 1.0 } };

static double MaxDifference(double[,] a, double[,] b)
{
    double maximum = 0.0;
    for (int i = 0; i < a.GetLength(0); i++)
    for (int j = 0; j < a.GetLength(1); j++) maximum = Math.Max(maximum, Math.Abs(a[i, j] - b[i, j]));
    return maximum;
}

static double MaxTensorDifference(double[,,,] a, double[,,,] b)
{
    double maximum = 0.0;
    for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++)
    for (int k = 0; k < 3; k++)
    for (int l = 0; l < 3; l++) maximum = Math.Max(maximum, Math.Abs(a[i, j, k, l] - b[i, j, k, l]));
    return maximum;
}

static double Trace(double[,] matrix) => matrix[0, 0] + matrix[1, 1] + matrix[2, 2];

static double Determinant(double[,] m) =>
    m[0, 0] * (m[1, 1] * m[2, 2] - m[1, 2] * m[2, 1])
    - m[0, 1] * (m[1, 0] * m[2, 2] - m[1, 2] * m[2, 0])
    + m[0, 2] * (m[1, 0] * m[2, 1] - m[1, 1] * m[2, 0]);

static double OrthogonalityError(double[,] matrix)
{
    double maximum = 0.0;
    for (int i = 0; i < 3; i++)
    for (int j = 0; j < 3; j++)
    {
        double product = 0.0;
        for (int k = 0; k < 3; k++) product += matrix[k, i] * matrix[k, j];
        maximum = Math.Max(maximum, Math.Abs(product - (i == j ? 1.0 : 0.0)));
    }
    return maximum;
}

static void Require(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}

readonly struct Vec3
{
    public Vec3(double x, double y, double z) => (X, Y, Z) = (x, y, z);
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public static Vec3 operator -(Vec3 value) => new(-value.X, -value.Y, -value.Z);
}

readonly struct Quat
{
    public Quat(double w, double x, double y, double z) => (W, X, Y, Z) = (w, x, y, z);
    public double W { get; }
    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public static Quat operator -(Quat value) => new(-value.W, -value.X, -value.Y, -value.Z);
}

sealed class MatrixMoments
{
    public double[,] First { get; } = new double[3, 3];
    public double[,,,] Second { get; } = new double[3, 3, 3, 3];

    public void Add(double[,] matrix, double weight)
    {
        for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
        {
            First[i, j] += weight * matrix[i, j];
            for (int k = 0; k < 3; k++)
            for (int l = 0; l < 3; l++) Second[i, j, k, l] += weight * matrix[i, j] * matrix[k, l];
        }
    }

    public (double FirstMaxError, double SecondMaxError) Errors()
    {
        double first = 0.0;
        double second = 0.0;
        for (int i = 0; i < 3; i++)
        for (int j = 0; j < 3; j++)
        {
            first = Math.Max(first, Math.Abs(First[i, j]));
            for (int k = 0; k < 3; k++)
            for (int l = 0; l < 3; l++)
            {
                double expected = i == k && j == l ? 1.0 / 3.0 : 0.0;
                second = Math.Max(second, Math.Abs(Second[i, j, k, l] - expected));
            }
        }
        return (first, second);
    }
}
