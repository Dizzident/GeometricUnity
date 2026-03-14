using System.Text.Json.Serialization;
using Gu.Artifacts;

namespace Gu.Phase5.Dossiers;

/// <summary>
/// Index of all validation dossiers in a run folder.
/// Written to phase5/dossiers/dossier_index.json.
/// </summary>
public sealed class DossierIndex
{
    /// <summary>Unique identifier for this index.</summary>
    [JsonPropertyName("indexId")]
    public required string IndexId { get; init; }

    /// <summary>Summary entries for each dossier, ordered by assembly time.</summary>
    [JsonPropertyName("entries")]
    public required IReadOnlyList<DossierIndexEntry> Entries { get; init; }

    /// <summary>When this index was last updated.</summary>
    [JsonPropertyName("updatedAt")]
    public required DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Summary entry in a DossierIndex.
/// </summary>
public sealed class DossierIndexEntry
{
    /// <summary>Dossier ID.</summary>
    [JsonPropertyName("dossierId")]
    public required string DossierId { get; init; }

    /// <summary>Human-readable title.</summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>Path to the dossier JSON file (relative to run folder).</summary>
    [JsonPropertyName("path")]
    public required string Path { get; init; }

    /// <summary>Overall evidence tier.</summary>
    [JsonPropertyName("overallEvidenceTier")]
    public required ArtifactEvidenceTier OverallEvidenceTier { get; init; }

    /// <summary>True if this dossier is acceptable as Phase V validation evidence.</summary>
    [JsonPropertyName("isAcceptableAsEvidence")]
    public required bool IsAcceptableAsEvidence { get; init; }

    /// <summary>Number of stale study contributions in this dossier.</summary>
    [JsonPropertyName("staleStudyCount")]
    public required int StaleStudyCount { get; init; }

    /// <summary>When the dossier was assembled.</summary>
    [JsonPropertyName("assembledAt")]
    public required DateTimeOffset AssembledAt { get; init; }
}
