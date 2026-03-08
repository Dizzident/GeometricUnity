using Gu.Branching;
using Gu.Core.Factories;

namespace Gu.Core.Tests;

public class BranchManifestValidatorTests
{
    private static BranchManifest CreateValidManifest() => new()
    {
        BranchId = "valid-branch",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "rev-1",
        CodeRevision = "abc123",
        ActiveGeometryBranch = "simplicial-4d",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "local-algebraic",
        ActiveShiabBranch = "first-order-curvature",
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
        InsertedAssumptionIds = new[] { "IA-1", "IA-2", "IA-3", "IA-4", "IA-5", "IA-6" },
        InsertedChoiceIds = new[] { "IX-1", "IX-2", "IX-3", "IX-4", "IX-5" },
    };

    [Fact]
    public void ValidManifest_PassesValidation()
    {
        var manifest = CreateValidManifest();
        var errors = BranchManifestValidator.Validate(manifest);
        Assert.Empty(errors);
    }

    [Fact]
    public void EmptyManifest_FailsValidation()
    {
        var manifest = BranchManifestFactory.CreateEmpty();
        var errors = BranchManifestValidator.Validate(manifest);
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void EmptyManifest_ValidateOrThrow_Throws()
    {
        var manifest = BranchManifestFactory.CreateEmpty();
        Assert.Throws<InvalidOperationException>(() =>
            BranchManifestValidator.ValidateOrThrow(manifest));
    }

    [Fact]
    public void ValidManifest_ValidateOrThrow_DoesNotThrow()
    {
        var manifest = CreateValidManifest();
        BranchManifestValidator.ValidateOrThrow(manifest); // should not throw
    }

    [Fact]
    public void AmbientDimensionLessThanBase_FailsValidation()
    {
        var manifest = new BranchManifest
        {
            BranchId = "test", SchemaVersion = "1.0.0",
            SourceEquationRevision = "rev-1", CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-4d", ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "local-algebraic", ActiveShiabBranch = "first-order-curvature",
            ActiveGaugeStrategy = "penalty", BaseDimension = 4, AmbientDimension = 3,
            LieAlgebraId = "su2", BasisConventionId = "basis-standard",
            ComponentOrderId = "order-row-major", AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-killing", NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(), InsertedChoiceIds = Array.Empty<string>(),
        };
        var errors = BranchManifestValidator.Validate(manifest);
        Assert.Contains(errors, e => e.Contains("AmbientDimension"));
    }

    [Fact]
    public void UnsetLieAlgebraId_FailsValidation()
    {
        var manifest = new BranchManifest
        {
            BranchId = "test", SchemaVersion = "1.0.0",
            SourceEquationRevision = "rev-1", CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-4d", ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "local-algebraic", ActiveShiabBranch = "first-order-curvature",
            ActiveGaugeStrategy = "penalty", BaseDimension = 4, AmbientDimension = 14,
            LieAlgebraId = "unset", BasisConventionId = "basis-standard",
            ComponentOrderId = "order-row-major", AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-killing", NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(), InsertedChoiceIds = Array.Empty<string>(),
        };
        var errors = BranchManifestValidator.Validate(manifest);
        Assert.Contains(errors, e => e.Contains("LieAlgebraId"));
    }
}
