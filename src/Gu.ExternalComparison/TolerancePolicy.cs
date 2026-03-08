using System.Text.Json.Serialization;

namespace Gu.ExternalComparison;

/// <summary>
/// Adaptive tolerance specification for comparison checks.
/// Supports relative deviation, factor bounds, and order-of-magnitude estimates.
/// Branch sensitivity and mesh dependence inform how aggressively to tighten.
/// </summary>
public sealed class TolerancePolicy
{
    /// <summary>Tolerance type: "RelativeDeviation", "FactorBound", or "OrderEstimate".</summary>
    [JsonPropertyName("baseToleranceType")]
    public required string BaseToleranceType { get; init; }

    /// <summary>Base tolerance value (interpretation depends on BaseToleranceType).</summary>
    [JsonPropertyName("baseValue")]
    public required double BaseValue { get; init; }

    /// <summary>Branch sensitivity: "Low", "Medium", "High".</summary>
    [JsonPropertyName("branchSensitivity")]
    public string BranchSensitivity { get; init; } = "Medium";

    /// <summary>Mesh dependence: "Converging", "Stable", "Unknown".</summary>
    [JsonPropertyName("meshDependence")]
    public string MeshDependence { get; init; } = "Unknown";

    /// <summary>Optional notes for human review.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
