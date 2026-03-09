using System.Text.Json.Serialization;
using Gu.Phase2.Predictions;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Result of a single comparison within a campaign.
/// </summary>
public sealed class ComparisonRunRecord
{
    /// <summary>Test ID from the PredictionTestRecord.</summary>
    [JsonPropertyName("testId")]
    public required string TestId { get; init; }

    /// <summary>Comparison mode used.</summary>
    [JsonPropertyName("mode")]
    public required ComparisonMode Mode { get; init; }

    /// <summary>Comparison score (interpretation depends on mode).</summary>
    [JsonPropertyName("score")]
    public required double Score { get; init; }

    /// <summary>Whether this comparison is considered a pass.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Uncertainty decomposition for this comparison.</summary>
    [JsonPropertyName("uncertainty")]
    public required UncertaintyRecord Uncertainty { get; init; }

    /// <summary>Resolved claim class after validation and comparison.</summary>
    [JsonPropertyName("resolvedClaimClass")]
    public required ClaimClass ResolvedClaimClass { get; init; }

    /// <summary>Human-readable summary of the comparison outcome.</summary>
    [JsonPropertyName("summary")]
    public required string Summary { get; init; }
}
