using Gu.Math;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class NormalizedWeakCouplingCandidatePromoterTests
{
    private static readonly string[] TargetObservableIds =
    [
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
    ];

    [Fact]
    public void Promote_WithUncertaintyAndStability_ReturnsAcceptedCandidate()
    {
        var candidate = MakeAcceptedCandidate();

        var result = NormalizedWeakCouplingCandidatePromoter.Promote(candidate, TargetObservableIds);

        Assert.Equal("normalized-weak-coupling-candidate-promoted", result.TerminalStatus);
        Assert.NotNull(result.Candidate);
        Assert.Equal("normalized-weak-coupling-inputs-ready", result.Phase61Audit.TerminalStatus);
        Assert.Empty(result.ClosureRequirements);
    }

    private static NormalizedWeakCouplingCandidateRecord MakeAcceptedCandidate()
    {
        var normalization = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2WithTracePairing());
        var matrixElement = NonProxyFermionCurrentMatrixElementDeriver.Derive(
            normalization,
            analyticDiracVariationAvailable: true,
            usesFiniteDifferenceProxy: false,
            usesCouplingProxyMagnitude: false);
        var extraction = DimensionlessWeakCouplingAmplitudeExtractor.Extract(
            matrixElement,
            normalization,
            rawMatrixElementMagnitude: 0.8,
            TargetObservableIds);
        var uncertainty = WeakCouplingUncertaintyPropagator.Propagate(
            extraction,
            rawMatrixElementUncertainty: 0.004,
            generatorNormalizationScaleUncertainty: 0.0);
        var value = uncertainty.Candidate!.CouplingValue!.Value;
        var stability = WeakCouplingBranchStabilityBuilder.Build(
            uncertainty.Candidate,
            [
                new WeakCouplingBranchVariantValue { VariantId = "branch-a", CouplingValue = value },
                new WeakCouplingBranchVariantValue { VariantId = "branch-b", CouplingValue = value * 1.0002 },
                new WeakCouplingBranchVariantValue { VariantId = "branch-c", CouplingValue = value * 0.9998 },
            ],
            relativeTolerance: 0.01);
        return stability.Candidate!;
    }
}
