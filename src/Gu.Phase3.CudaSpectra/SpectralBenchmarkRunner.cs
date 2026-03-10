using System.Diagnostics;

namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// Runs performance benchmarks on ISpectralKernel implementations.
/// Measures operator action timings and eigensolver performance.
/// </summary>
public sealed class SpectralBenchmarkRunner
{
    private readonly ISpectralKernel _kernel;
    private readonly string _backendName;

    public SpectralBenchmarkRunner(ISpectralKernel kernel, string backendName)
    {
        _kernel = kernel ?? throw new ArgumentNullException(nameof(kernel));
        _backendName = backendName;
    }

    /// <summary>
    /// Run operator action benchmarks.
    /// </summary>
    /// <param name="repetitions">Number of times to repeat each operation.</param>
    /// <param name="seed">Random seed for test vectors.</param>
    public SpectralBenchmarkArtifact RunOperatorBenchmarks(int repetitions = 100, int seed = 42)
    {
        int n = _kernel.StateDimension;
        int m = _kernel.ResidualDimension;
        var rng = new Random(seed);
        var v = RandomVector(rng, n);
        var w = RandomVector(rng, m);
        var resultN = new double[n];
        var resultM = new double[m];

        var timings = new List<SpectralBenchmarkArtifact.OperationTiming>();

        // Warmup
        _kernel.ApplySpectral(v, resultN);
        _kernel.ApplyMass(v, resultN);
        _kernel.ApplyJacobian(v, resultM);
        _kernel.ApplyAdjoint(w, resultN);

        // Benchmark ApplySpectral
        timings.Add(TimeOperation("ApplySpectral", repetitions, () =>
            _kernel.ApplySpectral(v, resultN)));

        // Benchmark ApplyMass
        timings.Add(TimeOperation("ApplyMass", repetitions, () =>
            _kernel.ApplyMass(v, resultN)));

        // Benchmark ApplyJacobian
        timings.Add(TimeOperation("ApplyJacobian", repetitions, () =>
            _kernel.ApplyJacobian(v, resultM)));

        // Benchmark ApplyAdjoint
        timings.Add(TimeOperation("ApplyAdjoint", repetitions, () =>
            _kernel.ApplyAdjoint(w, resultN)));

        return new SpectralBenchmarkArtifact
        {
            BenchmarkId = $"bench-{_backendName}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Timestamp = DateTimeOffset.UtcNow,
            StateDimension = n,
            ResidualDimension = m,
            Backend = _backendName,
            Timings = timings,
        };
    }

    /// <summary>
    /// Run eigensolver benchmark using LOBPCG.
    /// </summary>
    public SpectralBenchmarkArtifact RunEigensolverBenchmark(LobpcgConfig config)
    {
        var solver = new LobpcgSolver(_kernel);

        var sw = Stopwatch.StartNew();
        var result = solver.Solve(config);
        sw.Stop();

        return new SpectralBenchmarkArtifact
        {
            BenchmarkId = $"eigenbench-{_backendName}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            Timestamp = DateTimeOffset.UtcNow,
            StateDimension = _kernel.StateDimension,
            ResidualDimension = _kernel.ResidualDimension,
            Backend = _backendName,
            Timings = Array.Empty<SpectralBenchmarkArtifact.OperationTiming>(),
            EigensolverResult = new SpectralBenchmarkArtifact.EigensolverTiming
            {
                NumEigenvalues = config.NumEigenvalues,
                Iterations = result.Iterations,
                Converged = result.Converged,
                TotalMs = sw.Elapsed.TotalMilliseconds,
            },
        };
    }

    private static SpectralBenchmarkArtifact.OperationTiming TimeOperation(
        string name, int reps, Action action)
    {
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < reps; i++)
            action();
        sw.Stop();

        return new SpectralBenchmarkArtifact.OperationTiming
        {
            Operation = name,
            Repetitions = reps,
            TotalMs = sw.Elapsed.TotalMilliseconds,
        };
    }

    private static double[] RandomVector(Random rng, int n)
    {
        var v = new double[n];
        for (int i = 0; i < n; i++)
            v[i] = rng.NextDouble() - 0.5;
        return v;
    }
}
