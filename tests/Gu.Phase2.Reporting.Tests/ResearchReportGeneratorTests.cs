using Gu.Phase2.Canonicity;
using Gu.Phase2.Continuation;
using Gu.Phase2.Execution;
using Gu.Phase2.Reporting;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Reporting.Tests;

public class ResearchReportGeneratorTests
{
    private static readonly DocketAggregator EmptyDocketAggregator = _ => [];

    [Fact]
    public void Generate_EmptyBatch_ReturnsEmptyReport()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        var batchResult = MakeBatchResult(sweeps: [], stability: []);

        var report = generator.Generate(batchResult);

        Assert.Equal("report-batch-1", report.ReportId);
        Assert.Equal("batch-1", report.BatchId);
        Assert.Empty(report.BranchLocalConclusions);
        Assert.Empty(report.ComparisonReadyConclusions);
        Assert.Empty(report.OpenItems);
        Assert.Empty(report.NumericalOnlyResults);
        Assert.Empty(report.UninterpretedOutputs);
        Assert.Empty(report.RuledOutClaims);
        Assert.Empty(report.Dockets);
    }

    [Fact]
    public void Generate_AllConvergedSweep_BranchLocal()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        var sweepResult = ResearchBatchRunnerTests.MakeSweepResult(convergedCount: 2, totalBranches: 2);
        var batchResult = MakeBatchResult(sweeps: [("sweep-1", sweepResult)], stability: []);

        var report = generator.Generate(batchResult);

        Assert.Single(report.BranchLocalConclusions);
        Assert.Contains("all 2 branches converged", report.BranchLocalConclusions[0].Summary);
        Assert.Equal("sweep", report.BranchLocalConclusions[0].SourceCategory);
        Assert.Equal("moderate", report.BranchLocalConclusions[0].EvidenceStrength);
    }

    [Fact]
    public void Generate_PartialConvergence_NumericalOnly()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        var sweepResult = ResearchBatchRunnerTests.MakeSweepResult(convergedCount: 1, totalBranches: 3);
        var batchResult = MakeBatchResult(sweeps: [("sweep-1", sweepResult)], stability: []);

        var report = generator.Generate(batchResult);

        Assert.Single(report.NumericalOnlyResults);
        Assert.Contains("1/3", report.NumericalOnlyResults[0].Summary);
        Assert.Equal("numerical-only", report.NumericalOnlyResults[0].EvidenceStrength);
    }

    [Fact]
    public void Generate_NoConvergence_RuledOut()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        var sweepResult = ResearchBatchRunnerTests.MakeSweepResult(convergedCount: 0, totalBranches: 2);
        var batchResult = MakeBatchResult(sweeps: [("sweep-1", sweepResult)], stability: []);

        var report = generator.Generate(batchResult);

        Assert.Single(report.RuledOutClaims);
        Assert.Contains("no branches converged", report.RuledOutClaims[0].Summary);
        Assert.Equal("strong", report.RuledOutClaims[0].EvidenceStrength);
    }

    [Fact]
    public void Generate_EmptySweep_OpenItem()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        var sweepResult = ResearchBatchRunnerTests.MakeSweepResult(convergedCount: 0, totalBranches: 0);
        var batchResult = MakeBatchResult(sweeps: [("sweep-1", sweepResult)], stability: []);

        var report = generator.Generate(batchResult);

        Assert.Single(report.OpenItems);
        Assert.Contains("no run records", report.OpenItems[0].Summary);
    }

    [Fact]
    public void Generate_CleanStability_BranchLocal()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        var contResult = ResearchBatchRunnerTests.MakeContinuationResult(stepCount: 5);
        var batchResult = MakeBatchResult(sweeps: [], stability: [("stab-1", contResult)]);

        var report = generator.Generate(batchResult);

        Assert.Single(report.BranchLocalConclusions);
        Assert.Contains("5 steps completed cleanly", report.BranchLocalConclusions[0].Summary);
        Assert.Equal("stability", report.BranchLocalConclusions[0].SourceCategory);
    }

    [Fact]
    public void Generate_StabilityWithEvents_Uninterpreted()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        var events = new List<ContinuationEvent>
        {
            new()
            {
                Kind = ContinuationEventKind.HessianSignChange,
                Lambda = 0.5,
                Description = "sign change",
                Severity = "warning",
            },
        };
        var contResult = ResearchBatchRunnerTests.MakeContinuationResult(stepCount: 3, events: events);
        var batchResult = MakeBatchResult(sweeps: [], stability: [("stab-1", contResult)]);

        var report = generator.Generate(batchResult);

        Assert.Single(report.UninterpretedOutputs);
        Assert.Contains("1 events require interpretation", report.UninterpretedOutputs[0].Summary);
    }

    [Fact]
    public void Generate_EmptyStability_OpenItem()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        var contResult = ResearchBatchRunnerTests.MakeContinuationResult(stepCount: 0);
        var batchResult = MakeBatchResult(sweeps: [], stability: [("stab-1", contResult)]);

        var report = generator.Generate(batchResult);

        Assert.Single(report.OpenItems);
        Assert.Contains("no continuation path", report.OpenItems[0].Summary);
    }

    [Fact]
    public void Generate_MixedBatch_ComparisonReady()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        var sweepResult = ResearchBatchRunnerTests.MakeSweepResult(convergedCount: 1);
        var contResult = ResearchBatchRunnerTests.MakeContinuationResult(stepCount: 3);
        var batchResult = MakeBatchResult(
            sweeps: [("sweep-1", sweepResult)],
            stability: [("stab-1", contResult)]);

        var report = generator.Generate(batchResult);

        Assert.Single(report.ComparisonReadyConclusions);
        Assert.Contains("cross-comparison possible", report.ComparisonReadyConclusions[0].Summary);
    }

    [Fact]
    public void Generate_DocketAggregatorCalled()
    {
        var docket = new CanonicityDocket
        {
            ObjectClass = "A0",
            ActiveRepresentative = "flat",
            EquivalenceRelationId = "gauge-equiv",
            AdmissibleComparisonClass = "connection-1form",
            DownstreamClaimsBlockedUntilClosure = ["claim-1"],
            CurrentEvidence = [],
            KnownCounterexamples = [],
            PendingTheorems = [],
            StudyReports = [],
            Status = DocketStatus.Open,
        };

        bool called = false;
        DocketAggregator aggregator = _ =>
        {
            called = true;
            return [docket];
        };

        var generator = new ResearchReportGenerator(aggregator);
        var batchResult = MakeBatchResult(sweeps: [], stability: []);
        var report = generator.Generate(batchResult);

        Assert.True(called);
        Assert.Single(report.Dockets);
        Assert.Equal("A0", report.Dockets[0].ObjectClass);
    }

    [Fact]
    public void Generate_NullBatchResult_Throws()
    {
        var generator = new ResearchReportGenerator(EmptyDocketAggregator);
        Assert.Throws<ArgumentNullException>(() => generator.Generate(null!));
    }

    [Fact]
    public void Constructor_NullAggregator_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ResearchReportGenerator(null!));
    }

    // --- Helpers ---

    // Make helpers accessible from other test classes
    internal static Phase2BranchSweepResult MakeSweepResult(int convergedCount, int totalBranches = -1)
        => ResearchBatchRunnerTests.MakeSweepResult(convergedCount, totalBranches);

    private static ResearchBatchResult MakeBatchResult(
        IReadOnlyList<(string studyId, Phase2BranchSweepResult result)> sweeps,
        IReadOnlyList<(string studyId, ContinuationResult result)> stability)
    {
        var sweepDict = new Dictionary<string, Phase2BranchSweepResult>();
        foreach (var (id, r) in sweeps) sweepDict[id] = r;

        var stabilityDict = new Dictionary<string, ContinuationResult>();
        foreach (var (id, r) in stability) stabilityDict[id] = r;

        return new ResearchBatchResult
        {
            Spec = new ResearchBatchSpec
            {
                BatchId = "batch-1",
                Sweeps = [],
                StabilityStudies = [],
                ComparisonCampaignIds = [],
            },
            SweepResults = sweepDict,
            StabilityResults = stabilityDict,
            ExecutedCampaignIds = [],
            BatchStarted = DateTimeOffset.UtcNow,
            BatchCompleted = DateTimeOffset.UtcNow,
        };
    }
}
