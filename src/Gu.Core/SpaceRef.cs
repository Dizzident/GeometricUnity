using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Reference to a discrete space (X_h or Y_h) within the geometry context.
/// </summary>
public sealed class SpaceRef
{
    /// <summary>Unique identifier for this space.</summary>
    [JsonPropertyName("spaceId")]
    public required string SpaceId { get; init; }

    /// <summary>Dimension of the space.</summary>
    [JsonPropertyName("dimension")]
    public required int Dimension { get; init; }

    /// <summary>Human-readable label (e.g., "base_X_h", "ambient_Y_h").</summary>
    [JsonPropertyName("label")]
    public string? Label { get; init; }
}
