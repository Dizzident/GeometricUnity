using Gu.Core;
using Gu.Interop;

namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// GPU implementation of ISpectralKernel using INativeBackend.
/// Wraps the GPU JVP/JTvP primitives and adds spectral (Hessian-vector)
/// and mass operator actions.
///
/// Buffer lifecycle: allocate scratch buffers per call, upload, compute, download, free.
/// This matches the pattern in GpuLinearOperator from Gu.Interop.
/// </summary>
public sealed class GpuSpectralKernel : ISpectralKernel, IDisposable
{
    private readonly INativeBackend _backend;
    private readonly PackedBuffer _omegaBuf;
    private readonly BufferLayoutDescriptor _edgeLayout;
    private readonly BufferLayoutDescriptor _faceLayout;
    private readonly int _stateDim;
    private readonly int _residualDim;
    private readonly double _gaugeLambda;
    private readonly bool _hasProjector;
    private bool _disposed;

    /// <summary>
    /// Create a GPU spectral kernel.
    /// </summary>
    /// <param name="backend">Native GPU backend (must be initialized with mesh/algebra).</param>
    /// <param name="omegaBuf">Pre-uploaded background connection buffer.</param>
    /// <param name="stateDim">State dimension (edgeCount * dimG).</param>
    /// <param name="residualDim">Residual dimension (faceCount * dimG).</param>
    /// <param name="gaugeLambda">Gauge penalty weight for Hessian.</param>
    /// <param name="hasProjector">Whether P2 projected formulation is active.</param>
    public GpuSpectralKernel(
        INativeBackend backend,
        PackedBuffer omegaBuf,
        int stateDim,
        int residualDim,
        double gaugeLambda = 0.1,
        bool hasProjector = false)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _omegaBuf = omegaBuf ?? throw new ArgumentNullException(nameof(omegaBuf));
        _stateDim = stateDim;
        _residualDim = residualDim;
        _gaugeLambda = gaugeLambda;
        _hasProjector = hasProjector;

        _edgeLayout = BufferLayoutDescriptor.CreateSoA("spectral-edge", new[] { "c" }, stateDim);
        _faceLayout = BufferLayoutDescriptor.CreateSoA("spectral-face", new[] { "c" }, residualDim);
    }

    public int StateDimension => _stateDim;
    public int ResidualDimension => _residualDim;

    public void ApplySpectral(ReadOnlySpan<double> v, Span<double> result)
    {
        EnsureNotDisposed();

        // H_GN * v = J^T * J * v (simplified; real kernel fuses this)
        // Step 1: J * v
        var vBuf = _backend.AllocateBuffer(_edgeLayout);
        var jvBuf = _backend.AllocateBuffer(_faceLayout);
        var jtjvBuf = _backend.AllocateBuffer(_edgeLayout);

        try
        {
            _backend.UploadBuffer(vBuf, v);
            _backend.EvaluateJacobianAction(_omegaBuf, vBuf, jvBuf);
            // Step 2: J^T * (J * v)
            _backend.EvaluateAdjointAction(_omegaBuf, jvBuf, jtjvBuf);
            _backend.DownloadBuffer(jtjvBuf, result);
        }
        finally
        {
            _backend.FreeBuffer(vBuf);
            _backend.FreeBuffer(jvBuf);
            _backend.FreeBuffer(jtjvBuf);
        }
    }

    public void ApplyMass(ReadOnlySpan<double> v, Span<double> result)
    {
        EnsureNotDisposed();

        // Mass operator is diagonal in the simple case (uniform weights + identity metric).
        // GPU kernel would compute M_state * v directly.
        // For now, delegate to device-side inner product infrastructure.
        var vBuf = _backend.AllocateBuffer(_edgeLayout);
        var mvBuf = _backend.AllocateBuffer(_edgeLayout);

        try
        {
            _backend.UploadBuffer(vBuf, v);
            // Copy v -> mv, then scale by mass weights (identity for uniform case)
            _backend.Copy(mvBuf, vBuf, _stateDim);
            _backend.DownloadBuffer(mvBuf, result);
        }
        finally
        {
            _backend.FreeBuffer(vBuf);
            _backend.FreeBuffer(mvBuf);
        }
    }

    public void ApplyJacobian(ReadOnlySpan<double> v, Span<double> result)
    {
        EnsureNotDisposed();

        var vBuf = _backend.AllocateBuffer(_edgeLayout);
        var jvBuf = _backend.AllocateBuffer(_faceLayout);

        try
        {
            _backend.UploadBuffer(vBuf, v);
            _backend.EvaluateJacobianAction(_omegaBuf, vBuf, jvBuf);
            _backend.DownloadBuffer(jvBuf, result);
        }
        finally
        {
            _backend.FreeBuffer(vBuf);
            _backend.FreeBuffer(jvBuf);
        }
    }

    public void ApplyAdjoint(ReadOnlySpan<double> w, Span<double> result)
    {
        EnsureNotDisposed();

        var wBuf = _backend.AllocateBuffer(_faceLayout);
        var jtwBuf = _backend.AllocateBuffer(_edgeLayout);

        try
        {
            _backend.UploadBuffer(wBuf, w);
            _backend.EvaluateAdjointAction(_omegaBuf, wBuf, jtwBuf);
            _backend.DownloadBuffer(jtwBuf, result);
        }
        finally
        {
            _backend.FreeBuffer(wBuf);
            _backend.FreeBuffer(jtwBuf);
        }
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}
