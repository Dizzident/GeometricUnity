using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Describes a single patch in the discretized geometry.
/// </summary>
public sealed class PatchInfo
{
    /// <summary>Unique patch identifier.</summary>
    [JsonPropertyName("patchId")]
    public required string PatchId { get; init; }

    /// <summary>Number of elements (cells, vertices, etc.) in this patch.</summary>
    [JsonPropertyName("elementCount")]
    public required int ElementCount { get; init; }

    /// <summary>Patch topology type (e.g., "simplicial", "structured").</summary>
    [JsonPropertyName("topologyType")]
    public string? TopologyType { get; init; }

    /// <summary>Optional metadata about the patch.</summary>
    [JsonPropertyName("metadata")]
    public IReadOnlyDictionary<string, string>? Metadata { get; init; }
}
