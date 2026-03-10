using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Implements the physical projector as an ILinearOperator.
///
/// P_phys(v) = v - P_gauge(v) = v - sum_i q_i (q_i^T v).
///
/// This is the formulation P2 from the Phase III plan (projected complement).
/// The spectral operator H_phys = P_phys^T H P_phys operates on the
/// physical subspace, removing gauge directions from mode solves.
///
/// Delegates to GaugeProjector.ApplyPhysical for the actual computation.
/// </summary>
public sealed class PhysicalProjector : ILinearOperator
{
    private readonly GaugeProjector _projector;

    public PhysicalProjector(GaugeProjector projector)
    {
        _projector = projector ?? throw new ArgumentNullException(nameof(projector));
    }

    public TensorSignature InputSignature => _projector.InputSignature;

    public TensorSignature OutputSignature => _projector.OutputSignature;

    public int InputDimension => _projector.InputDimension;
    public int OutputDimension => _projector.OutputDimension;

    /// <summary>The underlying gauge projector.</summary>
    public GaugeProjector GaugeProjector => _projector;

    public FieldTensor Apply(FieldTensor v)
    {
        var result = _projector.ApplyPhysical(v.Coefficients);
        return new FieldTensor
        {
            Label = "P_phys*v",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = new[] { result.Length },
        };
    }

    public FieldTensor ApplyTranspose(FieldTensor v)
    {
        // P_phys is self-adjoint (symmetric projector), so P^T = P
        return Apply(v);
    }
}
