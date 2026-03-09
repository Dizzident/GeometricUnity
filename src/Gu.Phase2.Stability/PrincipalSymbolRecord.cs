using System.Text.Json.Serialization;

namespace Gu.Phase2.Stability;

/// <summary>
/// Records the principal symbol of a linearized operator at a single sample point (cell, covector).
/// Per IMPLEMENTATION_PLAN_P2.md Section 9.4.
///
/// The principal symbol sigma(x, xi) is the leading-order (highest-derivative) part of the
/// linearized PDE operator evaluated at position x with covector xi. Its eigenvalue structure
/// determines the local PDE type (elliptic, hyperbolic, mixed, degenerate).
///
/// For each sampled (x, xi):
/// - symbol matrix / matrix-free action
/// - symmetry/hermiticity flags
/// - definiteness indicators
/// - rank deficiency estimate
/// - gauge-null direction estimate
/// - branch metadata
/// </summary>
public sealed class PrincipalSymbolRecord
{
    /// <summary>Cell (face/element) index where the symbol was sampled.</summary>
    [JsonPropertyName("cellIndex")]
    public required int CellIndex { get; init; }

    /// <summary>Covector xi at which the symbol was evaluated.</summary>
    [JsonPropertyName("covector")]
    public required double[] Covector { get; init; }

    /// <summary>
    /// The symbol matrix sigma(x, xi) as a jagged array (row-major).
    /// Dimensions: [outputDim][inputDim] where inputDim = outputDim = dimG for gauge theory.
    /// </summary>
    [JsonPropertyName("symbolMatrix")]
    public required double[][] SymbolMatrix { get; init; }

    /// <summary>Eigenvalues of the symbol matrix, sorted ascending.</summary>
    [JsonPropertyName("eigenvalues")]
    public required double[] Eigenvalues { get; init; }

    /// <summary>Whether the symbol matrix is symmetric (within tolerance).</summary>
    [JsonPropertyName("isSymmetric")]
    public required bool IsSymmetric { get; init; }

    /// <summary>Symmetry error: max |S - S^T| / max |S|.</summary>
    [JsonPropertyName("symmetryError")]
    public required double SymmetryError { get; init; }

    /// <summary>
    /// Definiteness indicator: "positive-definite", "negative-definite",
    /// "positive-semidefinite", "negative-semidefinite", "indefinite", "zero".
    /// </summary>
    [JsonPropertyName("definitenessIndicator")]
    public required string DefinitenessIndicator { get; init; }

    /// <summary>
    /// Rank deficiency: number of eigenvalues below the zero threshold.
    /// 0 means full rank.
    /// </summary>
    [JsonPropertyName("rankDeficiency")]
    public required int RankDeficiency { get; init; }

    /// <summary>
    /// Estimated dimension of the gauge-null space within the symbol kernel.
    /// </summary>
    [JsonPropertyName("gaugeNullDimension")]
    public required int GaugeNullDimension { get; init; }

    /// <summary>Local PDE classification based on symbol eigenvalue structure.</summary>
    [JsonPropertyName("classification")]
    public required PdeClassification Classification { get; init; }

    /// <summary>Branch manifest ID that produced this symbol.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Gauge study mode used when computing this symbol.</summary>
    [JsonPropertyName("gaugeStudyMode")]
    public required GaugeStudyMode GaugeStudyMode { get; init; }

    /// <summary>
    /// Which operator was probed: "J" (Jacobian), "L_tilde" (gauge-fixed),
    /// "H" (Hessian).
    /// </summary>
    [JsonPropertyName("operatorId")]
    public required string OperatorId { get; init; }
}
