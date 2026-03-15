using System.Text.Json.Serialization;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Specification for a background atlas study.
/// Defines which environments, branches, seeds, and solve options to use.
/// </summary>
public sealed class BackgroundStudySpec
{
    /// <summary>Unique study identifier.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>Background specifications to solve.</summary>
    [JsonPropertyName("specs")]
    public required IReadOnlyList<BackgroundSpec> Specs { get; init; }

    /// <summary>
    /// State distance threshold for deduplication.
    /// Backgrounds closer than this threshold in state space are considered duplicates.
    /// </summary>
    [JsonPropertyName("deduplicationThreshold")]
    public double DeduplicationThreshold { get; init; } = 1e-6;

    /// <summary>
    /// Ranking criteria: "residual-then-stationarity", "stationarity-then-residual",
    /// or "admissibility-then-residual".
    /// </summary>
    [JsonPropertyName("rankingCriteria")]
    public string RankingCriteria { get; init; } = "admissibility-then-residual";

    /// <summary>Human-readable notes.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }

    /// <summary>G-003: geometry tier of the environment used (toy/structured/imported).</summary>
    [JsonPropertyName("environmentTier")]
    public string? EnvironmentTier { get; init; }

    /// <summary>
    /// D-001: ordered list of directory paths to search for branch manifest files.
    /// Each path is resolved relative to the study JSON file if relative.
    /// Used as the third fallback in manifest resolution order.
    /// </summary>
    [JsonPropertyName("manifestSearchPaths")]
    public IReadOnlyList<string>? ManifestSearchPaths { get; init; }
}
