using System.Text.Json.Serialization;
using Gu.Phase2.Comparison;

namespace Gu.Phase2.Reporting.Artifacts;

/// <summary>
/// Artifact wrapping a comparison campaign result for reporting.
/// Failures are preserved as first-class data -- never filtered.
/// </summary>
public sealed class ComparisonCampaignArtifact
{
    /// <summary>Unique artifact identifier.</summary>
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    /// <summary>Campaign ID.</summary>
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    /// <summary>The campaign result data.</summary>
    [JsonPropertyName("result")]
    public required ComparisonCampaignResult Result { get; init; }

    /// <summary>Number of successful comparison runs.</summary>
    [JsonPropertyName("successCount")]
    public required int SuccessCount { get; init; }

    /// <summary>Number of failed comparison runs.</summary>
    [JsonPropertyName("failureCount")]
    public required int FailureCount { get; init; }

    /// <summary>Schema version for this artifact type.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; } = "2.0.0";

    /// <summary>Timestamp when this artifact was produced.</summary>
    [JsonPropertyName("producedAt")]
    public required DateTimeOffset ProducedAt { get; init; }
}
