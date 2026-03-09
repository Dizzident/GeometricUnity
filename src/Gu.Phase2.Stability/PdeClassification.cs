using System.Text.Json.Serialization;

namespace Gu.Phase2.Stability;

/// <summary>
/// Local PDE classification at a sample point (cell, covector).
/// Based on the principal symbol eigenvalue structure.
/// These are numerical/study semantics only -- they do not by themselves prove a theorem.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PdeClassification
{
    /// <summary>All symbol eigenvalues have the same sign (positive or negative definite).</summary>
    EllipticLike,

    /// <summary>Symbol eigenvalues have mixed signs (indefinite, non-degenerate).</summary>
    HyperbolicLike,

    /// <summary>Some directions are elliptic-like, others degenerate or mixed.</summary>
    Mixed,

    /// <summary>Symbol has zero eigenvalues (rank-deficient) beyond expected gauge directions.</summary>
    Degenerate,

    /// <summary>Classification could not be determined (e.g., numerical issues).</summary>
    Unresolved,
}

/// <summary>
/// Study mode for PDE analysis: whether gauge is fixed or free.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum GaugeStudyMode
{
    /// <summary>Analysis with no gauge fixing applied (raw operator).</summary>
    GaugeFree,

    /// <summary>Analysis with gauge fixing applied (e.g., Coulomb slice).</summary>
    GaugeFixed,
}
