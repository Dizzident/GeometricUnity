using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Records a group of fermionic mode IDs that have been tracked as the "same mode"
/// across different spectral contexts (branch variants or refinement levels).
///
/// A family is seeded by a source mode and tracks its matched counterparts
/// across multiple contexts. The branch/refinement persistence scores reflect
/// how reliably this mode appears across contexts.
/// </summary>
public sealed class FermionModeFamilyRecord
{
    /// <summary>Unique family identifier (e.g., "family-bg001-mode-000").</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>
    /// Mode IDs that belong to this family, one per context.
    /// Ordered by context (same order as the input results list).
    /// </summary>
    [JsonPropertyName("memberModeIds")]
    public required List<string> MemberModeIds { get; init; }

    /// <summary>
    /// Context identifiers (spectral result IDs) that each member came from.
    /// Same length as MemberModeIds.
    /// </summary>
    [JsonPropertyName("contextIds")]
    public required List<string> ContextIds { get; init; }

    /// <summary>Mean |eigenvalue| across all members.</summary>
    [JsonPropertyName("meanEigenvalue")]
    public required double MeanEigenvalue { get; init; }

    /// <summary>
    /// Spread (standard deviation) of |eigenvalue| across members.
    /// Zero for single-context families.
    /// </summary>
    [JsonPropertyName("eigenvalueSpread")]
    public required double EigenvalueSpread { get; init; }

    /// <summary>
    /// Dominant chirality tag across all member modes ("left", "right", "mixed", "trivial").
    /// Determined by majority vote among member ChiralityDecomposition tags.
    /// </summary>
    [JsonPropertyName("dominantChirality")]
    public required string DominantChirality { get; init; }

    /// <summary>
    /// Family ID of the conjugation partner family, if known.
    /// Null if no conjugate family has been identified.
    /// </summary>
    [JsonPropertyName("conjugationPairFamilyId")]
    public string? ConjugationPairFamilyId { get; init; }

    /// <summary>
    /// Fraction of branch variant contexts in which this family has a member,
    /// in [0, 1]. 1.0 means the mode persists across all branch variants considered.
    /// </summary>
    [JsonPropertyName("branchPersistenceScore")]
    public required double BranchPersistenceScore { get; init; }

    /// <summary>
    /// Fraction of refinement level contexts in which this family has a member,
    /// in [0, 1]. 1.0 means the mode persists under mesh refinement.
    /// </summary>
    [JsonPropertyName("refinementPersistenceScore")]
    public required double RefinementPersistenceScore { get; init; }

    /// <summary>
    /// Whether this family is considered stable:
    /// stable = branchPersistenceScore >= threshold AND eigenvalueSpread is small.
    /// </summary>
    [JsonPropertyName("isStable")]
    public required bool IsStable { get; init; }

    /// <summary>
    /// Number of ambiguous match records contributing to this family.
    /// Nonzero means some assignments were uncertain.
    /// </summary>
    [JsonPropertyName("ambiguityCount")]
    public int AmbiguityCount { get; init; }

    /// <summary>Provenance of this family record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
