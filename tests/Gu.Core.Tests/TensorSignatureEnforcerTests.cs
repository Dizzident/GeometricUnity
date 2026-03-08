using Gu.Branching;

namespace Gu.Core.Tests;

public class TensorSignatureEnforcerTests
{
    private static TensorSignature CreateValidSignature() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "connection-1form",
        Degree = "1",
        LieAlgebraBasisId = "basis-standard",
        ComponentOrderId = "order-row-major",
        MemoryLayout = "dense-row-major",
        NumericPrecision = "float64",
    };

    private static FieldTensor CreateValidField() => new()
    {
        Label = "omega_h",
        Signature = CreateValidSignature(),
        Coefficients = new double[] { 1.0, 2.0, 3.0, 4.0 },
        Shape = new[] { 4 },
    };

    [Fact]
    public void ValidField_PassesValidation()
    {
        var field = CreateValidField();
        var errors = TensorSignatureEnforcer.ValidateField(field);
        Assert.Empty(errors);
    }

    [Fact]
    public void ShapeMismatch_FailsValidation()
    {
        var field = new FieldTensor
        {
            Label = "bad-field",
            Signature = CreateValidSignature(),
            Coefficients = new double[] { 1.0, 2.0, 3.0 },
            Shape = new[] { 2, 2 }, // 4 expected, 3 actual
        };
        var errors = TensorSignatureEnforcer.ValidateField(field);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("coefficients"));
    }

    [Fact]
    public void CompatibleSignatures_PassValidation()
    {
        var sigA = CreateValidSignature();
        var sigB = CreateValidSignature();
        var errors = TensorSignatureEnforcer.ValidateCompatibility(sigA, "A", sigB, "B");
        Assert.Empty(errors);
    }

    [Fact]
    public void IncompatibleCarrierType_FailsCompatibility()
    {
        var sigA = CreateValidSignature();
        var sigB = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form", // different carrier
            Degree = "1",
            LieAlgebraBasisId = "basis-standard",
            ComponentOrderId = "order-row-major",
            MemoryLayout = "dense-row-major",
        };
        var errors = TensorSignatureEnforcer.ValidateCompatibility(sigA, "A", sigB, "B");
        Assert.Contains(errors, e => e.Contains("Carrier type mismatch"));
    }

    [Fact]
    public void IncompatibleComponentOrder_FailsCompatibility()
    {
        var sigA = CreateValidSignature();
        var sigB = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "basis-standard",
            ComponentOrderId = "col-major",
            MemoryLayout = "dense-row-major",
        };
        var errors = TensorSignatureEnforcer.ValidateCompatibility(sigA, "A", sigB, "B");
        Assert.Contains(errors, e => e.Contains("Component order mismatch"));
    }

    [Fact]
    public void ValidateFieldOrThrow_ThrowsOnInvalid()
    {
        var field = new FieldTensor
        {
            Label = "invalid",
            Signature = CreateValidSignature(),
            Coefficients = new double[] { 1.0 },
            Shape = new[] { 2, 2 },
        };
        Assert.Throws<InvalidOperationException>(() =>
            TensorSignatureEnforcer.ValidateFieldOrThrow(field));
    }

    [Fact]
    public void ValidateAgainstManifest_MatchingConventions_Passes()
    {
        var sig = CreateValidSignature();
        var manifest = new BranchManifest
        {
            BranchId = "test",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1",
            CodeRevision = "c1",
            ActiveGeometryBranch = "g",
            ActiveObservationBranch = "o",
            ActiveTorsionBranch = "t",
            ActiveShiabBranch = "s",
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
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };

        var errors = TensorSignatureEnforcer.ValidateAgainstManifest(sig, "omega_h", manifest);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAgainstManifest_MismatchedBasis_Fails()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "different-basis",
            ComponentOrderId = "order-row-major",
            MemoryLayout = "dense-row-major",
        };
        var manifest = new BranchManifest
        {
            BranchId = "test",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1",
            CodeRevision = "c1",
            ActiveGeometryBranch = "g",
            ActiveObservationBranch = "o",
            ActiveTorsionBranch = "t",
            ActiveShiabBranch = "s",
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
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };

        var errors = TensorSignatureEnforcer.ValidateAgainstManifest(sig, "omega_h", manifest);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("basis"));
    }
}
