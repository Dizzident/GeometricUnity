namespace Gu.Phase3.Properties;

/// <summary>
/// Lightweight interaction proxy computer using a scalar objective function.
/// Computes cubic response C(v_i, v_j, v_k) via finite-difference of a scalar
/// functional F(x), without requiring a full solver backend.
///
/// Finite-difference formula (8-point stencil for third derivative):
///   C(u,v,w) = [F(x+eps*(u+v+w)) - F(x+eps*(u+v)) - F(x+eps*(u+w)) - F(x+eps*(v+w))
///              + F(x+eps*u) + F(x+eps*v) + F(x+eps*w) - F(x)] / eps^3
///
/// This is useful for unit testing and lightweight scenarios where the full
/// ISolverBackend infrastructure is not needed.
/// </summary>
public sealed class SimpleInteractionProxyComputer
{
    private readonly Func<double[], double> _objectiveFn;
    private readonly double _epsilon;

    /// <summary>
    /// Create a simple interaction proxy computer.
    /// </summary>
    /// <param name="objectiveFn">
    /// Evaluates the scalar objective functional at a given state vector.
    /// </param>
    /// <param name="epsilon">Finite-difference step size (default 1e-4).</param>
    public SimpleInteractionProxyComputer(Func<double[], double> objectiveFn, double epsilon = 1e-4)
    {
        ArgumentNullException.ThrowIfNull(objectiveFn);
        if (epsilon <= 0) throw new ArgumentOutOfRangeException(nameof(epsilon), "Must be positive.");
        _objectiveFn = objectiveFn;
        _epsilon = epsilon;
    }

    /// <summary>
    /// Compute the cubic response C(v_i, v_j, v_k) at background x.
    /// Uses 8-point finite-difference formula for the third derivative.
    /// </summary>
    public InteractionProxyRecord Compute(
        double[] background,
        double[] vi,
        double[] vj,
        double[] vk,
        string backgroundId,
        IReadOnlyList<string> modeIds)
    {
        ArgumentNullException.ThrowIfNull(background);
        ArgumentNullException.ThrowIfNull(vi);
        ArgumentNullException.ThrowIfNull(vj);
        ArgumentNullException.ThrowIfNull(vk);
        ArgumentNullException.ThrowIfNull(backgroundId);
        ArgumentNullException.ThrowIfNull(modeIds);

        int n = background.Length;
        if (vi.Length != n || vj.Length != n || vk.Length != n)
            throw new ArgumentException("All vectors must have the same length as the background.");

        double eps = _epsilon;

        // Evaluate F at the 8 points of the finite-difference stencil
        double f000 = _objectiveFn(background);
        double f100 = _objectiveFn(Perturb(background, eps, vi));
        double f010 = _objectiveFn(Perturb(background, eps, vj));
        double f001 = _objectiveFn(Perturb(background, eps, vk));
        double f110 = _objectiveFn(Perturb(background, eps, vi, vj));
        double f101 = _objectiveFn(Perturb(background, eps, vi, vk));
        double f011 = _objectiveFn(Perturb(background, eps, vj, vk));
        double f111 = _objectiveFn(Perturb(background, eps, vi, vj, vk));

        double cubicResponse = (f111 - f110 - f101 - f011 + f100 + f010 + f001 - f000) / (eps * eps * eps);

        // Estimate error by comparing with half-step
        double epsHalf = eps / 2.0;
        double fh100 = _objectiveFn(Perturb(background, epsHalf, vi));
        double fh010 = _objectiveFn(Perturb(background, epsHalf, vj));
        double fh001 = _objectiveFn(Perturb(background, epsHalf, vk));
        double fh110 = _objectiveFn(Perturb(background, epsHalf, vi, vj));
        double fh101 = _objectiveFn(Perturb(background, epsHalf, vi, vk));
        double fh011 = _objectiveFn(Perturb(background, epsHalf, vj, vk));
        double fh111 = _objectiveFn(Perturb(background, epsHalf, vi, vj, vk));

        double cubicResponseHalf = (fh111 - fh110 - fh101 - fh011 + fh100 + fh010 + fh001 - f000)
                                   / (epsHalf * epsHalf * epsHalf);

        double estimatedError = System.Math.Abs(cubicResponse - cubicResponseHalf);

        return new InteractionProxyRecord
        {
            ModeIds = modeIds,
            CubicResponse = cubicResponse,
            Epsilon = eps,
            Method = "finite-difference-residual",
            BackgroundId = backgroundId,
            EstimatedError = estimatedError,
        };
    }

    /// <summary>
    /// Convenience overload with string mode IDs.
    /// </summary>
    public InteractionProxyRecord Compute(
        double[] background,
        double[] vi,
        double[] vj,
        double[] vk,
        string backgroundId,
        string modeIdI,
        string modeIdJ,
        string modeIdK)
    {
        return Compute(background, vi, vj, vk, backgroundId, new[] { modeIdI, modeIdJ, modeIdK });
    }

    private static double[] Perturb(double[] x, double eps, params double[][] directions)
    {
        var result = new double[x.Length];
        Array.Copy(x, result, x.Length);
        foreach (var d in directions)
        {
            for (int i = 0; i < x.Length; i++)
                result[i] += eps * d[i];
        }
        return result;
    }
}
