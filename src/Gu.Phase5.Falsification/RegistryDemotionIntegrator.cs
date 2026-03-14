using Gu.Phase4.Registry;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Applies falsifier-driven demotions to a UnifiedParticleRegistry (M50).
///
/// Demotion rules (architect-confirmed):
///   Fatal  → cap affected candidate at C0 (regardless of current claim class)
///   High   → demote by 2 claim class levels (minimum C0)
///   Medium → demote by 1 claim class level  (minimum C0)
///   Low    → warning only (no demotion)
///
/// Only active falsifiers are applied. Inactive falsifiers are ignored.
/// Produces a new registry instance with updated records; never mutates in-place.
/// </summary>
public static class RegistryDemotionIntegrator
{
    /// <summary>
    /// Apply all active falsifiers to the registry and return a new registry instance.
    /// </summary>
    public static UnifiedParticleRegistry ApplyDemotions(
        UnifiedParticleRegistry registry,
        FalsifierSummary falsifiers)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(falsifiers);

        // Only active falsifiers drive demotions
        var activeFalsifiers = falsifiers.Falsifiers
            .Where(f => f.Active)
            .ToList();

        if (activeFalsifiers.Count == 0)
            return registry; // no changes needed

        var updatedCandidates = new List<UnifiedParticleRecord>(registry.Candidates.Count);
        foreach (var candidate in registry.Candidates)
        {
            var applicable = activeFalsifiers
                .Where(f => Affects(f, candidate))
                .ToList();

            if (applicable.Count == 0)
            {
                updatedCandidates.Add(candidate);
                continue;
            }

            int currentLevel = UnifiedParticleRegistry.ParseClaimClassLevel(candidate.ClaimClass);
            int demotedLevel = currentLevel;
            var newDemotions = new List<ParticleClaimDemotion>();

            foreach (var falsifier in applicable)
            {
                int targetLevel = demotedLevel;
                switch (falsifier.Severity)
                {
                    case FalsifierSeverity.Fatal:
                        targetLevel = 0; // cap at C0
                        break;
                    case FalsifierSeverity.High:
                        targetLevel = System.Math.Max(0, demotedLevel - 2);
                        break;
                    case FalsifierSeverity.Medium:
                        targetLevel = System.Math.Max(0, demotedLevel - 1);
                        break;
                    case FalsifierSeverity.Low:
                    case FalsifierSeverity.Informational:
                        // No demotion — only warning
                        continue;
                }

                if (targetLevel < demotedLevel)
                {
                    newDemotions.Add(new ParticleClaimDemotion
                    {
                        Reason = $"Phase5Falsifier-{falsifier.FalsifierType}",
                        Details = falsifier.Description,
                        FromClaimClass = ClaimClassString(demotedLevel),
                        ToClaimClass = ClaimClassString(targetLevel),
                        DemotedAt = DateTimeOffset.UtcNow,
                    });
                    demotedLevel = targetLevel;
                }
            }

            if (newDemotions.Count == 0)
            {
                updatedCandidates.Add(candidate);
                continue;
            }

            // Build updated candidate with new demotions merged
            var allDemotions = new List<ParticleClaimDemotion>(candidate.Demotions);
            allDemotions.AddRange(newDemotions);

            var updated = new UnifiedParticleRecord
            {
                ParticleId = candidate.ParticleId,
                SchemaVersion = candidate.SchemaVersion,
                ParticleType = candidate.ParticleType,
                PrimarySourceId = candidate.PrimarySourceId,
                ContributingSourceIds = candidate.ContributingSourceIds,
                BranchVariantSet = candidate.BranchVariantSet,
                BackgroundSet = candidate.BackgroundSet,
                Chirality = candidate.Chirality,
                MassLikeEnvelope = candidate.MassLikeEnvelope,
                BranchStabilityScore = candidate.BranchStabilityScore,
                ObservationConfidence = candidate.ObservationConfidence,
                ComparisonEvidenceScore = candidate.ComparisonEvidenceScore,
                ClaimClass = ClaimClassString(demotedLevel),
                ComputedWithUnverifiedGpu = candidate.ComputedWithUnverifiedGpu,
                Demotions = allDemotions,
                AmbiguityNotes = candidate.AmbiguityNotes,
                RegistryVersion = candidate.RegistryVersion,
                Provenance = candidate.Provenance,
            };
            updatedCandidates.Add(updated);
        }

        var newRegistry = new UnifiedParticleRegistry();
        foreach (var c in updatedCandidates)
            newRegistry.Register(c);
        return newRegistry;
    }

    // ------------------------------------------------------------------
    // Helpers
    // ------------------------------------------------------------------

    /// <summary>
    /// Determines whether a falsifier affects a given candidate.
    /// A falsifier affects a candidate if:
    ///   - The falsifier's TargetId matches the candidate's ParticleId or PrimarySourceId, OR
    ///   - The falsifier is branch-level (targets "unknown" or empty) and severity is Fatal
    ///     (Fatal falsifiers affect the entire registry).
    /// </summary>
    private static bool Affects(FalsifierRecord falsifier, UnifiedParticleRecord candidate)
    {
        // Direct match
        if (falsifier.TargetId == candidate.ParticleId ||
            falsifier.TargetId == candidate.PrimarySourceId)
            return true;

        // Fatal falsifiers with no specific target affect all candidates
        if (falsifier.Severity == FalsifierSeverity.Fatal &&
            (falsifier.TargetId == "unknown" || string.IsNullOrEmpty(falsifier.TargetId)))
            return true;

        return false;
    }

    private static string ClaimClassString(int level) => level switch
    {
        0 => "C0_NumericalMode",
        1 => "C1_NumericalHint",
        2 => "C2_ReproducibleMode",
        3 => "C3_BranchStableCandidate",
        4 => "C4_ObservationConsistentCandidate",
        5 => "C5_StrongIdentificationCandidate",
        _ => $"C{System.Math.Max(0, level)}_Unknown",
    };
}
