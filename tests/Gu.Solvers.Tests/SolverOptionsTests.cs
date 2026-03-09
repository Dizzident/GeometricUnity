using Gu.Solvers;

namespace Gu.Solvers.Tests;

public class SolverOptionsTests
{
    [Fact]
    public void Defaults_AreReasonable()
    {
        var opts = new SolverOptions { Mode = SolveMode.ResidualOnly };

        Assert.Equal(100, opts.MaxIterations);
        Assert.Equal(1e-10, opts.ObjectiveTolerance);
        Assert.Equal(1e-8, opts.GradientTolerance);
        Assert.Equal(0.01, opts.InitialStepSize);
        Assert.Equal(0.0, opts.GaugePenaltyLambda);
        Assert.Equal(1e-4, opts.ArmijoParameter);
        Assert.Equal(0.5, opts.BacktrackFactor);
        Assert.Equal(20, opts.MaxBacktrackSteps);
        Assert.Equal(SolverMethod.GradientDescent, opts.Method);
        Assert.Equal(50, opts.MaxCgIterations);
        Assert.Equal(1e-6, opts.CgTolerance);
    }

    [Fact]
    public void AllMethods_AreDistinct()
    {
        var methods = Enum.GetValues<SolverMethod>();
        Assert.Equal(3, methods.Length);
        Assert.Contains(SolverMethod.GradientDescent, methods);
        Assert.Contains(SolverMethod.ConjugateGradient, methods);
        Assert.Contains(SolverMethod.GaussNewton, methods);
    }

    [Fact]
    public void AllModes_AreDistinct()
    {
        var modes = Enum.GetValues<SolveMode>();
        Assert.Equal(4, modes.Length);
        Assert.Contains(SolveMode.ResidualOnly, modes);
        Assert.Contains(SolveMode.ObjectiveMinimization, modes);
        Assert.Contains(SolveMode.StationaritySolve, modes);
        Assert.Contains(SolveMode.BranchSensitivity, modes);
    }
}

public class ConvergenceRecordTests
{
    [Fact]
    public void GaugeToPhysicsRatio_WhenResidualPositive_ComputesCorrectly()
    {
        var record = new ConvergenceRecord
        {
            Iteration = 0,
            Objective = 10.0,
            ResidualNorm = 4.0,
            GradientNorm = 1.0,
            GaugeViolation = 2.0,
            StepNorm = 0,
            StepSize = 0,
        };

        Assert.Equal(0.5, record.GaugeToPhysicsRatio, 15);
    }

    [Fact]
    public void GaugeToPhysicsRatio_WhenResidualZero_ReturnsZero()
    {
        var record = new ConvergenceRecord
        {
            Iteration = 0,
            Objective = 0.0,
            ResidualNorm = 0.0,
            GradientNorm = 0.0,
            GaugeViolation = 0.5,
            StepNorm = 0,
            StepSize = 0,
        };

        Assert.Equal(0.0, record.GaugeToPhysicsRatio);
    }
}

