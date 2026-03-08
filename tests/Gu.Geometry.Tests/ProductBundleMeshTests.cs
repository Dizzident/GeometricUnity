using Gu.Geometry;

namespace Gu.Geometry.Tests;

public class ProductBundleMeshTests
{
    /// <summary>
    /// Triangle (2-simplex) x Edge (1-simplex) product.
    /// Freudenthal decomposition of a single prism should yield 3 tetrahedra.
    /// (Number of interleavings of 2 B-steps + 1 F-step = C(3,1) = 3.)
    /// </summary>
    [Fact]
    public void TriangleTimesEdge_Produces3Tetrahedra()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();

        Assert.Equal(2, product.BaseMesh.EmbeddingDimension);
        Assert.Equal(1, product.FiberMesh.EmbeddingDimension);
        Assert.Equal(3, product.EmbeddingDimension);
        Assert.Equal(3, product.ProductSimplicialDimension); // 2 + 1

        var yMesh = product.ToSimplicialMesh();

        // 3 base vertices * 2 fiber vertices = 6 product vertices
        Assert.Equal(6, yMesh.VertexCount);

        // 1 base cell * 1 fiber cell * C(3,1) = 3 tetrahedra
        Assert.Equal(3, yMesh.CellCount);

        // Each cell should be a tetrahedron (4 vertices)
        Assert.All(yMesh.CellVertices, cell => Assert.Equal(4, cell.Length));

