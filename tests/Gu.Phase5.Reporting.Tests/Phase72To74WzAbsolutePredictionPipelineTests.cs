using Gu.Core;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase72To74WzAbsolutePredictionPipelineTests
{
    [Fact]
    public void Phase71Bridge_BuildsCalibrationAndProjectsAbsoluteMasses()
    {
        var calibration = ElectroweakAbsoluteScaleCalibrationBuilder.BuildForWzAbsoluteMasses(
            MakePhase71Bridge(),
            externalElectroweakScaleGeV: 246.21965079413738,
            externalElectroweakScaleUncertaintyGeV: 0.00006332925595969921,
            MakeMapping("phase72-w-mapping", "w-boson", "phase22-phase12-candidate-0", "physical-w-boson-mass-gev"),
            MakeMapping("phase72-z-mapping", "z-boson", "phase22-phase12-candidate-2", "physical-z-boson-mass-gev"));

        Assert.Equal("validated", calibration.Status);
        Assert.Equal(62413568563037690, calibration.ScaleFactorGeVPerInternalMassUnit!.Value, precision: 0);

        var projection = WzAbsoluteMassObservableProjector.Project(
            [
                ElectroweakMassGenerationRelationDeriverTests.MakeMode("phase22-phase12-candidate-0", "w-boson", 1.1158059937692792E-15, 1.3476944083805217E-18),
                ElectroweakMassGenerationRelationDeriverTests.MakeMode("phase22-phase12-candidate-2", "z-boson", 1.268406657962647E-15, 1.5805767578887164E-18),
            ],
            calibration);

        Assert.Equal("projected", projection.Status);
        Assert.Equal(69.64143389516731, projection.Observables.Single(o => o.ObservableId == "physical-w-boson-mass-gev").Value, precision: 12);
        Assert.Equal(79.16578591256517, projection.Observables.Single(o => o.ObservableId == "physical-z-boson-mass-gev").Value, precision: 12);
    }

    [Fact]
    public void Phase71ProjectedMasses_FailPdgTargetComparison()
    {
        var comparison = WzAbsoluteMassTargetComparator.Compare(
            [
                MakePrediction("physical-w-boson-mass-gev", 69.64143389516731, 0.3679656283006216),
                MakePrediction("physical-z-boson-mass-gev", 79.16578591256517, 0.4189929362561984),
            ],
            [
                new WzAbsoluteMassTargetRecord { ObservableId = "physical-w-boson-mass-gev", TargetValue = 80.3692, TargetUncertainty = 0.0133 },
                new WzAbsoluteMassTargetRecord { ObservableId = "physical-z-boson-mass-gev", TargetValue = 91.1880, TargetUncertainty = 0.0020 },
            ]);

        Assert.Equal("wz-absolute-mass-target-comparison-failed", comparison.TerminalStatus);
        Assert.All(comparison.Comparisons, c => Assert.False(c.Passed));
    }

    internal static ElectroweakBridgeRecord MakePhase71Bridge() => new()
    {
        BridgeObservableId = "phase71-validated-electroweak-mass-generation-bridge",
        SourceModeIds = ["phase22-phase12-candidate-0", "phase22-phase12-candidate-2"],
        DimensionlessBridgeValue = 253487357169640.62,
        DimensionlessBridgeUncertainty = 1303892067413.5,
        InputKind = "validated-internal-mass-generation-relation",
        WeakCouplingNormalizationConvention = "physical-weak-coupling-normalization:su2-canonical-trace-half-v1",
        MassGenerationRelation = "mass-generation:electroweak-vev-times-normalized-weak-coupling:v1",
        ExcludedTargetObservableIds = ["physical-w-boson-mass-gev", "physical-z-boson-mass-gev"],
        Status = "validated",
        Assumptions = ["test"],
        ClosureRequirements = [],
    };

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
        Assumptions = [],
        ClosureRequirements = [],
    };

    private static QuantitativeObservableRecord MakePrediction(string observableId, double value, double uncertainty) => new()
    {
        ObservableId = observableId,
        Value = value,
        Uncertainty = new QuantitativeUncertainty { TotalUncertainty = uncertainty },
        BranchId = "branch",
        EnvironmentId = "env",
        RefinementLevel = "L0",
        ExtractionMethod = "test",
        DistributionModel = "gaussian",
        Provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.Parse("2026-04-29T00:00:00Z"),
            CodeRevision = "test",
            Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
            Backend = "cpu",
        },
    };
}
