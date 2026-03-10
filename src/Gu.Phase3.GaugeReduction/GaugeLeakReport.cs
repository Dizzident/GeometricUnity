using System.Text.Json.Serialization;

namespace Gu.Phase3.GaugeReduction;

/// <summary>
/// Report on gauge leakage of trial vectors or computed modes.
///
/// For each tested vector, records the fraction of its norm that lies
/// in the gauge subspace. Low leak scores indicate physically meaningful modes;
/// high leak scores indicate gauge artifacts.
/// </summary>
public sealed class GaugeLeakReport
{
    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Gauge rank used for leak computation.</summary>
    [JsonPropertyName("gaugeRank")]
    public required int GaugeRank { get; init; }

    /// <summary>Per-vector leak entries.</summary>
    [JsonPropertyName("entries")]
    public required IReadOnlyList<GaugeLeakEntry> Entries { get; init; }

    /// <summary>Maximum leak score across all entries.</summary>
    [JsonPropertyName("maxLeakScore")]
    public double MaxLeakScore { get; init; }

    /// <summary>Mean leak score across all entries.</summary>
    [JsonPropertyName("meanLeakScore")]
    public double MeanLeakScore { get; init; }

    /// <summary>
    /// Spectral gap between gauge and physical sectors.
    ///
    /// Defined as: smallest gauge singular value / largest physical-sector
    /// singular value (from the gauge SVD). A large gap indicates clean
    /// separation; a small gap warns of numerical mixing near boundaries.
    ///
    /// Set to null if not computed.
    /// </summary>
    [JsonPropertyName("spectralGap")]
    public double? SpectralGap { get; init; }
}

/// <summary>
/// Single entry in a gauge leak report.
/// </summary>
public sealed class GaugeLeakEntry
{
    /// <summary>Label identifying the tested vector (e.g., "mode-0", "trial-3").</summary>
    [JsonPropertyName("vectorLabel")]
    public required string VectorLabel { get; init; }

    /// <summary>Gauge leak score: ||P_gauge(v)|| / ||v||. Range [0, 1].</summary>
    [JsonPropertyName("leakScore")]
    public required double LeakScore { get; init; }

    /// <summary>Norm of the gauge projection ||P_gauge(v)||.</summary>
    [JsonPropertyName("gaugeNorm")]
    public required double GaugeNorm { get; init; }

    /// <summary>Total norm ||v||.</summary>
    [JsonPropertyName("totalNorm")]
    public required double TotalNorm { get; init; }
}
