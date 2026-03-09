using Gu.Branching;

namespace Gu.Phase2.Stability;

/// <summary>
/// A gauge constraint operator C_* as an ILinearOperator.
///
/// Maps connection perturbations to gauge violation values.
/// The gauge-fixing slice condition is C(u) = 0.
/// Its linearization C_* is a linear operator used in the
/// gauge-fixed linearized operator L_tilde = (J_*, C_*).
/// </summary>
public interface IGaugeConstraintOperator : ILinearOperator
{
    /// <summary>
    /// Identifies the gauge handling mode (e.g., "coulomb", "lorenz", "temporal").
    /// </summary>
    string GaugeHandlingMode { get; }
}
