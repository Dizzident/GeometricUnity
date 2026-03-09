using Gu.Core;
using Gu.Geometry;
using Gu.Phase2.Branches;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Continuation;
using Gu.Phase2.Execution;
using Gu.Phase2.Semantics;
using Gu.Phase2.Stability;
using Gu.Phase2.Viz;
using Gu.Solvers;

namespace Gu.Phase2.Viz.Tests;

public class SpectrumDashboardTests
{
    [Fact]
    public void Prepare_FromSpectrumRecord_ProducesPayload()
    {
        var record = new SpectrumRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            OperatorId = "H",
            ProbeMethod = "lanczos",
            RequestedCount = 5,
            ObtainedCount = 5,
            Values = new[] { -0.1, 0.001, 0.01, 0.5, 1.2 },
            ConvergenceStatus = "converged",
            ResidualNorms = new[] { 1e-10, 1e-10, 1e-10, 1e-10, 1e-10 },
            GaugeHandlingMode = "coulomb-slice",
            NormalizationConvention = "unit-l2",
            ModeSpace = "native-Y",
            StabilityInterpretation = "negative-modes-saddle",
        };

        var dashboard = new SpectrumDashboard();
        var payload = dashboard.Prepare(record);

        Assert.Equal("spectrum_dashboard", payload.ViewType);
        Assert.Equal("bg-1", payload.BackgroundStateId);
        Assert.Equal("H", payload.OperatorId);
        Assert.Equal(5, payload.Values.Length);
        Assert.Equal("negative-modes-saddle", payload.StabilityInterpretation);
        Assert.Equal("lanczos", payload.ProbeMethod);
        Assert.Equal(1, payload.NegativeModeCount);
    }

    [Fact]
    public void PrepareFromHessian_ProducesPayload()
    {
        var record = new HessianRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = 10.0,
            Dimension = 30,
            AssemblyMode = "matrix-free",
            SymmetryVerified = true,
            CoerciveModeCount = 8,
            SoftModeCount = 1,
            NearKernelCount = 0,
            NegativeModeCount = 1,
        };

        var dashboard = new SpectrumDashboard();
        var payload = dashboard.PrepareFromHessian(record);

        Assert.Equal("spectrum_dashboard", payload.ViewType);
        Assert.Equal(8, payload.CoerciveModeCount);
        Assert.Equal(1, payload.NegativeModeCount);
        Assert.Empty(payload.Values);
    }

    [Fact]
    public void PanelId_IsCorrect()
    {
        var dashboard = new SpectrumDashboard();
        Assert.Equal("spectrum-dashboard", dashboard.PanelId);
        Assert.Equal("Hessian Spectrum Dashboard", dashboard.Title);
    }
}

public class ContinuationPathViewTests
{
    [Fact]
    public void Prepare_ProducesPayloadFromContinuationResult()
    {
        var steps = new[]
        {
            new ContinuationStep
            {
                StepIndex = 0, Lambda = 0.0, Arclength = 0.0, StepSize = 0.1,
                ResidualNorm = 1.0, CorrectorIterations = 3, CorrectorConverged = true,
                Events = Array.Empty<ContinuationEvent>(),
            },
            new ContinuationStep
            {
                StepIndex = 1, Lambda = 0.5, Arclength = 0.5, StepSize = 0.1,
                ResidualNorm = 0.1, CorrectorIterations = 5, CorrectorConverged = true,
                Events = Array.Empty<ContinuationEvent>(),
            },
            new ContinuationStep
            {
                StepIndex = 2, Lambda = 1.0, Arclength = 1.0, StepSize = 0.1,
                ResidualNorm = 0.01, CorrectorIterations = 2, CorrectorConverged = true,
                Events = Array.Empty<ContinuationEvent>(),
            },
        };

        var result = new ContinuationResult
        {
            Spec = new ContinuationSpec
            {
                ParameterName = "lambda",
                LambdaStart = 0, LambdaEnd = 1,
                InitialStepSize = 0.1, MaxSteps = 100,
                MinStepSize = 0.01, MaxStepSize = 0.5,
                CorrectorTolerance = 1e-8, MaxCorrectorIterations = 20,
                BranchManifestId = "branch-1",
            },
            Path = steps,
            TerminationReason = "reached-end",
            TotalSteps = 3,
            TotalArclength = 1.0,
            LambdaMin = 0.0,
            LambdaMax = 1.0,
            AllEvents = Array.Empty<ContinuationEvent>(),
            Timestamp = DateTimeOffset.UtcNow,
        };

        var view = new ContinuationPathView();
        var payload = view.Prepare(result);

        Assert.Equal("continuation_path", payload.ViewType);
        Assert.Equal("lambda", payload.ParameterName);
        Assert.Equal(3, payload.TotalSteps);
        Assert.Equal("reached-end", payload.TerminationReason);
        Assert.Equal(0.0, payload.LambdaMin);
        Assert.Equal(1.0, payload.LambdaMax);
        Assert.Equal(3, payload.Series.Count);
        Assert.Equal("Residual Norm", payload.Series[0].Label);
        Assert.Equal(3, payload.Series[0].X.Length);
    }

    [Fact]
    public void PanelId_IsCorrect()
    {
        var view = new ContinuationPathView();
        Assert.Equal("continuation-path", view.PanelId);
    }
}

