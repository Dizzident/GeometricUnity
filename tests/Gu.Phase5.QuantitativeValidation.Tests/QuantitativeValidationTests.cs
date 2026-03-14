using System.Text.Json;
using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

/// <summary>
/// Tests for M49: Quantitative validation engine.
/// Covers UncertaintyPropagator, TargetMatcher, ConsistencyScoreCard,
/// ExternalTargetTable, and QuantitativeValidationRunner.
/// </summary>
public sealed class QuantitativeValidationTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
    };

    private static CalibrationPolicy StandardPolicy => new CalibrationPolicy
    {
        PolicyId = "standard-5sigma",
        Mode = "standard",
        SigmaThreshold = 5.0,
        RequireFullUncertainty = false,
    };

    private static CalibrationPolicy StrictPolicy => new CalibrationPolicy
    {
        PolicyId = "strict-full-uncertainty",
        Mode = "strict",
        SigmaThreshold = 3.0,
        RequireFullUncertainty = true,
    };

    // ─────────────────── UncertaintyPropagator ───────────────────

    [Fact]
    public void UncertaintyPropagator_AllComponents_QuadratureSum()
    {
        // sqrt(3^2 + 4^2 + 0^2 + 0^2) = 5
        var u = UncertaintyPropagator.Propagate(3.0, 4.0, 0.0, 0.0);
        Assert.Equal(3.0, u.BranchVariation);
        Assert.Equal(4.0, u.RefinementError);
        Assert.Equal(0.0, u.ExtractionError);
        Assert.Equal(0.0, u.EnvironmentSensitivity);
        Assert.Equal(5.0, u.TotalUncertainty, precision: 10);
    }

    [Fact]
    public void UncertaintyPropagator_NullComponents_StoredAsMinus1()
    {
        var u = UncertaintyPropagator.Propagate(null, null, null, null);
        Assert.Equal(-1, u.BranchVariation);
        Assert.Equal(-1, u.RefinementError);
        Assert.Equal(-1, u.ExtractionError);
        Assert.Equal(-1, u.EnvironmentSensitivity);
        Assert.Equal(-1, u.TotalUncertainty);
    }

    [Fact]
    public void UncertaintyPropagator_PartialComponents_TotalFromEstimatedOnly()
    {
        // Only branchVariation=3.0 estimated; refinementError=null
        var u = UncertaintyPropagator.Propagate(3.0, null, null, null);
        Assert.Equal(3.0, u.BranchVariation);
        Assert.Equal(-1, u.RefinementError);
        Assert.Equal(3.0, u.TotalUncertainty, precision: 10);
    }

    [Fact]
    public void UncertaintyPropagator_IsFullyEstimated_TrueWhenAllNonNegative()
    {
        var u = UncertaintyPropagator.Propagate(1.0, 2.0, 3.0, 4.0);
        Assert.True(u.IsFullyEstimated);
    }

    [Fact]
    public void UncertaintyPropagator_IsFullyEstimated_FalseWhenAnyMissing()
    {
        var u = UncertaintyPropagator.Propagate(1.0, null, 3.0, 4.0);
        Assert.False(u.IsFullyEstimated);
    }

    [Fact]
    public void UncertaintyPropagator_FourComponents_QuadratureSum()
    {
        // sqrt(1^2 + 2^2 + 3^2 + 4^2) = sqrt(30)
        var u = UncertaintyPropagator.Propagate(1.0, 2.0, 3.0, 4.0);
        double expected = System.Math.Sqrt(1 + 4 + 9 + 16);
        Assert.Equal(expected, u.TotalUncertainty, precision: 10);
    }

    // ─────────────────── TargetMatcher ───────────────────

    private static QuantitativeObservableRecord MakeObservable(
        string id, double value, double? totalUncertainty = null)
    {
        var u = totalUncertainty.HasValue
            ? UncertaintyPropagator.Propagate(totalUncertainty.Value, null, null, null)
            : UncertaintyPropagator.Propagate(null, null, null, null);
        return new QuantitativeObservableRecord
        {
            ObservableId = id,
            Value = value,
            Uncertainty = u,
            BranchId = "test-branch",
            EnvironmentId = "test-env",
            ExtractionMethod = "test",
            Provenance = MakeProvenance(),
        };
    }

    private static ExternalTarget MakeTarget(
        string id, double value, double uncertainty, string label = "T")
        => new ExternalTarget
        {
            Label = label,
            ObservableId = id,
            Value = value,
            Uncertainty = uncertainty,
            Source = "test-source",
        };

    [Fact]
    public void TargetMatcher_ExactMatch_PullZero_Passed()
    {
        var obs = MakeObservable("obs-1", 1.0, 0.1);
        var target = MakeTarget("obs-1", 1.0, 0.1);
        var match = TargetMatcher.Match(obs, target, StandardPolicy);
        Assert.Equal(0.0, match.Pull, precision: 10);
        Assert.True(match.Passed);
    }

    [Fact]
    public void TargetMatcher_FiveSigmaMismatch_Failed()
    {
        var obs = MakeObservable("obs-1", 10.0, 1.0);
        // target = 4.0, sigma_target = 0.0 (all sigma from computed)
        // pull = |10-4| / sqrt(1^2 + 0^2) = 6 > 5
        var target = MakeTarget("obs-1", 4.0, 0.0);
        var match = TargetMatcher.Match(obs, target, StandardPolicy);
        Assert.True(match.Pull > 5.0);
        Assert.False(match.Passed);
    }

    [Fact]
    public void TargetMatcher_WithinThreshold_Passed()
    {
        // pull = |1.0 - 1.0| / sqrt(0.1^2 + 0.1^2) = 0 → passed
        var obs = MakeObservable("obs-1", 1.0, 0.1);
        var target = MakeTarget("obs-1", 1.0, 0.1);
        var match = TargetMatcher.Match(obs, target, StandardPolicy);
        Assert.True(match.Passed);
    }

    [Fact]
    public void TargetMatcher_ComputedUncertaintyUnestimated_UsesTargetSigmaOnly()
    {
        // computed sigma = -1 (unestimated), target sigma = 1.0
        // pull = |computed - target| / target_sigma
        var obs = MakeObservable("obs-1", 2.0, totalUncertainty: null);
        var target = MakeTarget("obs-1", 0.0, 1.0);
        var match = TargetMatcher.Match(obs, target, StandardPolicy);
        // pull = |2 - 0| / 1.0 = 2.0 < 5 → passed
        Assert.Equal(2.0, match.Pull, precision: 10);
        Assert.True(match.Passed);
        Assert.NotNull(match.Notes); // Note about using target sigma only
    }

    [Fact]
    public void TargetMatcher_RequireFullUncertainty_UnestimatedFails()
    {
        var obs = MakeObservable("obs-1", 1.0, totalUncertainty: null);
        var target = MakeTarget("obs-1", 1.0, 0.1);
        var match = TargetMatcher.Match(obs, target, StrictPolicy);
        Assert.False(match.Passed);
        Assert.Equal(double.PositiveInfinity, match.Pull);
    }

    [Fact]
    public void TargetMatcher_PullFormula_Correct()
    {
        // computed = 3.0, sigma_computed = 3.0
        // target = 0.0, sigma_target = 4.0
        // pull = |3 - 0| / sqrt(9 + 16) = 3/5 = 0.6
        var obs = MakeObservable("obs-1", 3.0, 3.0);
        var target = MakeTarget("obs-1", 0.0, 4.0);
        var match = TargetMatcher.Match(obs, target, StandardPolicy);
        Assert.Equal(0.6, match.Pull, precision: 10);
        Assert.True(match.Passed); // 0.6 < 5
    }

    [Fact]
    public void TargetMatcher_BothSigmasZero_ExactEqual_PullZero()
    {
        var obs = MakeObservable("obs-1", 1.0, 0.0);
        var target = MakeTarget("obs-1", 1.0, 0.0);
        var match = TargetMatcher.Match(obs, target, StandardPolicy);
        Assert.Equal(0.0, match.Pull, precision: 10);
        Assert.True(match.Passed);
    }

    [Fact]
    public void TargetMatcher_BothSigmasZero_NotEqual_PullInfinity()
    {
        var obs = MakeObservable("obs-1", 2.0, 0.0);
        var target = MakeTarget("obs-1", 1.0, 0.0);
        var match = TargetMatcher.Match(obs, target, StandardPolicy);
        Assert.Equal(double.PositiveInfinity, match.Pull);
        Assert.False(match.Passed);
    }

    // ─────────────────── ConsistencyScoreCard ───────────────────

    [Fact]
    public void ConsistencyScoreCard_AllPass_OverallScore1()
    {
        var matches = new List<TargetMatchRecord>
        {
            new TargetMatchRecord
            {
                ObservableId = "a", TargetLabel = "T", TargetValue = 1, TargetUncertainty = 0.1,
                ComputedValue = 1, ComputedUncertainty = 0.1, Pull = 0, Passed = true
            },
            new TargetMatchRecord
            {
                ObservableId = "b", TargetLabel = "T", TargetValue = 2, TargetUncertainty = 0.1,
                ComputedValue = 2, ComputedUncertainty = 0.1, Pull = 0, Passed = true
            },
        };
        var card = new ConsistencyScoreCard
        {
            StudyId = "s1",
            SchemaVersion = "1.0.0",
            Matches = matches,
            TotalPassed = 2,
            TotalFailed = 0,
            OverallScore = 1.0,
            CalibrationPolicyId = "policy-1",
            Provenance = MakeProvenance(),
        };
        Assert.Equal(1.0, card.OverallScore);
        Assert.Equal(2, card.TotalPassed);
        Assert.Equal(0, card.TotalFailed);
    }

    [Fact]
    public void ConsistencyScoreCard_AllFail_OverallScore0()
    {
        var matches = new List<TargetMatchRecord>
        {
            new TargetMatchRecord
            {
                ObservableId = "a", TargetLabel = "T", TargetValue = 1, TargetUncertainty = 0.1,
                ComputedValue = 100, ComputedUncertainty = 0.1, Pull = 990, Passed = false
            },
        };
        var card = new ConsistencyScoreCard
        {
            StudyId = "s1",
            SchemaVersion = "1.0.0",
            Matches = matches,
            TotalPassed = 0,
            TotalFailed = 1,
            OverallScore = 0.0,
            CalibrationPolicyId = "policy-1",
            Provenance = MakeProvenance(),
        };
        Assert.Equal(0.0, card.OverallScore);
    }

    [Fact]
    public void ConsistencyScoreCard_JsonRoundTrip()
    {
        var card = new ConsistencyScoreCard
        {
            StudyId = "s1",
            SchemaVersion = "1.0.0",
            Matches = new List<TargetMatchRecord>(),
            TotalPassed = 0,
            TotalFailed = 0,
            OverallScore = double.NaN,
            CalibrationPolicyId = "p1",
            Provenance = MakeProvenance(),
        };
        var json = card.ToJson();
        var restored = ConsistencyScoreCard.FromJson(json);
        Assert.Equal(card.StudyId, restored.StudyId);
        Assert.Equal(card.CalibrationPolicyId, restored.CalibrationPolicyId);
    }

    // ─────────────────── ExternalTargetTable ───────────────────

    [Fact]
    public void ExternalTargetTable_JsonRoundTrip()
    {
        var table = new ExternalTargetTable
        {
            TableId = "table-1",
            Targets = new List<ExternalTarget>
            {
                new ExternalTarget
                {
                    Label = "T1",
                    ObservableId = "obs-1",
                    Value = 1.5,
                    Uncertainty = 0.05,
                    Source = "synthetic-toy-v1",
                    EvidenceTier = "toy-placeholder",
                },
            },
        };
        var json = table.ToJson();
        var restored = ExternalTargetTable.FromJson(json);
        Assert.Equal("table-1", restored.TableId);
        Assert.Single(restored.Targets);
        Assert.Equal("obs-1", restored.Targets[0].ObservableId);
        Assert.Equal(1.5, restored.Targets[0].Value);
    }

    [Fact]
    public void ExternalTargetTable_EmptyTargets_RoundTrip()
    {
        var table = new ExternalTargetTable
        {
            TableId = "empty-table",
            Targets = new List<ExternalTarget>(),
        };
        var json = table.ToJson();
        var restored = ExternalTargetTable.FromJson(json);
        Assert.Empty(restored.Targets);
    }

    // ─────────────────── QuantitativeValidationRunner ───────────────────

    [Fact]
    public void Runner_ExactMatches_AllPass()
    {
        var observables = new List<QuantitativeObservableRecord>
        {
            MakeObservable("obs-1", 1.0, 0.1),
            MakeObservable("obs-2", 2.0, 0.2),
        };
        var table = new ExternalTargetTable
        {
            TableId = "t1",
            Targets = new List<ExternalTarget>
            {
                MakeTarget("obs-1", 1.0, 0.1),
                MakeTarget("obs-2", 2.0, 0.2),
            },
        };
        var runner = new QuantitativeValidationRunner();
        var card = runner.Run("study-1", observables, table, StandardPolicy, MakeProvenance());
        Assert.Equal(2, card.TotalPassed);
        Assert.Equal(0, card.TotalFailed);
        Assert.Equal(1.0, card.OverallScore, precision: 10);
    }

    [Fact]
    public void Runner_AllFail_OverallScoreZero()
    {
        var observables = new List<QuantitativeObservableRecord>
        {
            MakeObservable("obs-1", 1000.0, 0.01),
        };
        var table = new ExternalTargetTable
        {
            TableId = "t1",
            Targets = new List<ExternalTarget>
            {
                MakeTarget("obs-1", 0.0, 0.01),
            },
        };
        var runner = new QuantitativeValidationRunner();
        var card = runner.Run("study-1", observables, table, StandardPolicy, MakeProvenance());
        Assert.Equal(0, card.TotalPassed);
        Assert.Equal(1, card.TotalFailed);
        Assert.Equal(0.0, card.OverallScore, precision: 10);
    }

    [Fact]
    public void Runner_ObservableWithNoTarget_Skipped()
    {
        var observables = new List<QuantitativeObservableRecord>
        {
            MakeObservable("obs-1", 1.0, 0.1),
            MakeObservable("obs-extra", 99.0, 1.0), // no target
        };
        var table = new ExternalTargetTable
        {
            TableId = "t1",
            Targets = new List<ExternalTarget>
            {
                MakeTarget("obs-1", 1.0, 0.1),
            },
        };
        var runner = new QuantitativeValidationRunner();
        var card = runner.Run("study-1", observables, table, StandardPolicy, MakeProvenance());
        Assert.Single(card.Matches);
        Assert.Equal(1, card.TotalPassed);
    }

    [Fact]
    public void Runner_NoMatches_ScoreNaN()
    {
        var observables = new List<QuantitativeObservableRecord>();
        var table = new ExternalTargetTable
        {
            TableId = "t1",
            Targets = new List<ExternalTarget>
            {
                MakeTarget("obs-1", 1.0, 0.1),
            },
        };
        var runner = new QuantitativeValidationRunner();
        var card = runner.Run("study-1", observables, table, StandardPolicy, MakeProvenance());
        Assert.Equal(0, card.TotalPassed);
        Assert.Equal(0, card.TotalFailed);
        Assert.True(double.IsNaN(card.OverallScore));
    }

    [Fact]
    public void Runner_SetsStudyIdAndPolicyId()
    {
        var runner = new QuantitativeValidationRunner();
        var card = runner.Run(
            "my-study",
            new List<QuantitativeObservableRecord>(),
            new ExternalTargetTable { TableId = "t1", Targets = new List<ExternalTarget>() },
            StandardPolicy,
            MakeProvenance());
        Assert.Equal("my-study", card.StudyId);
        Assert.Equal("standard-5sigma", card.CalibrationPolicyId);
    }

    [Fact]
    public void Runner_KnownPull_Integration()
    {
        // computed = 3.0, sigma_computed = 3.0, target = 0.0, sigma_target = 4.0
        // pull = 3/5 = 0.6 → passes at 5-sigma threshold
        var obs = MakeObservable("obs-1", 3.0, 3.0);
        var table = new ExternalTargetTable
        {
            TableId = "t1",
            Targets = new List<ExternalTarget> { MakeTarget("obs-1", 0.0, 4.0) },
        };
        var runner = new QuantitativeValidationRunner();
        var card = runner.Run("test", new List<QuantitativeObservableRecord> { obs }, table, StandardPolicy, MakeProvenance());
        Assert.Single(card.Matches);
        Assert.Equal(0.6, card.Matches[0].Pull, precision: 10);
        Assert.True(card.Matches[0].Passed);
    }

    // ─────────────────── CalibrationPolicy ───────────────────

    [Fact]
    public void CalibrationPolicy_Defaults_Correct()
    {
        var policy = new CalibrationPolicy
        {
            PolicyId = "default-test",
            Mode = "standard",
        };
        Assert.Equal(5.0, policy.SigmaThreshold);
        Assert.False(policy.RequireFullUncertainty);
    }

    // ─────────────────── QuantitativeUncertainty ───────────────────

    [Fact]
    public void QuantitativeUncertainty_DefaultValues_AllMinus1()
    {
        var u = new QuantitativeUncertainty();
        Assert.Equal(-1, u.BranchVariation);
        Assert.Equal(-1, u.RefinementError);
        Assert.Equal(-1, u.ExtractionError);
        Assert.Equal(-1, u.EnvironmentSensitivity);
        Assert.Equal(-1, u.TotalUncertainty);
        Assert.False(u.IsFullyEstimated);
    }
}
