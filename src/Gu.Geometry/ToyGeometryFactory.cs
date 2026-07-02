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
    /// G-003: Creates a structured 2D grid fiber bundle mesh with trivial fiber (BaseMesh == AmbientMesh).
    /// Produces 2 * rows * cols triangles via diagonal splitting of each quad cell.
    /// </summary>
    public static FiberBundleMesh CreateStructured2D(int rows, int cols)
    {
        int nv_x = cols + 1;
        int nv_y = rows + 1;
        int vertCount = nv_x * nv_y;
        int cellCount = 2 * rows * cols;

        // Vertex coordinates: unit grid [0,1]x[0,1]
        var coords = new double[vertCount * 2];
        for (int r = 0; r < nv_y; r++)
        {
            for (int c = 0; c < nv_x; c++)
            {
                int v = r * nv_x + c;
                coords[v * 2 + 0] = (double)c / cols;
                coords[v * 2 + 1] = (double)r / rows;
            }
        }

        // Triangulate: each quad (r,c)-(r,c+1)-(r+1,c)-(r+1,c+1) → 2 triangles
        var cells = new int[cellCount][];
        int ci = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int v00 = r * nv_x + c;
                int v10 = r * nv_x + c + 1;
                int v01 = (r + 1) * nv_x + c;
                int v11 = (r + 1) * nv_x + c + 1;
                cells[ci++] = new[] { v00, v10, v01 };
                cells[ci++] = new[] { v10, v11, v01 };
            }
        }

        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: coords,
            vertexCount: vertCount,
            cellVertices: cells);

        // Trivial fiber: BaseMesh == AmbientMesh, identity maps
        var xToY = new int[vertCount];
        var fiberVerts = new int[vertCount][];
        for (int v = 0; v < vertCount; v++)
        {
            xToY[v] = v;
            fiberVerts[v] = new[] { v };
        }

        var yToX = new int[vertCount];
        for (int v = 0; v < vertCount; v++) yToX[v] = v;

        var cellMap = new int[cellCount];
        for (int c = 0; c < cellCount; c++) cellMap[c] = c;

        var sectionCoeffs = new double[cellCount][];
        for (int c = 0; c < cellCount; c++) sectionCoeffs[c] = new[] { 1.0, 0.0, 0.0 };

        return new FiberBundleMesh
        {
            BaseMesh = mesh,
            AmbientMesh = mesh,
            YVertexToXVertex = yToX,
            FiberVerticesPerXVertex = fiberVerts,
            XVertexToYVertex = xToY,
            XCellToYCell = cellMap,
            SectionCoefficients = sectionCoeffs,
        };
    }

    /// <summary>
    /// Creates a structured 2D fiber-bundle geometry with the same 5D ambient/fiber layout as
    /// <see cref="CreateToy2D"/>, but on a rows x cols refined base mesh.
    /// </summary>
    public static FiberBundleMesh CreateStructuredFiberBundle2D(int rows, int cols)
    {
        int nvX = cols + 1;
        int nvY = rows + 1;
        int xVertCount = nvX * nvY;
        int xCellCount = 2 * rows * cols;

        var xCoords = new double[xVertCount * 2];
        for (int r = 0; r < nvY; r++)
        {
            for (int c = 0; c < nvX; c++)
            {
                int v = r * nvX + c;
                xCoords[v * 2 + 0] = (double)c / cols;
                xCoords[v * 2 + 1] = (double)r / rows;
            }
        }

        var xCells = new int[xCellCount][];
        int xci = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                int v00 = r * nvX + c;
                int v10 = r * nvX + c + 1;
                int v01 = (r + 1) * nvX + c;
                int v11 = (r + 1) * nvX + c + 1;
                xCells[xci++] = new[] { v00, v10, v01 };
                xCells[xci++] = new[] { v10, v11, v01 };
            }
        }

        var xMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: xCoords,
            vertexCount: xVertCount,
            cellVertices: xCells);

        int fiberSize = 3;
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

                yCoords[yv * dimY + 0] = x0;
                yCoords[yv * dimY + 1] = x1;
                yCoords[yv * dimY + 2] = f * 0.1;
                yCoords[yv * dimY + 3] = (f + 1) * 0.05;
                yCoords[yv * dimY + 4] = (2 - f) * 0.1;
            }

            xVertToYVert[xv] = xv * fiberSize;
        }

        var yCells = new List<int[]>();
        var xCellToYCell = new int[xMesh.CellCount];
        for (int xc = 0; xc < xMesh.CellCount; xc++)
        {
            var xCell = xMesh.CellVertices[xc];
            int a = xCell[0], b = xCell[1], c = xCell[2];

            int ya = xVertToYVert[a];
            int yb = xVertToYVert[b];
            int yc = xVertToYVert[c];
            xCellToYCell[xc] = yCells.Count;
            yCells.Add(new[] { ya, yb, yc });

            for (int f = 0; f < fiberSize - 1; f++)
            {
                int yaF = a * fiberSize + f;
                int yaF1 = a * fiberSize + f + 1;
                int ybF = b * fiberSize + f;
                yCells.Add(new[] { yaF, yaF1, ybF });
            }
        }

        var yMesh = MeshTopologyBuilder.Build(
            embeddingDimension: dimY,
            simplicialDimension: 2,
            vertexCoordinates: yCoords,
            vertexCount: yVertCount,
            cellVertices: yCells.ToArray());

        var sectionCoeffs = new double[xMesh.CellCount][];
        for (int xc = 0; xc < xMesh.CellCount; xc++)
            sectionCoeffs[xc] = new[] { 1.0, 0.0, 0.0 };

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

    /// <summary>
    /// Creates a 4D toy fiber bundle: a single tesseract via the Coxeter–Freudenthal–Kuhn
    /// triangulation (24 pentachora, equivalent to <see cref="SimplicialMeshGenerator.CreateUniform4D"/>
    /// with n=1), wrapped as a trivial-fiber <see cref="FiberBundleMesh"/> (BaseMesh == AmbientMesh,
    /// identity pi/sigma maps). This is the smallest human-debuggable 4D case for the M3 Shiab tests.
    /// </summary>
    public static FiberBundleMesh CreateToy4D()
    {
        var mesh = SimplicialMeshGenerator.CreateUniform4D(1);
        return WrapTrivialFiber4D(mesh);
    }

    /// <summary>
    /// Creates a structured 4D fiber-bundle geometry on the base X_h = CreateUniform4D(n) (dimX = 4).
    /// With <paramref name="fiberSize"/> == 1 (default) the fiber is trivial (BaseMesh == AmbientMesh,
    /// identity maps), which is what the M3 Einsteinian Shiab studies consume. With
    /// <paramref name="fiberSize"/> &gt; 1 a fiber of that many points is attached per base vertex in an
    /// embedding dimension dimY = 4 + fiberSize, with sigma selecting fiber point 0. The nontrivial-fiber
    /// (14D Y_h) dimension policy is a physicist-gated open question; the parameter defers it.
    /// </summary>
    public static FiberBundleMesh CreateStructuredFiberBundle4D(int n, int fiberSize = 1)
    {
        if (fiberSize < 1) throw new ArgumentOutOfRangeException(nameof(fiberSize), "Must be >= 1.");

        var xMesh = SimplicialMeshGenerator.CreateUniform4D(n);

        if (fiberSize == 1)
            return WrapTrivialFiber4D(xMesh);

        int xVertCount = xMesh.VertexCount;
        int yVertCount = xVertCount * fiberSize;
        int dimY = 4 + fiberSize;

        var yCoords = new double[yVertCount * dimY];
        var yVertToXVert = new int[yVertCount];
        var fiberVerts = new int[xVertCount][];
        var xVertToYVert = new int[xVertCount];

        for (int xv = 0; xv < xVertCount; xv++)
        {
            double x0 = xMesh.VertexCoordinates[xv * 4 + 0];
            double x1 = xMesh.VertexCoordinates[xv * 4 + 1];
            double x2 = xMesh.VertexCoordinates[xv * 4 + 2];
            double x3 = xMesh.VertexCoordinates[xv * 4 + 3];

            fiberVerts[xv] = new int[fiberSize];
            for (int f = 0; f < fiberSize; f++)
            {
                int yv = xv * fiberSize + f;
                fiberVerts[xv][f] = yv;
                yVertToXVert[yv] = xv;

                yCoords[yv * dimY + 0] = x0;
                yCoords[yv * dimY + 1] = x1;
                yCoords[yv * dimY + 2] = x2;
                yCoords[yv * dimY + 3] = x3;
                for (int j = 0; j < fiberSize; j++)
                    yCoords[yv * dimY + (4 + j)] = f * 0.1 * (j + 1);
            }

            // sigma selects fiber point 0
            xVertToYVert[xv] = xv * fiberSize;
        }

        // Y cells: the sigma-selected pentachoron per base cell.
        var yCells = new List<int[]>(xMesh.CellCount);
        var xCellToYCell = new int[xMesh.CellCount];
        for (int xc = 0; xc < xMesh.CellCount; xc++)
        {
            var xCell = xMesh.CellVertices[xc];
            xCellToYCell[xc] = yCells.Count;
            yCells.Add(xCell.Select(v => xVertToYVert[v]).ToArray());
        }

        var yMesh = MeshTopologyBuilder.Build(
            embeddingDimension: dimY,
            simplicialDimension: 4,
            vertexCoordinates: yCoords,
            vertexCount: yVertCount,
            cellVertices: yCells.ToArray());

        var sectionCoeffs = new double[xMesh.CellCount][];
        for (int xc = 0; xc < xMesh.CellCount; xc++)
            sectionCoeffs[xc] = new[] { 1.0, 0.0, 0.0, 0.0, 0.0 };

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
    /// Wraps a 4D base mesh as a trivial-fiber <see cref="FiberBundleMesh"/>
    /// (BaseMesh == AmbientMesh, identity pi/sigma maps), mirroring CreateStructured2D.
    /// </summary>
    private static FiberBundleMesh WrapTrivialFiber4D(SimplicialMesh mesh)
    {
        int vertCount = mesh.VertexCount;
        int cellCount = mesh.CellCount;

        var xToY = new int[vertCount];
        var fiberVerts = new int[vertCount][];
        var yToX = new int[vertCount];
        for (int v = 0; v < vertCount; v++)
        {
            xToY[v] = v;
            fiberVerts[v] = new[] { v };
            yToX[v] = v;
        }

        var cellMap = new int[cellCount];
        for (int c = 0; c < cellCount; c++) cellMap[c] = c;

        var sectionCoeffs = new double[cellCount][];
        for (int c = 0; c < cellCount; c++)
            sectionCoeffs[c] = new[] { 1.0, 0.0, 0.0, 0.0, 0.0 };

        return new FiberBundleMesh
        {
            BaseMesh = mesh,
            AmbientMesh = mesh,
            YVertexToXVertex = yToX,
            FiberVerticesPerXVertex = fiberVerts,
            XVertexToYVertex = xToY,
            XCellToYCell = cellMap,
            SectionCoefficients = sectionCoeffs,
        };
    }
}
