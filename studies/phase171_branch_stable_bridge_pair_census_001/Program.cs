using System.Text.Json;

const string DefaultOutputDir = "studies/phase171_branch_stable_bridge_pair_census_001/output";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase170Path = "studies/phase170_stable_source_shape_prediction_attempt_001/output/stable_source_shape_prediction_attempt.json";
const string Phase91Dir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string MatrixDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations";

var outputDir = Environment.GetEnvironmentVariable("PHASE171_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));
using var phase170 = JsonDocument.Parse(File.ReadAllText(Phase170Path));

double targetRaw = phase110.RootElement
    .GetProperty("repairTarget")
    .GetProperty("targetImpliedRawMatrixElementMagnitude")
    .GetDouble();
double rawGateRatio = 0.95;
double stabilitySpreadTolerance = 0.05;

var backgrounds = new[] { "bg-phase12-bg-a-20260315212202", "bg-phase12-bg-b-20260315212202" };
var backgroundModes = backgrounds.ToDictionary(
    backgroundId => backgroundId,
    backgroundId => LoadModes(Path.Combine(Phase91Dir, backgroundId, "branch_stability_promoted_fermion_modes.json")),
    StringComparer.Ordinal);
var matrixPaths = Directory.EnumerateFiles(MatrixDir, "*.matrix.json")
    .Select(path => new MatrixPathRecord(path, BackgroundIdFromPath(path), CandidateIdFromPath(path)))
    .OrderBy(record => record.BackgroundId, StringComparer.Ordinal)
    .ThenBy(record => record.CandidateId, StringComparer.Ordinal)
    .ToArray();

var pairKeys = backgroundModes[backgrounds[0]]
    .SelectMany(from => backgroundModes[backgrounds[0]].Where(to => to.ModeIndex != from.ModeIndex), (from, to) => new PairKey(from.ModeIndex, to.ModeIndex))
    .OrderBy(pair => pair.FromModeIndex)
    .ThenBy(pair => pair.ToModeIndex)
    .ToArray();
var candidateIds = matrixPaths
    .Select(record => record.CandidateId)
    .Distinct(StringComparer.Ordinal)
    .OrderBy(id => id, StringComparer.Ordinal)
    .ToArray();

var assessments = candidateIds
    .SelectMany(candidateId => pairKeys.Select(pair => Assess(candidateId, pair, backgrounds, backgroundModes, matrixPaths, targetRaw, rawGateRatio, stabilitySpreadTolerance)))
    .OrderByDescending(assessment => assessment.MinRawToTargetRatio)
    .ThenBy(assessment => assessment.RelativeSpread)
    .ThenBy(assessment => assessment.CandidateId, StringComparer.Ordinal)
    .ThenBy(assessment => assessment.Pair.FromModeIndex)
    .ThenBy(assessment => assessment.Pair.ToModeIndex)
    .ToArray();

var clearingAssessments = assessments
    .Where(assessment => assessment.RawGatePassed)
    .ToArray();
var stableClearingAssessments = assessments
    .Where(assessment => assessment.RawGatePassed && assessment.StabilityPassed)
    .ToArray();
var best = assessments.FirstOrDefault();
bool identityBacked = false;
string terminalStatus = stableClearingAssessments.Length > 0
    ? "branch-stable-bridge-pair-census-stable-raw-source-found-identity-blocked"
    : clearingAssessments.Length > 0
        ? "branch-stable-bridge-pair-census-raw-source-found-not-stable"
        : "branch-stable-bridge-pair-census-no-branch-stable-source-clears-gate";

var result = new
{
    phaseId = "phase171-branch-stable-bridge-pair-census",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    targetRaw,
    rawGateRatio,
    stabilitySpreadTolerance,
    identityBacked,
    backgroundIds = backgrounds,
    candidateCount = candidateIds.Length,
    orderedPairCount = pairKeys.Length,
    assessmentCount = assessments.Length,
    rawGateClearingAssessmentCount = clearingAssessments.Length,
    stableRawGateClearingAssessmentCount = stableClearingAssessments.Length,
    bestAssessment = best,
    topAssessments = assessments.Take(12).ToArray(),
    stableRawGateClearingAssessments = stableClearingAssessments.Take(12).ToArray(),
    diagnosis = stableClearingAssessments.Length > 0
        ? "A branch-stable mode-index pair and Phase12 variation candidate clears the raw-amplitude and sibling-background stability gates, but source identity remains unaudited."
        : clearingAssessments.Length > 0
            ? "Some branch-stable mode-index pairs clear the raw-amplitude gate on at least one sibling-background assessment, but none is stable enough across sibling backgrounds."
            : "No promoted Phase91 branch-stable mode-index pair and Phase12 variation candidate clears the raw-amplitude gate across sibling backgrounds.",
    nextWork = stableClearingAssessments.Length > 0
        ? "audit the stable raw-source candidate identity without W/Z target residuals, then attempt W/Z absolute prediction only if the identity audit passes"
        : "derive a new bridge source outside the current Phase91 promoted pair family or provide analytic source-shape law evidence; current local branch-stable pairs are exhausted",
    sourceEvidence = new
    {
        phase110Path = Phase110Path,
        phase170Path = Phase170Path,
        phase91Dir = Phase91Dir,
        matrixDir = MatrixDir,
    },
    upstreamStableSourceShapeStatus = JsonString(phase170.RootElement, "terminalStatus"),
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "branch_stable_bridge_pair_census.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "branch_stable_bridge_pair_census_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.targetRaw,
        result.rawGateRatio,
        result.stabilitySpreadTolerance,
        result.candidateCount,
        result.orderedPairCount,
        result.assessmentCount,
        result.rawGateClearingAssessmentCount,
        result.stableRawGateClearingAssessmentCount,
        result.bestAssessment,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"assessmentCount={assessments.Length}");
Console.WriteLine($"rawGateClearingAssessmentCount={clearingAssessments.Length}");
Console.WriteLine($"stableRawGateClearingAssessmentCount={stableClearingAssessments.Length}");
Console.WriteLine($"bestCandidateId={best?.CandidateId}");
Console.WriteLine($"bestPair={best?.Pair.FromModeIndex}->{best?.Pair.ToModeIndex}");
Console.WriteLine($"bestMinRawToTargetRatio={best?.MinRawToTargetRatio:R}");
Console.WriteLine($"bestRelativeSpread={best?.RelativeSpread:R}");

static PairAssessment Assess(
    string candidateId,
    PairKey pair,
    IReadOnlyList<string> backgrounds,
    IReadOnlyDictionary<string, IReadOnlyList<ModeRecord>> backgroundModes,
    IReadOnlyList<MatrixPathRecord> matrixPaths,
    double targetRaw,
    double rawGateRatio,
    double stabilitySpreadTolerance)
{
    var records = backgrounds
        .Select(backgroundId =>
        {
            var modes = backgroundModes[backgroundId];
            var from = modes.Single(mode => mode.ModeIndex == pair.FromModeIndex);
            var to = modes.Single(mode => mode.ModeIndex == pair.ToModeIndex);
            var matrixPath = matrixPaths.Single(path => path.BackgroundId == backgroundId && path.CandidateId == candidateId);
            using var matrix = JsonDocument.Parse(File.ReadAllText(matrixPath.Path));
            var matrixRe = LoadMatrix(matrix.RootElement.GetProperty("real"));
            var matrixIm = LoadMatrix(matrix.RootElement.GetProperty("imag"));
            var value = MatrixElement(matrixRe, matrixIm, from.Coefficients, to.Coefficients);
            double magnitude = Magnitude(value.Real, value.Imaginary);
            return new PairBackgroundRecord(
                backgroundId,
                from.ModeId,
                to.ModeId,
                value.Real,
                value.Imaginary,
                magnitude,
                magnitude / targetRaw);
        })
        .ToArray();

    double minRatio = records.Min(record => record.RawToTargetRatio);
    double maxRatio = records.Max(record => record.RawToTargetRatio);
    double meanRatio = records.Average(record => record.RawToTargetRatio);
    double spread = (maxRatio - minRatio) / Math.Max(Math.Abs(meanRatio), 1e-300);
    bool rawGatePassed = minRatio >= rawGateRatio;
    bool stabilityPassed = spread <= stabilitySpreadTolerance;
    return new PairAssessment(candidateId, pair, records, minRatio, maxRatio, spread, rawGatePassed, stabilityPassed, SourceIdentityBacked: false);
}

static IReadOnlyList<ModeRecord> LoadModes(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    return doc.RootElement.GetProperty("modes")
        .EnumerateArray()
        .Select(mode => new ModeRecord(
            RequiredString(mode, "modeId"),
            JsonInt(mode, "modeIndex") ?? throw new InvalidDataException("Missing modeIndex"),
            mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(x => x.GetDouble()).ToArray()))
        .OrderBy(mode => mode.ModeIndex)
        .ToArray();
}

static double[,] LoadMatrix(JsonElement rows)
{
    int rowCount = rows.GetArrayLength();
    int colCount = rows[0].GetArrayLength();
    var matrix = new double[rowCount, colCount];
    int row = 0;
    foreach (var rowElement in rows.EnumerateArray())
    {
        int col = 0;
        foreach (var value in rowElement.EnumerateArray())
            matrix[row, col++] = value.GetDouble();
        row++;
    }
    return matrix;
}

static (double Real, double Imaginary) MatrixElement(double[,] matrixRe, double[,] matrixIm, double[] phiI, double[] phiJ)
{
    int n = matrixRe.GetLength(0);
    var iNorm = Normalize(phiI);
    var jNorm = Normalize(phiJ);
    var deltaJ = new double[n * 2];
    for (int row = 0; row < n; row++)
    {
        double sumRe = 0.0;
        double sumIm = 0.0;
        for (int col = 0; col < n; col++)
        {
            double aRe = matrixRe[row, col];
            double aIm = matrixIm[row, col];
            double bRe = jNorm[col * 2];
            double bIm = jNorm[col * 2 + 1];
            sumRe += aRe * bRe - aIm * bIm;
            sumIm += aRe * bIm + aIm * bRe;
        }
        deltaJ[row * 2] = sumRe;
        deltaJ[row * 2 + 1] = sumIm;
    }

    double real = 0.0;
    double imaginary = 0.0;
    for (int k = 0; k < n; k++)
    {
        double iRe = iNorm[k * 2];
        double iIm = iNorm[k * 2 + 1];
        double dRe = deltaJ[k * 2];
        double dIm = deltaJ[k * 2 + 1];
        real += iRe * dRe + iIm * dIm;
        imaginary += iRe * dIm - iIm * dRe;
    }
    return (real, imaginary);
}

static double[] Normalize(double[] vector)
{
    double norm = Math.Sqrt(vector.Sum(v => v * v));
    return norm < 1e-30 ? vector : vector.Select(v => v / norm).ToArray();
}

static string BackgroundIdFromPath(string path)
{
    var name = Path.GetFileName(path);
    const string prefix = "variation-";
    const string marker = "-candidate-";
    int start = name.IndexOf(prefix, StringComparison.Ordinal);
    int markerIndex = name.IndexOf(marker, StringComparison.Ordinal);
    if (start < 0 || markerIndex < 0)
        return "";
    return name[(start + prefix.Length)..markerIndex];
}

static string CandidateIdFromPath(string path)
{
    var name = Path.GetFileName(path);
    const string marker = "-candidate-";
    int markerIndex = name.IndexOf(marker, StringComparison.Ordinal);
    int suffixIndex = name.IndexOf(".matrix.json", StringComparison.Ordinal);
    if (markerIndex < 0 || suffixIndex < 0)
        return Path.GetFileNameWithoutExtension(path);
    return "candidate-" + name[(markerIndex + marker.Length)..suffixIndex];
}

static double Magnitude(double real, double imaginary) => Math.Sqrt(real * real + imaginary * imaginary);
static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String ? property.GetString() : null;
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;
static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? throw new InvalidDataException($"Missing {propertyName}")
        : throw new InvalidDataException($"Missing {propertyName}");

sealed record ModeRecord(string ModeId, int ModeIndex, double[] Coefficients);
sealed record MatrixPathRecord(string Path, string BackgroundId, string CandidateId);
sealed record PairKey(int FromModeIndex, int ToModeIndex);
sealed record PairBackgroundRecord(
    string BackgroundId,
    string FromModeId,
    string ToModeId,
    double Real,
    double Imaginary,
    double Magnitude,
    double RawToTargetRatio);
sealed record PairAssessment(
    string CandidateId,
    PairKey Pair,
    IReadOnlyList<PairBackgroundRecord> BackgroundRecords,
    double MinRawToTargetRatio,
    double MaxRawToTargetRatio,
    double RelativeSpread,
    bool RawGatePassed,
    bool StabilityPassed,
    bool SourceIdentityBacked);
