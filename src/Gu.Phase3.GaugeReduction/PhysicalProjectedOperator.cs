using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Projects an operator H into the physical subspace: H_phys = P_phys^T H P_phys.
///
/// For symmetric H and a self-adjoint P_phys, this simplifies to P_phys H P_phys.
/// Gauge directions lie in the kernel: H_phys(v_gauge) = 0.
///
/// This wraps an existing ILinearOperator (e.g., HessianOperator from Phase 2)
/// without reimplementing it.
/// </summary>
public sealed class PhysicalProjectedOperator : ILinearOperator
{
    private readonly ILinearOperator _innerOperator;
    private readonly GaugeProjector _projector;

    /// <summary>
    /// Create a physical-projected operator: H_phys = P_phys^T H P_phys.
    /// </summary>
    /// <param name="innerOperator">The operator H to project (e.g., HessianOperator).</param>
    /// <param name="projector">The gauge projector providing P_phys.</param>
    public PhysicalProjectedOperator(ILinearOperator innerOperator, GaugeProjector projector)
    {
        _innerOperator = innerOperator ?? throw new ArgumentNullException(nameof(innerOperator));
        _projector = projector ?? throw new ArgumentNullException(nameof(projector));
    }

    /// <summary>The wrapped inner operator.</summary>
    public ILinearOperator InnerOperator => _innerOperator;

    /// <summary>The gauge projector.</summary>
    public GaugeProjector Projector => _projector;

    public TensorSignature InputSignature => _innerOperator.InputSignature;
    public TensorSignature OutputSignature => _innerOperator.OutputSignature;
    public int InputDimension => _innerOperator.InputDimension;
    public int OutputDimension => _innerOperator.OutputDimension;

    /// <summary>
    /// Apply H_phys(v) = P_phys(H(P_phys(v))).
    /// For symmetric H and self-adjoint P_phys, this equals P_phys^T H P_phys.
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        // Step 1: P_phys(v)
        var pv = _projector.ApplyPhysical(v.Coefficients);
        var pvTensor = new FieldTensor
        {
            Label = "P_phys*v",
            Signature = _innerOperator.InputSignature,
            Coefficients = pv,
            Shape = new[] { pv.Length },
        };

        // Step 2: H(P_phys(v))
        var hpv = _innerOperator.Apply(pvTensor);

        // Step 3: P_phys(H(P_phys(v)))
        var result = _projector.ApplyPhysical(hpv.Coefficients);
        return new FieldTensor
        {
            Label = "P_phys*H*P_phys*v",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { result.Length },
        };
    }

    /// <summary>
    /// Apply transpose: (P_phys^T H P_phys)^T = P_phys^T H^T P_phys.
    /// For symmetric H, this equals Apply.
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor v)
    {
        var pv = _projector.ApplyPhysical(v.Coefficients);
        var pvTensor = new FieldTensor
        {
            Label = "P_phys*v",
            Signature = _innerOperator.OutputSignature,
            Coefficients = pv,
            Shape = new[] { pv.Length },
        };

        var htpv = _innerOperator.ApplyTranspose(pvTensor);
        var result = _projector.ApplyPhysical(htpv.Coefficients);
        return new FieldTensor
        {
            Label = "P_phys*HT*P_phys*v",
            Signature = InputSignature,
            Coefficients = result,
            Shape = new[] { result.Length },
        };
    }
}
