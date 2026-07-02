using System.Numerics;

namespace Gu.Phase4.Spin;

/// <summary>
/// The reduced Spin(4) invariant-basis slice for Cl(4,0): the self-dual /
/// anti-self-dual projector on Λ²(T*X^4).
///
/// PHYSICS (FOUR_D_PLATFORM_PHYSICS_DECISIONS.md §3, i=2 entry): in Euclidean
/// signature the Hodge star on 2-forms satisfies *² = +1, so the 6-dimensional
/// Λ²(R^4) splits into the real self-dual (Λ²_+) and anti-self-dual (Λ²_−)
/// eigenspaces, each 3-dimensional. Under Spin(4) = SU(2)_L × SU(2)_R these are
/// invariant subspaces (Λ²_+ ≅ su(2)_L, Λ²_− ≅ su(2)_R). Projecting onto Λ²_±
/// realizes the invariant elements that carry the Ricci/Weyl split the
/// Einsteinian Shiab (M3) contracts — the structure the 2D toy provably cannot
/// hold (ShiabFamilyScopeChecker, blocked reason: Λ² is 1-dimensional on dimX=2).
///
/// RECORDED BOUNDARY (mandatory, physicist-gated): this is the REDUCED surrogate,
/// NOT the draft's [Λ^i(R^{7,7}) ⊗ u(64,64)]^{Spin(7,7)} basis. It uses Spin(4)
/// not Spin(7,7); it acts on Λ²(T*X^4) (⊗ ad components), not the ambient (7,7)
/// / u(64,64) structure. It therefore realizes none of the internal
/// su(3)×su(2)×u(1) content, the chimeric weld, or the vector-spinor 144, and no
/// result built on it may touch the Phase201 W/Z, Phase201 Higgs, or Phase256
/// observed-field contracts. <see cref="InvariantDimension"/> is recorded for the
/// reduced scope only. The full Definition 8.1 basis is a named, deferred gap.
///
/// Layout: the raw element is <c>ComponentsPerElement</c> copies of the 6-dim Λ²
/// coordinate vector, interleaved as [λ²-index · ComponentsPerElement + component]
/// (matching the codebase's [carrier · dim + component] convention). The projector
/// applies the 6×6 self-dual (or anti-self-dual) projector P_± = (I ± H)/2, with
/// H the Hodge involution, block-wise to each component. P_±² = P_± because
/// H² = I; trace P_± = 3 per component ⇒ rank (invariant dimension) 3 per
/// component.
/// </summary>
public sealed class ReducedCliffordSliceProjector : IInvariantBasisProjector
{
    /// <summary>Dimension of Λ²(R^4).</summary>
    public const int Lambda2Dimension = 6;

    /// <summary>Rank of each ± eigenspace of the Hodge star on Λ²(R^4).</summary>
    public const int EigenspaceDimension = 3;

    private readonly double[,] _projector; // 6x6, real
    private readonly int _componentsPerElement;

    /// <summary>The channel: true for self-dual (Λ²_+), false for anti-self-dual (Λ²_−).</summary>
    public bool SelfDual { get; }

    /// <inheritdoc/>
    public string ProjectorId { get; }

    /// <inheritdoc/>
    public int InvariantDimension => EigenspaceDimension * _componentsPerElement;

    /// <summary>Number of ad (endomorphism) copies of Λ² the projector acts on. Default 1.</summary>
    public int ComponentsPerElement => _componentsPerElement;

    /// <summary>Raw ambient dimension the projector consumes/produces (6 · ComponentsPerElement).</summary>
    public int RawDimension => Lambda2Dimension * _componentsPerElement;

    /// <summary>
    /// Build the reduced-slice projector.
    /// </summary>
    /// <param name="weylProjectorId">"self-dual" (default) or "anti-self-dual".</param>
    /// <param name="componentsPerElement">Number of ad copies of Λ² (default 1, pure Λ²).</param>
    public ReducedCliffordSliceProjector(string weylProjectorId = "self-dual", int componentsPerElement = 1)
    {
        if (componentsPerElement < 1)
            throw new ArgumentOutOfRangeException(nameof(componentsPerElement), componentsPerElement, "ComponentsPerElement must be >= 1.");

        SelfDual = weylProjectorId switch
        {
            "self-dual" => true,
            "anti-self-dual" => false,
            _ => throw new ArgumentException($"weylProjectorId must be \"self-dual\" or \"anti-self-dual\"; got \"{weylProjectorId}\".", nameof(weylProjectorId)),
        };

        _componentsPerElement = componentsPerElement;
        ProjectorId = $"reduced-spin4-slice/{weylProjectorId}";
        _projector = BuildProjector(SelfDual);
    }

    /// <summary>
    /// The 6×6 Hodge involution H on Λ²(R^4) in the canonical basis
    /// {01,02,03,12,13,23} (Euclidean, standard orientation):
    ///   *(01)=+23, *(02)=−13, *(03)=+12, *(12)=+03, *(13)=−02, *(23)=+01.
    /// H² = I.
    /// </summary>
    public static double[,] HodgeInvolution()
    {
        var h = new double[Lambda2Dimension, Lambda2Dimension];
        // columns = input basis, rows = output basis.
        h[5, 0] = 1;   // *(01) = +23
        h[4, 1] = -1;  // *(02) = -13
        h[3, 2] = 1;   // *(03) = +12
        h[2, 3] = 1;   // *(12) = +03
        h[1, 4] = -1;  // *(13) = -02
        h[0, 5] = 1;   // *(23) = +01
        return h;
    }

    private static double[,] BuildProjector(bool selfDual)
    {
        var h = HodgeInvolution();
        double s = selfDual ? 1.0 : -1.0;
        var p = new double[Lambda2Dimension, Lambda2Dimension];
        for (int i = 0; i < Lambda2Dimension; i++)
            for (int j = 0; j < Lambda2Dimension; j++)
                p[i, j] = 0.5 * ((i == j ? 1.0 : 0.0) + s * h[i, j]);
        return p;
    }

    /// <inheritdoc/>
    public Complex[] Project(Complex[] rawTensor)
    {
        ArgumentNullException.ThrowIfNull(rawTensor);
        if (rawTensor.Length != RawDimension)
            throw new ArgumentException($"rawTensor length {rawTensor.Length} != RawDimension {RawDimension} (6 * ComponentsPerElement {_componentsPerElement}).", nameof(rawTensor));

        int comp = _componentsPerElement;
        var result = new Complex[rawTensor.Length];
        for (int c = 0; c < comp; c++)
        {
            for (int i = 0; i < Lambda2Dimension; i++)
            {
                Complex acc = Complex.Zero;
                for (int j = 0; j < Lambda2Dimension; j++)
                {
                    double pij = _projector[i, j];
                    if (pij != 0.0)
                        acc += pij * rawTensor[j * comp + c];
                }
                result[i * comp + c] = acc;
            }
        }
        return result;
    }
}
