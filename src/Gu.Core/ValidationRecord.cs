using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// A single validation record capturing a test result.
/// </summary>
public sealed class ValidationRecord
{
    /// <summary>Validation rule identifier.</summary>
    [JsonPropertyName("ruleId")]
    public required string RuleId { get; init; }

    /// <summary>Category (e.g., "parity", "convergence", "gauge").</summary>
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>Whether the validation passed.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Measured value.</summary>
    [JsonPropertyName("measuredValue")]
    public double? MeasuredValue { get; init; }

    /// <summary>Tolerance or threshold used.</summary>
    [JsonPropertyName("tolerance")]
    public double? Tolerance { get; init; }

    /// <summary>Detail message.</summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }

    /// <summary>Timestamp of the check.</summary>
    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }
}
