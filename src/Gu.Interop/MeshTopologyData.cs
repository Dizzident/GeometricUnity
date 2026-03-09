using Gu.Geometry;

namespace Gu.Interop;

/// <summary>
/// Packed mesh topology data for upload to the native backend.
/// All arrays are flat (1D) with padding for uniform stride.
/// </summary>
public sealed class MeshTopologyData
{
    /// <summary>Number of edges in the mesh.</summary>
    public required int EdgeCount { get; init; }

    /// <summary>Number of faces in the mesh.</summary>
    public required int FaceCount { get; init; }

    /// <summary>Number of vertices in the mesh.</summary>
    public required int VertexCount { get; init; }

    /// <summary>Embedding space dimension.</summary>
    public required int EmbeddingDimension { get; init; }

    /// <summary>Maximum number of boundary edges per face (typically 3 for triangles).</summary>
    public required int MaxEdgesPerFace { get; init; }

    /// <summary>Lie algebra dimension.</summary>
    public required int DimG { get; init; }

    /// <summary>
    /// Face boundary edge indices, flat array [FaceCount * MaxEdgesPerFace].
    /// Padded with -1 for faces with fewer boundary edges.
    /// </summary>
    public required int[] FaceBoundaryEdges { get; init; }

    /// <summary>
    /// Face boundary orientations, flat array [FaceCount * MaxEdgesPerFace].
    /// Each entry is +1 or -1.
    /// </summary>
    public required int[] FaceBoundaryOrientations { get; init; }

    /// <summary>
    /// Edge vertex indices, flat array [EdgeCount * 2].
    /// Each edge: {v0, v1} with v0 less than v1.
    /// </summary>
    public required int[] EdgeVertices { get; init; }

    /// <summary>
    /// Vertex coordinates, flat array [VertexCount * EmbeddingDimension].
    /// May be null if vertex positions are not needed.
    /// </summary>
    public double[]? VertexCoordinates { get; init; }

    /// <summary>
    /// Create MeshTopologyData from a SimplicialMesh and Lie algebra dimension.
    /// Packs jagged arrays into flat padded arrays.
    /// </summary>
    public static MeshTopologyData FromMesh(SimplicialMesh mesh, int dimG)
    {
        ArgumentNullException.ThrowIfNull(mesh);

        int maxEdgesPerFace = 0;
        for (int i = 0; i < mesh.FaceCount; i++)
        {
            if (mesh.FaceBoundaryEdges[i].Length > maxEdgesPerFace)
                maxEdgesPerFace = mesh.FaceBoundaryEdges[i].Length;
        }

        var flatEdges = new int[mesh.FaceCount * maxEdgesPerFace];
        var flatOrients = new int[mesh.FaceCount * maxEdgesPerFace];

        // Initialize with -1 padding
        Array.Fill(flatEdges, -1);
        Array.Fill(flatOrients, 0);

        for (int fi = 0; fi < mesh.FaceCount; fi++)
        {
            var edges = mesh.FaceBoundaryEdges[fi];
            var orients = mesh.FaceBoundaryOrientations[fi];
            for (int j = 0; j < edges.Length; j++)
            {
                flatEdges[fi * maxEdgesPerFace + j] = edges[j];
                flatOrients[fi * maxEdgesPerFace + j] = orients[j];
            }
        }

        var flatEdgeVerts = new int[mesh.EdgeCount * 2];
        for (int ei = 0; ei < mesh.EdgeCount; ei++)
        {
            flatEdgeVerts[ei * 2] = mesh.Edges[ei][0];
            flatEdgeVerts[ei * 2 + 1] = mesh.Edges[ei][1];
        }

        return new MeshTopologyData
        {
            EdgeCount = mesh.EdgeCount,
            FaceCount = mesh.FaceCount,
            VertexCount = mesh.VertexCount,
            EmbeddingDimension = mesh.EmbeddingDimension,
            MaxEdgesPerFace = maxEdgesPerFace,
            DimG = dimG,
            FaceBoundaryEdges = flatEdges,
            FaceBoundaryOrientations = flatOrients,
            EdgeVertices = flatEdgeVerts,
            VertexCoordinates = (double[])mesh.VertexCoordinates.Clone(),
        };
    }
}
