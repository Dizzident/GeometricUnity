using Gu.Core;
using Gu.Phase3.Registry;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;

namespace Gu.Phase4.Registry;

/// <summary>
/// Merge algorithm for the unified particle registry (M42 spec).
///
/// Accepts:
/// - Phase III BosonRegistry (CandidateBosonRecords)
/// - Phase IV FermionFamilyAtlas (FermionModeFamily clusters)
/// - Phase IV FamilyClusterRecord list (generation-like clusters from M41)
/// - Phase IV CouplingAtlas (BosonFermionCouplingRecords for Interaction candidates)
///
/// For each input source, builds a UnifiedParticleRecord by:
///   1. Aggregating provenance (source IDs, background set, branch variant set)
///   2. Computing branch stability, observation confidence, comparison evidence
///   3. Assigning claim class based on available evidence
///   4. Applying demotion rules:
///      - UnverifiedGpu: caps at C1
///      - LowPersistence: branch stability < threshold demotes from C2+ to C1
///      - LowObservation: no observation data demotes from C3+ to C2
///      - AmbiguousMatching: ambiguity notes present demotes by 1 level
///   5. Attaching demotion history
///
/// PhysicsNote: Claim class assignment is conservative.
/// See PhysicsNote in UnifiedParticleRegistry for details.
/// </summary>
public sealed class RegistryMergeEngine
{
    private readonly RegistryMergeConfig _config;

    public RegistryMergeEngine(RegistryMergeConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Build a UnifiedParticleRegistry from all available Phase III/IV sources.
    /// Any argument may be null (treated as empty).
    /// </summary>
    /// <param name="bosonRegistry">Phase III boson registry (optional).</param>
    /// <param name="familyClusters">Phase IV family cluster records (optional).</param>
    /// <param name="fermionAtlas">Phase IV fermion family atlas (optional, fallback when no clusters).</param>
    /// <param name="couplingAtlas">Phase IV coupling atlas (optional).</param>
    /// <param name="provenance">Provenance metadata.</param>
    /// <param name="observationConfidenceByClusterId">
    /// Lookup of cluster ID → observation confidence (e.g. BranchPersistenceScore from
    /// FermionObservationSummary). When provided, ObservationConfidence is populated from
    /// this lookup instead of defaulting to 0.0, and the LowObservation demotion rule fires
    /// for C3+ fermion candidates below MinObservationConfidence.
    /// </param>
    /// <param name="interactionConfidenceByBosonModeId">
    /// Lookup of boson mode ID → observation confidence for interaction records.
    /// When provided, ObservationConfidence for coupling-derived records is set from this lookup.
    /// </param>
    public UnifiedParticleRegistry Build(
        BosonRegistry? bosonRegistry,
        IReadOnlyList<FamilyClusterRecord>? familyClusters,
        FermionFamilyAtlas? fermionAtlas,
        CouplingAtlas? couplingAtlas,
        ProvenanceMeta provenance,
        IReadOnlyDictionary<string, double>? observationConfidenceByClusterId = null,
        IReadOnlyDictionary<string, double>? interactionConfidenceByBosonModeId = null)
    {
        ArgumentNullException.ThrowIfNull(provenance);

        var registry = new UnifiedParticleRegistry
        {
            RegistryVersion = _config.RegistryVersion,
        };

        // 1. Ingest bosons from Phase III
        if (bosonRegistry is not null)
        {
            foreach (var boson in bosonRegistry.Candidates)
            {
                var record = BuildFromBoson(boson, provenance);
                registry.Register(record);
            }
        }

        // 2. Ingest fermionic family clusters from M41
        if (familyClusters is not null)
        {
            foreach (var cluster in familyClusters)
            {
                var record = BuildFromFamilyCluster(cluster, provenance, observationConfidenceByClusterId);
                registry.Register(record);
            }
        }

        // 3. Ingest isolated fermion families from M39 atlas if no M41 clusters provided
        if (familyClusters is null && fermionAtlas is not null)
        {
            foreach (var family in fermionAtlas.Families)
            {
                var record = BuildFromFermionFamily(family, provenance, observationConfidenceByClusterId);
                registry.Register(record);
            }
        }

        // 4. Ingest interaction candidates from M40 coupling atlas
        if (couplingAtlas is not null)
        {
            foreach (var coupling in couplingAtlas.Couplings)
            {
                var record = BuildFromCoupling(coupling, provenance, interactionConfidenceByBosonModeId);
                registry.Register(record);
            }
        }

        return registry;
    }

    // ------------------------------------------------------------------
    // Per-source builders
    // ------------------------------------------------------------------

    private UnifiedParticleRecord BuildFromBoson(CandidateBosonRecord boson, ProvenanceMeta provenance)
    {
        var demotions = new List<ParticleClaimDemotion>();
        string claimClass = BosonClaimToString(boson.ClaimClass);

        // Inherit unverified GPU demotion
        if (boson.ComputedWithUnverifiedGpu && ParseLevel(claimClass) > 1)
        {
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "UnverifiedGpu",
                Details = "Candidate includes modes computed with an unverified GPU backend. Claim capped at C1.",
                FromClaimClass = claimClass,
                ToClaimClass = "C1_LocalPersistentMode",
            });
            claimClass = "C1_LocalPersistentMode";
        }

