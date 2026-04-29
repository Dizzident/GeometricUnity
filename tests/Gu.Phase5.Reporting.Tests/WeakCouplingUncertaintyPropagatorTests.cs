using Gu.Math;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WeakCouplingUncertaintyPropagatorTests
{
    private static readonly string[] TargetObservableIds =
    [
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
    ];

    [Fact]
    public void Propagate_AddsFiniteUncertaintyToCandidate()
    {
        var extraction = MakeExtraction();

        var result = WeakCouplingUncertaintyPropagator.Propagate(
            extraction,
            rawMatrixElementUncertainty: 0.004,
            generatorNormalizationScaleUncertainty: 0.0);

        Assert.Equal("weak-coupling-uncertainty-propagated", result.TerminalStatus);
        Assert.NotNull(result.Candidate);
        Assert.Equal(0.004 * System.Math.Sqrt(0.5), result.Candidate!.CouplingUncertainty);
        Assert.Null(result.Candidate.BranchStabilityScore);
    }

    [Fact]
    public void PropagatedCandidate_RemainsBlockedOnlyOnBranchStability()
    {
        var propagated = WeakCouplingUncertaintyPropagator.Propagate(
            MakeExtraction(),
            rawMatrixElementUncertainty: 0.004,
            generatorNormalizationScaleUncertainty: 0.0);

        var audit = NormalizedWeakCouplingInputAuditor.Audit([propagated.Candidate!], TargetObservableIds);

        Assert.Equal("normalized-weak-coupling-inputs-blocked", audit.TerminalStatus);
        Assert.DoesNotContain("coupling uncertainty must be finite and non-negative", audit.ClosureRequirements);
        Assert.Contains("branch stability score must be finite and at least 0.95", audit.ClosureRequirements);
    }

    private static DimensionlessWeakCouplingAmplitudeExtractionResult MakeExtraction()
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
            TargetObservableIds);
    }
}
