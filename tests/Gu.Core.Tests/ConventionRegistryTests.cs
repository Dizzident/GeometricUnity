using Gu.Branching.Conventions;

namespace Gu.Core.Tests;

public class ConventionRegistryTests
{
    [Fact]
    public void CreateDefault_RegistersAllRequiredCategories()
    {
        // Per Section 11: basis, componentOrder, adjoint, pairing, norm conventions required
        var registry = ConventionRegistry.CreateDefault();

        Assert.True(registry.GetByCategory("basis").Any());
        Assert.True(registry.GetByCategory("componentOrder").Any());
        Assert.True(registry.GetByCategory("adjoint").Any());
        Assert.True(registry.GetByCategory("pairing").Any());
        Assert.True(registry.GetByCategory("norm").Any());
    }

    [Fact]
    public void CreateDefault_ContainsKnownConventions()
    {
        var registry = ConventionRegistry.CreateDefault();

        Assert.True(registry.Contains("basis-standard"));
        Assert.True(registry.Contains("order-row-major"));
        Assert.True(registry.Contains("adjoint-explicit"));
        Assert.True(registry.Contains("pairing-killing"));
        Assert.True(registry.Contains("pairing-trace"));
        Assert.True(registry.Contains("norm-l2-quadrature"));
    }

    [Fact]
    public void Register_DuplicateId_Throws()
    {
        var registry = new ConventionRegistry();
        var convention = new ConventionDescriptor
        {
            ConventionId = "test-conv",
            Category = "basis",
            Description = "Test convention"
        };

        registry.Register(convention);

        Assert.Throws<InvalidOperationException>(() => registry.Register(convention));
    }

    [Fact]
    public void Get_NonexistentId_Throws()
    {
        var registry = new ConventionRegistry();
        Assert.Throws<KeyNotFoundException>(() => registry.Get("nonexistent"));
    }

    [Fact]
    public void Get_ReturnsRegisteredConvention()
    {
        var registry = ConventionRegistry.CreateDefault();
        var convention = registry.Get("basis-standard");

        Assert.Equal("basis", convention.Category);
        Assert.False(string.IsNullOrWhiteSpace(convention.Description));
    }

    [Fact]
    public void AdjointConvention_RequiresExplicitDeclaration()
    {
        // Per Section 11.2: adjoint must be explicit, not silently assumed as transpose
        var registry = ConventionRegistry.CreateDefault();
        var adjoint = registry.Get("adjoint-explicit");

        Assert.Contains("explicit", adjoint.Description.ToLowerInvariant());
    }

    [Fact]
    public void GetByCategory_ReturnsOnlyMatchingCategory()
    {
        var registry = ConventionRegistry.CreateDefault();
        var pairings = registry.GetByCategory("pairing").ToList();

        Assert.True(pairings.Count >= 2); // killing + trace at minimum
        Assert.All(pairings, p => Assert.Equal("pairing", p.Category));
    }
}
