using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Solvers;

namespace Gu.Phase2.Stability;

/// <summary>
/// Gauge-fixed linearized operator L_tilde = (J, sqrt(lambda)*C).
///
/// L_tilde maps connection perturbations delta_omega to the stacked vector:
///   L_tilde(delta) = ( J * delta, sqrt(lambda) * C * delta )
///
/// where J is the physics Jacobian (dUpsilon/domega) and C is the gauge/slice
/// operator (Coulomb codifferential d^*).
///
/// The sqrt(lambda) scaling ensures H = L_tilde^T L_tilde = J^T J + lambda * C^T C,
/// matching the Gauss-Newton normal equations used in the Phase I solver.
///
/// Input space: connection DOFs (EdgeCount * dimG)
/// Output space: stacked (residual DOFs, gauge DOFs) = (FaceCount*dimG, VertexCount*dimG)
/// </summary>
public sealed class GaugeFixedLinearOperator : ILinearOperator
{
    private readonly ILinearOperator _jacobian;
    private readonly CoulombGaugePenalty _gauge;
    private readonly double _sqrtLambda;
    private readonly int _gaugeDim;

    /// <summary>
    /// Create a gauge-fixed linearized operator.
    /// </summary>
    /// <param name="jacobian">Physics Jacobian J = dUpsilon/domega.</param>
    /// <param name="gauge">Coulomb gauge penalty (provides d^* and d operators).</param>
    /// <param name="mesh">Simplicial mesh.</param>
    /// <param name="dimG">Lie algebra dimension.</param>
    public GaugeFixedLinearOperator(
        ILinearOperator jacobian,
        CoulombGaugePenalty gauge,
        SimplicialMesh mesh,
        int dimG)
    {
        _jacobian = jacobian ?? throw new ArgumentNullException(nameof(jacobian));
        _gauge = gauge ?? throw new ArgumentNullException(nameof(gauge));

        if (gauge.Lambda < 0)
            throw new ArgumentException("Gauge lambda must be non-negative.", nameof(gauge));

        _sqrtLambda = System.Math.Sqrt(gauge.Lambda);
        _gaugeDim = mesh.VertexCount * dimG;
    }

    public TensorSignature InputSignature => _jacobian.InputSignature;

    public TensorSignature OutputSignature => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "gauge-fixed-linearization",
        Degree = "mixed",
        LieAlgebraBasisId = _jacobian.OutputSignature.LieAlgebraBasisId,
        ComponentOrderId = "stacked-J-C",
        NumericPrecision = "float64",
        MemoryLayout = "dense-row-major",
    };

    public int InputDimension => _jacobian.InputDimension;

    public int OutputDimension => _jacobian.OutputDimension + _gaugeDim;

    /// <summary>
    /// Forward action: L_tilde * v = ( J*v, sqrt(lambda)*C*v ).
    /// C*v = d^*(v) (codifferential applied to the perturbation).
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        var jv = _jacobian.Apply(v);
        var cv = _gauge.ApplyCodifferential(v.Coefficients);

        var result = new double[OutputDimension];

        // Copy J*v into first block
        Array.Copy(jv.Coefficients, 0, result, 0, jv.Coefficients.Length);

        // Copy sqrt(lambda)*C*v into second block
        for (int i = 0; i < cv.Length; i++)
            result[jv.Coefficients.Length + i] = _sqrtLambda * cv[i];

        return new FieldTensor
        {
            Label = "L_tilde*v",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { OutputDimension },
        };
    }

    /// <summary>
    /// Transpose action: L_tilde^T * w = J^T * w_J + sqrt(lambda) * C^T * w_C.
    /// where w = (w_J, w_C) is the stacked output vector.
    /// C^T = d (exterior derivative, transpose of codifferential d^*).
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor w)
    {
        int jOutDim = _jacobian.OutputDimension;

        // Split w into (w_J, w_C)
        var wJ = new FieldTensor
        {
            Label = "w_J",
            Signature = _jacobian.OutputSignature,
            Coefficients = w.Coefficients[..jOutDim],
            Shape = new[] { jOutDim },
        };

        var wCCoeffs = w.Coefficients[jOutDim..];

        // J^T * w_J
        var jtWj = _jacobian.ApplyTranspose(wJ);

        // C^T * w_C = d(w_C) (exterior derivative is transpose of codifferential)
        var dWc = _gauge.ApplyExteriorDerivative(wCCoeffs);

        // Combine: J^T * w_J + sqrt(lambda) * C^T * w_C
        var result = new double[InputDimension];
        for (int i = 0; i < result.Length; i++)
            result[i] = jtWj.Coefficients[i] + _sqrtLambda * dWc[i];

        return new FieldTensor
        {
            Label = "L_tilde^T*w",
            Signature = _jacobian.InputSignature,
            Coefficients = result,
            Shape = new[] { InputDimension },
        };
    }
}
