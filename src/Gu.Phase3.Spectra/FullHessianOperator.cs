using Gu.Branching;
using Gu.Core;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Full Hessian operator H_full, which includes the residual correction term:
///
///   H_full(v) = J^T M J v + (dJ/dz[v])^T M Upsilon_*
///
/// The Gauss-Newton approximation H_GN = J^T M J drops the second term.
/// H_full is valid for any admissible background; H_GN only for B2.
///
/// PHYSICS CONSTRAINT #1: GN ONLY valid for B2 backgrounds.
/// This operator provides the full Hessian for non-B2 backgrounds.
///
/// Implementation: When no correction action is provided, falls back to H_GN.
/// When provided, the correction action computes (dJ/dz[v])^T M Upsilon_*
/// directly (e.g., via finite differences).
/// </summary>
public sealed class FullHessianOperator : ILinearOperator
{
    private readonly ILinearOperator _hessianGN;
    private readonly Func<FieldTensor, FieldTensor>? _correctionAction;

    /// <param name="hessianGN">Gauss-Newton Hessian H_GN = J^T M J.</param>
    /// <param name="correctionAction">
    /// Computes the residual correction (dJ/dz[v])^T M Upsilon_*.
    /// If null, operator degrades to H_GN (with explicit tracking).
    /// </param>
    public FullHessianOperator(
        ILinearOperator hessianGN,
        Func<FieldTensor, FieldTensor>? correctionAction = null)
    {
        _hessianGN = hessianGN ?? throw new ArgumentNullException(nameof(hessianGN));
        _correctionAction = correctionAction;
    }

    /// <summary>Whether the residual correction is active.</summary>
    public bool HasResidualCorrection => _correctionAction is not null;

    public TensorSignature InputSignature => _hessianGN.InputSignature;
    public TensorSignature OutputSignature => _hessianGN.OutputSignature;
    public int InputDimension => _hessianGN.InputDimension;
    public int OutputDimension => _hessianGN.OutputDimension;

    /// <summary>
    /// Apply H_full(v) = H_GN(v) + correction(v).
    /// </summary>
    public FieldTensor Apply(FieldTensor v)
    {
        var gnResult = _hessianGN.Apply(v);

        if (_correctionAction is null)
            return gnResult;

        var correction = _correctionAction(v);

        var result = new double[gnResult.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = gnResult.Coefficients[i] + correction.Coefficients[i];

        return new FieldTensor
        {
            Label = "H_full*v",
            Signature = OutputSignature,
            Coefficients = result,
            Shape = gnResult.Shape,
        };
    }

    /// <summary>
    /// Transpose = Apply (H_full is self-adjoint when both terms are).
    /// </summary>
    public FieldTensor ApplyTranspose(FieldTensor v) => Apply(v);
}
