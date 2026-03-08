namespace Gu.Interop;

/// <summary>
/// Lightweight snapshot of branch manifest data needed by the native backend
/// for initialization. Avoids passing the full BranchManifest across the interop boundary.
/// </summary>
public sealed class ManifestSnapshot
{
    /// <summary>Base manifold dimension dim(X).</summary>
    public required int BaseDimension { get; init; }

    /// <summary>Ambient/total space dimension dim(Y).</summary>
    public required int AmbientDimension { get; init; }

    /// <summary>Fiber dimension dim(Y) - dim(X).</summary>
    public int FiberDimension => AmbientDimension - BaseDimension;

    /// <summary>Lie algebra dimension (e.g., 3 for su(2), 8 for su(3)).</summary>
    public required int LieAlgebraDimension { get; init; }

    /// <summary>Lie algebra identifier.</summary>
    public required string LieAlgebraId { get; init; }

    /// <summary>Number of mesh cells/elements.</summary>
    public required int MeshCellCount { get; init; }

    /// <summary>Number of mesh vertices.</summary>
    public required int MeshVertexCount { get; init; }

    /// <summary>Component order convention ID.</summary>
    public required string ComponentOrderId { get; init; }

    /// <summary>Active torsion branch identifier.</summary>
    public required string TorsionBranchId { get; init; }

    /// <summary>Active Shiab branch identifier.</summary>
    public required string ShiabBranchId { get; init; }

    /// <summary>
    /// Create from a BranchManifest and mesh dimensions.
    /// </summary>
    public static ManifestSnapshot FromManifest(
        Core.BranchManifest manifest,
        int lieAlgebraDimension,
        int meshCellCount,
        int meshVertexCount)
    {
        return new ManifestSnapshot
        {
            BaseDimension = manifest.BaseDimension,
            AmbientDimension = manifest.AmbientDimension,
            LieAlgebraDimension = lieAlgebraDimension,
            LieAlgebraId = manifest.LieAlgebraId,
            MeshCellCount = meshCellCount,
            MeshVertexCount = meshVertexCount,
            ComponentOrderId = manifest.ComponentOrderId,
            TorsionBranchId = manifest.ActiveTorsionBranch,
            ShiabBranchId = manifest.ActiveShiabBranch,
        };
    }
}
