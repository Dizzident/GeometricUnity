using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Bundle of all residual components, plus the scalar objective I2_h.
/// </summary>
public sealed class ResidualBundle
{
    /// <summary>Residual components.</summary>
    [JsonPropertyName("components")]
    public required IReadOnlyList<ResidualComponent> Components { get; init; }

    /// <summary>Objective value I2_h = (1/2) Upsilon_h^T M_Upsilon Upsilon_h.</summary>
    [JsonPropertyName("objectiveValue")]
    public required double ObjectiveValue { get; init; }

    /// <summary>Total residual norm.</summary>
    [JsonPropertyName("totalNorm")]
    public required double TotalNorm { get; init; }
}
