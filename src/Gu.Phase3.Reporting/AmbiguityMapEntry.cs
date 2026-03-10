using System.Text.Json.Serialization;

namespace Gu.Phase3.Reporting;

/// <summary>
/// An entry in the ambiguity map: records where mode matching,
/// identification, or comparison was ambiguous.
/// Per Section 14.1: never hide ambiguity.
/// </summary>
public sealed class AmbiguityMapEntry
{
    /// <summary>Candidate ID with ambiguity.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Type of ambiguity: "matching", "identification", "comparison".</summary>
    [JsonPropertyName("ambiguityType")]
    public required string AmbiguityType { get; init; }

    /// <summary>Notes describing the ambiguity.</summary>
    [JsonPropertyName("notes")]
    public required IReadOnlyList<string> Notes { get; init; }

    /// <summary>Number of alternative interpretations.</summary>
    [JsonPropertyName("alternativeCount")]
    public int AlternativeCount { get; init; }
}
