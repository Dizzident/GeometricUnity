using System.Text.Json.Serialization;
using Gu.Phase3.Registry;

namespace Gu.Phase3.Reporting;

/// <summary>
/// Per-background spectral summary for the boson atlas report.
/// Contains eigenvalue, multiplicity, and gauge leak information
/// for all modes found on a given background.
/// </summary>
public sealed class SpectrumSheet
{
    /// <summary>Background ID this sheet summarizes.</summary>
    [JsonPropertyName("backgroundId")]
    public required string BackgroundId { get; init; }

    /// <summary>Eigenvalues of modes on this background.</summary>
    [JsonPropertyName("eigenvalues")]
    public required IReadOnlyList<double> Eigenvalues { get; init; }

    /// <summary>Multiplicities of modes on this background.</summary>
    [JsonPropertyName("multiplicities")]
    public required IReadOnlyList<int> Multiplicities { get; init; }

    /// <summary>Gauge leak scores of modes on this background.</summary>
    [JsonPropertyName("gaugeLeaks")]
    public required IReadOnlyList<double> GaugeLeaks { get; init; }

    /// <summary>Number of modes.</summary>
    [JsonPropertyName("modeCount")]
    public int ModeCount { get; init; }

    /// <summary>Convergence status: "converged", "not-converged", etc.</summary>
    [JsonPropertyName("convergenceStatus")]
    public required string ConvergenceStatus { get; init; }
}
