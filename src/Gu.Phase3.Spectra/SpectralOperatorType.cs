namespace Gu.Phase3.Spectra;

/// <summary>
/// Declares which spectral operator produced a spectrum.
/// PHYSICS CONSTRAINT #1: GN is ONLY valid for B2-level backgrounds (small residual).
/// Every spectrum must declare which operator produced it. Never default to GN silently.
/// </summary>
public enum SpectralOperatorType
{
    /// <summary>
    /// Gauss-Newton approximation: H_GN = J^T M_Upsilon J.
    /// ONLY valid for B2-level backgrounds where ||Upsilon_*|| is small.
    /// </summary>
    GaussNewton,

    /// <summary>
    /// Full Hessian: H_full = H_GN + residual correction term.
    /// Valid for any admissible background.
    /// </summary>
    FullHessian,
}
