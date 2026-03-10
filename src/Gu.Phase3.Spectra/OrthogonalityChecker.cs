using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Checks orthogonality of eigenvectors under a given inner product.
///
/// Computes the Gram matrix G_ij = v_i^T M v_j and reports:
/// - max off-diagonal |G_ij| for i != j (orthogonality defect),
/// - max |G_ii - 1| (normalization defect).
/// </summary>
public sealed class OrthogonalityChecker
{
    /// <summary>
    /// Compute the maximum orthogonality defect: max |v_i^T M v_j| for i != j.
    ///
    /// When massOperator is null, uses Euclidean inner product (M = I).
    /// </summary>
    /// <param name="modes">Mode records to check.</param>
    /// <param name="massOperator">M_state operator, or null for Euclidean.</param>
    /// <returns>Maximum off-diagonal absolute value of the Gram matrix.</returns>
    public static double MaxOrthogonalityDefect(
        IReadOnlyList<ModeRecord> modes,
        ILinearOperator? massOperator = null)
    {
        if (modes == null) throw new ArgumentNullException(nameof(modes));
        if (modes.Count < 2) return 0.0;

        double maxDefect = 0.0;

        for (int i = 0; i < modes.Count; i++)
        {
            double[] mv_i = ApplyMass(modes[i].ModeVector, massOperator);
            for (int j = i + 1; j < modes.Count; j++)
            {
                double dot = Dot(mv_i, modes[j].ModeVector);
                double defect = System.Math.Abs(dot);
                if (defect > maxDefect) maxDefect = defect;
            }
        }

        return maxDefect;
    }

    /// <summary>
    /// Compute the maximum normalization defect: max |v_i^T M v_i - 1| over all modes.
    /// </summary>
    public static double MaxNormalizationDefect(
        IReadOnlyList<ModeRecord> modes,
        ILinearOperator? massOperator = null)
    {
        if (modes == null) throw new ArgumentNullException(nameof(modes));
        if (modes.Count == 0) return 0.0;

        double maxDefect = 0.0;

        for (int i = 0; i < modes.Count; i++)
        {
            double[] mv = ApplyMass(modes[i].ModeVector, massOperator);
            double norm = Dot(mv, modes[i].ModeVector);
            double defect = System.Math.Abs(norm - 1.0);
            if (defect > maxDefect) maxDefect = defect;
        }

        return maxDefect;
    }

    /// <summary>
    /// Compute the full Gram matrix G_ij = v_i^T M v_j.
    /// </summary>
    public static double[,] GramMatrix(
        IReadOnlyList<ModeRecord> modes,
        ILinearOperator? massOperator = null)
    {
        if (modes == null) throw new ArgumentNullException(nameof(modes));

        int n = modes.Count;
        var gram = new double[n, n];

        for (int i = 0; i < n; i++)
        {
            double[] mv_i = ApplyMass(modes[i].ModeVector, massOperator);
            for (int j = i; j < n; j++)
            {
                double dot = Dot(mv_i, modes[j].ModeVector);
                gram[i, j] = dot;
                gram[j, i] = dot;
            }
        }

        return gram;
    }

    private static double[] ApplyMass(double[] v, ILinearOperator? massOperator)
    {
        if (massOperator == null) return v;

        var tensor = new FieldTensor
        {
            Label = "v",
            Signature = massOperator.InputSignature,
            Coefficients = v,
            Shape = new[] { v.Length },
        };
        return massOperator.Apply(tensor).Coefficients;
    }

    private static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}
