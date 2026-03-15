using System.Text.Json.Serialization;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// An external target value for quantitative comparison (M49).
/// Physics note: the reference campaign may mix toy-placeholder controls, derived-synthetic
/// checks, and stronger benchmark targets. None of these labels implies experimental truth.
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

    /// <summary>
    /// Distribution model for this target.
    /// Allowed values: "gaussian" (default), "gaussian-asymmetric", "student-t".
    /// </summary>
    [JsonPropertyName("distributionModel")]
    public string DistributionModel { get; init; } = "gaussian";

    /// <summary>Lower uncertainty (1-sigma equivalent) for asymmetric targets. Null = use Uncertainty.</summary>
    [JsonPropertyName("uncertaintyLower")]
    public double? UncertaintyLower { get; init; }

    /// <summary>Upper uncertainty (1-sigma equivalent) for asymmetric targets. Null = use Uncertainty.</summary>
    [JsonPropertyName("uncertaintyUpper")]
    public double? UncertaintyUpper { get; init; }

    /// <summary>Degrees of freedom for Student-t targets. Required when DistributionModel == "student-t".</summary>
    [JsonPropertyName("studentTDegreesOfFreedom")]
    public double? StudentTDegreesOfFreedom { get; init; }
}
