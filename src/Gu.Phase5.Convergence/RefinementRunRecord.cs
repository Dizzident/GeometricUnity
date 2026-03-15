using System.Text.Json.Serialization;
using Gu.Core;

namespace Gu.Phase5.Convergence;

/// <summary>
/// Results from running the physics pipeline at one refinement level (M47).
/// </summary>
public sealed class RefinementRunRecord
{
    /// <summary>Level identifier from the study spec.</summary>
    [JsonPropertyName("levelId")]
    public required string LevelId { get; init; }

    /// <summary>Effective mesh parameter max(h_X, h_F) used for Richardson extrapolation.</summary>
    [JsonPropertyName("meshParameter")]
    public required double MeshParameter { get; init; }

    /// <summary>Extracted target quantities at this refinement level.</summary>
    [JsonPropertyName("quantities")]
    public required IReadOnlyDictionary<string, double> Quantities { get; init; }

    /// <summary>Whether the solver converged at this refinement level.</summary>
    [JsonPropertyName("converged")]
    public required bool Converged { get; init; }

    /// <summary>Residual norm from the solver at this level.</summary>
    [JsonPropertyName("residualNorm")]
    public double ResidualNorm { get; init; }

    /// <summary>Provenance metadata.</summary>
    [JsonPropertyName("provenance")]
    public required ProvenanceMeta Provenance { get; init; }
}
