using Gu.Branching;
using Gu.Core;

namespace Gu.Phase2.Stability;

/// <summary>
/// LOBPCG (Locally Optimal Block Preconditioned Conjugate Gradient) stub
/// for computing smallest eigenvalues of symmetric operators.
///
/// This is a placeholder implementation that delegates to Lanczos.
/// A full LOBPCG implementation would be more efficient for computing
/// multiple eigenvalues simultaneously with preconditioning support.
/// </summary>
public sealed class LobpcgSpectrumProbe : ISpectrumProbe
{
    public string MethodId => "lanczos-delegated";

    private readonly LanczosSpectrumProbe _lanczos = new();

    public SpectrumProbeResult ComputeSmallestEigenvalues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200)
    {
        // Delegate to Lanczos for now; LOBPCG implementation is a future enhancement
        return _lanczos.ComputeSmallestEigenvalues(op, k, tolerance, maxIterations);
    }

    public SpectrumProbeResult ComputeSmallestSingularValues(
        ILinearOperator op, int k, double tolerance = 1e-8, int maxIterations = 200)
    {
        return _lanczos.ComputeSmallestSingularValues(op, k, tolerance, maxIterations);
    }
}
