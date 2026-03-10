using System.Text.Json.Serialization;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Result of comparing a single candidate boson against a target profile or external descriptor.
/// </summary>
public sealed class BosonComparisonResult
{
    /// <summary>Candidate ID that was compared.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Target profile or descriptor ID compared against.</summary>
    [JsonPropertyName("targetId")]
    public required string TargetId { get; init; }

    /// <summary>Comparison verdict.</summary>
    [JsonPropertyName("verdict")]
    public required ComparisonVerdict Verdict { get; init; }

    /// <summary>Compatibility score in [0, 1]. Higher = more compatible.</summary>
    [JsonPropertyName("compatibilityScore")]
    public double CompatibilityScore { get; init; }

    /// <summary>Detailed notes explaining the verdict.</summary>
    [JsonPropertyName("notes")]
    public IReadOnlyList<string> Notes { get; init; } = Array.Empty<string>();

    /// <summary>Candidate claim class at comparison time.</summary>
    [JsonPropertyName("candidateClaimClass")]
    public BosonClaimClass CandidateClaimClass { get; init; }
}
