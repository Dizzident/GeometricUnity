using System.Text.Json;

const string DefaultOutputDir = "studies/phase164_source_level_wz_bridge_candidate_census_001/output";
const string Phase91ModesPath = "studies/phase91_branch_stability_evidence_promotion_001/output/bg-phase12-bg-a-20260315212202/branch_stability_promoted_fermion_modes.json";
const string Phase95EvidencePath = "studies/phase95_target_blind_refinement_mode_matching_001/output/target_blind_refinement_mode_matching_evidence.json";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string MatrixDir = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations";

var outputDir = Environment.GetEnvironmentVariable("PHASE164_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase91 = JsonDocument.Parse(File.ReadAllText(Phase91ModesPath));
using var phase95Evidence = JsonDocument.Parse(File.ReadAllText(Phase95EvidencePath));
using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));

double targetRaw = phase110.RootElement
    .GetProperty("repairTarget")
    .GetProperty("targetImpliedRawMatrixElementMagnitude")
    .GetDouble();
double rawGateRatio = 0.95;

var sourcePairIds = phase95Evidence.RootElement.GetProperty("selectedPhase91Pair")
    .EnumerateArray()
    .Select(row => RequiredString(row, "modeId"))
    .ToArray();
var sourceModes = LoadModes(phase91.RootElement, sourcePairIds);

var candidateResults = Directory.EnumerateFiles(MatrixDir, "*.matrix.json")
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(path => EvaluateCandidate(path, sourceModes[0], sourceModes[1], targetRaw, rawGateRatio))
    .OrderByDescending(result => result.BestRawToTargetRatio)
    .ThenBy(result => result.CandidateId, StringComparer.Ordinal)
    .ToArray();

var promotableCandidates = candidateResults
    .Where(result => result.ClearsRawGate)
    .ToArray();
var best = candidateResults.FirstOrDefault();
string terminalStatus = promotableCandidates.Length > 0
    ? "source-level-wz-bridge-candidate-census-existing-source-clears-gate"
    : "source-level-wz-bridge-candidate-census-no-existing-source-clears-gate";

var result = new
{
    phaseId = "phase164-source-level-wz-bridge-candidate-census",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    targetRaw,
    rawGateRatio,
    sourcePairModeIds = sourcePairIds,
    candidateCount = candidateResults.Length,
    promotableCandidateCount = promotableCandidates.Length,
    bestCandidate = best,
    topCandidates = candidateResults.Take(10).ToArray(),
    promotableCandidates,
    diagnosis = promotableCandidates.Length > 0
        ? "At least one existing Phase12 variation matrix clears the source-level W/Z raw-amplitude gate; it requires a target-independent identity review before promotion."
        : "No existing Phase12 variation matrix clears the source-level W/Z raw-amplitude gate on the fixed Phase91 source-selected pair.",
    nextWork = promotableCandidates.Length > 0
        ? "audit the clearing candidate's source identity and sector relation before rerunning corrected W/Z projection"
        : "derive a new target-independent W/Z bridge source or a source-level amplitude normalization; the existing Phase12 variation matrices do not supply the absolute W/Z bridge",
    sourceEvidence = new
    {
        phase91ModesPath = Phase91ModesPath,
        phase95EvidencePath = Phase95EvidencePath,
        phase110Path = Phase110Path,
        matrixDir = MatrixDir,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "source_level_wz_bridge_candidate_census.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "source_level_wz_bridge_candidate_census_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.targetRaw,
        result.rawGateRatio,
        result.candidateCount,
        result.promotableCandidateCount,
        result.bestCandidate,
        result.topCandidates,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"candidateCount={candidateResults.Length}");
Console.WriteLine($"promotableCandidateCount={promotableCandidates.Length}");
Console.WriteLine($"bestCandidateId={best?.CandidateId}");
Console.WriteLine($"bestRawToTargetRatio={best?.BestRawToTargetRatio:R}");

static CandidateResult EvaluateCandidate(string path, ModeRecord from, ModeRecord to, double targetRaw, double rawGateRatio)
{
    using var matrix = JsonDocument.Parse(File.ReadAllText(path));
    var matrixRe = LoadMatrix(matrix.RootElement.GetProperty("real"));
    var matrixIm = LoadMatrix(matrix.RootElement.GetProperty("imag"));
    string candidateId = CandidateIdFromPath(path);
    var forward = Compute("phase91-source-selected", from, to, matrixRe, matrixIm, targetRaw);
    var reverse = Compute("phase91-source-selected", to, from, matrixRe, matrixIm, targetRaw);
    double bestRawToTargetRatio = Math.Max(forward.RawToTargetRatio, reverse.RawToTargetRatio);
    return new CandidateResult(
        candidateId,
        path,
        bestRawToTargetRatio,
        bestRawToTargetRatio >= rawGateRatio,
        forward,
        reverse);
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

static ModeRecord[] LoadModes(JsonElement root, IReadOnlyList<string> ids)
{
    var all = root.GetProperty("modes")
        .EnumerateArray()
        .Select(mode => new ModeRecord(
            RequiredString(mode, "modeId"),
            mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(x => x.GetDouble()).ToArray()))
        .ToDictionary(mode => mode.ModeId, StringComparer.Ordinal);
    return ids.Select(id => all[id]).ToArray();
}

static BridgeAttempt Compute(string pairKind, ModeRecord from, ModeRecord to, double[,] matrixRe, double[,] matrixIm, double targetRaw)
{
    var value = MatrixElement(matrixRe, matrixIm, from.Coefficients, to.Coefficients);
    double magnitude = Magnitude(value.Real, value.Imaginary);
    return new BridgeAttempt(pairKind, from.ModeId, to.ModeId, value.Real, value.Imaginary, magnitude, magnitude / targetRaw);
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

static double Magnitude(double real, double imaginary) => Math.Sqrt(real * real + imaginary * imaginary);
static string RequiredString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
        ? property.GetString() ?? throw new InvalidOperationException($"Missing {propertyName}")
        : throw new InvalidOperationException($"Missing {propertyName}");

sealed record ModeRecord(string ModeId, double[] Coefficients);
sealed record BridgeAttempt(string PairKind, string FromModeId, string ToModeId, double Real, double Imaginary, double Magnitude, double RawToTargetRatio);
sealed record CandidateResult(
    string CandidateId,
    string MatrixPath,
    double BestRawToTargetRatio,
    bool ClearsRawGate,
    BridgeAttempt Forward,
    BridgeAttempt Reverse);
