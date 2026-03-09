using Gu.Branching;
using Gu.Core;

namespace Gu.Phase2.Stability;

/// <summary>
/// Result of a spectrum probe computation.
/// </summary>
public sealed class SpectrumProbeResult
{
    /// <summary>Computed eigenvalues or singular values, sorted ascending.</summary>
    public required double[] Values { get; init; }

    /// <summary>
    /// Corresponding eigenvectors or singular vectors (column-major, may be null
    /// if only values were requested).
    /// Each vector has length = operator dimension.
    /// </summary>
    public FieldTensor[]? Vectors { get; init; }

    /// <summary>Residual norms ||A*v - lambda*v|| for each computed pair.</summary>
    public required double[] ResidualNorms { get; init; }

    /// <summary>Number of iterations used.</summary>
    public required int Iterations { get; init; }

    /// <summary>
    /// Convergence status: "converged", "partially-converged", "failed".
    /// </summary>
    public required string ConvergenceStatus { get; init; }
}

/// <summary>
/// Interface for spectrum probes that compute extremal eigenvalues/singular values
/// of matrix-free linear operators.
/// </summary>
public interface ISpectrumProbe
{
    /// <summary>Probe method identifier: "lanczos", "lobpcg", "randomized-svd", etc.</summary>
    string MethodId { get; }

    /// <summary>
    /// Compute the k smallest eigenvalues of a symmetric operator.
    /// </summary>
    /// <param name="op">Symmetric linear operator (e.g., HessianOperator).</param>
    /// <param name="k">Number of eigenvalues to compute.</param>
    /// <param name="tolerance">Convergence tolerance for residual norms.</param>
    /// <param name="maxIterations">Maximum number of iterations.</param>
    SpectrumProbeResult ComputeSmallestEigenvalues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200);

    /// <summary>
    /// Compute the k smallest singular values of a (possibly non-square) operator.
    /// </summary>
    /// <param name="op">Linear operator (e.g., GaugeFixedLinearOperator).</param>
    /// <param name="k">Number of singular values to compute.</param>
    /// <param name="tolerance">Convergence tolerance.</param>
    /// <param name="maxIterations">Maximum number of iterations.</param>
    SpectrumProbeResult ComputeSmallestSingularValues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200);
}
