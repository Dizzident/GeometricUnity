using Gu.Core;
using Gu.Geometry;
using Gu.Solvers;

namespace Gu.VulkanViewer.Tests;

public class WorkbenchSessionTests
{
    [Fact]
    public void Constructor_AcceptsMesh()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        Assert.Same(mesh, session.Mesh);
        Assert.Empty(session.AvailableFields);
        Assert.Empty(session.ConvergenceHistory);
    }

    [Fact]
    public void Constructor_ThrowsOnNullMesh()
    {
        Assert.Throws<ArgumentNullException>(() => new WorkbenchSession(null!));
    }

    [Fact]
    public void LoadFromSolverResult_PopulatesFieldsAndHistory()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var history = TestDataHelper.CreateConvergenceHistory(10);

        var result = new SolverResult
        {
            Converged = true,
            TerminationReason = "Objective tolerance reached",
            Iterations = 10,
            FinalObjective = 0.001,
            FinalResidualNorm = 0.01,
            FinalGradientNorm = 0.001,
            FinalGaugeViolation = 0.0001,
            FinalOmega = TestDataHelper.CreateEdgeScalarField(
                new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 }, "omega_h"),
            FinalDerivedState = derived,
            History = history,
            Mode = SolveMode.ObjectiveMinimization,
        };

        var initialOmega = TestDataHelper.CreateEdgeScalarField(
            new double[] { 0, 0, 0, 0, 0 }, "omega_h_initial");

        int count = session.LoadFromSolverResult(result, initialOmega);

        // Should have: Initial Omega, Final Omega, Curvature F, Torsion T, Shiab S, Residual Upsilon
        Assert.Equal(6, count);
        Assert.Equal(6, session.AvailableFields.Count);
        Assert.Equal(10, session.ConvergenceHistory.Count);
    }

    [Fact]
    public void LoadFromBundle_PopulatesFields()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var omega = TestDataHelper.CreateEdgeScalarField(
            new double[] { 0.1, 0.2, 0.3, 0.4, 0.5 }, "omega_h");

        var bundle = new ArtifactBundle
        {
            ArtifactId = "test-artifact",
            Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
            ReplayContract = CreateMinimalReplayContract(),
            InitialState = new DiscreteState
            {
                Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
                Geometry = CreateMinimalGeometryContext(),
                Omega = omega,
                Provenance = CreateMinimalProvenance(),
            },
            DerivedState = derived,
            Provenance = CreateMinimalProvenance(),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        int count = session.LoadFromBundle(bundle);

        // Initial Omega + Curvature F + Torsion T + Shiab S + Residual Upsilon = 5
        Assert.Equal(5, count);
    }

    [Fact]
    public void PrepareVisualization_ReturnsDataForLoadedField()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var result = CreateMinimalSolverResult(mesh, derived);

        session.LoadFromSolverResult(result);

        var data = session.PrepareVisualization("Final Omega");

        Assert.NotNull(data);
        Assert.Equal(mesh.VertexCount, data.VertexCount);
    }

    [Fact]
    public void PrepareVisualization_CaseInsensitiveLookup()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var result = CreateMinimalSolverResult(mesh, derived);

        session.LoadFromSolverResult(result);

        var data = session.PrepareVisualization("final omega");
        Assert.NotNull(data);
    }

    [Fact]
    public void PrepareVisualization_ThrowsOnUnknownField()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        Assert.Throws<ArgumentException>(() => session.PrepareVisualization("nonexistent"));
    }

    [Fact]
    public void PrepareVisualization_ResidualUsesDivergingScheme()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var result = CreateMinimalSolverResult(mesh, derived);

        session.LoadFromSolverResult(result);

        var data = session.PrepareVisualization("Residual Upsilon");

        // Auto-selected diverging scheme.
        Assert.Equal("diverging", data.ColorMap.ColorSchemeName);
    }

    [Fact]
    public void PrepareVisualization_WithExplicitScheme_OverridesDefault()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var result = CreateMinimalSolverResult(mesh, derived);

        session.LoadFromSolverResult(result);

        var data = session.PrepareVisualization("Residual Upsilon", "plasma");

        Assert.Equal("plasma", data.ColorMap.ColorSchemeName);
    }

    [Fact]
    public void GetConvergencePlot_ReturnsNull_WhenNoHistory()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        Assert.Null(session.GetConvergencePlot());
    }

    [Fact]
    public void GetConvergencePlot_ReturnsPlotData_WhenHistoryLoaded()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var result = CreateMinimalSolverResult(mesh, derived);

        session.LoadFromSolverResult(result);

        var plotData = session.GetConvergencePlot();

        Assert.NotNull(plotData);
        Assert.Equal(10, plotData.IterationCount);
        Assert.True(plotData.Converged);
    }

    [Fact]
    public void ExportSessionSummaryJson_ProducesValidJson()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var result = CreateMinimalSolverResult(mesh, derived);

        session.LoadFromSolverResult(result);

        string json = session.ExportSessionSummaryJson();

        // Should be valid JSON and contain key information.
        Assert.Contains("vertexCount", json);
        Assert.Contains("availableFields", json);
        Assert.Contains("convergenceInfo", json);
    }

    [Fact]
    public void ExportToObj_ProducesFiles()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var result = CreateMinimalSolverResult(mesh, derived);

        session.LoadFromSolverResult(result);

        string tempDir = Path.Combine(Path.GetTempPath(), $"gu_test_{Guid.NewGuid():N}");

        try
        {
            var (objPath, csvPath) = session.ExportToObj("Final Omega", tempDir);

            Assert.True(File.Exists(objPath));
            Assert.True(File.Exists(csvPath));
            Assert.Contains(".obj", objPath);
            Assert.Contains("_colors.csv", csvPath);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ExportToPly_ProducesFile()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var result = CreateMinimalSolverResult(mesh, derived);

        session.LoadFromSolverResult(result);

        string tempDir = Path.Combine(Path.GetTempPath(), $"gu_test_{Guid.NewGuid():N}");

        try
        {
            string plyPath = session.ExportToPly("Final Omega", tempDir);

            Assert.True(File.Exists(plyPath));
            string content = File.ReadAllText(plyPath);
            Assert.StartsWith("ply", content);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void ExportConvergenceCsv_ReturnsFalse_WhenNoHistory()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        string tempPath = Path.Combine(Path.GetTempPath(), $"gu_test_{Guid.NewGuid():N}.csv");

        try
        {
            bool exported = session.ExportConvergenceCsv(tempPath);
            Assert.False(exported);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void ExportConvergenceCsv_WritesFile_WhenHistoryPresent()
    {
        var mesh = TestDataHelper.CreateTriangleMesh2D();
        var session = new WorkbenchSession(mesh);

        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var result = CreateMinimalSolverResult(mesh, derived);

        session.LoadFromSolverResult(result);

        string tempPath = Path.Combine(Path.GetTempPath(), $"gu_test_{Guid.NewGuid():N}.csv");

        try
        {
            bool exported = session.ExportConvergenceCsv(tempPath);

            Assert.True(exported);
            Assert.True(File.Exists(tempPath));
            string content = File.ReadAllText(tempPath);
            Assert.StartsWith("Iteration,Objective,", content);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public void LoadFromSolverResult_DerivedDiagnostics_IncludesAdditionalFields()
    {
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var session = new WorkbenchSession(mesh);

        var derived = new DerivedState
        {
            CurvatureF = TestDataHelper.CreateFaceScalarField(new double[] { 0.1, 0.2 }, "F_h"),
            TorsionT = TestDataHelper.CreateFaceScalarField(new double[] { 0.05, 0.1 }, "T_h"),
            ShiabS = TestDataHelper.CreateFaceScalarField(new double[] { 0.03, 0.06 }, "S_h"),
            ResidualUpsilon = TestDataHelper.CreateFaceScalarField(new double[] { -0.01, 0.01 }, "Upsilon_h"),
            Diagnostics = new Dictionary<string, FieldTensor>
            {
                ["energy_density"] = TestDataHelper.CreateVertexScalarField(new double[] { 1, 2, 3, 4 }, "energy"),
            },
        };

        var result = CreateMinimalSolverResultWithDerived(mesh, derived);

        int count = session.LoadFromSolverResult(result);

        // Final Omega + Curvature F + Torsion T + Shiab S + Residual Upsilon + Diagnostic = 6
        Assert.Equal(6, count);
        Assert.Contains(session.AvailableFields, f => f.Name.Contains("energy_density"));
    }

    // ---- Helper methods ----

    private static SolverResult CreateMinimalSolverResult(SimplicialMesh mesh, DerivedState derived)
    {
        return CreateMinimalSolverResultWithDerived(mesh, derived);
    }

    private static SolverResult CreateMinimalSolverResultWithDerived(SimplicialMesh mesh, DerivedState derived)
    {
        return new SolverResult
        {
            Converged = true,
            TerminationReason = "Objective tolerance reached",
            Iterations = 10,
            FinalObjective = 0.001,
            FinalResidualNorm = 0.01,
            FinalGradientNorm = 0.001,
            FinalGaugeViolation = 0.0001,
            FinalOmega = TestDataHelper.CreateEdgeScalarField(
                new double[mesh.EdgeCount], "omega_h"),
            FinalDerivedState = derived,
            History = TestDataHelper.CreateConvergenceHistory(10),
            Mode = SolveMode.ObjectiveMinimization,
        };
    }

    private static GeometryContext CreateMinimalGeometryContext()
    {
        var baseSpace = new SpaceRef { SpaceId = "X_h", Dimension = 2 };
        var ambientSpace = new SpaceRef { SpaceId = "Y_h", Dimension = 3 };

        return new GeometryContext
        {
            BaseSpace = baseSpace,
            AmbientSpace = ambientSpace,
            DiscretizationType = "simplicial",
            QuadratureRuleId = "midpoint",
            BasisFamilyId = "Whitney-1",
            ProjectionBinding = new GeometryBinding
            {
                BindingType = "projection",
                SourceSpace = ambientSpace,
                TargetSpace = baseSpace,
            },
            ObservationBinding = new GeometryBinding
            {
                BindingType = "observation",
                SourceSpace = baseSpace,
                TargetSpace = ambientSpace,
            },
            Patches = Array.Empty<PatchInfo>(),
        };
    }

    private static ReplayContract CreateMinimalReplayContract()
    {
        return new ReplayContract
        {
            BranchManifest = new BranchManifest
            {
                BranchId = "test-branch",
                SchemaVersion = "1.0",
                SourceEquationRevision = "v1",
                CodeRevision = "test-rev",
                ActiveGeometryBranch = "flat-2d",
                ActiveObservationBranch = "identity",
                ActiveTorsionBranch = "zero",
                ActiveShiabBranch = "identity",
                ActiveGaugeStrategy = "none",
                BaseDimension = 2,
                AmbientDimension = 3,
                LieAlgebraId = "so(1)",
                BasisConventionId = "canonical",
                ComponentOrderId = "canonical",
                AdjointConventionId = "standard",
                PairingConventionId = "standard",
                NormConventionId = "L2",
                DifferentialFormMetricId = "standard",
                InsertedAssumptionIds = Array.Empty<string>(),
                InsertedChoiceIds = Array.Empty<string>(),
            },
            Deterministic = true,
            BackendId = "reference-cpu",
            ReplayTier = "R0",
        };
    }

    private static ProvenanceMeta CreateMinimalProvenance()
    {
        return new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CodeRevision = "test-rev",
            Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
            Backend = "reference-cpu",
        };
    }
}
