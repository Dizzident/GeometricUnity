using Gu.Core;
using Gu.Geometry;

namespace Gu.Geometry.Tests;

public class FiberBundleMeshTests
{
    /// <summary>
    /// Creates a toy fiber bundle: X is a single edge in 1D, Y has 2 fiber points per X vertex.
    /// dimX=1, dimY=2 (X embedded in first coord, fiber in second).
    /// </summary>
    private static FiberBundleMesh CreateToyBundle()
    {
        // X_h: single edge [0,1] in 1D
        var xMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 1,
            simplicialDimension: 1,
            vertexCoordinates: new double[] { 0.0, 1.0 },
            vertexCount: 2,
            cellVertices: new[] { new[] { 0, 1 } });

        // Y_h: 4 vertices in 2D forming triangles
        // Fiber over x=0: y vertices 0 (0,0) and 1 (0,1)
        // Fiber over x=1: y vertices 2 (1,0) and 3 (1,1)
        var yMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[]
            {
                0, 0,  // y-vertex 0 -> x-vertex 0
                0, 1,  // y-vertex 1 -> x-vertex 0
                1, 0,  // y-vertex 2 -> x-vertex 1
                1, 1,  // y-vertex 3 -> x-vertex 1
            },
            vertexCount: 4,
            cellVertices: new[]
            {
                new[] { 0, 1, 2 },
                new[] { 1, 2, 3 },
            });

        return new FiberBundleMesh
        {
            BaseMesh = xMesh,
            AmbientMesh = yMesh,
            YVertexToXVertex = new[] { 0, 0, 1, 1 },
            FiberVerticesPerXVertex = new[]
            {
                new[] { 0, 1 },  // fiber over x=0
                new[] { 2, 3 },  // fiber over x=1
            },
            // sigma selects first fiber point per X vertex
            XVertexToYVertex = new[] { 0, 2 },
            XCellToYCell = new[] { 0 },
            SectionCoefficients = new[]
            {
                new[] { 1.0, 0.0, 0.0 },  // vertex 0 in Y cell 0
            },
        };
    }

    [Fact]
    public void ValidateSection_ReturnsTrueForValidSection()
    {
        var bundle = CreateToyBundle();
        Assert.True(bundle.ValidateSection());
    }

    [Fact]
    public void ValidateFibers_ReturnsTrueForValidFibers()
    {
        var bundle = CreateToyBundle();
        Assert.True(bundle.ValidateFibers());
    }

    [Fact]
    public void ValidateSection_ReturnsFalseForBrokenSection()
    {
        var bundle = CreateToyBundle();
        // Create a broken section: x-vertex 0 maps to y-vertex 2 (which is in x-vertex 1's fiber)
        var broken = new FiberBundleMesh
        {
            BaseMesh = bundle.BaseMesh,
            AmbientMesh = bundle.AmbientMesh,
            YVertexToXVertex = bundle.YVertexToXVertex,
            FiberVerticesPerXVertex = bundle.FiberVerticesPerXVertex,
            XVertexToYVertex = new[] { 2, 0 }, // BROKEN: pi(sigma(0)) = 1 != 0
            XCellToYCell = bundle.XCellToYCell,
            SectionCoefficients = bundle.SectionCoefficients,
        };

        Assert.False(broken.ValidateSection());
    }

    [Fact]
    public void ManyToOne_MultipleYVerticesPerXVertex()
    {
        var bundle = CreateToyBundle();

        // Each X vertex has 2 Y vertices in its fiber
        Assert.Equal(2, bundle.FiberVerticesPerXVertex[0].Length);
        Assert.Equal(2, bundle.FiberVerticesPerXVertex[1].Length);

        // All Y vertices in fiber over x=0 project back to x=0
        foreach (int y in bundle.FiberVerticesPerXVertex[0])
            Assert.Equal(0, bundle.YVertexToXVertex[y]);

        // All Y vertices in fiber over x=1 project back to x=1
        foreach (int y in bundle.FiberVerticesPerXVertex[1])
            Assert.Equal(1, bundle.YVertexToXVertex[y]);
    }

    [Fact]
    public void ToGeometryContext_ProducesValidContext()
    {
        var bundle = CreateToyBundle();
        var ctx = bundle.ToGeometryContext("centroid-1d", "P1-1d");

        Assert.Equal("X_h", ctx.BaseSpace.SpaceId);
        Assert.Equal(1, ctx.BaseSpace.Dimension);
        Assert.Equal("Y_h", ctx.AmbientSpace.SpaceId);
        Assert.Equal(2, ctx.AmbientSpace.Dimension);
        Assert.Equal("simplicial", ctx.DiscretizationType);
        Assert.Equal("projection", ctx.ProjectionBinding.BindingType);
        Assert.Equal("observation", ctx.ObservationBinding.BindingType);
        Assert.Single(ctx.Patches);
        Assert.Equal(2, ctx.Patches[0].ElementCount); // 2 Y cells
    }
}
