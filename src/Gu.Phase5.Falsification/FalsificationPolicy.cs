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

    /// <summary>Severity assigned to environment-instability falsifiers. Default: "high".</summary>
    [JsonPropertyName("environmentInstabilitySeverity")]
    public string EnvironmentInstabilitySeverity { get; init; } = FalsifierSeverity.High;

    /// <summary>
    /// Sensitivity score above which observation-instability falsifiers are triggered.
    /// Default: 0.3 (WP-7).
    /// </summary>
    [JsonPropertyName("observationInstabilityThreshold")]
    public double ObservationInstabilityThreshold { get; init; } = 0.3;

    /// <summary>
    /// Structural mismatch score above which representation-content falsifiers are triggered
    /// at High severity. Fatal severity is triggered independently when MissingRequiredCount > 0.
    /// Default: 0.2 (WP-7).
    /// </summary>
    [JsonPropertyName("representationContentThreshold")]
    public double RepresentationContentThreshold { get; init; } = 0.2;

    /// <summary>Severity assigned to representation-content falsifiers. Default: "fatal".</summary>
    [JsonPropertyName("representationContentSeverity")]
    public string RepresentationContentSeverity { get; init; } = FalsifierSeverity.Fatal;

    /// <summary>
    /// Relative spread threshold above which coupling-inconsistency falsifiers are triggered.
    /// Default: 0.3 (WP-7).
    /// </summary>
    [JsonPropertyName("couplingInconsistencyThreshold")]
    public double CouplingInconsistencyThreshold { get; init; } = 0.3;

    /// <summary>Severity assigned to coupling-inconsistency falsifiers. Default: "high".</summary>
    [JsonPropertyName("couplingInconsistencySeverity")]
    public string CouplingInconsistencySeverity { get; init; } = FalsifierSeverity.High;
}
