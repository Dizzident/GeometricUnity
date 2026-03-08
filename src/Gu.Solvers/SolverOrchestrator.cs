using Gu.Branching;
using Gu.Core;

namespace Gu.Solvers;

/// <summary>
/// Orchestrates the solve loop. Backend-agnostic.
/// Handles convergence checks, mode selection (A/B/C), gauge penalty,
/// convergence diagnostics, and history tracking.
///
/// Mode A: Residual-only (single evaluation)
/// Mode B: Gradient descent with backtracking line search + gauge penalty
/// Mode C: Stationarity solve (gradient descent toward J^T M Upsilon + gauge = 0)
/// </summary>
public sealed class SolverOrchestrator
{
    private readonly ISolverBackend _backend;
    private readonly SolverOptions _options;

    public SolverOrchestrator(ISolverBackend backend, SolverOptions options)
    {
        _backend = backend ?? throw new ArgumentNullException(nameof(backend));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Execute the solve.
    /// </summary>
    public SolverResult Solve(
        FieldTensor initialOmega,
        FieldTensor a0,
        BranchManifest manifest,
        GeometryContext geometry)
    {
        return _options.Mode switch
        {
            SolveMode.ResidualOnly => SolveModeA(initialOmega, a0, manifest, geometry),
            SolveMode.ObjectiveMinimization => SolveModeB(initialOmega, a0, manifest, geometry),
            SolveMode.StationaritySolve => SolveModeC(initialOmega, a0, manifest, geometry),
            _ => throw new ArgumentOutOfRangeException(nameof(_options.Mode)),
        };
    }

    /// <summary>
    /// Mode A: Evaluate residual only (no optimization).
    /// Used to validate assembly and inspect the initial state.
    /// </summary>
    private SolverResult SolveModeA(
        FieldTensor omega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
    {
        var derived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
        double objective = _backend.EvaluateObjective(derived.ResidualUpsilon);
        double residualNorm = _backend.ComputeNorm(derived.ResidualUpsilon);

        var record = new ConvergenceRecord
        {
            Iteration = 0,
            Objective = objective,
            ResidualNorm = residualNorm,
            GradientNorm = 0,
            GaugeViolation = 0,
            StepNorm = 0,
            StepSize = 0,
        };

        return new SolverResult
        {
            Converged = false,
            TerminationReason = "Mode A: residual-only evaluation",
            Iterations = 0,
            FinalObjective = objective,
            FinalResidualNorm = residualNorm,
            FinalGradientNorm = 0,
            FinalGaugeViolation = 0,
            FinalOmega = omega,
            FinalDerivedState = derived,
            History = new[] { record },
            Mode = SolveMode.ResidualOnly,
        };
    }

    /// <summary>
    /// Mode B: Objective minimization via gradient descent with backtracking line search.
    /// Integrates gauge penalty: I2_total = I2_physics + (lambda/2)||omega||^2.
    /// omega_{k+1} = omega_k - alpha_k * (J^T M Upsilon + lambda * omega_k)
    /// </summary>
    private SolverResult SolveModeB(
        FieldTensor initialOmega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
    {
        var history = new List<ConvergenceRecord>();
        var diagnostics = new ConvergenceDiagnostics();
        var gaugePenalty = new GaugePenaltyTerm(_options.GaugePenaltyLambda);
        var omega = CloneField(initialOmega);

        for (int iter = 0; iter < _options.MaxIterations; iter++)
        {
            var derived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
            double physicsObjective = _backend.EvaluateObjective(derived.ResidualUpsilon);
            double totalObjective = gaugePenalty.AddToObjective(physicsObjective, omega);
            double residualNorm = _backend.ComputeNorm(derived.ResidualUpsilon);
            double gaugeViolation = gaugePenalty.ComputeViolationNorm(omega);

            var jacobian = _backend.BuildJacobian(omega, a0, derived.CurvatureF, manifest, geometry);
            var physicsGradient = _backend.ComputeGradient(jacobian, derived.ResidualUpsilon);
            var totalGradient = gaugePenalty.AddToGradient(physicsGradient, omega);
            double gradNorm = ComputeL2Norm(totalGradient);

            double stepSize = 0;
            double stepNorm = 0;

            var record = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = stepNorm,
                StepSize = stepSize,
            };
            history.Add(record);
            diagnostics.Record(record);

            // Check convergence: objective must be non-negative and below tolerance
            // (negative objectives indicate the metric is not positive-definite or
            // the problem is not a minimization in the usual sense)
            if (totalObjective >= 0 && totalObjective < _options.ObjectiveTolerance)
            {
                return BuildResult(true, "Objective below tolerance", iter, omega, derived, history);
            }
            if (gradNorm < _options.GradientTolerance)
            {
                return BuildResult(true, "Gradient norm below tolerance", iter, omega, derived, history);
            }
            if (diagnostics.IsStagnated())
            {
                return BuildResult(false, "Stagnation detected", iter, omega, derived, history);
            }

            // Backtracking line search on total objective
            stepSize = _options.InitialStepSize;
            FieldTensor? nextOmega = null;

            for (int bt = 0; bt < _options.MaxBacktrackSteps; bt++)
            {
                nextOmega = AddScaled(omega, totalGradient, -stepSize);
                var nextDerived = _backend.EvaluateDerived(nextOmega, a0, manifest, geometry);
                double nextPhysicsObj = _backend.EvaluateObjective(nextDerived.ResidualUpsilon);
                double nextTotalObj = gaugePenalty.AddToObjective(nextPhysicsObj, nextOmega);

                // Armijo condition: f(x+) <= f(x) - c * alpha * ||grad||^2
                double sufficientDecrease = _options.ArmijoParameter * stepSize * gradNorm * gradNorm;
                if (nextTotalObj <= totalObjective - sufficientDecrease)
                    break;

                stepSize *= _options.BacktrackFactor;
                nextOmega = null;
            }

            if (nextOmega == null)
            {
                return BuildResult(false, "Line search failed", iter, omega, derived, history);
            }

            stepNorm = stepSize * gradNorm;
            // Update the last record's step info
            history[^1] = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = stepNorm,
                StepSize = stepSize,
            };

            omega = nextOmega;
        }

        var finalDerived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
        return BuildResult(false, "Maximum iterations reached", _options.MaxIterations, omega, finalDerived, history);
    }

    /// <summary>
    /// Mode C: Stationarity solve. Gradient descent toward J^T M Upsilon + gauge = 0.
    /// Convergence is based solely on gradient norm (stationarity condition),
    /// not on objective tolerance. The solve terminates only when the gradient
    /// norm drops below GradientTolerance, ensuring a true stationary point.
    /// </summary>
    private SolverResult SolveModeC(
        FieldTensor initialOmega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
    {
        var history = new List<ConvergenceRecord>();
        var diagnostics = new ConvergenceDiagnostics();
        var gaugePenalty = new GaugePenaltyTerm(_options.GaugePenaltyLambda);
        var omega = CloneField(initialOmega);

        for (int iter = 0; iter < _options.MaxIterations; iter++)
        {
            var derived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
            double physicsObjective = _backend.EvaluateObjective(derived.ResidualUpsilon);
            double totalObjective = gaugePenalty.AddToObjective(physicsObjective, omega);
            double residualNorm = _backend.ComputeNorm(derived.ResidualUpsilon);
            double gaugeViolation = gaugePenalty.ComputeViolationNorm(omega);

            var jacobian = _backend.BuildJacobian(omega, a0, derived.CurvatureF, manifest, geometry);
            var physicsGradient = _backend.ComputeGradient(jacobian, derived.ResidualUpsilon);
            var totalGradient = gaugePenalty.AddToGradient(physicsGradient, omega);
            double gradNorm = ComputeL2Norm(totalGradient);

            double stepSize = 0;
            double stepNorm = 0;

            var record = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = stepNorm,
                StepSize = stepSize,
            };
            history.Add(record);
            diagnostics.Record(record);

            // Mode C convergence: only gradient norm (stationarity condition)
            if (gradNorm < _options.GradientTolerance)
            {
                return BuildResult(true, "Gradient norm below tolerance (stationarity)", iter, omega, derived, history);
            }
            if (diagnostics.IsStagnated())
            {
                return BuildResult(false, "Stagnation detected", iter, omega, derived, history);
            }

            // Backtracking line search on total objective
            stepSize = _options.InitialStepSize;
            FieldTensor? nextOmega = null;

            for (int bt = 0; bt < _options.MaxBacktrackSteps; bt++)
            {
                nextOmega = AddScaled(omega, totalGradient, -stepSize);
                var nextDerived = _backend.EvaluateDerived(nextOmega, a0, manifest, geometry);
                double nextPhysicsObj = _backend.EvaluateObjective(nextDerived.ResidualUpsilon);
                double nextTotalObj = gaugePenalty.AddToObjective(nextPhysicsObj, nextOmega);

                // Armijo condition: f(x+) <= f(x) - c * alpha * ||grad||^2
                double sufficientDecrease = _options.ArmijoParameter * stepSize * gradNorm * gradNorm;
                if (nextTotalObj <= totalObjective - sufficientDecrease)
                    break;

                stepSize *= _options.BacktrackFactor;
                nextOmega = null;
            }

            if (nextOmega == null)
            {
                return BuildResult(false, "Line search failed", iter, omega, derived, history);
            }

            stepNorm = stepSize * gradNorm;
            // Update the last record's step info
            history[^1] = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = stepNorm,
                StepSize = stepSize,
            };

            omega = nextOmega;
        }

        var finalDerived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
        return BuildResult(false, "Maximum iterations reached", _options.MaxIterations, omega, finalDerived, history);
    }

