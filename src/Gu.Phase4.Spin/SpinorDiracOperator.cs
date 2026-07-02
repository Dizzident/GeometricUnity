using System.Numerics;
using Gu.Geometry;

namespace Gu.Phase4.Spin;

/// <summary>
/// SpinorField-native discrete Dirac operator — a REFERENCE / PROBE for the
/// Clifford layer, NOT the production fermion operator.
///
/// The production discrete Dirac operator that the fermion spectral solver
/// consumes is Gu.Phase4.Dirac.CpuDiracOperatorAssembler (with
/// GammaEdgeScheme.EdgeVectorContraction on the 4D path). This class exists so the
/// Clifford/spinor layer has a typed, SpinorField-native operator to probe the
/// frame-contracted gamma directly; it shares the SAME edge-gamma definition as
/// the assembler via <see cref="EdgeGammaContraction"/> (single source of truth),
/// so the two never diverge.
///
/// Action (difference / covariant-hop stencil):
///   (D ψ)_v = Σ_{e ∋ v} s(v,e) · w_e · Γ̂(e) · ( ψ_{far(e,v)} − ψ_v ) / |e|  [+ m ψ_v]
/// where Γ̂(e) = ê·Γ is the UNIT frame contraction (from the shared helper),
/// s(v,e) ∈ {+1,−1} is the vertex–edge incidence sign
/// (<see cref="SimplicialMesh.VertexEdgeOrientations"/>), and far(e,v) is the
/// other endpoint. The 1/|e| is the finite-difference normalization carried by
/// the stencil, matching the assembler; Γ̂ acts on the Clifford spinor index only
/// (identity on the gauge index).
///
/// PHYSICIST-GATED (open question #4): edge weight w_e (default 1), mass m
/// (default 0) are constructor parameters; no convention is baked in.
/// HERMITICITY: the difference-form <see cref="BuildMatrix"/> D has an on-site
/// term that cancels only at interior vertices, so D is exactly anti-Hermitian
/// only away from boundaries. The massless PURE-HOPPING matrix
/// (<see cref="BuildHoppingMatrix"/>) is exactly anti-Hermitian on ANY mesh, so
/// i·H is Hermitian to machine precision — the self-adjointness carrier of the
/// Euclidean naive Dirac operator. Which carrier the 4D fermion studies consume
/// (periodic-torus D vs pure-hopping H) is routed to physicist-4d and does not
/// affect this reference operator's structural role.
/// </summary>
public sealed class SpinorDiracOperator
{
    private readonly SimplicialMesh _mesh;
    private readonly GammaOperatorBundle _gammas;
    private readonly double _defaultWeight;
    private readonly double[]? _edgeWeights;
    private readonly double _mass;

    /// <summary>The mesh the operator acts on.</summary>
    public SimplicialMesh Mesh => _mesh;

    /// <summary>The gamma bundle supplying γ^μ.</summary>
    public GammaOperatorBundle Gammas => _gammas;

    /// <summary>Clifford spinor dimension (matches the gamma bundle).</summary>
    public int SpinorDimension => _gammas.SpinorDimension;

    /// <summary>Mass term m (default 0). Applied as + m ψ_v.</summary>
    public double Mass => _mass;

    /// <summary>
    /// Build a SpinorField-native reference Dirac operator.
    /// </summary>
    /// <param name="mesh">Any simplicial mesh (spinors live on vertices).</param>
    /// <param name="gammas">Gamma bundle; its SpinorDimension sets the spinor index size.</param>
    /// <param name="edgeWeight">Uniform edge weight w_e (default 1).</param>
    /// <param name="mass">Mass term m (default 0).</param>
    /// <param name="edgeWeights">Optional per-edge weights (length EdgeCount); overrides <paramref name="edgeWeight"/> when supplied.</param>
    public SpinorDiracOperator(
        SimplicialMesh mesh,
        GammaOperatorBundle gammas,
        double edgeWeight = 1.0,
        double mass = 0.0,
        double[]? edgeWeights = null)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(gammas);
        if (edgeWeights is not null && edgeWeights.Length != mesh.EdgeCount)
            throw new ArgumentException($"edgeWeights length {edgeWeights.Length} != EdgeCount {mesh.EdgeCount}.", nameof(edgeWeights));
        if (gammas.GammaMatrices.Length < 1)
            throw new ArgumentException("Gamma bundle has no gamma matrices.", nameof(gammas));

