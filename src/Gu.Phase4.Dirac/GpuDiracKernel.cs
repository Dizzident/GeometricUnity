namespace Gu.Phase4.Dirac;

/// <summary>
/// GPU implementation of IDiracKernel.
///
/// When a real CUDA backend is available, this class would dispatch Dirac
/// operator actions to GPU kernels. Until the CUDA native library is linked
/// (see IA-5: CPU reference before CUDA trust), all operations fall back
/// transparently to the CPU reference kernel.
///
/// This follows the same pattern as GpuSpectralKernel in Gu.Phase3.CudaSpectra:
/// wrap a CPU fallback, expose ComputedWithCuda = false until real CUDA is active,
/// and pass all parity tests trivially (fallback == reference).
///
/// TODO(M44-GPU): when the CUDA native library is available, replace the
/// fallback delegation in each method with P/Invoke calls to:
///   gu_dirac_gamma_action_gpu
///   gu_dirac_apply_gpu
///   gu_dirac_mass_apply_gpu
///   gu_dirac_chirality_project_gpu
///   gu_dirac_coupling_proxy_gpu
/// </summary>
public sealed class GpuDiracKernel : IDiracKernel, IDisposable
{
    private readonly IDiracKernel _fallback;
    private bool _disposed;

    /// <summary>
    /// Create a GPU Dirac kernel that falls back to the provided CPU reference kernel.
    /// When real CUDA becomes available, the backend parameter will be used here.
    /// </summary>
    /// <param name="cpuFallback">CPU reference kernel (always available).</param>
    public GpuDiracKernel(CpuDiracKernel cpuFallback)
    {
        _fallback = cpuFallback ?? throw new ArgumentNullException(nameof(cpuFallback));
    }

    /// <inheritdoc />
    public int SpinorDimension => _fallback.SpinorDimension;

    /// <inheritdoc />
    public int SpacetimeDimension => _fallback.SpacetimeDimension;

    /// <summary>
    /// True if a real CUDA backend is active.
    /// Currently always false — CUDA native library not yet linked (IA-5).
    /// </summary>
    public bool IsCudaActive => false;

    /// <inheritdoc />
    public void ApplyGamma(int mu, ReadOnlySpan<double> spinor, Span<double> result)
    {
        EnsureNotDisposed();
        // CPU fallback — TODO(M44-GPU): dispatch gu_dirac_gamma_action_gpu when available
        _fallback.ApplyGamma(mu, spinor, result);
    }

    /// <inheritdoc />
    public void ApplyDirac(ReadOnlySpan<double> spinor, Span<double> result)
    {
        EnsureNotDisposed();
        // CPU fallback — TODO(M44-GPU): dispatch gu_dirac_apply_gpu when available
        _fallback.ApplyDirac(spinor, result);
    }

    /// <inheritdoc />
    public void ApplyMass(ReadOnlySpan<double> spinor, Span<double> result)
    {
        EnsureNotDisposed();
        // CPU fallback — TODO(M44-GPU): dispatch gu_dirac_mass_apply_gpu when available
        _fallback.ApplyMass(spinor, result);
    }

    /// <inheritdoc />
    public void ApplyChiralityProjector(bool left, ReadOnlySpan<double> spinor, Span<double> result)
    {
        EnsureNotDisposed();
        // CPU fallback — TODO(M44-GPU): dispatch gu_dirac_chirality_project_gpu when available
        _fallback.ApplyChiralityProjector(left, spinor, result);
    }

    /// <inheritdoc />
    public (double Real, double Imag) ComputeCouplingProxy(
        ReadOnlySpan<double> spinorI,
        ReadOnlySpan<double> spinorJ,
        ReadOnlySpan<double> bosonK)
    {
        EnsureNotDisposed();
        // CPU fallback — TODO(M44-GPU): dispatch gu_dirac_coupling_proxy_gpu when available
        return _fallback.ComputeCouplingProxy(spinorI, spinorJ, bosonK);
    }

    private void EnsureNotDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (!_disposed)
            _disposed = true;
    }
}
