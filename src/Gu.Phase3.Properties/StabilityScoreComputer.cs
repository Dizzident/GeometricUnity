using Gu.Phase3.ModeTracking;

namespace Gu.Phase3.Properties;

/// <summary>
/// Computes stability score cards from mode family tracking data.
/// Aggregates branch stability, refinement stability, and backend stability
/// into [0,1] scores.
/// (See IMPLEMENTATION_PLAN_P3.md Sections 4.10.6 - 4.10.8)
///
/// Branch stability: measures eigenvalue persistence across branch variants.
/// Refinement stability: measures eigenvalue convergence under mesh refinement.
/// Backend stability: measures agreement between CPU and CUDA pipelines.
/// </summary>
public static class StabilityScoreComputer
{
    /// <summary>
    /// Compute stability score card from a mode family and its member eigenvalues.
    /// </summary>
    /// <param name="family">The mode family record.</param>
    /// <param name="branchEigenvalues">Eigenvalues from different branch variants.</param>
    /// <param name="refinementEigenvalues">Eigenvalues from different refinement levels.</param>
    /// <param name="backendEigenvalues">Eigenvalues from different backends.</param>
    /// <param name="referenceMass">Reference mass scale for normalization (if zero, uses mean).</param>
    /// <returns>Stability score card.</returns>
    public static StabilityScoreCard Compute(
        ModeFamilyRecord family,
        IReadOnlyList<double> branchEigenvalues,
        IReadOnlyList<double> refinementEigenvalues,
        IReadOnlyList<double> backendEigenvalues,
        double referenceMass = 0)
    {
        ArgumentNullException.ThrowIfNull(family);
        ArgumentNullException.ThrowIfNull(branchEigenvalues);
        ArgumentNullException.ThrowIfNull(refinementEigenvalues);
        ArgumentNullException.ThrowIfNull(backendEigenvalues);

        double branchStability = ComputeStabilityFromEigenvalues(branchEigenvalues, referenceMass);
        double refinementStability = ComputeStabilityFromEigenvalues(refinementEigenvalues, referenceMass);
        double backendStability = ComputeStabilityFromEigenvalues(backendEigenvalues, referenceMass);

        double? maxDrift = ComputeMaxDrift(branchEigenvalues);

        return new StabilityScoreCard
        {
            EntityId = family.FamilyId,
            BranchStability = branchStability,
            RefinementStability = refinementStability,
            BackendStability = backendStability,
            BranchVariantCount = branchEigenvalues.Count,
            RefinementLevelCount = refinementEigenvalues.Count,
            BackendCount = backendEigenvalues.Count,
            MaxEigenvalueDrift = maxDrift,
        };
    }

    /// <summary>
    /// Compute stability from a set of eigenvalues.
    /// Stability = 1 - (spread / scale), clamped to [0, 1].
    /// If fewer than 2 values, returns 1.0 (no evidence of instability).
    /// </summary>
    public static double ComputeStabilityFromEigenvalues(IReadOnlyList<double> eigenvalues, double referenceMass = 0)
    {
        ArgumentNullException.ThrowIfNull(eigenvalues);

        if (eigenvalues.Count < 2) return 1.0;

        double min = double.MaxValue;
        double max = double.MinValue;
        double sum = 0;

        foreach (double ev in eigenvalues)
        {
            if (ev < min) min = ev;
            if (ev > max) max = ev;
            sum += ev;
        }

        double mean = sum / eigenvalues.Count;
        double spread = max - min;

        // Reference scale: use provided reference, or the mean magnitude, or 1.0 as fallback
        double scale = referenceMass > 0
            ? referenceMass
            : System.Math.Max(System.Math.Abs(mean), 1.0);

        double relativeSpread = spread / scale;

        // Stability = 1 - relative spread, clamped
        return System.Math.Max(0.0, System.Math.Min(1.0, 1.0 - relativeSpread));
    }

    /// <summary>
    /// Compute maximum drift (max - min) from a set of eigenvalues.
    /// Returns null if fewer than 2 values.
    /// </summary>
    public static double? ComputeMaxDrift(IReadOnlyList<double> eigenvalues)
    {
        ArgumentNullException.ThrowIfNull(eigenvalues);

        if (eigenvalues.Count < 2) return null;

        double min = double.MaxValue;
        double max = double.MinValue;
        foreach (double ev in eigenvalues)
        {
            if (ev < min) min = ev;
            if (ev > max) max = ev;
        }
        return max - min;
    }

    /// <summary>
    /// Compute a simple stability score card with uniform stability from an entity ID
    /// and a single set of eigenvalues (treated as branch eigenvalues).
    /// </summary>
    public static StabilityScoreCard FromSingleContext(string entityId, IReadOnlyList<double> eigenvalues)
    {
        ArgumentNullException.ThrowIfNull(entityId);
        ArgumentNullException.ThrowIfNull(eigenvalues);

        double stability = ComputeStabilityFromEigenvalues(eigenvalues);

        return new StabilityScoreCard
        {
            EntityId = entityId,
            BranchStability = stability,
            RefinementStability = 1.0,
            BackendStability = 1.0,
            BranchVariantCount = eigenvalues.Count,
            RefinementLevelCount = 1,
            BackendCount = 1,
            MaxEigenvalueDrift = ComputeMaxDrift(eigenvalues),
        };
    }
}
