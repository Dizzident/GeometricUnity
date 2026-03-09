using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Recovery;

/// <summary>
/// A typed observed output produced by the extraction pipeline (Section 12.2).
/// Wraps the Phase I ObservableSnapshot with recovery DAG provenance and claim ceiling.
/// </summary>
public sealed class ObservedOutputRecord
{
    /// <summary>Unique output record identifier.</summary>
    [JsonPropertyName("outputId")]
    public required string OutputId { get; init; }

    /// <summary>The observed output kind discriminator.</summary>
    [JsonPropertyName("kind")]
    public required ObservedOutputKind Kind { get; init; }

    /// <summary>The Phase I observable snapshot (values, signature, provenance).</summary>
    [JsonPropertyName("snapshot")]
    public required ObservableSnapshot Snapshot { get; init; }

    /// <summary>Recovery node ID that produced this output.</summary>
    [JsonPropertyName("recoveryNodeId")]
    public required string RecoveryNodeId { get; init; }

    /// <summary>Maximum claim class this output can support.</summary>
    [JsonPropertyName("claimCeiling")]
    public required ClaimClass ClaimCeiling { get; init; }
}
