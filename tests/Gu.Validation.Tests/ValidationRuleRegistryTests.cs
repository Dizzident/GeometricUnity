using Gu.Core;
using Gu.Validation;

namespace Gu.Validation.Tests;

public class ValidationRuleRegistryTests
{
    [Fact]
    public void CreateAlgebraicRules_HasThreeRules()
    {
        var algebra = TestHelpers.CreateSu2Algebra();
        var registry = ValidationRuleRegistry.CreateAlgebraicRules(algebra);

        Assert.Equal(3, registry.Rules.Count);
    }

    [Fact]
    public void CreateAlgebraicRules_AllInCorrectCategory()
    {
        var algebra = TestHelpers.CreateSu2Algebra();
        var registry = ValidationRuleRegistry.CreateAlgebraicRules(algebra);

        Assert.All(registry.Rules, r =>
            Assert.Equal(ValidationRuleRegistry.AlgebraicIdentity, r.Category));
    }

    [Fact]
    public void CreateSignatureRules_HasTwoRules()
    {
        var derived = TestHelpers.CreateValidDerivedState();
        var registry = ValidationRuleRegistry.CreateSignatureRules(derived);

        Assert.Equal(2, registry.Rules.Count);
    }

    [Fact]
    public void CreateManifestRules_HasOneRule()
    {
        var manifest = TestHelpers.CreateValidManifest();
        var registry = ValidationRuleRegistry.CreateManifestRules(manifest);

        Assert.Single(registry.Rules);
    }

    [Fact]
    public void CreateFullRegistry_HasSixRules()
    {
        var algebra = TestHelpers.CreateSu2Algebra();
        var derived = TestHelpers.CreateValidDerivedState();
        var manifest = TestHelpers.CreateValidManifest();

        var registry = ValidationRuleRegistry.CreateFullRegistry(algebra, derived, manifest);

        Assert.Equal(6, registry.Rules.Count);
    }

    [Fact]
    public void GetByCategory_ReturnsCorrectSubset()
    {
        var algebra = TestHelpers.CreateSu2Algebra();
        var derived = TestHelpers.CreateValidDerivedState();
        var manifest = TestHelpers.CreateValidManifest();

        var registry = ValidationRuleRegistry.CreateFullRegistry(algebra, derived, manifest);

        var algebraicRules = registry.GetByCategory(ValidationRuleRegistry.AlgebraicIdentity);
        Assert.Equal(3, algebraicRules.Count);

        var signatureRules = registry.GetByCategory(ValidationRuleRegistry.Signature);
        Assert.Equal(3, signatureRules.Count);
    }

    [Fact]
    public void ShapeMatchesCoefficients_CorrectShape_ReturnsTrue()
    {
        var field = TestHelpers.CreateTestField("test", new[] { 3, 4 });
        Assert.True(ValidationRuleRegistry.ShapeMatchesCoefficients(field));
    }

    [Fact]
    public void ShapeMatchesCoefficients_WrongSize_ReturnsFalse()
    {
        var field = new FieldTensor
        {
            Label = "test",
            Signature = TestHelpers.CreateTestSignature(),
            Shape = new[] { 3, 4 }, // expects 12
            Coefficients = new double[10], // only 10
        };
        Assert.False(ValidationRuleRegistry.ShapeMatchesCoefficients(field));
    }

    [Fact]
    public void ShapeMatchesCoefficients_EmptyShape_ReturnsTrueForEmptyCoeffs()
    {
        var field = new FieldTensor
        {
            Label = "test",
            Signature = TestHelpers.CreateTestSignature(),
            Shape = Array.Empty<int>(),
            Coefficients = Array.Empty<double>(),
        };
        Assert.True(ValidationRuleRegistry.ShapeMatchesCoefficients(field));
    }

    [Fact]
    public void ManifestHasNoUnsetFields_ValidManifest_ReturnsTrue()
    {
        var manifest = TestHelpers.CreateValidManifest();
        Assert.True(ValidationRuleRegistry.ManifestHasNoUnsetFields(manifest));
    }

    [Fact]
    public void ManifestHasNoUnsetFields_EmptyManifest_ReturnsFalse()
    {
        var manifest = TestHelpers.CreateIncompleteManifest();
        Assert.False(ValidationRuleRegistry.ManifestHasNoUnsetFields(manifest));
    }

    [Fact]
    public void CategoryConstants_AreCorrectStrings()
    {
        Assert.Equal("algebraic-identity", ValidationRuleRegistry.AlgebraicIdentity);
        Assert.Equal("conservation", ValidationRuleRegistry.Conservation);
        Assert.Equal("parity", ValidationRuleRegistry.Parity);
        Assert.Equal("convergence", ValidationRuleRegistry.Convergence);
        Assert.Equal("signature", ValidationRuleRegistry.Signature);
    }

    [Fact]
    public void Add_SingleRule_IncreasesCount()
    {
        var registry = new ValidationRuleRegistry();
        Assert.Empty(registry.Rules);

        registry.Add(new Gu.Validation.Rules.PredicateRule(
            "test-rule", "test", () => true, "test"));

        Assert.Single(registry.Rules);
    }
}
