using Gu.Discretization;

namespace Gu.Geometry.Tests;

public class QuadratureRuleTests
{
    [Theory]
    [InlineData(1, 1.0)]
    [InlineData(2, 0.5)]
    [InlineData(3, 1.0 / 6.0)]
    [InlineData(4, 1.0 / 24.0)]
    [InlineData(5, 1.0 / 120.0)]
    public void ReferenceSimplexVolume_IsCorrect(int dim, double expectedVolume)
    {
        Assert.Equal(expectedVolume, QuadratureRuleFactory.ReferenceSimplexVolume(dim), 12);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void CentroidRule_WeightsSumToVolume(int dim)
    {
        var rule = QuadratureRuleFactory.Centroid(dim);
        double volume = QuadratureRuleFactory.ReferenceSimplexVolume(dim);
        double weightSum = rule.Weights.Sum();
        Assert.Equal(volume, weightSum, 12);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void CentroidRule_PointIsCentroid(int dim)
    {
        var rule = QuadratureRuleFactory.Centroid(dim);
        Assert.Single(rule.Points);
        var point = rule.Points[0];
        double expectedCoord = 1.0 / (dim + 1);
        Assert.All(point, c => Assert.Equal(expectedCoord, c, 12));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void CentroidRule_BarycentricCoordsSumToOne(int dim)
    {
        var rule = QuadratureRuleFactory.Centroid(dim);
        foreach (var point in rule.Points)
        {
            double sum = point.Sum();
            Assert.Equal(1.0, sum, 12);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void VertexBasedRule_WeightsSumToVolume(int dim)
    {
        var rule = QuadratureRuleFactory.VertexBased(dim);
        double volume = QuadratureRuleFactory.ReferenceSimplexVolume(dim);
        double weightSum = rule.Weights.Sum();
        Assert.Equal(volume, weightSum, 12);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void VertexBasedRule_BarycentricCoordsSumToOne(int dim)
    {
        var rule = QuadratureRuleFactory.VertexBased(dim);
        foreach (var point in rule.Points)
        {
            double sum = point.Sum();
            Assert.Equal(1.0, sum, 12);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void VertexBasedRule_AllCoordsNonNegative(int dim)
    {
        var rule = QuadratureRuleFactory.VertexBased(dim);
        foreach (var point in rule.Points)
        {
            Assert.All(point, c => Assert.True(c >= 0, $"Negative barycentric coord: {c}"));
        }
    }

    [Fact]
    public void VertexBasedRule_2D_HasThreePoints()
    {
        var rule = QuadratureRuleFactory.VertexBased(2);
        Assert.Equal(3, rule.PointCount);
        Assert.Equal(2, rule.Order);
    }

    [Fact]
    public void VertexBasedRule_3D_HasFourPoints()
    {
        var rule = QuadratureRuleFactory.VertexBased(3);
        Assert.Equal(4, rule.PointCount);
        Assert.Equal(2, rule.Order);
    }
}
