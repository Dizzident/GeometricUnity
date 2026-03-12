using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;

namespace Gu.Phase4.Observation;

/// <summary>
/// Produces FermionObservationSummary and InteractionObservationSummary records
/// from family clusters and coupling atlases.
///
/// The pipeline is a simplified version of the Phase III ObservationPipeline,
/// adapted for fermionic candidates. It does not perform full sigma_h pullback
/// (that requires a complete observation operator), but extracts the key
/// observable properties: chirality, mass-like scale, conjugation status.
/// </summary>
public sealed class FermionObservationPipeline
{
    /// <summary>
    /// Produce observation summaries for all fermionic clusters.
    /// </summary>
    public IReadOnlyList<FermionObservationSummary> ObserveClusters(
        IReadOnlyList<FamilyClusterRecord> clusters,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(clusters);
        ArgumentNullException.ThrowIfNull(provenance);

        var results = new List<FermionObservationSummary>(clusters.Count);
        foreach (var cluster in clusters)
            results.Add(ObserveCluster(cluster, provenance));
        return results;
    }

    /// <summary>
    /// Produce interaction observation summaries from a coupling atlas.
    /// </summary>
    public IReadOnlyList<InteractionObservationSummary> ObserveInteractions(
        CouplingAtlas atlas,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(provenance);

        // Group by boson mode ID
        var byBoson = atlas.Couplings
            .GroupBy(c => c.BosonModeId)
            .ToList();

        var results = new List<InteractionObservationSummary>(byBoson.Count);
        foreach (var group in byBoson)
        {
            var couplings = group.ToList();
            double mean = couplings.Average(c => c.CouplingProxyMagnitude);
            double max = couplings.Max(c => c.CouplingProxyMagnitude);
            int nonZero = couplings.Count(c => c.CouplingProxyMagnitude > 1e-12);
            var fermionIds = couplings
                .SelectMany(c => new[] { c.FermionModeIdI, c.FermionModeIdJ })
                .Distinct()
                .ToList();

            results.Add(new InteractionObservationSummary
            {
                AtlasId = atlas.AtlasId,
                BosonModeId = group.Key,
                FermionModeIds = fermionIds,
                MeanCouplingMagnitude = mean,
                MaxCouplingMagnitude = max,
                NonZeroCouplingCount = nonZero,
                NormalizationConvention = atlas.NormalizationConvention,
                Provenance = provenance,
            });
        }

        return results;
    }

    private static FermionObservationSummary ObserveCluster(
        FamilyClusterRecord cluster,
        ProvenanceMeta provenance)
    {
        bool isTrivial = cluster.DominantChirality is "mixed" or "undetermined"
                         || cluster.ClusteringMethod == "singleton";

        var notes = new List<string>(cluster.ClusteringNotes);
        if (isTrivial)
            notes.Add("Trivial observation: chirality undetermined or singleton cluster.");
        if (cluster.AmbiguityScore > 0.5)
            notes.Add($"High ambiguity score ({cluster.AmbiguityScore:F2}); physical interpretation uncertain.");

        return new FermionObservationSummary
        {
            ClusterId = cluster.ClusterId,
            ObservedChirality = cluster.DominantChirality,
            XChirality = FermionObservationBuilder.SynthesizeXChirality(cluster.ClusterId, cluster.DominantChirality),
            YChirality = null, // only available when full per-mode eigenvectors are present
            MassLikeEnvelope = cluster.EigenvalueMagnitudeEnvelope,
            HasConjugatePair = cluster.HasConjugatePair,
            BranchPersistenceScore = cluster.MeanBranchPersistence,
            AmbiguityScore = cluster.AmbiguityScore,
            IsTrivial = isTrivial,
            Notes = notes,
            Provenance = provenance,
        };
    }
}
