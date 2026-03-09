using Gu.Core.Serialization;
using Gu.Phase2.Stability;

namespace Gu.Phase2.Stability.Tests;

public class GaugeFixedLinearizationRecordTests
{
    [Fact]
    public void Construction_SetsAllProperties()
    {
        var record = new GaugeFixedLinearizationRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            BaseLinearizationId = "lin-1",
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = 10.0,
            GaugeNullDimension = 3,
            GaugeNullSuppressed = true,
            SmallestSliceSingularValue = 0.042,
            ValidationStatus = "verified",
        };

        Assert.Equal("bg-1", record.BackgroundStateId);
        Assert.Equal("branch-1", record.BranchManifestId);
        Assert.Equal("lin-1", record.BaseLinearizationId);
        Assert.Equal("coulomb-slice", record.GaugeHandlingMode);
        Assert.Equal(10.0, record.GaugeLambda);
        Assert.Equal(3, record.GaugeNullDimension);
        Assert.True(record.GaugeNullSuppressed);
        Assert.Equal(0.042, record.SmallestSliceSingularValue);
        Assert.Equal("verified", record.ValidationStatus);
    }

    [Fact]
    public void SmallestSliceSingularValue_IsOptional()
    {
        var record = new GaugeFixedLinearizationRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            BaseLinearizationId = "lin-1",
            GaugeHandlingMode = "gauge-free",
            GaugeLambda = 0.0,
            GaugeNullDimension = 0,
            GaugeNullSuppressed = false,
            ValidationStatus = "unverified",
        };

        Assert.Null(record.SmallestSliceSingularValue);
    }

    [Fact]
    public void JsonRoundTrip_PreservesAllFields()
    {
        var record = new GaugeFixedLinearizationRecord
        {
            BackgroundStateId = "bg-42",
            BranchManifestId = "branch-7",
            BaseLinearizationId = "lin-99",
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = 5.5,
            GaugeNullDimension = 3,
            GaugeNullSuppressed = true,
            SmallestSliceSingularValue = 0.001,
            ValidationStatus = "verified",
        };

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<GaugeFixedLinearizationRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("bg-42", deserialized!.BackgroundStateId);
        Assert.Equal("branch-7", deserialized.BranchManifestId);
        Assert.Equal("lin-99", deserialized.BaseLinearizationId);
        Assert.Equal("coulomb-slice", deserialized.GaugeHandlingMode);
        Assert.Equal(5.5, deserialized.GaugeLambda);
        Assert.Equal(3, deserialized.GaugeNullDimension);
        Assert.True(deserialized.GaugeNullSuppressed);
        Assert.Equal(0.001, deserialized.SmallestSliceSingularValue);
        Assert.Equal("verified", deserialized.ValidationStatus);
    }

    [Fact]
    public void JsonRoundTrip_NullSingularValue()
    {
        var record = new GaugeFixedLinearizationRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            BaseLinearizationId = "lin-1",
            GaugeHandlingMode = "explicit-slice",
            GaugeLambda = 1.0,
            GaugeNullDimension = 0,
            GaugeNullSuppressed = false,
            ValidationStatus = "failed",
        };

        var json = GuJsonDefaults.Serialize(record);
        var deserialized = GuJsonDefaults.Deserialize<GaugeFixedLinearizationRecord>(json);

        Assert.NotNull(deserialized);
        Assert.Null(deserialized!.SmallestSliceSingularValue);
        Assert.Equal("failed", deserialized.ValidationStatus);
    }

    [Fact]
    public void ValidationStatus_PropagatesFromWorkbenchScenario()
    {
        // Simulate different validation outcomes
        var verified = new GaugeFixedLinearizationRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            BaseLinearizationId = "lin-1",
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = 10.0,
            GaugeNullDimension = 3,
            GaugeNullSuppressed = true,
            SmallestSliceSingularValue = 0.05,
            ValidationStatus = "verified",
        };

        var failed = new GaugeFixedLinearizationRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            BaseLinearizationId = "lin-1",
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = 10.0,
            GaugeNullDimension = 3,
            GaugeNullSuppressed = false,
            ValidationStatus = "failed",
        };

        Assert.Equal("verified", verified.ValidationStatus);
        Assert.Equal("failed", failed.ValidationStatus);
        Assert.True(verified.GaugeNullSuppressed);
        Assert.False(failed.GaugeNullSuppressed);
    }
}
