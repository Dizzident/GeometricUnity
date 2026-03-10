using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Result of comparing a single candidate boson against a single target profile.
///
/// The system never forces unique match: a candidate may be Compatible with
/// multiple targets, and a target may have multiple Compatible candidates.
/// </summary>
public sealed class ComparisonResult
{
    /// <summary>Candidate boson ID.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Target profile ID.</summary>
    [JsonPropertyName("profileId")]
    public required string ProfileId { get; init; }

    /// <summary>Comparison outcome.</summary>
    [JsonPropertyName("outcome")]
    public required ComparisonOutcome Outcome { get; init; }

    /// <summary>Mass compatibility score [0,1]. Higher = better match.</summary>
    [JsonPropertyName("massScore")]
    public required double MassScore { get; init; }

    /// <summary>Multiplicity compatibility score [0,1].</summary>
    [JsonPropertyName("multiplicityScore")]
    public required double MultiplicityScore { get; init; }

    /// <summary>Gauge leak score (lower = better, physical modes have low leak).</summary>
    [JsonPropertyName("gaugeLeakScore")]
    public required double GaugeLeakScore { get; init; }

    /// <summary>Diagnostic details explaining the outcome.</summary>
    [JsonPropertyName("details")]
    public required string Details { get; init; }
}
