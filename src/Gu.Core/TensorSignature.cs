using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Mandatory metadata for every tensor-like object, per Section 11.1.
/// Prevents silent changes in component order between CPU and GPU.
/// </summary>
public sealed class TensorSignature
{
    /// <summary>Ambient space this tensor lives in.</summary>
    [JsonPropertyName("ambientSpaceId")]
    public required string AmbientSpaceId { get; init; }

    /// <summary>Carrier type (e.g., "connection-1form", "curvature-2form", "residual-2form").</summary>
    [JsonPropertyName("carrierType")]
    public required string CarrierType { get; init; }

    /// <summary>Differential form degree (0, 1, 2, or "mixed").</summary>
    [JsonPropertyName("degree")]
    public required string Degree { get; init; }

    /// <summary>Lie algebra basis identifier.</summary>
    [JsonPropertyName("lieAlgebraBasisId")]
    public required string LieAlgebraBasisId { get; init; }

    /// <summary>Component ordering identifier.</summary>
    [JsonPropertyName("componentOrderId")]
    public required string ComponentOrderId { get; init; }

    /// <summary>Numeric precision (e.g., "float64", "float32").</summary>
    [JsonPropertyName("numericPrecision")]
    public string NumericPrecision { get; init; } = "float64";

    /// <summary>Memory layout descriptor (e.g., "dense-row-major", "sparse-csr").</summary>
    [JsonPropertyName("memoryLayout")]
    public required string MemoryLayout { get; init; }

    /// <summary>Backend packing descriptor (e.g., "SoA", "AoS", "interleaved").</summary>
    [JsonPropertyName("backendPacking")]
    public string? BackendPacking { get; init; }
}