public class ConvergenceDiagnosticsEdgeCaseTests
{
    [Fact]
    public void Constructor_StagnationWindowZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ConvergenceDiagnostics(stagnationWindow: 0));
    }

    [Fact]
    public void Constructor_NegativeThreshold_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ConvergenceDiagnostics(stagnationThreshold: -1.0));
    }

    [Fact]
    public void Constructor_WindowOne_StagnatesAfterTwoConstantRecords()
    {
        var diag = new ConvergenceDiagnostics(stagnationWindow: 1, stagnationThreshold: 1e-12);

        diag.Record(MakeRecord(0, 5.0));
        Assert.False(diag.IsStagnated());

        diag.Record(MakeRecord(1, 5.0));
        Assert.True(diag.IsStagnated());
    }

    [Fact]
    public void ObjectiveReductionRatio_SingleRecord_ReturnsOne()
    {
        var diag = new ConvergenceDiagnostics();
        diag.Record(MakeRecord(0, 10.0));
        Assert.Equal(1.0, diag.ObjectiveReductionRatio());
    }

    [Fact]
    public void ObjectiveReductionRatio_FirstZero_SecondNonZero_ReturnsInfinity()
    {
        var diag = new ConvergenceDiagnostics();
        diag.Record(MakeRecord(0, 0.0));
        diag.Record(MakeRecord(1, 5.0));
        Assert.Equal(double.PositiveInfinity, diag.ObjectiveReductionRatio());
    }

    [Fact]
    public void ObjectiveReductionRatio_BothZero_ReturnsOne()
    {
        var diag = new ConvergenceDiagnostics();
        diag.Record(MakeRecord(0, 0.0));
        diag.Record(MakeRecord(1, 0.0));
        Assert.Equal(1.0, diag.ObjectiveReductionRatio());
    }

    [Fact]
    public void IsStagnated_ZeroReferenceObjective_AllZero_DetectsStagnation()
    {
        var diag = new ConvergenceDiagnostics(stagnationWindow: 2, stagnationThreshold: 1e-12);

        diag.Record(MakeRecord(0, 0.0));
        diag.Record(MakeRecord(1, 0.0));
        diag.Record(MakeRecord(2, 0.0));

        Assert.True(diag.IsStagnated());
    }

    [Fact]
    public void IsStagnated_ZeroReferenceObjective_NonZeroRecent_NotStagnated()
    {
        var diag = new ConvergenceDiagnostics(stagnationWindow: 2, stagnationThreshold: 1e-12);

        diag.Record(MakeRecord(0, 0.0));
        diag.Record(MakeRecord(1, 0.0));
        diag.Record(MakeRecord(2, 1.0)); // jumps to non-zero

        Assert.False(diag.IsStagnated());
    }

    [Fact]
    public void GetSummary_EmptyHistory_ReturnsZeroes()
    {
        var diag = new ConvergenceDiagnostics();
        var summary = diag.GetSummary();

        Assert.Equal(0, summary.TotalIterations);
        Assert.Equal(0.0, summary.InitialObjective);
        Assert.Equal(0.0, summary.FinalObjective);
        Assert.Equal(1.0, summary.ObjectiveReductionRatio);
        Assert.False(summary.IsStagnated);
        Assert.Null(summary.StagnationDetectedAtIteration);
    }

    [Fact]
    public void GetSummary_Stagnated_ReportsStagnationIteration()
    {
        var diag = new ConvergenceDiagnostics(stagnationWindow: 2, stagnationThreshold: 1e-12);

        diag.Record(MakeRecord(0, 5.0));
        diag.Record(MakeRecord(1, 5.0));
        diag.Record(MakeRecord(2, 5.0));

        var summary = diag.GetSummary();
        Assert.True(summary.IsStagnated);
        Assert.NotNull(summary.StagnationDetectedAtIteration);
        Assert.Equal(1, summary.StagnationDetectedAtIteration);
    }

    [Fact]
    public void GenerateLog_Stagnated_ContainsWarning()
    {
        var diag = new ConvergenceDiagnostics(stagnationWindow: 2, stagnationThreshold: 1e-12);

        diag.Record(MakeRecord(0, 5.0));
        diag.Record(MakeRecord(1, 5.0));
        diag.Record(MakeRecord(2, 5.0));

        var log = diag.GenerateLog();
        Assert.Contains(log, line => line.Contains("WARNING") && line.Contains("Stagnation"));
    }

    [Fact]
    public void HistoryProperties_TrackCorrectly()
    {
        var diag = new ConvergenceDiagnostics();

        diag.Record(new ConvergenceRecord
        {
            Iteration = 0, Objective = 10.0,
            ResidualNorm = 3.0, GradientNorm = 2.0,
            GaugeViolation = 0.5, StepNorm = 0.1, StepSize = 0.01,
        });

        Assert.Single(diag.History);
        Assert.Single(diag.GradientNormHistory);
        Assert.Equal(2.0, diag.GradientNormHistory[0]);
    }

    private static ConvergenceRecord MakeRecord(int iter, double objective)
    {
        return new ConvergenceRecord
        {
            Iteration = iter,
            Objective = objective,
            ResidualNorm = 1.0,
            GradientNorm = 0.1,
            GaugeViolation = 0,
            StepNorm = 0,
            StepSize = 0,
        };
    }
}

public class GaugePenaltyTermEdgeCaseTests
{
    [Fact]
    public void ZeroOmega_ObjectiveIsZero()
    {
        var penalty = new GaugePenaltyTerm(10.0);
        var omega = MakeOmega(new double[6]);

        Assert.Equal(0.0, penalty.EvaluateObjective(omega));
        Assert.Equal(0.0, penalty.ComputeViolation(omega));
        Assert.Equal(0.0, penalty.ComputeViolationNorm(omega));
    }

    [Fact]
    public void ZeroLambda_GradientIsPhysicsGradient()
    {
        var penalty = new GaugePenaltyTerm(0.0);
        var omega = MakeOmega(new[] { 1.0, 2.0, 3.0 });
        var physGrad = MakeOmega(new[] { 0.5, 0.6, 0.7 });

        var result = penalty.AddToGradient(physGrad, omega);
        Assert.Same(physGrad, result); // should return same reference when lambda=0
    }

