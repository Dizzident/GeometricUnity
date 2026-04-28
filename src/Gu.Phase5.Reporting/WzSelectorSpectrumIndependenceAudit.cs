using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzSelectorSpectrumIndependenceAuditResult
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

    [JsonPropertyName("selectedWSourceId")]
    public string? SelectedWSourceId { get; init; }

    [JsonPropertyName("selectedZSourceId")]
    public string? SelectedZSourceId { get; init; }

    [JsonPropertyName("inspectedAlignedCellCount")]
    public required int InspectedAlignedCellCount { get; init; }

    [JsonPropertyName("proxyOnlyCellCount")]
    public required int ProxyOnlyCellCount { get; init; }

    [JsonPropertyName("solverBackedCellCount")]
    public required int SolverBackedCellCount { get; init; }

    [JsonPropertyName("ratioMin")]
    public double? RatioMin { get; init; }

    [JsonPropertyName("ratioMax")]
    public double? RatioMax { get; init; }

    [JsonPropertyName("ratioMean")]
    public double? RatioMean { get; init; }

    [JsonPropertyName("ratioInvariantAcrossSelectors")]
    public required bool RatioInvariantAcrossSelectors { get; init; }

    [JsonPropertyName("selectedSourceMethods")]
    public required IReadOnlyList<string> SelectedSourceMethods { get; init; }

    [JsonPropertyName("diagnosis")]
    public required IReadOnlyList<string> Diagnosis { get; init; }

    [JsonPropertyName("closureRequirements")]
    public required IReadOnlyList<string> ClosureRequirements { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzSelectorSpectrumCellAuditRecord
{
    [JsonPropertyName("selectorKey")]
    public required string SelectorKey { get; init; }

    [JsonPropertyName("wValue")]
    public required double WValue { get; init; }

    [JsonPropertyName("zValue")]
    public required double ZValue { get; init; }

    [JsonPropertyName("ratio")]
    public required double Ratio { get; init; }

    [JsonPropertyName("proxyOnly")]
    public required bool ProxyOnly { get; init; }

    [JsonPropertyName("solverBacked")]
    public required bool SolverBacked { get; init; }
}

public static class WzSelectorSpectrumIndependenceAudit
{
    public const string AlgorithmId = "p35-wz-selector-spectrum-independence-audit:v1";

    public static WzSelectorSpectrumIndependenceAuditResult Evaluate(
        string operatorSpectrumPathDiagnosticJson,
        string candidateModeSourcesJson,
        string spectraRoot,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorSpectrumPathDiagnosticJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidateModeSourcesJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(spectraRoot);

        var pathDiagnostic = GuJsonDefaults.Deserialize<WzOperatorSpectrumPathDiagnosticResult>(operatorSpectrumPathDiagnosticJson)
            ?? throw new InvalidDataException("Failed to deserialize W/Z operator spectrum path diagnostic.");
        var sources = GuJsonDefaults.Deserialize<CandidateModeSourceBridgeTable>(candidateModeSourcesJson)
            ?? throw new InvalidDataException("Failed to deserialize candidate-mode source bridge table.");

        var closure = new List<string>();
        var selected = pathDiagnostic.SelectedPairId?.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var wSourceId = selected is { Length: >= 1 } ? selected[0] : null;
        var zSourceId = selected is { Length: >= 2 } ? selected[1] : null;
        var wCandidateId = SourceCandidateId(wSourceId);
        var zCandidateId = SourceCandidateId(zSourceId);
        if (wCandidateId is null)
            closure.Add("selected W source candidate id is missing");
        if (zCandidateId is null)
            closure.Add("selected Z source candidate id is missing");
        if (!Directory.Exists(spectraRoot))
            closure.Add($"spectra root does not exist: {spectraRoot}");

        var cells = wCandidateId is null || zCandidateId is null || !Directory.Exists(spectraRoot)
            ? []
            : LoadAlignedCells(spectraRoot, wCandidateId, zCandidateId);

        if (cells.Count == 0)
            closure.Add("no aligned selected W/Z selector spectra could be inspected");

        var methods = sources.CandidateModeSources
            .Where(s => string.Equals(s.SourceId, wSourceId, StringComparison.Ordinal) ||
                        string.Equals(s.SourceId, zSourceId, StringComparison.Ordinal))
            .Select(s => s.SourceExtractionMethod)
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(m => m, StringComparer.Ordinal)
            .ToList();
        var proxyOnlyCount = cells.Count(c => c.ProxyOnly);
        var solverBackedCount = cells.Count(c => c.SolverBacked);
        var ratioInvariant = cells.Count > 0 && cells.Max(c => c.Ratio) - cells.Min(c => c.Ratio) <= 1e-12;

        if (cells.Count > 0 && proxyOnlyCount == cells.Count)
            closure.Add("selected W/Z selector spectra are proxy-only massLikeValues records with no solver/operator evidence");
        if (cells.Count > 0 && solverBackedCount == 0)
            closure.Add("no selected W/Z selector spectrum cell is backed by an operator bundle, solver method, or mode list");
        if (ratioInvariant)
            closure.Add("selected W/Z ratio is invariant across selector cells, consistent with deterministic rescaling rather than independent eigenvalue extraction");

        var terminal = closure.Count == 0
            ? "wz-selector-spectrum-independent-evidence-present"
            : "wz-selector-spectrum-independence-blocked";

        return new WzSelectorSpectrumIndependenceAuditResult
        {
            ResultId = "phase35-wz-selector-spectrum-independence-audit-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = terminal,
            AlgorithmId = AlgorithmId,
            SelectedPairId = pathDiagnostic.SelectedPairId,
            SelectedWSourceId = wSourceId,
            SelectedZSourceId = zSourceId,
            InspectedAlignedCellCount = cells.Count,
            ProxyOnlyCellCount = proxyOnlyCount,
            SolverBackedCellCount = solverBackedCount,
            RatioMin = cells.Count == 0 ? null : cells.Min(c => c.Ratio),
            RatioMax = cells.Count == 0 ? null : cells.Max(c => c.Ratio),
            RatioMean = cells.Count == 0 ? null : cells.Average(c => c.Ratio),
            RatioInvariantAcrossSelectors = ratioInvariant,
            SelectedSourceMethods = methods,
            Diagnosis = BuildDiagnosis(cells, methods, proxyOnlyCount, solverBackedCount, ratioInvariant),
            ClosureRequirements = closure.Distinct(StringComparer.Ordinal).ToList(),
            Provenance = provenance,
        };
    }

    private static IReadOnlyList<WzSelectorSpectrumCellAuditRecord> LoadAlignedCells(
        string spectraRoot,
        string wCandidateId,
        string zCandidateId)
    {
        var wCells = LoadSpectrumCells(spectraRoot, wCandidateId);
        var zCells = LoadSpectrumCells(spectraRoot, zCandidateId);
        return wCells.Keys.Intersect(zCells.Keys, StringComparer.Ordinal)
            .OrderBy(k => k, StringComparer.Ordinal)
            .Select(k =>
            {
                var w = wCells[k];
                var z = zCells[k];
                return new WzSelectorSpectrumCellAuditRecord
                {
                    SelectorKey = k,
                    WValue = w.Value,
                    ZValue = z.Value,
                    Ratio = z.Value == 0 ? double.NaN : w.Value / z.Value,
                    ProxyOnly = w.ProxyOnly && z.ProxyOnly,
                    SolverBacked = w.SolverBacked && z.SolverBacked,
                };
            })
            .Where(c => double.IsFinite(c.Ratio))
            .ToList();
    }

    private static IReadOnlyDictionary<string, SpectrumCell> LoadSpectrumCells(string spectraRoot, string candidateId)
    {
        var result = new Dictionary<string, SpectrumCell>(StringComparer.Ordinal);
        foreach (var path in Directory.EnumerateFiles(spectraRoot, $"{candidateId}__*_spectrum.json"))
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            var root = doc.RootElement;
            if (!root.TryGetProperty("massLikeValues", out var values) ||
                values.ValueKind != JsonValueKind.Array ||
                values.GetArrayLength() == 0)
            {
                continue;
            }

            var value = values[0].GetDouble();
            var solverBacked = HasNonEmpty(root, "operatorBundleId") ||
                               HasNonEmpty(root, "solverMethod") ||
                               HasNonEmpty(root, "operatorType") ||
                               HasNonEmptyArray(root, "modes") ||
                               HasNonEmptyArray(root, "modeRecords");
            var proxyOnly = !solverBacked &&
                            HasNonEmptyArray(root, "massLikeValues") &&
                            !HasNonEmptyArray(root, "eigenvalues");
            result[SelectorKey(path, candidateId)] = new SpectrumCell(value, proxyOnly, solverBacked);
        }

        return result;
    }

    private static bool HasNonEmpty(JsonElement root, string property)
        => root.TryGetProperty(property, out var value) &&
           value.ValueKind == JsonValueKind.String &&
           !string.IsNullOrWhiteSpace(value.GetString());

    private static bool HasNonEmptyArray(JsonElement root, string property)
        => root.TryGetProperty(property, out var value) &&
           value.ValueKind == JsonValueKind.Array &&
           value.GetArrayLength() > 0;

    private static string SelectorKey(string path, string candidateId)
    {
        var file = Path.GetFileName(path);
        var prefix = $"{candidateId}__";
        var suffix = file.EndsWith("_spectrum.json", StringComparison.Ordinal)
            ? file[..^"_spectrum.json".Length]
            : file;
        return suffix.StartsWith(prefix, StringComparison.Ordinal) ? suffix[prefix.Length..] : suffix;
    }

    private static IReadOnlyList<string> BuildDiagnosis(
        IReadOnlyList<WzSelectorSpectrumCellAuditRecord> cells,
        IReadOnlyList<string> methods,
        int proxyOnlyCount,
        int solverBackedCount,
        bool ratioInvariant)
    {
        var diagnosis = new List<string>();
        diagnosis.Add($"inspected {cells.Count} aligned selected W/Z selector spectrum cell(s)");
        diagnosis.Add($"proxy-only cells: {proxyOnlyCount}; solver-backed cells: {solverBackedCount}");
        if (methods.Count > 0)
            diagnosis.Add($"selected source extraction method(s): {string.Join(", ", methods)}");
        if (ratioInvariant && cells.Count > 0)
            diagnosis.Add($"selected W/Z ratio is invariant across selector cells at mean {cells.Average(c => c.Ratio):R}");
        if (proxyOnlyCount == cells.Count && cells.Count > 0)
            diagnosis.Add("selector spectra contain massLikeValues projections but no operator bundle, solver method, or mode list");
        if (proxyOnlyCount == cells.Count && cells.Count > 0)
            diagnosis.Add("next corrective work must replace selector proxy spectra with independent selector-specific operator/eigenvalue solves before physical W/Z prediction comparison");
        else if (ratioInvariant && solverBackedCount > 0)
            diagnosis.Add("next corrective work must replace bundle-backed deterministic rescaling with selector-specific independent eigenvalue extraction before physical W/Z prediction comparison");
        else
            diagnosis.Add("selector-specific independent eigenvalue evidence is present; next corrective work can move to physical W/Z prediction calibration and comparison");
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

    private sealed record SpectrumCell(double Value, bool ProxyOnly, bool SolverBacked);
}
