using System.Runtime.InteropServices;

namespace Gu.Interop;

/// <summary>
/// P/Invoke bindings to the native gu_cuda_core library.
/// These match the C API defined in gu_interop_types.h.
/// </summary>
internal static partial class NativeBindings
{
    private const string LibName = "gu_cuda_core";

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeInteropVersion
    {
        public int Major;
        public int Minor;
        public int Patch;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeManifestSnapshot
    {
        public int BaseDimension;
        public int AmbientDimension;
        public int LieAlgebraDimension;
        public int MeshCellCount;
        public int MeshVertexCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct NativeMeshTopologyHeader
    {
        public int EdgeCount;
        public int FaceCount;
        public int VertexCount;
        public int EmbeddingDimension;
        public int MaxEdgesPerFace;
        public int DimG;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    internal struct NativeErrorPacket
    {
        public int Code;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string Message;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
        public string Source;
    }

    // Lifecycle
    [LibraryImport(LibName, EntryPoint = "gu_initialize")]
    internal static partial int Initialize(in NativeManifestSnapshot manifest);

    [LibraryImport(LibName, EntryPoint = "gu_shutdown")]
    internal static partial void Shutdown();

    [LibraryImport(LibName, EntryPoint = "gu_get_version")]
    internal static partial NativeInteropVersion GetVersion();

    // Buffer management
    [LibraryImport(LibName, EntryPoint = "gu_allocate_buffer")]
    internal static partial int AllocateBuffer(int totalElements, int bytesPerElement);

    [LibraryImport(LibName, EntryPoint = "gu_upload_buffer")]
    internal static partial int UploadBuffer(int handle, nint data, nuint byteCount);

    [LibraryImport(LibName, EntryPoint = "gu_download_buffer")]
    internal static partial int DownloadBuffer(int handle, nint data, nuint byteCount);

    [LibraryImport(LibName, EntryPoint = "gu_free_buffer")]
    internal static partial int FreeBuffer(int handle);

    // Compute kernels
    [LibraryImport(LibName, EntryPoint = "gu_evaluate_curvature")]
    internal static partial int EvaluateCurvature(int omega, int curvatureOut);

    [LibraryImport(LibName, EntryPoint = "gu_evaluate_torsion")]
    internal static partial int EvaluateTorsion(int omega, int torsionOut);

    [LibraryImport(LibName, EntryPoint = "gu_evaluate_shiab")]
    internal static partial int EvaluateShiab(int omega, int shiabOut);

    [LibraryImport(LibName, EntryPoint = "gu_evaluate_residual")]
    internal static partial int EvaluateResidual(int shiab, int torsion, int residualOut);

    [LibraryImport(LibName, EntryPoint = "gu_evaluate_objective")]
    internal static partial int EvaluateObjective(int residual, out double objectiveOut);

    // Solver primitives (M10)
    [LibraryImport(LibName, EntryPoint = "gu_axpy")]
    internal static partial int Axpy(int y, double alpha, int x, int n);

    [LibraryImport(LibName, EntryPoint = "gu_inner_product")]
    internal static partial int InnerProduct(int u, int v, int n, out double result);

    [LibraryImport(LibName, EntryPoint = "gu_scale")]
    internal static partial int Scale(int x, double alpha, int n);

    [LibraryImport(LibName, EntryPoint = "gu_copy")]
    internal static partial int Copy(int dst, int src, int n);

    // Jacobian/adjoint operations (CUDA Stage 2)
    [LibraryImport(LibName, EntryPoint = "gu_evaluate_jacobian_action")]
    internal static partial int EvaluateJacobianAction(int omega, int delta, int jvOut);

    [LibraryImport(LibName, EntryPoint = "gu_evaluate_adjoint_action")]
    internal static partial int EvaluateAdjointAction(int omega, int v, int jtvOut);

    // Error reporting
    [LibraryImport(LibName, EntryPoint = "gu_get_last_error")]
    internal static partial nint GetLastError();

    // Extended data upload (GAP-9)
    [LibraryImport(LibName, EntryPoint = "gu_upload_mesh_topology")]
    internal static partial int UploadMeshTopology(
        in NativeMeshTopologyHeader header,
        nint faceBoundaryEdges,
        nint faceBoundaryOrientations,
        nint edgeVertices);

    [LibraryImport(LibName, EntryPoint = "gu_upload_vertex_coordinates")]
    internal static partial int UploadVertexCoordinates(
        nint vertexCoords,
        int vertexCount,
        int embeddingDim);

    [LibraryImport(LibName, EntryPoint = "gu_upload_structure_constants")]
    internal static partial int UploadStructureConstants(
        nint structureConstants,
        int dim);

    [LibraryImport(LibName, EntryPoint = "gu_upload_invariant_metric")]
    internal static partial int UploadInvariantMetric(
        nint metric,
        int dim);

    [LibraryImport(LibName, EntryPoint = "gu_upload_background_connection")]
    internal static partial int UploadBackgroundConnection(
        nint a0Coefficients,
        int edgeCount,
        int dimG);

    [LibraryImport(LibName, EntryPoint = "gu_has_physics_data")]
    internal static partial int HasPhysicsData();
}
