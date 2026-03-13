
using Gu.Phase4.Fermions;

namespace Gu.Phase4.CudaAcceleration;

/// <summary>
/// GPU stub implementation of IDiracKernel for M44 parity testing.
///
/// In environments without real CUDA (e.g., CI), this stub delegates all
/// computations to the same CPU code as CpuDiracKernel.
///
/// CI NOTE (M44-GPU deferred): Real CUDA dispatch is not implemented in Phase IV.
/// All parity tests in Gu.Phase4.CudaAcceleration.Tests are CPU-vs-CPU comparisons.
/// When CUDA hardware and a native library are available, this stub should be replaced
/// by a P/Invoke kernel (see Gu.Phase2.CudaInterop for the precedent pattern) and
/// ComputedWithCuda should return true only after VerificationStatus is confirmed.
///
/// PhysicsNote (M44): CPU/GPU parity is declared on the interface. Until a real
/// CUDA kernel is linked, VerificationStatus = "stub-unverified", ComputedWithCuda = false,
/// and any FermionSpectralResult produced must set ComputedWithUnverifiedGpu = true.
/// </summary>
public sealed class GpuDiracKernelStub : IDiracKernel
{
    private readonly CpuDiracKernel _cpuDelegate;

    public string VerificationStatus { get; } = "stub-unverified";

    public GpuDiracKernelStub(
        Gu.Phase4.Dirac.DiracOperatorBundle opBundle,
        IReadOnlyList<FermionModeRecord>? modes = null)
    {
        _cpuDelegate = new CpuDiracKernel(opBundle, modes);
    }

    /// <inheritdoc/>
    public int TotalDof => _cpuDelegate.TotalDof;

    /// <inheritdoc/>
    /// <remarks>
    /// Returns false: this stub delegates to CPU code and is not backed by a real CUDA kernel.
    /// Downstream code that sets FermionModeRecord.ComputedWithUnverifiedGpu should check
    /// ComputedWithCuda and/or VerificationStatus to determine the correct flag value.
    /// TODO(M44-GPU): set to true once a real CUDA kernel is wired via P/Invoke.
    /// </remarks>
    public bool ComputedWithCuda => false; // CPU fallback stub — not a real CUDA kernel

    // Each method below maps to a native function in native/gu_cuda_kernels/include/gu_cuda_kernels.h.
    // When CUDA is available, these should dispatch to the corresponding gu_dirac_* P/Invoke:
    //   ApplyDirac          → gu_dirac_apply_gpu
    //   ApplyMassPsi        → gu_dirac_mass_apply_gpu
    //   ProjectLeft/Right   → gu_dirac_chirality_project_gpu (left=1/0)
    //   AccumulateCouplingProxy → gu_dirac_coupling_proxy_gpu
    // Header signatures confirmed to match IDiracKernel interface (M44-GPU review).

    /// <inheritdoc/>
    public void ApplyDirac(ReadOnlySpan<double> psi, Span<double> result)
        => _cpuDelegate.ApplyDirac(psi, result); // TODO(M44-GPU): dispatch gu_dirac_apply_gpu

    /// <inheritdoc/>
    public void ApplyMassPsi(ReadOnlySpan<double> psi, Span<double> result)
        => _cpuDelegate.ApplyMassPsi(psi, result); // TODO(M44-GPU): dispatch gu_dirac_mass_apply_gpu

    /// <inheritdoc/>
    public void ProjectLeft(ReadOnlySpan<double> psi, Span<double> result)
        => _cpuDelegate.ProjectLeft(psi, result); // TODO(M44-GPU): dispatch gu_dirac_chirality_project_gpu(left=1)

    /// <inheritdoc/>
    public void ProjectRight(ReadOnlySpan<double> psi, Span<double> result)
        => _cpuDelegate.ProjectRight(psi, result); // TODO(M44-GPU): dispatch gu_dirac_chirality_project_gpu(left=0)

    /// <inheritdoc/>
    public double AccumulateCouplingProxy(
        ReadOnlySpan<double> bosonPerturbation,
        IReadOnlyList<(int ModeI, int ModeJ)> modePairs)
        => _cpuDelegate.AccumulateCouplingProxy(bosonPerturbation, modePairs); // TODO(M44-GPU): dispatch gu_dirac_coupling_proxy_gpu
}
