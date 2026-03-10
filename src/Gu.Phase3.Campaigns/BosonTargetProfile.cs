using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Comparison verdict for a boson candidate against a target profile.
/// Per Section 7.10: never force a unique match.
/// </summary>
public enum ComparisonVerdict
{
    /// <summary>Candidate is compatible with the target profile.</summary>
    Compatible,

    /// <summary>Candidate is incompatible with the target profile.</summary>
    Incompatible,

    /// <summary>Not enough evidence to determine compatibility.</summary>
    Underdetermined,

    /// <summary>Insufficient data for any conclusion.</summary>
    InsufficientEvidence,
}

/// <summary>
/// Abstract target descriptor for internal comparisons (BC1 mode).
/// Describes what a bosonic candidate should look like without
/// committing to a specific physical particle.
/// </summary>
public sealed class BosonTargetProfile
{
    /// <summary>Unique target profile identifier.</summary>
    [JsonPropertyName("profileId")]
    public required string ProfileId { get; init; }

    /// <summary>Human-readable label (e.g., "massless vector-like").</summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>Expected mass-like eigenvalue range [min, max]. Null = any.</summary>
    [JsonPropertyName("massRange")]
    public double[]? MassRange { get; init; }

    /// <summary>Expected multiplicity. Null = any.</summary>
    [JsonPropertyName("expectedMultiplicity")]
    public int? ExpectedMultiplicity { get; init; }

    /// <summary>Maximum allowed gauge leak score. Null = no constraint.</summary>
    [JsonPropertyName("maxGaugeLeak")]
    public double? MaxGaugeLeak { get; init; }

    /// <summary>Minimum required branch stability. Null = no constraint.</summary>
    [JsonPropertyName("minBranchStability")]
    public double? MinBranchStability { get; init; }

    /// <summary>Minimum required claim class. Null = any.</summary>
    [JsonPropertyName("minClaimClass")]
    public Registry.BosonClaimClass? MinClaimClass { get; init; }

    /// <summary>Descriptive tags (e.g., "vector-like", "scalar-like", "low-leak").</summary>
    [JsonPropertyName("tags")]
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}
