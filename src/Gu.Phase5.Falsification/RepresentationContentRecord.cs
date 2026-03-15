using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Records a structural representation content check for a candidate (M50).
/// Used to trigger RepresentationContent falsifiers when mode counts or structure
/// are inconsistent with expected representation content.
/// </summary>
public sealed class RepresentationContentRecord
{
    /// <summary>Identifier for this record.</summary>
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    /// <summary>Candidate or particle ID being checked.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Expected number of modes for this representation.</summary>
    [JsonPropertyName("expectedModeCount")]
    public required int ExpectedModeCount { get; init; }

    /// <summary>Observed number of modes in the spectrum.</summary>
    [JsonPropertyName("observedModeCount")]
    public required int ObservedModeCount { get; init; }

    /// <summary>
    /// Number of required representation content items that are absent.
    /// Triggers a Fatal falsifier when > 0 (WP-7).
    /// </summary>
    [JsonPropertyName("missingRequiredCount")]
    public int MissingRequiredCount { get; init; }

    /// <summary>
    /// Structural mismatch score (0–1). Triggers a High falsifier when
    /// &gt; RepresentationContentThreshold (WP-7).
    /// </summary>
    [JsonPropertyName("structuralMismatchScore")]
    public double StructuralMismatchScore { get; init; }

    /// <summary>Whether the mode count is consistent with the expected representation.</summary>
    [JsonPropertyName("consistent")]
    public required bool Consistent { get; init; }

    /// <summary>Optional description of the inconsistency.</summary>
    [JsonPropertyName("inconsistencyDescription")]
    public string? InconsistencyDescription { get; init; }

    /// <summary>Provenance of the backend that produced this record.</summary>
    [JsonPropertyName("provenance")]
    public ProvenanceMeta? Provenance { get; init; }
}
