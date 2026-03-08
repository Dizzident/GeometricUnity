using System.Text.Json.Serialization;

namespace Gu.Artifacts;

/// <summary>
/// Result of a replay validation (Section 20.4).
/// </summary>
public sealed class ReplayReport
{
    /// <summary>Replay outcome: Pass, Fail, or Invalid.</summary>
    [JsonPropertyName("outcome")]
    public required ReplayOutcome Outcome { get; init; }

    /// <summary>Replay tier that was validated.</summary>
    [JsonPropertyName("replayTier")]
    public required string ReplayTier { get; init; }

    /// <summary>Individual check results.</summary>
    [JsonPropertyName("checks")]
    public required IReadOnlyList<ReplayCheck> Checks { get; init; }

    /// <summary>Timestamp of the replay validation.</summary>
    [JsonPropertyName("validatedAt")]
    public required DateTimeOffset ValidatedAt { get; init; }
}

/// <summary>
/// Outcome of a replay validation.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReplayOutcome
{
    Pass,
    Fail,
    Invalid,
}

/// <summary>
/// Individual check within a replay validation.
/// </summary>
public sealed class ReplayCheck
{
    /// <summary>Name of the check.</summary>
    [JsonPropertyName("checkName")]
    public required string CheckName { get; init; }

    /// <summary>Whether the check passed.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Detail message.</summary>
    [JsonPropertyName("detail")]
    public string? Detail { get; init; }
}
