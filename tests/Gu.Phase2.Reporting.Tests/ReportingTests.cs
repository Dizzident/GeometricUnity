using Gu.Core;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Continuation;
using Gu.Phase2.Execution;
using Gu.Phase2.Recovery;
using Gu.Phase2.Reporting;
using Gu.Phase2.Reporting.Artifacts;
using Gu.Phase2.Semantics;
using Gu.Solvers;

namespace Gu.Phase2.Reporting.Tests;

public class ReportingTests
{
    private static EquivalenceSpec DummyEquivalence() => new()
    {
        Id = "eq-test",
        Name = "Test Equivalence",
        ComparedObjectClasses = ["observed-output"],
        NormalizationProcedure = "l2-normalize",
        AllowedTransformations = [],
        Metrics = ["l2-distance"],
        Tolerances = new Dictionary<string, double> { ["l2-distance"] = 1e-6 },
        InterpretationRule = "all-below-tolerance",
    };

    private static BranchSweepSpec DummySweepSpec(string studyId = "sweep-1") => new()
    {
        StudyId = studyId,
        EnvironmentId = "env-test",
        FamilyId = "family-test",
        VariantIds = ["v1", "v2"],
        InnerSolveMode = "ObjectiveMinimization",
        Equivalence = DummyEquivalence(),
    };

    private static BranchManifest DummyManifest(string branchId = "branch-v1") => new()
    {
        BranchId = branchId,
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "r1",
        CodeRevision = "test",
        ActiveGeometryBranch = "simplicial",
        ActiveObservationBranch = "sigma-pullback",
        ActiveTorsionBranch = "trivial",
        ActiveShiabBranch = "identity-shiab",
        ActiveGaugeStrategy = "coulomb",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "hodge-standard",
        InsertedAssumptionIds = [],
        InsertedChoiceIds = ["IX-1"],
    };

    private static BranchVariantManifest DummyVariant(string id = "v1") => new()
    {
        Id = id,
        ParentFamilyId = "family-test",
        A0Variant = "flat",
        BiConnectionVariant = "simple-a0-omega",
        TorsionVariant = "trivial",
        ShiabVariant = "identity-shiab",
        ObservationVariant = "sigma-pullback",
        ExtractionVariant = "standard",
        GaugeVariant = "coulomb",
        RegularityVariant = "sobolev-h1",
        PairingVariant = "killing",
        ExpectedClaimCeiling = "numerical-only",
    };

