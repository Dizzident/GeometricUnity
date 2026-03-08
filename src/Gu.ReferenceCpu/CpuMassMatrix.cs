using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// CPU mass matrix M_Upsilon for the residual inner product.
/// Decomposed into three ingredients:
/// - Lie algebra pairing M_Lie (from LieAlgebra.InvariantMetric)
/// - Form metric M_form (diagonal weights per face, from mesh geometry)
/// - Quadrature weights (uniform for P0 on simplicial mesh)
///
/// For su(2) with trace pairing: M_Lie = I_3, so M_Upsilon is three
/// decoupled copies of the form metric.
///
/// This is a reference implementation using flat (uniform) volume weights.
/// Production implementations should use proper Whitney 2-form mass matrices.
/// </summary>
public sealed class CpuMassMatrix
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly double[] _faceWeights;

    /// <summary>Lie algebra pairing convention.</summary>
    public string PairingId { get; }

    /// <summary>Form metric identifier.</summary>
    public string FormMetricId { get; }

    /// <summary>
    /// Create a mass matrix with uniform face weights.
    /// Each face gets weight = 1.0 (suitable for unit-volume reference elements).
    /// </summary>
    public CpuMassMatrix(SimplicialMesh mesh, LieAlgebra algebra)
        : this(mesh, algebra, CreateUniformWeights(mesh.FaceCount))
    {
    }

    /// <summary>
    /// Create a mass matrix with custom face weights.
    /// </summary>
    public CpuMassMatrix(SimplicialMesh mesh, LieAlgebra algebra, double[] faceWeights)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));

        if (faceWeights.Length != mesh.FaceCount)
            throw new ArgumentException(
                $"Expected {mesh.FaceCount} face weights, got {faceWeights.Length}.");

        _faceWeights = faceWeights;
        PairingId = algebra.PairingId;
        FormMetricId = "diagonal-uniform";
    }

    /// <summary>
    /// Apply M * v: computes w_f * g_{ab} * v^a_f for each face f and algebra index a.
    /// For trace pairing (g=I): just scales each face by its weight.
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        int faceCount = _mesh.FaceCount;
        int dimG = _algebra.Dimension;
        var result = new double[faceCount * dimG];

        for (int fi = 0; fi < faceCount; fi++)
        {
            double w = _faceWeights[fi];
            for (int a = 0; a < dimG; a++)
            {
                // Apply Lie algebra metric: sum_b g_{ab} * v^b_f
                double sum = 0;
                for (int b = 0; b < dimG; b++)
                {
                    sum += _algebra.InvariantMetric[a * dimG + b] * v.Coefficients[fi * dimG + b];
                }
                result[fi * dimG + a] = w * sum;
            }
        }

        return new FieldTensor
        {
            Label = $"M*{v.Label}",
            Signature = v.Signature,
            Coefficients = result,
            Shape = v.Shape,
        };
    }

    /// <summary>
    /// Inner product: u^T M v = sum_f w_f * sum_{a,b} g_{ab} * u^a_f * v^b_f.
    /// </summary>
    public double InnerProduct(FieldTensor u, FieldTensor v)
    {
        int faceCount = _mesh.FaceCount;
        int dimG = _algebra.Dimension;
        double total = 0;

        for (int fi = 0; fi < faceCount; fi++)
        {
            double w = _faceWeights[fi];
            for (int a = 0; a < dimG; a++)
            {
                for (int b = 0; b < dimG; b++)
                {
                    total += w * _algebra.InvariantMetric[a * dimG + b]
                        * u.Coefficients[fi * dimG + a]
                        * v.Coefficients[fi * dimG + b];
                }
            }
        }

        return total;
    }

    /// <summary>
    /// Norm: sqrt(v^T M v).
    /// </summary>
    public double Norm(FieldTensor v)
    {
        return System.Math.Sqrt(InnerProduct(v, v));
    }

    /// <summary>
    /// Objective I2_h = (1/2) Upsilon^T M Upsilon.
    /// </summary>
    public double EvaluateObjective(FieldTensor upsilon)
    {
        return 0.5 * InnerProduct(upsilon, upsilon);
    }

    private static double[] CreateUniformWeights(int faceCount)
    {
        var weights = new double[faceCount];
        Array.Fill(weights, 1.0);
        return weights;
    }
}
