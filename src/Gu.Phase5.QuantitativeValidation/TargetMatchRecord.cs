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

    /// <summary>Environment ID of the computed observable that supplied this match.</summary>
    [JsonPropertyName("computedEnvironmentId")]
    public string? ComputedEnvironmentId { get; init; }

    /// <summary>Environment tier of the computed observable that supplied this match.</summary>
    [JsonPropertyName("computedEnvironmentTier")]
    public string? ComputedEnvironmentTier { get; init; }

    /// <summary>Branch ID of the computed observable that supplied this match.</summary>
    [JsonPropertyName("computedBranchId")]
    public string? ComputedBranchId { get; init; }

    /// <summary>Refinement level of the computed observable that supplied this match.</summary>
    [JsonPropertyName("computedRefinementLevel")]
    public string? ComputedRefinementLevel { get; init; }

    /// <summary>Target source label carried through from the external target table.</summary>
    [JsonPropertyName("targetSource")]
    public string? TargetSource { get; init; }

    /// <summary>Optional target provenance label carried through from the external target table.</summary>
    [JsonPropertyName("targetProvenance")]
    public string? TargetProvenance { get; init; }

    /// <summary>Evidence tier for the matched target.</summary>
    [JsonPropertyName("targetEvidenceTier")]
    public string? TargetEvidenceTier { get; init; }

    /// <summary>Benchmark class for the matched target.</summary>
    [JsonPropertyName("targetBenchmarkClass")]
    public string? TargetBenchmarkClass { get; init; }

    /// <summary>Requested environment ID selector from the target, if any.</summary>
    [JsonPropertyName("targetEnvironmentId")]
    public string? TargetEnvironmentId { get; init; }

    /// <summary>Requested environment tier selector from the target, if any.</summary>
    [JsonPropertyName("targetEnvironmentTier")]
    public string? TargetEnvironmentTier { get; init; }
}
