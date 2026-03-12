using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Fermions;

/// <summary>
/// Tracks a fermion mode family across branch variants and refinement levels.
///
/// A family is a group of FermionModeRecords that are identified as
/// "the same mode" across different branches or mesh refinements, based
/// on eigenvalue proximity, eigenspace overlap, chirality profile similarity,
/// conjugation consistency, and observed-signature similarity.
///
/// PhysicsNote: A FermionModeFamily is NOT a physical particle family.
/// It is a spectral tracking object. Family-like physical clustering is
/// done in Gu.Phase4.FamilyClustering.
/// </summary>
public sealed class FermionModeFamily
{
    /// <summary>Unique family identifier.</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>All mode record IDs belonging to this family.</summary>
    [JsonPropertyName("memberModeIds")]
    public required List<string> MemberModeIds { get; init; }

    /// <summary>Background IDs where this family was observed.</summary>
    [JsonPropertyName("backgroundIds")]
    public required List<string> BackgroundIds { get; init; }

    /// <summary>Branch variant IDs where this family persists.</summary>
    [JsonPropertyName("branchVariantIds")]
    public required List<string> BranchVariantIds { get; init; }

    /// <summary>
    /// Envelope of eigenvalue magnitudes (min, mean, max) across all member modes.
    /// </summary>
    [JsonPropertyName("eigenvalueMagnitudeEnvelope")]
    public required double[] EigenvalueMagnitudeEnvelope { get; init; }

    /// <summary>
    /// Dominant chirality profile: "left", "right", "mixed", or "undetermined".
    /// Based on the most common ChiralityDecomposition.NetChirality sign across members.
    /// </summary>
    [JsonPropertyName("dominantChiralityProfile")]
    public required string DominantChiralityProfile { get; init; }

    /// <summary>
    /// Whether the majority of member modes have a conjugation partner within the family.
    /// </summary>
    [JsonPropertyName("hasConjugationPair")]
    public bool HasConjugationPair { get; init; }

    /// <summary>ID of the conjugation-paired family (if found).</summary>
    [JsonPropertyName("conjugateFamilyId")]
    public string? ConjugateFamilyId { get; init; }

    /// <summary>Branch persistence score [0, 1].</summary>
    [JsonPropertyName("branchPersistenceScore")]
    public double BranchPersistenceScore { get; init; }

    /// <summary>Refinement persistence score [0, 1].</summary>
    [JsonPropertyName("refinementPersistenceScore")]
    public double RefinementPersistenceScore { get; init; }

    /// <summary>Average gauge-leak score across member modes.</summary>
    [JsonPropertyName("averageGaugeLeakScore")]
    public double AverageGaugeLeakScore { get; init; }

    /// <summary>Ambiguity notes (e.g. multiple candidate matches).</summary>
    [JsonPropertyName("ambiguityNotes")]
    public List<string> AmbiguityNotes { get; init; } = new();

    /// <summary>Provenance of this family record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
