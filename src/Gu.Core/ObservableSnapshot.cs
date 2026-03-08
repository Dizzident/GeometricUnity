using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// A single observed quantity extracted through the observation pipeline.
/// </summary>
public sealed class ObservableSnapshot
{
    /// <summary>Observable identifier.</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Output type classification.</summary>
    [JsonPropertyName("outputType")]
    public required OutputType OutputType { get; init; }

    /// <summary>The observed values.</summary>
    [JsonPropertyName("values")]
    public required double[] Values { get; init; }

    /// <summary>Normalization metadata applied to produce these values.</summary>
    [JsonPropertyName("normalization")]
    public NormalizationMeta? Normalization { get; init; }

    /// <summary>Tensor signature of the observable.</summary>
    [JsonPropertyName("signature")]
    public TensorSignature? Signature { get; init; }

    /// <summary>
    /// Provenance proving this observable passed through the observation pipeline (sigma_h^*).
    /// Only set by ObservationPipeline. Null means no provenance (not pipeline-verified).
    /// </summary>
    [JsonPropertyName("provenance")]
    public ObservationProvenance? Provenance { get; init; }
}
