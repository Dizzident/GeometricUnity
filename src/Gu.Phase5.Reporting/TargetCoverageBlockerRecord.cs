using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Explicit blocker or waiver for an external target that is intentionally not
/// covered by the current computed observable artifact set.
/// </summary>
public sealed class TargetCoverageBlockerRecord
{
    /// <summary>Stable blocker identifier.</summary>
    [JsonPropertyName("blockerId")]
    public required string BlockerId { get; init; }

    /// <summary>Observable identifier requested by the blocked target.</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Optional target label for a narrower match.</summary>
    [JsonPropertyName("targetLabel")]
    public string? TargetLabel { get; init; }

    /// <summary>Optional target environment ID selector.</summary>
    [JsonPropertyName("targetEnvironmentId")]
    public string? TargetEnvironmentId { get; init; }

    /// <summary>Optional target environment tier selector.</summary>
    [JsonPropertyName("targetEnvironmentTier")]
    public string? TargetEnvironmentTier { get; init; }

    /// <summary>Why the target cannot be covered in the current campaign.</summary>
    [JsonPropertyName("blockerReason")]
    public required string BlockerReason { get; init; }

    /// <summary>What must be done to close this blocker.</summary>
    [JsonPropertyName("closureRequirement")]
    public required string ClosureRequirement { get; init; }
}

/// <summary>
/// Table of explicit target coverage blockers for a Phase V campaign.
/// </summary>
public sealed class TargetCoverageBlockerTable
{
    /// <summary>Stable table identifier.</summary>
    [JsonPropertyName("tableId")]
    public required string TableId { get; init; }

    /// <summary>Blocker records.</summary>
    [JsonPropertyName("blockers")]
    public required IReadOnlyList<TargetCoverageBlockerRecord> Blockers { get; init; }
}
