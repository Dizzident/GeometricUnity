using System.Text.Json;
using System.Text.Json.Serialization;

const string DefaultOutputDir = "studies/phase117_wz_repaired_pair_sweep_001/output";
const string ReplayRoot = "studies/phase117_wz_repaired_pair_sweep_001/output/replays";
const string Phase110ContractPath = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase69RelationPath = "studies/phase69_electroweak_mass_generation_relation_001/electroweak_mass_generation_relation.json";
const string Phase95ModesPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l0_2x2_refinement_matched_fermion_modes.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE117_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var contract = JsonDocument.Parse(File.ReadAllText(Phase110ContractPath));
using var relation = JsonDocument.Parse(File.ReadAllText(Phase69RelationPath));
using var modes = JsonDocument.Parse(File.ReadAllText(Phase95ModesPath));

double targetRaw = RequiredDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedRawMatrixElementMagnitude");
double targetWeak = RequiredDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedWeakCoupling");
double wInternal = RequiredDouble(relation.RootElement, "wInternalMass");
double zInternal = RequiredDouble(relation.RootElement, "zInternalMass");
double generatorScale = targetWeak / targetRaw;

var records = new List<PairSweepRecord>();
for (int i = 0; i < 12; i++)
{
    for (int j = i + 1; j < 12; j++)
    {
        var wPath = Path.Combine(ReplayRoot, $"pair-{i}-{j}", "w", "first_boson_prediction_attempt.json");
        var zPath = Path.Combine(ReplayRoot, $"pair-{i}-{j}", "z", "first_boson_prediction_attempt.json");
        if (!File.Exists(wPath) || !File.Exists(zPath))
            continue;

        using var wAttempt = JsonDocument.Parse(File.ReadAllText(wPath));
        using var zAttempt = JsonDocument.Parse(File.ReadAllText(zPath));
        var wBlockers = JsonStringArray(wAttempt.RootElement, "physicalPredictionGateBlockers");
        var zBlockers = JsonStringArray(zAttempt.RootElement, "physicalPredictionGateBlockers");
        double? wRaw = JsonDouble(wAttempt.RootElement, "couplingMagnitude");
        double? zRaw = JsonDouble(zAttempt.RootElement, "couplingMagnitude");
        double? wWeak = Product(wRaw, generatorScale);
        double? zWeak = Product(zRaw, generatorScale);
        double? wBridge = wWeak / 2.0 / wInternal;
        double? zBridge = zWeak / 2.0 / zInternal;
        double? spread = RelativeSpread(wBridge, zBridge);
        bool qualityPassed = wBlockers.Count == 0 && zBlockers.Count == 0;
        bool rawValidated =
            string.Equals(JsonString(wAttempt.RootElement, "rawMatrixElementEvidenceStatus"), "raw-weak-coupling-matrix-element-evidence-validated", StringComparison.Ordinal) &&
            string.Equals(JsonString(zAttempt.RootElement, "rawMatrixElementEvidenceStatus"), "raw-weak-coupling-matrix-element-evidence-validated", StringComparison.Ordinal);

        records.Add(new PairSweepRecord
        {
            PairId = $"pair-{i}-{j}",
            ModeI = i,
            ModeJ = j,
            WRawMatrixElementMagnitude = wRaw,
            ZRawMatrixElementMagnitude = zRaw,
            WNormalizedWeakCoupling = wWeak,
            ZNormalizedWeakCoupling = zWeak,
            WRawToTargetRatio = Ratio(wRaw, targetRaw),
            ZRawToTargetRatio = Ratio(zRaw, targetRaw),
            WBridge = wBridge,
            ZBridge = zBridge,
            CommonBridgeRelativeSpread = spread,
            CommonBridgePassed = spread is { } s && s <= 1e-12,
            FermionQualityPassed = qualityPassed,
            RawEvidenceValidated = rawValidated,
            MaxRawToTargetRatio = Math.Max(Ratio(wRaw, targetRaw) ?? 0.0, Ratio(zRaw, targetRaw) ?? 0.0),
            MinRawToTargetRatio = Math.Min(Ratio(wRaw, targetRaw) ?? 0.0, Ratio(zRaw, targetRaw) ?? 0.0),
            WReplayAttemptPath = wPath,
            ZReplayAttemptPath = zPath,
            Blockers = wBlockers.Concat(zBlockers).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToList(),
        });
    }
}

var admissible = records
    .Where(r => r.FermionQualityPassed && r.RawEvidenceValidated)
    .ToList();
var commonBridgeCandidates = admissible
    .Where(r => r.CommonBridgePassed)
    .OrderByDescending(r => r.MinRawToTargetRatio)
    .ToList();
var strongestByRaw = admissible
    .OrderByDescending(r => r.MinRawToTargetRatio)
    .ThenBy(r => r.CommonBridgeRelativeSpread ?? double.PositiveInfinity)
    .FirstOrDefault();
var strongestCommonBridge = commonBridgeCandidates.FirstOrDefault();

bool pairRepairFound = strongestCommonBridge is not null &&
                       strongestCommonBridge.WRawToTargetRatio is { } wr &&
                       strongestCommonBridge.ZRawToTargetRatio is { } zr &&
                       wr >= 0.95 &&
                       zr >= 0.95;

