using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;
using Gu.Phase3.Registry;

namespace Gu.Phase4.Registry;

/// <summary>
/// Builds a UnifiedParticleRegistry by merging bosonic, fermionic, and coupling candidates.
///
/// Algorithm (P4-IA §8.7):
/// 1. For each FamilyClusterRecord: create a UnifiedParticleRecord of type Fermion.
/// 2. For each CandidateBosonRecord: create a UnifiedParticleRecord of type Boson.
/// 3. For each CouplingAtlas record (optional): create records of type Interaction.
/// 4. Apply demotion rules:
///    - If any contributing source has ComputedWithUnverifiedGpu: cap claim class at C1.
///    - If branchStabilityScore &lt; StabilityThreshold: demote to C2 max.
///    - If ambiguityScore &gt; AmbiguityThreshold: add AmbiguousMatching demotion note.
/// 5. Assign claim classes (canonical full-name format, ARCH_P4.md §ParticleClaimClass):
///    - C0_NumericalMode: not yet evaluated
///    - C1_LocalPersistentMode: computed but unverified (e.g., unverified GPU)
///    - C2_BranchStableCandidate: unstable (low branch persistence)
///    - C3_ObservedStableCandidate: stable but no comparison
///    - C4_PhysicalAnalogyCandidate: stable + comparison compatible
///    - C5_StrongIdentificationCandidate: stable + repeated comparison compatible
///
/// The builder operates conservatively: when in doubt, C0_NumericalMode is assigned.
/// </summary>
public sealed class UnifiedRegistryBuilder
{
    private readonly UnifiedRegistryConfig _config;

    public UnifiedRegistryBuilder(UnifiedRegistryConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Build a registry from family clusters, boson candidates, and (optionally) coupling atlases.
    /// </summary>
    /// <param name="registryId">Registry identifier.</param>
    /// <param name="registryVersion">Registry version string.</param>
    /// <param name="fermionClusters">Phase IV family cluster records.</param>
    /// <param name="bosonCandidates">Phase III boson candidates (optional).</param>
    /// <param name="couplingAtlases">Phase IV coupling atlases (optional).</param>
    /// <param name="provenance">Provenance metadata.</param>
    /// <param name="observationConfidenceByClusterId">
    /// Lookup of cluster ID → observation confidence (e.g. BranchPersistenceScore from
    /// FermionObservationSummary). When provided, ObservationConfidence is populated from
    /// this lookup and the LowObservation demotion rule fires for C3+ candidates below
    /// StabilityThreshold. Pass null to retain the default 0.0 behaviour.
    /// </param>
    public UnifiedParticleRegistry Build(
        string registryId,
        string registryVersion,
        IReadOnlyList<FamilyClusterRecord> fermionClusters,
        IReadOnlyList<CandidateBosonRecord>? bosonCandidates,
        IReadOnlyList<CouplingAtlas>? couplingAtlases,
        ProvenanceMeta provenance,
        IReadOnlyDictionary<string, double>? observationConfidenceByClusterId = null)
    {
        ArgumentNullException.ThrowIfNull(registryId);
        ArgumentNullException.ThrowIfNull(registryVersion);
        ArgumentNullException.ThrowIfNull(fermionClusters);
        ArgumentNullException.ThrowIfNull(provenance);

        var records = new List<UnifiedParticleRecord>();
        var notes = new List<string>();

        // 1. Fermion candidates from family clusters
        int fermionIdx = 0;
        foreach (var cluster in fermionClusters)
        {
            string particleId = $"fermion-{registryId}-{fermionIdx:D4}";
            var record = BuildFermionRecord(particleId, cluster, registryVersion, provenance, observationConfidenceByClusterId);
            records.Add(record);
            fermionIdx++;
        }

        // 2. Boson candidates from Phase III registry
        if (bosonCandidates is not null)
        {
            int bosonIdx = 0;
            foreach (var boson in bosonCandidates)
            {
                string particleId = $"boson-{registryId}-{bosonIdx:D4}";
                var record = BuildBosonRecord(particleId, boson, registryVersion, provenance);
                records.Add(record);
                bosonIdx++;
            }
        }

        // 3. Interaction candidates from coupling atlases
        if (couplingAtlases is not null)
        {
            int coupIdx = 0;
            foreach (var atlas in couplingAtlases)
            {
                foreach (var coupling in atlas.Couplings)
                {
                    // Only include couplings above magnitude threshold
                    if (coupling.CouplingProxyMagnitude < _config.MinCouplingMagnitude)
                        continue;

                    string particleId = $"interaction-{registryId}-{coupIdx:D4}";
                    var record = BuildInteractionRecord(particleId, coupling, atlas, registryVersion, provenance);
                    records.Add(record);
                    coupIdx++;
                }
            }
        }

        if (records.Count == 0)
            notes.Add("Empty registry: no candidates met inclusion criteria.");

        var registry = new UnifiedParticleRegistry { RegistryVersion = registryVersion };
        foreach (var record in records)
            registry.Register(record);
        return registry;
    }

    // --- Private builders ---

    private UnifiedParticleRecord BuildFermionRecord(
        string particleId,
        FamilyClusterRecord cluster,
        string registryVersion,
        ProvenanceMeta provenance,
        IReadOnlyDictionary<string, double>? observationConfidenceByClusterId)
    {
        var demotions = new List<ParticleClaimDemotion>();
        // Default: stable, no comparison yet — canonical full-name format (BLOCKER-M42-1 fix)
        string claimClass = "C3_ObservedStableCandidate";

        // Demotion: low branch persistence
        if (cluster.MeanBranchPersistence < _config.StabilityThreshold)
        {
            claimClass = "C2_BranchStableCandidate";
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "LowPersistence",
                Details = $"Mean branch persistence {cluster.MeanBranchPersistence:F3} < threshold {_config.StabilityThreshold:F3}.",
                FromClaimClass = "C3_ObservedStableCandidate",
                ToClaimClass = "C2_BranchStableCandidate",
            });
        }

