using System.Text.Json.Serialization;
using Gu.Core;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Atlas of fermion mode families across branch variants and refinement levels
/// for a given branch family / background study.
///
/// This is the primary output of M39 (Fermion mode tracking and persistence atlas).
///
/// PhysicsNote: A FermionFamilyAtlas is a spectral tracking artifact.
/// Physical family identification (generation structure) requires M41.
/// </summary>
public sealed class FermionFamilyAtlas
{
    /// <summary>Unique atlas identifier.</summary>
    [JsonPropertyName("atlasId")]
    public required string AtlasId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Branch family or study ID this atlas was built for.
    /// </summary>
    [JsonPropertyName("branchFamilyId")]
    public required string BranchFamilyId { get; init; }

    /// <summary>Context IDs (branch variants / refinement levels) covered by this atlas.</summary>
    [JsonPropertyName("contextIds")]
    public required List<string> ContextIds { get; init; }

    /// <summary>Background IDs covered by this atlas.</summary>
    [JsonPropertyName("backgroundIds")]
    public required List<string> BackgroundIds { get; init; }

    /// <summary>All mode families identified in this atlas.</summary>
    [JsonPropertyName("families")]
    public required List<FermionModeFamily> Families { get; init; }

    /// <summary>Total number of unique modes tracked.</summary>
    [JsonIgnore]
    public int TotalModes => Families.Sum(f => f.MemberModeIds.Count);

    /// <summary>Number of families with branch persistence score > 0.5.</summary>
    [JsonIgnore]
    public int PersistentFamilyCount => Families.Count(f => f.BranchPersistenceScore > 0.5);

    /// <summary>
    /// Summary statistics for this atlas.
    /// </summary>
    [JsonPropertyName("summary")]
    public FermionAtlasSummary Summary { get; init; } = new();

    /// <summary>Provenance of this atlas.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

/// <summary>
/// Summary statistics for a FermionFamilyAtlas.
/// </summary>
public sealed class FermionAtlasSummary
{
    [JsonPropertyName("totalFamilies")]
    public int TotalFamilies { get; init; }

    [JsonPropertyName("persistentFamilies")]
    public int PersistentFamilies { get; init; }

    [JsonPropertyName("leftChiralFamilies")]
    public int LeftChiralFamilies { get; init; }

    [JsonPropertyName("rightChiralFamilies")]
    public int RightChiralFamilies { get; init; }

    [JsonPropertyName("mixedChiralFamilies")]
    public int MixedChiralFamilies { get; init; }

    [JsonPropertyName("familiesWithConjugatePair")]
    public int FamiliesWithConjugatePair { get; init; }

    [JsonPropertyName("meanBranchPersistence")]
    public double MeanBranchPersistence { get; init; }

    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = new();
}
