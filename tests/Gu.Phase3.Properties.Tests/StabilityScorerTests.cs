using Gu.Phase3.Spectra;

namespace Gu.Phase3.Properties.Tests;

public class StabilityScorerTests
{
    [Fact]
    public void BranchStability_SingleVariant_IsOne()
    {
        var mode = TestHelpers.MakeMode("m-1", 1.0, new double[] { 1, 0, 0 });
        var branches = new Dictionary<string, ModeRecord> { ["v1"] = mode };

        var card = StabilityScorer.ComputeBranchStability("fam-1", branches);
        Assert.Equal(1.0, card.BranchStability);
        Assert.Equal(1, card.BranchVariantCount);
    }

    [Fact]
    public void BranchStability_IdenticalEigenvalues_IsNearOne()
    {
        var branches = new Dictionary<string, ModeRecord>
        {
            ["v1"] = TestHelpers.MakeMode("m-1", 1.0, new double[] { 1, 0, 0 }),
            ["v2"] = TestHelpers.MakeMode("m-2", 1.0, new double[] { 1, 0, 0 }),
            ["v3"] = TestHelpers.MakeMode("m-3", 1.0, new double[] { 1, 0, 0 }),
        };

        var card = StabilityScorer.ComputeBranchStability("fam-1", branches);
        Assert.True(card.BranchStability > 0.99);
    }

    [Fact]
    public void BranchStability_LargeDrift_IsLow()
    {
        var branches = new Dictionary<string, ModeRecord>
        {
            ["v1"] = TestHelpers.MakeMode("m-1", 1.0, new double[] { 1, 0, 0 }),
            ["v2"] = TestHelpers.MakeMode("m-2", 5.0, new double[] { 0, 1, 0 }),
        };

        var card = StabilityScorer.ComputeBranchStability("fam-1", branches);
        Assert.True(card.BranchStability < 0.5, $"Expected low stability, got {card.BranchStability}");
        Assert.NotNull(card.MaxEigenvalueDrift);
        Assert.Equal(4.0, card.MaxEigenvalueDrift!.Value, 10);
    }

    [Fact]
    public void RefinementStability_SingleLevel_IsOne()
    {
        var modes = new[] { TestHelpers.MakeMode("m-1", 1.0, new double[] { 1, 0, 0 }) };
        var card = StabilityScorer.ComputeRefinementStability("fam-1", modes);
        Assert.Equal(1.0, card.RefinementStability);
    }

    [Fact]
    public void RefinementStability_ConvergingEigenvalues_IsHigh()
    {
        var modes = new[]
        {
            TestHelpers.MakeMode("m-coarse", 1.01, new double[] { 1, 0, 0 }),
            TestHelpers.MakeMode("m-fine", 1.00, new double[] { 1, 0, 0 }),
        };

        var card = StabilityScorer.ComputeRefinementStability("fam-1", modes);
        Assert.True(card.RefinementStability > 0.9);
    }

    [Fact]
    public void BackendStability_IdenticalResults_IsOne()
    {
        var cpu = TestHelpers.MakeMode("m-cpu", 1.0, new double[] { 1, 0, 0 });
        var cuda = TestHelpers.MakeMode("m-cuda", 1.0, new double[] { 1, 0, 0 });

        var card = StabilityScorer.ComputeBackendStability("fam-1", cpu, cuda);
        Assert.Equal(1.0, card.BackendStability, 10);
        Assert.Equal(2, card.BackendCount);
    }

    [Fact]
    public void BackendStability_SmallDiscrepancy_IsStillHigh()
    {
        var cpu = TestHelpers.MakeMode("m-cpu", 1.0, new double[] { 1, 0, 0 });
        var cuda = TestHelpers.MakeMode("m-cuda", 1.001, new double[] { 1, 0, 0 });

        var card = StabilityScorer.ComputeBackendStability("fam-1", cpu, cuda);
        Assert.True(card.BackendStability > 0.9);
    }

    [Fact]
    public void Merge_CombinesScores()
    {
        var branchCard = new StabilityScoreCard
        {
            EntityId = "fam-1",
            BranchStability = 0.8,
            RefinementStability = 1.0,
            BackendStability = 1.0,
            BranchVariantCount = 3,
            RefinementLevelCount = 0,
            BackendCount = 0,
            MaxEigenvalueDrift = 0.1,
        };
        var refineCard = new StabilityScoreCard
        {
            EntityId = "fam-1",
            BranchStability = 1.0,
            RefinementStability = 0.95,
            BackendStability = 1.0,
            BranchVariantCount = 0,
            RefinementLevelCount = 2,
            BackendCount = 0,
            MaxEigenvalueDrift = 0.01,
        };

        var merged = StabilityScorer.Merge("fam-1", branchCard, refineCard);
        Assert.Equal(0.8, merged.BranchStability);
        Assert.Equal(0.95, merged.RefinementStability);
        Assert.Equal(1.0, merged.BackendStability);
        Assert.Equal(3, merged.BranchVariantCount);
        Assert.Equal(2, merged.RefinementLevelCount);
    }

    [Fact]
    public void Merge_EmptyCards_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            StabilityScorer.Merge("fam-1"));
    }
}
