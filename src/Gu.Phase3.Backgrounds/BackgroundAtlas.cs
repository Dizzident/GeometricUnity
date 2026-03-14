using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// A curated collection of background records with ranking and rejection tracking (Section 7.1).
/// The atlas is the primary output of a background study.
/// </summary>
public sealed class BackgroundAtlas
{
    /// <summary>Unique atlas identifier.</summary>
    [JsonPropertyName("atlasId")]
    public required string AtlasId { get; init; }

    /// <summary>Study identifier that produced this atlas.</summary>
    [JsonPropertyName("studyId")]
    public required string StudyId { get; init; }

    /// <summary>
    /// Admitted backgrounds, ordered by ranking (best first).
    /// Only includes records with AdmissibilityLevel != Rejected.
    /// </summary>
    [JsonPropertyName("backgrounds")]
    public required IReadOnlyList<BackgroundRecord> Backgrounds { get; init; }

    /// <summary>
    /// Rejected backgrounds with reasons preserved (negative-result tracking).
    /// </summary>
    [JsonPropertyName("rejectedBackgrounds")]
    public required IReadOnlyList<BackgroundRecord> RejectedBackgrounds { get; init; }

    /// <summary>
    /// Ranking criteria used to order admitted backgrounds.
    /// </summary>
    [JsonPropertyName("rankingCriteria")]
    public required string RankingCriteria { get; init; }

    /// <summary>Total number of solve attempts (admitted + rejected).</summary>
    [JsonPropertyName("totalAttempts")]
    public required int TotalAttempts { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }

    /// <summary>Count of backgrounds at each admissibility level.</summary>
    [JsonPropertyName("admissibilityCounts")]
    public required IReadOnlyDictionary<string, int> AdmissibilityCounts { get; init; }

    /// <summary>G-003: geometry tier of the environment used (toy/structured/imported).</summary>
    [JsonPropertyName("environmentTier")]
    public string? EnvironmentTier { get; init; }
}
