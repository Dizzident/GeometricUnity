using Gu.Core;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Builds a FamilyClusterReport from a FermionFamilyAtlas.
///
/// Orchestrates FamilyClusteringEngine and computes the serializable summary.
/// </summary>
public sealed class FamilyClusterReportBuilder
{
    private readonly FamilyClusteringEngine _engine;

    public FamilyClusterReportBuilder(FamilyClusteringConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);
        _engine = new FamilyClusteringEngine(config);
    }

    /// <summary>
    /// Build a FamilyClusterReport from the families in the given atlas.
    /// </summary>
    public FamilyClusterReport Build(
        FermionFamilyAtlas atlas,
        string reportId,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(provenance);

        var clusters = _engine.Cluster(atlas.Families, provenance);
        var summary = ComputeSummary(clusters);

        return new FamilyClusterReport
        {
            ReportId = reportId,
            SourceAtlasId = atlas.AtlasId,
            Clusters = clusters.ToList(),
            Summary = summary,
            Provenance = provenance,
        };
    }

    private static FamilyClusterSummary ComputeSummary(IReadOnlyList<FamilyClusterRecord> clusters)
    {
        if (clusters.Count == 0)
            return new FamilyClusterSummary { Notes = new List<string> { "No clusters produced." } };

        int conjugation = clusters.Count(c => c.ClusteringMethod == "conjugation-rule");
        int proximity = clusters.Count(c => c.ClusteringMethod == "eigenvalue-proximity");
        int singleton = clusters.Count(c => c.ClusteringMethod == "singleton");
        int withPair = clusters.Count(c => c.HasConjugatePair);
        double meanAmbiguity = clusters.Average(c => c.AmbiguityScore);
        double meanPersistence = clusters.Average(c => c.MeanBranchPersistence);

        var notes = new List<string>();
        if (singleton == clusters.Count)
            notes.Add("All clusters are singletons; consider adding more spectral contexts.");
        if (meanAmbiguity > 0.5)
            notes.Add("High mean ambiguity score; tracking assignments may be unreliable.");

        return new FamilyClusterSummary
        {
            TotalClusters = clusters.Count,
            ConjugationRuleClusters = conjugation,
            ProximityClusters = proximity,
            SingletonClusters = singleton,
            ClustersWithConjugatePair = withPair,
            MeanAmbiguityScore = meanAmbiguity,
            MeanBranchPersistence = meanPersistence,
            Notes = notes,
        };
    }
}
