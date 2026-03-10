using Gu.Core;
using Gu.Phase3.Observables;

namespace Gu.Phase3.Observables.Tests;

public class FiniteDifferenceObservationTests
{
    private static TensorSignature ObsSig() => new()
    {
        AmbientSpaceId = "X_h",
        CarrierType = "curvature-2form",
        Degree = "2",
        LieAlgebraBasisId = "standard",
        ComponentOrderId = "face-major",
        MemoryLayout = "dense-row-major",
    };

    [Fact]
    public void LinearFunction_ExactFD()
    {
        // Obs(omega) = 2*omega (linear), so D_Obs = 2*I exactly
        FiniteDifferenceObservation.ObservationFunc observe = omega =>
        {
            var result = new double[omega.Length];
            for (int i = 0; i < omega.Length; i++)
                result[i] = 2.0 * omega[i];
            return new FieldTensor
            {
                Label = "obs",
                Signature = ObsSig(),
                Coefficients = result,
                Shape = new[] { result.Length },
            };
        };

        var omegaStar = new double[] { 1.0, 2.0, 3.0 };
        var fd = new FiniteDifferenceObservation(observe, omegaStar, "test-bg");

        var mode = new double[] { 1.0, 0.0, 0.0 };
        var sig = fd.Apply(mode, "mode-0");

        Assert.Equal(LinearizationMethod.FiniteDifference, sig.LinearizationMethod);
        // D_Obs(e_1) = 2*e_1
        Assert.Equal(2.0, sig.ObservedCoefficients[0], 6);
        Assert.Equal(0.0, sig.ObservedCoefficients[1], 6);
        Assert.Equal(0.0, sig.ObservedCoefficients[2], 6);
    }

    [Fact]
    public void QuadraticFunction_ApproximateFD()
    {
        // Obs(omega) = omega^2 (elementwise), D_Obs(v) = 2*omega_star*v
        FiniteDifferenceObservation.ObservationFunc observe = omega =>
        {
            var result = new double[omega.Length];
            for (int i = 0; i < omega.Length; i++)
                result[i] = omega[i] * omega[i];
            return new FieldTensor
            {
                Label = "obs",
                Signature = ObsSig(),
                Coefficients = result,
                Shape = new[] { result.Length },
            };
        };

        var omegaStar = new double[] { 3.0, 5.0 };
        var fd = new FiniteDifferenceObservation(observe, omegaStar, "test-bg", epsilon: 1e-7);

        var mode = new double[] { 1.0, 0.0 };
        var sig = fd.Apply(mode, "mode-0");

        // D_Obs(e_1) = 2*3 = 6 (with FD error O(eps))
        Assert.Equal(6.0, sig.ObservedCoefficients[0], 4);
        Assert.Equal(0.0, sig.ObservedCoefficients[1], 4);
    }

    [Fact]
    public void ZeroMode_ZeroSignature()
    {
        FiniteDifferenceObservation.ObservationFunc observe = omega =>
        {
            return new FieldTensor
            {
                Label = "obs",
                Signature = ObsSig(),
                Coefficients = (double[])omega.Clone(),
                Shape = new[] { omega.Length },
            };
        };

        var omegaStar = new double[] { 1.0, 2.0 };
        var fd = new FiniteDifferenceObservation(observe, omegaStar, "test-bg");

        var zero = new double[] { 0.0, 0.0 };
        var sig = fd.Apply(zero, "mode-zero");

        Assert.Equal(0.0, sig.L2Norm, 10);
    }

    [Fact]
    public void AnalyticVsFD_Agreement_ForLinearObs()
    {
        // For a linear observation, analytic and FD should agree exactly
        FiniteDifferenceObservation.ObservationFunc observe = omega =>
        {
            var result = new double[omega.Length];
            for (int i = 0; i < omega.Length; i++)
                result[i] = 3.0 * omega[i] + 1.0; // affine
            return new FieldTensor
            {
                Label = "obs",
                Signature = ObsSig(),
                Coefficients = result,
                Shape = new[] { result.Length },
            };
        };

        var omegaStar = new double[] { 1.0, 2.0 };
        var fd = new FiniteDifferenceObservation(observe, omegaStar, "test-bg");

        // "Analytic" D_Obs = 3*I
        var mode = new double[] { 1.0, 0.5 };
        var fdSig = fd.Apply(mode, "mode-0");

        // Expected: 3*mode = [3.0, 1.5]
        var analyticSig = new ObservedModeSignature
        {
            ModeId = "mode-0",
            BackgroundId = "test-bg",
            ObservedCoefficients = new double[] { 3.0, 1.5 },
            ObservedSignature = ObsSig(),
            ObservedShape = new[] { 2 },
            LinearizationMethod = LinearizationMethod.Analytic,
        };

        double error = ObservationLinearizationValidator.RelativeError(analyticSig, fdSig);
        Assert.True(error < 1e-6, $"Analytic vs FD error for linear: {error:E6}");
    }

    [Fact]
    public void RerunStability_DeterministicResult()
    {
        FiniteDifferenceObservation.ObservationFunc observe = omega =>
        {
            var result = new double[omega.Length];
            for (int i = 0; i < omega.Length; i++)
                result[i] = omega[i] * omega[i];
            return new FieldTensor
            {
                Label = "obs",
                Signature = ObsSig(),
                Coefficients = result,
                Shape = new[] { result.Length },
            };
        };

        var omegaStar = new double[] { 1.0, 2.0, 3.0 };
        var mode = new double[] { 0.1, 0.2, 0.3 };

        // Run twice
        var fd1 = new FiniteDifferenceObservation(observe, omegaStar, "test-bg");
        var sig1 = fd1.Apply(mode, "mode-0");

        var fd2 = new FiniteDifferenceObservation(observe, omegaStar, "test-bg");
        var sig2 = fd2.Apply(mode, "mode-0");

        // Results should be identical (deterministic)
        for (int i = 0; i < sig1.ObservedCoefficients.Length; i++)
            Assert.Equal(sig1.ObservedCoefficients[i], sig2.ObservedCoefficients[i], 15);
    }
}
