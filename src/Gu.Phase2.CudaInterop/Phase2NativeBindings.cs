using System.Runtime.InteropServices;

namespace Gu.Phase2.CudaInterop;

/// <summary>
/// P/Invoke bindings for the Phase II CUDA native library (libgu_phase2_cuda).
/// All functions return 0 on success, non-zero on error.
/// </summary>
internal static partial class Phase2NativeBindings
{
    private const string LibName = "libgu_phase2_cuda";

    [StructLayout(LayoutKind.Sequential)]
    internal struct Phase2KernelConfig
    {
        public int EdgeCount;
        public int FaceCount;
        public int DimG;
        public int MaxEdgesPerFace;
    }

    /// <summary>Initialize Phase II CUDA kernels with geometry configuration.</summary>
    [LibraryImport(LibName, EntryPoint = "gu_phase2_init")]
    internal static partial int Initialize(in Phase2KernelConfig config);

    /// <summary>Shutdown and free Phase II resources.</summary>
    [LibraryImport(LibName, EntryPoint = "gu_phase2_shutdown")]
    internal static partial int Shutdown();

    [LibraryImport(LibName, EntryPoint = "gu_phase2_upload_face_topology")]
    internal static partial int UploadFaceTopology(
        nint faceBoundaryEdges,
        nint faceBoundaryOrientations,
        int faceCount,
        int maxEdgesPerFace);

    [LibraryImport(LibName, EntryPoint = "gu_phase2_upload_structure_constants")]
    internal static partial int UploadStructureConstants(
        nint structureConstants,
        int dimG);

    [LibraryImport(LibName, EntryPoint = "gu_phase2_upload_background_connection")]
    internal static partial int UploadBackgroundConnection(
        nint a0,
        int edgeCount,
        int dimG);

    /// <summary>Compute Jv: Jacobian action at connection u on direction v.</summary>
    [LibraryImport(LibName, EntryPoint = "gu_phase2_jacobian_action")]
    internal static partial int JacobianAction(
        nint u, nint v, nint result,
        int edgeCount, int faceCount, int dimG,
        int branchFlags);

    /// <summary>Compute J^Tw: adjoint action at connection u on vector w.</summary>
    [LibraryImport(LibName, EntryPoint = "gu_phase2_adjoint_action")]
    internal static partial int AdjointAction(
        nint u, nint w, nint result,
        int edgeCount, int faceCount, int dimG,
        int branchFlags);

    /// <summary>Compute Hv: Hessian action at connection u on direction v.</summary>
    [LibraryImport(LibName, EntryPoint = "gu_phase2_hessian_action")]
    internal static partial int HessianAction(
        nint u, nint v, nint result,
        int edgeCount, int faceCount, int dimG,
        double lambda, int branchFlags);

    /// <summary>Batched residual evaluation for N branch variants.</summary>
    [LibraryImport(LibName, EntryPoint = "gu_phase2_batch_residual")]
    internal static partial int BatchResidual(
        nint connectionStates, nint residualsOut,
        int batchSize, int fieldDof, int residualDof,
        nint branchFlagsArray);
}
