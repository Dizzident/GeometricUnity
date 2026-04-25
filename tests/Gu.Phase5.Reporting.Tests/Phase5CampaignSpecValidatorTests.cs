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
    private Phase5CampaignSpec MakeValidSpec(
        string dir,
        string schemaVersion = "1.0.0",
        IReadOnlyList<string>? extraEnvPaths = null,
        string? targetCoverageBlockersPath = null,
        string? physicalObservableMappingsPath = null,
        string? observableClassificationsPath = null,
        string? physicalCalibrationPath = null)
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
            TargetCoverageBlockersPath = targetCoverageBlockersPath,
            PhysicalObservableMappingsPath = physicalObservableMappingsPath,
            ObservableClassificationsPath = observableClassificationsPath,
            PhysicalCalibrationPath = physicalCalibrationPath,
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

    [Fact]
    public void Validate_DuplicateObservableEnvironmentsWithoutTargetSelector_ReportsError()
    {
        var spec = MakeValidSpec(_tempDir);

        File.WriteAllText(
            Path.Combine(_tempDir, "observables.json"),
            GuJsonDefaults.Serialize(new[]
            {
                new QuantitativeObservableRecord
                {
                    ObservableId = "q1",
                    Value = 1.0,
                    Uncertainty = new QuantitativeUncertainty { TotalUncertainty = 0.03 },
                    BranchId = "V1",
                    EnvironmentId = "env-toy",
                    ExtractionMethod = "validator",
                    Provenance = MakeProvenance(),
                },
                new QuantitativeObservableRecord
                {
                    ObservableId = "q1",
                    Value = 1.1,
                    Uncertainty = new QuantitativeUncertainty { TotalUncertainty = 0.02 },
                    BranchId = "V1",
                    EnvironmentId = "env-structured",
                    ExtractionMethod = "validator",
                    Provenance = MakeProvenance(),
                },
            }));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("targetEnvironmentId or targetEnvironmentTier"));
    }

    [Fact]
    public void Validate_TargetWithNoComputedObservable_ReportsCoverageError()
    {
        var spec = MakeValidSpec(_tempDir);

        File.WriteAllText(
            Path.Combine(_tempDir, "targets.json"),
            GuJsonDefaults.Serialize(new ExternalTargetTable
            {
                TableId = "validator-targets",
                Targets =
                [
                    new ExternalTarget
                    {
                        Label = "missing-q",
                        ObservableId = "missing-q",
                        Value = 1.0,
                        Uncertainty = 0.1,
                        Source = "derived-synthetic-v1",
                        EvidenceTier = "derived-synthetic",
                        DistributionModel = "gaussian",
                    },
                ],
            }));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("missing-q") && e.Contains("no matching computed observable"));
    }

    [Fact]
    public void Validate_TargetWithNoComputedObservableAndBlocker_ReturnsValid()
    {
        var spec = MakeValidSpec(_tempDir, targetCoverageBlockersPath: "target_coverage_blockers.json");

        File.WriteAllText(
            Path.Combine(_tempDir, "targets.json"),
            GuJsonDefaults.Serialize(new ExternalTargetTable
            {
                TableId = "validator-targets",
                Targets =
                [
                    new ExternalTarget
                    {
                        Label = "missing-q",
                        ObservableId = "missing-q",
                        Value = 1.0,
                        Uncertainty = 0.1,
                        Source = "derived-synthetic-v1",
                        EvidenceTier = "derived-synthetic",
                        DistributionModel = "gaussian",
                    },
                ],
            }));

        File.WriteAllText(
            Path.Combine(_tempDir, "target_coverage_blockers.json"),
            GuJsonDefaults.Serialize(new TargetCoverageBlockerTable
            {
                TableId = "blockers",
                Blockers =
                [
                    new TargetCoverageBlockerRecord
                    {
                        BlockerId = "block-missing-q",
                        ObservableId = "missing-q",
                        TargetLabel = "missing-q",
                        BlockerReason = "Observable generation has not been implemented for this target.",
                        ClosureRequirement = "Generate the missing observable with uncertainty.",
                    },
                ],
            }));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);
        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Validate_PhysicalTargetWithoutMapping_ReportsPlainEnglishError()
    {
        var spec = MakeValidSpec(_tempDir);
        WritePhysicalTarget();

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("physical target 'w-mass'") && e.Contains("no physical observable mapping table"));
    }

    [Fact]
    public void Validate_PhysicalTargetWithBlockedMapping_ReportsClosureRequirement()
    {
        var spec = MakeValidSpec(_tempDir, physicalObservableMappingsPath: "physical_mappings.json");
        WritePhysicalTarget();
        WriteMappingTable(status: "blocked");

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("blocked by mapping 'map-w-mass'") && e.Contains("Calibrate the vector mass."));
    }

    [Fact]
    public void Validate_NonPhysicalBenchmarkTargets_DoNotRequirePhysicalMappings()
    {
        var spec = MakeValidSpec(_tempDir);

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Validate_PhysicalTargetWithValidatedMapping_ReturnsValid()
    {
        var spec = MakeValidSpec(
            _tempDir,
            physicalObservableMappingsPath: "physical_mappings.json",
            physicalCalibrationPath: "physical_calibrations.json");
        WritePhysicalTarget();
        WriteMappingTable(status: "validated");
        WriteCalibrationTable(status: "validated");

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);

        Assert.True(result.IsValid, string.Join("; ", result.Errors));
    }

    [Fact]
    public void Validate_PhysicalTargetWithValidatedMappingButNoCalibration_ReportsError()
    {
        var spec = MakeValidSpec(_tempDir, physicalObservableMappingsPath: "physical_mappings.json");
        WritePhysicalTarget();
        WriteMappingTable(status: "validated");

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("no physical calibration table") && e.Contains("map-w-mass"));
    }

    [Fact]
    public void Validate_PhysicalTargetWithBlockedCalibration_ReportsClosureRequirement()
    {
        var spec = MakeValidSpec(
            _tempDir,
            physicalObservableMappingsPath: "physical_mappings.json",
            physicalCalibrationPath: "physical_calibrations.json");
        WritePhysicalTarget();
        WriteMappingTable(status: "validated");
        WriteCalibrationTable(status: "blocked");

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("blocked by calibration 'cal-w-mass'") && e.Contains("Set the GeV scale."));
    }

    [Fact]
    public void Validate_PhysicalTargetWithoutCitationMetadata_ReportsEvidenceErrors()
    {
        var spec = MakeValidSpec(_tempDir, physicalObservableMappingsPath: "physical_mappings.json");
        WritePhysicalTargetWithoutCitationMetadata();
        WriteMappingTable(status: "validated");

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("physical target 'w-mass'") && e.Contains("citation"));
        Assert.Contains(result.Errors, e => e.Contains("physical target 'w-mass'") && e.Contains("sourceUrl"));
        Assert.Contains(result.Errors, e => e.Contains("physical target 'w-mass'") && e.Contains("retrievedAt"));
        Assert.Contains(result.Errors, e => e.Contains("physical target 'w-mass'") && e.Contains("unit."));
    }

    [Fact]
    public void Validate_ObservableClassificationPathRequiresEveryObservable()
    {
        var spec = MakeValidSpec(_tempDir, observableClassificationsPath: "observable_classifications.json");
        File.WriteAllText(
            Path.Combine(_tempDir, "observable_classifications.json"),
            GuJsonDefaults.Serialize(new ObservableClassificationTable
            {
                TableId = "classifications",
                Classifications = [],
            }));

        var result = Phase5CampaignSpecValidator.Validate(spec, _tempDir, requireReferenceSidecars: false);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("observable 'q1' has no explicit classification"));
    }

    private void WritePhysicalTarget()
    {
        File.WriteAllText(
            Path.Combine(_tempDir, "targets.json"),
            GuJsonDefaults.Serialize(new ExternalTargetTable
            {
                TableId = "validator-targets",
                Targets =
                [
                    new ExternalTarget
                    {
                        Label = "w-mass",
                        ObservableId = "q1",
                        Value = 80.0,
                        Uncertainty = 0.1,
                        Source = "physical-target-test",
                        EvidenceTier = "physical-prediction",
                        BenchmarkClass = "physical-observable",
                        ParticleId = "w-boson",
                        PhysicalObservableType = "mass",
                        UnitFamily = "mass-energy",
                        Unit = "GeV",
                        Citation = "Unit test physical target citation.",
                        SourceUrl = "https://example.test/w-mass",
                        RetrievedAt = "2026-04-25",
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
    }

    private void WritePhysicalTargetWithoutCitationMetadata()
    {
        File.WriteAllText(
            Path.Combine(_tempDir, "targets.json"),
            GuJsonDefaults.Serialize(new ExternalTargetTable
            {
                TableId = "validator-targets",
                Targets =
                [
                    new ExternalTarget
                    {
                        Label = "w-mass",
                        ObservableId = "q1",
                        Value = 80.0,
                        Uncertainty = 0.1,
                        Source = "physical-target-test",
                        EvidenceTier = "physical-prediction",
                        BenchmarkClass = "physical-observable",
                        ParticleId = "w-boson",
                        PhysicalObservableType = "mass",
                        UnitFamily = "mass-energy",
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
    }

    private void WriteMappingTable(string status)
    {
        File.WriteAllText(
            Path.Combine(_tempDir, "physical_mappings.json"),
            GuJsonDefaults.Serialize(new PhysicalObservableMappingTable
            {
                TableId = "physical-mappings",
                Mappings =
                [
                    new PhysicalObservableMapping
                    {
                        MappingId = "map-w-mass",
                        ParticleId = "w-boson",
                        PhysicalObservableType = "mass",
                        SourceComputedObservableId = "q1",
                        TargetPhysicalObservableId = "q1",
                        UnitFamily = "mass-energy",
                        Status = status,
                        Assumptions = ["test assumption"],
                        ClosureRequirements = ["Calibrate the vector mass."],
                    },
                ],
            }));
    }

    private void WriteCalibrationTable(string status)
    {
        File.WriteAllText(
            Path.Combine(_tempDir, "physical_calibrations.json"),
            GuJsonDefaults.Serialize(new PhysicalCalibrationTable
            {
                TableId = "physical-calibrations",
                Calibrations =
                [
                    new PhysicalCalibrationRecord
                    {
                        CalibrationId = "cal-w-mass",
                        MappingId = "map-w-mass",
                        SourceComputedObservableId = "q1",
                        SourceUnitFamily = "dimensionless",
                        TargetUnitFamily = "mass-energy",
                        TargetUnit = "GeV",
                        ScaleFactor = 80.0,
                        ScaleUncertainty = 0.1,
                        Status = status,
                        Method = "test-scale-setting",
                        Source = "test",
                        Assumptions = ["test calibration assumption"],
                        ClosureRequirements = ["Set the GeV scale."],
                    },
                ],
            }));
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
