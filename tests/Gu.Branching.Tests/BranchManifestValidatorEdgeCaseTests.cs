using Gu.Branching;
using Gu.Core;
using Gu.Core.Factories;

namespace Gu.Branching.Tests;

public class BranchManifestValidatorEdgeCaseTests
{
    private static BranchManifest ManifestWith(Action<ManifestBuilder> modify)
    {
        var b = new ManifestBuilder();
        modify(b);
        return b.Build();
    }

    [Fact]
    public void WhitespaceBranchId_FailsValidation()
    {
        var m = ManifestWith(b => b.BranchId = "   ");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("BranchId"));
    }

    [Fact]
    public void EmptySchemaVersion_FailsValidation()
    {
        var m = ManifestWith(b => b.SchemaVersion = "");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("SchemaVersion"));
    }

    [Fact]
    public void UnsetActiveGeometryBranch_FailsValidation()
    {
        var m = ManifestWith(b => b.ActiveGeometryBranch = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("ActiveGeometryBranch"));
    }

    [Fact]
    public void UnsetActiveObservationBranch_FailsValidation()
    {
        var m = ManifestWith(b => b.ActiveObservationBranch = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("ActiveObservationBranch"));
    }

    [Fact]
    public void UnsetActiveTorsionBranch_FailsValidation()
    {
        var m = ManifestWith(b => b.ActiveTorsionBranch = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("ActiveTorsionBranch"));
    }

    [Fact]
    public void UnsetActiveShiabBranch_FailsValidation()
    {
        var m = ManifestWith(b => b.ActiveShiabBranch = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("ActiveShiabBranch"));
    }

    [Fact]
    public void UnsetActiveGaugeStrategy_FailsValidation()
    {
        var m = ManifestWith(b => b.ActiveGaugeStrategy = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("ActiveGaugeStrategy"));
    }

    [Fact]
    public void ZeroBaseDimension_FailsValidation()
    {
        var m = ManifestWith(b => b.BaseDimension = 0);
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("BaseDimension"));
    }

    [Fact]
    public void NegativeAmbientDimension_FailsValidation()
    {
        var m = ManifestWith(b => b.AmbientDimension = -1);
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("AmbientDimension"));
    }

    [Fact]
    public void EqualDimensions_FailsValidation()
    {
        var m = ManifestWith(b => { b.BaseDimension = 4; b.AmbientDimension = 4; });
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("AmbientDimension"));
    }

    [Fact]
    public void UnsetBasisConventionId_FailsValidation()
    {
        var m = ManifestWith(b => b.BasisConventionId = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("BasisConventionId"));
    }

    [Fact]
    public void UnsetComponentOrderId_FailsValidation()
    {
        var m = ManifestWith(b => b.ComponentOrderId = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("ComponentOrderId"));
    }

    [Fact]
    public void UnsetAdjointConventionId_FailsValidation()
    {
        var m = ManifestWith(b => b.AdjointConventionId = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("AdjointConventionId"));
    }

    [Fact]
    public void UnsetPairingConventionId_FailsValidation()
    {
        var m = ManifestWith(b => b.PairingConventionId = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("PairingConventionId"));
    }

    [Fact]
    public void UnsetNormConventionId_FailsValidation()
    {
        var m = ManifestWith(b => b.NormConventionId = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("NormConventionId"));
    }

    [Fact]
    public void UnsetDifferentialFormMetricId_FailsValidation()
    {
        var m = ManifestWith(b => b.DifferentialFormMetricId = "unset");
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("DifferentialFormMetricId"));
    }

    [Fact]
    public void MultipleErrors_AllReported()
    {
        var m = ManifestWith(b =>
        {
            b.BranchId = "";
            b.BaseDimension = 0;
            b.LieAlgebraId = "unset";
        });

        var errors = BranchManifestValidator.Validate(m);
        Assert.True(errors.Count >= 3);
    }

    [Fact]
    public void ValidateOrThrow_InvalidManifest_ContainsAllErrors()
    {
        var m = ManifestWith(b =>
        {
            b.BranchId = "";
            b.LieAlgebraId = "unset";
        });

        var ex = Assert.Throws<InvalidOperationException>(
            () => BranchManifestValidator.ValidateOrThrow(m));

        Assert.Contains("BranchId", ex.Message);
        Assert.Contains("LieAlgebraId", ex.Message);
    }

    [Fact]
    public void NullInsertedAssumptionIds_FailsValidation()
    {
        var m = ManifestWith(b => b.InsertedAssumptionIds = null!);
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("InsertedAssumptionIds"));
    }

    [Fact]
    public void NullInsertedChoiceIds_FailsValidation()
    {
        var m = ManifestWith(b => b.InsertedChoiceIds = null!);
        var errors = BranchManifestValidator.Validate(m);
        Assert.Contains(errors, e => e.Contains("InsertedChoiceIds"));
    }

    /// <summary>
    /// Mutable builder for BranchManifest to simplify test construction.
    /// </summary>
    private sealed class ManifestBuilder
    {
        public string BranchId = "valid-branch";
        public string SchemaVersion = "1.0.0";
        public string SourceEquationRevision = "rev-1";
        public string CodeRevision = "abc123";
        public string ActiveGeometryBranch = "simplicial-4d";
        public string ActiveObservationBranch = "sigma-pullback";
        public string ActiveTorsionBranch = "local-algebraic";
        public string ActiveShiabBranch = "first-order-curvature";
        public string ActiveGaugeStrategy = "penalty";
        public int BaseDimension = 4;
        public int AmbientDimension = 14;
        public string LieAlgebraId = "su2";
        public string BasisConventionId = "basis-standard";
        public string ComponentOrderId = "order-row-major";
        public string AdjointConventionId = "adjoint-explicit";
        public string PairingConventionId = "pairing-killing";
        public string NormConventionId = "norm-l2-quadrature";
        public string DifferentialFormMetricId = "hodge-standard";
        public IReadOnlyList<string> InsertedAssumptionIds = new[] { "IA-1" };
        public IReadOnlyList<string> InsertedChoiceIds = new[] { "IX-1" };

        public BranchManifest Build() => new()
        {
            BranchId = BranchId,
            SchemaVersion = SchemaVersion,
            SourceEquationRevision = SourceEquationRevision,
            CodeRevision = CodeRevision,
            ActiveGeometryBranch = ActiveGeometryBranch,
            ActiveObservationBranch = ActiveObservationBranch,
            ActiveTorsionBranch = ActiveTorsionBranch,
            ActiveShiabBranch = ActiveShiabBranch,
            ActiveGaugeStrategy = ActiveGaugeStrategy,
            BaseDimension = BaseDimension,
            AmbientDimension = AmbientDimension,
            LieAlgebraId = LieAlgebraId,
            BasisConventionId = BasisConventionId,
            ComponentOrderId = ComponentOrderId,
            AdjointConventionId = AdjointConventionId,
            PairingConventionId = PairingConventionId,
            NormConventionId = NormConventionId,
            DifferentialFormMetricId = DifferentialFormMetricId,
            InsertedAssumptionIds = InsertedAssumptionIds,
            InsertedChoiceIds = InsertedChoiceIds,
        };
    }
}

