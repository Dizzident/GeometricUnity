using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// ILinearOperator wrapping a diagonal matrix (stored as a flat array of diagonal entries).
/// Used to represent diagonal M_state matrices in the gauge projector.
/// </summary>
public sealed class DiagonalOperator : ILinearOperator
{
    private readonly double[] _diag;

    public DiagonalOperator(double[] diag)
    {
        _diag = diag ?? throw new ArgumentNullException(nameof(diag));
    }

    /// <summary>The diagonal entries.</summary>
    public IReadOnlyList<double> Diagonal => _diag;

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

    public int InputDimension => _diag.Length;
    public int OutputDimension => _diag.Length;

    public FieldTensor Apply(FieldTensor v)
    {
        var result = new double[_diag.Length];
        for (int i = 0; i < _diag.Length; i++)
            result[i] = _diag[i] * v.Coefficients[i];
        return new FieldTensor
        {
            Label = "M*v",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { _diag.Length },
        };
    }

    /// <summary>Diagonal is symmetric, so transpose = same.</summary>
    public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);
}
