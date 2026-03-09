using Gu.Phase2.Semantics;

namespace Gu.Phase2.CudaInterop;

/// <summary>
/// Interface for Phase II Jacobian kernel operations.
/// Provides branch-parameterized Jv and J^Tw actions for Newton-Krylov solvers
/// and spectral probes.
///
/// Priority 1 kernel per IMPLEMENTATION_PLAN_P2.md Section 11.
/// </summary>
public interface IPhase2JacobianKernel
{
    /// <summary>
    /// Compute Jacobian-vector product: result = J(u) * v.
    /// J is the linearization of the residual map at connection u,
    /// parameterized by the branch variant.
    /// </summary>
    /// <param name="u">Current connection state (edge-valued, length = edgeCount * dimG).</param>
    /// <param name="v">Perturbation direction (edge-valued, length = edgeCount * dimG).</param>
    /// <param name="result">Output: J*v (face-valued, length = faceCount * dimG).</param>
    /// <param name="variant">Branch variant manifest for dispatch.</param>
    void ApplyJv(ReadOnlySpan<double> u, ReadOnlySpan<double> v, Span<double> result,
        BranchVariantManifest variant);

    /// <summary>
    /// Compute adjoint (transpose) action: result = J(u)^T * w.
    /// Maps a face-valued field to an edge-valued field.
    /// </summary>
    /// <param name="u">Current connection state (edge-valued).</param>
    /// <param name="w">Input vector (face-valued, length = faceCount * dimG).</param>
    /// <param name="result">Output: J^T*w (edge-valued, length = edgeCount * dimG).</param>
    /// <param name="variant">Branch variant manifest for dispatch.</param>
    void ApplyJtw(ReadOnlySpan<double> u, ReadOnlySpan<double> w, Span<double> result,
        BranchVariantManifest variant);
}
