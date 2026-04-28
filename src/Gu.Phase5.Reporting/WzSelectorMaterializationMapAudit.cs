using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzSelectorMaterializationMapAuditResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("branchVariantCount")]
    public required int BranchVariantCount { get; init; }

    [JsonPropertyName("mappedBranchVariantCount")]
    public required int MappedBranchVariantCount { get; init; }

    [JsonPropertyName("refinementLevelCount")]
    public required int RefinementLevelCount { get; init; }

    [JsonPropertyName("mappedRefinementLevelCount")]
    public required int MappedRefinementLevelCount { get; init; }

    [JsonPropertyName("environmentCount")]
    public required int EnvironmentCount { get; init; }

    [JsonPropertyName("mappedEnvironmentCount")]
    public required int MappedEnvironmentCount { get; init; }

    [JsonPropertyName("branchMappings")]
    public required IReadOnlyList<WzSelectorAxisMappingRecord> BranchMappings { get; init; }

    [JsonPropertyName("refinementMappings")]
    public required IReadOnlyList<WzSelectorAxisMappingRecord> RefinementMappings { get; init; }

    [JsonPropertyName("environmentMappings")]
    public required IReadOnlyList<WzSelectorAxisMappingRecord> EnvironmentMappings { get; init; }

    [JsonPropertyName("summaryBlockers")]
    public required IReadOnlyList<string> SummaryBlockers { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzSelectorAxisMappingRecord
{
    [JsonPropertyName("selectorId")]
    public required string SelectorId { get; init; }

    [JsonPropertyName("mapped")]
    public required bool Mapped { get; init; }

    [JsonPropertyName("sourceRecordId")]
    public string? SourceRecordId { get; init; }

    [JsonPropertyName("sourceArtifactRef")]
    public string? SourceArtifactRef { get; init; }

    [JsonPropertyName("evidenceKind")]
    public string? EvidenceKind { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}

public static class WzSelectorMaterializationMapAudit
{
    public const string AlgorithmId = "p37-wz-selector-materialization-map-audit:v1";

    public static WzSelectorMaterializationMapAuditResult Evaluate(
        string campaignSpecJson,
        string bridgeManifestJson,
        string refinementEvidenceManifestJson,
        string environmentCampaignJson,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(campaignSpecJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(bridgeManifestJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(refinementEvidenceManifestJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentCampaignJson);

        var spec = GuJsonDefaults.Deserialize<InternalVectorBosonSourceSpectrumCampaignSpec>(campaignSpecJson)
            ?? throw new InvalidDataException("Failed to deserialize selector source spectrum campaign spec.");
        using var bridgeDoc = JsonDocument.Parse(bridgeManifestJson);
        using var refinementDoc = JsonDocument.Parse(refinementEvidenceManifestJson);
        using var environmentDoc = JsonDocument.Parse(environmentCampaignJson);

        var branchMappings = BuildOrderedMappings(
            spec.BranchVariantIds,
            ReadStringArray(bridgeDoc.RootElement, "derivedVariantIds"),
            ReadStringArray(bridgeDoc.RootElement, "sourceRecordIds"),
            ReadStringArray(bridgeDoc.RootElement, "sourceStateArtifactRefs"),
            "bridge-atlas-source-background");
        var refinementMappings = BuildOrderedMappings(
            spec.RefinementLevels,
            ReadStringArray(refinementDoc.RootElement, "sourceRecordIds").Select(InferRefinementLevel).ToList(),
            ReadStringArray(refinementDoc.RootElement, "sourceRecordIds"),
            ReadStringArray(refinementDoc.RootElement, "sourceArtifactRefs"),
            OptionalString(refinementDoc.RootElement, "evidenceSource") ?? "refinement-source-background");
        var environmentMappings = BuildEnvironmentMappings(
            spec.EnvironmentIds,
            ReadStringArray(environmentDoc.RootElement, "environmentIds"));

        var blockers = BuildBlockers(branchMappings, refinementMappings, environmentMappings);
        return new WzSelectorMaterializationMapAuditResult
        {
            ResultId = "phase37-wz-selector-materialization-map-audit-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = blockers.Count == 0
                ? "selector-materialization-map-complete"
                : "selector-materialization-map-blocked",
            AlgorithmId = AlgorithmId,
            BranchVariantCount = branchMappings.Count,
            MappedBranchVariantCount = branchMappings.Count(m => m.Mapped),
            RefinementLevelCount = refinementMappings.Count,
            MappedRefinementLevelCount = refinementMappings.Count(m => m.Mapped),
            EnvironmentCount = environmentMappings.Count,
            MappedEnvironmentCount = environmentMappings.Count(m => m.Mapped),
            BranchMappings = branchMappings,
            RefinementMappings = refinementMappings,
            EnvironmentMappings = environmentMappings,
            SummaryBlockers = blockers,
            Provenance = provenance,
        };
    }

    private static IReadOnlyList<WzSelectorAxisMappingRecord> BuildOrderedMappings(
        IReadOnlyList<string> selectors,
        IReadOnlyList<string> mappedSelectors,
        IReadOnlyList<string> sourceRecordIds,
        IReadOnlyList<string> sourceArtifactRefs,
        string evidenceKind)
    {
        return selectors.Select(selector =>
        {
            var index = Enumerable.Range(0, mappedSelectors.Count)
                .FirstOrDefault(i => SelectorMatches(mappedSelectors[i], selector), -1);
            return index >= 0
                ? new WzSelectorAxisMappingRecord
                {
                    SelectorId = selector,
                    Mapped = true,
                    SourceRecordId = ElementAtOrNull(sourceRecordIds, index),
                    SourceArtifactRef = ElementAtOrNull(sourceArtifactRefs, index),
                    EvidenceKind = evidenceKind,
                    Blockers = [],
                }
                : new WzSelectorAxisMappingRecord
                {
                    SelectorId = selector,
                    Mapped = false,
                    EvidenceKind = evidenceKind,
                    Blockers = [$"no solver-backed source record maps selector '{selector}'"],
                };
        }).ToList();
    }

    private static IReadOnlyList<WzSelectorAxisMappingRecord> BuildEnvironmentMappings(
        IReadOnlyList<string> selectors,
        IReadOnlyList<string> declaredEnvironmentIds)
    {
        return selectors.Select(selector =>
        {
            var declared = declaredEnvironmentIds.Contains(selector, StringComparer.Ordinal);
            return new WzSelectorAxisMappingRecord
            {
                SelectorId = selector,
                Mapped = false,
                EvidenceKind = declared ? "declared-environment-only" : "missing-environment-declaration",
                Blockers = declared
                    ? [$"environment selector '{selector}' is declared but has no solver-backed background record map"]
                    : [$"environment selector '{selector}' is not declared and has no solver-backed background record map"],
            };
        }).ToList();
    }

    private static IReadOnlyList<string> BuildBlockers(
        IReadOnlyList<WzSelectorAxisMappingRecord> branchMappings,
        IReadOnlyList<WzSelectorAxisMappingRecord> refinementMappings,
        IReadOnlyList<WzSelectorAxisMappingRecord> environmentMappings)
    {
        var blockers = new List<string>();
        if (branchMappings.Any(m => !m.Mapped))
            blockers.Add($"{branchMappings.Count(m => !m.Mapped)} branch variant selector(s) lack source background mappings");
        if (refinementMappings.Any(m => !m.Mapped))
            blockers.Add($"{refinementMappings.Count(m => !m.Mapped)} refinement selector(s) lack source background mappings");
        if (environmentMappings.Any(m => !m.Mapped))
            blockers.Add($"{environmentMappings.Count(m => !m.Mapped)} environment selector(s) lack solver-backed source background mappings");
        return blockers;
    }

    private static string InferRefinementLevel(string sourceRecordId)
    {
        var match = System.Text.RegularExpressions.Regex.Match(sourceRecordId, @"(?:^|-)l(?<level>\d+)(?:-|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? $"L{match.Groups["level"].Value}" : sourceRecordId;
    }

    private static bool SelectorMatches(string mappedSelector, string selector)
        => string.Equals(mappedSelector, selector, StringComparison.Ordinal) ||
           selector.StartsWith($"{mappedSelector}-", StringComparison.Ordinal);

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string property)
        => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString() ?? string.Empty)
                .ToList()
            : [];

    private static string? OptionalString(JsonElement root, string property)
        => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    private static string? ElementAtOrNull(IReadOnlyList<string> values, int index)
        => index >= 0 && index < values.Count ? values[index] : null;
}
