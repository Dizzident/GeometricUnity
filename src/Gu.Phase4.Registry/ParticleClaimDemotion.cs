using System.Text.Json.Serialization;

namespace Gu.Phase4.Registry;

/// <summary>
/// Records a single demotion event applied to a particle claim.
/// Demotions lower the claim class or invalidate a candidate.
/// </summary>
public sealed class ParticleClaimDemotion
{
    /// <summary>Reason code for the demotion (e.g., "UnverifiedGpu", "LowPersistence").</summary>
    [JsonPropertyName("reason")]
    public required string Reason { get; init; }

    /// <summary>Human-readable details about why the demotion was applied.</summary>
    [JsonPropertyName("details")]
    public required string Details { get; init; }

    /// <summary>The claim class that was demoted from.</summary>
    [JsonPropertyName("fromClaimClass")]
    public required string FromClaimClass { get; init; }

    /// <summary>The claim class after demotion.</summary>
    [JsonPropertyName("toClaimClass")]
    public required string ToClaimClass { get; init; }

    /// <summary>UTC timestamp of the demotion.</summary>
    [JsonPropertyName("demotedAt")]
    public DateTimeOffset DemotedAt { get; init; } = DateTimeOffset.UtcNow;
}
