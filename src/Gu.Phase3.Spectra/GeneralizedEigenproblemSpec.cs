using System.Text.Json.Serialization;

namespace Gu.Phase3.Spectra;

/// <summary>
/// Specification for a generalized eigenproblem: H v = lambda M v.
/// Controls the eigensolver method, convergence criteria, and output options.
/// </summary>
public sealed class GeneralizedEigenproblemSpec
{
    /// <summary>Number of eigenvalues to compute (smallest algebraic).</summary>
    [JsonPropertyName("numEigenvalues")]
    public required int NumEigenvalues { get; init; }

    /// <summary>
    /// Solver method: "lanczos", "lobpcg", "inverse-iteration", "explicit-dense".
    /// Default: "lanczos" for large problems, "explicit-dense" for dim &lt;= 200.
    /// </summary>
    [JsonPropertyName("solverMethod")]
    public string SolverMethod { get; init; } = "auto";

    /// <summary>Convergence tolerance for residual norms.</summary>
    [JsonPropertyName("convergenceTolerance")]
    public double ConvergenceTolerance { get; init; } = 1e-8;

    /// <summary>Maximum number of iterations.</summary>
    [JsonPropertyName("maxIterations")]
    public int MaxIterations { get; init; } = 1000;

    /// <summary>
    /// Null mode threshold: eigenvalues with |lambda| below this are flagged.
    /// </summary>
    [JsonPropertyName("nullModeThreshold")]
    public double NullModeThreshold { get; init; } = 1e-8;

    /// <summary>
    /// Gauge leak threshold for null mode classification.
    /// Null modes with leak > threshold are classified as gauge artifacts.
    /// </summary>
    [JsonPropertyName("gaugeLeakThreshold")]
    public double GaugeLeakThreshold { get; init; } = 0.5;

    /// <summary>
    /// Relative tolerance for spectral clustering: modes with
    /// |lambda_i - lambda_j| / max(|lambda_i|, eps) &lt; clusterTol are clustered.
    /// </summary>
    [JsonPropertyName("clusterRelativeTolerance")]
    public double ClusterRelativeTolerance { get; init; } = 1e-4;

    /// <summary>
    /// Absolute tolerance for spectral clustering: modes with
    /// |lambda_i - lambda_j| &lt; clusterAbsTol are clustered regardless of relative gap.
    /// </summary>
    [JsonPropertyName("clusterAbsoluteTolerance")]
    public double ClusterAbsoluteTolerance { get; init; } = 1e-10;
}
