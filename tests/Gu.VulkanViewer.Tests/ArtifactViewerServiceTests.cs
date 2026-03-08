using Gu.Artifacts;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Geometry;
using Gu.Solvers;

namespace Gu.VulkanViewer.Tests;

public class ArtifactViewerServiceTests
{
    [Fact]
    public void LoadRunFolder_ThrowsOnInvalidPath()
    {
        var service = new ArtifactViewerService();
        Assert.Throws<InvalidOperationException>(() =>
            service.LoadRunFolder("/nonexistent/path/to/run"));
    }

    [Fact]
    public void LoadRunFolder_ThrowsOnEmptyString()
    {
        var service = new ArtifactViewerService();
        Assert.Throws<ArgumentException>(() => service.LoadRunFolder(""));
    }

    [Fact]
    public void LoadRunFolder_ReadsValidRunFolder()
    {
        string tempDir = CreateTempRunFolder(includeManifest: true);
        try
        {
            var service = new ArtifactViewerService();
            var snapshot = service.LoadRunFolder(tempDir);

            Assert.Equal(tempDir, snapshot.RunFolderPath);
            Assert.NotNull(snapshot.BranchManifest);
            Assert.Equal("test-branch", snapshot.BranchManifest!.BranchId);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void LoadRunFolder_ReturnsNullForMissingComponents()
    {
        string tempDir = CreateTempRunFolder(includeManifest: true);
        try
        {
            var service = new ArtifactViewerService();
            var snapshot = service.LoadRunFolder(tempDir);

            // No initial/final state, derived state, residuals written
            Assert.Null(snapshot.InitialState);
            Assert.Null(snapshot.FinalState);
            Assert.Null(snapshot.DerivedState);
            Assert.Null(snapshot.Residuals);
            Assert.Null(snapshot.SolverResult);
            Assert.Empty(snapshot.ComparisonRecords);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void PrepareViews_IncludesGeometryView()
    {
        var service = new ArtifactViewerService();
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var snapshot = new WorkbenchSnapshot
        {
            RunFolderPath = "/test",
            LoadedAt = DateTimeOffset.UtcNow,
        };

        var views = service.PrepareViews(snapshot, mesh);

        // At minimum, geometry view should always be present
        Assert.True(views.Count >= 1);
        Assert.Contains(views, v => v.ViewType == "geometry");
    }

    [Fact]
    public void PrepareViews_IncludesDerivedFieldViews()
    {
        var service = new ArtifactViewerService();
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);

        var snapshot = new WorkbenchSnapshot
        {
            RunFolderPath = "/test",
            DerivedState = derived,
            LoadedAt = DateTimeOffset.UtcNow,
        };

        var views = service.PrepareViews(snapshot, mesh);

        // geometry + 4 derived field views
        Assert.True(views.Count >= 5);
        Assert.Contains(views, v => v is FieldViewPayload fv && fv.FieldMode == "curvature");
        Assert.Contains(views, v => v is FieldViewPayload fv && fv.FieldMode == "torsion");
        Assert.Contains(views, v => v is FieldViewPayload fv && fv.FieldMode == "shiab");
        Assert.Contains(views, v => v is FieldViewPayload fv && fv.FieldMode == "residual");
    }

    [Fact]
    public void PrepareViews_IncludesResidualView()
    {
        var service = new ArtifactViewerService();
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);
        var residuals = new ResidualBundle
        {
            Components = Array.Empty<ResidualComponent>(),
            ObjectiveValue = 1.5,
            TotalNorm = 1.0,
        };

        var snapshot = new WorkbenchSnapshot
        {
            RunFolderPath = "/test",
            DerivedState = derived,
            Residuals = residuals,
            LoadedAt = DateTimeOffset.UtcNow,
        };

        var views = service.PrepareViews(snapshot, mesh);

        Assert.Contains(views, v => v.ViewType == "residual");
    }

    [Fact]
    public void PrepareViews_IncludesConvergenceView()
    {
        var service = new ArtifactViewerService();
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);

        var result = new SolverResult
        {
            Converged = true,
            TerminationReason = "converged",
            Iterations = 5,
            FinalObjective = 0.001,
            FinalResidualNorm = 0.01,
            FinalGradientNorm = 0.001,
            FinalGaugeViolation = 0.0001,
            FinalOmega = TestDataHelper.CreateEdgeScalarField(new double[mesh.EdgeCount], "omega"),
            FinalDerivedState = derived,
            History = TestDataHelper.CreateConvergenceHistory(5),
            Mode = SolveMode.ObjectiveMinimization,
        };

        var snapshot = new WorkbenchSnapshot
        {
            RunFolderPath = "/test",
            SolverResult = result,
            LoadedAt = DateTimeOffset.UtcNow,
        };

        var views = service.PrepareViews(snapshot, mesh);

        Assert.Contains(views, v => v.ViewType == "convergence");
    }

    [Fact]
    public void PrepareViews_IncludesSpectrumView()
    {
        var service = new ArtifactViewerService();
        var mesh = TestDataHelper.CreateQuadMesh3D();

        var linearization = new LinearizationState
        {
            Jacobian = new LinearOperatorModel
            {
                Label = "J",
                RealizationType = "dense",
                Rows = 6,
                Cols = 5,
            },
            GradientLikeResidual = TestDataHelper.CreateVertexScalarField(
                new double[] { 0.1, 0.2, 0.3, 0.4 }, "grad"),
        };

        var snapshot = new WorkbenchSnapshot
        {
            RunFolderPath = "/test",
            Linearization = linearization,
            LoadedAt = DateTimeOffset.UtcNow,
        };

        var views = service.PrepareViews(snapshot, mesh);

        Assert.Contains(views, v => v.ViewType == "spectrum");
    }

    [Fact]
    public void PrepareViews_IncludesComparisonOverlay()
    {
        var service = new ArtifactViewerService();
        var mesh = TestDataHelper.CreateQuadMesh3D();

        var records = new ExternalComparison.ComparisonRecord[]
        {
            CreateComparisonRecord(ExternalComparison.ComparisonOutcome.Pass, "test-obs-1"),
            CreateComparisonRecord(ExternalComparison.ComparisonOutcome.Fail, "test-obs-2"),
            CreateComparisonRecord(ExternalComparison.ComparisonOutcome.Invalid, "test-obs-3"),
        };

        var snapshot = new WorkbenchSnapshot
        {
            RunFolderPath = "/test",
            ComparisonRecords = records,
            LoadedAt = DateTimeOffset.UtcNow,
        };

        var views = service.PrepareViews(snapshot, mesh);

        var overlay = views.OfType<ComparisonOverlayPayload>().SingleOrDefault();
        Assert.NotNull(overlay);
        Assert.Equal(3, overlay!.TotalCount);
        Assert.Equal(1, overlay.PassCount);
        Assert.Equal(1, overlay.FailCount);
        Assert.Equal(1, overlay.InvalidCount);
        Assert.Equal(3, overlay.Entries.Count);
    }

    [Fact]
    public void PrepareViews_ReadOnlyConstraint_DoesNotModifySnapshot()
    {
        var service = new ArtifactViewerService();
        var mesh = TestDataHelper.CreateQuadMesh3D();
        var derived = TestDataHelper.CreateDerivedState(mesh.FaceCount, mesh.EdgeCount);

        // Copy original coefficients
        var originalCoeffs = (double[])derived.ResidualUpsilon.Coefficients.Clone();

        var snapshot = new WorkbenchSnapshot
        {
            RunFolderPath = "/test",
            DerivedState = derived,
            Residuals = new ResidualBundle
            {
                Components = Array.Empty<ResidualComponent>(),
                ObjectiveValue = 1.0,
                TotalNorm = 1.0,
            },
            LoadedAt = DateTimeOffset.UtcNow,
        };

        _ = service.PrepareViews(snapshot, mesh);

        // Verify artifact data was not modified (IX-5)
        Assert.Equal(originalCoeffs, derived.ResidualUpsilon.Coefficients);
    }

    [Fact]
    public void ComparisonOverlay_EntriesContainCorrectData()
    {
        var service = new ArtifactViewerService();
        var mesh = TestDataHelper.CreateTriangleMesh2D();

        var records = new[]
        {
            CreateComparisonRecord(
                ExternalComparison.ComparisonOutcome.Pass,
                "bianchi-residual",
                "structural_match",
                new Dictionary<string, double> { ["maxDeviation"] = 1e-12 },
                "Structural match passed"),
        };

        var snapshot = new WorkbenchSnapshot
        {
            RunFolderPath = "/test",
            ComparisonRecords = records,
            LoadedAt = DateTimeOffset.UtcNow,
        };

        var views = service.PrepareViews(snapshot, mesh);
        var overlay = views.OfType<ComparisonOverlayPayload>().Single();

        var entry = overlay.Entries[0];
        Assert.Equal("bianchi-residual", entry.ObservableId);
        Assert.Equal("structural_match", entry.ComparisonRule);
        Assert.Equal("Pass", entry.Outcome);
        Assert.Equal(1e-12, entry.PrimaryMetric);
        Assert.Equal("maxDeviation", entry.PrimaryMetricName);
    }

    // Helper methods

    private static string CreateTempRunFolder(bool includeManifest)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"gu_test_{Guid.NewGuid():N}");

