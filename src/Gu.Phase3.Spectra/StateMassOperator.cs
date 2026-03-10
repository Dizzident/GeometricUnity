using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.Phase3.Spectra;

/// <summary>
/// State-space mass matrix M_state as an ILinearOperator.
/// Provides the inner product on state perturbations (edge-valued connections on Y_h).
///
/// PHYSICS CONSTRAINT #3: Each block in BranchFieldLayout has its own inner product.
/// Connection blocks use edge 1-form inner product on Y_h with Lie algebra pairing.
///
/// For the single-connection-block case (Phase I compatible):
///   M_state(v)[e*dimG + a] = w_e * sum_b g_{ab} * v[e*dimG + b]
///
/// where w_e is the edge weight and g_{ab} is the Lie algebra metric.
/// </summary>
public sealed class StateMassOperator : ILinearOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly double[] _edgeWeights;

    public StateMassOperator(SimplicialMesh mesh, LieAlgebra algebra)
        : this(mesh, algebra, CreateUniformWeights(mesh.EdgeCount))
    {
    }

    public StateMassOperator(SimplicialMesh mesh, LieAlgebra algebra, double[] edgeWeights)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));

        if (edgeWeights.Length != mesh.EdgeCount)
            throw new ArgumentException(
                $"Expected {mesh.EdgeCount} edge weights, got {edgeWeights.Length}.");

        _edgeWeights = edgeWeights;
    }

    public TensorSignature InputSignature => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "standard",
        ComponentOrderId = "edge-major",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    public TensorSignature OutputSignature => InputSignature;

    public int InputDimension => _mesh.EdgeCount * _algebra.Dimension;
    public int OutputDimension => _mesh.EdgeCount * _algebra.Dimension;

    /// <summary>
    /// Apply M_state * v.
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        int edgeCount = _mesh.EdgeCount;
        int dimG = _algebra.Dimension;
        var result = new double[edgeCount * dimG];

        for (int e = 0; e < edgeCount; e++)
        {
            double w = _edgeWeights[e];
            for (int a = 0; a < dimG; a++)
            {
                double sum = 0;
                for (int b = 0; b < dimG; b++)
                {
                    sum += _algebra.InvariantMetric[a * dimG + b] * v.Coefficients[e * dimG + b];
                }
                result[e * dimG + a] = w * sum;
            }
        }

        return new FieldTensor
        {
            Label = "M_state*v",
            Signature = InputSignature,
            Coefficients = result,
            Shape = new[] { edgeCount, dimG },
        };
    }

    /// <summary>
    /// M_state is symmetric, so Apply = ApplyTranspose.
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);

    /// <summary>
    /// Compute inner product: u^T M_state v.
    /// </summary>
    public double InnerProduct(FieldTensor u, FieldTensor v)
    {
        var mv = Apply(v);
        double sum = 0;
        for (int i = 0; i < u.Coefficients.Length; i++)
            sum += u.Coefficients[i] * mv.Coefficients[i];
        return sum;
    }

    /// <summary>
    /// Compute norm: sqrt(v^T M_state v).
    /// </summary>
    public double Norm(FieldTensor v)
    {
        return System.Math.Sqrt(System.Math.Max(0, InnerProduct(v, v)));
    }

    private static double[] CreateUniformWeights(int edgeCount)
    {
        var weights = new double[edgeCount];
        Array.Fill(weights, 1.0);
        return weights;
    }
}
