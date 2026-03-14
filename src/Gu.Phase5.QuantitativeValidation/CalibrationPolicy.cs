using System.Text.Json.Serialization;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Policy for calibrated target comparison (M49).
/// Controls sigma threshold and whether full uncertainty estimates are required.
/// </summary>
public sealed class CalibrationPolicy
{
    /// <summary>Unique policy identifier.</summary>
    [JsonPropertyName("policyId")]
    public required string PolicyId { get; init; }

    /// <summary>
    /// Calibration mode: "lenient", "standard", or "strict".
    /// Used for human-readable reporting only; pass/fail is determined by SigmaThreshold.
    /// </summary>
    [JsonPropertyName("mode")]
    public required string Mode { get; init; }

    /// <summary>
    /// Maximum allowed pull for a target match to pass.
    /// pull = |computed - target| / sqrt(sigma_computed^2 + sigma_target^2)
    /// Default: 5.0 (physicist-confirmed).
    /// </summary>
    [JsonPropertyName("sigmaThreshold")]
    public double SigmaThreshold { get; init; } = 5.0;

    /// <summary>
    /// If true, reject TargetMatchRecord when computed uncertainty is unestimated (-1).
    /// If false, use only sigma_target for pull calculation when computed sigma is unknown.
    /// </summary>
    [JsonPropertyName("requireFullUncertainty")]
    public bool RequireFullUncertainty { get; init; } = false;
}
