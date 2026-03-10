namespace Gu.Phase3.Spectra;

/// <summary>
/// Formulation for how gauge directions are handled in the spectral problem.
/// Phase III implements P1 and P2; P3 is scaffolded.
/// </summary>
public enum PhysicalModeFormulation
{
    /// <summary>
    /// P1: Penalty-fixed. Add gauge penalty to spectral operator,
    /// compute modes in full tangent space. Fastest but weakest physically.
    /// </summary>
    PenaltyFixed,

    /// <summary>
    /// P2: Projected complement. Construct P_phys and compute spectra on
    /// H_phys = P_phys^T H P_phys. Default Phase III target.
    /// </summary>
    ProjectedComplement,

    /// <summary>
    /// P3: Quotient-aware generalized eigenproblem. Scaffolded for future use.
    /// </summary>
    QuotientAware,
}
