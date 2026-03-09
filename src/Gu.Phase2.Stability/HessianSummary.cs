using System.Text.Json.Serialization;

namespace Gu.Phase2.Stability;

/// <summary>
/// Summary of Hessian spectrum diagnostics for a single branch run.
/// Used as per-branch stability data in BranchRunRecord to enable
/// D_stab pairwise comparison across branch variants.
/// Per IMPLEMENTATION_PLAN_P2.md section 10.1.
/// </summary>
public sealed class HessianSummary
{
    /// <summary>Smallest eigenvalue of the Hessian H.</summary>
    [JsonPropertyName("smallestEigenvalue")]
    public required double SmallestEigenvalue { get; init; }

    /// <summary>Number of negative eigenvalues (saddle directions).</summary>
    [JsonPropertyName("negativeModeCount")]
    public required int NegativeModeCount { get; init; }

    /// <summary>Number of soft modes (small positive eigenvalues).</summary>
    [JsonPropertyName("softModeCount")]
    public required int SoftModeCount { get; init; }

    /// <summary>Number of near-kernel modes.</summary>
    [JsonPropertyName("nearKernelCount")]
    public required int NearKernelCount { get; init; }

    /// <summary>
    /// Stability classification: "strictly-positive-on-slice", "soft-modes-present",
    /// "near-zero-kernel", "negative-modes-saddle".
    /// </summary>
    [JsonPropertyName("stabilityClassification")]
    public required string StabilityClassification { get; init; }

    /// <summary>
    /// Gauge handling mode: "coulomb-slice", "explicit-slice", "gauge-free".
    /// </summary>
    [JsonPropertyName("gaugeHandlingMode")]
    public required string GaugeHandlingMode { get; init; }
}
