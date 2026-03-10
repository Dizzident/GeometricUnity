using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Result of a complete boson comparison campaign.
/// Contains all comparison results, including negative and underdetermined outcomes.
/// </summary>
public sealed class BosonCampaignResult
{
    /// <summary>Campaign ID.</summary>
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    /// <summary>Campaign mode used.</summary>
    [JsonPropertyName("mode")]
    public required BosonComparisonMode Mode { get; init; }

    /// <summary>All comparison results (including incompatible and underdetermined).</summary>
    [JsonPropertyName("results")]
    public required IReadOnlyList<BosonComparisonResult> Results { get; init; }

    /// <summary>Negative results (incompatible or insufficient evidence).</summary>
    [JsonPropertyName("negativeResults")]
    public required IReadOnlyList<BosonComparisonResult> NegativeResults { get; init; }

    /// <summary>Number of candidates that were compared.</summary>
    [JsonPropertyName("candidatesCompared")]
    public int CandidatesCompared { get; init; }

    /// <summary>Number of targets/descriptors used.</summary>
    [JsonPropertyName("targetsUsed")]
    public int TargetsUsed { get; init; }

    /// <summary>Completion timestamp.</summary>
    [JsonPropertyName("completedAt")]
    public DateTimeOffset CompletedAt { get; init; }
}
