using System.Text.Json.Serialization;

namespace Gu.Phase2.Stability;

/// <summary>
/// Records the gauge/slice operator C_* used in gauge-fixed linearization.
/// The gauge operator defines the slice condition: C_*(delta omega) = 0.
/// </summary>
public sealed class GaugeJacobianRecord
{
    /// <summary>Background state ID this gauge operator is associated with.</summary>
    [JsonPropertyName("backgroundStateId")]
    public required string BackgroundStateId { get; init; }

    /// <summary>
    /// Gauge handling mode: "coulomb-codifferential", "coulomb-laplacian", "l2-penalty", etc.
    /// </summary>
    [JsonPropertyName("gaugeMode")]
    public required string GaugeMode { get; init; }

    /// <summary>Gauge penalty weight lambda (if applicable).</summary>
    [JsonPropertyName("gaugeLambda")]
    public double? GaugeLambda { get; init; }

    /// <summary>Input dimension of C_* (number of connection DOFs).</summary>
    [JsonPropertyName("inputDimension")]
    public required int InputDimension { get; init; }

    /// <summary>Output dimension of C_* (number of gauge constraint DOFs).</summary>
    [JsonPropertyName("outputDimension")]
    public required int OutputDimension { get; init; }

    /// <summary>Whether the gauge operator is linear (should always be true for Phase II).</summary>
    [JsonPropertyName("isLinear")]
    public required bool IsLinear { get; init; }

    /// <summary>Reference connection used in gauge condition (null = zero).</summary>
    [JsonPropertyName("referenceConnectionLabel")]
    public string? ReferenceConnectionLabel { get; init; }
}
