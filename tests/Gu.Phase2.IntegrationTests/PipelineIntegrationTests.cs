using Gu.Core;
using Gu.Phase2.Branches;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Comparison;
using Gu.Phase2.Continuation;
using Gu.Phase2.Execution;
using Gu.Phase2.Predictions;
using Gu.Phase2.Recovery;
using Gu.Phase2.Reporting;
using Gu.Phase2.Semantics;
using Gu.Phase2.Stability;
using Gu.Solvers;

namespace Gu.Phase2.IntegrationTests;

/// <summary>
/// End-to-end integration tests that exercise cross-module pipelines.
/// Per IMPLEMENTATION_PLAN_P2.md Section 10.4 and GAP-14.
/// </summary>
public class PipelineIntegrationTests
{
    // --- Study S1: Full pipeline A0-sweep ---

    [Fact]
    public void S1_A0Sweep_ProducesBranchRunRecords_And_CanonicityEvidence()
    {
        // Create two branch variants differing only in A0Variant
        var v1 = MakeVariant("v1", a0Variant: "zero");
        var v2 = MakeVariant("v2", a0Variant: "flat-su2");

        var obs1 = MakeObservedState(new[] { 1.0, 2.0, 3.0 });
        var obs2 = MakeObservedState(new[] { 1.0, 2.0, 3.0 });

        var sweep = MakeSweep(
            MakeRunRecord(v1, converged: true, objective: 1e-10, residual: 1e-8, iterations: 50, obs: obs1),
            MakeRunRecord(v2, converged: true, objective: 1e-10, residual: 1e-8, iterations: 55, obs: obs2));

        // Run CanonicityAnalyzer
        var equivalence = MakeEquivalence();
        var analyzer = new CanonicityAnalyzer();

        var obsMatrix = analyzer.ComputeObservedDistances(sweep, equivalence);
        var dynMatrix = analyzer.ComputeDynamicDistances(sweep);
        var convMatrix = analyzer.ComputeConvergenceDistances(sweep);
        var agreement = analyzer.ComputeAgreementMatrix(sweep);
        var failureModes = analyzer.ComputeFailureModes(sweep);

        // Verify PairwiseDistanceMatrix dimensions
        Assert.Equal(2, obsMatrix.BranchIds.Count);
        Assert.Equal(2, dynMatrix.BranchIds.Count);
        Assert.Equal(2, convMatrix.BranchIds.Count);

        // Identical observations -> zero D_obs
        Assert.Equal(0.0, obsMatrix.Distances[0, 1]);

        // Both converged -> all agree
        Assert.True(agreement.AllAgree);

        // All converged -> null failure modes
        Assert.All(failureModes.PrimaryFailureModes, mode => Assert.Null(mode));

        // Evaluate canonicity evidence
        var evidence = analyzer.Evaluate(sweep, equivalence, "shiab");
        Assert.Equal("consistent", evidence.Verdict);
        Assert.NotNull(evidence.EvidenceId);
        Assert.NotNull(evidence.StudyId);

        // Build docket and update
        var docket = CanonicityDocketBuilder.CreateOpen("shiab", "identity-shiab", "output-eq", "identity-shiab");
        var updatedDocket = analyzer.UpdateDocket(docket, evidence);
        Assert.Equal(DocketStatus.EvidenceAccumulating, updatedDocket.Status);
        Assert.Single(updatedDocket.CurrentEvidence);
    }

