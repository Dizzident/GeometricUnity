using System.Runtime.InteropServices;

namespace Gu.Interop;

/// <summary>
/// Managed wrapper for the mesh descriptor passed to native/CUDA backends.
/// Encapsulates the mesh topology metadata (dimensions, counts) needed by
/// native compute kernels to correctly dispatch curvature, torsion, Shiab,
/// and residual computations.
///
/// Maps to the gu_manifest_snapshot_t on the native side.
///
/// Usage:
///   var desc = NativeMeshDescriptor.FromManifest(snapshot);
///   backend.Initialize(desc.ToManifestSnapshot());
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct NativeMeshDescriptor : IEquatable<NativeMeshDescriptor>
{
    /// <summary>Base manifold dimension dim(X). E.g. 4 for a 4-dimensional base.</summary>
    public int BaseDimension { get; init; }

    /// <summary>Ambient/total space dimension dim(Y). E.g. 14 for Y^14.</summary>
    public int AmbientDimension { get; init; }

    /// <summary>Lie algebra dimension dim(g). E.g. 3 for su(2), 8 for su(3).</summary>
    public int LieAlgebraDimension { get; init; }

    /// <summary>Number of mesh cells (top-dimensional simplices) in Y_h.</summary>
    public int MeshCellCount { get; init; }

    /// <summary>Number of mesh vertices (0-simplices) in Y_h.</summary>
    public int MeshVertexCount { get; init; }

    /// <summary>Number of mesh edges (1-simplices) in Y_h. Used for connection DOF sizing.</summary>
    public int MeshEdgeCount { get; init; }

    /// <summary>Number of mesh faces (2-simplices) in Y_h. Used for curvature DOF sizing.</summary>
    public int MeshFaceCount { get; init; }

    /// <summary>Fiber dimension: dim(Y) - dim(X).</summary>
    public int FiberDimension => AmbientDimension - BaseDimension;

    /// <summary>
    /// Total connection DOF count: EdgeCount * dim(g).
    /// This is the size of the omega coefficient buffer.
    /// </summary>
    public int ConnectionDofCount => MeshEdgeCount * LieAlgebraDimension;

    /// <summary>
    /// Total curvature DOF count: FaceCount * dim(g).
    /// This is the size of the curvature coefficient buffer.
    /// </summary>
    public int CurvatureDofCount => MeshFaceCount * LieAlgebraDimension;

    /// <summary>
    /// Create from a ManifestSnapshot, filling in edge and face counts.
    /// </summary>
    /// <param name="manifest">The manifest snapshot with basic dimensions.</param>
    /// <param name="edgeCount">Number of edges in the mesh.</param>
    /// <param name="faceCount">Number of faces in the mesh.</param>
    public static NativeMeshDescriptor FromManifest(
        ManifestSnapshot manifest,
        int edgeCount,
        int faceCount)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        return new NativeMeshDescriptor
        {
            BaseDimension = manifest.BaseDimension,
            AmbientDimension = manifest.AmbientDimension,
            LieAlgebraDimension = manifest.LieAlgebraDimension,
            MeshCellCount = manifest.MeshCellCount,
            MeshVertexCount = manifest.MeshVertexCount,
            MeshEdgeCount = edgeCount,
            MeshFaceCount = faceCount,
        };
    }

    /// <summary>
    /// Create directly from raw dimension values.
    /// </summary>
    public static NativeMeshDescriptor Create(
        int baseDimension,
        int ambientDimension,
        int lieAlgebraDimension,
        int meshCellCount,
        int meshVertexCount,
        int meshEdgeCount,
        int meshFaceCount)
    {
        return new NativeMeshDescriptor
        {
            BaseDimension = baseDimension,
            AmbientDimension = ambientDimension,
            LieAlgebraDimension = lieAlgebraDimension,
            MeshCellCount = meshCellCount,
            MeshVertexCount = meshVertexCount,
            MeshEdgeCount = meshEdgeCount,
            MeshFaceCount = meshFaceCount,
        };
    }

    /// <summary>
    /// Convert to ManifestSnapshot for backend initialization.
    /// </summary>
    public ManifestSnapshot ToManifestSnapshot(
        string lieAlgebraId = "su2",
        string componentOrderId = "order-row-major",
        string torsionBranchId = "trivial",
        string shiabBranchId = "identity")
    {
        return new ManifestSnapshot
        {
            BaseDimension = BaseDimension,
            AmbientDimension = AmbientDimension,
            LieAlgebraDimension = LieAlgebraDimension,
            LieAlgebraId = lieAlgebraId,
            MeshCellCount = MeshCellCount,
            MeshVertexCount = MeshVertexCount,
            ComponentOrderId = componentOrderId,
            TorsionBranchId = torsionBranchId,
            ShiabBranchId = shiabBranchId,
        };
    }

    /// <summary>
    /// Validate that all dimension values are positive and consistent.
    /// </summary>
    /// <exception cref="InvalidOperationException">If any dimension is invalid.</exception>
    public void Validate()
    {
        if (BaseDimension <= 0)
            throw new InvalidOperationException($"BaseDimension must be positive, got {BaseDimension}.");
        if (AmbientDimension <= BaseDimension)
            throw new InvalidOperationException(
                $"AmbientDimension ({AmbientDimension}) must exceed BaseDimension ({BaseDimension}).");
        if (LieAlgebraDimension <= 0)
            throw new InvalidOperationException($"LieAlgebraDimension must be positive, got {LieAlgebraDimension}.");
        if (MeshCellCount <= 0)
            throw new InvalidOperationException($"MeshCellCount must be positive, got {MeshCellCount}.");
        if (MeshVertexCount <= 0)
            throw new InvalidOperationException($"MeshVertexCount must be positive, got {MeshVertexCount}.");
        if (MeshEdgeCount < 0)
            throw new InvalidOperationException($"MeshEdgeCount must be non-negative, got {MeshEdgeCount}.");
        if (MeshFaceCount < 0)
            throw new InvalidOperationException($"MeshFaceCount must be non-negative, got {MeshFaceCount}.");
    }

    public bool Equals(NativeMeshDescriptor other) =>
        BaseDimension == other.BaseDimension &&
        AmbientDimension == other.AmbientDimension &&
        LieAlgebraDimension == other.LieAlgebraDimension &&
        MeshCellCount == other.MeshCellCount &&
        MeshVertexCount == other.MeshVertexCount &&
        MeshEdgeCount == other.MeshEdgeCount &&
        MeshFaceCount == other.MeshFaceCount;

    public override bool Equals(object? obj) => obj is NativeMeshDescriptor other && Equals(other);

    public override int GetHashCode() =>
        HashCode.Combine(BaseDimension, AmbientDimension, LieAlgebraDimension,
            MeshCellCount, MeshVertexCount, MeshEdgeCount, MeshFaceCount);

    public static bool operator ==(NativeMeshDescriptor left, NativeMeshDescriptor right) => left.Equals(right);
    public static bool operator !=(NativeMeshDescriptor left, NativeMeshDescriptor right) => !left.Equals(right);

    public override string ToString() =>
        $"MeshDescriptor(dim={BaseDimension}/{AmbientDimension}, g={LieAlgebraDimension}, " +
        $"cells={MeshCellCount}, verts={MeshVertexCount}, edges={MeshEdgeCount}, faces={MeshFaceCount})";
}