public class CanonicityHeatmapTests
{
    [Fact]
    public void Prepare_FlattensDistanceMatrix()
    {
        var distances = new double[3, 3];
        distances[0, 1] = 0.5;
        distances[1, 0] = 0.5;
        distances[0, 2] = 1.0;
        distances[2, 0] = 1.0;
        distances[1, 2] = 0.3;
        distances[2, 1] = 0.3;

        var matrix = new PairwiseDistanceMatrix
        {
            MetricId = "D_obs_max",
            BranchIds = new[] { "v1", "v2", "v3" },
            Distances = distances,
        };

        var heatmap = new CanonicityHeatmap();
        var payload = heatmap.Prepare(matrix);

        Assert.Equal("canonicity_heatmap", payload.ViewType);
        Assert.Equal("D_obs_max", payload.MetricId);
        Assert.Equal(3, payload.Dimension);
        Assert.Equal(9, payload.FlatDistances.Length);
        Assert.Equal(1.0, payload.MaxDistance);

        // Check flattened row-major layout
        Assert.Equal(0.0, payload.FlatDistances[0]); // [0,0]
        Assert.Equal(0.5, payload.FlatDistances[1]); // [0,1]
        Assert.Equal(1.0, payload.FlatDistances[2]); // [0,2]
        Assert.Equal(0.5, payload.FlatDistances[3]); // [1,0]
    }

    [Fact]
    public void PanelId_IsCorrect()
    {
        var heatmap = new CanonicityHeatmap();
        Assert.Equal("canonicity-heatmap", heatmap.PanelId);
    }
}

public class BranchOverlayViewTests
{
    [Fact]
    public void PanelId_IsCorrect()
    {
        var view = new BranchOverlayView();
        Assert.Equal("branch-overlay", view.PanelId);
        Assert.Equal("Branch Overlay (D_obs)", view.Title);
    }

    [Fact]
    public void Prepare_NullObservedState_FaceDiffsAreNaN()
    {
        var left = MakeRunRecord("v1", null);
        var right = MakeRunRecord("v2", null);
        var mesh = MakeTriangleMesh();

        var view = new BranchOverlayView();
        var payload = view.Prepare(left, right, mesh);

        Assert.Equal("branch_overlay", payload.ViewType);
        Assert.Equal("v1", payload.LeftBranchId);
        Assert.Equal("v2", payload.RightBranchId);
        Assert.All(payload.FaceDifferences, d => Assert.True(double.IsNaN(d)));
        Assert.Equal(0.0, payload.MaxDistance);
    }

    private static BranchRunRecord MakeRunRecord(string variantId, ObservedState? observedState)
    {
        var variant = new BranchVariantManifest
        {
            Id = variantId,
            ParentFamilyId = "fam-1",
            A0Variant = "zero",
            BiConnectionVariant = "simple",
            TorsionVariant = "augmented",
            ShiabVariant = "identity",
            ObservationVariant = "default",
            ExtractionVariant = "default",
            GaugeVariant = "coulomb",
            RegularityVariant = "smooth",
            PairingVariant = "trace",
            ExpectedClaimCeiling = "branch-local-numerical",
        };

        var manifest = new BranchManifest
        {
            BranchId = variantId,
            SchemaVersion = "1.0",
            SourceEquationRevision = "rev-1",
            CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-2d",
            ActiveObservationBranch = "default",
            ActiveTorsionBranch = "augmented",
            ActiveShiabBranch = "identity",
            ActiveGaugeStrategy = "coulomb",
            BaseDimension = 4,
            AmbientDimension = 14,
            LieAlgebraId = "su2",
            BasisConventionId = "canonical",
            ComponentOrderId = "edge-major",
            AdjointConventionId = "default",
            PairingConventionId = "trace",
            NormConventionId = "l2",
            DifferentialFormMetricId = "euclidean",
            InsertedAssumptionIds = Array.Empty<string>(),
            InsertedChoiceIds = Array.Empty<string>(),
        };

        var branchRef = new BranchRef { BranchId = variantId, SchemaVersion = "1.0" };
        return new BranchRunRecord
        {
            Variant = variant,
            Manifest = manifest,
            Converged = true,
            TerminationReason = "converged",
            FinalObjective = 1e-10,
            FinalResidualNorm = 1e-8,
            Iterations = 50,
            SolveMode = SolveMode.ObjectiveMinimization,
            ObservedState = observedState,
            ExtractionSucceeded = observedState != null,
            ComparisonAdmissible = observedState != null,
            ArtifactBundle = new ArtifactBundle
            {
                ArtifactId = $"art-{variantId}",
                Branch = branchRef,
                ReplayContract = new ReplayContract
                {
                    BranchManifest = manifest,
                    Deterministic = true,
                    BackendId = "cpu-reference",
                    ReplayTier = "R2",
                },
                Provenance = new ProvenanceMeta
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    CodeRevision = "abc123",
                    Branch = branchRef,
                },
                CreatedAt = DateTimeOffset.UtcNow,
            },
        };
    }

    private static SimplicialMesh MakeTriangleMesh()
    {
        // Simple triangle mesh: 3 vertices, 3 edges, 1 face
        return new SimplicialMesh
        {
            EmbeddingDimension = 2,
            SimplicialDimension = 2,
            VertexCount = 3,
            VertexCoordinates = new double[] { 0, 0, 1, 0, 0.5, 1 },
            CellVertices = new[] { new[] { 0, 1, 2 } },
            Edges = new[] { new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 2 } },
            Faces = new[] { new[] { 0, 1, 2 } },
            CellEdges = new[] { new[] { 0, 1, 2 } },
            CellFaces = new[] { new[] { 0 } },
            FaceBoundaryEdges = new[] { new[] { 0, 2, 1 } },
            FaceBoundaryOrientations = new[] { new[] { 1, 1, -1 } },
            VertexEdges = new[] { new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 2 } },
            VertexEdgeOrientations = new[] { new[] { 1, 1 }, new[] { -1, 1 }, new[] { -1, -1 } },
        };
    }
}
