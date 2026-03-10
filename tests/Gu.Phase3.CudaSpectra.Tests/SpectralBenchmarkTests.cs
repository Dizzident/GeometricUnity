using Gu.Phase3.CudaSpectra;

namespace Gu.Phase3.CudaSpectra.Tests;

public class SpectralBenchmarkTests
{
    [Fact]
    public void RunOperatorBenchmarks_ProducesTimings()
    {
        var kernel = new TestHelpers.DiagonalSpectralKernel(10);
        var runner = new SpectralBenchmarkRunner(kernel, "cpu-test");

        var artifact = runner.RunOperatorBenchmarks(repetitions: 5);

        Assert.Equal("cpu-test", artifact.Backend);
        Assert.Equal(10, artifact.StateDimension);
        Assert.Equal(10, artifact.ResidualDimension);
        Assert.Equal(4, artifact.Timings.Count);
        Assert.All(artifact.Timings, t =>
        {
            Assert.Equal(5, t.Repetitions);
            Assert.True(t.TotalMs >= 0);
            Assert.True(t.AverageMs >= 0);
        });
    }

    [Fact]
    public void RunEigensolverBenchmark_ProducesResult()
    {
        var kernel = new TestHelpers.DiagonalSpectralKernel(10);
        var runner = new SpectralBenchmarkRunner(kernel, "cpu-test");

        var config = new LobpcgConfig
        {
            NumEigenvalues = 2,
            BlockSize = 2,
            MaxIterations = 50,
            Tolerance = 1e-5,
        };

        var artifact = runner.RunEigensolverBenchmark(config);

        Assert.NotNull(artifact.EigensolverResult);
        Assert.Equal(2, artifact.EigensolverResult.NumEigenvalues);
        Assert.True(artifact.EigensolverResult.TotalMs >= 0);
        Assert.True(artifact.EigensolverResult.Iterations > 0);
    }

    [Fact]
    public void BenchmarkArtifact_SerializesRoundTrip()
    {
        var artifact = new SpectralBenchmarkArtifact
        {
            BenchmarkId = "test-bench",
            Timestamp = DateTimeOffset.UtcNow,
            StateDimension = 100,
            ResidualDimension = 50,
            Backend = "cpu",
            Timings = new[]
            {
                new SpectralBenchmarkArtifact.OperationTiming
                {
                    Operation = "ApplySpectral",
                    Repetitions = 10,
                    TotalMs = 5.5,
                },
            },
            EigensolverResult = new SpectralBenchmarkArtifact.EigensolverTiming
            {
                NumEigenvalues = 5,
                Iterations = 42,
                Converged = true,
                TotalMs = 120.5,
            },
        };

        var json = artifact.ToJson();
        Assert.False(string.IsNullOrWhiteSpace(json));

        var deserialized = SpectralBenchmarkArtifact.FromJson(json);
        Assert.NotNull(deserialized);
        Assert.Equal("test-bench", deserialized.BenchmarkId);
        Assert.Equal(100, deserialized.StateDimension);
        Assert.Single(deserialized.Timings);
        Assert.NotNull(deserialized.EigensolverResult);
        Assert.Equal(42, deserialized.EigensolverResult.Iterations);
    }

    [Fact]
    public void BenchmarkArtifact_FileRoundTrip()
    {
        var artifact = new SpectralBenchmarkArtifact
        {
            BenchmarkId = "file-bench",
            Timestamp = DateTimeOffset.UtcNow,
            StateDimension = 50,
            ResidualDimension = 30,
            Backend = "cpu",
            Timings = Array.Empty<SpectralBenchmarkArtifact.OperationTiming>(),
        };

        var path = Path.Combine(Path.GetTempPath(), $"bench-{Guid.NewGuid()}.json");
        try
        {
            artifact.WriteToFile(path);
            var loaded = SpectralBenchmarkArtifact.ReadFromFile(path);
            Assert.NotNull(loaded);
            Assert.Equal("file-bench", loaded.BenchmarkId);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Fact]
    public void OperationTiming_AverageMs_Computed()
    {
        var timing = new SpectralBenchmarkArtifact.OperationTiming
        {
            Operation = "test",
            Repetitions = 10,
            TotalMs = 50.0,
        };
        Assert.Equal(5.0, timing.AverageMs);
    }

    [Fact]
    public void OperationTiming_ZeroReps_AverageIsZero()
    {
        var timing = new SpectralBenchmarkArtifact.OperationTiming
        {
            Operation = "test",
            Repetitions = 0,
            TotalMs = 0.0,
        };
        Assert.Equal(0.0, timing.AverageMs);
    }

    [Fact]
    public void EigensolverTiming_PerIterationMs_Computed()
    {
        var timing = new SpectralBenchmarkArtifact.EigensolverTiming
        {
            NumEigenvalues = 5,
            Iterations = 10,
            Converged = true,
            TotalMs = 100.0,
        };
        Assert.Equal(10.0, timing.PerIterationMs);
    }
}
