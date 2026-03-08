using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Bundle of gauge conditions for an environment.
/// </summary>
public sealed class GaugeConditionBundle
{
    /// <summary>Gauge strategy type (e.g., "penalty", "nullspace-elimination", "constrained").</summary>
    [JsonPropertyName("strategyType")]
    public required string StrategyType { get; init; }

    /// <summary>Penalty coefficient, if using penalty gauge strategy.</summary>
    [JsonPropertyName("penaltyCoefficient")]
    public double? PenaltyCoefficient { get; init; }

    /// <summary>Parameters for the gauge conditions.</summary>
    [JsonPropertyName("parameters")]
    public IReadOnlyDictionary<string, string>? Parameters { get; init; }
}
