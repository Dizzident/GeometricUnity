namespace Gu.Geometry;

/// <summary>
/// Classification of edges and faces in a product mesh Y_h = X_h x F_h.
/// </summary>
public enum ProductElementType
{
    /// <summary>Both endpoints share the same fiber index (edge along base directions).</summary>
    Horizontal,

    /// <summary>Both endpoints share the same base index (edge along fiber directions).</summary>
    Vertical,

    /// <summary>Endpoints differ in both base and fiber indices.</summary>
    Mixed,
}

/// <summary>
/// Represents Y_h as a structured product mesh: Y_h = X_h x F_h.
/// Avoids unstructured high-dimensional simplicial complexes by exploiting
/// the fiber bundle product structure.
///
/// Product vertex numbering: Y vertex (xi, fj) = xi * FiberMesh.VertexCount + fj.
/// Decomposition to simplices uses Freudenthal (staircase) triangulation.
/// </summary>
public sealed class ProductBundleMesh
{
    /// <summary>The base space mesh X_h.</summary>
    public required SimplicialMesh BaseMesh { get; init; }

    /// <summary>The fiber mesh F_h.</summary>
    public required SimplicialMesh FiberMesh { get; init; }

    /// <summary>
    /// Identifier for the simplex decomposition method.
    /// Serialized in artifacts for reproducibility.
    /// </summary>
    public string DecompositionId => "freudenthal";

    /// <summary>Total embedding dimension of Y_h (dimX + dimF).</summary>
    public int EmbeddingDimension => BaseMesh.EmbeddingDimension + FiberMesh.EmbeddingDimension;

    /// <summary>Total simplicial dimension of product cells (dimX_simp + dimF_simp).</summary>
    public int ProductSimplicialDimension => BaseMesh.SimplicialDimension + FiberMesh.SimplicialDimension;

    /// <summary>Total number of product vertices.</summary>
    public int ProductVertexCount => BaseMesh.VertexCount * FiberMesh.VertexCount;

    /// <summary>
    /// Maps a (baseVertex, fiberVertex) pair to the product vertex index.
    /// </summary>
    public int ProductVertexIndex(int baseVertex, int fiberVertex)
        => baseVertex * FiberMesh.VertexCount + fiberVertex;

    /// <summary>
    /// Extracts the base vertex index from a product vertex index.
    /// </summary>
    public int BaseVertexOf(int productVertex)
        => productVertex / FiberMesh.VertexCount;

    /// <summary>
    /// Extracts the fiber vertex index from a product vertex index.
    /// </summary>
    public int FiberVertexOf(int productVertex)
        => productVertex % FiberMesh.VertexCount;

    /// <summary>
    /// Classifies a product edge by whether it is horizontal, vertical, or mixed.
    /// </summary>
    public ProductElementType ClassifyEdge(int v0, int v1)
    {
        int b0 = BaseVertexOf(v0), b1 = BaseVertexOf(v1);
        int f0 = FiberVertexOf(v0), f1 = FiberVertexOf(v1);

        if (f0 == f1) return ProductElementType.Horizontal;
        if (b0 == b1) return ProductElementType.Vertical;
        return ProductElementType.Mixed;
    }

    /// <summary>
    /// Classifies a product face (3 vertices) by its edge types.
    /// HH = all edges horizontal, VV = all edges vertical, Mixed = any mixed edge.
    /// </summary>
    public ProductElementType ClassifyFace(int v0, int v1, int v2)
    {
        var e01 = ClassifyEdge(v0, v1);
        var e02 = ClassifyEdge(v0, v2);
        var e12 = ClassifyEdge(v1, v2);

        if (e01 == ProductElementType.Horizontal && e02 == ProductElementType.Horizontal && e12 == ProductElementType.Horizontal)
            return ProductElementType.Horizontal;
        if (e01 == ProductElementType.Vertical && e02 == ProductElementType.Vertical && e12 == ProductElementType.Vertical)
            return ProductElementType.Vertical;
        return ProductElementType.Mixed;
    }

