using Gu.Branching;
using Gu.Core;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase2.Stability;

/// <summary>
/// Mass-weighted Hessian-style operator:
///
///   H(v) = J^T M_R J v + lambda * C^T M_0 C v
///
/// where:
/// - J is the physics Jacobian (dUpsilon/domega)
/// - M_R is the residual mass matrix (face-valued, Lie-algebra-weighted)
/// - C = d^* is the Coulomb gauge operator (codifferential)
/// - M_0 is the gauge mass matrix (vertex-valued, diagonal)
/// - lambda is the gauge penalty weight
///
/// H is self-adjoint and positive semi-definite (when M_R, M_0 are PSD).
/// It governs:
/// - local stability (strictly positive on slice -> coercive)
/// - soft modes (small positive eigenvalues)
/// - near-kernel (moduli candidates or unresolved gauge degeneracy)
/// - saddle behavior (negative modes indicate non-minimizer)
/// - branch fragility (wildly branch-dependent low spectrum)
///
/// This implementation is matrix-free: H*v is computed without forming any
/// explicit matrix.
///
/// PHYSICS NOTE: The naive L_tilde^T L_tilde without mass matrices gives
/// wrong stability semantics. The mass matrices M_R and M_0 encode the
/// inner product structure of the residual and gauge spaces respectively.
/// </summary>
public sealed class HessianOperator : ILinearOperator
{
    private readonly ILinearOperator _jacobian;
    private readonly CoulombGaugePenalty _gauge;
    private readonly CpuMassMatrix _residualMass;
    private readonly double[] _gaugeMassWeights;
    private readonly double _lambda;
    private readonly int _dimG;

    /// <summary>
    /// Create a mass-weighted Hessian operator.
    /// </summary>
    /// <param name="jacobian">Physics Jacobian J.</param>
    /// <param name="gauge">Coulomb gauge operator (provides d^*, d).</param>
    /// <param name="residualMass">Mass matrix M_R for residual (face) space.</param>
    /// <param name="gaugeMassWeights">Diagonal mass weights for gauge (vertex) space.
    /// Length must equal VertexCount. Each vertex DOF is weighted by this value.
    /// Pass null for uniform weights (all 1.0).</param>
    /// <param name="lambda">Gauge penalty weight.</param>
    /// <param name="dimG">Lie algebra dimension.</param>
    /// <param name="vertexCount">Number of vertices in mesh.</param>
    public HessianOperator(
        ILinearOperator jacobian,
        CoulombGaugePenalty gauge,
        CpuMassMatrix residualMass,
        double[]? gaugeMassWeights,
        double lambda,
        int dimG,
        int vertexCount)
    {
        _jacobian = jacobian ?? throw new ArgumentNullException(nameof(jacobian));
        _gauge = gauge ?? throw new ArgumentNullException(nameof(gauge));
        _residualMass = residualMass ?? throw new ArgumentNullException(nameof(residualMass));
        _lambda = lambda;
        _dimG = dimG;

        if (gaugeMassWeights != null)
        {
            if (gaugeMassWeights.Length != vertexCount)
                throw new ArgumentException(
                    $"Expected {vertexCount} gauge mass weights, got {gaugeMassWeights.Length}.");
            _gaugeMassWeights = gaugeMassWeights;
        }
        else
        {
            _gaugeMassWeights = new double[vertexCount];
            Array.Fill(_gaugeMassWeights, 1.0);
        }
    }

    public TensorSignature InputSignature => _jacobian.InputSignature;

    public TensorSignature OutputSignature => _jacobian.InputSignature;

    public int InputDimension => _jacobian.InputDimension;

    public int OutputDimension => _jacobian.InputDimension;

    /// <summary>
    /// Forward action: H * v = J^T M_R J v + lambda * C^T M_0 C v.
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        // Term 1: J^T M_R J v
        var jv = _jacobian.Apply(v);
        var mJv = _residualMass.Apply(jv);
        var jtMjv = _jacobian.ApplyTranspose(mJv);

        // Term 2: lambda * C^T M_0 C v
        // C = d^* (codifferential), C^T = d (exterior derivative)
        var cv = _gauge.ApplyCodifferential(v.Coefficients); // vertex-valued: VertexCount * dimG

        // Apply M_0 to Cv: diagonal mass weighting per vertex
        var m0Cv = new double[cv.Length];
        int vertexCount = _gaugeMassWeights.Length;
        for (int vi = 0; vi < vertexCount; vi++)
        {
            double w = _gaugeMassWeights[vi];
            for (int a = 0; a < _dimG; a++)
                m0Cv[vi * _dimG + a] = w * cv[vi * _dimG + a];
        }

        // C^T M_0 C v = d(M_0 * d^*(v))
        var ctM0Cv = _gauge.ApplyExteriorDerivative(m0Cv);

        // Combine: J^T M_R J v + lambda * C^T M_0 C v
        var result = new double[InputDimension];
        for (int i = 0; i < result.Length; i++)
            result[i] = jtMjv.Coefficients[i] + _lambda * ctM0Cv[i];

        return new FieldTensor
        {
            Label = "H*v",
            Signature = _jacobian.InputSignature,
            Coefficients = result,
            Shape = new[] { InputDimension },
        };
    }

    /// <summary>
    /// Transpose action: H^T * v = H * v (H is self-adjoint).
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor v)
    {
        return Apply(v);
    }
}
