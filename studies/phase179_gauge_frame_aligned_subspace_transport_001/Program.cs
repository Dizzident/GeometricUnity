using System.Text.Json;

const string DefaultOutputDir = "studies/phase179_gauge_frame_aligned_subspace_transport_001/output";
const string Phase174Path = "studies/phase174_protected_massless_subspace_audit_001/output/protected_massless_subspace_audit.json";
const string Phase178Path = "studies/phase178_protected_massless_subspace_transport_001/output/protected_massless_subspace_transport.json";
const string SignatureDir = "studies/phase12_joined_calculation_001/output/background_family/observables/mode_signatures";

var outputDir = Environment.GetEnvironmentVariable("PHASE179_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase174 = JsonDocument.Parse(File.ReadAllText(Phase174Path));
using var phase178 = File.Exists(Phase178Path) ? JsonDocument.Parse(File.ReadAllText(Phase178Path)) : null;

double rankTolerance = 1e-20;
double minSingularValueThreshold = 0.95;
double meanSingularValueThreshold = 0.99;
bool protectedSubspaceReady = JsonBool(phase174.RootElement, "subspaceDiagnosticReady") is true;
int? expectedDimension = JsonInt(phase174.RootElement, "protectedSubspaceDimension");
double? unalignedMinSingularValue = phase178 is null
    ? null
    : phase178.RootElement.TryGetProperty("pairwiseAudits", out var rawAudits) && rawAudits.ValueKind == JsonValueKind.Array
        ? rawAudits.EnumerateArray().Select(audit => JsonDouble(audit, "minSingularValue")).Where(value => value is not null).DefaultIfEmpty(null).Min()
        : null;

var modeRecords = Directory.EnumerateFiles(SignatureDir, "*.json")
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(LoadMode)
    .ToArray();
var grouped = modeRecords
    .GroupBy(mode => mode.BackgroundId, StringComparer.Ordinal)
    .OrderBy(group => group.Key, StringComparer.Ordinal)
    .Select(group => group.OrderBy(mode => mode.ModeIndex).ToArray())
    .ToArray();

if (grouped.Length != 2)
    throw new InvalidDataException($"Expected exactly two sibling backgrounds, found {grouped.Length}.");

var leftModes = grouped[0];
var rightModes = grouped[1];
if (leftModes.Length != rightModes.Length)
    throw new InvalidDataException("Sibling backgrounds have different mode counts.");

var rotation = ComputeGaugeRotation(sourceModes: rightModes, targetModes: leftModes);
var alignedRightModes = rightModes.Select(mode => ApplyGaugeRotation(mode, rotation)).ToArray();

var leftSubspace = BuildSubspace(leftModes[0].BackgroundId, leftModes, rankTolerance);
var rightRawSubspace = BuildSubspace(rightModes[0].BackgroundId, rightModes, rankTolerance);
var rightAlignedSubspace = BuildSubspace(rightModes[0].BackgroundId + "-gauge-frame-aligned", alignedRightModes, rankTolerance);
var rawAudit = CompareSubspaces(leftSubspace, rightRawSubspace);
var alignedAudit = CompareSubspaces(leftSubspace, rightAlignedSubspace);

bool dimensionStable = expectedDimension is not null
    && leftSubspace.Dimension == expectedDimension
    && rightAlignedSubspace.Dimension == expectedDimension;
bool alignedTransportStable = protectedSubspaceReady
    && dimensionStable
    && alignedAudit.MinSingularValue >= minSingularValueThreshold
    && alignedAudit.MeanSingularValue >= meanSingularValueThreshold;
string terminalStatus = alignedTransportStable
    ? "gauge-frame-aligned-subspace-transport-stable"
    : "gauge-frame-aligned-subspace-transport-not-stable";

var result = new
{
    phaseId = "phase179-gauge-frame-aligned-subspace-transport",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    rankTolerance,
    minSingularValueThreshold,
    meanSingularValueThreshold,
    protectedSubspaceReady,
    expectedDimension,
    dimensionStable,
    sourceBackgroundId = rightModes[0].BackgroundId,
    targetBackgroundId = leftModes[0].BackgroundId,
    gaugeFrameTransport = new
    {
        method = "orthogonal-procrustes-quaternion",
        source = rightModes[0].BackgroundId,
        target = leftModes[0].BackgroundId,
        rotation,
        determinant = Determinant3(rotation),
    },
    rawAudit,
    alignedAudit,
    unalignedMinSingularValue,
    alignedTransportStable,
    knownBosonPromotionAllowed = false,
    blockers = alignedTransportStable
        ? new[]
        {
            "gauge-frame aligned transport is stable only at protected-sector level",
            "no target-independent U(1) photon identity is derived",
            "no target-independent color-octet gluon identity is derived",
            "individual registry candidates remain branch-fragile C0 numerical modes",
        }
        : new[]
        {
            "even after target-independent gauge-frame alignment, protected massless subspace transport is not stable enough",
            "no target-independent U(1) photon identity is derived",
            "no target-independent color-octet gluon identity is derived",
        },
    nextWork = alignedTransportStable
        ? "use the gauge-frame transport only as sector-level support; derive U(1)/color identity split before photon/gluon prediction"
        : "current local massless-sector transport paths are exhausted; a new target-independent identity source is required before photon/gluon prediction",
    sourceEvidence = new
    {
        phase174Path = Phase174Path,
        phase178Path = File.Exists(Phase178Path) ? Phase178Path : null,
        signatureDir = SignatureDir,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "gauge_frame_aligned_subspace_transport.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "gauge_frame_aligned_subspace_transport_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.protectedSubspaceReady,
        result.expectedDimension,
        result.dimensionStable,
        result.gaugeFrameTransport,
        result.rawAudit,
        result.alignedAudit,
        result.alignedTransportStable,
        result.knownBosonPromotionAllowed,
        result.blockers,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"dimensionStable={dimensionStable}");
Console.WriteLine($"rawMinSingularValue={rawAudit.MinSingularValue:R}");
Console.WriteLine($"alignedMinSingularValue={alignedAudit.MinSingularValue:R}");
Console.WriteLine($"alignedTransportStable={alignedTransportStable}");

static ModeRecord LoadMode(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var root = doc.RootElement;
    var shape = root.GetProperty("observedShape").EnumerateArray().Select(item => item.GetInt32()).ToArray();
    var values = root.GetProperty("observedCoefficients").EnumerateArray().Select(item => item.GetDouble()).ToArray();
    return new ModeRecord(
        RequiredString(root, "modeId"),
        RequiredString(root, "backgroundId"),
        ModeIndex(RequiredString(root, "modeId")),
        shape[0],
        shape[1],
        values);
}

static int ModeIndex(string modeId)
{
    int index = modeId.LastIndexOf("-mode-", StringComparison.Ordinal);
    if (index < 0 || !int.TryParse(modeId[(index + "-mode-".Length)..], out int value))
        throw new InvalidDataException($"Cannot parse mode index from '{modeId}'.");
    return value;
}

static double[][] ComputeGaugeRotation(IReadOnlyList<ModeRecord> sourceModes, IReadOnlyList<ModeRecord> targetModes)
{
    var cross = new double[3, 3];
    for (int modeIndex = 0; modeIndex < sourceModes.Count; modeIndex++)
    {
        var source = sourceModes[modeIndex];
        var target = targetModes.Single(mode => mode.ModeIndex == source.ModeIndex);
        if (source.FaceCount != target.FaceCount || source.GaugeCount != 3 || target.GaugeCount != 3)
            throw new InvalidDataException("Gauge-frame transport expects matching 3-component face-major signatures.");

        for (int face = 0; face < source.FaceCount; face++)
        {
            for (int i = 0; i < 3; i++)
            {
                double sourceValue = source.Values[face * 3 + i];
                for (int j = 0; j < 3; j++)
                    cross[i, j] += sourceValue * target.Values[face * 3 + j];
            }
        }
    }

    var k = new double[4, 4];
    double sxx = cross[0, 0], sxy = cross[0, 1], sxz = cross[0, 2];
    double syx = cross[1, 0], syy = cross[1, 1], syz = cross[1, 2];
    double szx = cross[2, 0], szy = cross[2, 1], szz = cross[2, 2];
    double trace = sxx + syy + szz;
    k[0, 0] = trace;
    k[0, 1] = syz - szy;
    k[0, 2] = szx - sxz;
    k[0, 3] = sxy - syx;
    k[1, 0] = k[0, 1];
    k[1, 1] = sxx - syy - szz;
    k[1, 2] = sxy + syx;
    k[1, 3] = szx + sxz;
    k[2, 0] = k[0, 2];
    k[2, 1] = k[1, 2];
    k[2, 2] = -sxx + syy - szz;
    k[2, 3] = syz + szy;
    k[3, 0] = k[0, 3];
    k[3, 1] = k[1, 3];
    k[3, 2] = k[2, 3];
    k[3, 3] = -sxx - syy + szz;

    var eigen = JacobiEigen(k);
    int best = Enumerable.Range(0, 4).OrderByDescending(index => eigen.Values[index]).First();
    var q = Normalize(Enumerable.Range(0, 4).Select(row => eigen.Vectors[row, best]).ToArray());
    return QuaternionToRotation(q);
}

static ModeRecord ApplyGaugeRotation(ModeRecord mode, IReadOnlyList<IReadOnlyList<double>> rotation)
{
    var values = new double[mode.Values.Length];
    for (int face = 0; face < mode.FaceCount; face++)
    {
        for (int row = 0; row < mode.GaugeCount; row++)
        {
            double sum = 0.0;
            for (int col = 0; col < mode.GaugeCount; col++)
                sum += rotation[row][col] * mode.Values[face * mode.GaugeCount + col];
            values[face * mode.GaugeCount + row] = sum;
        }
    }

    return mode with { Values = values };
}

static double[][] QuaternionToRotation(IReadOnlyList<double> q)
{
    double w = q[0], x = q[1], y = q[2], z = q[3];
    return new[]
    {
        new[] { 1.0 - 2.0 * (y * y + z * z), 2.0 * (x * y - z * w), 2.0 * (x * z + y * w) },
        new[] { 2.0 * (x * y + z * w), 1.0 - 2.0 * (x * x + z * z), 2.0 * (y * z - x * w) },
        new[] { 2.0 * (x * z - y * w), 2.0 * (y * z + x * w), 1.0 - 2.0 * (x * x + y * y) },
    };
}

static BackgroundSubspace BuildSubspace(string backgroundId, IReadOnlyList<ModeRecord> modes, double rankTolerance)
{
    int ambientDimension = modes[0].Values.Length;
    var basis = new List<double[]>();
    foreach (var mode in modes.OrderBy(mode => mode.ModeIndex))
    {
        var vector = (double[])mode.Values.Clone();
        foreach (var basisVector in basis)
        {
            double projection = Dot(vector, basisVector);
            for (int i = 0; i < vector.Length; i++)
                vector[i] -= projection * basisVector[i];
        }

        double norm = Norm(vector);
        if (norm <= rankTolerance)
            continue;
        for (int i = 0; i < vector.Length; i++)
            vector[i] /= norm;
        basis.Add(vector);
    }

    return new BackgroundSubspace(backgroundId, modes.Count, ambientDimension, basis.Count, basis);
}

static PairwiseSubspaceAudit CompareSubspaces(BackgroundSubspace a, BackgroundSubspace b)
{
    int rows = a.Basis.Count;
    int cols = b.Basis.Count;
    var overlap = new double[rows, cols];
    for (int i = 0; i < rows; i++)
    {
        for (int j = 0; j < cols; j++)
            overlap[i, j] = Dot(a.Basis[i], b.Basis[j]);
    }

    var gram = new double[cols, cols];
    for (int i = 0; i < cols; i++)
    {
        for (int j = 0; j < cols; j++)
        {
            double sum = 0.0;
            for (int k = 0; k < rows; k++)
                sum += overlap[k, i] * overlap[k, j];
            gram[i, j] = sum;
        }
    }

    var eigen = JacobiEigen(gram);
    var singularValues = eigen.Values
        .Select(value => Math.Sqrt(Math.Clamp(value, 0.0, 1.0)))
        .OrderByDescending(value => value)
        .ToArray();
    double min = singularValues.DefaultIfEmpty(0.0).Min();
    double mean = singularValues.DefaultIfEmpty(0.0).Average();
    double maxPrincipalAngleDegrees = Math.Acos(Math.Clamp(min, -1.0, 1.0)) * 180.0 / Math.PI;
    return new PairwiseSubspaceAudit(
        a.BackgroundId,
        b.BackgroundId,
        singularValues,
        min,
        mean,
        maxPrincipalAngleDegrees);
}

static (double[] Values, double[,] Vectors) JacobiEigen(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    var v = new double[n, n];
    for (int i = 0; i < n; i++)
        v[i, i] = 1.0;

    for (int iter = 0; iter < 240; iter++)
    {
        int p = 0;
        int q = n > 1 ? 1 : 0;
        double max = n > 1 ? Math.Abs(a[p, q]) : 0.0;
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double value = Math.Abs(a[i, j]);
                if (value > max)
                {
                    max = value;
                    p = i;
                    q = j;
                }
            }
        }

        if (max < 1e-30)
            break;

        double theta = 0.5 * Math.Atan2(2.0 * a[p, q], a[q, q] - a[p, p]);
        double c = Math.Cos(theta);
        double s = Math.Sin(theta);

        for (int k = 0; k < n; k++)
        {
            double akp = a[k, p];
            double akq = a[k, q];
            a[k, p] = c * akp - s * akq;
            a[k, q] = s * akp + c * akq;
        }
        for (int k = 0; k < n; k++)
        {
            double apk = a[p, k];
            double aqk = a[q, k];
            a[p, k] = c * apk - s * aqk;
            a[q, k] = s * apk + c * aqk;
        }
        for (int k = 0; k < n; k++)
        {
            double vkp = v[k, p];
            double vkq = v[k, q];
            v[k, p] = c * vkp - s * vkq;
            v[k, q] = s * vkp + c * vkq;
        }
    }

    var values = Enumerable.Range(0, n).Select(i => Math.Max(0.0, a[i, i])).ToArray();
    return (values, v);
}

