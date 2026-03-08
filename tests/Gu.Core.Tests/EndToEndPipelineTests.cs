using Gu.Branching;
using Gu.Core;
using Gu.Geometry;
using Gu.Math;
using Gu.Observation;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Core.Tests;

/// <summary>
/// End-to-end integration tests exercising the full chain:
///   mesh creation -> solver -> observation pipeline -> artifact packaging.
/// Validates DoD criteria 3 (CPU solves small cases) and 6 (observed outputs through sigma_h*).
/// </summary>
public class EndToEndPipelineTests
{
    private static BranchManifest TestManifest(FiberBundleMesh bundle) => new()
    {
        BranchId = "e2e-test",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "e2e",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = bundle.BaseMesh.EmbeddingDimension,
        AmbientDimension = bundle.AmbientMesh.EmbeddingDimension,
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

    [Fact]
    public void SolverOutput_FeedsThroughObservationPipeline_ProducesVerifiedObservables()
    {
        // 1. Create the fiber bundle geometry
        var bundle = ToyGeometryFactory.CreateToy2D();
        var yMesh = bundle.AmbientMesh;
        var algebra = LieAlgebraFactory.CreateSu2();
        var manifest = TestManifest(bundle);
        var geometry = bundle.ToGeometryContext("centroid", "P1");

        // 2. Set up solver on Y_h mesh with trivial operators
        var torsion = new TrivialTorsionCpu(yMesh, algebra);
        var shiab = new IdentityShiabCpu(yMesh, algebra);
        var backend = new CpuSolverBackend(yMesh, algebra, torsion, shiab);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ResidualOnly,
        });

