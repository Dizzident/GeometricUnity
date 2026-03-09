using Gu.Core.Serialization;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Stability.Tests;

public class HessianSummaryTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var summary = new HessianSummary
        {
            SmallestEigenvalue = -0.05,
            NegativeModeCount = 1,
            SoftModeCount = 2,
            NearKernelCount = 0,
            StabilityClassification = "negative-modes-saddle",
            GaugeHandlingMode = "coulomb-slice",
        };

        Assert.Equal(-0.05, summary.SmallestEigenvalue);
        Assert.Equal(1, summary.NegativeModeCount);
        Assert.Equal(2, summary.SoftModeCount);
        Assert.Equal(0, summary.NearKernelCount);
        Assert.Equal("negative-modes-saddle", summary.StabilityClassification);
        Assert.Equal("coulomb-slice", summary.GaugeHandlingMode);
    }

    [Fact]
    public void JsonRoundTrip_PreservesAllFields()
    {
        var summary = new HessianSummary
        {
            SmallestEigenvalue = 0.123,
            NegativeModeCount = 0,
            SoftModeCount = 3,
            NearKernelCount = 1,
            StabilityClassification = "soft-modes-present",
            GaugeHandlingMode = "coulomb-slice",
        };

        var json = GuJsonDefaults.Serialize(summary);
        var deserialized = GuJsonDefaults.Deserialize<HessianSummary>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(0.123, deserialized!.SmallestEigenvalue);
        Assert.Equal(0, deserialized.NegativeModeCount);
        Assert.Equal(3, deserialized.SoftModeCount);
        Assert.Equal(1, deserialized.NearKernelCount);
        Assert.Equal("soft-modes-present", deserialized.StabilityClassification);
        Assert.Equal("coulomb-slice", deserialized.GaugeHandlingMode);
    }

    [Fact]
    public void StrictlyPositive_HasNoNegativeOrSoftModes()
    {
        var summary = new HessianSummary
        {
            SmallestEigenvalue = 1.5,
            NegativeModeCount = 0,
            SoftModeCount = 0,
            NearKernelCount = 0,
            StabilityClassification = "strictly-positive-on-slice",
            GaugeHandlingMode = "coulomb-slice",
        };

        Assert.Equal(0, summary.NegativeModeCount);
        Assert.Equal(0, summary.SoftModeCount);
        Assert.Equal("strictly-positive-on-slice", summary.StabilityClassification);
    }

    [Fact]
    public void TwoDifferentSummaries_HaveDifferentClassifications()
    {
        var stable = new HessianSummary
        {
            SmallestEigenvalue = 1.0,
            NegativeModeCount = 0,
            SoftModeCount = 0,
            NearKernelCount = 0,
            StabilityClassification = "strictly-positive-on-slice",
            GaugeHandlingMode = "coulomb-slice",
        };

        var saddle = new HessianSummary
        {
            SmallestEigenvalue = -0.5,
            NegativeModeCount = 2,
            SoftModeCount = 0,
            NearKernelCount = 0,
            StabilityClassification = "negative-modes-saddle",
            GaugeHandlingMode = "coulomb-slice",
        };

        Assert.NotEqual(stable.StabilityClassification, saddle.StabilityClassification);
        Assert.NotEqual(stable.SmallestEigenvalue, saddle.SmallestEigenvalue);
        Assert.NotEqual(stable.NegativeModeCount, saddle.NegativeModeCount);
    }
}
