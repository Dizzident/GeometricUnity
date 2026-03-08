namespace Gu.Geometry;

/// <summary>
/// Factory for creating small toy geometries used in testing and debugging.
/// These are tiny enough to be CPU-debuggable with human-readable output.
/// </summary>
public static class ToyGeometryFactory
{
    /// <summary>
    /// Creates a 2D toy fiber bundle:
    /// - X_h: 4 triangles covering a unit square in 2D (dimX=2)
    /// - Y_h: triangulated mesh in 5D with 3 fiber points per X vertex (dimY=5)
    /// - pi_h: many-to-one projection
    /// - sigma_h: selects first fiber point
    /// </summary>
    public static FiberBundleMesh CreateToy2D()
    {
        // X_h: unit square split into 4 triangles
        // Vertices: (0,0), (1,0), (0,1), (1,1), (0.5,0.5)
        var xMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[]
            {
                0, 0,     // v0
                1, 0,     // v1
                0, 1,     // v2
                1, 1,     // v3
                0.5, 0.5, // v4 (center)
            },
            vertexCount: 5,
            cellVertices: new[]
            {
                new[] { 0, 1, 4 }, // bottom
                new[] { 1, 3, 4 }, // right
                new[] { 3, 2, 4 }, // top
                new[] { 2, 0, 4 }, // left
            });

        // Y_h: each X vertex gets 3 fiber points in 5D
        // Fiber coordinates: first 2 dims = X coords, last 3 dims = fiber position
        // 5 X vertices * 3 fiber points = 15 Y vertices
        int fiberSize = 3;
        int xVertCount = 5;
        int yVertCount = xVertCount * fiberSize;
        int dimY = 5;

        var yCoords = new double[yVertCount * dimY];
        var yVertToXVert = new int[yVertCount];
        var fiberVerts = new int[xVertCount][];
        var xVertToYVert = new int[xVertCount];

        for (int xv = 0; xv < xVertCount; xv++)
        {
            double x0 = xMesh.VertexCoordinates[xv * 2];
            double x1 = xMesh.VertexCoordinates[xv * 2 + 1];

            fiberVerts[xv] = new int[fiberSize];

            for (int f = 0; f < fiberSize; f++)
            {
                int yv = xv * fiberSize + f;
                fiberVerts[xv][f] = yv;
                yVertToXVert[yv] = xv;

                // Base coordinates
                yCoords[yv * dimY + 0] = x0;
                yCoords[yv * dimY + 1] = x1;
                // Fiber coordinates: small offsets
                yCoords[yv * dimY + 2] = f * 0.1;
                yCoords[yv * dimY + 3] = (f + 1) * 0.05;
                yCoords[yv * dimY + 4] = (2 - f) * 0.1;
            }

            // sigma selects first fiber point
            xVertToYVert[xv] = xv * fiberSize;
        }

        // Build Y_h triangles: for each X cell, create cells connecting fiber points
        // Each X triangle (a,b,c) generates cells from fiber points
        var yCells = new List<int[]>();
        var xCellToYCell = new int[xMesh.CellCount];

        for (int xc = 0; xc < xMesh.CellCount; xc++)
        {
            var xCell = xMesh.CellVertices[xc];
            int a = xCell[0], b = xCell[1], c = xCell[2];

            // Create one triangle in Y from the sigma-selected fiber points
            int ya = xVertToYVert[a]; // fiber point 0 of vertex a
            int yb = xVertToYVert[b]; // fiber point 0 of vertex b
            int yc = xVertToYVert[c]; // fiber point 0 of vertex c

            xCellToYCell[xc] = yCells.Count;
            yCells.Add(new[] { ya, yb, yc });

            // Additional cells connecting fiber points within each fiber
            // Connect fiber points across adjacent X vertices
            for (int f = 0; f < fiberSize - 1; f++)
            {
                int ya_f = a * fiberSize + f;
                int ya_f1 = a * fiberSize + f + 1;
                int yb_f = b * fiberSize + f;
                yCells.Add(new[] { ya_f, ya_f1, yb_f });
            }
        }

        var yMesh = MeshTopologyBuilder.Build(
            embeddingDimension: dimY,
            simplicialDimension: 2,
            vertexCoordinates: yCoords,
            vertexCount: yVertCount,
            cellVertices: yCells.ToArray());

        // Section coefficients: unit vectors at sigma-selected vertex
        var sectionCoeffs = new double[xMesh.CellCount][];
        for (int xc = 0; xc < xMesh.CellCount; xc++)
        {
            sectionCoeffs[xc] = new[] { 1.0, 0.0, 0.0 };
        }

