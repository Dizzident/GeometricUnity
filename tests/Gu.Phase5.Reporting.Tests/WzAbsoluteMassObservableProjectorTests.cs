using Gu.Core;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzAbsoluteMassObservableProjectorTests
{
    [Fact]
    public void Project_EmitsAbsoluteWzMassObservablesForValidatedInputs()
    {
        var result = WzAbsoluteMassObservableProjector.Project(
            [MakeMode("w-mode", "w-boson", 2.0, 0.1), MakeMode("z-mode", "z-boson", 3.0, 0.2)],
            MakeCalibrationBuild(scale: 10.0, scaleUncertainty: 0.5));

        Assert.Equal("projected", result.Status);
        Assert.Empty(result.BlockReasons);

        var w = result.Observables.Single(o => o.ObservableId == "physical-w-boson-mass-gev");
        Assert.Equal(20.0, w.Value);
        Assert.Equal(System.Math.Sqrt(2.0), w.Uncertainty.TotalUncertainty, precision: 12);

        var z = result.Observables.Single(o => o.ObservableId == "physical-z-boson-mass-gev");
        Assert.Equal(30.0, z.Value);
        Assert.Equal(2.5, z.Uncertainty.TotalUncertainty, precision: 12);
    }

    [Fact]
    public void Project_BlocksWhenCalibrationBuildIsNotValidated()
    {
        var result = WzAbsoluteMassObservableProjector.Project(
            [MakeMode("w-mode", "w-boson", 2.0, 0.1), MakeMode("z-mode", "z-boson", 3.0, 0.2)],
            new ElectroweakAbsoluteScaleCalibrationBuildResult
            {
                Status = "blocked",
                ScaleFactorGeVPerInternalMassUnit = null,
                ScaleUncertaintyGeVPerInternalMassUnit = null,
                Calibrations = new PhysicalCalibrationTable { TableId = "empty", Calibrations = [] },
                BlockReasons = ["test"],
            });

        Assert.Equal("blocked", result.Status);
        Assert.Empty(result.Observables);
        Assert.Contains("electroweak absolute scale calibration build is not validated", result.BlockReasons);
    }

    [Fact]
    public void Project_BlocksWhenValidatedModeIsMissing()
    {
        var result = WzAbsoluteMassObservableProjector.Project(
            [MakeMode("w-mode", "w-boson", 2.0, 0.1)],
            MakeCalibrationBuild(scale: 10.0, scaleUncertainty: 0.5));

        Assert.Equal("blocked", result.Status);
        Assert.Contains("validated Z physical mode is missing", result.BlockReasons);
    }

    private static ElectroweakAbsoluteScaleCalibrationBuildResult MakeCalibrationBuild(
        double scale,
        double scaleUncertainty) => new()
    {
        Status = "validated",
        ScaleFactorGeVPerInternalMassUnit = scale,
        ScaleUncertaintyGeVPerInternalMassUnit = scaleUncertainty,
        Calibrations = new PhysicalCalibrationTable { TableId = "calibrations", Calibrations = [] },
        BlockReasons = [],
    };

    private static IdentifiedPhysicalModeRecord MakeMode(
        string modeId,
        string particleId,
        double value,
        double uncertainty) => new()
    {
        ModeId = modeId,
        ParticleId = particleId,
        ModeKind = "vector-boson-mass-mode",
        ObservableId = modeId,
        Value = value,
        Uncertainty = uncertainty,
        UnitFamily = "mass-energy",
        Unit = "internal-mass-unit",
        Status = "validated",
        EnvironmentId = "env",
        BranchId = "branch",
        RefinementLevel = "L0",
        ExtractionMethod = "test",
        Assumptions = [],
        ClosureRequirements = [],
        Provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UnixEpoch,
            CodeRevision = "test",
            Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
            Backend = "cpu",
        },
    };
}
