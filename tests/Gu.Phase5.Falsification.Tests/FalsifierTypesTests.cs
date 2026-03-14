using Gu.Phase5.Falsification;

namespace Gu.Phase5.Falsification.Tests;

/// <summary>
/// Tests for FalsifierTypes and FalsifierSeverity string constants (M50).
/// </summary>
public sealed class FalsifierTypesTests
{
    [Fact]
    public void FalsifierTypes_AllConstantsDefined()
    {
        Assert.Equal("branch-fragility", FalsifierTypes.BranchFragility);
        Assert.Equal("non-convergence", FalsifierTypes.NonConvergence);
        Assert.Equal("observation-instability", FalsifierTypes.ObservationInstability);
        Assert.Equal("environment-instability", FalsifierTypes.EnvironmentInstability);
        Assert.Equal("quantitative-mismatch", FalsifierTypes.QuantitativeMismatch);
        Assert.Equal("representation-content", FalsifierTypes.RepresentationContent);
        Assert.Equal("coupling-inconsistency", FalsifierTypes.CouplingInconsistency);
    }

    [Fact]
    public void FalsifierSeverity_AllConstantsDefined()
    {
        Assert.Equal("fatal", FalsifierSeverity.Fatal);
        Assert.Equal("high", FalsifierSeverity.High);
        Assert.Equal("medium", FalsifierSeverity.Medium);
        Assert.Equal("low", FalsifierSeverity.Low);
        Assert.Equal("informational", FalsifierSeverity.Informational);
    }
}
