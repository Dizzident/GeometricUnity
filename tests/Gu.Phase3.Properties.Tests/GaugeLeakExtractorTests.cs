using Gu.Math;

namespace Gu.Phase3.Properties.Tests;

public class GaugeLeakExtractorTests
{
    [Fact]
    public void ComputeLeakScore_ZeroVector_ReturnsZero()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var projector = TestHelpers.BuildGaugeProjector(mesh, algebra);
        var extractor = new GaugeLeakExtractor(projector);

        var zeroVec = new double[mesh.EdgeCount * algebra.Dimension];
        double leak = extractor.ComputeLeakScore(zeroVec);
        Assert.Equal(0.0, leak);
    }

    [Fact]
    public void ComputeLeakScore_IsInZeroOneRange()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var projector = TestHelpers.BuildGaugeProjector(mesh, algebra);
        var extractor = new GaugeLeakExtractor(projector);

        var rng = new Random(42);
        int dim = mesh.EdgeCount * algebra.Dimension;
        for (int t = 0; t < 10; t++)
        {
            var v = new double[dim];
            for (int i = 0; i < dim; i++) v[i] = rng.NextDouble() * 2 - 1;
            double leak = extractor.ComputeLeakScore(v);
            Assert.True(leak >= -1e-12, $"Gauge leak negative: {leak}");
            Assert.True(leak <= 1.0 + 1e-12, $"Gauge leak > 1: {leak}");
        }
    }

    [Fact]
    public void ComputeLeakScore_FromModeRecord()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var projector = TestHelpers.BuildGaugeProjector(mesh, algebra);
        var extractor = new GaugeLeakExtractor(projector);

        int dim = mesh.EdgeCount * algebra.Dimension;
        var v = new double[dim];
        v[0] = 1.0;
        var mode = TestHelpers.MakeMode("m-1", 1.0, v);

        double leakFromVec = extractor.ComputeLeakScore(v);
        double leakFromMode = extractor.ComputeLeakScore(mode);
        Assert.Equal(leakFromVec, leakFromMode, 12);
    }

    [Fact]
    public void Constructor_NullProjector_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GaugeLeakExtractor(null!));
    }
}
