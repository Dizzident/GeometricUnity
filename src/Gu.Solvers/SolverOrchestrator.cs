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
            SolveMode.BranchSensitivity => throw new InvalidOperationException(
                "Mode D (BranchSensitivity) cannot be run through SolverOrchestrator.Solve(). " +
                "Use BranchSensitivityRunner.Sweep() instead."),
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
    /// Mode B: Objective minimization. Dispatches to the selected solver method.
    /// </summary>
    private SolverResult SolveModeB(
        FieldTensor initialOmega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
    {
        return _options.Method switch
        {
            SolverMethod.ConjugateGradient => SolveModeBConjugateGradient(initialOmega, a0, manifest, geometry),
            SolverMethod.GaussNewton => SolveModeBGaussNewton(initialOmega, a0, manifest, geometry),
            _ => SolveModeBGradientDescent(initialOmega, a0, manifest, geometry),
        };
    }

    /// <summary>
    /// Mode B: Objective minimization via gradient descent with backtracking line search.
    /// Integrates gauge penalty: I2_total = I2_physics + (lambda/2)||omega||^2.
    /// omega_{k+1} = omega_k - alpha_k * (J^T M Upsilon + lambda * omega_k)
    /// </summary>
    private SolverResult SolveModeBGradientDescent(
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
    /// Mode C: Stationarity solve. Dispatches to the selected solver method.
    /// </summary>
    private SolverResult SolveModeC(
        FieldTensor initialOmega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
    {
        return _options.Method switch
        {
            SolverMethod.ConjugateGradient => SolveModeCConjugateGradient(initialOmega, a0, manifest, geometry),
            SolverMethod.GaussNewton => SolveModeCGaussNewton(initialOmega, a0, manifest, geometry),
            _ => SolveModeCGradientDescent(initialOmega, a0, manifest, geometry),
        };
    }

    /// <summary>
    /// Mode C: Stationarity solve via gradient descent toward J^T M Upsilon + gauge = 0.
    /// Convergence is based solely on gradient norm (stationarity condition),
    /// not on objective tolerance. The solve terminates only when the gradient
    /// norm drops below GradientTolerance, ensuring a true stationary point.
    /// </summary>
    private SolverResult SolveModeCGradientDescent(
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

    /// <summary>
    /// Mode B: Nonlinear conjugate gradient (Polak-Ribiere with restart).
    /// </summary>
    private SolverResult SolveModeBConjugateGradient(
        FieldTensor initialOmega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
    {
        var history = new List<ConvergenceRecord>();
        var diagnostics = new ConvergenceDiagnostics();
        var gaugePenalty = new GaugePenaltyTerm(_options.GaugePenaltyLambda);
        var omega = CloneField(initialOmega);
        FieldTensor? prevGradient = null;
        FieldTensor? searchDirection = null;

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

            var record = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = 0,
                StepSize = 0,
            };
            history.Add(record);
            diagnostics.Record(record);

            if (totalObjective >= 0 && totalObjective < _options.ObjectiveTolerance)
                return BuildResult(true, "Objective below tolerance", iter, omega, derived, history);
            if (gradNorm < _options.GradientTolerance)
                return BuildResult(true, "Gradient norm below tolerance", iter, omega, derived, history);
            if (diagnostics.IsStagnated())
                return BuildResult(false, "Stagnation detected", iter, omega, derived, history);

            // Polak-Ribiere beta with restart
            double beta = 0;
            if (prevGradient != null && searchDirection != null)
            {
                double dotDiff = DotProduct(totalGradient, SubtractFields(totalGradient, prevGradient));
                double dotOld = DotProduct(prevGradient, prevGradient);
                if (dotOld > 0)
                    beta = System.Math.Max(0, dotDiff / dotOld);
            }

            if (searchDirection == null || beta == 0)
                searchDirection = NegateField(totalGradient);
            else
                searchDirection = AddScaled(NegateField(totalGradient), searchDirection, beta);

            double stepSize = _options.InitialStepSize;
            FieldTensor? nextOmega = null;
            double dirNorm = ComputeL2Norm(searchDirection);

            for (int bt = 0; bt < _options.MaxBacktrackSteps; bt++)
            {
                nextOmega = AddScaled(omega, searchDirection, stepSize);
                var nextDerived = _backend.EvaluateDerived(nextOmega, a0, manifest, geometry);
                double nextPhysicsObj = _backend.EvaluateObjective(nextDerived.ResidualUpsilon);
                double nextTotalObj = gaugePenalty.AddToObjective(nextPhysicsObj, nextOmega);

                double directionalDeriv = -DotProduct(totalGradient, searchDirection);
                double sufficientDecrease = _options.ArmijoParameter * stepSize * directionalDeriv;
                if (nextTotalObj <= totalObjective - sufficientDecrease)
                    break;

                stepSize *= _options.BacktrackFactor;
                nextOmega = null;
            }

            if (nextOmega == null)
                return BuildResult(false, "Line search failed", iter, omega, derived, history);

            history[^1] = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = stepSize * dirNorm,
                StepSize = stepSize,
            };

            prevGradient = totalGradient;
            omega = nextOmega;
        }

        var finalDerived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
        return BuildResult(false, "Maximum iterations reached", _options.MaxIterations, omega, finalDerived, history);
    }

    /// <summary>
    /// Mode C: Nonlinear conjugate gradient for stationarity.
    /// </summary>
    private SolverResult SolveModeCConjugateGradient(
        FieldTensor initialOmega, FieldTensor a0, BranchManifest manifest, GeometryContext geometry)
    {
        var history = new List<ConvergenceRecord>();
        var diagnostics = new ConvergenceDiagnostics();
        var gaugePenalty = new GaugePenaltyTerm(_options.GaugePenaltyLambda);
        var omega = CloneField(initialOmega);
        FieldTensor? prevGradient = null;
        FieldTensor? searchDirection = null;

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

            var record = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = 0,
                StepSize = 0,
            };
            history.Add(record);
            diagnostics.Record(record);

            if (gradNorm < _options.GradientTolerance)
                return BuildResult(true, "Gradient norm below tolerance (stationarity)", iter, omega, derived, history);
            if (diagnostics.IsStagnated())
                return BuildResult(false, "Stagnation detected", iter, omega, derived, history);

            double beta = 0;
            if (prevGradient != null && searchDirection != null)
            {
                double dotDiff = DotProduct(totalGradient, SubtractFields(totalGradient, prevGradient));
                double dotOld = DotProduct(prevGradient, prevGradient);
                if (dotOld > 0)
                    beta = System.Math.Max(0, dotDiff / dotOld);
            }

            if (searchDirection == null || beta == 0)
                searchDirection = NegateField(totalGradient);
            else
                searchDirection = AddScaled(NegateField(totalGradient), searchDirection, beta);

            double stepSize = _options.InitialStepSize;
            FieldTensor? nextOmega = null;
            double dirNorm = ComputeL2Norm(searchDirection);

            for (int bt = 0; bt < _options.MaxBacktrackSteps; bt++)
            {
                nextOmega = AddScaled(omega, searchDirection, stepSize);
                var nextDerived = _backend.EvaluateDerived(nextOmega, a0, manifest, geometry);
                double nextPhysicsObj = _backend.EvaluateObjective(nextDerived.ResidualUpsilon);
                double nextTotalObj = gaugePenalty.AddToObjective(nextPhysicsObj, nextOmega);

                double directionalDeriv = -DotProduct(totalGradient, searchDirection);
                double sufficientDecrease = _options.ArmijoParameter * stepSize * directionalDeriv;
                if (nextTotalObj <= totalObjective - sufficientDecrease)
                    break;

                stepSize *= _options.BacktrackFactor;
                nextOmega = null;
            }

            if (nextOmega == null)
                return BuildResult(false, "Line search failed", iter, omega, derived, history);

            history[^1] = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = stepSize * dirNorm,
                StepSize = stepSize,
            };

            prevGradient = totalGradient;
            omega = nextOmega;
        }

        var finalDerived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
        return BuildResult(false, "Maximum iterations reached", _options.MaxIterations, omega, finalDerived, history);
    }

    /// <summary>
    /// Mode B: Gauss-Newton. Solves J^T J delta = -J^T M Upsilon via CG inner loop.
    /// </summary>
    private SolverResult SolveModeBGaussNewton(
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

            var record = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = 0,
                StepSize = 0,
            };
            history.Add(record);
            diagnostics.Record(record);

            if (totalObjective >= 0 && totalObjective < _options.ObjectiveTolerance)
                return BuildResult(true, "Objective below tolerance", iter, omega, derived, history);
            if (gradNorm < _options.GradientTolerance)
                return BuildResult(true, "Gradient norm below tolerance", iter, omega, derived, history);
            if (diagnostics.IsStagnated())
                return BuildResult(false, "Stagnation detected", iter, omega, derived, history);

            // Gauss-Newton: solve (J^T J + lambda I) delta = -(J^T M Upsilon + lambda omega) via CG
            var rhs = NegateField(totalGradient);
            var delta = SolveCgNormalEquations(jacobian, rhs, _options.GaugePenaltyLambda);
            double stepNorm = ComputeL2Norm(delta);

            // Line search along GN direction (natural step = 1)
            double stepSize = 1.0;
            FieldTensor? nextOmega = null;

            for (int bt = 0; bt < _options.MaxBacktrackSteps; bt++)
            {
                nextOmega = AddScaled(omega, delta, stepSize);
                var nextDerived = _backend.EvaluateDerived(nextOmega, a0, manifest, geometry);
                double nextPhysicsObj = _backend.EvaluateObjective(nextDerived.ResidualUpsilon);
                double nextTotalObj = gaugePenalty.AddToObjective(nextPhysicsObj, nextOmega);

                double sufficientDecrease = _options.ArmijoParameter * stepSize * gradNorm * stepNorm;
                if (nextTotalObj <= totalObjective - sufficientDecrease)
                    break;

                stepSize *= _options.BacktrackFactor;
                nextOmega = null;
            }

            if (nextOmega == null)
                return BuildResult(false, "Line search failed", iter, omega, derived, history);

            history[^1] = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = stepSize * stepNorm,
                StepSize = stepSize,
            };

            omega = nextOmega;
        }

        var finalDerived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
        return BuildResult(false, "Maximum iterations reached", _options.MaxIterations, omega, finalDerived, history);
    }

    /// <summary>
    /// Mode C: Gauss-Newton for stationarity.
    /// </summary>
    private SolverResult SolveModeCGaussNewton(
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

            var record = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = 0,
                StepSize = 0,
            };
            history.Add(record);
            diagnostics.Record(record);

            if (gradNorm < _options.GradientTolerance)
                return BuildResult(true, "Gradient norm below tolerance (stationarity)", iter, omega, derived, history);
            if (diagnostics.IsStagnated())
                return BuildResult(false, "Stagnation detected", iter, omega, derived, history);

            var rhs = NegateField(totalGradient);
            var delta = SolveCgNormalEquations(jacobian, rhs, _options.GaugePenaltyLambda);
            double stepNorm = ComputeL2Norm(delta);

            double stepSize = 1.0;
            FieldTensor? nextOmega = null;

            for (int bt = 0; bt < _options.MaxBacktrackSteps; bt++)
            {
                nextOmega = AddScaled(omega, delta, stepSize);
                var nextDerived = _backend.EvaluateDerived(nextOmega, a0, manifest, geometry);
                double nextPhysicsObj = _backend.EvaluateObjective(nextDerived.ResidualUpsilon);
                double nextTotalObj = gaugePenalty.AddToObjective(nextPhysicsObj, nextOmega);

                double sufficientDecrease = _options.ArmijoParameter * stepSize * gradNorm * stepNorm;
                if (nextTotalObj <= totalObjective - sufficientDecrease)
                    break;

                stepSize *= _options.BacktrackFactor;
                nextOmega = null;
            }

            if (nextOmega == null)
                return BuildResult(false, "Line search failed", iter, omega, derived, history);

            history[^1] = new ConvergenceRecord
            {
                Iteration = iter,
                Objective = totalObjective,
                ResidualNorm = residualNorm,
                GradientNorm = gradNorm,
                GaugeViolation = gaugeViolation,
                StepNorm = stepSize * stepNorm,
                StepSize = stepSize,
            };

            omega = nextOmega;
        }

        var finalDerived = _backend.EvaluateDerived(omega, a0, manifest, geometry);
        return BuildResult(false, "Maximum iterations reached", _options.MaxIterations, omega, finalDerived, history);
    }

    /// <summary>
    /// Solve (J^T J + lambda I) x = rhs via conjugate gradient (inner CG loop for Gauss-Newton).
    /// </summary>
    private FieldTensor SolveCgNormalEquations(ILinearOperator jacobian, FieldTensor rhs, double lambda)
    {
        var x = ZeroField(rhs);
        var r = CloneField(rhs);
        var p = CloneField(r);
        double rDotR = DotProduct(r, r);
        double rhsNorm = System.Math.Sqrt(rDotR);
        double tol = _options.CgTolerance * rhsNorm;

        if (rhsNorm < 1e-15)
            return x;

        for (int k = 0; k < _options.MaxCgIterations; k++)
        {
            // Ap = J^T J p + lambda p
            var jp = jacobian.Apply(p);
            var jtjp = jacobian.ApplyTranspose(jp);
            var ap = lambda > 0 ? AddScaled(jtjp, p, lambda) : jtjp;

            double pAp = DotProduct(p, ap);
            if (pAp <= 0)
                break; // Negative curvature direction; stop CG

            double alpha = rDotR / pAp;
            x = AddScaled(x, p, alpha);
            r = AddScaled(r, ap, -alpha);

            double rDotRNew = DotProduct(r, r);
            if (System.Math.Sqrt(rDotRNew) < tol)
                break;

            double cgBeta = rDotRNew / rDotR;
            p = AddScaled(r, p, cgBeta);
            rDotR = rDotRNew;
        }

        return x;
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

    private static double DotProduct(FieldTensor a, FieldTensor b)
    {
        double sum = 0;
        for (int i = 0; i < a.Coefficients.Length; i++)
            sum += a.Coefficients[i] * b.Coefficients[i];
        return sum;
    }

    private static FieldTensor NegateField(FieldTensor f)
    {
        var result = new double[f.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = -f.Coefficients[i];
        return new FieldTensor
        {
            Label = f.Label,
            Signature = f.Signature,
            Coefficients = result,
            Shape = f.Shape,
        };
    }

    private static FieldTensor SubtractFields(FieldTensor a, FieldTensor b)
    {
        var result = new double[a.Coefficients.Length];
        for (int i = 0; i < result.Length; i++)
            result[i] = a.Coefficients[i] - b.Coefficients[i];
        return new FieldTensor
        {
            Label = a.Label,
            Signature = a.Signature,
            Coefficients = result,
            Shape = a.Shape,
        };
    }

    private static FieldTensor ZeroField(FieldTensor template)
    {
        return new FieldTensor
        {
            Label = template.Label,
            Signature = template.Signature,
            Coefficients = new double[template.Coefficients.Length],
            Shape = template.Shape,
        };
    }
}
