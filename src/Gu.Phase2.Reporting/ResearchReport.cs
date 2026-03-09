using System.Text.Json.Serialization;
using Gu.Phase2.Canonicity;

namespace Gu.Phase2.Reporting;

/// <summary>
/// A research report generated from a batch run.
/// Per IMPLEMENTATION_PLAN_P2.md Section 14.4, every report must explicitly state:
/// - what is branch-local,
/// - what is comparison-ready,
/// - what remains open,
/// - what is numerical only,
/// - what is physically uninterpreted,
/// - what is ruled out by the study.
///
/// The reporting layer must make overclaiming difficult.
/// </summary>
public sealed class ResearchReport
{
    /// <summary>Unique report identifier.</summary>
    [JsonPropertyName("reportId")]
    public required string ReportId { get; init; }

    /// <summary>Batch ID this report was generated from.</summary>
    [JsonPropertyName("batchId")]
    public required string BatchId { get; init; }

    /// <summary>
    /// Conclusions that hold within a single branch only.
    /// These cannot be promoted to cross-branch claims without canonicity closure.
    /// </summary>
    [JsonPropertyName("branchLocalConclusions")]
    public required IReadOnlyList<ReportItem> BranchLocalConclusions { get; init; }

    /// <summary>
    /// Conclusions that have been validated across branches and are ready
    /// for external comparison campaigns.
    /// </summary>
    [JsonPropertyName("comparisonReadyConclusions")]
    public required IReadOnlyList<ReportItem> ComparisonReadyConclusions { get; init; }

    /// <summary>
    /// Items that remain open: unresolved canonicity, incomplete sweeps,
    /// unstable continuation paths, etc.
    /// </summary>
    [JsonPropertyName("openItems")]
    public required IReadOnlyList<ReportItem> OpenItems { get; init; }

    /// <summary>
    /// Results that are numerical only: convergence achieved but no
    /// physical interpretation or theorem-level backing.
    /// </summary>
    [JsonPropertyName("numericalOnlyResults")]
    public required IReadOnlyList<ReportItem> NumericalOnlyResults { get; init; }

    /// <summary>
    /// Outputs that have not been interpreted physically:
    /// structural results without recovery-gate passage.
    /// </summary>
    [JsonPropertyName("uninterpretedOutputs")]
    public required IReadOnlyList<ReportItem> UninterpretedOutputs { get; init; }

    /// <summary>
    /// Claims that are ruled out by this study: falsified predictions,
    /// negative comparison results, counterexamples to canonicity.
    /// </summary>
    [JsonPropertyName("ruledOutClaims")]
    public required IReadOnlyList<ReportItem> RuledOutClaims { get; init; }

    /// <summary>
    /// Aggregated canonicity dockets referenced by this batch.
    /// </summary>
    [JsonPropertyName("dockets")]
    public required IReadOnlyList<CanonicityDocket> Dockets { get; init; }

    /// <summary>Timestamp when this report was generated.</summary>
    [JsonPropertyName("generatedAt")]
    public required DateTimeOffset GeneratedAt { get; init; }
}

/// <summary>
/// A single item in a research report category.
/// </summary>
public sealed class ReportItem
{
    /// <summary>Short summary of the item.</summary>
    [JsonPropertyName("summary")]
    public required string Summary { get; init; }

    /// <summary>Category: "sweep", "stability", "comparison", "canonicity".</summary>
    [JsonPropertyName("sourceCategory")]
    public required string SourceCategory { get; init; }

    /// <summary>Study ID that produced this item.</summary>
    [JsonPropertyName("sourceStudyId")]
    public required string SourceStudyId { get; init; }

    /// <summary>Branch manifest ID, if branch-specific.</summary>
    [JsonPropertyName("branchManifestId")]
    public string? BranchManifestId { get; init; }

    /// <summary>Evidence strength: "strong", "moderate", "weak", "numerical-only".</summary>
    [JsonPropertyName("evidenceStrength")]
    public required string EvidenceStrength { get; init; }
}
