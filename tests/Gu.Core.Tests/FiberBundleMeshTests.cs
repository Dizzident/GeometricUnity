using Gu.Geometry;

namespace Gu.Core.Tests;

public class FiberBundleMeshTests
{
    /// <summary>
    /// Creates a toy fiber bundle: 1D base X_h (2 vertices, 1 edge),
    /// 2D ambient Y_h (4 vertices, 2 triangles), with fiber of 2 vertices per X vertex.
    /// </summary>
    private static FiberBundleMesh CreateToyBundle()
    {
        // Base mesh X_h: two points in 1D
        var baseMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 1,
            simplicialDimension: 1,
            vertexCoordinates: new double[] { 0.0, 1.0 },
            vertexCount: 2,
            cellVertices: new[] { new[] { 0, 1 } });

        // Ambient mesh Y_h: 4 vertices in 2D, two triangles
        //  Y vertices: y0=(0,0), y1=(0,1) fiber over x0
        //              y2=(1,0), y3=(1,1) fiber over x1
        var ambientMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 0, 1, 1, 0, 1, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2 }, new[] { 1, 2, 3 } });

        return new FiberBundleMesh
        {
            BaseMesh = baseMesh,
            AmbientMesh = ambientMesh,
            // pi_h: y0->x0, y1->x0, y2->x1, y3->x1
            YVertexToXVertex = new[] { 0, 0, 1, 1 },
            // Fibers: x0 -> {y0, y1}, x1 -> {y2, y3}
            FiberVerticesPerXVertex = new[] { new[] { 0, 1 }, new[] { 2, 3 } },
            // sigma_h: x0 -> y0, x1 -> y2 (pick one per fiber)
            XVertexToYVertex = new[] { 0, 2 },
            // Cell mapping: X cell 0 -> Y cell 0
            XCellToYCell = new[] { 0 },
            // Section coefficients (vertex-aligned)
            SectionCoefficients = new[] { new double[] { 1, 0, 0 } }
        };
    }

    [Fact]
    public void ValidateSection_ReturnsTrueForValidBundle()
    {
        // Per Section 4.2: pi(sigma(x)) == x must hold
        var bundle = CreateToyBundle();
        Assert.True(bundle.ValidateSection());
    }

    [Fact]
    public void ValidateFibers_ReturnsTrueForValidBundle()
    {
        var bundle = CreateToyBundle();
        Assert.True(bundle.ValidateFibers());
    }

    [Fact]
    public void ValidateSection_FailsForInvalidSection()
    {
        var baseMesh = MeshTopologyBuilder.Build(1, 1, new double[] { 0.0, 1.0 }, 2,
            new[] { new[] { 0, 1 } });
        var ambientMesh = MeshTopologyBuilder.Build(2, 2,
            new double[] { 0, 0, 0, 1, 1, 0, 1, 1 }, 4,
            new[] { new[] { 0, 1, 2 }, new[] { 1, 2, 3 } });

        var badBundle = new FiberBundleMesh
        {
            BaseMesh = baseMesh,
            AmbientMesh = ambientMesh,
            YVertexToXVertex = new[] { 0, 0, 1, 1 },
            FiberVerticesPerXVertex = new[] { new[] { 0, 1 }, new[] { 2, 3 } },
            // BAD section: x0 -> y2, but pi(y2) = 1 != 0
            XVertexToYVertex = new[] { 2, 3 },
            XCellToYCell = new[] { 0 },
            SectionCoefficients = new[] { new double[] { 1, 0, 0 } }
        };

        Assert.False(badBundle.ValidateSection());
    }

    [Fact]
    public void ToGeometryContext_ProducesCorrectBindings()
    {
        // Per Section 4.2: pi_h maps Y_h -> X_h, sigma_h maps X_h -> Y_h
        var bundle = CreateToyBundle();
        var ctx = bundle.ToGeometryContext("gauss-2", "lagrange-p1");

        Assert.Equal("projection", ctx.ProjectionBinding.BindingType);
        Assert.Equal("Y_h", ctx.ProjectionBinding.SourceSpace.SpaceId);
        Assert.Equal("X_h", ctx.ProjectionBinding.TargetSpace.SpaceId);

        Assert.Equal("observation", ctx.ObservationBinding.BindingType);
        Assert.Equal("X_h", ctx.ObservationBinding.SourceSpace.SpaceId);
        Assert.Equal("Y_h", ctx.ObservationBinding.TargetSpace.SpaceId);
    }

    [Fact]
    public void ToGeometryContext_HasCorrectDimensions()
    {
        var bundle = CreateToyBundle();
        var ctx = bundle.ToGeometryContext("gauss-2", "lagrange-p1");

        Assert.Equal(1, ctx.BaseSpace.Dimension);    // 1D base
        Assert.Equal(2, ctx.AmbientSpace.Dimension);  // 2D ambient
        Assert.Equal("simplicial", ctx.DiscretizationType);
    }

    [Fact]
    public void ToGeometryContext_PatchContainsMeshMetadata()
    {
        var bundle = CreateToyBundle();
        var ctx = bundle.ToGeometryContext("gauss-2", "lagrange-p1");

        Assert.Single(ctx.Patches);
        var patch = ctx.Patches[0];
        Assert.Equal(2, patch.ElementCount); // 2 triangles in Y_h
        Assert.Equal("simplicial", patch.TopologyType);
        Assert.NotNull(patch.Metadata);
        Assert.Equal("4", patch.Metadata!["vertexCount"]);
    }

    [Fact]
    public void ProjectionIsManytToOne()
    {
        // Multiple Y vertices map to the same X vertex
        var bundle = CreateToyBundle();
        int x0Count = bundle.YVertexToXVertex.Count(x => x == 0);
        Assert.Equal(2, x0Count); // 2 Y vertices in fiber over x0
    }
}
