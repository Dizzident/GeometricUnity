using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.Core.Tests;

public class BiConnectionBuilderTests
{
    private static SimplicialMesh CreateTwoTriangleMesh()
    {
        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[]
            {
                0, 0,
                1, 0,
                0, 1,
                1, 1,
            },
            vertexCount: 4,
            cellVertices: new[]
            {
                new[] { 0, 1, 2 },
                new[] { 1, 3, 2 },
            });
    }

    [Fact]
    public void FlatA0_AEqualsOmega_BEqualsNegOmega()
    {
        // With A0 = 0: A = 0 + omega = omega, B = 0 - omega = -omega
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var builder = BiConnectionBuilder.WithFlatA0(mesh, algebra, "test-branch");

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new double[] { 1.0, 2.0, 3.0 });
        omega.SetEdgeValue(1, new double[] { 4.0, 5.0, 6.0 });

        var (a, b) = builder.Build(omega);

        // A = omega
        Assert.Equal(1.0, a.GetEdgeValueArray(0)[0], 12);
        Assert.Equal(2.0, a.GetEdgeValueArray(0)[1], 12);
        Assert.Equal(3.0, a.GetEdgeValueArray(0)[2], 12);

        // B = -omega
        Assert.Equal(-1.0, b.GetEdgeValueArray(0)[0], 12);
        Assert.Equal(-2.0, b.GetEdgeValueArray(0)[1], 12);
        Assert.Equal(-3.0, b.GetEdgeValueArray(0)[2], 12);
    }

    [Fact]
    public void NonFlatA0_APlusB_EqualsTwoA0()
    {
        // A = A0 + omega, B = A0 - omega => A + B = 2*A0
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        var a0Coeffs = new double[mesh.EdgeCount * algebra.Dimension];
        for (int i = 0; i < a0Coeffs.Length; i++)
            a0Coeffs[i] = (i + 1) * 0.5;
        var a0 = new ConnectionField(mesh, algebra, a0Coeffs);
        var builder = new BiConnectionBuilder(a0, "test-branch");

        var omega = new ConnectionField(mesh, algebra);
        for (int i = 0; i < omega.Coefficients.Length; i++)
            omega.Coefficients[i] = (i + 1) * 0.3;

        var (aConn, bConn) = builder.Build(omega);

        // A + B = 2*A0
        for (int i = 0; i < a0Coeffs.Length; i++)
        {
            Assert.Equal(2 * a0Coeffs[i],
                aConn.Coefficients[i] + bConn.Coefficients[i], 12);
        }
    }

    [Fact]
    public void NonFlatA0_AMinusB_EqualsTwoOmega()
    {
        // A - B = (A0 + omega) - (A0 - omega) = 2*omega
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        var a0Coeffs = new double[mesh.EdgeCount * algebra.Dimension];
        for (int i = 0; i < a0Coeffs.Length; i++)
            a0Coeffs[i] = (i + 1) * 0.5;
        var a0 = new ConnectionField(mesh, algebra, a0Coeffs);
        var builder = new BiConnectionBuilder(a0, "test-branch");

        var omegaCoeffs = new double[mesh.EdgeCount * algebra.Dimension];
        for (int i = 0; i < omegaCoeffs.Length; i++)
            omegaCoeffs[i] = (i + 1) * 0.3;
        var omega = new ConnectionField(mesh, algebra, omegaCoeffs);

        var (aConn, bConn) = builder.Build(omega);

        // A - B = 2*omega
        for (int i = 0; i < omegaCoeffs.Length; i++)
        {
            Assert.Equal(2 * omegaCoeffs[i],
                aConn.Coefficients[i] - bConn.Coefficients[i], 12);
        }
    }

    [Fact]
    public void WithFlatA0_A0IsZero()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var builder = BiConnectionBuilder.WithFlatA0(mesh, algebra, "test-branch");

        Assert.All(builder.A0.Coefficients, c => Assert.Equal(0.0, c));
    }

    [Fact]
    public void BranchId_IsPreserved()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var builder = BiConnectionBuilder.WithFlatA0(mesh, algebra, "my-branch-42");

        Assert.Equal("my-branch-42", builder.BranchId);
    }

    [Fact]
    public void DifferentMesh_Throws()
    {
        var mesh1 = CreateTwoTriangleMesh();
        var mesh2 = CreateTwoTriangleMesh(); // different instance
        var algebra = LieAlgebraFactory.CreateSu2();

        var a0 = ConnectionField.Zero(mesh1, algebra);
        var builder = new BiConnectionBuilder(a0, "test");

        var omega = new ConnectionField(mesh2, algebra);

        Assert.Throws<ArgumentException>(() => builder.Build(omega));
    }

    [Fact]
    public void DifferentAlgebra_Throws()
    {
        var mesh = CreateTwoTriangleMesh();
        var su2 = LieAlgebraFactory.CreateSu2();
        var u1 = LieAlgebraFactory.CreateAbelian(1);

        var a0 = ConnectionField.Zero(mesh, su2);
        var builder = new BiConnectionBuilder(a0, "test");

        var omega = new ConnectionField(mesh, u1);

        Assert.Throws<ArgumentException>(() => builder.Build(omega));
    }

    [Fact]
    public void NullA0_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new BiConnectionBuilder(null!, "test"));
    }

    [Fact]
    public void NullBranchId_Throws()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var a0 = ConnectionField.Zero(mesh, algebra);

        Assert.Throws<ArgumentNullException>(() =>
            new BiConnectionBuilder(a0, null!));
    }
}
