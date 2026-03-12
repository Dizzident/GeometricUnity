namespace Gu.Phase3.Properties;

/// <summary>
/// Extracts mass-like scales by linear interpolation in background-parameter space.
///
/// Given a list of (backgroundParameter, massLikeScale) pairs sampled across a
/// continuation path, fits a linear model (massLikeScale = a + b * backgroundParameter)
/// and returns the intercept 'a' as the mass-like scale at backgroundParameter = 0.
///
/// This is NOT a dispersion-relation fit. For the physically motivated dispersion fit
/// (omega^2 = m^2 + k^2) see <see cref="DispersionFitMassExtractor.ComputeFromDispersion"/>.
/// </summary>
public static class LinearInterpolationMassExtractor
{
    /// <summary>
    /// Compute a mass-like scale by linear interpolation across background-parameter samples.
    /// </summary>
    /// <param name="modeId">Mode ID for provenance.</param>
    /// <param name="backgroundId">Background ID for provenance.</param>
    /// <param name="operatorType">Operator type string for provenance.</param>
    /// <param name="samples">
    /// List of (backgroundParameter, massLikeScale) pairs sampled across
    /// a continuation path or parameter sweep.
    /// </param>
    /// <returns>A <see cref="MassLikeScaleRecord"/> with ExtractionMethod="linear-interpolation".</returns>
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
            // Intercept 'a' is the mass-like scale at backgroundParameter = 0.
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
            ExtractionMethod = "linear-interpolation",
            OperatorType = operatorType,
            BackgroundId = backgroundId,
        };
    }
}
