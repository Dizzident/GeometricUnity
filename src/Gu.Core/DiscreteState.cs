using System.Text.Json.Serialization;

namespace Gu.Core;

/// <summary>
/// The independent discrete state: branch + geometry + the independent field omega_h (Section 10.3).
/// </summary>
public sealed class DiscreteState
{
    /// <summary>Branch reference for this state.</summary>
    [JsonPropertyName("branch")]
    public required BranchRef Branch { get; init; }

    /// <summary>Geometry context for this state.</summary>
    [JsonPropertyName("geometry")]
    public required GeometryContext Geometry { get; init; }

    /// <summary>Independent connection field omega_h.</summary>
    [JsonPropertyName("omega")]
    public required FieldTensor Omega { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
