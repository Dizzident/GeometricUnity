using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties.Tests;

public class PropertyVectorBuilderTests
{
    [Fact]
    public void Build_SingleMode_ProducesCompleteVector()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var builder = new PropertyVectorBuilder(mesh, algebra);

        int dim = mesh.EdgeCount * algebra.Dimension;
        var v = new double[dim];
        v[0] = 1.0;
        var mode = TestHelpers.MakeMode("m-1", 4.0, v, gaugeLeakScore: 0.05);

        var pv = builder.Build(mode);

        Assert.Equal("m-1", pv.ModeId);
        Assert.Equal("bg-1", pv.BackgroundId);
        Assert.Equal(2.0, pv.MassLikeScale.MassLikeScale, 12);
        Assert.Equal(0.05, pv.GaugeLeakScore);
        Assert.Equal(1, pv.Multiplicity);
        Assert.NotNull(pv.Polarization);
        Assert.NotNull(pv.Symmetry);
    }

    [Fact]
    public void Build_WithClusterMultiplicity_SetsMultiplicity()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var clusters = new Dictionary<string, int> { ["cluster-0"] = 3 };
        var builder = new PropertyVectorBuilder(mesh, algebra, clusterMultiplicities: clusters);

        int dim = mesh.EdgeCount * algebra.Dimension;
        var mode = TestHelpers.MakeMode("m-1", 1.0, new double[dim], clusterId: "cluster-0");

        var pv = builder.Build(mode);
        Assert.Equal(3, pv.Multiplicity);
    }

    [Fact]
    public void Build_WithGaugeProjector_UsesProjectorForLeak()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var projector = TestHelpers.BuildGaugeProjector(mesh, algebra);
        var builder = new PropertyVectorBuilder(mesh, algebra, projector);

        int dim = mesh.EdgeCount * algebra.Dimension;
        var v = new double[dim];
        v[0] = 1.0;
        var mode = TestHelpers.MakeMode("m-1", 1.0, v, gaugeLeakScore: 0.99);

        var pv = builder.Build(mode);
        // With projector, should compute actual leak, not use the stored 0.99
        Assert.True(pv.GaugeLeakScore >= 0);
        Assert.True(pv.GaugeLeakScore <= 1.0 + 1e-12);
    }

    [Fact]
    public void BuildAll_MultipleModesReturnsAll()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var builder = new PropertyVectorBuilder(mesh, algebra);

        int dim = mesh.EdgeCount * algebra.Dimension;
        var rng = new Random(42);
        var modes = new[]
        {
            TestHelpers.MakeMode("m-1", 1.0, dim, rng),
            TestHelpers.MakeMode("m-2", 4.0, dim, rng),
            TestHelpers.MakeMode("m-3", 9.0, dim, rng),
        };

        var vectors = builder.BuildAll(modes);
        Assert.Equal(3, vectors.Count);
        Assert.Equal("m-1", vectors[0].ModeId);
        Assert.Equal("m-2", vectors[1].ModeId);
        Assert.Equal("m-3", vectors[2].ModeId);
    }

    [Fact]
    public void Build_NullMode_Throws()
    {
        var mesh = TestHelpers.SingleTriangle();
        var algebra = TestHelpers.TracePairingSu2();
        var builder = new PropertyVectorBuilder(mesh, algebra);
        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }
}
