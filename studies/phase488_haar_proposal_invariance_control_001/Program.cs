using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

// Phase488: prospectively frozen exploration-only SO(3) Haar proposal control.
// All seeds, sample counts, observables, bins, and tolerances appear below before sampling.

const int SampleCount = 200_000;
const int DetailedBalanceBins = 8;
const int InversionBins = 10;
const double MeanTraceTolerance = 0.012;
const double MeanTraceSquaredTolerance = 0.025;
const double MeanCosThetaTolerance = 0.010;
const double MeanCosThetaSquaredTolerance = 0.010;
const double MatrixMeanTolerance = 0.010;
const double MatrixSquaredTolerance = 0.010;
const double OccupancyTolerance = 0.006;
const double MaxInversionZ = 5.75;
const double MaxDetailedBalanceZ = 5.75;
const string Phase487SummaryPath = "studies/phase487_independent_so3_haar_measure_control_001/output/independent_so3_haar_measure_control_summary.json";

using var phase487 = JsonDocument.Parse(File.ReadAllText(Phase487SummaryPath));
bool phase487PrecursorPassed = phase487.RootElement.GetProperty("terminalStatus").GetString() ==
        "independent-so3-haar-measure-control-passed-all-batteries-exploration-only"
    && phase487.RootElement.GetProperty("allBatteriesPassed").GetBoolean()
    && phase487.RootElement.GetProperty("independentOfPhase450Utilities").GetBoolean()
    && !phase487.RootElement.GetProperty("o4Discharged").GetBoolean()
    && phase487.RootElement.GetProperty("promotedPhysicalMassClaimCount").GetInt32() == 0;

var familySpecs = new[]
{
    new FamilySpec("uniform-quaternion-independence", 2026071501, FamilyKind.QuaternionIndependence),
    new FamilySpec("left-composition", 2026071502, FamilyKind.LeftComposition),
    new FamilySpec("right-composition", 2026071503, FamilyKind.RightComposition),
    new FamilySpec("direct-axis-angle-radial-law", 2026071504, FamilyKind.DirectAxisAngle),
};

var fixedLeft = QuaternionD.FromAxisAngle(1.0, 2.0, -1.0, 0.731);
var fixedRight = QuaternionD.FromAxisAngle(-2.0, 1.0, 3.0, 1.137);
var familyResults = familySpecs.Select(spec => EvaluateFamily(spec, fixedLeft, fixedRight)).ToArray();

var failures = familyResults.SelectMany(result => result.Failures.Select(failure => new
{
    result.Family,
    failure.Category,
    failure.Message,
})).ToArray();
bool passed = phase487PrecursorPassed && failures.Length == 0;
string verdictKind = !phase487PrecursorPassed ? "independent-measure-precursor-inadmissible"
    : passed ? "all-four-families-within-frozen-tolerances"
    : "one-or-more-families-outside-frozen-tolerances";
string terminalStatus = !phase487PrecursorPassed
    ? "haar-proposal-invariance-control-failed-closed-precursor-inadmissible"
    : "haar-proposal-invariance-control-" + (passed ? "passed-exploration-only" : "failed-closed-negative-control");

string contractText = string.Join("|",
    SampleCount, DetailedBalanceBins, InversionBins,
    MeanTraceTolerance, MeanTraceSquaredTolerance, MeanCosThetaTolerance,
    MeanCosThetaSquaredTolerance, MatrixMeanTolerance, MatrixSquaredTolerance,
    OccupancyTolerance, MaxInversionZ, MaxDetailedBalanceZ,
    string.Join(";", familySpecs.Select(x => $"{x.Name}:{x.Seed}:{x.Kind}")));
string frozenContractSha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(contractText))).ToLowerInvariant();

