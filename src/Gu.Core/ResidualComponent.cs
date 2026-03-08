using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// A single component of the residual field.
/// </summary>
public sealed class ResidualComponent
{
    /// <summary>Component label.</summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>Norm of this component.</summary>
    [JsonPropertyName("norm")]
    public required double Norm { get; init; }

    /// <summary>Field data for this component.</summary>
    [JsonPropertyName("field")]
    public required FieldTensor Field { get; init; }
}
