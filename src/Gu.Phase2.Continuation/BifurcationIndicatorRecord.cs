using System.Text.Json.Serialization;

namespace Gu.Phase2.Continuation;

/// <summary>
/// A classified bifurcation indicator with structured metadata.
/// Replaces raw <see cref="ContinuationEvent"/> usage in <see cref="StabilityAtlas.BifurcationIndicators"/>.
/// Per IMPLEMENTATION_PLAN_P2.md Section 8.2.
/// </summary>
public sealed class BifurcationIndicatorRecord
{
    /// <summary>Unique identifier for this indicator.</summary>
    [JsonPropertyName("indicatorId")]
    public required string IndicatorId { get; init; }

    /// <summary>Parameter value at which the bifurcation was detected.</summary>
    [JsonPropertyName("lambda")]
    public required double Lambda { get; init; }

    /// <summary>
    /// Classification kind: "fold", "branch-point", "hopf-candidate", "sign-change", "unknown".
    /// </summary>
    [JsonPropertyName("kind")]
    public required string Kind { get; init; }

    /// <summary>The continuation event kind that triggered this indicator.</summary>
    [JsonPropertyName("triggeringEvent")]
    public required ContinuationEventKind TriggeringEvent { get; init; }

    /// <summary>Which eigenmode was involved, if known.</summary>
    [JsonPropertyName("modeIndex")]
    public int? ModeIndex { get; init; }

    /// <summary>Eigenvalue at the point of detection, if available.</summary>
    [JsonPropertyName("eigenvalueAtDetection")]
    public double? EigenvalueAtDetection { get; init; }

    /// <summary>
    /// Confidence level: "numerical-only", "strong-numerical", "theorem-supported".
    /// </summary>
    [JsonPropertyName("confidence")]
    public required string Confidence { get; init; }

    /// <summary>Status of any theorem dependency for this classification.</summary>
    [JsonPropertyName("theoremDependencyStatus")]
    public required string TheoremDependencyStatus { get; init; }

    /// <summary>Human-readable description of the bifurcation indicator.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }
}
