using Gu.Discretization;

namespace Gu.Discretization.Tests;

public class QuadratureRuleEdgeCaseTests
{
    [Fact]
    public void Centroid_InvalidDimension_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => QuadratureRuleFactory.Centroid(0));
    }

    [Fact]
    public void VertexBased_InvalidDimension_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => QuadratureRuleFactory.VertexBased(0));
    }

    [Fact]
    public void ReferenceSimplexVolume_HighDimension_IsCorrect()
    {
        // 6! = 720 => vol = 1/720
        Assert.Equal(1.0 / 720.0, QuadratureRuleFactory.ReferenceSimplexVolume(6), 12);
    }

    [Fact]
    public void ReferenceSimplexVolume_Dimension1_IsOne()
    {
        Assert.Equal(1.0, QuadratureRuleFactory.ReferenceSimplexVolume(1), 12);
    }

    [Fact]
    public void Centroid_RuleId_ContainsDimension()
    {
        var rule = QuadratureRuleFactory.Centroid(3);
        Assert.Equal("simplex-3d-centroid", rule.RuleId);
    }

    [Fact]
    public void Centroid_HasSinglePoint()
    {
        for (int dim = 1; dim <= 5; dim++)
        {
            var rule = QuadratureRuleFactory.Centroid(dim);
            Assert.Equal(1, rule.PointCount);
        }
    }

    [Fact]
    public void Centroid_PointHasCorrectBarycentricsCount()
    {
        for (int dim = 1; dim <= 4; dim++)
        {
            var rule = QuadratureRuleFactory.Centroid(dim);
            Assert.Equal(dim + 1, rule.Points[0].Length);
        }
    }

    [Fact]
    public void VertexBased_1D_IsGauss2()
    {
        var rule = QuadratureRuleFactory.VertexBased(1);
        Assert.Equal("simplex-1d-gauss2", rule.RuleId);
        Assert.Equal(2, rule.PointCount);
        Assert.Equal(2, rule.Order);
    }

    [Fact]
    public void VertexBased_1D_WeightsAreEqual()
    {
        var rule = QuadratureRuleFactory.VertexBased(1);
        Assert.Equal(rule.Weights[0], rule.Weights[1], 12);
    }

    [Fact]
    public void VertexBased_1D_BarycentricsInUnitInterval()
    {
        var rule = QuadratureRuleFactory.VertexBased(1);
        foreach (var pt in rule.Points)
        {
            Assert.Equal(2, pt.Length);
            Assert.True(pt[0] >= 0 && pt[0] <= 1);
            Assert.True(pt[1] >= 0 && pt[1] <= 1);
            Assert.Equal(1.0, pt.Sum(), 12);
        }
    }

    [Fact]
    public void VertexBased_HighDimension_HasCorrectPointCount()
    {
        for (int dim = 1; dim <= 5; dim++)
        {
            var rule = QuadratureRuleFactory.VertexBased(dim);
            Assert.Equal(dim + 1, rule.PointCount);
        }
    }

    [Fact]
    public void VertexBased_AllWeightsPositive()
    {
        for (int dim = 1; dim <= 4; dim++)
        {
            var rule = QuadratureRuleFactory.VertexBased(dim);
            Assert.All(rule.Weights, w => Assert.True(w > 0, $"Negative weight in dim={dim}"));
        }
    }

    [Fact]
    public void Centroid_IntegratesConstantExactly()
    {
        // Integral of f=1 over reference simplex = volume
        for (int dim = 1; dim <= 4; dim++)
        {
            var rule = QuadratureRuleFactory.Centroid(dim);
            double integral = 0;
            for (int q = 0; q < rule.PointCount; q++)
                integral += rule.Weights[q] * 1.0; // f=1

            double volume = QuadratureRuleFactory.ReferenceSimplexVolume(dim);
            Assert.Equal(volume, integral, 12);
        }
    }

    [Fact]
    public void VertexBased_IntegratesConstantExactly()
    {
        for (int dim = 1; dim <= 4; dim++)
        {
            var rule = QuadratureRuleFactory.VertexBased(dim);
            double integral = 0;
            for (int q = 0; q < rule.PointCount; q++)
                integral += rule.Weights[q] * 1.0;

            double volume = QuadratureRuleFactory.ReferenceSimplexVolume(dim);
            Assert.Equal(volume, integral, 12);
        }
    }

    [Fact]
    public void PointCount_MatchesWeightsLength()
    {
        var rule = QuadratureRuleFactory.Centroid(3);
        Assert.Equal(rule.Weights.Length, rule.PointCount);
    }
}

