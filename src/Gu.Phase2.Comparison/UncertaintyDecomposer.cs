using Gu.Phase2.Execution;
using Gu.Phase2.Predictions;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Uncertainty decomposition pipeline (Section 10.5).
/// Propagates and combines uncertainty from asset, prediction, and branch-run sources.
/// Unestimated components (-1) are preserved, not zeroed.
/// </summary>
public static class UncertaintyDecomposer
{
    /// <summary>
    /// Decompose uncertainty by combining asset uncertainty with prediction metadata
    /// and branch run records. Uses the asset's uncertainty model as base,
    /// overlays prediction-derived components, and refines solver/discretization
    /// estimates from branch run convergence data.
    /// </summary>
    public static UncertaintyRecord Decompose(
        UncertaintyRecord assetUncertainty,
        PredictionTestRecord prediction,
        IReadOnlyList<BranchRunRecord> branchRuns)
    {
        ArgumentNullException.ThrowIfNull(assetUncertainty);
        ArgumentNullException.ThrowIfNull(prediction);
        ArgumentNullException.ThrowIfNull(branchRuns);

        double solver = assetUncertainty.Solver;
        double discretization = assetUncertainty.Discretization;
        double branch = assetUncertainty.Branch;

        if (prediction.NumericalDependencyStatus == "failed")
        {
            solver = -1;
            discretization = -1;
        }

        // Refine branch uncertainty from run records if available
        if (branchRuns.Count > 0)
        {
            int convergedCount = 0;
            double maxResidual = 0;
            foreach (var run in branchRuns)
            {
                if (run.Converged) convergedCount++;
                if (run.FinalResidualNorm > maxResidual)
                    maxResidual = run.FinalResidualNorm;
            }

            double convergenceRatio = (double)convergedCount / branchRuns.Count;

            // If all runs converged and branch uncertainty is estimated, refine it
            // by scaling with max residual norm (heuristic)
            if (convergenceRatio == 1.0 && branch >= 0 && maxResidual > 0)
            {
                branch = System.Math.Max(branch, maxResidual);
            }
            else if (convergenceRatio < 1.0 && branch >= 0)
            {
                // Some runs didn't converge: inflate branch uncertainty
                branch = System.Math.Max(branch, 1.0 - convergenceRatio);
            }
            else if (convergenceRatio < 0.5)
            {
                // Majority failed: mark branch as unestimated
                branch = -1;
            }
        }

        return new UncertaintyRecord
        {
            Discretization = discretization,
            Solver = solver,
            Branch = branch,
            Extraction = assetUncertainty.Extraction,
            Calibration = assetUncertainty.Calibration,
            DataAsset = assetUncertainty.DataAsset,
        };
    }

    /// <summary>
    /// Propagate uncertainty by combining asset uncertainty with prediction metadata.
    /// Uses the asset's uncertainty model as base and overlays prediction-derived components.
    /// Convenience overload without branch run records.
    /// </summary>
    public static UncertaintyRecord Propagate(
        UncertaintyRecord assetUncertainty,
        PredictionTestRecord prediction)
    {
        ArgumentNullException.ThrowIfNull(assetUncertainty);
        ArgumentNullException.ThrowIfNull(prediction);

        double solver = assetUncertainty.Solver;
        double discretization = assetUncertainty.Discretization;

        if (prediction.NumericalDependencyStatus == "failed")
        {
            solver = -1;
            discretization = -1;
        }

        return new UncertaintyRecord
        {
            Discretization = discretization,
            Solver = solver,
            Branch = assetUncertainty.Branch,
            Extraction = assetUncertainty.Extraction,
            Calibration = assetUncertainty.Calibration,
            DataAsset = assetUncertainty.DataAsset,
        };
    }

    /// <summary>
    /// Compute the total uncertainty as the quadrature sum of all estimated components.
    /// Unestimated components (-1) are skipped. Returns 0 if no components are estimated.
    /// </summary>
    public static double TotalUncertainty(UncertaintyRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        double sumSq = 0;
        int count = 0;

        void Add(double component)
        {
            if (component >= 0)
            {
                sumSq += component * component;
                count++;
            }
        }

        Add(record.Discretization);
        Add(record.Solver);
        Add(record.Branch);
        Add(record.Extraction);
        Add(record.Calibration);
        Add(record.DataAsset);

        return count > 0 ? System.Math.Sqrt(sumSq) : 0.0;
    }

    /// <summary>
    /// Count how many uncertainty components are unestimated (-1).
    /// </summary>
    public static int UnestimatedCount(UncertaintyRecord record)
    {
        ArgumentNullException.ThrowIfNull(record);

        int count = 0;
        if (record.Discretization < 0) count++;
        if (record.Solver < 0) count++;
        if (record.Branch < 0) count++;
        if (record.Extraction < 0) count++;
        if (record.Calibration < 0) count++;
        if (record.DataAsset < 0) count++;
        return count;
    }

    /// <summary>
    /// Returns true if all uncertainty components are estimated (none are -1).
    /// </summary>
    public static bool IsFullyEstimated(UncertaintyRecord record)
    {
        return UnestimatedCount(record) == 0;
    }
}
