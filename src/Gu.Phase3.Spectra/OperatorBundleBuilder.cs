using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Phase2.Stability;
using Gu.Phase3.Backgrounds;
using Gu.Phase3.GaugeReduction;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Builds LinearizedOperatorBundle instances from a background state.
/// Reuses Phase II infrastructure (HessianOperator, InfinitesimalGaugeMap, etc.)
/// and adds Phase III extensions (physical projector, full Hessian with correction).
///
/// PHYSICS CONSTRAINT #10: Extend existing Phase II infrastructure, don't replace.
/// </summary>
public sealed class OperatorBundleBuilder
{
    private readonly SimplicialMesh _mesh;
    private readonly LieAlgebra _algebra;
    private readonly CpuResidualAssembler _assembler;
    private readonly CpuMassMatrix _residualMass;
    private readonly ISolverBackend _backend;

    public OperatorBundleBuilder(
        SimplicialMesh mesh,
        LieAlgebra algebra,
        CpuResidualAssembler assembler,
        CpuMassMatrix residualMass,
        ISolverBackend backend)
    {
        _mesh = mesh ?? throw new ArgumentNullException(nameof(mesh));
        _algebra = algebra ?? throw new ArgumentNullException(nameof(algebra));
        _assembler = assembler ?? throw new ArgumentNullException(nameof(assembler));
        _residualMass = residualMass ?? throw new ArgumentNullException(nameof(residualMass));
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
    }

    /// <summary>
    /// Build a linearized operator bundle at a background state.
    /// </summary>
    public LinearizedOperatorBundle Build(
        LinearizedOperatorSpec spec,
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry,
        GaugeProjector? gaugeProjector = null)
    {
        ArgumentNullException.ThrowIfNull(spec);
        ArgumentNullException.ThrowIfNull(omega);
        ArgumentNullException.ThrowIfNull(a0);
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentNullException.ThrowIfNull(geometry);

        // Guard: QuotientAware (P3) is not yet implemented
        if (spec.Formulation == PhysicalModeFormulation.QuotientAware)
            throw new NotSupportedException(
                "PhysicalModeFormulation.QuotientAware (P3) is not yet implemented. " +
                "Use ProjectedComplement (P2) for equivalent physical results. " +
                "P3 is a computational optimization of P2, not a different physical answer.");

        // PHYSICS CONSTRAINT #1: Enforce GN-only-for-B2 rule
        if (spec.OperatorType == SpectralOperatorType.GaussNewton &&
            spec.BackgroundAdmissibility != AdmissibilityLevel.B2)
        {
            throw new InvalidOperationException(
                $"Gauss-Newton operator is ONLY valid for B2-level backgrounds. " +
                $"This background has admissibility {spec.BackgroundAdmissibility}. " +
                $"Use FullHessian instead.");
        }

        // Build physics Jacobian J (reusing Phase II)
        var conn = new ConnectionField(_mesh, _algebra, (double[])omega.Coefficients.Clone());
        var a0Conn = new ConnectionField(_mesh, _algebra, (double[])a0.Coefficients.Clone());
        var derived = _assembler.AssembleDerivedState(conn, a0Conn, manifest, geometry);
        var jacobian = _assembler.BuildJacobian(conn, a0Conn, derived.CurvatureF, manifest, geometry);

        // Build GN Hessian H_GN = J^T M_R J + lambda * C^T M_0 C (reusing Phase II HessianOperator)
        var gauge = new CoulombGaugePenalty(_mesh, _algebra.Dimension, spec.GaugeLambda);
        var hessianGN = new HessianOperator(
            jacobian, gauge, _residualMass, null,
            spec.GaugeLambda, _algebra.Dimension, _mesh.VertexCount);

        // Build spectral operator
        ILinearOperator spectralOp;
        if (spec.OperatorType == SpectralOperatorType.FullHessian)
        {
            // Build residual correction via FD of the gradient
            var correctionAction = BuildResidualCorrectionAction(
                omega, a0, manifest, geometry, derived.ResidualUpsilon, hessianGN);
            spectralOp = new FullHessianOperator(hessianGN, correctionAction);
        }
        else
        {
            spectralOp = hessianGN;
        }

        // Build state mass operator M_state
        var stateMass = new StateMassOperator(_mesh, _algebra);

        // Build physical projector (P2 formulation)
        ILinearOperator? physProjector = null;
        int? physDim = null;
        int? gaugeRank = null;

        if (spec.Formulation == PhysicalModeFormulation.ProjectedComplement && gaugeProjector is not null)
        {
            physProjector = new PhysicalProjector(gaugeProjector);
            gaugeRank = gaugeProjector.GaugeRank;
            physDim = gaugeProjector.PhysicalDimension;
        }

        return new LinearizedOperatorBundle
        {
            BundleId = $"op-bundle-{spec.BackgroundId}-{spec.OperatorType}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            BackgroundId = spec.BackgroundId,
            BranchManifestId = manifest.BranchId,
            OperatorType = spec.OperatorType,
            Formulation = spec.Formulation,
            BackgroundAdmissibility = spec.BackgroundAdmissibility,
            Jacobian = jacobian,
            SpectralOperator = spectralOp,
            MassOperator = stateMass,
            PhysicalProjector = physProjector,
            GaugeLambda = spec.GaugeLambda,
            StateDimension = _mesh.EdgeCount * _algebra.Dimension,
            PhysicalDimension = physDim,
            GaugeRank = gaugeRank,
        };
    }

