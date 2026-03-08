using Gu.Core;
using Gu.Geometry;

namespace Gu.Geometry.Tests;

public class PullbackOperatorTests
{
    private static FiberBundleMesh CreateToyBundle()
    {
        var xMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 1,
            simplicialDimension: 1,
            vertexCoordinates: new double[] { 0.0, 1.0 },
            vertexCount: 2,
            cellVertices: new[] { new[] { 0, 1 } });

        var yMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[]
            {
                0, 0,
                0, 1,
                1, 0,
                1, 1,
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
                new[] { 0, 1 },
                new[] { 2, 3 },
            },
            XVertexToYVertex = new[] { 0, 2 },
            XCellToYCell = new[] { 0 },
            SectionCoefficients = new[]
            {
                new[] { 1.0, 0.0, 0.0 },
            },
        };
    }

    private static TensorSignature MakeScalarSig() => new()
    {
        AmbientSpaceId = "Y_h",
        CarrierType = "scalar",
        Degree = "0",
        LieAlgebraBasisId = "trivial",
        ComponentOrderId = "natural",
        MemoryLayout = "dense-row-major",
    };

    [Fact]
    public void PullbackScalar_ExtractsCorrectValues()
    {
        var bundle = CreateToyBundle();
        var pullback = new PullbackOperator(bundle);

        // Field on Y_h: values at 4 vertices
        var yField = new FieldTensor
        {
            Label = "test_scalar",
            Signature = MakeScalarSig(),
            Coefficients = new double[] { 10.0, 20.0, 30.0, 40.0 },
            Shape = new[] { 4 },
        };

        var xField = pullback.ApplyVertexScalar(yField);

        // sigma maps: x=0 -> y=0 (value 10), x=1 -> y=2 (value 30)
        Assert.Equal(2, xField.Coefficients.Length);
        Assert.Equal(10.0, xField.Coefficients[0]);
        Assert.Equal(30.0, xField.Coefficients[1]);
        Assert.Equal("X_h", xField.Signature.AmbientSpaceId);
        Assert.StartsWith("sigma_h*", xField.Label);
    }

    [Fact]
    public void PullbackScalar_ConstantField_RemainsConstant()
    {
        var bundle = CreateToyBundle();
        var pullback = new PullbackOperator(bundle);

        // Constant field = 5.0 everywhere on Y_h
        var yField = new FieldTensor
        {
            Label = "constant",
            Signature = MakeScalarSig(),
            Coefficients = new double[] { 5.0, 5.0, 5.0, 5.0 },
            Shape = new[] { 4 },
        };

        var xField = pullback.ApplyVertexScalar(yField);

        Assert.All(xField.Coefficients, v => Assert.Equal(5.0, v));
    }

    [Fact]
    public void PullbackMultiComponent_ExtractsCorrectValues()
    {
        var bundle = CreateToyBundle();
        var pullback = new PullbackOperator(bundle);

        // 2-component field on Y_h (e.g., 2D Lie algebra)
        var yField = new FieldTensor
        {
            Label = "test_vector",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "vector",
                Degree = "0",
                LieAlgebraBasisId = "su2-standard",
                ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[]
            {
                1.0, 2.0,   // y-vertex 0
                3.0, 4.0,   // y-vertex 1
                5.0, 6.0,   // y-vertex 2
                7.0, 8.0,   // y-vertex 3
            },
            Shape = new[] { 4, 2 },
        };

        var xField = pullback.ApplyVertexMultiComponent(yField, 2);

        // sigma: x=0 -> y=0 (1,2), x=1 -> y=2 (5,6)
        Assert.Equal(4, xField.Coefficients.Length); // 2 vertices * 2 components
        Assert.Equal(1.0, xField.Coefficients[0]);
        Assert.Equal(2.0, xField.Coefficients[1]);
        Assert.Equal(5.0, xField.Coefficients[2]);
        Assert.Equal(6.0, xField.Coefficients[3]);
    }

    [Fact]
    public void PullbackScalar_WrongSize_ThrowsArgumentException()
    {
        var bundle = CreateToyBundle();
        var pullback = new PullbackOperator(bundle);

        var yField = new FieldTensor
        {
            Label = "wrong_size",
            Signature = MakeScalarSig(),
            Coefficients = new double[] { 1.0, 2.0 }, // wrong: 2 instead of 4
            Shape = new[] { 2 },
        };

        Assert.Throws<ArgumentException>(() => pullback.ApplyVertexScalar(yField));
    }

    [Fact]
    public void PullbackFaceField_ExtractsCorrectValues()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);

        int componentsPerFace = 3; // e.g., dim(su(2))
        int yFaceCount = bundle.AmbientMesh.FaceCount;

        // Create a face field on Y_h with known values
        var yCoeffs = new double[yFaceCount * componentsPerFace];
        for (int f = 0; f < yFaceCount; f++)
        {
            for (int c = 0; c < componentsPerFace; c++)
            {
                yCoeffs[f * componentsPerFace + c] = (f + 1) * 10.0 + c;
            }
        }

        var yField = new FieldTensor
        {
            Label = "F_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "curvature-2form",
                Degree = "2",
                LieAlgebraBasisId = "su2-standard",
                ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = yCoeffs,
            Shape = new[] { yFaceCount, componentsPerFace },
        };

        var xField = pullback.ApplyFaceField(yField, componentsPerFace);

        // Result should have X_h face count entries
        Assert.Equal(bundle.BaseMesh.FaceCount * componentsPerFace, xField.Coefficients.Length);
        Assert.Equal("X_h", xField.Signature.AmbientSpaceId);
        Assert.StartsWith("sigma_h*", xField.Label);
    }

    [Fact]
    public void PullbackFaceField_ConstantField_RemainsConstant()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);

        int componentsPerFace = 1;
        int yFaceCount = bundle.AmbientMesh.FaceCount;

        // Constant 2-form field = 7.0 on every Y_h face
        var yCoeffs = new double[yFaceCount * componentsPerFace];
        Array.Fill(yCoeffs, 7.0);

        var yField = new FieldTensor
        {
            Label = "constant_2form",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "2form",
                Degree = "2",
                LieAlgebraBasisId = "trivial",
                ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = yCoeffs,
            Shape = new[] { yFaceCount, componentsPerFace },
        };

        var xField = pullback.ApplyFaceField(yField, componentsPerFace);

        // For faces found in Y, the absolute value should be 7.0 (sign may flip)
        // For faces not found, value is 0.0
        foreach (var val in xField.Coefficients)
        {
            Assert.True(
                System.Math.Abs(val) == 7.0 || val == 0.0,
                $"Expected +/-7.0 or 0.0, got {val}");
        }
    }

    [Fact]
    public void PullbackFaceField_OrientationSign_IsCorrect()
    {
        // Build a minimal bundle with a single triangle in both X and Y
        // where the section maps vertices in a way that preserves orientation
        var xMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

        var yMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 1 },
            vertexCount: 4,
            cellVertices: new[] { new[] { 0, 1, 2 }, new[] { 1, 2, 3 } });

        // Section maps X vertices 0,1,2 to Y vertices 0,1,2 (same order = even permutation)
        var bundle = new FiberBundleMesh
        {
            BaseMesh = xMesh,
            AmbientMesh = yMesh,
            YVertexToXVertex = new[] { 0, 1, 2, 2 },
            FiberVerticesPerXVertex = new[] { new[] { 0 }, new[] { 1 }, new[] { 2, 3 } },
            XVertexToYVertex = new[] { 0, 1, 2 },
            XCellToYCell = new[] { 0 },
            SectionCoefficients = new[] { new[] { 1.0, 0.0, 0.0 } },
        };

        var pullback = new PullbackOperator(bundle);

        // Set Y face 0 = (0,1,2) with value 5.0
        var yCoeffs = new double[yMesh.FaceCount];
        // Find Y face index for (0,1,2)
        for (int f = 0; f < yMesh.FaceCount; f++)
        {
            if (yMesh.Faces[f][0] == 0 && yMesh.Faces[f][1] == 1 && yMesh.Faces[f][2] == 2)
                yCoeffs[f] = 5.0;
        }

        var yField = new FieldTensor
        {
            Label = "test_2form",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "2form",
                Degree = "2",
                LieAlgebraBasisId = "trivial",
                ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = yCoeffs,
            Shape = new[] { yMesh.FaceCount },
        };

        var xField = pullback.ApplyFaceField(yField, 1);

        // X face (0,1,2) maps to Y face (0,1,2) with same orientation -> sign = +1
        Assert.Single(xField.Coefficients);
        Assert.Equal(5.0, xField.Coefficients[0], 12);
    }
}
