using System.Text.Json.Serialization;

namespace Gu.Phase5.Reporting;

/// <summary>
/// Summary atlas of convergence/continuum results from a Phase V campaign (M53).
/// </summary>
public sealed class ConvergenceAtlas
{
    /// <summary>Total number of target quantities analyzed for convergence.</summary>
    [JsonPropertyName("totalQuantities")]
    public required int TotalQuantities { get; init; }

    /// <summary>Number of quantities that converged (produced ContinuumEstimateRecord).</summary>
    [JsonPropertyName("convergentCount")]
    public required int ConvergentCount { get; init; }

    /// <summary>Number of quantities that did not converge.</summary>
    [JsonPropertyName("nonConvergentCount")]
    public required int NonConvergentCount { get; init; }

    /// <summary>Number of quantities with insufficient refinement data.</summary>
    [JsonPropertyName("insufficientDataCount")]
    public required int InsufficientDataCount { get; init; }

    /// <summary>Human-readable summary lines for reporting.</summary>
    [JsonPropertyName("summaryLines")]
    public required IReadOnlyList<string> SummaryLines { get; init; }
}
