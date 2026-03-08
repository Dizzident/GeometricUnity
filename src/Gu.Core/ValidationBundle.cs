using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// A bundle of validation records for a run.
/// </summary>
public sealed class ValidationBundle
{
    /// <summary>Branch reference this validation applies to.</summary>
    [JsonPropertyName("branch")]
    public required BranchRef Branch { get; init; }

    /// <summary>All validation records.</summary>
    [JsonPropertyName("records")]
    public required IReadOnlyList<ValidationRecord> Records { get; init; }

    /// <summary>Whether all records passed.</summary>
    [JsonPropertyName("allPassed")]
    public required bool AllPassed { get; init; }
}
