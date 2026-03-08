using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// An inserted assumption (IA) or inserted choice (IX) that narrows the manuscript
/// to a computationally admissible branch.
/// </summary>
public sealed class InsertedAssumption
{
    /// <summary>Unique identifier (e.g., "IA-1", "IX-3").</summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>Short title.</summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>Full description of what this assumption/choice fixes.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Category: "assumption" or "choice".</summary>
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>Section of the implementation plan that defines this.</summary>
    [JsonPropertyName("planSection")]
    public required string PlanSection { get; init; }
}
