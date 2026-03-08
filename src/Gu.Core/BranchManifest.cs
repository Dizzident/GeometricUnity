using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// The single most important control object (Section 10.1).
/// Defines the active branch configuration for a Minimal GU v1 run.
/// No run can start without a valid branch manifest.
/// </summary>
public sealed class BranchManifest
{
    /// <summary>Unique branch identifier.</summary>
    [JsonPropertyName("branchId")]
    public required string BranchId { get; init; }

    /// <summary>Schema version for this manifest format.</summary>
    [JsonPropertyName("schemaVersion")]
    public required string SchemaVersion { get; init; }

    /// <summary>Source-equation revision that this branch is based on.</summary>
    [JsonPropertyName("sourceEquationRevision")]
    public required string SourceEquationRevision { get; init; }

    /// <summary>Code revision (e.g., git SHA) that built this manifest.</summary>
    [JsonPropertyName("codeRevision")]
    public required string CodeRevision { get; init; }

    /// <summary>Active geometry branch identifier.</summary>
    [JsonPropertyName("activeGeometryBranch")]
    public required string ActiveGeometryBranch { get; init; }

    /// <summary>Active observation branch identifier.</summary>
    [JsonPropertyName("activeObservationBranch")]
    public required string ActiveObservationBranch { get; init; }

    /// <summary>Active torsion branch identifier (IX-1).</summary>
    [JsonPropertyName("activeTorsionBranch")]
    public required string ActiveTorsionBranch { get; init; }

    /// <summary>Active Shiab branch identifier (IX-2).</summary>
    [JsonPropertyName("activeShiabBranch")]
    public required string ActiveShiabBranch { get; init; }

    /// <summary>Active gauge strategy (IA-4).</summary>
    [JsonPropertyName("activeGaugeStrategy")]
    public required string ActiveGaugeStrategy { get; init; }

    /// <summary>Base manifold dimension dim(X). Must be set explicitly.</summary>
    [JsonPropertyName("baseDimension")]
    public required int BaseDimension { get; init; }

    /// <summary>Ambient/total space dimension dim(Y).</summary>
    [JsonPropertyName("ambientDimension")]
    public required int AmbientDimension { get; init; }

    /// <summary>Fiber dimension dim(Y) - dim(X). Computed from AmbientDimension - BaseDimension.</summary>
    [JsonIgnore]
    public int FiberDimension => AmbientDimension - BaseDimension;

    /// <summary>Lie algebra identifier for the gauge group (IX-3).</summary>
    [JsonPropertyName("lieAlgebraId")]
    public required string LieAlgebraId { get; init; }

    /// <summary>Basis convention identifier (Section 11).</summary>
    [JsonPropertyName("basisConventionId")]
    public required string BasisConventionId { get; init; }

    /// <summary>Component ordering convention identifier (Section 11.1).</summary>
    [JsonPropertyName("componentOrderId")]
    public required string ComponentOrderId { get; init; }

    /// <summary>Adjoint convention identifier (Section 11.2).</summary>
    [JsonPropertyName("adjointConventionId")]
    public required string AdjointConventionId { get; init; }

    /// <summary>Pairing convention identifier (Section 11.3).</summary>
    [JsonPropertyName("pairingConventionId")]
    public required string PairingConventionId { get; init; }

    /// <summary>Norm convention identifier.</summary>
    [JsonPropertyName("normConventionId")]
    public required string NormConventionId { get; init; }

    /// <summary>Differential form metric identifier, separate from Lie algebra pairing (FIX-1).</summary>
    [JsonPropertyName("differentialFormMetricId")]
    public required string DifferentialFormMetricId { get; init; }

    /// <summary>Inserted assumptions active in this branch (IA-1 through IA-6).</summary>
    [JsonPropertyName("insertedAssumptionIds")]
    public required IReadOnlyList<string> InsertedAssumptionIds { get; init; }

    /// <summary>Inserted choices active in this branch (IX-1 through IX-5).</summary>
    [JsonPropertyName("insertedChoiceIds")]
    public required IReadOnlyList<string> InsertedChoiceIds { get; init; }

    /// <summary>Arbitrary branch parameters.</summary>
    [JsonPropertyName("parameters")]
    public IReadOnlyDictionary<string, string>? Parameters { get; init; }
}
