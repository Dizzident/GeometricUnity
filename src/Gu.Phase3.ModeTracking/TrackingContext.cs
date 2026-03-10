using System.Text.Json.Serialization;

namespace Gu.Phase3.ModeTracking;

/// <summary>
/// Describes the type of context change being tracked.
/// </summary>
public enum TrackingContextType
{
    /// <summary>Tracking across background continuation steps.</summary>
    Continuation,

    /// <summary>Tracking across branch variant changes.</summary>
    BranchSweep,

    /// <summary>Tracking across mesh refinement levels.</summary>
    Refinement,

    /// <summary>Tracking across backend changes (CPU vs GPU).</summary>
    Backend,
}

/// <summary>
/// Configuration for mode tracking across a specific context change.
/// </summary>
public sealed class TrackingConfig
{
    /// <summary>Type of context change.</summary>
    [JsonPropertyName("contextType")]
    public required TrackingContextType ContextType { get; init; }

    /// <summary>
    /// Matching threshold: mode pairs with aggregate score below this are unmatched.
    /// </summary>
    [JsonPropertyName("matchThreshold")]
    public double MatchThreshold { get; init; } = 0.5;

    /// <summary>
    /// Ambiguity threshold: if the best and second-best matches differ by less than
    /// this ratio, the match is flagged as ambiguous.
    /// </summary>
    [JsonPropertyName("ambiguityRatio")]
    public double AmbiguityRatio { get; init; } = 0.8;

    /// <summary>Weight for native overlap (O1) in aggregate score.</summary>
    [JsonPropertyName("nativeOverlapWeight")]
    public double NativeOverlapWeight { get; init; } = 0.3;

    /// <summary>Weight for observed signature overlap (O2) in aggregate score.</summary>
    [JsonPropertyName("observedOverlapWeight")]
    public double ObservedOverlapWeight { get; init; } = 0.4;

    /// <summary>Weight for feature distance (O3) in aggregate score.</summary>
    [JsonPropertyName("featureDistanceWeight")]
    public double FeatureDistanceWeight { get; init; } = 0.3;

    /// <summary>
    /// Scale for feature distance -> score conversion: score = exp(-dist/scale).
    /// </summary>
    [JsonPropertyName("featureDistanceScale")]
    public double FeatureDistanceScale { get; init; } = 1.0;

    /// <summary>
    /// Threshold for split/merge detection: a source-target pair must score
    /// at least this value for the pair to be considered part of a split or merge.
    /// </summary>
    [JsonPropertyName("splitThreshold")]
    public double SplitThreshold { get; init; } = 0.4;

    /// <summary>
    /// Relative degeneracy threshold for avoided-crossing detection.
    /// Two eigenvalues are near-degenerate if |lambda_A - lambda_B| / max(|lambda_A|, |lambda_B|, eps) is below this value.
    /// </summary>
    [JsonPropertyName("avoidedCrossingDegeneracyThreshold")]
    public double AvoidedCrossingDegeneracyThreshold { get; init; } = 0.1;
}
