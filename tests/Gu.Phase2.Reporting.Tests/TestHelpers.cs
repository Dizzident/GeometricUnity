using Gu.Core;
using Gu.Phase2.Canonicity;
using Gu.Phase2.Continuation;
using Gu.Phase2.Execution;
using Gu.Phase2.Reporting;
using Gu.Phase2.Reporting.Artifacts;
using Gu.Phase2.Semantics;
using Gu.Solvers;

namespace Gu.Phase2.Reporting.Tests;

internal static class TestHelpers
{
    internal static BranchVariantManifest MakeVariant(string id = "variant-001") => new()
    {
        Id = id,
        ParentFamilyId = "family-001",
        A0Variant = "a0-flat",
        BiConnectionVariant = "bi-simple",
        TorsionVariant = "torsion-aug",
        ShiabVariant = "shiab-identity",
        ObservationVariant = "obs-standard",
        ExtractionVariant = "extract-standard",
        GaugeVariant = "gauge-coulomb",
        RegularityVariant = "regularity-h1",
        PairingVariant = "pairing-trace",
        ExpectedClaimCeiling = "ExactStructuralConsequence",
    };

    internal static EquivalenceSpec MakeEquivalence() => new()
    {
        Id = "eq-001",
        Name = "Residual norm equivalence",
        ComparedObjectClasses = ["residual-norm"],
        NormalizationProcedure = "L2-normalize",
        AllowedTransformations = ["gauge"],
        Metrics = ["relative-l2"],
        Tolerances = new Dictionary<string, double> { ["relative-l2"] = 1e-6 },
        InterpretationRule = "all-metrics-below-tolerance",
    };

    internal static BranchFamilyManifest MakeFamily(string familyId = "family-001") => new()
    {
        FamilyId = familyId,
        Description = "Test branch family",
        Variants = [MakeVariant("variant-001"), MakeVariant("variant-002")],
        DefaultEquivalence = MakeEquivalence(),
        CreatedAt = DateTimeOffset.UtcNow,
    };

    internal static BranchManifest MakeBranchManifest() => new()
    {
        BranchId = "branch-001",
        SchemaVersion = "1.0.0",
        SourceEquationRevision = "rev-001",
        CodeRevision = "abc123",
        ActiveGeometryBranch = "geom-flat-3d",
        ActiveObservationBranch = "obs-standard",
        ActiveTorsionBranch = "torsion-augmented",
        ActiveShiabBranch = "shiab-identity",
        ActiveGaugeStrategy = "coulomb",
        BaseDimension = 4,
        AmbientDimension = 14,
        LieAlgebraId = "su2",
        BasisConventionId = "canonical",
        ComponentOrderId = "face-major",
        AdjointConventionId = "adjoint-explicit",
        PairingConventionId = "pairing-trace",
        NormConventionId = "norm-l2-quadrature",
        DifferentialFormMetricId = "metric-standard",
        InsertedAssumptionIds = ["IA-1"],
        InsertedChoiceIds = ["IX-1"],
    };

    internal static BranchRef MakeBranchRef() => new()
    {
        BranchId = "branch-001",
        SchemaVersion = "1.0.0",
    };

