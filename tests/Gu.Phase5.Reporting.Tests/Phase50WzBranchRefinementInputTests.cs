using Gu.Core.Serialization;
using Gu.Phase5.Convergence;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase50WzBranchRefinementInputTests
{
    private const string QuantityId = "physical-w-z-mass-ratio";

    [Fact]
    public void BranchQuantityValues_AreWzOnlyAndMatchPhase46SelectorVariants()
    {
        var table = LoadTable("branch_quantity_values.json");

        Assert.Equal("phase50-wz-branch-ratio-inputs", table.StudyId);
        Assert.Equal(
            [
                "bg-variant-95ee11778cc7ac01",
                "bg-variant-d840fea6e6d36748",
                "bg-variant-d84f5d66fd98b842",
                "bg-variant-53b598740d9569b4",
            ],
            table.Levels.Select(level => level.LevelId).ToArray());

        Assert.All(table.Levels, AssertWzOnlyQuantity);
        Assert.Equal(0.8800585830376143, table.Levels[0].Quantities[QuantityId], 1e-15);
        Assert.Equal(0.8790510375546156, table.Levels[1].Quantities[QuantityId], 1e-15);
        Assert.Equal(0.8806084654173567, table.Levels[2].Quantities[QuantityId], 1e-15);
        Assert.Equal(0.8790498288219942, table.Levels[3].Quantities[QuantityId], 1e-15);
    }

    [Fact]
    public void RefinementValues_AreWzOnlyAndMatchPhase46RefinementLevels()
    {
        var table = LoadTable("refinement_values.json");

        Assert.Equal("phase50-wz-refinement-ratio-inputs", table.StudyId);
        Assert.Equal(
            ["L0-2x2", "L1-4x4", "L2-8x8"],
            table.Levels.Select(level => level.LevelId).ToArray());

        Assert.All(table.Levels, AssertWzOnlyQuantity);
        Assert.All(table.Levels, level =>
            Assert.Equal(0.8796919787078953, level.Quantities[QuantityId], 1e-15));
    }

    private static void AssertWzOnlyQuantity(RefinementQuantityValueLevel level)
    {
        Assert.True(level.SolverConverged);
        var quantity = Assert.Single(level.Quantities);
        Assert.Equal(QuantityId, quantity.Key);
    }

    private static RefinementQuantityValueTable LoadTable(string fileName)
    {
        var path = Path.Combine(
            FindRepoRoot(),
            "studies",
            "phase50_wz_branch_refinement_inputs_001",
            "config",
            fileName);

        return GuJsonDefaults.Deserialize<RefinementQuantityValueTable>(File.ReadAllText(path))
            ?? throw new InvalidDataException($"Could not deserialize {path}.");
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "GeometricUnity.slnx")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}
