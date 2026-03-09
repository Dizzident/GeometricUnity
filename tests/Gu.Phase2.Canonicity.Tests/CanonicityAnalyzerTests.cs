using Gu.Core;
using Gu.Phase2.Branches;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Execution;
using Gu.Phase2.Semantics;
using Gu.Phase2.Stability;
using Gu.Solvers;

namespace Gu.Phase2.Canonicity.Tests;

public class CanonicityAnalyzerTests
{
    private static readonly EquivalenceSpec DefaultEquivalence = new()
    {
        Id = "eq-test",
        Name = "Test Equivalence",
        ComparedObjectClasses = new[] { "observed-output" },
        NormalizationProcedure = "none",
        AllowedTransformations = Array.Empty<string>(),
        Metrics = new[] { "D_obs" },
        Tolerances = new Dictionary<string, double> { ["D_obs"] = 1e-6 },
        InterpretationRule = "all-within-tolerance",
    };

    private static BranchVariantManifest MakeVariant(string id) => new()
    {
        Id = id,
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

    private static BranchManifest MakeManifest(string branchId) => new()
    {
        BranchId = branchId,
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

    private static ObservedState MakeObservedState(double[] values) => new()
    {
        ObservationBranchId = "default",
        Observables = new Dictionary<string, ObservableSnapshot>
        {
            ["residual-norm"] = new ObservableSnapshot
            {
                ObservableId = "residual-norm",
                OutputType = OutputType.Quantitative,
                Values = values,
                Provenance = new ObservationProvenance
                {
                    PullbackOperatorId = "test-pullback",
                    ObservationBranchId = "default",
                    IsVerified = true,
                    PipelineTimestamp = DateTimeOffset.UtcNow,
                },
            },
        },
        Provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CodeRevision = "abc123",
            Branch = new BranchRef { BranchId = "test", SchemaVersion = "1.0" },
        },
    };

    private static ArtifactBundle MakeArtifact(string variantId) => new()
    {
        ArtifactId = $"art-{variantId}",
        Branch = new BranchRef { BranchId = variantId, SchemaVersion = "1.0" },
        ReplayContract = new ReplayContract
        {
            BranchManifest = MakeManifest(variantId),
            Deterministic = true,
            BackendId = "cpu-reference",
            ReplayTier = "R2",
        },
        Provenance = new ProvenanceMeta
        {
            CreatedAt = DateTimeOffset.UtcNow,
            CodeRevision = "abc123",
            Branch = new BranchRef { BranchId = variantId, SchemaVersion = "1.0" },
        },
        CreatedAt = DateTimeOffset.UtcNow,
    };

    private static BranchRunRecord MakeRunRecord(
        string variantId,
        bool converged,
        double objective,
        double residualNorm,
        int iterations,
        string terminationReason = "converged",
        ObservedState? observedState = null,
        bool? extractionSucceeded = null,
        bool? comparisonAdmissible = null,
        HessianSummary? stabilityDiagnostics = null) => new()
    {
        Variant = MakeVariant(variantId),
        Manifest = MakeManifest(variantId),
        Converged = converged,
        TerminationReason = terminationReason,
        FinalObjective = objective,
        FinalResidualNorm = residualNorm,
        Iterations = iterations,
        SolveMode = SolveMode.ObjectiveMinimization,
        ObservedState = observedState,
        ExtractionSucceeded = extractionSucceeded ?? (observedState != null),
        ComparisonAdmissible = comparisonAdmissible ?? (observedState != null && observedState.Observables.Count > 0),
        StabilityDiagnostics = stabilityDiagnostics,
        ArtifactBundle = MakeArtifact(variantId),
    };

    private static BranchFamilyManifest MakeFamily(params string[] variantIds) => new()
    {
        FamilyId = "fam-1",
        Description = "Test family",
        Variants = variantIds.Select(MakeVariant).ToList(),
        DefaultEquivalence = DefaultEquivalence,
        CreatedAt = DateTimeOffset.UtcNow,
    };

    private static Phase2BranchSweepResult MakeSweep(
        params BranchRunRecord[] records) => new()
    {
        Family = MakeFamily(records.Select(r => r.Variant.Id).ToArray()),
        EnvironmentId = "env-test",
        RunRecords = records.ToList(),
        InnerMode = SolveMode.ObjectiveMinimization,
        SweepStarted = DateTimeOffset.UtcNow.AddMinutes(-1),
        SweepCompleted = DateTimeOffset.UtcNow,
    };

    // --- Pairwise Distance Matrix Tests ---

    [Fact]
    public void ObservedDistances_IdenticalBranches_AllZero()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0, 3.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, observedState: obs));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeObservedDistances(sweep, DefaultEquivalence);

        Assert.Equal("D_obs_max", matrix.MetricId);
        Assert.Equal(2, matrix.BranchIds.Count);
        Assert.Equal(0.0, matrix.Distances[0, 0]);
        Assert.Equal(0.0, matrix.Distances[1, 1]);
        Assert.Equal(0.0, matrix.Distances[0, 1]);
        Assert.Equal(0.0, matrix.Distances[1, 0]);
    }

    [Fact]
    public void ObservedDistances_DifferentBranches_NonZero()
    {
        var obs1 = MakeObservedState(new[] { 1.0, 0.0, 0.0 });
        var obs2 = MakeObservedState(new[] { 0.0, 0.0, 0.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs1),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, observedState: obs2));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeObservedDistances(sweep, DefaultEquivalence);

        Assert.Equal(1.0, matrix.Distances[0, 1], precision: 10);
        Assert.Equal(matrix.Distances[0, 1], matrix.Distances[1, 0]); // symmetric
    }

    [Fact]
    public void ObservedDistances_MissingObservedState_NaN()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, observedState: null));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeObservedDistances(sweep, DefaultEquivalence);

        Assert.True(double.IsNaN(matrix.Distances[0, 1]));
        Assert.True(double.IsNaN(matrix.Distances[1, 0]));
    }

    [Fact]
    public void DynamicDistances_DifferentObjectives_NonZero()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1.0, 0.5, 50),
            MakeRunRecord("v2", true, 2.0, 0.3, 50));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeDynamicDistances(sweep);

        Assert.Equal("D_dyn", matrix.MetricId);
        // |1.0 - 2.0| + |0.5 - 0.3| = 1.0 + 0.2 = 1.2
        Assert.Equal(1.2, matrix.Distances[0, 1], precision: 10);
        Assert.Equal(0.0, matrix.Distances[0, 0]);
    }

    [Fact]
    public void ConvergenceDistances_SameClass_IterationDiffOnly()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 60));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeConvergenceDistances(sweep);

        Assert.Equal("D_conv", matrix.MetricId);
        // |50 - 60| + 0 (same class) = 10
        Assert.Equal(10.0, matrix.Distances[0, 1], precision: 10);
    }

    [Fact]
    public void ConvergenceDistances_DifferentClass_AddsPenalty()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50),
            MakeRunRecord("v2", false, 1.0, 1.0, 100, terminationReason: "MaxIterations"));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeConvergenceDistances(sweep);

        // |50 - 100| + 1 (different class) = 51
        Assert.Equal(51.0, matrix.Distances[0, 1], precision: 10);
    }

    // --- Qualitative Classification Tests ---

    [Fact]
    public void ClassifyBranch_Converged()
    {
        var record = MakeRunRecord("v1", true, 1e-10, 1e-8, 50);
        Assert.Equal(QualitativeClass.Converged, CanonicityAnalyzer.ClassifyBranch(record));
    }

    [Fact]
    public void ClassifyBranch_Stalled()
    {
        var record = MakeRunRecord("v1", false, 1.0, 0.5, 100,
            terminationReason: "Stagnation detected");
        Assert.Equal(QualitativeClass.Stalled, CanonicityAnalyzer.ClassifyBranch(record));
    }

    [Fact]
    public void ClassifyBranch_Failed()
    {
        var record = MakeRunRecord("v1", false, 1.0, 1.0, 100,
            terminationReason: "MaxIterations");
        Assert.Equal(QualitativeClass.Failed, CanonicityAnalyzer.ClassifyBranch(record));
    }

    [Fact]
    public void AgreementMatrix_AllConverged_AllAgree()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 60));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeAgreementMatrix(sweep);

        Assert.True(matrix.AllAgree);
        Assert.True(matrix.Agrees[0, 1]);
        Assert.True(matrix.Agrees[1, 0]);
    }

    [Fact]
    public void AgreementMatrix_MixedClasses_Disagree()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50),
            MakeRunRecord("v2", false, 1.0, 1.0, 100, terminationReason: "MaxIterations"));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeAgreementMatrix(sweep);

        Assert.False(matrix.AllAgree);
        Assert.False(matrix.Agrees[0, 1]);
    }

    // --- Evaluate Tests ---

    [Fact]
    public void Evaluate_ConsistentBranches_VerdictConsistent()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0, 3.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 55, observedState: obs));

        var analyzer = new CanonicityAnalyzer();
        var evidence = analyzer.Evaluate(sweep, DefaultEquivalence, "shiab");

        Assert.Equal("consistent", evidence.Verdict);
        Assert.Equal(0.0, evidence.MaxObservedDeviation);
    }

    [Fact]
    public void Evaluate_InconsistentBranches_VerdictInconsistent()
    {
        var obs1 = MakeObservedState(new[] { 1.0, 0.0, 0.0 });
        var obs2 = MakeObservedState(new[] { 0.0, 0.0, 0.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs1),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, observedState: obs2));

        var analyzer = new CanonicityAnalyzer();
        var evidence = analyzer.Evaluate(sweep, DefaultEquivalence, "shiab");

        Assert.Equal("inconsistent", evidence.Verdict);
        Assert.True(evidence.MaxObservedDeviation > 1e-6);
    }

    [Fact]
    public void Evaluate_NoObservedState_Inconclusive()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50));

        var analyzer = new CanonicityAnalyzer();
        var evidence = analyzer.Evaluate(sweep, DefaultEquivalence, "shiab");

        Assert.Equal("inconclusive", evidence.Verdict);
    }

    // --- Docket Update Tests ---

    [Fact]
    public void UpdateDocket_AppendsEvidence_NeverDropsHistory()
    {
        var docket = CanonicityDocketBuilder.CreateOpen("shiab", "identity-shiab", "output-equivalence", "identity-shiab");
        var analyzer = new CanonicityAnalyzer();

        var ev1 = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-1",
            StudyId = "study-1",
            Verdict = "consistent",
            MaxObservedDeviation = 1e-9,
            Tolerance = 1e-6,
            Timestamp = DateTimeOffset.UtcNow,
        };

        var updated = analyzer.UpdateDocket(docket, ev1);
        Assert.Single(updated.CurrentEvidence);
        Assert.Equal(DocketStatus.EvidenceAccumulating, updated.Status);

        var ev2 = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-2",
            StudyId = "study-2",
            Verdict = "consistent",
            MaxObservedDeviation = 2e-9,
            Tolerance = 1e-6,
            Timestamp = DateTimeOffset.UtcNow,
        };

        var updated2 = analyzer.UpdateDocket(updated, ev2);
        Assert.Equal(2, updated2.CurrentEvidence.Count);
        Assert.Contains(updated2.CurrentEvidence, e => e.EvidenceId == "ev-1");
        Assert.Contains(updated2.CurrentEvidence, e => e.EvidenceId == "ev-2");
    }

    [Fact]
    public void UpdateDocket_Open_TransitionsToEvidenceAccumulating()
    {
        var docket = CanonicityDocketBuilder.CreateOpen("shiab", "identity-shiab", "output-equivalence", "identity-shiab");
        Assert.Equal(DocketStatus.Open, docket.Status);

        var analyzer = new CanonicityAnalyzer();
        var ev = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-1",
            StudyId = "study-1",
            Verdict = "consistent",
            MaxObservedDeviation = 1e-9,
            Tolerance = 1e-6,
            Timestamp = DateTimeOffset.UtcNow,
        };

        var updated = analyzer.UpdateDocket(docket, ev);
        Assert.Equal(DocketStatus.EvidenceAccumulating, updated.Status);
    }

    [Fact]
    public void UpdateDocket_InconsistentEvidence_AddsCounterexample()
    {
        var docket = CanonicityDocketBuilder.CreateOpen("shiab", "identity-shiab", "output-equivalence", "identity-shiab");
        var analyzer = new CanonicityAnalyzer();

        var ev = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-bad",
            StudyId = "study-1",
            Verdict = "inconsistent",
            MaxObservedDeviation = 1.0,
            Tolerance = 1e-6,
            Timestamp = DateTimeOffset.UtcNow,
        };

        var updated = analyzer.UpdateDocket(docket, ev);
        Assert.Single(updated.KnownCounterexamples);
        Assert.Contains("ev-bad", updated.KnownCounterexamples);
        // Does NOT auto-falsify
        Assert.Equal(DocketStatus.EvidenceAccumulating, updated.Status);
    }

    [Fact]
    public void UpdateDocket_PreservesAllExistingFields()
    {
        var docket = new CanonicityDocket
        {
            ObjectClass = "shiab",
            ActiveRepresentative = "identity-shiab",
            EquivalenceRelationId = "output-eq",
            AdmissibleComparisonClass = "identity-shiab,covariant-shiab",
            DownstreamClaimsBlockedUntilClosure = new[] { "claim-1" },
            CurrentEvidence = Array.Empty<CanonicityEvidenceRecord>(),
            KnownCounterexamples = new[] { "existing-ce" },
            PendingTheorems = new[] { "theorem-1" },
            StudyReports = new[] { "old-study" },
            Status = DocketStatus.EvidenceAccumulating,
        };

        var analyzer = new CanonicityAnalyzer();
        var ev = new CanonicityEvidenceRecord
        {
            EvidenceId = "ev-new",
            StudyId = "new-study",
            Verdict = "consistent",
            MaxObservedDeviation = 1e-9,
            Tolerance = 1e-6,
            Timestamp = DateTimeOffset.UtcNow,
        };

        var updated = analyzer.UpdateDocket(docket, ev);
        Assert.Equal("shiab", updated.ObjectClass);
        Assert.Equal("identity-shiab", updated.ActiveRepresentative);
        Assert.Single(updated.DownstreamClaimsBlockedUntilClosure);
        Assert.Single(updated.PendingTheorems);
        Assert.Contains("existing-ce", updated.KnownCounterexamples);
        Assert.Contains("old-study", updated.StudyReports);
        Assert.Contains("new-study", updated.StudyReports);
    }

    // --- Fragility Detection Tests ---

    [Fact]
    public void DetectFragility_AllConsistent_NoneFragile()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0, 3.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 55, observedState: obs));

        var analyzer = new CanonicityAnalyzer();
        var reports = analyzer.DetectFragility(sweep, DefaultEquivalence);

        Assert.Equal(2, reports.Count);
        Assert.All(reports, r => Assert.False(r.IsFragile));
    }

    [Fact]
    public void DetectFragility_OutlierBranch_Flagged()
    {
        var obs1 = MakeObservedState(new[] { 1.0, 2.0, 3.0 });
        var obs2 = MakeObservedState(new[] { 1.0, 2.0, 3.0 });
        var obsOutlier = MakeObservedState(new[] { 100.0, 200.0, 300.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs1),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 55, observedState: obs2),
            MakeRunRecord("v3", true, 1e-10, 1e-8, 52, observedState: obsOutlier));

        var analyzer = new CanonicityAnalyzer();
        var reports = analyzer.DetectFragility(sweep, DefaultEquivalence);

        Assert.Equal(3, reports.Count);
        // v3 is the outlier
        var v3Report = reports.Single(r => r.BranchId == "v3");
        Assert.True(v3Report.IsFragile);
        Assert.NotEmpty(v3Report.FragilityReasons);
    }

    [Fact]
    public void DetectFragility_QualitativeDisagreement_Flagged()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0, 3.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 55, observedState: obs),
            MakeRunRecord("v3", false, 1.0, 1.0, 100,
                terminationReason: "MaxIterations", observedState: obs));

        var analyzer = new CanonicityAnalyzer();
        var reports = analyzer.DetectFragility(sweep, DefaultEquivalence);

        var v3Report = reports.Single(r => r.BranchId == "v3");
        Assert.True(v3Report.IsFragile);
        Assert.Contains(v3Report.FragilityReasons, r => r.Contains("Qualitative class"));
    }

    // --- PairwiseDistanceMatrix Properties ---

    [Fact]
    public void DistanceMatrix_IsSymmetric_ZeroDiagonal()
    {
        var obs1 = MakeObservedState(new[] { 1.0, 0.0 });
        var obs2 = MakeObservedState(new[] { 0.0, 1.0 });
        var obs3 = MakeObservedState(new[] { 0.5, 0.5 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs1),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, observedState: obs2),
            MakeRunRecord("v3", true, 1e-10, 1e-8, 50, observedState: obs3));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeObservedDistances(sweep, DefaultEquivalence);

        int n = matrix.BranchIds.Count;
        for (int i = 0; i < n; i++)
        {
            Assert.Equal(0.0, matrix.Distances[i, i]);
            for (int j = i + 1; j < n; j++)
            {
                Assert.Equal(matrix.Distances[i, j], matrix.Distances[j, i]);
            }
        }
    }

    // --- Failure Mode Matrix Tests ---

    [Fact]
    public void FailureModes_AllConverged_AllNull()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 60, observedState: obs));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeFailureModes(sweep);

        Assert.Equal(2, matrix.BranchIds.Count);
        Assert.All(matrix.PrimaryFailureModes, mode => Assert.Null(mode));
        // All converged = same failure mode (null == null)
        Assert.True(matrix.SameFailureMode[0, 1]);
        Assert.True(matrix.SameFailureMode[1, 0]);
    }

    [Fact]
    public void FailureModes_SameFailure_Detected()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", false, 1.0, 1.0, 100, terminationReason: "diverged"),
            MakeRunRecord("v2", false, 2.0, 2.0, 100, terminationReason: "diverged rapidly"));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeFailureModes(sweep);

        Assert.Equal("solver-diverged", matrix.PrimaryFailureModes[0]);
        Assert.Equal("solver-diverged", matrix.PrimaryFailureModes[1]);
        Assert.True(matrix.SameFailureMode[0, 1]);
    }

    [Fact]
    public void FailureModes_DifferentFailures_Distinguished()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", false, 1.0, 1.0, 100, terminationReason: "diverged"),
            MakeRunRecord("v2", false, 0.5, 0.5, 100, terminationReason: "Stagnation detected"),
            MakeRunRecord("v3", false, 0.5, 0.5, 1000, terminationReason: "MaxIterations reached"));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeFailureModes(sweep);

        Assert.Equal("solver-diverged", matrix.PrimaryFailureModes[0]);
        Assert.Equal("solver-stagnated", matrix.PrimaryFailureModes[1]);
        Assert.Equal("max-iterations", matrix.PrimaryFailureModes[2]);
        Assert.False(matrix.SameFailureMode[0, 1]);
        Assert.False(matrix.SameFailureMode[0, 2]);
        Assert.False(matrix.SameFailureMode[1, 2]);
    }

    [Fact]
    public void FailureModes_ExtractorFailed_Detected()
    {
        // Converged but no ObservedState
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: null));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeFailureModes(sweep);

        Assert.Equal("extractor-failed", matrix.PrimaryFailureModes[0]);
    }

    [Fact]
    public void FailureModes_DefaultsToSolverDiverged_ForUnknownReason()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", false, 1.0, 1.0, 50, terminationReason: "SomeUnknownReason"));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeFailureModes(sweep);

        Assert.Equal("solver-diverged", matrix.PrimaryFailureModes[0]);
    }

    // --- Extraction Agreement Tests ---

    [Fact]
    public void ExtractionAgreement_BothExtracted_AllAgree()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, observedState: obs));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeExtractionAgreement(sweep);

        Assert.True(matrix.AllAgree);
        Assert.True(matrix.Agrees[0, 1]);
        Assert.True(matrix.ExtractionStatuses[0]);
        Assert.True(matrix.ExtractionStatuses[1]);
    }

    [Fact]
    public void ExtractionAgreement_OneExtracted_Disagree()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, observedState: null));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeExtractionAgreement(sweep);

        Assert.False(matrix.AllAgree);
        Assert.False(matrix.Agrees[0, 1]);
        Assert.True(matrix.ExtractionStatuses[0]);
        Assert.False(matrix.ExtractionStatuses[1]);
    }

    [Fact]
    public void ExtractionAgreement_NeitherExtracted_AllAgree()
    {
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeExtractionAgreement(sweep);

        Assert.True(matrix.AllAgree);
        Assert.False(matrix.ExtractionStatuses[0]);
    }

    // --- Admissibility Agreement Tests ---

    [Fact]
    public void AdmissibilityAgreement_BothAdmissible_AllAgree()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, observedState: obs));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeAdmissibilityAgreement(sweep);

        Assert.True(matrix.AllAgree);
        Assert.True(matrix.Agrees[0, 1]);
    }

    [Fact]
    public void AdmissibilityAgreement_OneInadmissible_Disagree()
    {
        var obs = MakeObservedState(new[] { 1.0, 2.0 });
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, observedState: obs),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, observedState: obs,
                comparisonAdmissible: false));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeAdmissibilityAgreement(sweep);

        Assert.False(matrix.AllAgree);
        Assert.False(matrix.Agrees[0, 1]);
        Assert.True(matrix.AdmissibilityStatuses[0]);
        Assert.False(matrix.AdmissibilityStatuses[1]);
    }

    // --- Stability Distance (D_stab) Tests ---

    private static HessianSummary MakeStabilitySummary(
        double smallestEigenvalue,
        int negativeModeCount = 0,
        int softModeCount = 0,
        int nearKernelCount = 0,
        string classification = "strictly-positive-on-slice") => new()
    {
        SmallestEigenvalue = smallestEigenvalue,
        NegativeModeCount = negativeModeCount,
        SoftModeCount = softModeCount,
        NearKernelCount = nearKernelCount,
        StabilityClassification = classification,
        GaugeHandlingMode = "coulomb-slice",
    };

    private static readonly EquivalenceSpec StabilityEquivalence = new()
    {
        Id = "eq-stab-test",
        Name = "Stability Equivalence",
        ComparedObjectClasses = new[] { "stability" },
        NormalizationProcedure = "none",
        AllowedTransformations = Array.Empty<string>(),
        Metrics = new[] { "D_stab" },
        Tolerances = new Dictionary<string, double> { ["stability"] = 1.0 },
        InterpretationRule = "all-within-tolerance",
    };

    [Fact]
    public void StabilityDistances_IdenticalStability_ZeroDistance()
    {
        var stab = MakeStabilitySummary(1.0);
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, stabilityDiagnostics: stab),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 55, stabilityDiagnostics: stab));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeStabilityDistances(sweep, StabilityEquivalence);

        Assert.Equal("D_stab", matrix.MetricId);
        Assert.Equal(0.0, matrix.Distances[0, 1]);
        Assert.Equal(0.0, matrix.Distances[1, 0]);
        Assert.Equal(0.0, matrix.Distances[0, 0]);
    }

    [Fact]
    public void StabilityDistances_DifferentNegativeModeCount_NonZero()
    {
        var stab1 = MakeStabilitySummary(1.0, negativeModeCount: 0);
        var stab2 = MakeStabilitySummary(1.0, negativeModeCount: 2);
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, stabilityDiagnostics: stab1),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, stabilityDiagnostics: stab2));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeStabilityDistances(sweep, StabilityEquivalence);

        // |1.0-1.0| + |0-2| + |0-0| = 2.0, normalized by 1.0
        Assert.Equal(2.0, matrix.Distances[0, 1], precision: 10);
    }

    [Fact]
    public void StabilityDistances_MissingStabilityData_NaN()
    {
        var stab = MakeStabilitySummary(1.0);
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, stabilityDiagnostics: stab),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, stabilityDiagnostics: null));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeStabilityDistances(sweep, StabilityEquivalence);

        Assert.True(double.IsNaN(matrix.Distances[0, 1]));
        Assert.True(double.IsNaN(matrix.Distances[1, 0]));
    }

    [Fact]
    public void StabilityDistances_DifferentEigenvalues_CorrectMetric()
    {
        var stab1 = MakeStabilitySummary(0.5, softModeCount: 1);
        var stab2 = MakeStabilitySummary(-0.3, negativeModeCount: 1, softModeCount: 2);
        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, stabilityDiagnostics: stab1),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, stabilityDiagnostics: stab2));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeStabilityDistances(sweep, StabilityEquivalence);

        // |0.5-(-0.3)| + |0-1| + |1-2| = 0.8 + 1 + 1 = 2.8
        Assert.Equal(2.8, matrix.Distances[0, 1], precision: 10);
    }

    [Fact]
    public void StabilityDistances_CustomNormalization()
    {
        var stab1 = MakeStabilitySummary(1.0);
        var stab2 = MakeStabilitySummary(3.0);
        var eqWithNorm = new EquivalenceSpec
        {
            Id = "eq-norm",
            Name = "Normalized Stability",
            ComparedObjectClasses = new[] { "stability" },
            NormalizationProcedure = "none",
            AllowedTransformations = Array.Empty<string>(),
            Metrics = new[] { "D_stab" },
            Tolerances = new Dictionary<string, double> { ["stability"] = 2.0 },
            InterpretationRule = "all-within-tolerance",
        };

        var sweep = MakeSweep(
            MakeRunRecord("v1", true, 1e-10, 1e-8, 50, stabilityDiagnostics: stab1),
            MakeRunRecord("v2", true, 1e-10, 1e-8, 50, stabilityDiagnostics: stab2));

        var analyzer = new CanonicityAnalyzer();
        var matrix = analyzer.ComputeStabilityDistances(sweep, eqWithNorm);

        // |1.0-3.0| / 2.0 = 1.0
        Assert.Equal(1.0, matrix.Distances[0, 1], precision: 10);
    }
}
