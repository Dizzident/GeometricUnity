using System.Text.Json;
using System.Text.Json.Serialization;

const string DefaultOutputDir = "studies/phase118_wz_matrix_element_normalization_diagnostic_001/output";
const string Phase115Path = "studies/phase115_wz_route_fermion_quality_replay_001/output/wz_route_fermion_quality_replay.json";
const string Phase114Path = "studies/phase114_wz_route_replayed_matrix_element_evidence_001/output/wz_route_replayed_matrix_element_evidence.json";
const string Phase117Path = "studies/phase117_wz_repaired_pair_sweep_001/output/wz_repaired_pair_sweep.json";
const string Phase110Path = "studies/phase110_wz_absolute_repair_execution_contract_001/output/wz_absolute_repair_execution_contract.json";
const string Phase63Path = "studies/phase63_su2_generator_normalization_001/su2_generator_normalization.json";
const string Phase95ModesPath = "studies/phase95_target_blind_refinement_mode_matching_001/output/phase94_l0_2x2_refinement_matched_fermion_modes.json";
const string WModePath = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/bg-phase12-bg-a-20260315212202-mode-0.json";
const string ZModePath = "studies/phase12_joined_calculation_001/output/background_family/spectra/modes/bg-phase12-bg-a-20260315212202-mode-2.json";

var outputDir = Environment.GetEnvironmentVariable("PHASE118_OUTPUT_DIR") ?? DefaultOutputDir;
Directory.CreateDirectory(outputDir);

using var phase115 = JsonDocument.Parse(File.ReadAllText(Phase115Path));
using var phase114 = JsonDocument.Parse(File.ReadAllText(Phase114Path));
using var phase117 = JsonDocument.Parse(File.ReadAllText(Phase117Path));
using var phase110 = JsonDocument.Parse(File.ReadAllText(Phase110Path));
using var phase63 = JsonDocument.Parse(File.ReadAllText(Phase63Path));
using var phase95 = JsonDocument.Parse(File.ReadAllText(Phase95ModesPath));
using var wMode = JsonDocument.Parse(File.ReadAllText(WModePath));
using var zMode = JsonDocument.Parse(File.ReadAllText(ZModePath));

double targetRaw = RequiredDouble(phase110.RootElement.GetProperty("repairTarget"), "targetImpliedRawMatrixElementMagnitude");
double targetWeak = RequiredDouble(phase110.RootElement.GetProperty("repairTarget"), "targetImpliedWeakCoupling");
double generatorScale = RequiredDouble(phase63.RootElement, "internalToPhysicalGeneratorScale");

var repairedW = BuildAmplitudeRecord("w-boson", FindRecord(phase115.RootElement, "w-boson"), targetRaw, targetWeak);
var repairedZ = BuildAmplitudeRecord("z-boson", FindRecord(phase115.RootElement, "z-boson"), targetRaw, targetWeak);
var invalidW = BuildAmplitudeRecord("w-boson", FindRecord(phase114.RootElement, "w-boson"), targetRaw, targetWeak);
var invalidZ = BuildAmplitudeRecord("z-boson", FindRecord(phase114.RootElement, "z-boson"), targetRaw, targetWeak);

var wVector = VectorNormRecord.FromModeJson("w-boson-source-mode", WModePath, wMode.RootElement);
var zVector = VectorNormRecord.FromModeJson("z-boson-source-mode", ZModePath, zMode.RootElement);
var fermions = BuildFermionVectorRecords(phase95.RootElement, [0, 3]);

double? repairedRawMeanScale = Mean([repairedW.RawRequiredScale, repairedZ.RawRequiredScale]);
double? repairedGeneratorMeanScale = Mean([repairedW.GeneratorScaleRequired, repairedZ.GeneratorScaleRequired]);
double? repairedRawScaleSpread = RelativeSpread(repairedW.RawRequiredScale, repairedZ.RawRequiredScale);
double? repairedGeneratorScaleSpread = RelativeSpread(repairedW.GeneratorScaleRequired, repairedZ.GeneratorScaleRequired);
double? invalidRawMeanScale = Mean([invalidW.RawRequiredScale, invalidZ.RawRequiredScale]);

