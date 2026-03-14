namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Propagates uncertainty from Phase V sources into a QuantitativeUncertainty record (M49).
///
/// Sources (physicist-confirmed):
///   branchVariation      = std dev of quantity across branch variants (from BranchRobustnessRecord)
///   refinementError      = error band from continuum estimate (from ContinuumEstimateRecord.ErrorBand)
///   extractionError      = from observation chain (fixed estimate or sensitivity)
///   environmentSensitivity = std dev across environment tiers
///   totalUncertainty     = sqrt(sum of squares of estimated components)
///
/// Convention: pass null for unestimated components; they are stored as -1 (not 0).
/// </summary>
public static class UncertaintyPropagator
{
    /// <summary>
    /// Build a QuantitativeUncertainty from the four independent sources.
    /// Null inputs become -1 (unestimated).
    /// Total is computed from all estimated (non-null) components via quadrature.
    /// </summary>
    public static QuantitativeUncertainty Propagate(
        double? branchVariation,
        double? refinementError,
        double? extractionError,
        double? environmentSensitivity)
    {
        double bv = branchVariation ?? -1;
        double re = refinementError ?? -1;
        double ee = extractionError ?? -1;
        double es = environmentSensitivity ?? -1;

        // Quadrature sum of estimated components
        double sumSq = 0;
        int count = 0;
        if (bv >= 0) { sumSq += bv * bv; count++; }
        if (re >= 0) { sumSq += re * re; count++; }
        if (ee >= 0) { sumSq += ee * ee; count++; }
        if (es >= 0) { sumSq += es * es; count++; }

        double total = count > 0 ? System.Math.Sqrt(sumSq) : -1;

        return new QuantitativeUncertainty
        {
            BranchVariation = bv,
            RefinementError = re,
            ExtractionError = ee,
            EnvironmentSensitivity = es,
            TotalUncertainty = total,
        };
    }
}