        // Create required directories
        foreach (var dir in RunFolderLayout.RequiredDirectories)
        {
            Directory.CreateDirectory(Path.Combine(tempDir, dir));
        }

        if (includeManifest)
        {
            var manifest = new BranchManifest
            {
                BranchId = "test-branch",
                SchemaVersion = "1.0",
                SourceEquationRevision = "v1",
                CodeRevision = "abc",
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
            };
            var json = GuJsonDefaults.Serialize(manifest);
            File.WriteAllText(
                Path.Combine(tempDir, RunFolderLayout.BranchManifestFile),
                json);
        }

        return tempDir;
    }

    private static ExternalComparison.ComparisonRecord CreateComparisonRecord(
        ExternalComparison.ComparisonOutcome outcome,
        string observableId,
        string comparisonRule = "relative_error",
        Dictionary<string, double>? metrics = null,
        string message = "test comparison")
    {
        return new ExternalComparison.ComparisonRecord
        {
            ComparisonId = Guid.NewGuid().ToString("N"),
            TemplateId = "tmpl-1",
            ObservableId = observableId,
            ReferenceSourceId = "test-ref",
            ReferenceVersion = "1.0",
            Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
            ComparisonRule = comparisonRule,
            ComparisonScope = "global",
            Outcome = outcome,
            Metrics = metrics ?? new Dictionary<string, double> { ["value"] = 0.0 },
            Message = message,
            PullbackOperatorId = "identity",
            ObservationBranchId = "test",
            Provenance = new ProvenanceMeta
            {
                CreatedAt = DateTimeOffset.UtcNow,
                CodeRevision = "abc",
                Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
            },
            ExecutedAt = DateTimeOffset.UtcNow,
        };
    }
}
