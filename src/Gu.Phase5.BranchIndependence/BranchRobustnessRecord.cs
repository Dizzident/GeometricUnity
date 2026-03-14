using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.BranchIndependence;

/// <summary>
/// Result record for a branch-robustness study (M46).
///
/// Aggregates the pairwise distance matrix, equivalence classes,
/// fragility scores, and invariance candidates for one study.
/// </summary>
public sealed class BranchRobustnessRecord
{
    /// <summary>Unique record identifier.</summary>
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Study spec that produced this record.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Branch variant IDs evaluated.</summary>
    [JsonPropertyName("branchVariantIds")]
    public required List<string> BranchVariantIds { get; init; }

    /// <summary>
    /// Pairwise distance matrices, one per target quantity.
    /// Key = target quantity ID.
    /// </summary>
    [JsonPropertyName("distanceMatrices")]
    public required Dictionary<string, BranchDistanceMatrix> DistanceMatrices { get; init; }

    /// <summary>
    /// Equivalence classes for each target quantity under the declared tolerances.
    /// Key = target quantity ID.
    /// </summary>
    [JsonPropertyName("equivalenceClasses")]
    public required Dictionary<string, List<BranchEquivalenceClass>> EquivalenceClasses { get; init; }

    /// <summary>
    /// Fragility records, one per target quantity.
    /// Key = target quantity ID.
    /// </summary>
    [JsonPropertyName("fragilityRecords")]
    public required Dictionary<string, FragilityRecord> FragilityRecords { get; init; }

    /// <summary>
    /// Invariance candidates identified across all quantities.
    /// </summary>
    [JsonPropertyName("invarianceCandidates")]
    public required List<InvarianceCandidateRecord> InvarianceCandidates { get; init; }

    /// <summary>
    /// Overall study summary: "robust", "fragile", "mixed", "inconclusive".
    /// "robust"      — all quantities are branch-invariant within tolerance.
    /// "fragile"     — at least one quantity is branch-sensitive.
    /// "mixed"       — some quantities invariant, some fragile.
    /// "inconclusive"— fewer than 2 variants evaluated.
    /// </summary>
    [JsonPropertyName("overallSummary")]
    public required string OverallSummary { get; init; }

    /// <summary>Diagnostic notes from the study runner.</summary>
    [JsonPropertyName("diagnosticNotes")]
    public List<string> DiagnosticNotes { get; init; } = new();

    /// <summary>Provenance of this record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
