using System.Numerics;

namespace Gu.Phase4.Spin;

/// <summary>
/// Projector onto a structure-group-invariant subspace of a
/// (Λ^i ⊗ spinor-endomorphism) tensor space — the discrete stand-in for the
/// invariant-basis construction of Definition 8.1.
///
/// RECORDED BOUNDARY (FOUR_D_PLATFORM_PHYSICS_DECISIONS.md §3): the draft's full
/// invariant basis is [Λ^i(R^{7,7}) ⊗ u(64,64)]^{Spin(7,7)}. That ambient (7,7)
/// / u(64,64) / Spin(7,7) construction is OUT OF SCOPE for M2. This interface is
/// the stable seam; the only M2 implementation is the reduced Spin(4) slice
/// (<see cref="ReducedCliffordSliceProjector"/>), which realizes the
/// Spin(4)-invariant elements of Λ^i(T*X^4) ⊗ End(S) for the Cl(4,0) spinor
/// bundle. Implementations must record their scope honestly via
/// <see cref="ProjectorId"/> and <see cref="InvariantDimension"/>.
/// </summary>
public interface IInvariantBasisProjector
{
    /// <summary>
    /// Identifier recording the projector's scope and channel, e.g.
    /// "reduced-spin4-slice/self-dual". Carries the recorded-boundary language:
    /// a reduced-slice id must make clear it is NOT the full Definition 8.1 basis.
    /// </summary>
    string ProjectorId { get; }

    /// <summary>
    /// Dimension (rank) of the invariant subspace this projector maps onto,
    /// reported honestly for the implementation's actual scope. For the reduced
    /// Cl(4) self-dual/anti-self-dual slice this is 3 per ad component (Λ²_± is
    /// 3-dimensional), NOT the full u(64,64) invariant count.
    /// </summary>
    int InvariantDimension { get; }

    /// <summary>
    /// Project a raw (Λ^i ⊗ spinor-endomorphism) element onto the invariant
    /// subspace. The returned array lies in the same ambient space as the input
    /// (same length) but within the invariant subspace, so that
    /// Project ∘ Project == Project (idempotence).
    /// </summary>
    Complex[] Project(Complex[] rawTensor);
}
