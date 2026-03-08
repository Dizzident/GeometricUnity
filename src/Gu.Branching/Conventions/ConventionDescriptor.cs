using System.Text.Json.Serialization;

namespace Gu.Branching.Conventions;

/// <summary>
/// Describes a registered convention (basis, ordering, adjoint, pairing, or norm).
/// </summary>
public sealed class ConventionDescriptor
{
    /// <summary>Unique convention identifier.</summary>
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    /// <summary>Convention category (e.g., "basis", "componentOrder", "adjoint", "pairing", "norm").</summary>
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>Human-readable description of the convention.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Optional parameters for the convention.</summary>
    [JsonPropertyName("parameters")]
    public IReadOnlyDictionary<string, string>? Parameters { get; init; }
}
