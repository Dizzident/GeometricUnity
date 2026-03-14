namespace Gu.Phase5.Convergence;

/// <summary>
/// Output of a refinement convergence study (M47).
/// Contains run records, successful continuum estimates, and failure records.
/// </summary>
public sealed class RefinementStudyResult
{
    /// <summary>Study identifier from the spec.</summary>
    public required string StudyId { get; init; }

    /// <summary>Run record for each refinement level.</summary>
    public required IReadOnlyList<RefinementRunRecord> RunRecords { get; init; }

    /// <summary>
    /// Continuum estimates for quantities that converged.
    /// </summary>
    public required IReadOnlyList<ContinuumEstimateRecord> ContinuumEstimates { get; init; }

    /// <summary>
    /// Failure records for quantities that did not converge.
    /// Non-convergence is a valid scientific result — always recorded.
    /// </summary>
    public required IReadOnlyList<ConvergenceFailureRecord> FailureRecords { get; init; }
}
