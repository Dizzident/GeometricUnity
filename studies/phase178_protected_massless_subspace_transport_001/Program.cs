using System.Text.Json;

const string DefaultOutputDir = "studies/phase178_protected_massless_subspace_transport_001/output";
const string Phase174Path = "studies/phase174_protected_massless_subspace_audit_001/output/protected_massless_subspace_audit.json";
const string SignatureDir = "studies/phase12_joined_calculation_001/output/background_family/observables/mode_signatures";

var outputDir = Environment.GetEnvironmentVariable("PHASE178_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase174 = JsonDocument.Parse(File.ReadAllText(Phase174Path));

double rankTolerance = 1e-20;
double minSingularValueThreshold = 0.95;
double meanSingularValueThreshold = 0.99;
bool protectedSubspaceReady = JsonBool(phase174.RootElement, "subspaceDiagnosticReady") is true;
int? expectedDimension = JsonInt(phase174.RootElement, "protectedSubspaceDimension");

var modeRecords = Directory.EnumerateFiles(SignatureDir, "*.json")
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(LoadMode)
    .ToArray();
var backgroundSubspaces = modeRecords
    .GroupBy(mode => mode.BackgroundId, StringComparer.Ordinal)
    .Select(group => BuildSubspace(group.Key, group.ToArray(), rankTolerance))
    .OrderBy(subspace => subspace.BackgroundId, StringComparer.Ordinal)
    .ToArray();

var pairwiseAudits = new List<PairwiseSubspaceAudit>();
for (int i = 0; i < backgroundSubspaces.Length; i++)
{
    for (int j = i + 1; j < backgroundSubspaces.Length; j++)
        pairwiseAudits.Add(CompareSubspaces(backgroundSubspaces[i], backgroundSubspaces[j]));
}

bool dimensionStable = expectedDimension is not null
    && backgroundSubspaces.All(subspace => subspace.Dimension == expectedDimension);
bool transportStable = protectedSubspaceReady
    && dimensionStable
    && pairwiseAudits.Count > 0
    && pairwiseAudits.All(audit =>
        audit.MinSingularValue >= minSingularValueThreshold
        && audit.MeanSingularValue >= meanSingularValueThreshold);
string terminalStatus = transportStable
    ? "protected-massless-subspace-transport-stable"
    : "protected-massless-subspace-transport-not-stable";

var result = new
{
    phaseId = "phase178-protected-massless-subspace-transport",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    rankTolerance,
    minSingularValueThreshold,
    meanSingularValueThreshold,
    protectedSubspaceReady,
    expectedDimension,
    dimensionStable,
    backgroundSubspaces,
    pairwiseAudits,
    transportStable,
    predictionMeaning = transportStable
        ? "target-independent protected massless sector dimension is branch-stable"
        : "protected massless sector transport is not stable enough for branch-stable sector promotion",
    knownBosonPromotionAllowed = false,
    blockers = transportStable
        ? new[]
        {
            "subspace transport is stable only at sector level",
            "no target-independent U(1) photon identity is derived",
            "no target-independent color-octet gluon identity is derived",
            "individual registry candidates remain branch-fragile C0 numerical modes",
        }
        : new[]
        {
            "protected massless subspace failed the sibling-background transport stability gate",
            "no target-independent U(1) photon identity is derived",
            "no target-independent color-octet gluon identity is derived",
        },
    nextWork = transportStable
        ? "use stable sector transport as supporting evidence only; derive U(1)/color identity split before photon/gluon prediction"
        : "derive a target-independent transport map or new sector identity source before using protected massless modes for prediction",
    sourceEvidence = new
    {
        phase174Path = Phase174Path,
        signatureDir = SignatureDir,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "protected_massless_subspace_transport.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "protected_massless_subspace_transport_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.protectedSubspaceReady,
        result.expectedDimension,
        result.dimensionStable,
        result.pairwiseAudits,
        result.transportStable,
        result.knownBosonPromotionAllowed,
        result.blockers,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"dimensionStable={dimensionStable}");
Console.WriteLine($"transportStable={transportStable}");
Console.WriteLine($"pairwiseCount={pairwiseAudits.Count}");
if (pairwiseAudits.Count > 0)
    Console.WriteLine($"minSingularValue={pairwiseAudits.Min(audit => audit.MinSingularValue):R}");

static ModeRecord LoadMode(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var root = doc.RootElement;
    var shape = root.GetProperty("observedShape").EnumerateArray().Select(item => item.GetInt32()).ToArray();
    var values = root.GetProperty("observedCoefficients").EnumerateArray().Select(item => item.GetDouble()).ToArray();
    return new ModeRecord(
        RequiredString(root, "modeId"),
        RequiredString(root, "backgroundId"),
        shape[0],
        shape[1],
        values);
}

static BackgroundSubspace BuildSubspace(string backgroundId, IReadOnlyList<ModeRecord> modes, double rankTolerance)
{
    int ambientDimension = modes[0].Values.Length;
    var basis = new List<double[]>();
    foreach (var mode in modes.OrderBy(mode => mode.ModeId, StringComparer.Ordinal))
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

    return new BackgroundSubspace(
        backgroundId,
        modes.Count,
        ambientDimension,
        basis.Count,
        modes.Select(mode => mode.ModeId).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
        basis);
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

    for (int iter = 0; iter < 200; iter++)
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
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ModeRecord(string ModeId, string BackgroundId, int FaceCount, int GaugeCount, double[] Values);
sealed record BackgroundSubspace(
    string BackgroundId,
    int ModeCount,
    int AmbientDimension,
    int Dimension,
    IReadOnlyList<string> ModeIds,
    IReadOnlyList<double[]> Basis);
sealed record PairwiseSubspaceAudit(
    string LeftBackgroundId,
    string RightBackgroundId,
    IReadOnlyList<double> SingularValuesDescending,
    double MinSingularValue,
    double MeanSingularValue,
    double MaxPrincipalAngleDegrees);
