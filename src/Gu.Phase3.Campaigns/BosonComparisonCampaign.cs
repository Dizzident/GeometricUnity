using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Specification for a boson comparison campaign.
///
/// A campaign defines which target profiles to compare against which
/// candidates (filtered by claim class or other criteria).
/// </summary>
public sealed class BosonComparisonCampaign
{
    /// <summary>Unique campaign identifier.</summary>
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    /// <summary>Human-readable campaign description.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }

    /// <summary>Target profiles to compare candidates against.</summary>
    [JsonPropertyName("targetProfiles")]
    public required IReadOnlyList<TargetProfile> TargetProfiles { get; init; }

    /// <summary>
    /// Minimum claim class for candidates to be included in the campaign.
    /// Candidates below this class are filtered out.
    /// </summary>
    [JsonPropertyName("minimumClaimClass")]
    public required Registry.BosonClaimClass MinimumClaimClass { get; init; }

    /// <summary>Comparison strategy identifier (e.g. "envelope-overlap", "threshold-based").</summary>
    [JsonPropertyName("comparisonStrategy")]
    public string ComparisonStrategy { get; init; } = "threshold-based";
}