int bosonVectorLength = wVector.Length;
int fermionCoefficientLength = fermions.FirstOrDefault()?.Length ?? 0;
int complexFermionDof = fermionCoefficientLength / 2;
var dimensionalCandidates = new[]
{
    ScaleCandidate("sqrt-boson-vector-length", Math.Sqrt(bosonVectorLength), repairedRawMeanScale),
    ScaleCandidate("boson-vector-length", bosonVectorLength, repairedRawMeanScale),
    ScaleCandidate("sqrt-fermion-coefficient-length", Math.Sqrt(fermionCoefficientLength), repairedRawMeanScale),
    ScaleCandidate("sqrt-complex-fermion-dof", Math.Sqrt(complexFermionDof), repairedRawMeanScale),
    ScaleCandidate("complex-fermion-dof", complexFermionDof, repairedRawMeanScale),
    ScaleCandidate("boson-vector-length-times-sqrt-complex-fermion-dof", bosonVectorLength * Math.Sqrt(complexFermionDof), repairedRawMeanScale),
    ScaleCandidate("boson-vector-length-times-sqrt-fermion-coefficient-length", bosonVectorLength * Math.Sqrt(fermionCoefficientLength), repairedRawMeanScale),
};

var ruledOut = new List<string>();
if (Math.Abs(wVector.Norm - 1.0) < 1e-12 && Math.Abs(zVector.Norm - 1.0) < 1e-12)
    ruledOut.Add("persisted W/Z boson source vectors are already unit-norm");
if (fermions.All(f => Math.Abs(f.Norm - 1.0) < 1e-12))
    ruledOut.Add("selected repaired fermion eigenvectors are already unit-norm");
if (generatorScale > 0.0 && repairedGeneratorMeanScale is { } requiredGenerator && requiredGenerator / generatorScale > 1e3)
    ruledOut.Add("canonical SU(2) trace-half generator normalization cannot explain a required generator scale thousands of times larger");
if (invalidRawMeanScale is { } invalidScale && invalidScale > 10.0)
    ruledOut.Add("restoring the invalid ungauged replay amplitude would still miss the target raw magnitude by more than an order of magnitude");

var plausible = new List<string>();
if (dimensionalCandidates.Any(c => c.RequiredToCandidateRatio is { } ratio && ratio is > 0.5 and < 2.0))
    plausible.Add("a dimensional lift or operator-measure factor is numerically close to the required common raw-amplitude scale");
plausible.Add("analytic Dirac-variation operator scale and boson connection-to-operator unit conversion remain unresolved");
plausible.Add("source-mode block interpretation remains unresolved because current replay consumes only the connection-space vector as delta-omega");

var terminalStatus = "wz-matrix-element-normalization-diagnostic-upstream-scale-blocked";
var result = new
{
    phaseId = "phase118-wz-matrix-element-normalization-diagnostic",
    terminalStatus,
    target = new
    {
        targetImpliedRawMatrixElementMagnitude = targetRaw,
        targetImpliedWeakCoupling = targetWeak,
        canonicalGeneratorScale = generatorScale,
    },
    repairedReplay = new
    {
        records = new[] { repairedW, repairedZ },
        rawRequiredScaleMean = repairedRawMeanScale,
        rawRequiredScaleRelativeSpread = repairedRawScaleSpread,
        generatorScaleRequiredMean = repairedGeneratorMeanScale,
        generatorScaleRequiredRelativeSpread = repairedGeneratorScaleSpread,
    },
    invalidUngaugedReplay = new
    {
        records = new[] { invalidW, invalidZ },
        rawRequiredScaleMean = invalidRawMeanScale,
        note = "This replay used ungauged high-residual fermion modes and is diagnostic only.",
    },
    vectorNorms = new
    {
        bosonSourceVectors = new[] { wVector, zVector },
        selectedFermionVectors = fermions,
    },
    phase117PairSweep = new
    {
        terminalStatus = JsonString(phase117.RootElement, "terminalStatus"),
        pairCount = JsonInt(phase117.RootElement, "pairCount"),
        admissiblePairCount = JsonInt(phase117.RootElement, "admissiblePairCount"),
        commonBridgePairCount = JsonInt(phase117.RootElement, "commonBridgePairCount"),
    },
    dimensionalScaleCandidates = dimensionalCandidates,
    ruledOutRepairClasses = ruledOut,
    plausibleRepairClasses = plausible,
    diagnosis = new[]
    {
        "changing repaired fermion pair is exhausted by Phase117",
        "unit vector normalization is not the missing factor for the persisted W/Z source vectors or selected repaired fermions",
        "canonical generator normalization is not large enough to explain the amplitude miss",
        "the remaining blocker is an upstream analytic variation/operator-source scale convention",
    },
    closureRequirements = new[]
    {
        "derive or audit the connection-mode-to-Dirac-variation operator unit scale",
        "materialize the boson source normalization convention used by Phase12 spectra as an operator-scale artifact",
        "replay Phase115 and Phase116 only after a target-independent operator/source scale is derived",
    },
    sourceEvidence = new
    {
        phase115Path = Phase115Path,
        phase114Path = Phase114Path,
        phase117Path = Phase117Path,
        phase110Path = Phase110Path,
        phase63Path = Phase63Path,
        phase95ModesPath = Phase95ModesPath,
        wModePath = WModePath,
        zModePath = ZModePath,
    },
};

