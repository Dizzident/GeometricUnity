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

public sealed class Phase5CampaignSpecValidatorTests : IDisposable
{
    private readonly string _tempDir;

    public Phase5CampaignSpecValidatorTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"gu-validator-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static ProvenanceMeta MakeProvenance() => new ProvenanceMeta
    {
        CreatedAt = DateTimeOffset.UtcNow,
        CodeRevision = "test-sha",
        Branch = new BranchRef { BranchId = "test-branch", SchemaVersion = "1.0" },
        Backend = "cpu",
    };

    /// <summary>
    /// Creates a minimal valid spec referencing files that actually exist in <paramref name="dir"/>.
    /// </summary>
    private Phase5CampaignSpec MakeValidSpec(string dir, string schemaVersion = "1.0.0",
        IReadOnlyList<string>? extraEnvPaths = null)
    {
        File.WriteAllText(
            Path.Combine(dir, "targets.json"),
            GuJsonDefaults.Serialize(new ExternalTargetTable
            {
                TableId = "validator-targets",
                Targets =
                [
                    new ExternalTarget
                    {
                        Label = "toy-q1",
                        ObservableId = "q1",
                        Value = 1.0,
                        Uncertainty = 0.1,
                        Source = "synthetic-toy-v1",
                        EvidenceTier = "toy-placeholder",
                        DistributionModel = "gaussian",
                    },
                    new ExternalTarget
                    {
                        Label = "derived-q1",
                        ObservableId = "q1",
                        Value = 1.05,
                        Uncertainty = 0.08,
                        Source = "derived-synthetic-v1",
                        EvidenceTier = "derived-synthetic",
                        DistributionModel = "gaussian",
                    },
                ],
            }));
        File.WriteAllText(
            Path.Combine(dir, "branch_quantity_values.json"),
            GuJsonDefaults.Serialize(new RefinementQuantityValueTable
            {
                StudyId = "branch-values",
                Levels =
                [
                    new RefinementQuantityValueLevel
                    {
                        LevelId = "V1",
                        SolverConverged = true,
                        Quantities = new Dictionary<string, double> { ["q1"] = 1.0 },
                    },
                    new RefinementQuantityValueLevel
                    {
                        LevelId = "V2",
                        SolverConverged = true,
                        Quantities = new Dictionary<string, double> { ["q1"] = 1.01 },
                    },
                ],
            }));
        File.WriteAllText(
            Path.Combine(dir, "refinement_values.json"),
            GuJsonDefaults.Serialize(new RefinementQuantityValueTable
            {
                StudyId = "refinement-values",
                Levels =
                [
                    new RefinementQuantityValueLevel
                    {
                        LevelId = "L0",
                        SolverConverged = true,
                        Quantities = new Dictionary<string, double> { ["q1"] = 1.2 },
                    },
                    new RefinementQuantityValueLevel
                    {
                        LevelId = "L1",
                        SolverConverged = true,
                        Quantities = new Dictionary<string, double> { ["q1"] = 1.1 },
                    },
                ],
            }));
        File.WriteAllText(
            Path.Combine(dir, "observables.json"),
            GuJsonDefaults.Serialize(new[]
            {
                new QuantitativeObservableRecord
                {
                    ObservableId = "q1",
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
                    EnvironmentId = "env-toy",
                    ExtractionMethod = "validator",
                    Provenance = MakeProvenance(),
                },
            }));
        File.WriteAllText(
            Path.Combine(dir, "registry.json"),
            GuJsonDefaults.Serialize(new UnifiedParticleRegistry()));
        File.WriteAllText(
            Path.Combine(dir, "env_toy.json"),
            GuJsonDefaults.Serialize(new EnvironmentRecord
            {
                EnvironmentId = "env-toy",
                GeometryTier = "toy",
                GeometryFingerprint = "toy-validator",
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
            }));

        var envPaths = new List<string> { "env_toy.json" };
        if (extraEnvPaths != null)
            envPaths.AddRange(extraEnvPaths);

        return new Phase5CampaignSpec
        {
            CampaignId = "test-campaign",
            SchemaVersion = schemaVersion,
            BranchFamilySpec = new BranchRobustnessStudySpec
            {
                StudyId = "branch-study",
                BranchVariantIds = ["V1", "V2"],
                TargetQuantityIds = ["q1"],
            },
            RefinementSpec = new RefinementStudySpec
            {
                StudyId = "refine-study",
                SchemaVersion = "1.0",
                BranchManifestId = "branch-1",
                TargetQuantities = ["q1"],
                RefinementLevels = [
                    new RefinementLevel { LevelId = "L0", MeshParameterX = 1.0, MeshParameterF = 1.0 },
                    new RefinementLevel { LevelId = "L1", MeshParameterX = 0.5, MeshParameterF = 0.5 },
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
            ExternalTargetTablePath = "targets.json",
            BranchQuantityValuesPath = "branch_quantity_values.json",
            RefinementValuesPath = "refinement_values.json",
            ObservablesPath = "observables.json",
            EnvironmentRecordPaths = envPaths,
            RegistryPath = "registry.json",
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

    // -----------------------------------------------------------------------
    // Tests: valid specs
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_ValidSpec_NonReferenceMode_ReturnsValid()
    {
        var spec = MakeValidSpec(_tempDir);
        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_ValidReferenceSpec_WithAllSidecars_ReturnsValid()
    {
        // Create sidecar stubs and a second env record
        File.WriteAllText(Path.Combine(_tempDir, "env_structured.json"), GuJsonDefaults.Serialize(new EnvironmentRecord
        {
            EnvironmentId = "env-structured",
            GeometryTier = "structured",
            GeometryFingerprint = "structured-validator",
            BaseDimension = 2,
            AmbientDimension = 2,
            EdgeCount = 16,
            FaceCount = 8,
            Admissibility = new EnvironmentAdmissibilityReport
            {
                Level = "structured",
                Checks = [],
                Passed = true,
            },
            Provenance = MakeProvenance(),
        }));
        File.WriteAllText(Path.Combine(_tempDir, "obs_chain.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "env_variance.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "rep_content.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "coupling_consistency.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "sidecar_summary.json"), GuJsonDefaults.Serialize(new SidecarSummary
        {
            StudyId = "validator-sidecars",
            Channels =
            [
                new SidecarChannelStatus { ChannelId = "observation-chain", Status = "skipped", InputCount = 0, OutputCount = 0 },
                new SidecarChannelStatus { ChannelId = "environment-variance", Status = "skipped", InputCount = 0, OutputCount = 0 },
                new SidecarChannelStatus { ChannelId = "representation-content", Status = "skipped", InputCount = 0, OutputCount = 0 },
                new SidecarChannelStatus { ChannelId = "coupling-consistency", Status = "skipped", InputCount = 0, OutputCount = 0 },
            ],
            Provenance = MakeProvenance(),
        }));

        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.1.0",
            extraEnvPaths: ["env_structured.json"]);

        var specWithSidecars = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = spec.ExternalTargetTablePath,
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
            BranchQuantityValuesPath = spec.BranchQuantityValuesPath,
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = spec.ObservablesPath,
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = spec.RegistryPath,
            ObservationChainPath = "obs_chain.json",
            EnvironmentVariancePath = "env_variance.json",
            RepresentationContentPath = "rep_content.json",
            CouplingConsistencyPath = "coupling_consistency.json",
        };

        var result = Phase5CampaignSpecValidator.Validate(specWithSidecars, _tempDir, requireReferenceSidecars: true);
        Assert.True(result.IsValid, string.Join("; ", result.Errors));
        Assert.Empty(result.Errors);
    }

    // -----------------------------------------------------------------------
    // Tests: missing required files
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_MissingTargetTableFile_ReportsError()
    {
        var spec = MakeValidSpec(_tempDir);
        File.Delete(Path.Combine(_tempDir, "targets.json"));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("externalTargetTablePath") && e.Contains("not found"));
    }

    [Fact]
    public void Validate_MissingRegistryFile_ReportsError()
    {
        var spec = MakeValidSpec(_tempDir);
        File.Delete(Path.Combine(_tempDir, "registry.json"));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("registryPath") && e.Contains("not found"));
    }

    [Fact]
    public void Validate_MissingEnvironmentRecordFile_ReportsError()
    {
        var spec = MakeValidSpec(_tempDir);
        File.Delete(Path.Combine(_tempDir, "env_toy.json"));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("environmentRecordPaths[0]") && e.Contains("not found"));
    }

