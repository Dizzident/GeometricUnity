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
    /// <param name="latticePeriod">
    /// 0 (default): the historical convention — face/volume vertex tuples stored in
    /// ascending GLOBAL-INDEX order. &gt; 0: LATTICE-CANONICAL mode for periodic lattice
    /// (torus) meshes with the given period per axis: face/volume tuples are stored in
    /// the coordinatewise chain order of their minimal-image lattice displacements,
    /// which depends only on the simplex's intrinsic lattice geometry (never on global
    /// vertex indices) and therefore commutes with every lattice translation.
    /// Boundary orientation signs are derived from the stored tuples, so the oriented
    /// chain complex (∂∘∂ = 0) is preserved exactly. Requires every subsimplex to span
    /// at most one lattice step per axis (Kuhn/Freudenthal-type lattice triangulations)
    /// and latticePeriod &gt;= 3 (minimal-image unambiguity); otherwise throws.
    /// The default path (0) is byte-identical to the historical behavior.
    /// </param>
    /// <param name="latticePeriods">
    /// Optional per-axis periods for anisotropic periodic lattices. When supplied,
    /// its length must equal <paramref name="embeddingDimension"/>, every period
    /// must be at least 3, and <paramref name="latticePeriod"/> must remain zero.
    /// This is the anisotropic counterpart of <paramref name="latticePeriod"/>;
    /// null preserves the historical scalar/default paths exactly.
    /// </param>
    public static SimplicialMesh Build(
        int embeddingDimension,
        int simplicialDimension,
        double[] vertexCoordinates,
        int vertexCount,
        int[][] cellVertices,
        int latticePeriod = 0,
        int[]? latticePeriods = null)
    {
        if (embeddingDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(embeddingDimension));
        if (simplicialDimension < 1)
            throw new ArgumentOutOfRangeException(nameof(simplicialDimension));
        if (vertexCoordinates.Length != vertexCount * embeddingDimension)
            throw new ArgumentException("Vertex coordinate array length mismatch.");
        if (latticePeriod < 0)
            throw new ArgumentOutOfRangeException(nameof(latticePeriod), "Must be >= 0.");
        if (latticePeriod is 1 or 2)
            throw new ArgumentOutOfRangeException(
                nameof(latticePeriod),
                "Lattice-canonical mode requires period >= 3 (minimal-image displacements of unit-step subsimplices are ambiguous below 3).");
        if (latticePeriods is not null)
        {
            if (latticePeriod != 0)
                throw new ArgumentException("Specify either latticePeriod or latticePeriods, not both.");
            if (latticePeriods.Length != embeddingDimension)
                throw new ArgumentException("Per-axis lattice-period count must equal the embedding dimension.", nameof(latticePeriods));
            if (latticePeriods.Any(period => period < 3))
                throw new ArgumentOutOfRangeException(nameof(latticePeriods), "Every lattice-canonical period must be >= 3.");
        }

        int[]? canonicalPeriods = latticePeriods;
        if (canonicalPeriods is null && latticePeriod > 0)
            canonicalPeriods = Enumerable.Repeat(latticePeriod, embeddingDimension).ToArray();

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
                                var tuple = new[] { sorted.Item1, sorted.Item2, sorted.Item3 };
                                if (canonicalPeriods is not null)
                                    LatticeChainOrder(tuple, vertexCoordinates, embeddingDimension, canonicalPeriods);
                                faceList.Add(tuple);
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
        // For face {v0, v1, v2} (stored tuple order), the boundary is the standard
        // simplicial boundary operator:
        //   d[v0,v1,v2] = [v1,v2] - [v0,v2] + [v0,v1]
        // Each oriented boundary edge [vi,vj] is expressed on the stored edge
        // (min,max): a factor +1 when vi < vj, -1 when the stored direction is
        // reversed. For the default (ascending) tuples this is exactly {+1,-1,+1};
        // lattice-canonical tuples may reorder, and the derived signs keep the
        // oriented chain complex (∂∘∂ = 0) exact.
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

            faceBoundaryEdges[fi] = new[] { e01, e02, e12 };
            faceBoundaryOrientations[fi] = new[]
            {
                v0 < v1 ? +1 : -1, // +[v0,v1]
                v0 < v2 ? -1 : +1, // -[v0,v2]
                v1 < v2 ? +1 : -1, // +[v1,v2]
            };
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
                                    var tuple = new[] { sorted.Item1, sorted.Item2, sorted.Item3, sorted.Item4 };
                                    if (canonicalPeriods is not null)
                                        LatticeChainOrder(tuple, vertexCoordinates, embeddingDimension, canonicalPeriods);
                                    volumeList.Add(tuple);
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
        // For volume {v0,v1,v2,v3} (stored tuple order), the boundary is:
        //   +[v1,v2,v3] - [v0,v2,v3] + [v0,v1,v3] - [v0,v1,v2]
        // i.e. omit v_i carries sign (-1)^i, mirroring the face->edge convention.
        // Each omitted ordered triple is expressed on the STORED face tuple with the
        // parity of the relating permutation. For the default path both tuples are
        // ascending, so the parity is +1 and the signs are exactly {+1,-1,+1,-1};
        // in lattice-canonical mode the omitted triple of a chain-ordered quadruple
        // is itself chain-ordered (a subchain), so the parity is again +1, but it is
        // computed generically to keep the oriented-chain-complex contract explicit.
        var volumeBoundaryFaces = new int[volumeList.Count][];
        var volumeBoundaryOrientations = new int[volumeList.Count][];
        Span<int> tri = stackalloc int[3];

        for (int vi = 0; vi < volumeList.Count; vi++)
        {
            var vol = volumeList[vi];
            var faces = new int[4];
            var orientations = new int[4];

            for (int omit = 0; omit < 4; omit++)
            {
                int t = 0;
                for (int k = 0; k < 4; k++)
                {
                    if (k != omit)
                        tri[t++] = vol[k];
                }

                int faceIdx = faceMap[SortThree(tri[0], tri[1], tri[2])];
                faces[omit] = faceIdx;
                int baseSign = (omit & 1) == 0 ? +1 : -1; // (-1)^omit
                orientations[omit] = baseSign * TriplePermutationParity(tri[0], tri[1], tri[2], faceList[faceIdx]);
            }

            volumeBoundaryFaces[vi] = faces;
            volumeBoundaryOrientations[vi] = orientations;
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

    /// <summary>
    /// Parity (+1 even / -1 odd) of the permutation relating the ordered triple
    /// (a, b, c) to the stored face tuple (a permutation of the same 3 vertices).
    /// </summary>
    private static int TriplePermutationParity(int a, int b, int c, int[] stored)
    {
        int p0 = stored[0] == a ? 0 : stored[0] == b ? 1 : 2;
        int p1 = stored[1] == a ? 0 : stored[1] == b ? 1 : 2;
        int p2 = stored[2] == a ? 0 : stored[2] == b ? 1 : 2;
        int inversions = (p0 > p1 ? 1 : 0) + (p0 > p2 ? 1 : 0) + (p1 > p2 ? 1 : 0);
        return (inversions & 1) == 0 ? +1 : -1;
    }

    /// <summary>
    /// Reorders a subsimplex vertex tuple IN PLACE into the lattice-canonical chain
    /// order: ascending in the coordinatewise partial order of minimal-image lattice
    /// displacements. On a Kuhn/Freudenthal-type lattice triangulation every
    /// subsimplex is a chain under that order (any two of its vertices differ by a
    /// componentwise-comparable 0/±1 step vector), so the order is total and strict.
    /// The rule reads only relative displacements, never global indices, hence it
    /// commutes with every lattice translation.
    /// </summary>
    private static void LatticeChainOrder(int[] tuple, double[] coords, int embeddingDimension, int[] periods)
    {
        // Insertion sort (tuples have <= 4 entries).
        for (int i = 1; i < tuple.Length; i++)
        {
            int j = i;
            while (j > 0 && !LatticeLeq(tuple[j - 1], tuple[j], coords, embeddingDimension, periods))
            {
                (tuple[j - 1], tuple[j]) = (tuple[j], tuple[j - 1]);
                j--;
            }
        }
    }

    /// <summary>
    /// True when the minimal-image lattice displacement from vertex <paramref name="a"/>
    /// to vertex <paramref name="b"/> is componentwise &gt;= 0. Throws when the two
    /// vertices are not chain-comparable or do not differ by a unit lattice step per
    /// axis — i.e. when the mesh is not a Kuhn-type lattice triangulation, for which
    /// the lattice-canonical convention is undefined.
    /// </summary>
    private static bool LatticeLeq(int a, int b, double[] coords, int embeddingDimension, int[] periods)
    {
        bool anyPositive = false, anyNegative = false;

        for (int d = 0; d < embeddingDimension; d++)
        {
            int period = periods[d];
            double diff = coords[b * embeddingDimension + d] - coords[a * embeddingDimension + d];
            double reduced = diff - period * Math.Round(diff / period);
            int step = (int)Math.Round(reduced);
            if (Math.Abs(reduced - step) > 1e-9 || Math.Abs(step) > 1)
                throw new InvalidOperationException(
                    $"Lattice-canonical mode requires unit-step lattice subsimplices: vertices {a} and {b} " +
                    $"have minimal-image displacement component {reduced} on axis {d} (period {period}).");

            if (step > 0) anyPositive = true;
            else if (step < 0) anyNegative = true;
        }

        if (anyPositive && anyNegative)
            throw new InvalidOperationException(
                $"Lattice-canonical mode requires chain-comparable subsimplex vertices, but vertices {a} and {b} " +
                "have a mixed-sign minimal-image displacement (not a Kuhn-type lattice triangulation).");

        return !anyNegative;
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
