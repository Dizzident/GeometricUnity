using Gu.Core;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Continuation;
using Gu.Phase2.Execution;
using Gu.Phase2.Recovery;
using Gu.Phase2.Reporting;
using Gu.Phase2.Semantics;
using Gu.Solvers;

namespace Gu.Phase2.Reporting.Tests;

public class ResearchBatchRunnerTests
{
    [Fact]
    public void Run_EmptyBatch_ReturnsEmptyResults()
    {
        var runner = new ResearchBatchRunner(
            _ => throw new InvalidOperationException("Should not be called"),
            _ => throw new InvalidOperationException("Should not be called"));

        var spec = MakeBatchSpec("batch-empty", sweeps: [], stabilityStudies: [], campaignIds: []);
        var result = runner.Run(spec);

        Assert.Same(spec, result.Spec);
        Assert.Empty(result.SweepResults);
        Assert.Empty(result.StabilityResults);
        Assert.Empty(result.ExecutedCampaignIds);
        Assert.True(result.BatchStarted <= result.BatchCompleted);
    }

    [Fact]
    public void Run_ExecutesSweepsInOrder()
    {
        var callOrder = new List<string>();
        var sweepResult = MakeSweepResult(convergedCount: 1);

        var runner = new ResearchBatchRunner(
            spec =>
            {
                callOrder.Add(spec.StudyId);
                return sweepResult;
            },
            _ => throw new InvalidOperationException("Should not be called"));

        var sweeps = new[]
        {
            MakeSweepSpec("sweep-1"),
            MakeSweepSpec("sweep-2"),
        };
        var spec = MakeBatchSpec("batch-sweeps", sweeps: sweeps, stabilityStudies: [], campaignIds: []);
        var result = runner.Run(spec);

        Assert.Equal(2, result.SweepResults.Count);
        Assert.Equal(["sweep-1", "sweep-2"], callOrder);
        Assert.Same(sweepResult, result.SweepResults["sweep-1"]);
        Assert.Same(sweepResult, result.SweepResults["sweep-2"]);
    }

    [Fact]
    public void Run_ExecutesStabilityStudiesInOrder()
    {
        var callOrder = new List<string>();
        var contResult = MakeContinuationResult(stepCount: 3);

        var runner = new ResearchBatchRunner(
            _ => throw new InvalidOperationException("Should not be called"),
            spec =>
            {
                callOrder.Add(spec.StudyId);
                return contResult;
            });

        var studies = new[]
        {
            MakeStabilitySpec("stab-1"),
            MakeStabilitySpec("stab-2"),
        };
        var spec = MakeBatchSpec("batch-stability", sweeps: [], stabilityStudies: studies, campaignIds: []);
        var result = runner.Run(spec);

        Assert.Equal(2, result.StabilityResults.Count);
        Assert.Equal(["stab-1", "stab-2"], callOrder);
    }

    [Fact]
    public void Run_RecordsComparisonCampaignIds()
    {
        var runner = new ResearchBatchRunner(_ => MakeSweepResult(0), _ => MakeContinuationResult(0));

        var spec = MakeBatchSpec("batch-campaigns", sweeps: [], stabilityStudies: [],
            campaignIds: ["campaign-a", "campaign-b"]);
        var result = runner.Run(spec);

        Assert.Equal(["campaign-a", "campaign-b"], result.ExecutedCampaignIds);
    }

    [Fact]
    public void Run_MixedBatch_ExecutesSweepsThenStability()
    {
        var executionOrder = new List<string>();
        var sweepResult = MakeSweepResult(1);
        var contResult = MakeContinuationResult(2);

        var runner = new ResearchBatchRunner(
            spec =>
            {
                executionOrder.Add($"sweep:{spec.StudyId}");
                return sweepResult;
            },
            spec =>
            {
                executionOrder.Add($"stability:{spec.StudyId}");
                return contResult;
            });

        var spec = MakeBatchSpec("batch-mixed",
            sweeps: [MakeSweepSpec("s1")],
            stabilityStudies: [MakeStabilitySpec("st1")],
            campaignIds: ["c1"]);
        var result = runner.Run(spec);

        Assert.Equal(["sweep:s1", "stability:st1"], executionOrder);
        Assert.Single(result.SweepResults);
        Assert.Single(result.StabilityResults);
        Assert.Single(result.ExecutedCampaignIds);
    }