        // Ambiguous matching demotion
        int ambiguityCount = boson.AmbiguityCount + boson.AmbiguityNotes.Count;
        if (ambiguityCount > _config.AmbiguityCountThreshold && ParseLevel(claimClass) > 0)
        {
            string demotedClass = DemoteOneLevel(claimClass);
            if (demotedClass != claimClass)
            {
                demotions.Add(new ParticleClaimDemotion
                {
                    Reason = "AmbiguousMatching",
                    Details = $"{ambiguityCount} ambiguity note(s) present. Demoted by one level.",
                    FromClaimClass = claimClass,
                    ToClaimClass = demotedClass,
                });
                claimClass = demotedClass;
            }
        }

        // Observation confidence
        double observationConfidence = boson.ObservationStabilityScore;
        if (observationConfidence < _config.MinObservationConfidence && ParseLevel(claimClass) >= 3)
        {
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "LowObservation",
                Details = $"Observation stability {observationConfidence:F3} below threshold {_config.MinObservationConfidence:F3}.",
                FromClaimClass = claimClass,
                ToClaimClass = "C2_BranchStableCandidate",
            });
            claimClass = "C2_BranchStableCandidate";
        }

        return new UnifiedParticleRecord
        {
            ParticleId = $"unified-boson-{boson.CandidateId}",
            ParticleType = UnifiedParticleType.Boson,
            PrimarySourceId = boson.CandidateId,
            ContributingSourceIds = boson.ContributingModeIds.ToList(),
            BranchVariantSet = boson.BranchVariantSet.ToList(),
            BackgroundSet = boson.BackgroundSet.ToList(),
            Chirality = null,
            MassLikeEnvelope = boson.MassLikeEnvelope,
            BranchStabilityScore = boson.BranchStabilityScore,
            ObservationConfidence = boson.ObservationStabilityScore,
            ComparisonEvidenceScore = 0.0,
            ClaimClass = claimClass,
            ComputedWithUnverifiedGpu = boson.ComputedWithUnverifiedGpu,
            Demotions = demotions,
            AmbiguityNotes = boson.AmbiguityNotes.ToList(),
            RegistryVersion = _config.RegistryVersion,
            Provenance = provenance,
        };
    }

    private UnifiedParticleRecord BuildFromFamilyCluster(
        FamilyClusterRecord cluster,
        ProvenanceMeta provenance,
        IReadOnlyDictionary<string, double>? observationConfidenceByClusterId)
    {
        var demotions = new List<ParticleClaimDemotion>();

        // Start at C1 — a family cluster has local persistence
        string claimClass = "C1_LocalPersistentMode";

        // Upgrade to C2 if well-persisted
        if (cluster.MeanBranchPersistence >= _config.MinBranchPersistenceForC2)
            claimClass = "C2_BranchStableCandidate";

        // Downgrade if persistence is low
        if (cluster.MeanBranchPersistence < _config.MinBranchPersistenceThreshold && ParseLevel(claimClass) >= 2)
        {
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "LowPersistence",
                Details = $"Mean branch persistence {cluster.MeanBranchPersistence:F3} below threshold {_config.MinBranchPersistenceThreshold:F3}.",
                FromClaimClass = claimClass,
                ToClaimClass = "C1_LocalPersistentMode",
            });
            claimClass = "C1_LocalPersistentMode";
        }

        // Ambiguity demotion
        if (cluster.AmbiguityScore > _config.AmbiguityScoreThreshold && ParseLevel(claimClass) > 0)
        {
            string demotedClass = DemoteOneLevel(claimClass);
            if (demotedClass != claimClass)
            {
                demotions.Add(new ParticleClaimDemotion
                {
                    Reason = "AmbiguousMatching",
                    Details = $"Cluster ambiguity score {cluster.AmbiguityScore:F3} exceeds threshold {_config.AmbiguityScoreThreshold:F3}.",
                    FromClaimClass = claimClass,
                    ToClaimClass = demotedClass,
                });
                claimClass = demotedClass;
            }
        }

        // Observation confidence from FermionObservationSummary.BranchPersistenceScore (GAP-2)
        double observationConfidence = 0.0;
        bool hasObsData = false;
        if (observationConfidenceByClusterId is not null
            && observationConfidenceByClusterId.TryGetValue(cluster.ClusterId, out double confVal))
        {
            observationConfidence = confVal;
            hasObsData = true;
        }

        // LowObservation demotion: fires only when real observation data was supplied and confidence is low
        if (hasObsData && observationConfidence < _config.MinObservationConfidence && ParseLevel(claimClass) >= 3)
        {
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "LowObservation",
                Details = $"Observation confidence {observationConfidence:F3} below threshold {_config.MinObservationConfidence:F3}.",
                FromClaimClass = claimClass,
                ToClaimClass = "C2_BranchStableCandidate",
            });
            claimClass = "C2_BranchStableCandidate";
        }

        // Collect background/branch info from cluster's provenance (cluster doesn't have its own sets)
        var backgroundSet = new List<string> { cluster.Provenance.Branch?.BranchId ?? "unknown" };
        var branchVariantSet = new List<string> { cluster.Provenance.Branch?.BranchId ?? "unknown" };

        return new UnifiedParticleRecord
        {
            ParticleId = $"unified-fermion-{cluster.ClusterId}",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = cluster.ClusterId,
            ContributingSourceIds = cluster.MemberFamilyIds.ToList(),
            BranchVariantSet = branchVariantSet,
            BackgroundSet = backgroundSet,
            Chirality = cluster.DominantChirality,
            MassLikeEnvelope = cluster.EigenvalueMagnitudeEnvelope,
            BranchStabilityScore = cluster.MeanBranchPersistence,
            ObservationConfidence = observationConfidence,
            ComparisonEvidenceScore = 0.0,
            ClaimClass = claimClass,
            ComputedWithUnverifiedGpu = false,
            Demotions = demotions,
            AmbiguityNotes = cluster.ClusteringNotes.ToList(),
            RegistryVersion = _config.RegistryVersion,
            Provenance = provenance,
        };
    }

    private UnifiedParticleRecord BuildFromFermionFamily(
        FermionModeFamily family,
        ProvenanceMeta provenance,
        IReadOnlyDictionary<string, double>? observationConfidenceByClusterId)
    {
        var demotions = new List<ParticleClaimDemotion>();
        string claimClass = "C1_LocalPersistentMode";

        if (family.BranchPersistenceScore >= _config.MinBranchPersistenceForC2)
            claimClass = "C2_BranchStableCandidate";

        if (family.BranchPersistenceScore < _config.MinBranchPersistenceThreshold && ParseLevel(claimClass) >= 2)
        {
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "LowPersistence",
                Details = $"Branch persistence {family.BranchPersistenceScore:F3} below threshold.",
                FromClaimClass = claimClass,
                ToClaimClass = "C1_LocalPersistentMode",
            });
            claimClass = "C1_LocalPersistentMode";
        }

        // Observation confidence from lookup (GAP-2); family ID is used as the key for atlas fallback path
        double observationConfidence = 0.0;
        bool hasObsData = false;
        if (observationConfidenceByClusterId is not null
            && observationConfidenceByClusterId.TryGetValue(family.FamilyId, out double confVal))
        {
            observationConfidence = confVal;
            hasObsData = true;
        }

        // LowObservation demotion: fires only when real observation data was supplied and confidence is low
        if (hasObsData && observationConfidence < _config.MinObservationConfidence && ParseLevel(claimClass) >= 3)
        {
            demotions.Add(new ParticleClaimDemotion
            {
                Reason = "LowObservation",
                Details = $"Observation confidence {observationConfidence:F3} below threshold {_config.MinObservationConfidence:F3}.",
                FromClaimClass = claimClass,
                ToClaimClass = "C2_BranchStableCandidate",
            });
            claimClass = "C2_BranchStableCandidate";
        }

        double eigenMean = family.EigenvalueMagnitudeEnvelope[1];
        var envelope = new[] { family.EigenvalueMagnitudeEnvelope[0], eigenMean, family.EigenvalueMagnitudeEnvelope[2] };

        return new UnifiedParticleRecord
        {
            ParticleId = $"unified-fermion-{family.FamilyId}",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = family.FamilyId,
            ContributingSourceIds = family.MemberModeIds,
            BranchVariantSet = family.BranchVariantIds,
            BackgroundSet = family.BackgroundIds,
            Chirality = family.DominantChiralityProfile,
            MassLikeEnvelope = envelope,
            BranchStabilityScore = family.BranchPersistenceScore,
            ObservationConfidence = observationConfidence,
            ComparisonEvidenceScore = 0.0,
            ClaimClass = claimClass,
            ComputedWithUnverifiedGpu = false,
            Demotions = demotions,
            AmbiguityNotes = family.AmbiguityNotes,
            RegistryVersion = _config.RegistryVersion,
            Provenance = provenance,
        };
    }

    private UnifiedParticleRecord BuildFromCoupling(
        BosonFermionCouplingRecord coupling,
        ProvenanceMeta provenance,
        IReadOnlyDictionary<string, double>? interactionConfidenceByBosonModeId)
    {
        var demotions = new List<ParticleClaimDemotion>();

        // Interaction candidates start at C0 (proxy, not observed)
        string claimClass = "C0_NumericalMode";

        // Upgrade to C1 if coupling is stable across branches
        if (coupling.BranchStabilityScore >= _config.MinBranchStabilityForInteractionC1)
            claimClass = "C1_LocalPersistentMode";

        // Observation confidence from InteractionObservationSummary (GAP-2)
        double observationConfidence = 0.0;
        if (interactionConfidenceByBosonModeId is not null
            && interactionConfidenceByBosonModeId.TryGetValue(coupling.BosonModeId, out double confVal))
        {
            observationConfidence = confVal;
        }

        double couplingMag = coupling.CouplingProxyMagnitude;
        var envelope = new[] { couplingMag, couplingMag, couplingMag };

        return new UnifiedParticleRecord
        {
            ParticleId = $"unified-interaction-{coupling.CouplingId}",
            ParticleType = UnifiedParticleType.Interaction,
            PrimarySourceId = coupling.CouplingId,
            ContributingSourceIds = new List<string>
            {
                coupling.BosonModeId,
                coupling.FermionModeIdI,
                coupling.FermionModeIdJ,
            },
            BranchVariantSet = new List<string>(),
            BackgroundSet = new List<string> { coupling.Provenance.Branch?.BranchId ?? "unknown" },
            Chirality = null,
            MassLikeEnvelope = envelope,
            BranchStabilityScore = coupling.BranchStabilityScore,
            ObservationConfidence = observationConfidence,
            ComparisonEvidenceScore = 0.0,
            ClaimClass = claimClass,
            ComputedWithUnverifiedGpu = false,
            Demotions = demotions,
            AmbiguityNotes = coupling.SelectionRuleNotes,
            RegistryVersion = _config.RegistryVersion,
            Provenance = provenance,
        };
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

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

    private static int ParseLevel(string claimClass)
        => UnifiedParticleRegistry.ParseClaimClassLevel(claimClass);

    private static string DemoteOneLevel(string claimClass)
    {
        int level = ParseLevel(claimClass);
        if (level <= 0) return claimClass;
        return (level - 1) switch
        {
            0 => "C0_NumericalMode",
            1 => "C1_LocalPersistentMode",
            2 => "C2_BranchStableCandidate",
            3 => "C3_ObservedStableCandidate",
            4 => "C4_PhysicalAnalogyCandidate",
            _ => claimClass,
        };
    }
}
