using System.Text.Json.Serialization;

namespace Gu.Phase2.Stability;

/// <summary>
/// Specification for stability study parameters, including eigenvalue classification thresholds.
/// Thresholds must come from this spec, never hardcoded.
/// </summary>
public sealed class StabilityStudySpec
{
    /// <summary>
    /// Eigenvalues above this threshold are classified as coercive (strictly positive).
    /// Default: 1e-4.
    /// </summary>
    [JsonPropertyName("softModeThreshold")]
    public required double SoftModeThreshold { get; init; }

    /// <summary>
    /// Eigenvalues between NearKernelThreshold and SoftModeThreshold are classified as soft modes.
    /// Eigenvalues between NegativeModeThreshold and NearKernelThreshold are near-kernel.
    /// Default: 1e-8.
    /// </summary>
    [JsonPropertyName("nearKernelThreshold")]
    public required double NearKernelThreshold { get; init; }

    /// <summary>
    /// Eigenvalues below this threshold are classified as negative modes (saddle directions).
    /// Default: -1e-8.
    /// </summary>
    [JsonPropertyName("negativeModeThreshold")]
    public required double NegativeModeThreshold { get; init; }

    /// <summary>
    /// Default thresholds suitable for most stability studies.
    /// </summary>
    public static StabilityStudySpec Default => new()
    {
        SoftModeThreshold = 1e-4,
        NearKernelThreshold = 1e-8,
        NegativeModeThreshold = -1e-8,
    };
}
