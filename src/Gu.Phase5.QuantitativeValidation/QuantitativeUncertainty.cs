using System.Text.Json.Serialization;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Decomposed uncertainty for a quantitative observable (M49).
/// Four independent sources combined in quadrature.
/// Convention: -1 means unestimated (never zeroed — follows Phase II convention).
/// </summary>
public sealed class QuantitativeUncertainty
{
    /// <summary>Branch variation: std dev of quantity across branch variants. -1 = unestimated.</summary>
    [JsonPropertyName("branchVariation")]
    public double BranchVariation { get; init; } = -1;

    /// <summary>Refinement error: error band from continuum estimate. -1 = unestimated.</summary>
    [JsonPropertyName("refinementError")]
    public double RefinementError { get; init; } = -1;

    /// <summary>Extraction error: uncertainty from observation/extraction chain. -1 = unestimated.</summary>
    [JsonPropertyName("extractionError")]
    public double ExtractionError { get; init; } = -1;

    /// <summary>Environment sensitivity: std dev across environment tiers. -1 = unestimated.</summary>
    [JsonPropertyName("environmentSensitivity")]
    public double EnvironmentSensitivity { get; init; } = -1;

    /// <summary>Total uncertainty (quadrature sum of estimated components). -1 = unestimated.</summary>
    [JsonPropertyName("totalUncertainty")]
    public double TotalUncertainty { get; init; } = -1;

    /// <summary>Lower total uncertainty for asymmetric models. Null = use TotalUncertainty.</summary>
    [JsonPropertyName("totalUncertaintyLower")]
    public double? TotalUncertaintyLower { get; init; }

    /// <summary>Upper total uncertainty for asymmetric models. Null = use TotalUncertainty.</summary>
    [JsonPropertyName("totalUncertaintyUpper")]
    public double? TotalUncertaintyUpper { get; init; }

    /// <summary>True if all four components are estimated (>= 0).</summary>
    [JsonIgnore]
    public bool IsFullyEstimated =>
        BranchVariation >= 0 && RefinementError >= 0 &&
        ExtractionError >= 0 && EnvironmentSensitivity >= 0;
}
