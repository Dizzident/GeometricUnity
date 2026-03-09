using Gu.Phase2.Semantics;

namespace Gu.Phase2.CudaInterop;

/// <summary>
/// Interface for Phase II Hessian-style operator kernel.
/// Computes H*v = L_tilde^T * M * L_tilde * v where L_tilde = (J, sqrt(lambda)*C).
///
/// Priority 2 kernel per IMPLEMENTATION_PLAN_P2.md Section 11.
/// </summary>
public interface IPhase2HessianKernel
{
    /// <summary>
    /// Compute Hessian-vector product: result = H(u) * v.
    /// H = J^T * M_R * J + lambda * C^T * M_0 * C
    /// where M_R is the residual mass matrix, M_0 is the connection mass matrix,
    /// and C is the Coulomb gauge operator.
    /// </summary>
    /// <param name="u">Current connection state (edge-valued).</param>
    /// <param name="v">Input vector (edge-valued, length = edgeCount * dimG).</param>
    /// <param name="result">Output: H*v (edge-valued, length = edgeCount * dimG).</param>
    /// <param name="variant">Branch variant manifest for dispatch.</param>
    /// <param name="lambda">Gauge penalty weight.</param>
    void ApplyHv(ReadOnlySpan<double> u, ReadOnlySpan<double> v, Span<double> result,
        BranchVariantManifest variant, double lambda);
}