        // Demotion: high ambiguity
        if (cluster.AmbiguityScore > _config.AmbiguityThreshold)
        {
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "AmbiguousMatching",
                Details = $"Cluster ambiguity score {cluster.AmbiguityScore:F3} > threshold {_config.AmbiguityThreshold:F3}.",
                FromClaimClass = claimClass,
                ToClaimClass = claimClass, // Note only — doesn't reduce class further unless already C3
            });
        }

        // If singleton with no strong evidence, downgrade to C2
        if (cluster.ClusteringMethod == "singleton" && cluster.MemberFamilyIds.Count == 1)
        {
            if (claimClass == "C3_ObservedStableCandidate")
                claimClass = "C2_BranchStableCandidate"; // provisional
        }

        // Observation confidence from FermionObservationSummary.BranchPersistenceScore (GAP-2)
        // Only set and apply when the caller provides a confidence lookup (observationConfidenceByClusterId != null).
        // When null is passed, no observation data was provided and we skip the LowObservation demotion.
        double observationConfidence = 0.0;
        bool hasObservationData = false;
        if (observationConfidenceByClusterId is not null
            && observationConfidenceByClusterId.TryGetValue(cluster.ClusterId, out double confVal2))
        {
            observationConfidence = confVal2;
            hasObservationData = true;
        }

        // LowObservation demotion: fires only when real observation data was supplied and confidence is low
        int claimLevel = UnifiedParticleRegistry.ParseClaimClassLevel(claimClass);
        if (hasObservationData && observationConfidence < _config.StabilityThreshold && claimLevel >= 3)
        {
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "LowObservation",
                Details = $"Observation confidence {observationConfidence:F3} below threshold {_config.StabilityThreshold:F3}.",
                FromClaimClass = claimClass,
                ToClaimClass = "C2_BranchStableCandidate",
            });
            claimClass = "C2_BranchStableCandidate";
        }

        return new UnifiedParticleRecord
        {
            ParticleId = particleId,
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = cluster.ClusterId,
            ContributingSourceIds = cluster.MemberFamilyIds.ToList(),
            BranchVariantSet = new List<string>(), // resolved from families if available
            BackgroundSet = new List<string>(),
            Chirality = cluster.DominantChirality,
            MassLikeEnvelope = cluster.EigenvalueMagnitudeEnvelope,
            BranchStabilityScore = cluster.MeanBranchPersistence,
            ObservationConfidence = observationConfidence,
            ComparisonEvidenceScore = 0.0,
            ClaimClass = claimClass,
            ComputedWithUnverifiedGpu = false, // unknown at this stage
            Demotions = demotions,
            AmbiguityNotes = cluster.ClusteringNotes.ToList(),
            RegistryVersion = registryVersion,
            Provenance = provenance,
        };
    }

    private UnifiedParticleRecord BuildBosonRecord(
        string particleId,
        CandidateBosonRecord boson,
        string registryVersion,
        ProvenanceMeta provenance)
    {
        var demotions = new List<ParticleClaimDemotion>();

        // Translate Phase III claim class to unified class (canonical full-name format, BLOCKER-M42-1 fix)
        string claimClass = BosonClaimToString(boson.ClaimClass);

        // Propagate GPU demotion
        int claimLevel = UnifiedParticleRegistry.ParseClaimClassLevel(claimClass);
        if (boson.ComputedWithUnverifiedGpu && claimLevel > 1)
        {
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "UnverifiedGpu",
                Details = "Boson computed with unverified GPU backend; capped at C1.",
                FromClaimClass = claimClass,
                ToClaimClass = "C1_LocalPersistentMode",
            });
            claimClass = "C1_LocalPersistentMode";
        }

        return new UnifiedParticleRecord
        {
            ParticleId = particleId,
            ParticleType = UnifiedParticleType.Boson,
            PrimarySourceId = boson.CandidateId,
            ContributingSourceIds = boson.ContributingModeIds.ToList(),
            BranchVariantSet = boson.BranchVariantSet.ToList(),
            BackgroundSet = boson.BackgroundSet.ToList(),
            Chirality = null, // bosons don't have chirality
            MassLikeEnvelope = boson.MassLikeEnvelope,
            BranchStabilityScore = boson.BranchStabilityScore,
            ObservationConfidence = boson.ObservationStabilityScore,
            ComparisonEvidenceScore = 0.0,
            ClaimClass = claimClass,
            ComputedWithUnverifiedGpu = boson.ComputedWithUnverifiedGpu,
            Demotions = demotions,
            AmbiguityNotes = boson.AmbiguityNotes.ToList(),
            RegistryVersion = registryVersion,
            Provenance = provenance,
        };
    }

    private static UnifiedParticleRecord BuildInteractionRecord(
        string particleId,
        BosonFermionCouplingRecord coupling,
        CouplingAtlas atlas,
        string registryVersion,
        ProvenanceMeta provenance)
    {
        return new UnifiedParticleRecord
        {
            ParticleId = particleId,
            ParticleType = UnifiedParticleType.Interaction,
            PrimarySourceId = coupling.CouplingId,
            ContributingSourceIds = new List<string>
            {
                coupling.BosonModeId,
                coupling.FermionModeIdI,
                coupling.FermionModeIdJ,
            },
            BranchVariantSet = new List<string>(),
            BackgroundSet = new List<string> { atlas.FermionBackgroundId },
            Chirality = null,
            MassLikeEnvelope = new[] { 0.0, coupling.CouplingProxyMagnitude, coupling.CouplingProxyMagnitude },
            BranchStabilityScore = coupling.BranchStabilityScore,
            ObservationConfidence = 0.0,
            ComparisonEvidenceScore = 0.0,
            ClaimClass = "C0_NumericalMode", // interactions are always C0 until M43 observation
            ComputedWithUnverifiedGpu = false,
            RegistryVersion = registryVersion,
            Provenance = provenance,
        };
    }

    // --- Helpers ---

    /// <summary>
    /// Map Phase III BosonClaimClass enum to canonical unified claim class string.
    /// C2 maps to type-agnostic "C2_BranchStableCandidate" (BLOCKER-M42-1, BLOCKER-M42-2 fix).
    /// </summary>
    private static string BosonClaimToString(BosonClaimClass c) => c switch
    {
        BosonClaimClass.C0_NumericalMode => "C0_NumericalMode",
        BosonClaimClass.C1_LocalPersistentMode => "C1_LocalPersistentMode",
        BosonClaimClass.C2_BranchStableBosonicCandidate => "C2_BranchStableCandidate",
        BosonClaimClass.C3_ObservedStableCandidate => "C3_ObservedStableCandidate",
        BosonClaimClass.C4_PhysicalAnalogyCandidate => "C4_PhysicalAnalogyCandidate",
        BosonClaimClass.C5_StrongIdentificationCandidate => "C5_StrongIdentificationCandidate",
        _ => "C0_NumericalMode",
    };
}
