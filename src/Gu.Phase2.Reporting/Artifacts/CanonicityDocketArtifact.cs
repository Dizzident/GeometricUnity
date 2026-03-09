using System.Text.Json.Serialization;
using Gu.Phase2.Canonicity;

namespace Gu.Phase2.Reporting.Artifacts;

/// <summary>
/// Artifact wrapping aggregated canonicity docket information for reporting.
/// Dockets track which branch-sensitive object classes have been shown canonical
/// (or remain open).
/// </summary>
public sealed class CanonicityDocketArtifact
{
    /// <summary>Unique artifact identifier.</summary>
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    /// <summary>The aggregated dockets.</summary>
    [JsonPropertyName("dockets")]
    public required IReadOnlyList<CanonicityDocket> Dockets { get; init; }

    /// <summary>Number of open (unclosed) dockets.</summary>
    [JsonPropertyName("openCount")]
    public required int OpenCount { get; init; }

    /// <summary>Number of closed dockets.</summary>
    [JsonPropertyName("closedCount")]
    public required int ClosedCount { get; init; }

    /// <summary>Schema version for this artifact type.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; } = "2.0.0";

    /// <summary>Timestamp when this artifact was produced.</summary>
    [JsonPropertyName("producedAt")]
    public required DateTimeOffset ProducedAt { get; init; }
}
