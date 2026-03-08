using Gu.Core;

namespace Gu.Solvers;

/// <summary>
/// Gauge stabilization penalty term (IA-4).
/// The Jacobian J_h may be singular due to gauge symmetry.
/// This penalty breaks the gauge degeneracy by adding:
///
///   I2_stabilized = I2 + (lambda/2) * ||omega||^2
///
/// This is a Coulomb-like gauge fixing: the penalty drives
/// omega toward the smallest-norm representative in its gauge orbit.
///
/// The gradient contribution is:  lambda * omega
/// The objective contribution is: (lambda/2) * ||omega||^2
///
/// More sophisticated gauge conditions (e.g., Lorenz gauge,
/// divergence-based) can be implemented by subclassing or
/// providing a custom IGaugePenalty.
/// </summary>
public sealed class GaugePenaltyTerm
{
    /// <summary>Penalty coefficient lambda.</summary>
    public double Lambda { get; }

    /// <summary>
    /// Create a gauge penalty term with the given coefficient.
    /// </summary>
    /// <param name="lambda">
    /// Penalty coefficient. Must be non-negative.
    /// Lambda = 0 disables the penalty entirely.
    /// </param>
    public GaugePenaltyTerm(double lambda)
    {
        if (lambda < 0)
            throw new ArgumentOutOfRangeException(nameof(lambda), "Gauge penalty lambda must be non-negative.");
        Lambda = lambda;
    }

    /// <summary>
    /// Evaluate the gauge penalty objective: (lambda/2) * ||omega||^2.
    /// </summary>
    public double EvaluateObjective(FieldTensor omega)
    {
        if (Lambda == 0) return 0;

        double normSq = 0;
        for (int i = 0; i < omega.Coefficients.Length; i++)
            normSq += omega.Coefficients[i] * omega.Coefficients[i];

        return 0.5 * Lambda * normSq;
    }

    /// <summary>
    /// Compute the gauge penalty gradient contribution: lambda * omega.
    /// This is added to the physics gradient G = J^T M Upsilon.
    /// </summary>
    public FieldTensor EvaluateGradient(FieldTensor omega)
    {
        var result = new double[omega.Coefficients.Length];

        if (Lambda != 0)
        {
            for (int i = 0; i < omega.Coefficients.Length; i++)
                result[i] = Lambda * omega.Coefficients[i];
        }

        return new FieldTensor
        {
            Label = "gauge_penalty_gradient",
            Signature = omega.Signature,
            Coefficients = result,
            Shape = omega.Shape,
        };
    }

    /// <summary>
    /// Compute the gauge violation measure: ||omega||^2.
    /// This is the raw quantity penalized (before scaling by lambda).
    /// </summary>
    public double ComputeViolation(FieldTensor omega)
    {
        double normSq = 0;
        for (int i = 0; i < omega.Coefficients.Length; i++)
            normSq += omega.Coefficients[i] * omega.Coefficients[i];
        return normSq;
    }

    /// <summary>
    /// Compute the gauge violation norm: ||omega||.
    /// </summary>
    public double ComputeViolationNorm(FieldTensor omega)
    {
        return System.Math.Sqrt(ComputeViolation(omega));
    }

    /// <summary>
    /// Add the gauge penalty gradient to the physics gradient in-place:
    /// G_total = G_physics + lambda * omega.
    /// Returns a new FieldTensor.
    /// </summary>
    public FieldTensor AddToGradient(FieldTensor physicsGradient, FieldTensor omega)
    {
        if (Lambda == 0) return physicsGradient;

        var result = new double[physicsGradient.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = physicsGradient.Coefficients[i] + Lambda * omega.Coefficients[i];

        return new FieldTensor
        {
            Label = "total_gradient",
            Signature = physicsGradient.Signature,
            Coefficients = result,
            Shape = physicsGradient.Shape,
        };
    }

    /// <summary>
    /// Add the gauge penalty objective to the physics objective:
    /// I2_total = I2_physics + (lambda/2) * ||omega||^2.
    /// </summary>
    public double AddToObjective(double physicsObjective, FieldTensor omega)
    {
        return physicsObjective + EvaluateObjective(omega);
    }
}
