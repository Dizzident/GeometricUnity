using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Dashboard summary of falsification results from a Phase V campaign (M53).
/// </summary>
public sealed class FalsificationDashboard
{
    /// <summary>Total falsifiers evaluated (active + inactive).</summary>
    [JsonPropertyName("totalFalsifiers")]
    public required int TotalFalsifiers { get; init; }

    /// <summary>Count of active fatal falsifiers (triggered hard failure).</summary>
    [JsonPropertyName("activeFatalCount")]
    public required int ActiveFatalCount { get; init; }

    /// <summary>Count of active high-severity falsifiers.</summary>
    [JsonPropertyName("activeHighCount")]
    public required int ActiveHighCount { get; init; }

    /// <summary>Number of candidates that were promoted in claim class.</summary>
    [JsonPropertyName("promotionCount")]
    public required int PromotionCount { get; init; }

    /// <summary>Number of candidates that were demoted in claim class.</summary>
    [JsonPropertyName("demotionCount")]
    public required int DemotionCount { get; init; }

    /// <summary>Human-readable summary lines for reporting.</summary>
    [JsonPropertyName("summaryLines")]
    public required IReadOnlyList<string> SummaryLines { get; init; }
}
