using System.Text.Json.Serialization;

namespace Gu.Phase2.Reporting;

/// <summary>
/// JSON-serializable dashboard summary for a research batch.
/// Provides a structured overview suitable for external consumption
/// or next-session handoff without the original manuscript.
/// </summary>
public sealed class BatchDashboardExport
{
    /// <summary>Report ID this dashboard was generated from.</summary>
    [JsonPropertyName("reportId")]
    public required string ReportId { get; init; }

    /// <summary>Batch ID.</summary>
    [JsonPropertyName("batchId")]
    public required string BatchId { get; init; }

    /// <summary>Number of branch sweep studies executed.</summary>
    [JsonPropertyName("sweepCount")]
    public required int SweepCount { get; init; }

    /// <summary>Number of stability studies executed.</summary>
    [JsonPropertyName("stabilityStudyCount")]
    public required int StabilityStudyCount { get; init; }

    /// <summary>Number of comparison campaigns referenced.</summary>
    [JsonPropertyName("campaignCount")]
    public required int CampaignCount { get; init; }

    /// <summary>Total branch runs across all sweeps.</summary>
    [JsonPropertyName("totalBranchRuns")]
    public required int TotalBranchRuns { get; init; }

    /// <summary>Count of branch-local conclusions.</summary>
    [JsonPropertyName("branchLocalCount")]
    public required int BranchLocalCount { get; init; }

    /// <summary>Count of comparison-ready conclusions.</summary>
    [JsonPropertyName("comparisonReadyCount")]
    public required int ComparisonReadyCount { get; init; }

    /// <summary>Count of open items requiring follow-up.</summary>
    [JsonPropertyName("openItemCount")]
    public required int OpenItemCount { get; init; }

    /// <summary>Count of numerical-only results.</summary>
    [JsonPropertyName("numericalOnlyCount")]
    public required int NumericalOnlyCount { get; init; }

    /// <summary>Count of uninterpreted outputs.</summary>
    [JsonPropertyName("uninterpretedCount")]
    public required int UninterpretedCount { get; init; }

    /// <summary>Count of ruled-out claims.</summary>
    [JsonPropertyName("ruledOutCount")]
    public required int RuledOutCount { get; init; }

    /// <summary>Count of canonicity dockets included.</summary>
    [JsonPropertyName("docketCount")]
    public required int DocketCount { get; init; }

    /// <summary>Count of open/accumulating dockets (not yet closed).</summary>
    [JsonPropertyName("openDocketCount")]
    public required int OpenDocketCount { get; init; }

    /// <summary>Whether the batch completed without errors.</summary>
    [JsonPropertyName("batchSucceeded")]
    public required bool BatchSucceeded { get; init; }

    /// <summary>Timestamp when this export was generated.</summary>
    [JsonPropertyName("exportedAt")]
    public required DateTimeOffset ExportedAt { get; init; }

    /// <summary>
    /// Create a dashboard export from a research report and batch result.
    /// </summary>
    public static BatchDashboardExport FromReport(ResearchReport report, ResearchBatchResult batchResult)
    {
        ArgumentNullException.ThrowIfNull(report);
        ArgumentNullException.ThrowIfNull(batchResult);

        return new BatchDashboardExport
        {
            ReportId = report.ReportId,
            BatchId = report.BatchId,
            SweepCount = batchResult.SweepResults.Count,
            StabilityStudyCount = batchResult.StabilityResults.Count,
            CampaignCount = batchResult.ExecutedCampaignIds.Count,
            TotalBranchRuns = batchResult.TotalBranchRuns,
            BranchLocalCount = report.BranchLocalConclusions.Count,
            ComparisonReadyCount = report.ComparisonReadyConclusions.Count,
            OpenItemCount = report.OpenItems.Count,
            NumericalOnlyCount = report.NumericalOnlyResults.Count,
            UninterpretedCount = report.UninterpretedOutputs.Count,
            RuledOutCount = report.RuledOutClaims.Count,
            DocketCount = report.Dockets.Count,
            OpenDocketCount = report.Dockets.Count(d =>
                d.Status == Canonicity.DocketStatus.Open ||
                d.Status == Canonicity.DocketStatus.EvidenceAccumulating),
            BatchSucceeded = batchResult.AllSucceeded,
            ExportedAt = DateTimeOffset.UtcNow,
        };
    }
}
