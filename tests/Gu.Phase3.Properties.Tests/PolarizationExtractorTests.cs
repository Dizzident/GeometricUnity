namespace Gu.Phase3.Properties.Tests;

public class PolarizationExtractorTests
{
    [Fact]
    public void Extract_ConcentratedInOneComponent_IsConnectionDominant()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var extractor = new PolarizationExtractor(mesh, algebra);

        // 3 edges * 3 dimG = 9 components
        // All energy in algebra component 0 of edge 0
        var v = new double[9];
        v[0] = 1.0; // edge 0, component 0
        var mode = TestHelpers.MakeMode("m-concentrated", 1.0, v);

        var pol = extractor.Extract(mode);

        Assert.Equal("connection-dominant", pol.DominantClass);
        Assert.True(pol.DominanceFraction > 0.8);
        Assert.Equal(1.0, pol.BlockEnergyFractions["connection"]);
    }

    [Fact]
    public void Extract_EvenlyDistributed_IsScalarLike()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var extractor = new PolarizationExtractor(mesh, algebra);

        // Equal energy across all 3 algebra components
        var v = new double[9];
        double val = 1.0 / System.Math.Sqrt(9);
        for (int i = 0; i < 9; i++) v[i] = val;
        var mode = TestHelpers.MakeMode("m-even", 1.0, v);

        var pol = extractor.Extract(mode);

        // Each component has ~1/3 of energy
        Assert.True(pol.DominanceFraction < 1.0 / 3 + 0.11);
    }

    [Fact]
    public void Extract_ZeroVector_HasEqualFractions()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var extractor = new PolarizationExtractor(mesh, algebra);

        var v = new double[9]; // all zeros
        var mode = TestHelpers.MakeMode("m-zero", 1.0, v);

        var pol = extractor.Extract(mode);

        // Default equal fractions
        foreach (var (key, frac) in pol.BlockEnergyFractions)
        {
            if (key.StartsWith("algebra-component-"))
                Assert.Equal(1.0 / 3, frac, 10);
        }
    }

    [Fact]
    public void Extract_IncludesModeIdAndBackground()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var extractor = new PolarizationExtractor(mesh, algebra);

        var mode = TestHelpers.MakeMode("m-prov", 1.0, new double[9], backgroundId: "bg-test");
        var pol = extractor.Extract(mode);

        Assert.Equal("m-prov", pol.ModeId);
        Assert.Equal("bg-test", pol.BackgroundId);
    }

    [Fact]
    public void Extract_FractionsSumToOne()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var extractor = new PolarizationExtractor(mesh, algebra);

        var rng = new Random(42);
        var mode = TestHelpers.MakeMode("m-rand", 1.0, 9, rng);

        var pol = extractor.Extract(mode);

        double componentSum = 0;
        foreach (var (key, frac) in pol.BlockEnergyFractions)
        {
            if (key.StartsWith("algebra-component-"))
                componentSum += frac;
        }
        Assert.Equal(1.0, componentSum, 10);
    }
}
