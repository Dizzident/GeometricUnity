using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Records coupling proxy consistency across branch variants for a candidate (M50).
/// Used to trigger CouplingInconsistency falsifiers when coupling proxies vary excessively
/// across branch variants.
/// </summary>
public sealed class CouplingConsistencyRecord
{
    /// <summary>Identifier for this record.</summary>
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    /// <summary>Candidate or particle ID being checked.</summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>Coupling type or label (e.g., "gauge", "yukawa").</summary>
    [JsonPropertyName("couplingType")]
    public required string CouplingType { get; init; }

    /// <summary>
    /// Relative spread of the coupling proxy across branch variants
    /// (max - min) / mean, or analogous normalized measure).
    /// </summary>
    [JsonPropertyName("relativeSpread")]
    public required double RelativeSpread { get; init; }

    /// <summary>Whether the coupling is consistent (spread below policy threshold).</summary>
    [JsonPropertyName("consistent")]
    public required bool Consistent { get; init; }

    /// <summary>Optional description.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    /// <summary>Provenance of the backend that produced this record.</summary>
    [JsonPropertyName("provenance")]
    public ProvenanceMeta? Provenance { get; init; }
}
