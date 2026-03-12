using System.Text.Json.Serialization;

namespace Gu.Phase4.Chirality;

/// <summary>
/// Records chirality stability of a fermionic mode across branch variants or
/// mesh refinement levels.
///
/// A mode is chirality-stable if its chiralityTag remains the same
/// across all variants in the stability set.
/// </summary>
public sealed class FermionStabilityRecord
{
    /// <summary>Mode ID being tracked.</summary>
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    /// <summary>Branch variant IDs or refinement labels included in this stability set.</summary>
    [JsonPropertyName("variantIds")]
    public required List<string> VariantIds { get; init; }

    /// <summary>ChiralityTag values across variants (one per variant, same order as VariantIds).</summary>
    [JsonPropertyName("chiralityTagHistory")]
    public required List<string> ChiralityTagHistory { get; init; }

    /// <summary>True if chiralityTag is constant across all variants.</summary>
    [JsonPropertyName("isChiralityStable")]
    public required bool IsChiralityStable { get; init; }

    /// <summary>LeftFraction values across variants.</summary>
    [JsonPropertyName("leftFractionHistory")]
    public required List<double> LeftFractionHistory { get; init; }

    /// <summary>Standard deviation of leftFraction across variants (0 = perfectly stable).</summary>
    [JsonPropertyName("leftFractionStdDev")]
    public required double LeftFractionStdDev { get; init; }

    /// <summary>Notes on instability or branch-dependent chirality.</summary>
    [JsonPropertyName("notes")]
    public List<string>? Notes { get; init; }
}
