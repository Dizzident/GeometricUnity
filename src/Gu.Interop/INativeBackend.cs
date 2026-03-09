namespace Gu.Interop;

/// <summary>
/// Interface for the native compute backend (CUDA or CPU-reference).
/// Operates on packed buffers at the GPU abstraction level.
/// This is SEPARATE from ISolverBackend (which operates on semantic types).
/// CudaSolverBackend wraps INativeBackend via composition.
///
/// All methods operate on pre-allocated PackedBuffers.
/// Buffer lifecycle: AllocateBuffer -> UploadBuffer -> compute -> DownloadBuffer -> FreeBuffer.
/// </summary>
public interface INativeBackend : IDisposable
{
    /// <summary>Version information for compatibility checking.</summary>
    InteropVersion Version { get; }

    /// <summary>
    /// Initialize the backend with manifest information.
    /// Must be called before any compute operations.
    /// </summary>
    void Initialize(ManifestSnapshot manifest);

    /// <summary>
    /// Allocate a packed buffer on the native/GPU side.
    /// </summary>
    PackedBuffer AllocateBuffer(BufferLayoutDescriptor layout);

    /// <summary>
    /// Upload data from CPU to a native/GPU buffer.
    /// </summary>
    void UploadBuffer(PackedBuffer buffer, ReadOnlySpan<double> data);

    /// <summary>
    /// Download data from a native/GPU buffer to CPU.
    /// </summary>
    void DownloadBuffer(PackedBuffer buffer, Span<double> data);

    /// <summary>
    /// Free a native/GPU buffer.
    /// </summary>
    void FreeBuffer(PackedBuffer buffer);

    /// <summary>
    /// Compute curvature F from connection omega.
    /// F = d(omega) + (1/2)[omega, omega]
    /// </summary>
    void EvaluateCurvature(PackedBuffer omega, PackedBuffer curvatureOut);

    /// <summary>
    /// Compute torsion T from connection omega (branch-dependent).
    /// </summary>
    void EvaluateTorsion(PackedBuffer omega, PackedBuffer torsionOut);

    /// <summary>
    /// Compute Shiab operator S from connection omega (branch-dependent).
    /// </summary>
    void EvaluateShiab(PackedBuffer omega, PackedBuffer shiabOut);

    /// <summary>
    /// Compute residual Upsilon = S - T from Shiab and torsion buffers.
    /// </summary>
    void EvaluateResidual(PackedBuffer shiab, PackedBuffer torsion, PackedBuffer residualOut);

    /// <summary>
    /// Compute the objective value I2_h = (1/2) integral &lt;Upsilon, Upsilon&gt; dmu_Y.
    /// Returns a scalar.
    /// </summary>
    double EvaluateObjective(PackedBuffer residual);

    /// <summary>
    /// Get the last error from the native side, if any.
    /// Returns null if no error occurred.
    /// </summary>
    ErrorPacket? GetLastError();

    /// <summary>
    /// Upload mesh topology data (face-boundary-edge incidence, orientations, edge-vertex connectivity).
    /// Required before physics kernels can execute real computations.
    /// </summary>
    void UploadMeshTopology(MeshTopologyData topology);

    /// <summary>
    /// Upload Lie algebra structure constants and invariant metric.
    /// Required before physics kernels can execute real computations.
    /// </summary>
    void UploadAlgebraData(AlgebraUploadData algebra);

    /// <summary>
    /// Upload background connection A0 coefficients.
    /// </summary>
    void UploadBackgroundConnection(ReadOnlySpan<double> a0Coefficients, int edgeCount, int dimG);

    /// <summary>
    /// Query whether mesh topology and algebra data have been uploaded.
    /// </summary>
    bool HasPhysicsData { get; }

    /// <summary>
    /// Compute Jacobian-vector product: Jv = (dUpsilon/domega) * delta.
    /// The Jacobian is the linearization of the residual map at the current omega.
    /// dUpsilon/domega = dS/domega - dT/domega.
    /// For identity Shiab + trivial torsion: J*delta = d(delta) + 0.5*sum_{i&lt;j}([omega_i,delta_j]+[delta_i,omega_j]).
    /// </summary>
    /// <param name="omega">Current connection (edge-valued).</param>
    /// <param name="delta">Perturbation direction (edge-valued).</param>
    /// <param name="jvOut">Output: J*delta (face-valued).</param>
    void EvaluateJacobianAction(PackedBuffer omega, PackedBuffer delta, PackedBuffer jvOut);

    /// <summary>
    /// Compute adjoint (transpose) action: JTv = (dUpsilon/domega)^T * v.
    /// Maps a face-valued field to an edge-valued field.
    /// </summary>
    /// <param name="omega">Current connection (edge-valued).</param>
    /// <param name="v">Input vector (face-valued).</param>
    /// <param name="jtvOut">Output: J^T*v (edge-valued).</param>
    void EvaluateAdjointAction(PackedBuffer omega, PackedBuffer v, PackedBuffer jtvOut);

    // --- GPU solver primitives (BLAS-like) for on-device Krylov iterations ---

    /// <summary>
    /// AXPY: y = y + alpha * x (in-place on y buffer).
    /// </summary>
    void Axpy(PackedBuffer y, double alpha, PackedBuffer x, int n);

    /// <summary>
    /// Compute inner product: result = sum(u[i] * v[i]).
    /// </summary>
    double InnerProduct(PackedBuffer u, PackedBuffer v, int n);

    /// <summary>
    /// Scale: x = alpha * x (in-place).
    /// </summary>
    void Scale(PackedBuffer x, double alpha, int n);

    /// <summary>
    /// Copy: dst = src.
    /// </summary>
    void Copy(PackedBuffer dst, PackedBuffer src, int n);
}
