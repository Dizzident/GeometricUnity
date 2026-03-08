using Gu.Core;
using Gu.Core.Serialization;
using Gu.Geometry;

namespace Gu.Geometry.Tests;

public class ToyGeometryTests
{
    [Fact]
    public void Toy2D_ValidFiberBundle()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();

        Assert.True(bundle.ValidateSection(), "Section validation failed");
        Assert.True(bundle.ValidateFibers(), "Fiber validation failed");

        // X_h: 5 vertices, 4 cells, dimX=2
        Assert.Equal(5, bundle.BaseMesh.VertexCount);
        Assert.Equal(4, bundle.BaseMesh.CellCount);
        Assert.Equal(2, bundle.BaseMesh.EmbeddingDimension);

        // Y_h: 15 vertices (5*3), dimY=5
        Assert.Equal(15, bundle.AmbientMesh.VertexCount);
        Assert.Equal(5, bundle.AmbientMesh.EmbeddingDimension);
        Assert.Equal(2, bundle.AmbientMesh.SimplicialDimension);
    }

    [Fact]
    public void Toy2D_FiberSizeIsThree()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();

        for (int x = 0; x < bundle.BaseMesh.VertexCount; x++)
        {
            Assert.Equal(3, bundle.FiberVerticesPerXVertex[x].Length);
        }
    }

    [Fact]
    public void Toy2D_PullbackWorks()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);

        // Create a linear scalar field on Y_h
        var yCoeffs = new double[bundle.AmbientMesh.VertexCount];
        for (int i = 0; i < yCoeffs.Length; i++)
            yCoeffs[i] = i * 1.0;

        var yField = new FieldTensor
        {
            Label = "test_linear",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "scalar",
                Degree = "0",
                LieAlgebraBasisId = "trivial",
                ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = yCoeffs,
            Shape = new[] { bundle.AmbientMesh.VertexCount },
        };

        var xField = pullback.ApplyVertexScalar(yField);

        Assert.Equal(bundle.BaseMesh.VertexCount, xField.Coefficients.Length);
        // sigma selects fiber point 0 (index 0, 3, 6, 9, 12 for each X vertex)
        Assert.Equal(0.0, xField.Coefficients[0]);  // y-vertex 0
        Assert.Equal(3.0, xField.Coefficients[1]);  // y-vertex 3
        Assert.Equal(6.0, xField.Coefficients[2]);  // y-vertex 6
        Assert.Equal(9.0, xField.Coefficients[3]);  // y-vertex 9
        Assert.Equal(12.0, xField.Coefficients[4]); // y-vertex 12
    }

    [Fact]
    public void Toy2D_GeometryContextSerializes()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var ctx = bundle.ToGeometryContext("simplex-2d-centroid", "P1-simplex-2d");

        var json = GuJsonDefaults.Serialize(ctx);
        Assert.Contains("\"baseSpace\"", json);
        Assert.Contains("\"ambientSpace\"", json);
        Assert.Contains("\"projectionBinding\"", json);
        Assert.Contains("\"observationBinding\"", json);

        var deserialized = GuJsonDefaults.Deserialize<GeometryContext>(json);
        Assert.NotNull(deserialized);
        Assert.Equal("X_h", deserialized.BaseSpace.SpaceId);
        Assert.Equal(2, deserialized.BaseSpace.Dimension);
        Assert.Equal("Y_h", deserialized.AmbientSpace.SpaceId);
        Assert.Equal(5, deserialized.AmbientSpace.Dimension);
    }

    [Fact]
    public void Toy3D_ValidFiberBundle()
    {
        var bundle = ToyGeometryFactory.CreateToy3D();

        Assert.True(bundle.ValidateSection(), "Section validation failed");
        Assert.True(bundle.ValidateFibers(), "Fiber validation failed");

        // X_h: 5 vertices, 2 cells, dimX=3
        Assert.Equal(5, bundle.BaseMesh.VertexCount);
        Assert.Equal(2, bundle.BaseMesh.CellCount);
        Assert.Equal(3, bundle.BaseMesh.EmbeddingDimension);
        Assert.Equal(3, bundle.BaseMesh.SimplicialDimension);

        // Y_h: 10 vertices (5*2), dimY=9
        Assert.Equal(10, bundle.AmbientMesh.VertexCount);
        Assert.Equal(9, bundle.AmbientMesh.EmbeddingDimension);
        Assert.Equal(3, bundle.AmbientMesh.SimplicialDimension);
    }

    [Fact]
    public void Toy3D_HasCorrectSubsimplexCounts()
    {
        var bundle = ToyGeometryFactory.CreateToy3D();

        // Y_h tetrahedra should have edges and faces
        Assert.True(bundle.AmbientMesh.EdgeCount > 0);
        Assert.True(bundle.AmbientMesh.FaceCount > 0);
    }
}
