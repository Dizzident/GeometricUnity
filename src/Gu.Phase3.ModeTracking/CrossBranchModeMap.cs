using System.Text.Json.Serialization;

namespace Gu.Phase3.ModeTracking;

/// <summary>
/// Maps modes across branch variant changes.
/// Records how modes from one branch variant correspond to modes in another.
/// </summary>
public sealed class CrossBranchModeMap
{
    /// <summary>Source branch manifest ID.</summary>
    [JsonPropertyName("sourceBranchId")]
    public required string SourceBranchId { get; init; }

    /// <summary>Target branch manifest ID.</summary>
    [JsonPropertyName("targetBranchId")]
    public required string TargetBranchId { get; init; }

    /// <summary>Alignment records between source and target modes.</summary>
    [JsonPropertyName("alignments")]
    public required IReadOnlyList<ModeAlignmentRecord> Alignments { get; init; }

    /// <summary>Number of matched pairs.</summary>
    [JsonPropertyName("matchedCount")]
    public int MatchedCount => Alignments.Count(a => a.AlignmentType == "matched");

    /// <summary>Number of births (new modes in target).</summary>
    [JsonPropertyName("birthCount")]
    public int BirthCount => Alignments.Count(a => a.AlignmentType == "birth");

    /// <summary>Number of deaths (modes lost from source).</summary>
    [JsonPropertyName("deathCount")]
    public int DeathCount => Alignments.Count(a => a.AlignmentType == "death");

    /// <summary>Number of ambiguous matches.</summary>
    [JsonPropertyName("ambiguousCount")]
    public int AmbiguousCount => Alignments.Count(a => a.AmbiguityNotes != null);
}