    private SolverResult BuildResult(
        bool converged, string reason, int iterations,
        FieldTensor omega, DerivedState derived, List<ConvergenceRecord> history)
    {
        var lastRecord = history.Count > 0 ? history[^1] : null;
        return new SolverResult
        {
            Converged = converged,
            TerminationReason = reason,
            Iterations = iterations,
            FinalObjective = lastRecord?.Objective ?? 0,
            FinalResidualNorm = lastRecord?.ResidualNorm ?? 0,
            FinalGradientNorm = lastRecord?.GradientNorm ?? 0,
            FinalGaugeViolation = lastRecord?.GaugeViolation ?? 0,
            FinalOmega = omega,
            FinalDerivedState = derived,
            History = history,
            Mode = _options.Mode,
        };
    }

    private static FieldTensor CloneField(FieldTensor f)
    {
        return new FieldTensor
        {
            Label = f.Label,
            Signature = f.Signature,
            Coefficients = (double[])f.Coefficients.Clone(),
            Shape = f.Shape,
        };
    }

    private static FieldTensor AddScaled(FieldTensor a, FieldTensor b, double alpha)
    {
        var result = new double[a.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = a.Coefficients[i] + alpha * b.Coefficients[i];
        return new FieldTensor
        {
            Label = a.Label,
            Signature = a.Signature,
            Coefficients = result,
            Shape = a.Shape,
        };
    }

    private static double ComputeL2Norm(FieldTensor f)
    {
        double sum = 0;
        for (int i = 0; i < f.Coefficients.Length; i++)
            sum += f.Coefficients[i] * f.Coefficients[i];
        return System.Math.Sqrt(sum);
    }
}
