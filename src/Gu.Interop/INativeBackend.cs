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
}
