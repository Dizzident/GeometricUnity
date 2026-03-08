namespace Gu.VulkanViewer.Tests;

public class ScalarFieldVisualizerTests
{
    [Fact]
    public void PrepareVisualization_2DMesh_ProducesValidData()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 0.0, 0.5, 1.0 });
        var visualizer = new ScalarFieldVisualizer();

        var data = visualizer.PrepareVisualization(field, mesh);

        Assert.Equal(3, data.VertexCount);
        Assert.Equal(3 * 3, data.Positions.Length); // 3 vertices * 3 components (xyz)
        Assert.Equal(3 * 4, data.Colors.Length);     // 3 vertices * 4 components (rgba)
        Assert.Equal(1, data.TriangleCount);
        Assert.Equal(3, (int)data.Indices.Length);    // 1 triangle * 3 indices
    }

    [Fact]
    public void PrepareVisualization_2DMesh_PadsZCoordinate()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 0.0, 0.5, 1.0 });
        var visualizer = new ScalarFieldVisualizer();

        var data = visualizer.PrepareVisualization(field, mesh);

        // For a 2D mesh, z coordinates should all be 0.
        for (int v = 0; v < data.VertexCount; v++)
        {
            Assert.Equal(0f, data.Positions[v * 3 + 2]); // z = 0
        }
    }

    [Fact]
    public void PrepareVisualization_3DMesh_ProducesCorrectPositions()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 0, 1, 2, 3 });
        var visualizer = new ScalarFieldVisualizer();

        var data = visualizer.PrepareVisualization(field, mesh);

        Assert.Equal(4, data.VertexCount);
        Assert.Equal(2, data.TriangleCount);

        // Verify first vertex position (0, 0, 0).
        Assert.Equal(0f, data.Positions[0]);
        Assert.Equal(0f, data.Positions[1]);
        Assert.Equal(0f, data.Positions[2]);

        // Verify second vertex position (1, 0, 0).
        Assert.Equal(1f, data.Positions[3]);
        Assert.Equal(0f, data.Positions[4]);
        Assert.Equal(0f, data.Positions[5]);
    }

    [Fact]
    public void PrepareVisualization_ColorsAreInValidRange()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { -5.0, 0.0, 5.0, 10.0 });
        var visualizer = new ScalarFieldVisualizer();

        var data = visualizer.PrepareVisualization(field, mesh);

        for (int i = 0; i < data.Colors.Length; i++)
        {
            Assert.InRange(data.Colors[i], 0f, 1f);
        }
    }

    [Fact]
    public void PrepareVisualization_WithEdgeField_AveragesToVertices()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        // 3 edges in the triangle mesh.
        var field = TestDataHelper.CreateEdgeScalarField(new double[] { 1.0, 2.0, 3.0 });
        var visualizer = new ScalarFieldVisualizer();

        var data = visualizer.PrepareVisualization(field, mesh);

        // Should still produce 3 vertices worth of colors.
        Assert.Equal(3, data.VertexCount);
        Assert.Equal(3 * 4, data.Colors.Length);

        // Colors should be valid.
        for (int i = 0; i < data.Colors.Length; i++)
        {
            Assert.InRange(data.Colors[i], 0f, 1f);
        }
    }

    [Fact]
    public void PrepareVisualization_WithFaceField_AveragesToVertices()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        // 2 faces in the quad mesh.
        var field = TestDataHelper.CreateFaceScalarField(new double[] { 1.0, 2.0 });
        var visualizer = new ScalarFieldVisualizer();

        var data = visualizer.PrepareVisualization(field, mesh);

        Assert.Equal(4, data.VertexCount);
        Assert.Equal(4 * 4, data.Colors.Length);

        for (int i = 0; i < data.Colors.Length; i++)
        {
            Assert.InRange(data.Colors[i], 0f, 1f);
        }
    }

    [Fact]
    public void PrepareVisualization_WithFixedRange_UsesSpecifiedRange()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 0.0, 0.5, 1.0 });

        // Fix range to [0, 10] so all values are in the first 10% of the color range.
        var visualizer = new ScalarFieldVisualizer(fixedMin: 0.0, fixedMax: 10.0);

        var data = visualizer.PrepareVisualization(field, mesh);

        Assert.Equal(0.0, data.ColorMap.MinValue);
        Assert.Equal(10.0, data.ColorMap.MaxValue);
    }

    [Fact]
    public void PrepareVisualization_WithCenterAtZero_ProducesSymmetricRange()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { -2.0, 1.0, 3.0 });

        var visualizer = new ScalarFieldVisualizer(centerAtZero: true);

        var data = visualizer.PrepareVisualization(field, mesh);

        Assert.Equal(-3.0, data.ColorMap.MinValue);
        Assert.Equal(3.0, data.ColorMap.MaxValue);
    }

    [Fact]
    public void PrepareVisualization_ColorMapMetadata_Populated()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 1.0, 2.0, 3.0 });
        var mapper = new ColorMapper("plasma");
        var visualizer = new ScalarFieldVisualizer(mapper);

        var data = visualizer.PrepareVisualization(field, mesh);

        Assert.Equal("plasma", data.ColorMap.ColorSchemeName);
        Assert.Equal(1.0, data.ColorMap.MinValue);
        Assert.Equal(3.0, data.ColorMap.MaxValue);
    }

    [Fact]
    public void PrepareVisualization_TriangleIndices_Are1Based_OnFaces()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 0, 1, 2, 3 });
        var visualizer = new ScalarFieldVisualizer();

        var data = visualizer.PrepareVisualization(field, mesh);

        // Faces: {0,1,2} and {0,2,3}
        Assert.Equal(0u, data.Indices[0]);
        Assert.Equal(1u, data.Indices[1]);
        Assert.Equal(2u, data.Indices[2]);
        Assert.Equal(0u, data.Indices[3]);
        Assert.Equal(2u, data.Indices[4]);
        Assert.Equal(3u, data.Indices[5]);
    }

    [Fact]
    public void PrepareVisualization_ThrowsOnNullField()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var visualizer = new ScalarFieldVisualizer();

        Assert.Throws<ArgumentNullException>(() => visualizer.PrepareVisualization(null!, mesh));
    }

    [Fact]
    public void PrepareVisualization_ThrowsOnNullMesh()
    {
        var field = TestDataHelper.CreateVertexScalarField(new double[] { 1.0 });
        var visualizer = new ScalarFieldVisualizer();

        Assert.Throws<ArgumentNullException>(() => visualizer.PrepareVisualization(field, null!));
    }

    [Fact]
    public void PrepareVisualization_MultiComponentVertexField_UsesNorm()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        // 3 vertices * 2 components = 6 coefficients.
        var field = TestDataHelper.CreateVertexScalarField(
            new double[] { 3.0, 4.0, 0.0, 1.0, 1.0, 0.0 },
            "multi-component");
        var visualizer = new ScalarFieldVisualizer();

        var data = visualizer.PrepareVisualization(field, mesh);

        // Should handle gracefully and produce valid output.
        Assert.Equal(3, data.VertexCount);
        for (int i = 0; i < data.Colors.Length; i++)
        {
            Assert.InRange(data.Colors[i], 0f, 1f);
        }
    }
}
