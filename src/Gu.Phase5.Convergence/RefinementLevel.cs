using System.Text.Json.Serialization;

namespace Gu.Phase5.Convergence;

/// <summary>
/// A single refinement level in a convergence study (M47).
/// Identified by levelId and characterized by mesh parameter h.
/// </summary>
public sealed class RefinementLevel
{
    /// <summary>Unique level identifier (e.g., "level-0", "level-1").</summary>
    [JsonPropertyName("levelId")]
    public required string LevelId { get; init; }

    /// <summary>
    /// Mesh parameter h for this level (e.g., maximum edge length).
    /// Smaller h = finer mesh.
    /// </summary>
    [JsonPropertyName("meshParameter")]
    public required double MeshParameter { get; init; }

    /// <summary>Human-readable description of this level.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
