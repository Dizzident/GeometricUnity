using System.Text.Json.Serialization;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Reporting;

/// <summary>
/// Negative result summary: records demotions, incompatible comparisons,
/// and insufficient evidence outcomes.
/// Per Section 14.4: preserve negative results as first-class outputs.
/// </summary>
public sealed class NegativeResultSummary
{
    /// <summary>Candidate ID (or "N/A" for campaign-level negatives).</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Type of negative result: "demotion", "comparison-incompatible",
    /// "comparison-underdetermined", "insufficient-evidence".</summary>
    [JsonPropertyName("resultType")]
    public required string ResultType { get; init; }

    /// <summary>Detailed description.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Original claim class before demotion (if applicable).</summary>
    [JsonPropertyName("originalClaimClass")]
    public BosonClaimClass? OriginalClaimClass { get; init; }

    /// <summary>Final claim class after demotion (if applicable).</summary>
    [JsonPropertyName("finalClaimClass")]
    public BosonClaimClass? FinalClaimClass { get; init; }
}
