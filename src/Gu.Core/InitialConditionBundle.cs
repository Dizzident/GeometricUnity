using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Bundle of initial conditions for an environment.
/// </summary>
public sealed class InitialConditionBundle
{
    /// <summary>Initial condition type (e.g., "flat-connection", "random-perturbation").</summary>
    [JsonPropertyName("conditionType")]
    public required string ConditionType { get; init; }

    /// <summary>Optional initial field data.</summary>
    [JsonPropertyName("initialField")]
    public FieldTensor? InitialField { get; init; }

    /// <summary>Parameters for the initial conditions.</summary>
    [JsonPropertyName("parameters")]
    public IReadOnlyDictionary<string, string>? Parameters { get; init; }
}
