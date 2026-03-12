namespace Gu.Phase4.Couplings;

/// <summary>
/// Configuration for the boson-fermion coupling proxy extraction.
/// </summary>
public sealed class CouplingExtractionConfig
{
    /// <summary>
    /// Normalization convention for coupling proxies.
    /// Valid values: "raw", "unit-modes", "unit-boson".
    /// Default: "unit-modes".
    /// </summary>
    public string NormalizationConvention { get; init; } = "unit-modes";

    /// <summary>
    /// Epsilon for finite-difference variation.
    /// Only used when VariationMethod = "finite-difference".
    /// Default: 1e-5.
    /// </summary>
    public double FiniteDiffEpsilon { get; init; } = 1e-5;

    /// <summary>
    /// Method to compute the Dirac variation.
    /// Valid values: "analytical", "finite-difference".
    /// Default: "analytical".
    /// </summary>
    public string VariationMethod { get; init; } = "analytical";

    /// <summary>
    /// If true, skip (i, j) pairs where i == j (diagonal couplings).
    /// Default: false (all pairs included).
    /// </summary>
    public bool SkipDiagonal { get; init; } = false;

    /// <summary>
    /// Threshold below which a coupling proxy magnitude is considered zero.
    /// Couplings below this threshold receive a SelectionRuleNote.
    /// Default: 1e-10.
    /// </summary>
    public double ZeroThreshold { get; init; } = 1e-10;

    /// <summary>Default configuration.</summary>
    public static CouplingExtractionConfig Default => new();
}
