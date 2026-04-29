using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WzAbsoluteMassMissDiagnosticTests
{
    [Fact]
    public void Diagnose_CoherentMiss_PrioritizesWeakCouplingNormalization()
    {
        var comparison = new WzAbsoluteMassTargetComparisonResult
        {
            AlgorithmId = WzAbsoluteMassTargetComparator.AlgorithmId,
            TerminalStatus = "wz-absolute-mass-target-comparison-failed",
            SigmaThreshold = 5,
            Comparisons =
            [
                MakeComparison("physical-w-boson-mass-gev", 69.64143389516731, 80.3692),
                MakeComparison("physical-z-boson-mass-gev", 79.16578591256517, 91.1880),
            ],
            ClosureRequirements = ["one or more W/Z absolute mass predictions fail the sigma threshold"],
        };

        var result = WzAbsoluteMassMissDiagnostic.Diagnose(comparison, currentWeakCoupling: 0.5656854249492381);

        Assert.Equal("wz-absolute-mass-miss-diagnosed", result.TerminalStatus);
        Assert.Equal(1.152952051189076, result.MeanRequiredScaleFactor!.Value, precision: 12);
        Assert.Equal(0.6522081710229882, result.RequiredWeakCoupling!.Value, precision: 12);
        Assert.Contains(result.Diagnosis, d => d.Contains("prioritize weak-coupling amplitude normalization", StringComparison.Ordinal));
    }

    private static WzAbsoluteMassComparisonRecord MakeComparison(string observableId, double predicted, double target) => new()
    {
        ObservableId = observableId,
        PredictedValue = predicted,
        PredictedUncertainty = 0.1,
        TargetValue = target,
        TargetUncertainty = 0.01,
        Delta = predicted - target,
        CombinedUncertainty = 0.1,
        SigmaResidual = System.Math.Abs(predicted - target) / 0.1,
        Passed = false,
    };
}
