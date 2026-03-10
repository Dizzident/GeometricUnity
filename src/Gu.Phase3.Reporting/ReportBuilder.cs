using Gu.Phase3.Campaigns;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Reporting;

/// <summary>
/// Builds a <see cref="BosonAtlasReport"/> from registry data, campaign results,
/// and stability information.
/// </summary>
public sealed class ReportBuilder
{
    private readonly string _studyId;
    private readonly List<SpectrumSheet> _spectrumSheets = new();
    private readonly List<BosonCampaignResult> _campaignResults = new();
    private readonly List<NegativeResultSummary> _negativeResults = new();

    public ReportBuilder(string studyId)
    {
        ArgumentNullException.ThrowIfNull(studyId);
        _studyId = studyId;
    }

    /// <summary>Add a spectrum sheet.</summary>
    public ReportBuilder AddSpectrumSheet(SpectrumSheet sheet)
    {
        ArgumentNullException.ThrowIfNull(sheet);
        _spectrumSheets.Add(sheet);
        return this;
    }

    /// <summary>Add a campaign result.</summary>
    public ReportBuilder AddCampaignResult(BosonCampaignResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        _campaignResults.Add(result);
        return this;
    }

    /// <summary>Add a negative result summary.</summary>
    public ReportBuilder AddNegativeResult(NegativeResultSummary entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _negativeResults.Add(entry);
        return this;
    }

    /// <summary>
    /// Build the boson atlas report from a registry and accumulated data.
    /// </summary>
    public BosonAtlasReport Build(BosonRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var stabilitySummaries = BuildStabilitySummaries(registry);
        var ambiguityEntries = BuildAmbiguityEntries(registry);
        var claimClassCounts = BuildClaimClassCounts(registry);

        return new BosonAtlasReport
        {
            ReportId = $"report-{_studyId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            StudyId = _studyId,
            RegistryVersion = registry.RegistryVersion,
            SpectrumSheets = _spectrumSheets.ToList(),
            StabilitySummaries = stabilitySummaries,
            AmbiguityEntries = ambiguityEntries,
            NegativeResults = _negativeResults.ToList(),
            CampaignResults = _campaignResults.ToList(),
            TotalCandidates = registry.Count,
            ClaimClassCounts = claimClassCounts,
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }

    private static Dictionary<string, int> BuildClaimClassCounts(BosonRegistry registry)
    {
        var counts = new Dictionary<string, int>();
        foreach (BosonClaimClass cls in Enum.GetValues<BosonClaimClass>())
        {
            int count = registry.Candidates.Count(c => c.ClaimClass == cls);
            if (count > 0)
            {
                counts[cls.ToString()] = count;
            }
        }
        return counts;
    }

    private static List<StabilitySummary> BuildStabilitySummaries(BosonRegistry registry)
    {
        var summaries = new List<StabilitySummary>();
        foreach (var candidate in registry.Candidates)
        {
            double minScore = System.Math.Min(
                System.Math.Min(candidate.BranchStabilityScore, candidate.RefinementStabilityScore),
                System.Math.Min(candidate.BackendStabilityScore, candidate.ObservationStabilityScore));

            string assessment = minScore >= 0.8 ? "stable" :
                                minScore >= 0.5 ? "fragile" : "unstable";

            summaries.Add(new StabilitySummary
            {
                CandidateId = candidate.CandidateId,
                BranchStability = candidate.BranchStabilityScore,
                RefinementStability = candidate.RefinementStabilityScore,
                BackendStability = candidate.BackendStabilityScore,
                ObservationStability = candidate.ObservationStabilityScore,
                DemotionCount = candidate.Demotions.Count,
                CurrentClaimClass = candidate.ClaimClass,
                OverallAssessment = assessment,
            });
        }

        return summaries;
    }

    private static List<AmbiguityMapEntry> BuildAmbiguityEntries(BosonRegistry registry)
    {
        var entries = new List<AmbiguityMapEntry>();

        foreach (var candidate in registry.Candidates)
        {
            if (candidate.AmbiguityNotes.Count > 0)
            {
                entries.Add(new AmbiguityMapEntry
                {
                    CandidateId = candidate.CandidateId,
                    AmbiguityType = "degenerate-candidates",
                    Notes = candidate.AmbiguityNotes.ToList(),
                    AlternativeCount = candidate.AmbiguityNotes.Count,
                });
            }
        }

        return entries;
    }
}
