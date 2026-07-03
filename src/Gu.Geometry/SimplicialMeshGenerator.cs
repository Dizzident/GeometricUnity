namespace Gu.Geometry;

/// <summary>
/// Generates simplicial meshes of arbitrary size for scaling benchmarks.
/// All generated meshes have proper topology (edges, faces, orientations)
/// built via <see cref="MeshTopologyBuilder"/>.
/// </summary>
public static class SimplicialMeshGenerator
{
    /// <summary>
    /// Creates a uniform 2D triangulation of a rectangular domain [0, cols] x [0, rows].
    /// Each grid square is split into 2 triangles, giving 2 * rows * cols faces.
    /// </summary>
    /// <param name="rows">Number of grid rows (>= 1).</param>
    /// <param name="cols">Number of grid columns (>= 1).</param>
    /// <returns>A <see cref="SimplicialMesh"/> with embedding dimension 2 and simplicial dimension 2.</returns>
    public static SimplicialMesh CreateUniform2D(int rows, int cols)
    {
        if (rows < 1) throw new ArgumentOutOfRangeException(nameof(rows), "Must be >= 1.");
        if (cols < 1) throw new ArgumentOutOfRangeException(nameof(cols), "Must be >= 1.");

        int vertexCount = (rows + 1) * (cols + 1);
        var coords = new double[vertexCount * 2];

        for (int r = 0; r <= rows; r++)
        {
            for (int c = 0; c <= cols; c++)
            {
                int v = r * (cols + 1) + c;
                coords[v * 2] = c;
                coords[v * 2 + 1] = r;
            }
        }

        int cellCount = 2 * rows * cols;
        var cells = new int[cellCount][];
        int ci = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                // Grid square vertices:
                //   v2 --- v3
                //   |  \   |
                //   |   \  |
                //   v0 --- v1
                int v0 = r * (cols + 1) + c;
                int v1 = v0 + 1;
                int v2 = v0 + (cols + 1);
                int v3 = v2 + 1;

                // Lower-left triangle
                cells[ci++] = [v0, v1, v2];
                // Upper-right triangle
                cells[ci++] = [v1, v3, v2];
            }
        }

        return MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: coords,
            vertexCount: vertexCount,
            cellVertices: cells);
    }

    /// <summary>
    /// Creates a uniform 2D triangulation targeting approximately <paramref name="targetFaceCount"/> faces.
    /// The actual face count will be the nearest even number >= <paramref name="targetFaceCount"/>.
    /// </summary>
    /// <param name="targetFaceCount">Desired number of triangular faces (>= 2).</param>
    /// <returns>A <see cref="SimplicialMesh"/> with embedding dimension 2 and simplicial dimension 2.</returns>
    public static SimplicialMesh CreateUniform2D(int targetFaceCount)
    {
        if (targetFaceCount < 2)
            throw new ArgumentOutOfRangeException(nameof(targetFaceCount), "Must be >= 2.");

        // Each grid square gives 2 faces. Pick rows x cols such that 2*rows*cols >= targetFaceCount.
        int nSquares = (targetFaceCount + 1) / 2; // ceiling division
        int side = (int)System.Math.Ceiling(System.Math.Sqrt(nSquares));
        int rows = side;
        int cols = (nSquares + side - 1) / side; // ceiling

        return CreateUniform2D(rows, cols);
    }

    /// <summary>
    /// Creates a uniform 3D tetrahedral mesh of a cube [0, n] x [0, n] x [0, n].
    /// Each grid cube is split into 5 tetrahedra, giving 5 * n^3 cells.
    /// </summary>
    /// <param name="n">Number of grid divisions per axis (>= 1).</param>
    /// <returns>A <see cref="SimplicialMesh"/> with embedding dimension 3 and simplicial dimension 3.</returns>
    public static SimplicialMesh CreateUniform3D(int n)
    {
        if (n < 1) throw new ArgumentOutOfRangeException(nameof(n), "Must be >= 1.");

        int vertexCount = (n + 1) * (n + 1) * (n + 1);
        var coords = new double[vertexCount * 3];

        int Idx(int x, int y, int z) => x * (n + 1) * (n + 1) + y * (n + 1) + z;

        for (int x = 0; x <= n; x++)
        {
            for (int y = 0; y <= n; y++)
            {
                for (int z = 0; z <= n; z++)
                {
                    int v = Idx(x, y, z);
                    coords[v * 3] = x;
                    coords[v * 3 + 1] = y;
                    coords[v * 3 + 2] = z;
                }
            }
        }

        // Each cube is split into 5 tetrahedra (Kuhn triangulation).
        var cells = new List<int[]>(5 * n * n * n);

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                for (int z = 0; z < n; z++)
                {
                    // Cube vertices:
                    int v0 = Idx(x, y, z);
                    int v1 = Idx(x + 1, y, z);
                    int v2 = Idx(x, y + 1, z);
                    int v3 = Idx(x + 1, y + 1, z);
                    int v4 = Idx(x, y, z + 1);
                    int v5 = Idx(x + 1, y, z + 1);
                    int v6 = Idx(x, y + 1, z + 1);
                    int v7 = Idx(x + 1, y + 1, z + 1);

                    // 5-tet decomposition (consistent across cubes)
                    cells.Add([v0, v1, v2, v4]);
                    cells.Add([v1, v2, v4, v5]);
                    cells.Add([v2, v4, v5, v6]);
                    cells.Add([v1, v2, v3, v5]);
                    cells.Add([v2, v3, v5, v7]);
                }
            }
        }

        return MeshTopologyBuilder.Build(
            embeddingDimension: 3,
            simplicialDimension: 3,
            vertexCoordinates: coords,
            vertexCount: vertexCount,
            cellVertices: cells.ToArray());
    }

    /// <summary>
    /// Creates a uniform 4D pentachoral mesh of a tesseract grid [0, n]^4.
    /// Each hypercube is split via the Coxeter–Freudenthal–Kuhn (S4 permutation)
    /// triangulation into 4! = 24 pentachora (4-simplices), giving 24 * n^4 cells.
    /// The construction is conforming across shared cell boundaries, so
    /// <see cref="MeshTopologyBuilder"/>'s vertex-tuple dedup yields a valid manifold.
    /// </summary>
    /// <param name="n">Number of grid divisions per axis (>= 1).</param>
    /// <returns>A <see cref="SimplicialMesh"/> with embedding dimension 4 and simplicial dimension 4.</returns>
    public static SimplicialMesh CreateUniform4D(int n)
    {
        if (n < 1) throw new ArgumentOutOfRangeException(nameof(n), "Must be >= 1.");
        return BuildKuhn4D(n, periodic: false);
    }

    /// <summary>
    /// Creates a PERIODIC uniform 4D pentachoral mesh: the Coxeter–Freudenthal–Kuhn
    /// triangulation of [0, n]^4 with opposite faces identified, i.e. a triangulated
    /// flat 4-torus (Z_n)^4. Identification (coordinate n ≡ coordinate 0 per axis)
    /// wraps edges/faces/volumes, so every subsimplex is interior: each face and each
    /// volume is shared by exactly 2 pentachora, and there is no boundary. The
    /// identification is applied purely at the vertex-index level, so the Freudenthal
    /// conformity and the (-1)^i boundary-orientation convention are preserved and the
    /// combinatorial ∂∘∂ = 0 (B1·B2 = 0, B2·B3 = 0) still holds exactly.
    /// </summary>
    /// <param name="n">
    /// Number of grid divisions per axis. Must be &gt;= 3: with n &lt; 3 the wrap
    /// collapses distinct subsimplices onto each other (the quotient is not a valid
    /// simplicial complex — a subsimplex would be shared by more than two cells), so
    /// the smallest well-formed 4-torus mesh is n = 3.
    /// </param>
    /// <returns>A <see cref="SimplicialMesh"/> with embedding dimension 4 and simplicial dimension 4.</returns>
    /// <remarks>
    /// Vertex coordinates are the base lattice positions in [0, n)^4; a wrapped edge
    /// therefore has a coordinate difference of magnitude n-1, not 1. Consumers that
    /// need geometric edge vectors (e.g. the frame-contracted Dirac operator) must apply
    /// the minimal-image convention using the period n. The mesh topology itself is
    /// coordinate-independent.
    /// </remarks>
    public static SimplicialMesh CreateUniform4DPeriodic(int n)
    {
        if (n < 3) throw new ArgumentOutOfRangeException(nameof(n), "Must be >= 3 for a well-formed 4-torus.");
        return BuildKuhn4D(n, periodic: true);
    }

    /// <summary>
    /// Shared Coxeter–Freudenthal–Kuhn 4-cube triangulation core. When
    /// <paramref name="periodic"/> is false this tiles the open block [0, n]^4
    /// ((n+1)^4 vertices); when true, opposite faces are identified (n^4 vertices,
    /// a flat 4-torus). Cell generation is identical; only the vertex indexing differs.
    /// </summary>
    private static SimplicialMesh BuildKuhn4D(int n, bool periodic)
    {
        int side = periodic ? n : n + 1;
        int vertexCount = side * side * side * side;
        var coords = new double[vertexCount * 4];

        // Row-major index, w fastest. Periodic identification wraps coordinate n to 0.
        int Idx(int x, int y, int z, int w)
        {
            if (periodic)
            {
                x %= n; y %= n; z %= n; w %= n;
            }
            return ((x * side + y) * side + z) * side + w;
        }

        for (int x = 0; x < side; x++)
        {
            for (int y = 0; y < side; y++)
            {
                for (int z = 0; z < side; z++)
                {
                    for (int w = 0; w < side; w++)
                    {
                        int v = Idx(x, y, z, w);
                        coords[v * 4 + 0] = x;
                        coords[v * 4 + 1] = y;
                        coords[v * 4 + 2] = z;
                        coords[v * 4 + 3] = w;
                    }
                }
            }
        }

        // The 24 permutations of the 4 axes (Heap-free explicit enumeration).
        var permutations = Permutations4();

        var cells = new List<int[]>(24 * n * n * n * n);
        var corner = new int[4];
        var p = new int[4];

        for (int x = 0; x < n; x++)
        {
            for (int y = 0; y < n; y++)
            {
                for (int z = 0; z < n; z++)
                {
                    for (int w = 0; w < n; w++)
                    {
                        corner[0] = x; corner[1] = y; corner[2] = z; corner[3] = w;

                        foreach (var perm in permutations)
                        {
                            var cellVerts = new int[5];
                            p[0] = corner[0]; p[1] = corner[1]; p[2] = corner[2]; p[3] = corner[3];
                            cellVerts[0] = Idx(p[0], p[1], p[2], p[3]);
                            for (int k = 0; k < 4; k++)
                            {
                                p[perm[k]] += 1;
                                cellVerts[k + 1] = Idx(p[0], p[1], p[2], p[3]);
                            }
                            cells.Add(cellVerts);
                        }
                    }
                }
            }
        }

        return MeshTopologyBuilder.Build(
            embeddingDimension: 4,
            simplicialDimension: 4,
            vertexCoordinates: coords,
            vertexCount: vertexCount,
            cellVertices: cells.ToArray());
    }

    /// <summary>Returns all 24 permutations of {0,1,2,3}.</summary>
    private static int[][] Permutations4()
    {
        var result = new List<int[]>(24);
        var items = new[] { 0, 1, 2, 3 };
        Permute(items, 0, result);
        return result.ToArray();
    }

    private static void Permute(int[] arr, int start, List<int[]> result)
    {
        if (start == arr.Length - 1)
        {
            result.Add((int[])arr.Clone());
            return;
        }
        for (int i = start; i < arr.Length; i++)
        {
            (arr[start], arr[i]) = (arr[i], arr[start]);
            Permute(arr, start + 1, result);
            (arr[start], arr[i]) = (arr[i], arr[start]);
        }
    }
}
