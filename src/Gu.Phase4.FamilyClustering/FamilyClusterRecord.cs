using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// A generation-like cluster of fermionic mode families.
///
/// Represents a hypothesis that multiple mode families form a generation-like
/// multiplet. Does NOT assert physical generation identity; that requires a
/// full observation and comparison campaign.
///
/// Conservative output convention:
/// - ClusterLabel is "cluster-N" (N = 0-based index), never "first generation" etc.
/// - AmbiguityScore = 0 means fully unambiguous assignment; 1 = maximally ambiguous.
///
/// PhysicsNote (P4-IA §8.6): family clustering is based on spectral and chirality
/// proximity. Physical generation identification is downstream in M43.
/// </summary>
public sealed class FamilyClusterRecord
{
    /// <summary>Unique cluster identifier (e.g., "cluster-0").</summary>
    [JsonPropertyName("clusterId")]
    public required string ClusterId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Conservative cluster label (e.g., "cluster-0", "cluster-1").</summary>
    [JsonPropertyName("clusterLabel")]
    public required string ClusterLabel { get; init; }

    /// <summary>Family IDs that belong to this cluster.</summary>
    [JsonPropertyName("memberFamilyIds")]
    public required List<string> MemberFamilyIds { get; init; }

    /// <summary>
    /// Number of member families.
    /// </summary>
    [JsonIgnore]
    public int MemberCount => MemberFamilyIds.Count;

    /// <summary>
    /// Dominant chirality profile of this cluster:
    /// "left", "right", "mixed", "conjugate-pair", "undetermined".
    /// </summary>
    [JsonPropertyName("dominantChirality")]
    public required string DominantChirality { get; init; }

    /// <summary>
    /// Whether this cluster contains a conjugation pair (left+right pair of families).
    /// </summary>
    [JsonPropertyName("hasConjugatePair")]
    public bool HasConjugatePair { get; init; }

    /// <summary>
    /// Eigenvalue magnitude envelope across all member families: [min, mean, max].
    /// </summary>
    [JsonPropertyName("eigenvalueMagnitudeEnvelope")]
    public required double[] EigenvalueMagnitudeEnvelope { get; init; }

    /// <summary>
    /// Ambiguity score [0, 1]:
    /// 0 = all assignments unambiguous, 1 = all assignments are ambiguous.
    /// Computed as the fraction of member families with ambiguity counts > 0.
    /// </summary>
    [JsonPropertyName("ambiguityScore")]
    public double AmbiguityScore { get; init; }

    /// <summary>
    /// Mean branch persistence score across member families [0, 1].
    /// </summary>
    [JsonPropertyName("meanBranchPersistence")]
    public double MeanBranchPersistence { get; init; }

    /// <summary>
    /// Clustering method used:
    /// "conjugation-rule" — paired by conjugation-pair resolution,
    /// "eigenvalue-proximity" — grouped by eigenvalue band proximity,
    /// "singleton" — single isolated family with no match criteria met.
    /// </summary>
    [JsonPropertyName("clusteringMethod")]
    public required string ClusteringMethod { get; init; }

    /// <summary>Notes and ambiguity messages from the clustering algorithm.</summary>
    [JsonPropertyName("clusteringNotes")]
    public List<string> ClusteringNotes { get; init; } = new();

    /// <summary>Provenance of this cluster record.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
