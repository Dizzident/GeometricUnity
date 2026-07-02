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

        int side = n + 1;
        int vertexCount = side * side * side * side;
        var coords = new double[vertexCount * 4];

        // Row-major index, w fastest.
        int Idx(int x, int y, int z, int w) => ((x * side + y) * side + z) * side + w;

        for (int x = 0; x <= n; x++)
        {
            for (int y = 0; y <= n; y++)
            {
                for (int z = 0; z <= n; z++)
                {
                    for (int w = 0; w <= n; w++)
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
