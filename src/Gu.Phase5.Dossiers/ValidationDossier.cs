using System.Text.Json.Serialization;
using Gu.Artifacts;
using Gu.Core;

namespace Gu.Phase5.Dossiers;

/// <summary>
/// The top-level validation dossier for a Phase V study.
/// Aggregates evidence from multiple study contributions and records
/// whether each piece of evidence is trustworthy (regenerated from current
/// code) or stale (checked-in artifact).
///
/// G-006: any study output cited as Phase V evidence must be regenerated from
/// the current code path and tied to a reproducible command sequence.
/// Stale checked-in artifacts must not be treated as validation proof.
/// </summary>
public sealed class ValidationDossier
{
    /// <summary>Unique identifier for this dossier.</summary>
    [JsonPropertyName("dossierId")]
    public required string DossierId { get; init; }

    /// <summary>Human-readable title for the study this dossier covers.</summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>
    /// All study contributions included in this dossier, in assembly order.
    /// </summary>
    [JsonPropertyName("studies")]
    public required IReadOnlyList<StudyManifest> Studies { get; init; }

    /// <summary>
    /// Overall evidence tier: the minimum tier across all study contributions.
    /// A dossier is only as trustworthy as its weakest evidence source.
    /// </summary>
    [JsonPropertyName("overallEvidenceTier")]
    public required ArtifactEvidenceTier OverallEvidenceTier { get; init; }

    /// <summary>
    /// Human-readable verdict string summarising whether this dossier provides
    /// genuine Phase V validation evidence.
    /// </summary>
    [JsonPropertyName("evidenceVerdict")]
    public required string EvidenceVerdict { get; init; }

    /// <summary>
    /// True if all studies have been regenerated from the current code and are
    /// acceptable as Phase V validation evidence.
    /// </summary>
    [JsonPropertyName("isAcceptableAsEvidence")]
    public required bool IsAcceptableAsEvidence { get; init; }

    /// <summary>
    /// List of study IDs that have stale or missing reproducibility bundles.
    /// These cannot be cited as Phase V evidence.
    /// </summary>
    [JsonPropertyName("staleStudyIds")]
    public required IReadOnlyList<string> StaleStudyIds { get; init; }

    /// <summary>
    /// Optional: notes from the assembler about what was found and what was rejected.
    /// </summary>
    [JsonPropertyName("assemblyNotes")]
    public IReadOnlyList<string>? AssemblyNotes { get; init; }

    /// <summary>When this dossier was assembled.</summary>
    [JsonPropertyName("assembledAt")]
    public required DateTimeOffset AssembledAt { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public ProvenanceMeta? Provenance { get; init; }
}
