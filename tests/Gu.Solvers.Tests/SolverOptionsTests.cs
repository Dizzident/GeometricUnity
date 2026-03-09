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
    }

    [Fact]
    public void AllModes_AreDistinct()
    {
        var modes = Enum.GetValues<SolveMode>();
        Assert.Equal(3, modes.Length);
        Assert.Contains(SolveMode.ResidualOnly, modes);
        Assert.Contains(SolveMode.ObjectiveMinimization, modes);
        Assert.Contains(SolveMode.StationaritySolve, modes);
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
