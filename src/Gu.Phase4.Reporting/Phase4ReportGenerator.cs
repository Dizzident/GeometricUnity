using Gu.Core;
using Gu.Phase4.Couplings;
using Gu.Phase4.FamilyClustering;
using Gu.Phase4.Fermions;
using Gu.Phase4.Registry;

namespace Gu.Phase4.Reporting;

/// <summary>
/// Generates a Phase4Report from a unified particle registry, fermion family atlas,
/// and coupling atlas.
///
/// Mirrors the Phase III BosonAtlasReportGenerator pattern.
/// Collects negative results from demotions and ambiguity entries from registry records.
/// </summary>
public sealed class Phase4ReportGenerator
{
    private const int TopCandidateLimit = 20;
    private const double NonzeroCouplingThreshold = 1e-10;
    private const double StableFamilyPersistenceThreshold = 0.5;

    /// <summary>
    /// Generate a Phase4Report.
    /// </summary>
    public Phase4Report Generate(
        string studyId,
        UnifiedParticleRegistry registry,
        FermionFamilyAtlas familyAtlas,
        CouplingAtlas couplingAtlas,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(familyAtlas);
        ArgumentNullException.ThrowIfNull(couplingAtlas);
        ArgumentNullException.ThrowIfNull(provenance);

        var fermionAtlasSummary = BuildFermionAtlasSummary(familyAtlas);
        var couplingAtlasSummary = BuildCouplingAtlasSummary(couplingAtlas);
        var registrySummary = BuildUnifiedRegistrySummary(registry);
        var negativeResults = CollectNegativeResults(registry);

        return new Phase4Report
        {
            ReportId = $"phase4-report-{studyId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            StudyId = studyId,
            FermionAtlas = fermionAtlasSummary,
            CouplingAtlas = couplingAtlasSummary,
            RegistrySummary = registrySummary,
            NegativeResults = negativeResults,
            Provenance = provenance,
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }

    // ------------------------------------------------------------------
    // Fermion atlas summary
    // ------------------------------------------------------------------

    private static FermionAtlasSummary BuildFermionAtlasSummary(FermionFamilyAtlas atlas)
    {
        var familySheets = new List<FermionFamilySheet>();
        var chiralitySummaries = new List<ChiralitySummaryEntry>();
        var conjugationSummaries = new List<ConjugationSummaryEntry>();

        foreach (var family in atlas.Families)
        {
            double meanEigenvalue = family.EigenvalueMagnitudeEnvelope.Length >= 2
                ? family.EigenvalueMagnitudeEnvelope[1] : 0.0;
            double spread = family.EigenvalueMagnitudeEnvelope.Length >= 3
                ? family.EigenvalueMagnitudeEnvelope[2] - family.EigenvalueMagnitudeEnvelope[0] : 0.0;

            familySheets.Add(new FermionFamilySheet
            {
                FamilyId = family.FamilyId,
                MeanEigenvalue = meanEigenvalue,
                EigenvalueSpread = spread,
                MemberCount = family.MemberModeIds.Count,
                IsStable = family.BranchPersistenceScore >= StableFamilyPersistenceThreshold,
                ClaimClass = family.BranchPersistenceScore >= 0.7 ? "C2_BranchStableCandidate" : "C1_LocalPersistentMode",
                MemberModeIds = family.MemberModeIds.ToList(),
            });

            chiralitySummaries.Add(BuildChiralitySummary(family));
            conjugationSummaries.Add(BuildConjugationSummary(family));
        }

        int stable = atlas.Families.Count(f => f.BranchPersistenceScore >= StableFamilyPersistenceThreshold);
        int ambiguous = atlas.Families.Count(f => f.AmbiguityNotes.Count > 0);

        return new FermionAtlasSummary
        {
            SummaryId = $"fermion-atlas-summary-{atlas.AtlasId}",
            FamilySheets = familySheets,
            ChiralitySummaries = chiralitySummaries,
            ConjugationSummaries = conjugationSummaries,
            TotalFamilies = atlas.Families.Count,
            StableFamilies = stable,
            AmbiguousFamilies = ambiguous,
        };
    }

    private static ChiralitySummaryEntry BuildChiralitySummary(FermionModeFamily family)
    {
        // Derive left/right projections from the dominant chirality profile.
        (double left, double right) = family.DominantChiralityProfile switch
        {
            "left"          => (0.9, 0.1),
            "right"         => (0.1, 0.9),
            "trivial"       => (0.5, 0.5),
            "conjugate-pair"=> (0.5, 0.5),
            _               => (0.5, 0.5), // mixed / undetermined
        };

        string status = family.DominantChiralityProfile switch
        {
            "left"           => "definite-left",
            "right"          => "definite-right",
            "trivial"        => "trivial",
            "conjugate-pair" => "mixed",
            "mixed"          => "mixed",
            _                => "not-applicable",
        };

        return new ChiralitySummaryEntry
        {
            FamilyId = family.FamilyId,
            LeftProjection = left,
            RightProjection = right,
            LeakageNorm = family.AverageGaugeLeakScore,
            ChiralityType = "Y",
            ChiralityStatus = status,
        };
    }

    private static ConjugationSummaryEntry BuildConjugationSummary(FermionModeFamily family)
    {
        bool hasPair = family.ConjugateFamilyId != null;
        return new ConjugationSummaryEntry
        {
            FamilyId = family.FamilyId,
            HasConjugatePair = hasPair,
            PairedFamilyId = family.ConjugateFamilyId,
            PairingScore = hasPair ? 1.0 : 0.0,
        };
    }

    // ------------------------------------------------------------------
    // Coupling atlas summary
    // ------------------------------------------------------------------

    private static CouplingAtlasSummary BuildCouplingAtlasSummary(CouplingAtlas atlas)
    {
        var byBoson = atlas.Couplings
            .GroupBy(c => c.BosonModeId)
            .ToList();

        var matrices = new List<CouplingMatrixSummary>(byBoson.Count);
        int totalCouplings = 0;
        int nonzero = 0;
        double maxMagnitude = 0.0;

        foreach (var group in byBoson)
        {
            var entries = group.ToList();
            totalCouplings += entries.Count;

            double maxEntry = 0.0;
            double frobSq = 0.0;
            int nzCount = 0;

            foreach (var c in entries)
            {
                double mag = c.CouplingProxyMagnitude;
                if (mag > NonzeroCouplingThreshold) nzCount++;
                if (mag > maxEntry) maxEntry = mag;
                frobSq += mag * mag;
                if (mag > maxMagnitude) maxMagnitude = mag;
            }

            nonzero += nzCount;

            matrices.Add(new CouplingMatrixSummary
            {
                BosonModeId = group.Key,
                FermionPairCount = entries.Count,
                MaxEntry = maxEntry,
                FrobeniusNorm = System.Math.Sqrt(frobSq),
            });
        }

        return new CouplingAtlasSummary
        {
            SummaryId = $"coupling-summary-{atlas.AtlasId}",
            CouplingMatrices = matrices,
            TotalCouplings = totalCouplings,
            NonzeroCouplings = nonzero,
            MaxCouplingMagnitude = maxMagnitude,
        };
    }

    // ------------------------------------------------------------------
    // Unified registry summary
    // ------------------------------------------------------------------

    private static UnifiedRegistrySummary BuildUnifiedRegistrySummary(UnifiedParticleRegistry registry)
    {
        int bosons = registry.Candidates.Count(c => c.ParticleType == UnifiedParticleType.Boson);
        int fermions = registry.Candidates.Count(c => c.ParticleType == UnifiedParticleType.Fermion);
        int interactions = registry.Candidates.Count(c => c.ParticleType == UnifiedParticleType.Interaction);

        var claimClassCounts = new Dictionary<string, int>();
        foreach (var c in registry.Candidates)
        {
            string key = c.ClaimClass.Length >= 2 ? c.ClaimClass[..2] : c.ClaimClass;
            claimClassCounts.TryGetValue(key, out int existing);
            claimClassCounts[key] = existing + 1;
        }

        var topCandidates = registry.Candidates
            .OrderByDescending(c => UnifiedParticleRegistry.ParseClaimClassLevel(c.ClaimClass))
            .ThenBy(c => c.MassLikeEnvelope.Length >= 2 ? c.MassLikeEnvelope[1] : 0.0)
            .Take(TopCandidateLimit)
            .Select(c => new CandidateParticleSummary
            {
                CandidateId = c.ParticleId,
                ParticleType = c.ParticleType.ToString(),
                ClaimClass = c.ClaimClass,
                MassLikeValue = c.MassLikeEnvelope.Length >= 2 ? c.MassLikeEnvelope[1] : 0.0,
                DemotionCount = c.Demotions.Count,
            })
            .ToList();

        return new UnifiedRegistrySummary
        {
            SummaryId = $"registry-summary-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            TotalBosons = bosons,
            TotalFermions = fermions,
            TotalInteractions = interactions,
            ClaimClassCounts = claimClassCounts,
            TopCandidates = topCandidates,
        };
    }

    // ------------------------------------------------------------------
    // Negative results
    // ------------------------------------------------------------------

    private static List<string> CollectNegativeResults(UnifiedParticleRegistry registry)
    {
        var results = new List<string>();

        foreach (var candidate in registry.Candidates)
        {
            foreach (var demotion in candidate.Demotions)
            {
                results.Add(
                    $"[demotion] {candidate.ParticleId} ({candidate.ParticleType}): " +
                    $"{demotion.Reason} — {demotion.Details} " +
                    $"({demotion.FromClaimClass} → {demotion.ToClaimClass})");
            }

            if (candidate.AmbiguityNotes.Count > 0)
            {
                foreach (var note in candidate.AmbiguityNotes)
                {
                    results.Add($"[ambiguity] {candidate.ParticleId}: {note}");
                }
            }
        }

        return results;
    }
}
