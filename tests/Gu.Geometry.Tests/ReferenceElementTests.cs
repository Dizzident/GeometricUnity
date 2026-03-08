using Gu.Discretization;

namespace Gu.Geometry.Tests;

public class ReferenceElementTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void CreateP1_SimplicialDimension_IsCorrect(int dim)
    {
        var elem = ReferenceElementFactory.CreateP1(dim);
        Assert.Equal(dim, elem.SimplicialDimension);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateP1_ReferenceVertices_HasNPlusOneVertices(int dim)
    {
        var elem = ReferenceElementFactory.CreateP1(dim);
        Assert.Equal(dim + 1, elem.ReferenceVertices.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateP1_Vertex0_IsAtOrigin(int dim)
    {
        var elem = ReferenceElementFactory.CreateP1(dim);
        Assert.All(elem.ReferenceVertices[0], c => Assert.Equal(0.0, c, 12));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateP1_VertexI_IsUnitVector(int dim)
    {
        var elem = ReferenceElementFactory.CreateP1(dim);
        for (int i = 1; i <= dim; i++)
        {
            var vertex = elem.ReferenceVertices[i];
            Assert.Equal(dim, vertex.Length);
            for (int j = 0; j < dim; j++)
            {
                double expected = (j == i - 1) ? 1.0 : 0.0;
                Assert.Equal(expected, vertex[j], 12);
            }
        }
    }

    [Theory]
    [InlineData(1, 1.0)]
    [InlineData(2, 0.5)]
    [InlineData(3, 1.0 / 6.0)]
    [InlineData(4, 1.0 / 24.0)]
    public void CreateP1_ReferenceVolume_IsCorrect(int dim, double expectedVolume)
    {
        var elem = ReferenceElementFactory.CreateP1(dim);
        Assert.Equal(expectedVolume, elem.ReferenceVolume, 12);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateP1_DefaultQuadrature_IsCentroid(int dim)
    {
        // quadratureOrder <= 1 uses centroid rule
        var elem = ReferenceElementFactory.CreateP1(dim);
        Assert.Equal(1, elem.Quadrature.Order);
        Assert.Equal(1, elem.Quadrature.PointCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateP1_Order2Quadrature_IsVertexBased(int dim)
    {
        var elem = ReferenceElementFactory.CreateP1(dim, quadratureOrder: 2);
        Assert.Equal(2, elem.Quadrature.Order);
        Assert.Equal(dim + 1, elem.Quadrature.PointCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateP1_Basis_IsP1(int dim)
    {
        var elem = ReferenceElementFactory.CreateP1(dim);
        Assert.Equal(1, elem.Basis.Order);
        Assert.Equal(dim + 1, elem.Basis.FunctionCount);
        Assert.Equal($"P1-simplex-{dim}d", elem.Basis.FamilyId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateP1_QuadratureDimension_MatchesElement(int dim)
    {
        var elem = ReferenceElementFactory.CreateP1(dim);
        Assert.Equal(dim, elem.Quadrature.SimplicialDimension);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateP1_BasisDimension_MatchesElement(int dim)
    {
        var elem = ReferenceElementFactory.CreateP1(dim);
        Assert.Equal(dim, elem.Basis.SimplicialDimension);
    }

    [Fact]
    public void CreateP1_InvalidDimension_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ReferenceElementFactory.CreateP1(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => ReferenceElementFactory.CreateP1(-1));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void CreateP1_ReferenceVertices_EachHasCorrectLength(int dim)
    {
        var elem = ReferenceElementFactory.CreateP1(dim);
        Assert.All(elem.ReferenceVertices, v => Assert.Equal(dim, v.Length));
    }

    [Fact]
    public void CreateP1_2D_IsStandardTriangle()
    {
        var elem = ReferenceElementFactory.CreateP1(2);

        // Standard reference triangle: (0,0), (1,0), (0,1)
        Assert.Equal(new double[] { 0, 0 }, elem.ReferenceVertices[0]);
        Assert.Equal(new double[] { 1, 0 }, elem.ReferenceVertices[1]);
        Assert.Equal(new double[] { 0, 1 }, elem.ReferenceVertices[2]);
    }

    [Fact]
    public void CreateP1_3D_IsStandardTetrahedron()
    {
        var elem = ReferenceElementFactory.CreateP1(3);

        // Standard reference tetrahedron: (0,0,0), (1,0,0), (0,1,0), (0,0,1)
        Assert.Equal(new double[] { 0, 0, 0 }, elem.ReferenceVertices[0]);
        Assert.Equal(new double[] { 1, 0, 0 }, elem.ReferenceVertices[1]);
        Assert.Equal(new double[] { 0, 1, 0 }, elem.ReferenceVertices[2]);
        Assert.Equal(new double[] { 0, 0, 1 }, elem.ReferenceVertices[3]);
    }
}
