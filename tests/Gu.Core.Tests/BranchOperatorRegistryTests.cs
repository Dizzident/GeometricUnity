using Gu.Branching;
using Gu.Core;
using Gu.Core.Factories;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;

namespace Gu.Core.Tests;

public class BranchOperatorRegistryTests
{
    private static BranchManifest CreateManifest(
        string torsionBranch = "trivial",
        string shiabBranch = "identity-shiab")
    {
        return new BranchManifest
        {
            BranchId = "test",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "rev-1",
            CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-2d",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = torsionBranch,
            ActiveShiabBranch = shiabBranch,
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 2,
            AmbientDimension = 5,
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
    }

    private static SimplicialMesh CreateMesh()
    {
        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });
    }

    [Fact]
    public void RegisterAndResolve_Torsion()
    {
        var registry = new BranchOperatorRegistry();
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        registry.RegisterTorsion("trivial",
            manifest => new TrivialTorsionCpu(mesh, algebra));

        var manifest = CreateManifest();
        var torsion = registry.ResolveTorsion(manifest);

        Assert.NotNull(torsion);
        Assert.Equal("trivial", torsion.BranchId);
    }

    [Fact]
    public void RegisterAndResolve_Shiab()
    {
        var registry = new BranchOperatorRegistry();
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        registry.RegisterShiab("identity-shiab",
            manifest => new IdentityShiabCpu(mesh, algebra));

        var manifest = CreateManifest();
        var shiab = registry.ResolveShiab(manifest);

        Assert.NotNull(shiab);
        Assert.Equal("identity-shiab", shiab.BranchId);
    }

    [Fact]
    public void UnregisteredTorsion_Throws()
    {
        var registry = new BranchOperatorRegistry();
        var manifest = CreateManifest(torsionBranch: "nonexistent");

        Assert.Throws<InvalidOperationException>(() =>
            registry.ResolveTorsion(manifest));
    }

    [Fact]
    public void UnregisteredShiab_Throws()
    {
        var registry = new BranchOperatorRegistry();
        var manifest = CreateManifest(shiabBranch: "nonexistent");

        Assert.Throws<InvalidOperationException>(() =>
            registry.ResolveShiab(manifest));
    }

    [Fact]
    public void ValidateCarrierMatch_MatchingTypes_Passes()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        // Both output "curvature-2form" -- should not throw
        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);
    }

    [Fact]
    public void ValidateCarrierMatch_Mismatched_Throws()
    {
        var mesh = CreateMesh();
        var algebra = LieAlgebraFactory.CreateSu2();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        // Create a stub with different carrier type
        var shiab = new MismatchedShiab();

        Assert.Throws<InvalidOperationException>(() =>
            BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab));
    }

    [Fact]
    public void RegisterBiConnection_DefaultStrategy()
    {
        var registry = new BranchOperatorRegistry();
        registry.RegisterBiConnection("simple-a0-omega",
            manifest => new SimpleBiConnectionCpu());

        var manifest = CreateManifest();
        var strategy = registry.ResolveBiConnection(manifest);

        Assert.NotNull(strategy);
        Assert.Equal("simple-a0-omega", strategy.StrategyId);
    }

    private sealed class MismatchedShiab : IShiabBranchOperator
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

        public FieldTensor Evaluate(FieldTensor curvatureF, FieldTensor omega,
            BranchManifest manifest, GeometryContext geometry)
            => throw new NotImplementedException();

        public FieldTensor Linearize(FieldTensor curvatureF, FieldTensor omega,
            FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry)
            => throw new NotImplementedException();
    }
}
