using Gu.Core;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

/// <summary>
/// Tests for TargetMatcher (M49).
/// </summary>
public sealed class TargetMatcherTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "abc123",
        Branch = new Gu.Core.BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
    };

    private static QuantitativeObservableRecord MakeObservable(
        double value,
        double totalSigma = 0.05)
    {
        return new QuantitativeObservableRecord
        {
            ObservableId = "obs-ratio",
            Value = value,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = totalSigma / 2,
                RefinementError = totalSigma / 2,
                ExtractionError = 0,
                EnvironmentSensitivity = 0,
                TotalUncertainty = totalSigma,
            },
            BranchId = "branch-v1",
            EnvironmentId = "env-toy",
            ExtractionMethod = "eigenvalue-ratio",
            Provenance = MakeProvenance(),
        };
    }

    private static ExternalTarget MakeTarget(double value, double sigma = 0.3)
        => new ExternalTarget
        {
            Label = "synthetic-boson-1",
            ObservableId = "obs-ratio",
            Value = value,
            Uncertainty = sigma,
            Source = "synthetic-toy-v1",
            EvidenceTier = "toy-placeholder",
        };

    private static CalibrationPolicy DefaultPolicy => new CalibrationPolicy
    {
        PolicyId = "default",
        Mode = "standard",
        SigmaThreshold = 5.0,
    };

    [Fact]
    public void Match_CloseValue_Passes()
    {
        var obs = MakeObservable(value: 1.05, totalSigma: 0.05);
        var target = MakeTarget(value: 1.0, sigma: 0.3);
        // pull = |1.05 - 1.0| / sqrt(0.05^2 + 0.3^2) ≈ 0.05 / 0.304 ≈ 0.165
        var match = TargetMatcher.Match(obs, target, DefaultPolicy);

        Assert.True(match.Passed);
        Assert.True(match.Pull < 1.0);
        Assert.Equal("obs-ratio", match.ObservableId);
    }

    [Fact]
    public void Match_FarValue_Fails()
    {
        var obs = MakeObservable(value: 5.0, totalSigma: 0.05);
        var target = MakeTarget(value: 1.0, sigma: 0.3);
        // pull = |5.0 - 1.0| / sqrt(0.05^2 + 0.3^2) ≈ 4.0 / 0.304 ≈ 13.2
        var match = TargetMatcher.Match(obs, target, DefaultPolicy);

        Assert.False(match.Passed);
        Assert.True(match.Pull > 5.0);
    }

    [Fact]
    public void Match_ExactValue_PullIsZero()
    {
        var obs = MakeObservable(value: 1.0, totalSigma: 0.1);
        var target = MakeTarget(value: 1.0, sigma: 0.2);

        var match = TargetMatcher.Match(obs, target, DefaultPolicy);

        Assert.Equal(0.0, match.Pull, precision: 10);
        Assert.True(match.Passed);
    }

    [Fact]
    public void Match_UnestimatedComputedSigma_UseTargetSigmaOnly()
    {
        // Create observable with unestimated total uncertainty
        var obs = new QuantitativeObservableRecord
        {
            ObservableId = "obs-ratio",
            Value = 1.1,
            Uncertainty = new QuantitativeUncertainty(), // all -1
            BranchId = "b1",
            EnvironmentId = "e1",
            ExtractionMethod = "eigenvalue-ratio",
            Provenance = MakeProvenance(),
        };
        var target = MakeTarget(value: 1.0, sigma: 0.3);
        // pull = |1.1 - 1.0| / 0.3 ≈ 0.333
        var match = TargetMatcher.Match(obs, target, DefaultPolicy);

        Assert.True(match.Passed);
        Assert.NotNull(match.Notes);
        Assert.Contains("target sigma only", match.Notes);
    }

    [Fact]
    public void Match_RequireFullUncertainty_UnestimatedFails()
    {
        var obs = new QuantitativeObservableRecord
        {
            ObservableId = "obs-ratio",
            Value = 1.0,
            Uncertainty = new QuantitativeUncertainty(), // all -1
            BranchId = "b1",
            EnvironmentId = "e1",
            ExtractionMethod = "eigenvalue-ratio",
            Provenance = MakeProvenance(),
        };
        var target = MakeTarget(value: 1.0, sigma: 0.3);
        var policy = new CalibrationPolicy
        {
            PolicyId = "strict",
            Mode = "strict",
            SigmaThreshold = 5.0,
            RequireFullUncertainty = true,
        };

        var match = TargetMatcher.Match(obs, target, policy);

        Assert.False(match.Passed);
        Assert.Equal(double.PositiveInfinity, match.Pull);
    }

    [Fact]
    public void Match_TargetAndObsCarryCorrectLabels()
    {
        var obs = MakeObservable(1.0);
        var target = MakeTarget(1.0);
        target = new ExternalTarget
        {
            Label = "my-target",
            ObservableId = target.ObservableId,
            Value = target.Value,
            Uncertainty = target.Uncertainty,
            Source = target.Source,
        };

        var match = TargetMatcher.Match(obs, target, DefaultPolicy);

        Assert.Equal("my-target", match.TargetLabel);
        Assert.Equal(1.0, match.TargetValue);
        Assert.Equal(1.0, match.ComputedValue);
    }

    // ─── WP-11: distribution model tests ───

    [Fact]
    public void Match_Gaussian_ExistingBehaviorUnchanged()
    {
        // Existing Gaussian test still passes: pull = |1.05 - 1.0| / sqrt(0.05^2 + 0.3^2)
        var obs = MakeObservable(value: 1.05, totalSigma: 0.05);
        var target = MakeTarget(value: 1.0, sigma: 0.3);
        // explicit gaussian model (default)
        target = new ExternalTarget
        {
            Label = target.Label, ObservableId = target.ObservableId,
            Value = target.Value, Uncertainty = target.Uncertainty,
            Source = target.Source, DistributionModel = "gaussian",
        };

        var match = TargetMatcher.Match(obs, target, DefaultPolicy);

        Assert.True(match.Passed);
        Assert.True(match.Pull < 1.0);
    }

    [Fact]
    public void Match_AsymmetricGaussian_UsesCorrectSideSigma()
    {
        // computed = 1.5, target = 1.0 → residual > 0 → use upper sigma = 0.1
        // pull = |1.5 - 1.0| / sqrt(0 + 0.1^2) = 5.0 (just fails at 5-sigma threshold)
        var obs = MakeObservable(value: 1.5, totalSigma: 0.0);
        var target = new ExternalTarget
        {
            Label = "asymmetric-target",
            ObservableId = "obs-ratio",
            Value = 1.0,
            Uncertainty = 0.5,       // symmetric fallback — should NOT be used
            UncertaintyLower = 0.5,  // lower sigma (for computed < target)
            UncertaintyUpper = 0.1,  // upper sigma (for computed > target)
            Source = "synthetic-toy-v1",
            DistributionModel = "gaussian-asymmetric",
        };

        var match = TargetMatcher.Match(obs, target, DefaultPolicy);

        // Upper sigma (0.1) used because residual = +0.5
        Assert.NotNull(match.Notes);
        Assert.Contains("upper", match.Notes);
        Assert.Equal(0.1, match.TargetUncertainty, precision: 10);
        // pull = 0.5 / 0.1 = 5.0, which equals sigmaThreshold (5.0) → passes (<=)
        Assert.Equal(5.0, match.Pull, precision: 10);
        Assert.True(match.Passed);

        // Now test with computed < target → lower sigma used
        var obsBelow = MakeObservable(value: 0.5, totalSigma: 0.0);
        var matchBelow = TargetMatcher.Match(obsBelow, target, DefaultPolicy);
        Assert.NotNull(matchBelow.Notes);
        Assert.Contains("lower", matchBelow.Notes);
        Assert.Equal(0.5, matchBelow.TargetUncertainty, precision: 10);
    }

    [Fact]
    public void Match_StudentT_SerializesAndScoresDeterministically()
    {
        // Student-t with nu=5: scale = sqrt(5/3) ≈ 1.2910
        // raw pull = |1.0 - 0.0| / sqrt(0.1^2 + 0.5^2) ≈ 1.0 / 0.5099 ≈ 1.9608
        // scaled pull = 1.9608 / 1.2910 ≈ 1.5188
        var obs = MakeObservable(value: 1.0, totalSigma: 0.1);
        var target = new ExternalTarget
        {
            Label = "student-t-target",
            ObservableId = "obs-ratio",
            Value = 0.0,
            Uncertainty = 0.5,
            Source = "synthetic-toy-v1",
            DistributionModel = "student-t",
            StudentTDegreesOfFreedom = 5.0,
        };

        var match1 = TargetMatcher.Match(obs, target, DefaultPolicy);
        var match2 = TargetMatcher.Match(obs, target, DefaultPolicy);

        // Deterministic: same inputs produce identical result
        Assert.Equal(match1.Pull, match2.Pull, precision: 12);
        Assert.Equal(match1.Passed, match2.Passed);

        // Correct scaling applied
        double nu = 5.0;
        double tScale = System.Math.Sqrt(nu / (nu - 2.0));
        double denom = System.Math.Sqrt(0.1 * 0.1 + 0.5 * 0.5);
        double rawPull = 1.0 / denom;
        double expectedPull = rawPull / tScale;
        Assert.Equal(expectedPull, match1.Pull, precision: 10);

        // Notes contain student-t info
        Assert.NotNull(match1.Notes);
        Assert.Contains("student-t", match1.Notes);
    }
}
