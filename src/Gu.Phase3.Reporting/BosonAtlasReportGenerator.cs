using System.Text.Json;
using System.Text.Json.Serialization;
using Gu.Phase3.Campaigns;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Reporting;

/// <summary>
/// Generates a complete boson atlas report from a registry, spectrum sheets,
/// and campaign results. Uses ReportBuilder internally but adds campaign-derived
/// negative results and ambiguity entries automatically.
/// </summary>
public sealed class BosonAtlasReportGenerator
{
    /// <summary>
    /// Generate a boson atlas report from a registry and campaign results.
    /// Spectrum sheets must be provided externally since they are per-background.
    /// </summary>
    public BosonAtlasReport Generate(
        string studyId,
        BosonRegistry registry,
        IReadOnlyList<BosonCampaignResult> campaignResults,
        IReadOnlyList<SpectrumSheet>? spectrumSheets = null)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(campaignResults);

        var builder = new ReportBuilder(studyId);

        // Add spectrum sheets if provided
        if (spectrumSheets != null)
        {
            foreach (var sheet in spectrumSheets)
                builder.AddSpectrumSheet(sheet);
        }

        // Add campaign results
        foreach (var result in campaignResults)
            builder.AddCampaignResult(result);

        // Add negative results from campaigns
        foreach (var campaign in campaignResults)
        {
            foreach (var neg in campaign.NegativeResults)
            {
                string resultType = neg.Verdict switch
                {
                    ComparisonVerdict.Incompatible => "comparison-incompatible",
                    ComparisonVerdict.Underdetermined => "comparison-underdetermined",
                    ComparisonVerdict.InsufficientEvidence => "insufficient-evidence",
                    _ => "comparison-negative",
                };

                builder.AddNegativeResult(new NegativeResultSummary
                {
                    CandidateId = neg.CandidateId,
                    ResultType = resultType,
                    Description = $"Comparison with {neg.TargetId}: {string.Join("; ", neg.Notes)}",
                });
            }
        }

        // Add negative results from demotions in registry
        foreach (var candidate in registry.Candidates)
        {
            foreach (var demotion in candidate.Demotions)
            {
                builder.AddNegativeResult(new NegativeResultSummary
                {
                    CandidateId = candidate.CandidateId,
                    ResultType = "demotion",
                    Description = $"{demotion.Reason}: {demotion.Details}",
                    OriginalClaimClass = demotion.PreviousClaimClass,
                    FinalClaimClass = demotion.DemotedClaimClass,
                });
            }
        }

        return builder.Build(registry);
    }

    /// <summary>
    /// Serialize a report to JSON.
    /// </summary>
    public static string ToJson(BosonAtlasReport report, bool indented = true)
    {
        return JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = indented,
            Converters = { new JsonStringEnumConverter() },
        });
    }
}
