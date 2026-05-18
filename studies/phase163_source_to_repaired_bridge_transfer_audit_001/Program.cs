using System.Text.Json;

const string DefaultOutputDir = "studies/phase163_source_to_repaired_bridge_transfer_audit_001/output";
const string Phase91ModesPath = "studies/phase91_branch_stability_evidence_promotion_001/output/bg-phase12-bg-a-20260315212202/branch_stability_promoted_fermion_modes.json";
const string Phase95ModesPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l0_2x2_refinement_matched_fermion_modes.json";
const string Phase95EvidencePath = "studies/phase95_target_blind_refinement_mode_matching_001/output/target_blind_refinement_mode_matching_evidence.json";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Candidate8MatrixPath = "studies/phase12_joined_calculation_001/output/background_family/fermions/couplings/variations/variation-bg-phase12-bg-a-20260315212202-candidate-8.matrix.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE163_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase91 = JsonDocument.Parse(File.ReadAllText(Phase91ModesPath));
using var phase95 = JsonDocument.Parse(File.ReadAllText(Phase95ModesPath));
using var phase95Evidence = JsonDocument.Parse(File.ReadAllText(Phase95EvidencePath));
using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));
using var candidate8Matrix = JsonDocument.Parse(File.ReadAllText(Candidate8MatrixPath));

double targetRaw = phase110.RootElement
    .GetProperty("repairTarget")
    .GetProperty("targetImpliedRawMatrixElementMagnitude")
    .GetDouble();
var matrixRe = LoadMatrix(candidate8Matrix.RootElement.GetProperty("real"));
var matrixIm = LoadMatrix(candidate8Matrix.RootElement.GetProperty("imag"));

var sourcePairIds = phase95Evidence.RootElement.GetProperty("selectedPhase91Pair")
    .EnumerateArray()
    .Select(row => RequiredString(row, "modeId"))
    .ToArray();
var repairedPairIds = phase95Evidence.RootElement.GetProperty("matchedPhase94L0Pair")
    .EnumerateArray()
    .Select(row => RequiredString(row, "modeId"))
    .ToArray();

var sourceModes = LoadModes(phase91.RootElement, sourcePairIds);
var repairedModes = LoadModes(phase95.RootElement, repairedPairIds);

var sourceForward = Compute("phase91-source-selected", sourceModes[0], sourceModes[1], matrixRe, matrixIm, targetRaw);
var sourceReverse = Compute("phase91-source-selected", sourceModes[1], sourceModes[0], matrixRe, matrixIm, targetRaw);
var repairedForward = Compute("phase94-repaired", repairedModes[0], repairedModes[1], matrixRe, matrixIm, targetRaw);
var repairedReverse = Compute("phase94-repaired", repairedModes[1], repairedModes[0], matrixRe, matrixIm, targetRaw);

double sourceBest = Math.Max(sourceForward.RawToTargetRatio, sourceReverse.RawToTargetRatio);
double repairedBest = Math.Max(repairedForward.RawToTargetRatio, repairedReverse.RawToTargetRatio);
double transferRatio = repairedBest / Math.Max(sourceBest, 1e-300);
bool sourceAlreadyBlocked = sourceBest < 0.95;
bool repairSuppressesAmplitude = sourceBest > 0 && transferRatio < 1e-6;

string terminalStatus = sourceAlreadyBlocked
    ? "source-to-repaired-bridge-transfer-source-already-absolute-scale-blocked"
    : repairSuppressesAmplitude
        ? "source-to-repaired-bridge-transfer-repair-suppresses-bridge"
        : "source-to-repaired-bridge-transfer-no-repair-suppression";

var result = new
{
    phaseId = "phase163-source-to-repaired-bridge-transfer-audit",
    terminalStatus,
    generatedAt = DateTimeOffset.UtcNow,
    targetRaw,
    sourceAttempts = new[] { sourceForward, sourceReverse },
    repairedAttempts = new[] { repairedForward, repairedReverse },
    transfer = new
    {
        sourceBestRawToTargetRatio = sourceBest,
        repairedBestRawToTargetRatio = repairedBest,
        repairedToSourceBestRatio = transferRatio,
        sourceAlreadyBlocked,
        repairSuppressesAmplitude,
    },
    diagnosis = sourceAlreadyBlocked
        ? "Candidate-8 is already far below the W/Z raw-amplitude gate on the Phase91 source-selected pair; the blocker is not introduced by Phase94 repair."
        : repairSuppressesAmplitude
            ? "Candidate-8 has adequate source-selected amplitude, but the Phase94 repaired pair suppresses it; the repair/matching transfer must be revised."
            : "Candidate-8 transfer does not show a repair-induced suppression large enough to explain the W/Z bridge failure.",
    nextWork = sourceAlreadyBlocked
        ? "derive a different target-independent W/Z bridge source or a source-level absolute normalization; do not use candidate-8 as the absolute W/Z bridge without new derivation"
        : repairSuppressesAmplitude
            ? "derive a repair-compatible boson variation or projection that preserves the source-selected candidate-8 bridge under Phase94 matching"
            : "audit the W/Z raw-amplitude gate and bridge relation for candidate-8 under target-independent normalization",
    sourceEvidence = new
    {
        phase91ModesPath = Phase91ModesPath,
        phase95ModesPath = Phase95ModesPath,
        phase95EvidencePath = Phase95EvidencePath,
        phase110Path = Phase110Path,
        candidate8MatrixPath = Candidate8MatrixPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
File.WriteAllText(Path.Combine(outputDir, "source_to_repaired_bridge_transfer_audit.json"), JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "source_to_repaired_bridge_transfer_audit_summary.json"),
    JsonSerializer.Serialize(new
    {
        result.phaseId,
        result.terminalStatus,
        result.targetRaw,
        result.transfer,
        result.diagnosis,
        result.nextWork,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"sourceBestRawToTargetRatio={sourceBest:R}");
Console.WriteLine($"repairedBestRawToTargetRatio={repairedBest:R}");
Console.WriteLine($"repairedToSourceBestRatio={transferRatio:R}");

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
