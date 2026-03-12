using System.Text.Json.Serialization;
using Gu.Branching;
using Gu.Core;
using Gu.Phase3.Backgrounds;

namespace Gu.Phase3.Spectra;

/// <summary>
/// A bundle of linearized operators at a background state, ready for spectral solves.
/// Wraps ApplyJ, ApplyJT, ApplyH_GN or ApplyH_full, ApplyM_state, ApplyP_phys.
///
/// PHYSICS CONSTRAINTS:
/// - #1: Every bundle declares which operator type produced it (GN vs Full Hessian).
///       GN is ONLY valid for B2 backgrounds.
/// - #3: M_state respects per-block inner products from BranchFieldLayout.
/// - #8: P_phys is M_state-self-adjoint.
/// </summary>
public sealed class LinearizedOperatorBundle
{
    /// <summary>Unique bundle identifier.</summary>
    public required string BundleId { get; init; }

    /// <summary>Background ID this bundle was built at.</summary>
    public required string BackgroundId { get; init; }

    /// <summary>Branch manifest ID.</summary>
    public required string BranchManifestId { get; init; }

    /// <summary>
    /// Which operator type: GaussNewton or FullHessian.
    /// PHYSICS: GN only valid for B2 backgrounds.
    /// </summary>
    public required SpectralOperatorType OperatorType { get; init; }

    /// <summary>Physical mode formulation used (P1/P2).</summary>
    public required PhysicalModeFormulation Formulation { get; init; }

    /// <summary>Admissibility level of the background.</summary>
    public required AdmissibilityLevel BackgroundAdmissibility { get; init; }

    /// <summary>Physics Jacobian J: state perturbations -> residual perturbations.</summary>
    public required ILinearOperator Jacobian { get; init; }

    /// <summary>
    /// Spectral operator H (GN or Full Hessian).
    /// H_GN = J^T M_Upsilon J, or H_full = H_GN + residual correction.
    /// </summary>
    public required ILinearOperator SpectralOperator { get; init; }

    /// <summary>
    /// State mass matrix M_state (inner product on perturbations).
    /// Used as RHS of generalized eigenproblem: H v = lambda M_state v.
    /// </summary>
    public required ILinearOperator MassOperator { get; init; }

    /// <summary>
    /// Physical projector P_phys (null for P1/penalty-fixed formulation).
    /// Must be M_state-self-adjoint (PHYSICS CONSTRAINT #8).
    /// </summary>
    public ILinearOperator? PhysicalProjector { get; init; }

    /// <summary>Gauge penalty lambda used in construction.</summary>
    public required double GaugeLambda { get; init; }

    /// <summary>State-space dimension (number of DOFs).</summary>
    public required int StateDimension { get; init; }

    /// <summary>Physical dimension after gauge reduction (StateDimension - GaugeRank).</summary>
    public int? PhysicalDimension { get; init; }

    /// <summary>Gauge rank (number of gauge directions removed).</summary>
    public int? GaugeRank { get; init; }

    /// <summary>
    /// Apply H (or P_phys^T H P_phys for P2 formulation) to a vector.
    /// This is the main spectral action.
    /// </summary>
    public FieldTensor ApplySpectral(FieldTensor v)
    {
        if (Formulation == PhysicalModeFormulation.QuotientAware)
            throw new NotSupportedException(
                "PhysicalModeFormulation.QuotientAware (P3) is not yet implemented. " +
                "Use ProjectedComplement (P2) for equivalent physical results. " +
                "P3 is a computational optimization of P2, not a different physical answer.");

        if (Formulation == PhysicalModeFormulation.ProjectedComplement && PhysicalProjector is not null)
        {
            // P_phys^T H P_phys v
            var pv = PhysicalProjector.Apply(v);
            var hpv = SpectralOperator.Apply(pv);
            return PhysicalProjector.ApplyTranspose(hpv);
        }
        return SpectralOperator.Apply(v);
    }

    /// <summary>
    /// Apply M_state (or P_phys^T M_state P_phys for P2 formulation) to a vector.
    /// This is the RHS of the generalized eigenproblem.
    /// </summary>
    public FieldTensor ApplyMass(FieldTensor v)
    {
        if (Formulation == PhysicalModeFormulation.QuotientAware)
            throw new NotSupportedException(
                "PhysicalModeFormulation.QuotientAware (P3) is not yet implemented. " +
                "Use ProjectedComplement (P2) for equivalent physical results. " +
                "P3 is a computational optimization of P2, not a different physical answer.");

        if (Formulation == PhysicalModeFormulation.ProjectedComplement && PhysicalProjector is not null)
        {
            var pv = PhysicalProjector.Apply(v);
            var mpv = MassOperator.Apply(pv);
            return PhysicalProjector.ApplyTranspose(mpv);
        }
        return MassOperator.Apply(v);
    }
}
