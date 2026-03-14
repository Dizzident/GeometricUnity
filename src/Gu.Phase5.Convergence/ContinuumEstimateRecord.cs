using System.Text.Json.Serialization;

namespace Gu.Phase5.Convergence;

/// <summary>
/// Continuum limit estimate for one target quantity from a refinement study (M47).
/// Produced when Richardson extrapolation succeeds.
/// </summary>
public sealed class ContinuumEstimateRecord
{
    /// <summary>Target quantity identifier.</summary>
    [JsonPropertyName("quantityId")]
    public required string QuantityId { get; init; }

    /// <summary>Extrapolated continuum value Q(h→0).</summary>
    [JsonPropertyName("extrapolatedValue")]
    public required double ExtrapolatedValue { get; init; }

    /// <summary>
    /// Half-width error band: uncertainty in the continuum estimate.
    /// Estimated as |Q_finest - Q_extrapolated|.
    /// </summary>
    [JsonPropertyName("errorBand")]
    public required double ErrorBand { get; init; }

    /// <summary>Estimated convergence order p from Richardson fit.</summary>
    [JsonPropertyName("convergenceOrder")]
    public required double ConvergenceOrder { get; init; }

    /// <summary>
    /// Convergence classification:
    /// "convergent", "weakly-convergent", "non-convergent", "insufficient-data".
    /// </summary>
    [JsonPropertyName("convergenceClassification")]
    public required string ConvergenceClassification { get; init; }

    /// <summary>Human-readable confidence note from the convergence classifier.</summary>
    [JsonPropertyName("confidenceNote")]
    public required string ConfidenceNote { get; init; }

    /// <summary>Run records that produced this estimate.</summary>
    [JsonPropertyName("runRecords")]
    public required IReadOnlyList<RefinementRunRecord> RunRecords { get; init; }
}
