using Gu.Phase5.Convergence;

namespace Gu.Phase5.Convergence.Tests;

public sealed class RichardsonExtrapolatorTests
{
    [Fact]
    public void Extrapolate_ExactSecondOrder_RecoversTrueLimit()
    {
        // Q(h) = 3.0 + 2.0 * h^2
        double q0True = 3.0;
        double[] hs = [1.0, 0.5, 0.25, 0.125];
        double[] qs = hs.Select(h => q0True + 2.0 * h * h).ToArray();

        var fit = RichardsonExtrapolator.Extrapolate("q", hs, qs);

        Assert.Equal("q", fit.QuantityId);
        Assert.Equal(q0True, fit.EstimatedLimit, precision: 4);
        Assert.True(fit.EstimatedOrder > 1.5, $"Expected order ~2, got {fit.EstimatedOrder}");
        Assert.True(fit.Residual < 1e-6);
    }

    [Fact]
    public void Extrapolate_ExactFirstOrder_RecoversTrueLimit()
    {
        // Q(h) = 5.0 + 1.5 * h
        double q0True = 5.0;
        double[] hs = [0.8, 0.4, 0.2];
        double[] qs = hs.Select(h => q0True + 1.5 * h).ToArray();

        var fit = RichardsonExtrapolator.Extrapolate("q", hs, qs);

        Assert.Equal(q0True, fit.EstimatedLimit, precision: 3);
        Assert.True(fit.EstimatedOrder > 0.5, $"Expected order ~1, got {fit.EstimatedOrder}");
    }

    [Fact]
    public void Extrapolate_TwoPoints_ReturnsResult()
    {
        double[] hs = [1.0, 0.5];
        double[] qs = [4.0, 3.5];

        var fit = RichardsonExtrapolator.Extrapolate("q", hs, qs);

        Assert.NotNull(fit);
        Assert.Equal(2, fit.MeshParameters.Length);
    }

    [Fact]
    public void Extrapolate_ThrowsOnMismatchedArrays()
    {
        Assert.Throws<ArgumentException>(() =>
            RichardsonExtrapolator.Extrapolate("q", [1.0, 0.5], [1.0]));
    }

    [Fact]
    public void Extrapolate_ThrowsOnTooFewPoints()
    {
        Assert.Throws<ArgumentException>(() =>
            RichardsonExtrapolator.Extrapolate("q", [1.0], [2.0]));
    }

    [Fact]
    public void Extrapolate_PreservesInputArrays()
    {
        double[] hs = [1.0, 0.5, 0.25];
        double[] qs = [3.0, 2.5, 2.25];

        var fit = RichardsonExtrapolator.Extrapolate("q", hs, qs);

        Assert.Equal(3, fit.MeshParameters.Length);
        Assert.Equal(3, fit.Values.Length);
    }
}