var result = new
{
    phaseId = "phase488-haar-proposal-invariance-control",
    generatedAt = DateTimeOffset.UtcNow,
    terminalStatus,
    verdictKind,
    applicationSubjectKind = "haar-proposal-invariance-control",
    planSection = "WAVE2_AMENDMENTS_2026-07-12 A6; CONVENTION_ROBUSTNESS_TRANCHE_PLAN_2026-07-15 Phase488",
    phase487SummaryPath = Phase487SummaryPath,
    phase487PrecursorPassed,
    proposalInvarianceControlPassed = passed,
    explorationOnly = true,
    prospectivelyFrozen = true,
    frozenContractSha256,
    configuration = new
    {
        sampleCount = SampleCount,
        detailedBalanceBins = DetailedBalanceBins,
        detailedBalanceBinDefinition = "eight equal-probability bins under F(theta)=(theta-sin(theta))/pi",
        inversionBins = InversionBins,
        targetMeasure = "normalized SO(3) Haar measure; zero action",
        acceptanceRule = "all proposals accepted under the zero-action Haar target",
        analyticMoments = new { meanTrace = 0.0, meanTraceSquared = 1.0, meanCosTheta = -0.5, meanCosThetaSquared = 0.5, matrixEntryMean = 0.0, matrixEntrySquared = 1.0 / 3.0 },
        tolerances = new
        {
            meanTrace = MeanTraceTolerance,
            meanTraceSquared = MeanTraceSquaredTolerance,
            meanCosTheta = MeanCosThetaTolerance,
            meanCosThetaSquared = MeanCosThetaSquaredTolerance,
            matrixEntryMean = MatrixMeanTolerance,
            matrixEntrySquared = MatrixSquaredTolerance,
            equalHaarBinOccupancy = OccupancyTolerance,
            maxInversionZ = MaxInversionZ,
            maxDetailedBalanceZ = MaxDetailedBalanceZ,
        },
        families = familySpecs.Select(x => new { x.Name, x.Seed, kind = x.Kind.ToString() }).ToArray(),
    },
    familyResults,
    negativeTaxonomy = new[]
    {
        "independent-measure-precursor-inadmissible",
        "analytic-stationary-moment-failure",
        "proposal-inversion-symmetry-failure",
        "left-invariance-failure",
        "right-invariance-failure",
        "acceptance-or-stationarity-failure",
        "binned-detailed-balance-failure",
    },
    failures,
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
    decision = passed
        ? "All four independently constructed proposal/sampling families satisfy the prospectively frozen reduced Haar controls. This is exploration-lane evidence only and authorizes no ruling, gate evaluation, production run, or physical-unit claim."
        : "At least one independently constructed proposal/sampling family violates a prospectively frozen control. The result fails closed and the named negative categories are preserved; no downstream use is authorized.",
};

string outputDirectory = "studies/phase488_haar_proposal_invariance_control_001/output";
Directory.CreateDirectory(outputDirectory);
var jsonOptions = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
string json = JsonSerializer.Serialize(result, jsonOptions);
File.WriteAllText(Path.Combine(outputDirectory, "haar_proposal_invariance_control.json"), json);
File.WriteAllText(Path.Combine(outputDirectory, "haar_proposal_invariance_control_summary.json"), json);

Console.WriteLine(terminalStatus);
foreach (var family in familyResults)
    Console.WriteLine($"{family.Family}: pass={family.Passed} dbZ={family.MaxDetailedBalanceZ:F3} invZ={family.MaxInversionZ:F3}");

if (!passed)
    throw new InvalidOperationException(!phase487PrecursorPassed
        ? "Phase488 fail-closed: Phase487 independent measure precursor is not admissible."
        : "Phase488 fail-closed: " + string.Join(" | ", failures.Select(x => $"{x.Family}/{x.Category}: {x.Message}")));

