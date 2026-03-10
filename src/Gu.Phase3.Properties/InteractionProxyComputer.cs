using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Phase3.Spectra;
using Gu.Solvers;

namespace Gu.Phase3.Properties;

/// <summary>
/// Computes finite-difference cubic response proxy:
///   C(v_i, v_j, v_k) ~ directional multilinear response of G
///
/// Uses 2nd-order FD of the gradient:
///   C(u, v, w) ≈ [G(z+eps*(u+v+w)) - G(z+eps*(u+v)) - G(z+eps*(u+w)) - G(z+eps*(v+w))
///                 + G(z+eps*u) + G(z+eps*v) + G(z+eps*w) - G(z)] / eps^3
///
/// (See IMPLEMENTATION_PLAN_P3.md Section 4.10.9)
/// CPU reference first.
/// </summary>
public sealed class InteractionProxyComputer
{
    private readonly ISolverBackend _backend;
    private readonly double _epsilon;

    /// <param name="backend">Solver backend for gradient evaluation.</param>
    /// <param name="epsilon">FD step size (default 1e-3 for cubic, coarser than quadratic).</param>
    public InteractionProxyComputer(ISolverBackend backend, double epsilon = 1e-3)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _epsilon = epsilon;
    }

    /// <summary>
    /// Compute cubic response C(v_i, v_j, v_k) at a background.
    /// </summary>
    public InteractionProxyRecord Compute(
        ModeRecord modeI, ModeRecord modeJ, ModeRecord modeK,
        FieldTensor omega, FieldTensor a0,
        BranchManifest manifest, GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(modeI);
        ArgumentNullException.ThrowIfNull(modeJ);
        ArgumentNullException.ThrowIfNull(modeK);

        int n = omega.Coefficients.Length;
        double eps = _epsilon;

        // Evaluate gradient at background
        double gNorm0 = EvaluateGradientNorm(omega, a0, manifest, geometry);

        // Evaluate at 7 perturbed points for 3rd-order FD
        var vi = modeI.ModeVector;
        var vj = modeJ.ModeVector;
        var vk = modeK.ModeVector;

        double g_uvw = EvaluateGradientNorm(Perturb(omega, eps, vi, vj, vk), a0, manifest, geometry);
        double g_uv = EvaluateGradientNorm(Perturb(omega, eps, vi, vj), a0, manifest, geometry);
        double g_uw = EvaluateGradientNorm(Perturb(omega, eps, vi, vk), a0, manifest, geometry);
        double g_vw = EvaluateGradientNorm(Perturb(omega, eps, vj, vk), a0, manifest, geometry);
        double g_u = EvaluateGradientNorm(Perturb(omega, eps, vi), a0, manifest, geometry);
        double g_v = EvaluateGradientNorm(Perturb(omega, eps, vj), a0, manifest, geometry);
        double g_w = EvaluateGradientNorm(Perturb(omega, eps, vk), a0, manifest, geometry);

        // 3rd-order FD of the gradient norm
        double cubicResponse = (g_uvw - g_uv - g_uw - g_vw + g_u + g_v + g_w - gNorm0) / (eps * eps * eps);

        return new InteractionProxyRecord
        {
            ModeIds = new[] { modeI.ModeId, modeJ.ModeId, modeK.ModeId },
            CubicResponse = cubicResponse,
            Epsilon = eps,
            Method = "finite-difference-gradient",
            BackgroundId = modeI.BackgroundId,
        };
    }

    private double EvaluateGradientNorm(
        FieldTensor omega, FieldTensor a0,
        BranchManifest manifest, GeometryContext geometry)
    {
        var derived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
        var jac = _backend.BuildJacobian(omega, a0, derived.CurvatureF, manifest, geometry);
        var grad = _backend.ComputeGradient(jac, derived.ResidualUpsilon);
        return _backend.ComputeNorm(grad);
    }

    private static FieldTensor Perturb(FieldTensor omega, double eps, params double[][] directions)
    {
        var coeffs = (double[])omega.Coefficients.Clone();
        foreach (var dir in directions)
        {
            for (int i = 0; i < coeffs.Length && i < dir.Length; i++)
                coeffs[i] += eps * dir[i];
        }
        return new FieldTensor
        {
            Label = "omega_pert",
            Signature = omega.Signature,
            Coefficients = coeffs,
            Shape = omega.Shape,
        };
    }
}
