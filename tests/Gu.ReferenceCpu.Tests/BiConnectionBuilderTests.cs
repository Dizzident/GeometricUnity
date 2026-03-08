using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

public class BiConnectionBuilderTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    /// <summary>
    /// With flat A0 = 0, bi-connection gives A = omega, B = -omega.
    /// </summary>
    [Fact]
    public void FlatA0_BiConnectionIsOmegaAndNegOmega()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var builder = BiConnectionBuilder.WithFlatA0(mesh, algebra, "test-branch");

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 2.0, 3.0 });
        omega.SetEdgeValue(1, new[] { 4.0, 5.0, 6.0 });
        omega.SetEdgeValue(2, new[] { 7.0, 8.0, 9.0 });

        var (aConn, bConn) = builder.Build(omega);

        // A = A0 + omega = 0 + omega = omega
        for (int i = 0; i < omega.Coefficients.Length; i++)
        {
            Assert.Equal(omega.Coefficients[i], aConn.Coefficients[i], 12);
        }

        // B = A0 - omega = 0 - omega = -omega
        for (int i = 0; i < omega.Coefficients.Length; i++)
        {
            Assert.Equal(-omega.Coefficients[i], bConn.Coefficients[i], 12);
        }
    }

    /// <summary>
    /// With non-trivial A0, A = A0 + omega and B = A0 - omega.
    /// </summary>
    [Fact]
    public void NonTrivialA0_BiConnectionIsCorrect()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();

        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 0.1, 0.2, 0.3 });
        a0.SetEdgeValue(1, new[] { 0.4, 0.5, 0.6 });
        a0.SetEdgeValue(2, new[] { 0.7, 0.8, 0.9 });

        var builder = new BiConnectionBuilder(a0, "test-branch");

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });
        omega.SetEdgeValue(2, new[] { 0.0, 0.0, 1.0 });

        var (aConn, bConn) = builder.Build(omega);

        // Verify A = A0 + omega
        Assert.Equal(0.1 + 1.0, aConn.GetEdgeValueArray(0)[0], 12);
        Assert.Equal(0.2 + 0.0, aConn.GetEdgeValueArray(0)[1], 12);
        Assert.Equal(0.4 + 0.0, aConn.GetEdgeValueArray(1)[0], 12);
        Assert.Equal(0.5 + 1.0, aConn.GetEdgeValueArray(1)[1], 12);

        // Verify B = A0 - omega
        Assert.Equal(0.1 - 1.0, bConn.GetEdgeValueArray(0)[0], 12);
        Assert.Equal(0.2 - 0.0, bConn.GetEdgeValueArray(0)[1], 12);
    }

    /// <summary>
    /// When omega = 0, both A and B equal A0.
    /// </summary>
    [Fact]
    public void ZeroOmega_BothConnectionsEqualA0()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();

        var a0 = new ConnectionField(mesh, algebra);
        a0.SetEdgeValue(0, new[] { 1.0, 2.0, 3.0 });

        var builder = new BiConnectionBuilder(a0, "test-branch");
        var omega = ConnectionField.Zero(mesh, algebra);

        var (aConn, bConn) = builder.Build(omega);

        for (int i = 0; i < a0.Coefficients.Length; i++)
        {
            Assert.Equal(a0.Coefficients[i], aConn.Coefficients[i], 12);
            Assert.Equal(a0.Coefficients[i], bConn.Coefficients[i], 12);
        }
    }

    /// <summary>
    /// Curvature of both bi-connections can be computed.
    /// </summary>
    [Fact]
    public void BiConnections_CurvatureComputable()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var builder = BiConnectionBuilder.WithFlatA0(mesh, algebra, "test-branch");

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });

        var (aConn, bConn) = builder.Build(omega);

        var fA = CurvatureAssembler.Assemble(aConn);
        var fB = CurvatureAssembler.Assemble(bConn);

        // Both should have the right number of face values
        Assert.Equal(mesh.FaceCount, fA.FaceCount);
        Assert.Equal(mesh.FaceCount, fB.FaceCount);
        Assert.Equal(algebra.Dimension, fA.AlgebraDimension);
    }

    /// <summary>
    /// A0 is stored explicitly and accessible.
    /// </summary>
    [Fact]
    public void A0_ExplicitlyAccessible()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var builder = BiConnectionBuilder.WithFlatA0(mesh, algebra, "test-branch");

        Assert.NotNull(builder.A0);
        Assert.Equal("test-branch", builder.BranchId);
        Assert.All(builder.A0.Coefficients, c => Assert.Equal(0.0, c, 12));
    }
}
