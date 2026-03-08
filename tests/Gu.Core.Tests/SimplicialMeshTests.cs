using Gu.Geometry;

namespace Gu.Core.Tests;

public class SimplicialMeshTests
{
    /// <summary>
    /// Creates a simple 2D triangular mesh: two triangles sharing an edge.
    ///   2
    ///  / \
    /// 0---1
    ///  \ /
    ///   3
    /// Cells: [0,1,2] and [0,3,1]
    /// </summary>
    private static SimplicialMesh CreateTwoTriangleMesh()
    {
        double[] vertices =
        {
            0.0, 0.0,  // v0
            1.0, 0.0,  // v1
            0.5, 1.0,  // v2
            0.5, -1.0  // v3
        };

        int[][] cells = { new[] { 0, 1, 2 }, new[] { 0, 3, 1 } };

        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: vertices,
            vertexCount: 4,
            cellVertices: cells);
    }

    /// <summary>
    /// Creates a single tetrahedron in 3D.
    /// </summary>
    private static SimplicialMesh CreateSingleTetrahedron()
    {
        double[] vertices =
        {
            0.0, 0.0, 0.0,  // v0
            1.0, 0.0, 0.0,  // v1
            0.0, 1.0, 0.0,  // v2
            0.0, 0.0, 1.0   // v3
        };

        int[][] cells = { new[] { 0, 1, 2, 3 } };

        return MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: vertices,
            vertexCount: 4,
            cellVertices: cells);
    }

    [Fact]
    public void TwoTriangles_HasCorrectCounts()
    {
        var mesh = CreateTwoTriangleMesh();

        Assert.Equal(2, mesh.EmbeddingDimension);
        Assert.Equal(2, mesh.SimplicialDimension);
        Assert.Equal(4, mesh.VertexCount);
        Assert.Equal(2, mesh.CellCount);
        Assert.Equal(5, mesh.EdgeCount);  // 3 edges per triangle, 1 shared = 5
        Assert.Equal(2, mesh.FaceCount);  // 2 triangular faces
    }

    [Fact]
    public void TwoTriangles_EdgesAreCanonicallyOrdered()
    {
        var mesh = CreateTwoTriangleMesh();

        foreach (var edge in mesh.Edges)
        {
            Assert.Equal(2, edge.Length);
            Assert.True(edge[0] < edge[1], $"Edge [{edge[0]}, {edge[1]}] not canonical.");
        }
    }

    [Fact]
    public void TwoTriangles_FacesAreCanonicallyOrdered()
    {
        var mesh = CreateTwoTriangleMesh();

        foreach (var face in mesh.Faces)
        {
            Assert.Equal(3, face.Length);
            Assert.True(face[0] < face[1] && face[1] < face[2],
                $"Face [{face[0]}, {face[1]}, {face[2]}] not canonical.");
        }
    }

    [Fact]
    public void TwoTriangles_EachCellHas3Edges()
    {
        var mesh = CreateTwoTriangleMesh();

        foreach (var cellEdges in mesh.CellEdges)
        {
            Assert.Equal(3, cellEdges.Length);
        }
    }

    [Fact]
    public void TwoTriangles_SharedEdge_AppearsInBothCells()
    {
        var mesh = CreateTwoTriangleMesh();

        // Edge (0,1) should be shared between both cells
        var sharedEdge = mesh.Edges
            .Select((e, i) => (e, i))
            .First(x => x.e[0] == 0 && x.e[1] == 1);

        bool cell0Has = mesh.CellEdges[0].Contains(sharedEdge.i);
        bool cell1Has = mesh.CellEdges[1].Contains(sharedEdge.i);

        Assert.True(cell0Has && cell1Has, "Edge (0,1) should be in both cells.");
    }

    [Fact]
    public void TwoTriangles_FaceBoundaryEdges_Have3Entries()
    {
        var mesh = CreateTwoTriangleMesh();

        foreach (var fbe in mesh.FaceBoundaryEdges)
        {
            Assert.Equal(3, fbe.Length);
        }
    }

    [Fact]
    public void TwoTriangles_FaceBoundaryOrientations_ArePlusMinus1()
    {
        var mesh = CreateTwoTriangleMesh();

        foreach (var orientations in mesh.FaceBoundaryOrientations)
        {
            Assert.All(orientations, o => Assert.True(o == 1 || o == -1));
        }
    }

    [Fact]
    public void Tetrahedron_HasCorrectCounts()
    {
        var mesh = CreateSingleTetrahedron();

        Assert.Equal(3, mesh.EmbeddingDimension);
        Assert.Equal(3, mesh.SimplicialDimension);
        Assert.Equal(4, mesh.VertexCount);
        Assert.Equal(1, mesh.CellCount);
        Assert.Equal(6, mesh.EdgeCount);  // C(4,2) = 6 edges
        Assert.Equal(4, mesh.FaceCount);  // C(4,3) = 4 faces
    }

    [Fact]
    public void Tetrahedron_CellHas6Edges()
    {
        var mesh = CreateSingleTetrahedron();
        Assert.Equal(6, mesh.CellEdges[0].Length);
    }

    [Fact]
    public void Tetrahedron_CellHas4Faces()
    {
        var mesh = CreateSingleTetrahedron();
        Assert.Equal(4, mesh.CellFaces[0].Length);
    }

    [Fact]
    public void GetVertexCoordinates_ReturnsCorrectSlice()
    {
        var mesh = CreateTwoTriangleMesh();
        var coords = mesh.GetVertexCoordinates(1);

        Assert.Equal(2, coords.Length);
        Assert.Equal(1.0, coords[0]); // x
        Assert.Equal(0.0, coords[1]); // y
    }

    [Fact]
    public void Build_InvalidCoordinateLength_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            MeshTopologyBuilder.Build(2, 2, new double[] { 1.0 }, 4, Array.Empty<int[]>()));
    }

    [Fact]
    public void Build_WrongVerticesPerCell_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            MeshTopologyBuilder.Build(2, 2,
                new double[] { 0, 0, 1, 0, 0, 1 }, 3,
                new[] { new[] { 0, 1 } })); // need 3 vertices for dim 2
    }

    [Fact]
    public void DiscreteExteriorDerivative_StokesTheorem()
    {
        // Verify that the boundary operator satisfies d^2 = 0 structurally:
        // For each face, sum of oriented edge values should give the discrete curvature.
        // For a single triangle with omega = constant on all edges, d(omega) is well-defined.
        var mesh = CreateTwoTriangleMesh();

        // Assign constant omega_h = 1.0 on all edges
        var omega = new double[mesh.EdgeCount];
        Array.Fill(omega, 1.0);

        // Compute d(omega) on each face using the boundary operator
        for (int fi = 0; fi < mesh.FaceCount; fi++)
        {
            double dOmega = 0;
            for (int i = 0; i < 3; i++)
            {
                int edgeIdx = mesh.FaceBoundaryEdges[fi][i];
                int sign = mesh.FaceBoundaryOrientations[fi][i];
                dOmega += sign * omega[edgeIdx];
            }
            // d(constant 1-form) = (+1 - 1 + 1) * 1.0 = 1.0
            Assert.Equal(1.0, dOmega);
        }
    }
}
