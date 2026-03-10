using Gu.Phase3.CudaSpectra;

namespace Gu.Phase3.CudaSpectra.Tests;

public class SpectralParityCheckerTests
{
    [Fact]
    public void Compare_IdenticalOutputs_Passes()
    {
        var a = new double[] { 1.0, 2.0, 3.0 };
        var b = new double[] { 1.0, 2.0, 3.0 };

        var result = SpectralParityChecker.Compare("test", a, b);
        Assert.True(result.Passed);
        Assert.Equal(0.0, result.MaxRelativeError);
        Assert.Equal(3, result.ComponentsCompared);
    }

    [Fact]
    public void Compare_SmallDifference_Passes()
    {
        var a = new double[] { 1.0, 2.0, 3.0 };
        var b = new double[] { 1.0 + 1e-12, 2.0, 3.0 };

        var result = SpectralParityChecker.Compare("test", a, b);
        Assert.True(result.Passed);
        Assert.True(result.MaxRelativeError < 1e-9);
    }

    [Fact]
    public void Compare_LargeDifference_Fails()
    {
        var a = new double[] { 1.0, 2.0, 3.0 };
        var b = new double[] { 1.0, 2.0, 4.0 };

        var result = SpectralParityChecker.Compare("test", a, b);
        Assert.False(result.Passed);
        Assert.True(result.MaxRelativeError > 0.1);
        Assert.Equal(2, result.MaxErrorIndex); // Component 2 has the error
    }

    [Fact]
    public void Compare_DifferentLengths_Throws()
    {
        var a = new double[] { 1.0, 2.0 };
        var b = new double[] { 1.0 };

        Assert.Throws<ArgumentException>(() => SpectralParityChecker.Compare("test", a, b));
    }

    [Fact]
    public void Compare_CustomTolerance()
    {
        var a = new double[] { 1.0 };
        var b = new double[] { 1.0 + 1e-6 };

        // With tight tolerance: fails
        var tight = SpectralParityChecker.Compare("test", a, b, 1e-8);
        Assert.False(tight.Passed);

        // With loose tolerance: passes
        var loose = SpectralParityChecker.Compare("test", a, b, 1e-4);
        Assert.True(loose.Passed);
    }

    [Fact]
    public void Compare_ZeroOutputs_ZeroError()
    {
        var a = new double[] { 0.0, 0.0, 0.0 };
        var b = new double[] { 0.0, 0.0, 0.0 };

        var result = SpectralParityChecker.Compare("test", a, b);
        Assert.True(result.Passed);
        Assert.Equal(0.0, result.MaxRelativeError);
    }

    [Fact]
    public void Compare_NearZero_UsesAbsoluteScaling()
    {
        // Formula: |a-b| / (1 + |a|) -- near zero the denominator is ~1
        var a = new double[] { 0.0 };
        var b = new double[] { 1e-10 };

        var result = SpectralParityChecker.Compare("test", a, b);
        Assert.True(result.Passed); // 1e-10 / 1.0 < 1e-9
    }

    [Fact]
    public void RunFullCheck_IdenticalKernels_AllPass()
    {
        int n = 10;
        var kernel1 = new TestHelpers.DiagonalSpectralKernel(n);
        var kernel2 = new TestHelpers.DiagonalSpectralKernel(n);

        var report = SpectralParityChecker.RunFullCheck(kernel1, kernel2, numTestVectors: 3);

        Assert.True(report.AllPassed);
        Assert.Equal(0.0, report.WorstError);
        Assert.Equal(12, report.Results.Count); // 3 vectors * 4 operations
    }

    [Fact]
    public void RunFullCheck_DimensionMismatch_Throws()
    {
        var k1 = new TestHelpers.DiagonalSpectralKernel(5);
        var k2 = new TestHelpers.DiagonalSpectralKernel(10);

        Assert.Throws<ArgumentException>(() =>
            SpectralParityChecker.RunFullCheck(k1, k2));
    }

    [Fact]
    public void ParityReport_EmptyResults()
    {
        var report = new SpectralParityChecker.ParityReport
        {
            Results = Array.Empty<SpectralParityChecker.ParityResult>(),
        };
        Assert.True(report.AllPassed);
        Assert.Equal(0.0, report.WorstError);
    }

    [Fact]
    public void ParityResult_Properties()
    {
        var result = new SpectralParityChecker.ParityResult
        {
            OperationName = "ApplySpectral",
            Passed = true,
            MaxRelativeError = 1e-12,
            Tolerance = 1e-9,
            ComponentsCompared = 100,
            MaxErrorIndex = 42,
        };
        Assert.Equal("ApplySpectral", result.OperationName);
        Assert.True(result.Passed);
        Assert.Equal(42, result.MaxErrorIndex);
    }
}
