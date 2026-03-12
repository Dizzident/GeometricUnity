using System.Text.Json.Serialization;

namespace Gu.Phase4.Spin;

/// <summary>
/// Specifies the gamma matrix basis convention for a given Clifford algebra.
/// Convention "dirac-tensor-product-v1" uses the recursive Pauli tensor-product
/// construction: Cl(2) base, then Cl(n) -> Cl(n+2) by tensoring with sigma_3.
/// </summary>
public sealed class GammaConventionSpec
{
    /// <summary>
    /// Unique identifier for this convention.
    /// Default: "dirac-tensor-product-v1".
    /// </summary>
    [JsonPropertyName("conventionId")]
    public required string ConventionId { get; init; }

    /// <summary>Clifford signature this convention applies to.</summary>
    [JsonPropertyName("signature")]
    public required CliffordSignature Signature { get; init; }

    /// <summary>
    /// Human-readable representation name.
    /// "standard" = Dirac tensor-product construction.
    /// "chiral" / "majorana" = branch variants (not implemented in initial pass).
    /// </summary>
    [JsonPropertyName("representation")]
    public required string Representation { get; init; }

    /// <summary>Spinor dimension = 2^floor(dim/2).</summary>
    [JsonPropertyName("spinorDimension")]
    public required int SpinorDimension { get; init; }

    /// <summary>True if dimY is even (chirality operator exists).</summary>
    [JsonPropertyName("hasChirality")]
    public required bool HasChirality { get; init; }

    /// <summary>Tolerance for anticommutation identity checks.</summary>
    [JsonPropertyName("anticommutationTolerance")]
    public double AnticommutationTolerance { get; init; } = 1e-12;
}
