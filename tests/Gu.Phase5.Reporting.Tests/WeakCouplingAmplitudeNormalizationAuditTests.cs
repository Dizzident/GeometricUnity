using Gu.Math;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WeakCouplingAmplitudeNormalizationAuditTests
{
    private static readonly string[] TargetObservableIds =
    [
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
    ];

    [Fact]
    public void Audit_Phase65AndPhase75Inputs_BlocksOnRawMatrixOrScalarRelation()
    {
        var extraction = MakePhase65Extraction();
        var diagnostic = MakePhase75Diagnostic();

        var result = WeakCouplingAmplitudeNormalizationAudit.Audit(extraction, diagnostic);

        Assert.Equal("weak-coupling-amplitude-normalization-audit-blocked", result.TerminalStatus);
        Assert.False(result.GeneratorNormalizationCanExplainMiss);
        Assert.Equal(0.9223616409512609, result.TargetImpliedRawMatrixElementMagnitude!.Value, precision: 12);
        Assert.Equal(1.152952051189076, result.RawMatrixElementRequiredScale!.Value, precision: 12);
        Assert.Equal(0.8152602137787353, result.TargetImpliedGeneratorScaleIfRawHeldFixed!.Value, precision: 12);
        Assert.Contains(result.Diagnosis, d => d.Contains("cannot explain the coherent W/Z miss", StringComparison.Ordinal));
        Assert.Contains(result.ClosureRequirements, c => c.Contains("replayed analytic matrix-element evaluation", StringComparison.Ordinal));
    }

    private static DimensionlessWeakCouplingAmplitudeExtractionResult MakePhase65Extraction()
    {
        var normalization = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2WithTracePairing());
        var matrixElement = NonProxyFermionCurrentMatrixElementDeriver.Derive(
            normalization,
            analyticDiracVariationAvailable: true,
            usesFiniteDifferenceProxy: false,
            usesCouplingProxyMagnitude: false);

        return DimensionlessWeakCouplingAmplitudeExtractor.Extract(
            matrixElement,
            normalization,
            rawMatrixElementMagnitude: 0.8,
            TargetObservableIds,
            provenanceId: "test");
    }

    private static WzAbsoluteMassMissDiagnosticResult MakePhase75Diagnostic()
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

        return WzAbsoluteMassMissDiagnostic.Diagnose(comparison, currentWeakCoupling: 0.5656854249492381);
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
