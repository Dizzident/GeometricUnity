using System.Text.Json.Serialization;

namespace Gu.Phase3.Registry;

/// <summary>
/// Reason for demoting a candidate boson's claim class.
/// </summary>
public enum DemotionReason
{
    /// <summary>Gauge leak score exceeds threshold.</summary>
    GaugeLeak,

    /// <summary>Mode does not persist under mesh refinement.</summary>
    RefinementFragility,

    /// <summary>Mode does not persist across branch variants.</summary>
    BranchFragility,

    /// <summary>Observed-space signature is unstable.</summary>
    ObservationInstability,

    /// <summary>Comparison with external target gives mismatch.</summary>
    ComparisonMismatch,

    /// <summary>Backend (CPU vs GPU) parity failure.</summary>
    BackendFragility,

    /// <summary>Ambiguous matching (multiple possible identifications).</summary>
    AmbiguousMatching,
}

/// <summary>
/// Records a demotion event: why a candidate was demoted and by how much.
/// </summary>
public sealed class BosonDemotionRecord
{
    /// <summary>Candidate ID that was demoted.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Reason for demotion.</summary>
    [JsonPropertyName("reason")]
    public required DemotionReason Reason { get; init; }

    /// <summary>Claim class before demotion.</summary>
    [JsonPropertyName("previousClaimClass")]
    public required BosonClaimClass PreviousClaimClass { get; init; }

    /// <summary>Claim class after demotion.</summary>
    [JsonPropertyName("demotedClaimClass")]
    public required BosonClaimClass DemotedClaimClass { get; init; }

    /// <summary>Diagnostic details about the demotion trigger.</summary>
    [JsonPropertyName("details")]
    public required string Details { get; init; }

    /// <summary>Metric value that triggered the demotion.</summary>
    [JsonPropertyName("triggerValue")]
    public double? TriggerValue { get; init; }

    /// <summary>Threshold that was exceeded.</summary>
    [JsonPropertyName("threshold")]
    public double? Threshold { get; init; }
}
