using System.Text.Json.Serialization;

namespace Gu.Phase2.Stability;

/// <summary>
/// Records spectrum probe results: eigenvalues of H or singular values of L_tilde.
/// Per IMPLEMENTATION_PLAN_P2.md section 10.3.
/// </summary>
public sealed class SpectrumRecord
{
    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundStateId")]
    public required string BackgroundStateId { get; init; }

    /// <summary>Branch manifest ID.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Operator probed: "H" or "L_tilde".</summary>
    [JsonPropertyName("operatorId")]
    public required string OperatorId { get; init; }

    /// <summary>
    /// Probe method: "lanczos", "lobpcg", "randomized-svd", "shift-invert", "explicit-dense".
    /// </summary>
    [JsonPropertyName("probeMethod")]
    public required string ProbeMethod { get; init; }

    /// <summary>Number of eigenvalues/singular values requested.</summary>
    [JsonPropertyName("requestedCount")]
    public required int RequestedCount { get; init; }

    /// <summary>Number actually obtained (may be less if convergence issues).</summary>
    [JsonPropertyName("obtainedCount")]
    public required int ObtainedCount { get; init; }

    /// <summary>
    /// The computed eigenvalues (for H) or singular values (for L_tilde),
    /// sorted ascending.
    /// </summary>
    [JsonPropertyName("values")]
    public required double[] Values { get; init; }

    /// <summary>
    /// Convergence status: "converged", "partially-converged", "failed".
    /// </summary>
    [JsonPropertyName("convergenceStatus")]
    public required string ConvergenceStatus { get; init; }

    /// <summary>Residual norms for each computed eigenpair (if available).</summary>
    [JsonPropertyName("residualNorms")]
    public double[]? ResidualNorms { get; init; }

    /// <summary>Gauge handling mode.</summary>
    [JsonPropertyName("gaugeHandlingMode")]
    public required string GaugeHandlingMode { get; init; }

    /// <summary>Normalization convention: "unit-l2", "unit-mass-weighted".</summary>
    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    /// <summary>Whether modes are in native Y-space or observed X-space projected.</summary>
    [JsonPropertyName("modeSpace")]
    public required string ModeSpace { get; init; }

    /// <summary>
    /// Stability interpretation based on spectrum semantics (section 9.5).
    /// </summary>
    [JsonPropertyName("stabilityInterpretation")]
    public string? StabilityInterpretation { get; init; }
}