    /// <summary>
    /// Decomposes the product mesh into a SimplicialMesh using Freudenthal triangulation.
    /// Each product cell (baseCell x fiberCell) is decomposed into simplices of dimension
    /// (baseDim + fiberDim) by enumerating all staircase paths through the product lattice.
    /// The decomposition is deterministic and canonical.
    /// </summary>
    public SimplicialMesh ToSimplicialMesh()
    {
        int dimY = EmbeddingDimension;
        int dimBase = BaseMesh.EmbeddingDimension;
        int dimFiber = FiberMesh.EmbeddingDimension;
        int fiberVertCount = FiberMesh.VertexCount;
        int totalVertCount = ProductVertexCount;

        // Build product vertex coordinates: [baseCoords | fiberCoords]
        var coords = new double[totalVertCount * dimY];
        for (int bv = 0; bv < BaseMesh.VertexCount; bv++)
        {
            for (int fv = 0; fv < fiberVertCount; fv++)
            {
                int pv = ProductVertexIndex(bv, fv);
                int offset = pv * dimY;

                // Copy base coordinates
                for (int d = 0; d < dimBase; d++)
                    coords[offset + d] = BaseMesh.VertexCoordinates[bv * dimBase + d];

                // Copy fiber coordinates
                for (int d = 0; d < dimFiber; d++)
                    coords[offset + dimBase + d] = FiberMesh.VertexCoordinates[fv * dimFiber + d];
            }
        }

        // Decompose each (baseCell x fiberCell) pair using Freudenthal triangulation
        int baseSimpDim = BaseMesh.SimplicialDimension;
        int fiberSimpDim = FiberMesh.SimplicialDimension;
        int productSimpDim = baseSimpDim + fiberSimpDim;

        var allCells = new List<int[]>();

        for (int bc = 0; bc < BaseMesh.CellCount; bc++)
        {
            var baseVerts = BaseMesh.CellVertices[bc]; // baseSimpDim + 1 vertices

            for (int fc = 0; fc < FiberMesh.CellCount; fc++)
            {
                var fiberVerts = FiberMesh.CellVertices[fc]; // fiberSimpDim + 1 vertices

                // Freudenthal triangulation of the product of two simplices.
                // A product simplex {b0,...,bd1} x {f0,...,fd2} is decomposed into
                // simplices indexed by permutations of (d1 B-steps + d2 F-steps).
                // Each simplex has d1+d2+1 vertices tracing a staircase path
                // from (b0,f0) to (bd1,fd2).
                var simplices = FreudenthalDecompose(baseVerts, fiberVerts, fiberVertCount);
                allCells.AddRange(simplices);
            }
        }

        return MeshTopologyBuilder.Build(
            embeddingDimension: dimY,
            simplicialDimension: productSimpDim,
            vertexCoordinates: coords,
            vertexCount: totalVertCount,
            cellVertices: allCells.ToArray());
    }

