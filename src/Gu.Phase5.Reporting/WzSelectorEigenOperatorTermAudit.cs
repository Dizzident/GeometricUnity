using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Phase5.Reporting;

public sealed class WzSelectorEigenOperatorTermAuditResult
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
    public required string SelectedPairId { get; init; }

    [JsonPropertyName("requiredScaleToTarget")]
    public required double RequiredScaleToTarget { get; init; }

    [JsonPropertyName("requiredRatioShiftFraction")]
    public required double RequiredRatioShiftFraction { get; init; }

    [JsonPropertyName("selectorRatioMin")]
    public double? SelectorRatioMin { get; init; }

    [JsonPropertyName("selectorRatioMax")]
    public double? SelectorRatioMax { get; init; }

    [JsonPropertyName("inspectedSpectrumCount")]
    public required int InspectedSpectrumCount { get; init; }

    [JsonPropertyName("solverBackedSpectrumCount")]
    public required int SolverBackedSpectrumCount { get; init; }

    [JsonPropertyName("nonTrivialOperatorTermEvidenceCount")]
    public required int NonTrivialOperatorTermEvidenceCount { get; init; }

    [JsonPropertyName("observedOperatorTypes")]
    public required IReadOnlyList<string> ObservedOperatorTypes { get; init; }

    [JsonPropertyName("observedSolverMethods")]
    public required IReadOnlyList<string> ObservedSolverMethods { get; init; }

    [JsonPropertyName("observedModeBlocks")]
    public required IReadOnlyList<string> ObservedModeBlocks { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public static class WzSelectorEigenOperatorTermAudit
{
    public const string AlgorithmId = "p45-wz-selector-eigen-operator-term-audit:v1";

    public static WzSelectorEigenOperatorTermAuditResult Evaluate(
        string ratioDiagnosticJson,
        string selectorVariationDiagnosticJson,
        string spectraRoot,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ratioDiagnosticJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectorVariationDiagnosticJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(spectraRoot);

        using var ratioDoc = JsonDocument.Parse(ratioDiagnosticJson);
        using var selectorDoc = JsonDocument.Parse(selectorVariationDiagnosticJson);

        var selectedPair = ratioDoc.RootElement.GetProperty("selectedPair");
        var pairId = selectedPair.GetProperty("pairId").GetString() ?? string.Empty;
        var requiredScale = selectedPair.GetProperty("requiredScaleToTarget").GetDouble();
        var requiredShift = requiredScale - 1.0;
        var selectorMin = TryGetDouble(selectorDoc.RootElement, "ratioMin");
        var selectorMax = TryGetDouble(selectorDoc.RootElement, "ratioMax");

        var selectedIds = pairId.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(SourceCandidateId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);
        var closure = new List<string>();
        if (selectedIds.Count < 2)
            closure.Add("selected W/Z source candidate ids could not be resolved");
        if (!Directory.Exists(spectraRoot))
            closure.Add($"spectra root does not exist: {spectraRoot}");

        var records = closure.Count == 0
            ? LoadSelectedSpectra(spectraRoot, selectedIds)
            : [];
        var operatorTypes = records
            .Select(r => r.OperatorType)
            .OfType<string>()
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
        var solverMethods = records
            .Select(r => r.SolverMethod)
            .OfType<string>()
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
        var modeBlocks = records
            .SelectMany(r => r.ModeBlocks)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(s => s, StringComparer.Ordinal)
            .ToList();
        var solverBackedCount = records.Count(r => r.SolverBacked);
        var termEvidenceCount = records.Count(r => r.HasNonTrivialOperatorTermEvidence);

        if (records.Count == 0)
            closure.Add("no selected W/Z selector-eigen spectra could be inspected");
        if (records.Count > 0 && solverBackedCount == 0)
            closure.Add("selected W/Z spectra do not include solver-backed operator evidence");
        if (records.Count > 0 && termEvidenceCount == 0 && System.Math.Abs(requiredShift) > 1e-12)
            closure.Add("no target-independent electroweak, mixing, or nontrivial mass-operator term evidence was emitted for the selected W/Z spectra");
        if (records.Count > 0 && modeBlocks.SequenceEqual(["connection"]))
            closure.Add("selected W/Z mode energy is entirely in the connection block; no electroweak or mixing block participates in the emitted modes");

        return new WzSelectorEigenOperatorTermAuditResult
        {
            ResultId = "phase45-wz-selector-eigen-operator-term-audit-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = closure.Count == 0
                ? "wz-selector-eigen-operator-term-ready"
                : "wz-selector-eigen-operator-term-blocked",
            AlgorithmId = AlgorithmId,
            SelectedPairId = pairId,
            RequiredScaleToTarget = requiredScale,
            RequiredRatioShiftFraction = requiredShift,
            SelectorRatioMin = selectorMin,
            SelectorRatioMax = selectorMax,
            InspectedSpectrumCount = records.Count,
            SolverBackedSpectrumCount = solverBackedCount,
            NonTrivialOperatorTermEvidenceCount = termEvidenceCount,
            ObservedOperatorTypes = operatorTypes,
            ObservedSolverMethods = solverMethods,
            ObservedModeBlocks = modeBlocks,
            Diagnosis = BuildDiagnosis(requiredScale, requiredShift, selectorMin, selectorMax, records, termEvidenceCount, modeBlocks),
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static IReadOnlyList<SelectorEigenSpectrumTermRecord> LoadSelectedSpectra(
        string spectraRoot,
        IReadOnlySet<string> selectedCandidateIds)
    {
        var records = new List<SelectorEigenSpectrumTermRecord>();
        foreach (var candidateId in selectedCandidateIds.OrderBy(id => id, StringComparer.Ordinal))
        foreach (var path in Directory.EnumerateFiles(spectraRoot, $"{candidateId}__*_spectrum.json"))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            var text = root.GetRawText();
            var operatorType = TryGetString(root, "operatorType") ?? TryGetString(root, "spectrumBundle", "operatorType");
            var solverMethod = TryGetString(root, "solverMethod") ?? TryGetString(root, "spectrumBundle", "solverMethod");
            var modeBlocks = ReadModeBlocks(root);
            records.Add(new SelectorEigenSpectrumTermRecord(
                Path.GetFileName(path),
                operatorType,
                solverMethod,
                HasNonEmpty(root, "operatorBundleId") || HasNonEmpty(root, "spectrumBundle"),
                HasNonTrivialTermEvidence(text, modeBlocks),
                modeBlocks));
        }

        return records;
    }

    private static bool HasNonTrivialTermEvidence(string spectrumJson, IReadOnlyList<string> modeBlocks)
    {
        var hasNamedTerm = spectrumJson.Contains("electroweak", StringComparison.OrdinalIgnoreCase) ||
            spectrumJson.Contains("mixing", StringComparison.OrdinalIgnoreCase) ||
            spectrumJson.Contains("charge", StringComparison.OrdinalIgnoreCase) ||
            spectrumJson.Contains("normalizationScaleFactor", StringComparison.OrdinalIgnoreCase) ||
            spectrumJson.Contains("operatorNormalizationDerivationId", StringComparison.OrdinalIgnoreCase) ||
            spectrumJson.Contains("nontrivial-mass", StringComparison.OrdinalIgnoreCase);
        var hasNonConnectionBlock = modeBlocks.Any(b => !string.Equals(b, "connection", StringComparison.Ordinal));
        return hasNamedTerm || hasNonConnectionBlock;
    }

    private static IReadOnlyList<string> ReadModeBlocks(JsonElement root)
    {
        var result = new SortedSet<string>(StringComparer.Ordinal);
        ReadModeBlocks(root, "modeRecords", result);
        if (root.TryGetProperty("spectrumBundle", out var bundle))
            ReadModeBlocks(bundle, "modes", result);
        return result.ToList();
    }

    private static void ReadModeBlocks(JsonElement root, string propertyName, ISet<string> result)
    {
        if (!root.TryGetProperty(propertyName, out var modes) || modes.ValueKind != JsonValueKind.Array)
            return;
        foreach (var mode in modes.EnumerateArray())
        {
            if (!mode.TryGetProperty("blockEnergyFractions", out var blocks) ||
                blocks.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            foreach (var block in blocks.EnumerateObject())
                result.Add(block.Name);
        }
    }

    private static IReadOnlyList<string> BuildDiagnosis(
        double requiredScale,
        double requiredShift,
        double? selectorMin,
        double? selectorMax,
        IReadOnlyList<SelectorEigenSpectrumTermRecord> records,
        int termEvidenceCount,
        IReadOnlyList<string> modeBlocks)
    {
        var diagnosis = new List<string>
        {
            $"selected pair requires target-independent ratio scale {requiredScale:R}, a fractional shift of {requiredShift:R}",
        };
        if (selectorMin is not null && selectorMax is not null)
            diagnosis.Add($"selector ratio envelope is [{selectorMin.Value:R}, {selectorMax.Value:R}]");
        diagnosis.Add($"inspected {records.Count} selected W/Z selector-eigen spectrum artifact(s)");
        diagnosis.Add($"{records.Count(r => r.SolverBacked)} inspected spectrum artifact(s) are solver-backed");
        if (termEvidenceCount == 0)
            diagnosis.Add("emitted selector-eigen spectra contain no electroweak, mixing, charge-sector, normalization-scale, or nontrivial mass-operator term evidence");
        else
            diagnosis.Add($"{termEvidenceCount} emitted spectrum artifact(s) contain nontrivial operator-term evidence");
        if (modeBlocks.Count > 0)
            diagnosis.Add($"observed emitted mode energy block(s): {string.Join(", ", modeBlocks)}");
        return diagnosis;
    }

    private static string SourceCandidateId(string sourceId)
        => sourceId.StartsWith("phase22-", StringComparison.Ordinal)
            ? sourceId["phase22-".Length..]
            : sourceId;

    private static string? TryGetString(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? TryGetString(JsonElement root, string parentName, string propertyName)
        => root.TryGetProperty(parentName, out var parent) ? TryGetString(parent, propertyName) : null;

    private static double? TryGetDouble(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : null;

    private static bool HasNonEmpty(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return false;
        return value.ValueKind switch
        {
            JsonValueKind.String => !string.IsNullOrWhiteSpace(value.GetString()),
            JsonValueKind.Object => value.EnumerateObject().Any(),
            JsonValueKind.Array => value.GetArrayLength() > 0,
            _ => false,
        };
    }

    private sealed record SelectorEigenSpectrumTermRecord(
        string FileName,
        string? OperatorType,
        string? SolverMethod,
        bool SolverBacked,
        bool HasNonTrivialOperatorTermEvidence,
        IReadOnlyList<string> ModeBlocks);
}
