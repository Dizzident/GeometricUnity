using Gu.Math;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class NonProxyFermionCurrentMatrixElementDeriverTests
{
    [Fact]
    public void Derive_WithCanonicalNormalizationAndAnalyticalVariation_ReturnsMatrixElementSource()
    {
        var normalization = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2WithTracePairing());

        var result = NonProxyFermionCurrentMatrixElementDeriver.Derive(
            normalization,
            analyticDiracVariationAvailable: true,
            usesFiniteDifferenceProxy: false,
            usesCouplingProxyMagnitude: false);

        Assert.Equal("non-proxy-fermion-current-matrix-element-derived", result.TerminalStatus);
        Assert.Equal(NonProxyFermionCurrentMatrixElementDeriver.SourceKind, result.SourceKind);
        Assert.False(result.UsesFiniteDifferenceProxy);
        Assert.False(result.UsesCouplingProxyMagnitude);
        Assert.False(result.ProducesDimensionlessCouplingValue);
        Assert.Empty(result.ClosureRequirements);
    }

    [Fact]
    public void Derive_RejectsFiniteDifferenceProxySource()
    {
        var normalization = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2WithTracePairing());

        var result = NonProxyFermionCurrentMatrixElementDeriver.Derive(
            normalization,
            analyticDiracVariationAvailable: true,
            usesFiniteDifferenceProxy: true,
            usesCouplingProxyMagnitude: true);

        Assert.Equal("non-proxy-fermion-current-matrix-element-blocked", result.TerminalStatus);
        Assert.Contains("matrix element source must not use finite-difference coupling proxies", result.ClosureRequirements);
        Assert.Contains("matrix element source must not use coupling proxy magnitudes", result.ClosureRequirements);
    }
}
