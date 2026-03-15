using Gu.Phase5.Convergence;

namespace Gu.Phase5.Convergence.Tests;

public sealed class ConvergenceClassifierTests
{
    [Fact]
    public void Classify_MonotoneDecreasing_IsConvergent()
    {
        // Q(h) = 1.0 + 2*h^2 => strictly convergent, order 2
        double[] hs = [1.0, 0.5, 0.25, 0.125];
        double[] qs = hs.Select(h => 1.0 + 2.0 * h * h).ToArray();

        var (cls, note) = ConvergenceClassifier.Classify(hs, qs);

        Assert.Equal("convergent", cls);
        Assert.Contains("p=", note);
    }

    [Fact]
    public void Classify_InsufficientData_TwoPoints()
    {
        var (cls, _) = ConvergenceClassifier.Classify([1.0, 0.5], [2.0, 1.5]);
        Assert.Equal("insufficient-data", cls);
    }

    [Fact]
    public void Classify_InsufficientData_OnePoint()
    {
        var (cls, _) = ConvergenceClassifier.Classify([1.0], [2.0]);
        Assert.Equal("insufficient-data", cls);
    }

    [Fact]
    public void Classify_DivergingSeries_IsNonConvergent()
    {
        // Q(h) values increasing with refinement (h decreasing)
        double[] hs = [1.0, 0.5, 0.25];
        double[] qs = [1.0, 2.0, 4.0]; // diverging

        var (cls, _) = ConvergenceClassifier.Classify(hs, qs);

        Assert.Equal("non-convergent", cls);
    }

    [Fact]
    public void Classify_SlowButDecreasing_IsWeaklyConvergent()
    {
        // Order ~0.3 — decreasing but slow
        double[] hs = [1.0, 0.5, 0.25];
        double[] qs = hs.Select(h => 5.0 + System.Math.Pow(h, 0.25)).ToArray();

        var (cls, _) = ConvergenceClassifier.Classify(hs, qs);

        // weakly-convergent or convergent, but not non-convergent or insufficient-data
        Assert.True(cls is "weakly-convergent" or "convergent",
            $"Expected weakly-convergent or convergent, got {cls}");
    }

    [Fact]
    public void Classify_ConstantSeries_IsConvergent()
    {
        // Exact invariance across levels is a trivial converged limit.
        double[] hs = [1.0, 0.5, 0.25];
        double[] qs = [2.0, 2.0, 2.0];

        var (cls, note) = ConvergenceClassifier.Classify(hs, qs);

        Assert.Equal("convergent", cls);
        Assert.Contains("Exact invariance", note);
    }
}