public class BasisFamilyEdgeCaseTests
{
    [Fact]
    public void P1_InvalidDimension_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => BasisFamilyFactory.P1(0, QuadratureRuleFactory.Centroid(1)));
    }

    [Fact]
    public void P1_DimensionMismatchWithQuadrature_Throws()
    {
        var quad3d = QuadratureRuleFactory.Centroid(3);
        Assert.Throws<ArgumentException>(
            () => BasisFamilyFactory.P1(2, quad3d));
    }

    [Fact]
    public void P1_SimplicialDimension_IsStored()
    {
        for (int dim = 1; dim <= 4; dim++)
        {
            var quad = QuadratureRuleFactory.Centroid(dim);
            var basis = BasisFamilyFactory.P1(dim, quad);
            Assert.Equal(dim, basis.SimplicialDimension);
        }
    }

    [Fact]
    public void P1_WithVertexBasedQuad_PartitionOfUnity()
    {
        // Higher-order quadrature: partition of unity should still hold
        for (int dim = 1; dim <= 3; dim++)
        {
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
    }

    [Fact]
    public void P1_ValuesAtVertex_AreKronecker()
    {
        // At vertex-biased quadrature points, the basis function
        // corresponding to that vertex should have the largest value
        int dim = 2;
        var quad = QuadratureRuleFactory.VertexBased(dim);
        var basis = BasisFamilyFactory.P1(dim, quad);

        for (int q = 0; q < quad.PointCount; q++)
        {
            // Find which basis function has the max value at this quad point
            int maxBasis = 0;
            double maxVal = basis.ValuesAtQuadPoints[0][q];
            for (int b = 1; b < basis.FunctionCount; b++)
            {
                if (basis.ValuesAtQuadPoints[b][q] > maxVal)
                {
                    maxVal = basis.ValuesAtQuadPoints[b][q];
                    maxBasis = b;
                }
            }

            // The quad point q is biased toward vertex q, so basis q should dominate
            Assert.Equal(q, maxBasis);
        }
    }

    [Fact]
    public void P1_AllValuesNonNegative()
    {
        // Barycentric coordinates are non-negative inside the simplex
        for (int dim = 1; dim <= 3; dim++)
        {
            var quad = QuadratureRuleFactory.VertexBased(dim);
            var basis = BasisFamilyFactory.P1(dim, quad);

            for (int b = 0; b < basis.FunctionCount; b++)
                for (int q = 0; q < quad.PointCount; q++)
                    Assert.True(basis.ValuesAtQuadPoints[b][q] >= 0,
                        $"Negative value at dim={dim}, basis={b}, quad={q}");
        }
    }
}

public class ReferenceElementEdgeCaseTests
{
    [Fact]
    public void CreateP1_InvalidDimension_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ReferenceElementFactory.CreateP1(0));
    }

    [Fact]
    public void CreateP1_DefaultQuadrature_IsCentroid()
    {
        var elem = ReferenceElementFactory.CreateP1(2);
        Assert.Equal(1, elem.Quadrature.PointCount);
        Assert.Equal(1, elem.Quadrature.Order);
    }

    [Fact]
    public void CreateP1_QuadratureOrder2_UsesVertexBased()
    {
        var elem = ReferenceElementFactory.CreateP1(2, quadratureOrder: 2);
        Assert.Equal(3, elem.Quadrature.PointCount);
        Assert.Equal(2, elem.Quadrature.Order);
    }

    [Fact]
    public void CreateP1_ReferenceVertices_Correct()
    {
        var elem = ReferenceElementFactory.CreateP1(2);

        // Vertex 0 at origin
        Assert.Equal(0.0, elem.ReferenceVertices[0][0]);
        Assert.Equal(0.0, elem.ReferenceVertices[0][1]);

        // Vertex 1 at (1,0)
        Assert.Equal(1.0, elem.ReferenceVertices[1][0]);
        Assert.Equal(0.0, elem.ReferenceVertices[1][1]);

        // Vertex 2 at (0,1)
        Assert.Equal(0.0, elem.ReferenceVertices[2][0]);
        Assert.Equal(1.0, elem.ReferenceVertices[2][1]);
    }

    [Fact]
    public void CreateP1_3D_HasFourVertices()
    {
        var elem = ReferenceElementFactory.CreateP1(3);
        Assert.Equal(4, elem.ReferenceVertices.Length);
        Assert.Equal(3, elem.SimplicialDimension);
    }

    [Fact]
    public void ReferenceVolume_MatchesFactory()
    {
        for (int dim = 1; dim <= 4; dim++)
        {
            var elem = ReferenceElementFactory.CreateP1(dim);
            double expected = QuadratureRuleFactory.ReferenceSimplexVolume(dim);
            Assert.Equal(expected, elem.ReferenceVolume, 12);
        }
    }

    [Fact]
    public void CreateP1_BasisDimension_Matches()
    {
        for (int dim = 1; dim <= 3; dim++)
        {
            var elem = ReferenceElementFactory.CreateP1(dim);
            Assert.Equal(dim + 1, elem.Basis.FunctionCount);
            Assert.Equal(dim, elem.Basis.SimplicialDimension);
        }
    }

    [Fact]
    public void CreateP1_1D_Vertex0AtOrigin()
    {
        var elem = ReferenceElementFactory.CreateP1(1);
        Assert.Single(elem.ReferenceVertices[0]);
        Assert.Equal(0.0, elem.ReferenceVertices[0][0]);
    }

    [Fact]
    public void CreateP1_1D_Vertex1AtOne()
    {
        var elem = ReferenceElementFactory.CreateP1(1);
        Assert.Equal(1.0, elem.ReferenceVertices[1][0]);
    }
}
