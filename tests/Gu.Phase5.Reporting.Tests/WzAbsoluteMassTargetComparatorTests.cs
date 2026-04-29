using Gu.Core;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzAbsoluteMassTargetComparatorTests
{
    [Fact]
    public void Compare_ReturnsPassedWhenPredictionsAreWithinThreshold()
    {
        var result = WzAbsoluteMassTargetComparator.Compare(
            [
                MakePrediction("physical-w-boson-mass-gev", 80.37, 0.02),
                MakePrediction("physical-z-boson-mass-gev", 91.188, 0.02),
            ],
            [
                new WzAbsoluteMassTargetRecord { ObservableId = "physical-w-boson-mass-gev", TargetValue = 80.3692, TargetUncertainty = 0.0133 },
                new WzAbsoluteMassTargetRecord { ObservableId = "physical-z-boson-mass-gev", TargetValue = 91.1880, TargetUncertainty = 0.0020 },
            ]);

        Assert.Equal("wz-absolute-mass-target-comparison-passed", result.TerminalStatus);
        Assert.All(result.Comparisons, c => Assert.True(c.Passed));
    }

    [Fact]
    public void Compare_ReturnsFailedWhenPredictionMissesThreshold()
    {
        var result = WzAbsoluteMassTargetComparator.Compare(
            [MakePrediction("physical-w-boson-mass-gev", 69.64, 0.37)],
            [new WzAbsoluteMassTargetRecord { ObservableId = "physical-w-boson-mass-gev", TargetValue = 80.3692, TargetUncertainty = 0.0133 }]);

        Assert.Equal("wz-absolute-mass-target-comparison-failed", result.TerminalStatus);
        Assert.False(result.Comparisons.Single().Passed);
        Assert.Contains("one or more W/Z absolute mass predictions fail the sigma threshold", result.ClosureRequirements);
    }

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
