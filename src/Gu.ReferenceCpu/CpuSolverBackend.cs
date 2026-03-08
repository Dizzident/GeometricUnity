using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Solvers;

namespace Gu.ReferenceCpu;

/// <summary>
/// CPU reference implementation of ISolverBackend.
/// Wraps CpuResidualAssembler, CpuMassMatrix, and CpuLocalJacobian.
/// </summary>
public sealed class CpuSolverBackend : ISolverBackend
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly CpuResidualAssembler _assembler;
    private readonly CpuMassMatrix _massMatrix;

    public CpuSolverBackend(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        ITorsionBranchOperator torsion,
        IShiabBranchOperator shiab)
        : this(mesh, algebra, torsion, shiab, new CpuMassMatrix(mesh, algebra))
    {
    }

    public CpuSolverBackend(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        ITorsionBranchOperator torsion,
        IShiabBranchOperator shiab,
        CpuMassMatrix massMatrix)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        _assembler = new CpuResidualAssembler(mesh, algebra, torsion, shiab);
        _massMatrix = massMatrix ?? throw new ArgumentNullException(nameof(massMatrix));
    }

    public DerivedState EvaluateDerived(
        FieldTensor omega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
    {
        var conn = new ConnectionField(_mesh, _algebra, (double[])omega.Coefficients.Clone());
        var a0Conn = new ConnectionField(_mesh, _algebra, (double[])a0.Coefficients.Clone());
        return _assembler.AssembleDerivedState(conn, a0Conn, manifest, geometry);
    }

    public double EvaluateObjective(FieldTensor upsilon)
    {
        return _massMatrix.EvaluateObjective(upsilon);
    }

    public ILinearOperator BuildJacobian(
        FieldTensor omega, FieldTensor a0, FieldTensor curvatureF,
        BranchManifest manifest, GeometryContext geometry)
    {
        var conn = new ConnectionField(_mesh, _algebra, (double[])omega.Coefficients.Clone());
        var a0Conn = new ConnectionField(_mesh, _algebra, (double[])a0.Coefficients.Clone());
        return _assembler.BuildJacobian(conn, a0Conn, curvatureF, manifest, geometry);
    }

    public FieldTensor ComputeGradient(ILinearOperator jacobian, FieldTensor upsilon)
    {
        if (jacobian is CpuLocalJacobian cpuJacobian)
        {
            return cpuJacobian.ComputeGradient(upsilon, _massMatrix);
        }

        // Fallback: J^T * (M * upsilon)
        var mUpsilon = _massMatrix.Apply(upsilon);
        return jacobian.ApplyTranspose(mUpsilon);
    }

    public double ComputeNorm(FieldTensor v)
    {
        return _massMatrix.Norm(v);
    }
}
