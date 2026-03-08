namespace Gu.VulkanViewer.Tests;

public class MeshExporterTests
{
    private VisualizationData CreateSampleVisualizationData()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 0.0, 0.5, 1.0 });
        var visualizer = new ScalarFieldVisualizer();
        return visualizer.PrepareVisualization(field, mesh);
    }

    [Fact]
    public void ToObj_ContainsHeader()
    {
        var data = CreateSampleVisualizationData();

        string obj = MeshExporter.ToObj(data);

        Assert.Contains("# Geometric Unity", obj);
        Assert.Contains($"# Vertices: {data.VertexCount}", obj);
        Assert.Contains($"# Triangles: {data.TriangleCount}", obj);
    }

    [Fact]
    public void ToObj_ContainsCorrectVertexCount()
    {
        var data = CreateSampleVisualizationData();

        string obj = MeshExporter.ToObj(data);
        var lines = obj.Split('\n');

        int vertexLines = lines.Count(l => l.StartsWith("v "));
        Assert.Equal(data.VertexCount, vertexLines);
    }

    [Fact]
    public void ToObj_ContainsCorrectFaceCount()
    {
        var data = CreateSampleVisualizationData();

        string obj = MeshExporter.ToObj(data);
        var lines = obj.Split('\n');

        int faceLines = lines.Count(l => l.StartsWith("f "));
        Assert.Equal(data.TriangleCount, faceLines);
    }

    [Fact]
    public void ToObj_FacesAreOneIndexed()
    {
        var data = CreateSampleVisualizationData();

        string obj = MeshExporter.ToObj(data);
        var lines = obj.Split('\n');

        var faceLines = lines.Where(l => l.StartsWith("f ")).ToList();
        Assert.Single(faceLines);

        // Indices should be 1-based (OBJ format).
        string faceLine = faceLines[0].Trim();
        string[] parts = faceLine.Split(' ');
        Assert.Equal("f", parts[0]);

        foreach (string idx in parts.Skip(1))
        {
            int index = int.Parse(idx);
            Assert.True(index >= 1, "OBJ face indices must be 1-based.");
        }
    }

    [Fact]
    public void ToObj_VertexPositionsMatch()
    {
        var data = CreateSampleVisualizationData();

        string obj = MeshExporter.ToObj(data);
        var lines = obj.Split('\n');

        var vertexLines = lines.Where(l => l.StartsWith("v ")).ToList();

        // First vertex in the 2D triangle mesh is (0, 0) -> (0, 0, 0).
        string[] parts = vertexLines[0].Trim().Split(' ');
        Assert.Equal("v", parts[0]);
        Assert.Equal(4, parts.Length); // "v x y z"
    }

    [Fact]
    public void ColorsToCs_ContainsHeader()
    {
        var data = CreateSampleVisualizationData();

        string csv = MeshExporter.ColorsToCs(data);

        Assert.StartsWith("vertex_index,R,G,B,A", csv);
    }

    [Fact]
    public void ColorsToCs_ContainsCorrectRowCount()
    {
        var data = CreateSampleVisualizationData();

        string csv = MeshExporter.ColorsToCs(data);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Header + one row per vertex.
        Assert.Equal(data.VertexCount + 1, lines.Length);
    }

    [Fact]
    public void ColorsToCs_ValuesAreInRange()
    {
        var data = CreateSampleVisualizationData();

        string csv = MeshExporter.ColorsToCs(data);
        var lines = csv.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Skip header.
        foreach (string line in lines.Skip(1))
        {
            string[] cols = line.Split(',');
            Assert.Equal(5, cols.Length); // index, R, G, B, A

            for (int c = 1; c <= 4; c++)
            {
                float val = float.Parse(cols[c], System.Globalization.CultureInfo.InvariantCulture);
                Assert.InRange(val, 0f, 1f);
            }
        }
    }

    [Fact]
    public void ToPly_ContainsHeader()
    {
        var data = CreateSampleVisualizationData();

        string ply = MeshExporter.ToPly(data);

        Assert.StartsWith("ply", ply);
        Assert.Contains("format ascii 1.0", ply);
        Assert.Contains($"element vertex {data.VertexCount}", ply);
        Assert.Contains($"element face {data.TriangleCount}", ply);
        Assert.Contains("end_header", ply);
    }

    [Fact]
    public void ToPly_VertexDataIncludesColors()
    {
        var data = CreateSampleVisualizationData();

        string ply = MeshExporter.ToPly(data);

        // After end_header, vertex lines should have 7 values (x y z r g b a).
        var lines = ply.Split('\n');
        int headerEnd = Array.FindIndex(lines, l => l.Trim() == "end_header");
        Assert.True(headerEnd > 0);

        for (int i = 0; i < data.VertexCount; i++)
        {
            string vertexLine = lines[headerEnd + 1 + i].Trim();
            string[] parts = vertexLine.Split(' ');
            Assert.Equal(7, parts.Length); // x y z r g b a
        }
    }

    [Fact]
    public void ToPly_FaceDataIsValid()
    {
        var data = CreateSampleVisualizationData();

        string ply = MeshExporter.ToPly(data);
        var lines = ply.Split('\n');
        int headerEnd = Array.FindIndex(lines, l => l.Trim() == "end_header");

        // Face lines start after vertex lines.
        int faceStart = headerEnd + 1 + data.VertexCount;
        for (int t = 0; t < data.TriangleCount; t++)
        {
            string faceLine = lines[faceStart + t].Trim();
            Assert.StartsWith("3 ", faceLine); // Triangle (3 vertices).
            string[] parts = faceLine.Split(' ');
            Assert.Equal(4, parts.Length); // "3 v0 v1 v2"
        }
    }

    [Fact]
    public void WriteFiles_CreatesFiles()
    {
        var data = CreateSampleVisualizationData();
        string tempDir = Path.Combine(Path.GetTempPath(), $"gu_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            string objPath = Path.Combine(tempDir, "test.obj");
            string csvPath = Path.Combine(tempDir, "test_colors.csv");

            MeshExporter.WriteFiles(data, objPath, csvPath);

            Assert.True(File.Exists(objPath));
            Assert.True(File.Exists(csvPath));

            // Verify non-empty content.
            Assert.True(new FileInfo(objPath).Length > 0);
            Assert.True(new FileInfo(csvPath).Length > 0);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ToObj_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => MeshExporter.ToObj(null!));
    }

    [Fact]
    public void ToPly_ThrowsOnNull()
    {
        Assert.Throws<ArgumentNullException>(() => MeshExporter.ToPly(null!));
    }

    [Fact]
    public void ToObj_QuadMesh_HasTwoFaces()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 0, 1, 2, 3 });
        var visualizer = new ScalarFieldVisualizer();
        var data = visualizer.PrepareVisualization(field, mesh);

        string obj = MeshExporter.ToObj(data);
        var lines = obj.Split('\n');

        int faceLines = lines.Count(l => l.StartsWith("f "));
        Assert.Equal(2, faceLines);

        int vertexLines = lines.Count(l => l.StartsWith("v "));
        Assert.Equal(4, vertexLines);
    }
}
