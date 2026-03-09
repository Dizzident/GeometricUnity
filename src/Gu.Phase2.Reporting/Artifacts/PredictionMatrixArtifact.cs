using System.Text.Json.Serialization;
using Gu.Phase2.Predictions;

namespace Gu.Phase2.Reporting.Artifacts;

/// <summary>
/// Artifact wrapping the prediction/test matrix for reporting.
/// Every comparison-facing output must have a typed PredictionTestRecord.
/// </summary>
public sealed class PredictionMatrixArtifact
{
    /// <summary>Unique artifact identifier.</summary>
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    /// <summary>Prediction test records in this matrix.</summary>
    [JsonPropertyName("predictions")]
    public required IReadOnlyList<PredictionTestRecord> Predictions { get; init; }

    /// <summary>Number of predictions by claim class.</summary>
    [JsonPropertyName("countByClaimClass")]
    public required IReadOnlyDictionary<string, int> CountByClaimClass { get; init; }

    /// <summary>Total number of predictions.</summary>
    [JsonPropertyName("totalCount")]
    public required int TotalCount { get; init; }

    /// <summary>Schema version for this artifact type.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; } = "2.0.0";

    /// <summary>Timestamp when this artifact was produced.</summary>
    [JsonPropertyName("producedAt")]
    public required DateTimeOffset ProducedAt { get; init; }
}
