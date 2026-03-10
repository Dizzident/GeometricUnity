using System.Text.Json.Serialization;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Diagnosis of null modes (zero or near-zero eigenvalues) in the spectrum.
/// Null modes may be:
/// - gauge artifacts (high gauge leak score),
/// - physical zero modes (Goldstone bosons, flat directions),
/// - numerical artifacts (poorly converged eigenpairs).
/// </summary>
public sealed class NullModeDiagnosis
{
    /// <summary>Eigenvalue threshold used to classify null modes.</summary>
    [JsonPropertyName("nullThreshold")]
    public required double NullThreshold { get; init; }

    /// <summary>Number of null modes found (|lambda| &lt; threshold).</summary>
    [JsonPropertyName("nullModeCount")]
    public required int NullModeCount { get; init; }

    /// <summary>Number of null modes identified as gauge artifacts (high gauge leak).</summary>
    [JsonPropertyName("gaugeNullCount")]
    public required int GaugeNullCount { get; init; }

    /// <summary>Number of null modes classified as physical zero modes.</summary>
    [JsonPropertyName("physicalNullCount")]
    public int PhysicalNullCount => NullModeCount - GaugeNullCount;

    /// <summary>Eigenvalues of null modes, sorted by |lambda|.</summary>
    [JsonPropertyName("nullEigenvalues")]
    public required double[] NullEigenvalues { get; init; }

    /// <summary>Gauge leak scores for each null mode.</summary>
    [JsonPropertyName("nullGaugeLeakScores")]
    public required double[] NullGaugeLeakScores { get; init; }

    /// <summary>
    /// Gauge leak threshold used to separate gauge from physical null modes.
    /// Modes with leak score > threshold are classified as gauge artifacts.
    /// </summary>
    [JsonPropertyName("gaugeLeakThreshold")]
    public required double GaugeLeakThreshold { get; init; }
}
