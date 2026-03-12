using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// A report summarizing the output of the M41 generation/family clustering pipeline.
///
/// Produced by FamilyClusteringEngine from a set of FermionModeFamily records
/// (typically from a FermionFamilyAtlas). Aggregates cluster statistics and
/// preserves conservative labeling conventions.
///
/// PhysicsNote: Clusters labeled "cluster-N" are spectral groupings only.
/// Physical generation identification requires M43 observation comparison.
/// </summary>
public sealed class FamilyClusterReport
{
    /// <summary>Unique report identifier.</summary>
    [JsonPropertyName("reportId")]
    public required string ReportId { get; init; }

    /// <summary>Schema version.</summary>
    [JsonPropertyName("schemaVersion")]
    public string SchemaVersion { get; init; } = "1.0.0";

    /// <summary>Atlas ID this report was built from.</summary>
    [JsonPropertyName("sourceAtlasId")]
    public required string SourceAtlasId { get; init; }

    /// <summary>All clusters produced by the clustering pipeline.</summary>
    [JsonPropertyName("clusters")]
    public required List<FamilyClusterRecord> Clusters { get; init; }

    /// <summary>Total number of clusters.</summary>
    [JsonIgnore]
    public int TotalClusters => Clusters.Count;

    /// <summary>Number of clusters produced by conjugation-rule pass.</summary>
    [JsonIgnore]
    public int ConjugationRuleClusters => Clusters.Count(c => c.ClusteringMethod == "conjugation-rule");

    /// <summary>Number of clusters produced by eigenvalue-proximity pass.</summary>
    [JsonIgnore]
    public int ProximityClusters => Clusters.Count(c => c.ClusteringMethod == "eigenvalue-proximity");

    /// <summary>Number of singleton clusters.</summary>
    [JsonIgnore]
    public int SingletonClusters => Clusters.Count(c => c.ClusteringMethod == "singleton");

    /// <summary>Number of clusters with HasConjugatePair == true.</summary>
    [JsonIgnore]
    public int ClustersWithConjugatePair => Clusters.Count(c => c.HasConjugatePair);

    /// <summary>Mean ambiguity score across all clusters.</summary>
    [JsonIgnore]
    public double MeanAmbiguityScore =>
        Clusters.Count > 0 ? Clusters.Average(c => c.AmbiguityScore) : 0.0;

    /// <summary>Mean branch persistence score across all clusters.</summary>
    [JsonIgnore]
    public double MeanBranchPersistence =>
        Clusters.Count > 0 ? Clusters.Average(c => c.MeanBranchPersistence) : 0.0;

    /// <summary>Summary statistics (serialized).</summary>
    [JsonPropertyName("summary")]
    public FamilyClusterSummary Summary { get; init; } = new();

    /// <summary>Provenance of this report.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}

/// <summary>
/// Summary statistics for a FamilyClusterReport.
/// </summary>
public sealed class FamilyClusterSummary
{
    [JsonPropertyName("totalClusters")]
    public int TotalClusters { get; init; }

    [JsonPropertyName("conjugationRuleClusters")]
    public int ConjugationRuleClusters { get; init; }

    [JsonPropertyName("proximityClusters")]
    public int ProximityClusters { get; init; }

    [JsonPropertyName("singletonClusters")]
    public int SingletonClusters { get; init; }

    [JsonPropertyName("clustersWithConjugatePair")]
    public int ClustersWithConjugatePair { get; init; }

    [JsonPropertyName("meanAmbiguityScore")]
    public double MeanAmbiguityScore { get; init; }

    [JsonPropertyName("meanBranchPersistence")]
    public double MeanBranchPersistence { get; init; }

    [JsonPropertyName("notes")]
    public List<string> Notes { get; init; } = new();
}
