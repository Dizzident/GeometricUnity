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
            TargetProvenance = "my-target-provenance",
            EvidenceTier = "derived-synthetic",
            TargetEnvironmentTier = "toy",
        };

        var match = TargetMatcher.Match(obs, target, DefaultPolicy, computedEnvironmentTier: "toy");

        Assert.Equal("my-target", match.TargetLabel);
        Assert.Equal(1.0, match.TargetValue);
        Assert.Equal(1.0, match.ComputedValue);
        Assert.Equal("env-toy", match.ComputedEnvironmentId);
        Assert.Equal("toy", match.ComputedEnvironmentTier);
        Assert.Equal("derived-synthetic", match.TargetEvidenceTier);
        Assert.Equal("my-target-provenance", match.TargetProvenance);
        Assert.Equal("toy", match.TargetEnvironmentTier);
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

    // ─── P6-M4: Reference study distribution model regression ───

    /// <summary>
    /// The 5 toy-placeholder targets in the reference study external_targets.json all use
    /// distributionModel="gaussian". Verify pull computation is consistent with Gaussian formula.
    /// </summary>
    [Theory]
    [InlineData("bosonic-eigenvalue-ratio-1", 0.1, 0.04)]
    [InlineData("bosonic-eigenvalue-ratio-2", 1.0, 0.4)]
    [InlineData("bosonic-eigenvalue-ratio-3", 5.0, 2.0)]
    [InlineData("fermionic-eigenvalue-ratio-1", 0.05, 0.02)]
    [InlineData("fermionic-eigenvalue-ratio-2", 2.0, 0.8)]
    public void ReferenceStudyToyTargets_GaussianModel_PullMatchesFormula(
        string observableId, double targetValue, double targetSigma)
    {
        double computedValue = targetValue;  // exact match → pull = 0
        var obs = new QuantitativeObservableRecord
        {
            ObservableId = observableId,
            Value = computedValue,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = 0.01,
                RefinementError = 0.01,
                ExtractionError = 0,
                EnvironmentSensitivity = 0,
                TotalUncertainty = 0.02,
            },
            BranchId = "V1-identity-shiab-trivial-torsion-simple-a0-omega",
            EnvironmentId = "env-toy-2d-trivial",
            ExtractionMethod = "eigenvalue-ratio",
            Provenance = MakeProvenance(),
        };
        var target = new ExternalTarget
        {
            Label = observableId + "-toy",
            ObservableId = observableId,
            Value = targetValue,
            Uncertainty = targetSigma,
            Source = "synthetic-toy-v1",
            EvidenceTier = "toy-placeholder",
            DistributionModel = "gaussian",
        };
        var policy = new CalibrationPolicy
        {
            PolicyId = "phase5-sigma5-policy",
            Mode = "sigma",
            SigmaThreshold = 5.0,
        };

        var match = TargetMatcher.Match(obs, target, policy);

        // Exact match → pull = 0
        Assert.Equal(0.0, match.Pull, precision: 10);
        Assert.True(match.Passed);
    }

    /// <summary>
    /// The derived-synthetic target in the reference study uses distributionModel="gaussian"
    /// (physicist guidance: analytically derived ratios have symmetric error propagation;
    /// gaussian-asymmetric requires physical justification for directional uncertainty).
    /// Verify it behaves identically to the standard gaussian formula.
    /// </summary>
    [Fact]
    public void ReferenceStudyDerivedSynthetic_GaussianModel_SymmetricPull()
    {
        // Reference study derived-synthetic target: value=0.08, sigma=0.025
        double targetValue = 0.08;
        double targetSigma = 0.025;
        double computedValue = 0.09;

        var obs = new QuantitativeObservableRecord
        {
            ObservableId = "bosonic-eigenvalue-ratio-1",
            Value = computedValue,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = 0,
                RefinementError = 0,
                ExtractionError = 0,
                EnvironmentSensitivity = 0,
                TotalUncertainty = 0,
            },
            BranchId = "V1",
            EnvironmentId = "env-toy",
            ExtractionMethod = "eigenvalue-ratio",
            Provenance = MakeProvenance(),
        };
        var target = new ExternalTarget
        {
            Label = "bosonic-mode-1-ratio-derived-synthetic",
            ObservableId = "bosonic-eigenvalue-ratio-1",
            Value = targetValue,
            Uncertainty = targetSigma,
            Source = "derived-synthetic-su2-model-v1",
            EvidenceTier = "derived-synthetic",
            DistributionModel = "gaussian",
        };
        var policy = new CalibrationPolicy
        {
            PolicyId = "phase5-sigma5-policy",
            Mode = "sigma",
            SigmaThreshold = 5.0,
        };

        var match = TargetMatcher.Match(obs, target, policy);

        // pull = |0.09 - 0.08| / sqrt(0 + 0.025^2) = 0.01 / 0.025 = 4.0 < 5.0 → passes
        double expectedPull = System.Math.Abs(computedValue - targetValue) / targetSigma;
        Assert.Equal(expectedPull, match.Pull, precision: 10);
        Assert.True(match.Passed);
    }

    /// <summary>
    /// The small-sample student-t target uses distributionModel="student-t" with nu=4
    /// (physicist guidance: nu = N-1 for N=5 samples; scale = sqrt(4/2) = sqrt(2) ≈ 1.414).
    /// Verify the student-t scaling is applied consistently.
    /// </summary>
    [Fact]
    public void ReferenceStudySmallSample_StudentTTarget_Nu4_ScaledPullIsCorrect()
    {
        // Reference study student-t target: value=0.06, sigma=0.03, nu=4 (N-1 for N=5)
        double targetValue = 0.06;
        double targetSigma = 0.03;
        double nu = 4.0;
        double computedValue = 0.09; // 1-sigma raw deviation from target

        var obs = new QuantitativeObservableRecord
        {
            ObservableId = "fermionic-eigenvalue-ratio-1",
            Value = computedValue,
            Uncertainty = new QuantitativeUncertainty
            {
                BranchVariation = 0,
                RefinementError = 0,
                ExtractionError = 0,
                EnvironmentSensitivity = 0,
                TotalUncertainty = 0,
            },
            BranchId = "V1",
            EnvironmentId = "env-toy",
            ExtractionMethod = "eigenvalue-ratio",
            Provenance = MakeProvenance(),
        };
        var target = new ExternalTarget
        {
            Label = "fermionic-mode-1-ratio-small-sample",
            ObservableId = "fermionic-eigenvalue-ratio-1",
            Value = targetValue,
            Uncertainty = targetSigma,
            Source = "synthetic-small-sample-v1",
            EvidenceTier = "toy-placeholder",
            DistributionModel = "student-t",
            StudentTDegreesOfFreedom = nu,
        };
        var policy = new CalibrationPolicy
        {
            PolicyId = "phase5-sigma5-policy",
            Mode = "sigma",
            SigmaThreshold = 5.0,
        };

        var match = TargetMatcher.Match(obs, target, policy);

        // Expected: raw pull = |0.09 - 0.06| / sqrt(0 + 0.03^2) = 1.0
        // scaled pull = 1.0 / sqrt(4/(4-2)) = 1.0 / sqrt(2) ≈ 0.7071
        double tScale = System.Math.Sqrt(nu / (nu - 2.0));
        double rawPull = System.Math.Abs(computedValue - targetValue) / targetSigma;
        double expectedPull = rawPull / tScale;

        Assert.Equal(expectedPull, match.Pull, precision: 10);
        Assert.True(match.Passed);
        Assert.NotNull(match.Notes);
        Assert.Contains("student-t", match.Notes);
        // nu=4: scale = sqrt(2), wider acceptance than gaussian
        Assert.True(tScale > 1.0, "Student-t scale must be > 1 to widen acceptance vs gaussian");
    }
}
