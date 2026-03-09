using System.Text.Json.Serialization;

namespace Gu.Phase2.Recovery;

/// <summary>
/// Discriminates the kind of observed output produced by the extraction pipeline (Section 12.2).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ObservedOutputKind
{
    /// <summary>A tensor field on the base manifold X.</summary>
    TensorField,

    /// <summary>A scalar invariant (trace, determinant, norm, etc.).</summary>
    ScalarInvariant,

    /// <summary>A mode spectrum (eigenvalues, singular values, etc.).</summary>
    ModeSpectrum,

    /// <summary>A response curve (parameter sweep, continuation curve).</summary>
    ResponseCurve,

    /// <summary>A structural pattern (topology, symmetry, qualitative feature).</summary>
    StructuralPattern,

    /// <summary>A comparison-ready quantity with units and uncertainty.</summary>
    ComparisonQuantity,
}
