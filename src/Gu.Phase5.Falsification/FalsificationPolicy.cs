using System.Text.Json.Serialization;

namespace Gu.Phase5.Falsification;

/// <summary>
/// Policy controlling falsifier evaluation thresholds and severity assignments (M50).
/// </summary>
public sealed class FalsificationPolicy
{
    /// <summary>
    /// Fragility score above which branch-fragility falsifiers are triggered.
    /// Default: 0.5 (physicist-confirmed: fragilityScore = maxDistToNeighbor / (meanDistToFamily + eps)).
    /// </summary>
    [JsonPropertyName("branchFragilityThreshold")]
    public double BranchFragilityThreshold { get; init; } = 0.5;

    /// <summary>Severity assigned to convergence-failure falsifiers. Default: "high".</summary>
    [JsonPropertyName("convergenceFailureSeverity")]
    public string ConvergenceFailureSeverity { get; init; } = FalsifierSeverity.High;

    /// <summary>Severity assigned to quantitative-mismatch falsifiers. Default: "high".</summary>
    [JsonPropertyName("quantitativeMismatchSeverity")]
    public string QuantitativeMismatchSeverity { get; init; } = FalsifierSeverity.High;

    /// <summary>
    /// Fraction of environment tiers with large variation above which environment-instability
    /// falsifiers are triggered. Default: 0.3.
    /// </summary>
    [JsonPropertyName("environmentInstabilityThreshold")]
    public double EnvironmentInstabilityThreshold { get; init; } = 0.3;
}