public class TensorSignatureEnforcerTests
{
    [Fact]
    public void ValidField_NoErrors()
    {
        var field = MakeField("test", 6, new[] { 2, 3 });
        var errors = TensorSignatureEnforcer.ValidateField(field);
        Assert.Empty(errors);
    }

    [Fact]
    public void ShapeMismatch_ReportsError()
    {
        var field = MakeField("test", 5, new[] { 2, 3 }); // 5 != 2*3=6
        var errors = TensorSignatureEnforcer.ValidateField(field);
        Assert.Contains(errors, e => e.Contains("5") && e.Contains("6"));
    }

    [Fact]
    public void NullSignature_ReportsError()
    {
        var field = new FieldTensor
        {
            Label = "test",
            Signature = null!,
            Coefficients = new double[3],
            Shape = new[] { 3 },
        };
        var errors = TensorSignatureEnforcer.ValidateField(field);
        Assert.Contains(errors, e => e.Contains("no tensor signature"));
    }

    [Fact]
    public void MissingAmbientSpaceId_ReportsError()
    {
        var field = new FieldTensor
        {
            Label = "test",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[3],
            Shape = new[] { 3 },
        };
        var errors = TensorSignatureEnforcer.ValidateField(field);
        Assert.Contains(errors, e => e.Contains("ambient space"));
    }

