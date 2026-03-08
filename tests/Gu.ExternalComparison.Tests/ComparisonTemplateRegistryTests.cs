using Gu.ExternalComparison;

namespace Gu.ExternalComparison.Tests;

public class ComparisonTemplateRegistryTests
{
    [Fact]
    public void HardStructuralTemplates_Contains5()
    {
        Assert.Equal(5, ComparisonTemplateRegistry.HardStructuralTemplates.Count);
    }

    [Fact]
    public void TemplateIds_AreUnique()
    {
        var ids = ComparisonTemplateRegistry.HardStructuralTemplates.Select(t => t.TemplateId).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());
    }

    [Fact]
    public void AllTemplates_UseStructuralFactAdapter()
    {
        Assert.All(ComparisonTemplateRegistry.HardStructuralTemplates,
            t => Assert.Equal("structural_fact", t.AdapterType));
    }

    [Fact]
    public void GetById_ExistingId_ReturnsTemplate()
    {
        var template = ComparisonTemplateRegistry.GetById("structural-bianchi-identity");
        Assert.NotNull(template);
        Assert.Equal("bianchi-residual", template.ObservableId);
    }

    [Fact]
    public void GetById_NonexistentId_ReturnsNull()
    {
        Assert.Null(ComparisonTemplateRegistry.GetById("nonexistent"));
    }

    [Fact]
    public void HardStructuralTemplateIds_MatchesTemplates()
    {
        Assert.Equal(
            ComparisonTemplateRegistry.HardStructuralTemplates.Count,
            ComparisonTemplateRegistry.HardStructuralTemplateIds.Count);

        for (int i = 0; i < ComparisonTemplateRegistry.HardStructuralTemplates.Count; i++)
        {
            Assert.Equal(
                ComparisonTemplateRegistry.HardStructuralTemplates[i].TemplateId,
                ComparisonTemplateRegistry.HardStructuralTemplateIds[i]);
        }
    }

    [Fact]
    public void AllTemplates_LinkToValidFalsifiers()
    {
        foreach (var template in ComparisonTemplateRegistry.HardStructuralTemplates)
        {
            var falsifier = FalsifierRegistry.GetById(template.FalsifierCondition);
            Assert.NotNull(falsifier);
        }
    }
}
