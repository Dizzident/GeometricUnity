using Gu.ExternalComparison;

namespace Gu.ExternalComparison.Tests;

public class FalsifierRegistryTests
{
    [Fact]
    public void All_Contains10Falsifiers()
    {
        Assert.Equal(10, FalsifierRegistry.All.Count);
    }

    [Fact]
    public void HardStructural_Contains5()
    {
        Assert.Equal(5, FalsifierRegistry.HardStructural.Count);
        Assert.All(FalsifierRegistry.HardStructural, f =>
        {
            Assert.Equal("HardStructural", f.Category);
            Assert.Equal("Fatal", f.Severity);
        });
    }

    [Fact]
    public void SoftPhysics_Contains5()
    {
        Assert.Equal(5, FalsifierRegistry.SoftPhysics.Count);
        Assert.All(FalsifierRegistry.SoftPhysics, f =>
        {
            Assert.Equal("SoftPhysics", f.Category);
            Assert.Equal("Warning", f.Severity);
        });
    }

    [Fact]
    public void AllIds_AreUnique()
    {
        var ids = FalsifierRegistry.All.Select(f => f.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void GetById_ExistingId_ReturnsFalsifier()
    {
        var check = FalsifierRegistry.GetById("F-HARD-01");
        Assert.NotNull(check);
        Assert.Equal("F-HARD-01", check.Id);
        Assert.Contains("Bianchi", check.Description);
    }

    [Fact]
    public void GetById_NonexistentId_ReturnsNull()
    {
        Assert.Null(FalsifierRegistry.GetById("F-NONEXISTENT"));
    }

    [Fact]
    public void AllFalsifiers_HaveNonEmptyDescriptions()
    {
        Assert.All(FalsifierRegistry.All, f =>
        {
            Assert.NotNull(f.Description);
            Assert.NotEmpty(f.Description);
        });
    }

    [Fact]
    public void AllFalsifiers_HaveValidTolerancePolicies()
    {
        Assert.All(FalsifierRegistry.All, f =>
        {
            Assert.NotNull(f.Tolerance);
            Assert.NotNull(f.Tolerance.BaseToleranceType);
            Assert.NotEmpty(f.Tolerance.BaseToleranceType);
        });
    }

    [Theory]
    [InlineData("F-HARD-01", "Bianchi")]
    [InlineData("F-HARD-02", "topological charge")]
    [InlineData("F-HARD-03", "gauge zero modes")]
    [InlineData("F-HARD-04", "carrier")]
    [InlineData("F-HARD-05", "covariantly")]
    [InlineData("F-SOFT-01", "decrease")]
    [InlineData("F-SOFT-02", "saddle")]
    [InlineData("F-SOFT-03", "stable")]
    [InlineData("F-SOFT-04", "orders of magnitude")]
    [InlineData("F-SOFT-05", "symmetries")]
    public void FalsifierDescriptions_ContainKeyTerms(string id, string expectedTerm)
    {
        var check = FalsifierRegistry.GetById(id);
        Assert.NotNull(check);
        Assert.Contains(expectedTerm, check.Description, StringComparison.OrdinalIgnoreCase);
    }
}
