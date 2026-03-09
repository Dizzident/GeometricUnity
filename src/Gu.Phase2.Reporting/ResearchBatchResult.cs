using System.Text.Json.Serialization;
using Gu.Phase2.Continuation;
using Gu.Phase2.Execution;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Reporting;

/// <summary>
/// Result of executing a research batch.
/// Contains per-sweep results, per-study stability results,
/// and aggregate metadata for report generation.
/// </summary>
public sealed class ResearchBatchResult
{
    /// <summary>The batch spec that was executed.</summary>
    [JsonPropertyName("spec")]
    public required ResearchBatchSpec Spec { get; init; }

    /// <summary>Per-sweep results, keyed by study ID.</summary>
    [JsonPropertyName("sweepResults")]
    public required IReadOnlyDictionary<string, Phase2BranchSweepResult> SweepResults { get; init; }

    /// <summary>Per-stability-study continuation results, keyed by study ID.</summary>
    [JsonPropertyName("stabilityResults")]
    public required IReadOnlyDictionary<string, ContinuationResult> StabilityResults { get; init; }

    /// <summary>Comparison campaign IDs that were executed.</summary>
    [JsonPropertyName("executedCampaignIds")]
    public required IReadOnlyList<string> ExecutedCampaignIds { get; init; }

    /// <summary>Timestamp when the batch started.</summary>
    [JsonPropertyName("batchStarted")]
    public required DateTimeOffset BatchStarted { get; init; }

    /// <summary>Timestamp when the batch completed.</summary>
    [JsonPropertyName("batchCompleted")]
    public required DateTimeOffset BatchCompleted { get; init; }

    /// <summary>Whether all sweeps and studies completed without errors.</summary>
    [JsonIgnore]
    public bool AllSucceeded =>
        SweepResults.Values.All(r => r.RunRecords.Count > 0) &&
        StabilityResults.Values.All(r => r.Path.Count > 0);

    /// <summary>Total number of branch runs across all sweeps.</summary>
    [JsonIgnore]
    public int TotalBranchRuns =>
        SweepResults.Values.Sum(r => r.RunRecords.Count);
}
