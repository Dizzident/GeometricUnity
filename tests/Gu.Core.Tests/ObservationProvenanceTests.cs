using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class ObservationProvenanceTests
{
    [Fact]
    public void RoundTrip_Serialization()
    {
        var prov = new ObservationProvenance
        {
            PullbackOperatorId = "sigma_h_star",
            ObservationBranchId = "sigma-pullback",
            TransformId = "norm-squared",
            IsVerified = true,
            PipelineTimestamp = new DateTimeOffset(2025, 6, 15, 12, 0, 0, TimeSpan.Zero),
        };

        var json = GuJsonDefaults.Serialize(prov);
        var deserialized = GuJsonDefaults.Deserialize<ObservationProvenance>(json);

        Assert.NotNull(deserialized);
        Assert.Equal("sigma_h_star", deserialized.PullbackOperatorId);
        Assert.Equal("sigma-pullback", deserialized.ObservationBranchId);
        Assert.Equal("norm-squared", deserialized.TransformId);
        Assert.True(deserialized.IsVerified);
        Assert.Equal(prov.PipelineTimestamp, deserialized.PipelineTimestamp);
    }

    [Fact]
    public void CamelCase_PropertyNames()
    {
        var prov = new ObservationProvenance
        {
            PullbackOperatorId = "sigma_h_star",
            ObservationBranchId = "test",
            IsVerified = true,
            PipelineTimestamp = DateTimeOffset.UtcNow,
        };

        var json = GuJsonDefaults.Serialize(prov);

        Assert.Contains("\"pullbackOperatorId\"", json);
        Assert.Contains("\"observationBranchId\"", json);
        Assert.Contains("\"isVerified\"", json);
        Assert.Contains("\"pipelineTimestamp\"", json);
    }

    [Fact]
    public void NullTransformId_OmittedInJson()
    {
        var prov = new ObservationProvenance
        {
            PullbackOperatorId = "sigma_h_star",
            ObservationBranchId = "test",
            TransformId = null,
            IsVerified = true,
            PipelineTimestamp = DateTimeOffset.UtcNow,
        };

        var json = GuJsonDefaults.Serialize(prov);

        Assert.DoesNotContain("\"transformId\"", json);
    }
}

public class OutputTypeTests
{
    [Fact]
    public void SerializesAsString()
    {
        var snap = new ObservableSnapshot
        {
            ObservableId = "test",
            OutputType = OutputType.ExactStructural,
            Values = new double[] { 1.0 },
        };

        var json = GuJsonDefaults.Serialize(snap);
        Assert.Contains("\"ExactStructural\"", json);
    }

    [Theory]
    [InlineData(OutputType.ExactStructural)]
    [InlineData(OutputType.SemiQuantitative)]
    [InlineData(OutputType.Quantitative)]
    public void AllValues_RoundTrip(OutputType ot)
    {
        var snap = new ObservableSnapshot
        {
            ObservableId = "test",
            OutputType = ot,
            Values = new double[] { 1.0 },
        };

        var json = GuJsonDefaults.Serialize(snap);
        var deserialized = GuJsonDefaults.Deserialize<ObservableSnapshot>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(ot, deserialized.OutputType);
    }
}
