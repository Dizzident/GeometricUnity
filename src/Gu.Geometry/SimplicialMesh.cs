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
    /// Faces[faceIdx] = { v0, v1, v2 } with v0 &lt; v1 &lt; v2 (canonical ordering).
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

    /// <summary>
    /// Gets the coordinates of a vertex as a span.
    /// </summary>
    public ReadOnlySpan<double> GetVertexCoordinates(int vertexIndex)
    {
        int offset = vertexIndex * EmbeddingDimension;
        return new ReadOnlySpan<double>(VertexCoordinates, offset, EmbeddingDimension);
    }
}
