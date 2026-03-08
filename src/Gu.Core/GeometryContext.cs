using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// Contains both the semantic and discrete geometry information (Section 10.2).
/// </summary>
public sealed class GeometryContext
{
    /// <summary>Base space X_h.</summary>
    [JsonPropertyName("baseSpace")]
    public required SpaceRef BaseSpace { get; init; }

    /// <summary>Ambient space Y_h.</summary>
    [JsonPropertyName("ambientSpace")]
    public required SpaceRef AmbientSpace { get; init; }

    /// <summary>Discretization type (e.g., "simplicial", "structured").</summary>
    [JsonPropertyName("discretizationType")]
    public required string DiscretizationType { get; init; }

    /// <summary>Quadrature rule identifier.</summary>
    [JsonPropertyName("quadratureRuleId")]
    public required string QuadratureRuleId { get; init; }

    /// <summary>Basis function family identifier.</summary>
    [JsonPropertyName("basisFamilyId")]
    public required string BasisFamilyId { get; init; }

    /// <summary>Projection binding pi_h: Y_h -> X_h.</summary>
    [JsonPropertyName("projectionBinding")]
    public required GeometryBinding ProjectionBinding { get; init; }

    /// <summary>Observation binding sigma_h: X_h -> Y_h.</summary>
    [JsonPropertyName("observationBinding")]
    public required GeometryBinding ObservationBinding { get; init; }

    /// <summary>Patch decomposition of the geometry.</summary>
    [JsonPropertyName("patches")]
    public required IReadOnlyList<PatchInfo> Patches { get; init; }

    /// <summary>Optional opaque geometry payload (e.g., mesh data).</summary>
    [JsonPropertyName("geometryPayload")]
    public byte[]? GeometryPayload { get; init; }
}