static FamilyResult EvaluateFamily(FamilySpec spec, QuaternionD fixedLeft, QuaternionD fixedRight)
{
    var rng = new Random(spec.Seed);
    var sums = new MomentSums();
    var leftSums = new MomentSums();
    var rightSums = new MomentSums();
    var inversion = new InversionSums(InversionBins);
    var occupancy = new int[DetailedBalanceBins];
    var transitions = new int[DetailedBalanceBins, DetailedBalanceBins];
    QuaternionD state = QuaternionD.Identity;
    int previousBin = -1;
    int accepted = 0;

    for (int sample = 0; sample < SampleCount; sample++)
    {
        QuaternionD proposal;
        Matrix3 rotation;
        switch (spec.Kind)
        {
            case FamilyKind.QuaternionIndependence:
                proposal = SampleShoemakeQuaternion(rng);
                state = proposal;
                rotation = state.ToMatrix();
                break;
            case FamilyKind.LeftComposition:
                proposal = SampleNormalizedGaussianQuaternion(rng);
                state = (proposal * state).Canonicalized();
                rotation = state.ToMatrix();
                break;
            case FamilyKind.RightComposition:
                proposal = SampleRadialAxisAngleQuaternion(rng);
                state = (state * proposal).Canonicalized();
                rotation = state.ToMatrix();
                break;
            case FamilyKind.DirectAxisAngle:
                (rotation, proposal) = SampleDirectAxisAngleMatrix(rng);
                state = proposal;
                break;
            default:
                throw new InvalidOperationException("Unknown proposal family.");
        }

        accepted++;
        sums.Add(rotation);
        leftSums.Add((fixedLeft * state).ToMatrix());
        rightSums.Add((state * fixedRight).ToMatrix());
        inversion.Add(proposal);

        int bin = HaarAngleBin(rotation.Angle, DetailedBalanceBins);
        occupancy[bin]++;
        if (previousBin >= 0) transitions[previousBin, bin]++;
        previousBin = bin;
    }

    var failures = new List<Failure>();
    MomentMetrics moments = sums.Metrics(SampleCount);
    MomentMetrics leftMoments = leftSums.Metrics(SampleCount);
    MomentMetrics rightMoments = rightSums.Metrics(SampleCount);
    double occupancyMaxError = occupancy.Max(count => Math.Abs((double)count / SampleCount - 1.0 / DetailedBalanceBins));
    double inversionMaxZ = inversion.MaxSymmetryZ;
    double detailedBalanceMaxZ = DetailedBalanceMaxZ(transitions);
    double acceptance = (double)accepted / SampleCount;

    CheckMoments(moments, "analytic-stationary-moment-failure", failures);
    CheckMoments(leftMoments, "left-invariance-failure", failures);
    CheckMoments(rightMoments, "right-invariance-failure", failures);
    if (inversionMaxZ > MaxInversionZ)
        failures.Add(new Failure("proposal-inversion-symmetry-failure", $"max mirrored-bin z={inversionMaxZ:R} exceeds {MaxInversionZ:R}"));
    if (acceptance != 1.0 || occupancyMaxError > OccupancyTolerance)
        failures.Add(new Failure("acceptance-or-stationarity-failure", $"acceptance={acceptance:R}; max equal-Haar-bin occupancy error={occupancyMaxError:R}"));
    if (detailedBalanceMaxZ > MaxDetailedBalanceZ)
        failures.Add(new Failure("binned-detailed-balance-failure", $"max flux-asymmetry z={detailedBalanceMaxZ:R} exceeds {MaxDetailedBalanceZ:R}"));

    return new FamilyResult(
        spec.Name, spec.Seed, SampleCount, acceptance, moments, leftMoments, rightMoments,
        occupancy, occupancyMaxError, inversionMaxZ, detailedBalanceMaxZ,
        failures.Count == 0, failures.ToArray());
}

