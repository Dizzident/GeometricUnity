using Gu.Math;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class DimensionlessWeakCouplingAmplitudeExtractorTests
{
    private static readonly string[] TargetObservableIds =
    [
        "physical-w-boson-mass-gev",
        "physical-z-boson-mass-gev",
    ];

    [Fact]
    public void Extract_WithDerivedInputs_EmitsNormalizedCandidateShape()
    {
        var normalization = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2WithTracePairing());
        var matrixElement = NonProxyFermionCurrentMatrixElementDeriver.Derive(
            normalization,
            analyticDiracVariationAvailable: true,
            usesFiniteDifferenceProxy: false,
            usesCouplingProxyMagnitude: false);

        var result = DimensionlessWeakCouplingAmplitudeExtractor.Extract(
            matrixElement,
            normalization,
            rawMatrixElementMagnitude: 0.8,
            TargetObservableIds,
            provenanceId: "test");

        Assert.Equal("dimensionless-weak-coupling-amplitude-extracted", result.TerminalStatus);
        Assert.NotNull(result.Candidate);
        Assert.Equal(NormalizedWeakCouplingInputAuditor.AcceptedSourceKind, result.Candidate!.SourceKind);
        Assert.Equal(Su2GeneratorNormalizationDeriver.NormalizationConventionId, result.Candidate.NormalizationConvention);
        Assert.Equal(0.8 * System.Math.Sqrt(0.5), result.Candidate.CouplingValue);
        Assert.Null(result.Candidate.CouplingUncertainty);
        Assert.Null(result.Candidate.BranchStabilityScore);
    }

    [Fact]
    public void ExtractedCandidate_RemainsBlockedByPhase61UntilUncertaintyAndStabilityExist()
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

        var audit = NormalizedWeakCouplingInputAuditor.Audit([extraction.Candidate!], TargetObservableIds);

        Assert.Equal("normalized-weak-coupling-inputs-blocked", audit.TerminalStatus);
        Assert.Contains("coupling uncertainty must be finite and non-negative", audit.ClosureRequirements);
        Assert.Contains("branch stability score must be finite and at least 0.95", audit.ClosureRequirements);
    }

    [Fact]
    public void Extract_RejectsProxyContaminatedMatrixElement()
    {
        var normalization = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2WithTracePairing());
        var matrixElement = NonProxyFermionCurrentMatrixElementDeriver.Derive(
            normalization,
            analyticDiracVariationAvailable: true,
            usesFiniteDifferenceProxy: true,
            usesCouplingProxyMagnitude: false);

        var result = DimensionlessWeakCouplingAmplitudeExtractor.Extract(
            matrixElement,
            normalization,
            rawMatrixElementMagnitude: 0.8,
            TargetObservableIds);

        Assert.Equal("dimensionless-weak-coupling-amplitude-blocked", result.TerminalStatus);
        Assert.Null(result.Candidate);
        Assert.Contains("non-proxy fermion-current matrix element has not been derived", result.ClosureRequirements);
        Assert.Contains("matrix element source must not use finite-difference coupling proxies", result.ClosureRequirements);
    }
}