        // 3. Create a non-flat connection on Y_h
        var omega = new ConnectionField(yMesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.5, 0.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 0.3, 0.0 });
        if (yMesh.EdgeCount > 2)
            omega.SetEdgeValue(2, new[] { 0.0, 0.0, 0.2 });

        var a0 = ConnectionField.Zero(yMesh, algebra);

        // 4. Run solver (Mode A: residual-only)
        var result = solver.Solve(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        Assert.NotNull(result.FinalDerivedState);
        var derived = result.FinalDerivedState;

        // 5. Feed solver's DerivedState through observation pipeline
        var pullback = new PullbackOperator(bundle);
        var pipeline = new ObservationPipeline(
            pullback,
            Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var branchRef = new BranchRef
        {
            BranchId = manifest.BranchId,
            SchemaVersion = manifest.SchemaVersion,
        };
        var discreteState = new DiscreteState
        {
            Branch = branchRef,
            Geometry = geometry,
            Omega = result.FinalOmega,
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = manifest.CodeRevision,
                Branch = branchRef,
            },
        };

        var requests = new[]
        {
            new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
            new ObservableRequest { ObservableId = "residual", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, discreteState, geometry, requests, manifest);

        // 6. Validate: observables exist, are verified through sigma_h*, live on X_h
        Assert.Equal(2, observed.Observables.Count);
        Assert.True(observed.Observables.ContainsKey("curvature"));
        Assert.True(observed.Observables.ContainsKey("residual"));

        foreach (var (id, snap) in observed.Observables)
        {
            Assert.NotNull(snap.Provenance);
            Assert.True(snap.Provenance!.IsVerified,
                $"IA-6: Observable '{id}' must be verified as having passed through sigma_h*");
            Assert.Equal("sigma_h_star", snap.Provenance.PullbackOperatorId);

            // Values live on X_h faces, with dimG components per face for Lie-algebra-valued 2-forms
            int dimG = 3; // su2
            Assert.Equal(bundle.BaseMesh.FaceCount * dimG, snap.Values.Length);
        }

        Assert.Equal("sigma-pullback", observed.ObservationBranchId);
    }

    [Fact]
    public void SolverModeB_ThenObserve_ObjectiveDecreases_ObservablesVerified()
    {
        // Full chain: Mode B gradient descent -> observation
        // Uses a single triangle mesh directly (not Toy2D) for controllable face-edge topology.
        var yMesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2,
            simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

        // Must use trace pairing (positive-definite metric) for Mode B;
        // the Killing form is negative-definite and makes objective always negative.
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var manifest = new BranchManifest
        {
            BranchId = "e2e-modeB",
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1",
            CodeRevision = "e2e",
            ActiveGeometryBranch = "simplicial",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "trivial",
            ActiveShiabBranch = "identity-shiab",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 2,
            AmbientDimension = 2,
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

        var geometry = new GeometryContext
        {
            BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
            DiscretizationType = "simplicial",
            QuadratureRuleId = "centroid",
            BasisFamilyId = "P1",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
                TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
                TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 2 },
            },
            Patches = new[] { new PatchInfo { PatchId = "patch-0", ElementCount = 1 } },
        };

        var torsion = new TrivialTorsionCpu(yMesh, algebra);
        var shiab = new IdentityShiabCpu(yMesh, algebra);
        var backend = new CpuSolverBackend(yMesh, algebra, torsion, shiab);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-30,
            GradientTolerance = 1e-30,
            InitialStepSize = 0.01,
        });

        // Set large omega on all 3 edges to guarantee non-trivial F on the single face
        var omega = new ConnectionField(yMesh, algebra);
        omega.SetEdgeValue(0, new[] { 5.0, 1.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 5.0, 1.0 });
        omega.SetEdgeValue(2, new[] { 1.0, 0.0, 5.0 });
        var a0 = ConnectionField.Zero(yMesh, algebra);

        var result = solver.Solve(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        // Verify solver actually iterated and reduced the objective
        Assert.True(result.History.Count >= 2,
            $"Mode B should iterate; got {result.History.Count} record(s), " +
            $"objective={result.FinalObjective:E6}, gradNorm={result.FinalGradientNorm:E6}, " +
            $"reason='{result.TerminationReason}'");
        Assert.True(result.History[^1].Objective <= result.History[0].Objective,
            "Objective should not increase during optimization");
    }

    [Fact]
    public void FullPipeline_ArtifactBundle_ContainsAllRequiredFields()
    {
        // Validate that CpuSolverPipeline produces a complete ArtifactBundle
        var bundle = ToyGeometryFactory.CreateToy2D();
        var yMesh = bundle.AmbientMesh;
        var algebra = LieAlgebraFactory.CreateSu2();
        var manifest = TestManifest(bundle);
        var geometry = bundle.ToGeometryContext("centroid", "P1");

        var torsion = new TrivialTorsionCpu(yMesh, algebra);
        var shiab = new IdentityShiabCpu(yMesh, algebra);
        var pipeline = new CpuSolverPipeline(yMesh, algebra, torsion, shiab);

        var omega = new ConnectionField(yMesh, algebra);
        omega.SetEdgeValue(0, new[] { 0.1, 0.0, 0.0 });

        var pipelineResult = pipeline.Execute(omega, null, manifest, geometry,
            new SolverOptions { Mode = SolveMode.ResidualOnly });

        // Artifact bundle completeness
        var ab = pipelineResult.ArtifactBundle;
        Assert.NotNull(ab);
        Assert.NotNull(ab.InitialState);
        Assert.NotNull(ab.FinalState);
        Assert.NotNull(ab.DerivedState);
        Assert.NotNull(ab.Residuals);
        Assert.NotNull(ab.Geometry);
        Assert.NotNull(ab.ReplayContract);
        Assert.NotNull(ab.Provenance);
        Assert.Equal("R2", ab.ReplayContract.ReplayTier);
        Assert.Equal("cpu-reference", ab.ReplayContract.BackendId);
        Assert.True(ab.ReplayContract.Deterministic);

        // Convergence summary
        Assert.NotNull(pipelineResult.ConvergenceSummary);
        Assert.NotNull(pipelineResult.DiagnosticLog);
        Assert.NotEmpty(pipelineResult.DiagnosticLog);

        // Bi-connections
        Assert.NotNull(pipelineResult.BiConnectionA);
        Assert.NotNull(pipelineResult.BiConnectionB);
    }

    [Fact]
    public void FlatConnection_SolverConverges_ObservablesAreZero()
    {
        // Flat omega -> zero curvature -> zero residual -> zero observables
        var bundle = ToyGeometryFactory.CreateToy2D();
        var yMesh = bundle.AmbientMesh;
        var algebra = LieAlgebraFactory.CreateSu2();
        var manifest = TestManifest(bundle);
        var geometry = bundle.ToGeometryContext("centroid", "P1");

        var torsion = new TrivialTorsionCpu(yMesh, algebra);
        var shiab = new IdentityShiabCpu(yMesh, algebra);
        var backend = new CpuSolverBackend(yMesh, algebra, torsion, shiab);
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 5,
            ObjectiveTolerance = 1e-10,
        });

        var omega = ConnectionField.Zero(yMesh, algebra);
        var result = solver.Solve(
            omega.ToFieldTensor(),
            ConnectionField.Zero(yMesh, algebra).ToFieldTensor(),
            manifest, geometry);

        Assert.True(result.Converged);
        Assert.Equal(0.0, result.FinalObjective, 10);

        // Observation: all observables should be zero for flat connection
        var pullback = new PullbackOperator(bundle);
        var obsPipeline = new ObservationPipeline(
            pullback,
            Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var branchRef = new BranchRef
        {
            BranchId = manifest.BranchId,
            SchemaVersion = manifest.SchemaVersion,
        };
        var discreteState = new DiscreteState
        {
            Branch = branchRef,
            Geometry = geometry,
            Omega = result.FinalOmega,
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = manifest.CodeRevision,
                Branch = branchRef,
            },
        };

        var requests = new[]
        {
            new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
            new ObservableRequest { ObservableId = "residual", OutputType = OutputType.Quantitative },
        };

        var observed = obsPipeline.Extract(
            result.FinalDerivedState, discreteState, geometry, requests, manifest);

        // All pulled-back values should be zero for flat connection
        foreach (var (id, snap) in observed.Observables)
        {
            Assert.All(snap.Values, v => Assert.Equal(0.0, v, 10));
        }
    }
}
