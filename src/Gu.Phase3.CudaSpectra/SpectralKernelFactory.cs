using Gu.Phase3.Spectra;

namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// Factory for creating ISpectralKernel instances.
/// Follows IA-5 (CPU reference before CUDA trust): always produces a
/// CpuSpectralKernel as the reference, and optionally a GPU kernel
/// when CUDA hardware is available and not disabled by config.
/// </summary>
public sealed class SpectralKernelFactory
{
    private readonly CudaSpectralConfig _config;

    public SpectralKernelFactory(CudaSpectralConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Create the CPU reference kernel from a linearized operator bundle.
    /// This is always available regardless of GPU status.
    /// </summary>
    public CpuSpectralKernel CreateCpuKernel(LinearizedOperatorBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        return new CpuSpectralKernel(bundle);
    }

    /// <summary>
    /// Create the best available kernel for the current environment.
    /// Returns GPU kernel if CUDA is available and not force-CPU;
    /// otherwise returns CPU reference kernel.
    /// </summary>
    public ISpectralKernel CreateKernel(LinearizedOperatorBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        if (_config.ForceCpu || !IsCudaAvailable())
        {
            return CreateCpuKernel(bundle);
        }

        // GPU path: in production this would create a GpuSpectralKernel
        // backed by the native CUDA backend. For now, fall back to CPU
        // since the CUDA native library is not yet linked.
        return CreateCpuKernel(bundle);
    }

    /// <summary>
    /// Whether CUDA GPU acceleration is available in the current environment.
    /// Returns false until native CUDA library is linked.
    /// </summary>
    public static bool IsCudaAvailable()
    {
        // CUDA availability is determined by native library probing.
        // Until the native shared library is built and deployed,
        // this always returns false -- enforcing CPU-reference-first (IA-5).
        return false;
    }

    /// <summary>Configuration used by this factory.</summary>
    public CudaSpectralConfig Config => _config;
}
