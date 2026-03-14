using System.Text.Json.Serialization;

namespace Gu.Phase5.Convergence;

/// <summary>
/// Records a convergence failure for one target quantity (M47).
/// Produced when Richardson extrapolation cannot produce a ContinuumEstimateRecord.
/// Non-convergence is a valid scientific result — record it, don't suppress it.
/// </summary>
public sealed class ConvergenceFailureRecord
{
    /// <summary>Target quantity identifier.</summary>
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    /// <summary>
    /// Failure type:
    /// "non-convergent"    -- deltas do not decrease across levels
    /// "insufficient-data" -- fewer than 3 refinement levels available
    /// "solver-failure"    -- solver did not converge at one or more levels
    /// "fit-failure"       -- Richardson fit residual too large to trust
    /// </summary>
    [JsonPropertyName("failureType")]
    public required string FailureType { get; init; }

    /// <summary>Human-readable description of the failure.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Observed quantity values at each mesh parameter (may be partial).</summary>
    [JsonPropertyName("observedValues")]
    public required double[] ObservedValues { get; init; }

    /// <summary>Mesh parameters at which runs were attempted.</summary>
    [JsonPropertyName("meshParameters")]
    public required double[] MeshParameters { get; init; }
}
