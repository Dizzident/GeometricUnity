namespace Gu.Geometry;

/// <summary>
/// Dimension-generic simplicial mesh representation.
/// Supports embedding dimensions from 1 to 14+ (for Y_h in production).
/// All subsimplex topology (edges, faces) and incidence data are included.
/// </summary>
public sealed class SimplicialMesh
{
    /// <summary>Dimension of the embedding space (e.g., 2, 3, 4, 14).</summary>
    public required int EmbeddingDimension { get; init; }

    /// <summary>Dimension of the top-level simplices (e.g., 2 for triangles, 3 for tetrahedra).</summary>
    public required int SimplicialDimension { get; init; }

    /// <summary>
    /// Vertex coordinates as a flat array: [v0_x0, v0_x1, ..., v1_x0, v1_x1, ...].
    /// Length = VertexCount * EmbeddingDimension.
    /// </summary>
    public required double[] VertexCoordinates { get; init; }

    /// <summary>Number of vertices.</summary>
    public required int VertexCount { get; init; }

    /// <summary>
    /// Cell vertex indices. Each cell is a (SimplicialDimension+1)-simplex.
    /// CellVertices[cellIdx] = array of vertex indices defining the cell.
    /// Vertex ordering defines orientation.
    /// </summary>
    public required int[][] CellVertices { get; init; }

    /// <summary>
    /// Edges (1-subsimplices). Each edge is a pair of vertex indices.
    /// Edges[edgeIdx] = { v0, v1 } with v0 &lt; v1 (canonical ordering).
    /// omega_h (connection 1-form) lives on edges.
    /// </summary>
    public required int[][] Edges { get; init; }

    /// <summary>
    /// Faces (2-subsimplices). Each face is a triple of vertex indices.
    /// Faces[faceIdx] = { v0, v1, v2 } stored in the mesh's canonical tuple order:
    /// ascending global index by default, or lattice-canonical chain order when the
    /// mesh was built with <see cref="MeshTopologyBuilder.Build"/>'s latticePeriod
    /// (translation-covariant on periodic lattice meshes). FaceBoundaryOrientations
    /// is always consistent with the stored tuple order.
    /// F_h (curvature 2-form) lives on faces.
    /// </summary>
    public required int[][] Faces { get; init; }

    /// <summary>
    /// For each cell, the indices of its edges.
    /// CellEdges[cellIdx] = array of edge indices belonging to this cell.
    /// </summary>
    public required int[][] CellEdges { get; init; }

    /// <summary>
    /// For each cell, the indices of its faces.
    /// CellFaces[cellIdx] = array of face indices belonging to this cell.
    /// </summary>
    public required int[][] CellFaces { get; init; }

    /// <summary>
    /// For each face, the indices of its boundary edges.
    /// FaceBoundaryEdges[faceIdx] = array of 3 edge indices forming the boundary.
    /// </summary>
    public required int[][] FaceBoundaryEdges { get; init; }

    /// <summary>
    /// Orientation signs for each face's boundary edges.
    /// FaceBoundaryOrientations[faceIdx][i] = +1 or -1.
    /// Used in the discrete exterior derivative: d(omega) on face = sum_i sign_i * omega(edge_i).
    /// </summary>
    public required int[][] FaceBoundaryOrientations { get; init; }

    /// <summary>
    /// Volumes (3-subsimplices). Each volume is a quadruple of vertex indices in the
    /// mesh's canonical tuple order (ascending global index by default; lattice-canonical
    /// chain order in latticePeriod mode). Populated only when SimplicialDimension &gt;= 3
    /// (empty for 2D meshes). ad-valued 3-forms live on volumes.
    /// </summary>
    public int[][] Volumes { get; init; } = Array.Empty<int[]>();

    /// <summary>
    /// For each volume, its 4 boundary faces (2-subsimplices), stored in the
    /// omit-vertex order (omit v0, omit v1, omit v2, omit v3) i.e. faces
    /// {v1v2v3, v0v2v3, v0v1v3, v0v1v2}, each in canonical sorted form.
    /// </summary>
    public int[][] VolumeBoundaryFaces { get; init; } = Array.Empty<int[]>();

    /// <summary>
    /// Orientation signs for each volume's boundary faces:
    /// VolumeBoundaryOrientations[vol][i] = (-1)^i × (parity of the stored face tuple
    /// relative to the omitted-vertex triple). Both the default and the
    /// lattice-canonical conventions realize the pure {+1, -1, +1, -1} pattern
    /// (sub-tuples of ascending tuples are ascending; subchains of chains are chains).
    /// Discrete d: (d omega3form)[vol] uses these; d(2-form)-&gt;3-form is the
    /// transpose/coboundary and uses the SAME signs.
    /// </summary>
    public int[][] VolumeBoundaryOrientations { get; init; } = Array.Empty<int[]>();

    /// <summary>
    /// For each top cell (4-simplex / pentachoron), the indices of its volumes.
    /// CellVolumes[cellIdx] = the C(5,4)=5 volume indices (omit each vertex).
    /// Empty for meshes with SimplicialDimension &lt; 3.
    /// </summary>
    public int[][] CellVolumes { get; init; } = Array.Empty<int[]>();

    /// <summary>
    /// For each vertex, the indices of incident edges.
    /// VertexEdges[vertIdx] = array of edge indices touching this vertex.
    /// Used by the codifferential d* (maps 1-forms on edges to 0-forms on vertices).
    /// </summary>
    public required int[][] VertexEdges { get; init; }

    /// <summary>
    /// Orientation signs for vertex-edge incidence.
    /// VertexEdgeOrientations[vertIdx][i] = +1 if vertex is the first endpoint (v0) of VertexEdges[vertIdx][i],
    /// -1 if vertex is the second endpoint (v1).
    /// For edge {v0, v1} with v0 &lt; v1: vertex v0 gets +1, vertex v1 gets -1.
    /// Used by the codifferential: (d* omega)[v] = sum_i sign(v,e_i) * omega(e_i).
    /// </summary>
    public required int[][] VertexEdgeOrientations { get; init; }

    /// <summary>Number of top-level cells.</summary>
    public int CellCount => CellVertices.Length;

    /// <summary>Number of edges (1-subsimplices).</summary>
    public int EdgeCount => Edges.Length;

    /// <summary>Number of faces (2-subsimplices).</summary>
    public int FaceCount => Faces.Length;

    /// <summary>Number of volumes (3-subsimplices). Zero for 2D meshes.</summary>
    public int VolumeCount => Volumes.Length;

    /// <summary>
    /// Gets the coordinates of a vertex as a span.
    /// </summary>
    public ReadOnlySpan<double> GetVertexCoordinates(int vertexIndex)
    {
        int offset = vertexIndex * EmbeddingDimension;
        return new ReadOnlySpan<double>(VertexCoordinates, offset, EmbeddingDimension);
    }
}
