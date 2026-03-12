using Gu.Core;

namespace Gu.Phase4.FamilyClustering;

/// <summary>
/// Builds a FermionFamilyAtlas from a set of named spectral results.
///
/// Orchestrates FermionFamilyBuilder, computes summary statistics,
/// and resolves conjugation-pair relationships between families.
/// </summary>
public sealed class FermionFamilyAtlasBuilder
{
    private readonly FermionTrackingConfig _config;

    public FermionFamilyAtlasBuilder(FermionTrackingConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Build a FermionFamilyAtlas from multiple spectral contexts.
    /// </summary>
    public FermionFamilyAtlas Build(
        string atlasId,
        string branchFamilyId,
        IReadOnlyList<NamedSpectralResult> results,
        ProvenanceMeta provenance)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(provenance);

        var familyBuilder = new FermionFamilyBuilder(_config);
        var families = familyBuilder.Build(results, provenance);

        // Resolve conjugation pairs between families
        families = ResolveConjugatePairs(families);

        // Compute summary
        var summary = ComputeSummary(families, results.Count);

        var contextIds = results.Select(r => r.ContextId).Distinct().ToList();
        var backgroundIds = results.Select(r => r.BackgroundId).Distinct().ToList();

        return new FermionFamilyAtlas
        {
            AtlasId = atlasId,
            BranchFamilyId = branchFamilyId,
            ContextIds = contextIds,
            BackgroundIds = backgroundIds,
            Families = families.ToList(),
            Summary = summary,
            Provenance = provenance,
        };
    }

    /// <summary>
    /// Resolve conjugation pairs between families.
    /// A conjugation pair is a pair of families where one has a dominant "left"
    /// chirality profile and the other "right", with similar eigenvalue envelopes.
    /// </summary>
    private static IReadOnlyList<Fermions.FermionModeFamily> ResolveConjugatePairs(
        IReadOnlyList<Fermions.FermionModeFamily> families)
    {
        // Build mutable copies with conjugate family IDs set
        var result = families.ToList();
        var paired = new HashSet<int>();

        for (int i = 0; i < result.Count; i++)
        {
            if (paired.Contains(i)) continue;
            var fi = result[i];

            for (int j = i + 1; j < result.Count; j++)
            {
                if (paired.Contains(j)) continue;
                var fj = result[j];

                if (AreLikelyCPConjugates(fi, fj))
                {
                    // Rebuild with conjugate pointers
                    result[i] = RebuildWithConjugate(fi, fj.FamilyId);
                    result[j] = RebuildWithConjugate(fj, fi.FamilyId);
                    paired.Add(i);
                    paired.Add(j);
                    break;
                }
            }
        }

        return result;
    }

    private static bool AreLikelyCPConjugates(
        Fermions.FermionModeFamily a,
        Fermions.FermionModeFamily b)
    {
        // Conjugates: opposite dominant chirality
        bool oppositeChirality =
            (a.DominantChiralityProfile == "left" && b.DominantChiralityProfile == "right") ||
            (a.DominantChiralityProfile == "right" && b.DominantChiralityProfile == "left");

        if (!oppositeChirality) return false;

        // Similar eigenvalue scale
        double aMean = a.EigenvalueMagnitudeEnvelope[1];
        double bMean = b.EigenvalueMagnitudeEnvelope[1];
        double scale = System.Math.Max(System.Math.Max(aMean, bMean), 1e-14);
        double relDiff = System.Math.Abs(aMean - bMean) / scale;
        return relDiff < 0.5;
    }

    private static Fermions.FermionModeFamily RebuildWithConjugate(
        Fermions.FermionModeFamily family,
        string conjugateFamilyId)
    {
        return new Fermions.FermionModeFamily
        {
            FamilyId = family.FamilyId,
            MemberModeIds = family.MemberModeIds,
            BackgroundIds = family.BackgroundIds,
            BranchVariantIds = family.BranchVariantIds,
            EigenvalueMagnitudeEnvelope = family.EigenvalueMagnitudeEnvelope,
            DominantChiralityProfile = family.DominantChiralityProfile,
            HasConjugationPair = true,
            ConjugateFamilyId = conjugateFamilyId,
            BranchPersistenceScore = family.BranchPersistenceScore,
            RefinementPersistenceScore = family.RefinementPersistenceScore,
            AverageGaugeLeakScore = family.AverageGaugeLeakScore,
            AmbiguityNotes = family.AmbiguityNotes,
            Provenance = family.Provenance,
        };
    }

    private static FermionAtlasSummary ComputeSummary(
        IReadOnlyList<Fermions.FermionModeFamily> families,
        int contextCount)
    {
        if (families.Count == 0)
            return new FermionAtlasSummary { Notes = new List<string> { "No families found." } };

        int persistent = families.Count(f => f.BranchPersistenceScore > 0.5);
        int left = families.Count(f => f.DominantChiralityProfile == "left");
        int right = families.Count(f => f.DominantChiralityProfile == "right");
        int mixed = families.Count(f => f.DominantChiralityProfile is "mixed" or "undetermined");
        int withPair = families.Count(f => f.HasConjugationPair);
        double meanPersistence = families.Average(f => f.BranchPersistenceScore);

        var notes = new List<string>();
        if (contextCount == 1)
            notes.Add("Only one spectral context provided; persistence scores reflect single-context coverage.");

        return new FermionAtlasSummary
        {
            TotalFamilies = families.Count,
            PersistentFamilies = persistent,
            LeftChiralFamilies = left,
            RightChiralFamilies = right,
            MixedChiralFamilies = mixed,
            FamiliesWithConjugatePair = withPair,
            MeanBranchPersistence = meanPersistence,
            Notes = notes,
        };
    }
}
