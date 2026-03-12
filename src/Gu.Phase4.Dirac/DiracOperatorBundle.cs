using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase4.Dirac;

/// <summary>
/// Record of an assembled Dirac operator on Y_h.
///
/// The Dirac operator is:
///   D_h = Gamma^mu * (nabla^{LC}_{spin,mu} + omega_mu^a * rho(T_a)) + M_branch + C_branch
///
/// In Phase IV:
/// - The Gamma matrices come from the GammaOperatorBundle (M33).
/// - The spin connection comes from SpinConnectionBundle (M35).
/// - M_branch and C_branch are zero in the initial implementation (set massBranchTermIncluded = false).
///
/// The explicit matrix (if assembled) is stored as a flat real array in row-major order,
/// complex entries interleaved as (re, im) pairs.
/// Shape: totalDof * totalDof * 2, where totalDof = cellCount * dofsPerCell.
/// </summary>
public sealed class DiracOperatorBundle
{
    /// <summary>Unique operator identifier.</summary>
    [JsonPropertyName("operatorId")]
    public required string OperatorId { get; init; }

    /// <summary>Fermionic background ID this operator is built on.</summary>
    [JsonPropertyName("fermionBackgroundId")]
    public required string FermionBackgroundId { get; init; }

    /// <summary>Fermionic field layout ID.</summary>
    [JsonPropertyName("layoutId")]
    public required string LayoutId { get; init; }

    /// <summary>Spin connection bundle ID used for assembly.</summary>
    [JsonPropertyName("spinConnectionId")]
    public required string SpinConnectionId { get; init; }

    /// <summary>Matrix shape as [totalDof, totalDof]. totalDof = cellCount * dofsPerCell.</summary>
    [JsonPropertyName("matrixShape")]
    public required int[] MatrixShape { get; init; }

    /// <summary>Total degrees of freedom (rows = cols = MatrixShape[0]).</summary>
    [JsonIgnore]
    public int TotalDof => MatrixShape[0];

    /// <summary>True if the explicit dense matrix is stored in ExplicitMatrix.</summary>
    [JsonPropertyName("hasExplicitMatrix")]
    public required bool HasExplicitMatrix { get; init; }

    /// <summary>
    /// Explicit complex dense matrix stored as real array of length TotalDof * TotalDof * 2.
    /// Layout: [row * TotalDof + col][re/im]. Null if HasExplicitMatrix is false.
    /// Not serialized to JSON (too large for metadata records); use binary storage.
    /// </summary>
    [JsonIgnore]
    public double[]? ExplicitMatrix { get; init; }

    /// <summary>Reference to persisted binary matrix artifact. Null if not persisted.</summary>
    [JsonPropertyName("explicitMatrixRef")]
    public string? ExplicitMatrixRef { get; init; }

    /// <summary>
    /// True if the operator is Hermitian (self-adjoint) for Riemannian signature.
    /// Computed from hermiticity diagnostic.
    /// </summary>
    [JsonPropertyName("isHermitian")]
    public required bool IsHermitian { get; init; }

    /// <summary>
    /// Relative Hermiticity residual: ||D - D^dagger|| / ||D||.
    /// 0 = perfectly Hermitian. Large value indicates assembly error.
    /// </summary>
    [JsonPropertyName("hermiticityResidual")]
    public required double HermiticityResidual { get; init; }

    /// <summary>Hermiticity tolerance used to determine IsHermitian.</summary>
    [JsonPropertyName("hermiticityTolerance")]
    public double HermiticityTolerance { get; init; } = 1e-10;

    /// <summary>True if the M_branch (torsion mass-like) term was included.</summary>
    [JsonPropertyName("massBranchTermIncluded")]
    public required bool MassBranchTermIncluded { get; init; }

    /// <summary>True if the C_branch (geometric coupling) term was included.</summary>
    [JsonPropertyName("correctionTermIncluded")]
    public required bool CorrectionTermIncluded { get; init; }

    /// <summary>True if gauge reduction was applied before assembly.</summary>
    [JsonPropertyName("gaugeReductionApplied")]
    public required bool GaugeReductionApplied { get; init; }

    /// <summary>Number of Y_h cells this operator is defined over.</summary>
    [JsonPropertyName("cellCount")]
    public required int CellCount { get; init; }

    /// <summary>Degrees of freedom per cell (spinorDim * gaugeDim).</summary>
    [JsonPropertyName("dofsPerCell")]
    public required int DofsPerCell { get; init; }

    /// <summary>Human-readable diagnostic notes.</summary>
    [JsonPropertyName("diagnosticNotes")]
    public List<string>? DiagnosticNotes { get; init; }

    /// <summary>Provenance of this bundle.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
