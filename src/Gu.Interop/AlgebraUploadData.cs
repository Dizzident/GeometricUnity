using global::Gu.Math;

namespace Gu.Interop;

/// <summary>
/// Packed Lie algebra data for upload to the native backend.
/// Contains structure constants and invariant metric.
/// </summary>
public sealed class AlgebraUploadData
{
    /// <summary>Lie algebra dimension (number of generators).</summary>
    public required int Dimension { get; init; }

    /// <summary>
    /// Structure constants f^c_{ab} as flat array [dim^3].
    /// Indexed as [a * dim * dim + b * dim + c].
    /// </summary>
    public required double[] StructureConstants { get; init; }

    /// <summary>
    /// Invariant metric g_{ab} as flat array [dim^2], row-major.
    /// </summary>
    public required double[] InvariantMetric { get; init; }

    /// <summary>
    /// Create from a LieAlgebra instance.
    /// </summary>
    public static AlgebraUploadData FromLieAlgebra(LieAlgebra algebra)
    {
        ArgumentNullException.ThrowIfNull(algebra);

        return new AlgebraUploadData
        {
            Dimension = algebra.Dimension,
            StructureConstants = (double[])algebra.StructureConstants.Clone(),
            InvariantMetric = (double[])algebra.InvariantMetric.Clone(),
        };
    }
}
