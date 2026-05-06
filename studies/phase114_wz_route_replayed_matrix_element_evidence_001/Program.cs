using System.Text.Json;
using System.Text.Json.Serialization;

const string DefaultOutputDir = "studies/phase114_wz_route_replayed_matrix_element_evidence_001/output";
const string WReplayDir = "studies/phase114_wz_route_replayed_matrix_element_evidence_001/output/w_candidate0_replay";
const string ZReplayDir = "studies/phase114_wz_route_replayed_matrix_element_evidence_001/output/z_candidate2_replay";
const string Phase110ContractPath = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase63GeneratorNormalizationPath = "studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE114_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var wAttempt = JsonDocument.Parse(File.ReadAllText(Path.Combine(WReplayDir, "first_boson_prediction_attempt.json")));
using var zAttempt = JsonDocument.Parse(File.ReadAllText(Path.Combine(ZReplayDir, "first_boson_prediction_attempt.json")));
using var wPackage = JsonDocument.Parse(File.ReadAllText(Path.Combine(WReplayDir, "first_boson_replay_package_summary.json")));
using var zPackage = JsonDocument.Parse(File.ReadAllText(Path.Combine(ZReplayDir, "first_boson_replay_package_summary.json")));
using var contract = JsonDocument.Parse(File.ReadAllText(Phase110ContractPath));
using var generator = JsonDocument.Parse(File.ReadAllText(Phase63GeneratorNormalizationPath));

var wRecord = BuildRecord("w-boson", wAttempt.RootElement, wPackage.RootElement, generator.RootElement);
var zRecord = BuildRecord("z-boson", zAttempt.RootElement, zPackage.RootElement, generator.RootElement);
bool rawEvidenceValidated = wRecord.RawEvidenceValidated && zRecord.RawEvidenceValidated;
bool fermionQualityPassed = wRecord.LocalGateBlockers.Count == 0 && zRecord.LocalGateBlockers.Count == 0;

var result = new
{
    phaseId = "phase114-wz-route-replayed-matrix-element-evidence",
    terminalStatus = rawEvidenceValidated && !fermionQualityPassed
        ? "wz-route-replayed-matrix-elements-built-fermion-quality-blocked"
        : rawEvidenceValidated
            ? "wz-route-replayed-matrix-elements-ready"
            : "wz-route-replayed-matrix-elements-blocked",
    strategyId = "replayed-analytic-raw-matrix-element",
    routeId = "wz-absolute-mass-repair",
    rawEvidenceValidated,
    fermionQualityPassed,
    repairAccepted = rawEvidenceValidated && fermionQualityPassed,
    records = new[] { wRecord, zRecord },
    targetImpliedWeakCoupling = JsonDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedWeakCoupling"),
    targetImpliedRawMatrixElementMagnitude = JsonDouble(contract.RootElement.GetProperty("repairTarget"), "targetImpliedRawMatrixElementMagnitude"),
    diagnosis = rawEvidenceValidated && !fermionQualityPassed
        ? new[]
        {
            "W/Z-route replayed analytic matrix-element evidence exists for candidate-0 and candidate-2",
            "raw matrix-element evidence validates under the Phase77 source-kind rules",
            "absolute repair still blocks because replay fermion modes are not gauge-reduced and have large residuals/stability scores of zero",
        }
        : new[]
        {
            "W/Z-route replayed analytic matrix-element evidence did not validate for both modes",
        },
    closureRequirements = fermionQualityPassed
        ? Array.Empty<string>()
        : new[]
        {
            "rerun W/Z-route replay with gauge-reduced exact fermion modes",
            "promote branch/refinement stability evidence onto the selected W/Z-route fermion modes",
            "rebuild W/Z-route raw matrix-element evidence after fermion quality gates pass",
        },
    sourceEvidence = new
    {
        wReplayAttemptPath = Path.Combine(WReplayDir, "first_boson_prediction_attempt.json"),
        zReplayAttemptPath = Path.Combine(ZReplayDir, "first_boson_prediction_attempt.json"),
        wReplayPackagePath = Path.Combine(WReplayDir, "first_boson_replay_package_summary.json"),
        zReplayPackagePath = Path.Combine(ZReplayDir, "first_boson_replay_package_summary.json"),
        contractPath = Phase110ContractPath,
        generatorNormalizationPath = Phase63GeneratorNormalizationPath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "wz_route_replayed_matrix_element_evidence.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_route_replayed_matrix_element_evidence_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase114-wz-route-replayed-matrix-element-evidence",
        result.terminalStatus,
        result.rawEvidenceValidated,
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
    var blockers = JsonStringArray(attempt, "physicalPredictionGateBlockers");
    return new WzRouteMatrixElementRecord
    {
        ParticleId = particleId,
        BosonModeId = JsonString(package, "bosonModeId"),
        ReplayTerminalStatus = JsonString(attempt, "replayTerminalStatus"),
        RawEvidenceValidated = string.Equals(
            JsonString(attempt, "rawMatrixElementEvidenceStatus"),
            "raw-weak-coupling-matrix-element-evidence-validated",
            StringComparison.Ordinal),
        RawMatrixElementMagnitude = raw,
        GeneratorNormalizationScale = scale,
        NormalizedWeakCoupling = Product(raw, scale),
        LocalGateBlockers = blockers,
        CouplingRecord = coupling.Clone(),
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

static IReadOnlyList<string> JsonStringArray(JsonElement element, string propertyName)
{
    if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
        return [];

    return value.EnumerateArray()
        .Where(item => item.ValueKind == JsonValueKind.String)
        .Select(item => item.GetString()!)
        .ToList();
}

static double? Product(double? a, double? b) =>
    a is { } x && b is { } y && double.IsFinite(x) && double.IsFinite(y) ? x * y : null;

public sealed class WzRouteMatrixElementRecord
{
    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("bosonModeId")]
    public required string? BosonModeId { get; init; }

    [JsonPropertyName("replayTerminalStatus")]
    public required string? ReplayTerminalStatus { get; init; }

    [JsonPropertyName("rawEvidenceValidated")]
    public required bool RawEvidenceValidated { get; init; }

    [JsonPropertyName("rawMatrixElementMagnitude")]
    public required double? RawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("generatorNormalizationScale")]
    public required double? GeneratorNormalizationScale { get; init; }

    [JsonPropertyName("normalizedWeakCoupling")]
    public required double? NormalizedWeakCoupling { get; init; }

    [JsonPropertyName("localGateBlockers")]
    public required IReadOnlyList<string> LocalGateBlockers { get; init; }

    [JsonPropertyName("couplingRecord")]
    public required JsonElement CouplingRecord { get; init; }
}
