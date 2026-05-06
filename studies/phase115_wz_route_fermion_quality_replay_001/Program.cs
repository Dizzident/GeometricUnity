using System.Text.Json;
using System.Text.Json.Serialization;

const string DefaultOutputDir = "studies/phase115_wz_route_fermion_quality_replay_001/output";
const string WReplayDir = "studies/phase115_wz_route_fermion_quality_replay_001/output/w_candidate0_repair_trial";
const string ZReplayDir = "studies/phase115_wz_route_fermion_quality_replay_001/output/z_candidate2_repair_trial";
const string Phase95FermionModesPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l0_2x2_refinement_matched_fermion_modes.json";
const string Phase110ContractPath = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase63GeneratorNormalizationPath = "studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE115_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var wAttempt = JsonDocument.Parse(File.ReadAllText(Path.Combine(WReplayDir, "first_boson_prediction_attempt.json")));
using var zAttempt = JsonDocument.Parse(File.ReadAllText(Path.Combine(ZReplayDir, "first_boson_prediction_attempt.json")));
using var wPackage = JsonDocument.Parse(File.ReadAllText(Path.Combine(WReplayDir, "first_boson_replay_package_summary.json")));
using var zPackage = JsonDocument.Parse(File.ReadAllText(Path.Combine(ZReplayDir, "first_boson_replay_package_summary.json")));
using var fermionModes = JsonDocument.Parse(File.ReadAllText(Phase95FermionModesPath));
using var contract = JsonDocument.Parse(File.ReadAllText(Phase110ContractPath));
using var generator = JsonDocument.Parse(File.ReadAllText(Phase63GeneratorNormalizationPath));

var wRecord = BuildRecord("w-boson", wAttempt.RootElement, wPackage.RootElement, generator.RootElement);
var zRecord = BuildRecord("z-boson", zAttempt.RootElement, zPackage.RootElement, generator.RootElement);
var selectedFermions = BuildSelectedFermionRecords(wAttempt.RootElement, fermionModes.RootElement);

bool rawEvidenceValidated = wRecord.RawEvidenceValidated && zRecord.RawEvidenceValidated;
bool replayReady = JsonBool(wAttempt.RootElement, "sourceBackedReplayReady") is true &&
                   JsonBool(zAttempt.RootElement, "sourceBackedReplayReady") is true;
bool fermionQualityPassed = wRecord.LocalGateBlockers.Count == 0 &&
                            zRecord.LocalGateBlockers.Count == 0 &&
                            selectedFermions.Count > 0 &&
                            selectedFermions.All(mode => mode.QualityPassed);
bool repairAccepted = rawEvidenceValidated && replayReady && fermionQualityPassed;

double? targetRaw = JsonDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedRawMatrixElementMagnitude");
double? targetWeak = JsonDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedWeakCoupling");

