using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzEnvironmentSourceClosureAuditResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("environmentCount")]
    public required int EnvironmentCount { get; init; }

    [JsonPropertyName("environmentRecordCount")]
    public required int EnvironmentRecordCount { get; init; }

    [JsonPropertyName("observableBackedCount")]
    public required int ObservableBackedCount { get; init; }

    [JsonPropertyName("backgroundBackedCount")]
    public required int BackgroundBackedCount { get; init; }

    [JsonPropertyName("environmentMappings")]
    public required IReadOnlyList<WzEnvironmentSourceClosureRecord> EnvironmentMappings { get; init; }

    [JsonPropertyName("summaryBlockers")]
    public required IReadOnlyList<string> SummaryBlockers { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzEnvironmentSourceClosureRecord
{
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("environmentRecordFound")]
    public required bool EnvironmentRecordFound { get; init; }

    [JsonPropertyName("environmentRecordPath")]
    public string? EnvironmentRecordPath { get; init; }

    [JsonPropertyName("observableBacked")]
    public required bool ObservableBacked { get; init; }

    [JsonPropertyName("observableRecordIds")]
    public required IReadOnlyList<string> ObservableRecordIds { get; init; }

    [JsonPropertyName("backgroundRecordFound")]
    public required bool BackgroundRecordFound { get; init; }

    [JsonPropertyName("backgroundRecordIds")]
    public required IReadOnlyList<string> BackgroundRecordIds { get; init; }

    [JsonPropertyName("backgroundRecordPaths")]
    public required IReadOnlyList<string> BackgroundRecordPaths { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}

public static class WzEnvironmentSourceClosureAudit
{
    public const string AlgorithmId = "p38-wz-environment-source-closure-audit:v1";

    public static WzEnvironmentSourceClosureAuditResult Evaluate(
        string campaignSpecJson,
        IReadOnlyList<string> environmentRecordPaths,
        string observablesJson,
        IReadOnlyList<string> backgroundRecordPaths,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(campaignSpecJson);
        ArgumentNullException.ThrowIfNull(environmentRecordPaths);
        ArgumentException.ThrowIfNullOrWhiteSpace(observablesJson);
        ArgumentNullException.ThrowIfNull(backgroundRecordPaths);

        var spec = GuJsonDefaults.Deserialize<InternalVectorBosonSourceSpectrumCampaignSpec>(campaignSpecJson)
            ?? throw new InvalidDataException("Failed to deserialize selector source spectrum campaign spec.");
        var environmentRecords = ReadEnvironmentRecords(environmentRecordPaths);
        var observableRecords = ReadObservableRecords(observablesJson);
        var backgroundRecords = ReadBackgroundRecords(backgroundRecordPaths);

        var mappings = spec.EnvironmentIds
            .Select(environmentId => BuildMapping(environmentId, environmentRecords, observableRecords, backgroundRecords))
            .ToList();
        var blockers = BuildBlockers(mappings);

        return new WzEnvironmentSourceClosureAuditResult
        {
            ResultId = "phase38-wz-environment-source-closure-audit-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = blockers.Count == 0
                ? "environment-source-closure-complete"
                : "environment-source-closure-blocked",
            AlgorithmId = AlgorithmId,
            EnvironmentCount = mappings.Count,
            EnvironmentRecordCount = mappings.Count(m => m.EnvironmentRecordFound),
            ObservableBackedCount = mappings.Count(m => m.ObservableBacked),
            BackgroundBackedCount = mappings.Count(m => m.BackgroundRecordFound),
            EnvironmentMappings = mappings,
            SummaryBlockers = blockers,
            Provenance = provenance,
        };
    }

    private static WzEnvironmentSourceClosureRecord BuildMapping(
        string environmentId,
        IReadOnlyList<EnvironmentRecordRef> environmentRecords,
        IReadOnlyList<ObservableRecordRef> observableRecords,
        IReadOnlyList<BackgroundRecordRef> backgroundRecords)
    {
        var environmentRecord = environmentRecords.FirstOrDefault(r => r.EnvironmentId == environmentId);
        var observables = observableRecords
            .Where(r => r.EnvironmentId == environmentId)
            .OrderBy(r => r.RecordId, StringComparer.Ordinal)
            .ToList();
        var backgrounds = backgroundRecords
            .Where(r => r.EnvironmentId == environmentId)
            .OrderBy(r => r.RecordId, StringComparer.Ordinal)
            .ToList();

        var blockers = new List<string>();
        if (environmentRecord is null)
            blockers.Add($"environment selector '{environmentId}' has no environment record");
        if (observables.Count == 0)
            blockers.Add($"environment selector '{environmentId}' has no computed observable support");
        if (backgrounds.Count == 0)
            blockers.Add($"environment selector '{environmentId}' has no persisted solver-backed background record");

        return new WzEnvironmentSourceClosureRecord
        {
            EnvironmentId = environmentId,
            EnvironmentRecordFound = environmentRecord is not null,
            EnvironmentRecordPath = environmentRecord?.Path,
            ObservableBacked = observables.Count > 0,
            ObservableRecordIds = observables.Select(o => o.RecordId).ToList(),
            BackgroundRecordFound = backgrounds.Count > 0,
            BackgroundRecordIds = backgrounds.Select(b => b.RecordId).ToList(),
            BackgroundRecordPaths = backgrounds.Select(b => b.Path).Distinct(StringComparer.Ordinal).ToList(),
            Blockers = blockers,
        };
    }

    private static IReadOnlyList<string> BuildBlockers(IReadOnlyList<WzEnvironmentSourceClosureRecord> mappings)
    {
        var blockers = new List<string>();
        if (mappings.Any(m => !m.EnvironmentRecordFound))
            blockers.Add($"{mappings.Count(m => !m.EnvironmentRecordFound)} environment selector(s) lack environment records");
        if (mappings.Any(m => !m.ObservableBacked))
            blockers.Add($"{mappings.Count(m => !m.ObservableBacked)} environment selector(s) lack observable support");
        if (mappings.Any(m => !m.BackgroundRecordFound))
            blockers.Add($"{mappings.Count(m => !m.BackgroundRecordFound)} environment selector(s) lack persisted solver-backed background records");
        return blockers;
    }

    private static IReadOnlyList<EnvironmentRecordRef> ReadEnvironmentRecords(IReadOnlyList<string> paths)
        => paths.SelectMany(path =>
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                var environmentId = OptionalString(doc.RootElement, "environmentId");
                return string.IsNullOrWhiteSpace(environmentId)
                    ? []
                    : new[] { new EnvironmentRecordRef(environmentId, path) };
            })
            .ToList();

    private static IReadOnlyList<ObservableRecordRef> ReadObservableRecords(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var records = doc.RootElement.ValueKind == JsonValueKind.Array
            ? doc.RootElement.EnumerateArray()
            : doc.RootElement.TryGetProperty("records", out var recordsElement) && recordsElement.ValueKind == JsonValueKind.Array
                ? recordsElement.EnumerateArray()
                : [];
        return records
            .Select(e => new ObservableRecordRef(
                OptionalString(e, "environmentId") ?? string.Empty,
                OptionalString(e, "recordId") ?? OptionalString(e, "observableId") ?? "unknown-observable-record"))
            .Where(r => !string.IsNullOrWhiteSpace(r.EnvironmentId))
            .ToList();
    }

    private static IReadOnlyList<BackgroundRecordRef> ReadBackgroundRecords(IReadOnlyList<string> paths)
        => paths.SelectMany(ReadBackgroundRecordsFromPath).ToList();

    private static IReadOnlyList<BackgroundRecordRef> ReadBackgroundRecordsFromPath(string path)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
            return ReadBackgroundRecordArray(doc.RootElement, path);

        if (doc.RootElement.TryGetProperty("backgrounds", out var backgrounds) && backgrounds.ValueKind == JsonValueKind.Array)
            return ReadBackgroundRecordArray(backgrounds, path);

        var record = ReadBackgroundRecordObject(doc.RootElement, path);
        return record is null ? [] : [record];
    }

    private static IReadOnlyList<BackgroundRecordRef> ReadBackgroundRecordArray(JsonElement array, string path)
        => array.EnumerateArray()
            .Select(e => ReadBackgroundRecordObject(e, path))
            .Where(r => r is not null)
            .Cast<BackgroundRecordRef>()
            .ToList();

    private static BackgroundRecordRef? ReadBackgroundRecordObject(JsonElement element, string path)
    {
        var environmentId = OptionalString(element, "environmentId");
        if (string.IsNullOrWhiteSpace(environmentId))
            return null;

        var recordId =
            OptionalString(element, "recordId") ??
            OptionalString(element, "backgroundRecordId") ??
            OptionalString(element, "backgroundId") ??
            Path.GetFileNameWithoutExtension(path);
        return new BackgroundRecordRef(environmentId, recordId, path);
    }

    private static string? OptionalString(JsonElement root, string property)
        => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private sealed record EnvironmentRecordRef(string EnvironmentId, string Path);

    private sealed record ObservableRecordRef(string EnvironmentId, string RecordId);

    private sealed record BackgroundRecordRef(string EnvironmentId, string RecordId, string Path);
}
