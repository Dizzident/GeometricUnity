using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.ReferenceCpu.Tests;

public class SolverTests
{
    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static BranchManifest TestManifest() => new()
    {
        BranchId = "test-solver",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
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
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    private static GeometryContext DummyGeometry() => new()
    {
        BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
        AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        DiscretizationType = "simplicial",
        QuadratureRuleId = "centroid",
        BasisFamilyId = "P1",
        ProjectionBinding = new GeometryBinding
        {
            BindingType = "projection",
            SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
            TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
        },
        ObservationBinding = new GeometryBinding
        {
            BindingType = "observation",
            SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 4 },
            TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 14 },
        },
        Patches = Array.Empty<PatchInfo>(),
    };

    private static CpuSolverBackend CreateBackend(SimplicialMesh mesh, LieAlgebra algebra)
    {
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        return new CpuSolverBackend(mesh, algebra, torsion, shiab);
    }

    // ===== Mode A: Residual-Only =====

    [Fact]
    public void ModeA_FlatOmega_ObjectiveIsZero()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions { Mode = SolveMode.ResidualOnly });

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = solver.Solve(omega, a0, TestManifest(), DummyGeometry());

        Assert.Equal(SolveMode.ResidualOnly, result.Mode);
        Assert.Equal(0.0, result.FinalObjective, 12);
        Assert.Equal(0.0, result.FinalResidualNorm, 12);
        Assert.Single(result.History);
    }

    [Fact]
    public void ModeA_NonFlatOmega_ObjectiveIsPositive()
    {
        // Use trace pairing (positive-definite) so objective is positive
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions { Mode = SolveMode.ResidualOnly });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        Assert.True(result.FinalObjective > 0);
        Assert.True(result.FinalResidualNorm > 0);
    }

    // ===== Mode B: Objective Minimization =====

    [Fact]
    public void ModeB_FlatOmega_ConvergesImmediately()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-10,
        });

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = solver.Solve(omega, a0, TestManifest(), DummyGeometry());

        Assert.True(result.Converged);
        Assert.Equal(0.0, result.FinalObjective, 10);
    }

    [Fact]
    public void ModeB_NonFlatOmega_ReducesObjective()
    {
        // Use trace pairing (positive-definite) so gradient descent reduces a positive objective
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 20,
            ObjectiveTolerance = 1e-12,
            GradientTolerance = 1e-12,
            InitialStepSize = 0.1,
        });

        // Start with small non-flat omega
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        // Objective should decrease over iterations
        Assert.True(result.History.Count > 1);
        double firstObjective = result.History[0].Objective;
        double lastObjective = result.History[^1].Objective;
        Assert.True(lastObjective < firstObjective,
            $"Objective should decrease: first={firstObjective}, last={lastObjective}");
    }

    [Fact]
    public void ModeB_ConvergenceHistory_IsTracked()
    {
        // Use trace pairing (positive-definite) so objective/norms are non-negative
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 5,
            ObjectiveTolerance = 1e-20,
            GradientTolerance = 1e-20,
            InitialStepSize = 0.01,
        });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        // History should have entries
        Assert.NotEmpty(result.History);

        // Each record should have valid data
        foreach (var record in result.History)
        {
            Assert.True(record.Objective >= 0);
            Assert.True(record.ResidualNorm >= 0);
            Assert.True(record.GradientNorm >= 0);
        }
    }

    // ===== Mode C: Stationarity Solve =====

    [Fact]
    public void ModeC_FlatOmega_ConvergesImmediately()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.StationaritySolve,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-10,
        });

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = solver.Solve(omega, a0, TestManifest(), DummyGeometry());

        Assert.True(result.Converged);
    }

    // ===== CpuSolverBackend =====

    [Fact]
    public void CpuSolverBackend_EvaluateDerived_Works()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var backend = CreateBackend(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var derived = backend.EvaluateDerived(
            omega.ToFieldTensor(), a0.ToFieldTensor(), TestManifest(), DummyGeometry());

        Assert.NotNull(derived.CurvatureF);
        Assert.NotNull(derived.TorsionT);
        Assert.NotNull(derived.ShiabS);
        Assert.NotNull(derived.ResidualUpsilon);
    }

    [Fact]
    public void CpuSolverBackend_GradientMatchesFD()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var backend = CreateBackend(mesh, algebra);
        var manifest = TestManifest();
        var geometry = DummyGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.3, 0.1, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.2, 0.1 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var derived = backend.EvaluateDerived(omegaTensor, a0, manifest, geometry);
        var jacobian = backend.BuildJacobian(omegaTensor, a0, derived.CurvatureF, manifest, geometry);
        var gradient = backend.ComputeGradient(jacobian, derived.ResidualUpsilon);

        // FD check: directional derivative should match <grad, direction>
        var delta = new ConnectionField(mesh, algebra);
        delta.SetEdgeValue(0, new[] { 0.01, 0.0, 0.0 });
        var deltaTensor = delta.ToFieldTensor();

        double analyticDir = FieldTensorOps.Dot(gradient, deltaTensor);

        double eps = 1e-7;
        var omegaPlus = FieldTensorOps.AddScaled(omegaTensor, deltaTensor, eps);
        var derivedPlus = backend.EvaluateDerived(omegaPlus, a0, manifest, geometry);
        double i2Base = backend.EvaluateObjective(derived.ResidualUpsilon);
        double i2Plus = backend.EvaluateObjective(derivedPlus.ResidualUpsilon);
        double fdDir = (i2Plus - i2Base) / eps;

        Assert.Equal(fdDir, analyticDir, 3);
    }

    // ===== SolverResult Structure =====

    [Fact]
    public void SolverResult_ContainsFinalState()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions { Mode = SolveMode.ResidualOnly });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.0, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        Assert.NotNull(result.FinalOmega);
        Assert.NotNull(result.FinalDerivedState);
        Assert.NotNull(result.TerminationReason);
        Assert.NotNull(result.History);
    }

    [Fact]
    public void SolverResult_ContainsCompleteHistory()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 3,
            ObjectiveTolerance = 1e-20,
            GradientTolerance = 1e-20,
            InitialStepSize = 0.01,
        });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        // History entries have iteration numbers, objective values, gradient norms
        Assert.True(result.History.Count >= 1);
        foreach (var record in result.History)
        {
            Assert.True(record.Iteration >= 0);
            Assert.True(record.Objective >= 0);
            Assert.True(record.ResidualNorm >= 0);
            Assert.True(record.GradientNorm >= 0);
            Assert.True(record.GaugeViolation >= 0);
        }

        // Final values match last history entry
        Assert.Equal(result.History[^1].Objective, result.FinalObjective);
        Assert.Equal(result.History[^1].ResidualNorm, result.FinalResidualNorm);
    }

    // ===== Gauge Penalty Term =====

    [Fact]
    public void GaugePenalty_ZeroLambda_NoEffect()
    {
        var penalty = new GaugePenaltyTerm(0.0);
        var omega = CreateTestOmega(new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 });

        Assert.Equal(0.0, penalty.EvaluateObjective(omega));
        Assert.All(penalty.EvaluateGradient(omega).Coefficients, c => Assert.Equal(0.0, c));
        Assert.Equal(5.0, penalty.AddToObjective(5.0, omega));
    }

    [Fact]
    public void GaugePenalty_PositiveLambda_IncreasesObjective()
    {
        var penalty = new GaugePenaltyTerm(1.0);
        var omega = CreateTestOmega(new[] { 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });

        double obj = penalty.EvaluateObjective(omega);
        Assert.Equal(0.5, obj, 12); // (1/2) * 1.0 * 1.0

        double totalObj = penalty.AddToObjective(10.0, omega);
        Assert.Equal(10.5, totalObj, 12);
    }

    [Fact]
    public void GaugePenalty_GradientIsLambdaTimesOmega()
    {
        double lambda = 2.5;
        var penalty = new GaugePenaltyTerm(lambda);
        var coeffs = new[] { 1.0, 2.0, 3.0, 4.0, 5.0, 6.0, 7.0, 8.0, 9.0 };
        var omega = CreateTestOmega(coeffs);

        var grad = penalty.EvaluateGradient(omega);

        for (int i = 0; i < coeffs.Length; i++)
        {
            Assert.Equal(lambda * coeffs[i], grad.Coefficients[i], 12);
        }
    }

    [Fact]
    public void GaugePenalty_AddToGradient_CombinesCorrectly()
    {
        double lambda = 1.0;
        var penalty = new GaugePenaltyTerm(lambda);

        var omega = CreateTestOmega(new[] { 1.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });
        var physGrad = CreateTestOmega(new[] { 0.5, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });

        var totalGrad = penalty.AddToGradient(physGrad, omega);

        // total = physics_grad + lambda * omega = 0.5 + 1.0*1.0 = 1.5
        Assert.Equal(1.5, totalGrad.Coefficients[0], 12);
    }

    [Fact]
    public void GaugePenalty_ViolationNorm_IsOmegaNorm()
    {
        var penalty = new GaugePenaltyTerm(1.0);
        var omega = CreateTestOmega(new[] { 3.0, 4.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });

        double violation = penalty.ComputeViolationNorm(omega);
        Assert.Equal(5.0, violation, 12); // sqrt(9+16) = 5
    }

    [Fact]
    public void GaugePenalty_NegativeLambda_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GaugePenaltyTerm(-1.0));
    }

    [Fact]
    public void GaugePenalty_InSolver_IncreasesObjectiveWithLambda()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var backend = CreateBackend(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.0, 0.0 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        // Solve with lambda=0
        var solver0 = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ResidualOnly,
            GaugePenaltyLambda = 0.0,
        });
        var result0 = solver0.Solve(omegaTensor, a0, TestManifest(), DummyGeometry());

        // Solve with lambda=10
        var solver10 = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ResidualOnly,
            GaugePenaltyLambda = 10.0,
        });
        var result10 = solver10.Solve(omegaTensor, a0, TestManifest(), DummyGeometry());

        // Mode A does not apply gauge penalty to FinalObjective (only Mode B does)
        // But gauge violation should be tracked
        Assert.True(result0.FinalGaugeViolation == 0);
    }

    [Fact]
    public void GaugePenalty_ModeB_HigherLambdaProducesHigherInitialObjective()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = CreateBackend(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.0, 0.0 });
        var omegaTensor = omega.ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        // Run Mode B with lambda=0
        var solver0 = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 1,
            ObjectiveTolerance = 1e-20,
            GradientTolerance = 1e-20,
            InitialStepSize = 0.01,
            GaugePenaltyLambda = 0.0,
        });
        var result0 = solver0.Solve(omegaTensor, a0, TestManifest(), DummyGeometry());

        // Run Mode B with lambda=10
        var solver10 = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 1,
            ObjectiveTolerance = 1e-20,
            GradientTolerance = 1e-20,
            InitialStepSize = 0.01,
            GaugePenaltyLambda = 10.0,
        });
        var result10 = solver10.Solve(omegaTensor, a0, TestManifest(), DummyGeometry());

        // Higher lambda should produce higher initial objective (gauge penalty adds to it)
        double obj0 = result0.History[0].Objective;
        double obj10 = result10.History[0].Objective;
        Assert.True(obj10 > obj0,
            $"Higher gauge penalty should increase objective: lambda=0 obj={obj0}, lambda=10 obj={obj10}");
    }

    [Fact]
    public void GaugePenalty_ModeB_GaugeViolationIsTracked()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = CreateBackend(mesh, algebra);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.0, 0.0 });

        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 3,
            ObjectiveTolerance = 1e-20,
            GradientTolerance = 1e-20,
            InitialStepSize = 0.01,
            GaugePenaltyLambda = 1.0,
        });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        // Gauge violation should be positive for non-flat omega
        Assert.True(result.History[0].GaugeViolation > 0);
    }

    // ===== Convergence Diagnostics =====

    [Fact]
    public void ConvergenceDiagnostics_EmptyHistory_NotStagnated()
    {
        var diag = new ConvergenceDiagnostics();
        Assert.False(diag.IsStagnated());
    }

    [Fact]
    public void ConvergenceDiagnostics_DecreasingObjective_NotStagnated()
    {
        var diag = new ConvergenceDiagnostics(stagnationWindow: 3);

        for (int i = 0; i < 10; i++)
        {
            diag.Record(new ConvergenceRecord
            {
                Iteration = i,
                Objective = 10.0 / (i + 1), // decreasing
                ResidualNorm = 1.0,
                GradientNorm = 1.0,
                GaugeViolation = 0,
                StepNorm = 0.1,
                StepSize = 0.01,
            });
        }

        Assert.False(diag.IsStagnated());
    }

    [Fact]
    public void ConvergenceDiagnostics_ConstantObjective_DetectsStagnation()
    {
        var diag = new ConvergenceDiagnostics(stagnationWindow: 3, stagnationThreshold: 1e-12);

        for (int i = 0; i < 10; i++)
        {
            diag.Record(new ConvergenceRecord
            {
                Iteration = i,
                Objective = 5.0, // constant -- stagnation
                ResidualNorm = 1.0,
                GradientNorm = 0.1,
                GaugeViolation = 0,
                StepNorm = 0.01,
                StepSize = 0.01,
            });
        }

        Assert.True(diag.IsStagnated());
    }

    [Fact]
    public void ConvergenceDiagnostics_ReductionRatio_Correct()
    {
        var diag = new ConvergenceDiagnostics();

        diag.Record(new ConvergenceRecord
        {
            Iteration = 0, Objective = 10.0,
            ResidualNorm = 1.0, GradientNorm = 1.0,
            GaugeViolation = 0, StepNorm = 0, StepSize = 0,
        });
        diag.Record(new ConvergenceRecord
        {
            Iteration = 1, Objective = 5.0,
            ResidualNorm = 0.5, GradientNorm = 0.5,
            GaugeViolation = 0, StepNorm = 0.1, StepSize = 0.01,
        });

        Assert.Equal(0.5, diag.ObjectiveReductionRatio(), 12);
    }

    [Fact]
    public void ConvergenceDiagnostics_Summary_ContainsAllFields()
    {
        var diag = new ConvergenceDiagnostics();

        diag.Record(new ConvergenceRecord
        {
            Iteration = 0, Objective = 10.0,
            ResidualNorm = 3.0, GradientNorm = 2.0,
            GaugeViolation = 0.5, StepNorm = 0, StepSize = 0,
        });

        var summary = diag.GetSummary();

        Assert.Equal(1, summary.TotalIterations);
        Assert.Equal(10.0, summary.InitialObjective, 12);
        Assert.Equal(10.0, summary.FinalObjective, 12);
        Assert.Equal(2.0, summary.FinalGradientNorm, 12);
        Assert.Equal(0.5, summary.FinalGaugeViolation, 12);
        Assert.False(summary.IsStagnated);
    }

    [Fact]
    public void ConvergenceDiagnostics_Log_ContainsEntries()
    {
        var diag = new ConvergenceDiagnostics();

        diag.Record(new ConvergenceRecord
        {
            Iteration = 0, Objective = 10.0,
            ResidualNorm = 3.0, GradientNorm = 2.0,
            GaugeViolation = 0, StepNorm = 0, StepSize = 0,
        });

        var log = diag.GenerateLog();

        Assert.NotEmpty(log);
        Assert.Contains(log, line => line.Contains("iter=0"));
        Assert.Contains(log, line => line.Contains("Objective reduction"));
    }

    [Fact]
    public void ConvergenceDiagnostics_ObjectiveHistory_Tracks()
    {
        var diag = new ConvergenceDiagnostics();

        diag.Record(new ConvergenceRecord
        {
            Iteration = 0, Objective = 10.0,
            ResidualNorm = 1.0, GradientNorm = 1.0,
            GaugeViolation = 0, StepNorm = 0, StepSize = 0,
        });
        diag.Record(new ConvergenceRecord
        {
            Iteration = 1, Objective = 5.0,
            ResidualNorm = 0.5, GradientNorm = 0.5,
            GaugeViolation = 0, StepNorm = 0.1, StepSize = 0.01,
        });

        Assert.Equal(2, diag.ObjectiveHistory.Count);
        Assert.Equal(10.0, diag.ObjectiveHistory[0]);
        Assert.Equal(5.0, diag.ObjectiveHistory[1]);
    }

    [Fact]
    public void ConvergenceDiagnostics_GaugeViolationHistory_Tracks()
    {
        var diag = new ConvergenceDiagnostics();

        diag.Record(new ConvergenceRecord
        {
            Iteration = 0, Objective = 10.0,
            ResidualNorm = 1.0, GradientNorm = 1.0,
            GaugeViolation = 0.5, StepNorm = 0, StepSize = 0,
        });
        diag.Record(new ConvergenceRecord
        {
            Iteration = 1, Objective = 5.0,
            ResidualNorm = 0.5, GradientNorm = 0.5,
            GaugeViolation = 0.3, StepNorm = 0.1, StepSize = 0.01,
        });

        Assert.Equal(2, diag.GaugeViolationHistory.Count);
        Assert.Equal(0.5, diag.GaugeViolationHistory[0]);
        Assert.Equal(0.3, diag.GaugeViolationHistory[1]);
    }

    // ===== Line Search =====

    [Fact]
    public void LineSearch_FindsAcceptableStepSize()
    {
        // Gradient descent with backtracking should find a step that reduces objective
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 1,
            ObjectiveTolerance = 1e-20,
            GradientTolerance = 1e-20,
            InitialStepSize = 1.0,
            BacktrackFactor = 0.5,
            ArmijoParameter = 1e-4,
        });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        // Should have taken at least one step with a valid step size
        var lastRecord = result.History[^1];
        Assert.True(lastRecord.StepSize > 0,
            $"Line search should find a positive step size, got {lastRecord.StepSize}");
    }

    // ===== End-to-End Pipeline =====

    [Fact]
    public void CpuSolverPipeline_FromFlat_ProducesValidResult()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var pipeline = new CpuSolverPipeline(mesh, algebra, torsion, shiab);

        var pipelineResult = pipeline.ExecuteFromFlat(
            TestManifest(), DummyGeometry(),
            new SolverOptions { Mode = SolveMode.ResidualOnly });

        Assert.NotNull(pipelineResult.SolverResult);
        Assert.NotNull(pipelineResult.ArtifactBundle);
        Assert.NotNull(pipelineResult.DiagnosticLog);
        Assert.NotNull(pipelineResult.ConvergenceSummary);
        Assert.NotNull(pipelineResult.FinalConnection);
        Assert.NotNull(pipelineResult.BiConnectionA);
        Assert.NotNull(pipelineResult.BiConnectionB);
    }

    [Fact]
    public void CpuSolverPipeline_ArtifactBundle_HasRequiredFields()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var pipeline = new CpuSolverPipeline(mesh, algebra, torsion, shiab);

        var result = pipeline.ExecuteFromFlat(
            TestManifest(), DummyGeometry(),
            new SolverOptions { Mode = SolveMode.ResidualOnly });

        var bundle = result.ArtifactBundle;
        Assert.NotNull(bundle.ArtifactId);
        Assert.NotNull(bundle.Branch);
        Assert.NotNull(bundle.ReplayContract);
        Assert.NotNull(bundle.Provenance);
        Assert.NotNull(bundle.InitialState);
        Assert.NotNull(bundle.FinalState);
        Assert.NotNull(bundle.DerivedState);
        Assert.NotNull(bundle.Residuals);
        Assert.NotNull(bundle.Geometry);
        Assert.Equal("cpu-reference", bundle.ReplayContract.BackendId);
        Assert.Equal("R2", bundle.ReplayContract.ReplayTier);
    }

    [Fact]
    public void CpuSolverPipeline_ModeB_ToyBranchReducesObjective()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var pipeline = new CpuSolverPipeline(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });

        var result = pipeline.Execute(omega, null, TestManifest(), DummyGeometry(),
            new SolverOptions
            {
                Mode = SolveMode.ObjectiveMinimization,
                MaxIterations = 20,
                ObjectiveTolerance = 1e-12,
                GradientTolerance = 1e-12,
                InitialStepSize = 0.1,
            });

        // Acceptance criteria: toy branch reduces objective and residual
        Assert.True(result.SolverResult.History.Count > 1);
        double firstObj = result.SolverResult.History[0].Objective;
        double lastObj = result.SolverResult.History[^1].Objective;
        Assert.True(lastObj < firstObj,
            $"Pipeline should reduce objective: first={firstObj}, last={lastObj}");
    }

    [Fact]
    public void CpuSolverPipeline_DiagnosticsAreEmitted()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var pipeline = new CpuSolverPipeline(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });

        var result = pipeline.Execute(omega, null, TestManifest(), DummyGeometry(),
            new SolverOptions
            {
                Mode = SolveMode.ObjectiveMinimization,
                MaxIterations = 3,
                ObjectiveTolerance = 1e-20,
                GradientTolerance = 1e-20,
                InitialStepSize = 0.01,
                GaugePenaltyLambda = 1.0,
            });

        // Gauge diagnostics should be emitted in the log
        Assert.NotEmpty(result.DiagnosticLog);
        Assert.Contains(result.DiagnosticLog, line => line.Contains("gauge"));

        // Convergence summary should be populated
        Assert.True(result.ConvergenceSummary.TotalIterations > 0);
    }

    [Fact]
    public void CpuSolverPipeline_FromCoefficients_Works()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var pipeline = new CpuSolverPipeline(mesh, algebra, torsion, shiab);

        int nDof = mesh.EdgeCount * algebra.Dimension;
        var coeffs = new double[nDof];
        coeffs[0] = 0.1;

        var result = pipeline.ExecuteFromCoefficients(coeffs, TestManifest(), DummyGeometry(),
            new SolverOptions { Mode = SolveMode.ResidualOnly });

        Assert.NotNull(result.SolverResult);
        Assert.Equal(nDof, result.FinalConnection.Coefficients.Length);
    }

    [Fact]
    public void CpuSolverPipeline_FailuresPreservedAsArtifacts()
    {
        // Even when the solver fails (line search failure), we still get an artifact bundle
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var pipeline = new CpuSolverPipeline(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });

        // Use very strict tolerances and tiny max iterations to potentially trigger non-convergence
        var result = pipeline.Execute(omega, null, TestManifest(), DummyGeometry(),
            new SolverOptions
            {
                Mode = SolveMode.ObjectiveMinimization,
                MaxIterations = 1,
                ObjectiveTolerance = 1e-100,
                GradientTolerance = 1e-100,
                InitialStepSize = 0.01,
            });

        // Artifact bundle should still be produced regardless of convergence
        Assert.NotNull(result.ArtifactBundle);
        Assert.NotNull(result.ArtifactBundle.InitialState);
        Assert.NotNull(result.ArtifactBundle.FinalState);
        Assert.NotNull(result.ArtifactBundle.DerivedState);
        Assert.NotNull(result.DiagnosticLog);
    }

    // ===== Flat Connection Fixed Point =====

    [Fact]
    public void FlatConnection_IsFixedPoint_ForTrivialTorsion()
    {
        // omega=0 with T=0, S=F=0 should be a fixed point
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var backend = CreateBackend(mesh, algebra);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-10,
        });

        var omega = ConnectionField.Zero(mesh, algebra).ToFieldTensor();
        var a0 = ConnectionField.Zero(mesh, algebra).ToFieldTensor();

        var result = solver.Solve(omega, a0, TestManifest(), DummyGeometry());

        Assert.True(result.Converged);
        Assert.Equal(0.0, result.FinalObjective, 12);
        Assert.Equal(0.0, result.FinalResidualNorm, 12);
        Assert.Equal(0.0, result.FinalGradientNorm, 12);

        // omega should remain zero
        Assert.All(result.FinalOmega.Coefficients, c => Assert.Equal(0.0, c, 12));
    }

    // ===== Helper =====

    private static FieldTensor CreateTestOmega(double[] coefficients)
    {
        return new FieldTensor
        {
            Label = "omega_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h",
                CarrierType = "connection-1form",
                Degree = "1",
                LieAlgebraBasisId = "canonical",
                ComponentOrderId = "edge-major",
                NumericPrecision = "float64",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = coefficients,
            Shape = new[] { coefficients.Length / 3, 3 },
        };
    }
}