    private static Phase2BranchSweepResult DummySweepResult(bool allConverged = true)
    {
        var variant1 = DummyVariant("v1");
        var manifest1 = DummyManifest("branch-v1");
        var family = new BranchFamilyManifest
        {
            FamilyId = "family-test",
            Description = "Test Family",
            Variants = [variant1],
            DefaultEquivalence = DummyEquivalence(),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        var records = new List<BranchRunRecord>
        {
            new()
            {
                Variant = variant1,
                Manifest = manifest1,
                Converged = allConverged,
                TerminationReason = allConverged ? "converged" : "max-iterations",
                FinalObjective = allConverged ? 1e-12 : 1.0,
                FinalResidualNorm = allConverged ? 1e-8 : 0.5,
                Iterations = 10,
                SolveMode = SolveMode.ObjectiveMinimization,
                ExtractionSucceeded = false,
                ComparisonAdmissible = false,
                ArtifactBundle = new ArtifactBundle
                {
                    ArtifactId = "art-v1",
                    Branch = new BranchRef { BranchId = "branch-v1", SchemaVersion = "1.0.0" },
                    ReplayContract = new ReplayContract
                    {
                        BranchManifest = manifest1,
                        Deterministic = true,
                        BackendId = "cpu-reference",
                        ReplayTier = "R2",
                    },
                    Provenance = new ProvenanceMeta
                    {
                        CreatedAt = DateTimeOffset.UtcNow,
                        CodeRevision = "test",
                        Branch = new BranchRef { BranchId = "branch-v1", SchemaVersion = "1.0.0" },
                        Backend = "cpu-reference",
                    },
                    CreatedAt = DateTimeOffset.UtcNow,
                },
            },
        };

        return new Phase2BranchSweepResult
        {
            Family = family,
            EnvironmentId = "env-test",
            RunRecords = records,
            InnerMode = SolveMode.ObjectiveMinimization,
            SweepStarted = DateTimeOffset.UtcNow,
            SweepCompleted = DateTimeOffset.UtcNow,
        };
    }

    private static ContinuationResult DummyContinuationResult(bool withEvents = false)
    {
        var events = withEvents
            ? new List<ContinuationEvent>
            {
                new()
                {
                    Kind = ContinuationEventKind.HessianSignChange,
                    Lambda = 0.5,
                    Description = "Fold detected",
                    Severity = "warning",
                },
            }
            : new List<ContinuationEvent>();

        return new ContinuationResult
        {
            Spec = new ContinuationSpec
            {
                ParameterName = "gauge-lambda",
                LambdaStart = 0.0,
                LambdaEnd = 1.0,
                InitialStepSize = 0.1,
                MaxSteps = 20,
                MinStepSize = 0.001,
                MaxStepSize = 0.5,
                CorrectorTolerance = 1e-8,
                MaxCorrectorIterations = 20,
                BranchManifestId = "branch-test",
            },
            Path = new List<ContinuationStep>
            {
                new()
                {
                    StepIndex = 0,
                    Lambda = 0.0,
                    Arclength = 0.0,
                    ResidualNorm = 1e-10,
                    CorrectorIterations = 3,
                    CorrectorConverged = true,
                    StepSize = 0.1,
                    Events = [],
                },
                new()
                {
                    StepIndex = 1,
                    Lambda = 0.1,
                    Arclength = 0.1,
                    ResidualNorm = 1e-9,
                    CorrectorIterations = 4,
                    CorrectorConverged = true,
                    StepSize = 0.1,
                    Events = events,
                },
            },
            TerminationReason = "reached-end",
            TotalSteps = 2,
            TotalArclength = 0.2,
            LambdaMin = 0.0,
            LambdaMax = 0.1,
            AllEvents = events,
            Timestamp = DateTimeOffset.UtcNow,
        };
    }

    private static ResearchBatchSpec DummyBatchSpec() => new()
    {
        BatchId = "batch-test",
        Sweeps = [DummySweepSpec()],
        StabilityStudies =
        [
            new StabilityStudySpec
            {
                StudyId = "stability-1",
                BackgroundStateId = "bg-flat",
                BranchManifestId = "branch-test",
                GaugeHandlingMode = "coulomb-slice",
                RequestedModeCount = 5,
                SpectrumProbeMethod = "lanczos",
                SoftModeThreshold = 1e-4,
                NearKernelThreshold = 1e-8,
                NegativeModeThreshold = -1e-8,
            },
        ],
        RecoveryStudies = [],
        ComparisonCampaignIds = ["campaign-1"],
    };

    private static ResearchBatchResult DummyBatchResult(
        bool sweepConverged = true, bool withEvents = false)
    {
        return new ResearchBatchResult
        {
            Spec = DummyBatchSpec(),
            SweepResults = new Dictionary<string, Phase2BranchSweepResult>
            {
                ["sweep-1"] = DummySweepResult(sweepConverged),
            },
            StabilityResults = new Dictionary<string, ContinuationResult>
            {
                ["stability-1"] = DummyContinuationResult(withEvents),
            },
            RecoveryResults = new Dictionary<string, IReadOnlyList<PhysicalIdentificationRecord>>(),
            ExecutedCampaignIds = ["campaign-1"],
            BatchStarted = DateTimeOffset.UtcNow.AddMinutes(-5),
            BatchCompleted = DateTimeOffset.UtcNow,
        };
    }

    [Fact]
    public void ResearchReportGenerator_Produces_ValidReport()
    {
        var generator = new ResearchReportGenerator(_ => []);
        var result = DummyBatchResult();
        var report = generator.Generate(result);

        Assert.Equal("report-batch-test", report.ReportId);
        Assert.Equal("batch-test", report.BatchId);
        Assert.NotEmpty(report.BranchLocalConclusions);
        Assert.NotNull(report.Dockets);
    }

    [Fact]
    public void Report_AllConverged_ClassifiedAsBranchLocal()
    {
        var generator = new ResearchReportGenerator(_ => []);
        var result = DummyBatchResult(sweepConverged: true);
        var report = generator.Generate(result);

        Assert.True(report.BranchLocalConclusions.Count >= 1);
        Assert.Contains(report.BranchLocalConclusions, r => r.Summary.Contains("converged"));
    }

    [Fact]
    public void Report_NoConvergence_ClassifiedAsRuledOut()
    {
        var generator = new ResearchReportGenerator(_ => []);
        var result = DummyBatchResult(sweepConverged: false);
        var report = generator.Generate(result);

        Assert.True(report.RuledOutClaims.Count >= 1);
        Assert.Contains(report.RuledOutClaims, r => r.Summary.Contains("no branches converged"));
    }

    [Fact]
    public void Report_StabilityWithEvents_ClassifiedAsUninterpreted()
    {
        var generator = new ResearchReportGenerator(_ => []);
        var result = DummyBatchResult(withEvents: true);
        var report = generator.Generate(result);

        Assert.True(report.UninterpretedOutputs.Count >= 1);
        Assert.Contains(report.UninterpretedOutputs, r => r.Summary.Contains("events require interpretation"));
    }

    [Fact]
    public void Report_CrossStudy_ComparisonReady()
    {
        var generator = new ResearchReportGenerator(_ => []);
        var result = DummyBatchResult();
        var report = generator.Generate(result);

        Assert.True(report.ComparisonReadyConclusions.Count >= 1);
    }

    [Fact]
    public void DashboardExport_FromReport_ProducesCounts()
    {
        var generator = new ResearchReportGenerator(_ => []);
        var batchResult = DummyBatchResult();
        var report = generator.Generate(batchResult);
        var dashboard = DashboardExport.FromReport(report, batchResult);

        Assert.Equal(report.ReportId, dashboard.ReportId);
        Assert.Equal("batch-test", dashboard.BatchId);
        Assert.True(dashboard.CategoryCounts.BranchLocal >= 1);
        Assert.Equal(1, dashboard.TotalBranchRuns);
    }

    [Fact]
    public void DashboardExport_ToJson_IsValidJson()
    {
        var generator = new ResearchReportGenerator(_ => []);
        var batchResult = DummyBatchResult();
        var report = generator.Generate(batchResult);
        var dashboard = DashboardExport.FromReport(report, batchResult);

        var json = dashboard.ToJson();
        Assert.Contains("reportId", json);
        Assert.Contains("categoryCounts", json);
        Assert.Contains("batch-test", json);
    }

    [Fact]
    public void ReportArtifactManifest_TotalCount_SumsAllCategories()
    {
        var manifest = new ReportArtifactManifest
        {
            SweepArtifacts = [],
            CanonicityArtifacts = [],
            StabilityArtifacts = [],
            ContinuationArtifacts = [],
            RecoveryArtifacts = [],
            PredictionArtifacts = [],
            ComparisonArtifacts = [],
        };

        Assert.Equal(0, manifest.TotalCount);
    }

    [Fact]
    public void ResearchBatchRunner_Executes_AllStudies()
    {
        int sweepCalls = 0;
        int stabilityCalls = 0;

        var runner = new ResearchBatchRunner(
            _ =>
            {
                sweepCalls++;
                return DummySweepResult();
            },
            _ =>
            {
                stabilityCalls++;
                return DummyContinuationResult();
            });

        var batchResult = runner.Run(DummyBatchSpec());

        Assert.Equal(1, sweepCalls);
        Assert.Equal(1, stabilityCalls);
        Assert.Single(batchResult.SweepResults);
        Assert.Single(batchResult.StabilityResults);
        Assert.True(batchResult.AllSucceeded);
    }
}
