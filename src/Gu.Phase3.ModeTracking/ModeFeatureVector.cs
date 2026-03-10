namespace Gu.Phase3.ModeTracking;

/// <summary>
/// Feature vector for a mode, used for invariant-feature-based matching (O3).
/// Contains properties that are comparable across different discretizations.
/// </summary>
public sealed class ModeFeatureVector
{
    /// <summary>Mode ID.</summary>
    public required string ModeId { get; init; }

    /// <summary>Eigenvalue.</summary>
    public required double Eigenvalue { get; init; }

    /// <summary>Gauge leak score.</summary>
    public required double GaugeLeakScore { get; init; }

    /// <summary>Multiplicity of the cluster this mode belongs to.</summary>
    public int Multiplicity { get; init; } = 1;

    /// <summary>
    /// Compute the feature distance between two mode feature vectors.
    /// Uses normalized Euclidean distance in feature space.
    /// </summary>
    public static double Distance(ModeFeatureVector a, ModeFeatureVector b)
    {
        // Eigenvalue distance (relative)
        double eigScale = System.Math.Max(
            System.Math.Abs(a.Eigenvalue), System.Math.Abs(b.Eigenvalue));
        eigScale = System.Math.Max(eigScale, 1e-10);
        double eigDist = System.Math.Abs(a.Eigenvalue - b.Eigenvalue) / eigScale;

        // Gauge leak distance
        double leakDist = System.Math.Abs(a.GaugeLeakScore - b.GaugeLeakScore);

        // Multiplicity distance
        double multDist = System.Math.Abs(a.Multiplicity - b.Multiplicity);

        // Weighted combination
        return 0.5 * eigDist + 0.3 * leakDist + 0.2 * multDist;
    }
}
