using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

public sealed class AtlasAndDashboardTests
{
    [Fact]
    public void BranchIndependenceAtlas_CorrectCounts()
    {
        var atlas = new BranchIndependenceAtlas
        {
            TotalQuantities = 5,
            InvariantCount = 3,
            FragileCount = 2,
            EquivalenceClassCount = 4,
            SummaryLines = ["- 3 invariant, 2 fragile"],
        };

        Assert.Equal(5, atlas.TotalQuantities);
        Assert.Equal(3, atlas.InvariantCount);
        Assert.Equal(2, atlas.FragileCount);
        Assert.Equal(4, atlas.EquivalenceClassCount);
    }

    [Fact]
    public void ConvergenceAtlas_CorrectCounts()
    {
        var atlas = new ConvergenceAtlas
        {
            TotalQuantities = 4,
            ConvergentCount = 2,
            NonConvergentCount = 1,
            InsufficientDataCount = 1,
            SummaryLines = ["- 2 convergent, 1 non-convergent, 1 insufficient"],
        };

        Assert.Equal(4, atlas.TotalQuantities);
        Assert.Equal(2, atlas.ConvergentCount);
        Assert.Equal(1, atlas.NonConvergentCount);
        Assert.Equal(1, atlas.InsufficientDataCount);
    }

    [Fact]
    public void FalsificationDashboard_CorrectCounts()
    {
        var dash = new FalsificationDashboard
        {
            TotalFalsifiers = 8,
            ActiveFatalCount = 0,
            ActiveHighCount = 2,
            PromotionCount = 1,
            DemotionCount = 2,
            SummaryLines = ["- 2 active high, 1 promotion, 2 demotions"],
        };

        Assert.Equal(8, dash.TotalFalsifiers);
        Assert.Equal(0, dash.ActiveFatalCount);
        Assert.Equal(2, dash.ActiveHighCount);
        Assert.Equal(1, dash.PromotionCount);
        Assert.Equal(2, dash.DemotionCount);
    }

    [Fact]
    public void BranchIndependenceAtlas_ZeroFragile_IsAllInvariant()
    {
        var atlas = new BranchIndependenceAtlas
        {
            TotalQuantities = 3,
            InvariantCount = 3,
            FragileCount = 0,
            EquivalenceClassCount = 1,
            SummaryLines = ["- all 3 invariant"],
        };

        Assert.Equal(0, atlas.FragileCount);
        Assert.Equal(atlas.TotalQuantities, atlas.InvariantCount);
    }

    [Fact]
    public void ConvergenceAtlas_AllInsufficient_HasNoConvergent()
    {
        var atlas = new ConvergenceAtlas
        {
            TotalQuantities = 2,
            ConvergentCount = 0,
            NonConvergentCount = 0,
            InsufficientDataCount = 2,
            SummaryLines = ["- insufficient data for all"],
        };

        Assert.Equal(0, atlas.ConvergentCount);
        Assert.Equal(2, atlas.InsufficientDataCount);
    }
}
