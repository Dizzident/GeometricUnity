using System.Numerics;

namespace Gu.Phase4.Spin;

/// <summary>
/// SINGLE SOURCE OF TRUTH for the frame-contracted edge gamma
///   Γ̂(e) = ê·Γ = Σ_μ (Δx_μ / |e|) γ^μ,
/// the Clifford contraction of the UNIT edge vector with the gamma matrices.
///
/// Both consumers call this — there is no second implementation:
///   - the production Dirac assembler (Gu.Phase4.Dirac.CpuDiracOperatorAssembler,
///     GammaEdgeScheme.EdgeVectorContraction), and
///   - the SpinorField-native reference operator
///     (<see cref="SpinorDiracOperator"/>).
///
/// Convention (physics decisions §6c, PINNED): Γ̂ is built from the UNIT edge
/// vector ê; the 1/|e| of the finite-difference stencil is applied by the caller
/// (the hopping term ψ_w − ψ_v is divided by |e|), NOT folded in here. On an
/// axis-aligned unit edge this reduces exactly to the single gamma γ^μ.
/// </summary>
public static class EdgeGammaContraction
{
    /// <summary>
    /// Unit edge-gamma Γ̂ = Σ_μ (Δ_μ/|Δ|) γ^μ for the displacement Δ = to − from,
    /// contracted over the first min(coordinate count, gamma count) directions.
    /// The output <paramref name="length"/> is |Δ| over the shared coordinate
    /// components. Returns null (degenerate edge) when |Δ| is below
    /// <paramref name="tolerance"/>.
    /// </summary>
    public static Complex[,]? UnitContract(
        ReadOnlySpan<double> from,
        ReadOnlySpan<double> to,
        GammaOperatorBundle gammas,
        out double length,
        double tolerance = 1e-14)
    {
        ArgumentNullException.ThrowIfNull(gammas);

        int coordDim = System.Math.Min(from.Length, to.Length);
        double sumSq = 0;
        for (int d = 0; d < coordDim; d++)
        {
            double delta = to[d] - from[d];
            sumSq += delta * delta;
        }
        length = System.Math.Sqrt(sumSq);
        if (length < tolerance) return null;

        int nGammas = gammas.GammaMatrices.Length;
        int contractDim = System.Math.Min(coordDim, nGammas);
        int s = gammas.SpinorDimension;
        var result = new Complex[s, s];
        for (int mu = 0; mu < contractDim; mu++)
        {
            double u = (to[mu] - from[mu]) / length;
            if (u == 0.0) continue;
            var g = gammas.GammaMatrices[mu];
            for (int r = 0; r < s; r++)
                for (int c = 0; c < s; c++)
                    result[r, c] += u * g[r, c];
        }
        return result;
    }
}
