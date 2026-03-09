using Gu.Branching;
using Gu.Core;
using Gu.Phase2.Branches;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Branches.Tests;

public class BranchVariantOperatorDispatchTests
{
    [Fact]
    public void Resolve_FromVariant_ReturnsOperators()
    {
        var registry = CreateRegistryWithStubs();
        var dispatch = new BranchVariantOperatorDispatch(registry);

        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var resolved = dispatch.Resolve(variant, baseManifest);

        Assert.Equal("v1", resolved.Manifest.BranchId);
        Assert.NotNull(resolved.Torsion);
        Assert.NotNull(resolved.Shiab);
        Assert.NotNull(resolved.BiConnection);
    }

    [Fact]
    public void Resolve_TwoVariants_GetDifferentManifestBranchIds()
    {
        var registry = CreateRegistryWithStubs();
        var dispatch = new BranchVariantOperatorDispatch(registry);

        var v1 = BranchVariantManifestTests.MakeVariant("v1");
        var v2 = BranchVariantManifestTests.MakeVariant("v2");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var r1 = dispatch.Resolve(v1, baseManifest);
        var r2 = dispatch.Resolve(v2, baseManifest);

        Assert.Equal("v1", r1.Manifest.BranchId);
        Assert.Equal("v2", r2.Manifest.BranchId);
    }

    [Fact]
    public void Resolve_UnregisteredTorsion_Throws()
    {
        var registry = new BranchOperatorRegistry();
        registry.RegisterShiab("identity-shiab", _ => new StubShiab());
        registry.RegisterBiConnection("simple-a0-omega", _ => new StubBiConnection());

        var dispatch = new BranchVariantOperatorDispatch(registry);
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        Assert.Throws<InvalidOperationException>(() => dispatch.Resolve(variant, baseManifest));
    }

    [Fact]
    public void Resolve_NullRegistry_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new BranchVariantOperatorDispatch(null!));
    }

    private static BranchOperatorRegistry CreateRegistryWithStubs()
    {
        var registry = new BranchOperatorRegistry();
        registry.RegisterTorsion("local-algebraic", _ => new StubTorsion());
        registry.RegisterShiab("identity-shiab", _ => new StubShiab());
        registry.RegisterBiConnection("simple-a0-omega", _ => new StubBiConnection());
        return registry;
    }

    private sealed class StubTorsion : ITorsionBranchOperator
    {
        public string BranchId => "local-algebraic";
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
            BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
        public FieldTensor Linearize(FieldTensor omega, FieldTensor a0,
            FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
    }

    private sealed class StubShiab : IShiabBranchOperator
    {
        public string BranchId => "identity-shiab";
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
        public FieldTensor Evaluate(FieldTensor curvatureF, FieldTensor omega,
            BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
        public FieldTensor Linearize(FieldTensor curvatureF, FieldTensor omega,
            FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
    }

    private sealed class StubBiConnection : IBiConnectionStrategy
    {
        public string StrategyId => "simple-a0-omega";
        public BiConnectionResult Evaluate(FieldTensor omega, FieldTensor a0,
            BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
        public BiConnectionResult Linearize(FieldTensor omega, FieldTensor a0,
            FieldTensor deltaOmega, BranchManifest manifest, GeometryContext geometry) => throw new NotImplementedException();
    }
}
