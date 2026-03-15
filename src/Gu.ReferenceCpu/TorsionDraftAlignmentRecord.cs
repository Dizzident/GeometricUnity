using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.ReferenceCpu;

/// <summary>
/// Records the draft-alignment status of a torsion operator branch.
///
/// The April 1, 2021 draft (Section 7) defines augmented torsion at the group level as:
///   T_g = $ - epsilon^{-1} d_{A0} epsilon
///
/// The current executable branch instead lowers it to:
///   T^aug = d_{A0}(omega - A0)
///
/// This record distinguishes "draft-form" torsion (matching the group-level formula) from
/// "branch-local-surrogate" torsion (the current lower-level computational realization),
/// so that downstream reports and manifests can correctly characterize the evidence tier.
///
/// Per D-P11-008: the current surrogate must not be described as draft-exact until
/// equivalence is derived and tested or a draft-form branch is implemented.
/// </summary>
public sealed class TorsionDraftAlignmentRecord
{
    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Draft-alignment status of the torsion operator.
    /// One of: "draft-form", "branch-local-surrogate".
    ///
    /// "draft-form": the operator is a materially close realization of the draft's
    ///   group-level formula T_g = $ - epsilon^{-1} d_{A0} epsilon.
    ///
    /// "branch-local-surrogate": the operator is a branch-defined lowering (currently
    ///   T^aug = d_{A0}(omega - A0)) that is not a literal formula-for-formula
    ///   reproduction of the draft's group-level expression. This is the status of
    ///   all current executable branches (Phase X and earlier).
    /// </summary>
    [JsonPropertyName("draftAlignmentStatus")]
    public required string DraftAlignmentStatus { get; init; }

    /// <summary>
    /// Human-readable description of the surrogate relationship.
    /// </summary>
    [JsonPropertyName("surrogateDescription")]
    public required string SurrogateDescription { get; init; }

    /// <summary>
    /// The draft section and formula that this operator relates to.
    /// </summary>
    [JsonPropertyName("draftReference")]
    public required string DraftReference { get; init; }

    /// <summary>
    /// The ASSUMPTIONS.md entry that records this lowering choice.
    /// </summary>
    [JsonPropertyName("assumptionId")]
    public required string AssumptionId { get; init; }

    /// <summary>
    /// Whether equivalence between this surrogate and the draft-form operator
    /// has been derived and tested in this repository.
    /// </summary>
    [JsonPropertyName("equivalenceDerived")]
    public bool EquivalenceDerived { get; init; }

    /// <summary>
    /// Conditions under which the surrogate matches the draft-form operator, if known.
    /// Null when equivalence has not been derived.
    /// </summary>
    [JsonPropertyName("equivalenceConditions")]
    public string? EquivalenceConditions { get; init; }

    /// <summary>Provenance of this alignment record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>
    /// Create the standard "branch-local-surrogate" record for the current
    /// T^aug = d_{A0}(omega - A0) executable operator.
    /// Per D-P11-008: do not label this as draft-exact.
    /// </summary>
    public static TorsionDraftAlignmentRecord CreateSurrogate(ProvenanceMeta provenance) =>
        new()
        {
            DraftAlignmentStatus = DraftAlignmentStatuses.BranchLocalSurrogate,
            SurrogateDescription =
                "Current executable branch uses T^aug = d_{A0}(omega - A0), a lower-level " +
                "covariant exterior derivative lowering of the connection difference alpha = omega - A0. " +
                "This is not a literal formula-for-formula reproduction of the draft's group-level " +
                "expression T_g = $ - epsilon^{-1} d_{A0} epsilon. " +
                "See ASSUMPTIONS.md A-008 and D-P11-008.",
            DraftReference = "Draft Section 7: T_g = $ - epsilon^{-1} d_{A0} epsilon",
            AssumptionId = "A-008",
            EquivalenceDerived = false,
            EquivalenceConditions = null,
            Provenance = provenance,
        };
}

/// <summary>
/// Well-known draft-alignment status constants.
/// </summary>
public static class DraftAlignmentStatuses
{
    /// <summary>
    /// The operator is a materially close realization of the draft formula.
    /// Only use this when equivalence is derived and tested.
    /// </summary>
    public const string DraftForm = "draft-form";

    /// <summary>
    /// The operator is a branch-defined computational lowering that is not a
    /// literal formula-for-formula reproduction of the draft's expression.
    /// This is the status of all current (Phase X and earlier) torsion operators.
    /// </summary>
    public const string BranchLocalSurrogate = "branch-local-surrogate";
}
