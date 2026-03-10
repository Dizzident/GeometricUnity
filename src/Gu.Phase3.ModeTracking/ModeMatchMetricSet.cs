using System.Text.Json.Serialization;

namespace Gu.Phase3.ModeTracking;

/// <summary>
/// A set of overlap/distance metrics between two modes from different contexts
/// (backgrounds, branches, refinement levels, backends).
///
/// Matching must not rely on any single metric. Use a weighted aggregate.
/// </summary>
public sealed class ModeMatchMetricSet
{
    /// <summary>
    /// O1: Native inner product overlap |v1^T M_state v2|.
    /// Only meaningful when both modes share the same state space.
    /// Null if not computable (different meshes).
    /// </summary>
    [JsonPropertyName("nativeOverlap")]
    public double? NativeOverlap { get; init; }

    /// <summary>
    /// O2: Observed signature overlap similarity(Obs(v1), Obs(v2)).
    /// Only available when ObservedModeSignature objects are provided.
    /// Null if not computable (no signatures available).
    /// </summary>
    [JsonPropertyName("observedSignatureOverlap")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ObservedSignatureOverlap { get; init; }

    /// <summary>
    /// O3: Invariant feature distance (eigenvalue, energy fractions, symmetry, etc.).
    /// Always computable. Lower = more similar.
    /// </summary>
    [JsonPropertyName("featureDistance")]
    public required double FeatureDistance { get; init; }

    /// <summary>
    /// Weighted aggregate score. Higher = better match. Range [0, 1].
    /// </summary>
    [JsonPropertyName("aggregateScore")]
    public required double AggregateScore { get; init; }

    /// <summary>Whether the aggregate score exceeds the matching threshold.</summary>
    [JsonPropertyName("isMatch")]
    public required bool IsMatch { get; init; }
}
