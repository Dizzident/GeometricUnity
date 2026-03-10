namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Grades a background state by its admissibility level (B0/B1/B2/Rejected).
/// See Section 4.1 for the three-level hierarchy.
/// </summary>
public sealed class AdmissibilityGrader
{
    private readonly double _toleranceResidualDiagnostic;
    private readonly double _toleranceStationary;
    private readonly double _toleranceResidualStrict;

    public AdmissibilityGrader(BackgroundSolveOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _toleranceResidualDiagnostic = options.ToleranceResidualDiagnostic;
        _toleranceStationary = options.ToleranceStationary;
        _toleranceResidualStrict = options.ToleranceResidualStrict;
    }

    public AdmissibilityGrader(
        double toleranceResidualDiagnostic,
        double toleranceStationary,
        double toleranceResidualStrict)
    {
        _toleranceResidualDiagnostic = toleranceResidualDiagnostic;
        _toleranceStationary = toleranceStationary;
        _toleranceResidualStrict = toleranceResidualStrict;
    }

    /// <summary>
    /// Grade a background by its residual and stationarity norms.
    /// </summary>
    /// <param name="residualNorm">||Upsilon_h(z_*)||</param>
    /// <param name="stationarityNorm">||G_h(z_*)||</param>
    /// <returns>The admissibility level and an optional rejection reason.</returns>
    public (AdmissibilityLevel Level, string? RejectionReason) Grade(
        double residualNorm, double stationarityNorm)
    {
        // B2: stationary AND strict residual
        if (stationarityNorm <= _toleranceStationary &&
            residualNorm <= _toleranceResidualStrict)
        {
            return (AdmissibilityLevel.B2, null);
        }

        // B1: stationary
        if (stationarityNorm <= _toleranceStationary)
        {
            return (AdmissibilityLevel.B1, null);
        }

        // B0: diagnostic (low residual only)
        if (residualNorm <= _toleranceResidualDiagnostic)
        {
            return (AdmissibilityLevel.B0, null);
        }

        // Rejected
        return (AdmissibilityLevel.Rejected,
            $"ResidualNorm={residualNorm:E6} > tol_diagnostic={_toleranceResidualDiagnostic:E6}, " +
            $"StationarityNorm={stationarityNorm:E6} > tol_stationary={_toleranceStationary:E6}");
    }
}
