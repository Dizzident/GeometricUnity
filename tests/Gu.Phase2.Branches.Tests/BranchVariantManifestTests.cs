using Gu.Core;
using Gu.Phase2.Branches;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Branches.Tests;

public class BranchVariantManifestTests
{
    [Fact]
    public void CanConstructVariantWithAllRequiredFields()
    {
        var variant = MakeVariant("v1");
        Assert.Equal("v1", variant.Id);
        Assert.Equal("fam-1", variant.ParentFamilyId);
    }

    [Fact]
    public void CanConstructFamilyWithMultipleVariants()
    {
        var family = MakeFamily("fam-1", MakeVariant("v1"), MakeVariant("v2"));
        Assert.Equal(2, family.Variants.Count);
    }

    public static BranchVariantManifest MakeVariant(
        string id,
        string familyId = "fam-1",
        string torsionVariant = "local-algebraic",
        string shiabVariant = "identity-shiab")
    {
        return new BranchVariantManifest
        {
            Id = id,
            ParentFamilyId = familyId,
            A0Variant = "flat-A0",
            BiConnectionVariant = "simple-a0-omega",
            TorsionVariant = torsionVariant,
            ShiabVariant = shiabVariant,
            ObservationVariant = "sigma-pullback",
            ExtractionVariant = "standard-extraction",
            GaugeVariant = "penalty",
            RegularityVariant = "standard-regularity",
            PairingVariant = "pairing-trace",
            ExpectedClaimCeiling = "branch-local-numerical",
        };
    }

    public static BranchFamilyManifest MakeFamily(
        string familyId,
        params BranchVariantManifest[] variants)
    {
        return new BranchFamilyManifest
        {
            FamilyId = familyId,
            Description = "Test family",
            Variants = variants,
            DefaultEquivalence = MakeEquivalence(),
            CreatedAt = DateTimeOffset.UtcNow,
        };
    }

    public static EquivalenceSpec MakeEquivalence()
    {
        return new EquivalenceSpec
        {
            Id = "eq-default",
            Name = "Default",
            ComparedObjectClasses = new[] { "observed-output" },
            NormalizationProcedure = "none",
            AllowedTransformations = Array.Empty<string>(),
            Metrics = new[] { "l2-norm" },
            Tolerances = new Dictionary<string, double> { ["l2-norm"] = 1e-6 },
            InterpretationRule = "equivalent-if-within-tolerance",
        };
    }

    public static BranchManifest MakeBaseManifest()
    {
        return new BranchManifest
        {
            BranchId = "base",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "rev-1",
            CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-4d",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "local-algebraic",
            ActiveShiabBranch = "identity-shiab",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraId = "su2",
            BasisConventionId = "basis-standard",
            ComponentOrderId = "order-row-major",
            AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-killing",
            NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = new[] { "IA-1" },
            InsertedChoiceIds = new[] { "IX-1" },
        };
    }
}

public class BranchVariantResolverTests
{
    [Fact]
    public void Resolve_ProducesValidManifest()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var manifest = BranchVariantResolver.Resolve(variant, baseManifest);

        Assert.Equal("v1", manifest.BranchId);
        Assert.Equal("local-algebraic", manifest.ActiveTorsionBranch);
        Assert.Equal("identity-shiab", manifest.ActiveShiabBranch);
        Assert.Equal("penalty", manifest.ActiveGaugeStrategy);
        Assert.Equal(4, manifest.BaseDimension);
        Assert.Equal(14, manifest.AmbientDimension);
        Assert.Equal("su2", manifest.LieAlgebraId);
        Assert.Equal("pairing-trace", manifest.PairingConventionId);
    }

    [Fact]
    public void Resolve_BiConnectionInParameters()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var manifest = BranchVariantResolver.Resolve(variant, baseManifest);

        Assert.NotNull(manifest.Parameters);
        Assert.Equal("simple-a0-omega", manifest.Parameters!["biConnectionStrategy"]);
    }

    [Fact]
    public void Resolve_TwoVariants_ProduceDifferentManifests()
    {
        var v1 = BranchVariantManifestTests.MakeVariant("v1", torsionVariant: "local-algebraic", shiabVariant: "identity-shiab");
        var v2 = BranchVariantManifestTests.MakeVariant("v2", torsionVariant: "augmented-torsion", shiabVariant: "covariant-shiab");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var m1 = BranchVariantResolver.Resolve(v1, baseManifest);
        var m2 = BranchVariantResolver.Resolve(v2, baseManifest);

        Assert.NotEqual(m1.ActiveTorsionBranch, m2.ActiveTorsionBranch);
        Assert.NotEqual(m1.ActiveShiabBranch, m2.ActiveShiabBranch);
        Assert.Equal(m1.BaseDimension, m2.BaseDimension);
        Assert.Equal(m1.LieAlgebraId, m2.LieAlgebraId);
    }

    [Fact]
    public void Resolve_PreservesBaseManifestFields()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var manifest = BranchVariantResolver.Resolve(variant, baseManifest);

        Assert.Equal("simplicial-4d", manifest.ActiveGeometryBranch);
        Assert.Equal("basis-standard", manifest.BasisConventionId);
        Assert.Equal("order-row-major", manifest.ComponentOrderId);
        Assert.Equal("hodge-standard", manifest.DifferentialFormMetricId);
    }

    [Fact]
    public void Resolve_OverridesVariantFields()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var manifest = BranchVariantResolver.Resolve(variant, baseManifest);

        // These should come from variant, not base
        Assert.Equal("sigma-pullback", manifest.ActiveObservationBranch);
        Assert.Equal("local-algebraic", manifest.ActiveTorsionBranch);
        Assert.Equal("identity-shiab", manifest.ActiveShiabBranch);
        Assert.Equal("penalty", manifest.ActiveGaugeStrategy);
        Assert.Equal("pairing-trace", manifest.PairingConventionId);
    }

    [Fact]
    public void Resolve_RejectsBaseWithBranchSensitiveParameters()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = new BranchManifest
        {
            BranchId = "base",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "rev-1",
            CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-4d",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "local-algebraic",
            ActiveShiabBranch = "identity-shiab",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraId = "su2",
            BasisConventionId = "basis-standard",
            ComponentOrderId = "order-row-major",
            AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-killing",
            NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = new[] { "IA-1" },
            InsertedChoiceIds = new[] { "IX-1" },
            Parameters = new Dictionary<string, string>
            {
                ["biConnectionStrategy"] = "hidden-default",
            },
        };

        var ex = Assert.Throws<ArgumentException>(
            () => BranchVariantResolver.Resolve(variant, baseManifest));
        Assert.Contains("branch-sensitive", ex.Message);
        Assert.Contains("biConnectionStrategy", ex.Message);
    }

    [Fact]
    public void Resolve_NullVariant_Throws()
    {
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();
        Assert.Throws<ArgumentNullException>(
            () => BranchVariantResolver.Resolve(null!, baseManifest));
    }

    [Fact]
    public void Resolve_NullBaseManifest_Throws()
    {
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        Assert.Throws<ArgumentNullException>(
            () => BranchVariantResolver.Resolve(variant, null!));
    }
}
