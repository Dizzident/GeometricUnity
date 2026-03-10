using Gu.Phase3.ModeTracking;
using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties;

/// <summary>
/// Computes branch, refinement, and backend stability scores for mode families.
/// (See IMPLEMENTATION_PLAN_P3.md Sections 4.10.6, 4.10.7, 4.10.8)
/// </summary>
public static class StabilityScorer
{
    /// <summary>
    /// Compute branch stability score from mode records across branch variants.
    /// </summary>
    /// <param name="familyId">Family identifier.</param>
    /// <param name="branchModes">Modes keyed by branch variant ID.</param>
    public static StabilityScoreCard ComputeBranchStability(
        string familyId,
        IReadOnlyDictionary<string, ModeRecord> branchModes)
    {
        ArgumentNullException.ThrowIfNull(familyId);
        ArgumentNullException.ThrowIfNull(branchModes);

        if (branchModes.Count <= 1)
        {
            return new StabilityScoreCard
            {
                EntityId = familyId,
                BranchStability = 1.0,
                RefinementStability = 1.0,
                BackendStability = 1.0,
                BranchVariantCount = branchModes.Count,
                RefinementLevelCount = 0,
                BackendCount = 0,
            };
        }

        var eigenvalues = branchModes.Values.Select(m => m.Eigenvalue).ToList();
        double mean = eigenvalues.Average();
        double maxDrift = eigenvalues.Max() - eigenvalues.Min();
        double relDrift = System.Math.Abs(mean) > 1e-30 ? maxDrift / System.Math.Abs(mean) : maxDrift;

        // Branch stability: exp(-relDrift/threshold)
        double branchStability = System.Math.Exp(-relDrift / 0.1);

        // Overlap persistence (if mode vectors available)
        double? maxOverlapLoss = ComputeMaxOverlapLoss(branchModes.Values.ToList());

        return new StabilityScoreCard
        {
            EntityId = familyId,
            BranchStability = branchStability,
            RefinementStability = 1.0, // Placeholder until refinement data available
            BackendStability = 1.0, // Placeholder until backend comparison available
            BranchVariantCount = branchModes.Count,
            RefinementLevelCount = 0,
            BackendCount = 0,
            MaxEigenvalueDrift = maxDrift,
            MaxOverlapLoss = maxOverlapLoss,
        };
    }

    /// <summary>
    /// Compute refinement stability from modes at different discretization levels.
    /// </summary>
    public static StabilityScoreCard ComputeRefinementStability(
        string familyId,
        IReadOnlyList<ModeRecord> refinementModes)
    {
        ArgumentNullException.ThrowIfNull(familyId);
        ArgumentNullException.ThrowIfNull(refinementModes);

        if (refinementModes.Count <= 1)
        {
            return new StabilityScoreCard
            {
                EntityId = familyId,
                BranchStability = 1.0,
                RefinementStability = 1.0,
                BackendStability = 1.0,
                BranchVariantCount = 0,
                RefinementLevelCount = refinementModes.Count,
                BackendCount = 0,
            };
        }

        var eigenvalues = refinementModes.Select(m => m.Eigenvalue).ToList();
        double maxDrift = eigenvalues.Max() - eigenvalues.Min();
        double mean = eigenvalues.Average();
        double relDrift = System.Math.Abs(mean) > 1e-30 ? maxDrift / System.Math.Abs(mean) : maxDrift;

        double refinementStability = System.Math.Exp(-relDrift / 0.1);

        return new StabilityScoreCard
        {
            EntityId = familyId,
            BranchStability = 1.0,
            RefinementStability = refinementStability,
            BackendStability = 1.0,
            BranchVariantCount = 0,
            RefinementLevelCount = refinementModes.Count,
            BackendCount = 0,
            MaxEigenvalueDrift = maxDrift,
        };
    }

    /// <summary>
    /// Compute backend stability from CPU vs CUDA mode pairs.
    /// </summary>
    public static StabilityScoreCard ComputeBackendStability(
        string familyId,
        ModeRecord cpuMode,
        ModeRecord cudaMode)
    {
        ArgumentNullException.ThrowIfNull(familyId);
        ArgumentNullException.ThrowIfNull(cpuMode);
        ArgumentNullException.ThrowIfNull(cudaMode);

        double drift = System.Math.Abs(cpuMode.Eigenvalue - cudaMode.Eigenvalue);
        double mean = System.Math.Abs(cpuMode.Eigenvalue + cudaMode.Eigenvalue) / 2;
        double relDrift = mean > 1e-30 ? drift / mean : drift;

        double backendStability = System.Math.Exp(-relDrift / 0.01);

        return new StabilityScoreCard
        {
            EntityId = familyId,
            BranchStability = 1.0,
            RefinementStability = 1.0,
            BackendStability = backendStability,
            BranchVariantCount = 0,
            RefinementLevelCount = 0,
            BackendCount = 2,
            MaxEigenvalueDrift = drift,
        };
    }

    /// <summary>
    /// Merge individual stability cards into a combined score card.
    /// </summary>
    public static StabilityScoreCard Merge(string entityId, params StabilityScoreCard[] cards)
    {
        if (cards.Length == 0)
            throw new ArgumentException("At least one score card required.", nameof(cards));

        double branch = cards.Min(c => c.BranchStability);
        double refine = cards.Min(c => c.RefinementStability);
        double backend = cards.Min(c => c.BackendStability);
        int branchCount = cards.Max(c => c.BranchVariantCount);
        int refineCount = cards.Max(c => c.RefinementLevelCount);
        int backendCount = cards.Max(c => c.BackendCount);
        double? maxDrift = cards.Where(c => c.MaxEigenvalueDrift.HasValue)
            .Select(c => c.MaxEigenvalueDrift!.Value).DefaultIfEmpty().Max();
        double? maxOverlap = cards.Where(c => c.MaxOverlapLoss.HasValue)
            .Select(c => c.MaxOverlapLoss!.Value).DefaultIfEmpty().Max();

        return new StabilityScoreCard
        {
            EntityId = entityId,
            BranchStability = branch,
            RefinementStability = refine,
            BackendStability = backend,
            BranchVariantCount = branchCount,
            RefinementLevelCount = refineCount,
            BackendCount = backendCount,
            MaxEigenvalueDrift = maxDrift > 0 ? maxDrift : null,
            MaxOverlapLoss = maxOverlap > 0 ? maxOverlap : null,
        };
    }

    private static double? ComputeMaxOverlapLoss(IReadOnlyList<ModeRecord> modes)
    {
        if (modes.Count <= 1) return null;

        double minOverlap = double.MaxValue;
        for (int i = 0; i < modes.Count; i++)
        {
            for (int j = i + 1; j < modes.Count; j++)
            {
                if (modes[i].ModeVector.Length != modes[j].ModeVector.Length)
                    continue;

                double dot = 0, normI = 0, normJ = 0;
                for (int k = 0; k < modes[i].ModeVector.Length; k++)
                {
                    dot += modes[i].ModeVector[k] * modes[j].ModeVector[k];
                    normI += modes[i].ModeVector[k] * modes[i].ModeVector[k];
                    normJ += modes[j].ModeVector[k] * modes[j].ModeVector[k];
                }
                double denom = System.Math.Sqrt(normI) * System.Math.Sqrt(normJ);
                double overlap = denom > 1e-30 ? System.Math.Abs(dot) / denom : 0;
                if (overlap < minOverlap) minOverlap = overlap;
            }
        }

        return minOverlap < double.MaxValue ? 1.0 - minOverlap : null;
    }
}
