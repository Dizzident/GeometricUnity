using System.Text.Json;

const string DefaultOutputDir = "studies/phase172_variation_subspace_bridge_norm_census_001/output";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase171Path = "studies/phase171_branch_stable_bridge_pair_census_001/output/branch_stable_bridge_pair_census.json";
const string Phase91Dir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string MatrixDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations";

var outputDir = Environment.GetEnvironmentVariable("PHASE172_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));
using var phase171 = JsonDocument.Parse(File.ReadAllText(Phase171Path));

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

var assessments = pairKeys
    .Select(pair => Assess(pair, backgrounds, backgroundModes, matrixPaths, targetRaw, rawGateRatio, stabilitySpreadTolerance))
    .OrderByDescending(assessment => assessment.MinRawToTargetRatio)
    .ThenBy(assessment => assessment.RelativeSpread)
    .ThenBy(assessment => assessment.Pair.FromModeIndex)
    .ThenBy(assessment => assessment.Pair.ToModeIndex)
    .ToArray();

var rawGatePassing = assessments.Where(assessment => assessment.RawGatePassed).ToArray();
var stableRawGatePassing = assessments.Where(assessment => assessment.RawGatePassed && assessment.StabilityPassed).ToArray();
var best = assessments.FirstOrDefault();
bool identityBacked = false;
string terminalStatus = stableRawGatePassing.Length > 0
    ? "variation-subspace-bridge-norm-census-stable-raw-source-found-identity-blocked"
    : rawGatePassing.Length > 0
        ? "variation-subspace-bridge-norm-census-raw-source-found-not-stable"
        : "variation-subspace-bridge-norm-census-no-subspace-source-clears-gate";

var result = new
{
    phaseId = "phase172-variation-subspace-bridge-norm-census",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    targetRaw,
    rawGateRatio,
    stabilitySpreadTolerance,
    identityBacked,
    backgroundIds = backgrounds,
    variationCandidateCountPerBackground = matrixPaths.GroupBy(path => path.BackgroundId).ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal),
    orderedPairCount = pairKeys.Length,
    assessmentCount = assessments.Length,
    rawGatePassingAssessmentCount = rawGatePassing.Length,
    stableRawGatePassingAssessmentCount = stableRawGatePassing.Length,
    bestAssessment = best,
    topAssessments = assessments.Take(12).ToArray(),
    stableRawGatePassingAssessments = stableRawGatePassing.Take(12).ToArray(),
    diagnosis = stableRawGatePassing.Length > 0
        ? "The full Phase12 variation subspace contains a stable raw-amplitude-clearing bridge-source candidate, but source identity remains unaudited."
        : rawGatePassing.Length > 0
            ? "The full Phase12 variation subspace clears the raw-amplitude gate for at least one pair but fails sibling-background stability."
            : "The full Phase12 variation subspace does not clear the W/Z raw-amplitude gate for any promoted mode-index pair.",
    nextWork = stableRawGatePassing.Length > 0
        ? "audit the variation-subspace bridge identity without W/Z target residuals, then attempt W/Z absolute prediction only if the identity audit passes"
        : "derive a W/Z bridge source outside the current Phase12 variation subspace, or provide a derivation-backed analytic source-shape law before another absolute prediction attempt",
    sourceEvidence = new
    {
        phase110Path = Phase110Path,
        phase171Path = Phase171Path,
        phase91Dir = Phase91Dir,
        matrixDir = MatrixDir,
    },
    upstreamBranchStableBridgePairCensusStatus = JsonString(phase171.RootElement, "terminalStatus"),
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "variation_subspace_bridge_norm_census.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "variation_subspace_bridge_norm_census_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.targetRaw,
        result.rawGateRatio,
        result.stabilitySpreadTolerance,
        result.variationCandidateCountPerBackground,
        result.orderedPairCount,
        result.assessmentCount,
        result.rawGatePassingAssessmentCount,
        result.stableRawGatePassingAssessmentCount,
        result.bestAssessment,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"assessmentCount={assessments.Length}");
Console.WriteLine($"rawGatePassingAssessmentCount={rawGatePassing.Length}");
Console.WriteLine($"stableRawGatePassingAssessmentCount={stableRawGatePassing.Length}");
Console.WriteLine($"bestPair={best?.Pair.FromModeIndex}->{best?.Pair.ToModeIndex}");
Console.WriteLine($"bestMinRawToTargetRatio={best?.MinRawToTargetRatio:R}");
Console.WriteLine($"bestRelativeSpread={best?.RelativeSpread:R}");

static SubspaceAssessment Assess(
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
            var componentRecords = matrixPaths
                .Where(path => path.BackgroundId == backgroundId)
                .Select(path =>
                {
                    using var matrix = JsonDocument.Parse(File.ReadAllText(path.Path));
                    var matrixRe = LoadMatrix(matrix.RootElement.GetProperty("real"));
                    var matrixIm = LoadMatrix(matrix.RootElement.GetProperty("imag"));
                    var value = MatrixElement(matrixRe, matrixIm, from.Coefficients, to.Coefficients);
                    double magnitude = Magnitude(value.Real, value.Imaginary);
                    return new SubspaceComponent(path.CandidateId, value.Real, value.Imaginary, magnitude);
                })
                .OrderBy(component => component.CandidateId, StringComparer.Ordinal)
                .ToArray();
            double subspaceNorm = Math.Sqrt(componentRecords.Sum(component => component.Magnitude * component.Magnitude));
            return new SubspaceBackgroundRecord(
                backgroundId,
                from.ModeId,
                to.ModeId,
                subspaceNorm,
                subspaceNorm / targetRaw,
                componentRecords.OrderByDescending(component => component.Magnitude).Take(6).ToArray());
        })
        .ToArray();

    double minRatio = records.Min(record => record.RawToTargetRatio);
    double maxRatio = records.Max(record => record.RawToTargetRatio);
    double meanRatio = records.Average(record => record.RawToTargetRatio);
    double spread = (maxRatio - minRatio) / Math.Max(Math.Abs(meanRatio), 1e-300);
    bool rawGatePassed = minRatio >= rawGateRatio;
    bool stabilityPassed = spread <= stabilitySpreadTolerance;
    return new SubspaceAssessment(pair, records, minRatio, maxRatio, spread, rawGatePassed, stabilityPassed, SourceIdentityBacked: false);
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
sealed record SubspaceComponent(string CandidateId, double Real, double Imaginary, double Magnitude);
sealed record SubspaceBackgroundRecord(
    string BackgroundId,
    string FromModeId,
    string ToModeId,
    double SubspaceNorm,
    double RawToTargetRatio,
    IReadOnlyList<SubspaceComponent> DominantComponents);
sealed record SubspaceAssessment(
    PairKey Pair,
    IReadOnlyList<SubspaceBackgroundRecord> BackgroundRecords,
    double MinRawToTargetRatio,
    double MaxRawToTargetRatio,
    double RelativeSpread,
    bool RawGatePassed,
    bool StabilityPassed,
    bool SourceIdentityBacked);
