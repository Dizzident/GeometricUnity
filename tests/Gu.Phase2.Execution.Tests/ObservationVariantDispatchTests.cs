using Gu.Core;
using Gu.Observation;
using Gu.Phase2.Branches;
using Gu.Phase2.Branches.Tests;
using Gu.Phase2.Semantics;

namespace Gu.Phase2.Execution.Tests;

public class ObservationVariantDispatchTests
{
    [Fact]
    public void Resolve_DefaultTransforms_ReturnsEmptyList()
    {
        var dispatch = new ObservationVariantDispatch();
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var config = dispatch.Resolve(variant, baseManifest);

        Assert.Empty(config.Transforms);
    }

    [Fact]
    public void Resolve_DefaultNormalization_ReturnsDimensionless()
    {
        var dispatch = new ObservationVariantDispatch();
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var config = dispatch.Resolve(variant, baseManifest);

        Assert.IsType<DimensionlessNormalizationPolicy>(config.Normalization);
    }

    [Fact]
    public void Resolve_DefaultRequests_ReturnsFourStandardObservables()
    {
        var dispatch = new ObservationVariantDispatch();
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var config = dispatch.Resolve(variant, baseManifest);

        Assert.Equal(4, config.Requests.Count);
        Assert.Contains(config.Requests, r => r.ObservableId == "curvature");
        Assert.Contains(config.Requests, r => r.ObservableId == "torsion");
        Assert.Contains(config.Requests, r => r.ObservableId == "shiab");
        Assert.Contains(config.Requests, r => r.ObservableId == "residual");
    }

    [Fact]
    public void Resolve_RegisteredTransforms_ReturnsCustomList()
    {
        var dispatch = new ObservationVariantDispatch();
        var customTransform = new StubTransform("custom-obs");
        dispatch.RegisterTransforms("sigma-pullback",
            _ => new IDerivedObservableTransform[] { customTransform });

        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var config = dispatch.Resolve(variant, baseManifest);

        Assert.Single(config.Transforms);
        Assert.Equal("custom-obs", config.Transforms[0].ObservableId);
    }

    [Fact]
    public void Resolve_RegisteredRequests_ReturnsCustomList()
    {
        var dispatch = new ObservationVariantDispatch();
        dispatch.RegisterRequests("sigma-pullback", _ => new[]
        {
            new ObservableRequest { ObservableId = "custom-field", OutputType = OutputType.Quantitative },
        });

        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var config = dispatch.Resolve(variant, baseManifest);

        Assert.Single(config.Requests);
        Assert.Equal("custom-field", config.Requests[0].ObservableId);
    }

    [Fact]
    public void Resolve_PreservesVariantIds()
    {
        var dispatch = new ObservationVariantDispatch();
        var variant = BranchVariantManifestTests.MakeVariant("v1");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var config = dispatch.Resolve(variant, baseManifest);

        Assert.Equal("sigma-pullback", config.ObservationVariantId);
        Assert.Equal("standard-extraction", config.ExtractionVariantId);
    }

    [Fact]
    public void Resolve_TwoVariants_ResolveIndependently()
    {
        var dispatch = new ObservationVariantDispatch();
        dispatch.RegisterRequests("sigma-pullback", _ => new[]
        {
            new ObservableRequest { ObservableId = "sigma-obs", OutputType = OutputType.Quantitative },
        });
        dispatch.RegisterRequests("alt-pullback", _ => new[]
        {
            new ObservableRequest { ObservableId = "alt-obs", OutputType = OutputType.SemiQuantitative },
        });

        var v1 = MakeVariantWithObs("v1", "sigma-pullback");
        var v2 = MakeVariantWithObs("v2", "alt-pullback");
        var baseManifest = BranchVariantManifestTests.MakeBaseManifest();

        var c1 = dispatch.Resolve(v1, baseManifest);
        var c2 = dispatch.Resolve(v2, baseManifest);

        Assert.Equal("sigma-obs", c1.Requests[0].ObservableId);
        Assert.Equal("alt-obs", c2.Requests[0].ObservableId);
    }

    private static BranchVariantManifest MakeVariantWithObs(string id, string obsVariant)
    {
        return new BranchVariantManifest
        {
            Id = id,
            ParentFamilyId = "fam-1",
            A0Variant = "flat-A0",
            BiConnectionVariant = "simple-a0-omega",
            TorsionVariant = "local-algebraic",
            ShiabVariant = "identity-shiab",
            ObservationVariant = obsVariant,
            ExtractionVariant = "standard-extraction",
            GaugeVariant = "penalty",
            RegularityVariant = "standard-regularity",
            PairingVariant = "pairing-trace",
            ExpectedClaimCeiling = "branch-local-numerical",
        };
    }

    private sealed class StubTransform : IDerivedObservableTransform
    {
        public StubTransform(string observableId) { ObservableId = observableId; }
        public string ObservableId { get; }
        public OutputType OutputType => OutputType.ExactStructural;
        public string TransformId => $"stub-{ObservableId}";
        public double[] Compute(PulledBackFields pulledBackFields, ObservableRequest request)
            => new double[] { 1.0 };
    }
}
