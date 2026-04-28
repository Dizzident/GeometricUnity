using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting;

public sealed class WzSelectorCellMaterializationAuditResult
{
    [JsonPropertyName("resultId")]
    public required string ResultId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    [JsonPropertyName("terminalStatus")]
    public required string TerminalStatus { get; init; }

    [JsonPropertyName("algorithmId")]
    public required string AlgorithmId { get; init; }

    [JsonPropertyName("totalCellCount")]
    public required int TotalCellCount { get; init; }

    [JsonPropertyName("materializedCellCount")]
    public required int MaterializedCellCount { get; init; }

    [JsonPropertyName("missingCellCount")]
    public required int MissingCellCount { get; init; }

    [JsonPropertyName("cellAudits")]
    public required IReadOnlyList<WzSelectorCellMaterializationRecord> CellAudits { get; init; }

    [JsonPropertyName("summaryBlockers")]
    public required IReadOnlyList<string> SummaryBlockers { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

public sealed class WzSelectorCellMaterializationRecord
{
    [JsonPropertyName("cellId")]
    public required string CellId { get; init; }

    [JsonPropertyName("sourceCandidateId")]
    public required string SourceCandidateId { get; init; }

    [JsonPropertyName("branchVariantId")]
    public required string BranchVariantId { get; init; }

    [JsonPropertyName("refinementLevel")]
    public required string RefinementLevel { get; init; }

    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    [JsonPropertyName("backgroundRecordPath")]
    public string? BackgroundRecordPath { get; init; }

    [JsonPropertyName("branchManifestPath")]
    public string? BranchManifestPath { get; init; }

    [JsonPropertyName("omegaStatePath")]
    public string? OmegaStatePath { get; init; }

    [JsonPropertyName("a0StatePath")]
    public string? A0StatePath { get; init; }

    [JsonPropertyName("geometryPath")]
    public string? GeometryPath { get; init; }

    [JsonPropertyName("environmentRecordPath")]
    public string? EnvironmentRecordPath { get; init; }

    [JsonPropertyName("materializationStatus")]
    public required string MaterializationStatus { get; init; }

    [JsonPropertyName("missingInputs")]
    public required IReadOnlyList<string> MissingInputs { get; init; }
}

public static class WzSelectorCellMaterializationAudit
{
    public const string AlgorithmId = "p36-wz-selector-cell-materialization-audit:v1";

    public static WzSelectorCellMaterializationAuditResult Evaluate(
        string campaignSpecJson,
        string sourceCandidatesJson,
        IReadOnlyList<string> artifactRoots,
        ProvenanceMeta provenance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(campaignSpecJson);
        ArgumentException.ThrowIfNullOrWhiteSpace(sourceCandidatesJson);
        ArgumentNullException.ThrowIfNull(artifactRoots);

        var spec = GuJsonDefaults.Deserialize<InternalVectorBosonSourceSpectrumCampaignSpec>(campaignSpecJson)
            ?? throw new InvalidDataException("Failed to deserialize selector source spectrum campaign spec.");
        var candidates = GuJsonDefaults.Deserialize<InternalVectorBosonSourceCandidateTable>(sourceCandidatesJson)
            ?? throw new InvalidDataException("Failed to deserialize source candidate table.");

        var index = ArtifactIndex.Build(artifactRoots.Where(Directory.Exists).ToList());
        var cellAudits = new List<WzSelectorCellMaterializationRecord>();
        foreach (var candidate in candidates.Candidates.OrderBy(c => c.SourceCandidateId, StringComparer.Ordinal))
        foreach (var branch in spec.BranchVariantIds)
        foreach (var refinement in spec.RefinementLevels)
        foreach (var environment in spec.EnvironmentIds)
        {
            cellAudits.Add(AuditCell(candidate.SourceCandidateId, branch, refinement, environment, index));
        }

        var materialized = cellAudits.Count(c => c.MaterializationStatus == "materialized");
        var blockers = BuildSummaryBlockers(cellAudits, artifactRoots);
        return new WzSelectorCellMaterializationAuditResult
        {
            ResultId = "phase36-wz-selector-cell-materialization-audit-v1",
            SchemaVersion = "1.0.0",
            TerminalStatus = materialized == cellAudits.Count && cellAudits.Count > 0
                ? "selector-cells-materialized"
                : "selector-cell-materialization-blocked",
            AlgorithmId = AlgorithmId,
            TotalCellCount = cellAudits.Count,
            MaterializedCellCount = materialized,
            MissingCellCount = cellAudits.Count - materialized,
            CellAudits = cellAudits,
            SummaryBlockers = blockers,
            Provenance = provenance,
        };
    }

    private static WzSelectorCellMaterializationRecord AuditCell(
        string sourceCandidateId,
        string branchVariantId,
        string refinementLevel,
        string environmentId,
        ArtifactIndex index)
    {
        var key = new SelectorKey(branchVariantId, refinementLevel, environmentId);
        var background = index.Backgrounds.TryGetValue(key, out var exactBackground)
            ? exactBackground
            : index.FindBackground(branchVariantId, refinementLevel, environmentId);
        var backgroundId = background?.BackgroundId;
        var manifest = backgroundId is null ? null : index.FindManifest(backgroundId, branchVariantId);
        var omega = backgroundId is null ? null : index.FindOmega(backgroundId);
        var a0 = index.FindA0(refinementLevel, environmentId);
        var geometry = index.FindGeometry(refinementLevel, environmentId);
        var environment = index.FindEnvironment(environmentId);

        var missing = new List<string>();
        if (background is null)
            missing.Add("backgroundRecord");
        if (manifest is null)
            missing.Add("branchManifest");
        if (omega is null)
            missing.Add("omegaState");
        if (a0 is null)
            missing.Add("a0State");
        if (geometry is null)
            missing.Add("geometryContext");
        if (environment is null)
            missing.Add("environmentRecord");

        return new WzSelectorCellMaterializationRecord
        {
            CellId = $"{sourceCandidateId}::{branchVariantId}::{refinementLevel}::{environmentId}",
            SourceCandidateId = sourceCandidateId,
            BranchVariantId = branchVariantId,
            RefinementLevel = refinementLevel,
            EnvironmentId = environmentId,
            BackgroundRecordPath = background?.Path,
            BranchManifestPath = manifest,
            OmegaStatePath = omega,
            A0StatePath = a0,
            GeometryPath = geometry,
            EnvironmentRecordPath = environment,
            MaterializationStatus = missing.Count == 0 ? "materialized" : "missing-inputs",
            MissingInputs = missing,
        };
    }

    private static IReadOnlyList<string> BuildSummaryBlockers(
        IReadOnlyList<WzSelectorCellMaterializationRecord> cells,
        IReadOnlyList<string> artifactRoots)
    {
        if (cells.Count == 0)
            return ["no selector cells were produced from the campaign spec"];
        if (cells.All(c => c.MaterializationStatus == "materialized"))
            return [];

        var blockers = new List<string>
        {
            $"{cells.Count(c => c.MaterializationStatus != "materialized")} selector cell(s) lack materialized solver inputs",
        };
        blockers.AddRange(cells
            .SelectMany(c => c.MissingInputs)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x, StringComparer.Ordinal)
            .Select(x => $"missing {x} for at least one selector cell"));
        if (!artifactRoots.Any(Directory.Exists))
            blockers.Add("no artifact root exists");
        return blockers;
    }

    private sealed record SelectorKey(string BranchVariantId, string RefinementLevel, string EnvironmentId);

    private sealed record BackgroundArtifact(string BackgroundId, string Path);

    private sealed class ArtifactIndex
    {
        public required IReadOnlyDictionary<SelectorKey, BackgroundArtifact> Backgrounds { get; init; }

        public required IReadOnlyList<BackgroundArtifact> BackgroundArtifacts { get; init; }

        public required IReadOnlyList<string> Manifests { get; init; }

        public required IReadOnlyList<string> Omegas { get; init; }

        public required IReadOnlyList<string> A0States { get; init; }

        public required IReadOnlyList<string> Geometries { get; init; }

        public required IReadOnlyList<string> Environments { get; init; }

        public static ArtifactIndex Build(IReadOnlyList<string> roots)
        {
            var files = roots
                .SelectMany(r => Directory.EnumerateFiles(r, "*.json", SearchOption.AllDirectories))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            var backgrounds = new Dictionary<SelectorKey, BackgroundArtifact>();
            foreach (var path in files)
            {
                var background = TryReadBackground(path);
                if (background is null)
                    continue;
                foreach (var key in CandidateKeys(background.Value.Element, path))
                    backgrounds.TryAdd(key, new BackgroundArtifact(background.Value.BackgroundId, path));
            }

            return new ArtifactIndex
            {
                Backgrounds = backgrounds,
                BackgroundArtifacts = backgrounds.Values.DistinctBy(b => b.Path).ToList(),
                Manifests = files.Where(IsManifestPath).ToList(),
                Omegas = files.Where(p => p.EndsWith("_omega.json", StringComparison.OrdinalIgnoreCase)).ToList(),
                A0States = files.Where(IsA0Path).ToList(),
                Geometries = files.Where(p => string.Equals(Path.GetFileName(p), "geometry.json", StringComparison.OrdinalIgnoreCase)).ToList(),
                Environments = files.Where(IsEnvironmentPath).ToList(),
            };
        }

        public BackgroundArtifact? FindBackground(string branchVariantId, string refinementLevel, string environmentId)
            => BackgroundArtifacts.FirstOrDefault(b =>
                FileTextContains(b.Path, environmentId) &&
                (FileTextContains(b.Path, branchVariantId) || Contains(b.Path, branchVariantId)) &&
                (FileTextContains(b.Path, refinementLevel) || Contains(b.Path, refinementLevel)));

        public string? FindManifest(string backgroundId, string branchVariantId)
            => Manifests.FirstOrDefault(p => Contains(p, backgroundId) || Contains(p, branchVariantId));

        public string? FindOmega(string backgroundId)
            => Omegas.FirstOrDefault(p => Contains(p, backgroundId));

        public string? FindA0(string refinementLevel, string environmentId)
            => A0States.FirstOrDefault(p => Contains(p, refinementLevel) || Contains(p, environmentId)) ?? A0States.FirstOrDefault();

        public string? FindGeometry(string refinementLevel, string environmentId)
            => Geometries.FirstOrDefault(p => Contains(p, refinementLevel) || Contains(p, environmentId)) ?? Geometries.FirstOrDefault();

        public string? FindEnvironment(string environmentId)
            => Environments.FirstOrDefault(p => Contains(p, environmentId) || FileTextContains(p, environmentId));

        private static (string BackgroundId, JsonElement Element)? TryReadBackground(string path)
        {
            try
            {
                using var doc = JsonDocument.Parse(File.ReadAllText(path));
                var root = doc.RootElement.Clone();
                if (!root.TryGetProperty("backgroundId", out var id) || id.ValueKind != JsonValueKind.String)
                    return null;
                if (!root.TryGetProperty("environmentId", out _))
                    return null;
                return (id.GetString() ?? string.Empty, root);
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException)
            {
                return null;
            }
        }

        private static IEnumerable<SelectorKey> CandidateKeys(JsonElement background, string path)
        {
            var branchValues = new[]
            {
                OptionalString(background, "branchVariantId"),
                OptionalString(background, "branchManifestId"),
                OptionalString(background, "consumedManifestArtifactRef"),
                OptionalString(background, "backgroundId"),
            }.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).ToList();
            if (background.TryGetProperty("provenance", out var provenance) &&
                provenance.TryGetProperty("branch", out var provenanceBranch) &&
                provenanceBranch.TryGetProperty("branchId", out var branchId) &&
                branchId.ValueKind == JsonValueKind.String)
            {
                branchValues.Add(branchId.GetString() ?? string.Empty);
            }

            var refinementValues = new[]
            {
                OptionalString(background, "refinementLevel"),
                OptionalString(background, "backgroundId"),
                path,
            }.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!).ToList();
            var environment = OptionalString(background, "environmentId");
            if (string.IsNullOrWhiteSpace(environment))
                yield break;

            foreach (var branch in branchValues.SelectMany(Tokens).Distinct(StringComparer.Ordinal))
            foreach (var refinement in refinementValues.SelectMany(Tokens).Distinct(StringComparer.Ordinal))
                yield return new SelectorKey(branch, refinement, environment);
        }

