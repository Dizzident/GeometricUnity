namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Admissibility grading for background states (Section 4.1).
/// B0: diagnostic (low residual only),
/// B1: stationary (low stationarity norm),
/// B2: stationary + low residual (strongest).
/// </summary>
public enum AdmissibilityLevel
{
    /// <summary>
    /// Diagnostic background: ||Upsilon_h(z_*)|| &lt;= tol_residual_diagnostic.
    /// Weak; only sufficient for exploratory spectral work.
    /// </summary>
    B0,

    /// <summary>
    /// Stationary background: ||G_h(z_*)|| &lt;= tol_stationary.
    /// Default target for Phase III.
    /// </summary>
    B1,

    /// <summary>
    /// Stationary + low residual: ||G_h(z_*)|| &lt;= tol_stationary AND
    /// ||Upsilon_h(z_*)|| &lt;= tol_residual_strict.
    /// Strongest target; preferred when possible.
    /// </summary>
    B2,

    /// <summary>
    /// Background failed all admissibility checks.
    /// Retained for negative-result preservation.
    /// </summary>
    Rejected,
}
