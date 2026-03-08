using Gu.Core;

namespace Gu.Solvers;

/// <summary>
/// Solver execution mode.
/// </summary>
public enum SolveMode
{
    /// <summary>Mode A: Residual-only evaluation (no optimization).</summary>
    ResidualOnly,

    /// <summary>Mode B: Objective minimization (gradient descent).</summary>
    ObjectiveMinimization,

    /// <summary>Mode C: Stationarity solve (J^T M Upsilon + gauge = 0).</summary>
    StationaritySolve,
}

/// <summary>
/// Options for the solver.
/// </summary>
public sealed class SolverOptions
{
    /// <summary>Solve mode.</summary>
    public required SolveMode Mode { get; init; }

    /// <summary>Maximum number of iterations (modes B, C).</summary>
    public int MaxIterations { get; init; } = 100;

    /// <summary>Objective tolerance for convergence.</summary>
    public double ObjectiveTolerance { get; init; } = 1e-10;

    /// <summary>Gradient norm tolerance for convergence.</summary>
    public double GradientTolerance { get; init; } = 1e-8;

    /// <summary>Step size for gradient descent (mode B).</summary>
    public double InitialStepSize { get; init; } = 0.01;

    /// <summary>Gauge penalty coefficient lambda (IA-4).</summary>
    public double GaugePenaltyLambda { get; init; } = 0.0;

    /// <summary>Backtracking line search: Armijo parameter.</summary>
    public double ArmijoParameter { get; init; } = 1e-4;

    /// <summary>Backtracking line search: step reduction factor.</summary>
    public double BacktrackFactor { get; init; } = 0.5;

    /// <summary>Maximum line search reductions.</summary>
    public int MaxBacktrackSteps { get; init; } = 20;
}

/// <summary>
/// Convergence record for a single iteration.
/// </summary>
public sealed class ConvergenceRecord
{
    /// <summary>Iteration number.</summary>
    public required int Iteration { get; init; }

    /// <summary>Objective I2_h.</summary>
    public required double Objective { get; init; }

    /// <summary>Residual norm ||Upsilon_h||.</summary>
    public required double ResidualNorm { get; init; }

    /// <summary>Gradient norm ||J^T M Upsilon||.</summary>
    public required double GradientNorm { get; init; }

    /// <summary>Gauge violation norm.</summary>
    public required double GaugeViolation { get; init; }

    /// <summary>Step norm ||delta_omega||.</summary>
    public required double StepNorm { get; init; }

    /// <summary>Step size used (after line search).</summary>
    public required double StepSize { get; init; }
}

/// <summary>
/// Result of a solve operation.
/// </summary>
public sealed class SolverResult
{
    /// <summary>Whether the solver converged.</summary>
    public required bool Converged { get; init; }

    /// <summary>Reason for termination.</summary>
    public required string TerminationReason { get; init; }

    /// <summary>Total iterations performed.</summary>
    public required int Iterations { get; init; }

    /// <summary>Final objective value.</summary>
    public required double FinalObjective { get; init; }

    /// <summary>Final residual norm.</summary>
    public required double FinalResidualNorm { get; init; }

    /// <summary>Final gradient norm.</summary>
    public required double FinalGradientNorm { get; init; }

    /// <summary>Final gauge violation.</summary>
    public required double FinalGaugeViolation { get; init; }

    /// <summary>Final omega field.</summary>
    public required FieldTensor FinalOmega { get; init; }

    /// <summary>Final derived state.</summary>
    public required DerivedState FinalDerivedState { get; init; }

    /// <summary>Convergence history.</summary>
    public required IReadOnlyList<ConvergenceRecord> History { get; init; }

    /// <summary>Solve mode used.</summary>
    public required SolveMode Mode { get; init; }
}