var result = new
{
    phaseId = "phase117-wz-repaired-pair-sweep",
    terminalStatus = pairRepairFound
        ? "wz-repaired-pair-sweep-found-projection-candidate"
        : "wz-repaired-pair-sweep-no-pair-repair",
    pairCount = records.Count,
    admissiblePairCount = admissible.Count,
    commonBridgePairCount = commonBridgeCandidates.Count,
    targetImpliedRawMatrixElementMagnitude = targetRaw,
    targetImpliedWeakCoupling = targetWeak,
    strongestByRaw,
    strongestCommonBridge,
    topPairsByRaw = admissible
        .OrderByDescending(r => r.MinRawToTargetRatio)
        .ThenBy(r => r.CommonBridgeRelativeSpread ?? double.PositiveInfinity)
        .Take(10)
        .ToList(),
    topPairsByCommonBridge = admissible
        .OrderBy(r => r.CommonBridgeRelativeSpread ?? double.PositiveInfinity)
        .ThenByDescending(r => r.MinRawToTargetRatio)
        .Take(10)
        .ToList(),
    diagnosis = pairRepairFound
        ? new[]
        {
            "a repaired exact fermion pair reaches the Phase110 target raw magnitude and passes common W/Z bridge consistency",
        }
        : new[]
        {
            "no target-blind repaired exact fermion pair simultaneously reaches the Phase110 target raw magnitude and passes common W/Z bridge consistency",
            "the strongest raw pair remains far below the Phase110 target-implied raw magnitude",
            "the common-bridge-passing pairs, if present, are not amplitude-repair candidates",
        },
    closureRequirements = pairRepairFound
        ? new[]
        {
            "rerun Phase115 and Phase116 with the selected repaired pair",
        }
        : new[]
        {
            "do not attempt another projection rerun by changing only the repaired fermion pair",
            "investigate analytic matrix-element normalization, boson vector/source normalization, or missing operator-scale factors upstream of the pair choice",
        },
    sourceEvidence = new
    {
        replayRoot = ReplayRoot,
        phase110ContractPath = Phase110ContractPath,
        phase69RelationPath = Phase69RelationPath,
        phase95ModesPath = Phase95ModesPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "wz_repaired_pair_sweep.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_repaired_pair_sweep_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase117-wz-repaired-pair-sweep",
        result.terminalStatus,
        result.pairCount,
        result.admissiblePairCount,
        result.commonBridgePairCount,
        strongestPairId = strongestByRaw?.PairId,
        strongestMinRawToTargetRatio = strongestByRaw?.MinRawToTargetRatio,
        strongestCommonBridgePairId = strongestCommonBridge?.PairId,
        result.closureRequirements,
    }, options));

Console.WriteLine(result.terminalStatus);
Console.WriteLine($"pairCount={records.Count}");
Console.WriteLine($"admissiblePairCount={admissible.Count}");
Console.WriteLine($"strongestByRaw={strongestByRaw?.PairId} minRawToTarget={strongestByRaw?.MinRawToTargetRatio:R}");

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property {propertyName}");

static IReadOnlyList<string> JsonStringArray(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        return [];

    return value.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.String)
        .Select(item => item.GetString()!)
        .ToList();
}

static double? Product(double? a, double b) =>
    a is { } x && double.IsFinite(x) && double.IsFinite(b) ? x * b : null;

static double? Ratio(double? numerator, double? denominator) =>
    numerator is { } n && denominator is { } d && double.IsFinite(n) && double.IsFinite(d) && Math.Abs(d) > 0.0
        ? n / d
        : null;

static double? RelativeSpread(double? a, double? b)
{
    if (a is not { } x || b is not { } y || !double.IsFinite(x) || !double.IsFinite(y))
        return null;
    double mean = (x + y) / 2.0;
    return Math.Abs(mean) > 0.0 ? Math.Abs(x - y) / Math.Abs(mean) : null;
}

public sealed class PairSweepRecord
{
    [JsonPropertyName("pairId")]
    public required string PairId { get; init; }

    [JsonPropertyName("modeI")]
    public required int ModeI { get; init; }

    [JsonPropertyName("modeJ")]
    public required int ModeJ { get; init; }

    [JsonPropertyName("wRawMatrixElementMagnitude")]
    public required double? WRawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("zRawMatrixElementMagnitude")]
    public required double? ZRawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("wNormalizedWeakCoupling")]
    public required double? WNormalizedWeakCoupling { get; init; }

    [JsonPropertyName("zNormalizedWeakCoupling")]
    public required double? ZNormalizedWeakCoupling { get; init; }

    [JsonPropertyName("wRawToTargetRatio")]
    public required double? WRawToTargetRatio { get; init; }

    [JsonPropertyName("zRawToTargetRatio")]
    public required double? ZRawToTargetRatio { get; init; }

    [JsonPropertyName("wBridge")]
    public required double? WBridge { get; init; }

    [JsonPropertyName("zBridge")]
    public required double? ZBridge { get; init; }

    [JsonPropertyName("commonBridgeRelativeSpread")]
    public required double? CommonBridgeRelativeSpread { get; init; }

    [JsonPropertyName("commonBridgePassed")]
    public required bool CommonBridgePassed { get; init; }

    [JsonPropertyName("fermionQualityPassed")]
    public required bool FermionQualityPassed { get; init; }

    [JsonPropertyName("rawEvidenceValidated")]
    public required bool RawEvidenceValidated { get; init; }

    [JsonPropertyName("maxRawToTargetRatio")]
    public required double MaxRawToTargetRatio { get; init; }

    [JsonPropertyName("minRawToTargetRatio")]
    public required double MinRawToTargetRatio { get; init; }

    [JsonPropertyName("wReplayAttemptPath")]
    public required string WReplayAttemptPath { get; init; }

    [JsonPropertyName("zReplayAttemptPath")]
    public required string ZReplayAttemptPath { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}
