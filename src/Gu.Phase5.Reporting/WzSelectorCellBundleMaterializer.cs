using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzSelectorCellBundleMaterializationResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("expectedSelectorCellCount")]
    public required int ExpectedSelectorCellCount { get; init; }

    [JsonPropertyName("writtenBundleCount")]
    public required int WrittenBundleCount { get; init; }

    [JsonPropertyName("skippedBundleCount")]
    public required int SkippedBundleCount { get; init; }

    [JsonPropertyName("bundles")]
    public required IReadOnlyList<WzSelectorCellBundleRecord> Bundles { get; init; }

    [JsonPropertyName("summaryBlockers")]
    public required IReadOnlyList<string> SummaryBlockers { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzSelectorCellBundleRecord
{
    [JsonPropertyName("bundleId")]
    public required string BundleId { get; init; }

    [JsonPropertyName("branchVariantId")]
    public required string BranchVariantId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public required string RefinementLevel { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("written")]
    public required bool Written { get; init; }

    [JsonPropertyName("backgroundRecordPath")]
    public string? BackgroundRecordPath { get; init; }

    [JsonPropertyName("blockers")]
    public required IReadOnlyList<string> Blockers { get; init; }
}

public static class WzSelectorCellBundleMaterializer
{
    public const string AlgorithmId = "p40-wz-selector-cell-bundle-materializer:v1";

    public static WzSelectorCellBundleMaterializationResult Materialize(
        string campaignSpecJson,
        string selectorMaterializationMapJson,
        string environmentClosureJson,
        string outputRoot,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(campaignSpecJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(selectorMaterializationMapJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(environmentClosureJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputRoot);

        var spec = GuJsonDefaults.Deserialize<InternalVectorBosonSourceSpectrumCampaignSpec>(campaignSpecJson)
            ?? throw new InvalidDataException("Failed to deserialize selector source spectrum campaign spec.");
        using var selectorMapDoc = JsonDocument.Parse(selectorMaterializationMapJson);
        using var environmentClosureDoc = JsonDocument.Parse(environmentClosureJson);

        var branchMappings = ReadAxisMappings(selectorMapDoc.RootElement, "branchMappings");
        var refinementMappings = ReadAxisMappings(selectorMapDoc.RootElement, "refinementMappings");
        var environmentMappings = ReadEnvironmentMappings(environmentClosureDoc.RootElement);

        Directory.CreateDirectory(outputRoot);
        var bundleRecords = new List<WzSelectorCellBundleRecord>();
        foreach (var branch in spec.BranchVariantIds)
        foreach (var refinement in spec.RefinementLevels)
        foreach (var environment in spec.EnvironmentIds)
        {
            var bundle = BuildBundle(branch, refinement, environment, branchMappings, refinementMappings, environmentMappings, outputRoot, provenance);
            bundleRecords.Add(bundle);
        }

        var skipped = bundleRecords.Where(b => !b.Written).ToList();
        var blockers = skipped
            .SelectMany(b => b.Blockers)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(b => b, StringComparer.Ordinal)
            .ToList();

        var result = new WzSelectorCellBundleMaterializationResult
        {
            ResultId = "phase40-wz-selector-cell-bundle-materialization-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = blockers.Count == 0
                ? "selector-cell-bundles-materialized"
                : "selector-cell-bundles-partial",
            AlgorithmId = AlgorithmId,
            ExpectedSelectorCellCount = bundleRecords.Count,
            WrittenBundleCount = bundleRecords.Count(b => b.Written),
            SkippedBundleCount = skipped.Count,
            Bundles = bundleRecords,
            SummaryBlockers = blockers,
            Provenance = provenance,
        };

        File.WriteAllText(Path.Combine(outputRoot, "selector_cell_bundle_manifest.json"), GuJsonDefaults.Serialize(result));
        return result;
    }

    private static WzSelectorCellBundleRecord BuildBundle(
        string branch,
        string refinement,
        string environment,
        IReadOnlyDictionary<string, AxisMap> branchMappings,
        IReadOnlyDictionary<string, AxisMap> refinementMappings,
        IReadOnlyDictionary<string, EnvironmentMap> environmentMappings,
        string outputRoot,
        ProvenanceMeta provenance)
    {
        var blockers = new List<string>();
        if (!branchMappings.TryGetValue(branch, out var branchMap) || !branchMap.Mapped)
            blockers.Add($"branch selector '{branch}' is not source-backed");
        if (!refinementMappings.TryGetValue(refinement, out var refinementMap) || !refinementMap.Mapped)
            blockers.Add($"refinement selector '{refinement}' is not source-backed");
        if (!environmentMappings.TryGetValue(environment, out var environmentMap) || !environmentMap.BackgroundBacked)
            blockers.Add($"environment selector '{environment}' is not background-backed");

        var bundleId = $"p40-{Sanitize(branch)}__{Sanitize(refinement)}__{Sanitize(environment)}";
        if (blockers.Count > 0)
        {
            return new WzSelectorCellBundleRecord
            {
                BundleId = bundleId,
                BranchVariantId = branch,
                RefinementLevel = refinement,
                EnvironmentId = environment,
                Written = false,
                Blockers = blockers,
            };
        }
        if (branchMap is null)
            throw new InvalidOperationException($"Missing branch selector map for '{branch}'.");
        if (refinementMap is null)
            throw new InvalidOperationException($"Missing refinement selector map for '{refinement}'.");
        if (environmentMap is null)
            throw new InvalidOperationException($"Missing environment selector map for '{environment}'.");

        var bundleDir = Path.Combine(outputRoot, bundleId);
        Directory.CreateDirectory(bundleDir);
        var backgroundId = $"{bundleId}-background";
        var manifestPath = Path.Combine(bundleDir, $"{backgroundId}_manifest.json");
        var omegaPath = Path.Combine(bundleDir, $"{backgroundId}_omega.json");
        var backgroundPath = Path.Combine(bundleDir, $"{backgroundId}.json");

        File.WriteAllText(backgroundPath, GuJsonDefaults.Serialize(new
        {
            backgroundId,
            environmentId = environment,
            branchVariantId = branch,
            refinementLevel = refinement,
            branchManifestId = branch,
            stateArtifactRef = Path.GetFileName(omegaPath),
            consumedManifestArtifactRef = Path.GetFileName(manifestPath),
            materializationKind = "axis-composed-selector-cell-bundle",
            branchSourceRecordId = branchMap.SourceRecordId,
            branchSourceArtifactRef = branchMap.SourceArtifactRef,
            refinementSourceRecordId = refinementMap.SourceRecordId,
            refinementSourceArtifactRef = refinementMap.SourceArtifactRef,
            environmentBackgroundRecordId = environmentMap.BackgroundRecordId,
            environmentBackgroundRecordPath = environmentMap.BackgroundRecordPath,
            provenance,
        }));
        File.WriteAllText(manifestPath, GuJsonDefaults.Serialize(new
        {
            branchId = branch,
            schemaVersion = "1.0",
            materializationKind = "selector-cell-branch-manifest-ref",
            sourceRecordId = branchMap.SourceRecordId,
            sourceArtifactRef = branchMap.SourceArtifactRef,
            provenance,
        }));
        File.WriteAllText(omegaPath, GuJsonDefaults.Serialize(new
        {
            label = "omega",
            backgroundId,
            materializationKind = "selector-cell-omega-ref",
            branchSourceArtifactRef = branchMap.SourceArtifactRef,
            refinementSourceArtifactRef = refinementMap.SourceArtifactRef,
            environmentBackgroundRecordPath = environmentMap.BackgroundRecordPath,
            provenance,
        }));
        File.WriteAllText(Path.Combine(bundleDir, "a0.json"), GuJsonDefaults.Serialize(new
        {
            label = "a0",
            environmentId = environment,
            refinementLevel = refinement,
            materializationKind = "selector-cell-a0-reference",
            provenance,
        }));
        File.WriteAllText(Path.Combine(bundleDir, "geometry.json"), GuJsonDefaults.Serialize(new
        {
            environmentId = environment,
            refinementLevel = refinement,
            materializationKind = "selector-cell-geometry-reference",
            environmentBackgroundRecordPath = environmentMap.BackgroundRecordPath,
            provenance,
        }));
        File.WriteAllText(Path.Combine(bundleDir, "environment.json"), GuJsonDefaults.Serialize(new
        {
            environmentId = environment,
            materializationKind = "selector-cell-environment-reference",
            environmentRecordPath = environmentMap.EnvironmentRecordPath,
            provenance,
        }));

        return new WzSelectorCellBundleRecord
        {
            BundleId = bundleId,
            BranchVariantId = branch,
            RefinementLevel = refinement,
            EnvironmentId = environment,
            Written = true,
            BackgroundRecordPath = backgroundPath,
            Blockers = [],
        };
    }

    private static IReadOnlyDictionary<string, AxisMap> ReadAxisMappings(JsonElement root, string property)
        => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
                .Select(e => new AxisMap(
                    RequiredString(e, "selectorId"),
                    OptionalBool(e, "mapped"),
                    OptionalString(e, "sourceRecordId"),
                    OptionalString(e, "sourceArtifactRef")))
                .ToDictionary(m => m.SelectorId, StringComparer.Ordinal)
            : new Dictionary<string, AxisMap>(StringComparer.Ordinal);

    private static IReadOnlyDictionary<string, EnvironmentMap> ReadEnvironmentMappings(JsonElement root)
        => root.TryGetProperty("environmentMappings", out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
                .Select(e => new EnvironmentMap(
                    RequiredString(e, "environmentId"),
                    OptionalBool(e, "backgroundRecordFound"),
                    OptionalString(e, "environmentRecordPath"),
                    ReadStringArray(e, "backgroundRecordIds").FirstOrDefault(),
                    ReadStringArray(e, "backgroundRecordPaths")
                        .FirstOrDefault(p => p.Contains("background_records", StringComparison.OrdinalIgnoreCase)) ??
                    ReadStringArray(e, "backgroundRecordPaths").FirstOrDefault()))
                .ToDictionary(m => m.EnvironmentId, StringComparer.Ordinal)
            : new Dictionary<string, EnvironmentMap>(StringComparer.Ordinal);

    private static IReadOnlyList<string> ReadStringArray(JsonElement root, string property)
        => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray()
                .Where(e => e.ValueKind == JsonValueKind.String)
                .Select(e => e.GetString() ?? string.Empty)
                .ToList()
            : [];

    private static string RequiredString(JsonElement root, string property)
        => OptionalString(root, property) ?? throw new InvalidDataException($"Missing required string property '{property}'.");

    private static string? OptionalString(JsonElement root, string property)
        => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private static bool OptionalBool(JsonElement root, string property)
        => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.True;

    private static string Sanitize(string value)
        => System.Text.RegularExpressions.Regex.Replace(value, @"[^A-Za-z0-9]+", "-").Trim('-');

    private sealed record AxisMap(string SelectorId, bool Mapped, string? SourceRecordId, string? SourceArtifactRef);

    private sealed record EnvironmentMap(
        string EnvironmentId,
        bool BackgroundBacked,
        string? EnvironmentRecordPath,
        string? BackgroundRecordId,
        string? BackgroundRecordPath);
}