    internal static ProvenanceMeta MakeProvenance() => new()
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "abc123",
        Branch = MakeBranchRef(),
        Backend = "cpu-reference",
    };

    internal static ReplayContract MakeReplayContract() => new()
    {
        BranchManifest = MakeBranchManifest(),
        Deterministic = true,
        BackendId = "cpu-reference",
        ReplayTier = "R2",
    };

    internal static ArtifactBundle MakeArtifactBundle() => new()
    {
        ArtifactId = "artifact-001",
        Branch = MakeBranchRef(),
        ReplayContract = MakeReplayContract(),
        Provenance = MakeProvenance(),
        CreatedAt = DateTimeOffset.UtcNow,
    };

    internal static BranchRunRecord MakeRunRecord(
        string variantId = "variant-001",
        bool converged = true) => new()
    {
        Variant = MakeVariant(variantId),
        Manifest = MakeBranchManifest(),
        Converged = converged,
        TerminationReason = converged ? "converged" : "max-iterations",
        FinalObjective = converged ? 1e-10 : 1.5,
        FinalResidualNorm = converged ? 1e-8 : 2.0,
        Iterations = converged ? 50 : 1000,
        SolveMode = SolveMode.ObjectiveMinimization,
        ExtractionSucceeded = false,
        ComparisonAdmissible = false,
        ArtifactBundle = MakeArtifactBundle(),
    };

    internal static Phase2BranchSweepResult MakeSweepResult(
        int convergedCount = 2,
        int divergedCount = 0) => new()
    {
        Family = MakeFamily(),
        EnvironmentId = "env-flat-3d",
        RunRecords = Enumerable.Range(0, convergedCount)
            .Select(i => MakeRunRecord($"variant-conv-{i}", converged: true))
            .Concat(Enumerable.Range(0, divergedCount)
                .Select(i => MakeRunRecord($"variant-div-{i}", converged: false)))
            .ToList(),
        InnerMode = SolveMode.ObjectiveMinimization,
        SweepStarted = DateTimeOffset.UtcNow.AddMinutes(-5),
        SweepCompleted = DateTimeOffset.UtcNow,
    };

    internal static ContinuationSpec MakeContinuationSpec() => new()
    {
        ParameterName = "gauge-lambda",
        LambdaStart = 0.0,
        LambdaEnd = 1.0,
        InitialStepSize = 0.01,
        MaxSteps = 100,
        MinStepSize = 1e-6,
        MaxStepSize = 0.1,
        CorrectorTolerance = 1e-8,
        MaxCorrectorIterations = 20,
        BranchManifestId = "branch-001",
    };

    internal static ContinuationStep MakeStep(int index, double lambda) => new()
    {
        StepIndex = index,
        Lambda = lambda,
        Arclength = lambda,
        StepSize = 0.01,
        ResidualNorm = 1e-8,
        CorrectorIterations = 5,
        CorrectorConverged = true,
        Events = [],
    };

    internal static ContinuationResult MakeContinuationResult(
        int stepCount = 3,
        int eventCount = 0) => new()
    {
        Spec = MakeContinuationSpec(),
        Path = Enumerable.Range(0, stepCount)
            .Select(i => MakeStep(i, i * 0.1))
            .ToList(),
        TerminationReason = "reached-end",
        TotalSteps = stepCount,
        TotalArclength = stepCount * 0.1,
        LambdaMin = 0.0,
        LambdaMax = stepCount * 0.1,
        AllEvents = Enumerable.Range(0, eventCount)
            .Select(i => new ContinuationEvent
            {
                Kind = ContinuationEventKind.HessianSignChange,
                Lambda = i * 0.05,
                Description = $"Sign change at step {i}",
                Severity = "warning",
            })
            .ToList(),
        Timestamp = DateTimeOffset.UtcNow,
    };

    internal static BranchSweepSpec MakeSweepSpec(string studyId = "sweep-001") => new()
    {
        StudyId = studyId,
        EnvironmentId = "env-flat-3d",
        FamilyId = "family-001",
        VariantIds = ["variant-001", "variant-002"],
        InnerSolveMode = "ObjectiveMinimization",
        Equivalence = MakeEquivalence(),
    };

    internal static StabilityStudySpec MakeStabilitySpec(string studyId = "stability-001") => new()
    {
        StudyId = studyId,
        BackgroundStateId = "state-001",
        BranchManifestId = "branch-001",
        GaugeHandlingMode = "coulomb-slice",
        RequestedModeCount = 6,
        SpectrumProbeMethod = "lanczos",
        SoftModeThreshold = 1e-4,
        NearKernelThreshold = 1e-8,
        NegativeModeThreshold = -1e-10,
    };

    internal static ResearchBatchSpec MakeBatchSpec(
        int sweepCount = 1,
        int stabilityCount = 1,
        int campaignCount = 1) => new()
    {
        BatchId = "batch-001",
        Sweeps = Enumerable.Range(0, sweepCount)
            .Select(i => MakeSweepSpec($"sweep-{i:D3}"))
            .ToList(),
        StabilityStudies = Enumerable.Range(0, stabilityCount)
            .Select(i => MakeStabilitySpec($"stability-{i:D3}"))
            .ToList(),
        RecoveryStudies = [],
        ComparisonCampaignIds = Enumerable.Range(0, campaignCount)
            .Select(i => $"campaign-{i:D3}")
            .ToList(),
    };

    internal static CanonicityDocket MakeDocket(
        string objectClass = "A0",
        DocketStatus status = DocketStatus.Open) => new()
    {
        ObjectClass = objectClass,
        ActiveRepresentative = "flat-A0",
        EquivalenceRelationId = "eq-001",
        AdmissibleComparisonClass = "gauge-orbit",
        DownstreamClaimsBlockedUntilClosure = ["physical-identification"],
        CurrentEvidence = [],
        KnownCounterexamples = [],
        PendingTheorems = ["uniqueness-theorem-A0"],
        StudyReports = [],
        Status = status,
    };
}
