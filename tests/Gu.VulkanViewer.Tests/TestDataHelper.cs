using Gu.Core;
using Gu.Geometry;
using Gu.Solvers;

namespace Gu.VulkanViewer.Tests;

/// <summary>
/// Shared test data builders for VulkanViewer tests.
/// </summary>
internal static class TestDataHelper
{
    /// <summary>
    /// Creates a minimal 2D triangle mesh: 3 vertices forming one triangle.
    /// Embedding dimension = 2, simplicial dimension = 2.
    /// </summary>
    public static SimplicialMesh CreateTriangleMesh2D()
    {
        // Edges: e0={0,1}, e1={0,2}, e2={1,2}
        // Vertex 0 touches e0 (+1, first endpoint) and e1 (+1, first endpoint)
        // Vertex 1 touches e0 (-1, second endpoint) and e2 (+1, first endpoint)
        // Vertex 2 touches e1 (-1, second endpoint) and e2 (-1, second endpoint)
        return new SimplicialMesh
        {
            EmbeddingDimension = 2,
            SimplicialDimension = 2,
            VertexCount = 3,
            VertexCoordinates = new double[] { 0, 0, 1, 0, 0.5, 1 },
            CellVertices = new[] { new[] { 0, 1, 2 } },
            Edges = new[] { new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 2 } },
            Faces = new[] { new[] { 0, 1, 2 } },
            CellEdges = new[] { new[] { 0, 1, 2 } },
            CellFaces = new[] { new[] { 0 } },
            FaceBoundaryEdges = new[] { new[] { 0, 2, 1 } },
            FaceBoundaryOrientations = new[] { new[] { 1, 1, -1 } },
            VertexEdges = new[] { new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 2 } },
            VertexEdgeOrientations = new[] { new[] { 1, 1 }, new[] { -1, 1 }, new[] { -1, -1 } },
        };
    }

    /// <summary>
    /// Creates a 3D quad mesh (4 vertices, 2 triangles forming a square).
    /// Embedding dimension = 3, simplicial dimension = 2.
    /// </summary>
    public static SimplicialMesh CreateQuadMesh3D()
    {
        // Edges: e0={0,1}, e1={0,2}, e2={0,3}, e3={1,2}, e4={2,3}
        // Vertex 0 touches e0(+1), e1(+1), e2(+1)
        // Vertex 1 touches e0(-1), e3(+1)
        // Vertex 2 touches e1(-1), e3(-1), e4(+1)
        // Vertex 3 touches e2(-1), e4(-1)
        return new SimplicialMesh
        {
            EmbeddingDimension = 3,
            SimplicialDimension = 2,
            VertexCount = 4,
            VertexCoordinates = new double[]
            {
                0, 0, 0,   // v0
                1, 0, 0,   // v1
                1, 1, 0,   // v2
                0, 1, 0,   // v3
            },
            CellVertices = new[]
            {
                new[] { 0, 1, 2 },
                new[] { 0, 2, 3 },
            },
            Edges = new[]
            {
                new[] { 0, 1 }, // e0
                new[] { 0, 2 }, // e1
                new[] { 0, 3 }, // e2
                new[] { 1, 2 }, // e3
                new[] { 2, 3 }, // e4
            },
            Faces = new[]
            {
                new[] { 0, 1, 2 }, // f0
                new[] { 0, 2, 3 }, // f1
            },
            CellEdges = new[]
            {
                new[] { 0, 1, 3 },
                new[] { 1, 2, 4 },
            },
            CellFaces = new[]
            {
                new[] { 0 },
                new[] { 1 },
            },
            FaceBoundaryEdges = new[]
            {
                new[] { 0, 3, 1 },
                new[] { 1, 4, 2 },
            },
            FaceBoundaryOrientations = new[]
            {
                new[] { 1, 1, -1 },
                new[] { 1, 1, -1 },
            },
            VertexEdges = new[]
            {
                new[] { 0, 1, 2 },   // v0 -> e0, e1, e2
                new[] { 0, 3 },       // v1 -> e0, e3
                new[] { 1, 3, 4 },   // v2 -> e1, e3, e4
                new[] { 2, 4 },       // v3 -> e2, e4
            },
            VertexEdgeOrientations = new[]
            {
                new[] { 1, 1, 1 },    // v0: first endpoint in all
                new[] { -1, 1 },       // v1: second in e0, first in e3
                new[] { -1, -1, 1 },  // v2: second in e1, second in e3, first in e4
                new[] { -1, -1 },      // v3: second in e2, second in e4
            },
        };
    }

    /// <summary>
    /// Creates a vertex-based scalar field with the specified values.
    /// </summary>
    public static FieldTensor CreateVertexScalarField(double[] values, string label = "test_scalar")
    {
        return new FieldTensor
        {
            Label = label,
            Coefficients = values,
            Shape = new[] { values.Length },
            Signature = CreateScalarSignature(),
        };
    }

    /// <summary>
    /// Creates an edge-based scalar field.
    /// </summary>
    public static FieldTensor CreateEdgeScalarField(double[] values, string label = "test_edge")
    {
        return new FieldTensor
        {
            Label = label,
            Coefficients = values,
            Shape = new[] { values.Length },
            Signature = CreateEdgeSignature(),
        };
    }

    /// <summary>
    /// Creates a face-based scalar field.
    /// </summary>
    public static FieldTensor CreateFaceScalarField(double[] values, string label = "test_face")
    {
        return new FieldTensor
        {
            Label = label,
            Coefficients = values,
            Shape = new[] { values.Length },
            Signature = CreateFaceSignature(),
        };
    }

    /// <summary>
    /// Creates sample convergence records for testing.
    /// </summary>
    public static List<ConvergenceRecord> CreateConvergenceHistory(int iterations = 10)
    {
        var history = new List<ConvergenceRecord>();
        for (int i = 0; i < iterations; i++)
        {
            double decay = Math.Exp(-0.5 * i);
            history.Add(new ConvergenceRecord
            {
                Iteration = i,
                Objective = 100.0 * decay * decay,
                ResidualNorm = 10.0 * decay,
                GradientNorm = 5.0 * decay,
                GaugeViolation = 0.1 * decay,
                StepNorm = 0.5 * decay,
                StepSize = 0.01 * (1.0 + 0.1 * i),
            });
        }

        return history;
    }

    /// <summary>
    /// Creates a minimal DerivedState with the specified mesh dimensions.
    /// </summary>
    public static DerivedState CreateDerivedState(int faceCount, int edgeCount)
    {
        return new DerivedState
        {
            CurvatureF = CreateFaceScalarField(
                Enumerable.Range(0, faceCount).Select(i => (double)i * 0.1).ToArray(),
                "F_h"),
            TorsionT = CreateFaceScalarField(
                Enumerable.Range(0, faceCount).Select(i => (double)i * 0.05).ToArray(),
                "T_h"),
            ShiabS = CreateFaceScalarField(
                Enumerable.Range(0, faceCount).Select(i => (double)i * 0.03).ToArray(),
                "S_h"),
            ResidualUpsilon = CreateFaceScalarField(
                Enumerable.Range(0, faceCount).Select(i => (double)(i - faceCount / 2) * 0.02).ToArray(),
                "Upsilon_h"),
        };
    }

    private static TensorSignature CreateScalarSignature()
    {
        return new TensorSignature
        {
            AmbientSpaceId = "test-space",
            CarrierType = "scalar-0form",
            Degree = "0",
            LieAlgebraBasisId = "trivial",
            ComponentOrderId = "canonical",
            MemoryLayout = "dense-row-major",
        };
    }

    private static TensorSignature CreateEdgeSignature()
    {
        return new TensorSignature
        {
            AmbientSpaceId = "test-space",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "trivial",
            ComponentOrderId = "canonical",
            MemoryLayout = "dense-row-major",
        };
    }

    private static TensorSignature CreateFaceSignature()
    {
        return new TensorSignature
        {
            AmbientSpaceId = "test-space",
            CarrierType = "curvature-2form",
            Degree = "2",
            LieAlgebraBasisId = "trivial",
            ComponentOrderId = "canonical",
            MemoryLayout = "dense-row-major",
        };
    }
}
