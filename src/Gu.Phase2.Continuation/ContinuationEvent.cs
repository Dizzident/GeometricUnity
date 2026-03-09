using System.Text.Json.Serialization;

namespace Gu.Phase2.Continuation;

/// <summary>
/// Types of events detected during continuation.
/// Per IMPLEMENTATION_PLAN_P2.md Section 10.2 required event detectors.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContinuationEventKind
{
    /// <summary>Smallest singular value approaching zero -- near-singularity.</summary>
    SingularValueCollapse,

    /// <summary>Sign change in Hessian spectrum -- stability boundary crossing.</summary>
    HessianSignChange,

    /// <summary>Corrector fails repeatedly in a small parameter range.</summary>
    StepRejectionBurst,

    /// <summary>Qualitative extractor output changes discontinuously.</summary>
    ExtractorFailure,

    /// <summary>Potential branch point (merge or split in solution family).</summary>
    BranchMergeSplitCandidate,

    /// <summary>Gauge-slice constraint becomes ill-conditioned.</summary>
    GaugeSliceBreakdown,
}

/// <summary>
/// A detected event during a continuation step.
/// </summary>
public sealed class ContinuationEvent
{
    /// <summary>Kind of event detected.</summary>
    [JsonPropertyName("kind")]
    public required ContinuationEventKind Kind { get; init; }

    /// <summary>Parameter value at which the event was detected.</summary>
    [JsonPropertyName("lambda")]
    public required double Lambda { get; init; }

    /// <summary>Human-readable description of the event.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Severity: "info", "warning", "critical".</summary>
    [JsonPropertyName("severity")]
    public required string Severity { get; init; }
}
