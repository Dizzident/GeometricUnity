using Gu.Core;

namespace Gu.Branching;

/// <summary>
/// Computes the Shiab branch term S_h. Per Section 4.8 and IX-2.
/// Output MUST have same TensorSignature as ITorsionBranchOperator output.
/// This is a strict identity requirement (all fields, including MemoryLayout
/// and ComponentOrderId) confirmed by the physicist.
/// This is a declared branch operator, not uniquely canonical (Section 23).
/// </summary>
public interface IShiabBranchOperator
{
    /// <summary>Shiab branch identifier for manifest.</summary>
    string BranchId { get; }

    /// <summary>Expected output carrier type (must match torsion output).</summary>
    string OutputCarrierType { get; }

    /// <summary>
    /// Full output TensorSignature. Must be strictly identical to the
    /// corresponding ITorsionBranchOperator.OutputSignature for Upsilon = S - T
    /// to be well-defined.
    /// </summary>
    TensorSignature OutputSignature { get; }

    /// <summary>Evaluate S_h.</summary>
    FieldTensor Evaluate(
        FieldTensor curvatureF,
        FieldTensor omega,
        BranchManifest manifest,
        GeometryContext geometry);

    /// <summary>
    /// Linearization: dS/domega acting on perturbation delta_omega.
    /// Note: this must account for omega-dependence through F AND any
    /// direct omega-dependence.
    /// </summary>
    FieldTensor Linearize(
        FieldTensor curvatureF,
        FieldTensor omega,
        FieldTensor deltaOmega,
        BranchManifest manifest,
        GeometryContext geometry);
}