static void CheckMoments(MomentMetrics m, string category, List<Failure> failures)
{
    var violations = new List<string>();
    if (Math.Abs(m.MeanTrace) > MeanTraceTolerance) violations.Add($"meanTrace={m.MeanTrace:R}");
    if (Math.Abs(m.MeanTraceSquared - 1.0) > MeanTraceSquaredTolerance) violations.Add($"meanTraceSquared={m.MeanTraceSquared:R}");
    if (Math.Abs(m.MeanCosTheta + 0.5) > MeanCosThetaTolerance) violations.Add($"meanCosTheta={m.MeanCosTheta:R}");
    if (Math.Abs(m.MeanCosThetaSquared - 0.5) > MeanCosThetaSquaredTolerance) violations.Add($"meanCosThetaSquared={m.MeanCosThetaSquared:R}");
    if (m.MaxAbsMatrixMean > MatrixMeanTolerance) violations.Add($"maxAbsMatrixMean={m.MaxAbsMatrixMean:R}");
    if (m.MaxAbsMatrixSquaredError > MatrixSquaredTolerance) violations.Add($"maxAbsMatrixSquaredError={m.MaxAbsMatrixSquaredError:R}");
    if (violations.Count != 0) failures.Add(new Failure(category, string.Join(", ", violations)));
}

static int HaarAngleBin(double theta, int bins)
{
    double cdf = (theta - Math.Sin(theta)) / Math.PI;
    return Math.Min(bins - 1, (int)(cdf * bins));
}

static double DetailedBalanceMaxZ(int[,] transitions)
{
    double max = 0.0;
    for (int i = 0; i < transitions.GetLength(0); i++)
        for (int j = i + 1; j < transitions.GetLength(1); j++)
        {
            double total = transitions[i, j] + transitions[j, i];
            if (total > 0.0)
                max = Math.Max(max, Math.Abs(transitions[i, j] - transitions[j, i]) / Math.Sqrt(total));
        }
    return max;
}

static QuaternionD SampleShoemakeQuaternion(Random rng)
{
    double u1 = rng.NextDouble();
    double u2 = rng.NextDouble();
    double u3 = rng.NextDouble();
    double a = Math.Sqrt(1.0 - u1);
    double b = Math.Sqrt(u1);
    return new QuaternionD(
        b * Math.Cos(2.0 * Math.PI * u3),
        a * Math.Sin(2.0 * Math.PI * u2),
        a * Math.Cos(2.0 * Math.PI * u2),
        b * Math.Sin(2.0 * Math.PI * u3)).Canonicalized();
}

static QuaternionD SampleNormalizedGaussianQuaternion(Random rng)
{
    double w = Gaussian(rng), x = Gaussian(rng), y = Gaussian(rng), z = Gaussian(rng);
    double inv = 1.0 / Math.Sqrt(w * w + x * x + y * y + z * z);
    return new QuaternionD(w * inv, x * inv, y * inv, z * inv).Canonicalized();
}

static QuaternionD SampleRadialAxisAngleQuaternion(Random rng)
{
    double theta = InvertHaarRadialCdf(rng.NextDouble());
    double z = 2.0 * rng.NextDouble() - 1.0;
    double phi = 2.0 * Math.PI * rng.NextDouble();
    double radial = Math.Sqrt(Math.Max(0.0, 1.0 - z * z));
    double halfSin = Math.Sin(0.5 * theta);
    return new QuaternionD(Math.Cos(0.5 * theta), halfSin * radial * Math.Cos(phi), halfSin * radial * Math.Sin(phi), halfSin * z);
}

static (Matrix3 Matrix, QuaternionD Quaternion) SampleDirectAxisAngleMatrix(Random rng)
{
    double theta = InvertHaarRadialCdf(rng.NextDouble());
    double axisZ = 2.0 * rng.NextDouble() - 1.0;
    double azimuth = 2.0 * Math.PI * rng.NextDouble();
    double axisRadius = Math.Sqrt(Math.Max(0.0, 1.0 - axisZ * axisZ));
    double x = axisRadius * Math.Cos(azimuth);
    double y = axisRadius * Math.Sin(azimuth);
    var matrix = Matrix3.Rodrigues(x, y, axisZ, theta);
    double s = Math.Sin(theta / 2.0);
    return (matrix, new QuaternionD(Math.Cos(theta / 2.0), s * x, s * y, s * axisZ));
}

