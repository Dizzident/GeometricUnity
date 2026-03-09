using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gu.Phase2.Reporting;

/// <summary>
/// Dashboard-ready export of a research report.
/// Provides summary statistics and structured data suitable for
/// rendering in a web dashboard or JSON-based handoff.
/// </summary>
public sealed class DashboardExport
{
    /// <summary>Report ID.</summary>
    [JsonPropertyName("reportId")]
    public required string ReportId { get; init; }

    /// <summary>Batch ID.</summary>
    [JsonPropertyName("batchId")]
    public required string BatchId { get; init; }

    /// <summary>Summary counts by epistemic category.</summary>
    [JsonPropertyName("categoryCounts")]
    public required DashboardCategoryCounts CategoryCounts { get; init; }

    /// <summary>Number of open canonicity dockets.</summary>
    [JsonPropertyName("openDocketCount")]
    public required int OpenDocketCount { get; init; }

    /// <summary>Total branch runs across all sweeps.</summary>
    [JsonPropertyName("totalBranchRuns")]
    public required int TotalBranchRuns { get; init; }

    /// <summary>Timestamp of report generation.</summary>
    [JsonPropertyName("generatedAt")]
    public required DateTimeOffset GeneratedAt { get; init; }

    /// <summary>
    /// Create a dashboard export from a research report and batch result.
    /// </summary>
    public static DashboardExport FromReport(ResearchReport report, ResearchBatchResult batchResult)
    {
        var openDockets = report.Dockets.Count(d =>
            d.Status == Gu.Phase2.Canonicity.DocketStatus.Open);

        return new DashboardExport
        {
            ReportId = report.ReportId,
            BatchId = report.BatchId,
            CategoryCounts = new DashboardCategoryCounts
            {
                BranchLocal = report.BranchLocalConclusions.Count,
                ComparisonReady = report.ComparisonReadyConclusions.Count,
                Open = report.OpenItems.Count,
                NumericalOnly = report.NumericalOnlyResults.Count,
                Uninterpreted = report.UninterpretedOutputs.Count,
                RuledOut = report.RuledOutClaims.Count,
            },
            OpenDocketCount = openDockets,
            TotalBranchRuns = batchResult.TotalBranchRuns,
            GeneratedAt = report.GeneratedAt,
        };
    }

    /// <summary>
    /// Serialize to JSON string.
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });
    }
}

/// <summary>
/// Summary counts per epistemic category for dashboard display.
/// </summary>
public sealed class DashboardCategoryCounts
{
    [JsonPropertyName("branchLocal")]
    public required int BranchLocal { get; init; }

    [JsonPropertyName("comparisonReady")]
    public required int ComparisonReady { get; init; }

    [JsonPropertyName("open")]
    public required int Open { get; init; }

    [JsonPropertyName("numericalOnly")]
    public required int NumericalOnly { get; init; }

    [JsonPropertyName("uninterpreted")]
    public required int Uninterpreted { get; init; }

    [JsonPropertyName("ruledOut")]
    public required int RuledOut { get; init; }
}
