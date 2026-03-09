using Gu.Geometry;

namespace Gu.Geometry.Tests;

public class SimplicialMeshGeneratorTests
{
    [Theory]
    [InlineData(1, 1, 2)]       // 1x1 grid = 2 faces
    [InlineData(2, 2, 8)]       // 2x2 grid = 8 faces
    [InlineData(5, 5, 50)]      // 5x5 grid = 50 faces
    [InlineData(10, 10, 200)]   // 10x10 grid = 200 faces
    public void CreateUniform2D_RowsCols_CorrectFaceCount(int rows, int cols, int expectedFaces)
    {
        var mesh = SimplicialMeshGenerator.CreateUniform2D(rows, cols);

        Assert.Equal(expectedFaces, mesh.FaceCount);
        Assert.Equal(2, mesh.EmbeddingDimension);
        Assert.Equal(2, mesh.SimplicialDimension);
        Assert.Equal((rows + 1) * (cols + 1), mesh.VertexCount);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(100)]
    [InlineData(1_000)]
    [InlineData(10_000)]
    public void CreateUniform2D_TargetFaces_AtLeastTarget(int targetFaces)
    {
        var mesh = SimplicialMeshGenerator.CreateUniform2D(targetFaces);

        Assert.True(mesh.FaceCount >= targetFaces,
            $"Got {mesh.FaceCount} faces, expected >= {targetFaces}");
    }

    [Fact]
    public void CreateUniform2D_TopologyIsConsistent()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform2D(4, 4);

        // Every face should have exactly 3 boundary edges
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            Assert.Equal(3, mesh.FaceBoundaryEdges[f].Length);
            Assert.Equal(3, mesh.FaceBoundaryOrientations[f].Length);
        }

        // Every edge index in face boundaries must be valid
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            foreach (int e in mesh.FaceBoundaryEdges[f])
            {
                Assert.True(e >= 0 && e < mesh.EdgeCount,
                    $"Face {f} references invalid edge {e}");
            }
        }

        // Every cell should have 3 vertices (triangles)
        for (int c = 0; c < mesh.CellCount; c++)
        {
            Assert.Equal(3, mesh.CellVertices[c].Length);
        }
    }

    [Theory]
    [InlineData(1, 5)]     // 1x1x1 = 5 tets
    [InlineData(2, 40)]    // 2x2x2 = 40 tets
    public void CreateUniform3D_CorrectCellCount(int n, int expectedCells)
    {
        var mesh = SimplicialMeshGenerator.CreateUniform3D(n);

        Assert.Equal(expectedCells, mesh.CellCount);
        Assert.Equal(3, mesh.EmbeddingDimension);
        Assert.Equal(3, mesh.SimplicialDimension);
        Assert.Equal((n + 1) * (n + 1) * (n + 1), mesh.VertexCount);
    }

    [Fact]
    public void CreateUniform3D_TopologyIsConsistent()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform3D(2);

        // Every cell should have 4 vertices (tetrahedra)
        for (int c = 0; c < mesh.CellCount; c++)
        {
            Assert.Equal(4, mesh.CellVertices[c].Length);
        }

        // Every face should have exactly 3 boundary edges
        for (int f = 0; f < mesh.FaceCount; f++)
        {
            Assert.Equal(3, mesh.FaceBoundaryEdges[f].Length);
        }

        // Edges should connect valid vertices
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            Assert.Equal(2, mesh.Edges[e].Length);
            Assert.True(mesh.Edges[e][0] < mesh.Edges[e][1],
                $"Edge {e} not in canonical order: [{mesh.Edges[e][0]}, {mesh.Edges[e][1]}]");
        }
    }

    [Fact]
    public void CreateUniform2D_LargeMesh_100K_Succeeds()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform2D(100_000);

        Assert.True(mesh.FaceCount >= 100_000);
        Assert.True(mesh.EdgeCount > 0);
        Assert.True(mesh.VertexCount > 0);

        // Verify Euler characteristic for a planar triangulation:
        // V - E + F = 1 (for a simply connected domain without boundary correction)
        // Actually for a grid: V - E + F = 1 (Euler for disk topology)
        int euler = mesh.VertexCount - mesh.EdgeCount + mesh.FaceCount;
        Assert.Equal(1, euler);
    }

    [Fact]
    public void CreateUniform2D_EulerCharacteristic()
    {
        // For any planar triangulation of a disk: V - E + F = 1
        foreach (int size in new[] { 2, 10, 50, 200 })
        {
            var mesh = SimplicialMeshGenerator.CreateUniform2D(size);
            int euler = mesh.VertexCount - mesh.EdgeCount + mesh.FaceCount;
            Assert.Equal(1, euler);
        }
    }

    [Fact]
    public void CreateUniform2D_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SimplicialMeshGenerator.CreateUniform2D(0, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => SimplicialMeshGenerator.CreateUniform2D(1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SimplicialMeshGenerator.CreateUniform2D(1));
    }

    [Fact]
    public void CreateUniform3D_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => SimplicialMeshGenerator.CreateUniform3D(0));
    }
}
