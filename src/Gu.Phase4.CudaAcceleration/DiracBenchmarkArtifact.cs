using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.CudaAcceleration;

/// <summary>
/// Timing record for a single Dirac kernel operation benchmark.
/// </summary>
public sealed class DiracOperationBenchmark
{
    [JsonPropertyName("operationName")]
    public required string OperationName { get; init; }

    [JsonPropertyName("totalDof")]
    public required int TotalDof { get; init; }

    [JsonPropertyName("numTrials")]
    public required int NumTrials { get; init; }

    [JsonPropertyName("cpuMeanMs")]
    public required double CpuMeanMs { get; init; }

    [JsonPropertyName("gpuMeanMs")]
    public required double GpuMeanMs { get; init; }

    /// <summary>Speedup ratio: cpuMeanMs / gpuMeanMs. For stubs this will be ~1.</summary>
    [JsonPropertyName("speedupRatio")]
    public double SpeedupRatio => GpuMeanMs > 1e-12 ? CpuMeanMs / GpuMeanMs : 1.0;
}

/// <summary>
/// Benchmark artifact for Phase IV CUDA acceleration (M44).
///
/// Records timing measurements for all Dirac operator actions.
/// For the GPU stub, speedup ~ 1 (same code). Real GPU measurements
/// would show speedup > 1 for large systems.
/// </summary>
public sealed class DiracBenchmarkArtifact
{
    [JsonPropertyName("artifactId")]
    public required string ArtifactId { get; init; }

    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    [JsonPropertyName("totalDof")]
    public required int TotalDof { get; init; }

    [JsonPropertyName("gpuVerificationStatus")]
    public required string GpuVerificationStatus { get; init; }

    [JsonPropertyName("operationBenchmarks")]
    public required List<DiracOperationBenchmark> OperationBenchmarks { get; init; }

    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

/// <summary>
/// Runner for DiracBenchmarkArtifact production.
/// </summary>
public static class DiracBenchmarkRunner
{
    /// <summary>
    /// Run timing benchmarks for all IDiracKernel operations.
    /// </summary>
    /// <param name="numTrials">Number of kernel invocations per operation.</param>
    public static DiracBenchmarkArtifact Run(
        IDiracKernel cpu,
        IDiracKernel gpu,
        string artifactId,
        int numTrials,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(cpu);
        ArgumentNullException.ThrowIfNull(gpu);
        ArgumentNullException.ThrowIfNull(provenance);

        if (numTrials < 1) numTrials = 1;
        int n = cpu.TotalDof;

        var rng = new Random(99);
        double[] psi = new double[n];
        double[] result = new double[n];
        for (int i = 0; i < n; i++) psi[i] = rng.NextDouble();

        var benchmarks = new List<DiracOperationBenchmark>
        {
            BenchmarkOp("ApplyDirac", n, numTrials, psi, result,
                (p, r) => cpu.ApplyDirac(p, r),
                (p, r) => gpu.ApplyDirac(p, r)),

            BenchmarkOp("ApplyMassPsi", n, numTrials, psi, result,
                (p, r) => cpu.ApplyMassPsi(p, r),
                (p, r) => gpu.ApplyMassPsi(p, r)),

            BenchmarkOp("ProjectLeft", n, numTrials, psi, result,
                (p, r) => cpu.ProjectLeft(p, r),
                (p, r) => gpu.ProjectLeft(p, r)),

            BenchmarkOp("ProjectRight", n, numTrials, psi, result,
                (p, r) => cpu.ProjectRight(p, r),
                (p, r) => gpu.ProjectRight(p, r)),
        };

        string gpuStatus = gpu is GpuDiracKernelStub stub
            ? stub.VerificationStatus
            : "production";

        return new DiracBenchmarkArtifact
        {
            ArtifactId = artifactId,
            TotalDof = n,
            GpuVerificationStatus = gpuStatus,
            OperationBenchmarks = benchmarks,
            Provenance = provenance,
        };
    }

    private static DiracOperationBenchmark BenchmarkOp(
        string name,
        int n,
        int trials,
        double[] psi,
        double[] result,
        Action<ReadOnlySpan<double>, Span<double>> cpuOp,
        Action<ReadOnlySpan<double>, Span<double>> gpuOp)
    {
        // Warm up
        cpuOp(psi, result);
        gpuOp(psi, result);

        var sw = System.Diagnostics.Stopwatch.StartNew();
        for (int i = 0; i < trials; i++) cpuOp(psi, result);
        sw.Stop();
        double cpuMs = sw.Elapsed.TotalMilliseconds / trials;

        sw.Restart();
        for (int i = 0; i < trials; i++) gpuOp(psi, result);
        sw.Stop();
        double gpuMs = sw.Elapsed.TotalMilliseconds / trials;

        return new DiracOperationBenchmark
        {
            OperationName = name,
            TotalDof = n,
            NumTrials = trials,
            CpuMeanMs = cpuMs,
            GpuMeanMs = gpuMs,
        };
    }
}
