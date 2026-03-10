using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties;

/// <summary>
/// Extracts mass-like scales from mode eigenvalues.
/// (See IMPLEMENTATION_PLAN_P3.md Section 4.10.2)
///
/// Convention: mass-like scale = sqrt(|lambda|), with sign matching lambda.
/// </summary>
public static class MassLikeScaleExtractor
{
    /// <summary>
    /// Extract mass-like scale from a single mode record.
    /// </summary>
    public static MassLikeScaleRecord Extract(ModeRecord mode)
    {
        ArgumentNullException.ThrowIfNull(mode);

        double lambda = mode.Eigenvalue;
        double scale = lambda >= 0
            ? System.Math.Sqrt(lambda)
            : -System.Math.Sqrt(System.Math.Abs(lambda));

        return new MassLikeScaleRecord
        {
            ModeId = mode.ModeId,
            Eigenvalue = lambda,
            MassLikeScale = scale,
            ExtractionMethod = "eigenvalue",
            OperatorType = mode.OperatorType.ToString(),
            BackgroundId = mode.BackgroundId,
        };
    }

    /// <summary>
    /// Extract mass-like scales from all modes in a spectrum.
    /// </summary>
    public static IReadOnlyList<MassLikeScaleRecord> ExtractAll(IEnumerable<ModeRecord> modes)
    {
        ArgumentNullException.ThrowIfNull(modes);
        return modes.Select(Extract).ToList();
    }
}
