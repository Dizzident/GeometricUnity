using Gu.Phase3.Registry;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Executes boson comparison campaigns against a registry.
/// Supports both BC1 (internal target profile) and BC2 (external analogy) modes.
/// All results -- including negative, underdetermined, and insufficient evidence --
/// are first-class outputs.
/// </summary>
public sealed class BosonCampaignRunner
{
    private readonly BosonProfileMatcher _matcher = new();

    /// <summary>
    /// Run a campaign against the given registry.
    /// </summary>
    public BosonCampaignResult Run(
        BosonCampaignSpec spec,
        BosonRegistry registry,
        IReadOnlyDictionary<string, BosonTargetProfile> profiles,
        IReadOnlyDictionary<string, ExternalAnalogyDescriptor> descriptors)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(profiles);
        ArgumentNullException.ThrowIfNull(descriptors);

        // Select candidates meeting the minimum claim class
        var candidates = registry.QueryByClaimClass(spec.MinClaimClass);
        if (!spec.IncludeDemoted)
        {
            candidates = candidates.Where(c => c.Demotions.Count == 0).ToList();
        }

        var allResults = new List<BosonComparisonResult>();
        var negativeResults = new List<BosonComparisonResult>();
        int targetsUsed = 0;

        switch (spec.Mode)
        {
            case BosonComparisonMode.InternalTargetProfile:
                targetsUsed = RunBC1(spec, candidates, profiles, allResults, negativeResults);
                break;

            case BosonComparisonMode.ExternalAnalogy:
                targetsUsed = RunBC2(spec, candidates, descriptors, allResults, negativeResults);
                break;
        }

        return new BosonCampaignResult
        {
            CampaignId = spec.CampaignId,
            Mode = spec.Mode,
            Results = allResults,
            NegativeResults = negativeResults,
            CandidatesCompared = candidates.Count,
            TargetsUsed = targetsUsed,
            CompletedAt = DateTimeOffset.UtcNow,
        };
    }

    private int RunBC1(
        BosonCampaignSpec spec,
        IReadOnlyList<CandidateBosonRecord> candidates,
        IReadOnlyDictionary<string, BosonTargetProfile> profiles,
        List<BosonComparisonResult> allResults,
        List<BosonComparisonResult> negativeResults)
    {
        int targetsUsed = 0;

        foreach (var profileId in spec.TargetProfileIds)
        {
            if (!profiles.TryGetValue(profileId, out var profile))
                continue;

            targetsUsed++;

            foreach (var candidate in candidates)
            {
                var result = _matcher.CompareToProfile(candidate, profile);
                allResults.Add(result);

                if (result.Verdict != ComparisonVerdict.Compatible)
                    negativeResults.Add(result);
            }
        }

        return targetsUsed;
    }

    private int RunBC2(
        BosonCampaignSpec spec,
        IReadOnlyList<CandidateBosonRecord> candidates,
        IReadOnlyDictionary<string, ExternalAnalogyDescriptor> descriptors,
        List<BosonComparisonResult> allResults,
        List<BosonComparisonResult> negativeResults)
    {
        int targetsUsed = 0;

        foreach (var descId in spec.ExternalDescriptorIds)
        {
            if (!descriptors.TryGetValue(descId, out var descriptor))
                continue;

            targetsUsed++;

            foreach (var candidate in candidates)
            {
                var result = _matcher.CompareToExternal(candidate, descriptor);
                allResults.Add(result);

                if (result.Verdict != ComparisonVerdict.Compatible)
                    negativeResults.Add(result);
            }
        }

        return targetsUsed;
    }
}
