using System.Text.Json.Serialization;

namespace Gu.Phase2.Stability;

/// <summary>
/// Records metadata about a gauge-fixed linearization (L_tilde = (J, sqrt(lambda)*C)).
/// Distinct from <see cref="LinearizationRecord"/> which captures the bare Jacobian J.
/// Per IMPLEMENTATION_PLAN_P2.md section 8.2.
/// </summary>
public sealed class GaugeFixedLinearizationRecord
{
    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundStateId")]
    public required string BackgroundStateId { get; init; }

    /// <summary>Branch manifest ID.</summary>
    [JsonPropertyName("branchManifestId")]
    public required string BranchManifestId { get; init; }

    /// <summary>Reference to the base LinearizationRecord ID (bare Jacobian).</summary>
    [JsonPropertyName("baseLinearizationId")]
    public required string BaseLinearizationId { get; init; }

    /// <summary>
    /// Gauge handling mode: "coulomb-slice", "explicit-slice", "gauge-free".
    /// </summary>
    [JsonPropertyName("gaugeHandlingMode")]
    public required string GaugeHandlingMode { get; init; }

    /// <summary>Gauge penalty weight lambda.</summary>
    [JsonPropertyName("gaugeLambda")]
    public required double GaugeLambda { get; init; }

    /// <summary>
    /// Dimension of gauge-null space of J before fixing.
    /// Expected to equal the dimension of the gauge group.
    /// </summary>
    [JsonPropertyName("gaugeNullDimension")]
    public required int GaugeNullDimension { get; init; }

    /// <summary>
    /// Whether the gauge null space was suppressed to within tolerance by C.
    /// </summary>
    [JsonPropertyName("gaugeNullSuppressed")]
    public required bool GaugeNullSuppressed { get; init; }

    /// <summary>
    /// Smallest singular value of L_tilde on the Coulomb slice.
    /// </summary>
    [JsonPropertyName("smallestSliceSingularValue")]
    public double? SmallestSliceSingularValue { get; init; }

    /// <summary>
    /// Validation status: "verified", "unverified", "failed".
    /// </summary>
    [JsonPropertyName("validationStatus")]
    public required string ValidationStatus { get; init; }
}
