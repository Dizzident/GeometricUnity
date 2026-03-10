using Gu.Phase3.CudaSpectra;

namespace Gu.Phase3.CudaSpectra.Tests;

public class CudaSpectralConfigTests
{
    [Fact]
    public void Defaults_AreReasonable()
    {
        var config = new CudaSpectralConfig();

        Assert.Equal(1e-9, config.ParityTolerance);
        Assert.Equal(256, config.CudaBlockSize);
        Assert.Equal(2, config.NumStreams);
        Assert.False(config.ForceCpu);
        Assert.True(config.EnableParityChecks);
        Assert.Equal(5, config.ParityTestVectors);
        Assert.Equal(42, config.ParitySeed);
        Assert.Equal(0L, config.MaxGpuMemoryBytes);
        Assert.NotNull(config.EigensolverConfig);
    }

    [Fact]
    public void CpuOnly_DisablesGpuAndParity()
    {
        var config = CudaSpectralConfig.CpuOnly();

        Assert.True(config.ForceCpu);
        Assert.False(config.EnableParityChecks);
    }

    [Fact]
    public void EigensolverConfig_HasDefaults()
    {
        var config = new CudaSpectralConfig();

        Assert.Equal(10, config.EigensolverConfig.NumEigenvalues);
        Assert.Equal(200, config.EigensolverConfig.MaxIterations);
        Assert.Equal(1e-8, config.EigensolverConfig.Tolerance);
    }

    [Fact]
    public void CustomConfig_RoundTrips()
    {
        var config = new CudaSpectralConfig
        {
            ParityTolerance = 1e-6,
            CudaBlockSize = 512,
            NumStreams = 4,
            ForceCpu = true,
            EnableParityChecks = false,
            ParityTestVectors = 10,
            ParitySeed = 99,
            MaxGpuMemoryBytes = 1024 * 1024 * 1024,
        };

        Assert.Equal(1e-6, config.ParityTolerance);
        Assert.Equal(512, config.CudaBlockSize);
        Assert.Equal(4, config.NumStreams);
        Assert.True(config.ForceCpu);
        Assert.False(config.EnableParityChecks);
        Assert.Equal(10, config.ParityTestVectors);
        Assert.Equal(99, config.ParitySeed);
        Assert.Equal(1024 * 1024 * 1024, config.MaxGpuMemoryBytes);
    }
}
