using System.Text.Json;

const string DefaultOutputDir = "studies/phase182_wz_operator_algebra_bridge_source_001/output";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase172Path = "studies/phase172_variation_subspace_bridge_norm_census_001/output/variation_subspace_bridge_norm_census.json";
const string Phase91Dir = "studies/phase91_branch_stability_evidence_promotion_001/output";
const string MatrixDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations";

var outputDir = Environment.GetEnvironmentVariable("PHASE182_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));
using var phase172 = JsonDocument.Parse(File.ReadAllText(Phase172Path));

double targetRaw = phase110.RootElement.GetProperty("repairTarget").GetProperty("targetImpliedRawMatrixElementMagnitude").GetDouble();
double rawGateRatio = 0.95;
double stabilitySpreadTolerance = 0.05;
double matrixNormFloor = 1e-300;
var backgrounds = new[] { "bg-phase12-bg-a-20260315212202", "bg-phase12-bg-b-20260315212202" };
var bestPair = phase172.RootElement.GetProperty("bestAssessment").GetProperty("pair");
var pair = new PairKey(JsonInt(bestPair, "fromModeIndex") ?? 4, JsonInt(bestPair, "toModeIndex") ?? 6);

var backgroundModes = backgrounds.ToDictionary(
    backgroundId => backgroundId,
    backgroundId => LoadModes(Path.Combine(Phase91Dir, backgroundId, "branch_stability_promoted_fermion_modes.json")),
    StringComparer.Ordinal);
var matricesByBackground = backgrounds.ToDictionary(
    backgroundId => backgroundId,
    backgroundId => Directory.EnumerateFiles(MatrixDir, $"variation-{backgroundId}-candidate-*.matrix.json")
        .OrderBy(path => CandidateIndex(path))
        .Select(LoadOperator)
        .ToArray(),
    StringComparer.Ordinal);

var candidateIds = BuildCandidateIds(matricesByBackground[backgrounds[0]]);
var assessments = candidateIds
    .Select(candidate => AssessCandidate(candidate, backgrounds, backgroundModes, matricesByBackground, pair, targetRaw, rawGateRatio, stabilitySpreadTolerance))
    .OrderByDescending(assessment => assessment.MinRawToTargetRatio)
    .ThenBy(assessment => assessment.RelativeSpread)
    .ThenBy(assessment => assessment.CandidateId, StringComparer.Ordinal)
    .ToArray();

var rawPassing = assessments.Where(assessment => assessment.RawGatePassed).ToArray();
var stablePassing = assessments.Where(assessment => assessment.RawGatePassed && assessment.StabilityPassed).ToArray();
var best = assessments.FirstOrDefault();
string terminalStatus = stablePassing.Length > 0
    ? "wz-operator-algebra-bridge-source-stable-raw-source-found-identity-review-required"
    : rawPassing.Length > 0
        ? "wz-operator-algebra-bridge-source-raw-source-found-not-stable"
        : "wz-operator-algebra-bridge-source-no-candidate-clears-gate";

var result = new
{
    phaseId = "phase182-wz-operator-algebra-bridge-source",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    targetRaw,
    rawGateRatio,
    stabilitySpreadTolerance,
    matrixNormFloor,
    pair,
    candidateConstruction = new
    {
        source = "Phase12 fermion coupling variation operators",
        normalization = "each variation operator is Hilbert-Schmidt normalized before algebraic composition",
        candidateFamilies = new[] { "single-normalized-operator", "normalized-commutator", "normalized-anticommutator" },
        targetValuesUsedForConstruction = false,
    },
    candidateCount = assessments.Length,
    rawGatePassingCandidateCount = rawPassing.Length,
    stableRawGatePassingCandidateCount = stablePassing.Length,
    bestCandidate = best,
    topCandidates = assessments.Take(12).ToArray(),
    stableRawGatePassingCandidates = stablePassing.Take(12).ToArray(),
    bridgeRevisionArtifactValidated = stablePassing.Length > 0,
    diagnosis = stablePassing.Length > 0
        ? "Operator-algebra construction found a stable raw-amplitude-clearing source candidate. It still needs identity review before prediction promotion."
        : rawPassing.Length > 0
            ? "Operator-algebra construction can clear raw amplitude, but not sibling-background stability."
            : "Hilbert-Schmidt-normalized operator-algebra candidates do not clear the W/Z raw-amplitude gate.",
    nextWork = stablePassing.Length > 0
        ? "audit the clearing operator-algebra source identity, then update P173/P181 if identity review passes"
        : "the current Phase12 variation-operator algebra does not supply the W/Z bridge source; next step is deriving the connection-mode-to-Dirac-variation operator unit scale called out by P118",
    sourceEvidence = new
    {
        phase110Path = Phase110Path,
        phase172Path = Phase172Path,
        phase91Dir = Phase91Dir,
        matrixDir = MatrixDir,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "wz_operator_algebra_bridge_source.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_operator_algebra_bridge_source_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.targetRaw,
        result.rawGateRatio,
        result.stabilitySpreadTolerance,
        result.pair,
        result.candidateConstruction,
        result.candidateCount,
        result.rawGatePassingCandidateCount,
        result.stableRawGatePassingCandidateCount,
        result.bestCandidate,
        result.topCandidates,
        result.bridgeRevisionArtifactValidated,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"candidateCount={assessments.Length}");
Console.WriteLine($"rawGatePassingCandidateCount={rawPassing.Length}");
Console.WriteLine($"stableRawGatePassingCandidateCount={stablePassing.Length}");
Console.WriteLine($"bestCandidateId={best?.CandidateId}");
Console.WriteLine($"bestMinRawToTargetRatio={best?.MinRawToTargetRatio:R}");

static CandidateAssessment AssessCandidate(
    CandidateSpec candidate,
    IReadOnlyList<string> backgrounds,
    IReadOnlyDictionary<string, IReadOnlyList<ModeRecord>> backgroundModes,
    IReadOnlyDictionary<string, OperatorRecord[]> matricesByBackground,
    PairKey pair,
    double targetRaw,
    double rawGateRatio,
    double stabilitySpreadTolerance)
{
    var records = backgrounds.Select(backgroundId =>
    {
        var modes = backgroundModes[backgroundId];
        var from = modes.Single(mode => mode.ModeIndex == pair.FromModeIndex);
        var to = modes.Single(mode => mode.ModeIndex == pair.ToModeIndex);
        var operators = matricesByBackground[backgroundId];
        var value = EvaluateComposite(candidate, operators, from.Coefficients, to.Coefficients);
        double magnitude = Magnitude(value.Real, value.Imaginary);
        return new BackgroundCandidateRecord(backgroundId, from.ModeId, to.ModeId, value.Real, value.Imaginary, magnitude, magnitude / targetRaw);
    }).ToArray();

    double minRatio = records.Min(record => record.RawToTargetRatio);
    double maxRatio = records.Max(record => record.RawToTargetRatio);
    double meanRatio = records.Average(record => record.RawToTargetRatio);
    double relativeSpread = (maxRatio - minRatio) / Math.Max(Math.Abs(meanRatio), 1e-300);
    bool rawGatePassed = minRatio >= rawGateRatio;
    bool stabilityPassed = relativeSpread <= stabilitySpreadTolerance;
    return new CandidateAssessment(candidate.CandidateId, candidate.Kind, candidate.LeftIndex, candidate.RightIndex, records, minRatio, maxRatio, relativeSpread, rawGatePassed, stabilityPassed);
}

static (double Real, double Imaginary) EvaluateComposite(CandidateSpec candidate, IReadOnlyList<OperatorRecord> operators, double[] from, double[] to)
{
    var iNorm = NormalizeVector(from);
    var jNorm = NormalizeVector(to);
    if (candidate.Kind == "single")
    {
        var applied = Apply(operators[candidate.LeftIndex].Normalized, jNorm);
        return Inner(iNorm, applied);
    }

    var left = operators[candidate.LeftIndex].Normalized;
    var right = operators[candidate.RightIndex].Normalized;
    var leftRight = Apply(left, Apply(right, jNorm));
    var rightLeft = Apply(right, Apply(left, jNorm));
    double sign = candidate.Kind == "commutator" ? -1.0 : 1.0;
    var composite = new double[leftRight.Length];
    for (int i = 0; i < composite.Length; i++)
        composite[i] = (leftRight[i] + sign * rightLeft[i]) / Math.Sqrt(2.0);
    return Inner(iNorm, composite);
}

static CandidateSpec[] BuildCandidateIds(IReadOnlyList<OperatorRecord> operators)
{
    var candidates = new List<CandidateSpec>();
    for (int i = 0; i < operators.Count; i++)
        candidates.Add(new CandidateSpec($"single:{operators[i].CandidateId}", "single", i, i));
    for (int i = 0; i < operators.Count; i++)
    {
        for (int j = i + 1; j < operators.Count; j++)
        {
            candidates.Add(new CandidateSpec($"commutator:{operators[i].CandidateId}:{operators[j].CandidateId}", "commutator", i, j));
            candidates.Add(new CandidateSpec($"anticommutator:{operators[i].CandidateId}:{operators[j].CandidateId}", "anticommutator", i, j));
        }
    }
    return candidates.ToArray();
}

static OperatorRecord LoadOperator(string path)
{
    using var doc = JsonDocument.Parse(File.ReadAllText(path));
    var real = LoadMatrix(doc.RootElement.GetProperty("real"));
    var imag = LoadMatrix(doc.RootElement.GetProperty("imag"));
    var normalized = NormalizeMatrix(real, imag);
    return new OperatorRecord(CandidateIdFromPath(path), normalized);
}

static ComplexMatrix NormalizeMatrix(double[,] real, double[,] imag)
{
    int n = real.GetLength(0);
    double norm = 0.0;
    for (int row = 0; row < n; row++)
    {
        for (int col = 0; col < n; col++)
            norm += real[row, col] * real[row, col] + imag[row, col] * imag[row, col];
    }
    norm = Math.Sqrt(norm);
    if (norm <= 1e-300)
        norm = 1.0;
    var outRe = new double[n, n];
    var outIm = new double[n, n];
    for (int row = 0; row < n; row++)
    {
        for (int col = 0; col < n; col++)
        {
            outRe[row, col] = real[row, col] / norm;
            outIm[row, col] = imag[row, col] / norm;
        }
    }
    return new ComplexMatrix(outRe, outIm);
}

static double[] Apply(ComplexMatrix matrix, double[] vector)
{
    int n = matrix.Real.GetLength(0);
    var result = new double[n * 2];
    for (int row = 0; row < n; row++)
    {
        double sumRe = 0.0;
        double sumIm = 0.0;
        for (int col = 0; col < n; col++)
        {
            double aRe = matrix.Real[row, col];
            double aIm = matrix.Imag[row, col];
            double bRe = vector[col * 2];
            double bIm = vector[col * 2 + 1];
            sumRe += aRe * bRe - aIm * bIm;
            sumIm += aRe * bIm + aIm * bRe;
        }
        result[row * 2] = sumRe;
        result[row * 2 + 1] = sumIm;
    }
    return result;
}

static (double Real, double Imaginary) Inner(double[] left, double[] right)
{
    double real = 0.0;
    double imaginary = 0.0;
    for (int k = 0; k < left.Length / 2; k++)
    {
        double iRe = left[k * 2];
        double iIm = left[k * 2 + 1];
        double dRe = right[k * 2];
        double dIm = right[k * 2 + 1];
        real += iRe * dRe + iIm * dIm;
        imaginary += iRe * dIm - iIm * dRe;
    }
    return (real, imaginary);
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

static double[] NormalizeVector(double[] vector)
{
    double norm = Math.Sqrt(vector.Sum(v => v * v));
    return norm < 1e-30 ? vector : vector.Select(v => v / norm).ToArray();
}

static int CandidateIndex(string path)
{
    string id = CandidateIdFromPath(path).Replace("candidate-", "", StringComparison.Ordinal);
    return int.TryParse(id, out int value) ? value : int.MaxValue;
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
static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? throw new InvalidDataException($"Missing {propertyName}")
        : throw new InvalidDataException($"Missing {propertyName}");
static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value) ? value : null;

sealed record PairKey(int FromModeIndex, int ToModeIndex);
sealed record ModeRecord(string ModeId, int ModeIndex, double[] Coefficients);
sealed record OperatorRecord(string CandidateId, ComplexMatrix Normalized);
sealed record ComplexMatrix(double[,] Real, double[,] Imag);
sealed record CandidateSpec(string CandidateId, string Kind, int LeftIndex, int RightIndex);
sealed record BackgroundCandidateRecord(string BackgroundId, string FromModeId, string ToModeId, double Real, double Imaginary, double Magnitude, double RawToTargetRatio);
sealed record CandidateAssessment(
    string CandidateId,
    string Kind,
    int LeftIndex,
    int RightIndex,
    IReadOnlyList<BackgroundCandidateRecord> Records,
    double MinRawToTargetRatio,
    double MaxRawToTargetRatio,
    double RelativeSpread,
    bool RawGatePassed,
    bool StabilityPassed);
