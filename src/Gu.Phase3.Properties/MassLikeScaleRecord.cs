using System.Text.Json.Serialization;

namespace Gu.Phase3.Properties;

/// <summary>
/// Mass-like scale extracted from a mode's eigenvalue.
/// This is a spectral invariant or mass-like proxy, not necessarily a physical mass.
/// (See IMPLEMENTATION_PLAN_P3.md Section 4.10.2)
///
/// Provenance is required per Section 14.5.
/// </summary>
public sealed class MassLikeScaleRecord
{
    /// <summary>Mode ID this scale was extracted from.</summary>
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    /// <summary>Raw eigenvalue lambda from the spectral solve.</summary>
    [JsonPropertyName("eigenvalue")]
    public required double Eigenvalue { get; init; }

    /// <summary>
    /// Mass-like scale: sqrt(|lambda|) when lambda >= 0, or -sqrt(|lambda|) for negative eigenvalues.
    /// </summary>
    [JsonPropertyName("massLikeScale")]
    public required double MassLikeScale { get; init; }

    /// <summary>Method used to extract the scale: "eigenvalue", "dispersion-fit", "observed-space-fit".</summary>
    [JsonPropertyName("extractionMethod")]
    public required string ExtractionMethod { get; init; }

    /// <summary>Operator type that produced the eigenvalue.</summary>
    [JsonPropertyName("operatorType")]
    public required string OperatorType { get; init; }

    /// <summary>Background ID for provenance.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Branch manifest ID for provenance.</summary>
    [JsonPropertyName("branchManifestId")]
    public string? BranchManifestId { get; init; }
}
