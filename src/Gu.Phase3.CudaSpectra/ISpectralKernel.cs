namespace Gu.Phase3.CudaSpectra;

/// <summary>
/// Interface for GPU-accelerated spectral operator kernels.
/// Provides Hessian-vector products, mass-vector products,
/// and projected spectral actions on device buffers.
///
/// Mirrors the CPU LinearizedOperatorBundle actions but operates
/// on packed double arrays for GPU dispatch.
/// </summary>
public interface ISpectralKernel
{
    /// <summary>
    /// Apply spectral operator H * v (or P^T H P v for projected formulation).
    /// </summary>
    /// <param name="v">Input perturbation vector (edge-valued, length = stateDim).</param>
    /// <param name="result">Output H*v (edge-valued, length = stateDim).</param>
    void ApplySpectral(ReadOnlySpan<double> v, Span<double> result);

    /// <summary>
    /// Apply mass operator M_state * v (or P^T M P v for projected formulation).
    /// </summary>
    /// <param name="v">Input perturbation vector (edge-valued, length = stateDim).</param>
    /// <param name="result">Output M*v (edge-valued, length = stateDim).</param>
    void ApplyMass(ReadOnlySpan<double> v, Span<double> result);

    /// <summary>
    /// Apply Jacobian J * v (state -> residual).
    /// </summary>
    /// <param name="v">Input perturbation (edge-valued, length = stateDim).</param>
    /// <param name="result">Output J*v (face-valued, length = residualDim).</param>
    void ApplyJacobian(ReadOnlySpan<double> v, Span<double> result);

    /// <summary>
    /// Apply adjoint J^T * w (residual -> state).
    /// </summary>
    /// <param name="w">Input residual vector (face-valued, length = residualDim).</param>
    /// <param name="result">Output J^T*w (edge-valued, length = stateDim).</param>
    void ApplyAdjoint(ReadOnlySpan<double> w, Span<double> result);

    /// <summary>State-space dimension (edgeCount * dimG).</summary>
    int StateDimension { get; }

    /// <summary>Residual-space dimension (faceCount * dimG).</summary>
    int ResidualDimension { get; }
}