        return new FiberBundleMesh
        {
            BaseMesh = xMesh,
            AmbientMesh = yMesh,
            YVertexToXVertex = yVertToXVert,
            FiberVerticesPerXVertex = fiberVerts,
            XVertexToYVertex = xVertToYVert,
            XCellToYCell = xCellToYCell,
            SectionCoefficients = sectionCoeffs,
        };
    }

    /// <summary>
    /// Creates a 2D toy product bundle mesh using ProductBundleMesh:
    /// - X_h: single triangle in 2D (dimX=2, simpDim=2)
    /// - F_h: single edge in 1D (dimF=1, simpDim=1)
    /// - Y_h = X_h x F_h: product mesh in 3D (dimY=3, simpDim=3)
    /// Decomposed to tetrahedra via Freudenthal triangulation.
    /// </summary>
    public static ProductBundleMesh CreateToyProduct2D()
    {
        var xMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

        var fMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 1,
            simplicialDimension: 1,
            vertexCoordinates: new double[] { 0.0, 1.0 },
            vertexCount: 2,
            cellVertices: new[] { new[] { 0, 1 } });

        return new ProductBundleMesh
        {
            BaseMesh = xMesh,
            FiberMesh = fMesh,
        };
    }

    /// <summary>
    /// Creates a minimal 3D toy fiber bundle:
    /// - X_h: 2 tetrahedra in 3D (dimX=3)
    /// - Y_h: mesh in 9D with 2 fiber points per X vertex (dimY=9)
    /// </summary>
    public static FiberBundleMesh CreateToy3D()
    {
        // X_h: unit cube corner split into 2 tetrahedra
        var xMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: new double[]
            {
                0, 0, 0,  // v0
                1, 0, 0,  // v1
                0, 1, 0,  // v2
                0, 0, 1,  // v3
                1, 1, 1,  // v4
            },
            vertexCount: 5,
            cellVertices: new[]
            {
                new[] { 0, 1, 2, 3 },
                new[] { 1, 2, 3, 4 },
            });

        int fiberSize = 2;
        int xVertCount = 5;
        int yVertCount = xVertCount * fiberSize;
        int dimY = 9;

        var yCoords = new double[yVertCount * dimY];
        var yVertToXVert = new int[yVertCount];
        var fiberVerts = new int[xVertCount][];
        var xVertToYVert = new int[xVertCount];

        for (int xv = 0; xv < xVertCount; xv++)
        {
            double x0 = xMesh.VertexCoordinates[xv * 3];
            double x1 = xMesh.VertexCoordinates[xv * 3 + 1];
            double x2 = xMesh.VertexCoordinates[xv * 3 + 2];

            fiberVerts[xv] = new int[fiberSize];

            for (int f = 0; f < fiberSize; f++)
            {
                int yv = xv * fiberSize + f;
                fiberVerts[xv][f] = yv;
                yVertToXVert[yv] = xv;

                // Base coordinates
                yCoords[yv * dimY + 0] = x0;
                yCoords[yv * dimY + 1] = x1;
                yCoords[yv * dimY + 2] = x2;
                // Fiber coordinates
                for (int fi = 0; fi < 6; fi++)
                    yCoords[yv * dimY + 3 + fi] = f * 0.1 * (fi + 1);
            }

            xVertToYVert[xv] = xv * fiberSize;
        }

        // Build Y tetrahedra from sigma-selected fiber points
        var yCells = new List<int[]>();
        var xCellToYCell = new int[xMesh.CellCount];

        for (int xc = 0; xc < xMesh.CellCount; xc++)
        {
            var xCell = xMesh.CellVertices[xc];
            xCellToYCell[xc] = yCells.Count;
            yCells.Add(xCell.Select(v => xVertToYVert[v]).ToArray());
        }

        var yMesh = MeshTopologyBuilder.Build(
            embeddingDimension: dimY,
            simplicialDimension: 3,
            vertexCoordinates: yCoords,
            vertexCount: yVertCount,
            cellVertices: yCells.ToArray());

        var sectionCoeffs = new double[xMesh.CellCount][];
        for (int xc = 0; xc < xMesh.CellCount; xc++)
        {
            sectionCoeffs[xc] = new[] { 1.0, 0.0, 0.0, 0.0 };
        }

        return new FiberBundleMesh
        {
            BaseMesh = xMesh,
            AmbientMesh = yMesh,
            YVertexToXVertex = yVertToXVert,
            FiberVerticesPerXVertex = fiberVerts,
            XVertexToYVertex = xVertToYVert,
            XCellToYCell = xCellToYCell,
            SectionCoefficients = sectionCoeffs,
        };
    }
}
