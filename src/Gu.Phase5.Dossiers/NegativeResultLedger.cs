using System.Text.Json.Serialization;
using Gu.Artifacts;
using Gu.Core;

namespace Gu.Phase5.Dossiers;

/// <summary>
/// A ledger of negative results from Phase V studies (M52).
///
/// Phase V engineering rule §12.4: negative results must be preserved as
/// first-class artifacts. A failed continuum story, a fragile branch,
/// or a falsified candidate is still a scientific result and must be
/// recorded — not discarded.
///
/// The ledger accumulates NegativeResultEntry records. Each entry explains
/// what failed, why it is a genuine negative result (not just missing data),
/// and what the downstream implications are for the candidate registry.
/// </summary>
public sealed class NegativeResultLedger
{
    /// <summary>Unique ledger identifier.</summary>
    [JsonPropertyName("ledgerId")]
    public required string LedgerId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Human-readable title for this ledger.</summary>
    [JsonPropertyName("title")]
    public required string Title { get; init; }

    /// <summary>All negative result entries in this ledger.</summary>
    [JsonPropertyName("entries")]
    public required List<NegativeResultEntry> Entries { get; init; }

    /// <summary>When this ledger was last updated.</summary>
    [JsonPropertyName("updatedAt")]
    public required DateTimeOffset UpdatedAt { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public ProvenanceMeta? Provenance { get; init; }

    /// <summary>Total number of entries.</summary>
    [JsonIgnore]
    public int Count => Entries.Count;
}

/// <summary>
/// A single negative result in a NegativeResultLedger.
/// </summary>
public sealed class NegativeResultEntry
{
    /// <summary>Unique entry identifier.</summary>
    [JsonPropertyName("entryId")]
    public required string EntryId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Category of negative result:
    /// "branch-fragility"           — quantity varies too much across branch family,
    /// "non-convergence"            — refinement sweep does not converge,
    /// "observation-instability"    — observation chain is branch-sensitive,
    /// "environment-instability"    — result sensitive to environment choice,
    /// "quantitative-mismatch"      — numerical result outside declared tolerance,
    /// "representation-content"     — representation content inconsistent with target,
    /// "coupling-inconsistency"     — coupling matrix inconsistent across variants,
    /// "no-mode-found"              — expected mode absent from spectrum,
    /// "study-stale"                — study output is stale and cannot be cited.
    /// </summary>
    [JsonPropertyName("category")]
    public required string Category { get; init; }

    /// <summary>Human-readable description of what failed.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>
    /// IDs of affected candidates in the boson or fermion registry.
    /// Empty if no registry candidate is directly affected.
    /// </summary>
    [JsonPropertyName("affectedCandidateIds")]
    public List<string> AffectedCandidateIds { get; init; } = new();

    /// <summary>
    /// The study ID that produced this negative result.
    /// </summary>
    [JsonPropertyName("sourceStudyId")]
    public required string SourceStudyId { get; init; }

    /// <summary>
    /// The branch variant ID (if branch-specific) or null if branch-agnostic.
    /// </summary>
    [JsonPropertyName("branchVariantId")]
    public string? BranchVariantId { get; init; }

    /// <summary>
    /// Quantitative evidence (e.g. fragility score, convergence rate) supporting this entry.
    /// </summary>
    [JsonPropertyName("quantitativeEvidence")]
    public string? QuantitativeEvidence { get; init; }

    /// <summary>
    /// Whether this negative result implies a demotion of one or more candidates.
    /// </summary>
    [JsonPropertyName("impliesDemotion")]
    public bool ImpliesDemotion { get; init; }

    /// <summary>
    /// Downstream action recommended:
    /// "demote-candidate", "flag-for-review", "archive", "no-action", "retry-with-different-env".
    /// </summary>
    [JsonPropertyName("recommendedAction")]
    public required string RecommendedAction { get; init; }

    /// <summary>Evidence tier at which this result was recorded.</summary>
    [JsonPropertyName("evidenceTier")]
    public ArtifactEvidenceTier EvidenceTier { get; init; } = ArtifactEvidenceTier.RegeneratedCpu;

    /// <summary>When this entry was recorded.</summary>
    [JsonPropertyName("recordedAt")]
    public required DateTimeOffset RecordedAt { get; init; }
}
