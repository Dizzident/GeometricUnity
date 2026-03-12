using Gu.Core;
using Gu.Geometry;
using Gu.Phase4.Fermions;
using Gu.Phase4.Spin;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Assembles the discrete Dirac operator D_h from a spin connection and gamma matrices.
///
/// D_h = Gamma^mu * (nabla^{LC}_{spin,mu} + omega_mu^a * rho(T_a)) + M_branch + C_branch
/// </summary>
public interface IDiracOperatorAssembler
{
    /// <summary>
    /// Assemble D_h into a DiracOperatorBundle.
    ///
    /// The fermionBackgroundId is derived from connection.BackgroundId.
    /// The explicit dense matrix is included when totalDof is small enough
    /// (controlled internally by the implementation).
    /// </summary>
    DiracOperatorBundle Assemble(
        SpinConnectionBundle connection,
        GammaOperatorBundle gammas,
        FermionFieldLayout layout,
        SimplicialMesh mesh,
        ProvenanceMeta provenance);

    /// <summary>
    /// Matrix-free apply: computes D_h * psi without re-assembling the matrix.
    /// psi is a flat real array of length totalDof * 2 (complex, re/im interleaved).
    /// Returns result of same shape.
    ///
    /// If bundle.HasExplicitMatrix is true, uses the precomputed explicit matrix.
    /// Otherwise, re-applies the operator using the stored mesh/connection data in bundle.
    /// </summary>
    double[] Apply(DiracOperatorBundle bundle, double[] psi);
}
