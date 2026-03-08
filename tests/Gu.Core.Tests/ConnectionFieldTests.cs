using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.Core.Tests;

public class ConnectionFieldTests
{
    private static SimplicialMesh CreateTwoTriangleMesh()
    {
        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[]
            {
                0, 0,   // v0
                1, 0,   // v1
                0, 1,   // v2
                1, 1,   // v3
            },
            vertexCount: 4,
            cellVertices: new[]
            {
                new[] { 0, 1, 2 },
                new[] { 1, 3, 2 },
            });
    }

    [Fact]
    public void Zero_AllCoefficientsAreZero()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = ConnectionField.Zero(mesh, algebra);

        Assert.All(omega.Coefficients, c => Assert.Equal(0.0, c));
    }

    [Fact]
    public void Constructor_SetsCorrectLength()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        Assert.Equal(mesh.EdgeCount * algebra.Dimension, omega.Coefficients.Length);
    }

    [Fact]
    public void Constructor_WithCoefficients_ValidatesLength()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        Assert.Throws<ArgumentException>(() =>
            new ConnectionField(mesh, algebra, new double[] { 1.0, 2.0 }));
    }

    [Fact]
    public void GetSetEdgeValue_RoundTrips()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        var value = new double[] { 1.0, 2.0, 3.0 };
        omega.SetEdgeValue(0, value);

        var retrieved = omega.GetEdgeValueArray(0);
        Assert.Equal(value, retrieved);
    }

    [Fact]
    public void GetEdgeValue_Span_ReturnsCorrectSlice()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        var value = new double[] { 4.0, 5.0, 6.0 };
        omega.SetEdgeValue(1, value);

        var span = omega.GetEdgeValue(1);
        Assert.Equal(3, span.Length);
        Assert.Equal(4.0, span[0]);
        Assert.Equal(5.0, span[1]);
        Assert.Equal(6.0, span[2]);
    }

    [Fact]
    public void SetEdgeValue_WrongLength_Throws()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        Assert.Throws<ArgumentException>(() =>
            omega.SetEdgeValue(0, new double[] { 1.0, 2.0 }));
    }

    [Fact]
    public void EdgeCount_MatchesMesh()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        Assert.Equal(mesh.EdgeCount, omega.EdgeCount);
    }

    [Fact]
    public void AlgebraDimension_MatchesAlgebra()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        Assert.Equal(3, omega.AlgebraDimension);
    }

    [Fact]
    public void ToFieldTensor_HasCorrectSignature()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        var ft = omega.ToFieldTensor();

        Assert.Equal("omega_h", ft.Label);
        Assert.Equal("Y_h", ft.Signature.AmbientSpaceId);
        Assert.Equal("connection-1form", ft.Signature.CarrierType);
        Assert.Equal("1", ft.Signature.Degree);
        Assert.Equal("float64", ft.Signature.NumericPrecision);
    }

    [Fact]
    public void ToFieldTensor_HasCorrectShape()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);

        var ft = omega.ToFieldTensor();

        Assert.Equal(new[] { mesh.EdgeCount, algebra.Dimension }, ft.Shape);
        Assert.Equal(mesh.EdgeCount * algebra.Dimension, ft.Coefficients.Length);
    }

    [Fact]
    public void ToFieldTensor_ClonesCoefficients()
    {
        var mesh = CreateTwoTriangleMesh();
        var algebra = LieAlgebraFactory.CreateSu2();
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new double[] { 1.0, 2.0, 3.0 });

        var ft = omega.ToFieldTensor();

        // Mutating original should not affect the FieldTensor
        omega.SetEdgeValue(0, new double[] { 0.0, 0.0, 0.0 });
        Assert.Equal(1.0, ft.Coefficients[0]);
    }

    [Fact]
    public void NullMesh_Throws()
    {
        var algebra = LieAlgebraFactory.CreateSu2();
        Assert.Throws<ArgumentNullException>(() => new ConnectionField(null!, algebra));
    }

    [Fact]
    public void NullAlgebra_Throws()
    {
        var mesh = CreateTwoTriangleMesh();
        Assert.Throws<ArgumentNullException>(() => new ConnectionField(mesh, null!));
    }
}
