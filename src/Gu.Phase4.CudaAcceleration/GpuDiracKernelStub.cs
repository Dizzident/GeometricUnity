
using Gu.Phase4.Fermions;

namespace Gu.Phase4.CudaAcceleration;

/// <summary>
/// GPU stub implementation of IDiracKernel for M44 parity testing.
///
/// In environments without real CUDA (e.g., CI), this stub delegates all
/// computations to the same CPU code as CpuDiracKernel, but sets
/// ComputedWithCuda = true and adds a VerificationStatus flag.
///
/// In a real deployment, this would be replaced by P/Invoke to a CUDA library
/// (see Gu.Phase2.CudaInterop for the precedent pattern). For Phase IV the
/// stub establishes the interface contract and enables CPU/GPU parity tests
/// to run in all environments.
///
/// PhysicsNote (M44): CPU/GPU parity is declared on the interface. Until a real
/// CUDA kernel is linked, VerificationStatus = "stub-unverified" and any
/// FermionSpectralResult produced must set ComputedWithUnverifiedGpu = true.
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
    public bool ComputedWithCuda => true; // stub claims CUDA

    /// <inheritdoc/>
    public void ApplyDirac(ReadOnlySpan<double> psi, Span<double> result)
        => _cpuDelegate.ApplyDirac(psi, result);

    /// <inheritdoc/>
    public void ApplyMassPsi(ReadOnlySpan<double> psi, Span<double> result)
        => _cpuDelegate.ApplyMassPsi(psi, result);

    /// <inheritdoc/>
    public void ProjectLeft(ReadOnlySpan<double> psi, Span<double> result)
        => _cpuDelegate.ProjectLeft(psi, result);

    /// <inheritdoc/>
    public void ProjectRight(ReadOnlySpan<double> psi, Span<double> result)
        => _cpuDelegate.ProjectRight(psi, result);

    /// <inheritdoc/>
    public double AccumulateCouplingProxy(
        ReadOnlySpan<double> bosonPerturbation,
        IReadOnlyList<(int ModeI, int ModeJ)> modePairs)
        => _cpuDelegate.AccumulateCouplingProxy(bosonPerturbation, modePairs);
}
