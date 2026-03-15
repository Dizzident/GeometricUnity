using Gu.Core;
using Gu.Phase4.Registry;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;

namespace Gu.Phase5.Reporting.Tests;

public sealed class Phase5CampaignCoverageTests
{
    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = new DateTimeOffset(2026, 3, 16, 0, 0, 0, TimeSpan.Zero),
        CodeRevision = "campaign-coverage-test",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0.0" },
        Backend = "cpu",
    };

    private static Phase5CampaignSpec MakeSpec() => new Phase5CampaignSpec
    {
        CampaignId = "coverage-campaign",
        SchemaVersion = "1.1.0",
        BranchFamilySpec = new BranchRobustnessStudySpec
        {
            StudyId = "branch-study",
            BranchVariantIds = ["V1", "V2", "V3"],
            TargetQuantityIds = ["q1"],
            AbsoluteTolerance = 1e-6,
            RelativeTolerance = 0.1,
        },
        RefinementSpec = new RefinementStudySpec
        {
            StudyId = "refine-study",
            SchemaVersion = "1.0",
            BranchManifestId = "V1",
            TargetQuantities = ["q1"],
            RefinementLevels =
            [
                new RefinementLevel { LevelId = "L0", MeshParameterX = 1.0, MeshParameterF = 1.0 },
                new RefinementLevel { LevelId = "L1", MeshParameterX = 0.5, MeshParameterF = 0.5 },
                new RefinementLevel { LevelId = "L2", MeshParameterX = 0.25, MeshParameterF = 0.25 },
            ],
            Provenance = MakeProvenance(),
        },
        EnvironmentCampaignSpec = new EnvironmentCampaignSpec
        {
            CampaignId = "env-campaign",
            SchemaVersion = "1.0",
            EnvironmentIds = ["env-toy", "env-structured"],
            BranchManifestId = "V1",
            TargetQuantities = ["obs-1"],
            Provenance = MakeProvenance(),
        },
        ExternalTargetTablePath = "targets.json",
        BranchQuantityValuesPath = "branch_values.json",
        RefinementValuesPath = "refinement_values.json",
        ObservablesPath = "observables.json",
        EnvironmentRecordPaths = ["env-toy.json", "env-structured.json"],
        RegistryPath = "registry.json",
        CalibrationPolicy = new CalibrationPolicy
        {
            PolicyId = "sigma-policy",
            Mode = "sigma",
            SigmaThreshold = 5.0,
        },
        FalsificationPolicy = new FalsificationPolicy(),
        Provenance = MakeProvenance(),
    };

    [Fact]
    public void RunFull_WithSidecarSummary_ThreadsCoverageIntoDossierAndFalsifiers()
    {
        var runner = new Phase5CampaignRunner();
        var registry = new UnifiedParticleRegistry();
        registry.Register(new UnifiedParticleRecord
        {
            ParticleId = "cand-001",
            ParticleType = UnifiedParticleType.Fermion,
            PrimarySourceId = "cluster-001",
            ContributingSourceIds = ["ferm-family-001"],
            BranchVariantSet = [],
            BackgroundSet = [],
            Chirality = "mixed",
            MassLikeEnvelope = [1.0, 1.1, 1.2],
            BranchStabilityScore = 1.0,
            ObservationConfidence = 0.5,
            ComparisonEvidenceScore = 0.5,
            ClaimClass = "C3_BranchStableCandidate",
            RegistryVersion = "1.0.0",
            Provenance = MakeProvenance(),
        });

        var sidecarSummary = new SidecarSummary
        {
            StudyId = "coverage-sidecars",
            Channels =
            [
                new SidecarChannelStatus { ChannelId = "observation-chain", Status = "evaluated", InputCount = 1, OutputCount = 1 },
                new SidecarChannelStatus { ChannelId = "environment-variance", Status = "evaluated", InputCount = 1, OutputCount = 1 },
                new SidecarChannelStatus { ChannelId = "representation-content", Status = "evaluated", InputCount = 1, OutputCount = 1 },
                new SidecarChannelStatus { ChannelId = "coupling-consistency", Status = "evaluated", InputCount = 1, OutputCount = 1 },
            ],
            Provenance = MakeProvenance(),
        };

        var result = runner.RunFull(
            MakeSpec(),
            branchPipelineExecutor: variantId => new Dictionary<string, double[]> { ["q1"] = [variantId == "V2" ? 1.01 : 1.0] },
            refinementPipelineExecutor: level => new Dictionary<string, double> { ["q1"] = 1.0 + (level.EffectiveMeshParameter * level.EffectiveMeshParameter) },
            observablesSource:
            [
                new QuantitativeObservableRecord
                {
                    ObservableId = "obs-1",
                    Value = 1.0,
                    Uncertainty = new QuantitativeUncertainty
                    {
                        BranchVariation = 0.02,
                        RefinementError = 0.02,
                        ExtractionError = 0.01,
                        EnvironmentSensitivity = 0.01,
                        TotalUncertainty = 0.03,
                    },
                    BranchId = "V1",
                    EnvironmentId = "env-structured",
                    ExtractionMethod = "ratio",
                    Provenance = MakeProvenance(),
                },
            ],
            targetTable: new ExternalTargetTable
            {
                TableId = "targets",
                Targets =
                [
                    new ExternalTarget
                    {
                        Label = "obs-1-target",
                        ObservableId = "obs-1",
                        Value = 1.0,
                        Uncertainty = 0.1,
                        Source = "derived-synthetic-v1",
                        EvidenceTier = "derived-synthetic",
                        DistributionModel = "gaussian",
                    },
                ],
            },
            registry: registry,
            observationChainRecords:
            [
                new ObservationChainRecord
                {
                    CandidateId = "cand-001",
                    PrimarySourceId = "cluster-001",
                    ObservableId = "obs-1",
                    CompletenessStatus = "complete",
                    SensitivityScore = 0.1,
                    AuxiliaryModelSensitivity = 0.1,
                    Passed = true,
                    Provenance = MakeProvenance(),
                },
            ],
            environmentRecords:
            [
                new EnvironmentRecord
                {
                    EnvironmentId = "env-toy",
                    GeometryTier = "toy",
                    GeometryFingerprint = "toy-001",
                    BaseDimension = 2,
                    AmbientDimension = 2,
                    EdgeCount = 4,
                    FaceCount = 2,
                    Admissibility = new EnvironmentAdmissibilityReport { Level = "toy", Checks = [], Passed = true },
                    Provenance = MakeProvenance(),
                },
                new EnvironmentRecord
                {
                    EnvironmentId = "env-structured",
                    GeometryTier = "structured",
                    GeometryFingerprint = "structured-001",
                    BaseDimension = 2,
                    AmbientDimension = 2,
                    EdgeCount = 16,
                    FaceCount = 8,
                    Admissibility = new EnvironmentAdmissibilityReport { Level = "structured", Checks = [], Passed = true },
                    Provenance = MakeProvenance(),
                },
            ],
            environmentVarianceRecords:
            [
                new EnvironmentVarianceRecord
                {
                    RecordId = "env-001",
                    QuantityId = "obs-1",
                    EnvironmentTierId = "toy+structured",
                    RelativeStdDev = 0.1,
                    Provenance = MakeProvenance(),
                },
            ],
            representationContentRecords:
            [
                new RepresentationContentRecord
                {
                    RecordId = "rep-001",
                    CandidateId = "cand-001",
                    ExpectedModeCount = 1,
                    ObservedModeCount = 1,
                    Consistent = true,
                    Provenance = MakeProvenance(),
                },
            ],
            couplingConsistencyRecords:
            [
                new CouplingConsistencyRecord
                {
                    RecordId = "cpl-001",
                    CandidateId = "cand-001",
                    CouplingType = "mass-envelope-proxy",
                    RelativeSpread = 0.1,
                    Consistent = true,
                    Provenance = MakeProvenance(),
                },
            ],
            sidecarSummary: sidecarSummary);

        Assert.NotNull(result.TypedDossier.SidecarCoverage);
        Assert.Equal(4, result.TypedDossier.SidecarCoverage!.Channels.Count);
        Assert.NotNull(result.TypedDossier.FalsifierSummary);
        Assert.NotNull(result.TypedDossier.FalsifierSummary!.EvaluationCoverage);
        Assert.Equal(4, result.TypedDossier.FalsifierSummary.EvaluationCoverage!.Count);
    }
}
