using System.Text.Json.Serialization;

namespace Gu.Phase3.Observables;

/// <summary>
/// Overlap metrics between observed mode signatures.
///
/// Used for mode matching in observed space (as opposed to native space).
/// </summary>
public sealed class ObservedOverlapMetrics
{
    /// <summary>
    /// Compute the L2 overlap between two observed signatures:
    ///   overlap = |s_i^T s_j| / (||s_i|| ||s_j||)
    ///
    /// Returns a value in [0, 1]. 1 = identical directions, 0 = orthogonal.
    /// </summary>
    public static double L2Overlap(ObservedModeSignature a, ObservedModeSignature b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));

        int len = System.Math.Min(a.ObservedCoefficients.Length, b.ObservedCoefficients.Length);
        double dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < len; i++)
        {
            dot += a.ObservedCoefficients[i] * b.ObservedCoefficients[i];
            normA += a.ObservedCoefficients[i] * a.ObservedCoefficients[i];
            normB += b.ObservedCoefficients[i] * b.ObservedCoefficients[i];
        }

        double denom = System.Math.Sqrt(normA) * System.Math.Sqrt(normB);
        if (denom < 1e-30) return 0.0;
        return System.Math.Abs(dot) / denom;
    }

    /// <summary>
    /// Compute the L2 distance between two observed signatures:
    ///   distance = ||s_i - s_j|| / max(||s_i||, ||s_j||)
    ///
    /// Returns 0 for identical signatures.
    /// </summary>
    public static double RelativeL2Distance(ObservedModeSignature a, ObservedModeSignature b)
    {
        if (a == null) throw new ArgumentNullException(nameof(a));
        if (b == null) throw new ArgumentNullException(nameof(b));

        int len = System.Math.Min(a.ObservedCoefficients.Length, b.ObservedCoefficients.Length);
        double normA = 0, normB = 0, normDiff = 0;
        for (int i = 0; i < len; i++)
        {
            double diff = a.ObservedCoefficients[i] - b.ObservedCoefficients[i];
            normDiff += diff * diff;
            normA += a.ObservedCoefficients[i] * a.ObservedCoefficients[i];
            normB += b.ObservedCoefficients[i] * b.ObservedCoefficients[i];
        }

        double maxNorm = System.Math.Max(System.Math.Sqrt(normA), System.Math.Sqrt(normB));
        if (maxNorm < 1e-30) return 0.0;
        return System.Math.Sqrt(normDiff) / maxNorm;
    }

    /// <summary>
    /// Compute an overlap matrix for a set of observed signatures.
    /// </summary>
    public static double[,] OverlapMatrix(IReadOnlyList<ObservedModeSignature> signatures)
    {
        if (signatures == null) throw new ArgumentNullException(nameof(signatures));

        int n = signatures.Count;
        var matrix = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            matrix[i, i] = 1.0;
            for (int j = i + 1; j < n; j++)
            {
                double overlap = L2Overlap(signatures[i], signatures[j]);
                matrix[i, j] = overlap;
                matrix[j, i] = overlap;
            }
        }
        return matrix;
    }
}
