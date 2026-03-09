using Gu.Phase2.Semantics;

namespace Gu.Phase2.CudaInterop;

/// <summary>
/// Interface for batched multi-branch residual evaluation on GPU.
/// Evaluates N branch variants in a single CUDA launch.
///
/// Priority 3 kernel per IMPLEMENTATION_PLAN_P2.md Section 11.
/// </summary>
public interface IPhase2BatchKernel
{
    /// <summary>
    /// Evaluate residuals for multiple branch variants in a single batched call.
    /// Each variant is evaluated at its corresponding connection state.
    /// </summary>
    /// <param name="variants">Branch variant manifests (one per batch item).</param>
    /// <param name="connectionStates">Connection states packed contiguously (each of length fieldDof).</param>
    /// <param name="residualsOut">Output residuals packed contiguously (each of length residualDof).</param>
    /// <param name="fieldDof">DOFs per connection state.</param>
    /// <param name="residualDof">DOFs per residual.</param>
    void EvaluateBatch(
        IReadOnlyList<BranchVariantManifest> variants,
        ReadOnlySpan<double> connectionStates,
        Span<double> residualsOut,
        int fieldDof,
        int residualDof);
}
