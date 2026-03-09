using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Solvers;

namespace Gu.Phase2.Stability;

/// <summary>
/// Coulomb slice operator C_* as an ILinearOperator.
///
/// C_*(delta_u) = d_{A0}^*(delta_u)  (codifferential: edges -> vertices)
/// C_*^T(phi)   = d_{A0}(phi)        (covariant exterior derivative: vertices -> edges)
///
/// The composition C_*^T C_* = d_{A0} d_{A0}^* is the gauge Laplacian on 1-forms.
///
/// Currently uses the flat d^* (no A0 bracket terms), which is exact when A_* = A0
/// on the simplest branch. For continuation paths where A_* != A0, the covariant
/// version d_{A0}^*(beta) = d^*(beta) - contraction([A0, beta]) should be used.
///
/// Key identity: C_* R_{z_*} = d_{A0}^*(-d_{A0}) = -Delta_gauge (on 0-forms).
/// This should be invertible on the complement of constant gauge transformations.
/// </summary>
public sealed class CoulombSliceOperator : IGaugeConstraintOperator
{
    private readonly CoulombGaugePenalty _gauge;
    private readonly int _edgeDim;
    private readonly int _vertexDim;
    private readonly TensorSignature _inputSig;
    private readonly TensorSignature _outputSig;

    /// <inheritdoc />
    public string GaugeHandlingMode => "coulomb";

    /// <summary>
    /// Create a Coulomb slice operator.
    /// </summary>
    /// <param name="gauge">Coulomb gauge penalty (provides d^* and d operators).</param>
    /// <param name="mesh">Simplicial mesh.</param>
    /// <param name="dimG">Lie algebra dimension.</param>
    /// <param name="basisId">Lie algebra basis ID for tensor signatures.</param>
    public CoulombSliceOperator(
        CoulombGaugePenalty gauge,
        SimplicialMesh mesh,
        int dimG,
        string basisId = "standard")
    {
        _gauge = gauge ?? throw new ArgumentNullException(nameof(gauge));
        _edgeDim = mesh.EdgeCount * dimG;
        _vertexDim = mesh.VertexCount * dimG;

        _inputSig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = basisId,
            ComponentOrderId = "edge-major",
            NumericPrecision = "float64",
            MemoryLayout = "dense-row-major",
        };

        _outputSig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "gauge-violation-0form",
            Degree = "0",
            LieAlgebraBasisId = basisId,
            ComponentOrderId = "vertex-major",
            NumericPrecision = "float64",
            MemoryLayout = "dense-row-major",
        };
    }

    public TensorSignature InputSignature => _inputSig;
    public TensorSignature OutputSignature => _outputSig;
    public int InputDimension => _edgeDim;
    public int OutputDimension => _vertexDim;

    /// <summary>
    /// Forward action: C_*(delta_u) = d^*(delta_u) (codifferential, edges -> vertices).
    /// TODO: generalize to covariant d_{A0}^* for non-zero A0
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        var result = _gauge.ApplyCodifferential(v.Coefficients);
        return new FieldTensor
        {
            Label = "C*v",
            Signature = _outputSig,
            Coefficients = result,
            Shape = new[] { _vertexDim },
        };
    }

    /// <summary>
    /// Transpose action: C_*^T(phi) = d(phi) (exterior derivative, vertices -> edges).
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor w)
    {
        var result = _gauge.ApplyExteriorDerivative(w.Coefficients);
        return new FieldTensor
        {
            Label = "C^T*w",
            Signature = _inputSig,
            Coefficients = result,
            Shape = new[] { _edgeDim },
        };
    }
}
