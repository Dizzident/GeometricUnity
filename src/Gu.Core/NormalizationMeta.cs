using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Describes the normalization/unit policy applied to an observable or tensor quantity.
/// </summary>
public sealed class NormalizationMeta
{
    /// <summary>Normalization scheme identifier.</summary>
    [JsonPropertyName("schemeId")]
    public required string SchemeId { get; init; }

    /// <summary>Scale factor applied.</summary>
    [JsonPropertyName("scaleFactor")]
    public double ScaleFactor { get; init; } = 1.0;

    /// <summary>Description of the normalization convention.</summary>
    [JsonPropertyName("description")]
    public string? Description { get; init; }
}
