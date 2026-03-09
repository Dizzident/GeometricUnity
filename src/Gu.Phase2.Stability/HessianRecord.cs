using System.Text.Json.Serialization;

namespace Gu.Phase2.Stability;

/// <summary>
/// Records metadata about the Hessian-style operator H = L_tilde^* L_tilde
/// at a background state. H governs local stability, soft modes, and
/// near-kernel structure.
/// </summary>
public sealed class HessianRecord
{
    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundStateId")]
    public required string BackgroundStateId { get; init; }

    /// <summary>Branch manifest ID.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Gauge handling mode used in L_tilde.</summary>
    [JsonPropertyName("gaugeHandlingMode")]
    public required string GaugeHandlingMode { get; init; }

    /// <summary>Gauge weight lambda (scaling of gauge block in L_tilde).</summary>
    [JsonPropertyName("gaugeLambda")]
    public required double GaugeLambda { get; init; }

    /// <summary>Dimension of H (square operator on connection DOFs).</summary>
    [JsonPropertyName("dimension")]
    public required int Dimension { get; init; }

    /// <summary>Assembly mode: "matrix-free" or "explicit-sparse".</summary>
    [JsonPropertyName("assemblyMode")]
    public required string AssemblyMode { get; init; }

    /// <summary>
    /// Whether H was verified to be symmetric (via random dot-product test).
    /// </summary>
    [JsonPropertyName("symmetryVerified")]
    public required bool SymmetryVerified { get; init; }

    /// <summary>Symmetry error: max |&lt;Hx,y&gt; - &lt;x,Hy&gt;| / (||x|| ||y||).</summary>
    [JsonPropertyName("symmetryError")]
    public double? SymmetryError { get; init; }

    /// <summary>Number of strictly positive (coercive) eigenvalues above soft threshold.</summary>
    [JsonPropertyName("coerciveModeCount")]
    public int? CoerciveModeCount { get; init; }

    /// <summary>Number of small positive eigenvalues (between near-kernel and soft thresholds).</summary>
    [JsonPropertyName("softModeCount")]
    public int? SoftModeCount { get; init; }

    /// <summary>Number of eigenvalues near zero (between negative and near-kernel thresholds).</summary>
    [JsonPropertyName("nearKernelCount")]
    public int? NearKernelCount { get; init; }

    /// <summary>Number of negative eigenvalues (below negative mode threshold).</summary>
    [JsonPropertyName("negativeModeCount")]
    public int? NegativeModeCount { get; init; }
}
