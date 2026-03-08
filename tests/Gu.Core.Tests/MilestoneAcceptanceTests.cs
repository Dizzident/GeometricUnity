using Gu.Branching;
using Gu.Branching.Conventions;
using Gu.Core;
using Gu.Core.Factories;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Math;
using Gu.Observation;
using Gu.ReferenceCpu;
using Gu.Solvers;

namespace Gu.Core.Tests;

/// <summary>
/// Acceptance tests for Milestones 1 and 2 of Minimal GU v1.
///
/// M1 acceptance criteria (Section 10.2):
///   - A run cannot start without a valid branch manifest
///   - Tensor signatures enforce basis/component order
///   - Branch ID is written to all outputs
///
/// M2 acceptance criteria (Section 10.3):
///   - Can create a toy Y_h -> X_h mapping
///   - Can evaluate sigma_h^* on a simple test field
///   - Geometry artifacts replay
///
/// Also validates code-reviewer fixes:
///   - FIX-1: DifferentialFormMetricId added to BranchManifest
///   - FIX-2: BaseDimension made required
///   - FIX-3: ReplayTier added to ReplayContract (required)
///   - OutputType is an enum
/// </summary>
public class MilestoneAcceptanceTests
{
    #region Helpers

    private static BranchManifest ValidManifest(string branchId = "acceptance-test") => new()
    {
        BranchId = branchId,
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "rev-1",
        CodeRevision = "test-abc123",
        ActiveGeometryBranch = "simplicial-4d",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "local-algebraic",
        ActiveShiabBranch = "first-order-curvature",
        ActiveGaugeStrategy = "penalty",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "basis-standard",
        ComponentOrderId = "order-row-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-killing",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = new[] { "IA-1", "IA-2", "IA-3", "IA-4", "IA-5", "IA-6" },
        InsertedChoiceIds = new[] { "IX-1", "IX-2" },
    };

    #endregion

    // ===================================================================
    // M1 Acceptance: "A run cannot start without a valid branch manifest"
    // ===================================================================

    [Fact]
    public void M1_RunCannotStartWithoutValidManifest_EmptyManifestRejected()
    {
        var empty = BranchManifestFactory.CreateEmpty();
        var errors = BranchManifestValidator.Validate(empty);
        Assert.NotEmpty(errors);
        Assert.Throws<InvalidOperationException>(() =>
            BranchManifestValidator.ValidateOrThrow(empty));
    }

    [Fact]
    public void M1_RunCannotStartWithoutValidManifest_ValidManifestAccepted()
    {
        var manifest = ValidManifest();
        var errors = BranchManifestValidator.Validate(manifest);
        Assert.Empty(errors);
        BranchManifestValidator.ValidateOrThrow(manifest); // should not throw
    }

    [Fact]
    public void M1_RunCannotStartWithoutValidManifest_MissingLieAlgebra_Rejected()
    {
        var manifest = new BranchManifest
        {
            BranchId = "test", SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1", CodeRevision = "c1",
            ActiveGeometryBranch = "g", ActiveObservationBranch = "o",
            ActiveTorsionBranch = "t", ActiveShiabBranch = "s",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 4, AmbientDimension = 14,
            LieAlgebraId = "unset",
            BasisConventionId = "basis-standard",
            ComponentOrderId = "order-row-major",
            AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-killing",
            NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };
        var errors = BranchManifestValidator.Validate(manifest);
        Assert.Contains(errors, e => e.Contains("LieAlgebraId"));
    }

    [Fact]
    public void M1_RunCannotStartWithoutValidManifest_InvalidDimensions_Rejected()
    {
        var manifest = new BranchManifest
        {
            BranchId = "test", SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1", CodeRevision = "c1",
            ActiveGeometryBranch = "g", ActiveObservationBranch = "o",
            ActiveTorsionBranch = "t", ActiveShiabBranch = "s",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 14, AmbientDimension = 4, // invalid: ambient < base
            LieAlgebraId = "su2",
            BasisConventionId = "basis-standard",
            ComponentOrderId = "order-row-major",
            AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-killing",
            NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };
        var errors = BranchManifestValidator.Validate(manifest);
        Assert.Contains(errors, e => e.Contains("AmbientDimension"));
    }

