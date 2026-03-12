using System.Text.Json.Serialization;

namespace Gu.Phase4.Registry;

/// <summary>
/// Configuration for the RegistryMergeEngine (M42).
/// Controls thresholds for claim class assignment and demotion rules.
/// </summary>
public sealed class RegistryMergeConfig
{
    /// <summary>Registry version string embedded in all output records.</summary>
    [JsonPropertyName("registryVersion")]
    public string RegistryVersion { get; init; } = "1.0.0";

    /// <summary>
    /// Minimum branch persistence score for a fermion candidate to reach C2.
    /// Default 0.8 (80% persistence across branch variants).
    /// </summary>
    [JsonPropertyName("minBranchPersistenceForC2")]
    public double MinBranchPersistenceForC2 { get; init; } = 0.8;

    /// <summary>
    /// Minimum branch persistence score below which C2+ candidates are demoted to C1.
    /// Default 0.5.
    /// </summary>
    [JsonPropertyName("minBranchPersistenceThreshold")]
    public double MinBranchPersistenceThreshold { get; init; } = 0.5;

    /// <summary>
    /// Minimum observation confidence for a candidate to retain C3+ claim class.
    /// Default 0.5.
    /// </summary>
    [JsonPropertyName("minObservationConfidence")]
    public double MinObservationConfidence { get; init; } = 0.5;

    /// <summary>
    /// Ambiguity count threshold above which the AmbiguousMatching demotion is triggered.
    /// Default 0 (any ambiguity triggers demotion for boson records).
    /// </summary>
    [JsonPropertyName("ambiguityCountThreshold")]
    public int AmbiguityCountThreshold { get; init; } = 0;

    /// <summary>
    /// Ambiguity score threshold (for cluster records) above which AmbiguousMatching
    /// demotion is triggered. Default 0.5 (majority of members ambiguous).
    /// </summary>
    [JsonPropertyName("ambiguityScoreThreshold")]
    public double AmbiguityScoreThreshold { get; init; } = 0.5;

    /// <summary>
    /// Minimum coupling branch stability score for an interaction candidate to reach C1.
    /// Default 0.5.
    /// </summary>
    [JsonPropertyName("minBranchStabilityForInteractionC1")]
    public double MinBranchStabilityForInteractionC1 { get; init; } = 0.5;

    /// <summary>Default configuration.</summary>
    public static RegistryMergeConfig Default { get; } = new();
}
