using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase2.Semantics;

/// <summary>
/// Describes the field-content layout for a branch variant.
/// Phase I used a single connection block; Phase II allows multi-block layouts
/// while remaining backward-compatible.
/// </summary>
public sealed class BranchFieldLayout
{
    /// <summary>Connection blocks in this layout (at least one required).</summary>
    [JsonPropertyName("connectionBlocks")]
    public required IReadOnlyList<FieldBlockDescriptor> ConnectionBlocks { get; init; }

    /// <summary>Auxiliary bosonic blocks (may be empty for Phase I compatibility).</summary>
    [JsonPropertyName("auxiliaryBosonicBlocks")]
    public required IReadOnlyList<FieldBlockDescriptor> AuxiliaryBosonicBlocks { get; init; }

    /// <summary>Gauge action rules describing how gauge acts on each block.</summary>
    [JsonPropertyName("gaugeActionRules")]
    public required IReadOnlyList<string> GaugeActionRules { get; init; }

    /// <summary>Which blocks are eligible for observation/extraction.</summary>
    [JsonPropertyName("observationEligibility")]
    public required IReadOnlyList<string> ObservationEligibility { get; init; }

    /// <summary>
    /// Creates a single-connection-block layout compatible with Phase I (A = omega, no auxiliaries).
    /// </summary>
    public static BranchFieldLayout CreatePhase1Compatible(string connectionBlockId = "omega")
    {
        return new BranchFieldLayout
        {
            ConnectionBlocks = new[]
            {
                new FieldBlockDescriptor
                {
                    BlockId = connectionBlockId,
                    FieldType = "connection",
                    DofCount = 0,
                    Signature = new TensorSignature
                    {
                        AmbientSpaceId = "Y_h",
                        CarrierType = "connection-1form",
                        Degree = "1",
                        LieAlgebraBasisId = "canonical",
                        ComponentOrderId = "edge-major",
                        MemoryLayout = "dense-row-major",
                    },
                }
            },
            AuxiliaryBosonicBlocks = Array.Empty<FieldBlockDescriptor>(),
            GaugeActionRules = new[] { "full-gauge-on-" + connectionBlockId },
            ObservationEligibility = new[] { connectionBlockId },
        };
    }
}

/// <summary>
/// Describes a single field block within a BranchFieldLayout.
/// </summary>
public sealed class FieldBlockDescriptor
{
    /// <summary>Unique block identifier within the layout.</summary>
    [JsonPropertyName("blockId")]
    public required string BlockId { get; init; }

    /// <summary>Field type: "connection", "auxiliary-scalar", etc.</summary>
    [JsonPropertyName("fieldType")]
    public required string FieldType { get; init; }

    /// <summary>Degrees of freedom count for this block.</summary>
    [JsonPropertyName("dofCount")]
    public required int DofCount { get; init; }

    /// <summary>Tensor signature for this block.</summary>
    [JsonPropertyName("signature")]
    public required TensorSignature Signature { get; init; }
}
