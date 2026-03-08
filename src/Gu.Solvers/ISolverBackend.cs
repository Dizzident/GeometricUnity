using Gu.Branching;
using Gu.Core;

namespace Gu.Solvers;

/// <summary>
/// Backend interface for solver computations. CPU impl in Gu.ReferenceCpu.
/// The SolverOrchestrator owns the iteration loop; backends do the math.
/// </summary>
public interface ISolverBackend
{
    /// <summary>Assemble derived state (F, T, S, Upsilon) from omega.</summary>
    DerivedState EvaluateDerived(FieldTensor omega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry);

    /// <summary>Evaluate objective I2_h = (1/2) Upsilon^T M Upsilon.</summary>
    double EvaluateObjective(FieldTensor upsilon);

    /// <summary>Build Jacobian operator.</summary>
    ILinearOperator BuildJacobian(FieldTensor omega, FieldTensor a0, FieldTensor curvatureF, BranchManifest manifest, GeometryContext geometry);

    /// <summary>Compute gradient: G = J^T M Upsilon.</summary>
    FieldTensor ComputeGradient(ILinearOperator jacobian, FieldTensor upsilon);

    /// <summary>Compute mass-weighted norm of a residual field.</summary>
    double ComputeNorm(FieldTensor v);
}