    [Fact]
    public void MissingCarrierType_ReportsError()
    {
        var field = new FieldTensor
        {
            Label = "test",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[3],
            Shape = new[] { 3 },
        };
        var errors = TensorSignatureEnforcer.ValidateField(field);
        Assert.Contains(errors, e => e.Contains("carrier type"));
    }

    [Fact]
    public void ValidateCompatibility_MatchingSignatures_NoErrors()
    {
        var sig = MakeSignature();
        var errors = TensorSignatureEnforcer.ValidateCompatibility(sig, "a", sig, "b");
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateCompatibility_DifferentCarrier_ReportsError()
    {
        var sigA = MakeSignature();
        var sigB = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "wrong-type",
            Degree = "1",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "edge-major",
            MemoryLayout = "dense-row-major",
        };

        var errors = TensorSignatureEnforcer.ValidateCompatibility(sigA, "a", sigB, "b");
        Assert.Contains(errors, e => e.Contains("Carrier type"));
    }

    [Fact]
    public void ValidateCompatibility_MultipleFieldMismatches_ReportsAll()
    {
        var sigA = MakeSignature();
        var sigB = new TensorSignature
        {
            AmbientSpaceId = "other",
            CarrierType = "other",
            Degree = "3",
            LieAlgebraBasisId = "other",
            ComponentOrderId = "other",
            NumericPrecision = "float32",
            MemoryLayout = "other",
        };

        var errors = TensorSignatureEnforcer.ValidateCompatibility(sigA, "a", sigB, "b");
        Assert.True(errors.Count >= 5);
    }

    [Fact]
    public void ValidateFieldOrThrow_InvalidField_Throws()
    {
        var field = MakeField("test", 5, new[] { 2, 3 });
        Assert.Throws<InvalidOperationException>(
            () => TensorSignatureEnforcer.ValidateFieldOrThrow(field));
    }

    [Fact]
    public void ValidateFieldOrThrow_ValidField_DoesNotThrow()
    {
        var field = MakeField("test", 6, new[] { 2, 3 });
        TensorSignatureEnforcer.ValidateFieldOrThrow(field);
    }

    [Fact]
    public void ValidateAgainstManifest_MatchingConventions_NoErrors()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "basis-standard",
            ComponentOrderId = "order-row-major",
            MemoryLayout = "dense-row-major",
        };
        var manifest = MakeManifest("basis-standard", "order-row-major");

        var errors = TensorSignatureEnforcer.ValidateAgainstManifest(sig, "test", manifest);
        Assert.Empty(errors);
    }

    [Fact]
    public void ValidateAgainstManifest_MismatchedBasis_ReportsError()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "wrong-basis",
            ComponentOrderId = "order-row-major",
            MemoryLayout = "dense-row-major",
        };
        var manifest = MakeManifest("basis-standard", "order-row-major");

        var errors = TensorSignatureEnforcer.ValidateAgainstManifest(sig, "test", manifest);
        Assert.Contains(errors, e => e.Contains("Lie algebra basis"));
    }

    [Fact]
    public void ValidateAgainstManifest_MismatchedComponentOrder_ReportsError()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "basis-standard",
            ComponentOrderId = "wrong-order",
            MemoryLayout = "dense-row-major",
        };
        var manifest = MakeManifest("basis-standard", "order-row-major");

        var errors = TensorSignatureEnforcer.ValidateAgainstManifest(sig, "test", manifest);
        Assert.Contains(errors, e => e.Contains("component order"));
    }

    private static FieldTensor MakeField(string label, int coeffCount, int[] shape)
    {
        return new FieldTensor
        {
            Label = label,
            Signature = MakeSignature(),
            Coefficients = new double[coeffCount],
            Shape = shape,
        };
    }

    private static TensorSignature MakeSignature()
    {
        return new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "edge-major",
            MemoryLayout = "dense-row-major",
        };
    }

    private static BranchManifest MakeManifest(string basisConvention, string componentOrder) => new()
    {
        BranchId = "test", SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1", CodeRevision = "c1",
        ActiveGeometryBranch = "geo", ActiveObservationBranch = "obs",
        ActiveTorsionBranch = "tor", ActiveShiabBranch = "shi",
        ActiveGaugeStrategy = "pen",
        BaseDimension = 4, AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = basisConvention,
        ComponentOrderId = componentOrder,
        AdjointConventionId = "adj", PairingConventionId = "pair",
        NormConventionId = "norm", DifferentialFormMetricId = "hodge",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = Array.Empty<string>(),
    };
}