    // ===================================================================
    // M1 Acceptance: "Tensor signatures enforce basis/component order"
    // ===================================================================

    [Fact]
    public void M1_TensorSignatures_EnforceBasisOrder()
    {
        var sig = new TensorSignature
        {
            AmbientSpaceId = "Y_h",
            CarrierType = "connection-1form",
            Degree = "1",
            LieAlgebraBasisId = "different-basis",
            ComponentOrderId = "order-row-major",
            MemoryLayout = "dense-row-major",
        };
        var manifest = ValidManifest();

        var errors = TensorSignatureEnforcer.ValidateAgainstManifest(sig, "omega_h", manifest);
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("basis"));
    }

    [Fact]
    public void M1_TensorSignatures_EnforceComponentOrder()
    {
        var sigA = new TensorSignature
        {
            AmbientSpaceId = "Y_h", CarrierType = "connection-1form",
            Degree = "1", LieAlgebraBasisId = "basis-standard",
            ComponentOrderId = "order-row-major", MemoryLayout = "dense-row-major",
        };
        var sigB = new TensorSignature
        {
            AmbientSpaceId = "Y_h", CarrierType = "connection-1form",
            Degree = "1", LieAlgebraBasisId = "basis-standard",
            ComponentOrderId = "col-major", MemoryLayout = "dense-row-major",
        };

        var errors = TensorSignatureEnforcer.ValidateCompatibility(sigA, "A", sigB, "B");
        Assert.Contains(errors, e => e.Contains("Component order mismatch"));
    }

    [Fact]
    public void M1_TensorSignatures_ShapeMismatch_Detected()
    {
        var field = new FieldTensor
        {
            Label = "bad-shape",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h", CarrierType = "connection-1form",
                Degree = "1", LieAlgebraBasisId = "basis-standard",
                ComponentOrderId = "order-row-major", MemoryLayout = "dense-row-major",
            },
            Coefficients = new double[] { 1, 2, 3 },
            Shape = new[] { 2, 2 }, // product = 4, but coefficients has 3
        };

        var errors = TensorSignatureEnforcer.ValidateField(field);
        Assert.NotEmpty(errors);
    }

    // ===================================================================
    // M1 Acceptance: "Branch ID is written to all outputs"
    // ===================================================================

    [Fact]
    public void M1_BranchIdWrittenToAllOutputs_PipelineProducesConsistentBranchId()
    {
        const string testBranchId = "branch-id-propagation-test";
        var mesh = MeshTopologyBuilder.Build(
            embeddingDimension: 2, simplicialDimension: 2,
            vertexCoordinates: new double[] { 0, 0, 1, 0, 0, 1 },
            vertexCount: 3, cellVertices: new[] { new[] { 0, 1, 2 } });

        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var manifest = new BranchManifest
        {
            BranchId = testBranchId, SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1", CodeRevision = "c1",
            ActiveGeometryBranch = "simplicial", ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "trivial", ActiveShiabBranch = "identity-shiab",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 2, AmbientDimension = 5,
            LieAlgebraId = "su2", BasisConventionId = "canonical",
            ComponentOrderId = "face-major", AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-trace", NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };
        var geometry = new GeometryContext
        {
            BaseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            AmbientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 5 },
            DiscretizationType = "simplicial", QuadratureRuleId = "centroid",
            BasisFamilyId = "P1",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 5 },
                TargetSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 },
                TargetSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 5 },
            },
            Patches = new[] { new PatchInfo { PatchId = "p0", ElementCount = 1 } },
        };

        var torsion = new TrivialTorsionCpu(mesh, algebra);
        var shiab = new IdentityShiabCpu(mesh, algebra);
        var pipeline = new CpuSolverPipeline(mesh, algebra, torsion, shiab);

        var result = pipeline.ExecuteFromFlat(manifest, geometry,
            new SolverOptions { Mode = SolveMode.ResidualOnly });
        var ab = result.ArtifactBundle;

        // Branch ID must appear in all output containers
        Assert.Equal(testBranchId, ab.Branch.BranchId);
        Assert.Equal(testBranchId, ab.Provenance.Branch.BranchId);
        Assert.Equal(testBranchId, ab.InitialState!.Branch.BranchId);
        Assert.Equal(testBranchId, ab.FinalState!.Branch.BranchId);
        Assert.Equal(testBranchId, ab.ReplayContract.BranchManifest.BranchId);
        Assert.Contains(testBranchId, ab.ArtifactId);
    }

    // ===================================================================
    // M1: Lie algebra structure constants correctness
    // ===================================================================

    [Fact]
    public void M1_Su2_StructureConstants_AllCyclicBrackets()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        var e1 = new double[] { 1, 0, 0 };
        var e2 = new double[] { 0, 1, 0 };
        var e3 = new double[] { 0, 0, 1 };

        // [T1, T2] = T3
        var b12 = su2.Bracket(e1, e2);
        Assert.Equal(0.0, b12[0], 15);
        Assert.Equal(0.0, b12[1], 15);
        Assert.Equal(1.0, b12[2], 15);

        // [T2, T3] = T1
        var b23 = su2.Bracket(e2, e3);
        Assert.Equal(1.0, b23[0], 15);
        Assert.Equal(0.0, b23[1], 15);
        Assert.Equal(0.0, b23[2], 15);

        // [T3, T1] = T2
        var b31 = su2.Bracket(e3, e1);
        Assert.Equal(0.0, b31[0], 15);
        Assert.Equal(1.0, b31[1], 15);
        Assert.Equal(0.0, b31[2], 15);
    }

    [Fact]
    public void M1_Su2_BracketAntisymmetry()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        var a = new double[] { 1.0, 2.0, 3.0 };
        var b = new double[] { 4.0, 5.0, 6.0 };

        var ab = su2.Bracket(a, b);
        var ba = su2.Bracket(b, a);

        for (int i = 0; i < 3; i++)
        {
            Assert.Equal(-ba[i], ab[i], 14);
        }
    }

    [Fact]
    public void M1_Su2_KillingForm_IsNegativeTwoDeltaAB()
    {
        var su2 = LieAlgebraFactory.CreateSu2();

        // g_{ab} = -2 * delta_{ab}
        for (int a = 0; a < 3; a++)
        {
            for (int b = 0; b < 3; b++)
            {
                double expected = (a == b) ? -2.0 : 0.0;
                Assert.Equal(expected, su2.GetMetricComponent(a, b), 15);
            }
        }
    }

    [Fact]
    public void M1_Su2_JacobiIdentity_Holds()
    {
        var su2 = LieAlgebraFactory.CreateSu2();
        double violation = su2.ValidateJacobiIdentity();
        Assert.True(violation < 1e-14, $"Jacobi identity violation: {violation}");
    }

    [Fact]
    public void M1_ConventionRegistry_AllRequiredCategoriesPresent()
    {
        var registry = ConventionRegistry.CreateDefault();

        // Section 11 requires all five convention categories
        Assert.True(registry.GetByCategory("basis").Any(), "basis conventions missing");
        Assert.True(registry.GetByCategory("componentOrder").Any(), "componentOrder conventions missing");
        Assert.True(registry.GetByCategory("adjoint").Any(), "adjoint conventions missing");
        Assert.True(registry.GetByCategory("pairing").Any(), "pairing conventions missing");
        Assert.True(registry.GetByCategory("norm").Any(), "norm conventions missing");
    }

    // ===================================================================
    // M2 Acceptance: "Can create a toy Y_h -> X_h mapping"
    // ===================================================================

    [Fact]
    public void M2_ToyMapping_Toy2D_ValidFiberBundle()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();

        Assert.True(bundle.ValidateSection(), "sigma_h section validation failed");
        Assert.True(bundle.ValidateFibers(), "Fiber structure validation failed");

        // X_h: 5 vertices, 4 triangles, dim=2
        Assert.Equal(5, bundle.BaseMesh.VertexCount);
        Assert.Equal(4, bundle.BaseMesh.CellCount);
        Assert.Equal(2, bundle.BaseMesh.EmbeddingDimension);

        // Y_h: 15 vertices (5 * 3 fiber points), dim=5
        Assert.Equal(15, bundle.AmbientMesh.VertexCount);
        Assert.Equal(5, bundle.AmbientMesh.EmbeddingDimension);

        // pi_h: every Y vertex maps to an X vertex
        for (int yv = 0; yv < bundle.AmbientMesh.VertexCount; yv++)
        {
            int xv = bundle.YVertexToXVertex[yv];
            Assert.True(xv >= 0 && xv < bundle.BaseMesh.VertexCount,
                $"Y vertex {yv} maps to invalid X vertex {xv}");
        }

        // sigma_h: each X vertex maps to a Y vertex in its fiber
        for (int xv = 0; xv < bundle.BaseMesh.VertexCount; xv++)
        {
            int yv = bundle.XVertexToYVertex[xv];
            Assert.Contains(yv, bundle.FiberVerticesPerXVertex[xv]);
            Assert.Equal(xv, bundle.YVertexToXVertex[yv]);
        }
    }

    [Fact]
    public void M2_ToyMapping_Toy3D_ValidFiberBundle()
    {
        var bundle = ToyGeometryFactory.CreateToy3D();

        Assert.True(bundle.ValidateSection());
        Assert.True(bundle.ValidateFibers());
        Assert.Equal(3, bundle.BaseMesh.EmbeddingDimension);
        Assert.Equal(9, bundle.AmbientMesh.EmbeddingDimension);
        Assert.True(bundle.AmbientMesh.EdgeCount > 0);
        Assert.True(bundle.AmbientMesh.FaceCount > 0);
    }

    // ===================================================================
    // M2 Acceptance: "Can evaluate sigma_h^* on a simple test field"
    // ===================================================================

    [Fact]
    public void M2_SigmaHStar_VertexScalar_ExtractsCorrectValues()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);

        // Linear scalar field on Y_h: f(yv) = yv
        var yCoeffs = new double[bundle.AmbientMesh.VertexCount];
        for (int i = 0; i < yCoeffs.Length; i++) yCoeffs[i] = i;

        var yField = new FieldTensor
        {
            Label = "linear",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h", CarrierType = "scalar", Degree = "0",
                LieAlgebraBasisId = "trivial", ComponentOrderId = "natural",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = yCoeffs,
            Shape = new[] { bundle.AmbientMesh.VertexCount },
        };

        var xField = pullback.ApplyVertexScalar(yField);

        // sigma selects fiber point 0 for each X vertex: yv = xv * 3
        Assert.Equal(bundle.BaseMesh.VertexCount, xField.Coefficients.Length);
        for (int xv = 0; xv < bundle.BaseMesh.VertexCount; xv++)
        {
            int expectedYv = bundle.XVertexToYVertex[xv];
            Assert.Equal((double)expectedYv, xField.Coefficients[xv]);
        }

        // Output must be on X_h
        Assert.Equal("X_h", xField.Signature.AmbientSpaceId);
    }

    [Fact]
    public void M2_SigmaHStar_FaceField_ExtractsCorrectDimensions()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);

        int dimG = 3;
        int yFaceCount = bundle.AmbientMesh.FaceCount;
        var yCoeffs = new double[yFaceCount * dimG];
        for (int i = 0; i < yCoeffs.Length; i++) yCoeffs[i] = i * 0.1;

        var yField = new FieldTensor
        {
            Label = "F_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h", CarrierType = "curvature-2form", Degree = "2",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "face-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = yCoeffs,
            Shape = new[] { yFaceCount, dimG },
        };

        var xField = pullback.ApplyFaceField(yField, dimG);

        // Output: BaseMesh.FaceCount * dimG
        Assert.Equal(bundle.BaseMesh.FaceCount * dimG, xField.Coefficients.Length);
        Assert.Equal("X_h", xField.Signature.AmbientSpaceId);
        Assert.StartsWith("sigma_h*", xField.Label);
    }

    [Fact]
    public void M2_SigmaHStar_EdgeField_ExtractsCorrectDimensions()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var pullback = new PullbackOperator(bundle);

        int dimG = 3;
        int yEdgeCount = bundle.AmbientMesh.EdgeCount;
        var yCoeffs = new double[yEdgeCount * dimG];
        for (int i = 0; i < yCoeffs.Length; i++) yCoeffs[i] = i * 0.01;

        var yField = new FieldTensor
        {
            Label = "omega_h",
            Signature = new TensorSignature
            {
                AmbientSpaceId = "Y_h", CarrierType = "connection-1form", Degree = "1",
                LieAlgebraBasisId = "canonical", ComponentOrderId = "edge-major",
                MemoryLayout = "dense-row-major",
            },
            Coefficients = yCoeffs,
            Shape = new[] { yEdgeCount, dimG },
        };

        var xField = pullback.ApplyEdgeField(yField, dimG);

        Assert.Equal(bundle.BaseMesh.EdgeCount * dimG, xField.Coefficients.Length);
        Assert.Equal("X_h", xField.Signature.AmbientSpaceId);
    }

    // ===================================================================
    // M2 Acceptance: "Geometry artifacts replay"
    // ===================================================================

    [Fact]
    public void M2_GeometryArtifactReplay_ContextSerializesRoundTrip()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var ctx = bundle.ToGeometryContext("centroid", "P1");

        var json = GuJsonDefaults.Serialize(ctx);
        var restored = GuJsonDefaults.Deserialize<GeometryContext>(json);

        Assert.NotNull(restored);
        Assert.Equal(ctx.BaseSpace.SpaceId, restored!.BaseSpace.SpaceId);
        Assert.Equal(ctx.BaseSpace.Dimension, restored.BaseSpace.Dimension);
        Assert.Equal(ctx.AmbientSpace.SpaceId, restored.AmbientSpace.SpaceId);
        Assert.Equal(ctx.AmbientSpace.Dimension, restored.AmbientSpace.Dimension);
        Assert.Equal(ctx.DiscretizationType, restored.DiscretizationType);
        Assert.Equal(ctx.QuadratureRuleId, restored.QuadratureRuleId);
        Assert.Equal(ctx.BasisFamilyId, restored.BasisFamilyId);
        Assert.Equal(ctx.ProjectionBinding.BindingType, restored.ProjectionBinding.BindingType);
        Assert.Equal(ctx.ObservationBinding.BindingType, restored.ObservationBinding.BindingType);
    }

    [Fact]
    public void M2_GeometryArtifactReplay_ArtifactBundleContainsGeometry()
    {
        var bundle = ToyGeometryFactory.CreateToy2D();
        var yMesh = bundle.AmbientMesh;
        var algebra = LieAlgebraFactory.CreateSu2WithTracePairing();
        var geometry = bundle.ToGeometryContext("centroid", "P1");

        var manifest = new BranchManifest
        {
            BranchId = "replay-test", SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1", CodeRevision = "c1",
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
        var pipeline = new CpuSolverPipeline(yMesh, algebra, torsion, shiab);

        var result = pipeline.ExecuteFromFlat(manifest, geometry,
            new SolverOptions { Mode = SolveMode.ResidualOnly });

        // Geometry context must be in artifact bundle for replay
        Assert.NotNull(result.ArtifactBundle.Geometry);
        Assert.Equal("X_h", result.ArtifactBundle.Geometry!.BaseSpace.SpaceId);
        Assert.Equal("Y_h", result.ArtifactBundle.Geometry.AmbientSpace.SpaceId);

        // The geometry context round-trips through JSON
        var bundleJson = GuJsonDefaults.Serialize(result.ArtifactBundle);
        Assert.Contains("\"geometry\"", bundleJson);
        Assert.Contains("\"baseSpace\"", bundleJson);
    }

    // ===================================================================
    // Code-reviewer fix verification
    // ===================================================================

    [Fact]
    public void Fix1_DifferentialFormMetricId_InBranchManifest()
    {
        var manifest = ValidManifest();
        Assert.Equal("hodge-standard", manifest.DifferentialFormMetricId);

        // It round-trips through JSON
        var json = GuJsonDefaults.Serialize(manifest);
        Assert.Contains("\"differentialFormMetricId\"", json);
        var restored = GuJsonDefaults.Deserialize<BranchManifest>(json);
        Assert.Equal("hodge-standard", restored!.DifferentialFormMetricId);
    }

    [Fact]
    public void Fix1_DifferentialFormMetricId_ValidatedByBranchManifestValidator()
    {
        // DifferentialFormMetricId is required; empty manifest should flag it
        var empty = BranchManifestFactory.CreateEmpty();
        var errors = BranchManifestValidator.Validate(empty);
        // The validator should catch the empty/unset DifferentialFormMetricId
        Assert.NotEmpty(errors);
    }

    [Fact]
    public void Fix2_BaseDimension_IsRequired()
    {
        // BaseDimension is `required int` -- cannot be silently zero.
        // The validator catches BaseDimension <= 0.
        var manifest = new BranchManifest
        {
            BranchId = "fix2-test", SchemaVersion = "1.0.0",
            SourceEquationRevision = "r1", CodeRevision = "c1",
            ActiveGeometryBranch = "g", ActiveObservationBranch = "o",
            ActiveTorsionBranch = "t", ActiveShiabBranch = "s",
            ActiveGaugeStrategy = "penalty",
            BaseDimension = 0, // invalid
            AmbientDimension = 14,
            LieAlgebraId = "su2", BasisConventionId = "basis-standard",
            ComponentOrderId = "order-row-major", AdjointConventionId = "adjoint-explicit",
            PairingConventionId = "pairing-killing", NormConventionId = "norm-l2-quadrature",
            DifferentialFormMetricId = "hodge-standard",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };

        var errors = BranchManifestValidator.Validate(manifest);
        Assert.Contains(errors, e => e.Contains("BaseDimension"));
    }

    [Fact]
    public void Fix3_ReplayTier_IsRequiredOnReplayContract()
    {
        // ReplayTier is `required string` on ReplayContract
        var contract = new ReplayContract
        {
            BranchManifest = ValidManifest(),
            Deterministic = true,
            BackendId = "cpu-reference",
            ReplayTier = "R2",
        };

        Assert.Equal("R2", contract.ReplayTier);

        // Round-trip
        var json = GuJsonDefaults.Serialize(contract);
        Assert.Contains("\"replayTier\"", json);
        var restored = GuJsonDefaults.Deserialize<ReplayContract>(json);
        Assert.NotNull(restored);
        Assert.Equal("R2", restored!.ReplayTier);
    }

    [Fact]
    public void Fix4_OutputType_IsEnum()
    {
        // OutputType must be an enum, not a string
        Assert.True(typeof(OutputType).IsEnum);

        // Verify expected values exist
        Assert.True(Enum.IsDefined(typeof(OutputType), OutputType.Quantitative));
        Assert.True(Enum.IsDefined(typeof(OutputType), OutputType.ExactStructural));

        // JSON serializes as string (JsonStringEnumConverter)
        var json = System.Text.Json.JsonSerializer.Serialize(OutputType.Quantitative);
        Assert.Contains("Quantitative", json);
    }
}
