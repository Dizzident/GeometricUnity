using System.Text.Json.Serialization;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Comparison;

/// <summary>
/// First-class artifact for a failed comparison (Section 13.5).
/// Failed comparisons are preserved, never filtered.
/// </summary>
public sealed class ComparisonFailureRecord
{
    /// <summary>Test ID that failed.</summary>
    [JsonPropertyName("testId")]
    public required string TestId { get; init; }

    /// <summary>Reason for failure.</summary>
    [JsonPropertyName("failureReason")]
    public required string FailureReason { get; init; }

    /// <summary>Failure level: "numerical", "branch-local", "extraction", "empirical".</summary>
    [JsonPropertyName("failureLevel")]
    public required string FailureLevel { get; init; }

    /// <summary>Whether this failure falsifies the prediction record.</summary>
    [JsonPropertyName("falsifiesRecord")]
    public required bool FalsifiesRecord { get; init; }

    /// <summary>Whether this failure blocks the entire campaign.</summary>
    [JsonPropertyName("blocksCampaign")]
    public required bool BlocksCampaign { get; init; }

    /// <summary>The demoted claim class resulting from this failure.</summary>
    [JsonPropertyName("demotedClaimClass")]
    public required ClaimClass DemotedClaimClass { get; init; }
}
