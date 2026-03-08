using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.ReferenceCpu.Tests;

public class BranchOperatorRegistryTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static BranchManifest ManifestWithBranches(
        string torsion = "trivial", string shiab = "identity-shiab") => new()
    {
        BranchId = "test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = torsion,
        ActiveShiabBranch = shiab,
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = Array.Empty<string>(),
    };

    [Fact]
    public void ResolveTorsion_RegisteredBranch_ReturnsOperator()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var registry = new BranchOperatorRegistry();
        registry.RegisterTorsion("trivial", _ => new TrivialTorsionCpu(mesh, algebra));

        var op = registry.ResolveTorsion(ManifestWithBranches());

        Assert.NotNull(op);
        Assert.Equal("trivial", op.BranchId);
    }

    [Fact]
    public void ResolveShiab_RegisteredBranch_ReturnsOperator()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var registry = new BranchOperatorRegistry();
        registry.RegisterShiab("identity-shiab", _ => new IdentityShiabCpu(mesh, algebra));

        var op = registry.ResolveShiab(ManifestWithBranches());

        Assert.NotNull(op);
        Assert.Equal("identity-shiab", op.BranchId);
    }

    [Fact]
    public void ResolveTorsion_UnregisteredBranch_Throws()
    {
        var registry = new BranchOperatorRegistry();

        var ex = Assert.Throws<InvalidOperationException>(
            () => registry.ResolveTorsion(ManifestWithBranches("nonexistent")));

        Assert.Contains("nonexistent", ex.Message);
    }

    [Fact]
    public void ResolveShiab_UnregisteredBranch_Throws()
    {
        var registry = new BranchOperatorRegistry();

        var ex = Assert.Throws<InvalidOperationException>(
            () => registry.ResolveShiab(ManifestWithBranches(shiab: "nonexistent")));

        Assert.Contains("nonexistent", ex.Message);
    }

    [Fact]
    public void ResolveBiConnection_DefaultsToSimple()
    {
        var registry = new BranchOperatorRegistry();
        registry.RegisterBiConnection("simple-a0-omega", _ => new SimpleBiConnectionCpu());

        var strategy = registry.ResolveBiConnection(ManifestWithBranches());

        Assert.NotNull(strategy);
        Assert.Equal("simple-a0-omega", strategy.StrategyId);
    }

    [Fact]
    public void ResolveBiConnection_CustomFromParameters()
    {
        var registry = new BranchOperatorRegistry();
        registry.RegisterBiConnection("simple-a0-omega", _ => new SimpleBiConnectionCpu());
        registry.RegisterBiConnection("custom", _ => new SimpleBiConnectionCpu()); // reuse for test

        var manifest = new BranchManifest
        {
            BranchId = "test",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1",
            CodeRevision = "test",
            ActiveGeometryBranch = "simplicial",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "trivial",
            ActiveShiabBranch = "identity-shiab",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraId = "su2",
            BasisConventionId = "canonical",
            ComponentOrderId = "face-major",
            AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-trace",
            NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
            Parameters = new Dictionary<string, string> { ["biConnectionStrategy"] = "custom" },
        };

        var strategy = registry.ResolveBiConnection(manifest);
        Assert.NotNull(strategy);
    }

    [Fact]
    public void ValidateCarrierMatch_MatchingCarriers_DoesNotThrow()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab); // should not throw
    }

    [Fact]
    public void ValidateCarrierMatch_MismatchedCarriers_Throws()
    {
        // Create a mock torsion with different carrier type
        var mockTorsion = new MismatchedCarrierTorsion();
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var shiab = new IdentityShiabCpu(mesh, algebra);

        Assert.Throws<InvalidOperationException>(
            () => BranchOperatorRegistry.ValidateCarrierMatch(mockTorsion, shiab));
    }

    /// <summary>
    /// Test helper: torsion operator with intentionally wrong carrier type.
    /// </summary>
    private sealed class MismatchedCarrierTorsion : ITorsionBranchOperator
    {
        public string BranchId => "mismatched";
        public string OutputCarrierType => "wrong-carrier-type";
        public TensorSignature OutputSignature => new()
        {
            AmbientSpaceId = "Y_h",
            CarrierType = OutputCarrierType,
            Degree = "2",
            LieAlgebraBasisId = "canonical",
            ComponentOrderId = "face-major",
            MemoryLayout = "dense-row-major",
        };

        public FieldTensor Evaluate(FieldTensor omega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
            => throw new NotImplementedException();

        public FieldTensor Linearize(FieldTensor omega, FieldTensor a0, FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry)
            => throw new NotImplementedException();
    }
}
