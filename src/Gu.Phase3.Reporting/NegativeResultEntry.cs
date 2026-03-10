using System.Text.Json.Serialization;

namespace Gu.Phase3.Reporting;

/// <summary>
/// Records a negative or null result: something that was looked for but not found,
/// or a hypothesis that was tested and falsified.
///
/// Negative results are first-class outputs of the research platform.
/// </summary>
public sealed class NegativeResultEntry
{
    /// <summary>Human-readable description of the negative result.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>Evidence supporting the negative conclusion.</summary>
    [JsonPropertyName("evidence")]
    public required string Evidence { get; init; }

    /// <summary>Impact or implication of the negative result.</summary>
    [JsonPropertyName("impact")]
    public required string Impact { get; init; }

    /// <summary>Category of negative result (e.g. "no-match", "instability", "gauge-artifact").</summary>
    [JsonPropertyName("category")]
    public string? Category { get; init; }
}
