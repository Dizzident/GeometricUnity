using Gu.Phase2.Branches;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Branches.Tests;

public class BranchFamilyValidatorTests
{
    [Fact]
    public void ValidVariant_NoErrors()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var errors = BranchFamilyValidator.ValidateVariant(variant);
        Assert.Empty(errors);
    }

    [Fact]
    public void EmptyId_ReportsError()
    {
        var variant = BranchVariantManifestTests.MakeVariant("");
        var errors = BranchFamilyValidator.ValidateVariant(variant);
        Assert.Contains(errors, e => e.Contains("Id"));
    }

    [Fact]
    public void EmptyTorsionVariant_ReportsError()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1", torsionVariant: "");
        var errors = BranchFamilyValidator.ValidateVariant(variant);
        Assert.Contains(errors, e => e.Contains("TorsionVariant"));
    }

    [Fact]
    public void ValidFamily_NoErrors()
    {
        var family = BranchVariantManifestTests.MakeFamily("fam-1",
            BranchVariantManifestTests.MakeVariant("v1"));
        var errors = BranchFamilyValidator.ValidateFamily(family);
        Assert.Empty(errors);
    }

    [Fact]
    public void EmptyFamilyId_ReportsError()
    {
        var family = BranchVariantManifestTests.MakeFamily("",
            BranchVariantManifestTests.MakeVariant("v1", familyId: ""));
        var errors = BranchFamilyValidator.ValidateFamily(family);
        Assert.Contains(errors, e => e.Contains("FamilyId"));
    }

    [Fact]
    public void DuplicateVariantIds_ReportsError()
    {
        var family = BranchVariantManifestTests.MakeFamily("fam-1",
            BranchVariantManifestTests.MakeVariant("v1"),
            BranchVariantManifestTests.MakeVariant("v1"));
        var errors = BranchFamilyValidator.ValidateFamily(family);
        Assert.Contains(errors, e => e.Contains("Duplicate"));
    }

    [Fact]
    public void VariantParentMismatch_ReportsError()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1", familyId: "other-family");
        var family = BranchVariantManifestTests.MakeFamily("fam-1", variant);
        var errors = BranchFamilyValidator.ValidateFamily(family);
        Assert.Contains(errors, e => e.Contains("parent"));
    }

    [Fact]
    public void ValidateFamilyOrThrow_InvalidFamily_Throws()
    {
        var family = BranchVariantManifestTests.MakeFamily("",
            BranchVariantManifestTests.MakeVariant("v1", familyId: ""));
        var ex = Assert.Throws<InvalidOperationException>(
            () => BranchFamilyValidator.ValidateFamilyOrThrow(family));
        Assert.Contains("FamilyId", ex.Message);
    }

    [Fact]
    public void ValidateVariantOrThrow_InvalidVariant_Throws()
    {
        var variant = BranchVariantManifestTests.MakeVariant("");
        Assert.Throws<InvalidOperationException>(
            () => BranchFamilyValidator.ValidateVariantOrThrow(variant));
    }

    [Fact]
    public void ValidEquivalenceSpec_NoErrors()
    {
        var spec = BranchVariantManifestTests.MakeEquivalence();
        var errors = BranchFamilyValidator.ValidateEquivalenceSpec(spec);
        Assert.Empty(errors);
    }

    [Fact]
    public void EmptyEquivalenceId_ReportsError()
    {
        var spec = new EquivalenceSpec
        {
            Id = "",
            Name = "Test",
            ComparedObjectClasses = new[] { "observed-output" },
            NormalizationProcedure = "none",
            AllowedTransformations = Array.Empty<string>(),
            Metrics = new[] { "l2-norm" },
            Tolerances = new Dictionary<string, double> { ["l2-norm"] = 1e-6 },
            InterpretationRule = "equivalent-if-within-tolerance",
        };
        var errors = BranchFamilyValidator.ValidateEquivalenceSpec(spec);
        Assert.Contains(errors, e => e.Contains("Id"));
    }

    [Fact]
    public void EmptyMetrics_ReportsError()
    {
        var spec = new EquivalenceSpec
        {
            Id = "eq-1",
            Name = "Test",
            ComparedObjectClasses = new[] { "observed-output" },
            NormalizationProcedure = "none",
            AllowedTransformations = Array.Empty<string>(),
            Metrics = Array.Empty<string>(),
            Tolerances = new Dictionary<string, double> { ["l2-norm"] = 1e-6 },
            InterpretationRule = "equivalent-if-within-tolerance",
        };
        var errors = BranchFamilyValidator.ValidateEquivalenceSpec(spec);
        Assert.Contains(errors, e => e.Contains("Metrics"));
    }

    [Fact]
    public void FamilyWithInvalidEquivalence_ReportsError()
    {
        var family = new BranchFamilyManifest
        {
            FamilyId = "fam-1",
            Description = "Test",
            Variants = new[] { BranchVariantManifestTests.MakeVariant("v1") },
            DefaultEquivalence = new EquivalenceSpec
            {
                Id = "",
                Name = "",
                ComparedObjectClasses = Array.Empty<string>(),
                NormalizationProcedure = "",
                AllowedTransformations = Array.Empty<string>(),
                Metrics = Array.Empty<string>(),
                Tolerances = new Dictionary<string, double>(),
                InterpretationRule = "",
            },
            CreatedAt = DateTimeOffset.UtcNow,
        };
        var errors = BranchFamilyValidator.ValidateFamily(family);
        Assert.Contains(errors, e => e.Contains("DefaultEquivalence"));
    }
}
