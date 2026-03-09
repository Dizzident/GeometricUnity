using Gu.Phase2.Canonicity;

namespace Gu.Phase2.Reporting;

/// <summary>
/// Callback for aggregating canonicity dockets from a batch result.
/// </summary>
public delegate IReadOnlyList<CanonicityDocket> DocketAggregator(ResearchBatchResult batchResult);

/// <summary>
/// Generates structured research reports from batch execution results.
/// Per IMPLEMENTATION_PLAN_P2.md Section 14, Milestone 22.
///
/// The generator is delegate-driven: callers supply a DocketAggregator
/// to keep canonicity logic decoupled from report generation.
/// </summary>
public sealed class ResearchReportGenerator
{
    private readonly DocketAggregator _docketAggregator;

    public ResearchReportGenerator(DocketAggregator docketAggregator)
    {
        _docketAggregator = docketAggregator ?? throw new ArgumentNullException(nameof(docketAggregator));
    }

    /// <summary>
    /// Generate a research report from a batch result.
    /// Classifies sweep and stability results into epistemic categories.
    /// </summary>
    public ResearchReport Generate(ResearchBatchResult batchResult)
    {
        ArgumentNullException.ThrowIfNull(batchResult);

        var branchLocal = new List<ReportItem>();
        var comparisonReady = new List<ReportItem>();
        var openItems = new List<ReportItem>();
        var numericalOnly = new List<ReportItem>();
        var uninterpreted = new List<ReportItem>();
        var ruledOut = new List<ReportItem>();

        // 1. Classify sweep results
        foreach (var (studyId, sweepResult) in batchResult.SweepResults)
        {
            if (sweepResult.RunRecords.Count == 0)
            {
                openItems.Add(new ReportItem
                {
                    Summary = $"Sweep '{studyId}' produced no run records.",
                    SourceCategory = "sweep",
                    SourceStudyId = studyId,
                    EvidenceStrength = "weak",
                });
                continue;
            }

            var convergedCount = sweepResult.RunRecords.Count(r => r.Converged);
            var totalCount = sweepResult.RunRecords.Count;

            if (convergedCount == totalCount)
            {
                branchLocal.Add(new ReportItem
                {
                    Summary = $"Sweep '{studyId}': all {totalCount} branches converged.",
                    SourceCategory = "sweep",
                    SourceStudyId = studyId,
                    EvidenceStrength = "moderate",
                });
            }
            else if (convergedCount > 0)
            {
                numericalOnly.Add(new ReportItem
                {
                    Summary = $"Sweep '{studyId}': {convergedCount}/{totalCount} branches converged (partial).",
                    SourceCategory = "sweep",
                    SourceStudyId = studyId,
                    EvidenceStrength = "numerical-only",
                });
            }
            else
            {
                ruledOut.Add(new ReportItem
                {
                    Summary = $"Sweep '{studyId}': no branches converged.",
                    SourceCategory = "sweep",
                    SourceStudyId = studyId,
                    EvidenceStrength = "strong",
                });
            }

            // Check for uninterpreted outputs (converged but no observed state)
            foreach (var record in sweepResult.RunRecords)
            {
                if (record.Converged && record.ObservedState == null)
                {
                    uninterpreted.Add(new ReportItem
                    {
                        Summary = $"Branch '{record.Variant.Id}' in sweep '{studyId}' converged but has no observed outputs.",
                        SourceCategory = "sweep",
                        SourceStudyId = studyId,
                        BranchManifestId = record.Manifest.BranchId,
                        EvidenceStrength = "numerical-only",
                    });
                }
            }
        }

        // 2. Classify stability results
        foreach (var (studyId, contResult) in batchResult.StabilityResults)
        {
            if (contResult.Path.Count == 0)
            {
                openItems.Add(new ReportItem
                {
                    Summary = $"Stability study '{studyId}' produced no continuation path.",
                    SourceCategory = "stability",
                    SourceStudyId = studyId,
                    EvidenceStrength = "weak",
                });
                continue;
            }

            if (contResult.AllEvents.Count > 0)
            {
                uninterpreted.Add(new ReportItem
                {
                    Summary = $"Stability study '{studyId}': {contResult.Path.Count} steps, " +
                              $"{contResult.AllEvents.Count} events require interpretation.",
                    SourceCategory = "stability",
                    SourceStudyId = studyId,
                    EvidenceStrength = "numerical-only",
                });
            }
            else
            {
                branchLocal.Add(new ReportItem
                {
                    Summary = $"Stability study '{studyId}': {contResult.Path.Count} steps completed cleanly.",
                    SourceCategory = "stability",
                    SourceStudyId = studyId,
                    EvidenceStrength = "moderate",
                });
            }
        }

        // 3. Cross-study: if we have both sweep and stability, mark comparison-ready
        if (batchResult.SweepResults.Count > 0 && batchResult.StabilityResults.Count > 0)
        {
            comparisonReady.Add(new ReportItem
            {
                Summary = $"Batch contains {batchResult.SweepResults.Count} sweep(s) and " +
                          $"{batchResult.StabilityResults.Count} stability study/studies — cross-comparison possible.",
                SourceCategory = "batch",
                SourceStudyId = batchResult.Spec.BatchId,
                EvidenceStrength = "moderate",
            });
        }

        // 4. Record comparison campaigns as open items
        foreach (var campaignId in batchResult.ExecutedCampaignIds)
        {
            openItems.Add(new ReportItem
            {
                Summary = $"Comparison campaign '{campaignId}' was scheduled.",
                SourceCategory = "comparison",
                SourceStudyId = campaignId,
                EvidenceStrength = "weak",
            });
        }

        // 5. Aggregate dockets and classify
        var dockets = _docketAggregator(batchResult);
        foreach (var docket in dockets)
        {
            if (docket.Status == DocketStatus.Open || docket.Status == DocketStatus.EvidenceAccumulating)
            {
                openItems.Add(new ReportItem
                {
                    Summary = $"Canonicity docket '{docket.ObjectClass}' is {docket.Status}: " +
                              $"{docket.DownstreamClaimsBlockedUntilClosure.Count} downstream claims blocked.",
                    SourceCategory = "canonicity",
                    SourceStudyId = docket.ObjectClass,
                    EvidenceStrength = "weak",
                });
            }
            else if (docket.Status == DocketStatus.Falsified)
            {
                ruledOut.Add(new ReportItem
                {
                    Summary = $"Canonicity docket '{docket.ObjectClass}' falsified with " +
                              $"{docket.KnownCounterexamples.Count} counterexamples.",
                    SourceCategory = "canonicity",
                    SourceStudyId = docket.ObjectClass,
                    EvidenceStrength = "strong",
                });
            }
        }

        return new ResearchReport
        {
            ReportId = $"report-{batchResult.Spec.BatchId}",
            BatchId = batchResult.Spec.BatchId,
            BranchLocalConclusions = branchLocal,
            ComparisonReadyConclusions = comparisonReady,
            OpenItems = openItems,
            NumericalOnlyResults = numericalOnly,
            UninterpretedOutputs = uninterpreted,
            RuledOutClaims = ruledOut,
            Dockets = dockets,
            GeneratedAt = DateTimeOffset.UtcNow,
        };
    }
}