public class InsertedAssumptionLedgerTests
{
    [Fact]
    public void All_ContainsExpectedAssumptions()
    {
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IA-1"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IA-5"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IX-1"));
        Assert.True(InsertedAssumptionLedger.All.ContainsKey("IX-5"));
    }

    [Fact]
    public void Assumptions_OnlyContainsIA()
    {
        Assert.All(InsertedAssumptionLedger.Assumptions,
            a => Assert.Equal("assumption", a.Category));
        Assert.All(InsertedAssumptionLedger.Assumptions,
            a => Assert.StartsWith("IA-", a.Id));
    }

    [Fact]
    public void Choices_OnlyContainsIX()
    {
        Assert.All(InsertedAssumptionLedger.Choices,
            c => Assert.Equal("choice", c.Category));
        Assert.All(InsertedAssumptionLedger.Choices,
            c => Assert.StartsWith("IX-", c.Id));
    }

    [Fact]
    public void AllEntries_HaveNonEmptyFields()
    {
        foreach (var entry in InsertedAssumptionLedger.All.Values)
        {
            Assert.False(string.IsNullOrWhiteSpace(entry.Id));
            Assert.False(string.IsNullOrWhiteSpace(entry.Title));
            Assert.False(string.IsNullOrWhiteSpace(entry.Description));
            Assert.False(string.IsNullOrWhiteSpace(entry.Category));
        }
    }

    [Fact]
    public void AssumptionCount_IsSix()
    {
        Assert.Equal(6, InsertedAssumptionLedger.Assumptions.Count);
    }

    [Fact]
    public void ChoiceCount_IsFive()
    {
        Assert.Equal(5, InsertedAssumptionLedger.Choices.Count);
    }

    [Fact]
    public void TotalCount_IsEleven()
    {
        Assert.Equal(11, InsertedAssumptionLedger.All.Count);
    }
}

public class BranchOperatorRegistryEdgeCaseTests
{
    [Fact]
    public void RegisterTorsion_NullBranchId_Throws()
    {
        var registry = new BranchOperatorRegistry();
        Assert.Throws<ArgumentNullException>(
            () => registry.RegisterTorsion(null!, _ => null!));
    }

    [Fact]
    public void RegisterTorsion_NullFactory_Throws()
    {
        var registry = new BranchOperatorRegistry();
        Assert.Throws<ArgumentNullException>(
            () => registry.RegisterTorsion("test", null!));
    }

    [Fact]
    public void RegisterShiab_NullBranchId_Throws()
    {
        var registry = new BranchOperatorRegistry();
        Assert.Throws<ArgumentNullException>(
            () => registry.RegisterShiab(null!, _ => null!));
    }

    [Fact]
    public void RegisterShiab_NullFactory_Throws()
    {
        var registry = new BranchOperatorRegistry();
        Assert.Throws<ArgumentNullException>(
            () => registry.RegisterShiab("test", null!));
    }

    [Fact]
    public void RegisterBiConnection_NullStrategyId_Throws()
    {
        var registry = new BranchOperatorRegistry();
        Assert.Throws<ArgumentNullException>(
            () => registry.RegisterBiConnection(null!, _ => null!));
    }

    [Fact]
    public void RegisterBiConnection_NullFactory_Throws()
    {
        var registry = new BranchOperatorRegistry();
        Assert.Throws<ArgumentNullException>(
            () => registry.RegisterBiConnection("test", null!));
    }

    [Fact]
    public void ResolveBiConnection_CustomStrategy_FromParameters()
    {
        var registry = new BranchOperatorRegistry();
        bool customCalled = false;

        registry.RegisterBiConnection("my-custom-strategy", _ =>
        {
            customCalled = true;
            return new StubBiConnection("my-custom-strategy");
        });

        var manifest = MakeManifest(new Dictionary<string, string>
        {
            ["biConnectionStrategy"] = "my-custom-strategy",
        });

        var result = registry.ResolveBiConnection(manifest);
        Assert.True(customCalled);
        Assert.Equal("my-custom-strategy", result.StrategyId);
    }