        Assert.Equal(3, yMesh.EmbeddingDimension);
        Assert.Equal(3, yMesh.SimplicialDimension);
    }

    /// <summary>
    /// Product vertex numbering: (base_i, fiber_j) -> base_i * fiberVertCount + fiber_j.
    /// </summary>
    [Fact]
    public void ProductVertexNumbering_IsCorrect()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();

        Assert.Equal(0, product.ProductVertexIndex(0, 0));
        Assert.Equal(1, product.ProductVertexIndex(0, 1));
        Assert.Equal(2, product.ProductVertexIndex(1, 0));
        Assert.Equal(3, product.ProductVertexIndex(1, 1));
        Assert.Equal(4, product.ProductVertexIndex(2, 0));
        Assert.Equal(5, product.ProductVertexIndex(2, 1));
    }

    /// <summary>
    /// Base and fiber vertex extraction from product indices.
    /// </summary>
    [Fact]
    public void BaseAndFiberExtraction_IsCorrect()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();

        for (int bv = 0; bv < product.BaseMesh.VertexCount; bv++)
        {
            for (int fv = 0; fv < product.FiberMesh.VertexCount; fv++)
            {
                int pv = product.ProductVertexIndex(bv, fv);
                Assert.Equal(bv, product.BaseVertexOf(pv));
                Assert.Equal(fv, product.FiberVertexOf(pv));
            }
        }
    }

    /// <summary>
    /// Product vertex coordinates are [baseCoords | fiberCoords].
    /// </summary>
    [Fact]
    public void ProductCoordinates_AreConcatenated()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();
        var yMesh = product.ToSimplicialMesh();

        // Vertex (base=1, fiber=0): base coords = (1,0), fiber coords = (0.0)
        int pv = product.ProductVertexIndex(1, 0);
        var coords = yMesh.GetVertexCoordinates(pv);

        Assert.Equal(3, coords.Length);
        Assert.Equal(1.0, coords[0], 12); // base x
        Assert.Equal(0.0, coords[1], 12); // base y
        Assert.Equal(0.0, coords[2], 12); // fiber

        // Vertex (base=2, fiber=1): base coords = (0,1), fiber coords = (1.0)
        int pv2 = product.ProductVertexIndex(2, 1);
        var coords2 = yMesh.GetVertexCoordinates(pv2);

        Assert.Equal(0.0, coords2[0], 12);
        Assert.Equal(1.0, coords2[1], 12);
        Assert.Equal(1.0, coords2[2], 12);
    }

    /// <summary>
    /// Edge classification: horizontal, vertical, mixed.
    /// </summary>
    [Fact]
    public void EdgeClassification_IsCorrect()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();

        // Horizontal: same fiber index, different base
        Assert.Equal(ProductElementType.Horizontal, product.ClassifyEdge(
            product.ProductVertexIndex(0, 0), product.ProductVertexIndex(1, 0)));

        // Vertical: same base index, different fiber
        Assert.Equal(ProductElementType.Vertical, product.ClassifyEdge(
            product.ProductVertexIndex(0, 0), product.ProductVertexIndex(0, 1)));

        // Mixed: different base AND fiber
        Assert.Equal(ProductElementType.Mixed, product.ClassifyEdge(
            product.ProductVertexIndex(0, 0), product.ProductVertexIndex(1, 1)));
    }

    /// <summary>
    /// Face classification: HH, VV, Mixed.
    /// </summary>
    [Fact]
    public void FaceClassification_IsCorrect()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();

        // All-horizontal face: three different base vertices, same fiber
        Assert.Equal(ProductElementType.Horizontal, product.ClassifyFace(
            product.ProductVertexIndex(0, 0),
            product.ProductVertexIndex(1, 0),
            product.ProductVertexIndex(2, 0)));

        // Mixed face: at least one mixed edge
        Assert.Equal(ProductElementType.Mixed, product.ClassifyFace(
            product.ProductVertexIndex(0, 0),
            product.ProductVertexIndex(1, 0),
            product.ProductVertexIndex(1, 1)));
    }

    /// <summary>
    /// DecompositionId is "freudenthal".
    /// </summary>
    [Fact]
    public void DecompositionId_IsFreudenthal()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();
        Assert.Equal("freudenthal", product.DecompositionId);
    }

    /// <summary>
    /// ToFiberBundleMesh produces valid section and projection.
    /// </summary>
    [Fact]
    public void ToFiberBundleMesh_ValidSection()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();
        var bundle = product.ToFiberBundleMesh();

        Assert.True(bundle.ValidateSection());
        Assert.True(bundle.ValidateFibers());

        Assert.Equal(product.BaseMesh.VertexCount, bundle.BaseMesh.VertexCount);
        Assert.Equal(product.ProductVertexCount, bundle.AmbientMesh.VertexCount);

        // sigma selects fiber vertex 0
        for (int bv = 0; bv < product.BaseMesh.VertexCount; bv++)
        {
            Assert.Equal(product.ProductVertexIndex(bv, 0), bundle.XVertexToYVertex[bv]);
        }
    }

    /// <summary>
    /// ToFiberBundleMesh projection is correct (many-to-one).
    /// </summary>
    [Fact]
    public void ToFiberBundleMesh_ProjectionIsCorrect()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();
        var bundle = product.ToFiberBundleMesh();

        for (int pv = 0; pv < product.ProductVertexCount; pv++)
        {
            int expectedBase = product.BaseVertexOf(pv);
            Assert.Equal(expectedBase, bundle.YVertexToXVertex[pv]);
        }
    }

    /// <summary>
    /// Freudenthal simplex count: product of d1-simplex and d2-simplex
    /// produces C(d1+d2, d1) simplices per product cell pair.
    /// For edge x edge (1+1): C(2,1) = 2 triangles.
    /// </summary>
    [Fact]
    public void EdgeTimesEdge_Produces2Triangles()
    {
        var xMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 1,
            simplicialDimension: 1,
            vertexCoordinates: new double[] { 0.0, 1.0 },
            vertexCount: 2,
            cellVertices: new[] { new[] { 0, 1 } });

        var fMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 1,
            simplicialDimension: 1,
            vertexCoordinates: new double[] { 0.0, 1.0 },
            vertexCount: 2,
            cellVertices: new[] { new[] { 0, 1 } });

        var product = new ProductBundleMesh { BaseMesh = xMesh, FiberMesh = fMesh };

        var yMesh = product.ToSimplicialMesh();

        Assert.Equal(4, yMesh.VertexCount);      // 2 * 2
        Assert.Equal(2, yMesh.CellCount);         // C(2,1) = 2 triangles
        Assert.Equal(2, yMesh.SimplicialDimension); // 1 + 1
        Assert.All(yMesh.CellVertices, cell => Assert.Equal(3, cell.Length));
    }

    /// <summary>
    /// All vertices of each Freudenthal simplex are distinct.
    /// </summary>
    [Fact]
    public void FreudenthalSimplices_HaveDistinctVertices()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();
        var yMesh = product.ToSimplicialMesh();

        foreach (var cell in yMesh.CellVertices)
        {
            Assert.Equal(cell.Length, cell.Distinct().Count());
        }
    }

    /// <summary>
    /// All Freudenthal simplices share vertex (b0,f0) -> (b_last, f_last) endpoints,
    /// starting from the product of first vertices and ending at product of last vertices.
    /// </summary>
    [Fact]
    public void FreudenthalSimplices_ShareStartAndEnd()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();
        var yMesh = product.ToSimplicialMesh();

        // For triangle {0,1,2} x edge {0,1}, all 3 tets should contain
        // vertex (b0=0,f0=0) = 0 and vertex (b2=2,f1=1) = 5
        int startVert = product.ProductVertexIndex(0, 0);
        int endVert = product.ProductVertexIndex(2, 1);

        foreach (var cell in yMesh.CellVertices)
        {
            Assert.Contains(startVert, cell);
            Assert.Contains(endVert, cell);
        }
    }

    /// <summary>
    /// ToFiberBundleMesh can be used with PullbackOperator.
    /// </summary>
    [Fact]
    public void ProductBundle_WorksWithPullbackOperator()
    {
        var product = ToyGeometryFactory.CreateToyProduct2D();
        var bundle = product.ToFiberBundleMesh();
        var pullback = new PullbackOperator(bundle);

        // Create a scalar field on Y_h
        var yCoeffs = new double[bundle.AmbientMesh.VertexCount];
        for (int i = 0; i < yCoeffs.Length; i++)
            yCoeffs[i] = i * 1.5;

        var yField = new Core.FieldTensor
        {
            Label = "test_scalar",
            Signature = new Core.TensorSignature
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
        Assert.Equal("X_h", xField.Signature.AmbientSpaceId);

        // sigma selects fiber vertex 0, so x=i maps to product vertex i*2
        for (int bv = 0; bv < bundle.BaseMesh.VertexCount; bv++)
        {
            int expectedPv = product.ProductVertexIndex(bv, 0);
            Assert.Equal(expectedPv * 1.5, xField.Coefficients[bv], 12);
        }
    }
}
