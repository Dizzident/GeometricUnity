using Gu.Branching;
using Gu.Core;

namespace Gu.ReferenceCpu;

/// <summary>
/// Finite-difference verification harness for Jacobian operators.
/// Compares J*v (analytic) against (f(x+eps*v) - f(x))/eps (numerical).
/// </summary>
public static class FiniteDifferenceVerifier
{
    /// <summary>
    /// Result of a finite-difference verification.
    /// </summary>
    public sealed class VerificationResult
    {
        /// <summary>Maximum absolute difference between analytic and FD.</summary>
        public required double MaxAbsoluteError { get; init; }

        /// <summary>Maximum relative error (where FD is non-negligible).</summary>
        public required double MaxRelativeError { get; init; }

        /// <summary>Index of the component with maximum absolute error.</summary>
        public required int MaxErrorIndex { get; init; }

        /// <summary>Whether the verification passed (max error below tolerance).</summary>
        public required bool Passed { get; init; }

        /// <summary>Step size used.</summary>
        public required double Epsilon { get; init; }
    }

    /// <summary>
    /// Verify a linear operator against finite differences.
    /// </summary>
    /// <param name="jacobian">The linear operator to verify.</param>
    /// <param name="evaluateResidual">Function that computes residual from omega FieldTensor.</param>
    /// <param name="omega">Base point (connection field as FieldTensor).</param>
    /// <param name="perturbation">Direction to perturb.</param>
    /// <param name="eps">Finite difference step size.</param>
    /// <param name="absTol">Absolute tolerance for pass/fail.</param>
    public static VerificationResult Verify(
        ILinearOperator jacobian,
        Func<FieldTensor, FieldTensor> evaluateResidual,
        FieldTensor omega,
        FieldTensor perturbation,
        double eps = 1e-7,
        double absTol = 1e-4)
    {
        // Analytic: J * perturbation
        var analyticResult = jacobian.Apply(perturbation);

        // Numerical: (f(omega + eps*perturbation) - f(omega)) / eps
        var omegaPerturbed = FieldTensorOps.AddScaled(omega, perturbation, eps);
        var fBase = evaluateResidual(omega);
        var fPerturbed = evaluateResidual(omegaPerturbed);

        int n = analyticResult.Coefficients.Length;
        double maxAbsErr = 0;
        double maxRelErr = 0;
        int maxErrIdx = 0;

        for (int i = 0; i < n; i++)
        {
            double fd = (fPerturbed.Coefficients[i] - fBase.Coefficients[i]) / eps;
            double analytic = analyticResult.Coefficients[i];
            double absErr = System.Math.Abs(fd - analytic);

            if (absErr > maxAbsErr)
            {
                maxAbsErr = absErr;
                maxErrIdx = i;
            }

            double scale = System.Math.Max(System.Math.Abs(fd), System.Math.Abs(analytic));
            if (scale > 1e-12)
            {
                double relErr = absErr / scale;
                if (relErr > maxRelErr)
                    maxRelErr = relErr;
            }
        }

        return new VerificationResult
        {
            MaxAbsoluteError = maxAbsErr,
            MaxRelativeError = maxRelErr,
            MaxErrorIndex = maxErrIdx,
            Passed = maxAbsErr < absTol,
            Epsilon = eps,
        };
    }

    /// <summary>
    /// Verify J^T (transpose) against finite differences of the objective gradient.
    /// Checks that J^T * M * Upsilon matches (dI2/domega) via FD.
    /// </summary>
    public static VerificationResult VerifyGradient(
        CpuLocalJacobian jacobian,
        CpuMassMatrix massMatrix,
        Func<FieldTensor, FieldTensor> evaluateUpsilon,
        FieldTensor omega,
        FieldTensor perturbation,
        double eps = 1e-7,
        double absTol = 1e-4)
    {
        // Analytic gradient: J^T * M * Upsilon
        var upsilon = evaluateUpsilon(omega);
        var gradient = jacobian.ComputeGradient(upsilon, massMatrix);

        // Directional derivative: <grad, perturbation> (analytic)
        double analyticDirectional = FieldTensorOps.Dot(gradient, perturbation);

        // FD: (I2(omega + eps*v) - I2(omega)) / eps
        var omegaPlus = FieldTensorOps.AddScaled(omega, perturbation, eps);
        var upsilonPlus = evaluateUpsilon(omegaPlus);
        double i2Base = massMatrix.EvaluateObjective(upsilon);
        double i2Plus = massMatrix.EvaluateObjective(upsilonPlus);
        double fdDirectional = (i2Plus - i2Base) / eps;

        double absErr = System.Math.Abs(fdDirectional - analyticDirectional);
        double scale = System.Math.Max(System.Math.Abs(fdDirectional), System.Math.Abs(analyticDirectional));
        double relErr = scale > 1e-12 ? absErr / scale : 0;

        return new VerificationResult
        {
            MaxAbsoluteError = absErr,
            MaxRelativeError = relErr,
            MaxErrorIndex = -1, // scalar comparison
            Passed = absErr < absTol,
            Epsilon = eps,
        };
    }
}
