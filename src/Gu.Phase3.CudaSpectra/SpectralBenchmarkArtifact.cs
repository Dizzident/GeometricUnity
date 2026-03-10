using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Core.Serialization;

namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// Performance benchmark artifact for spectral operations.
/// Records timings for CPU vs GPU kernel actions and eigensolver runs.
/// Stored as JSON artifacts for comparison across runs.
/// </summary>
public sealed class SpectralBenchmarkArtifact
{
    /// <summary>Unique benchmark identifier.</summary>
    public required string BenchmarkId { get; init; }

    /// <summary>Timestamp of the benchmark run.</summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>Problem size: state dimension.</summary>
    public required int StateDimension { get; init; }

    /// <summary>Problem size: residual dimension.</summary>
    public required int ResidualDimension { get; init; }

    /// <summary>Backend used: "cpu" or "gpu".</summary>
    public required string Backend { get; init; }

    /// <summary>Individual operation timings.</summary>
    public required IReadOnlyList<OperationTiming> Timings { get; init; }

    /// <summary>Eigensolver timing (if applicable).</summary>
    public EigensolverTiming? EigensolverResult { get; init; }

    /// <summary>
    /// Timing for a single operator action.
    /// </summary>
    public sealed class OperationTiming
    {
        /// <summary>Operation name (ApplySpectral, ApplyMass, etc.).</summary>
        public required string Operation { get; init; }

        /// <summary>Number of repetitions.</summary>
        public required int Repetitions { get; init; }

        /// <summary>Total elapsed time in milliseconds.</summary>
        public required double TotalMs { get; init; }

        /// <summary>Average time per call in milliseconds.</summary>
        public double AverageMs => Repetitions > 0 ? TotalMs / Repetitions : 0;
    }

    /// <summary>
    /// Timing for an eigensolver run.
    /// </summary>
    public sealed class EigensolverTiming
    {
        /// <summary>Number of eigenvalues requested.</summary>
        public required int NumEigenvalues { get; init; }

        /// <summary>Number of iterations performed.</summary>
        public required int Iterations { get; init; }

        /// <summary>Whether the solver converged.</summary>
        public required bool Converged { get; init; }

        /// <summary>Total elapsed time in milliseconds.</summary>
        public required double TotalMs { get; init; }

        /// <summary>Time per iteration in milliseconds.</summary>
        public double PerIterationMs => Iterations > 0 ? TotalMs / Iterations : 0;
    }

    /// <summary>Serialize to JSON.</summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, GuJsonDefaults.Options);
    }

    /// <summary>Deserialize from JSON.</summary>
    public static SpectralBenchmarkArtifact? FromJson(string json)
    {
        return JsonSerializer.Deserialize<SpectralBenchmarkArtifact>(json, GuJsonDefaults.Options);
    }

    /// <summary>Write to a file.</summary>
    public void WriteToFile(string path)
    {
        File.WriteAllText(path, ToJson());
    }

    /// <summary>Read from a file.</summary>
    public static SpectralBenchmarkArtifact? ReadFromFile(string path)
    {
        return FromJson(File.ReadAllText(path));
    }
}
