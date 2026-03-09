using Gu.Core;

namespace Gu.Solvers;

/// <summary>
/// Abstraction for gauge-fixing penalty terms added to the solver objective.
/// Implementations include simple L2 penalty (GaugePenaltyTerm) and
/// proper Coulomb gauge penalty (CoulombGaugePenalty).
/// </summary>
public interface IGaugePenalty
{
    /// <summary>Penalty coefficient lambda.</summary>
    double Lambda { get; }

    /// <summary>
    /// Evaluate the gauge penalty objective contribution.
    /// </summary>
    double EvaluateObjective(FieldTensor omega);

    /// <summary>
    /// Evaluate the gauge penalty gradient contribution.
    /// </summary>
    FieldTensor EvaluateGradient(FieldTensor omega);

    /// <summary>
    /// Compute the gauge violation norm (un-scaled by lambda).
    /// </summary>
    double ComputeViolationNorm(FieldTensor omega);

    /// <summary>
    /// Add gauge penalty to physics objective: I2_total = I2_physics + penalty(omega).
    /// </summary>
    double AddToObjective(double physicsObjective, FieldTensor omega);

    /// <summary>
    /// Add gauge penalty gradient to physics gradient.
    /// Returns a new FieldTensor with combined gradient.
    /// </summary>
    FieldTensor AddToGradient(FieldTensor physicsGradient, FieldTensor omega);
}
