using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Normalization conventions for eigenvectors.
/// </summary>
public enum NormalizationConvention
{
    /// <summary>Unit L2 norm: v^T v = 1.</summary>
    L2Unit,

    /// <summary>Unit M_state norm: v^T M_state v = 1.</summary>
    MStateUnit,

    /// <summary>Max block norm = 1: largest Lie-algebra block has unit norm.</summary>
    MaxBlockNorm,
}

/// <summary>
/// Normalizes mode vectors under a declared convention.
///
/// The normalization convention affects amplitudes, overlap metrics,
/// and coupling proxies, so it must be serialized with the mode record.
/// </summary>
public sealed class ModeNormalizer
{
    /// <summary>
    /// Normalize a mode vector under the L2 convention: v / ||v||_2.
    /// Returns a new array; does not modify the input.
    /// </summary>
    public static double[] NormalizeL2(double[] v)
    {
        if (v == null) throw new ArgumentNullException(nameof(v));

        double norm = L2Norm(v);
        if (norm < 1e-30) return (double[])v.Clone();

        var result = new double[v.Length];
        for (int i = 0; i < v.Length; i++)
            result[i] = v[i] / norm;
        return result;
    }

    /// <summary>
    /// Normalize a mode vector under M_state: v / sqrt(v^T M v).
    /// </summary>
    public static double[] NormalizeMState(double[] v, ILinearOperator massOperator)
    {
        if (v == null) throw new ArgumentNullException(nameof(v));
        if (massOperator == null) throw new ArgumentNullException(nameof(massOperator));

        var tensor = new FieldTensor
        {
            Label = "v",
            Signature = massOperator.InputSignature,
            Coefficients = v,
            Shape = new[] { v.Length },
        };
        double[] mv = massOperator.Apply(tensor).Coefficients;
        double norm = System.Math.Sqrt(Dot(v, mv));
        if (norm < 1e-30) return (double[])v.Clone();

        var result = new double[v.Length];
        for (int i = 0; i < v.Length; i++)
            result[i] = v[i] / norm;
        return result;
    }

    /// <summary>
    /// Normalize a mode vector under MaxBlockNorm convention.
    /// The vector is divided by the L2 norm of its largest Lie-algebra block.
    /// Each block has size dimG, with blocks indexed edge-major.
    /// </summary>
    /// <param name="v">Mode vector.</param>
    /// <param name="dimG">Lie algebra dimension (block size).</param>
    public static double[] NormalizeMaxBlockNorm(double[] v, int dimG)
    {
        if (v == null) throw new ArgumentNullException(nameof(v));
        if (dimG <= 0) throw new ArgumentException("dimG must be positive.", nameof(dimG));

        int numBlocks = v.Length / dimG;
        if (numBlocks == 0) return (double[])v.Clone();

        double maxBlockNorm = 0;
        for (int b = 0; b < numBlocks; b++)
        {
            double blockNorm = 0;
            int offset = b * dimG;
            for (int k = 0; k < dimG; k++)
                blockNorm += v[offset + k] * v[offset + k];
            blockNorm = System.Math.Sqrt(blockNorm);
            if (blockNorm > maxBlockNorm)
                maxBlockNorm = blockNorm;
        }

        if (maxBlockNorm < 1e-30) return (double[])v.Clone();

        var result = new double[v.Length];
        for (int i = 0; i < v.Length; i++)
            result[i] = v[i] / maxBlockNorm;
        return result;
    }

    /// <summary>
    /// Normalize a mode vector under the given convention.
    /// </summary>
    /// <param name="v">Mode vector.</param>
    /// <param name="convention">Normalization convention.</param>
    /// <param name="massOperator">M_state operator (required for MStateUnit).</param>
    /// <param name="dimG">Lie algebra dimension (required for MaxBlockNorm).</param>
    public static double[] Normalize(
        double[] v,
        NormalizationConvention convention,
        ILinearOperator? massOperator = null,
        int dimG = 0)
    {
        return convention switch
        {
            NormalizationConvention.L2Unit => NormalizeL2(v),
            NormalizationConvention.MStateUnit => NormalizeMState(v,
                massOperator ?? throw new ArgumentException("massOperator required for MStateUnit convention.")),
            NormalizationConvention.MaxBlockNorm => NormalizeMaxBlockNorm(v,
                dimG > 0 ? dimG : throw new ArgumentException("dimG required for MaxBlockNorm convention.")),
            _ => throw new ArgumentOutOfRangeException(nameof(convention)),
        };
    }

    /// <summary>
    /// Returns the serialization string for a normalization convention.
    /// </summary>
    public static string ConventionToString(NormalizationConvention convention) => convention switch
    {
        NormalizationConvention.L2Unit => "unit-L2-norm",
        NormalizationConvention.MStateUnit => "unit-M-norm",
        NormalizationConvention.MaxBlockNorm => "max-block-norm",
        _ => throw new ArgumentOutOfRangeException(nameof(convention)),
    };

    private static double L2Norm(double[] v)
    {
        double sum = 0;
        for (int i = 0; i < v.Length; i++)
            sum += v[i] * v[i];
        return System.Math.Sqrt(sum);
    }

    private static double Dot(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++)
            sum += a[i] * b[i];
        return sum;
    }
}