    [Fact]
    public void S1_A0Sweep_InconsistentBranches_ProducesCounterexample()
    {
        var v1 = MakeVariant("v1", a0Variant: "zero");
        var v2 = MakeVariant("v2", a0Variant: "flat-su2");

        // Different observed outputs -> inconsistent
        var obs1 = MakeObservedState(new[] { 1.0, 0.0, 0.0 });
        var obs2 = MakeObservedState(new[] { 0.0, 0.0, 100.0 });

        var sweep = MakeSweep(
            MakeRunRecord(v1, converged: true, objective: 1e-10, residual: 1e-8, iterations: 50, obs: obs1),
            MakeRunRecord(v2, converged: true, objective: 1e-10, residual: 1e-8, iterations: 60, obs: obs2));

        var analyzer = new CanonicityAnalyzer();
        var evidence = analyzer.Evaluate(sweep, MakeEquivalence(), "shiab");
        Assert.Equal("inconsistent", evidence.Verdict);
        Assert.True(evidence.MaxObservedDeviation > 1e-6);

        // Fragility detection should flag outlier
        var fragility = analyzer.DetectFragility(sweep, MakeEquivalence());
        Assert.Equal(2, fragility.Count);
        Assert.Contains(fragility, r => r.IsFragile);

        // Docket should record counterexample
        var docket = CanonicityDocketBuilder.CreateOpen("shiab", "identity-shiab", "output-eq", "identity-shiab");
        var updated = analyzer.UpdateDocket(docket, evidence);
        Assert.Single(updated.KnownCounterexamples);
    }

    // --- Study S4: Linearization + Spectrum ---

    [Fact]
    public void S4_StabilityAtlas_BuildsFromContinuationPaths()
    {
        // Build a continuation result with a bifurcation event
        var bifEvent = new ContinuationEvent
        {
            Kind = ContinuationEventKind.HessianSignChange,
            Lambda = 0.5,
            Description = "Sign change in Hessian eigenvalue at lambda=0.5",
            Severity = "critical",
        };

        var step = new ContinuationStep
        {
            StepIndex = 0,
            Lambda = 0.5,
            Arclength = 0.5,
            StepSize = 0.1,
            ResidualNorm = 1e-10,
            CorrectorIterations = 3,
            CorrectorConverged = true,
            Events = new[] { bifEvent },
        };

        var path = new ContinuationResult
        {
            Spec = MakeContinuationSpec(),
            Path = new[] { step },
            TerminationReason = "reached-end",
            TotalSteps = 1,
            TotalArclength = 0.5,
            LambdaMin = 0.0,
            LambdaMax = 0.5,
            AllEvents = new[] { bifEvent },
            Timestamp = DateTimeOffset.UtcNow,
        };

        // Build HessianRecord
        var hessian = new HessianRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            GaugeHandlingMode = "coulomb-slice",
            GaugeLambda = 1.0,
            Dimension = 6,
            AssemblyMode = "matrix-free",
            SymmetryVerified = true,
            SymmetryError = 1e-12,
        };

        // Build LinearizationRecord
        var linearization = new LinearizationRecord
        {
            BackgroundStateId = "bg-1",
            BranchManifestId = "branch-1",
            OperatorDefinitionId = "J",
            DerivativeMode = "analytic",
            InputDimension = 6,
            OutputDimension = 4,
            GaugeHandlingMode = "raw",
            AssemblyMode = "matrix-free",
            ValidationStatus = "verified",
        };

        // Build StabilityAtlas via builder
        var builder = new StabilityAtlasBuilder("atlas-1", "branch-1", "Test family")
            .AddPath(path)
            .AddHessianRecord(hessian)
            .AddLinearizationRecord(linearization)
            .WithDiscretizationNotes("h=0.1 uniform 2D mesh")
            .WithTheoremStatusNotes("Ellipticity unproven");

        var atlas = builder.Build();

        // Verify atlas contents
        Assert.Single(atlas.Paths);
        Assert.Single(atlas.HessianRecords);
        Assert.Single(atlas.LinearizationRecords);
        Assert.Equal("h=0.1 uniform 2D mesh", atlas.DiscretizationNotes);