var result = new
{
    phaseId = "phase115-wz-route-fermion-quality-replay",
    terminalStatus = repairAccepted
        ? "wz-route-fermion-quality-replay-ready-absolute-projection"
        : rawEvidenceValidated
            ? "wz-route-fermion-quality-replay-blocked"
            : "wz-route-fermion-quality-replay-raw-evidence-blocked",
    strategyId = "replayed-analytic-raw-matrix-element",
    routeId = "wz-absolute-mass-repair",
    rawEvidenceValidated,
    replayReady,
    fermionQualityPassed,
    repairAccepted,
    records = new[] { wRecord, zRecord },
    selectedFermions,
    targetComparison = new
    {
        targetImpliedWeakCoupling = targetWeak,
        targetImpliedRawMatrixElementMagnitude = targetRaw,
        wRawToTargetRatio = Ratio(wRecord.RawMatrixElementMagnitude, targetRaw),
        zRawToTargetRatio = Ratio(zRecord.RawMatrixElementMagnitude, targetRaw),
        wNormalizedWeakCouplingToTargetRatio = Ratio(wRecord.NormalizedWeakCoupling, targetWeak),
        zNormalizedWeakCouplingToTargetRatio = Ratio(zRecord.NormalizedWeakCoupling, targetWeak),
        status = repairAccepted
            ? "projection-input-ready-target-comparison-deferred"
            : "projection-input-blocked",
        note = "The repaired replay clears input-quality gates; absolute W/Z projection must be rerun before making a physical target comparison.",
    },
    diagnosis = repairAccepted
        ? new[]
        {
            "W/Z-route replayed analytic matrix-element evidence validates for candidate-0 and candidate-2",
            "selected fermion modes are Phase95 target-blind matched Phase94 exact projected modes",
            "fermion gauge-reduction, residual, branch-stability, and refinement-stability gates pass",
            "absolute projection is now eligible to rerun, but no external W/Z mass prediction is claimed by this phase",
        }
        : new[]
        {
            "W/Z-route repaired replay did not clear all raw-evidence and fermion-quality gates",
        },
    closureRequirements = repairAccepted
        ? new[]
        {
            "rerun absolute W/Z projection using the repaired W/Z-route replay evidence",
            "compare the rerun projection against the Phase110 target-independent repair contract",
        }
        : new[]
        {
            "repair W/Z-route replay raw matrix-element validation",
            "repair selected W/Z-route fermion quality evidence",
        },
    sourceEvidence = new
    {
        wReplayAttemptPath = Path.Combine(WReplayDir, "first_boson_prediction_attempt.json"),
        zReplayAttemptPath = Path.Combine(ZReplayDir, "first_boson_prediction_attempt.json"),
        wReplayPackagePath = Path.Combine(WReplayDir, "first_boson_replay_package_summary.json"),
        zReplayPackagePath = Path.Combine(ZReplayDir, "first_boson_replay_package_summary.json"),
        fermionModeSourcePath = Phase95FermionModesPath,
        contractPath = Phase110ContractPath,
        generatorNormalizationPath = Phase63GeneratorNormalizationPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "wz_route_fermion_quality_replay.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_route_fermion_quality_replay_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase115-wz-route-fermion-quality-replay",
        result.terminalStatus,
        result.rawEvidenceValidated,
        result.replayReady,
        result.fermionQualityPassed,
        result.repairAccepted,
        result.closureRequirements,
    }, options));

Console.WriteLine(result.terminalStatus);

static WzRouteMatrixElementRecord BuildRecord(
    string particleId,
    JsonElement attempt,
    JsonElement package,
    JsonElement generator)
{
    var coupling = package.GetProperty("coupling");
    double? raw = JsonDouble(coupling, "couplingProxyMagnitude");
    double? scale = JsonDouble(generator, "internalToPhysicalGeneratorScale");
    return new WzRouteMatrixElementRecord
    {
        ParticleId = particleId,
        BosonModeId = JsonString(package, "bosonModeId"),
        ReplayTerminalStatus = JsonString(attempt, "replayTerminalStatus"),
        AttemptTerminalStatus = JsonString(attempt, "terminalStatus"),
        RawEvidenceValidated = string.Equals(
            JsonString(attempt, "rawMatrixElementEvidenceStatus"),
            "raw-weak-coupling-matrix-element-evidence-validated",
            StringComparison.Ordinal),
        ProductionMaterializationStatus = JsonString(attempt, "productionMaterializationStatus"),
        RawMatrixElementMagnitude = raw,
        GeneratorNormalizationScale = scale,
        NormalizedWeakCoupling = Product(raw, scale),
        SelectedFermionModeIds = JsonStringArray(attempt, "selectedFermionModeIds"),
        SelectedFermionModeIndices = JsonIntArray(attempt, "selectedFermionModeIndices"),
        LocalGateBlockers = JsonStringArray(attempt, "physicalPredictionGateBlockers"),
        CouplingRecord = coupling.Clone(),
    };
}

static IReadOnlyList<SelectedFermionRecord> BuildSelectedFermionRecords(JsonElement attempt, JsonElement fermionModes)
{
    var indices = JsonIntArray(attempt, "selectedFermionModeIndices");
    if (!fermionModes.TryGetProperty("modes", out var modes) || modes.ValueKind != JsonValueKind.Array)
        return [];

    var modeList = modes.EnumerateArray().ToArray();
    return indices
        .Where(index => index >= 0 && index < modeList.Length)
        .Select(index => BuildSelectedFermionRecord(index, modeList[index]))
        .ToList();
}