        _mesh = mesh;
        _gammas = gammas;
        _defaultWeight = edgeWeight;
        _edgeWeights = edgeWeights;
        _mass = mass;
    }

    private double Weight(int edge) => _edgeWeights is null ? _defaultWeight : _edgeWeights[edge];

    /// <summary>
    /// The UNIT frame-contracted gamma Γ̂(e) = ê·Γ for edge index
    /// <paramref name="edge"/>, delegated to the shared
    /// <see cref="EdgeGammaContraction.UnitContract"/> (single source of truth).
    /// A SpinorDimension × SpinorDimension complex matrix; null for a degenerate edge.
    /// On an axis-aligned unit edge this equals γ^μ.
    /// </summary>
    public Complex[,]? FrameGamma(int edge)
        => EdgeGamma(edge, out _);

    private Complex[,]? EdgeGamma(int edge, out double length)
    {
        int[] endpoints = _mesh.Edges[edge];
        return EdgeGammaContraction.UnitContract(
            _mesh.GetVertexCoordinates(endpoints[0]),
            _mesh.GetVertexCoordinates(endpoints[1]),
            _gammas, out length);
    }

    /// <summary>
    /// Apply the difference-form Dirac operator:
    ///   (D ψ)_v = Σ_{e ∋ v} s(v,e) w_e Γ̂(e) ( ψ_far − ψ_v ) / |e| + m ψ_v.
    /// On a constant spinor the hopping differences vanish, leaving m ψ.
    /// </summary>
    public SpinorField Apply(SpinorField psi)
    {
        ArgumentNullException.ThrowIfNull(psi);
        if (psi.SpinorDimension != SpinorDimension)
            throw new ArgumentException($"Spinor field dimension {psi.SpinorDimension} != operator SpinorDimension {SpinorDimension}.", nameof(psi));
        if (!ReferenceEquals(psi.Mesh, _mesh))
            throw new ArgumentException("Spinor field is defined on a different mesh.", nameof(psi));

        int s = SpinorDimension;
        int gauge = psi.GaugeComponents;
        var result = new SpinorField(_mesh, s, gauge);

        for (int v = 0; v < _mesh.VertexCount; v++)
        {
            int[] incident = _mesh.VertexEdges[v];
            int[] signs = _mesh.VertexEdgeOrientations[v];
            Span<Complex> outBlock = result.GetVertexSpinorMutable(v);

            for (int i = 0; i < incident.Length; i++)
            {
                int edge = incident[i];
                Complex[,]? gamma = EdgeGamma(edge, out double length);
                if (gamma is null) continue;

                int[] endpoints = _mesh.Edges[edge];
                int far = endpoints[0] == v ? endpoints[1] : endpoints[0];
                double coeff = signs[i] * Weight(edge) / length;

                for (int g = 0; g < gauge; g++)
                {
                    for (int r = 0; r < s; r++)
                    {
                        Complex acc = Complex.Zero;
                        for (int col = 0; col < s; col++)
                        {
                            Complex diff = psi.Values[psi.Index(far, g, col)] - psi.Values[psi.Index(v, g, col)];
                            acc += gamma[r, col] * diff;
                        }
                        outBlock[g * s + r] += coeff * acc;
                    }
                }
            }

            if (_mass != 0.0)
            {
                ReadOnlySpan<Complex> inBlock = psi.GetVertexSpinor(v);
                for (int k = 0; k < outBlock.Length; k++)
                    outBlock[k] += _mass * inBlock[k];
            }
        }

        return result;
    }

    /// <summary>
    /// Dense matrix of the difference-form operator exactly as <see cref="Apply"/>
    /// computes it, for the pure Clifford spinor field (gauge multiplicity 1).
    /// Dimension = (VertexCount · SpinorDimension)². Row/col index of
    /// (vertex v, spinor r) is <c>v * SpinorDimension + r</c>.
    /// </summary>
    public Complex[,] BuildMatrix()
    {
        int s = SpinorDimension;
        int dim = _mesh.VertexCount * s;
        var m = new Complex[dim, dim];

        for (int v = 0; v < _mesh.VertexCount; v++)
        {
            int[] incident = _mesh.VertexEdges[v];
            int[] signs = _mesh.VertexEdgeOrientations[v];
            for (int i = 0; i < incident.Length; i++)
            {
                int edge = incident[i];
                Complex[,]? gamma = EdgeGamma(edge, out double length);
                if (gamma is null) continue;

                double coeff = signs[i] * Weight(edge) / length;
                int[] endpoints = _mesh.Edges[edge];
                int far = endpoints[0] == v ? endpoints[1] : endpoints[0];

                for (int r = 0; r < s; r++)
                    for (int col = 0; col < s; col++)
                    {
                        m[v * s + r, far * s + col] += coeff * gamma[r, col];
                        m[v * s + r, v * s + col] -= coeff * gamma[r, col];
                    }
            }

            if (_mass != 0.0)
                for (int r = 0; r < s; r++)
                    m[v * s + r, v * s + r] += _mass;
        }

        return m;
    }

    /// <summary>
    /// Dense matrix of the massless PURE-HOPPING operator (no on-site difference
    /// term, no mass):
    ///   (H ψ)_v = Σ_{e ∋ v} s(v,e) w_e Γ̂(e) ψ_{far} / |e|.
    /// With Hermitian γ^μ (Riemannian Cl(4,0)) this matrix is exactly
    /// anti-Hermitian on any mesh, so <c>i · H</c> is Hermitian to machine
    /// precision. Dimension and index convention match <see cref="BuildMatrix"/>.
    /// </summary>
    public Complex[,] BuildHoppingMatrix()
    {
        int s = SpinorDimension;
        int dim = _mesh.VertexCount * s;
        var m = new Complex[dim, dim];

        for (int v = 0; v < _mesh.VertexCount; v++)
        {
            int[] incident = _mesh.VertexEdges[v];
            int[] signs = _mesh.VertexEdgeOrientations[v];
            for (int i = 0; i < incident.Length; i++)
            {
                int edge = incident[i];
                Complex[,]? gamma = EdgeGamma(edge, out double length);
                if (gamma is null) continue;

                double coeff = signs[i] * Weight(edge) / length;
                int[] endpoints = _mesh.Edges[edge];
                int far = endpoints[0] == v ? endpoints[1] : endpoints[0];

                for (int r = 0; r < s; r++)
                    for (int col = 0; col < s; col++)
                        m[v * s + r, far * s + col] += coeff * gamma[r, col];
            }
        }

        return m;
    }
}
