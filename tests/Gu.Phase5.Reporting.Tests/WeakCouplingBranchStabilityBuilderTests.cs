using Gu.Math;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class WeakCouplingBranchStabilityBuilderTests
{
    private static readonly string[] TargetObservableIds =
    [
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
    ];

    [Fact]
    public void Build_AddsPassingBranchStabilityScore()
    {
        var candidate = MakeUncertaintyCandidate();

        var result = WeakCouplingBranchStabilityBuilder.Build(
            candidate,
            MakeStableVariants(candidate.CouplingValue!.Value),
            relativeTolerance: 0.01);

        Assert.Equal("weak-coupling-branch-stability-derived", result.TerminalStatus);
        Assert.NotNull(result.Candidate);
        Assert.True(result.Candidate!.BranchStabilityScore >= 0.95);
    }

    [Fact]
    public void StableCandidate_PassesPhase61Audit()
    {
        var candidate = MakeUncertaintyCandidate();
        var stability = WeakCouplingBranchStabilityBuilder.Build(
            candidate,
            MakeStableVariants(candidate.CouplingValue!.Value),
            relativeTolerance: 0.01);

        var audit = NormalizedWeakCouplingInputAuditor.Audit([stability.Candidate!], TargetObservableIds);

        Assert.Equal("normalized-weak-coupling-inputs-ready", audit.TerminalStatus);
        Assert.Empty(audit.ClosureRequirements);
    }

    private static NormalizedWeakCouplingCandidateRecord MakeUncertaintyCandidate()
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
        return WeakCouplingUncertaintyPropagator.Propagate(
            extraction,
            rawMatrixElementUncertainty: 0.004,
            generatorNormalizationScaleUncertainty: 0.0).Candidate!;
    }

    private static WeakCouplingBranchVariantValue[] MakeStableVariants(double center) =>
    [
        new() { VariantId = "branch-a", CouplingValue = center },
        new() { VariantId = "branch-b", CouplingValue = center * 1.0002 },
        new() { VariantId = "branch-c", CouplingValue = center * 0.9998 },
    ];
}
