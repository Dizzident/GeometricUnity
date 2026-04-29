using Gu.Core.Serialization;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class ElectroweakBridgeValidatorTests
{
    [Fact]
    public void ValidateForAbsoluteWzProjection_AcceptsNormalizedValidatedBridge()
    {
        var errors = ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection(MakeValidBridge());

        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateForAbsoluteWzProjection_RejectsCouplingProfileMeanMagnitude()
    {
        var bridge = MakeBridge(inputKind: "coupling-profile-mean-magnitude");

        var errors = ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection(bridge);

        Assert.Contains(errors, e => e.Contains("rejected", StringComparison.Ordinal));
    }

    [Fact]
    public void ValidateForAbsoluteWzProjection_RejectsMissingTargetExclusions()
    {
        var bridge = new ElectroweakBridgeRecord
        {
            BridgeObservableId = "bridge",
            SourceModeIds = ["phase22-phase12-candidate-0", "phase22-phase12-candidate-2"],
            DimensionlessBridgeValue = 0.65,
            DimensionlessBridgeUncertainty = 0.01,
            InputKind = "normalized-internal-weak-coupling",
            WeakCouplingNormalizationConvention = "test-normalization",
            MassGenerationRelation = "test-relation",
            ExcludedTargetObservableIds = [],
            Status = "validated",
            Assumptions = [],
            ClosureRequirements = [],
        };

        var errors = ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection(bridge);

        Assert.Contains("bridge must exclude physical-w-boson-mass-gev", errors);
        Assert.Contains("bridge must exclude physical-z-boson-mass-gev", errors);
    }

    [Fact]
    public void ValidateForAbsoluteWzProjection_RejectsUnestimatedUncertainty()
    {
        var bridge = MakeBridge(uncertainty: -1);

        var errors = ElectroweakBridgeValidator.ValidateForAbsoluteWzProjection(bridge);

        Assert.Contains("dimensionlessBridgeUncertainty must be finite and non-negative", errors);
    }

    [Fact]
    public void ElectroweakBridgeTable_JsonRoundTrip_PreservesBridgeFields()
    {
        var table = new ElectroweakBridgeTable
        {
            TableId = "bridges",
            Bridges = [MakeValidBridge()],
        };

        var json = GuJsonDefaults.Serialize(table);
        var roundTrip = GuJsonDefaults.Deserialize<ElectroweakBridgeTable>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal("bridges", roundTrip!.TableId);
        Assert.Equal("phase56-test-normalized-bridge", roundTrip.Bridges[0].BridgeObservableId);
        Assert.Equal("normalized-internal-weak-coupling", roundTrip.Bridges[0].InputKind);
    }

    private static ElectroweakBridgeRecord MakeValidBridge()
        => MakeBridge();

    private static ElectroweakBridgeRecord MakeBridge(
        string inputKind = "normalized-internal-weak-coupling",
        double uncertainty = 0.01) => new()
    {
        BridgeObservableId = "phase56-test-normalized-bridge",
        SourceModeIds = ["phase22-phase12-candidate-0", "phase22-phase12-candidate-2"],
        DimensionlessBridgeValue = 0.65,
        DimensionlessBridgeUncertainty = uncertainty,
        InputKind = inputKind,
        WeakCouplingNormalizationConvention = "test-normalized-internal-weak-coupling",
        MassGenerationRelation = "test-mass-generation-relation",
        ExcludedTargetObservableIds = ["physical-w-boson-mass-gev", "physical-z-boson-mass-gev"],
        Status = "validated",
        Assumptions = ["unit test bridge"],
        ClosureRequirements = [],
    };
}
