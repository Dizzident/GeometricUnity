using System.Text.Json.Serialization;

namespace Gu.Phase3.Properties;

/// <summary>
/// Polarization / tensor class descriptor for a mode.
/// Measures how the mode energy distributes among state blocks and tensor signatures.
/// (See IMPLEMENTATION_PLAN_P3.md Section 4.10.3)
///
/// Classification is operational, not metaphysical.
/// </summary>
public sealed class PolarizationDescriptor
{
    /// <summary>Mode ID.</summary>
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    /// <summary>
    /// Energy fractions by block label.
    /// Keys are block names (e.g., "connection", "auxiliary").
    /// Values are fractions in [0,1] summing to 1.
    /// </summary>
    [JsonPropertyName("blockEnergyFractions")]
    public required IReadOnlyDictionary<string, double> BlockEnergyFractions { get; init; }

    /// <summary>
    /// Dominant polarization class:
    /// "scalar-like", "vector-like", "2-form-like", "mixed",
    /// "connection-dominant", "auxiliary-block-dominant".
    /// </summary>
    [JsonPropertyName("dominantClass")]
    public required string DominantClass { get; init; }

    /// <summary>Fraction of total energy in the dominant block.</summary>
    [JsonPropertyName("dominanceFraction")]
    public required double DominanceFraction { get; init; }

    /// <summary>Background ID for provenance.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }
}
