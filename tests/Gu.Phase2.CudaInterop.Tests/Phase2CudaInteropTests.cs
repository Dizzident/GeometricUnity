using Gu.Phase2.CudaInterop;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.CudaInterop.Tests;

/// <summary>
/// Tests for Phase II CUDA interop contracts, parity checker, and type construction.
/// GPU-dependent tests (Jv, J^Tw, Hv parity) are skipped when the native library
/// is not available. Contract and parity-checker tests run on all platforms.
/// </summary>
public class Phase2CudaInteropTests
{
    private static BranchVariantManifest TestVariant(string id = "v1") => new()
    {
        Id = id,
        ParentFamilyId = "test-family",
        A0Variant = "zero",
        BiConnectionVariant = "A0-plus-omega",
        TorsionVariant = "trivial",
        ShiabVariant = "identity",
        ObservationVariant = "sigma-pullback",
        ExtractionVariant = "default",
        GaugeVariant = "coulomb",
        RegularityVariant = "standard",
        PairingVariant = "trace",
        ExpectedClaimCeiling = "structural",
    };

    [Fact]
    public void Phase2ResidualKernelArgs_CanConstruct()
    {
        var args = new Phase2ResidualKernelArgs
        {
            Variant = TestVariant(),
            FieldDof = 9,
            ResidualDof = 3,
            DimG = 3,
            EdgeCount = 3,
            FaceCount = 1,
        };

        Assert.Equal(9, args.FieldDof);
        Assert.Equal(3, args.ResidualDof);
        Assert.Equal("v1", args.Variant.Id);
    }

    [Fact]
    public void ParityChecker_IdenticalOutputs_Passes()
    {
        var cpu = new double[] { 1.0, 2.0, 3.0, 4.0 };
        var gpu = new double[] { 1.0, 2.0, 3.0, 4.0 };

        var result = Phase2ParityChecker.Compare("test-kernel", cpu, gpu);

        Assert.True(result.Passed);
        Assert.Equal(0.0, result.MaxRelativeError);
        Assert.Equal(4, result.ComponentsCompared);
        Assert.Equal("test-kernel", result.KernelName);
    }

    [Fact]
    public void ParityChecker_SmallDifference_Passes()
    {
        var cpu = new double[] { 1.0, 2.0, 3.0 };
        var gpu = new double[] { 1.0 + 1e-12, 2.0 - 1e-12, 3.0 + 1e-12 };

        var result = Phase2ParityChecker.Compare("Jv", cpu, gpu);

        Assert.True(result.Passed);
        Assert.True(result.MaxRelativeError < 1e-9);
    }

    [Fact]
    public void ParityChecker_LargeDifference_Fails()
    {
        var cpu = new double[] { 1.0, 2.0, 3.0 };
        var gpu = new double[] { 1.1, 2.0, 3.0 };

        var result = Phase2ParityChecker.Compare("Jv", cpu, gpu);

        Assert.False(result.Passed);
        Assert.True(result.MaxRelativeError > 0.01);
    }

    [Fact]
    public void ParityChecker_MismatchedLengths_Throws()
    {
        var cpu = new double[] { 1.0, 2.0 };
        var gpu = new double[] { 1.0, 2.0, 3.0 };

        Assert.Throws<ArgumentException>(() =>
            Phase2ParityChecker.Compare("test", cpu, gpu));
    }

    [Fact]
    public void ParityChecker_CustomTolerance_Respected()
    {
        var cpu = new double[] { 1.0, 2.0 };
        var gpu = new double[] { 1.001, 2.001 };

        var loose = Phase2ParityChecker.Compare("test", cpu, gpu, tolerance: 0.01);
        Assert.True(loose.Passed);

        var tight = Phase2ParityChecker.Compare("test", cpu, gpu, tolerance: 1e-6);
        Assert.False(tight.Passed);
    }

    [Fact]
    public void IPhase2JacobianKernel_InterfaceHasCorrectMethods()
    {
        // Verify the interface shape at compile time
        var type = typeof(IPhase2JacobianKernel);
        Assert.NotNull(type.GetMethod("ApplyJv"));
        Assert.NotNull(type.GetMethod("ApplyJtw"));
    }

