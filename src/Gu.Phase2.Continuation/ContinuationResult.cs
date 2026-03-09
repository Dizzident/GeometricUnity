using System.Text.Json.Serialization;

namespace Gu.Phase2.Continuation;

/// <summary>
/// Full result of a continuation run: the path of converged steps,
/// termination reason, and summary statistics.
/// </summary>
public sealed class ContinuationResult
{
    /// <summary>The continuation specification that produced this result.</summary>
    [JsonPropertyName("spec")]
    public required ContinuationSpec Spec { get; init; }

    /// <summary>Converged steps along the continuation path.</summary>
    [JsonPropertyName("path")]
    public required IReadOnlyList<ContinuationStep> Path { get; init; }

    /// <summary>
    /// Termination reason: "reached-end", "max-steps", "min-step-size",
    /// "corrector-failure", "singular-point", "user-requested".
    /// </summary>
    [JsonPropertyName("terminationReason")]
    public required string TerminationReason { get; init; }

    /// <summary>Total number of steps completed.</summary>
    [JsonPropertyName("totalSteps")]
    public required int TotalSteps { get; init; }

    /// <summary>Total arclength traversed.</summary>
    [JsonPropertyName("totalArclength")]
    public required double TotalArclength { get; init; }

    /// <summary>Parameter range covered: [lambdaMin, lambdaMax].</summary>
    [JsonPropertyName("lambdaMin")]
    public required double LambdaMin { get; init; }

    /// <summary>Parameter range covered: [lambdaMin, lambdaMax].</summary>
    [JsonPropertyName("lambdaMax")]
    public required double LambdaMax { get; init; }

    /// <summary>All events detected across all steps.</summary>
    [JsonPropertyName("allEvents")]
    public required IReadOnlyList<ContinuationEvent> AllEvents { get; init; }

    /// <summary>Timestamp when this result was produced.</summary>
    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }
}
