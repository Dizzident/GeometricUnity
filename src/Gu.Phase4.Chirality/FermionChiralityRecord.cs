using System.Text.Json.Serialization;

namespace Gu.Phase4.Chirality;

/// <summary>
/// Wraps the three chirality decompositions for a single fermionic mode:
///   - XChirality: base 4D chirality (always present, dimX is even in GU)
///   - YChirality: full Y-space chirality (null when dimY is odd)
///   - FChirality: fiber chirality (null when dimF is odd or not decomposing)
///
/// Relation: Gamma_Y = Gamma_X * Gamma_F (up to sign convention).
/// </summary>
public sealed class FermionChiralityRecord
{
    /// <summary>Mode ID this record applies to.</summary>
    [JsonPropertyName("fermionModeId")]
    public required string FermionModeId { get; init; }

    /// <summary>
    /// X-space (base) chirality decomposition. Always present — dimX=4 is even in GU.
    /// </summary>
    [JsonPropertyName("xChirality")]
    public required ChiralityDecomposition XChirality { get; init; }

    /// <summary>
    /// Full Y-space chirality decomposition. Null when dimY is odd.
    /// </summary>
    [JsonPropertyName("yChirality")]
    public ChiralityDecomposition? YChirality { get; init; }

    /// <summary>
    /// Fiber F-space chirality decomposition. Null when dimF is odd or
    /// ChiralityConventionSpec.FiberDimension is not set.
    /// </summary>
    [JsonPropertyName("fChirality")]
    public ChiralityDecomposition? FChirality { get; init; }
}