        private static IEnumerable<string> Tokens(string value)
        {
            yield return value;
            foreach (System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(value, @"L\d+(?:-[A-Za-z0-9]+)?|bg-variant-[A-Za-z0-9]+|env-[A-Za-z0-9_.-]+"))
                yield return match.Value;
        }

        private static bool IsManifestPath(string path)
            => path.EndsWith("_manifest.json", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(Path.GetFileName(path), "branch_manifest.json", StringComparison.OrdinalIgnoreCase);

        private static bool IsA0Path(string path)
            => string.Equals(Path.GetFileName(path), "a0.json", StringComparison.OrdinalIgnoreCase) ||
               path.EndsWith("_a0.json", StringComparison.OrdinalIgnoreCase);

        private static bool IsEnvironmentPath(string path)
            => Path.GetFileName(path).Contains("environment", StringComparison.OrdinalIgnoreCase) ||
               FileTextContains(path, "\"environmentId\"");

        private static string? OptionalString(JsonElement root, string property)
            => root.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;

        private static bool Contains(string value, string token)
            => !string.IsNullOrWhiteSpace(token) &&
               value.Contains(token, StringComparison.OrdinalIgnoreCase);

        private static bool FileTextContains(string path, string token)
        {
            try
            {
                return File.ReadAllText(path).Contains(token, StringComparison.OrdinalIgnoreCase);
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
