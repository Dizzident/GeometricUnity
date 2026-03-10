using Gu.Core;
using Gu.Geometry;

namespace Gu.Phase3.Observables;

/// <summary>
/// Finite-difference approximation of the linearized observation map.
///
/// Used for validation against the analytic linearization.
/// Computes:
///   D_Obs_FD(v) ≈ (Obs(z_* + eps*v) - Obs(z_*)) / eps
///
/// where Obs = sigma_h^* o Upsilon is the full nonlinear observation pipeline.
/// </summary>
public sealed class FiniteDifferenceObservation
{
    /// <summary>
    /// Delegate that computes the full nonlinear observation: Obs(omega) -> observed field on X_h.
    /// </summary>
    public delegate FieldTensor ObservationFunc(double[] omega);

    private readonly ObservationFunc _observe;
    private readonly double[] _omegaStar;
    private readonly FieldTensor _obsStar;
    private readonly string _backgroundId;
    private readonly double _epsilon;

    /// <summary>
    /// Create a finite-difference observation operator.
    /// </summary>
    /// <param name="observe">Full nonlinear observation pipeline.</param>
    /// <param name="omegaStar">Background connection (linearization point).</param>
    /// <param name="backgroundId">Background state ID.</param>
    /// <param name="epsilon">Finite-difference step size (default 1e-7).</param>
    public FiniteDifferenceObservation(
        ObservationFunc observe,
        double[] omegaStar,
        string backgroundId,
        double epsilon = 1e-7)
    {
        _observe = observe ?? throw new ArgumentNullException(nameof(observe));
        _omegaStar = omegaStar ?? throw new ArgumentNullException(nameof(omegaStar));
        _backgroundId = backgroundId ?? throw new ArgumentNullException(nameof(backgroundId));
        _epsilon = epsilon;

        // Pre-compute observation at background
        _obsStar = _observe(_omegaStar);
    }

    /// <summary>
    /// Apply finite-difference linearized observation to a mode vector:
    ///   D_Obs_FD(v) = (Obs(z_* + eps*v) - Obs(z_*)) / eps
    /// </summary>
    public ObservedModeSignature Apply(double[] modeVector, string modeId)
    {
        if (modeVector == null) throw new ArgumentNullException(nameof(modeVector));
        if (modeVector.Length != _omegaStar.Length)
            throw new ArgumentException(
                $"Mode vector length {modeVector.Length} != background length {_omegaStar.Length}.");

        // Perturbed state: z_* + eps * v
        var perturbed = new double[_omegaStar.Length];
        for (int i = 0; i < _omegaStar.Length; i++)
            perturbed[i] = _omegaStar[i] + _epsilon * modeVector[i];

        var obsPerturbed = _observe(perturbed);

        // Finite difference: (Obs(z_* + eps*v) - Obs(z_*)) / eps
        var coeffs = new double[_obsStar.Coefficients.Length];
        for (int i = 0; i < coeffs.Length; i++)
            coeffs[i] = (obsPerturbed.Coefficients[i] - _obsStar.Coefficients[i]) / _epsilon;

        return new ObservedModeSignature
        {
            ModeId = modeId,
            BackgroundId = _backgroundId,
            ObservedCoefficients = coeffs,
            ObservedSignature = _obsStar.Signature,
            ObservedShape = _obsStar.Shape.ToArray(),
            LinearizationMethod = LinearizationMethod.FiniteDifference,
        };
    }
}
