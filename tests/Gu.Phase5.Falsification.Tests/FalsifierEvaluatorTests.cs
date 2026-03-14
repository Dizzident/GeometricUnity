using Gu.Core;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Falsification.Tests;

/// <summary>
/// Tests for FalsifierEvaluator (M50).
/// </summary>
public sealed class FalsifierEvaluatorTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "abc123",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
    };

    private static BranchRobustnessRecord MakeRobustnessRecord(double fragilityScore, string classification)
    {
        var fragilityRecords = new Dictionary<string, FragilityRecord>
        {
            ["q-residual"] = new FragilityRecord
            {
                TargetQuantityId = "q-residual",
                FragilityScore = fragilityScore,
                MaxDistanceToNeighbor = fragilityScore * 0.5,
                MeanDistanceToFamily = 0.5,
                Classification = classification,
                MaxDistancePair = ["v1", "v2"],
                VariantCount = 4,
            },
        };

        return new BranchRobustnessRecord
        {
            RecordId = "rec-001",
            StudyId = "study-001",
            BranchVariantIds = ["v1", "v2", "v3", "v4"],
            DistanceMatrices = new Dictionary<string, BranchDistanceMatrix>(),
            EquivalenceClasses = new Dictionary<string, List<BranchEquivalenceClass>>(),
            FragilityRecords = fragilityRecords,
            InvarianceCandidates = new List<InvarianceCandidateRecord>(),
            OverallSummary = classification == "fragile" ? "fragile" : "robust",
            Provenance = MakeProvenance(),
        };
    }

    private static ConvergenceFailureRecord MakeFailure(string failureType = "non-convergent")
        => new ConvergenceFailureRecord
        {
            QuantityId = "q-lambda",
            FailureType = failureType,
            Description = $"Quantity q-lambda: {failureType}.",
            ObservedValues = [1.0, 1.05, 1.1],
            MeshParameters = [0.5, 0.25, 0.125],
        };

    private static ConsistencyScoreCard MakeScoreCard(bool passed)
    {
        var match = new TargetMatchRecord
        {
            ObservableId = "obs-boson-ratio",
            TargetLabel = "synthetic-boson-1",
            TargetValue = 1.0,
            TargetUncertainty = 0.3,
            ComputedValue = passed ? 1.1 : 5.0,
            ComputedUncertainty = 0.05,
            Pull = passed ? 0.3 : 13.0,
            Passed = passed,
        };

        return new ConsistencyScoreCard
        {
            StudyId = "study-001",
            SchemaVersion = "1.0.0",
            Matches = [match],
            TotalPassed = passed ? 1 : 0,
            TotalFailed = passed ? 0 : 1,
            OverallScore = passed ? 1.0 : 0.0,
            CalibrationPolicyId = "default",
            Provenance = MakeProvenance(),
        };
    }

    [Fact]
    public void Evaluate_NoSources_EmptySummary()
    {
        var evaluator = new FalsifierEvaluator();
        var policy = new FalsificationPolicy();

        var summary = evaluator.Evaluate(
            "study-empty",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            scoreCard: null,
            policy: policy,
            provenance: MakeProvenance());

        Assert.Empty(summary.Falsifiers);
        Assert.Equal(0, summary.ActiveFatalCount);
        Assert.Equal(0, summary.TotalActiveCount);
    }

    [Fact]
    public void Evaluate_FragileRecord_ProducesBranchFragilityFalsifier()
    {
        var evaluator = new FalsifierEvaluator();
        var policy = new FalsificationPolicy { BranchFragilityThreshold = 0.5 };
        var branchRecord = MakeRobustnessRecord(fragilityScore: 0.75, classification: "fragile");

        var summary = evaluator.Evaluate(
            "study-fragile",
            branchRecord: branchRecord,
            convergenceRecords: null,
            convergenceFailures: null,
            scoreCard: null,
            policy: policy,
            provenance: MakeProvenance());

        Assert.Single(summary.Falsifiers);
        var falsifier = summary.Falsifiers[0];
        Assert.Equal(FalsifierTypes.BranchFragility, falsifier.FalsifierType);
        Assert.Equal(FalsifierSeverity.High, falsifier.Severity);
        Assert.True(falsifier.Active);
        Assert.Equal("q-residual", falsifier.TargetId);
        Assert.Equal(1, summary.ActiveHighCount);
        Assert.Equal(1, summary.TotalActiveCount);
    }

    [Fact]
    public void Evaluate_RobustRecord_NoBranchFragilityFalsifier()
    {
        var evaluator = new FalsifierEvaluator();
        var policy = new FalsificationPolicy { BranchFragilityThreshold = 0.5 };
        var branchRecord = MakeRobustnessRecord(fragilityScore: 0.1, classification: "invariant");

        var summary = evaluator.Evaluate(
            "study-robust",
            branchRecord: branchRecord,
            convergenceRecords: null,
            convergenceFailures: null,
            scoreCard: null,
            policy: policy,
            provenance: MakeProvenance());

        Assert.Empty(summary.Falsifiers);
    }

    [Fact]
    public void Evaluate_NonConvergent_ProducesActiveFalsifier()
    {
        var evaluator = new FalsifierEvaluator();
        var policy = new FalsificationPolicy();
        var failures = new[] { MakeFailure("non-convergent") };

        var summary = evaluator.Evaluate(
            "study-nonconv",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: failures,
            scoreCard: null,
            policy: policy,
            provenance: MakeProvenance());

        Assert.Single(summary.Falsifiers);
        var falsifier = summary.Falsifiers[0];
        Assert.Equal(FalsifierTypes.NonConvergence, falsifier.FalsifierType);
        Assert.Equal(FalsifierSeverity.High, falsifier.Severity);
        Assert.True(falsifier.Active);
        Assert.Equal(1, summary.ActiveHighCount);
    }

    [Fact]
    public void Evaluate_InsufficientData_ProducesInactiveFalsifier()
    {
        var evaluator = new FalsifierEvaluator();
        var policy = new FalsificationPolicy();
        var failures = new[] { MakeFailure("insufficient-data") };

        var summary = evaluator.Evaluate(
            "study-insufficient",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: failures,
            scoreCard: null,
            policy: policy,
            provenance: MakeProvenance());

        Assert.Single(summary.Falsifiers);
        var falsifier = summary.Falsifiers[0];
        Assert.Equal(FalsifierSeverity.Informational, falsifier.Severity);
        Assert.False(falsifier.Active);
        Assert.Equal(0, summary.TotalActiveCount);
    }

    [Fact]
    public void Evaluate_FailedTargetMatch_ProducesQuantitativeMismatchFalsifier()
    {
        var evaluator = new FalsifierEvaluator();
        var policy = new FalsificationPolicy();
        var scoreCard = MakeScoreCard(passed: false);

        var summary = evaluator.Evaluate(
            "study-mismatch",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            scoreCard: scoreCard,
            policy: policy,
            provenance: MakeProvenance());

        Assert.Single(summary.Falsifiers);
        var falsifier = summary.Falsifiers[0];
        Assert.Equal(FalsifierTypes.QuantitativeMismatch, falsifier.FalsifierType);
        Assert.Equal(FalsifierSeverity.High, falsifier.Severity);
        Assert.True(falsifier.Active);
    }

    [Fact]
    public void Evaluate_PassedTargetMatch_NoFalsifier()
    {
        var evaluator = new FalsifierEvaluator();
        var policy = new FalsificationPolicy();
        var scoreCard = MakeScoreCard(passed: true);

        var summary = evaluator.Evaluate(
            "study-pass",
            branchRecord: null,
            convergenceRecords: null,
            convergenceFailures: null,
            scoreCard: scoreCard,
            policy: policy,
            provenance: MakeProvenance());

        Assert.Empty(summary.Falsifiers);
    }

    [Fact]
    public void Evaluate_MultipleSources_AggregatesAll()
    {
        var evaluator = new FalsifierEvaluator();
        var policy = new FalsificationPolicy();
        var branchRecord = MakeRobustnessRecord(0.75, "fragile");
        var failures = new[] { MakeFailure("non-convergent") };
        var scoreCard = MakeScoreCard(passed: false);

        var summary = evaluator.Evaluate(
            "study-multi",
            branchRecord: branchRecord,
            convergenceRecords: null,
            convergenceFailures: failures,
            scoreCard: scoreCard,
            policy: policy,
            provenance: MakeProvenance());

        Assert.Equal(3, summary.Falsifiers.Count);
        Assert.All(summary.Falsifiers, f => Assert.True(f.Active));
        Assert.Equal(3, summary.TotalActiveCount);
        Assert.Equal(3, summary.ActiveHighCount);
    }

    [Fact]
    public void Evaluate_StudyId_PropagatedToSummary()
    {
        var evaluator = new FalsifierEvaluator();
        var summary = evaluator.Evaluate(
            "my-study-id",
            null, null, null, null,
            new FalsificationPolicy(),
            MakeProvenance());

        Assert.Equal("my-study-id", summary.StudyId);
    }
}
