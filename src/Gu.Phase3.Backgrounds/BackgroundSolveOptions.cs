using System.Text.Json.Serialization;
using Gu.Solvers;

namespace Gu.Phase3.Backgrounds;

/// <summary>
/// Options controlling how a background state is solved for.
/// Wraps the inner solver options plus admissibility tolerances.
/// </summary>
public sealed class BackgroundSolveOptions
{
    /// <summary>
    /// Solver mode for the background solve. Typically ObjectiveMinimization or StationaritySolve.
    /// </summary>
    [JsonPropertyName("solveMode")]
    public required SolveMode SolveMode { get; init; }

    /// <summary>Solver method (gradient descent, CG, Gauss-Newton).</summary>
    [JsonPropertyName("solverMethod")]
    public SolverMethod SolverMethod { get; init; } = SolverMethod.GradientDescent;

    /// <summary>Maximum solver iterations.</summary>
    [JsonPropertyName("maxIterations")]
    public int MaxIterations { get; init; } = 200;

    /// <summary>Initial step size for iterative solvers.</summary>
    [JsonPropertyName("initialStepSize")]
    public double InitialStepSize { get; init; } = 0.01;

    /// <summary>Gauge penalty lambda.</summary>
    [JsonPropertyName("gaugePenaltyLambda")]
    public double GaugePenaltyLambda { get; init; } = 0.0;

    /// <summary>Gauge strategy.</summary>
    [JsonPropertyName("gaugeStrategy")]
    public GaugeStrategy GaugeStrategy { get; init; } = GaugeStrategy.L2Penalty;

    /// <summary>
    /// Residual norm threshold for B0 (diagnostic) admissibility.
    /// </summary>
    [JsonPropertyName("toleranceResidualDiagnostic")]
    public double ToleranceResidualDiagnostic { get; init; } = 1e-4;

    /// <summary>
    /// Stationarity norm threshold for B1 admissibility.
    /// </summary>
    [JsonPropertyName("toleranceStationary")]
    public double ToleranceStationary { get; init; } = 1e-6;

    /// <summary>
    /// Strict residual norm threshold for B2 admissibility.
    /// </summary>
    [JsonPropertyName("toleranceResidualStrict")]
    public double ToleranceResidualStrict { get; init; } = 1e-8;

    /// <summary>
    /// Build a SolverOptions from these background solve options.
    /// </summary>
    public SolverOptions ToSolverOptions()
    {
        return new SolverOptions
        {
            Mode = SolveMode,
            Method = SolverMethod,
            MaxIterations = MaxIterations,
            InitialStepSize = InitialStepSize,
            GaugePenaltyLambda = GaugePenaltyLambda,
            GaugeStrategy = GaugeStrategy,
            ObjectiveTolerance = ToleranceResidualStrict,
            GradientTolerance = ToleranceStationary,
        };
    }
}
