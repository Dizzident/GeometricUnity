using Gu.Core;
using Gu.Phase4.Chirality;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;

namespace Gu.Phase4.Observation;

/// <summary>
/// Builds FermionObservationSummary and InteractionObservationSummary records
/// from Phase IV family clusters and coupling atlases.
///
/// M43: Fermionic observation summaries connect the spectral output to the
/// observation pipeline for comparison against experimental data.
///
/// Conservative conventions:
/// - Observation is structural (chirality, mass-like scale, conjugation).
/// - Physical mass calibration is NOT performed here.
/// - Trivial observations (mixed/undetermined, singleton with low persistence)
///   are flagged and should not be directly compared to known particles.
/// </summary>
public static class FermionObservationBuilder
{
    /// <summary>
    /// Build a FermionObservationSummary from a FamilyClusterRecord.
    ///
    /// The observation captures:
    /// - chirality (from cluster.DominantChirality)
    /// - mass-like scale (from cluster.EigenvalueMagnitudeEnvelope)
    /// - conjugation status (from cluster.HasConjugatePair)
    /// - stability indicators
    /// </summary>
    public static FermionObservationSummary Build(
        FamilyClusterRecord cluster,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(provenance);

        bool isTrivial = cluster.DominantChirality is "mixed" or "undetermined"
            || (cluster.ClusteringMethod == "singleton" && cluster.MeanBranchPersistence < 0.5);

        var notes = new List<string>();
        if (isTrivial)
            notes.Add($"Trivial observation: chirality={cluster.DominantChirality}, method={cluster.ClusteringMethod}, persistence={cluster.MeanBranchPersistence:F3}.");
        if (cluster.AmbiguityScore > 0.0)
            notes.Add($"Ambiguity score={cluster.AmbiguityScore:F3}; matching was not fully unambiguous.");

        // Per D-P11-010: label all current observations as proxy-observation.
        // Full sigma_h pullback from Y_h to X_h is not yet implemented.
        notes.Add(
            "Observation path: proxy-observation. " +
            "This summary is produced from cluster-level spectral properties (chirality tag, " +
            "eigenvalue envelope) without a full sigma_h pullback from Y to X. " +
            "Do not treat this as full draft-aligned pullback evidence (D-P11-010).");

        return new FermionObservationSummary
        {
            ClusterId = cluster.ClusterId,
            ObservedChirality = cluster.DominantChirality,
            XChirality = SynthesizeXChirality(cluster.ClusterId, cluster.DominantChirality),
            YChirality = null, // only available when full per-mode eigenvectors are present
            MassLikeEnvelope = cluster.EigenvalueMagnitudeEnvelope,
            HasConjugatePair = cluster.HasConjugatePair,
            ConjugateClusterId = null, // resolved externally if needed
            BranchPersistenceScore = cluster.MeanBranchPersistence,
            AmbiguityScore = cluster.AmbiguityScore,
            IsTrivial = isTrivial,
            ObservationPathLabel = ObservationPathLabels.ProxyObservation,
            Notes = notes,
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Build observation summaries for all clusters in a list.
    /// </summary>
    public static List<FermionObservationSummary> BuildAll(
        IReadOnlyList<FamilyClusterRecord> clusters,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(clusters);
        ArgumentNullException.ThrowIfNull(provenance);

        return clusters.Select(c => Build(c, provenance)).ToList();
    }

    /// <summary>
    /// Build an InteractionObservationSummary from a CouplingAtlas,
    /// aggregating over all fermionic modes for a given boson mode ID.
    /// </summary>
    public static InteractionObservationSummary BuildInteractionSummary(
        string bosonModeId,
        CouplingAtlas atlas,
        double zeroThreshold,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(bosonModeId);
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(provenance);

        var matchingCouplings = atlas.Couplings
            .Where(c => c.BosonModeId == bosonModeId)
            .ToList();

        var fermionModeIds = matchingCouplings
            .SelectMany(c => new[] { c.FermionModeIdI, c.FermionModeIdJ })
            .Distinct()
            .ToList();

        double meanMag = matchingCouplings.Count > 0
            ? matchingCouplings.Average(c => c.CouplingProxyMagnitude)
            : 0.0;

        double maxMag = matchingCouplings.Count > 0
            ? matchingCouplings.Max(c => c.CouplingProxyMagnitude)
            : 0.0;

        int nonZeroCount = matchingCouplings.Count(c => c.CouplingProxyMagnitude > zeroThreshold);

        return new InteractionObservationSummary
        {
            AtlasId = atlas.AtlasId,
            BosonModeId = bosonModeId,
            FermionModeIds = fermionModeIds,
            MeanCouplingMagnitude = meanMag,
            MaxCouplingMagnitude = maxMag,
            NonZeroCouplingCount = nonZeroCount,
            NormalizationConvention = atlas.NormalizationConvention,
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Synthesize an X-space ChiralityDecomposition from a dominant chirality string tag.
    /// Used when full per-mode eigenvectors are not available (cluster-level summary).
    /// Convention: "left-is-minus" (standard GU convention).
    /// </summary>
    public static ChiralityDecomposition SynthesizeXChirality(string modeId, string chiralityTag)
    {
        (double left, double right, string status) = chiralityTag switch
        {
            "left"           => (1.0, 0.0, "definite-left"),
            "right"          => (0.0, 1.0, "definite-right"),
            "conjugate-pair" => (0.5, 0.5, "mixed"),
            _                => (0.5, 0.5, "mixed"),   // "mixed", "undetermined", or unknown
        };

        return new ChiralityDecomposition
        {
            ModeId = modeId,
            LeftFraction = left,
            RightFraction = right,
            MixedFraction = 0.0,
            ChiralityTag = chiralityTag,
            ChiralityStatus = status,
            LeakageDiagnostic = System.Math.Abs(left + right - 1.0),
            SignConvention = "left-is-minus",
            DiagnosticNotes = new List<string>
            {
                "Synthesized from cluster dominant-chirality tag; no per-mode eigenvectors available.",
            },
        };
    }

    /// <summary>
    /// Build InteractionObservationSummary for each distinct boson mode ID in a CouplingAtlas.
    /// </summary>
    public static List<InteractionObservationSummary> BuildAllInteractionSummaries(
        CouplingAtlas atlas,
        double zeroThreshold,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(atlas);
        ArgumentNullException.ThrowIfNull(provenance);

        var bosonModeIds = atlas.Couplings.Select(c => c.BosonModeId).Distinct().ToList();
        return bosonModeIds
            .Select(b => BuildInteractionSummary(b, atlas, zeroThreshold, provenance))
            .ToList();
    }
}