static double[] Normalize(double[] vector)
{
    double norm = Norm(vector);
    return norm <= 1e-300 ? vector : vector.Select(value => value / norm).ToArray();
}

static double Determinant3(IReadOnlyList<IReadOnlyList<double>> m) =>
    m[0][0] * (m[1][1] * m[2][2] - m[1][2] * m[2][1])
    - m[0][1] * (m[1][0] * m[2][2] - m[1][2] * m[2][0])
    + m[0][2] * (m[1][0] * m[2][1] - m[1][1] * m[2][0]);
static double Dot(IReadOnlyList<double> a, IReadOnlyList<double> b) =>
    a.Zip(b, (x, y) => x * y).Sum();
static double Norm(IReadOnlyList<double> vector) =>
    Math.Sqrt(vector.Sum(value => value * value));
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetDouble(out var value) ? value : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ModeRecord(string ModeId, string BackgroundId, int ModeIndex, int FaceCount, int GaugeCount, double[] Values);
sealed record BackgroundSubspace(
    string BackgroundId,
    int ModeCount,
    int AmbientDimension,
    int Dimension,
    IReadOnlyList<double[]> Basis);
sealed record PairwiseSubspaceAudit(
    string LeftBackgroundId,
    string RightBackgroundId,
    IReadOnlyList<double> SingularValuesDescending,
    double MinSingularValue,
    double MeanSingularValue,
    double MaxPrincipalAngleDegrees);