    [Fact]
    public void IPhase2HessianKernel_InterfaceHasCorrectMethods()
    {
        var type = typeof(IPhase2HessianKernel);
        Assert.NotNull(type.GetMethod("ApplyHv"));
    }

    [Fact]
    public void IPhase2BatchKernel_InterfaceHasCorrectMethods()
    {
        var type = typeof(IPhase2BatchKernel);
        Assert.NotNull(type.GetMethod("EvaluateBatch"));
    }

    [Fact]
    public void Phase2CudaBackend_ThrowsWhenNotInitialized()
    {
        using var backend = new Phase2CudaBackend();
        var variant = TestVariant();
        var u = new double[9];
        var v = new double[9];
        var result = new double[3];

        Assert.Throws<InvalidOperationException>(() =>
            backend.ApplyJv(u, v, result, variant));
    }

    [Fact]
    public void Phase2CudaBackend_ThrowsAfterDispose()
    {
        var backend = new Phase2CudaBackend();
        backend.Dispose();

        var variant = TestVariant();
        var u = new double[9];
        var v = new double[9];
        var result = new double[3];

        Assert.Throws<ObjectDisposedException>(() =>
            backend.ApplyJv(u, v, result, variant));
    }

    [Fact]
    public void Phase2CudaBackend_ImplementsAllInterfaces()
    {
        using var backend = new Phase2CudaBackend();

        Assert.IsAssignableFrom<IPhase2JacobianKernel>(backend);
        Assert.IsAssignableFrom<IPhase2HessianKernel>(backend);
        Assert.IsAssignableFrom<IPhase2BatchKernel>(backend);
        Assert.IsAssignableFrom<IDisposable>(backend);
    }

    [Fact]
    public void Phase2CudaBackend_DisposeIsIdempotent()
    {
        var backend = new Phase2CudaBackend();
        backend.Dispose();
        backend.Dispose(); // Should not throw
    }

    [Fact]
    public void Phase2CudaBackend_ApplyJv_ThrowsForNonZeroInput()
    {
        using var backend = new Phase2CudaBackend();
        var variant = TestVariant();
        var u = new double[] { 1.0, 0.0, 0.0 };
        var v = new double[3];
        var result = new double[3];

        var ex = Assert.Throws<NotSupportedException>(() =>
            backend.ApplyJv(u, v, result, variant));
        Assert.Contains("CUDA kernels are stubs", ex.Message);
    }

    [Fact]
    public void Phase2CudaBackend_ApplyJtw_ThrowsForNonZeroInput()
    {
        using var backend = new Phase2CudaBackend();
        var variant = TestVariant();
        var u = new double[3];
        var w = new double[] { 0.0, 0.5, 0.0 };
        var result = new double[3];

        var ex = Assert.Throws<NotSupportedException>(() =>
            backend.ApplyJtw(u, w, result, variant));
        Assert.Contains("CUDA kernels are stubs", ex.Message);
    }

    [Fact]
    public void Phase2CudaBackend_ApplyHv_ThrowsForNonZeroInput()
    {
        using var backend = new Phase2CudaBackend();
        var variant = TestVariant();
        var u = new double[] { 0.0, 0.0, 1e-15 };
        var v = new double[3];
        var result = new double[3];

        var ex = Assert.Throws<NotSupportedException>(() =>
            backend.ApplyHv(u, v, result, variant, 1.0));
        Assert.Contains("CUDA kernels are stubs", ex.Message);
    }

    [Fact]
    public void Phase2CudaBackend_EvaluateBatch_ThrowsForNonZeroInput()
    {
        using var backend = new Phase2CudaBackend();
        var variants = new[] { TestVariant() };
        var states = new double[] { 0.0, 0.0, 1.0 };
        var residuals = new double[3];

        var ex = Assert.Throws<NotSupportedException>(() =>
            backend.EvaluateBatch(variants, states, residuals, 3, 3));
        Assert.Contains("CUDA kernels are stubs", ex.Message);
    }
}
