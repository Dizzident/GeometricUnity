namespace Gu.Phase3.Observables;

/// <summary>
/// Declares how an observed mode signature was computed.
/// Every observed signature must declare its linearization method.
/// </summary>
public enum LinearizationMethod
{
    /// <summary>Analytic / chain-rule linearization.</summary>
    Analytic,

    /// <summary>Finite-difference approximation.</summary>
    FiniteDifference,

    /// <summary>Hybrid: analytic with finite-difference correction.</summary>
    Hybrid,
}
