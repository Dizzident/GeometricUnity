using System.Text.Json.Serialization;

namespace Gu.Phase2.Comparison;

/// <summary>
/// Records the calibration strategy used in a campaign (Section 13.4).
/// No hidden tuning is allowed -- all calibration must be explicit.
/// </summary>
public sealed class CalibrationRecord
{
    /// <summary>Unique calibration identifier.</summary>
    [JsonPropertyName("calibrationId")]
    public required string CalibrationId { get; init; }

    /// <summary>Calibration policy: "fixed", "fitted", "inverse", "sensitivity-only".</summary>
    [JsonPropertyName("policy")]
    public required string Policy { get; init; }

    /// <summary>Calibrated parameter values (parameter-name -> value).</summary>
    [JsonPropertyName("parameters")]
    public required IReadOnlyDictionary<string, double> Parameters { get; init; }

    /// <summary>Method used: "a-priori", "least-squares", "inverse-solve", "free".</summary>
    [JsonPropertyName("fitMethod")]
    public required string FitMethod { get; init; }
}
