using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzOperatorSpectrumPathDiagnosticResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("selectedPairId")]
    public string? SelectedPairId { get; init; }

    [JsonPropertyName("targetValue")]
    public required double TargetValue { get; init; }

    [JsonPropertyName("requiredScaleToTarget")]
    public double? RequiredScaleToTarget { get; init; }

    [JsonPropertyName("layerRatios")]
    public required IReadOnlyList<WzOperatorSpectrumLayerRatioRecord> LayerRatios { get; init; }

    [JsonPropertyName("alignedSpectrumPointCount")]
    public required int AlignedSpectrumPointCount { get; init; }

    [JsonPropertyName("spectrumRatioMin")]
    public double? SpectrumRatioMin { get; init; }

    [JsonPropertyName("spectrumRatioMax")]
    public double? SpectrumRatioMax { get; init; }

    [JsonPropertyName("spectrumRatioMean")]
    public double? SpectrumRatioMean { get; init; }

    [JsonPropertyName("firstMismatchLayer")]
    public string? FirstMismatchLayer { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzOperatorSpectrumLayerRatioRecord
{
    [JsonPropertyName("layerId")]
    public required string LayerId { get; init; }

    [JsonPropertyName("wValue")]
    public required double WValue { get; init; }

    [JsonPropertyName("zValue")]
    public required double ZValue { get; init; }

    [JsonPropertyName("ratio")]
    public required double Ratio { get; init; }

    [JsonPropertyName("targetDelta")]
    public required double TargetDelta { get; init; }

    [JsonPropertyName("requiredScaleToTarget")]
    public required double RequiredScaleToTarget { get; init; }

    [JsonPropertyName("matchesCandidateModeSourceRatio")]
    public required bool MatchesCandidateModeSourceRatio { get; init; }
}

public static class WzOperatorSpectrumPathDiagnostic
{
    public const string AlgorithmId = "p34-wz-operator-spectrum-path-diagnostic:v1";

    public static WzOperatorSpectrumPathDiagnosticResult Evaluate(
        string p33NormalizationClosureJson,
        string candidateModeSourcesJson,
        string sourceCandidatesJson,
        string modeFamiliesJson,
        string spectraRoot,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(p33NormalizationClosureJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateModeSourcesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCandidatesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(modeFamiliesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(spectraRoot);

        var p33Closure = GuJsonDefaults.Deserialize<WzNormalizationClosureDiagnosticResult>(p33NormalizationClosureJson)
            ?? throw new InvalidDataException("Failed to deserialize W/Z normalization closure diagnostic.");
        var sources = GuJsonDefaults.Deserialize<CandidateModeSourceBridgeTable>(candidateModeSourcesJson)
            ?? throw new InvalidDataException("Failed to deserialize candidate-mode source bridge table.");
        using var sourceCandidates = JsonDocument.Parse(sourceCandidatesJson);
        using var modeFamilies = JsonDocument.Parse(modeFamiliesJson);

        var closure = new List<string>();
        var wSourceId = p33Closure.SelectedPairId?.Split('/', StringSplitOptions.TrimEntries).FirstOrDefault();
        var zSourceId = p33Closure.SelectedPairId?.Split('/', StringSplitOptions.TrimEntries).Skip(1).FirstOrDefault();
        var wCandidateId = SourceCandidateId(wSourceId);
        var zCandidateId = SourceCandidateId(zSourceId);
        if (wCandidateId is null)
            closure.Add("selected W source candidate id is missing");
        if (zCandidateId is null)
            closure.Add("selected Z source candidate id is missing");
        if (!Directory.Exists(spectraRoot))
            closure.Add($"spectra root does not exist: {spectraRoot}");

        var layers = new List<WzOperatorSpectrumLayerRatioRecord>();
        if (wCandidateId is not null && zCandidateId is not null)
        {
            AddLayerFromCandidateModeSources(layers, sources.CandidateModeSources, wSourceId, zSourceId, p33Closure.TargetValue);
            AddLayerFromJsonArray(layers, sourceCandidates.RootElement, "source-candidates", "candidates", "sourceCandidateId", "massLikeValue", wCandidateId, zCandidateId, p33Closure.TargetValue);
            AddLayerFromJsonArray(layers, modeFamilies.RootElement, "mode-families", "families", "sourceCandidateId", "massLikeValue", wCandidateId, zCandidateId, p33Closure.TargetValue);
        }

        if (layers.Count == 0)
            closure.Add("no selected W/Z ratio layers could be reconstructed");

        var spectrumRatios = wCandidateId is null || zCandidateId is null || !Directory.Exists(spectraRoot)
            ? []
            : LoadAlignedSpectrumRatios(spectraRoot, wCandidateId, zCandidateId);
        var sourceRatio = layers.FirstOrDefault(l => l.LayerId == "candidate-mode-sources")?.Ratio;
        var firstMismatch = layers.FirstOrDefault(l => !l.MatchesCandidateModeSourceRatio)?.LayerId;
        var diagnosis = BuildDiagnosis(layers, spectrumRatios, p33Closure.TargetValue, sourceRatio, firstMismatch);

        return new WzOperatorSpectrumPathDiagnosticResult
        {
            ResultId = "phase34-wz-operator-spectrum-path-diagnostic-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = closure.Count == 0 ? "wz-operator-spectrum-path-diagnostic-complete" : "wz-operator-spectrum-path-diagnostic-blocked",
            AlgorithmId = AlgorithmId,
            SelectedPairId = p33Closure.SelectedPairId,
            TargetValue = p33Closure.TargetValue,
            RequiredScaleToTarget = p33Closure.RequiredScaleToTarget,
            LayerRatios = layers,
            AlignedSpectrumPointCount = spectrumRatios.Count,
            SpectrumRatioMin = spectrumRatios.Count == 0 ? null : spectrumRatios.Min(),
            SpectrumRatioMax = spectrumRatios.Count == 0 ? null : spectrumRatios.Max(),
            SpectrumRatioMean = spectrumRatios.Count == 0 ? null : spectrumRatios.Average(),
            FirstMismatchLayer = firstMismatch,
            Diagnosis = diagnosis,
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static void AddLayerFromCandidateModeSources(
        List<WzOperatorSpectrumLayerRatioRecord> layers,
        IReadOnlyList<CandidateModeSourceRecord> sources,
        string? wSourceId,
        string? zSourceId,
        double target)
    {
        var w = sources.FirstOrDefault(s => string.Equals(s.SourceId, wSourceId, StringComparison.Ordinal));
        var z = sources.FirstOrDefault(s => string.Equals(s.SourceId, zSourceId, StringComparison.Ordinal));
        if (w is not null && z is not null)
            layers.Add(CreateLayer("candidate-mode-sources", w.Value, z.Value, target, null));
    }

    private static void AddLayerFromJsonArray(
        List<WzOperatorSpectrumLayerRatioRecord> layers,
        JsonElement root,
        string layerId,
        string arrayProperty,
        string idProperty,
        string valueProperty,
        string wCandidateId,
        string zCandidateId,
        double target)
    {
        if (!root.TryGetProperty(arrayProperty, out var array) || array.ValueKind != JsonValueKind.Array)
            return;
        double? w = null;
        double? z = null;
        foreach (var item in array.EnumerateArray())
        {
            if (!item.TryGetProperty(idProperty, out var idElement) || idElement.ValueKind != JsonValueKind.String ||
                !item.TryGetProperty(valueProperty, out var valueElement) || !valueElement.TryGetDouble(out var value))
            {
                continue;
            }

            var id = SourceCandidateId(idElement.GetString());
            if (string.Equals(id, wCandidateId, StringComparison.Ordinal))
                w = value;
            if (string.Equals(id, zCandidateId, StringComparison.Ordinal))
                z = value;
        }

        if (w is not null && z is not null)
            layers.Add(CreateLayer(layerId, w.Value, z.Value, target, layers.FirstOrDefault()?.Ratio));
    }

    private static WzOperatorSpectrumLayerRatioRecord CreateLayer(
        string layerId,
        double w,
        double z,
        double target,
        double? sourceRatio)
    {
        var ratio = w / z;
        return new WzOperatorSpectrumLayerRatioRecord
        {
            LayerId = layerId,
            WValue = w,
            ZValue = z,
            Ratio = ratio,
            TargetDelta = ratio - target,
            RequiredScaleToTarget = target / ratio,
            MatchesCandidateModeSourceRatio = sourceRatio is null || System.Math.Abs(ratio - sourceRatio.Value) <= 1e-12,
        };
    }

    private static IReadOnlyList<double> LoadAlignedSpectrumRatios(string spectraRoot, string wCandidateId, string zCandidateId)
    {
        var wSpectra = LoadSpectra(spectraRoot, wCandidateId);
        var zSpectra = LoadSpectra(spectraRoot, zCandidateId);
        return wSpectra.Keys.Intersect(zSpectra.Keys, StringComparer.Ordinal)
            .OrderBy(k => k, StringComparer.Ordinal)
            .Select(k => wSpectra[k] / zSpectra[k])
            .ToList();
    }

    private static IReadOnlyDictionary<string, double> LoadSpectra(string spectraRoot, string candidateId)
    {
        var result = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var path in Directory.EnumerateFiles(spectraRoot, $"{candidateId}__*_spectrum.json"))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            if (!root.TryGetProperty("branchVariantId", out var branch) ||
                !root.TryGetProperty("refinementLevel", out var refinement) ||
                !root.TryGetProperty("environmentId", out var environment) ||
                !root.TryGetProperty("massLikeValues", out var values) ||
                values.ValueKind != JsonValueKind.Array ||
                values.GetArrayLength() == 0 ||
                !values[0].TryGetDouble(out var value))
            {
                continue;
            }

            result[$"{branch.GetString()}::{refinement.GetString()}::{environment.GetString()}"] = value;
        }

        return result;
    }

    private static IReadOnlyList<string> BuildDiagnosis(
        IReadOnlyList<WzOperatorSpectrumLayerRatioRecord> layers,
        IReadOnlyList<double> spectrumRatios,
        double target,
        double? sourceRatio,
        string? firstMismatch)
    {
        var diagnosis = new List<string>();
        if (sourceRatio is not null)
            diagnosis.Add($"candidate-mode source ratio is {sourceRatio.Value:R}, target delta {(sourceRatio.Value - target):R}");
        if (firstMismatch is null && layers.Count > 1)
            diagnosis.Add("candidate-mode sources, source candidates, and mode families carry the same selected W/Z ratio");
        if (spectrumRatios.Count > 0)
            diagnosis.Add($"aligned spectrum files carry ratio envelope [{spectrumRatios.Min():R}, {spectrumRatios.Max():R}] across {spectrumRatios.Count} points");
        if (firstMismatch is null && spectrumRatios.Count > 0 && sourceRatio is not null &&
            spectrumRatios.All(r => System.Math.Abs(r - sourceRatio.Value) <= 1e-12))
        {
            diagnosis.Add("the mismatch is already present in the per-point spectrum/mode eigenvalues, before Phase22 aggregation or physical promotion");
        }
        diagnosis.Add("next corrective work should inspect the upstream mass-like operator/eigenvalue extraction rather than normalization");
        return diagnosis;
    }

    private static string? SourceCandidateId(string? sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceId))
            return null;
        var trimmed = sourceId.Trim();
        if (trimmed.StartsWith("phase22-", StringComparison.Ordinal))
            trimmed = trimmed["phase22-".Length..];
        if (System.Text.RegularExpressions.Regex.Match(trimmed, @"candidate-\d+") is { Success: true } match)
            return $"phase12-{match.Value}";
        return trimmed.StartsWith("phase12-candidate-", StringComparison.Ordinal) ? trimmed : null;
    }
}
