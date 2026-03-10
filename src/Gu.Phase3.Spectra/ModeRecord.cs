using System.Text.Json.Serialization;

namespace Gu.Phase3.Spectra;

/// <summary>
/// A single computed eigenmode from a spectral solve.
///
/// PHYSICS CONSTRAINT #1: Every mode declares which operator type produced it.
/// PHYSICS CONSTRAINT #2: Gauge leak score is mandatory for every mode.
/// </summary>
public sealed class ModeRecord
{
    /// <summary>Unique mode identifier.</summary>
    [JsonPropertyName("modeId")]
    public required string ModeId { get; init; }

    /// <summary>Background state ID.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Which operator type produced this mode (GN vs Full Hessian).</summary>
    [JsonPropertyName("operatorType")]
    public required SpectralOperatorType OperatorType { get; init; }

    /// <summary>Eigenvalue.</summary>
    [JsonPropertyName("eigenvalue")]
    public required double Eigenvalue { get; init; }

    /// <summary>Residual norm ||H v - lambda M v|| / ||v||_M.</summary>
    [JsonPropertyName("residualNorm")]
    public required double ResidualNorm { get; init; }

    /// <summary>Normalization convention: "unit-M-norm" or "unit-L2-norm".</summary>
    [JsonPropertyName("normalizationConvention")]
    public required string NormalizationConvention { get; init; }

    /// <summary>Multiplicity cluster ID (assigned by spectral clustering).</summary>
    [JsonPropertyName("multiplicityClusterId")]
    public string? MultiplicityClusterId { get; init; }

    /// <summary>Gauge leak score: fraction of mode energy in gauge subspace.</summary>
    [JsonPropertyName("gaugeLeakScore")]
    public required double GaugeLeakScore { get; init; }

    /// <summary>Mode vector coefficients.</summary>
    [JsonPropertyName("modeVector")]
    public required double[] ModeVector { get; init; }

    /// <summary>Null mode diagnosis (set for modes with |eigenvalue| near zero).</summary>
    [JsonPropertyName("nullModeDiagnosis")]
    public NullModeDiagnosis? NullModeDiagnosis { get; init; }

    /// <summary>Index within the spectrum (0-based, sorted by eigenvalue ascending).</summary>
    [JsonPropertyName("modeIndex")]
    public required int ModeIndex { get; init; }
}
