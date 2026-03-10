namespace Gu.Phase3.Properties;

/// <summary>
/// Extracts mass-like scales from dispersion relation fitting.
///
/// Given a list of (backgroundParameter, massLikeScale) pairs sampled across
/// a continuation path, fits a linear dispersion relation and returns a
/// mass-like scale record with ExtractionMethod = "dispersion-fit".
///
/// // Full dispersion fit requires campaign-level continuation data (Phase IV)
/// </summary>
public static class DispersionFitMassExtractor
{
    /// <summary>
    /// Compute a dispersion-fit mass-like scale from sampled data points.
    ///
    /// Current implementation: linear slope fit via finite differences.
    /// A full dispersion relation fit (e.g., omega^2 = m^2 + k^2) requires
    /// campaign-level continuation data and is deferred to Phase IV.
    /// </summary>
    /// <param name="modeId">Mode ID for provenance.</param>
    /// <param name="backgroundId">Background ID for provenance.</param>
    /// <param name="operatorType">Operator type string for provenance.</param>
    /// <param name="samples">
    /// List of (backgroundParameter, massLikeScale) pairs sampled across
    /// a continuation path or parameter sweep.
    /// </param>
    /// <returns>A <see cref="MassLikeScaleRecord"/> with ExtractionMethod="dispersion-fit".</returns>
    public static MassLikeScaleRecord Compute(
        string modeId,
        string backgroundId,
        string operatorType,
        IReadOnlyList<(double BackgroundParameter, double MassLikeScale)> samples)
    {
        ArgumentNullException.ThrowIfNull(modeId);
        ArgumentNullException.ThrowIfNull(backgroundId);
        ArgumentNullException.ThrowIfNull(operatorType);
        ArgumentNullException.ThrowIfNull(samples);

        if (samples.Count == 0)
            throw new ArgumentException("At least one sample is required.", nameof(samples));

        // Linear slope fit: use average finite differences as the fitted mass-like scale.
        // For a single point, just return that point's mass-like scale.
        double fittedScale;
        double representativeEigenvalue;

        if (samples.Count == 1)
        {
            fittedScale = samples[0].MassLikeScale;
            representativeEigenvalue = fittedScale * fittedScale;
        }
        else
        {
            // Linear regression: massLikeScale = a + b * backgroundParameter
            // Intercept 'a' is the fitted mass-like scale at backgroundParameter = 0.
            double sumX = 0, sumY = 0, sumXX = 0, sumXY = 0;
            for (int i = 0; i < samples.Count; i++)
            {
                double x = samples[i].BackgroundParameter;
                double y = samples[i].MassLikeScale;
                sumX += x;
                sumY += y;
                sumXX += x * x;
                sumXY += x * y;
            }

            int n = samples.Count;
            double denom = n * sumXX - sumX * sumX;

            if (System.Math.Abs(denom) < 1e-30)
            {
                // All samples at the same parameter value; use mean mass-like scale
                fittedScale = sumY / n;
            }
            else
            {
                // Intercept: a = (sumY * sumXX - sumX * sumXY) / denom
                fittedScale = (sumY * sumXX - sumX * sumXY) / denom;
            }

            representativeEigenvalue = fittedScale >= 0
                ? fittedScale * fittedScale
                : -(fittedScale * fittedScale);
        }

        return new MassLikeScaleRecord
        {
            ModeId = modeId,
            Eigenvalue = representativeEigenvalue,
            MassLikeScale = fittedScale,
            ExtractionMethod = "dispersion-fit",
            OperatorType = operatorType,
            BackgroundId = backgroundId,
        };
    }
}
