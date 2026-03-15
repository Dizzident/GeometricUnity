using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Documents the scope and boundary of the Shiab operator family exercised in a Phase XI
/// evidence campaign.
///
/// Per D-P11-009: the current runtime relies on `identity-shiab` in the standard path and
/// `first-order-curvature` as the paired comparison branch. Neither is the canonical draft
/// operator. This record preserves that boundary explicitly in the executed artifacts.
///
/// The draft (Section 8) explicitly leaves Shiab as an underdetermined family. This record
/// states what was exercised, what was NOT exercised, and the artifact-backed reason why
/// expansion beyond the current pair is blocked in repository context.
/// </summary>
public sealed class ShiabFamilyScopeRecord
{
    /// <summary>Unique record identifier.</summary>
    [JsonPropertyName("recordId")]
    public required string RecordId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>
    /// IDs of the Shiab branches exercised in the standard campaign path.
    /// Typically ["identity-shiab"].
    /// </summary>
    [JsonPropertyName("standardPathShiabIds")]
    public required IReadOnlyList<string> StandardPathShiabIds { get; init; }

    /// <summary>
    /// IDs of the Shiab branches exercised in the paired companion path.
    /// Typically ["first-order-curvature"].
    /// </summary>
    [JsonPropertyName("pairedPathShiabIds")]
    public required IReadOnlyList<string> PairedPathShiabIds { get; init; }

    /// <summary>
    /// Whether the exercised Shiab operators are mathematically distinct.
    /// False when both standard and paired paths implement S=F (identity on curvature).
    /// </summary>
    [JsonPropertyName("operatorsAreMathematicallyDistinct")]
    public required bool OperatorsAreMathematicallyDistinct { get; init; }

    /// <summary>
    /// Overall branch result for the standard Shiab path.
    /// Must be "mixed", "robust", or "not-evaluated".
    /// </summary>
    [JsonPropertyName("standardPathBranchResult")]
    public required string StandardPathBranchResult { get; init; }

    /// <summary>
    /// Overall branch result for the paired Shiab path.
    /// Must be "mixed", "robust", or "not-evaluated".
    /// </summary>
    [JsonPropertyName("pairedPathBranchResult")]
    public required string PairedPathBranchResult { get; init; }

    /// <summary>
    /// Whether the paired Shiab path changes any project-level conclusion
    /// relative to the standard path.
    /// </summary>
    [JsonPropertyName("pairedPathChangesConclusion")]
    public required bool PairedPathChangesConclusion { get; init; }

    /// <summary>
    /// Artifact-backed reason why Shiab family expansion is blocked in the current
    /// repository context. Must cite specific artifact paths or executed study results.
    /// Null only if expansion was actually executed.
    /// </summary>
    [JsonPropertyName("expansionBlockedReason")]
    public string? ExpansionBlockedReason { get; init; }

    /// <summary>
    /// Paths to the artifact files that support the expansion-blocked claim.
    /// E.g. the branch robustness records showing mixed result for both paths.
    /// </summary>
    [JsonPropertyName("supportingArtifactPaths")]
    public IReadOnlyList<string>? SupportingArtifactPaths { get; init; }

    /// <summary>
    /// Explicit statement required by D-P11-009: neither operator is the canonically
    /// selected draft operator. Must begin with "FAMILY-OPEN:".
    /// </summary>
    [JsonPropertyName("familyOpenStatement")]
    public required string FamilyOpenStatement { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