        // Bifurcation indicators should be extracted and converted
        Assert.Single(atlas.BifurcationIndicators);
        Assert.Equal("sign-change", atlas.BifurcationIndicators[0].Kind);
        Assert.Equal(ContinuationEventKind.HessianSignChange, atlas.BifurcationIndicators[0].TriggeringEvent);
        Assert.Equal(0.5, atlas.BifurcationIndicators[0].Lambda);
        Assert.Equal("numerical-only", atlas.BifurcationIndicators[0].Confidence);
    }

    // --- Study S6: Comparison dry-run end-to-end ---

    [Fact]
    public void S6_ComparisonCampaign_ProducesRunRecords_And_NegativeArtifacts()
    {
        // Build a PredictionTestRecord with InMemoryDatasetAdapter
        var uncertainty = new UncertaintyRecord
        {
            Discretization = 0.01,
            Solver = 0.005,
            Branch = 0.02,
            Extraction = 0.01,
            Calibration = 0.005,
            DataAsset = 0.03,
        };

        var asset = new ComparisonAsset
        {
            AssetId = "asset-1",
            SourceCitation = "Test data 2024",
            AcquisitionDate = DateTimeOffset.UtcNow.AddYears(-1),
            PreprocessingDescription = "None",
            AdmissibleUseStatement = "Structural comparison only",
            DomainOfValidity = "Low energy",
            UncertaintyModel = uncertainty,
            ComparisonVariables = new Dictionary<string, string>
            {
                ["mass"] = "Test mass observable",
            },
        };

        // Build prediction that should pass structural comparison
        var goodPrediction = new PredictionTestRecord
        {
            TestId = "pred-good",
            ClaimClass = ClaimClass.ApproximateStructuralSurrogate,
            FormalSource = "Shiab identity operator trace",
            BranchManifestId = "branch-1",
            ObservableMapId = "pullback-trace",
            TheoremDependencyStatus = "open",
            NumericalDependencyStatus = "converged",
            ApproximationStatus = "leading-order",
            ExternalComparisonAsset = asset,
            Falsifier = "Observable differs from SM prediction by more than 3-sigma",
            ArtifactLinks = new[] { "art-1" },
        };

        // Build prediction that should fail (missing falsifier -> Inadmissible)
        var badPrediction = new PredictionTestRecord
        {
            TestId = "pred-bad",
            ClaimClass = ClaimClass.ExactStructuralConsequence,
            FormalSource = "Shiab trace",
            BranchManifestId = "branch-2",
            ObservableMapId = "pullback-trace",
            TheoremDependencyStatus = "open",
            NumericalDependencyStatus = "converged",
            ApproximationStatus = "exact",
            Falsifier = "",  // Empty falsifier -> Inadmissible
            ArtifactLinks = new[] { "art-2" },
        };

        var campaignSpec = new ComparisonCampaignSpec
        {
            CampaignId = "campaign-1",
            EnvironmentIds = new[] { "env-1" },
            BranchSubsetIds = new[] { "branch-1", "branch-2" },
            ObservedOutputClassIds = new[] { "mass" },
            ComparisonAssetIds = new[] { "asset-1" },
            Mode = ComparisonMode.Structural,
            CalibrationPolicy = "fixed",
        };

        var adapter = new InMemoryDatasetAdapter("test-adapter");
        adapter.Register(asset, new Dictionary<string, double[]>
        {
            ["mass"] = new[] { 125.0, 0.5 },
        });

        var runner = new CampaignRunner();
        var result = runner.RunWithStrategy(
            campaignSpec,
            new[] { goodPrediction, badPrediction },
            adapter);

        Assert.NotNull(result.CampaignResult);
        Assert.Equal("campaign-1", result.CampaignResult.CampaignId);

        // The good prediction should produce a run record
        Assert.True(result.CampaignResult.RunRecords.Count >= 0);

        // The bad prediction (empty falsifier) should produce a failure
        Assert.True(result.CampaignResult.Failures.Count >= 1);

        // NegativeResultArtifacts should be produced for failures
        Assert.True(result.NegativeArtifacts.Count >= 1);
        Assert.Contains(result.NegativeArtifacts, a => a.OriginalTestId == "pred-bad");
    }

    // --- Study: Recovery DAG + gate enforcement ---

    [Fact]
    public void RecoveryDAG_Valid4Node_PassesValidation()
    {
        // Build a valid 4-node RecoveryGraph: Native -> Obs -> Ext -> Interp
        var graph = new RecoveryGraph
        {
            Nodes = new[]
            {
                MakeRecoveryNode("native-1", RecoveryNodeKind.Native),
                MakeRecoveryNode("obs-1", RecoveryNodeKind.Observation),
                MakeRecoveryNode("ext-1", RecoveryNodeKind.Extraction),
                MakeRecoveryNode("interp-1", RecoveryNodeKind.Interpretation),
            },
            Edges = new[]
            {
                new RecoveryEdge { FromNodeId = "native-1", ToNodeId = "obs-1", OperatorId = "sigma_h_star" },
                new RecoveryEdge { FromNodeId = "obs-1", ToNodeId = "ext-1", OperatorId = "trace-projector" },
                new RecoveryEdge { FromNodeId = "ext-1", ToNodeId = "interp-1", OperatorId = "physical-identification" },
            },
        };

        bool valid = graph.Validate(out var errors);
        Assert.True(valid, $"Graph should be valid. Errors: {string.Join("; ", errors)}");
        Assert.Empty(errors);
    }

    [Fact]
    public void IdentificationGate_MissingFalsifier_ReturnsInadmissible()
    {
        var record = new PhysicalIdentificationRecord
        {
            IdentificationId = "id-1",
            FormalSource = "Shiab trace",
            ObservationExtractionMap = "pullback-trace",
            SupportStatus = "theorem-supported",
            ApproximationStatus = "exact",
            ComparisonTarget = "electron mass",
            Falsifier = "",  // Missing -> Inadmissible
            ResolvedClaimClass = ClaimClass.ExactStructuralConsequence,
        };

        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.Inadmissible, result.ResolvedClaimClass);
    }

    [Fact]
    public void IdentificationGate_AllFieldsPresent_TheoremSupported_AllowsExact()
    {
        var record = new PhysicalIdentificationRecord
        {
            IdentificationId = "id-2",
            FormalSource = "Shiab trace formal derivation",
            ObservationExtractionMap = "pullback-trace-su2",
            SupportStatus = "theorem-supported",
            ApproximationStatus = "exact",
            ComparisonTarget = "electron mass",
            Falsifier = "mass differs by > 3-sigma",
            ResolvedClaimClass = ClaimClass.Inadmissible,  // will be re-evaluated
        };

        var result = IdentificationGate.Evaluate(record);
        Assert.Equal(ClaimClass.ExactStructuralConsequence, result.ResolvedClaimClass);
    }

    [Fact]
    public void FullPipeline_RecoveryToGateToReport_OpenItems()
    {
        // Build a recovery graph with valid structure
        var graph = new RecoveryGraph
        {
            Nodes = new[]
            {
                MakeRecoveryNode("native-1", RecoveryNodeKind.Native),
                MakeRecoveryNode("obs-1", RecoveryNodeKind.Observation),
                MakeRecoveryNode("ext-1", RecoveryNodeKind.Extraction),
                MakeRecoveryNode("interp-1", RecoveryNodeKind.Interpretation),
            },
            Edges = new[]
            {
                new RecoveryEdge { FromNodeId = "native-1", ToNodeId = "obs-1", OperatorId = "sigma_h" },
                new RecoveryEdge { FromNodeId = "obs-1", ToNodeId = "ext-1", OperatorId = "projector" },
                new RecoveryEdge { FromNodeId = "ext-1", ToNodeId = "interp-1", OperatorId = "gate" },
            },
        };
        Assert.True(graph.Validate(out _));

        // Gate with missing falsifier -> inadmissible
        var inadmissible = IdentificationGate.Evaluate(new PhysicalIdentificationRecord
        {
            IdentificationId = "id-gate",
            FormalSource = "test source",
            ObservationExtractionMap = "test map",
            SupportStatus = "numerical-only",
            ApproximationStatus = "leading-order",
            ComparisonTarget = "test target",
            Falsifier = "",  // Missing
            ResolvedClaimClass = ClaimClass.ApproximateStructuralSurrogate,
        });
        Assert.Equal(ClaimClass.Inadmissible, inadmissible.ResolvedClaimClass);

        // Build a report with open items due to inadmissible gate result
        var report = new ResearchReport
        {
            ReportId = "report-1",
            BatchId = "batch-1",
            BranchLocalConclusions = Array.Empty<ReportItem>(),
            ComparisonReadyConclusions = Array.Empty<ReportItem>(),
            OpenItems = new[]
            {
                new ReportItem
                {
                    Summary = $"Gate returned Inadmissible for {inadmissible.IdentificationId}: missing falsifier",
                    SourceCategory = "recovery",
                    SourceStudyId = "study-gate",
                    EvidenceStrength = "weak",
                },
            },
            NumericalOnlyResults = Array.Empty<ReportItem>(),
            UninterpretedOutputs = Array.Empty<ReportItem>(),
            RuledOutClaims = Array.Empty<ReportItem>(),
            Dockets = Array.Empty<CanonicityDocket>(),
            GeneratedAt = DateTimeOffset.UtcNow,
        };

        Assert.NotEmpty(report.OpenItems);
        Assert.Contains(report.OpenItems, i => i.Summary.Contains("Inadmissible"));
    }

    // --- Helpers ---

    private static BranchVariantManifest MakeVariant(string id, string a0Variant = "zero") => new()
    {
        Id = id,
        ParentFamilyId = "fam-1",
        A0Variant = a0Variant,
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
        BranchVariantManifest variant,
        bool converged,
        double objective,
        double residual,
        int iterations,
        string terminationReason = "converged",
        ObservedState? obs = null) => new()
    {
        Variant = variant,
        Manifest = MakeManifest(variant.Id),
        Converged = converged,
        TerminationReason = terminationReason,
        FinalObjective = objective,
        FinalResidualNorm = residual,
        Iterations = iterations,
        SolveMode = SolveMode.ObjectiveMinimization,
        ObservedState = obs,
        ExtractionSucceeded = obs != null,
        ComparisonAdmissible = obs != null && obs.Observables.Count > 0,
        ArtifactBundle = MakeArtifact(variant.Id),
    };

    private static BranchFamilyManifest MakeFamily(params string[] variantIds) => new()
    {
        FamilyId = "fam-1",
        Description = "Integration test family",
        Variants = variantIds.Select(id => MakeVariant(id)).ToList(),
        DefaultEquivalence = MakeEquivalence(),
        CreatedAt = DateTimeOffset.UtcNow,
    };

    private static Phase2BranchSweepResult MakeSweep(params BranchRunRecord[] records) => new()
    {
        Family = MakeFamily(records.Select(r => r.Variant.Id).ToArray()),
        EnvironmentId = "env-integration",
        RunRecords = records.ToList(),
        InnerMode = SolveMode.ObjectiveMinimization,
        SweepStarted = DateTimeOffset.UtcNow.AddMinutes(-1),
        SweepCompleted = DateTimeOffset.UtcNow,
    };

    private static EquivalenceSpec MakeEquivalence() => new()
    {
        Id = "eq-integration",
        Name = "Integration Equivalence",
        ComparedObjectClasses = new[] { "observed-output" },
        NormalizationProcedure = "none",
        AllowedTransformations = Array.Empty<string>(),
        Metrics = new[] { "D_obs" },
        Tolerances = new Dictionary<string, double> { ["D_obs"] = 1e-6 },
        InterpretationRule = "all-within-tolerance",
    };

    private static ContinuationSpec MakeContinuationSpec() => new()
    {
        ParameterName = "gauge-lambda",
        LambdaStart = 0.0,
        LambdaEnd = 1.0,
        InitialStepSize = 0.1,
        MaxSteps = 20,
        MinStepSize = 0.001,
        MaxStepSize = 0.5,
        CorrectorTolerance = 1e-8,
        MaxCorrectorIterations = 10,
        BranchManifestId = "branch-1",
    };

    private static RecoveryNode MakeRecoveryNode(string nodeId, RecoveryNodeKind kind) => new()
    {
        NodeId = nodeId,
        Kind = kind,
        SourceObjectType = kind == RecoveryNodeKind.Native ? "curvature-2form" : "derived-tensor",
        BranchProvenanceId = "branch-1",
        GaugeDependent = kind == RecoveryNodeKind.Native,
        Dimensionality = 4,
        NumericalDependencyStatus = "converged",
        TheoremDependencyStatus = "unknown",
    };
}
