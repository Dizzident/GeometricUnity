namespace Gu.Phase3.Properties;

/// <summary>
/// Extracts mass-like scales by fitting the relativistic dispersion relation omega^2 = m^2 + k^2.
///
/// Requires multiple (k, omega^2) data points from either varying mesh sizes or
/// varying periodic-boundary wavevectors. For single-mode eigenvalue extraction use
/// <see cref="MassLikeScaleExtractor"/>. For background-parameter sweep fitting use
/// <see cref="LinearInterpolationMassExtractor"/>.
/// </summary>
public static class DispersionFitMassExtractor
{
    /// <summary>
    /// Fit the dispersion relation omega^2 = m^2 + k^2 to recover the mass m.
    ///
    /// Uses ordinary least squares on the model: omega^2_j = m^2 + k^2_j.
    /// Equivalently, minimizes sum_j (omega^2_j - m^2 - k^2_j)^2 over m^2.
    /// The OLS estimate is: m^2 = mean(omega^2_j - k^2_j).
    ///
    /// Special cases:
    /// - If m^2 ~ 0 (|m^2| &lt; tolerance), notes "massless" in the record.
    /// - If m^2 &lt; 0, notes "tachyonic" and returns negative MassLikeScale.
    /// - If only one point is provided, a fit cannot be validated; notes "single-point-fallback".
    /// </summary>
    /// <param name="modeId">Mode ID for provenance.</param>
    /// <param name="backgroundId">Background ID for provenance.</param>
    /// <param name="operatorType">Operator type string for provenance.</param>
    /// <param name="kValues">Momentum magnitudes k_j (&gt;= 0).</param>
    /// <param name="eigenvalueSquareds">Corresponding eigenvalues omega^2_j.</param>
    /// <param name="masslessTolerance">Threshold below which |m^2| is considered massless. Default 1e-8.</param>
    /// <returns>A <see cref="MassLikeScaleRecord"/> with ExtractionMethod="dispersion-fit".</returns>
    public static MassLikeScaleRecord ComputeFromDispersion(
        string modeId,
        string backgroundId,
        string operatorType,
        IReadOnlyList<double> kValues,
        IReadOnlyList<double> eigenvalueSquareds,
        double masslessTolerance = 1e-8)
    {
        ArgumentNullException.ThrowIfNull(modeId);
        ArgumentNullException.ThrowIfNull(backgroundId);
        ArgumentNullException.ThrowIfNull(operatorType);
        ArgumentNullException.ThrowIfNull(kValues);
        ArgumentNullException.ThrowIfNull(eigenvalueSquareds);

        if (kValues.Count == 0)
            throw new ArgumentException("At least one data point is required.", nameof(kValues));

        if (kValues.Count != eigenvalueSquareds.Count)
            throw new ArgumentException(
                $"kValues ({kValues.Count}) and eigenvalueSquareds ({eigenvalueSquareds.Count}) must have the same length.");

        // OLS estimate: m^2 = mean(omega^2_j - k^2_j)
        double sumResiduals = 0.0;
        for (int j = 0; j < kValues.Count; j++)
            sumResiduals += eigenvalueSquareds[j] - kValues[j] * kValues[j];

        double mSquared = sumResiduals / kValues.Count;

        // Derive mass-like scale and representative eigenvalue
        double massLikeScale;
        if (mSquared >= 0)
            massLikeScale = System.Math.Sqrt(mSquared);
        else
            massLikeScale = -System.Math.Sqrt(-mSquared);

        string? notes = null;
        if (kValues.Count == 1)
            notes = "single-point-fallback";
        else if (System.Math.Abs(mSquared) < masslessTolerance)
            notes = "massless";
        else if (mSquared < 0)
            notes = "tachyonic";

        return new MassLikeScaleRecord
        {
            ModeId = modeId,
            Eigenvalue = mSquared,
            MassLikeScale = massLikeScale,
            ExtractionMethod = "dispersion-fit",
            OperatorType = operatorType,
            BackgroundId = backgroundId,
            Notes = notes,
        };
    }
}