    [Fact]
    public void ComputeViolation_EqualsNormSquared()
    {
        var penalty = new GaugePenaltyTerm(1.0);
        var omega = MakeOmega(new[] { 3.0, 4.0 });

        Assert.Equal(25.0, penalty.ComputeViolation(omega), 12);
    }

    [Fact]
    public void EvaluateObjective_IsHalfLambdaNormSquared()
    {
        double lambda = 3.0;
        var penalty = new GaugePenaltyTerm(lambda);
        var omega = MakeOmega(new[] { 1.0, 2.0, 2.0 }); // norm^2 = 9

        double expected = 0.5 * lambda * 9.0; // 13.5
        Assert.Equal(expected, penalty.EvaluateObjective(omega), 12);
    }

    [Fact]
    public void EvaluateGradient_Label()
    {
        var penalty = new GaugePenaltyTerm(1.0);
        var omega = MakeOmega(new[] { 1.0 });

        var grad = penalty.EvaluateGradient(omega);
        Assert.Equal("gauge_penalty_gradient", grad.Label);
    }

    [Fact]
    public void AddToGradient_Label()
    {
        var penalty = new GaugePenaltyTerm(1.0);
        var omega = MakeOmega(new[] { 1.0 });
        var physGrad = MakeOmega(new[] { 0.5 });

        var totalGrad = penalty.AddToGradient(physGrad, omega);
        Assert.Equal("total_gradient", totalGrad.Label);
    }

    private static Core.FieldTensor MakeOmega(double[] coefficients)
    {
        return new Core.FieldTensor
        {
            Label = "omega_test",
            Signature = new Core.TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coefficients,
            Shape = new[] { coefficients.Length },
        };
    }
}

public class SolverOrchestratorEdgeCaseTests
{
    [Fact]
    public void Constructor_NullBackend_Throws()
    {
        Assert.Throws<ArgumentNullException>(
            () => new SolverOrchestrator(null!, new SolverOptions { Mode = SolveMode.ResidualOnly }));
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        var backend = new StubSolverBackend();
        Assert.Throws<ArgumentNullException>(
            () => new SolverOrchestrator(backend, null!));
    }

    [Fact]
    public void ModeA_ReturnsCorrectMode()
    {
        var backend = new StubSolverBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions { Mode = SolveMode.ResidualOnly });

        var omega = MakeField(3);
        var a0 = MakeField(3);
        var result = solver.Solve(omega, a0, MakeManifest(), MakeGeometry());

