using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Comparison mode for boson campaigns.
/// </summary>
public enum BosonComparisonMode
{
    /// <summary>BC1: compare against internal target profiles.</summary>
    InternalTargetProfile,

    /// <summary>BC2: compare against external analogy descriptors.</summary>
    ExternalAnalogy,
}

/// <summary>
/// Specification for a boson comparison campaign.
/// Defines what candidates to compare and what targets/descriptors to use.
/// </summary>
public sealed class BosonCampaignSpec
{
    /// <summary>Unique campaign identifier.</summary>
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    /// <summary>Campaign comparison mode.</summary>
    [JsonPropertyName("mode")]
    public required BosonComparisonMode Mode { get; init; }

    /// <summary>Internal target profile IDs for BC1 mode.</summary>
    [JsonPropertyName("targetProfileIds")]
    public IReadOnlyList<string> TargetProfileIds { get; init; } = Array.Empty<string>();

    /// <summary>External analogy descriptor IDs for BC2 mode.</summary>
    [JsonPropertyName("externalDescriptorIds")]
    public IReadOnlyList<string> ExternalDescriptorIds { get; init; } = Array.Empty<string>();

    /// <summary>Minimum claim class for candidates to include.</summary>
    [JsonPropertyName("minClaimClass")]
    public Registry.BosonClaimClass MinClaimClass { get; init; } = Registry.BosonClaimClass.C0_NumericalMode;

    /// <summary>Whether to include demoted candidates in the comparison.</summary>
    [JsonPropertyName("includeDemoted")]
    public bool IncludeDemoted { get; init; } = true;
}
