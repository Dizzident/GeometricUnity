using System.Text.Json.Serialization;

namespace Gu.Phase4.Spin;

/// <summary>
/// Specifies the chirality operator convention.
/// Only meaningful when HasChirality == true (even spacetime dimension).
/// For odd dimensions, chirality is not well-defined on Y.
/// </summary>
public sealed class ChiralityConventionSpec
{
    /// <summary>Unique identifier for this convention.</summary>
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    /// <summary>
    /// Sign convention for projectors.
    /// "left-is-minus": P_L = (I - Gamma_chi)/2  (left = -1 eigenspace)
    /// "left-is-plus":  P_L = (I + Gamma_chi)/2  (left = +1 eigenspace)
    /// Default: "left-is-minus".
    /// </summary>
    [JsonPropertyName("signConvention")]
    public required string SignConvention { get; init; }

    /// <summary>
    /// Phase factor string for the chirality operator.
    /// Gamma_chi = phaseFactor * Gamma_1 * ... * Gamma_n
    /// For Cl(2k,0): phaseFactor = i^k.
    /// Example: dimY=4 => i^2 = -1, so Gamma_chi = -Gamma_1*Gamma_2*Gamma_3*Gamma_4.
    /// Example: dimY=14 => i^7 = -i, so Gamma_chi = -i*Gamma_1*...*Gamma_14.
    /// </summary>
    [JsonPropertyName("phaseFactor")]
    public required string PhaseFactor { get; init; }

    /// <summary>Whether this dimension supports a chirality operator.</summary>
    [JsonPropertyName("hasChirality")]
    public required bool HasChirality { get; init; }

    /// <summary>
    /// Identifier for the full Y-space chirality operator (e.g. "Y-chirality").
    /// Null for odd dimY where no chirality operator exists.
    /// </summary>
    [JsonPropertyName("fullChiralityOperator")]
    public string? FullChiralityOperator { get; init; }

    /// <summary>
    /// Identifier for the base X-space chirality operator (e.g. "X-chirality").
    /// Null when dimX is odd or BaseDimension is not set.
    /// </summary>
    [JsonPropertyName("baseChiralityOperator")]
    public string? BaseChiralityOperator { get; init; }

    /// <summary>
    /// Identifier for the fiber F-space chirality operator (e.g. "F-chirality").
    /// Null when dimF is odd or FiberDimension is not set.
    /// </summary>
    [JsonPropertyName("fiberChiralityOperator")]
    public string? FiberChiralityOperator { get; init; }

    /// <summary>
    /// Dimension of the base space X. The X-chirality operator uses the first
    /// BaseDimension gamma matrices. Null if not decomposing Y = X x F.
    /// </summary>
    [JsonPropertyName("baseDimension")]
    public int? BaseDimension { get; init; }

    /// <summary>
    /// Dimension of the fiber F = Y - X. The F-chirality operator uses gammas
    /// indexed BaseDimension through SpacetimeDimension-1.
    /// Null if not decomposing Y = X x F.
    /// </summary>
    [JsonPropertyName("fiberDimension")]
    public int? FiberDimension { get; init; }
}
