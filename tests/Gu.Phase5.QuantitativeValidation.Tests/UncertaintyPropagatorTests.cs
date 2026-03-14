using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.QuantitativeValidation.Tests;

/// <summary>
/// Tests for UncertaintyPropagator (M49).
/// </summary>
public sealed class UncertaintyPropagatorTests
{
    [Fact]
    public void Propagate_AllNull_AllMinusOne()
    {
        var u = UncertaintyPropagator.Propagate(null, null, null, null);

        Assert.Equal(-1, u.BranchVariation);
        Assert.Equal(-1, u.RefinementError);
        Assert.Equal(-1, u.ExtractionError);
        Assert.Equal(-1, u.EnvironmentSensitivity);
        Assert.Equal(-1, u.TotalUncertainty);
        Assert.False(u.IsFullyEstimated);
    }

    [Fact]
    public void Propagate_AllProvided_ComputesQuadratureSum()
    {
        // sqrt(3^2 + 4^2 + 0^2 + 0^2) = 5
        var u = UncertaintyPropagator.Propagate(3.0, 4.0, 0.0, 0.0);

        Assert.Equal(3.0, u.BranchVariation);
        Assert.Equal(4.0, u.RefinementError);
        Assert.Equal(0.0, u.ExtractionError);
        Assert.Equal(0.0, u.EnvironmentSensitivity);
        Assert.Equal(5.0, u.TotalUncertainty, precision: 10);
        Assert.True(u.IsFullyEstimated);
    }

    [Fact]
    public void Propagate_PartialProvided_TotalFromEstimatedOnly()
    {
        // Only branchVariation=3 and refinementError=4 provided → total=5
        var u = UncertaintyPropagator.Propagate(3.0, 4.0, null, null);

        Assert.Equal(3.0, u.BranchVariation);
        Assert.Equal(4.0, u.RefinementError);
        Assert.Equal(-1, u.ExtractionError);
        Assert.Equal(-1, u.EnvironmentSensitivity);
        Assert.Equal(5.0, u.TotalUncertainty, precision: 10);
        Assert.False(u.IsFullyEstimated);
    }

    [Fact]
    public void Propagate_SingleComponent_TotalEqualsThatComponent()
    {
        var u = UncertaintyPropagator.Propagate(null, 7.0, null, null);

        Assert.Equal(7.0, u.TotalUncertainty, precision: 10);
    }
}
