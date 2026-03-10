using System.Text.Json.Serialization;

namespace Gu.Phase3.ModeTracking;

/// <summary>
/// Records the alignment (matching) between a mode in context A and a mode in context B.
/// </summary>
public sealed class ModeAlignmentRecord
{
    /// <summary>Mode ID in context A (source).</summary>
    [JsonPropertyName("sourceModeId")]
    public required string SourceModeId { get; init; }

    /// <summary>Mode ID in context B (target).</summary>
    [JsonPropertyName("targetModeId")]
    public required string TargetModeId { get; init; }

    /// <summary>Matching metrics between the two modes.</summary>
    [JsonPropertyName("metrics")]
    public required ModeMatchMetricSet Metrics { get; init; }

    /// <summary>
    /// Alignment type: "matched", "birth", "death", "split", "merge", "avoided-crossing".
    /// </summary>
    [JsonPropertyName("alignmentType")]
    public required string AlignmentType { get; init; }

    /// <summary>
    /// Confidence level [0, 1]. Low confidence indicates ambiguity.
    /// </summary>
    [JsonPropertyName("confidence")]
    public required double Confidence { get; init; }

    /// <summary>Ambiguity notes (if matching was ambiguous).</summary>
    [JsonPropertyName("ambiguityNotes")]
    public string? AmbiguityNotes { get; init; }
}
