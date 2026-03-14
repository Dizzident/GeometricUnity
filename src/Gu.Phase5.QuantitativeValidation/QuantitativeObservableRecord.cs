using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// A computed observable with full uncertainty decomposition (M49).
/// </summary>
public sealed class QuantitativeObservableRecord
{
    /// <summary>Observable identifier (matches external target ObservableId).</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Computed value.</summary>
    [JsonPropertyName("value")]
    public required double Value { get; init; }

    /// <summary>Uncertainty decomposition.</summary>
    [JsonPropertyName("uncertainty")]
    public required QuantitativeUncertainty Uncertainty { get; init; }

    /// <summary>Branch ID where this observable was computed.</summary>
    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    /// <summary>Environment ID where this observable was computed.</summary>
    [JsonPropertyName("environmentId")]
    public required string EnvironmentId { get; init; }

    /// <summary>Refinement level at which this observable was extracted (null = not refinement-resolved).</summary>
    [JsonPropertyName("refinementLevel")]
    public string? RefinementLevel { get; init; }

    /// <summary>Method used to extract this observable (e.g., "eigenvalue-ratio", "dispersion-fit").</summary>
    [JsonPropertyName("extractionMethod")]
    public required string ExtractionMethod { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
