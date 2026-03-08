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
        };
    }

    private static (int, int, int) SortThree(int a, int b, int c)
    {
        if (a > b) (a, b) = (b, a);
        if (b > c) (b, c) = (c, b);
        if (a > b) (a, b) = (b, a);
        return (a, b, c);
    }
}
