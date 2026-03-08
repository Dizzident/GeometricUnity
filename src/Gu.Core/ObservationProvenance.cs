using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Proof that an observable was produced through the observation pipeline (sigma_h^*).
/// Only ObservationPipeline should create instances with IsVerified = true.
/// Enforces observation discipline (IA-6): no Y-quantity reaches comparison without sigma_h^*.
/// </summary>
public sealed class ObservationProvenance
{
    /// <summary>Identifier of the pullback operator used (sigma_h^* implementation).</summary>
    [JsonPropertyName("pullbackOperatorId")]
    public required string PullbackOperatorId { get; init; }

    /// <summary>Observation branch identifier from the branch manifest.</summary>
    [JsonPropertyName("observationBranchId")]
    public required string ObservationBranchId { get; init; }

    /// <summary>Derived observable transform applied, if any.</summary>
    [JsonPropertyName("transformId")]
    public string? TransformId { get; init; }

    /// <summary>
    /// Whether this observable was verified as having passed through the full pipeline.
    /// Only ObservationPipeline sets this to true.
    /// </summary>
    [JsonPropertyName("isVerified")]
    public required bool IsVerified { get; init; }

    /// <summary>Timestamp when the pipeline produced this observable.</summary>
    [JsonPropertyName("pipelineTimestamp")]
    public required DateTimeOffset PipelineTimestamp { get; init; }
}
