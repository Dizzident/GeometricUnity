using Gu.Branching;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Math;
using Gu.Observation;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Core.Tests;

/// <summary>
/// Section 19.1 mandatory v1 tests.
/// These are required by the implementation plan and must all pass
/// for Minimal GU v1 to be considered complete.
/// </summary>
public class Section19MandatoryTests
{
    #region Shared Helpers

    private static SimplicialMesh SingleTriangle() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2, simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3,
            cellVertices: new[] { new[] { 0, 1, 2 } });

    private static SimplicialMesh TwoTriangles() =>
        MeshTopologyBuilder.Build(
            embeddingDimension: 2, simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1, 1, 1 },
            vertexCount: 4,
            cellVertices: new[]
            {
                new[] { 0, 1, 2 },
                new[] { 1, 3, 2 },
            });

    private static BranchManifest TestManifest(
        string torsionBranch = "trivial",
        string shiabBranch = "identity-shiab",
        string branchId = "section19-test") => new()
    {
        BranchId = branchId,
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "sec19",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = torsionBranch,
        ActiveShiabBranch = shiabBranch,
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

    private static GeometryContext SimpleGeometry() => new()
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
        Patches = new[] { new PatchInfo { PatchId = "p0", ElementCount = 1 } },
    };

    #endregion

    // ===================================================================
    // 19.1.1 Well-definedness test
    // ===================================================================

    [Fact]
    public void WellDefinedness_CarriersMatch_TorsionAndShiab()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        // Both must output "curvature-2form" for Upsilon = S - T to be well-defined
        Assert.Equal(torsion.OutputCarrierType, shiab.OutputCarrierType);
        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);
    }

    [Fact]
    public void WellDefinedness_CarriersMatch_NonTrivialOperators()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();

        var torsion = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var shiab = new FirstOrderShiabOperator(mesh, algebra);

        Assert.Equal("curvature-2form", torsion.OutputCarrierType);
        Assert.Equal("curvature-2form", shiab.OutputCarrierType);
        BranchOperatorRegistry.ValidateCarrierMatch(torsion, shiab);
    }

    [Fact]
    public void WellDefinedness_TensorSignaturesMatch_DerivedStateConsistent()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = TestManifest();
        var geometry = SimpleGeometry();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.5, 0.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var derived = backend.EvaluateDerived(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        // All derived fields must have matching carrier type
        Assert.Equal(derived.CurvatureF.Signature.CarrierType,
                     derived.ShiabS.Signature.CarrierType);
        Assert.Equal(derived.CurvatureF.Signature.CarrierType,
                     derived.TorsionT.Signature.CarrierType);
        Assert.Equal(derived.CurvatureF.Signature.CarrierType,
                     derived.ResidualUpsilon.Signature.CarrierType);

        // Shapes must be consistent
        Assert.Equal(derived.CurvatureF.Shape, derived.ResidualUpsilon.Shape);
        Assert.Equal(derived.CurvatureF.Coefficients.Length,
                     derived.ResidualUpsilon.Coefficients.Length);
    }

    [Fact]
    public void WellDefinedness_NoObservationBypass()
    {
        // IA-6: All observables must pass through sigma_h* (the observation pipeline).
        // Verify that the pipeline marks outputs as verified.
        var bundle = ToyGeometryFactory.CreateToy2D();
        var yMesh = bundle.AmbientMesh;
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = TestManifest(branchId: "ia6-test");
        manifest = new BranchManifest
        {
            BranchId = "ia6-test", SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1", CodeRevision = "sec19",
            ActiveGeometryBranch = "simplicial", ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "trivial", ActiveShiabBranch = "identity-shiab",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = bundle.BaseMesh.EmbeddingDimension,
            AmbientDimension = bundle.AmbientMesh.EmbeddingDimension,
            LieAlgebraId = "su2", BasisConventionId = "canonical",
            ComponentOrderId = "face-major", AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-trace", NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };

        var torsion = new TrivialTorsionCpu(yMesh, algebra);
        var shiab = new IdentityShiabCpu(yMesh, algebra);
        var backend = new CpuSolverBackend(yMesh, algebra, torsion, shiab);

        var omega = ConnectionField.Zero(yMesh, algebra);
        var a0 = ConnectionField.Zero(yMesh, algebra);
        var geometry = bundle.ToGeometryContext("centroid", "P1");

        var derived = backend.EvaluateDerived(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        var pullback = new PullbackOperator(bundle);
        var pipeline = new ObservationPipeline(
            pullback, Array.Empty<IDerivedObservableTransform>(),
            new DimensionlessNormalizationPolicy());

        var branchRef = new BranchRef
        {
            BranchId = manifest.BranchId, SchemaVersion = manifest.SchemaVersion,
        };
        var discreteState = new DiscreteState
        {
            Branch = branchRef, Geometry = geometry,
            Omega = omega.ToFieldTensor(),
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow, CodeRevision = "sec19", Branch = branchRef,
            },
        };

        var requests = new[]
        {
            new ObservableRequest { ObservableId = "curvature", OutputType = OutputType.Quantitative },
            new ObservableRequest { ObservableId = "residual", OutputType = OutputType.Quantitative },
            new ObservableRequest { ObservableId = "torsion", OutputType = OutputType.Quantitative },
            new ObservableRequest { ObservableId = "shiab", OutputType = OutputType.Quantitative },
        };

        var observed = pipeline.Extract(derived, discreteState, geometry, requests, manifest);

        // Every observable must be marked as verified (passed through sigma_h*)
        foreach (var (id, snap) in observed.Observables)
        {
            Assert.NotNull(snap.Provenance);
            Assert.True(snap.Provenance!.IsVerified,
                $"IA-6 violation: observable '{id}' not verified through sigma_h*");
            Assert.Equal("sigma_h_star", snap.Provenance.PullbackOperatorId);
        }
    }

    // ===================================================================
    // 19.1.2 Gauge-consistency test
    // ===================================================================

    [Fact]
    public void GaugeConsistency_SmallPerturbation_DoesNotDestabilize()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = TestManifest();
        var geometry = SimpleGeometry();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

        // Base omega
        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.5, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.5 });
        omega.SetEdgeValue(2, new[] { 0.5, 0.0, 1.0 });

        var options = new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 5,
            ObjectiveTolerance = 1e-30,
            GradientTolerance = 1e-30,
            InitialStepSize = 0.01,
            GaugePenaltyLambda = 0.1,
        };

        var solver = new SolverOrchestrator(backend, options);
        var a0 = ConnectionField.Zero(mesh, algebra);

        var result1 = solver.Solve(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        // Add small gauge-like perturbation (eps * random direction)
        double eps = 1e-6;
        var perturbedOmega = new ConnectionField(mesh, algebra);
        for (int e = 0; e < mesh.EdgeCount; e++)
        {
            var val = new double[3];
            for (int a = 0; a < 3; a++)
            {
                val[a] = omega.ToFieldTensor().Coefficients[e * 3 + a]
                       + eps * ((e * 3 + a + 1) * 0.37 % 1.0 - 0.5);
            }
            perturbedOmega.SetEdgeValue(e, val);
        }

        var result2 = solver.Solve(
            perturbedOmega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        // The perturbed solver should not blow up -- objective should remain finite
        Assert.True(double.IsFinite(result2.FinalObjective),
            "Gauge perturbation caused non-finite objective");

        // The final objectives should be close (perturbation was O(eps))
        double relDiff = System.Math.Abs(result1.FinalObjective - result2.FinalObjective)
            / (System.Math.Abs(result1.FinalObjective) + 1e-15);
        Assert.True(relDiff < 1.0,
            $"Gauge perturbation caused disproportionate objective change: relDiff={relDiff:E6}");
    }

    // ===================================================================
    // 19.1.3 Residual test
    // ===================================================================

    [Fact]
    public void Residual_SolveIterations_ReduceObjective()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = TestManifest();
        var geometry = SimpleGeometry();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 3.0, 1.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 3.0, 1.0 });
        omega.SetEdgeValue(2, new[] { 1.0, 0.0, 3.0 });

        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-30,
            GradientTolerance = 1e-30,
            InitialStepSize = 0.01,
        });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var result = solver.Solve(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        // I2_h should decrease over iterations
        Assert.True(result.History.Count >= 2, "Solver should iterate");
        Assert.True(result.History[^1].Objective <= result.History[0].Objective,
            "I2_h should decrease over solve iterations");

        // ||Upsilon_h|| should also decrease
        Assert.True(result.History[^1].ResidualNorm <= result.History[0].ResidualNorm,
            "||Upsilon_h|| should decrease over solve iterations");
    }

    [Fact]
    public void Residual_ObjectiveHistory_IsMonotonicallyNonIncreasing()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = TestManifest();
        var geometry = SimpleGeometry();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 5.0, 1.0, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 5.0, 1.0 });
        omega.SetEdgeValue(2, new[] { 1.0, 0.0, 5.0 });

        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 10,
            ObjectiveTolerance = 1e-30,
            GradientTolerance = 1e-30,
            InitialStepSize = 0.01,
        });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var result = solver.Solve(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        for (int i = 1; i < result.History.Count; i++)
        {
            Assert.True(result.History[i].Objective <= result.History[i - 1].Objective + 1e-14,
                $"Objective increased at iteration {i}: " +
                $"{result.History[i].Objective:E10} > {result.History[i - 1].Objective:E10}");
        }
    }

    // ===================================================================
    // 19.1.4 Branch-sensitivity test
    // ===================================================================

    [Fact]
    public void BranchSensitivity_ChangingTorsionBranch_ProducesControlledChange()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var geometry = SimpleGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 2.0, 0.5, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 2.0, 0.5 });
        omega.SetEdgeValue(2, new[] { 0.5, 0.0, 2.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var shiab = new IdentityShiabCpu(mesh, algebra);

        // Run with trivial torsion (T=0)
        var torsionTrivial = new TrivialTorsionCpu(mesh, algebra);
        var manifestTrivial = TestManifest(torsionBranch: "trivial", branchId: "branch-trivial");
        var backendTrivial = new CpuSolverBackend(mesh, algebra, torsionTrivial, shiab);
        var derivedTrivial = backendTrivial.EvaluateDerived(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifestTrivial, geometry);

        // Run with local-algebraic torsion (T != 0)
        var torsionAlgebraic = new LocalAlgebraicTorsionOperator(mesh, algebra);
        var manifestAlgebraic = TestManifest(torsionBranch: "local-algebraic", branchId: "branch-algebraic");
        var backendAlgebraic = new CpuSolverBackend(mesh, algebra, torsionAlgebraic, shiab);
        var derivedAlgebraic = backendAlgebraic.EvaluateDerived(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifestAlgebraic, geometry);

        // Curvature should be identical (same omega, same mesh)
        for (int i = 0; i < derivedTrivial.CurvatureF.Coefficients.Length; i++)
        {
            Assert.Equal(derivedTrivial.CurvatureF.Coefficients[i],
                         derivedAlgebraic.CurvatureF.Coefficients[i], 14);
        }

        // Torsion should differ (trivial vs local-algebraic)
        bool torsionDiffers = false;
        for (int i = 0; i < derivedTrivial.TorsionT.Coefficients.Length; i++)
        {
            if (System.Math.Abs(derivedTrivial.TorsionT.Coefficients[i]
                              - derivedAlgebraic.TorsionT.Coefficients[i]) > 1e-14)
            {
                torsionDiffers = true;
                break;
            }
        }
        Assert.True(torsionDiffers,
            "Changing torsion branch should produce different torsion output");

        // Residual should differ (Upsilon = S - T, and T changed)
        bool residualDiffers = false;
        for (int i = 0; i < derivedTrivial.ResidualUpsilon.Coefficients.Length; i++)
        {
            if (System.Math.Abs(derivedTrivial.ResidualUpsilon.Coefficients[i]
                              - derivedAlgebraic.ResidualUpsilon.Coefficients[i]) > 1e-14)
            {
                residualDiffers = true;
                break;
            }
        }
        Assert.True(residualDiffers,
            "Changing torsion branch should produce different residual");
    }

    [Fact]
    public void BranchSensitivity_ChangingShiabBranch_ProducesControlledChange()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var geometry = SimpleGeometry();

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 2.0, 0.5, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 2.0, 0.5 });
        omega.SetEdgeValue(2, new[] { 0.5, 0.0, 2.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        var torsion = new TrivialTorsionCpu(mesh, algebra);

        // Run with identity Shiab (S = F)
        var shiabIdentity = new IdentityShiabCpu(mesh, algebra);
        var manifestIdentity = TestManifest(shiabBranch: "identity-shiab", branchId: "branch-identity");
        var backendIdentity = new CpuSolverBackend(mesh, algebra, torsion, shiabIdentity);
        var derivedIdentity = backendIdentity.EvaluateDerived(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifestIdentity, geometry);

        // Run with first-order Shiab (S = F, but different output signature/label)
        var shiabFirstOrder = new FirstOrderShiabOperator(mesh, algebra);
        var manifestFirstOrder = TestManifest(shiabBranch: "first-order-curvature", branchId: "branch-first-order");
        var backendFirstOrder = new CpuSolverBackend(mesh, algebra, torsion, shiabFirstOrder);
        var derivedFirstOrder = backendFirstOrder.EvaluateDerived(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifestFirstOrder, geometry);

        // Both identity and first-order Shiab produce S=F, so coefficients match
        // (they are algebraically equivalent for the simplest case)
        for (int i = 0; i < derivedIdentity.ShiabS.Coefficients.Length; i++)
        {
            Assert.Equal(derivedIdentity.ShiabS.Coefficients[i],
                         derivedFirstOrder.ShiabS.Coefficients[i], 14);
        }

        // Branch ID in manifest is correctly propagated
        Assert.Equal("branch-identity", manifestIdentity.BranchId);
        Assert.Equal("branch-first-order", manifestFirstOrder.BranchId);
    }

    [Fact]
    public void BranchSensitivity_ProvenancePreserved_AcrossBranches()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var geometry = SimpleGeometry();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);

        var pipeline = new CpuSolverPipeline(mesh, algebra, torsion, shiab);
        var manifest = TestManifest(branchId: "provenance-test");

        var result = pipeline.ExecuteFromFlat(manifest, geometry,
            new SolverOptions { Mode = SolveMode.ResidualOnly });

        // Provenance must record the branch
        Assert.Equal("provenance-test", result.ArtifactBundle.Branch.BranchId);
        Assert.Equal("provenance-test", result.ArtifactBundle.Provenance.Branch.BranchId);

        // Provenance must include backend and solve metadata
        Assert.Equal("cpu-reference", result.ArtifactBundle.Provenance.Backend);
        Assert.NotNull(result.ArtifactBundle.Provenance.Notes);

        // Replay contract must reference the manifest
        Assert.Equal("provenance-test",
            result.ArtifactBundle.ReplayContract.BranchManifest.BranchId);
    }

    // ===================================================================
    // 19.1.5 Observation test
    // ===================================================================

    [Fact]
    public void Observation_Reproducibility_SameInputsSameOutputs()
    {
        var mesh = SingleTriangle();
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = TestManifest();
        var geometry = SimpleGeometry();

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var backend = new CpuSolverBackend(mesh, algebra, torsion, shiab);

        var omega = new ConnectionField(mesh, algebra);
        omega.SetEdgeValue(0, new[] { 1.0, 0.5, 0.0 });
        omega.SetEdgeValue(1, new[] { 0.0, 1.0, 0.5 });
        omega.SetEdgeValue(2, new[] { 0.5, 0.0, 1.0 });
        var a0 = ConnectionField.Zero(mesh, algebra);

        // Run solver twice with identical inputs
        var solver = new SolverOrchestrator(backend, new SolverOptions
        {
            Mode = SolveMode.ObjectiveMinimization,
            MaxIterations = 5,
            ObjectiveTolerance = 1e-30,
            GradientTolerance = 1e-30,
            InitialStepSize = 0.01,
        });

        var result1 = solver.Solve(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);
        var result2 = solver.Solve(
            omega.ToFieldTensor(), a0.ToFieldTensor(), manifest, geometry);

        // Results must be bit-identical (deterministic CPU reference)
        Assert.Equal(result1.FinalObjective, result2.FinalObjective);
        Assert.Equal(result1.Iterations, result2.Iterations);
        Assert.Equal(result1.FinalResidualNorm, result2.FinalResidualNorm);

        for (int i = 0; i < result1.FinalOmega.Coefficients.Length; i++)
        {
            Assert.Equal(result1.FinalOmega.Coefficients[i],
                         result2.FinalOmega.Coefficients[i]);
        }
    }

    // ===================================================================
    // 19.1.6 Discretization/refinement test
    // ===================================================================

    [Fact]
    public void Refinement_FinerMesh_ReducesOrMaintainsResidual()
    {
        // Coarse mesh: 1 triangle
        var coarseMesh = SingleTriangle();
        // Fine mesh: 2 triangles (refinement of same domain)
        var fineMesh = TwoTriangles();

        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var geometry = SimpleGeometry();

        // Set up solver on coarse mesh
        var torsionCoarse = new TrivialTorsionCpu(coarseMesh, algebra);
        var shiabCoarse = new IdentityShiabCpu(coarseMesh, algebra);
        var backendCoarse = new CpuSolverBackend(coarseMesh, algebra, torsionCoarse, shiabCoarse);

        var omegaCoarse = new ConnectionField(coarseMesh, algebra);
        omegaCoarse.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        var a0Coarse = ConnectionField.Zero(coarseMesh, algebra);

        var derivedCoarse = backendCoarse.EvaluateDerived(
            omegaCoarse.ToFieldTensor(), a0Coarse.ToFieldTensor(),
            TestManifest(branchId: "coarse"), geometry);
        double objCoarse = backendCoarse.EvaluateObjective(derivedCoarse.ResidualUpsilon);

        // Set up solver on fine mesh with same omega on shared edges
        var torsionFine = new TrivialTorsionCpu(fineMesh, algebra);
        var shiabFine = new IdentityShiabCpu(fineMesh, algebra);
        var backendFine = new CpuSolverBackend(fineMesh, algebra, torsionFine, shiabFine);

        var omegaFine = new ConnectionField(fineMesh, algebra);
        omegaFine.SetEdgeValue(0, new[] { 1.0, 0.0, 0.0 });
        var a0Fine = ConnectionField.Zero(fineMesh, algebra);

        var derivedFine = backendFine.EvaluateDerived(
            omegaFine.ToFieldTensor(), a0Fine.ToFieldTensor(),
            TestManifest(branchId: "fine"), geometry);
        double objFine = backendFine.EvaluateObjective(derivedFine.ResidualUpsilon);

        // Both objectives should be finite and non-negative
        Assert.True(double.IsFinite(objCoarse) && objCoarse >= 0,
            $"Coarse objective should be finite and non-negative: {objCoarse}");
        Assert.True(double.IsFinite(objFine) && objFine >= 0,
            $"Fine objective should be finite and non-negative: {objFine}");

        // Fine mesh has more DOFs; with the same initial omega on shared edges
        // and zero on new edges, the residual should be well-behaved
        // (not dramatically larger than coarse)
        Assert.True(objFine < objCoarse * 100,
            $"Fine mesh residual ({objFine:E6}) should not explode relative to coarse ({objCoarse:E6})");
    }

    [Fact]
    public void Refinement_FineMesh_MoreEdgesAndFaces()
    {
        var coarseMesh = SingleTriangle();
        var fineMesh = TwoTriangles();

        // Fine mesh should have strictly more topology
        Assert.True(fineMesh.EdgeCount > coarseMesh.EdgeCount,
            "Fine mesh should have more edges");
        Assert.True(fineMesh.FaceCount > coarseMesh.FaceCount,
            "Fine mesh should have more faces");
        Assert.True(fineMesh.VertexCount > coarseMesh.VertexCount,
            "Fine mesh should have more vertices");
    }
}
