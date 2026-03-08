using System.Text.Json.Serialization;

namespace Gu.Benchmarks;

/// <summary>
/// Records timing and parity results for a single benchmark run.
/// Produced as an artifact for post-hoc analysis.
/// </summary>
public sealed class BenchmarkReport
{
    [JsonPropertyName("benchmarkId")]
    public required string BenchmarkId { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("backendId")]
    public required string BackendId { get; init; }

    [JsonPropertyName("problemSize")]
    public required int ProblemSize { get; init; }

    [JsonPropertyName("iterations")]
    public required int Iterations { get; init; }

    [JsonPropertyName("totalTimeMs")]
    public required double TotalTimeMs { get; init; }

    [JsonPropertyName("perIterationTimeMs")]
    public required double PerIterationTimeMs { get; init; }

    [JsonPropertyName("finalObjective")]
    public required double FinalObjective { get; init; }

    [JsonPropertyName("converged")]
    public required bool Converged { get; init; }

    [JsonPropertyName("terminationReason")]
    public required string TerminationReason { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
