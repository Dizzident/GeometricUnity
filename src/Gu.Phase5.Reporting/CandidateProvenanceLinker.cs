using Gu.Phase4.Registry;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Applies optional candidate-specific provenance links to a registry without removing
/// any pre-existing registry identities.
/// </summary>
public static class CandidateProvenanceLinker
{
    public static UnifiedParticleRegistry Apply(
        UnifiedParticleRegistry registry,
        IReadOnlyList<CandidateProvenanceLinkRecord>? links)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (links is null || links.Count == 0)
            return registry;

        var linksByCandidateId = links
            .Where(link => !string.IsNullOrWhiteSpace(link.CandidateId))
            .GroupBy(link => link.CandidateId, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.Ordinal);

        var enrichedCandidates = registry.Candidates
            .Select(candidate =>
            {
                if (!linksByCandidateId.TryGetValue(candidate.ParticleId, out var candidateLinks))
                    return candidate;

                var branchVariantSet = candidate.BranchVariantSet
                    .Concat(candidateLinks.SelectMany(link => link.BranchVariantIds))
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList();

                var backgroundSet = candidate.BackgroundSet
                    .Concat(candidateLinks.SelectMany(link => link.BackgroundIds))
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Distinct(StringComparer.Ordinal)
                    .OrderBy(id => id, StringComparer.Ordinal)
                    .ToList();

                return new UnifiedParticleRecord
                {
                    ParticleId = candidate.ParticleId,
                    SchemaVersion = candidate.SchemaVersion,
                    ParticleType = candidate.ParticleType,
                    PrimarySourceId = candidate.PrimarySourceId,
                    ContributingSourceIds = candidate.ContributingSourceIds.ToList(),
                    BranchVariantSet = branchVariantSet,
                    BackgroundSet = backgroundSet,
                    Chirality = candidate.Chirality,
                    MassLikeEnvelope = candidate.MassLikeEnvelope.ToArray(),
                    BranchStabilityScore = candidate.BranchStabilityScore,
                    ObservationConfidence = candidate.ObservationConfidence,
                    ComparisonEvidenceScore = candidate.ComparisonEvidenceScore,
                    ClaimClass = candidate.ClaimClass,
                    ComputedWithUnverifiedGpu = candidate.ComputedWithUnverifiedGpu,
                    Demotions = candidate.Demotions.ToList(),
                    AmbiguityNotes = candidate.AmbiguityNotes.ToList(),
                    RegistryVersion = candidate.RegistryVersion,
                    Provenance = candidate.Provenance,
                };
            })
            .ToList();

        return new UnifiedParticleRegistry
        {
            RegistryVersion = registry.RegistryVersion,
            SchemaVersion = registry.SchemaVersion,
            Candidates = enrichedCandidates,
        };
    }
}
