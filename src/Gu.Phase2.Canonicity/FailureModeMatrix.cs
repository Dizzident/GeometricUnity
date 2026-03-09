using System.Text.Json.Serialization;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Matrix capturing how branches fail, distinct from whether they converged.
/// Allows distinguishing solver divergence, stagnation, iteration cap,
/// extractor failure, and gauge breakdown.
/// Per IMPLEMENTATION_PLAN_P2.md Section 9.6.
/// </summary>
public sealed class FailureModeMatrix
{
    /// <summary>Branch variant IDs labeling rows and columns.</summary>
    [JsonPropertyName("branchIds")]
    public required IReadOnlyList<string> BranchIds { get; init; }

    /// <summary>
    /// Primary failure mode per branch. Null if converged normally.
    /// Values: "solver-diverged", "solver-stagnated", "max-iterations",
    /// "extractor-failed", "gauge-breakdown", "not-attempted", or null.
    /// </summary>
    [JsonPropertyName("primaryFailureModes")]
    public required IReadOnlyList<string?> PrimaryFailureModes { get; init; }

    /// <summary>
    /// Boolean matrix: SameFailureMode[i,j] = true if branches i and j
    /// failed for the same reason (including both being null/converged).
    /// </summary>
    [JsonPropertyName("sameFailureMode")]
    public required bool[,] SameFailureMode { get; init; }
}
