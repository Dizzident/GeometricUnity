using Gu.Core;
using Gu.Phase2.Recovery;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Recovery.Tests;

public sealed class ObservedOutputRecordTests
{
    [Fact]
    public void ObservedOutputRecord_WrapsSnapshot()
    {
        var snapshot = new ObservableSnapshot
        {
            ObservableId = "curvature",
            OutputType = OutputType.ExactStructural,
            Values = [1.0, 2.0, 3.0],
        };

        var record = new ObservedOutputRecord
        {
            OutputId = "out-001",
            Kind = ObservedOutputKind.TensorField,
            Snapshot = snapshot,
            RecoveryNodeId = "e1",
            ClaimCeiling = ClaimClass.ApproximateStructuralSurrogate,
        };

        Assert.Equal("out-001", record.OutputId);
        Assert.Equal(ObservedOutputKind.TensorField, record.Kind);
        Assert.Equal("curvature", record.Snapshot.ObservableId);
        Assert.Equal(ClaimClass.ApproximateStructuralSurrogate, record.ClaimCeiling);
    }

    [Theory]
    [InlineData(ObservedOutputKind.TensorField)]
    [InlineData(ObservedOutputKind.ScalarInvariant)]
    [InlineData(ObservedOutputKind.ModeSpectrum)]
    [InlineData(ObservedOutputKind.ResponseCurve)]
    [InlineData(ObservedOutputKind.StructuralPattern)]
    [InlineData(ObservedOutputKind.ComparisonQuantity)]
    public void AllOutputKinds_Constructible(ObservedOutputKind kind)
    {
        var record = new ObservedOutputRecord
        {
            OutputId = $"out-{kind}",
            Kind = kind,
            Snapshot = new ObservableSnapshot
            {
                ObservableId = "test",
                OutputType = OutputType.ExactStructural,
                Values = [42.0],
            },
            RecoveryNodeId = "e1",
            ClaimCeiling = ClaimClass.SpeculativeInterpretation,
        };

        Assert.Equal(kind, record.Kind);
    }
}
