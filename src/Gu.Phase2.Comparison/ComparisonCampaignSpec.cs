using System.Text.Json.Serialization;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Specification for a comparison campaign (Section 13.1).
/// A campaign may span multiple environments, branches, output classes, and assets.
/// </summary>
public sealed class ComparisonCampaignSpec
{
    /// <summary>Unique campaign identifier.</summary>
    [JsonPropertyName("campaignId")]
    public required string CampaignId { get; init; }

    /// <summary>Environment IDs included in this campaign.</summary>
    [JsonPropertyName("environmentIds")]
    public required IReadOnlyList<string> EnvironmentIds { get; init; }

    /// <summary>Branch subset IDs to compare.</summary>
    [JsonPropertyName("branchSubsetIds")]
    public required IReadOnlyList<string> BranchSubsetIds { get; init; }

    /// <summary>Observed output class IDs to compare.</summary>
    [JsonPropertyName("observedOutputClassIds")]
    public required IReadOnlyList<string> ObservedOutputClassIds { get; init; }

    /// <summary>Comparison asset IDs to compare against.</summary>
    [JsonPropertyName("comparisonAssetIds")]
    public required IReadOnlyList<string> ComparisonAssetIds { get; init; }

    /// <summary>Comparison mode: Structural, SemiQuantitative, or Quantitative.</summary>
    [JsonPropertyName("mode")]
    public required ComparisonMode Mode { get; init; }

    /// <summary>Calibration policy: "fixed", "fitted", "inverse", "sensitivity-only".</summary>
    [JsonPropertyName("calibrationPolicy")]
    public required string CalibrationPolicy { get; init; }
}
