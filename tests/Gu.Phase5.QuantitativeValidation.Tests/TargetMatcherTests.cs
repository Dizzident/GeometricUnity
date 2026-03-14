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
}