    /// <summary>
    /// Wraps the simplicial decomposition into a FiberBundleMesh for backward compatibility
    /// with PullbackOperator and other existing operators.
    /// sigma_h selects fiber vertex 0 for each base vertex.
    /// </summary>
    public FiberBundleMesh ToFiberBundleMesh()
    {
        var yMesh = ToSimplicialMesh();
        int fiberVertCount = FiberMesh.VertexCount;

        // pi_h: drop fiber index
        var yVertToXVert = new int[ProductVertexCount];
        var fiberVertsPerX = new int[BaseMesh.VertexCount][];
        var xVertToYVert = new int[BaseMesh.VertexCount];

        for (int bv = 0; bv < BaseMesh.VertexCount; bv++)
        {
            fiberVertsPerX[bv] = new int[fiberVertCount];
            for (int fv = 0; fv < fiberVertCount; fv++)
            {
                int pv = ProductVertexIndex(bv, fv);
                yVertToXVert[pv] = bv;
                fiberVertsPerX[bv][fv] = pv;
            }

            // sigma_h: select fiber vertex 0
            xVertToYVert[bv] = ProductVertexIndex(bv, 0);
        }

        // Map X cells to Y cells: find the first Y cell whose base-projected vertices
        // match the X cell (with all fiber indices = 0, i.e., the sigma-image)
        var xCellToYCell = new int[BaseMesh.CellCount];
        for (int bc = 0; bc < BaseMesh.CellCount; bc++)
        {
            var baseVerts = BaseMesh.CellVertices[bc];
            var sigmaVerts = new HashSet<int>(baseVerts.Select(bv => ProductVertexIndex(bv, 0)));

            xCellToYCell[bc] = -1;
            for (int yc = 0; yc < yMesh.CellCount; yc++)
            {
                if (yMesh.CellVertices[yc].Length == sigmaVerts.Count &&
                    yMesh.CellVertices[yc].All(v => sigmaVerts.Contains(v)))
                {
                    xCellToYCell[bc] = yc;
                    break;
                }
            }
        }

        // Section coefficients: unit vectors (vertex-aligned section)
        var sectionCoeffs = new double[BaseMesh.CellCount][];
        int vertsPerCell = BaseMesh.SimplicialDimension + 1;
        for (int bc = 0; bc < BaseMesh.CellCount; bc++)
        {
            sectionCoeffs[bc] = new double[vertsPerCell];
            sectionCoeffs[bc][0] = 1.0;
        }

        return new FiberBundleMesh
        {
            BaseMesh = BaseMesh,
            AmbientMesh = yMesh,
            YVertexToXVertex = yVertToXVert,
            FiberVerticesPerXVertex = fiberVertsPerX,
            XVertexToYVertex = xVertToYVert,
            XCellToYCell = xCellToYCell,
            SectionCoefficients = sectionCoeffs,
        };
    }

    /// <summary>
    /// Freudenthal triangulation of a product of two simplices.
    /// Given base simplex vertices [b0,...,bd1] and fiber simplex vertices [f0,...,fd2],
    /// generates all (d1+d2)! / (d1! * d2!) simplices of dimension d1+d2.
    /// Each simplex corresponds to a staircase path from (b0,f0) to (bd1,fd2).
    /// </summary>
    private static List<int[]> FreudenthalDecompose(int[] baseVerts, int[] fiberVerts, int fiberVertCount)
    {
        int d1 = baseVerts.Length - 1; // base simplicial dim
        int d2 = fiberVerts.Length - 1; // fiber simplicial dim
        int totalDim = d1 + d2;

        // Each staircase path is a sequence of d1 'B' steps and d2 'F' steps.
        // A step sequence is a permutation of {B,B,...,F,F,...}.
        // We enumerate all such interleavings.
        var result = new List<int[]>();
        var steps = new int[totalDim]; // 0 = B step, 1 = F step
        EnumerateStaircases(steps, 0, d1, d2, baseVerts, fiberVerts, fiberVertCount, result);
        return result;
    }

    private static void EnumerateStaircases(
        int[] steps, int pos, int bRemaining, int fRemaining,
        int[] baseVerts, int[] fiberVerts, int fiberVertCount,
        List<int[]> result)
    {
        if (pos == steps.Length)
        {
            // Convert staircase path to simplex vertices
            var simplex = StaircaseToSimplex(steps, baseVerts, fiberVerts, fiberVertCount);
            result.Add(simplex);
            return;
        }

        if (bRemaining > 0)
        {
            steps[pos] = 0; // B step
            EnumerateStaircases(steps, pos + 1, bRemaining - 1, fRemaining, baseVerts, fiberVerts, fiberVertCount, result);
        }

        if (fRemaining > 0)
        {
            steps[pos] = 1; // F step
            EnumerateStaircases(steps, pos + 1, bRemaining, fRemaining - 1, baseVerts, fiberVerts, fiberVertCount, result);
        }
    }

    private static int[] StaircaseToSimplex(int[] steps, int[] baseVerts, int[] fiberVerts, int fiberVertCount)
    {
        int totalDim = steps.Length;
        var simplex = new int[totalDim + 1];

        int bi = 0, fi = 0;
        simplex[0] = baseVerts[0] * fiberVertCount + fiberVerts[0];

        for (int s = 0; s < totalDim; s++)
        {
            if (steps[s] == 0)
                bi++;
            else
                fi++;

            simplex[s + 1] = baseVerts[bi] * fiberVertCount + fiberVerts[fi];
        }

        return simplex;
    }
}
