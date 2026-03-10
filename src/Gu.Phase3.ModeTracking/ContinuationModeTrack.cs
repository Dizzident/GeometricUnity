using System.Text.Json.Serialization;

namespace Gu.Phase3.ModeTracking;

/// <summary>
/// Tracks modes along a continuation path (sequence of backgrounds
/// parameterized by a continuous parameter).
/// </summary>
public sealed class ContinuationModeTrack
{
    /// <summary>Unique track identifier.</summary>
    [JsonPropertyName("trackId")]
    public required string TrackId { get; init; }

    /// <summary>Background IDs along the continuation path (ordered).</summary>
    [JsonPropertyName("pathBackgroundIds")]
    public required IReadOnlyList<string> PathBackgroundIds { get; init; }

    /// <summary>Mode families built from tracking along the path.</summary>
    [JsonPropertyName("families")]
    public required IReadOnlyList<ModeFamilyRecord> Families { get; init; }

    /// <summary>Number of families that persist across the full path.</summary>
    [JsonPropertyName("persistentFamilyCount")]
    public int PersistentFamilyCount =>
        Families.Count(f => f.ContextIds.Count == PathBackgroundIds.Count);

    /// <summary>Total number of ambiguous matches along the path.</summary>
    [JsonPropertyName("totalAmbiguities")]
    public int TotalAmbiguities => Families.Sum(f => f.AmbiguityCount);
}