static double InvertHaarRadialCdf(double u)
{
    double lower = 0.0, upper = Math.PI;
    for (int iteration = 0; iteration < 60; iteration++)
    {
        double middle = 0.5 * (lower + upper);
        double cdf = (middle - Math.Sin(middle)) / Math.PI;
        if (cdf < u) lower = middle; else upper = middle;
    }
    return 0.5 * (lower + upper);
}

static double Gaussian(Random rng)
{
    double u1 = Math.Max(rng.NextDouble(), double.Epsilon);
    return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * rng.NextDouble());
}

enum FamilyKind { QuaternionIndependence, LeftComposition, RightComposition, DirectAxisAngle }

sealed class FamilySpec(string name, int seed, FamilyKind kind)
{
    public string Name { get; } = name;
    public int Seed { get; } = seed;
    public FamilyKind Kind { get; } = kind;
}

sealed class Failure(string category, string message)
{
    public string Category { get; } = category;
    public string Message { get; } = message;
}

sealed class FamilyResult(
    string family, int seed, int sampleCount, double acceptance, MomentMetrics moments,
    MomentMetrics leftComposedMoments, MomentMetrics rightComposedMoments, int[] angleBinOccupancy,
    double maxOccupancyError, double maxInversionZ, double maxDetailedBalanceZ, bool passed, Failure[] failures)
{
    public string Family { get; } = family;
    public int Seed { get; } = seed;
    public int SampleCount { get; } = sampleCount;
    public double Acceptance { get; } = acceptance;
    public MomentMetrics Moments { get; } = moments;
    public MomentMetrics LeftComposedMoments { get; } = leftComposedMoments;
    public MomentMetrics RightComposedMoments { get; } = rightComposedMoments;
    public int[] AngleBinOccupancy { get; } = angleBinOccupancy;
    public double MaxOccupancyError { get; } = maxOccupancyError;
    public double MaxInversionZ { get; } = maxInversionZ;
    public double MaxDetailedBalanceZ { get; } = maxDetailedBalanceZ;
    public bool Passed { get; } = passed;
    public Failure[] Failures { get; } = failures;
}

sealed class MomentMetrics(double meanTrace, double meanTraceSquared, double meanCosTheta, double meanCosThetaSquared, double maxAbsMatrixMean, double maxAbsMatrixSquaredError)
{
    public double MeanTrace { get; } = meanTrace;
    public double MeanTraceSquared { get; } = meanTraceSquared;
    public double MeanCosTheta { get; } = meanCosTheta;
    public double MeanCosThetaSquared { get; } = meanCosThetaSquared;
    public double MaxAbsMatrixMean { get; } = maxAbsMatrixMean;
    public double MaxAbsMatrixSquaredError { get; } = maxAbsMatrixSquaredError;
}

sealed class MomentSums
{
    private double _trace, _traceSquared, _cosTheta, _cosThetaSquared;
    private readonly double[] _entries = new double[9];
    private readonly double[] _entriesSquared = new double[9];

    public void Add(Matrix3 matrix)
    {
        double trace = matrix.Trace;
        double cosTheta = Math.Clamp((trace - 1.0) / 2.0, -1.0, 1.0);
        _trace += trace;
        _traceSquared += trace * trace;
        _cosTheta += cosTheta;
        _cosThetaSquared += cosTheta * cosTheta;
        for (int i = 0; i < 9; i++)
        {
            double value = matrix[i];
            _entries[i] += value;
            _entriesSquared[i] += value * value;
        }
    }

    public MomentMetrics Metrics(int count) => new(
        _trace / count,
        _traceSquared / count,
        _cosTheta / count,
        _cosThetaSquared / count,
        _entries.Max(x => Math.Abs(x / count)),
        _entriesSquared.Max(x => Math.Abs(x / count - 1.0 / 3.0)));
}

