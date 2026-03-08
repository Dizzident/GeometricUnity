using Gu.Geometry;

namespace Gu.Geometry.Tests;

public class SimplicialMeshTests
{
    /// <summary>
    /// Build a single triangle in 2D: vertices (0,0), (1,0), (0,1).
    /// Verify edge and face extraction.
    /// </summary>
    [Fact]
    public void SingleTriangle_ExtractsCorrectTopology()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

        Assert.Equal(1, mesh.CellCount);
        Assert.Equal(3, mesh.EdgeCount);
        Assert.Equal(1, mesh.FaceCount);
        Assert.Equal(3, mesh.VertexCount);

        // The single face should be the cell itself (for 2D simplex, face = cell)
        Assert.Equal(new[] { 0, 1, 2 }, mesh.Faces[0]);

        // Edges: {0,1}, {0,2}, {1,2} in canonical order
        var edgeSet = mesh.Edges.Select(e => (e[0], e[1])).ToHashSet();
        Assert.Contains((0, 1), edgeSet);
        Assert.Contains((0, 2), edgeSet);
        Assert.Contains((1, 2), edgeSet);
    }

    /// <summary>
    /// Build two adjacent triangles sharing an edge.
    /// Verify shared edge is not duplicated.
    /// </summary>
    [Fact]
    public void TwoTriangles_SharedEdgeNotDuplicated()
    {
        // Triangle 1: (0,0), (1,0), (0,1)  [vertices 0,1,2]
        // Triangle 2: (1,0), (1,1), (0,1)  [vertices 1,3,2]
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[]
            {
                new[] { 0, 1, 2 },
                new[] { 1, 3, 2 },
            });

        Assert.Equal(2, mesh.CellCount);
        Assert.Equal(4, mesh.VertexCount);
        // Edges: {0,1}, {0,2}, {1,2}, {1,3}, {2,3} = 5 unique edges
        Assert.Equal(5, mesh.EdgeCount);
        // Faces = 2 (one per triangle)
        Assert.Equal(2, mesh.FaceCount);

        // Both cells should reference their 3 edges
        Assert.Equal(3, mesh.CellEdges[0].Length);
        Assert.Equal(3, mesh.CellEdges[1].Length);

        // The shared edge {1,2} should appear in both cells
        var sharedEdgeIdx = Array.FindIndex(mesh.Edges, e => e[0] == 1 && e[1] == 2);
        Assert.True(sharedEdgeIdx >= 0);
        Assert.Contains(sharedEdgeIdx, mesh.CellEdges[0]);
        Assert.Contains(sharedEdgeIdx, mesh.CellEdges[1]);
    }

    /// <summary>
    /// Build a single tetrahedron in 3D.
    /// Should have 6 edges and 4 faces.
    /// </summary>
    [Fact]
    public void SingleTetrahedron_CorrectSubsimplexCounts()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: new double[]
            {
                0, 0, 0,  // vertex 0
                1, 0, 0,  // vertex 1
                0, 1, 0,  // vertex 2
                0, 0, 1,  // vertex 3
            },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2, 3 } });

        // C(4,2) = 6 edges
        Assert.Equal(6, mesh.EdgeCount);
        // C(4,3) = 4 faces
        Assert.Equal(4, mesh.FaceCount);
        // 1 cell
        Assert.Equal(1, mesh.CellCount);
        // Cell should reference all 6 edges and 4 faces
        Assert.Equal(6, mesh.CellEdges[0].Length);
        Assert.Equal(4, mesh.CellFaces[0].Length);
    }

    /// <summary>
    /// Face boundary edges should have 3 edges each with correct orientations.
    /// d[v0,v1,v2] = [v1,v2] - [v0,v2] + [v0,v1]
    /// </summary>
    [Fact]
    public void FaceBoundary_HasThreeEdgesWithOrientations()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

        Assert.Single(mesh.FaceBoundaryEdges);
        Assert.Equal(3, mesh.FaceBoundaryEdges[0].Length);
        Assert.Equal(3, mesh.FaceBoundaryOrientations[0].Length);

        // Orientations should be +1, -1, +1
        Assert.Equal(+1, mesh.FaceBoundaryOrientations[0][0]);
        Assert.Equal(-1, mesh.FaceBoundaryOrientations[0][1]);
        Assert.Equal(+1, mesh.FaceBoundaryOrientations[0][2]);
    }

    /// <summary>
    /// Verify GetVertexCoordinates returns correct values.
    /// </summary>
    [Fact]
    public void GetVertexCoordinates_ReturnsCorrectSlice()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: new double[]
            {
                0, 0, 0,
                1, 0, 0,
                0, 1, 0,
                0, 0, 1,
            },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2, 3 } });

        var coords2 = mesh.GetVertexCoordinates(2);
        Assert.Equal(3, coords2.Length);
        Assert.Equal(0.0, coords2[0]);
        Assert.Equal(1.0, coords2[1]);
        Assert.Equal(0.0, coords2[2]);
    }

    /// <summary>
    /// 1D mesh should have edges but no faces.
    /// </summary>
    [Fact]
    public void OneDimensional_HasEdgesNoFaces()
    {
        // Two line segments: [0,1] and [1,2] on the real line
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 1,
            simplicialDimension: 1,
            vertexCoordinates: new double[] { 0, 0.5, 1.0 },
            vertexCount: 3,
            cellVertices: new[]
            {
                new[] { 0, 1 },
                new[] { 1, 2 },
            });

        Assert.Equal(2, mesh.CellCount);
        Assert.Equal(2, mesh.EdgeCount); // Each cell IS an edge
        Assert.Equal(0, mesh.FaceCount); // 1D has no 2-faces
    }

    /// <summary>
    /// High-dimensional test: 5D simplex (pentatope) in 5D embedding.
    /// Should have C(6,2)=15 edges and C(6,3)=20 faces.
    /// </summary>
    [Fact]
    public void Pentatope_In5D_CorrectSubsimplexCounts()
    {
        // 5-simplex has 6 vertices
        var coords = new double[6 * 5];
        // Vertex 0 at origin, vertex i at e_{i-1}
        for (int i = 1; i <= 5; i++)
            coords[i * 5 + (i - 1)] = 1.0;

        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 5,
            simplicialDimension: 5,
            vertexCoordinates: coords,
            vertexCount: 6,
            cellVertices: new[] { new[] { 0, 1, 2, 3, 4, 5 } });

        Assert.Equal(15, mesh.EdgeCount);  // C(6,2)
        Assert.Equal(20, mesh.FaceCount);  // C(6,3)
        Assert.Equal(1, mesh.CellCount);
    }

    /// <summary>
    /// Validation: cell vertex count must match simplicial dimension + 1.
    /// </summary>
    [Fact]
    public void InvalidCellSize_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2, 3 } })); // 4 vertices for dim=2 simplex
    }

    /// <summary>
    /// Vertex-edge incidence: each vertex of a triangle touches 2 edges.
    /// Signs follow convention: +1 if vertex is first endpoint, -1 if second.
    /// </summary>
    [Fact]
    public void SingleTriangle_VertexEdgeIncidence()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

        // Each vertex touches exactly 2 edges in a triangle
        Assert.Equal(3, mesh.VertexEdges.Length);
        Assert.Equal(2, mesh.VertexEdges[0].Length);
        Assert.Equal(2, mesh.VertexEdges[1].Length);
        Assert.Equal(2, mesh.VertexEdges[2].Length);

        // Verify orientation signs: for edge {v0, v1} with v0 < v1,
        // v0 gets +1, v1 gets -1
        for (int v = 0; v < mesh.VertexCount; v++)
        {
            for (int i = 0; i < mesh.VertexEdges[v].Length; i++)
            {
                int edgeIdx = mesh.VertexEdges[v][i];
                int expectedSign = (mesh.Edges[edgeIdx][0] == v) ? +1 : -1;
                Assert.Equal(expectedSign, mesh.VertexEdgeOrientations[v][i]);
            }
        }
    }

    /// <summary>
    /// Vertex-edge incidence: interior vertex of two triangles touches more edges.
    /// </summary>
    [Fact]
    public void TwoTriangles_SharedVertex_CorrectIncidence()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[]
            {
                new[] { 0, 1, 2 },
                new[] { 1, 3, 2 },
            });

        // Vertex 1 is shared: edges {0,1}, {1,2}, {1,3} = 3 incident edges
        Assert.Equal(3, mesh.VertexEdges[1].Length);

        // Vertex 0 is a corner: edges {0,1}, {0,2} = 2 incident edges
        Assert.Equal(2, mesh.VertexEdges[0].Length);

        // All edges incident to each vertex are unique
        for (int v = 0; v < mesh.VertexCount; v++)
        {
            Assert.Equal(mesh.VertexEdges[v].Length, mesh.VertexEdges[v].Distinct().Count());
        }
    }

    /// <summary>
    /// Codifferential d* consistency: for each edge, the sum of its contributions
    /// to its two endpoint vertices should cancel (opposite signs).
    /// </summary>
    [Fact]
    public void VertexEdgeOrientations_OppositeAtEndpoints()
    {
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

        for (int ei = 0; ei < mesh.EdgeCount; ei++)
        {
            int v0 = mesh.Edges[ei][0], v1 = mesh.Edges[ei][1];

            // Find the sign for this edge at v0
            int idx0 = Array.IndexOf(mesh.VertexEdges[v0], ei);
            int sign0 = mesh.VertexEdgeOrientations[v0][idx0];

            // Find the sign for this edge at v1
            int idx1 = Array.IndexOf(mesh.VertexEdges[v1], ei);
            int sign1 = mesh.VertexEdgeOrientations[v1][idx1];

            // Signs should be opposite
            Assert.Equal(+1, sign0);
            Assert.Equal(-1, sign1);
        }
    }
}
