using System.Text.Json.Serialization;

namespace Gu.Phase2.Semantics;

/// <summary>
/// A single piece of evidence relevant to a canonicity docket.
/// </summary>
public sealed class CanonicityEvidenceRecord
{
    /// <summary>Unique evidence record identifier.</summary>
    [JsonPropertyName("evidenceId")]
    public required string EvidenceId { get; init; }

    /// <summary>Study that produced this evidence.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Verdict: "consistent", "inconsistent", "inconclusive".</summary>
    [JsonPropertyName("verdict")]
    public required string Verdict { get; init; }

    /// <summary>Maximum observed deviation across compared branches.</summary>
    [JsonPropertyName("maxObservedDeviation")]
    public required double MaxObservedDeviation { get; init; }

    /// <summary>Tolerance used for the comparison.</summary>
    [JsonPropertyName("tolerance")]
    public required double Tolerance { get; init; }

    /// <summary>Timestamp when the evidence was produced.</summary>
    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }
}
