using System.Text.Json.Serialization;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Canonicity;

/// <summary>
/// Tracks the canonicity status of a branch-sensitive object class.
/// A docket closes only by actual uniqueness theorem, classification theorem,
/// or explicit invariance evidence strong enough to support a declared claim class.
/// The software must not mark a docket as closed just because one branch behaves well numerically.
/// </summary>
public sealed class CanonicityDocket
{
    /// <summary>Object class this docket tracks (e.g. "A0", "torsion", "shiab").</summary>
    [JsonPropertyName("objectClass")]
    public required string ObjectClass { get; init; }

    /// <summary>Currently active representative for this object class.</summary>
    [JsonPropertyName("activeRepresentative")]
    public required string ActiveRepresentative { get; init; }

    /// <summary>Equivalence relation identifier used for comparison.</summary>
    [JsonPropertyName("equivalenceRelationId")]
    public required string EquivalenceRelationId { get; init; }

    /// <summary>Admissible comparison class.</summary>
    [JsonPropertyName("admissibleComparisonClass")]
    public required string AdmissibleComparisonClass { get; init; }

    /// <summary>
    /// Claims that are blocked until this docket is closed.
    /// These downstream claims cannot be made while canonicity is unresolved.
    /// </summary>
    [JsonPropertyName("downstreamClaimsBlockedUntilClosure")]
    public required IReadOnlyList<string> DownstreamClaimsBlockedUntilClosure { get; init; }

    /// <summary>Current evidence records.</summary>
    [JsonPropertyName("currentEvidence")]
    public required IReadOnlyList<CanonicityEvidenceRecord> CurrentEvidence { get; init; }

    /// <summary>Known counterexamples to closure.</summary>
    [JsonPropertyName("knownCounterexamples")]
    public required IReadOnlyList<string> KnownCounterexamples { get; init; }

    /// <summary>Pending theorems that could close this docket.</summary>
    [JsonPropertyName("pendingTheorems")]
    public required IReadOnlyList<string> PendingTheorems { get; init; }

    /// <summary>Study report identifiers that have contributed evidence.</summary>
    [JsonPropertyName("studyReports")]
    public required IReadOnlyList<string> StudyReports { get; init; }

    /// <summary>Docket status.</summary>
    [JsonPropertyName("status")]
    public required DocketStatus Status { get; init; }
}