    [Fact]
    public void Run_NullSpec_Throws()
    {
        var runner = new ResearchBatchRunner(_ => MakeSweepResult(0), _ => MakeContinuationResult(0));
        Assert.Throws<ArgumentNullException>(() => runner.Run(null!));
    }

    [Fact]
    public void Constructor_NullSweepExecutor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ResearchBatchRunner(null!, _ => MakeContinuationResult(0)));
    }

    [Fact]
    public void Constructor_NullStabilityExecutor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new ResearchBatchRunner(_ => MakeSweepResult(0), null!));
    }

    [Fact]
    public void AllSucceeded_TrueWhenAllHaveRecords()
    {
        var runner = new ResearchBatchRunner(
            _ => MakeSweepResult(convergedCount: 1),
            _ => MakeContinuationResult(stepCount: 2));

        var spec = MakeBatchSpec("batch-success",
            sweeps: [MakeSweepSpec("s1")],
            stabilityStudies: [MakeStabilitySpec("st1")],
            campaignIds: []);
        var result = runner.Run(spec);

        Assert.True(result.AllSucceeded);
    }

    [Fact]
    public void AllSucceeded_FalseWhenSweepHasNoRecords()
    {
        var runner = new ResearchBatchRunner(
            _ => MakeSweepResult(convergedCount: 0, totalBranches: 0),
            _ => MakeContinuationResult(stepCount: 2));

        var spec = MakeBatchSpec("batch-fail",
            sweeps: [MakeSweepSpec("s1")],
            stabilityStudies: [MakeStabilitySpec("st1")],
            campaignIds: []);
        var result = runner.Run(spec);

        Assert.False(result.AllSucceeded);
    }

    [Fact]
    public void TotalBranchRuns_SumsAcrossSweeps()
    {
        int callCount = 0;
        var runner = new ResearchBatchRunner(
            _ =>
            {
                callCount++;
                return MakeSweepResult(convergedCount: callCount, totalBranches: callCount + 1);
            },
            _ => MakeContinuationResult(0));

        var spec = MakeBatchSpec("batch-count",
            sweeps: [MakeSweepSpec("s1"), MakeSweepSpec("s2")],
            stabilityStudies: [],
            campaignIds: []);
        var result = runner.Run(spec);

        // s1: 2 branches, s2: 3 branches = 5 total
        Assert.Equal(5, result.TotalBranchRuns);
    }

    [Fact]
    public void Run_ExecutesRecoveryStudiesInOrder()
    {
        var callOrder = new List<string>();
        var recoveryResult = new List<PhysicalIdentificationRecord>
        {
            new()
            {
                IdentificationId = "pid-1",
                FormalSource = "native-state-1",
                ObservationExtractionMap = "sigma-pullback-standard",
                SupportStatus = "numerical-only",
                ApproximationStatus = "leading-order",
                ComparisonTarget = "mass-ratio",
                Falsifier = "mass-ratio > 10",
                ResolvedClaimClass = ClaimClass.PostdictionTarget,
            },
        };

        var runner = new ResearchBatchRunner(
            _ => MakeSweepResult(0),
            _ => MakeContinuationResult(0),
            spec =>
            {
                callOrder.Add(spec.StudyId);
                return recoveryResult;
            });

        var recoveryStudies = new[]
        {
            MakeRecoverySpec("recovery-1"),
            MakeRecoverySpec("recovery-2"),
        };
        var spec = MakeBatchSpec("batch-recovery",
            sweeps: [], stabilityStudies: [], campaignIds: [],
            recoveryStudies: recoveryStudies);
        var result = runner.Run(spec);

        Assert.Equal(2, result.RecoveryResults.Count);
        Assert.Equal(["recovery-1", "recovery-2"], callOrder);
        Assert.Same(recoveryResult, result.RecoveryResults["recovery-1"]);
    }

    [Fact]
    public void Run_NoRecoveryExecutor_SkipsRecoveryStudies()
    {
        var runner = new ResearchBatchRunner(
            _ => MakeSweepResult(0),
            _ => MakeContinuationResult(0));

        var spec = MakeBatchSpec("batch-no-recovery",
            sweeps: [], stabilityStudies: [], campaignIds: [],
            recoveryStudies: [MakeRecoverySpec("recovery-1")]);
        var result = runner.Run(spec);

        Assert.Empty(result.RecoveryResults);
    }

    // --- Helpers ---

    private static RecoveryStudySpec MakeRecoverySpec(string studyId) => new()
    {
        StudyId = studyId,
        SweepResultId = "sweep-result-1",
        RecoveryGraphId = "graph-1",
        EnforceIdentificationGate = true,
        MaxAllowedClaimClass = "numerical-only",
    };

    private static ResearchBatchSpec MakeBatchSpec(
        string batchId,
        IReadOnlyList<BranchSweepSpec> sweeps,
        IReadOnlyList<StabilityStudySpec> stabilityStudies,
        IReadOnlyList<string> campaignIds,
        IReadOnlyList<RecoveryStudySpec>? recoveryStudies = null) => new()
    {
        BatchId = batchId,
        Sweeps = sweeps,
        StabilityStudies = stabilityStudies,
        RecoveryStudies = recoveryStudies ?? [],
        ComparisonCampaignIds = campaignIds,
    };

    private static BranchSweepSpec MakeSweepSpec(string studyId) => new()
    {
        StudyId = studyId,
        EnvironmentId = "env-test",
        FamilyId = "fam-test",
        VariantIds = ["v1"],
        InnerSolveMode = "ResidualOnly",
        Equivalence = new EquivalenceSpec
        {
            Id = "eq-1",
            Name = "test-eq",
            ComparedObjectClasses = ["residual-norm"],
            NormalizationProcedure = "none",
            AllowedTransformations = [],
            Metrics = ["l2"],
            Tolerances = new Dictionary<string, double> { ["l2"] = 1e-6 },
            InterpretationRule = "all-below-tolerance",
        },
    };

    private static StabilityStudySpec MakeStabilitySpec(string studyId) => new()
    {
        StudyId = studyId,
        BackgroundStateId = "bg-1",
        BranchManifestId = "manifest-1",
        GaugeHandlingMode = "gauge-free",
        RequestedModeCount = 6,
        SpectrumProbeMethod = "lanczos",
        SoftModeThreshold = 1e-3,
        NearKernelThreshold = 1e-6,
        NegativeModeThreshold = -1e-10,
    };

    internal static Phase2BranchSweepResult MakeSweepResult(int convergedCount, int totalBranches = -1)
    {
        if (totalBranches < 0) totalBranches = convergedCount;

        var records = new List<BranchRunRecord>();
        for (int i = 0; i < totalBranches; i++)
        {
            records.Add(MakeBranchRunRecord($"v-{i}", converged: i < convergedCount));
        }

        return new Phase2BranchSweepResult
        {
            Family = new BranchFamilyManifest
            {
                FamilyId = "fam-test",
                Description = "test family",
                Variants = [],
                DefaultEquivalence = new EquivalenceSpec
                {
                    Id = "eq-1", Name = "test", ComparedObjectClasses = [], NormalizationProcedure = "none",
                    AllowedTransformations = [], Metrics = [], Tolerances = new Dictionary<string, double>(),
                    InterpretationRule = "none",
                },
                CreatedAt = DateTimeOffset.UtcNow,
            },
            EnvironmentId = "env-test",
            RunRecords = records,
            InnerMode = SolveMode.ResidualOnly,
            SweepStarted = DateTimeOffset.UtcNow,
            SweepCompleted = DateTimeOffset.UtcNow,
        };
    }

    private static BranchRunRecord MakeBranchRunRecord(string variantId, bool converged)
    {
        var manifest = new BranchManifest
        {
            BranchId = variantId,
            SchemaVersion = "1.0.0",
            SourceEquationRevision = "rev-1",
            CodeRevision = "abc123",
            ActiveGeometryBranch = "simplicial-4d",
            ActiveObservationBranch = "sigma-pullback",
            ActiveTorsionBranch = "local-algebraic",
            ActiveShiabBranch = "identity-shiab",
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
            InsertedAssumptionIds = ["IA-1"],
            InsertedChoiceIds = ["IX-1"],
        };

        return new BranchRunRecord
        {
            Variant = new BranchVariantManifest
            {
                Id = variantId,
                ParentFamilyId = "fam-test",
                A0Variant = "flat",
                BiConnectionVariant = "simple-a0-omega",
                TorsionVariant = "local-algebraic",
                ShiabVariant = "identity-shiab",
                ObservationVariant = "sigma-pullback",
                ExtractionVariant = "standard",
                GaugeVariant = "penalty",
                RegularityVariant = "sobolev-h1",
                PairingVariant = "killing",
                ExpectedClaimCeiling = "numerical-only",
            },
            Manifest = manifest,
            Converged = converged,
            TerminationReason = converged ? "converged" : "max-iterations",
            FinalObjective = converged ? 1e-10 : 1.0,
            FinalResidualNorm = converged ? 1e-10 : 1.0,
            Iterations = 10,
            SolveMode = SolveMode.ResidualOnly,
            ExtractionSucceeded = false,
            ComparisonAdmissible = false,
            ArtifactBundle = new ArtifactBundle
            {
                ArtifactId = $"art-{variantId}",
                Branch = new BranchRef { BranchId = variantId, SchemaVersion = "1.0.0" },
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
                    Branch = new BranchRef { BranchId = variantId, SchemaVersion = "1.0.0" },
                    Backend = "cpu-reference",
                },
                CreatedAt = DateTimeOffset.UtcNow,
            },
        };
    }

    internal static ContinuationResult MakeContinuationResult(
        int stepCount,
        IReadOnlyList<ContinuationEvent>? events = null)
    {
        var steps = new List<ContinuationStep>();
        for (int i = 0; i < stepCount; i++)
        {
            steps.Add(new ContinuationStep
            {
                StepIndex = i,
                Lambda = 0.1 * i,
                Arclength = 0.1 * i,
                StepSize = 0.1,
                ResidualNorm = 1e-8,
                CorrectorIterations = 3,
                CorrectorConverged = true,
                Events = [],
            });
        }

        return new ContinuationResult
        {
            Spec = new ContinuationSpec
            {
                ParameterName = "gauge-lambda",
                LambdaStart = 0.0,
                LambdaEnd = 1.0,
                InitialStepSize = 0.1,
                MaxSteps = 100,
                MinStepSize = 1e-6,
                MaxStepSize = 0.5,
                CorrectorTolerance = 1e-8,
                MaxCorrectorIterations = 20,
                BranchManifestId = "manifest-1",
            },
            Path = steps,
            TerminationReason = stepCount > 0 ? "reached-end" : "max-steps",
            TotalSteps = stepCount,
            TotalArclength = 0.1 * stepCount,
            LambdaMin = 0.0,
            LambdaMax = 0.1 * stepCount,
            AllEvents = events ?? [],
            Timestamp = DateTimeOffset.UtcNow,
        };
    }
}
