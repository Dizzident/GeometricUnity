using Gu.Core;

namespace Gu.Branching;

/// <summary>
/// Computes the torsion branch term T_h. Per Section 4.7 and IX-1.
/// Output MUST have same TensorSignature as IShiabBranchOperator output.
/// This is a strict identity requirement (all fields, including MemoryLayout
/// and ComponentOrderId) confirmed by the physicist.
/// </summary>
public interface ITorsionBranchOperator
{
    /// <summary>Torsion branch identifier for manifest.</summary>
    string BranchId { get; }

    /// <summary>Expected output carrier type (must match Shiab output).</summary>
    string OutputCarrierType { get; }

    /// <summary>
    /// Full output TensorSignature. Must be strictly identical to the
    /// corresponding IShiabBranchOperator.OutputSignature for Upsilon = S - T
    /// to be well-defined.
    /// </summary>
    TensorSignature OutputSignature { get; }

    /// <summary>Evaluate T_h.</summary>
    FieldTensor Evaluate(
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry);

    /// <summary>
    /// Linearization: dT/domega acting on perturbation delta_omega.
    /// Required for Jacobian assembly (M5).
    /// </summary>
    FieldTensor Linearize(
        FieldTensor omega,
        FieldTensor a0,
        FieldTensor deltaOmega,
        BranchManifest manifest,
        GeometryContext geometry);
}