var options = new JsonSerializerOptions { WriteIndented = true };
File.WriteAllText(
    Path.Combine(outputDir, "wz_matrix_element_normalization_diagnostic.json"),
    JsonSerializer.Serialize(result, options));
File.WriteAllText(
    Path.Combine(outputDir, "wz_matrix_element_normalization_diagnostic_summary.json"),
    JsonSerializer.Serialize(new
    {
        phaseId = "phase118-wz-matrix-element-normalization-diagnostic",
        terminalStatus,
        repairedRawRequiredScaleMean = repairedRawMeanScale,
        repairedRawRequiredScaleRelativeSpread = repairedRawScaleSpread,
        ruledOutRepairClasses = ruledOut,
        plausibleRepairClasses = plausible,
        result.closureRequirements,
    }, options));

Console.WriteLine(terminalStatus);
Console.WriteLine($"repairedRawRequiredScaleMean={repairedRawMeanScale:R}");
Console.WriteLine($"repairedRawRequiredScaleRelativeSpread={repairedRawScaleSpread:R}");

static JsonElement FindRecord(JsonElement root, string particleId)
{
    foreach (var record in root.GetProperty("records").EnumerateArray())
    {
        if (string.Equals(JsonString(record, "particleId"), particleId, StringComparison.Ordinal))
            return record.Clone();
    }

    throw new InvalidDataException($"Missing record for {particleId}");
}

static AmplitudeRecord BuildAmplitudeRecord(string particleId, JsonElement record, double targetRaw, double targetWeak)
{
    double raw = RequiredDouble(record, "rawMatrixElementMagnitude");
    double normalized = RequiredDouble(record, "normalizedWeakCoupling");
    return new AmplitudeRecord
    {
        ParticleId = particleId,
        RawMatrixElementMagnitude = raw,
        NormalizedWeakCoupling = normalized,
        RawToTargetRatio = raw / targetRaw,
        WeakCouplingToTargetRatio = normalized / targetWeak,
        RawRequiredScale = targetRaw / raw,
        GeneratorScaleRequired = targetWeak / raw,
    };
}

static IReadOnlyList<VectorNormRecord> BuildFermionVectorRecords(JsonElement modesRoot, IReadOnlyList<int> indices)
{
    var modes = modesRoot.GetProperty("modes").EnumerateArray().ToArray();
    return indices.Select(index => VectorNormRecord.FromArray(
            $"phase95-fermion-mode-{index}",
            JsonString(modes[index], "modeId"),
            modes[index].GetProperty("eigenvectorCoefficients").EnumerateArray().Select(x => x.GetDouble()).ToArray()))
        .ToList();
}

