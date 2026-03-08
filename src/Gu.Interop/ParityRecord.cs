using System.Text.Json.Serialization;

namespace Gu.Interop;

/// <summary>
/// Records a single CPU-vs-GPU parity comparison result.
/// Written as a first-class artifact in the parity subfolder.
/// </summary>
public sealed class ParityRecord
{
    /// <summary>Unique record identifier.</summary>
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    /// <summary>Which kernel was tested (e.g. "curvature", "torsion", "residual", "objective").</summary>
    [JsonPropertyName("kernelName")]
    public required string KernelName { get; init; }

    /// <summary>CPU backend identifier.</summary>
    [JsonPropertyName("cpuBackendId")]
    public required string CpuBackendId { get; init; }

    /// <summary>GPU backend identifier.</summary>
    [JsonPropertyName("gpuBackendId")]
    public required string GpuBackendId { get; init; }

    /// <summary>Number of elements compared.</summary>
    [JsonPropertyName("elementCount")]
    public required int ElementCount { get; init; }

    /// <summary>Maximum absolute difference across all elements.</summary>
    [JsonPropertyName("maxAbsoluteError")]
    public required double MaxAbsoluteError { get; init; }

    /// <summary>Maximum relative difference (relative to CPU result magnitude).</summary>
    [JsonPropertyName("maxRelativeError")]
    public required double MaxRelativeError { get; init; }

    /// <summary>L2 norm of the difference vector.</summary>
    [JsonPropertyName("l2ErrorNorm")]
    public required double L2ErrorNorm { get; init; }

    /// <summary>Declared tolerance for pass/fail.</summary>
    [JsonPropertyName("tolerance")]
    public required double Tolerance { get; init; }

    /// <summary>Whether the parity check passed.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Additional details or failure message.</summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>Timestamp of the comparison.</summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
