using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Records the Phase XI P11-M5 examination outcome for the active representation-content
/// fatal falsifier. Produced by the campaign runner when a representation-content fatal
/// is active and has been examined per P11-M5.
///
/// Status values:
///   "preserved-as-blocker" — the fatal was examined and cannot be closed in the current
///                             repository context. It is preserved as an explicit unresolved
///                             scientific limitation per D-P11-004.
///   "resolved"             — the fatal was closed by stronger candidate content (not the
///                             current Phase XI outcome).
/// </summary>
public sealed class RepresentationContentStabilizationRecord
{
    /// <summary>Unique identifier for this stabilization record.</summary>
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Stabilization status:
    /// "preserved-as-blocker" or "resolved".
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>
    /// The candidate ID that carries the active fatal representation-content falsifier.
    /// </summary>
    [JsonPropertyName("candidateId")]
    public required string CandidateId { get; init; }

    /// <summary>
    /// The falsifier ID from the active FalsifierSummary.
    /// </summary>
    [JsonPropertyName("falsifierId")]
    public required string FalsifierId { get; init; }

    /// <summary>
    /// Human-readable reason why the fatal cannot be closed in the current repository context.
    /// Required when status = "preserved-as-blocker".
    /// </summary>
    [JsonPropertyName("blockerReason")]
    public required string BlockerReason { get; init; }

    /// <summary>
    /// Artifact references that were searched and found to not provide closing evidence.
    /// These are the same refs as the RepresentationContentRecord.SourceArtifactRefs.
    /// </summary>
    [JsonPropertyName("searchedArtifactRefs")]
    public IReadOnlyList<string>? SearchedArtifactRefs { get; init; }

    /// <summary>
    /// What it would take to close this blocker in a future phase.
    /// </summary>
    [JsonPropertyName("closureRequirement")]
    public string? ClosureRequirement { get; init; }

    /// <summary>Binding decision that mandates preservation of this fatal.</summary>
    [JsonPropertyName("bindingDecision")]
    public string BindingDecision { get; init; } = "D-P11-004";

    /// <summary>Provenance of this stabilization record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
