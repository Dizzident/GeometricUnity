using System.Text.Json.Serialization;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// An external target value for quantitative comparison (M49).
/// Physics note: all targets for M53 must carry targetProvenance="synthetic-toy-v1"
/// and evidenceTier="toy-placeholder". Never label as physical predictions.
/// </summary>
public sealed class ExternalTarget
{
    /// <summary>Human-readable label for this target.</summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>Observable ID this target applies to.</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Target value.</summary>
    [JsonPropertyName("value")]
    public required double Value { get; init; }

    /// <summary>Target uncertainty (1-sigma equivalent).</summary>
    [JsonPropertyName("uncertainty")]
    public required double Uncertainty { get; init; }

    /// <summary>Provenance string for this target (e.g., "synthetic-toy-v1").</summary>
    [JsonPropertyName("source")]
    public required string Source { get; init; }

    /// <summary>Evidence tier (e.g., "toy-placeholder").</summary>
    [JsonPropertyName("evidenceTier")]
    public string? EvidenceTier { get; init; }
}
