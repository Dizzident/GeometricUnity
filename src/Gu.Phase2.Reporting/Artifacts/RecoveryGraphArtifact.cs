using System.Text.Json.Serialization;
using Gu.Phase2.Recovery;

namespace Gu.Phase2.Reporting.Artifacts;

/// <summary>
/// Artifact wrapping the recovery graph for reporting.
/// The recovery graph is the mandatory typed path for all observation/extraction.
/// </summary>
public sealed class RecoveryGraphArtifact
{
    /// <summary>Unique artifact identifier.</summary>
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    /// <summary>Branch manifest ID.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>The recovery graph.</summary>
    [JsonPropertyName("graph")]
    public required RecoveryGraph Graph { get; init; }

    /// <summary>Number of nodes in the graph.</summary>
    [JsonPropertyName("nodeCount")]
    public required int NodeCount { get; init; }

    /// <summary>Whether the graph passed validation.</summary>
    [JsonPropertyName("isValid")]
    public required bool IsValid { get; init; }

    /// <summary>Validation errors, if any.</summary>
    [JsonPropertyName("validationErrors")]
    public IReadOnlyList<string>? ValidationErrors { get; init; }

    /// <summary>Schema version for this artifact type.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; } = "2.0.0";

    /// <summary>Timestamp when this artifact was produced.</summary>
    [JsonPropertyName("producedAt")]
    public required DateTimeOffset ProducedAt { get; init; }
}
