using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class ElectroweakAbsoluteScaleCalibrationBuilderTests
{
    [Fact]
    public void BuildForWzAbsoluteMasses_ProducesSharedWzScaleForValidBridge()
    {
        var result = ElectroweakAbsoluteScaleCalibrationBuilder.BuildForWzAbsoluteMasses(
            MakeValidBridge(),
            externalElectroweakScaleGeV: 246.21965079413738,
            externalElectroweakScaleUncertaintyGeV: 0.00006332925595969921,
            MakeMapping("map-w", "w-boson", "w-internal", "physical-w-boson-mass-gev"),
            MakeMapping("map-z", "z-boson", "z-internal", "physical-z-boson-mass-gev"));

        Assert.Equal("validated", result.Status);
        Assert.Equal(160.0427730161893, result.ScaleFactorGeVPerInternalMassUnit!.Value, precision: 12);
        Assert.Equal(2.4621965082854724, result.ScaleUncertaintyGeVPerInternalMassUnit!.Value, precision: 12);
        Assert.Equal(2, result.Calibrations.Calibrations.Count);
        Assert.All(result.Calibrations.Calibrations, calibration =>
        {
            Assert.Equal("validated", calibration.Status);
            Assert.Equal(result.ScaleFactorGeVPerInternalMassUnit.Value, calibration.ScaleFactor);
            Assert.Equal(result.ScaleUncertaintyGeVPerInternalMassUnit.Value, calibration.ScaleUncertainty);
            Assert.Empty(calibration.ClosureRequirements);
        });
    }

    [Fact]
    public void BuildForWzAbsoluteMasses_BlocksRejectedBridge()
    {
        var result = ElectroweakAbsoluteScaleCalibrationBuilder.BuildForWzAbsoluteMasses(
            MakeBridge(inputKind: "coupling-profile-mean-magnitude"),
            externalElectroweakScaleGeV: 246.21965079413738,
            externalElectroweakScaleUncertaintyGeV: 0.00006332925595969921,
            MakeMapping("map-w", "w-boson", "w-internal", "physical-w-boson-mass-gev"),
            MakeMapping("map-z", "z-boson", "z-internal", "physical-z-boson-mass-gev"));

        Assert.Equal("blocked", result.Status);
        Assert.Empty(result.Calibrations.Calibrations);
        Assert.Contains(result.BlockReasons, reason => reason.Contains("rejected", StringComparison.Ordinal));
    }

    [Fact]
    public void BuildForWzAbsoluteMasses_BlocksWrongMappings()
    {
        var result = ElectroweakAbsoluteScaleCalibrationBuilder.BuildForWzAbsoluteMasses(
            MakeValidBridge(),
            externalElectroweakScaleGeV: 246.21965079413738,
            externalElectroweakScaleUncertaintyGeV: 0.00006332925595969921,
            MakeMapping("map-w", "w-boson", "w-internal", "physical-w-boson-mass-gev"),
            MakeMapping("map-z", "z-boson", "z-internal", "physical-w-boson-mass-gev"));

        Assert.Equal("blocked", result.Status);
        Assert.Contains("Z mapping must target physical-z-boson-mass-gev", result.BlockReasons);
    }

    private static PhysicalObservableMapping MakeMapping(
        string mappingId,
        string particleId,
        string sourceObservableId,
        string targetObservableId) => new()
    {
        MappingId = mappingId,
        ParticleId = particleId,
        PhysicalObservableType = "mass",
        SourceComputedObservableId = sourceObservableId,
        TargetPhysicalObservableId = targetObservableId,
        UnitFamily = "mass-energy",
        Status = "validated",
        Assumptions = ["test mapping"],
        ClosureRequirements = [],
    };

    private static ElectroweakBridgeRecord MakeValidBridge()
        => MakeBridge();

    private static ElectroweakBridgeRecord MakeBridge(
        string inputKind = "normalized-internal-weak-coupling") => new()
    {
        BridgeObservableId = "test-electroweak-bridge",
        SourceModeIds = ["phase22-phase12-candidate-0", "phase22-phase12-candidate-2"],
        DimensionlessBridgeValue = 0.65,
        DimensionlessBridgeUncertainty = 0.01,
        InputKind = inputKind,
        WeakCouplingNormalizationConvention = "test-normalization",
        MassGenerationRelation = "test-mass-generation",
        ExcludedTargetObservableIds = ["physical-w-boson-mass-gev", "physical-z-boson-mass-gev"],
        Status = "validated",
        Assumptions = ["test bridge"],
        ClosureRequirements = [],
    };
}
