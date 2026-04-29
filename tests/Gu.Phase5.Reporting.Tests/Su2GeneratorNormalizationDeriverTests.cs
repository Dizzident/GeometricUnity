using Gu.Math;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Su2GeneratorNormalizationDeriverTests
{
    [Fact]
    public void Derive_WithTracePairedCanonicalSu2_ReturnsNormalizationConvention()
    {
        var result = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2WithTracePairing());

        Assert.Equal("su2-generator-normalization-derived", result.TerminalStatus);
        Assert.Equal(Su2GeneratorNormalizationDeriver.NormalizationConventionId, result.NormalizationConventionId);
        Assert.Equal(0.5, result.PhysicalTraceMetricDiagonal);
        Assert.Equal(System.Math.Sqrt(0.5), result.InternalToPhysicalGeneratorScale);
        Assert.Empty(result.ClosureRequirements);
    }

    [Fact]
    public void Derive_WithKillingPairedSu2_IsBlocked()
    {
        var result = Su2GeneratorNormalizationDeriver.Derive(LieAlgebraFactory.CreateSu2());

        Assert.Equal("su2-generator-normalization-blocked", result.TerminalStatus);
        Assert.Null(result.NormalizationConventionId);
        Assert.Contains("pairingId must be trace", result.ClosureRequirements);
        Assert.Contains("trace pairing metric must be positive identity in the canonical generator basis", result.ClosureRequirements);
    }
}