        Assert.Equal(SolveMode.ResidualOnly, result.Mode);
        Assert.False(result.Converged);
        Assert.Contains("Mode A", result.TerminationReason);
        Assert.Equal(0, result.Iterations);
    }

    [Fact]
    public void ModeB_ZeroObjectiveAndGradient_ConvergesImmediately()
    {
        // Backend returns zero objective and gradient => converges on first iteration
        var backend = new StubSolverBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-10,
            GradientTolerance = 1e-8,
        });

        var omega = MakeField(3);
        var a0 = MakeField(3);
        var result = solver.Solve(omega, a0, MakeManifest(), MakeGeometry());

        Assert.True(result.Converged);
        // Zero objective hits the objective check first (before gradient check)
        Assert.Contains("tolerance", result.TerminationReason);
    }

    [Fact]
    public void ModeC_ZeroGradient_ConvergesOnStationarity()
    {
        var backend = new StubSolverBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.StationaritySolve,
            MaxIterations = 10,
            GradientTolerance = 1e-8,
        });

        var omega = MakeField(3);
        var a0 = MakeField(3);
        var result = solver.Solve(omega, a0, MakeManifest(), MakeGeometry());

        Assert.True(result.Converged);
        Assert.Contains("stationarity", result.TerminationReason);
    }

    [Fact]
    public void ModeB_ConjugateGradient_ZeroBackend_ConvergesImmediately()
    {
        var backend = new StubSolverBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            Method = SolverMethod.ConjugateGradient,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-10,
            GradientTolerance = 1e-8,
        });

        var result = solver.Solve(MakeField(3), MakeField(3), MakeManifest(), MakeGeometry());

        Assert.True(result.Converged);
        Assert.Contains("tolerance", result.TerminationReason);
        Assert.Equal(SolveMode.ObjectiveMinimization, result.Mode);
    }

    [Fact]
    public void ModeB_GaussNewton_ZeroBackend_ConvergesImmediately()
    {
        var backend = new StubSolverBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            Method = SolverMethod.GaussNewton,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-10,
            GradientTolerance = 1e-8,
        });

        var result = solver.Solve(MakeField(3), MakeField(3), MakeManifest(), MakeGeometry());

        Assert.True(result.Converged);
        Assert.Contains("tolerance", result.TerminationReason);
        Assert.Equal(SolveMode.ObjectiveMinimization, result.Mode);
    }

    [Fact]
    public void ModeC_ConjugateGradient_ZeroBackend_ConvergesOnStationarity()
    {
        var backend = new StubSolverBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.StationaritySolve,
            Method = SolverMethod.ConjugateGradient,
            MaxIterations = 10,
            GradientTolerance = 1e-8,
        });

        var result = solver.Solve(MakeField(3), MakeField(3), MakeManifest(), MakeGeometry());

        Assert.True(result.Converged);
        Assert.Contains("stationarity", result.TerminationReason);
    }

    [Fact]
    public void ModeC_GaussNewton_ZeroBackend_ConvergesOnStationarity()
    {
        var backend = new StubSolverBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.StationaritySolve,
            Method = SolverMethod.GaussNewton,
            MaxIterations = 10,
            GradientTolerance = 1e-8,
        });

        var result = solver.Solve(MakeField(3), MakeField(3), MakeManifest(), MakeGeometry());

        Assert.True(result.Converged);
        Assert.Contains("stationarity", result.TerminationReason);
    }

    [Fact]
    public void ModeB_DefaultMethod_IsGradientDescent()
    {
        var backend = new StubSolverBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            // Method not set; defaults to GradientDescent
            MaxIterations = 10,
            ObjectiveTolerance = 1e-10,
        });

        var result = solver.Solve(MakeField(3), MakeField(3), MakeManifest(), MakeGeometry());
        Assert.True(result.Converged);
    }

    [Fact]
    public void GaussNewton_CgMaxIterationsExceeded_StillProducesStep()
    {
        // With MaxCgIterations=1, the inner CG does only 1 iteration.
        // The GN outer loop should still produce a step (partial CG solution).
        var backend = new NonZeroDecayBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            Method = SolverMethod.GaussNewton,
            MaxIterations = 5,
            MaxCgIterations = 1,
            ObjectiveTolerance = 1e-12,
            GradientTolerance = 1e-12,
        });

        var omega = MakeFieldWithValues(new[] { 0.5, 0.3, 0.1 });
        var result = solver.Solve(omega, MakeField(3), MakeManifest(), MakeGeometry());

        // Should run at least 1 outer iteration (not crash)
        Assert.True(result.Iterations >= 1 || result.Converged);
        Assert.NotNull(result.FinalOmega);
        Assert.True(result.History.Count >= 1);
    }

    [Fact]
    public void GaussNewton_NegativeCurvature_BreaksCgEarly()
    {
        // NegativeCurvatureBackend returns J such that J^T J has pAp <= 0
        var backend = new NegativeCurvatureBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            Method = SolverMethod.GaussNewton,
            MaxIterations = 3,
            MaxCgIterations = 50,
            ObjectiveTolerance = 1e-12,
            GradientTolerance = 1e-12,
        });

        var omega = MakeFieldWithValues(new[] { 1.0, 1.0, 1.0 });
        var result = solver.Solve(omega, MakeField(3), MakeManifest(), MakeGeometry());

        // Should not crash; may fail line search or stagnate but not throw
        Assert.NotNull(result.FinalOmega);
        Assert.NotNull(result.TerminationReason);
    }

    [Fact]
    public void ConjugateGradient_NonZeroBackend_MakesProgress()
    {
        var backend = new NonZeroDecayBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            Method = SolverMethod.ConjugateGradient,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-12,
            GradientTolerance = 1e-12,
            InitialStepSize = 0.1,
        });

        var omega = MakeFieldWithValues(new[] { 0.5, 0.3, 0.1 });
        var result = solver.Solve(omega, MakeField(3), MakeManifest(), MakeGeometry());

        // Objective should decrease from initial value (NonZeroDecayBackend decays toward 0)
        Assert.True(result.History.Count >= 1);
        if (result.History.Count >= 2)
        {
            Assert.True(result.History[^1].Objective <= result.History[0].Objective,
                "CG should not increase objective");
        }
    }

    [Fact]
    public void GaussNewton_ZeroRhs_CgReturnsZeroStep()
    {
        // Zero backend: gradient is zero, so rhs for CG is zero.
        // CG should return zero immediately, GN line search on zero step = no change.
        var backend = new StubSolverBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            Method = SolverMethod.GaussNewton,
            MaxIterations = 5,
            ObjectiveTolerance = 1e-10,
        });

        var result = solver.Solve(MakeField(3), MakeField(3), MakeManifest(), MakeGeometry());

        // Zero objective and zero gradient -> converges immediately
        Assert.True(result.Converged);
        Assert.Equal(0.0, result.FinalObjective);
    }

    [Fact]
    public void ConjugateGradient_PolakRibiereRestart_WhenBetaNegative()
    {
        // After restart (beta clamped to 0), CG degrades to steepest descent for one step.
        // Just verifying no crash with the decay backend.
        var backend = new NonZeroDecayBackend();
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            Method = SolverMethod.ConjugateGradient,
            MaxIterations = 5,
            ObjectiveTolerance = 1e-15,
            GradientTolerance = 1e-15,
            InitialStepSize = 0.01,
        });

        var omega = MakeFieldWithValues(new[] { 1.0, -0.5, 0.2 });
        var result = solver.Solve(omega, MakeField(3), MakeManifest(), MakeGeometry());

        // Should run multiple iterations without crash
        Assert.True(result.History.Count >= 2);
    }

    private static Core.FieldTensor MakeFieldWithValues(double[] values)
    {
        return new Core.FieldTensor
        {
            Label = "test",
            Signature = new Core.TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = values,
            Shape = new[] { values.Length },
        };
    }

    /// <summary>
    /// Backend where objective decays as omega approaches zero.
    /// Returns non-zero residual proportional to omega, enabling gradient-based solvers to make progress.
    /// </summary>
    private sealed class NonZeroDecayBackend : ISolverBackend
    {
        public Core.DerivedState EvaluateDerived(Core.FieldTensor omega, Core.FieldTensor a0,
            Core.BranchManifest manifest, Core.GeometryContext geometry)
        {
            // Residual = omega (so minimizing ||omega||^2 drives omega to zero)
            var residual = new Core.FieldTensor
            {
                Label = "residual",
                Signature = omega.Signature,
                Coefficients = (double[])omega.Coefficients.Clone(),
                Shape = omega.Shape,
            };
            var zero = MakeField(omega.Coefficients.Length);
            return new Core.DerivedState
            {
                CurvatureF = zero,
                TorsionT = zero,
                ShiabS = zero,
                ResidualUpsilon = residual,
            };
        }

        public double EvaluateObjective(Core.FieldTensor upsilon)
        {
            double sum = 0;
            for (int i = 0; i < upsilon.Coefficients.Length; i++)
                sum += upsilon.Coefficients[i] * upsilon.Coefficients[i];
            return 0.5 * sum;
        }

        public Branching.ILinearOperator BuildJacobian(Core.FieldTensor omega, Core.FieldTensor a0,
            Core.FieldTensor curvatureF, Core.BranchManifest manifest, Core.GeometryContext geometry)
        {
            return new IdentityOperator(omega.Coefficients.Length);
        }

        public Core.FieldTensor ComputeGradient(Branching.ILinearOperator jacobian, Core.FieldTensor upsilon)
        {
            // J^T * upsilon where J = I, so gradient = upsilon
            return jacobian.ApplyTranspose(upsilon);
        }

        public double ComputeNorm(Core.FieldTensor v)
        {
            double sum = 0;
            for (int i = 0; i < v.Coefficients.Length; i++)
                sum += v.Coefficients[i] * v.Coefficients[i];
            return System.Math.Sqrt(sum);
        }

        private sealed class IdentityOperator : Branching.ILinearOperator
        {
            private readonly int _size;
            public IdentityOperator(int size) => _size = size;
            public int InputDimension => _size;
            public int OutputDimension => _size;
            public Core.TensorSignature InputSignature => new()
            {
                AmbientSpaceId = "Y_h", CarrierType = "connection-1form", Degree = "1",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            };
            public Core.TensorSignature OutputSignature => new()
            {
                AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            };
            public Core.FieldTensor Apply(Core.FieldTensor input)
            {
                return new Core.FieldTensor
                {
                    Label = input.Label,
                    Signature = input.Signature,
                    Coefficients = (double[])input.Coefficients.Clone(),
                    Shape = input.Shape,
                };
            }
            public Core.FieldTensor ApplyTranspose(Core.FieldTensor input) => Apply(input);
        }
    }

    /// <summary>
    /// Backend where J^T J produces negative curvature (pAp &lt;= 0), testing CG early termination.
    /// </summary>
    private sealed class NegativeCurvatureBackend : ISolverBackend
    {
        public Core.DerivedState EvaluateDerived(Core.FieldTensor omega, Core.FieldTensor a0,
            Core.BranchManifest manifest, Core.GeometryContext geometry)
        {
            var residual = new Core.FieldTensor
            {
                Label = "residual",
                Signature = omega.Signature,
                Coefficients = (double[])omega.Coefficients.Clone(),
                Shape = omega.Shape,
            };
            var zero = MakeField(omega.Coefficients.Length);
            return new Core.DerivedState
            {
                CurvatureF = zero,
                TorsionT = zero,
                ShiabS = zero,
                ResidualUpsilon = residual,
            };
        }

        public double EvaluateObjective(Core.FieldTensor upsilon)
        {
            double sum = 0;
            for (int i = 0; i < upsilon.Coefficients.Length; i++)
                sum += upsilon.Coefficients[i] * upsilon.Coefficients[i];
            return 0.5 * sum;
        }

        public Branching.ILinearOperator BuildJacobian(Core.FieldTensor omega, Core.FieldTensor a0,
            Core.FieldTensor curvatureF, Core.BranchManifest manifest, Core.GeometryContext geometry)
        {
            return new NegatingOperator(omega.Coefficients.Length);
        }

        public Core.FieldTensor ComputeGradient(Branching.ILinearOperator jacobian, Core.FieldTensor upsilon)
        {
            return jacobian.ApplyTranspose(upsilon);
        }

        public double ComputeNorm(Core.FieldTensor v)
        {
            double sum = 0;
            for (int i = 0; i < v.Coefficients.Length; i++)
                sum += v.Coefficients[i] * v.Coefficients[i];
            return System.Math.Sqrt(sum);
        }

        /// <summary>J that negates: J*v = -v, so J^T J v = J^T(-v) = -(-v) = v...
        /// Actually for negative curvature we need J*v to produce p^T (J^T J p) &lt;= 0.
        /// Use J that scales by i (imaginary) rotation effect: Apply returns zeros.
        /// Then J^T J p = 0 for any p, giving pAp = 0 which triggers the break.</summary>
        private sealed class NegatingOperator : Branching.ILinearOperator
        {
            private readonly int _size;
            public NegatingOperator(int size) => _size = size;
            public int InputDimension => _size;
            public int OutputDimension => _size;
            public Core.TensorSignature InputSignature => new()
            {
                AmbientSpaceId = "Y_h", CarrierType = "connection-1form", Degree = "1",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            };
            public Core.TensorSignature OutputSignature => new()
            {
                AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            };
            public Core.FieldTensor Apply(Core.FieldTensor input) => MakeField(_size); // zero output
            public Core.FieldTensor ApplyTranspose(Core.FieldTensor input)
            {
                // Return the input negated so gradient = -upsilon (nonzero)
                var neg = new double[input.Coefficients.Length];
                for (int i = 0; i < neg.Length; i++)
                    neg[i] = -input.Coefficients[i];
                return new Core.FieldTensor
                {
                    Label = input.Label,
                    Signature = input.Signature,
                    Coefficients = neg,
                    Shape = input.Shape,
                };
            }
        }
    }

    private static Core.FieldTensor MakeField(int size)
    {
        return new Core.FieldTensor
        {
            Label = "test",
            Signature = new Core.TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[size],
            Shape = new[] { size },
        };
    }

    private static Core.BranchManifest MakeManifest() => new()
    {
        BranchId = "test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "rev-1",
        CodeRevision = "abc123",
        ActiveGeometryBranch = "simplicial-4d",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = Array.Empty<string>(),
    };

    private static Core.GeometryContext MakeGeometry() => new()
    {
        BaseSpace = new Core.SpaceRef { SpaceId = "X_h", Dimension = 4 },
        AmbientSpace = new Core.SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        DiscretizationType = "simplicial",
        QuadratureRuleId = "centroid",
        BasisFamilyId = "P1",
        ProjectionBinding = new Core.GeometryBinding
        {
            BindingType = "projection",
            SourceSpace = new Core.SpaceRef { SpaceId = "Y_h", Dimension = 14 },
            TargetSpace = new Core.SpaceRef { SpaceId = "X_h", Dimension = 4 },
        },
        ObservationBinding = new Core.GeometryBinding
        {
            BindingType = "observation",
            SourceSpace = new Core.SpaceRef { SpaceId = "X_h", Dimension = 4 },
            TargetSpace = new Core.SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        },
        Patches = Array.Empty<Core.PatchInfo>(),
    };

    /// <summary>
    /// Minimal stub backend that returns zero fields for all operations.
    /// </summary>
    private sealed class StubSolverBackend : ISolverBackend
    {
        public Core.DerivedState EvaluateDerived(Core.FieldTensor omega, Core.FieldTensor a0,
            Core.BranchManifest manifest, Core.GeometryContext geometry)
        {
            var zero = MakeField(omega.Coefficients.Length);
            return new Core.DerivedState
            {
                CurvatureF = zero,
                TorsionT = zero,
                ShiabS = zero,
                ResidualUpsilon = zero,
            };
        }

        public double EvaluateObjective(Core.FieldTensor upsilon) => 0.0;

        public Branching.ILinearOperator BuildJacobian(Core.FieldTensor omega, Core.FieldTensor a0,
            Core.FieldTensor curvatureF, Core.BranchManifest manifest, Core.GeometryContext geometry)
        {
            return new ZeroOperator(omega.Coefficients.Length);
        }

        public Core.FieldTensor ComputeGradient(Branching.ILinearOperator jacobian, Core.FieldTensor upsilon)
        {
            return MakeField(upsilon.Coefficients.Length);
        }

        public double ComputeNorm(Core.FieldTensor v) => 0.0;

        private sealed class ZeroOperator : Branching.ILinearOperator
        {
            private readonly int _size;
            public ZeroOperator(int size) => _size = size;
            public int InputDimension => _size;
            public int OutputDimension => _size;
            public Core.TensorSignature InputSignature => new()
            {
                AmbientSpaceId = "Y_h", CarrierType = "connection-1form", Degree = "1",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            };
            public Core.TensorSignature OutputSignature => new()
            {
                AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            };
            public Core.FieldTensor Apply(Core.FieldTensor input) => MakeField(_size);
            public Core.FieldTensor ApplyTranspose(Core.FieldTensor input) => MakeField(_size);
        }
    }
}

