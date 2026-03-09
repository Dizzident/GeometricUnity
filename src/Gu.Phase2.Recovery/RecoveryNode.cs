using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase2.Recovery;

/// <summary>
/// A single node in the recovery DAG (Section 12.1).
/// Every node carries required metadata: source object type, tensor/signature metadata,
/// branch provenance, gauge dependence flag, dimensionality metadata,
/// numerical dependency status, theorem dependency status.
/// </summary>
public sealed class RecoveryNode
{
    /// <summary>Unique node identifier within the recovery graph.</summary>
    [JsonPropertyName("nodeId")]
    public required string NodeId { get; init; }

    /// <summary>Kind of node in the recovery pipeline.</summary>
    [JsonPropertyName("kind")]
    public required RecoveryNodeKind Kind { get; init; }

    /// <summary>Source object type identifier (e.g., "curvature-2form", "residual-derived-tensor").</summary>
    [JsonPropertyName("sourceObjectType")]
    public required string SourceObjectType { get; init; }

    /// <summary>Tensor/signature metadata for the object at this node, if applicable.</summary>
    [JsonPropertyName("signature")]
    public TensorSignature? Signature { get; init; }

    /// <summary>Branch provenance ID tracking which branch variant produced this node.</summary>
    [JsonPropertyName("branchProvenanceId")]
    public required string BranchProvenanceId { get; init; }

    /// <summary>Whether this node's output is gauge-dependent.</summary>
    [JsonPropertyName("gaugeDependent")]
    public required bool GaugeDependent { get; init; }

    /// <summary>Dimensionality of the space this object lives in.</summary>
    [JsonPropertyName("dimensionality")]
    public required int Dimensionality { get; init; }

    /// <summary>Numerical dependency status (e.g., "converged", "exploratory", "failed").</summary>
    [JsonPropertyName("numericalDependencyStatus")]
    public required string NumericalDependencyStatus { get; init; }

    /// <summary>Theorem dependency status (e.g., "satisfied", "partial", "unknown").</summary>
    [JsonPropertyName("theoremDependencyStatus")]
    public required string TheoremDependencyStatus { get; init; }
}
