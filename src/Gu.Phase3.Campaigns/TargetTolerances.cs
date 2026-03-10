using System.Text.Json.Serialization;

namespace Gu.Phase3.Campaigns;

/// <summary>
/// Tolerance thresholds for comparing a candidate boson against a target profile.
/// </summary>
public sealed class TargetTolerances
{
    /// <summary>Relative mass tolerance (fractional).</summary>
    [JsonPropertyName("massTolerance")]
    public double MassTolerance { get; init; } = 0.1;

    /// <summary>Absolute multiplicity tolerance.</summary>
    [JsonPropertyName("multiplicityTolerance")]
    public int MultiplicityTolerance { get; init; } = 1;

    /// <summary>Maximum gauge leak score for compatibility.</summary>
    [JsonPropertyName("maxGaugeLeakForCompatibility")]
    public double MaxGaugeLeakForCompatibility { get; init; } = 0.15;

    /// <summary>Minimum branch stability for sufficient evidence.</summary>
    [JsonPropertyName("minBranchStability")]
    public double MinBranchStability { get; init; } = 0.3;

    /// <summary>Minimum refinement stability for sufficient evidence.</summary>
    [JsonPropertyName("minRefinementStability")]
    public double MinRefinementStability { get; init; } = 0.3;
}
