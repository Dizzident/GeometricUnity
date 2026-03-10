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

    /// <summary>
    /// Energy fraction per tensor signature label (e.g., "connection-1form" -> 1.0).
    /// Values are non-negative and sum to 1.0.
    /// </summary>
    [JsonPropertyName("tensorEnergyFractions")]
    public Dictionary<string, double>? TensorEnergyFractions { get; init; }

    /// <summary>
    /// Energy fraction per state-space block (e.g., per Lie algebra component).
    /// Values are non-negative and sum to 1.0.
    /// </summary>
    [JsonPropertyName("blockEnergyFractions")]
    public Dictionary<string, double>? BlockEnergyFractions { get; init; }

    /// <summary>
    /// Artifact path reference for the stored mode vector on disk.
    /// Set when mode vectors are serialized to an artifact bundle.
    /// </summary>
    [JsonPropertyName("modeVectorArtifactRef")]
    public string? ModeVectorArtifactRef { get; init; }

    /// <summary>
    /// Reference to the computed observed mode signature (set by M29 Observables pipeline).
    /// </summary>
    [JsonPropertyName("observedSignatureRef")]
    public string? ObservedSignatureRef { get; init; }
}
