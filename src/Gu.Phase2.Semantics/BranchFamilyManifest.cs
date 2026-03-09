using System.Text.Json.Serialization;

namespace Gu.Phase2.Semantics;

/// <summary>
/// Groups a family of branch variants for a Phase II study.
/// A branch family defines the space of admissible branch variants to be compared.
/// </summary>
public sealed class BranchFamilyManifest
{
    /// <summary>Unique family identifier.</summary>
    [JsonPropertyName("familyId")]
    public required string FamilyId { get; init; }

    /// <summary>Description of what this family studies.</summary>
    [JsonPropertyName("description")]
    public required string Description { get; init; }

    /// <summary>The branch variants in this family.</summary>
    [JsonPropertyName("variants")]
    public required IReadOnlyList<BranchVariantManifest> Variants { get; init; }

    /// <summary>Default equivalence specification for comparing variants in this family.</summary>
    [JsonPropertyName("defaultEquivalence")]
    public required EquivalenceSpec DefaultEquivalence { get; init; }

    /// <summary>Timestamp when this family was created.</summary>
    [JsonPropertyName("createdAt")]
    public required DateTimeOffset CreatedAt { get; init; }
}
