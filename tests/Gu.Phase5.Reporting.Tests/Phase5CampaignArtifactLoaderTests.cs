using System.Text.Json;
using Gu.Core;
using Gu.Core.Serialization;
using Gu.Phase4.Registry;
using Gu.Phase5.BranchIndependence;
using Gu.Phase5.Convergence;
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
            artifacts.Registry);

        Assert.NotNull(result);
        Assert.Equal("wp3-integration-campaign", result.Report.StudyId);
        // Both dossier types are present (WP-5 / D-006)
        Assert.NotNull(result.TypedDossier);
        Assert.NotNull(result.ProvenanceDossier);
        // Study manifests: exactly two (positive + negative)
        Assert.Equal(2, result.StudyManifests.Count);
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
