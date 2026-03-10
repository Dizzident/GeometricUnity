using System.Text.Json.Serialization;

namespace Gu.Phase3.Properties;

/// <summary>
/// Symmetry content descriptor for a mode.
/// Records the mode's transformation behavior under available discrete/continuous symmetries.
/// (See IMPLEMENTATION_PLAN_P3.md Section 4.10.4)
/// </summary>
public sealed class SymmetryDescriptor
{
    /// <summary>Mode ID.</summary>
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    /// <summary>
    /// Parity eigenvalue under parity-like involution.
    /// +1 = even, -1 = odd, null = not applicable.
    /// </summary>
    [JsonPropertyName("parityEigenvalue")]
    public double? ParityEigenvalue { get; init; }

    /// <summary>
    /// Symmetry labels assigned by discrete symmetry analysis.
    /// E.g., rotation quantum numbers, algebra decomposition labels.
    /// </summary>
    [JsonPropertyName("symmetryLabels")]
    public IReadOnlyList<string> SymmetryLabels { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Overlap with each symmetry sector (keyed by sector label).
    /// Values in [0,1], sum to 1 across all sectors.
    /// </summary>
    [JsonPropertyName("sectorOverlaps")]
    public IReadOnlyDictionary<string, double>? SectorOverlaps { get; init; }

    /// <summary>Background ID for provenance.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }
}
