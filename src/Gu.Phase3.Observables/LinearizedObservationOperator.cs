using Gu.Branching;
using Gu.Core;
using Gu.Geometry;

namespace Gu.Phase3.Observables;

/// <summary>
/// Linearized observation map: D_Obs = sigma_h^* o J_h.
///
/// Given a connection perturbation delta_omega (mode vector), computes:
///   D_Obs(delta_omega) = sigma_h^*(J_h * delta_omega)
///
/// where J_h = dUpsilon/domega is the residual Jacobian and sigma_h^* is
/// the pullback to X_h. This produces an observed-space perturbation
/// (field on X_h) for each mode.
///
/// Supports both analytic linearization (via J_h) and finite-difference
/// fallback. Every result declares its linearization method.
/// </summary>
public sealed class LinearizedObservationOperator
{
    private readonly ILinearOperator _jacobian;
    private readonly PullbackOperator _pullback;
    private readonly string _backgroundId;

    /// <summary>Background state ID this operator was linearized at.</summary>
    public string BackgroundId => _backgroundId;

    /// <summary>Input dimension (connection space).</summary>
    public int InputDimension => _jacobian.InputDimension;

    /// <summary>
    /// Create a linearized observation operator from a Jacobian and pullback.
    /// </summary>
    /// <param name="jacobian">Residual Jacobian J_h = dUpsilon/domega at the background.</param>
    /// <param name="pullback">Pullback operator sigma_h^*.</param>
    /// <param name="backgroundId">Background state ID.</param>
    public LinearizedObservationOperator(
        ILinearOperator jacobian,
        PullbackOperator pullback,
        string backgroundId)
    {
        _jacobian = jacobian ?? throw new ArgumentNullException(nameof(jacobian));
        _pullback = pullback ?? throw new ArgumentNullException(nameof(pullback));
        _backgroundId = backgroundId ?? throw new ArgumentNullException(nameof(backgroundId));
    }

    /// <summary>
    /// Apply the linearized observation map to a connection perturbation:
    ///   D_Obs(v) = sigma_h^*(J * v)
    ///
    /// Returns an observed-space field on X_h.
    /// </summary>
    public ObservedModeSignature Apply(double[] modeVector, string modeId)
    {
        if (modeVector == null) throw new ArgumentNullException(nameof(modeVector));

        // Step 1: J * v (residual perturbation in Y_h)
        var vTensor = new FieldTensor
        {
            Label = $"mode-{modeId}",
            Signature = _jacobian.InputSignature,
            Coefficients = modeVector,
            Shape = new[] { modeVector.Length },
        };
        var jv = _jacobian.Apply(vTensor);

        // Step 2: sigma_h^*(J * v) (pullback to X_h)
        // The Jacobian output is a face-based 2-form (curvature-like).
        // Determine components per face from Jacobian output.
        int componentsPerFace = jv.Coefficients.Length > 0 && jv.Shape.Count >= 2
            ? jv.Shape[1]
            : 1;
        var observed = _pullback.ApplyFaceField(jv, componentsPerFace);

        return new ObservedModeSignature
        {
            ModeId = modeId,
            BackgroundId = _backgroundId,
            ObservedCoefficients = observed.Coefficients,
            ObservedSignature = observed.Signature,
            ObservedShape = observed.Shape.ToArray(),
            LinearizationMethod = LinearizationMethod.Analytic,
        };
    }
}