static SelectedFermionRecord BuildSelectedFermionRecord(int selectedIndex, JsonElement mode)
{
    double? residual = JsonDouble(mode, "residualNorm");
    double? branch = JsonDouble(mode, "branchStabilityScore");
    double? refinement = JsonDouble(mode, "refinementStabilityScore");
    bool gaugeReduced = JsonBool(mode, "gaugeReductionApplied") is true;
    var blockers = new List<string>();
    if (!gaugeReduced)
        blockers.Add("fermion mode was not gauge-reduced");
    if (residual is not { } r || !double.IsFinite(r) || r > 1e-6)
        blockers.Add($"fermion residual norm {residual:R} exceeds 1E-06");
    if (branch is not { } b || !double.IsFinite(b) || b < 0.5)
        blockers.Add($"fermion branch stability {branch:R} is below 0.5");
    if (refinement is not { } f || !double.IsFinite(f) || f < 0.5)
        blockers.Add($"fermion refinement stability {refinement:R} is below 0.5");

    return new SelectedFermionRecord
    {
        SelectedIndex = selectedIndex,
        ModeId = JsonString(mode, "modeId"),
        GaugeReductionApplied = gaugeReduced,
        ResidualNorm = residual,
        BranchStabilityScore = branch,
        RefinementStabilityScore = refinement,
        QualityPassed = blockers.Count == 0,
        Blockers = blockers,
    };
}

static string? JsonString(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
        ? value.GetString()
        : null;

static double? JsonDouble(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var d)
        ? d
        : null;

static bool? JsonBool(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind is JsonValueKind.True or JsonValueKind.False
        ? value.GetBoolean()
        : null;

static IReadOnlyList<string> JsonStringArray(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        return [];

    return value.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.String)
        .Select(item => item.GetString()!)
        .ToList();
}

static IReadOnlyList<int> JsonIntArray(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        return [];

    return value.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out _))
        .Select(item => item.GetInt32())
        .ToList();
}

static double? Product(double? a, double? b) =>
    a is { } x && b is { } y && double.IsFinite(x) && double.IsFinite(y) ? x * y : null;

static double? Ratio(double? value, double? target) =>
    value is { } x && target is { } y && double.IsFinite(x) && double.IsFinite(y) && Math.Abs(y) > 0
        ? x / y
        : null;

public sealed class WzRouteMatrixElementRecord
{
    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("bosonModeId")]
    public required string? BosonModeId { get; init; }

    [JsonPropertyName("replayTerminalStatus")]
    public required string? ReplayTerminalStatus { get; init; }

    [JsonPropertyName("attemptTerminalStatus")]
    public required string? AttemptTerminalStatus { get; init; }

    [JsonPropertyName("rawEvidenceValidated")]
    public required bool RawEvidenceValidated { get; init; }

    [JsonPropertyName("productionMaterializationStatus")]
    public required string? ProductionMaterializationStatus { get; init; }

    [JsonPropertyName("rawMatrixElementMagnitude")]
    public required double? RawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("generatorNormalizationScale")]
    public required double? GeneratorNormalizationScale { get; init; }

    [JsonPropertyName("normalizedWeakCoupling")]
    public required double? NormalizedWeakCoupling { get; init; }

    [JsonPropertyName("selectedFermionModeIds")]
    public required IReadOnlyList<string> SelectedFermionModeIds { get; init; }

    [JsonPropertyName("selectedFermionModeIndices")]
    public required IReadOnlyList<int> SelectedFermionModeIndices { get; init; }

    [JsonPropertyName("localGateBlockers")]
    public required IReadOnlyList<string> LocalGateBlockers { get; init; }

    [JsonPropertyName("couplingRecord")]
    public required JsonElement CouplingRecord { get; init; }
}

public sealed class SelectedFermionRecord
{
    [JsonPropertyName("selectedIndex")]
    public required int SelectedIndex { get; init; }

    [JsonPropertyName("modeId")]
    public required string? ModeId { get; init; }

    [JsonPropertyName("gaugeReductionApplied")]
    public required bool GaugeReductionApplied { get; init; }

    [JsonPropertyName("residualNorm")]
    public required double? ResidualNorm { get; init; }

    [JsonPropertyName("branchStabilityScore")]
    public required double? BranchStabilityScore { get; init; }

    [JsonPropertyName("refinementStabilityScore")]
    public required double? RefinementStabilityScore { get; init; }

    [JsonPropertyName("qualityPassed")]
    public required bool QualityPassed { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}
