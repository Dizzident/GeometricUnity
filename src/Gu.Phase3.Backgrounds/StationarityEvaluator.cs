using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.ReferenceCpu;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Computes the stationarity norm ||G_h(z_*)|| = ||J^T M_Upsilon Upsilon||.
///
/// A background z_* is stationary iff G_h(z_*) = 0, meaning the
/// objective gradient vanishes. This is the "first-order optimality"
/// condition: the gradient of I2_h = (1/2) Upsilon^T M Upsilon
/// w.r.t. omega is G = J^T M Upsilon.
///
/// The stationarity norm is the L2 norm of this gradient vector.
/// </summary>
public sealed class StationarityEvaluator
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly CpuResidualAssembler _assembler;
    private readonly CpuMassMatrix _massMatrix;

    public StationarityEvaluator(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        CpuResidualAssembler assembler,
        CpuMassMatrix massMatrix)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        _assembler = assembler ?? throw new ArgumentNullException(nameof(assembler));
        _massMatrix = massMatrix ?? throw new ArgumentNullException(nameof(massMatrix));
    }

    /// <summary>
    /// Evaluate the stationarity norm ||G_h(z_*)|| at a given background state.
    ///
    /// Uses the existing LinearizationWorkbench to build the Jacobian,
    /// then computes G = J^T M Upsilon and returns its L2 norm.
    /// </summary>
    /// <param name="background">Background state record with omega, A0, and derived state.</param>
    /// <param name="manifest">Branch manifest.</param>
    /// <param name="geometry">Geometry context.</param>
    /// <returns>The stationarity norm ||G||_2.</returns>
    public double Evaluate(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(background);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(geometry);

        var workbench = new LinearizationWorkbench(_mesh, _algebra, _assembler, _massMatrix);
        var jacobian = workbench.BuildJacobian(background, manifest, geometry);

        var gradient = jacobian.ComputeGradient(background.DerivedState.ResidualUpsilon, _massMatrix);
        return L2Norm(gradient.Coefficients);
    }

    /// <summary>
    /// Evaluate stationarity from raw omega and A0 tensors.
    ///
    /// Assembles derived state, builds Jacobian, computes G = J^T M Upsilon.
    /// </summary>
    public double Evaluate(
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(omega);
        ArgumentNullException.ThrowIfNull(a0);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(geometry);

        var workbench = new LinearizationWorkbench(_mesh, _algebra, _assembler, _massMatrix);
        var bg = workbench.CreateBackgroundState(
            "stationarity-eval",
            omega, a0, manifest, geometry,
            solverConverged: true);

        return Evaluate(bg, manifest, geometry);
    }

    /// <summary>
    /// Compute the full gradient vector G = J^T M Upsilon (not just the norm).
    /// </summary>
    public FieldTensor ComputeGradient(
        BackgroundStateRecord background,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        ArgumentNullException.ThrowIfNull(background);

        var workbench = new LinearizationWorkbench(_mesh, _algebra, _assembler, _massMatrix);
        var jacobian = workbench.BuildJacobian(background, manifest, geometry);

        return jacobian.ComputeGradient(background.DerivedState.ResidualUpsilon, _massMatrix);
    }

    private static double L2Norm(double[] v)
    {
        double sum = 0;
        for (int i = 0; i < v.Length; i++)
            sum += v[i] * v[i];
        return System.Math.Sqrt(sum);
    }
}
