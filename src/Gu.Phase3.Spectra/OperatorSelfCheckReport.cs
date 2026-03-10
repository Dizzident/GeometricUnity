using System.Text.Json.Serialization;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Self-check report for a linearized operator bundle.
/// Verifies symmetry of H and M_state, and positive semi-definiteness of M_state.
/// </summary>
public sealed class OperatorSelfCheckReport
{
    /// <summary>Bundle ID that was checked.</summary>
    [JsonPropertyName("bundleId")]
    public required string BundleId { get; init; }

    /// <summary>Maximum symmetry error: max |u^T H v - v^T H u| over random probes.</summary>
    [JsonPropertyName("spectralSymmetryError")]
    public required double SpectralSymmetryError { get; init; }

    /// <summary>Maximum symmetry error for M_state.</summary>
    [JsonPropertyName("massSymmetryError")]
    public required double MassSymmetryError { get; init; }

    /// <summary>Minimum v^T M_state v observed over random probes (should be >= 0).</summary>
    [JsonPropertyName("massMinQuadratic")]
    public required double MassMinQuadratic { get; init; }

    /// <summary>Whether all checks passed within tolerance.</summary>
    [JsonPropertyName("passed")]
    public required bool Passed { get; init; }

    /// <summary>Tolerance used for the checks.</summary>
    [JsonPropertyName("tolerance")]
    public required double Tolerance { get; init; }

    /// <summary>Number of random probe vectors used.</summary>
    [JsonPropertyName("probeCount")]
    public required int ProbeCount { get; init; }
}
