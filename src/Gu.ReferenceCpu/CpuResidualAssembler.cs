using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;

namespace Gu.ReferenceCpu;

/// <summary>
/// CPU reference residual assembler. Orchestrates:
/// 1. Curvature F_h from omega_h
/// 2. Torsion T_h from branch operator
/// 3. Shiab S_h from branch operator
/// 4. Residual Upsilon_h = S_h - T_h
/// 5. Objective I2_h = (1/2) Upsilon^T M Upsilon
/// 6. Jacobian J_h = dS/domega - dT/domega
/// 7. Gradient G_h = J^T M Upsilon
/// </summary>
public sealed class CpuResidualAssembler
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly ITorsionBranchOperator _torsion;
    private readonly IShiabBranchOperator _shiab;

    public CpuResidualAssembler(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        ITorsionBranchOperator torsion,
        IShiabBranchOperator shiab)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        _torsion = torsion ?? throw new ArgumentNullException(nameof(torsion));
        _shiab = shiab ?? throw new ArgumentNullException(nameof(shiab));

        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);
    }

    /// <summary>
    /// Assemble full derived state from omega_h.
    /// Returns curvature F, torsion T, Shiab S, and residual Upsilon = S - T.
    /// </summary>
    public DerivedState AssembleDerivedState(
        ConnectionField omega,
        ConnectionField a0,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        var omegaTensor = omega.ToFieldTensor();
        var a0Tensor = a0.ToFieldTensor();

        // 1. Curvature
        var curvature = CurvatureAssembler.Assemble(omega);
        var curvatureTensor = curvature.ToFieldTensor();

        // 2. Torsion
        var torsionT = _torsion.Evaluate(omegaTensor, a0Tensor, manifest, geometry);

        // 3. Shiab
        var shiabS = _shiab.Evaluate(curvatureTensor, omegaTensor, manifest, geometry);

        // 4. Residual: Upsilon = S - T
        var upsilon = FieldTensorOps.Subtract(shiabS, torsionT);
        upsilon = new FieldTensor
        {
            Label = "Upsilon_h",
            Signature = upsilon.Signature,
            Coefficients = upsilon.Coefficients,
            Shape = upsilon.Shape,
        };

        return new DerivedState
        {
            CurvatureF = curvatureTensor,
            TorsionT = torsionT,
            ShiabS = shiabS,
            ResidualUpsilon = upsilon,
        };
    }

    /// <summary>
    /// Evaluate the residual bundle with objective and component norms.
    /// </summary>
    public ResidualBundle EvaluateResidual(
        DerivedState derived,
        CpuMassMatrix massMatrix)
    {
        double objective = massMatrix.EvaluateObjective(derived.ResidualUpsilon);
        double totalNorm = massMatrix.Norm(derived.ResidualUpsilon);

        var components = new List<ResidualComponent>
        {
            new()
            {
                Label = "Shiab_S",
                Norm = FieldTensorOps.L2Norm(derived.ShiabS),
                Field = derived.ShiabS,
            },
            new()
            {
                Label = "Torsion_T",
                Norm = FieldTensorOps.L2Norm(derived.TorsionT),
                Field = derived.TorsionT,
            },
            new()
            {
                Label = "Upsilon",
                Norm = totalNorm,
                Field = derived.ResidualUpsilon,
            },
        };

        return new ResidualBundle
        {
            Components = components,
            ObjectiveValue = objective,
            TotalNorm = totalNorm,
        };
    }

    /// <summary>
    /// Build a matrix-free Jacobian operator.
    /// </summary>
    public CpuLocalJacobian BuildJacobian(
        ConnectionField omega,
        ConnectionField a0,
        FieldTensor curvatureF,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        return new CpuLocalJacobian(
            _shiab, _torsion,
            omega.ToFieldTensor(), a0.ToFieldTensor(),
            curvatureF,
            manifest, geometry,
            _mesh, _algebra);
    }
}
