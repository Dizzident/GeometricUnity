namespace Gu.Geometry;

/// <summary>
/// Builds a SimplicialMesh from vertex coordinates and cell definitions,
/// extracting all subsimplex topology (edges, faces) and incidence data.
/// </summary>
public static class MeshTopologyBuilder
{
    /// <summary>
    /// Build a complete SimplicialMesh from vertices and cells.
    /// Extracts edges (1-subsimplices), faces (2-subsimplices),
    /// cell-edge/cell-face incidence, and face boundary orientations.
    /// </summary>
    /// <param name="embeddingDimension">Dimension of embedding space.</param>
    /// <param name="simplicialDimension">Dimension of top-level simplices.</param>
    /// <param name="vertexCoordinates">Flat vertex coordinate array [vertexCount * embeddingDim].</param>
    /// <param name="vertexCount">Number of vertices.</param>
    /// <param name="cellVertices">Cell definitions: cellVertices[cellIdx] = vertex indices.</param>
    public static SimplicialMesh Build(
        int embeddingDimension,
        int simplicialDimension,
        double[] vertexCoordinates,
        int vertexCount,
        int[][] cellVertices)
    {
        if (embeddingDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(embeddingDimension));
        if (simplicialDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(simplicialDimension));
        if (vertexCoordinates.Length != vertexCount * embeddingDimension)
            throw new ArgumentException("Vertex coordinate array length mismatch.");

        foreach (var cell in cellVertices)
        {
            if (cell.Length != simplicialDimension + 1)
                throw new ArgumentException(
                    $"Cell has {cell.Length} vertices but simplicialDimension={simplicialDimension} requires {simplicialDimension + 1}.");
        }

        // Extract unique edges with canonical ordering (v0 < v1)
        var edgeMap = new Dictionary<(int, int), int>();
        var edgeList = new List<int[]>();
        var cellEdgesList = new List<int[]>();

        foreach (var cell in cellVertices)
        {
            var cellEdges = new List<int>();
            for (int i = 0; i < cell.Length; i++)
            {
                for (int j = i + 1; j < cell.Length; j++)
                {
                    var key = (Math.Min(cell[i], cell[j]), Math.Max(cell[i], cell[j]));
                    if (!edgeMap.TryGetValue(key, out int edgeIdx))
                    {
                        edgeIdx = edgeList.Count;
                        edgeMap[key] = edgeIdx;
                        edgeList.Add(new[] { key.Item1, key.Item2 });
                    }
                    cellEdges.Add(edgeIdx);
                }
            }
            cellEdgesList.Add(cellEdges.ToArray());
        }

        // Extract unique faces with canonical ordering (v0 < v1 < v2)
        var faceMap = new Dictionary<(int, int, int), int>();
        var faceList = new List<int[]>();
        var cellFacesList = new List<int[]>();

        // Only extract faces if simplicial dimension >= 2
        if (simplicialDimension >= 2)
        {
            foreach (var cell in cellVertices)
            {
                var cellFaces = new List<int>();
                for (int i = 0; i < cell.Length; i++)
                {
                    for (int j = i + 1; j < cell.Length; j++)
                    {
                        for (int k = j + 1; k < cell.Length; k++)
                        {
                            var sorted = SortThree(cell[i], cell[j], cell[k]);
                            if (!faceMap.TryGetValue(sorted, out int faceIdx))
                            {
                                faceIdx = faceList.Count;
                                faceMap[sorted] = faceIdx;
                                faceList.Add(new[] { sorted.Item1, sorted.Item2, sorted.Item3 });
                            }
                            cellFaces.Add(faceIdx);
                        }
                    }
                }
                cellFacesList.Add(cellFaces.ToArray());
            }
        }
        else
        {
            // 1D simplices have no 2-faces
            for (int i = 0; i < cellVertices.Length; i++)
                cellFacesList.Add(Array.Empty<int>());
        }

        // Compute face boundary edges and orientations
        // For face {v0, v1, v2} (canonical order), the boundary is:
        //   +edge(v0,v1) - edge(v0,v2) + edge(v1,v2)
        // This follows the standard simplicial boundary operator:
        //   d[v0,v1,v2] = [v1,v2] - [v0,v2] + [v0,v1]
        var faceBoundaryEdges = new int[faceList.Count][];
        var faceBoundaryOrientations = new int[faceList.Count][];

        for (int fi = 0; fi < faceList.Count; fi++)
        {
            var face = faceList[fi];
            int v0 = face[0], v1 = face[1], v2 = face[2];

            // Boundary edges in canonical (sorted) form
            int e01 = edgeMap[(Math.Min(v0, v1), Math.Max(v0, v1))];
            int e02 = edgeMap[(Math.Min(v0, v2), Math.Max(v0, v2))];
            int e12 = edgeMap[(Math.Min(v1, v2), Math.Max(v1, v2))];

            // Standard simplicial boundary: d[v0,v1,v2] = [v1,v2] - [v0,v2] + [v0,v1]
            faceBoundaryEdges[fi] = new[] { e01, e02, e12 };
            faceBoundaryOrientations[fi] = new[] { +1, -1, +1 };
        }

        // Extract unique volumes (3-subsimplices / tetrahedra) with canonical ordering
        // (v0 < v1 < v2 < v3). Mirrors the face block one degree higher.
        var volumeMap = new Dictionary<(int, int, int, int), int>();
        var volumeList = new List<int[]>();
        var cellVolumesList = new List<int[]>();

        if (simplicialDimension >= 3)
        {
            foreach (var cell in cellVertices)
            {
                var cellVolumes = new List<int>();
                for (int i = 0; i < cell.Length; i++)
                {
                    for (int j = i + 1; j < cell.Length; j++)
                    {
                        for (int k = j + 1; k < cell.Length; k++)
                        {
                            for (int l = k + 1; l < cell.Length; l++)
                            {
                                var sorted = SortFour(cell[i], cell[j], cell[k], cell[l]);
                                if (!volumeMap.TryGetValue(sorted, out int volIdx))
                                {
                                    volIdx = volumeList.Count;
                                    volumeMap[sorted] = volIdx;
                                    volumeList.Add(new[] { sorted.Item1, sorted.Item2, sorted.Item3, sorted.Item4 });
                                }
                                cellVolumes.Add(volIdx);
                            }
                        }
                    }
                }
                cellVolumesList.Add(cellVolumes.ToArray());
            }
        }
        else
        {
            for (int i = 0; i < cellVertices.Length; i++)
                cellVolumesList.Add(Array.Empty<int>());
        }

        // Compute volume boundary faces and orientations.
        // For volume {v0,v1,v2,v3} (canonical order), the boundary is:
        //   +[v1,v2,v3] - [v0,v2,v3] + [v0,v1,v3] - [v0,v1,v2]
        // i.e. omit v_i carries sign (-1)^i, mirroring the face->edge convention.
        var volumeBoundaryFaces = new int[volumeList.Count][];
        var volumeBoundaryOrientations = new int[volumeList.Count][];

        for (int vi = 0; vi < volumeList.Count; vi++)
        {
            var vol = volumeList[vi];
            int v0 = vol[0], v1 = vol[1], v2 = vol[2], v3 = vol[3];

            int f0 = faceMap[SortThree(v1, v2, v3)]; // omit v0, sign +1
            int f1 = faceMap[SortThree(v0, v2, v3)]; // omit v1, sign -1
            int f2 = faceMap[SortThree(v0, v1, v3)]; // omit v2, sign +1
            int f3 = faceMap[SortThree(v0, v1, v2)]; // omit v3, sign -1

            volumeBoundaryFaces[vi] = new[] { f0, f1, f2, f3 };
            volumeBoundaryOrientations[vi] = new[] { +1, -1, +1, -1 };
        }

        // Compute vertex-edge incidence and orientations
        // For each vertex, collect incident edges and signs.
        // Edge {v0, v1} with v0 < v1: v0 gets sign +1, v1 gets sign -1.
        var vertexEdgesLists = new List<int>[vertexCount];
        var vertexEdgeOrientLists = new List<int>[vertexCount];
        for (int v = 0; v < vertexCount; v++)
        {
            vertexEdgesLists[v] = new List<int>();
            vertexEdgeOrientLists[v] = new List<int>();
        }

        for (int ei = 0; ei < edgeList.Count; ei++)
        {
            int v0 = edgeList[ei][0], v1 = edgeList[ei][1];
            vertexEdgesLists[v0].Add(ei);
            vertexEdgeOrientLists[v0].Add(+1); // v0 is first endpoint
            vertexEdgesLists[v1].Add(ei);
            vertexEdgeOrientLists[v1].Add(-1); // v1 is second endpoint
        }

        var vertexEdges = new int[vertexCount][];
        var vertexEdgeOrientations = new int[vertexCount][];
        for (int v = 0; v < vertexCount; v++)
        {
            vertexEdges[v] = vertexEdgesLists[v].ToArray();
            vertexEdgeOrientations[v] = vertexEdgeOrientLists[v].ToArray();
        }

        // The volume (3-subsimplex) layer is populated only for simplicialDimension >= 3.
        // For lower dimensions the new arrays keep their empty-array defaults so the
        // 2D construction path is unchanged.
        bool hasVolumes = simplicialDimension >= 3;

        return new SimplicialMesh
        {
            EmbeddingDimension = embeddingDimension,
            SimplicialDimension = simplicialDimension,
            VertexCoordinates = vertexCoordinates,
            VertexCount = vertexCount,
            CellVertices = cellVertices,
            Edges = edgeList.ToArray(),
            Faces = faceList.ToArray(),
            CellEdges = cellEdgesList.ToArray(),
            CellFaces = cellFacesList.ToArray(),
            FaceBoundaryEdges = faceBoundaryEdges,
            FaceBoundaryOrientations = faceBoundaryOrientations,
            VertexEdges = vertexEdges,
            VertexEdgeOrientations = vertexEdgeOrientations,
            Volumes = hasVolumes ? volumeList.ToArray() : Array.Empty<int[]>(),
            VolumeBoundaryFaces = hasVolumes ? volumeBoundaryFaces : Array.Empty<int[]>(),
            VolumeBoundaryOrientations = hasVolumes ? volumeBoundaryOrientations : Array.Empty<int[]>(),
            CellVolumes = hasVolumes ? cellVolumesList.ToArray() : Array.Empty<int[]>(),
        };
    }

    private static (int, int, int) SortThree(int a, int b, int c)
    {
        if (a > b) (a, b) = (b, a);
        if (b > c) (b, c) = (c, b);
        if (a > b) (a, b) = (b, a);
        return (a, b, c);
    }

    private static (int, int, int, int) SortFour(int a, int b, int c, int d)
    {
        if (a > b) (a, b) = (b, a);
        if (c > d) (c, d) = (d, c);
        if (a > c) (a, c) = (c, a);
        if (b > d) (b, d) = (d, b);
        if (b > c) (b, c) = (c, b);
        return (a, b, c, d);
    }
}