sealed class InversionSums
{
    private readonly int[,] _histograms;
    private readonly int _bins;

    public InversionSums(int bins)
    {
        _bins = bins;
        _histograms = new int[3, bins];
    }

    public void Add(QuaternionD q)
    {
        double[] values = { q.X, q.Y, q.Z };
        for (int component = 0; component < 3; component++)
        {
            int bin = Math.Min(_bins - 1, (int)((Math.Clamp(values[component], -1.0, 1.0) + 1.0) * 0.5 * _bins));
            _histograms[component, bin]++;
        }
    }

    public double MaxSymmetryZ
    {
        get
        {
            double max = 0.0;
            for (int component = 0; component < 3; component++)
                for (int bin = 0; bin < _bins / 2; bin++)
                {
                    int inverse = _bins - 1 - bin;
                    double total = _histograms[component, bin] + _histograms[component, inverse];
                    if (total > 0.0)
                        max = Math.Max(max, Math.Abs(_histograms[component, bin] - _histograms[component, inverse]) / Math.Sqrt(total));
                }
            return max;
        }
    }
}

readonly struct QuaternionD(double w, double x, double y, double z)
{
    public static QuaternionD Identity => new(1.0, 0.0, 0.0, 0.0);
    public double W { get; } = w;
    public double X { get; } = x;
    public double Y { get; } = y;
    public double Z { get; } = z;

    public QuaternionD Canonicalized() => W < 0.0 ? new(-W, -X, -Y, -Z) : this;

    public static QuaternionD operator *(QuaternionD a, QuaternionD b) => new(
        a.W * b.W - a.X * b.X - a.Y * b.Y - a.Z * b.Z,
        a.W * b.X + a.X * b.W + a.Y * b.Z - a.Z * b.Y,
        a.W * b.Y - a.X * b.Z + a.Y * b.W + a.Z * b.X,
        a.W * b.Z + a.X * b.Y - a.Y * b.X + a.Z * b.W);

    public Matrix3 ToMatrix()
    {
        double xx = X * X, yy = Y * Y, zz = Z * Z;
        double xy = X * Y, xz = X * Z, yz = Y * Z;
        double wx = W * X, wy = W * Y, wz = W * Z;
        return new Matrix3(new[]
        {
            1.0 - 2.0 * (yy + zz), 2.0 * (xy - wz), 2.0 * (xz + wy),
            2.0 * (xy + wz), 1.0 - 2.0 * (xx + zz), 2.0 * (yz - wx),
            2.0 * (xz - wy), 2.0 * (yz + wx), 1.0 - 2.0 * (xx + yy),
        });
    }

    public static QuaternionD FromAxisAngle(double x, double y, double z, double theta)
    {
        double inv = 1.0 / Math.Sqrt(x * x + y * y + z * z);
        double s = Math.Sin(theta / 2.0) * inv;
        return new QuaternionD(Math.Cos(theta / 2.0), s * x, s * y, s * z);
    }
}

readonly struct Matrix3
{
    private readonly double[] _values;
    public Matrix3(double[] values) => _values = values;
    public double this[int index] => _values[index];
    public double Trace => _values[0] + _values[4] + _values[8];
    public double Angle => Math.Acos(Math.Clamp((Trace - 1.0) / 2.0, -1.0, 1.0));

    public static Matrix3 Rodrigues(double x, double y, double z, double theta)
    {
        double c = Math.Cos(theta), s = Math.Sin(theta), d = 1.0 - c;
        return new Matrix3(new[]
        {
            c + x*x*d, x*y*d - z*s, x*z*d + y*s,
            y*x*d + z*s, c + y*y*d, y*z*d - x*s,
            z*x*d - y*s, z*y*d + x*s, c + z*z*d,
        });
    }
}
