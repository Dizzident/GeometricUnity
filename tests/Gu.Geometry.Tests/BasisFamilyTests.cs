using Gu.Discretization;

namespace Gu.Geometry.Tests;

public class BasisFamilyTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void P1_FunctionCount_IsNPlusOne(int dim)
    {
        var quad = QuadratureRuleFactory.Centroid(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);
        Assert.Equal(dim + 1, basis.FunctionCount);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void P1_Order_IsOne(int dim)
    {
        var quad = QuadratureRuleFactory.Centroid(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);
        Assert.Equal(1, basis.Order);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void P1_FamilyId_ContainsDimension(int dim)
    {
        var quad = QuadratureRuleFactory.Centroid(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);
        Assert.Equal($"P1-simplex-{dim}d", basis.FamilyId);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void P1_ValuesAtCentroid_AreEqual(int dim)
    {
        // At the centroid, all barycentric coordinates are equal = 1/(n+1).
        // P1 basis function i = lambda_i, so all values should be 1/(n+1).
        var quad = QuadratureRuleFactory.Centroid(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);

        double expected = 1.0 / (dim + 1);
        for (int b = 0; b < basis.FunctionCount; b++)
        {
            Assert.Equal(expected, basis.ValuesAtQuadPoints[b][0], 12);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void P1_PartitionOfUnity_AtAllQuadPoints(int dim)
    {
        // Sum of all P1 basis functions at any point = sum of barycentric coords = 1.
        var quad = QuadratureRuleFactory.VertexBased(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);

        for (int q = 0; q < quad.PointCount; q++)
        {
            double sum = 0;
            for (int b = 0; b < basis.FunctionCount; b++)
                sum += basis.ValuesAtQuadPoints[b][q];
            Assert.Equal(1.0, sum, 12);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void P1_GradientIsDelta_InBarycentricCoords(int dim)
    {
        // d(lambda_b)/d(lambda_j) = delta_{b,j}
        var quad = QuadratureRuleFactory.Centroid(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);
        int nBary = dim + 1;

        for (int b = 0; b < basis.FunctionCount; b++)
        {
            for (int q = 0; q < quad.PointCount; q++)
            {
                Assert.Equal(nBary, basis.GradientsAtQuadPoints[b][q].Length);
                for (int j = 0; j < nBary; j++)
                {
                    double expected = (b == j) ? 1.0 : 0.0;
                    Assert.Equal(expected, basis.GradientsAtQuadPoints[b][q][j], 12);
                }
            }
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void P1_GradientIsConstant_AcrossQuadPoints(int dim)
    {
        // P1 gradients in barycentric coords are constant (delta_{b,j}).
        // Verify they're the same at every quadrature point.
        var quad = QuadratureRuleFactory.VertexBased(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);

        for (int b = 0; b < basis.FunctionCount; b++)
        {
            for (int q = 1; q < quad.PointCount; q++)
            {
                for (int j = 0; j < dim + 1; j++)
                {
                    Assert.Equal(
                        basis.GradientsAtQuadPoints[b][0][j],
                        basis.GradientsAtQuadPoints[b][q][j], 12);
                }
            }
        }
    }

    [Fact]
    public void P1_DimensionMismatch_Throws()
    {
        var quad2d = QuadratureRuleFactory.Centroid(2);
        Assert.Throws<ArgumentException>(() => BasisFamilyFactory.P1(3, quad2d));
    }

    [Fact]
    public void P1_InvalidDimension_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            BasisFamilyFactory.P1(0, QuadratureRuleFactory.Centroid(1)));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void P1_ValuesArrayShape_IsCorrect(int dim)
    {
        var quad = QuadratureRuleFactory.VertexBased(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);

        // ValuesAtQuadPoints[basisIdx][quadPointIdx]
        Assert.Equal(basis.FunctionCount, basis.ValuesAtQuadPoints.Length);
        for (int b = 0; b < basis.FunctionCount; b++)
        {
            Assert.Equal(quad.PointCount, basis.ValuesAtQuadPoints[b].Length);
        }
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void P1_GradientsArrayShape_IsCorrect(int dim)
    {
        var quad = QuadratureRuleFactory.VertexBased(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);

        // GradientsAtQuadPoints[basisIdx][quadPointIdx][barycentricComponentIdx]
        Assert.Equal(basis.FunctionCount, basis.GradientsAtQuadPoints.Length);
        for (int b = 0; b < basis.FunctionCount; b++)
        {
            Assert.Equal(quad.PointCount, basis.GradientsAtQuadPoints[b].Length);
            for (int q = 0; q < quad.PointCount; q++)
            {
                Assert.Equal(dim + 1, basis.GradientsAtQuadPoints[b][q].Length);
            }
        }
    }
}
