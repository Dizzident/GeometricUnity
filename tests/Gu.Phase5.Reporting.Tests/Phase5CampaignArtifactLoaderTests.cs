using System.Text.Json;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase4.Registry;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
using Gu.Phase5.Dossiers;
using Gu.Phase5.Environments;
using Gu.Phase5.Falsification;
using Gu.Phase5.QuantitativeValidation;
using Gu.Phase5.Reporting;

namespace Gu.Phase5.Reporting.Tests;

/// <summary>
/// WP-3: Tests for Phase5CampaignArtifactLoader and the run-phase5-campaign path.
///
/// Covers:
///   1. Campaign run produces full output artifact tree (branch/, convergence/,
///      quantitative/, falsification/, dossiers/, reports/)
///   2. Reproduction command in manifests uses run-phase5-campaign
///   3. Both typed dossier (Phase5ValidationDossier) and provenance dossier
///      (ValidationDossier) are present in the result
/// </summary>
public sealed class Phase5CampaignArtifactLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public Phase5CampaignArtifactLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"gu-wp3-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "wp3-test-sha",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    private RefinementQuantityValueTable MakeBranchValues() => new RefinementQuantityValueTable
    {
        StudyId = "branch-study",
        Levels =
        [
            new RefinementQuantityValueLevel
            {
                LevelId = "V1",
                SolverConverged = true,
                Quantities = new Dictionary<string, double> { ["q1"] = 1.0, ["q2"] = 5.0 },
            },
            new RefinementQuantityValueLevel
            {
                LevelId = "V2",
                SolverConverged = true,
                Quantities = new Dictionary<string, double> { ["q1"] = 1.005, ["q2"] = 5.02 },
            },
            new RefinementQuantityValueLevel
            {
                LevelId = "V3",
                SolverConverged = true,
                Quantities = new Dictionary<string, double> { ["q1"] = 0.998, ["q2"] = 4.99 },
            },
        ],
    };

    private RefinementQuantityValueTable MakeRefinementValues() => new RefinementQuantityValueTable
    {
        StudyId = "refine-study",
        Levels =
        [
            new RefinementQuantityValueLevel
            {
                LevelId = "L0",
                SolverConverged = true,
                Quantities = new Dictionary<string, double> { ["q1"] = 2.25 },
            },
            new RefinementQuantityValueLevel
            {
                LevelId = "L1",
                SolverConverged = true,
                Quantities = new Dictionary<string, double> { ["q1"] = 2.0625 },
            },
            new RefinementQuantityValueLevel
            {
                LevelId = "L2",
                SolverConverged = true,
                Quantities = new Dictionary<string, double> { ["q1"] = 2.015625 },
            },
        ],
    };

    private BridgeManifest MakeBridgeManifest(int sourceRecordCount = 3) => new BridgeManifest
    {
        ManifestId = "bridge-test-001",
        SourceAtlasPath = "/tmp/atlas.json",
        SourceRecordIds = Enumerable.Range(0, sourceRecordCount)
            .Select(i => $"bg-{i}")
            .ToList(),
        SourceStateArtifactRefs = Enumerable.Range(0, sourceRecordCount)
            .Select(i => $"states/bg-{i}.json")
            .ToList(),
        DerivedVariantIds = Enumerable.Range(0, sourceRecordCount)
            .Select(i => $"V{i + 1}")
            .ToList(),
        Provenance = MakeProvenance(),
    };

    private EnvironmentRecord MakeEnvironmentRecord() => new EnvironmentRecord
    {
        EnvironmentId = "env-toy",
        GeometryTier = "toy",
        GeometryFingerprint = "toy-v1",
        BaseDimension = 2,
        AmbientDimension = 2,
        EdgeCount = 4,
        FaceCount = 2,
        Admissibility = new EnvironmentAdmissibilityReport
        {
            Level = "toy",
            Checks = [],
            Passed = true,
        },
        Provenance = MakeProvenance(),
    };

    private Phase5CampaignSpec WriteArtifactsAndMakeSpec()
    {
        // Write branch values file
        var branchValuesPath = Path.Combine(_tempDir, "branch_values.json");
        File.WriteAllText(branchValuesPath, GuJsonDefaults.Serialize(MakeBranchValues()));

        // Write refinement values file
        var refinementValuesPath = Path.Combine(_tempDir, "refinement_values.json");
        File.WriteAllText(refinementValuesPath, GuJsonDefaults.Serialize(MakeRefinementValues()));

        // Write observables file (empty array)
        var observablesPath = Path.Combine(_tempDir, "observables.json");
        File.WriteAllText(observablesPath, "[]");

        // Write environment record file
        var envPath = Path.Combine(_tempDir, "env_toy.json");
        File.WriteAllText(envPath, GuJsonDefaults.Serialize(MakeEnvironmentRecord()));

        // Write targets file
        var targetsPath = Path.Combine(_tempDir, "targets.json");
        var targetTable = new ExternalTargetTable { TableId = "test-targets", Targets = [] };
        File.WriteAllText(targetsPath, GuJsonDefaults.Serialize(targetTable));

        // Write registry file
        var registryPath = Path.Combine(_tempDir, "registry.json");
        var registry = new UnifiedParticleRegistry();
        File.WriteAllText(registryPath, GuJsonDefaults.Serialize(registry));

        return new Phase5CampaignSpec
        {
            CampaignId = "wp3-integration-campaign",
            SchemaVersion = "1.0.0",
            BranchFamilySpec = new BranchRobustnessStudySpec
            {
                StudyId = "branch-study",
                BranchVariantIds = ["V1", "V2", "V3"],
                TargetQuantityIds = ["q1", "q2"],
            },
            RefinementSpec = new RefinementStudySpec
            {
                StudyId = "refine-study",
                SchemaVersion = "1.0",
                BranchManifestId = "branch-1",
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
                EnvironmentIds = ["env-toy"],
                BranchManifestId = "branch-1",
                TargetQuantities = ["q1"],
                Provenance = MakeProvenance(),
            },
            ExternalTargetTablePath = targetsPath,
            BranchQuantityValuesPath = branchValuesPath,
            RefinementValuesPath = refinementValuesPath,
            ObservablesPath = observablesPath,
            EnvironmentRecordPaths = [envPath],
            RegistryPath = registryPath,
            CalibrationPolicy = new CalibrationPolicy
            {
                PolicyId = "test-policy",
                Mode = "sigma",
                SigmaThreshold = 5.0,
            },
            FalsificationPolicy = new FalsificationPolicy(),
            Provenance = MakeProvenance(),
        };
    }

    /// <summary>
    /// WP-3 test 1: ArtifactLoader loads all required artifacts from spec.
    /// </summary>
    [Fact]
    public void ArtifactLoader_LoadsAllRequiredArtifacts()
    {
        var spec = WriteArtifactsAndMakeSpec();
        var artifacts = Phase5CampaignArtifactLoader.Load(spec, _tempDir);

        Assert.NotNull(artifacts);
        Assert.NotNull(artifacts.BranchQuantityValues);
        Assert.NotNull(artifacts.RefinementValues);
        Assert.NotNull(artifacts.Observables);
        Assert.NotNull(artifacts.EnvironmentRecords);
        Assert.NotEmpty(artifacts.EnvironmentRecords);
        Assert.NotNull(artifacts.TargetTable);
        Assert.NotNull(artifacts.Registry);
    }

    [Fact]
    public void ArtifactLoader_LoadsInferredBridgeManifest_WhenPresent()
    {
        var spec = WriteArtifactsAndMakeSpec();
        File.WriteAllText(
            Path.Combine(_tempDir, "bridge_manifest.json"),
            GuJsonDefaults.Serialize(MakeBridgeManifest()));

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, _tempDir);

        Assert.NotNull(artifacts.RefinementEvidenceManifest);
        Assert.Equal("bridge-derived", artifacts.RefinementEvidenceManifest!.EvidenceSource);
        Assert.Equal(3, artifacts.RefinementEvidenceManifest.SourceRecordIds.Count);
    }

    [Fact]
    public void ArtifactLoader_LoadsExplicitRefinementEvidenceManifest_WhenPresent()
    {
        var spec = WriteArtifactsAndMakeSpec();
        File.WriteAllText(
            Path.Combine(_tempDir, "refinement_evidence_manifest.json"),
            GuJsonDefaults.Serialize(new RefinementEvidenceManifest
            {
                ManifestId = "direct-refinement-001",
                StudyId = spec.RefinementSpec.StudyId,
                EvidenceSource = "direct-solver-backed",
                SourceRecordIds = ["bg-L0", "bg-L1", "bg-L2"],
                SourceArtifactRefs = ["/tmp/bg-L0.json", "/tmp/bg-L1.json", "/tmp/bg-L2.json"],
                Provenance = MakeProvenance(),
            }));

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, _tempDir);

        Assert.NotNull(artifacts.RefinementEvidenceManifest);
        Assert.Equal("direct-solver-backed", artifacts.RefinementEvidenceManifest!.EvidenceSource);
    }

    [Fact]
    public void ArtifactLoader_LoadsReferenceStudyCampaignArtifacts()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);
        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        Assert.NotNull(artifacts.BranchQuantityValues);
        Assert.NotNull(artifacts.RefinementValues);
        Assert.NotEmpty(artifacts.EnvironmentRecords);
        Assert.NotNull(artifacts.TargetTable);
        Assert.NotNull(artifacts.Registry);
    }

    /// <summary>
    /// WP-3 test 2: RunFull via artifact loader routes branch and refinement
    /// values into the executor delegates and produces a full output tree.
    /// </summary>
    [Fact]
    public void RunFull_ViaArtifactLoader_ProducesCompleteResult()
    {
        var spec = WriteArtifactsAndMakeSpec();
        File.WriteAllText(
            Path.Combine(_tempDir, "bridge_manifest.json"),
            GuJsonDefaults.Serialize(MakeBridgeManifest()));
        var artifacts = Phase5CampaignArtifactLoader.Load(spec, _tempDir);

        // Build executors from the loaded artifacts (mirrors RunPhase5Campaign in CLI)
        Func<string, IReadOnlyDictionary<string, double[]>> branchExecutor = variantId =>
        {
            var level = artifacts.BranchQuantityValues.Levels
                .FirstOrDefault(l => l.LevelId == variantId);
            if (level is null) return new Dictionary<string, double[]>();
            return level.Quantities.ToDictionary(kv => kv.Key, kv => new double[] { kv.Value });
        };

        Func<RefinementLevel, IReadOnlyDictionary<string, double>> refinementExecutor = level =>
        {
            var valueLevel = artifacts.RefinementValues.Levels
                .FirstOrDefault(l => l.LevelId == level.LevelId);
            if (valueLevel is null) return new Dictionary<string, double>();
            return valueLevel.Quantities;
        };

        var runner = new Phase5CampaignRunner();
        var result = runner.RunFull(
            spec,
            branchExecutor,
            refinementExecutor,
            artifacts.Observables,
            artifacts.TargetTable,
            artifacts.Registry,
            refinementEvidenceManifest: artifacts.RefinementEvidenceManifest);

        Assert.NotNull(result);
        Assert.Equal("wp3-integration-campaign", result.Report.StudyId);
        // Both dossier types are present (WP-5 / D-006)
        Assert.NotNull(result.TypedDossier);
        Assert.NotNull(result.ProvenanceDossier);
        // Study manifests: exactly two (positive + negative)
        Assert.Equal(2, result.StudyManifests.Count);
        Assert.Contains(result.Report.ConvergenceAtlas!.SummaryLines,
            line => line.Contains("bridge-derived", StringComparison.Ordinal));
        Assert.Contains(result.Report.ConvergenceAtlas!.SummaryLines,
            line => line.Contains("multi-variant admitted atlas", StringComparison.Ordinal));
    }

    /// <summary>
    /// WP-3 test 3: Reproduction command in study manifests uses run-phase5-campaign (D-005).
    /// </summary>
    [Fact]
    public void StudyManifests_ReproductionCommand_UsesRunPhase5Campaign()
    {
        var spec = WriteArtifactsAndMakeSpec();
        var artifacts = Phase5CampaignArtifactLoader.Load(spec, _tempDir);

        Func<string, IReadOnlyDictionary<string, double[]>> branchExecutor = variantId =>
            new Dictionary<string, double[]> { ["q1"] = [1.0] };

        Func<RefinementLevel, IReadOnlyDictionary<string, double>> refinementExecutor = level =>
            new Dictionary<string, double> { ["q1"] = 2.0 + level.EffectiveMeshParameter * level.EffectiveMeshParameter };

        var runner = new Phase5CampaignRunner();
        var result = runner.RunFull(
            spec,
            branchExecutor,
            refinementExecutor,
            artifacts.Observables,
            artifacts.TargetTable,
            artifacts.Registry);

        // Every study manifest must carry a reproduction command containing "run-phase5-campaign"
        Assert.All(result.StudyManifests, manifest =>
        {
            Assert.NotNull(manifest.Reproducibility);
            var commands = manifest.Reproducibility.ReproductionCommands;
            Assert.NotNull(commands);
            Assert.Contains(commands, cmd => cmd.Contains("run-phase5-campaign"));
        });
    }

    [Fact]
    public void RunFull_CandidateProvenanceLinks_OnlyLinkedCandidatesGainBranchAndRefinementEvidenceIds()
    {
        var spec = WriteArtifactsAndMakeSpec();
        File.WriteAllText(
            Path.Combine(_tempDir, "bridge_manifest.json"),
            GuJsonDefaults.Serialize(MakeBridgeManifest()));

        var registry = new UnifiedParticleRegistry
        {
            Candidates =
            [
                new UnifiedParticleRecord
                {
                    ParticleId = "cand-linked",
                    ParticleType = UnifiedParticleType.Fermion,
                    PrimarySourceId = "src-linked",
                    ContributingSourceIds = ["src-linked"],
                    BranchVariantSet = [],
                    BackgroundSet = [],
                    Chirality = "mixed",
                    MassLikeEnvelope = [1.0, 1.0, 1.0],
                    BranchStabilityScore = 1.0,
                    ObservationConfidence = 0.9,
                    ComparisonEvidenceScore = 0.0,
                    ClaimClass = "C2_BranchStableCandidate",
                    ComputedWithUnverifiedGpu = false,
                    RegistryVersion = "1.0.0",
                    Provenance = MakeProvenance(),
                },
                new UnifiedParticleRecord
                {
                    ParticleId = "cand-unlinked",
                    ParticleType = UnifiedParticleType.Fermion,
                    PrimarySourceId = "src-unlinked",
                    ContributingSourceIds = ["src-unlinked"],
                    BranchVariantSet = [],
                    BackgroundSet = [],
                    Chirality = "mixed",
                    MassLikeEnvelope = [1.0, 1.0, 1.0],
                    BranchStabilityScore = 1.0,
                    ObservationConfidence = 0.9,
                    ComparisonEvidenceScore = 0.0,
                    ClaimClass = "C2_BranchStableCandidate",
                    ComputedWithUnverifiedGpu = false,
                    RegistryVersion = "1.0.0",
                    Provenance = MakeProvenance(),
                },
            ],
        };
        File.WriteAllText(Path.Combine(_tempDir, "registry.json"), GuJsonDefaults.Serialize(registry));

        File.WriteAllText(
            Path.Combine(_tempDir, "candidate_provenance_links.json"),
            GuJsonDefaults.Serialize(new List<CandidateProvenanceLinkRecord>
            {
                new CandidateProvenanceLinkRecord
                {
                    CandidateId = "cand-linked",
                    BranchVariantIds = ["V1", "V2"],
                    BackgroundIds = ["bg-0", "bg-1"],
                },
            }));

        File.WriteAllText(
            Path.Combine(_tempDir, "observables.json"),
            GuJsonDefaults.Serialize(new List<QuantitativeObservableRecord>
            {
                new QuantitativeObservableRecord
                {
                    ObservableId = "obs-linked",
                    Value = 1.0,
                    Uncertainty = new QuantitativeUncertainty { TotalUncertainty = 0.1 },
                    BranchId = "V1",
                    EnvironmentId = "env-toy",
                    RefinementLevel = "L2",
                    ExtractionMethod = "test",
                    Provenance = MakeProvenance(),
                },
                new QuantitativeObservableRecord
                {
                    ObservableId = "obs-unlinked",
                    Value = 1.0,
                    Uncertainty = new QuantitativeUncertainty { TotalUncertainty = 0.1 },
                    BranchId = "V1",
                    EnvironmentId = "env-toy",
                    RefinementLevel = "L2",
                    ExtractionMethod = "test",
                    Provenance = MakeProvenance(),
                },
            }));

        File.WriteAllText(
            Path.Combine(_tempDir, "observation_chain.json"),
            GuJsonDefaults.Serialize(new List<ObservationChainRecord>
            {
                new ObservationChainRecord
                {
                    CandidateId = "cand-linked",
                    PrimarySourceId = "src-linked",
                    ObservableId = "obs-linked",
                    CompletenessStatus = "complete",
                    SensitivityScore = 0.1,
                    AuxiliaryModelSensitivity = 0.1,
                    Passed = true,
                    Provenance = MakeProvenance(),
                },
                new ObservationChainRecord
                {
                    CandidateId = "cand-unlinked",
                    PrimarySourceId = "src-unlinked",
                    ObservableId = "obs-unlinked",
                    CompletenessStatus = "complete",
                    SensitivityScore = 0.1,
                    AuxiliaryModelSensitivity = 0.1,
                    Passed = true,
                    Provenance = MakeProvenance(),
                },
            }));

        File.WriteAllText(
            Path.Combine(_tempDir, "environment_variance.json"),
            GuJsonDefaults.Serialize(new List<EnvironmentVarianceRecord>
            {
                new EnvironmentVarianceRecord
                {
                    RecordId = "env-var-obs-linked",
                    QuantityId = "obs-linked",
                    EnvironmentTierId = "toy+structured",
                    RelativeStdDev = 0.01,
                    Flagged = false,
                    Provenance = MakeProvenance(),
                },
                new EnvironmentVarianceRecord
                {
                    RecordId = "env-var-obs-unlinked",
                    QuantityId = "obs-unlinked",
                    EnvironmentTierId = "toy+structured",
                    RelativeStdDev = 0.01,
                    Flagged = false,
                    Provenance = MakeProvenance(),
                },
            }));

        var targetTable = new ExternalTargetTable
        {
            TableId = "linked-targets",
            Targets =
            [
                new ExternalTarget
                {
                    ObservableId = "obs-linked",
                    Label = "target-linked",
                    Value = 1.0,
                    Uncertainty = 0.1,
                    Source = "test",
                    DistributionModel = "gaussian",
                },
                new ExternalTarget
                {
                    ObservableId = "obs-unlinked",
                    Label = "target-unlinked",
                    Value = 1.0,
                    Uncertainty = 0.1,
                    Source = "test",
                    DistributionModel = "gaussian",
                },
            ],
        };
        File.WriteAllText(Path.Combine(_tempDir, "targets.json"), GuJsonDefaults.Serialize(targetTable));

        var specWithSidecars = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = "targets.json",
            BranchQuantityValuesPath = spec.BranchQuantityValuesPath,
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = "observables.json",
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = "registry.json",
            ObservationChainPath = "observation_chain.json",
            EnvironmentVariancePath = "environment_variance.json",
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
        };

        var artifacts = Phase5CampaignArtifactLoader.Load(specWithSidecars, _tempDir);

        Func<string, IReadOnlyDictionary<string, double[]>> branchExecutor = variantId =>
        {
            var level = artifacts.BranchQuantityValues.Levels.First(l => l.LevelId == variantId);
            return level.Quantities.ToDictionary(kv => kv.Key, kv => new[] { kv.Value });
        };

        Func<RefinementLevel, IReadOnlyDictionary<string, double>> refinementExecutor = level =>
            artifacts.RefinementValues.Levels.First(l => l.LevelId == level.LevelId).Quantities;

        var runner = new Phase5CampaignRunner();
        var result = runner.RunFull(
            specWithSidecars,
            branchExecutor,
            refinementExecutor,
            artifacts.Observables,
            artifacts.TargetTable,
            artifacts.Registry,
            candidateProvenanceLinks: artifacts.CandidateProvenanceLinks,
            observationChainRecords: artifacts.ObservationChainRecords,
            environmentRecords: artifacts.EnvironmentRecords,
            environmentVarianceRecords: artifacts.EnvironmentVarianceRecords,
            refinementEvidenceManifest: artifacts.RefinementEvidenceManifest);

        var linkedEscalation = result.TypedDossier.ClaimEscalations.Single(e => e.CandidateId == "cand-linked");
        var unlinkedEscalation = result.TypedDossier.ClaimEscalations.Single(e => e.CandidateId == "cand-unlinked");
        var linkedBranchEvidence = linkedEscalation.GateResults.Single(g => g.GateId == EscalationGates.BranchRobust).EvidenceRecordIds;
        var linkedRefinementEvidence = linkedEscalation.GateResults.Single(g => g.GateId == EscalationGates.RefinementBounded).EvidenceRecordIds;
        var unlinkedBranchEvidence = unlinkedEscalation.GateResults.Single(g => g.GateId == EscalationGates.BranchRobust).EvidenceRecordIds;
        var unlinkedRefinementEvidence = unlinkedEscalation.GateResults.Single(g => g.GateId == EscalationGates.RefinementBounded).EvidenceRecordIds;

        Assert.NotNull(linkedBranchEvidence);
        Assert.NotNull(linkedRefinementEvidence);
        Assert.NotNull(unlinkedBranchEvidence);
        Assert.NotNull(unlinkedRefinementEvidence);
        Assert.Equal(["V1", "V2"], linkedBranchEvidence);
        Assert.Equal(["bg-0", "bg-1"], linkedRefinementEvidence);
        Assert.Empty(unlinkedBranchEvidence);
        Assert.Empty(unlinkedRefinementEvidence);
    }

    /// <summary>
    /// ArtifactLoader throws ArtifactLoadException when a required file is missing.
    /// </summary>
    [Fact]
    public void ArtifactLoader_ThrowsOnMissingRequiredArtifact()
    {
        var spec = WriteArtifactsAndMakeSpec();
        // Point branch values to a nonexistent file
        var badSpec = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = spec.ExternalTargetTablePath,
            BranchQuantityValuesPath = "nonexistent_branch_values.json",
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = spec.ObservablesPath,
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = spec.RegistryPath,
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
        };

        Assert.Throws<ArtifactLoadException>(() =>
            Phase5CampaignArtifactLoader.Load(badSpec, _tempDir));
    }

    /// <summary>
    /// Optional sidecar paths that are null or absent yield null JsonElement.
    /// </summary>
    [Fact]
    public void ArtifactLoader_OptionalSidecars_AreNullWhenAbsent()
    {
        var spec = WriteArtifactsAndMakeSpec();
        // Spec has no optional sidecar paths set (they are null by default)
        var artifacts = Phase5CampaignArtifactLoader.Load(spec, _tempDir);

        Assert.Null(artifacts.ObservationChainRecords);
        Assert.Null(artifacts.EnvironmentVarianceRecords);
        Assert.Null(artifacts.RepresentationContentRecords);
        Assert.Null(artifacts.CouplingConsistencyRecords);
    }

    // ─── P6-M4: Environment and target upgrade tests ───

    /// <summary>
    /// P6-M4: Reference campaign must include at least one toy AND one structured environment record (D-P6-004).
    /// Verifies the upgraded campaign.json satisfies the multi-environment requirement.
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_HasToyAndStructuredEnvironments()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        var tiers = artifacts.EnvironmentRecords.Select(r => r.GeometryTier).ToList();
        Assert.Contains("toy", tiers);
        Assert.Contains("structured", tiers);
    }

    /// <summary>
    /// P6-M4: Reference campaign includes an imported environment record with all required provenance fields (D-P6-004).
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_ImportedEnvironmentHasRequiredProvenance()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        var importedRecords = artifacts.EnvironmentRecords
            .Where(r => r.GeometryTier == "imported")
            .ToList();

        Assert.NotEmpty(importedRecords);
        foreach (var record in importedRecords)
        {
            Assert.NotNull(record.DatasetId);
            Assert.NotEmpty(record.DatasetId);
            Assert.NotNull(record.SourceHash);
            Assert.NotEmpty(record.SourceHash);
            Assert.NotNull(record.ConversionVersion);
            Assert.NotEmpty(record.ConversionVersion);
        }
    }

    /// <summary>
    /// P6-M4: Reference campaign includes the 4 sidecar paths required by schema 1.1.0 (D-P6-001).
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_HasAllFourSidecarPaths()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        Assert.NotNull(spec.ObservationChainPath);
        Assert.NotNull(spec.EnvironmentVariancePath);
        Assert.NotNull(spec.RepresentationContentPath);
        Assert.NotNull(spec.CouplingConsistencyPath);
    }

    /// <summary>
    /// P6-M4: All targets in the reference study target table have an explicit distributionModel (D-P6-005).
    /// </summary>
    [Fact]
    public void ReferenceStudyTargets_AllHaveExplicitDistributionModel()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        Assert.NotEmpty(artifacts.TargetTable.Targets);
        Assert.All(artifacts.TargetTable.Targets, t =>
        {
            Assert.NotNull(t.DistributionModel);
            Assert.NotEmpty(t.DistributionModel);
            // Must be one of the known distribution models
            Assert.Contains(t.DistributionModel,
                new[] { "gaussian", "gaussian-asymmetric", "student-t" });
        });
    }

    /// <summary>
    /// P6-M4: Reference campaign schema version must be 1.1.0 (D-P6-001).
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_SchemaVersionIs110()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        Assert.Equal("1.1.0", spec.SchemaVersion);
    }

    /// <summary>
    /// P6-M4: A campaign with only toy environment records fails the "toy-only" check.
    /// This test documents the expected behavior for the Phase VI validator (P6-M1).
    /// Until the validator is implemented, we verify the spec loads and that the
    /// environment tier list contains only "toy" — so any validator implementation
    /// must reject it.
    /// </summary>
    [Fact]
    public void ToyOnlyCampaign_EnvironmentTiers_ContainsOnlyToy()
    {
        // Build a spec with only a toy environment record
        var spec = WriteArtifactsAndMakeSpec();
        var artifacts = Phase5CampaignArtifactLoader.Load(spec, _tempDir);

        var tiers = artifacts.EnvironmentRecords.Select(r => r.GeometryTier).ToList();
        // The toy-only spec loaded from WriteArtifactsAndMakeSpec has only "toy" tier
        Assert.DoesNotContain("structured", tiers);
        Assert.DoesNotContain("imported", tiers);
        Assert.Contains("toy", tiers);
        // A Phase VI validator must reject this as a primary evidence campaign
        // (tested in Phase5CampaignValidatorTests once P6-M1 is implemented)
    }

    // ─── P11-M1: Candidate-linked branch/background provenance tests ───

    /// <summary>
    /// P11-M1: Reference study campaign includes a candidate_provenance_links.json file
    /// and the loader picks it up as non-null CandidateProvenanceLinks.
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_P11M1_HasCandidateProvenanceLinks()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        Assert.NotNull(artifacts.CandidateProvenanceLinks);
        Assert.NotEmpty(artifacts.CandidateProvenanceLinks!);
    }

    /// <summary>
    /// P11-M1: The linked candidate in the reference study carries non-empty branch variant IDs.
    /// This satisfies the requirement that at least one candidate has non-empty evidenceRecordIds
    /// in the branch-robust gate.
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_P11M1_LinkedCandidateHasNonEmptyBranchVariantIds()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        Assert.NotNull(artifacts.CandidateProvenanceLinks);
        var linkedRecord = artifacts.CandidateProvenanceLinks!
            .First(link => link.BranchVariantIds.Any());
        Assert.NotEmpty(linkedRecord.BranchVariantIds);
        Assert.NotEmpty(linkedRecord.BackgroundIds);
    }

    /// <summary>
    /// P11-M1: Mixed population contract — at least one candidate has provenance links and at
    /// least one candidate lacks links. Verifies both populations coexist correctly.
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_P11M1_MixedPopulation_LinkedAndUnlinkedCandidatesDistinct()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        Assert.NotNull(artifacts.CandidateProvenanceLinks);
        var links = artifacts.CandidateProvenanceLinks!;

        // At least one linked candidate
        Assert.Contains(links, link => link.BranchVariantIds.Any());

        // The registry has candidates — some are not linked (since links only cover one candidate)
        Assert.NotNull(artifacts.Registry);
        var linkedCandidateIds = links.Select(l => l.CandidateId).ToHashSet(StringComparer.Ordinal);
        var unlinkedCandidates = artifacts.Registry.Candidates
            .Where(c => !linkedCandidateIds.Contains(c.ParticleId))
            .ToList();
        Assert.NotEmpty(unlinkedCandidates);

        // Unlinked candidates retain empty branch/background sets (fail-closed)
        Assert.All(unlinkedCandidates, c =>
        {
            Assert.Empty(c.BranchVariantSet);
            Assert.Empty(c.BackgroundSet);
        });
    }

    /// <summary>
    /// P11-M1: After provenance linker is applied, the linked candidate gains non-empty
    /// branch variant IDs while the unlinked candidates stay empty (D-P11-003).
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_P11M1_ProvenanceLinker_LinkedGainsIds_UnlinkedStaysEmpty()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        var enrichedRegistry = CandidateProvenanceLinker.Apply(
            artifacts.Registry,
            artifacts.CandidateProvenanceLinks);

        var links = artifacts.CandidateProvenanceLinks!;
        var linkedCandidateId = links.First(l => l.BranchVariantIds.Any()).CandidateId;

        var linkedCandidate = enrichedRegistry.Candidates
            .Single(c => c.ParticleId == linkedCandidateId);
        var unlinkedCandidates = enrichedRegistry.Candidates
            .Where(c => c.ParticleId != linkedCandidateId)
            .ToList();

        // Linked candidate: non-empty branch variant IDs and background IDs
        Assert.NotEmpty(linkedCandidate.BranchVariantSet);
        Assert.NotEmpty(linkedCandidate.BackgroundSet);

        // Unlinked candidates: still empty (fail-closed per D-P11-003)
        Assert.All(unlinkedCandidates, c =>
        {
            Assert.Empty(c.BranchVariantSet);
            Assert.Empty(c.BackgroundSet);
        });
    }

    // ─── P11-M2: Genuine external imported evidence tests ───

    /// <summary>
    /// P11-M2: Reference study campaign must include at least one external imported environment
    /// record with a datasetId that does not start with "repo-internal-".
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_P11M2_HasGenuineExternalImportedEnvironment()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        var externalImported = artifacts.EnvironmentRecords
            .Where(r => r.GeometryTier == "imported" &&
                        r.DatasetId is not null &&
                        !r.DatasetId.StartsWith("repo-internal-", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(externalImported);
    }

    /// <summary>
    /// P11-M2: The external imported record and the repo-internal imported record are both present
    /// and explicitly distinct (D-P11-002: do not relabel internal as external).
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_P11M2_InternalAndExternalImportsAreDistinct()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        var importedRecords = artifacts.EnvironmentRecords
            .Where(r => r.GeometryTier == "imported")
            .ToList();

        // At least 2 imported records
        Assert.True(importedRecords.Count >= 2);

        // At least one is repo-internal (preserved per D-P11-002)
        Assert.Contains(importedRecords, r =>
            r.DatasetId is not null &&
            r.DatasetId.StartsWith("repo-internal-", StringComparison.Ordinal));

        // At least one is genuinely external (new for P11-M2)
        Assert.Contains(importedRecords, r =>
            r.DatasetId is not null &&
            !r.DatasetId.StartsWith("repo-internal-", StringComparison.Ordinal) &&
            !r.DatasetId.StartsWith("synthetic-example-", StringComparison.Ordinal));

        // All dataset IDs are distinct — no relabeling
        var datasetIds = importedRecords
            .Where(r => r.DatasetId is not null)
            .Select(r => r.DatasetId!)
            .ToList();
        Assert.Equal(datasetIds.Count, datasetIds.Distinct(StringComparer.Ordinal).Count());
    }

    /// <summary>
    /// P11-M2: External imported environment record has all required provenance fields
    /// (datasetId, sourceHash, conversionVersion).
    /// </summary>
    [Fact]
    public void ReferenceStudyCampaign_P11M2_ExternalImportedEnvironment_HasRequiredProvenanceFields()
    {
        var repoRoot = FindRepoRoot();
        var specPath = Path.Combine(
            repoRoot,
            "studies",
            "phase5_su2_branch_refinement_env_validation",
            "config",
            "campaign.json");
        var spec = GuJsonDefaults.Deserialize<Phase5CampaignSpec>(File.ReadAllText(specPath));
        Assert.NotNull(spec);

        var artifacts = Phase5CampaignArtifactLoader.Load(spec, Path.GetDirectoryName(specPath)!);

        var externalImported = artifacts.EnvironmentRecords
            .Where(r => r.GeometryTier == "imported" &&
                        r.DatasetId is not null &&
                        !r.DatasetId.StartsWith("repo-internal-", StringComparison.Ordinal))
            .ToList();

        Assert.NotEmpty(externalImported);
        foreach (var record in externalImported)
        {
            Assert.NotNull(record.DatasetId);
            Assert.NotEmpty(record.DatasetId!);
            Assert.NotNull(record.SourceHash);
            Assert.NotEmpty(record.SourceHash!);
            Assert.NotNull(record.ConversionVersion);
            Assert.NotEmpty(record.ConversionVersion!);
        }
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "GeometricUnity.slnx")))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate repository root from test base directory.");
    }
}
