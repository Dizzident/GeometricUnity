using System.Text.Json.Serialization;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison;

/// <summary>
/// First-class artifact for negative/failed comparison results (Section 13.5).
/// Wraps a ComparisonFailureRecord with additional campaign and prediction context,
/// ensuring negative results are never discarded.
/// </summary>
public sealed class NegativeResultArtifact
{
    /// <summary>Unique artifact identifier.</summary>
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    /// <summary>Campaign ID that produced this negative result.</summary>
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    /// <summary>The failure record with full details.</summary>
    [JsonPropertyName("failure")]
    public required ComparisonFailureRecord Failure { get; init; }

    /// <summary>Original test ID of the prediction that failed.</summary>
    [JsonPropertyName("originalTestId")]
    public required string OriginalTestId { get; init; }

    /// <summary>Comparison mode that was attempted.</summary>
    [JsonPropertyName("attemptedMode")]
    public required ComparisonMode AttemptedMode { get; init; }

    /// <summary>Branch manifest ID for traceability.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Whether this negative result falsifies the underlying prediction.</summary>
    [JsonPropertyName("isFalsification")]
    public required bool IsFalsification { get; init; }

    /// <summary>Timestamp when this artifact was created.</summary>
    [JsonPropertyName("createdAt")]
    public required DateTimeOffset CreatedAt { get; init; }
}
