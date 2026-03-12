namespace Gu.Phase4.CudaAcceleration;

/// <summary>
/// Simplified IDiracKernel interface for Phase IV CUDA acceleration parity testing.
///
/// This is a matrix-based kernel interface scoped to Gu.Phase4.CudaAcceleration,
/// distinct from the context-rich Gu.Phase4.Dirac.IDiracKernel.
///
/// Operates on real-interleaved complex spinor vectors of length TotalDof
/// where TotalDof = 2 * cellCount * dofsPerCell.
///
/// CPU and GPU implementations share this interface to enable
/// CPU/GPU parity testing via DiracParityChecker.
/// </summary>
public interface IDiracKernel
{
    /// <summary>
    /// Apply the Dirac operator: result = D * psi.
    /// Input and output are real-interleaved complex vectors of length TotalDof.
    /// </summary>
    void ApplyDirac(ReadOnlySpan<double> psi, Span<double> result);

    /// <summary>
    /// Apply the mass-psi operator: result = M_psi * psi.
    /// For unit-weight M_psi, this is the identity.
    /// Input and output length = TotalDof.
    /// </summary>
    void ApplyMassPsi(ReadOnlySpan<double> psi, Span<double> result);

    /// <summary>
    /// Apply left-chirality projector: result = P_L * psi.
    /// P_L = (I - gamma5) / 2.
    /// Input and output length = TotalDof.
    /// </summary>
    void ProjectLeft(ReadOnlySpan<double> psi, Span<double> result);

    /// <summary>
    /// Apply right-chirality projector: result = P_R * psi.
    /// P_R = (I + gamma5) / 2.
    /// Input and output length = TotalDof.
    /// </summary>
    void ProjectRight(ReadOnlySpan<double> psi, Span<double> result);

    /// <summary>
    /// Accumulate coupling proxy: returns sum over (i,j) pairs of |&lt;phi_i, delta_D[b] phi_j&gt;|.
    /// Used for coupling proxy diagnostics.
    /// </summary>
    double AccumulateCouplingProxy(
        ReadOnlySpan<double> bosonPerturbation,
        IReadOnlyList<(int ModeI, int ModeJ)> modePairs);

    /// <summary>Total degrees of freedom (real dimension = 2 * cellCount * dofsPerCell).</summary>
    int TotalDof { get; }

    /// <summary>Whether this kernel is backed by a verified CUDA implementation.</summary>
    bool ComputedWithCuda { get; }
}
