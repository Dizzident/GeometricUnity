namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Configuration for fermion mode tracking across branch variants and refinement.
/// </summary>
public sealed class FermionTrackingConfig
{
    /// <summary>
    /// Minimum aggregate match score [0, 1] to consider two modes the same family.
    /// </summary>
    public double MatchThreshold { get; init; } = 0.5;

    /// <summary>
    /// Scale factor for eigenvalue-band similarity score.
    /// Two modes with |lambda_i - lambda_j| / max(|lambda_i|, 1e-14) &lt; EigenvalueRelTol
    /// get a high eigenvalue similarity score.
    /// </summary>
    public double EigenvalueRelTol { get; init; } = 0.3;

    /// <summary>
    /// Weight for eigenvalue band similarity [0, 1].
    /// </summary>
    public double EigenvalueWeight { get; init; } = 0.4;

    /// <summary>
    /// Weight for chirality profile similarity [0, 1].
    /// </summary>
    public double ChiralityWeight { get; init; } = 0.3;

    /// <summary>
    /// Weight for eigenspace overlap (if eigenvectors available) [0, 1].
    /// </summary>
    public double EigenspaceWeight { get; init; } = 0.3;

    /// <summary>
    /// Ambiguity margin: if best and second-best aggregate scores are within this
    /// margin of each other, the match is flagged as ambiguous.
    /// </summary>
    public double AmbiguityMargin { get; init; } = 0.1;

    /// <summary>
    /// Maximum ratio of branch-persistence successes to attempts to get score = 1.
    /// If a mode is present in all branches, its branch persistence score = 1.
    /// </summary>
    public double FullPersistenceThreshold { get; init; } = 1.0;
}
