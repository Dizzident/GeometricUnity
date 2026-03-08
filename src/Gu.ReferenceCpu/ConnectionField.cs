using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// Represents a discrete connection omega_h as an ad(P)-valued 1-form on Y_h.
/// Stored as Lie-algebra coefficients per edge of the ambient mesh.
/// Total DOFs = edgeCount * dim(g).
/// </summary>
public sealed class ConnectionField
{
    /// <summary>The ambient mesh Y_h on which omega_h lives.</summary>
    public SimplicialMesh Mesh { get; }

    /// <summary>The Lie algebra for the gauge group.</summary>
    public LieAlgebra Algebra { get; }

    /// <summary>
    /// Coefficient data: flat array of length EdgeCount * Algebra.Dimension.
    /// Indexed as [edgeIdx * dim(g) + algebraIdx].
    /// </summary>
    public double[] Coefficients { get; }

    /// <summary>Number of edges in the mesh.</summary>
    public int EdgeCount => Mesh.EdgeCount;

    /// <summary>Dimension of the Lie algebra.</summary>
    public int AlgebraDimension => Algebra.Dimension;

    public ConnectionField(SimplicialMesh mesh, LieAlgebra algebra)
    {
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        Algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        Coefficients = new double[mesh.EdgeCount * algebra.Dimension];
    }

    public ConnectionField(SimplicialMesh mesh, LieAlgebra algebra, double[] coefficients)
    {
        Mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        Algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        if (coefficients.Length != mesh.EdgeCount * algebra.Dimension)
            throw new ArgumentException(
                $"Expected {mesh.EdgeCount * algebra.Dimension} coefficients, got {coefficients.Length}.");
        Coefficients = coefficients;
    }

    /// <summary>
    /// Gets the Lie-algebra-valued coefficient vector for a given edge.
    /// Returns a span of length dim(g).
    /// </summary>
    public ReadOnlySpan<double> GetEdgeValue(int edgeIndex)
    {
        return new ReadOnlySpan<double>(Coefficients, edgeIndex * AlgebraDimension, AlgebraDimension);
    }

    /// <summary>
    /// Gets the Lie-algebra-valued coefficient vector for a given edge as a new array.
    /// </summary>
    public double[] GetEdgeValueArray(int edgeIndex)
    {
        var result = new double[AlgebraDimension];
        Array.Copy(Coefficients, edgeIndex * AlgebraDimension, result, 0, AlgebraDimension);
        return result;
    }

    /// <summary>
    /// Sets the Lie-algebra-valued coefficient vector for a given edge.
    /// </summary>
    public void SetEdgeValue(int edgeIndex, double[] value)
    {
        if (value.Length != AlgebraDimension)
            throw new ArgumentException($"Expected {AlgebraDimension} components.");
        Array.Copy(value, 0, Coefficients, edgeIndex * AlgebraDimension, AlgebraDimension);
    }

    /// <summary>
    /// Converts to a FieldTensor with proper TensorSignature.
    /// </summary>
    public FieldTensor ToFieldTensor()
    {
        return new FieldTensor
        {
            Label = "omega_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = Algebra.BasisOrderId,
                ComponentOrderId = "edge-major",
                NumericPrecision = "float64",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = (double[])Coefficients.Clone(),
            Shape = new[] { EdgeCount, AlgebraDimension },
        };
    }

    /// <summary>
    /// Creates a zero (flat) connection.
    /// </summary>
    public static ConnectionField Zero(SimplicialMesh mesh, LieAlgebra algebra)
    {
        return new ConnectionField(mesh, algebra);
    }
}
