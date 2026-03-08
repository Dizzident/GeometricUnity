using Gu.Core;

namespace Gu.Branching;

/// <summary>
/// Produces a pair of connections (A_omega, B_omega) from the active connection
/// and distinguished connection. Branch-defined per Section 4.6.
/// </summary>
public interface IBiConnectionStrategy
{
    /// <summary>Strategy identifier for manifest serialization.</summary>
    string StrategyId { get; }

    /// <summary>
    /// Compute the bi-connection pair.
    /// </summary>
    BiConnectionResult Evaluate(
        FieldTensor omega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry);

    /// <summary>
    /// Linearization: d(A,B)/d(omega) acting on perturbation.
    /// Returns (dA, dB) given delta_omega.
    /// </summary>
    BiConnectionResult Linearize(
        FieldTensor omega,
        FieldTensor a0,
        FieldTensor deltaOmega,
        BranchManifest manifest,
        GeometryContext geometry);
}

/// <summary>
/// Result of bi-connection construction: a pair (A, B) of connection-valued fields.
/// </summary>
public sealed class BiConnectionResult
{
    /// <summary>First connection A_omega (typically background-derived).</summary>
    public required FieldTensor ConnectionA { get; init; }

    /// <summary>Second connection B_omega (typically active-derived).</summary>
    public required FieldTensor ConnectionB { get; init; }
}
