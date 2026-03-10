using System.Text.Json.Serialization;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Metrics computed for a background state after solving.
/// </summary>
public sealed class BackgroundMetrics
{
    /// <summary>Residual norm ||Upsilon_h(z_*)||.</summary>
    [JsonPropertyName("residualNorm")]
    public required double ResidualNorm { get; init; }

    /// <summary>Stationarity norm ||G_h(z_*)|| = ||J^T M Upsilon||.</summary>
    [JsonPropertyName("stationarityNorm")]
    public required double StationarityNorm { get; init; }

    /// <summary>Objective value I2_h(z_*).</summary>
    [JsonPropertyName("objectiveValue")]
    public required double ObjectiveValue { get; init; }

    /// <summary>Final gauge violation norm.</summary>
    [JsonPropertyName("gaugeViolation")]
    public required double GaugeViolation { get; init; }

    /// <summary>Total solver iterations used.</summary>
    [JsonPropertyName("solverIterations")]
    public required int SolverIterations { get; init; }

    /// <summary>Whether the solver converged.</summary>
    [JsonPropertyName("solverConverged")]
    public required bool SolverConverged { get; init; }

    /// <summary>Termination reason from the solver.</summary>
    [JsonPropertyName("terminationReason")]
    public required string TerminationReason { get; init; }

    /// <summary>
    /// Whether this background qualifies for Gauss-Newton approximation (B2-level only).
    /// Physicist constraint: GN is only valid when residual is small.
    /// </summary>
    [JsonPropertyName("gaussNewtonValid")]
    public required bool GaussNewtonValid { get; init; }
}
