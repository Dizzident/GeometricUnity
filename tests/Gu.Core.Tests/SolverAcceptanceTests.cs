using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Core.Tests;

/// <summary>
/// Acceptance tests for the solver (Milestone 6) and the critical M5 criterion:
/// "I2_h drives Upsilon_h down" -- gradient descent reduces the objective.
/// Tests both operator families: Trivial+Identity and LocalAlgebraic+FirstOrder.
/// </summary>
public class SolverAcceptanceTests
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
        ActiveTorsionBranch = "local-algebraic",
        ActiveShiabBranch = "first-order-curvature",
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

    // ===== M5 Acceptance: I2_h drives Upsilon_h down =====

    [Fact]
    public void TrivialOperators_ModeB_ObjectiveDecreases()
    {
        // Use trace pairing (positive-definite) so gradient descent reduces a positive objective
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 20,
            ObjectiveTolerance = 1e-12,
            GradientTolerance = 1e-12,
            InitialStepSize = 0.1,
        });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        Assert.True(result.History.Count >= 2, "Should take at least 2 iterations");
        Assert.True(result.History[^1].Objective < result.History[0].Objective,
            $"Objective should decrease: first={result.History[0].Objective:E6}, last={result.History[^1].Objective:E6}");
    }

    [Fact]
    public void NonTrivialOperators_ModeB_ObjectiveDecreases()
    {
        // Use trace pairing (positive-definite) so gradient descent reduces a positive objective
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 20,
            ObjectiveTolerance = 1e-12,
            GradientTolerance = 1e-12,
            InitialStepSize = 0.01,
        });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.1, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        Assert.True(result.History.Count >= 2, "Should take at least 2 iterations");
        Assert.True(result.History[^1].Objective < result.History[0].Objective,
            $"Objective should decrease: first={result.History[0].Objective:E6}, last={result.History[^1].Objective:E6}");
    }

    // ===== M6 Acceptance: Solver modes =====

    [Fact]
    public void NonTrivialOperators_ModeA_ReturnsValidResult()
    {
        // Use trace pairing (positive-definite) so objective is positive for non-flat connections
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);
        var solver = new SolverOrchestrator(backend, new SolverOptions { Mode = SolveMode.ResidualOnly });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        Assert.Equal(SolveMode.ResidualOnly, result.Mode);
        Assert.True(result.FinalObjective > 0);
        Assert.NotNull(result.FinalDerivedState);
        Assert.Single(result.History);
    }

    [Fact]
    public void NonTrivialOperators_ModeC_FlatOmega_ConvergesImmediately()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2();
        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);
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
        Assert.Equal(0.0, result.FinalObjective, 10);
    }

    // ===== Convergence history monotonicity =====

    [Fact]
    public void ModeB_ObjectiveHistory_IsMonotonicallyDecreasing()
    {
        // Use trace pairing (positive-definite) so gradient descent produces decreasing objectives
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-20,
            GradientTolerance = 1e-20,
            InitialStepSize = 0.01,
        });

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.2, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.15, 0.0 });

        var result = solver.Solve(omega.ToFieldTensor(), ConnectionField.Zero(mesh, algebra).ToFieldTensor(),
            TestManifest(), DummyGeometry());

        // With Armijo backtracking, objective should be monotonically decreasing
        for (int i = 1; i < result.History.Count; i++)
        {
            Assert.True(result.History[i].Objective <= result.History[i - 1].Objective + 1e-14,
                $"Objective should decrease: iter {i - 1}={result.History[i - 1].Objective:E6}, iter {i}={result.History[i].Objective:E6}");
        }
    }

    // ===== SolverOptions validation =====

    [Fact]
    public void SolverOptions_AllModesExist()
    {
        var modes = Enum.GetValues<SolveMode>();
        Assert.Equal(3, modes.Length);
        Assert.Contains(SolveMode.ResidualOnly, modes);
        Assert.Contains(SolveMode.ObjectiveMinimization, modes);
        Assert.Contains(SolveMode.StationaritySolve, modes);
    }
}
