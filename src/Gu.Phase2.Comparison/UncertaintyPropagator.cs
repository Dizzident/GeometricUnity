using Gu.Phase2.Predictions;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Uncertainty decomposition pipeline (Section 10.5).
/// Propagates and combines uncertainty from asset and prediction sources.
/// Unestimated components (-1) are preserved, not zeroed.
/// </summary>
public static class UncertaintyPropagator
{
    /// <summary>
    /// Propagate uncertainty by combining asset uncertainty with prediction metadata.
    /// Uses the asset's uncertainty model as base and overlays prediction-derived components.
    /// </summary>
    public static UncertaintyRecord Propagate(
        UncertaintyRecord assetUncertainty,
        PredictionTestRecord prediction)
    {
        ArgumentNullException.ThrowIfNull(assetUncertainty);
        ArgumentNullException.ThrowIfNull(prediction);

        // Solver and discretization uncertainty: if prediction reports converged numerical status,
        // keep the asset's estimates; if exploratory/failed, mark as unestimated
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
