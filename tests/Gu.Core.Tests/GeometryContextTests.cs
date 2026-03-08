using Gu.Core;
using Gu.Core.Serialization;

namespace Gu.Core.Tests;

public class GeometryContextTests
{
    private static GeometryContext CreateSample()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4, Label = "base_X_h" };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14, Label = "ambient_Y_h" };

        return new GeometryContext
        {
            BaseSpace = baseSpace,
            AmbientSpace = ambientSpace,
            DiscretizationType = "simplicial",
            QuadratureRuleId = "gauss-legendre-2",
            BasisFamilyId = "lagrange-p1",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = ambientSpace,
                TargetSpace = baseSpace,
                MappingStrategy = "fiber-projection"
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = baseSpace,
                TargetSpace = ambientSpace,
                MappingStrategy = "section-pullback"
            },
            Patches = new[]
            {
                new PatchInfo { PatchId = "patch-0", ElementCount = 64, TopologyType = "simplicial" },
                new PatchInfo { PatchId = "patch-1", ElementCount = 128, TopologyType = "simplicial" }
            }
        };
    }

    [Fact]
    public void RoundTrip_AllFieldsPreserved()
    {
        var original = CreateSample();
        var json = GuJsonDefaults.Serialize(original);
        var deserialized = GuJsonDefaults.Deserialize<GeometryContext>(json);

        Assert.NotNull(deserialized);
        Assert.Equal(original.BaseSpace.SpaceId, deserialized.BaseSpace.SpaceId);
        Assert.Equal(original.AmbientSpace.SpaceId, deserialized.AmbientSpace.SpaceId);
        Assert.Equal(original.DiscretizationType, deserialized.DiscretizationType);
        Assert.Equal(original.QuadratureRuleId, deserialized.QuadratureRuleId);
        Assert.Equal(original.BasisFamilyId, deserialized.BasisFamilyId);
        Assert.Equal(2, deserialized.Patches.Count);
    }

    [Fact]
    public void ProjectionBinding_MapsYToX()
    {
        // Per Section 4.2: pi_h maps discrete Y_h geometry to discrete X_h geometry
        var ctx = CreateSample();
        Assert.Equal("projection", ctx.ProjectionBinding.BindingType);
        Assert.Equal("Y_h", ctx.ProjectionBinding.SourceSpace.SpaceId);
        Assert.Equal("X_h", ctx.ProjectionBinding.TargetSpace.SpaceId);
    }

    [Fact]
    public void ObservationBinding_MapsXToY()
    {
        // Per Section 4.2: sigma_h is the discrete observation map
        var ctx = CreateSample();
        Assert.Equal("observation", ctx.ObservationBinding.BindingType);
        Assert.Equal("X_h", ctx.ObservationBinding.SourceSpace.SpaceId);
        Assert.Equal("Y_h", ctx.ObservationBinding.TargetSpace.SpaceId);
    }

    [Fact]
    public void GeometryPayload_IsOptional()
    {
        var ctx = CreateSample();
        Assert.Null(ctx.GeometryPayload);
    }

    [Fact]
    public void GeometryPayload_RoundTrips()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 };
        var ctx = new GeometryContext
        {
            BaseSpace = baseSpace,
            AmbientSpace = ambientSpace,
            DiscretizationType = "structured",
            QuadratureRuleId = "gauss-2",
            BasisFamilyId = "lagrange-p1",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = ambientSpace,
                TargetSpace = baseSpace
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = baseSpace,
                TargetSpace = ambientSpace
            },
            Patches = Array.Empty<PatchInfo>(),
            GeometryPayload = new byte[] { 1, 2, 3, 4, 5 }
        };

        var json = GuJsonDefaults.Serialize(ctx);
        var deserialized = GuJsonDefaults.Deserialize<GeometryContext>(json);

        Assert.NotNull(deserialized);
        Assert.NotNull(deserialized.GeometryPayload);
        Assert.Equal(ctx.GeometryPayload, deserialized.GeometryPayload);
    }
}