    // -----------------------------------------------------------------------
    // Tests: reference mode — missing sidecars
    // -----------------------------------------------------------------------

    [Fact]
    public void Validate_ReferenceModeWithNullSidecars_ReportsAllFour()
    {
        // Create a second env record so the env count check passes
        File.WriteAllText(Path.Combine(_tempDir, "env_structured.json"), "{}");
        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.1.0",
            extraEnvPaths: ["env_structured.json"]);

        // Sidecars are all null (default)
        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: true);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("observationChainPath") && e.Contains("required in reference mode"));
        Assert.Contains(result.Errors, e => e.Contains("environmentVariancePath") && e.Contains("required in reference mode"));
        Assert.Contains(result.Errors, e => e.Contains("representationContentPath") && e.Contains("required in reference mode"));
        Assert.Contains(result.Errors, e => e.Contains("couplingConsistencyPath") && e.Contains("required in reference mode"));
    }

    [Fact]
    public void Validate_ReferenceModeOnlyOneEnvRecord_ReportsError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "obs_chain.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "env_variance.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "rep_content.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "coupling_consistency.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "sidecar_summary.json"), GuJsonDefaults.Serialize(new SidecarSummary
        {
            StudyId = "validator-sidecars",
            Channels =
            [
                new SidecarChannelStatus { ChannelId = "observation-chain", Status = "skipped", InputCount = 0, OutputCount = 0 },
                new SidecarChannelStatus { ChannelId = "environment-variance", Status = "skipped", InputCount = 0, OutputCount = 0 },
                new SidecarChannelStatus { ChannelId = "representation-content", Status = "skipped", InputCount = 0, OutputCount = 0 },
                new SidecarChannelStatus { ChannelId = "coupling-consistency", Status = "skipped", InputCount = 0, OutputCount = 0 },
            ],
            Provenance = MakeProvenance(),
        }));

        // Only 1 env record (no extraEnvPaths)
        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.1.0");

        var specWithSidecars = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = spec.ExternalTargetTablePath,
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
            BranchQuantityValuesPath = spec.BranchQuantityValuesPath,
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = spec.ObservablesPath,
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = spec.RegistryPath,
            ObservationChainPath = "obs_chain.json",
            EnvironmentVariancePath = "env_variance.json",
            RepresentationContentPath = "rep_content.json",
            CouplingConsistencyPath = "coupling_consistency.json",
        };

        var result = Phase5CampaignSpecValidator.Validate(specWithSidecars, _tempDir, requireReferenceSidecars: true);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("structured environment"));
    }

    [Fact]
    public void Validate_ReferenceModeOldSchemaVersion_ReportsError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "env_structured.json"), "{}");
        File.WriteAllText(Path.Combine(_tempDir, "obs_chain.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "env_variance.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "rep_content.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "coupling_consistency.json"), "[]");

        // Schema version is 1.0.0, not 1.1.0
        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.0.0",
            extraEnvPaths: ["env_structured.json"]);

        var specWithSidecars = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = spec.ExternalTargetTablePath,
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
            BranchQuantityValuesPath = spec.BranchQuantityValuesPath,
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = spec.ObservablesPath,
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = spec.RegistryPath,
            ObservationChainPath = "obs_chain.json",
            EnvironmentVariancePath = "env_variance.json",
            RepresentationContentPath = "rep_content.json",
            CouplingConsistencyPath = "coupling_consistency.json",
        };

        var result = Phase5CampaignSpecValidator.Validate(specWithSidecars, _tempDir, requireReferenceSidecars: true);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("schemaVersion") && e.Contains("1.1.0"));
    }

    [Fact]
    public void Validate_ReferenceModeWithMissingSidecarFile_ReportsError()
    {
        File.WriteAllText(Path.Combine(_tempDir, "env_structured.json"), "{}");
        // obs_chain.json intentionally NOT created
        File.WriteAllText(Path.Combine(_tempDir, "env_variance.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "rep_content.json"), "[]");
        File.WriteAllText(Path.Combine(_tempDir, "coupling_consistency.json"), "[]");

        var spec = MakeValidSpec(_tempDir, schemaVersion: "1.1.0",
            extraEnvPaths: ["env_structured.json"]);

        var specWithSidecars = new Phase5CampaignSpec
        {
            CampaignId = spec.CampaignId,
            SchemaVersion = spec.SchemaVersion,
            BranchFamilySpec = spec.BranchFamilySpec,
            RefinementSpec = spec.RefinementSpec,
            EnvironmentCampaignSpec = spec.EnvironmentCampaignSpec,
            ExternalTargetTablePath = spec.ExternalTargetTablePath,
            CalibrationPolicy = spec.CalibrationPolicy,
            FalsificationPolicy = spec.FalsificationPolicy,
            Provenance = spec.Provenance,
            BranchQuantityValuesPath = spec.BranchQuantityValuesPath,
            RefinementValuesPath = spec.RefinementValuesPath,
            ObservablesPath = spec.ObservablesPath,
            EnvironmentRecordPaths = spec.EnvironmentRecordPaths,
            RegistryPath = spec.RegistryPath,
            ObservationChainPath = "obs_chain.json",      // file does not exist
            EnvironmentVariancePath = "env_variance.json",
            RepresentationContentPath = "rep_content.json",
            CouplingConsistencyPath = "coupling_consistency.json",
        };

        var result = Phase5CampaignSpecValidator.Validate(specWithSidecars, _tempDir, requireReferenceSidecars: true);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("observationChainPath") && e.Contains("not found"));
    }
}
