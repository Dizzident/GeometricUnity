using System.Text.Json.Serialization;

namespace Gu.Phase4.Chirality;

/// <summary>
/// Summary of gauge leak across a set of fermionic modes.
///
/// A mode with high gauge leak has most of its weight in pure gauge directions
/// (zero modes of the physical operator). Such modes are unphysical and should
/// be filtered before comparison.
/// </summary>
public sealed class GaugeLeakSummary
{
    /// <summary>Background ID this summary applies to.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Total number of modes analyzed.</summary>
    [JsonPropertyName("totalModes")]
    public required int TotalModes { get; init; }

    /// <summary>Number of modes with gaugeLeakScore > leakThreshold.</summary>
    [JsonPropertyName("highLeakModeCount")]
    public required int HighLeakModeCount { get; init; }

    /// <summary>Threshold used to classify high-leak modes.</summary>
    [JsonPropertyName("leakThreshold")]
    public required double LeakThreshold { get; init; }

    /// <summary>Mean gauge leak score across all modes.</summary>
    [JsonPropertyName("meanLeakScore")]
    public required double MeanLeakScore { get; init; }

    /// <summary>Maximum gauge leak score across all modes.</summary>
    [JsonPropertyName("maxLeakScore")]
    public required double MaxLeakScore { get; init; }

    /// <summary>IDs of modes with high gauge leak.</summary>
    [JsonPropertyName("highLeakModeIds")]
    public required List<string> HighLeakModeIds { get; init; }
}
