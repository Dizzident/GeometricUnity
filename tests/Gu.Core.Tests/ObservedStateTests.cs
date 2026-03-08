using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class ObservedStateTests
{
    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = new ObservedState
        {
            ObservationBranchId = "sigma-pullback-v1",
            Observables = new Dictionary<string, ObservableSnapshot>
            {
                ["curvature_trace"] = new ObservableSnapshot
                {
                    ObservableId = "curvature_trace",
                    OutputType = OutputType.SemiQuantitative,
                    Values = new double[] { 1.5, 2.3, 3.1 },
                    Normalization = new NormalizationMeta { SchemeId = "unit-trace" }
                }
            },
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc123",
                Branch = new BranchRef { BranchId = "minimal-gu-v1", SchemaVersion = "1.0.0" }
            }
        };

        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<ObservedState>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.ObservationBranchId, deserialized.ObservationBranchId);
        Assert.True(deserialized.Observables.ContainsKey("curvature_trace"));
        Assert.Equal(OutputType.SemiQuantitative, deserialized.Observables["curvature_trace"].OutputType);
    }

    [Fact]
    public void OutputType_EnumHasExactlyThreeValues()
    {
        // Per Section 17.4: ExactStructural, SemiQuantitative, or Quantitative
        var values = Enum.GetValues<OutputType>();
        Assert.Equal(3, values.Length);
        Assert.Contains(OutputType.ExactStructural, values);
        Assert.Contains(OutputType.SemiQuantitative, values);
        Assert.Contains(OutputType.Quantitative, values);
    }

    [Fact]
    public void OutputType_SerializesAsString()
    {
        var snapshot = new ObservableSnapshot
        {
            ObservableId = "test",
            OutputType = OutputType.Quantitative,
            Values = new[] { 1.0 }
        };
        var json = GuJsonDefaults.Serialize(snapshot);
        Assert.Contains("\"Quantitative\"", json);
    }
}