static ScaleCandidate ScaleCandidate(string id, double value, double? required)
{
    return new ScaleCandidate
    {
        CandidateId = id,
        CandidateValue = value,
        RequiredToCandidateRatio = required is { } r && value > 0.0 ? r / value : null,
        CandidateToRequiredRatio = required is { } r2 && r2 > 0.0 ? value / r2 : null,
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

static int? JsonInt(JsonElement element, string propertyName) =>
    element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var i)
        ? i
        : null;

static double RequiredDouble(JsonElement element, string propertyName) =>
    JsonDouble(element, propertyName) ?? throw new InvalidDataException($"Missing numeric property {propertyName}");

static double? Mean(IReadOnlyList<double?> values)
{
    var finite = values.Where(v => v is { } x && double.IsFinite(x)).Select(v => v!.Value).ToArray();
    return finite.Length == 0 ? null : finite.Average();
}

static double? RelativeSpread(double? a, double? b)
{
    if (a is not { } x || b is not { } y || !double.IsFinite(x) || !double.IsFinite(y))
        return null;

    double mean = (x + y) / 2.0;
    return Math.Abs(mean) > 0.0 ? Math.Abs(x - y) / Math.Abs(mean) : null;
}

public sealed class AmplitudeRecord
{
    [JsonPropertyName("particleId")]
    public required string ParticleId { get; init; }

    [JsonPropertyName("rawMatrixElementMagnitude")]
    public required double RawMatrixElementMagnitude { get; init; }

    [JsonPropertyName("normalizedWeakCoupling")]
    public required double NormalizedWeakCoupling { get; init; }

    [JsonPropertyName("rawToTargetRatio")]
    public required double RawToTargetRatio { get; init; }

    [JsonPropertyName("weakCouplingToTargetRatio")]
    public required double WeakCouplingToTargetRatio { get; init; }

    [JsonPropertyName("rawRequiredScale")]
    public required double RawRequiredScale { get; init; }

    [JsonPropertyName("generatorScaleRequired")]
    public required double GeneratorScaleRequired { get; init; }
}

public sealed class VectorNormRecord
{
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    [JsonPropertyName("sourcePath")]
    public string? SourcePath { get; init; }

    [JsonPropertyName("modeId")]
    public string? ModeId { get; init; }

    [JsonPropertyName("length")]
    public required int Length { get; init; }

    [JsonPropertyName("norm")]
    public required double Norm { get; init; }

    [JsonPropertyName("maxAbs")]
    public required double MaxAbs { get; init; }

    [JsonPropertyName("nonzeroCount")]
    public required int NonzeroCount { get; init; }

    public static VectorNormRecord FromModeJson(string recordId, string path, JsonElement mode)
    {
        var vector = mode.TryGetProperty("modeVector", out var modeVector)
            ? modeVector.EnumerateArray().Select(x => x.GetDouble()).ToArray()
            : mode.GetProperty("eigenvectorCoefficients").EnumerateArray().Select(x => x.GetDouble()).ToArray();

        var source = FromArray(recordId, FindString(mode, "modeId") ?? FindString(mode, "modeRecordId"), vector);
        return new VectorNormRecord
        {
            RecordId = source.RecordId,
            SourcePath = path,
            ModeId = source.ModeId,
            Length = source.Length,
            Norm = source.Norm,
            MaxAbs = source.MaxAbs,
            NonzeroCount = source.NonzeroCount,
        };
    }

    public static VectorNormRecord FromArray(string recordId, string? modeId, IReadOnlyList<double> vector)
    {
        return new VectorNormRecord
        {
            RecordId = recordId,
            ModeId = modeId,
            Length = vector.Count,
            Norm = Math.Sqrt(vector.Sum(x => x * x)),
            MaxAbs = vector.Count == 0 ? 0.0 : vector.Max(x => Math.Abs(x)),
            NonzeroCount = vector.Count(x => Math.Abs(x) > 1e-12),
        };
    }

    private static string? FindString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}

public sealed class ScaleCandidate
{
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    [JsonPropertyName("candidateValue")]
    public required double CandidateValue { get; init; }

    [JsonPropertyName("requiredToCandidateRatio")]
    public required double? RequiredToCandidateRatio { get; init; }

    [JsonPropertyName("candidateToRequiredRatio")]
    public required double? CandidateToRequiredRatio { get; init; }
}
