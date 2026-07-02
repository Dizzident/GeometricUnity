namespace Gu.Phase4.Dirac;

/// <summary>
/// Selects how <see cref="CpuDiracOperatorAssembler"/> maps a mesh edge to its
/// Clifford element Γ̂(e) in the discrete Dirac hop
/// (D_h ψ)_v = Σ_{e=(v,w)} Γ̂(e)·(ψ_w − ψ_v)/|e|.
/// </summary>
public enum GammaEdgeScheme
{
    /// <summary>
    /// EXISTING DEFAULT (backward-compatible). Γ̂(e) = γ^μ for the single dominant
    /// axis μ = argmax_μ |Δx_μ|. On axis-aligned edges this is exact; on diagonal
    /// (Freudenthal) edges it is an arbitrary tie-break. All existing callers use
    /// this scheme and are byte-identical.
    /// </summary>
    DominantAxis = 0,

    /// <summary>
    /// 4D refinement (design §2.4). Γ̂(e) = Σ_μ (Δx_μ / |e|) · γ^μ = ê·Γ, the
    /// Clifford contraction of the UNIT edge vector. Hermitian (real combination
    /// of Hermitian Riemannian gammas) and reduces EXACTLY to DominantAxis on
    /// axis-aligned edges. Chosen for the Freudenthal 4D mesh, where most edges
    /// are diagonal and the dominant-axis tie-break is ill-defined.
    /// </summary>
    EdgeVectorContraction = 1,
}
