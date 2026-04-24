using System.Text.Json.Serialization;

namespace Gu.Phase5.QuantitativeValidation;

/// <summary>
/// Coverage status for one external target in a quantitative validation run.
/// </summary>
public sealed class TargetCoverageRecord
{
    /// <summary>Observable identifier requested by the target.</summary>
    [JsonPropertyName("observableId")]
    public required string ObservableId { get; init; }

    /// <summary>Human-readable target label.</summary>
    [JsonPropertyName("targetLabel")]
    public required string TargetLabel { get; init; }

    /// <summary>Coverage status for this target.</summary>
    [JsonPropertyName("status")]
    public required string Status { get; init; }

    /// <summary>Number of computed observables matching the target selectors.</summary>
    [JsonPropertyName("candidateCount")]
    public required int CandidateCount { get; init; }

    /// <summary>Requested environment ID selector from the target, if any.</summary>
    [JsonPropertyName("targetEnvironmentId")]
    public string? TargetEnvironmentId { get; init; }

    /// <summary>Requested environment tier selector from the target, if any.</summary>
    [JsonPropertyName("targetEnvironmentTier")]
    public string? TargetEnvironmentTier { get; init; }

    /// <summary>Optional diagnostic note for missing or ambiguous coverage.</summary>
    [JsonPropertyName("notes")]
    public string? Notes { get; init; }
}
