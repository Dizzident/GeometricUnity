using Gu.Branching;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Diagnoses near-zero eigenvalue modes in a spectrum.
///
/// Near-zero modes may be:
/// - gauge artifacts (high gauge leak score),
/// - exact symmetries (Goldstone bosons, flat directions),
/// - discretization artifacts,
/// - unresolved (insufficient information to classify).
///
/// Classification uses the gauge leak score and eigenvalue magnitude
/// relative to the null threshold.
/// </summary>
public sealed class NullModeDiagnoser
{
    /// <summary>
    /// Eigenvalue threshold: modes with |lambda| below this are considered null.
    /// </summary>
    public double NullThreshold { get; }

    /// <summary>
    /// Gauge leak threshold: null modes with leak score above this are gauge artifacts.
    /// </summary>
    public double GaugeLeakThreshold { get; }

    /// <summary>
    /// Create a null mode diagnoser.
    /// </summary>
    /// <param name="nullThreshold">Eigenvalue threshold for null classification (default 1e-8).</param>
    /// <param name="gaugeLeakThreshold">Gauge leak threshold for gauge artifact classification (default 0.9).</param>
    public NullModeDiagnoser(double nullThreshold = 1e-8, double gaugeLeakThreshold = 0.9)
    {
        if (nullThreshold < 0) throw new ArgumentException("Null threshold must be non-negative.", nameof(nullThreshold));
        if (gaugeLeakThreshold < 0 || gaugeLeakThreshold > 1)
            throw new ArgumentException("Gauge leak threshold must be in [0, 1].", nameof(gaugeLeakThreshold));
        NullThreshold = nullThreshold;
        GaugeLeakThreshold = gaugeLeakThreshold;
    }

    /// <summary>
    /// Diagnose null modes in a collection of mode records.
    /// Returns a NullModeDiagnosis summary, or null if no null modes are found.
    /// </summary>
    public NullModeDiagnosis? Diagnose(IReadOnlyList<ModeRecord> modes)
    {
        if (modes == null) throw new ArgumentNullException(nameof(modes));

        var nullModes = new List<(double eigenvalue, double gaugeLeakScore)>();
        for (int i = 0; i < modes.Count; i++)
        {
            if (System.Math.Abs(modes[i].Eigenvalue) < NullThreshold)
            {
                nullModes.Add((modes[i].Eigenvalue, modes[i].GaugeLeakScore));
            }
        }

        if (nullModes.Count == 0) return null;

        // Sort by |lambda|
        nullModes.Sort((a, b) => System.Math.Abs(a.eigenvalue).CompareTo(System.Math.Abs(b.eigenvalue)));

        int gaugeCount = 0;
        for (int i = 0; i < nullModes.Count; i++)
        {
            if (nullModes[i].gaugeLeakScore > GaugeLeakThreshold)
                gaugeCount++;
        }

        return new NullModeDiagnosis
        {
            NullThreshold = NullThreshold,
            NullModeCount = nullModes.Count,
            GaugeNullCount = gaugeCount,
            NullEigenvalues = nullModes.Select(m => m.eigenvalue).ToArray(),
            NullGaugeLeakScores = nullModes.Select(m => m.gaugeLeakScore).ToArray(),
            GaugeLeakThreshold = GaugeLeakThreshold,
        };
    }

    /// <summary>
    /// Classify a single mode's null status.
    /// </summary>
    public NullModeClassification Classify(ModeRecord mode)
    {
        if (mode == null) throw new ArgumentNullException(nameof(mode));
        if (System.Math.Abs(mode.Eigenvalue) >= NullThreshold)
            return NullModeClassification.NotNull;
        if (mode.GaugeLeakScore > GaugeLeakThreshold)
            return NullModeClassification.GaugeArtifact;
        if (mode.ResidualNorm > NullThreshold * 10)
            return NullModeClassification.DiscretizationArtifact;
        if (mode.GaugeLeakScore < 0.1)
            return NullModeClassification.ExactSymmetry;
        return NullModeClassification.Unresolved;
    }
}

/// <summary>
/// Classification of a null mode.
/// </summary>
public enum NullModeClassification
{
    /// <summary>Mode eigenvalue is above the null threshold; not a null mode.</summary>
    NotNull,

    /// <summary>Gauge artifact: high gauge leak score, should be discarded.</summary>
    GaugeArtifact,

    /// <summary>Exact symmetry: physical zero mode (Goldstone boson, flat direction).</summary>
    ExactSymmetry,

    /// <summary>Discretization artifact: high residual suggests numerical issue.</summary>
    DiscretizationArtifact,

    /// <summary>Unresolved: insufficient information to classify.</summary>
    Unresolved,
}
