using System.Text.Json.Serialization;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Result of matching a computed observable against an external target (M49).
/// </summary>
public sealed class TargetMatchRecord
{
    /// <summary>Observable identifier.</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Human-readable label for the target.</summary>
    [JsonPropertyName("targetLabel")]
    public required string TargetLabel { get; init; }

    /// <summary>Target value.</summary>
    [JsonPropertyName("targetValue")]
    public required double TargetValue { get; init; }

    /// <summary>Target uncertainty (1-sigma).</summary>
    [JsonPropertyName("targetUncertainty")]
    public required double TargetUncertainty { get; init; }

    /// <summary>Computed value.</summary>
    [JsonPropertyName("computedValue")]
    public required double ComputedValue { get; init; }

    /// <summary>Computed uncertainty (total, or -1 if unestimated).</summary>
    [JsonPropertyName("computedUncertainty")]
    public required double ComputedUncertainty { get; init; }

    /// <summary>
    /// Pull statistic: |computed - target| / sqrt(sigma_computed^2 + sigma_target^2).
    /// If computedUncertainty == -1, only sigma_target is used in denominator.
    /// </summary>
    [JsonPropertyName("pull")]
    public required double Pull { get; init; }

    /// <summary>True if pull &lt;= sigmaThreshold from the calibration policy.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Optional note (e.g., "computed uncertainty unestimated, using target sigma only").</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
