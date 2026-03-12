using System.Text.Json.Serialization;

namespace Gu.Phase4.Registry;

/// <summary>
/// Configuration for UnifiedRegistryBuilder.
/// </summary>
public sealed class UnifiedRegistryConfig
{
    /// <summary>
    /// Minimum branch persistence score required for a fermion cluster to receive C3 or above.
    /// Default 0.5.
    /// </summary>
    [JsonPropertyName("stabilityThreshold")]
    public double StabilityThreshold { get; init; } = 0.5;

    /// <summary>
    /// Ambiguity score above which an AmbiguousMatching demotion note is added.
    /// Default 0.5.
    /// </summary>
    [JsonPropertyName("ambiguityThreshold")]
    public double AmbiguityThreshold { get; init; } = 0.5;

    /// <summary>
    /// Minimum coupling proxy magnitude for an interaction record to be included.
    /// Default 0 (include all).
    /// </summary>
    [JsonPropertyName("minCouplingMagnitude")]
    public double MinCouplingMagnitude { get; init; } = 0.0;

    /// <summary>Default configuration.</summary>
    public static UnifiedRegistryConfig Default { get; } = new();
}