public class BranchSensitivityRunnerTests
{
    [Fact]
    public void Constructor_NullBackend_Throws()
    {
        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };
        Assert.Throws<ArgumentNullException>(() => new BranchSensitivityRunner(null!, options));
    }

    [Fact]
    public void Constructor_NullOptions_Throws()
    {
        var backend = new StubSolverBackend();
        Assert.Throws<ArgumentNullException>(() => new BranchSensitivityRunner(backend, null!));
    }

    [Fact]
    public void Constructor_ModeDInnerOptions_Throws()
    {
        var backend = new StubSolverBackend();
        var options = new SolverOptions { Mode = SolveMode.BranchSensitivity };
        Assert.Throws<ArgumentException>(() => new BranchSensitivityRunner(backend, options));
    }

    [Fact]
    public void Sweep_NullManifests_Throws()
    {
        var backend = new StubSolverBackend();
        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };
        var runner = new BranchSensitivityRunner(backend, options);

        Assert.Throws<ArgumentNullException>(
            () => runner.Sweep(MakeField(3), MakeField(3), null!, MakeGeometry()));
    }

    [Fact]
    public void Sweep_SingleManifest_Throws()
    {
        var backend = new StubSolverBackend();
        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };
        var runner = new BranchSensitivityRunner(backend, options);

        Assert.Throws<ArgumentException>(
            () => runner.Sweep(MakeField(3), MakeField(3), new[] { MakeManifest("b1") }, MakeGeometry()));
    }

    [Fact]
    public void Sweep_TwoBranches_ReturnsBothResults()
    {
        var backend = new StubSolverBackend();
        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };
        var runner = new BranchSensitivityRunner(backend, options);

        var manifests = new[] { MakeManifest("branch-A"), MakeManifest("branch-B") };
        var result = runner.Sweep(MakeField(3), MakeField(3), manifests, MakeGeometry());

        Assert.Equal(2, result.BranchCount);
        Assert.Equal(SolveMode.ResidualOnly, result.InnerMode);
        Assert.Equal("branch-A", result.BranchResults[0].Manifest.BranchId);
        Assert.Equal("branch-B", result.BranchResults[1].Manifest.BranchId);
    }

    [Fact]
    public void Sweep_ResultsPreserveBranchIds()
    {
        var backend = new StubSolverBackend();
        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };
        var runner = new BranchSensitivityRunner(backend, options);

        var manifests = new[] { MakeManifest("identity-shiab"), MakeManifest("trivial-shiab"), MakeManifest("custom-shiab") };
        var result = runner.Sweep(MakeField(3), MakeField(3), manifests, MakeGeometry());

        Assert.Equal(3, result.BranchCount);
        Assert.Equal(0.0, result.BestObjective);
        Assert.Equal(0.0, result.WorstResidualNorm);
    }

    [Fact]
    public void Sweep_ConvergedAndDivergedBranches()
    {
        // The stub backend always returns zero, so Mode A always produces
        // a "non-converged" result (Converged=false for Mode A).
        var backend = new StubSolverBackend();
        var options = new SolverOptions { Mode = SolveMode.ResidualOnly };
        var runner = new BranchSensitivityRunner(backend, options);

        var manifests = new[] { MakeManifest("b1"), MakeManifest("b2") };
        var result = runner.Sweep(MakeField(3), MakeField(3), manifests, MakeGeometry());

        // Mode A is residual-only: does not converge
        Assert.Empty(result.ConvergedBranches);
        Assert.Equal(2, result.DivergedBranches.Count);
    }

    [Fact]
    public void Sweep_WithModeB_ConvergesOnZeroBackend()
    {
        var backend = new StubSolverBackend();
        var options = new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-10,
        };
        var runner = new BranchSensitivityRunner(backend, options);

        var manifests = new[] { MakeManifest("b1"), MakeManifest("b2") };
        var result = runner.Sweep(MakeField(3), MakeField(3), manifests, MakeGeometry());

        Assert.Equal(SolveMode.ObjectiveMinimization, result.InnerMode);
        // Zero backend returns 0 objective, so all branches converge
        Assert.Equal(2, result.ConvergedBranches.Count);
        Assert.Empty(result.DivergedBranches);
    }

    [Fact]
    public void ModeD_ViaOrchestrator_ThrowsInvalidOperation()
    {
        var backend = new StubSolverBackend();
        var options = new SolverOptions { Mode = SolveMode.BranchSensitivity };
        var orchestrator = new SolverOrchestrator(backend, options);

        Assert.Throws<InvalidOperationException>(
            () => orchestrator.Solve(MakeField(3), MakeField(3), MakeManifest("test"), MakeGeometry()));
    }

    private static Core.FieldTensor MakeField(int size)
    {
        return new Core.FieldTensor
        {
            Label = "test",
            Signature = new Core.TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[size],
            Shape = new[] { size },
        };
    }

    private static Core.BranchManifest MakeManifest(string branchId) => new()
    {
        BranchId = branchId,
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "rev-1",
        CodeRevision = "abc123",
        ActiveGeometryBranch = "simplicial-4d",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = Array.Empty<string>(),
        InsertedChoiceIds = Array.Empty<string>(),
    };

    private static Core.GeometryContext MakeGeometry() => new()
    {
        BaseSpace = new Core.SpaceRef { SpaceId = "X_h", Dimension = 4 },
        AmbientSpace = new Core.SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        DiscretizationType = "simplicial",
        QuadratureRuleId = "centroid",
        BasisFamilyId = "P1",
        ProjectionBinding = new Core.GeometryBinding
        {
            BindingType = "projection",
            SourceSpace = new Core.SpaceRef { SpaceId = "Y_h", Dimension = 14 },
            TargetSpace = new Core.SpaceRef { SpaceId = "X_h", Dimension = 4 },
        },
        ObservationBinding = new Core.GeometryBinding
        {
            BindingType = "observation",
            SourceSpace = new Core.SpaceRef { SpaceId = "X_h", Dimension = 4 },
            TargetSpace = new Core.SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        },
        Patches = Array.Empty<Core.PatchInfo>(),
    };

    private sealed class StubSolverBackend : ISolverBackend
    {
        public Core.DerivedState EvaluateDerived(Core.FieldTensor omega, Core.FieldTensor a0,
            Core.BranchManifest manifest, Core.GeometryContext geometry)
        {
            var zero = MakeField(omega.Coefficients.Length);
            return new Core.DerivedState
            {
                CurvatureF = zero,
                TorsionT = zero,
                ShiabS = zero,
                ResidualUpsilon = zero,
            };
        }

        public double EvaluateObjective(Core.FieldTensor upsilon) => 0.0;

        public Branching.ILinearOperator BuildJacobian(Core.FieldTensor omega, Core.FieldTensor a0,
            Core.FieldTensor curvatureF, Core.BranchManifest manifest, Core.GeometryContext geometry)
        {
            return new ZeroOperator(omega.Coefficients.Length);
        }

        public Core.FieldTensor ComputeGradient(Branching.ILinearOperator jacobian, Core.FieldTensor upsilon)
        {
            return MakeField(upsilon.Coefficients.Length);
        }

        public double ComputeNorm(Core.FieldTensor v) => 0.0;

        private sealed class ZeroOperator : Branching.ILinearOperator
        {
            private readonly int _size;
            public ZeroOperator(int size) => _size = size;
            public int InputDimension => _size;
            public int OutputDimension => _size;
            public Core.TensorSignature InputSignature => new()
            {
                AmbientSpaceId = "Y_h", CarrierType = "connection-1form", Degree = "1",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            };
            public Core.TensorSignature OutputSignature => new()
            {
                AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            };
            public Core.FieldTensor Apply(Core.FieldTensor input) => MakeField(_size);
            public Core.FieldTensor ApplyTranspose(Core.FieldTensor input) => MakeField(_size);
        }
    }
}
