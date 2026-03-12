
using Gu.Phase4.Fermions;

namespace Gu.Phase4.CudaAcceleration;

/// <summary>
/// Factory for creating IDiracKernel instances.
///
/// Returns a CpuDiracKernel or GpuDiracKernelStub depending on whether
/// CUDA is requested and available.
///
/// In Phase IV there is no real CUDA Dirac kernel implemented yet
/// (CUDA is reserved for the Phase III spectral solver path). This
/// factory always returns the stub when GPU is requested, and sets
/// VerificationStatus = "stub-unverified" accordingly.
///
/// Usage:
///   var cpu = DiracKernelFactory.CreateCpu(bundle);
///   var gpu = DiracKernelFactory.CreateGpu(bundle);
///   var report = DiracParityChecker.Check(cpu, gpu, ...);
/// </summary>
public static class DiracKernelFactory
{
    /// <summary>
    /// Create a CPU Dirac kernel from an operator bundle.
    /// </summary>
    public static CpuDiracKernel CreateCpu(
        Gu.Phase4.Dirac.DiracOperatorBundle bundle,
        IReadOnlyList<FermionModeRecord>? modes = null)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        return new CpuDiracKernel(bundle, modes);
    }

    /// <summary>
    /// Create a GPU (stub) Dirac kernel from an operator bundle.
    ///
    /// Returns a GpuDiracKernelStub — CPU code that claims CUDA for interface
    /// compatibility. Real CUDA bindings are out of scope for Phase IV.
    /// </summary>
    public static GpuDiracKernelStub CreateGpu(
        Gu.Phase4.Dirac.DiracOperatorBundle bundle,
        IReadOnlyList<FermionModeRecord>? modes = null)
    {
        ArgumentNullException.ThrowIfNull(bundle);
        return new GpuDiracKernelStub(bundle, modes);
    }

    /// <summary>
    /// Create the appropriate kernel based on configuration.
    /// If useCuda is true and no real CUDA is available, returns a stub.
    /// </summary>
    public static IDiracKernel Create(
        Gu.Phase4.Dirac.DiracOperatorBundle bundle,
        bool useCuda,
        IReadOnlyList<FermionModeRecord>? modes = null)
    {
        return useCuda
            ? CreateGpu(bundle, modes)
            : CreateCpu(bundle, modes);
    }
}
