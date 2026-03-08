using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// The observed state: X-space quantities extracted through sigma_h* (Section 10.6).
/// </summary>
public sealed class ObservedState
{
    /// <summary>Observation branch identifier.</summary>
    [JsonPropertyName("observationBranchId")]
    public required string ObservationBranchId { get; init; }

    /// <summary>Map of observable ID to snapshot.</summary>
    [JsonPropertyName("observables")]
    public required IReadOnlyDictionary<string, ObservableSnapshot> Observables { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
