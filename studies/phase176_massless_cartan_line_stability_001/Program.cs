using System.Text.Json;

const string DefaultOutputDir = "studies/phase176_massless_cartan_line_stability_001/output";
const string Phase174Path = "studies/phase174_protected_massless_subspace_audit_001/output/protected_massless_subspace_audit.json";
const string Phase175Path = "studies/phase175_massless_gauge_identity_split_feasibility_001/output/massless_gauge_identity_split_feasibility.json";
const string SignatureDir = "studies/phase12_joined_calculation_001/output/background_family/observables/mode_signatures";

var outputDir = Environment.GetEnvironmentVariable("PHASE176_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase174 = JsonDocument.Parse(File.ReadAllText(Phase174Path));
using var phase175 = JsonDocument.Parse(File.ReadAllText(Phase175Path));

double principalFractionThreshold = 0.80;
double principalGapThreshold = 0.20;
double lineStabilityDotThreshold = 0.95;
bool protectedSubspaceReady = JsonBool(phase174.RootElement, "subspaceDiagnosticReady") is true;

var modeRecords = Directory.EnumerateFiles(SignatureDir, "*.json")
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(LoadMode)
    .ToArray();
var backgroundAudits = modeRecords
    .GroupBy(mode => mode.BackgroundId, StringComparer.Ordinal)
    .Select(group => AuditBackground(group.Key, group.ToArray(), principalFractionThreshold, principalGapThreshold))
    .OrderBy(audit => audit.BackgroundId, StringComparer.Ordinal)
    .ToArray();

double? pairwiseLineDot = backgroundAudits.Length == 2
    ? Math.Abs(Dot(backgroundAudits[0].PrincipalLine, backgroundAudits[1].PrincipalLine))
    : null;
bool principalLineStable = pairwiseLineDot is not null && pairwiseLineDot >= lineStabilityDotThreshold;
bool everyBackgroundDominant = backgroundAudits.All(audit => audit.DominancePassed);
bool u1CartanLineCandidatePresent = protectedSubspaceReady && everyBackgroundDominant && principalLineStable;
string terminalStatus = u1CartanLineCandidatePresent
    ? "massless-cartan-line-u1-candidate-ready"
    : "massless-cartan-line-u1-candidate-not-supported";

var result = new
{
    phaseId = "phase176-massless-cartan-line-stability",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    principalFractionThreshold,
    principalGapThreshold,
    lineStabilityDotThreshold,
    protectedSubspaceReady,
    backgroundAudits,
    pairwiseLineDot,
    principalLineStable,
    everyBackgroundDominant,
    u1CartanLineCandidatePresent,
    blockers = u1CartanLineCandidatePresent
        ? Array.Empty<string>()
        : new[]
        {
            "massless gauge covariance does not define a dominant one-dimensional Cartan-like line on every sibling background",
            "principal gauge line is not sufficiently stable across sibling backgrounds",
            "without a stable one-dimensional gauge line, the protected massless subspace cannot be promoted to a photon U(1) identity",
        },
    nextWork = u1CartanLineCandidatePresent
        ? "materialize the stable Cartan-line U(1) identity and rerun photon masslessness prediction contracts"
        : "supply an external or newly derived target-independent U(1) sector artifact; no local Cartan-line identity is present in current Phase12 massless signatures",
    sourceEvidence = new
    {
        phase174Path = Phase174Path,
        phase175Path = Phase175Path,
        phase175Status = JsonString(phase175.RootElement, "terminalStatus"),
        signatureDir = SignatureDir,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "massless_cartan_line_stability.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "massless_cartan_line_stability_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.protectedSubspaceReady,
        result.backgroundAudits,
        result.pairwiseLineDot,
        result.principalLineStable,
        result.everyBackgroundDominant,
        result.u1CartanLineCandidatePresent,
        result.blockers,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"pairwiseLineDot={pairwiseLineDot:R}");
Console.WriteLine($"everyBackgroundDominant={everyBackgroundDominant}");
Console.WriteLine($"u1CartanLineCandidatePresent={u1CartanLineCandidatePresent}");

static ModeRecord LoadMode(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var root = doc.RootElement;
    var shape = root.GetProperty("observedShape").EnumerateArray().Select(item => item.GetInt32()).ToArray();
    var values = root.GetProperty("observedCoefficients").EnumerateArray().Select(item => item.GetDouble()).ToArray();
    return new ModeRecord(RequiredString(root, "modeId"), RequiredString(root, "backgroundId"), shape[0], shape[1], values);
}

static BackgroundAudit AuditBackground(string backgroundId, IReadOnlyList<ModeRecord> modes, double principalFractionThreshold, double principalGapThreshold)
{
    int gaugeCount = modes[0].GaugeCount;
    var covariance = new double[gaugeCount, gaugeCount];
    foreach (var mode in modes)
    {
        for (int face = 0; face < mode.FaceCount; face++)
        {
            for (int i = 0; i < gaugeCount; i++)
            {
                double vi = mode.Values[face * gaugeCount + i];
                for (int j = 0; j < gaugeCount; j++)
                    covariance[i, j] += vi * mode.Values[face * gaugeCount + j];
            }
        }
    }

    var eigen = JacobiEigen(covariance);
    var order = Enumerable.Range(0, gaugeCount)
        .OrderByDescending(index => eigen.Values[index])
        .ToArray();
    var eigenvalues = order.Select(index => eigen.Values[index]).ToArray();
    var principal = Normalize(order.Select(_ => 0.0).ToArray());
    principal = Enumerable.Range(0, gaugeCount).Select(row => eigen.Vectors[row, order[0]]).ToArray();
    principal = Normalize(principal);
    double total = eigenvalues.Sum();
    double principalFraction = total > 0.0 ? eigenvalues[0] / total : 0.0;
    double secondFraction = total > 0.0 && eigenvalues.Length > 1 ? eigenvalues[1] / total : 0.0;
    double gap = principalFraction - secondFraction;
    bool dominancePassed = principalFraction >= principalFractionThreshold && gap >= principalGapThreshold;
    return new BackgroundAudit(
        backgroundId,
        modes.Count,
        MatrixToJagged(covariance),
        eigenvalues,
        principal,
        principalFraction,
        secondFraction,
        gap,
        dominancePassed);
}

static (double[] Values, double[,] Vectors) JacobiEigen(double[,] input)
{
    int n = input.GetLength(0);
    var a = (double[,])input.Clone();
    var v = new double[n, n];
    for (int i = 0; i < n; i++)
        v[i, i] = 1.0;

    for (int iter = 0; iter < 80; iter++)
    {
        int p = 0;
        int q = 1;
        double max = Math.Abs(a[p, q]);
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
    double norm = Math.Sqrt(vector.Sum(value => value * value));
    return norm <= 1e-300 ? vector : vector.Select(value => value / norm).ToArray();
}

static double Dot(IReadOnlyList<double> a, IReadOnlyList<double> b) =>
    a.Zip(b, (x, y) => x * y).Sum();
static double[][] MatrixToJagged(double[,] matrix)
{
    int rows = matrix.GetLength(0);
    int cols = matrix.GetLength(1);
    var result = new double[rows][];
    for (int row = 0; row < rows; row++)
    {
        result[row] = new double[cols];
        for (int col = 0; col < cols; col++)
            result[row][col] = matrix[row, col];
    }
    return result;
}
static string RequiredString(JsonElement element, string propertyName) =>
    JsonString(element, propertyName) ?? throw new InvalidDataException($"Missing string property '{propertyName}'.");
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) ? property.ValueKind switch { JsonValueKind.True => true, JsonValueKind.False => false, _ => null } : null;

sealed record ModeRecord(string ModeId, string BackgroundId, int FaceCount, int GaugeCount, double[] Values);
sealed record BackgroundAudit(
    string BackgroundId,
    int ModeCount,
    IReadOnlyList<IReadOnlyList<double>> GaugeCovariance,
    IReadOnlyList<double> EigenvaluesDescending,
    IReadOnlyList<double> PrincipalLine,
    double PrincipalFraction,
    double SecondFraction,
    double PrincipalGap,
    bool DominancePassed);