    [Fact]
    public void ResolveBiConnection_NoParameters_UsesDefault()
    {
        var registry = new BranchOperatorRegistry();
        registry.RegisterBiConnection("simple-a0-omega", _ => new StubBiConnection("simple-a0-omega"));

        var manifest = MakeManifest(null);
        var result = registry.ResolveBiConnection(manifest);
        Assert.Equal("simple-a0-omega", result.StrategyId);
    }

    [Fact]
    public void ResolveBiConnection_UnregisteredStrategy_Throws()
    {
        var registry = new BranchOperatorRegistry();
        var manifest = MakeManifest(new Dictionary<string, string>
        {
            ["biConnectionStrategy"] = "nonexistent",
        });

        var ex = Assert.Throws<InvalidOperationException>(
            () => registry.ResolveBiConnection(manifest));
        Assert.Contains("nonexistent", ex.Message);
    }

    [Fact]
    public void RegisterTorsion_OverwritesPrevious()
    {
        var registry = new BranchOperatorRegistry();
        int callCount = 0;

        registry.RegisterTorsion("test", _ =>
        {
            callCount = 1;
            return new StubTorsion();
        });
        registry.RegisterTorsion("test", _ =>
        {
            callCount = 2;
            return new StubTorsion();
        });

        var manifest = MakeManifest(null);
        manifest = new BranchManifest
        {
            BranchId = manifest.BranchId,
            SchemaVersion = manifest.SchemaVersion,
            SourceEquationRevision = manifest.SourceEquationRevision,
            CodeRevision = manifest.CodeRevision,
            ActiveGeometryBranch = manifest.ActiveGeometryBranch,
            ActiveObservationBranch = manifest.ActiveObservationBranch,
            ActiveTorsionBranch = "test",
            ActiveShiabBranch = manifest.ActiveShiabBranch,
            ActiveGaugeStrategy = manifest.ActiveGaugeStrategy,
            BaseDimension = manifest.BaseDimension,
            AmbientDimension = manifest.AmbientDimension,
            LieAlgebraId = manifest.LieAlgebraId,
            BasisConventionId = manifest.BasisConventionId,
            ComponentOrderId = manifest.ComponentOrderId,
            AdjointConventionId = manifest.AdjointConventionId,
            PairingConventionId = manifest.PairingConventionId,
            NormConventionId = manifest.NormConventionId,
            DifferentialFormMetricId = manifest.DifferentialFormMetricId,
            InsertedAssumptionIds = manifest.InsertedAssumptionIds,
            InsertedChoiceIds = manifest.InsertedChoiceIds,
        };
        registry.ResolveTorsion(manifest);

        Assert.Equal(2, callCount);
    }

    private static BranchManifest MakeManifest(Dictionary<string, string>? parameters) => new()
    {
        BranchId = "test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "c1",
        ActiveGeometryBranch = "geo",
        ActiveObservationBranch = "obs",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adj",
        PairingConventionId = "pair",
        NormConventionId = "norm",
        DifferentialFormMetricId = "hodge",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = Array.Empty<string>(),
        Parameters = parameters,
    };

    private sealed class StubBiConnection : IBiConnectionStrategy
    {
        public string StrategyId { get; }
        public StubBiConnection(string id) => StrategyId = id;
        public BiConnectionResult Evaluate(FieldTensor omega, FieldTensor a0,
            BranchManifest manifest, GeometryContext geometry)
            => throw new NotImplementedException();
        public BiConnectionResult Linearize(FieldTensor omega, FieldTensor a0,
            FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry)
            => throw new NotImplementedException();
    }

    private sealed class StubTorsion : ITorsionBranchOperator
    {
        public string BranchId => "test";
        public string OutputCarrierType => "curvature-2form";
        public TensorSignature OutputSignature => new()
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "face-major",
            MemoryLayout = "dense-row-major",
        };
        public FieldTensor Evaluate(FieldTensor omega, FieldTensor a0,
            BranchManifest manifest, GeometryContext geometry)
            => throw new NotImplementedException();
        public FieldTensor Linearize(FieldTensor omega, FieldTensor a0,
            FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry)
            => throw new NotImplementedException();
    }
}
