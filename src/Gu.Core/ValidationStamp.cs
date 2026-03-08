using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// A stamp indicating that a particular validation check has been performed.
/// </summary>
public sealed class ValidationStamp
{
    /// <summary>Identifier of the validation rule.</summary>
    [JsonPropertyName("ruleId")]
    public required string RuleId { get; init; }

    /// <summary>Whether the validation passed.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Timestamp of the check.</summary>
    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>Optional detail message.</summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }
}
