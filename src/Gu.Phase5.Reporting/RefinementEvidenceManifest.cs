using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Declares the provenance tier for a refinement values table.
/// This keeps bridge-derived ladders distinct from direct solver-backed ladders.
/// </summary>
public sealed class RefinementEvidenceManifest
{
    /// <summary>Unique manifest identifier.</summary>
    [JsonPropertyName("manifestId")]
    public required string ManifestId { get; init; }

    /// <summary>Refinement study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Evidence source classification: bridge-derived or direct-solver-backed.</summary>
    [JsonPropertyName("evidenceSource")]
    public required string EvidenceSource { get; init; }

    /// <summary>Background record IDs that seeded the refinement ladder.</summary>
    [JsonPropertyName("sourceRecordIds")]
    public required IReadOnlyList<string> SourceRecordIds { get; init; }

    /// <summary>Source artifact refs that produced the refinement ladder.</summary>
    [JsonPropertyName("sourceArtifactRefs")]
    public required IReadOnlyList<string> SourceArtifactRefs { get; init; }

    /// <summary>Optional human-readable notes.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    /// <summary>Provenance metadata for this manifest.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