    /// <summary>
    /// Build the residual correction: (dJ/dz[v])^T M Upsilon_* via FD.
    ///
    /// Correction(v) = (G(z+eps*v) - G(z))/eps - H_GN*v
    ///
    /// where G(z) = J(z)^T M Upsilon(z) is the gradient.
    ///
    /// PHYSICS CONSTRAINT #6: Use eps ~ 1e-4 for float64.
    /// </summary>
    private Func<FieldTensor, FieldTensor>? BuildResidualCorrectionAction(
        FieldTensor omega, FieldTensor a0,
        BranchManifest manifest, GeometryContext geometry,
        FieldTensor upsilonStar, ILinearOperator hessianGN)
    {
        double residualNorm = _backend.ComputeNorm(upsilonStar);
        if (residualNorm < 1e-12)
            return null; // Negligible residual, correction ~ 0

        double eps = 1e-4; // PHYSICS CONSTRAINT #6

        // Pre-compute gradient at background
        var jacobianStar = _backend.BuildJacobian(omega, a0, null!, manifest, geometry);
        var gradStar = _backend.ComputeGradient(jacobianStar, upsilonStar);

        return v =>
        {
            // Perturbed state omega + eps*v
            var pertCoeffs = new double[omega.Coefficients.Length];
            for (int i = 0; i < pertCoeffs.Length; i++)
                pertCoeffs[i] = omega.Coefficients[i] + eps * v.Coefficients[i];

            var pertOmega = new FieldTensor
            {
                Label = "omega_pert",
                Signature = omega.Signature,
                Coefficients = pertCoeffs,
                Shape = omega.Shape,
            };

            // Gradient at perturbed state
            var derivedPert = _backend.EvaluateDerived(pertOmega, a0, manifest, geometry);
            var jacPert = _backend.BuildJacobian(pertOmega, a0, derivedPert.CurvatureF, manifest, geometry);
            var gradPert = _backend.ComputeGradient(jacPert, derivedPert.ResidualUpsilon);

            // Full FD Hessian action: H_full*v ~ (G_pert - G_star) / eps
            // Correction = H_full*v - H_GN*v
            var hgnV = hessianGN.Apply(v);

            var correction = new double[omega.Coefficients.Length];
            for (int i = 0; i < correction.Length; i++)
                correction[i] = (gradPert.Coefficients[i] - gradStar.Coefficients[i]) / eps
                               - hgnV.Coefficients[i];

            return new FieldTensor
            {
                Label = "H_correction*v",
                Signature = omega.Signature,
                Coefficients = correction,
                Shape = omega.Shape,
            };
        };
    }
}
