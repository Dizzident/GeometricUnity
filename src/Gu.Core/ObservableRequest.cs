using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// A request for a specific observable to be extracted from the observation pipeline.
/// </summary>
public sealed class ObservableRequest
{
    /// <summary>Observable identifier.</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Expected output type classification.</summary>
    [JsonPropertyName("outputType")]
    public required OutputType OutputType { get; init; }

    /// <summary>Optional normalization to apply.</summary>
    [JsonPropertyName("normalization")]
    public NormalizationMeta? Normalization { get; init; }
}
