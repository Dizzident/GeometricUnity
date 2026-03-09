using System.Text.Json.Serialization;

namespace Gu.Phase2.Recovery;

/// <summary>
/// An edge in the recovery DAG connecting two nodes.
/// Edges carry the operator identifier describing the transformation applied.
/// </summary>
public sealed class RecoveryEdge
{
    /// <summary>Source node identifier.</summary>
    [JsonPropertyName("fromNodeId")]
    public required string FromNodeId { get; init; }

    /// <summary>Target node identifier.</summary>
    [JsonPropertyName("toNodeId")]
    public required string ToNodeId { get; init; }

    /// <summary>Operator identifier (e.g., "sigma_h_star", "trace-projector").</summary>
    [JsonPropertyName("operatorId")]
    public required string OperatorId { get; init; }
}
